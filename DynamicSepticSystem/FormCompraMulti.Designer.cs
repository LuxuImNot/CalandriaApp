namespace DynamicSepticSystem
{
    partial class FormCompraMulti
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
            this.components = new System.ComponentModel.Container();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.panelCasas = new System.Windows.Forms.Panel();
            this.btnEliminarCasa = new System.Windows.Forms.Button();
            this.lstCasas = new System.Windows.Forms.ListBox();
            this.btnAgregarLote = new System.Windows.Forms.Button();
            this.txtLote = new System.Windows.Forms.TextBox();
            this.txtManzana = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblCasasTitle = new System.Windows.Forms.Label();
            this.panelInsumos = new System.Windows.Forms.Panel();
            this.olvCarrito = new BrightIdeasSoftware.FastObjectListView();
            this.label4 = new System.Windows.Forms.Label();
            this.panelBusqueda = new System.Windows.Forms.Panel();
            this.btnAgregarInsumo = new System.Windows.Forms.Button();
            this.btnEliminarInsumo = new System.Windows.Forms.Button();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.olvCatalogo = new BrightIdeasSoftware.FastObjectListView();
            this.contextMenuCatalogo = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editarInsumoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelProveedor = new System.Windows.Forms.Panel();
            this.lblProvTelefono = new System.Windows.Forms.Label();
            this.lblProvDireccion = new System.Windows.Forms.Label();
            this.lblProvRFC = new System.Windows.Forms.Label();
            this.lblProvNombre = new System.Windows.Forms.Label();
            this.btnAgregarProveedor = new System.Windows.Forms.Button();
            this.cmbNombreProveedor = new System.Windows.Forms.ComboBox();
            this.cmbCodigoProveedor = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnGenerarOrden = new System.Windows.Forms.Button();
            this.btnVerRepositorio = new System.Windows.Forms.Button();
            this.panelHeader.SuspendLayout();
            this.panelCasas.SuspendLayout();
            this.panelInsumos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCarrito)).BeginInit();
            this.panelBusqueda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogo)).BeginInit();
            this.contextMenuCatalogo.SuspendLayout();
            this.panelProveedor.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(108)))), ((int)(((byte)(46)))));
            this.panelHeader.Controls.Add(this.lblTitulo);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelHeader.Size = new System.Drawing.Size(1226, 60);
            this.panelHeader.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(12, 13);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(478, 32);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "ORDEN DE COMPRA - MÚLTIPLES CASAS";
            // 
            // panelCasas
            // 
            this.panelCasas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.panelCasas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCasas.Controls.Add(this.btnEliminarCasa);
            this.panelCasas.Controls.Add(this.lstCasas);
            this.panelCasas.Controls.Add(this.btnAgregarLote);
            this.panelCasas.Controls.Add(this.txtLote);
            this.panelCasas.Controls.Add(this.txtManzana);
            this.panelCasas.Controls.Add(this.label2);
            this.panelCasas.Controls.Add(this.label1);
            this.panelCasas.Controls.Add(this.lblCasasTitle);
            this.panelCasas.Location = new System.Drawing.Point(12, 75);
            this.panelCasas.Name = "panelCasas";
            this.panelCasas.Padding = new System.Windows.Forms.Padding(10);
            this.panelCasas.Size = new System.Drawing.Size(360, 370);
            this.panelCasas.TabIndex = 1;
            // 
            // btnEliminarCasa
            // 
            this.btnEliminarCasa.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnEliminarCasa.FlatAppearance.BorderSize = 0;
            this.btnEliminarCasa.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminarCasa.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEliminarCasa.ForeColor = System.Drawing.Color.White;
            this.btnEliminarCasa.Location = new System.Drawing.Point(13, 330);
            this.btnEliminarCasa.Name = "btnEliminarCasa";
            this.btnEliminarCasa.Size = new System.Drawing.Size(332, 28);
            this.btnEliminarCasa.TabIndex = 7;
            this.btnEliminarCasa.Text = "🗑️ ELIMINAR CASA SELECCIONADA";
            this.btnEliminarCasa.UseVisualStyleBackColor = false;
            this.btnEliminarCasa.Click += new System.EventHandler(this.btnEliminarCasa_Click);
            // 
            // lstCasas
            // 
            this.lstCasas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstCasas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstCasas.FormattingEnabled = true;
            this.lstCasas.ItemHeight = 15;
            this.lstCasas.Location = new System.Drawing.Point(13, 135);
            this.lstCasas.Name = "lstCasas";
            this.lstCasas.Size = new System.Drawing.Size(332, 182);
            this.lstCasas.TabIndex = 6;
            // 
            // btnAgregarLote
            // 
            this.btnAgregarLote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAgregarLote.FlatAppearance.BorderSize = 0;
            this.btnAgregarLote.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarLote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAgregarLote.ForeColor = System.Drawing.Color.White;
            this.btnAgregarLote.Location = new System.Drawing.Point(243, 98);
            this.btnAgregarLote.Name = "btnAgregarLote";
            this.btnAgregarLote.Size = new System.Drawing.Size(102, 28);
            this.btnAgregarLote.TabIndex = 5;
            this.btnAgregarLote.Text = "➕ AGREGAR";
            this.btnAgregarLote.UseVisualStyleBackColor = false;
            this.btnAgregarLote.Click += new System.EventHandler(this.btnAgregarLote_Click);
            // 
            // txtLote
            // 
            this.txtLote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLote.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtLote.Location = new System.Drawing.Point(127, 100);
            this.txtLote.Name = "txtLote";
            this.txtLote.Size = new System.Drawing.Size(100, 25);
            this.txtLote.TabIndex = 4;
            // 
            // txtManzana
            // 
            this.txtManzana.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtManzana.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtManzana.Location = new System.Drawing.Point(13, 100);
            this.txtManzana.Name = "txtManzana";
            this.txtManzana.Size = new System.Drawing.Size(100, 25);
            this.txtManzana.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(124, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "LOTE:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(10, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "MANZANA:";
            // 
            // lblCasasTitle
            // 
            this.lblCasasTitle.AutoSize = true;
            this.lblCasasTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCasasTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(53)))), ((int)(((byte)(23)))));
            this.lblCasasTitle.Location = new System.Drawing.Point(9, 10);
            this.lblCasasTitle.Name = "lblCasasTitle";
            this.lblCasasTitle.Size = new System.Drawing.Size(228, 42);
            this.lblCasasTitle.TabIndex = 0;
            this.lblCasasTitle.Text = "🏠 CASAS A INCLUIR\r\n    EN LA ORDEN DE COMPRA";
            // 
            // panelInsumos
            // 
            this.panelInsumos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelInsumos.Controls.Add(this.olvCarrito);
            this.panelInsumos.Controls.Add(this.label4);
            this.panelInsumos.Controls.Add(this.panelBusqueda);
            this.panelInsumos.Controls.Add(this.olvCatalogo);
            this.panelInsumos.Location = new System.Drawing.Point(390, 75);
            this.panelInsumos.Name = "panelInsumos";
            this.panelInsumos.Size = new System.Drawing.Size(824, 598);
            this.panelInsumos.TabIndex = 2;
            // 
            // olvCarrito
            // 
            this.olvCarrito.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvCarrito.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.olvCarrito.HideSelection = false;
            this.olvCarrito.Location = new System.Drawing.Point(3, 31);
            this.olvCarrito.Name = "olvCarrito";
            this.olvCarrito.ShowGroups = false;
            this.olvCarrito.Size = new System.Drawing.Size(818, 250);
            this.olvCarrito.TabIndex = 3;
            this.olvCarrito.UseCompatibleStateImageBehavior = false;
            this.olvCarrito.View = System.Windows.Forms.View.Details;
            this.olvCarrito.VirtualMode = true;
            this.olvCarrito.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.olvCarrito_MouseDoubleClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(53)))), ((int)(((byte)(23)))));
            this.label4.Location = new System.Drawing.Point(3, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(469, 20);
            this.label4.TabIndex = 2;
            this.label4.Text = "🛒 INSUMOS POR CASA (Edita cantidades | Doble clic → Eliminar)";
            // 
            // panelBusqueda
            // 
            this.panelBusqueda.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBusqueda.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.panelBusqueda.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBusqueda.Controls.Add(this.btnAgregarInsumo);
            this.panelBusqueda.Controls.Add(this.btnEliminarInsumo);
            this.panelBusqueda.Controls.Add(this.txtBuscar);
            this.panelBusqueda.Controls.Add(this.lblBuscar);
            this.panelBusqueda.Location = new System.Drawing.Point(3, 295);
            this.panelBusqueda.Name = "panelBusqueda";
            this.panelBusqueda.Padding = new System.Windows.Forms.Padding(10);
            this.panelBusqueda.Size = new System.Drawing.Size(818, 50);
            this.panelBusqueda.TabIndex = 1;
            // 
            // btnAgregarInsumo
            // 
            this.btnAgregarInsumo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAgregarInsumo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAgregarInsumo.FlatAppearance.BorderSize = 0;
            this.btnAgregarInsumo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarInsumo.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnAgregarInsumo.ForeColor = System.Drawing.Color.White;
            this.btnAgregarInsumo.Location = new System.Drawing.Point(730, 10);
            this.btnAgregarInsumo.Name = "btnAgregarInsumo";
            this.btnAgregarInsumo.Size = new System.Drawing.Size(91, 28);
            this.btnAgregarInsumo.TabIndex = 2;
            this.btnAgregarInsumo.Text = "➕ AGREGAR";
            this.btnAgregarInsumo.UseVisualStyleBackColor = false;
            this.btnAgregarInsumo.Click += new System.EventHandler(this.btnAgregarInsumo_Click);
            // 
            // btnEliminarInsumo
            // 
            this.btnEliminarInsumo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEliminarInsumo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnEliminarInsumo.FlatAppearance.BorderSize = 0;
            this.btnEliminarInsumo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminarInsumo.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnEliminarInsumo.ForeColor = System.Drawing.Color.White;
            this.btnEliminarInsumo.Location = new System.Drawing.Point(640, 10);
            this.btnEliminarInsumo.Name = "btnEliminarInsumo";
            this.btnEliminarInsumo.Size = new System.Drawing.Size(84, 28);
            this.btnEliminarInsumo.TabIndex = 3;
            this.btnEliminarInsumo.Text = "🗑 ELIMINAR";
            this.btnEliminarInsumo.UseVisualStyleBackColor = false;
            this.btnEliminarInsumo.Click += new System.EventHandler(this.btnEliminarInsumo_Click);
            // 
            // txtBuscar
            // 
            this.txtBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBuscar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBuscar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBuscar.Location = new System.Drawing.Point(260, 12);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(370, 25);
            this.txtBuscar.TabIndex = 1;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            // 
            // lblBuscar
            // 
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBuscar.Location = new System.Drawing.Point(13, 15);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new System.Drawing.Size(227, 19);
            this.lblBuscar.TabIndex = 0;
            this.lblBuscar.Text = "🔍 BUSCAR (Clave/Descripción):";
            // 
            // olvCatalogo
            // 
            this.olvCatalogo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvCatalogo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.olvCatalogo.HideSelection = false;
            this.olvCatalogo.Location = new System.Drawing.Point(3, 351);
            this.olvCatalogo.Name = "olvCatalogo";
            this.olvCatalogo.ShowGroups = false;
            this.olvCatalogo.Size = new System.Drawing.Size(818, 244);
            this.olvCatalogo.TabIndex = 0;
            this.olvCatalogo.ContextMenuStrip = this.contextMenuCatalogo;
            this.olvCatalogo.UseCompatibleStateImageBehavior = false;
            this.olvCatalogo.View = System.Windows.Forms.View.Details;
            this.olvCatalogo.VirtualMode = true;
            this.olvCatalogo.DoubleClick += new System.EventHandler(this.olvCatalogo_DoubleClick);
            // 
            // contextMenuCatalogo
            // 
            this.contextMenuCatalogo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editarInsumoToolStripMenuItem});
            this.contextMenuCatalogo.Name = "contextMenuCatalogo";
            this.contextMenuCatalogo.Size = new System.Drawing.Size(153, 48);
            // 
            // editarInsumoToolStripMenuItem
            // 
            this.editarInsumoToolStripMenuItem.Name = "editarInsumoToolStripMenuItem";
            this.editarInsumoToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.editarInsumoToolStripMenuItem.Text = "Editar Insumo";
            this.editarInsumoToolStripMenuItem.Click += new System.EventHandler(this.editarInsumoToolStripMenuItem_Click);
            // 
            // panelProveedor
            // 
            this.panelProveedor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelProveedor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.panelProveedor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelProveedor.Controls.Add(this.lblProvTelefono);
            this.panelProveedor.Controls.Add(this.lblProvDireccion);
            this.panelProveedor.Controls.Add(this.lblProvRFC);
            this.panelProveedor.Controls.Add(this.lblProvNombre);
            this.panelProveedor.Controls.Add(this.btnAgregarProveedor);
            this.panelProveedor.Controls.Add(this.cmbNombreProveedor);
            this.panelProveedor.Controls.Add(this.cmbCodigoProveedor);
            this.panelProveedor.Controls.Add(this.label3);
            this.panelProveedor.Location = new System.Drawing.Point(12, 455);
            this.panelProveedor.Name = "panelProveedor";
            this.panelProveedor.Padding = new System.Windows.Forms.Padding(10);
            this.panelProveedor.Size = new System.Drawing.Size(360, 283);
            this.panelProveedor.TabIndex = 3;
            // 
            // lblProvTelefono
            // 
            this.lblProvTelefono.AutoSize = true;
            this.lblProvTelefono.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProvTelefono.Location = new System.Drawing.Point(10, 181);
            this.lblProvTelefono.Name = "lblProvTelefono";
            this.lblProvTelefono.Size = new System.Drawing.Size(27, 13);
            this.lblProvTelefono.TabIndex = 7;
            this.lblProvTelefono.Text = "Tel: ";
            // 
            // lblProvDireccion
            // 
            this.lblProvDireccion.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProvDireccion.Location = new System.Drawing.Point(10, 142);
            this.lblProvDireccion.Name = "lblProvDireccion";
            this.lblProvDireccion.Size = new System.Drawing.Size(332, 39);
            this.lblProvDireccion.TabIndex = 6;
            this.lblProvDireccion.Text = "Dirección: ";
            // 
            // lblProvRFC
            // 
            this.lblProvRFC.AutoSize = true;
            this.lblProvRFC.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProvRFC.Location = new System.Drawing.Point(13, 109);
            this.lblProvRFC.Name = "lblProvRFC";
            this.lblProvRFC.Size = new System.Drawing.Size(33, 13);
            this.lblProvRFC.TabIndex = 5;
            this.lblProvRFC.Text = "RFC: ";
            // 
            // lblProvNombre
            // 
            this.lblProvNombre.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProvNombre.Location = new System.Drawing.Point(13, 80);
            this.lblProvNombre.Name = "lblProvNombre";
            this.lblProvNombre.Size = new System.Drawing.Size(332, 15);
            this.lblProvNombre.TabIndex = 4;
            this.lblProvNombre.Text = "Nombre: ";
            // 
            // btnAgregarProveedor
            // 
            this.btnAgregarProveedor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnAgregarProveedor.FlatAppearance.BorderSize = 0;
            this.btnAgregarProveedor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarProveedor.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnAgregarProveedor.ForeColor = System.Drawing.Color.White;
            this.btnAgregarProveedor.Location = new System.Drawing.Point(13, 241);
            this.btnAgregarProveedor.Name = "btnAgregarProveedor";
            this.btnAgregarProveedor.Size = new System.Drawing.Size(332, 32);
            this.btnAgregarProveedor.TabIndex = 3;
            this.btnAgregarProveedor.Text = "➕ AGREGAR NUEVO PROVEEDOR";
            this.btnAgregarProveedor.UseVisualStyleBackColor = false;
            this.btnAgregarProveedor.Click += new System.EventHandler(this.btnAgregarProveedor_Click);
            // 
            // cmbNombreProveedor
            // 
            this.cmbNombreProveedor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNombreProveedor.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cmbNombreProveedor.FormattingEnabled = true;
            this.cmbNombreProveedor.Location = new System.Drawing.Point(13, 50);
            this.cmbNombreProveedor.Name = "cmbNombreProveedor";
            this.cmbNombreProveedor.Size = new System.Drawing.Size(160, 21);
            this.cmbNombreProveedor.TabIndex = 2;
            // 
            // cmbCodigoProveedor
            // 
            this.cmbCodigoProveedor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCodigoProveedor.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cmbCodigoProveedor.FormattingEnabled = true;
            this.cmbCodigoProveedor.Location = new System.Drawing.Point(185, 50);
            this.cmbCodigoProveedor.Name = "cmbCodigoProveedor";
            this.cmbCodigoProveedor.Size = new System.Drawing.Size(160, 21);
            this.cmbCodigoProveedor.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(53)))), ((int)(((byte)(23)))));
            this.label3.Location = new System.Drawing.Point(9, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(222, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "📋 SELECCIONAR PROVEEDOR";
            // 
            // btnGenerarOrden
            // 
            this.btnGenerarOrden.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerarOrden.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnGenerarOrden.FlatAppearance.BorderSize = 0;
            this.btnGenerarOrden.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerarOrden.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerarOrden.ForeColor = System.Drawing.Color.White;
            this.btnGenerarOrden.Location = new System.Drawing.Point(565, 680);
            this.btnGenerarOrden.Name = "btnGenerarOrden";
            this.btnGenerarOrden.Size = new System.Drawing.Size(649, 60);
            this.btnGenerarOrden.TabIndex = 4;
            this.btnGenerarOrden.Text = "✅ GENERAR ORDEN DE COMPRA MÚLTIPLE";
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
            this.btnVerRepositorio.Location = new System.Drawing.Point(390, 680);
            this.btnVerRepositorio.Name = "btnVerRepositorio";
            this.btnVerRepositorio.Size = new System.Drawing.Size(165, 60);
            this.btnVerRepositorio.TabIndex = 5;
            this.btnVerRepositorio.Text = "📂 VER\r\nREPOSITORIO";
            this.btnVerRepositorio.UseVisualStyleBackColor = false;
            this.btnVerRepositorio.Click += new System.EventHandler(this.btnVerRepositorio_Click);
            // 
            // FormCompraMulti
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1226, 750);
            this.Controls.Add(this.btnVerRepositorio);
            this.Controls.Add(this.btnGenerarOrden);
            this.Controls.Add(this.panelProveedor);
            this.Controls.Add(this.panelInsumos);
            this.Controls.Add(this.panelCasas);
            this.Controls.Add(this.panelHeader);
            this.MinimumSize = new System.Drawing.Size(1240, 780);
            this.Name = "FormCompraMulti";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Orden de Compra Múltiple - Sistema Pilaris";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelCasas.ResumeLayout(false);
            this.panelCasas.PerformLayout();
            this.panelInsumos.ResumeLayout(false);
            this.panelInsumos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCarrito)).EndInit();
            this.panelBusqueda.ResumeLayout(false);
            this.panelBusqueda.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogo)).EndInit();
            this.contextMenuCatalogo.ResumeLayout(false);
            this.panelProveedor.ResumeLayout(false);
            this.panelProveedor.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelCasas;
        private System.Windows.Forms.Button btnEliminarCasa;
        private System.Windows.Forms.ListBox lstCasas;
        private System.Windows.Forms.Button btnAgregarLote;
        private System.Windows.Forms.TextBox txtLote;
        private System.Windows.Forms.TextBox txtManzana;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblCasasTitle;
        private System.Windows.Forms.Panel panelInsumos;
        private BrightIdeasSoftware.FastObjectListView olvCarrito;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panelBusqueda;
        private System.Windows.Forms.Button btnAgregarInsumo;
        private System.Windows.Forms.Button btnEliminarInsumo;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.Label lblBuscar;
        private BrightIdeasSoftware.FastObjectListView olvCatalogo;
        private System.Windows.Forms.ContextMenuStrip contextMenuCatalogo;
        private System.Windows.Forms.ToolStripMenuItem editarInsumoToolStripMenuItem;
        private System.Windows.Forms.Panel panelProveedor;
        private System.Windows.Forms.Label lblProvTelefono;
        private System.Windows.Forms.Label lblProvDireccion;
        private System.Windows.Forms.Label lblProvRFC;
        private System.Windows.Forms.Label lblProvNombre;
        private System.Windows.Forms.Button btnAgregarProveedor;
        private System.Windows.Forms.ComboBox cmbNombreProveedor;
        private System.Windows.Forms.ComboBox cmbCodigoProveedor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnGenerarOrden;
        private System.Windows.Forms.Button btnVerRepositorio;
    }}