namespace DynamicSepticSystem
{
    partial class FormAgregarConcepto
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
            this.lblNombreConcepto = new System.Windows.Forms.Label();
            this.txtNombreConcepto = new System.Windows.Forms.TextBox();
            this.grpPosicion = new System.Windows.Forms.GroupBox();
            this.lblIndicadorPosicion = new System.Windows.Forms.Label();
            this.lstPosicion = new System.Windows.Forms.ListBox();
            this.grpPartidas = new System.Windows.Forms.GroupBox();
            this.lblContadorPartidas = new System.Windows.Forms.Label();
            this.btnEditarPartida = new System.Windows.Forms.Button();
            this.btnEliminarPartida = new System.Windows.Forms.Button();
            this.btnAgregarPartida = new System.Windows.Forms.Button();
            this.dgvPartidas = new System.Windows.Forms.DataGridView();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grpPosicion.SuspendLayout();
            this.grpPartidas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPartidas)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTitulo.Location = new System.Drawing.Point(20, 20);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(256, 25);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "? Agregar Nuevo Concepto";
            // 
            // lblNombreConcepto
            // 
            this.lblNombreConcepto.AutoSize = true;
            this.lblNombreConcepto.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombreConcepto.Location = new System.Drawing.Point(20, 60);
            this.lblNombreConcepto.Name = "lblNombreConcepto";
            this.lblNombreConcepto.Size = new System.Drawing.Size(139, 17);
            this.lblNombreConcepto.TabIndex = 1;
            this.lblNombreConcepto.Text = "Nombre del Concepto:";
            // 
            // txtNombreConcepto
            // 
            this.txtNombreConcepto.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNombreConcepto.Location = new System.Drawing.Point(165, 57);
            this.txtNombreConcepto.Name = "txtNombreConcepto";
            this.txtNombreConcepto.Size = new System.Drawing.Size(635, 25);
            this.txtNombreConcepto.TabIndex = 2;
            // 
            // grpPosicion
            // 
            this.grpPosicion.Controls.Add(this.lblIndicadorPosicion);
            this.grpPosicion.Controls.Add(this.lstPosicion);
            this.grpPosicion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpPosicion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.grpPosicion.Location = new System.Drawing.Point(20, 100);
            this.grpPosicion.Name = "grpPosicion";
            this.grpPosicion.Size = new System.Drawing.Size(350, 430);
            this.grpPosicion.TabIndex = 3;
            this.grpPosicion.TabStop = false;
            this.grpPosicion.Text = "?? Posición de Inserción";
            // 
            // lblIndicadorPosicion
            // 
            this.lblIndicadorPosicion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIndicadorPosicion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblIndicadorPosicion.Location = new System.Drawing.Point(15, 380);
            this.lblIndicadorPosicion.Name = "lblIndicadorPosicion";
            this.lblIndicadorPosicion.Size = new System.Drawing.Size(320, 40);
            this.lblIndicadorPosicion.TabIndex = 1;
            this.lblIndicadorPosicion.Text = "? Se insertará al FINAL de la lista";
            this.lblIndicadorPosicion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lstPosicion
            // 
            this.lstPosicion.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstPosicion.FormattingEnabled = true;
            this.lstPosicion.ItemHeight = 14;
            this.lstPosicion.Location = new System.Drawing.Point(15, 30);
            this.lstPosicion.Name = "lstPosicion";
            this.lstPosicion.Size = new System.Drawing.Size(320, 340);
            this.lstPosicion.TabIndex = 0;
            // 
            // grpPartidas
            // 
            this.grpPartidas.Controls.Add(this.lblContadorPartidas);
            this.grpPartidas.Controls.Add(this.btnEditarPartida);
            this.grpPartidas.Controls.Add(this.btnEliminarPartida);
            this.grpPartidas.Controls.Add(this.btnAgregarPartida);
            this.grpPartidas.Controls.Add(this.dgvPartidas);
            this.grpPartidas.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpPartidas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.grpPartidas.Location = new System.Drawing.Point(390, 100);
            this.grpPartidas.Name = "grpPartidas";
            this.grpPartidas.Size = new System.Drawing.Size(610, 430);
            this.grpPartidas.TabIndex = 4;
            this.grpPartidas.TabStop = false;
            this.grpPartidas.Text = "?? Partidas del Concepto";
            // 
            // lblContadorPartidas
            // 
            this.lblContadorPartidas.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblContadorPartidas.ForeColor = System.Drawing.Color.Gray;
            this.lblContadorPartidas.Location = new System.Drawing.Point(15, 395);
            this.lblContadorPartidas.Name = "lblContadorPartidas";
            this.lblContadorPartidas.Size = new System.Drawing.Size(200, 25);
            this.lblContadorPartidas.TabIndex = 4;
            this.lblContadorPartidas.Text = "Total: 0 partida(s)";
            this.lblContadorPartidas.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnEditarPartida
            // 
            this.btnEditarPartida.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(196)))), ((int)(((byte)(15)))));
            this.btnEditarPartida.FlatAppearance.BorderSize = 0;
            this.btnEditarPartida.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditarPartida.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEditarPartida.ForeColor = System.Drawing.Color.White;
            this.btnEditarPartida.Location = new System.Drawing.Point(340, 385);
            this.btnEditarPartida.Name = "btnEditarPartida";
            this.btnEditarPartida.Size = new System.Drawing.Size(100, 35);
            this.btnEditarPartida.TabIndex = 3;
            this.btnEditarPartida.Text = "?? Editar";
            this.btnEditarPartida.UseVisualStyleBackColor = false;
            this.btnEditarPartida.Click += new System.EventHandler(this.btnEditarPartida_Click);
            // 
            // btnEliminarPartida
            // 
            this.btnEliminarPartida.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnEliminarPartida.FlatAppearance.BorderSize = 0;
            this.btnEliminarPartida.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminarPartida.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEliminarPartida.ForeColor = System.Drawing.Color.White;
            this.btnEliminarPartida.Location = new System.Drawing.Point(490, 385);
            this.btnEliminarPartida.Name = "btnEliminarPartida";
            this.btnEliminarPartida.Size = new System.Drawing.Size(100, 35);
            this.btnEliminarPartida.TabIndex = 2;
            this.btnEliminarPartida.Text = "??? Eliminar";
            this.btnEliminarPartida.UseVisualStyleBackColor = false;
            this.btnEliminarPartida.Click += new System.EventHandler(this.btnEliminarPartida_Click);
            // 
            // btnAgregarPartida
            // 
            this.btnAgregarPartida.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAgregarPartida.FlatAppearance.BorderSize = 0;
            this.btnAgregarPartida.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarPartida.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregarPartida.ForeColor = System.Drawing.Color.White;
            this.btnAgregarPartida.Location = new System.Drawing.Point(230, 385);
            this.btnAgregarPartida.Name = "btnAgregarPartida";
            this.btnAgregarPartida.Size = new System.Drawing.Size(100, 35);
            this.btnAgregarPartida.TabIndex = 1;
            this.btnAgregarPartida.Text = "? Agregar";
            this.btnAgregarPartida.UseVisualStyleBackColor = false;
            this.btnAgregarPartida.Click += new System.EventHandler(this.btnAgregarPartida_Click);
            // 
            // dgvPartidas
            // 
            this.dgvPartidas.AllowUserToAddRows = false;
            this.dgvPartidas.AllowUserToDeleteRows = false;
            this.dgvPartidas.BackgroundColor = System.Drawing.Color.White;
            this.dgvPartidas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvPartidas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPartidas.Location = new System.Drawing.Point(15, 30);
            this.dgvPartidas.Name = "dgvPartidas";
            this.dgvPartidas.ReadOnly = true;
            this.dgvPartidas.RowHeadersVisible = false;
            this.dgvPartidas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPartidas.Size = new System.Drawing.Size(580, 345);
            this.dgvPartidas.TabIndex = 0;
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(820, 550);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(140, 45);
            this.btnGuardar.TabIndex = 5;
            this.btnGuardar.Text = "?? Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(970, 550);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(140, 45);
            this.btnCancelar.TabIndex = 6;
            this.btnCancelar.Text = "? Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // FormAgregarConcepto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1130, 615);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.grpPartidas);
            this.Controls.Add(this.grpPosicion);
            this.Controls.Add(this.txtNombreConcepto);
            this.Controls.Add(this.lblNombreConcepto);
            this.Controls.Add(this.lblTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAgregarConcepto";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Agregar Concepto - Dynamic Septic System";
            this.grpPosicion.ResumeLayout(false);
            this.grpPartidas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPartidas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblNombreConcepto;
        private System.Windows.Forms.TextBox txtNombreConcepto;
        private System.Windows.Forms.GroupBox grpPosicion;
        private System.Windows.Forms.Label lblIndicadorPosicion;
        private System.Windows.Forms.ListBox lstPosicion;
        private System.Windows.Forms.GroupBox grpPartidas;
        private System.Windows.Forms.Label lblContadorPartidas;
        private System.Windows.Forms.Button btnEditarPartida;
        private System.Windows.Forms.Button btnEliminarPartida;
        private System.Windows.Forms.Button btnAgregarPartida;
        private System.Windows.Forms.DataGridView dgvPartidas;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
