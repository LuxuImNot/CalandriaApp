namespace DynamicSepticSystem
{
    partial class FormEvidenciasFotograficas
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
            this.panelSuperior = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.panelControles = new System.Windows.Forms.Panel();
            this.btnGuardarFoto = new System.Windows.Forms.Button();
            this.btnCapturarFoto = new System.Windows.Forms.Button();
            this.txtTituloFoto = new System.Windows.Forms.TextBox();
            this.lblTituloFoto = new System.Windows.Forms.Label();
            this.cmbLote = new System.Windows.Forms.ComboBox();
            this.lblLote = new System.Windows.Forms.Label();
            this.cmbManzana = new System.Windows.Forms.ComboBox();
            this.lblManzana = new System.Windows.Forms.Label();
            this.panelCentral = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBoxVistaPrevia = new System.Windows.Forms.GroupBox();
            this.picVistaPrevia = new System.Windows.Forms.PictureBox();
            this.lblInfoImagen = new System.Windows.Forms.Label();
            this.groupBoxEvidencias = new System.Windows.Forms.GroupBox();
            this.dgvEvidencias = new System.Windows.Forms.DataGridView();
            this.panelBotonesGrid = new System.Windows.Forms.Panel();
            this.btnEliminarFoto = new System.Windows.Forms.Button();
            this.btnVerFoto = new System.Windows.Forms.Button();
            this.btnExportarPDF = new System.Windows.Forms.Button();
            this.lblEstadisticas = new System.Windows.Forms.Label();
            this.panelSuperior.SuspendLayout();
            this.panelControles.SuspendLayout();
            this.panelCentral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBoxVistaPrevia.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picVistaPrevia)).BeginInit();
            this.groupBoxEvidencias.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEvidencias)).BeginInit();
            this.panelBotonesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSuperior
            // 
            this.panelSuperior.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panelSuperior.Controls.Add(this.lblTitulo);
            this.panelSuperior.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSuperior.Location = new System.Drawing.Point(0, 0);
            this.panelSuperior.Name = "panelSuperior";
            this.panelSuperior.Size = new System.Drawing.Size(1200, 60);
            this.panelSuperior.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(1200, 60);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "?? EVIDENCIAS FOTOGRÁFICAS";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelControles
            // 
            this.panelControles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelControles.Controls.Add(this.btnGuardarFoto);
            this.panelControles.Controls.Add(this.btnCapturarFoto);
            this.panelControles.Controls.Add(this.txtTituloFoto);
            this.panelControles.Controls.Add(this.lblTituloFoto);
            this.panelControles.Controls.Add(this.cmbLote);
            this.panelControles.Controls.Add(this.lblLote);
            this.panelControles.Controls.Add(this.cmbManzana);
            this.panelControles.Controls.Add(this.lblManzana);
            this.panelControles.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControles.Location = new System.Drawing.Point(0, 60);
            this.panelControles.Name = "panelControles";
            this.panelControles.Padding = new System.Windows.Forms.Padding(10);
            this.panelControles.Size = new System.Drawing.Size(1200, 100);
            this.panelControles.TabIndex = 1;
            // 
            // btnGuardarFoto
            // 
            this.btnGuardarFoto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnGuardarFoto.Enabled = false;
            this.btnGuardarFoto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarFoto.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnGuardarFoto.ForeColor = System.Drawing.Color.White;
            this.btnGuardarFoto.Location = new System.Drawing.Point(1020, 50);
            this.btnGuardarFoto.Name = "btnGuardarFoto";
            this.btnGuardarFoto.Size = new System.Drawing.Size(160, 35);
            this.btnGuardarFoto.TabIndex = 7;
            this.btnGuardarFoto.Text = "?? Guardar Evidencia";
            this.btnGuardarFoto.UseVisualStyleBackColor = false;
            this.btnGuardarFoto.Click += new System.EventHandler(this.btnGuardarFoto_Click);
            // 
            // btnCapturarFoto
            // 
            this.btnCapturarFoto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnCapturarFoto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapturarFoto.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCapturarFoto.ForeColor = System.Drawing.Color.White;
            this.btnCapturarFoto.Location = new System.Drawing.Point(820, 50);
            this.btnCapturarFoto.Name = "btnCapturarFoto";
            this.btnCapturarFoto.Size = new System.Drawing.Size(190, 35);
            this.btnCapturarFoto.TabIndex = 6;
            this.btnCapturarFoto.Text = "?? Capturar/Seleccionar Foto";
            this.btnCapturarFoto.UseVisualStyleBackColor = false;
            this.btnCapturarFoto.Click += new System.EventHandler(this.btnCapturarFoto_Click);
            // 
            // txtTituloFoto
            // 
            this.txtTituloFoto.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTituloFoto.Location = new System.Drawing.Point(420, 55);
            this.txtTituloFoto.MaxLength = 200;
            this.txtTituloFoto.Name = "txtTituloFoto";
            this.txtTituloFoto.Size = new System.Drawing.Size(380, 25);
            this.txtTituloFoto.TabIndex = 5;
            this.txtTituloFoto.TextChanged += new System.EventHandler(this.txtTituloFoto_TextChanged);
            // 
            // lblTituloFoto
            // 
            this.lblTituloFoto.AutoSize = true;
            this.lblTituloFoto.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTituloFoto.Location = new System.Drawing.Point(420, 35);
            this.lblTituloFoto.Name = "lblTituloFoto";
            this.lblTituloFoto.Size = new System.Drawing.Size(158, 15);
            this.lblTituloFoto.TabIndex = 4;
            this.lblTituloFoto.Text = "Título/Descripción de la Foto:";
            // 
            // cmbLote
            // 
            this.cmbLote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLote.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbLote.FormattingEnabled = true;
            this.cmbLote.Location = new System.Drawing.Point(230, 55);
            this.cmbLote.Name = "cmbLote";
            this.cmbLote.Size = new System.Drawing.Size(160, 25);
            this.cmbLote.TabIndex = 3;
            this.cmbLote.SelectedIndexChanged += new System.EventHandler(this.cmbLote_SelectedIndexChanged);
            // 
            // lblLote
            // 
            this.lblLote.AutoSize = true;
            this.lblLote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLote.Location = new System.Drawing.Point(230, 35);
            this.lblLote.Name = "lblLote";
            this.lblLote.Size = new System.Drawing.Size(34, 15);
            this.lblLote.TabIndex = 2;
            this.lblLote.Text = "Lote:";
            // 
            // cmbManzana
            // 
            this.cmbManzana.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManzana.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbManzana.FormattingEnabled = true;
            this.cmbManzana.Location = new System.Drawing.Point(20, 55);
            this.cmbManzana.Name = "cmbManzana";
            this.cmbManzana.Size = new System.Drawing.Size(180, 25);
            this.cmbManzana.TabIndex = 1;
            this.cmbManzana.SelectedIndexChanged += new System.EventHandler(this.cmbManzana_SelectedIndexChanged);
            // 
            // lblManzana
            // 
            this.lblManzana.AutoSize = true;
            this.lblManzana.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblManzana.Location = new System.Drawing.Point(20, 35);
            this.lblManzana.Name = "lblManzana";
            this.lblManzana.Size = new System.Drawing.Size(60, 15);
            this.lblManzana.TabIndex = 0;
            this.lblManzana.Text = "Manzana:";
            // 
            // panelCentral
            // 
            this.panelCentral.Controls.Add(this.splitContainer1);
            this.panelCentral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCentral.Location = new System.Drawing.Point(0, 160);
            this.panelCentral.Name = "panelCentral";
            this.panelCentral.Padding = new System.Windows.Forms.Padding(10);
            this.panelCentral.Size = new System.Drawing.Size(1200, 540);
            this.panelCentral.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(10, 10);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBoxVistaPrevia);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxEvidencias);
            this.splitContainer1.Size = new System.Drawing.Size(1180, 520);
            this.splitContainer1.SplitterDistance = 450;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBoxVistaPrevia
            // 
            this.groupBoxVistaPrevia.Controls.Add(this.picVistaPrevia);
            this.groupBoxVistaPrevia.Controls.Add(this.lblInfoImagen);
            this.groupBoxVistaPrevia.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxVistaPrevia.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxVistaPrevia.Location = new System.Drawing.Point(0, 0);
            this.groupBoxVistaPrevia.Name = "groupBoxVistaPrevia";
            this.groupBoxVistaPrevia.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxVistaPrevia.Size = new System.Drawing.Size(450, 520);
            this.groupBoxVistaPrevia.TabIndex = 0;
            this.groupBoxVistaPrevia.TabStop = false;
            this.groupBoxVistaPrevia.Text = "Vista Previa de la Foto";
            // 
            // picVistaPrevia
            // 
            this.picVistaPrevia.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picVistaPrevia.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picVistaPrevia.Location = new System.Drawing.Point(10, 26);
            this.picVistaPrevia.Name = "picVistaPrevia";
            this.picVistaPrevia.Size = new System.Drawing.Size(430, 454);
            this.picVistaPrevia.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picVistaPrevia.TabIndex = 0;
            this.picVistaPrevia.TabStop = false;
            this.picVistaPrevia.DoubleClick += new System.EventHandler(this.picVistaPrevia_DoubleClick);
            // 
            // lblInfoImagen
            // 
            this.lblInfoImagen.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblInfoImagen.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblInfoImagen.ForeColor = System.Drawing.Color.Gray;
            this.lblInfoImagen.Location = new System.Drawing.Point(10, 480);
            this.lblInfoImagen.Name = "lblInfoImagen";
            this.lblInfoImagen.Size = new System.Drawing.Size(430, 30);
            this.lblInfoImagen.TabIndex = 1;
            this.lblInfoImagen.Text = "Selecciona una foto para ver la vista previa";
            this.lblInfoImagen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxEvidencias
            // 
            this.groupBoxEvidencias.Controls.Add(this.dgvEvidencias);
            this.groupBoxEvidencias.Controls.Add(this.panelBotonesGrid);
            this.groupBoxEvidencias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxEvidencias.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxEvidencias.Location = new System.Drawing.Point(0, 0);
            this.groupBoxEvidencias.Name = "groupBoxEvidencias";
            this.groupBoxEvidencias.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxEvidencias.Size = new System.Drawing.Size(726, 520);
            this.groupBoxEvidencias.TabIndex = 0;
            this.groupBoxEvidencias.TabStop = false;
            this.groupBoxEvidencias.Text = "Evidencias Guardadas";
            // 
            // dgvEvidencias
            // 
            this.dgvEvidencias.AllowUserToAddRows = false;
            this.dgvEvidencias.AllowUserToDeleteRows = false;
            this.dgvEvidencias.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvEvidencias.BackgroundColor = System.Drawing.Color.White;
            this.dgvEvidencias.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEvidencias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEvidencias.Location = new System.Drawing.Point(10, 26);
            this.dgvEvidencias.MultiSelect = false;
            this.dgvEvidencias.Name = "dgvEvidencias";
            this.dgvEvidencias.ReadOnly = true;
            this.dgvEvidencias.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvEvidencias.Size = new System.Drawing.Size(706, 424);
            this.dgvEvidencias.TabIndex = 0;
            this.dgvEvidencias.SelectionChanged += new System.EventHandler(this.dgvEvidencias_SelectionChanged);
            this.dgvEvidencias.DoubleClick += new System.EventHandler(this.dgvEvidencias_DoubleClick);
            // 
            // panelBotonesGrid
            // 
            this.panelBotonesGrid.Controls.Add(this.lblEstadisticas);
            this.panelBotonesGrid.Controls.Add(this.btnExportarPDF);
            this.panelBotonesGrid.Controls.Add(this.btnEliminarFoto);
            this.panelBotonesGrid.Controls.Add(this.btnVerFoto);
            this.panelBotonesGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotonesGrid.Location = new System.Drawing.Point(10, 450);
            this.panelBotonesGrid.Name = "panelBotonesGrid";
            this.panelBotonesGrid.Size = new System.Drawing.Size(706, 60);
            this.panelBotonesGrid.TabIndex = 1;
            // 
            // btnEliminarFoto
            // 
            this.btnEliminarFoto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnEliminarFoto.Enabled = false;
            this.btnEliminarFoto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminarFoto.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEliminarFoto.ForeColor = System.Drawing.Color.White;
            this.btnEliminarFoto.Location = new System.Drawing.Point(135, 10);
            this.btnEliminarFoto.Name = "btnEliminarFoto";
            this.btnEliminarFoto.Size = new System.Drawing.Size(120, 40);
            this.btnEliminarFoto.TabIndex = 1;
            this.btnEliminarFoto.Text = "??? Eliminar";
            this.btnEliminarFoto.UseVisualStyleBackColor = false;
            this.btnEliminarFoto.Click += new System.EventHandler(this.btnEliminarFoto_Click);
            // 
            // btnVerFoto
            // 
            this.btnVerFoto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnVerFoto.Enabled = false;
            this.btnVerFoto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerFoto.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnVerFoto.ForeColor = System.Drawing.Color.White;
            this.btnVerFoto.Location = new System.Drawing.Point(10, 10);
            this.btnVerFoto.Name = "btnVerFoto";
            this.btnVerFoto.Size = new System.Drawing.Size(120, 40);
            this.btnVerFoto.TabIndex = 0;
            this.btnVerFoto.Text = "??? Ver Foto";
            this.btnVerFoto.UseVisualStyleBackColor = false;
            this.btnVerFoto.Click += new System.EventHandler(this.btnVerFoto_Click);
            // 
            // btnExportarPDF
            // 
            this.btnExportarPDF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.btnExportarPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportarPDF.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportarPDF.ForeColor = System.Drawing.Color.White;
            this.btnExportarPDF.Location = new System.Drawing.Point(260, 10);
            this.btnExportarPDF.Name = "btnExportarPDF";
            this.btnExportarPDF.Size = new System.Drawing.Size(150, 40);
            this.btnExportarPDF.TabIndex = 2;
            this.btnExportarPDF.Text = "?? Exportar a PDF";
            this.btnExportarPDF.UseVisualStyleBackColor = false;
            this.btnExportarPDF.Click += new System.EventHandler(this.btnExportarPDF_Click);
            // 
            // lblEstadisticas
            // 
            this.lblEstadisticas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEstadisticas.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblEstadisticas.ForeColor = System.Drawing.Color.Gray;
            this.lblEstadisticas.Location = new System.Drawing.Point(420, 10);
            this.lblEstadisticas.Name = "lblEstadisticas";
            this.lblEstadisticas.Size = new System.Drawing.Size(280, 40);
            this.lblEstadisticas.TabIndex = 3;
            this.lblEstadisticas.Text = "Total evidencias: 0";
            this.lblEstadisticas.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormEvidenciasFotograficas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.panelCentral);
            this.Controls.Add(this.panelControles);
            this.Controls.Add(this.panelSuperior);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "FormEvidenciasFotograficas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Evidencias Fotográficas - Sistema Pilaris";
            this.Load += new System.EventHandler(this.FormEvidenciasFotograficas_Load);
            this.panelSuperior.ResumeLayout(false);
            this.panelControles.ResumeLayout(false);
            this.panelControles.PerformLayout();
            this.panelCentral.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBoxVistaPrevia.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picVistaPrevia)).EndInit();
            this.groupBoxEvidencias.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvEvidencias)).EndInit();
            this.panelBotonesGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSuperior;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelControles;
        private System.Windows.Forms.ComboBox cmbManzana;
        private System.Windows.Forms.Label lblManzana;
        private System.Windows.Forms.ComboBox cmbLote;
        private System.Windows.Forms.Label lblLote;
        private System.Windows.Forms.TextBox txtTituloFoto;
        private System.Windows.Forms.Label lblTituloFoto;
        private System.Windows.Forms.Button btnCapturarFoto;
        private System.Windows.Forms.Button btnGuardarFoto;
        private System.Windows.Forms.Panel panelCentral;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBoxVistaPrevia;
        private System.Windows.Forms.PictureBox picVistaPrevia;
        private System.Windows.Forms.Label lblInfoImagen;
        private System.Windows.Forms.GroupBox groupBoxEvidencias;
        private System.Windows.Forms.DataGridView dgvEvidencias;
        private System.Windows.Forms.Panel panelBotonesGrid;
        private System.Windows.Forms.Button btnVerFoto;
        private System.Windows.Forms.Button btnEliminarFoto;
        private System.Windows.Forms.Button btnExportarPDF;
        private System.Windows.Forms.Label lblEstadisticas;
    }
}
