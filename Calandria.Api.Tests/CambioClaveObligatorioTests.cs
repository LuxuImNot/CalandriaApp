using System;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using Calandria.Api.Auth;
using Xunit;

namespace Calandria.Api.Tests
{
    // El cambio obligatorio se apoya en dos piezas chicas de las que depende todo
    // lo demás: el claim "cambioClave" en el token y la ruta que JwtMessageHandler
    // deja pasar pese a ese claim. Si EsCambioDeClave() empieza a decir que sí de
    // más, el candado queda abierto sin que nada más falle.
    public class CambioClaveObligatorioTests
    {
        private static bool EsCambioDeClave(string url)
        {
            MethodInfo metodo = typeof(JwtMessageHandler).GetMethod(
                "EsCambioDeClave", BindingFlags.NonPublic | BindingFlags.Static);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            return (bool)metodo.Invoke(null, new object[] { request });
        }

        [Theory]
        [InlineData("http://localhost:8733/api/auth/cambiar-clave")]
        [InlineData("http://localhost:8733/api/auth/cambiar-clave/")]
        [InlineData("http://localhost:8733/API/AUTH/CAMBIAR-CLAVE")]
        public void LaRutaDeCambio_SiPasa(string url)
        {
            Assert.True(EsCambioDeClave(url));
        }

        [Theory]
        [InlineData("http://localhost:8733/api/auth/login")]
        [InlineData("http://localhost:8733/api/auth/mi-perfil")]
        [InlineData("http://localhost:8733/api/destajos")]
        [InlineData("http://localhost:8733/api/perfiles/usuarios")]
        // El sufijo no debe poder usarse para colar otra ruta.
        [InlineData("http://localhost:8733/api/destajos?x=/api/auth/cambiar-clave")]
        [InlineData("http://localhost:8733/api/auth/cambiar-clave-otra-cosa")]
        public void TodoLoDemas_NoPasa(string url)
        {
            Assert.False(EsCambioDeClave(url));
        }

        [Fact]
        public void TokenConCambioPendiente_TraeElClaim()
        {
            string token = TokenService.Generar(
                "juan", "Admin", new[] { "sistema.perfiles" }, clienteId: 1,
                expiraUtc: out DateTime _, cambioClaveRequerido: true);

            ClaimsPrincipal principal = TokenService.Validar(token);
            Assert.NotNull(principal);
            Assert.True(principal.HasClaim("cambioClave", "1"));
        }

        [Fact]
        public void TokenNormal_NoTraeElClaim()
        {
            string token = TokenService.Generar(
                "juan", "Admin", new[] { "sistema.perfiles" }, clienteId: 1,
                expiraUtc: out DateTime _);

            ClaimsPrincipal principal = TokenService.Validar(token);
            Assert.NotNull(principal);
            Assert.False(principal.HasClaim("cambioClave", "1"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("corta1")]          // 6 caracteres
        [InlineData("  conespacios  ")] // espacios al inicio/final
        public void PoliticaClave_RechazaLasMalas(string clave)
        {
            Assert.NotNull(PoliticaClave.Rechazo(clave));
        }

        [Theory]
        [InlineData("Temporal123")]
        [InlineData("ocho1234")]        // exactamente el mínimo
        public void PoliticaClave_AceptaLasBuenas(string clave)
        {
            Assert.Null(PoliticaClave.Rechazo(clave));
        }
    }
}
