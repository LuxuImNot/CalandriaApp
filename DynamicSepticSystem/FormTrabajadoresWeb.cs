// Trabajadores híbrido: ventana dedicada que hospeda en WebView2 la página
// Calandria.Api/ui/trabajadores.html (registro de trabajadores, gestión de
// cuadrillas y perfil con historial de recibos en una sola UI con rail de
// navegación), igual que FormAlmacen.Web.cs / FormComprasWeb.cs. Reemplaza a
// FormRegistrarTrabajador/FormAsignarCuadrilla/FormPerfilTrabajador cuando
// TrabajadoresWeb=true (ver PanelPrincipal.AbrirFormRegistrarTrabajador y
// compañía); con TrabajadoresWeb=false los formularios clásicos siguen
// intactos.
//
// Toda lectura/escritura pasa por api/trabajadores (y, para leer cuadrillas
// existentes/recibos de nómina, api/nomina) vía ApiClient: nada de SQL
// directo desde este puente. Los documentos (CURP/RFC/INE/NSS, recibos) se
// descargan a un archivo temporal y se abren con el visor de PDF del sistema,
// igual que FormComprasWeb.VerPdfRepositorioWeb.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
    public class FormTrabajadoresWeb : Form
    {
        private readonly string _vistaInicial;
        private WebView2 webTrabajadores;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "trabajadores.html");

        public FormTrabajadoresWeb(string vistaInicial = "registro")
        {
            _vistaInicial = vistaInicial;

            Text = "Trabajadores - Sistema Calandria";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1280, 800);
            MinimumSize = new Size(1024, 640);
            WindowState = FormWindowState.Maximized;
            BackColor = Color.White;

            webTrabajadores = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webTrabajadores);

            webTrabajadores.CoreWebView2InitializationCompleted += WebTrabajadores_Init;
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
                    "Calandria", "WebView2Trabajadores");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webTrabajadores.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("TrabajadoresWeb", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo iniciar el módulo web de Trabajadores:\n\n" + ex.Message,
                    "Trabajadores", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
            }
        }

        private void WebTrabajadores_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("TrabajadoresWeb", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar el módulo web de Trabajadores.", "Trabajadores",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            var core = webTrabajadores.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebTrabajadores_Mensaje;
            core.NavigationCompleted += (s2, e2) =>
            {
                if (!e2.IsSuccess) return;
                EnviarSesionWeb();
                if (_vistaInicial != "registro")
                    Push(new { tipo = "vistaInicial", vista = _vistaInicial });
                _ = CargarTemaObraAsync();
            };

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("TrabajadoresWeb", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webTrabajadores?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz web de Trabajadores.", "Trabajadores",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            webTrabajadores.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/trabajadores/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/trabajadores");
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
                ErrorLogger.RegistrarMensaje("TrabajadoresWeb", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webTrabajadores?.CoreWebView2 == null) return;
            try { webTrabajadores.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("TrabajadoresWeb", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        /// <summary>Manda la paleta de acento de la obra activa (cosmético, nunca bloquea si falla).</summary>
        private async Task CargarTemaObraAsync()
        {
            object msg = null;
            try { msg = await Task.Run(() => TemaObra.ObtenerParaPush()); }
            catch { /* cosmético */ }
            if (!IsDisposed && msg != null) Push(msg);
        }

        /// <summary>Usuario/perfil y si puede editar (trabajadores.editar), para ocultar registro/cuadrillas a quien sólo tiene trabajadores.ver.</summary>
        private void EnviarSesionWeb()
        {
            Push(new
            {
                tipo = "sesion",
                usuario = Global.UsuarioActual?.Nombre ?? "",
                perfil = Global.UsuarioActual?.Perfil ?? "",
                puedeEditar = Global.UsuarioActual?.TienePermiso("trabajadores.editar") ?? false
            });
        }

        private void PushError(string mensaje)
        {
            if (webTrabajadores?.CoreWebView2 == null) return;
            try { webTrabajadores.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { tipo = "error", mensaje }, CamelCaseSettings)); }
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
            ErrorLogger.RegistrarMensaje("TrabajadoresWeb", contexto + ": " + msg);
            PushError(contexto + ": " + msg);
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgId { public int id; }
        private class MsgTrabajadorContexto { public int id; public string contexto; }
        private class MsgGuardarTrabajador
        {
            public int? id;
            public string nombre, nacionalidad, curp, rfc, ine, nss;
            public DateTime? fechaNacimiento;
            public string fotoBase64, pdfCurpBase64, pdfRfcBase64, pdfIneBase64, pdfNssBase64;
        }
        private class MsgVerDocumento { public int id; public string tipo; }
        private class MsgVerDocumentoLocal { public string base64, nombre; }
        private class MsgCargarCuadrilla { public string codigo; }
        private class MsgMiembroJs { public int? idTrabajador; public string nombre, rol, telefono; public bool esJefe; }
        private class MsgGuardarCuadrilla { public string codigo; public List<MsgMiembroJs> miembros; }

        private void WebTrabajadores_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
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
                    case "cargar-trabajadores": CargarTrabajadoresWeb(); break;
                    case "clave-nueva": ClaveNuevaWeb(); break;
                    case "cargar-trabajador":
                        { var d = JsonConvert.DeserializeObject<MsgTrabajadorContexto>(json); CargarTrabajadorWeb(d.id, d.contexto); }
                        break;
                    case "guardar-trabajador":
                        { var d = JsonConvert.DeserializeObject<MsgGuardarTrabajador>(json); GuardarTrabajadorWeb(d); }
                        break;
                    case "eliminar-trabajador":
                        { var d = JsonConvert.DeserializeObject<MsgId>(json); EliminarTrabajadorWeb(d.id); }
                        break;
                    case "ver-documento":
                        { var d = JsonConvert.DeserializeObject<MsgVerDocumento>(json); VerDocumentoWeb(d.id, d.tipo); }
                        break;
                    case "ver-documento-local":
                        { var d = JsonConvert.DeserializeObject<MsgVerDocumentoLocal>(json); VerDocumentoLocalWeb(d.base64, d.nombre); }
                        break;
                    case "cargar-recibos":
                        { var d = JsonConvert.DeserializeObject<MsgId>(json); CargarRecibosWeb(d.id); }
                        break;
                    case "ver-recibo":
                        { var d = JsonConvert.DeserializeObject<MsgId>(json); VerReciboWeb(d.id); }
                        break;
                    case "cargar-codigos-cuadrilla": CargarCodigosCuadrillaWeb(); break;
                    case "cargar-cuadrilla":
                        { var d = JsonConvert.DeserializeObject<MsgCargarCuadrilla>(json); CargarCuadrillaWeb(d.codigo); }
                        break;
                    case "guardar-cuadrilla":
                        { var d = JsonConvert.DeserializeObject<MsgGuardarCuadrilla>(json); GuardarCuadrillaWeb(d); }
                        break;
                    case "volver":
                        Close();
                        break;
                    default:
                        ErrorLogger.RegistrarMensaje("TrabajadoresWeb", "Acción desconocida desde la página: " + accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                ManejarErrorApi(ex, "No se pudo completar la acción solicitada");
            }
        }

        // ------------------------------------------------------------------
        // Trabajadores — CRUD
        // ------------------------------------------------------------------

        private void CargarTrabajadoresWeb()
        {
            try { Push(new { tipo = "trabajadores", lista = ApiClient.Get<List<TrabajadorApi>>("/api/trabajadores") ?? new List<TrabajadorApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los trabajadores"); }
        }

        private void ClaveNuevaWeb()
        {
            try { Push(new { tipo = "claveNueva", clave = ApiClient.Get<ClaveNuevaApi>("/api/trabajadores/clave-nueva")?.Clave }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo generar la matrícula"); }
        }

        private void CargarTrabajadorWeb(int id, string contexto)
        {
            try
            {
                var d = ApiClient.Get<TrabajadorDetalleApi>($"/api/trabajadores/{id}");
                if (d == null) { PushError("No se encontró el trabajador."); return; }
                Push(new
                {
                    tipo = "trabajadorDetalle",
                    contexto,
                    id = d.Id,
                    clave = d.Clave,
                    nombre = d.Nombre,
                    nacionalidad = d.Nacionalidad,
                    fechaNacimiento = d.FechaNacimiento,
                    curp = d.Curp,
                    rfc = d.Rfc,
                    ine = d.Ine,
                    nss = d.Nss,
                    fotoBase64 = d.FotoBase64,
                    tieneCurp = d.TieneCurp, curpKb = d.CurpKb,
                    tieneRfc = d.TieneRfc, rfcKb = d.RfcKb,
                    tieneIne = d.TieneIne, ineKb = d.IneKb,
                    tieneNss = d.TieneNss, nssKb = d.NssKb,
                    rolCuadrilla = d.RolCuadrilla
                });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el trabajador"); }
        }

        private void GuardarTrabajadorWeb(MsgGuardarTrabajador d)
        {
            try
            {
                var resp = ApiClient.Post<TrabajadorDetalleApi>("/api/trabajadores", new
                {
                    Id = d.id,
                    Nombre = d.nombre,
                    Nacionalidad = d.nacionalidad,
                    FechaNacimiento = d.fechaNacimiento,
                    Curp = d.curp,
                    Rfc = d.rfc,
                    Ine = d.ine,
                    Nss = d.nss,
                    FotoBase64 = d.fotoBase64,
                    PdfCurpBase64 = d.pdfCurpBase64,
                    PdfRfcBase64 = d.pdfRfcBase64,
                    PdfIneBase64 = d.pdfIneBase64,
                    PdfNssBase64 = d.pdfNssBase64
                });
                Push(new
                {
                    tipo = "trabajadorGuardado",
                    ok = true,
                    detalle = new
                    {
                        id = resp.Id, clave = resp.Clave, nombre = resp.Nombre, nacionalidad = resp.Nacionalidad,
                        fechaNacimiento = resp.FechaNacimiento, curp = resp.Curp, rfc = resp.Rfc, ine = resp.Ine, nss = resp.Nss,
                        fotoBase64 = resp.FotoBase64,
                        tieneCurp = resp.TieneCurp, curpKb = resp.CurpKb,
                        tieneRfc = resp.TieneRfc, rfcKb = resp.RfcKb,
                        tieneIne = resp.TieneIne, ineKb = resp.IneKb,
                        tieneNss = resp.TieneNss, nssKb = resp.NssKb,
                        rolCuadrilla = resp.RolCuadrilla
                    }
                });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Push(new { tipo = "trabajadorGuardado", ok = false, mensaje = "Ya existe un trabajador con esa matrícula." });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "trabajadorGuardado", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "trabajadorGuardado", ok = false, mensaje = "No se pudo guardar: " + ex.Message });
            }
        }

        private void EliminarTrabajadorWeb(int id)
        {
            try
            {
                int filas = ApiClient.Post<int>($"/api/trabajadores/{id}/eliminar", new { });
                Push(new { tipo = "trabajadorEliminado", ok = filas > 0, mensaje = filas > 0 ? "Trabajador eliminado." : "No se encontró el trabajador." });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo eliminar el trabajador"); }
        }

        // ------------------------------------------------------------------
        // Documentos y recibos — descarga a temporal + visor del sistema
        // ------------------------------------------------------------------

        private static readonly string[] TiposDocumentoValidos = { "curp", "rfc", "ine", "nss" };

        private void AbrirPdfTemporal(byte[] bytes, string nombreBase)
        {
            if (bytes == null || bytes.Length == 0) { PushError("No hay PDF cargado para " + nombreBase + "."); return; }
            string archivo = Path.Combine(Path.GetTempPath(), $"{nombreBase}_{Guid.NewGuid():N}.pdf");
            File.WriteAllBytes(archivo, bytes);
            try { Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true }); }
            catch { PushError($"PDF guardado en: {archivo}"); }
        }

        private void VerDocumentoWeb(int id, string tipo)
        {
            if (Array.IndexOf(TiposDocumentoValidos, (tipo ?? "").ToLowerInvariant()) < 0) return;
            try { AbrirPdfTemporal(ApiClient.GetBytes($"/api/trabajadores/{id}/documento/{tipo}"), tipo.ToUpperInvariant()); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo abrir el documento"); }
        }

        private void VerDocumentoLocalWeb(string base64, string nombre)
        {
            if (string.IsNullOrEmpty(base64)) { PushError("No hay archivo pendiente para " + nombre + "."); return; }
            try { AbrirPdfTemporal(Convert.FromBase64String(base64), nombre ?? "documento"); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo abrir el documento"); }
        }

        private void CargarRecibosWeb(int id)
        {
            try { Push(new { tipo = "recibos", lista = ApiClient.Get<List<ReciboTrabajadorApi>>($"/api/trabajadores/{id}/recibos") ?? new List<ReciboTrabajadorApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los recibos"); }
        }

        private void VerReciboWeb(int idRecibo)
        {
            try
            {
                byte[] pdf = ApiClient.GetBytes($"/api/nomina/recibos/{idRecibo}/pdf");
                if (pdf == null || pdf.Length == 0) { PushError("El PDF ya no está disponible."); return; }
                AbrirPdfTemporal(pdf, "Recibo");
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo abrir el recibo"); }
        }

        // ------------------------------------------------------------------
        // Cuadrillas — la lectura reutiliza api/nomina (ya expuesto); el
        // guardado vive en api/trabajadores/cuadrillas.
        // ------------------------------------------------------------------

        private void CargarCodigosCuadrillaWeb()
        {
            try { Push(new { tipo = "codigosCuadrilla", lista = ApiClient.Get<List<string>>("/api/nomina/cuadrillas") ?? new List<string>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las cuadrillas"); }
        }

        private void CargarCuadrillaWeb(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return;
            try
            {
                var miembros = ApiClient.Get<List<MiembroCuadrillaApi>>($"/api/nomina/cuadrillas/{Uri.EscapeDataString(codigo)}/miembros") ?? new List<MiembroCuadrillaApi>();
                Push(new { tipo = "cuadrillaCargada", codigo, miembros });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar la cuadrilla"); }
        }

        private void GuardarCuadrillaWeb(MsgGuardarCuadrilla d)
        {
            try
            {
                var miembros = d.miembros ?? new List<MsgMiembroJs>();
                var resp = ApiClient.Post<CuadrillaGuardadaApi>("/api/trabajadores/cuadrillas", new
                {
                    Codigo = d.codigo,
                    Miembros = miembros.ConvertAll(m => new
                    {
                        IdTrabajador = m.idTrabajador,
                        Nombre = m.nombre,
                        Rol = m.rol,
                        EsJefe = m.esJefe,
                        Telefono = m.telefono
                    })
                });
                Push(new { tipo = "cuadrillaGuardada", ok = true, codigo = resp.Codigo });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "cuadrillaGuardada", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "cuadrillaGuardada", ok = false, mensaje = "No se pudo guardar la cuadrilla: " + ex.Message });
            }
        }
    }
}
