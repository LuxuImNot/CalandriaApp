// Servicio compartido para las SALIDAS de almacen (vales) y la liberacion
// automatica de insumos al activar un destajo. Es la fuente unica de la logica
// de salidas: tanto el flujo manual (FormAlmacen) como el automatico
// (FormActivarTareasTreeList) la usan, para no duplicar la generacion de vales.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    public class SalidaAlmacenService
    {
        private readonly string connectionString;

        public SalidaAlmacenService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        }

        public SalidaAlmacenService(string cs)
        {
            connectionString = cs;
        }

        // ====================================================================
        // Esquema (idempotente, defensivo) — equivalente a
        // SQL_SCRIPTS/AlmacenLiberacionDestajos.sql
        // ====================================================================
        public void EnsureEsquema()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                AsegurarEsquema(conn, null);
            }
        }

        /// <summary>Asegura el esquema usando una conexion ya abierta.</summary>
        public void EnsureEsquema(SqlConnection conn) => AsegurarEsquema(conn, null);

        private void AsegurarEsquema(SqlConnection conn, SqlTransaction tx)
        {
            string ddl = @"
IF OBJECT_ID(N'dbo.SalidasAlmacen', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.SalidasAlmacen','OrigenRuta')  IS NULL ALTER TABLE dbo.SalidasAlmacen ADD [OrigenRuta] NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.SalidasAlmacen','OrigenNodoID')IS NULL ALTER TABLE dbo.SalidasAlmacen ADD [OrigenNodoID] INT NULL;
    IF COL_LENGTH('dbo.SalidasAlmacen','OrigenTipo')  IS NULL ALTER TABLE dbo.SalidasAlmacen ADD [OrigenTipo] NVARCHAR(20) NULL;
END
IF OBJECT_ID(N'dbo.FoliosSalidaAlmacen', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.FoliosSalidaAlmacen','OrigenRuta')  IS NULL ALTER TABLE dbo.FoliosSalidaAlmacen ADD [OrigenRuta] NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.FoliosSalidaAlmacen','OrigenNodoID')IS NULL ALTER TABLE dbo.FoliosSalidaAlmacen ADD [OrigenNodoID] INT NULL;
END
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
            using (var cmd = new SqlCommand(ddl, conn, tx))
                cmd.ExecuteNonQuery();
        }

        // ====================================================================
        // Stock disponible = SUM(Entradas) - SUM(Salidas) por Clave
        // ====================================================================
        public decimal ObtenerDisponible(SqlConnection conn, string clave, SqlTransaction tx = null)
        {
            const string sql = @"
                SELECT ISNULL((SELECT SUM(Cantidad) FROM EntradasAlmacen WHERE Clave = @c), 0)
                     - ISNULL((SELECT SUM(Cantidad) FROM SalidasAlmacen  WHERE Clave = @c), 0)";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@c", clave ?? string.Empty);
                var res = cmd.ExecuteScalar();
                return (res == null || res == DBNull.Value) ? 0m : Convert.ToDecimal(res);
            }
        }

        // ====================================================================
        // Idempotencia: ¿este destajo ya libero sus insumos?
        // ====================================================================
        public bool YaLiberado(string ruta, int nodoId, string manzana, string lote)
        {
            const string sql = @"
                SELECT COUNT(*) FROM dbo.SalidasAlmacen
                WHERE OrigenRuta = @r AND OrigenNodoID = @n AND Manzana = @m AND Lote = @l";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@r", ruta ?? string.Empty);
                        cmd.Parameters.AddWithValue("@n", nodoId);
                        cmd.Parameters.AddWithValue("@m", manzana ?? string.Empty);
                        cmd.Parameters.AddWithValue("@l", lote ?? string.Empty);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch
            {
                // Si la columna OrigenRuta aun no existe, no hay nada liberado.
                return false;
            }
        }

        // ====================================================================
        // Excepciones (p.ej. desactivar un destajo ya liberado)
        // ====================================================================
        public void RegistrarExcepcion(
            string tipo, string manzana, string lote, string ruta, int nodoId,
            string nombreDestajo, string usuario, string justificacion, string detalleJson, decimal total)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                AsegurarEsquema(conn, null);
                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.ExcepcionesLiberacionAlmacen
                    (Tipo, Manzana, Lote, Ruta, NodoID, NombreDestajo, Usuario, Justificacion, DetalleJson, TotalImporte)
                    VALUES (@Tipo, @Manzana, @Lote, @Ruta, @NodoID, @Nombre, @Usuario, @Just, @Detalle, @Total)", conn))
                {
                    cmd.Parameters.AddWithValue("@Tipo", tipo ?? "Excepcion");
                    cmd.Parameters.AddWithValue("@Manzana", (object)manzana ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Lote", (object)lote ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Ruta", (object)ruta ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NodoID", nodoId);
                    cmd.Parameters.AddWithValue("@Nombre", (object)nombreDestajo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Usuario", (object)usuario ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Just", (object)justificacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Detalle", (object)detalleJson ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>Insumos ya liberados por un destajo (para snapshot de excepcion).</summary>
        public List<InsumoSalida> ObtenerInsumosLiberados(string ruta, int nodoId, string manzana, string lote)
        {
            var lista = new List<InsumoSalida>();
            const string sql = @"
                SELECT Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe
                FROM dbo.SalidasAlmacen
                WHERE OrigenRuta = @r AND OrigenNodoID = @n AND Manzana = @m AND Lote = @l";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@r", ruta ?? string.Empty);
                        cmd.Parameters.AddWithValue("@n", nodoId);
                        cmd.Parameters.AddWithValue("@m", manzana ?? string.Empty);
                        cmd.Parameters.AddWithValue("@l", lote ?? string.Empty);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                lista.Add(new InsumoSalida
                                {
                                    Codigo = rd["Clave"]?.ToString(),
                                    Descripcion = rd["Descripcion"]?.ToString(),
                                    Unidad = rd["Unidad"]?.ToString(),
                                    Cantidad = rd["Cantidad"] != DBNull.Value ? Convert.ToDecimal(rd["Cantidad"]) : 0m,
                                    PrecioUnitario = rd["PrecioUnitario"] != DBNull.Value ? Convert.ToDecimal(rd["PrecioUnitario"]) : 0m,
                                    Importe = rd["Importe"] != DBNull.Value ? Convert.ToDecimal(rd["Importe"]) : 0m
                                });
                            }
                        }
                    }
                }
            }
            catch { }
            return lista;
        }

        // ====================================================================
        // Vale: folio + PDF + repositorio  (reutilizados por FormAlmacen)
        // ====================================================================
        public string GenerarFolioSalida(string manzana, string lote, SqlConnection conn, SqlTransaction tx = null)
        {
            string anio = DateTime.Now.Year.ToString();
            int siguienteNumero = ObtenerSiguienteNumeroSalida(manzana, lote, conn, tx);
            return $"VALE-M{manzana}-L{lote}-{siguienteNumero:D3}-{anio}";
        }

        public int ObtenerSiguienteNumeroSalida(string manzana, string lote, SqlConnection conn, SqlTransaction tx = null)
        {
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(MAX(NumeroSalida), 0) + 1
                FROM FoliosSalidaAlmacen
                WHERE Manzana = @Manzana AND Lote = @Lote", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Manzana", manzana);
                cmd.Parameters.AddWithValue("@Lote", lote);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void CrearTablasRepositorioSalidasSiNoExisten(SqlConnection conn, SqlTransaction tx = null)
        {
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosSalidaAlmacen'", conn, tx))
            {
                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) return;
            }

            string scriptPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "SQL_SCRIPTS", "CrearTablasRepositorioSalidasAlmacen.sql");
            if (!File.Exists(scriptPath)) return;

            string script = File.ReadAllText(scriptPath);
            foreach (string batch in script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                try
                {
                    using (var batchCmd = new SqlCommand(batch, conn, tx))
                        batchCmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        public void GuardarValeSalidaEnRepositorio(
            string folio, string rutaPdf, List<InsumoSalida> insumos, decimal totalImporte, DatosVale datos,
            string manzana, string lote, string prototipo, string origenRuta, int? origenNodoId,
            SqlConnection conn, SqlTransaction tx = null)
        {
            byte[] pdfBytes = File.ReadAllBytes(rutaPdf);
            DateTime fechaSolicitud = DateTime.Now;
            int numeroSalida = ObtenerSiguienteNumeroSalida(manzana, lote, conn, tx);

            int folioId;
            using (var cmd = new SqlCommand(@"
                INSERT INTO FoliosSalidaAlmacen
                (Folio, Manzana, Lote, Prototipo, Obra, FechaSolicitud, DiaSolicitud, MesSolicitud, AnioSolicitud,
                 TotalImporte, NumeroSalida, Usuario, Solicitante, ResidenteObra, EncargadoAlmacen, Observaciones, Estado,
                 OrigenRuta, OrigenNodoID)
                VALUES
                (@Folio, @Manzana, @Lote, @Prototipo, @Obra, @Fecha, @Dia, @Mes, @Anio,
                 @Total, @NumSalida, @Usuario, @Solicitante, @Residente, @Encargado, @Obs, 'PENDIENTE',
                 @OrigenRuta, @OrigenNodoID);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Folio", folio);
                cmd.Parameters.AddWithValue("@Manzana", manzana);
                cmd.Parameters.AddWithValue("@Lote", lote);
                cmd.Parameters.AddWithValue("@Prototipo", (object)prototipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Obra", $"M{manzana}-L{lote}");
                cmd.Parameters.AddWithValue("@Fecha", fechaSolicitud);
                cmd.Parameters.AddWithValue("@Dia", fechaSolicitud.Day);
                cmd.Parameters.AddWithValue("@Mes", fechaSolicitud.Month);
                cmd.Parameters.AddWithValue("@Anio", fechaSolicitud.Year);
                cmd.Parameters.AddWithValue("@Total", totalImporte);
                cmd.Parameters.AddWithValue("@NumSalida", numeroSalida);
                cmd.Parameters.AddWithValue("@Usuario", Global.UsuarioActual?.Nombre ?? Environment.UserName);
                cmd.Parameters.AddWithValue("@Solicitante", (object)datos?.Solicitante ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Residente", (object)datos?.ResidenteObra ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Encargado", (object)datos?.EncargadoAlmacen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Obs", (object)datos?.Observaciones ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OrigenRuta", (object)origenRuta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OrigenNodoID", (object)origenNodoId ?? DBNull.Value);
                folioId = (int)cmd.ExecuteScalar();
            }

            int numeroFila = 1;
            foreach (var insumo in insumos)
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO FoliosSalidaAlmacenDetalle
                    (FolioId, NumeroFila, Codigo, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe)
                    VALUES (@FolioId, @NumFila, @Codigo, @Desc, @Unidad, @Cant, @Precio, @Importe)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@FolioId", folioId);
                    cmd.Parameters.AddWithValue("@NumFila", numeroFila++);
                    cmd.Parameters.AddWithValue("@Codigo", insumo.Codigo ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Desc", insumo.Descripcion ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Unidad", insumo.Unidad ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Cant", insumo.Cantidad);
                    cmd.Parameters.AddWithValue("@Precio", insumo.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@Importe", insumo.Importe);
                    cmd.ExecuteNonQuery();
                }
            }

            using (var cmd = new SqlCommand(@"
                INSERT INTO PDFsSalidaAlmacen
                (FolioId, Folio, Manzana, Lote, NombreArchivo, ContenidoPDF, TamanioBytes)
                VALUES (@FolioId, @Folio, @Manzana, @Lote, @Nombre, @Contenido, @Tamanio)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@FolioId", folioId);
                cmd.Parameters.AddWithValue("@Folio", folio);
                cmd.Parameters.AddWithValue("@Manzana", manzana);
                cmd.Parameters.AddWithValue("@Lote", lote);
                cmd.Parameters.AddWithValue("@Nombre", Path.GetFileName(rutaPdf));
                cmd.Parameters.AddWithValue("@Contenido", pdfBytes);
                cmd.Parameters.AddWithValue("@Tamanio", (long)pdfBytes.Length);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Genera el PDF del vale de salida y devuelve la ruta. Si
        /// <paramref name="abrirPdf"/> es true, lo abre al terminar.
        /// </summary>
        public string GenerarPDFValeSalida(
            string folio, List<InsumoSalida> insumos, decimal totalImporte, DatosVale datos,
            string manzana, string lote, bool abrirPdf)
        {
            string carpetaPdf = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CALANDRIA RESIDENCIAL", "PDFValesSalida");
            Directory.CreateDirectory(carpetaPdf);

            string rutaPdf = Path.Combine(carpetaPdf, $"ValeSalida_{folio}.pdf");

            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = "Vale de Salida de Almacen - " + folio;
            pdf.Info.Author = "Desarrolladora de Casas Camaney";
            pdf.Info.Subject = "Vale de Salida";
            pdf.Info.Creator = "Sistema Calandria";

            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont fontTitle = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontBold = new XFont("Arial", 9, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontSmall = new XFont("Arial", 7, XFontStyle.Regular);
            XFont fontTiny = new XFont("Arial", 6, XFontStyle.Regular);

            int margin = 10;
            double y = margin;
            double pageWidth = page.Width - (margin * 2);

            // ENCABEZADO
            XRect borderRect = new XRect(margin, margin, pageWidth, 80);
            gfx.DrawRectangle(XPens.Black, borderRect);

            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    using (var logo = XImage.FromStream(ms))
                        gfx.DrawImage(logo, margin + 5, margin + 5, 120, 70);
                }
            }
            catch { }

            gfx.DrawLine(XPens.Black, margin + 135, margin, margin + 135, margin + 80);

            double textStartX = margin + 145;
            double textY = margin + 15;
            gfx.DrawString("DESARROLLADORA DE CASAS CAMANEY", fontBold, XBrushes.Black,
                new XRect(textStartX, textY, pageWidth - 155, 15), XStringFormats.TopCenter);
            textY += 13;
            gfx.DrawString("ALMACEN DE INSUMOS Y HERRAMIENTAS", fontNormal, XBrushes.Black,
                new XRect(textStartX, textY, pageWidth - 155, 15), XStringFormats.TopCenter);
            textY += 13;
            gfx.DrawString("VALE DE SALIDA DE ALMACEN", fontTitle, XBrushes.Black,
                new XRect(textStartX, textY, pageWidth - 155, 15), XStringFormats.TopCenter);

            y = margin + 80;
            gfx.DrawLine(XPens.Black, margin, y, margin + pageWidth, y);
            y += 5;

            // INFORMACION BASICA
            double col1X = margin + 10;
            double col2X = margin + 140;
            double col3X = margin + 300;
            double col4X = margin + 430;

            gfx.DrawString("OBRA", fontNormal, XBrushes.Black, col1X, y);
            gfx.DrawString($"M{manzana}-L{lote}", fontNormal, XBrushes.Black, col1X + 60, y);
            gfx.DrawLine(XPens.Black, col1X + 30, y + 2, col1X + 120, y + 2);

            gfx.DrawString("Edificacion", fontNormal, XBrushes.Black, col2X, y);
            gfx.DrawRectangle(XPens.Black, new XRect(col2X + 60, y - 5, 80, 12));

            gfx.DrawString("Urbanizacion", fontNormal, XBrushes.Black, col3X, y);
            gfx.DrawRectangle(XPens.Black, new XRect(col3X + 65, y - 5, 60, 12));

            gfx.DrawString("FOLIO:", fontBold, XBrushes.Black, col4X, y);
            string folioNumero = folio.Substring(folio.LastIndexOf('-') + 1);
            gfx.DrawString(folioNumero, fontBold, XBrushes.Black, col4X + 100, y);
            gfx.DrawLine(XPens.Black, col4X + 40, y + 2, col4X + 150, y + 2);

            y += 20;
            gfx.DrawString("FECHA DE SOLICITUD:", fontNormal, XBrushes.Black, col4X, y);
            y += 12;

            DateTime fecha = DateTime.Now;
            double diaX = col4X + 15;
            double mesX = diaX + 35;
            double anioX = mesX + 35;

            gfx.DrawRectangle(XPens.Black, new XRect(diaX, y, 25, 12));
            gfx.DrawString(fecha.Day.ToString("D2"), fontNormal, XBrushes.Black, new XRect(diaX, y + 2, 25, 12), XStringFormats.TopCenter);
            gfx.DrawRectangle(XPens.Black, new XRect(mesX, y, 25, 12));
            gfx.DrawString(fecha.Month.ToString("D2"), fontNormal, XBrushes.Black, new XRect(mesX, y + 2, 25, 12), XStringFormats.TopCenter);
            gfx.DrawRectangle(XPens.Black, new XRect(anioX, y, 35, 12));
            gfx.DrawString(fecha.Year.ToString(), fontNormal, XBrushes.Black, new XRect(anioX, y + 2, 35, 12), XStringFormats.TopCenter);

            y += 13;
            gfx.DrawString("Dia", fontTiny, XBrushes.Black, new XRect(diaX, y, 25, 10), XStringFormats.TopCenter);
            gfx.DrawString("Mes", fontTiny, XBrushes.Black, new XRect(mesX, y, 25, 10), XStringFormats.TopCenter);
            gfx.DrawString("Anio", fontTiny, XBrushes.Black, new XRect(anioX, y, 35, 10), XStringFormats.TopCenter);

            y += 15;
            gfx.DrawLine(XPens.Black, margin, y, margin + pageWidth, y);
            y += 5;

            // TABLA DE INSUMOS
            string[] headers = { "N", "CODIGO", "DESCRIPCION", "UNIDAD", "CANTIDAD", "PRECIO UNIT.", "IMPORTE", "OBSERVACIONES" };
            double[] widths = { 25, 60, 185, 50, 60, 75, 75, 60 };

            double tableY = y;
            double currentX = margin;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[i], 16));
                gfx.DrawString(headers[i], fontBold, XBrushes.Black,
                    new XRect(currentX + 2, tableY + 3, widths[i] - 4, 14), XStringFormats.TopCenter);
                currentX += widths[i];
            }
            tableY += 16;

            int maxFilas = 10;
            double rowHeight = 18;
            int filaActual = 1;
            foreach (var insumo in insumos.Take(maxFilas))
            {
                currentX = margin;
                gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[0], rowHeight));
                gfx.DrawString(filaActual.ToString(), fontSmall, XBrushes.Black, new XRect(currentX + 2, tableY + 5, widths[0] - 4, rowHeight), XStringFormats.TopCenter);
                currentX += widths[0];

                gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[1], rowHeight));
                gfx.DrawString(insumo.Codigo ?? "", fontSmall, XBrushes.Black, new XRect(currentX + 3, tableY + 5, widths[1] - 6, rowHeight), XStringFormats.TopLeft);
                currentX += widths[1];

                string desc = (insumo.Descripcion ?? "");
                if (desc.Length > 40) desc = desc.Substring(0, 37) + "...";
                gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[2], rowHeight));
                gfx.DrawString(desc, fontSmall, XBrushes.Black, new XRect(currentX + 3, tableY + 5, widths[2] - 6, rowHeight), XStringFormats.TopLeft);
                currentX += widths[2];

                gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[3], rowHeight));
                gfx.DrawString(insumo.Unidad ?? "", fontSmall, XBrushes.Black, new XRect(currentX + 2, tableY + 5, widths[3] - 4, rowHeight), XStringFormats.TopCenter);
                currentX += widths[3];

                gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[4], rowHeight));
                gfx.DrawString(insumo.Cantidad.ToString("N2"), fontSmall, XBrushes.Black, new XRect(currentX + 2, tableY + 5, widths[4] - 4, rowHeight), XStringFormats.TopCenter);
                currentX += widths[4];

                gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[5], rowHeight));
                gfx.DrawString("$   " + insumo.PrecioUnitario.ToString("N2"), fontSmall, XBrushes.Black, new XRect(currentX + 3, tableY + 5, widths[5] - 6, rowHeight), XStringFormats.TopRight);
                currentX += widths[5];

                gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[6], rowHeight));
                gfx.DrawString("$   " + insumo.Importe.ToString("N2"), fontSmall, XBrushes.Black, new XRect(currentX + 3, tableY + 5, widths[6] - 6, rowHeight), XStringFormats.TopRight);
                currentX += widths[6];

                gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[7], rowHeight));
                gfx.DrawString("-", fontSmall, XBrushes.Black, new XRect(currentX + 2, tableY + 5, widths[7] - 4, rowHeight), XStringFormats.TopCenter);

                tableY += rowHeight;
                filaActual++;
            }
            while (filaActual <= maxFilas)
            {
                currentX = margin;
                for (int i = 0; i < widths.Length; i++)
                {
                    gfx.DrawRectangle(XPens.Black, XBrushes.White, new XRect(currentX, tableY, widths[i], rowHeight));
                    if (i == 0)
                        gfx.DrawString(filaActual.ToString(), fontSmall, XBrushes.Black, new XRect(currentX + 2, tableY + 5, widths[i] - 4, rowHeight), XStringFormats.TopCenter);
                    currentX += widths[i];
                }
                tableY += rowHeight;
                filaActual++;
            }

            tableY += 8;
            double totalLabelX = margin + 390;
            gfx.DrawString("TOTAL", fontBold, XBrushes.Black, totalLabelX, tableY);
            gfx.DrawString("$" + totalImporte.ToString("N2"), fontBold, XBrushes.Black, totalLabelX + 150, tableY);

            // FIRMAS
            tableY += 50;
            double firmaWidth = 200;
            double firma1X = margin + 40;
            double firma2X = margin + 290;
            gfx.DrawLine(XPens.Black, firma1X, tableY, firma1X + firmaWidth, tableY);
            gfx.DrawString("SOLICITANTE", fontSmall, XBrushes.Black, new XRect(firma1X, tableY + 3, firmaWidth, 15), XStringFormats.TopCenter);
            gfx.DrawLine(XPens.Black, firma2X, tableY, firma2X + firmaWidth, tableY);
            gfx.DrawString("ENTREGA LOS INSUMOS", fontSmall, XBrushes.Black, new XRect(firma2X, tableY + 3, firmaWidth, 15), XStringFormats.TopCenter);

            tableY += 60;
            gfx.DrawLine(XPens.Black, firma1X, tableY, firma1X + firmaWidth, tableY);
            gfx.DrawString("RESIDENTE DE OBRA", fontSmall, XBrushes.Black, new XRect(firma1X, tableY + 3, firmaWidth, 15), XStringFormats.TopCenter);
            gfx.DrawString("Nombre y Firma", fontTiny, XBrushes.Black, new XRect(firma1X, tableY + 15, firmaWidth, 15), XStringFormats.TopCenter);
            gfx.DrawLine(XPens.Black, firma2X, tableY, firma2X + firmaWidth, tableY);
            gfx.DrawString("Encargado de almacen", fontSmall, XBrushes.Black, new XRect(firma2X, tableY + 3, firmaWidth, 15), XStringFormats.TopCenter);

            pdf.Save(rutaPdf);
            pdf.Close();

            if (abrirPdf)
            {
                try { Process.Start(new ProcessStartInfo(rutaPdf) { UseShellExecute = true }); }
                catch { try { Process.Start("explorer.exe", carpetaPdf); } catch { } }
            }

            return rutaPdf;
        }
    }
}
