using System;

namespace Calandria.Api.Models
{
    /// <summary>Saldo del add-on de IA para una obra en un rango de fechas.</summary>
    public sealed class SaldoObraDto
    {
        public int ObraId { get; set; }
        public string ObraNombre { get; set; }
        public int Conteo { get; set; }
        public decimal MontoACobrar { get; set; }

        /// <summary>
        /// False si la obra todavía no tiene la tabla ConciliacionesIaLog (falta
        /// correr Calandria.Api/Sql/8_APLICAR_ConciliacionesIaLog.sql ahí). Conteo
        /// y MontoACobrar quedan en 0 en ese caso, no es que de verdad no deba nada.
        /// </summary>
        public bool TablaDisponible { get; set; }
    }

    /// <summary>Una fila de ConciliacionesIaLog, para el detalle/historial de una obra.</summary>
    public sealed class ConciliacionIaHistorialDto
    {
        public string Uuid { get; set; }
        public string FolioOC { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
    }

    public sealed class GuardarPdfFacturacionRequest
    {
        public int? ObraId { get; set; }
        public string ObraNombre { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int Conteo { get; set; }
        public decimal MontoACobrar { get; set; }
        public string NombreArchivo { get; set; }
        public string PdfBase64{ get; set; }
    }

    public sealed class PdfFacturacionDto
    {
        public int Id { get; set; }
        public int? ObraId { get; set; }
        public string ObraNombre { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int Conteo { get; set; }
        public decimal MontoACobrar { get; set; }
        public string NombreArchivo { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Usuario { get; set; }
    }
}
