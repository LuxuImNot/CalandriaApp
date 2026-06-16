using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DynamicSepticSystem.PanelPrincipal;

namespace DynamicSepticSystem
{
    public static class Global
    {
        public static Usuario UsuarioActual { get; set; }

        /// <summary>
        /// True si el usuario en sesión es el administrador (usuario "admin").
        /// Mismo criterio que el resto de la app.
        /// </summary>
        public static bool EsAdmin =>
            UsuarioActual != null
            && !string.IsNullOrEmpty(UsuarioActual.Nombre)
            && string.Equals(UsuarioActual.Nombre, "admin", StringComparison.OrdinalIgnoreCase);
    }

}
