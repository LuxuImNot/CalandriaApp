// Designer file for FormAlmacen - UI Controls
using System;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    partial class FormAlmacen
    {
        private System.ComponentModel.IContainer components = null;

        // Tab controls
        public TabControl TabsControl;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPageHistorial;
        private TabPage tabPageInventario;
        private TabPage tabPageConsultaCasa; // ? NUEVA PESTA?A

        // Tab 1: ENTRADAS
        private FlowLayoutPanel flwCasasAsociadas;
        private Button btnEliminardeOrden;
        // private Button btnEditarNombreOrden; // ? ELIMINADO - No es necesario
        private Label label6;
        private Label label3;
        private TextBox txtBuscarEntrada;
        private Label lblCasa;
        private Label lblLoteDisplay;
        private Label lblManzanaDisplay;
        private Button btnCapturarEntrada;
        private DataGridView dgvEntrada;
        private ComboBox cmbOrdenesCompra;
        private Button btnBuscarOrden;

        // Tab 2: SALIDAS
        private Label lblCasaSalida;
        private Label label7;
        private Button btnSeleccionarCasaSalida;
        private Label lblSummary;
        private TextBox txtBuscarSalida;
        private TextBox txtTrimLoteSalida;
        private TextBox txtTrimManzanaSalida;
        private Label lblInstrucciones;
        private DataGridView dgvInsumos;
        private Label label1;
        private Label label2;
        private Button btnRegistrarSalida;
        private Button btnVerRepositorioVales; // ?? NUEVO BOTÓN

        // Tab 3: HISTORIAL
        private ObjectListView objectListViewHistorial;

        // Tab 4: INVENTARIO
        private DataGridView dgvInventario;
        private TextBox txtBuscarInventario;
        private Label lblTotalInventario;
        private Label lblBuscarInventario;
        private Button btnExportarInventario;
        private Button btnActualizarInventario;

        // Tab 5: CONSULTA POR CASA ? NUEVA
        private ComboBox cmbManzanaConsulta;
        private ComboBox cmbLoteConsulta;
        private DataGridView dgvConsultaCasa;
        private TextBox txtBuscarConsultaCasa;
        private Label lblManzanaConsulta;
        private Label lblLoteConsulta;
        private Label lblBuscarConsultaCasa;
        private Label lblCasaSeleccionada;
        private Label lblPrototipoConsulta;
        private Label lblTotalInsumosConsulta;
        private Label lblTotalImporteConsulta;
        private Label lblDiscrepanciasConsulta;
        private Button btnExportarConsultaCasa;
        private Button btnActualizarConsultaCasa;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.tabPageHistorial = new System.Windows.Forms.TabPage();
            this.objectListViewHistorial = new BrightIdeasSoftware.ObjectListView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lblCasaSalida = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnSeleccionarCasaSalida = new System.Windows.Forms.Button();
            this.lblSummary = new System.Windows.Forms.Label();
            this.txtBuscarSalida = new System.Windows.Forms.TextBox();
            this.txtTrimLoteSalida = new System.Windows.Forms.TextBox();
            this.txtTrimManzanaSalida = new System.Windows.Forms.TextBox();
            this.lblInstrucciones = new System.Windows.Forms.Label();
            this.dgvInsumos = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnRegistrarSalida = new System.Windows.Forms.Button();
            this.btnVerRepositorioVales = new System.Windows.Forms.Button(); // ?? NUEVO BOTÓN
            this.TabsControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.flwCasasAsociadas = new System.Windows.Forms.FlowLayoutPanel();
            this.btnEliminardeOrden = new System.Windows.Forms.Button();
            // this.btnEditarNombreOrden = new System.Windows.Forms.Button(); // ? ELIMINADO
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBuscarEntrada = new System.Windows.Forms.TextBox();
            this.lblCasa = new System.Windows.Forms.Label();
            this.lblLoteDisplay = new System.Windows.Forms.Label();
            this.lblManzanaDisplay = new System.Windows.Forms.Label();
            this.btnCapturarEntrada = new System.Windows.Forms.Button();
            this.dgvEntrada = new System.Windows.Forms.DataGridView();
            this.cmbOrdenesCompra = new System.Windows.Forms.ComboBox();
            this.btnBuscarOrden = new System.Windows.Forms.Button();
            // ? CONTROLES NUEVOS PARA INVENTARIO
            this.tabPageInventario = new System.Windows.Forms.TabPage();
            this.dgvInventario = new System.Windows.Forms.DataGridView();
            this.txtBuscarInventario = new System.Windows.Forms.TextBox();
            this.lblTotalInventario = new System.Windows.Forms.Label();
            this.lblBuscarInventario = new System.Windows.Forms.Label();
            this.btnExportarInventario = new System.Windows.Forms.Button();
            this.btnActualizarInventario = new System.Windows.Forms.Button();
            // ? CONTROLES NUEVOS PARA CONSULTA POR CASA
            this.tabPageConsultaCasa = new System.Windows.Forms.TabPage();
            this.cmbManzanaConsulta = new System.Windows.Forms.ComboBox();
            this.cmbLoteConsulta = new System.Windows.Forms.ComboBox();
            this.dgvConsultaCasa = new System.Windows.Forms.DataGridView();
            this.txtBuscarConsultaCasa = new System.Windows.Forms.TextBox();
            this.lblManzanaConsulta = new System.Windows.Forms.Label();
            this.lblLoteConsulta = new System.Windows.Forms.Label();
            this.lblBuscarConsultaCasa = new System.Windows.Forms.Label();
            this.lblCasaSeleccionada = new System.Windows.Forms.Label();
            this.lblPrototipoConsulta = new System.Windows.Forms.Label();
            this.lblTotalInsumosConsulta = new System.Windows.Forms.Label();
            this.lblTotalImporteConsulta = new System.Windows.Forms.Label();
            this.lblDiscrepanciasConsulta = new System.Windows.Forms.Label();
            this.btnExportarConsultaCasa = new System.Windows.Forms.Button();
            this.btnActualizarConsultaCasa = new System.Windows.Forms.Button();
            this.tabPageHistorial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectListViewHistorial)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInsumos)).BeginInit();
            this.TabsControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEntrada)).BeginInit();
            this.tabPageInventario.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInventario)).BeginInit();
            this.tabPageConsultaCasa.SuspendLayout(); // ? NUEVO
            ((System.ComponentModel.ISupportInitialize)(this.dgvConsultaCasa)).BeginInit(); // ? NUEVO
            this.SuspendLayout();
            // 
            // tabPageHistorial
            // 
            this.tabPageHistorial.Controls.Add(this.objectListViewHistorial);
            this.tabPageHistorial.Name = "tabPageHistorial";
            this.tabPageHistorial.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHistorial.TabIndex = 2;
            this.tabPageHistorial.Text = "HISTORIAL";
            this.tabPageHistorial.UseVisualStyleBackColor = true;
            // 
            // objectListViewHistorial
            // 
            this.objectListViewHistorial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectListViewHistorial.HideSelection = false;
            this.objectListViewHistorial.Name = "objectListViewHistorial";
            this.objectListViewHistorial.TabIndex = 0;
            this.objectListViewHistorial.UseCompatibleStateImageBehavior = false;
            this.objectListViewHistorial.View = System.Windows.Forms.View.Details;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnVerRepositorioVales);
            this.tabPage2.Controls.Add(this.lblCasaSalida);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.btnSeleccionarCasaSalida);
            this.tabPage2.Controls.Add(this.lblSummary);
            this.tabPage2.Controls.Add(this.txtBuscarSalida);
            this.tabPage2.Controls.Add(this.txtTrimLoteSalida);
            this.tabPage2.Controls.Add(this.txtTrimManzanaSalida);
            this.tabPage2.Controls.Add(this.lblInstrucciones);
            this.tabPage2.Controls.Add(this.dgvInsumos);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.btnRegistrarSalida);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "SALIDAS";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lblCasaSalida
            // 
            this.lblCasaSalida.AutoSize = true;
            this.lblCasaSalida.Name = "lblCasaSalida";
            this.lblCasaSalida.TabIndex = 19;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Name = "label7";
            this.label7.TabIndex = 18;
            this.label7.Text = "BUSCAR INSUMO";
            // 
            // btnSeleccionarCasaSalida
            // 
            this.btnSeleccionarCasaSalida.Name = "btnSeleccionarCasaSalida";
            this.btnSeleccionarCasaSalida.TabIndex = 17;
            this.btnSeleccionarCasaSalida.Text = "SELECCIONAR CASA";
            this.btnSeleccionarCasaSalida.UseVisualStyleBackColor = true;
            this.btnSeleccionarCasaSalida.Click += new System.EventHandler(this.BtnSeleccionarCasaSalida_Click);
            // 
            // lblSummary
            // 
            this.lblSummary.AutoSize = true;
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.TabIndex = 16;
            // 
            // txtBuscarSalida
            // 
            this.txtBuscarSalida.Name = "txtBuscarSalida";
            this.txtBuscarSalida.TabIndex = 15;
            this.txtBuscarSalida.TextChanged += new System.EventHandler(this.TxtBuscarSalida_TextChanged);
            // 
            // txtTrimLoteSalida
            // 
            this.txtTrimLoteSalida.Name = "txtTrimLoteSalida";
            this.txtTrimLoteSalida.TabIndex = 9;
            // 
            // txtTrimManzanaSalida
            // 
            this.txtTrimManzanaSalida.Name = "txtTrimManzanaSalida";
            this.txtTrimManzanaSalida.TabIndex = 8;
            // 
            // lblInstrucciones
            // 
            this.lblInstrucciones.AutoSize = true;
            this.lblInstrucciones.Name = "lblInstrucciones";
            this.lblInstrucciones.TabIndex = 14;
            this.lblInstrucciones.Text = "SELECCIONA CASA DESTINO";
            // 
            // dgvInsumos
            // 
            this.dgvInsumos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInsumos.Name = "dgvInsumos";
            this.dgvInsumos.TabIndex = 13;
            this.dgvInsumos.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvInsumos_CellContentClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Name = "label1";
            this.label1.TabIndex = 12;
            this.label1.Text = "LOTE";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Name = "label2";
            this.label2.TabIndex = 11;
            this.label2.Text = "MANZANA";
            // 
            // btnRegistrarSalida
            // 
            this.btnRegistrarSalida.Name = "btnRegistrarSalida";
            this.btnRegistrarSalida.TabIndex = 10;
            this.btnRegistrarSalida.Text = "CAPTURAR SALIDA";
            this.btnRegistrarSalida.UseVisualStyleBackColor = true;
            // 
            // btnVerRepositorioVales
            // 
            this.btnVerRepositorioVales.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVerRepositorioVales.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnVerRepositorioVales.FlatAppearance.BorderSize = 0;
            this.btnVerRepositorioVales.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerRepositorioVales.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVerRepositorioVales.ForeColor = System.Drawing.Color.White;
            this.btnVerRepositorioVales.Location = new System.Drawing.Point(1042, 635);
            this.btnVerRepositorioVales.Name = "btnVerRepositorioVales";
            this.btnVerRepositorioVales.Size = new System.Drawing.Size(215, 45);
            this.btnVerRepositorioVales.TabIndex = 20;
            this.btnVerRepositorioVales.Text = "\U0001F4E6 VER VALES DE SALIDA";
            this.btnVerRepositorioVales.UseVisualStyleBackColor = false;
            this.btnVerRepositorioVales.Click += new System.EventHandler(this.BtnVerRepositorioVales_Click);
            // 
            // TabsControl
            // 
            this.TabsControl.Controls.Add(this.tabPage1);
            this.TabsControl.Controls.Add(this.tabPage2);
            this.TabsControl.Controls.Add(this.tabPageInventario);
            this.TabsControl.Controls.Add(this.tabPageConsultaCasa); // ? NUEVA
            this.TabsControl.Controls.Add(this.tabPageHistorial);
            this.TabsControl.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TabsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabsControl.Name = "TabsControl";
            this.TabsControl.SelectedIndex = 0;
            this.TabsControl.TabIndex = 0;
            this.TabsControl.SelectedIndexChanged += new System.EventHandler(this.TabsControl_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.flwCasasAsociadas);
            // this.tabPage1.Controls.Add(this.btnEditarNombreOrden); // ? ELIMINADO
            this.tabPage1.Controls.Add(this.btnEliminardeOrden);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.txtBuscarEntrada);
            this.tabPage1.Controls.Add(this.lblCasa);
            this.tabPage1.Controls.Add(this.lblLoteDisplay);
            this.tabPage1.Controls.Add(this.lblManzanaDisplay);
            this.tabPage1.Controls.Add(this.btnCapturarEntrada);
            this.tabPage1.Controls.Add(this.dgvEntrada);
            this.tabPage1.Controls.Add(this.cmbOrdenesCompra);
            this.tabPage1.Controls.Add(this.btnBuscarOrden);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "ENTRADAS";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // flwCasasAsociadas
            // 
            this.flwCasasAsociadas.Name = "flwCasasAsociadas";
            this.flwCasasAsociadas.TabIndex = 20;
            // 
            // btnEliminardeOrden
            // 
            this.btnEliminardeOrden.Name = "btnEliminardeOrden";
            this.btnEliminardeOrden.TabIndex = 19;
            this.btnEliminardeOrden.Text = "ELIMINAR INSUMO(S) SELECCIONADOS";
            this.btnEliminardeOrden.UseVisualStyleBackColor = true;
            this.btnEliminardeOrden.Click += new System.EventHandler(this.btnEliminardeOrden_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Name = "label6";
            this.label6.TabIndex = 18;
            this.label6.Text = "BUSCAR EN ORDEN";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Name = "label3";
            this.label3.TabIndex = 15;
            this.label3.Text = "SELECCIONA ORDEN DE COMPRA";
            // 
            // txtBuscarEntrada
            // 
            this.txtBuscarEntrada.Name = "txtBuscarEntrada";
            this.txtBuscarEntrada.TabIndex = 9;
            this.txtBuscarEntrada.TextChanged += new System.EventHandler(this.TxtBuscarEntrada_TextChanged);
            // 
            // lblCasa
            // 
            this.lblCasa.AutoSize = true;
            this.lblCasa.Name = "lblCasa";
            this.lblCasa.TabIndex = 8;
            // 
            // lblLoteDisplay
            // 
            this.lblLoteDisplay.AutoSize = true;
            this.lblLoteDisplay.Name = "lblLoteDisplay";
            this.lblLoteDisplay.TabIndex = 7;
            // 
            // lblManzanaDisplay
            // 
            this.lblManzanaDisplay.AutoSize = true;
            this.lblManzanaDisplay.Name = "lblManzanaDisplay";
            this.lblManzanaDisplay.TabIndex = 6;
            // 
            // btnCapturarEntrada
            // 
            this.btnCapturarEntrada.Name = "btnCapturarEntrada";
            this.btnCapturarEntrada.TabIndex = 5;
            this.btnCapturarEntrada.Text = "CAPTURAR ENTRADA";
            this.btnCapturarEntrada.UseVisualStyleBackColor = true;
            this.btnCapturarEntrada.Click += new System.EventHandler(this.BtnCapturarEntrada_Click);
            // 
            // dgvEntrada
            // 
            this.dgvEntrada.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEntrada.Name = "dgvEntrada";
            this.dgvEntrada.TabIndex = 4;
            this.dgvEntrada.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvEntrada_CurrentCellDirtyStateChanged);
            // 
            // cmbOrdenesCompra
            // 
            this.cmbOrdenesCompra.FormattingEnabled = true;
            this.cmbOrdenesCompra.Name = "cmbOrdenesCompra";
            this.cmbOrdenesCompra.TabIndex = 3;
            this.cmbOrdenesCompra.SelectedIndexChanged += new System.EventHandler(this.cmbOrdenesCompra_SelectedIndexChanged);
            // 
            // btnBuscarOrden
            // 
            this.btnBuscarOrden.Name = "btnBuscarOrden";
            this.btnBuscarOrden.TabIndex = 2;
            this.btnBuscarOrden.Text = "BUSCAR ORDEN";
            this.btnBuscarOrden.UseVisualStyleBackColor = true;
            this.btnBuscarOrden.Click += new System.EventHandler(this.BtnBuscarOrden_Click);
            // 
            // tabPageInventario ?? NUEVA PESTA?A COMPLETA
            // 
            this.tabPageInventario.Controls.Add(this.btnActualizarInventario);
            this.tabPageInventario.Controls.Add(this.btnExportarInventario);
            this.tabPageInventario.Controls.Add(this.lblTotalInventario);
            this.tabPageInventario.Controls.Add(this.txtBuscarInventario);
            this.tabPageInventario.Controls.Add(this.lblBuscarInventario);
            this.tabPageInventario.Controls.Add(this.dgvInventario);
            this.tabPageInventario.Location = new System.Drawing.Point(4, 22);
            this.tabPageInventario.Name = "tabPageInventario";
            this.tabPageInventario.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInventario.Size = new System.Drawing.Size(1272, 694);
            this.tabPageInventario.TabIndex = 3;
            this.tabPageInventario.Text = "INVENTARIO";
            this.tabPageInventario.UseVisualStyleBackColor = true;
            // 
            // dgvInventario
            // 
            this.dgvInventario.AllowUserToAddRows = false;
            this.dgvInventario.AllowUserToDeleteRows = false;
            this.dgvInventario.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvInventario.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInventario.Location = new System.Drawing.Point(15, 85);
            this.dgvInventario.Name = "dgvInventario";
            this.dgvInventario.ReadOnly = true;
            this.dgvInventario.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvInventario.Size = new System.Drawing.Size(1242, 530);
            this.dgvInventario.TabIndex = 0;
            // 
            // txtBuscarInventario
            // 
            this.txtBuscarInventario.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBuscarInventario.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBuscarInventario.Location = new System.Drawing.Point(120, 15);
            this.txtBuscarInventario.Name = "txtBuscarInventario";
            this.txtBuscarInventario.Size = new System.Drawing.Size(1137, 25);
            this.txtBuscarInventario.TabIndex = 1;
            this.txtBuscarInventario.TextChanged += new System.EventHandler(this.TxtBuscarInventario_TextChanged);
            // 
            // lblBuscarInventario
            // 
            this.lblBuscarInventario.AutoSize = true;
            this.lblBuscarInventario.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBuscarInventario.Location = new System.Drawing.Point(15, 18);
            this.lblBuscarInventario.Name = "lblBuscarInventario";
            this.lblBuscarInventario.Size = new System.Drawing.Size(99, 19);
            this.lblBuscarInventario.TabIndex = 2;
            this.lblBuscarInventario.Text = "?? BUSCAR:";
            // 
            // lblTotalInventario
            // 
            this.lblTotalInventario.AutoSize = true;
            this.lblTotalInventario.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalInventario.Location = new System.Drawing.Point(15, 55);
            this.lblTotalInventario.Name = "lblTotalInventario";
            this.lblTotalInventario.Size = new System.Drawing.Size(250, 19);
            this.lblTotalInventario.TabIndex = 3;
            this.lblTotalInventario.Text = "?? Total de entradas: 0";
            // 
            // btnExportarInventario
            // 
            this.btnExportarInventario.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportarInventario.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportarInventario.Location = new System.Drawing.Point(1042, 635);
            this.btnExportarInventario.Name = "btnExportarInventario";
            this.btnExportarInventario.Size = new System.Drawing.Size(215, 40);
            this.btnExportarInventario.TabIndex = 4;
            this.btnExportarInventario.Text = "?? EXPORTAR A EXCEL";
            this.btnExportarInventario.UseVisualStyleBackColor = true;
            this.btnExportarInventario.Click += new System.EventHandler(this.BtnExportarInventario_Click);
            // 
            // btnActualizarInventario
            // 
            this.btnActualizarInventario.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnActualizarInventario.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnActualizarInventario.Location = new System.Drawing.Point(15, 635);
            this.btnActualizarInventario.Name = "btnActualizarInventario";
            this.btnActualizarInventario.Size = new System.Drawing.Size(200, 40);
            this.btnActualizarInventario.TabIndex = 5;
            this.btnActualizarInventario.Text = "? ACTUALIZAR";
            this.btnActualizarInventario.UseVisualStyleBackColor = true;
            this.btnActualizarInventario.Click += new System.EventHandler(this.BtnActualizarInventario_Click);
            // 
            // tabPageConsultaCasa
            // 
            this.tabPageConsultaCasa.Controls.Add(this.btnActualizarConsultaCasa);
            this.tabPageConsultaCasa.Controls.Add(this.btnExportarConsultaCasa);
            this.tabPageConsultaCasa.Controls.Add(this.lblDiscrepanciasConsulta);
            this.tabPageConsultaCasa.Controls.Add(this.lblTotalImporteConsulta);
            this.tabPageConsultaCasa.Controls.Add(this.lblTotalInsumosConsulta);
            this.tabPageConsultaCasa.Controls.Add(this.lblPrototipoConsulta);
            this.tabPageConsultaCasa.Controls.Add(this.lblCasaSeleccionada);
            this.tabPageConsultaCasa.Controls.Add(this.dgvConsultaCasa);
            this.tabPageConsultaCasa.Controls.Add(this.txtBuscarConsultaCasa);
            this.tabPageConsultaCasa.Controls.Add(this.lblBuscarConsultaCasa);
            this.tabPageConsultaCasa.Controls.Add(this.cmbLoteConsulta);
            this.tabPageConsultaCasa.Controls.Add(this.lblLoteConsulta);
            this.tabPageConsultaCasa.Controls.Add(this.cmbManzanaConsulta);
            this.tabPageConsultaCasa.Controls.Add(this.lblManzanaConsulta);
            this.tabPageConsultaCasa.Location = new System.Drawing.Point(4, 22);
            this.tabPageConsultaCasa.Name = "tabPageConsultaCasa";
            this.tabPageConsultaCasa.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConsultaCasa.Size = new System.Drawing.Size(1272, 694);
            this.tabPageConsultaCasa.TabIndex = 4;
            this.tabPageConsultaCasa.Text = "CONSULTA POR CASA";
            this.tabPageConsultaCasa.UseVisualStyleBackColor = true;
            // 
            // lblManzanaConsulta
            // 
            this.lblManzanaConsulta.AutoSize = true;
            this.lblManzanaConsulta.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblManzanaConsulta.Location = new System.Drawing.Point(15, 18);
            this.lblManzanaConsulta.Name = "lblManzanaConsulta";
            this.lblManzanaConsulta.Size = new System.Drawing.Size(80, 19);
            this.lblManzanaConsulta.TabIndex = 0;
            this.lblManzanaConsulta.Text = "MANZANA:";
            // 
            // cmbManzanaConsulta
            // 
            this.cmbManzanaConsulta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManzanaConsulta.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbManzanaConsulta.FormattingEnabled = true;
            this.cmbManzanaConsulta.Location = new System.Drawing.Point(110, 15);
            this.cmbManzanaConsulta.Name = "cmbManzanaConsulta";
            this.cmbManzanaConsulta.Size = new System.Drawing.Size(150, 25);
            this.cmbManzanaConsulta.TabIndex = 1;
            this.cmbManzanaConsulta.SelectedIndexChanged += new System.EventHandler(this.CmbManzanaConsulta_SelectedIndexChanged);
            // 
            // lblLoteConsulta
            // 
            this.lblLoteConsulta.AutoSize = true;
            this.lblLoteConsulta.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblLoteConsulta.Location = new System.Drawing.Point(280, 18);
            this.lblLoteConsulta.Name = "lblLoteConsulta";
            this.lblLoteConsulta.Size = new System.Drawing.Size(48, 19);
            this.lblLoteConsulta.TabIndex = 2;
            this.lblLoteConsulta.Text = "LOTE:";
            // 
            // cmbLoteConsulta
            // 
            this.cmbLoteConsulta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLoteConsulta.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbLoteConsulta.FormattingEnabled = true;
            this.cmbLoteConsulta.Location = new System.Drawing.Point(335, 15);
            this.cmbLoteConsulta.Name = "cmbLoteConsulta";
            this.cmbLoteConsulta.Size = new System.Drawing.Size(150, 25);
            this.cmbLoteConsulta.TabIndex = 3;
            this.cmbLoteConsulta.SelectedIndexChanged += new System.EventHandler(this.CmbLoteConsulta_SelectedIndexChanged);
            // 
            // lblBuscarConsultaCasa
            // 
            this.lblBuscarConsultaCasa.AutoSize = true;
            this.lblBuscarConsultaCasa.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBuscarConsultaCasa.Location = new System.Drawing.Point(510, 18);
            this.lblBuscarConsultaCasa.Name = "lblBuscarConsultaCasa";
            this.lblBuscarConsultaCasa.Size = new System.Drawing.Size(73, 19);
            this.lblBuscarConsultaCasa.TabIndex = 4;
            this.lblBuscarConsultaCasa.Text = "?? BUSCAR:";
            // 
            // txtBuscarConsultaCasa
            // 
            this.txtBuscarConsultaCasa.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBuscarConsultaCasa.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBuscarConsultaCasa.Location = new System.Drawing.Point(590, 15);
            this.txtBuscarConsultaCasa.Name = "txtBuscarConsultaCasa";
            this.txtBuscarConsultaCasa.Size = new System.Drawing.Size(667, 25);
            this.txtBuscarConsultaCasa.TabIndex = 5;
            this.txtBuscarConsultaCasa.TextChanged += new System.EventHandler(this.TxtBuscarConsultaCasa_TextChanged);
            // 
            // lblCasaSeleccionada
            // 
            this.lblCasaSeleccionada.AutoSize = true;
            this.lblCasaSeleccionada.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            this.lblCasaSeleccionada.ForeColor = System.Drawing.Color.Gray;
            this.lblCasaSeleccionada.Location = new System.Drawing.Point(15, 55);
            this.lblCasaSeleccionada.Name = "lblCasaSeleccionada";
            this.lblCasaSeleccionada.Size = new System.Drawing.Size(250, 19);
            this.lblCasaSeleccionada.TabIndex = 6;
            this.lblCasaSeleccionada.Text = "Seleccione una casa para ver sus insumos";
            // 
            // lblPrototipoConsulta
            // 
            this.lblPrototipoConsulta.AutoSize = true;
            this.lblPrototipoConsulta.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPrototipoConsulta.Location = new System.Drawing.Point(15, 80);
            this.lblPrototipoConsulta.Name = "lblPrototipoConsulta";
            this.lblPrototipoConsulta.Size = new System.Drawing.Size(0, 19);
            this.lblPrototipoConsulta.TabIndex = 7;
            // 
            // lblTotalInsumosConsulta
            // 
            this.lblTotalInsumosConsulta.AutoSize = true;
            this.lblTotalInsumosConsulta.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalInsumosConsulta.Location = new System.Drawing.Point(400, 55);
            this.lblTotalInsumosConsulta.Name = "lblTotalInsumosConsulta";
            this.lblTotalInsumosConsulta.Size = new System.Drawing.Size(0, 15);
            this.lblTotalInsumosConsulta.TabIndex = 8;
            // 
            // lblTotalImporteConsulta
            // 
            this.lblTotalImporteConsulta.AutoSize = true;
            this.lblTotalImporteConsulta.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalImporteConsulta.Location = new System.Drawing.Point(400, 80);
            this.lblTotalImporteConsulta.Name = "lblTotalImporteConsulta";
            this.lblTotalImporteConsulta.Size = new System.Drawing.Size(0, 15);
            this.lblTotalImporteConsulta.TabIndex = 9;
            // 
            // lblDiscrepanciasConsulta
            // 
            this.lblDiscrepanciasConsulta.AutoSize = true;
            this.lblDiscrepanciasConsulta.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDiscrepanciasConsulta.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblDiscrepanciasConsulta.Location = new System.Drawing.Point(650, 55);
            this.lblDiscrepanciasConsulta.Name = "lblDiscrepanciasConsulta";
            this.lblDiscrepanciasConsulta.Size = new System.Drawing.Size(0, 15);
            this.lblDiscrepanciasConsulta.TabIndex = 10;
            this.lblDiscrepanciasConsulta.Visible = false;
            // 
            // dgvConsultaCasa
            // 
            this.dgvConsultaCasa.AllowUserToAddRows = false;
            this.dgvConsultaCasa.AllowUserToDeleteRows = false;
            this.dgvConsultaCasa.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvConsultaCasa.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvConsultaCasa.Location = new System.Drawing.Point(15, 110);
            this.dgvConsultaCasa.Name = "dgvConsultaCasa";
            this.dgvConsultaCasa.ReadOnly = true;
            this.dgvConsultaCasa.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvConsultaCasa.Size = new System.Drawing.Size(1242, 505);
            this.dgvConsultaCasa.TabIndex = 11;
            // 
            // btnExportarConsultaCasa
            // 
            this.btnExportarConsultaCasa.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportarConsultaCasa.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportarConsultaCasa.Location = new System.Drawing.Point(1042, 635);
            this.btnExportarConsultaCasa.Name = "btnExportarConsultaCasa";
            this.btnExportarConsultaCasa.Size = new System.Drawing.Size(215, 40);
            this.btnExportarConsultaCasa.TabIndex = 12;
            this.btnExportarConsultaCasa.Text = "?? EXPORTAR A EXCEL";
            this.btnExportarConsultaCasa.UseVisualStyleBackColor = true;
            this.btnExportarConsultaCasa.Click += new System.EventHandler(this.BtnExportarConsultaCasa_Click);
            // 
            // btnActualizarConsultaCasa
            // 
            this.btnActualizarConsultaCasa.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnActualizarConsultaCasa.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnActualizarConsultaCasa.Location = new System.Drawing.Point(15, 635);
            this.btnActualizarConsultaCasa.Name = "btnActualizarConsultaCasa";
            this.btnActualizarConsultaCasa.Size = new System.Drawing.Size(200, 40);
            this.btnActualizarConsultaCasa.TabIndex = 13;
            this.btnActualizarConsultaCasa.Text = "?? ACTUALIZAR";
            this.btnActualizarConsultaCasa.UseVisualStyleBackColor = true;
            this.btnActualizarConsultaCasa.Click += new System.EventHandler(this.BtnActualizarConsultaCasa_Click);
            // 
            // FormAlmacen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.TabsControl);
            this.MinimumSize = new System.Drawing.Size(1024, 600);
            this.Name = "FormAlmacen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gestión de Almacén - Pilaris";
            this.tabPageHistorial.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.objectListViewHistorial)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInsumos)).EndInit();
            this.TabsControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEntrada)).EndInit();
            this.tabPageInventario.ResumeLayout(false);
            this.tabPageInventario.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInventario)).EndInit();
            this.tabPageConsultaCasa.ResumeLayout(false); // ? NUEVO
            this.tabPageConsultaCasa.PerformLayout(); // ? NUEVO
            ((System.ComponentModel.ISupportInitialize)(this.dgvConsultaCasa)).EndInit(); // ? NUEVO
            this.ResumeLayout(false);
        }
    }
}
