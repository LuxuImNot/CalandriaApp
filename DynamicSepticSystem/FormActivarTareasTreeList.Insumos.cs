// Apartado "Insumos" del formulario de activacion de destajos: enlista todos los
// insumos Material de la casa comparando lo PROGRAMADO (de los destajos) contra
// lo USADO (salidas del almacen hacia esa Manzana/Lote).
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormActivarTareasTreeList
    {
        private void MostrarInsumos()
        {
            if (itemsTareas == null || itemsTareas.Count == 0)
            {
                MessageBox.Show("Carga las tareas de una Manzana y Lote para ver los insumos.",
                    "Insumos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataTable dt = ConstruirTablaInsumos();

            string titulo = string.IsNullOrEmpty(manzanaActual)
                ? "Insumos — Programados / Usados"
                : $"Insumos — M{manzanaActual} L{loteActual}  (Programados / Usados)";

            using (var dlg = new Form
            {
                Text = titulo,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(880, 620),
                MinimumSize = new Size(640, 420),
                ShowInTaskbar = false,
                Font = new Font("Segoe UI", 9F)
            })
            {
                var grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    DataSource = dt,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    EnableHeadersVisualStyles = false
                };
                grid.ColumnHeadersDefaultCellStyle.BackColor = GuiaPrimario;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                grid.ColumnHeadersHeight = 32;
                grid.RowTemplate.Height = 26;

                ConfigurarColumnasInsumos(grid);
                grid.CellFormatting += (s, e) => FormatearFilaInsumo(grid, e);

                // Resumen inferior
                decimal totalProg = dt.AsEnumerable().Sum(r => r.Field<decimal>("ImporteProg"));
                decimal totalUsado = dt.AsEnumerable().Sum(r => r.Field<decimal>("ImporteUsado"));
                int conExceso = dt.AsEnumerable().Count(r => r.Field<decimal>("Usado") > r.Field<decimal>("Programado"));

                var footer = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = GuiaSuave, Padding = new Padding(12, 8, 12, 8) };
                footer.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = GuiaBorde });
                footer.Controls.Add(new Label
                {
                    Dock = DockStyle.Left,
                    AutoSize = false,
                    Width = 600,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = GuiaTexto,
                    Text = $"Insumos: {dt.Rows.Count}     ·     Importe programado: {totalProg.ToString("C2", CultureInfo.CurrentCulture)}" +
                           $"     ·     Importe usado: {totalUsado.ToString("C2", CultureInfo.CurrentCulture)}" +
                           (conExceso > 0 ? $"     ·     ⚠ {conExceso} con exceso" : "")
                });

                var btnCerrar = new Button
                {
                    Text = "Cerrar",
                    Dock = DockStyle.Right,
                    Width = 110,
                    BackColor = Color.FromArgb(99, 110, 114),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.OK
                };
                btnCerrar.FlatAppearance.BorderSize = 0;
                footer.Controls.Add(btnCerrar);

                dlg.Controls.Add(grid);
                dlg.Controls.Add(footer);
                dlg.AcceptButton = btnCerrar;
                dlg.ShowDialog(this);
            }
        }

        /// <summary>
        /// Tabla con un renglon por insumo Material: Programado (destajos) vs Usado
        /// (salidas de almacen de la casa), cotejados por Clave (respaldo por nombre).
        /// </summary>
        private DataTable ConstruirTablaInsumos()
        {
            var dt = new DataTable();
            dt.Columns.Add("Clave", typeof(string));
            dt.Columns.Add("Insumo", typeof(string));
            dt.Columns.Add("Unidad", typeof(string));
            dt.Columns.Add("Programado", typeof(decimal));
            dt.Columns.Add("Usado", typeof(decimal));
            dt.Columns.Add("Pendiente", typeof(decimal));
            dt.Columns.Add("ImporteProg", typeof(decimal));
            dt.Columns.Add("ImporteUsado", typeof(decimal));

            var indice = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);

            string Clave(string clave, string nombre) =>
                !string.IsNullOrWhiteSpace(clave) ? "C:" + clave.Trim().ToUpperInvariant()
                                                  : "N:" + (nombre ?? "").Trim().ToUpperInvariant();

            // --- Programado: insumos Material (Nivel 2) de los destajos ---
            var programados = itemsTareas
                .Where(i => i.Nivel == 2 && i.TipoTareaEnum == TipoTarea.Material)
                .GroupBy(i => Clave(i.Clave, i.Nombre));

            foreach (var g in programados)
            {
                var primero = g.First();
                decimal cant = g.Sum(x => x.Cantidad);
                decimal precio = primero.PrecioUnitario;
                var row = dt.NewRow();
                row["Clave"] = primero.Clave ?? "";
                row["Insumo"] = primero.Nombre ?? "";
                row["Unidad"] = primero.Unidad ?? "";
                row["Programado"] = cant;
                row["Usado"] = 0m;
                row["Pendiente"] = cant;
                row["ImporteProg"] = cant * precio;
                row["ImporteUsado"] = 0m;
                dt.Rows.Add(row);
                indice[g.Key] = row;
            }

            // --- Usado: salidas del almacen hacia esta casa ---
            if (!string.IsNullOrEmpty(manzanaActual) && !string.IsNullOrEmpty(loteActual))
            {
                try
                {
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(@"
                            SELECT ISNULL(Clave,'') AS Clave,
                                   MAX(Descripcion) AS Descripcion,
                                   MAX(Unidad) AS Unidad,
                                   SUM(Cantidad) AS Usado,
                                   SUM(Importe) AS Importe
                            FROM dbo.SalidasAlmacen
                            WHERE Manzana = @m AND Lote = @l
                            GROUP BY ISNULL(Clave,'')", conn))
                        {
                            cmd.Parameters.AddWithValue("@m", manzanaActual);
                            cmd.Parameters.AddWithValue("@l", loteActual);
                            using (var rd = cmd.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    string clave = rd["Clave"]?.ToString() ?? "";
                                    string desc = rd["Descripcion"]?.ToString() ?? "";
                                    string unidad = rd["Unidad"]?.ToString() ?? "";
                                    decimal usado = rd["Usado"] != DBNull.Value ? Convert.ToDecimal(rd["Usado"]) : 0m;
                                    decimal importe = rd["Importe"] != DBNull.Value ? Convert.ToDecimal(rd["Importe"]) : 0m;

                                    string key = Clave(clave, desc);
                                    if (indice.TryGetValue(key, out var row))
                                    {
                                        row["Usado"] = usado;
                                        row["ImporteUsado"] = importe;
                                        row["Pendiente"] = (decimal)row["Programado"] - usado;
                                    }
                                    else
                                    {
                                        var nuevo = dt.NewRow();
                                        nuevo["Clave"] = clave;
                                        nuevo["Insumo"] = desc;
                                        nuevo["Unidad"] = unidad;
                                        nuevo["Programado"] = 0m;
                                        nuevo["Usado"] = usado;
                                        nuevo["Pendiente"] = -usado;
                                        nuevo["ImporteProg"] = 0m;
                                        nuevo["ImporteUsado"] = importe;
                                        dt.Rows.Add(nuevo);
                                        indice[key] = nuevo;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudieron leer los insumos usados del almacen:\n\n" + ex.Message,
                        "Insumos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // Orden: por Insumo
            dt.DefaultView.Sort = "Insumo ASC";
            return dt.DefaultView.ToTable();
        }

        private void ConfigurarColumnasInsumos(DataGridView grid)
        {
            // Ocultar columnas auxiliares de importe
            if (grid.Columns.Contains("ImporteProg")) grid.Columns["ImporteProg"].Visible = false;
            if (grid.Columns.Contains("ImporteUsado")) grid.Columns["ImporteUsado"].Visible = false;

            void Set(string nombre, string header, int weight, bool numerico)
            {
                if (!grid.Columns.Contains(nombre)) return;
                var c = grid.Columns[nombre];
                c.HeaderText = header;
                c.FillWeight = weight;
                if (numerico)
                {
                    c.DefaultCellStyle.Format = "N2";
                    c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            Set("Clave", "Clave", 18, false);
            Set("Insumo", "Insumo", 48, false);
            Set("Unidad", "Unidad", 12, false);
            Set("Programado", "Programado", 16, true);
            Set("Usado", "Usado", 16, true);
            Set("Pendiente", "Pendiente", 16, true);

            if (grid.Columns.Contains("Unidad"))
                grid.Columns["Unidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void FormatearFilaInsumo(DataGridView grid, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            var row = grid.Rows[e.RowIndex];
            if (row.DataBoundItem == null) return;

            decimal prog = Convert.ToDecimal(row.Cells["Programado"].Value ?? 0m);
            decimal usado = Convert.ToDecimal(row.Cells["Usado"].Value ?? 0m);

            // Exceso (usado mayor que lo programado): rojo suave.
            if (usado > prog)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);
                row.DefaultCellStyle.ForeColor = Color.FromArgb(192, 57, 43);
            }
            // Completamente usado (programado y usado == programado): verde suave.
            else if (prog > 0m && usado >= prog)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(240, 249, 243);
                row.DefaultCellStyle.ForeColor = GuiaTexto;
            }
            // Sin usar aun: gris.
            else if (usado == 0m)
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = GuiaTextoSuave;
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = GuiaTexto;
            }
        }
    }
}
