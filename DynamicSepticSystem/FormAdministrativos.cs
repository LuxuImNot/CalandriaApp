using BrightIdeasSoftware;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Consolidado financiero por casa. Layout HUD: detalle (tabs) a la izquierda
    /// y panel-resumen con tarjetas Estimaciones / Destajos (M.O. + Material) a la derecha.
    /// </summary>
    public partial class FormAdministrativos : Form
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private readonly CultureInfo culturaMx = CultureInfo.GetCultureInfo("es-MX");

        private string manzanaActual = "";
        private string loteActual = "";
        private string prototipoActual = "";

        private readonly List<RegistroEstimacion> registrosEstimacion = new List<RegistroEstimacion>();
        private readonly List<RegistroDestajo> registrosDestajo = new List<RegistroDestajo>();

        private decimal totalEstimaciones = 0m;
        private decimal totalDestajos = 0m;
        private decimal totalManoObra = 0m;
        private decimal totalMaterial = 0m;
        private int conteoMO = 0;
        private int conteoMat = 0;

        public FormAdministrativos()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            ConfigurarOlvEstimaciones();
            ConfigurarOlvDestajos();
            this.Load += FormAdministrativos_Load;
            this.cmbManzana.SelectedIndexChanged += CmbManzana_SelectedIndexChanged;
        }

        private void FormAdministrativos_Load(object sender, EventArgs e)
        {
            CargarManzanas();
            ResetBarrasMOMat();
        }

        #region UI helpers (borde de tarjetas + barras)

        private void PintarBordeCard(object sender, PaintEventArgs e)
        {
            var p = sender as Panel;
            if (p == null) return;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using (var pen = new Pen(Color.FromArgb(220, 210, 195)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            }
            // Acento café arriba
            using (var brush = new SolidBrush(Color.FromArgb(179, 108, 46)))
            {
                e.Graphics.FillRectangle(brush, 0, 0, p.Width, 3);
            }
        }

        private void PintarBordeSubcardVerde(object sender, PaintEventArgs e)
        {
            PintarBordeColor(sender, e, Color.FromArgb(176, 220, 184));
        }

        private void PintarBordeSubcardRojo(object sender, PaintEventArgs e)
        {
            PintarBordeColor(sender, e, Color.FromArgb(235, 195, 190));
        }

        private void PintarBordeColor(object sender, PaintEventArgs e, Color color)
        {
            var p = sender as Panel;
            if (p == null) return;
            using (var pen = new Pen(color))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            }
        }

        private void ResetBarrasMOMat()
        {
            // Barras horizontales proporcionales: por defecto ambas al 50/50 vacías
            barraMO.Width = (int)((cardMO.ClientSize.Width - cardMO.Padding.Horizontal) * 0.5);
            barraMat.Width = (int)((cardMat.ClientSize.Width - cardMat.Padding.Horizontal) * 0.5);
        }

        // -- Stepper del proceso de destajo (6 pasos) -----------------------
        // Refuerza visualmente que cada destajo recorre un flujo:
        //   1 Identificar · 2 Clasificar · 3 Cuantificar · 4 Asignar · 5 Ejecutar · 6 Cerrar
        // Las columnas del OLV de destajos están numeradas con el mismo orden.
        //   Columnas (qué se muestra)
        //   Disparador (cuándo se llena)
        //   Regla    (cómo se computa el dinero)
        private static readonly (string Titulo, string Columnas, string Disparador, string Regla)[] PasosDestajo =
        {
            ("IDENTIFICAR",
             "ID · Destajo / Tarea",
             "Al activar la tarea en la ruta crítica",
             "ActivacionTareasRuta + Ruta(Tunera/Calandra)"),

            ("CLASIFICAR",
             "Tipo: M.O. o Material",
             "Definido al crear el nodo en FormEditorTreeList",
             "Determina la regla de gasto reconocido"),

            ("CUANTIFICAR",
             "Cant. × P. Unit. = Importe",
             "Capturado en columnas personalizadas del nodo",
             "Importe = compromiso económico del destajo"),

            ("ASIGNAR",
             "Cuadrilla responsable",
             "Desde FormAsignarCuadrilla",
             "Habilita el pago de M.O. vía nómina"),

            ("EJECUTAR",
             "Gastado vs. comprometido",
             "M.O.: al asignar nómina · Material: al finalizar",
             "Suma NominaTareasAsignada / Importe finalizado"),

            ("CERRAR",
             "Estado: Activado → Con cuadrilla → Finalizado",
             "Al marcar Finalizado y registrar Fecha Fin.",
             "Material: libera el gasto del Importe"),
        };

        private void panelStepperDestajos_Resize(object sender, EventArgs e)
        {
            panelStepperDestajos.Invalidate();
        }

        private void PintarStepperDestajos(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var p = (Panel)sender;
            var area = new Rectangle(
                p.Padding.Left, p.Padding.Top,
                p.ClientSize.Width  - p.Padding.Horizontal,
                p.ClientSize.Height - p.Padding.Vertical);

            var brushCafeOscuro = new SolidBrush(Color.FromArgb(88, 53, 23));
            var brushCafe       = new SolidBrush(Color.FromArgb(179, 108, 46));
            var brushBlanco     = Brushes.White;
            var brushTitulo     = new SolidBrush(Color.FromArgb(60, 36, 15));
            var brushColumnas   = new SolidBrush(Color.FromArgb(88, 53, 23));
            var brushDisparador = new SolidBrush(Color.FromArgb(117, 117, 117));
            var brushRegla      = new SolidBrush(Color.FromArgb(155, 110, 70));
            var brushHeader     = new SolidBrush(Color.FromArgb(60, 36, 15));
            var brushHeaderSub  = new SolidBrush(Color.FromArgb(117, 117, 117));
            var penConector     = new Pen(Color.FromArgb(200, 175, 140), 2f);
            var penSeparador    = new Pen(Color.FromArgb(220, 210, 195), 1f);

            var fontHeader     = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            var fontHeaderSub  = new Font("Segoe UI", 8F, FontStyle.Italic);
            var fontNum        = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            var fontTitulo     = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            var fontColumnas   = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold);
            var fontDisparador = new Font("Segoe UI", 7.25F);
            var fontRegla      = new Font("Segoe UI", 7.25F, FontStyle.Italic);
            var sfCentro       = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            var sfIzquierda    = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            try
            {
                // === Header del stepper ===
                var rectHeader = new RectangleF(area.Left, area.Top, area.Width, 16);
                g.DrawString("CICLO DE VIDA DE UN DESTAJO",
                    fontHeader, brushHeader, rectHeader, sfIzquierda);

                var rectHeaderSub = new RectangleF(area.Left, area.Top + 16, area.Width, 14);
                g.DrawString("De la activación de la tarea al gasto reconocido y cierre — el mismo orden se refleja en las columnas (1·…6·)",
                    fontHeaderSub, brushHeaderSub, rectHeaderSub, sfIzquierda);

                int yPasos = area.Top + 34;
                g.DrawLine(penSeparador, area.Left, yPasos - 4, area.Right, yPasos - 4);

                // === Pasos ===
                int n = PasosDestajo.Length;
                float slot = (float)area.Width / n;
                const int circuloDiam = 26;
                int yCirculo = yPasos;

                for (int i = 0; i < n; i++)
                {
                    float cx = area.Left + slot * (i + 0.5f);
                    int xCirculo = (int)cx - circuloDiam / 2;

                    // Conector hacia el siguiente paso
                    if (i < n - 1)
                    {
                        float xIni = cx + circuloDiam / 2f + 4;
                        float xFin = area.Left + slot * (i + 1.5f) - circuloDiam / 2f - 4;
                        float yLin = yCirculo + circuloDiam / 2f;
                        g.DrawLine(penConector, xIni, yLin, xFin - 8, yLin);
                        var flecha = new[]
                        {
                            new PointF(xFin, yLin),
                            new PointF(xFin - 8, yLin - 4),
                            new PointF(xFin - 8, yLin + 4),
                        };
                        g.FillPolygon(brushCafe, flecha);
                    }

                    // Círculo numerado
                    var rectCirc = new Rectangle(xCirculo, yCirculo, circuloDiam, circuloDiam);
                    g.FillEllipse(brushCafeOscuro, rectCirc);
                    g.DrawString((i + 1).ToString(), fontNum, brushBlanco, rectCirc, sfCentro);

                    // Título del paso
                    float yTexto = yCirculo + circuloDiam + 2;
                    var rectTit = new RectangleF(area.Left + slot * i, yTexto, slot, 13);
                    g.DrawString(PasosDestajo[i].Titulo, fontTitulo, brushTitulo, rectTit, sfCentro);

                    // Columnas asociadas
                    var rectCol = new RectangleF(area.Left + slot * i, yTexto + 13, slot, 12);
                    g.DrawString(PasosDestajo[i].Columnas, fontColumnas, brushColumnas, rectCol, sfCentro);

                    // Disparador (cuándo)
                    var rectDisp = new RectangleF(area.Left + slot * i, yTexto + 25, slot, 12);
                    g.DrawString(PasosDestajo[i].Disparador, fontDisparador, brushDisparador, rectDisp, sfCentro);

                    // Regla (cómo)
                    var rectReg = new RectangleF(area.Left + slot * i, yTexto + 37, slot, 12);
                    g.DrawString(PasosDestajo[i].Regla, fontRegla, brushRegla, rectReg, sfCentro);
                }
            }
            finally
            {
                brushCafeOscuro.Dispose();
                brushCafe.Dispose();
                brushTitulo.Dispose();
                brushColumnas.Dispose();
                brushDisparador.Dispose();
                brushRegla.Dispose();
                brushHeader.Dispose();
                brushHeaderSub.Dispose();
                penConector.Dispose();
                penSeparador.Dispose();
                fontHeader.Dispose();
                fontHeaderSub.Dispose();
                fontNum.Dispose();
                fontTitulo.Dispose();
                fontColumnas.Dispose();
                fontDisparador.Dispose();
                fontRegla.Dispose();
                sfCentro.Dispose();
                sfIzquierda.Dispose();
            }
        }

        #endregion

        #region Configuración de ObjectListView (Estimaciones agrupado por Etapa)

        private void ConfigurarOlvEstimaciones()
        {
            olvEstimaciones.UseCellFormatEvents = true;
            olvEstimaciones.FormatRow += OlvEstimaciones_FormatRow;
            olvEstimaciones.FormatCell += OlvEstimaciones_FormatCell;

            // Agrupar por Etapa
            olvEstimaciones.AlwaysGroupByColumn = colEtapa;
            olvEstimaciones.ShowGroups = true;

            // Desactivar el sort interno: respetamos el orden con que vienen los datos
            olvEstimaciones.Sorting = System.Windows.Forms.SortOrder.None;
            olvEstimaciones.SortGroupItemsByPrimaryColumn = false;
            olvEstimaciones.ShowSortIndicators = false;
            olvEstimaciones.CustomSorter = (col, order) => { /* no-op */ };

            // Group key SIEMPRE string no nulo
            colEtapa.GroupKeyGetter = (object rowObject) =>
            {
                var r = rowObject as RegistroEstimacion;
                if (r == null) return "(Sin etapa)";
                return string.IsNullOrEmpty(r.Etapa) ? "(Sin etapa)" : r.Etapa;
            };
            colEtapa.GroupKeyToTitleConverter = (object groupKey) =>
            {
                string etapa = groupKey == null ? "(Sin etapa)" : groupKey.ToString();
                if (string.IsNullOrEmpty(etapa)) etapa = "(Sin etapa)";
                int conteo = 0;
                decimal subtotal = 0m;
                foreach (var r in registrosEstimacion)
                {
                    string clave = string.IsNullOrEmpty(r.Etapa) ? "(Sin etapa)" : r.Etapa;
                    if (clave == etapa)
                    {
                        conteo++;
                        subtotal += r.MontoEjecutado;
                    }
                }
                return $"{etapa}   ·   {conteo} concepto(s)   ·   Subtotal: {subtotal.ToString("C2", culturaMx)}";
            };

            // AspectGetters: NUNCA null y SIEMPRE el mismo tipo por columna
            colWBS.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroEstimacion;
                return r == null ? "" : (r.WBS ?? "");
            };
            colCodigo.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroEstimacion;
                return r == null ? "" : (r.Codigo ?? "");
            };
            colPartida.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroEstimacion;
                return r == null ? "" : (r.Partida ?? "");
            };
            colAvance.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroEstimacion;
                return r == null ? 0m : r.AvancePorcentaje;
            };
            colAvance.AspectToStringConverter = (cellValue) =>
            {
                if (cellValue is decimal d) return d.ToString("N2") + " %";
                return cellValue == null ? "" : cellValue.ToString();
            };
            colMontoEjecutado.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroEstimacion;
                return r == null ? 0m : r.MontoEjecutado;
            };
            colMontoEjecutado.AspectToStringConverter = (cellValue) =>
            {
                if (cellValue is decimal d) return d.ToString("C2", culturaMx);
                return cellValue == null ? "" : cellValue.ToString();
            };
            // Devolvemos siempre string para evitar líos de comparación con DateTime?
            colFechaFin.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroEstimacion;
                if (r == null || !r.FechaFinalizacion.HasValue) return "—";
                return r.FechaFinalizacion.Value.ToString("dd/MM/yyyy");
            };

            // Header style
            var headerStyle = new HeaderFormatStyle();
            headerStyle.Normal.BackColor = Color.FromArgb(88, 53, 23);
            headerStyle.Normal.ForeColor = Color.White;
            headerStyle.Normal.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            headerStyle.Hot.BackColor = Color.FromArgb(120, 75, 35);
            headerStyle.Hot.ForeColor = Color.White;
            olvEstimaciones.HeaderFormatStyle = headerStyle;
            olvEstimaciones.HeaderUsesThemes = false;
        }

        private void OlvEstimaciones_FormatRow(object sender, FormatRowEventArgs e)
        {
            var reg = e.Model as RegistroEstimacion;
            if (reg == null) return;
            if (reg.AvancePorcentaje >= 100m)
            {
                e.Item.BackColor = Color.FromArgb(232, 245, 233);
            }
        }

        private void OlvEstimaciones_FormatCell(object sender, FormatCellEventArgs e)
        {
            if (e.Column == colMontoEjecutado)
            {
                e.SubItem.ForeColor = Color.FromArgb(46, 134, 75);
                e.SubItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            }
        }

        #endregion

        #region Configuración del ObjectListView (Destajos agrupado por Categoría)

        private void ConfigurarOlvDestajos()
        {
            olvDestajos.UseCellFormatEvents = true;
            olvDestajos.FormatRow += OlvDestajos_FormatRow;
            olvDestajos.FormatCell += OlvDestajos_FormatCell;

            olvDestajos.AlwaysGroupByColumn = colDesCategoria;
            olvDestajos.ShowGroups = true;
            olvDestajos.Sorting = System.Windows.Forms.SortOrder.None;
            olvDestajos.SortGroupItemsByPrimaryColumn = false;
            olvDestajos.ShowSortIndicators = false;
            olvDestajos.CustomSorter = (col, order) => { /* no-op */ };

            colDesCategoria.GroupKeyGetter = (object rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                if (r == null) return "(Sin categoría)";
                return string.IsNullOrEmpty(r.Categoria) ? "(Sin categoría)" : r.Categoria;
            };
            colDesCategoria.GroupKeyToTitleConverter = (object groupKey) =>
            {
                string cat = groupKey == null ? "(Sin categoría)" : groupKey.ToString();
                if (string.IsNullOrEmpty(cat)) cat = "(Sin categoría)";

                int conteoMO_ = 0, conteoMat_ = 0;
                decimal gastoMO = 0m, gastoMat = 0m, total = 0m;
                foreach (var r in registrosDestajo)
                {
                    string clave = string.IsNullOrEmpty(r.Categoria) ? "(Sin categoría)" : r.Categoria;
                    if (clave != cat) continue;
                    total += r.Importe;
                    if (r.Tipo == TipoTarea.ManoDeObra)
                    {
                        conteoMO_++;
                        gastoMO += r.MontoGastado;
                    }
                    else if (r.Tipo == TipoTarea.Material)
                    {
                        conteoMat_++;
                        if (r.Finalizado) gastoMat += r.Importe;
                    }
                }
                return $"{cat}   ·   M.O.: {gastoMO.ToString("C2", culturaMx)} ({conteoMO_})   ·   Material: {gastoMat.ToString("C2", culturaMx)} ({conteoMat_})";
            };

            // AspectGetters no-null y de tipo consistente
            colDesID.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                return r == null ? 0 : r.NodoID;
            };
            colDesTipo.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                return r == null ? "" : TipoTareaTexto(r.Tipo);
            };
            colDesTarea.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                return r == null ? "" : (r.Nombre ?? "");
            };
            colDesCantidad.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                return r == null ? 0m : r.Cantidad;
            };
            colDesCantidad.AspectToStringConverter = (cv) =>
            {
                if (cv is decimal d) return d.ToString("N2");
                return cv == null ? "" : cv.ToString();
            };
            colDesUnidad.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                return r == null ? "" : (r.Unidad ?? "");
            };
            colDesPrecioUnitario.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                return r == null ? 0m : r.PrecioUnitario;
            };
            colDesPrecioUnitario.AspectToStringConverter = (cv) =>
            {
                if (cv is decimal d) return d.ToString("C2", culturaMx);
                return cv == null ? "" : cv.ToString();
            };
            colDesImporte.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                return r == null ? 0m : r.Importe;
            };
            colDesImporte.AspectToStringConverter = (cv) =>
            {
                if (cv is decimal d) return d.ToString("C2", culturaMx);
                return cv == null ? "" : cv.ToString();
            };
            colDesGastado.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                return r == null ? 0m : r.MontoGastado;
            };
            colDesGastado.AspectToStringConverter = (cv) =>
            {
                if (cv is decimal d) return d == 0m ? "—" : d.ToString("C2", culturaMx);
                return cv == null ? "—" : cv.ToString();
            };
            colDesCuadrilla.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                return r == null ? "" : (r.Cuadrilla ?? "");
            };
            colDesEstado.AspectGetter = (rowObject) =>
            {
                var r = rowObject as RegistroDestajo;
                if (r == null) return "";
                return r.EstadoTexto;
            };

            var headerStyle = new HeaderFormatStyle();
            headerStyle.Normal.BackColor = Color.FromArgb(88, 53, 23);
            headerStyle.Normal.ForeColor = Color.White;
            headerStyle.Normal.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            headerStyle.Hot.BackColor = Color.FromArgb(120, 75, 35);
            headerStyle.Hot.ForeColor = Color.White;
            olvDestajos.HeaderFormatStyle = headerStyle;
            olvDestajos.HeaderUsesThemes = false;
        }

        private void OlvDestajos_FormatRow(object sender, FormatRowEventArgs e)
        {
            var r = e.Model as RegistroDestajo;
            if (r == null) return;

            // Resalta filas con gasto computado (M.O. con nómina o Material finalizado)
            if (r.GastoComputado)
                e.Item.BackColor = Color.FromArgb(232, 245, 233);
            else if (r.Finalizado)
                e.Item.BackColor = Color.FromArgb(248, 252, 248);
            else
                e.Item.BackColor = Color.FromArgb(255, 250, 230);
        }

        private void OlvDestajos_FormatCell(object sender, FormatCellEventArgs e)
        {
            var r = e.Model as RegistroDestajo;
            if (r == null) return;

            if (e.Column == colDesTipo)
            {
                if (r.Tipo == TipoTarea.ManoDeObra)
                {
                    e.SubItem.ForeColor = Color.FromArgb(33, 99, 50);
                    e.SubItem.BackColor = Color.FromArgb(216, 239, 219);
                    e.SubItem.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
                }
                else if (r.Tipo == TipoTarea.Material)
                {
                    e.SubItem.ForeColor = Color.FromArgb(155, 41, 28);
                    e.SubItem.BackColor = Color.FromArgb(248, 220, 215);
                    e.SubItem.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
                }
                else
                {
                    e.SubItem.ForeColor = Color.FromArgb(117, 117, 117);
                }
            }
            else if (e.Column == colDesImporte)
            {
                e.SubItem.ForeColor = Color.FromArgb(46, 134, 75);
                e.SubItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            }
            else if (e.Column == colDesGastado)
            {
                if (r.GastoComputado)
                {
                    e.SubItem.ForeColor = Color.FromArgb(46, 134, 75);
                    e.SubItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                }
                else
                {
                    e.SubItem.ForeColor = Color.FromArgb(170, 170, 170);
                }
            }
            else if (e.Column == colDesEstado)
            {
                if (r.Finalizado)
                {
                    e.SubItem.ForeColor = Color.FromArgb(33, 99, 50);
                    e.SubItem.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
                }
                else if (!string.IsNullOrEmpty(r.Cuadrilla))
                {
                    e.SubItem.ForeColor = Color.FromArgb(179, 108, 46);
                }
                else
                {
                    e.SubItem.ForeColor = Color.FromArgb(117, 117, 117);
                }
            }
        }

        #endregion

        #region Carga de filtros

        private void CargarManzanas()
        {
            try
            {
                cmbManzana.Items.Clear();
                cmbLote.Items.Clear();

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            cmbManzana.Items.Add(reader["Manzana"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No fue posible cargar Manzanas: " + ex.Message,
                    "Administrativos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbManzana_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbLote.Items.Clear();
            if (cmbManzana.SelectedItem == null) return;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT DISTINCT Lote FROM InventarioCasas WHERE Manzana = @m ORDER BY Lote", conn))
                    {
                        cmd.Parameters.AddWithValue("@m", cmbManzana.SelectedItem.ToString());
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                cmbLote.Items.Add(reader["Lote"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No fue posible cargar Lotes: " + ex.Message,
                    "Administrativos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Consulta

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Seleccione Manzana y Lote para consultar.",
                    "Administrativos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            manzanaActual = cmbManzana.SelectedItem.ToString();
            loteActual = cmbLote.SelectedItem.ToString();
            prototipoActual = ObtenerPrototipo(manzanaActual, loteActual);

            lblCasaInfo.Text = string.IsNullOrEmpty(prototipoActual)
                ? $"Casa: Mz {manzanaActual} · Lt {loteActual}"
                : $"Casa: Mz {manzanaActual} · Lt {loteActual}  ·  Prototipo: {prototipoActual}";

            Cursor = Cursors.WaitCursor;
            try
            {
                CargarEstimaciones();
                CargarDestajos();
                RefrescarHUD();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar: " + ex.Message,
                    "Administrativos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string ObtenerPrototipo(string manzana, string lote)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);
                        var r = cmd.ExecuteScalar();
                        return r == null || r == DBNull.Value ? "" : r.ToString();
                    }
                }
            }
            catch { return ""; }
        }

        private void CargarEstimaciones()
        {
            registrosEstimacion.Clear();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var colsAvance = LeerColumnas(conn, "AvanceManualObra");
                bool tieneFecha = colsAvance.Contains("FechaFinalizacion");

                var colsPres = LeerColumnas(conn, "PresupuestoObra");
                bool tieneCodigo = colsPres.Contains("Codigo");
                bool tieneEtapa = colsPres.Contains("Etapa");
                bool tienePartida = colsPres.Contains("Partida");

                var sb = new System.Text.StringBuilder();
                sb.Append("SELECT a.WBS, a.AvancePorcentaje, a.MontoEjecutado");
                if (tieneFecha) sb.Append(", a.FechaFinalizacion");
                if (tieneCodigo) sb.Append(", p.Codigo");
                if (tieneEtapa) sb.Append(", p.Etapa");
                if (tienePartida) sb.Append(", p.Partida");
                sb.Append(" FROM AvanceManualObra a ");
                sb.Append(" LEFT JOIN PresupuestoObra p ON TRY_CAST(a.WBS AS INT) = p.WBS_Correcto ");
                sb.Append(" WHERE a.Manzana = @m AND a.Lote = @l ");
                sb.Append(" ORDER BY ");
                if (tieneEtapa) sb.Append("p.Etapa, ");
                sb.Append("a.WBS");

                using (var cmd = new SqlCommand(sb.ToString(), conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzanaActual);
                    cmd.Parameters.AddWithValue("@l", loteActual);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var reg = new RegistroEstimacion
                            {
                                WBS = reader["WBS"]?.ToString() ?? "",
                                Codigo = tieneCodigo && reader["Codigo"] != DBNull.Value ? reader["Codigo"].ToString() : "",
                                Etapa = tieneEtapa && reader["Etapa"] != DBNull.Value ? reader["Etapa"].ToString() : "",
                                Partida = tienePartida && reader["Partida"] != DBNull.Value ? reader["Partida"].ToString() : "",
                                AvancePorcentaje = LeerDecimalSeguro(reader, "AvancePorcentaje"),
                                MontoEjecutado = LeerDecimalSeguro(reader, "MontoEjecutado"),
                                FechaFinalizacion = tieneFecha && reader["FechaFinalizacion"] != DBNull.Value
                                    ? (DateTime?)Convert.ToDateTime(reader["FechaFinalizacion"]) : null
                            };
                            registrosEstimacion.Add(reg);
                        }
                    }
                }
            }

            totalEstimaciones = registrosEstimacion.Sum(r => r.MontoEjecutado);

            olvEstimaciones.SetObjects(registrosEstimacion);
        }

        private void CargarDestajos()
        {
            registrosDestajo.Clear();
            totalDestajos = 0m;
            totalManoObra = 0m;
            totalMaterial = 0m;
            conteoMO = 0;
            conteoMat = 0;

            string ruta = !string.IsNullOrEmpty(prototipoActual) &&
                          prototipoActual.ToUpper().Contains("CALANDRA")
                ? "RutaCalandraDestajo"
                : "RutaTuneraDestajo";

            // 1) Cargar montos asignados de nómina por NodoID
            var nominaPorNodo = CargarMontosNomina(ruta);

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                if (!ExisteTabla(conn, "ActivacionTareasRuta")) return;
                if (!ExisteTabla(conn, ruta)) return;
                bool existeColumnasRuta = ExisteTabla(conn, ruta + "_Columnas");

                var colsRuta = LeerColumnas(conn, ruta);
                bool tieneTipoTarea = colsRuta.Contains("TipoTarea");

                var colsAct = LeerColumnas(conn, "ActivacionTareasRuta");
                bool tieneActFinalizado = colsAct.Contains("Finalizado");
                bool tieneActFechaFin = colsAct.Contains("FechaFinalizacion");
                bool tieneActCuadrilla = colsAct.Contains("CuadrillaAsignada");

                string tipoSelect = tieneTipoTarea ? "ISNULL(r.TipoTarea, 0) AS TipoTarea" : "0 AS TipoTarea";
                string tipoGroup = tieneTipoTarea ? ", r.TipoTarea" : "";
                string finSelect = tieneActFinalizado ? "ISNULL(a.Finalizado, 0) AS Finalizado" : "0 AS Finalizado";
                string fechaFinSelect = tieneActFechaFin ? "a.FechaFinalizacion" : "CAST(NULL AS DATETIME) AS FechaFinalizacion";
                string cuadrillaSelect = tieneActCuadrilla ? "a.CuadrillaAsignada" : "CAST(NULL AS NVARCHAR(20)) AS CuadrillaAsignada";

                string columnasJoin;
                string cantidadSelect, unidadSelect, precioSelect;
                if (existeColumnasRuta)
                {
                    columnasJoin = $"LEFT JOIN {ruta}_Columnas c ON r.ID = c.NodoID";
                    cantidadSelect = "ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad";
                    unidadSelect = "ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad'   THEN c.Valor END), '')  AS Unidad";
                    precioSelect = "ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS PrecioUnitario";
                }
                else
                {
                    columnasJoin = "";
                    cantidadSelect = "'0' AS Cantidad";
                    unidadSelect = "'' AS Unidad";
                    precioSelect = "'0' AS PrecioUnitario";
                }

                string groupBy = existeColumnasRuta
                    ? $@"GROUP BY a.NodoID, a.DesatajoActivado,
                                 {(tieneActFinalizado ? "a.Finalizado," : "")}
                                 {(tieneActCuadrilla ? "a.CuadrillaAsignada," : "")}
                                 {(tieneActFechaFin ? "a.FechaFinalizacion," : "")}
                                 r.Nombre, r.Descripcion{tipoGroup}"
                    : "";

                // Categoría = nombre del nodo padre (Sub-Padre) en la ruta
                bool tieneParent = colsRuta.Contains("ParentId");
                string categoriaSelect = tieneParent
                    ? "ISNULL(p_parent.Nombre, '') AS Categoria"
                    : "'' AS Categoria";
                string parentJoin = tieneParent
                    ? $"LEFT JOIN {ruta} p_parent ON p_parent.ID = r.ParentId"
                    : "";
                string categoriaGroup = tieneParent ? ", p_parent.Nombre" : "";

                string sql = $@"
                    SELECT  a.NodoID,
                            a.DesatajoActivado,
                            {finSelect},
                            {cuadrillaSelect},
                            {fechaFinSelect},
                            r.Nombre,
                            r.Descripcion,
                            {categoriaSelect},
                            {tipoSelect},
                            {cantidadSelect},
                            {unidadSelect},
                            {precioSelect}
                    FROM ActivacionTareasRuta a
                    INNER JOIN {ruta} r ON a.NodoID = r.ID
                    {parentJoin}
                    {columnasJoin}
                    WHERE a.Manzana = @m
                      AND a.Lote    = @l
                      AND a.Ruta    = @ruta
                      AND ISNULL(a.DesatajoActivado, 0) = 1
                    {(existeColumnasRuta ? groupBy + categoriaGroup : "")}
                    ORDER BY {(tieneParent ? "p_parent.Nombre, " : "")}r.Nombre";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzanaActual);
                    cmd.Parameters.AddWithValue("@l", loteActual);
                    cmd.Parameters.AddWithValue("@ruta", ruta);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal cantidad = ParseDecimalSeguro(reader["Cantidad"]?.ToString());
                            decimal precio = ParseDecimalSeguro(reader["PrecioUnitario"]?.ToString());
                            bool finalizado = reader["Finalizado"] != DBNull.Value && Convert.ToBoolean(reader["Finalizado"]);
                            TipoTarea tipo = TipoTarea.Ninguno;
                            if (reader["TipoTarea"] != DBNull.Value)
                                tipo = (TipoTarea)Convert.ToInt32(reader["TipoTarea"]);

                            int nodoId = reader["NodoID"] != DBNull.Value ? Convert.ToInt32(reader["NodoID"]) : 0;
                            decimal nominaMonto = 0m;
                            nominaPorNodo.TryGetValue(nodoId, out nominaMonto);

                            string categoria = "";
                            try { categoria = reader["Categoria"]?.ToString() ?? ""; }
                            catch { categoria = ""; }

                            var reg = new RegistroDestajo
                            {
                                NodoID = nodoId,
                                Nombre = reader["Nombre"]?.ToString() ?? "",
                                Descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : "",
                                Categoria = categoria,
                                Cantidad = cantidad,
                                Unidad = reader["Unidad"]?.ToString() ?? "",
                                PrecioUnitario = precio,
                                Cuadrilla = reader["CuadrillaAsignada"] != DBNull.Value ? reader["CuadrillaAsignada"].ToString() : "",
                                Finalizado = finalizado,
                                FechaFinalizacion = reader["FechaFinalizacion"] != DBNull.Value
                                    ? (DateTime?)Convert.ToDateTime(reader["FechaFinalizacion"]) : null,
                                Tipo = tipo,
                                NominaAsignada = nominaMonto
                            };
                            registrosDestajo.Add(reg);
                        }
                    }
                }
            }

            foreach (var r in registrosDestajo)
            {
                totalDestajos += r.Importe;
                if (r.Tipo == TipoTarea.ManoDeObra)
                {
                    conteoMO++;
                    // M.O. se considera gastado al asignar nómina
                    totalManoObra += r.MontoGastado;
                }
                else if (r.Tipo == TipoTarea.Material)
                {
                    conteoMat++;
                    // Material se considera gastado al finalizar el destajo
                    if (r.Finalizado) totalMaterial += r.Importe;
                }
            }

            olvDestajos.SetObjects(registrosDestajo);
        }

        private Dictionary<int, decimal> CargarMontosNomina(string ruta)
        {
            var resultado = new Dictionary<int, decimal>();
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (!ExisteTabla(conn, "NominaTareasAsignada")) return resultado;

                    string sql = @"
                        SELECT NodoID, SUM(ISNULL(Monto, 0)) AS MontoTotal
                        FROM NominaTareasAsignada
                        WHERE Manzana = @m AND Lote = @l AND Ruta = @r
                        GROUP BY NodoID";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzanaActual);
                        cmd.Parameters.AddWithValue("@l", loteActual);
                        cmd.Parameters.AddWithValue("@r", ruta);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int nodoId = Convert.ToInt32(reader["NodoID"]);
                                decimal monto = LeerDecimalSeguro(reader, "MontoTotal");
                                resultado[nodoId] = monto;
                            }
                        }
                    }
                }
            }
            catch { /* tabla puede no existir aún */ }
            return resultado;
        }

        private string TipoTareaTexto(TipoTarea t)
        {
            switch (t)
            {
                case TipoTarea.ManoDeObra: return "M. de Obra";
                case TipoTarea.Material: return "Material";
                default: return "—";
            }
        }

        private void RefrescarHUD()
        {
            // Estimaciones
            lblCardEstValor.Text = totalEstimaciones.ToString("C2", culturaMx);
            int conceptos = registrosEstimacion.Count;
            int finalizadosEst = registrosEstimacion.Count(r => r.AvancePorcentaje >= 100m);
            int etapas = registrosEstimacion
                .Select(r => string.IsNullOrEmpty(r.Etapa) ? "(Sin etapa)" : r.Etapa)
                .Distinct().Count();
            lblCardEstInfo.Text = $"{conceptos} concepto(s) en {etapas} etapa(s) · {finalizadosEst} al 100%";

            // Destajos: el gasto reconocido sale de las dos reglas (M.O. nómina + Material finalizado)
            decimal totalGastadoDestajos = totalManoObra + totalMaterial;
            lblCardDesValor.Text = totalGastadoDestajos.ToString("C2", culturaMx);
            int finalizadosDes = registrosDestajo.Count(r => r.Finalizado);
            int conMO_asignada = registrosDestajo.Count(r => r.Tipo == TipoTarea.ManoDeObra && r.NominaAsignada > 0);
            lblCardDesInfo.Text =
                $"Gastado de {totalDestajos.ToString("C2", culturaMx)} comprometido · " +
                $"{registrosDestajo.Count} act. · {finalizadosDes} fin. · {conMO_asignada} c/nómina";

            // M.O. — se considera gastado al asignar nómina
            lblMOValor.Text = totalManoObra.ToString("C2", culturaMx);
            decimal importeMOComprometido = registrosDestajo
                .Where(r => r.Tipo == TipoTarea.ManoDeObra).Sum(r => r.Importe);
            decimal pctMO = importeMOComprometido == 0 ? 0 : (totalManoObra / importeMOComprometido) * 100m;
            lblMOInfo.Text = $"{conMO_asignada}/{conteoMO} con nómina  ·  {pctMO:N1}% comprometido";

            // Material — se considera gastado al finalizar el destajo
            lblMatValor.Text = totalMaterial.ToString("C2", culturaMx);
            int matFinalizados = registrosDestajo.Count(r => r.Tipo == TipoTarea.Material && r.Finalizado);
            decimal importeMatComprometido = registrosDestajo
                .Where(r => r.Tipo == TipoTarea.Material).Sum(r => r.Importe);
            decimal pctMat = importeMatComprometido == 0 ? 0 : (totalMaterial / importeMatComprometido) * 100m;
            lblMatInfo.Text = $"{matFinalizados}/{conteoMat} finalizados  ·  {pctMat:N1}% comprometido";

            // Barras proporcionales al % gastado de su propio compromiso (M.O. y Material por separado)
            int anchoCard = Math.Max(50, cardMO.ClientSize.Width);
            barraMO.Width = Math.Max(2, (int)(anchoCard * (double)(pctMO / 100m)));
            barraMat.Width = Math.Max(2, (int)(anchoCard * (double)(pctMat / 100m)));

            // Gran total = ejecutado estimaciones + gasto reconocido destajos
            lblGranTotal.Text = (totalEstimaciones + totalGastadoDestajos).ToString("C2", culturaMx);
        }

        #endregion

        #region Utilidades SQL

        private System.Collections.Generic.HashSet<string> LeerColumnas(SqlConnection conn, string tabla)
        {
            var set = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t", conn))
            {
                cmd.Parameters.AddWithValue("@t", tabla);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) set.Add(r.GetString(0));
                }
            }
            return set;
        }

        private bool ExisteTabla(SqlConnection conn, string tabla)
        {
            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @t", conn))
            {
                cmd.Parameters.AddWithValue("@t", tabla);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private decimal LeerDecimalSeguro(System.Data.IDataReader reader, string columna)
        {
            try
            {
                int idx = reader.GetOrdinal(columna);
                if (reader.IsDBNull(idx)) return 0m;
                object v = reader.GetValue(idx);
                if (v == null) return 0m;
                if (v is decimal d) return d;
                if (v is double dbl)
                {
                    if (double.IsNaN(dbl) || double.IsInfinity(dbl)) return 0m;
                    return (decimal)dbl;
                }
                if (v is float f)
                {
                    if (float.IsNaN(f) || float.IsInfinity(f)) return 0m;
                    return (decimal)f;
                }
                if (v is int i) return i;
                if (v is long l) return l;
                return ParseDecimalSeguro(v.ToString());
            }
            catch { return 0m; }
        }

        private decimal ParseDecimalSeguro(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;
            decimal d;
            if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) return d;
            if (decimal.TryParse(valor, NumberStyles.Any, culturaMx, out d)) return d;
            if (decimal.TryParse(valor, out d)) return d;
            return 0m;
        }

        #endregion

        #region Exportación PDF

        private void btnExportarPDF_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
            {
                MessageBox.Show("Primero consulte una casa (Manzana / Lote).",
                    "Administrativos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CALANDRIA RESIDENCIAL", "Administrativos");
                Directory.CreateDirectory(carpeta);

                string sugerido = $"Administrativos_M{manzanaActual}_L{loteActual}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

                using (var sfd = new SaveFileDialog
                {
                    Filter = "Archivo PDF (*.pdf)|*.pdf",
                    FileName = sugerido,
                    InitialDirectory = carpeta,
                    Title = "Exportar concentrado administrativo a PDF"
                })
                {
                    if (sfd.ShowDialog() != DialogResult.OK) return;
                    GenerarPDF(sfd.FileName);
                    var resp = MessageBox.Show(
                        $"PDF generado:\n{sfd.FileName}\n\n¿Desea abrirlo ahora?",
                        "Administrativos", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (resp == DialogResult.Yes)
                    {
                        try { Process.Start(sfd.FileName); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar PDF: " + ex.Message,
                    "Administrativos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarPDF(string rutaArchivo)
        {
            using (var pdf = new PdfDocument())
            {
                pdf.Info.Title = $"Administrativos — Mz {manzanaActual} Lt {loteActual}";
                pdf.Info.Author = "Sistema Calandria";
                pdf.Info.Subject = "Concentrado financiero por casa";
                pdf.Info.Creator = "DynamicSepticSystem";

                var page = pdf.AddPage();
                page.Size = PdfSharp.PageSize.Letter;
                page.Orientation = PdfSharp.PageOrientation.Landscape;
                var gfx = XGraphics.FromPdfPage(page);

                var fontTitulo = new XFont("Arial", 16, XFontStyle.Bold);
                var fontSub = new XFont("Arial", 10, XFontStyle.Regular);
                var fontSeccion = new XFont("Arial", 12, XFontStyle.Bold);
                var fontHeader = new XFont("Arial", 9, XFontStyle.Bold);
                var fontCell = new XFont("Arial", 8, XFontStyle.Regular);
                var fontGrupo = new XFont("Arial", 9.5, XFontStyle.Bold);
                var fontTotal = new XFont("Arial", 11, XFontStyle.Bold);

                var brushCafe = new XSolidBrush(XColor.FromArgb(88, 53, 23));
                var brushCafeBar = new XSolidBrush(XColor.FromArgb(179, 108, 46));
                var brushBlanco = XBrushes.White;
                var brushTexto = XBrushes.Black;
                var brushVerde = new XSolidBrush(XColor.FromArgb(46, 134, 75));
                var brushRojo = new XSolidBrush(XColor.FromArgb(155, 41, 28));
                var brushAlt = new XSolidBrush(XColor.FromArgb(250, 247, 242));
                var brushGrupo = new XSolidBrush(XColor.FromArgb(235, 222, 200));

                double margin = 30;
                double pageW = page.Width - 2 * margin;
                double y = margin;

                // Header
                gfx.DrawRectangle(brushCafe, margin, y, pageW, 50);
                gfx.DrawString("CONCENTRADO ADMINISTRATIVO", fontTitulo, brushBlanco,
                    new XRect(margin + 10, y + 6, pageW - 20, 22), XStringFormats.TopLeft);
                gfx.DrawString($"Mz {manzanaActual} · Lt {loteActual} · Prototipo: {(string.IsNullOrEmpty(prototipoActual) ? "—" : prototipoActual)}",
                    fontSub, brushBlanco,
                    new XRect(margin + 10, y + 28, pageW - 20, 18), XStringFormats.TopLeft);
                gfx.DrawString($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",
                    fontSub, brushBlanco,
                    new XRect(margin, y + 28, pageW - 10, 18), XStringFormats.TopRight);
                y += 60;

                // ====== Resumen tipo HUD en el PDF ======
                double cardW = (pageW - 20) / 3.0;
                DibujarTarjetaPDF(gfx, margin, y, cardW, "ESTIMACIONES", totalEstimaciones.ToString("C2", culturaMx),
                    $"{registrosEstimacion.Count} concepto(s)", brushCafe, brushVerde, fontHeader, fontGrupo, fontCell);
                DibujarTarjetaPDF(gfx, margin + cardW + 10, y, cardW, "DESTAJOS · MANO DE OBRA", totalManoObra.ToString("C2", culturaMx),
                    $"{conteoMO} destajo(s)", brushCafe, brushVerde, fontHeader, fontGrupo, fontCell);
                DibujarTarjetaPDF(gfx, margin + 2 * (cardW + 10), y, cardW, "DESTAJOS · MATERIAL", totalMaterial.ToString("C2", culturaMx),
                    $"{conteoMat} destajo(s)", brushCafe, brushRojo, fontHeader, fontGrupo, fontCell);
                y += 70;

                // ============ ESTIMACIONES agrupado ============
                gfx.DrawRectangle(brushCafeBar, margin, y, pageW, 24);
                gfx.DrawString("ESTIMACIONES — Agrupadas por Etapa", fontSeccion, brushBlanco,
                    new XRect(margin + 8, y + 4, pageW - 16, 18), XStringFormats.TopLeft);
                y += 24;

                double[] anchoEst = { 70, 90, 360, 90, 130, 90 };
                string[] hdrEst = { "WBS", "Código", "Partida", "Avance %", "Monto Ejec.", "Fecha" };
                y = DibujarFilaTabla(gfx, margin, y, anchoEst, hdrEst, fontHeader, brushCafe, brushBlanco, true);

                var grupos = registrosEstimacion
                    .GroupBy(r => string.IsNullOrEmpty(r.Etapa) ? "(Sin etapa)" : r.Etapa)
                    .OrderBy(g => g.Key);

                foreach (var grupo in grupos)
                {
                    if (y > page.Height - 100)
                    {
                        page = pdf.AddPage();
                        page.Size = PdfSharp.PageSize.Letter;
                        page.Orientation = PdfSharp.PageOrientation.Landscape;
                        gfx.Dispose();
                        gfx = XGraphics.FromPdfPage(page);
                        y = margin;
                        y = DibujarFilaTabla(gfx, margin, y, anchoEst, hdrEst, fontHeader, brushCafe, brushBlanco, true);
                    }

                    decimal subtotal = grupo.Sum(r => r.MontoEjecutado);
                    gfx.DrawRectangle(brushGrupo, margin, y, pageW, 18);
                    gfx.DrawString($"  {grupo.Key}   ·   {grupo.Count()} concepto(s)",
                        fontGrupo, brushCafe,
                        new XRect(margin + 4, y + 1, pageW - 200, 16), XStringFormats.CenterLeft);
                    gfx.DrawString($"Subtotal: {subtotal.ToString("C2", culturaMx)}",
                        fontGrupo, brushVerde,
                        new XRect(margin, y + 1, pageW - 6, 16), XStringFormats.CenterRight);
                    y += 18;

                    int fila = 0;
                    foreach (var r in grupo)
                    {
                        if (y > page.Height - 60)
                        {
                            page = pdf.AddPage();
                            page.Size = PdfSharp.PageSize.Letter;
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                            gfx.Dispose();
                            gfx = XGraphics.FromPdfPage(page);
                            y = margin;
                            y = DibujarFilaTabla(gfx, margin, y, anchoEst, hdrEst, fontHeader, brushCafe, brushBlanco, true);
                        }

                        string[] valores =
                        {
                            r.WBS,
                            r.Codigo,
                            Truncar(r.Partida, 75),
                            r.AvancePorcentaje.ToString("N2"),
                            r.MontoEjecutado.ToString("C2", culturaMx),
                            r.FechaFinalizacion.HasValue ? r.FechaFinalizacion.Value.ToString("dd/MM/yyyy") : "—"
                        };
                        XBrush fondo = (fila % 2 == 0) ? (XBrush)brushAlt : XBrushes.White;
                        y = DibujarFilaTabla(gfx, margin, y, anchoEst, valores, fontCell, fondo, brushTexto, false);
                        fila++;
                    }
                }

                y += 4;
                gfx.DrawString("TOTAL ESTIMADO EJECUTADO:", fontTotal, brushCafe,
                    new XRect(margin, y, pageW - 130, 18), XStringFormats.TopRight);
                gfx.DrawString(totalEstimaciones.ToString("C2", culturaMx), fontTotal, brushVerde,
                    new XRect(margin, y, pageW, 18), XStringFormats.TopRight);
                y += 28;

                // ============ DESTAJOS ============
                if (y > page.Height - 140)
                {
                    page = pdf.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }

                gfx.DrawRectangle(brushCafeBar, margin, y, pageW, 24);
                gfx.DrawString("DESTAJOS — Agrupados por Categoría (M.O. gastado al asignar nómina · Material gastado al finalizar)",
                    fontSeccion, brushBlanco,
                    new XRect(margin + 8, y + 4, pageW - 16, 18), XStringFormats.TopLeft);
                y += 24;

                double[] anchoDes = { 35, 70, 215, 45, 45, 75, 85, 85, 65, 65 };
                string[] hdrDes = { "ID", "Tipo", "Destajo", "Cant.", "Unid.", "P. Unit.", "Importe", "Gastado", "Cuadr.", "Estado" };
                y = DibujarFilaTabla(gfx, margin, y, anchoDes, hdrDes, fontHeader, brushCafe, brushBlanco, true);

                var gruposDes = registrosDestajo
                    .GroupBy(r => string.IsNullOrEmpty(r.Categoria) ? "(Sin categoría)" : r.Categoria)
                    .OrderBy(g => g.Key);

                foreach (var grupo in gruposDes)
                {
                    if (y > page.Height - 100)
                    {
                        page = pdf.AddPage();
                        page.Size = PdfSharp.PageSize.Letter;
                        page.Orientation = PdfSharp.PageOrientation.Landscape;
                        gfx.Dispose();
                        gfx = XGraphics.FromPdfPage(page);
                        y = margin;
                        y = DibujarFilaTabla(gfx, margin, y, anchoDes, hdrDes, fontHeader, brushCafe, brushBlanco, true);
                    }

                    decimal subMO = grupo.Where(r => r.Tipo == TipoTarea.ManoDeObra).Sum(r => r.MontoGastado);
                    decimal subMat = grupo.Where(r => r.Tipo == TipoTarea.Material && r.Finalizado).Sum(r => r.Importe);

                    gfx.DrawRectangle(brushGrupo, margin, y, pageW, 18);
                    gfx.DrawString($"  {grupo.Key}   ·   {grupo.Count()} destajo(s)",
                        fontGrupo, brushCafe,
                        new XRect(margin + 4, y + 1, pageW - 300, 16), XStringFormats.CenterLeft);
                    gfx.DrawString($"M.O.: {subMO.ToString("C2", culturaMx)}   ·   Material: {subMat.ToString("C2", culturaMx)}",
                        fontGrupo, brushVerde,
                        new XRect(margin, y + 1, pageW - 6, 16), XStringFormats.CenterRight);
                    y += 18;

                    int filaDes = 0;
                    foreach (var r in grupo)
                    {
                        if (y > page.Height - 60)
                        {
                            page = pdf.AddPage();
                            page.Size = PdfSharp.PageSize.Letter;
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                            gfx.Dispose();
                            gfx = XGraphics.FromPdfPage(page);
                            y = margin;
                            y = DibujarFilaTabla(gfx, margin, y, anchoDes, hdrDes, fontHeader, brushCafe, brushBlanco, true);
                        }

                        string gastadoTxt = r.MontoGastado == 0m ? "—" : r.MontoGastado.ToString("C2", culturaMx);
                        string[] valores =
                        {
                            r.NodoID.ToString(),
                            TipoTareaTexto(r.Tipo),
                            Truncar(r.Nombre, 50),
                            r.Cantidad.ToString("N2"),
                            r.Unidad,
                            r.PrecioUnitario.ToString("C2", culturaMx),
                            r.Importe.ToString("C2", culturaMx),
                            gastadoTxt,
                            r.Cuadrilla,
                            r.EstadoTexto
                        };
                        XBrush fondo = r.GastoComputado ? (XBrush)new XSolidBrush(XColor.FromArgb(232, 245, 233))
                                                        : ((filaDes % 2 == 0) ? (XBrush)brushAlt : XBrushes.White);
                        y = DibujarFilaTabla(gfx, margin, y, anchoDes, valores, fontCell, fondo, brushTexto, false);
                        filaDes++;
                    }
                }

                y += 4;
                decimal gastoDestajos = totalManoObra + totalMaterial;
                gfx.DrawString("TOTAL GASTADO EN DESTAJOS:", fontTotal, brushCafe,
                    new XRect(margin, y, pageW - 130, 18), XStringFormats.TopRight);
                gfx.DrawString(gastoDestajos.ToString("C2", culturaMx), fontTotal, brushVerde,
                    new XRect(margin, y, pageW, 18), XStringFormats.TopRight);
                y += 28;

                // Gran total
                if (y > page.Height - 50)
                {
                    page = pdf.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }
                gfx.DrawRectangle(brushCafe, margin, y, pageW, 36);
                gfx.DrawString("GRAN TOTAL DE LA CASA:", new XFont("Arial", 13, XFontStyle.Bold), brushBlanco,
                    new XRect(margin + 10, y + 9, pageW - 180, 22), XStringFormats.TopLeft);
                gfx.DrawString((totalEstimaciones + totalManoObra + totalMaterial).ToString("C2", culturaMx),
                    new XFont("Arial", 15, XFontStyle.Bold), brushBlanco,
                    new XRect(margin + 10, y + 7, pageW - 20, 24), XStringFormats.TopRight);

                gfx.Dispose();
                pdf.Save(rutaArchivo);
            }
        }

        private void DibujarTarjetaPDF(XGraphics gfx, double x, double y, double w,
            string titulo, string valor, string info,
            XBrush brushTitulo, XBrush brushValor, XFont fontHdr, XFont fontVal, XFont fontInfo)
        {
            // Card body
            gfx.DrawRectangle(XBrushes.White, x, y, w, 60);
            gfx.DrawRectangle(new XPen(XColor.FromArgb(220, 210, 195)), x, y, w, 60);
            // Acento café
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(179, 108, 46)), x, y, w, 3);

            gfx.DrawString(titulo, fontHdr, brushTitulo,
                new XRect(x + 10, y + 6, w - 20, 16), XStringFormats.CenterLeft);
            gfx.DrawString(valor, new XFont("Arial", 16, XFontStyle.Bold), brushValor,
                new XRect(x + 10, y + 22, w - 20, 22), XStringFormats.CenterLeft);
            gfx.DrawString(info, fontInfo, XBrushes.Gray,
                new XRect(x + 10, y + 44, w - 20, 14), XStringFormats.CenterLeft);
        }

        private double DibujarFilaTabla(XGraphics gfx, double xInicial, double y, double[] anchos,
            string[] valores, XFont font, XBrush fondo, XBrush texto, bool isHeader)
        {
            double altura = isHeader ? 22 : 16;
            double x = xInicial;
            double totalW = 0;
            foreach (var a in anchos) totalW += a;

            gfx.DrawRectangle(fondo, xInicial, y, totalW, altura);

            for (int i = 0; i < valores.Length && i < anchos.Length; i++)
            {
                var rect = new XRect(x + 4, y + 2, anchos[i] - 8, altura - 4);
                XStringFormat format;
                if (isHeader) format = XStringFormats.Center;
                else if (i == valores.Length - 1) format = XStringFormats.Center;
                else if (i >= valores.Length - 3) format = XStringFormats.CenterRight;
                else format = XStringFormats.CenterLeft;
                gfx.DrawString(valores[i] ?? "", font, texto, rect, format);
                x += anchos[i];
            }
            gfx.DrawLine(XPens.LightGray, xInicial, y + altura, xInicial + totalW, y + altura);
            return y + altura;
        }

        private string Truncar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }

        #endregion

        #region Modelos internos

        public class RegistroEstimacion
        {
            public string WBS { get; set; }
            public string Codigo { get; set; }
            public string Etapa { get; set; }
            public string Partida { get; set; }
            public decimal AvancePorcentaje { get; set; }
            public decimal MontoEjecutado { get; set; }
            public DateTime? FechaFinalizacion { get; set; }
        }

        public class RegistroDestajo
        {
            public int NodoID { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Categoria { get; set; }
            public decimal Cantidad { get; set; }
            public string Unidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public string Cuadrilla { get; set; }
            public bool Finalizado { get; set; }
            public DateTime? FechaFinalizacion { get; set; }
            public TipoTarea Tipo { get; set; }
            /// <summary>Total acumulado en NominaTareasAsignada para este destajo.</summary>
            public decimal NominaAsignada { get; set; }
            public decimal Importe => Cantidad * PrecioUnitario;

            /// <summary>
            /// Monto considerado "gastado" según reglas de negocio:
            ///   M.O.: total asignado en nómina · Material: importe sólo si está finalizado.
            /// </summary>
            public decimal MontoGastado
            {
                get
                {
                    if (Tipo == TipoTarea.ManoDeObra) return NominaAsignada;
                    if (Tipo == TipoTarea.Material) return Finalizado ? Importe : 0m;
                    return 0m;
                }
            }

            public bool GastoComputado => MontoGastado > 0m;

            public string TipoTexto
            {
                get
                {
                    switch (Tipo)
                    {
                        case TipoTarea.ManoDeObra: return "M. de Obra";
                        case TipoTarea.Material: return "Material";
                        default: return "—";
                    }
                }
            }

            public string EstadoTexto
            {
                get
                {
                    if (Finalizado) return "Finalizado";
                    if (!string.IsNullOrEmpty(Cuadrilla)) return "Con cuadrilla";
                    return "Activado";
                }
            }
        }

        #endregion
    }
}
