using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;
using System.Drawing;
using System.Text;
using DynamicSepticSystem;
using MaterialSkin;
using MaterialSkin.Controls;
using static DynamicSepticSystem.PanelPrincipal;

namespace DynamicSepticSystem
{
    public partial class FormLogin : MaterialForm
    {
        public static string UsuarioActivo = "";
        public static string RolActivo = "";

        public FormLogin()
        {
            InitializeComponent();
            
            // Aplicar tema MaterialSkin personalizado con colores corporativos
            ThemeManager.AplicarTemaMaterial(this);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string usuarioIngresado = txtUsuario.Text.Trim();
            // No se recorta la contraseña: los espacios al inicio/fin son válidos.
            string claveIngresada = txtClave.Text;

            if (string.IsNullOrWhiteSpace(usuarioIngresado) || string.IsNullOrWhiteSpace(claveIngresada))
            {
                MessageBox.Show("Ingresa usuario y contraseña.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT ClaveHash, Rol FROM Usuarios WHERE Nombre = @usuario", conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuarioIngresado);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hashAlmacenado = reader["ClaveHash"].ToString();
                                string rol = reader["Rol"].ToString();

                                bool claveValida = PasswordHasher.Verificar(
                                    claveIngresada, hashAlmacenado, out bool necesitaRehash);

                                if (claveValida)
                                {
                                    // El lector debe cerrarse antes de reutilizar la conexión.
                                    reader.Close();

                                    // Migración perezosa: si el hash estaba en el formato
                                    // heredado (SHA-256 sin sal), se re-guarda con PBKDF2.
                                    if (necesitaRehash)
                                    {
                                        try
                                        {
                                            ActualizarHashClave(conn, usuarioIngresado,
                                                PasswordHasher.Hash(claveIngresada));
                                        }
                                        catch (Exception exRehash)
                                        {
                                            // No impide el login; solo se registra.
                                            ErrorLogger.Registrar(exRehash, "FormLogin.ActualizarHashClave");
                                        }
                                    }

                                    Global.UsuarioActual = new Usuario
                                    {
                                        Nombre = usuarioIngresado,
                                        Clave = hashAlmacenado,
                                        Permisos = rol == "Admin"
                                            ? new List<string> { "Agregar", "Guardar", "Eliminar", "Ver" }
                                            : new List<string> { "Ver" }
                                    };

                                    MessageBox.Show("Logueado como: " + Global.UsuarioActual.Nombre);
                                    this.Hide();
                                    this.DialogResult = DialogResult.OK;
                                    this.Close();
                                }
                                else
                                {
                                    // Mensaje genérico: no revela si el usuario existe.
                                    MessageBox.Show("Usuario o contraseña incorrectos.");
                                }
                            }
                            else
                            {
                                // Mismo mensaje que clave incorrecta para no filtrar
                                // qué usuarios existen (enumeración de cuentas).
                                MessageBox.Show("Usuario o contraseña incorrectos.");
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // El detalle técnico va solo al log; al usuario un mensaje genérico.
                ErrorLogger.Registrar(sqlEx, "FormLogin.btnLogin");
                MessageBox.Show(
                    "No se pudo conectar a la base de datos.\n\n" +
                    "Verifica tu conexión e inténtalo de nuevo.",
                    "Error de Conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                ErrorLogger.Registrar(ex, "FormLogin.btnLogin");
                MessageBox.Show(
                    "Ocurrió un error inesperado al iniciar sesión. Inténtalo de nuevo.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Re-guarda el hash de la contraseña de un usuario (migración perezosa
        /// del formato heredado a PBKDF2). Reutiliza la conexión ya abierta.
        /// </summary>
        private void ActualizarHashClave(SqlConnection conn, string usuario, string nuevoHash)
        {
            using (var cmd = new SqlCommand(
                "UPDATE Usuarios SET ClaveHash = @h WHERE Nombre = @u", conn))
            {
                cmd.Parameters.AddWithValue("@h", nuevoHash);
                cmd.Parameters.AddWithValue("@u", usuario);
                cmd.ExecuteNonQuery();
            }
        }

        private void txtClave_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnLogin.PerformClick();
            }
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            // Obtener versión local
            string versionLocal = Actualizador.VersionLocal;
            string versionServidor = "Obteniendo...";

            // Mostrar versión local inmediatamente
            lblVersion.Text = $"Versión local: {versionLocal} | Servidor: {versionServidor}";
            lblVersion.ForeColor = ThemeManager.ColorTextoSecundario;
            lblVersion.Refresh();

            // Obtener versión del servidor en segundo plano (async)
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    // Obtener versión desde GitHub
                    string githubOwner = ConfigurationManager.AppSettings["GitHubOwner"] ?? "LuxuImNot";
                    string githubRepo = ConfigurationManager.AppSettings["GitHubRepo"] ?? "CalandriaApp";
                    string githubToken = ConfigurationManager.AppSettings["GitHubToken"];

                    using (var httpClient = new System.Net.Http.HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("User-Agent", "CalandriaResidencial");
                        httpClient.Timeout = TimeSpan.FromSeconds(5);

                        if (!string.IsNullOrEmpty(githubToken))
                        {
                            httpClient.DefaultRequestHeaders.Add("Authorization", $"token {githubToken}");
                        }

                        string releasesUrl = $"https://api.github.com/repos/{githubOwner}/{githubRepo}/releases/latest";
                        var response = await httpClient.GetAsync(releasesUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            string releasesJson = await response.Content.ReadAsStringAsync();

                            // Parsear tag_name manualmente
                            int tagIndex = releasesJson.IndexOf("\"tag_name\":");
                            if (tagIndex >= 0)
                            {
                                int startQuote = releasesJson.IndexOf("\"", tagIndex + 11);
                                int endQuote = releasesJson.IndexOf("\"", startQuote + 1);
                                versionServidor = releasesJson.Substring(startQuote + 1, endQuote - startQuote - 1);
                            }
                            else
                            {
                                versionServidor = "Sin releases";
                            }
                        }
                        else
                        {
                            versionServidor = "No disponible";
                        }
                    }
                }
                catch
                {
                    // Si GitHub falla, mostrar no disponible
                    versionServidor = "No disponible";
                }

                // Actualizar UI en el thread principal
                if (lblVersion.InvokeRequired)
                {
                    lblVersion.Invoke(new Action(() =>
                    {
                        lblVersion.Text = $"Versión local: {versionLocal} | Servidor: {versionServidor}";
                        
                        // Cambiar color según resultado
                        if (versionServidor == "N/D" || versionServidor == "No disponible" || versionServidor == "Sin releases")
                        {
                            lblVersion.ForeColor = ThemeManager.ColorError;
                        }
                        else if (versionServidor != versionLocal)
                        {
                            lblVersion.ForeColor = Color.Orange; // Hay actualización disponible
                        }
                        else
                        {
                            lblVersion.ForeColor = ThemeManager.ColorTextoSecundario;
                        }
                        
                        lblVersion.Refresh();
                    }));
                }
            });
        }

        private void chkVerClave_CheckedChanged(object sender, EventArgs e)
        {
            if (chkVerClave.Checked)
            {
                // Mostrar contraseña
                txtClave.UseSystemPasswordChar = false;
                txtClave.PasswordChar = '\0';
                txtClave.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            }
            else
            {
                // Ocultar contraseña
                txtClave.UseSystemPasswordChar = true;
                txtClave.PasswordChar = '●';
                txtClave.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            }
        }
    }
}
