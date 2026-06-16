namespace DynamicSepticSystem
{
    partial class FormDetalleEstimacion
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblFolio = new System.Windows.Forms.Label();
            this.lblNumeroEstimacion = new System.Windows.Forms.Label();
            this.panelInfo = new System.Windows.Forms.Panel();
            this.lblFecha = new System.Windows.Forms.Label();
            this.lblManzanaLote = new System.Windows.Forms.Label();
            this.lblProveedor = new System.Windows.Forms.Label();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.olvDetalles = new BrightIdeasSoftware.ObjectListView();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblTotalPartidas = new System.Windows.Forms.Label();
            this.lblTotalMonto = new System.Windows.Forms.Label();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.panelInfo.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvDetalles)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.panelTop.Controls.Add(this.lblFolio);
            this.panelTop.Controls.Add(this.lblNumeroEstimacion);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelTop.Size = new System.Drawing.Size(1100, 60);
            this.panelTop.TabIndex = 0;
            // 
            // lblFolio
            // 
            this.lblFolio.AutoSize = true;
            this.lblFolio.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblFolio.ForeColor = System.Drawing.Color.White;
            this.lblFolio.Location = new System.Drawing.Point(15, 15);
            this.lblFolio.Name = "lblFolio";
            this.lblFolio.Size = new System.Drawing.Size(200, 30);
            this.lblFolio.TabIndex = 0;
            this.lblFolio.Text = "Folio: EST-0001";
            // 
            // lblNumeroEstimacion
            // 
            this.lblNumeroEstimacion.AutoSize = true;
            this.lblNumeroEstimacion.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblNumeroEstimacion.ForeColor = System.Drawing.Color.White;
            this.lblNumeroEstimacion.Location = new System.Drawing.Point(900, 20);
            this.lblNumeroEstimacion.Name = "lblNumeroEstimacion";
            this.lblNumeroEstimacion.Size = new System.Drawing.Size(150, 21);
            this.lblNumeroEstimacion.TabIndex = 1;
            this.lblNumeroEstimacion.Text = "Estimación No. 1";
            // 
            // panelInfo
            // 
            this.panelInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelInfo.Controls.Add(this.lblFecha);
            this.panelInfo.Controls.Add(this.lblManzanaLote);
            this.panelInfo.Controls.Add(this.lblProveedor);
            this.panelInfo.Controls.Add(this.lblDescripcion);
            this.panelInfo.Controls.Add(this.lblTotal);
            this.panelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelInfo.Location = new System.Drawing.Point(0, 60);
            this.panelInfo.Name = "panelInfo";
            this.panelInfo.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelInfo.Size = new System.Drawing.Size(1100, 120);
            this.panelInfo.TabIndex = 1;
            // 
            // lblFecha
            // 
            this.lblFecha.AutoSize = true;
            this.lblFecha.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblFecha.Location = new System.Drawing.Point(15, 15);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Size = new System.Drawing.Size(200, 19);
            this.lblFecha.TabIndex = 0;
            this.lblFecha.Text = "Fecha: 01/01/2024 10:00";
            // 
            // lblManzanaLote
            // 
            this.lblManzanaLote.AutoSize = true;
            this.lblManzanaLote.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblManzanaLote.Location = new System.Drawing.Point(15, 40);
            this.lblManzanaLote.Name = "lblManzanaLote";
            this.lblManzanaLote.Size = new System.Drawing.Size(200, 19);
            this.lblManzanaLote.TabIndex = 1;
            this.lblManzanaLote.Text = "M1 L1 - Prototipo Calandra";
            // 
            // lblProveedor
            // 
            this.lblProveedor.AutoSize = true;
            this.lblProveedor.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblProveedor.Location = new System.Drawing.Point(15, 65);
            this.lblProveedor.Name = "lblProveedor";
            this.lblProveedor.Size = new System.Drawing.Size(250, 19);
            this.lblProveedor.TabIndex = 2;
            this.lblProveedor.Text = "Proveedor: Ignacio Durán León";
            // 
            // lblDescripcion
            // 
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDescripcion.Location = new System.Drawing.Point(15, 90);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(300, 19);
            this.lblDescripcion.TabIndex = 3;
            this.lblDescripcion.Text = "Descripción: Subcontratista Edificación";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.lblTotal.Location = new System.Drawing.Point(850, 40);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(180, 25);
            this.lblTotal.TabIndex = 4;
            this.lblTotal.Text = "Total: $100,000.00";
            // 
            // olvDetalles
            // 
            this.olvDetalles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvDetalles.FullRowSelect = true;
            this.olvDetalles.GridLines = true;
            this.olvDetalles.HideSelection = false;
            this.olvDetalles.Location = new System.Drawing.Point(0, 180);
            this.olvDetalles.Name = "olvDetalles";
            this.olvDetalles.Size = new System.Drawing.Size(1100, 370);
            this.olvDetalles.TabIndex = 2;
            this.olvDetalles.UseCompatibleStateImageBehavior = false;
            this.olvDetalles.View = System.Windows.Forms.View.Details;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelBottom.Controls.Add(this.lblTotalPartidas);
            this.panelBottom.Controls.Add(this.lblTotalMonto);
            this.panelBottom.Controls.Add(this.btnCerrar);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 550);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelBottom.Size = new System.Drawing.Size(1100, 50);
            this.panelBottom.TabIndex = 3;
            // 
            // lblTotalPartidas
            // 
            this.lblTotalPartidas.AutoSize = true;
            this.lblTotalPartidas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTotalPartidas.Location = new System.Drawing.Point(15, 17);
            this.lblTotalPartidas.Name = "lblTotalPartidas";
            this.lblTotalPartidas.Size = new System.Drawing.Size(120, 15);
            this.lblTotalPartidas.TabIndex = 0;
            this.lblTotalPartidas.Text = "Total de partidas: 0";
            // 
            // lblTotalMonto
            // 
            this.lblTotalMonto.AutoSize = true;
            this.lblTotalMonto.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalMonto.Location = new System.Drawing.Point(400, 17);
            this.lblTotalMonto.Name = "lblTotalMonto";
            this.lblTotalMonto.Size = new System.Drawing.Size(150, 15);
            this.lblTotalMonto.TabIndex = 1;
            this.lblTotalMonto.Text = "Total ejecutado: $0.00";
            // 
            // btnCerrar
            // 
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(965, 10);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(120, 30);
            this.btnCerrar.TabIndex = 2;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // FormDetalleEstimacion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Controls.Add(this.olvDetalles);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelInfo);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormDetalleEstimacion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Detalle de Estimación";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelInfo.ResumeLayout(false);
            this.panelInfo.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvDetalles)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblFolio;
        private System.Windows.Forms.Label lblNumeroEstimacion;
        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.Label lblManzanaLote;
        private System.Windows.Forms.Label lblProveedor;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.Label lblTotal;
        private BrightIdeasSoftware.ObjectListView olvDetalles;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblTotalPartidas;
        private System.Windows.Forms.Label lblTotalMonto;
        private System.Windows.Forms.Button btnCerrar;
    }
}
