using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Calandria.Api.Auth
{
    /// <summary>
    /// Exige que el usuario autenticado esté en Configuracion.SuperAdmins. A propósito
    /// NO usa el sistema de Perfiles/Permisos (RequierePermiso): ese catálogo lo controla
    /// cada cliente para sus propios perfiles, y si "dar de alta clientes nuevos" viviera
    /// ahí, cualquier cliente podría creárselo a sí mismo. Solo lo tiene el operador de
    /// la plataforma.
    /// </summary>
    public sealed class RequiereSuperAdminAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;
            string nombre = principal?.Identity?.Name;
            return nombre != null && Configuracion.SuperAdmins.Contains(nombre);
        }
    }
}
