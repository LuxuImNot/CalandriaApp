using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

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

        /// <summary>
        /// Logging detallado de transacciones: además de método/ruta/estado/tiempo,
        /// vuelca el CUERPO de las peticiones de escritura (POST/PUT/PATCH) y el usuario.
        /// Útil para supervisar en vivo desde la consola. Default true (se puede apagar
        /// con "LogDetallado=false" en la config).
        /// </summary>
        public static bool LogDetallado
        {
            get
            {
                var v = ConfigurationManager.AppSettings["LogDetallado"];
                return !bool.TryParse(v, out bool b) || b; // ausente/!parseable => true
            }
        }

        public static string CadenaConexion =>
            ConfigurationManager.ConnectionStrings["CalandriaConn"]?.ConnectionString;

        /// <summary>BD maestra: Usuarios, Perfiles, PerfilPermisos, Obras, UsuarioObras.</summary>
        public static string CadenaConexionControl =>
            ConfigurationManager.ConnectionStrings["CalandriaControlConn"]?.ConnectionString;

        /// <summary>Cadena de conexión a la BD de una obra, mismo servidor que la BD maestra.</summary>
        public static string CadenaConexionObra(string nombreBd)
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(CadenaConexionControl)
            {
                InitialCatalog = nombreBd
            };
            return builder.ConnectionString;
        }

        /// <summary>
        /// Carpeta con el script de esquema (.sql) que se aplica al crear una obra
        /// nueva. Por omisión "Sql" junto al exe del servicio, igual que UiRuta.
        /// </summary>
        public static string SqlPlantillaObraRuta
        {
            get
            {
                var v = ConfigurationManager.AppSettings["SqlPlantillaObraRuta"];
                if (string.IsNullOrWhiteSpace(v)) v = System.IO.Path.Combine("Sql", "ObraPlantilla.sql");
                return System.IO.Path.IsPathRooted(v)
                    ? v
                    : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, v);
            }
        }

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

        // ---- Emparejador automático para conciliar facturas ----
        /// <summary>
        /// Proveedor del emparejador automático para conceptos de factura vs OC:
        /// "Ollama" (local, gratis, privado), "Gemini" (requiere key) o
        /// vacío/"None" para desactivarlo (solo cotejo determinista). Default "Ollama".
        /// </summary>
        public static string IaProveedor =>
            ConfigurationManager.AppSettings["IaProveedor"] ?? "Ollama";

        /// <summary>URL del servidor Ollama local. Default http://localhost:11434.</summary>
        public static string OllamaUrl =>
            ConfigurationManager.AppSettings["OllamaUrl"] ?? "http://localhost:11434";

        /// <summary>Modelo de Ollama a usar (debe estar descargado con `ollama pull`). Default llama3.1.</summary>
        public static string OllamaModel =>
            ConfigurationManager.AppSettings["OllamaModel"] ?? "llama3.1";

        public static string GeminiApiKey =>
            ConfigurationManager.AppSettings["GeminiApiKey"];

        public static string GeminiModel =>
            ConfigurationManager.AppSettings["GeminiModel"] ?? "gemini-2.0-flash";

        /// <summary>
        /// Clave AES-256 (32 bytes en Base64) para cifrar en reposo los PDFs de
        /// identidad de trabajadores (INE/CURP/RFC/NSS). Genera una nueva con
        /// `openssl rand -base64 32` y ponla en secrets.config -> DocEncryptionKey.
        /// </summary>
        public static string DocEncryptionKey =>
            ConfigurationManager.AppSettings["DocEncryptionKey"]
            ?? throw new InvalidOperationException("Falta DocEncryptionKey en la configuración (secrets.config).");

        /// <summary>
        /// Usuarios (Usuarios.Nombre) con permiso de plataforma para dar de alta clientes
        /// nuevos -- fuera del sistema de Perfiles/Permisos que cada cliente controla, para
        /// que ningún cliente pueda otorgárselo a sí mismo. Lista separada por comas en
        /// secrets.config -> SuperAdmins.
        /// </summary>
        public static HashSet<string> SuperAdmins
        {
            get
            {
                string v = ConfigurationManager.AppSettings["SuperAdmins"] ?? "";
                return new HashSet<string>(
                    v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()),
                    StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Carpeta con la interfaz web que sirve UiController (panel.html). Por
        /// omisión "ui" junto al exe del servicio, de modo que actualizar la UI del
        /// cliente sea copiar un archivo en el servidor. Admite ruta absoluta.
        /// </summary>
        public static string UiRuta
        {
            get
            {
                var v = ConfigurationManager.AppSettings["UiRuta"];
                if (string.IsNullOrWhiteSpace(v)) v = "ui";
                return System.IO.Path.IsPathRooted(v)
                    ? v
                    : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, v);
            }
        }
    }
}
