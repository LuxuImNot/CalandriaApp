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
        private sealed class PerfilItem
        {
            public int Id;
            public string Nombre;
            public override string ToString() => Nombre;
        }

        public FormRegistrarUsuario()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            CargarPerfiles();
        }

        private void CargarPerfiles()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Id, Nombre FROM Perfiles ORDER BY Nombre", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbRol.Items.Add(new PerfilItem
                            {
                                Id = (int)reader["Id"],
                                Nombre = reader["Nombre"].ToString()
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ErrorLogger.Registrar(ex, "FormRegistrarUsuario.CargarPerfiles");
                MessageBox.Show("No se pudieron cargar los perfiles disponibles.");
            }
            if (cmbRol.Items.Count > 0)
                cmbRol.SelectedIndex = 0;
        }

        private void btnRegistrar_Click_1(object sender, EventArgs e)
        {
            string nuevoUsuario = txtNuevoUsuario.Text.Trim();
            // No se recorta la contraseña: los espacios al inicio/fin son válidos.
            string nuevaClave = txtNuevaClave.Text;
            PerfilItem perfil = cmbRol.SelectedItem as PerfilItem;

            if (string.IsNullOrWhiteSpace(nuevoUsuario) || string.IsNullOrWhiteSpace(nuevaClave))
            {
                MessageBox.Show("Usuario y contraseña requeridos.");
                return;
            }
            if (perfil == null)
            {
                MessageBox.Show("Selecciona un perfil.");
                return;
            }

            string hash = PasswordHasher.Hash(nuevaClave);

            string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Usuarios (Nombre, ClaveHash, Rol, PerfilId) VALUES (@n, @h, @r, @p)", conn))
                {
                    cmd.Parameters.AddWithValue("@n", nuevoUsuario);
                    cmd.Parameters.AddWithValue("@h", hash);
                    cmd.Parameters.AddWithValue("@r", perfil.Nombre);
                    cmd.Parameters.AddWithValue("@p", perfil.Id);

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
