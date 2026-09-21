using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Constructor de perfiles y permisos: el admin crea/edita perfiles (roles)
    /// eligiendo permisos del catálogo fijo (PermisosCatalogo) y asigna perfiles
    /// a los usuarios existentes. Reemplaza el criterio anterior de Usuarios.Rol
    /// ("Admin" / cualquier otra cosa) por perfiles configurables.
    /// </summary>
    [RoutePrefix("api/perfiles"), RequierePermiso("sistema.perfiles")]
    public class PerfilesController : ApiController
    {
        [HttpGet, Route("catalogo-permisos")]
        public IHttpActionResult CatalogoPermisos()
        {
            return Ok(PermisosCatalogo.Todos.Select(p => new PermisoDto
            {
                Clave = p.Clave,
                Modulo = p.Modulo,
                Etiqueta = p.Etiqueta
            }).ToList());
        }

        [HttpGet, Route("")]
        public IHttpActionResult Listar()
        {
            var lista = new List<PerfilResumenDto>();
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                @"SELECT p.Id, p.Nombre, p.Descripcion, p.EsSistema,
                         (SELECT COUNT(*) FROM Usuarios u WHERE u.PerfilId = p.Id) AS CantidadUsuarios
                  FROM Perfiles p WHERE p.ClienteId = @clienteId ORDER BY p.Nombre", conn))
            {
                cmd.Parameters.AddWithValue("@clienteId", ClienteActual.Id(User));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new PerfilResumenDto
                        {
                            Id = (int)reader["Id"],
                            Nombre = reader["Nombre"].ToString(),
                            Descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString(),
                            EsSistema = (bool)reader["EsSistema"],
                            CantidadUsuarios = (int)reader["CantidadUsuarios"]
                        });
                    }
                }
            }
            return Ok(lista);
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Detalle(int id)
        {
            int clienteId = ClienteActual.Id(User);
            using (var conn = Db.AbrirMaestra())
            {
                PerfilDetalleDto dto = null;
                using (var cmd = new SqlCommand(
                    "SELECT Id, Nombre, Descripcion, EsSistema FROM Perfiles WHERE Id = @id AND ClienteId = @clienteId", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dto = new PerfilDetalleDto
                            {
                                Id = (int)reader["Id"],
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString(),
                                EsSistema = (bool)reader["EsSistema"],
                                Permisos = new List<string>()
                            };
                        }
                    }
                }
                if (dto == null) return NotFound();

                using (var cmd = new SqlCommand(
                    "SELECT Permiso FROM PerfilPermisos WHERE PerfilId = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            dto.Permisos.Add(reader.GetString(0));
                    }
                }
                return Ok(dto);
            }
        }

        /// <summary>POST /api/perfiles · crea (Id nulo) o actualiza (Id presente) un perfil.</summary>
        [HttpPost, Route("")]
        public IHttpActionResult Guardar([FromBody] GuardarPerfilRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Nombre))
                return BadRequest("El nombre del perfil es obligatorio.");

            string nombre = req.Nombre.Trim();
            string descripcion = string.IsNullOrWhiteSpace(req.Descripcion) ? null : req.Descripcion.Trim();
            var permisos = (req.Permisos ?? new List<string>()).Distinct().ToList();

            var invalidos = permisos.Where(p => !PermisosCatalogo.Existe(p)).ToList();
            if (invalidos.Count > 0)
                return BadRequest("Permiso desconocido: " + string.Join(", ", invalidos));

            int clienteId = ClienteActual.Id(User);

            using (var conn = Db.AbrirMaestra())
            {
                int perfilId;
                try
                {
                    if (req.Id.HasValue)
                    {
                        perfilId = req.Id.Value;
                        using (var cmd = new SqlCommand(
                            "UPDATE Perfiles SET Nombre = @nombre, Descripcion = @descripcion WHERE Id = @id AND ClienteId = @clienteId", conn))
                        {
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@descripcion", (object)descripcion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@id", perfilId);
                            cmd.Parameters.AddWithValue("@clienteId", clienteId);
                            if (cmd.ExecuteNonQuery() == 0) return NotFound();
                        }
                    }
                    else
                    {
                        using (var cmd = new SqlCommand(
                            @"INSERT INTO Perfiles (Nombre, Descripcion, ClienteId) OUTPUT INSERTED.Id
                              VALUES (@nombre, @descripcion, @clienteId)", conn))
                        {
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@descripcion", (object)descripcion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@clienteId", clienteId);
                            perfilId = (int)cmd.ExecuteScalar();
                        }
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    return Conflict();
                }

                using (var cmd = new SqlCommand("DELETE FROM PerfilPermisos WHERE PerfilId = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", perfilId);
                    cmd.ExecuteNonQuery();
                }
                foreach (var permiso in permisos)
                {
                    using (var cmd = new SqlCommand(
                        "INSERT INTO PerfilPermisos (PerfilId, Permiso) VALUES (@id, @permiso)", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", perfilId);
                        cmd.Parameters.AddWithValue("@permiso", permiso);
                        cmd.ExecuteNonQuery();
                    }
                }

                return Ok(new { id = perfilId });
            }
        }

        [HttpPost, Route("{id:int}/eliminar")]
        public IHttpActionResult Eliminar(int id)
        {
            int clienteId = ClienteActual.Id(User);
            using (var conn = Db.AbrirMaestra())
            {
                bool esSistema;
                using (var cmd = new SqlCommand("SELECT EsSistema FROM Perfiles WHERE Id = @id AND ClienteId = @clienteId", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    var valor = cmd.ExecuteScalar();
                    if (valor == null) return NotFound();
                    esSistema = (bool)valor;
                }
                if (esSistema)
                    return BadRequest("No se puede eliminar un perfil de sistema.");

                int usuariosAsignados;
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE PerfilId = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    usuariosAsignados = (int)cmd.ExecuteScalar();
                }
                if (usuariosAsignados > 0)
                    return BadRequest("Hay " + usuariosAsignados + " usuario(s) con este perfil asignado; reasígnalos antes de eliminarlo.");

                using (var cmd = new SqlCommand("DELETE FROM Perfiles WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        [HttpGet, Route("usuarios")]
        public IHttpActionResult Usuarios()
        {
            var lista = new List<UsuarioPerfilDto>();
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                @"SELECT u.Nombre, u.PerfilId, p.Nombre AS PerfilNombre,
                         u.FotoBytes, u.FotoExtension, u.FechaAlta, u.FechaAltaConfirmada
                  FROM Usuarios u LEFT JOIN Perfiles p ON p.Id = u.PerfilId
                  WHERE u.ClienteId = @clienteId
                  ORDER BY u.Nombre", conn))
            {
                cmd.Parameters.AddWithValue("@clienteId", ClienteActual.Id(User));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var foto = reader["FotoBytes"] as byte[];
                        lista.Add(new UsuarioPerfilDto
                        {
                            Nombre = reader["Nombre"].ToString(),
                            PerfilId = reader["PerfilId"] as int?,
                            PerfilNombre = reader["PerfilNombre"] == DBNull.Value ? null : reader["PerfilNombre"].ToString(),
                            FotoBase64 = foto != null && foto.Length > 0 ? Convert.ToBase64String(foto) : null,
                            FotoExtension = reader["FotoExtension"] as string,
                            FechaAlta = (DateTime)reader["FechaAlta"],
                            FechaAltaConfirmada = (bool)reader["FechaAltaConfirmada"]
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>Alta de usuario: foto y fecha de ingreso son obligatorias (a diferencia de los ya existentes, que pueden no tenerlas todavía).</summary>
        [HttpPost, Route("usuarios/crear")]
        public IHttpActionResult CrearUsuario([FromBody] CrearUsuarioRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Usuario))
                return BadRequest("Usuario y contraseña son obligatorios.");

            string rechazoClave = PoliticaClave.Rechazo(req.Clave);
            if (rechazoClave != null) return BadRequest(rechazoClave);
            if (string.IsNullOrEmpty(req.FotoBase64))
                return BadRequest("La foto es obligatoria.");
            if (req.FechaAlta == default(DateTime))
                return BadRequest("La fecha de ingreso es obligatoria.");

            byte[] foto;
            try { foto = Convert.FromBase64String(req.FotoBase64); }
            catch (FormatException) { return BadRequest("La foto no es válida."); }
            if (foto.Length == 0 || foto.Length > 5 * 1024 * 1024)
                return BadRequest("La foto no puede superar los 5 MB.");
            if (!Services.ImagenValidacion.EsImagenValida(foto))
                return BadRequest("El archivo no es una imagen válida (jpg/png/webp).");

            string usuario = req.Usuario.Trim();
            int clienteId = ClienteActual.Id(User);

            using (var conn = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand("SELECT 1 FROM Perfiles WHERE Id = @perfilId AND ClienteId = @clienteId", conn))
                {
                    cmd.Parameters.AddWithValue("@perfilId", req.PerfilId);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    if (cmd.ExecuteScalar() == null) return BadRequest("El perfil no existe.");
                }

                try
                {
                    using (var cmd = new SqlCommand(
                        @"INSERT INTO Usuarios (Nombre, ClaveHash, PerfilId, FotoBytes, FotoExtension, FechaAlta, FechaAltaConfirmada, ClienteId)
                          VALUES (@usuario, @hash, @perfilId, @foto, @ext, @fecha, 1, @clienteId)", conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuario);
                        cmd.Parameters.AddWithValue("@hash", DynamicSepticSystem.PasswordHasher.Hash(req.Clave));
                        cmd.Parameters.AddWithValue("@perfilId", req.PerfilId);
                        cmd.Parameters.AddWithValue("@foto", foto);
                        cmd.Parameters.AddWithValue("@ext", (object)req.Extension ?? "jpg");
                        cmd.Parameters.AddWithValue("@fecha", req.FechaAlta);
                        cmd.Parameters.AddWithValue("@clienteId", clienteId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    return Conflict();
                }
            }
            return Ok();
        }

        /// <summary>
        /// Restablece la contraseña de un usuario cuando la olvidó. El admin dicta
        /// una temporal y la cuenta queda marcada: entra, pero no opera hasta
        /// cambiarla (JwtMessageHandler solo le deja api/auth/cambiar-clave).
        /// El admin nunca ve ni recupera la contraseña anterior: no existe en claro.
        /// </summary>
        [HttpPost, Route("usuarios/{usuario}/restablecer-clave")]
        public IHttpActionResult RestablecerClave(string usuario, [FromBody] RestablecerClaveRequest req)
        {
            if (req == null) return BadRequest("Falta la contraseña temporal.");

            string rechazo = PoliticaClave.Rechazo(req.ClaveTemporal);
            if (rechazo != null) return BadRequest(rechazo);

            string destino = (usuario ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(destino)) return BadRequest("Falta el usuario.");

            // Un admin de cliente no puede restablecerle la contraseña a un
            // superadministrador: sería escalar privilegios desde la pantalla de
            // Perfiles. Solo otro superadministrador puede.
            bool destinoEsSuper = Configuracion.SuperAdmins.Contains(destino);
            bool quienPideEsSuper = Configuracion.SuperAdmins.Contains(User.Identity.Name);
            if (destinoEsSuper && !quienPideEsSuper)
                return StatusCode(System.Net.HttpStatusCode.Forbidden);

            int clienteId = ClienteActual.Id(User);

            using (var conn = Db.AbrirMaestra())
            {
                // El filtro por ClienteId es lo que impide restablecerle la clave a
                // un usuario de otro cliente hospedado en el mismo servidor.
                using (var cmd = new SqlCommand(
                    @"UPDATE Usuarios
                      SET ClaveHash = @hash, CambioClaveRequerido = 1
                      WHERE Nombre = @usuario AND ClienteId = @clienteId", conn))
                {
                    cmd.Parameters.AddWithValue("@hash", DynamicSepticSystem.PasswordHasher.Hash(req.ClaveTemporal));
                    cmd.Parameters.AddWithValue("@usuario", destino);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    if (cmd.ExecuteNonQuery() == 0) return NotFound();
                }
            }

            return Ok();
        }

        /// <summary>Sube o reemplaza la foto de un usuario existente.</summary>
        [HttpPost, Route("usuarios/{usuario}/foto")]
        public IHttpActionResult SubirFotoUsuario(string usuario, [FromBody] SubirFotoUsuarioRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.FotoBase64))
                return BadRequest("La foto no puede estar vacía.");

            byte[] foto;
            try { foto = Convert.FromBase64String(req.FotoBase64); }
            catch (FormatException) { return BadRequest("La foto no es válida."); }
            if (foto.Length == 0 || foto.Length > 5 * 1024 * 1024)
                return BadRequest("La foto no puede superar los 5 MB.");
            if (!Services.ImagenValidacion.EsImagenValida(foto))
                return BadRequest("El archivo no es una imagen válida (jpg/png/webp).");

            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                "UPDATE Usuarios SET FotoBytes = @foto, FotoExtension = @ext WHERE Nombre = @usuario AND ClienteId = @clienteId", conn))
            {
                cmd.Parameters.AddWithValue("@foto", foto);
                cmd.Parameters.AddWithValue("@ext", (object)req.Extension ?? "jpg");
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@clienteId", ClienteActual.Id(User));
                if (cmd.ExecuteNonQuery() == 0) return NotFound();
            }
            return Ok();
        }

        /// <summary>Confirma/corrige la fecha de ingreso de un usuario que aún tiene la de respaldo (script de migración).</summary>
        [HttpPost, Route("usuarios/{usuario}/fecha-alta")]
        public IHttpActionResult ConfirmarFechaAlta(string usuario, [FromBody] ConfirmarFechaAltaRequest req)
        {
            if (req == null || req.FechaAlta == default(DateTime))
                return BadRequest("La fecha de ingreso es obligatoria.");

            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                "UPDATE Usuarios SET FechaAlta = @fecha, FechaAltaConfirmada = 1 WHERE Nombre = @usuario AND ClienteId = @clienteId", conn))
            {
                cmd.Parameters.AddWithValue("@fecha", req.FechaAlta);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@clienteId", ClienteActual.Id(User));
                if (cmd.ExecuteNonQuery() == 0) return NotFound();
            }
            return Ok();
        }

        [HttpPost, Route("usuarios/asignar")]
        public IHttpActionResult AsignarPerfil([FromBody] AsignarPerfilRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Usuario))
                return BadRequest("Usuario obligatorio.");

            int clienteId = ClienteActual.Id(User);

            using (var conn = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand("SELECT 1 FROM Perfiles WHERE Id = @perfilId AND ClienteId = @clienteId", conn))
                {
                    cmd.Parameters.AddWithValue("@perfilId", req.PerfilId);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    if (cmd.ExecuteScalar() == null) return BadRequest("El perfil no existe.");
                }

                using (var cmd = new SqlCommand(
                    "UPDATE Usuarios SET PerfilId = @perfilId WHERE Nombre = @usuario AND ClienteId = @clienteId", conn))
                {
                    cmd.Parameters.AddWithValue("@perfilId", req.PerfilId);
                    cmd.Parameters.AddWithValue("@usuario", req.Usuario.Trim());
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    if (cmd.ExecuteNonQuery() == 0) return NotFound();
                }
            }
            return Ok();
        }
    }
}
