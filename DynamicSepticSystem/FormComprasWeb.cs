// Compras híbrido: ventana dedicada que hospeda en WebView2 la página
// Calandria.Api/ui/compras.html (orden múltiple, compra indirecta y consulta
// de órdenes en una sola UI con rail de navegación), igual que
// FormAlmacen.Web.cs / FormActivarTareasTreeList.Web.cs. A diferencia de esas
// pantallas, aquí no hay un formulario clásico que ocultar: FormComprasWeb
// reemplaza directamente a FormCompraMulti/FormCompraIndirecta cuando
// ComprasWeb=true (ver PanelPrincipal.AbrirFormCompraMulti/Indirecta); con
// ComprasWeb=false los formularios clásicos siguen intactos.
//
// Toda lectura/escritura pasa por api/compras, api/ordenescompra,
// api/repositorio-ordenes y api/proveedores (ApiClient): nada de SQL directo
// desde este puente. El PDF se genera en el cliente con PdfSharp
// (OrdenCompraPdfService, extraído tal cual de FormCompraMulti/
// FormCompraIndirecta) y se archiva en el repositorio vía API.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
    public class FormComprasWeb : Form
    {
        private readonly string _vistaInicial;
        private WebView2 webCompras;

        /// <summary>Casa con la que abre la ventana cuando viene del tablero.</summary>
        private string _preMz, _preLote;

        /// <summary>
        /// Deja la casa del tablero ya agregada a la orden multiple, para no
        /// volver a teclear manzana y lote.
        /// </summary>
        public void PreseleccionarCasa(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote)) return;
            _preMz = manzana.Trim();
            _preLote = lote.Trim();
            EnviarPreseleccion();
        }

        private void EnviarPreseleccion()
        {
            if (string.IsNullOrEmpty(_preMz) || webCompras?.CoreWebView2 == null) return;
            Push(new { tipo = "preseleccion", manzana = _preMz, lote = _preLote });
        }

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "compras.html");

        public FormComprasWeb(string vistaInicial = "multi")
        {
            _vistaInicial = vistaInicial;

            Text = "Compras - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1280, 800);
            MinimumSize = new Size(1024, 640);
            WindowState = FormWindowState.Maximized;
            BackColor = Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webCompras = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webCompras);

            webCompras.CoreWebView2InitializationCompleted += WebCompras_Init;
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
                    "Calandria", "WebView2Compras");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webCompras.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("ComprasWeb", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo iniciar el módulo web de Compras:\n\n" + ex.Message,
                    "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
            }
        }

        private void WebCompras_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("ComprasWeb", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar el módulo web de Compras.", "Compras",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            var core = webCompras.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebCompras_Mensaje;
            core.NavigationCompleted += (s2, e2) =>
            {
                if (!e2.IsSuccess) return;
                EnviarSesionWeb();
                if (_vistaInicial != "multi")
                    Push(new { tipo = "vistaInicial", vista = _vistaInicial });
                EnviarPreseleccion();
                _ = CargarTemaObraAsync();
            };

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("ComprasWeb", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webCompras?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz web de Compras.", "Compras",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            webCompras.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/compras/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/compras");
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
                ErrorLogger.RegistrarMensaje("ComprasWeb", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webCompras?.CoreWebView2 == null) return;
            try { webCompras.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("ComprasWeb", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        /// <summary>Manda la paleta de acento de la obra activa (cosmético, nunca bloquea si falla).</summary>
        private async Task CargarTemaObraAsync()
        {
            object msg = null;
            try { msg = await Task.Run(() => TemaObra.ObtenerParaPush()); }
            catch { /* cosmético */ }
            if (!IsDisposed && msg != null) Push(msg);
        }

        /// <summary>Usuario/perfil y si puede editar (compras.editar), para ocultar Nueva orden/Indirecta a quien sólo tiene compras.ver.</summary>
        private void EnviarSesionWeb()
        {
            Push(new
            {
                tipo = "sesion",
                usuario = Global.UsuarioActual?.Nombre ?? "",
                perfil = Global.UsuarioActual?.Perfil ?? "",
                puedeEditar = Global.UsuarioActual?.TienePermiso("compras.editar") ?? false
            });
        }

        private void PushError(string mensaje)
        {
            if (webCompras?.CoreWebView2 == null) return;
            try { webCompras.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { tipo = "error", mensaje }, CamelCaseSettings)); }
            catch { /* si ni el aviso de error sale, no hay más que intentar */ }
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

        private void ManejarErrorApi(Exception ex, string contexto)
        {
            var apiEx = ex as ApiException;
            string msg = apiEx != null ? MensajeDe(apiEx) : ex.Message;
            ErrorLogger.RegistrarMensaje("ComprasWeb", contexto + ": " + msg);
            PushError(contexto + ": " + msg);
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgCasaManzanaLote { public string manzana; public string lote; public string prototipo; }
        private class MsgCasaPrototipo { public string manzana; public string lote; public string prototipo; }
        private class MsgCargarCatalogo { public string prototipo; }
        private class MsgCargarCatalogoMulti { public List<MsgCasaPrototipo> casas; }
        private class MsgUpsertCatalogoMulti
        {
            public List<string> prototipos;
            public string clave, descripcion, unidad, familia;
            public decimal cantidad;
            public decimal? precio;
            public bool actualizarCampos;
        }
        private class MsgEliminarCatalogoMulti { public List<string> prototipos; public string clave; }
        private class MsgDetalleJs
        {
            public string clave, descripcion, unidad, familia;
            public decimal cantidad, precioUnitario, importeTotal;
        }
        private class MsgGenerarMultiple
        {
            public string nombreOrden;
            public List<MsgCasaManzanaLote> casas;
            public string proveedorNombre, proveedorClave;
            public List<MsgDetalleJs> detalles;
        }
        private class MsgGenerarIndirecta
        {
            public string nombreOrden, tipoTitulo, proveedorNombre, proveedorClave;
            public List<MsgDetalleJs> detalles;
        }
        private class MsgCrearIndirecto { public string clave, descripcion, unidad; }
        private class MsgCrearProveedor { public string claveUnica, nombre, rfc, direccion, telefono; }
        private class MsgRepositorio { public bool soloIndirectas; }
        private class MsgFolioId { public int folioId; public string folio; }

        private void WebCompras_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
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
            try
            {
                switch (accion)
                {
                    case "cargar-proveedores": CargarProveedoresWeb(); break;
                    case "cargar-catalogo":
                        { var d = JsonConvert.DeserializeObject<MsgCargarCatalogo>(json); CargarCatalogoWeb(d.prototipo); }
                        break;
                    case "agregar-casa":
                        { var d = JsonConvert.DeserializeObject<MsgCasaManzanaLote>(json); AgregarCasaWeb(d.manzana, d.lote); }
                        break;
                    case "cargar-catalogo-multi":
                        { var d = JsonConvert.DeserializeObject<MsgCargarCatalogoMulti>(json); CargarCatalogoMultiWeb(d.casas); }
                        break;
                    case "upsert-catalogo-multi":
                        { var d = JsonConvert.DeserializeObject<MsgUpsertCatalogoMulti>(json); UpsertCatalogoMultiWeb(d); }
                        break;
                    case "eliminar-catalogo-multi":
                        { var d = JsonConvert.DeserializeObject<MsgEliminarCatalogoMulti>(json); EliminarCatalogoMultiWeb(d); }
                        break;
                    case "generar-orden-multiple":
                        { var d = JsonConvert.DeserializeObject<MsgGenerarMultiple>(json); GenerarOrdenMultipleWeb(d); }
                        break;
                    case "cargar-indirectas": CargarIndirectasWeb(); break;
                    case "crear-insumo-indirecto":
                        { var d = JsonConvert.DeserializeObject<MsgCrearIndirecto>(json); CrearInsumoIndirectoWeb(d); }
                        break;
                    case "generar-orden-indirecta":
                        { var d = JsonConvert.DeserializeObject<MsgGenerarIndirecta>(json); GenerarOrdenIndirectaWeb(d); }
                        break;
                    case "crear-proveedor":
                        { var d = JsonConvert.DeserializeObject<MsgCrearProveedor>(json); CrearProveedorWeb(d); }
                        break;
                    case "cargar-repositorio":
                        { var d = JsonConvert.DeserializeObject<MsgRepositorio>(json); CargarRepositorioWeb(d.soloIndirectas); }
                        break;
                    case "ver-pdf-repositorio":
                        { var d = JsonConvert.DeserializeObject<MsgFolioId>(json); VerPdfRepositorioWeb(d.folioId, d.folio); }
                        break;
                    case "ver-detalle-repositorio":
                        { var d = JsonConvert.DeserializeObject<MsgFolioId>(json); VerDetalleRepositorioWeb(d.folioId); }
                        break;
                    case "eliminar-repositorio":
                        { var d = JsonConvert.DeserializeObject<MsgFolioId>(json); EliminarRepositorioWeb(d.folioId); }
                        break;
                    case "volver":
                        Close();
                        break;
                    default:
                        ErrorLogger.RegistrarMensaje("ComprasWeb", "Acción desconocida desde la página: " + accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                ManejarErrorApi(ex, "No se pudo completar la acción solicitada");
            }
        }

        // ------------------------------------------------------------------
        // Proveedores
        // ------------------------------------------------------------------

        private void CargarProveedoresWeb()
        {
            try { Push(new { tipo = "proveedores", lista = ApiClient.Get<List<ProveedorApi>>("/api/proveedores") ?? new List<ProveedorApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los proveedores"); }
        }

        private void CargarCatalogoWeb(string prototipo)
        {
            try
            {
                Push(new
                {
                    tipo = "catalogo",
                    prototipo,
                    lista = ApiClient.Get<List<CatalogoMaterialApi>>("/api/compras/catalogo?prototipo=" + Uri.EscapeDataString(prototipo ?? "")) ?? new List<CatalogoMaterialApi>()
                });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el catálogo"); }
        }

        private void CrearProveedorWeb(MsgCrearProveedor d)
        {
            try
            {
                var resp = ApiClient.Post<ProveedorCreadoApi>("/api/proveedores", new
                {
                    ClaveUnica = d.claveUnica,
                    Nombre = d.nombre,
                    Rfc = d.rfc,
                    Direccion = d.direccion,
                    Telefono = d.telefono
                });
                Push(new { tipo = "proveedorCreado", ok = true, mensaje = $"Proveedor guardado correctamente. Folio: {resp?.Folio}" });
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Push(new { tipo = "proveedorCreado", ok = false, mensaje = "Ya existe un proveedor con esa Clave Única." });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo guardar el proveedor"); }
        }

        // ------------------------------------------------------------------
        // Orden múltiple — casas, catálogo agregado, explosión
        // ------------------------------------------------------------------

        private void AgregarCasaWeb(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote)) return;
            try
            {
                string prototipo = ApiClient.Get<string>(
                    $"/api/compras/prototipo?manzana={Uri.EscapeDataString(manzana)}&lote={Uri.EscapeDataString(lote)}");
                if (string.IsNullOrEmpty(prototipo)) { PushError("No se encontró el prototipo para esta casa."); return; }
                Push(new { tipo = "casaAgregada", manzana, lote, prototipo });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo agregar la casa"); }
        }

        /// <summary>
        /// Réplica de FormCompraMulti.ActualizarCatalogoInsumos: por cada casa suma
        /// los insumos de destajos activados (catalogo-destajos) y les resta lo ya
        /// pendiente (pendientes), agrupando por clave o, si no hay clave, por
        /// descripción normalizada. Quita lo que quede en 0 o negativo.
        /// </summary>
        private void CargarCatalogoMultiWeb(List<MsgCasaPrototipo> casas)
        {
            casas = casas ?? new List<MsgCasaPrototipo>();
            try
            {
                var insumosTotales = new Dictionary<string, CatalogoMaterialApi>();
                var faltantesPorClave = new Dictionary<string, decimal>();

                foreach (var casa in casas)
                {
                    var pendientes = ApiClient.Get<List<PendienteMaterialApi>>(
                        $"/api/compras/pendientes?manzana={Uri.EscapeDataString(casa.manzana ?? "")}&lote={Uri.EscapeDataString(casa.lote ?? "")}")
                        ?? new List<PendienteMaterialApi>();
                    foreach (var p in pendientes)
                    {
                        if (p.CantidadPendiente <= 0) continue;
                        faltantesPorClave[p.Clave] = faltantesPorClave.TryGetValue(p.Clave, out var acc) ? acc + p.CantidadPendiente : p.CantidadPendiente;
                    }
                }

                foreach (var casa in casas)
                {
                    var filas = ApiClient.Get<List<CatalogoMaterialApi>>(
                        "/api/compras/catalogo-destajos" +
                        $"?prototipo={Uri.EscapeDataString(casa.prototipo ?? "")}" +
                        $"&manzana={Uri.EscapeDataString(casa.manzana ?? "")}" +
                        $"&lote={Uri.EscapeDataString(casa.lote ?? "")}") ?? new List<CatalogoMaterialApi>();

                    foreach (var fila in filas)
                    {
                        string clave = fila.Clave ?? "";
                        string key = !string.IsNullOrWhiteSpace(clave) ? "C:" + clave : "N:" + (fila.Descripcion ?? "").Trim().ToUpperInvariant();

                        if (insumosTotales.TryGetValue(key, out var existente))
                        {
                            existente.Cantidad += Math.Round(fila.Cantidad, 3);
                        }
                        else
                        {
                            insumosTotales[key] = new CatalogoMaterialApi
                            {
                                Clave = clave,
                                Descripcion = fila.Descripcion,
                                Unidad = fila.Unidad,
                                Cantidad = Math.Round(fila.Cantidad, 3),
                                Familia = fila.Familia,
                                Precio = fila.Precio,
                                ClaveResuelta = fila.ClaveResuelta
                            };
                        }
                    }
                }

                foreach (var insumo in insumosTotales.Values)
                {
                    if (string.IsNullOrWhiteSpace(insumo.Clave)) continue;
                    if (faltantesPorClave.TryGetValue(insumo.Clave, out var pendiente) && pendiente > 0)
                        insumo.Cantidad = Math.Round(insumo.Cantidad - pendiente, 3);
                }

                var lista = insumosTotales.Values.Where(i => i.Cantidad > 0).ToList();
                Push(new { tipo = "catalogoMulti", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el catálogo de insumos"); }
        }

        /// <summary>Réplica de AgregarInsumoAExplosion / editarInsumoToolStripMenuItem_Click: aplica en cada prototipo, acumulando errores.</summary>
        private void UpsertCatalogoMultiWeb(MsgUpsertCatalogoMulti d)
        {
            var prototipos = d.prototipos ?? new List<string>();
            int aplicados = 0;
            var errores = new List<string>();
            foreach (var prototipo in prototipos)
            {
                try
                {
                    ApiClient.Post("/api/compras/catalogo", new
                    {
                        Prototipo = prototipo,
                        Clave = d.clave,
                        Descripcion = d.descripcion ?? "",
                        Unidad = d.unidad ?? "",
                        Cantidad = d.cantidad,
                        Familia = d.familia ?? "MANUAL",
                        Precio = d.precio,
                        ActualizarCampos = d.actualizarCampos
                    });
                    aplicados++;
                }
                catch (Exception ex) { errores.Add($"{prototipo}: {ex.Message}"); }
            }
            string mensaje = errores.Count > 0
                ? $"Explosión actualizada en {aplicados} de {prototipos.Count} tabla(s). Errores: {string.Join(" | ", errores)}"
                : "Explosión actualizada.";
            Push(new { tipo = "upsertResultado", ok = errores.Count == 0, mensaje });
        }

        /// <summary>Réplica de btnEliminarInsumo_Click: elimina la clave de la explosión de cada prototipo.</summary>
        private void EliminarCatalogoMultiWeb(MsgEliminarCatalogoMulti d)
        {
            var prototipos = d.prototipos ?? new List<string>();
            int eliminados = 0;
            var errores = new List<string>();
            foreach (var prototipo in prototipos)
            {
                try { eliminados += ApiClient.Post<int>("/api/compras/catalogo/eliminar", new { Prototipo = prototipo, Clave = d.clave }); }
                catch (Exception ex) { errores.Add($"{prototipo}: {ex.Message}"); }
            }
            string mensaje = errores.Count > 0
                ? $"Se eliminaron {eliminados} fila(s). Errores: {string.Join(" | ", errores)}"
                : $"Insumo '{d.clave}' eliminado correctamente de {prototipos.Count} tabla(s) ({eliminados} fila(s)).";
            Push(new { tipo = "eliminarCatalogoResultado", ok = errores.Count == 0, mensaje, clave = d.clave });
        }

        private static List<DetalleOrdenApi> DetallesDe(List<MsgDetalleJs> detalles) =>
            (detalles ?? new List<MsgDetalleJs>()).Select(i => new DetalleOrdenApi
            {
                Clave = i.clave,
                Descripcion = i.descripcion,
                Unidad = i.unidad,
                Cantidad = i.cantidad,
                PrecioUnitario = i.precioUnitario,
                ImporteTotal = i.importeTotal,
                Familia = i.familia
            }).ToList();

        private void GenerarOrdenMultipleWeb(MsgGenerarMultiple d)
        {
            string folioOC;
            try
            {
                var detalles = DetallesDe(d.detalles);
                var resp = ApiClient.Post<FolioOrdenApi>("/api/ordenescompra/multiple", new
                {
                    Usuario = Environment.UserName,
                    NombreOrden = d.nombreOrden,
                    Casas = (d.casas ?? new List<MsgCasaManzanaLote>()).Select(c => new CasaOrdenApi { Manzana = c.manzana, Lote = c.lote }).ToList(),
                    Detalles = detalles
                });
                folioOC = resp?.FolioOC;
                if (string.IsNullOrEmpty(folioOC)) throw new Exception("El servidor no devolvió un folio.");

                var insumosPdf = detalles.Select(x => new OrdenCompraPdfService.InsumoPdf
                { Clave = x.Clave, Descripcion = x.Descripcion, Unidad = x.Unidad, Cantidad = x.Cantidad, Precio = x.PrecioUnitario }).ToList();
                string rutaPdf = OrdenCompraPdfService.GenerarMultiple(folioOC, insumosPdf, d.proveedorNombre, d.proveedorClave);

                string pdfBase64 = File.Exists(rutaPdf) ? Convert.ToBase64String(File.ReadAllBytes(rutaPdf)) : null;
                string nombreArchivo = File.Exists(rutaPdf) ? Path.GetFileName(rutaPdf) : null;

                ApiClient.Post("/api/repositorio-ordenes/multiple", new
                {
                    FolioOC = folioOC,
                    Usuario = Environment.UserName,
                    NombreProveedor = d.proveedorNombre,
                    CodigoProveedor = d.proveedorClave,
                    NombreArchivo = nombreArchivo,
                    PdfBase64 = pdfBase64,
                    Detalles = detalles,
                    Casas = (d.casas ?? new List<MsgCasaManzanaLote>())
                        .Select(c => new CasaRepoApi { Manzana = c.manzana, Lote = c.lote, Prototipo = c.prototipo }).ToList()
                });

                Push(new { tipo = "ordenGenerada", ok = true, folio = folioOC, contexto = "multi" });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "ordenGenerada", ok = false, mensaje = "Error al generar la orden: " + ex.Message, contexto = "multi" });
            }
        }

        // ------------------------------------------------------------------
        // Compra indirecta
        // ------------------------------------------------------------------

        private void CargarIndirectasWeb()
        {
            try { Push(new { tipo = "indirectas", lista = ApiClient.Get<List<InsumoIndirectoApi>>("/api/compras/indirectas") ?? new List<InsumoIndirectoApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el catálogo de compras indirectas"); }
        }

        private void CrearInsumoIndirectoWeb(MsgCrearIndirecto d)
        {
            try
            {
                ApiClient.Post("/api/compras/indirectas", new { Clave = d.clave, Descripcion = d.descripcion, Unidad = d.unidad });
                Push(new { tipo = "insumoIndirectoCreado", ok = true, mensaje = "Insumo agregado exitosamente." });
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Push(new { tipo = "insumoIndirectoCreado", ok = false, mensaje = "Ya existe un insumo con esa clave." });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo guardar el insumo"); }
        }

        private void GenerarOrdenIndirectaWeb(MsgGenerarIndirecta d)
        {
            string folioOC;
            try
            {
                var detalles = DetallesDe(d.detalles);
                var resp = ApiClient.Post<FolioOrdenApi>("/api/ordenescompra/indirecta", new
                {
                    Usuario = Environment.UserName,
                    NombreOrden = d.nombreOrden,
                    ProveedorClave = d.proveedorClave,
                    Detalles = detalles
                });
                folioOC = resp?.FolioOC;
                if (string.IsNullOrEmpty(folioOC)) throw new Exception("El servidor no devolvió un folio.");

                var insumosPdf = detalles.Select(x => new OrdenCompraPdfService.InsumoPdf
                { Clave = x.Clave, Descripcion = x.Descripcion, Unidad = x.Unidad, Cantidad = x.Cantidad, Precio = x.PrecioUnitario }).ToList();
                string rutaPdf = OrdenCompraPdfService.GenerarIndirecta(folioOC, insumosPdf, d.proveedorNombre, d.proveedorClave);

                string pdfBase64 = File.Exists(rutaPdf) ? Convert.ToBase64String(File.ReadAllBytes(rutaPdf)) : null;
                string nombreArchivo = File.Exists(rutaPdf) ? Path.GetFileName(rutaPdf) : null;
                string tipoOrden = (d.tipoTitulo ?? "").ToUpperInvariant().Contains("ADMINISTRATIVA") ? "ADMINISTRATIVA" : "INDIRECTA";

                ApiClient.Post("/api/repositorio-ordenes/indirecta", new
                {
                    FolioOC = folioOC,
                    Usuario = Environment.UserName,
                    TipoOrden = tipoOrden,
                    NombreProveedor = d.proveedorNombre,
                    CodigoProveedor = d.proveedorClave,
                    NombreArchivo = nombreArchivo,
                    PdfBase64 = pdfBase64,
                    Detalles = detalles
                });

                Push(new { tipo = "ordenGenerada", ok = true, folio = folioOC, contexto = "indirecta" });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "ordenGenerada", ok = false, mensaje = "Error al generar la orden: " + ex.Message, contexto = "indirecta" });
            }
        }

        // ------------------------------------------------------------------
        // Consulta de órdenes (repositorio)
        // ------------------------------------------------------------------

        private void CargarRepositorioWeb(bool soloIndirectas)
        {
            try
            {
                string query = "/api/repositorio-ordenes?soloIndirectas=" + (soloIndirectas ? "true" : "false");
                Push(new { tipo = "repositorio", lista = ApiClient.Get<List<OrdenCompraInfo>>(query) ?? new List<OrdenCompraInfo>() });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las órdenes"); }
        }

        private void VerPdfRepositorioWeb(int folioId, string folio)
        {
            try
            {
                byte[] pdfBytes = ApiClient.GetBytes($"/api/repositorio-ordenes/{folioId}/pdf");
                if (pdfBytes == null || pdfBytes.Length == 0) { PushError("No se encontró el PDF almacenado para esta orden."); return; }

                string nombreArchivo = $"OrdenCompra_{folio}.pdf";
                string tempPath = Path.Combine(Path.GetTempPath(), nombreArchivo);
                File.WriteAllBytes(tempPath, pdfBytes);
                try { Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true }); }
                catch { PushError($"PDF guardado en: {tempPath}"); }
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo abrir el PDF"); }
        }

        private void VerDetalleRepositorioWeb(int folioId)
        {
            try
            {
                var detalle = ApiClient.Get<DetalleOrdenRepoApi>($"/api/repositorio-ordenes/{folioId}/detalle");
                if (detalle == null) { PushError("No se encontró el detalle de la orden."); return; }
                Push(new
                {
                    tipo = "detalleOrden",
                    folio = detalle.Folio,
                    manzana = detalle.Manzana,
                    lote = detalle.Lote,
                    fechaGeneracion = detalle.FechaGeneracion,
                    tipoOrden = detalle.TipoOrden,
                    nombreProveedor = detalle.NombreProveedor,
                    codigoProveedor = detalle.CodigoProveedor,
                    totalSinIVA = detalle.TotalSinIVA,
                    iva = detalle.IVA,
                    totalConIVA = detalle.TotalConIVA,
                    numeroOrden = detalle.NumeroOrden,
                    usuario = detalle.Usuario,
                    estado = detalle.Estado,
                    casas = detalle.Casas,
                    insumos = detalle.Insumos
                });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el detalle de la orden"); }
        }

        private void EliminarRepositorioWeb(int folioId)
        {
            try
            {
                int filas = ApiClient.Post<int>($"/api/repositorio-ordenes/{folioId}/eliminar", new { });
                Push(new { tipo = "repositorioEliminado", ok = filas > 0, mensaje = filas > 0 ? "Orden eliminada exitosamente." : "No se encontró la orden." });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo eliminar la orden"); }
        }
    }
}
