using System;
using System.Collections.Generic;

namespace Calandria.Api.Models
{
    /// <summary>Fila de AvanceManualObra/PresupuestoObra (reproduce RegistroEstimacion de FormAdministrativos).</summary>
    public sealed class EstimacionItemDto
    {
        public string Wbs { get; set; }
        public string Codigo { get; set; }
        public string Etapa { get; set; }
        public string Partida { get; set; }
        public decimal AvancePorcentaje { get; set; }
        public decimal MontoEjecutado { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
    }

    /// <summary>Fila de destajo activado (reproduce RegistroDestajo de FormAdministrativos).</summary>
    public sealed class DestajoItemDto
    {
        public int NodoId { get; set; }
        public string Tipo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public decimal MontoGastado { get; set; }
        public string Cuadrilla { get; set; }
        public bool Finalizado { get; set; }
        public string Estado { get; set; }
    }

    /// <summary>Orden de compra ligada a la casa (OrdenesCompra_Casas), con su total de partidas.</summary>
    public sealed class CompraCasaDto
    {
        public string FolioOC { get; set; }
        public DateTime? Fecha { get; set; }
        public string TipoOrden { get; set; }
        public string NombreOrden { get; set; }
        public string Proveedor { get; set; }
        public int NumPartidas { get; set; }
        public decimal Importe { get; set; }
    }

    /// <summary>Vale de salida de Almacén hacia la casa (SalidasAlmacen).</summary>
    public sealed class SalidaAlmacenCasaDto
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string Justificacion { get; set; }
    }

    /// <summary>Asignación de nómina a un destajo de la casa (NominaTareasAsignada).</summary>
    public sealed class NominaCasaDto
    {
        public string NombreTarea { get; set; }
        public string CodigoCuadrilla { get; set; }
        public string NombreTrabajador { get; set; }
        public string Rol { get; set; }
        public bool EsJefe { get; set; }
        public decimal Monto { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    /// <summary>
    /// Concentrado financiero completo de una casa: Estimaciones + Destajos (igual que
    /// FormAdministrativos hoy) más Compras/Salidas de Almacén/Nómina detallada como
    /// secciones informativas independientes. GranTotal NO las incluye — ver nota en
    /// AdministrativosController.Consolidado.
    /// </summary>
    public sealed class ConsolidadoCasaDto
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }

        public List<EstimacionItemDto> Estimaciones { get; set; } = new List<EstimacionItemDto>();
        public List<DestajoItemDto> Destajos { get; set; } = new List<DestajoItemDto>();
        public List<CompraCasaDto> Compras { get; set; } = new List<CompraCasaDto>();
        public List<SalidaAlmacenCasaDto> SalidasAlmacen { get; set; } = new List<SalidaAlmacenCasaDto>();
        public List<NominaCasaDto> Nomina { get; set; } = new List<NominaCasaDto>();

        public decimal TotalEstimaciones { get; set; }
        public decimal TotalDestajosComprometido { get; set; }
        public decimal TotalManoObraGastado { get; set; }
        public decimal TotalMaterialGastado { get; set; }
        public decimal TotalCompras { get; set; }
        public decimal TotalSalidasAlmacen { get; set; }
        public decimal TotalNomina { get; set; }

        /// <summary>TotalEstimaciones + TotalManoObraGastado + TotalMaterialGastado (misma fórmula que RefrescarHUD/GenerarPDF de siempre).</summary>
        public decimal GranTotal { get; set; }
    }
}
