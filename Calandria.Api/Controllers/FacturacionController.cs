using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Facturación del add-on "Conciliación de factura con IA" a Pilaris: saldo
    /// por obra (todas las obras activas de esta instalación — cada una tiene su
    /// propia BD, ConciliacionesIaLog vive ahí, no en la BD maestra), historial de
    /// facturas conciliadas por obra, y repositorio de PDFs de resumen generados
    /// (PDFsFacturacion, en la BD maestra: es información del proveedor del
    /// sistema, no de una obra en particular). Todo bajo sistema.facturacion —
    /// deliberadamente fuera del perfil de un usuario normal de compras.
    /// </summary>
    [RoutePrefix("api/facturacion"), RequierePermiso("sistema.facturacion")]
    public class FacturacionController : ApiController
    {
        /// <summary>GET api/facturacion/saldos?desde=&amp;hasta= — conteo y monto a cobrar de cada obra activa en el rango.</summary>
        [HttpGet, Route("saldos")]
        public IHttpActionResult Saldos(DateTime desde, DateTime hasta)
        {
            DateTime hastaFin = hasta.Date.AddDays(1).AddSeconds(-1);
            var resultado = new List<SaldoObraDto>();

            foreach (var o in ListarObras())
            {
                var dto = new SaldoObraDto { ObraId = o.Id, ObraNombre = o.Nombre };
                try
                {
                    using (var conn = new SqlConnection(Configuracion.CadenaConexionObra(o.NombreBD)))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM ConciliacionesIaLog WHERE Fecha BETWEEN @d AND @h", conn))
                        {
                            cmd.Parameters.AddWithValue("@d", desde.Date);
                            cmd.Parameters.AddWithValue("@h", hastaFin);
                            dto.Conteo = (int)cmd.ExecuteScalar();
                        }
                    }
                    dto.MontoACobrar = Math.Max(dto.Conteo * 18m, 500m);
                    dto.TablaDisponible = true;
                }
                catch (SqlException)
                {
                    // Obra sin la migración 8_APLICAR_ConciliacionesIaLog.sql aplicada
                    // todavía: no es que no deba nada, es que no hay dato.
                    dto.TablaDisponible = false;
                }
                resultado.Add(dto);
            }

            return Ok(resultado);
        }

        /// <summary>GET api/facturacion/{obraId}/historial?desde=&amp;hasta= — facturas conciliadas con IA de una obra puntual.</summary>
        [HttpGet, Route("{obraId:int}/historial")]
        public IHttpActionResult Historial(int obraId, DateTime desde, DateTime hasta)
        {
            string nombreBd = ObtenerNombreBd(obraId);
            if (nombreBd == null) return NotFound();

            DateTime hastaFin = hasta.Date.AddDays(1).AddSeconds(-1);
            var lista = new List<ConciliacionIaHistorialDto>();
            try
            {
                using (var conn = new SqlConnection(Configuracion.CadenaConexionObra(nombreBd)))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT Uuid, FolioOC, Fecha, Usuario FROM ConciliacionesIaLog " +
                        "WHERE Fecha BETWEEN @d AND @h ORDER BY Fecha DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@d", desde.Date);
                        cmd.Parameters.AddWithValue("@h", hastaFin);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                lista.Add(new ConciliacionIaHistorialDto
                                {
                                    Uuid = r["Uuid"].ToString(),
                                    FolioOC = r["FolioOC"].ToString(),
                                    Fecha = Convert.ToDateTime(r["Fecha"]),
                                    Usuario = r["Usuario"] == DBNull.Value ? "" : r["Usuario"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException)
            {
                // Tabla no disponible en esta obra todavía: historial vacío, no error.
            }
            return Ok(lista);
        }

        // ------------------------------------------------------------------
        // Repositorio de PDFs de resumen (BD maestra: PDFsFacturacion)
        // ------------------------------------------------------------------

        /// <summary>POST api/facturacion/repositorio · archiva un PDF de resumen ya generado en el cliente.</summary>
        [HttpPost, Route("repositorio")]
        public IHttpActionResult GuardarPdf([FromBody] GuardarPdfFacturacionRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.PdfBase64))
                return BadRequest("Falta el PDF.");

            byte[] pdf = Convert.FromBase64String(req.PdfBase64);

            using (var conn = Db.AbrirMaestra())
            {
                EnsureTabla(conn);
                using (var cmd = new SqlCommand(@"
INSERT INTO PDFsFacturacion
(ObraId, ObraNombre, Desde, Hasta, Conteo, MontoACobrar, NombreArchivo, ContenidoPDF, TamanioBytes, Usuario)
OUTPUT INSERTED.Id
VALUES (@obraId, @obraNombre, @desde, @hasta, @conteo, @monto, @nombre, @cont, @tam, @usuario)", conn))
                {
                    cmd.Parameters.AddWithValue("@obraId", (object)req.ObraId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@obraNombre", (object)req.ObraNombre ?? "Todas las obras");
                    cmd.Parameters.AddWithValue("@desde", req.Desde.Date);
                    cmd.Parameters.AddWithValue("@hasta", req.Hasta.Date);
                    cmd.Parameters.AddWithValue("@conteo", req.Conteo);
                    cmd.Parameters.AddWithValue("@monto", req.MontoACobrar);
                    cmd.Parameters.AddWithValue("@nombre", (object)req.NombreArchivo ?? "");
                    var pCont = cmd.Parameters.Add("@cont", SqlDbType.VarBinary, -1);
                    pCont.Value = pdf;
                    cmd.Parameters.AddWithValue("@tam", (long)pdf.Length);
                    cmd.Parameters.AddWithValue("@usuario", User.Identity.Name);
                    int id = (int)cmd.ExecuteScalar();
                    return Ok(new { Id = id });
                }
            }
        }

        /// <summary>GET api/facturacion/repositorio · historial de PDFs de resumen generados.</summary>
        [HttpGet, Route("repositorio")]
        public IHttpActionResult ListarRepositorio()
        {
            var lista = new List<PdfFacturacionDto>();
            using (var conn = Db.AbrirMaestra())
            {
                EnsureTabla(conn);
                using (var cmd = new SqlCommand(@"
SELECT Id, ObraId, ObraNombre, Desde, Hasta, Conteo, MontoACobrar, NombreArchivo, FechaGeneracion, Usuario
FROM PDFsFacturacion ORDER BY FechaGeneracion DESC", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new PdfFacturacionDto
                        {
                            Id = (int)r["Id"],
                            ObraId = r["ObraId"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ObraId"]),
                            ObraNombre = r["ObraNombre"].ToString(),
                            Desde = Convert.ToDateTime(r["Desde"]),
                            Hasta = Convert.ToDateTime(r["Hasta"]),
                            Conteo = Convert.ToInt32(r["Conteo"]),
                            MontoACobrar = Convert.ToDecimal(r["MontoACobrar"]),
                            NombreArchivo = r["NombreArchivo"]?.ToString(),
                            FechaGeneracion = Convert.ToDateTime(r["FechaGeneracion"]),
                            Usuario = r["Usuario"]?.ToString()
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>GET api/facturacion/repositorio/{id}/pdf · binario del PDF archivado.</summary>
        [HttpGet, Route("repositorio/{id:int}/pdf")]
        public HttpResponseMessage Pdf(int id)
        {
            byte[] bytes = null;
            string nombre = null;
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand("SELECT ContenidoPDF, NombreArchivo FROM PDFsFacturacion WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        bytes = r["ContenidoPDF"] == DBNull.Value ? null : (byte[])r["ContenidoPDF"];
                        nombre = r["NombreArchivo"]?.ToString();
                    }
                }
            }

            if (bytes == null || bytes.Length == 0)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            resp.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment") { FileName = nombre ?? $"Facturacion_{id}.pdf" };
            return resp;
        }

        /// <summary>POST api/facturacion/repositorio/{id}/eliminar · borra un PDF archivado.</summary>
        [HttpPost, Route("repositorio/{id:int}/eliminar")]
        public IHttpActionResult EliminarPdf(int id)
        {
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand("DELETE FROM PDFsFacturacion WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                return Ok(cmd.ExecuteNonQuery());
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private sealed class ObraRow
        {
            public int Id;
            public string Nombre;
            public string NombreBD;
        }

        private static List<ObraRow> ListarObras()
        {
            var lista = new List<ObraRow>();
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand("SELECT Id, Nombre, NombreBD FROM Obras WHERE Activa = 1 ORDER BY Nombre", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    lista.Add(new ObraRow { Id = (int)r["Id"], Nombre = r["Nombre"].ToString(), NombreBD = r["NombreBD"].ToString() });
            }
            return lista;
        }

        private static string ObtenerNombreBd(int obraId)
        {
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand("SELECT NombreBD FROM Obras WHERE Id = @id AND Activa = 1", conn))
            {
                cmd.Parameters.AddWithValue("@id", obraId);
                return cmd.ExecuteScalar() as string;
            }
        }

        /// <summary>Crea PDFsFacturacion en la BD maestra si no existe (mismo patrón que RepositorioOrdenesController.EnsureTablas).</summary>
        private static void EnsureTabla(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PDFsFacturacion')
BEGIN
    CREATE TABLE PDFsFacturacion (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ObraId INT NULL,
        ObraNombre NVARCHAR(200) NOT NULL,
        Desde DATE NOT NULL,
        Hasta DATE NOT NULL,
        Conteo INT NOT NULL,
        MontoACobrar DECIMAL(18,2) NOT NULL,
        NombreArchivo NVARCHAR(255),
        ContenidoPDF VARBINARY(MAX) NOT NULL,
        TamanioBytes BIGINT,
        Usuario NVARCHAR(100),
        FechaGeneracion DATETIME DEFAULT GETDATE()
    );
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }
    }
}
