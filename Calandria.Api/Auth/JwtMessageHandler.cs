using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Calandria.Api.Auth
{
    /// <summary>
    /// Lee el encabezado "Authorization: Bearer &lt;token&gt;", lo valida y, si es
    /// correcto, establece el principal de la petición. Combinado con el filtro
    /// global [Authorize], deja todos los endpoints protegidos por defecto.
    /// Los endpoints públicos se marcan con [AllowAnonymous].
    /// </summary>
    public sealed class JwtMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var auth = request.Headers.Authorization;
            if (auth != null &&
                auth.Scheme == "Bearer" &&
                !string.IsNullOrWhiteSpace(auth.Parameter))
            {
                ClaimsPrincipal principal = TokenService.Validar(auth.Parameter);
                if (principal != null)
                {
                    Thread.CurrentPrincipal = principal;
                    request.GetRequestContext().Principal = principal;
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
