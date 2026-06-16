using System.Configuration;

namespace Calandria.Api
{
    /// <summary>
    /// Acceso central a la configuración (Calandria.Api.exe.config en el servidor).
    /// La cadena de conexión y los secretos viven SOLO aquí, en el servidor,
    /// nunca en el cliente WinForms.
    /// </summary>
    public static class Configuracion
    {
        public static string BaseUrl =>
            ConfigurationManager.AppSettings["BaseUrl"] ?? "http://localhost:8733";

        public static string CadenaConexion =>
            ConfigurationManager.ConnectionStrings["CalandriaConn"]?.ConnectionString;
    }
}
