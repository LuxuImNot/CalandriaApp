using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Calandria.Api.Auth;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Calandria.Api.Tests
{
    public class TokenServiceTests
    {
        [Fact]
        public void GenerarYValidar_RoundTrip_ConservaUsuarioRolYPermisos()
        {
            string token = TokenService.Generar("juan", "Capturista", new[] { "compras.ver", "compras.editar" }, out DateTime expiraUtc);

            ClaimsPrincipal principal = TokenService.Validar(token);

            Assert.NotNull(principal);
            Assert.Equal("juan", principal.Identity.Name);
            Assert.True(principal.IsInRole("Capturista"));
            Assert.True(principal.HasClaim("perm", "compras.ver"));
            Assert.True(principal.HasClaim("perm", "compras.editar"));
            Assert.True(expiraUtc > DateTime.UtcNow);
        }

        [Fact]
        public void Validar_TokenAlterado_DevuelveNull()
        {
            string token = TokenService.Generar("juan", "Capturista", null, out _);
            // Cambia el último caracter de la firma para invalidarla.
            char ultimo = token[token.Length - 1];
            char reemplazo = ultimo == 'A' ? 'B' : 'A';
            string alterado = token.Substring(0, token.Length - 1) + reemplazo;

            ClaimsPrincipal principal = TokenService.Validar(alterado);

            Assert.Null(principal);
        }

        [Fact]
        public void Validar_TextoQueNoEsUnToken_DevuelveNullSinLanzar()
        {
            Assert.Null(TokenService.Validar("esto-no-es-un-jwt"));
            Assert.Null(TokenService.Validar(""));
        }

        [Fact]
        public void Validar_TokenExpirado_DevuelveNull()
        {
            // Firmado con el mismo secreto que usa Configuracion.JwtSecreto en pruebas
            // (App.config), pero con vencimiento en el pasado.
            byte[] claveBytes = Encoding.UTF8.GetBytes("prueba-unidad-secreto-no-usar-en-produccion-0000");
            var creds = new SigningCredentials(new SymmetricSecurityKey(claveBytes), SecurityAlgorithms.HmacSha256);
            var expirado = new JwtSecurityToken(
                issuer: "Calandria.Api.Tests",
                audience: "Calandria.Api.Tests",
                claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, "juan") },
                notBefore: DateTime.UtcNow.AddHours(-2),
                expires: DateTime.UtcNow.AddHours(-1),
                signingCredentials: creds);
            string token = new JwtSecurityTokenHandler().WriteToken(expirado);

            ClaimsPrincipal principal = TokenService.Validar(token);

            Assert.Null(principal);
        }
    }
}
