using System.Collections.Generic;
using System.Linq;

namespace Calandria.Api.Auth
{
    /// <summary>
    /// Catálogo fijo de permisos que un perfil puede tener. Los módulos son los
    /// que ya existen en la app (menú de PanelPrincipal); no es una lista que el
    /// admin edite, solo arma perfiles combinando estas claves.
    /// </summary>
    public sealed class Permiso
    {
        public string Clave { get; set; }
        public string Modulo { get; set; }
        public string Etiqueta { get; set; }
    }

    public static class PermisosCatalogo
    {
        public static readonly List<Permiso> Todos = new List<Permiso>
        {
            new Permiso { Clave = "sistema.administrador", Modulo = "Sistema", Etiqueta = "Administrador del sistema (acceso total)" },
            new Permiso { Clave = "sistema.perfiles",       Modulo = "Sistema", Etiqueta = "Gestionar perfiles y permisos" },
            new Permiso { Clave = "sistema.usuarios",       Modulo = "Sistema", Etiqueta = "Gestionar usuarios" },
            new Permiso { Clave = "sistema.obras",           Modulo = "Sistema", Etiqueta = "Crear y aprovisionar obras" },
            new Permiso { Clave = "sistema.facturacion",     Modulo = "Sistema", Etiqueta = "Ver facturación de add-ons (información del proveedor del sistema)" },

            new Permiso { Clave = "almacen.ver",     Modulo = "Almacén", Etiqueta = "Ver" },
            new Permiso { Clave = "almacen.editar",  Modulo = "Almacén", Etiqueta = "Editar (entradas/salidas)" },

            new Permiso { Clave = "compras.ver",     Modulo = "Compras", Etiqueta = "Ver" },
            new Permiso { Clave = "compras.editar",  Modulo = "Compras", Etiqueta = "Editar (órdenes de compra)" },

            new Permiso { Clave = "nomina.ver",      Modulo = "Nómina", Etiqueta = "Ver" },
            new Permiso { Clave = "nomina.editar",   Modulo = "Nómina", Etiqueta = "Editar" },

            new Permiso { Clave = "trabajadores.ver",    Modulo = "Trabajadores", Etiqueta = "Ver" },
            new Permiso { Clave = "trabajadores.editar", Modulo = "Trabajadores", Etiqueta = "Editar" },

            new Permiso { Clave = "destajos.ver",    Modulo = "Destajos", Etiqueta = "Ver" },
            new Permiso { Clave = "destajos.editar", Modulo = "Destajos", Etiqueta = "Editar (activar tareas)" },

            new Permiso { Clave = "estimaciones.ver",    Modulo = "Estimaciones", Etiqueta = "Ver" },
            new Permiso { Clave = "estimaciones.editar", Modulo = "Estimaciones", Etiqueta = "Editar" },

            new Permiso { Clave = "errores.ver", Modulo = "Sistema", Etiqueta = "Ver registro de errores" },
        };

        private static readonly HashSet<string> _claves =
            new HashSet<string>(Todos.Select(p => p.Clave));

        public static bool Existe(string clave) => clave != null && _claves.Contains(clave);
    }
}
