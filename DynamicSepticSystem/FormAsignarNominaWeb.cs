// Asignación de Nómina por tarea: ventana dedicada que hospeda en WebView2 la
// página Calandria.Api/ui/asignar-nomina.html, reemplazando al diálogo nativo
// FormAsignarNomina para el botón "Distribución de nómina" de la ficha de un
// destajo Terminado en destajos.html. FormAsignarNomina.cs sigue intacta: la
// usa el árbol nativo (FormActivarTareasTreeList.cs), que no se migra aquí.
//
// La generación de PDF (PdfSharp) se queda igual que en el diálogo nativo —
// sólo cambia quién captura la cuadrilla y los montos.
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
    public class FormAsignarNominaWeb : Form
    {
        private readonly string _manzana;
        private readonly string _lote;
        private readonly string _ruta;
        private readonly int _nodoId;
        private readonly string _nombreTarea;
        private readonly decimal _totalDistribuir;

        private WebView2 webNomina;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "asignar-nomina.html");

        public FormAsignarNominaWeb(
            string manzana, string lote, string ruta, int nodoId, string nombreTarea, decimal totalDistribuir)
        {
            _manzana = manzana ?? "";
            _lote = lote ?? "";
            _ruta = ruta ?? "";
            _nodoId = nodoId;
            _nombreTarea = nombreTarea ?? "";
            _totalDistribuir = totalDistribuir;

            Text = "Asignar Nómina - " + _nombreTarea;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(760, 560);
            ClientSize = new Size(860, 640);
            BackColor = Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

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
                    "Calandria", "WebView2AsignarNomina");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webNomina.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("AsignarNominaWeb", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo iniciar la ventana de asignación de nómina:\n\n" + ex.Message,
                    "Asignar Nómina", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
            }
        }

        private void WebNomina_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("AsignarNominaWeb", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar la ventana de asignación de nómina.", "Asignar Nómina",
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
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("AsignarNominaWeb", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webNomina?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz de asignación de nómina.", "Asignar Nómina",
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/asignar-nomina/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/asignar-nomina");
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
                ErrorLogger.RegistrarMensaje("AsignarNominaWeb", "No se pudo traer la UI del API: " + ex.Message);
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
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("AsignarNominaWeb", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        private void EnviarDatosIniciales()
        {
            List<string> cuadrillas;
            try { cuadrillas = ApiClient.Get<List<string>>("/api/nomina/cuadrillas") ?? new List<string>(); }
            catch (Exception ex)
            {
                Push(new { tipo = "error", mensaje = "No se pudieron cargar las cuadrillas: " + ex.Message });
                cuadrillas = new List<string>();
            }

            string cuadrillaSeleccionada = null;
            List<MiembroCuadrillaApi> miembros = new List<MiembroCuadrillaApi>();
            Dictionary<string, decimal> montosPrevios = null;

            try
            {
                var asignacion = ApiClient.Get<AsignacionNominaApi>(
                    "/api/nomina/asignacion"
                    + "?manzana=" + Uri.EscapeDataString(_manzana)
                    + "&lote=" + Uri.EscapeDataString(_lote)
                    + "&ruta=" + Uri.EscapeDataString(_ruta)
                    + "&nodoId=" + _nodoId);

                if (!string.IsNullOrEmpty(asignacion?.CodigoCuadrilla))
                {
                    cuadrillaSeleccionada = asignacion.CodigoCuadrilla;
                    if (!cuadrillas.Contains(cuadrillaSeleccionada)) cuadrillas.Add(cuadrillaSeleccionada);

                    montosPrevios = new Dictionary<string, decimal>();
                    foreach (var mt in asignacion.Montos ?? new List<MontoTrabajadorApi>())
                        montosPrevios[ClaveMiembro(mt.IdTrabajador, mt.NombreTrabajador)] = mt.Monto;

                    miembros = ApiClient.Get<List<MiembroCuadrillaApi>>(
                        "/api/nomina/cuadrillas/" + Uri.EscapeDataString(cuadrillaSeleccionada) + "/miembros")
                        ?? new List<MiembroCuadrillaApi>();
                }
            }
            catch { /* sin asignación previa: estado inicial vacío */ }

            Push(new
            {
                tipo = "datos",
                nombreTarea = _nombreTarea,
                totalDistribuir = _totalDistribuir,
                puedeEditar = Global.UsuarioActual?.TienePermiso("nomina.editar") ?? false,
                cuadrillas,
                cuadrillaSeleccionada,
                miembros = miembros.Select(m => new
                {
                    idTrabajador = m.IdTrabajador,
                    nombre = m.Nombre,
                    rol = m.Rol,
                    esJefe = m.EsJefe,
                    telefono = m.Telefono,
                    monto = montosPrevios != null && montosPrevios.TryGetValue(ClaveMiembro(m.IdTrabajador, m.Nombre), out var mo) ? mo : 0m
                })
            });
        }

        private static string ClaveMiembro(int? idTrabajador, string nombre) =>
            idTrabajador.HasValue ? "ID:" + idTrabajador.Value : "N:" + (nombre ?? "").Trim().ToUpperInvariant();

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgCargarMiembros { public string cuadrilla; }
        private class MsgMiembro { public int? idTrabajador; public string nombre, rol, telefono; public bool esJefe; public decimal monto; }
        private class MsgGuardar { public string cuadrilla; public List<MsgMiembro> miembros; }

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
                    case "cargar-miembros":
                        { var d = JsonConvert.DeserializeObject<MsgCargarMiembros>(json); CargarMiembrosWeb(d?.cuadrilla); }
                        break;
                    case "guardar":
                        { var d = JsonConvert.DeserializeObject<MsgGuardar>(json); GuardarWeb(d?.cuadrilla, d?.miembros ?? new List<MsgMiembro>()); }
                        break;
                    case "cerrar":
                        Close();
                        break;
                    default:
                        ErrorLogger.RegistrarMensaje("AsignarNominaWeb", "Acción desconocida desde la página: " + m.accion);
                        break;
                }
            }));
        }

        private void CargarMiembrosWeb(string cuadrilla)
        {
            if (string.IsNullOrWhiteSpace(cuadrilla)) return;
            try
            {
                var miembros = ApiClient.Get<List<MiembroCuadrillaApi>>(
                    "/api/nomina/cuadrillas/" + Uri.EscapeDataString(cuadrilla) + "/miembros") ?? new List<MiembroCuadrillaApi>();
                Push(new
                {
                    tipo = "miembros",
                    miembros = miembros.Select(m => new { idTrabajador = m.IdTrabajador, nombre = m.Nombre, rol = m.Rol, esJefe = m.EsJefe, telefono = m.Telefono })
                });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "error", mensaje = "Error cargando miembros de la cuadrilla: " + ex.Message });
            }
        }

        private void GuardarWeb(string cuadrilla, List<MsgMiembro> miembros)
        {
            if (string.IsNullOrWhiteSpace(cuadrilla) || miembros.Count == 0)
            {
                Push(new { tipo = "guardarResultado", ok = false, mensaje = "Selecciona una cuadrilla con miembros." });
                return;
            }

            try
            {
                var request = new GuardarAsignacionRequestApi
                {
                    Manzana = _manzana,
                    Lote = _lote,
                    Ruta = _ruta,
                    NodoId = _nodoId,
                    NombreTarea = _nombreTarea,
                    CodigoCuadrilla = cuadrilla,
                    TotalDistribuir = _totalDistribuir
                };
                foreach (var m in miembros)
                {
                    request.Lineas.Add(new LineaAsignacionNominaApi
                    {
                        IdTrabajador = m.idTrabajador,
                        Nombre = m.nombre ?? "",
                        Rol = m.rol ?? "",
                        EsJefe = m.esJefe,
                        Monto = m.monto
                    });
                }

                ApiClient.Post("/api/nomina/asignacion", request);

                Push(new { tipo = "guardarResultado", ok = true });

                GenerarPdfNomina(cuadrilla, miembros);

                Close();
            }
            catch (ApiException ex)
            {
                Push(new { tipo = "guardarResultado", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "guardarResultado", ok = false, mensaje = "Error guardando la nómina: " + ex.Message });
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

        // ------------------------------------------------------------------
        // Generación de PDF — idéntica a FormAsignarNomina.cs, adaptada a
        // MsgMiembro (lo que captura la página) en vez del BindingList nativo.
        // ------------------------------------------------------------------

        private void GenerarPdfNomina(string codigoCuadrilla, List<MsgMiembro> miembros)
        {
            try
            {
                string nombreArchivo =
                    $"Nomina_M{_manzana}-L{_lote}_Nodo{_nodoId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string archivoTemp = Path.Combine(Path.GetTempPath(), nombreArchivo);

                CrearPdfNomina(archivoTemp, codigoCuadrilla, miembros);

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    nombreArchivo);

                using (var preview = new FormPdfPreview(archivoTemp, sugerido))
                    preview.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "La nómina se guardó pero no fue posible generar el PDF:\n" + ex.Message,
                    "PDF Nómina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CrearPdfNomina(string archivo, string codigoCuadrilla, List<MsgMiembro> miembros)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Asignación de Nómina";
            doc.Info.Author = "Pilaris";

            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            var gfx = XGraphics.FromPdfPage(page);

            var fuenteTitulo = new XFont("Arial", 16, XFontStyle.Bold);
            var fuenteSubtitulo = new XFont("Arial", 11, XFontStyle.Bold);
            var fuenteSeccion = new XFont("Arial", 10, XFontStyle.Bold);
            var fuenteNormal = new XFont("Arial", 9, XFontStyle.Regular);
            var fuenteEncabezado = new XFont("Arial", 9, XFontStyle.Bold);
            var fuentePequena = new XFont("Arial", 8, XFontStyle.Regular);

            var colorPrincipal = XColor.FromArgb(13, 71, 161);
            var colorSecundario = XColor.FromArgb(33, 150, 243);
            var colorBorde = XColor.FromArgb(189, 189, 189);
            var colorTexto = XColor.FromArgb(33, 33, 33);
            var colorGrisClaro = XColor.FromArgb(245, 245, 245);
            var colorVerde = XColor.FromArgb(39, 174, 96);
            var colorRojo = XColor.FromArgb(192, 57, 43);

            var brochaPrincipal = new XSolidBrush(colorPrincipal);
            var brochaSecundario = new XSolidBrush(colorSecundario);
            var brochaTexto = new XSolidBrush(colorTexto);
            var brochaBlanca = XBrushes.White;
            var brochaGrisClaro = new XSolidBrush(colorGrisClaro);
            var brochaVerde = new XSolidBrush(colorVerde);
            var brochaRojo = new XSolidBrush(colorRojo);

            var penBorde = new XPen(colorBorde, 0.5);
            var penPrincipal = new XPen(colorPrincipal, 1.5);

            double margen = 30;
            double ancho = page.Width - 2 * margen;
            double y = margen;

            gfx.DrawRectangle(brochaPrincipal, margen, y, ancho, 50);

            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var logo = XImage.FromStream(ms);
                    gfx.DrawImage(logo, margen + 6, y + 6, 38, 38);
                }
            }
            catch { }

            gfx.DrawString("CALANDRIA RESIDENCIAL", fuenteSubtitulo, brochaBlanca,
                new XRect(margen + 50, y + 6, ancho - 50, 14), XStringFormats.TopLeft);
            gfx.DrawString("ASIGNACIÓN DE NÓMINA", fuenteTitulo, brochaBlanca,
                new XRect(margen + 50, y + 22, ancho - 50, 18), XStringFormats.TopLeft);

            y += 60;

            gfx.DrawRectangle(brochaGrisClaro, margen, y, ancho, 38);
            gfx.DrawRectangle(penBorde, margen, y, ancho, 38);

            double colInfo = ancho / 4;
            string[] etiquetas = { "MANZANA", "LOTE", "NODO ID", "FECHA" };
            string[] valores = {
                $"M{_manzana}",
                $"L{_lote}",
                _nodoId.ToString(),
                DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.CreateSpecificCulture("es-MX"))
            };

            for (int i = 0; i < 4; i++)
            {
                double xCol = margen + i * colInfo;
                gfx.DrawString(etiquetas[i], fuenteEncabezado, brochaSecundario,
                    new XRect(xCol + 6, y + 4, colInfo - 12, 10), XStringFormats.TopLeft);
                gfx.DrawString(valores[i], fuenteSubtitulo, brochaPrincipal,
                    new XRect(xCol + 6, y + 16, colInfo - 12, 16), XStringFormats.TopLeft);
                if (i > 0)
                    gfx.DrawLine(penBorde, xCol, y + 4, xCol, y + 34);
            }

            y += 46;

            gfx.DrawRectangle(brochaSecundario, margen, y, ancho, 16);
            gfx.DrawString("INFORMACIÓN DE LA TAREA", fuenteSeccion, brochaBlanca,
                new XRect(margen + 4, y + 2, ancho - 8, 13), XStringFormats.TopLeft);
            y += 16;

            double altoDatos = 50;
            gfx.DrawRectangle(brochaGrisClaro, margen, y, ancho, altoDatos);
            gfx.DrawRectangle(penBorde, margen, y, ancho, altoDatos);

            gfx.DrawString("Tarea: " + TruncarN(_nombreTarea, 95), fuenteNormal, brochaTexto,
                new XRect(margen + 6, y + 6, ancho - 12, 12), XStringFormats.TopLeft);
            gfx.DrawString("Ruta: " + (_ruta ?? "-"), fuenteNormal, brochaTexto,
                new XRect(margen + 6, y + 20, ancho - 12, 12), XStringFormats.TopLeft);
            gfx.DrawString("Cuadrilla asignada: " + (codigoCuadrilla ?? "-"), fuenteNormal, brochaTexto,
                new XRect(margen + 6, y + 34, ancho - 12, 12), XStringFormats.TopLeft);

            y += altoDatos + 8;

            gfx.DrawRectangle(brochaVerde, margen, y, ancho, 22);
            gfx.DrawString(
                "TOTAL A DISTRIBUIR: " + _totalDistribuir.ToString("C2", CultureInfo.CurrentCulture),
                fuenteSubtitulo, brochaBlanca,
                new XRect(margen + 6, y + 4, ancho - 12, 16), XStringFormats.TopLeft);

            y += 30;

            gfx.DrawRectangle(brochaSecundario, margen, y, ancho, 16);
            gfx.DrawString("DISTRIBUCIÓN POR TRABAJADOR", fuenteSeccion, brochaBlanca,
                new XRect(margen + 4, y + 2, ancho - 8, 13), XStringFormats.TopLeft);
            y += 16;

            double colTrabajador = ancho * 0.45;
            double colRol = ancho * 0.18;
            double colTel = ancho * 0.17;
            double colMonto = ancho - colTrabajador - colRol - colTel;

            gfx.DrawRectangle(brochaPrincipal, margen, y, ancho, 14);
            gfx.DrawString("Trabajador", fuenteEncabezado, brochaBlanca,
                new XRect(margen + 4, y + 2, colTrabajador - 4, 10), XStringFormats.TopLeft);
            gfx.DrawString("Rol", fuenteEncabezado, brochaBlanca,
                new XRect(margen + colTrabajador + 4, y + 2, colRol - 4, 10), XStringFormats.TopLeft);
            gfx.DrawString("Teléfono", fuenteEncabezado, brochaBlanca,
                new XRect(margen + colTrabajador + colRol + 4, y + 2, colTel - 4, 10), XStringFormats.TopLeft);
            gfx.DrawString("Monto", fuenteEncabezado, brochaBlanca,
                new XRect(margen + colTrabajador + colRol + colTel + 4, y + 2, colMonto - 8, 10),
                XStringFormats.TopRight);

            y += 14;

            decimal asignadoTotal = 0m;
            bool altern = false;

            foreach (var m in miembros)
            {
                XSolidBrush brochaFila = m.esJefe
                    ? new XSolidBrush(XColor.FromArgb(255, 243, 224))
                    : (altern ? brochaGrisClaro : new XSolidBrush(XColor.FromArgb(255, 255, 255)));

                gfx.DrawRectangle(brochaFila, margen, y, ancho, 14);
                gfx.DrawRectangle(penBorde, margen, y, ancho, 14);

                string prefijo = m.esJefe ? "[JEFE] " : "";
                gfx.DrawString(TruncarN(prefijo + (m.nombre ?? "-"), 55), fuenteNormal, brochaTexto,
                    new XRect(margen + 4, y + 2, colTrabajador - 4, 10), XStringFormats.TopLeft);
                gfx.DrawString(TruncarN(m.rol ?? "-", 18), fuenteNormal, brochaTexto,
                    new XRect(margen + colTrabajador + 4, y + 2, colRol - 4, 10), XStringFormats.TopLeft);
                gfx.DrawString(TruncarN(m.telefono ?? "-", 18), fuenteNormal, brochaTexto,
                    new XRect(margen + colTrabajador + colRol + 4, y + 2, colTel - 4, 10), XStringFormats.TopLeft);
                gfx.DrawString(m.monto.ToString("C2", CultureInfo.CurrentCulture),
                    fuenteNormal, brochaTexto,
                    new XRect(margen + colTrabajador + colRol + colTel + 4, y + 2, colMonto - 8, 10),
                    XStringFormats.TopRight);

                y += 14;
                asignadoTotal += m.monto;
                altern = !altern;
            }

            gfx.DrawRectangle(brochaPrincipal, margen, y, ancho, 16);
            gfx.DrawString("TOTAL ASIGNADO", fuenteEncabezado, brochaBlanca,
                new XRect(margen + 4, y + 3, ancho - colMonto - 8, 12), XStringFormats.TopLeft);
            gfx.DrawString(asignadoTotal.ToString("C2", CultureInfo.CurrentCulture),
                fuenteEncabezado, brochaBlanca,
                new XRect(margen + colTrabajador + colRol + colTel + 4, y + 3, colMonto - 8, 12),
                XStringFormats.TopRight);

            y += 22;

            decimal diferencia = _totalDistribuir - asignadoTotal;
            if (Math.Abs(diferencia) >= 0.01m)
            {
                gfx.DrawString(
                    "Diferencia respecto al total: " + diferencia.ToString("C2", CultureInfo.CurrentCulture),
                    fuenteNormal, brochaRojo,
                    new XRect(margen, y, ancho, 12), XStringFormats.TopLeft);
                y += 14;
            }

            y += 30;
            double anchoFirma = (ancho / 3) - 6;
            double altoFirma = 50;
            double xFirma1 = margen;
            double xFirma2 = margen + anchoFirma + 9;
            double xFirma3 = margen + (anchoFirma + 9) * 2;
            double yLinea = y + altoFirma - 14;

            gfx.DrawLine(penBorde, xFirma1, yLinea, xFirma1 + anchoFirma, yLinea);
            gfx.DrawString("Jefe de Cuadrilla", fuentePequena, brochaTexto,
                new XRect(xFirma1, yLinea + 4, anchoFirma, 10), XStringFormats.TopCenter);

            gfx.DrawLine(penBorde, xFirma2, yLinea, xFirma2 + anchoFirma, yLinea);
            gfx.DrawString("Supervisor de Obra", fuentePequena, brochaTexto,
                new XRect(xFirma2, yLinea + 4, anchoFirma, 10), XStringFormats.TopCenter);

            gfx.DrawLine(penBorde, xFirma3, yLinea, xFirma3 + anchoFirma, yLinea);
            gfx.DrawString("Administración", fuentePequena, brochaTexto,
                new XRect(xFirma3, yLinea + 4, anchoFirma, 10), XStringFormats.TopCenter);

            double yPie = page.Height - margen - 10;
            gfx.DrawLine(penPrincipal, margen, yPie, margen + ancho, yPie);
            yPie += 2;
            string usuario = Global.UsuarioActual?.Nombre ?? Environment.UserName;
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm",
                CultureInfo.CreateSpecificCulture("es-MX"));
            gfx.DrawString($"Generado por: {usuario}  ·  {fecha}", fuentePequena, brochaTexto,
                new XRect(margen, yPie, ancho / 2, 10), XStringFormats.TopLeft);
            gfx.DrawString("© Pilaris", fuentePequena, brochaTexto,
                new XRect(margen + ancho / 2, yPie, ancho / 2, 10), XStringFormats.TopRight);

            doc.Save(archivo);
        }

        private static string TruncarN(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }
    }
}
