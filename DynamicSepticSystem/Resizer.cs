using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public class FormBase : Form
    {
        public FormBase()
        {
            this.Load += (s, e) => AplicarAnchorAutomatico(this);
        }

        private void AplicarAnchorAutomatico(Control contenedor)
        {
            foreach (Control ctrl in contenedor.Controls)
            {
                ctrl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

                if (ctrl.HasChildren)
                    AplicarAnchorAutomatico(ctrl);
            }
        }
    }

}
