// Plano de sembrado en su propia ventana. Salio del panel principal
// (panel.html) cuando ese paso a ser un tablero: el plano pesaba 706 KB de
// imagen y se llevaba la pantalla completa, asi que ahora vive aparte y el
// panel solo conserva una miniatura con boton para abrir esta ventana.
//
// Hospeda Calandria.Api/ui/sembrado.html, mismo patron que FormAvanceMasivoWeb.
// No se abre con ShowDialog() (ver reference_webview2_multi_instance: el panel
// principal tambien hospeda un WebView2 y dos bucles modales se cuelgan).
// Expone CasaSeleccionada para que el panel siga la casa que se elija aqui.
using System;
using System.Collections.Generic;
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
    public class FormSembradoWeb : Form
    {
        /// <summary>Casa elegida en el plano (manzana, lote). La escucha el panel.</summary>
        public event EventHandler<CasaPlanoEventArgs> CasaSeleccionada;

        /// <summary>La pagina ya cargo y puede recibir el push de casas.</summary>
        public event EventHandler PlanoListo;

        private WebView2 webPlano;
        private bool listo;

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "sembrado.html");

        public FormSembradoWeb()
        {
            Text = "Plano de sembrado - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1180, 800);
            MinimumSize = new Size(820, 560);
            BackColor = Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webPlano = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webPlano);

            webPlano.CoreWebView2InitializationCompleted += WebPlano_Init;
            _ = IniciarWebView2Async();
        }

        /// <summary>
        /// Carpeta de datos propia: varias ventanas WebView2 del mismo proceso no
        /// comparten el entorno implicito (ver reference_webview2_multi_instance).
        /// </summary>
        private async Task IniciarWebView2Async()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2Sembrado");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webPlano.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("SembradoWeb", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo abrir el plano de sembrado:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
            }
        }

        private void WebPlano_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("SembradoWeb", "WebView2 no inicializo: " + e.InitializationException);
                MessageBox.Show("No se pudo abrir el plano de sembrado.", "Pilaris",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            var core = webPlano.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebPlano_Mensaje;

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("SembradoWeb", "Fallo cargando la pagina: " + ex.Message); }

            if (IsDisposed || webPlano?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar el plano de sembrado." + Environment.NewLine + Environment.NewLine +
                    "Verifica la conexion con el servidor.", "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            webPlano.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/sembrado/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/sembrado");
                if (datos != null && datos.Length > 0)
                {
                    string html = Encoding.UTF8.GetString(datos);
                    try { Directory.CreateDirectory(CacheDir); File.WriteAllText(CacheHtml, html, new UTF8Encoding(false)); }
                    catch { /* la cache es un extra; si falla, seguimos */ }
                    return html;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("SembradoWeb", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        /// <summary>
        /// Las mismas casas que ya arma el panel (coordenadas + progreso real).
        /// No se recalculan aqui: el panel es el unico que le pregunta al API.
        /// </summary>
        public void EnviarCasas(object casas, string manzanaActual, string loteActual)
        {
            if (!listo || webPlano?.CoreWebView2 == null) return;
            try
            {
                webPlano.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new
                {
                    tipo = "datos",
                    casas = casas,
                    manzana = manzanaActual,
                    lote = loteActual
                }));
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("SembradoWeb", "Fallo al enviar las casas al plano: " + ex.Message);
            }
        }

        /// <summary>Paleta de acento de la obra activa (cosmetico).</summary>
        public void EnviarTema(object tema)
        {
            if (!listo || webPlano?.CoreWebView2 == null || tema == null) return;
            try { webPlano.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(tema)); }
            catch { /* cosmetico */ }
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MensajePlano
        {
            public string accion;
            public string manzana;
            public string lote;
        }

        private void WebPlano_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            MensajePlano m;
            try { m = JsonConvert.DeserializeObject<MensajePlano>(e.WebMessageAsJson); }
            catch { return; }
            if (m?.accion == null) return;

            // BeginInvoke: no despachar dentro del propio callback COM de WebView2.
            BeginInvoke((Action)(() =>
            {
                if (m.accion == "plano-listo")
                {
                    listo = true;
                    PlanoListo?.Invoke(this, EventArgs.Empty);
                }
                else if (m.accion == "seleccion" && !string.IsNullOrEmpty(m.manzana) && !string.IsNullOrEmpty(m.lote))
                {
                    CasaSeleccionada?.Invoke(this, new CasaPlanoEventArgs(m.manzana, m.lote));
                }
            }));
        }
    }

    public class CasaPlanoEventArgs : EventArgs
    {
        public string Manzana { get; }
        public string Lote { get; }

        public CasaPlanoEventArgs(string manzana, string lote)
        {
            Manzana = manzana;
            Lote = lote;
        }
    }
}
