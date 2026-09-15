using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Almacén: Consulta por casa (original), Historial, Inventario,
    /// Entradas y Salidas. Reproducen exactamente las consultas y reglas de
    /// FormAlmacen_ConsultaCasa / _Historial / _Inventario / _Entradas / _Salidas
    /// (ver plan de migración a web). El surtido queda desacoplado de la
    /// activación de destajos: activar NO libera insumos, eso sólo ocurre aquí.
    /// </summary>
    [RoutePrefix("api/almacen"), RequierePermiso("almacen.ver")]
    public class AlmacenController : ApiController
    {
        private const string TablaRutaTunera = "RutaTuneraDestajo";
        private const string TablaRutaCalandra = "RutaCalandraDestajo";

        private const string RutaTodas = "Todas";
        private const string RutaTunera = "Tunera";
        private const string RutaCalandra = "Calandra";

        /// <summary>GET /api/almacen/manzanas</summary>
        [HttpGet, Route("manzanas")]
        public IHttpActionResult Manzanas()
        {
            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                @"SELECT DISTINCT Manzana
                  FROM ActivacionTareasRuta
                  WHERE Manzana IS NOT NULL AND Manzana <> ''
                  ORDER BY Manzana", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    lista.Add(reader["Manzana"].ToString());
            }
            return Ok(lista);
        }

        /// <summary>GET /api/almacen/lotes?manzana=X</summary>
        [HttpGet, Route("lotes")]
        public IHttpActionResult Lotes(string manzana)
        {
            if (string.IsNullOrWhiteSpace(manzana))
                return BadRequest("Falta el parámetro 'manzana'.");

            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                @"SELECT DISTINCT Lote
                  FROM ActivacionTareasRuta
                  WHERE Manzana = @manzana
                    AND Lote IS NOT NULL AND Lote <> ''
                  ORDER BY Lote", conn))
            {
                cmd.Parameters.AddWithValue("@manzana", manzana);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(reader["Lote"].ToString());
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/almacen/materiales?manzana=X&amp;lote=Y&amp;ruta=Todas|Tunera|Calandra</summary>
        [HttpGet, Route("materiales")]
        public IHttpActionResult Materiales(string manzana, string lote, string ruta = RutaTodas)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                return BadRequest("Faltan los parámetros 'manzana' y/o 'lote'.");

            bool incluirTunera = ruta == RutaTodas || ruta == RutaTunera;
            bool incluirCalandra = ruta == RutaTodas || ruta == RutaCalandra;

            var resultado = new List<MaterialCasaDto>();
            using (var conn = Db.Abrir())
            {
                if (incluirTunera) CargarRuta(conn, resultado, TablaRutaTunera, manzana, lote);
                if (incluirCalandra) CargarRuta(conn, resultado, TablaRutaCalandra, manzana, lote);
            }
            return Ok(resultado);
        }

        private static void CargarRuta(SqlConnection conn, List<MaterialCasaDto> destino,
            string tablaRuta, string manzana, string lote)
        {
            string sql = $@"
                SELECT
                    a.Prototipo,
                    parent.Nombre        AS DestajoNombre,
                    child.Nombre         AS MaterialNombre,
                    child.Descripcion    AS MaterialDescripcion,
                    a.CuadrillaAsignada,
                    a.FechaActivacion,
                    a.FechaFinalizacion,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad'   THEN c.Valor END), '')  AS Unidad,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS PrecioUnitario
                FROM [{tablaRuta}] child
                INNER JOIN [{tablaRuta}] parent
                       ON child.ParentId = parent.ID
                INNER JOIN ActivacionTareasRuta a
                       ON a.NodoID = parent.ID
                      AND a.Ruta   = @ruta
                LEFT JOIN [{tablaRuta}_Columnas] c
                       ON c.NodoID = child.ID
                WHERE a.Manzana   = @m
                  AND a.Lote      = @l
                  AND a.Finalizado = 1
                  AND child.TipoTarea = 1   /* TipoTarea.Material */
                GROUP BY a.Prototipo, parent.Nombre, child.Nombre, child.Descripcion,
                         a.CuadrillaAsignada, a.FechaActivacion, a.FechaFinalizacion
                ORDER BY a.FechaFinalizacion DESC, parent.Nombre, child.Nombre";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.Parameters.AddWithValue("@ruta", tablaRuta);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal cantidad = ParseDecimal(reader["Cantidad"]);
                        decimal precio = ParseDecimal(reader["PrecioUnitario"]);

                        destino.Add(new MaterialCasaDto
                        {
                            Ruta = RutaCorta(tablaRuta),
                            Destajo = reader["DestajoNombre"]?.ToString() ?? string.Empty,
                            Material = reader["MaterialNombre"]?.ToString() ?? string.Empty,
                            Descripcion = reader["MaterialDescripcion"] == DBNull.Value
                                ? "" : reader["MaterialDescripcion"].ToString(),
                            Unidad = reader["Unidad"]?.ToString() ?? "",
                            Cantidad = cantidad,
                            PrecioUnitario = precio,
                            Importe = cantidad * precio,
                            Cuadrilla = reader["CuadrillaAsignada"] == DBNull.Value
                                ? "" : reader["CuadrillaAsignada"].ToString(),
                            FechaActivacion = reader["FechaActivacion"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(reader["FechaActivacion"]),
                            FechaFinalizacion = reader["FechaFinalizacion"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(reader["FechaFinalizacion"]),
                            Prototipo = reader["Prototipo"] == DBNull.Value
                                ? "" : reader["Prototipo"].ToString()
                        });
                    }
                }
            }
        }

        private static HashSet<string> LeerColumnas(SqlConnection conn, string tabla)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t", conn))
            {
                cmd.Parameters.AddWithValue("@t", tabla);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) set.Add(r.GetString(0));
                }
            }
            return set;
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

        private static string RutaCorta(string tablaRuta)
        {
            if (tablaRuta == TablaRutaTunera) return RutaTunera;
            if (tablaRuta == TablaRutaCalandra) return RutaCalandra;
            return tablaRuta;
        }

        // ==================================================================
        // Historial (FormAlmacen_Historial.cs)
        // ==================================================================

        /// <summary>GET /api/almacen/historial</summary>
        [HttpGet, Route("historial")]
        public IHttpActionResult Historial()
        {
            var lista = new List<HistorialMovimientoDto>();
            using (var conn = Db.Abrir())
            // TOP 2000: sin esto la tabla crece sin tope y cada consulta serializa el
            // historial completo. 2000 movimientos más recientes cubre por mucho el uso
            // normal de la pantalla; si hiciera falta más habría que paginar de verdad.
            using (var cmd = new SqlCommand("SELECT TOP 2000 * FROM HistorialMovimientos ORDER BY Fecha DESC", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new HistorialMovimientoDto
                    {
                        Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                        TipoMovimiento = reader["TipoMovimiento"]?.ToString() ?? "",
                        Clave = reader["Clave"]?.ToString() ?? "",
                        Descripcion = reader["Descripcion"]?.ToString() ?? "",
                        Unidad = reader["Unidad"]?.ToString() ?? "",
                        Cantidad = ParseDecimal(reader["Cantidad"]),
                        PrecioUnitario = ParseDecimal(reader["PrecioUnitario"]),
                        Usuario = reader["Usuario"]?.ToString() ?? "",
                        Manzana = reader["Manzana"]?.ToString() ?? "",
                        Lote = reader["Lote"]?.ToString() ?? "",
                        Prototipo = reader["Prototipo"]?.ToString() ?? "",
                        Justificacion = reader["Justificacion"] == DBNull.Value ? null : reader["Justificacion"].ToString()
                    });
                }
            }
            return Ok(lista);
        }

        // ==================================================================
        // Inventario — listado de EntradasAlmacen (FormAlmacen_Inventario.cs)
        // ==================================================================

        /// <summary>GET /api/almacen/inventario</summary>
        [HttpGet, Route("inventario")]
        public IHttpActionResult Inventario()
        {
            var lista = new List<InventarioAlmacenItemDto>();
            using (var conn = Db.Abrir())
            // TOP 2000: mismo motivo que en Historial() — evita serializar la tabla
            // completa en cada consulta.
            using (var cmd = new SqlCommand(@"
                SELECT TOP 2000 FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe,
                       Manzana, Lote, Prototipo, FechaEntrada, Usuario, Justificacion, EsExcepcion
                FROM EntradasAlmacen
                ORDER BY FechaEntrada DESC", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new InventarioAlmacenItemDto
                    {
                        FolioOC = reader["FolioOC"]?.ToString() ?? "",
                        Clave = reader["Clave"]?.ToString() ?? "",
                        Descripcion = reader["Descripcion"]?.ToString() ?? "",
                        Unidad = reader["Unidad"]?.ToString() ?? "",
                        Cantidad = ParseDecimal(reader["Cantidad"]),
                        PrecioUnitario = ParseDecimal(reader["PrecioUnitario"]),
                        Importe = ParseDecimal(reader["Importe"]),
                        Manzana = reader["Manzana"]?.ToString() ?? "",
                        Lote = reader["Lote"]?.ToString() ?? "",
                        Prototipo = reader["Prototipo"] == DBNull.Value ? "" : reader["Prototipo"].ToString(),
                        FechaEntrada = reader.GetDateTime(reader.GetOrdinal("FechaEntrada")),
                        Usuario = reader["Usuario"]?.ToString() ?? "",
                        Justificacion = reader["Justificacion"] == DBNull.Value ? null : reader["Justificacion"].ToString(),
                        EsExcepcion = reader["EsExcepcion"] != DBNull.Value && Convert.ToBoolean(reader["EsExcepcion"])
                    });
                }
            }
            return Ok(lista);
        }

        // ==================================================================
        // Entradas (FormAlmacen_Entradas.cs / .EntradasTarjetas.cs)
        // ==================================================================

        /// <summary>GET /api/almacen/ordenes-pendientes</summary>
        [HttpGet, Route("ordenes-pendientes")]
        public IHttpActionResult OrdenesPendientes()
        {
            var lista = new List<OrdenPendienteDto>();
            using (var conn = Db.Abrir())
            {
                // NombreOrden se agregó después; algunas BD de obra más antiguas no la tienen.
                bool tieneNombreOrden = LeerColumnas(conn, "OrdenesCompra").Contains("NombreOrden");
                string nombreOrdenSelect = tieneNombreOrden ? "o.NombreOrden" : "NULL AS NombreOrden";
                using (var cmd = new SqlCommand($@"
                SELECT DISTINCT d.FolioOC, {nombreOrdenSelect}
                FROM OrdenesCompraDetalle d
                INNER JOIN OrdenesCompra o ON d.FolioOC = o.FolioOC
                WHERE d.Estado = 'PENDIENTE'
                ORDER BY d.FolioOC DESC", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(new OrdenPendienteDto
                        {
                            Folio = reader["FolioOC"]?.ToString() ?? "",
                            NombreOrden = reader["NombreOrden"] == DBNull.Value ? null : reader["NombreOrden"].ToString()
                        });
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/almacen/oc/{folio}/detalle — renglones PENDIENTES de una orden</summary>
        [HttpGet, Route("oc/{folio}/detalle")]
        public IHttpActionResult OcDetalle(string folio)
        {
            if (string.IsNullOrWhiteSpace(folio)) return BadRequest("Falta el folio.");
            var lista = new List<OcDetallePendienteDto>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(@"
                SELECT Id, Clave, Descripcion, Unidad, Cantidad AS CantidadComprada, Estado, Justificacion
                FROM OrdenesCompraDetalle
                WHERE FolioOC = @folio AND Estado = 'PENDIENTE'", conn))
            {
                cmd.Parameters.AddWithValue("@folio", folio);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(new OcDetallePendienteDto
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Clave = reader["Clave"]?.ToString() ?? "",
                            Descripcion = reader["Descripcion"]?.ToString() ?? "",
                            Unidad = reader["Unidad"]?.ToString() ?? "",
                            CantidadComprada = ParseDecimal(reader["CantidadComprada"]),
                            Estado = reader["Estado"]?.ToString() ?? "",
                            Justificacion = reader["Justificacion"] == DBNull.Value ? null : reader["Justificacion"].ToString()
                        });
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/almacen/oc/{folio}/casas — casas asociadas a la orden (OrdenesCompra_Casas)</summary>
        [HttpGet, Route("oc/{folio}/casas")]
        public IHttpActionResult OcCasas(string folio)
        {
            if (string.IsNullOrWhiteSpace(folio)) return BadRequest("Falta el folio.");
            var lista = new List<OcCasaDto>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand("SELECT Manzana, Lote FROM OrdenesCompra_Casas WHERE FolioOC = @folio", conn))
            {
                cmd.Parameters.AddWithValue("@folio", folio);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(new OcCasaDto { Manzana = reader["Manzana"].ToString(), Lote = reader["Lote"].ToString() });
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/almacen/casa?manzana=&amp;lote= — prototipo desde InventarioCasas</summary>
        [HttpGet, Route("casa")]
        public IHttpActionResult Casa(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                return BadRequest("Faltan los parámetros 'manzana' y/o 'lote'.");

            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand("SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                var prototipo = cmd.ExecuteScalar()?.ToString();
                if (string.IsNullOrEmpty(prototipo)) return NotFound();
                return Ok(new CasaAlmacenDto { Manzana = manzana, Lote = lote, Prototipo = prototipo });
            }
        }

        /// <summary>
        /// POST /api/almacen/entrada — captura la entrada de uno o más renglones de una
        /// OC. Reproduce BtnCapturarEntrada_Click: inserta EntradasAlmacen + HistorialMovimientos
        /// por renglón y recalcula el Estado (RECIBIDO/PENDIENTE) de OrdenesCompraDetalle.
        /// Mejora de servidor: si la cantidad no coincide con la comprada, exige Justificacion
        /// (antes sólo se pedía en el diálogo del cliente).
        /// </summary>
        [HttpPost, Route("entrada"), RequierePermiso("almacen.editar")]
        public IHttpActionResult CapturarEntrada([FromBody] CapturarEntradaRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.FolioOC) || req.Lineas == null || req.Lineas.Count == 0)
                return BadRequest("Faltan datos de la entrada.");
            if (string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Falta la casa destino.");

            string usuario = User?.Identity?.Name ?? "desconocido";
            var insumosPdf = new List<InsumoMovimientoDto>();

            using (var conn = Db.Abrir())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    foreach (var linea in req.Lineas)
                    {
                        if (linea.CantidadRecibida <= 0) continue;

                        string clave, descripcion, unidad;
                        decimal cantidadComprada, precioUnitario;
                        using (var cmdInfo = new SqlCommand(
                            "SELECT Clave, Descripcion, Unidad, Cantidad, PrecioUnitario FROM OrdenesCompraDetalle WHERE Id = @id", conn, tx))
                        {
                            cmdInfo.Parameters.AddWithValue("@id", linea.IdDetalle);
                            using (var rd = cmdInfo.ExecuteReader())
                            {
                                if (!rd.Read()) continue;
                                clave = rd["Clave"]?.ToString() ?? "";
                                descripcion = rd["Descripcion"]?.ToString() ?? "";
                                unidad = rd["Unidad"]?.ToString() ?? "";
                                cantidadComprada = ParseDecimal(rd["Cantidad"]);
                                precioUnitario = ParseDecimal(rd["PrecioUnitario"]);
                            }
                        }

                        bool esExcepcion = linea.CantidadRecibida != cantidadComprada;
                        if (esExcepcion && string.IsNullOrWhiteSpace(linea.Justificacion))
                            return BadRequest($"La cantidad capturada de {clave} no coincide con la comprada: falta justificación.");

                        decimal importe = precioUnitario * linea.CantidadRecibida;

                        using (var cmd = new SqlCommand(@"
                            INSERT INTO EntradasAlmacen
                            (FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, FechaEntrada, Manzana, Lote, Prototipo, Usuario, EsExcepcion, Justificacion)
                            VALUES
                            (@FolioOC, @Clave, @Descripcion, @Unidad, @Cantidad, @Precio, @Importe, GETDATE(), @Manzana, @Lote, @Prototipo, @Usuario, @EsExcepcion, @Justificacion)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@FolioOC", req.FolioOC);
                            cmd.Parameters.AddWithValue("@Clave", clave);
                            cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                            cmd.Parameters.AddWithValue("@Unidad", unidad);
                            cmd.Parameters.AddWithValue("@Cantidad", linea.CantidadRecibida);
                            cmd.Parameters.AddWithValue("@Precio", precioUnitario);
                            cmd.Parameters.AddWithValue("@Importe", importe);
                            cmd.Parameters.AddWithValue("@Manzana", req.Manzana);
                            cmd.Parameters.AddWithValue("@Lote", req.Lote);
                            cmd.Parameters.AddWithValue("@Prototipo", (object)req.Prototipo ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Usuario", usuario);
                            cmd.Parameters.AddWithValue("@EsExcepcion", esExcepcion);
                            cmd.Parameters.AddWithValue("@Justificacion", (object)linea.Justificacion ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmdHist = new SqlCommand(@"
                            INSERT INTO HistorialMovimientos
                            (Fecha, TipoMovimiento, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, Usuario, Manzana, Lote, Prototipo, Justificacion)
                            VALUES
                            (GETDATE(), 'Entrada', @Clave, @Descripcion, @Unidad, @Cantidad, @Precio, @Importe, @Usuario, @Manzana, @Lote, @Prototipo, @Justificacion)", conn, tx))
                        {
                            cmdHist.Parameters.AddWithValue("@Clave", clave);
                            cmdHist.Parameters.AddWithValue("@Descripcion", descripcion);
                            cmdHist.Parameters.AddWithValue("@Unidad", unidad);
                            cmdHist.Parameters.AddWithValue("@Cantidad", linea.CantidadRecibida);
                            cmdHist.Parameters.AddWithValue("@Precio", precioUnitario);
                            cmdHist.Parameters.AddWithValue("@Importe", importe);
                            cmdHist.Parameters.AddWithValue("@Usuario", usuario);
                            cmdHist.Parameters.AddWithValue("@Manzana", req.Manzana);
                            cmdHist.Parameters.AddWithValue("@Lote", req.Lote);
                            cmdHist.Parameters.AddWithValue("@Prototipo", (object)req.Prototipo ?? DBNull.Value);
                            cmdHist.Parameters.AddWithValue("@Justificacion", (object)linea.Justificacion ?? DBNull.Value);
                            cmdHist.ExecuteNonQuery();
                        }

                        decimal totalRecibido;
                        using (var cmdCheck = new SqlCommand(
                            "SELECT ISNULL(SUM(Cantidad), 0) FROM EntradasAlmacen WHERE FolioOC = @f AND Clave = @c", conn, tx))
                        {
                            cmdCheck.Parameters.AddWithValue("@f", req.FolioOC);
                            cmdCheck.Parameters.AddWithValue("@c", clave);
                            totalRecibido = ParseDecimal(cmdCheck.ExecuteScalar());
                        }

                        string nuevoEstado = totalRecibido >= cantidadComprada ? "RECIBIDO" : "PENDIENTE";
                        using (var cmdUpdate = new SqlCommand(
                            "UPDATE OrdenesCompraDetalle SET Estado = @estado WHERE Id = @id", conn, tx))
                        {
                            cmdUpdate.Parameters.AddWithValue("@estado", nuevoEstado);
                            cmdUpdate.Parameters.AddWithValue("@id", linea.IdDetalle);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        insumosPdf.Add(new InsumoMovimientoDto
                        {
                            Clave = clave,
                            Descripcion = descripcion,
                            Unidad = unidad,
                            Cantidad = linea.CantidadRecibida,
                            PrecioUnitario = precioUnitario,
                            Importe = importe,
                            Justificacion = linea.Justificacion
                        });
                    }

                    if (insumosPdf.Count == 0)
                    {
                        tx.Rollback();
                        return BadRequest("No hay cantidades válidas capturadas para registrar.");
                    }

                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }

            return Ok(new ResultadoEntradaDto { Ok = true, Mensaje = "Entrada capturada correctamente.", Insumos = insumosPdf });
        }

        /// <summary>
        /// POST /api/almacen/oc/detalle/{id}/eliminar — marca un renglón de OC como ELIMINADO
        /// (mismo efecto que btnEliminardeOrden_Click / la tarjeta de Entradas).
        /// </summary>
        [HttpPost, Route("oc/detalle/{id}/eliminar"), RequierePermiso("almacen.editar")]
        public IHttpActionResult EliminarDetalle(int id, [FromBody] EliminarDetalleRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Justificacion))
                return BadRequest("La justificación es obligatoria.");

            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "UPDATE OrdenesCompraDetalle SET Estado = 'ELIMINADO', Justificacion = @j WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@j", req.Justificacion);
                cmd.Parameters.AddWithValue("@id", id);
                int filas = cmd.ExecuteNonQuery();
                if (filas == 0) return NotFound();
            }
            return Ok(new { ok = true });
        }

        // ==================================================================
        // Salidas (FormAlmacen_Salidas.cs / .ExtensionPDFSalidas.cs / SalidaAlmacenService.cs)
        // ==================================================================

        /// <summary>Acumulador por clave/nombre de los insumos Material de destajos activados.</summary>
        private class InsumoSurtirAcumulado
        {
            public string Clave;
            public string Descripcion;
            public string Unidad;
            public decimal Precio;
            public decimal Programado;
            public readonly HashSet<string> Destajos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Normaliza un nombre para cotejar insumos entre catálogo/almacén/destajos (igual que FormAlmacen_Salidas.NormNombre).</summary>
        private static string NormNombreSalida(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            string t = s.Trim().ToUpperInvariant();
            var sb = new StringBuilder(t.Length);
            bool espacioPrevio = false;
            foreach (char ch in t.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                    continue;
                if (char.IsWhiteSpace(ch))
                {
                    if (!espacioPrevio && sb.Length > 0) sb.Append(' ');
                    espacioPrevio = true;
                }
                else { sb.Append(ch); espacioPrevio = false; }
            }
            return sb.ToString().Trim().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// GET /api/almacen/salida/pendientes?manzana=&amp;lote= — insumos Material pendientes
        /// de surtir de los destajos ACTIVADOS de la casa (CargarInsumosDisponiblesSalida).
        /// El surtido está desacoplado de la activación: activar NO libera insumos.
        /// </summary>
        [HttpGet, Route("salida/pendientes")]
        public IHttpActionResult SalidaPendientes(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                return BadRequest("Faltan los parámetros 'manzana' y/o 'lote'.");

            string mz = manzana.Trim();
            string lt = lote.Trim();

            using (var conn = Db.Abrir())
            {
                string prototipo;
                using (var cmdProto = new SqlCommand("SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
                {
                    cmdProto.Parameters.AddWithValue("@m", mz);
                    cmdProto.Parameters.AddWithValue("@l", lt);
                    prototipo = cmdProto.ExecuteScalar()?.ToString();
                }
                if (string.IsNullOrEmpty(prototipo))
                    return NotFound();

                string ruta = prototipo.ToUpperInvariant().Contains("CALANDRA") ? TablaRutaCalandra : TablaRutaTunera;

                var programado = new Dictionary<string, InsumoSurtirAcumulado>(StringComparer.OrdinalIgnoreCase);

                string sqlProg = $@"
                    SELECT
                        ISNULL(MAX(CASE WHEN c.NombreColumna = 'Clave'    THEN c.Valor END), '') AS Clave,
                        r.Nombre AS Descripcion,
                        MAX(ISNULL(a.NombreTarea, '')) AS Destajo,
                        ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad'   THEN c.Valor END), '') AS Unidad,
                        ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad,
                        ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS Precio
                    FROM [{ruta}] r
                    INNER JOIN ActivacionTareasRuta a
                        ON a.NodoID = r.ParentId
                       AND a.Manzana = @m
                       AND a.Lote = @l
                       AND a.Ruta = @ruta
                       AND a.DesatajoActivado = 1
                    LEFT JOIN [{ruta}_Columnas] c ON c.NodoID = r.ID
                    WHERE r.TipoTarea = 1
                    GROUP BY r.ID, r.Nombre";

                using (var cmd = new SqlCommand(sqlProg, conn))
                {
                    cmd.Parameters.AddWithValue("@m", mz);
                    cmd.Parameters.AddWithValue("@l", lt);
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            string clave = (rd["Clave"]?.ToString() ?? "").Trim();
                            string nombre = (rd["Descripcion"]?.ToString() ?? "").Trim();
                            string destajo = (rd["Destajo"]?.ToString() ?? "").Trim();
                            if (clave.Length == 0 && nombre.Length == 0) continue;

                            decimal cant = ParseDecimal(rd["Cantidad"]);
                            decimal precio = ParseDecimal(rd["Precio"]);

                            string key = clave.Length > 0 ? "C:" + clave.ToUpperInvariant() : "N:" + NormNombreSalida(nombre);
                            if (!programado.TryGetValue(key, out var ins))
                            {
                                ins = new InsumoSurtirAcumulado
                                {
                                    Clave = clave,
                                    Descripcion = nombre,
                                    Unidad = (rd["Unidad"]?.ToString() ?? "").Trim(),
                                    Precio = precio
                                };
                                programado[key] = ins;
                            }
                            ins.Programado += cant;
                            if (ins.Precio == 0m && precio > 0m) ins.Precio = precio;
                            if (destajo.Length > 0) ins.Destajos.Add(destajo);
                        }
                    }
                }

                if (programado.Count == 0)
                {
                    int activados;
                    using (var cmd = new SqlCommand(
                        @"SELECT COUNT(*) FROM ActivacionTareasRuta
                          WHERE Manzana = @m AND Lote = @l
                            AND Ruta = @ruta AND DesatajoActivado = 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@m", mz);
                        cmd.Parameters.AddWithValue("@l", lt);
                        cmd.Parameters.AddWithValue("@ruta", ruta);
                        activados = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    string msg = activados == 0
                        ? $"No se encontraron destajos ACTIVADOS para M{mz}-L{lt} en la ruta \"{ruta}\"."
                        : $"Hay {activados} destajo(s) activado(s), pero sus tareas no incluyen insumos de tipo Material.";
                    return Ok(new PendientesSalidaDto
                    {
                        Manzana = mz,
                        Lote = lt,
                        Prototipo = prototipo,
                        Insumos = new List<InsumoPendienteSalidaDto>(),
                        Mensaje = msg
                    });
                }

                var dispClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                var dispNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                var nombreAClave = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                CargarInventarioAlmacenSalida(conn, dispClave, dispNombre, nombreAClave);

                var catalogoNombreAClave = CargarCatalogoClavesPorNombreSalida(conn, ruta);

                var surtClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                var surtNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = new SqlCommand(@"
                    SELECT ISNULL(Clave,'') AS Clave, ISNULL(Descripcion,'') AS Descripcion, SUM(Cantidad) AS Surtido
                    FROM SalidasAlmacen
                    WHERE Manzana = @m AND Lote = @l
                    GROUP BY ISNULL(Clave,''), ISNULL(Descripcion,'')", conn))
                {
                    cmd.Parameters.AddWithValue("@m", mz);
                    cmd.Parameters.AddWithValue("@l", lt);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            string clv = (rd["Clave"]?.ToString() ?? "").Trim().ToUpperInvariant();
                            string nom = NormNombreSalida(rd["Descripcion"]?.ToString());
                            decimal s = ParseDecimal(rd["Surtido"]);
                            if (clv.Length > 0) { surtClave.TryGetValue(clv, out var a); surtClave[clv] = a + s; }
                            if (nom.Length > 0) { surtNombre.TryGetValue(nom, out var b); surtNombre[nom] = b + s; }
                        }
                    }
                }

                var resultado = new List<InsumoPendienteSalidaDto>();
                foreach (var ins in programado.Values)
                {
                    string nombreKey = NormNombreSalida(ins.Descripcion);
                    string claveAlmacen = ins.Clave ?? "";
                    if (string.IsNullOrEmpty(claveAlmacen))
                    {
                        if (catalogoNombreAClave.TryGetValue(nombreKey, out var ckCat) && ckCat.Length > 0)
                            claveAlmacen = ckCat;
                        else if (nombreAClave.TryGetValue(nombreKey, out var ckAlm) && ckAlm.Length > 0)
                            claveAlmacen = ckAlm;
                    }

                    bool hayClave = !string.IsNullOrEmpty(claveAlmacen);
                    string claveKey = claveAlmacen.ToUpperInvariant();

                    decimal yaSurtido = hayClave
                        ? (surtClave.TryGetValue(claveKey, out var sc) ? sc : (surtNombre.TryGetValue(nombreKey, out var sn0) ? sn0 : 0m))
                        : (surtNombre.TryGetValue(nombreKey, out var sn) ? sn : 0m);

                    decimal disponible = hayClave
                        ? (dispClave.TryGetValue(claveKey, out var dc) ? dc : 0m)
                        : (dispNombre.TryGetValue(nombreKey, out var dn) ? dn : 0m);

                    decimal pendiente = ins.Programado - yaSurtido;

                    resultado.Add(new InsumoPendienteSalidaDto
                    {
                        Destajo = ins.Destajos.Count > 0 ? string.Join(", ", ins.Destajos) : "—",
                        Clave = claveAlmacen,
                        Descripcion = string.IsNullOrEmpty(ins.Descripcion) ? "Sin descripcion" : ins.Descripcion,
                        Unidad = string.IsNullOrEmpty(ins.Unidad) ? "PZA" : ins.Unidad,
                        PrecioUnitario = ins.Precio,
                        Disponible = disponible,
                        MaximoPermitido = pendiente > 0m ? pendiente : 0m
                    });
                }

                resultado = resultado.OrderByDescending(r => r.MaximoPermitido).ThenBy(r => r.Descripcion).ToList();

                return Ok(new PendientesSalidaDto { Manzana = mz, Lote = lt, Prototipo = prototipo, Insumos = resultado });
            }
        }

        /// <summary>Disponible (Entradas - Salidas) por CLAVE y por NOMBRE (igual que CargarInventarioAlmacen).</summary>
        private static void CargarInventarioAlmacenSalida(
            SqlConnection conn,
            Dictionary<string, decimal> dispClave,
            Dictionary<string, decimal> dispNombre,
            Dictionary<string, string> nombreAClave)
        {
            var entClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var entNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var mejorEntradaPorNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(Clave,'') AS Clave, ISNULL(Descripcion,'') AS Descripcion, SUM(Cantidad) AS Entrada
                FROM EntradasAlmacen
                GROUP BY ISNULL(Clave,''), ISNULL(Descripcion,'')", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    string clv = (rd["Clave"]?.ToString() ?? "").Trim();
                    string nom = (rd["Descripcion"]?.ToString() ?? "").Trim();
                    decimal ent = ParseDecimal(rd["Entrada"]);
                    string clvK = clv.ToUpperInvariant();
                    string nomK = NormNombreSalida(nom);

                    if (clvK.Length > 0) { entClave.TryGetValue(clvK, out var a); entClave[clvK] = a + ent; }
                    if (nomK.Length > 0) { entNombre.TryGetValue(nomK, out var b); entNombre[nomK] = b + ent; }

                    if (nomK.Length > 0 && clv.Length > 0 &&
                        (!mejorEntradaPorNombre.TryGetValue(nomK, out var best) || ent > best))
                    {
                        mejorEntradaPorNombre[nomK] = ent;
                        nombreAClave[nomK] = clv;
                    }
                }
            }

            var salClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var salNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(Clave,'') AS Clave, ISNULL(Descripcion,'') AS Descripcion, SUM(Cantidad) AS Salida
                FROM SalidasAlmacen
                GROUP BY ISNULL(Clave,''), ISNULL(Descripcion,'')", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    string clv = (rd["Clave"]?.ToString() ?? "").Trim().ToUpperInvariant();
                    string nom = NormNombreSalida(rd["Descripcion"]?.ToString());
                    decimal sal = ParseDecimal(rd["Salida"]);
                    if (clv.Length > 0) { salClave.TryGetValue(clv, out var a); salClave[clv] = a + sal; }
                    if (nom.Length > 0) { salNombre.TryGetValue(nom, out var b); salNombre[nom] = b + sal; }
                }
            }

            foreach (var kv in entClave)
                dispClave[kv.Key] = kv.Value - (salClave.TryGetValue(kv.Key, out var s) ? s : 0m);
            foreach (var kv in entNombre)
                dispNombre[kv.Key] = kv.Value - (salNombre.TryGetValue(kv.Key, out var s) ? s : 0m);
        }

        /// <summary>Mapa NOMBRE normalizado -> Clave del catálogo maestro (igual que CargarCatalogoClavesPorNombre).</summary>
        private static Dictionary<string, string> CargarCatalogoClavesPorNombreSalida(SqlConnection conn, string ruta)
        {
            var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string tabla = (ruta ?? "").ToUpperInvariant().Contains("CALANDRA") ? "InsumosCalandraEXP" : "InsumosTuneraEXP";
            try
            {
                using (var cmd = new SqlCommand($"SELECT Clave, [Descripción] AS Descripcion FROM dbo.{tabla}", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string clave = (rd["Clave"]?.ToString() ?? "").Trim();
                        string nom = NormNombreSalida(rd["Descripcion"]?.ToString());
                        if (nom.Length == 0 || clave.Length == 0) continue;
                        if (!mapa.ContainsKey(nom)) mapa[nom] = clave;
                    }
                }
            }
            catch
            {
                // Sin catálogo: se cae a EntradasAlmacen por nombre (igual que la version clásica).
            }
            return mapa;
        }

        /// <summary>
        /// POST /api/almacen/salida — registra la salida (SalidasAlmacen + HistorialMovimientos)
        /// y reserva el folio del vale (RegistrarSalida). El PDF se genera en el cliente con
        /// PdfSharp y se archiva después con POST /api/almacen/salida/vale.
        /// </summary>
        [HttpPost, Route("salida"), RequierePermiso("almacen.editar")]
        public IHttpActionResult RegistrarSalida([FromBody] RegistrarSalidaRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Falta la casa destino.");
            if (req.Insumos == null || req.Insumos.Count == 0)
                return BadRequest("No hay insumos para registrar salida.");

            string usuario = User?.Identity?.Name ?? "desconocido";
            var registrados = new List<InsumoMovimientoDto>();
            decimal totalImporte = 0m;

            using (var conn = Db.Abrir())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    foreach (var linea in req.Insumos)
                    {
                        if (linea.CantidadSolicitada <= 0) continue;
                        decimal importe = linea.CantidadSolicitada * linea.PrecioUnitario;

                        using (var cmd = new SqlCommand(@"
                            INSERT INTO SalidasAlmacen
                            (Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, FechaSalida, Manzana, Lote, Prototipo, Justificacion)
                            VALUES (@Clave, @Descripcion, @Unidad, @Cantidad, @PrecioUnitario, @Importe, GETDATE(), @Manzana, @Lote, @Prototipo, NULL)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Clave", linea.Clave ?? "");
                            cmd.Parameters.AddWithValue("@Descripcion", linea.Descripcion ?? "");
                            cmd.Parameters.AddWithValue("@Unidad", linea.Unidad ?? "");
                            cmd.Parameters.AddWithValue("@Cantidad", linea.CantidadSolicitada);
                            cmd.Parameters.AddWithValue("@PrecioUnitario", linea.PrecioUnitario);
                            cmd.Parameters.AddWithValue("@Importe", importe);
                            cmd.Parameters.AddWithValue("@Manzana", req.Manzana);
                            cmd.Parameters.AddWithValue("@Lote", req.Lote);
                            cmd.Parameters.AddWithValue("@Prototipo", (object)req.Prototipo ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        using (var hist = new SqlCommand(@"
                            INSERT INTO HistorialMovimientos
                            (Fecha, TipoMovimiento, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, Usuario, Manzana, Lote, Prototipo, Justificacion)
                            VALUES (GETDATE(), 'Salida', @Clave, @Descripcion, @Unidad, @Cantidad, @PrecioUnitario, @Importe, @Usuario, @Manzana, @Lote, @Prototipo, NULL)", conn, tx))
                        {
                            hist.Parameters.AddWithValue("@Clave", linea.Clave ?? "");
                            hist.Parameters.AddWithValue("@Descripcion", linea.Descripcion ?? "");
                            hist.Parameters.AddWithValue("@Unidad", linea.Unidad ?? "");
                            hist.Parameters.AddWithValue("@Cantidad", linea.CantidadSolicitada);
                            hist.Parameters.AddWithValue("@PrecioUnitario", linea.PrecioUnitario);
                            hist.Parameters.AddWithValue("@Importe", importe);
                            hist.Parameters.AddWithValue("@Usuario", usuario);
                            hist.Parameters.AddWithValue("@Manzana", req.Manzana);
                            hist.Parameters.AddWithValue("@Lote", req.Lote);
                            hist.Parameters.AddWithValue("@Prototipo", (object)req.Prototipo ?? DBNull.Value);
                            hist.ExecuteNonQuery();
                        }

                        totalImporte += importe;
                        registrados.Add(new InsumoMovimientoDto
                        {
                            Clave = linea.Clave,
                            Descripcion = linea.Descripcion,
                            Unidad = linea.Unidad,
                            Cantidad = linea.CantidadSolicitada,
                            PrecioUnitario = linea.PrecioUnitario,
                            Importe = importe
                        });
                    }

                    if (registrados.Count == 0)
                    {
                        tx.Rollback();
                        return BadRequest("No se capturó ninguna cantidad para registrar salida.");
                    }

                    EnsureTablaRepositorioVales(conn, tx);
                    string folio = GenerarFolioSalida(conn, tx, req.Manzana, req.Lote);

                    tx.Commit();

                    return Ok(new ResultadoSalidaDto
                    {
                        Ok = true,
                        Mensaje = "Salida registrada correctamente.",
                        Folio = folio,
                        TotalImporte = totalImporte,
                        Insumos = registrados
                    });
                }
                catch { tx.Rollback(); throw; }
            }
        }

        private static string GenerarFolioSalida(SqlConnection conn, SqlTransaction tx, string manzana, string lote)
        {
            int siguiente;
            using (var cmd = new SqlCommand(
                "SELECT ISNULL(MAX(NumeroSalida), 0) + 1 FROM FoliosSalidaAlmacen WHERE Manzana = @m AND Lote = @l", conn, tx))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                siguiente = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return $"VALE-M{manzana}-L{lote}-{siguiente:D3}-{DateTime.Now.Year}";
        }

        /// <summary>
        /// POST /api/almacen/salida/vale — archiva en el repositorio (FoliosSalidaAlmacen +
        /// Detalle + PDFsSalidaAlmacen) el vale cuyo PDF ya generó el cliente con PdfSharp.
        /// </summary>
        [HttpPost, Route("salida/vale"), RequierePermiso("almacen.editar")]
        public IHttpActionResult ArchivarVale([FromBody] ArchivarValeSalidaRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Folio) || string.IsNullOrWhiteSpace(req.PdfBase64))
                return BadRequest("Faltan datos del vale.");

            byte[] pdfBytes;
            try { pdfBytes = Convert.FromBase64String(req.PdfBase64); }
            catch { return BadRequest("El contenido del PDF no es base64 válido."); }

            using (var conn = Db.Abrir())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    EnsureTablaRepositorioVales(conn, tx);

                    DateTime fecha = DateTime.Now;
                    int numeroSalida;
                    using (var cmd = new SqlCommand(
                        "SELECT ISNULL(MAX(NumeroSalida), 0) + 1 FROM FoliosSalidaAlmacen WHERE Manzana = @m AND Lote = @l", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@m", req.Manzana);
                        cmd.Parameters.AddWithValue("@l", req.Lote);
                        numeroSalida = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    int folioId;
                    using (var cmd = new SqlCommand(@"
                        INSERT INTO FoliosSalidaAlmacen
                        (Folio, Manzana, Lote, Prototipo, Obra, FechaSolicitud, DiaSolicitud, MesSolicitud, AnioSolicitud,
                         TotalImporte, NumeroSalida, Usuario, Solicitante, ResidenteObra, EncargadoAlmacen, Observaciones, Estado)
                        VALUES
                        (@Folio, @Manzana, @Lote, @Prototipo, @Obra, @Fecha, @Dia, @Mes, @Anio,
                         @Total, @NumSalida, @Usuario, @Solicitante, @Residente, @Encargado, @Obs, 'PENDIENTE');
                        SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Folio", req.Folio);
                        cmd.Parameters.AddWithValue("@Manzana", req.Manzana);
                        cmd.Parameters.AddWithValue("@Lote", req.Lote);
                        cmd.Parameters.AddWithValue("@Prototipo", (object)req.Prototipo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Obra", $"M{req.Manzana}-L{req.Lote}");
                        cmd.Parameters.AddWithValue("@Fecha", fecha);
                        cmd.Parameters.AddWithValue("@Dia", fecha.Day);
                        cmd.Parameters.AddWithValue("@Mes", fecha.Month);
                        cmd.Parameters.AddWithValue("@Anio", fecha.Year);
                        cmd.Parameters.AddWithValue("@Total", req.TotalImporte);
                        cmd.Parameters.AddWithValue("@NumSalida", numeroSalida);
                        cmd.Parameters.AddWithValue("@Usuario", User?.Identity?.Name ?? "desconocido");
                        cmd.Parameters.AddWithValue("@Solicitante", (object)req.Solicitante ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Residente", (object)req.ResidenteObra ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Encargado", (object)req.EncargadoAlmacen ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Obs", (object)req.Observaciones ?? DBNull.Value);
                        folioId = (int)cmd.ExecuteScalar();
                    }

                    int numeroFila = 1;
                    foreach (var insumo in req.Insumos ?? new List<InsumoMovimientoDto>())
                    {
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO FoliosSalidaAlmacenDetalle
                            (FolioId, NumeroFila, Codigo, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe)
                            VALUES (@FolioId, @NumFila, @Codigo, @Desc, @Unidad, @Cant, @Precio, @Importe)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@FolioId", folioId);
                            cmd.Parameters.AddWithValue("@NumFila", numeroFila++);
                            cmd.Parameters.AddWithValue("@Codigo", insumo.Clave ?? "");
                            cmd.Parameters.AddWithValue("@Desc", insumo.Descripcion ?? "");
                            cmd.Parameters.AddWithValue("@Unidad", insumo.Unidad ?? "");
                            cmd.Parameters.AddWithValue("@Cant", insumo.Cantidad);
                            cmd.Parameters.AddWithValue("@Precio", insumo.PrecioUnitario);
                            cmd.Parameters.AddWithValue("@Importe", insumo.Importe);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO PDFsSalidaAlmacen
                        (FolioId, Folio, Manzana, Lote, NombreArchivo, ContenidoPDF, TamanioBytes)
                        VALUES (@FolioId, @Folio, @Manzana, @Lote, @Nombre, @Contenido, @Tamanio)", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@FolioId", folioId);
                        cmd.Parameters.AddWithValue("@Folio", req.Folio);
                        cmd.Parameters.AddWithValue("@Manzana", req.Manzana);
                        cmd.Parameters.AddWithValue("@Lote", req.Lote);
                        cmd.Parameters.AddWithValue("@Nombre",
                            string.IsNullOrWhiteSpace(req.NombreArchivo) ? $"ValeSalida_{req.Folio}.pdf" : req.NombreArchivo);
                        cmd.Parameters.AddWithValue("@Contenido", pdfBytes);
                        cmd.Parameters.AddWithValue("@Tamanio", (long)pdfBytes.Length);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return Ok(new { ok = true, folioId });
                }
                catch { tx.Rollback(); throw; }
            }
        }

        /// <summary>DDL idempotente del repositorio de vales (equivalente a CrearTablasRepositorioSalidasAlmacen.sql / SalidaAlmacenService.CrearTablasRepositorioSalidasSiNoExisten).</summary>
        private static void EnsureTablaRepositorioVales(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FoliosSalidaAlmacen]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FoliosSalidaAlmacen] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Folio] NVARCHAR(50) NOT NULL UNIQUE,
        [Manzana] NVARCHAR(10) NOT NULL,
        [Lote] NVARCHAR(10) NOT NULL,
        [Prototipo] NVARCHAR(50) NULL,
        [Obra] NVARCHAR(200) NULL,
        [FechaSolicitud] DATETIME NOT NULL DEFAULT(GETDATE()),
        [DiaSolicitud] INT NULL,
        [MesSolicitud] INT NULL,
        [AnioSolicitud] INT NULL,
        [TotalImporte] DECIMAL(18,2) NULL DEFAULT(0),
        [NumeroSalida] INT NULL,
        [Usuario] NVARCHAR(100) NULL,
        [Solicitante] NVARCHAR(200) NULL,
        [ResidenteObra] NVARCHAR(200) NULL,
        [EncargadoAlmacen] NVARCHAR(200) NULL,
        [Observaciones] NVARCHAR(MAX) NULL,
        [Estado] NVARCHAR(20) NULL DEFAULT('PENDIENTE')
    );
END
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FoliosSalidaAlmacenDetalle]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FoliosSalidaAlmacenDetalle] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [FolioId] INT NOT NULL,
        [NumeroFila] INT NULL,
        [Codigo] NVARCHAR(100) NULL,
        [Descripcion] NVARCHAR(500) NULL,
        [Unidad] NVARCHAR(50) NULL,
        [Cantidad] DECIMAL(18,3) NULL,
        [PrecioUnitario] DECIMAL(18,2) NULL,
        [Importe] DECIMAL(18,2) NULL,
        [Observaciones] NVARCHAR(500) NULL,
        CONSTRAINT [FK_FoliosSalidaDetalle_Folio]
            FOREIGN KEY ([FolioId]) REFERENCES [dbo].[FoliosSalidaAlmacen]([Id])
            ON DELETE CASCADE
    );
END
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PDFsSalidaAlmacen]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PDFsSalidaAlmacen] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [FolioId] INT NOT NULL,
        [Folio] NVARCHAR(50) NOT NULL,
        [Manzana] NVARCHAR(10) NOT NULL,
        [Lote] NVARCHAR(10) NOT NULL,
        [NombreArchivo] NVARCHAR(255) NOT NULL,
        [ContenidoPDF] VARBINARY(MAX) NOT NULL,
        [TamanioBytes] BIGINT NOT NULL,
        [FechaAlmacenamiento] DATETIME NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT [FK_PDFsSalida_Folio]
            FOREIGN KEY ([FolioId]) REFERENCES [dbo].[FoliosSalidaAlmacen]([Id])
            ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_PDFsSalida_ManzanaLote] ON [dbo].[PDFsSalidaAlmacen] ([Manzana], [Lote]);
    CREATE NONCLUSTERED INDEX [IX_PDFsSalida_Folio] ON [dbo].[PDFsSalidaAlmacen] ([Folio]);
END";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }
    }
}
