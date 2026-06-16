using System;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormAgregarInsumoIndirecto : Form
    {
        public string Clave { get; private set; }
        public string Descripcion { get; private set; }
        public string Unidad { get; private set; }

        public FormAgregarInsumoIndirecto()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string clave = txtClave.Text.Trim();
            string descripcion = txtDescripcion.Text.Trim();
            string unidad = txtUnidad.Text.Trim();

            if (string.IsNullOrEmpty(clave))
            {
                MessageBox.Show("La clave es obligatoria.", "Validaci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClave.Focus();
                return;
            }

            if (string.IsNullOrEmpty(descripcion))
            {
                MessageBox.Show("La descripci�n es obligatoria.", "Validaci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescripcion.Focus();
                return;
            }

            if (string.IsNullOrEmpty(unidad))
            {
                MessageBox.Show("La unidad es obligatoria.", "Validaci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnidad.Focus();
                return;
            }

            // Asegurar que termine con "-A"
            if (!clave.EndsWith("-A"))
            {
                clave += "-A";
            }

            Clave = clave;
            Descripcion = descripcion;
            Unidad = unidad;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
