namespace DynamicSepticSystem
{
    partial class FormHardProgress
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
            this.btnExportarPDF = new System.Windows.Forms.Button();
            this.btnCargarAvance = new System.Windows.Forms.Button();
            this.cmbLote = new System.Windows.Forms.ComboBox();
            this.cmbManzana = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblEstadisticas = new System.Windows.Forms.Label();
            this.progressBarAvance = new System.Windows.Forms.ProgressBar();
            this.lblAvanceGeneral = new System.Windows.Forms.Label();
            this.lblTotalEjecutado = new System.Windows.Forms.Label();
            this.lblTotalPresupuestado = new System.Windows.Forms.Label();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.olvAvance = new DynamicSepticSystem.SafeTreeListView();
            this.pictureBoxGrafica = new System.Windows.Forms.PictureBox();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvAvance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGrafica)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.panelTop.Controls.Add(this.btnExportarPDF);
            this.panelTop.Controls.Add(this.btnCargarAvance);
            this.panelTop.Controls.Add(this.cmbLote);
            this.panelTop.Controls.Add(this.cmbManzana);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(10);
            this.panelTop.Size = new System.Drawing.Size(1200, 80);
            this.panelTop.TabIndex = 0;
            // 
            // btnExportarPDF
            // 
            this.btnExportarPDF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnExportarPDF.FlatAppearance.BorderSize = 0;
            this.btnExportarPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportarPDF.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnExportarPDF.ForeColor = System.Drawing.Color.White;
            this.btnExportarPDF.Location = new System.Drawing.Point(830, 20);
            this.btnExportarPDF.Name = "btnExportarPDF";
            this.btnExportarPDF.Size = new System.Drawing.Size(150, 40);
            this.btnExportarPDF.TabIndex = 5;
            this.btnExportarPDF.Text = "Exportar PDF";
            this.btnExportarPDF.UseVisualStyleBackColor = false;
            this.btnExportarPDF.Click += new System.EventHandler(this.btnExportarPDF_Click);
            // 
            // btnCargarAvance
            // 
            this.btnCargarAvance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnCargarAvance.FlatAppearance.BorderSize = 0;
            this.btnCargarAvance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarAvance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCargarAvance.ForeColor = System.Drawing.Color.White;
            this.btnCargarAvance.Location = new System.Drawing.Point(650, 20);
            this.btnCargarAvance.Name = "btnCargarAvance";
            this.btnCargarAvance.Size = new System.Drawing.Size(160, 40);
            this.btnCargarAvance.TabIndex = 4;
            this.btnCargarAvance.Text = "Cargar Avance";
            this.btnCargarAvance.UseVisualStyleBackColor = false;
            this.btnCargarAvance.Click += new System.EventHandler(this.btnCargarAvance_Click);
            // 
            // cmbLote
            // 
            this.cmbLote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLote.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.cmbLote.FormattingEnabled = true;
            this.cmbLote.Location = new System.Drawing.Point(390, 27);
            this.cmbLote.Name = "cmbLote";
            this.cmbLote.Size = new System.Drawing.Size(220, 28);
            this.cmbLote.TabIndex = 3;
            // 
            // cmbManzana
            // 
            this.cmbManzana.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManzana.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.cmbManzana.FormattingEnabled = true;
            this.cmbManzana.Location = new System.Drawing.Point(100, 27);
            this.cmbManzana.Name = "cmbManzana";
            this.cmbManzana.Size = new System.Drawing.Size(220, 28);
            this.cmbManzana.TabIndex = 2;
            this.cmbManzana.SelectedIndexChanged += new System.EventHandler(this.cmbManzana_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(340, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 19);
            this.label2.TabIndex = 1;
            this.label2.Text = "Lote:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(20, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Manzana:";
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.panelBottom.Controls.Add(this.lblEstadisticas);
            this.panelBottom.Controls.Add(this.progressBarAvance);
            this.panelBottom.Controls.Add(this.lblAvanceGeneral);
            this.panelBottom.Controls.Add(this.lblTotalEjecutado);
            this.panelBottom.Controls.Add(this.lblTotalPresupuestado);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 620);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(10);
            this.panelBottom.Size = new System.Drawing.Size(1200, 100);
            this.panelBottom.TabIndex = 1;
            // 
            // lblEstadisticas
            // 
            this.lblEstadisticas.AutoSize = true;
            this.lblEstadisticas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadisticas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblEstadisticas.Location = new System.Drawing.Point(20, 70);
            this.lblEstadisticas.Name = "lblEstadisticas";
            this.lblEstadisticas.Size = new System.Drawing.Size(260, 15);
            this.lblEstadisticas.TabIndex = 4;
            this.lblEstadisticas.Text = "Completadas: 0 | En Progreso: 0 | Sin Iniciar: 0";
            // 
            // progressBarAvance
            // 
            this.progressBarAvance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarAvance.Location = new System.Drawing.Point(20, 40);
            this.progressBarAvance.Name = "progressBarAvance";
            this.progressBarAvance.Size = new System.Drawing.Size(1160, 20);
            this.progressBarAvance.TabIndex = 3;
            // 
            // lblAvanceGeneral
            // 
            this.lblAvanceGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAvanceGeneral.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblAvanceGeneral.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.lblAvanceGeneral.Location = new System.Drawing.Point(900, 12);
            this.lblAvanceGeneral.Name = "lblAvanceGeneral";
            this.lblAvanceGeneral.Size = new System.Drawing.Size(280, 20);
            this.lblAvanceGeneral.TabIndex = 2;
            this.lblAvanceGeneral.Text = "Avance General: 0.0%";
            this.lblAvanceGeneral.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblTotalEjecutado
            // 
            this.lblTotalEjecutado.AutoSize = true;
            this.lblTotalEjecutado.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalEjecutado.Location = new System.Drawing.Point(380, 13);
            this.lblTotalEjecutado.Name = "lblTotalEjecutado";
            this.lblTotalEjecutado.Size = new System.Drawing.Size(155, 19);
            this.lblTotalEjecutado.TabIndex = 1;
            this.lblTotalEjecutado.Text = "Total Ejecutado: $0.00";
            // 
            // lblTotalPresupuestado
            // 
            this.lblTotalPresupuestado.AutoSize = true;
            this.lblTotalPresupuestado.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalPresupuestado.Location = new System.Drawing.Point(20, 13);
            this.lblTotalPresupuestado.Name = "lblTotalPresupuestado";
            this.lblTotalPresupuestado.Size = new System.Drawing.Size(196, 19);
            this.lblTotalPresupuestado.TabIndex = 0;
            this.lblTotalPresupuestado.Text = "Total Presupuestado: $0.00";
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 80);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.olvAvance);
            this.splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(10, 5, 5, 5);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.pictureBoxGrafica);
            this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(5, 5, 10, 5);
            this.splitContainer.Size = new System.Drawing.Size(1200, 540);
            this.splitContainer.SplitterDistance = 700;
            this.splitContainer.TabIndex = 2;
            // 
            // olvAvance
            // 
            this.olvAvance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvAvance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.olvAvance.FullRowSelect = true;
            this.olvAvance.HideSelection = false;
            this.olvAvance.Location = new System.Drawing.Point(10, 5);
            this.olvAvance.Name = "olvAvance";
            this.olvAvance.ShowGroups = false;
            this.olvAvance.Size = new System.Drawing.Size(685, 530);
            this.olvAvance.TabIndex = 0;
            this.olvAvance.UseCompatibleStateImageBehavior = false;
            this.olvAvance.View = System.Windows.Forms.View.Details;
            this.olvAvance.VirtualMode = true;
            // 
            // pictureBoxGrafica
            // 
            this.pictureBoxGrafica.BackColor = System.Drawing.Color.White;
            this.pictureBoxGrafica.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxGrafica.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxGrafica.Location = new System.Drawing.Point(5, 5);
            this.pictureBoxGrafica.Name = "pictureBoxGrafica";
            this.pictureBoxGrafica.Size = new System.Drawing.Size(481, 530);
            this.pictureBoxGrafica.TabIndex = 0;
            this.pictureBoxGrafica.TabStop = false;
            // 
            // FormHardProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1200, 720);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "FormHardProgress";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Avance Hard Progress - Sistema Pilaris";
            this.Resize += new System.EventHandler(this.FormHardProgress_Resize);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvAvance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGrafica)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnExportarPDF;
        private System.Windows.Forms.Button btnCargarAvance;
        private System.Windows.Forms.ComboBox cmbLote;
        private System.Windows.Forms.ComboBox cmbManzana;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblEstadisticas;
        private System.Windows.Forms.ProgressBar progressBarAvance;
        private System.Windows.Forms.Label lblAvanceGeneral;
        private System.Windows.Forms.Label lblTotalEjecutado;
        private System.Windows.Forms.Label lblTotalPresupuestado;
        private System.Windows.Forms.SplitContainer splitContainer;
        private DynamicSepticSystem.SafeTreeListView olvAvance;
        private System.Windows.Forms.PictureBox pictureBoxGrafica;
    }
}
