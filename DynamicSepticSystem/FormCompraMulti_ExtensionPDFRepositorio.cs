using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Extensión parcial de FormCompraMulti para gestionar el repositorio de PDFs de órdenes de compra
    /// </summary>
    public partial class FormCompraMulti
    {
        // MÉTODO: Obtener siguiente número de orden para una casa específica
        private int ObtenerSiguienteNumeroOrden(string manzana, string lote)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT ISNULL(MAX(NumeroOrden), 0) + 1 
                        FROM FoliosOrdenCompra 
                        WHERE Manzana = @manzana AND Lote = @lote";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@manzana", manzana);
                        cmd.Parameters.AddWithValue("@lote", lote);

                        var result = cmd.ExecuteScalar();
                        return result != DBNull.Value ? Convert.ToInt32(result) : 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener número de orden: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }

        // MÉTODO: Guardar folio de orden de compra en BD
        private int GuardarFolioOrdenCompra(string folio, string tipoOrden, string nombreProveedor, 
            string codigoProveedor, decimal subtotal, decimal iva, decimal total, 
            string manzana = null, string lote = null, int? numeroOrden = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO FoliosOrdenCompra 
                        (Folio, Manzana, Lote, FechaGeneracion, TipoOrden, NombreProveedor, CodigoProveedor, 
                         TotalSinIVA, IVA, TotalConIVA, NumeroOrden, Usuario, Estado)
                        VALUES 
                        (@folio, @manzana, @lote, GETDATE(), @tipo, @nombreProv, @codigoProv, 
                         @subtotal, @iva, @total, @numOrden, @usuario, 'PENDIENTE');
                        
                        SELECT CAST(SCOPE_IDENTITY() AS INT)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@folio", folio);
                        cmd.Parameters.AddWithValue("@manzana", (object)manzana ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@lote", (object)lote ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@tipo", tipoOrden);
                        cmd.Parameters.AddWithValue("@nombreProv", nombreProveedor ?? string.Empty);
                        cmd.Parameters.AddWithValue("@codigoProv", codigoProveedor ?? string.Empty);
                        cmd.Parameters.AddWithValue("@subtotal", subtotal);
                        cmd.Parameters.AddWithValue("@iva", iva);
                        cmd.Parameters.AddWithValue("@total", total);
                        cmd.Parameters.AddWithValue("@numOrden", (object)numeroOrden ?? DBNull.Value);
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

        // MÉTODO: Guardar detalle de insumos del folio
        private void GuardarDetalleOrdenCompra(int folioId, System.Collections.Generic.List<InsumoOrdenCompra> insumos)
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
                        (@folioId, @clave, @desc, @unidad, @cantidad, @precio, @importe, @familia)";

                    foreach (var insumo in insumos)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@folioId", folioId);
                            cmd.Parameters.AddWithValue("@clave", insumo.Clave ?? string.Empty);
                            cmd.Parameters.AddWithValue("@desc", insumo.Descripcion ?? string.Empty);
                            cmd.Parameters.AddWithValue("@unidad", insumo.Unidad ?? string.Empty);
                            cmd.Parameters.AddWithValue("@cantidad", insumo.Cantidad);
                            cmd.Parameters.AddWithValue("@precio", insumo.Precio);
                            cmd.Parameters.AddWithValue("@importe", insumo.Importe);
                            cmd.Parameters.AddWithValue("@familia", insumo.Familia ?? string.Empty);

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

        // MÉTODO: Guardar casas incluidas en orden múltiple
        private void GuardarCasasOrdenCompra(int folioId, System.Collections.Generic.List<CasaSeleccionada> casas)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO FoliosOrdenCompra_Casas 
                        (FolioId, Manzana, Lote, Prototipo)
                        VALUES 
                        (@folioId, @manzana, @lote, @prototipo)";

                    foreach (var casa in casas)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@folioId", folioId);
                            cmd.Parameters.AddWithValue("@manzana", casa.Manzana);
                            cmd.Parameters.AddWithValue("@lote", casa.Lote);
                            cmd.Parameters.AddWithValue("@prototipo", casa.Prototipo ?? string.Empty);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar casas: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // MÉTODO: Guardar PDF en BD como VARBINARY
        private void GuardarPDFOrdenCompraEnBD(int folioId, string folio, string rutaPdf, 
            string tipoOrden, string manzana = null, string lote = null)
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
                        (@folioId, @folio, @manzana, @lote, @tipo, @nombreArchivo, @contenidoPDF, @tamanio, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@folioId", folioId);
                        cmd.Parameters.AddWithValue("@folio", folio);
                        cmd.Parameters.AddWithValue("@manzana", (object)manzana ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@lote", (object)lote ?? DBNull.Value);
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

        // MÉTODO: Guardar orden completa en repositorio (llamar después de generar PDF)
        public void GuardarOrdenEnRepositorio(string folioOC, string rutaPdf, 
            System.Collections.Generic.List<InsumoOrdenCompra> insumos,
            System.Collections.Generic.List<CasaSeleccionada> casas = null)
        {
            try
            {
                // Verificar que las tablas existan
                CrearTablasRepositorioSiNoExisten();

                // Calcular totales
                decimal subtotal = insumos.Sum(i => i.Importe);
                decimal iva = subtotal * 0.16m;
                decimal total = subtotal + iva;

                string nombreProv = cmbNombreProveedor.Text;
                string codigoProv = cmbCodigoProveedor.Text;

                // Determinar tipo de orden y manzana/lote
                string tipoOrden = "MULTIPLE";
                string manzana = null;
                string lote = null;
                int? numeroOrden = null;

                // Si solo hay una casa, considerar como orden individual
                if (casas != null && casas.Count == 1)
                {
                    tipoOrden = "INDIVIDUAL";
                    manzana = casas[0].Manzana;
                    lote = casas[0].Lote;
                    numeroOrden = ObtenerSiguienteNumeroOrden(manzana, lote);
                }
                else if (casas != null && casas.Count > 1)
                {
                    // Para órdenes múltiples, usar la primera casa como referencia principal
                    manzana = casas[0].Manzana;
                    lote = casas[0].Lote;
                }

                // 1. Guardar folio
                int folioId = GuardarFolioOrdenCompra(folioOC, tipoOrden, nombreProv, codigoProv, 
                    subtotal, iva, total, manzana, lote, numeroOrden);

                if (folioId <= 0)
                {
                    MessageBox.Show("No se pudo guardar el folio en el repositorio", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2. Guardar detalle de insumos
                GuardarDetalleOrdenCompra(folioId, insumos);

                // 3. Guardar casas incluidas (si es orden múltiple)
                if (casas != null && casas.Count > 0)
                {
                    GuardarCasasOrdenCompra(folioId, casas);
                }

                // 4. Guardar PDF en BD
                if (File.Exists(rutaPdf))
                {
                    GuardarPDFOrdenCompraEnBD(folioId, folioOC, rutaPdf, tipoOrden, manzana, lote);
                }

                // Mensaje de éxito
                MessageBox.Show(
                    $"? Orden guardada en repositorio exitosamente\n\n" +
                    $"Folio: {folioOC}\n" +
                    $"Tipo: {tipoOrden}\n" +
                    $"Insumos: {insumos.Count}\n" +
                    $"Total: {total:C2}\n\n" +
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

        // MÉTODO: Abrir repositorio de PDFs (agregar botón en el diseñador)
        public void AbrirRepositorioPDFsOrdenesCompra(string manzana = null, string lote = null)
        {
            try
            {
                // Verificar que las tablas existan
                CrearTablasRepositorioSiNoExisten();

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
