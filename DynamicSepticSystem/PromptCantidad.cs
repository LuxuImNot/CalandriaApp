using System;
using System.Windows.Forms;
using DynamicSepticSystem;

public class PromptCantidad : Form
{
    public string Resultado => txtCantidad.Text;
    private TextBox txtCantidad;

    public PromptCantidad()
    {
        this.Text = "Cantidad de salida";
        this.Size = new System.Drawing.Size(300, 150);
        this.StartPosition = FormStartPosition.CenterParent;

        Label label = new Label { Text = "Cantidad a registrar:", Left = 10, Top = 20, Width = 250 };
        txtCantidad = new TextBox { Left = 10, Top = 50, Width = 250 };

        Button btnOk = new Button { Text = "Aceptar", Left = 100, Width = 80, Top = 80 };
        btnOk.Click += (sender, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

        this.Controls.Add(label);
        this.Controls.Add(txtCantidad);
        this.Controls.Add(btnOk);

        ThemeManager.AplicarTema(this);
    }
}
