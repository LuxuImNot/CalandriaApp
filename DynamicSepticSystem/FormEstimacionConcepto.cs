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
    public partial class FormEstimacionConcepto : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        private List<ItemEstimacionConcepto> itemsConceptos = new List<ItemEstimacionConcepto>();
        private string prototipoActual = "";
        private string manzanaActual = "";
        private string loteActual = "";

        public FormEstimacionConcepto()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            ConfigurarObjectListView();
            CargarManzanas();
            this.Load += FormEstimacionConcepto_Load;
        }

        private void FormEstimacionConcepto_Load(object sender, EventArgs e)
        {
            this.Text = "Estimaci�n por Conceptos - Sistema Calandria";
        }

        private void CargarManzanas()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbManzana.Items.Clear();
                        while (reader.Read())
                        {
                            cmbManzana.Items.Add(reader["Manzana"].ToString());
                        }
                    }
                }
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Lote FROM InventarioCasas WHERE Manzana = @m ORDER BY Lote";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", cmbManzana.SelectedItem.ToString());
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbLote.Items.Clear();
                            while (reader.Read())
                            {
                                cmbLote.Items.Add(reader["Lote"].ToString());
                            }
                        }
                    }
                }
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
                MessageBox.Show("Por favor selecciona Manzana y Lote", "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            manzanaActual = cmbManzana.SelectedItem.ToString();
            loteActual = cmbLote.SelectedItem.ToString();

            prototipoActual = ObtenerPrototipo(manzanaActual, loteActual);
            if (string.IsNullOrEmpty(prototipoActual))
            {
                MessageBox.Show($"No se encontr� el prototipo para M{manzanaActual}-L{loteActual}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Text = $"Estimaci�n por Conceptos - M{manzanaActual} L{loteActual} ({prototipoActual})";

            CargarConceptos();
        }

        private string ObtenerPrototipo(string manzana, string lote)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        private void ConfigurarObjectListView()
        {
            olvEstimacionConceptos.FullRowSelect = true;
            // Disable editing here: edits must be done in FormAvanceObra
            olvEstimacionConceptos.CellEditActivation = ObjectListView.CellEditActivateMode.None;
            olvEstimacionConceptos.UseAlternatingBackColors = true;
            olvEstimacionConceptos.AlternateRowBackColor = Color.FromArgb(240, 248, 255);
            olvEstimacionConceptos.CheckBoxes = true;
            olvEstimacionConceptos.CheckedAspectName = "Incluir";

            // ? DESHABILITAR agrupamiento para mantener orden SQL
            olvEstimacionConceptos.ShowGroups = false;
            
            // ? BLOQUEAR ordenamiento por clic en encabezado
            olvEstimacionConceptos.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            
            // ? DESHABILITAR la capacidad de reordenar columnas manualmente
            olvEstimacionConceptos.AllowColumnReorder = false;

            // Columna Checkbox (Incluir en PDF)
            var colIncluir = new OLVColumn("", "Incluir")
            {
                Width = 30,
                IsEditable = false,
                CheckBoxes = true,
                Sortable = false // ? Deshabilitar ordenamiento
            };

            // Columna C�digo - SIN AspectGetter para mantener orden original
            var colCodigo = new OLVColumn("C�digo", "Codigo") 
            { 
                Width = 80, 
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false // ? Deshabilitar ordenamiento
            };

            // Columna Concepto
            var colConcepto = new OLVColumn("Concepto", "Concepto") 
            { 
                Width = 320, 
                IsEditable = false,
                Sortable = false // ? Deshabilitar ordenamiento
            };

            // Columna Total
            var colTotal = new OLVColumn("Total", "Total")
            {
                Width = 120,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}",
                Sortable = false // ? Deshabilitar ordenamiento
            };

            // Columna Avance % (NON-EDITABLE here)
            var colAvance = new OLVColumn("Avance %", "AvancePorcentaje")
            {
                Width = 90,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectToStringFormat = "{0:N1}%",
                Sortable = false // ? Deshabilitar ordenamiento
            };

            // Columna Ejecutado (NON-EDITABLE here)
            var colEjecutado = new OLVColumn("Ejecutado", "MontoEjecutado")
            {
                Width = 120,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}",
                Sortable = false // ? Deshabilitar ordenamiento
            };

            olvEstimacionConceptos.AllColumns.AddRange(new[] { colIncluir, colCodigo, colConcepto, colTotal, colAvance, colEjecutado });
            olvEstimacionConceptos.RebuildColumns();

            // Disable finishing edits because editing is disabled
            olvEstimacionConceptos.CellEditFinishing += (s, e) => { e.Cancel = true; };

            // Event on checkbox remains
            olvEstimacionConceptos.ItemChecked += (s, e) =>
            {
                var item = olvEstimacionConceptos.GetModelObject(e.Item.Index) as ItemEstimacionConcepto;
                if (item != null)
                {
                    item.Incluir = e.Item.Checked;
                }
            };
        }

        private void CargarConceptos()
        {
            itemsConceptos = CargarConceptosDesdeTabla();
            CargarAvancesGuardados();

            // ? Solo establecer objetos SIN aplicar ordenamiento
            // El orden se mantiene tal como viene de la base de datos
            olvEstimacionConceptos.SetObjects(itemsConceptos);

            ActualizarTotales();
            DibujarGraficas();
        }

        private List<ItemEstimacionConcepto> CargarConceptosDesdeTabla()
        {
            var items = new List<ItemEstimacionConcepto>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ? CORREGIDO: Verifica si existe columna Codigo y ordena num�ricamente
                    string sqlCheck = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Estimacion(Concepto)' 
                        AND COLUMN_NAME = 'Codigo'";

                    bool tieneColumnaCodigo = false;
                    using (SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn))
                    {
                        int count = (int)cmdCheck.ExecuteScalar();
                        tieneColumnaCodigo = count > 0;
                    }

                    // Determinar columna de importe en la tabla Estimacion(Concepto) seg�n prototipoActual
                    // Nota: prototipo correcto es 'CALANDRA' (sin 'I')
                    string columnaDeseada = prototipoActual.ToUpper().Contains("CALANDRA") ? "CostoCalandra" : "CostoTunera";
                    string columnaACast = null;

                    // Verificar si existe la columna deseada
                    string sqlCheckCol = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Estimacion(Concepto)' 
                        AND COLUMN_NAME = @col";
                    using (SqlCommand cmdCheckCol = new SqlCommand(sqlCheckCol, conn))
                    {
                        cmdCheckCol.Parameters.AddWithValue("@col", columnaDeseada);
                        int countCol = (int)cmdCheckCol.ExecuteScalar();
                        if (countCol > 0)
                        {
                            columnaACast = columnaDeseada;
                        }
                        else
                        {
                            // Si no existe la columna deseada, verificar si existe TOTAL (compatibilidad)
                            using (SqlCommand cmdCheckTotal = new SqlCommand(@"
                                SELECT COUNT(*) 
                                FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_NAME = 'Estimacion(Concepto)' 
                                AND COLUMN_NAME = 'TOTAL'", conn))
                            {
                                int countTotal = (int)cmdCheckTotal.ExecuteScalar();
                                if (countTotal > 0)
                                {
                                    columnaACast = "TOTAL";
                                }
                                else
                                {
                                    MessageBox.Show($"No se encontr� columna de importe en la tabla Estimacion(Concepto). Buscada: {columnaDeseada} o TOTAL", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return items;
                                }
                            }
                        }
                    }

                    string sql;
                    if (tieneColumnaCodigo)
                    {
                        // ? Usar la columna Codigo existente y ordenar NUM�RICAMENTE
                        // Se usa TRY_CAST para convertir a INT y ordenar correctamente
                        sql = $@"
                            SELECT 
                                Codigo,
                                Concepto,
                                SUM(CAST([{columnaACast}] as FLOAT)) as Total
                            FROM [dbo].[Estimacion(Concepto)]
                            GROUP BY Codigo, Concepto
                            ORDER BY 
                                CASE 
                                    WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                                    THEN TRY_CAST(Codigo AS INT)
                                    ELSE 999999 
                                END,
                                Codigo";
                    }
                    else
                    {
                        // Si no existe, generar c�digos con ROW_NUMBER()
                        sql = $@"
                            SELECT 
                                ROW_NUMBER() OVER (ORDER BY Concepto) as Codigo,
                                Concepto,
                                SUM(CAST([{columnaACast}] as FLOAT)) as Total
                            FROM [dbo].[Estimacion(Concepto)]
                            GROUP BY Concepto
                            ORDER BY Concepto";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string codigo = reader["Codigo"].ToString();
                                string concepto = reader["Concepto"].ToString();
                                double total = Convert.ToDouble(reader["Total"]);

                                items.Add(new ItemEstimacionConcepto
                                {
                                    Codigo = codigo,
                                    Concepto = concepto,
                                    Total = total,
                                    AvancePorcentaje = 0,
                                    MontoEjecutado = 0,
                                    Incluir = true // ? Por defecto incluir en PDF
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar conceptos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return items;
        }

        private void CargarAvancesGuardados()
        {
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Crear tabla si no existe
                    string sqlCheck = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualConcepto')
                        BEGIN
                            CREATE TABLE AvanceManualConcepto (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Manzana NVARCHAR(10),
                                Lote NVARCHAR(10),
                                Prototipo NVARCHAR(50),
                                Codigo NVARCHAR(10),
                                Concepto NVARCHAR(200),
                                AvancePorcentaje FLOAT,
                                FechaActualizacion DATETIME DEFAULT GETDATE()
                            );
                        END";
                    using (SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn))
                    {
                        cmdCheck.ExecuteNonQuery();
                    }

                    // Detectar si existe columna MontoEjecutado
                    bool tieneMonto = false;
                    using (SqlCommand cmdCols = new SqlCommand(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualConcepto'", conn))
                    using (SqlDataReader rc = cmdCols.ExecuteReader())
                    {
                        while (rc.Read())
                        {
                            var col = rc.GetString(0);
                            if (string.Equals(col, "MontoEjecutado", StringComparison.OrdinalIgnoreCase))
                            {
                                tieneMonto = true;
                                break;
                            }
                        }
                    }

                    string sql = tieneMonto ? "SELECT Codigo, AvancePorcentaje, MontoEjecutado FROM AvanceManualConcepto WHERE Manzana = @m AND Lote = @l" :
                                             "SELECT Codigo, AvancePorcentaje FROM AvanceManualConcepto WHERE Manzana = @m AND Lote = @l";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzanaActual);
                        cmd.Parameters.AddWithValue("@l", loteActual);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string codigo = reader["Codigo"].ToString();
                                double avance = reader["AvancePorcentaje"] != DBNull.Value ? Convert.ToDouble(reader["AvancePorcentaje"]) : 0.0;
                                double monto = double.NaN;
                                if (tieneMonto)
                                {
                                    try
                                    {
                                        int idx = reader.GetOrdinal("MontoEjecutado");
                                        if (!reader.IsDBNull(idx))
                                            monto = Convert.ToDouble(reader.GetValue(idx));
                                    }
                                    catch { monto = double.NaN; }
                                }

                                var item = itemsConceptos.FirstOrDefault(i => i.Codigo == codigo);
                                if (item != null)
                                {
                                    item.AvancePorcentaje = avance;
                                    if (!double.IsNaN(monto))
                                        item.MontoEjecutado = monto;
                                    else
                                        item.MontoEjecutado = item.Total * (avance / 100.0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar avances guardados: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarAvanceEnBD(ItemEstimacionConcepto item)
        {
            // ? CORREGIDO: Ahora valida campos antes de usar (consistente con FormAvanceConcepto)
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Detectar si la columna MontoEjecutado existe
                    bool tieneMonto = false;
                    using (SqlCommand cmdCols = new SqlCommand(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualConcepto'", conn))
                    using (SqlDataReader rc = cmdCols.ExecuteReader())
                    {
                        while (rc.Read())
                        {
                            var col = rc.GetString(0);
                            if (string.Equals(col, "MontoEjecutado", StringComparison.OrdinalIgnoreCase))
                            {
                                tieneMonto = true;
                                break;
                            }
                        }
                    }

                    if (tieneMonto)
                    {
                        string sql = @"
                            IF EXISTS (SELECT 1 FROM AvanceManualConcepto WHERE Manzana=@m AND Lote=@l AND Codigo=@cod)
                                UPDATE AvanceManualConcepto 
                                SET AvancePorcentaje=@avance, MontoEjecutado=@monto, FechaActualizacion=GETDATE()
                                WHERE Manzana=@m AND Lote=@l AND Codigo=@cod
                            ELSE
                                INSERT INTO AvanceManualConcepto (Manzana, Lote, Prototipo, Codigo, Concepto, AvancePorcentaje, MontoEjecutado)
                                VALUES (@m, @l, @proto, @cod, @concepto, @avance, @monto)";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@m", manzanaActual);
                            cmd.Parameters.AddWithValue("@l", loteActual);
                            cmd.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@cod", item.Codigo ?? "");
                            cmd.Parameters.AddWithValue("@concepto", item.Concepto ?? "");
                            cmd.Parameters.AddWithValue("@avance", item.AvancePorcentaje);

                            var p = cmd.Parameters.Add("@monto", SqlDbType.Decimal);
                            p.Precision = 18;
                            p.Scale = 2;
                            p.Value = Math.Round(item.MontoEjecutado, 2);

                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string sql = @"
                            IF EXISTS (SELECT 1 FROM AvanceManualConcepto WHERE Manzana=@m AND Lote=@l AND Codigo=@cod)
                                UPDATE AvanceManualConcepto 
                                SET AvancePorcentaje=@avance, FechaActualizacion=GETDATE()
                                WHERE Manzana=@m AND Lote=@l AND Codigo=@cod
                            ELSE
                                INSERT INTO AvanceManualConcepto (Manzana, Lote, Prototipo, Codigo, Concepto, AvancePorcentaje)
                                VALUES (@m, @l, @proto, @cod, @concepto, @avance)";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@m", manzanaActual);
                            cmd.Parameters.AddWithValue("@l", loteActual);
                            cmd.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@cod", item.Codigo ?? "");
                            cmd.Parameters.AddWithValue("@concepto", item.Concepto ?? "");
                            cmd.Parameters.AddWithValue("@avance", item.AvancePorcentaje);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
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
                DibujarGraficaPastel(g, new Rectangle(20, 20, 180, 180));
                DibujarGraficaBarras(g, new Rectangle(20, 220, width - 40, height - 240));
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

        private void DibujarGraficaBarras(Graphics g, Rectangle rect)
        {
            var conceptos = itemsConceptos
                .OrderByDescending(c => c.Total)
                .ToList();

            if (conceptos.Count == 0) return;

            double maxValor = conceptos.Max(c => c.Total);
            if (maxValor == 0) return;

            int barWidth = Math.Max(40, (rect.Width - 60) / conceptos.Count);
            int spacing = 10;

            using (Font font = new Font("Segoe UI", 7))
            using (Brush textBrush = new SolidBrush(Color.Black))
            using (Pen gridPen = new Pen(Color.LightGray, 1))
            {
                for (int i = 0; i <= 5; i++)
                {
                    int y = rect.Y + (rect.Height * i / 5);
                    g.DrawLine(gridPen, rect.X, y, rect.Right, y);
                }

                int x = rect.X + 30;
                foreach (var concepto in conceptos)
                {
                    int alturaTotal = (int)((concepto.Total / maxValor) * (rect.Height - 60));
                    int alturaEjecutado = (int)((concepto.MontoEjecutado / maxValor) * (rect.Height - 60));
                    int yBase = rect.Bottom - 40;

                    using (Brush brushFondo = new SolidBrush(Color.FromArgb(220, 220, 220)))
                    {
                        g.FillRectangle(brushFondo, x, yBase - alturaTotal, barWidth - spacing, alturaTotal);
                    }

                    using (Brush brushEjecutado = new SolidBrush(Color.FromArgb(0, 122, 204)))
                    {
                        g.FillRectangle(brushEjecutado, x, yBase - alturaEjecutado, barWidth - spacing, alturaEjecutado);
                    }

                    string etiqueta = concepto.Concepto;
                    if (etiqueta.Length > 15)
                        etiqueta = etiqueta.Substring(0, 12) + "...";

                    g.TranslateTransform(x + (barWidth - spacing) / 2, yBase + 5);
                    g.RotateTransform(-45);
                    g.DrawString(etiqueta, font, textBrush, 0, 0);
                    g.ResetTransform();

                    if (concepto.Total > 0)
                    {
                        double porcentaje = (concepto.MontoEjecutado / concepto.Total) * 100;
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

        private void btnMarcarTodos_Click(object sender, EventArgs e)
        {
            foreach (var item in itemsConceptos)
            {
                item.Incluir = true;
            }
            olvEstimacionConceptos.RefreshObjects(itemsConceptos);
        }

        private void btnDesmarcarTodos_Click(object sender, EventArgs e)
        {
            foreach (var item in itemsConceptos)
            {
                item.Incluir = false;
            }
            olvEstimacionConceptos.RefreshObjects(itemsConceptos);
        }

        private void btnExportarPDF_Click(object sender, EventArgs e)
        {
            var conceptosSeleccionados = itemsConceptos.Where(i => i.Incluir).ToList();

            if (conceptosSeleccionados.Count == 0)
            {
                MessageBox.Show("Selecciona al menos un concepto para incluir en el PDF", 
                    "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = $"EstimacionConceptos_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GenerarPDFEstimacion(sfd.FileName, conceptosSeleccionados);
                    MessageBox.Show("PDF generado exitosamente", "�xito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var result = MessageBox.Show("�Deseas abrir el PDF?", "Abrir PDF", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar PDF: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GenerarPDFEstimacion(string rutaPdf, List<ItemEstimacionConcepto> conceptos)
        {
            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = "Estimaci�n por Conceptos - Sistema Calandria";
            pdf.Info.Author = "Sistema Calandria Residencial";
            pdf.Info.Subject = "Reporte de Estimaci�n por Conceptos";
            pdf.Info.Keywords = "Construcci�n, Estimaci�n, Conceptos";

            double totalPresupuestado = conceptos.Sum(i => i.Total);
            double totalEjecutado = conceptos.Sum(i => i.MontoEjecutado);
            double avanceGeneral = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;

            XColor colorPrimario = XColor.FromArgb(0, 122, 204);
            XColor colorSecundario = XColor.FromArgb(46, 204, 113);
            XColor colorAccento = XColor.FromArgb(52, 73, 94);
            XColor colorFondo = XColor.FromArgb(236, 240, 241);

            // ============= P�GINA 1: PORTADA Y RESUMEN =============
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

            gfx.DrawString("ESTIMACI�N POR CONCEPTOS", fontTituloGrande, XBrushes.White,
                new XRect(0, 25, page.Width, 30), XStringFormats.TopCenter);
            gfx.DrawString("Sistema de Control de Construcci�n", fontSubtitulo, XBrushes.White,
                new XRect(0, 55, page.Width, 20), XStringFormats.TopCenter);

            double y = 130;

            XPen penBorde = new XPen(colorPrimario, 2);

            gfx.DrawRectangle(penBorde, new XSolidBrush(colorFondo), 40, y, 240, 60);
            gfx.DrawString("INFORMACI�N DEL REPORTE", fontSubtituloSeccion, new XSolidBrush(colorPrimario), 50, y + 10);
            gfx.DrawString($"Fecha de generaci�n:", fontNegrita, XBrushes.Black, 50, y + 30);
            gfx.DrawString($"{DateTime.Now:dd/MM/yyyy HH:mm}", fontNormal, XBrushes.Black, 180, y + 30);

            gfx.DrawRectangle(penBorde, new XSolidBrush(colorFondo), 300, y, 260, 60);
            gfx.DrawString("CONCEPTOS", fontSubtituloSeccion, new XSolidBrush(colorPrimario), 310, y + 10);
            gfx.DrawString($"Total de conceptos:", fontNegrita, XBrushes.Black, 310, y + 30);
            gfx.DrawString($"{itemsConceptos.Count}", fontNormal, XBrushes.Black, 430, y + 30);
            gfx.DrawString($"Conceptos en reporte:", fontNegrita, XBrushes.Black, 310, y + 45);
            gfx.DrawString($"{conceptos.Count}", fontNormal, XBrushes.Black, 460, y + 45);

            y += 80;

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

            // Falta por ejecutar (dinero restante)
            double faltaPorEjecutar = totalPresupuestado - totalEjecutado;
            XColor colorFalta = faltaPorEjecutar <= 0 ? XColor.FromArgb(46, 204, 113) : XColor.FromArgb(231, 76, 60);
            gfx.DrawRectangle(penBorde, new XSolidBrush(colorFondo), xMetrica, y, anchoMetrica, 70);
            gfx.DrawString("FALTA POR EJECUTAR", fontPequena, new XSolidBrush(colorAccento),
                new XRect(xMetrica, y + 10, anchoMetrica, 20), XStringFormats.TopCenter);
            gfx.DrawString(faltaPorEjecutar.ToString("C2"), fontTituloGrande, new XSolidBrush(colorFalta),
                new XRect(xMetrica, y + 30, anchoMetrica, 30), XStringFormats.TopCenter);

            y += 90;

            int completadas = conceptos.Count(i => i.AvancePorcentaje >= 100);
            int enProgreso = conceptos.Count(i => i.AvancePorcentaje > 0 && i.AvancePorcentaje < 100);
            int sinIniciar = conceptos.Count(i => i.AvancePorcentaje == 0);

            gfx.DrawRectangle(penBorde, XBrushes.White, 40, y, 520, 70);
            gfx.DrawString("ESTADO DE CONCEPTOS SELECCIONADOS", fontSubtituloSeccion, new XSolidBrush(colorPrimario), 50, y + 10);

            int xEstado = 60;
            gfx.DrawRectangle(new XSolidBrush(colorSecundario), xEstado, y + 28, 10, 10);
            gfx.DrawString($"Completados: {completadas}", fontNormal, XBrushes.Black, xEstado + 15, y + 27);

            xEstado += 150;
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(243, 156, 18)), xEstado, y + 28, 10, 10);
            gfx.DrawString($"En progreso: {enProgreso}", fontNormal, XBrushes.Black, xEstado + 15, y + 27);

            xEstado += 150;
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(189, 195, 199)), xEstado, y + 28, 10, 10);
            gfx.DrawString($"Sin iniciar: {sinIniciar}", fontNormal, XBrushes.Black, xEstado + 15, y + 27);

            gfx.DrawString($"* Este reporte incluye los {conceptos.Count} conceptos seleccionados de {itemsConceptos.Count} totales",
                fontPequena, new XSolidBrush(XColor.FromArgb(127, 140, 141)),
                new XRect(40, y + 50, 520, 15), XStringFormats.TopCenter);

            y += 80;

            DibujarGraficaPastelPDF(gfx, new XRect(180, y, 240, 240), avanceGeneral, colorPrimario, colorSecundario);

            DibujarPiePagina(gfx, page, 1, fontPequena);

            // ============= P�GINA 2: DETALLE DE CONCEPTOS =============
            GenerarPaginasDetalleConceptos(pdf, conceptos, fontTitulo, fontSubtituloSeccion, fontNormal, 
                fontPequena, fontNegrita, colorPrimario, colorSecundario, colorAccento, colorFondo);

            pdf.Save(rutaPdf);
        }

        private void GenerarPaginasDetalleConceptos(PdfDocument pdf, List<ItemEstimacionConcepto> conceptos,
            XFont fontTitulo, XFont fontSubtituloSeccion, XFont fontNormal, XFont fontPequena, XFont fontNegrita,
            XColor colorPrimario, XColor colorSecundario, XColor colorAccento, XColor colorFondo)
        {
            var conceptosOrdenados = conceptos.OrderBy(i => i.Codigo).ToList();

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
            gfxPagina.DrawString("C�d.", fontNegrita, XBrushes.White, 45, y + 7);
            gfxPagina.DrawString("Concepto", fontNegrita, XBrushes.White, 90, y + 7);
            gfxPagina.DrawString("Presupuesto", fontNegrita, XBrushes.White, 320, y + 7);
            gfxPagina.DrawString("Avance", fontNegrita, XBrushes.White, 420, y + 7);
            gfxPagina.DrawString("Ejecutado", fontNegrita, XBrushes.White, 490, y + 7);
            y += 25;

            bool alternar = false;
            foreach (var concepto in conceptosOrdenados)
            {
                if (y > pagina.Height - 80)
                {
                    DibujarPiePagina(gfxPagina, pagina, numeroPagina++, fontPequena);
                    pagina = pdf.AddPage();
                    pagina.Size = PdfSharp.PageSize.Letter;
                    gfxPagina = XGraphics.FromPdfPage(pagina);
                    gfxPagina.DrawRectangle(new XSolidBrush(colorPrimario), 0, 0, pagina.Width, 50);
                    gfxPagina.DrawString("DETALLE DE CONCEPTOS (continuaci�n)", fontSubtituloSeccion, XBrushes.White,
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

            gfx.DrawString("Sistema Calandria Residencial - Estimaci�n por Conceptos", fontPequena,
                XBrushes.Gray, 40, yPie);

            gfx.DrawString($"P�gina {numeroPagina}", fontPequena, XBrushes.Gray,
                new XRect(0, yPie, page.Width, 20), XStringFormats.TopCenter);

            gfx.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), fontPequena, XBrushes.Gray,
                new XRect(0, yPie, page.Width - 40, 20), XStringFormats.TopRight);
        }

        private void FormEstimacionConcepto_Resize(object sender, EventArgs e)
        {
            if (itemsConceptos.Count > 0)
            {
                DibujarGraficas();
            }
        }
    }

    public class ItemEstimacionConcepto
    {
        public bool Incluir { get; set; } // ? Para checkbox de inclusi�n en PDF
        public string Codigo { get; set; }
        public string Concepto { get; set; }
        public double Total { get; set; }
        public double AvancePorcentaje { get; set; }
        public double MontoEjecutado { get; set; }
    }
}
