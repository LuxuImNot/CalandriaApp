using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Calandria.Api.Auth
{
    /// <summary>
    /// Emite y valida tokens JWT firmados con clave simétrica (HMAC-SHA256).
    /// El secreto vive solo en la configuración del servidor.
    /// </summary>
    public static class TokenService
    {
        private static SymmetricSecurityKey Clave()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(Configuracion.JwtSecreto);
            if (bytes.Length < 32)
                throw new InvalidOperationException("JwtSecreto debe tener al menos 32 bytes (256 bits).");
            return new SymmetricSecurityKey(bytes);
        }

        public static string Generar(string usuario, string rol, IEnumerable<string> permisos, int clienteId, out DateTime expiraUtc)
        {
            expiraUtc = DateTime.UtcNow.AddHours(Configuracion.JwtHorasVigencia);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario),
                new Claim(ClaimTypes.Name, usuario),
                new Claim(ClaimTypes.Role, rol ?? string.Empty),
                new Claim("cliente", clienteId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            if (permisos != null)
                foreach (var permiso in permisos)
                    claims.Add(new Claim("perm", permiso));

            var creds = new SigningCredentials(Clave(), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: Configuracion.JwtIssuer,
                audience: Configuracion.JwtAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiraUtc,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>Valida un token y devuelve el principal, o null si es inválido.</summary>
        public static ClaimsPrincipal Validar(string token)
        {
            var parametros = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Configuracion.JwtIssuer,
                ValidateAudience = true,
                ValidAudience = Configuracion.JwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = Clave(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            try
            {
                return new JwtSecurityTokenHandler()
                    .ValidateToken(token, parametros, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
