namespace DynamicSepticSystem
{
    partial class FormGestionarPartidas
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
            this.lblTitulo = new System.Windows.Forms.Label();
            this.grpConcepto = new System.Windows.Forms.GroupBox();
            this.cmbConceptos = new System.Windows.Forms.ComboBox();
            this.lblConcepto = new System.Windows.Forms.Label();
            this.grpPartidas = new System.Windows.Forms.GroupBox();
            this.dgvPartidas = new System.Windows.Forms.DataGridView();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.lblContador = new System.Windows.Forms.Label();
            this.btnSubir = new System.Windows.Forms.Button();
            this.btnBajar = new System.Windows.Forms.Button();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.btnEditar = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.panelAcciones = new System.Windows.Forms.Panel();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grpConcepto.SuspendLayout();
            this.grpPartidas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPartidas)).BeginInit();
            this.panelBotones.SuspendLayout();
            this.panelAcciones.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.lblTitulo.Size = new System.Drawing.Size(900, 60);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "\u270F Gestionar Partidas de Conceptos";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpConcepto
            // 
            this.grpConcepto.Controls.Add(this.cmbConceptos);
            this.grpConcepto.Controls.Add(this.lblConcepto);
            this.grpConcepto.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpConcepto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.grpConcepto.Location = new System.Drawing.Point(20, 80);
            this.grpConcepto.Name = "grpConcepto";
            this.grpConcepto.Padding = new System.Windows.Forms.Padding(10);
            this.grpConcepto.Size = new System.Drawing.Size(860, 100);
            this.grpConcepto.TabIndex = 1;
            this.grpConcepto.TabStop = false;
            this.grpConcepto.Text = "\u1F4CD Seleccionar Concepto";
            // 
            // cmbConceptos
            // 
            this.cmbConceptos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConceptos.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbConceptos.FormattingEnabled = true;
            this.cmbConceptos.Location = new System.Drawing.Point(20, 55);
            this.cmbConceptos.Name = "cmbConceptos";
            this.cmbConceptos.Size = new System.Drawing.Size(820, 25);
            this.cmbConceptos.TabIndex = 1;
            this.cmbConceptos.SelectedIndexChanged += new System.EventHandler(this.cmbConceptos_SelectedIndexChanged);
            // 
            // lblConcepto
            // 
            this.lblConcepto.AutoSize = true;
            this.lblConcepto.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblConcepto.ForeColor = System.Drawing.Color.Gray;
            this.lblConcepto.Location = new System.Drawing.Point(20, 30);
            this.lblConcepto.Name = "lblConcepto";
            this.lblConcepto.Size = new System.Drawing.Size(350, 17);
            this.lblConcepto.TabIndex = 0;
            this.lblConcepto.Text = "Selecciona el concepto cuyas partidas deseas gestionar:";
            // 
            // grpPartidas
            // 
            this.grpPartidas.Controls.Add(this.dgvPartidas);
            this.grpPartidas.Controls.Add(this.panelBotones);
            this.grpPartidas.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpPartidas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.grpPartidas.Location = new System.Drawing.Point(20, 200);
            this.grpPartidas.Name = "grpPartidas";
            this.grpPartidas.Padding = new System.Windows.Forms.Padding(10);
            this.grpPartidas.Size = new System.Drawing.Size(860, 420);
            this.grpPartidas.TabIndex = 2;
            this.grpPartidas.TabStop = false;
            this.grpPartidas.Text = "\u1F14CB Partidas del Concepto";
            // 
            // dgvPartidas
            // 
            this.dgvPartidas.AllowUserToAddRows = false;
            this.dgvPartidas.BackgroundColor = System.Drawing.Color.White;
            this.dgvPartidas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvPartidas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPartidas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPartidas.Location = new System.Drawing.Point(10, 26);
            this.dgvPartidas.MultiSelect = false;
            this.dgvPartidas.Name = "dgvPartidas";
            this.dgvPartidas.RowHeadersVisible = false;
            this.dgvPartidas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPartidas.Size = new System.Drawing.Size(840, 334);
            this.dgvPartidas.TabIndex = 0;
            // 
            // panelBotones
            // 
            this.panelBotones.Controls.Add(this.lblContador);
            this.panelBotones.Controls.Add(this.btnSubir);
            this.panelBotones.Controls.Add(this.btnBajar);
            this.panelBotones.Controls.Add(this.btnAgregar);
            this.panelBotones.Controls.Add(this.btnEditar);
            this.panelBotones.Controls.Add(this.btnEliminar);
            this.panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotones.Location = new System.Drawing.Point(10, 360);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Padding = new System.Windows.Forms.Padding(5);
            this.panelBotones.Size = new System.Drawing.Size(840, 50);
            this.panelBotones.TabIndex = 1;
            // 
            // lblContador
            // 
            this.lblContador.AutoSize = true;
            this.lblContador.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblContador.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblContador.Location = new System.Drawing.Point(10, 17);
            this.lblContador.Name = "lblContador";
            this.lblContador.Size = new System.Drawing.Size(100, 17);
            this.lblContador.TabIndex = 0;
            this.lblContador.Text = "Total: 0 partida(s)";
            // 
            // btnSubir
            // 
            this.btnSubir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnSubir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSubir.FlatAppearance.BorderSize = 0;
            this.btnSubir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubir.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSubir.ForeColor = System.Drawing.Color.White;
            this.btnSubir.Location = new System.Drawing.Point(350, 7);
            this.btnSubir.Name = "btnSubir";
            this.btnSubir.Size = new System.Drawing.Size(80, 35);
            this.btnSubir.TabIndex = 4;
            this.btnSubir.Text = "\u25B2 Subir";
            this.btnSubir.UseVisualStyleBackColor = false;
            this.btnSubir.Click += new System.EventHandler(this.btnSubir_Click);
            // 
            // btnBajar
            // 
            this.btnBajar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnBajar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBajar.FlatAppearance.BorderSize = 0;
            this.btnBajar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBajar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnBajar.ForeColor = System.Drawing.Color.White;
            this.btnBajar.Location = new System.Drawing.Point(440, 7);
            this.btnBajar.Name = "btnBajar";
            this.btnBajar.Size = new System.Drawing.Size(80, 35);
            this.btnBajar.TabIndex = 5;
            this.btnBajar.Text = "\u25BC Bajar";
            this.btnBajar.UseVisualStyleBackColor = false;
            this.btnBajar.Click += new System.EventHandler(this.btnBajar_Click);
            // 
            // btnAgregar
            // 
            this.btnAgregar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAgregar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAgregar.FlatAppearance.BorderSize = 0;
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnAgregar.ForeColor = System.Drawing.Color.White;
            this.btnAgregar.Location = new System.Drawing.Point(530, 7);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(100, 35);
            this.btnAgregar.TabIndex = 1;
            this.btnAgregar.Text = "\u2795 Agregar";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // btnEditar
            // 
            this.btnEditar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(196)))), ((int)(((byte)(15)))));
            this.btnEditar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditar.FlatAppearance.BorderSize = 0;
            this.btnEditar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnEditar.ForeColor = System.Drawing.Color.White;
            this.btnEditar.Location = new System.Drawing.Point(640, 7);
            this.btnEditar.Name = "btnEditar";
            this.btnEditar.Size = new System.Drawing.Size(90, 35);
            this.btnEditar.TabIndex = 2;
            this.btnEditar.Text = "\u270F Editar";
            this.btnEditar.UseVisualStyleBackColor = false;
            this.btnEditar.Click += new System.EventHandler(this.btnEditar_Click);
            // 
            // btnEliminar
            // 
            this.btnEliminar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnEliminar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEliminar.FlatAppearance.BorderSize = 0;
            this.btnEliminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnEliminar.ForeColor = System.Drawing.Color.White;
            this.btnEliminar.Location = new System.Drawing.Point(740, 7);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(90, 35);
            this.btnEliminar.TabIndex = 3;
            this.btnEliminar.Text = "\u1F5D1 Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = false;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // panelAcciones
            // 
            this.panelAcciones.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelAcciones.Controls.Add(this.btnGuardar);
            this.panelAcciones.Controls.Add(this.btnCancelar);
            this.panelAcciones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelAcciones.Location = new System.Drawing.Point(0, 640);
            this.panelAcciones.Name = "panelAcciones";
            this.panelAcciones.Padding = new System.Windows.Forms.Padding(20);
            this.panelAcciones.Size = new System.Drawing.Size(900, 80);
            this.panelAcciones.TabIndex = 3;
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(640, 20);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(120, 40);
            this.btnGuardar.TabIndex = 0;
            this.btnGuardar.Text = "\u1F4BE Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(770, 20);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(110, 40);
            this.btnCancelar.TabIndex = 1;
            this.btnCancelar.Text = "\u274C Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // FormGestionarPartidas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 720);
            this.Controls.Add(this.panelAcciones);
            this.Controls.Add(this.grpPartidas);
            this.Controls.Add(this.grpConcepto);
            this.Controls.Add(this.lblTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormGestionarPartidas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gestionar Partidas de Conceptos";
            this.grpConcepto.ResumeLayout(false);
            this.grpConcepto.PerformLayout();
            this.grpPartidas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPartidas)).EndInit();
            this.panelBotones.ResumeLayout(false);
            this.panelBotones.PerformLayout();
            this.panelAcciones.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox grpConcepto;
        private System.Windows.Forms.ComboBox cmbConceptos;
        private System.Windows.Forms.Label lblConcepto;
        private System.Windows.Forms.GroupBox grpPartidas;
        private System.Windows.Forms.DataGridView dgvPartidas;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Label lblContador;
        private System.Windows.Forms.Button btnSubir;
        private System.Windows.Forms.Button btnBajar;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Panel panelAcciones;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
