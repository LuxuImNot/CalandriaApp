using System;
using System.Web.Http;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoint de diagnóstico (anónimo). Sirve para comprobar que el host
    /// está vivo sin tocar la base de datos. GET /api/health
    /// </summary>
    [RoutePrefix("api/health")]
    [AllowAnonymous]
    public class HealthController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            return Ok(new
            {
                servicio = "Calandria.Api",
                estado = "ok",
                utc = DateTime.UtcNow
            });
        }
    }
}
