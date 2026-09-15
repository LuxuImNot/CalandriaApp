using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Calandria.Api.Auth
{
    /// <summary>
    /// Cliente (empresa dueña de una o más obras) del usuario autenticado en la
    /// petición actual, leído del claim "cliente" del JWT (ver
    /// TokenService.Generar). Aísla los datos de cada cliente cuando el mismo
    /// servidor hospeda a varios: Obras, Perfiles y Usuarios se filtran por esto.
    /// </summary>
    public static class ClienteActual
    {
        public static int Id(IPrincipal principal)
        {
            var claims = principal as ClaimsPrincipal;
            var valor = claims?.FindFirst("cliente")?.Value;
            if (!int.TryParse(valor, out int id))
                throw new InvalidOperationException("El token no trae el claim 'cliente'; vuelve a iniciar sesión.");
            return id;
        }
    }
}
