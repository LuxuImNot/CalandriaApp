namespace DynamicSepticSystem
{
    partial class FormDetalleValeSalida
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
            this.panelInfo = new System.Windows.Forms.Panel();
            this.lblFolio = new System.Windows.Forms.Label();
            this.lblObra = new System.Windows.Forms.Label();
            this.lblPrototipo = new System.Windows.Forms.Label();
            this.lblFecha = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblSolicitante = new System.Windows.Forms.Label();
            this.lblResidente = new System.Windows.Forms.Label();
            this.lblEncargado = new System.Windows.Forms.Label();
            this.lblEstado = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.olvDetalle = new BrightIdeasSoftware.ObjectListView();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.panelInfo.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvDetalle)).BeginInit();
            this.SuspendLayout();
            // 
            // panelInfo
            // 
            this.panelInfo.Controls.Add(this.lblEstado);
            this.panelInfo.Controls.Add(this.lblEncargado);
            this.panelInfo.Controls.Add(this.lblResidente);
            this.panelInfo.Controls.Add(this.lblSolicitante);
            this.panelInfo.Controls.Add(this.lblTotal);
            this.panelInfo.Controls.Add(this.lblFecha);
            this.panelInfo.Controls.Add(this.lblPrototipo);
            this.panelInfo.Controls.Add(this.lblObra);
            this.panelInfo.Controls.Add(this.lblFolio);
            this.panelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelInfo.Location = new System.Drawing.Point(0, 0);
            this.panelInfo.Name = "panelInfo";
            this.panelInfo.Padding = new System.Windows.Forms.Padding(15);
            this.panelInfo.Size = new System.Drawing.Size(900, 180);
            this.panelInfo.TabIndex = 0;
            // 
            // lblFolio
            // 
            this.lblFolio.AutoSize = true;
            this.lblFolio.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblFolio.Location = new System.Drawing.Point(18, 18);
            this.lblFolio.Name = "lblFolio";
            this.lblFolio.Size = new System.Drawing.Size(47, 21);
            this.lblFolio.TabIndex = 0;
            this.lblFolio.Text = "Folio:";
            // 
            // lblObra
            // 
            this.lblObra.AutoSize = true;
            this.lblObra.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblObra.Location = new System.Drawing.Point(18, 48);
            this.lblObra.Name = "lblObra";
            this.lblObra.Size = new System.Drawing.Size(42, 19);
            this.lblObra.TabIndex = 1;
            this.lblObra.Text = "Obra:";
            // 
            // lblPrototipo
            // 
            this.lblPrototipo.AutoSize = true;
            this.lblPrototipo.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPrototipo.Location = new System.Drawing.Point(18, 73);
            this.lblPrototipo.Name = "lblPrototipo";
            this.lblPrototipo.Size = new System.Drawing.Size(73, 19);
            this.lblPrototipo.TabIndex = 2;
            this.lblPrototipo.Text = "Prototipo:";
            // 
            // lblFecha
            // 
            this.lblFecha.AutoSize = true;
            this.lblFecha.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblFecha.Location = new System.Drawing.Point(18, 98);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Size = new System.Drawing.Size(48, 19);
            this.lblFecha.TabIndex = 3;
            this.lblFecha.Text = "Fecha:";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotal.Location = new System.Drawing.Point(18, 123);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(45, 19);
            this.lblTotal.TabIndex = 4;
            this.lblTotal.Text = "Total:";
            // 
            // lblSolicitante
            // 
            this.lblSolicitante.AutoSize = true;
            this.lblSolicitante.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSolicitante.Location = new System.Drawing.Point(400, 48);
            this.lblSolicitante.Name = "lblSolicitante";
            this.lblSolicitante.Size = new System.Drawing.Size(79, 19);
            this.lblSolicitante.TabIndex = 5;
            this.lblSolicitante.Text = "Solicitante:";
            // 
            // lblResidente
            // 
            this.lblResidente.AutoSize = true;
            this.lblResidente.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblResidente.Location = new System.Drawing.Point(400, 73);
            this.lblResidente.Name = "lblResidente";
            this.lblResidente.Size = new System.Drawing.Size(72, 19);
            this.lblResidente.TabIndex = 6;
            this.lblResidente.Text = "Residente:";
            // 
            // lblEncargado
            // 
            this.lblEncargado.AutoSize = true;
            this.lblEncargado.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblEncargado.Location = new System.Drawing.Point(400, 98);
            this.lblEncargado.Name = "lblEncargado";
            this.lblEncargado.Size = new System.Drawing.Size(81, 19);
            this.lblEncargado.TabIndex = 7;
            this.lblEncargado.Text = "Encargado:";
            // 
            // lblEstado
            // 
            this.lblEstado.AutoSize = true;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblEstado.Location = new System.Drawing.Point(400, 123);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(54, 19);
            this.lblEstado.TabIndex = 8;
            this.lblEstado.Text = "Estado:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.olvDetalle);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(0, 180);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(10);
            this.groupBox1.Size = new System.Drawing.Size(900, 290);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Insumos Incluidos";
            // 
            // olvDetalle
            // 
            this.olvDetalle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvDetalle.FullRowSelect = true;
            this.olvDetalle.HideSelection = false;
            this.olvDetalle.Location = new System.Drawing.Point(10, 26);
            this.olvDetalle.Name = "olvDetalle";
            this.olvDetalle.ShowGroups = false;
            this.olvDetalle.Size = new System.Drawing.Size(880, 254);
            this.olvDetalle.TabIndex = 0;
            this.olvDetalle.UseCompatibleStateImageBehavior = false;
            this.olvDetalle.View = System.Windows.Forms.View.Details;
            // 
            // btnCerrar
            // 
            this.btnCerrar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.Location = new System.Drawing.Point(0, 470);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(900, 40);
            this.btnCerrar.TabIndex = 2;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // FormDetalleValeSalida
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 510);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCerrar);
            this.Controls.Add(this.panelInfo);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormDetalleValeSalida";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Detalle de Vale de Salida";
            this.panelInfo.ResumeLayout(false);
            this.panelInfo.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvDetalle)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Label lblEncargado;
        private System.Windows.Forms.Label lblResidente;
        private System.Windows.Forms.Label lblSolicitante;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.Label lblPrototipo;
        private System.Windows.Forms.Label lblObra;
        private System.Windows.Forms.Label lblFolio;
        private System.Windows.Forms.GroupBox groupBox1;
        private BrightIdeasSoftware.ObjectListView olvDetalle;
        private System.Windows.Forms.Button btnCerrar;
    }
}
