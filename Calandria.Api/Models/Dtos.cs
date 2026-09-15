using System;
using System.Collections.Generic;

namespace Calandria.Api.Models
{
    public sealed class LoginRequest
    {
        public string Usuario { get; set; }
        public string Clave { get; set; }
    }

    public sealed class LoginResponse
    {
        public string Token { get; set; }
        public string Usuario { get; set; }
        public string Rol { get; set; }
        public List<string> Permisos { get; set; }
        public DateTime ExpiraUtc { get; set; }
        public bool EsSuperAdmin { get; set; }
    }

    /// <summary>
    /// Una fila de material invertido en una casa (destajo finalizado tipo Material).
    /// Refleja el esquema que arma FormAlmacen_ConsultaCasa.
    /// </summary>
    public sealed class MaterialCasaDto
    {
        public string Ruta { get; set; }
        public string Destajo { get; set; }
        public string Material { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string Cuadrilla { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string Prototipo { get; set; }
    }
}
