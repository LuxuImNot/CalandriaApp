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
    /// Alta de un cliente nuevo (empresa que va a usar el sistema): crea el Cliente, su
    /// primer perfil "Administrador" (con todos los permisos salvo sistema.facturacion,
    /// que es información del proveedor de la plataforma, no del cliente), su primera
    /// obra (aprovisiona la BD, ver ObraProvisioning) y su primer usuario admin.
    ///
    /// A propósito NO usa RequierePermiso: esto es para el operador de la plataforma
    /// (ver RequiereSuperAdminAttribute), nunca para el admin de un cliente.
    /// </summary>
    [RoutePrefix("api/clientes"), RequiereSuperAdmin]
    public class ClientesController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult Listar()
        {
            var lista = new List<ClienteResumenDto>();
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                @"SELECT c.Id, c.Nombre, c.Activo, c.FechaCreacion,
                         (SELECT COUNT(*) FROM Obras o WHERE o.ClienteId = c.Id) AS CantidadObras,
                         (SELECT COUNT(*) FROM Usuarios u WHERE u.ClienteId = c.Id) AS CantidadUsuarios
                  FROM Clientes c ORDER BY c.FechaCreacion DESC", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new ClienteResumenDto
                    {
                        Id = (int)reader["Id"],
                        Nombre = reader["Nombre"].ToString(),
                        Activo = (bool)reader["Activo"],
                        FechaCreacion = (DateTime)reader["FechaCreacion"],
                        CantidadObras = (int)reader["CantidadObras"],
                        CantidadUsuarios = (int)reader["CantidadUsuarios"]
                    });
                }
            }
            return Ok(lista);
        }

        [HttpPost, Route("")]
        public IHttpActionResult Crear([FromBody] CrearClienteRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.NombreCliente))
                return BadRequest("El nombre del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(req.NombreObra))
                return BadRequest("El nombre de la primera obra es obligatorio.");
            if (string.IsNullOrWhiteSpace(req.Usuario) || string.IsNullOrEmpty(req.Clave))
                return BadRequest("Usuario y contraseña del administrador son obligatorios.");
            if (string.IsNullOrEmpty(req.FotoBase64))
                return BadRequest("La foto del administrador es obligatoria.");
            if (req.FechaAlta == default(DateTime))
                return BadRequest("La fecha de ingreso es obligatoria.");

            byte[] foto;
            try { foto = Convert.FromBase64String(req.FotoBase64); }
            catch (FormatException) { return BadRequest("La foto no es válida."); }
            if (foto.Length == 0 || foto.Length > 5 * 1024 * 1024)
                return BadRequest("La foto no puede superar los 5 MB.");
            if (!Services.ImagenValidacion.EsImagenValida(foto))
                return BadRequest("El archivo no es una imagen válida (jpg/png/webp).");

            string nombreCliente = req.NombreCliente.Trim();
            string nombreObra = req.NombreObra.Trim();
            string usuario = req.Usuario.Trim();

            using (var master = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand("SELECT 1 FROM Clientes WHERE Nombre = @nombre", master))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombreCliente);
                    if (cmd.ExecuteScalar() != null)
                        return Conflict();
                }
                using (var cmd = new SqlCommand("SELECT 1 FROM Obras WHERE Nombre = @nombre", master))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombreObra);
                    if (cmd.ExecuteScalar() != null)
                        return Conflict();
                }
                using (var cmd = new SqlCommand("SELECT 1 FROM Usuarios WHERE Nombre = @usuario", master))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    if (cmd.ExecuteScalar() != null)
                        return Conflict();
                }

                int clienteId;
                using (var cmd = new SqlCommand(
                    "INSERT INTO Clientes (Nombre) OUTPUT INSERTED.Id VALUES (@nombre)", master))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombreCliente);
                    clienteId = (int)cmd.ExecuteScalar();
                }

                int perfilId;
                using (var cmd = new SqlCommand(
                    @"INSERT INTO Perfiles (Nombre, Descripcion, EsSistema, ClienteId) OUTPUT INSERTED.Id
                      VALUES ('Administrador', 'Acceso total (creado automáticamente al dar de alta el cliente)', 1, @clienteId)", master))
                {
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    perfilId = (int)cmd.ExecuteScalar();
                }
                foreach (var permiso in PermisosCatalogo.Todos.Where(p => p.Clave != "sistema.facturacion"))
                {
                    using (var cmd = new SqlCommand(
                        "INSERT INTO PerfilPermisos (PerfilId, Permiso) VALUES (@id, @permiso)", master))
                    {
                        cmd.Parameters.AddWithValue("@id", perfilId);
                        cmd.Parameters.AddWithValue("@permiso", permiso.Clave);
                        cmd.ExecuteNonQuery();
                    }
                }

                string nombreBd = Services.ObraProvisioning.CrearBaseDeDatos(master, nombreObra);

                int obraId;
                using (var cmd = new SqlCommand(
                    "INSERT INTO Obras (Nombre, NombreBD, ClienteId) OUTPUT INSERTED.Id VALUES (@nombre, @nombreBd, @clienteId)", master))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombreObra);
                    cmd.Parameters.AddWithValue("@nombreBd", nombreBd);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    obraId = (int)cmd.ExecuteScalar();
                }

                using (var cmd = new SqlCommand(
                    @"INSERT INTO Usuarios (Nombre, ClaveHash, PerfilId, FotoBytes, FotoExtension, FechaAlta, FechaAltaConfirmada, ClienteId)
                      VALUES (@usuario, @hash, @perfilId, @foto, @ext, @fecha, 1, @clienteId)", master))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    cmd.Parameters.AddWithValue("@hash", DynamicSepticSystem.PasswordHasher.Hash(req.Clave));
                    cmd.Parameters.AddWithValue("@perfilId", perfilId);
                    cmd.Parameters.AddWithValue("@foto", foto);
                    cmd.Parameters.AddWithValue("@ext", (object)req.Extension ?? "jpg");
                    cmd.Parameters.AddWithValue("@fecha", req.FechaAlta);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    try { cmd.ExecuteNonQuery(); }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) { return Conflict(); }
                }

                using (var cmd = new SqlCommand(
                    "INSERT INTO UsuarioObras (Usuario, ObraId) VALUES (@usuario, @obraId)", master))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    cmd.Parameters.AddWithValue("@obraId", obraId);
                    cmd.ExecuteNonQuery();
                }

                return Ok(new CrearClienteResponseDto { ClienteId = clienteId, ObraId = obraId, NombreBD = nombreBd });
            }
        }
    }
}
