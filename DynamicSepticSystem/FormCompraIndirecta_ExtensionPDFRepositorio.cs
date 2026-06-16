using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Generic;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Extensión parcial de FormCompraIndirecta para gestionar el repositorio de PDFs de órdenes de compra indirectas
    /// </summary>
    public partial class FormCompraIndirecta
    {
        // MÉTODO: Guardar folio de orden de compra indirecta en BD
        private int GuardarFolioOrdenCompraIndirecta(string folio, string tipoOrden, string nombreProveedor, 
            string codigoProveedor, decimal subtotal, decimal iva, decimal total)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO FoliosOrdenCompra 
                        (Folio, Manzana, Lote, FechaGeneracion, TipoOrden, NombreProveedor, CodigoProveedor, 
                         TotalSinIVA, IVA, TotalConIVA, Usuario, Estado)
                        VALUES 
                        (@folio, NULL, NULL, GETDATE(), @tipo, @nombreProv, @codigoProv, 
                         @subtotal, @iva, @total, @usuario, 'PENDIENTE');
                        
                        SELECT CAST(SCOPE_IDENTITY() AS INT)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@folio", folio);
                        cmd.Parameters.AddWithValue("@tipo", tipoOrden);
                        cmd.Parameters.AddWithValue("@nombreProv", nombreProveedor ?? string.Empty);
                        cmd.Parameters.AddWithValue("@codigoProv", codigoProveedor ?? string.Empty);
                        cmd.Parameters.AddWithValue("@subtotal", subtotal);
                        cmd.Parameters.AddWithValue("@iva", iva);
                        cmd.Parameters.AddWithValue("@total", total);
                        cmd.Parameters.AddWithValue("@usuario", Environment.UserName);

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar folio: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        // MÉTODO: Guardar detalle de insumos indirectos del folio
        private void GuardarDetalleOrdenCompraIndirecta(int folioId, List<InsumoIndirecto> insumos)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO FoliosOrdenCompraDetalle 
                        (FolioId, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Familia)
                        VALUES 
                        (@folioId, @clave, @desc, @unidad, @cantidad, @precio, @importe, NULL)";

                    foreach (var insumo in insumos)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@folioId", folioId);
                            cmd.Parameters.AddWithValue("@clave", insumo.Clave ?? string.Empty);
                            cmd.Parameters.AddWithValue("@desc", insumo.Descripcion ?? string.Empty);
                            cmd.Parameters.AddWithValue("@unidad", insumo.Unidad ?? string.Empty);
                            cmd.Parameters.AddWithValue("@cantidad", insumo.Cantidad);
                            cmd.Parameters.AddWithValue("@precio", insumo.Costo);
                            cmd.Parameters.AddWithValue("@importe", insumo.Importe);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar detalle: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // MÉTODO: Guardar PDF en BD como VARBINARY
        private void GuardarPDFOrdenCompraIndirectaEnBD(int folioId, string folio, string rutaPdf, string tipoOrden)
        {
            try
            {
                byte[] pdfBytes = File.ReadAllBytes(rutaPdf);
                string nombreArchivo = Path.GetFileName(rutaPdf);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO PDFsOrdenCompra 
                        (FolioId, Folio, Manzana, Lote, TipoOrden, NombreArchivo, ContenidoPDF, TamanioBytes, FechaAlmacenamiento)
                        VALUES 
                        (@folioId, @folio, NULL, NULL, @tipo, @nombreArchivo, @contenidoPDF, @tamanio, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@folioId", folioId);
                        cmd.Parameters.AddWithValue("@folio", folio);
                        cmd.Parameters.AddWithValue("@tipo", tipoOrden);
                        cmd.Parameters.AddWithValue("@nombreArchivo", nombreArchivo);
                        cmd.Parameters.AddWithValue("@contenidoPDF", pdfBytes);
                        cmd.Parameters.AddWithValue("@tamanio", pdfBytes.Length);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar PDF en BD: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // MÉTODO: Guardar orden indirecta completa en repositorio (llamar después de generar PDF)
        public void GuardarOrdenIndirectaEnRepositorio(string folioOC, string rutaPdf, 
            List<InsumoIndirecto> insumos, string tipoTitulo)
        {
            try
            {
                // Verificar que las tablas existan
                CrearTablasRepositorioSiNoExisten();

                // Calcular totales
                decimal subtotal = insumos.Sum(i => i.Importe);
                decimal iva = subtotal * 0.16m;
                decimal total = subtotal + iva;

                // Obtener información del proveedor
                string claveProveedor = txtClaveProveedor.Text;
                string nombreProveedor = lblNombreProveedor.Text.Replace("Nombre: ", "").Trim();

                // Determinar tipo de orden basado en el título seleccionado
                string tipoOrden = tipoTitulo.Contains("ADMINISTRATIVA") ? "ADMINISTRATIVA" : "INDIRECTA";

                // 1. Guardar folio
                int folioId = GuardarFolioOrdenCompraIndirecta(folioOC, tipoOrden, nombreProveedor, 
                    claveProveedor, subtotal, iva, total);

                if (folioId <= 0)
                {
                    MessageBox.Show("No se pudo guardar el folio en el repositorio", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2. Guardar detalle de insumos
                GuardarDetalleOrdenCompraIndirecta(folioId, insumos);

                // 3. Guardar PDF en BD
                if (File.Exists(rutaPdf))
                {
                    GuardarPDFOrdenCompraIndirectaEnBD(folioId, folioOC, rutaPdf, tipoOrden);
                }

                // Mensaje de éxito
                MessageBox.Show(
                    $"? Orden guardada en repositorio exitosamente\n\n" +
                    $"Folio: {folioOC}\n" +
                    $"Tipo: {tipoOrden}\n" +
                    $"Insumos: {insumos.Count}\n" +
                    $"Total: {total:C2}\n\n" +
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

        // MÉTODO: Crear tablas del repositorio si no existen
        private void CrearTablasRepositorioSiNoExisten()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Verificar si la tabla principal existe
                    string checkSql = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'FoliosOrdenCompra'";

                    using (SqlCommand cmd = new SqlCommand(checkSql, conn))
                    {
                        int count = (int)cmd.ExecuteScalar();
                        
                        if (count == 0)
                        {
                            // Las tablas no existen, ejecutar script de creación
                            string scriptPath = Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory, 
                                "SQL_CrearTablasRepositorioPDFsOrdenesCompra.sql");

                            if (File.Exists(scriptPath))
                            {
                                string script = File.ReadAllText(scriptPath);
                                
                                // Ejecutar script por lotes
                                string[] batches = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                                
                                foreach (string batch in batches)
                                {
                                    if (!string.IsNullOrWhiteSpace(batch))
                                    {
                                        try
                                        {
                                            using (SqlCommand batchCmd = new SqlCommand(batch, conn))
                                            {
                                                batchCmd.ExecuteNonQuery();
                                            }
                                        }
                                        catch { /* Ignorar errores de PRINT */ }
                                    }
                                }

                                MessageBox.Show(
                                    "Las tablas del repositorio de PDFs fueron creadas exitosamente.", 
                                    "Inicialización Completa", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show(
                                    "No se encontró el script SQL de inicialización.\n\n" +
                                    "Busca: SQL_CrearTablasRepositorioPDFsOrdenesCompra.sql\n\n" +
                                    "Y ejecútalo manualmente en la base de datos.",
                                    "Advertencia",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar tablas: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // MÉTODO: Abrir repositorio de PDFs de órdenes indirectas (agregar botón en el diseñador)
        public void AbrirRepositorioPDFsOrdenesIndirectas()
        {
            try
            {
                // Verificar que las tablas existan
                CrearTablasRepositorioSiNoExisten();

                // Abrir repositorio filtrado solo para órdenes indirectas/administrativas (sin manzana/lote)
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
