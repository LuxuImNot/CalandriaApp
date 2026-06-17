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

        // ---- helpers ----

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
