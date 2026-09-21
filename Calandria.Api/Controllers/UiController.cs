using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Sirve la interfaz web del cliente (el panel principal hospedado en WebView2).
    /// El objetivo es poder actualizar la UI sin reinstalar el cliente: el cliente
    /// pregunta la versión, y sólo descarga el HTML cuando el hash cambió.
    ///
    /// El HTML se sirve como texto y NUNCA se navega directo desde el WebView2:
    /// lo descarga el cliente con ApiClient (que ya lleva el JWT) y lo inyecta con
    /// NavigateToString. Así el token no entra a la página y no hace falta CORS.
    ///
    /// Los archivos viven en la carpeta "ui" junto al exe del servicio; la ruta se
    /// puede mover con la clave UiRuta de la configuración.
    /// </summary>
    [RoutePrefix("api/ui")]
    public class UiController : ApiController
    {
        /// <summary>
        /// Páginas que este endpoint puede servir. Whitelist explícita (igual que
        /// la whitelist de tabla en DestajosController): el nombre llega del
        /// cliente, así que no basta con RutaSegura(); sólo estos nombres existen.
        /// </summary>
        private static readonly string[] PaginasPermitidas = { "panel", "destajos", "almacen", "compras", "trabajadores", "editor-tareas", "perfiles", "obra", "distribucion-nomina", "asignar-nomina", "facturacion", "terminos", "administrativos", "evidencias", "clientes", "avance-masivo", "sembrado", "inversion" };

        private static string Carpeta =>
            Configuracion.UiRuta;

        private static string ArchivoDe(string pagina) =>
            Array.IndexOf(PaginasPermitidas, pagina) >= 0 ? pagina + ".html" : null;

        /// <summary>
        /// Resuelve el archivo pedido dentro de la carpeta de UI, impidiendo salir
        /// de ella (path traversal con ".." o rutas absolutas).
        /// </summary>
        private static string RutaSegura(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return null;
            if (nombre.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return null;

            string raiz = Path.GetFullPath(Carpeta);
            string destino = Path.GetFullPath(Path.Combine(raiz, nombre));

            // Tiene que quedar dentro de la raíz, comparando con separador final
            // para que "ui-otro" no pase por ser prefijo de "ui".
            string conBarra = raiz.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? raiz
                : raiz + Path.DirectorySeparatorChar;
            return destino.StartsWith(conBarra, StringComparison.OrdinalIgnoreCase) ? destino : null;
        }

        private static string Sha256(byte[] datos)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(datos)).Replace("-", "").ToLowerInvariant();
        }

        private sealed class ArchivoCacheado
        {
            public DateTime ModificadoUtc;
            public byte[] Datos;
            public string Hash;
        }

        // Evita releer y rehashear el HTML (hasta ~900KB) en cada /version y cada
        // /archivo: se recalcula solo si LastWriteTimeUtc cambió (p. ej. al
        // reemplazar el archivo en el servidor).
        private static readonly ConcurrentDictionary<string, ArchivoCacheado> _cache =
            new ConcurrentDictionary<string, ArchivoCacheado>(StringComparer.OrdinalIgnoreCase);

        private static ArchivoCacheado Cargar(string ruta)
        {
            var fi = new FileInfo(ruta);
            if (_cache.TryGetValue(ruta, out var actual) && actual.ModificadoUtc == fi.LastWriteTimeUtc)
                return actual;

            var datos = File.ReadAllBytes(ruta);
            var cacheado = new ArchivoCacheado
            {
                ModificadoUtc = fi.LastWriteTimeUtc,
                Datos = datos,
                Hash = Sha256(datos)
            };
            _cache[ruta] = cacheado;
            return cacheado;
        }

        /// <summary>
        /// Versión del panel: hash y tamaño. El cliente lo compara contra su copia
        /// en caché y sólo descarga si cambió. Es la llamada de cada arranque, así
        /// que debe ser barata.
        /// </summary>
        [HttpGet, Route("{pagina}/version")]
        public IHttpActionResult Version(string pagina) => VersionDe(ArchivoDe(pagina));

        /// <summary>
        /// El HTML del panel. Se devuelve con ETag para que un cliente que ya lo
        /// tenga reciba 304 y no gaste ancho de banda.
        /// </summary>
        [HttpGet, Route("{pagina}")]
        public HttpResponseMessage Archivo(string pagina) => ServirArchivo(ArchivoDe(pagina));

        /// <summary>
        /// login.html es la única página que se pide sin JWT (el login todavía no
        /// tiene token). No lleva datos de usuario embebidos, sólo plantilla/JS; el
        /// resto de páginas conserva la autorización global de la app.
        /// </summary>
        [HttpGet, Route("login/version"), AllowAnonymous]
        public IHttpActionResult VersionLogin() => VersionDe("login.html");

        [HttpGet, Route("login"), AllowAnonymous]
        public HttpResponseMessage ArchivoLogin() => ServirArchivo("login.html");

        /// <summary>
        /// actualizacion.html también se pide sin JWT: el chequeo de versión corre
        /// en segundo plano desde FormLogin antes de que el usuario haya iniciado
        /// sesión. Tampoco lleva datos de usuario embebidos.
        /// </summary>
        [HttpGet, Route("actualizacion/version"), AllowAnonymous]
        public IHttpActionResult VersionActualizacion() => VersionDe("actualizacion.html");

        [HttpGet, Route("actualizacion"), AllowAnonymous]
        public HttpResponseMessage ArchivoActualizacion() => ServirArchivo("actualizacion.html");

        private IHttpActionResult VersionDe(string archivo)
        {
            if (archivo == null) return NotFound();

            string ruta = RutaSegura(archivo);
            if (ruta == null || !File.Exists(ruta))
                return NotFound();

            var cacheado = Cargar(ruta);
            return Ok(new
            {
                hash = cacheado.Hash,
                bytes = cacheado.Datos.Length,
                modificadoUtc = cacheado.ModificadoUtc
            });
        }

        private HttpResponseMessage ServirArchivo(string archivo)
        {
            if (archivo == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            string ruta = RutaSegura(archivo);
            if (ruta == null || !File.Exists(ruta))
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var cacheado = Cargar(ruta);
            var datos = cacheado.Datos;
            string etag = "\"" + cacheado.Hash + "\"";

            var entrantes = Request.Headers.IfNoneMatch;
            if (entrantes != null)
            {
                foreach (var t in entrantes)
                    if (t.Tag == etag)
                        return Request.CreateResponse(HttpStatusCode.NotModified);
            }

            var resp = Request.CreateResponse(HttpStatusCode.OK);
            resp.Content = new ByteArrayContent(datos);
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html") { CharSet = "utf-8" };
            resp.Headers.ETag = new EntityTagHeaderValue(etag);
            return resp;
        }
    }
}
