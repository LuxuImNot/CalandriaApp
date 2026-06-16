using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

public static class Reportes
{
    // Genera PDF por DESTAJO usando solo:
    // - de InsumosPorDestajo: Destajo, Clave, Insumo
    // - de ExplosionInsumos : MIN(Cantidad), MIN(Unidad)
    public static void GenerarPDFInsumosPorDestajo_Min(string destajo)
    {
        string connStr = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        // Ahora traemos UNIDAD y CANTIDAD como TEXTO para poder poner "N/A" en no-insumos
        var filas = new List<(string Clave, string Descripcion, string UnidadTxt, string CantidadTxt)>();

        string sql = @"
SELECT 
    ipd.Clave,
    Descripcion = COALESCE(MAX(ei.Descripcion), MAX(NULLIF(ipd.Insumo, ''))),
    UnidadTxt   = CASE WHEN COUNT(ei.Clave) > 0 THEN MAX(ei.Unidad) ELSE 'N/A' END,
    CantidadTxt = CASE WHEN COUNT(ei.Clave) > 0 
                       THEN CONVERT(NVARCHAR(40), MIN(CAST(ei.Cantidad AS float)))
                       ELSE 'N/A' END
FROM dbo.InsumosPorDestajo AS ipd
LEFT JOIN dbo.ExplosionInsumos AS ei
       ON ei.Clave = ipd.Clave
WHERE
    REPLACE(LTRIM(RTRIM(ipd.Destajo)),'  ',' ') COLLATE Latin1_General_CI_AI =
    REPLACE(LTRIM(RTRIM(@destajo))    ,'  ',' ') COLLATE Latin1_General_CI_AI
GROUP BY ipd.Clave
ORDER BY ipd.Clave;";

        using (var conn = new SqlConnection(connStr))
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@destajo", destajo ?? "");
            conn.Open();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    filas.Add((
                        r["Clave"]?.ToString() ?? "",
                        r["Descripcion"]?.ToString() ?? "",
                        r["UnidadTxt"]?.ToString() ?? "",
                        r["CantidadTxt"]?.ToString() ?? ""
                    ));
                }
            }
        }

        if (filas.Count == 0)
        {
            System.Windows.Forms.MessageBox.Show("No hay insumos definidos para este destajo.");
            return;
        }

        // ========= PDF =========
        string carpetaPdf = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "CALANDRIA RESIDENCIAL", "PDFInsumosDestajo");
        Directory.CreateDirectory(carpetaPdf);

        string San(string s) { foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_'); return s.Trim(); }
        string rutaPdf = Path.Combine(carpetaPdf, $"Insumos_{San(destajo)}_{DateTime.Now:yyyyMMdd}.pdf");

        var pdf = new PdfDocument();
        var page = pdf.AddPage();
        page.Size = PdfSharp.PageSize.Letter;

        var gfx = XGraphics.FromPdfPage(page);
        var font = new XFont("Arial", 10, XFontStyle.Regular);
        var bold = new XFont("Arial", 10, XFontStyle.Bold);
        var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
        var tf = new XTextFormatter(gfx);

        int marginX = 40, y = 40;
        double usableWidth = page.Width - marginX * 2;
        using (var ms = new MemoryStream())

        {
            DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            using (var logo = XImage.FromStream(ms))
            {
                gfx.DrawImage(logo, page.Width - 150, 20, 100, 50); // posición y tamaño iguales a antes
            }
        }

        // Encabezado estilo de tus otros PDFs
        gfx.DrawString("Desarrolladora de Casas Camaney", bold, XBrushes.Black, marginX, y); y += 15;
        gfx.DrawString("Blvd. Periférico sur y Carretera a la Colorada", font, XBrushes.Black, marginX, y); y += 15;
        gfx.DrawString("Tel. 662-XXX-XXXX | Email: constcas@empresa.com", font, XBrushes.Black, marginX, y); y += 30;
        gfx.DrawLine(XPens.Gray, marginX, y, page.Width - marginX, y); y += 25;

        gfx.DrawString("EXPLOSIÓN DE INSUMOS POR DESTAJO", titleFont, XBrushes.Black,
            new XRect(marginX, y, usableWidth, 30), XStringFormats.Center);
        y += 40;

        string usuario = DynamicSepticSystem.Global.UsuarioActual?.Nombre ?? Environment.UserName;
        gfx.DrawString($"Destajo: {destajo}", font, XBrushes.Black, marginX, y); y += 18;
        gfx.DrawString($"Usuario: {usuario}", font, XBrushes.Black, marginX, y); y += 18;
        gfx.DrawString($"Fecha: {DateTime.Now:dd/MM/yyyy}", font, XBrushes.Black, marginX, y); y += 22;

        // ---------- AutoSize de columnas ----------
        // Columnas: 0=CLAVE, 1=DESCRIPCIÓN, 2=UNIDAD, 3=CANTIDAD (texto)
        string[] headers = { "CLAVE", "DESCRIPCIÓN", "UNIDAD", "CANTIDAD (mín)" };

        double pad = 8;
        double[] need = new double[4];

        // medir headers
        for (int i = 0; i < headers.Length; i++)
            need[i] = Math.Max(need[i], gfx.MeasureString(headers[i], bold).Width + pad);

        // medir datos (usa CantidadTxt/UnidadTxt como string → permite 'N/A')
        foreach (var f in filas)
        {
            need[0] = Math.Max(need[0], gfx.MeasureString(f.Clave ?? "", font).Width + pad);
            need[1] = Math.Max(need[1], gfx.MeasureString(f.Descripcion ?? "", font).Width + pad);
            need[2] = Math.Max(need[2], gfx.MeasureString(f.UnidadTxt ?? "", font).Width + pad);
            need[3] = Math.Max(need[3], gfx.MeasureString(f.CantidadTxt ?? "", font).Width + pad);
        }

        // mínimos
        double minClave = 70, minDesc = 140, minUni = 60, minCant = 80;
        need[0] = Math.Max(need[0], minClave);
        need[1] = Math.Max(need[1], minDesc);
        need[2] = Math.Max(need[2], minUni);
        need[3] = Math.Max(need[3], minCant);

        double totalNeed = need[0] + need[1] + need[2] + need[3];
        double[] col = (double[])need.Clone();

        if (totalNeed > usableWidth)
        {
            double scale = usableWidth / totalNeed;
            for (int i = 0; i < col.Length; i++) col[i] = Math.Floor(col[i] * scale);

            col[0] = Math.Max(col[0], minClave);
            col[1] = Math.Max(col[1], minDesc);
            col[2] = Math.Max(col[2], minUni);
            col[3] = Math.Max(col[3], minCant);

            double sum = col[0] + col[1] + col[2] + col[3];
            if (sum > usableWidth)
            {
                double exceso = sum - usableWidth;
                double reducible = Math.Max(0, col[1] - minDesc);
                double reduce = Math.Min(exceso, reducible);
                col[1] -= reduce;
                sum -= reduce;

                if (sum > usableWidth)
                {
                    exceso = sum - usableWidth;
                    reducible = Math.Max(0, col[0] - minClave);
                    reduce = Math.Min(exceso, reducible);
                    col[0] -= reduce;
                    sum -= reduce;
                }
                if (sum > usableWidth)
                {
                    exceso = sum - usableWidth;
                    double reducible2 = Math.Max(0, col[2] - minUni) + Math.Max(0, col[3] - minCant);
                    if (reducible2 > 0)
                    {
                        double r2 = Math.Min(exceso, reducible2);
                        double pUni = Math.Max(0, col[2] - minUni) / reducible2;
                        double pCant = 1 - pUni;
                        col[2] -= r2 * pUni;
                        col[3] -= r2 * pCant;
                    }
                }
            }
        }
        else
        {
            col[1] += (usableWidth - totalNeed); // el sobrante a DESCRIPCIÓN
        }

        int rowH = 22;
        int x = marginX;

        // Encabezados
        int colX = x;
        for (int i = 0; i < headers.Length; i++)
        {
            gfx.DrawRectangle(XPens.Black, colX, y, col[i], rowH);
            gfx.DrawString(headers[i], bold, XBrushes.Black,
                new XRect(colX, y, col[i], rowH), XStringFormats.Center);
            colX += (int)col[i];
        }
        y += rowH;

        // Filas
        foreach (var f in filas)
        {
            if (y > page.Height - 100)
            {
                page = pdf.AddPage();
                page.Size = PdfSharp.PageSize.Letter;
                gfx = XGraphics.FromPdfPage(page);
                tf = new XTextFormatter(gfx);
                y = 40;
            }

            string clave = f.Clave ?? "";
            string desc = f.Descripcion ?? "";
            string unidad = f.UnidadTxt ?? "";
            string cant = f.CantidadTxt ?? "";

            // altura dinámica por descripción
            int linesDesc = Math.Max(1, (int)Math.Ceiling(gfx.MeasureString(desc, font).Width / Math.Max(1, col[1] - pad)));
            int alto = Math.Max(rowH, linesDesc * rowH);

            colX = x;

            // CLAVE
            gfx.DrawRectangle(XPens.Black, colX, y, col[0], alto);
            gfx.DrawString(clave, font, XBrushes.Black,
                new XRect(colX + 3, y + 3, col[0] - 6, alto - 6), XStringFormats.TopLeft);
            colX += (int)col[0];

            // DESCRIPCIÓN (wrap)
            gfx.DrawRectangle(XPens.Black, colX, y, col[1], alto);
            tf.DrawString(desc, font, XBrushes.Black, new XRect(colX + 3, y + 3, col[1] - 6, alto - 6));
            colX += (int)col[1];

            // UNIDAD
            gfx.DrawRectangle(XPens.Black, colX, y, col[2], alto);
            gfx.DrawString(unidad, font, XBrushes.Black,
                new XRect(colX + 3, y + 3, col[2] - 6, alto - 6), XStringFormats.TopLeft);
            colX += (int)col[2];

            // CANTIDAD (texto: número o 'N/A')
            gfx.DrawRectangle(XPens.Black, colX, y, col[3], alto);
            gfx.DrawString(cant, font, XBrushes.Black,
                new XRect(colX + 3, y + 3, col[3] - 6, alto - 6), XStringFormats.TopLeft);

            y += alto;
        }

        y += 28;
        gfx.DrawString("_______________________________", font, XBrushes.Black, marginX, y);
        gfx.DrawString("Encargado de Compras - Nombre y Firma", font, XBrushes.Black, marginX, y + 18);

        pdf.Save(rutaPdf);
        try { Process.Start(rutaPdf); } catch { }
    }


}
