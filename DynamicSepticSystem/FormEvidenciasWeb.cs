// Evidencias híbrido: ventana dedicada que hospeda en WebView2 la página
// Calandria.Api/ui/evidencias.html (galería de evidencias fotográficas por
// casa: agregar, ver a pantalla completa, borrar y exportar PDF), mismo
// patrón que FormTrabajadoresWeb.cs / FormComprasWeb.cs. Reemplaza a
// FormEvidenciasFotograficas cuando EvidenciasWeb=true (ver
// PanelPrincipal.AbrirFormEvidencias); con EvidenciasWeb=false el formulario
// clásico sigue intacto.
//
// Toda lectura/escritura pasa por api/evidencias (y api/avances para el
// catálogo de manzanas/lotes) vía ApiClient: nada de SQL directo desde este
// puente. El PDF se sigue generando en el cliente con PdfSharp (portado de
// FormEvidenciasFotograficas.GenerarPDFEvidencias) pero alimentado por lo que
// trae el API, no por SQL directo.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    public class FormEvidenciasWeb : Form
    {
        private WebView2 webEvidencias;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "evidencias.html");

        public FormEvidenciasWeb()
        {
            Text = "Evidencias Fotográficas - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new System.Drawing.Size(1280, 800);
            MinimumSize = new System.Drawing.Size(1024, 640);
            WindowState = FormWindowState.Maximized;
            BackColor = System.Drawing.Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webEvidencias = new WebView2 { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.White };
            Controls.Add(webEvidencias);

            webEvidencias.CoreWebView2InitializationCompleted += WebEvidencias_Init;
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
                    "Calandria", "WebView2Evidencias");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webEvidencias.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("EvidenciasWeb", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo iniciar el módulo web de Evidencias:\n\n" + ex.Message,
                    "Evidencias", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
            }
        }

        private void WebEvidencias_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("EvidenciasWeb", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar el módulo web de Evidencias.", "Evidencias",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            var core = webEvidencias.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebEvidencias_Mensaje;
            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("EvidenciasWeb", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webEvidencias?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz web de Evidencias.", "Evidencias",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            webEvidencias.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/evidencias/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/evidencias");
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
                ErrorLogger.RegistrarMensaje("EvidenciasWeb", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webEvidencias?.CoreWebView2 == null) return;
            try { webEvidencias.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("EvidenciasWeb", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        private void PushError(string mensaje)
        {
            if (webEvidencias?.CoreWebView2 == null) return;
            try { webEvidencias.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { tipo = "error", mensaje }, CamelCaseSettings)); }
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
            ErrorLogger.RegistrarMensaje("EvidenciasWeb", contexto + ": " + msg);
            PushError(contexto + ": " + msg);
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgManzana { public string manzana; }
        private class MsgCasa { public string manzana, lote; }
        private class MsgCargarFoto { public int id; }
        private class MsgGuardarEvidencia { public string manzana, lote, titulo, fotoBase64, extension; }
        private class MsgEliminarEvidencia { public int id; public string manzana, lote; }

        private void WebEvidencias_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
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
                    case "cargar-manzanas": CargarManzanasWeb(); break;
                    case "cargar-lotes":
                        { var d = JsonConvert.DeserializeObject<MsgManzana>(json); CargarLotesWeb(d.manzana); }
                        break;
                    case "cargar-evidencias":
                        { var d = JsonConvert.DeserializeObject<MsgCasa>(json); CargarEvidenciasWeb(d.manzana, d.lote); }
                        break;
                    case "cargar-foto":
                        { var d = JsonConvert.DeserializeObject<MsgCargarFoto>(json); CargarFotoWeb(d.id); }
                        break;
                    case "guardar-evidencia":
                        { var d = JsonConvert.DeserializeObject<MsgGuardarEvidencia>(json); GuardarEvidenciaWeb(d); }
                        break;
                    case "eliminar-evidencia":
                        { var d = JsonConvert.DeserializeObject<MsgEliminarEvidencia>(json); EliminarEvidenciaWeb(d.id, d.manzana, d.lote); }
                        break;
                    case "exportar-pdf":
                        { var d = JsonConvert.DeserializeObject<MsgCasa>(json); ExportarPdfWeb(d.manzana, d.lote); }
                        break;
                    case "volver":
                        Close();
                        break;
                    default:
                        ErrorLogger.RegistrarMensaje("EvidenciasWeb", "Acción desconocida desde la página: " + accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                ManejarErrorApi(ex, "No se pudo completar la acción solicitada");
            }
        }

        // ------------------------------------------------------------------
        // Lecturas / escritura — todas vía api/evidencias y api/avances
        // ------------------------------------------------------------------

        private void CargarManzanasWeb()
        {
            try { Push(new { tipo = "manzanas", lista = ApiClient.Get<List<string>>("/api/avances/manzanas") ?? new List<string>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las manzanas"); }
        }

        private void CargarLotesWeb(string manzana)
        {
            if (string.IsNullOrWhiteSpace(manzana)) return;
            try { Push(new { tipo = "lotes", lista = ApiClient.Get<List<string>>("/api/avances/lotes?manzana=" + Uri.EscapeDataString(manzana)) ?? new List<string>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los lotes"); }
        }

        private void CargarEvidenciasWeb(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote)) return;
            try
            {
                var lista = ApiClient.Get<List<EvidenciaApi>>(
                    $"/api/evidencias?manzana={Uri.EscapeDataString(manzana)}&lote={Uri.EscapeDataString(lote)}") ?? new List<EvidenciaApi>();
                Push(new { tipo = "evidencias", manzana, lote, lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las evidencias"); }
        }

        private void CargarFotoWeb(int id)
        {
            try
            {
                byte[] bytes = ApiClient.GetBytes($"/api/evidencias/{id}/foto");
                if (bytes == null || bytes.Length == 0) { PushError("No se pudo cargar la foto."); return; }
                Push(new { tipo = "foto", id, base64 = Convert.ToBase64String(bytes) });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar la foto"); }
        }

        private void GuardarEvidenciaWeb(MsgGuardarEvidencia d)
        {
            try
            {
                bool ok = ApiClient.Post<bool>("/api/evidencias", new
                {
                    Manzana = d.manzana,
                    Lote = d.lote,
                    Titulo = d.titulo,
                    FotoBase64 = d.fotoBase64,
                    Extension = d.extension ?? "jpg",
                    Usuario = Environment.UserName
                });
                Push(new { tipo = "evidenciaGuardada", ok, mensaje = ok ? null : "No se pudo guardar la evidencia." });
                if (ok) CargarEvidenciasWeb(d.manzana, d.lote);
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "evidenciaGuardada", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "evidenciaGuardada", ok = false, mensaje = "No se pudo guardar: " + ex.Message });
            }
        }

        private void EliminarEvidenciaWeb(int id, string manzana, string lote)
        {
            try
            {
                bool ok = ApiClient.Post<bool>($"/api/evidencias/{id}/eliminar", new { });
                Push(new { tipo = "evidenciaEliminada", ok, mensaje = ok ? "Evidencia eliminada." : "No se encontró la evidencia." });
                if (ok) CargarEvidenciasWeb(manzana, lote);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo eliminar la evidencia"); }
        }

        // ------------------------------------------------------------------
        // Exportar PDF — portado de FormEvidenciasFotograficas.GenerarPDFEvidencias,
        // alimentado por el API en vez de SQL directo.
        // ------------------------------------------------------------------

        private void ExportarPdfWeb(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote)) return;
            try
            {
                var lista = ApiClient.Get<List<EvidenciaApi>>(
                    $"/api/evidencias?manzana={Uri.EscapeDataString(manzana)}&lote={Uri.EscapeDataString(lote)}") ?? new List<EvidenciaApi>();
                if (lista.Count == 0)
                {
                    Push(new { tipo = "pdfExportado", ok = false, mensaje = "No hay evidencias para exportar." });
                    return;
                }

                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CALANDRIA RESIDENCIAL", "Evidencias");
                Directory.CreateDirectory(carpeta);

                using (var sfd = new SaveFileDialog
                {
                    Filter = "Archivo PDF (*.pdf)|*.pdf",
                    FileName = $"Evidencias_M{manzana}_L{lote}_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                    InitialDirectory = carpeta
                })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;

                    GenerarPdfEvidencias(sfd.FileName, manzana, lote, lista);
                    Push(new { tipo = "pdfExportado", ok = true, mensaje = "PDF generado: " + Path.GetFileName(sfd.FileName) });

                    var r = MessageBox.Show("¿Desea abrir el PDF?", "Abrir PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r == DialogResult.Yes)
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("EvidenciasWeb", "No se pudo exportar el PDF: " + ex.Message);
                Push(new { tipo = "pdfExportado", ok = false, mensaje = "No se pudo generar el PDF: " + ex.Message });
            }
        }

        private static void GenerarPdfEvidencias(string rutaPdf, string manzana, string lote, List<EvidenciaApi> evidencias)
        {
            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = $"Evidencias Fotográficas - M{manzana} L{lote}";
            pdf.Info.Author = "Sistema Calandria Residencial";
            pdf.Info.Subject = "Reporte de Evidencias Fotográficas";

            XColor colorPrimario = XColor.FromArgb(ThemeManager.ColorPrincipal.R,
                ThemeManager.ColorPrincipal.G, ThemeManager.ColorPrincipal.B);

            XFont fontSubtitulo = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 10);
            XFont fontPequena = new XFont("Arial", 8);

            PdfPage portada = pdf.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(portada);

            gfx.DrawRectangle(new XSolidBrush(colorPrimario), 0, 0, portada.Width, 100);
            gfx.DrawString("EVIDENCIAS FOTOGRÁFICAS", new XFont("Arial", 20, XFontStyle.Bold),
                XBrushes.White, new XRect(0, 30, portada.Width, 40), XStringFormats.TopCenter);

            double y = 130;
            gfx.DrawString($"Manzana: {manzana}", fontSubtitulo, XBrushes.Black, 50, y);
            y += 30;
            gfx.DrawString($"Lote: {lote}", fontSubtitulo, XBrushes.Black, 50, y);
            y += 30;
            gfx.DrawString($"Fecha de reporte: {DateTime.Now:dd/MM/yyyy HH:mm}", fontNormal, XBrushes.Black, 50, y);
            y += 25;
            gfx.DrawString($"Total de evidencias: {evidencias.Count}", fontNormal, XBrushes.Black, 50, y);

            int contador = 0;
            foreach (var evidencia in evidencias)
            {
                if (contador % 2 == 0)
                {
                    portada = pdf.AddPage();
                    gfx = XGraphics.FromPdfPage(portada);
                    y = 50;
                }

                byte[] fotoBytes = ApiClient.GetBytes($"/api/evidencias/{evidencia.Id}/foto");
                if (fotoBytes != null && fotoBytes.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(fotoBytes))
                    {
                        XImage imagen = XImage.FromStream(ms);

                        gfx.DrawString(evidencia.Titulo, fontSubtitulo, new XSolidBrush(colorPrimario), 50, y);
                        y += 20;

                        gfx.DrawString($"Fecha: {evidencia.Fecha:dd/MM/yyyy HH:mm} | Usuario: {evidencia.Usuario}",
                            fontPequena, XBrushes.Gray, 50, y);
                        y += 20;

                        double anchoMax = portada.Width - 100;
                        double altoMax = 300;
                        double ratio = Math.Min(anchoMax / imagen.PixelWidth, altoMax / imagen.PixelHeight);
                        double anchoFinal = imagen.PixelWidth * ratio;
                        double altoFinal = imagen.PixelHeight * ratio;

                        gfx.DrawImage(imagen, 50, y, anchoFinal, altoFinal);
                        y += altoFinal + 30;
                    }
                }

                contador++;
            }

            pdf.Save(rutaPdf);
        }
    }
}
