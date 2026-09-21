namespace Calandria.Api.Auth
{
    /// <summary>
    /// Requisitos mínimos de una contraseña. Vive en un solo lugar porque lo
    /// consultan el alta de usuarios, el restablecimiento del administrador y el
    /// cambio que hace el propio usuario: si los tres no piden lo mismo, el
    /// eslabón más flojo es el que manda.
    /// </summary>
    public static class PoliticaClave
    {
        public const int LargoMinimo = 8;

        /// <summary>
        /// Devuelve null si la contraseña sirve, o el motivo del rechazo listo
        /// para mandárselo al usuario.
        /// </summary>
        public static string Rechazo(string clave)
        {
            if (string.IsNullOrEmpty(clave))
                return "La contraseña es obligatoria.";
            if (clave.Length < LargoMinimo)
                return "La contraseña debe tener al menos " + LargoMinimo + " caracteres.";
            if (clave.Trim().Length != clave.Length)
                return "La contraseña no puede empezar ni terminar con espacios.";
            return null;
        }
    }
}
