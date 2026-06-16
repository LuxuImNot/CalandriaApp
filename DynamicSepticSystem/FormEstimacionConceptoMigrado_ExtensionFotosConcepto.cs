using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Extensión que agrega al PDF de estimación una hoja aparte con las fotos
    /// adjuntas a cada concepto/partida (tabla FotosConcepto, vía GestorFotosConcepto).
    /// Cada foto lleva un subtítulo que indica a qué concepto o partida pertenece.
    /// Es independiente de las "evidencias fotográficas" (GestorEvidencias).
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        /// <summary>
        /// Recorre los conceptos y sus partidas en el mismo orden del árbol
        /// (concepto seguido de sus partidas) para dibujar sus fotos.
        /// </summary>
        private List<NodoConcepto> AplanarConceptosParaFotos()
        {
            var lista = new List<NodoConcepto>();
            if (nodosRaiz == null) return lista;

            foreach (var concepto in nodosRaiz)
            {
                lista.Add(concepto);
                if (concepto.Partidas != null)
                {
                    foreach (var partida in concepto.Partidas)
                        lista.Add(partida);
                }
            }
            return lista;
        }

        /// <summary>
        /// Construye el subtítulo que identifica a qué concepto/partida pertenece la foto.
        /// </summary>
        private string ObtenerSubtituloFotoConcepto(NodoConcepto nodo)
        {
            if (nodo.EsConcepto)
            {
                string codigo = string.IsNullOrWhiteSpace(nodo.Codigo) ? "" : $"{nodo.Codigo} - ";
                return $"CONCEPTO: {codigo}{nodo.Nombre}";
            }

            return $"PARTIDA (WBS {nodo.WBS}): {nodo.Nombre}";
        }

        /// <summary>
        /// Dibuja en hojas nuevas del PDF las fotos adjuntas a cada concepto/partida
        /// de la casa seleccionada (Manzana + Lote). No hace nada si no hay fotos.
        /// </summary>
        private void DibujarFotosConceptoPDF(PdfDocument documento)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
                return;

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            if (gestorFotosConcepto == null)
                gestorFotosConcepto = new GestorFotosConcepto(connectionString);

            // 1) Recolectar todos los pares (nodo, foto) que tengan imagen asociada.
            var items = new List<KeyValuePair<NodoConcepto, FotoConceptoInfo>>();
            try
            {
                foreach (var nodo in AplanarConceptosParaFotos())
                {
                    var fotos = gestorFotosConcepto.ListarFotos(
                        manzana, lote, ObtenerIdentificadorNodo(nodo), nodo.EsConcepto);

                    foreach (var foto in fotos)
                        items.Add(new KeyValuePair<NodoConcepto, FotoConceptoInfo>(nodo, foto));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer las fotos de conceptos: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (items.Count == 0)
                return;

            try
            {
                XColor colorEncabezado = XColor.FromArgb(41, 128, 185); // azul
                XColor colorSubtitulo = XColor.FromArgb(52, 73, 94);    // gris azulado
                XColor colorInfo = XColor.FromArgb(236, 240, 241);      // gris claro

                XFont fontTitulo = new XFont("Arial", 14, XFontStyle.Bold);
                XFont fontSubtitulo = new XFont("Arial", 10, XFontStyle.Bold);
                XFont fontNormal = new XFont("Arial", 9);
                XFont fontPequena = new XFont("Arial", 8);

                PdfPage pagina = documento.AddPage();
                pagina.Size = PdfSharp.PageSize.Letter;
                XGraphics gfx = XGraphics.FromPdfPage(pagina);

                double margen = 50;
                double anchoUtil = pagina.Width - (margen * 2);
                double y = 40;

                // Encabezado de la sección
                gfx.DrawRectangle(new XSolidBrush(colorEncabezado), margen, y, anchoUtil, 25);
                gfx.DrawString("FOTOS POR CONCEPTO", fontTitulo, XBrushes.White,
                    new XRect(margen, y, anchoUtil, 25), XStringFormats.Center);
                y += 35;

                gfx.DrawString($"Manzana {manzana} - Lote {lote}",
                    fontSubtitulo, XBrushes.Black, margen, y);
                y += 25;

                int contador = 0;
                foreach (var item in items)
                {
                    NodoConcepto nodo = item.Key;
                    FotoConceptoInfo foto = item.Value;

                    // 2 fotos por página
                    if (contador > 0 && contador % 2 == 0)
                    {
                        pagina = documento.AddPage();
                        pagina.Size = PdfSharp.PageSize.Letter;
                        gfx = XGraphics.FromPdfPage(pagina);
                        y = 40;
                    }

                    try
                    {
                        byte[] fotoBytes = gestorFotosConcepto.ObtenerFoto(foto.Id);

                        // Subtítulo: a qué concepto/partida pertenece la foto
                        gfx.DrawRectangle(new XSolidBrush(colorSubtitulo), margen, y, anchoUtil, 20);
                        gfx.DrawString(ObtenerSubtituloFotoConcepto(nodo), fontSubtitulo, XBrushes.White,
                            new XRect(margen + 6, y, anchoUtil - 12, 20), XStringFormats.CenterLeft);
                        y += 22;

                        // Descripción de la foto (si tiene) + fecha y usuario
                        string descripcion = string.IsNullOrWhiteSpace(foto.Descripcion)
                            ? "(Sin descripción)" : foto.Descripcion;
                        gfx.DrawRectangle(new XSolidBrush(colorInfo), margen, y, anchoUtil, 16);
                        gfx.DrawString(descripcion, fontNormal, XBrushes.Black, margen + 6, y + 12);
                        y += 18;

                        string info = $"Fecha: {foto.FechaFormateada} | Usuario: {foto.Usuario}";
                        gfx.DrawString(info, fontPequena, XBrushes.Gray, margen + 6, y + 10);
                        y += 15;

                        if (fotoBytes != null && fotoBytes.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(fotoBytes))
                            {
                                XImage imagen = XImage.FromStream(ms);

                                double anchoMaxImg = anchoUtil - 10;
                                double altoMaxImg = 250; // permite 2 fotos por página con su subtítulo
                                double ratio = Math.Min(anchoMaxImg / imagen.PixelWidth,
                                    altoMaxImg / imagen.PixelHeight);
                                double anchoFinal = imagen.PixelWidth * ratio;
                                double altoFinal = imagen.PixelHeight * ratio;

                                double xImagen = margen + (anchoUtil - anchoFinal) / 2;

                                gfx.DrawRectangle(XPens.LightGray, xImagen - 2, y - 2,
                                    anchoFinal + 4, altoFinal + 4);
                                gfx.DrawImage(imagen, xImagen, y, anchoFinal, altoFinal);
                                y += altoFinal + 25;
                            }
                        }
                        else
                        {
                            gfx.DrawString("[No se pudo cargar la imagen]",
                                fontNormal, XBrushes.Red, margen + 6, y + 10);
                            y += 25;
                        }
                    }
                    catch (Exception ex)
                    {
                        gfx.DrawString($"[Error al cargar la foto: {ex.Message}]",
                            fontNormal, XBrushes.Red, margen + 6, y + 10);
                        y += 25;
                    }

                    contador++;
                }

                // Pie de página en la última hoja de fotos
                y = pagina.Height - 40;
                gfx.DrawString($"Total de fotos de conceptos incluidas: {items.Count}",
                    fontPequena, XBrushes.Gray,
                    new XRect(margen, y, anchoUtil, 20), XStringFormats.Center);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar las fotos de conceptos en PDF: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
