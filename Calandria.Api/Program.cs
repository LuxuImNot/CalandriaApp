using System;
using System.ServiceProcess;
using System.Threading;

namespace Calandria.Api
{
    /// <summary>
    /// Punto de entrada. Corre como Servicio de Windows en producción y como
    /// consola cuando se ejecuta manualmente (Environment.UserInteractive), lo
    /// que facilita depurar el host sin instalar el servicio.
    /// </summary>
    internal static class Program
    {
        private static void Main()
        {
            if (Environment.UserInteractive)
            {
                using (var servicio = new CalandriaService())
                {
                    servicio.IniciarConsola();
                    Console.WriteLine("Calandria API en ejecución en " + Configuracion.BaseUrl);

                    if (Console.IsInputRedirected)
                    {
                        // Ejecución detached (sin teclado): mantener vivo hasta Ctrl+C / cierre.
                        var detener = new ManualResetEventSlim(false);
                        Console.CancelKeyPress += (s, e) => { e.Cancel = true; detener.Set(); };
                        Console.WriteLine("Modo detached. Ctrl+C para detener...");
                        detener.Wait();
                    }
                    else
                    {
                        Console.WriteLine("Presiona ENTER para detener...");
                        Console.ReadLine();
                    }

                    servicio.DetenerConsola();
                }
            }
            else
            {
                ServiceBase.Run(new CalandriaService());
            }
        }
    }
}
