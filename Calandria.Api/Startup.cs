using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Calandria.Api.Auth;
using Calandria.Api.Logging;
using Newtonsoft.Json.Serialization;
using Owin;

namespace Calandria.Api
{
    /// <summary>
    /// Configuración del pipeline OWIN + Web API.
    /// </summary>
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Log de peticiones/respuestas en consola (al inicio del pipeline para
            // envolver todo, incluida la autenticación).
            app.Use<ConsoleLoggingMiddleware>();

            var config = new HttpConfiguration();

            // Loguea el detalle de cualquier excepción no controlada (causa real de
            // los HTTP 500) a consola y archivo, e incluye ese detalle en la
            // respuesta para poder diagnosticar también con un cliente.
            config.Services.Add(typeof(IExceptionLogger), new ApiExceptionLogger());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Autenticación JWT: el handler establece el principal a partir del
            // token Bearer, y el filtro global exige autorización por defecto.
            // Los endpoints públicos se marcan con [AllowAnonymous].
            config.MessageHandlers.Add(new JwtMessageHandler());
            config.Filters.Add(new AuthorizeAttribute());

            // JSON camelCase, sin XML.
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();

            app.UseWebApi(config);
        }
    }
}
