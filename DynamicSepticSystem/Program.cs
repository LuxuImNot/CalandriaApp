using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;
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

#if DEBUG
            // Al DEPURAR (solo build Debug + depurador conectado) se puede saltar el
            // login con un usuario admin local, para no perder tiempo logueándose en
            // cada sesión de pruebas. Se pregunta cada vez (para poder probar el login
            // real, p. ej. login.html, sin desconectar el depurador). No afecta el
            // .exe de Release.
            if (Debugger.IsAttached &&
                MessageBox.Show("¿Saltar el login? (modo desarrollo)", "Depuración",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Global.UsuarioActual = new PanelPrincipal.Usuario
                {
                    Nombre = "admin",
                    Clave = "",
                    Perfil = "Admin",
                    Permisos = new List<string>
                    {
                        "sistema.administrador", "sistema.perfiles", "sistema.usuarios",
                        "almacen.ver", "almacen.editar", "compras.ver", "compras.editar",
                        "nomina.ver", "nomina.editar", "trabajadores.ver", "trabajadores.editar",
                        "destajos.ver", "destajos.editar", "estimaciones.ver", "estimaciones.editar",
                        "errores.ver"
                    }
                };

                // Token API para las pantallas ya migradas. Con credenciales de
                // desarrollo en App.config (DebugUser / DebugPass) se salta el login
                // del todo. Best-effort: si falla, el usuario admin local de arriba
                // ya cubre permisos (las pantallas migradas quedan en modo local).
                try
                {
                    string du = ConfigurationManager.AppSettings["DebugUser"];
                    string dp = ConfigurationManager.AppSettings["DebugPass"];
                    if (!string.IsNullOrEmpty(du) && !string.IsNullOrEmpty(dp))
                    {
                        ApiClient.Login(du, dp);
                        // Multi-obra: sin selector en el atajo de depuración, se toma
                        // la primera obra a la que el usuario de prueba tenga acceso.
                        var obras = ApiClient.Get<List<ObraApi>>("/api/obras");
                        if (obras != null && obras.Count > 0)
                        {
                            Global.ObraActualId = obras[0].Id;
                            Global.ObraActualNombre = obras[0].Nombre;
                        }
                    }
                }
                catch { /* sin token de API; el modo local ya cubre permisos */ }

                Application.Run(new PanelPrincipal());
                return;
            }
#endif

            // FormLogin hospeda un WebView2: no se abre con ShowDialog() (ver
            // reference_webview2_multi_instance). Se corre como formulario
            // principal y, al loguear con éxito, cede el paso a PanelPrincipal.
            using (var login = new FormLogin())
            {
                login.LoginExitoso += (s, e) =>
                {
                    login.Hide();
                    var seleccion = new FormSeleccionObraWeb();
                    seleccion.ObraSeleccionada += (s2, e2) => seleccion.Close();
                    seleccion.FormClosed += (s2, e2) =>
                    {
                        if (!seleccion.SeSeleccionoObra) { login.Close(); return; }
                        var panel = new PanelPrincipal();
                        panel.FormClosed += (s3, e3) => login.Close();
                        panel.Show();
                    };
                    seleccion.Show();
                };
                Application.Run(login);
            }
        }



    }
}
