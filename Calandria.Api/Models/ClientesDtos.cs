using System;
using System.Collections.Generic;

namespace Calandria.Api.Models
{
    public sealed class ClienteResumenDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int CantidadObras { get; set; }
        public int CantidadUsuarios { get; set; }
    }

    public sealed class CrearClienteRequest
    {
        public string NombreCliente { get; set; }
        public string NombreObra { get; set; }
        public string Usuario { get; set; }
        public string Clave { get; set; }
        public string FotoBase64 { get; set; }
        public string Extension { get; set; }
        public DateTime FechaAlta { get; set; }
    }

    public sealed class CrearClienteResponseDto
    {
        public int ClienteId { get; set; }
        public int ObraId { get; set; }
        public string NombreBD { get; set; }
    }
}
