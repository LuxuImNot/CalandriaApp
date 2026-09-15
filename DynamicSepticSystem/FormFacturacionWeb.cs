// Facturación IA: ventana dedicada que hospeda en WebView2 la página
// Calandria.Api/ui/facturacion.html (saldo por obra del add-on "Conciliación
// de factura con IA", historial por obra y repositorio de PDFs de resumen),
// igual que FormPerfilesWeb.cs. Sólo accesible con el permiso
// sistema.facturacion (ver PanelPrincipal), es información de facturación
// del proveedor del sistema (Pilaris), no del cliente.
//
// Toda lectura/escritura pasa por api/facturacion vía ApiClient: nada de SQL
// directo desde este puente. El PDF se genera en el cliente con PdfSharp
// (FacturacionPdfService) y se archiva en el repositorio vía API, igual que
// FormComprasWeb hace con OrdenCompraPdfService.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public class FormFacturacionWeb : Form
    {
        private WebView2 webFacturacion;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "facturacion.html");

        public FormFacturacionWeb()
        {
            Text = "Facturación IA - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new System.Drawing.Size(1180, 760);
            MinimumSize = new System.Drawing.Size(960, 600);
            WindowState = FormWindowState.Maximized;
            BackColor = System.Drawing.Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webFacturacion = new WebView2 { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.White };
            Controls.Add(webFacturacion);

            webFacturacion.CoreWebView2InitializationCompleted += WebFacturacion_Init;
            _ = IniciarWebView2Async();
        }

        /// <summary>
        /// Entorno y carpeta de datos propios: varias ventanas WebView2 del mismo
        /// proceso no deben compartir el entorno implícito (ver
        /// reference_webview2_multi_instance / FormAlmacen.Web.cs). Reintenta unas
        /// veces: la creación del entorno falla intermitentemente (E_ABORT /
        /// "Catastrophic failure") y luego funciona al reintentar, ya visto en
        /// Destajos, Compras, Panel y Perfiles.
        /// </summary>
        private async Task IniciarWebView2Async()
        {
            const int intentos = 3;
            for (int intento = 1; intento <= intentos; intento++)
            {
                try
                {
                    string userDataFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Calandria", "WebView2Facturacion");
                    var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                    await webFacturacion.EnsureCoreWebView2Async(entorno);
                    return;
                }
                catch (Exception ex)
                {
                    if (IsDisposed) return;
                    if (intento == intentos)
                    {
                        ErrorLogger.RegistrarMensaje("FacturacionWeb", "WebView2 no pudo iniciar: " + ex.Message);
                        MessageBox.Show("No se pudo iniciar el módulo de Facturación IA:\n\n" + ex.Message,
                            "Facturación IA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                    }
                    await Task.Delay(500 * intento);
                }
            }
        }

        private void WebFacturacion_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("FacturacionWeb", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar el módulo de Facturación IA.", "Facturación IA",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            var core = webFacturacion.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebFacturacion_Mensaje;
            core.NavigationCompleted += (s2, e2) =>
            {
                if (!e2.IsSuccess) return;
                EnviarSesionWeb();
                _ = CargarTemaObraAsync();
            };

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("FacturacionWeb", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webFacturacion?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz web de Facturación IA.", "Facturación IA",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            webFacturacion.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/facturacion/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/facturacion");
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
                ErrorLogger.RegistrarMensaje("FacturacionWeb", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webFacturacion?.CoreWebView2 == null) return;
            try { webFacturacion.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("FacturacionWeb", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        /// <summary>Manda la paleta de acento de la obra activa (cosmético, nunca bloquea si falla).</summary>
        private async Task CargarTemaObraAsync()
        {
            object msg = null;
            try { msg = await Task.Run(() => TemaObra.ObtenerParaPush()); }
            catch { /* cosmético */ }
            if (!IsDisposed && msg != null) Push(msg);
        }

        private void EnviarSesionWeb()
        {
            Push(new
            {
                tipo = "sesion",
                usuario = Global.UsuarioActual?.Nombre ?? "",
                perfil = Global.UsuarioActual?.Perfil ?? ""
            });
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
            ErrorLogger.RegistrarMensaje("FacturacionWeb", contexto + ": " + msg);
            Push(new { tipo = "error", mensaje = contexto + ": " + msg });
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgId { public int id; }
        private class MsgPeriodo { public string desde; public string hasta; }
        private class MsgHistorial { public int obraId; public string obraNombre; public string desde; public string hasta; }

        private void WebFacturacion_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
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
                    case "cargar-saldos":
                        { var d = JsonConvert.DeserializeObject<MsgPeriodo>(json); CargarSaldosWeb(d.desde, d.hasta); }
                        break;
                    case "cargar-historial":
                        { var d = JsonConvert.DeserializeObject<MsgHistorial>(json); CargarHistorialWeb(d); }
                        break;
                    case "generar-pdf-consolidado":
                        { var d = JsonConvert.DeserializeObject<MsgPeriodo>(json); GenerarPdfConsolidadoWeb(d.desde, d.hasta); }
                        break;
                    case "generar-pdf-obra":
                        { var d = JsonConvert.DeserializeObject<MsgHistorial>(json); GenerarPdfObraWeb(d); }
                        break;
                    case "cargar-repositorio": CargarRepositorioWeb(); break;
                    case "ver-pdf-repositorio":
                        { var d = JsonConvert.DeserializeObject<MsgId>(json); VerPdfRepositorioWeb(d.id); }
                        break;
                    case "eliminar-repositorio":
                        { var d = JsonConvert.DeserializeObject<MsgId>(json); EliminarRepositorioWeb(d.id); }
                        break;
                    case "volver":
                        Close();
                        break;
                    default:
                        ErrorLogger.RegistrarMensaje("FacturacionWeb", "Acción desconocida desde la página: " + accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                ManejarErrorApi(ex, "No se pudo completar la acción solicitada");
            }
        }

        // ------------------------------------------------------------------
        // Saldo por obra + historial
        // ------------------------------------------------------------------

        private void CargarSaldosWeb(string desde, string hasta)
        {
            try
            {
                var lista = ApiClient.Get<List<SaldoObraApi>>($"/api/facturacion/saldos?desde={desde}&hasta={hasta}") ?? new List<SaldoObraApi>();
                Push(new { tipo = "saldos", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los saldos"); }
        }

        private void CargarHistorialWeb(MsgHistorial d)
        {
            try
            {
                var lista = ApiClient.Get<List<ConciliacionIaHistorialApi>>(
                    $"/api/facturacion/{d.obraId}/historial?desde={d.desde}&hasta={d.hasta}") ?? new List<ConciliacionIaHistorialApi>();
                Push(new { tipo = "historial", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el historial"); }
        }

        // ------------------------------------------------------------------
        // Generación de PDF (FacturacionPdfService) + archivado en repositorio
        // ------------------------------------------------------------------

        private void GenerarPdfConsolidadoWeb(string desde, string hasta)
        {
            try
            {
                var lista = ApiClient.Get<List<SaldoObraApi>>($"/api/facturacion/saldos?desde={desde}&hasta={hasta}") ?? new List<SaldoObraApi>();
                var disponibles = lista.Where(o => o.TablaDisponible).ToList();
                if (disponibles.Count == 0)
                {
                    Push(new { tipo = "pdfGenerado", ok = false, mensaje = "No hay datos disponibles para generar el PDF en este periodo." });
                    return;
                }

                var filas = disponibles.Select(o => new[] { o.ObraNombre, o.Conteo.ToString(), o.MontoACobrar.ToString("C2") }).ToList();
                int conteoTotal = disponibles.Sum(o => o.Conteo);
                decimal montoTotal = disponibles.Sum(o => o.MontoACobrar);

                string nombreArchivo = $"Facturacion_{desde}_a_{hasta}.pdf";
                string rutaPdf = FacturacionPdfService.Generar(
                    nombreArchivo,
                    "Resumen de facturación · Conciliación de factura con IA",
                    $"Periodo {desde} a {hasta} · Todas las obras",
                    new[] { "Obra", "Facturas conciliadas", "Monto a cobrar" },
                    new[] { 275, 100, 140 },
                    new[] { false, true, true },
                    filas,
                    "Total a cobrar", montoTotal.ToString("C2"),
                    Global.UsuarioActual?.Nombre ?? "");

                ArchivarPdfEnRepositorio(rutaPdf, null, "Todas las obras", desde, hasta, conteoTotal, montoTotal);
                Push(new { tipo = "pdfGenerado", ok = true, mensaje = "PDF consolidado generado y archivado en el repositorio." });
            }
            catch (ApiException ex) { Push(new { tipo = "pdfGenerado", ok = false, mensaje = MensajeDe(ex) }); }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("FacturacionWeb", "GenerarPdfConsolidadoWeb: " + ex.Message);
                Push(new { tipo = "pdfGenerado", ok = false, mensaje = "No se pudo generar el PDF: " + ex.Message });
            }
        }

        private void GenerarPdfObraWeb(MsgHistorial d)
        {
            try
            {
                var lista = ApiClient.Get<List<ConciliacionIaHistorialApi>>(
                    $"/api/facturacion/{d.obraId}/historial?desde={d.desde}&hasta={d.hasta}") ?? new List<ConciliacionIaHistorialApi>();
                int conteo = lista.Count;
                decimal monto = Math.Max(conteo * 18m, 500m);

                var filas = lista.Select(f => new[] { f.Uuid, f.FolioOC, f.Fecha.ToString("dd/MM/yyyy HH:mm"), f.Usuario }).ToList();

                string nombreArchivo = $"Facturacion_{Sanear(d.obraNombre)}_{d.desde}_a_{d.hasta}.pdf";
                string rutaPdf = FacturacionPdfService.Generar(
                    nombreArchivo,
                    "Detalle de facturación · " + d.obraNombre,
                    $"Periodo {d.desde} a {d.hasta} · Conciliación de factura con IA",
                    new[] { "UUID (CFDI)", "Folio OC", "Fecha", "Usuario" },
                    new[] { 190, 110, 90, 125 },
                    null,
                    filas,
                    "Total a cobrar", monto.ToString("C2"),
                    Global.UsuarioActual?.Nombre ?? "");

                ArchivarPdfEnRepositorio(rutaPdf, d.obraId, d.obraNombre, d.desde, d.hasta, conteo, monto);
                Push(new { tipo = "pdfGenerado", ok = true, mensaje = $"PDF de {d.obraNombre} generado y archivado en el repositorio." });
            }
            catch (ApiException ex) { Push(new { tipo = "pdfGenerado", ok = false, mensaje = MensajeDe(ex) }); }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("FacturacionWeb", "GenerarPdfObraWeb: " + ex.Message);
                Push(new { tipo = "pdfGenerado", ok = false, mensaje = "No se pudo generar el PDF: " + ex.Message });
            }
        }

        private static void ArchivarPdfEnRepositorio(string rutaPdf, int? obraId, string obraNombre,
            string desde, string hasta, int conteo, decimal monto)
        {
            string pdfBase64 = Convert.ToBase64String(File.ReadAllBytes(rutaPdf));
            ApiClient.Post("/api/facturacion/repositorio", new
            {
                ObraId = obraId,
                ObraNombre = obraNombre,
                Desde = desde,
                Hasta = hasta,
                Conteo = conteo,
                MontoACobrar = monto,
                NombreArchivo = Path.GetFileName(rutaPdf),
                PdfBase64 = pdfBase64
            });
        }

        private static string Sanear(string s) =>
            string.IsNullOrWhiteSpace(s) ? "Obra" : new string(s.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());

        // ------------------------------------------------------------------
        // Repositorio de PDFs
        // ------------------------------------------------------------------

        private void CargarRepositorioWeb()
        {
            try { Push(new { tipo = "repositorio", lista = ApiClient.Get<List<PdfFacturacionApi>>("/api/facturacion/repositorio") ?? new List<PdfFacturacionApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el repositorio"); }
        }

        private void VerPdfRepositorioWeb(int id)
        {
            try
            {
                byte[] pdfBytes = ApiClient.GetBytes($"/api/facturacion/repositorio/{id}/pdf");
                if (pdfBytes == null || pdfBytes.Length == 0) { Push(new { tipo = "error", mensaje = "No se encontró el PDF archivado." }); return; }

                string tempPath = Path.Combine(Path.GetTempPath(), $"FacturacionIA_{id}.pdf");
                File.WriteAllBytes(tempPath, pdfBytes);
                try { Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true }); }
                catch { Push(new { tipo = "error", mensaje = $"PDF guardado en: {tempPath}" }); }
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo abrir el PDF"); }
        }

        private void EliminarRepositorioWeb(int id)
        {
            try
            {
                int filas = ApiClient.Post<int>($"/api/facturacion/repositorio/{id}/eliminar", new { });
                Push(new { tipo = "repositorioEliminado", ok = filas > 0, mensaje = filas > 0 ? "PDF eliminado." : "No se encontró el PDF." });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "repositorioEliminado", ok = false, mensaje = "No se pudo eliminar: " + ex.Message });
            }
        }
    }
}
