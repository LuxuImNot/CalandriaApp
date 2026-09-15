using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Calandria.Api.Logging
{
    /// <summary>
    /// Middleware OWIN que registra cada petición entrante (">>") y su respuesta
    /// saliente ("&lt;&lt;") con método, ruta, código de estado y tiempo.
    ///
    /// Escribe a DOS destinos: a la consola (con color, útil al correr el host en
    /// modo consola) y a un archivo diario "logs\api-YYYYMMDD.log" junto al exe
    /// (imprescindible como Servicio de Windows, donde no hay consola adjunta).
    ///
    /// Con LogDetallado (config, default true) también vuelca el USUARIO autenticado
    /// y el CUERPO de las peticiones de escritura (POST/PUT/PATCH), con tope de tamaño
    /// y una lista de exclusión para NO volcar contraseñas ni cargas base64 (login,
    /// evidencias, fotos, conciliar-factura). Así se supervisan las transacciones en vivo.
    /// </summary>
    public sealed class ConsoleLoggingMiddleware : OwinMiddleware
    {
        private static readonly object _candado = new object();
        private static readonly string _dirLogs =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        // Tope del cuerpo a registrar (evita saturar la consola/archivo).
        private const int MaxCuerpo = 4000;

        // Rutas cuyo cuerpo NO se registra (secretos o cargas grandes en base64).
        private static readonly string[] _rutasSinCuerpo =
        {
            "auth/login", "evidencias", "fotos", "conciliar-factura", "trabajadores"
        };

        public ConsoleLoggingMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var req = context.Request;
            string linea = $"{req.Method} {req.Uri.PathAndQuery}";

            bool detallado = Configuracion.LogDetallado;
            string usuario = detallado ? UsuarioDe(context) : null;
            string sufijoUsuario = string.IsNullOrEmpty(usuario) ? "" : $" (user: {usuario})";

            Escribir(ConsoleColor.Cyan, $"{Hora()} >> {linea}{sufijoUsuario}");

            // Vuelca el cuerpo de las transacciones de escritura (sin secretos/base64).
            if (detallado && EsEscritura(req.Method) && !RutaExcluida(req.Uri.AbsolutePath))
            {
                string cuerpo = await LeerCuerpoRequest(context);
                if (!string.IsNullOrWhiteSpace(cuerpo))
                    Escribir(ConsoleColor.DarkGray, $"{Hora()}    body: {Resumir(cuerpo)}");
            }

            var sw = Stopwatch.StartNew();
            try
            {
                await Next.Invoke(context);
                sw.Stop();

                int status = context.Response.StatusCode;
                Escribir(ColorEstado(status), $"{Hora()} << {status} {linea} ({sw.ElapsedMilliseconds} ms){sufijoUsuario}");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Escribir(ConsoleColor.Red,
                    $"{Hora()} !! {linea} EX {ex.GetType().Name}: {ex.Message} ({sw.ElapsedMilliseconds} ms){sufijoUsuario}");
                throw;
            }
        }

        private static bool EsEscritura(string metodo) =>
            metodo == "POST" || metodo == "PUT" || metodo == "PATCH" || metodo == "DELETE";

        private static bool RutaExcluida(string ruta)
        {
            string r = (ruta ?? "").ToLowerInvariant();
            foreach (var x in _rutasSinCuerpo)
                if (r.Contains(x)) return true;
            return false;
        }

        private static string UsuarioDe(IOwinContext context)
        {
            try { return context.Authentication?.User?.Identity?.Name; }
            catch { return null; }
        }

        /// <summary>
        /// Lee el cuerpo de la petición y lo deja re-leíble para el resto del pipeline
        /// (reemplaza el stream por uno en memoria posicionado al inicio).
        /// </summary>
        private static async Task<string> LeerCuerpoRequest(IOwinContext context)
        {
            try
            {
                var original = context.Request.Body;
                if (original == null) return "";

                var ms = new MemoryStream();
                await original.CopyToAsync(ms);
                ms.Position = 0;

                string texto;
                using (var reader = new StreamReader(ms, Encoding.UTF8, false, 1024, leaveOpen: true))
                    texto = await reader.ReadToEndAsync();

                ms.Position = 0;
                context.Request.Body = ms; // downstream vuelve a leer desde el inicio
                return texto;
            }
            catch
            {
                return ""; // el logging nunca debe afectar la petición
            }
        }

        /// <summary>Una sola línea, recortado al tope.</summary>
        private static string Resumir(string cuerpo)
        {
            string s = cuerpo.Replace("\r", " ").Replace("\n", " ").Trim();
            if (s.Length > MaxCuerpo) s = s.Substring(0, MaxCuerpo) + "…[+" + (cuerpo.Length - MaxCuerpo) + " chars]";
            return s;
        }

        private static string Hora() => DateTime.Now.ToString("HH:mm:ss");

        private static ConsoleColor ColorEstado(int status)
        {
            if (status >= 500) return ConsoleColor.Red;
            if (status >= 400) return ConsoleColor.Yellow;
            return ConsoleColor.Green;
        }

        private static void Escribir(ConsoleColor color, string texto)
        {
            EscribirConsola(color, texto);
            EscribirArchivo(texto);
        }

        private static void EscribirConsola(ConsoleColor color, string texto)
        {
            // El color solo tiene sentido (y solo es seguro) con consola adjunta.
            if (!Environment.UserInteractive)
            {
                Console.WriteLine(texto);
                return;
            }

            var previo = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(texto);
            }
            finally
            {
                Console.ForegroundColor = previo;
            }
        }

        private static void EscribirArchivo(string texto)
        {
            try
            {
                lock (_candado)
                {
                    Directory.CreateDirectory(_dirLogs);
                    string archivo = Path.Combine(_dirLogs, $"api-{DateTime.Now:yyyyMMdd}.log");
                    File.AppendAllText(archivo, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {texto}{Environment.NewLine}");
                }
            }
            catch
            {
                // El logging nunca debe tumbar una petición.
            }
        }
    }
}
