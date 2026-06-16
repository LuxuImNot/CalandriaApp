// Inventory tracking functionality - FormAlmacen
// Muestra todas las entradas registradas en el almacén
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using ClosedXML.Excel;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        /// <summary>
        /// Carga todas las entradas registradas en el almacén desde la base de datos
        /// </summary>
        private void CargarInventarioCompleto()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Consulta para obtener todas las entradas con información detallada
                    // NOTA: La tabla EntradasAlmacen tiene: Cantidad, EsExcepcion (no CantidadOrdenada/Recibida/TieneExcepcion)
                    string sql = @"
                        SELECT 
                            e.FolioOC,
                            e.Clave,
                            e.Descripcion,
                            e.Unidad,
                            e.Cantidad,
                            e.PrecioUnitario,
                            e.Importe,
                            e.Manzana,
                            e.Lote,
                            e.Prototipo,
                            e.FechaEntrada,
                            e.Usuario,
                            e.Justificacion,
                            e.EsExcepcion,
                            CASE 
                                WHEN e.EsExcepcion = 1 THEN 'DISCREPANCIA'
                                ELSE 'COMPLETO'
                            END AS Estado
                        FROM EntradasAlmacen e
                        ORDER BY e.FechaEntrada DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        
                        dgvInventario.DataSource = dt;
                        
                        // Configurar columnas después de cargar datos
                        ConfigurarColumnasInventario();
                        
                        // Actualizar el contador
                        ActualizarContadorInventario(dt.Rows.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error al cargar el inventario:\n\n{ex.Message}",
                    "Error de Carga",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Configura las columnas del DataGridView de inventario
        /// </summary>
        private void ConfigurarColumnasInventario()
        {
            if (dgvInventario.Columns.Count == 0) return;

            // Configurar orden y ancho de columnas (SIN EMOJIS - Windows Forms no los soporta bien)
            var columnas = new[]
            {
                new { Name = "FechaEntrada", Header = "Fecha", Width = 130 },
                new { Name = "Estado", Header = "Estado", Width = 100 },
                new { Name = "FolioOC", Header = "Folio OC", Width = 120 },
                new { Name = "Clave", Header = "Clave", Width = 100 },
                new { Name = "Descripcion", Header = "Descripción", Width = 280 },
                new { Name = "Unidad", Header = "Unidad", Width = 70 },
                new { Name = "Cantidad", Header = "Cantidad", Width = 90 },
                new { Name = "PrecioUnitario", Header = "P.U.", Width = 90 },
                new { Name = "Importe", Header = "Importe", Width = 100 },
                new { Name = "Manzana", Header = "Manzana", Width = 80 },
                new { Name = "Lote", Header = "Lote", Width = 70 },
                new { Name = "Prototipo", Header = "Prototipo", Width = 120 },
                new { Name = "Usuario", Header = "Usuario", Width = 100 },
                new { Name = "EsExcepcion", Header = "Excepción", Width = 80 },
                new { Name = "Justificacion", Header = "Justificación", Width = 220 }
            };

            foreach (var col in columnas)
            {
                if (dgvInventario.Columns.Contains(col.Name))
                {
                    var column = dgvInventario.Columns[col.Name];
                    column.HeaderText = col.Header;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    column.Width = col.Width;
                    column.ReadOnly = true;
                }
            }

            // Formato de moneda
            if (dgvInventario.Columns.Contains("PrecioUnitario"))
            {
                dgvInventario.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
                dgvInventario.Columns["PrecioUnitario"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            
            if (dgvInventario.Columns.Contains("Importe"))
            {
                dgvInventario.Columns["Importe"].DefaultCellStyle.Format = "C2";
                dgvInventario.Columns["Importe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // Formato numérico para cantidades
            if (dgvInventario.Columns.Contains("Cantidad"))
            {
                dgvInventario.Columns["Cantidad"].DefaultCellStyle.Format = "N2";
                dgvInventario.Columns["Cantidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // Formato de fecha
            if (dgvInventario.Columns.Contains("FechaEntrada"))
            {
                dgvInventario.Columns["FechaEntrada"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }

            // Centrar columnas booleanas
            if (dgvInventario.Columns.Contains("EsExcepcion"))
            {
                dgvInventario.Columns["EsExcepcion"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // Colorear la columna Estado según el valor
            if (dgvInventario.Columns.Contains("Estado"))
            {
                dgvInventario.Columns["Estado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // Aplicar tema corporativo
            EstilizarDataGridViewAlmacen(dgvInventario);
            
            // Aplicar colores a las filas según el estado
            foreach (DataGridViewRow row in dgvInventario.Rows)
            {
                if (row.Cells["Estado"].Value != null)
                {
                    string estado = row.Cells["Estado"].Value.ToString();
                    if (estado == "DISCREPANCIA")
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230); // Rojo claro
                        row.Cells["Estado"].Style.ForeColor = Color.DarkRed;
                        row.Cells["Estado"].Style.Font = new Font(dgvInventario.Font, FontStyle.Bold);
                    }
                    else if (estado == "COMPLETO")
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230); // Verde claro
                        row.Cells["Estado"].Style.ForeColor = Color.DarkGreen;
                        row.Cells["Estado"].Style.Font = new Font(dgvInventario.Font, FontStyle.Bold);
                    }
                }
            }
        }

        /// <summary>
        /// Actualiza el contador de entradas en el label
        /// </summary>
        private void ActualizarContadorInventario(int total)
        {
            if (dgvInventario.DataSource is DataTable dt)
            {
                // Calcular estadísticas
                decimal totalImporte = 0;
                int conExcepcion = 0;
                
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Importe"] != DBNull.Value)
                        totalImporte += Convert.ToDecimal(row["Importe"]);
                    
                    if (row["EsExcepcion"] != DBNull.Value && Convert.ToBoolean(row["EsExcepcion"]))
                        conExcepcion++;
                }

                lblTotalInventario.Text = $"Total de entradas: {total} | Importe total: {totalImporte:C2}";
                
                if (conExcepcion > 0)
                {
                    lblTotalInventario.Text += $" | Con discrepancia: {conExcepcion}";
                    lblTotalInventario.ForeColor = Color.OrangeRed;
                }
                else
                {
                    lblTotalInventario.ForeColor = ThemeManager.ColorExito;
                }
            }
        }

        /// <summary>
        /// Filtra el inventario según el texto de búsqueda
        /// </summary>
        private void TxtBuscarInventario_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvInventario.DataSource is DataTable dt)
                {
                    string filtro = txtBuscarInventario.Text.Trim();
                    
                    if (string.IsNullOrEmpty(filtro))
                    {
                        dt.DefaultView.RowFilter = string.Empty;
                    }
                    else
                    {
                        // Buscar en múltiples columnas
                        string rowFilter = $@"
                            Clave LIKE '%{filtro}%' OR 
                            Descripcion LIKE '%{filtro}%' OR 
                            FolioOC LIKE '%{filtro}%' OR 
                            Manzana LIKE '%{filtro}%' OR 
                            Lote LIKE '%{filtro}%' OR 
                            Prototipo LIKE '%{filtro}%' OR
                            Usuario LIKE '%{filtro}%' OR
                            Justificacion LIKE '%{filtro}%'";
                        
                        dt.DefaultView.RowFilter = rowFilter;
                    }
                    
                    // Actualizar contador con resultados filtrados
                    ActualizarContadorInventario(dt.DefaultView.Count);
                }
            }
            catch (Exception)
            {
                // Si hay error en el filtro, limpiar
                if (dgvInventario.DataSource is DataTable dt)
                {
                    dt.DefaultView.RowFilter = string.Empty;
                }
            }
        }

        /// <summary>
        /// Botón para actualizar manualmente el inventario
        /// </summary>
        private void BtnActualizarInventario_Click(object sender, EventArgs e)
        {
            CargarInventarioCompleto();
            MessageBox.Show(
                "✅ Inventario actualizado correctamente",
                "Actualización Completa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Exporta el inventario a un archivo Excel
        /// </summary>
        private void BtnExportarInventario_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvInventario.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "⚠️ No hay datos para exportar",
                        "Sin Datos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                    FileName = $"Inventario_Almacen_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    Title = "Guardar Inventario"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportarInventarioExcel(saveDialog.FileName);
                    
                    var result = MessageBox.Show(
                        $"✅ Inventario exportado exitosamente:\n\n{saveDialog.FileName}\n\n¿Desea abrir el archivo?",
                        "Exportación Exitosa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information
                    );

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error al exportar a Excel:\n\n{ex.Message}",
                    "Error de Exportación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Exporta los datos del inventario a un archivo Excel usando ClosedXML
        /// </summary>
        private void ExportarInventarioExcel(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventario");
                
                // Título principal
                worksheet.Cell(1, 1).Value = "INVENTARIO DE ALMACÉN - CALANDRIA RESIDENCIAL";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(179, 108, 46); // Color corporativo
                worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
                worksheet.Range(1, 1, 1, 14).Merge();
                
                // Fecha de generación
                worksheet.Cell(2, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cell(2, 1).Style.Font.Italic = true;
                worksheet.Range(2, 1, 2, 14).Merge();

                // Encabezados de columnas (fila 4)
                int row = 4;
                int col = 1;
                
                var headers = new[]
                {
                    "Fecha", "Estado", "Folio OC", "Clave", "Descripción", "Unidad",
                    "Cantidad", "P.U.", "Importe",
                    "Manzana", "Lote", "Prototipo", "Usuario", "Justificación"
                };

                foreach (var header in headers)
                {
                    worksheet.Cell(row, col).Value = header;
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    col++;
                }

                // Datos
                row = 5;
                foreach (DataGridViewRow dgvRow in dgvInventario.Rows)
                {
                    if (dgvRow.IsNewRow) continue;

                    col = 1;
                    // Convertir valores explícitamente a string para evitar errores de tipo
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["FechaEntrada"].Value?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["Estado"].Value?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["FolioOC"].Value?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["Clave"].Value?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["Descripcion"].Value?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["Unidad"].Value?.ToString() ?? "";
                    
                    // Valores numéricos - convertir explícitamente
                    var cantidad = dgvRow.Cells["Cantidad"].Value;
                    worksheet.Cell(row, col++).Value = cantidad != null && cantidad != DBNull.Value 
                        ? Convert.ToDouble(cantidad) : 0;
                    
                    var precioUnitario = dgvRow.Cells["PrecioUnitario"].Value;
                    worksheet.Cell(row, col++).Value = precioUnitario != null && precioUnitario != DBNull.Value 
                        ? Convert.ToDouble(precioUnitario) : 0;
                    
                    var importe = dgvRow.Cells["Importe"].Value;
                    worksheet.Cell(row, col++).Value = importe != null && importe != DBNull.Value 
                        ? Convert.ToDouble(importe) : 0;
                    
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["Manzana"].Value?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["Lote"].Value?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["Prototipo"].Value?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["Usuario"].Value?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = dgvRow.Cells["Justificacion"].Value?.ToString() ?? "";
                    
                    row++;
                }

                // Ajustar anchos de columna
                worksheet.Columns().AdjustToContents();
                
                // Aplicar formatos de moneda
                worksheet.Column(8).Style.NumberFormat.Format = "$#,##0.00"; // P.U.
                worksheet.Column(9).Style.NumberFormat.Format = "$#,##0.00"; // Importe
                
                // Aplicar formatos numéricos
                worksheet.Column(7).Style.NumberFormat.Format = "0.00"; // Cantidad

                // Guardar archivo
                workbook.SaveAs(filePath);
            }
        }
    }
}
