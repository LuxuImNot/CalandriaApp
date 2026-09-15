namespace DynamicSepticSystem
{
    partial class FormEstimacionConceptoMigrado
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.panelFiltros = new System.Windows.Forms.Panel();
            this.lblEvidenciasSeleccionadas = new System.Windows.Forms.Label();
            this.btnSeleccionarEvidencias = new System.Windows.Forms.Button();
            this.txtPuestoFirma3 = new System.Windows.Forms.TextBox();
            this.txtPuestoFirma2 = new System.Windows.Forms.TextBox();
            this.txtPuestoFirma1 = new System.Windows.Forms.TextBox();
            this.txtFirma3 = new System.Windows.Forms.TextBox();
            this.lblFirma3 = new System.Windows.Forms.Label();
            this.txtFirma2 = new System.Windows.Forms.TextBox();
            this.lblFirma2 = new System.Windows.Forms.Label();
            this.txtFirma1 = new System.Windows.Forms.TextBox();
            this.lblFirma1 = new System.Windows.Forms.Label();
            this.btnRepositorioPDFs = new System.Windows.Forms.Button();
            this.btnCargarAvance = new System.Windows.Forms.Button();
            this.cmbLote = new System.Windows.Forms.ComboBox();
            this.lblLote = new System.Windows.Forms.Label();
            this.cmbManzana = new System.Windows.Forms.ComboBox();
            this.lblManzana = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtProveedor = new System.Windows.Forms.TextBox();
            this.lblProveedor = new System.Windows.Forms.Label();
            this.panelTotales = new System.Windows.Forms.Panel();
            this.progressBarAvance = new System.Windows.Forms.ProgressBar();
            this.lblEstadisticas = new System.Windows.Forms.Label();
            this.lblAvanceGeneral = new System.Windows.Forms.Label();
            this.lblPorEjecutar = new System.Windows.Forms.Label();
            this.lblTotalEjecutado = new System.Windows.Forms.Label();
            this.lblTotalPresupuestado = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelIzquierdo = new System.Windows.Forms.Panel();
            this.olvEstimacionConceptos = new DynamicSepticSystem.SafeTreeListView();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.btnGuardarYExportar = new System.Windows.Forms.Button();
            this.btnDesmarcarTodos = new System.Windows.Forms.Button();
            this.btnMarcarTodos = new System.Windows.Forms.Button();
            this.btnAgregarConcepto = new System.Windows.Forms.Button();
            this.btnGestionarPartidas = new System.Windows.Forms.Button();
            this.panelDerecho = new System.Windows.Forms.Panel();
            this.pictureBoxPreviewPDF = new System.Windows.Forms.PictureBox();
            this.lblPreview = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelFiltros.SuspendLayout();
            this.panelTotales.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelIzquierdo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvEstimacionConceptos)).BeginInit();
            this.panelBotones.SuspendLayout();
            this.panelDerecho.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewPDF)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panelTop.Controls.Add(this.lblTitulo);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1400, 60);
            this.panelTop.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(1400, 60);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "ESTIMACIÓN POR CONCEPTOS";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelFiltros
            // 
            this.panelFiltros.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelFiltros.Controls.Add(this.lblEvidenciasSeleccionadas);
            this.panelFiltros.Controls.Add(this.btnSeleccionarEvidencias);
            this.panelFiltros.Controls.Add(this.txtPuestoFirma3);
            this.panelFiltros.Controls.Add(this.txtPuestoFirma2);
            this.panelFiltros.Controls.Add(this.txtPuestoFirma1);
            this.panelFiltros.Controls.Add(this.txtFirma3);
            this.panelFiltros.Controls.Add(this.lblFirma3);
            this.panelFiltros.Controls.Add(this.txtFirma2);
            this.panelFiltros.Controls.Add(this.lblFirma2);
            this.panelFiltros.Controls.Add(this.txtFirma1);
            this.panelFiltros.Controls.Add(this.lblFirma1);
            this.panelFiltros.Controls.Add(this.btnRepositorioPDFs);
            this.panelFiltros.Controls.Add(this.btnCargarAvance);
            this.panelFiltros.Controls.Add(this.cmbLote);
            this.panelFiltros.Controls.Add(this.lblLote);
            this.panelFiltros.Controls.Add(this.cmbManzana);
            this.panelFiltros.Controls.Add(this.lblManzana);
            this.panelFiltros.Controls.Add(this.txtDescripcion);
            this.panelFiltros.Controls.Add(this.lblDescripcion);
            this.panelFiltros.Controls.Add(this.txtProveedor);
            this.panelFiltros.Controls.Add(this.lblProveedor);
            this.panelFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltros.Location = new System.Drawing.Point(0, 60);
            this.panelFiltros.Name = "panelFiltros";
            this.panelFiltros.Size = new System.Drawing.Size(1400, 140);
            this.panelFiltros.TabIndex = 1;
            // 
            // lblEvidenciasSeleccionadas
            // 
            this.lblEvidenciasSeleccionadas.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblEvidenciasSeleccionadas.ForeColor = System.Drawing.Color.Gray;
            this.lblEvidenciasSeleccionadas.Location = new System.Drawing.Point(1205, 117);
            this.lblEvidenciasSeleccionadas.Name = "lblEvidenciasSeleccionadas";
            this.lblEvidenciasSeleccionadas.Size = new System.Drawing.Size(180, 20);
            this.lblEvidenciasSeleccionadas.TabIndex = 21;
            this.lblEvidenciasSeleccionadas.Text = "📷 Sin evidencias";
            this.lblEvidenciasSeleccionadas.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSeleccionarEvidencias
            // 
            this.btnSeleccionarEvidencias.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnSeleccionarEvidencias.FlatAppearance.BorderSize = 0;
            this.btnSeleccionarEvidencias.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSeleccionarEvidencias.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSeleccionarEvidencias.ForeColor = System.Drawing.Color.White;
            this.btnSeleccionarEvidencias.Location = new System.Drawing.Point(1208, 79);
            this.btnSeleccionarEvidencias.Name = "btnSeleccionarEvidencias";
            this.btnSeleccionarEvidencias.Size = new System.Drawing.Size(180, 35);
            this.btnSeleccionarEvidencias.TabIndex = 20;
            this.btnSeleccionarEvidencias.Text = "📷 Seleccionar Fotos";
            this.btnSeleccionarEvidencias.UseVisualStyleBackColor = false;
            this.btnSeleccionarEvidencias.Click += new System.EventHandler(this.btnSeleccionarEvidencias_Click);
            // 
            // txtPuestoFirma3
            // 
            this.txtPuestoFirma3.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtPuestoFirma3.Location = new System.Drawing.Point(1000, 102);
            this.txtPuestoFirma3.Name = "txtPuestoFirma3";
            this.txtPuestoFirma3.Size = new System.Drawing.Size(120, 22);
            this.txtPuestoFirma3.TabIndex = 19;
            this.txtPuestoFirma3.Text = "Subcontratista";
            this.txtPuestoFirma3.TextChanged += new System.EventHandler(this.OnDatosChanged);
            // 
            // txtPuestoFirma2
            // 
            this.txtPuestoFirma2.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtPuestoFirma2.Location = new System.Drawing.Point(605, 102);
            this.txtPuestoFirma2.Name = "txtPuestoFirma2";
            this.txtPuestoFirma2.Size = new System.Drawing.Size(120, 22);
            this.txtPuestoFirma2.TabIndex = 16;
            this.txtPuestoFirma2.Text = "Gerente de Obra";
            this.txtPuestoFirma2.TextChanged += new System.EventHandler(this.OnDatosChanged);
            // 
            // txtPuestoFirma1
            // 
            this.txtPuestoFirma1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtPuestoFirma1.Location = new System.Drawing.Point(245, 102);
            this.txtPuestoFirma1.Name = "txtPuestoFirma1";
            this.txtPuestoFirma1.Size = new System.Drawing.Size(120, 22);
            this.txtPuestoFirma1.TabIndex = 13;
            this.txtPuestoFirma1.Text = "Gerente De Proyecto";
            this.txtPuestoFirma1.TextChanged += new System.EventHandler(this.OnDatosChanged);
            // 
            // txtFirma3
            // 
            this.txtFirma3.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtFirma3.Location = new System.Drawing.Point(845, 102);
            this.txtFirma3.Name = "txtFirma3";
            this.txtFirma3.Size = new System.Drawing.Size(150, 22);
            this.txtFirma3.TabIndex = 18;
            this.txtFirma3.Text = "Ignacio Durán León";
            this.txtFirma3.TextChanged += new System.EventHandler(this.OnDatosChanged);
            // 
            // lblFirma3
            // 
            this.lblFirma3.AutoSize = true;
            this.lblFirma3.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblFirma3.Location = new System.Drawing.Point(750, 105);
            this.lblFirma3.Name = "lblFirma3";
            this.lblFirma3.Size = new System.Drawing.Size(85, 13);
            this.lblFirma3.TabIndex = 17;
            this.lblFirma3.Text = "Subcontratista:";
            // 
            // txtFirma2
            // 
            this.txtFirma2.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtFirma2.Location = new System.Drawing.Point(450, 102);
            this.txtFirma2.Name = "txtFirma2";
            this.txtFirma2.Size = new System.Drawing.Size(150, 22);
            this.txtFirma2.TabIndex = 15;
            this.txtFirma2.Text = "Ing. Carlos Tabardillo Herrera";
            this.txtFirma2.TextChanged += new System.EventHandler(this.OnDatosChanged);
            // 
            // lblFirma2
            // 
            this.lblFirma2.AutoSize = true;
            this.lblFirma2.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblFirma2.Location = new System.Drawing.Point(390, 105);
            this.lblFirma2.Name = "lblFirma2";
            this.lblFirma2.Size = new System.Drawing.Size(48, 13);
            this.lblFirma2.TabIndex = 14;
            this.lblFirma2.Text = "Firma 2:";
            // 
            // txtFirma1
            // 
            this.txtFirma1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtFirma1.Location = new System.Drawing.Point(90, 102);
            this.txtFirma1.Name = "txtFirma1";
            this.txtFirma1.Size = new System.Drawing.Size(150, 22);
            this.txtFirma1.TabIndex = 12;
            this.txtFirma1.Text = "ING. J. Rafael Monroy Díaz";
            this.txtFirma1.TextChanged += new System.EventHandler(this.OnDatosChanged);
            // 
            // lblFirma1
            // 
            this.lblFirma1.AutoSize = true;
            this.lblFirma1.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblFirma1.Location = new System.Drawing.Point(30, 105);
            this.lblFirma1.Name = "lblFirma1";
            this.lblFirma1.Size = new System.Drawing.Size(48, 13);
            this.lblFirma1.TabIndex = 11;
            this.lblFirma1.Text = "Firma 1:";
            // 
            // btnRepositorioPDFs
            // 
            this.btnRepositorioPDFs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnRepositorioPDFs.FlatAppearance.BorderSize = 0;
            this.btnRepositorioPDFs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRepositorioPDFs.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRepositorioPDFs.ForeColor = System.Drawing.Color.White;
            this.btnRepositorioPDFs.Location = new System.Drawing.Point(666, 18);
            this.btnRepositorioPDFs.Name = "btnRepositorioPDFs";
            this.btnRepositorioPDFs.Size = new System.Drawing.Size(180, 35);
            this.btnRepositorioPDFs.TabIndex = 5;
            this.btnRepositorioPDFs.Text = "📁 Ver Estimaciones";
            this.btnRepositorioPDFs.UseVisualStyleBackColor = false;
            this.btnRepositorioPDFs.Click += new System.EventHandler(this.btnRepositorioPDFs_Click);
            // 
            // btnCargarAvance
            // 
            this.btnCargarAvance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnCargarAvance.FlatAppearance.BorderSize = 0;
            this.btnCargarAvance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarAvance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCargarAvance.ForeColor = System.Drawing.Color.White;
            this.btnCargarAvance.Location = new System.Drawing.Point(510, 18);
            this.btnCargarAvance.Name = "btnCargarAvance";
            this.btnCargarAvance.Size = new System.Drawing.Size(150, 35);
            this.btnCargarAvance.TabIndex = 4;
            this.btnCargarAvance.Text = "?? Cargar Avance";
            this.btnCargarAvance.UseVisualStyleBackColor = false;
            this.btnCargarAvance.Click += new System.EventHandler(this.btnCargarAvance_Click);
            // 
            // cmbLote
            // 
            this.cmbLote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLote.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbLote.FormattingEnabled = true;
            this.cmbLote.Location = new System.Drawing.Point(340, 22);
            this.cmbLote.Name = "cmbLote";
            this.cmbLote.Size = new System.Drawing.Size(150, 25);
            this.cmbLote.TabIndex = 3;
            // 
            // lblLote
            // 
            this.lblLote.AutoSize = true;
            this.lblLote.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblLote.Location = new System.Drawing.Point(290, 25);
            this.lblLote.Name = "lblLote";
            this.lblLote.Size = new System.Drawing.Size(42, 19);
            this.lblLote.TabIndex = 2;
            this.lblLote.Text = "Lote:";
            // 
            // cmbManzana
            // 
            this.cmbManzana.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManzana.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbManzana.FormattingEnabled = true;
            this.cmbManzana.Location = new System.Drawing.Point(120, 22);
            this.cmbManzana.Name = "cmbManzana";
            this.cmbManzana.Size = new System.Drawing.Size(150, 25);
            this.cmbManzana.TabIndex = 1;
            this.cmbManzana.SelectedIndexChanged += new System.EventHandler(this.cmbManzana_SelectedIndexChanged);
            // 
            // lblManzana
            // 
            this.lblManzana.AutoSize = true;
            this.lblManzana.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblManzana.Location = new System.Drawing.Point(30, 25);
            this.lblManzana.Name = "lblManzana";
            this.lblManzana.Size = new System.Drawing.Size(73, 19);
            this.lblManzana.TabIndex = 0;
            this.lblManzana.Text = "Manzana:";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDescripcion.Location = new System.Drawing.Point(480, 65);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(300, 23);
            this.txtDescripcion.TabIndex = 8;
            this.txtDescripcion.Text = "Subcontratista Edificación";
            this.txtDescripcion.TextChanged += new System.EventHandler(this.OnDatosChanged);
            // 
            // lblDescripcion
            // 
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDescripcion.Location = new System.Drawing.Point(390, 68);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(75, 15);
            this.lblDescripcion.TabIndex = 7;
            this.lblDescripcion.Text = "Descripción:";
            // 
            // txtProveedor
            // 
            this.txtProveedor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProveedor.Location = new System.Drawing.Point(120, 65);
            this.txtProveedor.Name = "txtProveedor";
            this.txtProveedor.Size = new System.Drawing.Size(250, 23);
            this.txtProveedor.TabIndex = 6;
            this.txtProveedor.Text = "Ignacio Durán León";
            this.txtProveedor.TextChanged += new System.EventHandler(this.OnDatosChanged);
            // 
            // lblProveedor
            // 
            this.lblProveedor.AutoSize = true;
            this.lblProveedor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProveedor.Location = new System.Drawing.Point(30, 68);
            this.lblProveedor.Name = "lblProveedor";
            this.lblProveedor.Size = new System.Drawing.Size(69, 15);
            this.lblProveedor.TabIndex = 5;
            this.lblProveedor.Text = "Proveedor:";
            // 
            // panelTotales
            // 
            this.panelTotales.BackColor = System.Drawing.Color.White;
            this.panelTotales.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTotales.Controls.Add(this.progressBarAvance);
            this.panelTotales.Controls.Add(this.lblEstadisticas);
            this.panelTotales.Controls.Add(this.lblAvanceGeneral);
            this.panelTotales.Controls.Add(this.lblPorEjecutar);
            this.panelTotales.Controls.Add(this.lblTotalEjecutado);
            this.panelTotales.Controls.Add(this.lblTotalPresupuestado);
            this.panelTotales.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTotales.Location = new System.Drawing.Point(0, 200);
            this.panelTotales.Name = "panelTotales";
            this.panelTotales.Size = new System.Drawing.Size(1400, 100);
            this.panelTotales.TabIndex = 2;
            // 
            // progressBarAvance
            // 
            this.progressBarAvance.Location = new System.Drawing.Point(30, 70);
            this.progressBarAvance.Name = "progressBarAvance";
            this.progressBarAvance.Size = new System.Drawing.Size(350, 20);
            this.progressBarAvance.TabIndex = 4;
            // 
            // lblEstadisticas
            // 
            this.lblEstadisticas.AutoSize = true;
            this.lblEstadisticas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadisticas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblEstadisticas.Location = new System.Drawing.Point(400, 70);
            this.lblEstadisticas.Name = "lblEstadisticas";
            this.lblEstadisticas.Size = new System.Drawing.Size(246, 15);
            this.lblEstadisticas.TabIndex = 3;
            this.lblEstadisticas.Text = "Completados: 0 | En Progreso: 0 | Sin Iniciar: 0";
            // 
            // lblAvanceGeneral
            // 
            this.lblAvanceGeneral.AutoSize = true;
            this.lblAvanceGeneral.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblAvanceGeneral.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblAvanceGeneral.Location = new System.Drawing.Point(30, 40);
            this.lblAvanceGeneral.Name = "lblAvanceGeneral";
            this.lblAvanceGeneral.Size = new System.Drawing.Size(160, 21);
            this.lblAvanceGeneral.TabIndex = 2;
            this.lblAvanceGeneral.Text = "Avance General: 0%";
            // 
            // lblPorEjecutar
            // 
            this.lblPorEjecutar.AutoSize = true;
            this.lblPorEjecutar.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblPorEjecutar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.lblPorEjecutar.Location = new System.Drawing.Point(750, 10);
            this.lblPorEjecutar.Name = "lblPorEjecutar";
            this.lblPorEjecutar.Size = new System.Drawing.Size(129, 20);
            this.lblPorEjecutar.TabIndex = 5;
            this.lblPorEjecutar.Text = "Por Ejecutar: $0.00";
            // 
            // lblTotalEjecutado
            // 
            this.lblTotalEjecutado.AutoSize = true;
            this.lblTotalEjecutado.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblTotalEjecutado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblTotalEjecutado.Location = new System.Drawing.Point(400, 10);
            this.lblTotalEjecutado.Name = "lblTotalEjecutado";
            this.lblTotalEjecutado.Size = new System.Drawing.Size(154, 20);
            this.lblTotalEjecutado.TabIndex = 1;
            this.lblTotalEjecutado.Text = "Total Ejecutado: $0.00";
            // 
            // lblTotalPresupuestado
            // 
            this.lblTotalPresupuestado.AutoSize = true;
            this.lblTotalPresupuestado.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblTotalPresupuestado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTotalPresupuestado.Location = new System.Drawing.Point(30, 10);
            this.lblTotalPresupuestado.Name = "lblTotalPresupuestado";
            this.lblTotalPresupuestado.Size = new System.Drawing.Size(185, 20);
            this.lblTotalPresupuestado.TabIndex = 0;
            this.lblTotalPresupuestado.Text = "Total Presupuestado: $0.00";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 300);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelIzquierdo);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelDerecho);
            this.splitContainer1.Size = new System.Drawing.Size(1400, 500);
            this.splitContainer1.SplitterDistance = 700;
            this.splitContainer1.TabIndex = 3;
            // 
            // panelIzquierdo
            // 
            this.panelIzquierdo.BackColor = System.Drawing.Color.White;
            this.panelIzquierdo.Controls.Add(this.olvEstimacionConceptos);
            this.panelIzquierdo.Controls.Add(this.panelBotones);
            this.panelIzquierdo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelIzquierdo.Location = new System.Drawing.Point(0, 0);
            this.panelIzquierdo.Name = "panelIzquierdo";
            this.panelIzquierdo.Size = new System.Drawing.Size(700, 500);
            this.panelIzquierdo.TabIndex = 0;
            // 
            // olvEstimacionConceptos
            // 
            this.olvEstimacionConceptos.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;
            this.olvEstimacionConceptos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvEstimacionConceptos.FullRowSelect = true;
            this.olvEstimacionConceptos.HideSelection = false;
            this.olvEstimacionConceptos.Location = new System.Drawing.Point(0, 0);
            this.olvEstimacionConceptos.Name = "olvEstimacionConceptos";
            this.olvEstimacionConceptos.OwnerDraw = true;
            this.olvEstimacionConceptos.ShowGroups = false;
            this.olvEstimacionConceptos.Size = new System.Drawing.Size(700, 450);
            this.olvEstimacionConceptos.TabIndex = 0;
            this.olvEstimacionConceptos.UseAlternatingBackColors = true;
            this.olvEstimacionConceptos.UseCompatibleStateImageBehavior = false;
            this.olvEstimacionConceptos.View = System.Windows.Forms.View.Details;
            this.olvEstimacionConceptos.VirtualMode = true;
            // 
            // panelBotones
            // 
            this.panelBotones.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.panelBotones.Controls.Add(this.btnGestionarPartidas);
            this.panelBotones.Controls.Add(this.btnAgregarConcepto);
            this.panelBotones.Controls.Add(this.btnGuardarYExportar);
            this.panelBotones.Controls.Add(this.btnDesmarcarTodos);
            this.panelBotones.Controls.Add(this.btnMarcarTodos);
            this.panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotones.Location = new System.Drawing.Point(0, 450);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Size = new System.Drawing.Size(700, 50);
            this.panelBotones.TabIndex = 1;
            // 
            // btnGuardarYExportar
            // 
            this.btnGuardarYExportar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnGuardarYExportar.FlatAppearance.BorderSize = 0;
            this.btnGuardarYExportar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarYExportar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnGuardarYExportar.ForeColor = System.Drawing.Color.White;
            this.btnGuardarYExportar.Location = new System.Drawing.Point(395, 0);
            this.btnGuardarYExportar.Name = "btnGuardarYExportar";
            this.btnGuardarYExportar.Size = new System.Drawing.Size(280, 50);
            this.btnGuardarYExportar.TabIndex = 3;
            this.btnGuardarYExportar.Text = "?? Guardar y Exportar PDF";
            this.btnGuardarYExportar.UseVisualStyleBackColor = false;
            this.btnGuardarYExportar.Click += new System.EventHandler(this.btnGuardarYExportar_Click);
            // 
            // btnDesmarcarTodos
            // 
            this.btnDesmarcarTodos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnDesmarcarTodos.FlatAppearance.BorderSize = 0;
            this.btnDesmarcarTodos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesmarcarTodos.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDesmarcarTodos.ForeColor = System.Drawing.Color.White;
            this.btnDesmarcarTodos.Location = new System.Drawing.Point(210, 0);
            this.btnDesmarcarTodos.Name = "btnDesmarcarTodos";
            this.btnDesmarcarTodos.Size = new System.Drawing.Size(170, 50);
            this.btnDesmarcarTodos.TabIndex = 2;
            this.btnDesmarcarTodos.Text = "? Desmarcar Todos";
            this.btnDesmarcarTodos.UseVisualStyleBackColor = false;
            this.btnDesmarcarTodos.Click += new System.EventHandler(this.btnDesmarcarTodos_Click);
            // 
            // btnMarcarTodos
            // 
            this.btnMarcarTodos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnMarcarTodos.FlatAppearance.BorderSize = 0;
            this.btnMarcarTodos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMarcarTodos.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnMarcarTodos.ForeColor = System.Drawing.Color.White;
            this.btnMarcarTodos.Location = new System.Drawing.Point(25, 0);
            this.btnMarcarTodos.Name = "btnMarcarTodos";
            this.btnMarcarTodos.Size = new System.Drawing.Size(170, 50);
            this.btnMarcarTodos.TabIndex = 1;
            this.btnMarcarTodos.Text = "? Marcar Todos";
            this.btnMarcarTodos.UseVisualStyleBackColor = false;
            this.btnMarcarTodos.Click += new System.EventHandler(this.btnMarcarTodos_Click);
            // 
            // btnAgregarConcepto
            // 
            this.btnAgregarConcepto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnAgregarConcepto.FlatAppearance.BorderSize = 0;
            this.btnAgregarConcepto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarConcepto.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAgregarConcepto.ForeColor = System.Drawing.Color.White;
            this.btnAgregarConcepto.Location = new System.Drawing.Point(680, 7);
            this.btnAgregarConcepto.Name = "btnAgregarConcepto";
            this.btnAgregarConcepto.Size = new System.Drawing.Size(160, 36);
            this.btnAgregarConcepto.TabIndex = 4;
            this.btnAgregarConcepto.Text = "\u2795 Agregar Concepto";
            this.btnAgregarConcepto.UseVisualStyleBackColor = false;
            this.btnAgregarConcepto.Click += new System.EventHandler(this.btnAgregarConcepto_Click);
            // 
            // btnGestionarPartidas
            // 
            this.btnGestionarPartidas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.btnGestionarPartidas.FlatAppearance.BorderSize = 0;
            this.btnGestionarPartidas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGestionarPartidas.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnGestionarPartidas.ForeColor = System.Drawing.Color.White;
            this.btnGestionarPartidas.Location = new System.Drawing.Point(850, 7);
            this.btnGestionarPartidas.Name = "btnGestionarPartidas";
            this.btnGestionarPartidas.Size = new System.Drawing.Size(160, 36);
            this.btnGestionarPartidas.TabIndex = 5;
            this.btnGestionarPartidas.Text = "\u270F Gestionar Partidas";
            this.btnGestionarPartidas.UseVisualStyleBackColor = false;
            this.btnGestionarPartidas.Click += new System.EventHandler(this.btnGestionarPartidas_Click);
            // 
            // panelDerecho
            // 
            this.panelDerecho.BackColor = System.Drawing.Color.White;
            this.panelDerecho.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDerecho.Controls.Add(this.pictureBoxPreviewPDF);
            this.panelDerecho.Controls.Add(this.lblPreview);
            this.panelDerecho.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDerecho.Location = new System.Drawing.Point(0, 0);
            this.panelDerecho.Name = "panelDerecho";
            this.panelDerecho.Size = new System.Drawing.Size(696, 500);
            this.panelDerecho.TabIndex = 0;
            // 
            // pictureBoxPreviewPDF
            // 
            this.pictureBoxPreviewPDF.BackColor = System.Drawing.Color.White;
            this.pictureBoxPreviewPDF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreviewPDF.Location = new System.Drawing.Point(0, 35);
            this.pictureBoxPreviewPDF.Name = "pictureBoxPreviewPDF";
            this.pictureBoxPreviewPDF.Size = new System.Drawing.Size(694, 463);
            this.pictureBoxPreviewPDF.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPreviewPDF.TabIndex = 1;
            this.pictureBoxPreviewPDF.TabStop = false;
            // 
            // lblPreview
            // 
            this.lblPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.lblPreview.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPreview.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPreview.ForeColor = System.Drawing.Color.White;
            this.lblPreview.Location = new System.Drawing.Point(0, 0);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblPreview.Size = new System.Drawing.Size(694, 35);
            this.lblPreview.TabIndex = 0;
            this.lblPreview.Text = "?? Vista Previa del PDF";
            this.lblPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormEstimacionConceptoMigrado
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelTotales);
            this.Controls.Add(this.panelFiltros);
            this.Controls.Add(this.panelTop);
            this.Name = "FormEstimacionConceptoMigrado";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Estimación por Conceptos - Sistema Pilaris";
            this.panelTop.ResumeLayout(false);
            this.panelFiltros.ResumeLayout(false);
            this.panelFiltros.PerformLayout();
            this.panelTotales.ResumeLayout(false);
            this.panelTotales.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelIzquierdo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvEstimacionConceptos)).EndInit();
            this.panelBotones.ResumeLayout(false);
            this.panelDerecho.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewPDF)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelFiltros;
        private System.Windows.Forms.Button btnRepositorioPDFs;
        private System.Windows.Forms.Button btnSeleccionarEvidencias;
        private System.Windows.Forms.Label lblEvidenciasSeleccionadas;
        private System.Windows.Forms.Button btnCargarAvance;
        private System.Windows.Forms.ComboBox cmbLote;
        private System.Windows.Forms.Label lblLote;
        private System.Windows.Forms.ComboBox cmbManzana;
        private System.Windows.Forms.Label lblManzana;
        private System.Windows.Forms.TextBox txtDescripcion;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.TextBox txtProveedor;
        private System.Windows.Forms.Label lblProveedor;
        private System.Windows.Forms.TextBox txtFirma3;
        private System.Windows.Forms.Label lblFirma3;
        private System.Windows.Forms.TextBox txtFirma2;
        private System.Windows.Forms.Label lblFirma2;
        private System.Windows.Forms.TextBox txtFirma1;
        private System.Windows.Forms.Label lblFirma1;
        private System.Windows.Forms.TextBox txtPuestoFirma3;
        private System.Windows.Forms.TextBox txtPuestoFirma2;
        private System.Windows.Forms.TextBox txtPuestoFirma1;
        private System.Windows.Forms.Panel panelTotales;
        private System.Windows.Forms.ProgressBar progressBarAvance;
        private System.Windows.Forms.Label lblEstadisticas;
        private System.Windows.Forms.Label lblAvanceGeneral;
        private System.Windows.Forms.Label lblPorEjecutar;
        private System.Windows.Forms.Label lblTotalEjecutado;
        private System.Windows.Forms.Label lblTotalPresupuestado;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelIzquierdo;
        private DynamicSepticSystem.SafeTreeListView olvEstimacionConceptos;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Button btnGuardarYExportar;
        private System.Windows.Forms.Button btnDesmarcarTodos;
        private System.Windows.Forms.Button btnMarcarTodos;
        private System.Windows.Forms.Button btnAgregarConcepto;
        private System.Windows.Forms.Button btnGestionarPartidas;
        private System.Windows.Forms.Panel panelDerecho;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreviewPDF;
    }
}
