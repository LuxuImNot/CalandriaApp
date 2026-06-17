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
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormAvanceObra : Form
    {
        private List<NodoCategoria> nodosRaiz = new List<NodoCategoria>();
        private string prototipoActual = "";

        public FormAvanceObra()
        {
            InitializeComponent();
            ConfigurarTreeListView();
            CargarManzanas();
            this.Load += FormAvanceObra_Load;
            
            // Aplicar tema
            ThemeManager.AplicarTema(this);
        }

        private void FormAvanceObra_Load(object sender, EventArgs e)
        {
            this.Text = "Avance de Obra (Solo Lectura) - Sistema Calandria";
        }

        private void ConfigurarTreeListView()
        {
            // Configurar como TreeListView
            olvAvance.CanExpandGetter = delegate(object x) {
                var nodo = (NodoCategoria)x;
                return nodo.EsCategoria && nodo.Partidas.Count > 0;
            };
            
            olvAvance.ChildrenGetter = delegate(object x) {
                return ((NodoCategoria)x).Partidas;
            };

            olvAvance.FullRowSelect = true;
            // ?? MODO SOLO LECTURA - Edición deshabilitada
            olvAvance.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.None;
            olvAvance.UseAlternatingBackColors = true;
            olvAvance.AlternateRowBackColor = Color.FromArgb(240, 248, 255);
            
            // ?? DESHABILITAR SORTING para mantener orden 1-12
            olvAvance.ShowSortIndicators = false;
            olvAvance.Sorting = System.Windows.Forms.SortOrder.None;

            // Columnas
            var colNombre = new OLVColumn("Categoría / Partida", "Nombre") 
            { 
                Width = 350,
                IsEditable = false, 
                Sortable = false,  // ?? NO permitir ordenar por esta columna
                FillsFreeSpace = true
            };

            var colCosto = new OLVColumn("Costo", "ImporteTotal")
            {
                Width = 130,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}",
                Sortable = false  // ?? NO permitir ordenar
            };

            // Columna Avance % (SOLO LECTURA)
            var colAvance = new OLVColumn("Avance %", "AvancePorcentaje")
            {
                Width = 120,
                IsEditable = false, // ?? SOLO LECTURA
                TextAlign = HorizontalAlignment.Center,
                AspectToStringFormat = "{0:N1}%",
                Sortable = false  // ?? NO permitir ordenar
            };

            // Columna Ejecutado (SOLO LECTURA)
            var colEjecutado = new OLVColumn("Ejecutado", "ImporteEjecutado")
            {
                Width = 140,
                IsEditable = false, // ?? SOLO LECTURA
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}",
                Sortable = false  // ?? NO permitir ordenar
            };

            olvAvance.AllColumns.AddRange(new[] { colNombre, colCosto, colAvance, colEjecutado });
            olvAvance.RebuildColumns();

            // Aplicar formato condicional para partidas completadas
            olvAvance.RowFormatter = delegate(OLVListItem item)
            {
                var nodo = (NodoCategoria)item.RowObject;
                
                // Si la partida está completada al 100%, ponerla en gris
                if (!nodo.EsCategoria && nodo.AvancePorcentaje >= 100)
                {
                    item.BackColor = Color.FromArgb(220, 220, 220);
                    item.ForeColor = Color.FromArgb(100, 100, 100);
                    
                    // Hacer que el texto se vea "tachado" visualmente
                    Font currentFont = item.Font ?? olvAvance.Font;
                    item.Font = new Font(currentFont, FontStyle.Strikeout);
                }
            };
            
            // ?? DESHABILITAR sorting al hacer clic en columnas
            olvAvance.BeforeSorting += (s, e) => e.Canceled = true;
        }

        private NodoCategoria EncontrarPadre(NodoCategoria hijo)
        {
            foreach (var raiz in nodosRaiz)
            {
                if (raiz.Partidas.Contains(hijo))
                    return raiz;
            }
            return null;
        }

        private void RecalcularAvanceCategoria(NodoCategoria categoria)
        {
            if (!categoria.EsCategoria || categoria.Partidas.Count == 0)
                return;

            double totalPartidas = categoria.Partidas.Sum(p => p.ImporteTotal);
            double ejecutadoPartidas = categoria.Partidas.Sum(p => p.ImporteEjecutado);

            categoria.ImporteTotal = totalPartidas;
            categoria.ImporteEjecutado = ejecutadoPartidas;
            categoria.AvancePorcentaje = totalPartidas > 0 
                ? (ejecutadoPartidas / totalPartidas) * 100.0 
                : 0;
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
                MessageBox.Show("Por favor selecciona Manzana y Lote", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            prototipoActual = ObtenerPrototipo(manzana, lote);
            if (string.IsNullOrEmpty(prototipoActual))
            {
                MessageBox.Show($"No se encontró el prototipo para M{manzana}-L{lote}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Text = $"Avance de Obra - M{manzana} L{lote} ({prototipoActual})";

            CargarAvanceJerarquico(manzana, lote);
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

        private void CargarAvanceJerarquico(string manzana, string lote)
        {
            nodosRaiz = new List<NodoCategoria>();

            try
            {
                var resp = ApiClient.Get<JerarquicoAvanceApi>(
                    $"/api/avances/jerarquico?manzana={Uri.EscapeDataString(manzana ?? "")}" +
                    $"&lote={Uri.EscapeDataString(lote ?? "")}" +
                    $"&prototipo={Uri.EscapeDataString(prototipoActual ?? "")}");

                var partidas = new List<Tuple<int, string, string, string, string, double>>();
                if (resp?.Partidas != null)
                    foreach (var p in resp.Partidas)
                        partidas.Add(Tuple.Create(p.Wbs, p.Codigo ?? "", p.Padre ?? "", p.Etapa ?? "", p.Partida ?? "", p.ImporteTotal));

                var avancesPartidas = new Dictionary<int, Tuple<double, double>>();
                if (resp?.Avances != null)
                    foreach (var a in resp.Avances)
                        avancesPartidas[a.Wbs] = Tuple.Create(a.AvancePorcentaje, a.MontoEjecutado ?? double.NaN);
                {

                    // Agrupar por Codigo numérico (NO por Padre alfabético)
                    // ? Usar un diccionario ordenado por Codigo numérico
                    var categoriasDict = new SortedDictionary<int, NodoCategoria>();
                    var codigoToCategoria = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                    foreach (var partida in partidas)
                    {
                        string codigo = partida.Item2;
                        string padre = partida.Item3;
                        
                        // ? Convertir Codigo a entero para ordenar correctamente
                        int codigoNumerico = 999;
                        if (!string.IsNullOrEmpty(codigo) && int.TryParse(codigo, out int codigoInt))
                        {
                            codigoNumerico = codigoInt;
                        }
                        else if (!string.IsNullOrEmpty(padre))
                        {
                            // Si no hay Codigo, generar uno basado en el Padre usando el diccionario estándar
                            var ordenCategorias = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "Preliminares", 1 },
                                { "Cimentación", 2 },
                                { "Cimentacion", 2 },
                                { "Estructura", 3 },
                                { "Losas", 3 },
                                { "Muros", 3 },
                                { "Ins. Hidraulica", 4 },
                                { "Inst. Hidraulica", 4 },
                                { "Instalacion Hidraulica", 4 },
                                { "Inst. Sanitaria", 4 },
                                { "Instalacion Sanitaria", 4 },
                                { "Gas LP", 4 },
                                { "Inst. Electrica", 5 },
                                { "Instalacion Electrica", 5 },
                                { "Albañilería", 6 },
                                { "Albanileria", 6 },
                                { "Acabados", 7 },
                                { "Acabados Exteriores", 7 },
                                { "Acabados Interiores", 7 },
                                { "Herrería, Aluminio y Vidrio", 8 },
                                { "Herreria, Aluminio y Vidrio", 8 },
                                { "Carpintería", 9 },
                                { "Carpinteria", 9 },
                                { "Cerrajería", 9 },
                                { "Cerrajeria", 9 },
                                { "Carpinteria y Cerrajeria", 9 },
                                { "Muebles", 10 },
                                { "Accesorios", 10 },
                                { "Muebles y Accesorios", 10 },
                                { "Inst especiales y Obra Exterior", 11 },
                                { "Limpieza e Instalaciones especiales", 11 },
                                { "Obra Exterior", 11 },
                                { "Urbanización", 12 },
                                { "Urbanizacion", 12 }
                            };
                            
                            if (ordenCategorias.TryGetValue(padre, out int ordenPadre))
                            {
                                codigoNumerico = ordenPadre;
                            }
                        }
                        
                        // Crear o recuperar categoría
                        if (!categoriasDict.ContainsKey(codigoNumerico))
                        {
                            categoriasDict[codigoNumerico] = new NodoCategoria
                            {
                                EsCategoria = true,
                                Nombre = padre,
                                Codigo = codigoNumerico.ToString(),
                                ImporteTotal = 0,
                                AvancePorcentaje = 0,
                                ImporteEjecutado = 0,
                                Partidas = new List<NodoCategoria>()
                            };
                            codigoToCategoria[padre] = codigoNumerico;
                        }

                        // Crear nodo partida
                        var nodoPartida = new NodoCategoria
                        {
                            EsCategoria = false,
                            WBS = partida.Item1,
                            Codigo = codigo,
                            Nombre = $"{partida.Item4} > {partida.Item5}",
                            Padre = padre,
                            Etapa = partida.Item4,
                            Partida = partida.Item5,
                            ImporteTotal = partida.Item6,
                            AvancePorcentaje = 0,
                            ImporteEjecutado = 0
                        };

                        // Aplicar avance guardado si existe
                        if (avancesPartidas.TryGetValue(partida.Item1, out var avance))
                        {
                            nodoPartida.AvancePorcentaje = avance.Item1;
                            nodoPartida.ImporteEjecutado = !double.IsNaN(avance.Item2) 
                                ? avance.Item2 
                                : nodoPartida.ImporteTotal * (nodoPartida.AvancePorcentaje / 100.0);
                        }

                        categoriasDict[codigoNumerico].Partidas.Add(nodoPartida);
                    }

                    // Calcular totales por categoría
                    foreach (var categoria in categoriasDict.Values)
                    {
                        RecalcularAvanceCategoria(categoria);
                    }

                    // ? Las categorías ya están ordenadas numéricamente (1-12) gracias al SortedDictionary
                    nodosRaiz = categoriasDict.Values.ToList();
                    
                }

                // Establecer raíces y COLAPSAR todos los nodos
                olvAvance.Roots = nodosRaiz;
                olvAvance.CollapseAll();

                ActualizarTotales();
                DibujarGraficas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar avance: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarAvancePartidaEnBD(NodoCategoria partida)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null) return;

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            try
            {
                // El servidor hace el upsert en AvanceManualObra y recalcula el
                // avance por concepto (AvanceManualConcepto) en la misma llamada.
                ApiClient.Post("/api/avances/partida", new
                {
                    Manzana = manzana,
                    Lote = lote,
                    Prototipo = prototipoActual,
                    Wbs = partida.WBS,
                    AvancePorcentaje = partida.AvancePorcentaje,
                    Concepto = partida.Padre ?? "",
                    ImporteTotal = partida.ImporteTotal,
                    ImporteEjecutado = partida.ImporteEjecutado
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar avance: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarTotales()
        {
            double totalPresupuestado = nodosRaiz.Sum(i => i.ImporteTotal);
            double totalEjecutado = nodosRaiz.Sum(i => i.ImporteEjecutado);
            double porEjecutar = totalPresupuestado - totalEjecutado;
            double avanceGeneral = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;

            lblTotalPresupuestado.Text = $"Total Presupuestado: {totalPresupuestado:C2}";
            lblTotalPresupuestado.ForeColor = Color.FromArgb(52, 73, 94);
            
            lblTotalEjecutado.Text = $"Total Ejecutado: {totalEjecutado:C2}";
            lblTotalEjecutado.ForeColor = Color.FromArgb(46, 204, 113);
            
            lblPorEjecutar.Text = $"Por Ejecutar: {porEjecutar:C2}";
            lblPorEjecutar.ForeColor = Color.FromArgb(231, 76, 60);
            
            lblAvanceGeneral.Text = $"Avance General: {avanceGeneral:F1}%";
            lblAvanceGeneral.ForeColor = Color.FromArgb(52, 73, 94);

            progressBarAvance.Value = Math.Min(100, (int)avanceGeneral);

            // Contar partidas (no categorías)
            var todasPartidas = nodosRaiz.SelectMany(c => c.Partidas).ToList();
            int completadas = todasPartidas.Count(i => i.AvancePorcentaje >= 100);
            int enProgreso = todasPartidas.Count(i => i.AvancePorcentaje > 0 && i.AvancePorcentaje < 100);
            int sinIniciar = todasPartidas.Count(i => i.AvancePorcentaje == 0);

            lblEstadisticas.Text = $"Completados: {completadas} | En Progreso: {enProgreso} | Sin Iniciar: {sinIniciar}";
            lblEstadisticas.ForeColor = Color.FromArgb(127, 140, 141);
        }

        private void DibujarGraficas()
        {
            if (nodosRaiz.Count == 0) return;

            int width = pictureBoxGrafica.Width;
            int height = pictureBoxGrafica.Height;

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                DibujarGraficaPastel(g, new Rectangle(20, 20, 180, 180));
                DibujarGraficaBarras(g, new Rectangle(20, 220, width - 40, height - 240));
            }

            pictureBoxGrafica.Image?.Dispose();
            pictureBoxGrafica.Image = bmp;
        }

        private void DibujarGraficaPastel(Graphics g, Rectangle rect)
        {
            double totalPresupuestado = nodosRaiz.Sum(i => i.ImporteTotal);
            double totalEjecutado = nodosRaiz.Sum(i => i.ImporteEjecutado);
            double avanceGeneral = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;

            float anguloEjecutado = (float)(avanceGeneral * 3.6);
            float anguloRestante = 360 - anguloEjecutado;

            // Usar color principal del tema
            using (Brush brushEjecutado = new SolidBrush(ThemeManager.ColorPrincipal))
            {
                g.FillPie(brushEjecutado, rect, 0, anguloEjecutado);
            }

            using (Brush brushRestante = new SolidBrush(Color.FromArgb(220, 220, 220)))
            {
                g.FillPie(brushRestante, rect, anguloEjecutado, anguloRestante);
            }

            using (Pen pen = new Pen(ThemeManager.ColorSecundario, 2))
            {
                g.DrawEllipse(pen, rect);
            }

            string textoAvance = $"{avanceGeneral:F1}%";
            using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(ThemeManager.ColorTextoOscuro))
            {
                SizeF textSize = g.MeasureString(textoAvance, font);
                PointF textPos = new PointF(rect.X + (rect.Width - textSize.Width) / 2, rect.Y + (rect.Height - textSize.Height) / 2);
                g.DrawString(textoAvance, font, textBrush, textPos);
            }

            using (Font fontLeyenda = new Font("Segoe UI", 9))
            using (Brush textBrush = new SolidBrush(ThemeManager.ColorTextoOscuro))
            {
                g.FillRectangle(new SolidBrush(ThemeManager.ColorPrincipal), 220, 40, 15, 15);
                g.DrawString("Ejecutado", fontLeyenda, textBrush, 240, 38);
                g.FillRectangle(new SolidBrush(Color.FromArgb(220, 220, 220)), 220, 65, 15, 15);
                g.DrawString("Restante", fontLeyenda, textBrush, 240, 63);
            }
        }

        private void DibujarGraficaBarras(Graphics g, Rectangle rect)
        {
            // Las categorías ya están en nodosRaiz
            var categorias = nodosRaiz
                .Select(c => new
                {
                    Categoria = c.Nombre,
                    Total = c.ImporteTotal,
                    Ejecutado = c.ImporteEjecutado
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            if (categorias.Count == 0) return;

            double maxValor = categorias.Max(c => c.Total);
            if (maxValor == 0) return;

            int barWidth = Math.Max(40, (rect.Width - 60) / categorias.Count);
            int spacing = 10;

            using (Font font = new Font("Segoe UI", 7))
            using (Brush textBrush = new SolidBrush(ThemeManager.ColorTextoOscuro))
            using (Pen gridPen = new Pen(Color.LightGray, 1))
            {
                for (int i = 0; i <= 5; i++)
                {
                    int y = rect.Y + (rect.Height * i / 5);
                    g.DrawLine(gridPen, rect.X, y, rect.Right, y);
                }

                int x = rect.X + 30;
                foreach (var cat in categorias)
                {
                    int alturaTotal = (int)((cat.Total / maxValor) * (rect.Height - 60));
                    int alturaEjecutado = (int)((cat.Ejecutado / maxValor) * (rect.Height - 60));
                    int yBase = rect.Bottom - 40;

                    using (Brush brushFondo = new SolidBrush(Color.FromArgb(220, 220, 220)))
                    {
                        g.FillRectangle(brushFondo, x, yBase - alturaTotal, barWidth - spacing, alturaTotal);
                    }

                    // Usar color principal para la barra ejecutada
                    using (Brush brushEjecutado = new SolidBrush(ThemeManager.ColorPrincipal))
                    {
                        g.FillRectangle(brushEjecutado, x, yBase - alturaEjecutado, barWidth - spacing, alturaEjecutado);
                    }

                    string etiqueta = cat.Categoria;
                    if (etiqueta.Length > 15)
                        etiqueta = etiqueta.Substring(0, 12) + "...";

                    g.TranslateTransform(x + (barWidth - spacing) / 2, yBase + 5);
                    g.RotateTransform(-45);
                    g.DrawString(etiqueta, font, textBrush, 0, 0);
                    g.ResetTransform();

                    if (cat.Total > 0)
                    {
                        double porcentaje = (cat.Ejecutado / cat.Total) * 100;
                        string textoPorcentaje = $"{porcentaje:F0}%";
                        SizeF textSize = g.MeasureString(textoPorcentaje, font);

                        using (Font fontPorcentaje = new Font("Segoe UI", 8, FontStyle.Bold))
                        {
                            g.DrawString(textoPorcentaje, fontPorcentaje, textBrush,
                                x + (barWidth - spacing - textSize.Width) / 2,
                                yBase - alturaTotal - 15);
                        }
                    }

                    x += barWidth;
                }
            }
        }

        private void btnExportarPDF_Click(object sender, EventArgs e)
        {
            if (nodosRaiz.Count == 0)
            {
                MessageBox.Show("Primero carga el avance de una casa", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = $"AvanceObra_M{cmbManzana.SelectedItem}_L{cmbLote.SelectedItem}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GenerarPDFAvance(sfd.FileName);
                    MessageBox.Show("PDF generado exitosamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var result = MessageBox.Show("¿Deseas abrir el PDF?", "Abrir PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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

        private void GenerarPDFAvance(string rutaPdf)
        {
            // Método simplificado que solo genera un PDF básico
            // Puedes expandirlo según necesites
            PdfDocument pdf = new PdfDocument();
            PdfPage page = pdf.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 12);

            gfx.DrawString($"Avance de Obra - M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem}", 
                font, XBrushes.Black, new XRect(0, 20, page.Width, 30), XStringFormats.TopCenter);

            double y = 60;
            foreach (var categoria in nodosRaiz)
            {
                gfx.DrawString($"{categoria.Nombre}: {categoria.AvancePorcentaje:F1}%", 
                    font, XBrushes.Black, 40, y);
                y += 20;
            }

            pdf.Save(rutaPdf);
        }

        private void FormAvanceObra_Resize(object sender, EventArgs e)
        {
            if (nodosRaiz.Count > 0)
            {
                DibujarGraficas();
            }
        }

        private static string NormalizeForComparison(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            string normalized = input.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (char c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant();
        }

        public void ShowEvidenciaPreview(Image image)
        {
            if (pictureBoxGrafica == null) return;

            if (pictureBoxGrafica.InvokeRequired)
            {
                pictureBoxGrafica.Invoke(new Action(() => ShowEvidenciaPreview(image)));
                return;
            }

            try
            {
                if (pictureBoxGrafica.Image != null)
                {
                    try { pictureBoxGrafica.Image.Dispose(); } catch { }
                    pictureBoxGrafica.Image = null;
                }

                if (image != null)
                {
                    pictureBoxGrafica.Image = new Bitmap(image);
                    pictureBoxGrafica.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (pictureBoxGrafica != null && pictureBoxGrafica.Image != null)
                {
                    try { pictureBoxGrafica.Image.Dispose(); } catch { }
                    pictureBoxGrafica.Image = null;
                }
            }
            catch { }

            base.OnFormClosing(e);
        }
    }
}
