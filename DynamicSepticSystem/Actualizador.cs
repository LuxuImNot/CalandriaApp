using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public static class Actualizador
    {
        // Versión del propio ejecutable (AssemblyFileVersion de Properties\AssemblyInfo.cs,
        // la que el release script bumpea antes de compilar). Ya no depende de un
        // version.txt suelto que se podía desincronizar del binario real.
        public static string VersionLocal
        {
            get
            {
                try
                {
                    var ruta = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var version = FileVersionInfo.GetVersionInfo(ruta).FileVersion;
                    if (!string.IsNullOrWhiteSpace(version)) return version;
                }
                catch (Exception ex)
                {
                    Log($"Error leyendo la versión del ensamblado: {ex.Message}");
                }
                return "0.0.0.0";
            }
        }

        // P/Invoke para programar movimientos en reinicio (fallback)
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);
        private const int MOVEFILE_REPLACE_EXISTING = 0x1;
        private const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x4;

        // --------------------------------------------------
        // Comparación de versiones personalizada
        // Ignora cualquier texto después de '-'
        public static bool EsNuevaVersion(string local, string remota)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(remota))
                    return false;

                string Clean(string v)
                {
                    var idx = v.IndexOf('-');
                    var baseV = idx >= 0 ? v.Substring(0, idx) : v;
                    return baseV.Trim();
                }

                var l = Clean(local);
                var r = Clean(remota);

                var partsL = l.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                var partsR = r.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                var max = Math.Max(partsL.Length, partsR.Length);
                for (int i = 0; i < max; i++)
                {
                    int a = 0, b = 0;
                    if (i < partsL.Length) int.TryParse(partsL[i], out a);
                    if (i < partsR.Length) int.TryParse(partsR[i], out b);
                    if (b > a) return true; // remoto mayor
                    if (b < a) return false; // local mayor
                }
                return false; // iguales
            }
            catch
            {
                return false;
            }
        }

        // --------------------------------------------------
        // Obtener información de la release más reciente desde la API de GitHub
        // (internal: FormLogin la reutiliza para mostrar la versión disponible).
        internal static async Task<(string Tag, string VersionText, string ZipUrl)> GetLatestReleaseInfoAsync()
        {
            var owner = ConfigurationManager.AppSettings["GitHubOwner"] ?? "LuxuImNot";
            var repo = ConfigurationManager.AppSettings["GitHubRepo"] ?? "CalandriaApp";
            var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            var token = ConfigurationManager.AppSettings["GitHubToken"];

            Log($"Consultando API de GitHub: {apiUrl}");

            using (var http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromSeconds(15);
                http.DefaultRequestHeaders.UserAgent.ParseAdd("CalandriaUpdater/1.0");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    try { http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", token); } catch { }
                }

                string json;
                try
                {
                    json = await http.GetStringAsync(apiUrl);
                }
                catch (Exception ex)
                {
                    // Sin internet, repo privado/inexistente, sin releases, etc. Esto es
                    // esperado en operación normal (p. ej. sin conexión): no es un error
                    // que deba interrumpir al usuario, solo se registra.
                    Log("Error obteniendo release latest desde API: " + ex.Message);
                    return (null, null, null);
                }

                var tagMatch = Regex.Match(json, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
                var tag = tagMatch.Success ? tagMatch.Groups[1].Value : null;

                // El tag ES la versión (ej. "v1.9.5.0"); se le quita la "v" para que
                // se pueda comparar tal cual contra VersionLocal ("1.9.5.0").
                string versionText = tag != null && tag.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                    ? tag.Substring(1)
                    : tag;

                var zipMatch = Regex.Match(json, "\"browser_download_url\"\\s*:\\s*\"([^\"]+\\.zip)\"", RegexOptions.IgnoreCase);
                string zipUrl = null;
                if (zipMatch.Success)
                    zipUrl = zipMatch.Groups[1].Value.Replace("\\/", "/");

                if (string.IsNullOrWhiteSpace(zipUrl) && !string.IsNullOrWhiteSpace(tag))
                    zipUrl = $"https://codeload.github.com/{owner}/{repo}/zip/refs/tags/{tag}";

                return (tag, versionText, zipUrl);
            }
        }

        // --------------------------------------------------
        // Resultado de un chequeo silencioso de actualización.
        public sealed class ResultadoChequeo
        {
            public bool HayActualizacion;
            public string VersionLocal;
            public string VersionRemota;
            public string ZipUrl;
        }

        /// <summary>
        /// Consulta la release más reciente y compara contra la versión local.
        /// Nunca lanza ni muestra UI: pensado para correr en segundo plano en cada
        /// arranque sin interrumpir al usuario (sin internet simplemente no hay
        /// actualización detectada).
        /// </summary>
        public static async Task<ResultadoChequeo> ComprobarAsync()
        {
            var local = VersionLocal;
            try
            {
                var latest = await GetLatestReleaseInfoAsync();
                bool hay = !string.IsNullOrWhiteSpace(latest.VersionText) && EsNuevaVersion(local, latest.VersionText);
                return new ResultadoChequeo
                {
                    HayActualizacion = hay,
                    VersionLocal = local,
                    VersionRemota = latest.VersionText,
                    ZipUrl = latest.ZipUrl
                };
            }
            catch (Exception ex)
            {
                Log("ComprobarAsync error: " + ex.Message);
                return new ResultadoChequeo { HayActualizacion = false, VersionLocal = local };
            }
        }

        // --------------------------------------------------
        // Progreso reportado durante la descarga/aplicación (para que el llamador
        // lo muestre en su propia UI, p. ej. FormActualizacionWeb).
        public sealed class ProgresoActualizacion
        {
            public int Porcentaje;
            public string Estado;
        }

        /// <summary>Error esperado del proceso de actualización, con mensaje listo para mostrar al usuario.</summary>
        public sealed class ActualizacionException : Exception
        {
            /// <summary>True si el usuario canceló (p. ej. el UAC), no es una falla real.</summary>
            public bool Cancelado { get; }
            public ActualizacionException(string mensaje, bool cancelado = false) : base(mensaje) => Cancelado = cancelado;
        }

        /// <summary>
        /// Descarga el paquete de la versión indicada, verifica su integridad/firma
        /// y lanza Updater.exe elevado para aplicarlo. Si tiene éxito, termina el
        /// proceso actual (Environment.Exit) para que Updater.exe pueda reemplazar
        /// los archivos en uso; por eso un "return" normal solo ocurre si el
        /// instalador no pudo iniciarse pero la actualización quedó preparada.
        /// Lanza <see cref="ActualizacionException"/> con mensaje en español ante
        /// cualquier fallo o cancelación; el llamador decide cómo mostrarlo.
        /// </summary>
        public static async Task<bool> DescargarYAplicarAsync(string zipUrl, string versionRemota, IProgress<ProgresoActualizacion> progreso)
        {
            Log("========================================");
            Log("INICIO DE PROCESO DE ACTUALIZACIÓN");
            Log($"Versión local actual: {VersionLocal} -> remota: {versionRemota}");
            Log("========================================");

            if (string.IsNullOrWhiteSpace(zipUrl))
                throw new ActualizacionException("No se encontró un paquete ZIP para la release más reciente.");

            // Seguridad: el paquete solo puede descargarse por HTTPS desde hosts de
            // confianza (GitHub). Evita que un UpdateUrl manipulado en el config
            // redirija la descarga a un servidor arbitrario.
            var updateUrl = ConfigurationManager.AppSettings["UpdateUrl"] ?? zipUrl;
            if (!HostPermitido(updateUrl))
            {
                Log($"URL de descarga RECHAZADA (host no permitido o no HTTPS): {updateUrl}");
                throw new ActualizacionException("La URL de actualización no está permitida por seguridad. La operación se canceló.");
            }

            // Paso 1: Descargar ZIP con progreso
            progreso?.Report(new ProgresoActualizacion { Porcentaje = 0, Estado = "Preparando descarga..." });
            var tempZip = Path.Combine(Path.GetTempPath(), "CalandriaUpdate_" + Guid.NewGuid().ToString("N") + ".zip");

            try
            {
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromMinutes(10);
                    using (var resp = await http.GetAsync(updateUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        resp.EnsureSuccessStatusCode();
                        var total = resp.Content.Headers.ContentLength.GetValueOrDefault(-1L);

                        using (var contentStream = await resp.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            var buffer = new byte[81920];
                            long totalRead = 0L;
                            int read;
                            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                int percent = total > 0 ? (int)Math.Round((totalRead * 100.0) / total) : 0;
                                progreso?.Report(new ProgresoActualizacion { Porcentaje = Math.Min(100, Math.Max(0, percent)), Estado = "Descargando actualización..." });
                            }
                        }
                    }
                }
                Log($"Descarga completada: {new FileInfo(tempZip).Length / 1024.0 / 1024.0:F2} MB");
            }
            catch (Exception ex)
            {
                Log("Error descargar ZIP: " + ex);
                throw new ActualizacionException("Error al descargar el paquete de actualización: " + ex.Message);
            }

            // Paso 2: Verificar INTEGRIDAD del paquete (SHA-256) antes de extraer o
            // ejecutar nada. El hash esperado se publica junto al ZIP como
            // "<zip>.sha256" (mismo canal HTTPS de GitHub). Si no hay hash:
            //   - RequireUpdateHash=true  -> se aborta (modo estricto).
            //   - RequireUpdateHash=false -> se continúa con advertencia (compat).
            progreso?.Report(new ProgresoActualizacion { Porcentaje = 100, Estado = "Verificando integridad del paquete..." });
            string hashEsperado = await DescargarHashEsperadoAsync(updateUrl);
            bool requiereHash = string.Equals(
                (ConfigurationManager.AppSettings["RequireUpdateHash"] ?? "false").Trim(),
                "true", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(hashEsperado))
            {
                if (!VerificarSha256(tempZip, hashEsperado))
                {
                    try { File.Delete(tempZip); } catch { }
                    Log("ABORTADO: el hash SHA-256 del paquete no coincide.");
                    throw new ActualizacionException("El paquete de actualización no pasó la verificación de integridad (SHA-256). No se aplicará la actualización.");
                }
            }
            else if (requiereHash)
            {
                try { File.Delete(tempZip); } catch { }
                Log("ABORTADO: RequireUpdateHash=true pero no hay '.sha256' publicado para el paquete.");
                throw new ActualizacionException("No se encontró el hash de verificación del paquete y la política de seguridad exige verificarlo. Actualización cancelada.");
            }
            else
            {
                Log("ADVERTENCIA: el paquete no tiene '.sha256' publicado; se continúa (RequireUpdateHash=false).");
            }

            // Paso 3: extraer ZIP en carpeta temporal
            progreso?.Report(new ProgresoActualizacion { Porcentaje = 100, Estado = "Extrayendo paquete..." });
            var destino = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var tempDir = Path.Combine(destino, "update_temp_") + Guid.NewGuid().ToString("N");

            try
            {
                Directory.CreateDirectory(tempDir);

                using (var archive = new ZipArchive(File.OpenRead(tempZip)))
                {
                    int extracted = 0;
                    foreach (var entry in archive.Entries)
                    {
                        try
                        {
                            var entryPath = entry.FullName.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
                            var fullPath = Path.GetFullPath(Path.Combine(tempDir, entryPath));

                            // Contención anti Zip Slip: rechazar entradas cuya ruta
                            // resuelta escape de la carpeta temporal (p.ej. "..\..\").
                            var raizSegura = Path.GetFullPath(tempDir + Path.DirectorySeparatorChar);
                            if (!fullPath.StartsWith(raizSegura, StringComparison.OrdinalIgnoreCase))
                            {
                                Log($"Entrada de ZIP RECHAZADA por path traversal: {entry.FullName}");
                                continue;
                            }

                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                Directory.CreateDirectory(fullPath);
                                continue;
                            }

                            var dir = Path.GetDirectoryName(fullPath);
                            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                            using (var entryStream = entry.Open())
                            using (var outStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                entryStream.CopyTo(outStream);
                            }

                            try
                            {
                                if (entry.LastWriteTime != default)
                                    File.SetLastWriteTimeUtc(fullPath, entry.LastWriteTime.UtcDateTime);
                            }
                            catch { }

                            extracted++;
                        }
                        catch (Exception exEntry)
                        {
                            Log($"Error extrayendo entrada '{entry.FullName}': {exEntry.Message}");
                        }
                    }
                    Log($"Archivos extraídos exitosamente: {extracted}/{archive.Entries.Count}");
                }

                try { File.Delete(tempZip); } catch { }
            }
            catch (Exception ex)
            {
                Log("Error preparar actualización: " + ex);
                try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
                throw new ActualizacionException("La actualización se descargó pero no se pudo preparar: " + ex.Message);
            }

            // Paso 4: lanzar Updater.exe elevado
            var updaterPath = Path.Combine(tempDir, "Updater.exe");
            if (File.Exists(updaterPath))
            {
                // Verificación de firma digital ANTES de ejecutar con privilegios de
                // administrador. Si se configura 'UpdateSignerThumbprint' en
                // App.config, el Updater.exe debe estar firmado con ESE certificado;
                // si no coincide, se bloquea (evita ejecutar binarios no autorizados
                // como admin). Si no se configura, se omite (compatibilidad).
                var thumbprintFirma = ConfigurationManager.AppSettings["UpdateSignerThumbprint"];
                if (!string.IsNullOrWhiteSpace(thumbprintFirma))
                {
                    if (!FirmaConfiable(updaterPath, thumbprintFirma))
                    {
                        try { Directory.Delete(tempDir, true); } catch { }
                        Log("ABORTADO: la firma de Updater.exe no es de confianza.");
                        throw new ActualizacionException("La actualización no pasó la verificación de firma digital. No se aplicará por seguridad.");
                    }
                    Log("Firma de Updater.exe verificada correctamente.");
                }

                progreso?.Report(new ProgresoActualizacion { Porcentaje = 100, Estado = "Iniciando instalador (se pedirán permisos de administrador)..." });

                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = updaterPath,
                        Arguments = $"\"{destino}\" \"{tempDir}\" \"{Path.GetFileName(Application.ExecutablePath)}\"",
                        UseShellExecute = true,
                        Verb = "runas", // Solicitar elevación UAC
                        WorkingDirectory = tempDir
                    };

                    Log("Iniciando Updater.exe con elevación UAC");
                    Process.Start(psi);

                    Log("ACTUALIZACIÓN INICIADA - Cerrando aplicación");
                    Log("========================================");

                    try { Application.Exit(); } catch { }
                    Environment.Exit(0);
                    return true; // inalcanzable, pero mantiene la firma del método
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    // Usuario canceló el UAC o no tiene permisos.
                    Log($"Usuario canceló UAC o sin permisos: {ex.Message}");
                    throw new ActualizacionException(
                        $"Se canceló la elevación de permisos. La actualización quedó lista en:\n{tempDir}\n\n" +
                        "Para aplicarla manualmente: ve a esa carpeta y ejecuta Updater.exe como administrador.",
                        cancelado: true);
                }
                catch (Exception ex)
                {
                    Log("Error al iniciar Updater.exe: " + ex);
                    throw new ActualizacionException(
                        $"Error al iniciar el actualizador: {ex.Message}\n\n" +
                        $"Puedes ejecutar Updater.exe manualmente como administrador desde:\n{tempDir}");
                }
            }

            // Sin Updater.exe en el paquete: copiar lo que se pueda y programar
            // reemplazos en el próximo reinicio para lo que esté en uso.
            Log("Updater.exe no encontrado en el paquete; aplicando actualización sin él...");
            var failed = new System.Collections.Generic.List<string>();
            try
            {
                var files = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    try
                    {
                        var rel = f.Substring(tempDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        var destFile = Path.Combine(destino, rel);
                        var destDir = Path.GetDirectoryName(destFile);
                        if (!string.IsNullOrEmpty(destDir)) Directory.CreateDirectory(destDir);

                        if (string.Equals(Path.GetFileName(destFile), Path.GetFileName(Application.ExecutablePath), StringComparison.OrdinalIgnoreCase))
                        {
                            try { MoveFileEx(f, destFile, MOVEFILE_DELAY_UNTIL_REBOOT | MOVEFILE_REPLACE_EXISTING); }
                            catch { failed.Add(rel); }
                            continue;
                        }

                        try
                        {
                            File.Copy(f, destFile, true);
                            try { File.SetLastWriteTimeUtc(destFile, File.GetLastWriteTimeUtc(f)); } catch { }
                        }
                        catch (Exception exCopy) when (exCopy is IOException || exCopy is UnauthorizedAccessException)
                        {
                            try { MoveFileEx(f, destFile, MOVEFILE_DELAY_UNTIL_REBOOT | MOVEFILE_REPLACE_EXISTING); }
                            catch { failed.Add(rel); }
                        }
                    }
                    catch (Exception exFile)
                    {
                        Log($"Error copiando archivo desde temp: {exFile.Message}");
                        try { failed.Add(f.Substring(tempDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)); } catch { }
                    }
                }

                if (failed.Count == 0)
                {
                    Log($"Actualización completada sin fallos: {versionRemota}");
                    Log("========================================");
                    return true;
                }

                var sample = string.Join(", ", failed.Take(5));
                Log($"Actualización parcialmente preparada, archivos fallaron: {string.Join(";", failed)}");
                throw new ActualizacionException(
                    $"La actualización se preparó en:\n{tempDir}\n\nAlgunos archivos no pudieron copiarse: {sample}...\n\n" +
                    "Reinicia el sistema para aplicar los cambios pendientes.");
            }
            catch (ActualizacionException) { throw; }
            catch (Exception exCopy)
            {
                Log("Error aplicando actualización directamente: " + exCopy);
                throw new ActualizacionException($"La actualización se extrajo en:\n{tempDir}\n\nError al copiar archivos: {exCopy.Message}");
            }
        }

        // ------------------------------------------------------------------
        // Seguridad de la actualización
        // ------------------------------------------------------------------

        // Hosts permitidos para descargar el paquete (solo HTTPS de GitHub).
        private static readonly string[] HostsDescargaPermitidos =
        {
            "github.com", "www.github.com", "codeload.github.com",
            "objects.githubusercontent.com", "release-assets.githubusercontent.com"
        };

        /// <summary>True si la URL es HTTPS y apunta a un host de descarga de confianza.</summary>
        private static bool HostPermitido(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var u)) return false;
            if (u.Scheme != Uri.UriSchemeHttps) return false;
            return HostsDescargaPermitidos.Contains(u.Host, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Descarga el hash SHA-256 esperado del paquete, publicado por convención
        /// como "&lt;url-del-zip&gt;.sha256" (texto con el hash en hex). Devuelve el
        /// hash en minúsculas o null si no existe. Nunca lanza.
        /// </summary>
        private static async Task<string> DescargarHashEsperadoAsync(string updateUrl)
        {
            var hashUrl = updateUrl + ".sha256";
            if (!HostPermitido(hashUrl)) return null;
            try
            {
                var token = ConfigurationManager.AppSettings["GitHubToken"];
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(20);
                    http.DefaultRequestHeaders.UserAgent.ParseAdd("CalandriaUpdater/1.0");
                    if (!string.IsNullOrWhiteSpace(token))
                        try { http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", token); } catch { }

                    var resp = await http.GetAsync(hashUrl);
                    if (!resp.IsSuccessStatusCode)
                    {
                        Log($"No hay '.sha256' publicado para el paquete ({(int)resp.StatusCode}).");
                        return null;
                    }
                    var txt = (await resp.Content.ReadAsStringAsync()).Trim();
                    // Admite tanto "<hash>" como el formato "<hash>  archivo".
                    var m = Regex.Match(txt, "[0-9a-fA-F]{64}");
                    return m.Success ? m.Value.ToLowerInvariant() : null;
                }
            }
            catch (Exception ex)
            {
                Log("Error obteniendo hash esperado: " + ex.Message);
                return null;
            }
        }

        /// <summary>Compara el SHA-256 real del archivo contra el esperado (hex). Nunca lanza.</summary>
        private static bool VerificarSha256(string archivo, string esperadoHex)
        {
            try
            {
                using (var sha = SHA256.Create())
                using (var fs = File.OpenRead(archivo))
                {
                    var hex = BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
                    bool ok = string.Equals(hex, esperadoHex, StringComparison.OrdinalIgnoreCase);
                    Log(ok
                        ? "Integridad OK: el SHA-256 del paquete coincide."
                        : $"Integridad FALLIDA: SHA-256 no coincide. Esperado={esperadoHex} Real={hex}");
                    return ok;
                }
            }
            catch (Exception ex)
            {
                Log("Error verificando SHA-256: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Verifica que el archivo esté firmado (Authenticode) con una cadena válida y
        /// que el certificado del firmante coincida con el thumbprint esperado. Nunca lanza.
        /// </summary>
        private static bool FirmaConfiable(string archivo, string thumbprintEsperado)
        {
            try
            {
                var cert = new X509Certificate2(X509Certificate.CreateFromSignedFile(archivo));
                using (var chain = new X509Chain())
                {
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                    if (!chain.Build(cert))
                    {
                        Log("Firma: la cadena del certificado no es válida.");
                        return false;
                    }
                }
                var esperado = (thumbprintEsperado ?? string.Empty)
                    .Replace(" ", string.Empty).Replace(":", string.Empty);
                bool ok = string.Equals(cert.Thumbprint, esperado, StringComparison.OrdinalIgnoreCase);
                if (!ok) Log($"Firma: el thumbprint no coincide (cert={cert.Thumbprint}).");
                return ok;
            }
            catch (Exception ex)
            {
                Log("Firma: el archivo no tiene una firma válida: " + ex.Message);
                return false;
            }
        }

        private static void Log(string msg)
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Actualizador.log");
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                File.AppendAllText(path, $"[{timestamp}] {msg}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
