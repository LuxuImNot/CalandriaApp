using System.Web.Http;
using Owin;

namespace Calandria.Api
{
    /// <summary>
    /// Configuración del pipeline OWIN + Web API. La autenticación JWT se
    /// agregará aquí en la siguiente fase (app.UseJwtBearerAuthentication...).
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

            // Respuestas JSON por defecto (sin XML).
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            app.UseWebApi(config);
        }
    }
}
