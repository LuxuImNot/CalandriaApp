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
        // Versi�n del propio ejecutable (AssemblyFileVersion de Properties\AssemblyInfo.cs,
        // la que el release script bumpea antes de compilar). Ya no depende de un
        // version.txt suelto que se pod�a desincronizar del binario real.
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
                    Log($"Error leyendo la versi�n del ensamblado: {ex.Message}");
                }
                return "0.0.0.0";
            }
        }

        // Exclusiones opcionales por nombre/carpeta
        private static readonly string[] Excluir = new[] { ".git", ".vs" };

        // P/Invoke para programar movimientos en reinicio (fallback)
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);
        private const int MOVEFILE_REPLACE_EXISTING = 0x1;
        private const int MOVEFILE_COPY_ALLOWED = 0x2;
        private const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x4;
        private const int MOVEFILE_WRITE_THROUGH = 0x8;

        // --------------------------------------------------
        // Comparaci�n de versiones personalizada
        // Ignora cualquier texto despu�s de '-'
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
        // Prueba de conectividad a internet y GitHub
        public static async Task<(bool Success, string Message)> TestConectividadAsync()
        {
            Log("=== Iniciando test de conectividad ===");
            
            // Test 1: Conectividad b�sica a internet
            try
            {
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(10);
                    http.DefaultRequestHeaders.UserAgent.ParseAdd("CalandriaUpdater/1.0");
                    
                    Log("Test 1: Probando conectividad a internet (google.com)...");
                    var resp = await http.GetAsync("https://www.google.com");
                    if (!resp.IsSuccessStatusCode)
                    {
                        Log("Test 1 FALLIDO: No se pudo conectar a internet");
                        return (false, "No hay conexi�n a internet disponible.");
                    }
                    Log("Test 1 OK: Conexi�n a internet disponible");
                }
            }
            catch (Exception ex)
            {
                Log("Test 1 ERROR: " + ex.Message);
                return (false, "Error al verificar conexi�n a internet: " + ex.Message);
            }

            // Test 2: Conectividad a GitHub API
            try
            {
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(15);
                    http.DefaultRequestHeaders.UserAgent.ParseAdd("CalandriaUpdater/1.0");
                    
                    var owner = ConfigurationManager.AppSettings["GitHubOwner"] ?? "LuxuImNot";
                    var repo = ConfigurationManager.AppSettings["GitHubRepo"] ?? "CalandriaApp";
                    var apiUrl = $"https://api.github.com/repos/{owner}/{repo}";
                    
                    Log($"Test 2: Probando conectividad a GitHub API ({apiUrl})...");
                    var resp = await http.GetAsync(apiUrl);
                    
                    if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Log("Test 2 FALLIDO: Repositorio no encontrado (404)");
                        return (false, $"El repositorio {owner}/{repo} no existe o no es p�blico.");
                    }
                    
                    if (!resp.IsSuccessStatusCode)
                    {
                        Log($"Test 2 FALLIDO: GitHub API devolvi� {resp.StatusCode}");
                        return (false, $"Error al conectar con GitHub API: {resp.StatusCode}");
                    }
                    
                    Log("Test 2 OK: GitHub API accesible");
                }
            }
            catch (Exception ex)
            {
                Log("Test 2 ERROR: " + ex.Message);
                return (false, "Error al conectar con GitHub: " + ex.Message);
            }

            // Test 3: Verificar si hay releases disponibles
            try
            {
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(15);
                    http.DefaultRequestHeaders.UserAgent.ParseAdd("CalandriaUpdater/1.0");
                    
                    var owner = ConfigurationManager.AppSettings["GitHubOwner"] ?? "LuxuImNot";
                    var repo = ConfigurationManager.AppSettings["GitHubRepo"] ?? "CalandriaApp";
                    var releasesUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
                    
                    Log($"Test 3: Verificando releases disponibles ({releasesUrl})...");
                    var resp = await http.GetAsync(releasesUrl);
                    
                    if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Log("Test 3 ADVERTENCIA: No hay releases publicadas");
                        return (false, "El repositorio no tiene releases publicadas. Crea una release en GitHub primero.");
                    }
                    
                    if (!resp.IsSuccessStatusCode)
                    {
                        Log($"Test 3 FALLIDO: Error al obtener releases: {resp.StatusCode}");
                        return (false, $"Error al obtener releases: {resp.StatusCode}");
                    }
                    
                    var content = await resp.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content) || content.Length < 50)
                    {
                        Log("Test 3 FALLIDO: Respuesta vac�a o inv�lida de la API");
                        return (false, "La respuesta de GitHub API es inv�lida.");
                    }
                    
                    Log("Test 3 OK: Releases disponibles");
                }
            }
            catch (Exception ex)
            {
                Log("Test 3 ERROR: " + ex.Message);
                return (false, "Error al verificar releases: " + ex.Message);
            }

            Log("=== Test de conectividad completado exitosamente ===");
            return (true, "Todas las verificaciones de conectividad pasaron correctamente.");
        }

        // --------------------------------------------------
        // Obtener informaci�n de la release m�s reciente desde la API de GitHub
        // (internal: FormLogin la reutiliza para mostrar la versi�n disponible).
        internal static async Task<(string Tag, string VersionText, string ZipUrl)> GetLatestReleaseInfoAsync()
        {
            var owner = ConfigurationManager.AppSettings["GitHubOwner"] ?? "LuxuImNot";
            var repo = ConfigurationManager.AppSettings["GitHubRepo"] ?? "CalandriaApp";
            var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            var token = ConfigurationManager.AppSettings["GitHubToken"];

            Log($"Consultando API de GitHub: {apiUrl}");

            using (var http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromSeconds(30);
                http.DefaultRequestHeaders.UserAgent.ParseAdd("CalandriaUpdater/1.0");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    try { http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", token); } catch { }
                    Log("Token de autenticaci�n configurado");
                }

                string json;
                try
                {
                    json = await http.GetStringAsync(apiUrl);
                    Log($"Respuesta de API recibida ({json.Length} caracteres)");
                }
                catch (Exception ex)
                {
                    Log("Error obteniendo release latest desde API: " + ex.Message);
                    return (null, null, null);
                }

                // Extraer tag_name
                var tagMatch = Regex.Match(json, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
                var tag = tagMatch.Success ? tagMatch.Groups[1].Value : null;
                Log($"Tag extra�do: {tag ?? "(ninguno)"}");

                // El tag ES la versi�n (ej. "v1.9.5.0"); se le quita la "v" para que
                // se pueda comparar tal cual contra VersionLocal ("1.9.5.0").
                string versionText = tag != null && tag.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                    ? tag.Substring(1)
                    : tag;

                // Extraer URL de ZIP dentro de assets
                var zipMatch = Regex.Match(json, "\"browser_download_url\"\\s*:\\s*\"([^\"]+\\.zip)\"", RegexOptions.IgnoreCase);
                string zipUrl = null;
                if (zipMatch.Success)
                    zipUrl = zipMatch.Groups[1].Value.Replace("\\/", "/");

                Log($"URL de ZIP en assets: {zipUrl ?? "(no encontrado)"}");

                // Si no hay ZIP en assets, usar codeload por tag
                if (string.IsNullOrWhiteSpace(zipUrl) && !string.IsNullOrWhiteSpace(tag))
                {
                    zipUrl = $"https://codeload.github.com/{owner}/{repo}/zip/refs/tags/{tag}";
                    Log($"Usando codeload como fallback: {zipUrl}");
                }

                return (tag, versionText, zipUrl);
            }
        }

        // --------------------------------------------------
        // Verificar y actualizar: consulta la release m�s reciente en GitHub, compara
        // el tag contra VersionLocal y si es necesario descarga el ZIP y lo extrae
        public static async Task<bool> VerificarYActualizarAsync()
        {
            FormProgresoActualizacion frmProgreso = null;
            string versionRemotaRaw = null;

            try
            {
                Log("========================================");
                Log("INICIO DE PROCESO DE ACTUALIZACI�N");
                Log($"Versi�n local actual: {VersionLocal}");
                Log("========================================");

                // Paso 1: Test de conectividad
                Log("Paso 1: Verificando conectividad...");
                var testResult = await TestConectividadAsync();
                if (!testResult.Success)
                {
                    MessageBox.Show($"Fallo en test de conectividad:\n\n{testResult.Message}\n\nRevisa tu conexi�n a internet y la configuraci�n del repositorio.", 
                        "Error de Conectividad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log($"FALLO EN CONECTIVIDAD: {testResult.Message}");
                    return false;
                }
                Log($"Conectividad OK: {testResult.Message}");

                // Paso 2: Obtener info de la �ltima release desde la API
                Log("Paso 2: Obteniendo informaci�n de la �ltima release...");
                var latest = await GetLatestReleaseInfoAsync();
                if (string.IsNullOrWhiteSpace(latest.ZipUrl) && string.IsNullOrWhiteSpace(latest.VersionText))
                {
                    MessageBox.Show("No se pudo obtener informaci�n de la release m�s reciente desde GitHub.\n\nVerifica que:\n- El repositorio exista\n- Haya releases publicadas\n- La configuraci�n de GitHubOwner/GitHubRepo sea correcta", 
                        "Actualizaci�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log("GetLatestReleaseInfoAsync devolvi� informaci�n vac�a");
                    return false;
                }

                versionRemotaRaw = latest.VersionText;
                Log($"Versi�n remota detectada: {versionRemotaRaw}");

                var local = VersionLocal;
                var remotaParaMostrar = versionRemotaRaw;

                // Paso 3: Comparaci�n personalizada
                Log("Paso 3: Comparando versiones...");
                bool esNueva = EsNuevaVersion(local, versionRemotaRaw);
                Log($"�Es nueva versi�n? {esNueva} (Local: {local}, Remota: {versionRemotaRaw})");
                
                if (!esNueva)
                {
                    MessageBox.Show($"Tu versi�n ({local}) ya est� actualizada.\n\nVersi�n disponible: {remotaParaMostrar}", 
                        "Actualizaci�n", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log($"No hay actualizaci�n necesaria");
                    return false;
                }

                // Paso 4: Obtener URL de descarga
                Log("Paso 4: Preparando descarga...");
                var updateUrl = ConfigurationManager.AppSettings["UpdateUrl"] ?? latest.ZipUrl;
                Log($"URL de descarga: {updateUrl}");

                if (string.IsNullOrWhiteSpace(updateUrl))
                {
                    MessageBox.Show("No se encontr� un paquete ZIP para la release m�s reciente.\n\nAseg�rate de subir un archivo .zip como asset en la release.",
                        "Actualizaci�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log("No se encontr� URL de ZIP para descargar");
                    return false;
                }

                // Seguridad: el paquete solo puede descargarse por HTTPS desde hosts
                // de confianza (GitHub). Evita que un UpdateUrl manipulado en el config
                // redirija la descarga a un servidor arbitrario.
                if (!HostPermitido(updateUrl))
                {
                    MessageBox.Show("La URL de actualizaci�n no est� permitida por seguridad.\n\nLa operaci�n se cancel�.",
                        "Actualizaci�n bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log($"URL de descarga RECHAZADA (host no permitido o no HTTPS): {updateUrl}");
                    return false;
                }

                // Preparar form de progreso usando FormProgresoActualizacion en hilo STA
                var uiThread = new Thread(() =>
                {
                    try
                    {
                        frmProgreso = new FormProgresoActualizacion();
                        try
                        {
                            frmProgreso.progressBar1.Style = ProgressBarStyle.Continuous;
                            frmProgreso.progressBar1.Maximum = 100;
                            frmProgreso.progressBar1.Value = 0;
                            frmProgreso.lblEstado.Text = "Preparando descarga...";
                        }
                        catch { }
                        Application.Run(frmProgreso);
                    }
                    catch { }
                });
                uiThread.SetApartmentState(ApartmentState.STA);
                uiThread.IsBackground = true;
                uiThread.Start();

                // Esperar a que el form exista (mejor esfuerzo)
                int esperas = 0;
                while (frmProgreso == null && esperas++ < 40) await Task.Delay(50);

                // Paso 5: Descargar ZIP con progreso
                Log("Paso 5: Descargando paquete de actualizaci�n...");
                var tempZip = Path.Combine(Path.GetTempPath(), "CalandriaUpdate_" + Guid.NewGuid().ToString("N") + ".zip");
                Log($"Archivo temporal: {tempZip}");
                
                try
                {
                    using (var http = new HttpClient())
                    {
                        http.Timeout = TimeSpan.FromMinutes(10); // Timeout generoso para descargas grandes
                        using (var resp = await http.GetAsync(updateUrl, HttpCompletionOption.ResponseHeadersRead))
                        {
                            resp.EnsureSuccessStatusCode();
                            var total = resp.Content.Headers.ContentLength.GetValueOrDefault(-1L);
                            Log($"Tama�o del paquete: {(total > 0 ? (total / 1024.0 / 1024.0).ToString("F2") + " MB" : "desconocido")}");
                            
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

                                    int percent = 0;
                                    if (total > 0)
                                        percent = (int)Math.Round((totalRead * 100.0) / total);

                                    try
                                    {
                                        if (frmProgreso != null && !frmProgreso.IsDisposed)
                                        {
                                            frmProgreso.Invoke((Action)(() =>
                                            {
                                                try { frmProgreso.progressBar1.Value = Math.Min(100, Math.Max(0, percent)); } catch { }
                                                try { frmProgreso.lblEstado.Text = $"Descargando {Path.GetFileName(updateUrl)} � {frmProgreso.progressBar1.Value}%"; } catch { }
                                            }));
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    Log($"Descarga completada: {new FileInfo(tempZip).Length / 1024.0 / 1024.0:F2} MB");
                }
                catch (Exception ex)
                {
                    try { if (frmProgreso != null && !frmProgreso.IsDisposed) frmProgreso.Invoke((Action)(() => frmProgreso.Close())); } catch { }
                    MessageBox.Show("Error al descargar el paquete de actualizaci�n:\n\n" + ex.Message + "\n\nIntenta nuevamente m�s tarde.", 
                        "Error de Descarga", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log("Error descargar ZIP: " + ex);
                    return false;
                }

                // Cerrar form de progreso
                try { if (frmProgreso != null && !frmProgreso.IsDisposed) frmProgreso.Invoke((Action)(() => frmProgreso.Close())); } catch { }

                // Paso 5.1: Verificar INTEGRIDAD del paquete (SHA-256) ANTES de extraer
                // o ejecutar nada. El hash esperado se publica junto al ZIP como
                // "<zip>.sha256" (mismo canal HTTPS de GitHub). Si no hay hash:
                //   - RequireUpdateHash=true  -> se aborta (modo estricto).
                //   - RequireUpdateHash=false -> se contin�a con advertencia (compat).
                Log("Paso 5.1: Verificando integridad del paquete (SHA-256)...");
                string hashEsperado = await DescargarHashEsperadoAsync(updateUrl);
                bool requiereHash = string.Equals(
                    (ConfigurationManager.AppSettings["RequireUpdateHash"] ?? "false").Trim(),
                    "true", StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(hashEsperado))
                {
                    if (!VerificarSha256(tempZip, hashEsperado))
                    {
                        try { File.Delete(tempZip); } catch { }
                        MessageBox.Show("El paquete de actualizaci�n no pas� la verificaci�n de integridad (SHA-256).\n\nNo se aplicar� la actualizaci�n.",
                            "Actualizaci�n bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log("ABORTADO: el hash SHA-256 del paquete no coincide.");
                        return false;
                    }
                }
                else if (requiereHash)
                {
                    try { File.Delete(tempZip); } catch { }
                    MessageBox.Show("No se encontr� el hash de verificaci�n del paquete y la pol�tica de seguridad exige verificarlo.\n\nActualizaci�n cancelada.",
                        "Actualizaci�n bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log("ABORTADO: RequireUpdateHash=true pero no hay '.sha256' publicado para el paquete.");
                    return false;
                }
                else
                {
                    Log("ADVERTENCIA: el paquete no tiene '.sha256' publicado; se contin�a (RequireUpdateHash=false).");
                }

                // Paso 6: EXTRAER ZIP EN CARPETA TEMPORAL
                Log("Paso 6: Extrayendo paquete...");
                try
                {
                    var destino = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    Log($"Directorio de instalaci�n: {destino}");

                    // Crear carpeta temporal dentro del directorio de la aplicaci�n
                    var tempDir = Path.Combine(destino, "update_temp_") + Guid.NewGuid().ToString("N");
                    Directory.CreateDirectory(tempDir);
                    Log($"Carpeta temporal creada: {tempDir}");

                    using (var archive = new ZipArchive(File.OpenRead(tempZip)))
                    {
                        Log($"Extrayendo {archive.Entries.Count} archivos...");
                        int extracted = 0;
                        
                        foreach (var entry in archive.Entries)
                        {
                            try
                            {
                                // Normalizar la ruta dentro del zip
                                var entryPath = entry.FullName.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
                                var fullPath = Path.GetFullPath(Path.Combine(tempDir, entryPath));

                                // Contenci�n anti Zip Slip: rechazar entradas cuya ruta
                                // resuelta escape de la carpeta temporal (p.ej. "..\..\").
                                var raizSegura = Path.GetFullPath(tempDir + Path.DirectorySeparatorChar);
                                if (!fullPath.StartsWith(raizSegura, StringComparison.OrdinalIgnoreCase))
                                {
                                    Log($"Entrada de ZIP RECHAZADA por path traversal: {entry.FullName}");
                                    continue;
                                }

                                // Si es directorio
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
                                    {
                                        File.SetLastWriteTimeUtc(fullPath, entry.LastWriteTime.UtcDateTime);
                                    }
                                }
                                catch { }
                                
                                extracted++;
                            }
                            catch (Exception exEntry)
                            {
                                Log($"Error extrayendo entrada '{entry.FullName}': {exEntry.Message}");
                            }
                        }
                        
                        Log($"Archivos extra�dos exitosamente: {extracted}/{archive.Entries.Count}");
                    }

                    try { File.Delete(tempZip); Log("Archivo ZIP temporal eliminado"); } catch { }

                    // Intentar lanzar Updater.exe desde la carpeta temporal con elevaci�n
                    var updaterPath = Path.Combine(tempDir, "Updater.exe");
                    if (File.Exists(updaterPath))
                    {
                        Log($"Updater.exe encontrado: {updaterPath}");

                        // Verificaci�n de firma digital ANTES de ejecutar con privilegios
                        // de administrador. Si se configura 'UpdateSignerThumbprint' en
                        // App.config, el Updater.exe debe estar firmado con ESE certificado;
                        // si no coincide, se bloquea (evita ejecutar binarios no autorizados
                        // como admin). Si no se configura, se omite (compatibilidad).
                        var thumbprintFirma = ConfigurationManager.AppSettings["UpdateSignerThumbprint"];
                        if (!string.IsNullOrWhiteSpace(thumbprintFirma))
                        {
                            if (!FirmaConfiable(updaterPath, thumbprintFirma))
                            {
                                try { Directory.Delete(tempDir, true); } catch { }
                                MessageBox.Show("La actualizaci�n no pas� la verificaci�n de firma digital.\n\nNo se aplicar� por seguridad.",
                                    "Actualizaci�n bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Log("ABORTADO: la firma de Updater.exe no es de confianza.");
                                return false;
                            }
                            Log("Firma de Updater.exe verificada correctamente.");
                        }
                        else
                        {
                            Log("UpdateSignerThumbprint no configurado; se omite verificaci�n de firma (se recomienda configurarlo).");
                        }

                        // Mostrar mensaje al usuario ANTES de solicitar elevaci�n
                        var confirmResult = MessageBox.Show(
                            $"Se descarg� la actualizaci�n a la versi�n {remotaParaMostrar}.\n\n" +
                            "Para aplicar la actualizaci�n, necesitamos permisos de administrador.\n\n" +
                            "�Deseas continuar?\n\n" +
                            "Se cerrar� la aplicaci�n y se solicitar�n permisos de administrador.",
                            "Actualizaci�n Disponible",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (confirmResult != DialogResult.Yes)
                        {
                            Log("Usuario cancel� la aplicaci�n de actualizaci�n");
                            MessageBox.Show(
                                $"La actualizaci�n fue descargada pero no se aplic�.\n\n" +
                                $"Puedes aplicarla m�s tarde ejecutando manualmente:\n{updaterPath}\n\n" +
                                "Aseg�rate de ejecutarlo como Administrador (clic derecho > Ejecutar como administrador)",
                                "Actualizaci�n Cancelada",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            return false;
                        }

                        try
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = updaterPath,
                                Arguments = $"\"{destino}\" \"{tempDir}\" \"{Path.GetFileName(Application.ExecutablePath)}\"",
                                UseShellExecute = true,
                                Verb = "runas", // Solicitar elevaci�n UAC
                                WorkingDirectory = tempDir
                            };

                            
                            Log("Iniciando Updater.exe con elevaci�n UAC");
                            Process.Start(psi);

                            // Informar usuario y salir para que Updater pueda reemplazar archivos
                            MessageBox.Show(
                                $"Actualizaci�n iniciada correctamente.\n\n" +
                                $"Se aplicar� la versi�n {remotaParaMostrar}.\n\n" +
                                "La aplicaci�n se cerrar� ahora para completar la instalaci�n.",
                                "Actualizaci�n en Progreso",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            Log("========================================");
                            Log("ACTUALIZACI�N INICIADA - Cerrando aplicaci�n");
                            Log("========================================");
                            
                            // Salir de la aplicaci�n para permitir que el updater reemplace archivos
                            try { Application.Exit(); } catch { }
                            Environment.Exit(0);
                            return true;
                        }
                        catch (System.ComponentModel.Win32Exception ex)
                        {
                            // Usuario cancel� el UAC o no tiene permisos
                            Log($"Usuario cancel� UAC o sin permisos: {ex.Message}");
                            MessageBox.Show(
                                "No se pudo iniciar el actualizador porque se cancel� la elevaci�n de privilegios o no tienes permisos de administrador.\n\n" +
                                "Para aplicar la actualizaci�n manualmente:\n\n" +
                                "1. Cierra esta aplicaci�n\n" +
                                $"2. Ve a la carpeta: {tempDir}\n" +
                                "3. Haz clic derecho en Updater.exe\n" +
                                "4. Selecciona 'Ejecutar como administrador'\n\n" +
                                "La actualizaci�n quedar� pendiente hasta que completes estos pasos.",
                                "Permisos de Administrador Requeridos",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            return false;
                        }
                        catch (Exception ex)
                        {
                            Log("Error al iniciar Updater.exe: " + ex);
                            MessageBox.Show(
                                $"Error al iniciar el actualizador:\n\n{ex.Message}\n\n" +
                                "Instrucciones para actualizar manualmente:\n\n" +
                                "1. Cierra esta aplicaci�n\n" +
                                $"2. Ve a: {tempDir}\n" +
                                "3. Ejecuta Updater.exe como Administrador\n" +
                                "   (clic derecho > Ejecutar como administrador)",
                                "Error al Iniciar Updater",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else
                    {
                        Log("Updater.exe no encontrado en el paquete");
                    }

                    // Si no existe Updater.exe: intentar copiar lo que sea posible y programar reemplazos en reinicio
                    Log("Intentando aplicar actualizaci�n sin Updater.exe...");
                    var failed = new System.Collections.Generic.List<string>();
                    try
                    {
                        var files = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories);
                        Log($"Intentando copiar {files.Length} archivos...");
                        
                        foreach (var f in files)
                        {
                            try
                            {
                                var rel = f.Substring(tempDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                                var destFile = Path.Combine(destino, rel);
                                var destDir = Path.GetDirectoryName(destFile);
                                if (!string.IsNullOrEmpty(destDir)) Directory.CreateDirectory(destDir);

                                // No sobreescribir el propio ejecutable en caso de estar en uso
                                if (string.Equals(Path.GetFileName(destFile), Path.GetFileName(Application.ExecutablePath), StringComparison.OrdinalIgnoreCase))
                                {
                                    // programar reemplazo en reinicio desde temp
                                    try
                                    {
                                        MoveFileEx(f, destFile, MOVEFILE_DELAY_UNTIL_REBOOT | MOVEFILE_REPLACE_EXISTING);
                                        Log($"Programado reemplazo en reinicio: {destFile}");
                                    }
                                    catch { failed.Add(rel); }
                                    continue;
                                }

                                try
                                {
                                    File.Copy(f, destFile, true);
                                    try { File.SetLastWriteTimeUtc(destFile, File.GetLastWriteTimeUtc(f)); } catch { }
                                }
                                catch (IOException)
                                {
                                    try
                                    {
                                        MoveFileEx(f, destFile, MOVEFILE_DELAY_UNTIL_REBOOT | MOVEFILE_REPLACE_EXISTING);
                                        Log($"Programado reemplazo en reinicio (IO): {destFile}");
                                    }
                                    catch { failed.Add(rel); }
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    try
                                    {
                                        MoveFileEx(f, destFile, MOVEFILE_DELAY_UNTIL_REBOOT | MOVEFILE_REPLACE_EXISTING);
                                        Log($"Programado reemplazo en reinicio (Perm): {destFile}");
                                    }
                                    catch { failed.Add(rel); }
                                }
                            }
                            catch (Exception exFile)
                            {
                                Log($"Error copiando archivo desde temp: {exFile.Message}");
                                try { var rel2 = f.Substring(tempDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar); failed.Add(rel2); } catch { }
                            }
                        }

                        if (failed.Count == 0)
                        {
                            MessageBox.Show($"Actualizaci�n aplicada correctamente.\n\nSi hay archivos en uso se aplicar�n al reiniciar el sistema.\n\nVersi�n: {remotaParaMostrar}", 
                                "Actualizaci�n Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Log($"Actualizaci�n completada sin fallos: {remotaParaMostrar}");
                            Log("========================================");
                            return true;
                        }
                        else
                        {
                            var sample = string.Join(", ", failed.Take(5));
                            MessageBox.Show($"La actualizaci�n se prepar� en:\n{tempDir}\n\nAlgunos archivos no pudieron copiarse: {sample}...\n\nSe recomienda:\n1. Reiniciar el sistema para aplicar los cambios programados\n2. O ejecutar Updater.exe manualmente desde la carpeta temporal", 
                                "Actualizaci�n Parcial", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Log($"Actualizaci�n parcialmente preparada, archivos fallaron: {string.Join(";", failed)}");
                            return true;
                        }
                    }
                    catch (Exception exCopy)
                    {
                        Log("Error aplicando actualizaci�n directamente: " + exCopy);
                        MessageBox.Show($"La actualizaci�n se extrajo en:\n{tempDir}\n\nError al copiar archivos: {exCopy.Message}\n\nEjecuta Updater.exe manualmente desde esa carpeta con permisos de administrador.", 
                            "Error al Aplicar Actualizaci�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"La actualizaci�n se descarg� pero no se pudo preparar:\n\n{ex.Message}\n\nRevisa los logs en Actualizador.log para m�s detalles.", 
                        "Error de Preparaci�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log("Error preparar actualizaci�n: " + ex);
                    try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
                    return false;
                }
            }
            catch (Exception ex)
            {
                try { if (frmProgreso != null && !frmProgreso.IsDisposed) frmProgreso.Invoke((Action)(() => frmProgreso.Close())); } catch { }
                MessageBox.Show($"Error general en el proceso de actualizaci�n:\n\n{ex.Message}\n\nRevisa Actualizador.log para detalles completos.", 
                    "Error de Actualizaci�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log("Error verificacion/actualizacion: " + ex);
                Log("========================================");
                return false;
            }
        }

        // Resto de m�todos originales
        private static bool DebeExcluir(string ruta)
        {
            foreach (var pat in Excluir)
                if (ruta.IndexOf(pat, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        private static void CopiarConReintentos(string origen, string destino, int reintentos, int esperaMs)
        {
            Exception lastEx = null;
            for (int i = 0; i < reintentos; i++)
            {
                try
                {
                    try
                    {
                        if (File.Exists(destino))
                        {
                            try
                            {
                                File.SetAttributes(destino, FileAttributes.Normal);
                                File.Delete(destino);
                            }
                            catch (Exception delEx)
                            {
                                Log($"No se pudo eliminar archivo destino antes de copiar ({destino}): {delEx.Message}");
                            }
                        }
                    }
                    catch { }

                    File.Copy(origen, destino, true);
                    File.SetLastWriteTimeUtc(destino, File.GetLastWriteTimeUtc(origen));
                    try
                    {
                        if (string.Equals(Path.GetFileName(destino), "Updater.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var ads = destino + ":Zone.Identifier";
                                if (File.Exists(ads))
                                {
                                    File.Delete(ads);
                                    Log($"Eliminado Zone.Identifier para {destino}");
                                }
                            }
                            catch (Exception exAds)
                            {
                                Log($"No se pudo eliminar Zone.Identifier de {destino}: {exAds.Message}");
                            }
                        }
                    }
                    catch { }
                    return;
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    lastEx = ex;
                    Log($"CopiarConReintentos intento {i + 1} falla: {ex.Message}");
                    Thread.Sleep(esperaMs);
                }
            }

            try
            {
                File.Copy(origen, destino, true);
                try
                {
                    File.SetLastWriteTimeUtc(destino, File.GetLastWriteTimeUtc(origen));
                }
                catch (Exception exTime)
                {
                    Log($"No se pudo establecer timestamp en destino {destino}: {exTime.Message}");
                }
                try
                {
                    if (string.Equals(Path.GetFileName(destino), "Updater.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var ads = destino + ":Zone.Identifier";
                            if (File.Exists(ads))
                            {
                                File.Delete(ads);
                                Log($"Eliminado Zone.Identifier para {destino}");
                            }
                        }
                        catch (Exception exAds)
                        {
                            Log($"No se pudo eliminar Zone.Identifier de {destino}: {exAds.Message}");
                        }
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                Log($"CopiarConReintentos �ltimo intento falla para {origen} -> {destino}: {ex}");
                return;
            }
        }

        // ------------------------------------------------------------------
        // Seguridad de la actualizaci�n
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
        /// Descarga el hash SHA-256 esperado del paquete, publicado por convenci�n
        /// como "&lt;url-del-zip&gt;.sha256" (texto con el hash en hex). Devuelve el
        /// hash en min�sculas o null si no existe. Nunca lanza.
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
        /// Verifica que el archivo est� firmado (Authenticode) con una cadena v�lida y
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
                        Log("Firma: la cadena del certificado no es v�lida.");
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
                Log("Firma: el archivo no tiene una firma v�lida: " + ex.Message);
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

        private static string GetRelativePath(string basePath, string fullPath)
        {
            try
            {
                var baseDir = basePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                    return Path.GetFileName(fullPath);
                return fullPath.Substring(baseDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch { return Path.GetFileName(fullPath); }
        }
    }
}
