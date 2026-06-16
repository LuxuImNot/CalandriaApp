using System;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Di�logo para seleccionar un nivel de nodo
    /// </summary>
    public partial class DialogSeleccionarNivel : Form
    {
        public int NivelSeleccionado { get; private set; }

        public DialogSeleccionarNivel()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
        }

        private void InitializeComponent()
        {
            this.Text = "Seleccionar Nivel";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(350, 180);
            this.Font = new System.Drawing.Font("Segoe UI", 9);

            var lblInstruccion = new Label();
            lblInstruccion.Text = "Selecciona el nivel de tareas a modificar:";
            lblInstruccion.Location = new System.Drawing.Point(15, 15);
            lblInstruccion.Size = new System.Drawing.Size(320, 25);
            lblInstruccion.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);

            var rdoPadres = new RadioButton();
            rdoPadres.Text = "?? Padres (Nivel 0)";
            rdoPadres.Location = new System.Drawing.Point(30, 50);
            rdoPadres.Size = new System.Drawing.Size(200, 25);
            rdoPadres.Tag = 0;
            rdoPadres.Font = new System.Drawing.Font("Segoe UI", 10);

            var rdoSubPadres = new RadioButton();
            rdoSubPadres.Text = "?? Sub-Padres (Nivel 1)";
            rdoSubPadres.Location = new System.Drawing.Point(30, 85);
            rdoSubPadres.Size = new System.Drawing.Size(200, 25);
            rdoSubPadres.Tag = 1;
            rdoSubPadres.Font = new System.Drawing.Font("Segoe UI", 10);
            rdoSubPadres.Checked = true; // Por defecto seleccionar Sub-Padres

            var rdoHijos = new RadioButton();
            rdoHijos.Text = "?? Hijos (Nivel 2)";
            rdoHijos.Location = new System.Drawing.Point(30, 120);
            rdoHijos.Size = new System.Drawing.Size(200, 25);
            rdoHijos.Tag = 2;
            rdoHijos.Font = new System.Drawing.Font("Segoe UI", 10);

            var btnAceptar = new Button();
            btnAceptar.Text = "? Aceptar";
            btnAceptar.Location = new System.Drawing.Point(150, 155);
            btnAceptar.Size = new System.Drawing.Size(90, 32);
            btnAceptar.BackColor = System.Drawing.Color.FromArgb(46, 204, 113);
            btnAceptar.ForeColor = System.Drawing.Color.White;
            btnAceptar.FlatStyle = FlatStyle.Flat;
            btnAceptar.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnAceptar.Click += (s, e) =>
            {
                if (rdoPadres.Checked) NivelSeleccionado = 0;
                else if (rdoSubPadres.Checked) NivelSeleccionado = 1;
                else if (rdoHijos.Checked) NivelSeleccionado = 2;
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            var btnCancelar = new Button();
            btnCancelar.Text = "? Cancelar";
            btnCancelar.Location = new System.Drawing.Point(245, 155);
            btnCancelar.Size = new System.Drawing.Size(90, 32);
            btnCancelar.BackColor = System.Drawing.Color.FromArgb(127, 140, 141);
            btnCancelar.ForeColor = System.Drawing.Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnCancelar.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.Add(lblInstruccion);
            this.Controls.Add(rdoPadres);
            this.Controls.Add(rdoSubPadres);
            this.Controls.Add(rdoHijos);
            this.Controls.Add(btnAceptar);
            this.Controls.Add(btnCancelar);

            this.AcceptButton = btnAceptar;
            this.CancelButton = btnCancelar;
        }
    }
}
