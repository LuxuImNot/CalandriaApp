using System;
using System.Net;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormAgregarProveedor : Form
    {
        public FormAgregarProveedor()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            // La tabla PROVEEDORESCALANDRIA la asegura el API (ProveedoresController).
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string claveUnica = txtClaveUnica.Text.Trim().ToUpper();
            string nombre = txtNombre.Text.Trim();
            string rfc = txtRFC.Text.Trim().ToUpper();
            string direccion = txtDireccion.Text.Trim();
            string telefono = txtTelefono.Text.Trim();

            // Validaciones
            if (string.IsNullOrWhiteSpace(claveUnica))
            {
                MessageBox.Show("La Clave Única es obligatoria.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClaveUnica.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("El Nombre es obligatorio.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(rfc))
            {
                MessageBox.Show("El RFC es obligatorio.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRFC.Focus();
                return;
            }

            // Validar formato de RFC (básico)
            if (rfc.Length < 12 || rfc.Length > 13)
            {
                MessageBox.Show("El RFC debe tener 12 o 13 caracteres.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRFC.Focus();
                return;
            }

            try
            {
                ApiClient.Post("/api/proveedores", new
                {
                    ClaveUnica = claveUnica,
                    Nombre = nombre,
                    Rfc = rfc,
                    Direccion = direccion,
                    Telefono = telefono
                });

                MessageBox.Show("Proveedor guardado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                MessageBox.Show("Ya existe un proveedor con esa Clave Única.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormAgregarProveedor_Load(object sender, EventArgs e)
        {
            // Evento de carga del formulario
        }
    }
}
