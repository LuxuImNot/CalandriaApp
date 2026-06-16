using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem.ModernUI
{
    // =========================================================================
    // ModernCard: panel con esquinas redondeadas, sombra suave y header opcional.
    // =========================================================================
    public class ModernCard : Panel
    {
        private string _title = string.Empty;
        private string _subtitle = string.Empty;
        private Color _accentColor = Color.FromArgb(179, 108, 46);
        private Image _headerIcon;
        private int _cornerRadius = 12;
        private int _headerHeight = 0;

        public ModernCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Padding = new Padding(16, 16, 16, 16);
        }

        [Category("Modern UI")]
        public string CardTitle
        {
            get => _title;
            set { _title = value ?? string.Empty; RecalcPadding(); Invalidate(); }
        }

        [Category("Modern UI")]
        public string CardSubtitle
        {
            get => _subtitle;
            set { _subtitle = value ?? string.Empty; Invalidate(); }
        }

        [Category("Modern UI")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        [Category("Modern UI")]
        public Image HeaderIcon
        {
            get => _headerIcon;
            set { _headerIcon = value; Invalidate(); }
        }

        [Category("Modern UI"), DefaultValue(12)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        private void RecalcPadding()
        {
            _headerHeight = string.IsNullOrEmpty(_title) ? 0 : 46;
            Padding = new Padding(16, 16 + _headerHeight, 16, 16);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // ThemeManager puede haber reescrito el Padding; lo restauramos.
            RecalcPadding();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle full = ClientRectangle;
            // Sombra simulada (offset 2px abajo, color suave)
            Rectangle shadow = new Rectangle(full.X + 1, full.Y + 2, full.Width - 2, full.Height - 2);
            using (var shadowPath = RoundedRect(shadow, _cornerRadius))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(28, 0, 0, 0)))
            {
                g.FillPath(shadowBrush, shadowPath);
            }

            Rectangle card = new Rectangle(full.X, full.Y, full.Width - 2, full.Height - 4);
            using (var path = RoundedRect(card, _cornerRadius))
            using (var bg = new SolidBrush(Color.White))
            {
                g.FillPath(bg, path);

                // Borde sutil
                using (var pen = new Pen(Color.FromArgb(230, 226, 219), 1f))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Header
            if (!string.IsNullOrEmpty(_title))
            {
                // Línea de acento en la parte superior
                using (var accent = new SolidBrush(_accentColor))
                {
                    Rectangle accentRect = new Rectangle(card.X + 12, card.Y + 12, 4, 22);
                    using (var ap = RoundedRect(accentRect, 2))
                        g.FillPath(accent, ap);
                }

                int textLeft = card.X + 24;
                if (_headerIcon != null)
                {
                    g.DrawImage(_headerIcon, card.X + 22, card.Y + 12, 22, 22);
                    textLeft = card.X + 50;
                }

                using (var titleFont = new Font("Segoe UI Semibold", 11.5f, FontStyle.Bold))
                using (var titleBrush = new SolidBrush(Color.FromArgb(48, 42, 36)))
                {
                    g.DrawString(_title, titleFont, titleBrush, textLeft, card.Y + 11);
                }

                if (!string.IsNullOrEmpty(_subtitle))
                {
                    using (var sFont = new Font("Segoe UI", 8.25f, FontStyle.Regular))
                    using (var sBrush = new SolidBrush(Color.FromArgb(140, 134, 124)))
                    {
                        SizeF tSize;
                        using (var titleFont = new Font("Segoe UI Semibold", 11.5f, FontStyle.Bold))
                            tSize = g.MeasureString(_title, titleFont);
                        g.DrawString(_subtitle, sFont, sBrush, textLeft + tSize.Width + 8, card.Y + 16);
                    }
                }

                // Divisor sutil bajo el header
                using (var pen = new Pen(Color.FromArgb(240, 236, 230), 1f))
                {
                    g.DrawLine(pen, card.X + 16, card.Y + 42, card.Right - 16, card.Y + 42);
                }
            }

            // NOTA: NO llamamos a base.OnPaint(e) para evitar que ThemeManager
            // dibuje un borde rectangular encima de nuestras esquinas redondeadas.
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // =========================================================================
    // KpiCard: tarjeta KPI con valor grande, etiqueta y acento.
    // Soporta animación del valor numérico.
    // =========================================================================
    public class KpiCard : Control
    {
        private string _label = "KPI";
        private string _valueText = "0";
        private string _suffix = string.Empty;
        private Color _accentColor = Color.FromArgb(179, 108, 46);
        private string _trend = string.Empty;
        private Image _icon;

        // Animación numérica
        private float _displayedValue = 0f;
        private float _targetValue = 0f;
        private bool _animateNumeric = false;
        private string _format = "F1";
        private Timer _animTimer;

        public KpiCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(260, 110);
            _animTimer = new Timer { Interval = 16 };
            _animTimer.Tick += AnimTick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer?.Stop();
                _animTimer?.Dispose();
                _animTimer = null;
            }
            base.Dispose(disposing);
        }

        [Category("KPI")]
        public string LabelText
        {
            get => _label;
            set { _label = value ?? string.Empty; Invalidate(); }
        }

        [Category("KPI")]
        public string ValueText
        {
            get => _valueText;
            set { _valueText = value ?? string.Empty; _animateNumeric = false; _animTimer.Stop(); Invalidate(); }
        }

        [Category("KPI")]
        public string Suffix
        {
            get => _suffix;
            set { _suffix = value ?? string.Empty; Invalidate(); }
        }

        [Category("KPI")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        [Category("KPI")]
        public string Trend
        {
            get => _trend;
            set { _trend = value ?? string.Empty; Invalidate(); }
        }

        [Category("KPI")]
        public Image Icon
        {
            get => _icon;
            set { _icon = value; Invalidate(); }
        }

        /// <summary>
        /// Anima el valor numérico desde el actual hasta target.
        /// </summary>
        public void SetValueAnimated(float target, string format = "F1", string suffix = null)
        {
            _targetValue = target;
            _format = format ?? "F1";
            if (suffix != null) _suffix = suffix;
            _animateNumeric = true;
            _animTimer.Start();
        }

        private void AnimTick(object sender, EventArgs e)
        {
            float diff = _targetValue - _displayedValue;
            if (Math.Abs(diff) < 0.05f)
            {
                _displayedValue = _targetValue;
                _animTimer.Stop();
            }
            else
            {
                // Easing exponencial (suave al final)
                _displayedValue += diff * 0.18f;
            }
            _valueText = _displayedValue.ToString(_format, System.Globalization.CultureInfo.InvariantCulture);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle full = ClientRectangle;

            // Sombra
            Rectangle shadow = new Rectangle(full.X + 1, full.Y + 2, full.Width - 2, full.Height - 2);
            using (var p = RoundedRect(shadow, 14))
            using (var b = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                g.FillPath(b, p);

            // Fondo
            Rectangle card = new Rectangle(full.X, full.Y, full.Width - 2, full.Height - 4);
            using (var p = RoundedRect(card, 14))
            {
                using (var bg = new SolidBrush(Color.White))
                    g.FillPath(bg, p);
                using (var pen = new Pen(Color.FromArgb(232, 228, 222), 1f))
                    g.DrawPath(pen, p);
            }

            // Barra de acento izquierda (tipo "stripe")
            Rectangle stripe = new Rectangle(card.X, card.Y + 14, 4, card.Height - 28);
            using (var sb = new SolidBrush(_accentColor))
            using (var sp = RoundedRect(stripe, 2))
                g.FillPath(sb, sp);

            // Icon circle (top-right) - más compacto
            int iconBoxSize = 30;
            Rectangle iconBox = new Rectangle(card.Right - iconBoxSize - 14, card.Y + 12, iconBoxSize, iconBoxSize);
            using (var bg = new SolidBrush(Color.FromArgb(25, _accentColor)))
                g.FillEllipse(bg, iconBox);
            if (_icon != null)
            {
                g.DrawImage(_icon, iconBox.X + 6, iconBox.Y + 6, iconBox.Width - 12, iconBox.Height - 12);
            }
            else
            {
                using (var dotBrush = new SolidBrush(_accentColor))
                {
                    int dot = 9;
                    g.FillEllipse(dotBrush, iconBox.X + (iconBox.Width - dot) / 2, iconBox.Y + (iconBox.Height - dot) / 2, dot, dot);
                }
            }

            // Label (top)
            using (var labelFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (var labelBrush = new SolidBrush(Color.FromArgb(140, 134, 124)))
            {
                g.DrawString(_label.ToUpperInvariant(), labelFont, labelBrush, card.X + 16, card.Y + 14);
            }

            // Value (centro)
            using (var valueFont = new Font("Segoe UI", 18f, FontStyle.Bold))
            using (var valueBrush = new SolidBrush(Color.FromArgb(38, 32, 26)))
            {
                SizeF vSize = g.MeasureString(_valueText, valueFont);
                float vy = card.Y + 30;
                g.DrawString(_valueText, valueFont, valueBrush, card.X + 14, vy);

                if (!string.IsNullOrEmpty(_suffix))
                {
                    using (var sfxFont = new Font("Segoe UI", 10f, FontStyle.Bold))
                    using (var sfxBrush = new SolidBrush(_accentColor))
                    {
                        g.DrawString(_suffix, sfxFont, sfxBrush, card.X + 14 + vSize.Width - 4, vy + 12);
                    }
                }
            }

            // Trend (abajo)
            if (!string.IsNullOrEmpty(_trend))
            {
                using (var trendFont = new Font("Segoe UI", 7.5f, FontStyle.Regular))
                using (var trendBrush = new SolidBrush(Color.FromArgb(115, 110, 102)))
                {
                    g.DrawString(_trend, trendFont, trendBrush, card.X + 16, card.Bottom - 18);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // =========================================================================
    // AnimatedProgressBar: barra de progreso custom con gradiente,
    // esquinas redondeadas y animación suave al cambiar de valor.
    // =========================================================================
    public class AnimatedProgressBar : Control
    {
        private float _value = 0f;
        private float _targetValue = 0f;
        private float _maxValue = 100f;
        private Color _gradientStart = Color.FromArgb(179, 108, 46);
        private Color _gradientEnd = Color.FromArgb(217, 142, 24);
        private Color _trackColor = Color.FromArgb(238, 234, 226);
        private bool _showPercent = true;
        private int _cornerRadius = 8;
        private Timer _timer;

        public AnimatedProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Height = 16;
            _timer = new Timer { Interval = 16 };
            _timer.Tick += TimerTick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _timer = null;
            }
            base.Dispose(disposing);
        }

        [Category("ProgressBar")]
        public float Value
        {
            get => _value;
            set
            {
                float v = Math.Max(0, Math.Min(_maxValue, value));
                _value = v;
                _targetValue = v;
                _timer.Stop();
                Invalidate();
            }
        }

        public void SetValueAnimated(float target)
        {
            _targetValue = Math.Max(0, Math.Min(_maxValue, target));
            _timer.Start();
        }

        [Category("ProgressBar"), DefaultValue(100f)]
        public float MaxValue
        {
            get => _maxValue;
            set { _maxValue = Math.Max(1, value); Invalidate(); }
        }

        [Category("ProgressBar")]
        public Color GradientStart
        {
            get => _gradientStart;
            set { _gradientStart = value; Invalidate(); }
        }

        [Category("ProgressBar")]
        public Color GradientEnd
        {
            get => _gradientEnd;
            set { _gradientEnd = value; Invalidate(); }
        }

        [Category("ProgressBar")]
        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; Invalidate(); }
        }

        [Category("ProgressBar"), DefaultValue(true)]
        public bool ShowPercent
        {
            get => _showPercent;
            set { _showPercent = value; Invalidate(); }
        }

        [Category("ProgressBar"), DefaultValue(8)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            float diff = _targetValue - _value;
            if (Math.Abs(diff) < 0.1f)
            {
                _value = _targetValue;
                _timer.Stop();
            }
            else
            {
                _value += diff * 0.18f;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle r = new Rectangle(0, 0, Width, Height);

            // Track
            using (var trackPath = RoundedRect(r, _cornerRadius))
            using (var trackBrush = new SolidBrush(_trackColor))
            {
                g.FillPath(trackBrush, trackPath);
            }

            // Fill
            float pct = _maxValue > 0 ? (_value / _maxValue) : 0;
            pct = Math.Max(0, Math.Min(1, pct));
            int fillWidth = (int)(Width * pct);

            if (fillWidth > 2)
            {
                Rectangle fillRect = new Rectangle(0, 0, fillWidth, Height);
                using (var fillPath = RoundedRect(fillRect, _cornerRadius))
                using (var brush = new LinearGradientBrush(fillRect, _gradientStart, _gradientEnd, LinearGradientMode.Horizontal))
                {
                    g.FillPath(brush, fillPath);
                }

                // Brillo superior
                Rectangle gloss = new Rectangle(0, 0, fillWidth, Math.Max(1, Height / 2));
                using (var fillPath = RoundedRect(fillRect, _cornerRadius))
                using (var glossBrush = new LinearGradientBrush(gloss,
                    Color.FromArgb(60, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical))
                {
                    Region prev = g.Clip;
                    g.SetClip(fillPath);
                    g.FillRectangle(glossBrush, gloss);
                    g.Clip = prev;
                }
            }

            // Texto centrado
            if (_showPercent)
            {
                string text = $"{pct * 100:F1}%";
                using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    SizeF tSize = g.MeasureString(text, font);
                    float tx = (Width - tSize.Width) / 2f;
                    float ty = (Height - tSize.Height) / 2f;
                    // Sombra
                    using (var sBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                        g.DrawString(text, font, sBrush, tx + 1, ty + 1);
                    using (var tBrush = new SolidBrush(Color.White))
                        g.DrawString(text, font, tBrush, tx, ty);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (radius <= 0 || r.Width < d || r.Height < d) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // =========================================================================
    // CircularIconButton: botón circular pequeño para overlay del mapa.
    // =========================================================================
    public class CircularIconButton : Button
    {
        private Color _baseColor = Color.FromArgb(245, 255, 255, 255);
        private Color _hoverColor = Color.White;
        private Color _glyphColor = Color.FromArgb(60, 50, 40);
        private string _glyph = "+";
        private bool _hovered = false;

        public CircularIconButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Size = new Size(34, 34);
        }

        [Category("Modern UI")]
        public string Glyph
        {
            get => _glyph;
            set { _glyph = value ?? ""; Invalidate(); }
        }

        [Category("Modern UI")]
        public Color GlyphColor
        {
            get => _glyphColor;
            set { _glyphColor = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle r = new Rectangle(0, 0, Width, Height);
            // Sombra
            using (var sb = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                g.FillEllipse(sb, r.X + 1, r.Y + 2, r.Width - 2, r.Height - 2);

            Rectangle btn = new Rectangle(0, 0, Width - 2, Height - 3);
            using (var bg = new SolidBrush(_hovered ? _hoverColor : _baseColor))
                g.FillEllipse(bg, btn);
            using (var pen = new Pen(Color.FromArgb(220, 215, 207), 1f))
                g.DrawEllipse(pen, btn);

            using (var font = new Font("Segoe UI", 13f, FontStyle.Bold))
            using (var brush = new SolidBrush(_glyphColor))
            {
                SizeF tSize = g.MeasureString(_glyph, font);
                g.DrawString(_glyph, font, brush, btn.X + (btn.Width - tSize.Width) / 2f, btn.Y + (btn.Height - tSize.Height) / 2f - 1);
            }
        }
    }

    // =========================================================================
    // RoundedTextBox: TextBox con esquinas redondeadas (vía contenedor).
    // No es un TextBox subclase porque WinForms hace tricky el redondeo nativo;
    // usamos un panel que pinta el borde y aloja un TextBox sin borde dentro.
    // =========================================================================
    public class RoundedTextBoxPanel : Panel
    {
        public TextBox Inner { get; private set; }
        private string _placeholder = string.Empty;
        private bool _focused = false;

        public RoundedTextBoxPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Height = 36;

            Inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(38, 32, 26)
            };
            Inner.GotFocus += (s, e) => { _focused = true; Invalidate(); };
            Inner.LostFocus += (s, e) => { _focused = false; Invalidate(); };
            Inner.TextChanged += (s, e) => Invalidate();
            Controls.Add(Inner);
        }

        [Category("Modern UI")]
        public string Placeholder
        {
            get => _placeholder;
            set { _placeholder = value ?? ""; Invalidate(); }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            if (Inner != null)
            {
                Inner.Location = new Point(12, (Height - Inner.PreferredHeight) / 2);
                Inner.Width = Width - 24;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = RoundedRect(r, 8))
            using (var bg = new SolidBrush(Color.White))
            {
                g.FillPath(bg, path);
                using (var pen = new Pen(_focused ? Color.FromArgb(179, 108, 46) : Color.FromArgb(220, 215, 207),
                    _focused ? 1.6f : 1f))
                    g.DrawPath(pen, path);
            }

            // Placeholder
            if (string.IsNullOrEmpty(Inner?.Text) && !string.IsNullOrEmpty(_placeholder))
            {
                using (var font = new Font("Segoe UI", 10.5f, FontStyle.Italic))
                using (var brush = new SolidBrush(Color.FromArgb(160, 154, 144)))
                {
                    g.DrawString(_placeholder, font, brush, 14, (Height - font.GetHeight(g)) / 2f);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // =========================================================================
    // ModernNavBar: barra de navegación horizontal con underline animado,
    // hover suave por ítem, y dropdown flotante (ModernNavDropdown).
    // =========================================================================
    public class ModernNavBar : Control
    {
        public class NavItem
        {
            public string Text;
            public List<NavSubItem> Children = new List<NavSubItem>();
            public Action OnClick; // si no tiene children
            internal Rectangle Bounds;
        }
        public class NavSubItem
        {
            public string Text;
            public Action OnClick;
            public bool IsSeparator;
            public Color? AccentColor;
            public bool IsBold;
        }

        private List<NavItem> _items = new List<NavItem>();
        private int _hoveredIndex = -1;
        private int _activeIndex = -1;
        private float _underlineX, _underlineW;
        private float _targetX, _targetW;
        private readonly Dictionary<int, float> _hoverOpacity = new Dictionary<int, float>();
        private Timer _animTimer;
        private ModernNavDropdown _dropdown;
        private Color _accentColor = Color.FromArgb(217, 142, 24);

        // Anti-rebote: si el dropdown se cerró por click en el mismo item,
        // no lo reabrimos inmediatamente.
        private int _lastClosedIdx = -1;
        private DateTime _lastCloseTime = DateTime.MinValue;

        [Category("Modern UI")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        public ModernNavBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(88, 53, 23);
            Height = 48;
            _animTimer = new Timer { Interval = 16 };
            _animTimer.Tick += AnimTick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer?.Stop();
                _animTimer?.Dispose();
                _animTimer = null;
                if (_dropdown != null && !_dropdown.IsDisposed) _dropdown.Close();
            }
            base.Dispose(disposing);
        }

        public void SetItems(IEnumerable<NavItem> items)
        {
            _items = items?.ToList() ?? new List<NavItem>();
            LayoutItems();
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            LayoutItems();
        }

        private void LayoutItems()
        {
            if (_items == null || _items.Count == 0) return;

            // TextRenderer no requiere handle del control (funciona aunque la
            // ventana aún no exista, como en el constructor).
            int x = 20;
            using (var font = new Font("Segoe UI Semibold", 9.25f, FontStyle.Bold))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    var sz = TextRenderer.MeasureText(item.Text ?? "", font);
                    int w = sz.Width + 32;
                    item.Bounds = new Rectangle(x, 0, w, Height);
                    x += w;
                }
            }
        }

        private void AnimTick(object sender, EventArgs e)
        {
            bool anyChange = false;

            // Underline
            float diffX = _targetX - _underlineX;
            float diffW = _targetW - _underlineW;
            if (Math.Abs(diffX) > 0.5f || Math.Abs(diffW) > 0.5f)
            {
                _underlineX += diffX * 0.28f;
                _underlineW += diffW * 0.28f;
                anyChange = true;
            }
            else
            {
                _underlineX = _targetX;
                _underlineW = _targetW;
            }

            // Hover opacities
            for (int i = 0; i < _items.Count; i++)
            {
                float target = (i == _hoveredIndex) ? 1f : 0f;
                float current = _hoverOpacity.TryGetValue(i, out var v) ? v : 0f;
                float diff = target - current;
                if (Math.Abs(diff) > 0.02f)
                {
                    _hoverOpacity[i] = current + diff * 0.28f;
                    anyChange = true;
                }
                else
                {
                    _hoverOpacity[i] = target;
                }
            }

            if (anyChange) Invalidate();
            else _animTimer.Stop();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Fondo con gradiente sutil café
            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, Math.Max(1, Width), Math.Max(1, Height)),
                Color.FromArgb(96, 58, 25),
                Color.FromArgb(72, 44, 19),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, 0, 0, Width, Height);
            }

            // Línea inferior divisora muy sutil
            using (var pen = new Pen(Color.FromArgb(40, 0, 0, 0), 1f))
                g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

            // Items
            using (var font = new Font("Segoe UI Semibold", 9.25f, FontStyle.Bold))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    float hover = _hoverOpacity.TryGetValue(i, out var v) ? v : 0f;
                    bool isActive = (i == _activeIndex);

                    // Hover background (capa blanca translúcida)
                    if (hover > 0.01f || isActive)
                    {
                        int alpha = (int)(hover * 38) + (isActive ? 30 : 0);
                        alpha = Math.Min(255, alpha);
                        using (var b = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                            g.FillRectangle(b, item.Bounds);
                    }

                    // Texto
                    Color txt = isActive ? Color.White : Color.FromArgb(232, 224, 213);
                    using (var brush = new SolidBrush(txt))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(item.Text, font, brush, item.Bounds, sf);
                    }
                }
            }

            // Underline animado (acento ámbar)
            if (_underlineW > 1)
            {
                int pad = 12;
                var ux = _underlineX + pad;
                var uw = _underlineW - pad * 2;
                if (uw >= 2)    
                {
                    Rectangle r = new Rectangle((int)ux, Height - 3, (int)uw, 3);
                    using (var path = RoundRect(r, 2))
                    using (var brush = new LinearGradientBrush(r,
                        _accentColor, Lighten(_accentColor, 1.15f), LinearGradientMode.Horizontal))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int newHover = HitTest(e.Location);
            if (newHover != _hoveredIndex)
            {
                _hoveredIndex = newHover;
                Cursor = (newHover >= 0) ? Cursors.Hand : Cursors.Default;
                UpdateUnderlineTarget();
                _animTimer.Start();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hoveredIndex = -1;
            UpdateUnderlineTarget();
            _animTimer.Start();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int idx = HitTest(e.Location);
            if (idx >= 0)
            {
                // Si el dropdown se acaba de cerrar por click en el mismo item, no re-abrir
                if (idx == _lastClosedIdx &&
                    (DateTime.UtcNow - _lastCloseTime).TotalMilliseconds < 250)
                {
                    _lastClosedIdx = -1;
                    return;
                }

                var item = _items[idx];
                bool wasOpen = (_activeIndex == idx);
                CloseDropdown();

                if (wasOpen)
                {
                    // Toggle off
                }
                else if (item.Children != null && item.Children.Count > 0)
                {
                    ShowDropdown(idx);
                }
                else
                {
                    item.OnClick?.Invoke();
                }
            }
            base.OnMouseDown(e);
        }

        private void UpdateUnderlineTarget()
        {
            int idx = _hoveredIndex >= 0 ? _hoveredIndex : _activeIndex;
            if (idx >= 0 && idx < _items.Count)
            {
                _targetX = _items[idx].Bounds.X;
                _targetW = _items[idx].Bounds.Width;
            }
            else
            {
                _targetW = 0; // colapsa underline
            }
        }

        private int HitTest(Point p)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Bounds.Contains(p)) return i;
            }
            return -1;
        }

        private void ShowDropdown(int idx)
        {
            _activeIndex = idx;
            UpdateUnderlineTarget();
            _animTimer.Start();
            Invalidate();

            var item = _items[idx];
            Point screenLoc = PointToScreen(new Point(item.Bounds.X, Height));
            _dropdown = new ModernNavDropdown(item.Children, screenLoc, item.Bounds.Width);
            _dropdown.FormClosed += (s, e) =>
            {
                _lastClosedIdx = _activeIndex;
                _lastCloseTime = DateTime.UtcNow;
                _dropdown = null;
                _activeIndex = -1;
                UpdateUnderlineTarget();
                _animTimer.Start();
                Invalidate();
            };
            _dropdown.Show(FindForm());
        }

        private void CloseDropdown()
        {
            if (_dropdown != null && !_dropdown.IsDisposed)
            {
                _dropdown.Close();
                _dropdown = null;
            }
            _activeIndex = -1;
            UpdateUnderlineTarget();
            _animTimer.Start();
            Invalidate();
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (radius <= 0 || r.Width < d || r.Height < d) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color Lighten(Color c, float f) =>
            Color.FromArgb(c.A,
                (int)Math.Min(255, c.R * f),
                (int)Math.Min(255, c.G * f),
                (int)Math.Min(255, c.B * f));
    }

    // =========================================================================
    // ModernNavDropdown: form flotante para el dropdown de la nav bar
    // con fade-in + slide animados y hover suave en cada item.
    // =========================================================================
    public class ModernNavDropdown : Form
    {
        private readonly List<ModernNavBar.NavSubItem> _items;
        private int _hoveredIndex = -1;
        private const int ItemHeight = 34;
        private const int SeparatorHeight = 9;
        private const int PaddingTop = 8;
        private const int PaddingBottom = 8;
        private const int MinWidth = 230;
        private const int MaxWidth = 360;
        private Timer _animTimer;
        private float _slideRemaining = 12f; // px que aún debe descender
        private double _opacityTarget = 1.0;

        public ModernNavDropdown(List<ModernNavBar.NavSubItem> items, Point screenLocation, int minBarWidth)
        {
            _items = items ?? new List<ModernNavBar.NavSubItem>();

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = false;
            DoubleBuffered = true;
            Opacity = 0;
            BackColor = Color.Magenta; // se hace transparente
            TransparencyKey = Color.Magenta;

            // Calcular tamaño
            int height = PaddingTop + PaddingBottom;
            int width = Math.Max(MinWidth, minBarWidth);
            using (var g = CreateGraphics())
            using (var font = new Font("Segoe UI", 9f, FontStyle.Regular))
            {
                foreach (var it in _items)
                {
                    int h = it.IsSeparator ? SeparatorHeight : ItemHeight;
                    height += h;
                    if (!it.IsSeparator && !string.IsNullOrEmpty(it.Text))
                    {
                        var sz = g.MeasureString(it.Text, font);
                        width = Math.Max(width, (int)Math.Ceiling(sz.Width) + 56);
                    }
                }
            }
            width = Math.Min(MaxWidth, width);
            Size = new Size(width, height);

            // Posicionar 12px arriba de la posición destino y animar bajada
            Location = new Point(screenLocation.X, screenLocation.Y - (int)_slideRemaining);

            MouseMove += OnMouseMoveLocal;
            MouseClick += OnMouseClickLocal;
            MouseLeave += (s, e) => { _hoveredIndex = -1; Invalidate(); };

            _animTimer = new Timer { Interval = 16 };
            _animTimer.Tick += AnimTick;
            _animTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _animTimer?.Stop();
            _animTimer?.Dispose();
            base.OnFormClosed(e);
        }

        private void AnimTick(object sender, EventArgs e)
        {
            bool changed = false;

            // Fade-in
            double diff = _opacityTarget - Opacity;
            if (Math.Abs(diff) > 0.02)
            {
                Opacity = Math.Max(0, Math.Min(1, Opacity + diff * 0.30));
                changed = true;
            }
            else Opacity = _opacityTarget;

            // Slide-down
            if (Math.Abs(_slideRemaining) > 0.5f)
            {
                float step = _slideRemaining * 0.30f;
                _slideRemaining -= step;
                Location = new Point(Location.X, Location.Y + (int)Math.Ceiling(step));
                changed = true;
            }

            if (!changed) _animTimer.Stop();
        }

        private void OnMouseMoveLocal(object sender, MouseEventArgs e)
        {
            int newHover = HitTest(e.Location);
            if (newHover != _hoveredIndex)
            {
                _hoveredIndex = newHover;
                Cursor = (newHover >= 0) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        private void OnMouseClickLocal(object sender, MouseEventArgs e)
        {
            int idx = HitTest(e.Location);
            if (idx >= 0 && !_items[idx].IsSeparator)
            {
                var item = _items[idx];
                this.Close();
                item.OnClick?.Invoke();
            }
        }

        private int HitTest(Point p)
        {
            int y = PaddingTop;
            for (int i = 0; i < _items.Count; i++)
            {
                int h = _items[i].IsSeparator ? SeparatorHeight : ItemHeight;
                if (!_items[i].IsSeparator && p.Y >= y && p.Y < y + h && p.X >= 0 && p.X < Width)
                    return i;
                y += h;
            }
            return -1;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            // Cerrar al perder el foco (click fuera).
            // Diferido vía BeginInvoke: cerrar durante la transición de foco
            // puede provocar que Windows minimice el form owner.
            if (!IsDisposed && IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed) this.Close();
                }));
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Magenta); // transparency key
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Sombra ligera (offset)
            Rectangle shadow = new Rectangle(1, 2, Width - 2, Height - 2);
            using (var path = RoundRect(shadow, 10))
            using (var br = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                g.FillPath(br, path);

            // Card de fondo
            Rectangle card = new Rectangle(0, 0, Width - 2, Height - 4);
            using (var path = RoundRect(card, 10))
            {
                using (var bg = new SolidBrush(Color.White))
                    g.FillPath(bg, path);
                using (var pen = new Pen(Color.FromArgb(228, 222, 213), 1f))
                    g.DrawPath(pen, path);
            }

            // Items
            int y = PaddingTop;
            using (var font = new Font("Segoe UI", 9f, FontStyle.Regular))
            using (var fontBold = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    int h = item.IsSeparator ? SeparatorHeight : ItemHeight;

                    if (item.IsSeparator)
                    {
                        using (var pen = new Pen(Color.FromArgb(238, 232, 222), 1f))
                            g.DrawLine(pen, 14, y + 4, Width - 18, y + 4);
                    }
                    else
                    {
                        Rectangle row = new Rectangle(6, y, Width - 16, h);
                        bool isHover = (i == _hoveredIndex);

                        if (isHover)
                        {
                            using (var rPath = RoundRect(row, 6))
                            using (var bg = new SolidBrush(Color.FromArgb(247, 240, 232)))
                                g.FillPath(bg, rPath);
                        }

                        Color textColor = item.AccentColor ?? Color.FromArgb(48, 42, 36);
                        var f = item.IsBold ? fontBold : font;

                        using (var brush = new SolidBrush(textColor))
                        {
                            var sf = new StringFormat { LineAlignment = StringAlignment.Center };
                            g.DrawString(item.Text, f, brush, new RectangleF(20, y, Width - 40, h), sf);
                        }

                        // Indicador izquierdo en hover
                        if (isHover)
                        {
                            using (var ind = new SolidBrush(Color.FromArgb(217, 142, 24)))
                            {
                                var bar = new Rectangle(10, y + 8, 3, h - 16);
                                using (var bp = RoundRect(bar, 2)) g.FillPath(ind, bp);
                            }
                        }
                    }

                    y += h;
                }
            }
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (radius <= 0 || r.Width < d || r.Height < d) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // =========================================================================
    // PrimaryButton: botón principal con gradiente y esquinas redondeadas.
    // =========================================================================
    public class PrimaryButton : Button
    {
        private Color _gradientStart = Color.FromArgb(179, 108, 46);
        private Color _gradientEnd = Color.FromArgb(217, 142, 24);
        private bool _hovered = false;
        private bool _pressed = false;

        public PrimaryButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Height = 38;
        }

        [Category("Modern UI")]
        public Color GradientStart { get => _gradientStart; set { _gradientStart = value; Invalidate(); } }
        [Category("Modern UI")]
        public Color GradientEnd { get => _gradientEnd; set { _gradientEnd = value; Invalidate(); } }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs mevent) { _pressed = true; Invalidate(); base.OnMouseDown(mevent); }
        protected override void OnMouseUp(MouseEventArgs mevent) { _pressed = false; Invalidate(); base.OnMouseUp(mevent); }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle r = new Rectangle(0, 0, Width, Height);

            // Sombra
            Rectangle shadow = new Rectangle(r.X + 1, r.Y + 2, r.Width - 2, r.Height - 3);
            using (var sp = RoundedRect(shadow, 8))
            using (var sb = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                g.FillPath(sb, sp);

            Rectangle btn = new Rectangle(0, 0, Width - 2, Height - 4);
            using (var path = RoundedRect(btn, 8))
            {
                Color start = _gradientStart;
                Color end = _gradientEnd;
                if (_pressed) { start = Darken(start, 0.85f); end = Darken(end, 0.85f); }
                else if (_hovered) { start = Lighten(start, 1.05f); end = Lighten(end, 1.05f); }

                using (var brush = new LinearGradientBrush(btn, start, end, LinearGradientMode.Vertical))
                    g.FillPath(brush, path);
            }

            // Texto
            TextRenderer.DrawText(g, Text, Font, btn, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private static Color Darken(Color c, float f) =>
            Color.FromArgb(c.A, (int)Math.Max(0, c.R * f), (int)Math.Max(0, c.G * f), (int)Math.Max(0, c.B * f));
        private static Color Lighten(Color c, float f) =>
            Color.FromArgb(c.A, (int)Math.Min(255, c.R * f), (int)Math.Min(255, c.G * f), (int)Math.Min(255, c.B * f));

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
