using System;
using System.Linq;
using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Extensión para rediseño del PDF de estimación
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        private void DibujarPDFNuevoFormato(XGraphics g, PdfPage pg, bool prev)
        {
            var naranja = new XSolidBrush(XColor.FromArgb(230, 126, 34));
            var gris = new XSolidBrush(XColor.FromArgb(236, 240, 241));
            var pen = new XPen(XColor.FromArgb(189, 195, 199), 0.5);

            var fTit = new XFont("Arial", 10, XFontStyle.Bold);
            var fNor = new XFont("Arial", 8, XFontStyle.Regular);
            var fPeq = new XFont("Arial", 7, XFontStyle.Regular);

            double m = 50, w = pg.Width - 100, y = 40;

            // Barra naranja
            g.DrawRectangle(naranja, m, y, w, 4);
            y += 10;

            // FOLIO en la esquina superior derecha (formato simplificado)
            var manzana = cmbManzana.SelectedItem?.ToString() ?? "0";
            var lote = cmbLote.SelectedItem?.ToString() ?? "0";
            var numEst = ObtenerSiguienteNumeroEstimacion(manzana, lote);
            var folio = GenerarFolio(manzana, lote, numEst);
            
            var folioSize = g.MeasureString(folio, fTit);
            g.DrawRectangle(pen, gris, m + w - 180, y, 180, 16);
            g.DrawString("FOLIO:", fPeq, XBrushes.Black, m + w - 175, y + 11);
            g.DrawString(folio, fNor, XBrushes.Black, m + w - folioSize.Width - 5, y + 11);
            y += 18;

            // MODIFICADO: UBICACIÓN (Manzana/Lote) + FECHA en una sola fila
            double c1 = w * 0.30, c2 = w * 0.25, c3 = w * 0.20, c4 = w * 0.25;

            // Encabezados
            g.DrawRectangle(gris, m, y, c1, 16);
            g.DrawRectangle(pen, gris, m, y, c1, 16);
            g.DrawString("UBICACIÓN:", fPeq, XBrushes.Black, m + 3, y + 11);

            g.DrawRectangle(gris, m + c1, y, c2, 16);
            g.DrawRectangle(pen, gris, m + c1, y, c2, 16);
            g.DrawString("PROVEEDOR:", fPeq, XBrushes.Black, m + c1 + 3, y + 11);

            g.DrawRectangle(gris, m + c1 + c2, y, c3, 16);
            g.DrawRectangle(pen, gris, m + c1 + c2, y, c3, 16);
            g.DrawString("FECHA:", fPeq, XBrushes.Black, m + c1 + c2 + 3, y + 11);

            g.DrawRectangle(gris, m + c1 + c2 + c3, y, c4, 16);
            g.DrawRectangle(pen, gris, m + c1 + c2 + c3, y, c4, 16);
            g.DrawString("ESTIMACIÓN No.:", fPeq, XBrushes.Black, m + c1 + c2 + c3 + 3, y + 11);
            y += 16;

            // Valores
            var ubicacion = $"M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem}";
            g.DrawRectangle(pen, XBrushes.White, m, y, c1, 16);
            g.DrawString(ubicacion, fNor, XBrushes.Black, m + 3, y + 11);

            var prov = string.IsNullOrWhiteSpace(txtProveedor.Text) ? "No especificado" : txtProveedor.Text;
            g.DrawRectangle(pen, XBrushes.White, m + c1, y, c2, 16);
            g.DrawString(prov, fNor, XBrushes.Black, m + c1 + 3, y + 11);

            g.DrawRectangle(pen, XBrushes.White, m + c1 + c2, y, c3, 16);
            g.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), fNor, XBrushes.Black, m + c1 + c2 + 3, y + 11);

            g.DrawRectangle(pen, XBrushes.White, m + c1 + c2 + c3, y, c4, 16);
            g.DrawString(numEst.ToString(), fNor, XBrushes.Black, m + c1 + c2 + c3 + 3, y + 11);
            y += 18;

            // DESCRIPCIÓN
            g.DrawRectangle(gris, m, y, w, 16);
            g.DrawRectangle(pen, gris, m, y, w, 16);
            g.DrawString("DESCRIPCIÓN:", fPeq, XBrushes.Black, m + 3, y + 11);
            y += 16;

            var desc = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? "No especificado" : txtDescripcion.Text;
            g.DrawRectangle(pen, XBrushes.White, m, y, w, 16);
            g.DrawString(desc, fNor, XBrushes.Black, m + 3, y + 11);
            y += 18;

            // CARGAR A
            g.DrawRectangle(gris, m, y, w, 16);
            g.DrawRectangle(pen, gris, m, y, w, 16);
            g.DrawString("CARGAR A:", fPeq, XBrushes.Black, m + 3, y + 11);
            y += 16;

            var cargA = $"M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem} Prototipo {prototipoActual}";
            g.DrawRectangle(pen, XBrushes.White, m, y, w, 16);
            g.DrawString(cargA, fNor, XBrushes.Black, m + 3, y + 11);
            y += 18;

            // IMPORTE
            var totPres = nodosRaiz.Sum(i => i.Total);
            
            g.DrawRectangle(gris, m, y, w, 16);
            g.DrawRectangle(pen, gris, m, y, w, 16);
            g.DrawString("IMPORTE DEL CONTRATO:", fPeq, XBrushes.Black, m + 3, y + 11);
            y += 16;

            g.DrawRectangle(pen, XBrushes.White, m, y, w, 16);
            g.DrawString(totPres.ToString("C2"), fTit, XBrushes.Black, m + 3, y + 11);
            y += 18;

            // ANTICIPO
            var totEjec = nodosRaiz.Where(c => c.Partidas.Any(p => p.Incluir))
                .SelectMany(c => c.Partidas.Where(p => p.Incluir))
                .Sum(p => p.Total);

            var porcAmort = prototipoActual.ToUpper().Contains("TUNERA") ? 0.11 : 0.095;
            var amort = totEjec * porcAmort;
            var porcTxt = prototipoActual.ToUpper().Contains("TUNERA") ? "11.0" : "9.5";

            double a1 = w * 0.15, a2 = w * 0.35, a3 = w * 0.15, a4 = w * 0.35;

            g.DrawRectangle(gris, m, y, a1, 16);
            g.DrawRectangle(pen, gris, m, y, a1, 16);
            g.DrawString("ANTICIPO:", fPeq, XBrushes.Black, m + 3, y + 11);

            g.DrawRectangle(pen, XBrushes.White, m + a1, y, a2, 16);
            g.DrawString(amort.ToString("C2"), fNor, XBrushes.Black, m + a1 + 3, y + 11);

            g.DrawRectangle(gris, m + a1 + a2, y, a3, 16);
            g.DrawRectangle(pen, gris, m + a1 + a2, y, a3, 16);
            g.DrawString("ANTICIPO %:", fPeq, XBrushes.Black, m + a1 + a2 + 3, y + 11);

            g.DrawRectangle(pen, XBrushes.White, m + a1 + a2 + a3, y, a4, 16);
            g.DrawString(porcTxt, fNor, XBrushes.Black, m + a1 + a2 + a3 + 3, y + 11);
            y += 20;

            // CONCEPTOS
            g.DrawRectangle(naranja, m, y, w, 16);
            g.DrawString("CONCEPTOS A ESTIMAR", fTit, XBrushes.White, m + 3, y + 11);
            y += 18;

            // Tabla con 3 columnas: Concepto/Partida | m² | Monto
            double cC = w * 0.60;  // Concepto/Partida - 60%
            double cM2 = w * 0.15; // m² - 15%
            double cMonto = w * 0.25; // Monto - 25%

            // Encabezados de tabla
            g.DrawRectangle(gris, m, y, cC, 14);
            g.DrawRectangle(pen, gris, m, y, cC, 14);
            g.DrawString("Concepto / Partida", fPeq, XBrushes.Black, m + 3, y + 10);

            g.DrawRectangle(gris, m + cC, y, cM2, 14);
            g.DrawRectangle(pen, gris, m + cC, y, cM2, 14);
            g.DrawString("m²", fPeq, XBrushes.Black, m + cC + 3, y + 10);

            g.DrawRectangle(gris, m + cC + cM2, y, cMonto, 14);
            g.DrawRectangle(pen, gris, m + cC + cM2, y, cMonto, 14);
            g.DrawString("Monto", fPeq, XBrushes.Black, m + cC + cM2 + 3, y + 10);
            y += 14;

            // Partidas
            foreach (var con in nodosRaiz)
            {
                var parts = con.Partidas.Where(p => p.Incluir).ToList();
                if (parts.Count > 0)
                {
                    // Encabezado de concepto
                    g.DrawRectangle(naranja, m, y, w, 13);
                    g.DrawString(con.Nombre, fNor, XBrushes.White, m + 3, y + 9);
                    y += 13;

                    foreach (var par in parts)
                    {
                        // Columna 1: Concepto/Partida
                        g.DrawRectangle(pen, XBrushes.White, m, y, cC, 11);
                        var nom = par.Nombre.Length > 65 ? par.Nombre.Substring(0, 62) + "..." : par.Nombre;
                        g.DrawString($"  {con.Nombre} >  {nom}", fPeq, XBrushes.Black, m + 3, y + 8);
                        
                        // Columna 2: m² (solo si es dinámica y tiene m²)
                        g.DrawRectangle(pen, XBrushes.White, m + cC, y, cM2, 11);
                        if (par.EsDinamica && par.MetrosCuadrados > 0)
                        {
                            string m2Text = par.MetrosCuadrados.ToString("N2");
                            var szM2 = g.MeasureString(m2Text, fPeq);
                            g.DrawString(m2Text, fPeq, XBrushes.Black, 
                                m + cC + (cM2 / 2) - (szM2.Width / 2), y + 8);
                        }
                        else
                        {
                            g.DrawString("-", fPeq, XBrushes.Black, 
                                m + cC + (cM2 / 2) - 3, y + 8);
                        }
                        
                        // Columna 3: Monto
                        g.DrawRectangle(pen, XBrushes.White, m + cC + cM2, y, cMonto, 11);
                        var sz = g.MeasureString(par.Total.ToString("C2"), fPeq);
                        g.DrawString(par.Total.ToString("C2"), fPeq, XBrushes.Black, 
                            m + cC + cM2 + cMonto - sz.Width - 3, y + 8);
                        
                        y += 11;

                        if (y > pg.Height - 250 && !prev) // Más espacio para firmas
                        {
                            pg = g.PdfPage.Owner.AddPage();
                            pg.Size = PdfSharp.PageSize.Letter;
                            g = XGraphics.FromPdfPage(pg);
                            y = 40;
                        }
                    }

                    // Total del concepto
                    var tc = parts.Sum(p => p.Total);
                    var totalM2Concepto = parts.Where(p => p.EsDinamica).Sum(p => p.MetrosCuadrados);
                    
                    g.DrawRectangle(naranja, m, y, cC, 14);
                    g.DrawString($"Total {con.Nombre}", fNor, XBrushes.White, m + 3, y + 10);
                    
                    g.DrawRectangle(naranja, m + cC, y, cM2, 14);
                    if (totalM2Concepto > 0)
                    {
                        string m2Text = totalM2Concepto.ToString("N2");
                        var szM2T = g.MeasureString(m2Text, fNor);
                        g.DrawString(m2Text, fNor, XBrushes.White, 
                            m + cC + (cM2 / 2) - (szM2T.Width / 2), y + 10);
                    }
                    
                    g.DrawRectangle(naranja, m + cC + cM2, y, cMonto, 14);
                    var szT = g.MeasureString(tc.ToString("C2"), fNor);
                    g.DrawString(tc.ToString("C2"), fNor, XBrushes.White, 
                        m + cC + cM2 + cMonto - szT.Width - 3, y + 10);
                    y += 16;
                }
            }

            // Totales con mejor espaciado y alineación
            y += 10;

            double cL = w * 0.55, cV = w * 0.45;

            // Total de Requisición
            g.DrawRectangle(gris, m + cL, y, cV, 16);
            g.DrawRectangle(pen, gris, m + cL, y, cV, 16);
            g.DrawString("TOTAL REQUISICIÓN", fPeq, XBrushes.Black, m + cL + 5, y + 11);

            var szR = g.MeasureString(totEjec.ToString("C2"), fNor);
            g.DrawString(totEjec.ToString("C2"), fNor, XBrushes.Black, 
                m + w - szR.Width - 5, y + 11);
            y += 16;

            // Amortización
            g.DrawRectangle(gris, m + cL, y, cV, 16);
            g.DrawRectangle(pen, gris, m + cL, y, cV, 16);
            g.DrawString($"AMORTIZACIÓN {porcTxt}%", fPeq, XBrushes.Black, m + cL + 5, y + 11);

            var szA = g.MeasureString(amort.ToString("C2"), fNor);
            g.DrawString(amort.ToString("C2"), fNor, XBrushes.Black, 
                m + w - szA.Width - 5, y + 11);
            y += 16;

            // Total Estimación - Barra naranja más alta y con mejor espaciado
            var totEst = totEjec - amort;
            g.DrawRectangle(naranja, m + cL, y, cV, 20);
            g.DrawString("TOTAL ESTIMACIÓN", fTit, XBrushes.White, m + cL + 5, y + 14);

            var szE = g.MeasureString(totEst.ToString("C2"), fTit);
            g.DrawString(totEst.ToString("C2"), fTit, XBrushes.White, 
                m + w - szE.Width - 5, y + 14);
            y += 30;

            // Firmas más abajo - calcular posición dinámica
            double espacioRestante = pg.Height - y - 50;
            
            if (espacioRestante > 100)
            {
                y = pg.Height - 120;
            }
            else
            {
                y += 20;
            }

            // SECCIÓN DE FIRMAS
            DibujarFirmasPDF(g, pg, m, w, ref y);
        }

        /// <summary>
        /// Dibuja la sección de firmas en el PDF
        /// </summary>
        private void DibujarFirmasPDF(XGraphics g, PdfPage pg, double m, double w, ref double y)
        {
            var fNor = new XFont("Arial", 9, XFontStyle.Regular);
            var fPeq = new XFont("Arial", 8, XFontStyle.Regular);
            var penFirma = new XPen(XColor.FromArgb(52, 73, 94), 1);

            // Ancho de cada firma (dividir en 3)
            double firmaWidth = (w - 40) / 3; // 40 = espacio entre firmas
            double firma1X = m;
            double firma2X = m + firmaWidth + 20;
            double firma3X = m + (firmaWidth + 20) * 2;

            // Obtener valores de las firmas (con valores por defecto)
            string nombreFirma1 = "ING. J. Rafael Monroy Díaz";
            string puestoFirma1 = "Dir. De Proyecto";

            string nombreFirma2 = "Ing. Carlos Tabardillo Herrera";
            string puestoFirma2 = "Gerente de Obra";

            string nombreFirma3 = string.IsNullOrWhiteSpace(txtProveedor.Text) 
                ? "Ignacio Durán León" 
                : txtProveedor.Text;
            string puestoFirma3 = "Subcontratista";

            // Dibujar Firma 1
            g.DrawLine(penFirma, firma1X, y, firma1X + firmaWidth, y);
            g.DrawString(nombreFirma1, fNor, XBrushes.Black, 
                new XRect(firma1X, y + 5, firmaWidth, 15), XStringFormats.TopCenter);
            g.DrawString(puestoFirma1, fPeq, XBrushes.DarkGray, 
                new XRect(firma1X, y + 20, firmaWidth, 15), XStringFormats.TopCenter);

            // Dibujar Firma 2
            g.DrawLine(penFirma, firma2X, y, firma2X + firmaWidth, y);
            g.DrawString(nombreFirma2, fNor, XBrushes.Black, 
                new XRect(firma2X, y + 5, firmaWidth, 15), XStringFormats.TopCenter);
            g.DrawString(puestoFirma2, fPeq, XBrushes.DarkGray, 
                new XRect(firma2X, y + 20, firmaWidth, 15), XStringFormats.TopCenter);

            // Dibujar Firma 3 (Subcontratista)
            g.DrawLine(penFirma, firma3X, y, firma3X + firmaWidth, y);
            g.DrawString(nombreFirma3, fNor, XBrushes.Black, 
                new XRect(firma3X, y + 5, firmaWidth, 15), XStringFormats.TopCenter);
            g.DrawString(puestoFirma3, fPeq, XBrushes.DarkGray, 
                new XRect(firma3X, y + 20, firmaWidth, 15), XStringFormats.TopCenter);

            y += 40; // Ajustar posición después de las firmas
        }
    }
}
