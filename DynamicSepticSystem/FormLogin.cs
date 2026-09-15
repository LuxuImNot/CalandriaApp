// Login migrado a WebView2: hospeda Calandria.Api/ui/login.html, igual patrón
// que FormTrabajadoresWeb.cs / FormComprasWeb.cs / FormPerfilesWeb.cs. La
// autenticación ya no toca SQL directo desde el cliente: todo pasa por
// ApiClient.Login (api/auth/login), que ya hace la verificación PBKDF2 y la
// migración perezosa del hash en el servidor.
//
// A diferencia de un login clásico, este formulario NO se abre con
// ShowDialog() (ver reference_webview2_multi_instance: un WebView2 dentro de
// un bucle modal puede colgar el proceso). En su lugar expone el evento
// LoginExitoso; quien lo use debe llamar Show()/Show(this) y suscribirse.
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DynamicSepticSystem
{
    public class FormLogin : Form
    {
        /// <summary>Se dispara tras un login exitoso (Global.UsuarioActual ya está listo).</summary>
        public event EventHandler LoginExitoso;

        private WebView2 webLogin;
        private Actualizador.ResultadoChequeo _ultimoChequeoActualizacion;
        private FormActualizacionWeb _formActualizacion;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "login.html");

        public FormLogin()
        {
            Text = "Iniciar sesión - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 720);
            MinimumSize = new Size(900, 600);
            BackColor = Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webLogin = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webLogin);

            webLogin.CoreWebView2InitializationCompleted += WebLogin_Init;
            _ = IniciarWebView2Async();
        }

        /// <summary>
        /// Entorno y carpeta de datos propios: varias ventanas WebView2 del mismo
        /// proceso no deben compartir el entorno implícito (ver
        /// reference_webview2_multi_instance / FormAlmacen.Web.cs).
        /// </summary>
        private async Task IniciarWebView2Async()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2Login");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webLogin.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("Login", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo iniciar la pantalla de inicio de sesión:\n\n" + ex.Message,
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WebLogin_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("Login", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar la pantalla de inicio de sesión.", "Pilaris",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var core = webLogin.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebLogin_Mensaje;

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("Login", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webLogin?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show(
                    "No se pudo descargar la interfaz de inicio de sesión.\n\nVerifica la conexión con el servidor.",
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            webLogin.CoreWebView2.NavigateToString(html);
        }

        private static string Sha256(byte[] d)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(d)).Replace("-", "").ToLowerInvariant();
        }

        private class VersionUi { public string hash; public int bytes; }

        private static string ObtenerHtml()
        {
            string cache = File.Exists(CacheHtml) ? File.ReadAllText(CacheHtml, Encoding.UTF8) : null;
            try
            {
                var ver = ApiClient.Get<VersionUi>("/api/ui/login/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/login");
                if (datos != null && datos.Length > 0)
                {
                    string html = Encoding.UTF8.GetString(datos);
                    try { Directory.CreateDirectory(CacheDir); File.WriteAllText(CacheHtml, html, new UTF8Encoding(false)); }
                    catch { /* la caché es un extra; si falla, seguimos */ }
                    return html;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("Login", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webLogin?.CoreWebView2 == null) return;
            try { webLogin.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("Login", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgLogin { public string usuario, clave; }

        private void WebLogin_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;
            MsgAccion m;
            try { m = JsonConvert.DeserializeObject<MsgAccion>(json); }
            catch { return; }
            if (m?.accion == null) return;

            // BeginInvoke: no despachar dentro del propio callback COM de WebView2.
            BeginInvoke((Action)(() => DespacharMensaje(m.accion, json)));
        }

        private void DespacharMensaje(string accion, string json)
        {
            switch (accion)
            {
                case "login":
                    { var d = JsonConvert.DeserializeObject<MsgLogin>(json); IntentarLogin(d.usuario, d.clave); }
                    break;
                case "cargar-version":
                    _ = EnviarVersionAppAsync();
                    break;
                case "ver-actualizacion":
                    MostrarVentanaActualizacion();
                    break;
                default:
                    ErrorLogger.RegistrarMensaje("Login", "Acción desconocida desde la página: " + accion);
                    break;
            }
        }

        private void IntentarLogin(string usuario, string clave)
        {
            usuario = (usuario ?? "").Trim();
            // No se recorta la contraseña: los espacios al inicio/fin son válidos.

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave))
            {
                Push(new { tipo = "loginResultado", ok = false, mensaje = "Ingresa usuario y contraseña." });
                return;
            }

            try
            {
                var resp = ApiClient.Login(usuario, clave);
                Global.UsuarioActual = new PanelPrincipal.Usuario
                {
                    Nombre = resp.Usuario ?? usuario,
                    Perfil = resp.Rol,
                    Permisos = resp.Permisos ?? new System.Collections.Generic.List<string>(),
                    EsSuperAdmin = resp.EsSuperAdmin
                };

                Push(new { tipo = "loginResultado", ok = true });
                LoginExitoso?.Invoke(this, EventArgs.Empty);
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Push(new { tipo = "loginResultado", ok = false, mensaje = "Usuario o contraseña incorrectos." });
            }
            catch (ApiException ex) when ((int)ex.StatusCode == 429)
            {
                Push(new { tipo = "loginResultado", ok = false, mensaje = "Demasiados intentos. Espera un momento e inténtalo de nuevo." });
            }
            catch (Exception ex)
            {
                ErrorLogger.Registrar(ex, "FormLogin.IntentarLogin");
                Push(new { tipo = "loginResultado", ok = false, mensaje = "No se pudo conectar con el servidor. Verifica tu conexión." });
            }
        }

        private async Task EnviarVersionAppAsync()
        {
            string local = Actualizador.VersionLocal;
            Push(new { tipo = "version", local, servidor = (string)null, actualizacion = false });

            // Chequeo silencioso: sin internet o sin releases simplemente no reporta
            // actualización, no interrumpe el arranque ni muestra errores.
            _ultimoChequeoActualizacion = await Actualizador.ComprobarAsync();
            var r = _ultimoChequeoActualizacion;

            Push(new
            {
                tipo = "version",
                local,
                servidor = string.IsNullOrEmpty(r.VersionRemota) ? "No disponible" : r.VersionRemota,
                actualizacion = r.HayActualizacion
            });

            if (r.HayActualizacion && !IsDisposed)
                MostrarVentanaActualizacion();
        }

        /// <summary>
        /// Muestra (o reactiva) la ventana de actualización obligatoria con el
        /// último resultado conocido. Se deshabilita este login mientras tanto:
        /// no hace falta volver a habilitarlo después, porque de aquí en
        /// adelante el proceso siempre termina (ya sea porque la actualización
        /// se aplicó, o porque el usuario cerró esa ventana para salir).
        /// </summary>
        private void MostrarVentanaActualizacion()
        {
            var r = _ultimoChequeoActualizacion;
            if (r == null || !r.HayActualizacion) return;

            if (_formActualizacion != null && !_formActualizacion.IsDisposed)
            {
                _formActualizacion.Activate();
                return;
            }

            Enabled = false;
            _formActualizacion = new FormActualizacionWeb(r.VersionLocal, r.VersionRemota, r.ZipUrl);
            _formActualizacion.Show();
        }
    }
}
