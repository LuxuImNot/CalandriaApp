using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormAgregarProveedor : Form
    {
        public FormAgregarProveedor()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            InicializarTablaProveedores();
        }

        /// <summary>
        /// Crea la tabla PROVEEDORESCALANDRIA si no existe
        /// </summary>
        private void InicializarTablaProveedores()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PROVEEDORESCALANDRIA') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.PROVEEDORESCALANDRIA (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ClaveUnica NVARCHAR(50) NOT NULL UNIQUE,
        Nombre NVARCHAR(200) NOT NULL,
        RFC NVARCHAR(13) NOT NULL,
        Direccion NVARCHAR(500) NULL,
        Telefono NVARCHAR(20) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
    );
END";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al inicializar tabla de proveedores: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    string sql = @"INSERT INTO PROVEEDORESCALANDRIA (ClaveUnica, Nombre, RFC, Direccion, Telefono) 
                                   VALUES (@clave, @nombre, @rfc, @direccion, @telefono)";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@clave", claveUnica);
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@rfc", rfc);
                        cmd.Parameters.AddWithValue("@direccion", string.IsNullOrWhiteSpace(direccion) ? (object)DBNull.Value : direccion);
                        cmd.Parameters.AddWithValue("@telefono", string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono);
                        
                        cmd.ExecuteNonQuery();
                        
                        MessageBox.Show("Proveedor guardado correctamente.", "Éxito", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Duplicate key
                {
                    MessageBox.Show("Ya existe un proveedor con esa Clave Única.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Error al guardar: " + ex.Message, "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
