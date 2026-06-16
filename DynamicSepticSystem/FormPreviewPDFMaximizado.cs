using System;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Formulario para mostrar el preview del PDF maximizado
    /// </summary>
    public class FormPreviewPDFMaximizado : Form
    {
        private PictureBox pictureBoxPreview;
        private Panel panelControles;
        private Button btnCerrar;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnZoomFit;
        private Label lblZoom;
        private TrackBar trackBarZoom;
        
        private float zoomLevel = 1.0f;
        private Image imagenOriginal;

        public FormPreviewPDFMaximizado(Image imagenPreview)
        {
            if (imagenPreview == null)
                throw new ArgumentNullException(nameof(imagenPreview));

            imagenOriginal = new Bitmap(imagenPreview);
            
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            MostrarImagen();
        }

        private void InitializeComponent()
        {
            // Configuraci�n del formulario
            this.Text = "Vista Previa de Estimaci�n - PDF";
            this.Size = new Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(44, 62, 80);
            this.KeyPreview = true;
            this.KeyDown += FormPreviewPDFMaximizado_KeyDown;

            // Panel de controles superior
            panelControles = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(10)
            };

            // Bot�n Cerrar
            btnCerrar = new Button
            {
                Text = "? Cerrar",
                Size = new Size(100, 40),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => this.Close();

            // Bot�n Zoom In
            btnZoomIn = new Button
            {
                Text = "??+",
                Size = new Size(80, 40),
                Location = new Point(120, 10),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnZoomIn.FlatAppearance.BorderSize = 0;
            btnZoomIn.Click += BtnZoomIn_Click;

            // Bot�n Zoom Out
            btnZoomOut = new Button
            {
                Text = "??-",
                Size = new Size(80, 40),
                Location = new Point(210, 10),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnZoomOut.FlatAppearance.BorderSize = 0;
            btnZoomOut.Click += BtnZoomOut_Click;

            // Bot�n Ajustar
            btnZoomFit = new Button
            {
                Text = "? Ajustar",
                Size = new Size(100, 40),
                Location = new Point(300, 10),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnZoomFit.FlatAppearance.BorderSize = 0;
            btnZoomFit.Click += BtnZoomFit_Click;

            // TrackBar Zoom
            trackBarZoom = new TrackBar
            {
                Location = new Point(410, 10),
                Size = new Size(250, 40),
                Minimum = 50,
                Maximum = 300,
                Value = 100,
                TickFrequency = 25,
                TickStyle = TickStyle.BottomRight,
                BackColor = Color.FromArgb(52, 73, 94)
            };
            trackBarZoom.ValueChanged += TrackBarZoom_ValueChanged;

            // Label Zoom
            lblZoom = new Label
            {
                Text = "100%",
                Location = new Point(670, 20),
                Size = new Size(60, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // PictureBox para mostrar la imagen
            pictureBoxPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(44, 62, 80),
                Cursor = Cursors.Hand
            };
            pictureBoxPreview.MouseWheel += PictureBoxPreview_MouseWheel;

            // Agregar controles al panel
            panelControles.Controls.AddRange(new Control[] 
            { 
                btnCerrar, btnZoomIn, btnZoomOut, btnZoomFit, trackBarZoom, lblZoom 
            });

            // Agregar controles al formulario
            this.Controls.Add(pictureBoxPreview);
            this.Controls.Add(panelControles);

            // Instrucciones
            var lblInstrucciones = new Label
            {
                Text = "?? Usa la rueda del mouse para hacer zoom | ESC para cerrar",
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.FromArgb(149, 165, 166),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblInstrucciones);
        }

        private void FormPreviewPDFMaximizado_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
            {
                AumentarZoom();
            }
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                DisminuirZoom();
            }
            else if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0)
            {
                AjustarZoom();
            }
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            AumentarZoom();
        }

        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            DisminuirZoom();
        }

        private void BtnZoomFit_Click(object sender, EventArgs e)
        {
            AjustarZoom();
        }

        private void TrackBarZoom_ValueChanged(object sender, EventArgs e)
        {
            zoomLevel = trackBarZoom.Value / 100f;
            lblZoom.Text = $"{trackBarZoom.Value}%";
            ActualizarZoom();
        }

        private void PictureBoxPreview_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                AumentarZoom();
            }
            else
            {
                DisminuirZoom();
            }
        }

        private void AumentarZoom()
        {
            int nuevoValor = Math.Min(trackBarZoom.Value + 25, trackBarZoom.Maximum);
            trackBarZoom.Value = nuevoValor;
        }

        private void DisminuirZoom()
        {
            int nuevoValor = Math.Max(trackBarZoom.Value - 25, trackBarZoom.Minimum);
            trackBarZoom.Value = nuevoValor;
        }

        private void AjustarZoom()
        {
            pictureBoxPreview.SizeMode = PictureBoxSizeMode.Zoom;
            trackBarZoom.Value = 100;
        }

        private void ActualizarZoom()
        {
            if (imagenOriginal == null) return;

            try
            {
                pictureBoxPreview.SizeMode = PictureBoxSizeMode.CenterImage;
                
                int nuevoAncho = (int)(imagenOriginal.Width * zoomLevel);
                int nuevoAlto = (int)(imagenOriginal.Height * zoomLevel);

                if (nuevoAncho > 0 && nuevoAlto > 0)
                {
                    var imagenZoom = new Bitmap(nuevoAncho, nuevoAlto);
                    using (var g = Graphics.FromImage(imagenZoom))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(imagenOriginal, 0, 0, nuevoAncho, nuevoAlto);
                    }

                    if (pictureBoxPreview.Image != null && pictureBoxPreview.Image != imagenOriginal)
                    {
                        pictureBoxPreview.Image.Dispose();
                    }
                    
                    pictureBoxPreview.Image = imagenZoom;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar zoom: {ex.Message}");
            }
        }

        private void MostrarImagen()
        {
            if (imagenOriginal != null)
            {
                pictureBoxPreview.Image = new Bitmap(imagenOriginal);
                pictureBoxPreview.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (pictureBoxPreview.Image != null && pictureBoxPreview.Image != imagenOriginal)
                {
                    pictureBoxPreview.Image.Dispose();
                }
                
                if (imagenOriginal != null)
                {
                    imagenOriginal.Dispose();
                    imagenOriginal = null;
                }
            }
            catch { }

            base.OnFormClosing(e);
        }
    }
}
