namespace DynamicSepticSystem
{
    partial class FormReporte
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnClose;

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
            this.SuspendLayout();
            // 
            // lblTitle
            // 
          
            // 
            // btnClose
            // 
           
            // 
            // FormReporte
            // 
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnClose);
            this.Name = "FormReporte";
            this.ResumeLayout(false);
        }
    }
}
