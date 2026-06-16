namespace DynamicSepticSystem
{
    partial class FormSeleccionarTitulo
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
            this.btnAdministrativa = new System.Windows.Forms.Button();
            this.btnIndirecta = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.lblInstruccion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.Location = new System.Drawing.Point(12, 9);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(267, 21);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "SELECCIONAR TIPO DE ORDEN";
            // 
            // btnAdministrativa
            // 
            this.btnAdministrativa.BackColor = System.Drawing.Color.LightSkyBlue;
            this.btnAdministrativa.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdministrativa.Location = new System.Drawing.Point(16, 65);
            this.btnAdministrativa.Name = "btnAdministrativa";
            this.btnAdministrativa.Size = new System.Drawing.Size(350, 50);
            this.btnAdministrativa.TabIndex = 1;
            this.btnAdministrativa.Text = "ORDEN DE COMPRA ADMINISTRATIVA";
            this.btnAdministrativa.UseVisualStyleBackColor = false;
            this.btnAdministrativa.Click += new System.EventHandler(this.btnAdministrativa_Click);
            // 
            // btnIndirecta
            // 
            this.btnIndirecta.BackColor = System.Drawing.Color.LightGreen;
            this.btnIndirecta.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIndirecta.Location = new System.Drawing.Point(16, 121);
            this.btnIndirecta.Name = "btnIndirecta";
            this.btnIndirecta.Size = new System.Drawing.Size(350, 50);
            this.btnIndirecta.TabIndex = 2;
            this.btnIndirecta.Text = "ORDEN DE COMPRA INDIRECTA";
            this.btnIndirecta.UseVisualStyleBackColor = false;
            this.btnIndirecta.Click += new System.EventHandler(this.btnIndirecta_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.LightCoral;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(116, 187);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(150, 35);
            this.btnCancelar.TabIndex = 3;
            this.btnCancelar.Text = "CANCELAR";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // lblInstruccion
            // 
            this.lblInstruccion.AutoSize = true;
            this.lblInstruccion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruccion.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblInstruccion.Location = new System.Drawing.Point(13, 36);
            this.lblInstruccion.Name = "lblInstruccion";
            this.lblInstruccion.Size = new System.Drawing.Size(314, 15);
            this.lblInstruccion.TabIndex = 4;
            this.lblInstruccion.Text = "Seleccione el tipo de título que aparecerá en el documento:";
            // 
            // FormSeleccionarTitulo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 241);
            this.Controls.Add(this.lblInstruccion);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnIndirecta);
            this.Controls.Add(this.btnAdministrativa);
            this.Controls.Add(this.lblTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSeleccionarTitulo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Seleccionar Título";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Button btnAdministrativa;
        private System.Windows.Forms.Button btnIndirecta;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label lblInstruccion;
    }
}
