using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

            string hashAlmacenado = null;
            string rol = null;

            using (var conn = Db.Abrir())
            {
                using (var cmd = new SqlCommand(
                    "SELECT ClaveHash, Rol FROM Usuarios WHERE Nombre = @usuario", conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hashAlmacenado = reader["ClaveHash"]?.ToString();
                            rol = reader["Rol"]?.ToString();
                        }
                    }
                }

                // Mensaje genérico tanto si el usuario no existe como si la clave es
                // incorrecta (evita enumeración de cuentas), igual que FormLogin.
                if (hashAlmacenado == null)
                    return Unauthorized();

                bool valida = PasswordHasher.Verificar(req.Clave, hashAlmacenado, out bool necesitaRehash);
                if (!valida)
                    return Unauthorized();

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
            }

            string token = TokenService.Generar(usuario, rol, out DateTime expiraUtc);

            return Ok(new LoginResponse
            {
                Token = token,
                Usuario = usuario,
                Rol = rol,
                Permisos = PermisosPorRol(rol),
                ExpiraUtc = expiraUtc
            });
        }

        private static List<string> PermisosPorRol(string rol)
        {
            return rol == "Admin"
                ? new List<string> { "Agregar", "Guardar", "Eliminar", "Ver" }
                : new List<string> { "Ver" };
        }
    }
}
