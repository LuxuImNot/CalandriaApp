using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Obras (sitios de construcción), cada una con su propia base de datos en el
    /// mismo servidor. Crear una obra aprovisiona la BD automáticamente a partir
    /// de la plantilla de esquema (Configuracion.SqlPlantillaObraRuta).
    /// </summary>
    [RoutePrefix("api/obras")]
    public class ObrasController : ApiController
    {
        /// <summary>Obras a las que el usuario autenticado tiene acceso, con conteos para saber si aún les falta configuración.</summary>
        [HttpGet, Route("")]
        public IHttpActionResult Mias()
        {
            var filas = new List<ObraFila>();
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                @"SELECT o.Id, o.Nombre, o.NombreBD FROM Obras o
                  JOIN UsuarioObras uo ON uo.ObraId = o.Id
                  WHERE uo.Usuario = @usuario AND o.Activa = 1 AND o.ClienteId = @clienteId
                  ORDER BY o.Nombre", conn))
            {
                cmd.Parameters.AddWithValue("@usuario", User.Identity.Name);
                cmd.Parameters.AddWithValue("@clienteId", ClienteActual.Id(User));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        filas.Add(new ObraFila
                        {
                            Id = (int)reader["Id"],
                            Nombre = reader["Nombre"].ToString(),
                            NombreBD = reader["NombreBD"].ToString()
                        });
                }
            }

            var lista = filas.Select(f => new ObraDto
            {
                Id = f.Id,
                Nombre = f.Nombre,
                NodosArbol = ContarFilas(f.NombreBD, "RutaCalandraDestajo") + ContarFilas(f.NombreBD, "RutaTuneraDestajo"),
                Insumos = ContarFilas(f.NombreBD, "InsumosCalandraEXP") + ContarFilas(f.NombreBD, "InsumosTuneraEXP"),
                Proveedores = ContarFilas(f.NombreBD, "PROVEEDORESCALANDRIA")
            }).ToList();

            return Ok(lista);
        }

        private sealed class ObraFila
        {
            public int Id;
            public string Nombre;
            public string NombreBD;
        }

        /// <summary>COUNT(*) de una tabla en la BD de una obra, para el indicador de "configuración pendiente".</summary>
        private static int ContarFilas(string nombreBd, string tabla)
        {
            using (var conn = new SqlConnection(Configuracion.CadenaConexionObra(nombreBd)))
            {
                conn.Open();
                using (var cmd = new SqlCommand($"SELECT COUNT(*) FROM [{tabla}]", conn))
                    return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>Catálogo completo de obras, para la pantalla de asignación en Perfiles.</summary>
        [HttpGet, Route("todas"), RequierePermiso("sistema.perfiles")]
        public IHttpActionResult Todas()
        {
            var lista = new List<ObraDto>();
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                "SELECT Id, Nombre FROM Obras WHERE Activa = 1 AND ClienteId = @clienteId ORDER BY Nombre", conn))
            {
                cmd.Parameters.AddWithValue("@clienteId", ClienteActual.Id(User));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(new ObraDto { Id = (int)reader["Id"], Nombre = reader["Nombre"].ToString() });
                }
            }
            return Ok(lista);
        }

        /// <summary>Obras asignadas a cada usuario, para el checklist de Perfiles.</summary>
        [HttpGet, Route("asignaciones"), RequierePermiso("sistema.perfiles")]
        public IHttpActionResult Asignaciones()
        {
            var porUsuario = new Dictionary<string, AsignacionObrasUsuarioDto>(StringComparer.OrdinalIgnoreCase);
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                @"SELECT u.Nombre AS Usuario, o.Id AS ObraId
                  FROM Usuarios u
                  LEFT JOIN UsuarioObras uo ON uo.Usuario = u.Nombre
                  LEFT JOIN Obras o ON o.Id = uo.ObraId AND o.ClienteId = u.ClienteId
                  WHERE u.ClienteId = @clienteId", conn))
            {
                cmd.Parameters.AddWithValue("@clienteId", ClienteActual.Id(User));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string usuario = reader["Usuario"].ToString();
                        if (!porUsuario.TryGetValue(usuario, out var dto))
                            porUsuario[usuario] = dto = new AsignacionObrasUsuarioDto { Usuario = usuario, ObraIds = new List<int>() };

                        if (reader["ObraId"] != DBNull.Value)
                            dto.ObraIds.Add((int)reader["ObraId"]);
                    }
                }
            }
            return Ok(porUsuario.Values.ToList());
        }

        /// <summary>Reemplaza la lista completa de obras a las que un usuario tiene acceso.</summary>
        [HttpPost, Route("usuario/asignar"), RequierePermiso("sistema.perfiles")]
        public IHttpActionResult AsignarObrasUsuario([FromBody] AsignarObrasRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Usuario))
                return BadRequest("Usuario obligatorio.");

            var obraIds = (req.ObraIds ?? new List<int>()).Distinct().ToList();
            int clienteId = ClienteActual.Id(User);

            using (var conn = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand("SELECT 1 FROM Usuarios WHERE Nombre = @usuario AND ClienteId = @clienteId", conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", req.Usuario.Trim());
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    if (cmd.ExecuteScalar() == null) return NotFound();
                }

                if (obraIds.Count > 0)
                {
                    using (var cmd = new SqlCommand(
                        $"SELECT COUNT(*) FROM Obras WHERE ClienteId = @clienteId AND Id IN ({string.Join(",", obraIds)})", conn))
                    {
                        cmd.Parameters.AddWithValue("@clienteId", clienteId);
                        if ((int)cmd.ExecuteScalar() != obraIds.Count)
                            return BadRequest("Una o más obras no existen o no pertenecen a tu empresa.");
                    }
                }

                using (var cmd = new SqlCommand("DELETE FROM UsuarioObras WHERE Usuario = @usuario", conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", req.Usuario.Trim());
                    cmd.ExecuteNonQuery();
                }
                foreach (var obraId in obraIds)
                {
                    using (var cmd = new SqlCommand(
                        "INSERT INTO UsuarioObras (Usuario, ObraId) VALUES (@usuario, @obraId)", conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", req.Usuario.Trim());
                        cmd.Parameters.AddWithValue("@obraId", obraId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return Ok();
        }

        /// <summary>Aprovisiona una obra nueva: crea la BD, aplica la plantilla de esquema y da acceso al creador.</summary>
        [HttpPost, Route(""), RequierePermiso("sistema.obras")]
        public IHttpActionResult Crear([FromBody] CrearObraRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Nombre))
                return BadRequest("El nombre de la obra es obligatorio.");

            string nombre = req.Nombre.Trim();
            string nombreBd;
            int clienteId = ClienteActual.Id(User);

            using (var master = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand("SELECT 1 FROM Obras WHERE Nombre = @nombre", master))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    if (cmd.ExecuteScalar() != null)
                        return Conflict();
                }
                nombreBd = Services.ObraProvisioning.CrearBaseDeDatos(master, nombre);
            }

            int obraId;
            using (var master = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand(
                    "INSERT INTO Obras (Nombre, NombreBD, ClienteId) OUTPUT INSERTED.Id VALUES (@nombre, @nombreBd, @clienteId)", master))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@nombreBd", nombreBd);
                    cmd.Parameters.AddWithValue("@clienteId", clienteId);
                    obraId = (int)cmd.ExecuteScalar();
                }
                using (var cmd = new SqlCommand(
                    "INSERT INTO UsuarioObras (Usuario, ObraId) VALUES (@usuario, @obraId)", master))
                {
                    cmd.Parameters.AddWithValue("@usuario", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@obraId", obraId);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new CrearObraResponseDto { Id = obraId, NombreBD = nombreBd });
        }

        /// <summary>
        /// Sube el logo de una obra y guarda la paleta de 3 colores que
        /// ExtractorColoresLogo sugiere a partir de él (acento de las pantallas
        /// web de esa obra).
        /// </summary>
        [HttpPost, Route("{id:int}/personalizacion"), RequierePermiso("sistema.obras")]
        public IHttpActionResult Personalizar(int id, [FromBody] PersonalizarObraRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.LogoBase64))
                return BadRequest("El logo no puede estar vacío.");

            byte[] logo = Convert.FromBase64String(req.LogoBase64);
            if (logo.Length == 0)
                return BadRequest("El logo no puede estar vacío.");
            if (logo.Length > 5 * 1024 * 1024)
                return BadRequest("El logo no puede superar los 5 MB.");
            if (!Services.ImagenValidacion.EsImagenValida(logo))
                return BadRequest("El archivo no es una imagen válida (jpg/png/webp).");

            Services.ExtractorColoresLogo.Paleta paleta;
            try
            {
                paleta = Services.ExtractorColoresLogo.Sugerir(logo);
            }
            catch (Exception ex)
            {
                return BadRequest("No se pudo procesar la imagen: " + ex.Message);
            }

            using (var master = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                @"UPDATE Obras SET LogoBytes=@logo, LogoExtension=@ext,
                    ColorPrimario=@p, ColorSecundario=@s, ColorSuave=@sv
                  WHERE Id=@id AND Activa=1 AND ClienteId=@clienteId", master))
            {
                cmd.Parameters.AddWithValue("@clienteId", ClienteActual.Id(User));
                cmd.Parameters.AddWithValue("@logo", logo);
                cmd.Parameters.AddWithValue("@ext", (object)req.Extension ?? "png");
                cmd.Parameters.AddWithValue("@p", paleta.ColorPrimario);
                cmd.Parameters.AddWithValue("@s", paleta.ColorSecundario);
                cmd.Parameters.AddWithValue("@sv", paleta.ColorSuave);
                cmd.Parameters.AddWithValue("@id", id);
                if (cmd.ExecuteNonQuery() == 0) return NotFound();
            }

            return Ok(new TemaObraDto
            {
                ColorPrimario = paleta.ColorPrimario,
                ColorSecundario = paleta.ColorSecundario,
                ColorSuave = paleta.ColorSuave,
                TieneLogo = true
            });
        }

        /// <summary>Paleta + estado del logo de una obra, para que sus pantallas web se acentúen con ella.</summary>
        [HttpGet, Route("{id:int}/tema")]
        public IHttpActionResult Tema(int id)
        {
            using (var master = Db.AbrirMaestra())
            {
                if (!TieneAcceso(master, User.Identity.Name, id)) return Forbidden();

                using (var cmd = new SqlCommand(
                    @"SELECT ColorPrimario, ColorSecundario, ColorSuave, LogoBytes, LogoExtension
                      FROM Obras WHERE Id=@id AND Activa=1", master))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return NotFound();
                        return Ok(new TemaObraDto
                        {
                            ColorPrimario = reader["ColorPrimario"] as string,
                            ColorSecundario = reader["ColorSecundario"] as string,
                            ColorSuave = reader["ColorSuave"] as string,
                            TieneLogo = reader["LogoBytes"] != DBNull.Value,
                            LogoExtension = reader["LogoExtension"] as string
                        });
                    }
                }
            }
        }

        /// <summary>Logo binario de una obra (para mostrar en el selector). 404 si no tiene.</summary>
        [HttpGet, Route("{id:int}/logo")]
        public HttpResponseMessage Logo(int id)
        {
            byte[] bytes;
            using (var master = Db.AbrirMaestra())
            {
                if (!TieneAcceso(master, User.Identity.Name, id))
                    return Request.CreateResponse(System.Net.HttpStatusCode.Forbidden);

                using (var cmd = new SqlCommand("SELECT LogoBytes FROM Obras WHERE Id=@id AND Activa=1", master))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    bytes = cmd.ExecuteScalar() as byte[];
                }
            }

            if (bytes == null || bytes.Length == 0)
                return Request.CreateResponse(System.Net.HttpStatusCode.NotFound);

            var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            return resp;
        }

        private static bool TieneAcceso(SqlConnection master, string usuario, int obraId)
        {
            using (var cmd = new SqlCommand(
                "SELECT 1 FROM UsuarioObras WHERE Usuario=@usuario AND ObraId=@obraId", master))
            {
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@obraId", obraId);
                return cmd.ExecuteScalar() != null;
            }
        }

        private IHttpActionResult Forbidden() => StatusCode(System.Net.HttpStatusCode.Forbidden);

        /// <summary>
        /// Elimina una obra de forma permanente: quita el acceso de todos los
        /// usuarios, borra el registro y tira la BD completa (DROP DATABASE).
        /// No hay vuelta atrás, por eso exige escribir el nombre exacto de la
        /// obra como confirmación (el cliente ya pide checkbox + nombre escrito
        /// antes de llegar aquí; esta es la verificación de servidor, la que
        /// de verdad cuenta).
        /// </summary>
        [HttpPost, Route("{id:int}/eliminar"), RequierePermiso("sistema.obras")]
        public IHttpActionResult Eliminar(int id, [FromBody] EliminarObraRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.NombreConfirmacion))
                return BadRequest("Escribe el nombre de la obra para confirmar.");

            string nombre, nombreBd;
            using (var master = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                "SELECT Nombre, NombreBD FROM Obras WHERE Id = @id AND Activa = 1 AND ClienteId = @clienteId", master))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@clienteId", ClienteActual.Id(User));
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return NotFound();
                    nombre = reader["Nombre"].ToString();
                    nombreBd = reader["NombreBD"].ToString();
                }
            }

            if (!string.Equals(req.NombreConfirmacion.Trim(), nombre, StringComparison.Ordinal))
                return BadRequest("El nombre escrito no coincide con el de la obra.");

            // La obra original (creada antes de este sistema, sobre la BD de
            // producción histórica) no se elimina desde aquí: no tiene sentido
            // borrar la BD que sigue siendo la principal.
            if (string.Equals(nombreBd, "CALANDRIA", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Esta obra no se puede eliminar.");

            using (var master = Db.AbrirMaestra())
            {
                using (var cmd = new SqlCommand("DELETE FROM UsuarioObras WHERE ObraId = @id", master))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand("DELETE FROM Obras WHERE Id = @id", master))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            using (var servidor = Db.AbrirServidor())
            using (var cmd = new SqlCommand(
                $"ALTER DATABASE [{nombreBd}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{nombreBd}]",
                servidor))
            {
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }
    }
}
