using System.Collections.Generic;

namespace Calandria.Api.Models
{
    // ---- Avance de obra jerárquico (FormAvanceObra / FormHardProgress) ----

    /// <summary>Una partida de PresupuestoObra con su importe (columna de costo según prototipo).</summary>
    public sealed class PartidaAvanceDto
    {
        public int Wbs { get; set; }
        public string Codigo { get; set; }
        public string Padre { get; set; }
        public string Etapa { get; set; }
        public string Partida { get; set; }
        public double ImporteTotal { get; set; }
    }

    /// <summary>Avance guardado de una partida (AvanceManualObra). MontoEjecutado es null si la columna no existe/está vacía.</summary>
    public sealed class AvanceGuardadoDto
    {
        public int Wbs { get; set; }
        public double AvancePorcentaje { get; set; }
        public double? MontoEjecutado { get; set; }
    }

    /// <summary>Datos para reconstruir el árbol de avance de una casa: partidas + avances guardados.</summary>
    public sealed class JerarquicoAvanceResponse
    {
        public List<PartidaAvanceDto> Partidas { get; set; } = new List<PartidaAvanceDto>();
        public List<AvanceGuardadoDto> Avances { get; set; } = new List<AvanceGuardadoDto>();
    }

    /// <summary>Alta/actualización del avance de una partida (upsert en AvanceManualObra).</summary>
    public sealed class GuardarAvancePartidaRequest
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public int Wbs { get; set; }
        public double AvancePorcentaje { get; set; }
        public string Concepto { get; set; }
        public double ImporteTotal { get; set; }
        public double ImporteEjecutado { get; set; }
    }

    // ---- Avance por concepto (FormAvanceConcepto) ----

    /// <summary>Un concepto de Estimacion(Concepto) con su importe total (columna según prototipo).</summary>
    public sealed class ConceptoAvanceDto
    {
        public string Codigo { get; set; }
        public string Concepto { get; set; }
        public double Total { get; set; }
    }

    /// <summary>Total e importe ejecutado agregados por Padre (de PresupuestoObra + AvanceManualObra).</summary>
    public sealed class AvancePorPadreDto
    {
        public string Padre { get; set; }
        public double Total { get; set; }
        public double Ejecutado { get; set; }
    }

    /// <summary>Alta/actualización del avance de un concepto (upsert en AvanceManualConcepto).</summary>
    public sealed class GuardarAvanceConceptoRequest
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Codigo { get; set; }
        public string Concepto { get; set; }
        public double AvancePorcentaje { get; set; }
    }
}
