using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
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
            List<string> permisos;

            using (var conn = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand(
                    @"SELECT u.ClaveHash, u.PerfilId, u.ClienteId, p.Nombre AS PerfilNombre
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
            string token = TokenService.Generar(usuario, perfilNombre, permisos, clienteId, out DateTime expiraUtc);

            return Ok(new LoginResponse
            {
                Token = token,
                Usuario = usuario,
                Rol = perfilNombre,
                Permisos = permisos,
                ExpiraUtc = expiraUtc,
                EsSuperAdmin = Configuracion.SuperAdmins.Contains(usuario)
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
