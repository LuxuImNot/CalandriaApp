using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase parcial que contiene la funcionalidad de vista previa del PDF
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        private void OnDatosChanged(object sender, EventArgs e)
        {
            timerPreview.Stop();
            timerPreview.Start();
        }

        private void TimerPreview_Tick(object sender, EventArgs e)
        {
            timerPreview.Stop();
            ActualizarPreviewPDF();
        }

        private void ActualizarPreviewPDF()
        {
            try
            {
                if (nodosRaiz == null || nodosRaiz.Count == 0 || 
                    cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
                {
                    pictureBoxPreviewPDF.Image = null;
                    return;
                }

                using (var ms = new MemoryStream())
                {
                    GenerarPDFPreview(ms);
                    ms.Position = 0;
                    
                    var preview = GenerarImagenDesdePDFReal(ms);
                    
                    if (pictureBoxPreviewPDF.Image != null)
                    {
                        var oldImage = pictureBoxPreviewPDF.Image;
                        pictureBoxPreviewPDF.Image = preview;
                        oldImage.Dispose();
                    }
                    else
                    {
                        pictureBoxPreviewPDF.Image = preview;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en preview: {ex.Message}");
            }
        }

        private void GenerarPDFPreview(MemoryStream ms)
        {
            PdfDocument pdf = new PdfDocument();
            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            DibujarContenidoPDF(gfx, page, true);

            pdf.Save(ms, false);
        }

        private Bitmap GenerarImagenDesdePDFReal(MemoryStream pdfStream)
        {
            int width = 850;
            int height = 1100;
            
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                
                DibujarContenidoPreview(g, width, height);
            }
            
            return bitmap;
        }

        private void DibujarContenidoPreview(Graphics g, int width, int height)
        {
            Color naranjaOscuro = Color.FromArgb(230, 126, 34);
            Color naranjaClaro = Color.FromArgb(255, 160, 60);
            Color grisClaro = Color.FromArgb(236, 240, 241);
            Color verdeTerminado = Color.FromArgb(39, 174, 96);
            
            using (var brushNaranja = new SolidBrush(naranjaOscuro))
            using (var brushNaranjaClaro = new SolidBrush(naranjaClaro))
            using (var brushGris = new SolidBrush(grisClaro))
            using (var brushVerde = new SolidBrush(verdeTerminado))
            using (var fontTitulo = new Font("Arial", 12, FontStyle.Bold))
            using (var fontNormal = new Font("Arial", 10))
            using (var fontPequena = new Font("Arial", 9))
            using (var fontMini = new Font("Arial", 8))
            using (var penLinea = new Pen(Color.FromArgb(189, 195, 199), 1))
            {
                int margen = 50;
                int anchoUtil = width - (margen * 2);
                float y = 40;
                
                // Barra superior naranja
                g.FillRectangle(brushNaranja, margen, y, anchoUtil, 6);
                y += 15;
                
                // FOLIO (formato simplificado global)
                var manzana = cmbManzana.SelectedItem?.ToString() ?? "0";
                var lote = cmbLote.SelectedItem?.ToString() ?? "0";
                var numEst = ObtenerSiguienteNumeroEstimacion(manzana, lote);
                var folio = GenerarFolio(manzana, lote, numEst);
                
                g.FillRectangle(brushGris, margen + anchoUtil - 200, (int)y, 200, 18);
                g.DrawRectangle(penLinea, margen + anchoUtil - 200, (int)y, 200, 18);
                g.DrawString("FOLIO:", fontPequena, Brushes.Black, margen + anchoUtil - 195, y + 4);
                var folioSize = g.MeasureString(folio, fontNormal);
                g.DrawString(folio, fontNormal, Brushes.Black, margen + anchoUtil - folioSize.Width - 8, y + 4);
                y += 22;
                
                // MODIFICADO: Primera fila con Ubicación, Proveedor, Fecha, Est. No.
                float colUbicacion = anchoUtil * 0.20f;
                float colProveedor = anchoUtil * 0.35f;
                float colFecha = anchoUtil * 0.20f;
                float colEstNo = anchoUtil * 0.25f;
                
                // Encabezados
                g.FillRectangle(brushGris, margen, y, colUbicacion, 18);
                g.DrawString("UBICACIÓN:", fontPequena, Brushes.Black, margen + 5, y + 4);
                
                g.FillRectangle(brushGris, margen + colUbicacion, y, colProveedor, 18);
                g.DrawString("PROVEEDOR:", fontPequena, Brushes.Black, margen + colUbicacion + 5, y + 4);
                
                g.FillRectangle(brushGris, margen + colUbicacion + colProveedor, y, colFecha, 18);
                g.DrawString("FECHA:", fontPequena, Brushes.Black, margen + colUbicacion + colProveedor + 5, y + 4);
                
                g.FillRectangle(brushGris, margen + colUbicacion + colProveedor + colFecha, y, colEstNo, 18);
                g.DrawString("EST. No.:", fontPequena, Brushes.Black, margen + colUbicacion + colProveedor + colFecha + 5, y + 4);
                y += 20;
                
                // Valores
                string ubicacion = $"M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem}";
                g.FillRectangle(Brushes.White, margen, y, colUbicacion, 18);
                g.DrawRectangle(penLinea, margen, y, colUbicacion, 18);
                g.DrawString(ubicacion, fontPequena, Brushes.Black, margen + 5, y + 4);
                
                string proveedor = !string.IsNullOrWhiteSpace(txtProveedor.Text) ? txtProveedor.Text : "No especificado";
                g.FillRectangle(Brushes.White, margen + colUbicacion, y, colProveedor, 18);
                g.DrawRectangle(penLinea, margen + colUbicacion, y, colProveedor, 18);
                g.DrawString(proveedor, fontPequena, Brushes.Black, margen + colUbicacion + 5, y + 4);
                
                g.FillRectangle(Brushes.White, margen + colUbicacion + colProveedor, y, colFecha, 18);
                g.DrawRectangle(penLinea, margen + colUbicacion + colProveedor, y, colFecha, 18);
                g.DrawString(DateTime.Now.ToString("dd/MM/yy"), fontPequena, Brushes.Black, margen + colUbicacion + colProveedor + 5, y + 4);
                
                g.FillRectangle(Brushes.White, margen + colUbicacion + colProveedor + colFecha, y, colEstNo, 18);
                g.DrawRectangle(penLinea, margen + colUbicacion + colProveedor + colFecha, y, colEstNo, 18);
                g.DrawString(numEst.ToString(), fontPequena, Brushes.Black, margen + colUbicacion + colProveedor + colFecha + 5, y + 4);
                y += 22;
                
                // DESCRIPCIÓN
                g.FillRectangle(brushGris, margen, y, anchoUtil, 18);
                g.DrawString("DESCRIPCION:", fontPequena, Brushes.Black, margen + 5, y + 4);
                y += 20;
                g.FillRectangle(Brushes.White, margen, y, anchoUtil, 18);
                g.DrawRectangle(penLinea, margen, y, anchoUtil, 18);
                string descripcion = !string.IsNullOrWhiteSpace(txtDescripcion.Text) ? txtDescripcion.Text : "No especificado";
                g.DrawString(descripcion, fontPequena, Brushes.Black, margen + 5, y + 4);
                
                // CARGAR A
                y += 22;
                g.FillRectangle(brushGris, margen, y, anchoUtil, 18);
                g.DrawString("CARGAR A:", fontPequena, Brushes.Black, margen + 5, y + 4);
                y += 20;
                g.FillRectangle(Brushes.White, margen, y, anchoUtil, 18);
                g.DrawRectangle(penLinea, margen, y, anchoUtil, 18);
                string cargarA = (cmbManzana.SelectedItem != null && cmbLote.SelectedItem != null) 
                    ? $"M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem} Prototipo {prototipoActual}"
                    : "No seleccionado";
                g.DrawString(cargarA, fontPequena, Brushes.Black, margen + 5, y + 4);
                
                y += 30;
                
                // CONCEPTOS A ESTIMAR
                g.FillRectangle(brushNaranja, margen, y, anchoUtil, 24);
                g.DrawString("CONCEPTOS A ESTIMAR", fontTitulo, Brushes.White, margen + 8, y + 6);
                y += 28;
                
                // Encabezados de tabla
                g.FillRectangle(brushGris, margen, y, anchoUtil * 0.60f, 20);
                g.FillRectangle(brushGris, margen + anchoUtil * 0.60f, y, anchoUtil * 0.40f, 20);
                g.DrawString("Concepto / Partida", fontNormal, Brushes.Black, margen + 5, y + 5);
                g.DrawString("Monto", fontNormal, Brushes.Black, margen + anchoUtil * 0.63f, y + 5);
                y += 22;
                
                if (nodosRaiz == null || nodosRaiz.Count == 0)
                {
                    g.DrawString("No hay datos para mostrar", fontNormal, Brushes.Gray, margen + anchoUtil / 3, y + 50);
                    return;
                }
                
                bool colorAlternado = false;
                int maxPartidas = 4;
                
                foreach (var concepto in nodosRaiz)
                {
                    var partidasSelec = concepto.Partidas.Where(p => p.Incluir).ToList();
                    if (partidasSelec.Count > 0)
                    {
                        if (y > height - 200) break;
                        
                        // Encabezado del concepto
                        g.FillRectangle(brushNaranjaClaro, margen, y, anchoUtil, 16);
                        string nombreCorto = concepto.Nombre.Length > 40 ? concepto.Nombre.Substring(0, 37) + "..." : concepto.Nombre;
                        g.DrawString(nombreCorto, fontNormal, Brushes.White, margen + 5, y + 3);
                        y += 18;
                        
                        // Partidas
                        foreach (var partida in partidasSelec.Take(maxPartidas))
                        {
                            var fondoFila = colorAlternado ? brushGris : Brushes.White;
                            g.FillRectangle(fondoFila, margen, y, anchoUtil, 15);
                            g.DrawLine(penLinea, margen, y + 15, margen + anchoUtil, y + 15);
                            
                            string nombrePartida = partida.Nombre.Length > 40 ? partida.Nombre.Substring(0, 37) + "..." : partida.Nombre;
                            var colorTexto = partida.Completado ? Brushes.Gray : Brushes.Black;
                            g.DrawString("  " + nombrePartida, fontPequena, colorTexto, margen + 8, y + 2);
                            
                            g.DrawString(partida.Total.ToString("C0"), fontPequena, colorTexto, margen + anchoUtil * 0.63f, y + 2);
                            
                            if (partida.Completado)
                            {
                                g.DrawString("TERMINADO", fontMini, brushVerde, margen + anchoUtil * 0.70f, y + 3);
                            }
                            
                            y += 15;
                            colorAlternado = !colorAlternado;
                        }
                        
                        if (partidasSelec.Count > maxPartidas)
                        {
                            g.FillRectangle(Brushes.LightGray, margen, y, anchoUtil, 14);
                            g.DrawString($"  ... y {partidasSelec.Count - maxPartidas} partida(s) más", fontMini, Brushes.DarkGray, margen + 8, y + 2);
                            y += 16;
                        }
                        
                        // Total del concepto
                        double totalConcepto = partidasSelec.Sum(p => p.Total);
                        g.FillRectangle(brushNaranja, margen, y, anchoUtil, 18);
                        string nombreConceptoCorto = concepto.Nombre.Length > 25 ? concepto.Nombre.Substring(0, 22) + "..." : concepto.Nombre;
                        g.DrawString($"Total {nombreConceptoCorto}", fontNormal, Brushes.White, margen + 8, y + 4);
                        g.DrawString(totalConcepto.ToString("C0"), fontNormal, Brushes.White, margen + anchoUtil * 0.63f, y + 4);
                        y += 22;
                    }
                }
                
                // Totales
                y = height - 130;
                
                double totalEjecutado = nodosRaiz.Where(c => c.Partidas.Any(p => p.Incluir))
                    .SelectMany(c => c.Partidas.Where(p => p.Incluir))
                    .Sum(p => p.Total);
                
                double porcentajeAmortizacion = prototipoActual.ToUpper().Contains("TUNERA") ? 0.11 : 0.095;
                double amortizacion = totalEjecutado * porcentajeAmortizacion;
                double totalEstimacion = totalEjecutado - amortizacion;
                
                // Cajas de totales
                g.FillRectangle(brushGris, margen + 200, y, 140, 16);
                g.FillRectangle(Brushes.White, margen + 340, y, 140, 16);
                g.DrawRectangle(penLinea, margen + 200, y, 140, 16);
                g.DrawRectangle(penLinea, margen + 340, y, 140, 16);
                g.DrawString("TOTAL REQUISICIÓN", fontNormal, Brushes.Black, margen + 205, y + 3);
                g.DrawString(totalEjecutado.ToString("C2"), fontNormal, Brushes.Black, margen + 350, y + 3);
                y += 18;
                
                string porcentajeTexto = prototipoActual.ToUpper().Contains("TUNERA") ? "11.0" : "9.5";
                g.FillRectangle(brushGris, margen + 200, y, 140, 16);
                g.FillRectangle(Brushes.White, margen + 340, y, 140, 16);
                g.DrawRectangle(penLinea, margen + 200, y, 140, 16);
                g.DrawRectangle(penLinea, margen + 340, y, 140, 16);
                g.DrawString($"AMORTIZACIÓN {porcentajeTexto}%", fontNormal, Brushes.Black, margen + 205, y + 3);
                g.DrawString(amortizacion.ToString("C2"), fontNormal, Brushes.Black, margen + 350, y + 3);
                y += 18;
                
                g.FillRectangle(brushNaranja, margen + 200, y, 140, 20);
                g.FillRectangle(brushNaranja, margen + 340, y, 140, 20);
                g.DrawString("TOTAL ESTIMACIÓN", fontTitulo, Brushes.White, margen + 205, y + 4);
                g.DrawString(totalEstimacion.ToString("C2"), fontTitulo, Brushes.White, margen + 350, y + 4);
                
                // Línea inferior naranja
                y = height - 45;
                g.FillRectangle(brushNaranja, margen, y, anchoUtil, 4);
                
                // Mensaje de preview con número de estimación global
                y = height - 28;
                int totalPartidas = nodosRaiz.Sum(c => c.Partidas.Count(p => p.Incluir));
                string mensaje = $"Vista Previa - Estimación No. {numEst} - {totalPartidas} partida(s)";
                g.DrawString(mensaje, fontPequena, Brushes.Gray, margen, y);
            }
        }
    }
}
