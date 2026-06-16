// Extensión para generar PDFs de vales de salida de almacén
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        /// <summary>
        /// Genera el PDF del vale de salida y lo guarda en el repositorio
        /// </summary>
        private void GenerarValeSalidaPDF()
        {
            if (casaActual == null || casaInventario == null)
            {
                MessageBox.Show("?? Primero selecciona una casa destino.",
                    "Casa requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable dt = dgvInsumos.DataSource as DataTable;
            if (dt == null || dt.Columns.Count == 1)
            {
                MessageBox.Show("?? No hay insumos cargados para generar vale.",
                    "Sin insumos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Recolectar insumos con cantidad solicitada > 0
            var insumosSalida = new List<InsumoSalida>();
            decimal totalImporte = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (!dt.Columns.Contains("CantidadSolicitada") || row.IsNull("CantidadSolicitada"))
                    continue;

                decimal solicitada = Convert.ToDecimal(row["CantidadSolicitada"]);
                if (solicitada <= 0) continue;

                decimal precioUnitario = Convert.ToDecimal(row["PrecioUnitario"]);
                decimal importe = solicitada * precioUnitario;

                insumosSalida.Add(new InsumoSalida
                {
                    Codigo = row["Clave"].ToString(),
                    Descripcion = row["Descripcion"].ToString(),
                    Unidad = row["Unidad"].ToString(),
                    Cantidad = solicitada,
                    PrecioUnitario = precioUnitario,
                    Importe = importe
                });

                totalImporte += importe;
            }

            if (insumosSalida.Count == 0)
            {
                MessageBox.Show("?? No se capturó ninguna cantidad para generar el vale.",
                    "Sin cantidades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Generar folio único
                string folio = GenerarFolioSalida(casaActual.Manzana, casaActual.Lote);

                // Solicitar datos adicionales
                var datosAdicionales = SolicitarDatosVale();
                if (datosAdicionales == null)
                    return; // Usuario canceló

                // Generar PDF
                string rutaPdf = GenerarPDFValeSalida(folio, insumosSalida, totalImporte, datosAdicionales);

                // Guardar en repositorio
                GuardarValeSalidaEnRepositorio(folio, rutaPdf, insumosSalida, totalImporte, datosAdicionales);

                MessageBox.Show(
                    $"? Vale de salida generado correctamente.\n\n" +
                    $"Folio: {folio}\n" +
                    $"Total insumos: {insumosSalida.Count}\n" +
                    $"Importe total: {totalImporte:C2}\n\n" +
                    $"El PDF se guardó en el repositorio.",
                    "Vale Generado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"? Error al generar vale de salida:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Genera un folio único para el vale de salida
        /// Formato: VALE-M{manzana}-L{lote}-{numero:000}-{año}
        /// </summary>
        private string GenerarFolioSalida(string manzana, string lote)
        {
            int siguienteNumero = 1;
            string anio = DateTime.Now.Year.ToString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Obtener el siguiente número correlativo
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT ISNULL(MAX(NumeroSalida), 0) + 1 
                    FROM FoliosSalidaAlmacen 
                    WHERE Manzana = @Manzana AND Lote = @Lote", conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    siguienteNumero = (int)cmd.ExecuteScalar();
                }
            }

            return $"VALE-M{manzana}-L{lote}-{siguienteNumero:D3}-{anio}";
        }

        /// <summary>
        /// Solicita datos adicionales para el vale (solicitante, residente de obra, etc.)
        /// </summary>
        private DatosVale SolicitarDatosVale()
        {
            using (Form formDatos = new Form())
            {
                formDatos.Text = "Datos del Vale de Salida";
                formDatos.StartPosition = FormStartPosition.CenterParent;
                formDatos.Size = new Size(500, 280);
                formDatos.FormBorderStyle = FormBorderStyle.FixedDialog;
                formDatos.MaximizeBox = false;
                formDatos.MinimizeBox = false;

                int y = 20;

                // Solicitante
                Label lblSolicitante = new Label
                {
                    Text = "Solicitante:",
                    Location = new Point(20, y),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 9F)
                };
                TextBox txtSolicitante = new TextBox
                {
                    Location = new Point(130, y),
                    Size = new Size(340, 25),
                    Font = new Font("Segoe UI", 9F)
                };
                y += 35;

                // Residente de Obra
                Label lblResidente = new Label
                {
                    Text = "Residente de Obra:",
                    Location = new Point(20, y),
                    Size = new Size(110, 20),
                    Font = new Font("Segoe UI", 9F)
                };
                TextBox txtResidente = new TextBox
                {
                    Location = new Point(130, y),
                    Size = new Size(340, 25),
                    Font = new Font("Segoe UI", 9F)
                };
                y += 35;

                // Encargado de Almacén
                Label lblEncargado = new Label
                {
                    Text = "Encargado de Almacén:",
                    Location = new Point(20, y),
                    Size = new Size(110, 20),
                    Font = new Font("Segoe UI", 9F)
                };
                TextBox txtEncargado = new TextBox
                {
                    Location = new Point(130, y),
                    Size = new Size(340, 25),
                    Font = new Font("Segoe UI", 9F),
                    Text = Global.UsuarioActual?.Nombre ?? Environment.UserName
                };
                y += 35;

                // Observaciones
                Label lblObservaciones = new Label
                {
                    Text = "Observaciones:",
                    Location = new Point(20, y),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 9F)
                };
                TextBox txtObservaciones = new TextBox
                {
                    Location = new Point(130, y),
                    Size = new Size(340, 50),
                    Font = new Font("Segoe UI", 9F),
                    Multiline = true
                };
                y += 65;

                // Botones
                Button btnOk = new Button
                {
                    Text = "Generar Vale",
                    DialogResult = DialogResult.OK,
                    Location = new Point(260, y),
                    Size = new Size(110, 35),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    BackColor = ThemeManager.ColorExito,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                Button btnCancelar = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(380, y),
                    Size = new Size(90, 35),
                    Font = new Font("Segoe UI", 9F)
                };

                formDatos.Controls.AddRange(new Control[] {
                    lblSolicitante, txtSolicitante,
                    lblResidente, txtResidente,
                    lblEncargado, txtEncargado,
                    lblObservaciones, txtObservaciones,
                    btnOk, btnCancelar
                });

                formDatos.AcceptButton = btnOk;
                formDatos.CancelButton = btnCancelar;

                if (formDatos.ShowDialog() == DialogResult.OK)
                {
                    return new DatosVale
                    {
                        Solicitante = txtSolicitante.Text.Trim(),
                        ResidenteObra = txtResidente.Text.Trim(),
                        EncargadoAlmacen = txtEncargado.Text.Trim(),
                        Observaciones = txtObservaciones.Text.Trim()
                    };
                }

                return null;
            }
        }

        /// <summary>
        /// Genera el PDF del vale de salida basado en el formato proporcionado
        /// </summary>
        private string GenerarPDFValeSalida(string folio, List<InsumoSalida> insumos, decimal totalImporte, DatosVale datos)
        {
            // Crear carpeta para PDFs
            string carpetaPdf = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CALANDRIA RESIDENCIAL", "PDFValesSalida");
            Directory.CreateDirectory(carpetaPdf);

            string rutaPdf = Path.Combine(carpetaPdf, $"ValeSalida_{folio}.pdf");

            // Crear documento PDF
            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = "Vale de Salida de Almacén - " + folio;
            pdf.Info.Author = "Desarrolladora de Casas Camaney";
            pdf.Info.Subject = "Vale de Salida";
            pdf.Info.Creator = "Sistema Calandria";

            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Definir fuentes
            XFont fontTitle = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontBold = new XFont("Arial", 9, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontSmall = new XFont("Arial", 7, XFontStyle.Regular);
            XFont fontTiny = new XFont("Arial", 6, XFontStyle.Regular);

            int margin = 10;
            double x = margin;
            double y = margin;
            double pageWidth = page.Width - (margin * 2);

            // ==================== ENCABEZADO ====================
            // Dibujar borde exterior
            XRect borderRect = new XRect(margin, margin, pageWidth, 80);
            gfx.DrawRectangle(XPens.Black, borderRect);

            // Logo CALANDRIA (Izquierda)
            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    using (var logo = XImage.FromStream(ms))
                    {
                        int logoWidth = 120;
                        int logoHeight = 70;
                        gfx.DrawImage(logo, margin + 5, margin + 5, logoWidth, logoHeight);
                    }
                }
            }
            catch { }

            // Línea vertical después del logo
            gfx.DrawLine(XPens.Black, margin + 135, margin, margin + 135, margin + 80);

            // Información de la empresa (Centro-Derecha)
            double textStartX = margin + 145;
            double textY = margin + 15;

            gfx.DrawString("DESARROLLADORA DE CASAS CAMANEY",
                fontBold, XBrushes.Black,
                new XRect(textStartX, textY, pageWidth - 155, 15), XStringFormats.TopCenter);
            
            textY += 13;
            gfx.DrawString("ALMACEN DE INSUMOS Y HERRAMIENTAS",
                fontNormal, XBrushes.Black,
                new XRect(textStartX, textY, pageWidth - 155, 15), XStringFormats.TopCenter);
            
            textY += 13;
            gfx.DrawString("VALE DE SALIDA DE ALMACEN",
                fontTitle, XBrushes.Black,
                new XRect(textStartX, textY, pageWidth - 155, 15), XStringFormats.TopCenter);

            // Línea horizontal después del encabezado
            y = margin + 80;
            gfx.DrawLine(XPens.Black, margin, y, margin + pageWidth, y);

            y += 5;

            // ==================== INFORMACIÓN BÁSICA ====================
            // Fila 1: OBRA, Edificación, Urbanización, FOLIO
            double col1X = margin + 10;
            double col2X = margin + 140;
            double col3X = margin + 300;
            double col4X = margin + 430;

            // OBRA
            gfx.DrawString("OBRA", fontNormal, XBrushes.Black, col1X, y);
            string obraTexto = $"M{casaActual.Manzana}-L{casaActual.Lote}";
            gfx.DrawString(obraTexto, fontNormal, XBrushes.Black, col1X + 60, y);
            // Línea de subrayado para OBRA
            gfx.DrawLine(XPens.Black, col1X + 30, y + 2, col1X + 120, y + 2);

            // Edificación
            gfx.DrawString("Edificación", fontNormal, XBrushes.Black, col2X, y);
            XRect edificacionRect = new XRect(col2X + 60, y - 5, 80, 12);
            gfx.DrawRectangle(XPens.Black, edificacionRect);

            // Urbanización
            gfx.DrawString("Urbanización", fontNormal, XBrushes.Black, col3X, y);
            XRect urbanizacionRect = new XRect(col3X + 65, y - 5, 60, 12);
            gfx.DrawRectangle(XPens.Black, urbanizacionRect);

            // FOLIO
            gfx.DrawString("FOLIO:", fontBold, XBrushes.Black, col4X, y);
            string folioNumero = folio.Substring(folio.LastIndexOf('-') + 1);
            gfx.DrawString(folioNumero, fontBold, XBrushes.Black, col4X + 100, y);
            // Línea de subrayado para FOLIO
            gfx.DrawLine(XPens.Black, col4X + 40, y + 2, col4X + 150, y + 2);

            y += 20;

            // FECHA DE SOLICITUD (alineado a la derecha)
            gfx.DrawString("FECHA DE SOLICITUD:", fontNormal, XBrushes.Black, col4X, y);
            
            y += 12;

            // Casillas de fecha
            DateTime fecha = DateTime.Now;
            double fechaStartX = col4X + 15;
            double diaX = fechaStartX;
            double mesX = diaX + 35;
            double anioX = mesX + 35;

            // Día
            XRect diaRect = new XRect(diaX, y, 25, 12);
            gfx.DrawRectangle(XPens.Black, diaRect);
            gfx.DrawString(fecha.Day.ToString("D2"), fontNormal, XBrushes.Black,
                new XRect(diaX, y + 2, 25, 12), XStringFormats.TopCenter);

            // Mes
            XRect mesRect = new XRect(mesX, y, 25, 12);
            gfx.DrawRectangle(XPens.Black, mesRect);
            gfx.DrawString(fecha.Month.ToString("D2"), fontNormal, XBrushes.Black,
                new XRect(mesX, y + 2, 25, 12), XStringFormats.TopCenter);

            // Año
            XRect anioRect = new XRect(anioX, y, 35, 12);
            gfx.DrawRectangle(XPens.Black, anioRect);
            gfx.DrawString(fecha.Year.ToString(), fontNormal, XBrushes.Black,
                new XRect(anioX, y + 2, 35, 12), XStringFormats.TopCenter);

            // Etiquetas debajo de las casillas
            y += 13;
            gfx.DrawString("Día", fontTiny, XBrushes.Black,
                new XRect(diaX, y, 25, 10), XStringFormats.TopCenter);
            gfx.DrawString("Mes", fontTiny, XBrushes.Black,
                new XRect(mesX, y, 25, 10), XStringFormats.TopCenter);
            gfx.DrawString("Año", fontTiny, XBrushes.Black,
                new XRect(anioX, y, 35, 10), XStringFormats.TopCenter);

            y += 15;

            // Línea divisoria
            gfx.DrawLine(XPens.Black, margin, y, margin + pageWidth, y);
            y += 5;

            // ==================== TABLA DE INSUMOS ====================
            // Encabezados de tabla
            string[] headers = { "Nº", "CÓDIGO", "DESCRIPCIÓN", "UNIDAD", "CANTIDAD", "PRECIO UNIT.", "IMPORTE", "OBSERVACIONES" };
            double[] widths = { 25, 60, 185, 50, 60, 75, 75, 60 };

            double tableY = y;
            double currentX = margin;

            // Dibujar encabezados
            for (int i = 0; i < headers.Length; i++)
            {
                XRect headerRect = new XRect(currentX, tableY, widths[i], 16);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, headerRect);
                gfx.DrawString(headers[i], fontBold, XBrushes.Black,
                    new XRect(currentX + 2, tableY + 3, widths[i] - 4, 14),
                    XStringFormats.TopCenter);
                currentX += widths[i];
            }

            tableY += 16;

            // Dibujar filas de datos (máximo 10 filas)
            int maxFilas = 10;
            double rowHeight = 18;
            int filaActual = 1;

            foreach (var insumo in insumos.Take(maxFilas))
            {
                currentX = margin;

                // Número de fila
                XRect numRect = new XRect(currentX, tableY, widths[0], rowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, numRect);
                gfx.DrawString(filaActual.ToString(), fontSmall, XBrushes.Black,
                    new XRect(currentX + 2, tableY + 5, widths[0] - 4, rowHeight),
                    XStringFormats.TopCenter);
                currentX += widths[0];

                // Código
                XRect codRect = new XRect(currentX, tableY, widths[1], rowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, codRect);
                gfx.DrawString(insumo.Codigo, fontSmall, XBrushes.Black,
                    new XRect(currentX + 3, tableY + 5, widths[1] - 6, rowHeight),
                    XStringFormats.TopLeft);
                currentX += widths[1];

                // Descripción
                string desc = insumo.Descripcion.Length > 40 ? insumo.Descripcion.Substring(0, 37) + "..." : insumo.Descripcion;
                XRect descRect = new XRect(currentX, tableY, widths[2], rowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, descRect);
                gfx.DrawString(desc, fontSmall, XBrushes.Black,
                    new XRect(currentX + 3, tableY + 5, widths[2] - 6, rowHeight),
                    XStringFormats.TopLeft);
                currentX += widths[2];

                // Unidad
                XRect unidRect = new XRect(currentX, tableY, widths[3], rowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, unidRect);
                gfx.DrawString(insumo.Unidad, fontSmall, XBrushes.Black,
                    new XRect(currentX + 2, tableY + 5, widths[3] - 4, rowHeight),
                    XStringFormats.TopCenter);
                currentX += widths[3];

                // Cantidad
                XRect cantRect = new XRect(currentX, tableY, widths[4], rowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, cantRect);
                gfx.DrawString(insumo.Cantidad.ToString("N0"), fontSmall, XBrushes.Black,
                    new XRect(currentX + 2, tableY + 5, widths[4] - 4, rowHeight),
                    XStringFormats.TopCenter);
                currentX += widths[4];

                // Precio Unitario
                XRect precioRect = new XRect(currentX, tableY, widths[5], rowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, precioRect);
                gfx.DrawString("$   " + insumo.PrecioUnitario.ToString("N2"), fontSmall, XBrushes.Black,
                    new XRect(currentX + 3, tableY + 5, widths[5] - 6, rowHeight),
                    XStringFormats.TopRight);
                currentX += widths[5];

                // Importe
                XRect impRect = new XRect(currentX, tableY, widths[6], rowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, impRect);
                gfx.DrawString("$   " + insumo.Importe.ToString("N2"), fontSmall, XBrushes.Black,
                    new XRect(currentX + 3, tableY + 5, widths[6] - 6, rowHeight),
                    XStringFormats.TopRight);
                currentX += widths[6];

                // Observaciones
                XRect obsRect = new XRect(currentX, tableY, widths[7], rowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, obsRect);
                gfx.DrawString("-", fontSmall, XBrushes.Black,
                    new XRect(currentX + 2, tableY + 5, widths[7] - 4, rowHeight),
                    XStringFormats.TopCenter);

                tableY += rowHeight;
                filaActual++;
            }

            // Rellenar filas vacías hasta completar 10
            while (filaActual <= maxFilas)
            {
                currentX = margin;
                for (int i = 0; i < widths.Length; i++)
                {
                    XRect emptyRect = new XRect(currentX, tableY, widths[i], rowHeight);
                    gfx.DrawRectangle(XPens.Black, XBrushes.White, emptyRect);
                    if (i == 0)
                    {
                        gfx.DrawString(filaActual.ToString(), fontSmall, XBrushes.Black,
                            new XRect(currentX + 2, tableY + 5, widths[i] - 4, rowHeight),
                            XStringFormats.TopCenter);
                    }
                    else if (i == 6) // Columna de Importe vacía
                    {
                        gfx.DrawString("$                  -", fontSmall, XBrushes.Black,
                            new XRect(currentX + 3, tableY + 5, widths[i] - 6, rowHeight),
                            XStringFormats.TopRight);
                    }
                    else if (i == 7) // Columna de Observaciones vacía
                    {
                        gfx.DrawString("-", fontSmall, XBrushes.Black,
                            new XRect(currentX + 2, tableY + 5, widths[i] - 4, rowHeight),
                            XStringFormats.TopCenter);
                    }
                    currentX += widths[i];
                }
                tableY += rowHeight;
                filaActual++;
            }

            // TOTAL
            tableY += 8;
            double totalLabelX = margin + 390;
            gfx.DrawString("TOTAL", fontBold, XBrushes.Black, totalLabelX, tableY);
            gfx.DrawString("$" + totalImporte.ToString("N2"), fontBold, XBrushes.Black, totalLabelX + 150, tableY);

            // ==================== SECCIÓN DE FIRMAS ====================
            tableY += 50;

            double firmaWidth = 200;
            double firmaHeight = 35;
            double firma1X = margin + 40;
            double firma2X = margin + 290;

            // Fila superior de firmas
            // SOLICITANTE
            gfx.DrawLine(XPens.Black, firma1X, tableY, firma1X + firmaWidth, tableY);
            gfx.DrawString("SOLICITANTE", fontSmall, XBrushes.Black,
                new XRect(firma1X, tableY + 3, firmaWidth, 15), XStringFormats.TopCenter);

            // ENTREGA LOS INSUMOS
            gfx.DrawLine(XPens.Black, firma2X, tableY, firma2X + firmaWidth, tableY);
            gfx.DrawString("ENTREGA LOS INSUMOS", fontSmall, XBrushes.Black,
                new XRect(firma2X, tableY + 3, firmaWidth, 15), XStringFormats.TopCenter);

            tableY += firmaHeight + 25;

            // Fila inferior de firmas
            // RESIDENTE DE OBRA
            gfx.DrawLine(XPens.Black, firma1X, tableY, firma1X + firmaWidth, tableY);
            gfx.DrawString("RESIDENTE DE OBRA", fontSmall, XBrushes.Black,
                new XRect(firma1X, tableY + 3, firmaWidth, 15), XStringFormats.TopCenter);
            gfx.DrawString("Nombre y Firma", fontTiny, XBrushes.Black,
                new XRect(firma1X, tableY + 15, firmaWidth, 15), XStringFormats.TopCenter);

            // ENCARGADO DE ALMACÉN
            gfx.DrawLine(XPens.Black, firma2X, tableY, firma2X + firmaWidth, tableY);
            gfx.DrawString("Encargado de almacén", fontSmall, XBrushes.Black,
                new XRect(firma2X, tableY + 3, firmaWidth, 15), XStringFormats.TopCenter);

            // Guardar PDF
            pdf.Save(rutaPdf);
            pdf.Close();

            // Abrir PDF
            try
            {
                var psi = new ProcessStartInfo(rutaPdf) { UseShellExecute = true };
                Process.Start(psi);
            }
            catch
            {
                try { Process.Start("explorer.exe", carpetaPdf); } catch { }
            }

            return rutaPdf;
        }

        /// <summary>
        /// Guarda el vale de salida en el repositorio de la base de datos
        /// </summary>
        private void GuardarValeSalidaEnRepositorio(string folio, string rutaPdf, List<InsumoSalida> insumos,
            decimal totalImporte, DatosVale datos)
        {
            try
            {
                // Leer el PDF como bytes
                byte[] pdfBytes = File.ReadAllBytes(rutaPdf);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Verificar que las tablas existan
                    CrearTablasRepositorioSalidasSiNoExisten(conn);

                    DateTime fechaSolicitud = DateTime.Now;
                    int numeroSalida = ObtenerSiguienteNumeroSalida(casaActual.Manzana, casaActual.Lote, conn);

                    // 1. Insertar en FoliosSalidaAlmacen
                    int folioId;
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO FoliosSalidaAlmacen 
                        (Folio, Manzana, Lote, Prototipo, Obra, FechaSolicitud, DiaSolicitud, MesSolicitud, AnioSolicitud,
                         TotalImporte, NumeroSalida, Usuario, Solicitante, ResidenteObra, EncargadoAlmacen, Observaciones, Estado)
                        VALUES 
                        (@Folio, @Manzana, @Lote, @Prototipo, @Obra, @Fecha, @Dia, @Mes, @Anio,
                         @Total, @NumSalida, @Usuario, @Solicitante, @Residente, @Encargado, @Obs, 'PENDIENTE');
                        SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
                    {
                        cmd.Parameters.AddWithValue("@Folio", folio);
                        cmd.Parameters.AddWithValue("@Manzana", casaActual.Manzana);
                        cmd.Parameters.AddWithValue("@Lote", casaActual.Lote);
                        cmd.Parameters.AddWithValue("@Prototipo", casaInventario?.Prototipo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Obra", $"M{casaActual.Manzana}-L{casaActual.Lote}");
                        cmd.Parameters.AddWithValue("@Fecha", fechaSolicitud);
                        cmd.Parameters.AddWithValue("@Dia", fechaSolicitud.Day);
                        cmd.Parameters.AddWithValue("@Mes", fechaSolicitud.Month);
                        cmd.Parameters.AddWithValue("@Anio", fechaSolicitud.Year);
                        cmd.Parameters.AddWithValue("@Total", totalImporte);
                        cmd.Parameters.AddWithValue("@NumSalida", numeroSalida);
                        cmd.Parameters.AddWithValue("@Usuario", Global.UsuarioActual?.Nombre ?? Environment.UserName);
                        cmd.Parameters.AddWithValue("@Solicitante", datos.Solicitante ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Residente", datos.ResidenteObra ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Encargado", datos.EncargadoAlmacen ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Obs", datos.Observaciones ?? (object)DBNull.Value);

                        folioId = (int)cmd.ExecuteScalar();
                    }

                    // 2. Insertar detalle de insumos
                    int numeroFila = 1;
                    foreach (var insumo in insumos)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO FoliosSalidaAlmacenDetalle 
                            (FolioId, NumeroFila, Codigo, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe)
                            VALUES (@FolioId, @NumFila, @Codigo, @Desc, @Unidad, @Cant, @Precio, @Importe)", conn))
                        {
                            cmd.Parameters.AddWithValue("@FolioId", folioId);
                            cmd.Parameters.AddWithValue("@NumFila", numeroFila++);
                            cmd.Parameters.AddWithValue("@Codigo", insumo.Codigo);
                            cmd.Parameters.AddWithValue("@Desc", insumo.Descripcion);
                            cmd.Parameters.AddWithValue("@Unidad", insumo.Unidad);
                            cmd.Parameters.AddWithValue("@Cant", insumo.Cantidad);
                            cmd.Parameters.AddWithValue("@Precio", insumo.PrecioUnitario);
                            cmd.Parameters.AddWithValue("@Importe", insumo.Importe);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 3. Guardar PDF en la base de datos
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO PDFsSalidaAlmacen 
                        (FolioId, Folio, Manzana, Lote, NombreArchivo, ContenidoPDF, TamanioBytes)
                        VALUES (@FolioId, @Folio, @Manzana, @Lote, @Nombre, @Contenido, @Tamanio)", conn))
                    {
                        cmd.Parameters.AddWithValue("@FolioId", folioId);
                        cmd.Parameters.AddWithValue("@Folio", folio);
                        cmd.Parameters.AddWithValue("@Manzana", casaActual.Manzana);
                        cmd.Parameters.AddWithValue("@Lote", casaActual.Lote);
                        cmd.Parameters.AddWithValue("@Nombre", Path.GetFileName(rutaPdf));
                        cmd.Parameters.AddWithValue("@Contenido", pdfBytes);
                        cmd.Parameters.AddWithValue("@Tamanio", pdfBytes.Length);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar en repositorio: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el siguiente número correlativo de salida para una casa
        /// </summary>
        private int ObtenerSiguienteNumeroSalida(string manzana, string lote, SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT ISNULL(MAX(NumeroSalida), 0) + 1 
                FROM FoliosSalidaAlmacen 
                WHERE Manzana = @Manzana AND Lote = @Lote", conn))
            {
                cmd.Parameters.AddWithValue("@Manzana", manzana);
                cmd.Parameters.AddWithValue("@Lote", lote);
                return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Crea las tablas del repositorio de salidas si no existen
        /// </summary>
        private void CrearTablasRepositorioSalidasSiNoExisten(SqlConnection conn)
        {
            string checkSql = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'FoliosSalidaAlmacen'";

            using (SqlCommand cmd = new SqlCommand(checkSql, conn))
            {
                int count = (int)cmd.ExecuteScalar();

                if (count == 0)
                {
                    string scriptPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "SQL_SCRIPTS", "CrearTablasRepositorioSalidasAlmacen.sql");

                    if (File.Exists(scriptPath))
                    {
                        string script = File.ReadAllText(scriptPath);
                        string[] batches = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string batch in batches)
                        {
                            if (!string.IsNullOrWhiteSpace(batch))
                            {
                                try
                                {
                                    using (SqlCommand batchCmd = new SqlCommand(batch, conn))
                                    {
                                        batchCmd.ExecuteNonQuery();
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Abre el repositorio de vales de salida
        /// </summary>
        public void AbrirRepositorioValesSalida(string manzana = null, string lote = null)
        {
            try
            {
                using (var formRepo = new FormRepositorioValesSalida(manzana, lote))
                {
                    formRepo.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir repositorio: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Clases auxiliares
    public class InsumoSalida
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
    }

    public class DatosVale
    {
        public string Solicitante { get; set; }
        public string ResidenteObra { get; set; }
        public string EncargadoAlmacen { get; set; }
        public string Observaciones { get; set; }
    }
}
