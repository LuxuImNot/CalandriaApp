using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;
using Newtonsoft.Json;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Destajos: árbol de la ruta, activación/finalización/desactivación/
    /// reapertura de destajos (Nivel 1), guardado en bloque y archivo de PDFs.
    /// Reproduce el SQL de FormActivarTareasTreeList.cs (ver CONTINUAR_DESTAJOS_WEB.md).
    /// </summary>
    [RoutePrefix("api/destajos"), RequierePermiso("destajos.ver")]
    public class DestajosController : ApiController
    {
        private const string TablaRutaTunera = "RutaTuneraDestajo";
        private const string TablaRutaCalandra = "RutaCalandraDestajo";

        private static string ValidarRuta(string ruta)
        {
            if (ruta == TablaRutaTunera) return TablaRutaTunera;
            if (ruta == TablaRutaCalandra) return TablaRutaCalandra;
            return null;
        }

        private static bool EsAdmin(System.Security.Principal.IPrincipal user)
        {
            return user.IsInRole("Admin") ||
                   string.Equals(user.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase);
        }

        // ==================================================================
        // Lectura
        // ==================================================================

        /// <summary>GET /api/destajos/manzanas</summary>
        [HttpGet, Route("manzanas")]
        public IHttpActionResult Manzanas()
        {
            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read()) lista.Add(reader["Manzana"].ToString());
            }
            return Ok(lista);
        }

        /// <summary>GET /api/destajos/lotes?manzana=X</summary>
        [HttpGet, Route("lotes")]
        public IHttpActionResult Lotes(string manzana)
        {
            if (string.IsNullOrWhiteSpace(manzana))
                return BadRequest("Falta el parámetro 'manzana'.");

            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT DISTINCT Lote FROM InventarioCasas WHERE Manzana = @m ORDER BY Lote", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) lista.Add(reader["Lote"].ToString());
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/destajos/casa?manzana=X&amp;lote=Y</summary>
        [HttpGet, Route("casa")]
        public IHttpActionResult Casa(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                return BadRequest("Faltan los parámetros 'manzana' y/o 'lote'.");

            string prototipo;
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                var result = cmd.ExecuteScalar();
                prototipo = result?.ToString() ?? "";
            }

            if (string.IsNullOrEmpty(prototipo))
                return NotFound();

            return Ok(new CasaDestajoDto
            {
                Manzana = manzana,
                Lote = lote,
                Prototipo = prototipo,
                Ruta = RutaDeprototipo(prototipo)
            });
        }

        private static string RutaDeprototipo(string prototipo)
        {
            return (prototipo ?? "").ToUpper().Contains("CALANDRA") ? TablaRutaCalandra : TablaRutaTunera;
        }

        /// <summary>
        /// GET /api/destajos/resumen-casas — progreso real (destajos finalizados,
        /// monto ejecutado) de TODAS las casas, para el mapa del panel. Agrupa por
        /// Ruta para cargar el catálogo de nodos una sola vez por prototipo en vez
        /// de una vez por casa (118 casas, sólo 2 rutas).
        /// </summary>
        [HttpGet, Route("resumen-casas")]
        public IHttpActionResult ResumenCasas()
        {
            var casas = new List<(string Manzana, string Lote, string Prototipo)>();
            using (var conn = Db.Abrir())
            {
                using (var cmd = new SqlCommand(
                    "SELECT Manzana, Lote, Prototipo FROM InventarioCasas", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        casas.Add((reader["Manzana"].ToString(), reader["Lote"].ToString(),
                            reader["Prototipo"].ToString()));
                }

                EnsureTablaActivacion(conn);

                var resultado = new List<ResumenCasaDto>();
                foreach (var grupo in casas.GroupBy(c => RutaDeprototipo(c.Prototipo)))
                {
                    string tablaRuta = grupo.Key;
                    var nodos = CargarNodos(conn, tablaRuta);
                    var nivel1 = nodos.Where(n => n.Nivel == 1).ToList();
                    var nivel2 = nodos.Where(n => n.Nivel == 2).ToList();
                    decimal importeTotal = nivel2.Sum(n => n.Importe);
                    int totalDestajos = nivel1.Count;

                    var activacionesPorCasa = CargarActivacionesPorRuta(conn, tablaRuta);

                    foreach (var c in grupo)
                    {
                        if (!activacionesPorCasa.TryGetValue((c.Manzana, c.Lote), out var activaciones))
                            activaciones = new Dictionary<int, ActivacionInfo>();
                        var finIds = new HashSet<int>(nivel1
                            .Where(n => activaciones.TryGetValue(n.Id, out var a) && a.Finalizado)
                            .Select(n => n.Id));
                        int activados = nivel1.Count(n =>
                            activaciones.TryGetValue(n.Id, out var a) && a.DesatajoActivado);
                        decimal importeTerminado = nivel2.Where(n => finIds.Contains(n.ParentId)).Sum(n => n.Importe);

                        DateTime? ultima = activaciones.Count > 0
                            ? activaciones.Values.Max(a => a.FechaActualizacion)
                            : null;

                        string estado = totalDestajos > 0 && finIds.Count == totalDestajos ? "ok"
                            : activados == 0 && finIds.Count == 0 ? "idle"
                            : "warn";

                        resultado.Add(new ResumenCasaDto
                        {
                            Manzana = c.Manzana,
                            Lote = c.Lote,
                            Prototipo = c.Prototipo,
                            Destajos = totalDestajos,
                            Terminados = finIds.Count,
                            Activados = activados,
                            ImporteTotal = importeTotal,
                            ImporteTerminado = importeTerminado,
                            // Ponderado por importe, igual que CalcularResumen (la
                            // pantalla de destajos): así el mapa y el árbol dicen lo
                            // mismo de la misma casa. Sin importes, conteo de destajos.
                            AvancePct = importeTotal > 0
                                ? (int)Math.Round(importeTerminado * 100m / importeTotal)
                                : (totalDestajos > 0 ? (int)Math.Round(finIds.Count * 100m / totalDestajos) : 0),
                            Estado = estado,
                            UltimaActualizacion = ultima
                        });
                    }
                }

                // Respaldo: casas sin actividad de destajos pero con avance capturado a
                // mano (FormHardProgress / AvanceManualObra) — sin esto se ven "idle" en
                // el mapa aunque sí tengan progreso real registrado por esa vía.
                if (ExisteTablaRuta(conn, "AvanceManualObra"))
                {
                    var manual = CargarAvanceManualPorCasa(conn);
                    foreach (var r in resultado)
                    {
                        if (r.Estado != "idle") continue;
                        if (!manual.TryGetValue((r.Manzana, r.Lote), out var m)) continue;

                        r.AvancePct = m.AvancePct;
                        r.Estado = m.AvancePct >= 100 ? "ok" : m.AvancePct > 0 ? "warn" : "idle";
                        r.UltimaActualizacion = m.UltimaActualizacion;
                    }
                }

                return Ok(resultado);
            }
        }

        private class AvanceManualResumen
        {
            public int AvancePct;
            public DateTime? UltimaActualizacion;
        }

        /// <summary>
        /// Avance manual (AvanceManualObra) agregado por casa, para el respaldo de
        /// ResumenCasas. Promedio simple de AvancePorcentaje por WBS: ImporteTotal
        /// casi nunca se captura en esta tabla (queda NULL), así que no sirve para
        /// ponderar.
        /// </summary>
        private static Dictionary<(string, string), AvanceManualResumen> CargarAvanceManualPorCasa(SqlConnection conn)
        {
            var resultado = new Dictionary<(string, string), AvanceManualResumen>();
            using (var cmd = new SqlCommand(@"
                SELECT Manzana, Lote,
                       AVG(ISNULL(AvancePorcentaje, 0)) AS Pct,
                       MAX(FechaActualizacion) AS Ultima
                FROM AvanceManualObra
                GROUP BY Manzana, Lote", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    resultado[(reader["Manzana"].ToString(), reader["Lote"].ToString())] = new AvanceManualResumen
                    {
                        AvancePct = (int)Math.Round(Convert.ToDouble(reader["Pct"])),
                        UltimaActualizacion = reader["Ultima"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Ultima"])
                    };
                }
            }
            return resultado;
        }

        /// <summary>
        /// GET /api/destajos/insumos-casa?manzana=X&amp;lote=Y — lo surtido del almacén
        /// a esa casa, agregado por clave/descripción y con el importe REAL de las
        /// salidas (no cantidad x precio programado). Es lo que la vista "Insumos"
        /// necesita además del árbol: detecta excesos y los insumos surtidos que no
        /// estaban programados en ningún destajo.
        /// </summary>
        [HttpGet, Route("insumos-casa")]
        public IHttpActionResult InsumosCasa(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                return BadRequest("Faltan los parámetros 'manzana' y/o 'lote'.");

            var lista = new List<InsumoCasaAlmacenDto>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(Clave,'') AS Clave,
                       MAX(Descripcion) AS Descripcion,
                       MAX(Unidad) AS Unidad,
                       SUM(Cantidad) AS Usado,
                       SUM(Importe) AS Importe
                FROM dbo.SalidasAlmacen
                WHERE LTRIM(RTRIM(Manzana)) = @m AND LTRIM(RTRIM(Lote)) = @l
                GROUP BY ISNULL(Clave,'')", conn))
            {
                cmd.Parameters.AddWithValue("@m", (manzana ?? "").Trim());
                cmd.Parameters.AddWithValue("@l", (lote ?? "").Trim());
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new InsumoCasaAlmacenDto
                        {
                            Clave = (rd["Clave"]?.ToString() ?? "").Trim(),
                            Descripcion = rd["Descripcion"]?.ToString() ?? "",
                            Unidad = rd["Unidad"]?.ToString() ?? "",
                            Usado = rd["Usado"] != DBNull.Value ? Convert.ToDecimal(rd["Usado"]) : 0m,
                            Importe = rd["Importe"] != DBNull.Value ? Convert.ToDecimal(rd["Importe"]) : 0m
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/destajos/arbol?manzana=X&amp;lote=Y&amp;ruta=RutaTuneraDestajo|RutaCalandraDestajo</summary>
        [HttpGet, Route("arbol")]
        public IHttpActionResult Arbol(string manzana, string lote, string ruta)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                return BadRequest("Faltan los parámetros 'manzana' y/o 'lote'.");
            string tablaRuta = ValidarRuta(ruta);
            if (tablaRuta == null)
                return BadRequest("Ruta inválida.");

            using (var conn = Db.Abrir())
            {
                string prototipo = ObtenerPrototipo(conn, manzana, lote);

                var nodos = CargarNodos(conn, tablaRuta);

                EnsureTablaActivacion(conn);
                var activaciones = CargarActivaciones(conn, manzana, lote, tablaRuta);
                foreach (var n in nodos)
                {
                    if (!activaciones.TryGetValue(n.Id, out var a)) continue;
                    n.Activa = a.Activa;
                    n.CuadrillaAsignada = a.CuadrillaAsignada;
                    n.DesatajoActivado = a.DesatajoActivado;
                    n.Finalizado = a.Finalizado;
                    n.FechaActivacion = a.FechaActivacion;
                    n.FechaFinalizacion = a.FechaFinalizacion;
                }

                var (porClave, porNombre) = CargarSurtido(conn, manzana, lote);
                foreach (var n in nodos.Where(n => n.TipoTarea == (int)TipoTareaMaterial))
                    n.Surtido = SurtidoDeInsumo(porClave, porNombre, n.Clave, n.Nombre);

                CalcularEstados(nodos);

                var resumen = CalcularResumen(nodos);

                return Ok(new ArbolDestajosDto
                {
                    Manzana = manzana,
                    Lote = lote,
                    Prototipo = prototipo,
                    Ruta = tablaRuta,
                    Nodos = nodos,
                    Resumen = resumen
                });
            }
        }

        /// <summary>
        /// GET /api/destajos/catalogo-avance-masivo — destajos (Nivel 1) de ambas
        /// rutas fusionados por Categoría+Nombre, para el treelist de Avance Masivo.
        /// Igual criterio de "resolver por nombre entre prototipos" que ya usa el
        /// catálogo de insumos de destajos para la OC. El orden es el mismo árbol
        /// (Categoría, sus destajos en Orden, próxima categoría…) que ya usan
        /// Arbol()/editor-tareas — CargarNodos ya lo entrega así (OrdenarComoArbol);
        /// aquí sólo se conserva, nunca se reordena alfabéticamente.
        /// </summary>
        [HttpGet, Route("catalogo-avance-masivo")]
        public IHttpActionResult CatalogoAvanceMasivo()
        {
            using (var conn = Db.Abrir())
                return Ok(CargarCatalogoAvanceMasivo(conn));
        }

        /// <summary>Construye el catálogo fusionado de CatalogoAvanceMasivo(); factorizado para reusarse en EstadoAvanceMasivo.</summary>
        private static List<CatalogoDestajoMasivoDto> CargarCatalogoAvanceMasivo(SqlConnection conn)
        {
            var merged = new Dictionary<string, CatalogoDestajoMasivoDto>(StringComparer.OrdinalIgnoreCase);
            var orden = new List<string>();

            void Agregar(string tablaRuta, bool esTunera)
            {
                var nodos = CargarNodos(conn, tablaRuta);
                var categorias = nodos.Where(n => n.Nivel == 0).ToDictionary(n => n.Id, n => n.Nombre);
                foreach (var n in nodos.Where(n => n.Nivel == 1))
                {
                    string categoria = categorias.TryGetValue(n.ParentId, out var cat) ? cat : "";
                    string key = categoria.Trim() + "||" + n.Nombre.Trim();
                    if (!merged.TryGetValue(key, out var dto))
                    {
                        dto = new CatalogoDestajoMasivoDto { Categoria = categoria, Destajo = n.Nombre };
                        merged[key] = dto;
                        orden.Add(key);
                    }
                    if (esTunera) dto.NodoIdTunera = n.Id; else dto.NodoIdCalandra = n.Id;
                }
            }
            Agregar(TablaRutaTunera, true);
            Agregar(TablaRutaCalandra, false);

            return orden.Select(k => merged[k]).ToList();
        }

        /// <summary>
        /// POST /api/destajos/estado-avance-masivo — para las casas dadas, cuántas ya
        /// tienen cada destajo del catálogo finalizado. El treelist de Avance Masivo
        /// usa esto para marcar (check) los destajos que ya están completos en TODAS
        /// las casas seleccionadas y mostrar un badge cuando sólo están parciales.
        /// </summary>
        [HttpPost, Route("estado-avance-masivo")]
        public IHttpActionResult EstadoAvanceMasivo([FromBody] List<AvanceMasivoCasaItem> casas)
        {
            if (casas == null || casas.Count == 0) return Ok(new List<EstadoDestajoMasivoDto>());

            using (var conn = Db.Abrir())
            {
                EnsureTablaActivacion(conn);

                var catalogo = CargarCatalogoAvanceMasivo(conn);
                var resultado = catalogo.Select(d => new EstadoDestajoMasivoDto
                {
                    Categoria = d.Categoria,
                    Destajo = d.Destajo,
                    NodoIdTunera = d.NodoIdTunera,
                    NodoIdCalandra = d.NodoIdCalandra,
                    CasasTotal = casas.Count,
                    CasasCompletas = 0
                }).ToList();

                foreach (var grupo in casas.GroupBy(c => RutaDeprototipo(c.Prototipo)))
                {
                    string tablaRuta = grupo.Key;
                    var activacionesPorCasa = CargarActivacionesPorRuta(conn, tablaRuta);

                    foreach (var c in grupo)
                    {
                        if (!activacionesPorCasa.TryGetValue((c.Manzana, c.Lote), out var activaciones)) continue;

                        foreach (var d in resultado)
                        {
                            int? nodoId = tablaRuta == TablaRutaTunera ? d.NodoIdTunera : d.NodoIdCalandra;
                            if (nodoId == null) continue;
                            if (activaciones.TryGetValue(nodoId.Value, out var a) && a.Finalizado)
                                d.CasasCompletas++;
                        }
                    }
                }

                return Ok(resultado);
            }
        }

        private const int TipoTareaMaterial = 1;

        private static string ObtenerPrototipo(SqlConnection conn, string manzana, string lote)
        {
            using (var cmd = new SqlCommand(
                "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
        }

        private static List<NodoDestajoDto> CargarNodos(SqlConnection conn, string tablaRuta)
        {
            var nodos = new List<NodoDestajoDto>();
            string sql = $@"
                SELECT
                    r.ID, r.Nombre, r.Descripcion, r.Nivel, r.Orden, r.TipoTarea, r.ParentId,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad'   THEN c.Valor END), '')  AS Unidad,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS PrecioUnitario,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Clave'    THEN c.Valor END), '')  AS Clave
                FROM [{tablaRuta}] r
                LEFT JOIN [{tablaRuta}_Columnas] c ON r.ID = c.NodoID
                GROUP BY r.ID, r.Nombre, r.Descripcion, r.Nivel, r.Orden, r.TipoTarea, r.ParentId
                ORDER BY r.Nivel, r.Orden";

            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int tipoTarea = reader["TipoTarea"] != DBNull.Value ? Convert.ToInt32(reader["TipoTarea"]) : 0;
                    decimal cantidad = ParseDecimal(reader["Cantidad"]);
                    decimal precio = ParseDecimal(reader["PrecioUnitario"]);

                    nodos.Add(new NodoDestajoDto
                    {
                        Id = Convert.ToInt32(reader["ID"]),
                        ParentId = reader["ParentId"] != DBNull.Value ? Convert.ToInt32(reader["ParentId"]) : 0,
                        Nombre = reader["Nombre"].ToString(),
                        Descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString(),
                        Clave = reader["Clave"] == DBNull.Value ? "" : reader["Clave"].ToString().Trim(),
                        Nivel = Convert.ToInt32(reader["Nivel"]),
                        Orden = Convert.ToInt32(reader["Orden"]),
                        TipoTarea = tipoTarea,
                        TipoTareaTexto = tipoTarea == 1 ? "Material" : tipoTarea == 2 ? "Mano de Obra" : "-",
                        Cantidad = cantidad,
                        Unidad = reader["Unidad"] == DBNull.Value ? "" : reader["Unidad"].ToString(),
                        PrecioUnitario = precio,
                        Importe = cantidad * precio,
                        Activa = true
                    });
                }
            }
            return OrdenarComoArbol(nodos);
        }

        /// <summary>
        /// El SELECT trae los nodos aplanados por Nivel,Orden — eso ordena bien a
        /// los HERMANOS de un mismo padre, pero "aplanar por nivel" no es lo mismo
        /// que un recorrido de árbol: si dos categorías comparten número de Orden
        /// en sus destajos, un simple ORDER BY las entrelaza. Se reordena aquí
        /// para que cada nodo quede seguido de sus propios hijos (categoría,
        /// todos sus destajos con sus insumos/mano de obra, categoría siguiente…),
        /// igual que arma el árbol en memoria FormEditorTreeList.cs (Diccionario +
        /// ParentID) antes de mostrarlo — esa es la referencia de "orden correcto".
        /// </summary>
        private static List<NodoDestajoDto> OrdenarComoArbol(List<NodoDestajoDto> planos)
        {
            var porPadre = planos.GroupBy(n => n.ParentId).ToDictionary(g => g.Key, g => g.ToList());
            var resultado = new List<NodoDestajoDto>(planos.Count);

            void Recorrer(int parentId)
            {
                if (!porPadre.TryGetValue(parentId, out var hijos)) return;
                foreach (var hijo in hijos)
                {
                    resultado.Add(hijo);
                    Recorrer(hijo.Id);
                }
            }

            Recorrer(0); // los nodos raíz (Nivel 0) tienen ParentId = 0
            return resultado;
        }

        private class ActivacionInfo
        {
            public bool Activa;
            public string CuadrillaAsignada;
            public bool DesatajoActivado;
            public bool Finalizado;
            public DateTime? FechaActivacion;
            public DateTime? FechaFinalizacion;
            public DateTime? FechaActualizacion;
        }

        private static Dictionary<int, ActivacionInfo> CargarActivaciones(
            SqlConnection conn, string manzana, string lote, string tablaRuta)
        {
            var dict = new Dictionary<int, ActivacionInfo>();
            using (var cmd = new SqlCommand(@"
                SELECT NodoID, Activa, CuadrillaAsignada, DesatajoActivado,
                       ISNULL(Finalizado, 0) AS Finalizado, FechaFinalizacion, FechaActivacion,
                       FechaActualizacion
                FROM ActivacionTareasRuta
                WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.Parameters.AddWithValue("@ruta", tablaRuta);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dict[Convert.ToInt32(reader["NodoID"])] = new ActivacionInfo
                        {
                            Activa = Convert.ToBoolean(reader["Activa"]),
                            CuadrillaAsignada = reader["CuadrillaAsignada"] == DBNull.Value ? "" : reader["CuadrillaAsignada"].ToString(),
                            DesatajoActivado = reader["DesatajoActivado"] != DBNull.Value && Convert.ToBoolean(reader["DesatajoActivado"]),
                            Finalizado = reader["Finalizado"] != DBNull.Value && Convert.ToBoolean(reader["Finalizado"]),
                            FechaActivacion = reader["FechaActivacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaActivacion"]),
                            FechaFinalizacion = reader["FechaFinalizacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaFinalizacion"]),
                            FechaActualizacion = reader["FechaActualizacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaActualizacion"])
                        };
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// Igual que CargarActivaciones pero para TODA la ruta de una sola query,
        /// agrupada en memoria por (Manzana, Lote). La usa ResumenCasas para no
        /// hacer una query por casa (una ruta cubre ~118 casas).
        /// </summary>
        private static Dictionary<(string Manzana, string Lote), Dictionary<int, ActivacionInfo>> CargarActivacionesPorRuta(
            SqlConnection conn, string tablaRuta)
        {
            var resultado = new Dictionary<(string, string), Dictionary<int, ActivacionInfo>>();
            using (var cmd = new SqlCommand(@"
                SELECT Manzana, Lote, NodoID, Activa, CuadrillaAsignada, DesatajoActivado,
                       ISNULL(Finalizado, 0) AS Finalizado, FechaFinalizacion, FechaActivacion,
                       FechaActualizacion
                FROM ActivacionTareasRuta
                WHERE Ruta = @ruta", conn))
            {
                cmd.Parameters.AddWithValue("@ruta", tablaRuta);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var clave = (reader["Manzana"].ToString(), reader["Lote"].ToString());
                        if (!resultado.TryGetValue(clave, out var dict))
                        {
                            dict = new Dictionary<int, ActivacionInfo>();
                            resultado[clave] = dict;
                        }
                        dict[Convert.ToInt32(reader["NodoID"])] = new ActivacionInfo
                        {
                            Activa = Convert.ToBoolean(reader["Activa"]),
                            CuadrillaAsignada = reader["CuadrillaAsignada"] == DBNull.Value ? "" : reader["CuadrillaAsignada"].ToString(),
                            DesatajoActivado = reader["DesatajoActivado"] != DBNull.Value && Convert.ToBoolean(reader["DesatajoActivado"]),
                            Finalizado = reader["Finalizado"] != DBNull.Value && Convert.ToBoolean(reader["Finalizado"]),
                            FechaActivacion = reader["FechaActivacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaActivacion"]),
                            FechaFinalizacion = reader["FechaFinalizacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaFinalizacion"]),
                            FechaActualizacion = reader["FechaActualizacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaActualizacion"])
                        };
                    }
                }
            }
            return resultado;
        }

        private static (Dictionary<string, decimal> porClave, Dictionary<string, decimal> porNombre) CargarSurtido(
            SqlConnection conn, string manzana, string lote)
        {
            var porClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var porNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(Clave,'') AS Clave, ISNULL(Descripcion,'') AS Descripcion, SUM(Cantidad) AS Surtido
                FROM dbo.SalidasAlmacen
                WHERE Manzana = @m AND Lote = @l
                GROUP BY ISNULL(Clave,''), ISNULL(Descripcion,'')", conn))
            {
                cmd.Parameters.AddWithValue("@m", (manzana ?? "").Trim());
                cmd.Parameters.AddWithValue("@l", (lote ?? "").Trim());
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string clave = (rd["Clave"]?.ToString() ?? "").Trim();
                        string nombre = (rd["Descripcion"]?.ToString() ?? "").Trim();
                        decimal s = rd["Surtido"] != DBNull.Value ? Convert.ToDecimal(rd["Surtido"]) : 0m;
                        if (clave.Length > 0)
                        {
                            porClave.TryGetValue(clave, out var a);
                            porClave[clave] = a + s;
                        }
                        if (nombre.Length > 0)
                        {
                            porNombre.TryGetValue(nombre, out var b);
                            porNombre[nombre] = b + s;
                        }
                    }
                }
            }
            return (porClave, porNombre);
        }

        private static decimal SurtidoDeInsumo(
            Dictionary<string, decimal> porClave, Dictionary<string, decimal> porNombre, string clave, string nombre)
        {
            if (!string.IsNullOrWhiteSpace(clave) && porClave.TryGetValue(clave.Trim(), out var v)) return v;
            if (!string.IsNullOrWhiteSpace(nombre) && porNombre.TryGetValue(nombre.Trim(), out var w)) return w;
            return 0m;
        }

        /// <summary>
        /// Calcula el Estado (Bloqueado/Disponible/Activado/Terminado) de cada nodo
        /// Nivel 1, agrupando por categoría (Nivel 0) y respetando el orden ya
        /// cargado (Orden ascendente). Ver PanelPasos.cs: mismo criterio.
        /// </summary>
        private static void CalcularEstados(List<NodoDestajoDto> nodos)
        {
            var porCategoria = nodos.Where(n => n.Nivel == 1).GroupBy(n => n.ParentId);
            foreach (var grupo in porCategoria)
            {
                var destajos = grupo.ToList();
                for (int i = 0; i < destajos.Count; i++)
                {
                    var d = destajos[i];
                    bool desbloqueado = i == 0 || destajos[i - 1].Finalizado;
                    d.Estado =
                        (!desbloqueado && !d.Finalizado && !d.DesatajoActivado) ? "Bloqueado"
                        : d.Finalizado ? "Terminado"
                        : d.DesatajoActivado ? "Activado"
                        : "Disponible";
                }
            }
        }

        private static ResumenDestajosDto CalcularResumen(List<NodoDestajoDto> nodos)
        {
            var destajos = nodos.Where(n => n.Nivel == 1).ToList();
            var finIds = new HashSet<int>(destajos.Where(d => d.Finalizado).Select(d => d.Id));
            var hijos = nodos.Where(n => n.Nivel == 2).ToList();
            var materiales = hijos.Where(h => h.TipoTarea == TipoTareaMaterial).ToList();

            decimal totalEco = hijos.Sum(h => h.Importe);
            var hijosFin = hijos.Where(h => finIds.Contains(h.ParentId)).ToList();
            decimal ganado = hijosFin.Sum(h => h.Importe);

            return new ResumenDestajosDto
            {
                Categorias = nodos.Count(n => n.Nivel == 0),
                Destajos = destajos.Count,
                Terminados = destajos.Count(d => d.Estado == "Terminado"),
                Activados = destajos.Count(d => d.Estado == "Activado"),
                Disponibles = destajos.Count(d => d.Estado == "Disponible"),
                Bloqueados = destajos.Count(d => d.Estado == "Bloqueado"),
                Insumos = materiales.Count,
                InsumosPendientes = materiales.Count(m => m.Surtido < m.Cantidad),
                ImporteTotal = totalEco,
                ImporteTerminado = ganado,
                AvancePct = totalEco > 0m ? (int)Math.Round(ganado / totalEco * 100m) : 0
            };
        }

        /// <summary>GET /api/destajos/cuadrillas</summary>
        [HttpGet, Route("cuadrillas")]
        public IHttpActionResult Cuadrillas()
        {
            var lista = new List<CuadrillaDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaMiembrosCuadrilla(conn);
                using (var cmd = new SqlCommand(@"
                    SELECT CodigoCuadrilla, COUNT(*) AS Miembros,
                           MAX(CASE WHEN EsJefe = 1 THEN Nombre END) AS Jefe
                    FROM MiembrosCuadrilla
                    GROUP BY CodigoCuadrilla
                    ORDER BY CodigoCuadrilla", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new CuadrillaDto
                        {
                            Codigo = reader["CodigoCuadrilla"].ToString(),
                            Miembros = Convert.ToInt32(reader["Miembros"]),
                            Jefe = reader["Jefe"] == DBNull.Value ? "" : reader["Jefe"].ToString()
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/destajos/cuadrillas/{codigo}/miembros</summary>
        [HttpGet, Route("cuadrillas/{codigo}/miembros")]
        public IHttpActionResult MiembrosCuadrilla(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest("Falta el código de cuadrilla.");

            var lista = new List<MiembroCuadrillaDestajoDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaMiembrosCuadrilla(conn);
                using (var cmd = new SqlCommand(@"
                    SELECT m.Nombre, m.Rol, m.EsJefe, m.Telefono, t.ClaveTrabajador
                    FROM MiembrosCuadrilla m
                    LEFT JOIN TRABAJADORES t ON t.IdTrabajador = m.IdTrabajador
                    WHERE m.CodigoCuadrilla = @codigo
                    ORDER BY m.EsJefe DESC, m.Nombre", conn))
                {
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new MiembroCuadrillaDestajoDto
                            {
                                Clave = reader["ClaveTrabajador"] == DBNull.Value ? "" : reader["ClaveTrabajador"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Rol = reader["Rol"].ToString(),
                                EsJefe = Convert.ToBoolean(reader["EsJefe"]),
                                Telefono = reader["Telefono"] == DBNull.Value ? "" : reader["Telefono"].ToString()
                            });
                        }
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>
        /// GET /api/destajos/reporte-semana?desde=&amp;hasta= — destajos activados,
        /// terminados o pendientes en el periodo, ambas rutas. Replica el SQL de
        /// FormReporteDestajosSemana.ConsultarRuta; el cliente arma el árbol
        /// Estado→Casa→Destajo igual que el formulario clásico.
        /// </summary>
        [HttpGet, Route("reporte-semana")]
        public IHttpActionResult ReporteSemana(DateTime desde, DateTime hasta)
        {
            DateTime hastaFin = hasta.Date.AddDays(1).AddSeconds(-1);
            var lista = new List<RegistroSemanaDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaActivacion(conn);
                foreach (var ruta in new[] { TablaRutaCalandra, TablaRutaTunera })
                {
                    if (!ExisteTablaRuta(conn, ruta)) continue;
                    lista.AddRange(ConsultarReporteSemanaDeRuta(conn, ruta, desde.Date, hastaFin));
                }
            }
            return Ok(lista);
        }

        private static bool ExisteTablaRuta(SqlConnection conn, string nombre)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM sys.tables WHERE name = @t", conn))
            {
                cmd.Parameters.AddWithValue("@t", nombre);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static List<RegistroSemanaDto> ConsultarReporteSemanaDeRuta(
            SqlConnection conn, string ruta, DateTime desde, DateTime hasta)
        {
            var lista = new List<RegistroSemanaDto>();
            string sql = $@"
                SELECT
                    a.Manzana, a.Lote, a.NodoID,
                    d.Nombre        AS DestajoNombre,
                    cat.Nombre      AS CategoriaNombre,
                    a.CuadrillaAsignada,
                    a.Finalizado,
                    a.FechaActivacion,
                    a.FechaActualizacion,
                    a.FechaFinalizacion
                FROM ActivacionTareasRuta a
                INNER JOIN [{ruta}] d   ON d.ID = a.NodoID
                LEFT  JOIN [{ruta}] cat ON cat.ID = d.ParentId
                WHERE a.Ruta = @ruta
                  AND a.DesatajoActivado = 1
                  AND (
                        ISNULL(a.FechaActivacion, a.FechaActualizacion) BETWEEN @desde AND @hasta
                     OR a.FechaFinalizacion BETWEEN @desde AND @hasta
                     OR a.Finalizado = 0
                  )";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ruta", ruta);
                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime? fAct = reader["FechaActivacion"] == DBNull.Value
                            ? (reader["FechaActualizacion"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["FechaActualizacion"]))
                            : Convert.ToDateTime(reader["FechaActivacion"]);

                        lista.Add(new RegistroSemanaDto
                        {
                            Manzana = reader["Manzana"].ToString(),
                            Lote = reader["Lote"].ToString(),
                            Ruta = ruta,
                            DestajoNombre = reader["DestajoNombre"].ToString(),
                            CategoriaNombre = reader["CategoriaNombre"] == DBNull.Value
                                ? "(Sin categoría)" : reader["CategoriaNombre"].ToString(),
                            Cuadrilla = reader["CuadrillaAsignada"] == DBNull.Value
                                ? "" : reader["CuadrillaAsignada"].ToString(),
                            FechaActivacion = fAct,
                            FechaFinalizacion = reader["FechaFinalizacion"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(reader["FechaFinalizacion"]),
                            Finalizado = reader["Finalizado"] != DBNull.Value && Convert.ToBoolean(reader["Finalizado"])
                        });
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// GET /api/destajos/reporte-cuadrilla?desde=&amp;hasta=&amp;manzana=&amp;lote= —
        /// destajos finalizados con cuadrilla asignada, ambas rutas. `manzana`/`lote`
        /// opcionales (checkbox "solo casa actual" del formulario clásico). Replica
        /// el SQL de FormDestajosPorCuadrilla.ConsultarRegistrosDeRuta.
        /// </summary>
        [HttpGet, Route("reporte-cuadrilla")]
        public IHttpActionResult ReporteCuadrilla(DateTime desde, DateTime hasta, string manzana = null, string lote = null)
        {
            DateTime hastaFin = hasta.Date.AddDays(1).AddSeconds(-1);
            bool soloCasa = !string.IsNullOrWhiteSpace(manzana) && !string.IsNullOrWhiteSpace(lote);

            var lista = new List<RegistroCuadrillaDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaActivacion(conn);
                EnsureColumnasNominaDistribuida(conn);
                foreach (var ruta in new[] { TablaRutaCalandra, TablaRutaTunera })
                {
                    if (!ExisteTablaRuta(conn, ruta)) continue;
                    lista.AddRange(ConsultarReporteCuadrillaDeRuta(conn, ruta, desde.Date, hastaFin, soloCasa, manzana, lote));
                }
            }
            return Ok(lista);
        }

        private static List<RegistroCuadrillaDto> ConsultarReporteCuadrillaDeRuta(
            SqlConnection conn, string ruta, DateTime desde, DateTime hasta,
            bool soloCasa, string manzana, string lote)
        {
            var lista = new List<RegistroCuadrillaDto>();

            // Importe por hijos (Nivel 2) con TRY_CAST para tolerar valores no
            // numéricos en la tabla de columnas dinámicas — igual que el cliente.
            string sql = $@"
                SELECT
                    a.CuadrillaAsignada, a.Manzana, a.Lote,
                    a.NodoID                AS DestajoID,
                    d.Nombre                AS DestajoNombre,
                    a.FechaFinalizacion,
                    cat.ID                  AS CategoriaID,
                    cat.Nombre              AS CategoriaNombre,
                    ISNULL(imp.Importe, 0)  AS Importe,
                    ISNULL(a.NominaDistribuida, 0)   AS NominaDistribuida,
                    a.FechaDistribucionNomina        AS FechaDistribucionNomina
                FROM ActivacionTareasRuta a
                INNER JOIN [{ruta}] d   ON d.ID = a.NodoID
                INNER JOIN [{ruta}] cat ON cat.ID = d.ParentId
                OUTER APPLY (
                    SELECT SUM(
                        ISNULL(TRY_CAST(cant.Valor AS DECIMAL(18,4)), 0) *
                        ISNULL(TRY_CAST(prec.Valor AS DECIMAL(18,4)), 0)
                    ) AS Importe
                    FROM [{ruta}] h
                    LEFT JOIN [{ruta}_Columnas] cant ON cant.NodoID = h.ID AND cant.NombreColumna = 'Cantidad'
                    LEFT JOIN [{ruta}_Columnas] prec ON prec.NodoID = h.ID AND prec.NombreColumna = 'Precio'
                    WHERE h.ParentId = d.ID
                ) imp
                WHERE a.Finalizado = 1
                  AND a.Ruta = @ruta
                  AND a.CuadrillaAsignada IS NOT NULL
                  AND LTRIM(RTRIM(a.CuadrillaAsignada)) <> ''
                  AND a.FechaFinalizacion BETWEEN @desde AND @hasta
                  {(soloCasa ? "AND a.Manzana = @m AND a.Lote = @l" : "")}
                ORDER BY a.CuadrillaAsignada, cat.Orden, d.Orden, a.Manzana, a.Lote";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ruta", ruta);
                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta);
                if (soloCasa)
                {
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new RegistroCuadrillaDto
                        {
                            Cuadrilla = reader["CuadrillaAsignada"].ToString(),
                            Manzana = reader["Manzana"].ToString(),
                            Lote = reader["Lote"].ToString(),
                            DestajoId = Convert.ToInt32(reader["DestajoID"]),
                            DestajoNombre = reader["DestajoNombre"].ToString(),
                            FechaFinalizacion = reader["FechaFinalizacion"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(reader["FechaFinalizacion"]),
                            CategoriaId = Convert.ToInt32(reader["CategoriaID"]),
                            CategoriaNombre = reader["CategoriaNombre"].ToString(),
                            Importe = reader["Importe"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Importe"]),
                            Ruta = ruta,
                            NominaDistribuida = reader["NominaDistribuida"] != DBNull.Value && Convert.ToBoolean(reader["NominaDistribuida"]),
                            FechaDistribucionNomina = reader["FechaDistribucionNomina"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(reader["FechaDistribucionNomina"])
                        });
                    }
                }
            }
            return lista;
        }

        private static void EnsureColumnasNominaDistribuida(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ActivacionTareasRuta')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE Name = N'NominaDistribuida' AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
        ALTER TABLE ActivacionTareasRuta ADD NominaDistribuida BIT NOT NULL DEFAULT 0;
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE Name = N'FechaDistribucionNomina' AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
        ALTER TABLE ActivacionTareasRuta ADD FechaDistribucionNomina DATETIME NULL;
END";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }

        // ==================================================================
        // Escritura
        // ==================================================================

        /// <summary>POST /api/destajos/activar</summary>
        [HttpPost, Route("activar"), RequierePermiso("destajos.editar")]
        public IHttpActionResult Activar([FromBody] ActivarDestajoRequest req)
        {
            if (req == null) return BadRequest("Falta el cuerpo de la petición.");
            if (string.IsNullOrWhiteSpace(req.Cuadrilla))
                return BadRequest("Falta la cuadrilla.");
            string tablaRuta = ValidarRuta(req.Ruta);
            if (tablaRuta == null) return BadRequest("Ruta inválida.");

            using (var conn = Db.Abrir())
            {
                if (!EstaDesbloqueado(conn, tablaRuta, req.NodoId, out string error))
                    return BadRequest(error);

                EnsureTablaActivacion(conn);
                string nombre = ObtenerNombreNodo(conn, tablaRuta, req.NodoId);
                var ahora = DateTime.Now;

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        PersistirActivacion(conn, tx, req, nombre,
                            activa: true, cuadrilla: req.Cuadrilla, desatajoActivado: true,
                            finalizado: false, fechaActivacion: ahora, fechaFinalizacion: null);
                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }

                return Ok(new ResultadoDestajoDto
                {
                    Ok = true,
                    Mensaje = "Destajo activado.",
                    Nodo = new NodoDestajoDto
                    {
                        Id = req.NodoId, Nombre = nombre, Nivel = 1, Activa = true,
                        CuadrillaAsignada = req.Cuadrilla, DesatajoActivado = true,
                        Finalizado = false, FechaActivacion = ahora, Estado = "Activado"
                    }
                });
            }
        }

        /// <summary>POST /api/destajos/finalizar</summary>
        [HttpPost, Route("finalizar"), RequierePermiso("destajos.editar")]
        public IHttpActionResult Finalizar([FromBody] DestajoRefRequest req)
        {
            if (req == null) return BadRequest("Falta el cuerpo de la petición.");
            string tablaRuta = ValidarRuta(req.Ruta);
            if (tablaRuta == null) return BadRequest("Ruta inválida.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaActivacion(conn);
                var activacion = CargarActivacionNodo(conn, req);
                if (activacion == null || !activacion.DesatajoActivado || string.IsNullOrEmpty(activacion.CuadrillaAsignada))
                    return BadRequest("Sólo se puede finalizar un destajo previamente activado con cuadrilla.");
                if (activacion.Finalizado)
                    return BadRequest("Este destajo ya está finalizado.");

                string nombre = ObtenerNombreNodo(conn, tablaRuta, req.NodoId);
                var ahora = DateTime.Now;

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        PersistirActivacion(conn, tx, req, nombre,
                            activa: true, cuadrilla: activacion.CuadrillaAsignada, desatajoActivado: true,
                            finalizado: true, fechaActivacion: activacion.FechaActivacion, fechaFinalizacion: ahora);
                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }

                return Ok(new ResultadoDestajoDto
                {
                    Ok = true,
                    Mensaje = "Destajo finalizado.",
                    Nodo = new NodoDestajoDto
                    {
                        Id = req.NodoId, Nombre = nombre, Nivel = 1, Activa = true,
                        CuadrillaAsignada = activacion.CuadrillaAsignada, DesatajoActivado = true,
                        Finalizado = true, FechaActivacion = activacion.FechaActivacion,
                        FechaFinalizacion = ahora, Estado = "Terminado"
                    }
                });
            }
        }

        /// <summary>
        /// Cuadrilla genérica con la que Avance Masivo activa/finaliza destajos en
        /// bloque: no hay selector de cuadrilla real en ese formulario (decisión de
        /// producto), así que estas activaciones quedan marcadas para reasignar la
        /// cuadrilla real después en Destajos si hace falta para nómina.
        /// </summary>
        private const string CuadrillaAvanceMasivo = "ADMINISTRATIVO";

        /// <summary>
        /// POST /api/destajos/avance-masivo · finaliza uno o más destajos en varias
        /// casas a la vez (usa la cuadrilla genérica <see cref="CuadrillaAvanceMasivo"/>).
        /// Como un destajo sólo se puede finalizar si el anterior de su categoría ya
        /// está terminado (ver EstaDesbloqueado/CalcularEstados), aquí se finaliza
        /// también, en cascada, cualquier destajo previo pendiente de esa categoría
        /// en esa casa — así el destajo pedido siempre queda desbloqueado.
        /// </summary>
        [HttpPost, Route("avance-masivo"), RequierePermiso("destajos.editar")]
        public IHttpActionResult AvanceMasivo([FromBody] AvanceMasivoDestajosRequest req)
        {
            if (req?.Casas == null || req.Casas.Count == 0)
                return BadRequest("No se recibieron casas a actualizar.");
            if (req?.Destajos == null || req.Destajos.Count == 0)
                return BadRequest("No se recibieron destajos a marcar.");

            int ok = 0;
            var errores = new List<string>();

            using (var conn = Db.Abrir())
            {
                EnsureTablaActivacion(conn);

                // Cadena de desbloqueo (Nivel 1 por categoría, en Orden) y nombres,
                // una sola vez por ruta en vez de una vez por casa.
                var cadenasPorRuta = new Dictionary<string, Dictionary<int, List<int>>>();
                var nombresPorRuta = new Dictionary<string, Dictionary<int, string>>();
                var padrePorRuta = new Dictionary<string, Dictionary<int, int>>();

                void CargarRutaSiFalta(string tablaRuta)
                {
                    if (cadenasPorRuta.ContainsKey(tablaRuta)) return;
                    var nivel1 = CargarNodos(conn, tablaRuta).Where(n => n.Nivel == 1).ToList();
                    cadenasPorRuta[tablaRuta] = nivel1.GroupBy(n => n.ParentId)
                        .ToDictionary(g => g.Key, g => g.Select(n => n.Id).ToList());
                    nombresPorRuta[tablaRuta] = nivel1.ToDictionary(n => n.Id, n => n.Nombre);
                    padrePorRuta[tablaRuta] = nivel1.ToDictionary(n => n.Id, n => n.ParentId);
                }
                CargarRutaSiFalta(TablaRutaTunera);
                CargarRutaSiFalta(TablaRutaCalandra);

                foreach (var c in req.Casas)
                {
                    string tablaRuta = RutaDeprototipo(c.Prototipo);
                    var activaciones = CargarActivaciones(conn, c.Manzana, c.Lote, tablaRuta);
                    var ahora = DateTime.Now;
                    int okLocal = 0;
                    var erroresLocal = new List<string>();

                    using (var tx = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var item in req.Destajos)
                            {
                                int? nodoIdN = tablaRuta == TablaRutaTunera ? item.NodoIdTunera : item.NodoIdCalandra;
                                if (nodoIdN == null)
                                {
                                    erroresLocal.Add($"M{c.Manzana}-L{c.Lote}: ese destajo no existe en la ruta de esta casa.");
                                    continue;
                                }
                                int nodoId = nodoIdN.Value;
                                if (!padrePorRuta[tablaRuta].TryGetValue(nodoId, out int parentId))
                                {
                                    erroresLocal.Add($"M{c.Manzana}-L{c.Lote}: destajo no encontrado.");
                                    continue;
                                }

                                var cadena = cadenasPorRuta[tablaRuta][parentId];
                                int idx = cadena.IndexOf(nodoId);
                                for (int i = 0; i <= idx; i++)
                                {
                                    int nid = cadena[i];
                                    activaciones.TryGetValue(nid, out var act);
                                    if (act != null && act.Finalizado) continue;

                                    var refNodo = new DestajoRefRequest
                                    {
                                        Manzana = c.Manzana, Lote = c.Lote, Ruta = tablaRuta,
                                        Prototipo = c.Prototipo, NodoId = nid
                                    };
                                    var fechaActivacion = act?.FechaActivacion ?? ahora;
                                    PersistirActivacion(conn, tx, refNodo, nombresPorRuta[tablaRuta][nid],
                                        activa: true, cuadrilla: CuadrillaAvanceMasivo, desatajoActivado: true,
                                        finalizado: true, fechaActivacion: fechaActivacion, fechaFinalizacion: ahora);

                                    activaciones[nid] = new ActivacionInfo
                                    {
                                        Activa = true, CuadrillaAsignada = CuadrillaAvanceMasivo,
                                        DesatajoActivado = true, Finalizado = true,
                                        FechaActivacion = fechaActivacion, FechaFinalizacion = ahora
                                    };
                                }
                                okLocal++;
                            }
                            tx.Commit();
                            ok += okLocal;
                            errores.AddRange(erroresLocal);
                        }
                        catch (Exception ex)
                        {
                            tx.Rollback();
                            errores.Add($"M{c.Manzana}-L{c.Lote}: {ex.Message}");
                        }
                    }
                }
            }

            return Ok(new AvanceMasivoResultadoDto { Ok = ok, Errores = errores });
        }

        /// <summary>POST /api/destajos/desactivar · [ADMIN]</summary>
        [HttpPost, Route("desactivar"), RequierePermiso("destajos.editar")]
        public IHttpActionResult Desactivar([FromBody] DesactivarDestajoRequest req)
        {
            if (req == null) return BadRequest("Falta el cuerpo de la petición.");
            if (!EsAdmin(User))
                return StatusCode(HttpStatusCode.Forbidden);
            string tablaRuta = ValidarRuta(req.Ruta);
            if (tablaRuta == null) return BadRequest("Ruta inválida.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaActivacion(conn);
                var activacion = CargarActivacionNodo(conn, req);
                if (activacion == null || !activacion.DesatajoActivado)
                    return BadRequest("Este destajo ya está desactivado.");

                bool liberado = YaLiberado(conn, tablaRuta, req.NodoId, req.Manzana, req.Lote);
                if (liberado)
                {
                    if (string.IsNullOrWhiteSpace(req.Justificacion))
                        return BadRequest("El destajo ya liberó insumos: la justificación es obligatoria.");
                    RegistrarExcepcionLiberado(conn, req, tablaRuta, User?.Identity?.Name);
                }

                string nombre = ObtenerNombreNodo(conn, tablaRuta, req.NodoId);

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        PersistirActivacion(conn, tx, req, nombre,
                            activa: false, cuadrilla: null, desatajoActivado: false,
                            finalizado: false, fechaActivacion: null, fechaFinalizacion: null);
                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }

                return Ok(new ResultadoDestajoDto
                {
                    Ok = true,
                    Mensaje = "Destajo desactivado.",
                    Nodo = new NodoDestajoDto
                    {
                        Id = req.NodoId, Nombre = nombre, Nivel = 1, Activa = false,
                        DesatajoActivado = false, Finalizado = false, Estado = "Disponible"
                    }
                });
            }
        }

        /// <summary>POST /api/destajos/reabrir · [ADMIN]</summary>
        [HttpPost, Route("reabrir"), RequierePermiso("destajos.editar")]
        public IHttpActionResult Reabrir([FromBody] ReabrirDestajoRequest req)
        {
            if (req == null) return BadRequest("Falta el cuerpo de la petición.");
            if (!EsAdmin(User))
                return StatusCode(HttpStatusCode.Forbidden);
            string tablaRuta = ValidarRuta(req.Ruta);
            if (tablaRuta == null) return BadRequest("Ruta inválida.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaActivacion(conn);
                var activacion = CargarActivacionNodo(conn, req);
                if (activacion == null || !activacion.Finalizado)
                    return BadRequest("Sólo se puede reabrir un destajo que ya está finalizado.");

                if (req.BorrarNomina)
                    BorrarNominaDeManoDeObra(conn, tablaRuta, req);

                string nombre = ObtenerNombreNodo(conn, tablaRuta, req.NodoId);

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        PersistirActivacion(conn, tx, req, nombre,
                            activa: activacion.Activa, cuadrilla: activacion.CuadrillaAsignada, desatajoActivado: true,
                            finalizado: false, fechaActivacion: activacion.FechaActivacion, fechaFinalizacion: null);
                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }

                return Ok(new ResultadoDestajoDto
                {
                    Ok = true,
                    Mensaje = "Destajo reabierto.",
                    Nodo = new NodoDestajoDto
                    {
                        Id = req.NodoId, Nombre = nombre, Nivel = 1, Activa = activacion.Activa,
                        CuadrillaAsignada = activacion.CuadrillaAsignada, DesatajoActivado = true,
                        Finalizado = false, FechaActivacion = activacion.FechaActivacion, Estado = "Activado"
                    }
                });
            }
        }

        /// <summary>POST /api/destajos/guardar · reemplazo en bloque (btnGuardar de hoy).</summary>
        [HttpPost, Route("guardar"), RequierePermiso("destajos.editar")]
        public IHttpActionResult Guardar([FromBody] GuardarActivacionesRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Faltan manzana y/o lote.");
            string tablaRuta = ValidarRuta(req.Ruta);
            if (tablaRuta == null) return BadRequest("Ruta inválida.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaActivacion(conn);

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmdDel = new SqlCommand(
                            "DELETE FROM ActivacionTareasRuta WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta",
                            conn, tx))
                        {
                            cmdDel.Parameters.AddWithValue("@m", req.Manzana);
                            cmdDel.Parameters.AddWithValue("@l", req.Lote);
                            cmdDel.Parameters.AddWithValue("@ruta", tablaRuta);
                            cmdDel.ExecuteNonQuery();
                        }

                        foreach (var nodo in req.Nodos ?? new List<NodoActivacionRequest>())
                        {
                            DateTime? fechaActivacion = nodo.DesatajoActivado
                                ? (nodo.FechaActivacion ?? DateTime.Now)
                                : (DateTime?)null;

                            InsertarActivacion(conn, tx, req.Manzana, req.Lote, req.Prototipo, tablaRuta,
                                nodo.NodoId, nodo.Nombre, nodo.Activa, nodo.CuadrillaAsignada, nodo.DesatajoActivado,
                                nodo.Finalizado, nodo.FechaFinalizacion, fechaActivacion);
                        }

                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }
            }

            return Ok(new ResultadoDestajoDto { Ok = true, Mensaje = "Configuración guardada." });
        }

        /// <summary>POST /api/destajos/pdf · archiva en BD un PDF ya generado en el cliente.</summary>
        [HttpPost, Route("pdf"), RequierePermiso("destajos.editar")]
        public IHttpActionResult GuardarPdf([FromBody] GuardarPdfDestajoRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.ContenidoBase64) || string.IsNullOrWhiteSpace(req.NombreArchivo))
                return BadRequest("Faltan datos del PDF.");

            byte[] bytes;
            try { bytes = Convert.FromBase64String(req.ContenidoBase64); }
            catch { return BadRequest("El contenido del PDF no es base64 válido."); }

            using (var conn = Db.Abrir())
            {
                EnsureTablaPdfsDestajos(conn);
                using (var cmd = new SqlCommand(@"
                    INSERT INTO PDFsDestajos
                        (Manzana, Lote, Prototipo, Ruta, NodoID, NombreDestajo,
                         CuadrillaAsignada, NombreArchivo, ContenidoPDF, TamanioBytes,
                         Usuario, FechaGeneracion)
                    VALUES
                        (@m, @l, @proto, @ruta, @nodo, @nombre,
                         @cuadrilla, @archivo, @contenido, @tam,
                         @usuario, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@m", req.Manzana ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@l", req.Lote ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@proto", (object)req.Prototipo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ruta", (object)req.Ruta ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nodo", req.NodoId);
                    cmd.Parameters.AddWithValue("@nombre", (object)req.NombreDestajo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cuadrilla",
                        string.IsNullOrEmpty(req.CuadrillaAsignada) ? (object)DBNull.Value : req.CuadrillaAsignada);
                    cmd.Parameters.AddWithValue("@archivo", req.NombreArchivo);
                    cmd.Parameters.Add("@contenido", System.Data.SqlDbType.VarBinary, -1).Value = bytes;
                    cmd.Parameters.AddWithValue("@tam", (long)bytes.Length);
                    cmd.Parameters.AddWithValue("@usuario", User?.Identity?.Name ?? "");
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new ResultadoDestajoDto { Ok = true, Mensaje = "PDF archivado." });
        }

        // ==================================================================
        // Helpers de escritura
        // ==================================================================

        private static bool EstaDesbloqueado(SqlConnection conn, string tablaRuta, int nodoId, out string error)
        {
            error = null;
            int parentId;
            using (var cmd = new SqlCommand($"SELECT ParentId, Orden FROM [{tablaRuta}] WHERE ID = @id AND Nivel = 1", conn))
            {
                cmd.Parameters.AddWithValue("@id", nodoId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        error = "El destajo no existe.";
                        return false;
                    }
                    parentId = Convert.ToInt32(reader["ParentId"]);
                }
            }

            var hermanos = new List<(int Id, int Orden)>();
            using (var cmd = new SqlCommand(
                $"SELECT ID, Orden FROM [{tablaRuta}] WHERE Nivel = 1 AND ParentId = @p ORDER BY Orden", conn))
            {
                cmd.Parameters.AddWithValue("@p", parentId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        hermanos.Add((Convert.ToInt32(reader["ID"]), Convert.ToInt32(reader["Orden"])));
                }
            }

            int idx = hermanos.FindIndex(h => h.Id == nodoId);
            if (idx <= 0) return true;

            int anteriorId = hermanos[idx - 1].Id;
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(Finalizado, 0) FROM ActivacionTareasRuta
                WHERE NodoID = @nodo", conn))
            {
                cmd.Parameters.AddWithValue("@nodo", anteriorId);
                var result = cmd.ExecuteScalar();
                bool finalizadoAnterior = result != null && result != DBNull.Value && Convert.ToBoolean(result);
                if (!finalizadoAnterior)
                {
                    error = "Termina el destajo anterior de la categoría antes de activar éste.";
                    return false;
                }
            }
            return true;
        }

        private static string ObtenerNombreNodo(SqlConnection conn, string tablaRuta, int nodoId)
        {
            using (var cmd = new SqlCommand($"SELECT Nombre FROM [{tablaRuta}] WHERE ID = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", nodoId);
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
        }

        private static ActivacionInfo CargarActivacionNodo(SqlConnection conn, DestajoRefRequest req)
        {
            using (var cmd = new SqlCommand(@"
                SELECT Activa, CuadrillaAsignada, DesatajoActivado, ISNULL(Finalizado, 0) AS Finalizado,
                       FechaActivacion, FechaFinalizacion
                FROM ActivacionTareasRuta
                WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta AND NodoID = @nodo", conn))
            {
                cmd.Parameters.AddWithValue("@m", req.Manzana);
                cmd.Parameters.AddWithValue("@l", req.Lote);
                cmd.Parameters.AddWithValue("@ruta", req.Ruta);
                cmd.Parameters.AddWithValue("@nodo", req.NodoId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new ActivacionInfo
                    {
                        Activa = Convert.ToBoolean(reader["Activa"]),
                        CuadrillaAsignada = reader["CuadrillaAsignada"] == DBNull.Value ? "" : reader["CuadrillaAsignada"].ToString(),
                        DesatajoActivado = reader["DesatajoActivado"] != DBNull.Value && Convert.ToBoolean(reader["DesatajoActivado"]),
                        Finalizado = Convert.ToBoolean(reader["Finalizado"]),
                        FechaActivacion = reader["FechaActivacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaActivacion"]),
                        FechaFinalizacion = reader["FechaFinalizacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaFinalizacion"])
                    };
                }
            }
        }

        /// <summary>DELETE + INSERT de un solo nodo (upsert artesanal, igual que el cliente).</summary>
        private static void PersistirActivacion(
            SqlConnection conn, SqlTransaction tx, DestajoRefRequest req, string nombre,
            bool activa, string cuadrilla, bool desatajoActivado, bool finalizado,
            DateTime? fechaActivacion, DateTime? fechaFinalizacion)
        {
            using (var cmdDel = new SqlCommand(@"
                DELETE FROM ActivacionTareasRuta
                WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta AND NodoID = @nodo", conn, tx))
            {
                cmdDel.Parameters.AddWithValue("@m", req.Manzana);
                cmdDel.Parameters.AddWithValue("@l", req.Lote);
                cmdDel.Parameters.AddWithValue("@ruta", req.Ruta);
                cmdDel.Parameters.AddWithValue("@nodo", req.NodoId);
                cmdDel.ExecuteNonQuery();
            }

            InsertarActivacion(conn, tx, req.Manzana, req.Lote, req.Prototipo, req.Ruta,
                req.NodoId, nombre, activa, cuadrilla, desatajoActivado, finalizado,
                fechaFinalizacion, fechaActivacion);
        }

        private static void InsertarActivacion(
            SqlConnection conn, SqlTransaction tx, string manzana, string lote, string prototipo, string ruta,
            int nodoId, string nombre, bool activa, string cuadrilla, bool desatajoActivado, bool finalizado,
            DateTime? fechaFinalizacion, DateTime? fechaActivacion)
        {
            using (var cmdIns = new SqlCommand(@"
                INSERT INTO ActivacionTareasRuta
                (Manzana, Lote, Prototipo, Ruta, NodoID, NombreTarea, Activa, CuadrillaAsignada, DesatajoActivado,
                 Finalizado, FechaFinalizacion, FechaActualizacion, FechaActivacion)
                VALUES (@m, @l, @proto, @ruta, @nodo, @nombre, @activa, @cuadrilla, @desatActivado,
                        @finalizado, @fechaFin, GETDATE(), @fechaAct)", conn, tx))
            {
                cmdIns.Parameters.AddWithValue("@m", manzana ?? "");
                cmdIns.Parameters.AddWithValue("@l", lote ?? "");
                cmdIns.Parameters.AddWithValue("@proto", (object)prototipo ?? DBNull.Value);
                cmdIns.Parameters.AddWithValue("@ruta", ruta ?? "");
                cmdIns.Parameters.AddWithValue("@nodo", nodoId);
                cmdIns.Parameters.AddWithValue("@nombre", nombre ?? "");
                cmdIns.Parameters.AddWithValue("@activa", activa);
                cmdIns.Parameters.AddWithValue("@cuadrilla", string.IsNullOrEmpty(cuadrilla) ? (object)DBNull.Value : cuadrilla);
                cmdIns.Parameters.AddWithValue("@desatActivado", desatajoActivado);
                cmdIns.Parameters.AddWithValue("@finalizado", finalizado);
                cmdIns.Parameters.AddWithValue("@fechaFin", fechaFinalizacion.HasValue ? (object)fechaFinalizacion.Value : DBNull.Value);
                cmdIns.Parameters.AddWithValue("@fechaAct", fechaActivacion.HasValue ? (object)fechaActivacion.Value : DBNull.Value);
                cmdIns.ExecuteNonQuery();
            }
        }

        private static bool YaLiberado(SqlConnection conn, string ruta, int nodoId, string manzana, string lote)
        {
            try
            {
                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM dbo.SalidasAlmacen
                    WHERE OrigenRuta = @r AND OrigenNodoID = @n AND Manzana = @m AND Lote = @l", conn))
                {
                    cmd.Parameters.AddWithValue("@r", ruta ?? "");
                    cmd.Parameters.AddWithValue("@n", nodoId);
                    cmd.Parameters.AddWithValue("@m", manzana ?? "");
                    cmd.Parameters.AddWithValue("@l", lote ?? "");
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
            catch
            {
                // Si la columna OrigenRuta aún no existe, no hay nada liberado.
                return false;
            }
        }

        private static void RegistrarExcepcionLiberado(SqlConnection conn, DesactivarDestajoRequest req, string tablaRuta, string usuario)
        {
            EnsureTablaExcepcionesLiberacion(conn);

            var insumos = new List<object>();
            decimal total = 0m;
            using (var cmd = new SqlCommand(@"
                SELECT Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe
                FROM dbo.SalidasAlmacen
                WHERE OrigenRuta = @r AND OrigenNodoID = @n AND Manzana = @m AND Lote = @l", conn))
            {
                cmd.Parameters.AddWithValue("@r", tablaRuta);
                cmd.Parameters.AddWithValue("@n", req.NodoId);
                cmd.Parameters.AddWithValue("@m", req.Manzana ?? "");
                cmd.Parameters.AddWithValue("@l", req.Lote ?? "");
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        decimal importe = rd["Importe"] != DBNull.Value ? Convert.ToDecimal(rd["Importe"]) : 0m;
                        total += importe;
                        insumos.Add(new
                        {
                            Codigo = rd["Clave"]?.ToString(),
                            Descripcion = rd["Descripcion"]?.ToString(),
                            Unidad = rd["Unidad"]?.ToString(),
                            Cantidad = rd["Cantidad"] != DBNull.Value ? Convert.ToDecimal(rd["Cantidad"]) : 0m,
                            PrecioUnitario = rd["PrecioUnitario"] != DBNull.Value ? Convert.ToDecimal(rd["PrecioUnitario"]) : 0m,
                            Importe = importe
                        });
                    }
                }
            }

            string nombre = ObtenerNombreNodo(conn, tablaRuta, req.NodoId);
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.ExcepcionesLiberacionAlmacen
                (Tipo, Manzana, Lote, Ruta, NodoID, NombreDestajo, Usuario, Justificacion, DetalleJson, TotalImporte)
                VALUES (@Tipo, @Manzana, @Lote, @Ruta, @NodoID, @Nombre, @Usuario, @Just, @Detalle, @Total)", conn))
            {
                cmd.Parameters.AddWithValue("@Tipo", "DesactivacionDestajoLiberado");
                cmd.Parameters.AddWithValue("@Manzana", (object)req.Manzana ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Lote", (object)req.Lote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Ruta", (object)tablaRuta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NodoID", req.NodoId);
                cmd.Parameters.AddWithValue("@Nombre", (object)nombre ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Usuario", (object)usuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Just", (object)req.Justificacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Detalle", JsonConvert.SerializeObject(insumos));
                cmd.Parameters.AddWithValue("@Total", total);
                cmd.ExecuteNonQuery();
            }
        }

        private static void BorrarNominaDeManoDeObra(SqlConnection conn, string tablaRuta, ReabrirDestajoRequest req)
        {
            var hijosManoDeObra = new List<int>();
            using (var cmd = new SqlCommand(
                $"SELECT ID FROM [{tablaRuta}] WHERE ParentId = @p AND Nivel = 2 AND TipoTarea = 2", conn))
            {
                cmd.Parameters.AddWithValue("@p", req.NodoId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) hijosManoDeObra.Add(Convert.ToInt32(reader["ID"]));
                }
            }
            if (hijosManoDeObra.Count == 0) return;

            using (var cmd = new SqlCommand(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'NominaTareasAsignada')
                    DELETE FROM NominaTareasAsignada
                    WHERE Manzana = @m AND Lote = @l AND Ruta = @r AND NodoID = @nodo", conn))
            {
                cmd.Parameters.AddWithValue("@m", req.Manzana ?? "");
                cmd.Parameters.AddWithValue("@l", req.Lote ?? "");
                cmd.Parameters.AddWithValue("@r", tablaRuta);
                var pNodo = cmd.Parameters.Add("@nodo", System.Data.SqlDbType.Int);
                foreach (var nodoId in hijosManoDeObra)
                {
                    pNodo.Value = nodoId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static decimal ParseDecimal(object valor)
        {
            if (valor == null || valor == DBNull.Value) return 0m;
            string s = valor.ToString();
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal r)) return r;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal r2)) return r2;
            return 0m;
        }

        // ==================================================================
        // DDL en el servidor (antes vivía en el cliente; ver NominaController.EnsureTabla)
        // ==================================================================

        private static void EnsureTablaActivacion(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivacionTareasRuta')
BEGIN
    CREATE TABLE ActivacionTareasRuta (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(10),
        Lote NVARCHAR(10),
        Prototipo NVARCHAR(50),
        Ruta NVARCHAR(50),
        NodoID INT,
        NombreTarea NVARCHAR(200),
        Activa BIT,
        CuadrillaAsignada NVARCHAR(20) NULL,
        DesatajoActivado BIT DEFAULT 0,
        Finalizado BIT DEFAULT 0,
        FechaFinalizacion DATETIME NULL,
        FechaActualizacion DATETIME DEFAULT GETDATE()
    );
END
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'CuadrillaAsignada' AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    ALTER TABLE ActivacionTareasRuta ADD CuadrillaAsignada NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'DesatajoActivado' AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    ALTER TABLE ActivacionTareasRuta ADD DesatajoActivado BIT DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'Finalizado' AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    ALTER TABLE ActivacionTareasRuta ADD Finalizado BIT DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'FechaFinalizacion' AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    ALTER TABLE ActivacionTareasRuta ADD FechaFinalizacion DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'FechaActivacion' AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    ALTER TABLE ActivacionTareasRuta ADD FechaActivacion DATETIME NULL;";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }

        private static void EnsureTablaExcepcionesLiberacion(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.ExcepcionesLiberacionAlmacen', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExcepcionesLiberacionAlmacen(
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Fecha] DATETIME NOT NULL CONSTRAINT DF_ExcLibAlm_Fecha DEFAULT (GETDATE()),
        [Tipo] NVARCHAR(40) NOT NULL,
        [Manzana] NVARCHAR(10) NULL,
        [Lote] NVARCHAR(10) NULL,
        [Ruta] NVARCHAR(100) NULL,
        [NodoID] INT NULL,
        [NombreDestajo] NVARCHAR(255) NULL,
        [Usuario] NVARCHAR(100) NULL,
        [Justificacion] NVARCHAR(MAX) NULL,
        [DetalleJson] NVARCHAR(MAX) NULL,
        [TotalImporte] DECIMAL(18,2) NULL,
        CONSTRAINT PK_ExcepcionesLiberacionAlmacen PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }

        private static void EnsureTablaPdfsDestajos(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PDFsDestajos')
BEGIN
    CREATE TABLE PDFsDestajos (
        Id                INT IDENTITY(1,1) PRIMARY KEY,
        Manzana           NVARCHAR(10)  NOT NULL,
        Lote              NVARCHAR(10)  NOT NULL,
        Prototipo         NVARCHAR(50)  NULL,
        Ruta              NVARCHAR(50)  NULL,
        NodoID            INT           NULL,
        NombreDestajo     NVARCHAR(200) NULL,
        CuadrillaAsignada NVARCHAR(20)  NULL,
        NombreArchivo     NVARCHAR(255) NOT NULL,
        ContenidoPDF      VARBINARY(MAX) NOT NULL,
        TamanioBytes      BIGINT        NOT NULL,
        Usuario           NVARCHAR(100) NULL,
        FechaGeneracion   DATETIME      NOT NULL DEFAULT GETDATE()
    );
    CREATE INDEX IX_PDFsDestajos_ManzanaLote ON PDFsDestajos(Manzana, Lote);
    CREATE INDEX IX_PDFsDestajos_Fecha       ON PDFsDestajos(FechaGeneracion DESC);
    CREATE INDEX IX_PDFsDestajos_Ruta        ON PDFsDestajos(Ruta);
END";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }

        private static void EnsureTablaMiembrosCuadrilla(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.MiembrosCuadrilla') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.MiembrosCuadrilla (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CodigoCuadrilla NVARCHAR(20) NOT NULL,
        IdTrabajador INT NULL,
        Nombre NVARCHAR(200) NOT NULL,
        Rol NVARCHAR(50) NOT NULL,
        EsJefe BIT NOT NULL DEFAULT(0),
        Telefono NVARCHAR(50) NULL,
        Foto VARBINARY(MAX) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
    );
END";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }
    }
}
