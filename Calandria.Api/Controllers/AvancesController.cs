using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Avance de obra jerárquico por casa (FormAvanceObra y FormHardProgress).
    /// El servidor entrega los datos crudos (partidas de PresupuestoObra + avances
    /// guardados de AvanceManualObra) y persiste el avance de cada partida; el
    /// cliente conserva el armado del árbol y las gráficas. Manzanas/lotes/prototipo
    /// salen de InventarioCasas (todas las casas), no de ActivacionTareasRuta.
    /// </summary>
    [RoutePrefix("api/avances"), RequierePermiso("estimaciones.ver")]
    public class AvancesController : ApiController
    {
        /// <summary>GET /api/avances/manzanas · todas las manzanas del inventario.</summary>
        [HttpGet, Route("manzanas")]
        public IHttpActionResult Manzanas()
        {
            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    if (r["Manzana"] != DBNull.Value) lista.Add(r["Manzana"].ToString());
            }
            return Ok(lista);
        }

        /// <summary>GET /api/avances/lotes?manzana=X · lotes de una manzana.</summary>
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
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        if (r["Lote"] != DBNull.Value) lista.Add(r["Lote"].ToString());
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/avances/prototipo?manzana=X&amp;lote=Y · prototipo de la casa ("" si no hay).</summary>
        [HttpGet, Route("prototipo")]
        public IHttpActionResult Prototipo(string manzana, string lote)
        {
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana ?? "");
                cmd.Parameters.AddWithValue("@l", lote ?? "");
                var result = cmd.ExecuteScalar();
                return Ok((result?.ToString() ?? "").Trim());
            }
        }

        /// <summary>
        /// GET /api/avances/jerarquico?manzana=&amp;lote=&amp;prototipo= · partidas de
        /// PresupuestoObra (con la columna de costo según el prototipo) + avances
        /// guardados, para que el cliente arme el árbol.
        /// </summary>
        [HttpGet, Route("jerarquico")]
        public IHttpActionResult Jerarquico(string manzana, string lote, string prototipo = null)
        {
            var resp = new JerarquicoAvanceResponse();

            using (var conn = Db.Abrir())
            {
                EnsureTablaAvanceManualObra(conn);

                var columnas = ColumnasDe(conn, "PresupuestoObra");

                // Columna de costo según prototipo, con fallback si no existe.
                string columnaCosto = (prototipo ?? "").ToUpperInvariant().Contains("TUNERA")
                    ? "CostoTunera" : "CostoCalandra";
                if (!columnas.Contains(columnaCosto))
                {
                    columnaCosto = columnas.Contains("TOTAL") ? "TOTAL"
                        : columnas.Contains("CostoCalandra") ? "CostoCalandra" : "CostoTunera";
                }

                bool tieneCodigo = columnas.Contains("Codigo");

                string sql = tieneCodigo
                    ? $@"
SELECT
    ROW_NUMBER() OVER (
        ORDER BY
            CASE WHEN TRY_CAST(Codigo AS INT) IS NOT NULL THEN TRY_CAST(Codigo AS INT) ELSE 999999 END,
            Codigo, Etapa, Partida) AS WBS,
    Codigo, Padre, Etapa, Partida,
    ISNULL(CAST([{columnaCosto}] AS FLOAT), 0) AS ImporteTotal
FROM PresupuestoObra
ORDER BY
    CASE WHEN TRY_CAST(Codigo AS INT) IS NOT NULL THEN TRY_CAST(Codigo AS INT) ELSE 999999 END,
    Codigo, Etapa, Partida"
                    : $@"
SELECT
    ROW_NUMBER() OVER (ORDER BY Padre, Etapa, Partida) AS WBS,
    Padre, Etapa, Partida,
    ISNULL(CAST([{columnaCosto}] AS FLOAT), 0) AS ImporteTotal
FROM PresupuestoObra
ORDER BY Padre, Etapa, Partida";

                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        resp.Partidas.Add(new PartidaAvanceDto
                        {
                            Wbs = Convert.ToInt32(r["WBS"]),
                            Codigo = tieneCodigo && r["Codigo"] != DBNull.Value ? r["Codigo"].ToString() : "",
                            Padre = r["Padre"]?.ToString() ?? "",
                            Etapa = r["Etapa"]?.ToString() ?? "",
                            Partida = r["Partida"]?.ToString() ?? "",
                            ImporteTotal = Convert.ToDouble(r["ImporteTotal"])
                        });
                    }
                }

                resp.Avances = CargarAvancesGuardados(conn, manzana, lote);
            }

            return Ok(resp);
        }

        /// <summary>
        /// POST /api/avances/partida · guarda (upsert) el avance de una partida en
        /// AvanceManualObra y recalcula el avance por concepto (AvanceManualConcepto).
        /// </summary>
        [HttpPost, Route("partida"), RequierePermiso("estimaciones.editar")]
        public IHttpActionResult GuardarPartida([FromBody] GuardarAvancePartidaRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Faltan manzana y/o lote.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaAvanceManualObra(conn);

                var columnas = ColumnasDe(conn, "AvanceManualObra");
                bool tieneImporteTotal = columnas.Contains("ImporteTotal");
                bool tieneMonto = columnas.Contains("MontoEjecutado") || columnas.Contains("ImporteEjecutado");

                string sql;
                if (tieneImporteTotal && tieneMonto)
                {
                    sql = @"IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                               UPDATE AvanceManualObra SET AvancePorcentaje=@avance, Concepto=@concepto, ImporteTotal=@importe, MontoEjecutado=@monto, FechaActualizacion=GETDATE()
                               WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                           ELSE
                               INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, ImporteTotal, MontoEjecutado, AvancePorcentaje)
                               VALUES (@m, @l, @proto, @wbs, @concepto, @importe, @monto, @avance)";
                }
                else if (tieneImporteTotal)
                {
                    sql = @"IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                               UPDATE AvanceManualObra SET AvancePorcentaje=@avance, Concepto=@concepto, ImporteTotal=@importe, FechaActualizacion=GETDATE()
                               WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                           ELSE
                               INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, ImporteTotal, AvancePorcentaje)
                               VALUES (@m, @l, @proto, @wbs, @concepto, @importe, @avance)";
                }
                else
                {
                    sql = @"IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                               UPDATE AvanceManualObra SET AvancePorcentaje=@avance, Concepto=@concepto, FechaActualizacion=GETDATE()
                               WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                           ELSE
                               INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, AvancePorcentaje)
                               VALUES (@m, @l, @proto, @wbs, @concepto, @avance)";
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@m", req.Manzana);
                    cmd.Parameters.AddWithValue("@l", req.Lote);
                    cmd.Parameters.AddWithValue("@proto", (object)req.Prototipo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@wbs", req.Wbs.ToString());
                    cmd.Parameters.AddWithValue("@avance", req.AvancePorcentaje);
                    cmd.Parameters.AddWithValue("@concepto", req.Concepto ?? "");

                    if (tieneImporteTotal)
                    {
                        var p = cmd.Parameters.Add("@importe", SqlDbType.Decimal);
                        p.Precision = 18; p.Scale = 2;
                        p.Value = Math.Round(req.ImporteTotal, 2);
                    }
                    if (tieneMonto)
                    {
                        var p = cmd.Parameters.Add("@monto", SqlDbType.Decimal);
                        p.Precision = 18; p.Scale = 2;
                        p.Value = Math.Round(req.ImporteEjecutado, 2);
                    }

                    cmd.ExecuteNonQuery();
                }

                ActualizarAvanceConceptos(conn, req.Manzana, req.Lote);
            }

            return Ok();
        }

        /// <summary>
        /// GET /api/avances/conceptos?prototipo= · conceptos de Estimacion(Concepto)
        /// con su importe total (columna de costo según prototipo, con fallback a TOTAL).
        /// </summary>
        [HttpGet, Route("conceptos")]
        public IHttpActionResult Conceptos(string prototipo)
        {
            var items = new List<ConceptoAvanceDto>();
            using (var conn = Db.Abrir())
            {
                var columnas = ColumnasDe(conn, "Estimacion(Concepto)");
                string columnaDeseada = (prototipo ?? "").ToUpperInvariant().Contains("CALANDRA")
                    ? "CostoCalandra" : "CostoTunera";
                string columnaACast = columnas.Contains(columnaDeseada) ? columnaDeseada
                    : columnas.Contains("TOTAL") ? "TOTAL" : null;
                if (columnaACast == null)
                    return BadRequest($"No se encontró columna de importe en la tabla Estimacion(Concepto). Buscada: {columnaDeseada} o TOTAL");

                bool tieneCodigo = columnas.Contains("Codigo");
                string sql = tieneCodigo
                    ? $@"
SELECT Codigo, Concepto, SUM(CAST([{columnaACast}] AS FLOAT)) AS Total
FROM [dbo].[Estimacion(Concepto)]
GROUP BY Codigo, Concepto
ORDER BY CASE WHEN TRY_CAST(Codigo AS INT) IS NOT NULL THEN TRY_CAST(Codigo AS INT) ELSE 999999 END, Codigo"
                    : $@"
SELECT ROW_NUMBER() OVER (ORDER BY Concepto) AS Codigo, Concepto, SUM(CAST([{columnaACast}] AS FLOAT)) AS Total
FROM [dbo].[Estimacion(Concepto)]
GROUP BY Concepto
ORDER BY Concepto";

                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        items.Add(new ConceptoAvanceDto
                        {
                            Codigo = r["Codigo"].ToString(),
                            Concepto = r["Concepto"].ToString(),
                            Total = Convert.ToDouble(r["Total"])
                        });
                }
            }
            return Ok(items);
        }

        /// <summary>
        /// GET /api/avances/avance-por-padre?manzana=&amp;lote=&amp;prototipo= · agrega
        /// por Padre el total (de PresupuestoObra) y el ejecutado (aplicando el avance
        /// de AvanceManualObra de la casa). El mapeo concepto→padres lo hace el cliente.
        /// </summary>
        [HttpGet, Route("avance-por-padre")]
        public IHttpActionResult AvancePorPadre(string manzana, string lote, string prototipo = null)
        {
            var resultado = new List<AvancePorPadreDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaAvanceManualObra(conn);

                var columnas = ColumnasDe(conn, "PresupuestoObra");
                string columnaCosto = (prototipo ?? "").ToUpperInvariant().Contains("CALANDRA")
                    ? "CostoCalandra" : "CostoTunera";
                if (!columnas.Contains(columnaCosto))
                    columnaCosto = columnas.Contains("TOTAL") ? "TOTAL"
                        : columnas.Contains("CostoTunera") ? "CostoTunera" : "CostoCalandra";

                // Partidas con WBS y Padre.
                var presRows = new List<Tuple<int, string, double>>();
                string sqlPres = $@"
SELECT ROW_NUMBER() OVER (ORDER BY Padre, Etapa, Partida) AS WBS, Padre,
       ISNULL(CAST([{columnaCosto}] AS FLOAT), 0) AS ImporteTotal
FROM PresupuestoObra
ORDER BY Padre, Etapa, Partida";
                using (var cmd = new SqlCommand(sqlPres, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        presRows.Add(Tuple.Create(
                            r["WBS"] != DBNull.Value ? Convert.ToInt32(r["WBS"]) : 0,
                            r["Padre"]?.ToString() ?? "",
                            r["ImporteTotal"] != DBNull.Value ? Convert.ToDouble(r["ImporteTotal"]) : 0.0));
                }

                // Avance por WBS de la casa.
                var avancePorWbs = new Dictionary<int, double>();
                using (var cmd = new SqlCommand(
                    "SELECT WBS, AvancePorcentaje FROM AvanceManualObra WHERE Manzana = @m AND Lote = @l", conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana ?? "");
                    cmd.Parameters.AddWithValue("@l", lote ?? "");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            if (int.TryParse(r["WBS"]?.ToString(), out int wbs) && wbs > 0)
                                avancePorWbs[wbs] = r["AvancePorcentaje"] != DBNull.Value
                                    ? Convert.ToDouble(r["AvancePorcentaje"]) : 0.0;
                        }
                    }
                }

                // Agregar por Padre (total y ejecutado).
                var padreDict = new Dictionary<string, Tuple<double, double>>(StringComparer.OrdinalIgnoreCase);
                foreach (var pr in presRows)
                {
                    string padre = pr.Item2?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(padre)) continue;

                    double importe = pr.Item3;
                    double ejecutado = avancePorWbs.TryGetValue(pr.Item1, out double avPerc)
                        ? importe * (avPerc / 100.0) : 0.0;

                    var prev = padreDict.TryGetValue(padre, out var t) ? t : Tuple.Create(0.0, 0.0);
                    padreDict[padre] = Tuple.Create(prev.Item1 + importe, prev.Item2 + ejecutado);
                }

                foreach (var kvp in padreDict)
                    resultado.Add(new AvancePorPadreDto { Padre = kvp.Key, Total = kvp.Value.Item1, Ejecutado = kvp.Value.Item2 });
            }
            return Ok(resultado);
        }

        /// <summary>
        /// POST /api/avances/concepto · upsert del avance de un concepto en
        /// AvanceManualConcepto (asegura la tabla).
        /// </summary>
        [HttpPost, Route("concepto"), RequierePermiso("estimaciones.editar")]
        public IHttpActionResult GuardarConcepto([FromBody] GuardarAvanceConceptoRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Faltan manzana y/o lote.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaAvanceManualConcepto(conn);

                const string sql = @"
IF EXISTS (SELECT 1 FROM AvanceManualConcepto WHERE Manzana=@m AND Lote=@l AND Codigo=@cod)
    UPDATE AvanceManualConcepto SET AvancePorcentaje=@avance, FechaActualizacion=GETDATE()
    WHERE Manzana=@m AND Lote=@l AND Codigo=@cod
ELSE
    INSERT INTO AvanceManualConcepto (Manzana, Lote, Prototipo, Codigo, Concepto, AvancePorcentaje)
    VALUES (@m, @l, @proto, @cod, @concepto, @avance)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@m", req.Manzana);
                    cmd.Parameters.AddWithValue("@l", req.Lote);
                    cmd.Parameters.AddWithValue("@proto", (object)req.Prototipo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cod", (object)req.Codigo ?? "");
                    cmd.Parameters.AddWithValue("@concepto", (object)req.Concepto ?? "");
                    cmd.Parameters.AddWithValue("@avance", req.AvancePorcentaje);
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        /// <summary>
        /// GET /api/avances/estimacion-jerarquica?manzana=&amp;lote=&amp;prototipo= ·
        /// partidas de PresupuestoObra (con costo resuelto por prototipo y sondeo de
        /// columnas) + avances guardados de AvanceManualObra (con m² y fecha). El
        /// cliente arma el árbol de conceptos.
        /// </summary>
        [HttpGet, Route("estimacion-jerarquica")]
        public IHttpActionResult EstimacionJerarquica(string manzana, string lote, string prototipo = null)
        {
            var resp = new EstimacionJerarquicaResponse();
            using (var conn = Db.Abrir())
            {
                EnsureTablaAvanceManualObra(conn);
                resp.Partidas = PartidasDinamicas(conn, prototipo);
                resp.Avances = AvancesPartidas(conn, manzana, lote);
            }
            return Ok(resp);
        }

        /// <summary>
        /// GET /api/avances/conceptos-selector · conceptos existentes (Codigo numérico +
        /// Nombre) para el selector de posición al agregar un concepto. Si la tabla no
        /// tiene columna Codigo, genera uno según el orden estándar de conceptos.
        /// </summary>
        [HttpGet, Route("conceptos-selector")]
        public IHttpActionResult ConceptosSelector()
        {
            var conceptos = new List<ConceptoExistenteDto>();
            using (var conn = Db.Abrir())
            {
                bool tieneCodigo = ColumnasDe(conn, "Estimacion(Concepto)").Contains("Codigo");

                string sql = tieneCodigo
                    ? @"
SELECT Codigo, Concepto
FROM (
    SELECT DISTINCT Codigo, Concepto,
        CASE WHEN TRY_CAST(Codigo AS INT) IS NOT NULL THEN TRY_CAST(Codigo AS INT) ELSE 999999 END AS CodigoNumerico
    FROM [dbo].[Estimacion(Concepto)]
    WHERE Codigo IS NOT NULL AND Concepto IS NOT NULL
) AS Conceptos
ORDER BY CodigoNumerico, Codigo"
                    : @"
WITH ConceptosOrdenados AS (
    SELECT DISTINCT Concepto,
        CASE Concepto
            WHEN 'Preliminares' THEN 1
            WHEN 'Cimentación' THEN 2 WHEN 'Cimentacion' THEN 2
            WHEN 'Estructura' THEN 3
            WHEN 'Ins. Hidraulica, Sanitaria y Gas LP' THEN 4 WHEN 'Inst. Hidraulica, Sanitaria y Gas LP' THEN 4
            WHEN 'Inst. Eléctrica' THEN 5 WHEN 'Inst. Electrica' THEN 5
            WHEN 'Albañilería' THEN 6 WHEN 'AlbanILERIA' THEN 6 WHEN 'Albañileria' THEN 6
            WHEN 'Acabados' THEN 7
            WHEN 'Herrería, Aluminio y Vidrio' THEN 8 WHEN 'Herreria, Aluminio y Vidrio' THEN 8
            WHEN 'Carpintería y Cerrajería' THEN 9 WHEN 'Carpinteria y Cerrajeria' THEN 9
            WHEN 'Muebles y Accesorios' THEN 10
            WHEN 'Inst especiales y Obra Exterior' THEN 11
            WHEN 'Urbanización' THEN 12 WHEN 'Urbanizacion' THEN 12
            ELSE 999
        END AS OrdenConcepto
    FROM [dbo].[Estimacion(Concepto)]
    WHERE Concepto IS NOT NULL
)
SELECT CAST(OrdenConcepto AS NVARCHAR(10)) AS Codigo, Concepto
FROM ConceptosOrdenados
ORDER BY OrdenConcepto";

                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (int.TryParse(r["Codigo"]?.ToString(), out int codigo))
                            conceptos.Add(new ConceptoExistenteDto
                            {
                                Codigo = codigo,
                                Nombre = r["Concepto"]?.ToString() ?? ""
                            });
                    }
                }
            }
            return Ok(conceptos);
        }

        /// <summary>
        /// POST /api/avances/concepto-nuevo · inserta un concepto nuevo (sus partidas en
        /// Estimacion(Concepto) y PresupuestoObra) opcionalmente renumerando los conceptos
        /// posteriores (Codigo &gt;= @codigo), todo en una transacción.
        /// </summary>
        [HttpPost, Route("concepto-nuevo"), RequierePermiso("estimaciones.editar")]
        public IHttpActionResult ConceptoNuevo([FromBody] ConceptoNuevoRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Nombre))
                return BadRequest("Falta el nombre del concepto.");
            if (req.Partidas == null || req.Partidas.Count == 0)
                return BadRequest("El concepto no tiene partidas.");

            using (var conn = Db.Abrir())
            {
                bool tieneCodigoEstimacion = ColumnasDe(conn, "Estimacion(Concepto)").Contains("Codigo");
                bool tieneCodigoPresupuesto = ColumnasDe(conn, "PresupuestoObra").Contains("Codigo");

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        if (req.Renumerar && tieneCodigoEstimacion)
                        {
                            using (var cmd = new SqlCommand(@"
UPDATE [dbo].[Estimacion(Concepto)]
SET Codigo = CAST((TRY_CAST(Codigo AS INT) + 1) AS NVARCHAR(10))
WHERE TRY_CAST(Codigo AS INT) >= @codigoDesde AND TRY_CAST(Codigo AS INT) IS NOT NULL", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@codigoDesde", req.Codigo);
                                cmd.ExecuteNonQuery();
                            }
                            if (tieneCodigoPresupuesto)
                            {
                                using (var cmd = new SqlCommand(@"
UPDATE PresupuestoObra
SET Codigo = CAST((TRY_CAST(Codigo AS INT) + 1) AS NVARCHAR(10))
WHERE TRY_CAST(Codigo AS INT) >= @codigoDesde AND TRY_CAST(Codigo AS INT) IS NOT NULL", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@codigoDesde", req.Codigo);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        string sqlEstimacion = tieneCodigoEstimacion
                            ? @"INSERT INTO [dbo].[Estimacion(Concepto)] (Codigo, Concepto, Padre, Etapa, Partida, TOTAL, CostoTunera, CostoCalandra)
                                VALUES (@codigo, @concepto, @padre, @etapa, @partida, @total, @costoTunera, @costoCalandra)"
                            : @"INSERT INTO [dbo].[Estimacion(Concepto)] (Concepto, Padre, Etapa, Partida, TOTAL, CostoTunera, CostoCalandra)
                                VALUES (@concepto, @padre, @etapa, @partida, @total, @costoTunera, @costoCalandra)";

                        string sqlPresupuesto = tieneCodigoPresupuesto
                            ? @"INSERT INTO PresupuestoObra (Codigo, Padre, Etapa, Partida, CostoTunera, CostoCalandra)
                                VALUES (@codigo, @padre, @etapa, @partida, @costoTunera, @costoCalandra)"
                            : @"INSERT INTO PresupuestoObra (Padre, Etapa, Partida, CostoTunera, CostoCalandra)
                                VALUES (@padre, @etapa, @partida, @costoTunera, @costoCalandra)";

                        foreach (var p in req.Partidas)
                        {
                            using (var cmd = new SqlCommand(sqlEstimacion, conn, tx))
                            {
                                if (tieneCodigoEstimacion) cmd.Parameters.AddWithValue("@codigo", req.Codigo.ToString());
                                cmd.Parameters.AddWithValue("@concepto", req.Nombre);
                                cmd.Parameters.AddWithValue("@padre", req.Nombre);
                                cmd.Parameters.AddWithValue("@etapa", (object)p.Etapa ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@partida", (object)p.Partida ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@total", p.CostoTunera); // Usar Tunera como TOTAL (igual que el cliente)
                                cmd.Parameters.AddWithValue("@costoTunera", p.CostoTunera);
                                cmd.Parameters.AddWithValue("@costoCalandra", p.CostoCalandra);
                                cmd.ExecuteNonQuery();
                            }
                            using (var cmd = new SqlCommand(sqlPresupuesto, conn, tx))
                            {
                                if (tieneCodigoPresupuesto) cmd.Parameters.AddWithValue("@codigo", req.Codigo.ToString());
                                cmd.Parameters.AddWithValue("@padre", req.Nombre);
                                cmd.Parameters.AddWithValue("@etapa", (object)p.Etapa ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@partida", (object)p.Partida ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@costoTunera", p.CostoTunera);
                                cmd.Parameters.AddWithValue("@costoCalandra", p.CostoCalandra);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
            return Ok();
        }

        /// <summary>
        /// POST /api/avances/partida-estimacion · upsert del avance de una partida en
        /// AvanceManualObra incluyendo MetrosCuadrados y FechaFinalizacion (sondea ambas
        /// columnas). Equivale a GuardarAvancePartidaEnBD del form de estimación.
        /// </summary>
        [HttpPost, Route("partida-estimacion"), RequierePermiso("estimaciones.editar")]
        public IHttpActionResult GuardarPartidaEstimacion([FromBody] GuardarAvancePartidaEstimacionRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Faltan manzana y/o lote.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaAvanceManualObra(conn);
                var cols = ColumnasDe(conn, "AvanceManualObra");

                UpsertAvance(conn, null, cols.Contains("MetrosCuadrados"), cols.Contains("FechaFinalizacion"),
                    req.Manzana, req.Lote, req.Prototipo, req.Wbs.ToString(),
                    req.AvancePorcentaje, req.MontoEjecutado, req.MetrosCuadrados, req.FechaFinalizacion, null);
            }
            return Ok();
        }

        /// <summary>
        /// POST /api/avances/resetear-partida · pone a 0 el avance/monto/m² de una partida
        /// o concepto (FechaFinalizacion a NULL). Equivale a "Resetear Progreso".
        /// </summary>
        [HttpPost, Route("resetear-partida"), RequierePermiso("estimaciones.editar")]
        public IHttpActionResult ResetearPartida([FromBody] ResetearPartidaRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Faltan manzana y/o lote.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaAvanceManualObra(conn);
                var cols = ColumnasDe(conn, "AvanceManualObra");

                var sets = new List<string> { "AvancePorcentaje = 0", "MontoEjecutado = 0" };
                if (cols.Contains("MetrosCuadrados")) sets.Add("MetrosCuadrados = 0");
                if (cols.Contains("FechaFinalizacion")) sets.Add("FechaFinalizacion = NULL");

                using (var cmd = new SqlCommand(
                    $"UPDATE AvanceManualObra SET {string.Join(", ", sets)} WHERE Manzana = @m AND Lote = @l AND WBS = @wbs", conn))
                {
                    cmd.Parameters.AddWithValue("@m", req.Manzana);
                    cmd.Parameters.AddWithValue("@l", req.Lote);
                    cmd.Parameters.AddWithValue("@wbs", req.Wbs ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        /// <summary>
        /// POST /api/avances/marcar-completadas · marca partidas y/o conceptos al 100% en
        /// AvanceManualObra, en una transacción. Sirve para "Guardar y exportar"
        /// (ActualizarAvancesA100PorCiento) y para "Terminar sin estimación" (admin). Los
        /// montos y m² vienen ya calculados por el cliente.
        /// </summary>
        [HttpPost, Route("marcar-completadas"), RequierePermiso("estimaciones.editar")]
        public IHttpActionResult MarcarCompletadas([FromBody] MarcarCompletadasRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Faltan manzana y/o lote.");

            DateTime fecha = req.FechaFinalizacion ?? DateTime.Now;

            using (var conn = Db.Abrir())
            {
                EnsureTablaAvanceManualObra(conn);
                var cols = ColumnasDe(conn, "AvanceManualObra");
                bool tieneM2 = cols.Contains("MetrosCuadrados");
                bool tieneFecha = cols.Contains("FechaFinalizacion");

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        if (req.Partidas != null)
                            foreach (var p in req.Partidas)
                                UpsertAvance(conn, tx, tieneM2, tieneFecha, req.Manzana, req.Lote, req.Prototipo,
                                    p.Wbs.ToString(), 100.0, p.Monto, p.MetrosCuadrados, fecha, null);

                        if (req.Conceptos != null)
                            foreach (var c in req.Conceptos)
                                UpsertAvance(conn, tx, tieneM2, tieneFecha, req.Manzana, req.Lote, req.Prototipo,
                                    c.Wbs.ToString(), 100.0, c.Monto, null, fecha, c.Nombre ?? "");

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
            return Ok();
        }

        // ---- helpers ----

        /// <summary>
        /// Upsert genérico en AvanceManualObra. Sondea qué columnas opcionales escribir:
        /// MetrosCuadrados (solo si tieneM2 y metros != null), FechaFinalizacion (si tieneFecha)
        /// y Concepto (si concepto != null, para registrar conceptos con WBS negativo).
        /// </summary>
        private static void UpsertAvance(SqlConnection conn, SqlTransaction tx, bool tieneM2, bool tieneFecha,
            string manzana, string lote, string prototipo, string wbs, double avance, double monto,
            double? metros, DateTime? fechaFin, string concepto)
        {
            bool escribirM2 = tieneM2 && metros.HasValue;

            var setList = new List<string> { "AvancePorcentaje=@avance", "MontoEjecutado=@monto", "FechaActualizacion=GETDATE()" };
            var insCols = new List<string> { "Manzana", "Lote", "Prototipo", "WBS", "AvancePorcentaje", "MontoEjecutado" };
            var insVals = new List<string> { "@m", "@l", "@proto", "@wbs", "@avance", "@monto" };

            if (concepto != null) { setList.Add("Concepto=@concepto"); insCols.Add("Concepto"); insVals.Add("@concepto"); }
            if (escribirM2) { setList.Add("MetrosCuadrados=@metros"); insCols.Add("MetrosCuadrados"); insVals.Add("@metros"); }
            if (tieneFecha) { setList.Add("FechaFinalizacion=@fechaFin"); insCols.Add("FechaFinalizacion"); insVals.Add("@fechaFin"); }

            string sql = $@"
IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
    UPDATE AvanceManualObra SET {string.Join(", ", setList)} WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
ELSE
    INSERT INTO AvanceManualObra ({string.Join(", ", insCols)}) VALUES ({string.Join(", ", insVals)})";

            using (var cmd = tx == null ? new SqlCommand(sql, conn) : new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.Parameters.AddWithValue("@proto", string.IsNullOrEmpty(prototipo) ? (object)DBNull.Value : prototipo);
                cmd.Parameters.AddWithValue("@wbs", wbs);
                cmd.Parameters.AddWithValue("@avance", avance);
                cmd.Parameters.AddWithValue("@monto", monto);
                if (concepto != null) cmd.Parameters.AddWithValue("@concepto", (object)concepto ?? DBNull.Value);
                if (escribirM2) cmd.Parameters.AddWithValue("@metros", metros.Value);
                if (tieneFecha) cmd.Parameters.AddWithValue("@fechaFin", fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Porta CargarTodasLasPartidas: lee PresupuestoObra con sondeo de columnas.</summary>
        private static List<PartidaDinamicaDto> PartidasDinamicas(SqlConnection conn, string prototipo)
        {
            var partidas = new List<PartidaDinamicaDto>();
            var cols = ColumnasDe(conn, "PresupuestoObra");

            if (!cols.Contains("WBS_Correcto"))
                return partidas; // sin WBS_Correcto no hay nada que armar

            bool tieneCodigo = cols.Contains("Codigo");
            bool tienePadre = cols.Contains("Padre");
            bool tieneEtapa = cols.Contains("Etapa");
            bool tienePartida = cols.Contains("Partida");
            bool tieneCostoTunera = cols.Contains("CostoTunera");
            bool tieneCostoCalandra = cols.Contains("CostoCalandra");
            bool tieneEsDinamica = cols.Contains("EsDinamica");
            bool tieneValorM2Tunera = cols.Contains("ValorM2Tunera");
            bool tieneValorM2Calandra = cols.Contains("ValorM2Calandra");
            bool tieneLimiteM2 = cols.Contains("LimiteM2");
            bool tieneLimiteM2Tunera = cols.Contains("LimiteM2Tunera");
            bool tieneLimiteM2Calandra = cols.Contains("LimiteM2Calandra");
            bool tienePrototipos = cols.Contains("PrototiposAplicables");

            var columnas = new List<string> { "WBS_Correcto AS WBS" };
            if (tieneCodigo) columnas.Add("Codigo");
            if (tienePadre) columnas.Add("Padre");
            if (tieneEtapa) columnas.Add("Etapa");
            if (tienePartida) columnas.Add("Partida");
            if (tieneCostoTunera) columnas.Add("ISNULL(CostoTunera, 0) AS CostoTunera");
            if (tieneCostoCalandra) columnas.Add("ISNULL(CostoCalandra, 0) AS CostoCalandra");
            if (tieneEsDinamica) columnas.Add("ISNULL(EsDinamica, 0) AS EsDinamica");
            if (tieneValorM2Tunera) columnas.Add("ISNULL(ValorM2Tunera, 0) AS ValorM2Tunera");
            if (tieneValorM2Calandra) columnas.Add("ISNULL(ValorM2Calandra, 0) AS ValorM2Calandra");
            if (tieneLimiteM2) columnas.Add("ISNULL(LimiteM2, 0) AS LimiteM2");
            if (tieneLimiteM2Tunera) columnas.Add("ISNULL(LimiteM2Tunera, 0) AS LimiteM2Tunera");
            if (tieneLimiteM2Calandra) columnas.Add("ISNULL(LimiteM2Calandra, 0) AS LimiteM2Calandra");
            if (tienePrototipos) columnas.Add("PrototiposAplicables");

            string sql = $@"
SELECT {string.Join(", ", columnas)}
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
ORDER BY {(tieneCodigo ? "Codigo, " : "")}WBS_Correcto";

            bool usarCalandra = !string.IsNullOrEmpty(prototipo)
                && !prototipo.ToUpperInvariant().Contains("TUNERA") && tieneCostoCalandra;

            using (var cmd = new SqlCommand(sql, conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    double costo = tieneCostoTunera ? Convert.ToDouble(r["CostoTunera"]) : 0;
                    if (usarCalandra) costo = Convert.ToDouble(r["CostoCalandra"]);

                    partidas.Add(new PartidaDinamicaDto
                    {
                        Wbs = Convert.ToInt32(r["WBS"]),
                        Codigo = tieneCodigo ? (r["Codigo"]?.ToString() ?? "") : "",
                        Padre = tienePadre ? (r["Padre"]?.ToString() ?? "") : "",
                        Etapa = tieneEtapa ? (r["Etapa"]?.ToString() ?? "") : "",
                        Partida = tienePartida ? (r["Partida"]?.ToString() ?? "") : "",
                        Costo = costo,
                        EsDinamica = tieneEsDinamica && Convert.ToBoolean(r["EsDinamica"]),
                        ValorM2Tunera = tieneValorM2Tunera ? Convert.ToDouble(r["ValorM2Tunera"]) : 0,
                        ValorM2Calandra = tieneValorM2Calandra ? Convert.ToDouble(r["ValorM2Calandra"]) : 0,
                        LimiteM2 = tieneLimiteM2 ? Convert.ToDouble(r["LimiteM2"]) : 0,
                        LimiteM2Tunera = tieneLimiteM2Tunera ? Convert.ToDouble(r["LimiteM2Tunera"]) : 0,
                        LimiteM2Calandra = tieneLimiteM2Calandra ? Convert.ToDouble(r["LimiteM2Calandra"]) : 0,
                        PrototiposAplicables = tienePrototipos ? r["PrototiposAplicables"]?.ToString() : null
                    });
                }
            }
            return partidas;
        }

        /// <summary>Porta CargarAvancesPartidas: lee AvanceManualObra con m² y fecha (sondeadas).</summary>
        private static List<AvancePartidaDto> AvancesPartidas(SqlConnection conn, string manzana, string lote)
        {
            var lista = new List<AvancePartidaDto>();
            var cols = ColumnasDe(conn, "AvanceManualObra");
            bool tieneM2 = cols.Contains("MetrosCuadrados");
            bool tieneFecha = cols.Contains("FechaFinalizacion");

            string sql = "SELECT WBS, AvancePorcentaje, MontoEjecutado";
            if (tieneFecha) sql += ", FechaFinalizacion";
            if (tieneM2) sql += ", MetrosCuadrados";
            sql += " FROM AvanceManualObra WHERE Manzana = @manzana AND Lote = @lote";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@manzana", manzana ?? "");
                cmd.Parameters.AddWithValue("@lote", lote ?? "");
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (!int.TryParse(r["WBS"]?.ToString(), out int wbs)) continue;

                        lista.Add(new AvancePartidaDto
                        {
                            Wbs = wbs,
                            AvancePorcentaje = r["AvancePorcentaje"] != DBNull.Value ? Convert.ToDouble(r["AvancePorcentaje"]) : 0,
                            MontoEjecutado = r["MontoEjecutado"] != DBNull.Value ? Convert.ToDouble(r["MontoEjecutado"]) : 0,
                            FechaFinalizacion = tieneFecha && r["FechaFinalizacion"] != DBNull.Value
                                ? (DateTime?)Convert.ToDateTime(r["FechaFinalizacion"]) : null,
                            MetrosCuadrados = tieneM2 && r["MetrosCuadrados"] != DBNull.Value ? Convert.ToDouble(r["MetrosCuadrados"]) : 0
                        });
                    }
                }
            }
            return lista;
        }

        private static List<AvanceGuardadoDto> CargarAvancesGuardados(SqlConnection conn, string manzana, string lote)
        {
            var lista = new List<AvanceGuardadoDto>();
            var columnas = ColumnasDe(conn, "AvanceManualObra");
            bool tieneMonto = columnas.Contains("MontoEjecutado") || columnas.Contains("ImporteEjecutado");

            string sql = tieneMonto
                ? "SELECT WBS, AvancePorcentaje, MontoEjecutado FROM AvanceManualObra WHERE Manzana = @m AND Lote = @l"
                : "SELECT WBS, AvancePorcentaje FROM AvanceManualObra WHERE Manzana = @m AND Lote = @l";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana ?? "");
                cmd.Parameters.AddWithValue("@l", lote ?? "");
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (!int.TryParse(r["WBS"]?.ToString(), out int wbs)) continue;
                        double? monto = null;
                        if (tieneMonto && r["MontoEjecutado"] != DBNull.Value)
                            monto = Convert.ToDouble(r["MontoEjecutado"]);

                        lista.Add(new AvanceGuardadoDto
                        {
                            Wbs = wbs,
                            AvancePorcentaje = Convert.ToDouble(r["AvancePorcentaje"]),
                            MontoEjecutado = monto
                        });
                    }
                }
            }
            return lista;
        }

        private static void ActualizarAvanceConceptos(SqlConnection conn, string manzana, string lote)
        {
            try
            {
                const string sql = @"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualConcepto')
                    BEGIN
                        UPDATE c SET
                            c.AvancePorcentaje = (
                                SELECT AVG(p.AvancePorcentaje)
                                FROM AvanceManualObra p
                                WHERE p.Manzana = @m AND p.Lote = @l AND p.Concepto = c.Concepto
                            ),
                            c.FechaActualizacion = GETDATE()
                        FROM AvanceManualConcepto c
                        WHERE c.Manzana = @m AND c.Lote = @l
                    END";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana ?? "");
                    cmd.Parameters.AddWithValue("@l", lote ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Igual que el cliente original: no impide guardar la partida.
            }
        }

        private static void EnsureTablaAvanceManualObra(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualObra')
BEGIN
    CREATE TABLE AvanceManualObra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(10), Lote NVARCHAR(10),
        Prototipo NVARCHAR(50), WBS NVARCHAR(50),
        Concepto NVARCHAR(200), ImporteTotal FLOAT,
        AvancePorcentaje FLOAT,
        FechaActualizacion DATETIME DEFAULT GETDATE()
    );
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private static void EnsureTablaAvanceManualConcepto(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualConcepto')
BEGIN
    CREATE TABLE AvanceManualConcepto (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(10), Lote NVARCHAR(10),
        Prototipo NVARCHAR(50), Codigo NVARCHAR(50),
        Concepto NVARCHAR(200), AvancePorcentaje FLOAT,
        FechaActualizacion DATETIME DEFAULT GETDATE()
    );
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private static HashSet<string> ColumnasDe(SqlConnection conn, string tabla)
        {
            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t", conn))
            {
                cmd.Parameters.AddWithValue("@t", tabla);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        columnas.Add(r.GetString(0));
                }
            }
            return columnas;
        }
    }
}
