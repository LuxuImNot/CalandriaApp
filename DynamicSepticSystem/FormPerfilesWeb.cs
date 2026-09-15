// Perfiles y Permisos: ventana dedicada que hospeda en WebView2 la página
// Calandria.Api/ui/perfiles.html (constructor de perfiles + asignación de
// perfil a usuarios), igual que FormTrabajadoresWeb.cs / FormComprasWeb.cs.
// Reemplaza al diálogo nativo FormPerfiles.
//
// Toda lectura/escritura pasa por api/perfiles vía ApiClient: nada de SQL
// directo desde este puente.
using System;
using System.Collections.Generic;
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
    public class FormPerfilesWeb : Form
    {
        private WebView2 webPerfiles;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "perfiles.html");

        private static void Log(string msg) => ErrorLogger.RegistrarMensaje("PerfilesWeb", msg);

        public FormPerfilesWeb()
        {
            Log("Constructor: iniciando");
            Text = "Perfiles y Permisos - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new System.Drawing.Size(1180, 760);
            MinimumSize = new System.Drawing.Size(960, 600);
            WindowState = FormWindowState.Maximized;
            BackColor = System.Drawing.Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webPerfiles = new WebView2 { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.White };
            Controls.Add(webPerfiles);

            webPerfiles.CoreWebView2InitializationCompleted += WebPerfiles_Init;
            Log("Constructor: lanzando IniciarWebView2Async");
            _ = IniciarWebView2Async();
        }

        /// <summary>
        /// Entorno y carpeta de datos propios: varias ventanas WebView2 del mismo
        /// proceso no deben compartir el entorno implícito (ver
        /// reference_webview2_multi_instance / FormAlmacen.Web.cs).
        ///
        /// La creación del entorno falla intermitentemente con COMException (E_ABORT /
        /// "Catastrophic failure" / "Class not registered") y luego funciona al
        /// reintentar — ya se vio en Destajos, Compras y Panel (ver errores.log). Se
        /// reintenta unas veces antes de rendirse.
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
                        "Calandria", "WebView2Perfiles");
                    Log($"IniciarWebView2Async intento {intento}/{intentos}: CreateAsync (carpeta={userDataFolder})");
                    var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                    Log($"IniciarWebView2Async intento {intento}: entorno creado, versión runtime={entorno.BrowserVersionString}");

                    Log($"IniciarWebView2Async intento {intento}: EnsureCoreWebView2Async...");
                    await webPerfiles.EnsureCoreWebView2Async(entorno);
                    Log($"IniciarWebView2Async intento {intento}: EnsureCoreWebView2Async OK");
                    return;
                }
                catch (Exception ex)
                {
                    Log($"IniciarWebView2Async intento {intento}/{intentos} FALLÓ: {ex.GetType().Name}: {ex.Message}");
                    if (IsDisposed) { Log("IniciarWebView2Async: form ya disposed, abortando"); return; }
                    if (intento == intentos)
                    {
                        MessageBox.Show("No se pudo iniciar el módulo de Perfiles:\n\n" + ex.Message,
                            "Perfiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                    }
                    await Task.Delay(500 * intento);
                }
            }
        }

        private void WebPerfiles_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            Log("WebPerfiles_Init: IsSuccess=" + e.IsSuccess);
            if (!e.IsSuccess)
            {
                Log("WebPerfiles_Init: WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar el módulo de Perfiles.", "Perfiles",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            try
            {
                var core = webPerfiles.CoreWebView2;
                core.Settings.AreDefaultContextMenusEnabled = System.Diagnostics.Debugger.IsAttached;
                core.Settings.AreDevToolsEnabled = System.Diagnostics.Debugger.IsAttached;
                core.Settings.IsStatusBarEnabled = false;

                core.WebMessageReceived += WebPerfiles_Mensaje;
                core.NavigationCompleted += (s2, e2) =>
                {
                    Log($"NavigationCompleted: IsSuccess={e2.IsSuccess} WebErrorStatus={e2.WebErrorStatus}");
                    if (e2.IsSuccess) _ = CargarTemaObraAsync();
                };
                core.NavigationStarting += (s2, e2) => Log("NavigationStarting: " + e2.Uri?.Substring(0, Math.Min(60, e2.Uri.Length)));
                core.ProcessFailed += (s2, e2) =>
                    Log($"ProcessFailed: Kind={e2.ProcessFailedKind} Reason={e2.Reason} ExitCode={e2.ExitCode}");

                Log("WebPerfiles_Init: configuración de WebView2 aplicada OK");
            }
            catch (Exception ex)
            {
                Log("WebPerfiles_Init: fallo configurando WebView2: " + ex.GetType().Name + ": " + ex.Message);
                MessageBox.Show("No se pudo preparar el módulo de Perfiles:\n\n" + ex.Message,
                    "Perfiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            Log("CargarHtmlAsync: descargando HTML...");
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { Log("CargarHtmlAsync: fallo cargando la página: " + ex.GetType().Name + ": " + ex.Message); }

            Log($"CargarHtmlAsync: HTML obtenido = {(html == null ? "null" : html.Length + " chars")}");

            if (IsDisposed || webPerfiles?.CoreWebView2 == null)
            {
                Log("CargarHtmlAsync: form disposed o CoreWebView2 nulo, abortando antes de navegar");
                return;
            }
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz web de Perfiles.", "Perfiles",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            try
            {
                Log("CargarHtmlAsync: llamando NavigateToString");
                webPerfiles.CoreWebView2.NavigateToString(html);
                Log("CargarHtmlAsync: NavigateToString retornó sin excepción");
            }
            catch (Exception ex)
            {
                Log("CargarHtmlAsync: NavigateToString lanzó: " + ex.GetType().Name + ": " + ex.Message);
            }
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/perfiles/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/perfiles");
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
                ErrorLogger.RegistrarMensaje("PerfilesWeb", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webPerfiles?.CoreWebView2 == null) return;
            try { webPerfiles.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("PerfilesWeb", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        /// <summary>Manda la paleta de acento de la obra activa (cosmético, nunca bloquea si falla).</summary>
        private async Task CargarTemaObraAsync()
        {
            object msg = null;
            try { msg = await Task.Run(() => TemaObra.ObtenerParaPush()); }
            catch { /* cosmético */ }
            if (!IsDisposed && msg != null) Push(msg);
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
            ErrorLogger.RegistrarMensaje("PerfilesWeb", contexto + ": " + msg);
            Push(new { tipo = "error", mensaje = contexto + ": " + msg });
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgId { public int id; }
        private class MsgGuardarPerfil { public int? id; public string nombre, descripcion; public List<string> permisos; }
        private class MsgAsignarPerfil { public string usuario; public int perfilId; }
        private class MsgAsignarObras { public string usuario; public List<int> obraIds; }
        private class MsgCrearUsuario { public string usuario, clave, fotoBase64, extension, fechaAlta; public int perfilId; }
        private class MsgFotoUsuario { public string usuario, fotoBase64, extension; }
        private class MsgFechaAlta { public string usuario, fechaAlta; }

        private void WebPerfiles_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;
            MsgAccion m;
            try { m = JsonConvert.DeserializeObject<MsgAccion>(json); }
            catch (Exception ex) { Log("WebPerfiles_Mensaje: JSON inválido: " + ex.Message); return; }
            if (m?.accion == null) return;

            Log("WebPerfiles_Mensaje: acción recibida de la página = " + m.accion);
            // BeginInvoke: no despachar dentro del propio callback COM de WebView2.
            BeginInvoke((Action)(() => DespacharMensaje(m.accion, json)));
        }

        private void DespacharMensaje(string accion, string json)
        {
            try
            {
                switch (accion)
                {
                    case "cargar-perfiles": CargarPerfilesWeb(); break;
                    case "cargar-catalogo": CargarCatalogoWeb(); break;
                    case "cargar-detalle-perfil":
                        { var d = JsonConvert.DeserializeObject<MsgId>(json); CargarDetallePerfilWeb(d.id); }
                        break;
                    case "guardar-perfil":
                        { var d = JsonConvert.DeserializeObject<MsgGuardarPerfil>(json); GuardarPerfilWeb(d); }
                        break;
                    case "eliminar-perfil":
                        { var d = JsonConvert.DeserializeObject<MsgId>(json); EliminarPerfilWeb(d.id); }
                        break;
                    case "cargar-usuarios": CargarUsuariosWeb(); break;
                    case "asignar-perfil":
                        { var d = JsonConvert.DeserializeObject<MsgAsignarPerfil>(json); AsignarPerfilWeb(d); }
                        break;
                    case "crear-usuario":
                        { var d = JsonConvert.DeserializeObject<MsgCrearUsuario>(json); CrearUsuarioWeb(d); }
                        break;
                    case "subir-foto-usuario":
                        { var d = JsonConvert.DeserializeObject<MsgFotoUsuario>(json); SubirFotoUsuarioWeb(d); }
                        break;
                    case "confirmar-fecha-alta":
                        { var d = JsonConvert.DeserializeObject<MsgFechaAlta>(json); ConfirmarFechaAltaWeb(d); }
                        break;
                    case "cargar-obras-catalogo": CargarObrasCatalogoWeb(); break;
                    case "cargar-asignaciones-obras": CargarAsignacionesObrasWeb(); break;
                    case "guardar-obras-usuario":
                        { var d = JsonConvert.DeserializeObject<MsgAsignarObras>(json); GuardarObrasUsuarioWeb(d); }
                        break;
                    case "volver":
                        Close();
                        break;
                    default:
                        ErrorLogger.RegistrarMensaje("PerfilesWeb", "Acción desconocida desde la página: " + accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                ManejarErrorApi(ex, "No se pudo completar la acción solicitada");
            }
        }

        // ------------------------------------------------------------------
        // Perfiles — CRUD
        // ------------------------------------------------------------------

        private void CargarPerfilesWeb()
        {
            try { Push(new { tipo = "perfiles", lista = ApiClient.Get<List<PerfilResumenApi>>("/api/perfiles") ?? new List<PerfilResumenApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los perfiles"); }
        }

        private void CargarCatalogoWeb()
        {
            try { Push(new { tipo = "catalogo", lista = ApiClient.Get<List<PermisoApi>>("/api/perfiles/catalogo-permisos") ?? new List<PermisoApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el catálogo de permisos"); }
        }

        private void CargarDetallePerfilWeb(int id)
        {
            try
            {
                var d = ApiClient.Get<PerfilDetalleApi>($"/api/perfiles/{id}");
                if (d == null) { Push(new { tipo = "error", mensaje = "No se encontró el perfil." }); return; }
                Push(new { tipo = "detallePerfil", id = d.Id, nombre = d.Nombre, descripcion = d.Descripcion, esSistema = d.EsSistema, permisos = d.Permisos });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo cargar el perfil"); }
        }

        private void GuardarPerfilWeb(MsgGuardarPerfil d)
        {
            try
            {
                var resp = ApiClient.Post<GuardarPerfilRespApi>("/api/perfiles", new
                {
                    Id = d.id,
                    Nombre = d.nombre,
                    Descripcion = d.descripcion,
                    Permisos = d.permisos ?? new List<string>()
                });
                Push(new { tipo = "perfilGuardado", ok = true, id = resp?.Id });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Push(new { tipo = "perfilGuardado", ok = false, mensaje = "Ya existe un perfil con ese nombre." });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "perfilGuardado", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "perfilGuardado", ok = false, mensaje = "No se pudo guardar: " + ex.Message });
            }
        }

        private void EliminarPerfilWeb(int id)
        {
            try
            {
                ApiClient.Post($"/api/perfiles/{id}/eliminar", new { });
                Push(new { tipo = "perfilEliminado", ok = true });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "perfilEliminado", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "perfilEliminado", ok = false, mensaje = "No se pudo eliminar: " + ex.Message });
            }
        }

        // ------------------------------------------------------------------
        // Usuarios — asignación de perfil
        // ------------------------------------------------------------------

        private void CargarUsuariosWeb()
        {
            try
            {
                var lista = ApiClient.Get<List<UsuarioPerfilApi>>("/api/perfiles/usuarios") ?? new List<UsuarioPerfilApi>();
                var paraJs = lista.Select(u => new
                {
                    nombre = u.Nombre,
                    perfilId = u.PerfilId,
                    perfilNombre = u.PerfilNombre,
                    foto = !string.IsNullOrEmpty(u.FotoBase64)
                        ? $"data:{TemaObra.MimeDeExtension(u.FotoExtension)};base64,{u.FotoBase64}"
                        : null,
                    fechaAlta = u.FechaAlta.ToString("yyyy-MM-dd"),
                    fechaAltaConfirmada = u.FechaAltaConfirmada
                }).ToList();
                Push(new { tipo = "usuarios", lista = paraJs });
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los usuarios"); }
        }

        private void AsignarPerfilWeb(MsgAsignarPerfil d)
        {
            try
            {
                ApiClient.Post("/api/perfiles/usuarios/asignar", new { Usuario = d.usuario, PerfilId = d.perfilId });
                Push(new { tipo = "perfilAsignado", ok = true, usuario = d.usuario });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "perfilAsignado", ok = false, usuario = d.usuario, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "perfilAsignado", ok = false, usuario = d.usuario, mensaje = "No se pudo asignar: " + ex.Message });
            }
        }

        private void CrearUsuarioWeb(MsgCrearUsuario d)
        {
            try
            {
                ApiClient.Post("/api/perfiles/usuarios/crear", new
                {
                    Usuario = d.usuario,
                    Clave = d.clave,
                    PerfilId = d.perfilId,
                    FotoBase64 = d.fotoBase64,
                    Extension = d.extension,
                    FechaAlta = d.fechaAlta
                });
                Push(new { tipo = "usuarioCreado", ok = true, usuario = d.usuario });
                CargarUsuariosWeb();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Push(new { tipo = "usuarioCreado", ok = false, mensaje = "Ya existe un usuario con ese nombre." });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "usuarioCreado", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "usuarioCreado", ok = false, mensaje = "No se pudo crear: " + ex.Message });
            }
        }

        private void SubirFotoUsuarioWeb(MsgFotoUsuario d)
        {
            try
            {
                ApiClient.Post($"/api/perfiles/usuarios/{Uri.EscapeDataString(d.usuario)}/foto",
                    new { FotoBase64 = d.fotoBase64, Extension = d.extension });
                Push(new { tipo = "fotoUsuarioGuardada", ok = true, usuario = d.usuario });
                CargarUsuariosWeb();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "fotoUsuarioGuardada", ok = false, usuario = d.usuario, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "fotoUsuarioGuardada", ok = false, usuario = d.usuario, mensaje = "No se pudo guardar la foto: " + ex.Message });
            }
        }

        private void ConfirmarFechaAltaWeb(MsgFechaAlta d)
        {
            try
            {
                ApiClient.Post($"/api/perfiles/usuarios/{Uri.EscapeDataString(d.usuario)}/fecha-alta",
                    new { FechaAlta = d.fechaAlta });
                Push(new { tipo = "fechaAltaGuardada", ok = true, usuario = d.usuario });
                CargarUsuariosWeb();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "fechaAltaGuardada", ok = false, usuario = d.usuario, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "fechaAltaGuardada", ok = false, usuario = d.usuario, mensaje = "No se pudo guardar: " + ex.Message });
            }
        }

        // ------------------------------------------------------------------
        // Obras — asignación de acceso por usuario
        // ------------------------------------------------------------------

        private void CargarObrasCatalogoWeb()
        {
            try { Push(new { tipo = "obrasCatalogo", lista = ApiClient.Get<List<ObraApi>>("/api/obras/todas") ?? new List<ObraApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las obras"); }
        }

        private void CargarAsignacionesObrasWeb()
        {
            try { Push(new { tipo = "asignacionesObras", lista = ApiClient.Get<List<AsignacionObrasUsuarioApi>>("/api/obras/asignaciones") ?? new List<AsignacionObrasUsuarioApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar las asignaciones de obras"); }
        }

        private void GuardarObrasUsuarioWeb(MsgAsignarObras d)
        {
            try
            {
                ApiClient.Post("/api/obras/usuario/asignar", new { Usuario = d.usuario, ObraIds = d.obraIds ?? new List<int>() });
                Push(new { tipo = "obrasUsuarioGuardado", ok = true, usuario = d.usuario });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "obrasUsuarioGuardado", ok = false, usuario = d.usuario, mensaje = "No se pudo guardar: " + ex.Message });
            }
        }
    }
}
