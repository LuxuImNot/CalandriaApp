using System;
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

    // ---- Estimación jerárquica (FormEstimacionConceptoMigrado.CargaDatos) ----

    /// <summary>Partida de PresupuestoObra con su info dinámica (Costo ya resuelto por prototipo).</summary>
    public sealed class PartidaDinamicaDto
    {
        public int Wbs { get; set; }
        public string Codigo { get; set; }
        public string Padre { get; set; }
        public string Etapa { get; set; }
        public string Partida { get; set; }
        public double Costo { get; set; }
        public bool EsDinamica { get; set; }
        public double ValorM2Tunera { get; set; }
        public double ValorM2Calandra { get; set; }
        public double LimiteM2 { get; set; }
        public double LimiteM2Tunera { get; set; }
        public double LimiteM2Calandra { get; set; }
        public string PrototiposAplicables { get; set; }
    }

    /// <summary>Avance guardado de una partida con m² y fecha de finalización (AvanceManualObra).</summary>
    public sealed class AvancePartidaDto
    {
        public int Wbs { get; set; }
        public double AvancePorcentaje { get; set; }
        public double MontoEjecutado { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public double MetrosCuadrados { get; set; }
    }

    /// <summary>Partidas (PresupuestoObra) + avances (AvanceManualObra) para armar el árbol de estimación.</summary>
    public sealed class EstimacionJerarquicaResponse
    {
        public List<PartidaDinamicaDto> Partidas { get; set; } = new List<PartidaDinamicaDto>();
        public List<AvancePartidaDto> Avances { get; set; } = new List<AvancePartidaDto>();
    }
}
