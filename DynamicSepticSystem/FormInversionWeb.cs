// Inversión general de la obra, hospeda Calandria.Api/ui/inversion.html.
// Mismo patrón que FormClientesWeb.cs: la página llega del API (con caché por
// hash) y se inyecta con NavigateToString; los datos van y vienen por mensajes.
using System;
using System.Collections.Generic;
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
    public class FormInversionWeb : Form
    {
        private WebView2 webInversion;

        private static readonly JsonSerializerSettings CamelCaseSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static string CacheDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calandria", "ui");

        private static string CacheHtml => Path.Combine(CacheDir, "inversion.html");

        public FormInversionWeb()
        {
            Text = "Inversión general - Sistema Pilaris";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1160, 800);
            MinimumSize = new Size(820, 560);
            BackColor = Color.White;
            ThemeManager.AplicarIconoPorDefecto(this);

            webInversion = new WebView2 { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(webInversion);

            webInversion.CoreWebView2InitializationCompleted += WebInversion_Init;
            _ = IniciarWebView2Async();
        }

        private async Task IniciarWebView2Async()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Calandria", "WebView2Inversion");
                var entorno = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                await webInversion.EnsureCoreWebView2Async(entorno);
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("InversionWeb", "WebView2 no pudo iniciar: " + ex.Message);
                MessageBox.Show("No se pudo iniciar el módulo de Inversión:\n\n" + ex.Message,
                    "Pilaris", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
            }
        }

        private void WebInversion_Init(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                ErrorLogger.RegistrarMensaje("InversionWeb", "WebView2 no inicializó: " + e.InitializationException);
                MessageBox.Show("No se pudo iniciar el módulo de Inversión.", "Pilaris",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
                return;
            }

            var core = webInversion.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = Debugger.IsAttached;
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.Settings.IsStatusBarEnabled = false;

            core.WebMessageReceived += WebInversion_Mensaje;

            _ = CargarHtmlAsync();
        }

        private async Task CargarHtmlAsync()
        {
            string html = null;
            try { html = await Task.Run(() => ObtenerHtml()); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("InversionWeb", "Fallo cargando la página: " + ex.Message); }

            if (IsDisposed || webInversion?.CoreWebView2 == null) return;
            if (html == null)
            {
                MessageBox.Show("No se pudo descargar la interfaz web de Inversión.", "Pilaris",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            webInversion.CoreWebView2.NavigateToString(html);
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
                var ver = ApiClient.Get<VersionUi>("/api/ui/inversion/version");
                if (cache != null && ver != null && ver.hash == Sha256(Encoding.UTF8.GetBytes(cache)))
                    return cache;

                var datos = ApiClient.GetBytes("/api/ui/inversion");
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
                ErrorLogger.RegistrarMensaje("InversionWeb", "No se pudo traer la UI del API: " + ex.Message);
            }
            return cache;
        }

        // ------------------------------------------------------------------
        // Puente C# -> JS
        // ------------------------------------------------------------------

        private void Push(object payload)
        {
            if (webInversion?.CoreWebView2 == null) return;
            try { webInversion.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(payload, CamelCaseSettings)); }
            catch (Exception ex) { ErrorLogger.RegistrarMensaje("InversionWeb", "Fallo al enviar datos a la página: " + ex.Message); }
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

        private void PushError(Exception ex, string contexto)
        {
            var apiEx = ex as ApiException;
            string msg = apiEx != null ? MensajeDe(apiEx) : ex.Message;
            ErrorLogger.RegistrarMensaje("InversionWeb", contexto + ": " + msg);
            Push(new { tipo = "error", mensaje = contexto + ": " + msg });
        }

        // ------------------------------------------------------------------
        // Puente JS -> C#
        // ------------------------------------------------------------------

        private class MsgAccion { public string accion; }
        private class MsgGuardar
        {
            public string categoria;
            public decimal importe, aplicado;
        }
        private class MsgCategoria { public string categoria; }
        private class MsgRegistrarAccion
        {
            public string categoria, concepto;
            public decimal cantidad;
        }
        private class MsgEditarMovimiento
        {
            public string categoria, concepto;
            public decimal cantidad;
            public int id;
        }
        private class MsgEliminarMovimiento
        {
            public string categoria;
            public int id;
        }

        /// <summary>Los controles de editar/borrar del historial son sólo para la credencial "admin".</summary>
        private static bool EsAdmin =>
            string.Equals(Global.UsuarioActual?.Nombre, "admin", StringComparison.OrdinalIgnoreCase);

        private void WebInversion_Mensaje(object sender, CoreWebView2WebMessageReceivedEventArgs e)
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
            switch (accion)
            {
                case "cargar-inversion": CargarInversion(); break;
                case "guardar-inversion": GuardarInversion(JsonConvert.DeserializeObject<MsgGuardar>(json)); break;
                case "historial-inversion": CargarHistorial(JsonConvert.DeserializeObject<MsgCategoria>(json)); break;
                case "registrar-accion": RegistrarAccion(JsonConvert.DeserializeObject<MsgRegistrarAccion>(json)); break;
                case "editar-movimiento": EditarMovimiento(JsonConvert.DeserializeObject<MsgEditarMovimiento>(json)); break;
                case "eliminar-movimiento": EliminarMovimiento(JsonConvert.DeserializeObject<MsgEliminarMovimiento>(json)); break;
                case "volver": Close(); break;
                default:
                    ErrorLogger.RegistrarMensaje("InversionWeb", "Acción desconocida desde la página: " + accion);
                    break;
            }
        }

        private void CargarInversion()
        {
            try
            {
                Push(new
                {
                    tipo = "inversion",
                    esAdmin = EsAdmin,
                    lista = ApiClient.Get<List<RenglonInversionApi>>("/api/inversion") ?? new List<RenglonInversionApi>()
                });
            }
            catch (Exception ex) { PushError(ex, "No se pudo cargar la inversión"); }
        }

        private void GuardarInversion(MsgGuardar d)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.categoria)) return;
            try
            {
                ApiClient.Post<object>("/api/inversion/guardar", new
                {
                    Categoria = d.categoria,
                    Importe = d.importe,
                    Aplicado = d.aplicado
                });
                Push(new { tipo = "guardado", ok = true, categoria = d.categoria });
            }
            catch (Exception ex)
            {
                var apiEx = ex as ApiException;
                string msg = apiEx != null ? MensajeDe(apiEx) : ex.Message;
                ErrorLogger.RegistrarMensaje("InversionWeb", "No se pudo guardar " + d.categoria + ": " + msg);
                Push(new { tipo = "guardado", ok = false, categoria = d.categoria, mensaje = "No se pudo guardar " + d.categoria + ": " + msg });
            }
        }

        private void CargarHistorial(MsgCategoria d)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.categoria)) return;
            try
            {
                var lista = ApiClient.Get<List<MovimientoInversionApi>>(
                    "/api/inversion/historial?categoria=" + Uri.EscapeDataString(d.categoria));
                Push(new { tipo = "historial", categoria = d.categoria, lista = lista ?? new List<MovimientoInversionApi>() });
            }
            catch (Exception ex) { PushError(ex, "No se pudo cargar el historial de " + d.categoria); }
        }

        private class RespuestaAccion
        {
            public bool Ok { get; set; }
            public string Categoria { get; set; }
            public decimal Aplicado { get; set; }
        }

        private void RegistrarAccion(MsgRegistrarAccion d)
        {
            if (d == null || string.IsNullOrWhiteSpace(d.categoria)) return;
            try
            {
                var r = ApiClient.Post<RespuestaAccion>("/api/inversion/accion", new
                {
                    Categoria = d.categoria,
                    Concepto = d.concepto,
                    Cantidad = d.cantidad
                });
                Push(new { tipo = "accionRegistrada", ok = true, categoria = d.categoria, aplicado = r != null ? r.Aplicado : 0m });
            }
            catch (Exception ex)
            {
                var apiEx = ex as ApiException;
                string msg = apiEx != null ? MensajeDe(apiEx) : ex.Message;
                ErrorLogger.RegistrarMensaje("InversionWeb", "No se pudo registrar la acción en " + d.categoria + ": " + msg);
                Push(new { tipo = "accionRegistrada", ok = false, categoria = d.categoria, mensaje = msg });
            }
        }

        private void EditarMovimiento(MsgEditarMovimiento d)
        {
            if (d == null || d.id <= 0) return;
            EnviarMovimiento(d.categoria, "/api/inversion/movimiento/editar",
                new { Id = d.id, Concepto = d.concepto, Cantidad = d.cantidad },
                "No se pudo editar el movimiento");
        }

        private void EliminarMovimiento(MsgEliminarMovimiento d)
        {
            if (d == null || d.id <= 0) return;
            EnviarMovimiento(d.categoria, "/api/inversion/movimiento/eliminar",
                new { Id = d.id }, "No se pudo borrar el movimiento");
        }

        private void EnviarMovimiento(string categoria, string ruta, object cuerpo, string contexto)
        {
            if (!EsAdmin)
            {
                Push(new { tipo = "movimiento", ok = false, categoria, mensaje = "Sólo el usuario admin puede editar o borrar movimientos." });
                return;
            }
            try
            {
                var r = ApiClient.Post<RespuestaAccion>(ruta, cuerpo);
                Push(new { tipo = "movimiento", ok = true, categoria, aplicado = r != null ? r.Aplicado : 0m });
            }
            catch (Exception ex)
            {
                var apiEx = ex as ApiException;
                string msg = apiEx != null ? MensajeDe(apiEx) : ex.Message;
                ErrorLogger.RegistrarMensaje("InversionWeb", contexto + " de " + categoria + ": " + msg);
                Push(new { tipo = "movimiento", ok = false, categoria, mensaje = contexto + ": " + msg });
            }
        }
    }

    public class MovimientoInversionApi
    {
        public int Id { get; set; }
        public string Categoria { get; set; }
        public string Tipo { get; set; }
        public string Concepto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal? Anterior { get; set; }
        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class RenglonInversionApi
    {
        public string Categoria { get; set; }
        public int Orden { get; set; }
        public decimal Importe { get; set; }
        public decimal Aplicado { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string UsuarioModificacion { get; set; }
    }
}
