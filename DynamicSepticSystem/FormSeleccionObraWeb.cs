// Selección de obra migrada a WebView2: hospeda Calandria.Api/ui/obra.html,
// mismo patrón que FormLogin.cs/FormTrabajadoresWeb.cs. Aparece tras el login
// (o al cambiar de obra desde PanelPrincipal) y fija Global.ObraActualId antes
// de continuar.
//
// No se abre con ShowDialog() (ver reference_webview2_multi_instance: un
// WebView2 dentro de un bucle modal puede colgar el proceso). Expone el
// evento ObraSeleccionada; quien lo use debe llamar Show() y suscribirse.
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
    public class FormSeleccionObraWeb : Form
    {
        /// <summary>Se dispara cuando el usuario elige una obra (Global.ObraActualId ya está listo).</summary>
        public event EventHandler ObraSeleccionada;

        /// <summary>True si se llegó a elegir una obra antes de cerrar este formulario.</summary>
        public bool SeSeleccionoObra { get; private set; }

        private WebView2 webObra;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "obra.html");

        public FormSeleccionObraWeb()
        {
            Text = "Selección de obra - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 720);
            MinimumSize = new Size(900, 600);
            BackColor = Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webObra = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webObra);

            webObra.CoreWebView2InitializationCompleted += WebObra_Init;
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
                    "Calandria", "WebView2Obra");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webObra.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("SeleccionObra", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo iniciar la pantalla de selección de obra:\n\n" + ex.Message,
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WebObra_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("SeleccionObra", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar la pantalla de selección de obra.", "Pilaris",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var core = webObra.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebObra_Mensaje;

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("SeleccionObra", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webObra?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show(
                    "No se pudo descargar la interfaz de selección de obra.\n\nVerifica la conexión con el servidor.",
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            webObra.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/obra/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/obra");
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
                ErrorLogger.RegistrarMensaje("SeleccionObra", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webObra?.CoreWebView2 == null) return;
            try { webObra.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("SeleccionObra", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgElegirObra { public int id; public string nombre; }
        private class MsgCrearObra { public string nombre; }
        private class MsgEliminarObra { public int id; public string nombreConfirmacion; }
        private class MsgGuardarLogo { public int id; public string logoBase64; public string extension; }

        private void WebObra_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
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
                case "cargar-obras":
                    CargarObras();
                    break;
                case "elegir-obra":
                    { var d = JsonConvert.DeserializeObject<MsgElegirObra>(json); ElegirObra(d.id, d.nombre); }
                    break;
                case "crear-obra":
                    { var d = JsonConvert.DeserializeObject<MsgCrearObra>(json); CrearObra(d.nombre); }
                    break;
                case "eliminar-obra":
                    { var d = JsonConvert.DeserializeObject<MsgEliminarObra>(json); EliminarObra(d.id, d.nombreConfirmacion); }
                    break;
                case "guardar-logo":
                    { var d = JsonConvert.DeserializeObject<MsgGuardarLogo>(json); GuardarLogo(d.id, d.logoBase64, d.extension); }
                    break;
                default:
                    ErrorLogger.RegistrarMensaje("SeleccionObra", "Acción desconocida desde la página: " + accion);
                    break;
            }
        }

        private void CargarObras()
        {
            try
            {
                var lista = ApiClient.Get<System.Collections.Generic.List<ObraApi>>("/api/obras");
                Push(new { tipo = "obras", lista = lista ?? new System.Collections.Generic.List<ObraApi>() });
                Push(new { tipo = "permisos", puedeCrear = Global.UsuarioActual != null && Global.UsuarioActual.TienePermiso("sistema.obras") });
            }
            catch (Exception ex)
            {
                ErrorLogger.Registrar(ex, "FormSeleccionObraWeb.CargarObras");
                Push(new { tipo = "obras", lista = new System.Collections.Generic.List<ObraApi>() });
            }
        }

        private void ElegirObra(int id, string nombre)
        {
            Global.ObraActualId = id;
            Global.ObraActualNombre = nombre;
            SeSeleccionoObra = true;
            ObraSeleccionada?.Invoke(this, EventArgs.Empty);
        }

        private void CrearObra(string nombre)
        {
            try
            {
                ApiClient.Post<CrearObraResponseApi>("/api/obras", new { nombre });
                Push(new { tipo = "obraCreada" });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Push(new { tipo = "obraError", mensaje = "Ya existe una obra con ese nombre." });
            }
            catch (Exception ex)
            {
                ErrorLogger.Registrar(ex, "FormSeleccionObraWeb.CrearObra");
                Push(new { tipo = "obraError", mensaje = "No se pudo crear la obra. Verifica tu conexión." });
            }
        }

        private void EliminarObra(int id, string nombreConfirmacion)
        {
            try
            {
                ApiClient.Post($"/api/obras/{id}/eliminar", new { nombreConfirmacion });
                Push(new { tipo = "obraEliminada" });
                CargarObras();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "obraEliminarError", mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                ErrorLogger.Registrar(ex, "FormSeleccionObraWeb.EliminarObra");
                Push(new { tipo = "obraEliminarError", mensaje = "No se pudo eliminar la obra. Verifica tu conexión." });
            }
        }

        private void GuardarLogo(int id, string logoBase64, string extension)
        {
            try
            {
                var tema = ApiClient.Post<TemaObraApi>($"/api/obras/{id}/personalizacion",
                    new { logoBase64, extension });
                Push(new
                {
                    tipo = "logoGuardado",
                    colorPrimario = tema?.ColorPrimario,
                    colorSecundario = tema?.ColorSecundario,
                    colorSuave = tema?.ColorSuave
                });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "logoError", mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                ErrorLogger.Registrar(ex, "FormSeleccionObraWeb.GuardarLogo");
                Push(new { tipo = "logoError", mensaje = "No se pudo guardar el logo. Verifica tu conexión." });
            }
        }

        private static string MensajeDe(ApiException ex)
        {
            try
            {
                var cuerpo = JsonConvert.DeserializeAnonymousType(ex.Cuerpo, new { Message = "" });
                return string.IsNullOrEmpty(cuerpo?.Message) ? ex.Cuerpo : cuerpo.Message;
            }
            catch { return ex.Cuerpo; }
        }
    }
}
