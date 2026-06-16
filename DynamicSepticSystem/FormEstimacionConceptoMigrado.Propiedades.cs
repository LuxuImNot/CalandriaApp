using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase parcial que implementa la ventana de "Propiedades" de un concepto o
    /// partida (adjuntar una o varias fotos) y la miniatura flotante que aparece
    /// pegada al cursor al pasar el mouse sobre un nodo con fotos.
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        // Gestor de fotos por concepto/partida (compartido por propiedades y hover)
        private GestorFotosConcepto gestorFotosConcepto;

        // --- Soporte para la miniatura flotante (hover) ---
        private FotoHoverPopup _fotoPopup;
        private NodoConcepto _ultimoNodoHover;
        private string _hoverHouseKey;
        // Caché de miniaturas por nodo (null = ya consultado y sin fotos)
        private readonly Dictionary<string, Image> _cacheThumbs = new Dictionary<string, Image>();

        /// <summary>
        /// Calcula el identificador estable de un nodo: Código para conceptos,
        /// WBS para partidas. Debe coincidir entre las propiedades y el hover.
        /// </summary>
        private string ObtenerIdentificadorNodo(NodoConcepto nodo)
        {
            return nodo.EsConcepto
                ? (string.IsNullOrWhiteSpace(nodo.Codigo) ? nodo.Nombre : nodo.Codigo)
                : nodo.WBS.ToString();
        }

        /// <summary>
        /// Abre la ventana de propiedades del nodo seleccionado para adjuntar fotos.
        /// </summary>
        private void MenuPropiedades_Click(object sender, EventArgs e)
        {
            var nodo = olvEstimacionConceptos.SelectedObject as NodoConcepto;

            if (nodo == null)
            {
                MessageBox.Show("Selecciona un concepto o partida primero.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar Manzana y Lote antes de adjuntar fotos.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();
            string identificador = ObtenerIdentificadorNodo(nodo);

            try
            {
                using (var form = new FormPropiedadesConcepto(
                    manzana, lote, identificador, nodo.EsConcepto, nodo.Nombre))
                {
                    form.ShowDialog(this);
                }

                // Las fotos pudieron cambiar: invalidar la miniatura en caché de este nodo
                InvalidarThumbNodo(nodo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir las propiedades: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Miniatura flotante al pasar el mouse (hover)

        /// <summary>
        /// Configura los eventos para mostrar la miniatura flotante. Se llama
        /// desde el constructor.
        /// </summary>
        private void ConfigurarPreviewFotosHover()
        {
            if (gestorFotosConcepto == null)
                gestorFotosConcepto = new GestorFotosConcepto(connectionString);

            if (olvEstimacionConceptos == null)
                return;

            olvEstimacionConceptos.MouseMove += OlvEstimacionConceptos_MouseMoveHover;
            olvEstimacionConceptos.MouseLeave += (s, e) => OcultarPreviewFoto();
        }

        private void OlvEstimacionConceptos_MouseMoveHover(object sender, MouseEventArgs e)
        {
            try
            {
                var hit = olvEstimacionConceptos.OlvHitTest(e.X, e.Y);
                var nodo = hit?.RowObject as NodoConcepto;

                if (nodo == null)
                {
                    _ultimoNodoHover = null;
                    OcultarPreviewFoto();
                    return;
                }

                if (!ReferenceEquals(nodo, _ultimoNodoHover))
                {
                    _ultimoNodoHover = nodo;
                    Image thumb = ObtenerThumbnailNodo(nodo);

                    if (thumb == null)
                    {
                        OcultarPreviewFoto();
                        return;
                    }

                    MostrarPreviewFoto(thumb);
                }

                // Reposicionar pegado al cursor
                PosicionarPreviewFoto();
            }
            catch
            {
                OcultarPreviewFoto();
            }
        }

        /// <summary>
        /// Obtiene (con caché) la miniatura de la primera foto de un nodo.
        /// Devuelve null si el nodo no tiene fotos o no hay casa seleccionada.
        /// </summary>
        private Image ObtenerThumbnailNodo(NodoConcepto nodo)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
                return null;

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            // Si cambió la casa, limpiar la caché de miniaturas
            string houseKey = manzana + "|" + lote;
            if (houseKey != _hoverHouseKey)
            {
                LimpiarCacheThumbs();
                _hoverHouseKey = houseKey;
            }

            string key = (nodo.EsConcepto ? "C:" : "P:") + ObtenerIdentificadorNodo(nodo);

            if (_cacheThumbs.TryGetValue(key, out Image cached))
                return cached; // puede ser null (sin fotos)

            Image thumb = null;
            try
            {
                var fotos = gestorFotosConcepto.ListarFotos(
                    manzana, lote, ObtenerIdentificadorNodo(nodo), nodo.EsConcepto);

                if (fotos.Count > 0)
                {
                    byte[] bytes = gestorFotosConcepto.ObtenerFoto(fotos[0].Id);
                    if (bytes != null && bytes.Length > 0)
                        thumb = CrearMiniatura(bytes, 240);
                }
            }
            catch
            {
                thumb = null;
            }

            _cacheThumbs[key] = thumb; // cachear resultado (incl. null)
            return thumb;
        }

        private void InvalidarThumbNodo(NodoConcepto nodo)
        {
            string key = (nodo.EsConcepto ? "C:" : "P:") + ObtenerIdentificadorNodo(nodo);
            if (_cacheThumbs.TryGetValue(key, out Image img))
            {
                img?.Dispose();
                _cacheThumbs.Remove(key);
            }
            // Forzar recálculo si se vuelve a hacer hover sobre el mismo nodo
            _ultimoNodoHover = null;
        }

        private void LimpiarCacheThumbs()
        {
            foreach (var img in _cacheThumbs.Values)
                img?.Dispose();
            _cacheThumbs.Clear();
        }

        private static Image CrearMiniatura(byte[] bytes, int maxLado)
        {
            using (var ms = new MemoryStream(bytes))
            using (var original = Image.FromStream(ms))
            {
                double ratio = Math.Min((double)maxLado / original.Width, (double)maxLado / original.Height);
                if (ratio > 1) ratio = 1; // no agrandar imágenes pequeñas

                int w = Math.Max(1, (int)(original.Width * ratio));
                int h = Math.Max(1, (int)(original.Height * ratio));

                var bmp = new Bitmap(w, h);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawImage(original, 0, 0, w, h);
                }
                return bmp;
            }
        }

        private void MostrarPreviewFoto(Image thumb)
        {
            if (_fotoPopup == null || _fotoPopup.IsDisposed)
                _fotoPopup = new FotoHoverPopup();

            _fotoPopup.SetImage(thumb);

            if (!_fotoPopup.Visible)
                _fotoPopup.MostrarSinActivar();

            PosicionarPreviewFoto();
        }

        private void PosicionarPreviewFoto()
        {
            if (_fotoPopup == null || _fotoPopup.IsDisposed || !_fotoPopup.Visible)
                return;

            Point cursor = Cursor.Position;
            int offset = 18;
            int x = cursor.X + offset;
            int y = cursor.Y + offset;

            // Mantener dentro del área de trabajo de la pantalla del cursor
            Rectangle area = Screen.FromPoint(cursor).WorkingArea;
            if (x + _fotoPopup.Width > area.Right)
                x = cursor.X - offset - _fotoPopup.Width;
            if (y + _fotoPopup.Height > area.Bottom)
                y = cursor.Y - offset - _fotoPopup.Height;

            if (x < area.Left) x = area.Left;
            if (y < area.Top) y = area.Top;

            _fotoPopup.Location = new Point(x, y);
        }

        private void OcultarPreviewFoto()
        {
            if (_fotoPopup != null && !_fotoPopup.IsDisposed && _fotoPopup.Visible)
                _fotoPopup.Hide();
        }

        #endregion
    }

    /// <summary>
    /// Ventana emergente sin bordes que muestra una miniatura pegada al cursor
    /// sin robar el foco al formulario principal.
    /// </summary>
    internal class FotoHoverPopup : Form
    {
        private readonly PictureBox _pic;

        public FotoHoverPopup()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.FromArgb(44, 62, 80); // marco oscuro
            Padding = new Padding(3);
            Size = new Size(246, 246);

            _pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            Controls.Add(_pic);
        }

        public void SetImage(Image img)
        {
            // El popup usa una copia para no interferir con la caché
            var anterior = _pic.Image;
            _pic.Image = img == null ? null : new Bitmap(img);
            anterior?.Dispose();

            if (img != null)
            {
                // Ajustar el tamaño del popup a la miniatura (+ marco)
                Size = new Size(img.Width + Padding.Horizontal, img.Height + Padding.Vertical);
            }
        }

        /// <summary>
        /// Muestra el popup sin activarlo (sin robar el foco).
        /// </summary>
        public void MostrarSinActivar()
        {
            Show();
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TOPMOST = 0x00000008;
                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;

                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOPMOST | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _pic.Image?.Dispose();
            base.Dispose(disposing);
        }
    }
}
