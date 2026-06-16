namespace DynamicSepticSystem
{
    partial class FormRepositorioValesSalida
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
            this.panelFiltros = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.chkFechaDesde = new System.Windows.Forms.CheckBox();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.chkFechaHasta = new System.Windows.Forms.CheckBox();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.olvVales = new BrightIdeasSoftware.ObjectListView();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.btnVerPDF = new System.Windows.Forms.Button();
            this.btnDetalle = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.btnActualizar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblContador = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelSuperior.SuspendLayout();
            this.panelFiltros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvVales)).BeginInit();
            this.panelBotones.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSuperior
            // 
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
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(1200, 60);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "REPOSITORIO DE VALES DE SALIDA";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelFiltros
            // 
            this.panelFiltros.Controls.Add(this.dtpHasta);
            this.panelFiltros.Controls.Add(this.chkFechaHasta);
            this.panelFiltros.Controls.Add(this.dtpDesde);
            this.panelFiltros.Controls.Add(this.chkFechaDesde);
            this.panelFiltros.Controls.Add(this.txtBuscar);
            this.panelFiltros.Controls.Add(this.label1);
            this.panelFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltros.Location = new System.Drawing.Point(0, 60);
            this.panelFiltros.Name = "panelFiltros";
            this.panelFiltros.Padding = new System.Windows.Forms.Padding(10);
            this.panelFiltros.Size = new System.Drawing.Size(1200, 60);
            this.panelFiltros.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Buscar:";
            // 
            // txtBuscar
            // 
            this.txtBuscar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBuscar.Location = new System.Drawing.Point(67, 19);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(300, 23);
            this.txtBuscar.TabIndex = 1;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            // 
            // chkFechaDesde
            // 
            this.chkFechaDesde.AutoSize = true;
            this.chkFechaDesde.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkFechaDesde.Location = new System.Drawing.Point(390, 21);
            this.chkFechaDesde.Name = "chkFechaDesde";
            this.chkFechaDesde.Size = new System.Drawing.Size(62, 19);
            this.chkFechaDesde.TabIndex = 2;
            this.chkFechaDesde.Text = "Desde:";
            this.chkFechaDesde.UseVisualStyleBackColor = true;
            this.chkFechaDesde.CheckedChanged += new System.EventHandler(this.chkFechaDesde_CheckedChanged);
            // 
            // dtpDesde
            // 
            this.dtpDesde.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDesde.Location = new System.Drawing.Point(458, 19);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.Size = new System.Drawing.Size(120, 23);
            this.dtpDesde.TabIndex = 3;
            this.dtpDesde.ValueChanged += new System.EventHandler(this.dtpDesde_ValueChanged);
            // 
            // chkFechaHasta
            // 
            this.chkFechaHasta.AutoSize = true;
            this.chkFechaHasta.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkFechaHasta.Location = new System.Drawing.Point(600, 21);
            this.chkFechaHasta.Name = "chkFechaHasta";
            this.chkFechaHasta.Size = new System.Drawing.Size(58, 19);
            this.chkFechaHasta.TabIndex = 4;
            this.chkFechaHasta.Text = "Hasta:";
            this.chkFechaHasta.UseVisualStyleBackColor = true;
            this.chkFechaHasta.CheckedChanged += new System.EventHandler(this.chkFechaHasta_CheckedChanged);
            // 
            // dtpHasta
            // 
            this.dtpHasta.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHasta.Location = new System.Drawing.Point(664, 19);
            this.dtpHasta.Name = "dtpHasta";
            this.dtpHasta.Size = new System.Drawing.Size(120, 23);
            this.dtpHasta.TabIndex = 5;
            this.dtpHasta.ValueChanged += new System.EventHandler(this.dtpHasta_ValueChanged);
            // 
            // olvVales
            // 
            this.olvVales.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvVales.FullRowSelect = true;
            this.olvVales.HideSelection = false;
            this.olvVales.Location = new System.Drawing.Point(0, 120);
            this.olvVales.Name = "olvVales";
            this.olvVales.ShowGroups = false;
            this.olvVales.Size = new System.Drawing.Size(1200, 405);
            this.olvVales.TabIndex = 2;
            this.olvVales.UseCompatibleStateImageBehavior = false;
            this.olvVales.View = System.Windows.Forms.View.Details;
            // 
            // panelBotones
            // 
            this.panelBotones.Controls.Add(this.btnCerrar);
            this.panelBotones.Controls.Add(this.btnActualizar);
            this.panelBotones.Controls.Add(this.btnEliminar);
            this.panelBotones.Controls.Add(this.btnDetalle);
            this.panelBotones.Controls.Add(this.btnVerPDF);
            this.panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotones.Location = new System.Drawing.Point(0, 525);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Padding = new System.Windows.Forms.Padding(10);
            this.panelBotones.Size = new System.Drawing.Size(1200, 65);
            this.panelBotones.TabIndex = 3;
            // 
            // btnVerPDF
            // 
            this.btnVerPDF.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVerPDF.Location = new System.Drawing.Point(13, 13);
            this.btnVerPDF.Name = "btnVerPDF";
            this.btnVerPDF.Size = new System.Drawing.Size(150, 40);
            this.btnVerPDF.TabIndex = 0;
            this.btnVerPDF.Text = "?? Ver PDF";
            this.btnVerPDF.UseVisualStyleBackColor = true;
            this.btnVerPDF.Click += new System.EventHandler(this.btnVerPDF_Click);
            // 
            // btnDetalle
            // 
            this.btnDetalle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDetalle.Location = new System.Drawing.Point(169, 13);
            this.btnDetalle.Name = "btnDetalle";
            this.btnDetalle.Size = new System.Drawing.Size(150, 40);
            this.btnDetalle.TabIndex = 1;
            this.btnDetalle.Text = "?? Detalle";
            this.btnDetalle.UseVisualStyleBackColor = true;
            this.btnDetalle.Click += new System.EventHandler(this.btnDetalle_Click);
            // 
            // btnEliminar
            // 
            this.btnEliminar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEliminar.Location = new System.Drawing.Point(325, 13);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(150, 40);
            this.btnEliminar.TabIndex = 2;
            this.btnEliminar.Text = "??? Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = true;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // btnActualizar
            // 
            this.btnActualizar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnActualizar.Location = new System.Drawing.Point(481, 13);
            this.btnActualizar.Name = "btnActualizar";
            this.btnActualizar.Size = new System.Drawing.Size(150, 40);
            this.btnActualizar.TabIndex = 3;
            this.btnActualizar.Text = "?? Actualizar";
            this.btnActualizar.UseVisualStyleBackColor = true;
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.Location = new System.Drawing.Point(1037, 13);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(150, 40);
            this.btnCerrar.TabIndex = 4;
            this.btnCerrar.Text = "? Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblContador});
            this.statusStrip1.Location = new System.Drawing.Point(0, 590);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1200, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblContador
            // 
            this.lblContador.Name = "lblContador";
            this.lblContador.Size = new System.Drawing.Size(103, 17);
            this.lblContador.Text = "Total: 0 vale(s)";
            // 
            // FormRepositorioValesSalida
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 612);
            this.Controls.Add(this.olvVales);
            this.Controls.Add(this.panelBotones);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panelFiltros);
            this.Controls.Add(this.panelSuperior);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormRepositorioValesSalida";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Repositorio de Vales de Salida";
            this.panelSuperior.ResumeLayout(false);
            this.panelFiltros.ResumeLayout(false);
            this.panelFiltros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvVales)).EndInit();
            this.panelBotones.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelSuperior;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelFiltros;
        private System.Windows.Forms.DateTimePicker dtpHasta;
        private System.Windows.Forms.CheckBox chkFechaHasta;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.CheckBox chkFechaDesde;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.Label label1;
        private BrightIdeasSoftware.ObjectListView olvVales;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Button btnActualizar;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Button btnDetalle;
        private System.Windows.Forms.Button btnVerPDF;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblContador;
    }
}
