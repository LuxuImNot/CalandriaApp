// Almacen hibrido: la pagina hospedada en WebView2 (Calandria.Api/ui/almacen.html)
// sustituye el panel clasico (entradas, salidas, inventario, historial, consulta
// por casa); el resto del formulario queda oculto detras, igual que
// PanelPrincipal.Web.cs / FormActivarTareasTreeList.Web.cs.
//
// Toda lectura/escritura pasa por api/almacen (AlmacenController): nada de SQL
// directo desde este puente. El vale de salida se sigue generando en el cliente
// con PdfSharp (SalidaAlmacenService.GenerarPDFValeSalida, reutilizado tal cual,
// sin sus pasos de folio/DB que ahora vive en el API) y solo se archiva via
// api/almacen/salida/vale.
//
// Se puede apagar sin recompilar con AlmacenWeb=false en App.config.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
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

namespace DynamicSepticSystem
{
    public partial class FormAlmacen
    {
        private WebView2 webAlmacen;
        private readonly List<Control> ocultosPorAlmacenWeb = new List<Control>();

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "almacen.html");

        private static bool AlmacenWebActivo
        {
            get
            {
                var v = ConfigurationManager.AppSettings["AlmacenWeb"];
                return string.IsNullOrWhiteSpace(v) || v.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        // ------------------------------------------------------------------
        // Montaje
        // ------------------------------------------------------------------

        private void InicializarPanelWeb()
        {
            if (!AlmacenWebActivo) return;

            try
            {
                CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (WebView2RuntimeNotFoundException)
            {
                ErrorLogger.RegistrarMensaje("AlmacenWeb", "WebView2 no está instalado; se usa el formulario clásico.");
                return;
            }
            catch { return; }

            ocultosPorAlmacenWeb.Clear();
            foreach (Control c in this.Controls)
                if (c.Visible) ocultosPorAlmacenWeb.Add(c);

            webAlmacen = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            this.Controls.Add(webAlmacen);
            webAlmacen.BringToFront();

            foreach (var c in ocultosPorAlmacenWeb) c.Visible = false;

            webAlmacen.CoreWebView2InitializationCompleted += WebAlmacen_Init;
            _ = IniciarWebView2Async();
        }

        /// <summary>
        /// Entorno propio con su propia carpeta de datos de usuario: FormAlmacen se
        /// abre no-modal (Show, ver PanelPrincipal.AbrirFormAlmacen) junto al panel
        /// principal y a Destajos, que ya tienen su propio WebView2 activo. Compartir
        /// el entorno implicito entre varios controles WebView2 del mismo proceso ya
        /// causo un COMException que tumbaba el proceso entero (ver
        /// reference_webview2_multi_instance / MIGRACION_UI_WEB.md §5.1); por eso
        /// entorno + carpeta separados, y todo el arranque en try/catch para caer al
        /// formulario clasico ante cualquier fallo en vez de crashear.
        /// </summary>
        private async Task IniciarWebView2Async()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2Almacen");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webAlmacen.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("AlmacenWeb", "WebView2 no pudo iniciar: " + ex.Message);
                if (!IsDisposed) RestaurarPanelClasico();
            }
        }

        private void WebAlmacen_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("AlmacenWeb", "WebView2 no inicializó: " + e.InitializationException);
                RestaurarPanelClasico();
                return;
            }

