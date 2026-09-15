namespace DynamicSepticSystem
{
    partial class FormAvanceObra
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
            this.cmbManzana = new System.Windows.Forms.ComboBox();
            this.cmbLote = new System.Windows.Forms.ComboBox();
            this.btnCargarAvance = new System.Windows.Forms.Button();
            this.btnExportarPDF = new System.Windows.Forms.Button();
            this.olvAvance = new DynamicSepticSystem.SafeTreeListView();
            this.lblTotalPresupuestado = new System.Windows.Forms.Label();
            this.lblTotalEjecutado = new System.Windows.Forms.Label();
            this.lblPorEjecutar = new System.Windows.Forms.Label();
            this.lblAvanceGeneral = new System.Windows.Forms.Label();
            this.progressBarAvance = new System.Windows.Forms.ProgressBar();
            this.lblEstadisticas = new System.Windows.Forms.Label();
            this.pictureBoxGrafica = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.olvAvance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGrafica)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbManzana
            // 
            this.cmbManzana.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManzana.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbManzana.FormattingEnabled = true;
            this.cmbManzana.Location = new System.Drawing.Point(100, 20);
            this.cmbManzana.Name = "cmbManzana";
            this.cmbManzana.Size = new System.Drawing.Size(150, 25);
            this.cmbManzana.TabIndex = 0;
            this.cmbManzana.SelectedIndexChanged += new System.EventHandler(this.cmbManzana_SelectedIndexChanged);
            // 
            // cmbLote
            // 
            this.cmbLote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLote.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbLote.FormattingEnabled = true;
            this.cmbLote.Location = new System.Drawing.Point(320, 20);
            this.cmbLote.Name = "cmbLote";
            this.cmbLote.Size = new System.Drawing.Size(150, 25);
            this.cmbLote.TabIndex = 1;
            // 
            // btnCargarAvance
            // 
            this.btnCargarAvance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnCargarAvance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarAvance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCargarAvance.ForeColor = System.Drawing.Color.White;
            this.btnCargarAvance.Location = new System.Drawing.Point(500, 15);
            this.btnCargarAvance.Name = "btnCargarAvance";
            this.btnCargarAvance.Size = new System.Drawing.Size(150, 35);
            this.btnCargarAvance.TabIndex = 2;
            this.btnCargarAvance.Text = "?? Cargar Avance";
            this.btnCargarAvance.UseVisualStyleBackColor = false;
            this.btnCargarAvance.Click += new System.EventHandler(this.btnCargarAvance_Click);
            // 
            // btnExportarPDF
            // 
            this.btnExportarPDF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnExportarPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportarPDF.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnExportarPDF.ForeColor = System.Drawing.Color.White;
            this.btnExportarPDF.Location = new System.Drawing.Point(670, 15);
            this.btnExportarPDF.Name = "btnExportarPDF";
            this.btnExportarPDF.Size = new System.Drawing.Size(150, 35);
            this.btnExportarPDF.TabIndex = 3;
            this.btnExportarPDF.Text = "?? Exportar PDF";
            this.btnExportarPDF.UseVisualStyleBackColor = false;
            this.btnExportarPDF.Click += new System.EventHandler(this.btnExportarPDF_Click);
            // 
            // olvAvance
            // 
            this.olvAvance.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvAvance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.olvAvance.FullRowSelect = true;
            this.olvAvance.HideSelection = false;
            this.olvAvance.Location = new System.Drawing.Point(20, 150);
            this.olvAvance.Name = "olvAvance";
            this.olvAvance.ShowGroups = false;
            this.olvAvance.Size = new System.Drawing.Size(500, 400);
            this.olvAvance.TabIndex = 4;
            this.olvAvance.UseCompatibleStateImageBehavior = false;
            this.olvAvance.View = System.Windows.Forms.View.Details;
            this.olvAvance.VirtualMode = true;
            // 
            // lblTotalPresupuestado
            // 
            this.lblTotalPresupuestado.AutoSize = true;
            this.lblTotalPresupuestado.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular);
            this.lblTotalPresupuestado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTotalPresupuestado.Location = new System.Drawing.Point(20, 60);
            this.lblTotalPresupuestado.Name = "lblTotalPresupuestado";
            this.lblTotalPresupuestado.Size = new System.Drawing.Size(196, 20);
            this.lblTotalPresupuestado.TabIndex = 5;
            this.lblTotalPresupuestado.Text = "Total Presupuestado: $0.00";
            // 
            // lblTotalEjecutado
            // 
            this.lblTotalEjecutado.AutoSize = true;
            this.lblTotalEjecutado.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular);
            this.lblTotalEjecutado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblTotalEjecutado.Location = new System.Drawing.Point(280, 60);
            this.lblTotalEjecutado.Name = "lblTotalEjecutado";
            this.lblTotalEjecutado.Size = new System.Drawing.Size(158, 20);
            this.lblTotalEjecutado.TabIndex = 6;
            this.lblTotalEjecutado.Text = "Total Ejecutado: $0.00";
            // 
            // lblPorEjecutar
            // 
            this.lblPorEjecutar.AutoSize = true;
            this.lblPorEjecutar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular);
            this.lblPorEjecutar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.lblPorEjecutar.Location = new System.Drawing.Point(540, 60);
            this.lblPorEjecutar.Name = "lblPorEjecutar";
            this.lblPorEjecutar.Size = new System.Drawing.Size(141, 20);
            this.lblPorEjecutar.TabIndex = 13;
            this.lblPorEjecutar.Text = "Por Ejecutar: $0.00";
            // 
            // lblAvanceGeneral
            // 
            this.lblAvanceGeneral.AutoSize = true;
            this.lblAvanceGeneral.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblAvanceGeneral.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblAvanceGeneral.Location = new System.Drawing.Point(20, 95);
            this.lblAvanceGeneral.Name = "lblAvanceGeneral";
            this.lblAvanceGeneral.Size = new System.Drawing.Size(158, 21);
            this.lblAvanceGeneral.TabIndex = 7;
            this.lblAvanceGeneral.Text = "Avance General: 0%";
            // 
            // progressBarAvance
            // 
            this.progressBarAvance.Location = new System.Drawing.Point(200, 95);
            this.progressBarAvance.Name = "progressBarAvance";
            this.progressBarAvance.Size = new System.Drawing.Size(350, 25);
            this.progressBarAvance.TabIndex = 8;
            // 
            // lblEstadisticas
            // 
            this.lblEstadisticas.AutoSize = true;
            this.lblEstadisticas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadisticas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblEstadisticas.Location = new System.Drawing.Point(570, 100);
            this.lblEstadisticas.Name = "lblEstadisticas";
            this.lblEstadisticas.Size = new System.Drawing.Size(250, 15);
            this.lblEstadisticas.TabIndex = 9;
            this.lblEstadisticas.Text = "Completados: 0 | En Progreso: 0 | Sin Iniciar: 0";
            // 
            // pictureBoxGrafica
            // 
            this.pictureBoxGrafica.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxGrafica.BackColor = System.Drawing.Color.White;
            this.pictureBoxGrafica.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxGrafica.Location = new System.Drawing.Point(540, 150);
            this.pictureBoxGrafica.Name = "pictureBoxGrafica";
            this.pictureBoxGrafica.Size = new System.Drawing.Size(400, 400);
            this.pictureBoxGrafica.TabIndex = 10;
            this.pictureBoxGrafica.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(20, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 19);
            this.label1.TabIndex = 11;
            this.label1.Text = "Manzana:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(270, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 19);
            this.label2.TabIndex = 12;
            this.label2.Text = "Lote:";
            // 
            // FormAvanceObra
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(960, 570);
            this.Controls.Add(this.lblPorEjecutar);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBoxGrafica);
            this.Controls.Add(this.lblEstadisticas);
            this.Controls.Add(this.progressBarAvance);
            this.Controls.Add(this.lblAvanceGeneral);
            this.Controls.Add(this.lblTotalEjecutado);
            this.Controls.Add(this.lblTotalPresupuestado);
            this.Controls.Add(this.olvAvance);
            this.Controls.Add(this.btnExportarPDF);
            this.Controls.Add(this.btnCargarAvance);
            this.Controls.Add(this.cmbLote);
            this.Controls.Add(this.cmbManzana);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "FormAvanceObra";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Avance de Obra - Sistema Pilaris";
            this.Resize += new System.EventHandler(this.FormAvanceObra_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.olvAvance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGrafica)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbManzana;
        private System.Windows.Forms.ComboBox cmbLote;
        private System.Windows.Forms.Button btnCargarAvance;
        private System.Windows.Forms.Button btnExportarPDF;
        private DynamicSepticSystem.SafeTreeListView olvAvance;
        private System.Windows.Forms.Label lblTotalPresupuestado;
        private System.Windows.Forms.Label lblTotalEjecutado;
        private System.Windows.Forms.Label lblPorEjecutar;
        private System.Windows.Forms.Label lblAvanceGeneral;
        private System.Windows.Forms.ProgressBar progressBarAvance;
        private System.Windows.Forms.Label lblEstadisticas;
        private System.Windows.Forms.PictureBox pictureBoxGrafica;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
