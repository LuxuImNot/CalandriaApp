using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public class FormPdfPreview : Form
    {
        private readonly string _tempFile;
        private readonly string _suggestedSavePath;
        private Button btnOpen;
        private Button btnSave;
        private Button btnCancel;
        private Label lblInfo;

        public bool Saved { get; private set; }
        public string SavedPath { get; private set; }

        public FormPdfPreview(string tempFile, string suggestedSavePath)
        {
            _tempFile = tempFile;
            _suggestedSavePath = suggestedSavePath;
            InitializeComponent();
            ThemeManager.AplicarTema(this);
        }

        private void InitializeComponent()
        {
            this.Text = "Vista previa PDF";
            this.Width = 520;
            this.Height = 160;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblInfo = new Label { Left = 12, Top = 12, Width = 480, Height = 40 };
            lblInfo.Text = "Se gener� una vista previa del PDF en un archivo temporal. Puedes abrirla, guardar una copia o cancelar.";
            this.Controls.Add(lblInfo);

            btnOpen = new Button { Text = "Abrir vista previa", Left = 12, Top = 60, Width = 150, Height = 30 };
            btnOpen.Click += BtnOpen_Click;
            this.Controls.Add(btnOpen);

            btnSave = new Button { Text = "Guardar copia", Left = 180, Top = 60, Width = 150, Height = 30 };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button { Text = "Cancelar", Left = 348, Top = 60, Width = 150, Height = 30, DialogResult = DialogResult.Cancel };
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(_tempFile))
                    Process.Start(new ProcessStartInfo(_tempFile) { UseShellExecute = true });
                else
                    MessageBox.Show("Archivo temporal no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog { Filter = "PDF Files|*.pdf", FileName = Path.GetFileName(_suggestedSavePath) };
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                var dest = sfd.FileName;
                File.Copy(_tempFile, dest, true);
                Saved = true;
                SavedPath = dest;
                MessageBox.Show("Copia guardada en:\n" + dest, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error guardando copia: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try { if (File.Exists(_tempFile)) File.Delete(_tempFile); } catch { }
            this.Close();
        }
    }
}
