namespace DynamicSepticSystem
{
    partial class FormAvanceConcepto
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
            this.btnAbrirReporte = new System.Windows.Forms.Button();
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
            this.tableLayoutPanelContenido = new System.Windows.Forms.TableLayoutPanel();
            this.panelIzquierdo = new System.Windows.Forms.Panel();
            this.olvAvanceConceptos = new BrightIdeasSoftware.ObjectListView();
            this.lblTituloTabla = new System.Windows.Forms.Label();
            this.panelDerecho = new System.Windows.Forms.Panel();
            this.pictureBoxGrafica = new System.Windows.Forms.PictureBox();
            this.lblTituloGrafica = new System.Windows.Forms.Label();
            this.pictureBoxFotoUltima = new System.Windows.Forms.PictureBox();
            this.lblTituloFoto = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelFiltros.SuspendLayout();
            this.panelTotales.SuspendLayout();
            this.tableLayoutPanelContenido.SuspendLayout();
            this.panelIzquierdo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvAvanceConceptos)).BeginInit();
            this.panelDerecho.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGrafica)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFotoUltima)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.panelTop.Controls.Add(this.lblTitulo);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1400, 70);
            this.panelTop.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(1400, 70);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "? CONSULTA DE AVANCE POR CONCEPTOS";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelFiltros
            // 
            this.panelFiltros.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelFiltros.Controls.Add(this.btnAbrirReporte);
            this.panelFiltros.Controls.Add(this.btnCargarAvance);
            this.panelFiltros.Controls.Add(this.cmbLote);
            this.panelFiltros.Controls.Add(this.lblLote);
            this.panelFiltros.Controls.Add(this.cmbManzana);
            this.panelFiltros.Controls.Add(this.lblManzana);
            this.panelFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltros.Location = new System.Drawing.Point(0, 70);
            this.panelFiltros.Name = "panelFiltros";
            this.panelFiltros.Padding = new System.Windows.Forms.Padding(20, 15, 20, 15);
            this.panelFiltros.Size = new System.Drawing.Size(1400, 80);
            this.panelFiltros.TabIndex = 1;
            // 
            // lblManzana
            // 
            this.lblManzana.AutoSize = true;
            this.lblManzana.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblManzana.Location = new System.Drawing.Point(30, 28);
            this.lblManzana.Name = "lblManzana";
            this.lblManzana.Size = new System.Drawing.Size(76, 19);
            this.lblManzana.TabIndex = 0;
            this.lblManzana.Text = "Manzana:";
            // 
            // cmbManzana
            // 
            this.cmbManzana.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManzana.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbManzana.FormattingEnabled = true;
            this.cmbManzana.Location = new System.Drawing.Point(120, 25);
            this.cmbManzana.Name = "cmbManzana";
            this.cmbManzana.Size = new System.Drawing.Size(150, 25);
            this.cmbManzana.TabIndex = 1;
            this.cmbManzana.SelectedIndexChanged += new System.EventHandler(this.cmbManzana_SelectedIndexChanged);
            // 
            // lblLote
            // 
            this.lblLote.AutoSize = true;
            this.lblLote.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblLote.Location = new System.Drawing.Point(290, 28);
            this.lblLote.Name = "lblLote";
            this.lblLote.Size = new System.Drawing.Size(44, 19);
            this.lblLote.TabIndex = 2;
            this.lblLote.Text = "Lote:";
            // 
            // cmbLote
            // 
            this.cmbLote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLote.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbLote.FormattingEnabled = true;
            this.cmbLote.Location = new System.Drawing.Point(340, 25);
            this.cmbLote.Name = "cmbLote";
            this.cmbLote.Size = new System.Drawing.Size(150, 25);
            this.cmbLote.TabIndex = 3;
            // 
            // btnCargarAvance
            // 
            this.btnCargarAvance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnCargarAvance.FlatAppearance.BorderSize = 0;
            this.btnCargarAvance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarAvance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCargarAvance.ForeColor = System.Drawing.Color.White;
            this.btnCargarAvance.Location = new System.Drawing.Point(520, 18);
            this.btnCargarAvance.Name = "btnCargarAvance";
            this.btnCargarAvance.Size = new System.Drawing.Size(160, 40);
            this.btnCargarAvance.TabIndex = 4;
            this.btnCargarAvance.Text = "? Cargar Avance";
            this.btnCargarAvance.UseVisualStyleBackColor = false;
            this.btnCargarAvance.Click += new System.EventHandler(this.btnCargarAvance_Click);
            // 
            // btnAbrirReporte
            // 
            this.btnAbrirReporte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnAbrirReporte.FlatAppearance.BorderSize = 0;
            this.btnAbrirReporte.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbrirReporte.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAbrirReporte.ForeColor = System.Drawing.Color.White;
            this.btnAbrirReporte.Location = new System.Drawing.Point(700, 18);
            this.btnAbrirReporte.Name = "btnAbrirReporte";
            this.btnAbrirReporte.Size = new System.Drawing.Size(200, 40);
            this.btnAbrirReporte.TabIndex = 5;
            this.btnAbrirReporte.Text = "? Abrir Reporte Completo";
            this.btnAbrirReporte.UseVisualStyleBackColor = false;
            this.btnAbrirReporte.Click += new System.EventHandler(this.btnAbrirReporte_Click);
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
            this.panelTotales.Location = new System.Drawing.Point(0, 150);
            this.panelTotales.Name = "panelTotales";
            this.panelTotales.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.panelTotales.Size = new System.Drawing.Size(1400, 100);
            this.panelTotales.TabIndex = 2;
            // 
            // lblTotalPresupuestado
            // 
            this.lblTotalPresupuestado.AutoSize = true;
            this.lblTotalPresupuestado.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular);
            this.lblTotalPresupuestado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTotalPresupuestado.Location = new System.Drawing.Point(30, 12);
            this.lblTotalPresupuestado.Name = "lblTotalPresupuestado";
            this.lblTotalPresupuestado.Size = new System.Drawing.Size(196, 20);
            this.lblTotalPresupuestado.TabIndex = 0;
            this.lblTotalPresupuestado.Text = "Total Presupuestado: $0.00";
            // 
            // lblTotalEjecutado
            // 
            this.lblTotalEjecutado.AutoSize = true;
            this.lblTotalEjecutado.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular);
            this.lblTotalEjecutado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblTotalEjecutado.Location = new System.Drawing.Point(400, 12);
            this.lblTotalEjecutado.Name = "lblTotalEjecutado";
            this.lblTotalEjecutado.Size = new System.Drawing.Size(158, 20);
            this.lblTotalEjecutado.TabIndex = 1;
            this.lblTotalEjecutado.Text = "Total Ejecutado: $0.00";
            // 
            // lblAvanceGeneral
            // 
            this.lblAvanceGeneral.AutoSize = true;
            this.lblAvanceGeneral.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblAvanceGeneral.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.lblAvanceGeneral.Location = new System.Drawing.Point(30, 42);
            this.lblAvanceGeneral.Name = "lblAvanceGeneral";
            this.lblAvanceGeneral.Size = new System.Drawing.Size(158, 21);
            this.lblAvanceGeneral.TabIndex = 2;
            this.lblAvanceGeneral.Text = "Avance General: 0%";
            // 
            // progressBarAvance
            // 
            this.progressBarAvance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarAvance.Location = new System.Drawing.Point(230, 42);
            this.progressBarAvance.Name = "progressBarAvance";
            this.progressBarAvance.Size = new System.Drawing.Size(600, 25);
            this.progressBarAvance.TabIndex = 4;
            // 
            // lblEstadisticas
            // 
            this.lblEstadisticas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEstadisticas.AutoSize = true;
            this.lblEstadisticas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadisticas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblEstadisticas.Location = new System.Drawing.Point(850, 47);
            this.lblEstadisticas.Name = "lblEstadisticas";
            this.lblEstadisticas.Size = new System.Drawing.Size(250, 15);
            this.lblEstadisticas.TabIndex = 3;
            this.lblEstadisticas.Text = "Completados: 0 | En Progreso: 0 | Sin Iniciar: 0";
            // 
            // tableLayoutPanelContenido
            // 
            this.tableLayoutPanelContenido.ColumnCount = 2;
            this.tableLayoutPanelContenido.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelContenido.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelContenido.Controls.Add(this.panelIzquierdo, 0, 0);
            this.tableLayoutPanelContenido.Controls.Add(this.panelDerecho, 1, 0);
            this.tableLayoutPanelContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelContenido.Location = new System.Drawing.Point(0, 250);
            this.tableLayoutPanelContenido.Name = "tableLayoutPanelContenido";
            this.tableLayoutPanelContenido.Padding = new System.Windows.Forms.Padding(10);
            this.tableLayoutPanelContenido.RowCount = 1;
            this.tableLayoutPanelContenido.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelContenido.Size = new System.Drawing.Size(1400, 550);
            this.tableLayoutPanelContenido.TabIndex = 3;
            // 
            // panelIzquierdo
            // 
            this.panelIzquierdo.BackColor = System.Drawing.Color.White;
            this.panelIzquierdo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelIzquierdo.Controls.Add(this.olvAvanceConceptos);
            this.panelIzquierdo.Controls.Add(this.lblTituloTabla);
            this.panelIzquierdo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelIzquierdo.Location = new System.Drawing.Point(13, 13);
            this.panelIzquierdo.Name = "panelIzquierdo";
            this.panelIzquierdo.Size = new System.Drawing.Size(684, 524);
            this.panelIzquierdo.TabIndex = 0;
            // 
            // lblTituloTabla
            // 
            this.lblTituloTabla.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTituloTabla.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTituloTabla.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTituloTabla.ForeColor = System.Drawing.Color.White;
            this.lblTituloTabla.Location = new System.Drawing.Point(0, 0);
            this.lblTituloTabla.Name = "lblTituloTabla";
            this.lblTituloTabla.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblTituloTabla.Size = new System.Drawing.Size(682, 40);
            this.lblTituloTabla.TabIndex = 0;
            this.lblTituloTabla.Text = "? Conceptos y Partidas";
            this.lblTituloTabla.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // olvAvanceConceptos
            // 
            this.olvAvanceConceptos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvAvanceConceptos.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.olvAvanceConceptos.FullRowSelect = true;
            this.olvAvanceConceptos.HideSelection = false;
            this.olvAvanceConceptos.Location = new System.Drawing.Point(0, 40);
            this.olvAvanceConceptos.Name = "olvAvanceConceptos";
            this.olvAvanceConceptos.ShowGroups = false;
            this.olvAvanceConceptos.Size = new System.Drawing.Size(682, 482);
            this.olvAvanceConceptos.TabIndex = 1;
            this.olvAvanceConceptos.UseCompatibleStateImageBehavior = false;
            this.olvAvanceConceptos.View = System.Windows.Forms.View.Details;
            // 
            // panelDerecho
            // 
            this.panelDerecho.BackColor = System.Drawing.Color.White;
            this.panelDerecho.Controls.Add(this.pictureBoxGrafica);
            this.panelDerecho.Controls.Add(this.lblTituloGrafica);
            this.panelDerecho.Controls.Add(this.pictureBoxFotoUltima);
            this.panelDerecho.Controls.Add(this.lblTituloFoto);
            this.panelDerecho.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDerecho.Location = new System.Drawing.Point(703, 13);
            this.panelDerecho.Name = "panelDerecho";
            this.panelDerecho.Size = new System.Drawing.Size(684, 524);
            this.panelDerecho.TabIndex = 1;
            // 
            // lblTituloFoto
            // 
            this.lblTituloFoto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.lblTituloFoto.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTituloFoto.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTituloFoto.ForeColor = System.Drawing.Color.White;
            this.lblTituloFoto.Location = new System.Drawing.Point(0, 0);
            this.lblTituloFoto.Name = "lblTituloFoto";
            this.lblTituloFoto.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblTituloFoto.Size = new System.Drawing.Size(684, 35);
            this.lblTituloFoto.TabIndex = 0;
            this.lblTituloFoto.Text = "? Última Evidencia Fotográfica";
            this.lblTituloFoto.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBoxFotoUltima
            // 
            this.pictureBoxFotoUltima.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.pictureBoxFotoUltima.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxFotoUltima.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBoxFotoUltima.Location = new System.Drawing.Point(0, 35);
            this.pictureBoxFotoUltima.Name = "pictureBoxFotoUltima";
            this.pictureBoxFotoUltima.Size = new System.Drawing.Size(684, 200);
            this.pictureBoxFotoUltima.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxFotoUltima.TabIndex = 1;
            this.pictureBoxFotoUltima.TabStop = false;
            // 
            // lblTituloGrafica
            // 
            this.lblTituloGrafica.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.lblTituloGrafica.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTituloGrafica.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTituloGrafica.ForeColor = System.Drawing.Color.White;
            this.lblTituloGrafica.Location = new System.Drawing.Point(0, 235);
            this.lblTituloGrafica.Name = "lblTituloGrafica";
            this.lblTituloGrafica.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblTituloGrafica.Size = new System.Drawing.Size(684, 35);
            this.lblTituloGrafica.TabIndex = 2;
            this.lblTituloGrafica.Text = "? Gráfica de Avance";
            this.lblTituloGrafica.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBoxGrafica
            // 
            this.pictureBoxGrafica.BackColor = System.Drawing.Color.White;
            this.pictureBoxGrafica.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxGrafica.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxGrafica.Location = new System.Drawing.Point(0, 270);
            this.pictureBoxGrafica.Name = "pictureBoxGrafica";
            this.pictureBoxGrafica.Size = new System.Drawing.Size(684, 254);
            this.pictureBoxGrafica.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxGrafica.TabIndex = 3;
            this.pictureBoxGrafica.TabStop = false;
            // 
            // FormAvanceConcepto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.tableLayoutPanelContenido);
            this.Controls.Add(this.panelTotales);
            this.Controls.Add(this.panelFiltros);
            this.Controls.Add(this.panelTop);
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Name = "FormAvanceConcepto";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Avance por Conceptos - Consulta";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Resize += new System.EventHandler(this.FormAvanceConcepto_Resize);
            this.panelTop.ResumeLayout(false);
            this.panelFiltros.ResumeLayout(false);
            this.panelFiltros.PerformLayout();
            this.panelTotales.ResumeLayout(false);
            this.panelTotales.PerformLayout();
            this.tableLayoutPanelContenido.ResumeLayout(false);
            this.panelIzquierdo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvAvanceConceptos)).EndInit();
            this.panelDerecho.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGrafica)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFotoUltima)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelFiltros;
        private System.Windows.Forms.Button btnAbrirReporte;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelContenido;
        private System.Windows.Forms.Panel panelIzquierdo;
        private BrightIdeasSoftware.ObjectListView olvAvanceConceptos;
        private System.Windows.Forms.Label lblTituloTabla;
        private System.Windows.Forms.Panel panelDerecho;
        private System.Windows.Forms.PictureBox pictureBoxGrafica;
        private System.Windows.Forms.Label lblTituloGrafica;
        private System.Windows.Forms.PictureBox pictureBoxFotoUltima;
        private System.Windows.Forms.Label lblTituloFoto;
    }
}
