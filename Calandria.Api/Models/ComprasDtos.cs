namespace Calandria.Api.Models
{
    /// <summary>
    /// Proveedor de la tabla unificada PROVEEDORESCALANDRIA. La lista completa se
    /// sirve de una vez y el cliente filtra/ordena localmente (catálogo pequeño).
    /// </summary>
    public sealed class ProveedorDto
    {
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
}
