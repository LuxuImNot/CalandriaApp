using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase para gestionar evidencias fotográficas de avance de obra
    /// </summary>
    public class GestorEvidencias
    {
        private readonly string connectionString;

        public GestorEvidencias()
        {
            connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        }

        public GestorEvidencias(string connString)
        {
            connectionString = connString;
        }

        // Helper: Detectar el nombre real de la columna de tamaño (puede variar con/ sin acentos)
        private string DetectarColumnaTamanio(SqlConnection conn)
        {
            try
            {
                string sql = @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EvidenciasFotograficas'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var col = reader.GetString(0);
                        if (Normalize(col).Equals("TAMANIOKB", StringComparison.OrdinalIgnoreCase))
                            return col; // devuelve el nombre tal cual está en la BD
                    }
                }
            }
            catch
            {
                // ignore and fallback
            }

            // fallback names (orden preferente)
            var fallbacks = new[] { "TamañoKB", "TamanioKB", "TamanoKB", "Tamanio_KB", "Tamano_KB", "TAMANIOKB" };
            foreach (var f in fallbacks)
            {
                try
                {
                    string check = $"SELECT TOP 1 [{f}] FROM EvidenciasFotograficas";
                    using (SqlCommand cmd = new SqlCommand(check, conn))
                    {
                        cmd.ExecuteScalar();
                        return f;
                    }
                }
                catch { }
            }

            // último recurso: devolver un nombre sin acento que probablemente exista
            return "TamanioKB";
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).Replace('Ñ', 'N').Replace('ñ', 'n').ToUpperInvariant();
        }

        /// <summary>
        /// Guarda una evidencia fotográfica en la base de datos
        /// </summary>
        public bool GuardarEvidencia(string manzana, string lote, string titulo, byte[] foto, string extension, string usuario = "Sistema")
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                    throw new ArgumentException("Manzana y Lote son obligatorios");

                if (string.IsNullOrWhiteSpace(titulo) || titulo.Length < 3)
                    throw new ArgumentException("El título debe tener al menos 3 caracteres");

                if (foto == null || foto.Length == 0)
                    throw new ArgumentException("La foto no puede estar vacía");

                if (foto.Length > 10 * 1024 * 1024) // 10 MB
                    throw new ArgumentException("La foto no puede superar los 10 MB");

                // Obtener prototipo
                string prototipo = ObtenerPrototipo(manzana, lote);

                // Calcular tamaño en KB
                double tamañoKB = foto.Length / 1024.0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string colTamanio = DetectarColumnaTamanio(conn);

                    string sql = $@"
                        INSERT INTO EvidenciasFotograficas 
                            (Manzana, Lote, Prototipo, TituloFoto, Foto, Extension, [{colTamanio}], UsuarioCaptura)
                        VALUES 
                            (@Manzana, @Lote, @Prototipo, @Titulo, @Foto, @Extension, @TamanioKB, @Usuario)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Manzana", manzana);
                        cmd.Parameters.AddWithValue("@Lote", lote);
                        cmd.Parameters.AddWithValue("@Prototipo", prototipo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Titulo", titulo);
                        cmd.Parameters.AddWithValue("@Foto", foto);
                        cmd.Parameters.AddWithValue("@Extension", extension ?? "jpg");
                        cmd.Parameters.AddWithValue("@TamanioKB", tamañoKB);
                        cmd.Parameters.AddWithValue("@Usuario", usuario);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar evidencia: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene la última evidencia capturada para un lote específico
        /// </summary>
        public EvidenciaInfo ObtenerUltimaEvidencia(string manzana, string lote)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string colTamanio = DetectarColumnaTamanio(conn);

                    // Usar el nombre de columna detectado y aliasarlo como 'TamanioKB' para mantener índices del reader
                    string sql = $@"
                        SELECT TOP 1
                            Id, Manzana, Lote, Prototipo, TituloFoto, 
                            Foto, Extension, [{colTamanio}] AS TamanoKB, FechaCaptura, UsuarioCaptura
                        FROM EvidenciasFotograficas
                        WHERE Manzana = @Manzana AND Lote = @Lote
                        ORDER BY FechaCaptura DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Manzana", manzana);
                        cmd.Parameters.AddWithValue("@Lote", lote);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new EvidenciaInfo
                                {
                                    Id = reader.GetInt32(0),
                                    Manzana = reader.GetString(1),
                                    Lote = reader.GetString(2),
                                    Prototipo = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                    Titulo = reader.GetString(4),
                                    FotoBytes = (byte[])reader[5],
                                    Extension = reader.IsDBNull(6) ? "jpg" : reader.GetString(6),
                                    TamañoKB = reader.GetDouble(7),
                                    Fecha = reader.GetDateTime(8),
                                    Usuario = reader.IsDBNull(9) ? "Sistema" : reader.GetString(9)
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener última evidencia: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lista todas las evidencias de un lote específico
        /// </summary>
        public List<EvidenciaInfo> ListarEvidencias(string manzana, string lote)
        {
            var evidencias = new List<EvidenciaInfo>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string colTamanio = DetectarColumnaTamanio(conn);

                    string sql = $@"
                        SELECT 
                            Id, Manzana, Lote, Prototipo, TituloFoto, 
                            Extension, [{colTamanio}], FechaCaptura, UsuarioCaptura
                        FROM EvidenciasFotograficas
                        WHERE Manzana = @Manzana AND Lote = @Lote
                        ORDER BY FechaCaptura DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Manzana", manzana);
                        cmd.Parameters.AddWithValue("@Lote", lote);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                evidencias.Add(new EvidenciaInfo
                                {
                                    Id = reader.GetInt32(0),
                                    Manzana = reader.GetString(1),
                                    Lote = reader.GetString(2),
                                    Prototipo = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                    Titulo = reader.GetString(4),
                                    Extension = reader.IsDBNull(5) ? "jpg" : reader.GetString(5),
                                    TamañoKB = reader.GetDouble(6),
                                    Fecha = reader.GetDateTime(7),
                                    Usuario = reader.IsDBNull(8) ? "Sistema" : reader.GetString(8)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar evidencias: {ex.Message}", ex);
            }

            return evidencias;
        }

        /// <summary>
        /// Obtiene la foto completa de una evidencia específica
        /// </summary>
        public byte[] ObtenerFoto(int idEvidencia)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = "SELECT Foto FROM EvidenciasFotograficas WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", idEvidencia);

                        var result = cmd.ExecuteScalar();
                        return result as byte[];
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener foto: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina una evidencia de la base de datos
        /// </summary>
        public bool EliminarEvidencia(int idEvidencia)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = "DELETE FROM EvidenciasFotograficas WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", idEvidencia);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar evidencia: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Redimensiona una imagen si excede las dimensiones máximas especificadas
        /// </summary>
        public static byte[] RedimensionarImagen(byte[] imagenBytes, int maxAncho = 1024, int maxAlto = 768)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(imagenBytes))
                {
                    using (Image imagenOriginal = Image.FromStream(ms))
                    {
                        // Verificar si necesita redimensionamiento
                        if (imagenOriginal.Width <= maxAncho && imagenOriginal.Height <= maxAlto)
                        {
                            return imagenBytes; // No necesita redimensionamiento
                        }

                        // Calcular nuevas dimensiones manteniendo la proporción
                        double ratioAncho = (double)maxAncho / imagenOriginal.Width;
                        double ratioAlto = (double)maxAlto / imagenOriginal.Height;
                        double ratio = Math.Min(ratioAncho, ratioAlto);

                        int nuevoAncho = (int)(imagenOriginal.Width * ratio);
                        int nuevoAlto = (int)(imagenOriginal.Height * ratio);

                        // Crear nueva imagen redimensionada
                        using (Bitmap imagenRedimensionada = new Bitmap(nuevoAncho, nuevoAlto))
                        {
                            using (Graphics graphics = Graphics.FromImage(imagenRedimensionada))
                            {
                                graphics.CompositingQuality = CompositingQuality.HighQuality;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.SmoothingMode = SmoothingMode.HighQuality;

                                graphics.DrawImage(imagenOriginal, 0, 0, nuevoAncho, nuevoAlto);
                            }

                            // Convertir a bytes con calidad 85
                            return ConvertirImagenABytes(imagenRedimensionada, 85L);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al redimensionar imagen: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convierte una imagen a array de bytes en formato JPEG
        /// </summary>
        public static byte[] ConvertirImagenABytes(Image imagen, long calidad = 85L)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // Configurar parámetros de calidad JPEG
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, calidad);

                    var jpegCodec = GetEncoder(ImageFormat.Jpeg);

                    if (jpegCodec != null)
                    {
                        imagen.Save(ms, jpegCodec, encoderParameters);
                    }
                    else
                    {
                        // Fallback si no se encuentra codec JPEG
                        imagen.Save(ms, ImageFormat.Jpeg);
                    }

                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al convertir imagen a bytes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convierte un array de bytes a una imagen
        /// </summary>
        public static Image ConvertirBytesAImagen(byte[] bytes)
        {
            try
            {
                if (bytes == null || bytes.Length == 0)
                    return null;

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    // Crear imagen desde el stream y devolver una copia que no dependa del stream
                    using (Image img = Image.FromStream(ms))
                    {
                        var copia = new Bitmap(img);
                        return copia;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al convertir bytes a imagen: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el codec de imagen para un formato específico
        /// </summary>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// Obtiene el prototipo de una casa según su manzana y lote
        /// </summary>
        private string ObtenerPrototipo(string manzana, string lote)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);

                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Obtiene estadísticas generales de evidencias
        /// </summary>
        public Dictionary<string, object> ObtenerEstadisticas()
        {
            var estadisticas = new Dictionary<string, object>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string colTamanio = DetectarColumnaTamanio(conn);

                    string sql = $@"
                        SELECT 
                            COUNT(*) AS TotalEvidencias,
                            COUNT(DISTINCT Manzana) AS ManzanasConEvidencias,
                            COUNT(DISTINCT CONCAT(Manzana, '-', Lote)) AS LotesConEvidencias,
                            ISNULL(SUM([{colTamanio}]) / 1024.0, 0) AS TotalMB,
                            ISNULL(AVG([{colTamanio}]), 0) AS PromedioKB,
                            MIN(FechaCaptura) AS PrimeraEvidencia,
                            MAX(FechaCaptura) AS UltimaEvidencia
                        FROM EvidenciasFotograficas";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            estadisticas["TotalEvidencias"] = reader.GetInt32(0);
                            estadisticas["ManzanasConEvidencias"] = reader.GetInt32(1);
                            estadisticas["LotesConEvidencias"] = reader.GetInt32(2);
                            estadisticas["TotalMB"] = reader.GetDouble(3);
                            estadisticas["PromedioKB"] = reader.GetDouble(4);
                            estadisticas["PrimeraEvidencia"] = reader.IsDBNull(5) ? (object)null : reader.GetDateTime(5);
                            estadisticas["UltimaEvidencia"] = reader.IsDBNull(6) ? (object)null : reader.GetDateTime(6);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener estadísticas: {ex.Message}", ex);
            }

            return estadisticas;
        }
    }

    /// <summary>
    /// Clase para almacenar información de una evidencia fotográfica
    /// </summary>
    public class EvidenciaInfo
    {
        public int Id { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Titulo { get; set; }
        public byte[] FotoBytes { get; set; }
        public string Extension { get; set; }
        public double TamañoKB { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }

        public string TamañoFormateado
        {
            get
            {
                if (TamañoKB < 1024)
                    return $"{TamañoKB:F1} KB";
                else
                    return $"{TamañoKB / 1024:F2} MB";
            }
        }

        public string FechaFormateada => Fecha.ToString("dd/MM/yyyy HH:mm");
    }
}
