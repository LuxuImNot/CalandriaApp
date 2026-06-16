using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using DynamicSepticSystem;

namespace DynamicSepticSystem
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        public static string VersionActual = "1.1";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorLogger.Inicializar();
            ThemeManager.InicializarAplicacion();

            // Skip the splash screen when running under the debugger
            if (!Debugger.IsAttached)
            {
                using (var splash = new FormSplash())
                {
                    if (splash.ShowDialog() != DialogResult.OK)
                        return;
                }
            }

            var _ = Actualizador.VerificarYActualizarAsync().Result;

            using (var login = new FormLogin())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new PanelPrincipal());
                }
            }
        }



    }
}
