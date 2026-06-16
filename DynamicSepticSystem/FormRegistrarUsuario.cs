using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormRegistrarUsuario : Form
    {
        public FormRegistrarUsuario()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            cmbRol.Items.Add("Admin");
            cmbRol.Items.Add("SoloLectura");
            cmbRol.SelectedIndex = 0;
        }

        private void btnRegistrar_Click_1(object sender, EventArgs e)
        {
            string nuevoUsuario = txtNuevoUsuario.Text.Trim();
            // No se recorta la contraseña: los espacios al inicio/fin son válidos.
            string nuevaClave = txtNuevaClave.Text;
            string rol = cmbRol.SelectedItem?.ToString() ?? "SoloLectura";

            if (string.IsNullOrWhiteSpace(nuevoUsuario) || string.IsNullOrWhiteSpace(nuevaClave))
            {
                MessageBox.Show("Usuario y contraseña requeridos.");
                return;
            }

            string hash = PasswordHasher.Hash(nuevaClave);

            string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Usuarios (Nombre, ClaveHash, Rol) VALUES (@n, @h, @r)", conn))
                {
                    cmd.Parameters.AddWithValue("@n", nuevoUsuario);
                    cmd.Parameters.AddWithValue("@h", hash);
                    cmd.Parameters.AddWithValue("@r", rol);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Usuario registrado exitosamente.");
                        this.Close();
                    }
                    catch (SqlException ex)
                    {
                        // El detalle (p.ej. nombre duplicado) va al log; mensaje genérico al usuario.
                        ErrorLogger.Registrar(ex, "FormRegistrarUsuario.btnRegistrar");
                        MessageBox.Show("No se pudo registrar el usuario. Verifica los datos e inténtalo de nuevo.");
                    }
                }
            }
        }

        private void FormRegistrarUsuario_Load(object sender, EventArgs e)
        {

        }
    }
}
