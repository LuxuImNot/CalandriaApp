using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Cliente HTTP hacia Calandria.Api. Centraliza la URL base, el token JWT
    /// obtenido al iniciar sesión y la (de)serialización JSON.
    ///
    /// Durante la migración la app es híbrida: las pantallas ya migradas usan
    /// este cliente; las demás siguen pegándole directo a SQL. Si el API no está
    /// disponible, <see cref="Login"/> falla de forma controlada y solo las
    /// pantallas migradas se ven afectadas.
    /// </summary>
    public static class ApiClient
    {
        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static string BaseUrl =>
            (ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:8733")
            .TrimEnd('/');

        /// <summary>Token JWT de la sesión actual (vacío si no se ha autenticado por API).</summary>
        public static string Token { get; private set; }

        /// <summary>Vencimiento (UTC) del token actual, o null si no hay sesión API.</summary>
        public static DateTime? ExpiraUtc { get; private set; }

        public static bool Autenticado => !string.IsNullOrEmpty(Token);

        /// <summary>
        /// Autentica contra /api/auth/login y guarda el token. Lanza excepción si
        /// las credenciales son inválidas o el API no responde.
        /// </summary>
        public static LoginResponseApi Login(string usuario, string clave)
        {
            string body = JsonConvert.SerializeObject(new { usuario, clave });
            using (var content = new StringContent(body, Encoding.UTF8, "application/json"))
            using (var resp = Http.PostAsync(BaseUrl + "/api/auth/login", content)
                                  .GetAwaiter().GetResult())
            {
                string json = LeerOLanzar(resp);
                var login = JsonConvert.DeserializeObject<LoginResponseApi>(json);
                Token = login?.Token;
                ExpiraUtc = login?.ExpiraUtc;
                return login;
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado y se queda con el token
        /// nuevo que devuelve el API. Hay que reemplazarlo sí o sí: el token con
        /// el que se llega aquí puede traer la marca de cambio pendiente, que lo
        /// deja inservible para cualquier otra ruta.
        /// </summary>
        public static LoginResponseApi CambiarClave(string claveActual, string claveNueva)
        {
            var resp = Post<LoginResponseApi>("/api/auth/cambiar-clave",
                                              new { claveActual, claveNueva });
            if (!string.IsNullOrEmpty(resp?.Token))
            {
                Token = resp.Token;
                ExpiraUtc = resp.ExpiraUtc;
            }
            return resp;
        }

        public static void CerrarSesion()
        {
            Token = null;
            ExpiraUtc = null;
        }

        /// <summary>
        /// Agrega a la petición el Bearer de sesión y, si hay una obra activa,
        /// el header X-Obra-Id que el API usa para resolver contra qué base de
        /// datos de obra correr la consulta.
        /// </summary>
        private static void AplicarEncabezados(HttpRequestMessage req)
        {
            if (Autenticado)
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            if (Global.ObraActualId.HasValue)
                req.Headers.Add("X-Obra-Id", Global.ObraActualId.Value.ToString());
        }

        /// <summary>GET tipado a una ruta relativa (p. ej. "/api/almacen/manzanas").</summary>
        public static T Get<T>(string rutaRelativa)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + rutaRelativa))
            {
                AplicarEncabezados(req);

                using (var resp = Http.SendAsync(req).GetAwaiter().GetResult())
                {
                    string json = LeerOLanzar(resp);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
        }

        /// <summary>
        /// GET de contenido binario (p. ej. el PDF de un recibo). Devuelve null
        /// si el API responde 404 (sin contenido).
        /// </summary>
        public static byte[] GetBytes(string rutaRelativa)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + rutaRelativa))
            {
                AplicarEncabezados(req);

                using (var resp = Http.SendAsync(req).GetAwaiter().GetResult())
                {
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                        return null;
                    if (!resp.IsSuccessStatusCode)
                        throw CrearExcepcion(resp);
                    return resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                }
            }
        }

        /// <summary>
        /// POST de un cuerpo JSON a una ruta relativa, sin esperar contenido de
        /// respuesta tipado (p. ej. guardar una asignación). Lanza si el API
        /// responde con error.
        /// </summary>
        public static void Post(string rutaRelativa, object cuerpo)
        {
            EnviarPost(rutaRelativa, cuerpo);
        }

        /// <summary>POST de un cuerpo JSON que devuelve un resultado tipado.</summary>
        public static T Post<T>(string rutaRelativa, object cuerpo)
        {
            string json = EnviarPost(rutaRelativa, cuerpo);
            return string.IsNullOrWhiteSpace(json)
                ? default(T)
                : JsonConvert.DeserializeObject<T>(json);
        }

        private static string EnviarPost(string rutaRelativa, object cuerpo)
        {
            string body = JsonConvert.SerializeObject(cuerpo);
            using (var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl + rutaRelativa))
            {
                AplicarEncabezados(req);
                req.Content = new StringContent(body, Encoding.UTF8, "application/json");

                using (var resp = Http.SendAsync(req).GetAwaiter().GetResult())
                {
                    return LeerOLanzar(resp);
                }
            }
        }

        /// <summary>
        /// Lee el cuerpo de una respuesta exitosa o lanza <see cref="ApiException"/>
        /// con el código de estado y el mensaje del servidor si falló.
        /// </summary>
        private static string LeerOLanzar(HttpResponseMessage resp)
        {
            if (!resp.IsSuccessStatusCode)
                throw CrearExcepcion(resp);
            return resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private static ApiException CrearExcepcion(HttpResponseMessage resp)
        {
            string cuerpo = "";
            try { cuerpo = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult(); }
            catch { /* sin cuerpo */ }
            return new ApiException(resp.StatusCode, cuerpo);
        }
    }

    /// <summary>
    /// Error devuelto por el API (código de estado distinto de 2xx). Permite a los
    /// formularios distinguir casos como 409 Conflict (clave duplicada).
    /// </summary>
    public sealed class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string Cuerpo { get; }

        public ApiException(HttpStatusCode statusCode, string cuerpo)
            : base(statusCode == HttpStatusCode.Forbidden
                ? "No tienes permiso para realizar esta acción."
                : $"El API respondió {(int)statusCode} ({statusCode}).")
        {
            StatusCode = statusCode;
            Cuerpo = cuerpo;
        }

        /// <summary>
        /// El texto que el API mandó en el cuerpo (Web API serializa BadRequest("…")
        /// como {"Message":"…"}), o <see cref="Exception.Message"/> si no vino nada
        /// legible. Media docena de formularios traen esta misma desserialización
        /// copiada; los nuevos usan esta y los viejos pueden migrar cuando se toquen.
        /// </summary>
        public string Mensaje
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Cuerpo)) return Message;
                try
                {
                    var cuerpo = Newtonsoft.Json.JsonConvert
                        .DeserializeAnonymousType(Cuerpo, new { Message = "" });
                    return string.IsNullOrEmpty(cuerpo?.Message) ? Cuerpo : cuerpo.Message;
                }
                catch { return Cuerpo; }
            }
        }
    }

    // ---- Modelos de transporte (coinciden con los DTO del API) ----

    public sealed class LoginResponseApi
    {
        public string Token { get; set; }
        public string Usuario { get; set; }
        public string Rol { get; set; }
        public List<string> Permisos { get; set; }
        public DateTime ExpiraUtc { get; set; }
        public bool EsSuperAdmin { get; set; }

        /// <summary>
        /// La contraseña la puso un administrador. El token que viene con esto en
        /// true solo sirve para /api/auth/cambiar-clave: el resto del API lo
        /// rechaza con 403 hasta que el usuario ponga una contraseña propia.
        /// </summary>
        public bool CambioClaveRequerido { get; set; }
    }

    /// <summary>Si el usuario autenticado ya aceptó la versión vigente de Términos/Privacidad (api/terminos/estado).</summary>
    public sealed class EstadoTerminosApi
    {
        public bool RequiereAceptar { get; set; }
        public string Hash { get; set; }
    }

    public sealed class MaterialCasaApi
    {
        public string Ruta { get; set; }
        public string Destajo { get; set; }
        public string Material { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string Cuadrilla { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string Prototipo { get; set; }
    }

    // ---- Nómina ----

    public sealed class MontoTrabajadorApi
    {
        public int? IdTrabajador { get; set; }
        public string NombreTrabajador { get; set; }
        public decimal Monto { get; set; }
    }

    public sealed class AsignacionNominaApi
    {
        public string CodigoCuadrilla { get; set; }
        public List<MontoTrabajadorApi> Montos { get; set; } = new List<MontoTrabajadorApi>();
    }

    public sealed class LineaAsignacionNominaApi
    {
        public int? IdTrabajador { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public bool EsJefe { get; set; }
        public decimal Monto { get; set; }
    }

    public sealed class GuardarAsignacionRequestApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public int NodoId { get; set; }
        public string NombreTarea { get; set; }
        public string CodigoCuadrilla { get; set; }
        public decimal TotalDistribuir { get; set; }
        public List<LineaAsignacionNominaApi> Lineas { get; set; } = new List<LineaAsignacionNominaApi>();
    }

    public sealed class EliminarAsignacionRequestApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public List<int> NodoIds { get; set; } = new List<int>();
    }

    // ---- Compras · Proveedores ----

    public sealed class ProveedorApi
    {
        public string Folio { get; set; }
        public string ClaveUnica { get; set; }
        public string Nombre { get; set; }
        public string Rfc { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
    }

    public sealed class ProveedorCreadoApi
    {
        public string Folio { get; set; }
    }

    // ---- Compras · Órdenes de compra ----

    public sealed class CasaOrdenApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
    }

    public sealed class DetalleOrdenApi
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal ImporteTotal { get; set; }
        public string Familia { get; set; }
    }

    public sealed class FolioOrdenApi
    {
        public string FolioOC { get; set; }
    }

    // ---- Compras · catálogos de material ----

    public sealed class CatalogoMaterialApi
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public string Familia { get; set; }
        public decimal Precio { get; set; }

        // true si el insumo tiene clave de almacén (del árbol o resuelta por catálogo);
        // false si quedó sin clave (se rastreará por nombre). Solo lo llena catalogo-destajos.
        public bool ClaveResuelta { get; set; } = true;
    }

    public sealed class PendienteMaterialApi
    {
        public string Clave { get; set; }
        public decimal CantidadPendiente { get; set; }
    }

    // ---- Conciliación de factura (CFDI) contra la OC ----

    public sealed class ConciliarFacturaResponseApi
    {
        public string FolioOC { get; set; }
        public string Emisor { get; set; }
        public string Rfc { get; set; }
        public string Uuid { get; set; }
        public decimal TotalFactura { get; set; }
        public bool UsoIA { get; set; }
        public string Aviso { get; set; }
        public List<LineaConciliacionApi> Lineas { get; set; } = new List<LineaConciliacionApi>();
        public List<FacturaConceptoApi> SinAsignar { get; set; } = new List<FacturaConceptoApi>();
    }

    public sealed class LineaConciliacionApi
    {
        public int IdDetalle { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal CantidadOC { get; set; }
        public decimal CantidadFacturada { get; set; }
        public decimal Diferencia { get; set; }
        public string DescripcionFactura { get; set; }
        public string Origen { get; set; }
        public double Confianza { get; set; }
    }

    public sealed class FacturaConceptoApi
    {
        public string NoIdentificacion { get; set; }
        public string ClaveProdServ { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Importe { get; set; }
    }

    public sealed class UpsertCatalogoResponseApi
    {
        public bool CostoAplicado { get; set; }
    }

    public sealed class InsumoIndirectoApi
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
    }

    // ---- Compras · repositorio de PDFs de órdenes ----

    public sealed class GuardarRepoResponseApi
    {
        public int FolioId { get; set; }
        public string TipoOrden { get; set; }
        public decimal Total { get; set; }
        public int Insumos { get; set; }
    }

    public sealed class CasaRepoApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
    }

    /// <summary>Detalle completo de una orden del repositorio (cabecera + casas + insumos).</summary>
    public sealed class DetalleOrdenRepoApi
    {
        public string Folio { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string TipoOrden { get; set; }
        public string NombreProveedor { get; set; }
        public string CodigoProveedor { get; set; }
        public decimal TotalSinIVA { get; set; }
        public decimal IVA { get; set; }
        public decimal TotalConIVA { get; set; }
        public int? NumeroOrden { get; set; }
        public string Usuario { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public List<CasaRepoApi> Casas { get; set; } = new List<CasaRepoApi>();
        public List<DetalleInsumo> Insumos { get; set; } = new List<DetalleInsumo>();
    }

    // ---- Estimaciones · evidencias fotográficas ----

    /// <summary>
    /// Evidencia fotográfica (api/evidencias). FotoBase64 solo viene relleno al
    /// pedir la última evidencia de un lote; en el listado va vacío.
    /// </summary>
    public sealed class EvidenciaApi
    {
        public int Id { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Titulo { get; set; }
        public string FotoBase64 { get; set; }
        public string Extension { get; set; }
        public double TamanioKB { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
    }

    // ---- Estimaciones · fotos de concepto/partida ----

    /// <summary>Metadatos de una foto de concepto/partida (api/fotos-concepto).</summary>
    public sealed class FotoConceptoApi
    {
        public int Id { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Identificador { get; set; }
        public bool EsConcepto { get; set; }
        public string NombreNodo { get; set; }
        public string Descripcion { get; set; }
        public string Extension { get; set; }
        public double TamanioKB { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
    }

    // ---- Destajos · evidencias fotográficas (api/evidencias-destajo) ----

    /// <summary>Metadatos de una evidencia fotográfica de destajo, sin el binario.</summary>
    public sealed class EvidenciaDestajoApi
    {
        public int Id { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public int NodoId { get; set; }
        public string NombreDestajo { get; set; }
        public string Descripcion { get; set; }
        public string Extension { get; set; }
        public double TamanioKB { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
    }

    // ---- Estimaciones · avance de obra jerárquico (FormAvanceObra / FormHardProgress) ----

    /// <summary>Una partida de PresupuestoObra con su importe (api/avances/jerarquico).</summary>
    public sealed class PartidaAvanceApi
    {
        public int Wbs { get; set; }
        public string Codigo { get; set; }
        public string Padre { get; set; }
        public string Etapa { get; set; }
        public string Partida { get; set; }
        public double ImporteTotal { get; set; }
    }

    /// <summary>Avance guardado de una partida (MontoEjecutado null si no aplica).</summary>
    public sealed class AvanceGuardadoApi
    {
        public int Wbs { get; set; }
        public double AvancePorcentaje { get; set; }
        public double? MontoEjecutado { get; set; }
    }

    /// <summary>Partidas + avances guardados de una casa, para armar el árbol de avance.</summary>
    public sealed class JerarquicoAvanceApi
    {
        public List<PartidaAvanceApi> Partidas { get; set; } = new List<PartidaAvanceApi>();
        public List<AvanceGuardadoApi> Avances { get; set; } = new List<AvanceGuardadoApi>();
    }

    // ---- Estimaciones · avance por concepto (FormAvanceConcepto) ----

    /// <summary>Un concepto de Estimacion(Concepto) con su importe total (api/avances/conceptos).</summary>
    public sealed class ConceptoAvanceApi
    {
        public string Codigo { get; set; }
        public string Concepto { get; set; }
        public double Total { get; set; }
    }

    /// <summary>Total y ejecutado agregados por Padre (api/avances/avance-por-padre).</summary>
    public sealed class AvancePorPadreApi
    {
        public string Padre { get; set; }
        public double Total { get; set; }
        public double Ejecutado { get; set; }
    }

    // ---- Estimaciones · estimación jerárquica (FormEstimacionConceptoMigrado) ----

    /// <summary>Partida de PresupuestoObra con info dinámica (api/avances/estimacion-jerarquica).</summary>
    public sealed class PartidaDinamicaApi
    {
        public int Wbs { get; set; }
        public string Codigo { get; set; }
        public string Padre { get; set; }
        public string Etapa { get; set; }
        public string Partida { get; set; }
        public double Costo { get; set; }
        public bool EsDinamica { get; set; }
        public double ValorM2Tunera { get; set; }
        public double ValorM2Calandra { get; set; }
        public double LimiteM2 { get; set; }
        public double LimiteM2Tunera { get; set; }
        public double LimiteM2Calandra { get; set; }
        public string PrototiposAplicables { get; set; }
    }

    /// <summary>Avance guardado de una partida con m² y fecha (AvanceManualObra).</summary>
    public sealed class AvancePartidaApi
    {
        public int Wbs { get; set; }
        public double AvancePorcentaje { get; set; }
        public double MontoEjecutado { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public double MetrosCuadrados { get; set; }
    }

    /// <summary>Partidas + avances de una casa, para armar el árbol de estimación.</summary>
    public sealed class EstimacionJerarquicaApi
    {
        public List<PartidaDinamicaApi> Partidas { get; set; } = new List<PartidaDinamicaApi>();
        public List<AvancePartidaApi> Avances { get; set; } = new List<AvancePartidaApi>();
    }

    // ---- Estimaciones · agregar/gestionar conceptos (api/avances) ----

    /// <summary>Concepto existente para el selector de posición (api/avances/conceptos-selector).</summary>
    public sealed class ConceptoExistenteApi
    {
        public int Codigo { get; set; }
        public string Nombre { get; set; }
    }

    /// <summary>Partida nueva a insertar al crear un concepto (api/avances/concepto-nuevo).</summary>
    public sealed class PartidaNuevaApi
    {
        public string Etapa { get; set; }
        public string Partida { get; set; }
        public double CostoTunera { get; set; }
        public double CostoCalandra { get; set; }
    }

    /// <summary>Alta de un concepto nuevo con renumeración opcional (api/avances/concepto-nuevo).</summary>
    public sealed class ConceptoNuevoApi
    {
        public int Codigo { get; set; }
        public string Nombre { get; set; }
        public bool Renumerar { get; set; }
        public List<PartidaNuevaApi> Partidas { get; set; } = new List<PartidaNuevaApi>();
    }

    /// <summary>Upsert de avance de partida con m²/fecha (api/avances/partida-estimacion).</summary>
    public sealed class GuardarAvancePartidaEstimacionApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public int Wbs { get; set; }
        public double AvancePorcentaje { get; set; }
        public double MontoEjecutado { get; set; }
        public double MetrosCuadrados { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
    }

    /// <summary>Reset de avance de una partida/concepto (api/avances/resetear-partida).</summary>
    public sealed class ResetearPartidaApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Wbs { get; set; }
    }

    /// <summary>Partida a marcar al 100% (api/avances/marcar-completadas).</summary>
    public sealed class PartidaCompletadaApi
    {
        public int Wbs { get; set; }
        public double Monto { get; set; }
        public double MetrosCuadrados { get; set; }
    }

    /// <summary>Concepto a registrar como completado, WBS negativo (api/avances/marcar-completadas).</summary>
    public sealed class ConceptoCompletadoApi
    {
        public int Wbs { get; set; }
        public string Nombre { get; set; }
        public double Monto { get; set; }
    }

    /// <summary>Marca partidas/conceptos al 100% en una transacción (api/avances/marcar-completadas).</summary>
    public sealed class MarcarCompletadasApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public List<PartidaCompletadaApi> Partidas { get; set; } = new List<PartidaCompletadaApi>();
        public List<ConceptoCompletadoApi> Conceptos { get; set; } = new List<ConceptoCompletadoApi>();
    }

    // ---- Estimaciones · folios y PDFs (api/folios-estimacion) ----

    /// <summary>Una línea del detalle del folio de estimación (api/folios-estimacion).</summary>
    public sealed class DetalleFolioApi
    {
        public string CodigoConcepto { get; set; }
        public string NombreConcepto { get; set; }
        public int Wbs { get; set; }
        public string NombrePartida { get; set; }
        public double MontoPresupuestado { get; set; }
        public double MontoEjecutado { get; set; }
        public double AvancePorcentaje { get; set; }
    }

    /// <summary>Estimación completa a guardar: cabecera + detalle + PDF base64 (api/folios-estimacion).</summary>
    public sealed class GuardarFolioEstimacionApi
    {
        public string Folio { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public int NumeroEstimacion { get; set; }
        public string Proveedor { get; set; }
        public string Descripcion { get; set; }
        public double ImporteContrato { get; set; }
        public double TotalRequisicion { get; set; }
        public double Amortizacion { get; set; }
        public double PorcentajeAmortizacion { get; set; }
        public double TotalEstimacion { get; set; }
        public string Usuario { get; set; }
        public List<DetalleFolioApi> Detalle { get; set; } = new List<DetalleFolioApi>();
        public string NombreArchivoPdf { get; set; }
        public string PdfBase64 { get; set; }
    }

    /// <summary>Respuesta al guardar una estimación: Id del folio (api/folios-estimacion).</summary>
    public sealed class GuardarFolioEstimacionRespApi
    {
        public int FolioId { get; set; }
    }

    // ---- Destajos (api/destajos) ----

    public sealed class NodoDestajoApi
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Clave { get; set; }
        public int Nivel { get; set; }
        public int Orden { get; set; }
        public int TipoTarea { get; set; }
        public string TipoTareaTexto { get; set; }
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public bool Activa { get; set; }
        public string CuadrillaAsignada { get; set; }
        public bool DesatajoActivado { get; set; }
        public bool Finalizado { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public decimal Surtido { get; set; }
        public string Estado { get; set; }
    }

    public sealed class ResumenDestajosApi
    {
        public int Categorias { get; set; }
        public int Destajos { get; set; }
        public int Terminados { get; set; }
        public int Activados { get; set; }
        public int Disponibles { get; set; }
        public int Bloqueados { get; set; }
        public int Insumos { get; set; }
        public int InsumosPendientes { get; set; }
        public decimal ImporteTotal { get; set; }
        public decimal ImporteTerminado { get; set; }
        public int AvancePct { get; set; }
    }

    public sealed class ArbolDestajosApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Ruta { get; set; }
        public List<NodoDestajoApi> Nodos { get; set; } = new List<NodoDestajoApi>();
        public ResumenDestajosApi Resumen { get; set; }
    }

    public sealed class CasaDestajoApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Ruta { get; set; }
    }

    /// <summary>Progreso real de una casa (api/destajos/resumen-casas), para el mapa del panel.</summary>
    public sealed class ResumenCasaApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public int Destajos { get; set; }
        public int Terminados { get; set; }
        public int Activados { get; set; }
        public decimal ImporteTotal { get; set; }
        public decimal ImporteTerminado { get; set; }
        public int AvancePct { get; set; }
        public string Estado { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
    }

    /// <summary>Un destajo fusionado entre rutas (api/destajos/catalogo-avance-masivo), para FormAvanceMasivoWeb.</summary>
    public sealed class CatalogoDestajoMasivoApi
    {
        public string Categoria { get; set; }
        public string Destajo { get; set; }
        public int? NodoIdTunera { get; set; }
        public int? NodoIdCalandra { get; set; }
    }

    /// <summary>Resultado de api/destajos/avance-masivo.</summary>
    public sealed class AvanceMasivoResultadoApi
    {
        public int Ok { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
    }

    /// <summary>Estado de un destajo frente a un conjunto de casas (api/destajos/estado-avance-masivo).</summary>
    public sealed class EstadoDestajoMasivoApi
    {
        public string Categoria { get; set; }
        public string Destajo { get; set; }
        public int? NodoIdTunera { get; set; }
        public int? NodoIdCalandra { get; set; }
        public int CasasCompletas { get; set; }
        public int CasasTotal { get; set; }
    }

    public sealed class CuadrillaDestajoApi
    {
        public string Codigo { get; set; }
        public int Miembros { get; set; }
        public string Jefe { get; set; }
    }

    /// <summary>Fila de api/destajos/insumos-casa: lo surtido del almacén a una casa.</summary>
    public sealed class InsumoCasaAlmacenApi
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Usado { get; set; }
        public decimal Importe { get; set; }
    }

    public sealed class MiembroCuadrillaDestajoApi
    {
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public bool EsJefe { get; set; }
        public string Telefono { get; set; }
    }

    public sealed class ResultadoDestajoApi
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
    }

    /// <summary>Fila de api/destajos/reporte-semana.</summary>
    public sealed class RegistroSemanaApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public string DestajoNombre { get; set; }
        public string CategoriaNombre { get; set; }
        public string Cuadrilla { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public bool Finalizado { get; set; }
    }

    /// <summary>Fila de api/destajos/reporte-cuadrilla.</summary>
    public sealed class RegistroCuadrillaApi
    {
        public string Cuadrilla { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public int DestajoId { get; set; }
        public string DestajoNombre { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; }
        public decimal Importe { get; set; }
        public string Ruta { get; set; }
        public bool NominaDistribuida { get; set; }
        public DateTime? FechaDistribucionNomina { get; set; }
    }

    /// <summary>Fila de api/nomina/reporte (listado agregado por trabajador).</summary>
    public sealed class NominaReporteApi
    {
        public int? IdTrabajador { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public string Cuadrillas { get; set; }
        public int NumRecibos { get; set; }
        public decimal Monto { get; set; }
    }

    /// <summary>Fila de api/nomina/recibos (cabecera del recibo, sin el PDF binario).</summary>
    public sealed class ReciboNominaApi
    {
        public int Id { get; set; }
        public int? IdTrabajador { get; set; }
        public string NombreTrabajador { get; set; }
        public string Rol { get; set; }
        public string CodigoCuadrilla { get; set; }
        public string Concepto { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaRecibo { get; set; }
        public DateTime? PeriodoDesde { get; set; }
        public DateTime? PeriodoHasta { get; set; }
        public decimal TotalCuadrilla { get; set; }
        public bool TienePdf { get; set; }
    }

    // ---- Almacén (api/almacen) ----

    public sealed class HistorialMovimientoApi
    {
        public DateTime Fecha { get; set; }
        public string TipoMovimiento { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string Usuario { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Justificacion { get; set; }
    }

    public sealed class InventarioAlmacenItemApi
    {
        public string FolioOC { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public DateTime FechaEntrada { get; set; }
        public string Usuario { get; set; }
        public string Justificacion { get; set; }
        public bool EsExcepcion { get; set; }
        public string Estado { get; set; }
    }

    public sealed class OrdenPendienteApi
    {
        public string Folio { get; set; }
        public string NombreOrden { get; set; }
    }

    public sealed class OcDetallePendienteApi
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal CantidadComprada { get; set; }
        public string Estado { get; set; }
        public string Justificacion { get; set; }
    }

    public sealed class OcCasaApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
    }

    public sealed class CasaAlmacenApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
    }

    public sealed class InsumoMovimientoApi
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string Justificacion { get; set; }
    }

    public sealed class ResultadoEntradaApi
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public List<InsumoMovimientoApi> Insumos { get; set; }
    }

    public sealed class InsumoPendienteSalidaApi
    {
        public string Destajo { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Disponible { get; set; }
        public decimal MaximoPermitido { get; set; }
    }

    public sealed class PendientesSalidaApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public List<InsumoPendienteSalidaApi> Insumos { get; set; }
        public string Mensaje { get; set; }
    }

    public sealed class ResultadoSalidaApi
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public string Folio { get; set; }
        public decimal TotalImporte { get; set; }
        public List<InsumoMovimientoApi> Insumos { get; set; }
    }

    // ---- Trabajadores (api/trabajadores) ----

    public sealed class TrabajadorApi
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Nacionalidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Curp { get; set; }
        public string Rfc { get; set; }
        public string Ine { get; set; }
        public string Nss { get; set; }
    }

    public sealed class TrabajadorDetalleApi
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Nacionalidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Curp { get; set; }
        public string Rfc { get; set; }
        public string Ine { get; set; }
        public string Nss { get; set; }
        public string FotoBase64 { get; set; }
        public bool TieneCurp { get; set; }
        public int CurpKb { get; set; }
        public bool TieneRfc { get; set; }
        public int RfcKb { get; set; }
        public bool TieneIne { get; set; }
        public int IneKb { get; set; }
        public bool TieneNss { get; set; }
        public int NssKb { get; set; }
        public string RolCuadrilla { get; set; }
    }

    public sealed class ClaveNuevaApi
    {
        public string Clave { get; set; }
    }

    public sealed class MiembroCuadrillaApi
    {
        public int? IdTrabajador { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public bool EsJefe { get; set; }
        public string Telefono { get; set; }
    }

    public sealed class CuadrillaGuardadaApi
    {
        public string Codigo { get; set; }
    }

    public sealed class ReciboTrabajadorApi
    {
        public int Id { get; set; }
        public int? IdTrabajador { get; set; }
        public string NombreTrabajador { get; set; }
        public string Rol { get; set; }
        public string CodigoCuadrilla { get; set; }
        public string Concepto { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaRecibo { get; set; }
        public DateTime? PeriodoDesde { get; set; }
        public DateTime? PeriodoHasta { get; set; }
        public decimal TotalCuadrilla { get; set; }
        public bool TienePdf { get; set; }
    }

    // ---- Editor de Tareas (api/editor-tareas) ----

    public sealed class NodoEditorApi
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Orden { get; set; }
        public int Nivel { get; set; }
        public int TipoTarea { get; set; }
        public string TipoTareaTexto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public bool EsNuevo { get; set; }
        public Dictionary<string, string> Valores { get; set; } = new Dictionary<string, string>();
    }

    public sealed class ColumnaDefApi
    {
        public string Nombre { get; set; }
        public string Titulo { get; set; }
        public int Ancho { get; set; }
        public string TipoDato { get; set; }
        public bool EsEditable { get; set; }
        public string Formato { get; set; }
        public bool EsCalculada { get; set; }
        public int TipoOperacion { get; set; }
        public string ColumnaOrigen1 { get; set; }
        public string ColumnaOrigen2 { get; set; }
    }

    public sealed class ArbolEditorApi
    {
        public string Ruta { get; set; }
        public List<NodoEditorApi> Nodos { get; set; } = new List<NodoEditorApi>();
        public List<ColumnaDefApi> Columnas { get; set; } = new List<ColumnaDefApi>();
    }

    public sealed class GuardarArbolResponseApi
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public Dictionary<int, int> IdsReasignados { get; set; } = new Dictionary<int, int>();
    }

    // ---- Perfiles y permisos (api/perfiles) ----

    public sealed class PermisoApi
    {
        public string Clave { get; set; }
        public string Modulo { get; set; }
        public string Etiqueta { get; set; }
    }

    public sealed class PerfilResumenApi
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsSistema { get; set; }
        public int CantidadUsuarios { get; set; }
    }

    public sealed class PerfilDetalleApi
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsSistema { get; set; }
        public List<string> Permisos { get; set; } = new List<string>();
    }

    public sealed class UsuarioPerfilApi
    {
        public string Nombre { get; set; }
        public int? PerfilId { get; set; }
        public string PerfilNombre { get; set; }
        public string FotoBase64 { get; set; }
        public string FotoExtension { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool FechaAltaConfirmada { get; set; }
    }

    public sealed class GuardarPerfilRespApi
    {
        public int Id { get; set; }
    }

    // ---- Obras (api/obras) ----

    public sealed class ObraApi
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int NodosArbol { get; set; }
        public int Insumos { get; set; }
        public int Proveedores { get; set; }
    }

    public sealed class CrearObraResponseApi
    {
        public int Id { get; set; }
        public string NombreBD { get; set; }
    }

    public sealed class AsignacionObrasUsuarioApi
    {
        public string Usuario { get; set; }
        public List<int> ObraIds { get; set; } = new List<int>();
    }

    /// <summary>Paleta de acento + estado del logo de la obra activa (api/obras/{id}/tema).</summary>
    public sealed class TemaObraApi
    {
        public string ColorPrimario { get; set; }
        public string ColorSecundario { get; set; }
        public string ColorSuave { get; set; }
        public bool TieneLogo { get; set; }
        public string LogoExtension { get; set; }
    }

    /// <summary>Desglose del perfil activo (api/auth/mi-perfil), para el modal del avatar del rail.</summary>
    public sealed class MiPerfilApi
    {
        public string Usuario { get; set; }
        public string Perfil { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool TieneFoto { get; set; }
        public string FotoExtension { get; set; }
        public List<PermisoApi> Permisos { get; set; }
    }

    // ---- Facturación IA (api/facturacion) ----

    public sealed class SaldoObraApi
    {
        public int ObraId { get; set; }
        public string ObraNombre { get; set; }
        public int Conteo { get; set; }
        public decimal MontoACobrar { get; set; }
        public bool TablaDisponible { get; set; }
    }

    public sealed class ConciliacionIaHistorialApi
    {
        public string Uuid { get; set; }
        public string FolioOC { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
    }

    public sealed class PdfFacturacionApi
    {
        public int Id { get; set; }
        public int? ObraId { get; set; }
        public string ObraNombre { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int Conteo { get; set; }
        public decimal MontoACobrar { get; set; }
        public string NombreArchivo { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Usuario { get; set; }
    }

    // ---- Administrativos · consulta por casa (api/administrativos) ----

    public sealed class EstimacionItemApi
    {
        public string Wbs { get; set; }
        public string Codigo { get; set; }
        public string Etapa { get; set; }
        public string Partida { get; set; }
        public decimal AvancePorcentaje { get; set; }
        public decimal MontoEjecutado { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
    }

    public sealed class DestajoItemApi
    {
        public int NodoId { get; set; }
        public string Tipo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public decimal MontoGastado { get; set; }
        public string Cuadrilla { get; set; }
        public bool Finalizado { get; set; }
        public string Estado { get; set; }
    }

    public sealed class CompraCasaApi
    {
        public string FolioOC { get; set; }
        public DateTime? Fecha { get; set; }
        public string TipoOrden { get; set; }
        public string NombreOrden { get; set; }
        public string Proveedor { get; set; }
        public int NumPartidas { get; set; }
        public decimal Importe { get; set; }
    }

    public sealed class SalidaAlmacenCasaApi
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string Justificacion { get; set; }
    }

    public sealed class NominaCasaApi
    {
        public string NombreTarea { get; set; }
        public string CodigoCuadrilla { get; set; }
        public string NombreTrabajador { get; set; }
        public string Rol { get; set; }
        public bool EsJefe { get; set; }
        public decimal Monto { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    /// <summary>Concentrado financiero completo de una casa (api/administrativos/consolidado).</summary>
    public sealed class ConsolidadoCasaApi
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public List<EstimacionItemApi> Estimaciones { get; set; } = new List<EstimacionItemApi>();
        public List<DestajoItemApi> Destajos { get; set; } = new List<DestajoItemApi>();
        public List<CompraCasaApi> Compras { get; set; } = new List<CompraCasaApi>();
        public List<SalidaAlmacenCasaApi> SalidasAlmacen { get; set; } = new List<SalidaAlmacenCasaApi>();
        public List<NominaCasaApi> Nomina { get; set; } = new List<NominaCasaApi>();
        public decimal TotalEstimaciones { get; set; }
        public decimal TotalDestajosComprometido { get; set; }
        public decimal TotalManoObraGastado { get; set; }
        public decimal TotalMaterialGastado { get; set; }
        public decimal TotalCompras { get; set; }
        public decimal TotalSalidasAlmacen { get; set; }
        public decimal TotalNomina { get; set; }
        public decimal GranTotal { get; set; }
    }
}
