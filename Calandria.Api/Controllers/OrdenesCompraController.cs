using System;
using System.Data.SqlClient;
using System.Web.Http;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Compras · alta de órdenes de compra. Reproduce el guardado de
    /// FormCompraMulti (tipo MULTIPLE: cabecera + casas + detalle) y
    /// FormCompraIndirecta (tipo INDIRECTA: cabecera + detalle). El folio se genera
    /// en el servidor dentro de la misma transacción que los INSERT y se devuelve
    /// al cliente (lo necesita para nombrar el PDF y el repositorio).
    /// </summary>
    [RoutePrefix("api/ordenescompra")]
    public class OrdenesCompraController : ApiController
    {
        /// <summary>POST /api/ordenescompra/multiple → folio generado.</summary>
        [HttpPost, Route("multiple")]
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
        [HttpPost, Route("indirecta")]
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
        /// Folio consecutivo del día: PREFIJO + yyyyMMdd + "-" + NNN, contando los
        /// folios ya existentes con ese prefijo (igual que GenerarFolioOC del cliente,
        /// pero dentro de la transacción del alta).
        /// </summary>
        private static string GenerarFolio(SqlConnection conn, SqlTransaction tx, string prefijo)
        {
            string baseFolio = prefijo + DateTime.Now.ToString("yyyyMMdd") + "-";
            int consecutivo = 1;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM OrdenesCompra WHERE FolioOC LIKE @base + '%'", conn, tx))
            {
                cmd.Parameters.AddWithValue("@base", baseFolio);
                consecutivo += (int)cmd.ExecuteScalar();
            }
            return baseFolio + consecutivo.ToString("D3");
        }
    }
}
