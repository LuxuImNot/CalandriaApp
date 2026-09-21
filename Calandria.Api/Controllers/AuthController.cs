using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;
using DynamicSepticSystem; // PasswordHasher (archivo enlazado)

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Autenticación. Reproduce la lógica de FormLogin (verificación PBKDF2 con
    /// migración perezosa desde SHA-256) pero emite un JWT en vez de mantener
    /// estado en el cliente.
    /// </summary>
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        [HttpPost, Route("login"), AllowAnonymous]
        public IHttpActionResult Login([FromBody] LoginRequest req)
        {
            if (req == null ||
                string.IsNullOrWhiteSpace(req.Usuario) ||
                string.IsNullOrEmpty(req.Clave))
            {
                return BadRequest("Ingresa usuario y contraseña.");
            }

            string usuario = req.Usuario.Trim();

            if (LoginThrottle.Bloqueado(usuario))
                return StatusCode((System.Net.HttpStatusCode)429);

            string hashAlmacenado = null;
            string perfilNombre = null;
            int? perfilId = null;
            int clienteId = 0;
            bool cambioClaveRequerido = false;
            List<string> permisos;

            using (var conn = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand(
                    @"SELECT u.ClaveHash, u.PerfilId, u.ClienteId, u.CambioClaveRequerido,
                             p.Nombre AS PerfilNombre
                      FROM Usuarios u LEFT JOIN Perfiles p ON p.Id = u.PerfilId
                      WHERE u.Nombre = @usuario", conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hashAlmacenado = reader["ClaveHash"]?.ToString();
                            perfilId = reader["PerfilId"] as int?;
                            clienteId = (int)reader["ClienteId"];
                            perfilNombre = reader["PerfilNombre"]?.ToString();
                            cambioClaveRequerido = reader["CambioClaveRequerido"] != DBNull.Value &&
                                                   (bool)reader["CambioClaveRequerido"];
                        }
                    }
                }

                // Mensaje genérico tanto si el usuario no existe como si la clave es
                // incorrecta (evita enumeración de cuentas), igual que FormLogin.
                if (hashAlmacenado == null)
                {
                    LoginThrottle.RegistrarFallo(usuario);
                    return Unauthorized();
                }

                bool valida = PasswordHasher.Verificar(req.Clave, hashAlmacenado, out bool necesitaRehash);
                if (!valida)
                {
                    LoginThrottle.RegistrarFallo(usuario);
                    return Unauthorized();
                }

                // Migración perezosa del hash heredado a PBKDF2.
                if (necesitaRehash)
                {
                    try
                    {
                        using (var upd = new SqlCommand(
                            "UPDATE Usuarios SET ClaveHash = @h WHERE Nombre = @u", conn))
                        {
                            upd.Parameters.AddWithValue("@h", PasswordHasher.Hash(req.Clave));
                            upd.Parameters.AddWithValue("@u", usuario);
                            upd.ExecuteNonQuery();
                        }
                    }
                    catch
                    {
                        // No impide el login.
                    }
                }

                permisos = new List<string>();
                if (perfilId.HasValue)
                {
                    using (var cmd = new SqlCommand(
                        "SELECT Permiso FROM PerfilPermisos WHERE PerfilId = @perfilId", conn))
                    {
                        cmd.Parameters.AddWithValue("@perfilId", perfilId.Value);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                permisos.Add(reader.GetString(0));
                        }
                    }
                }
            }

            LoginThrottle.RegistrarExito(usuario);
            string token = TokenService.Generar(usuario, perfilNombre, permisos, clienteId,
                                                out DateTime expiraUtc, cambioClaveRequerido);

            return Ok(new LoginResponse
            {
                Token = token,
                Usuario = usuario,
                Rol = perfilNombre,
                Permisos = permisos,
                ExpiraUtc = expiraUtc,
                EsSuperAdmin = Configuracion.SuperAdmins.Contains(usuario),
                CambioClaveRequerido = cambioClaveRequerido
            });
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado. Exige la actual: un token
        /// robado no alcanza para quedarse con la cuenta. Es la única ruta que
        /// acepta un token marcado con "cambioClave" (ver JwtMessageHandler), así
        /// que también sirve para el cambio obligatorio del primer ingreso.
        /// Devuelve un token nuevo, ya sin la marca.
        /// </summary>
        [HttpPost, Route("cambiar-clave")]
        public IHttpActionResult CambiarClave([FromBody] CambiarClaveRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.ClaveActual))
                return BadRequest("Ingresa tu contraseña actual.");

            string rechazo = PoliticaClave.Rechazo(req.ClaveNueva);
            if (rechazo != null) return BadRequest(rechazo);

            string usuario = User.Identity.Name;
            if (string.IsNullOrWhiteSpace(usuario)) return Unauthorized();

            // Una contraseña "nueva" igual a la actual deja la cuenta con la
            // temporal que dictó el administrador, que es justo lo que se quiere evitar.
            if (req.ClaveNueva == req.ClaveActual)
                return BadRequest("La contraseña nueva debe ser distinta de la actual.");

            // El throttle del login cubre también esto: si no, la clave actual se
            // vuelve un oráculo para adivinar a fuerza bruta con un token válido.
            if (LoginThrottle.Bloqueado(usuario))
                return StatusCode((System.Net.HttpStatusCode)429);

            using (var conn = Db.AbrirMaestra())
            {
                string hashAlmacenado;
                using (var cmd = new SqlCommand(
                    "SELECT ClaveHash FROM Usuarios WHERE Nombre = @u", conn))
                {
                    cmd.Parameters.AddWithValue("@u", usuario);
                    hashAlmacenado = cmd.ExecuteScalar() as string;
                }
                if (hashAlmacenado == null) return Unauthorized();

                if (!PasswordHasher.Verificar(req.ClaveActual, hashAlmacenado, out _))
                {
                    LoginThrottle.RegistrarFallo(usuario);
                    return BadRequest("La contraseña actual no es correcta.");
                }
                LoginThrottle.RegistrarExito(usuario);

                using (var upd = new SqlCommand(
                    @"UPDATE Usuarios SET ClaveHash = @h, CambioClaveRequerido = 0
                      WHERE Nombre = @u", conn))
                {
                    upd.Parameters.AddWithValue("@h", PasswordHasher.Hash(req.ClaveNueva));
                    upd.Parameters.AddWithValue("@u", usuario);
                    upd.ExecuteNonQuery();
                }
            }

            // El token viejo puede traer el claim "cambioClave", que lo deja
            // inservible para todo lo demás: hay que reemplazarlo aquí mismo o el
            // usuario queda encerrado hasta volver a iniciar sesión.
            var permisos = ((ClaimsPrincipal)User).FindAll("perm").Select(c => c.Value).ToList();
            string rol = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.Role)?.Value;
            string token = TokenService.Generar(
                usuario, string.IsNullOrEmpty(rol) ? null : rol, permisos,
                ClienteActual.Id(User), out DateTime expiraUtc);

            return Ok(new LoginResponse
            {
                Token = token,
                Usuario = usuario,
                Rol = rol,
                Permisos = permisos,
                ExpiraUtc = expiraUtc,
                EsSuperAdmin = Configuracion.SuperAdmins.Contains(usuario),
                CambioClaveRequerido = false
            });
        }

        /// <summary>Desglose del perfil activo (rail del panel web): perfil, alta de la cuenta y permisos con etiqueta.</summary>
        [HttpGet, Route("mi-perfil")]
        public IHttpActionResult MiPerfil()
        {
            string perfilNombre = null;
            int? perfilId = null;
            DateTime fechaAlta;
            bool tieneFoto;

            string fotoExtension;

            using (var conn = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand(
                    @"SELECT u.PerfilId, p.Nombre AS PerfilNombre, u.FechaAlta, u.FotoBytes, u.FotoExtension
                      FROM Usuarios u LEFT JOIN Perfiles p ON p.Id = u.PerfilId
                      WHERE u.Nombre = @usuario", conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", User.Identity.Name);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return NotFound();
                        perfilId = reader["PerfilId"] as int?;
                        perfilNombre = reader["PerfilNombre"] as string;
                        fechaAlta = (DateTime)reader["FechaAlta"];
                        tieneFoto = reader["FotoBytes"] != DBNull.Value;
                        fotoExtension = reader["FotoExtension"] as string;
                    }
                }

                var permisos = new List<string>();
                if (perfilId.HasValue)
                {
                    using (var cmd = new SqlCommand(
                        "SELECT Permiso FROM PerfilPermisos WHERE PerfilId = @perfilId", conn))
                    {
                        cmd.Parameters.AddWithValue("@perfilId", perfilId.Value);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                permisos.Add(reader.GetString(0));
                        }
                    }
                }

                return Ok(new MiPerfilDto
                {
                    Usuario = User.Identity.Name,
                    Perfil = perfilNombre,
                    FechaAlta = fechaAlta,
                    TieneFoto = tieneFoto,
                    FotoExtension = fotoExtension,
                    Permisos = PermisosCatalogo.Todos
                        .Where(p => permisos.Contains(p.Clave))
                        .Select(p => new PermisoDto { Clave = p.Clave, Modulo = p.Modulo, Etiqueta = p.Etiqueta })
                        .ToList(),
                    EsSuperAdmin = Configuracion.SuperAdmins.Contains(User.Identity.Name)
                });
            }
        }

        /// <summary>Foto del usuario autenticado (binaria). 404 si no tiene.</summary>
        [HttpGet, Route("mi-perfil/foto")]
        public HttpResponseMessage MiFoto()
        {
            byte[] bytes;
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand("SELECT FotoBytes FROM Usuarios WHERE Nombre = @usuario", conn))
            {
                cmd.Parameters.AddWithValue("@usuario", User.Identity.Name);
                bytes = cmd.ExecuteScalar() as byte[];
            }

            if (bytes == null || bytes.Length == 0)
                return Request.CreateResponse(System.Net.HttpStatusCode.NotFound);

            var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            return resp;
        }
    }
}
