using System;
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

        // ---- JWT ----
        public static string JwtSecreto =>
            ConfigurationManager.AppSettings["JwtSecreto"]
            ?? throw new InvalidOperationException("Falta JwtSecreto en la configuración.");

        public static string JwtIssuer =>
            ConfigurationManager.AppSettings["JwtIssuer"] ?? "Calandria.Api";

        public static string JwtAudience =>
            ConfigurationManager.AppSettings["JwtAudience"] ?? "CalandriaCliente";

        public static int JwtHorasVigencia
        {
            get
            {
                var v = ConfigurationManager.AppSettings["JwtHorasVigencia"];
                return int.TryParse(v, out int h) && h > 0 ? h : 12;
            }
        }
    }
}
