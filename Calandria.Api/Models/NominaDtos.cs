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
}
