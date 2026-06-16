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
            lblTitulo.Text = "GESTIÓN DE DESTAJOS";
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

            // ===== Panel central (TreeListView) =====
            var panelCentral = new Panel();
            panelCentral.Dock = DockStyle.Fill;
            panelCentral.Padding = new Padding(20, 14, 20, 14);
            panelCentral.BackColor = Color.White;

            var marco = new Panel();
            marco.Dock = DockStyle.Fill;
            marco.BorderStyle = BorderStyle.FixedSingle;
            marco.BackColor = colorBorde;
            marco.Padding = new Padding(1);

            this.olvTareas = new DynamicSepticSystem.SafeTreeListView();
            this.olvTareas.Dock = DockStyle.Fill;
            this.olvTareas.View = View.Details;
            this.olvTareas.FullRowSelect = true;
            this.olvTareas.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.olvTareas.GridLines = false;
            this.olvTareas.BorderStyle = BorderStyle.None;
            this.olvTareas.Font = new Font("Segoe UI", 9.25F);

            marco.Controls.Add(this.olvTareas);
            panelCentral.Controls.Add(marco);

            // ===== Panel inferior =====
            var panelBotones = new Panel();
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Height = 138;
            panelBotones.BackColor = colorSuave;
            panelBotones.Padding = new Padding(20, 12, 20, 12);

            var separadorTop = new Panel();
            separadorTop.Dock = DockStyle.Top;
            separadorTop.Height = 1;
            separadorTop.BackColor = colorBorde;
            panelBotones.Controls.Add(separadorTop);

            // --- Fila 1: acciones masivas ---
            this.btnMarcarTodos = new Button();
            this.btnMarcarTodos.Text = "Marcar Todo";
            this.btnMarcarTodos.Location = new Point(22, 16);
            this.btnMarcarTodos.Size = new Size(120, 32);
            this.btnMarcarTodos.BackColor = colorExito;
            this.btnMarcarTodos.ForeColor = Color.White;
            this.btnMarcarTodos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnMarcarTodos.FlatStyle = FlatStyle.Flat;
            this.btnMarcarTodos.FlatAppearance.BorderSize = 0;
            this.btnMarcarTodos.Cursor = Cursors.Hand;
            this.btnMarcarTodos.Click += new System.EventHandler(this.btnMarcarTodos_Click);

            this.btnDesmarcarTodos = new Button();
            this.btnDesmarcarTodos.Text = "Desmarcar Todo";
            this.btnDesmarcarTodos.Location = new Point(148, 16);
            this.btnDesmarcarTodos.Size = new Size(140, 32);
            this.btnDesmarcarTodos.BackColor = colorPeligro;
            this.btnDesmarcarTodos.ForeColor = Color.White;
            this.btnDesmarcarTodos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnDesmarcarTodos.FlatStyle = FlatStyle.Flat;
            this.btnDesmarcarTodos.FlatAppearance.BorderSize = 0;
            this.btnDesmarcarTodos.Cursor = Cursors.Hand;
            this.btnDesmarcarTodos.Click += new System.EventHandler(this.btnDesmarcarTodos_Click);

            this.btnMarcarPorNivel = new Button();
            this.btnMarcarPorNivel.Text = "+ Por Nivel";
            this.btnMarcarPorNivel.Location = new Point(294, 16);
            this.btnMarcarPorNivel.Size = new Size(110, 32);
            this.btnMarcarPorNivel.BackColor = colorAcento;
            this.btnMarcarPorNivel.ForeColor = Color.White;
            this.btnMarcarPorNivel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnMarcarPorNivel.FlatStyle = FlatStyle.Flat;
            this.btnMarcarPorNivel.FlatAppearance.BorderSize = 0;
            this.btnMarcarPorNivel.Cursor = Cursors.Hand;
            this.btnMarcarPorNivel.Click += new System.EventHandler(this.btnMarcarPorNivel_Click);

            this.btnDesmarcarPorNivel = new Button();
            this.btnDesmarcarPorNivel.Text = "− Por Nivel";
            this.btnDesmarcarPorNivel.Location = new Point(410, 16);
            this.btnDesmarcarPorNivel.Size = new Size(110, 32);
            this.btnDesmarcarPorNivel.BackColor = colorNeutro;
            this.btnDesmarcarPorNivel.ForeColor = Color.White;
            this.btnDesmarcarPorNivel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnDesmarcarPorNivel.FlatStyle = FlatStyle.Flat;
            this.btnDesmarcarPorNivel.FlatAppearance.BorderSize = 0;
            this.btnDesmarcarPorNivel.Cursor = Cursors.Hand;
            this.btnDesmarcarPorNivel.Click += new System.EventHandler(this.btnDesmarcarPorNivel_Click);

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
            this.lblPorcentaje.Size = new Size(280, 20);
            this.lblPorcentaje.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            this.lblPorcentaje.ForeColor = colorPrimario;
            this.lblPorcentaje.TextAlign = ContentAlignment.MiddleLeft;

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

            panelBotones.Controls.Add(this.btnMarcarTodos);
            panelBotones.Controls.Add(this.btnDesmarcarTodos);
            panelBotones.Controls.Add(this.btnMarcarPorNivel);
            panelBotones.Controls.Add(this.btnDesmarcarPorNivel);
            panelBotones.Controls.Add(this.btnDestajosPorCuadrilla);
            panelBotones.Controls.Add(this.btnRepositorio);
            panelBotones.Controls.Add(this.btnGuardar);
            panelBotones.Controls.Add(this.btnCerrar);
            panelBotones.Controls.Add(this.lblEstadisticas);
            panelBotones.Controls.Add(this.progressBarActivacion);
            panelBotones.Controls.Add(this.lblPorcentaje);

            // ===== Formulario =====
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1460, 820);
            this.MinimumSize = new Size(1180, 700);
            this.BackColor = Color.White;
            this.Controls.Add(panelCentral);
            this.Controls.Add(ConstruirPanelGuia());
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelFiltros);
            this.Controls.Add(panelBanner);
            this.Name = "FormActivarTareasTreeList";
            this.Text = "Gestión de Destajos por Casa";
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
        private Button btnMarcarTodos;
        private Button btnDesmarcarTodos;
        private Button btnMarcarPorNivel;
        private Button btnDesmarcarPorNivel;
        private Label lblEstadisticas;
        private ProgressBar progressBarActivacion;
        private Label lblPorcentaje;
        private Button btnRepositorio;
        private Button btnDestajosPorCuadrilla;
        private Button btnGuardar;
        private Button btnCerrar;
    }
}
