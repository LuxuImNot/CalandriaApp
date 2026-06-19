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
    /// Folios de estimación de FormEstimacionConceptoMigrado: numeración global,
    /// creación de las tablas (FoliosEstimacion / FoliosEstimacionDetalle /
    /// PDFsEstimacion) y guardado de una estimación completa (cabecera + detalle +
    /// PDF) en una transacción. El PDF se genera en el cliente y viaja en base64.
    /// </summary>
    [RoutePrefix("api/folios-estimacion")]
    public class FoliosEstimacionController : ApiController
    {
        /// <summary>
        /// GET /api/folios-estimacion/siguiente-numero · siguiente número de estimación
        /// global (MAX(NumeroEstimacion)+1 de todo el proyecto, 1 si no hay tabla).
        /// </summary>
        [HttpGet, Route("siguiente-numero")]
        public IHttpActionResult SiguienteNumero()
        {
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(@"
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosEstimacion')
    SELECT ISNULL(MAX(NumeroEstimacion), 0) + 1 FROM FoliosEstimacion;
ELSE
    SELECT 1;", conn))
            {
                var result = cmd.ExecuteScalar();
                return Ok(result == null || result == DBNull.Value ? 1 : Convert.ToInt32(result));
            }
        }

        /// <summary>
        /// POST /api/folios-estimacion/ensure-tablas · crea las tablas de folios si no
        /// existen (lo usa el cliente antes de abrir el repositorio de PDFs).
        /// </summary>
        [HttpPost, Route("ensure-tablas")]
        public IHttpActionResult EnsureTablasEndpoint()
        {
            using (var conn = Db.Abrir())
                EnsureTablas(conn, null);
            return Ok();
        }

        /// <summary>
        /// POST /api/folios-estimacion · guarda la cabecera (FoliosEstimacion), el detalle
        /// (FoliosEstimacionDetalle, vinculando IdPresupuestoObra si la columna existe) y
        /// el PDF (PDFsEstimacion) en una transacción. Devuelve el Id del folio.
        /// </summary>
        [HttpPost, Route("")]
        public IHttpActionResult Guardar([FromBody] GuardarFolioEstimacionRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Folio))
                return BadRequest("Falta el folio.");
            if (string.IsNullOrWhiteSpace(req.Manzana) || string.IsNullOrWhiteSpace(req.Lote))
                return BadRequest("Faltan manzana y/o lote.");

            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn, null);
                bool tieneIdPresupuesto = ColumnasDe(conn, "FoliosEstimacionDetalle").Contains("IdPresupuestoObra");

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int folioId = InsertarCabecera(conn, tx, req);
                        InsertarDetalle(conn, tx, folioId, req.Detalle, tieneIdPresupuesto);
                        InsertarPdf(conn, tx, folioId, req);

                        tx.Commit();
                        return Ok(new GuardarFolioEstimacionResponse { FolioId = folioId });
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // ---- helpers ----

        private static int InsertarCabecera(SqlConnection conn, SqlTransaction tx, GuardarFolioEstimacionRequest req)
        {
            using (var cmd = new SqlCommand(@"
INSERT INTO FoliosEstimacion
(Folio, Manzana, Lote, Prototipo, FechaGeneracion, Proveedor, Descripcion,
 ImporteContrato, TotalRequisicion, Amortizacion, PorcentajeAmortizacion,
 TotalEstimacion, NumeroEstimacion, Usuario)
VALUES
(@folio, @manzana, @lote, @prototipo, GETDATE(), @proveedor, @descripcion,
 @importeContrato, @totalRequisicion, @amortizacion, @porcentajeAmortizacion,
 @totalEstimacion, @numeroEstimacion, @usuario);
SELECT SCOPE_IDENTITY();", conn, tx))
            {
                cmd.Parameters.AddWithValue("@folio", req.Folio);
                cmd.Parameters.AddWithValue("@manzana", req.Manzana);
                cmd.Parameters.AddWithValue("@lote", req.Lote);
                cmd.Parameters.AddWithValue("@prototipo", (object)req.Prototipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@proveedor", (object)req.Proveedor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@descripcion", (object)req.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@importeContrato", req.ImporteContrato);
                cmd.Parameters.AddWithValue("@totalRequisicion", req.TotalRequisicion);
                cmd.Parameters.AddWithValue("@amortizacion", req.Amortizacion);
                cmd.Parameters.AddWithValue("@porcentajeAmortizacion", req.PorcentajeAmortizacion);
                cmd.Parameters.AddWithValue("@totalEstimacion", req.TotalEstimacion);
                cmd.Parameters.AddWithValue("@numeroEstimacion", req.NumeroEstimacion);
                cmd.Parameters.AddWithValue("@usuario", (object)req.Usuario ?? DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static void InsertarDetalle(SqlConnection conn, SqlTransaction tx, int folioId,
            List<DetalleFolioDto> detalle, bool tieneIdPresupuesto)
        {
            if (detalle == null) return;

            string sql = tieneIdPresupuesto
                ? @"INSERT INTO FoliosEstimacionDetalle
(FolioId, CodigoConcepto, NombreConcepto, WBS, IdPresupuestoObra, NombrePartida,
 MontoPresupuestado, MontoEjecutado, AvancePorcentaje)
VALUES (@folioId, @codigoConcepto, @nombreConcepto, @wbs, @idPresupuestoObra, @nombrePartida,
 @montoPresupuestado, @montoEjecutado, @avancePorcentaje)"
                : @"INSERT INTO FoliosEstimacionDetalle
(FolioId, CodigoConcepto, NombreConcepto, WBS, NombrePartida,
 MontoPresupuestado, MontoEjecutado, AvancePorcentaje)
VALUES (@folioId, @codigoConcepto, @nombreConcepto, @wbs, @nombrePartida,
 @montoPresupuestado, @montoEjecutado, @avancePorcentaje)";

            foreach (var d in detalle)
            {
                int? idPresupuesto = null;
                if (tieneIdPresupuesto && d.Wbs > 0)
                {
                    using (var cmdGet = new SqlCommand(
                        "SELECT Id FROM PresupuestoObra WHERE WBS_Correcto = @wbs", conn, tx))
                    {
                        cmdGet.Parameters.AddWithValue("@wbs", d.Wbs);
                        var v = cmdGet.ExecuteScalar();
                        if (v != null && v != DBNull.Value) idPresupuesto = Convert.ToInt32(v);
                    }
                }

                using (var cmd = new SqlCommand(sql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@folioId", folioId);
                    cmd.Parameters.AddWithValue("@codigoConcepto", (object)d.CodigoConcepto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nombreConcepto", (object)d.NombreConcepto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@wbs", d.Wbs);
                    if (tieneIdPresupuesto)
                        cmd.Parameters.AddWithValue("@idPresupuestoObra", idPresupuesto.HasValue ? (object)idPresupuesto.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@nombrePartida", (object)d.NombrePartida ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@montoPresupuestado", d.MontoPresupuestado);
                    cmd.Parameters.AddWithValue("@montoEjecutado", d.MontoEjecutado);
                    cmd.Parameters.AddWithValue("@avancePorcentaje", d.AvancePorcentaje);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void InsertarPdf(SqlConnection conn, SqlTransaction tx, int folioId, GuardarFolioEstimacionRequest req)
        {
            if (string.IsNullOrEmpty(req.PdfBase64))
                return;

            byte[] pdf = Convert.FromBase64String(req.PdfBase64);
            using (var cmd = new SqlCommand(@"
INSERT INTO PDFsEstimacion
(FolioId, Folio, Manzana, Lote, NombreArchivo, ContenidoPDF, TamanioBytes, FechaAlmacenamiento)
VALUES (@folioId, @folio, @manzana, @lote, @nombreArchivo, @contenidoPDF, @tamanio, GETDATE())", conn, tx))
            {
                cmd.Parameters.AddWithValue("@folioId", folioId);
                cmd.Parameters.AddWithValue("@folio", req.Folio);
                cmd.Parameters.AddWithValue("@manzana", req.Manzana);
                cmd.Parameters.AddWithValue("@lote", req.Lote);
                cmd.Parameters.AddWithValue("@nombreArchivo", (object)req.NombreArchivoPdf ?? "");
                var pCont = cmd.Parameters.Add("@contenidoPDF", SqlDbType.VarBinary, -1);
                pCont.Value = pdf;
                cmd.Parameters.AddWithValue("@tamanio", (long)pdf.Length);
                cmd.ExecuteNonQuery();
            }
        }

        private static void EnsureTablas(SqlConnection conn, SqlTransaction tx)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosEstimacion')
BEGIN
    CREATE TABLE FoliosEstimacion (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Folio NVARCHAR(50) NOT NULL UNIQUE,
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        Prototipo NVARCHAR(50),
        FechaGeneracion DATETIME DEFAULT GETDATE(),
        Proveedor NVARCHAR(200),
        Descripcion NVARCHAR(500),
        ImporteContrato FLOAT,
        TotalRequisicion FLOAT,
        Amortizacion FLOAT,
        PorcentajeAmortizacion FLOAT,
        TotalEstimacion FLOAT,
        NumeroEstimacion INT,
        Usuario NVARCHAR(100),
        Observaciones NVARCHAR(MAX)
    );
END
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosEstimacionDetalle')
BEGIN
    CREATE TABLE FoliosEstimacionDetalle (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        CodigoConcepto NVARCHAR(10),
        NombreConcepto NVARCHAR(200),
        WBS INT,
        NombrePartida NVARCHAR(500),
        MontoPresupuestado FLOAT,
        MontoEjecutado FLOAT,
        AvancePorcentaje FLOAT,
        FOREIGN KEY (FolioId) REFERENCES FoliosEstimacion(Id) ON DELETE CASCADE
    );
END
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PDFsEstimacion')
BEGIN
    CREATE TABLE PDFsEstimacion (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Folio NVARCHAR(50) NOT NULL,
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        NombreArchivo NVARCHAR(255),
        ContenidoPDF VARBINARY(MAX) NOT NULL,
        TamanioBytes BIGINT,
        FechaAlmacenamiento DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (FolioId) REFERENCES FoliosEstimacion(Id) ON DELETE CASCADE
    );
END";
            using (var cmd = tx == null ? new SqlCommand(sql, conn) : new SqlCommand(sql, conn, tx))
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
