using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Calandria.Api.Auth
{
    /// <summary>
    /// Exige que el JWT de la petición traiga el permiso indicado (claim tipo
    /// "perm", ver <see cref="TokenService.Generar"/>). Se apoya en el filtro
    /// global [Authorize] (Startup.cs) para autenticación; esto solo agrega la
    /// verificación de autorización granular.
    /// </summary>
    public sealed class RequierePermisoAttribute : AuthorizeAttribute
    {
        private readonly string _permiso;

        public RequierePermisoAttribute(string permiso)
        {
            _permiso = permiso;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;
            return principal != null && principal.HasClaim("perm", _permiso);
        }
    }
}
