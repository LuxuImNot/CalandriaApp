using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Compras · catálogos de material (tablas de explosión por
    /// prototipo: COMPRASCALANDRA / COMPRASTUNERA) y catálogo de compras indirectas.
    /// Reproduce la lógica de FormCompraMulti y FormCompraIndirecta.
    ///
    /// Centralización: el catálogo canónico es el de Destajos (InsumosTuneraEXP /
    /// InsumosCalandraEXP). COMPRASCALANDRA/COMPRASTUNERA son ahora VISTAS sobre
    /// esas tablas (ver SQL_SCRIPTS/Centralizacion_03), por eso Catalogo/Upsert/
    /// Eliminar siguen funcionando sin cambios. Los insumos indirectos viven en
    /// Insumos*EXP con Tipo='Indirecto' (ya no en una tabla COMPRASINDIRECTAS).
    ///
    /// El nombre de la tabla se interpola en el SQL, por eso el prototipo se mapea
    /// SIEMPRE a una tabla en lista blanca (igual que GetExplosionTableForPrototipo
    /// del cliente); nunca se usa texto libre.
    /// </summary>
    [RoutePrefix("api/compras"), RequierePermiso("compras.ver")]
    public class ComprasController : ApiController
    {
        private const string TablaCalandra = "COMPRASCALANDRA";
        private const string TablaTunera = "COMPRASTUNERA";

        // Catálogo canónico (tablas EXP de Destajos). Lista blanca; nunca texto libre.
        private const string CatalogoCalandra = "InsumosCalandraEXP";
        private const string CatalogoTunera = "InsumosTuneraEXP";

        // Árbol de destajos (rutas). Lista blanca; el nombre se interpola en el SQL.
        private const string RutaCalandra = "RutaCalandraDestajo";
        private const string RutaTunera = "RutaTuneraDestajo";

        private static string RutaDePrototipo(string prototipo)
        {
            if (string.IsNullOrWhiteSpace(prototipo))
                return RutaCalandra;
            return prototipo.ToUpperInvariant().Contains("TUNERA") ? RutaTunera : RutaCalandra;
        }

        private static string TablaDePrototipo(string prototipo)
        {
            if (string.IsNullOrWhiteSpace(prototipo))
                return TablaCalandra;
            var p = prototipo.ToUpperInvariant();
            if (p.Contains("TUNERA")) return TablaTunera;
            return TablaCalandra; // CALANDRA/CALANDRIA y fallback seguro
        }

        /// <summary>GET /api/compras/prototipo?manzana=&amp;lote= → prototipo de InventarioCasas.</summary>
        [HttpGet, Route("prototipo")]
        public IHttpActionResult Prototipo(string manzana, string lote)
        {
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana ?? "");
                cmd.Parameters.AddWithValue("@l", lote ?? "");
                var result = cmd.ExecuteScalar();
                return Ok(result?.ToString() ?? "");
            }
        }

        /// <summary>
        /// GET /api/compras/catalogo?prototipo=X → filas del catálogo de explosión.
        /// El precio se resuelve probando las posibles columnas de costo de la tabla
        /// (Costo / CostoUnitario / Precio / costo / precio), como hacía el cliente.
        /// </summary>
        [HttpGet, Route("catalogo")]
        public IHttpActionResult Catalogo(string prototipo)
        {
            string tabla = TablaDePrototipo(prototipo);
            var lista = new List<CatalogoMaterialDto>();

            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand($"SELECT * FROM [{tabla}]", conn))
            using (var reader = cmd.ExecuteReader())
            {
                var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                    columnas.Add(reader.GetName(i));

                string colPrecio = PrimeraColumna(columnas,
                    "Costo", "CostoUnitario", "Precio", "costo", "precio");
                string colCantidad = columnas.Contains("Cantidad") ? "Cantidad" : null;

                while (reader.Read())
                {
                    lista.Add(new CatalogoMaterialDto
                    {
                        Clave = Leer(reader, columnas, "Clave"),
                        Descripcion = Leer(reader, columnas, "Descripcion"),
                        Unidad = Leer(reader, columnas, "Unidad"),
                        Familia = Leer(reader, columnas, "Familia"),
                        Cantidad = colCantidad == null ? 0m : ParseDecimal(reader[colCantidad]),
                        Precio = colPrecio == null ? 0m : ParseDecimal(reader[colPrecio])
                    });
                }
            }
            return Ok(lista);
        }

        /// <summary>
        /// GET /api/compras/catalogo-destajos?prototipo=X&amp;manzana=M&amp;lote=L → insumos
        /// Material del activador de destajos para comprar.
        ///
        /// Ya no restringe por destajo activado de cada casa: en cuanto se activa el
        /// primer destajo en CUALQUIER casa (ActivacionTareasRuta.DesatajoActivado=1),
        /// se habilita el catálogo COMPLETO del prototipo para comprar. Si todavía no
        /// se ha activado ningún destajo en ninguna casa, no hay nada que comprar.
        /// Lee el pivote _Columnas (Cantidad/Unidad/Precio/Clave/Familia), agrupa por
        /// nombre y resuelve la clave: (1) la del pivote; (2) catálogo Insumos*EXP por
        /// nombre normalizado; (3) sin resolver. ClaveResuelta marca si quedó con clave
        /// de almacén (los sin clave se rastrean por nombre).
        /// </summary>
        [HttpGet, Route("catalogo-destajos")]
        public IHttpActionResult CatalogoDestajos(string prototipo, string manzana = null, string lote = null)
        {
            string tablaRuta = RutaDePrototipo(prototipo);
            string tablaCatalogo = tablaRuta == RutaTunera ? CatalogoTunera : CatalogoCalandra;

            // Agrupado por nombre normalizado (o por clave del árbol si no hay nombre).
            var porInsumo = new Dictionary<string, AggInsumo>(StringComparer.Ordinal);

            using (var conn = Db.Abrir())
            {
                // Gate global: ¿ya se activó el destajo 1 de alguna casa? Si no, catálogo vacío.
                using (var cmdGate = new SqlCommand(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM ActivacionTareasRuta WHERE DesatajoActivado = 1) THEN 1 ELSE 0 END", conn))
                {
                    if ((int)cmdGate.ExecuteScalar() == 0)
                        return Ok(new List<CatalogoMaterialDto>());
                }

                // 1) Mapa nombre normalizado -> clave del catálogo maestro.
                var nombreAClave = CargarClavesPorNombre(conn, tablaCatalogo);

                string sql = $@"
                    SELECT
                        r.Nombre AS Nombre,
                        ISNULL(MAX(CASE WHEN c.NombreColumna = 'Clave'    THEN c.Valor END), '') AS Clave,
                        ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad'   THEN c.Valor END), '') AS Unidad,
                        ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad,
                        ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS Precio,
                        ISNULL(MAX(CASE WHEN c.NombreColumna = 'Familia'  THEN c.Valor END), '') AS Familia
                    FROM [{tablaRuta}] r
                    LEFT JOIN [{tablaRuta}_Columnas] c ON c.NodoID = r.ID
                    WHERE r.TipoTarea = 1
                    GROUP BY r.ID, r.Nombre";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string nombre = (reader["Nombre"]?.ToString() ?? "").Trim();
                        string claveArbol = (reader["Clave"]?.ToString() ?? "").Trim();
                        if (nombre.Length == 0 && claveArbol.Length == 0) continue;

                        decimal cantidad = ParseDecimal(reader["Cantidad"]);
                        decimal precio = ParseDecimal(reader["Precio"]);
                        string unidad = (reader["Unidad"]?.ToString() ?? "").Trim();
                        string familia = (reader["Familia"]?.ToString() ?? "").Trim();

                        string norm = NormNombre(nombre);
                        string key = norm.Length > 0 ? "N:" + norm : "C:" + claveArbol.ToUpperInvariant();

                        if (!porInsumo.TryGetValue(key, out var ag))
                        {
                            // Resolver clave: (1) del árbol; (2) catálogo por nombre.
                            string clave = claveArbol;
                            if (clave.Length == 0 && norm.Length > 0)
                                nombreAClave.TryGetValue(norm, out clave);

                            ag = new AggInsumo
                            {
                                Clave = clave ?? "",
                                Descripcion = nombre,
                                Unidad = unidad,
                                Familia = string.IsNullOrEmpty(familia) ? "DESTAJO" : familia,
                                Precio = precio
                            };
                            porInsumo[key] = ag;
                        }

                        ag.Cantidad += cantidad;
                        if (ag.Precio == 0m && precio > 0m) ag.Precio = precio;
                        if (string.IsNullOrEmpty(ag.Unidad) && unidad.Length > 0) ag.Unidad = unidad;
                    }
                    }
                }
            }

            var lista = new List<CatalogoMaterialDto>();
            foreach (var ag in porInsumo.Values)
            {
                lista.Add(new CatalogoMaterialDto
                {
                    Clave = ag.Clave,
                    Descripcion = ag.Descripcion,
                    Unidad = ag.Unidad,
                    Cantidad = ag.Cantidad,
                    Familia = ag.Familia,
                    Precio = ag.Precio,
                    ClaveResuelta = !string.IsNullOrWhiteSpace(ag.Clave)
                });
            }
            return Ok(lista);
        }

        /// <summary>
        /// GET /api/compras/pendientes?manzana=&amp;lote= → cantidad pendiente de surtir
        /// por insumo (detalle PENDIENTE menos lo ya entrado en almacén).
        /// </summary>
        [HttpGet, Route("pendientes")]
        public IHttpActionResult Pendientes(string manzana, string lote)
        {
            var lista = new List<PendienteMaterialDto>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(@"
SELECT d.Clave, SUM(d.Cantidad) - ISNULL((
    SELECT SUM(e.Cantidad)
    FROM EntradasAlmacen e
    WHERE e.FolioOC = d.FolioOC AND e.Clave = d.Clave
), 0) AS CantidadPendiente
FROM OrdenesCompraDetalle d
INNER JOIN OrdenesCompra_Casas c ON d.FolioOC = c.FolioOC
WHERE c.Manzana = @m AND c.Lote = @l AND d.Estado = 'PENDIENTE'
GROUP BY d.FolioOC, d.Clave, d.Cantidad", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana ?? "");
                cmd.Parameters.AddWithValue("@l", lote ?? "");
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new PendienteMaterialDto
                        {
                            Clave = reader["Clave"]?.ToString() ?? "",
                            CantidadPendiente = reader["CantidadPendiente"] == DBNull.Value
                                ? 0m : Convert.ToDecimal(reader["CantidadPendiente"])
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>
        /// POST /api/compras/catalogo · alta/edición de una fila del catálogo de un
        /// prototipo (check + UPDATE/INSERT) y, si llega Precio, actualiza Costo.
        /// </summary>
        [HttpPost, Route("catalogo"), RequierePermiso("compras.editar")]
        public IHttpActionResult UpsertCatalogo([FromBody] UpsertCatalogoRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Clave))
                return BadRequest("Falta la clave del insumo.");

            string tabla = TablaDePrototipo(req.Prototipo);
            bool costoAplicado = false;

            using (var conn = Db.Abrir())
            {
                bool existe;
                using (var cmdCheck = new SqlCommand($"SELECT COUNT(*) FROM [{tabla}] WHERE Clave = @clave", conn))
                {
                    cmdCheck.Parameters.AddWithValue("@clave", req.Clave);
                    existe = (int)cmdCheck.ExecuteScalar() > 0;
                }

                if (existe)
                {
                    if (req.ActualizarCampos)
                    {
                        using (var cmd = new SqlCommand(
                            $"UPDATE [{tabla}] SET Descripcion = @desc, Unidad = @unidad, Cantidad = @cantidad, Familia = @familia WHERE Clave = @clave", conn))
                        {
                            cmd.Parameters.AddWithValue("@desc", (object)req.Descripcion ?? "");
                            cmd.Parameters.AddWithValue("@unidad", (object)req.Unidad ?? "");
                            cmd.Parameters.AddWithValue("@cantidad", req.Cantidad);
                            cmd.Parameters.AddWithValue("@familia", (object)req.Familia ?? "MANUAL");
                            cmd.Parameters.AddWithValue("@clave", req.Clave);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (var cmd = new SqlCommand(
                        $"INSERT INTO [{tabla}] (Clave, Descripcion, Unidad, Cantidad, Familia) VALUES (@clave, @desc, @unidad, @cantidad, @familia)", conn))
                    {
                        cmd.Parameters.AddWithValue("@clave", req.Clave);
                        cmd.Parameters.AddWithValue("@desc", (object)req.Descripcion ?? "");
                        cmd.Parameters.AddWithValue("@unidad", (object)req.Unidad ?? "");
                        cmd.Parameters.AddWithValue("@cantidad", req.Cantidad);
                        cmd.Parameters.AddWithValue("@familia", (object)req.Familia ?? "MANUAL");
                        cmd.ExecuteNonQuery();
                    }
                }

                if (req.Precio.HasValue)
                {
                    try
                    {
                        using (var cmd = new SqlCommand($"UPDATE [{tabla}] SET Costo = @precio WHERE Clave = @clave", conn))
                        {
                            cmd.Parameters.AddWithValue("@precio", req.Precio.Value);
                            cmd.Parameters.AddWithValue("@clave", req.Clave);
                            cmd.ExecuteNonQuery();
                        }
                        costoAplicado = true;
                    }
                    catch (SqlException) { /* la tabla no tiene columna Costo */ }
                }
            }

            return Ok(new UpsertCatalogoResponse { CostoAplicado = costoAplicado });
        }

        /// <summary>POST /api/compras/catalogo/eliminar → filas eliminadas.</summary>
        [HttpPost, Route("catalogo/eliminar"), RequierePermiso("compras.editar")]
        public IHttpActionResult EliminarCatalogo([FromBody] EliminarCatalogoRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Clave))
                return BadRequest("Falta la clave del insumo.");

            string tabla = TablaDePrototipo(req.Prototipo);
            int filas;
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand($"DELETE FROM [{tabla}] WHERE Clave = @clave", conn))
            {
                cmd.Parameters.AddWithValue("@clave", req.Clave);
                filas = cmd.ExecuteNonQuery();
            }
            return Ok(filas);
        }

        // ---- Insumos indirectos (Insumos*EXP con Tipo='Indirecto') ----

        /// <summary>
        /// GET /api/compras/indirectas → catálogo de compras indirectas.
        /// Lee Insumos*EXP (ambos prototipos) donde Tipo='Indirecto', deduplicado por Clave.
        /// </summary>
        [HttpGet, Route("indirectas")]
        public IHttpActionResult Indirectas()
        {
            var lista = new List<InsumoIndirectoDto>();
            using (var conn = Db.Abrir())
            {
                EnsureCatalogoExp(conn);
                using (var cmd = new SqlCommand(
                    "SELECT Clave, MAX(Descripcion) AS Descripcion, MAX(Unidad) AS Unidad FROM (" +
                    " SELECT Clave, [Descripción] AS Descripcion, Unidad FROM dbo.InsumosTuneraEXP   WHERE Tipo = N'Indirecto'" +
                    " UNION ALL" +
                    " SELECT Clave, [Descripción] AS Descripcion, Unidad FROM dbo.InsumosCalandraEXP WHERE Tipo = N'Indirecto'" +
                    ") x GROUP BY Clave ORDER BY Clave", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new InsumoIndirectoDto
                        {
                            Clave = reader["Clave"]?.ToString() ?? "",
                            Descripcion = reader["Descripcion"]?.ToString() ?? "",
                            Unidad = reader["Unidad"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>
        /// POST /api/compras/indirectas · alta de un insumo indirecto (409 si ya existe
        /// como indirecto). Se refleja en ambos catálogos EXP con Tipo='Indirecto'; solo
        /// se inserta donde la clave no exista, para no chocar con la PK.
        /// </summary>
        [HttpPost, Route("indirectas"), RequierePermiso("compras.editar")]
        public IHttpActionResult CrearIndirecta([FromBody] CrearInsumoIndirectoRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Clave))
                return BadRequest("Falta la clave del insumo.");

            using (var conn = Db.Abrir())
            {
                EnsureCatalogoExp(conn);

                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM (" +
                    " SELECT Clave FROM dbo.InsumosTuneraEXP   WHERE Clave = @c AND Tipo = N'Indirecto'" +
                    " UNION SELECT Clave FROM dbo.InsumosCalandraEXP WHERE Clave = @c AND Tipo = N'Indirecto') x", conn))
                {
                    cmd.Parameters.AddWithValue("@c", req.Clave);
                    if ((int)cmd.ExecuteScalar() > 0) return Conflict();
                }

                foreach (var tabla in new[] { CatalogoTunera, CatalogoCalandra })
                {
                    using (var cmd = new SqlCommand(
                        $"IF NOT EXISTS (SELECT 1 FROM dbo.[{tabla}] WHERE Clave = @c) " +
                        $"INSERT INTO dbo.[{tabla}] (Clave, [Descripción], Unidad, Tipo) " +
                        "VALUES (@c, @d, @u, N'Indirecto')", conn))
                    {
                        cmd.Parameters.AddWithValue("@c", req.Clave);
                        cmd.Parameters.AddWithValue("@d", (object)req.Descripcion ?? "");
                        cmd.Parameters.AddWithValue("@u", (object)req.Unidad ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return Ok();
        }

        /// <summary>
        /// Asegura el catálogo canónico (tablas EXP de destajos). Crea InsumosTuneraEXP
        /// e InsumosCalandraEXP si faltan (esquema canónico) y garantiza la columna Tipo,
        /// para que los endpoints de indirectos funcionen aunque alguna no exista todavía.
        /// </summary>
        private static void EnsureCatalogoExp(SqlConnection conn)
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.InsumosTuneraEXP', N'U') IS NULL
    CREATE TABLE dbo.InsumosTuneraEXP([Clave] NVARCHAR(50) NOT NULL,
        [Descripción] NVARCHAR(1000) NULL, [Unidad] NVARCHAR(20) NULL,
        [Cantidad] DECIMAL(18,6) NULL, [Costo] DECIMAL(18,4) NULL,
        [Importe] DECIMAL(18,4) NULL, [Porcentaje] DECIMAL(18,12) NULL,
        [Tipo] NVARCHAR(50) NULL, [Familia] NVARCHAR(80) NULL,
        CONSTRAINT [PK_InsumosTuneraEXP] PRIMARY KEY CLUSTERED ([Clave] ASC));
IF OBJECT_ID(N'dbo.InsumosCalandraEXP', N'U') IS NULL
    CREATE TABLE dbo.InsumosCalandraEXP([Clave] NVARCHAR(50) NOT NULL,
        [Descripción] NVARCHAR(1000) NULL, [Unidad] NVARCHAR(20) NULL,
        [Cantidad] DECIMAL(18,6) NULL, [Costo] DECIMAL(18,4) NULL,
        [Importe] DECIMAL(18,4) NULL, [Porcentaje] DECIMAL(18,12) NULL,
        [Tipo] NVARCHAR(50) NULL, [Familia] NVARCHAR(80) NULL,
        CONSTRAINT [PK_InsumosCalandraEXP] PRIMARY KEY CLUSTERED ([Clave] ASC));
IF COL_LENGTH('dbo.InsumosTuneraEXP', N'Tipo') IS NULL
    ALTER TABLE dbo.InsumosTuneraEXP ADD [Tipo] NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.InsumosCalandraEXP', N'Tipo') IS NULL
    ALTER TABLE dbo.InsumosCalandraEXP ADD [Tipo] NVARCHAR(50) NULL;";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        // ---- helpers ----

        /// <summary>Acumulador de un insumo del árbol agrupado por nombre.</summary>
        private sealed class AggInsumo
        {
            public string Clave;
            public string Descripcion;
            public string Unidad;
            public string Familia;
            public decimal Cantidad;
            public decimal Precio;
        }

        /// <summary>
        /// Mapa NOMBRE normalizado → Clave del catálogo maestro (InsumosTuneraEXP /
        /// InsumosCalandraEXP). Misma fuente que usa FormAlmacen_Salidas para resolver
        /// la clave de un insumo de destajo capturado sin clave.
        /// </summary>
        private static Dictionary<string, string> CargarClavesPorNombre(SqlConnection conn, string tablaCatalogo)
        {
            var mapa = new Dictionary<string, string>(StringComparer.Ordinal);
            try
            {
                using (var cmd = new SqlCommand(
                    $"SELECT Clave, [Descripción] AS Descripcion FROM dbo.[{tablaCatalogo}]", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string clave = (rd["Clave"]?.ToString() ?? "").Trim();
                        string nom = NormNombre(rd["Descripcion"]?.ToString());
                        if (nom.Length == 0 || clave.Length == 0) continue;
                        if (!mapa.ContainsKey(nom)) mapa[nom] = clave; // primera coincidencia
                    }
                }
            }
            catch
            {
                // Sin catálogo: los insumos sin clave quedan sin resolver (ClaveResuelta=false).
            }
            return mapa;
        }

        /// <summary>
        /// Normaliza un nombre para cotejo: sin acentos, espacios colapsados, MAYÚSCULAS.
        /// Réplica de NormNombre de FormAlmacen_Salidas para que la resolución coincida.
        /// </summary>
        private static string NormNombre(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            string t = s.Trim().ToUpperInvariant();
            var sb = new StringBuilder(t.Length);
            bool espacioPrevio = false;
            foreach (char ch in t.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                    continue; // quita el acento
                if (char.IsWhiteSpace(ch))
                {
                    if (!espacioPrevio && sb.Length > 0) sb.Append(' ');
                    espacioPrevio = true;
                }
                else { sb.Append(ch); espacioPrevio = false; }
            }
            return sb.ToString().Trim().Normalize(NormalizationForm.FormC);
        }

        private static string PrimeraColumna(HashSet<string> columnas, params string[] candidatas)
        {
            foreach (var c in candidatas)
                if (columnas.Contains(c)) return c;
            return null;
        }

        private static string Leer(SqlDataReader reader, HashSet<string> columnas, string col)
        {
            if (!columnas.Contains(col)) return "";
            var v = reader[col];
            return v == DBNull.Value ? "" : v.ToString();
        }

        private static decimal ParseDecimal(object valor)
        {
            if (valor == null || valor == DBNull.Value) return 0m;
            string s = valor.ToString();
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal r)) return r;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal r2)) return r2;
            return 0m;
        }
    }
}
