using System;

namespace Calandria.Api.Models
{
    /// <summary>
    /// Proveedor de la tabla unificada PROVEEDORESCALANDRIA. La lista completa se
    /// sirve de una vez y el cliente filtra/ordena localmente (catálogo pequeño).
    /// </summary>
    public sealed class ProveedorDto
    {
        public string Folio { get; set; }
        public string ClaveUnica { get; set; }
        public string Nombre { get; set; }
        public string Rfc { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
    }

    /// <summary>Alta de proveedor (FormAgregarProveedor).</summary>
    public sealed class CrearProveedorRequest
    {
        public string ClaveUnica { get; set; }
        public string Nombre { get; set; }
        public string Rfc { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
    }

    /// <summary>Respuesta al crear un proveedor: el folio autogenerado (PROV-NNNN).</summary>
    public sealed class ProveedorCreadoResponse
    {
        public string Folio { get; set; }
    }

    // ---- Órdenes de compra ----

    /// <summary>Una casa cubierta por una orden múltiple (OrdenesCompra_Casas).</summary>
    public sealed class CasaOrdenDto
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
    }

    /// <summary>Una línea de detalle de la orden (OrdenesCompraDetalle).</summary>
    public sealed class DetalleOrdenDto
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal ImporteTotal { get; set; }
        public string Familia { get; set; }
    }

    /// <summary>Alta de orden MÚLTIPLE (FormCompraMulti): cabecera + casas + detalle.</summary>
    public sealed class CrearOrdenMultipleRequest
    {
        public string Usuario { get; set; }
        public string NombreOrden { get; set; }
        public System.Collections.Generic.List<CasaOrdenDto> Casas { get; set; }
        public System.Collections.Generic.List<DetalleOrdenDto> Detalles { get; set; }
    }

    /// <summary>Alta de orden INDIRECTA (FormCompraIndirecta): cabecera + detalle.</summary>
    public sealed class CrearOrdenIndirectaRequest
    {
        public string Usuario { get; set; }
        public string NombreOrden { get; set; }
        public string ProveedorClave { get; set; }
        public System.Collections.Generic.List<DetalleOrdenDto> Detalles { get; set; }
    }

    /// <summary>Folio generado por el servidor al guardar una orden.</summary>
    public sealed class FolioOrdenResponse
    {
        public string FolioOC { get; set; }
    }

    // ---- Conciliación de factura (CFDI) contra la OC ----

    /// <summary>Petición: el XML del CFDI (base64) y el folio de la OC a conciliar.</summary>
    public sealed class ConciliarFacturaRequest
    {
        public string FolioOC { get; set; }
        public string XmlBase64 { get; set; }
    }

    /// <summary>Un concepto leído del CFDI (datos exactos del XML).</summary>
    public sealed class FacturaConceptoDto
    {
        public string NoIdentificacion { get; set; }
        public string ClaveProdServ { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Importe { get; set; }
    }

    /// <summary>
    /// Una línea de la OC ya conciliada contra la factura: cuánto se compró vs cuánto
    /// se facturó y de dónde salió el emparejamiento (clave / nombre / automático / sin match).
    /// </summary>
    public sealed class LineaConciliacionDto
    {
        public int IdDetalle { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal CantidadOC { get; set; }
        public decimal CantidadFacturada { get; set; }
        public decimal Diferencia { get; set; }          // facturada - OC (negativo = faltante/parcial)
        public string DescripcionFactura { get; set; }   // texto del concepto emparejado
        public string Origen { get; set; }               // "Clave" | "Nombre" | "Auto" | "SinMatch"
        public double Confianza { get; set; }            // 0..1 (1 = exacta por clave)
    }

    /// <summary>Resultado de conciliar un CFDI contra una OC.</summary>
    public sealed class ConciliarFacturaResponse
    {
        public string FolioOC { get; set; }
        public string Emisor { get; set; }
        public string Rfc { get; set; }
        public string Uuid { get; set; }
        public decimal TotalFactura { get; set; }
        public bool UsoIA { get; set; }
        public string Aviso { get; set; }
        public System.Collections.Generic.List<LineaConciliacionDto> Lineas { get; set; }
            = new System.Collections.Generic.List<LineaConciliacionDto>();
        // Conceptos de la factura que no se pudieron asignar a ninguna línea de la OC.
        public System.Collections.Generic.List<FacturaConceptoDto> SinAsignar { get; set; }
            = new System.Collections.Generic.List<FacturaConceptoDto>();
    }

    /// <summary>
    /// Resumen para facturar el add-on de "Conciliación de factura con IA" (Pilaris):
    /// cuántas facturas se conciliaron con el emparejador automático en el rango y el
    /// monto a cobrar ($18 MXN/factura, mínimo $500 MXN/mes por obra).
    /// </summary>
    public sealed class ConciliacionesIaResumenDto
    {
        public int Conteo { get; set; }
        public decimal MontoACobrar { get; set; }
    }

    // ---- Catálogos de material (COMPRASCALANDRA / COMPRASTUNERA, por prototipo) ----

