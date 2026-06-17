using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Extensión parcial de FormCompraIndirecta para guardar la orden indirecta en
    /// el repositorio de PDFs vía API (RepositorioOrdenesController).
    /// </summary>
    public partial class FormCompraIndirecta
    {
        // MÉTODO: Guardar orden indirecta completa en repositorio (llamar después de generar PDF)
        public void GuardarOrdenIndirectaEnRepositorio(string folioOC, string rutaPdf,
            List<InsumoIndirecto> insumos, string tipoTitulo)
        {
            try
            {
                string claveProveedor = txtClaveProveedor.Text;
                string nombreProveedor = lblNombreProveedor.Text.Replace("Nombre: ", "").Trim();
                string tipoOrden = tipoTitulo.Contains("ADMINISTRATIVA") ? "ADMINISTRATIVA" : "INDIRECTA";

                string pdfBase64 = File.Exists(rutaPdf)
                    ? Convert.ToBase64String(File.ReadAllBytes(rutaPdf)) : null;
                string nombreArchivo = File.Exists(rutaPdf) ? Path.GetFileName(rutaPdf) : null;

                var resp = ApiClient.Post<GuardarRepoResponseApi>("/api/repositorio-ordenes/indirecta", new
                {
                    FolioOC = folioOC,
                    Usuario = Environment.UserName,
                    TipoOrden = tipoOrden,
                    NombreProveedor = nombreProveedor,
                    CodigoProveedor = claveProveedor,
                    NombreArchivo = nombreArchivo,
                    PdfBase64 = pdfBase64,
                    Detalles = insumos.Select(i => new DetalleOrdenApi
                    {
                        Clave = i.Clave,
                        Descripcion = i.Descripcion,
                        Unidad = i.Unidad,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.Costo,
                        ImporteTotal = i.Importe,
                        Familia = null
                    }).ToList()
                });

                MessageBox.Show(
                    $"? Orden guardada en repositorio exitosamente\n\n" +
                    $"Folio: {folioOC}\n" +
                    $"Tipo: {resp?.TipoOrden}\n" +
                    $"Insumos: {resp?.Insumos}\n" +
                    $"Total: {resp?.Total:C2}\n\n" +
                    $"El PDF se almacenó en la base de datos.\n\n" +
                    $"Puedes consultar todas las órdenes indirectas en el repositorio.",
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

        // MÉTODO: Abrir repositorio filtrado a órdenes indirectas/administrativas
        public void AbrirRepositorioPDFsOrdenesIndirectas()
        {
            try
            {
                using (var formRepo = new FormRepositorioPDFsOrdenesCompra(null, null, soloIndirectas: true))
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
