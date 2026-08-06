using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Pantalla principal WEB: la página hospedada en WebView2 ocupa la ventana
    /// completa —menú incluido— y el shell WinForms queda oculto detrás. Las 30
    /// pantallas existentes se siguen abriendo como formularios, invocadas desde
    /// el puente; lo único que desaparece es el panel principal de WinForms.
    ///
    /// El HTML lo sirve Calandria.Api (GET api/ui/panel), así que la interfaz se
    /// actualiza copiando un archivo en el servidor, sin reinstalar el cliente. Lo
    /// descarga ESTE lado con ApiClient —que ya lleva el JWT— y se inyecta con
    /// NavigateToString: el token nunca entra a la página y no hace falta CORS.
    /// Si el API no responde, se usa la última copia en caché.
    ///
    /// Se puede desactivar sin recompilar con PanelWeb=false en App.config, que
    /// devuelve el panel WinForms de siempre. Desde la página también se puede
    /// volver a él en caliente (acción "panel-clasico").
    /// </summary>
    public partial class PanelPrincipal
    {
        private WebView2 webPanel;
        private bool webPanelListo;

        /// <summary>Controles del shell WinForms que el panel web tapó, para poder restaurarlos.</summary>
        private readonly List<Control> ocultosPorPanelWeb = new List<Control>();

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "panel.html");

        private static bool PanelWebActivo
        {
            get
            {
                var v = ConfigurationManager.AppSettings["PanelWeb"];
                return string.IsNullOrWhiteSpace(v) || v.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        // ------------------------------------------------------------------
        // Montaje
        // ------------------------------------------------------------------

        /// <summary>
        /// Sustituye TODO el contenido de la ventana por el WebView2: menú, barra
        /// lateral y panel central de WinForms se ocultan y la página ocupa el
        /// área completa. Se llama desde Form1_Load. Cualquier fallo deja el panel
        /// WinForms intacto: la UI nueva no debe poder dejar la app inservible.
        /// </summary>
        private void InicializarPanelWeb()
        {
            if (!PanelWebActivo) return;

            try
            {
                CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (WebView2RuntimeNotFoundException)
            {
                // Sin runtime no hay panel web; se queda el de siempre y se avisa
                // una sola vez en el log, sin diálogo que estorbe el arranque.
                ErrorLogger.RegistrarMensaje("PanelWeb", "WebView2 no está instalado; se usa el panel WinForms clásico.");
                return;
            }
            catch { return; }

            // Se anota qué estaba visible antes de taparlo: así "volver al panel
            // clásico" restaura exactamente el estado previo y no revive controles
            // que ya estaban ocultos (menuStripGeneral, panelDevTools…).
            ocultosPorPanelWeb.Clear();
            foreach (Control c in this.Controls)
                if (c.Visible) ocultosPorPanelWeb.Add(c);

            webPanel = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(237, 235, 231)
            };
            this.Controls.Add(webPanel);
            webPanel.BringToFront();

            foreach (var c in ocultosPorPanelWeb) c.Visible = false;

            webPanel.CoreWebView2InitializationCompleted += WebPanel_Init;
            _ = IniciarWebView2PanelAsync();
        }

        /// <summary>
        /// Entorno propio con carpeta de datos en AppData: EnsureCoreWebView2Async(null)
        /// usa el entorno implícito, que crea su carpeta de datos junto al .exe —
        /// si la app está instalada en Program Files (sin permisos de escritura
        /// para el usuario normal), WebView2 falla con "We couldn't create the
        /// data directory" fuera del entorno de depuración. Mismo patrón que
        /// FormAlmacen.Web.cs / FormComprasWeb.cs / FormTrabajadoresWeb.cs.
        /// </summary>
        private async Task IniciarWebView2PanelAsync()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2Panel");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webPanel.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("PanelWeb", "WebView2 no pudo iniciar: " + ex.Message);
                RestaurarPanelClasico();
            }
        }

        private void WebPanel_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("PanelWeb", "WebView2 no inicializó: " + e.InitializationException);
                RestaurarPanelClasico();
                return;
            }

            var core = webPanel.CoreWebView2;

            // Menú contextual y DevTools fuera: esto es una pantalla de la app, no
            // un navegador. DevTools se deja si hay depurador conectado.
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebPanel_Mensaje;
            core.NavigationCompleted += (s2, e2) =>
            {
                if (e2.IsSuccess) { webPanelListo = true; EnviarDatosAlPanel(); _ = EnviarTemaAlPanel(); }
            };

            string html = ObtenerHtml();
            if (html == null) { RestaurarPanelClasico(); return; }
            core.NavigateToString(html);
        }

        /// <summary>
        /// Devuelve la ventana al shell WinForms de siempre. Se usa como red de
        /// seguridad si el panel web falla, y también a petición del usuario desde
        /// la propia página.
        /// </summary>
        private void RestaurarPanelClasico()
        {
            if (webPanel != null) webPanel.Visible = false;
            foreach (var c in ocultosPorPanelWeb) c.Visible = true;
        }

        // ------------------------------------------------------------------
        // Obtención del HTML: API -> caché
        // ------------------------------------------------------------------

        private static string Sha256(byte[] d)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(d)).Replace("-", "").ToLowerInvariant();
        }

        private class VersionUi { public string hash; public int bytes; }

        /// <summary>
        /// Devuelve el HTML del panel. Pregunta al API por el hash; si coincide con
        /// la copia en caché no descarga nada. Si el API no responde, usa la caché.
        /// Devuelve null si no hay ninguna de las dos.
        /// </summary>
        private static string ObtenerHtml()
        {
            string cache = File.Exists(CacheHtml) ? File.ReadAllText(CacheHtml, Encoding.UTF8) : null;

            try
            {
                var ver = ApiClient.Get<VersionUi>("/api/ui/panel/version");
                if (cache != null && ver != null && ver.hash ==
                    Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;   // sin cambios

                var datos = ApiClient.GetBytes("/api/ui/panel");
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
                ErrorLogger.RegistrarMensaje("PanelWeb", "No se pudo traer la UI del API: " + ex.Message);
            }

            return cache;   // puede ser null
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private class CasaWeb
        {
            public string mz;
            public int lote;
            public float x;
            public float y;
            public string proto;
            // --- Progreso real (api/destajos/resumen-casas); ver AplicarResumenReal ---
            public string estado;
            public int avance;
            public int total;
            public int comp;
            public decimal ejec;
            public decimal presup;
            public int dias;
        }

        /// <summary>
        /// Empuja al panel las casas con sus coordenadas reales
        /// (coordenadas_mapa.json, el mismo origen que usa el mapa clásico) y su
        /// progreso real (ActivacionTareasRuta, vía api/destajos/resumen-casas).
        /// </summary>
        private void EnviarDatosAlPanel()
        {
            if (!webPanelListo || webPanel?.CoreWebView2 == null) return;

            try
            {
                var lista = LeerCoordenadasParaWeb();
                AplicarResumenReal(lista);
                var carga = new
                {
                    tipo = "datos",
                    usuario = Global.UsuarioActual?.Nombre ?? "",
                    rol = Global.UsuarioActual?.Perfil ?? "",
                    // La página esconde las opciones [ADMIN] con esto; el switch de
                    // WebPanel_Mensaje las vuelve a validar (la página no es confiable).
                    esAdmin = Global.EsAdmin,
                    casas = lista
                };
                webPanel.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(carga));
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("PanelWeb", "Fallo al enviar datos al panel web: " + ex.Message);
            }
        }

        /// <summary>Manda la paleta de acento de la obra activa (cosmético, nunca bloquea si falla).</summary>
        private async Task EnviarTemaAlPanel()
        {
            object msg = null;
            try { msg = await Task.Run(() => TemaObra.ObtenerParaPush()); }
            catch { /* cosmético */ }
            if (msg != null && webPanel?.CoreWebView2 != null)
                webPanel.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(msg));
        }

        /// <summary>
        /// Desglose del perfil activo (api/auth/mi-perfil + foto), a petición de la
        /// página cuando el usuario hace clic en el avatar del rail.
        /// </summary>
        private async Task EnviarMiPerfilAlPanel()
        {
            object msg;
            try
            {
                msg = await Task.Run(() =>
                {
                    var perfil = ApiClient.Get<MiPerfilApi>("/api/auth/mi-perfil");
                    if (perfil == null) return null;

                    string fotoDataUri = null;
                    if (perfil.TieneFoto)
                    {
                        var bytes = ApiClient.GetBytes("/api/auth/mi-perfil/foto");
                        if (bytes != null && bytes.Length > 0)
                            fotoDataUri = $"data:{TemaObra.MimeDeExtension(perfil.FotoExtension)};base64," + Convert.ToBase64String(bytes);
                    }

                    return new
                    {
                        tipo = "miPerfil",
                        usuario = perfil.Usuario,
                        perfil = perfil.Perfil,
                        fechaAlta = perfil.FechaAlta.ToString("yyyy-MM-dd"),
                        foto = fotoDataUri,
                        permisos = perfil.Permisos?.Select(p => new { modulo = p.Modulo, etiqueta = p.Etiqueta }).ToList()
                    };
                });
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("PanelWeb", "No se pudo traer el desglose del perfil: " + ex.Message);
                msg = new { tipo = "miPerfilError", mensaje = "No se pudo cargar el perfil. Intenta de nuevo." };
            }

            if (msg != null && webPanel?.CoreWebView2 != null)
                webPanel.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(msg));
        }

        private static List<CasaWeb> LeerCoordenadasParaWeb()
        {
            string ruta = Path.Combine(Application.StartupPath, "coordenadas_mapa.json");
            if (!File.Exists(ruta)) return new List<CasaWeb>();

            var crudas = JsonConvert.DeserializeObject<List<CoordenadasCasa>>(
                File.ReadAllText(ruta, Encoding.UTF8)) ?? new List<CoordenadasCasa>();

            return crudas
                .Where(c => !string.IsNullOrWhiteSpace(c.Manzana) && !string.IsNullOrWhiteSpace(c.Lote))
                .Select(c => new CasaWeb
                {
                    mz = c.Manzana,
                    lote = int.TryParse(c.Lote, out int l) ? l : 0,
                    x = c.X,
                    y = c.Y,
                    proto = "",
                    estado = "idle"
                })
                .Where(c => c.lote > 0)
                .ToList();
        }

        /// <summary>
        /// Rellena cada CasaWeb con su progreso real. Si el API no responde, las
        /// casas se quedan en "idle"/0 (nunca con datos inventados).
        /// </summary>
        private static void AplicarResumenReal(List<CasaWeb> lista)
        {
            try
            {
                var resumen = ApiClient.Get<List<ResumenCasaApi>>("/api/destajos/resumen-casas")
                    ?? new List<ResumenCasaApi>();
                var porCasa = resumen.ToDictionary(r => (r.Manzana?.Trim(), r.Lote?.Trim()));

                foreach (var c in lista)
                {
                    if (!porCasa.TryGetValue((c.mz, c.lote.ToString()), out var r))
                        continue;

                    c.proto = r.Prototipo ?? "";
                    c.estado = r.Estado ?? "idle";
                    c.avance = r.AvancePct;
                    c.total = r.Destajos;
                    c.comp = r.Terminados;
                    c.ejec = r.ImporteTerminado;
                    c.presup = r.ImporteTotal;
                    c.dias = r.UltimaActualizacion.HasValue
                        ? Math.Max(0, (int)(DateTime.Now - r.UltimaActualizacion.Value).TotalDays)
                        : 0;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("PanelWeb", "No se pudo traer el progreso real de las casas: " + ex.Message);
            }
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MensajeWeb
        {
            public string accion;
            public string manzana;
            public string lote;
        }

        /// <summary>
        /// Traduce las acciones de la página a las pantallas WinForms que ya
        /// existen. La casa seleccionada se propaga por los mismos campos que usa
        /// el panel clásico, para que las pantallas no noten la diferencia.
        /// </summary>
        private void WebPanel_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            MensajeWeb m;
            try { m = JsonConvert.DeserializeObject<MensajeWeb>(e.WebMessageAsJson); }
            catch { return; }
            if (m?.accion == null) return;

            try
            {
                if (!string.IsNullOrWhiteSpace(m.manzana)) txtManzana.Text = m.manzana;
                if (!string.IsNullOrWhiteSpace(m.lote)) txtLote.Text = m.lote;

                // Las opciones [ADMIN] se validan aquí además de ocultarse en la
                // página: el HTML llega del servidor, pero no manda sobre permisos.
                if ((m.accion == "hard-progress" || m.accion == "mapear-coordenadas" ||
                     m.accion == "log-errores" || m.accion == "editar-explosiones") && !Global.EsAdmin)
                {
                    MessageBox.Show("Esta opción es sólo para el administrador.",
                        "Permisos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                switch (m.accion)
                {
                    // --- plano ---
                    case "seleccion":
                        // Reutiliza la búsqueda del panel clásico para dejar
                        // casaActual y las etiquetas coherentes.
                        btnBuscarCasa_Click(this, EventArgs.Empty);
                        break;

                    // --- COMPRAS ---
                    case "compra-multiple":     AbrirFormCompraMulti();            break;
                    case "compra-indirecta":    AbrirFormCompraIndirecta();        break;
                    case "consultar-ordenes":   AbrirRepositorioOrdenesCompra();   break;

                    // --- ALMACÉN ---
                    case "almacen":             AbrirFormAlmacen();                break;

                    // --- OBRA ---
                    case "destajos":            AbrirFormActivarTareasTreeList();  break;
                    case "editor-tareas":       AbrirFormEditorTreeList();         break;
                    case "avance-partidas":     AbrirFormAvanceObra();             break;
                    case "avance-conceptos":    AbrirFormAvanceConcepto();         break;
                    case "estimacion":          AbrirFormEstimacionConcepto();     break;
                    case "ruta-critica":        AbrirFormRutaCritica();            break;
                    case "editar-explosiones":  AbrirFormEditarExplosiones();      break;
                    case "hard-progress":       AbrirFormHardProgress();           break;
                    case "mapear-coordenadas":  AbrirFormMapearCoordenadas();      break;

                    // --- EVIDENCIAS ---
                    case "evidencias":          AbrirFormEvidencias();             break;

                    // --- PERSONAL ---
                    case "trabajadores":        AbrirFormRegistrarTrabajador();    break;
                    case "cuadrillas":          AbrirFormGestionCuadrillas();      break;
                    case "perfiles":            AbrirFormPerfilTrabajador();       break;

                    // --- ADMINISTRATIVOS ---
                    case "administrativos":     AbrirFormAdministrativos();        break;
                    case "log-errores":         AbrirFormLogErrores();             break;
                    case "diagnostico":         btnDiagnosticoConexion_Click(this, EventArgs.Empty); break;
                    case "perfiles-permisos":   btnGestionPerfiles_Click(this, EventArgs.Empty); break;

                    // --- sesión / escape ---
                    case "mi-perfil":           _ = EnviarMiPerfilAlPanel();       break;
                    case "panel-clasico":       RestaurarPanelClasico();           break;
                    case "cerrar-sesion":
                        btnCerrarSesion_Click(this, EventArgs.Empty);
                        // Si entró otro usuario, la página debe reflejarlo (nombre,
                        // rol y opciones de administrador).
                        if (!IsDisposed) EnviarDatosAlPanel();
                        break;

                    default:
                        ErrorLogger.RegistrarMensaje("PanelWeb", "Acción desconocida desde la página: " + m.accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir la pantalla solicitada:\n\n" + ex.Message,
                    "Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
