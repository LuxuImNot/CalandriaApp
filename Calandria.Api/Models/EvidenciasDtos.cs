using System;

namespace Calandria.Api.Models
{
    // ---- Evidencias fotográficas de avance de obra (tabla EvidenciasFotograficas) ----

    /// <summary>Alta de una evidencia. La foto viaja en base64.</summary>
    public sealed class GuardarEvidenciaRequest
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Titulo { get; set; }
        public string FotoBase64 { get; set; }
        public string Extension { get; set; }
        public string Usuario { get; set; }
    }

    /// <summary>
    /// Evidencia fotográfica. En el listado <see cref="FotoBase64"/> viene vacío;
    /// solo se rellena al pedir la última evidencia de un lote.
    /// </summary>
    public sealed class EvidenciaDto
    {
        public int Id { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Titulo { get; set; }
        public string FotoBase64 { get; set; }
        public string Extension { get; set; }
        public double TamanioKB { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
    }

    // ---- Fotos adjuntas a un concepto/partida de una casa (tabla FotosConcepto) ----

    /// <summary>Alta de una foto de concepto/partida. La foto viaja en base64.</summary>
    public sealed class GuardarFotoConceptoRequest
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Identificador { get; set; }
        public bool EsConcepto { get; set; }
        public string NombreNodo { get; set; }
        public string Descripcion { get; set; }
        public string FotoBase64 { get; set; }
        public string Extension { get; set; }
        public string Usuario { get; set; }
    }

    /// <summary>Metadatos de una foto de concepto/partida (sin el binario).</summary>
    public sealed class FotoConceptoDto
    {
        public int Id { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Identificador { get; set; }
        public bool EsConcepto { get; set; }
        public string NombreNodo { get; set; }
        public string Descripcion { get; set; }
        public string Extension { get; set; }
        public double TamanioKB { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
    }

    /// <summary>Cambio de descripción de una foto de concepto.</summary>
    public sealed class ActualizarDescripcionRequest
    {
        public string Descripcion { get; set; }
    }
}
