using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Http;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Compras · catálogos de material (tablas de explosión por
    /// prototipo: COMPRASCALANDRA / COMPRASTUNERA) y catálogo de compras indirectas
    /// (COMPRASINDIRECTAS). Reproduce la lógica de FormCompraMulti y FormCompraIndirecta.
    ///
    /// El nombre de la tabla de explosión se interpola en el SQL, por eso el
    /// prototipo se mapea SIEMPRE a una de dos tablas en lista blanca (igual que
    /// GetExplosionTableForPrototipo del cliente); nunca se usa texto libre.
    /// </summary>
    [RoutePrefix("api/compras")]
    public class ComprasController : ApiController
    {
        private const string TablaCalandra = "COMPRASCALANDRA";
        private const string TablaTunera = "COMPRASTUNERA";

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
        [HttpPost, Route("catalogo")]
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
        [HttpPost, Route("catalogo/eliminar")]
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

        // ---- COMPRASINDIRECTAS ----

        /// <summary>GET /api/compras/indirectas → catálogo de compras indirectas.</summary>
        [HttpGet, Route("indirectas")]
        public IHttpActionResult Indirectas()
        {
            var lista = new List<InsumoIndirectoDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaIndirectas(conn);
                using (var cmd = new SqlCommand(
                    "SELECT Clave, Descripcion, Unidad FROM dbo.COMPRASINDIRECTAS ORDER BY Clave", conn))
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

        /// <summary>POST /api/compras/indirectas · alta (409 si clave duplicada).</summary>
        [HttpPost, Route("indirectas")]
        public IHttpActionResult CrearIndirecta([FromBody] CrearInsumoIndirectoRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Clave))
                return BadRequest("Falta la clave del insumo.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaIndirectas(conn);
                try
                {
                    using (var cmd = new SqlCommand(
                        "INSERT INTO COMPRASINDIRECTAS (Clave, Descripcion, Unidad) VALUES (@c, @d, @u)", conn))
                    {
                        cmd.Parameters.AddWithValue("@c", req.Clave);
                        cmd.Parameters.AddWithValue("@d", (object)req.Descripcion ?? "");
                        cmd.Parameters.AddWithValue("@u", (object)req.Unidad ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    return Conflict();
                }
            }
            return Ok();
        }

        private static void EnsureTablaIndirectas(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.COMPRASINDIRECTAS') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.COMPRASINDIRECTAS (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Clave NVARCHAR(100) NOT NULL UNIQUE,
        Descripcion NVARCHAR(500) NULL,
        Unidad NVARCHAR(50) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
    );
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        // ---- helpers ----

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
