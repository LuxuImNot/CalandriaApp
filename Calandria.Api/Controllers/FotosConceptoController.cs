using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Fotos adjuntas a un concepto/partida de una casa (Manzana + Lote +
    /// Identificador del nodo). Reproduce el servicio de cliente GestorFotosConcepto.
    /// La tabla FotosConcepto se crea automáticamente la primera vez (idempotente).
    /// La foto viaja en base64 al guardar y como binario al consultar.
    /// </summary>
    [RoutePrefix("api/fotos-concepto")]
    public class FotosConceptoController : ApiController
    {
        /// <summary>POST /api/fotos-concepto · guarda una foto. Devuelve el Id nuevo.</summary>
        [HttpPost, Route("")]
        public IHttpActionResult Guardar([FromBody] GuardarFotoConceptoRequest req)
        {
            if (req == null)
                return BadRequest("Petición vacía.");
            if (string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Manzana y Lote son obligatorios");
            if (string.IsNullOrWhiteSpace(req.Identificador))
                return BadRequest("El identificador del concepto es obligatorio");
            if (string.IsNullOrEmpty(req.FotoBase64))
                return BadRequest("La foto no puede estar vacía");

            byte[] foto = Convert.FromBase64String(req.FotoBase64);
            if (foto.Length == 0)
                return BadRequest("La foto no puede estar vacía");
            if (foto.Length > 10 * 1024 * 1024)
                return BadRequest("La foto no puede superar los 10 MB");

            double tamanioKB = foto.Length / 1024.0;

            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                string sql = @"
INSERT INTO FotosConcepto
    (Manzana, Lote, Identificador, EsConcepto, NombreNodo, Descripcion, Foto, Extension, TamanioKB, UsuarioCaptura)
OUTPUT INSERTED.Id
VALUES
    (@Manzana, @Lote, @Identificador, @EsConcepto, @NombreNodo, @Descripcion, @Foto, @Extension, @TamanioKB, @Usuario)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", req.Manzana);
                    cmd.Parameters.AddWithValue("@Lote", req.Lote);
                    cmd.Parameters.AddWithValue("@Identificador", req.Identificador);
                    cmd.Parameters.AddWithValue("@EsConcepto", req.EsConcepto);
                    cmd.Parameters.AddWithValue("@NombreNodo", (object)req.NombreNodo ?? DBNull.Value);
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
        /// GET /api/fotos-concepto?manzana=&amp;lote=&amp;identificador=&amp;esConcepto=
        /// · metadatos de las fotos de un concepto/partida (sin binario).
        /// </summary>
        [HttpGet, Route("")]
        public IHttpActionResult Listar(string manzana, string lote, string identificador, bool esConcepto)
        {
            var lista = new List<FotoConceptoDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                string sql = @"
SELECT Id, Manzana, Lote, Identificador, EsConcepto, NombreNodo,
       Descripcion, Extension, TamanioKB, FechaCaptura, UsuarioCaptura
FROM FotosConcepto
WHERE Manzana = @Manzana AND Lote = @Lote
  AND Identificador = @Identificador AND EsConcepto = @EsConcepto
ORDER BY FechaCaptura ASC, Id ASC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    cmd.Parameters.AddWithValue("@Identificador", identificador);
                    cmd.Parameters.AddWithValue("@EsConcepto", esConcepto);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new FotoConceptoDto
                            {
                                Id = r.GetInt32(0),
                                Manzana = r.GetString(1),
                                Lote = r.GetString(2),
                                Identificador = r.GetString(3),
                                EsConcepto = r.GetBoolean(4),
                                NombreNodo = r.IsDBNull(5) ? "" : r.GetString(5),
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

        /// <summary>
        /// GET /api/fotos-concepto/contar?manzana=&amp;lote=&amp;identificador=&amp;esConcepto=
        /// · número de fotos del concepto/partida.
        /// </summary>
        [HttpGet, Route("contar")]
        public IHttpActionResult Contar(string manzana, string lote, string identificador, bool esConcepto)
        {
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                string sql = @"
SELECT COUNT(*) FROM FotosConcepto
WHERE Manzana = @Manzana AND Lote = @Lote
  AND Identificador = @Identificador AND EsConcepto = @EsConcepto";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    cmd.Parameters.AddWithValue("@Identificador", identificador);
                    cmd.Parameters.AddWithValue("@EsConcepto", esConcepto);
                    return Ok(Convert.ToInt32(cmd.ExecuteScalar()));
                }
            }
        }

        /// <summary>GET /api/fotos-concepto/{id}/foto · binario completo de la foto.</summary>
        [HttpGet, Route("{id:int}/foto")]
        public HttpResponseMessage Foto(int id)
        {
            byte[] bytes;
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);
                using (var cmd = new SqlCommand("SELECT Foto FROM FotosConcepto WHERE Id = @Id", conn))
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

        /// <summary>POST /api/fotos-concepto/{id}/descripcion · actualiza la descripción.</summary>
        [HttpPost, Route("{id:int}/descripcion")]
        public IHttpActionResult ActualizarDescripcion(int id, [FromBody] ActualizarDescripcionRequest req)
        {
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);
                using (var cmd = new SqlCommand(
                    "UPDATE FotosConcepto SET Descripcion = @Descripcion WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Descripcion", (object)req?.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        /// <summary>POST /api/fotos-concepto/{id}/eliminar · borra la foto. Devuelve true si borró.</summary>
        [HttpPost, Route("{id:int}/eliminar")]
        public IHttpActionResult Eliminar(int id)
        {
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);
                using (var cmd = new SqlCommand("DELETE FROM FotosConcepto WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return Ok(cmd.ExecuteNonQuery() > 0);
                }
            }
        }

        // ---- helpers ----

        /// <summary>Crea la tabla FotosConcepto si aún no existe. Idempotente.</summary>
        private static void EnsureTabla(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FotosConcepto')
BEGIN
    CREATE TABLE FotosConcepto (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(50) NOT NULL,
        Lote NVARCHAR(50) NOT NULL,
        Identificador NVARCHAR(100) NOT NULL,
        EsConcepto BIT NOT NULL DEFAULT(1),
        NombreNodo NVARCHAR(255) NULL,
        Descripcion NVARCHAR(255) NULL,
        Foto VARBINARY(MAX) NOT NULL,
        Extension NVARCHAR(10) NULL,
        TamanioKB FLOAT NULL,
        FechaCaptura DATETIME NOT NULL DEFAULT(GETDATE()),
        UsuarioCaptura NVARCHAR(100) NULL
    );
    CREATE INDEX IX_FotosConcepto_Nodo
        ON FotosConcepto (Manzana, Lote, Identificador, EsConcepto);
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }
    }
}
