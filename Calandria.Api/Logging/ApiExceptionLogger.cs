using System;
using System.IO;
using System.Text;
using System.Web.Http.ExceptionHandling;

namespace Calandria.Api.Logging
{
    /// <summary>
    /// Logger global de Web API: cuando una acción lanza una excepción no
    /// controlada (que el framework convierte en HTTP 500), aquí se vuelca el
    /// detalle completo (tipo, mensaje y stack, incluidas las inner exceptions) a
    /// la consola y al archivo "logs\api-YYYYMMDD.log". Imprescindible para ver la
    /// causa real de un 500, que el middleware de peticiones no alcanza a capturar.
    /// </summary>
    public sealed class ApiExceptionLogger : ExceptionLogger
    {
        private static readonly object _candado = new object();
        private static readonly string _dirLogs =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        public override void Log(ExceptionLoggerContext context)
        {
            var req = context.Request;
            string encabezado =
                $"{DateTime.Now:HH:mm:ss} !! EXCEPCION {req?.Method} {req?.RequestUri?.PathAndQuery}";
            string detalle = Describir(context.Exception);

            EscribirConsola(encabezado, detalle);
            EscribirArchivo(encabezado + Environment.NewLine + detalle);
        }

        private static string Describir(Exception ex)
        {
            var sb = new StringBuilder();
            int nivel = 0;
            while (ex != null)
            {
                string prefijo = nivel == 0 ? "" : new string(' ', nivel * 2) + "---> ";
                sb.AppendLine($"{prefijo}{ex.GetType().FullName}: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                    sb.AppendLine(ex.StackTrace);
                ex = ex.InnerException;
                nivel++;
            }
            return sb.ToString().TrimEnd();
        }

        private static void EscribirConsola(string encabezado, string detalle)
        {
            ConsoleColor previo = ConsoleColor.Gray;
            bool color = Environment.UserInteractive;
            if (color)
            {
                previo = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
            }
            try
            {
                Console.WriteLine(encabezado);
                Console.WriteLine(detalle);
            }
            finally
            {
                if (color) Console.ForegroundColor = previo;
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
                    File.AppendAllText(archivo, texto + Environment.NewLine);
                }
            }
            catch
            {
                // El logging nunca debe tumbar una petición.
            }
        }
    }
}
