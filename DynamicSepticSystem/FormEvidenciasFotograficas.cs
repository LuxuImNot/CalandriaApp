using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    public partial class FormEvidenciasFotograficas : Form
    {
        private string connectionString;
        private GestorEvidencias gestorEvidencias;
        private Image imagenSeleccionada;
        private byte[] imagenBytes;

        public FormEvidenciasFotograficas()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            gestorEvidencias = new GestorEvidencias(connectionString);
            
            // Aplicar tema
            ThemeManager.AplicarTema(this);
        }

        public FormEvidenciasFotograficas(string manzana, string lote)
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            gestorEvidencias = new GestorEvidencias(connectionString);
            
            // Aplicar tema
            ThemeManager.AplicarTema(this);
            
            // Pre-seleccionar manzana y lote
            this.Load += (s, e) =>
            {
                cmbManzana.SelectedItem = manzana;
                cmbLote.SelectedItem = lote;
            };
        }

        private void FormEvidenciasFotograficas_Load(object sender, EventArgs e)
        {
            try
            {
                CargarManzanas();
                ConfigurarDataGridView();
                dgvEvidencias.CellClick += dgvEvidencias_CellClick;
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el formulario: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Carga de Datos

        private void CargarManzanas()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbManzana.Items.Clear();
                        while (reader.Read())
                        {
                            cmbManzana.Items.Add(reader["Manzana"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar manzanas: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarLotes(string manzana)
        {
            try
            {
                cmbLote.Items.Clear();
                cmbLote.Text = "";

                if (string.IsNullOrEmpty(manzana))
                    return;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Lote FROM InventarioCasas WHERE Manzana = @m ORDER BY Lote";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cmbLote.Items.Add(reader["Lote"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar lotes: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarEvidencias()
        {
            try
            {
                if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
                {
                    dgvEvidencias.DataSource = null;
                    lblEstadisticas.Text = "Total evidencias: 0";
                    return;
                }

                string manzana = cmbManzana.SelectedItem.ToString();
                string lote = cmbLote.SelectedItem.ToString();

                var evidencias = gestorEvidencias.ListarEvidencias(manzana, lote);

                var dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(int));
                dataTable.Columns.Add("Título", typeof(string));
                dataTable.Columns.Add("Fecha", typeof(string));
                dataTable.Columns.Add("Tamaño", typeof(string));
                dataTable.Columns.Add("Usuario", typeof(string));

                foreach (var ev in evidencias)
                {
                    dataTable.Rows.Add(
                        ev.Id,
                        ev.Titulo,
                        ev.FechaFormateada,
                        ev.TamañoFormateado,
                        ev.Usuario
                    );
                }

                dgvEvidencias.DataSource = dataTable;

                // Ocultar columna Id
                if (dgvEvidencias.Columns["Id"] != null)
                    dgvEvidencias.Columns["Id"].Visible = false;

                // Seleccionar la primera fila para actualizar la vista previa automáticamente
                dgvEvidencias.ClearSelection();
                if (dgvEvidencias.Rows.Count > 0)
                {
                    try
                    {
                        dgvEvidencias.Rows[0].Selected = true;
                        // Establecer CurrentCell para asegurar que la fila seleccionada es visible y dispara eventos apropiados
                        if (dgvEvidencias.Rows[0].Cells.Count > 0 && dgvEvidencias.Rows[0].Cells[0] != null)
                        {
                            dgvEvidencias.CurrentCell = dgvEvidencias.Rows[0].Cells[0];
                        }

                        // Llamar al handler para cargar la vista previa inmediatamente
                        dgvEvidencias_SelectionChanged(dgvEvidencias, EventArgs.Empty);
                    }
                    catch
                    {
                        // ignorar errores de selección
                    }
                }

                lblEstadisticas.Text = $"Total evidencias: {evidencias.Count} | " +
                    $"Espacio: {evidencias.Sum(e => e.TamañoKB) / 1024:F2} MB";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar evidencias: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Configuración UI

        private void ConfigurarDataGridView()
        {
            dgvEvidencias.AutoGenerateColumns = true;
            dgvEvidencias.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEvidencias.MultiSelect = false;
            dgvEvidencias.ReadOnly = true;
            dgvEvidencias.AllowUserToAddRows = false;
            dgvEvidencias.AllowUserToDeleteRows = false;
            dgvEvidencias.RowHeadersVisible = false;
            
            // Aplicar estilos del tema
            ThemeManager.AplicarTemaControl(dgvEvidencias);
        }

        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvEvidencias.SelectedRows.Count > 0;
            bool hayImagen = imagenSeleccionada != null;
            bool hayTexto = txtTituloFoto.Text.Trim().Length >= 3;
            bool hayLoteSeleccionado = cmbManzana.SelectedItem != null && cmbLote.SelectedItem != null;

            btnVerFoto.Enabled = haySeleccion;
            btnEliminarFoto.Enabled = haySeleccion;
            btnGuardarFoto.Enabled = hayImagen && hayTexto && hayLoteSeleccionado;
            btnCapturarFoto.Enabled = hayLoteSeleccionado;
        }

        #endregion

        #region Eventos de Controles

        private void cmbManzana_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem != null)
            {
                CargarLotes(cmbManzana.SelectedItem.ToString());
                LimpiarFormulario();
            }
        }

        private void cmbLote_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLote.SelectedItem != null)
            {
                CargarEvidencias();
                LimpiarFormulario();
            }
        }

        private void txtTituloFoto_TextChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        private void dgvEvidencias_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();

            try
            {
                if (dgvEvidencias.SelectedRows.Count == 0)
                {
                    // clear preview
                    if (imagenSeleccionada != null)
                    {
                        imagenSeleccionada.Dispose();
                        imagenSeleccionada = null;
                    }
                    picVistaPrevia.Image = null;
                    lblInfoImagen.Text = "Selecciona una foto para ver la vista previa";
                    return;
                }

                // Try to get Id from selected row(s) robustly
                DataGridViewRow row = dgvEvidencias.SelectedRows.Count > 0 ? dgvEvidencias.SelectedRows[0] : dgvEvidencias.CurrentRow;
                if (row == null)
                {
                    lblInfoImagen.Text = "Selecciona una foto para ver la vista previa";
                    return;
                }

                object idObj = null;
                if (dgvEvidencias.Columns["Id"] != null)
                    idObj = row.Cells[dgvEvidencias.Columns["Id"].Index].Value;
                else if (row.Cells.Count > 0)
                    idObj = row.Cells[0].Value;

                if (idObj == null) return;

                int idEvidencia;
                if (!int.TryParse(idObj.ToString(), out idEvidencia)) return;

                // Load preview by id
                MostrarPreviewPorId(idEvidencia);

                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                // No interrumpir la selección, solo mostrar mensaje
                MessageBox.Show($"Error al cargar vista previa: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvEvidencias_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                var row = dgvEvidencias.Rows[e.RowIndex];
                object idObj = null;
                if (dgvEvidencias.Columns["Id"] != null)
                    idObj = row.Cells[dgvEvidencias.Columns["Id"].Index].Value;
                else if (row.Cells.Count > 0)
                    idObj = row.Cells[0].Value;
                if (idObj == null) return;
                if (int.TryParse(idObj.ToString(), out int idEvidencia))
                {
                    MostrarPreviewPorId(idEvidencia);
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Carga la imagen de la evidencia indicada por id y actualiza la vista previa.
        /// </summary>
        /// <param name="idEvidencia"></param>
        private void MostrarPreviewPorId(int idEvidencia)
        {
            try
            {
                byte[] fotoBytes = gestorEvidencias.ObtenerFoto(idEvidencia);
                if (fotoBytes != null && fotoBytes.Length > 0)
                {
                    // Dispose previous
                    if (imagenSeleccionada != null)
                    {
                        imagenSeleccionada.Dispose();
                        imagenSeleccionada = null;
                    }

                    using (var ms = new MemoryStream(fotoBytes))
                    using (var tmp = Image.FromStream(ms))
                    {
                        // Create independent copy
                        var bmpPreview = new Bitmap(tmp);

                        // Assign to picture box (dispose previous image first)
                        picVistaPrevia.Image?.Dispose();
                        picVistaPrevia.Image = new Bitmap(bmpPreview);

                        // Keep a separate copy for internal use
                        imagenSeleccionada = new Bitmap(bmpPreview);

                        // Also show in FormAvanceObra if it's open
                        ShowInFormAvance(imagenSeleccionada);

                        double tamañoKB = fotoBytes.Length / 1024.0;
                        lblInfoImagen.Text = $"Imagen seleccionada: {imagenSeleccionada.Width}x{imagenSeleccionada.Height}px | Tamaño: {tamañoKB:F1} KB";
                    }
                }
                else
                {
                    // No image
                    picVistaPrevia.Image?.Dispose();
                    picVistaPrevia.Image = null;

                    if (imagenSeleccionada != null)
                    {
                        imagenSeleccionada.Dispose();
                        imagenSeleccionada = null;
                    }

                    ShowInFormAvance(null);
                    lblInfoImagen.Text = "No se pudo cargar la vista previa de la imagen";
                }
            }
            catch
            {
                // on any error, clear preview
                try { picVistaPrevia.Image?.Dispose(); } catch { }
                picVistaPrevia.Image = null;
                try { imagenSeleccionada?.Dispose(); } catch { }
                imagenSeleccionada = null;
                ShowInFormAvance(null);
                lblInfoImagen.Text = "No se pudo cargar la vista previa de la imagen";
            }
        }

        private void dgvEvidencias_DoubleClick(object sender, EventArgs e)
        {
            btnVerFoto_Click(sender, e);
        }

        private void picVistaPrevia_DoubleClick(object sender, EventArgs e)
        {
            if (imagenSeleccionada != null)
            {
                MostrarImagenCompleta(imagenSeleccionada);
            }
        }

        #endregion

        #region Captura y Guardado

        private void btnCapturarFoto_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Seleccionar Foto";
                    ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp|Todos los archivos|*.*";
                    ofd.FilterIndex = 1;

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        // Validar tamaño del archivo
                        FileInfo fileInfo = new FileInfo(ofd.FileName);
                        if (fileInfo.Length > 10 * 1024 * 1024) // 10 MB
                        {
                            MessageBox.Show("El archivo no puede superar los 10 MB", "Archivo muy grande", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Cargar imagen
                        byte[] bytesOriginales = File.ReadAllBytes(ofd.FileName);
                        
                        // Redimensionar si es necesario
                        imagenBytes = GestorEvidencias.RedimensionarImagen(bytesOriginales, 1024, 768);

                        // Mostrar en vista previa
                        if (imagenSeleccionada != null)
                        {
                            imagenSeleccionada.Dispose();
                        }

                        using (MemoryStream ms = new MemoryStream(imagenBytes))
                        using (var tmp = Image.FromStream(ms))
                        {
                            var bmpPreview = new Bitmap(tmp);

                            picVistaPrevia.Image?.Dispose();
                            picVistaPrevia.Image = new Bitmap(bmpPreview);

                            // Keep an independent copy
                            imagenSeleccionada = new Bitmap(bmpPreview);
                        }

                        // También mostrar en FormAvanceObra si está abierto
                        ShowInFormAvance(imagenSeleccionada);

                        double tamañoKB = imagenBytes.Length / 1024.0;
                        lblInfoImagen.Text = $"Imagen cargada: {imagenSeleccionada.Width}x{imagenSeleccionada.Height}px | " +
                            $"Tamaño: {tamañoKB:F1} KB";

                        ActualizarEstadoBotones();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar imagen: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuardarFoto_Click(object sender, EventArgs e)
        {
            try
            {
                // Validaciones
                if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar Manzana y Lote", "Validación", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtTituloFoto.Text) || txtTituloFoto.Text.Trim().Length < 3)
                {
                    MessageBox.Show("El título debe tener al menos 3 caracteres", "Validación", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTituloFoto.Focus();
                    return;
                }

                if (imagenBytes == null || imagenBytes.Length == 0)
                {
                    MessageBox.Show("Debe seleccionar una foto", "Validación", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Guardar en base de datos
                string manzana = cmbManzana.SelectedItem.ToString();
                string lote = cmbLote.SelectedItem.ToString();
                string titulo = txtTituloFoto.Text.Trim();
                string usuario = Environment.UserName;

                this.Cursor = Cursors.WaitCursor;
                btnGuardarFoto.Enabled = false;

                bool resultado = gestorEvidencias.GuardarEvidencia(manzana, lote, titulo, imagenBytes, "jpg", usuario);

                this.Cursor = Cursors.Default;

                if (resultado)
                {
                    MessageBox.Show("Evidencia guardada exitosamente", "Éxito", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Recargar lista y limpiar
                    CargarEvidencias();
                    LimpiarFormulario();
                }
                else
                {
                    MessageBox.Show("No se pudo guardar la evidencia", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnGuardarFoto.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                btnGuardarFoto.Enabled = true;
                MessageBox.Show($"Error al guardar evidencia: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Visualización y Eliminación

        private void btnVerFoto_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvEvidencias.SelectedRows.Count == 0)
                    return;

                int idEvidencia = Convert.ToInt32(dgvEvidencias.SelectedRows[0].Cells["Id"].Value);
                byte[] fotoBytes = gestorEvidencias.ObtenerFoto(idEvidencia);

                if (fotoBytes != null && fotoBytes.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(fotoBytes))
                    {
                        Image imagen = Image.FromStream(ms);
                        MostrarImagenCompleta(imagen);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ver foto: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminarFoto_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvEvidencias.SelectedRows.Count == 0)
                    return;

                int idEvidencia = Convert.ToInt32(dgvEvidencias.SelectedRows[0].Cells["Id"].Value);
                string titulo = dgvEvidencias.SelectedRows[0].Cells["Título"].Value.ToString();

                var result = MessageBox.Show(
                    $"¿Está seguro de eliminar la evidencia '{titulo}'?\n\nEsta acción no se puede deshacer.",
                    "Confirmar Eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    bool eliminado = gestorEvidencias.EliminarEvidencia(idEvidencia);

                    if (eliminado)
                    {
                        MessageBox.Show("Evidencia eliminada exitosamente", "Éxito", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarEvidencias();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar la evidencia", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar evidencia: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarImagenCompleta(Image imagen)
        {
            Form visor = new Form
            {
                Text = "Visor de Imagen",
                Size = new Size(800, 600),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.Black
            };

            PictureBox pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = new Bitmap(imagen),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            visor.Controls.Add(pic);
            visor.ShowDialog();
            pic.Image?.Dispose();
        }

        #endregion

        #region Exportar PDF

        private void btnExportarPDF_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar Manzana y Lote", "Atención", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var evidencias = gestorEvidencias.ListarEvidencias(
                    cmbManzana.SelectedItem.ToString(),
                    cmbLote.SelectedItem.ToString());

                if (evidencias.Count == 0)
                {
                    MessageBox.Show("No hay evidencias para exportar", "Atención", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    FileName = $"Evidencias_M{cmbManzana.SelectedItem}_L{cmbLote.SelectedItem}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    GenerarPDFEvidencias(sfd.FileName, evidencias);
                    this.Cursor = Cursors.Default;

                    MessageBox.Show("PDF generado exitosamente", "Éxito", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var result = MessageBox.Show("¿Desea abrir el PDF?", "Abrir PDF", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarPDFEvidencias(string rutaPdf, List<EvidenciaInfo> evidencias)
        {
            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = $"Evidencias Fotográficas - M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem}";
            pdf.Info.Author = "Sistema Calandria Residencial";
            pdf.Info.Subject = "Reporte de Evidencias Fotográficas";

            // Usar colores del tema
            XColor colorPrimario = XColor.FromArgb(ThemeManager.ColorPrincipal.R, 
                ThemeManager.ColorPrincipal.G, ThemeManager.ColorPrincipal.B);
            XColor colorSecundario = XColor.FromArgb(ThemeManager.ColorSecundario.R, 
                ThemeManager.ColorSecundario.G, ThemeManager.ColorSecundario.B);
                
            XFont fontTitulo = new XFont("Arial", 16, XFontStyle.Bold);
            XFont fontSubtitulo = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 10);
            XFont fontPequena = new XFont("Arial", 8);

            // Portada
            PdfPage portada = pdf.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(portada);

            gfx.DrawRectangle(new XSolidBrush(colorPrimario), 0, 0, portada.Width, 100);
            gfx.DrawString("EVIDENCIAS FOTOGRÁFICAS", new XFont("Arial", 20, XFontStyle.Bold), 
                XBrushes.White, new XRect(0, 30, portada.Width, 40), XStringFormats.TopCenter);

            double y = 130;
            gfx.DrawString($"Manzana: {cmbManzana.SelectedItem}", fontSubtitulo, XBrushes.Black, 50, y);
            y += 30;
            gfx.DrawString($"Lote: {cmbLote.SelectedItem}", fontSubtitulo, XBrushes.Black, 50, y);
            y += 30;
            gfx.DrawString($"Fecha de reporte: {DateTime.Now:dd/MM/yyyy HH:mm}", fontNormal, XBrushes.Black, 50, y);
            y += 25;
            gfx.DrawString($"Total de evidencias: {evidencias.Count}", fontNormal, XBrushes.Black, 50, y);

            // Páginas de evidencias (2 por página)
            int contador = 0;
            foreach (var evidencia in evidencias)
            {
                if (contador % 2 == 0)
                {
                    portada = pdf.AddPage();
                    gfx = XGraphics.FromPdfPage(portada);
                    y = 50;
                }

                // Obtener imagen
                byte[] fotoBytes = gestorEvidencias.ObtenerFoto(evidencia.Id);
                if (fotoBytes != null && fotoBytes.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(fotoBytes))
                    {
                        XImage imagen = XImage.FromStream(ms);

                        // Título
                        gfx.DrawString(evidencia.Titulo, fontSubtitulo, new XSolidBrush(colorPrimario), 50, y);
                        y += 20;

                        // Información
                        gfx.DrawString($"Fecha: {evidencia.FechaFormateada} | Usuario: {evidencia.Usuario}", 
                            fontPequena, XBrushes.Gray, 50, y);
                        y += 20;

                        // Imagen
                        double anchoMax = portada.Width - 100;
                        double altoMax = 300;
                        double ratio = Math.Min(anchoMax / imagen.PixelWidth, altoMax / imagen.PixelHeight);
                        double anchoFinal = imagen.PixelWidth * ratio;
                        double altoFinal = imagen.PixelHeight * ratio;

                        gfx.DrawImage(imagen, 50, y, anchoFinal, altoFinal);
                        y += altoFinal + 30;
                    }
                }

                contador++;
            }

            pdf.Save(rutaPdf);
        }

        #endregion

        #region Utilidades

        private void LimpiarFormulario()
        {
            txtTituloFoto.Clear();
            
            if (imagenSeleccionada != null)
            {
                imagenSeleccionada.Dispose();
                imagenSeleccionada = null;
            }

            picVistaPrevia.Image = null;
            imagenBytes = null;
            lblInfoImagen.Text = "Selecciona una foto para ver la vista previa";
            
            // Clear preview in FormAvanceObra as well
            ShowInFormAvance(null);
            
            ActualizarEstadoBotones();
        }

        private void ShowInFormAvance(Image image)
        {
            try
            {
                var forms = Application.OpenForms.OfType<FormHardProgress>().ToList();
                foreach (var frm in forms)
                {
                    try
                    {
                        // Pass the image (FormHardProgress will copy it internally)
                        frm.ShowEvidenciaPreview(image);
                    }
                    catch { }
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (imagenSeleccionada != null)
            {
                imagenSeleccionada.Dispose();
            }

            base.OnFormClosing(e);
        }

        #endregion
    }
}
