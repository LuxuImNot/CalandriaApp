namespace DynamicSepticSystem
{
    partial class FormInsumos
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.DataGridView dgvInsumos;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.dgvInsumos = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInsumos)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Height = 30;
            this.lblTitle.Text = "";
            // 
            // btnClose
            // 
            this.btnClose.Text = "Cancelar";
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnClose.Height = 30;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            // 
            // btnOk
            // 
            this.btnOk.Text = "Aceptar";
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnOk.Height = 30;
            this.btnOk.Name = "btnOk";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // dgvInsumos
            // 
            this.dgvInsumos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvInsumos.Location = new System.Drawing.Point(0, 30);
            this.dgvInsumos.Name = "dgvInsumos";
            this.dgvInsumos.Size = new System.Drawing.Size(600, 340);
            this.dgvInsumos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInsumos.AllowUserToAddRows = false;
            this.dgvInsumos.RowHeadersVisible = false;
            // 
            // FormInsumos
            // 
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Controls.Add(this.dgvInsumos);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnClose);
            this.Name = "FormInsumos";
            ((System.ComponentModel.ISupportInitialize)(this.dgvInsumos)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
