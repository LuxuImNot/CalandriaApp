using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Manejador global de errores. Registra cada excepción (de cualquier equipo)
    /// en la base de datos central (tabla LogErrores) para poder consultarla desde
    /// el panel administrativo, con respaldo en archivo local. Evita además que el
    /// diálogo de error de WinForms se muestre en bucle cuando una misma excepción
    /// se repite (p.ej. en cada repintado de un control).
    /// </summary>
    public static class ErrorLogger
    {
        private const string TablaLog = "LogErrores";

        private static readonly object _lock = new object();

        private static readonly string CarpetaLog = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DynamicSepticSystem", "logs");

        public static readonly string RutaLog = Path.Combine(CarpetaLog, "errores.log");

        // Antispam: no repetir registro/diálogo de la misma excepción seguida.
        private static string _firmaLog;
        private static DateTime _vezLog;
        private static string _firmaDialogo;
        private static DateTime _vezDialogo;

        /// <summary>
        /// Engancha los manejadores globales. Llamar una sola vez al inicio de Main.
        /// </summary>
        public static void Inicializar()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Manejar(e.Exception, "ThreadException");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Registrar(e.ExceptionObject as Exception, "UnhandledException");
        }

        private static void Manejar(Exception ex, string origen)
        {
            Registrar(ex, origen);

            // Si la misma excepción se repite en pocos segundos (típico al expandir
            // y repintar), solo la registramos y dejamos que la app continúe sin
            // volver a interrumpir al usuario con el diálogo.
            string firma = (ex?.GetType().FullName ?? "null") + "|" + (ex?.Message ?? "");
            DateTime ahora = DateTime.Now;
            if (firma == _firmaDialogo && (ahora - _vezDialogo).TotalSeconds < 10)
            {
                _vezDialogo = ahora;
                return;
            }
            _firmaDialogo = firma;
            _vezDialogo = ahora;

            MessageBox.Show(
                "Ocurrió un error inesperado. La aplicación intentará continuar.\n\n" +
                "El detalle se registró para el administrador.\n\n" +
                (ex?.Message ?? ""),
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>Registra una excepción. Nunca lanza.</summary>
        public static void Registrar(Exception ex, string origen)
        {
            Registrar(origen,
                ex?.GetType().FullName ?? "null",
                ex?.Message ?? "",
                ex?.ToString() ?? "null");
        }

        /// <summary>Registra un mensaje manual desde un bloque catch. Nunca lanza.</summary>
        public static void RegistrarMensaje(string origen, string mensaje)
        {
            Registrar(origen, "Manual", mensaje, mensaje);
        }

        private static void Registrar(string origen, string tipo, string mensaje, string detalle)
        {
            // Antispam de escritura: la misma firma seguida no se repite en 30 s.
            string firma = (tipo ?? "") + "|" + (mensaje ?? "");
            DateTime ahora = DateTime.Now;
            lock (_lock)
            {
                if (firma == _firmaLog && (ahora - _vezLog).TotalSeconds < 30)
                {
                    _vezLog = ahora;
                    return;
                }
                _firmaLog = firma;
                _vezLog = ahora;
            }

            string equipo = SeguroEntorno(() => Environment.MachineName);
            string usuario = SeguroEntorno(() => Global.UsuarioActual?.Nombre) ?? "(sin sesión)";
            string version = SeguroEntorno(() => Program.VersionActual);

            // Adjuntamos los specs del equipo al detalle: así cada error guardado
            // (archivo y BD central) lleva el contexto de hardware/SO, clave para
            // diagnosticar fallas dependientes del entorno sin migrar la tabla.
            string detalleConSpecs = "[Specs] " + ObtenerSpecs() + Environment.NewLine +
                                     Environment.NewLine + (detalle ?? "null");

            EscribirArchivo(ahora, origen, equipo, usuario, version, detalleConSpecs);
            // La BD puede estar lenta o inaccesible: nunca debe bloquear ni tumbar la app.
            Task.Run(() => EscribirBaseDatos(ahora, origen, equipo, usuario, version, tipo, mensaje, detalleConSpecs));
        }

        private static void EscribirArchivo(DateTime fecha, string origen, string equipo,
            string usuario, string version, string detalle)
        {
            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(CarpetaLog);
                    File.AppendAllText(RutaLog,
                        $"==== {fecha:yyyy-MM-dd HH:mm:ss} [{origen}] equipo={equipo} usuario={usuario} v{version} ===={Environment.NewLine}" +
                        (detalle ?? "null") + Environment.NewLine + Environment.NewLine);
                }
            }
            catch
            {
                // El logger nunca debe provocar un fallo adicional.
            }
        }

        private static void EscribirBaseDatos(DateTime fecha, string origen, string equipo,
            string usuario, string version, string tipo, string mensaje, string detalle)
        {
            try
            {
                string cadena = ObtenerCadenaConexion();
                if (string.IsNullOrEmpty(cadena)) return;

                using (var conn = new SqlConnection(cadena))
                {
                    conn.Open();
                    AsegurarTabla(conn);

                    string sql = $@"
                        INSERT INTO {TablaLog}
                            (Fecha, Equipo, Usuario, Version, Origen, TipoExcepcion, Mensaje, StackTrace)
                        VALUES
                            (@Fecha, @Equipo, @Usuario, @Version, @Origen, @Tipo, @Mensaje, @Stack)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = 15;
                        cmd.Parameters.AddWithValue("@Fecha", fecha);
                        cmd.Parameters.AddWithValue("@Equipo", (object)Recortar(equipo, 100) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Usuario", (object)Recortar(usuario, 100) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Version", (object)Recortar(version, 20) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Origen", (object)Recortar(origen, 100) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Tipo", (object)Recortar(tipo, 200) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Mensaje", (object)mensaje ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Stack", (object)detalle ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Si la BD no está disponible, el respaldo en archivo ya quedó escrito.
            }
        }

        private static void AsegurarTabla(SqlConnection conn)
        {
            string sql = $@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = '{TablaLog}')
                BEGIN
                    CREATE TABLE {TablaLog} (
                        ID            INT IDENTITY(1,1) PRIMARY KEY,
                        Fecha         DATETIME       NOT NULL,
                        Equipo        NVARCHAR(100)  NULL,
                        Usuario       NVARCHAR(100)  NULL,
                        Version       NVARCHAR(20)   NULL,
                        Origen        NVARCHAR(100)  NULL,
                        TipoExcepcion NVARCHAR(200)  NULL,
                        Mensaje       NVARCHAR(MAX)  NULL,
                        StackTrace    NVARCHAR(MAX)  NULL
                    );
                    CREATE INDEX IX_{TablaLog}_Fecha ON {TablaLog}(Fecha DESC);
                END";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 15;
                cmd.ExecuteNonQuery();
            }
        }

        public static string ObtenerCadenaConexion()
        {
            try
            {
                return ConfigurationManager.ConnectionStrings["CalandriaConn"]?.ConnectionString;
            }
            catch
            {
                return null;
            }
        }

        private static string Recortar(string valor, int max)
        {
            if (string.IsNullOrEmpty(valor)) return valor;
            return valor.Length <= max ? valor : valor.Substring(0, max);
        }

        private static string SeguroEntorno(Func<string> f)
        {
            try { return f(); } catch { return null; }
        }

        // Specs del equipo: se calculan una sola vez (no cambian en la sesión).
        private static string _specs;

        /// <summary>
        /// Devuelve una línea compacta con SO, CPU, RAM y pantalla/DPI del equipo.
        /// Nunca lanza; cada dato se obtiene de forma aislada para que un fallo
        /// parcial no impida registrar el resto.
        /// </summary>
        private static string ObtenerSpecs()
        {
            if (_specs != null) return _specs;

            string so = SeguroEntorno(ObtenerSO) ?? "SO=?";
            string cpu = SeguroEntorno(ObtenerCPU) ?? "CPU=?";
            string ram = SeguroEntorno(ObtenerRAM) ?? "RAM=?";
            string pantalla = SeguroEntorno(ObtenerPantalla) ?? "Pantalla=?";

            _specs = $"{so} | {cpu} | {ram} | {pantalla}";
            return _specs;
        }

        private static string ObtenerSO()
        {
            string nombre = null;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    nombre = key?.GetValue("ProductName") as string;
                }
            }
            catch { }

            string arq = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            string ver = Environment.OSVersion.Version.ToString();
            return $"SO={(string.IsNullOrEmpty(nombre) ? "Windows" : nombre)} ({ver}) {arq}";
        }

        private static string ObtenerCPU()
        {
            string nombre = null;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    nombre = (key?.GetValue("ProcessorNameString") as string)?.Trim();
                }
            }
            catch { }

            return $"CPU={(string.IsNullOrEmpty(nombre) ? "?" : nombre)} ({Environment.ProcessorCount} lógicos)";
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        private static string ObtenerRAM()
        {
            var estado = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(estado))
            {
                double gb = estado.ullTotalPhys / (1024d * 1024d * 1024d);
                return $"RAM={gb:0.0} GB";
            }
            return "RAM=?";
        }

        private static string ObtenerPantalla()
        {
            var b = Screen.PrimaryScreen?.Bounds ?? Rectangle.Empty;
            int dpi = 96;
            try
            {
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    dpi = (int)Math.Round(g.DpiX);
                }
            }
            catch { }

            int monitores = SeguroEntero(() => Screen.AllScreens.Length, 1);
            return $"Pantalla={b.Width}x{b.Height} @{dpi}dpi ({monitores} mon)";
        }

        private static int SeguroEntero(Func<int> f, int valorPorDefecto)
        {
            try { return f(); } catch { return valorPorDefecto; }
        }
    }
}
