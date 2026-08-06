// Distribución de Nómina: ventana dedicada que hospeda en WebView2 la página
// Calandria.Api/ui/distribucion-nomina.html, reemplazando al diálogo nativo
// FormDistribucionNomina para el flujo que abre destajos.html (reporte por
// cuadrilla → "Distribuir"). FormDistribucionNomina.cs sigue intacta: la usa
// FormDestajosPorCuadrilla.cs (menú clásico), que no se migra en esta pasada.
//
// La generación de PDF (PdfSharp) se queda igual que en el diálogo nativo —
// sólo cambia quién captura monto/concepto por trabajador.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    public class FormDistribucionNominaWeb : Form
    {
        private readonly string _codigoCuadrilla;
        private readonly DateTime _desde;
        private readonly DateTime _hasta;
        private readonly List<FormDistribucionNomina.DestajoTerminado> _destajos;
        private readonly decimal _totalCuadrilla;

        private WebView2 webNomina;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "distribucion-nomina.html");

        public FormDistribucionNominaWeb(
            string codigoCuadrilla, DateTime desde, DateTime hasta,
            List<FormDistribucionNomina.DestajoTerminado> destajos)
        {
            _codigoCuadrilla = codigoCuadrilla ?? "";
            _desde = desde.Date;
            _hasta = hasta.Date;
            _destajos = destajos ?? new List<FormDistribucionNomina.DestajoTerminado>();
            _totalCuadrilla = _destajos.Sum(d => d.Importe);

            Text = "Distribución de Nómina - " + _codigoCuadrilla;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(960, 620);
            ClientSize = new Size(1060, 720);
            BackColor = Color.White;

            webNomina = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webNomina);

            webNomina.CoreWebView2InitializationCompleted += WebNomina_Init;
            _ = IniciarWebView2Async();
        }

        /// <summary>Entorno y carpeta de datos propios, igual que los demás hosts (ver reference_webview2_multi_instance).</summary>
        private async Task IniciarWebView2Async()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2DistribucionNomina");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webNomina.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("DistribucionNominaWeb", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo iniciar la ventana de distribución de nómina:\n\n" + ex.Message,
                    "Distribución de Nómina", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
            }
        }

        private void WebNomina_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("DistribucionNominaWeb", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar la ventana de distribución de nómina.", "Distribución de Nómina",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            var core = webNomina.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebNomina_Mensaje;
            core.NavigationCompleted += (s2, e2) => { if (e2.IsSuccess) EnviarDatosIniciales(); };

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("DistribucionNominaWeb", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webNomina?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz de distribución de nómina.", "Distribución de Nómina",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            webNomina.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/distribucion-nomina/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/distribucion-nomina");
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
                ErrorLogger.RegistrarMensaje("DistribucionNominaWeb", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webNomina?.CoreWebView2 == null) return;
            try { webNomina.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("DistribucionNominaWeb", "Fallo al enviar datos a la página: " + ex.Message); }
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

        private void EnviarDatosIniciales()
        {
            List<MiembroCuadrillaApi> miembros;
            try
            {
                miembros = ApiClient.Get<List<MiembroCuadrillaApi>>(
                    "/api/nomina/cuadrillas/" + Uri.EscapeDataString(_codigoCuadrilla) + "/miembros")
                    ?? new List<MiembroCuadrillaApi>();
            }
            catch (Exception ex)
            {
                Push(new { tipo = "error", mensaje = "No se pudieron cargar los miembros de la cuadrilla: " + ex.Message });
                miembros = new List<MiembroCuadrillaApi>();
            }

            Push(new
            {
                tipo = "datos",
                cuadrilla = _codigoCuadrilla,
                desde = _desde.ToString("yyyy-MM-dd"),
                hasta = _hasta.ToString("yyyy-MM-dd"),
                totalCuadrilla = _totalCuadrilla,
                puedeEditar = Global.UsuarioActual?.TienePermiso("nomina.editar") ?? false,
                destajosCount = _destajos.Count,
                conceptoAuto = ConstruirConceptoDesdeDestajos(),
                miembros = miembros.Select(m => new
                {
                    idTrabajador = m.IdTrabajador,
                    nombre = m.Nombre,
                    rol = m.Rol,
                    esJefe = m.EsJefe
                })
            });
        }

        private string ConstruirConceptoDesdeDestajos()
        {
            if (_destajos.Count == 0) return "";
            var nombres = _destajos
                .Select(d => (d.Nombre ?? "").Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToList();
            return string.Join(", ", nombres);
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgMiembro { public int? idTrabajador; public string nombre, rol; public bool esJefe; public decimal monto; public string concepto; }
        private class MsgGenerar { public List<MsgMiembro> miembros; }

        private void WebNomina_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;
            MsgAccion m;
            try { m = JsonConvert.DeserializeObject<MsgAccion>(json); }
            catch { return; }
            if (m?.accion == null) return;

            BeginInvoke((Action)(() =>
            {
                switch (m.accion)
                {
                    case "generar":
                        { var d = JsonConvert.DeserializeObject<MsgGenerar>(json); GenerarWeb(d?.miembros ?? new List<MsgMiembro>()); }
                        break;
                    case "cerrar":
                        Close();
                        break;
                    default:
                        ErrorLogger.RegistrarMensaje("DistribucionNominaWeb", "Acción desconocida desde la página: " + m.accion);
                        break;
                }
            }));
        }

        private class MsgAccion { public string accion; }

        private void GenerarWeb(List<MsgMiembro> miembros)
        {
            if (miembros.Count == 0)
            {
                Push(new { tipo = "generarResultado", ok = false, mensaje = "La cuadrilla no tiene miembros para generar recibos." });
                return;
            }
            var sinConcepto = miembros.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.concepto));
            if (sinConcepto != null)
            {
                Push(new { tipo = "generarResultado", ok = false, mensaje = "Falta el concepto del recibo para " + sinConcepto.nombre + "." });
                return;
            }

            try
            {
                string nombreArchivo = string.Format(
                    "DistribucionNomina_{0}_{1:yyyyMMdd_HHmmss}.pdf",
                    SanitizarNombre(_codigoCuadrilla), DateTime.Now);
                string archivoTemp = Path.Combine(Path.GetTempPath(), nombreArchivo);

                CrearPdfDistribucion(archivoTemp, miembros);

                var request = new
                {
                    CodigoCuadrilla = _codigoCuadrilla,
                    Desde = _desde,
                    Hasta = _hasta,
                    TotalCuadrilla = _totalCuadrilla,
                    Recibos = miembros.Select(m => new
                    {
                        IdTrabajador = m.idTrabajador,
                        NombreTrabajador = m.nombre,
                        Rol = m.rol,
                        Concepto = m.concepto,
                        Monto = m.monto,
                        PdfBase64 = Convert.ToBase64String(CrearPdfReciboIndividual(m))
                    }).ToList(),
                    Destajos = _destajos
                        .Where(d => d.NodoID > 0)
                        .Select(d => new { d.Manzana, d.Lote, d.Ruta, NodoId = d.NodoID })
                        .ToList()
                };
                ApiClient.Post("/api/nomina/distribucion", request);

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    nombreArchivo);

                Push(new { tipo = "generarResultado", ok = true });

                using (var preview = new FormPdfPreview(archivoTemp, sugerido))
                    preview.ShowDialog(this);

                Close();
            }
            catch (ApiException ex)
            {
                Push(new { tipo = "generarResultado", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "generarResultado", ok = false, mensaje = "Error generando el PDF de distribución: " + ex.Message });
            }
        }

        private static string SanitizarNombre(string s)
        {
            if (string.IsNullOrEmpty(s)) return "Cuadrilla";
            var sb = new StringBuilder();
            foreach (var c in s)
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            return sb.ToString();
        }

        // ------------------------------------------------------------------
        // Generación de PDF — idéntica a FormDistribucionNomina.cs, adaptada a
        // MsgMiembro (lo que captura la página) en vez del BindingList nativo.
        // ------------------------------------------------------------------

        private void CrearPdfDistribucion(string archivo, List<MsgMiembro> miembros)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Distribución de Nómina - " + _codigoCuadrilla;
            doc.Info.Author = "Calandria Residencial";

            DibujarPaginaListadoDestajos(doc, miembros);

            foreach (var m in miembros)
                DibujarPaginaRecibo(doc, m);

            doc.Save(archivo);
        }

        private byte[] CrearPdfReciboIndividual(MsgMiembro m)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Recibo de Nómina - " + m.nombre;
            doc.Info.Author = "Calandria Residencial";
            DibujarPaginaRecibo(doc, m);

            using (var ms = new MemoryStream())
            {
                doc.Save(ms, false);
                return ms.ToArray();
            }
        }

        private void DibujarPaginaListadoDestajos(PdfDocument doc, List<MsgMiembro> miembros)
        {
            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            var gfx = XGraphics.FromPdfPage(page);

            var fuenteTitulo = new XFont("Arial", 15, XFontStyle.Bold);
            var fuenteSubtitulo = new XFont("Arial", 10, XFontStyle.Bold);
            var fuenteSeccion = new XFont("Arial", 10, XFontStyle.Bold);
            var fuenteEnc = new XFont("Arial", 9, XFontStyle.Bold);
            var fuenteCampo = new XFont("Arial", 9, XFontStyle.Bold);
            var fuenteNorm = new XFont("Arial", 9, XFontStyle.Regular);
            var fuentePeq = new XFont("Arial", 8, XFontStyle.Regular);

            var brochaAzul = new XSolidBrush(XColor.FromArgb(13, 71, 161));
            var brochaCelta = new XSolidBrush(XColor.FromArgb(33, 150, 243));
            var brochaTexto = new XSolidBrush(XColor.FromArgb(33, 33, 33));
            var brochaGris = new XSolidBrush(XColor.FromArgb(245, 245, 245));
            var brochaVerde = new XSolidBrush(XColor.FromArgb(39, 174, 96));
            var brochaBlanca = XBrushes.White;
            var penBorde = new XPen(XColor.FromArgb(189, 189, 189), 0.5);
            var penCampo = new XPen(XColor.FromArgb(120, 120, 120), 0.8);

            var cul = CultureInfo.GetCultureInfo("es-MX");

            double margen = 30;
            double ancho = page.Width - 2 * margen;
            double y = margen;

            gfx.DrawRectangle(brochaAzul, margen, y, ancho, 50);
            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(
                        ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var logo = XImage.FromStream(ms);
                    gfx.DrawImage(logo, margen + 6, y + 6, 38, 38);
                }
            }
            catch { }
            gfx.DrawString("CAMANEY DESARROLLO Y CONSTRUCCIÓN SA DE CV",
                fuenteSubtitulo, brochaBlanca,
                new XRect(margen + 50, y + 8, ancho - 56, 14), XStringFormats.TopLeft);
            gfx.DrawString("REPORTE DE DISTRIBUCIÓN DE DESTAJOS", fuenteTitulo, brochaBlanca,
                new XRect(margen + 50, y + 24, ancho - 56, 18), XStringFormats.TopLeft);
            y += 60;

            Action<string, double, double, double, string> campo = (etiqueta, x, yy, xFin, valor) =>
            {
                gfx.DrawString(etiqueta, fuenteCampo, brochaTexto,
                    new XRect(x, yy, 150, 12), XStringFormats.TopLeft);
                double xLinea = x + gfx.MeasureString(etiqueta + " ", fuenteCampo).Width + 2;
                if (!string.IsNullOrEmpty(valor))
                    gfx.DrawString(valor, fuenteNorm, brochaTexto,
                        new XRect(xLinea, yy, xFin - xLinea, 12), XStringFormats.TopLeft);
                gfx.DrawLine(penCampo, xLinea, yy + 12, xFin, yy + 12);
            };

            double bloqueAlto = 96;
            gfx.DrawRectangle(brochaGris, margen, y, ancho, bloqueAlto);
            gfx.DrawRectangle(penBorde, margen, y, ancho, bloqueAlto);

            double colIzq = margen + 10;
            double finIzq = margen + ancho * 0.55 - 10;
            double colDer = margen + ancho * 0.55 + 6;
            double finDer = margen + ancho - 10;

            double yf = y + 8;
            campo("Edificación:", colIzq, yf, finIzq, "");
            campo("Cuadrilla:", colDer, yf, finDer, _codigoCuadrilla);
            yf += 18;
            campo("Urbanización:", colIzq, yf, finIzq, "");
            campo("Nombre de supervisor:", colDer, yf, finDer, "");
            yf += 18;
            campo("No. Supervisor:", colIzq, yf, finIzq, "");
            campo("Periodo:", colDer, yf, finDer,
                string.Format("{0:dd/MM/yyyy} al {1:dd/MM/yyyy}", _desde, _hasta));
            yf += 18;
            campo("Fecha:", colIzq, yf, finIzq, DateTime.Today.ToString("dd/MM/yyyy", cul));
            yf += 18;
            campo("Jefe de Cuadrilla:", colIzq, yf, finIzq, "");
            y += bloqueAlto + 10;

            gfx.DrawRectangle(brochaCelta, margen, y, ancho, 16);
            gfx.DrawString("DESTAJOS FINALIZADOS", fuenteSeccion, brochaBlanca,
                new XRect(margen + 4, y + 2, ancho - 8, 13), XStringFormats.TopLeft);
            y += 16;

            double colNum = ancho * 0.07;
            double colDest = ancho * 0.45;
            double colCasa = ancho * 0.13;
            double colPrecio = ancho * 0.15;
            double colObs = ancho - colNum - colDest - colCasa - colPrecio;

            Action dibujarEncabezadoDestajos = () =>
            {
                gfx.DrawRectangle(brochaAzul, margen, y, ancho, 14);
                double xc = margen;
                gfx.DrawString("Nº destajo", fuenteEnc, brochaBlanca,
                    new XRect(xc + 4, y + 2, colNum - 4, 10), XStringFormats.TopLeft);
                xc += colNum;
                gfx.DrawString("Destajo", fuenteEnc, brochaBlanca,
                    new XRect(xc + 4, y + 2, colDest - 4, 10), XStringFormats.TopLeft);
                xc += colDest;
                gfx.DrawString("Casa", fuenteEnc, brochaBlanca,
                    new XRect(xc + 4, y + 2, colCasa - 4, 10), XStringFormats.TopLeft);
                xc += colCasa;
                gfx.DrawString("Precio", fuenteEnc, brochaBlanca,
                    new XRect(xc + 4, y + 2, colPrecio - 8, 10), XStringFormats.TopRight);
                xc += colPrecio;
                gfx.DrawString("Observaciones", fuenteEnc, brochaBlanca,
                    new XRect(xc + 4, y + 2, colObs - 4, 10), XStringFormats.TopLeft);
                y += 14;
            };
            dibujarEncabezadoDestajos();

            bool alt = false;
            decimal total = 0m;
            int numero = 1;
            foreach (var d in _destajos.OrderBy(x => x.Categoria).ThenBy(x => x.Casa))
            {
                if (y > page.Height - 110)
                {
                    page = doc.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margen;
                    dibujarEncabezadoDestajos();
                }

                var brochaFila = alt ? brochaGris : new XSolidBrush(XColor.FromArgb(255, 255, 255));
                gfx.DrawRectangle(brochaFila, margen, y, ancho, 14);
                gfx.DrawRectangle(penBorde, margen, y, ancho, 14);

                double xc = margen;
                gfx.DrawString(numero.ToString(), fuenteNorm, brochaTexto,
                    new XRect(xc + 4, y + 2, colNum - 4, 10), XStringFormats.TopLeft);
                xc += colNum;
                gfx.DrawString(Truncar(d.Nombre ?? "-", 58), fuenteNorm, brochaTexto,
                    new XRect(xc + 4, y + 2, colDest - 4, 10), XStringFormats.TopLeft);
                xc += colDest;
                gfx.DrawString(Truncar(d.Casa ?? "-", 16), fuenteNorm, brochaTexto,
                    new XRect(xc + 4, y + 2, colCasa - 4, 10), XStringFormats.TopLeft);
                xc += colCasa;
                gfx.DrawString(d.Importe.ToString("C2", cul), fuenteNorm, brochaTexto,
                    new XRect(xc + 4, y + 2, colPrecio - 8, 10), XStringFormats.TopRight);

                y += 14;
                total += d.Importe;
                alt = !alt;
                numero++;
            }

            gfx.DrawRectangle(brochaVerde, margen, y, ancho, 16);
            gfx.DrawString("TOTAL DESTAJOS", fuenteEnc, brochaBlanca,
                new XRect(margen + 4, y + 3, ancho - colPrecio - colObs - 8, 12), XStringFormats.TopLeft);
            gfx.DrawString(total.ToString("C2", cul), fuenteEnc, brochaBlanca,
                new XRect(margen + colNum + colDest + colCasa + 4, y + 3, colPrecio - 8, 12),
                XStringFormats.TopRight);
            y += 24;

            if (y > page.Height - 160)
            {
                page = doc.AddPage();
                page.Size = PdfSharp.PageSize.Letter;
                gfx = XGraphics.FromPdfPage(page);
                y = margen;
            }

            gfx.DrawRectangle(brochaCelta, margen, y, ancho, 16);
            gfx.DrawString("INTEGRANTES DE CUADRILLA  ·  MONTO ASIGNADO", fuenteSeccion, brochaBlanca,
                new XRect(margen + 4, y + 2, ancho - 8, 13), XStringFormats.TopLeft);
            y += 16;

            double iTrab = ancho * 0.50;
            double iRol = ancho * 0.20;
            double iMonto = ancho * 0.15;
            double iObs = ancho - iTrab - iRol - iMonto;

            gfx.DrawRectangle(brochaAzul, margen, y, ancho, 14);
            double xi = margen;
            gfx.DrawString("Integrante de cuadrilla", fuenteEnc, brochaBlanca,
                new XRect(xi + 4, y + 2, iTrab - 4, 10), XStringFormats.TopLeft);
            xi += iTrab;
            gfx.DrawString("Rol", fuenteEnc, brochaBlanca,
                new XRect(xi + 4, y + 2, iRol - 4, 10), XStringFormats.TopLeft);
            xi += iRol;
            gfx.DrawString("Monto asignado", fuenteEnc, brochaBlanca,
                new XRect(xi + 4, y + 2, iMonto - 8, 10), XStringFormats.TopRight);
            xi += iMonto;
            gfx.DrawString("Observaciones", fuenteEnc, brochaBlanca,
                new XRect(xi + 4, y + 2, iObs - 4, 10), XStringFormats.TopLeft);
            y += 14;

            alt = false;
            decimal asignado = 0m;
            foreach (var m in miembros)
            {
                if (y > page.Height - 110)
                {
                    page = doc.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margen;
                }
                var brochaFila = m.esJefe
                    ? new XSolidBrush(XColor.FromArgb(255, 243, 224))
                    : (alt ? brochaGris : new XSolidBrush(XColor.FromArgb(255, 255, 255)));
                gfx.DrawRectangle(brochaFila, margen, y, ancho, 14);
                gfx.DrawRectangle(penBorde, margen, y, ancho, 14);

                xi = margen;
                string prefijo = m.esJefe ? "[JEFE] " : "";
                gfx.DrawString(Truncar(prefijo + (m.nombre ?? "-"), 64), fuenteNorm, brochaTexto,
                    new XRect(xi + 4, y + 2, iTrab - 4, 10), XStringFormats.TopLeft);
                xi += iTrab;
                gfx.DrawString(Truncar(m.rol ?? "-", 26), fuenteNorm, brochaTexto,
                    new XRect(xi + 4, y + 2, iRol - 4, 10), XStringFormats.TopLeft);
                xi += iRol;
                gfx.DrawString(m.monto.ToString("C2", cul), fuenteNorm, brochaTexto,
                    new XRect(xi + 4, y + 2, iMonto - 8, 10), XStringFormats.TopRight);

                y += 14;
                asignado += m.monto;
                alt = !alt;
            }

            gfx.DrawRectangle(brochaAzul, margen, y, ancho, 16);
            gfx.DrawString("TOTAL ASIGNADO", fuenteEnc, brochaBlanca,
                new XRect(margen + 4, y + 3, iTrab + iRol - 8, 12), XStringFormats.TopLeft);
            gfx.DrawString(asignado.ToString("C2", cul), fuenteEnc, brochaBlanca,
                new XRect(margen + iTrab + iRol + 4, y + 3, iMonto - 8, 12), XStringFormats.TopRight);
            y += 24;

            double altoFirmas = 64;
            if (y > page.Height - margen - altoFirmas - 20)
            {
                page = doc.AddPage();
                page.Size = PdfSharp.PageSize.Letter;
                gfx = XGraphics.FromPdfPage(page);
                y = margen;
            }

            double yFirma = page.Height - margen - altoFirmas;
            double anchoFirma = ancho / 3;
            string[] etiquetasFirma =
            {
                "Nombre y Firma Residente",
                "Nombre y firma Calidad",
                "Nombre y firma Gerente de Proyecto"
            };
            for (int i = 0; i < etiquetasFirma.Length; i++)
            {
                double xc = margen + i * anchoFirma;
                gfx.DrawLine(penCampo, xc + 14, yFirma, xc + anchoFirma - 14, yFirma);
                gfx.DrawString(etiquetasFirma[i], fuentePeq, brochaTexto,
                    new XRect(xc, yFirma + 3, anchoFirma, 12), XStringFormats.TopCenter);
            }

            double yPie = page.Height - margen - 10;
            gfx.DrawLine(new XPen(XColor.FromArgb(13, 71, 161), 1.5),
                margen, yPie, margen + ancho, yPie);
            string usuario = Global.UsuarioActual?.Nombre ?? Environment.UserName;
            gfx.DrawString(string.Format("Generado por: {0} · {1:dd/MM/yyyy HH:mm}",
                usuario, DateTime.Now),
                fuentePeq, brochaTexto,
                new XRect(margen, yPie + 2, ancho / 2, 10), XStringFormats.TopLeft);
            gfx.DrawString("© Calandria Residencial",
                fuentePeq, brochaTexto,
                new XRect(margen + ancho / 2, yPie + 2, ancho / 2, 10), XStringFormats.TopRight);
        }

        private void DibujarPaginaRecibo(PdfDocument doc, MsgMiembro m)
        {
            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            var gfx = XGraphics.FromPdfPage(page);

            var fuenteTitulo = new XFont("Times New Roman", 16, XFontStyle.Bold);
            var fuenteCuerpo = new XFont("Times New Roman", 12, XFontStyle.Regular);
            var fuenteCuerpoNeg = new XFont("Times New Roman", 12, XFontStyle.Bold);
            var fuentePie = new XFont("Times New Roman", 10, XFontStyle.Regular);

            var brochaTexto = new XSolidBrush(XColor.FromArgb(20, 20, 20));
            var penFirma = new XPen(XColor.FromArgb(80, 80, 80), 0.8);

            double margen = 60;
            double ancho = page.Width - 2 * margen;
            double y = margen;

            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(
                        ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var logo = XImage.FromStream(ms);
                    gfx.DrawImage(logo, margen + ancho - 60, y, 60, 60);
                }
            }
            catch { }

            string fechaTxt = string.Format(CultureInfo.GetCultureInfo("es-MX"),
                "Hermosillo, Sonora. {0:dd} de {0:MMMM} de {0:yyyy}", DateTime.Today);
            gfx.DrawString(fechaTxt, fuenteCuerpo, brochaTexto,
                new XRect(margen, y, ancho - 70, 18), XStringFormats.TopLeft);
            y += 36;

            gfx.DrawString("Recibo de Nómina", fuenteTitulo, brochaTexto,
                new XRect(margen, y, ancho, 22), XStringFormats.TopCenter);
            y += 48;

            string monto = m.monto.ToString("N2", CultureInfo.GetCultureInfo("es-MX"));
            string letras = NumeroALetras.Convertir(m.monto, "PESOS", "M.N.");
            string concepto = (m.concepto ?? "").Trim();

            string cuerpo = string.Format(
                "Yo, {0}, declaro recibir la suma en efectivo de ${1} ({2}) por servicios prestados " +
                "por concepto de {3}, de la empresa CAMANEY DESARROLLO Y CONSTRUCCIÓN SA DE CV.",
                (m.nombre ?? "").Trim(), monto, letras, concepto);

            y = DibujarParrafo(gfx, cuerpo, fuenteCuerpo, brochaTexto,
                margen, y, ancho, 18);

            y += 22;
            gfx.DrawString(string.Format(
                "Cuadrilla: {0}     Periodo: {1:dd/MM/yyyy} al {2:dd/MM/yyyy}",
                _codigoCuadrilla, _desde, _hasta),
                fuentePie, brochaTexto,
                new XRect(margen, y, ancho, 14), XStringFormats.TopLeft);

            y = page.Height - margen - 90;
            double anchoFirma = 260;
            double xFirma = margen + (ancho - anchoFirma) / 2;
            gfx.DrawLine(penFirma, xFirma, y, xFirma + anchoFirma, y);
            y += 6;
            gfx.DrawString((m.nombre ?? "").Trim(), fuenteCuerpoNeg, brochaTexto,
                new XRect(margen, y, ancho, 18), XStringFormats.TopCenter);
            y += 18;
            if (!string.IsNullOrEmpty(m.rol))
            {
                gfx.DrawString(m.rol, fuentePie, brochaTexto,
                    new XRect(margen, y, ancho, 14), XStringFormats.TopCenter);
            }
        }

        private static double DibujarParrafo(
            XGraphics gfx, string texto, XFont fuente, XBrush brocha,
            double x, double y, double ancho, double interlineado)
        {
            if (string.IsNullOrEmpty(texto)) return y;
            var palabras = texto.Split(' ');
            var lineaActual = new StringBuilder();
            double yActual = y;

            foreach (var palabra in palabras)
            {
                string prueba = lineaActual.Length == 0
                    ? palabra
                    : lineaActual + " " + palabra;
                var size = gfx.MeasureString(prueba, fuente);
                if (size.Width > ancho && lineaActual.Length > 0)
                {
                    gfx.DrawString(lineaActual.ToString(), fuente, brocha,
                        new XRect(x, yActual, ancho, interlineado), XStringFormats.TopLeft);
                    yActual += interlineado;
                    lineaActual.Clear();
                    lineaActual.Append(palabra);
                }
                else
                {
                    lineaActual.Length = 0;
                    lineaActual.Append(prueba);
                }
            }
            if (lineaActual.Length > 0)
            {
                gfx.DrawString(lineaActual.ToString(), fuente, brocha,
                    new XRect(x, yActual, ancho, interlineado), XStringFormats.TopLeft);
                yActual += interlineado;
            }
            return yActual;
        }

        private static string Truncar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }
    }
}
