using System;
using System.IO;
using System.Security.Cryptography;

namespace Calandria.Api.Services
{
    /// <summary>
    /// Cifra en reposo los PDFs de identidad de trabajadores (INE/CURP/RFC/NSS) antes
    /// de guardarlos en SQL, para que una copia/backup de la BD no exponga esos
    /// documentos en claro. AES-256-CBC con IV aleatorio por valor, prefijado al
    /// resultado. Si el valor leído no está cifrado (documento subido antes de
    /// activar esto, reconocible por el encabezado "%PDF"), se devuelve tal cual
    /// para no romper descargas de lo ya guardado.
    /// </summary>
    public static class CifradoDocumentos
    {
        private static readonly byte[] FirmaPdf = { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"

        private static byte[] Clave => Convert.FromBase64String(Configuracion.DocEncryptionKey);

        public static byte[] Cifrar(byte[] claro)
        {
            if (claro == null || claro.Length == 0) return claro;
            using (var aes = Aes.Create())
            {
                aes.Key = Clave;
                aes.GenerateIV();
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        cs.Write(claro, 0, claro.Length);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] Descifrar(byte[] datos)
        {
            if (datos == null || datos.Length == 0) return datos;
            if (EsPdfSinCifrar(datos)) return datos;

            using (var aes = Aes.Create())
            {
                aes.Key = Clave;
                byte[] iv = new byte[aes.IV.Length];
                Array.Copy(datos, iv, iv.Length);
                aes.IV = iv;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        cs.Write(datos, iv.Length, datos.Length - iv.Length);
                    return ms.ToArray();
                }
            }
        }

        private static bool EsPdfSinCifrar(byte[] datos)
        {
            if (datos.Length < FirmaPdf.Length) return false;
            for (int i = 0; i < FirmaPdf.Length; i++)
                if (datos[i] != FirmaPdf[i]) return false;
            return true;
        }
    }
}
