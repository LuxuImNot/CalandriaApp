// Ventana de actualización migrada a WebView2: hospeda
// Calandria.Api/ui/actualizacion.html, mismo patrón que FormLogin.cs /
// FormTerminosWeb.cs. Se muestra cuando Actualizador.ComprobarAsync()
// detecta una versión nueva (chequeo en segundo plano desde FormLogin, sin
// bloquear el arranque) o cuando el usuario la abre manualmente desde el
// badge de "Actualización disponible" en el login.
//
// La actualización es OBLIGATORIA: no hay botón para posponerla, y cerrar
// esta ventana por cualquier vía cierra toda la aplicación (ver
// OnFormClosing). No se abre con ShowDialog() (ver
// reference_webview2_multi_instance); en su lugar, FormLogin se deshabilita
// mientras esta ventana está abierta.
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
    public class FormActualizacionWeb : Form
    {
        private readonly string _versionLocal;
        private readonly string _versionRemota;
        private readonly string _zipUrl;

        private WebView2 webActualizacion;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "actualizacion.html");

        public FormActualizacionWeb(string versionLocal, string versionRemota, string zipUrl)
        {
            _versionLocal = versionLocal;
            _versionRemota = versionRemota;
            _zipUrl = zipUrl;

            Text = "Actualización obligatoria - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(560, 420);
            MinimumSize = new Size(480, 360);
            MinimizeBox = false;
            BackColor = Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webActualizacion = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webActualizacion);

            webActualizacion.CoreWebView2InitializationCompleted += WebActualizacion_Init;
            _ = IniciarWebView2Async();
        }

        /// <summary>
        /// La actualización es obligatoria: no hay forma de "esquivar" esta
        /// ventana y seguir usando el login de atrás. Cerrarla por cualquier vía
        /// (botón "Salir", la X, Alt+F4) cierra TODA la aplicación, no solo esta
        /// ventana. En el camino exitoso nunca se llega aquí porque
        /// Actualizador.DescargarYAplicarAsync ya termina el proceso antes.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Environment.Exit(0);
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
                    "Calandria", "WebView2Actualizacion");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webActualizacion.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("Actualizacion", "WebView2 no pudo iniciar: " + ex.Message);
                Close();
            }
        }

        private void WebActualizacion_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("Actualizacion", "WebView2 no inicializó: " + e.InitializationException);
                Close();
                return;
            }

            var core = webActualizacion.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebActualizacion_Mensaje;

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("Actualizacion", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webActualizacion?.CoreWebView2 == null) return;
            if (html == null) { Close(); return; }

            webActualizacion.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/actualizacion/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/actualizacion");
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
                ErrorLogger.RegistrarMensaje("Actualizacion", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webActualizacion?.CoreWebView2 == null) return;
            try { webActualizacion.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("Actualizacion", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }

        private void WebActualizacion_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
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
                case "cargar-info":
                    Push(new { tipo = "info", local = _versionLocal, remota = _versionRemota });
                    break;
                case "actualizar":
                    _ = AplicarActualizacionAsync();
                    break;
                case "salir":
                    Close();
                    break;
                default:
                    ErrorLogger.RegistrarMensaje("Actualizacion", "Acción desconocida desde la página: " + accion);
                    break;
            }
        }

        private async Task AplicarActualizacionAsync()
        {
            var progreso = new Progress<Actualizador.ProgresoActualizacion>(p =>
                Push(new { tipo = "progreso", porcentaje = p.Porcentaje, estado = p.Estado }));

            try
            {
                // En el camino exitoso, Actualizador.DescargarYAplicarAsync termina el
                // proceso (Environment.Exit) antes de volver aquí.
                await Actualizador.DescargarYAplicarAsync(_zipUrl, _versionRemota, progreso);
            }
            catch (Actualizador.ActualizacionException ex)
            {
                Push(new { tipo = "error", mensaje = ex.Message, cancelado = ex.Cancelado });
            }
            catch (Exception ex)
            {
                ErrorLogger.Registrar(ex, "FormActualizacionWeb.Aplicar");
                Push(new { tipo = "error", mensaje = "Ocurrió un error inesperado al actualizar: " + ex.Message });
            }
        }
    }
}
