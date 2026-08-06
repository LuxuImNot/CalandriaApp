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

        /// <summary>Obra activa en esta sesión (se envía como header X-Obra-Id en cada llamada al API).</summary>
        public static int? ObraActualId { get; set; }

        /// <summary>Nombre de la obra activa, solo para mostrarlo en la UI.</summary>
        public static string ObraActualNombre { get; set; }

        /// <summary>
        /// True si el perfil del usuario en sesión trae el permiso
        /// "sistema.administrador". Punto único de verificación: todo lo que antes
        /// comparaba Nombre == "admin" debe usar esto en su lugar.
        /// </summary>
        public static bool EsAdmin =>
            UsuarioActual != null && UsuarioActual.TienePermiso("sistema.administrador");
    }

}
