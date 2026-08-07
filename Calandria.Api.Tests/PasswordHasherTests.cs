using DynamicSepticSystem;
using Xunit;

namespace Calandria.Api.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Verificar_AceptaLaClaveCorrecta()
        {
            string hash = PasswordHasher.Hash("Obra2026!");

            bool ok = PasswordHasher.Verificar("Obra2026!", hash, out bool necesitaRehash);

            Assert.True(ok);
            Assert.False(necesitaRehash);
        }

        [Fact]
        public void Verificar_RechazaUnaClaveIncorrecta()
        {
            string hash = PasswordHasher.Hash("Obra2026!");

            bool ok = PasswordHasher.Verificar("otra-clave", hash, out _);

            Assert.False(ok);
        }

        [Fact]
        public void Hash_UsaSalAleatoria_MismaClaveProduceHashesDistintos()
        {
            string hash1 = PasswordHasher.Hash("Obra2026!");
            string hash2 = PasswordHasher.Hash("Obra2026!");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Verificar_AceptaHashHeredadoSha256_YPideRehash()
        {
            // Formato legacy: SHA-256 sin sal, 64 hex chars (ver PasswordHasher.Verificar).
            string sha256DeHola = ComputarSha256Hex("hola123");

            bool ok = PasswordHasher.Verificar("hola123", sha256DeHola, out bool necesitaRehash);

            Assert.True(ok);
            Assert.True(necesitaRehash);
        }

        [Fact]
        public void Verificar_HashAlmacenadoCorrupto_NoLanzaYRechaza()
        {
            bool ok = PasswordHasher.Verificar("cualquiera", "pbkdf2$no-es-numero$abc$def", out _);

            Assert.False(ok);
        }

        [Fact]
        public void Verificar_ClaveOAlmacenadoNulos_Rechaza()
        {
            Assert.False(PasswordHasher.Verificar(null, "algo", out _));
            Assert.False(PasswordHasher.Verificar("algo", null, out _));
            Assert.False(PasswordHasher.Verificar("algo", "", out _));
        }

        private static string ComputarSha256Hex(string texto)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(texto));
                return System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
