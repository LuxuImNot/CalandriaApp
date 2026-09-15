namespace Calandria.Api.Services
{
    /// <summary>
    /// Valida que un binario subido como "foto" sea realmente una imagen de un tipo
    /// permitido, por firma binaria (no por la extensión que declara el cliente).
    /// </summary>
    public static class ImagenValidacion
    {
        public static bool EsImagenValida(byte[] datos)
        {
            if (datos == null || datos.Length < 4) return false;

            // JPEG: FF D8 FF
            if (datos[0] == 0xFF && datos[1] == 0xD8 && datos[2] == 0xFF) return true;

            // PNG: 89 50 4E 47
            if (datos[0] == 0x89 && datos[1] == 0x50 && datos[2] == 0x4E && datos[3] == 0x47) return true;

            // WEBP: "RIFF"...."WEBP"
            if (datos.Length >= 12 && datos[0] == 0x52 && datos[1] == 0x49 && datos[2] == 0x46 && datos[3] == 0x46
                && datos[8] == 0x57 && datos[9] == 0x45 && datos[10] == 0x42 && datos[11] == 0x50) return true;

            return false;
        }
    }
}
