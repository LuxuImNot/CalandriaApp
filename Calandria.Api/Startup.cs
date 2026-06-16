using System.Web.Http;
using Calandria.Api.Auth;
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
            var config = new HttpConfiguration();

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
