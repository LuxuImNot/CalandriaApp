using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    internal class GenerarExplosionInsumosPDF
    {
        // Store rutas criticas passed from caller (e.g., PanelPrincipal)
        private readonly Dictionary<string, List<PanelPrincipal.TareaRutaCritica>> rutasCriticasPorModelo;

        public GenerarExplosionInsumosPDF(Dictionary<string, List<PanelPrincipal.TareaRutaCritica>> rutas = null)
        {
            rutasCriticasPorModelo = rutas ?? new Dictionary<string, List<PanelPrincipal.TareaRutaCritica>>();
        }

        private void GenerarExplosionPDF(string modelo, string rutaArchivoPDF)
        {
            // Normaliza la clave del modelo
            string claveRuta = null;
            string modeloUpper = modelo.Trim().ToUpper();
            if (modeloUpper.Contains("TUNERA"))
                claveRuta = "TUNERA";
            else if (modeloUpper.Contains("CALANDRA"))
                claveRuta = "CALANDRA";
            else
                claveRuta = modeloUpper;

            if (!rutasCriticasPorModelo.ContainsKey(claveRuta))
            {
                MessageBox.Show($"No hay ruta crítica para el modelo: {modelo}");
                return;
            }
            var tareas = FlattenTareas(rutasCriticasPorModelo[claveRuta]);
            var insumosPorDestajo = new List<(string Destajo, string Insumo, int Cantidad)>();

            foreach (var tarea in tareas)
            {
                if (tarea.Detalles == null || tarea.Detalles.Count == 0) continue;
                foreach (var insumo in tarea.Detalles)
                {
                    var partes = insumo.Split(':');
                    string nombreInsumo = partes[0].Trim();
                    int cantidad = 1;
                    if (partes.Length > 1) int.TryParse(partes[1], out cantidad);
                    insumosPorDestajo.Add((tarea.Nombre, nombreInsumo, cantidad));
                }
            }

            PdfDocument pdf = new PdfDocument();
            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 10, XFontStyle.Regular);
            XFont bold = new XFont("Arial", 10, XFontStyle.Bold);
            XFont titleFont = new XFont("Arial", 14, XFontStyle.Bold);

            int x = 40;
            int y = 40;

            // ENCABEZADO
            gfx.DrawString("Desarrolladora de Casas Camaney", bold, XBrushes.Black, x, y); y += 15;
            gfx.DrawString("Blvd. Periférico sur. Y Carretera a la Colorada", font, XBrushes.Black, x, y); y += 15;
            gfx.DrawString("Tel. 662xxxxxxxxxx", font, XBrushes.Black, x, y); y += 15;
            gfx.DrawString("Email constcas@h......com", font, XBrushes.Black, x, y); y += 30;

            // LOGO
            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var logo = XImage.FromStream(ms);
                    gfx.DrawImage(logo, page.Width - 150, 20, 100, 50);
                }
            }
            catch
            {
                // ignore missing resource
            }


            // TÍTULO Y DATOS DE REPORTE
            gfx.DrawString("EXPLOSIÓN DE INSUMOS POR DESTAJO", titleFont, XBrushes.Black, new XRect(x, y, page.Width - 2 * x, 30), XStringFormats.Center); y += 40;
            gfx.DrawString($"Modelo: " + modelo, font, XBrushes.Black, x + 350, y); y += 20;
            gfx.DrawString($"Fecha: " + DateTime.Now.ToShortDateString(), font, XBrushes.Black, x, y); y += 30;

            // CABECERA DE TABLA
            string[] headers = { "DESTAJO", "INSUMO", "CANTIDAD" };
            int[] widths = { 200, 200, 80 };
            int tableX = x;
            int rowHeight = 20;

            int colDestajo = tableX;
            int colInsumo = colDestajo + widths[0];
            int colCantidad = colInsumo + widths[1];

            // Dibuja encabezados de tabla
            for (int i = 0, col = tableX; i < headers.Length; i++)
            {
                gfx.DrawRectangle(XPens.Black, col, y, widths[i], rowHeight);
                gfx.DrawString(headers[i], bold, XBrushes.Black, new XRect(col, y, widths[i], rowHeight), XStringFormats.Center);
                col += widths[i];
            }
            y += rowHeight;

            // CONTENIDO DE TABLA
            foreach (var item in insumosPorDestajo)
            {
                int alto = rowHeight;
                gfx.DrawRectangle(XPens.Black, colDestajo, y, widths[0], alto);
                gfx.DrawString(item.Destajo, font, XBrushes.Black, new XRect(colDestajo, y, widths[0], alto), XStringFormats.TopLeft);

                gfx.DrawRectangle(XPens.Black, colInsumo, y, widths[1], alto);
                gfx.DrawString(item.Insumo, font, XBrushes.Black, new XRect(colInsumo, y, widths[1], alto), XStringFormats.TopLeft);

                gfx.DrawRectangle(XPens.Black, colCantidad, y, widths[2], alto);
                gfx.DrawString(item.Cantidad.ToString(), font, XBrushes.Black, new XRect(colCantidad, y, widths[2], alto), XStringFormats.TopLeft);

                y += alto;
                if (y > page.Height - 100)
                {
                    page = pdf.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            y += 10;
            gfx.DrawString("Nombre y Firma", font, XBrushes.Black, tableX, y);
            gfx.DrawString("Encargado de Compras", font, XBrushes.Black, tableX, y + 15);

            pdf.Save(rutaArchivoPDF);
            MessageBox.Show($"Reporte PDF generado en: {rutaArchivoPDF}");
        }

        // Helper to flatten tarea hierarchy
        public List<PanelPrincipal.TareaRutaCritica> FlattenTareas(List<PanelPrincipal.TareaRutaCritica> tareas)
        {
            var flat = new List<PanelPrincipal.TareaRutaCritica>();
            void Rec(PanelPrincipal.TareaRutaCritica t)
            {
                flat.Add(t);
                foreach (var h in t.Hijos) Rec(h);
            }
            foreach (var t in tareas) Rec(t);
            return flat;
        }
    }
}
