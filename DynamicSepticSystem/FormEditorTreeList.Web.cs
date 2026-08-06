// Editor de Tareas hibrido: la pagina hospedada en WebView2 (Calandria.Api/ui/editor-tareas.html)
// sustituye el TreeListView clasico; el resto del formulario clasico (arbol, dialogos,
// gestor de insumos) queda oculto detras, igual que FormActivarTareasTreeList.Web.cs.
//
// Toda lectura/escritura de esta pantalla pasa por api/editor-tareas
// (EditorTareasController): nada de SQL directo desde este puente. El guardado es
// UN SOLO POST atomico (arbol completo + definiciones de columnas); el resto de la
// edicion (agregar/renombrar/mover/eliminar nodos, columnas, insumos) vive en memoria
// dentro de la propia pagina hasta que el usuario pulsa Guardar.
//
// Se puede apagar sin recompilar con EditorTareasWeb=false en App.config.
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public partial class FormEditorTreeList
    {
        private WebView2 webPanel;
        private readonly List<Control> ocultosPorPanelWeb = new List<Control>();

        /// <summary>Estado de "sin guardar" del editor WEB, separado de cambiosPendientes (que sólo aplica al árbol clásico en memoria).</summary>
        private bool _webDirty;

        // ProcessDictionaryKeys=false: NodoEditorApi.Valores es Dictionary<string,string> con
        // nombres de columna reales como clave (p. ej. "Duración") — el resolver camelCase
        // por defecto también camelCasea claves de diccionario, desincronizándolas de
        // ColumnaDefApi.Nombre (que llega intacto porque es un valor, no un nombre de propiedad).
        private static readonly JsonSerializerSettings CamelCaseSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(processDictionaryKeys: false, overrideSpecifiedNames: true)
            }
        };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "editor-tareas.html");

        private static bool EditorTareasWebActivo
        {
            get
            {
                var v = ConfigurationManager.AppSettings["EditorTareasWeb"];
                return string.IsNullOrWhiteSpace(v) || v.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        // ------------------------------------------------------------------
        // Montaje
        // ------------------------------------------------------------------

        private void InicializarPanelWeb()
        {
            if (!EditorTareasWebActivo) return;

            try
            {
                CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (WebView2RuntimeNotFoundException)
            {
                ErrorLogger.RegistrarMensaje("EditorTareasWeb", "WebView2 no está instalado; se usa el formulario clásico.");
                return;
            }
            catch { return; }

            ocultosPorPanelWeb.Clear();
            foreach (Control c in this.Controls)
                if (c.Visible) ocultosPorPanelWeb.Add(c);

            webPanel = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            this.Controls.Add(webPanel);
            webPanel.BringToFront();

            foreach (var c in ocultosPorPanelWeb) c.Visible = false;

            webPanel.CoreWebView2InitializationCompleted += WebPanel_Init;
            _ = IniciarWebView2Async();
        }

        /// <summary>
        /// Este formulario se abre sin ser modal, sobre PanelPrincipal, que ya tiene
        /// su propio WebView2 activo desde el arranque. Compartir el entorno
        /// implícito entre dos controles del mismo proceso puede fallar con
        /// COMException (ver reference_webview2_multi_instance) y tumbar TODO el
        /// proceso si la excepción no se observa. Por eso: entorno propio con su
        /// propia carpeta de datos, y todo el arranque en try/catch para degradar
        /// siempre al formulario clásico.
        /// </summary>
        private async Task IniciarWebView2Async()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2EditorTareas");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webPanel.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("EditorTareasWeb", "WebView2 no pudo iniciar: " + ex.Message);
                if (!IsDisposed) RestaurarPanelClasico();
            }
        }

        private void WebPanel_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("EditorTareasWeb", "WebView2 no inicializó: " + e.InitializationException);
                RestaurarPanelClasico();
                return;
            }

            var core = webPanel.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            // La página pide sus datos ("cargar-ruta") apenas corre su <script>, sin
            // esperar NavigationCompleted — igual que Destajos.
            core.WebMessageReceived += WebPanel_Mensaje;
            core.NavigationCompleted += (s2, e2) =>
            {
                if (e2.IsSuccess) _ = CargarTemaObraAsync();
            };

            _ = CargarHtmlAsync();
        }

        /// <summary>
        /// ObtenerHtml() hace HTTP síncrono (ApiClient usa GetAwaiter().GetResult()
        /// por dentro); llamarlo directo desde WebPanel_Init lo bloquearía en el hilo
        /// de UI dentro del propio callback COM de WebView2. Se manda a un hilo de
        /// fondo y sólo se vuelve al hilo de UI para navegar.
        /// </summary>
        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("EditorTareasWeb", "Fallo cargando la página: " + ex.Message);
            }

            if (IsDisposed || webPanel?.CoreWebView2 == null) return;
            if (html == null) { RestaurarPanelClasico(); return; }
            webPanel.CoreWebView2.NavigateToString(html);
        }

        private void RestaurarPanelClasico()
        {
            if (webPanel != null) webPanel.Visible = false;
            foreach (var c in ocultosPorPanelWeb) c.Visible = true;
        }

        // ------------------------------------------------------------------
        // Obtención del HTML: API -> caché (idéntico patrón que FormActivarTareasTreeList.Web.cs)
        // ------------------------------------------------------------------

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
                var ver = ApiClient.Get<VersionUi>("/api/ui/editor-tareas/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/editor-tareas");
                if (datos != null && datos.Length > 0)
                {
                    string html = Encoding.UTF8.GetString(datos);
                    try
                    {
                        Directory.CreateDirectory(CacheDir);
                        File.WriteAllText(CacheHtml, html, new UTF8Encoding(false));
                    }
                    catch { /* la caché es un extra; si falla, seguimos */ }
                    return html;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("EditorTareasWeb", "No se pudo traer la UI del API: " + ex.Message);
            }

            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private static bool EsUsuarioAdmin()
        {
            return Global.EsAdmin;
        }

        /// <summary>GET del árbol de <see cref="nombreTabla"/> y envío a la página. La llamada bloqueante corre en Task.Run; PostWebMessageAsJson siempre vuelve al hilo de UI.</summary>
        private async Task CargarYEnviarArbolAsync()
        {
            ArbolEditorApi arbol;
            try
            {
                string ruta = nombreTabla;
                arbol = await Task.Run(() => ApiClient.Get<ArbolEditorApi>(
                    "/api/editor-tareas/arbol?ruta=" + Uri.EscapeDataString(ruta)));
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el árbol"); return; }

            if (webPanel?.CoreWebView2 == null || arbol == null) return;

            var carga = new
            {
                tipo = "datos",
                ruta = arbol.Ruta ?? nombreTabla,
                nodos = arbol.Nodos ?? new List<NodoEditorApi>(),
                columnas = arbol.Columnas ?? new List<ColumnaDefApi>(),
                esAdmin = EsUsuarioAdmin()
            };
            webPanel.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(carga, CamelCaseSettings));
        }

        private void PushError(string mensaje)
        {
            if (webPanel?.CoreWebView2 == null) return;
            try
            {
                webPanel.CoreWebView2.PostWebMessageAsJson(
                    JsonConvert.SerializeObject(new { tipo = "error", mensaje = mensaje }, CamelCaseSettings));
            }
            catch { /* si ni el aviso de error sale, no hay más que intentar */ }
        }

        /// <summary>Manda la paleta de acento de la obra activa (cosmético, nunca bloquea si falla).</summary>
        private async Task CargarTemaObraAsync()
        {
            object msg = null;
            try { msg = await Task.Run(() => TemaObra.ObtenerParaPush()); }
            catch { /* cosmético */ }
            if (IsDisposed || msg == null || webPanel?.CoreWebView2 == null) return;
            webPanel.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(msg, CamelCaseSettings));
        }

        private void ManejarErrorApi(Exception ex, string contexto)
        {
            var apiEx = ex as ApiException;
            string msg = apiEx != null ? MensajeDe(apiEx) : ex.Message;
            ErrorLogger.RegistrarMensaje("EditorTareasWeb", contexto + ": " + msg);
            MessageBox.Show(contexto + ":\n\n" + msg, "Editor de Tareas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            PushError(msg);
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

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MensajeWebEditorTareas
        {
            public string accion;
            public string ruta;
            public bool? valor;
            public List<NodoEditorApi> nodos;
            public List<ColumnaDefApi> columnas;
        }

        private void WebPanel_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            MensajeWebEditorTareas m;
            try { m = JsonConvert.DeserializeObject<MensajeWebEditorTareas>(e.WebMessageAsJson); }
            catch { return; }
            if (m?.accion == null) return;

            // No despachar aquí mismo: este método corre DENTRO de la llamada COM
            // entrante de WebView2 (ver FormActivarTareasTreeList.Web.cs / referencia
            // de multi-instancia WebView2). BeginInvoke lo manda a una vuelta de
            // mensajes nueva, fuera del stack de WebView2.
            BeginInvoke((Action)(() => DespacharMensaje(m)));
        }

        private async void DespacharMensaje(MensajeWebEditorTareas m)
        {
            try
            {
                switch (m.accion)
                {
                    case "cargar-ruta":
                        if (!string.IsNullOrWhiteSpace(m.ruta)) nombreTabla = m.ruta;
                        await CargarYEnviarArbolAsync();
                        break;

                    case "guardar":
                        await GuardarWeb(m);
                        break;

                    case "dirty":
                        _webDirty = m.valor ?? false;
                        break;

                    case "volver":
                        RestaurarPanelClasico();
                        break;

                    default:
                        ErrorLogger.RegistrarMensaje("EditorTareasWeb", "Acción desconocida desde la página: " + m.accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                ManejarErrorApi(ex, "No se pudo completar la acción solicitada");
            }
        }

        /// <summary>POST atómico (árbol + columnas) — [ADMIN] se revalida en el servidor.</summary>
        private async Task GuardarWeb(MensajeWebEditorTareas m)
        {
            try
            {
                string ruta = !string.IsNullOrWhiteSpace(m.ruta) ? m.ruta : nombreTabla;
                var resultado = await Task.Run(() => ApiClient.Post<GuardarArbolResponseApi>("/api/editor-tareas/guardar", new
                {
                    Ruta = ruta,
                    Nodos = m.nodos ?? new List<NodoEditorApi>(),
                    Columnas = m.columnas ?? new List<ColumnaDefApi>()
                }));

                _webDirty = false;
                if (webPanel?.CoreWebView2 == null) return;

                var carga = new
                {
                    tipo = "guardado",
                    mensaje = resultado?.Mensaje ?? "Guardado correctamente.",
                    idsReasignados = resultado?.IdsReasignados ?? new Dictionary<int, int>()
                };
                webPanel.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(carga, CamelCaseSettings));
            }
            catch (Exception ex)
            {
                ManejarErrorApi(ex, "No se pudo guardar");
            }
        }
    }
}
