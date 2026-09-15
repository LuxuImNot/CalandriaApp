using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Formulario de diagn�stico de conexi�n a base de datos y servicios externos (GitHub)
    /// Permite probar la conexi�n y diagnosticar problemas comunes
    /// </summary>
    public partial class FormDiagnosticoConexion : Form
    {
        private string connectionString;
        private TextBox txtResultados;
        private Button btnProbarConexion;
        private Button btnProbarRed;
        private Button btnProbarGitHub;
        private Button btnVerDetalles;
        private Button btnCopiarLog;
        private ProgressBar progressBar;
        private Label lblEstado;

        public FormDiagnosticoConexion()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        }

        private void InitializeComponent()
        {
            this.Text = "Diagn�stico de Conexi�n - CALANDRIA RESIDENCIAL";
            this.Size = new Size(750, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Panel superior con estado
            var panelEstado = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = ThemeManager.ColorPrincipal,
                Padding = new Padding(15)
            };

            lblEstado = new Label
            {
                Text = "?? Diagn�stico de Conexi�n",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 30
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };

            panelEstado.Controls.Add(progressBar);
            panelEstado.Controls.Add(lblEstado);

            // Panel de botones (2 filas)
            var panelBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 240, 240),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            btnProbarConexion = new Button
            {
                Text = "?? SQL Server",
                Width = 140,
                Height = 38,
                BackColor = ThemeManager.ColorSecundario,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(3)
            };
            btnProbarConexion.FlatAppearance.BorderSize = 0;
            btnProbarConexion.Click += BtnProbarConexion_Click;

            btnProbarRed = new Button
            {
                Text = "?? Internet",
                Width = 140,
                Height = 38,
                BackColor = ThemeManager.ColorSecundario,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(3)
            };
            btnProbarRed.FlatAppearance.BorderSize = 0;
            btnProbarRed.Click += BtnProbarRed_Click;

            btnProbarGitHub = new Button
            {
                Text = "?? GitHub",
                Width = 140,
                Height = 38,
                BackColor = Color.FromArgb(36, 41, 46), // Color GitHub
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(3)
            };
            btnProbarGitHub.FlatAppearance.BorderSize = 0;
            btnProbarGitHub.Click += BtnProbarGitHub_Click;

            btnVerDetalles = new Button
            {
                Text = "?? Configuraci�n",
                Width = 140,
                Height = 38,
                BackColor = ThemeManager.ColorSecundario,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(3)
            };
            btnVerDetalles.FlatAppearance.BorderSize = 0;
            btnVerDetalles.Click += BtnVerDetalles_Click;

            btnCopiarLog = new Button
            {
                Text = "?? Copiar Log",
                Width = 140,
                Height = 38,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(3)
            };
            btnCopiarLog.FlatAppearance.BorderSize = 0;
            btnCopiarLog.Click += BtnCopiarLog_Click;

            panelBotones.Controls.Add(btnProbarConexion);
            panelBotones.Controls.Add(btnProbarRed);
            panelBotones.Controls.Add(btnProbarGitHub);
            panelBotones.Controls.Add(btnVerDetalles);
            panelBotones.Controls.Add(btnCopiarLog);

            // �rea de resultados
            txtResultados = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220),
                ReadOnly = true,
                Padding = new Padding(10)
            };

            // Agregar controles al formulario
            this.Controls.Add(txtResultados);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelEstado);

            // Ejecutar diagn�stico autom�tico al cargar
            this.Load += async (s, e) => await EjecutarDiagnosticoCompleto();
        }

        private async void BtnProbarConexion_Click(object sender, EventArgs e)
        {
            await ProbarConexionSQL();
        }

        private async void BtnProbarRed_Click(object sender, EventArgs e)
        {
            await ProbarConectividadRed();
        }

        private async void BtnProbarGitHub_Click(object sender, EventArgs e)
        {
            await ProbarConexionGitHub();
        }

        private void BtnVerDetalles_Click(object sender, EventArgs e)
        {
            MostrarDetallesConfiguracion();
        }

        private void BtnCopiarLog_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtResultados.Text))
            {
                Clipboard.SetText(txtResultados.Text);
                MessageBox.Show("Log copiado al portapapeles", "�xito", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task EjecutarDiagnosticoCompleto()
        {
            AgregarLinea("??????????????????????????????????????????????????????????");
            AgregarLinea("?  DIAGN�STICO DE CONEXI�N - CALANDRIA RESIDENCIAL       ?");
            AgregarLinea("??????????????????????????????????????????????????????????");
            AgregarLinea($"Fecha/Hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            AgregarLinea("");

            await ProbarConectividadRed();
            await ProbarConexionSQL();
            await ProbarConexionGitHub();
            MostrarDetallesConfiguracion();

            AgregarLinea("");
            AgregarLinea("???????????????????????????????????????????????????????");
            AgregarLinea("DIAGN�STICO COMPLETADO");
            AgregarLinea("???????????????????????????????????????????????????????");
        }

        private async Task ProbarConectividadRed()
        {
            AgregarLinea("");
            AgregarLinea("?????????????????????????????????????????????????????");
            AgregarLinea("?? PRUEBA DE CONECTIVIDAD DE RED");
            AgregarLinea("?????????????????????????????????????????????????????");

            MostrarProgreso(true);
            lblEstado.Text = "?? Probando conectividad de red...";

            try
            {
                // Probar ping a Azure SQL
                AgregarLinea("Probando conexi�n a Azure SQL Server...");
                var ping = new Ping();
                var reply = await ping.SendPingAsync("calandria-sqlserver.database.windows.net", 5000);
                
                if (reply.Status == IPStatus.Success)
                {
                    AgregarLineaExito($"? Ping exitoso: {reply.RoundtripTime}ms");
                }
                else
                {
                    AgregarLineaError($"? Ping fallido: {reply.Status}");
                }

                // Probar conexi�n a Internet general
                AgregarLinea("");
                AgregarLinea("Probando conexi�n a Internet (Google DNS)...");
                var replyInternet = await ping.SendPingAsync("8.8.8.8", 5000);
                
                if (replyInternet.Status == IPStatus.Success)
                {
                    AgregarLineaExito($"? Internet disponible: {replyInternet.RoundtripTime}ms");
                }
                else
                {
                    AgregarLineaError($"? Sin conexi�n a Internet: {replyInternet.Status}");
                }

                // Probar GitHub
                AgregarLinea("");
                AgregarLinea("Probando conexi�n a GitHub...");
                var replyGitHub = await ping.SendPingAsync("github.com", 5000);
                
                if (replyGitHub.Status == IPStatus.Success)
                {
                    AgregarLineaExito($"? GitHub accesible: {replyGitHub.RoundtripTime}ms");
                }
                else
                {
                    AgregarLineaError($"? No se puede alcanzar GitHub: {replyGitHub.Status}");
                }

                // Informaci�n de red local
                AgregarLinea("");
                AgregarLinea("Informaci�n de red local:");
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && 
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        AgregarLinea($"  � {ni.Name}: {ni.NetworkInterfaceType} ({ni.Speed / 1000000} Mbps)");
                    }
                }
            }
            catch (Exception ex)
            {
                AgregarLineaError($"? Error en prueba de red: {ex.Message}");
            }
            finally
            {
                MostrarProgreso(false);
            }
        }

        private async Task ProbarConexionGitHub()
        {
            AgregarLinea("");
            AgregarLinea("?????????????????????????????????????????????????????");
            AgregarLinea("?? PRUEBA DE CONEXI�N A GITHUB");
            AgregarLinea("?????????????????????????????????????????????????????");

            MostrarProgreso(true);
            lblEstado.Text = "?? Probando conexi�n a GitHub...";

            try
            {
                // Leer configuraci�n de GitHub
                string githubOwner = ConfigurationManager.AppSettings["GitHubOwner"] ?? "LuxuImNot";
                string githubRepo = ConfigurationManager.AppSettings["GitHubRepo"] ?? "CalandriaApp";
                string githubToken = ConfigurationManager.AppSettings["GitHubToken"] ?? "";

                AgregarLinea($"Repositorio configurado: {githubOwner}/{githubRepo}");
                AgregarLinea($"Token configurado: {(string.IsNullOrEmpty(githubToken) ? "? NO" : "? S�")}");
                AgregarLinea("");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "CalandriaResidencial");
                    httpClient.Timeout = TimeSpan.FromSeconds(10);

                    if (!string.IsNullOrEmpty(githubToken))
                    {
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"token {githubToken}");
                    }

                    // 1. Probar conexi�n a GitHub API
                    AgregarLinea("1?? Probando GitHub API...");
                    var stopwatch = Stopwatch.StartNew();
                    
                    try
                    {
                        var response = await httpClient.GetAsync("https://api.github.com");
                        stopwatch.Stop();

                        if (response.IsSuccessStatusCode)
                        {
                            AgregarLineaExito($"   ? GitHub API accesible ({stopwatch.ElapsedMilliseconds}ms)");
                            
                            // Verificar rate limit
                            if (response.Headers.Contains("X-RateLimit-Remaining"))
                            {
                                var remaining = response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault();
                                AgregarLinea($"   � Rate Limit restante: {remaining}");
                            }
                        }
                        else
                        {
                            AgregarLineaError($"   ? Error HTTP: {response.StatusCode}");
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        AgregarLineaError("   ? Timeout al conectar con GitHub API");
                    }
                    catch (HttpRequestException ex)
                    {
                        AgregarLineaError($"   ? Error de conexi�n: {ex.Message}");
                    }

                    AgregarLinea("");

                    // 2. Probar acceso al repositorio
                    AgregarLinea("2?? Verificando acceso al repositorio...");
                    string repoUrl = $"https://api.github.com/repos/{githubOwner}/{githubRepo}";
                    
                    try
                    {
                        stopwatch.Restart();
                        var repoResponse = await httpClient.GetAsync(repoUrl);
                        stopwatch.Stop();

                        if (repoResponse.IsSuccessStatusCode)
                        {
                            AgregarLineaExito($"   ? Repositorio accesible ({stopwatch.ElapsedMilliseconds}ms)");
                            
                            // Leer informaci�n del repo
                            string repoJson = await repoResponse.Content.ReadAsStringAsync();
                            
                            // Parsear manualmente (sin Newtonsoft.Json para evitar dependencia)
                            if (repoJson.Contains("\"private\":true"))
                            {
                                AgregarLinea("   � Tipo: Privado ??");
                            }
                            else if (repoJson.Contains("\"private\":false"))
                            {
                                AgregarLinea("   � Tipo: P�blico ??");
                            }
                        }
                        else if (repoResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            AgregarLineaError("   ? Repositorio no encontrado");
                            AgregarLinea("   ?? Verifica GitHubOwner y GitHubRepo en App.config");
                        }
                        else if (repoResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            AgregarLineaError("   ? No autorizado (token inv�lido o expirado)");
                            AgregarLinea("   ?? Verifica o regenera el GitHubToken en App.config");
                        }
                        else
                        {
                            AgregarLineaError($"   ? Error HTTP: {repoResponse.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AgregarLineaError($"   ? Error: {ex.Message}");
                    }

                    AgregarLinea("");

                    // 3. Probar acceso a releases
                    AgregarLinea("3?? Verificando acceso a releases...");
                    string releasesUrl = $"https://api.github.com/repos/{githubOwner}/{githubRepo}/releases/latest";
                    
                    try
                    {
                        stopwatch.Restart();
                        var releasesResponse = await httpClient.GetAsync(releasesUrl);
                        stopwatch.Stop();

                        if (releasesResponse.IsSuccessStatusCode)
                        {
                            AgregarLineaExito($"   ? Releases accesibles ({stopwatch.ElapsedMilliseconds}ms)");
                            
                            string releasesJson = await releasesResponse.Content.ReadAsStringAsync();
                            
                            // Parsear tag_name manualmente
                            int tagIndex = releasesJson.IndexOf("\"tag_name\":");
                            if (tagIndex >= 0)
                            {
                                int startQuote = releasesJson.IndexOf("\"", tagIndex + 11);
                                int endQuote = releasesJson.IndexOf("\"", startQuote + 1);
                                string latestVersion = releasesJson.Substring(startQuote + 1, endQuote - startQuote - 1);
                                if (latestVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                                    latestVersion = latestVersion.Substring(1);

                                AgregarLinea($"   � �ltima versi�n disponible: {latestVersion}");
                                AgregarLinea($"   � Versi�n local: {Actualizador.VersionLocal}");

                                if (Actualizador.EsNuevaVersion(Actualizador.VersionLocal, latestVersion))
                                {
                                    AgregarLinea("   ??  Hay una actualizaci�n disponible");
                                }
                                else
                                {
                                    AgregarLinea("   ? Aplicaci�n actualizada");
                                }
                            }
                        }
                        else if (releasesResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            AgregarLineaError("   ? No se encontraron releases");
                            AgregarLinea("   ?? Verifica que existan releases en el repositorio");
                        }
                        else
                        {
                            AgregarLineaError($"   ? Error HTTP: {releasesResponse.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AgregarLineaError($"   ? Error: {ex.Message}");
                    }
                }

                AgregarLinea("");
                AgregarLinea("?? DIAGN�STICO DE PROBLEMAS COMUNES:");
                
                if (string.IsNullOrEmpty(githubToken))
                {
                    AgregarLineaError("  ?? No hay token de GitHub configurado");
                    AgregarLinea("  ?? Soluci�n:");
                    AgregarLinea("     1. Ve a GitHub ? Settings ? Developer settings ? Personal access tokens");
                    AgregarLinea("     2. Genera un nuevo token con permisos 'repo'");
                    AgregarLinea("     3. Agr�galo a App.config:");
                    AgregarLinea("        <add key=\"GitHubToken\" value=\"tu_token_aqu�\" />");
                }

                lblEstado.Text = "? Diagn�stico GitHub completado";
                lblEstado.ForeColor = Color.LightGreen;
            }
            catch (Exception ex)
            {
                AgregarLineaError($"? Error general: {ex.Message}");
                lblEstado.Text = "? Error en diagn�stico GitHub";
                lblEstado.ForeColor = Color.FromArgb(231, 76, 60);
            }
            finally
            {
                MostrarProgreso(false);
            }
        }

        private async Task ProbarConexionSQL()
        {
            AgregarLinea("");
            AgregarLinea("?????????????????????????????????????????????????????");
            AgregarLinea("?? PRUEBA DE CONEXI�N A BASE DE DATOS");
            AgregarLinea("?????????????????????????????????????????????????????");

            MostrarProgreso(true);
            lblEstado.Text = "?? Probando conexi�n a SQL Server...";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    AgregarLinea("Intentando conectar a Azure SQL Database...");
                    
                    var stopwatch = Stopwatch.StartNew();
                    await conn.OpenAsync();
                    stopwatch.Stop();

                    AgregarLineaExito($"? Conexi�n exitosa ({stopwatch.ElapsedMilliseconds}ms)");

                    // Obtener informaci�n del servidor
                    using (var cmd = new SqlCommand("SELECT @@VERSION, DB_NAME(), SUSER_NAME()", conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                AgregarLinea("");
                                AgregarLinea("Informaci�n del servidor:");
                                AgregarLinea($"  � Base de datos: {reader.GetString(1)}");
                                AgregarLinea($"  � Usuario conectado: {reader.GetString(2)}");
                            }
                        }
                    }

                    // Probar consulta simple
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES", conn))
                    {
                        var count = await cmd.ExecuteScalarAsync();
                        AgregarLineaExito($"  � Tablas en BD: {count}");
                    }

                    lblEstado.Text = "? Conexi�n exitosa";
                    lblEstado.ForeColor = Color.LightGreen;
                }
            }
            catch (SqlException sqlEx)
            {
                AgregarLineaError("? ERROR DE SQL SERVER:");
                AgregarLineaError($"  C�digo: {sqlEx.Number}");
                AgregarLineaError($"  Mensaje: {sqlEx.Message}");

                lblEstado.Text = "? Error de conexi�n SQL";
                lblEstado.ForeColor = Color.FromArgb(231, 76, 60);

                // Diagnosticar errores comunes
                DiagnosticarErrorSQL(sqlEx);
            }
            catch (Exception ex)
            {
                AgregarLineaError($"? Error general: {ex.Message}");
                lblEstado.Text = "? Error de conexi�n";
                lblEstado.ForeColor = Color.FromArgb(231, 76, 60);
            }
            finally
            {
                MostrarProgreso(false);
            }
        }

        private void DiagnosticarErrorSQL(SqlException sqlEx)
        {
            AgregarLinea("");
            AgregarLinea("?? DIAGN�STICO DEL ERROR:");

            switch (sqlEx.Number)
            {
                case 53:
                case 2:
                case -1:
                    AgregarLineaError("  ?? No se puede conectar al servidor");
                    AgregarLinea("  Posibles causas:");
                    AgregarLinea("    � El servidor Azure SQL no est� disponible");
                    AgregarLinea("    � Tu IP est� bloqueada por el firewall de Azure");
                    AgregarLinea("    � Problemas de red/internet");
                    AgregarLinea("");
                    AgregarLinea("  ?? Soluciones:");
                    AgregarLinea("    1. Verifica tu conexi�n a Internet");
                    AgregarLinea("    2. Ve a Azure Portal ? SQL Server ? Firewalls and virtual networks");
                    AgregarLinea("    3. Agrega tu IP actual a las reglas de firewall");
                    break;

                case 18456:
                    AgregarLineaError("  ?? Error de autenticaci�n");
                    AgregarLinea("  Posibles causas:");
                    AgregarLinea("    � Usuario o contrase�a incorrectos");
                    AgregarLinea("    � La contrase�a ha expirado");
                    AgregarLinea("");
                    AgregarLinea("  ?? Soluciones:");
                    AgregarLinea("    1. Verifica el usuario y contrase�a en App.config");
                    AgregarLinea("    2. Resetea la contrase�a en Azure Portal");
                    break;

                case 40613:
                case 40197:
                    AgregarLineaError("  ?? Base de datos no disponible temporalmente");
                    AgregarLinea("  Posibles causas:");
                    AgregarLinea("    � Azure est� realizando mantenimiento");
                    AgregarLinea("    � Se excedi� el DTU l�mite");
                    AgregarLinea("");
                    AgregarLinea("  ?? Soluciones:");
                    AgregarLinea("    1. Espera unos minutos y reintenta");
                    AgregarLinea("    2. Verifica el estado del servicio en Azure Portal");
                    break;

                case 4060:
                    AgregarLineaError("  ?? No se puede abrir la base de datos");
                    AgregarLinea("  Posibles causas:");
                    AgregarLinea("    � La base de datos no existe");
                    AgregarLinea("    � El nombre de la BD es incorrecto");
                    AgregarLinea("");
                    AgregarLinea("  ?? Soluciones:");
                    AgregarLinea("    1. Verifica el nombre de BD en App.config");
                    AgregarLinea("    2. Confirma que la BD existe en Azure Portal");
                    break;

                default:
                    AgregarLineaError($"  ?? Error no categorizado (C�digo: {sqlEx.Number})");
                    AgregarLinea("  ?? Busca este c�digo de error en:");
                    AgregarLinea("    https://docs.microsoft.com/sql/relational-databases/errors-events/database-engine-events-and-errors");
                    break;
            }
        }

        private void MostrarDetallesConfiguracion()
        {
            AgregarLinea("");
            AgregarLinea("?????????????????????????????????????????????????????");
            AgregarLinea("?? DETALLES DE CONFIGURACI�N");
            AgregarLinea("?????????????????????????????????????????????????????");

            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);

                AgregarLinea("SQL Server:");
                AgregarLinea($"  � Data Source: {builder.DataSource}");
                AgregarLinea($"  � Initial Catalog: {builder.InitialCatalog}");
                AgregarLinea($"  � User ID: {builder.UserID}");
                AgregarLinea($"  � Encrypt: {builder.Encrypt}");
                AgregarLinea($"  � Trust Server Certificate: {builder.TrustServerCertificate}");
                AgregarLinea($"  � Connection Timeout: {builder.ConnectTimeout}s");

                AgregarLinea("");
                AgregarLinea("GitHub:");
                AgregarLinea($"  � Owner: {ConfigurationManager.AppSettings["GitHubOwner"] ?? "No configurado"}");
                AgregarLinea($"  � Repo: {ConfigurationManager.AppSettings["GitHubRepo"] ?? "No configurado"}");
                AgregarLinea($"  � Token: {(string.IsNullOrEmpty(ConfigurationManager.AppSettings["GitHubToken"]) ? "? No configurado" : "? Configurado")}");

                AgregarLinea("");
                AgregarLinea("Aplicaci�n:");
                AgregarLinea($"  � Versi�n: {Application.ProductVersion}");
                AgregarLinea($"  � Ruta: {Application.StartupPath}");
                AgregarLinea($"  � .NET Framework: {Environment.Version}");
                AgregarLinea($"  � SO: {Environment.OSVersion}");
            }
            catch (Exception ex)
            {
                AgregarLineaError($"Error al leer configuraci�n: {ex.Message}");
            }
        }

        private void AgregarLinea(string texto)
        {
            if (txtResultados.InvokeRequired)
            {
                txtResultados.Invoke(new Action(() => AgregarLinea(texto)));
                return;
            }

            txtResultados.AppendText(texto + Environment.NewLine);
        }

        private void AgregarLineaExito(string texto)
        {
            AgregarLinea(texto);
        }

        private void AgregarLineaError(string texto)
        {
            AgregarLinea(texto);
        }

        private void MostrarProgreso(bool mostrar)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => MostrarProgreso(mostrar)));
                return;
            }

            if (mostrar)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.Visible = true;
            }
            else
            {
                progressBar.Visible = false;
                progressBar.Style = ProgressBarStyle.Continuous;
            }
        }
    }
}
