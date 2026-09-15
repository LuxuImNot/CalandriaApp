// Consulta por Casa (Dinero): hospeda en WebView2 la página
// Calandria.Api/ui/administrativos.html, mismo patrón que FormPerfilesWeb.cs /
// FormTrabajadoresWeb.cs. Ya no existe un panel clásico al que volver si
// WebView2 falla — la migración a web se completó (ver FormAdministrativos.cs) —
// así que un fallo aquí muestra el error y cierra la ventana en vez de caer a
// un panel WinForms.
//
// Toda lectura pasa por api/administrativos (AdministrativosController): nada de
// SQL directo desde este puente. Es un modulo de solo lectura + exportar PDF: no
// hay acciones de escritura. El PDF se sigue generando en el cliente con PdfSharp
// (GenerarPDF, reutilizado tal cual) pero alimentado por el ultimo consolidado que
// trajo el API, no por SQL directo.
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public partial class FormAdministrativos
    {
        private WebView2 webAdministrativos;
        private ConsolidadoCasaApi ultimoConsolidado;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "administrativos.html");

        // ------------------------------------------------------------------
        // Montaje
        // ------------------------------------------------------------------

        private void InicializarPanelWeb()
        {
            webAdministrativos = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White
            };
            this.Controls.Add(webAdministrativos);

            webAdministrativos.CoreWebView2InitializationCompleted += WebAdministrativos_Init;
            _ = IniciarWebView2Async();
        }

        /// <summary>
        /// Entorno propio con su propia carpeta de datos de usuario: FormAdministrativos
        /// se abre no-modal (Show, ver PanelPrincipal.AbrirFormAdministrativos) junto al
        /// panel principal y otros formularios con su propio WebView2 activo. Compartir el
        /// entorno implícito entre varios controles WebView2 del mismo proceso ya causó un
        /// COMException que tumbaba el proceso entero (ver reference_webview2_multi_instance).
        ///
        /// La creación del entorno falla intermitentemente con COMException (E_ABORT /
        /// "Catastrophic failure" / "Class not registered") y luego funciona al reintentar
        /// (ya visto en Destajos, Compras, Panel y Perfiles). Se reintenta unas veces antes
        /// de rendirse: sin panel clásico al que caer, un fallo transitorio no debe cerrar
        /// la ventana en el primer intento.
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
                        "Calandria", "WebView2Administrativos");
                    var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                    await webAdministrativos.EnsureCoreWebView2Async(entorno);
                    return;
                }
                catch (Exception ex)
                {
                    ErrorLogger.RegistrarMensaje("AdministrativosWeb", $"IniciarWebView2Async intento {intento}/{intentos} falló: " + ex.Message);
                    if (IsDisposed) return;
                    if (intento == intentos)
                    {
                        MessageBox.Show("No se pudo iniciar el módulo de Administrativos:\n\n" + ex.Message,
                            "Administrativos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                    }
                    await Task.Delay(500 * intento);
                }
            }
        }

        private void WebAdministrativos_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("AdministrativosWeb", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar el módulo de Administrativos.", "Administrativos",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            var core = webAdministrativos.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebAdministrativos_Mensaje;
            core.NavigationCompleted += async (s2, e2) =>
            {
                if (e2.IsSuccess) await CargarTemaObraAsync();
            };

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("AdministrativosWeb", "Fallo cargando la página: " + ex.Message);
            }

            if (IsDisposed || webAdministrativos?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz web de Administrativos.", "Administrativos",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            webAdministrativos.CoreWebView2.NavigateToString(html);
        }

        // ------------------------------------------------------------------
        // Obtención del HTML: API -> caché (idéntico patrón que Almacén/Destajos/Panel)
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/administrativos/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/administrativos");
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
                ErrorLogger.RegistrarMensaje("AdministrativosWeb", "No se pudo traer la UI del API: " + ex.Message);
            }

            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webAdministrativos?.CoreWebView2 == null) return;
            try
            {
                webAdministrativos.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings));
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("AdministrativosWeb", "Fallo al enviar datos a la página: " + ex.Message);
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

        private void PushError(string mensaje)
        {
            if (webAdministrativos?.CoreWebView2 == null) return;
            try
            {
                webAdministrativos.CoreWebView2.PostWebMessageAsJson(
                    JsonConvert.SerializeObject(new { tipo = "error", mensaje }, CamelCaseSettings));
            }
            catch { /* si ni el aviso de error sale, no hay más que intentar */ }
        }

        private void ManejarErrorApi(Exception ex, string contexto)
        {
            var apiEx = ex as ApiException;
            string msg = apiEx != null ? MensajeDe(apiEx) : ex.Message;
            ErrorLogger.RegistrarMensaje("AdministrativosWeb", contexto + ": " + msg);
            MessageBox.Show(contexto + ":\n\n" + msg, "Administrativos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        private class MsgLotes { public string manzana; }
        private class MsgCasa { public string manzana; public string lote; }

        private void WebAdministrativos_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;
            MsgAccion m;
            try { m = JsonConvert.DeserializeObject<MsgAccion>(json); }
            catch { return; }
            if (m?.accion == null) return;

            // BeginInvoke: igual que en Almacén, para no despachar dentro del propio
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
                        { var d = JsonConvert.DeserializeObject<MsgLotes>(json); await CargarLotesWeb(d.manzana); }
                        break;
                    case "cargar-consolidado":
                        { var d = JsonConvert.DeserializeObject<MsgCasa>(json); await CargarConsolidadoWeb(d.manzana, d.lote); }
                        break;
                    case "exportar-pdf":
                        { var d = JsonConvert.DeserializeObject<MsgCasa>(json); await ExportarPdfWeb(d.manzana, d.lote); }
                        break;
                    case "volver":
                        Close();
                        break;

                    default:
                        ErrorLogger.RegistrarMensaje("AdministrativosWeb", "Acción desconocida desde la página: " + accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo completar la acción solicitada:\n\n" + ex.Message,
                    "Administrativos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ------------------------------------------------------------------
        // Lecturas — todas vía api/administrativos, nada de SQL directo
        // ------------------------------------------------------------------

        private async Task CargarManzanasWeb()
        {
            try
            {
                var lista = await Task.Run(() => ApiClient.Get<List<string>>("/api/administrativos/manzanas")) ?? new List<string>();
                Push(new { tipo = "manzanas", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las manzanas"); }
        }

        private async Task CargarLotesWeb(string manzana)
        {
            if (string.IsNullOrWhiteSpace(manzana)) return;
            try
            {
                var lista = await Task.Run(() => ApiClient.Get<List<string>>("/api/administrativos/lotes?manzana=" + Uri.EscapeDataString(manzana))) ?? new List<string>();
                Push(new { tipo = "lotes", lista });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los lotes"); }
        }

        private async Task CargarConsolidadoWeb(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote)) return;
            try
            {
                string url = "/api/administrativos/consolidado?manzana=" + Uri.EscapeDataString(manzana)
                    + "&lote=" + Uri.EscapeDataString(lote);
                var consolidado = await Task.Run(() => ApiClient.Get<ConsolidadoCasaApi>(url));
                if (consolidado == null) { PushError("No se encontró información para M" + manzana + " L" + lote + "."); return; }

                ultimoConsolidado = consolidado;
                manzanaActual = manzana;
                loteActual = lote;
                prototipoActual = consolidado.Prototipo;

                Push(new
                {
                    tipo = "consolidado",
                    manzana = consolidado.Manzana,
                    lote = consolidado.Lote,
                    prototipo = consolidado.Prototipo,
                    estimaciones = consolidado.Estimaciones,
                    destajos = consolidado.Destajos,
                    compras = consolidado.Compras,
                    salidasAlmacen = consolidado.SalidasAlmacen,
                    nomina = consolidado.Nomina,
                    totalEstimaciones = consolidado.TotalEstimaciones,
                    totalDestajosComprometido = consolidado.TotalDestajosComprometido,
                    totalManoObraGastado = consolidado.TotalManoObraGastado,
                    totalMaterialGastado = consolidado.TotalMaterialGastado,
                    totalCompras = consolidado.TotalCompras,
                    totalSalidasAlmacen = consolidado.TotalSalidasAlmacen,
                    totalNomina = consolidado.TotalNomina,
                    granTotal = consolidado.GranTotal
                });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo consultar la casa"); }
        }

        // ------------------------------------------------------------------
        // Exportar PDF — reutiliza GenerarPDF, alimentado por el último
        // consolidado traído del API (no vuelve a tocar SQL directo).
        // ------------------------------------------------------------------

        private async Task ExportarPdfWeb(string manzana, string lote)
        {
            if (ultimoConsolidado == null || ultimoConsolidado.Manzana != manzana || ultimoConsolidado.Lote != lote)
                await CargarConsolidadoWeb(manzana, lote);

            if (ultimoConsolidado == null)
            {
                Push(new { tipo = "pdfExportado", ok = false, mensaje = "No hay datos consultados para exportar." });
                return;
            }

            MapearConsolidadoALocal(ultimoConsolidado);

            try
            {
                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CALANDRIA RESIDENCIAL", "Administrativos");
                Directory.CreateDirectory(carpeta);

                string sugerido = $"Administrativos_M{manzanaActual}_L{loteActual}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

                using (var sfd = new SaveFileDialog
                {
                    Filter = "Archivo PDF (*.pdf)|*.pdf",
                    FileName = sugerido,
                    InitialDirectory = carpeta,
                    Title = "Exportar concentrado administrativo a PDF"
                })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK)
                    {
                        Push(new { tipo = "pdfExportado", ok = false, mensaje = "Exportación cancelada." });
                        return;
                    }

                    GenerarPDF(sfd.FileName, ultimoConsolidado.Compras, ultimoConsolidado.SalidasAlmacen, ultimoConsolidado.Nomina);
                    Push(new { tipo = "pdfExportado", ok = true, mensaje = "PDF generado: " + sfd.FileName });

                    var resp = MessageBox.Show(
                        $"PDF generado:\n{sfd.FileName}\n\n¿Desea abrirlo ahora?",
                        "Administrativos", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (resp == DialogResult.Yes)
                    {
                        try { System.Diagnostics.Process.Start(sfd.FileName); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("AdministrativosWeb", "Error generando el PDF: " + ex.Message);
                Push(new { tipo = "pdfExportado", ok = false, mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Traduce el ConsolidadoCasaApi (traído del API) a los mismos campos que ya
        /// usa GenerarPDF (registrosEstimacion/registrosDestajo/totales), sin volver a
        /// pegarle a SQL directo — GenerarPDF no hace SQL, sólo dibuja lo que ya está
        /// en memoria, así que basta con llenar esos campos igual que lo hacían
        /// CargarEstimaciones/CargarDestajos/RefrescarHUD antes de la migración.
        /// </summary>
        private void MapearConsolidadoALocal(ConsolidadoCasaApi c)
        {
            registrosEstimacion.Clear();
            foreach (var e in c.Estimaciones ?? new List<EstimacionItemApi>())
            {
                registrosEstimacion.Add(new RegistroEstimacion
                {
                    WBS = e.Wbs,
                    Codigo = e.Codigo,
                    Etapa = e.Etapa,
                    Partida = e.Partida,
                    AvancePorcentaje = e.AvancePorcentaje,
                    MontoEjecutado = e.MontoEjecutado,
                    FechaFinalizacion = e.FechaFinalizacion
                });
            }

            registrosDestajo.Clear();
            foreach (var d in c.Destajos ?? new List<DestajoItemApi>())
            {
                TipoTarea tipo = d.Tipo == "M. de Obra" ? TipoTarea.ManoDeObra
                    : d.Tipo == "Material" ? TipoTarea.Material
                    : TipoTarea.Ninguno;

                registrosDestajo.Add(new RegistroDestajo
                {
                    NodoID = d.NodoId,
                    Nombre = d.Nombre,
                    Categoria = d.Categoria,
                    Cantidad = d.Cantidad,
                    Unidad = d.Unidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Cuadrilla = d.Cuadrilla,
                    Finalizado = d.Finalizado,
                    Tipo = tipo,
                    // NominaAsignada sólo importa para el destajo de M.O.: el importe
                    // gastado ya viene calculado del API (MontoGastado), así que se
                    // reconstruye aquí para que RegistroDestajo.MontoGastado coincida.
                    NominaAsignada = tipo == TipoTarea.ManoDeObra ? d.MontoGastado : 0m
                });
            }

            totalEstimaciones = c.TotalEstimaciones;
            totalManoObra = c.TotalManoObraGastado;
            totalMaterial = c.TotalMaterialGastado;
            conteoMO = registrosDestajo.Count(r => r.Tipo == TipoTarea.ManoDeObra);
            conteoMat = registrosDestajo.Count(r => r.Tipo == TipoTarea.Material);
        }
    }
}
