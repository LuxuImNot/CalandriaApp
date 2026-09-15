using System.Threading;

namespace Calandria.Api.Data
{
    /// <summary>
    /// Cadena de conexión de la obra activa en la petición actual, fijada por
    /// JwtMessageHandler a partir del header X-Obra-Id. AsyncLocal (no
    /// ThreadStatic) porque el pipeline OWIN es async y el valor debe viajar
    /// con la Task, no con el hilo.
    /// </summary>
    public static class ObraContext
    {
        private static readonly AsyncLocal<string> _cadena = new AsyncLocal<string>();

        public static string CadenaActual
        {
            get => _cadena.Value;
            set => _cadena.Value = value;
        }
    }
}
