using System;
using System.Collections.Generic;

namespace Calandria.Api.Models
{
    // ==================================================================
    // Historial (FormAlmacen_Historial.cs)
    // ==================================================================

    public class HistorialMovimientoDto
    {
        public DateTime Fecha { get; set; }
        public string TipoMovimiento { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe => Math.Round(PrecioUnitario * Cantidad, 2);
        public string Usuario { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Justificacion { get; set; }
    }

    // ==================================================================
    // Inventario — listado de EntradasAlmacen (FormAlmacen_Inventario.cs)
    // ==================================================================

    public class InventarioAlmacenItemDto
    {
        public string FolioOC { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public DateTime FechaEntrada { get; set; }
        public string Usuario { get; set; }
        public string Justificacion { get; set; }
        public bool EsExcepcion { get; set; }
        public string Estado => EsExcepcion ? "DISCREPANCIA" : "COMPLETO";
    }

    // ==================================================================
    // Entradas (FormAlmacen_Entradas.cs / .EntradasTarjetas.cs)
    // ==================================================================

    public class OrdenPendienteDto
    {
        public string Folio { get; set; }
        public string NombreOrden { get; set; }
    }

    public class OcDetallePendienteDto
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal CantidadComprada { get; set; }
        public string Estado { get; set; }
        public string Justificacion { get; set; }
    }

    public class OcCasaDto
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
    }

    public class CasaAlmacenDto
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
    }

    public class EntradaLineaRequest
    {
        public int IdDetalle { get; set; }
        public decimal CantidadRecibida { get; set; }
        public string Justificacion { get; set; }
    }

    public class CapturarEntradaRequest
    {
        public string FolioOC { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public List<EntradaLineaRequest> Lineas { get; set; }
    }

    /// <summary>Insumo de un movimiento (entrada o salida), usado por el cliente para regenerar el PDF.</summary>
    public class InsumoMovimientoDto
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string Justificacion { get; set; }
    }

    public class ResultadoEntradaDto
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public List<InsumoMovimientoDto> Insumos { get; set; }
    }

    public class EliminarDetalleRequest
    {
        public string Justificacion { get; set; }
    }

    // ==================================================================
    // Salidas (FormAlmacen_Salidas.cs / .ExtensionPDFSalidas.cs)
    // ==================================================================

    public class InsumoPendienteSalidaDto
    {
        public string Destajo { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Disponible { get; set; }
        public decimal MaximoPermitido { get; set; }
    }

    public class PendientesSalidaDto
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public List<InsumoPendienteSalidaDto> Insumos { get; set; }
        /// <summary>Mensaje informativo cuando no hay destajos activados (Insumos queda vacío).</summary>
        public string Mensaje { get; set; }
    }

    public class SalidaLineaRequest
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal CantidadSolicitada { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class RegistrarSalidaRequest
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public List<SalidaLineaRequest> Insumos { get; set; }
    }

    public class ResultadoSalidaDto
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public string Folio { get; set; }
        public decimal TotalImporte { get; set; }
        public List<InsumoMovimientoDto> Insumos { get; set; }
    }

    public class ArchivarValeSalidaRequest
    {
        public string Folio { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public decimal TotalImporte { get; set; }
        public string Solicitante { get; set; }
        public string ResidenteObra { get; set; }
        public string EncargadoAlmacen { get; set; }
        public string Observaciones { get; set; }
        public string NombreArchivo { get; set; }
        public string PdfBase64 { get; set; }
        public List<InsumoMovimientoDto> Insumos { get; set; }
    }
}
