using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;
using Calandria.Api.Services;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Compras · alta de órdenes de compra. Reproduce el guardado de
    /// FormCompraMulti (tipo MULTIPLE: cabecera + casas + detalle) y
    /// FormCompraIndirecta (tipo INDIRECTA: cabecera + detalle). El folio se genera
    /// en el servidor dentro de la misma transacción que los INSERT y se devuelve
    /// al cliente (lo necesita para nombrar el PDF y el repositorio).
    /// </summary>
    [RoutePrefix("api/ordenescompra"), RequierePermiso("compras.ver")]
    public class OrdenesCompraController : ApiController
    {
        /// <summary>POST /api/ordenescompra/multiple → folio generado.</summary>
        [HttpPost, Route("multiple"), RequierePermiso("compras.editar")]
        public IHttpActionResult CrearMultiple([FromBody] CrearOrdenMultipleRequest req)
        {
            if (req == null)
                return BadRequest("Cuerpo vacío.");
            if (req.Detalles == null || req.Detalles.Count == 0)
                return BadRequest("La orden no tiene detalle.");

            using (var conn = Db.Abrir())
            using (var tx = conn.BeginTransaction())
            {
                string folio = GenerarFolio(conn, tx, "OC-MULTI-");

                using (var cmd = new SqlCommand(
                    "INSERT INTO OrdenesCompra (FolioOC, Fecha, Usuario, TipoOrden, NombreOrden) " +
                    "VALUES (@folio, @fecha, @usuario, @tipo, @nombre)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@folio", folio);
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                    cmd.Parameters.AddWithValue("@usuario", (object)req.Usuario ?? "");
                    cmd.Parameters.AddWithValue("@tipo", "MULTIPLE");
                    cmd.Parameters.AddWithValue("@nombre", (object)req.NombreOrden ?? "");
                    cmd.ExecuteNonQuery();
                }

                if (req.Casas != null)
                {
                    foreach (var casa in req.Casas)
                    {
                        using (var cmd = new SqlCommand(
                            "INSERT INTO OrdenesCompra_Casas (FolioOC, Manzana, Lote) VALUES (@folio, @m, @l)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@folio", folio);
                            cmd.Parameters.AddWithValue("@m", (object)casa.Manzana ?? "");
                            cmd.Parameters.AddWithValue("@l", (object)casa.Lote ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                foreach (var d in req.Detalles)
                    InsertarDetalle(conn, tx, folio, d, incluirFamiliaEstado: true);

                tx.Commit();
                return Ok(new FolioOrdenResponse { FolioOC = folio });
            }
        }

        /// <summary>POST /api/ordenescompra/indirecta → folio generado.</summary>
        [HttpPost, Route("indirecta"), RequierePermiso("compras.editar")]
        public IHttpActionResult CrearIndirecta([FromBody] CrearOrdenIndirectaRequest req)
        {
            if (req == null)
                return BadRequest("Cuerpo vacío.");
            if (req.Detalles == null || req.Detalles.Count == 0)
                return BadRequest("La orden no tiene detalle.");

            using (var conn = Db.Abrir())
            using (var tx = conn.BeginTransaction())
            {
                string folio = GenerarFolio(conn, tx, "OC-IND-");

                using (var cmd = new SqlCommand(
                    "INSERT INTO OrdenesCompra (FolioOC, Fecha, Usuario, TipoOrden, ProveedorClave, NombreOrden) " +
                    "VALUES (@folio, @fecha, @usuario, @tipo, @prov, @nombre)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@folio", folio);
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                    cmd.Parameters.AddWithValue("@usuario", (object)req.Usuario ?? "");
                    cmd.Parameters.AddWithValue("@tipo", "INDIRECTA");
                    cmd.Parameters.AddWithValue("@prov", (object)req.ProveedorClave ?? "");
                    cmd.Parameters.AddWithValue("@nombre", (object)req.NombreOrden ?? "");
                    cmd.ExecuteNonQuery();
                }

                foreach (var d in req.Detalles)
                    InsertarDetalle(conn, tx, folio, d, incluirFamiliaEstado: false);

                tx.Commit();
                return Ok(new FolioOrdenResponse { FolioOC = folio });
            }
        }

        /// <summary>
        /// POST /api/ordenescompra/conciliar-factura → compara un CFDI (XML) contra el
        /// detalle PENDIENTE de la OC. El XML da los datos exactos; el emparejamiento es
        /// determinista (clave → nombre) y, si está configurado, un emparejador automático para lo dudoso. NO registra
        /// la entrada: solo devuelve el comparativo para precargar y que el usuario confirme.
        /// </summary>
        [HttpPost, Route("conciliar-factura"), RequierePermiso("compras.editar")]
        public IHttpActionResult ConciliarFactura([FromBody] ConciliarFacturaRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.FolioOC))
                return BadRequest("Falta el folio de la orden.");
            if (string.IsNullOrWhiteSpace(req.XmlBase64))
                return BadRequest("Falta el XML de la factura.");

            string xml;
            try
            {
                byte[] xmlBytes = Convert.FromBase64String(req.XmlBase64);
                if (xmlBytes.Length > 10 * 1024 * 1024)
                    return BadRequest("El XML no puede superar los 10 MB");
                xml = System.Text.Encoding.UTF8.GetString(xmlBytes);
            }
            catch (FormatException)
            {
                return BadRequest("El XML enviado no es base64 válido.");
            }

            CfdiParseado cfdi;
            try
            {
                cfdi = CfdiParser.Parse(xml);
            }
            catch (Exception)
            {
                return BadRequest("No se pudo leer el CFDI: el XML no tiene un formato válido.");
            }

            var lineasOC = new List<OcLinea>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT Id, Clave, Descripcion, Unidad, Cantidad FROM OrdenesCompraDetalle " +
                "WHERE FolioOC = @folio AND Estado = 'PENDIENTE'", conn))
            {
                cmd.Parameters.AddWithValue("@folio", req.FolioOC);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lineasOC.Add(new OcLinea
                        {
                            IdDetalle = Convert.ToInt32(reader["Id"]),
                            Clave = reader["Clave"]?.ToString() ?? "",
                            Descripcion = reader["Descripcion"]?.ToString() ?? "",
                            Unidad = reader["Unidad"]?.ToString() ?? "",
                            Cantidad = reader["Cantidad"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Cantidad"])
                        });
                    }
                }
            }

            if (lineasOC.Count == 0)
                return BadRequest("La orden no tiene partidas pendientes para conciliar.");

            var resp = ConciliadorFactura.Conciliar(req.FolioOC, cfdi, lineasOC);

            if (resp.UsoIA && !string.IsNullOrWhiteSpace(cfdi.Uuid))
            {
                using (var conn = Db.Abrir())
                    RegistrarConciliacionIaLog(conn, null, cfdi.Uuid, req.FolioOC, User.Identity.Name);
            }

            return Ok(resp);
        }

        /// <summary>
        /// Registra, deduplicado por Uuid (folio fiscal del CFDI), que una factura se
        /// concilió usando el emparejador automático (IA) — es la base para facturar el
        /// add-on a Pilaris. El mismo CFDI puede pasar por ConciliarFactura varias veces
        /// mientras el usuario ajusta la conciliación antes de confirmar, así que no debe
        /// contarse más de una vez por Uuid. Comprobación de existencia antes del insert,
        /// respaldada por el constraint único UQ_ConciliacionesIaLog_Uuid para la carrera
        /// entre dos peticiones concurrentes con el mismo CFDI.
        /// </summary>
        private static void RegistrarConciliacionIaLog(
            SqlConnection conn, SqlTransaction tx, string uuid, string folioOC, string usuario)
        {
            using (var check = new SqlCommand("SELECT COUNT(*) FROM ConciliacionesIaLog WHERE Uuid = @uuid", conn, tx))
            {
                check.Parameters.AddWithValue("@uuid", uuid);
                if ((int)check.ExecuteScalar() > 0)
                    return;
            }

            try
            {
                using (var cmd = new SqlCommand(
                    "INSERT INTO ConciliacionesIaLog (Uuid, FolioOC, Fecha, Usuario) VALUES (@uuid, @folio, @fecha, @usuario)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@uuid", uuid);
                    cmd.Parameters.AddWithValue("@folio", folioOC);
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                    cmd.Parameters.AddWithValue("@usuario", (object)usuario ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Ya lo insertó una petición concurrente con el mismo Uuid: ignorar.
            }
        }

        /// <summary>
        /// GET /api/ordenescompra/conciliaciones-ia/resumen?desde=&amp;hasta= — conteo de
        /// facturas conciliadas con el emparejador IA en el rango y el monto a cobrar del
        /// add-on ($18 MXN/factura, mínimo $500 MXN/mes). Es información de facturación
        /// del proveedor del sistema (Pilaris), no del cliente: requiere sistema.facturacion,
        /// que no debe estar en el perfil de un usuario normal de compras.
        /// </summary>
        [HttpGet, Route("conciliaciones-ia/resumen"), RequierePermiso("sistema.facturacion")]
        public IHttpActionResult ResumenConciliacionesIa(DateTime desde, DateTime hasta)
        {
            DateTime hastaFin = hasta.Date.AddDays(1).AddSeconds(-1);

            int conteo;
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM ConciliacionesIaLog WHERE Fecha BETWEEN @d AND @h", conn))
            {
                cmd.Parameters.AddWithValue("@d", desde.Date);
                cmd.Parameters.AddWithValue("@h", hastaFin);
                conteo = (int)cmd.ExecuteScalar();
            }

            return Ok(new ConciliacionesIaResumenDto
            {
                Conteo = conteo,
                MontoACobrar = Math.Max(conteo * 18m, 500m)
            });
        }

        private static void InsertarDetalle(SqlConnection conn, SqlTransaction tx,
            string folio, DetalleOrdenDto d, bool incluirFamiliaEstado)
        {
            string sql = incluirFamiliaEstado
                ? "INSERT INTO OrdenesCompraDetalle (FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Familia, Estado) " +
                  "VALUES (@folio, @c, @d, @u, @q, @precio, @importe, @f, 'PENDIENTE')"
                : "INSERT INTO OrdenesCompraDetalle (FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal) " +
                  "VALUES (@folio, @c, @d, @u, @q, @precio, @importe)";

            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@folio", folio);
                cmd.Parameters.AddWithValue("@c", (object)d.Clave ?? "");
                cmd.Parameters.AddWithValue("@d", (object)d.Descripcion ?? "");
                cmd.Parameters.AddWithValue("@u", (object)d.Unidad ?? "");
                cmd.Parameters.AddWithValue("@q", d.Cantidad);
                cmd.Parameters.AddWithValue("@precio", d.PrecioUnitario);
                cmd.Parameters.AddWithValue("@importe", d.ImporteTotal);
                if (incluirFamiliaEstado)
                    cmd.Parameters.AddWithValue("@f", (object)d.Familia ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Folio consecutivo global: PREFIJO + NNNNNN, contando todos los folios ya
        /// existentes con ese prefijo (sin reiniciar por día), dentro de la
        /// transacción del alta.
        /// </summary>
        private static string GenerarFolio(SqlConnection conn, SqlTransaction tx, string prefijo)
        {
            int consecutivo = 1;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM OrdenesCompra WHERE FolioOC LIKE @base + '%'", conn, tx))
            {
                cmd.Parameters.AddWithValue("@base", prefijo);
                consecutivo += (int)cmd.ExecuteScalar();
            }
            return prefijo + consecutivo.ToString("D6");
        }
    }
}
