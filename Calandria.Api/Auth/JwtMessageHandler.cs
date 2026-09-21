using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Calandria.Api.Data;

namespace Calandria.Api.Auth
{
    /// <summary>
    /// Lee el encabezado "Authorization: Bearer &lt;token&gt;", lo valida y, si es
    /// correcto, establece el principal de la petición. Combinado con el filtro
    /// global [Authorize], deja todos los endpoints protegidos por defecto.
    /// Los endpoints públicos se marcan con [AllowAnonymous].
    ///
    /// Multi-obra: si la petición trae X-Obra-Id, valida contra UsuarioObras
    /// (BD maestra) que el usuario tiene acceso y deja la cadena de conexión de
    /// esa obra en ObraContext, para que Db.Abrir() la resuelva sin que los
    /// controllers operativos reciban ni pasen ningún parámetro de obra.
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

                    // Contraseña puesta por un administrador: la cuenta entra pero
                    // no opera hasta cambiarla. Se corta aquí, en el único punto por
                    // el que pasan todas las peticiones, para que ningún cliente
                    // (escritorio, web o curl) pueda saltárselo.
                    if (principal.HasClaim("cambioClave", "1") && !EsCambioDeClave(request))
                    {
                        var bloqueo = request.CreateResponse(HttpStatusCode.Forbidden);
                        bloqueo.Content = new StringContent(
                            "Debes cambiar tu contraseña antes de continuar.");
                        bloqueo.Headers.Add("X-Cambio-Clave-Requerido", "1");
                        return Task.FromResult(bloqueo);
                    }

                    if (request.Headers.TryGetValues("X-Obra-Id", out var valores) &&
                        int.TryParse(valores.FirstOrDefault(), out int obraId))
                    {
                        string nombreBd = ResolverBaseDeDatos(principal.Identity.Name, obraId);
                        if (nombreBd == null)
                        {
                            var respuesta = request.CreateResponse(HttpStatusCode.Forbidden);
                            respuesta.Content = new StringContent("Sin acceso a la obra solicitada.");
                            return Task.FromResult(respuesta);
                        }
                        ObraContext.CadenaActual = Configuracion.CadenaConexionObra(nombreBd);
                    }
                }
            }

            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>La única ruta que puede usar un token marcado con "cambioClave".</summary>
        private static bool EsCambioDeClave(HttpRequestMessage request)
        {
            string ruta = request.RequestUri.AbsolutePath.TrimEnd('/');
            return ruta.EndsWith("/api/auth/cambiar-clave", System.StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolverBaseDeDatos(string usuario, int obraId)
        {
            using (var conn = Db.AbrirMaestra())
            using (var cmd = new SqlCommand(
                @"SELECT o.NombreBD FROM Obras o
                  JOIN UsuarioObras uo ON uo.ObraId = o.Id
                  WHERE uo.Usuario = @usuario AND o.Id = @obraId AND o.Activa = 1", conn))
            {
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@obraId", obraId);
                return cmd.ExecuteScalar() as string;
            }
        }
    }
}
