namespace DynamicSepticSystem
{
    partial class FormDetalleOrdenCompra
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
            this.groupBoxInfo = new System.Windows.Forms.GroupBox();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblIVA = new System.Windows.Forms.Label();
            this.lblSubtotal = new System.Windows.Forms.Label();
            this.lblProveedor = new System.Windows.Forms.Label();
            this.lblManzanaLote = new System.Windows.Forms.Label();
            this.lblEstado = new System.Windows.Forms.Label();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.lblFecha = new System.Windows.Forms.Label();
            this.lblTipo = new System.Windows.Forms.Label();
            this.lblFolio = new System.Windows.Forms.Label();
            this.groupBoxCasas = new System.Windows.Forms.GroupBox();
            this.txtCasas = new System.Windows.Forms.TextBox();
            this.olvDetalle = new BrightIdeasSoftware.ObjectListView();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.lblTotalInsumos = new System.Windows.Forms.Label();
            this.panelSuperior.SuspendLayout();
            this.groupBoxInfo.SuspendLayout();
            this.groupBoxCasas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvDetalle)).BeginInit();
            this.panelBotones.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSuperior
            // 
            this.panelSuperior.Controls.Add(this.lblTitulo);
            this.panelSuperior.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSuperior.Location = new System.Drawing.Point(0, 0);
            this.panelSuperior.Name = "panelSuperior";
            this.panelSuperior.Size = new System.Drawing.Size(1000, 50);
            this.panelSuperior.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(1000, 50);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "?? DETALLE DE ORDEN DE COMPRA";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxInfo
            // 
            this.groupBoxInfo.Controls.Add(this.lblTotal);
            this.groupBoxInfo.Controls.Add(this.lblIVA);
            this.groupBoxInfo.Controls.Add(this.lblSubtotal);
            this.groupBoxInfo.Controls.Add(this.lblProveedor);
            this.groupBoxInfo.Controls.Add(this.lblManzanaLote);
            this.groupBoxInfo.Controls.Add(this.lblEstado);
            this.groupBoxInfo.Controls.Add(this.lblUsuario);
            this.groupBoxInfo.Controls.Add(this.lblFecha);
            this.groupBoxInfo.Controls.Add(this.lblTipo);
            this.groupBoxInfo.Controls.Add(this.lblFolio);
            this.groupBoxInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxInfo.Location = new System.Drawing.Point(0, 50);
            this.groupBoxInfo.Name = "groupBoxInfo";
            this.groupBoxInfo.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxInfo.Size = new System.Drawing.Size(1000, 150);
            this.groupBoxInfo.TabIndex = 1;
            this.groupBoxInfo.TabStop = false;
            this.groupBoxInfo.Text = "Información General";
            // 
            // lblFolio
            // 
            this.lblFolio.AutoSize = true;
            this.lblFolio.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFolio.Location = new System.Drawing.Point(13, 30);
            this.lblFolio.Name = "lblFolio";
            this.lblFolio.Size = new System.Drawing.Size(37, 15);
            this.lblFolio.TabIndex = 0;
            this.lblFolio.Text = "Folio:";
            // 
            // lblTipo
            // 
            this.lblTipo.AutoSize = true;
            this.lblTipo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTipo.Location = new System.Drawing.Point(13, 55);
            this.lblTipo.Name = "lblTipo";
            this.lblTipo.Size = new System.Drawing.Size(33, 15);
            this.lblTipo.TabIndex = 1;
            this.lblTipo.Text = "Tipo:";
            // 
            // lblFecha
            // 
            this.lblFecha.AutoSize = true;
            this.lblFecha.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFecha.Location = new System.Drawing.Point(13, 80);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Size = new System.Drawing.Size(41, 15);
            this.lblFecha.TabIndex = 2;
            this.lblFecha.Text = "Fecha:";
            // 
            // lblUsuario
            // 
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblUsuario.Location = new System.Drawing.Point(13, 105);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(50, 15);
            this.lblUsuario.TabIndex = 3;
            this.lblUsuario.Text = "Usuario:";
            // 
            // lblEstado
            // 
            this.lblEstado.AutoSize = true;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstado.Location = new System.Drawing.Point(350, 30);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(45, 15);
            this.lblEstado.TabIndex = 4;
            this.lblEstado.Text = "Estado:";
            // 
            // lblManzanaLote
            // 
            this.lblManzanaLote.AutoSize = true;
            this.lblManzanaLote.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblManzanaLote.Location = new System.Drawing.Point(350, 55);
            this.lblManzanaLote.Name = "lblManzanaLote";
            this.lblManzanaLote.Size = new System.Drawing.Size(94, 15);
            this.lblManzanaLote.TabIndex = 5;
            this.lblManzanaLote.Text = "Manzana/Lote:";
            // 
            // lblProveedor
            // 
            this.lblProveedor.AutoSize = true;
            this.lblProveedor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProveedor.Location = new System.Drawing.Point(350, 80);
            this.lblProveedor.Name = "lblProveedor";
            this.lblProveedor.Size = new System.Drawing.Size(66, 15);
            this.lblProveedor.TabIndex = 6;
            this.lblProveedor.Text = "Proveedor:";
            // 
            // lblSubtotal
            // 
            this.lblSubtotal.AutoSize = true;
            this.lblSubtotal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtotal.Location = new System.Drawing.Point(700, 30);
            this.lblSubtotal.Name = "lblSubtotal";
            this.lblSubtotal.Size = new System.Drawing.Size(58, 15);
            this.lblSubtotal.TabIndex = 7;
            this.lblSubtotal.Text = "Subtotal:";
            // 
            // lblIVA
            // 
            this.lblIVA.AutoSize = true;
            this.lblIVA.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblIVA.Location = new System.Drawing.Point(700, 55);
            this.lblIVA.Name = "lblIVA";
            this.lblIVA.Size = new System.Drawing.Size(29, 15);
            this.lblIVA.TabIndex = 8;
            this.lblIVA.Text = "IVA:";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotal.Location = new System.Drawing.Point(700, 80);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(62, 21);
            this.lblTotal.TabIndex = 9;
            this.lblTotal.Text = "TOTAL:";
            // 
            // groupBoxCasas
            // 
            this.groupBoxCasas.Controls.Add(this.txtCasas);
            this.groupBoxCasas.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxCasas.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxCasas.Location = new System.Drawing.Point(0, 200);
            this.groupBoxCasas.Name = "groupBoxCasas";
            this.groupBoxCasas.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxCasas.Size = new System.Drawing.Size(1000, 100);
            this.groupBoxCasas.TabIndex = 2;
            this.groupBoxCasas.TabStop = false;
            this.groupBoxCasas.Text = "Casas Incluidas";
            // 
            // txtCasas
            // 
            this.txtCasas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCasas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCasas.Location = new System.Drawing.Point(10, 26);
            this.txtCasas.Multiline = true;
            this.txtCasas.Name = "txtCasas";
            this.txtCasas.ReadOnly = true;
            this.txtCasas.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCasas.Size = new System.Drawing.Size(980, 64);
            this.txtCasas.TabIndex = 0;
            // 
            // olvDetalle
            // 
            this.olvDetalle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvDetalle.FullRowSelect = true;
            this.olvDetalle.HideSelection = false;
            this.olvDetalle.Location = new System.Drawing.Point(0, 300);
            this.olvDetalle.Name = "olvDetalle";
            this.olvDetalle.ShowGroups = false;
            this.olvDetalle.Size = new System.Drawing.Size(1000, 255);
            this.olvDetalle.TabIndex = 3;
            this.olvDetalle.UseCompatibleStateImageBehavior = false;
            this.olvDetalle.View = System.Windows.Forms.View.Details;
            // 
            // panelBotones
            // 
            this.panelBotones.Controls.Add(this.lblTotalInsumos);
            this.panelBotones.Controls.Add(this.btnCerrar);
            this.panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotones.Location = new System.Drawing.Point(0, 555);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Padding = new System.Windows.Forms.Padding(10);
            this.panelBotones.Size = new System.Drawing.Size(1000, 65);
            this.panelBotones.TabIndex = 4;
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.Location = new System.Drawing.Point(837, 13);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(150, 40);
            this.btnCerrar.TabIndex = 0;
            this.btnCerrar.Text = "? Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            // 
            // lblTotalInsumos
            // 
            this.lblTotalInsumos.AutoSize = true;
            this.lblTotalInsumos.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalInsumos.Location = new System.Drawing.Point(13, 23);
            this.lblTotalInsumos.Name = "lblTotalInsumos";
            this.lblTotalInsumos.Size = new System.Drawing.Size(141, 19);
            this.lblTotalInsumos.TabIndex = 1;
            this.lblTotalInsumos.Text = "Total de insumos: 0";
            // 
            // FormDetalleOrdenCompra
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 620);
            this.Controls.Add(this.olvDetalle);
            this.Controls.Add(this.panelBotones);
            this.Controls.Add(this.groupBoxCasas);
            this.Controls.Add(this.groupBoxInfo);
            this.Controls.Add(this.panelSuperior);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormDetalleOrdenCompra";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Detalle de Orden de Compra";
            this.panelSuperior.ResumeLayout(false);
            this.groupBoxInfo.ResumeLayout(false);
            this.groupBoxInfo.PerformLayout();
            this.groupBoxCasas.ResumeLayout(false);
            this.groupBoxCasas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvDetalle)).EndInit();
            this.panelBotones.ResumeLayout(false);
            this.panelBotones.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSuperior;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox groupBoxInfo;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblIVA;
        private System.Windows.Forms.Label lblSubtotal;
        private System.Windows.Forms.Label lblProveedor;
        private System.Windows.Forms.Label lblManzanaLote;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.Label lblTipo;
        private System.Windows.Forms.Label lblFolio;
        private System.Windows.Forms.GroupBox groupBoxCasas;
        private System.Windows.Forms.TextBox txtCasas;
        private BrightIdeasSoftware.ObjectListView olvDetalle;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Label lblTotalInsumos;
        private System.Windows.Forms.Button btnCerrar;
    }
}
