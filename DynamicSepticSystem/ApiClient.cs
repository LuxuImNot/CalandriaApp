using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Cliente HTTP hacia Calandria.Api. Centraliza la URL base, el token JWT
    /// obtenido al iniciar sesión y la (de)serialización JSON.
    ///
    /// Durante la migración la app es híbrida: las pantallas ya migradas usan
    /// este cliente; las demás siguen pegándole directo a SQL. Si el API no está
    /// disponible, <see cref="Login"/> falla de forma controlada y solo las
    /// pantallas migradas se ven afectadas.
    /// </summary>
    public static class ApiClient
    {
        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static string BaseUrl =>
            (ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:8733")
            .TrimEnd('/');

        /// <summary>Token JWT de la sesión actual (vacío si no se ha autenticado por API).</summary>
        public static string Token { get; private set; }

        public static bool Autenticado => !string.IsNullOrEmpty(Token);

        /// <summary>
        /// Autentica contra /api/auth/login y guarda el token. Lanza excepción si
        /// las credenciales son inválidas o el API no responde.
        /// </summary>
        public static LoginResponseApi Login(string usuario, string clave)
        {
            string body = JsonConvert.SerializeObject(new { usuario, clave });
            using (var content = new StringContent(body, Encoding.UTF8, "application/json"))
            using (var resp = Http.PostAsync(BaseUrl + "/api/auth/login", content)
                                  .GetAwaiter().GetResult())
            {
                resp.EnsureSuccessStatusCode();
                string json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var login = JsonConvert.DeserializeObject<LoginResponseApi>(json);
                Token = login?.Token;
                return login;
            }
        }

        public static void CerrarSesion() => Token = null;

        /// <summary>GET tipado a una ruta relativa (p. ej. "/api/almacen/manzanas").</summary>
        public static T Get<T>(string rutaRelativa)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + rutaRelativa))
            {
                if (Autenticado)
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                using (var resp = Http.SendAsync(req).GetAwaiter().GetResult())
                {
                    resp.EnsureSuccessStatusCode();
                    string json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
        }

        /// <summary>
        /// GET de contenido binario (p. ej. el PDF de un recibo). Devuelve null
        /// si el API responde 404 (sin contenido).
        /// </summary>
        public static byte[] GetBytes(string rutaRelativa)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + rutaRelativa))
            {
                if (Autenticado)
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                using (var resp = Http.SendAsync(req).GetAwaiter().GetResult())
                {
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                        return null;
                    resp.EnsureSuccessStatusCode();
                    return resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                }
            }
        }

        /// <summary>
        /// POST de un cuerpo JSON a una ruta relativa, sin esperar contenido de
        /// respuesta tipado (p. ej. guardar una asignación). Lanza si el API
        /// responde con error.
        /// </summary>
        public static void Post(string rutaRelativa, object cuerpo)
        {
            EnviarPost(rutaRelativa, cuerpo);
        }

        /// <summary>POST de un cuerpo JSON que devuelve un resultado tipado.</summary>
        public static T Post<T>(string rutaRelativa, object cuerpo)
        {
            string json = EnviarPost(rutaRelativa, cuerpo);
            return string.IsNullOrWhiteSpace(json)
                ? default(T)
                : JsonConvert.DeserializeObject<T>(json);
        }

        private static string EnviarPost(string rutaRelativa, object cuerpo)
        {
            string body = JsonConvert.SerializeObject(cuerpo);
            using (var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl + rutaRelativa))
            {
                if (Autenticado)
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                req.Content = new StringContent(body, Encoding.UTF8, "application/json");

                using (var resp = Http.SendAsync(req).GetAwaiter().GetResult())
                {
                    resp.EnsureSuccessStatusCode();
                    return resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
        }
    }

    // ---- Modelos de transporte (coinciden con los DTO del API) ----

    public sealed class LoginResponseApi
    {
        public string Token { get; set; }
        public string Usuario { get; set; }
        public string Rol { get; set; }
        public List<string> Permisos { get; set; }
        public DateTime ExpiraUtc { get; set; }
    }

    public sealed class MaterialCasaApi
    {
        public string Ruta { get; set; }
        public string Destajo { get; set; }
        public string Material { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string Cuadrilla { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string Prototipo { get; set; }
    }

    // ---- Nómina ----

    public sealed class MontoTrabajadorApi
    {
        public int? IdTrabajador { get; set; }
        public string NombreTrabajador { get; set; }
        public decimal Monto { get; set; }
    }

    public sealed class AsignacionNominaApi
    {
        public string CodigoCuadrilla { get; set; }
        public List<MontoTrabajadorApi> Montos { get; set; } = new List<MontoTrabajadorApi>();
    }

    public sealed class LineaAsignacionNominaApi
    {
        public int? IdTrabajador { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public bool EsJefe { get; set; }
        public decimal Monto { get; set; }
    }

    public sealed class GuardarAsignacionRequestApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public int NodoId { get; set; }
        public string NombreTarea { get; set; }
        public string CodigoCuadrilla { get; set; }
        public decimal TotalDistribuir { get; set; }
        public List<LineaAsignacionNominaApi> Lineas { get; set; } = new List<LineaAsignacionNominaApi>();
    }
}