            var core = webAlmacen.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebAlmacen_Mensaje;
            core.NavigationCompleted += async (s2, e2) =>
            {
                // La vista por defecto de la pagina es Entradas; empuja sus ordenes
                // pendientes de una vez para que no se vea vacia hasta el primer clic
                // del riel (la pagina solo pide datos cuando el usuario cambia de vista).
                if (e2.IsSuccess) { EnviarSesionWeb(); await CargarOrdenesWeb(); await CargarTemaObraAsync(); }
            };

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("AlmacenWeb", "Fallo cargando la página: " + ex.Message);
            }

            if (IsDisposed || webAlmacen?.CoreWebView2 == null) return;
            if (html == null) { RestaurarPanelClasico(); return; }
            webAlmacen.CoreWebView2.NavigateToString(html);
        }

        private void RestaurarPanelClasico()
        {
            if (webAlmacen != null) webAlmacen.Visible = false;
            foreach (var c in ocultosPorAlmacenWeb) c.Visible = true;
        }

        // ------------------------------------------------------------------
        // Obtención del HTML: API -> caché (idéntico patrón que Destajos/Panel)
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/almacen/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/almacen");
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
                ErrorLogger.RegistrarMensaje("AlmacenWeb", "No se pudo traer la UI del API: " + ex.Message);
            }

            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webAlmacen?.CoreWebView2 == null) return;
            try
            {
                webAlmacen.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings));
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("AlmacenWeb", "Fallo al enviar datos a la página: " + ex.Message);
            }
        }

        /// <summary>Manda la paleta de acento de la obra activa (cosmético, nunca bloquea si falla).</summary>
        private async Task CargarTemaObraAsync()
        {
            object msg = null;
            try { msg = await Task.Run(() => TemaObra.ObtenerParaPush()); }
            catch { /* cosmético */ }
            if (!IsDisposed && msg != null) Push(msg);
        }

        /// <summary>
        /// Usuario/perfil y si puede editar (almacen.editar) para que la página
        /// oculte Entradas/Salidas a quien sólo tiene almacen.ver. El servidor
        /// también lo exige (RequierePermiso): esto es sólo para no mostrar
        /// botones que van a fallar.
        /// </summary>
        private void EnviarSesionWeb()
        {
            Push(new
            {
                tipo = "sesion",
                usuario = Global.UsuarioActual?.Nombre ?? "",
                perfil = Global.UsuarioActual?.Perfil ?? "",
                puedeEditar = Global.UsuarioActual?.TienePermiso("almacen.editar") ?? false
            });
        }

        private void PushError(string mensaje)
        {
            if (webAlmacen?.CoreWebView2 == null) return;
            try
            {
                webAlmacen.CoreWebView2.PostWebMessageAsJson(
                    JsonConvert.SerializeObject(new { tipo = "error", mensaje }, CamelCaseSettings));
            }
            catch { /* si ni el aviso de error sale, no hay más que intentar */ }
        }

        private void ManejarErrorApi(Exception ex, string contexto)
        {
            var apiEx = ex as ApiException;
            string msg = apiEx != null ? MensajeDe(apiEx) : ex.Message;
            ErrorLogger.RegistrarMensaje("AlmacenWeb", contexto + ": " + msg);
            MessageBox.Show(contexto + ":\n\n" + msg, "Almacén", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private class MsgAccion { public string accion; }
        private class MsgLotes { public string manzana; public string seccion; }
        private class MsgMateriales { public string manzana; public string lote; public string ruta; }
        private class MsgOcDetalle { public string folio; }
        private class MsgEliminarLinea { public int id; public string justificacion; }
        private class MsgConciliarFactura { public string folioOC; public string xmlBase64; }
        private class MsgLineaEntrada { public int idDetalle; public decimal cantidadRecibida; public string justificacion; }
        private class MsgCapturarEntrada
        {
            public string folioOC;
            public string manzana;
            public string lote;
            public string prototipo;
            public List<MsgLineaEntrada> lineas;
        }
        private class MsgCasa { public string manzana; public string lote; }
        private class MsgInsumoSalida
        {
            public string clave;
            public decimal cantidadSolicitada;
            public decimal precioUnitario;
            public string descripcion;
            public string unidad;
        }
        private class MsgSurtir
        {
            public string manzana;
            public string lote;
            public string prototipo;
            public List<MsgInsumoSalida> insumos;
        }
        private class MsgInsumoMovimiento
        {
            public string clave;
            public string descripcion;
            public string unidad;
            public decimal cantidad;
            public decimal precioUnitario;
            public decimal importe;
            public string justificacion;
        }
        private class MsgGenerarVale
        {
            public string folio;
            public string manzana;
            public string lote;
            public string prototipo;
            public decimal totalImporte;
            public List<MsgInsumoMovimiento> insumos;
        }

        private void WebAlmacen_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;
            MsgAccion m;
            try { m = JsonConvert.DeserializeObject<MsgAccion>(json); }
            catch { return; }
            if (m?.accion == null) return;

            // BeginInvoke: igual que en Destajos, para no despachar dentro del propio
            // callback COM de WebView2 (ver reference_webview2_multi_instance).
            BeginInvoke((Action)(() => DespacharMensaje(m.accion, json)));
        }

        // async void: punto de entrada del despacho (vía BeginInvoke); nadie lo espera.
        // Las excepciones, incluidas las de las esperas (await) de más abajo, quedan
        // cubiertas por el try/catch.
        private async void DespacharMensaje(string accion, string json)
        {
            try
            {
                switch (accion)
                {
                    case "cargar-manzanas":
                        await CargarManzanasWeb();
                        break;
                    case "cargar-lotes":
                        { var d = JsonConvert.DeserializeObject<MsgLotes>(json); await CargarLotesWeb(d.manzana, d.seccion); }
                        break;
                    case "cargar-materiales":
                        { var d = JsonConvert.DeserializeObject<MsgMateriales>(json); await CargarMaterialesWeb(d.manzana, d.lote, d.ruta); }
                        break;
                    case "cargar-ordenes":
                        await CargarOrdenesWeb();
                        break;
                    case "cargar-oc-detalle":
                        { var d = JsonConvert.DeserializeObject<MsgOcDetalle>(json); await CargarOcDetalleWeb(d.folio); }
                        break;
                    case "capturar-entrada":
                        { var d = JsonConvert.DeserializeObject<MsgCapturarEntrada>(json); await CapturarEntradaWeb(d); }
                        break;
                    case "eliminar-linea-oc":
                        { var d = JsonConvert.DeserializeObject<MsgEliminarLinea>(json); await EliminarLineaOcWeb(d.id, d.justificacion); }
                        break;
                    case "conciliar-factura":
                        { var d = JsonConvert.DeserializeObject<MsgConciliarFactura>(json); await ConciliarFacturaWeb(d.folioOC, d.xmlBase64); }
                        break;
                    case "cargar-inventario":
                        await CargarInventarioWeb();
                        break;
                    case "cargar-historial":
                        await CargarHistorialWeb();
                        break;
                    case "cargar-casa-salida":
                        { var d = JsonConvert.DeserializeObject<MsgCasa>(json); await CargarCasaSalidaWeb(d.manzana, d.lote); }
                        break;
                    case "surtir":
                        { var d = JsonConvert.DeserializeObject<MsgSurtir>(json); await SurtirWeb(d); }
                        break;
                    case "generar-vale-pdf":
                        { var d = JsonConvert.DeserializeObject<MsgGenerarVale>(json); await GenerarValeWeb(d); }
                        break;
                    case "volver":
                        RestaurarPanelClasico();
                        break;

                    default:
                        ErrorLogger.RegistrarMensaje("AlmacenWeb", "Acción desconocida desde la página: " + accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo completar la acción solicitada:\n\n" + ex.Message,
                    "Almacén", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ------------------------------------------------------------------
        // Lecturas — todas vía api/almacen, nada de SQL directo
        // ------------------------------------------------------------------

        private async Task CargarManzanasWeb()
        {
            try
            {
                var lista = await Task.Run(() => ApiClient.Get<List<string>>("/api/almacen/manzanas")) ?? new List<string>();
                Push(new { tipo = "manzanas", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las manzanas"); }
        }

        private async Task CargarLotesWeb(string manzana, string seccion)
        {
            if (string.IsNullOrWhiteSpace(manzana)) return;
            try
            {
                var lista = await Task.Run(() => ApiClient.Get<List<string>>("/api/almacen/lotes?manzana=" + Uri.EscapeDataString(manzana))) ?? new List<string>();
                Push(new { tipo = "lotes", lista, seccion });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los lotes"); }
        }

        private async Task CargarMaterialesWeb(string manzana, string lote, string ruta)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote)) return;
            try
            {
                string url = "/api/almacen/materiales?manzana=" + Uri.EscapeDataString(manzana)
                    + "&lote=" + Uri.EscapeDataString(lote)
                    + "&ruta=" + Uri.EscapeDataString(string.IsNullOrWhiteSpace(ruta) ? "Todas" : ruta);
                var lista = await Task.Run(() => ApiClient.Get<List<MaterialCasaApi>>(url)) ?? new List<MaterialCasaApi>();
                Push(new { tipo = "materiales", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar la consulta por casa"); }
        }

        private async Task CargarOrdenesWeb()
        {
            try
            {
                var lista = await Task.Run(() => ApiClient.Get<List<OrdenPendienteApi>>("/api/almacen/ordenes-pendientes")) ?? new List<OrdenPendienteApi>();
                Push(new { tipo = "ordenes", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las órdenes pendientes"); }
        }

        private async Task CargarOcDetalleWeb(string folio)
        {
            if (string.IsNullOrWhiteSpace(folio)) return;
            try
            {
                var detalle = await Task.Run(() => ApiClient.Get<List<OcDetallePendienteApi>>(
                    "/api/almacen/oc/" + Uri.EscapeDataString(folio) + "/detalle")) ?? new List<OcDetallePendienteApi>();
                var casas = await Task.Run(() => ApiClient.Get<List<OcCasaApi>>(
                    "/api/almacen/oc/" + Uri.EscapeDataString(folio) + "/casas")) ?? new List<OcCasaApi>();
                Push(new { tipo = "ocDetalle", folio, detalle, casas });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el detalle de la orden"); }
        }

        private async Task CargarInventarioWeb()
        {
            try
            {
                var lista = await Task.Run(() => ApiClient.Get<List<InventarioAlmacenItemApi>>("/api/almacen/inventario")) ?? new List<InventarioAlmacenItemApi>();
                Push(new { tipo = "inventario", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el inventario"); }
        }

        private async Task CargarHistorialWeb()
        {
            try
            {
                var lista = await Task.Run(() => ApiClient.Get<List<HistorialMovimientoApi>>("/api/almacen/historial")) ?? new List<HistorialMovimientoApi>();
                Push(new { tipo = "historial", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el historial"); }
        }

        private async Task CargarCasaSalidaWeb(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote)) return;
            try
            {
                var d = await Task.Run(() => ApiClient.Get<PendientesSalidaApi>(
                    "/api/almacen/salida/pendientes?manzana=" + Uri.EscapeDataString(manzana) + "&lote=" + Uri.EscapeDataString(lote)));
                if (d == null) { PushError("No se encontró la casa M" + manzana + " L" + lote + "."); return; }
                Push(new { tipo = "pendientesSalida", manzana = d.Manzana, lote = d.Lote, prototipo = d.Prototipo, insumos = d.Insumos, mensaje = d.Mensaje });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar la casa"); }
        }

        // ------------------------------------------------------------------
        // Escrituras — todas vía api/almacen
        // ------------------------------------------------------------------

        private async Task CapturarEntradaWeb(MsgCapturarEntrada d)
        {
            if (d?.lineas == null || d.lineas.Count == 0) return;
            try
            {
                var resultado = await Task.Run(() => ApiClient.Post<ResultadoEntradaApi>("/api/almacen/entrada", new
                {
                    FolioOC = d.folioOC,
                    Manzana = d.manzana,
                    Lote = d.lote,
                    Prototipo = d.prototipo,
                    Lineas = d.lineas.Select(l => new { IdDetalle = l.idDetalle, CantidadRecibida = l.cantidadRecibida, Justificacion = l.justificacion })
                }));
                Push(new { tipo = "resultadoEntrada", ok = resultado?.Ok ?? false, mensaje = resultado?.Mensaje, insumos = resultado?.Insumos });
            }
            catch (ApiException ex)
            {
                Push(new { tipo = "resultadoEntrada", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo capturar la entrada"); }
        }

        private async Task EliminarLineaOcWeb(int id, string justificacion)
        {
            if (string.IsNullOrWhiteSpace(justificacion))
            {
                Push(new { tipo = "lineaEliminada", ok = false, mensaje = "La justificación es obligatoria." });
                return;
            }
            try
            {
                await Task.Run(() => ApiClient.Post("/api/almacen/oc/detalle/" + id + "/eliminar", new { Justificacion = justificacion }));
                Push(new { tipo = "lineaEliminada", ok = true, mensaje = "Renglón eliminado." });
            }
            catch (ApiException ex)
            {
                Push(new { tipo = "lineaEliminada", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo eliminar el renglón"); }
        }

        // Igual que BtnConciliarFactura_Click en FormAlmacen_Entradas.cs: sube el CFDI,
        // el servidor lo compara contra el detalle pendiente por clave/nombre y la página
        // precarga "recibido" con lo facturado.
        private async Task ConciliarFacturaWeb(string folioOC, string xmlBase64)
        {
            if (string.IsNullOrWhiteSpace(folioOC) || string.IsNullOrWhiteSpace(xmlBase64)) return;
            try
            {
                var resultado = await Task.Run(() => ApiClient.Post<ConciliarFacturaResponseApi>(
                    "/api/ordenescompra/conciliar-factura",
                    new { FolioOC = folioOC, XmlBase64 = xmlBase64 }));
                if (resultado == null) { PushError("El servidor no devolvió conciliación."); return; }
                Push(new
                {
                    tipo = "conciliacion",
                    emisor = resultado.Emisor,
                    rfc = resultado.Rfc,
                    uuid = resultado.Uuid,
                    totalFactura = resultado.TotalFactura,
                    usoIA = resultado.UsoIA,
                    aviso = resultado.Aviso,
                    lineas = resultado.Lineas,
                    sinAsignar = resultado.SinAsignar
                });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo conciliar la factura"); }
        }

        private async Task SurtirWeb(MsgSurtir d)
        {
            if (d?.insumos == null || d.insumos.Count == 0) return;
            try
            {
                var resultado = await Task.Run(() => ApiClient.Post<ResultadoSalidaApi>("/api/almacen/salida", new
                {
                    Manzana = d.manzana,
                    Lote = d.lote,
                    Prototipo = d.prototipo,
                    Insumos = d.insumos.Select(i => new
                    {
                        Clave = i.clave,
                        Descripcion = i.descripcion,
                        Unidad = i.unidad,
                        CantidadSolicitada = i.cantidadSolicitada,
                        PrecioUnitario = i.precioUnitario
                    })
                }));
                Push(new
                {
                    tipo = "resultadoSalida",
                    ok = resultado?.Ok ?? false,
                    mensaje = resultado?.Mensaje,
                    folio = resultado?.Folio,
                    totalImporte = resultado?.TotalImporte ?? 0m,
                    insumos = resultado?.Insumos
                });
            }
            catch (ApiException ex)
            {
                Push(new { tipo = "resultadoSalida", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo registrar la salida"); }
        }

        /// <summary>
        /// Genera el PDF del vale (PdfSharp, vía SalidaAlmacenService.GenerarPDFValeSalida
        /// reutilizado tal cual — no toca DB, sólo arma el archivo) y lo archiva vía
        /// api/almacen/salida/vale. El folio ya lo resolvió el servidor en /api/almacen/salida;
        /// aquí no se vuelve a generar folio ni se llama a
        /// CrearTablasRepositorioSalidasSiNoExisten/GuardarValeSalidaEnRepositorio (esas
        /// escrituras ya viven en el API, llamarlas aquí también duplicaría el registro).
        /// </summary>
        private async Task GenerarValeWeb(MsgGenerarVale d)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.folio)) return;
            try
            {
                var datos = SolicitarDatosVale();
                if (datos == null) return; // cancelado

                var insumosSalida = (d.insumos ?? new List<MsgInsumoMovimiento>()).Select(i => new InsumoSalida
                {
                    Codigo = i.clave,
                    Descripcion = i.descripcion,
                    Unidad = i.unidad,
                    Cantidad = i.cantidad,
                    PrecioUnitario = i.precioUnitario,
                    Importe = i.importe
                }).ToList();

                var svc = new SalidaAlmacenService(connectionString);
                string rutaPdf = svc.GenerarPDFValeSalida(
                    d.folio, insumosSalida, d.totalImporte, datos, d.manzana, d.lote, abrirPdf: true);

                byte[] pdfBytes = File.ReadAllBytes(rutaPdf);
                await Task.Run(() => ApiClient.Post("/api/almacen/salida/vale", new
                {
                    Folio = d.folio,
                    Manzana = d.manzana,
                    Lote = d.lote,
                    Prototipo = d.prototipo,
                    TotalImporte = d.totalImporte,
                    Solicitante = datos.Solicitante,
                    ResidenteObra = datos.ResidenteObra,
                    EncargadoAlmacen = datos.EncargadoAlmacen,
                    Observaciones = datos.Observaciones,
                    NombreArchivo = Path.GetFileName(rutaPdf),
                    PdfBase64 = Convert.ToBase64String(pdfBytes),
                    Insumos = d.insumos
                }));

                Push(new { tipo = "valeGenerado", ok = true, mensaje = "Vale archivado correctamente." });
            }
            catch (ApiException ex)
            {
                Push(new { tipo = "valeGenerado", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("AlmacenWeb", "Error generando el vale: " + ex.Message);
                Push(new { tipo = "valeGenerado", ok = false, mensaje = ex.Message });
            }
        }
    }
}
