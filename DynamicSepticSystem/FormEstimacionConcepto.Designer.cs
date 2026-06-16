namespace DynamicSepticSystem
{
    partial class FormEstimacionConcepto
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.panelFiltros = new System.Windows.Forms.Panel();
            this.btnCargarAvance = new System.Windows.Forms.Button();
            this.cmbLote = new System.Windows.Forms.ComboBox();
            this.lblLote = new System.Windows.Forms.Label();
            this.cmbManzana = new System.Windows.Forms.ComboBox();
            this.lblManzana = new System.Windows.Forms.Label();
            this.panelTotales = new System.Windows.Forms.Panel();
            this.progressBarAvance = new System.Windows.Forms.ProgressBar();
            this.lblEstadisticas = new System.Windows.Forms.Label();
            this.lblAvanceGeneral = new System.Windows.Forms.Label();
            this.lblTotalEjecutado = new System.Windows.Forms.Label();
            this.lblTotalPresupuestado = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.btnExportarPDF = new System.Windows.Forms.Button();
            this.btnDesmarcarTodos = new System.Windows.Forms.Button();
            this.btnMarcarTodos = new System.Windows.Forms.Button();
            this.olvEstimacionConceptos = new BrightIdeasSoftware.ObjectListView();
            this.pictureBoxGrafica = new System.Windows.Forms.PictureBox();
            this.panelTop.SuspendLayout();
            this.panelFiltros.SuspendLayout();
            this.panelTotales.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvEstimacionConceptos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGrafica)).BeginInit();
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
            this.panelFiltros.Controls.Add(this.btnCargarAvance);
            this.panelFiltros.Controls.Add(this.cmbLote);
            this.panelFiltros.Controls.Add(this.lblLote);
            this.panelFiltros.Controls.Add(this.cmbManzana);
            this.panelFiltros.Controls.Add(this.lblManzana);
            this.panelFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltros.Location = new System.Drawing.Point(0, 60);
            this.panelFiltros.Name = "panelFiltros";
            this.panelFiltros.Size = new System.Drawing.Size(1400, 70);
            this.panelFiltros.TabIndex = 1;
            // 
            // btnCargarAvance
            // 
            this.btnCargarAvance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnCargarAvance.FlatAppearance.BorderSize = 0;
            this.btnCargarAvance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarAvance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCargarAvance.ForeColor = System.Drawing.Color.White;
            this.btnCargarAvance.Location = new System.Drawing.Point(550, 18);
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
            this.cmbLote.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbLote.FormattingEnabled = true;
            this.cmbLote.Location = new System.Drawing.Point(370, 22);
            this.cmbLote.Name = "cmbLote";
            this.cmbLote.Size = new System.Drawing.Size(150, 25);
            this.cmbLote.TabIndex = 3;
            // 
            // lblLote
            // 
            this.lblLote.AutoSize = true;
            this.lblLote.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLote.Location = new System.Drawing.Point(320, 25);
            this.lblLote.Name = "lblLote";
            this.lblLote.Size = new System.Drawing.Size(44, 19);
            this.lblLote.TabIndex = 2;
            this.lblLote.Text = "Lote:";
            // 
            // cmbManzana
            // 
            this.cmbManzana.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManzana.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.lblManzana.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblManzana.Location = new System.Drawing.Point(30, 25);
            this.lblManzana.Name = "lblManzana";
            this.lblManzana.Size = new System.Drawing.Size(76, 19);
            this.lblManzana.TabIndex = 0;
            this.lblManzana.Text = "Manzana:";
            // 
            // panelTotales
            // 
            this.panelTotales.BackColor = System.Drawing.Color.White;
            this.panelTotales.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTotales.Controls.Add(this.progressBarAvance);
            this.panelTotales.Controls.Add(this.lblEstadisticas);
            this.panelTotales.Controls.Add(this.lblAvanceGeneral);
            this.panelTotales.Controls.Add(this.lblTotalEjecutado);
            this.panelTotales.Controls.Add(this.lblTotalPresupuestado);
            this.panelTotales.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTotales.Location = new System.Drawing.Point(0, 130);
            this.panelTotales.Name = "panelTotales";
            this.panelTotales.Size = new System.Drawing.Size(1400, 100);
            this.panelTotales.TabIndex = 2;
            // 
            // progressBarAvance
            // 
            this.progressBarAvance.Location = new System.Drawing.Point(30, 70);
            this.progressBarAvance.Name = "progressBarAvance";
            this.progressBarAvance.Size = new System.Drawing.Size(400, 20);
            this.progressBarAvance.TabIndex = 4;
            // 
            // lblEstadisticas
            // 
            this.lblEstadisticas.AutoSize = true;
            this.lblEstadisticas.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEstadisticas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblEstadisticas.Location = new System.Drawing.Point(470, 70);
            this.lblEstadisticas.Name = "lblEstadisticas";
            this.lblEstadisticas.Size = new System.Drawing.Size(250, 15);
            this.lblEstadisticas.TabIndex = 3;
            this.lblEstadisticas.Text = "Completados: 0 | En Progreso: 0 | Sin Iniciar: 0";
            // 
            // lblAvanceGeneral
            // 
            this.lblAvanceGeneral.AutoSize = true;
            this.lblAvanceGeneral.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvanceGeneral.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblAvanceGeneral.Location = new System.Drawing.Point(30, 40);
            this.lblAvanceGeneral.Name = "lblAvanceGeneral";
            this.lblAvanceGeneral.Size = new System.Drawing.Size(158, 21);
            this.lblAvanceGeneral.TabIndex = 2;
            this.lblAvanceGeneral.Text = "Avance General: 0%";
            // 
            // lblTotalEjecutado
            // 
            this.lblTotalEjecutado.AutoSize = true;
            this.lblTotalEjecutado.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalEjecutado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblTotalEjecutado.Location = new System.Drawing.Point(470, 40);
            this.lblTotalEjecutado.Name = "lblTotalEjecutado";
            this.lblTotalEjecutado.Size = new System.Drawing.Size(158, 20);
            this.lblTotalEjecutado.TabIndex = 1;
            this.lblTotalEjecutado.Text = "Total Ejecutado: $0.00";
            // 
            // lblTotalPresupuestado
            // 
            this.lblTotalPresupuestado.AutoSize = true;
            this.lblTotalPresupuestado.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalPresupuestado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTotalPresupuestado.Location = new System.Drawing.Point(470, 10);
            this.lblTotalPresupuestado.Name = "lblTotalPresupuestado";
            this.lblTotalPresupuestado.Size = new System.Drawing.Size(196, 20);
            this.lblTotalPresupuestado.TabIndex = 0;
            this.lblTotalPresupuestado.Text = "Total Presupuestado: $0.00";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 230);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelBotones);
            this.splitContainer1.Panel1.Controls.Add(this.olvEstimacionConceptos);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pictureBoxGrafica);
            this.splitContainer1.Size = new System.Drawing.Size(1400, 570);
            this.splitContainer1.SplitterDistance = 320;
            this.splitContainer1.TabIndex = 3;
            // 
            // panelBotones
            // 
            this.panelBotones.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelBotones.Controls.Add(this.btnExportarPDF);
            this.panelBotones.Controls.Add(this.btnDesmarcarTodos);
            this.panelBotones.Controls.Add(this.btnMarcarTodos);
            this.panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotones.Location = new System.Drawing.Point(0, 270);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Size = new System.Drawing.Size(1400, 50);
            this.panelBotones.TabIndex = 1;
            // 
            // btnExportarPDF
            // 
            this.btnExportarPDF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnExportarPDF.FlatAppearance.BorderSize = 0;
            this.btnExportarPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportarPDF.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportarPDF.ForeColor = System.Drawing.Color.White;
            this.btnExportarPDF.Location = new System.Drawing.Point(390, 8);
            this.btnExportarPDF.Name = "btnExportarPDF";
            this.btnExportarPDF.Size = new System.Drawing.Size(180, 35);
            this.btnExportarPDF.TabIndex = 2;
            this.btnExportarPDF.Text = "?? Exportar a PDF";
            this.btnExportarPDF.UseVisualStyleBackColor = false;
            this.btnExportarPDF.Click += new System.EventHandler(this.btnExportarPDF_Click);
            // 
            // btnDesmarcarTodos
            // 
            this.btnDesmarcarTodos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnDesmarcarTodos.FlatAppearance.BorderSize = 0;
            this.btnDesmarcarTodos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesmarcarTodos.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDesmarcarTodos.ForeColor = System.Drawing.Color.White;
            this.btnDesmarcarTodos.Location = new System.Drawing.Point(200, 8);
            this.btnDesmarcarTodos.Name = "btnDesmarcarTodos";
            this.btnDesmarcarTodos.Size = new System.Drawing.Size(160, 35);
            this.btnDesmarcarTodos.TabIndex = 1;
            this.btnDesmarcarTodos.Text = "? Desmarcar Todos";
            this.btnDesmarcarTodos.UseVisualStyleBackColor = false;
            this.btnDesmarcarTodos.Click += new System.EventHandler(this.btnDesmarcarTodos_Click);
            // 
            // btnMarcarTodos
            // 
            this.btnMarcarTodos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnMarcarTodos.FlatAppearance.BorderSize = 0;
            this.btnMarcarTodos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMarcarTodos.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMarcarTodos.ForeColor = System.Drawing.Color.White;
            this.btnMarcarTodos.Location = new System.Drawing.Point(30, 8);
            this.btnMarcarTodos.Name = "btnMarcarTodos";
            this.btnMarcarTodos.Size = new System.Drawing.Size(160, 35);
            this.btnMarcarTodos.TabIndex = 0;
            this.btnMarcarTodos.Text = "? Marcar Todos";
            this.btnMarcarTodos.UseVisualStyleBackColor = false;
            this.btnMarcarTodos.Click += new System.EventHandler(this.btnMarcarTodos_Click);
            // 
            // olvEstimacionConceptos
            // 
            this.olvEstimacionConceptos.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;
            this.olvEstimacionConceptos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvEstimacionConceptos.FullRowSelect = true;
            this.olvEstimacionConceptos.HideSelection = false;
            this.olvEstimacionConceptos.Location = new System.Drawing.Point(0, 0);
            this.olvEstimacionConceptos.Name = "olvEstimacionConceptos";
            this.olvEstimacionConceptos.ShowGroups = false;
            this.olvEstimacionConceptos.Size = new System.Drawing.Size(1400, 320);
            this.olvEstimacionConceptos.TabIndex = 0;
            this.olvEstimacionConceptos.UseAlternatingBackColors = true;
            this.olvEstimacionConceptos.UseCompatibleStateImageBehavior = false;
            this.olvEstimacionConceptos.View = System.Windows.Forms.View.Details;
            // ? BLOQUEAR ORDENAMIENTO POR CLIC EN ENCABEZADO
            this.olvEstimacionConceptos.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            // 
            // pictureBoxGrafica
            // 
            this.pictureBoxGrafica.BackColor = System.Drawing.Color.White;
            this.pictureBoxGrafica.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxGrafica.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxGrafica.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxGrafica.Name = "pictureBoxGrafica";
            this.pictureBoxGrafica.Size = new System.Drawing.Size(1400, 246);
            this.pictureBoxGrafica.TabIndex = 0;
            this.pictureBoxGrafica.TabStop = false;
            // 
            // FormEstimacionConcepto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelTotales);
            this.Controls.Add(this.panelFiltros);
            this.Controls.Add(this.panelTop);
            this.Name = "FormEstimacionConcepto";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Estimación por Conceptos - Sistema Calandria";
            this.Resize += new System.EventHandler(this.FormEstimacionConcepto_Resize);
            this.panelTop.ResumeLayout(false);
            this.panelFiltros.ResumeLayout(false);
            this.panelFiltros.PerformLayout();
            this.panelTotales.ResumeLayout(false);
            this.panelTotales.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvEstimacionConceptos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGrafica)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelFiltros;
        private System.Windows.Forms.Button btnCargarAvance;
        private System.Windows.Forms.ComboBox cmbLote;
        private System.Windows.Forms.Label lblLote;
        private System.Windows.Forms.ComboBox cmbManzana;
        private System.Windows.Forms.Label lblManzana;
        private System.Windows.Forms.Panel panelTotales;
        private System.Windows.Forms.ProgressBar progressBarAvance;
        private System.Windows.Forms.Label lblEstadisticas;
        private System.Windows.Forms.Label lblAvanceGeneral;
        private System.Windows.Forms.Label lblTotalEjecutado;
        private System.Windows.Forms.Label lblTotalPresupuestado;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Button btnExportarPDF;
        private System.Windows.Forms.Button btnDesmarcarTodos;
        private System.Windows.Forms.Button btnMarcarTodos;
        private BrightIdeasSoftware.ObjectListView olvEstimacionConceptos;
        private System.Windows.Forms.PictureBox pictureBoxGrafica;
    }
}
