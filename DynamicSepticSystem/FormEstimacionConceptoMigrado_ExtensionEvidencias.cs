using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Extensi�n para manejo de evidencias fotogr�ficas en PDFs de estimaci�n
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        /// <summary>
        /// Abre el formulario para seleccionar evidencias fotogr�ficas del repositorio
        /// </summary>
        private void btnSeleccionarEvidencias_Click(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar Manzana y Lote antes de agregar evidencias.", 
                    "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            try
            {
                // Obtener todas las evidencias disponibles para esta casa
                var evidenciasDisponibles = gestorEvidencias.ListarEvidencias(manzana, lote);

                if (evidenciasDisponibles.Count == 0)
                {
                    var result = MessageBox.Show(
                        $"No hay evidencias fotogr�ficas disponibles para M{manzana} L{lote}.\n\n" +
                        "�Deseas abrir el formulario de evidencias para agregar fotos?",
                        "Sin Evidencias", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        using (var formEvidencias = new FormEvidenciasFotograficas(manzana, lote))
                        {
                            formEvidencias.ShowDialog(this);
                        }
                        // Volver a intentar cargar despu�s de cerrar el formulario
                        evidenciasDisponibles = gestorEvidencias.ListarEvidencias(manzana, lote);
                        if (evidenciasDisponibles.Count == 0)
                            return;
                    }
                    else
                    {
                        return;
                    }
                }

                // Mostrar formulario de selecci�n
                using (var formSeleccion = new FormSeleccionEvidenciasPDF(evidenciasDisponibles, evidenciasSeleccionadas))
                {
                    if (formSeleccion.ShowDialog(this) == DialogResult.OK)
                    {
                        evidenciasSeleccionadas = formSeleccion.EvidenciasSeleccionadas;
                        ActualizarLabelEvidencias();
                        ActualizarPreviewPDF();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al seleccionar evidencias: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Actualiza el label que muestra cu�ntas evidencias est�n seleccionadas
        /// </summary>
        private void ActualizarLabelEvidencias()
        {
            if (this.Controls.ContainsKey("lblEvidenciasSeleccionadas"))
            {
                var lbl = this.Controls["lblEvidenciasSeleccionadas"] as Label;
                if (lbl != null)
                {
                    lbl.Text = evidenciasSeleccionadas.Count > 0
                        ? $"?? {evidenciasSeleccionadas.Count} foto(s) seleccionada(s)"
                        : "?? Sin evidencias";
                    lbl.ForeColor = evidenciasSeleccionadas.Count > 0
                        ? Color.FromArgb(39, 174, 96)
                        : Color.Gray;
                }
            }
        }

        /// <summary>
        /// Dibuja las evidencias fotogr�ficas en el PDF despu�s del contenido principal
        /// </summary>
        private void DibujarEvidenciasPDF(PdfDocument documento)
        {
            if (evidenciasSeleccionadas == null || evidenciasSeleccionadas.Count == 0)
                return;

            try
            {
                // Colores del tema
                XColor colorPrimario = XColor.FromArgb(ThemeManager.ColorPrincipal.R, 
                    ThemeManager.ColorPrincipal.G, ThemeManager.ColorPrincipal.B);
                XColor colorNaranja = XColor.FromArgb(230, 126, 34);
                XColor colorGris = XColor.FromArgb(236, 240, 241);

                XFont fontTitulo = new XFont("Arial", 14, XFontStyle.Bold);
                XFont fontSubtitulo = new XFont("Arial", 10, XFontStyle.Bold);
                XFont fontNormal = new XFont("Arial", 9);
                XFont fontPequena = new XFont("Arial", 8);

                // Crear nueva p�gina para evidencias
                PdfPage pagina = documento.AddPage();
                pagina.Size = PdfSharp.PageSize.Letter;
                XGraphics gfx = XGraphics.FromPdfPage(pagina);

                double margen = 50;
                double anchoUtil = pagina.Width - (margen * 2);
                double y = 40;

                // Encabezado de secci�n
                gfx.DrawRectangle(new XSolidBrush(colorNaranja), margen, y, anchoUtil, 25);
                gfx.DrawString("EVIDENCIAS FOTOGR�FICAS", fontTitulo, XBrushes.White, 
                    new XRect(margen, y, anchoUtil, 25), XStringFormats.Center);
                y += 35;

                // Informaci�n de la casa
                gfx.DrawString($"Manzana {cmbManzana.SelectedItem} - Lote {cmbLote.SelectedItem}", 
                    fontSubtitulo, XBrushes.Black, margen, y);
                y += 25;

                // Dibujar cada evidencia (2 por p�gina)
                int contador = 0;
                foreach (var evidencia in evidenciasSeleccionadas)
                {
                    // Si ya hay 2 evidencias en la p�gina, crear nueva p�gina
                    if (contador > 0 && contador % 2 == 0)
                    {
                        pagina = documento.AddPage();
                        pagina.Size = PdfSharp.PageSize.Letter;
                        gfx = XGraphics.FromPdfPage(pagina);
                        y = 40;
                    }

                    try
                    {
                        // Obtener imagen de la base de datos
                        byte[] fotoBytes = gestorEvidencias.ObtenerFoto(evidencia.Id);
                        
                        if (fotoBytes != null && fotoBytes.Length > 0)
                        {
                            // T�tulo de la evidencia
                            gfx.DrawRectangle(new XSolidBrush(colorGris), margen, y, anchoUtil, 18);
                            gfx.DrawString(evidencia.Titulo, fontSubtitulo, XBrushes.Black, margen + 5, y + 13);
                            y += 20;

                            // Informaci�n (fecha y usuario)
                            string info = $"Fecha: {evidencia.FechaFormateada} | Usuario: {evidencia.Usuario}";
                            gfx.DrawString(info, fontPequena, XBrushes.Gray, margen + 5, y + 10);
                            y += 15;

                            // Cargar y dibujar imagen
                            using (MemoryStream ms = new MemoryStream(fotoBytes))
                            {
                                XImage imagen = XImage.FromStream(ms);

                                // Calcular dimensiones manteniendo aspect ratio
                                double anchoMaxImg = anchoUtil - 10;
                                double altoMaxImg = 280; // Altura m�xima para permitir 2 fotos por p�gina
                                double ratio = Math.Min(anchoMaxImg / imagen.PixelWidth, altoMaxImg / imagen.PixelHeight);
                                double anchoFinal = imagen.PixelWidth * ratio;
                                double altoFinal = imagen.PixelHeight * ratio;

                                // Centrar imagen horizontalmente
                                double xImagen = margen + (anchoUtil - anchoFinal) / 2;

                                // Dibujar borde alrededor de la imagen
                                gfx.DrawRectangle(XPens.LightGray, xImagen - 2, y - 2, anchoFinal + 4, altoFinal + 4);
                                
                                // Dibujar imagen
                                gfx.DrawImage(imagen, xImagen, y, anchoFinal, altoFinal);
                                y += altoFinal + 25;
                            }
                        }
                        else
                        {
                            // Si no se pudo cargar la imagen, mostrar mensaje
                            gfx.DrawString($"[No se pudo cargar la imagen: {evidencia.Titulo}]", 
                                fontNormal, XBrushes.Red, margen + 5, y + 10);
                            y += 20;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Registrar error pero continuar con las dem�s evidencias
                        gfx.DrawString($"[Error al cargar evidencia: {ex.Message}]", 
                            fontNormal, XBrushes.Red, margen + 5, y + 10);
                        y += 20;
                    }

                    contador++;
                }

                // Pie de p�gina en la �ltima p�gina de evidencias
                y = pagina.Height - 40;
                gfx.DrawString($"Total de evidencias incluidas: {evidenciasSeleccionadas.Count}", 
                    fontPequena, XBrushes.Gray, 
                    new XRect(margen, y, anchoUtil, 20), XStringFormats.Center);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar evidencias en PDF: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Limpiar evidencias seleccionadas (llamar cuando se cambia de casa)
        /// </summary>
        private void LimpiarEvidenciasSeleccionadas()
        {
            evidenciasSeleccionadas.Clear();
            ActualizarLabelEvidencias();
        }
    }

    /// <summary>
    /// Formulario para seleccionar evidencias fotogr�ficas
    /// </summary>
    public class FormSeleccionEvidenciasPDF : Form
    {
        private CheckedListBox clbEvidencias;
        private Button btnAceptar;
        private Button btnCancelar;
        private Button btnVerPreview;
        private PictureBox picPreview;
        private Label lblInfo;
        private List<EvidenciaInfo> evidenciasDisponibles;
        private GestorEvidencias gestor;
        public List<EvidenciaInfo> EvidenciasSeleccionadas { get; private set; }

        public FormSeleccionEvidenciasPDF(List<EvidenciaInfo> disponibles, List<EvidenciaInfo> yaSeleccionadas)
        {
            evidenciasDisponibles = disponibles;
            EvidenciasSeleccionadas = new List<EvidenciaInfo>(yaSeleccionadas);
            
            gestor = new GestorEvidencias();

            InitializeComponent();
            ThemeManager.AplicarTema(this);
            CargarEvidencias();
        }

        private void InitializeComponent()
        {
            this.Text = "Seleccionar Evidencias Fotogr�ficas para PDF";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Panel izquierdo con lista
            var panelLista = new Panel
            {
                Dock = DockStyle.Left,
                Width = 450,
                Padding = new Padding(10)
            };

            lblInfo = new Label
            {
                Text = "Selecciona las evidencias a incluir en el PDF:",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ThemeManager.ColorPrincipal
            };

            clbEvidencias = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                Font = new Font("Segoe UI", 9)
            };
            clbEvidencias.SelectedIndexChanged += ClbEvidencias_SelectedIndexChanged;
            clbEvidencias.ItemCheck += ClbEvidencias_ItemCheck;

            panelLista.Controls.Add(clbEvidencias);
            panelLista.Controls.Add(lblInfo);

            // Panel derecho con preview
            var panelPreview = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lblPreview = new Label
            {
                Text = "Vista Previa:",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ThemeManager.ColorPrincipal
            };

            picPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            panelPreview.Controls.Add(picPreview);
            panelPreview.Controls.Add(lblPreview);

            // Panel de botones
            var panelBotones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            btnAceptar = new Button
            {
                Text = "? Aceptar",
                Width = 120,
                Height = 40,
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Right,
                BackColor = ThemeManager.ColorPrincipal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Location = new Point(panelBotones.Width - 140, 10);
            btnAceptar.Click += BtnAceptar_Click;

            btnCancelar = new Button
            {
                Text = "? Cancelar",
                Width = 120,
                Height = 40,
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Right,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Location = new Point(panelBotones.Width - 270, 10);

            btnVerPreview = new Button
            {
                Text = "?? Ver Completa",
                Width = 140,
                Height = 40,
                Anchor = AnchorStyles.Left,
                BackColor = ThemeManager.ColorSecundario,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 10)
            };
            btnVerPreview.FlatAppearance.BorderSize = 0;
            btnVerPreview.Click += BtnVerPreview_Click;

            panelBotones.Controls.Add(btnAceptar);
            panelBotones.Controls.Add(btnCancelar);
            panelBotones.Controls.Add(btnVerPreview);

            this.Controls.Add(panelPreview);
            this.Controls.Add(panelLista);
            this.Controls.Add(panelBotones);

            this.AcceptButton = btnAceptar;
            this.CancelButton = btnCancelar;

            // Aplicar tema
            ThemeManager.AplicarTema(this);
        }

        private void CargarEvidencias()
        {
            clbEvidencias.Items.Clear();
            
            foreach (var evidencia in evidenciasDisponibles)
            {
                string item = $"{evidencia.Titulo} ({evidencia.FechaFormateada})";
                clbEvidencias.Items.Add(item);
                
                // Marcar las que ya estaban seleccionadas
                if (EvidenciasSeleccionadas.Any(e => e.Id == evidencia.Id))
                {
                    clbEvidencias.SetItemChecked(clbEvidencias.Items.Count - 1, true);
                }
            }

            if (clbEvidencias.Items.Count > 0)
            {
                clbEvidencias.SelectedIndex = 0;
            }
        }

        private void ClbEvidencias_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (clbEvidencias.SelectedIndex < 0) return;

            try
            {
                var evidencia = evidenciasDisponibles[clbEvidencias.SelectedIndex];
                byte[] fotoBytes = gestor.ObtenerFoto(evidencia.Id);

                if (fotoBytes != null && fotoBytes.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(fotoBytes))
                    {
                        picPreview.Image?.Dispose();
                        picPreview.Image = new Bitmap(Image.FromStream(ms));
                    }
                }
            }
            catch
            {
                picPreview.Image = null;
            }
        }

        private void ClbEvidencias_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Actualizar despu�s del cambio
            this.BeginInvoke(new Action(() =>
            {
                lblInfo.Text = $"Seleccionadas: {clbEvidencias.CheckedItems.Count} de {clbEvidencias.Items.Count}";
            }));
        }

        private void BtnVerPreview_Click(object sender, EventArgs e)
        {
            if (picPreview.Image == null) return;

            var formZoom = new Form
            {
                Text = "Vista Completa",
                Size = new Size(800, 600),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White
            };

            var pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = new Bitmap(picPreview.Image),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            formZoom.Controls.Add(pic);
            formZoom.ShowDialog(this);
            pic.Image?.Dispose();
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            EvidenciasSeleccionadas.Clear();
            
            for (int i = 0; i < clbEvidencias.CheckedItems.Count; i++)
            {
                int index = clbEvidencias.CheckedIndices[i];
                EvidenciasSeleccionadas.Add(evidenciasDisponibles[index]);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            picPreview.Image?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
