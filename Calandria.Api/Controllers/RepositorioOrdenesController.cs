using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints del repositorio de PDFs de órdenes de compra (tablas
    /// FoliosOrdenCompra / FoliosOrdenCompraDetalle / FoliosOrdenCompra_Casas /
    /// PDFsOrdenCompra). Reproduce los parciales *_ExtensionPDFRepositorio,
    /// FormRepositorioPDFsOrdenesCompra y FormDetalleOrdenCompra. El PDF viaja en
    /// base64 al guardar y como binario al consultar.
    /// </summary>
    [RoutePrefix("api/repositorio-ordenes"), RequierePermiso("compras.ver")]
    public class RepositorioOrdenesController : ApiController
    {
        /// <summary>POST /api/repositorio-ordenes/multiple · guarda una orden múltiple/individual.</summary>
        [HttpPost, Route("multiple"), RequierePermiso("compras.editar")]
        public IHttpActionResult GuardarMultiple([FromBody] GuardarRepoMultipleRequest req)
        {
            if (req == null || req.Detalles == null || req.Detalles.Count == 0)
                return BadRequest("La orden no tiene detalle.");

            decimal subtotal = req.Detalles.Sum(d => d.ImporteTotal);
            decimal iva = subtotal * 0.16m;
            decimal total = subtotal + iva;

            string tipoOrden = "MULTIPLE";
            string manzana = null, lote = null;
            int? numeroOrden = null;
            var casas = req.Casas ?? new List<CasaRepoDto>();

            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn);

                if (casas.Count == 1)
                {
                    tipoOrden = "INDIVIDUAL";
                    manzana = casas[0].Manzana;
                    lote = casas[0].Lote;
                    numeroOrden = SiguienteNumeroOrden(conn, null, manzana, lote);
                }
                else if (casas.Count > 1)
                {
                    manzana = casas[0].Manzana;
                    lote = casas[0].Lote;
                }

                using (var tx = conn.BeginTransaction())
                {
                    int folioId = InsertarFolio(conn, tx, req.FolioOC, manzana, lote, tipoOrden,
                        req.NombreProveedor, req.CodigoProveedor, subtotal, iva, total, numeroOrden, req.Usuario);

                    InsertarDetalle(conn, tx, folioId, req.Detalles);

                    foreach (var c in casas)
                    {
                        using (var cmd = new SqlCommand(
                            "INSERT INTO FoliosOrdenCompra_Casas (FolioId, Manzana, Lote, Prototipo) VALUES (@f, @m, @l, @p)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@f", folioId);
                            cmd.Parameters.AddWithValue("@m", (object)c.Manzana ?? "");
                            cmd.Parameters.AddWithValue("@l", (object)c.Lote ?? "");
                            cmd.Parameters.AddWithValue("@p", (object)c.Prototipo ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    InsertarPdf(conn, tx, folioId, req.FolioOC, manzana, lote, tipoOrden, req.NombreArchivo, req.PdfBase64);

                    tx.Commit();

                    return Ok(new GuardarRepoResponse
                    {
                        FolioId = folioId,
                        TipoOrden = tipoOrden,
                        Total = total,
                        Insumos = req.Detalles.Count
                    });
                }
            }
        }

        /// <summary>POST /api/repositorio-ordenes/indirecta · guarda una orden indirecta/administrativa.</summary>
        [HttpPost, Route("indirecta"), RequierePermiso("compras.editar")]
        public IHttpActionResult GuardarIndirecta([FromBody] GuardarRepoIndirectaRequest req)
        {
            if (req == null || req.Detalles == null || req.Detalles.Count == 0)
                return BadRequest("La orden no tiene detalle.");

            decimal subtotal = req.Detalles.Sum(d => d.ImporteTotal);
            decimal iva = subtotal * 0.16m;
            decimal total = subtotal + iva;
            string tipoOrden = string.IsNullOrWhiteSpace(req.TipoOrden) ? "INDIRECTA" : req.TipoOrden;

            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn);

                using (var tx = conn.BeginTransaction())
                {
                    int folioId = InsertarFolio(conn, tx, req.FolioOC, null, null, tipoOrden,
                        req.NombreProveedor, req.CodigoProveedor, subtotal, iva, total, null, req.Usuario);

                    InsertarDetalle(conn, tx, folioId, req.Detalles);
                    InsertarPdf(conn, tx, folioId, req.FolioOC, null, null, tipoOrden, req.NombreArchivo, req.PdfBase64);

                    tx.Commit();

                    return Ok(new GuardarRepoResponse
                    {
                        FolioId = folioId,
                        TipoOrden = tipoOrden,
                        Total = total,
                        Insumos = req.Detalles.Count
                    });
                }
            }
        }

        /// <summary>
        /// GET /api/repositorio-ordenes?manzana=&amp;lote=&amp;soloIndirectas= · listado de órdenes.
        /// </summary>
        [HttpGet, Route("")]
        public IHttpActionResult Listar(string manzana = null, string lote = null, bool soloIndirectas = false)
        {
            var lista = new List<OrdenRepoDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn);

                string sql = @"
SELECT f.Id, f.Folio, f.Manzana, f.Lote, f.FechaGeneracion, f.TipoOrden,
       f.NombreProveedor, f.CodigoProveedor, f.TotalConIVA, f.NumeroOrden, f.Estado,
       COUNT(DISTINCT d.Id) AS TotalInsumos,
       STUFF((SELECT ', M' + c.Manzana + '-L' + c.Lote
              FROM FoliosOrdenCompra_Casas c WHERE c.FolioId = f.Id
              FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS CasasIncluidas
FROM FoliosOrdenCompra f
LEFT JOIN FoliosOrdenCompraDetalle d ON f.Id = d.FolioId
WHERE 1=1";

                bool filtroCasa = !soloIndirectas && !string.IsNullOrEmpty(manzana) && !string.IsNullOrEmpty(lote);
                if (soloIndirectas)
                {
                    sql += " AND f.TipoOrden IN ('INDIRECTA', 'ADMINISTRATIVA')";
                }
                else if (filtroCasa)
                {
                    sql += @" AND ((f.Manzana = @manzana AND f.Lote = @lote)
                                OR EXISTS (SELECT 1 FROM FoliosOrdenCompra_Casas fc
                                           WHERE fc.FolioId = f.Id AND fc.Manzana = @manzana AND fc.Lote = @lote))";
                }

                sql += @"
GROUP BY f.Id, f.Folio, f.Manzana, f.Lote, f.FechaGeneracion, f.TipoOrden,
         f.NombreProveedor, f.CodigoProveedor, f.TotalConIVA, f.NumeroOrden, f.Estado
ORDER BY f.FechaGeneracion DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (filtroCasa)
                    {
                        cmd.Parameters.AddWithValue("@manzana", manzana);
                        cmd.Parameters.AddWithValue("@lote", lote);
                    }
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new OrdenRepoDto
                            {
                                Id = r.GetInt32(0),
                                Folio = r.GetString(1),
                                Manzana = r.IsDBNull(2) ? "N/A" : r.GetString(2),
                                Lote = r.IsDBNull(3) ? "N/A" : r.GetString(3),
                                FechaGeneracion = r.GetDateTime(4),
                                TipoOrden = r.GetString(5),
                                NombreProveedor = r.IsDBNull(6) ? "" : r.GetString(6),
                                CodigoProveedor = r.IsDBNull(7) ? "" : r.GetString(7),
                                TotalConIVA = r.IsDBNull(8) ? 0m : Convert.ToDecimal(r.GetValue(8)),
                                NumeroOrden = r.IsDBNull(9) ? (int?)null : r.GetInt32(9),
                                Estado = r.IsDBNull(10) ? "PENDIENTE" : r.GetString(10),
                                TotalInsumos = r.GetInt32(11),
                                CasasIncluidas = r.IsDBNull(12) ? "" : r.GetString(12)
                            });
                        }
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/repositorio-ordenes/{folioId}/pdf · binario del PDF almacenado.</summary>
        [HttpGet, Route("{folioId:int}/pdf")]
        public HttpResponseMessage Pdf(int folioId)
        {
            byte[] bytes = null;
            string nombre = null;
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT ContenidoPDF, NombreArchivo FROM PDFsOrdenCompra WHERE FolioId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", folioId);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        bytes = r["ContenidoPDF"] == DBNull.Value ? null : (byte[])r["ContenidoPDF"];
                        nombre = r["NombreArchivo"]?.ToString();
                    }
                }
            }

            if (bytes == null || bytes.Length == 0)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            resp.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment") { FileName = nombre ?? $"OrdenCompra_{folioId}.pdf" };
            return resp;
        }

        /// <summary>GET /api/repositorio-ordenes/{folioId}/detalle · cabecera + casas + insumos.</summary>
        [HttpGet, Route("{folioId:int}/detalle")]
        public IHttpActionResult Detalle(int folioId)
        {
            var dto = new DetalleOrdenRepoResponse();
            bool encontrado = false;

            using (var conn = Db.Abrir())
            {
                using (var cmd = new SqlCommand(@"
SELECT Folio, Manzana, Lote, FechaGeneracion, TipoOrden, NombreProveedor, CodigoProveedor,
       TotalSinIVA, IVA, TotalConIVA, NumeroOrden, Usuario, Estado, Observaciones
FROM FoliosOrdenCompra WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", folioId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            encontrado = true;
                            dto.Folio = r["Folio"]?.ToString();
                            dto.Manzana = r["Manzana"] == DBNull.Value ? null : r["Manzana"].ToString();
                            dto.Lote = r["Lote"] == DBNull.Value ? null : r["Lote"].ToString();
                            dto.FechaGeneracion = Convert.ToDateTime(r["FechaGeneracion"]);
                            dto.TipoOrden = r["TipoOrden"]?.ToString();
                            dto.NombreProveedor = r["NombreProveedor"]?.ToString();
                            dto.CodigoProveedor = r["CodigoProveedor"]?.ToString();
                            dto.TotalSinIVA = r["TotalSinIVA"] == DBNull.Value ? 0m : Convert.ToDecimal(r["TotalSinIVA"]);
                            dto.IVA = r["IVA"] == DBNull.Value ? 0m : Convert.ToDecimal(r["IVA"]);
                            dto.TotalConIVA = r["TotalConIVA"] == DBNull.Value ? 0m : Convert.ToDecimal(r["TotalConIVA"]);
                            dto.NumeroOrden = r["NumeroOrden"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["NumeroOrden"]);
                            dto.Usuario = r["Usuario"]?.ToString();
                            dto.Estado = r["Estado"]?.ToString();
                            dto.Observaciones = r["Observaciones"] == DBNull.Value ? null : r["Observaciones"].ToString();
                        }
                    }
                }

                if (!encontrado)
                    return NotFound();

                using (var cmd = new SqlCommand(
                    "SELECT Manzana, Lote, Prototipo FROM FoliosOrdenCompra_Casas WHERE FolioId = @id ORDER BY Manzana, Lote", conn))
                {
                    cmd.Parameters.AddWithValue("@id", folioId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            dto.Casas.Add(new CasaRepoDto
                            {
                                Manzana = r.GetString(0),
                                Lote = r.GetString(1),
                                Prototipo = r.IsDBNull(2) ? "" : r.GetString(2)
                            });
                    }
                }

                using (var cmd = new SqlCommand(@"
SELECT Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Familia
FROM FoliosOrdenCompraDetalle WHERE FolioId = @id ORDER BY Clave", conn))
                {
                    cmd.Parameters.AddWithValue("@id", folioId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            dto.Insumos.Add(new DetalleInsumoRepoDto
                            {
                                Clave = r.GetString(0),
                                Descripcion = r.IsDBNull(1) ? "" : r.GetString(1),
                                Unidad = r.IsDBNull(2) ? "" : r.GetString(2),
                                Cantidad = r.IsDBNull(3) ? 0m : Convert.ToDecimal(r.GetValue(3)),
                                PrecioUnitario = r.IsDBNull(4) ? 0m : Convert.ToDecimal(r.GetValue(4)),
                                ImporteTotal = r.IsDBNull(5) ? 0m : Convert.ToDecimal(r.GetValue(5)),
                                Familia = r.IsDBNull(6) ? "" : r.GetString(6)
                            });
                    }
                }
            }
            return Ok(dto);
        }

        /// <summary>POST /api/repositorio-ordenes/{folioId}/eliminar · borra el folio (cascada).</summary>
        [HttpPost, Route("{folioId:int}/eliminar"), RequierePermiso("compras.editar")]
        public IHttpActionResult Eliminar(int folioId)
        {
            int filas;
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand("DELETE FROM FoliosOrdenCompra WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", folioId);
                filas = cmd.ExecuteNonQuery();
            }
            return Ok(filas);
        }

        // ---- helpers ----

        private static int InsertarFolio(SqlConnection conn, SqlTransaction tx, string folio,
            string manzana, string lote, string tipoOrden, string nombreProv, string codigoProv,
            decimal subtotal, decimal iva, decimal total, int? numeroOrden, string usuario)
        {
            using (var cmd = new SqlCommand(@"
INSERT INTO FoliosOrdenCompra
(Folio, Manzana, Lote, FechaGeneracion, TipoOrden, NombreProveedor, CodigoProveedor,
 TotalSinIVA, IVA, TotalConIVA, NumeroOrden, Usuario, Estado)
VALUES
(@folio, @m, @l, GETDATE(), @tipo, @np, @cp, @sub, @iva, @tot, @num, @user, 'PENDIENTE');
SELECT CAST(SCOPE_IDENTITY() AS INT)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@folio", (object)folio ?? "");
                cmd.Parameters.AddWithValue("@m", (object)manzana ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@l", (object)lote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tipo", (object)tipoOrden ?? "");
                cmd.Parameters.AddWithValue("@np", (object)nombreProv ?? "");
                cmd.Parameters.AddWithValue("@cp", (object)codigoProv ?? "");
                cmd.Parameters.AddWithValue("@sub", subtotal);
                cmd.Parameters.AddWithValue("@iva", iva);
                cmd.Parameters.AddWithValue("@tot", total);
                cmd.Parameters.AddWithValue("@num", (object)numeroOrden ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@user", (object)usuario ?? "");
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static void InsertarDetalle(SqlConnection conn, SqlTransaction tx,
            int folioId, List<DetalleOrdenDto> detalles)
        {
            foreach (var d in detalles)
            {
                using (var cmd = new SqlCommand(@"
INSERT INTO FoliosOrdenCompraDetalle
(FolioId, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Familia)
VALUES (@f, @c, @d, @u, @q, @p, @i, @fam)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@f", folioId);
                    cmd.Parameters.AddWithValue("@c", (object)d.Clave ?? "");
                    cmd.Parameters.AddWithValue("@d", (object)d.Descripcion ?? "");
                    cmd.Parameters.AddWithValue("@u", (object)d.Unidad ?? "");
                    cmd.Parameters.AddWithValue("@q", d.Cantidad);
                    cmd.Parameters.AddWithValue("@p", d.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@i", d.ImporteTotal);
                    cmd.Parameters.AddWithValue("@fam", (object)d.Familia ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void InsertarPdf(SqlConnection conn, SqlTransaction tx, int folioId, string folio,
            string manzana, string lote, string tipoOrden, string nombreArchivo, string pdfBase64)
        {
            if (string.IsNullOrEmpty(pdfBase64))
                return;

            byte[] pdf = Convert.FromBase64String(pdfBase64);
            using (var cmd = new SqlCommand(@"
INSERT INTO PDFsOrdenCompra
(FolioId, Folio, Manzana, Lote, TipoOrden, NombreArchivo, ContenidoPDF, TamanioBytes, FechaAlmacenamiento)
VALUES (@f, @folio, @m, @l, @tipo, @nom, @cont, @tam, GETDATE())", conn, tx))
            {
                cmd.Parameters.AddWithValue("@f", folioId);
                cmd.Parameters.AddWithValue("@folio", (object)folio ?? "");
                cmd.Parameters.AddWithValue("@m", (object)manzana ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@l", (object)lote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tipo", (object)tipoOrden ?? "");
                cmd.Parameters.AddWithValue("@nom", (object)nombreArchivo ?? "");
                var pCont = cmd.Parameters.Add("@cont", SqlDbType.VarBinary, -1);
                pCont.Value = pdf;
                cmd.Parameters.AddWithValue("@tam", (long)pdf.Length);
                cmd.ExecuteNonQuery();
            }
        }

        private static int SiguienteNumeroOrden(SqlConnection conn, SqlTransaction tx, string manzana, string lote)
        {
            using (var cmd = new SqlCommand(
                "SELECT ISNULL(MAX(NumeroOrden), 0) + 1 FROM FoliosOrdenCompra WHERE Manzana = @m AND Lote = @l", conn, tx))
            {
                cmd.Parameters.AddWithValue("@m", (object)manzana ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@l", (object)lote ?? DBNull.Value);
                var v = cmd.ExecuteScalar();
                return v == null || v == DBNull.Value ? 1 : Convert.ToInt32(v);
            }
        }

        /// <summary>Crea las 4 tablas del repositorio si no existen (con FK ON DELETE CASCADE).</summary>
        private static void EnsureTablas(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosOrdenCompra')
BEGIN
    CREATE TABLE FoliosOrdenCompra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Folio NVARCHAR(50) NOT NULL UNIQUE,
        Manzana NVARCHAR(10) NULL,
        Lote NVARCHAR(10) NULL,
        FechaGeneracion DATETIME DEFAULT GETDATE(),
        TipoOrden NVARCHAR(20) NOT NULL,
        NombreProveedor NVARCHAR(200),
        CodigoProveedor NVARCHAR(50),
        TotalSinIVA DECIMAL(18,2) DEFAULT 0,
        IVA DECIMAL(18,2) DEFAULT 0,
        TotalConIVA DECIMAL(18,2) DEFAULT 0,
        NumeroOrden INT,
        Usuario NVARCHAR(100),
        Observaciones NVARCHAR(MAX),
        Estado NVARCHAR(20) DEFAULT 'PENDIENTE'
    );
END
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosOrdenCompraDetalle')
BEGIN
    CREATE TABLE FoliosOrdenCompraDetalle (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Clave NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500),
        Unidad NVARCHAR(50),
        Cantidad DECIMAL(18,3) NOT NULL,
        PrecioUnitario DECIMAL(18,2) DEFAULT 0,
        ImporteTotal DECIMAL(18,2) DEFAULT 0,
        Familia NVARCHAR(100),
        FOREIGN KEY (FolioId) REFERENCES FoliosOrdenCompra(Id) ON DELETE CASCADE
    );
END
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosOrdenCompra_Casas')
BEGIN
    CREATE TABLE FoliosOrdenCompra_Casas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        Prototipo NVARCHAR(50),
        FOREIGN KEY (FolioId) REFERENCES FoliosOrdenCompra(Id) ON DELETE CASCADE
    );
END
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PDFsOrdenCompra')
BEGIN
    CREATE TABLE PDFsOrdenCompra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Folio NVARCHAR(50) NOT NULL,
        Manzana NVARCHAR(10) NULL,
        Lote NVARCHAR(10) NULL,
        TipoOrden NVARCHAR(20) NOT NULL,
        NombreArchivo NVARCHAR(255),
        ContenidoPDF VARBINARY(MAX) NOT NULL,
        TamanioBytes BIGINT,
        FechaAlmacenamiento DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (FolioId) REFERENCES FoliosOrdenCompra(Id) ON DELETE CASCADE
    );
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }
    }
}
