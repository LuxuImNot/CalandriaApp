using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase para gestionar evidencias fotográficas de avance de obra.
    ///
    /// Migrado a Web API: el acceso a datos vive en EvidenciasController
    /// (api/evidencias). Las firmas públicas se conservan para no tocar los forms
    /// que la consumen; los helpers de imagen (estáticos) siguen ejecutándose en
    /// el cliente. Los constructores conservan su firma por compatibilidad pero ya
    /// no usan la cadena de conexión.
    /// </summary>
    public class GestorEvidencias
    {
        public GestorEvidencias()
        {
        }

        public GestorEvidencias(string connString)
        {
        }

        /// <summary>
        /// Guarda una evidencia fotográfica en la base de datos.
        /// </summary>
        public bool GuardarEvidencia(string manzana, string lote, string titulo, byte[] foto, string extension, string usuario = "Sistema")
        {
            try
            {
                // Validaciones (fallo rápido en el cliente, el servidor las repite)
                if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                    throw new ArgumentException("Manzana y Lote son obligatorios");

                if (string.IsNullOrWhiteSpace(titulo) || titulo.Length < 3)
                    throw new ArgumentException("El título debe tener al menos 3 caracteres");

                if (foto == null || foto.Length == 0)
                    throw new ArgumentException("La foto no puede estar vacía");

                if (foto.Length > 10 * 1024 * 1024) // 10 MB
                    throw new ArgumentException("La foto no puede superar los 10 MB");

                return ApiClient.Post<bool>("/api/evidencias", new
                {
                    Manzana = manzana,
                    Lote = lote,
                    Titulo = titulo,
                    FotoBase64 = Convert.ToBase64String(foto),
                    Extension = extension ?? "jpg",
                    Usuario = usuario ?? "Sistema"
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar evidencia: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene la última evidencia capturada para un lote específico.
        /// </summary>
        public EvidenciaInfo ObtenerUltimaEvidencia(string manzana, string lote)
        {
            try
            {
                var dto = ApiClient.Get<EvidenciaApi>(
                    $"/api/evidencias/ultima?manzana={Uri.EscapeDataString(manzana ?? "")}&lote={Uri.EscapeDataString(lote ?? "")}");

                if (dto == null)
                    return null;

                var info = MapDto(dto);
                info.FotoBytes = string.IsNullOrEmpty(dto.FotoBase64)
                    ? null
                    : Convert.FromBase64String(dto.FotoBase64);
                return info;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener última evidencia: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lista todas las evidencias de un lote específico.
        /// </summary>
        public List<EvidenciaInfo> ListarEvidencias(string manzana, string lote)
        {
            try
            {
                var dtos = ApiClient.Get<List<EvidenciaApi>>(
                    $"/api/evidencias?manzana={Uri.EscapeDataString(manzana ?? "")}&lote={Uri.EscapeDataString(lote ?? "")}");

                var evidencias = new List<EvidenciaInfo>();
                if (dtos != null)
                    foreach (var dto in dtos)
                        evidencias.Add(MapDto(dto));
                return evidencias;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar evidencias: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene la foto completa de una evidencia específica.
        /// </summary>
        public byte[] ObtenerFoto(int idEvidencia)
        {
            try
            {
                return ApiClient.GetBytes($"/api/evidencias/{idEvidencia}/foto");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener foto: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina una evidencia de la base de datos.
        /// </summary>
        public bool EliminarEvidencia(int idEvidencia)
        {
            try
            {
                return ApiClient.Post<bool>($"/api/evidencias/{idEvidencia}/eliminar", null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar evidencia: {ex.Message}", ex);
            }
        }

        private static EvidenciaInfo MapDto(EvidenciaApi dto)
        {
            return new EvidenciaInfo
            {
                Id = dto.Id,
                Manzana = dto.Manzana,
                Lote = dto.Lote,
                Prototipo = dto.Prototipo ?? "",
                Titulo = dto.Titulo,
                Extension = dto.Extension ?? "jpg",
                TamañoKB = dto.TamanioKB,
                Fecha = dto.Fecha,
                Usuario = dto.Usuario ?? "Sistema"
            };
        }

        /// <summary>
        /// Redimensiona una imagen si excede las dimensiones máximas especificadas.
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
        /// Convierte una imagen a array de bytes en formato JPEG.
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
        /// Convierte un array de bytes a una imagen.
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
        /// Obtiene el codec de imagen para un formato específico.
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
    }

    /// <summary>
    /// Clase para almacenar información de una evidencia fotográfica.
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