    /// <summary>Fila del catálogo de explosión de un prototipo.</summary>
    public sealed class CatalogoMaterialDto
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public string Familia { get; set; }
        public decimal Precio { get; set; }

        /// <summary>
        /// true si el insumo tiene clave de almacén (del árbol o resuelta por catálogo);
        /// false si quedó sin clave (se rastreará por nombre). Solo lo usa el catálogo de
        /// destajos; en el catálogo plano por prototipo todas las filas traen clave.
        /// </summary>
        public bool ClaveResuelta { get; set; }
    }

    /// <summary>Cantidad pendiente de surtir de un insumo en una casa.</summary>
    public sealed class PendienteMaterialDto
    {
        public string Clave { get; set; }
        public decimal CantidadPendiente { get; set; }
    }

    /// <summary>
    /// Alta/edición de una fila del catálogo de un prototipo. Si <see cref="Precio"/>
    /// llega con valor se actualiza la columna Costo (best-effort). Si
    /// <see cref="ActualizarCampos"/> es false y la fila ya existe, no se tocan los
    /// campos descriptivos (solo el costo) — reproduce el caso "aplicar precio".
    /// </summary>
    public sealed class UpsertCatalogoRequest
    {
        public string Prototipo { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public string Familia { get; set; }
        public decimal? Precio { get; set; }
        public bool ActualizarCampos { get; set; }
    }

    public sealed class UpsertCatalogoResponse
    {
        /// <summary>Si se logró escribir la columna Costo (false si la tabla no la tiene).</summary>
        public bool CostoAplicado { get; set; }
    }

    public sealed class EliminarCatalogoRequest
    {
        public string Prototipo { get; set; }
        public string Clave { get; set; }
    }

    // ---- COMPRASINDIRECTAS ----

    public sealed class InsumoIndirectoDto
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
    }

    public sealed class CrearInsumoIndirectoRequest
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
    }

    // ---- Repositorio de PDFs de órdenes de compra ----

    /// <summary>Casa incluida en una orden múltiple del repositorio.</summary>
    public sealed class CasaRepoDto
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
    }

    /// <summary>Alta de orden MÚLTIPLE en el repositorio (FormCompraMulti).</summary>
    public sealed class GuardarRepoMultipleRequest
    {
        public string FolioOC { get; set; }
        public string Usuario { get; set; }
        public string NombreProveedor { get; set; }
        public string CodigoProveedor { get; set; }
        public string NombreArchivo { get; set; }
        public string PdfBase64 { get; set; }
        public System.Collections.Generic.List<DetalleOrdenDto> Detalles { get; set; }
        public System.Collections.Generic.List<CasaRepoDto> Casas { get; set; }
    }

    /// <summary>Alta de orden INDIRECTA/ADMINISTRATIVA en el repositorio (FormCompraIndirecta).</summary>
    public sealed class GuardarRepoIndirectaRequest
    {
        public string FolioOC { get; set; }
        public string Usuario { get; set; }
        public string TipoOrden { get; set; }
        public string NombreProveedor { get; set; }
        public string CodigoProveedor { get; set; }
        public string NombreArchivo { get; set; }
        public string PdfBase64 { get; set; }
        public System.Collections.Generic.List<DetalleOrdenDto> Detalles { get; set; }
    }

    /// <summary>Resultado del alta en repositorio (para el mensaje de confirmación).</summary>
    public sealed class GuardarRepoResponse
    {
        public int FolioId { get; set; }
        public string TipoOrden { get; set; }
        public decimal Total { get; set; }
        public int Insumos { get; set; }
    }

    /// <summary>Fila del listado del repositorio (FormRepositorioPDFsOrdenesCompra).</summary>
    public sealed class OrdenRepoDto
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string TipoOrden { get; set; }
        public string NombreProveedor { get; set; }
        public string CodigoProveedor { get; set; }
        public decimal TotalConIVA { get; set; }
        public int? NumeroOrden { get; set; }
        public string Estado { get; set; }
        public int TotalInsumos { get; set; }
        public string CasasIncluidas { get; set; }
    }

    /// <summary>Línea de detalle de una orden del repositorio.</summary>
    public sealed class DetalleInsumoRepoDto
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal ImporteTotal { get; set; }
        public string Familia { get; set; }
    }

    /// <summary>Detalle completo de una orden del repositorio (FormDetalleOrdenCompra).</summary>
    public sealed class DetalleOrdenRepoResponse
    {
        public string Folio { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string TipoOrden { get; set; }
        public string NombreProveedor { get; set; }
        public string CodigoProveedor { get; set; }
        public decimal TotalSinIVA { get; set; }
        public decimal IVA { get; set; }
        public decimal TotalConIVA { get; set; }
        public int? NumeroOrden { get; set; }
        public string Usuario { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public System.Collections.Generic.List<CasaRepoDto> Casas { get; set; } = new System.Collections.Generic.List<CasaRepoDto>();
        public System.Collections.Generic.List<DetalleInsumoRepoDto> Insumos { get; set; } = new System.Collections.Generic.List<DetalleInsumoRepoDto>();
    }
}
