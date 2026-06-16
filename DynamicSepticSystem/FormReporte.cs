using System;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormReporte : Form
    {
        public Tarea Tarea { get; private set; }

        public FormReporte()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
        }

        public FormReporte(Tarea tarea) : this()
        {
            Tarea = tarea;
            lblTitle.Text = "Formulario Reporte para: " + Tarea?.Nombre;
            this.Text = "Reporte - " + (Tarea?.Nombre ?? "");
        }
    }
}
