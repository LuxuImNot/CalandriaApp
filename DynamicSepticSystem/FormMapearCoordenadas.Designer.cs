namespace DynamicSepticSystem
{
    partial class FormMapearCoordenadas
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
            this.btnCerrar = new System.Windows.Forms.Button();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.groupBoxCasaActual = new System.Windows.Forms.GroupBox();
            this.btnAsignarCoordenada = new System.Windows.Forms.Button();
            this.lblCoordenadaActual = new System.Windows.Forms.Label();
            this.txtLote = new System.Windows.Forms.TextBox();
            this.txtManzana = new System.Windows.Forms.TextBox();
            this.lblLote = new System.Windows.Forms.Label();
            this.lblManzana = new System.Windows.Forms.Label();
            this.groupBoxLista = new System.Windows.Forms.GroupBox();
            this.listViewCoordenadas = new System.Windows.Forms.ListView();
            this.columnManzana = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnLote = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnEliminarSeleccionado = new System.Windows.Forms.Button();
            this.groupBoxAcciones = new System.Windows.Forms.GroupBox();
            this.btnExportarJSON = new System.Windows.Forms.Button();
            this.btnImportarJSON = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnLimpiarTodo = new System.Windows.Forms.Button();
            this.panelMapa = new System.Windows.Forms.Panel();
            this.pictureBoxMapa = new System.Windows.Forms.PictureBox();
            this.lblInstrucciones = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelTop.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.groupBoxCasaActual.SuspendLayout();
            this.groupBoxLista.SuspendLayout();
            this.groupBoxAcciones.SuspendLayout();
            this.panelMapa.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMapa)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(53)))), ((int)(((byte)(23)))));
            this.panelTop.Controls.Add(this.lblTitulo);
            this.panelTop.Controls.Add(this.btnCerrar);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1200, 60);
            this.panelTop.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(15, 15);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(454, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "??? Mapear Coordenadas del Sembrado";
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnCerrar.FlatAppearance.BorderSize = 0;
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(1120, 12);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(68, 36);
            this.btnCerrar.TabIndex = 1;
            this.btnCerrar.Text = "?";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // panelLeft
            // 
            this.panelLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.panelLeft.Controls.Add(this.groupBoxCasaActual);
            this.panelLeft.Controls.Add(this.groupBoxLista);
            this.panelLeft.Controls.Add(this.groupBoxAcciones);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 60);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(10);
            this.panelLeft.Size = new System.Drawing.Size(350, 640);
            this.panelLeft.TabIndex = 1;
            // 
            // groupBoxCasaActual
            // 
            this.groupBoxCasaActual.Controls.Add(this.btnAsignarCoordenada);
            this.groupBoxCasaActual.Controls.Add(this.lblCoordenadaActual);
            this.groupBoxCasaActual.Controls.Add(this.txtLote);
            this.groupBoxCasaActual.Controls.Add(this.txtManzana);
            this.groupBoxCasaActual.Controls.Add(this.lblLote);
            this.groupBoxCasaActual.Controls.Add(this.lblManzana);
            this.groupBoxCasaActual.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxCasaActual.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxCasaActual.Location = new System.Drawing.Point(10, 10);
            this.groupBoxCasaActual.Name = "groupBoxCasaActual";
            this.groupBoxCasaActual.Size = new System.Drawing.Size(330, 180);
            this.groupBoxCasaActual.TabIndex = 0;
            this.groupBoxCasaActual.TabStop = false;
            this.groupBoxCasaActual.Text = "Casa a Mapear";
            // 
            // btnAsignarCoordenada
            // 
            this.btnAsignarCoordenada.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAsignarCoordenada.Enabled = false;
            this.btnAsignarCoordenada.FlatAppearance.BorderSize = 0;
            this.btnAsignarCoordenada.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAsignarCoordenada.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAsignarCoordenada.ForeColor = System.Drawing.Color.White;
            this.btnAsignarCoordenada.Location = new System.Drawing.Point(20, 130);
            this.btnAsignarCoordenada.Name = "btnAsignarCoordenada";
            this.btnAsignarCoordenada.Size = new System.Drawing.Size(290, 35);
            this.btnAsignarCoordenada.TabIndex = 5;
            this.btnAsignarCoordenada.Text = "?? Asignar Coordenada";
            this.btnAsignarCoordenada.UseVisualStyleBackColor = false;
            this.btnAsignarCoordenada.Click += new System.EventHandler(this.btnAsignarCoordenada_Click);
            // 
            // lblCoordenadaActual
            // 
            this.lblCoordenadaActual.AutoSize = true;
            this.lblCoordenadaActual.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblCoordenadaActual.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblCoordenadaActual.Location = new System.Drawing.Point(20, 105);
            this.lblCoordenadaActual.Name = "lblCoordenadaActual";
            this.lblCoordenadaActual.Size = new System.Drawing.Size(211, 15);
            this.lblCoordenadaActual.TabIndex = 4;
            this.lblCoordenadaActual.Text = "Haz clic en el mapa para seleccionar";
            // 
            // txtLote
            // 
            this.txtLote.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtLote.Location = new System.Drawing.Point(200, 65);
            this.txtLote.Name = "txtLote";
            this.txtLote.Size = new System.Drawing.Size(110, 25);
            this.txtLote.TabIndex = 3;
            this.txtLote.TextChanged += new System.EventHandler(this.txtCasaInfo_TextChanged);
            // 
            // txtManzana
            // 
            this.txtManzana.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtManzana.Location = new System.Drawing.Point(200, 30);
            this.txtManzana.Name = "txtManzana";
            this.txtManzana.Size = new System.Drawing.Size(110, 25);
            this.txtManzana.TabIndex = 1;
            this.txtManzana.TextChanged += new System.EventHandler(this.txtCasaInfo_TextChanged);
            // 
            // lblLote
            // 
            this.lblLote.AutoSize = true;
            this.lblLote.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblLote.Location = new System.Drawing.Point(20, 68);
            this.lblLote.Name = "lblLote";
            this.lblLote.Size = new System.Drawing.Size(36, 17);
            this.lblLote.TabIndex = 2;
            this.lblLote.Text = "Lote:";
            // 
            // lblManzana
            // 
            this.lblManzana.AutoSize = true;
            this.lblManzana.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblManzana.Location = new System.Drawing.Point(20, 33);
            this.lblManzana.Name = "lblManzana";
            this.lblManzana.Size = new System.Drawing.Size(64, 17);
            this.lblManzana.TabIndex = 0;
            this.lblManzana.Text = "Manzana:";
            // 
            // groupBoxLista
            // 
            this.groupBoxLista.Controls.Add(this.listViewCoordenadas);
            this.groupBoxLista.Controls.Add(this.btnEliminarSeleccionado);
            this.groupBoxLista.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxLista.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxLista.Location = new System.Drawing.Point(10, 10);
            this.groupBoxLista.Name = "groupBoxLista";
            this.groupBoxLista.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxLista.Size = new System.Drawing.Size(330, 450);
            this.groupBoxLista.TabIndex = 1;
            this.groupBoxLista.TabStop = false;
            this.groupBoxLista.Text = "Coordenadas Guardadas";
            // 
            // listViewCoordenadas
            // 
            this.listViewCoordenadas.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnManzana,
            this.columnLote,
            this.columnX,
            this.columnY});
            this.listViewCoordenadas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewCoordenadas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.listViewCoordenadas.FullRowSelect = true;
            this.listViewCoordenadas.GridLines = true;
            this.listViewCoordenadas.HideSelection = false;
            this.listViewCoordenadas.Location = new System.Drawing.Point(10, 26);
            this.listViewCoordenadas.Name = "listViewCoordenadas";
            this.listViewCoordenadas.Size = new System.Drawing.Size(310, 374);
            this.listViewCoordenadas.TabIndex = 0;
            this.listViewCoordenadas.UseCompatibleStateImageBehavior = false;
            this.listViewCoordenadas.View = System.Windows.Forms.View.Details;
            this.listViewCoordenadas.SelectedIndexChanged += new System.EventHandler(this.listViewCoordenadas_SelectedIndexChanged);
            // 
            // columnManzana
            // 
            this.columnManzana.Text = "Mz";
            this.columnManzana.Width = 50;
            // 
            // columnLote
            // 
            this.columnLote.Text = "Lote";
            this.columnLote.Width = 50;
            // 
            // columnX
            // 
            this.columnX.Text = "X";
            this.columnX.Width = 100;
            // 
            // columnY
            // 
            this.columnY.Text = "Y";
            this.columnY.Width = 100;
            // 
            // btnEliminarSeleccionado
            // 
            this.btnEliminarSeleccionado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnEliminarSeleccionado.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnEliminarSeleccionado.Enabled = false;
            this.btnEliminarSeleccionado.FlatAppearance.BorderSize = 0;
            this.btnEliminarSeleccionado.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminarSeleccionado.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEliminarSeleccionado.ForeColor = System.Drawing.Color.White;
            this.btnEliminarSeleccionado.Location = new System.Drawing.Point(10, 400);
            this.btnEliminarSeleccionado.Name = "btnEliminarSeleccionado";
            this.btnEliminarSeleccionado.Size = new System.Drawing.Size(310, 40);
            this.btnEliminarSeleccionado.TabIndex = 1;
            this.btnEliminarSeleccionado.Text = "??? Eliminar Seleccionado";
            this.btnEliminarSeleccionado.UseVisualStyleBackColor = false;
            this.btnEliminarSeleccionado.Click += new System.EventHandler(this.btnEliminarSeleccionado_Click);
            // 
            // groupBoxAcciones
            // 
            this.groupBoxAcciones.Controls.Add(this.btnExportarJSON);
            this.groupBoxAcciones.Controls.Add(this.btnImportarJSON);
            this.groupBoxAcciones.Controls.Add(this.btnGuardar);
            this.groupBoxAcciones.Controls.Add(this.btnLimpiarTodo);
            this.groupBoxAcciones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxAcciones.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxAcciones.Location = new System.Drawing.Point(10, 460);
            this.groupBoxAcciones.Name = "groupBoxAcciones";
            this.groupBoxAcciones.Size = new System.Drawing.Size(330, 170);
            this.groupBoxAcciones.TabIndex = 2;
            this.groupBoxAcciones.TabStop = false;
            this.groupBoxAcciones.Text = "Acciones";
            // 
            // btnExportarJSON
            // 
            this.btnExportarJSON.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnExportarJSON.FlatAppearance.BorderSize = 0;
            this.btnExportarJSON.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportarJSON.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportarJSON.ForeColor = System.Drawing.Color.White;
            this.btnExportarJSON.Location = new System.Drawing.Point(170, 70);
            this.btnExportarJSON.Name = "btnExportarJSON";
            this.btnExportarJSON.Size = new System.Drawing.Size(140, 40);
            this.btnExportarJSON.TabIndex = 3;
            this.btnExportarJSON.Text = "?? Exportar JSON";
            this.btnExportarJSON.UseVisualStyleBackColor = false;
            this.btnExportarJSON.Click += new System.EventHandler(this.btnExportarJSON_Click);
            // 
            // btnImportarJSON
            // 
            this.btnImportarJSON.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnImportarJSON.FlatAppearance.BorderSize = 0;
            this.btnImportarJSON.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportarJSON.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnImportarJSON.ForeColor = System.Drawing.Color.White;
            this.btnImportarJSON.Location = new System.Drawing.Point(20, 70);
            this.btnImportarJSON.Name = "btnImportarJSON";
            this.btnImportarJSON.Size = new System.Drawing.Size(140, 40);
            this.btnImportarJSON.TabIndex = 2;
            this.btnImportarJSON.Text = "?? Importar JSON";
            this.btnImportarJSON.UseVisualStyleBackColor = false;
            this.btnImportarJSON.Click += new System.EventHandler(this.btnImportarJSON_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(20, 25);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(290, 40);
            this.btnGuardar.TabIndex = 0;
            this.btnGuardar.Text = "?? Guardar Configuración";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnLimpiarTodo
            // 
            this.btnLimpiarTodo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnLimpiarTodo.FlatAppearance.BorderSize = 0;
            this.btnLimpiarTodo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiarTodo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLimpiarTodo.ForeColor = System.Drawing.Color.White;
            this.btnLimpiarTodo.Location = new System.Drawing.Point(20, 120);
            this.btnLimpiarTodo.Name = "btnLimpiarTodo";
            this.btnLimpiarTodo.Size = new System.Drawing.Size(290, 40);
            this.btnLimpiarTodo.TabIndex = 1;
            this.btnLimpiarTodo.Text = "??? Limpiar Todo";
            this.btnLimpiarTodo.UseVisualStyleBackColor = false;
            this.btnLimpiarTodo.Click += new System.EventHandler(this.btnLimpiarTodo_Click);
            // 
            // panelMapa
            // 
            this.panelMapa.BackColor = System.Drawing.Color.White;
            this.panelMapa.Controls.Add(this.pictureBoxMapa);
            this.panelMapa.Controls.Add(this.lblInstrucciones);
            this.panelMapa.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMapa.Location = new System.Drawing.Point(350, 60);
            this.panelMapa.Name = "panelMapa";
            this.panelMapa.Padding = new System.Windows.Forms.Padding(10);
            this.panelMapa.Size = new System.Drawing.Size(850, 640);
            this.panelMapa.TabIndex = 2;
            // 
            // pictureBoxMapa
            // 
            this.pictureBoxMapa.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.pictureBoxMapa.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxMapa.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBoxMapa.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxMapa.Location = new System.Drawing.Point(10, 10);
            this.pictureBoxMapa.Name = "pictureBoxMapa";
            this.pictureBoxMapa.Size = new System.Drawing.Size(830, 585);
            this.pictureBoxMapa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxMapa.TabIndex = 0;
            this.pictureBoxMapa.TabStop = false;
            this.pictureBoxMapa.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxMapa_Paint);
            this.pictureBoxMapa.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxMapa_MouseClick);
            this.pictureBoxMapa.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxMapa_MouseMove);
            // 
            // lblInstrucciones
            // 
            this.lblInstrucciones.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(220)))));
            this.lblInstrucciones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblInstrucciones.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblInstrucciones.Location = new System.Drawing.Point(10, 595);
            this.lblInstrucciones.Name = "lblInstrucciones";
            this.lblInstrucciones.Padding = new System.Windows.Forms.Padding(10);
            this.lblInstrucciones.Size = new System.Drawing.Size(830, 35);
            this.lblInstrucciones.TabIndex = 1;
            this.lblInstrucciones.Text = "?? Instrucciones: Ingresa Manzana y Lote, luego haz clic en el mapa donde está l" +
    "a casa. Presiona \'Asignar Coordenada\' para guardar.";
            this.lblInstrucciones.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(350, 678);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(850, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(179, 17);
            this.toolStripStatusLabel.Text = "Listo | 0 coordenadas guardadas";
            // 
            // FormMapearCoordenadas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelMapa);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormMapearCoordenadas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mapear Coordenadas - Calandria Residencial";
            this.Load += new System.EventHandler(this.FormMapearCoordenadas_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelLeft.ResumeLayout(false);
            this.groupBoxCasaActual.ResumeLayout(false);
            this.groupBoxCasaActual.PerformLayout();
            this.groupBoxLista.ResumeLayout(false);
            this.groupBoxAcciones.ResumeLayout(false);
            this.panelMapa.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMapa)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.GroupBox groupBoxCasaActual;
        private System.Windows.Forms.TextBox txtManzana;
        private System.Windows.Forms.Label lblManzana;
        private System.Windows.Forms.TextBox txtLote;
        private System.Windows.Forms.Label lblLote;
        private System.Windows.Forms.Label lblCoordenadaActual;
        private System.Windows.Forms.Button btnAsignarCoordenada;
        private System.Windows.Forms.GroupBox groupBoxLista;
        private System.Windows.Forms.ListView listViewCoordenadas;
        private System.Windows.Forms.ColumnHeader columnManzana;
        private System.Windows.Forms.ColumnHeader columnLote;
        private System.Windows.Forms.ColumnHeader columnX;
        private System.Windows.Forms.ColumnHeader columnY;
        private System.Windows.Forms.Button btnEliminarSeleccionado;
        private System.Windows.Forms.GroupBox groupBoxAcciones;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnLimpiarTodo;
        private System.Windows.Forms.Panel panelMapa;
        private System.Windows.Forms.PictureBox pictureBoxMapa;
        private System.Windows.Forms.Label lblInstrucciones;
        private System.Windows.Forms.Button btnExportarJSON;
        private System.Windows.Forms.Button btnImportarJSON;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}
