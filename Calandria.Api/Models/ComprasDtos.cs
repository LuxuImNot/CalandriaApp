namespace Calandria.Api.Models
{
    /// <summary>
    /// Proveedor de la tabla unificada PROVEEDORESCALANDRIA. La lista completa se
    /// sirve de una vez y el cliente filtra/ordena localmente (catálogo pequeño).
    /// </summary>
    public sealed class ProveedorDto
    {
        public string ClaveUnica { get; set; }
        public string Nombre { get; set; }
        public string Rfc { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
    }

    /// <summary>Alta de proveedor (FormAgregarProveedor).</summary>
    public sealed class CrearProveedorRequest
    {
        public string ClaveUnica { get; set; }
        public string Nombre { get; set; }
        public string Rfc { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
    }
}
