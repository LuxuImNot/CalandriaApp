using System;
using System.Collections.Generic;

namespace Calandria.Api.Models
{
    /// <summary>Miembro de una cuadrilla (refleja MiembrosCuadrilla).</summary>
    public sealed class MiembroCuadrillaDto
    {
        public int? IdTrabajador { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public bool EsJefe { get; set; }
        public string Telefono { get; set; }
    }

    /// <summary>Monto asignado a un trabajador en una asignación de nómina.</summary>
    public sealed class MontoTrabajadorDto
    {
        public int? IdTrabajador { get; set; }
        public string NombreTrabajador { get; set; }
        public decimal Monto { get; set; }
    }

    /// <summary>
    /// Asignación de nómina previamente guardada para una tarea concreta
    /// (Manzana/Lote/Ruta/NodoID). Si no existe, <see cref="CodigoCuadrilla"/>
    /// llega nulo y <see cref="Montos"/> vacío.
    /// </summary>
    public sealed class AsignacionNominaDto
    {
        public string CodigoCuadrilla { get; set; }
        public List<MontoTrabajadorDto> Montos { get; set; } = new List<MontoTrabajadorDto>();
    }

    /// <summary>Una línea (un trabajador) de la asignación a guardar.</summary>
    public sealed class LineaAsignacionNomina
    {
        public int? IdTrabajador { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public bool EsJefe { get; set; }
        public decimal Monto { get; set; }
    }

    /// <summary>
    /// Cuerpo de POST /api/nomina/asignacion. Reemplaza por completo la
    /// asignación de la tarea (DELETE + INSERT en transacción), igual que
    /// FormAsignarNomina.BtnGuardar.
    /// </summary>
    public sealed class GuardarAsignacionRequest
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public int NodoId { get; set; }
        public string NombreTarea { get; set; }
        public string CodigoCuadrilla { get; set; }
        public decimal TotalDistribuir { get; set; }
        public List<LineaAsignacionNomina> Lineas { get; set; }
    }

    // ---- Reporte y recibos de nómina ----

    /// <summary>
    /// Fila agregada por trabajador del reporte de nómina (FormReporteNomina).
    /// </summary>
    public sealed class NominaReporteDto
    {
        public int? IdTrabajador { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public string Cuadrillas { get; set; }
        public int NumRecibos { get; set; }
        public decimal Monto { get; set; }
    }

    /// <summary>
    /// Cabecera de un recibo de nómina (FormVisorRecibosNomina). No incluye el
    /// PDF (binario): se descarga aparte vía /recibos/{id}/pdf.
    /// </summary>
    public sealed class ReciboNominaDto
    {
        public int Id { get; set; }
        public int? IdTrabajador { get; set; }
        public string NombreTrabajador { get; set; }
        public string Rol { get; set; }
        public string CodigoCuadrilla { get; set; }
        public string Concepto { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaRecibo { get; set; }
        public DateTime? PeriodoDesde { get; set; }
        public DateTime? PeriodoHasta { get; set; }
        public decimal TotalCuadrilla { get; set; }
        public bool TienePdf { get; set; }
    }

    // ---- Distribución de nómina (escritura: FormDistribucionNomina) ----

    /// <summary>Un recibo a persistir, con su PDF en base64.</summary>
    public sealed class ReciboInput
    {
        public int? IdTrabajador { get; set; }
        public string NombreTrabajador { get; set; }
        public string Rol { get; set; }
        public string Concepto { get; set; }
        public decimal Monto { get; set; }
        public string PdfBase64 { get; set; }
    }

    /// <summary>Un destajo finalizado a marcar como nómina distribuida.</summary>
    public sealed class DestajoDistribuidoInput
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public int NodoId { get; set; }
    }

    /// <summary>
    /// Cuerpo de POST /api/nomina/distribucion. Persiste todos los recibos de la
    /// cuadrilla y marca sus destajos como distribuidos, en una transacción.
    /// </summary>
    public sealed class DistribucionNominaRequest
    {
        public string CodigoCuadrilla { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public decimal TotalCuadrilla { get; set; }
        public List<ReciboInput> Recibos { get; set; }
        public List<DestajoDistribuidoInput> Destajos { get; set; }
    }
}
