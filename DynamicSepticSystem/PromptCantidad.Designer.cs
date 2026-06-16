using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class PromptCantidad : Form
    {
        public PromptCantidad()
        {
            InitializeComponent();
        }

        public string Resultado => txtCantidad.Text;

        private TextBox txtCantidad;

        private void InitializeComponent()
        {
            this.txtCantidad = new System.Windows.Forms.TextBox();
            var btnOk = new System.Windows.Forms.Button();
            var lbl = new System.Windows.Forms.Label();

            this.SuspendLayout();

            // Label
            lbl.Text = "Cantidad a registrar:";
            lbl.Location = new System.Drawing.Point(10, 10);
            lbl.Size = new System.Drawing.Size(200, 20);

            // TextBox
            txtCantidad.Location = new System.Drawing.Point(10, 35);
            txtCantidad.Size = new System.Drawing.Size(250, 20);

            // Botón OK
            btnOk.Text = "Aceptar";
            btnOk.Location = new System.Drawing.Point(90, 70);
            btnOk.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            // Ventana
            this.ClientSize = new System.Drawing.Size(280, 110);
            this.Controls.Add(lbl);
            this.Controls.Add(txtCantidad);
            this.Controls.Add(btnOk);
            this.Text = "Cantidad";
            this.StartPosition = FormStartPosition.CenterParent;

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
