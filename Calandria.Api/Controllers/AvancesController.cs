using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;
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
    [RoutePrefix("api/avances")]
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
        [HttpPost, Route("partida")]
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
        [HttpPost, Route("concepto")]
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

        // ---- helpers ----

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
