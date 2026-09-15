using System;
using System.Collections.Generic;
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
    /// Evidencia fotográfica de un destajo (Nivel 1 de RutaCalandraDestajo/
    /// RutaTuneraDestajo), identificada por Manzana+Lote+Ruta+NodoId — igual que
    /// ActivacionTareasRuta. Nueva (no existía en el escritorio clásico ni en
    /// destajos.html); reproduce el patrón de FotosConceptoController.
    /// La tabla EvidenciasDestajo se crea automáticamente la primera vez (idempotente).
    /// La foto viaja en base64 al guardar y como binario al consultar.
    /// </summary>
    [RoutePrefix("api/evidencias-destajo"), RequierePermiso("destajos.ver")]
    public class EvidenciasDestajoController : ApiController
    {
        /// <summary>POST /api/evidencias-destajo · guarda una evidencia. Devuelve el Id nuevo.</summary>
        [HttpPost, Route(""), RequierePermiso("destajos.editar")]
        public IHttpActionResult Guardar([FromBody] GuardarEvidenciaDestajoRequest req)
        {
            if (req == null)
                return BadRequest("Petición vacía.");
            if (string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Manzana y Lote son obligatorios");
            if (string.IsNullOrWhiteSpace(req.Ruta))
                return BadRequest("La ruta es obligatoria");
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
                EnsureTabla(conn);

                string sql = @"
INSERT INTO EvidenciasDestajo
    (Manzana, Lote, Ruta, NodoId, NombreDestajo, Descripcion, Foto, Extension, TamanioKB, UsuarioCaptura)
OUTPUT INSERTED.Id
VALUES
    (@Manzana, @Lote, @Ruta, @NodoId, @NombreDestajo, @Descripcion, @Foto, @Extension, @TamanioKB, @Usuario)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", req.Manzana);
                    cmd.Parameters.AddWithValue("@Lote", req.Lote);
                    cmd.Parameters.AddWithValue("@Ruta", req.Ruta);
                    cmd.Parameters.AddWithValue("@NodoId", req.NodoId);
                    cmd.Parameters.AddWithValue("@NombreDestajo", (object)req.NombreDestajo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", (object)req.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Foto", foto);
                    cmd.Parameters.AddWithValue("@Extension", req.Extension ?? "jpg");
                    cmd.Parameters.AddWithValue("@TamanioKB", tamanioKB);
                    cmd.Parameters.AddWithValue("@Usuario", req.Usuario ?? "Sistema");
                    return Ok(Convert.ToInt32(cmd.ExecuteScalar()));
                }
            }
        }

        /// <summary>
        /// GET /api/evidencias-destajo?manzana=&amp;lote=&amp;ruta=&amp;nodoId=
        /// · metadatos de las evidencias de un destajo (sin binario).
        /// </summary>
        [HttpGet, Route("")]
        public IHttpActionResult Listar(string manzana, string lote, string ruta, int nodoId)
        {
            var lista = new List<EvidenciaDestajoDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                string sql = @"
SELECT Id, Manzana, Lote, Ruta, NodoId, NombreDestajo,
       Descripcion, Extension, TamanioKB, FechaCaptura, UsuarioCaptura
FROM EvidenciasDestajo
WHERE Manzana = @Manzana AND Lote = @Lote AND Ruta = @Ruta AND NodoId = @NodoId
ORDER BY FechaCaptura ASC, Id ASC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    cmd.Parameters.AddWithValue("@Ruta", ruta);
                    cmd.Parameters.AddWithValue("@NodoId", nodoId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new EvidenciaDestajoDto
                            {
                                Id = r.GetInt32(0),
                                Manzana = r.GetString(1),
                                Lote = r.GetString(2),
                                Ruta = r.GetString(3),
                                NodoId = r.GetInt32(4),
                                NombreDestajo = r.IsDBNull(5) ? "" : r.GetString(5),
                                Descripcion = r.IsDBNull(6) ? "" : r.GetString(6),
                                Extension = r.IsDBNull(7) ? "jpg" : r.GetString(7),
                                TamanioKB = r.IsDBNull(8) ? 0 : r.GetDouble(8),
                                Fecha = r.GetDateTime(9),
                                Usuario = r.IsDBNull(10) ? "Sistema" : r.GetString(10)
                            });
                        }
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/evidencias-destajo/{id}/foto · binario completo de la evidencia.</summary>
        [HttpGet, Route("{id:int}/foto")]
        public HttpResponseMessage Foto(int id)
        {
            byte[] bytes;
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);
                using (var cmd = new SqlCommand("SELECT Foto FROM EvidenciasDestajo WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    bytes = cmd.ExecuteScalar() as byte[];
                }
            }

            if (bytes == null || bytes.Length == 0)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return resp;
        }

        /// <summary>POST /api/evidencias-destajo/{id}/eliminar · [ADMIN] borra la evidencia.</summary>
        [HttpPost, Route("{id:int}/eliminar"), RequierePermiso("destajos.editar")]
        public IHttpActionResult Eliminar(int id)
        {
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);
                using (var cmd = new SqlCommand("DELETE FROM EvidenciasDestajo WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return Ok(cmd.ExecuteNonQuery() > 0);
                }
            }
        }

        // ---- helpers ----

        /// <summary>Crea la tabla EvidenciasDestajo si aún no existe. Idempotente.</summary>
        private static void EnsureTabla(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EvidenciasDestajo')
BEGIN
    CREATE TABLE EvidenciasDestajo (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(50) NOT NULL,
        Lote NVARCHAR(50) NOT NULL,
        Ruta NVARCHAR(50) NOT NULL,
        NodoId INT NOT NULL,
        NombreDestajo NVARCHAR(255) NULL,
        Descripcion NVARCHAR(255) NULL,
        Foto VARBINARY(MAX) NOT NULL,
        Extension NVARCHAR(10) NULL,
        TamanioKB FLOAT NULL,
        FechaCaptura DATETIME NOT NULL DEFAULT(GETDATE()),
        UsuarioCaptura NVARCHAR(100) NULL
    );
    CREATE INDEX IX_EvidenciasDestajo_Nodo
        ON EvidenciasDestajo (Manzana, Lote, Ruta, NodoId);
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }
    }
}
