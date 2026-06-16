// Consulta de materiales por casa - FormAlmacen
// Muestra los destajos finalizados de tipo "Material" para una casa específica,
// uniendo ActivacionTareasRuta con las rutas (Tunera/Calandra) y sus columnas
// (Cantidad, Unidad, Precio). El "inventario por casa" equivale al material
// realmente invertido (destajos finalizados marcados como Material).
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        // Nombres físicos de las tablas de rutas
        private const string TablaRutaTunera = "RutaTuneraDestajo";
        private const string TablaRutaCalandra = "RutaCalandraDestajo";

        private bool _consultaWired = false;

        /// <summary>
        /// Carga las manzanas disponibles a partir de las casas con destajos activados.
        /// </summary>
        private void CargarManzanasConsulta()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT DISTINCT Manzana
                        FROM ActivacionTareasRuta
                        WHERE Manzana IS NOT NULL AND Manzana <> ''
                        ORDER BY Manzana";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbManzanaConsulta.Items.Clear();
                        cmbManzanaConsulta.Items.Add("-- Seleccionar --");

                        while (reader.Read())
                        {
                            cmbManzanaConsulta.Items.Add(reader["Manzana"].ToString());
                        }

                        if (cmbManzanaConsulta.Items.Count > 0)
                            cmbManzanaConsulta.SelectedIndex = 0;
                    }
                }

                CablearEventosConsultaCasa();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar manzanas:\n\n{ex.Message}",
                    "Error de carga",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Suscribe el handler del filtro de ruta a la primera invocación (no antes,
        /// porque el ComboBox lo crea el Layout en runtime).
        /// </summary>
        private void CablearEventosConsultaCasa()
        {
            if (_consultaWired || cmbRutaConsulta == null) return;
            cmbRutaConsulta.SelectedIndexChanged += (s, e) =>
            {
                if (cmbManzanaConsulta.SelectedIndex > 0 && cmbLoteConsulta.SelectedIndex > 0)
                    CargarMaterialesPorCasa();
            };
            _consultaWired = true;
        }

        /// <summary>
        /// Carga los lotes disponibles para la manzana seleccionada.
        /// </summary>
        private void CmbManzanaConsulta_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbLoteConsulta.Items.Clear();
            cmbLoteConsulta.Text = "";
            dgvConsultaCasa.DataSource = null;
            LimpiarEstadisticasConsulta();

            if (cmbManzanaConsulta.SelectedIndex <= 0 ||
                cmbManzanaConsulta.SelectedItem.ToString() == "-- Seleccionar --")
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT DISTINCT Lote
                        FROM ActivacionTareasRuta
                        WHERE Manzana = @manzana
                          AND Lote IS NOT NULL AND Lote <> ''
                        ORDER BY Lote";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@manzana", cmbManzanaConsulta.SelectedItem.ToString());

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbLoteConsulta.Items.Add("-- Seleccionar --");
                            while (reader.Read())
                                cmbLoteConsulta.Items.Add(reader["Lote"].ToString());

                            if (cmbLoteConsulta.Items.Count > 0)
                                cmbLoteConsulta.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar lotes:\n\n{ex.Message}",
                    "Error de carga",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Al elegir un lote concreto, carga los materiales finalizados.
        /// </summary>
        private void CmbLoteConsulta_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLoteConsulta.SelectedIndex <= 0 ||
                cmbLoteConsulta.SelectedItem.ToString() == "-- Seleccionar --")
            {
                dgvConsultaCasa.DataSource = null;
                LimpiarEstadisticasConsulta();
                return;
            }

            CargarMaterialesPorCasa();
        }

        /// <summary>
        /// Carga los destajos finalizados de tipo Material asignados a la casa,
        /// filtrados por la ruta seleccionada (Todas / Tunera / Calandra).
        /// </summary>
        private void CargarMaterialesPorCasa()
        {
            try
            {
                string manzana = cmbManzanaConsulta.SelectedItem.ToString();
                string lote = cmbLoteConsulta.SelectedItem.ToString();
                string filtroRuta = cmbRutaConsulta?.SelectedItem?.ToString() ?? RutaFiltroTodas;

                DataTable dt = ConstruirTablaMateriales();

                bool incluirTunera = filtroRuta == RutaFiltroTodas || filtroRuta == RutaFiltroTunera;
                bool incluirCalandra = filtroRuta == RutaFiltroTodas || filtroRuta == RutaFiltroCalandra;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (incluirTunera) CargarMaterialesDeRuta(conn, dt, TablaRutaTunera, manzana, lote);
                    if (incluirCalandra) CargarMaterialesDeRuta(conn, dt, TablaRutaCalandra, manzana, lote);
                }

                dgvConsultaCasa.DataSource = dt;
                ConfigurarColumnasConsultaCasa();
                ActualizarEstadisticasConsulta(dt, manzana, lote);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar materiales de la casa:\n\n{ex.Message}",
                    "Error de carga",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Crea el esquema de la tabla en memoria con tipos fuertes para que las
        /// columnas numéricas y de fecha se formateen correctamente.
        /// </summary>
        private static DataTable ConstruirTablaMateriales()
        {
            var dt = new DataTable();
            dt.Columns.Add("Ruta", typeof(string));
            dt.Columns.Add("Destajo", typeof(string));
            dt.Columns.Add("Material", typeof(string));
            dt.Columns.Add("Descripcion", typeof(string));
            dt.Columns.Add("Unidad", typeof(string));
            dt.Columns.Add("Cantidad", typeof(decimal));
            dt.Columns.Add("PrecioUnitario", typeof(decimal));
            dt.Columns.Add("Importe", typeof(decimal));
            dt.Columns.Add("Cuadrilla", typeof(string));
            dt.Columns.Add("FechaActivacion", typeof(DateTime));
            dt.Columns.Add("FechaFinalizacion", typeof(DateTime));
            dt.Columns.Add("Prototipo", typeof(string));
            return dt;
        }

        /// <summary>
        /// Ejecuta la consulta JOIN para una ruta específica y agrega filas al DataTable.
        /// Cada fila corresponde a una tarea hija de tipo Material cuyo destajo padre
        /// (Sub-Padre) está finalizado para la casa. Cantidad/Unidad/Precio provienen
        /// de las columnas dinámicas de la tarea hija.
        /// </summary>
        private static void CargarMaterialesDeRuta(SqlConnection conn, DataTable dt, string tablaRuta, string manzana, string lote)
        {
            string sql = $@"
                SELECT
                    a.Prototipo,
                    parent.Nombre        AS DestajoNombre,
                    child.Nombre         AS MaterialNombre,
                    child.Descripcion    AS MaterialDescripcion,
                    a.CuadrillaAsignada,
                    a.FechaActivacion,
                    a.FechaFinalizacion,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad'   THEN c.Valor END), '')  AS Unidad,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS PrecioUnitario
                FROM [{tablaRuta}] child
                INNER JOIN [{tablaRuta}] parent
                       ON child.ParentId = parent.ID
                INNER JOIN ActivacionTareasRuta a
                       ON a.NodoID = parent.ID
                      AND a.Ruta   = @ruta
                LEFT JOIN [{tablaRuta}_Columnas] c
                       ON c.NodoID = child.ID
                WHERE a.Manzana   = @m
                  AND a.Lote      = @l
                  AND a.Finalizado = 1
                  AND child.TipoTarea = 1   /* TipoTarea.Material */
                GROUP BY a.Prototipo, parent.Nombre, child.Nombre, child.Descripcion,
                         a.CuadrillaAsignada, a.FechaActivacion, a.FechaFinalizacion
                ORDER BY a.FechaFinalizacion DESC, parent.Nombre, child.Nombre";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.Parameters.AddWithValue("@ruta", tablaRuta);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal cantidad = ParseDecimal(reader["Cantidad"]);
                        decimal precio = ParseDecimal(reader["PrecioUnitario"]);
                        decimal importe = cantidad * precio;

                        DataRow row = dt.NewRow();
                        row["Ruta"] = RutaCorta(tablaRuta);
                        row["Destajo"] = reader["DestajoNombre"]?.ToString() ?? string.Empty;
                        row["Material"] = reader["MaterialNombre"]?.ToString() ?? string.Empty;
                        row["Descripcion"] = reader["MaterialDescripcion"] == DBNull.Value
                            ? ""
                            : reader["MaterialDescripcion"].ToString();
                        row["Unidad"] = reader["Unidad"]?.ToString() ?? "";
                        row["Cantidad"] = cantidad;
                        row["PrecioUnitario"] = precio;
                        row["Importe"] = importe;
                        row["Cuadrilla"] = reader["CuadrillaAsignada"] == DBNull.Value
                            ? ""
                            : reader["CuadrillaAsignada"].ToString();
                        row["FechaActivacion"] = reader["FechaActivacion"] == DBNull.Value
                            ? (object)DBNull.Value
                            : Convert.ToDateTime(reader["FechaActivacion"]);
                        row["FechaFinalizacion"] = reader["FechaFinalizacion"] == DBNull.Value
                            ? (object)DBNull.Value
                            : Convert.ToDateTime(reader["FechaFinalizacion"]);
                        row["Prototipo"] = reader["Prototipo"] == DBNull.Value
                            ? ""
                            : reader["Prototipo"].ToString();
                        dt.Rows.Add(row);
                    }
                }
            }
        }

        private static decimal ParseDecimal(object valor)
        {
            if (valor == null || valor == DBNull.Value) return 0m;
            string s = valor.ToString();
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal r)) return r;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal r2)) return r2;
            return 0m;
        }

        private static string RutaCorta(string tablaRuta)
        {
            if (tablaRuta == TablaRutaTunera) return "Tunera";
            if (tablaRuta == TablaRutaCalandra) return "Calandra";
            return tablaRuta;
        }

        /// <summary>
        /// Aplica orden, anchos, formatos y resaltado al DataGridView.
        /// </summary>
        private void ConfigurarColumnasConsultaCasa()
        {
            if (dgvConsultaCasa.Columns.Count == 0) return;

            var columnas = new[]
            {
                new { Name = "Ruta",              Header = "Ruta",              Width = 90  },
                new { Name = "Destajo",           Header = "Destajo",           Width = 220 },
                new { Name = "Material",          Header = "Material",          Width = 220 },
                new { Name = "Descripcion",       Header = "Descripción",       Width = 240 },
                new { Name = "Unidad",            Header = "Unidad",            Width = 70  },
                new { Name = "Cantidad",          Header = "Cantidad",          Width = 90  },
                new { Name = "PrecioUnitario",   Header = "P.U.",              Width = 90  },
                new { Name = "Importe",           Header = "Importe",           Width = 110 },
                new { Name = "Cuadrilla",         Header = "Cuadrilla",         Width = 100 },
                new { Name = "FechaActivacion",   Header = "Activado",          Width = 110 },
                new { Name = "FechaFinalizacion", Header = "Finalizado",        Width = 110 },
                new { Name = "Prototipo",         Header = "Prototipo",         Width = 100 }
            };

            foreach (var col in columnas)
            {
                if (dgvConsultaCasa.Columns.Contains(col.Name))
                {
                    var c = dgvConsultaCasa.Columns[col.Name];
                    c.HeaderText = col.Header;
                    c.Width = col.Width;
                    c.ReadOnly = true;
                    c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }

            // Formato moneda
            if (dgvConsultaCasa.Columns.Contains("PrecioUnitario"))
            {
                dgvConsultaCasa.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
                dgvConsultaCasa.Columns["PrecioUnitario"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvConsultaCasa.Columns.Contains("Importe"))
            {
                dgvConsultaCasa.Columns["Importe"].DefaultCellStyle.Format = "C2";
                dgvConsultaCasa.Columns["Importe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvConsultaCasa.Columns.Contains("Cantidad"))
            {
                dgvConsultaCasa.Columns["Cantidad"].DefaultCellStyle.Format = "N2";
                dgvConsultaCasa.Columns["Cantidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (dgvConsultaCasa.Columns.Contains("FechaActivacion"))
                dgvConsultaCasa.Columns["FechaActivacion"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgvConsultaCasa.Columns.Contains("FechaFinalizacion"))
                dgvConsultaCasa.Columns["FechaFinalizacion"].DefaultCellStyle.Format = "dd/MM/yyyy";

            if (dgvConsultaCasa.Columns.Contains("Ruta"))
                dgvConsultaCasa.Columns["Ruta"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EstilizarDataGridViewAlmacen(dgvConsultaCasa);

            // Pintar suavemente las filas según la ruta para discriminar visualmente.
            foreach (DataGridViewRow row in dgvConsultaCasa.Rows)
            {
                if (row.IsNewRow) continue;
                string ruta = row.Cells["Ruta"].Value?.ToString();
                if (ruta == "Tunera")
                {
                    row.Cells["Ruta"].Style.ForeColor = ThemeManager.ColorPrincipalMenuBar;
                    row.Cells["Ruta"].Style.Font = new Font(dgvConsultaCasa.Font, FontStyle.Bold);
                }
                else if (ruta == "Calandra")
                {
                    row.Cells["Ruta"].Style.ForeColor = ThemeManager.ColorInfo;
                    row.Cells["Ruta"].Style.Font = new Font(dgvConsultaCasa.Font, FontStyle.Bold);
                }
            }
        }

        /// <summary>
        /// Actualiza badge, prototipo y métricas de la casa seleccionada.
        /// </summary>
        private void ActualizarEstadisticasConsulta(DataTable dt, string manzana, string lote)
        {
            try
            {
                int totalMateriales = dt.Rows.Count;
                decimal totalImporte = 0m;
                string prototipo = "";

                foreach (DataRow row in dt.Rows)
                {
                    if (row["Importe"] != DBNull.Value)
                        totalImporte += Convert.ToDecimal(row["Importe"]);
                    if (string.IsNullOrEmpty(prototipo) && row["Prototipo"] != DBNull.Value)
                        prototipo = row["Prototipo"].ToString();
                }

                lblCasaSeleccionada.Text = $"Casa M{manzana} · L{lote}";
                ActualizarBadgeCasa(lblCasaSeleccionada, true);

                lblPrototipoConsulta.Text = string.IsNullOrEmpty(prototipo)
                    ? ""
                    : $"Prototipo: {prototipo}";
                lblPrototipoConsulta.ForeColor = ThemeManager.ColorTextoOscuro;

                int destajosDistintos = 0;
                var setDestajos = new System.Collections.Generic.HashSet<string>();
                foreach (DataRow row in dt.Rows)
                {
                    string clave = $"{row["Ruta"]}|{row["Destajo"]}";
                    if (setDestajos.Add(clave)) destajosDistintos++;
                }

                lblTotalInsumosConsulta.Text = totalMateriales == 1
                    ? "1 material · 1 destajo"
                    : $"{totalMateriales} materiales · {destajosDistintos} destajos";
                lblTotalImporteConsulta.Text = $"Importe invertido: {totalImporte:C2}";

                lblDiscrepanciasConsulta.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al actualizar estadísticas: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Reinicia los textos/estado de la cabecera cuando no hay casa elegida.
        /// </summary>
        private void LimpiarEstadisticasConsulta()
        {
            lblCasaSeleccionada.Text = "Seleccione una casa para ver sus materiales invertidos";
            ActualizarBadgeCasa(lblCasaSeleccionada, false);

            lblPrototipoConsulta.Text = "";
            lblTotalInsumosConsulta.Text = "";
            lblTotalImporteConsulta.Text = "";
            lblDiscrepanciasConsulta.Visible = false;
        }

        /// <summary>
        /// Filtra el grid por texto libre en columnas relevantes.
        /// </summary>
        private void TxtBuscarConsultaCasa_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvConsultaCasa.DataSource is DataTable dt)
                {
                    string filtro = txtBuscarConsultaCasa.Text.Trim().Replace("'", "''");

                    if (string.IsNullOrEmpty(filtro))
                    {
                        dt.DefaultView.RowFilter = string.Empty;
                    }
                    else
                    {
                        dt.DefaultView.RowFilter = $@"
                            Destajo LIKE '%{filtro}%' OR
                            Material LIKE '%{filtro}%' OR
                            Descripcion LIKE '%{filtro}%' OR
                            Unidad LIKE '%{filtro}%' OR
                            Cuadrilla LIKE '%{filtro}%' OR
                            Prototipo LIKE '%{filtro}%' OR
                            Ruta LIKE '%{filtro}%'";
                    }
                }
            }
            catch (Exception)
            {
                if (dgvConsultaCasa.DataSource is DataTable dt)
                    dt.DefaultView.RowFilter = string.Empty;
            }
        }

        private void BtnExportarConsultaCasa_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvConsultaCasa.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "No hay datos para exportar. Primero seleccione una casa con destajos Material finalizados.",
                        "Sin datos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string manzana = cmbManzanaConsulta.SelectedItem.ToString();
                string lote = cmbLoteConsulta.SelectedItem.ToString();

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                    FileName = $"Materiales_M{manzana}_L{lote}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    Title = "Guardar materiales de casa"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportarConsultaCasaExcel(saveDialog.FileName, manzana, lote);

                    var result = MessageBox.Show(
                        $"Archivo guardado en:\n\n{saveDialog.FileName}\n\n¿Desea abrirlo?",
                        "Exportación correcta",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);
                    if (result == DialogResult.Yes)
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al exportar a Excel:\n\n{ex.Message}",
                    "Error de exportación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Genera el archivo Excel con las columnas del nuevo esquema.
        /// </summary>
        private void ExportarConsultaCasaExcel(string filePath, string manzana, string lote)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Materiales");

                ws.Cell(1, 1).Value = "MATERIALES INVERTIDOS POR CASA · Calandria Residencial";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(179, 108, 46);
                ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
                ws.Range(1, 1, 1, 12).Merge();

                ws.Cell(2, 1).Value = $"Casa: Manzana {manzana} · Lote {lote}";
                ws.Cell(2, 1).Style.Font.Bold = true;
                ws.Cell(2, 1).Style.Font.FontSize = 12;
                ws.Range(2, 1, 2, 12).Merge();

                ws.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Cell(3, 1).Style.Font.Italic = true;
                ws.Range(3, 1, 3, 12).Merge();

                int row = 5;
                var headers = new[]
                {
                    "Ruta", "Destajo", "Material", "Descripción", "Unidad",
                    "Cantidad", "P.U.", "Importe", "Cuadrilla",
                    "Activado", "Finalizado", "Prototipo"
                };

                int col = 1;
                foreach (var header in headers)
                {
                    ws.Cell(row, col).Value = header;
                    ws.Cell(row, col).Style.Font.Bold = true;
                    ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    col++;
                }

                row = 6;
                foreach (DataGridViewRow dgvRow in dgvConsultaCasa.Rows)
                {
                    if (dgvRow.IsNewRow) continue;
                    col = 1;
                    ws.Cell(row, col++).Value = dgvRow.Cells["Ruta"].Value?.ToString() ?? "";
                    ws.Cell(row, col++).Value = dgvRow.Cells["Destajo"].Value?.ToString() ?? "";
                    ws.Cell(row, col++).Value = dgvRow.Cells["Material"].Value?.ToString() ?? "";
                    ws.Cell(row, col++).Value = dgvRow.Cells["Descripcion"].Value?.ToString() ?? "";
                    ws.Cell(row, col++).Value = dgvRow.Cells["Unidad"].Value?.ToString() ?? "";
                    ws.Cell(row, col++).Value = ValorNumerico(dgvRow.Cells["Cantidad"].Value);
                    ws.Cell(row, col++).Value = ValorNumerico(dgvRow.Cells["PrecioUnitario"].Value);
                    ws.Cell(row, col++).Value = ValorNumerico(dgvRow.Cells["Importe"].Value);
                    ws.Cell(row, col++).Value = dgvRow.Cells["Cuadrilla"].Value?.ToString() ?? "";
                    ws.Cell(row, col++).Value = ValorFecha(dgvRow.Cells["FechaActivacion"].Value);
                    ws.Cell(row, col++).Value = ValorFecha(dgvRow.Cells["FechaFinalizacion"].Value);
                    ws.Cell(row, col++).Value = dgvRow.Cells["Prototipo"].Value?.ToString() ?? "";
                    row++;
                }

                ws.Columns().AdjustToContents();
                ws.Column(6).Style.NumberFormat.Format = "0.00";        // Cantidad
                ws.Column(7).Style.NumberFormat.Format = "$#,##0.00";  // P.U.
                ws.Column(8).Style.NumberFormat.Format = "$#,##0.00";  // Importe

                workbook.SaveAs(filePath);
            }
        }

        private static double ValorNumerico(object v)
        {
            if (v == null || v == DBNull.Value) return 0d;
            return Convert.ToDouble(v);
        }

        private static string ValorFecha(object v)
        {
            if (v == null || v == DBNull.Value) return "";
            return Convert.ToDateTime(v).ToString("dd/MM/yyyy");
        }

        private void BtnActualizarConsultaCasa_Click(object sender, EventArgs e)
        {
            if (cmbManzanaConsulta.SelectedIndex > 0 && cmbLoteConsulta.SelectedIndex > 0)
            {
                CargarMaterialesPorCasa();
                ActualizarStatusFooter($"Consulta actualizada · M{cmbManzanaConsulta.SelectedItem} L{cmbLoteConsulta.SelectedItem}");
            }
            else
            {
                MessageBox.Show(
                    "Debe seleccionar una manzana y un lote primero.",
                    "Selección requerida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
