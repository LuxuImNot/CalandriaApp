using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Nómina · asignación de importe de una tarea de Mano de Obra
    /// entre los miembros de una cuadrilla. Reproduce FormAsignarNomina:
    /// cuadrillas y miembros desde MiembrosCuadrilla; la asignación se guarda en
    /// NominaTareasAsignada (DELETE + INSERT por tarea).
    /// </summary>
    [RoutePrefix("api/nomina")]
    public class NominaController : ApiController
    {
        /// <summary>GET /api/nomina/cuadrillas → códigos de cuadrilla.</summary>
        [HttpGet, Route("cuadrillas")]
        public IHttpActionResult Cuadrillas()
        {
            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT DISTINCT CodigoCuadrilla FROM MiembrosCuadrilla ORDER BY CodigoCuadrilla", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    lista.Add(reader.GetString(0));
            }
            return Ok(lista);
        }

        /// <summary>GET /api/nomina/cuadrillas/{codigo}/miembros</summary>
        [HttpGet, Route("cuadrillas/{codigo}/miembros")]
        public IHttpActionResult Miembros(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest("Falta el código de cuadrilla.");

            var lista = new List<MiembroCuadrillaDto>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(@"
                SELECT IdTrabajador, Nombre, Rol, EsJefe, Telefono
                FROM MiembrosCuadrilla
                WHERE CodigoCuadrilla = @codigo
                ORDER BY EsJefe DESC, Nombre", conn))
            {
                cmd.Parameters.AddWithValue("@codigo", codigo);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new MiembroCuadrillaDto
                        {
                            IdTrabajador = reader["IdTrabajador"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(reader["IdTrabajador"]),
                            Nombre = reader["Nombre"]?.ToString() ?? "",
                            Rol = reader["Rol"]?.ToString() ?? "",
                            EsJefe = reader["EsJefe"] != DBNull.Value && Convert.ToBoolean(reader["EsJefe"]),
                            Telefono = reader["Telefono"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>
        /// GET /api/nomina/asignacion?manzana=&amp;lote=&amp;ruta=&amp;nodoId=
        /// Devuelve la asignación previa de la tarea (o vacía si no hay).
        /// </summary>
        [HttpGet, Route("asignacion")]
        public IHttpActionResult Asignacion(string manzana, string lote, string ruta, int nodoId)
        {
            var dto = new AsignacionNominaDto();
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                using (var cmd = new SqlCommand(@"
                    SELECT CodigoCuadrilla, IdTrabajador, NombreTrabajador, Monto
                    FROM NominaTareasAsignada
                    WHERE Manzana = @m AND Lote = @l AND Ruta = @r AND NodoID = @nodo", conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana ?? "");
                    cmd.Parameters.AddWithValue("@l", lote ?? "");
                    cmd.Parameters.AddWithValue("@r", ruta ?? "");
                    cmd.Parameters.AddWithValue("@nodo", nodoId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dto.CodigoCuadrilla = reader["CodigoCuadrilla"]?.ToString();
                            dto.Montos.Add(new MontoTrabajadorDto
                            {
                                IdTrabajador = reader["IdTrabajador"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(reader["IdTrabajador"]),
                                NombreTrabajador = reader["NombreTrabajador"]?.ToString() ?? "",
                                Monto = reader["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Monto"])
                            });
                        }
                    }
                }
            }
            return Ok(dto);
        }

        /// <summary>
        /// POST /api/nomina/asignacion · reemplaza la asignación de la tarea
        /// (DELETE + INSERT en una transacción), igual que FormAsignarNomina.
        /// </summary>
        [HttpPost, Route("asignacion")]
        public IHttpActionResult GuardarAsignacion([FromBody] GuardarAsignacionRequest req)
        {
            if (req == null)
                return BadRequest("Cuerpo vacío.");
            if (string.IsNullOrWhiteSpace(req.CodigoCuadrilla))
                return BadRequest("Falta la cuadrilla.");
            if (req.Lineas == null || req.Lineas.Count == 0)
                return BadRequest("No hay miembros que asignar.");

            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                using (var tx = conn.BeginTransaction())
                {
                    using (var cmdDel = new SqlCommand(@"
                        DELETE FROM NominaTareasAsignada
                        WHERE Manzana = @m AND Lote = @l AND Ruta = @r AND NodoID = @nodo", conn, tx))
                    {
                        cmdDel.Parameters.AddWithValue("@m", req.Manzana ?? "");
                        cmdDel.Parameters.AddWithValue("@l", req.Lote ?? "");
                        cmdDel.Parameters.AddWithValue("@r", req.Ruta ?? "");
                        cmdDel.Parameters.AddWithValue("@nodo", req.NodoId);
                        cmdDel.ExecuteNonQuery();
                    }

                    foreach (var linea in req.Lineas)
                    {
                        using (var cmdIns = new SqlCommand(@"
                            INSERT INTO NominaTareasAsignada
                            (Manzana, Lote, Ruta, NodoID, NombreTarea, CodigoCuadrilla,
                             IdTrabajador, NombreTrabajador, Rol, EsJefe, Monto, TotalAsignado, FechaActualizacion)
                            VALUES
                            (@m, @l, @r, @nodo, @tarea, @codigo,
                             @idTrab, @nombre, @rol, @esJefe, @monto, @total, GETDATE())", conn, tx))
                        {
                            cmdIns.Parameters.AddWithValue("@m", req.Manzana ?? "");
                            cmdIns.Parameters.AddWithValue("@l", req.Lote ?? "");
                            cmdIns.Parameters.AddWithValue("@r", req.Ruta ?? "");
                            cmdIns.Parameters.AddWithValue("@nodo", req.NodoId);
                            cmdIns.Parameters.AddWithValue("@tarea", req.NombreTarea ?? "");
                            cmdIns.Parameters.AddWithValue("@codigo", req.CodigoCuadrilla);
                            cmdIns.Parameters.AddWithValue("@idTrab", (object)linea.IdTrabajador ?? DBNull.Value);
                            cmdIns.Parameters.AddWithValue("@nombre", linea.Nombre ?? "");
                            cmdIns.Parameters.AddWithValue("@rol", linea.Rol ?? "");
                            cmdIns.Parameters.AddWithValue("@esJefe", linea.EsJefe);
                            cmdIns.Parameters.AddWithValue("@monto", linea.Monto);
                            cmdIns.Parameters.AddWithValue("@total", req.TotalDistribuir);
                            cmdIns.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
            return Ok();
        }

        /// <summary>
        /// GET /api/nomina/reporte?desde=&amp;hasta= · agregado por trabajador.
        /// Reproduce FormReporteNomina pero agrupa siempre en el servidor (sin
        /// depender de STRING_AGG / versión de SQL Server).
        /// </summary>
        [HttpGet, Route("reporte")]
        public IHttpActionResult Reporte(DateTime desde, DateTime hasta)
        {
            var raw = new List<(int? id, string nombre, string rol, decimal monto, string cuadrilla, string clave)>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaRecibos(conn);

                using (var cmd = new SqlCommand(@"
                    SELECT r.IdTrabajador, r.NombreTrabajador, r.Rol, r.Monto,
                           r.CodigoCuadrilla, t.ClaveTrabajador
                    FROM RecibosNomina r
                    LEFT JOIN TRABAJADORES t ON t.IdTrabajador = r.IdTrabajador
                    WHERE r.FechaRecibo BETWEEN @d AND @h
                       OR (r.PeriodoDesde IS NOT NULL AND r.PeriodoHasta IS NOT NULL
                           AND r.PeriodoDesde <= @h AND r.PeriodoHasta >= @d)", conn))
                {
                    cmd.Parameters.AddWithValue("@d", desde.Date);
                    cmd.Parameters.AddWithValue("@h", hasta.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            raw.Add((
                                reader["IdTrabajador"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IdTrabajador"]),
                                reader["NombreTrabajador"]?.ToString() ?? "",
                                reader["Rol"] == DBNull.Value ? "" : reader["Rol"].ToString(),
                                reader["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Monto"]),
                                reader["CodigoCuadrilla"] == DBNull.Value ? "" : reader["CodigoCuadrilla"].ToString(),
                                reader["ClaveTrabajador"] == DBNull.Value ? "" : reader["ClaveTrabajador"].ToString()));
                        }
                    }
                }
            }

            var resultado = raw
                .GroupBy(r => new { r.id, r.nombre })
                .Select(g => new NominaReporteDto
                {
                    IdTrabajador = g.Key.id,
                    Nombre = g.Key.nombre,
                    Rol = g.Select(x => x.rol).FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? "",
                    Monto = g.Sum(x => x.monto),
                    NumRecibos = g.Count(),
                    Cuadrillas = string.Join(", ",
                        g.Select(x => x.cuadrilla).Where(s => !string.IsNullOrEmpty(s)).Distinct()),
                    Clave = g.Select(x => x.clave).FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? ""
                })
                .OrderBy(x => x.Nombre)
                .ToList();

            return Ok(resultado);
        }

        /// <summary>
        /// GET /api/nomina/recibos?desde=&amp;hasta= · cabeceras de recibos (sin PDF).
        /// </summary>
        [HttpGet, Route("recibos")]
        public IHttpActionResult Recibos(DateTime desde, DateTime hasta)
        {
            var lista = new List<ReciboNominaDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaRecibos(conn);

                using (var cmd = new SqlCommand(@"
                    SELECT Id, IdTrabajador, NombreTrabajador, Rol, CodigoCuadrilla, Concepto, Monto,
                           FechaRecibo, PeriodoDesde, PeriodoHasta, TotalCuadrilla,
                           CASE WHEN Pdf IS NULL THEN 0 ELSE 1 END AS TienePdf
                    FROM RecibosNomina
                    WHERE FechaRecibo BETWEEN @d AND @h
                       OR (PeriodoDesde IS NOT NULL AND PeriodoHasta IS NOT NULL
                           AND PeriodoDesde <= @h AND PeriodoHasta >= @d)
                    ORDER BY FechaRecibo DESC, NombreTrabajador", conn))
                {
                    cmd.Parameters.AddWithValue("@d", desde.Date);
                    cmd.Parameters.AddWithValue("@h", hasta.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ReciboNominaDto
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IdTrabajador = reader["IdTrabajador"] == DBNull.Value
                                    ? (int?)null : Convert.ToInt32(reader["IdTrabajador"]),
                                NombreTrabajador = reader["NombreTrabajador"]?.ToString() ?? "",
                                Rol = reader["Rol"] == DBNull.Value ? "" : reader["Rol"].ToString(),
                                CodigoCuadrilla = reader["CodigoCuadrilla"] == DBNull.Value
                                    ? "" : reader["CodigoCuadrilla"].ToString(),
                                Concepto = reader["Concepto"] == DBNull.Value ? "" : reader["Concepto"].ToString(),
                                Monto = reader["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Monto"]),
                                FechaRecibo = Convert.ToDateTime(reader["FechaRecibo"]),
                                PeriodoDesde = reader["PeriodoDesde"] == DBNull.Value
                                    ? (DateTime?)null : Convert.ToDateTime(reader["PeriodoDesde"]),
                                PeriodoHasta = reader["PeriodoHasta"] == DBNull.Value
                                    ? (DateTime?)null : Convert.ToDateTime(reader["PeriodoHasta"]),
                                TotalCuadrilla = reader["TotalCuadrilla"] == DBNull.Value
                                    ? 0m : Convert.ToDecimal(reader["TotalCuadrilla"]),
                                TienePdf = Convert.ToInt32(reader["TienePdf"]) == 1
                            });
                        }
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/nomina/recibos/{id}/pdf · binario del recibo.</summary>
        [HttpGet, Route("recibos/{id:int}/pdf")]
        public HttpResponseMessage Pdf(int id)
        {
            byte[] bytes = null;
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand("SELECT Pdf FROM RecibosNomina WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value)
                    bytes = (byte[])obj;
            }

            if (bytes == null || bytes.Length == 0)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return resp;
        }

        /// <summary>
        /// POST /api/nomina/distribucion · persiste los recibos de la cuadrilla y
        /// marca sus destajos como nómina distribuida, en una transacción.
        /// Reproduce FormDistribucionNomina (GuardarRecibo + MarcarNominaDistribuida).
        /// </summary>
        [HttpPost, Route("distribucion")]
        public IHttpActionResult Distribucion([FromBody] DistribucionNominaRequest req)
        {
            if (req == null)
                return BadRequest("Cuerpo vacío.");
            if (req.Recibos == null || req.Recibos.Count == 0)
                return BadRequest("No hay recibos que guardar.");

            string usuario = User?.Identity?.Name ?? "api";

            using (var conn = Db.Abrir())
            {
                EnsureTablaRecibos(conn);
                EnsureColumnasNominaDistribuida(conn);

                using (var tx = conn.BeginTransaction())
                {
                    foreach (var r in req.Recibos)
                    {
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO RecibosNomina
                            (IdTrabajador, NombreTrabajador, Rol, CodigoCuadrilla, Concepto, Monto,
                             FechaRecibo, PeriodoDesde, PeriodoHasta, TotalCuadrilla, Pdf, Usuario)
                            VALUES
                            (@id, @nombre, @rol, @codigo, @concepto, @monto,
                             @fecha, @desde, @hasta, @total, @pdf, @usuario)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", (object)r.IdTrabajador ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@nombre", r.NombreTrabajador ?? "");
                            cmd.Parameters.AddWithValue("@rol", (object)r.Rol ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@codigo", req.CodigoCuadrilla ?? "");
                            cmd.Parameters.AddWithValue("@concepto", r.Concepto ?? "");
                            cmd.Parameters.AddWithValue("@monto", r.Monto);
                            cmd.Parameters.AddWithValue("@fecha", DateTime.Today);
                            cmd.Parameters.AddWithValue("@desde", req.Desde.Date);
                            cmd.Parameters.AddWithValue("@hasta", req.Hasta.Date);
                            cmd.Parameters.AddWithValue("@total", req.TotalCuadrilla);

                            byte[] pdf = string.IsNullOrEmpty(r.PdfBase64)
                                ? null : Convert.FromBase64String(r.PdfBase64);
                            var pPdf = cmd.Parameters.Add("@pdf", SqlDbType.VarBinary, -1);
                            pPdf.Value = (pdf != null && pdf.Length > 0) ? (object)pdf : DBNull.Value;

                            cmd.Parameters.AddWithValue("@usuario", usuario);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (req.Destajos != null)
                    {
                        foreach (var d in req.Destajos)
                        {
                            if (d.NodoId <= 0) continue;
                            using (var cmd = new SqlCommand(@"
                                UPDATE ActivacionTareasRuta
                                   SET NominaDistribuida = 1,
                                       FechaDistribucionNomina = GETDATE()
                                 WHERE Manzana = @m AND Lote = @l AND Ruta = @r AND NodoID = @nodo", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@m", d.Manzana ?? "");
                                cmd.Parameters.AddWithValue("@l", d.Lote ?? "");
                                cmd.Parameters.AddWithValue("@r", d.Ruta ?? "");
                                cmd.Parameters.AddWithValue("@nodo", d.NodoId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    tx.Commit();
                }
            }
            return Ok();
        }

        /// <summary>
        /// Crea NominaTareasAsignada si no existe (idempotente). Antes lo hacía el
        /// cliente en cada apertura del formulario; ahora vive en el servidor.
        /// </summary>
        private static void EnsureTabla(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NominaTareasAsignada')
BEGIN
    CREATE TABLE NominaTareasAsignada (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(10),
        Lote NVARCHAR(10),
        Ruta NVARCHAR(50),
        NodoID INT,
        NombreTarea NVARCHAR(300),
        CodigoCuadrilla NVARCHAR(20),
        IdTrabajador INT NULL,
        NombreTrabajador NVARCHAR(200),
        Rol NVARCHAR(50),
        EsJefe BIT,
        Monto DECIMAL(18,2),
        TotalAsignado DECIMAL(18,2),
        FechaActualizacion DATETIME DEFAULT GETDATE()
    );
END";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }

        /// <summary>Crea RecibosNomina si no existe (idempotente).</summary>
        private static void EnsureTablaRecibos(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RecibosNomina')
BEGIN
    CREATE TABLE RecibosNomina (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdTrabajador INT NULL,
        NombreTrabajador NVARCHAR(200) NOT NULL,
        Rol NVARCHAR(80) NULL,
        CodigoCuadrilla NVARCHAR(20) NULL,
        Concepto NVARCHAR(MAX) NULL,
        Monto DECIMAL(18,2) NOT NULL,
        FechaRecibo DATE NOT NULL,
        PeriodoDesde DATE NULL,
        PeriodoHasta DATE NULL,
        TotalCuadrilla DECIMAL(18,2) NULL,
        Pdf VARBINARY(MAX) NULL,
        Usuario NVARCHAR(120) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Agrega de forma idempotente las columnas NominaDistribuida y
        /// FechaDistribucionNomina a ActivacionTareasRuta (bandera de pago).
        /// </summary>
        private static void EnsureColumnasNominaDistribuida(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ActivacionTareasRuta')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE Name = N'NominaDistribuida'
                     AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    BEGIN
        ALTER TABLE ActivacionTareasRuta ADD NominaDistribuida BIT NOT NULL DEFAULT 0;
    END
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE Name = N'FechaDistribucionNomina'
                     AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    BEGIN
        ALTER TABLE ActivacionTareasRuta ADD FechaDistribucionNomina DATETIME NULL;
    END
END";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }
    }
}
