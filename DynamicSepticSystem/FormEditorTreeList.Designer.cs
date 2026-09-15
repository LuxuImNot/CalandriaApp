namespace DynamicSepticSystem
{
    partial class FormEditorTreeList
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
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.groupBoxAcciones = new System.Windows.Forms.GroupBox();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.groupBoxColumnas = new System.Windows.Forms.GroupBox();
            this.btnGestionarColumnas = new System.Windows.Forms.Button();
            this.btnGestorInsumos = new System.Windows.Forms.Button();
            this.groupBoxJerarquia = new System.Windows.Forms.GroupBox();
            this.btnConvertirAPadre = new System.Windows.Forms.Button();
            this.btnBajarJerarquia = new System.Windows.Forms.Button();
            this.btnSubirJerarquia = new System.Windows.Forms.Button();
            this.groupBoxOrden = new System.Windows.Forms.GroupBox();
            this.btnBajar = new System.Windows.Forms.Button();
            this.btnSubir = new System.Windows.Forms.Button();
            this.groupBoxVista = new System.Windows.Forms.GroupBox();
            this.btnExpandirTodo = new System.Windows.Forms.Button();
            this.btnColapsarTodo = new System.Windows.Forms.Button();
            this.groupBoxGestion = new System.Windows.Forms.GroupBox();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.btnRenombrar = new System.Windows.Forms.Button();
            this.btnAsignarTipo = new System.Windows.Forms.Button();
            this.btnAgregarHijo = new System.Windows.Forms.Button();
            this.btnAgregarSubPadre = new System.Windows.Forms.Button();
            this.btnAgregarPadre = new System.Windows.Forms.Button();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.panelSelectorRuta = new System.Windows.Forms.Panel();
            this.cboRutas = new System.Windows.Forms.ComboBox();
            this.lblRuta = new System.Windows.Forms.Label();
            this.treeListView = new DynamicSepticSystem.SafeTreeListView();
            this.panelSidebar.SuspendLayout();
            this.groupBoxAcciones.SuspendLayout();
            this.groupBoxColumnas.SuspendLayout();
            this.groupBoxJerarquia.SuspendLayout();
            this.groupBoxOrden.SuspendLayout();
            this.groupBoxVista.SuspendLayout();
            this.groupBoxGestion.SuspendLayout();
            this.panelSelectorRuta.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).BeginInit();
            this.SuspendLayout();
            // 
            // panelSidebar
            // 
            this.panelSidebar.AutoScroll = true;
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelSidebar.Controls.Add(this.groupBoxAcciones);
            this.panelSidebar.Controls.Add(this.groupBoxColumnas);
            this.panelSidebar.Controls.Add(this.groupBoxJerarquia);
            this.panelSidebar.Controls.Add(this.groupBoxVista);
            this.panelSidebar.Controls.Add(this.groupBoxOrden);
            this.panelSidebar.Controls.Add(this.groupBoxGestion);
            this.panelSidebar.Controls.Add(this.panelSelectorRuta);
            this.panelSidebar.Controls.Add(this.lblTitulo);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Location = new System.Drawing.Point(0, 0);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.panelSidebar.Size = new System.Drawing.Size(230, 661);
            this.panelSidebar.TabIndex = 0;
            // 
            // groupBoxAcciones
            // 
            this.groupBoxAcciones.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAcciones.Controls.Add(this.btnCerrar);
            this.groupBoxAcciones.Controls.Add(this.btnGuardar);
            this.groupBoxAcciones.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxAcciones.Location = new System.Drawing.Point(10, 715);
            this.groupBoxAcciones.Name = "groupBoxAcciones";
            this.groupBoxAcciones.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxAcciones.Size = new System.Drawing.Size(210, 100);
            this.groupBoxAcciones.TabIndex = 5;
            this.groupBoxAcciones.TabStop = false;
            this.groupBoxAcciones.Text = "Acciones";
            // 
            // btnCerrar
            // 
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnCerrar.FlatAppearance.BorderSize = 0;
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(108, 28);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(90, 56);
            this.btnCerrar.TabIndex = 1;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(12, 28);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(90, 56);
            this.btnGuardar.TabIndex = 0;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // groupBoxColumnas
            // 
            this.groupBoxColumnas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxColumnas.Controls.Add(this.btnGestorInsumos);
            this.groupBoxColumnas.Controls.Add(this.btnGestionarColumnas);
            this.groupBoxColumnas.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxColumnas.Location = new System.Drawing.Point(10, 610);
            this.groupBoxColumnas.Name = "groupBoxColumnas";
            this.groupBoxColumnas.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxColumnas.Size = new System.Drawing.Size(210, 100);
            this.groupBoxColumnas.TabIndex = 4;
            this.groupBoxColumnas.TabStop = false;
            this.groupBoxColumnas.Text = "Columnas e Insumos";
            // 
            // btnGestionarColumnas
            // 
            this.btnGestionarColumnas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGestionarColumnas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnGestionarColumnas.FlatAppearance.BorderSize = 0;
            this.btnGestionarColumnas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGestionarColumnas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnGestionarColumnas.ForeColor = System.Drawing.Color.White;
            this.btnGestionarColumnas.Location = new System.Drawing.Point(12, 22);
            this.btnGestionarColumnas.Name = "btnGestionarColumnas";
            this.btnGestionarColumnas.Size = new System.Drawing.Size(186, 30);
            this.btnGestionarColumnas.TabIndex = 0;
            this.btnGestionarColumnas.Text = "Gestionar Columnas";
            this.btnGestionarColumnas.UseVisualStyleBackColor = false;
            this.btnGestionarColumnas.Click += new System.EventHandler(this.btnGestionarColumnas_Click);
            //
            // btnGestorInsumos
            //
            this.btnGestorInsumos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGestorInsumos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnGestorInsumos.FlatAppearance.BorderSize = 0;
            this.btnGestorInsumos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGestorInsumos.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnGestorInsumos.ForeColor = System.Drawing.Color.White;
            this.btnGestorInsumos.Location = new System.Drawing.Point(12, 58);
            this.btnGestorInsumos.Name = "btnGestorInsumos";
            this.btnGestorInsumos.Size = new System.Drawing.Size(186, 30);
            this.btnGestorInsumos.TabIndex = 1;
            this.btnGestorInsumos.Text = "Gestor de Insumos";
            this.btnGestorInsumos.UseVisualStyleBackColor = false;
            this.btnGestorInsumos.Click += new System.EventHandler(this.btnGestorInsumos_Click);
            //
            // groupBoxJerarquia
            // 
            this.groupBoxJerarquia.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxJerarquia.Controls.Add(this.btnConvertirAPadre);
            this.groupBoxJerarquia.Controls.Add(this.btnBajarJerarquia);
            this.groupBoxJerarquia.Controls.Add(this.btnSubirJerarquia);
            this.groupBoxJerarquia.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxJerarquia.Location = new System.Drawing.Point(10, 480);
            this.groupBoxJerarquia.Name = "groupBoxJerarquia";
            this.groupBoxJerarquia.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxJerarquia.Size = new System.Drawing.Size(210, 120);
            this.groupBoxJerarquia.TabIndex = 7;
            this.groupBoxJerarquia.TabStop = false;
            this.groupBoxJerarquia.Text = "Jerarqu�a";
            //
            // btnConvertirAPadre
            //
            this.btnConvertirAPadre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConvertirAPadre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(160)))), ((int)(((byte)(133)))));
            this.btnConvertirAPadre.FlatAppearance.BorderSize = 0;
            this.btnConvertirAPadre.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConvertirAPadre.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnConvertirAPadre.ForeColor = System.Drawing.Color.White;
            this.btnConvertirAPadre.Location = new System.Drawing.Point(12, 72);
            this.btnConvertirAPadre.Name = "btnConvertirAPadre";
            this.btnConvertirAPadre.Size = new System.Drawing.Size(186, 38);
            this.btnConvertirAPadre.TabIndex = 2;
            this.btnConvertirAPadre.Text = "Convertir a Padre";
            this.btnConvertirAPadre.UseVisualStyleBackColor = false;
            this.btnConvertirAPadre.Click += new System.EventHandler(this.btnConvertirAPadre_Click);
            //
            // btnSubirJerarquia
            // 
            this.btnSubirJerarquia.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(188)))), ((int)(((byte)(156)))));
            this.btnSubirJerarquia.FlatAppearance.BorderSize = 0;
            this.btnSubirJerarquia.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubirJerarquia.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnSubirJerarquia.ForeColor = System.Drawing.Color.White;
            this.btnSubirJerarquia.Location = new System.Drawing.Point(12, 22);
            this.btnSubirJerarquia.Name = "btnSubirJerarquia";
            this.btnSubirJerarquia.Size = new System.Drawing.Size(90, 44);
            this.btnSubirJerarquia.TabIndex = 0;
            this.btnSubirJerarquia.Text = "Subir";
            this.btnSubirJerarquia.UseVisualStyleBackColor = false;
            this.btnSubirJerarquia.Click += new System.EventHandler(this.btnSubirJerarquia_Click);
            // 
            // btnBajarJerarquia
            // 
            this.btnBajarJerarquia.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(188)))), ((int)(((byte)(156)))));
            this.btnBajarJerarquia.FlatAppearance.BorderSize = 0;
            this.btnBajarJerarquia.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBajarJerarquia.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnBajarJerarquia.ForeColor = System.Drawing.Color.White;
            this.btnBajarJerarquia.Location = new System.Drawing.Point(108, 22);
            this.btnBajarJerarquia.Name = "btnBajarJerarquia";
            this.btnBajarJerarquia.Size = new System.Drawing.Size(90, 44);
            this.btnBajarJerarquia.TabIndex = 1;
            this.btnBajarJerarquia.Text = "Bajar";
            this.btnBajarJerarquia.UseVisualStyleBackColor = false;
            this.btnBajarJerarquia.Click += new System.EventHandler(this.btnBajarJerarquia_Click);
            // 
            // groupBoxOrden
            // 
            this.groupBoxOrden.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOrden.Controls.Add(this.btnBajar);
            this.groupBoxOrden.Controls.Add(this.btnSubir);
            this.groupBoxOrden.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxOrden.Location = new System.Drawing.Point(10, 400);
            this.groupBoxOrden.Name = "groupBoxOrden";
            this.groupBoxOrden.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxOrden.Size = new System.Drawing.Size(210, 70);
            this.groupBoxOrden.TabIndex = 3;
            this.groupBoxOrden.TabStop = false;
            this.groupBoxOrden.Text = "Orden";
            // 
            // btnSubir
            // 
            this.btnSubir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.btnSubir.FlatAppearance.BorderSize = 0;
            this.btnSubir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubir.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnSubir.ForeColor = System.Drawing.Color.White;
            this.btnSubir.Location = new System.Drawing.Point(12, 22);
            this.btnSubir.Name = "btnSubir";
            this.btnSubir.Size = new System.Drawing.Size(90, 40);
            this.btnSubir.TabIndex = 0;
            this.btnSubir.Text = "Subir";
            this.btnSubir.UseVisualStyleBackColor = false;
            this.btnSubir.Click += new System.EventHandler(this.btnSubir_Click);
            // 
            // btnBajar
            // 
            this.btnBajar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.btnBajar.FlatAppearance.BorderSize = 0;
            this.btnBajar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBajar.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnBajar.ForeColor = System.Drawing.Color.White;
            this.btnBajar.Location = new System.Drawing.Point(108, 22);
            this.btnBajar.Name = "btnBajar";
            this.btnBajar.Size = new System.Drawing.Size(90, 40);
            this.btnBajar.TabIndex = 1;
            this.btnBajar.Text = "Bajar";
            this.btnBajar.UseVisualStyleBackColor = false;
            this.btnBajar.Click += new System.EventHandler(this.btnBajar_Click);
            // 
            // groupBoxVista
            // 
            this.groupBoxVista.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxVista.Controls.Add(this.btnExpandirTodo);
            this.groupBoxVista.Controls.Add(this.btnColapsarTodo);
            this.groupBoxVista.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxVista.Location = new System.Drawing.Point(10, 295);
            this.groupBoxVista.Name = "groupBoxVista";
            this.groupBoxVista.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxVista.Size = new System.Drawing.Size(210, 95);
            this.groupBoxVista.TabIndex = 2;
            this.groupBoxVista.TabStop = false;
            this.groupBoxVista.Text = "Vista";
            // 
            // btnExpandirTodo
            // 
            this.btnExpandirTodo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnExpandirTodo.FlatAppearance.BorderSize = 0;
            this.btnExpandirTodo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpandirTodo.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnExpandirTodo.ForeColor = System.Drawing.Color.White;
            this.btnExpandirTodo.Location = new System.Drawing.Point(12, 26);
            this.btnExpandirTodo.Name = "btnExpandirTodo";
            this.btnExpandirTodo.Size = new System.Drawing.Size(90, 60);
            this.btnExpandirTodo.TabIndex = 0;
            this.btnExpandirTodo.Text = "Expandir Todo";
            this.btnExpandirTodo.UseVisualStyleBackColor = false;
            this.btnExpandirTodo.Click += new System.EventHandler(this.btnExpandirTodo_Click);
            // 
            // btnColapsarTodo
            // 
            this.btnColapsarTodo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnColapsarTodo.FlatAppearance.BorderSize = 0;
            this.btnColapsarTodo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnColapsarTodo.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnColapsarTodo.ForeColor = System.Drawing.Color.White;
            this.btnColapsarTodo.Location = new System.Drawing.Point(108, 26);
            this.btnColapsarTodo.Name = "btnColapsarTodo";
            this.btnColapsarTodo.Size = new System.Drawing.Size(90, 60);
            this.btnColapsarTodo.TabIndex = 1;
            this.btnColapsarTodo.Text = "Colapsar Todo";
            this.btnColapsarTodo.UseVisualStyleBackColor = false;
            this.btnColapsarTodo.Click += new System.EventHandler(this.btnColapsarTodo_Click);
            // 
            // groupBoxGestion
            // 
            this.groupBoxGestion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGestion.Controls.Add(this.btnAsignarTipo);
            this.groupBoxGestion.Controls.Add(this.btnEliminar);
            this.groupBoxGestion.Controls.Add(this.btnRenombrar);
            this.groupBoxGestion.Controls.Add(this.btnAgregarHijo);
            this.groupBoxGestion.Controls.Add(this.btnAgregarSubPadre);
            this.groupBoxGestion.Controls.Add(this.btnAgregarPadre);
            this.groupBoxGestion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxGestion.Location = new System.Drawing.Point(10, 120);
            this.groupBoxGestion.Name = "groupBoxGestion";
            this.groupBoxGestion.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxGestion.Size = new System.Drawing.Size(210, 165);
            this.groupBoxGestion.TabIndex = 1;
            this.groupBoxGestion.TabStop = false;
            this.groupBoxGestion.Text = "Gesti�n de Nodos";
            // 
            // btnAsignarTipo
            // 
            this.btnAsignarTipo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAsignarTipo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            this.btnAsignarTipo.FlatAppearance.BorderSize = 0;
            this.btnAsignarTipo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAsignarTipo.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnAsignarTipo.ForeColor = System.Drawing.Color.White;
            this.btnAsignarTipo.Location = new System.Drawing.Point(108, 106);
            this.btnAsignarTipo.Name = "btnAsignarTipo";
            this.btnAsignarTipo.Size = new System.Drawing.Size(90, 38);
            this.btnAsignarTipo.TabIndex = 5;
            this.btnAsignarTipo.Text = "Tipo";
            this.btnAsignarTipo.UseVisualStyleBackColor = false;
            this.btnAsignarTipo.Click += new System.EventHandler(this.btnAsignarTipo_Click);
            // 
            // btnEliminar
            // 
            this.btnEliminar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnEliminar.FlatAppearance.BorderSize = 0;
            this.btnEliminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnEliminar.ForeColor = System.Drawing.Color.White;
            this.btnEliminar.Location = new System.Drawing.Point(12, 106);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(90, 38);
            this.btnEliminar.TabIndex = 4;
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = false;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // btnRenombrar
            // 
            this.btnRenombrar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRenombrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(196)))), ((int)(((byte)(15)))));
            this.btnRenombrar.FlatAppearance.BorderSize = 0;
            this.btnRenombrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRenombrar.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnRenombrar.ForeColor = System.Drawing.Color.White;
            this.btnRenombrar.Location = new System.Drawing.Point(108, 64);
            this.btnRenombrar.Name = "btnRenombrar";
            this.btnRenombrar.Size = new System.Drawing.Size(90, 38);
            this.btnRenombrar.TabIndex = 3;
            this.btnRenombrar.Text = "Renombrar";
            this.btnRenombrar.UseVisualStyleBackColor = false;
            this.btnRenombrar.Click += new System.EventHandler(this.btnRenombrar_Click);
            // 
            // btnAgregarHijo
            // 
            this.btnAgregarHijo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnAgregarHijo.FlatAppearance.BorderSize = 0;
            this.btnAgregarHijo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarHijo.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnAgregarHijo.ForeColor = System.Drawing.Color.White;
            this.btnAgregarHijo.Location = new System.Drawing.Point(12, 64);
            this.btnAgregarHijo.Name = "btnAgregarHijo";
            this.btnAgregarHijo.Size = new System.Drawing.Size(90, 38);
            this.btnAgregarHijo.TabIndex = 2;
            this.btnAgregarHijo.Text = "Hijo";
            this.btnAgregarHijo.UseVisualStyleBackColor = false;
            this.btnAgregarHijo.Click += new System.EventHandler(this.btnAgregarHijo_Click);
            // 
            // btnAgregarSubPadre
            // 
            this.btnAgregarSubPadre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAgregarSubPadre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnAgregarSubPadre.FlatAppearance.BorderSize = 0;
            this.btnAgregarSubPadre.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarSubPadre.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnAgregarSubPadre.ForeColor = System.Drawing.Color.White;
            this.btnAgregarSubPadre.Location = new System.Drawing.Point(108, 22);
            this.btnAgregarSubPadre.Name = "btnAgregarSubPadre";
            this.btnAgregarSubPadre.Size = new System.Drawing.Size(90, 38);
            this.btnAgregarSubPadre.TabIndex = 1;
            this.btnAgregarSubPadre.Text = "Sub-Padre";
            this.btnAgregarSubPadre.UseVisualStyleBackColor = false;
            this.btnAgregarSubPadre.Click += new System.EventHandler(this.btnAgregarSubPadre_Click);
            // 
            // btnAgregarPadre
            // 
            this.btnAgregarPadre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAgregarPadre.FlatAppearance.BorderSize = 0;
            this.btnAgregarPadre.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarPadre.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnAgregarPadre.ForeColor = System.Drawing.Color.White;
            this.btnAgregarPadre.Location = new System.Drawing.Point(12, 22);
            this.btnAgregarPadre.Name = "btnAgregarPadre";
            this.btnAgregarPadre.Size = new System.Drawing.Size(90, 38);
            this.btnAgregarPadre.TabIndex = 0;
            this.btnAgregarPadre.Text = "Padre";
            this.btnAgregarPadre.UseVisualStyleBackColor = false;
            this.btnAgregarPadre.Click += new System.EventHandler(this.btnAgregarPadre_Click);
            // 
            // lblTitulo
            // 
            this.lblTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(53)))), ((int)(((byte)(23)))));
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.lblTitulo.Size = new System.Drawing.Size(230, 50);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Editor de TreeList";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelSelectorRuta
            // 
            this.panelSelectorRuta.BackColor = System.Drawing.Color.White;
            this.panelSelectorRuta.Controls.Add(this.cboRutas);
            this.panelSelectorRuta.Controls.Add(this.lblRuta);
            this.panelSelectorRuta.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSelectorRuta.Location = new System.Drawing.Point(0, 50);
            this.panelSelectorRuta.Name = "panelSelectorRuta";
            this.panelSelectorRuta.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.panelSelectorRuta.Size = new System.Drawing.Size(230, 60);
            this.panelSelectorRuta.TabIndex = 6;
            // 
            // cboRutas
            // 
            this.cboRutas.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cboRutas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRutas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboRutas.FormattingEnabled = true;
            this.cboRutas.Location = new System.Drawing.Point(10, 29);
            this.cboRutas.Name = "cboRutas";
            this.cboRutas.Size = new System.Drawing.Size(210, 23);
            this.cboRutas.TabIndex = 1;
            // 
            // lblRuta
            // 
            this.lblRuta.AutoSize = true;
            this.lblRuta.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRuta.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblRuta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(53)))), ((int)(((byte)(23)))));
            this.lblRuta.Location = new System.Drawing.Point(10, 8);
            this.lblRuta.Name = "lblRuta";
            this.lblRuta.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblRuta.Size = new System.Drawing.Size(104, 17);
            this.lblRuta.TabIndex = 0;
            this.lblRuta.Text = "Seleccionar Ruta:";
            // 
            // treeListView
            // 
            this.treeListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListView.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.treeListView.FullRowSelect = true;
            this.treeListView.HideSelection = false;
            this.treeListView.Location = new System.Drawing.Point(230, 0);
            this.treeListView.Name = "treeListView";
            this.treeListView.ShowGroups = false;
            this.treeListView.Size = new System.Drawing.Size(854, 661);
            this.treeListView.TabIndex = 1;
            this.treeListView.UseCompatibleStateImageBehavior = false;
            this.treeListView.View = System.Windows.Forms.View.Details;
            this.treeListView.VirtualMode = true;
            // 
            // FormEditorTreeList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 661);
            this.Controls.Add(this.treeListView);
            this.Controls.Add(this.panelSidebar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "FormEditorTreeList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Editor de TreeList - Pilaris";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEditorTreeList_FormClosing);
            this.panelSidebar.ResumeLayout(false);
            this.groupBoxAcciones.ResumeLayout(false);
            this.groupBoxColumnas.ResumeLayout(false);
            this.groupBoxJerarquia.ResumeLayout(false);
            this.groupBoxOrden.ResumeLayout(false);
            this.groupBoxVista.ResumeLayout(false);
            this.groupBoxGestion.ResumeLayout(false);
            this.panelSelectorRuta.ResumeLayout(false);
            this.panelSelectorRuta.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSidebar;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelSelectorRuta;
        private System.Windows.Forms.ComboBox cboRutas;
        private System.Windows.Forms.Label lblRuta;
        private System.Windows.Forms.GroupBox groupBoxGestion;
        private System.Windows.Forms.Button btnAgregarPadre;
        private System.Windows.Forms.Button btnAgregarSubPadre;
        private System.Windows.Forms.Button btnAgregarHijo;
        private System.Windows.Forms.Button btnAsignarTipo;
        private System.Windows.Forms.Button btnRenombrar;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.GroupBox groupBoxOrden;
        private System.Windows.Forms.Button btnSubir;
        private System.Windows.Forms.Button btnBajar;
        private System.Windows.Forms.GroupBox groupBoxColumnas;
        private System.Windows.Forms.Button btnGestionarColumnas;
        private System.Windows.Forms.Button btnGestorInsumos;
        private System.Windows.Forms.GroupBox groupBoxAcciones;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCerrar;
        private DynamicSepticSystem.SafeTreeListView treeListView;
        private System.Windows.Forms.GroupBox groupBoxVista;
        private System.Windows.Forms.Button btnExpandirTodo;
        private System.Windows.Forms.Button btnColapsarTodo;
        private System.Windows.Forms.GroupBox groupBoxJerarquia;
        private System.Windows.Forms.Button btnSubirJerarquia;
        private System.Windows.Forms.Button btnBajarJerarquia;
        private System.Windows.Forms.Button btnConvertirAPadre;
    }
}
