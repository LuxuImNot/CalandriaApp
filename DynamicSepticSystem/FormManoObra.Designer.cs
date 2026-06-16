namespace DynamicSepticSystem
{
    partial class FormManoObra
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridView dgvMano;
        private System.Windows.Forms.Button btnPdf;
        private System.Windows.Forms.Button btnOk;

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
            this.dgvMano = new System.Windows.Forms.DataGridView();
            this.btnPdf = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
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
            
            // btnPdf
            // 
            this.btnPdf.Text = "PDF";
            this.btnPdf.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnPdf.Height = 30;
            this.btnPdf.Click += (s, e) => this.btnPdf_Click(s, e);
            
            // btnOk
            // 
            this.btnOk.Text = "Aceptar";
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnOk.Height = 30;
            this.btnOk.Click += (s, e) => this.btnOk_Click(s, e);
            
            // dgvMano
            // 
            this.dgvMano.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMano.Location = new System.Drawing.Point(0, 30);
            this.dgvMano.Name = "dgvMano";
            this.dgvMano.Size = new System.Drawing.Size(600, 340);
            this.dgvMano.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMano.AllowUserToAddRows = false;
            this.dgvMano.RowHeadersVisible = false;
            // 
            // FormManoObra
            // 
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Controls.Add(this.dgvMano);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnPdf);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnClose);
            this.Name = "FormManoObra";
            this.ResumeLayout(false);
        }
    }
}
