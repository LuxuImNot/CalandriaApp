using System;
using System.Collections.Generic;

namespace Calandria.Api.Models
{
    /// <summary>
    /// Un nodo del árbol de destajos. La jerarquía viaja plana (Id/ParentId) y la
    /// arma quien consume: es el mismo orden que devolvía el SELECT del cliente.
    /// </summary>
    public class NodoDestajoDto
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Clave { get; set; }
        public int Nivel { get; set; }
        public int Orden { get; set; }

        /// <summary>Valor crudo de TipoTarea (1 = Material).</summary>
        public int TipoTarea { get; set; }
        public string TipoTareaTexto { get; set; }

        public decimal Cantidad { get; set; }
        public string Unidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }

        // --- Estado de activación (ActivacionTareasRuta) ---
        public bool Activa { get; set; }
        public string CuadrillaAsignada { get; set; }
        public bool DesatajoActivado { get; set; }
        public bool Finalizado { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

        /// <summary>
        /// Cantidad ya surtida desde almacén para este insumo (cotejo por clave y,
        /// si no tiene, por nombre). Sólo tiene sentido en nodos Material.
        /// </summary>
        public decimal Surtido { get; set; }

        /// <summary>
        /// Paso del asistente para los destajos (Nivel 1):
        /// bloqueado · disponible · activado · terminado. Se calcula en el servidor
        /// para que la secuencia sea la misma en cualquier cliente.
        /// </summary>
        public string Estado { get; set; }
    }

    /// <summary>Contadores del encabezado, derivados de los mismos nodos.</summary>
    public class ResumenDestajosDto
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

    /// <summary>Respuesta de GET api/destajos/arbol: todo lo que la pantalla necesita.</summary>
    public class ArbolDestajosDto
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Ruta { get; set; }
        public List<NodoDestajoDto> Nodos { get; set; }
        public ResumenDestajosDto Resumen { get; set; }
    }

    public class CasaDestajoDto
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        /// <summary>Tabla de ruta que corresponde al prototipo.</summary>
        public string Ruta { get; set; }
    }

    /// <summary>
    /// Progreso real de una casa (mismos totales que ResumenDestajosDto, uno por
    /// casa), para pintar el mapa del panel con datos de ActivacionTareasRuta en
    /// vez de valores decorativos.
    /// </summary>
    public class ResumenCasaDto
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
        /// <summary>"ok" (terminada) · "warn" (en proceso) · "idle" (sin iniciar).</summary>
        public string Estado { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
    }

    public class MiembroCuadrillaDestajoDto
    {
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public bool EsJefe { get; set; }
        public string Telefono { get; set; }
    }

    public class CuadrillaDto
    {
        public string Codigo { get; set; }
        public int Miembros { get; set; }
        public string Jefe { get; set; }
    }

    // ======================= Peticiones de escritura =======================

    /// <summary>Campos comunes: identifican el destajo dentro de una casa y ruta.</summary>
    public class DestajoRefRequest
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public string Prototipo { get; set; }
        public int NodoId { get; set; }
    }

    public class ActivarDestajoRequest : DestajoRefRequest
    {
        /// <summary>Código de cuadrilla. Obligatorio: activar es asignar cuadrilla.</summary>
        public string Cuadrilla { get; set; }
    }

    public class DesactivarDestajoRequest : DestajoRefRequest
    {
        /// <summary>
        /// Obligatoria cuando el destajo ya liberó insumos: queda en la bitácora de
        /// excepciones. El stock no se devuelve.
        /// </summary>
        public string Justificacion { get; set; }
    }

    public class ReabrirDestajoRequest : DestajoRefRequest
    {
        /// <summary>
        /// true borra la asignación de nómina de la mano de obra del destajo.
        /// Los recibos ya emitidos nunca se eliminan.
        /// </summary>
        public bool BorrarNomina { get; set; }
    }

    /// <summary>Estado de un nodo en el guardado masivo.</summary>
    public class NodoActivacionRequest
    {
        public int NodoId { get; set; }
        public string Nombre { get; set; }
        public bool Activa { get; set; }
        public string CuadrillaAsignada { get; set; }
        public bool DesatajoActivado { get; set; }
        public bool Finalizado { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
    }

    /// <summary>Reemplaza en bloque las activaciones de una casa (botón Guardar).</summary>
    public class GuardarActivacionesRequest
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Ruta { get; set; }
        public string Prototipo { get; set; }
        public List<NodoActivacionRequest> Nodos { get; set; }
    }

    /// <summary>Archiva en BD un PDF de asignación o finalización ya generado.</summary>
    public class GuardarPdfDestajoRequest : DestajoRefRequest
    {
        public string NombreDestajo { get; set; }
        public string CuadrillaAsignada { get; set; }
        public string NombreArchivo { get; set; }
        /// <summary>Contenido del PDF en base64.</summary>
        public string ContenidoBase64 { get; set; }
    }

    /// <summary>Respuesta de las transiciones de estado: el nodo tal como quedó.</summary>
    public class ResultadoDestajoDto
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public NodoDestajoDto Nodo { get; set; }
    }

    // ===================== Avance Masivo (FormAvanceMasivoWeb) =====================

    /// <summary>
    /// Un destajo (Nivel 1) fusionado por Categoría+Nombre entre RutaTuneraDestajo y
    /// RutaCalandraDestajo, para el treelist de Avance Masivo. NodoId* queda null si
    /// ese destajo no existe en esa ruta.
    /// </summary>
    public class CatalogoDestajoMasivoDto
    {
        public string Categoria { get; set; }
        public string Destajo { get; set; }
        public int? NodoIdTunera { get; set; }
        public int? NodoIdCalandra { get; set; }
    }

    public class AvanceMasivoDestajoItem
    {
        public int? NodoIdTunera { get; set; }
        public int? NodoIdCalandra { get; set; }
    }

    public class AvanceMasivoCasaItem
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
    }

    /// <summary>POST /api/destajos/avance-masivo · marca varios destajos como terminados en varias casas.</summary>
    public class AvanceMasivoDestajosRequest
    {
        public List<AvanceMasivoDestajoItem> Destajos { get; set; }
        public List<AvanceMasivoCasaItem> Casas { get; set; }
    }

    public class AvanceMasivoResultadoDto
    {
        public int Ok { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
    }

    /// <summary>
    /// Cuántas de las casas consultadas (POST /api/destajos/estado-avance-masivo) ya
    /// tienen este destajo finalizado — para marcarlo en el treelist de Avance Masivo.
    /// </summary>
    public class EstadoDestajoMasivoDto
    {
        public string Categoria { get; set; }
        public string Destajo { get; set; }
        public int? NodoIdTunera { get; set; }
        public int? NodoIdCalandra { get; set; }
        public int CasasCompletas { get; set; }
        public int CasasTotal { get; set; }
    }

    /// <summary>
    /// Una fila de GET api/destajos/reporte-semana: un destajo (Nivel 1) activado,
    /// terminado o pendiente dentro del periodo. El cliente agrupa por Estado →
    /// Casa → Destajo (mismo criterio que FormReporteDestajosSemana.cs).
    /// </summary>
    public class RegistroSemanaDto
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

    /// <summary>
    /// Una fila de GET api/destajos/reporte-cuadrilla: un destajo finalizado con
    /// cuadrilla asignada. El cliente agrupa por Cuadrilla → Categoría → Destajo
    /// (mismo criterio que FormDestajosPorCuadrilla.cs).
    /// </summary>
    public class RegistroCuadrillaDto
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
}
