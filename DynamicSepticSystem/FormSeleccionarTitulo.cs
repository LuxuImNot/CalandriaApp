using System;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormSeleccionarTitulo : Form
    {
        public string TituloSeleccionado { get; private set; }

        public FormSeleccionarTitulo()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
        }

        private void btnAdministrativa_Click(object sender, EventArgs e)
        {
            TituloSeleccionado = "Orden de Compra Administrativa";
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnIndirecta_Click(object sender, EventArgs e)
        {
            TituloSeleccionado = "Orden de Compra Indirecta";
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
