using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Evidencias fotográficas de avance de obra (tabla EvidenciasFotograficas).
    /// Reproduce el servicio de cliente GestorEvidencias: alta, última evidencia de
    /// un lote, listado, binario y borrado. La foto viaja en base64 al guardar y
    /// como binario al consultar. La columna de tamaño puede variar con/sin acentos,
    /// así que se detecta en el servidor.
    /// </summary>
    [RoutePrefix("api/evidencias"), RequierePermiso("estimaciones.ver")]
    public class EvidenciasController : ApiController
    {
        /// <summary>POST /api/evidencias · guarda una evidencia. Devuelve true.</summary>
        [HttpPost, Route(""), RequierePermiso("estimaciones.editar")]
        public IHttpActionResult Guardar([FromBody] GuardarEvidenciaRequest req)
        {
            if (req == null)
                return BadRequest("Petición vacía.");
            if (string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Manzana y Lote son obligatorios");
            if (string.IsNullOrWhiteSpace(req.Titulo) || req.Titulo.Length < 3)
                return BadRequest("El título debe tener al menos 3 caracteres");
            if (string.IsNullOrEmpty(req.FotoBase64))
                return BadRequest("La foto no puede estar vacía");

            byte[] foto = Convert.FromBase64String(req.FotoBase64);
            if (foto.Length == 0)
                return BadRequest("La foto no puede estar vacía");
            if (foto.Length > 10 * 1024 * 1024)
                return BadRequest("La foto no puede superar los 10 MB");
            if (!Services.ImagenValidacion.EsImagenValida(foto))
                return BadRequest("El archivo no es una imagen válida (jpg/png/webp)");

            double tamanioKB = foto.Length / 1024.0;

            using (var conn = Db.Abrir())
            {
                string prototipo = ObtenerPrototipo(conn, req.Manzana, req.Lote);
                string colTamanio = DetectarColumnaTamanio(conn);

                string sql = $@"
INSERT INTO EvidenciasFotograficas
    (Manzana, Lote, Prototipo, TituloFoto, Foto, Extension, [{colTamanio}], UsuarioCaptura)
VALUES
    (@Manzana, @Lote, @Prototipo, @Titulo, @Foto, @Extension, @TamanioKB, @Usuario)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", req.Manzana);
                    cmd.Parameters.AddWithValue("@Lote", req.Lote);
                    cmd.Parameters.AddWithValue("@Prototipo", (object)prototipo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Titulo", req.Titulo);
                    cmd.Parameters.AddWithValue("@Foto", foto);
                    cmd.Parameters.AddWithValue("@Extension", req.Extension ?? "jpg");
                    cmd.Parameters.AddWithValue("@TamanioKB", tamanioKB);
                    cmd.Parameters.AddWithValue("@Usuario", req.Usuario ?? "Sistema");
                    return Ok(cmd.ExecuteNonQuery() > 0);
                }
            }
        }

        /// <summary>
        /// GET /api/evidencias/ultima?manzana=&amp;lote= · última evidencia del lote,
        /// con la foto en base64. Devuelve null si no hay.
        /// </summary>
        [HttpGet, Route("ultima")]
        public IHttpActionResult Ultima(string manzana, string lote)
        {
            using (var conn = Db.Abrir())
            {
                string colTamanio = DetectarColumnaTamanio(conn);
                string sql = $@"
SELECT TOP 1 Id, Manzana, Lote, Prototipo, TituloFoto,
       Foto, Extension, [{colTamanio}] AS TamanioKB, FechaCaptura, UsuarioCaptura
FROM EvidenciasFotograficas
WHERE Manzana = @Manzana AND Lote = @Lote
ORDER BY FechaCaptura DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            return Ok((EvidenciaDto)null);

                        var foto = (byte[])r["Foto"];
                        return Ok(new EvidenciaDto
                        {
                            Id = Convert.ToInt32(r["Id"]),
                            Manzana = r["Manzana"]?.ToString(),
                            Lote = r["Lote"]?.ToString(),
                            Prototipo = r["Prototipo"] == DBNull.Value ? "" : r["Prototipo"].ToString(),
                            Titulo = r["TituloFoto"]?.ToString(),
                            FotoBase64 = foto == null ? null : Convert.ToBase64String(foto),
                            Extension = r["Extension"] == DBNull.Value ? "jpg" : r["Extension"].ToString(),
                            TamanioKB = r["TamanioKB"] == DBNull.Value ? 0 : Convert.ToDouble(r["TamanioKB"]),
                            Fecha = Convert.ToDateTime(r["FechaCaptura"]),
                            Usuario = r["UsuarioCaptura"] == DBNull.Value ? "Sistema" : r["UsuarioCaptura"].ToString()
                        });
                    }
                }
            }
        }

        /// <summary>GET /api/evidencias?manzana=&amp;lote= · listado del lote (sin binario).</summary>
        [HttpGet, Route("")]
        public IHttpActionResult Listar(string manzana, string lote)
        {
            var lista = new List<EvidenciaDto>();
            using (var conn = Db.Abrir())
            {
                string colTamanio = DetectarColumnaTamanio(conn);
                string sql = $@"
SELECT Id, Manzana, Lote, Prototipo, TituloFoto,
       Extension, [{colTamanio}] AS TamanioKB, FechaCaptura, UsuarioCaptura
FROM EvidenciasFotograficas
WHERE Manzana = @Manzana AND Lote = @Lote
ORDER BY FechaCaptura DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new EvidenciaDto
                            {
                                Id = Convert.ToInt32(r["Id"]),
                                Manzana = r["Manzana"]?.ToString(),
                                Lote = r["Lote"]?.ToString(),
                                Prototipo = r["Prototipo"] == DBNull.Value ? "" : r["Prototipo"].ToString(),
                                Titulo = r["TituloFoto"]?.ToString(),
                                Extension = r["Extension"] == DBNull.Value ? "jpg" : r["Extension"].ToString(),
                                TamanioKB = r["TamanioKB"] == DBNull.Value ? 0 : Convert.ToDouble(r["TamanioKB"]),
                                Fecha = Convert.ToDateTime(r["FechaCaptura"]),
                                Usuario = r["UsuarioCaptura"] == DBNull.Value ? "Sistema" : r["UsuarioCaptura"].ToString()
                            });
                        }
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/evidencias/{id}/foto · binario completo de la evidencia.</summary>
        [HttpGet, Route("{id:int}/foto")]
        public HttpResponseMessage Foto(int id)
        {
            byte[] bytes = null;
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand("SELECT Foto FROM EvidenciasFotograficas WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                bytes = cmd.ExecuteScalar() as byte[];
            }

            if (bytes == null || bytes.Length == 0)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return resp;
        }

        /// <summary>POST /api/evidencias/{id}/eliminar · borra la evidencia. Devuelve true si borró.</summary>
        [HttpPost, Route("{id:int}/eliminar"), RequierePermiso("estimaciones.editar")]
        public IHttpActionResult Eliminar(int id)
        {
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand("DELETE FROM EvidenciasFotograficas WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                return Ok(cmd.ExecuteNonQuery() > 0);
            }
        }

        // ---- helpers (portados de GestorEvidencias) ----

        /// <summary>Prototipo de una casa según manzana/lote ("" si no se encuentra).</summary>
        private static string ObtenerPrototipo(SqlConnection conn, string manzana, string lote)
        {
            try
            {
                using (var cmd = new SqlCommand(
                    "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                    return cmd.ExecuteScalar()?.ToString() ?? "";
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Detecta el nombre real de la columna de tamaño (puede variar con/sin
        /// acentos: TamañoKB, TamanioKB, …). Devuelve el nombre tal cual está en la BD.
        /// </summary>
        private static string DetectarColumnaTamanio(SqlConnection conn)
        {
            try
            {
                using (var cmd = new SqlCommand(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EvidenciasFotograficas'", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var col = reader.GetString(0);
                        if (Normalize(col).Equals("TAMANIOKB", StringComparison.OrdinalIgnoreCase))
                            return col;
                    }
                }
            }
            catch { /* fallback */ }

            var fallbacks = new[] { "TamañoKB", "TamanioKB", "TamanoKB", "Tamanio_KB", "Tamano_KB", "TAMANIOKB" };
            foreach (var f in fallbacks)
            {
                try
                {
                    using (var cmd = new SqlCommand($"SELECT TOP 1 [{f}] FROM EvidenciasFotograficas", conn))
                    {
                        cmd.ExecuteScalar();
                        return f;
                    }
                }
                catch { }
            }

            return "TamanioKB";
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC)
                .Replace('Ñ', 'N').Replace('ñ', 'n').ToUpperInvariant();
        }
    }
}
