// Destajos hibrido: la pagina hospedada en WebView2 (Calandria.Api/ui/destajos.html)
// sustituye el panel paso-a-paso (PanelPasos.cs); el resto del formulario clasico
// queda oculto detras, igual que PanelPrincipal.Web.cs hace con el panel principal.
//
// Toda lectura/escritura de esta pantalla pasa por api/destajos (DestajosController):
// nada de SQL directo desde este puente. Los dialogos que ya existian y no
// necesitaban migrar (FormAsignarCuadrilla, FormAsignarNomina, PromptJustificacion,
// el PDF con PdfSharp) se reutilizan tal cual.
//
// Se puede apagar sin recompilar con DestajosWeb=false en App.config.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ClosedXML.Excel;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    public partial class FormActivarTareasTreeList
    {
        private WebView2 webPanel;
        private readonly List<Control> ocultosPorPanelWeb = new List<Control>();

        /// <summary>Ultimo arbol cargado; lo usan las acciones para no repetir el GET.</summary>
        private ArbolDestajosApi _arbolActual;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "destajos.html");

        private static bool DestajosWebActivo
        {
            get
            {
                var v = ConfigurationManager.AppSettings["DestajosWeb"];
                return string.IsNullOrWhiteSpace(v) || v.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        // ------------------------------------------------------------------
        // Montaje
        // ------------------------------------------------------------------

        private void InicializarPanelWeb()
        {
            if (!DestajosWebActivo) return;

            try
            {
                CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (WebView2RuntimeNotFoundException)
            {
                ErrorLogger.RegistrarMensaje("DestajosWeb", "WebView2 no está instalado; se usa el formulario clásico.");
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
        /// Este formulario se abre como diálogo modal ENCIMA del panel principal,
        /// que ya tiene su propio WebView2 activo desde el arranque. Compartir el
        /// entorno implícito (EnsureCoreWebView2Async(null)) entre dos controles
        /// del mismo proceso puede fallar con COMException en el SDK de WebView2
        /// que usa el proyecto (1.0.4078.44) — y como antes era una llamada
        /// "fire-and-forget" sin observar, esa excepción tumbaba TODO el proceso
        /// en vez de caer al formulario clásico. Por eso: (1) entorno propio, con
        /// su propia carpeta de datos de usuario, para no competir con el del
        /// panel; (2) todo el arranque envuelto en try/catch, para que cualquier
        /// fallo de WebView2 —cualquiera que sea la causa— caiga al formulario
        /// clásico en vez de crashear.
        /// </summary>
        private async Task IniciarWebView2Async()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2Destajos");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webPanel.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("DestajosWeb", "WebView2 no pudo iniciar: " + ex.Message);
                if (!IsDisposed) RestaurarPanelClasico();
            }
        }

        private void WebPanel_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("DestajosWeb", "WebView2 no inicializó: " + e.InitializationException);
                RestaurarPanelClasico();
                return;
            }

            var core = webPanel.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            // OJO: la página manda "cargar-manzanas" apenas corre su <script>,
            // que ocurre ANTES de que dispare NavigationCompleted (equivalente a
            // "load", no a cuando el DOM ya puede correr JS). Por eso las
            // respuestas del puente (PushSimple/PushError/EnviarDatosAlPanel) NO
            // deben esperar a NavigationCompleted: sólo necesitan CoreWebView2 no
            // nulo, que ya existe si estamos dentro de WebMessageReceived.
            core.WebMessageReceived += WebPanel_Mensaje;
            core.NavigationCompleted += (s2, e2) =>
            {
                if (e2.IsSuccess) { EnviarDatosAlPanel(); _ = CargarTemaObraAsync(); }
            };

            _ = CargarHtmlAsync();
        }

        /// <summary>
        /// ObtenerHtml() hace HTTP síncrono (ApiClient usa GetAwaiter().GetResult()
        /// por dentro). Llamarlo directo desde WebPanel_Init lo bloqueaba en el
        /// hilo de UI **dentro del propio callback COM** de WebView2 — con el
        /// diálogo modal de por medio, eso colgaba la ventana unos segundos y
        /// terminaba tumbando el proceso. Se manda a un hilo de fondo y sólo se
        /// vuelve al hilo de UI para navegar.
        /// </summary>
        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("DestajosWeb", "Fallo cargando la página: " + ex.Message);
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
        // Obtención del HTML: API -> caché (idéntico patrón que PanelPrincipal.Web.cs)
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/destajos/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/destajos");
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
                ErrorLogger.RegistrarMensaje("DestajosWeb", "No se pudo traer la UI del API: " + ex.Message);
            }

            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void EnviarDatosAlPanel()
        {
            if (webPanel?.CoreWebView2 == null || _arbolActual == null) return;
            try
            {
                var carga = new
                {
                    tipo = "datos",
                    usuario = Global.UsuarioActual?.Nombre ?? "",
                    rol = Global.UsuarioActual?.Perfil ?? "",
                    esAdmin = EsUsuarioAdmin(),
                    puedeEditar = Global.UsuarioActual?.TienePermiso("destajos.editar") ?? false,
                    arbol = _arbolActual
                };
                webPanel.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(carga, CamelCaseSettings));
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("DestajosWeb", "Fallo al enviar datos al panel web: " + ex.Message);
            }
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

        private void PushSimple(string tipo, List<string> lista)
        {
            if (webPanel?.CoreWebView2 == null) return;
            webPanel.CoreWebView2.PostWebMessageAsJson(
                JsonConvert.SerializeObject(new { tipo = tipo, lista = lista }, CamelCaseSettings));
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

        private void ManejarErrorApi(Exception ex, string contexto)
        {
            var apiEx = ex as ApiException;
            string msg = apiEx != null ? MensajeDe(apiEx) : ex.Message;
            ErrorLogger.RegistrarMensaje("DestajosWeb", contexto + ": " + msg);
            MessageBox.Show(contexto + ":\n\n" + msg, "Destajos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private class MensajeWebDestajos
        {
            public string accion;
            public string manzana;
            public string lote;
            public string ruta;
            public string prototipo;
            public int? nodoId;
            public string desde;
            public string hasta;
            public string cuadrilla;
            public string formato;
            public int[] ids;
            public ReciboMsgDto recibo;
        }

        /// <summary>Recibo tal como ya lo tiene cargado la página (viene de reporte-recibos-datos);
        /// se manda de vuelta completo porque no hay GET de un recibo suelto por Id.</summary>
        private class ReciboMsgDto
        {
            public int id;
            public string nombreTrabajador;
            public string rol;
            public string codigoCuadrilla;
            public string concepto;
            public decimal monto;
            public DateTime fechaRecibo;
            public DateTime? periodoDesde;
            public DateTime? periodoHasta;
            public bool tienePdf;
        }

        private void WebPanel_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            MensajeWebDestajos m;
            try { m = JsonConvert.DeserializeObject<MensajeWebDestajos>(e.WebMessageAsJson); }
            catch { return; }
            if (m?.accion == null) return;

            // No despachar aquí mismo: este método corre DENTRO de la llamada COM
            // entrante de WebView2. Cualquier acción que abra un diálogo (que a su
            // vez puede llamar al API en su constructor, p. ej. FormAsignarNomina)
            // o que llame al API directo (ApiClient es síncrono) bloqueaba el hilo
            // de UI mientras esa llamada COM seguía "en vuelo" — con el tiempo
            // (~1 min en pruebas) eso tumbaba el proceso entero, sin necesitar que
            // el usuario interactúe (la página pide "cargar-manzanas" sola al
            // abrir). BeginInvoke lo manda a una vuelta de mensajes nueva, ya fuera
            // del stack de WebView2 — ver reference_webview2_multi_instance.
            BeginInvoke((Action)(() => DespacharMensaje(m)));
        }

        // async void: es el punto de entrada del despacho (llamado vía BeginInvoke),
        // no hay quien lo espere. Las excepciones quedan cubiertas por el try/catch
        // de abajo, que envuelve también las esperas (await) de más adelante.
        private async void DespacharMensaje(MensajeWebDestajos m)
        {
            try
            {
                // [ADMIN] se valida aquí además de ocultarse/esconderse en la página
                // y de revalidarse otra vez en el API: la página no manda sobre permisos.
                if ((m.accion == "desactivar" || m.accion == "reabrir") && !EsUsuarioAdmin())
                {
                    MessageBox.Show("Esta opción es sólo para el administrador.",
                        "Permisos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                switch (m.accion)
                {
                    case "cargar-manzanas":     await CargarManzanasWeb();                                   break;
                    case "cargar-lotes":        await CargarLotesWeb(m.manzana);                             break;
                    case "cargar-casa":         await CargarCasaWeb(m.manzana, m.lote);                      break;
                    case "activar":
                    case "cambiar-cuadrilla":   if (m.nodoId.HasValue) await ActivarWeb(m.nodoId.Value);      break;
                    case "finalizar":           if (m.nodoId.HasValue) await FinalizarWeb(m.nodoId.Value);    break;
                    case "desactivar":          if (m.nodoId.HasValue) await DesactivarWeb(m.nodoId.Value);   break;
                    case "reabrir":             if (m.nodoId.HasValue) await ReabrirWeb(m.nodoId.Value);      break;
                    case "regenerar-pdf":       if (m.nodoId.HasValue) await RegenerarPdfWeb(m.nodoId.Value); break;
                    case "distribucion-nomina": if (m.nodoId.HasValue) DistribucionNominaWeb(m.nodoId.Value); break;
                    case "foto-reciente":       if (m.nodoId.HasValue) await FotoRecienteWeb(m.nodoId.Value); break;
                    case "insumos":             InsumosWeb();                                          break;
                    case "arbol-completo":      MostrarArbolCompleto();                                break;
                    case "reporte-semana-datos":     ReporteSemanaDatosWeb(m.desde, m.hasta);          break;
                    case "reporte-semana-pdf":       ReporteSemanaPdfWeb(m.desde, m.hasta);            break;
                    case "reporte-cuadrilla-datos":  ReporteCuadrillaDatosWeb(m.desde, m.hasta, m.manzana, m.lote); break;
                    case "reporte-cuadrilla-distribuir":
                        ReporteCuadrillaDistribuirWeb(m.cuadrilla, m.desde, m.hasta, m.manzana, m.lote);
                        break;
                    case "reporte-nomina-datos":     ReporteNominaDatosWeb(m.desde, m.hasta);          break;
                    case "reporte-nomina-pdf":       ReporteNominaPdfWeb(m.desde, m.hasta);            break;
                    case "reporte-recibos-datos":    ReporteRecibosDatosWeb(m.desde, m.hasta);         break;
                    case "reporte-recibos-abrir":       if (m.recibo != null) ReciboAbrirWeb(m.recibo);       break;
                    case "reporte-recibos-reimprimir":  if (m.recibo != null) ReciboReimprimirWeb(m.recibo);  break;
                    case "reporte-recibos-guardar":     if (m.recibo != null) ReciboGuardarComoWeb(m.recibo); break;
                    case "reporte-casa-datos":       ReporteCasaDatosWeb();                           break;
                    case "reporte-casa-excel":       ReporteCasaExcelWeb(m.formato, m.ids);           break;
                    case "repositorio-pdfs":    RepositorioPdfsWeb();                                  break;
                    case "volver":              RestaurarPanelClasico();                                break;

                    default:
                        ErrorLogger.RegistrarMensaje("DestajosWeb", "Acción desconocida desde la página: " + m.accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo completar la acción solicitada:\n\n" + ex.Message,
                    "Destajos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ------------------------------------------------------------------
        // Lecturas — todas vía api/destajos, nada de SQL directo
        // ------------------------------------------------------------------

        private async Task CargarManzanasWeb()
        {
            try
            {
                var lista = await Task.Run(() => ApiClient.Get<List<string>>("/api/destajos/manzanas")) ?? new List<string>();
                PushSimple("manzanas", lista);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las manzanas"); }
        }

        private async Task CargarLotesWeb(string manzana)
        {
            if (string.IsNullOrWhiteSpace(manzana)) return;
            try
            {
                var lista = await Task.Run(() => ApiClient.Get<List<string>>(
                    "/api/destajos/lotes?manzana=" + Uri.EscapeDataString(manzana))) ?? new List<string>();
                PushSimple("lotes", lista);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los lotes"); }
        }

        private async Task CargarCasaWeb(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote)) return;
            try
            {
                var casa = await Task.Run(() => ApiClient.Get<CasaDestajoApi>(
                    "/api/destajos/casa?manzana=" + Uri.EscapeDataString(manzana) + "&lote=" + Uri.EscapeDataString(lote)));
                if (casa == null) { PushError("No se encontró la casa M" + manzana + " L" + lote + "."); return; }

                var arbol = await Task.Run(() => ApiClient.Get<ArbolDestajosApi>(
                    "/api/destajos/arbol?manzana=" + Uri.EscapeDataString(manzana)
                    + "&lote=" + Uri.EscapeDataString(lote)
                    + "&ruta=" + Uri.EscapeDataString(casa.Ruta)));
                if (arbol == null) { PushError("No se pudo cargar el árbol de destajos de esa casa."); return; }

                _arbolActual = arbol;
                MapearItems(arbol);
                EnviarDatosAlPanel();
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar la casa"); }
        }

        /// <summary>Repite la última consulta de árbol y la vuelve a empujar a la página.</summary>
        private async Task RefrescarArbol()
        {
            try
            {
                var arbol = await Task.Run(() => ApiClient.Get<ArbolDestajosApi>(
                    "/api/destajos/arbol?manzana=" + Uri.EscapeDataString(manzanaActual)
                    + "&lote=" + Uri.EscapeDataString(loteActual)
                    + "&ruta=" + Uri.EscapeDataString(rutaActual)));
                if (arbol == null) return;
                _arbolActual = arbol;
                MapearItems(arbol);
                EnviarDatosAlPanel();
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo actualizar la información"); }
        }

        /// <summary>
        /// Foto más reciente de EvidenciasDestajo para el destajo indicado (api/evidencias-destajo).
        /// Siempre responde {tipo:"foto-destajo",...} —con fotoBase64 null si no hay o falló la
        /// consulta— para que la página nunca se quede colgada en "Cargando…"; es un dato
        /// secundario, así que un fallo aquí no interrumpe con un MessageBox.
        /// </summary>
        private async Task FotoRecienteWeb(int nodoId)
        {
            EvidenciaDestajoApi ultima = null;
            byte[] bytes = null;
            string manzana = manzanaActual, lote = loteActual, ruta = rutaActual;
            try
            {
                await Task.Run(() =>
                {
                    var lista = ApiClient.Get<List<EvidenciaDestajoApi>>(
                        "/api/evidencias-destajo?manzana=" + Uri.EscapeDataString(manzana ?? "") +
                        "&lote=" + Uri.EscapeDataString(lote ?? "") +
                        "&ruta=" + Uri.EscapeDataString(ruta ?? "") +
                        "&nodoId=" + nodoId);
                    // El listado viene ordenado ASC por fecha: la última posición es la más reciente.
                    ultima = lista?.LastOrDefault();
                    if (ultima != null)
                        bytes = ApiClient.GetBytes($"/api/evidencias-destajo/{ultima.Id}/foto");
                });
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("DestajosWeb", "No se pudo cargar la foto reciente: " + ex.Message);
            }

            if (webPanel?.CoreWebView2 == null) return;
            webPanel.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new
            {
                tipo = "foto-destajo",
                nodoId,
                fotoBase64 = bytes != null && bytes.Length > 0 ? Convert.ToBase64String(bytes) : null,
                extension = ultima?.Extension,
                descripcion = ultima?.Descripcion,
                usuario = ultima?.Usuario,
                fecha = ultima?.Fecha,
                tamanioKB = ultima?.TamanioKB
            }, CamelCaseSettings));
        }

        /// <summary>
        /// Convierte el árbol del API al modelo que ya usan el formulario clásico
        /// (itemsTareas, olvTareas, lblContextoCasa, ActualizarEstadisticas) para
        /// poder reutilizar sin cambios MostrarArbolCompleto/MostrarHistorial/PDF.
        /// </summary>
        private void MapearItems(ArbolDestajosApi arbol)
        {
            manzanaActual = arbol.Manzana ?? "";
            loteActual = arbol.Lote ?? "";
            prototipoActual = arbol.Prototipo ?? "";
            rutaActual = arbol.Ruta ?? "";

            itemsTareas = new List<ItemTareaActivacion>();
            _surtidoPorClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            _surtidoPorNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            int contador = 0;
            foreach (var n in (arbol.Nodos ?? new List<NodoDestajoApi>()).OrderBy(x => x.Nivel).ThenBy(x => x.Orden))
            {
                if (n.Nivel == 1) contador++;
                var item = ADto(n);
                item.Contador = n.Nivel == 1 ? contador : 0;
                itemsTareas.Add(item);

                if (n.Nivel == 2 && n.TipoTarea == (int)TipoTarea.Material)
                {
                    string clave = (n.Clave ?? "").Trim();
                    string nombre = (n.Nombre ?? "").Trim();
                    if (clave.Length > 0) _surtidoPorClave[clave] = n.Surtido;
                    if (nombre.Length > 0) _surtidoPorNombre[nombre] = n.Surtido;
                }
            }

            var nodosRaiz = itemsTareas.Where(i => i.Nivel == 0).ToList();
            _suprimirFlujoActivacion = true;
            try { olvTareas.SetObjects(nodosRaiz); olvTareas.CollapseAll(); }
            finally { _suprimirFlujoActivacion = false; }

            lblContextoCasa.Text = string.Format(
                "Casa M{0} L{1}  ·  Prototipo: {2}  ·  Ruta: {3}",
                manzanaActual, loteActual, prototipoActual, rutaActual);

            ActualizarEstadisticas();
            SeleccionarDestajoActual();
        }

        private ItemTareaActivacion ADto(NodoDestajoApi n) => new ItemTareaActivacion
        {
            ID = n.Id,
            ParentId = n.ParentId,
            Nombre = n.Nombre,
            Descripcion = n.Descripcion ?? "",
            Clave = n.Clave ?? "",
            Nivel = n.Nivel,
            Tipo = ObtenerTipoNodo(n.Nivel),
            TipoTarea = n.TipoTareaTexto,
            TipoTareaEnum = (TipoTarea)n.TipoTarea,
            Cantidad = n.Cantidad,
            Unidad = n.Unidad ?? "",
            PrecioUnitario = n.PrecioUnitario,
            Activa = n.Activa,
            CuadrillaAsignada = n.CuadrillaAsignada ?? "",
            DesatajoActivado = n.DesatajoActivado,
            Finalizado = n.Finalizado,
            FechaActivacion = n.FechaActivacion,
            FechaFinalizacion = n.FechaFinalizacion,
            Surtido = n.Surtido
        };

        // ------------------------------------------------------------------
        // Escrituras — todas vía api/destajos; los diálogos que no eran SQL de
        // destajos (cuadrilla, nómina, justificación, PDF) se reutilizan tal cual.
        // ------------------------------------------------------------------

        private async Task ActivarWeb(int nodoId)
        {
            var destajoItem = itemsTareas.FirstOrDefault(i => i.ID == nodoId && i.Nivel == 1);
            string nombreDestajo = destajoItem?.Nombre ?? "Destajo";

            string cuadrilla;
            using (var formCuadrilla = new FormAsignarCuadrilla(nombreDestajo))
            {
                var dr = formCuadrilla.ShowDialog(this);
                if (dr != DialogResult.OK || formCuadrilla.CuadrillaAsignada == null
                    || string.IsNullOrEmpty(formCuadrilla.CuadrillaAsignada.CodigoCuadrilla))
                    return;
                cuadrilla = formCuadrilla.CuadrillaAsignada.CodigoCuadrilla;
            }

            try
            {
                await Task.Run(() => ApiClient.Post("/api/destajos/activar", new
                {
                    Manzana = manzanaActual,
                    Lote = loteActual,
                    Ruta = rutaActual,
                    Prototipo = prototipoActual,
                    NodoId = nodoId,
                    Cuadrilla = cuadrilla
                }));
                await RefrescarArbol();
                await GenerarPdfWeb(nodoId, esFinalizacion: false);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo activar el destajo"); }
        }

        private async Task FinalizarWeb(int nodoId)
        {
            var destajoItem = itemsTareas.FirstOrDefault(i => i.ID == nodoId && i.Nivel == 1);
            if (destajoItem == null) return;

            if (!destajoItem.DesatajoActivado || string.IsNullOrEmpty(destajoItem.CuadrillaAsignada))
            {
                MessageBox.Show("Sólo se puede finalizar un destajo previamente activado con cuadrilla.",
                    "Finalizar destajo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var rsp = MessageBox.Show(
                $"¿Finalizar el destajo \"{destajoItem.Nombre}\"?\n\n" +
                "Se generará el PDF de finalización y quedará habilitada la asignación de nómina.",
                "Finalizar destajo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rsp != DialogResult.Yes) return;

            try
            {
                await Task.Run(() => ApiClient.Post("/api/destajos/finalizar", new
                {
                    Manzana = manzanaActual,
                    Lote = loteActual,
                    Ruta = rutaActual,
                    Prototipo = prototipoActual,
                    NodoId = nodoId
                }));
                await RefrescarArbol();
                await GenerarPdfWeb(nodoId, esFinalizacion: true);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo finalizar el destajo"); }
        }

        /// <summary>
        /// Intenta desactivar sin justificación; si el destajo ya liberó insumos el
        /// API responde 400 pidiéndola, y sólo entonces se muestra el diálogo.
        /// </summary>
        private async Task DesactivarWeb(int nodoId)
        {
            var destajoItem = itemsTareas.FirstOrDefault(i => i.ID == nodoId && i.Nivel == 1);
            string nombre = destajoItem?.Nombre ?? "este destajo";

            var rsp = MessageBox.Show(
                $"¿Desactivar el destajo \"{nombre}\"?\n\n" +
                "Se eliminará la cuadrilla asignada y se marcará como desactivado.",
                "Desactivar destajo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (rsp != DialogResult.Yes) return;

            string justificacion = null;
            for (int intento = 0; intento < 2; intento++)
            {
                try
                {
                    await Task.Run(() => ApiClient.Post("/api/destajos/desactivar", new
                    {
                        Manzana = manzanaActual,
                        Lote = loteActual,
                        Ruta = rutaActual,
                        Prototipo = prototipoActual,
                        NodoId = nodoId,
                        Justificacion = justificacion
                    }));
                    await RefrescarArbol();
                    return;
                }
                catch (ApiException ex)
                {
                    bool faltaJustificacion = ex.StatusCode == HttpStatusCode.BadRequest
                        && MensajeDe(ex).IndexOf("justificaci", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (faltaJustificacion && intento == 0)
                    {
                        justificacion = PromptJustificacion.Pedir(this, "Justificación requerida",
                            $"Desactivar el destajo \"{nombre}\" ya liberó insumos del almacén.\n" +
                            "El stock NO se devolverá. Indica el motivo de la desactivación:",
                            null, true);
                        if (justificacion == null) return; // cancelado
                        continue;
                    }
                    ManejarErrorApi(ex, "No se pudo desactivar el destajo");
                    return;
                }
                catch (Exception ex) { ManejarErrorApi(ex, "No se pudo desactivar el destajo"); return; }
            }
        }

        /// <summary>
        /// Deshace el progreso de un destajo finalizado (vuelve a ACTIVADO). Si hay
        /// nómina asignada a su mano de obra, pregunta si borrarla (recibos ya
        /// emitidos nunca se eliminan); el borrado lo hace el propio API.
        /// </summary>
        private async Task ReabrirWeb(int nodoId)
        {
            var destajoItem = itemsTareas.FirstOrDefault(i => i.ID == nodoId && i.Nivel == 1);
            if (destajoItem == null) return;

            var rsp0 = MessageBox.Show(
                $"¿Reabrir el destajo \"{destajoItem.Nombre}\"?\n\n" +
                "Volverá al estado ACTIVADO (conserva su cuadrilla) y podrás finalizarlo de nuevo.",
                "Reabrir destajo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rsp0 != DialogResult.Yes) return;

            bool borrarNomina = false;
            var nodosConNomina = DetectarNodosConNominaAsignada(destajoItem);
            if (nodosConNomina.Count > 0)
            {
                var rsp = MessageBox.Show(
                    "Este destajo tiene nómina asignada a su mano de obra.\n\n" +
                    "• Sí  → borrar la asignación de nómina al reabrir.\n" +
                    "• No  → conservar la asignación tal cual.\n\n" +
                    "(Los recibos ya emitidos no se eliminan en ningún caso.)",
                    "Nómina asignada", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (rsp == DialogResult.Cancel) return;
                borrarNomina = rsp == DialogResult.Yes;
            }

            try
            {
                await Task.Run(() => ApiClient.Post("/api/destajos/reabrir", new
                {
                    Manzana = manzanaActual,
                    Lote = loteActual,
                    Ruta = rutaActual,
                    Prototipo = prototipoActual,
                    NodoId = nodoId,
                    BorrarNomina = borrarNomina
                }));
                await RefrescarArbol();
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo reabrir el destajo"); }
        }

        private async Task RegenerarPdfWeb(int nodoId)
        {
            var destajo = _arbolActual?.Nodos.FirstOrDefault(n => n.Id == nodoId && n.Nivel == 1);
            if (destajo == null) return;

            if (!destajo.DesatajoActivado || string.IsNullOrEmpty(destajo.CuadrillaAsignada))
            {
                MessageBox.Show("Este destajo no está activado o no tiene cuadrilla asignada.",
                    "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            await GenerarPdfWeb(nodoId, esFinalizacion: destajo.Finalizado);
        }

        /// <summary>
        /// Arma el PDF con PdfSharp (CrearPdfDestajo, reutilizado tal cual del flujo
        /// clásico) y lo archiva vía api/destajos/pdf en lugar de GuardarPdfDestajoEnBD.
        /// </summary>
        private async Task GenerarPdfWeb(int nodoId, bool esFinalizacion)
        {
            var destajo = _arbolActual?.Nodos.FirstOrDefault(n => n.Id == nodoId && n.Nivel == 1);
            if (destajo == null || !destajo.DesatajoActivado || string.IsNullOrEmpty(destajo.CuadrillaAsignada))
                return;

            try
            {
                var miembrosApi = await Task.Run(() => ApiClient.Get<List<MiembroCuadrillaDestajoApi>>(
                    "/api/destajos/cuadrillas/" + Uri.EscapeDataString(destajo.CuadrillaAsignada) + "/miembros"))
                    ?? new List<MiembroCuadrillaDestajoApi>();
                var miembros = miembrosApi.Select(m => new MiembroResumen
                {
                    Clave = m.Clave ?? "",
                    Nombre = m.Nombre,
                    Rol = m.Rol,
                    EsJefe = m.EsJefe,
                    Telefono = m.Telefono ?? ""
                }).ToList();

                var hijos = _arbolActual.Nodos
                    .Where(n => n.ParentId == destajo.Id && n.Nivel == 2)
                    .Select(ADto).ToList();
                var itemDestajo = ADto(destajo);

                string tituloPrincipal = esFinalizacion ? "ACTA DE FINALIZACIÓN" : "ASIGNACIÓN DE DESTAJO";
                string subtitulo = esFinalizacion ? "Trabajos Terminados y Validados" : "Asignación de Cuadrilla y Trabajos";
                string prefijoTemp = esFinalizacion ? "Finalizacion" : "Destajo";
                string prefijoSugerido = esFinalizacion ? "Finalizacion" : "Asignacion";

                string nombreSeguro = SanitizarNombreArchivo(destajo.Nombre);
                string nombreArchivo =
                    $"{prefijoTemp}_M{_arbolActual.Manzana}-L{_arbolActual.Lote}_{destajo.Id}_{nombreSeguro}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string archivoTemp = Path.Combine(Path.GetTempPath(), nombreArchivo);

                CrearPdfDestajo(archivoTemp, itemDestajo, miembros, hijos, tituloPrincipal, subtitulo);

                var bytes = File.ReadAllBytes(archivoTemp);
                await Task.Run(() => ApiClient.Post("/api/destajos/pdf", new
                {
                    Manzana = _arbolActual.Manzana,
                    Lote = _arbolActual.Lote,
                    Ruta = _arbolActual.Ruta,
                    Prototipo = _arbolActual.Prototipo,
                    NodoId = destajo.Id,
                    NombreDestajo = destajo.Nombre,
                    CuadrillaAsignada = destajo.CuadrillaAsignada,
                    NombreArchivo = nombreArchivo,
                    ContenidoBase64 = Convert.ToBase64String(bytes)
                }));

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"{prefijoSugerido}_M{_arbolActual.Manzana}-L{_arbolActual.Lote}_{destajo.Nombre}.pdf");

                using (var preview = new FormPdfPreview(archivoTemp, sugerido))
                    preview.ShowDialog(this);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "Error generando el PDF"); }
        }

        private void DistribucionNominaWeb(int nodoId)
        {
            if (_arbolActual == null) return;
            var hijosManoDeObra = _arbolActual.Nodos
                .Where(n => n.ParentId == nodoId && n.Nivel == 2 && n.TipoTarea == (int)TipoTarea.ManoDeObra && n.Importe > 0m)
                .ToList();
            if (hijosManoDeObra.Count == 0)
            {
                MessageBox.Show("Este destajo no tiene tareas de mano de obra para distribuir.",
                    "Distribución de nómina", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AbrirAsignarNominaSecuencial(hijosManoDeObra, 0);
        }

        /// <summary>
        /// Abre una ventana FormAsignarNominaWeb por cada tarea de mano de obra,
        /// una a la vez (Show(), no ShowDialog() — ver reference_webview2_multi_instance).
        /// Al cerrar una, se encadena la siguiente, igual que hacía el bucle
        /// ShowDialog() del diálogo nativo.
        /// </summary>
        private void AbrirAsignarNominaSecuencial(List<NodoDestajoApi> hijos, int indice)
        {
            if (indice >= hijos.Count) return;
            var hijo = hijos[indice];
            var form = new FormAsignarNominaWeb(manzanaActual, loteActual, rutaActual, hijo.Id, hijo.Nombre, hijo.Importe);
            form.FormClosed += (s2, e2) => AbrirAsignarNominaSecuencial(hijos, indice + 1);
            form.Show();
        }

        // ------------------------------------------------------------------
        // Documentos (Reportes): las 4 vistas viven ahora en la propia página
        // (data-view="reporte-*"). El host sólo hace de puente a la API — igual
        // que el resto de esta clase — y a PdfSharp para exportar/reimprimir,
        // reusando exactamente el layout que ya tenían los diálogos clásicos
        // (FormReporteDestajosSemana, FormDestajosPorCuadrilla, FormReporteNomina,
        // FormVisorRecibosNomina), que siguen intactos para el menú clásico.
        // ------------------------------------------------------------------

        private static DateTime FechaODefault(string iso, DateTime porDefecto)
        {
            return DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d.Date : porDefecto.Date;
        }

        private static DateTime LunesDeEstaSemana(DateTime referencia)
        {
            int diff = (7 + (referencia.DayOfWeek - DayOfWeek.Monday)) % 7;
            return referencia.AddDays(-diff).Date;
        }

        private void PushReporte(string tipo, object lista)
        {
            if (webPanel?.CoreWebView2 == null) return;
            webPanel.CoreWebView2.PostWebMessageAsJson(
                JsonConvert.SerializeObject(new { tipo = tipo, lista = lista }, CamelCaseSettings));
        }

        // ---- Destajos por semana ----

        private void ReporteSemanaDatosWeb(string desde, string hasta)
        {
            try
            {
                DateTime d = FechaODefault(desde, LunesDeEstaSemana(DateTime.Today));
                DateTime h = FechaODefault(hasta, DateTime.Today);
                var lista = ApiClient.Get<List<RegistroSemanaApi>>(
                    "/api/destajos/reporte-semana?desde=" + d.ToString("yyyy-MM-dd") + "&hasta=" + h.ToString("yyyy-MM-dd"))
                    ?? new List<RegistroSemanaApi>();
                PushReporte("reporte-semana", lista);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el reporte semanal"); }
        }

        private enum EstadoSemanaWeb { FueraDePeriodo, Activado, Terminado, Pendiente }

        private static EstadoSemanaWeb ClasificarEstadoSemana(RegistroSemanaApi r, DateTime desde, DateTime hastaFin)
        {
            bool terminadoEnPeriodo = r.Finalizado && r.FechaFinalizacion.HasValue
                && r.FechaFinalizacion.Value >= desde && r.FechaFinalizacion.Value <= hastaFin;
            bool activadoEnPeriodo = r.FechaActivacion.HasValue
                && r.FechaActivacion.Value >= desde && r.FechaActivacion.Value <= hastaFin;
            if (terminadoEnPeriodo) return EstadoSemanaWeb.Terminado;
            if (!r.Finalizado) return activadoEnPeriodo ? EstadoSemanaWeb.Activado : EstadoSemanaWeb.Pendiente;
            return EstadoSemanaWeb.FueraDePeriodo;
        }

        private static string TextoEstadoSemana(EstadoSemanaWeb e)
        {
            switch (e)
            {
                case EstadoSemanaWeb.Terminado: return "TERMINADOS EN EL PERIODO";
                case EstadoSemanaWeb.Activado: return "ACTIVADOS EN EL PERIODO";
                case EstadoSemanaWeb.Pendiente: return "PENDIENTES (activados sin terminar)";
                default: return "OTROS";
            }
        }

        private void ReporteSemanaPdfWeb(string desde, string hasta)
        {
            try
            {
                DateTime d = FechaODefault(desde, LunesDeEstaSemana(DateTime.Today));
                DateTime h = FechaODefault(hasta, DateTime.Today);
                DateTime hastaFin = h.AddDays(1).AddSeconds(-1);

                var registros = ApiClient.Get<List<RegistroSemanaApi>>(
                    "/api/destajos/reporte-semana?desde=" + d.ToString("yyyy-MM-dd") + "&hasta=" + h.ToString("yyyy-MM-dd"))
                    ?? new List<RegistroSemanaApi>();

                if (registros.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar.", "Exportar PDF",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var conEstado = registros.Select(r => (Reg: r, Estado: ClasificarEstadoSemana(r, d, hastaFin))).ToList();
                int activados = conEstado.Count(x => x.Estado == EstadoSemanaWeb.Activado);
                int terminados = conEstado.Count(x => x.Estado == EstadoSemanaWeb.Terminado);
                int pendientes = conEstado.Count(x => x.Estado == EstadoSemanaWeb.Pendiente);
                string resumen = string.Format(
                    "Periodo: {0} a {1}   ·   Activados: {2}   ·   Terminados: {3}   ·   Pendientes (sin terminar): {4}",
                    d.ToString("dd/MM/yyyy"), h.ToString("dd/MM/yyyy"), activados, terminados, pendientes);

                string archivo = Path.Combine(Path.GetTempPath(),
                    $"ReporteDestajosSemana_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                using (var doc = new PdfDocument())
                {
                    doc.Info.Title = "Reporte Semanal de Destajos";
                    var page = doc.AddPage();
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    DibujarReporteSemanaPdf(doc, page, conEstado, resumen);
                    doc.Save(archivo);
                }
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "Error generando el PDF del reporte semanal"); }
        }

        private static void DibujarReporteSemanaPdf(
            PdfDocument doc, PdfPage page,
            List<(RegistroSemanaApi Reg, EstadoSemanaWeb Estado)> conEstado, string resumen)
        {
            var gfx = XGraphics.FromPdfPage(page);
            var fontTitulo = new XFont("Segoe UI", 16, XFontStyle.Bold);
            var fontSub = new XFont("Segoe UI", 9, XFontStyle.Italic);
            var fontSeccion = new XFont("Segoe UI", 11, XFontStyle.Bold);
            var fontHeader = new XFont("Segoe UI", 8.5, XFontStyle.Bold);
            var fontTexto = new XFont("Segoe UI", 8.5, XFontStyle.Regular);

            double margenIzq = 30, margenDer = 30, y = 30;
            double ancho = page.Width - margenIzq - margenDer;

            gfx.DrawString("REPORTE SEMANAL DE DESTAJOS", fontTitulo, XBrushes.Black,
                new XRect(margenIzq, y, ancho, 22), XStringFormats.TopLeft);
            y += 24;
            gfx.DrawString(resumen, fontSub, XBrushes.DarkSlateGray,
                new XRect(margenIzq, y, ancho, 16), XStringFormats.TopLeft);
            y += 22;

            string[] headers = { "Estado", "Casa", "Ruta", "Categoría · Destajo", "Cuadrilla", "Activado", "Terminado" };
            double[] anchos = { 90, 60, 60, 280, 80, 70, 70 };

            foreach (var estado in new[] { EstadoSemanaWeb.Terminado, EstadoSemanaWeb.Activado, EstadoSemanaWeb.Pendiente })
            {
                var deEsteEstado = conEstado.Where(par => par.Estado == estado).Select(par => par.Reg).ToList();
                if (deEsteEstado.Count == 0) continue;

                if (y > page.Height - 80)
                {
                    page = doc.AddPage();
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 30;
                }

                gfx.DrawRectangle(XBrushes.DarkSlateBlue, new XRect(margenIzq, y, ancho, 18));
                gfx.DrawString($"{TextoEstadoSemana(estado)}  ({deEsteEstado.Count})",
                    fontSeccion, XBrushes.White, new XRect(margenIzq + 4, y + 2, ancho, 16), XStringFormats.TopLeft);
                y += 22;

                double x = margenIzq;
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(220, 230, 245)), new XRect(x, y, anchos[i], 16));
                    gfx.DrawString(headers[i], fontHeader, XBrushes.Black,
                        new XRect(x + 3, y + 1, anchos[i] - 4, 14), XStringFormats.TopLeft);
                    x += anchos[i];
                }
                y += 18;

                foreach (var r in deEsteEstado
                    .OrderBy(rr => rr.Manzana).ThenBy(rr => rr.Lote)
                    .ThenBy(rr => rr.CategoriaNombre).ThenBy(rr => rr.DestajoNombre))
                {
                    if (y > page.Height - 30)
                    {
                        page = doc.AddPage();
                        page.Orientation = PdfSharp.PageOrientation.Landscape;
                        gfx.Dispose();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 30;
                    }
                    x = margenIzq;
                    string[] celdas =
                    {
                        TextoEstadoSemana(estado).Split(' ')[0],
                        $"M{r.Manzana}-L{r.Lote}",
                        r.Ruta == "RutaCalandraDestajo" ? "Calandra" : "Tunera",
                        $"{r.CategoriaNombre} · {r.DestajoNombre}",
                        r.Cuadrilla ?? "",
                        r.FechaActivacion?.ToString("dd/MM/yyyy") ?? "",
                        r.FechaFinalizacion?.ToString("dd/MM/yyyy") ?? ""
                    };
                    for (int i = 0; i < celdas.Length; i++)
                    {
                        gfx.DrawRectangle(XPens.LightGray, new XRect(x, y, anchos[i], 16));
                        gfx.DrawString(RecortarTexto(celdas[i], anchos[i] - 4, fontTexto, gfx),
                            fontTexto, XBrushes.Black, new XRect(x + 3, y + 1, anchos[i] - 4, 14), XStringFormats.TopLeft);
                        x += anchos[i];
                    }
                    y += 16;
                }
                y += 10;
            }
            gfx.Dispose();
        }

        private static string RecortarTexto(string texto, double ancho, XFont font, XGraphics gfx)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            if (gfx.MeasureString(texto, font).Width <= ancho) return texto;
            string actual = texto;
            while (actual.Length > 0 && gfx.MeasureString(actual + "…", font).Width > ancho)
                actual = actual.Substring(0, actual.Length - 1);
            return actual + "…";
        }

        // ---- Terminados por cuadrilla ----

        private void ReporteCuadrillaDatosWeb(string desde, string hasta, string manzana, string lote)
        {
            try
            {
                DateTime d = FechaODefault(desde, LunesDeEstaSemana(DateTime.Today));
                DateTime h = FechaODefault(hasta, DateTime.Today);
                string url = "/api/destajos/reporte-cuadrilla?desde=" + d.ToString("yyyy-MM-dd") + "&hasta=" + h.ToString("yyyy-MM-dd");
                if (!string.IsNullOrEmpty(manzana) && !string.IsNullOrEmpty(lote))
                    url += "&manzana=" + Uri.EscapeDataString(manzana) + "&lote=" + Uri.EscapeDataString(lote);

                var lista = ApiClient.Get<List<RegistroCuadrillaApi>>(url) ?? new List<RegistroCuadrillaApi>();
                PushReporte("reporte-cuadrilla", lista);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el reporte por cuadrilla"); }
        }

        private void ReporteCuadrillaDistribuirWeb(string cuadrilla, string desde, string hasta, string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(cuadrilla)) return;
            try
            {
                DateTime d = FechaODefault(desde, LunesDeEstaSemana(DateTime.Today));
                DateTime h = FechaODefault(hasta, DateTime.Today);
                string url = "/api/destajos/reporte-cuadrilla?desde=" + d.ToString("yyyy-MM-dd") + "&hasta=" + h.ToString("yyyy-MM-dd");
                if (!string.IsNullOrEmpty(manzana) && !string.IsNullOrEmpty(lote))
                    url += "&manzana=" + Uri.EscapeDataString(manzana) + "&lote=" + Uri.EscapeDataString(lote);

                var registros = (ApiClient.Get<List<RegistroCuadrillaApi>>(url) ?? new List<RegistroCuadrillaApi>())
                    .Where(r => string.Equals(r.Cuadrilla, cuadrilla, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (registros.Count == 0)
                {
                    MessageBox.Show("La cuadrilla seleccionada no tiene destajos finalizados en el periodo.",
                        "Distribución de Nómina", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var destajos = registros.Select(r => new FormDistribucionNomina.DestajoTerminado
                {
                    Cuadrilla = cuadrilla,
                    Casa = $"M{r.Manzana}-L{r.Lote}",
                    Manzana = r.Manzana,
                    Lote = r.Lote,
                    Nombre = r.DestajoNombre,
                    Categoria = r.CategoriaNombre,
                    FechaFinalizacion = r.FechaFinalizacion,
                    Importe = r.Importe,
                    Ruta = r.Ruta,
                    NodoID = r.DestajoId,
                    NominaDistribuida = r.NominaDistribuida,
                    FechaDistribucionNomina = r.FechaDistribucionNomina
                }).ToList();

                var yaDistribuidos = destajos.Where(dd => dd.NominaDistribuida).ToList();
                if (yaDistribuidos.Count > 0)
                {
                    var resumen = string.Join(Environment.NewLine, yaDistribuidos.Take(5).Select(dd =>
                        $"  • {dd.Nombre}  (M{dd.Manzana}-L{dd.Lote}, distribuido el {dd.FechaDistribucionNomina:dd/MM/yyyy})"));
                    if (yaDistribuidos.Count > 5)
                        resumen += Environment.NewLine + $"  • ... y {yaDistribuidos.Count - 5} más";

                    var resp = MessageBox.Show(
                        $"{yaDistribuidos.Count} destajo(s) ya tienen nómina distribuida:\n\n{resumen}\n\n" +
                        "¿Continuar y volver a generar recibos para ellos?\n\n" +
                        "Esto sobrescribirá la fecha de distribución y emitirá nuevos PDFs.",
                        "Nómina ya distribuida", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
                    if (resp != DialogResult.Yes) return;
                }

                // Show(), no ShowDialog(): un WebView2 modal sobre el del panel
                // principal (también WebView2) aborta la inicialización con
                // COMException E_ABORT — ver reference_webview2_multi_instance.
                var form = new FormDistribucionNominaWeb(cuadrilla, d, h, destajos);
                form.FormClosed += (s2, e2) => ReporteCuadrillaDatosWeb(desde, hasta, manzana, lote);
                form.Show();
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo generar la distribución de nómina"); }
        }

        // ---- Listado de nómina ----

        private void ReporteNominaDatosWeb(string desde, string hasta)
        {
            try
            {
                DateTime d = FechaODefault(desde, LunesDeEstaSemana(DateTime.Today));
                DateTime h = FechaODefault(hasta, DateTime.Today);
                var lista = ApiClient.Get<List<NominaReporteApi>>(
                    "/api/nomina/reporte?desde=" + d.ToString("yyyy-MM-dd") + "&hasta=" + h.ToString("yyyy-MM-dd"))
                    ?? new List<NominaReporteApi>();
                PushReporte("reporte-nomina", lista);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el listado de nómina"); }
        }

        private void ReporteNominaPdfWeb(string desde, string hasta)
        {
            try
            {
                DateTime d = FechaODefault(desde, LunesDeEstaSemana(DateTime.Today));
                DateTime h = FechaODefault(hasta, DateTime.Today);
                var items = ApiClient.Get<List<NominaReporteApi>>(
                    "/api/nomina/reporte?desde=" + d.ToString("yyyy-MM-dd") + "&hasta=" + h.ToString("yyyy-MM-dd"))
                    ?? new List<NominaReporteApi>();

                if (items.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar.", "Exportar PDF",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int recibos = items.Sum(x => x.NumRecibos);
                decimal total = items.Sum(x => x.Monto);
                string resumen = string.Format(
                    "Periodo: {0} a {1}   ·   Trabajadores: {2}   ·   Recibos: {3}   ·   Total a rayar: {4}",
                    d.ToString("dd/MM/yyyy"), h.ToString("dd/MM/yyyy"),
                    items.Count, recibos, total.ToString("C2", CultureInfo.CurrentCulture));

                string archivo = Path.Combine(Path.GetTempPath(), $"ReporteNomina_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                using (var doc = new PdfDocument())
                {
                    doc.Info.Title = "Listado de Nómina";
                    var page = doc.AddPage();
                    DibujarListadoNominaPdf(doc, page, items, resumen);
                    doc.Save(archivo);
                }
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "Error generando el PDF del listado de nómina"); }
        }

        private static void DibujarListadoNominaPdf(PdfDocument doc, PdfPage page, List<NominaReporteApi> items, string resumen)
        {
            var gfx = XGraphics.FromPdfPage(page);
            var fontTitulo = new XFont("Segoe UI", 16, XFontStyle.Bold);
            var fontSub = new XFont("Segoe UI", 9, XFontStyle.Italic);
            var fontHeader = new XFont("Segoe UI", 9, XFontStyle.Bold);
            var fontTexto = new XFont("Segoe UI", 9, XFontStyle.Regular);
            var fontTotales = new XFont("Segoe UI", 11, XFontStyle.Bold);

            double margenIzq = 30, margenDer = 30, y = 30;
            double ancho = page.Width - margenIzq - margenDer;

            gfx.DrawString("LISTADO DE NÓMINA - RAYA SEMANAL", fontTitulo, XBrushes.Black,
                new XRect(margenIzq, y, ancho, 22), XStringFormats.TopLeft);
            y += 24;
            gfx.DrawString(resumen, fontSub, XBrushes.DarkSlateGray,
                new XRect(margenIzq, y, ancho, 16), XStringFormats.TopLeft);
            y += 24;

            string[] headers = { "#", "Clave", "Trabajador", "Rol", "Cuadrillas", "Recibos", "Monto a rayar", "Firma" };
            double[] anchos = { 30, 60, 180, 90, 110, 50, 80, 130 };

            double x = margenIzq;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(41, 60, 88)), new XRect(x, y, anchos[i], 22));
                gfx.DrawString(headers[i], fontHeader, XBrushes.White,
                    new XRect(x + 3, y + 4, anchos[i] - 4, 18), i == 6 ? XStringFormats.TopRight : XStringFormats.TopLeft);
                x += anchos[i];
            }
            y += 22;

            decimal total = 0m;
            int n = 1;
            foreach (var it in items)
            {
                if (y > page.Height - 70)
                {
                    page = doc.AddPage();
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 30;
                }
                x = margenIzq;
                string[] celdas =
                {
                    n.ToString(), it.Clave ?? "", it.Nombre ?? "", it.Rol ?? "", it.Cuadrillas ?? "",
                    it.NumRecibos.ToString(), it.Monto.ToString("C2", CultureInfo.CurrentCulture), ""
                };
                for (int i = 0; i < celdas.Length; i++)
                {
                    gfx.DrawRectangle(XPens.LightGray, new XRect(x, y, anchos[i], 22));
                    gfx.DrawString(RecortarTexto(celdas[i], anchos[i] - 4, fontTexto, gfx),
                        fontTexto, XBrushes.Black, new XRect(x + 3, y + 5, anchos[i] - 4, 18),
                        i == 6 ? XStringFormats.TopRight : XStringFormats.TopLeft);
                    x += anchos[i];
                }
                y += 22;
                total += it.Monto;
                n++;
            }

            y += 6;
            gfx.DrawString("TOTAL A RAYAR: " + total.ToString("C2", CultureInfo.CurrentCulture),
                fontTotales, XBrushes.Black, new XRect(margenIzq, y, ancho, 22), XStringFormats.TopRight);
            gfx.Dispose();
        }

        // ---- Recibos de nómina ----

        private void ReporteRecibosDatosWeb(string desde, string hasta)
        {
            try
            {
                DateTime d = FechaODefault(desde, DateTime.Today.AddDays(-30));
                DateTime h = FechaODefault(hasta, DateTime.Today);
                var lista = ApiClient.Get<List<ReciboNominaApi>>(
                    "/api/nomina/recibos?desde=" + d.ToString("yyyy-MM-dd") + "&hasta=" + h.ToString("yyyy-MM-dd"))
                    ?? new List<ReciboNominaApi>();
                PushReporte("reporte-recibos", lista);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar los recibos de nómina"); }
        }

        private static string SanitizarNombreRecibo(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "recibo";
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (var c in nombre)
                sb.Append(Array.IndexOf(invalid, c) >= 0 || c == ' ' ? '_' : c);
            return sb.ToString();
        }

        private void ReciboAbrirWeb(ReciboMsgDto r)
        {
            if (!r.tienePdf) { ReciboReimprimirWeb(r); return; }
            try
            {
                byte[] bytes = ApiClient.GetBytes("/api/nomina/recibos/" + r.id + "/pdf");
                if (bytes == null || bytes.Length == 0)
                {
                    MessageBox.Show("El PDF está vacío en la base de datos.",
                        "Abrir PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string archivo = Path.Combine(Path.GetTempPath(),
                    string.Format("Recibo_{0}_{1}.pdf", r.id, SanitizarNombreRecibo(r.nombreTrabajador)));
                File.WriteAllBytes(archivo, bytes);
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo abrir el PDF"); }
        }

        private void ReciboReimprimirWeb(ReciboMsgDto r)
        {
            try
            {
                string archivo = Path.Combine(Path.GetTempPath(),
                    string.Format("ReciboReimpreso_{0}_{1:yyyyMMdd_HHmmss}.pdf",
                        SanitizarNombreRecibo(r.nombreTrabajador), DateTime.Now));
                using (var fs = new FileStream(archivo, FileMode.Create, FileAccess.Write))
                    GenerarReciboPdfWeb(r, fs);
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo reimprimir el recibo"); }
        }

        private void ReciboGuardarComoWeb(ReciboMsgDto r)
        {
            try
            {
                byte[] bytes;
                if (r.tienePdf)
                {
                    bytes = ApiClient.GetBytes("/api/nomina/recibos/" + r.id + "/pdf");
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        GenerarReciboPdfWeb(r, ms);
                        bytes = ms.ToArray();
                    }
                }
                if (bytes == null || bytes.Length == 0)
                {
                    MessageBox.Show("No se pudo obtener el PDF.", "Guardar PDF",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PDF (*.pdf)|*.pdf";
                    sfd.FileName = string.Format("Recibo_{0}_{1:yyyyMMdd}.pdf",
                        SanitizarNombreRecibo(r.nombreTrabajador), r.fechaRecibo);
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;
                    File.WriteAllBytes(sfd.FileName, bytes);
                    MessageBox.Show("Guardado en:\n" + sfd.FileName, "Guardar PDF",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo guardar el PDF"); }
        }

        private static void GenerarReciboPdfWeb(ReciboMsgDto r, Stream destino)
        {
            using (var doc = new PdfDocument())
            {
                doc.Info.Title = "Recibo de Nómina";
                var page = doc.AddPage();
                page.Size = PdfSharp.PageSize.Letter;

                var gfx = XGraphics.FromPdfPage(page);
                var fontTit = new XFont("Segoe UI", 20, XFontStyle.Bold);
                var fontSub = new XFont("Segoe UI", 11, XFontStyle.Italic);
                var fontLbl = new XFont("Segoe UI", 11, XFontStyle.Bold);
                var fontTxt = new XFont("Segoe UI", 11, XFontStyle.Regular);
                var fontMonto = new XFont("Segoe UI", 22, XFontStyle.Bold);
                var fontLetras = new XFont("Segoe UI", 10, XFontStyle.Italic);

                double mIzq = 50, mDer = 50;
                double ancho = page.Width - mIzq - mDer;
                double y = 50;

                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(41, 60, 88)), new XRect(mIzq, y, ancho, 50));
                gfx.DrawString("RECIBO DE NÓMINA", fontTit, XBrushes.White,
                    new XRect(mIzq + 12, y + 14, ancho - 24, 30), XStringFormats.TopLeft);
                gfx.DrawString("Pilaris", fontSub, XBrushes.LightGray,
                    new XRect(mIzq + 12, y + 32, ancho - 24, 16), XStringFormats.TopLeft);
                y += 64;

                gfx.DrawString("Folio: " + r.id.ToString("D6"), fontTxt, XBrushes.Black,
                    new XRect(mIzq, y, ancho, 18), XStringFormats.TopRight);
                gfx.DrawString("Fecha de pago: " + r.fechaRecibo.ToString("dd/MM/yyyy"), fontTxt, XBrushes.Black,
                    new XRect(mIzq, y, ancho, 18), XStringFormats.TopLeft);
                y += 26;

                DibujarCampoRecibo(gfx, mIzq, ref y, ancho, "Trabajador:", r.nombreTrabajador, fontLbl, fontTxt);
                DibujarCampoRecibo(gfx, mIzq, ref y, ancho, "Rol:", r.rol ?? "—", fontLbl, fontTxt);
                DibujarCampoRecibo(gfx, mIzq, ref y, ancho, "Cuadrilla:", r.codigoCuadrilla ?? "—", fontLbl, fontTxt);
                if (r.periodoDesde.HasValue && r.periodoHasta.HasValue)
                {
                    DibujarCampoRecibo(gfx, mIzq, ref y, ancho, "Periodo:",
                        r.periodoDesde.Value.ToString("dd/MM/yyyy") + " a " + r.periodoHasta.Value.ToString("dd/MM/yyyy"),
                        fontLbl, fontTxt);
                }
                DibujarCampoRecibo(gfx, mIzq, ref y, ancho, "Concepto:", r.concepto ?? "", fontLbl, fontTxt);

                y += 14;
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(245, 247, 250)), new XRect(mIzq, y, ancho, 80));
                gfx.DrawString("IMPORTE A RECIBIR", fontLbl, XBrushes.DarkSlateGray,
                    new XRect(mIzq + 14, y + 8, ancho - 28, 18), XStringFormats.TopLeft);
                gfx.DrawString(r.monto.ToString("C2", CultureInfo.CurrentCulture), fontMonto, XBrushes.Black,
                    new XRect(mIzq + 14, y + 28, ancho - 28, 38), XStringFormats.TopRight);

                string letras = "(" + NumeroALetras.Convertir(r.monto) + ")";
                gfx.DrawString(letras, fontLetras, XBrushes.DarkSlateGray,
                    new XRect(mIzq + 14, y + 60, ancho - 28, 16), XStringFormats.TopLeft);
                y += 96;

                y += 70;
                double anchoFirma = (ancho - 40) / 2;
                gfx.DrawLine(XPens.Black, mIzq, y, mIzq + anchoFirma, y);
                gfx.DrawLine(XPens.Black, mIzq + anchoFirma + 40, y, mIzq + ancho, y);
                gfx.DrawString("Firma del trabajador", fontTxt, XBrushes.Black,
                    new XRect(mIzq, y + 4, anchoFirma, 18), XStringFormats.TopCenter);
                gfx.DrawString("Entrega / Recibido por", fontTxt, XBrushes.Black,
                    new XRect(mIzq + anchoFirma + 40, y + 4, anchoFirma, 18), XStringFormats.TopCenter);

                gfx.Dispose();
                doc.Save(destino, false);
            }
        }

        private static void DibujarCampoRecibo(XGraphics gfx, double mIzq, ref double y, double ancho,
            string etiqueta, string valor, XFont fontLbl, XFont fontTxt)
        {
            gfx.DrawString(etiqueta, fontLbl, XBrushes.Black, new XRect(mIzq, y, 110, 20), XStringFormats.TopLeft);
            gfx.DrawString(valor ?? "", fontTxt, XBrushes.Black, new XRect(mIzq + 110, y, ancho - 110, 20), XStringFormats.TopLeft);
            y += 22;
        }

        private void RepositorioPdfsWeb()
        {
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
            {
                MessageBox.Show("Selecciona y carga una casa primero (Manzana / Lote).",
                    "Repositorio de PDFs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var f = new FormRepositorioDestajos(manzanaActual, loteActual, prototipoActual))
                f.ShowDialog(this);
        }

        // ------------------------------------------------------------------
        // Insumos — mismo cálculo que ConstruirTablaInsumos, pero el "Usado" sale
        // del Surtido que ya trae cada nodo desde api/destajos/arbol (sin SQL nueva).
        // ------------------------------------------------------------------

        private void InsumosWeb()
        {
            if (itemsTareas == null || itemsTareas.Count == 0)
            {
                MessageBox.Show("Carga las tareas de una Manzana y Lote para ver los insumos.",
                    "Insumos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataTable dt = ConstruirTablaInsumosWeb();
            string titulo = string.IsNullOrEmpty(manzanaActual)
                ? "Insumos — Programados / Usados"
                : $"Insumos — M{manzanaActual} L{loteActual}  (Programados / Usados)";

            using (var dlg = new Form
            {
                Text = titulo,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(880, 620),
                MinimumSize = new Size(640, 420),
                ShowInTaskbar = false,
                Font = new Font("Segoe UI", 9F)
            })
            {
                var grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    DataSource = dt,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    EnableHeadersVisualStyles = false
                };
                dlg.Controls.Add(grid);
                dlg.ShowDialog(this);
            }
        }


        // ------------------------------------------------------------------
        // Documentos: explosion de insumos y precios por destajo
        // ------------------------------------------------------------------

        /// <summary>
        /// Una fila por destajo (Nivel 1) de la casa cargada: sus insumos Material
        /// y el importe de Mano de Obra. Las dos vistas de "Documentos" (explosion
        /// y precios) se pintan con este mismo payload -- sale del arbol que ya esta
        /// en memoria (itemsTareas), sin volver a pegarle al API.
        /// </summary>
        private class DestajoCasaReporte
        {
            public int Id { get; set; }
            public string Categoria { get; set; }
            public string Destajo { get; set; }
            public string Cuadrilla { get; set; }
            public string Estado { get; set; }
            public decimal Material { get; set; }
            public decimal ManoObra { get; set; }
            public List<InsumoCasaReporte> Insumos { get; set; }
        }

        private class InsumoCasaReporte
        {
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Unidad { get; set; }
            public decimal Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Importe { get; set; }
        }

        private List<DestajoCasaReporte> ArmarReporteCasaWeb()
        {
            if (itemsTareas == null) return new List<DestajoCasaReporte>();

            var categorias = itemsTareas.Where(i => i.Nivel == 0)
                .GroupBy(i => i.ID)
                .ToDictionary(g => g.Key, g => g.First().Nombre);

            return itemsTareas
                .Where(d => d.Nivel == 1)
                .Select(d =>
                {
                    var hijos = itemsTareas.Where(i => i.ParentId == d.ID).ToList();
                    var materiales = hijos.Where(i => i.TipoTareaEnum == TipoTarea.Material).ToList();
                    return new DestajoCasaReporte
                    {
                        Id = d.ID,
                        Categoria = categorias.TryGetValue(d.ParentId, out string cat) ? cat : "(sin categor�a)",
                        Destajo = d.Nombre ?? "",
                        Cuadrilla = d.CuadrillaAsignada ?? "",
                        Estado = d.Finalizado ? "Finalizado" : d.DesatajoActivado ? "Activado" : "Pendiente",
                        Material = materiales.Sum(i => i.Total),
                        ManoObra = hijos.Where(i => i.TipoTareaEnum == TipoTarea.ManoDeObra).Sum(i => i.Total),
                        Insumos = materiales.Select(i => new InsumoCasaReporte
                        {
                            Clave = i.Clave ?? "",
                            Nombre = i.Nombre ?? "",
                            Unidad = i.Unidad ?? "",
                            Cantidad = i.Cantidad,
                            PrecioUnitario = i.PrecioUnitario,
                            Importe = i.Total
                        }).ToList()
                    };
                })
                .ToList();
        }

        private void ReporteCasaDatosWeb()
        {
            PushReporte("reporte-casa-destajos", ArmarReporteCasaWeb());
        }

        /// <summary>
        /// ids: los destajos marcados en la pagina (la vista de precios deja elegir
        /// categorias y destajos). Null o vacio = toda la casa.
        /// </summary>
        private void ReporteCasaExcelWeb(string formato, int[] ids)
        {
            var datos = ArmarReporteCasaWeb();
            int total = datos.Count;
            if (ids != null && ids.Length > 0)
            {
                var marcados = new HashSet<int>(ids);
                datos = datos.Where(d => marcados.Contains(d.Id)).ToList();
            }
            if (datos.Count == 0)
            {
                MessageBox.Show(total == 0
                        ? "Carga una manzana y lote para exportar el reporte."
                        : "No hay ningun destajo seleccionado.",
                    "Exportar a Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string nota = datos.Count < total
                ? "Seleccion parcial: " + datos.Count + " de " + total + " destajos"
                : "";

            bool precios = formato == "precios";
            string casa = string.IsNullOrEmpty(manzanaActual) ? "" : "_M" + manzanaActual + "-L" + loteActual;
            using (var dlg = new SaveFileDialog
            {
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                FileName = (precios ? "PreciosPorDestajo" : "ExplosionInsumos") + casa +
                           DateTime.Now.ToString("_yyyyMMdd_HHmm") + ".xlsx",
                Title = precios ? "Guardar reporte de precios" : "Guardar explosi�n de insumos"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    if (precios) EscribirPreciosExcel(dlg.FileName, datos, nota);
                    else EscribirExplosionExcel(dlg.FileName, datos, nota);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Registrar(ex, "FormActivarTareasTreeList.ReporteCasaExcelWeb");
                    MessageBox.Show("No se pudo guardar el archivo:" + Environment.NewLine + Environment.NewLine + ex.Message,
                        "Exportar a Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (MessageBox.Show("Archivo guardado:" + Environment.NewLine + Environment.NewLine + dlg.FileName +
                        Environment.NewLine + Environment.NewLine + "�Abrirlo ahora?",
                        "Exportar a Excel", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    Process.Start(dlg.FileName);
            }
        }

        /// <summary>Titulo + casa + fecha, comun a las dos hojas. Devuelve la fila del encabezado de columnas.</summary>
        private int EncabezadoHojaExcel(IXLWorksheet hoja, string titulo, int columnas, string nota)
        {
            hoja.Cell(1, 1).Value = titulo;
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Font.FontSize = 14;
            hoja.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(179, 108, 46);
            hoja.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            hoja.Range(1, 1, 1, columnas).Merge();

            string casa = string.IsNullOrEmpty(manzanaActual)
                ? "Sin casa cargada"
                : "Manzana " + manzanaActual + " � Lote " + loteActual;
            hoja.Cell(2, 1).Value = casa + "  �  generado " + DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                + (string.IsNullOrEmpty(nota) ? "" : "  -  " + nota);
            hoja.Cell(2, 1).Style.Font.Italic = true;
            hoja.Range(2, 1, 2, columnas).Merge();
            return 4;
        }

        private static void EncabezadosColumnas(IXLWorksheet hoja, int fila, string[] titulos)
        {
            for (int c = 0; c < titulos.Length; c++)
            {
                var celda = hoja.Cell(fila, c + 1);
                celda.Value = titulos[c];
                celda.Style.Font.Bold = true;
                celda.Style.Fill.BackgroundColor = XLColor.LightGray;
                celda.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        /// <summary>
        /// Explosion: una fila por insumo, repitiendo categoria/destajo. Plana a
        /// proposito -- asi se filtra y se pivotea en Excel; el agrupado por destajo
        /// ya lo muestra la pagina.
        /// </summary>
        private void EscribirExplosionExcel(string ruta, List<DestajoCasaReporte> datos, string nota)
        {
            using (var libro = new XLWorkbook())
            {
                var hoja = libro.Worksheets.Add("Explosion");
                int fila = EncabezadoHojaExcel(hoja, "EXPLOSI�N DE INSUMOS POR DESTAJO", 10, nota);
                EncabezadosColumnas(hoja, fila, new[]
                {
                    "Categor�a", "Destajo", "Cuadrilla", "Estado",
                    "Clave", "Insumo", "Unidad", "Cantidad", "P.U.", "Importe"
                });

                int primera = fila + 1;
                fila = primera;
                foreach (var d in datos)
                {
                    foreach (var i in d.Insumos)
                    {
                        hoja.Cell(fila, 1).Value = d.Categoria;
                        hoja.Cell(fila, 2).Value = d.Destajo;
                        hoja.Cell(fila, 3).Value = d.Cuadrilla;
                        hoja.Cell(fila, 4).Value = d.Estado;
                        hoja.Cell(fila, 5).Value = i.Clave;
                        hoja.Cell(fila, 6).Value = i.Nombre;
                        hoja.Cell(fila, 7).Value = i.Unidad;
                        hoja.Cell(fila, 8).Value = (double)i.Cantidad;
                        hoja.Cell(fila, 9).Value = (double)i.PrecioUnitario;
                        hoja.Cell(fila, 10).Value = (double)i.Importe;
                        fila++;
                    }
                }

                if (fila > primera)
                {
                    hoja.Range(primera - 1, 1, fila - 1, 10).SetAutoFilter();
                    hoja.Cell(fila, 6).Value = "TOTAL MATERIAL";
                    hoja.Cell(fila, 6).Style.Font.Bold = true;
                    hoja.Cell(fila, 10).FormulaA1 = "SUM(J" + primera + ":J" + (fila - 1) + ")";
                    hoja.Cell(fila, 10).Style.Font.Bold = true;
                }

                hoja.Column(8).Style.NumberFormat.Format = "0.00";
                hoja.Column(9).Style.NumberFormat.Format = "$#,##0.00";
                hoja.Column(10).Style.NumberFormat.Format = "$#,##0.00";
                hoja.Columns().AdjustToContents();
                libro.SaveAs(ruta);
            }
        }

        /// <summary>Precios: un renglon por destajo agrupado por categoria, con subtotales y total.</summary>
        private void EscribirPreciosExcel(string ruta, List<DestajoCasaReporte> datos, string nota)
        {
            using (var libro = new XLWorkbook())
            {
                var hoja = libro.Worksheets.Add("Precios");
                int fila = EncabezadoHojaExcel(hoja, "PRECIOS POR DESTAJO", 6, nota);
                EncabezadosColumnas(hoja, fila, new[] { "Destajo", "Cuadrilla", "Estado", "Material", "Mano de obra", "Total" });
                fila++;

                foreach (var grupo in datos.GroupBy(d => d.Categoria))
                {
                    hoja.Cell(fila, 1).Value = grupo.Key;
                    hoja.Range(fila, 1, fila, 6).Style.Font.Bold = true;
                    hoja.Range(fila, 1, fila, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 238, 228);
                    fila++;

                    foreach (var d in grupo)
                    {
                        hoja.Cell(fila, 1).Value = d.Destajo;
                        hoja.Cell(fila, 2).Value = d.Cuadrilla;
                        hoja.Cell(fila, 3).Value = d.Estado;
                        hoja.Cell(fila, 4).Value = (double)d.Material;
                        hoja.Cell(fila, 5).Value = (double)d.ManoObra;
                        hoja.Cell(fila, 6).Value = (double)(d.Material + d.ManoObra);
                        fila++;
                    }

                    hoja.Cell(fila, 1).Value = "Total " + grupo.Key;
                    hoja.Cell(fila, 4).Value = (double)grupo.Sum(d => d.Material);
                    hoja.Cell(fila, 5).Value = (double)grupo.Sum(d => d.ManoObra);
                    hoja.Cell(fila, 6).Value = (double)grupo.Sum(d => d.Material + d.ManoObra);
                    hoja.Range(fila, 1, fila, 6).Style.Font.Bold = true;
                    hoja.Range(fila, 1, fila, 6).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    fila += 2;
                }

                hoja.Cell(fila, 1).Value = "TOTAL DE LA CASA";
                hoja.Cell(fila, 4).Value = (double)datos.Sum(d => d.Material);
                hoja.Cell(fila, 5).Value = (double)datos.Sum(d => d.ManoObra);
                hoja.Cell(fila, 6).Value = (double)datos.Sum(d => d.Material + d.ManoObra);
                hoja.Range(fila, 1, fila, 6).Style.Font.Bold = true;
                hoja.Range(fila, 1, fila, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(179, 108, 46);
                hoja.Range(fila, 1, fila, 6).Style.Font.FontColor = XLColor.White;

                hoja.Columns(4, 6).Style.NumberFormat.Format = "$#,##0.00";
                hoja.Columns().AdjustToContents();
                libro.SaveAs(ruta);
            }
        }

        private DataTable ConstruirTablaInsumosWeb()
        {
            var dt = new DataTable();
            dt.Columns.Add("Clave", typeof(string));
            dt.Columns.Add("Insumo", typeof(string));
            dt.Columns.Add("Unidad", typeof(string));
            dt.Columns.Add("Programado", typeof(decimal));
            dt.Columns.Add("Usado", typeof(decimal));
            dt.Columns.Add("Pendiente", typeof(decimal));
            dt.Columns.Add("ImporteProg", typeof(decimal));
            dt.Columns.Add("ImporteUsado", typeof(decimal));

            string Clave(string clave, string nombre) =>
                !string.IsNullOrWhiteSpace(clave) ? "C:" + clave.Trim().ToUpperInvariant()
                                                  : "N:" + (nombre ?? "").Trim().ToUpperInvariant();

            var grupos = itemsTareas
                .Where(i => i.Nivel == 2 && i.TipoTareaEnum == TipoTarea.Material)
                .GroupBy(i => Clave(i.Clave, i.Nombre));

            foreach (var g in grupos)
            {
                var primero = g.First();
                decimal cant = g.Sum(x => x.Cantidad);
                // El Surtido ya es el total resuelto por clave/nombre para toda la
                // casa (api/destajos/arbol); es el mismo valor en cada nodo del grupo.
                decimal usado = primero.Surtido;
                decimal precio = primero.PrecioUnitario;

                var row = dt.NewRow();
                row["Clave"] = primero.Clave ?? "";
                row["Insumo"] = primero.Nombre ?? "";
                row["Unidad"] = primero.Unidad ?? "";
                row["Programado"] = cant;
                row["Usado"] = usado;
                row["Pendiente"] = Math.Max(0m, cant - usado);
                row["ImporteProg"] = cant * precio;
                row["ImporteUsado"] = usado * precio;
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}
