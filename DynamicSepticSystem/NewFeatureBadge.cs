using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Persistencia de las features "NUEVO" que el usuario ya descarto
    /// ("No volver a mostrar"). Una clave por linea en un archivo en LocalAppData.
    /// </summary>
    internal static class NewFeatureStore
    {
        private static readonly string _archivo = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CalandriaResidencial", "features_descartadas.txt");

        private static HashSet<string> _cache;

        private static HashSet<string> Cargar()
        {
            if (_cache != null) return _cache;
            _cache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (File.Exists(_archivo))
                    foreach (var l in File.ReadAllLines(_archivo))
                    {
                        var k = (l ?? "").Trim();
                        if (k.Length > 0) _cache.Add(k);
                    }
            }
            catch { /* sin persistencia: se mostraran siempre */ }
            return _cache;
        }

        public static bool EstaDescartada(string key) =>
            !string.IsNullOrWhiteSpace(key) && Cargar().Contains(key);

        public static void Descartar(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            var set = Cargar();
            if (!set.Add(key)) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_archivo));
                File.WriteAllLines(_archivo, set);
            }
            catch { /* el descarte solo aplica a esta sesion si falla la escritura */ }
        }
    }

    /// <summary>
    /// Globo explicativo de una feature nueva: encabezado "NUEVO", titulo,
    /// descripcion y un boton "No volver a mostrar". Form sin borde que no roba el
    /// foco (WS_EX_NOACTIVATE) para que aparezca al posar el cursor.
    /// </summary>
    internal sealed class NewFeaturePopup : Form
    {
        public event EventHandler NoMostrar;

        private static readonly Color Oro = Color.FromArgb(245, 176, 0);
        private static readonly Color Tinta = Color.FromArgb(44, 62, 80);

        public NewFeaturePopup(string titulo, string descripcion)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.White;
            Width = 300;
            Padding = new Padding(1);

            var lblNuevo = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Text = "  ★  NUEVO",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Oro,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblTitulo = new Label
            {
                AutoSize = false,
                Location = new Point(12, 34),
                Size = new Size(Width - 24, 22),
                Text = titulo ?? "",
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                ForeColor = Tinta
            };

            var lblDesc = new Label
            {
                AutoSize = false,
                Location = new Point(12, 58),
                Size = new Size(Width - 24, 60),
                Text = descripcion ?? "",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(80, 90, 100)
            };
            // Ajusta el alto al texto.
            using (var g = CreateGraphics())
            {
                var sz = g.MeasureString(lblDesc.Text, lblDesc.Font, lblDesc.Width);
                lblDesc.Height = Math.Max(36, (int)Math.Ceiling(sz.Height) + 4);
            }

            var btnNo = new Button
            {
                Text = "No volver a mostrar",
                Font = new Font("Segoe UI", 8.5F),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(120, 130, 140),
                BackColor = Color.FromArgb(245, 246, 248),
                Size = new Size(Width - 24, 28),
                Location = new Point(12, lblDesc.Bottom + 8),
                Cursor = Cursors.Hand
            };
            btnNo.FlatAppearance.BorderColor = Color.FromArgb(220, 224, 228);
            btnNo.Click += (s, e) => NoMostrar?.Invoke(this, EventArgs.Empty);

            Height = btnNo.Bottom + 12;
            Controls.Add(lblTitulo);
            Controls.Add(lblDesc);
            Controls.Add(btnNo);
            Controls.Add(lblNuevo);
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000;   // WS_EX_NOACTIVATE
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(220, 224, 228)))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }

    /// <summary>
    /// Insignia "NUEVO": una estrella animada (efecto gif via timer) que marca un
    /// control nuevo. Al posar el cursor expande un globo con titulo + descripcion y un
    /// boton "No volver a mostrar" que la oculta permanentemente (por usuario). Si la
    /// feature ya fue descartada, no se crea. Es circular (Region) para no dejar
    /// esquinas sobre el control de abajo.
    /// </summary>
    public sealed class NewFeatureBadge : Control
    {
        private readonly Timer _anim = new Timer { Interval = 60 };
        private readonly Timer _hover = new Timer { Interval = 250 };
        private float _fase;
        private readonly string _featureKey;
        private readonly string _titulo;
        private readonly string _descripcion;
        private NewFeaturePopup _popup;

        private static readonly Color Oro = Color.FromArgb(255, 196, 0);

        private NewFeatureBadge(string featureKey, string titulo, string descripcion)
        {
            _featureKey = featureKey;
            _titulo = titulo;
            _descripcion = descripcion;

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            Size = new Size(22, 22);
            Cursor = Cursors.Help;
            TabStop = false;

            _anim.Tick += (s, e) => { _fase += 0.14f; Invalidate(); };
            _hover.Tick += (s, e) => ChequearHover();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AjustarRegion();
            _anim.Start();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AjustarRegion();
        }

        private void AjustarRegion()
        {
            using (var p = new GraphicsPath())
            {
                p.AddEllipse(0, 0, Width, Height);
                Region = new Region(p);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Chip circular (sticker) para que se lea sobre cualquier fondo.
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var fondo = new SolidBrush(Color.White))
            using (var borde = new Pen(Color.FromArgb(235, 200, 120)))
            {
                g.FillEllipse(fondo, rect);
                g.DrawEllipse(borde, rect);
            }

            // Estrella dorada que titila (pulso de tamano y alfa).
            float pulso = 0.70f + 0.30f * (float)Math.Sin(_fase);
            var c = new PointF(Width / 2f, Height / 2f);
            float rExt = (Math.Min(Width, Height) / 2f - 3f) * (0.85f + 0.15f * pulso);
            using (var path = EstrellaPath(c, rExt, rExt * 0.45f))
            using (var br = new SolidBrush(Color.FromArgb((int)(160 + 95 * pulso), Oro)))
            using (var pen = new Pen(Color.FromArgb(220, 180, 80), 1f))
            {
                g.FillPath(br, path);
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath EstrellaPath(PointF c, float rExt, float rInt)
        {
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                float r = (i % 2 == 0) ? rExt : rInt;
                double ang = -Math.PI / 2 + i * Math.PI / 5;
                pts[i] = new PointF(c.X + (float)(r * Math.Cos(ang)), c.Y + (float)(r * Math.Sin(ang)));
            }
            var path = new GraphicsPath();
            path.AddPolygon(pts);
            return path;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            MostrarPopup();
        }

        private void MostrarPopup()
        {
            if (_popup != null && !_popup.IsDisposed) return;

            _popup = new NewFeaturePopup(_titulo, _descripcion);
            _popup.NoMostrar += (s, ev) =>
            {
                NewFeatureStore.Descartar(_featureKey);
                OcultarPopup();
                Visible = false;
                _anim.Stop();
                try { Parent?.Controls.Remove(this); } catch { }
                Dispose();
            };

            // Posicion: bajo-derecha de la insignia, dentro de la pantalla.
            var anclaPantalla = PointToScreen(new Point(Width, Height));
            var area = Screen.FromControl(this).WorkingArea;
            int x = Math.Min(anclaPantalla.X, area.Right - _popup.Width - 8);
            int y = anclaPantalla.Y + 4;
            if (y + _popup.Height > area.Bottom) y = anclaPantalla.Y - _popup.Height - Height - 4;
            _popup.Location = new Point(Math.Max(area.Left + 4, x), Math.Max(area.Top + 4, y));

            _popup.Show();
            _hover.Start();
        }

        private void OcultarPopup()
        {
            _hover.Stop();
            if (_popup != null && !_popup.IsDisposed)
            {
                _popup.Close();
                _popup.Dispose();
            }
            _popup = null;
        }

        private void ChequearHover()
        {
            if (_popup == null || _popup.IsDisposed) { _hover.Stop(); return; }
            var p = Cursor.Position;
            bool sobreBadge = !IsDisposed && ClientRectangle.Contains(PointToClient(p));
            bool sobrePopup = _popup.Bounds.Contains(p);
            if (!sobreBadge && !sobrePopup) OcultarPopup();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _anim.Dispose();
                _hover.Dispose();
                if (_popup != null && !_popup.IsDisposed) _popup.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Coloca una insignia "NUEVO" en la esquina superior derecha de <paramref name="objetivo"/>.
        /// Si la feature ya fue descartada, no hace nada. Devuelve la insignia o null.
        /// </summary>
        public static NewFeatureBadge Adjuntar(Control objetivo, string featureKey,
            string titulo, string descripcion, Point? desplazamiento = null)
        {
            if (objetivo == null || objetivo.Parent == null) return null;
            if (NewFeatureStore.EstaDescartada(featureKey)) return null;

            var badge = new NewFeatureBadge(featureKey, titulo, descripcion);
            var parent = objetivo.Parent;
            parent.Controls.Add(badge);
            badge.BringToFront();

            var off = desplazamiento ?? new Point(-2, -2);
            void Reposicionar()
            {
                if (badge.IsDisposed || objetivo.IsDisposed) return;
                badge.Location = new Point(
                    objetivo.Right - badge.Width + off.X,
                    objetivo.Top + off.Y);
                badge.BringToFront();
            }
            Reposicionar();
            objetivo.LocationChanged += (s, e) => Reposicionar();
            objetivo.SizeChanged += (s, e) => Reposicionar();
            return badge;
        }
    }
}
