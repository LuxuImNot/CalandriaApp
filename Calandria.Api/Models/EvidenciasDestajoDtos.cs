using System;

namespace Calandria.Api.Models
{
    // ---- Evidencia fotográfica de un destajo (tabla EvidenciasDestajo) ----

    /// <summary>Alta de una evidencia de destajo. La foto viaja en base64.</summary>
    public sealed class GuardarEvidenciaDestajoRequest
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public int NodoId { get; set; }
        public string NombreDestajo { get; set; }
        public string Descripcion { get; set; }
        public string FotoBase64 { get; set; }
        public string Extension { get; set; }
        public string Usuario { get; set; }
    }

    /// <summary>Metadatos de una evidencia de destajo (sin el binario).</summary>
    public sealed class EvidenciaDestajoDto
    {
        public int Id { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public int NodoId { get; set; }
        public string NombreDestajo { get; set; }
        public string Descripcion { get; set; }
        public string Extension { get; set; }
        public double TamanioKB { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
    }
}
