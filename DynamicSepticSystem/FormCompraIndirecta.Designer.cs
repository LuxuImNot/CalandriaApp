namespace DynamicSepticSystem
{
    partial class FormCompraIndirecta
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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblInstrucciones = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.panelBusqueda = new System.Windows.Forms.Panel();
            this.btnAgregarInsumo = new System.Windows.Forms.Button();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.lblCatalogo = new System.Windows.Forms.Label();
            this.olvCatalogo = new BrightIdeasSoftware.FastObjectListView();
            this.lblCarrito = new System.Windows.Forms.Label();
            this.olvCarrito = new BrightIdeasSoftware.FastObjectListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTelefonoProveedor = new System.Windows.Forms.Label();
            this.lblDireccionProveedor = new System.Windows.Forms.Label();
            this.lblRFCProveedor = new System.Windows.Forms.Label();
            this.lblNombreProveedor = new System.Windows.Forms.Label();
            this.btnAgregarProveedor = new System.Windows.Forms.Button();
            this.txtClaveProveedor = new System.Windows.Forms.ComboBox();
            this.lblClaveProveedor = new System.Windows.Forms.Label();
            this.btnGenerarOrden = new System.Windows.Forms.Button();
            this.btnVerRepositorio = new System.Windows.Forms.Button();
            this.panelHeader.SuspendLayout();
            this.panelBusqueda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvCarrito)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(108)))), ((int)(((byte)(46)))));
            this.panelHeader.Controls.Add(this.lblInstrucciones);
            this.panelHeader.Controls.Add(this.lblTitulo);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelHeader.Size = new System.Drawing.Size(1085, 80);
            this.panelHeader.TabIndex = 0;
            // 
            // lblInstrucciones
            // 
            this.lblInstrucciones.AutoSize = true;
            this.lblInstrucciones.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstrucciones.ForeColor = System.Drawing.Color.White;
            this.lblInstrucciones.Location = new System.Drawing.Point(15, 50);
            this.lblInstrucciones.Name = "lblInstrucciones";
            this.lblInstrucciones.Size = new System.Drawing.Size(673, 15);
            this.lblInstrucciones.TabIndex = 1;
            this.lblInstrucciones.Text = "Busca y selecciona insumos del catálogo (doble clic para agregar al carrito). Edi" +
    "ta cantidades y costos directamente en el carrito.";
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(12, 13);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(380, 32);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "ORDEN DE COMPRA INDIRECTA";
            // 
            // panelBusqueda
            // 
            this.panelBusqueda.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBusqueda.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.panelBusqueda.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBusqueda.Controls.Add(this.btnAgregarInsumo);
            this.panelBusqueda.Controls.Add(this.txtBuscar);
            this.panelBusqueda.Controls.Add(this.lblBuscar);
            this.panelBusqueda.Location = new System.Drawing.Point(17, 95);
            this.panelBusqueda.Name = "panelBusqueda";
            this.panelBusqueda.Padding = new System.Windows.Forms.Padding(10);
            this.panelBusqueda.Size = new System.Drawing.Size(1056, 50);
            this.panelBusqueda.TabIndex = 1;
            // 
            // btnAgregarInsumo
            // 
            this.btnAgregarInsumo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAgregarInsumo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAgregarInsumo.FlatAppearance.BorderSize = 0;
            this.btnAgregarInsumo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarInsumo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregarInsumo.ForeColor = System.Drawing.Color.White;
            this.btnAgregarInsumo.Location = new System.Drawing.Point(820, 10);
            this.btnAgregarInsumo.Name = "btnAgregarInsumo";
            this.btnAgregarInsumo.Size = new System.Drawing.Size(220, 28);
            this.btnAgregarInsumo.TabIndex = 2;
            this.btnAgregarInsumo.Text = "? AGREGAR NUEVO INSUMO";
            this.btnAgregarInsumo.UseVisualStyleBackColor = false;
            this.btnAgregarInsumo.Click += new System.EventHandler(this.btnAgregarInsumo_Click);
            // 
            // txtBuscar
            // 
            this.txtBuscar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBuscar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBuscar.Location = new System.Drawing.Point(260, 12);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(350, 25);
            this.txtBuscar.TabIndex = 1;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            // 
            // lblBuscar
            // 
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBuscar.Location = new System.Drawing.Point(13, 15);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new System.Drawing.Size(219, 19);
            this.lblBuscar.TabIndex = 0;
            this.lblBuscar.Text = "?? BUSCAR (Clave/Descripción):";
            // 
            // lblCatalogo
            // 
            this.lblCatalogo.AutoSize = true;
            this.lblCatalogo.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCatalogo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(53)))), ((int)(((byte)(23)))));
            this.lblCatalogo.Location = new System.Drawing.Point(13, 155);
            this.lblCatalogo.Name = "lblCatalogo";
            this.lblCatalogo.Size = new System.Drawing.Size(348, 20);
            this.lblCatalogo.TabIndex = 2;
            this.lblCatalogo.Text = "?? CATÁLOGO DE INSUMOS (Doble clic ? Carrito)";
            // 
            // olvCatalogo
            // 
            this.olvCatalogo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvCatalogo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.olvCatalogo.HideSelection = false;
            this.olvCatalogo.Location = new System.Drawing.Point(17, 178);
            this.olvCatalogo.Name = "olvCatalogo";
            this.olvCatalogo.ShowGroups = false;
            this.olvCatalogo.Size = new System.Drawing.Size(1056, 200);
            this.olvCatalogo.TabIndex = 3;
            this.olvCatalogo.UseCompatibleStateImageBehavior = false;
            this.olvCatalogo.View = System.Windows.Forms.View.Details;
            this.olvCatalogo.VirtualMode = true;
            this.olvCatalogo.DoubleClick += new System.EventHandler(this.olvCatalogo_DoubleClick);
            // 
            // lblCarrito
            // 
            this.lblCarrito.AutoSize = true;
            this.lblCarrito.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCarrito.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(53)))), ((int)(((byte)(23)))));
            this.lblCarrito.Location = new System.Drawing.Point(13, 390);
            this.lblCarrito.Name = "lblCarrito";
            this.lblCarrito.Size = new System.Drawing.Size(516, 20);
            this.lblCarrito.TabIndex = 4;
            this.lblCarrito.Text = "?? CARRITO DE COMPRA (Edita cantidades/costos | Doble clic ? Eliminar)";
            // 
            // olvCarrito
            // 
            this.olvCarrito.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvCarrito.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.olvCarrito.HideSelection = false;
            this.olvCarrito.Location = new System.Drawing.Point(17, 413);
            this.olvCarrito.Name = "olvCarrito";
            this.olvCarrito.ShowGroups = false;
            this.olvCarrito.Size = new System.Drawing.Size(1056, 180);
            this.olvCarrito.TabIndex = 5;
            this.olvCarrito.UseCompatibleStateImageBehavior = false;
            this.olvCarrito.View = System.Windows.Forms.View.Details;
            this.olvCarrito.VirtualMode = true;
            this.olvCarrito.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.olvCarrito_MouseDoubleClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.lblTelefonoProveedor);
            this.groupBox1.Controls.Add(this.lblDireccionProveedor);
            this.groupBox1.Controls.Add(this.lblRFCProveedor);
            this.groupBox1.Controls.Add(this.lblNombreProveedor);
            this.groupBox1.Controls.Add(this.btnAgregarProveedor);
            this.groupBox1.Controls.Add(this.txtClaveProveedor);
            this.groupBox1.Controls.Add(this.lblClaveProveedor);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(108)))), ((int)(((byte)(46)))));
            this.groupBox1.Location = new System.Drawing.Point(17, 599);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(10);
            this.groupBox1.Size = new System.Drawing.Size(580, 210);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "📋 INFORMACIÓN DEL PROVEEDOR";
            // 
            // lblTelefonoProveedor
            // 
            this.lblTelefonoProveedor.AutoSize = true;
            this.lblTelefonoProveedor.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTelefonoProveedor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblTelefonoProveedor.Location = new System.Drawing.Point(10, 135);
            this.lblTelefonoProveedor.Name = "lblTelefonoProveedor";
            this.lblTelefonoProveedor.Size = new System.Drawing.Size(27, 15);
            this.lblTelefonoProveedor.TabIndex = 6;
            this.lblTelefonoProveedor.Text = "Tel: ";
            // 
            // lblDireccionProveedor
            // 
            this.lblDireccionProveedor.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDireccionProveedor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblDireccionProveedor.Location = new System.Drawing.Point(13, 89);
            this.lblDireccionProveedor.Name = "lblDireccionProveedor";
            this.lblDireccionProveedor.Size = new System.Drawing.Size(550, 46);
            this.lblDireccionProveedor.TabIndex = 5;
            this.lblDireccionProveedor.Text = "Dirección: ";
            // 
            // lblRFCProveedor
            // 
            this.lblRFCProveedor.AutoSize = true;
            this.lblRFCProveedor.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRFCProveedor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblRFCProveedor.Location = new System.Drawing.Point(13, 74);
            this.lblRFCProveedor.Name = "lblRFCProveedor";
            this.lblRFCProveedor.Size = new System.Drawing.Size(34, 15);
            this.lblRFCProveedor.TabIndex = 4;
            this.lblRFCProveedor.Text = "RFC: ";
            // 
            // lblNombreProveedor
            // 
            this.lblNombreProveedor.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombreProveedor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblNombreProveedor.Location = new System.Drawing.Point(13, 56);
            this.lblNombreProveedor.Name = "lblNombreProveedor";
            this.lblNombreProveedor.Size = new System.Drawing.Size(550, 18);
            this.lblNombreProveedor.TabIndex = 3;
            this.lblNombreProveedor.Text = "Nombre: ";
            // 
            // btnAgregarProveedor
            // 
            this.btnAgregarProveedor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnAgregarProveedor.FlatAppearance.BorderSize = 0;
            this.btnAgregarProveedor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarProveedor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAgregarProveedor.ForeColor = System.Drawing.Color.White;
            this.btnAgregarProveedor.Location = new System.Drawing.Point(10, 179);
            this.btnAgregarProveedor.Name = "btnAgregarProveedor";
            this.btnAgregarProveedor.Size = new System.Drawing.Size(550, 30);
            this.btnAgregarProveedor.TabIndex = 2;
            this.btnAgregarProveedor.Text = "➕ AGREGAR NUEVO PROVEEDOR";
            this.btnAgregarProveedor.UseVisualStyleBackColor = false;
            this.btnAgregarProveedor.Click += new System.EventHandler(this.btnAgregarProveedor_Click);
            // 
            // txtClaveProveedor
            // 
            this.txtClaveProveedor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.txtClaveProveedor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtClaveProveedor.FormattingEnabled = true;
            this.txtClaveProveedor.Location = new System.Drawing.Point(135, 30);
            this.txtClaveProveedor.Name = "txtClaveProveedor";
            this.txtClaveProveedor.Size = new System.Drawing.Size(250, 23);
            this.txtClaveProveedor.TabIndex = 1;
            this.txtClaveProveedor.SelectedIndexChanged += new System.EventHandler(this.txtClaveProveedor_SelectedIndexChanged);
            // 
            // lblClaveProveedor
            // 
            this.lblClaveProveedor.AutoSize = true;
            this.lblClaveProveedor.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblClaveProveedor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(53)))), ((int)(((byte)(23)))));
            this.lblClaveProveedor.Location = new System.Drawing.Point(15, 32);
            this.lblClaveProveedor.Name = "lblClaveProveedor";
            this.lblClaveProveedor.Size = new System.Drawing.Size(118, 17);
            this.lblClaveProveedor.TabIndex = 0;
            this.lblClaveProveedor.Text = "Seleccionar Clave:";
            // 
            // btnGenerarOrden
            // 
            this.btnGenerarOrden.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerarOrden.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnGenerarOrden.FlatAppearance.BorderSize = 0;
            this.btnGenerarOrden.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerarOrden.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerarOrden.ForeColor = System.Drawing.Color.White;
            this.btnGenerarOrden.Location = new System.Drawing.Point(795, 709);
            this.btnGenerarOrden.Name = "btnGenerarOrden";
            this.btnGenerarOrden.Size = new System.Drawing.Size(278, 80);
            this.btnGenerarOrden.TabIndex = 7;
            this.btnGenerarOrden.Text = "✅ GENERAR ORDEN";
            this.btnGenerarOrden.UseVisualStyleBackColor = false;
            this.btnGenerarOrden.Click += new System.EventHandler(this.btnGenerarOrden_Click);
            // 
            // btnVerRepositorio
            // 
            this.btnVerRepositorio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVerRepositorio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnVerRepositorio.FlatAppearance.BorderSize = 0;
            this.btnVerRepositorio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerRepositorio.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerRepositorio.ForeColor = System.Drawing.Color.White;
            this.btnVerRepositorio.Location = new System.Drawing.Point(620, 709);
            this.btnVerRepositorio.Name = "btnVerRepositorio";
            this.btnVerRepositorio.Size = new System.Drawing.Size(165, 80);
            this.btnVerRepositorio.TabIndex = 8;
            this.btnVerRepositorio.Text = "📂 VER\r\nREPOSITORIO";
            this.btnVerRepositorio.UseVisualStyleBackColor = false;
            this.btnVerRepositorio.Click += new System.EventHandler(this.btnVerRepositorio_Click);
            // 
            // FormCompraIndirecta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1085, 820);
            this.Controls.Add(this.btnVerRepositorio);
            this.Controls.Add(this.btnGenerarOrden);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.olvCarrito);
            this.Controls.Add(this.lblCarrito);
            this.Controls.Add(this.olvCatalogo);
            this.Controls.Add(this.lblCatalogo);
            this.Controls.Add(this.panelBusqueda);
            this.Controls.Add(this.panelHeader);
            this.MinimumSize = new System.Drawing.Size(1100, 850);
            this.Name = "FormCompraIndirecta";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Orden de Compra Indirecta - Sistema Pilaris";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelBusqueda.ResumeLayout(false);
            this.panelBusqueda.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvCarrito)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblInstrucciones;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelBusqueda;
        private System.Windows.Forms.Button btnAgregarInsumo;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.Label lblCatalogo;
        private BrightIdeasSoftware.FastObjectListView olvCatalogo;
        private System.Windows.Forms.Label lblCarrito;
        private BrightIdeasSoftware.FastObjectListView olvCarrito;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnAgregarProveedor;
        private System.Windows.Forms.ComboBox txtClaveProveedor;
        private System.Windows.Forms.Label lblClaveProveedor;
        private System.Windows.Forms.Label lblNombreProveedor;
        private System.Windows.Forms.Label lblRFCProveedor;
        private System.Windows.Forms.Label lblDireccionProveedor;
        private System.Windows.Forms.Label lblTelefonoProveedor;
        private System.Windows.Forms.Button btnGenerarOrden;
        private System.Windows.Forms.Button btnVerRepositorio;
    }
}
