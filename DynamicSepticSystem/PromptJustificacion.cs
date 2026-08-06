using System;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Dialogo modal corporativo para capturar una justificacion obligatoria.
    /// Reemplaza al viejo Microsoft.VisualBasic.Interaction.InputBox (estilo VB6,
    /// con acentos rotos) por una ventana tematica: encabezado por severidad,
    /// area de texto multilinea y validacion (el boton Guardar se habilita solo
    /// cuando hay texto). Las cadenas usan escapes \u para no depender de la
    /// codificacion del archivo.
    /// Uso: string j = PromptJustificacion.Pedir(this, titulo, mensaje, detalle, peligro);
    /// </summary>
    public partial class PromptJustificacion : Form
    {
        private TextBox txtJustificacion;
        private Button btnGuardar;

        /// <summary>Texto capturado, ya recortado.</summary>
        public string Resultado => txtJustificacion.Text.Trim();

        public PromptJustificacion(string titulo, string mensaje, string detalle, bool peligro)
        {
            InitializeComponent();
            ConstruirUi(titulo, mensaje, detalle, peligro);
        }

        /// <summary>
        /// Muestra el dialogo y devuelve la justificacion capturada, o null si el
        /// usuario cancela. Nunca devuelve cadena vacia (Guardar exige texto).
        /// </summary>
        public static string Pedir(IWin32Window owner, string titulo, string mensaje,
            string detalle = null, bool peligro = false)
        {
            using (var dlg = new PromptJustificacion(titulo, mensaje, detalle, peligro))
            {
                return dlg.ShowDialog(owner) == DialogResult.OK ? dlg.Resultado : null;
            }
        }

        private void ConstruirUi(string titulo, string mensaje, string detalle, bool peligro)
        {
            bool hayDetalle = !string.IsNullOrWhiteSpace(detalle);
            Color acento      = peligro ? ThemeManager.ColorError      : ThemeManager.ColorAdvertencia;
            Color acentoSuave = peligro ? ThemeManager.ColorErrorSuave : ThemeManager.ColorAdvertenciaSuave;
            const int ancho = 480;

            SuspendLayout();

            Text            = titulo;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowInTaskbar   = false;
            ShowIcon        = false;
            BackColor       = ThemeManager.ColorFondo;
            Font            = ThemeManager.FuenteRegular;

            // ---- Encabezado por severidad ----
            var header = new Panel
            {
                Location = new Point(0, 0), Size = new Size(ancho, 62), BackColor = acentoSuave
            };
            var icono = new Label
            {
                Location = new Point(18, 0), Size = new Size(40, 62),
                Text = peligro ? "\u26A0" : "\u270E",   // signo de advertencia / lapiz
                Font = new Font(Font.FontFamily, 22F, FontStyle.Bold),
                ForeColor = acento, TextAlign = ContentAlignment.MiddleCenter
            };
            var lblTitulo = new Label
            {
                Location = new Point(64, 0), Size = new Size(ancho - 80, 62),
                Text = titulo, Font = ThemeManager.FuenteSubtitulo,
                ForeColor = ThemeManager.ColorTextoOscuro, TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(icono);
            header.Controls.Add(lblTitulo);
            var franja = new Panel
            {
                Location = new Point(0, 62), Size = new Size(ancho, 3), BackColor = acento
            };

            // ---- Cuerpo ----
            int y = 82;
            var lblMensaje = new Label
            {
                Location = new Point(22, y), Size = new Size(ancho - 44, 40),
                Text = mensaje, Font = ThemeManager.FuenteSemi, ForeColor = ThemeManager.ColorTextoOscuro
            };
            y += 38;

            Label lblDetalle = null;
            if (hayDetalle)
            {
                lblDetalle = new Label
                {
                    Location = new Point(22, y), Size = new Size(ancho - 44, 38),
                    Text = detalle, Font = ThemeManager.FuenteRegular,
                    ForeColor = ThemeManager.ColorTextoSecundario
                };
                y += 42;
            }

            var lblCampo = new Label
            {
                Location = new Point(22, y), Size = new Size(ancho - 44, 18),
                Text = "Escribe la justificaci\u00f3n (obligatoria):",
                Font = ThemeManager.FuenteEtiqueta, ForeColor = ThemeManager.ColorTextoSecundario
            };
            y += 22;

            txtJustificacion = new TextBox
            {
                Location = new Point(22, y), Size = new Size(ancho - 44, 96),
                Multiline = true, BorderStyle = BorderStyle.FixedSingle,
                Font = ThemeManager.FuenteRegular, ForeColor = ThemeManager.ColorTextoOscuro,
                BackColor = ThemeManager.ColorFondo
            };
            txtJustificacion.TextChanged += (s, e) =>
                btnGuardar.Enabled = !string.IsNullOrWhiteSpace(txtJustificacion.Text);
            y += 96 + 18;

            // ---- Botones ----
            btnGuardar = new Button
            {
                Text = "Guardar", Size = new Size(120, 34),
                Location = new Point(ancho - 22 - 120, y),
                DialogResult = DialogResult.OK, Enabled = false
            };
            if (peligro) ThemeManager.EstilizarBotonPeligro(btnGuardar);
            else ThemeManager.EstilizarBotonExito(btnGuardar);

            var btnCancelar = new Button
            {
                Text = "Cancelar", Size = new Size(110, 34),
                Location = new Point(ancho - 22 - 120 - 10 - 110, y),
                DialogResult = DialogResult.Cancel
            };
            ThemeManager.EstilizarBotonSecundario(btnCancelar);

            ClientSize = new Size(ancho, y + 34 + 18);

            Controls.Add(header);
            Controls.Add(franja);
            Controls.Add(lblMensaje);
            if (lblDetalle != null) Controls.Add(lblDetalle);
            Controls.Add(lblCampo);
            Controls.Add(txtJustificacion);
            Controls.Add(btnGuardar);
            Controls.Add(btnCancelar);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;

            ResumeLayout(true);
            ActiveControl = txtJustificacion;
        }
    }
}
