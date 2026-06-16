using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    partial class FormRepositorioDestajos
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // ===== Paleta =====
        private static readonly Color colorPrimario  = Color.FromArgb(41, 60, 88);
        private static readonly Color colorPrimarioOsc = Color.FromArgb(31, 47, 71);
        private static readonly Color colorAcento    = Color.FromArgb(52, 152, 219);
        private static readonly Color colorExito     = Color.FromArgb(39, 174, 96);
        private static readonly Color colorPeligro   = Color.FromArgb(231, 76, 60);
        private static readonly Color colorNeutro    = Color.FromArgb(127, 140, 141);
        private static readonly Color colorSuave     = Color.FromArgb(248, 250, 253);
        private static readonly Color colorBorde     = Color.FromArgb(218, 224, 232);
        private static readonly Color colorTexto     = Color.FromArgb(33, 47, 67);

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            // ============================================================
            // BANNER SUPERIOR (header con degradado)
            // ============================================================
            var panelBanner = new GradientPanel(colorPrimario, colorPrimarioOsc);
            panelBanner.Dock = DockStyle.Top;
            panelBanner.Height = 90;

            var iconoCirculo = new CirclePanel(Color.FromArgb(60, 255, 255, 255));
            iconoCirculo.Size = new Size(54, 54);
            iconoCirculo.Location = new Point(22, 18);

            var lblIcono = new Label();
            lblIcono.Text = "PDF";
            lblIcono.Font = new Font("Segoe UI Black", 11F, FontStyle.Bold);
            lblIcono.ForeColor = Color.White;
            lblIcono.AutoSize = false;
            lblIcono.Size = iconoCirculo.Size;
            lblIcono.TextAlign = ContentAlignment.MiddleCenter;
            lblIcono.BackColor = Color.Transparent;
            iconoCirculo.Controls.Add(lblIcono);

            this.lblTitulo = new Label();
            this.lblTitulo.Text = "Repositorio de PDFs";
            this.lblTitulo.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            this.lblTitulo.ForeColor = Color.White;
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.BackColor = Color.Transparent;
            this.lblTitulo.Location = new Point(90, 22);

            this.lblSubtitulo = new Label();
            this.lblSubtitulo.Text = "Almacenamiento en base de datos";
            this.lblSubtitulo.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.lblSubtitulo.ForeColor = Color.FromArgb(189, 200, 215);
            this.lblSubtitulo.AutoSize = true;
            this.lblSubtitulo.BackColor = Color.Transparent;
            this.lblSubtitulo.Location = new Point(92, 56);

            panelBanner.Controls.Add(this.lblTitulo);
            panelBanner.Controls.Add(this.lblSubtitulo);
            panelBanner.Controls.Add(iconoCirculo);

            // ============================================================
            // PANEL DE ESTADÍSTICAS (3 tarjetas)
            // ============================================================
            var panelStats = new Panel();
            panelStats.Dock = DockStyle.Top;
            panelStats.Height = 84;
            panelStats.BackColor = colorSuave;
            panelStats.Padding = new Padding(20, 12, 20, 12);

            var card1 = CrearTarjeta("PDFs almacenados", "0", colorAcento, out this.lblStatTotal);
            var card2 = CrearTarjeta("Tamaño total", "0 KB", colorExito, out this.lblStatTamano);
            var card3 = CrearTarjeta("Último PDF", "—", Color.FromArgb(155, 89, 182), out this.lblStatUltimo);

            var layoutStats = new TableLayoutPanel();
            layoutStats.Dock = DockStyle.Fill;
            layoutStats.ColumnCount = 3;
            layoutStats.RowCount = 1;
            layoutStats.BackColor = Color.Transparent;
            layoutStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            layoutStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            layoutStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            layoutStats.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutStats.Margin = new Padding(0);
            layoutStats.Padding = new Padding(0, 0, 0, 0);

            layoutStats.Controls.Add(card1, 0, 0);
            layoutStats.Controls.Add(card2, 1, 0);
            layoutStats.Controls.Add(card3, 2, 0);
            panelStats.Controls.Add(layoutStats);

            // ============================================================
            // PANEL CENTRAL (lista)
            // ============================================================
            var panelCentral = new Panel();
            panelCentral.Dock = DockStyle.Fill;
            panelCentral.Padding = new Padding(20, 8, 20, 8);
            panelCentral.BackColor = Color.White;

            var marco = new Panel();
            marco.Dock = DockStyle.Fill;
            marco.BorderStyle = BorderStyle.None;
            marco.BackColor = colorBorde;
            marco.Padding = new Padding(1);

            this.olvPdfs = new BrightIdeasSoftware.ObjectListView();
            this.olvPdfs.Dock = DockStyle.Fill;
            this.olvPdfs.View = View.Details;
            this.olvPdfs.FullRowSelect = true;
            this.olvPdfs.GridLines = false;
            this.olvPdfs.BorderStyle = BorderStyle.None;
            this.olvPdfs.Font = new Font("Segoe UI", 9.5F);
            this.olvPdfs.BackColor = Color.White;
            this.olvPdfs.EmptyListMsg = "Aún no se han generado PDFs para esta casa.";
            this.olvPdfs.EmptyListMsgFont = new Font("Segoe UI", 11F, FontStyle.Italic);

            marco.Controls.Add(this.olvPdfs);
            panelCentral.Controls.Add(marco);

            // ============================================================
            // PANEL INFERIOR (acciones + estado)
            // ============================================================
            var panelBotones = new Panel();
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Height = 78;
            panelBotones.BackColor = colorSuave;
            panelBotones.Padding = new Padding(20, 12, 20, 14);

            var separadorTop = new Panel();
            separadorTop.Dock = DockStyle.Top;
            separadorTop.Height = 1;
            separadorTop.BackColor = colorBorde;
            panelBotones.Controls.Add(separadorTop);

            this.btnAbrir       = CrearBotonPlano("Abrir PDF",      colorAcento,                  new Point(22, 18),  120);
            this.btnGuardarComo = CrearBotonPlano("Guardar copia…", colorExito,                   new Point(146, 18), 130);
            this.btnEliminar    = CrearBotonPlano("Eliminar",       colorPeligro,                 new Point(282, 18), 110);
            this.btnActualizar  = CrearBotonPlano("Actualizar",     Color.FromArgb(155, 89, 182), new Point(398, 18), 110);

            this.btnAbrir.Click       += new System.EventHandler(this.btnAbrir_Click);
            this.btnGuardarComo.Click += new System.EventHandler(this.btnGuardarComo_Click);
            this.btnEliminar.Click    += new System.EventHandler(this.btnEliminar_Click);
            this.btnActualizar.Click  += new System.EventHandler(this.btnActualizar_Click);

            this.btnCerrar = CrearBotonPlano("Cerrar", Color.FromArgb(99, 110, 114), new Point(0, 18), 100);
            this.btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);

            this.lblEstado = new Label();
            this.lblEstado.Text = "";
            this.lblEstado.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.lblEstado.AutoSize = false;
            this.lblEstado.TextAlign = ContentAlignment.MiddleRight;
            this.lblEstado.Size = new Size(360, 22);
            this.lblEstado.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            this.lblEstado.ForeColor = colorNeutro;

            panelBotones.Controls.Add(this.btnAbrir);
            panelBotones.Controls.Add(this.btnGuardarComo);
            panelBotones.Controls.Add(this.btnEliminar);
            panelBotones.Controls.Add(this.btnActualizar);
            panelBotones.Controls.Add(this.lblEstado);
            panelBotones.Controls.Add(this.btnCerrar);

            // Reposicionar btnCerrar y lblEstado al cargar (anchored a la derecha)
            panelBotones.Resize += (s, e) =>
            {
                this.btnCerrar.Left = panelBotones.ClientSize.Width - this.btnCerrar.Width - 22;
                this.lblEstado.Left = this.btnCerrar.Left - this.lblEstado.Width - 16;
                this.lblEstado.Top = 22;
            };

            // ============================================================
            // FORMULARIO
            // ============================================================
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1020, 620);
            this.MinimumSize = new Size(840, 520);
            this.BackColor = Color.White;
            this.Controls.Add(panelCentral);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelStats);
            this.Controls.Add(panelBanner);
            this.Name = "FormRepositorioDestajos";
            this.Text = "Repositorio de PDFs";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F);
            this.DoubleBuffered = true;
            this.ResumeLayout(false);
        }

        // ============================================================
        // Helpers de UI
        // ============================================================
        private static Panel CrearTarjeta(string etiqueta, string valorInicial, Color acento, out Label lblValor)
        {
            var card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = Color.White;
            card.Margin = new Padding(6, 0, 6, 0);
            card.Padding = new Padding(14, 8, 14, 8);

            // Borde sutil
            card.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (var pen = new Pen(Color.FromArgb(228, 233, 240)))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
                // Acento lateral
                using (var brush = new SolidBrush(acento))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, 4, card.Height);
                }
            };

            var lblEtiqueta = new Label();
            lblEtiqueta.Text = etiqueta.ToUpper();
            lblEtiqueta.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblEtiqueta.ForeColor = Color.FromArgb(127, 140, 141);
            lblEtiqueta.AutoSize = false;
            lblEtiqueta.Dock = DockStyle.Top;
            lblEtiqueta.Height = 18;
            lblEtiqueta.TextAlign = ContentAlignment.MiddleLeft;
            lblEtiqueta.Padding = new Padding(8, 0, 0, 0);

            lblValor = new Label();
            lblValor.Text = valorInicial;
            lblValor.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            lblValor.ForeColor = Color.FromArgb(33, 47, 67);
            lblValor.AutoSize = false;
            lblValor.Dock = DockStyle.Fill;
            lblValor.TextAlign = ContentAlignment.MiddleLeft;
            lblValor.Padding = new Padding(8, 0, 0, 0);

            card.Controls.Add(lblValor);
            card.Controls.Add(lblEtiqueta);
            return card;
        }

        private static Button CrearBotonPlano(string texto, Color color, Point ubicacion, int ancho)
        {
            var btn = new Button();
            btn.Text = texto;
            btn.Location = ubicacion;
            btn.Size = new Size(ancho, 34);
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;

            // Hover effect
            var colorHover = ControlPaint.Light(color, 0.15f);
            var colorOriginal = color;
            btn.MouseEnter += (s, e) => btn.BackColor = colorHover;
            btn.MouseLeave += (s, e) => btn.BackColor = colorOriginal;

            return btn;
        }

        // ============================================================
        // Paneles personalizados (degradado / círculo)
        // ============================================================
        private class GradientPanel : Panel
        {
            private readonly Color c1, c2;
            public GradientPanel(Color c1, Color c2)
            {
                this.c1 = c1;
                this.c2 = c2;
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                using (var brush = new LinearGradientBrush(this.ClientRectangle, c1, c2, 0F))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
                base.OnPaint(e);
            }
            protected override void OnResize(System.EventArgs eventargs)
            {
                base.OnResize(eventargs);
                this.Invalidate();
            }
        }

        private class CirclePanel : Panel
        {
            private readonly Color colorRelleno;
            public CirclePanel(Color color)
            {
                this.colorRelleno = color;
                this.BackColor = Color.Transparent;
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.SupportsTransparentBackColor, true);
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(colorRelleno))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, this.Width - 1, this.Height - 1);
                }
                base.OnPaint(e);
            }
        }

        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblStatTotal;
        private Label lblStatTamano;
        private Label lblStatUltimo;
        private BrightIdeasSoftware.ObjectListView olvPdfs;
        private Button btnAbrir;
        private Button btnGuardarComo;
        private Button btnEliminar;
        private Button btnActualizar;
        private Button btnCerrar;
        private Label lblEstado;
    }
}
