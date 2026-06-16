using System;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class PromptJustificacion : Form
    {
        private TextBox txtJustificacion;
        public string Resultado => txtJustificacion.Text;

        public PromptJustificacion()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
        }
    }
}

        
