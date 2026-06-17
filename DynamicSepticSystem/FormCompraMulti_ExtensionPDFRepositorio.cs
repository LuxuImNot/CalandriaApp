using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Extensión parcial de FormCompraMulti para guardar la orden en el repositorio
    /// de PDFs vía API (RepositorioOrdenesController). El servidor crea las tablas,
    /// determina tipo/manzana/lote/número y persiste folio+detalle+casas+PDF en una
    /// transacción.
    /// </summary>
    public partial class FormCompraMulti
    {
        // MÉTODO: Guardar orden completa en repositorio (llamar después de generar PDF)
        public void GuardarOrdenEnRepositorio(string folioOC, string rutaPdf,
            List<InsumoOrdenCompra> insumos,
            List<CasaSeleccionada> casas = null)
        {
            try
            {
                string pdfBase64 = File.Exists(rutaPdf)
                    ? Convert.ToBase64String(File.ReadAllBytes(rutaPdf)) : null;
                string nombreArchivo = File.Exists(rutaPdf) ? Path.GetFileName(rutaPdf) : null;

                var resp = ApiClient.Post<GuardarRepoResponseApi>("/api/repositorio-ordenes/multiple", new
                {
                    FolioOC = folioOC,
                    Usuario = Environment.UserName,
                    NombreProveedor = cmbNombreProveedor.Text,
                    CodigoProveedor = cmbCodigoProveedor.Text,
                    NombreArchivo = nombreArchivo,
                    PdfBase64 = pdfBase64,
                    Detalles = insumos.Select(i => new DetalleOrdenApi
                    {
                        Clave = i.Clave,
                        Descripcion = i.Descripcion,
                        Unidad = i.Unidad,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.Precio,
                        ImporteTotal = i.Importe,
                        Familia = i.Familia
                    }).ToList(),
                    Casas = (casas ?? new List<CasaSeleccionada>()).Select(c => new CasaRepoApi
                    {
                        Manzana = c.Manzana,
                        Lote = c.Lote,
                        Prototipo = c.Prototipo
                    }).ToList()
                });

                MessageBox.Show(
                    $"? Orden guardada en repositorio exitosamente\n\n" +
                    $"Folio: {folioOC}\n" +
                    $"Tipo: {resp?.TipoOrden}\n" +
                    $"Insumos: {resp?.Insumos}\n" +
                    $"Total: {resp?.Total:C2}\n\n" +
                    $"El PDF se almacenó en la base de datos.",
                    "Repositorio Actualizado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar en repositorio: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // MÉTODO: Abrir repositorio de PDFs (las tablas las asegura el API al consultar)
        public void AbrirRepositorioPDFsOrdenesCompra(string manzana = null, string lote = null)
        {
            try
            {
                using (var formRepo = new FormRepositorioPDFsOrdenesCompra(manzana, lote))
                {
                    formRepo.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir repositorio: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
