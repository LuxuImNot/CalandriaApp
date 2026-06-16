using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Ventana de "Propiedades" de un concepto o partida en la que se pueden
    /// adjuntar, visualizar y eliminar una o varias fotos asociadas a la casa
    /// (Manzana + Lote) seleccionada.
    /// </summary>
    public class FormPropiedadesConcepto : Form
    {
        private readonly GestorFotosConcepto gestor;
        private readonly string manzana;
        private readonly string lote;
        private readonly string identificador;
        private readonly bool esConcepto;
        private readonly string nombreNodo;

        private FlowLayoutPanel panelGaleria;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblContador;
        private Button btnAgregar;
        private Button btnCerrar;

        public FormPropiedadesConcepto(string manzana, string lote, string identificador,
            bool esConcepto, string nombreNodo)
        {
            this.manzana = manzana;
            this.lote = lote;
            this.identificador = identificador;
            this.esConcepto = esConcepto;
            this.nombreNodo = nombreNodo;
            this.gestor = new GestorFotosConcepto();

            InitializeComponent();
            ThemeManager.AplicarTema(this);
            CargarFotos();
        }

        private void InitializeComponent()
        {
            this.Text = "Propiedades - Fotos del " + (esConcepto ? "Concepto" : "Partida");
            this.Size = new Size(820, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(640, 480);
            this.BackColor = Color.White;

            // Encabezado
            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = ThemeManager.ColorPrincipal,
                Padding = new Padding(15, 8, 15, 8)
            };

            lblTitulo = new Label
            {
                Text = (esConcepto ? "📋 " : "🔧 ") + nombreNodo,
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblSubtitulo = new Label
            {
                Text = $"Manzana {manzana} · Lote {lote}   |   Clave: {identificador}",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(230, 230, 230),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelHeader.Controls.Add(lblSubtitulo);
            panelHeader.Controls.Add(lblTitulo);

            // Barra de botones
            var panelBotones = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(245, 247, 249),
                Padding = new Padding(15, 10, 15, 10)
            };

            btnAgregar = new Button
            {
                Text = "➕ Agregar Fotos",
                Width = 160,
                Height = 36,
                Location = new Point(15, 10),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Click += BtnAgregar_Click;

            lblContador = new Label
            {
                Text = "0 foto(s)",
                Location = new Point(190, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(127, 140, 141)
            };

            panelBotones.Controls.Add(btnAgregar);
            panelBotones.Controls.Add(lblContador);

            // Galería de fotos
            panelGaleria = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(12),
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            // Pie con botón cerrar
            var panelPie = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.FromArgb(245, 247, 249),
                Padding = new Padding(15, 10, 15, 10)
            };

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Width = 120,
                Height = 36,
                Anchor = AnchorStyles.Right,
                Location = new Point(panelPie.Width - 135, 10),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                DialogResult = DialogResult.OK,
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            panelPie.Controls.Add(btnCerrar);

            this.Controls.Add(panelGaleria);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelPie);

            this.AcceptButton = btnCerrar;
            this.CancelButton = btnCerrar;
        }

        /// <summary>
        /// Carga (o recarga) las miniaturas de las fotos asociadas.
        /// </summary>
        private void CargarFotos()
        {
            // Liberar imágenes previas
            foreach (Control c in panelGaleria.Controls)
            {
                if (c is Panel p && p.Tag is PictureBox)
                {
                    var pb = (PictureBox)p.Tag;
                    pb.Image?.Dispose();
                }
            }
            panelGaleria.Controls.Clear();

            try
            {
                var fotos = gestor.ListarFotos(manzana, lote, identificador, esConcepto);
                lblContador.Text = $"{fotos.Count} foto(s)";

                if (fotos.Count == 0)
                {
                    var lblVacio = new Label
                    {
                        Text = "No hay fotos adjuntas.\nUsa \"Agregar Fotos\" para anexar evidencia a este " +
                               (esConcepto ? "concepto." : "partida."),
                        AutoSize = false,
                        Width = 400,
                        Height = 60,
                        Margin = new Padding(20),
                        Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                        ForeColor = Color.FromArgb(149, 165, 166),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    panelGaleria.Controls.Add(lblVacio);
                    return;
                }

                foreach (var foto in fotos)
                {
                    panelGaleria.Controls.Add(CrearTarjetaFoto(foto));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las fotos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Crea una tarjeta con la miniatura, la info y los botones de una foto.
        /// </summary>
        private Panel CrearTarjetaFoto(FotoConceptoInfo foto)
        {
            var tarjeta = new Panel
            {
                Width = 230,
                Height = 240,
                Margin = new Padding(8),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            var pb = new PictureBox
            {
                Width = 228,
                Height = 150,
                Dock = DockStyle.Top,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(236, 240, 241),
                Cursor = Cursors.Hand
            };

            try
            {
                byte[] bytes = gestor.ObtenerFoto(foto.Id);
                if (bytes != null && bytes.Length > 0)
                {
                    using (var ms = new MemoryStream(bytes))
                    using (var img = Image.FromStream(ms))
                    {
                        pb.Image = new Bitmap(img);
                    }
                }
            }
            catch
            {
                // miniatura vacía si falla la carga
            }

            pb.Click += (s, e) => VerFotoCompleta(foto.Id, foto.Descripcion);

            // Mantener referencia para liberar la imagen luego
            tarjeta.Tag = pb;

            var lblInfo = new Label
            {
                Text = $"{foto.FechaFormateada}\n{foto.TamanioFormateado} · {foto.Usuario}",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(90, 90, 90),
                Padding = new Padding(5, 3, 5, 0)
            };

            var panelAcciones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                BackColor = Color.Transparent
            };

            var btnVer = new Button
            {
                Text = "👁 Ver",
                Width = 70,
                Height = 28,
                Location = new Point(5, 3),
                BackColor = ThemeManager.ColorSecundario,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVer.FlatAppearance.BorderSize = 0;
            btnVer.Click += (s, e) => VerFotoCompleta(foto.Id, foto.Descripcion);

            var btnEliminar = new Button
            {
                Text = "🗑 Eliminar",
                Width = 90,
                Height = 28,
                Location = new Point(80, 3),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += (s, e) => EliminarFoto(foto);

            panelAcciones.Controls.Add(btnVer);
            panelAcciones.Controls.Add(btnEliminar);

            // El orden de Dock importa: agregar de abajo hacia arriba
            tarjeta.Controls.Add(panelAcciones);
            tarjeta.Controls.Add(lblInfo);
            tarjeta.Controls.Add(pb);

            return tarjeta;
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccionar foto(s) para adjuntar";
                ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp|Todos los archivos|*.*";
                ofd.Multiselect = true;

                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                int guardadas = 0;
                int errores = 0;

                this.Cursor = Cursors.WaitCursor;
                try
                {
                    foreach (var archivo in ofd.FileNames)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(archivo);
                            if (fileInfo.Length > 10 * 1024 * 1024)
                            {
                                MessageBox.Show($"'{fileInfo.Name}' supera los 10 MB y se omitió.",
                                    "Archivo muy grande", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                errores++;
                                continue;
                            }

                            byte[] originales = File.ReadAllBytes(archivo);
                            byte[] redimensionada = GestorEvidencias.RedimensionarImagen(originales, 1280, 960);

                            gestor.GuardarFoto(manzana, lote, identificador, esConcepto, nombreNodo,
                                Path.GetFileNameWithoutExtension(archivo), redimensionada, "jpg",
                                Environment.UserName);

                            guardadas++;
                        }
                        catch (Exception exFoto)
                        {
                            errores++;
                            System.Diagnostics.Debug.WriteLine($"Error al guardar foto: {exFoto.Message}");
                        }
                    }
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }

                CargarFotos();

                if (guardadas > 0)
                {
                    MessageBox.Show(
                        $"{guardadas} foto(s) adjuntada(s) correctamente." +
                        (errores > 0 ? $"\n{errores} no se pudieron guardar." : ""),
                        "Fotos adjuntadas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (errores > 0)
                {
                    MessageBox.Show("No se pudo adjuntar ninguna foto.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void VerFotoCompleta(int id, string descripcion)
        {
            try
            {
                byte[] bytes = gestor.ObtenerFoto(id);
                if (bytes == null || bytes.Length == 0)
                    return;

                using (var visor = new Form
                {
                    Text = string.IsNullOrWhiteSpace(descripcion) ? "Vista de Foto" : descripcion,
                    Size = new Size(900, 700),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.Black
                })
                {
                    var pic = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Zoom
                    };

                    using (var ms = new MemoryStream(bytes))
                    using (var img = Image.FromStream(ms))
                    {
                        pic.Image = new Bitmap(img);
                    }

                    visor.Controls.Add(pic);
                    visor.ShowDialog(this);
                    pic.Image?.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar la foto: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EliminarFoto(FotoConceptoInfo foto)
        {
            var result = MessageBox.Show(
                "¿Eliminar esta foto?\n\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                if (gestor.EliminarFoto(foto.Id))
                {
                    CargarFotos();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar la foto.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la foto: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Liberar imágenes de las miniaturas
            foreach (Control c in panelGaleria.Controls)
            {
                if (c is Panel p && p.Tag is PictureBox pb)
                {
                    pb.Image?.Dispose();
                }
            }
            base.OnFormClosing(e);
        }
    }
}
