// Alta de clientes (empresas) nuevos, hospeda Calandria.Api/ui/clientes.html,
// mismo patrón que FormTerminosWeb.cs / FormTrabajadoresWeb.cs. Solo tiene
// sentido para el operador de la plataforma (api/clientes exige
// RequiereSuperAdmin en el servidor); el menú que abre esta ventana debe
// mostrarse solo si AuthController.MiPerfil().EsSuperAdmin vino en true.
using System;
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
    public class FormClientesWeb : Form
    {
        private WebView2 webClientes;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "clientes.html");

        public FormClientesWeb()
        {
            Text = "Clientes - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1000, 760);
            MinimumSize = new Size(760, 560);
            BackColor = Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webClientes = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webClientes);

            webClientes.CoreWebView2InitializationCompleted += WebClientes_Init;
            _ = IniciarWebView2Async();
        }

        private async Task IniciarWebView2Async()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2Clientes");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webClientes.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("ClientesWeb", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo iniciar el módulo de Clientes:\n\n" + ex.Message,
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
            }
        }

        private void WebClientes_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("ClientesWeb", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar el módulo de Clientes.", "Pilaris",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            var core = webClientes.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebClientes_Mensaje;

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("ClientesWeb", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webClientes?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz web de Clientes.", "Pilaris",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            webClientes.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/clientes/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/clientes");
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
                ErrorLogger.RegistrarMensaje("ClientesWeb", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webClientes?.CoreWebView2 == null) return;
            try { webClientes.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("ClientesWeb", "Fallo al enviar datos a la página: " + ex.Message); }
        }

        private void PushError(string mensaje)
        {
            if (webClientes?.CoreWebView2 == null) return;
            try { webClientes.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(new { tipo = "error", mensaje }, CamelCaseSettings)); }
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
            ErrorLogger.RegistrarMensaje("ClientesWeb", contexto + ": " + msg);
            PushError(contexto + ": " + msg);
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgCrearCliente
        {
            public string nombreCliente, nombreObra, usuario, clave, fechaAlta, fotoBase64, extension;
        }

        private void WebClientes_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
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
                    case "cargar-clientes": CargarClientesWeb(); break;
                    case "crear-cliente":
                        { var d = JsonConvert.DeserializeObject<MsgCrearCliente>(json); CrearClienteWeb(d); }
                        break;
                    case "volver":
                        Close();
                        break;
                    default:
                        ErrorLogger.RegistrarMensaje("ClientesWeb", "Acción desconocida desde la página: " + accion);
                        break;
                }
            }
            catch (Exception ex)
            {
                ManejarErrorApi(ex, "No se pudo completar la acción solicitada");
            }
        }

        private void CargarClientesWeb()
        {
            try { Push(new { tipo = "clientes", lista = ApiClient.Get<System.Collections.Generic.List<ClienteApi>>("/api/clientes") ?? new System.Collections.Generic.List<ClienteApi>() }); }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudieron cargar los clientes"); }
        }

        private void CrearClienteWeb(MsgCrearCliente d)
        {
            try
            {
                DateTime.TryParse(d.fechaAlta, out DateTime fecha);
                ApiClient.Post<object>("/api/clientes", new
                {
                    NombreCliente = d.nombreCliente,
                    NombreObra = d.nombreObra,
                    Usuario = d.usuario,
                    Clave = d.clave,
                    FotoBase64 = d.fotoBase64,
                    Extension = d.extension,
                    FechaAlta = fecha
                });
                Push(new { tipo = "clienteCreado", ok = true });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Push(new { tipo = "clienteCreado", ok = false, mensaje = "Ya existe un cliente, obra o usuario con ese nombre." });
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Push(new { tipo = "clienteCreado", ok = false, mensaje = MensajeDe(ex) });
            }
            catch (Exception ex)
            {
                Push(new { tipo = "clienteCreado", ok = false, mensaje = "No se pudo crear el cliente: " + ex.Message });
            }
        }
    }

    public class ClienteApi
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int CantidadObras { get; set; }
        public int CantidadUsuarios { get; set; }
    }
}
