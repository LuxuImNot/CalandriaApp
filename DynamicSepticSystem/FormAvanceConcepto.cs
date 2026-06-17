using BrightIdeasSoftware;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormAvanceConcepto : Form
    {
        private List<ItemConcepto> itemsConceptos = new List<ItemConcepto>();
        private string prototipoActual = "";

        public FormAvanceConcepto()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            ConfigurarObjectListView();
            CargarManzanas();
            this.Load += FormAvanceConcepto_Load;
        }

        private void FormAvanceConcepto_Load(object sender, EventArgs e)
        {
            this.Text = "Avance por Conceptos - Sistema Calandria";
        }

        private void ConfigurarObjectListView()
        {
            olvAvanceConceptos.FullRowSelect = true;
            // Disable editing here: edits must be done in FormAvanceObra
            olvAvanceConceptos.CellEditActivation = ObjectListView.CellEditActivateMode.None;
            olvAvanceConceptos.UseAlternatingBackColors = true;
            olvAvanceConceptos.AlternateRowBackColor = Color.FromArgb(240, 248, 255);

            // Habilitar agrupamiento
            olvAvanceConceptos.ShowGroups = true;
            olvAvanceConceptos.GroupImageList = new ImageList();

            // Columna Cï¿½digo
            var colCodigo = new OLVColumn("Cï¿½digo", "Codigo") 
            { 
                Width = 80, 
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center
            };

            // Columna Concepto (agrupable)
            var colConcepto = new OLVColumn("Concepto", "Concepto") 
            { 
                Width = 350, 
                IsEditable = false 
            };

            // Configurar agrupamiento por Concepto
            colConcepto.GroupKeyGetter = (rowObject) =>
            {
                var item = (ItemConcepto)rowObject;
                return item.Concepto;
            };

            colConcepto.GroupKeyToTitleConverter = (groupKey) =>
            {
                return $"{groupKey}";
            };

            // Columna Total
            var colTotal = new OLVColumn("TOTAL", "Total")
            {
                Width = 120,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}"
            };

            // Columna Avance % - make non-editable here
            var colAvance = new OLVColumn("Avance %", "AvancePorcentaje")
            {
                Width = 90,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectToStringFormat = "{0:N1}%"
            };

            // Columna Ejecutado - make non-editable here
            var colEjecutado = new OLVColumn("Ejecutado", "MontoEjecutado")
            {
                Width = 120,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}"
            };

            olvAvanceConceptos.AllColumns.AddRange(new[] { colCodigo, colConcepto, colTotal, colAvance, colEjecutado });
            olvAvanceConceptos.RebuildColumns();

            // Remove handlers that attempted to save from this form; keep validation hooks inert
            olvAvanceConceptos.CellEditFinishing += (s, e) =>
            {
                // Editing disabled - do nothing
                e.Cancel = true;
            };

            // Establecer columna de agrupamiento por defecto
            olvAvanceConceptos.PrimarySortColumn = colConcepto;
            olvAvanceConceptos.PrimarySortOrder = System.Windows.Forms.SortOrder.Ascending;
        }

        private void CargarManzanas()
        {
            try
            {
                cmbManzana.Items.Clear();
                var manzanas = ApiClient.Get<List<string>>("/api/avances/manzanas");
                if (manzanas != null)
                    foreach (var m in manzanas)
                        cmbManzana.Items.Add(m);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar manzanas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbManzana_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null) return;

            try
            {
                cmbLote.Items.Clear();
                var lotes = ApiClient.Get<List<string>>(
                    $"/api/avances/lotes?manzana={Uri.EscapeDataString(cmbManzana.SelectedItem.ToString())}");
                if (lotes != null)
                    foreach (var l in lotes)
                        cmbLote.Items.Add(l);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar lotes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCargarAvance_Click(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Por favor selecciona Manzana y Lote", "Atenciï¿½n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            prototipoActual = ObtenerPrototipo(manzana, lote);
            if (string.IsNullOrEmpty(prototipoActual))
            {
                MessageBox.Show($"No se encontrï¿½ el prototipo para M{manzana}-L{lote}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Text = $"Avance por Conceptos - M{manzana} L{lote} ({prototipoActual})";

            itemsConceptos = CargarConceptosPorPrototipo(prototipoActual);
            CargarAvancesGuardados(manzana, lote);

            // Establecer objetos con agrupamiento
            olvAvanceConceptos.SetObjects(itemsConceptos);
            olvAvanceConceptos.BuildGroups(olvAvanceConceptos.PrimarySortColumn, System.Windows.Forms.SortOrder.Ascending);

            // Expandir todos los grupos manualmente
            if (olvAvanceConceptos.OLVGroups != null)
            {
                foreach (OLVGroup group in olvAvanceConceptos.OLVGroups)
                {
                    group.Collapsed = false;
                }
                olvAvanceConceptos.Invalidate();
            }

            ActualizarTotales();
            DibujarGraficas();

            // Cargar y mostrar la ï¿½ltima foto de la casa en el control pictureBoxFotoUltima
            try
            {
                var gestor = new GestorEvidencias();
                var ultima = gestor.ObtenerUltimaEvidencia(manzana, lote);
                if (ultima != null &&ultima.FotoBytes != null && ultima.FotoBytes.Length > 0)
                {
                    using (var ms = new MemoryStream(ultima.FotoBytes))
                    {
                        var img = Image.FromStream(ms);
                        pictureBoxFotoUltima.Image?.Dispose();
                        pictureBoxFotoUltima.Image = new Bitmap(img);
                    }
                }
                else
                {
                    pictureBoxFotoUltima.Image = null;
                }
            }
            catch { pictureBoxFotoUltima.Image = null; }
        }

        private string ObtenerPrototipo(string manzana, string lote)
        {
            try
            {
                return ApiClient.Get<string>(
                    $"/api/avances/prototipo?manzana={Uri.EscapeDataString(manzana ?? "")}&lote={Uri.EscapeDataString(lote ?? "")}") ?? "";
            }
            catch
            {
                return "";
            }
        }

        private List<ItemConcepto> CargarConceptosPorPrototipo(string prototipo)
        {
            var items = new List<ItemConcepto>();

            try
            {
                var conceptos = ApiClient.Get<List<ConceptoAvanceApi>>(
                    $"/api/avances/conceptos?prototipo={Uri.EscapeDataString(prototipo ?? "")}");
                if (conceptos != null)
                    foreach (var c in conceptos)
                        items.Add(new ItemConcepto
                        {
                            Codigo = c.Codigo,
                            Concepto = c.Concepto,
                            Total = c.Total,
                            AvancePorcentaje = 0,
                            MontoEjecutado = 0
                        });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar conceptos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return items;
        }

        private void CargarAvancesGuardados(string manzana, string lote)
        {
            try
            {
                var padresApi = ApiClient.Get<List<AvancePorPadreApi>>(
                    $"/api/avances/avance-por-padre?manzana={Uri.EscapeDataString(manzana ?? "")}" +
                    $"&lote={Uri.EscapeDataString(lote ?? "")}" +
                    $"&prototipo={Uri.EscapeDataString(prototipoActual ?? "")}");

                var padreDict = new Dictionary<string, Tuple<double, double>>(StringComparer.OrdinalIgnoreCase);
                if (padresApi != null)
                    foreach (var pp in padresApi)
                        padreDict[pp.Padre] = Tuple.Create(pp.Total, pp.Ejecutado);
                {

                    // Mapeo en cï¿½digo: Concepto -> lista de Padres que componen ese concepto
                    var mapping = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Acabados", new[] { "Acabados Exteriores", "Acabados Interiores" } },
                        { "Estructura", new[] { "Losas", "Muros" } },
                        { "Limpieza e Instalaciones especiales", new[] { "Inst especiales y Obra Exterior" } },
                        // Agregar aquï¿½ mï¿½s mapeos segï¿½n sea necesario
                    };

                    // Para cada concepto, sumar los padres mapeados y aplicar avance agregado
                    foreach (var itemConcepto in itemsConceptos)
                    {
                        double sumaTotal = 0.0;
                        double sumaEjecutado = 0.0;

                        if (mapping.TryGetValue(itemConcepto.Concepto, out string[] padresMap))
                        {
                            foreach (var padre in padresMap)
                            {
                                var match = padreDict.FirstOrDefault(kvp => string.Equals(kvp.Key, padre, StringComparison.OrdinalIgnoreCase));
                                if (!string.IsNullOrEmpty(match.Key))
                                {
                                    sumaTotal += match.Value.Item1;
                                    sumaEjecutado += match.Value.Item2;
                                }
                            }
                        }
                        else
                        {
                            // Intentar coincidencias por nombre
                            foreach (var kvp in padreDict)
                            {
                                string padre = kvp.Key;
                                if (itemConcepto.Concepto.IndexOf(padre, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    padre.IndexOf(itemConcepto.Concepto, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    sumaTotal += kvp.Value.Item1;
                                    sumaEjecutado += kvp.Value.Item2;
                                }
                            }
                        }

                        if (sumaTotal > 0)
                        {
                            // Ajuste: usar la suma ejecutada directamente para la columna Ejecutado
                            // y calcular el porcentaje respecto al total del concepto en Estimacion(Concepto)
                            itemConcepto.MontoEjecutado = sumaEjecutado;
                            if (itemConcepto.Total > 0)
                            {
                                itemConcepto.AvancePorcentaje = (sumaEjecutado / itemConcepto.Total) * 100.0;
                            }
                            else
                            {
                                itemConcepto.AvancePorcentaje = 0;
                            }
                        }
                        else
                        {
                            // Si no hay datos, dejar en 0
                            itemConcepto.AvancePorcentaje = 0;
                            itemConcepto.MontoEjecutado = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar avances guardados: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarAvanceEnBD(ItemConcepto item)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null) return;

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            try
            {
                // El servidor hace el upsert en AvanceManualConcepto (y asegura la tabla).
                ApiClient.Post("/api/avances/concepto", new
                {
                    Manzana = manzana,
                    Lote = lote,
                    Prototipo = prototipoActual,
                    Codigo = item.Codigo,
                    Concepto = item.Concepto,
                    AvancePorcentaje = item.AvancePorcentaje
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar avance: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarTotales()
        {
            double totalPresupuestado = itemsConceptos.Sum(i => i.Total);
            double totalEjecutado = itemsConceptos.Sum(i => i.MontoEjecutado);
            double avanceGeneral = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;

            lblTotalPresupuestado.Text = $"Total Presupuestado: {totalPresupuestado:C2}";
            lblTotalEjecutado.Text = $"Total Ejecutado: {totalEjecutado:C2}";
            lblAvanceGeneral.Text = $"Avance General: {avanceGeneral:F1}%";

            progressBarAvance.Value = Math.Min(100, (int)avanceGeneral);

            int completadas = itemsConceptos.Count(i => i.AvancePorcentaje >= 100);
            int enProgreso = itemsConceptos.Count(i => i.AvancePorcentaje > 0 && i.AvancePorcentaje < 100);
            int sinIniciar = itemsConceptos.Count(i => i.AvancePorcentaje == 0);

            lblEstadisticas.Text = $"Completados: {completadas} | En Progreso: {enProgreso} | Sin Iniciar: {sinIniciar}";

            if (avanceGeneral < 30)
                lblAvanceGeneral.ForeColor = Color.FromArgb(231, 76, 60);
            else if (avanceGeneral < 70)
                lblAvanceGeneral.ForeColor = Color.FromArgb(243, 156, 18);
            else
                lblAvanceGeneral.ForeColor = Color.FromArgb(46, 204, 113);
        }

        private void DibujarGraficas()
        {
            if (itemsConceptos.Count == 0) return;

            int width = pictureBoxGrafica.Width;
            int height = pictureBoxGrafica.Height;

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Solo pastel
                DibujarGraficaPastel(g, new Rectangle((width-220)/2, 20, 220, 220));
            }

            pictureBoxGrafica.Image?.Dispose();
            pictureBoxGrafica.Image = bmp;
        }

        private void DibujarGraficaPastel(Graphics g, Rectangle rect)
        {
            double totalPresupuestado = itemsConceptos.Sum(i => i.Total);
            double totalEjecutado = itemsConceptos.Sum(i => i.MontoEjecutado);
            double avanceGeneral = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;

            float anguloEjecutado = (float)(avanceGeneral * 3.6);
            float anguloRestante = 360 - anguloEjecutado;

            using (Brush brushEjecutado = new SolidBrush(Color.FromArgb(46, 204, 113)))
            {
                g.FillPie(brushEjecutado, rect, 0, anguloEjecutado);
            }

            using (Brush brushRestante = new SolidBrush(Color.FromArgb(220, 220, 220)))
            {
                g.FillPie(brushRestante, rect, anguloEjecutado, anguloRestante);
            }

            using (Pen pen = new Pen(Color.Gray, 2))
            {
                g.DrawEllipse(pen, rect);
            }

            string textoAvance = $"{avanceGeneral:F1}%";
            using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                SizeF textSize = g.MeasureString(textoAvance, font);
                PointF textPos = new PointF(rect.X + (rect.Width - textSize.Width) / 2, rect.Y + (rect.Height - textSize.Height) / 2);
                g.DrawString(textoAvance, font, textBrush, textPos);
            }

            using (Font fontLeyenda = new Font("Segoe UI", 9))
            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(46, 204, 113)), 220, 40, 15, 15);
                g.DrawString("Ejecutado", fontLeyenda, textBrush, 240, 38);
                g.FillRectangle(new SolidBrush(Color.FromArgb(220, 220, 220)), 220, 65, 15, 15);
                g.DrawString("Restante", fontLeyenda, textBrush, 240, 63);
            }
        }

        private void btnExportarPDF_Click(object sender, EventArgs e)
        {
            if (itemsConceptos.Count == 0)
            {
                MessageBox.Show("Primero carga el avance de una casa", "Atenciï¿½n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = $"AvanceConceptos_M{cmbManzana.SelectedItem}_L{cmbLote.SelectedItem}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GenerarPDFAvance(sfd.FileName);
                    MessageBox.Show("PDF generado exitosamente", "ï¿½xito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var result = MessageBox.Show("ï¿½Deseas abrir el PDF?", "Abrir PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnAbrirReporte_Click(object sender, EventArgs e)
        {
        }

        private void GenerarPDFAvance(string rutaPdf)
        {
            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = $"Avance por Conceptos - M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem}";
            pdf.Info.Author = "Sistema Calandria Residencial";
            pdf.Info.Subject = "Reporte de Avance por Conceptos";
            pdf.Info.Keywords = "Construcciï¿½n, Avance, Conceptos";

            double totalPresupuestado = itemsConceptos.Sum(i => i.Total);
            double totalEjecutado = itemsConceptos.Sum(i => i.MontoEjecutado);
            double avanceGeneral = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;

            XColor colorPrimario = XColor.FromArgb(0, 122, 204);
            XColor colorSecundario = XColor.FromArgb(46, 204, 113);
            XColor colorAccento = XColor.FromArgb(52, 73, 94);
            XColor colorFondo = XColor.FromArgb(236, 240, 241);

            // ============= Pï¿½GINA 1: PORTADA Y RESUMEN =============
            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            gfx.DrawRectangle(new XSolidBrush(colorPrimario), 0, 0, page.Width, 100);

            XFont fontTituloGrande = new XFont("Arial", 24, XFontStyle.Bold);
            XFont fontSubtitulo = new XFont("Arial", 14, XFontStyle.Regular);
            XFont fontTitulo = new XFont("Arial", 16, XFontStyle.Bold);
            XFont fontSubtituloSeccion = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 10);
            XFont fontPequena = new XFont("Arial", 8);
            XFont fontNegrita = new XFont("Arial", 10, XFontStyle.Bold);

            gfx.DrawString("AVANCE POR CONCEPTOS", fontTituloGrande, XBrushes.White,
                new XRect(0, 25, page.Width, 30), XStringFormats.TopCenter);
            gfx.DrawString("Sistema de Control de Construcciï¿½n", fontSubtitulo, XBrushes.White,
                new XRect(0, 55, page.Width, 20), XStringFormats.TopCenter);

            double y = 130;

            XPen penBorde = new XPen(colorPrimario, 2);

            gfx.DrawRectangle(penBorde, new XSolidBrush(colorFondo), 40, y, 240, 80);
            gfx.DrawString("DATOS DEL PROYECTO", fontSubtituloSeccion, new XSolidBrush(colorPrimario), 50, y + 10);
            gfx.DrawString($"Ubicaciï¿½n:", fontNegrita, XBrushes.Black, 50, y + 30);
            gfx.DrawString($"Manzana {cmbManzana.SelectedItem}, Lote {cmbLote.SelectedItem}", fontNormal, XBrushes.Black, 50, y + 45);
            gfx.DrawString($"Prototipo:", fontNegrita, XBrushes.Black, 50, y + 60);
            gfx.DrawString($"{prototipoActual}", fontNormal, XBrushes.Black, 120, y + 60);

            gfx.DrawRectangle(penBorde, new XSolidBrush(colorFondo), 300, y, 260, 80);
            gfx.DrawString("INFORMACIï¿½N DEL REPORTE", fontSubtituloSeccion, new XSolidBrush(colorPrimario), 310, y + 10);
            gfx.DrawString($"Fecha de generaciï¿½n:", fontNegrita, XBrushes.Black, 310, y + 30);
            gfx.DrawString($"{DateTime.Now:dd/MM/yyyy HH:mm}", fontNormal, XBrushes.Black, 430, y + 30);
            gfx.DrawString($"Total de conceptos:", fontNegrita, XBrushes.Black, 310, y + 45);
            gfx.DrawString($"{itemsConceptos.Count}", fontNormal, XBrushes.Black, 430, y + 45);

            y += 110;

            gfx.DrawRectangle(new XSolidBrush(colorPrimario), 40, y, 520, 30);
            gfx.DrawString("RESUMEN EJECUTIVO", fontTitulo, XBrushes.White,
                new XRect(40, y + 5, 520, 25), XStringFormats.TopCenter);
            y += 40;

            int anchoMetrica = 160;
            int xMetrica = 50;

            gfx.DrawRectangle(penBorde, new XSolidBrush(colorFondo), xMetrica, y, anchoMetrica, 70);
            gfx.DrawString("TOTAL PRESUPUESTADO", fontPequena, new XSolidBrush(colorAccento),
                new XRect(xMetrica, y + 10, anchoMetrica, 20), XStringFormats.TopCenter);
            gfx.DrawString(totalPresupuestado.ToString("C2"), fontTituloGrande, new XSolidBrush(colorAccento),
                new XRect(xMetrica, y + 30, anchoMetrica, 30), XStringFormats.TopCenter);

            xMetrica += anchoMetrica + 20;

            gfx.DrawRectangle(penBorde, new XSolidBrush(colorFondo), xMetrica, y, anchoMetrica, 70);
            gfx.DrawString("TOTAL EJECUTADO", fontPequena, new XSolidBrush(colorSecundario),
                new XRect(xMetrica, y + 10, anchoMetrica, 20), XStringFormats.TopCenter);
            gfx.DrawString(totalEjecutado.ToString("C2"), fontTituloGrande, new XSolidBrush(colorSecundario),
                new XRect(xMetrica, y + 30, anchoMetrica, 30), XStringFormats.TopCenter);

            xMetrica += anchoMetrica + 20;

            XColor colorAvance = avanceGeneral < 30 ? XColor.FromArgb(231, 76, 60) :
                                 avanceGeneral < 70 ? XColor.FromArgb(243, 156, 18) :
                                 XColor.FromArgb(46, 204, 113);
            gfx.DrawRectangle(penBorde, new XSolidBrush(colorFondo), xMetrica, y, anchoMetrica, 70);
            gfx.DrawString("AVANCE GENERAL", fontPequena, new XSolidBrush(colorAvance),
                new XRect(xMetrica, y + 10, anchoMetrica, 20), XStringFormats.TopCenter);
            gfx.DrawString($"{avanceGeneral:F1}%", fontTituloGrande, new XSolidBrush(colorAvance),
                new XRect(xMetrica, y + 30, anchoMetrica, 30), XStringFormats.TopCenter);

            y += 90;

            int completadas = itemsConceptos.Count(i => i.AvancePorcentaje >= 100);
            int enProgreso = itemsConceptos.Count(i => i.AvancePorcentaje > 0 && i.AvancePorcentaje < 100);
            int sinIniciar = itemsConceptos.Count(i => i.AvancePorcentaje == 0);
            int totalConAvance = completadas + enProgreso;

            gfx.DrawRectangle(penBorde, XBrushes.White, 40, y, 520, 70);
            gfx.DrawString("ESTADO DE CONCEPTOS", fontSubtituloSeccion, new XSolidBrush(colorPrimario), 50, y + 10);

            int xEstado = 60;
            gfx.DrawRectangle(new XSolidBrush(colorSecundario), xEstado, y + 28, 10, 10);
            gfx.DrawString($"Completados: {completadas}", fontNormal, XBrushes.Black, xEstado + 15, y + 27);

            xEstado += 150;
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(243, 156, 18)), xEstado, y + 28, 10, 10);
            gfx.DrawString($"En progreso: {enProgreso}", fontNormal, XBrushes.Black, xEstado + 15, y + 27);

            xEstado += 150;
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(189, 195, 199)), xEstado, y + 28, 10, 10);
            gfx.DrawString($"Sin iniciar: {sinIniciar}", fontNormal, XBrushes.Black, xEstado + 15, y + 27);

            gfx.DrawString($"* Este reporte muestra {totalConAvance} de {itemsConceptos.Count} conceptos (solo los que tienen avance)",
                fontPequena, new XSolidBrush(XColor.FromArgb(127, 140, 141)),
                new XRect(40, y + 50, 520, 15), XStringFormats.TopCenter);

            y += 80;

            DibujarGraficaPastelPDF(gfx, new XRect(180, y, 240, 240), avanceGeneral, colorPrimario, colorSecundario);

            DibujarPiePagina(gfx, page, 1, fontPequena);

            // ============= Pï¿½GINA 2: DETALLE DE CONCEPTOS =============
            GenerarPaginasDetalleConceptos(pdf, fontTitulo, fontSubtituloSeccion, fontNormal, fontPequena, fontNegrita,
                colorPrimario, colorSecundario, colorAccento, colorFondo);

            pdf.Save(rutaPdf);
        }

        private void GenerarPaginasDetalleConceptos(PdfDocument pdf, XFont fontTitulo, XFont fontSubtituloSeccion,
            XFont fontNormal, XFont fontPequena, XFont fontNegrita,
            XColor colorPrimario, XColor colorSecundario, XColor colorAccento, XColor colorFondo)
        {
            var conceptosConAvance = itemsConceptos
                .Where(i => i.AvancePorcentaje > 0)
                .OrderBy(i => i.Codigo)
                .ToList();

            if (conceptosConAvance.Count == 0)
            {
                PdfPage page = pdf.AddPage();
                page.Size = PdfSharp.PageSize.Letter;
                XGraphics gfx = XGraphics.FromPdfPage(page);

                gfx.DrawString("No hay conceptos en progreso para mostrar", fontTitulo, XBrushes.Gray,
                    new XRect(0, page.Height / 2 - 20, page.Width, 40), XStringFormats.TopCenter);

                DibujarPiePagina(gfx, page, 2, fontPequena);
                return;
            }

            PdfPage pagina = pdf.AddPage();
            pagina.Size = PdfSharp.PageSize.Letter;
            XGraphics gfxPagina = XGraphics.FromPdfPage(pagina);

            gfxPagina.DrawRectangle(new XSolidBrush(colorPrimario), 0, 0, pagina.Width, 60);
            gfxPagina.DrawString("DETALLE DE CONCEPTOS", fontTitulo, XBrushes.White,
                new XRect(0, 20, pagina.Width, 30), XStringFormats.TopCenter);

            double y = 80;
            int numeroPagina = 2;

            // Encabezado de tabla
            XPen penBorde = new XPen(colorPrimario, 1.5);
            gfxPagina.DrawRectangle(new XSolidBrush(colorPrimario), 40, y, 520, 25);
            gfxPagina.DrawString("Cï¿½d.", fontNegrita, XBrushes.White, 45, y + 7);
            gfxPagina.DrawString("Concepto", fontNegrita, XBrushes.White, 90, y + 7);
            gfxPagina.DrawString("Presupuesto", fontNegrita, XBrushes.White, 320, y + 7);
            gfxPagina.DrawString("Avance", fontNegrita, XBrushes.White, 420, y + 7);
            gfxPagina.DrawString("Ejecutado", fontNegrita, XBrushes.White, 490, y + 7);
            y += 25;

            bool alternar = false;
            foreach (var concepto in conceptosConAvance)
            {
                if (y > pagina.Height - 80)
                {
                    DibujarPiePagina(gfxPagina, pagina, numeroPagina++, fontPequena);
                    pagina = pdf.AddPage();
                    pagina.Size = PdfSharp.PageSize.Letter;
                    gfxPagina = XGraphics.FromPdfPage(pagina);
                    gfxPagina.DrawRectangle(new XSolidBrush(colorPrimario), 0, 0, pagina.Width, 50);
                    gfxPagina.DrawString("DETALLE DE CONCEPTOS (continuaciï¿½n)", fontSubtituloSeccion, XBrushes.White,
                        new XRect(0, 15, pagina.Width, 30), XStringFormats.TopCenter);
                    y = 70;
                    alternar = false;
                }

                XBrush brushFondo = alternar ? new XSolidBrush(colorFondo) : XBrushes.White;
                gfxPagina.DrawRectangle(brushFondo, 40, y, 520, 20);

                gfxPagina.DrawString(concepto.Codigo, fontPequena, XBrushes.Black, 45, y + 6);

                string conceptoNombre = concepto.Concepto.Length > 30 ? concepto.Concepto.Substring(0, 27) + "..." : concepto.Concepto;
                gfxPagina.DrawString(conceptoNombre, fontPequena, XBrushes.Black, 90, y + 6);
                gfxPagina.DrawString(concepto.Total.ToString("C2"), fontPequena, XBrushes.Black, 320, y + 6);

                XColor colorAvanceItem = concepto.AvancePorcentaje >= 100 ? colorSecundario :
                                          concepto.AvancePorcentaje > 0 ? XColor.FromArgb(243, 156, 18) :
                                          XColor.FromArgb(189, 195, 199);
                gfxPagina.DrawString($"{concepto.AvancePorcentaje:F1}%", fontPequena, new XSolidBrush(colorAvanceItem), 420, y + 6);
                gfxPagina.DrawString(concepto.MontoEjecutado.ToString("C2"), fontPequena, XBrushes.Black, 490, y + 6);

                y += 20;
                alternar = !alternar;
            }

            DibujarPiePagina(gfxPagina, pagina, numeroPagina, fontPequena);
        }

        private void DibujarGraficaPastelPDF(XGraphics gfx, XRect rect, double porcentajeCompletado,
            XColor colorPrimario, XColor colorSecundario)
        {
            gfx.DrawEllipse(new XSolidBrush(XColor.FromArgb(220, 220, 220)), rect);

            if (porcentajeCompletado > 0)
            {
                double angulo = (porcentajeCompletado / 100.0) * 360;
                XColor colorGradiente = porcentajeCompletado < 30 ? XColor.FromArgb(231, 76, 60) :
                                        porcentajeCompletado < 70 ? XColor.FromArgb(243, 156, 18) :
                                        colorSecundario;
                gfx.DrawPie(new XSolidBrush(colorGradiente), rect, -90, angulo);
            }

            double radioInterior = rect.Width * 0.6 / 2;
            XRect rectInterior = new XRect(
                rect.X + (rect.Width - rect.Width * 0.6) / 2,
                rect.Y + (rect.Height - rect.Height * 0.6) / 2,
                rect.Width * 0.6,
                rect.Height * 0.6
            );
            gfx.DrawEllipse(XBrushes.White, rectInterior);

            XFont fontGrande = new XFont("Arial", 28, XFontStyle.Bold);
            string textoPorcentaje = $"{porcentajeCompletado:F1}%";
            XSize textSize = gfx.MeasureString(textoPorcentaje, fontGrande);
            gfx.DrawString(textoPorcentaje, fontGrande, XBrushes.Black,
                new XRect(rect.X, rect.Y + rect.Height / 2 - textSize.Height / 2, rect.Width, textSize.Height),
                XStringFormats.Center);
        }

        private void DibujarPiePagina(XGraphics gfx, PdfPage page, int numeroPagina, XFont fontPequena)
        {
            double yPie = page.Height - 30;

            gfx.DrawLine(new XPen(XColor.FromArgb(189, 195, 199), 1), 40, yPie - 10, page.Width - 40, yPie - 10);

            gfx.DrawString("Sistema Calandria Residencial - Avance por Conceptos", fontPequena,
                XBrushes.Gray, 40, yPie);

            gfx.DrawString($"Pï¿½gina {numeroPagina}", fontPequena, XBrushes.Gray,
                new XRect(0, yPie, page.Width, 20), XStringFormats.TopCenter);

            gfx.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), fontPequena, XBrushes.Gray,
                new XRect(0, yPie, page.Width - 40, 20), XStringFormats.TopRight);
        }

        private void FormAvanceConcepto_Resize(object sender, EventArgs e)
        {
            if (itemsConceptos.Count > 0)
            {
                DibujarGraficas();
            }
        }
    }

    public class ItemConcepto
    {
        public string Codigo { get; set; }
        public string Concepto { get; set; }
        public double Total { get; set; }
        public double AvancePorcentaje { get; set; }
        public double MontoEjecutado { get; set; }
        // Compatibilidad: permite seleccionar conceptos para exportar
        public bool Incluir { get; set; } = true;
    }
}
