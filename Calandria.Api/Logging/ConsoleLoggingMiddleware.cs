using System;
using System.Diagnostics;
using System.IO;
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
    /// No vuelca cuerpos (las subidas de fotos van en base64 y saturarían la salida).
    /// </summary>
    public sealed class ConsoleLoggingMiddleware : OwinMiddleware
    {
        private static readonly object _candado = new object();
        private static readonly string _dirLogs =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        public ConsoleLoggingMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var req = context.Request;
            string linea = $"{req.Method} {req.Uri.PathAndQuery}";

            Escribir(ConsoleColor.Cyan, $"{Hora()} >> {linea}");

            var sw = Stopwatch.StartNew();
            try
            {
                await Next.Invoke(context);
                sw.Stop();

                int status = context.Response.StatusCode;
                Escribir(ColorEstado(status), $"{Hora()} << {status} {linea} ({sw.ElapsedMilliseconds} ms)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Escribir(ConsoleColor.Red,
                    $"{Hora()} !! {linea} EX {ex.GetType().Name}: {ex.Message} ({sw.ElapsedMilliseconds} ms)");
                throw;
            }
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
