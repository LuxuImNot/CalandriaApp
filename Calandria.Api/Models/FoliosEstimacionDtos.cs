using System.Collections.Generic;

namespace Calandria.Api.Models
{
    // ---- Folios de estimación (FormEstimacionConceptoMigrado_ExtensionFolios) ----

    /// <summary>Una línea del detalle del folio (una partida incluida en la estimación).</summary>
    public sealed class DetalleFolioDto
    {
        public string CodigoConcepto { get; set; }
        public string NombreConcepto { get; set; }
        public int Wbs { get; set; }
        public string NombrePartida { get; set; }
        public double MontoPresupuestado { get; set; }
        public double MontoEjecutado { get; set; }
        public double AvancePorcentaje { get; set; }
    }

    /// <summary>
    /// Guarda una estimación completa (cabecera FoliosEstimacion + detalle
    /// FoliosEstimacionDetalle + PDF en PDFsEstimacion) en una transacción.
    /// El cliente genera el folio, los totales y el PDF (base64).
    /// </summary>
    public sealed class GuardarFolioEstimacionRequest
    {
        public string Folio { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public int NumeroEstimacion { get; set; }
        public string Proveedor { get; set; }
        public string Descripcion { get; set; }
        public double ImporteContrato { get; set; }
        public double TotalRequisicion { get; set; }
        public double Amortizacion { get; set; }
        public double PorcentajeAmortizacion { get; set; }
        public double TotalEstimacion { get; set; }
        public string Usuario { get; set; }

        public List<DetalleFolioDto> Detalle { get; set; } = new List<DetalleFolioDto>();

        public string NombreArchivoPdf { get; set; }
        public string PdfBase64 { get; set; }
    }

    /// <summary>Respuesta al guardar una estimación: el Id del folio generado.</summary>
    public sealed class GuardarFolioEstimacionResponse
    {
        public int FolioId { get; set; }
    }
}
