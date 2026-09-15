using System.Collections.Generic;

namespace Calandria.Api.Models
{
    public sealed class PermisoDto
    {
        public string Clave { get; set; }
        public string Modulo { get; set; }
        public string Etiqueta { get; set; }
    }

    public sealed class PerfilResumenDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsSistema { get; set; }
        public int CantidadUsuarios { get; set; }
    }

    public sealed class PerfilDetalleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsSistema { get; set; }
        public List<string> Permisos { get; set; }
    }

    public sealed class GuardarPerfilRequest
    {
        public int? Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public List<string> Permisos { get; set; }
    }

    public sealed class UsuarioPerfilDto
    {
        public string Nombre { get; set; }
        public int? PerfilId { get; set; }
        public string PerfilNombre { get; set; }
        public string FotoBase64 { get; set; }
        public string FotoExtension { get; set; }
        public System.DateTime FechaAlta { get; set; }
        public bool FechaAltaConfirmada { get; set; }
    }

    public sealed class AsignarPerfilRequest
    {
        public string Usuario { get; set; }
        public int PerfilId { get; set; }
    }

    /// <summary>Desglose del perfil activo, para el modal que abre el avatar del rail.</summary>
    public sealed class MiPerfilDto
    {
        public string Usuario { get; set; }
        public string Perfil { get; set; }
        public System.DateTime FechaAlta { get; set; }
        public bool TieneFoto { get; set; }
        public string FotoExtension { get; set; }
        public List<PermisoDto> Permisos { get; set; }
        public bool EsSuperAdmin { get; set; }
    }

    /// <summary>Alta de usuario desde el gestor de perfiles: foto y fecha de ingreso son obligatorias.</summary>
    public sealed class CrearUsuarioRequest
    {
        public string Usuario { get; set; }
        public string Clave { get; set; }
        public int PerfilId { get; set; }
        public string FotoBase64 { get; set; }
        public string Extension { get; set; }
        public System.DateTime FechaAlta { get; set; }
    }

    public sealed class SubirFotoUsuarioRequest
    {
        public string FotoBase64 { get; set; }
        public string Extension { get; set; }
    }

    public sealed class ConfirmarFechaAltaRequest
    {
        public System.DateTime FechaAlta { get; set; }
    }
}
