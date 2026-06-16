using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Hashing de contraseñas con PBKDF2 (Rfc2898DeriveBytes, HMAC-SHA256),
    /// sal aleatoria por usuario y comparación de tiempo constante.
    ///
    /// Formato almacenado en Usuarios.ClaveHash:
    ///     pbkdf2$&lt;iteraciones&gt;$&lt;salBase64&gt;$&lt;hashBase64&gt;
    ///
    /// Compatibilidad: los hashes heredados (SHA-256 sin sal, 64 hex) se siguen
    /// validando con <see cref="Verificar"/>, que marca <c>necesitaRehash=true</c>
    /// para migrarlos a PBKDF2 tras un inicio de sesión correcto. Así ningún
    /// usuario existente queda fuera ni tiene que restablecer su contraseña.
    /// </summary>
    public static class PasswordHasher
    {
        private const int Iteraciones = 100_000;
        private const int TamSal = 16;
        private const int TamHash = 32;
        private const string Etiqueta = "pbkdf2";

        /// <summary>Genera el hash PBKDF2 (con sal nueva) de una contraseña.</summary>
        public static string Hash(string clave)
        {
            if (clave == null) throw new ArgumentNullException(nameof(clave));

            byte[] sal = new byte[TamSal];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(sal);

            byte[] hash = DerivarPBKDF2(clave, sal, Iteraciones, TamHash);

            return string.Join("$",
                Etiqueta,
                Iteraciones.ToString(),
                Convert.ToBase64String(sal),
                Convert.ToBase64String(hash));
        }

        /// <summary>
        /// Verifica una contraseña contra el valor almacenado. Acepta tanto el
        /// formato PBKDF2 nuevo como el hash heredado SHA-256 sin sal.
        /// </summary>
        /// <param name="necesitaRehash">
        /// true cuando la verificación fue correcta pero el hash almacenado está en
        /// formato heredado; el llamador debería re-guardar <see cref="Hash"/>.
        /// </param>
        public static bool Verificar(string clave, string almacenado, out bool necesitaRehash)
        {
            necesitaRehash = false;
            if (clave == null || string.IsNullOrEmpty(almacenado)) return false;

            // Formato heredado: 64 caracteres hexadecimales (SHA-256 sin sal).
            if (almacenado.Length == 64 && Regex.IsMatch(almacenado, "^[0-9a-fA-F]+$"))
            {
                bool okLegacy = IgualdadConstante(Sha256Hex(clave), almacenado.ToLowerInvariant());
                necesitaRehash = okLegacy;
                return okLegacy;
            }

            // Formato PBKDF2: pbkdf2$iter$sal$hash
            string[] partes = almacenado.Split('$');
            if (partes.Length != 4 || partes[0] != Etiqueta) return false;

            if (!int.TryParse(partes[1], out int iter) || iter <= 0) return false;

            byte[] sal, esperado;
            try
            {
                sal = Convert.FromBase64String(partes[2]);
                esperado = Convert.FromBase64String(partes[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] calculado = DerivarPBKDF2(clave, sal, iter, esperado.Length);
            return IgualdadConstante(calculado, esperado);
        }

        private static byte[] DerivarPBKDF2(string clave, byte[] sal, int iter, int tam)
        {
            using (var kdf = new Rfc2898DeriveBytes(clave, sal, iter, HashAlgorithmName.SHA256))
                return kdf.GetBytes(tam);
        }

        private static string Sha256Hex(string texto)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static bool IgualdadConstante(string a, string b)
        {
            return IgualdadConstante(Encoding.UTF8.GetBytes(a ?? ""), Encoding.UTF8.GetBytes(b ?? ""));
        }

        private static bool IgualdadConstante(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int dif = 0;
            for (int i = 0; i < a.Length; i++)
                dif |= a[i] ^ b[i];
            return dif == 0;
        }
    }
}
