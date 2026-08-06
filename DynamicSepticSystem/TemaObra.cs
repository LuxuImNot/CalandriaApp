using System;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Trae la paleta de acento (y el logo, si tiene) de la obra activa, para que
    /// cada pantalla web la aplique sobre sus variables CSS --brass/--brass-2/
    /// --brass-soft. Cosmético: si falla, la pantalla se queda con el tema café
    /// por defecto, nunca bloquea la carga.
    /// </summary>
    public static class TemaObra
    {
        public static object ObtenerParaPush()
        {
            if (!Global.ObraActualId.HasValue) return null;
            try
            {
                var tema = ApiClient.Get<TemaObraApi>($"/api/obras/{Global.ObraActualId}/tema");
                if (tema == null) return null;

                string logoDataUri = null;
                if (tema.TieneLogo)
                {
                    var bytes = ApiClient.GetBytes($"/api/obras/{Global.ObraActualId}/logo");
                    if (bytes != null && bytes.Length > 0)
                    {
                        string mime = MimeDeExtension(tema.LogoExtension);
                        logoDataUri = $"data:{mime};base64," + Convert.ToBase64String(bytes);
                    }
                }

                return new
                {
                    tipo = "tema",
                    colorPrimario = tema.ColorPrimario,
                    colorSecundario = tema.ColorSecundario,
                    colorSuave = tema.ColorSuave,
                    logo = logoDataUri
                };
            }
            catch (Exception ex)
            {
                ErrorLogger.RegistrarMensaje("TemaObra", "No se pudo cargar el tema de la obra: " + ex.Message);
                return null;
            }
        }

        internal static string MimeDeExtension(string ext)
        {
            switch ((ext ?? "").Trim('.').ToLowerInvariant())
            {
                case "jpg":
                case "jpeg": return "image/jpeg";
                case "webp": return "image/webp";
                default: return "image/png";
            }
        }
    }
}
