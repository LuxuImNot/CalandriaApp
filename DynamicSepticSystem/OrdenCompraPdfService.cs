using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Genera el PDF corporativo de una orden de compra (múltiple o indirecta).
    /// Extraído tal cual de FormCompraMulti.GenerarPDFCorporativo y
    /// FormCompraIndirecta.GenerarPDF para que FormComprasWeb (el puente WebView2)
    /// pueda generarlo sin depender de controles de un formulario clásico.
    /// </summary>
    public static class OrdenCompraPdfService
    {
        public sealed class InsumoPdf
        {
            public string Clave;
            public string Descripcion;
            public string Unidad;
            public decimal Cantidad;
            public decimal Precio;
            public decimal Importe => Math.Round(Precio * Cantidad, 2);
        }

        private static string CarpetaPdf()
        {
            string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CALANDRIA RESIDENCIAL", "PDFOrdenesCompra");
            Directory.CreateDirectory(carpeta);
            return carpeta;
        }

        private static void DibujarEncabezadoEmpresa(XGraphics gfx, int x, ref int y, XFont fontNormal, XFont fontSmall, PdfPage page, int margin)
        {
            gfx.DrawString("Desarrolladora de Casas Camaney", fontNormal, XBrushes.Black, x, y);
            y += 12;
            gfx.DrawString("Blvd. Periférico sur. Y Carretera a la Colorada", fontSmall, XBrushes.Black, x, y);
            y += 10;
            gfx.DrawString("Tel. 662xxxxxxxxxx", fontSmall, XBrushes.Black, x, y);
            y += 10;
            gfx.DrawString("Email constcas@h.....com", fontSmall, XBrushes.Black, x, y);

            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    using (var logo = XImage.FromStream(ms))
                    {
                        int logoWidth = 100, logoHeight = 89; // proporción del logo Pilaris
                        int logoX = (int)page.Width - margin - logoWidth;
                        gfx.DrawImage(logo, logoX, margin - 5, logoWidth, logoHeight);
                    }
                }
            }
            catch { /* sin logo no se detiene la generación del PDF */ }
        }

        private static void DibujarEncabezadoTabla(XGraphics gfx, int x, int y, string[] headers, int[] widths, int headerRowHeight, XFont fontTableHeader)
        {
            int currentX = x;
            for (int i = 0; i < headers.Length; i++)
            {
                var cellRect = new XRect(currentX, y, widths[i], headerRowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, cellRect);
                gfx.DrawString(headers[i], fontTableHeader, XBrushes.Black,
                    new XRect(currentX + 2, y + 5, widths[i] - 4, headerRowHeight - 5), XStringFormats.TopCenter);
                currentX += widths[i];
            }
        }

        /// <summary>Orden múltiple (FormCompraMulti.GenerarPDFCorporativo, sin cambios de layout).</summary>
        public static string GenerarMultiple(string folioOC, List<InsumoPdf> insumos, string nombreProveedor, string codigoProveedor, bool abrirPdf = true)
        {
            string rutaPdf = Path.Combine(CarpetaPdf(), $"OrdenCompra_{folioOC}.pdf");

            var pdf = new PdfDocument();
            pdf.Info.Title = "Orden de Compra - " + folioOC;
            pdf.Info.Author = "Desarrolladora de Casas Camaney";
            pdf.Info.Subject = "Orden de Compra";
            pdf.Info.Creator = "Sistema Pilaris";

            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont fontBold = new XFont("Arial", 9, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontSmall = new XFont("Arial", 7, XFontStyle.Regular);
            XFont fontTableHeader = new XFont("Arial", 8, XFontStyle.Bold);
            XFont fontTitle = new XFont("Arial", 12, XFontStyle.Bold);

            int margin = 30, x = margin, y = margin;
            int pageWidth = (int)page.Width - margin * 2;

            DibujarEncabezadoEmpresa(gfx, x, ref y, fontNormal, fontSmall, page, margin);
            y = margin + 75;

            gfx.DrawString("ORDEN DE COMPRA", fontTitle, XBrushes.Black, new XRect(margin, y, pageWidth, 20), XStringFormats.Center);
            y += 30;

            gfx.DrawString("Proveedor", fontNormal, XBrushes.Black, x, y);
            gfx.DrawLine(XPens.Black, x + 70, y + 5, x + 280, y + 5);
            gfx.DrawString(nombreProveedor, fontNormal, XBrushes.Black, x + 72, y);

            int folioX = (int)page.Width - margin - 180;
            gfx.DrawString("Folio", fontNormal, XBrushes.Black, folioX, y);
            gfx.DrawLine(XPens.Black, folioX + 35, y + 5, folioX + 180, y + 5);
            gfx.DrawString(folioOC, fontNormal, XBrushes.Black, folioX + 37, y);
            y += 20;

            gfx.DrawString("Codigo Prov", fontNormal, XBrushes.Black, x, y);
            gfx.DrawLine(XPens.Black, x + 70, y + 5, x + 280, y + 5);
            gfx.DrawString(codigoProveedor, fontNormal, XBrushes.Black, x + 72, y);

            gfx.DrawString("Fecha", fontNormal, XBrushes.Black, folioX, y);
            gfx.DrawLine(XPens.Black, folioX + 35, y + 5, folioX + 180, y + 5);
            gfx.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), fontNormal, XBrushes.Black, folioX + 37, y);
            y += 35;

            string[] headers = { "CODIGO", "INSUMO", "UNIDAD", "CANTIDAD", "PRECIO", "IMPORTE" };
            int[] widths = { 70, 200, 55, 70, 75, 80 };
            int totalTableWidth = 550, headerRowHeight = 20, dataRowHeight = 18;

            DibujarEncabezadoTabla(gfx, x, y, headers, widths, headerRowHeight, fontTableHeader);
            y += headerRowHeight;

            decimal total = 0;
            foreach (var insumo in insumos)
            {
                if (y + dataRowHeight > page.Height - 120)
                {
                    page = pdf.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                    DibujarEncabezadoTabla(gfx, x, y, headers, widths, headerRowHeight, fontTableHeader);
                    y += headerRowHeight;
                }

                int currentX = x;
                var rowRect = new XRect(x, y, totalTableWidth, dataRowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, rowRect);

                gfx.DrawString(insumo.Clave, fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[0] - 4, dataRowHeight - 4), XStringFormats.TopLeft);
                gfx.DrawLine(XPens.Black, currentX + widths[0], y, currentX + widths[0], y + dataRowHeight);
                currentX += widths[0];

                string descCorta = insumo.Descripcion;
                if (descCorta != null && descCorta.Length > 40) descCorta = descCorta.Substring(0, 37) + "...";
                gfx.DrawString(descCorta, fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[1] - 4, dataRowHeight - 4), XStringFormats.TopLeft);
                gfx.DrawLine(XPens.Black, currentX + widths[1], y, currentX + widths[1], y + dataRowHeight);
                currentX += widths[1];

                gfx.DrawString(insumo.Unidad, fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[2] - 4, dataRowHeight - 4), XStringFormats.TopCenter);
                gfx.DrawLine(XPens.Black, currentX + widths[2], y, currentX + widths[2], y + dataRowHeight);
                currentX += widths[2];

                gfx.DrawString(insumo.Cantidad.ToString("N2"), fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[3] - 4, dataRowHeight - 4), XStringFormats.TopCenter);
                gfx.DrawLine(XPens.Black, currentX + widths[3], y, currentX + widths[3], y + dataRowHeight);
                currentX += widths[3];

                gfx.DrawString(insumo.Precio.ToString("C2"), fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[4] - 4, dataRowHeight - 4), XStringFormats.TopRight);
                gfx.DrawLine(XPens.Black, currentX + widths[4], y, currentX + widths[4], y + dataRowHeight);
                currentX += widths[4];

                gfx.DrawString(insumo.Importe.ToString("C2"), fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[5] - 4, dataRowHeight - 4), XStringFormats.TopRight);

                y += dataRowHeight;
                total += insumo.Importe;
            }

            gfx.DrawLine(XPens.Black, x, y, x + totalTableWidth, y);
            y += 20;

            DibujarTotales(gfx, page, y, total, fontBold, fontNormal, margin);
            DibujarFirmas(gfx, page, fontSmall, margin);

            pdf.Save(rutaPdf);
            pdf.Close();

            if (abrirPdf) AbrirArchivo(rutaPdf);
            return rutaPdf;
        }

        /// <summary>Compra indirecta/administrativa (FormCompraIndirecta.GenerarPDF, sin cambios de layout).</summary>
        public static string GenerarIndirecta(string folioOC, List<InsumoPdf> insumos, string nombreProveedor, string claveProveedor, bool abrirPdf = true)
        {
            string rutaPdf = Path.Combine(CarpetaPdf(), $"OrdenCompraIndirecta_{folioOC}.pdf");

            var pdf = new PdfDocument();
            pdf.Info.Title = "Orden de Compra - " + folioOC;
            pdf.Info.Author = "Desarrolladora de Casas Camaney";
            pdf.Info.Subject = "Orden de Compra";
            pdf.Info.Creator = "Sistema Pilaris";

            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont fontTitle = new XFont("Arial", 14, XFontStyle.Bold);
            XFont fontBold = new XFont("Arial", 9, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontSmall = new XFont("Arial", 7, XFontStyle.Regular);
            XFont fontTableHeader = new XFont("Arial", 8, XFontStyle.Bold);

            int margin = 30, x = margin, y = margin;
            int pageWidth = (int)page.Width - margin * 2;

            gfx.DrawString("Desarrolladora de Casas Camaney", new XFont("Arial", 9, XFontStyle.Bold), XBrushes.Black, x, y);
            y += 12;
            gfx.DrawString("Blvd. Periférico sur. Y Carretera a la Colorada", fontSmall, XBrushes.Black, x, y);
            y += 10;
            gfx.DrawString("Tel. 662xxxxxxxxxx", fontSmall, XBrushes.Black, x, y);
            y += 10;
            gfx.DrawString("Email constcas@h.....com", fontSmall, XBrushes.Black, x, y);

            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    using (var logo = XImage.FromStream(ms))
                    {
                        int logoWidth = 120, logoHeight = 107; // proporción del logo Pilaris
                        int logoX = (int)page.Width - margin - logoWidth;
                        gfx.DrawImage(logo, logoX, margin - 5, logoWidth, logoHeight);
                    }
                }
            }
            catch { }

            y = margin + 85;

            gfx.DrawString("ORDEN DE COMPRA", fontTitle, XBrushes.Black, new XRect(margin, y, pageWidth, 20), XStringFormats.Center);
            y += 30;

            int folioBoxWidth = 180;
            int folioBoxX = (int)page.Width - margin - folioBoxWidth;
            int folioY = y;
            gfx.DrawString("Folio_________", fontNormal, XBrushes.Black, folioBoxX, folioY);
            gfx.DrawString(folioOC, fontNormal, XBrushes.Black, folioBoxX + 60, folioY);
            folioY += 15;
            gfx.DrawString("Fecha_________", fontNormal, XBrushes.Black, folioBoxX, folioY);
            gfx.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), fontNormal, XBrushes.Black, folioBoxX + 60, folioY);
            y += 45;

            int provY = y;
            gfx.DrawString("Proveedor_____________________________", fontNormal, XBrushes.Black, x, provY);
            gfx.DrawString(nombreProveedor, fontNormal, XBrushes.Black, x + 100, provY);
            provY += 15;
            gfx.DrawString("Codigo Prov___________________________", fontNormal, XBrushes.Black, x, provY);
            gfx.DrawString(claveProveedor, fontNormal, XBrushes.Black, x + 100, provY);
            y = provY + 30;

            string[] headers = { "CODIGO", "INSUMO", "UNIDAD", "CANTIDAD", "PRECIO", "IMPORTE" };
            int[] widths = { 70, 200, 55, 70, 75, 80 };
            int totalTableWidth = 550, headerRowHeight = 20, dataRowHeight = 18;

            DibujarEncabezadoTabla(gfx, x, y, headers, widths, headerRowHeight, fontTableHeader);
            y += headerRowHeight;

            decimal total = 0;
            foreach (var insumo in insumos)
            {
                if (y + dataRowHeight > page.Height - 120)
                {
                    page = pdf.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                    DibujarEncabezadoTabla(gfx, x, y, headers, widths, headerRowHeight, fontTableHeader);
                    y += headerRowHeight;
                }

                int currentX = x;
                var rowRect = new XRect(x, y, totalTableWidth, dataRowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, rowRect);

                gfx.DrawString(insumo.Clave, fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[0] - 4, dataRowHeight - 4), XStringFormats.TopLeft);
                gfx.DrawLine(XPens.Black, currentX + widths[0], y, currentX + widths[0], y + dataRowHeight);
                currentX += widths[0];

                string descCorta = insumo.Descripcion;
                if (descCorta != null && descCorta.Length > 40) descCorta = descCorta.Substring(0, 37) + "...";
                gfx.DrawString(descCorta, fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[1] - 4, dataRowHeight - 4), XStringFormats.TopLeft);
                gfx.DrawLine(XPens.Black, currentX + widths[1], y, currentX + widths[1], y + dataRowHeight);
                currentX += widths[1];

                gfx.DrawString(insumo.Unidad, fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[2] - 4, dataRowHeight - 4), XStringFormats.TopCenter);
                gfx.DrawLine(XPens.Black, currentX + widths[2], y, currentX + widths[2], y + dataRowHeight);
                currentX += widths[2];

                gfx.DrawString(insumo.Cantidad.ToString("N2"), fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[3] - 4, dataRowHeight - 4), XStringFormats.TopCenter);
                gfx.DrawLine(XPens.Black, currentX + widths[3], y, currentX + widths[3], y + dataRowHeight);
                currentX += widths[3];

                gfx.DrawString(insumo.Precio.ToString("C2"), fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[4] - 4, dataRowHeight - 4), XStringFormats.TopRight);
                gfx.DrawLine(XPens.Black, currentX + widths[4], y, currentX + widths[4], y + dataRowHeight);
                currentX += widths[4];

                gfx.DrawString(insumo.Importe.ToString("C2"), fontSmall, XBrushes.Black, new XRect(currentX + 2, y + 4, widths[5] - 4, dataRowHeight - 4), XStringFormats.TopRight);

                y += dataRowHeight;
                total += insumo.Importe;
            }

            gfx.DrawLine(XPens.Black, x, y, x + totalTableWidth, y);
            y += 20;

            DibujarTotales(gfx, page, y, total, fontBold, fontNormal, margin);
            DibujarFirmas(gfx, page, fontSmall, margin);

            pdf.Save(rutaPdf);
            pdf.Close();

            if (abrirPdf) AbrirArchivo(rutaPdf);
            return rutaPdf;
        }

        private static void DibujarTotales(XGraphics gfx, PdfPage page, int y, decimal subtotal, XFont fontBold, XFont fontNormal, int margin)
        {
            decimal iva = Math.Round(subtotal * 0.16m, 2);
            decimal totalConIVA = subtotal + iva;

            int totalesBoxWidth = 250;
            int totalesBoxX = (int)page.Width - margin - totalesBoxWidth;
            int labelWidth = 120, valueWidth = 130;

            gfx.DrawString("SUBTOTAL:", fontBold, XBrushes.Black, new XRect(totalesBoxX, y, labelWidth, 15), XStringFormats.CenterLeft);
            gfx.DrawString(subtotal.ToString("C2"), fontNormal, XBrushes.Black, new XRect(totalesBoxX + labelWidth, y, valueWidth, 15), XStringFormats.CenterRight);
            y += 18;

            gfx.DrawString("IVA (16%):", fontBold, XBrushes.Black, new XRect(totalesBoxX, y, labelWidth, 15), XStringFormats.CenterLeft);
            gfx.DrawString(iva.ToString("C2"), fontNormal, XBrushes.Black, new XRect(totalesBoxX + labelWidth, y, valueWidth, 15), XStringFormats.CenterRight);
            y += 18;

            gfx.DrawLine(XPens.Black, totalesBoxX, y, totalesBoxX + totalesBoxWidth, y);
            y += 5;

            var totalRect = new XRect(totalesBoxX, y, totalesBoxWidth, 22);
            gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, totalRect);
            gfx.DrawString("TOTAL:", fontBold, XBrushes.Black, new XRect(totalesBoxX + 5, y + 5, labelWidth - 5, 15), XStringFormats.CenterLeft);
            gfx.DrawString(totalConIVA.ToString("C2"), fontBold, XBrushes.Black, new XRect(totalesBoxX + labelWidth, y + 5, valueWidth - 5, 15), XStringFormats.CenterRight);
        }

        private static void DibujarFirmas(XGraphics gfx, PdfPage page, XFont fontSmall, int margin)
        {
            int y = (int)page.Height - margin - 70;
            int firmaWidth = 200, firma1X = margin + 50;
            gfx.DrawLine(XPens.Black, firma1X, y, firma1X + firmaWidth, y);
            y += 5;
            gfx.DrawString("Nombre y Firma", fontSmall, XBrushes.Black, new XRect(firma1X, y, firmaWidth, 15), XStringFormats.TopCenter);
            y += 12;
            gfx.DrawString("Encargado de Compras", fontSmall, XBrushes.Black, new XRect(firma1X, y, firmaWidth, 15), XStringFormats.TopCenter);
        }

        private static void AbrirArchivo(string ruta)
        {
            try { Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true }); }
            catch { try { Process.Start("explorer.exe", Path.GetDirectoryName(ruta)); } catch { } }
        }
    }
}
