using System.Drawing;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    partial class FormActivarTareasTreeList
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

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // ===== Paleta =====
            Color colorPrimario = Color.FromArgb(41, 60, 88);          // Azul navy oscuro
            Color colorAcento = Color.FromArgb(52, 152, 219);          // Azul claro
            Color colorExito = Color.FromArgb(39, 174, 96);
            Color colorPeligro = Color.FromArgb(231, 76, 60);
            Color colorNeutro = Color.FromArgb(127, 140, 141);
            Color colorSuave = Color.FromArgb(248, 250, 253);
            Color colorBorde = Color.FromArgb(218, 224, 232);

            // ===== Banner superior =====
            var panelBanner = new Panel();
            panelBanner.Dock = DockStyle.Top;
            panelBanner.Height = 64;
            panelBanner.BackColor = colorPrimario;

            var lblTitulo = new Label();
            lblTitulo.Text = "ACTIVACIÓN DE DESTAJOS";
            lblTitulo.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(20, 10);

            var lblSubtitulo = new Label();
            lblSubtitulo.Text = "Activa, asigna cuadrilla y genera la orden de cada destajo";
            lblSubtitulo.Font = new Font("Segoe UI", 9F);
            lblSubtitulo.ForeColor = Color.FromArgb(189, 200, 215);
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Location = new Point(22, 38);

            panelBanner.Controls.Add(lblSubtitulo);
            panelBanner.Controls.Add(lblTitulo);

            // ===== Panel de filtros =====
            var panelFiltros = new Panel();
            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Height = 96;
            panelFiltros.BackColor = colorSuave;
            panelFiltros.Padding = new Padding(20, 12, 20, 10);

            var lblManzana = new Label();
            lblManzana.Text = "Manzana";
            lblManzana.Location = new Point(22, 14);
            lblManzana.Size = new Size(80, 18);
            lblManzana.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblManzana.ForeColor = colorNeutro;

            this.cmbManzana = new ComboBox();
            this.cmbManzana.Location = new Point(22, 33);
            this.cmbManzana.Size = new Size(120, 26);
            this.cmbManzana.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbManzana.Font = new Font("Segoe UI", 10F);
            this.cmbManzana.FlatStyle = FlatStyle.Flat;

            var lblLote = new Label();
            lblLote.Text = "Lote";
            lblLote.Location = new Point(154, 14);
            lblLote.Size = new Size(60, 18);
            lblLote.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblLote.ForeColor = colorNeutro;

            this.cmbLote = new ComboBox();
            this.cmbLote.Location = new Point(154, 33);
            this.cmbLote.Size = new Size(110, 26);
            this.cmbLote.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbLote.Font = new Font("Segoe UI", 10F);
            this.cmbLote.FlatStyle = FlatStyle.Flat;

            this.btnCargar = new Button();
            this.btnCargar.Text = "Cargar Tareas";
            this.btnCargar.Location = new Point(276, 33);
            this.btnCargar.Size = new Size(150, 28);
            this.btnCargar.BackColor = colorAcento;
            this.btnCargar.ForeColor = Color.White;
            this.btnCargar.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.btnCargar.FlatStyle = FlatStyle.Flat;
            this.btnCargar.FlatAppearance.BorderSize = 0;
            this.btnCargar.Cursor = Cursors.Hand;
            this.btnCargar.Click += new System.EventHandler(this.btnCargar_Click);

            var lblBuscar = new Label();
            lblBuscar.Text = "Buscar";
            lblBuscar.Location = new Point(450, 14);
            lblBuscar.Size = new Size(60, 18);
            lblBuscar.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblBuscar.ForeColor = colorNeutro;

            this.txtBuscar = new TextBox();
            this.txtBuscar.Location = new Point(450, 34);
            this.txtBuscar.Size = new Size(260, 26);
            this.txtBuscar.Font = new Font("Segoe UI", 10F);
            this.txtBuscar.BorderStyle = BorderStyle.FixedSingle;

            this.lblContextoCasa = new Label();
            this.lblContextoCasa.Text = "Selecciona una casa para comenzar";
            this.lblContextoCasa.Location = new Point(22, 68);
            this.lblContextoCasa.Size = new Size(900, 20);
            this.lblContextoCasa.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            this.lblContextoCasa.ForeColor = colorNeutro;

            panelFiltros.Controls.Add(lblManzana);
            panelFiltros.Controls.Add(this.cmbManzana);
            panelFiltros.Controls.Add(lblLote);
            panelFiltros.Controls.Add(this.cmbLote);
            panelFiltros.Controls.Add(this.btnCargar);
            panelFiltros.Controls.Add(lblBuscar);
            panelFiltros.Controls.Add(this.txtBuscar);
            panelFiltros.Controls.Add(this.lblContextoCasa);

            // ===== Panel central (TreeListView) — oculto; se muestra en ventana aparte =====
            this.panelCentralOculto = new Panel();
            this.panelCentralOculto.Dock = DockStyle.Fill;
            this.panelCentralOculto.Padding = new Padding(20, 14, 20, 14);
            this.panelCentralOculto.BackColor = Color.White;
            this.panelCentralOculto.Visible = false;

            this.marcoArbol = new Panel();
            this.marcoArbol.Dock = DockStyle.Fill;
            this.marcoArbol.BorderStyle = BorderStyle.FixedSingle;
            this.marcoArbol.BackColor = colorBorde;
            this.marcoArbol.Padding = new Padding(1);

            this.olvTareas = new DynamicSepticSystem.SafeTreeListView();
            this.olvTareas.Dock = DockStyle.Fill;
            this.olvTareas.View = View.Details;
            this.olvTareas.FullRowSelect = true;
            this.olvTareas.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.olvTareas.GridLines = false;
            this.olvTareas.BorderStyle = BorderStyle.None;
            this.olvTareas.Font = new Font("Segoe UI", 9.25F);

            this.marcoArbol.Controls.Add(this.olvTareas);
            this.panelCentralOculto.Controls.Add(this.marcoArbol);

            // ===== Panel inferior =====
            var panelBotones = new Panel();
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Height = 160;
            panelBotones.BackColor = colorSuave;
            panelBotones.Padding = new Padding(20, 12, 20, 12);

            var separadorTop = new Panel();
            separadorTop.Dock = DockStyle.Top;
            separadorTop.Height = 1;
            separadorTop.BackColor = colorBorde;
            panelBotones.Controls.Add(separadorTop);

            // --- Fila 1: ver árbol completo (ventana aparte) ---
            this.btnArbol = new Button();
            this.btnArbol.Text = "Ver árbol completo";
            this.btnArbol.Location = new Point(22, 16);
            this.btnArbol.Size = new Size(200, 32);
            this.btnArbol.BackColor = colorPrimario;
            this.btnArbol.ForeColor = Color.White;
            this.btnArbol.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnArbol.FlatStyle = FlatStyle.Flat;
            this.btnArbol.FlatAppearance.BorderSize = 0;
            this.btnArbol.Cursor = Cursors.Hand;
            this.btnArbol.Click += new System.EventHandler(this.btnArbol_Click);

            // --- Fila 2: estadísticas ---
            this.lblEstadisticas = new Label();
            this.lblEstadisticas.Text = "Sin datos cargados";
            this.lblEstadisticas.Location = new Point(22, 60);
            this.lblEstadisticas.Size = new Size(1040, 18);
            this.lblEstadisticas.Font = new Font("Segoe UI", 9F);
            this.lblEstadisticas.ForeColor = Color.FromArgb(64, 80, 100);

            this.progressBarActivacion = new ProgressBar();
            this.progressBarActivacion.Location = new Point(22, 84);
            this.progressBarActivacion.Size = new Size(720, 16);
            this.progressBarActivacion.Style = ProgressBarStyle.Continuous;

            this.lblPorcentaje = new Label();
            this.lblPorcentaje.Text = "0%";
            this.lblPorcentaje.Location = new Point(752, 82);
            this.lblPorcentaje.Size = new Size(380, 20);
            this.lblPorcentaje.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            this.lblPorcentaje.ForeColor = colorPrimario;
            this.lblPorcentaje.TextAlign = ContentAlignment.MiddleLeft;

            // --- Fila 3: barra de etapas (Obra Negra / Albañilería / Acabados / Acabados Finales) ---
            this.panelStages = new Panel();
            this.panelStages.Location = new Point(22, 110);
            this.panelStages.Size = new Size(1040, 30);
            this.panelStages.BackColor = colorSuave;

            // --- Botones derecha ---
            this.btnDestajosPorCuadrilla = new Button();
            this.btnDestajosPorCuadrilla.Text = "Reportes ▼";
            this.btnDestajosPorCuadrilla.Location = new Point(540, 16);
            this.btnDestajosPorCuadrilla.Size = new Size(180, 32);
            this.btnDestajosPorCuadrilla.BackColor = Color.FromArgb(22, 160, 133);
            this.btnDestajosPorCuadrilla.ForeColor = Color.White;
            this.btnDestajosPorCuadrilla.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnDestajosPorCuadrilla.FlatStyle = FlatStyle.Flat;
            this.btnDestajosPorCuadrilla.FlatAppearance.BorderSize = 0;
            this.btnDestajosPorCuadrilla.Cursor = Cursors.Hand;
            this.btnDestajosPorCuadrilla.Click += new System.EventHandler(this.btnDestajosPorCuadrilla_Click);

            this.btnRepositorio = new Button();
            this.btnRepositorio.Text = "Repositorio PDFs";
            this.btnRepositorio.Location = new Point(726, 16);
            this.btnRepositorio.Size = new Size(140, 32);
            this.btnRepositorio.BackColor = Color.FromArgb(41, 128, 185);
            this.btnRepositorio.ForeColor = Color.White;
            this.btnRepositorio.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnRepositorio.FlatStyle = FlatStyle.Flat;
            this.btnRepositorio.FlatAppearance.BorderSize = 0;
            this.btnRepositorio.Cursor = Cursors.Hand;
            this.btnRepositorio.Click += new System.EventHandler(this.btnRepositorio_Click);

            this.btnGuardar = new Button();
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.Location = new Point(872, 16);
            this.btnGuardar.Size = new Size(96, 32);
            this.btnGuardar.BackColor = Color.FromArgb(155, 89, 182);
            this.btnGuardar.ForeColor = Color.White;
            this.btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnGuardar.FlatStyle = FlatStyle.Flat;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.Cursor = Cursors.Hand;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);

            this.btnCerrar = new Button();
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.Location = new Point(974, 16);
            this.btnCerrar.Size = new Size(90, 32);
            this.btnCerrar.BackColor = Color.FromArgb(99, 110, 114);
            this.btnCerrar.ForeColor = Color.White;
            this.btnCerrar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnCerrar.FlatStyle = FlatStyle.Flat;
            this.btnCerrar.FlatAppearance.BorderSize = 0;
            this.btnCerrar.Cursor = Cursors.Hand;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);

            panelBotones.Controls.Add(this.btnArbol);
            panelBotones.Controls.Add(this.btnDestajosPorCuadrilla);
            panelBotones.Controls.Add(this.btnRepositorio);
            panelBotones.Controls.Add(this.btnGuardar);
            panelBotones.Controls.Add(this.btnCerrar);
            panelBotones.Controls.Add(this.lblEstadisticas);
            panelBotones.Controls.Add(this.progressBarActivacion);
            panelBotones.Controls.Add(this.lblPorcentaje);
            panelBotones.Controls.Add(this.panelStages);

            // ===== Formulario =====
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1460, 820);
            this.MinimumSize = new Size(1180, 700);
            this.BackColor = Color.White;
            this.Controls.Add(ConstruirPanelPasos());
            this.Controls.Add(this.panelCentralOculto);
            this.Controls.Add(ConstruirPanelGuia());
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelFiltros);
            this.Controls.Add(panelBanner);
            this.Name = "FormActivarTareasTreeList";
            this.Text = "Activación de Destajos";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F);
            this.DoubleBuffered = true;
        }

        private ComboBox cmbManzana;
        private ComboBox cmbLote;
        private Button btnCargar;
        private TextBox txtBuscar;
        private Label lblContextoCasa;
        private DynamicSepticSystem.SafeTreeListView olvTareas;
        private Panel panelCentralOculto;
        private Panel marcoArbol;
        private Button btnArbol;
        private Label lblEstadisticas;
        private ProgressBar progressBarActivacion;
        private Label lblPorcentaje;
        private Panel panelStages;
        private Button btnRepositorio;
        private Button btnDestajosPorCuadrilla;
        private Button btnGuardar;
        private Button btnCerrar;
    }
}
