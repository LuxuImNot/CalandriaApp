using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text;

namespace Calandria.Updater
{
    internal static class Program
    {
        // Tunables: ajustar si los reintentos son muchos
        private const int CopyRetries = 5; // n�mero de intentos para copiar cada archivo

        [STAThread]
        static int Main(string[] args)
        {
            // LOG argumentos y entorno al inicio
            string logBoot = null;
            string carpetaLocal = null;
            string carpetaTemp = null;
            string exePrincipal = null;
            try
            {
                // Args: [0]=carpetaLocal [1]=carpetaTemp [2]=exePrincipal
                if (args != null && args.Length >= 3)
                {
                    carpetaLocal = args[0].Trim('"');
                    carpetaTemp = args[1].Trim('"');
                    exePrincipal = args[2].Trim('"');
                }
                else
                {
                    // intentar detecci�n autom�tica si no se pasaron argumentos
                    carpetaLocal = Environment.CurrentDirectory;

                    try
                    {
                        // Buscar carpetas "update_temp*" en current dir y en directorio del ejecutable
                        string cur = Environment.CurrentDirectory;
                        string exeDir = Path.GetDirectoryName(Application.ExecutablePath) ?? cur;

                        string foundTemp = null;
                        var searchDirs = new[] { cur, exeDir, Path.GetDirectoryName(cur) ?? cur };
                        foreach (var sd in searchDirs.Distinct())
                        {
                            try
                            {
                                if (Directory.Exists(sd))
                                {
                                    var matches = Directory.GetDirectories(sd, "update_temp*");
                                    if (matches != null && matches.Length > 0)
                                    {
                                        foundTemp = matches.OrderByDescending(d => d).First();
                                        break;
                                    }
                                }
                            }
                            catch { }
                        }

                        // Also try one level deeper in case the temp folder is inside the current dir
                        if (foundTemp == null)
                        {
                            try
                            {
                                var deeper = Directory.GetDirectories(cur, "update_temp*", SearchOption.AllDirectories).FirstOrDefault();
                                if (!string.IsNullOrEmpty(deeper)) foundTemp = deeper;
                            }
                            catch { }
                        }

                        if (!string.IsNullOrEmpty(foundTemp))
                        {
                            carpetaTemp = foundTemp;
                            // carpetaLocal as parent of temp
                            var parent = Path.GetDirectoryName(foundTemp.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                            carpetaLocal = parent ?? carpetaLocal;

                            // intentar detectar exe principal en carpeta local (primer exe que no sea Updater)
                            try
                            {
                                var exes = Directory.GetFiles(carpetaLocal, "*.exe", SearchOption.TopDirectoryOnly)
                                                    .Where(p => !p.EndsWith("Updater.exe", StringComparison.OrdinalIgnoreCase))
                                                    .ToArray();
                                if (exes.Length > 0)
                                {
                                    exePrincipal = Path.GetFileName(exes.OrderBy(p => p).First());
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                logBoot = Path.Combine(carpetaLocal, "updater_boot.log");
                File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Updater iniciado\nArgs: {string.Join(", ", args ?? new string[0])}\nCurrentDir: {Environment.CurrentDirectory}\nExe: {Application.ExecutablePath}\nUser: {Environment.UserName}\n");
            }
            catch { }

            if (args == null || args.Length < 3)
            {
                try { File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: argumentos insuficientes; valores detectados: carpetaLocal={carpetaLocal} carpetaTemp={carpetaTemp} exePrincipal={exePrincipal}\n"); } catch { }
                // If we were able to auto-detect necessary info, continue; otherwise return error
                if (string.IsNullOrEmpty(carpetaTemp) || string.IsNullOrEmpty(exePrincipal) || string.IsNullOrEmpty(carpetaLocal))
                {
                    try { File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: No se pudo detectar autom�ticamente carpetaTemp/carpetaLocal/exePrincipal\n"); } catch { }
                    return 2;
                }
            }

            try
            {
                // Auto-elevaci�n si hace falta (Program Files, etc.)
                if (!IsAdmin())
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = Application.ExecutablePath,
                            Arguments = string.Join(" ", args.Select(a => $"\"{a}\"").ToArray()),
                            Verb = "runas", // UAC
                            UseShellExecute = true
                        };
                        File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Intentando auto-elevaci�n UAC\n");
                        Process.Start(psi);
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        Log(carpetaLocal, "UAC cancelado o sin privilegios. Abortando.");
                        File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR UAC: {ex}\n");
                        return 1;
                    }
                }

                // Para cuando Updater arranca (tras aprobar el UAC), la app principal
                // ya llam� Environment.Exit(0) hace rato: este espera es solo un
                // colch�n de seguridad, no el mecanismo real anti-bloqueo (eso lo
                // hace el reintento con backoff en CopiarRecursivoConLog). 120s aqu�
                // solo alargaba la espera en equipos lentos sin ganar nada.
                EsperarCierreExe(carpetaLocal, exePrincipal, TimeSpan.FromSeconds(15));

                Log(carpetaLocal, $"Iniciando copia desde TEMP: {carpetaTemp} -> {carpetaLocal}");
                File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Copiando archivos...\n");
                int archivosCopiados = 0;
                try
                {
                    archivosCopiados = CopiarRecursivoConLog(carpetaTemp, carpetaLocal, logBoot);
                }
                catch (Exception ex)
                {
                    Log(carpetaLocal, "ERROR en copia: " + ex);
                    File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR en copia: {ex}\n");
                    MessageBox.Show($"Error al copiar archivos: {ex.Message}", "Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }

                if (archivosCopiados == 0)
                {
                    Log(carpetaLocal, "No se copi� ning�n archivo.");
                    File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] No se copi� ning�n archivo\n");
                    MessageBox.Show("No se copi� ning�n archivo. Verifica permisos y rutas.", "Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Limpieza: eliminar carpeta temporal SOLO si se copiaron archivos
                try
                {
                    if (archivosCopiados > 0)
                    {
                        try { Directory.Delete(carpetaTemp, true); }
                        catch (Exception exDel) { File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARN: no se pudo borrar carpeta temp: {exDel.Message}\n"); }
                    }
                    else
                    {
                        // Mantener carpeta temp para diagn�stico si no se copi� nada
                        File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: carpeta temp preservada para diagn�stico: {carpetaTemp}\n");
                    }
                }
                catch { }

                string exePath = Path.Combine(carpetaLocal, exePrincipal);
                bool lanzado = LanzarAplicacionRobusto(exePath, carpetaLocal, out string detalle);

                Log(carpetaLocal, lanzado
                    ? $"Actualizaci�n aplicada. Relanzado OK: {exePath}"
                    : $"Actualizaci�n aplicada, pero NO se pudo relanzar. Detalle: {detalle}");
                File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Lanzar app: {exePath} -> {lanzado} Detalle: {detalle}\n");

                if (!lanzado)
                {
                    try
                    {
                        MessageBox.Show(
                            "Se aplic� la actualizaci�n, pero no se pudo reabrir autom�ticamente.\n" +
                            "Por favor, abre la aplicaci�n manualmente desde:\n" + exePath +
                            "\n\nDetalle: " + detalle,
                            "Actualizaci�n aplicada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch { }
                }

                return lanzado ? 0 : 1;
            }
            catch (Exception ex)
            {
                Log(carpetaLocal, "ERROR: " + ex);
                try
                {
                    MessageBox.Show("Error al aplicar actualizaci�n:\n" + ex.Message,
                                    "Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
                try { File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR general: {ex}\n"); } catch { }
                return 1;
            }
        }

        // NUEVO: Copia con log y contador
        static int CopiarRecursivoConLog(string src, string dst, string logBoot)
        {
            int copiados = 0;
            foreach (var dir in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
            {
                var rel = dir.Substring(src.Length).TrimStart(new char[] {'\\', '/'});
                Directory.CreateDirectory(Path.Combine(dst, rel));
            }
            foreach (var file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            {
                var rel = file.Substring(src.Length).TrimStart(new char[] {'\\', '/'});
                var destino = Path.Combine(dst, rel);
                var dirdest = Path.GetDirectoryName(destino) ?? dst;
                Directory.CreateDirectory(dirdest);
                // intentos para copiar (reintentos si el archivo est� en uso por otro proceso)
                // If the destination is this running Updater exe, skip copying it to avoid "file in use" errors
                try
                {
                    var currentExe = Process.GetCurrentProcess().MainModule.FileName;
                    if (string.Equals(Path.GetFullPath(destino), Path.GetFullPath(currentExe), StringComparison.OrdinalIgnoreCase))
                    {
                        File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Omitiendo copia de Updater en uso: {destino}\n");
                        continue;
                    }
                }
                catch { }
                for (int i = 0; i < CopyRetries; i++)
                 {
                    try
                    {
                        // File.Copy ya intenta abrir origen/destino; si alguno est�
                        // bloqueado lanza IOException/UnauthorizedAccessException y el
                        // catch de abajo reintenta con backoff. Antes se probaba abrir
                        // cada archivo por separado ANTES de copiarlo (hasta 3 aperturas
                        // por archivo): en equipos con antivirus que reescanea cada
                        // apertura, eso triplicaba la latencia por archivo sin aportar
                        // nada en el caso normal (sin bloqueo).
                        if (File.Exists(destino)) File.SetAttributes(destino, FileAttributes.Normal);

                        File.Copy(file, destino, true);
                        File.SetLastWriteTimeUtc(destino, File.GetLastWriteTimeUtc(file));
                        copiados++;
                        File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Copiado: {file} -> {destino}\n");
                        break; // �xito
                    }
                    catch (IOException ex)
                    {
                        // posible lock por otro proceso
                        Thread.Sleep(250 + i * 200); // backoff creciente
                        if (i == CopyRetries - 1)
                        {
                            File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR IO tras reintentos: {file} -> {destino} : {ex}\n");
                            throw;
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Thread.Sleep(250 + i * 200);
                        if (i == CopyRetries - 1)
                        {
                            File.AppendAllText(logBoot, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR permisos tras reintentos: {file} -> {destino} : {ex}\n");
                            throw;
                        }
                    }
                 }
             }
             return copiados;
         }

        static void EsperarCierreExe(string carpetaLocal, string exePrincipal, TimeSpan max)
        {
            string exeSinExt = Path.GetFileNameWithoutExtension(exePrincipal);
            var sw = Stopwatch.StartNew();

            while (sw.Elapsed < max)
            {
                var vivos = Process.GetProcessesByName(exeSinExt)
                    .Where(p =>
                    {
                        try
                        {
                            string ruta = Path.GetDirectoryName(p.MainModule.FileName);
                            return string.Equals(ruta, carpetaLocal, StringComparison.OrdinalIgnoreCase);
                        }
                        catch { return false; }
                    })
                    .ToList();

                if (vivos.Count == 0) break;
                Thread.Sleep(250);
            }
        }

        static bool LanzarAplicacionRobusto(string exePath, string workingDir, out string detalle)
        {
            detalle = "";

            try
            {
                if (!File.Exists(exePath))
                {
                    detalle = "EXE no existe en: " + exePath;
                    return false;
                }

                // Intento 1: arranque directo con shell + WD
                for (int i = 1; i <= 3; i++)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = exePath,
                            WorkingDirectory = workingDir,
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                        Thread.Sleep(200); // dar respiro para que levante
                        return true;
                    }
                    catch (Exception e1)
                    {
                        detalle = $"Intento1 fallo #{i}: {e1.Message}";
                        Thread.Sleep(250);
                    }
                }

                // Intento 2: via cmd /c start "" "exe"
                try
                {
                    var psi2 = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c start \"\" \"" + exePath + "\"",
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(psi2);
                    Thread.Sleep(300);
                    return true;
                }
                catch (Exception e2)
                {
                    detalle = "Intento2 (cmd start) fall�: " + e2.Message;
                }

                // Intento 3: sin shell, directo
                try
                {
                    var p = new Process();
                    p.StartInfo.FileName = exePath;
                    p.StartInfo.WorkingDirectory = workingDir;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    Thread.Sleep(200);
                    return true;
                }
                catch (Exception e3)
                {
                    detalle = "Intento3 directo fall�: " + e3.Message;
                    return false;
                }
            }
            catch (Exception e)
            {
                detalle = "Excepci�n general en relanzar: " + e.Message;
                return false;
            }
        }

        static bool IsAdmin()
        {
            try
            {
                var wi = WindowsIdentity.GetCurrent();
                var wp = new WindowsPrincipal(wi);
                return wp.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        static void Log(string carpetaLocal, string msg)
        {
            try
            {
                var log = Path.Combine(carpetaLocal, "Updater.log");
                File.AppendAllText(log, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + msg + Environment.NewLine);
            }
            catch { }
        }

        // Calcula el hash SHA1 de un string y lo devuelve en formato hexadecimal.
        static string Sha1Hex(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Concat(hashBytes.Select(b => b.ToString("x2")));
            }
        }
    }
}
