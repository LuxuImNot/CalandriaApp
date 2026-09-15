using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Genera el PDF de resumen de facturación del add-on "Conciliación de
    /// factura con IA" (saldo consolidado por obra, o el historial de una obra
    /// puntual) para que FormFacturacionWeb lo archive en el repositorio vía API.
    /// Reporte simple (título/tabla/total), no lleva el membrete de la
    /// constructora: es información del proveedor del sistema, no del cliente.
    /// </summary>
    public static class FacturacionPdfService
    {
        private static string CarpetaPdf()
        {
            string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CALANDRIA RESIDENCIAL", "PDFFacturacionIA");
            Directory.CreateDirectory(carpeta);
            return carpeta;
        }

        /// <summary>
        /// Tabla genérica: encabezados/anchos definen las columnas (anchos deben
        /// sumar ~515pt, el ancho útil de una página A4 con márgenes de 40pt),
        /// alinearDerecha marca qué columnas son numéricas.
        /// </summary>
        public static string Generar(string nombreArchivo, string titulo, string subtitulo,
            string[] encabezados, int[] anchos, bool[] alinearDerecha, List<string[]> filas,
            string totalEtiqueta, string totalValor, string usuario, bool abrirPdf = true)
        {
            string rutaPdf = Path.Combine(CarpetaPdf(), nombreArchivo);

            var pdf = new PdfDocument();
            var page = pdf.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            const int margin = 40;
            int x = margin, y = margin;

            var fontEyebrow = new XFont("Arial", 8, XFontStyle.Regular);
            var fontTitle = new XFont("Arial", 15, XFontStyle.Bold);
            var fontSubtitle = new XFont("Arial", 9, XFontStyle.Regular);
            var fontHeader = new XFont("Arial", 8.5, XFontStyle.Bold);
            var fontCell = new XFont("Arial", 8.5, XFontStyle.Regular);
            var fontBold = new XFont("Arial", 10.5, XFontStyle.Bold);
            var fontFooter = new XFont("Arial", 7, XFontStyle.Regular);

            gfx.DrawString("CALANDRIA · FACTURACIÓN IA", fontEyebrow, XBrushes.Gray, x, y); y += 16;
            gfx.DrawString(titulo, fontTitle, XBrushes.Black, x, y); y += 20;
            gfx.DrawString(subtitulo, fontSubtitle, XBrushes.Black, x, y); y += 22;

            const int headerRowHeight = 20, dataRowHeight = 18;

            void DibujarEncabezado()
            {
                int cx = x;
                for (int i = 0; i < encabezados.Length; i++)
                {
                    var rect = new XRect(cx, y, anchos[i], headerRowHeight);
                    gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, rect);
                    gfx.DrawString(encabezados[i], fontHeader, XBrushes.Black, rect, XStringFormats.Center);
                    cx += anchos[i];
                }
                y += headerRowHeight;
            }

            DibujarEncabezado();

            foreach (var fila in filas)
            {
                if (y + dataRowHeight > page.Height - margin - 50)
                {
                    page = pdf.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                    DibujarEncabezado();
                }

                int cx = x;
                for (int i = 0; i < fila.Length; i++)
                {
                    var rect = new XRect(cx, y, anchos[i], dataRowHeight);
                    gfx.DrawRectangle(XPens.Black, XBrushes.White, rect);
                    var celda = new XRect(cx + 4, y, anchos[i] - 8, dataRowHeight);
                    var alineacion = alinearDerecha != null && i < alinearDerecha.Length && alinearDerecha[i]
                        ? XStringFormats.CenterRight : XStringFormats.CenterLeft;
                    gfx.DrawString(fila[i] ?? "", fontCell, XBrushes.Black, celda, alineacion);
                    cx += anchos[i];
                }
                y += dataRowHeight;
            }

            y += 16;
            gfx.DrawString($"{totalEtiqueta}: {totalValor}", fontBold, XBrushes.Black, x, y);

            int footerY = (int)page.Height - margin - 14;
            gfx.DrawString($"Generado por {usuario} el {DateTime.Now:dd/MM/yyyy HH:mm}", fontFooter, XBrushes.Gray, x, footerY);

            pdf.Save(rutaPdf);
            pdf.Close();

            if (abrirPdf) AbrirArchivo(rutaPdf);
            return rutaPdf;
        }

        private static void AbrirArchivo(string ruta)
        {
            try { Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true }); }
            catch { try { Process.Start("explorer.exe", Path.GetDirectoryName(ruta)); } catch { } }
        }
    }
}
