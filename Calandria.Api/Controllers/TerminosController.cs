using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Web.Http;
using Calandria.Api.Data;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Aceptación de Términos de Uso / Aviso de Privacidad (Calandria.Api/ui/terminos.html,
    /// tabla TerminosAceptados en la BD maestra — ver Sql/9_APLICAR_TerminosAceptados.sql).
    /// La "versión vigente" es el hash SHA-256 del propio terminos.html: si se reemplaza ese
    /// archivo en el servidor, el hash cambia y todos los usuarios deben volver a aceptar.
    /// </summary>
    [RoutePrefix("api/terminos")]
    public class TerminosController : ApiController
    {
        /// <summary>Si el usuario autenticado ya aceptó la versión vigente de los términos.</summary>
        [HttpGet, Route("estado")]
        public IHttpActionResult Estado()
        {
            string hash = HashActual();
            if (hash == null) return Ok(new { requiereAceptar = false, hash = (string)null });

            bool aceptado;
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                "SELECT 1 FROM TerminosAceptados WHERE Usuario = @usuario AND VersionHash = @hash", conn))
            {
                cmd.Parameters.AddWithValue("@usuario", User.Identity.Name);
                cmd.Parameters.AddWithValue("@hash", hash);
                aceptado = cmd.ExecuteScalar() != null;
            }

            return Ok(new { requiereAceptar = !aceptado, hash });
        }

        /// <summary>Registra la aceptación de la versión vigente para el usuario autenticado.</summary>
        [HttpPost, Route("aceptar")]
        public IHttpActionResult Aceptar()
        {
            string hash = HashActual();
            if (hash == null) return BadRequest("No hay términos vigentes que aceptar.");

            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM TerminosAceptados WHERE Usuario = @usuario AND VersionHash = @hash)
                  INSERT INTO TerminosAceptados (Usuario, VersionHash) VALUES (@usuario, @hash)", conn))
            {
                cmd.Parameters.AddWithValue("@usuario", User.Identity.Name);
                cmd.Parameters.AddWithValue("@hash", hash);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        private static string HashActual()
        {
            string ruta = Path.Combine(Configuracion.UiRuta, "terminos.html");
            if (!File.Exists(ruta)) return null;

            byte[] datos = File.ReadAllBytes(ruta);
            using (var sha = SHA256.Create())
                return System.BitConverter.ToString(sha.ComputeHash(datos)).Replace("-", "").ToLowerInvariant();
        }
    }
}
