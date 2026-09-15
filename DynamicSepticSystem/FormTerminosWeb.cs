// Aceptación de Términos de Uso / Aviso de Privacidad, migrada a WebView2:
// hospeda Calandria.Api/ui/terminos.html, mismo patrón que FormLogin.cs /
// FormSeleccionObraWeb.cs. Se muestra tras el login solo si
// api/terminos/estado indica que el usuario todavía no aceptó la versión
// vigente (ver TerminosController).
//
// No se abre con ShowDialog() (ver reference_webview2_multi_instance: un
// WebView2 dentro de un bucle modal puede colgar el proceso). Expone el
// evento TerminosAceptados; quien lo use debe llamar Show() y suscribirse.
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

namespace DynamicSepticSystem
{
    public class FormTerminosWeb : Form
    {
        /// <summary>Se dispara cuando el usuario acepta los términos vigentes.</summary>
        public event EventHandler TerminosAceptados;

        /// <summary>True si llegó a aceptar antes de cerrar este formulario.</summary>
        public bool Aceptado { get; private set; }

        private WebView2 webTerminos;

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "terminos.html");

        public FormTerminosWeb()
        {
            Text = "Términos de Uso y Aviso de Privacidad - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1000, 720);
            MinimumSize = new Size(760, 560);
            BackColor = Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webTerminos = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webTerminos);

            webTerminos.CoreWebView2InitializationCompleted += WebTerminos_Init;
            _ = IniciarWebView2Async();
        }

        /// <summary>
        /// Entorno y carpeta de datos propios: varias ventanas WebView2 del mismo
        /// proceso no deben compartir el entorno implícito (ver
        /// reference_webview2_multi_instance).
        /// </summary>
        private async Task IniciarWebView2Async()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2Terminos");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webTerminos.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("Terminos", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo mostrar los Términos de Uso:\n\n" + ex.Message,
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WebTerminos_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("Terminos", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo mostrar los Términos de Uso.", "Pilaris",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var core = webTerminos.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebTerminos_Mensaje;

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("Terminos", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webTerminos?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show(
                    "No se pudieron descargar los Términos de Uso.\n\nVerifica la conexión con el servidor.",
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            webTerminos.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/terminos/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/terminos");
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
                ErrorLogger.RegistrarMensaje("Terminos", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }

        private void WebTerminos_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;
            MsgAccion m;
            try { m = JsonConvert.DeserializeObject<MsgAccion>(json); }
            catch { return; }
            if (m?.accion == null) return;

            // BeginInvoke: no despachar dentro del propio callback COM de WebView2.
            BeginInvoke((Action)(() => DespacharMensaje(m.accion)));
        }

        private void DespacharMensaje(string accion)
        {
            switch (accion)
            {
                case "aceptar":
                    Aceptar();
                    break;
                default:
                    ErrorLogger.RegistrarMensaje("Terminos", "Acción desconocida desde la página: " + accion);
                    break;
            }
        }

        private void Aceptar()
        {
            try
            {
                ApiClient.Post("/api/terminos/aceptar", new { });
                Aceptado = true;
                TerminosAceptados?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorLogger.Registrar(ex, "FormTerminosWeb.Aceptar");
                MessageBox.Show("No se pudo registrar la aceptación. Verifica tu conexión e inténtalo de nuevo.",
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
