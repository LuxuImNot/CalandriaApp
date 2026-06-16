using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Globalization;

namespace DynamicSepticSystem
{
    public partial class FormManoObra : Form
    {
        public Tarea Tarea { get; private set; }
        private PanelPrincipal.Casa _casa;
        private CasaInventario _inventario;
        private readonly InventarioService _svc = new InventarioService();

        public FormManoObra()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            // wire events if designer assigned
            try { dgvMano.CellContentClick += DgvMano_CellContentClick; } catch { }
        }

        public FormManoObra(Tarea tarea) : this()
        {
            Tarea = tarea;
            lblTitle.Text = "Formulario Mano de Obra para: " + Tarea?.Nombre;
            this.Text = "Mano de Obra - " + (Tarea?.Nombre ?? "");
        }

        public void SetCasa(PanelPrincipal.Casa casa, CasaInventario inventario)
        {
            _casa = casa;
            _inventario = inventario;
            LoadManoObraForPrototipo(inventario?.Prototipo);

            // try to load previously saved delivered mano
            try
            {
                if (_casa != null)
                {
                    var dtSaved = _svc.ObtenerManoObraEntregada(_casa.Manzana, _casa.Lote, Tarea?.WBS);
                    if (dtSaved != null && dgvMano.DataSource is System.Data.DataTable dtCurrent)
                    {
                        System.Collections.Generic.List<string> keys = null;
                        if (dtSaved.Columns.Contains("Clave"))
                        {
                            keys = new System.Collections.Generic.List<string>();
                            foreach (System.Data.DataRow rr in dtSaved.Rows)
                            {
                                try
                                {
                                    var v = rr["Clave"]?.ToString();
                                    if (!string.IsNullOrWhiteSpace(v)) keys.Add(v);
                                }
                                catch { }
                            }
                        }

                        if (keys != null && keys.Count > 0)
                        {
                            foreach (System.Data.DataRow row in dtCurrent.Rows)
                            {
                                var clave = row.Table.Columns.Contains("Clave") ? row["Clave"]?.ToString() : null;
                                if (!string.IsNullOrWhiteSpace(clave) && keys.Contains(clave)) row["Entregado"] = true;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void LoadManoObraForPrototipo(string prototipo)
        {
            if (string.IsNullOrWhiteSpace(prototipo)) return;
            string tabla = prototipo.ToUpper().Contains("TUNERA") ? "ManoObraTunera" : "ManoObraCalandra";
            // Try several queries to be tolerant with column names/encodings
            string[] candidateSqls = new[]
            {
                // preferred: alias Descripci�n -> Descripcion
                $"SELECT Clave, [Descripci�n] AS Descripcion, Unidad, Cantidad, Costo, Importe, Porcentaje FROM dbo.{tabla} ORDER BY Clave",
                // without alias (some DBs may expose column with accent and driver maps differently)
                $"SELECT Clave, [Descripci�n], Unidad, Cantidad, Costo, Importe, Porcentaje FROM dbo.{tabla} ORDER BY Clave",
                // fallback: select all
                $"SELECT * FROM dbo.{tabla} ORDER BY Clave"
            };

            var dt = new System.Data.DataTable();
            Exception lastEx = null;
            bool loaded = false;
            foreach (var s in candidateSqls)
            {
                try
                {
                    dt.Clear();
                    using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString))
                    using (var da = new System.Data.SqlClient.SqlDataAdapter(s, conn))
                    {
                        da.Fill(dt);
                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        loaded = true;
                        // If query returned column 'Descripci�n' but not 'Descripcion', normalize
                        if (!dt.Columns.Contains("Descripcion") && dt.Columns.Contains("Descripci�n"))
                        {
                            dt.Columns.Add("Descripcion", typeof(string));
                            foreach (System.Data.DataRow r in dt.Rows)
                                r["Descripcion"] = r["Descripci�n"] ?? string.Empty;
                        }

                        break;
                    }
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    // try next
                }
            }

            if (!loaded)
            {
                if (lastEx != null)
                    MessageBox.Show("Error al obtener mano de obra: " + lastEx.Message, "Mano de obra", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show($"No se encontraron registros en la tabla de mano de obra ({tabla}). Verifica que exista y que el prototipo sea correcto.", "Mano de obra", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Ensure expected columns exist with safe defaults
            if (dt.Columns.Contains("Descripcion") && !dt.Columns.Contains("Descripci�n")) { /* ok */ }
            // Replace DBNull with empty string for display
            try
            {
                foreach (System.Data.DataRow r in dt.Rows)
                {
                    foreach (System.Data.DataColumn c in dt.Columns)
                    {
                        if (r.IsNull(c))
                        {
                            if (c.DataType == typeof(string)) r[c] = string.Empty;
                        }
                    }
                }
            }
            catch { }

            if (!dt.Columns.Contains("Entregado")) dt.Columns.Add("Entregado", typeof(bool));
            dgvMano.DataSource = dt;
        }

        private void DgvMano_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // placeholder: can handle checkbox toggles
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (_casa != null && dgvMano.DataSource is System.Data.DataTable dt)
                {
                    _svc.GuardarManoObraEntregada(_casa.Manzana, _casa.Lote, Tarea?.WBS, dt.Clone());
                    // save only Entregado rows
                    var entregados = dt.Clone();
                    foreach (System.Data.DataRow r in dt.Rows)
                    {
                        try
                        {
                            var val = r["Entregado"];
                            bool b = false;
                            if (val != DBNull.Value && val != null) b = Convert.ToBoolean(val);
                            if (b) entregados.ImportRow(r);
                        }
                        catch { }
                    }
                    _svc.GuardarManoObraEntregada(_casa.Manzana, _casa.Lote, Tarea?.WBS, entregados);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error guardando mano de obra: " + ex.Message);
            }
        }

        private void btnPdf_Click(object sender, EventArgs e)
        {
            try
            {
                System.Data.DataTable dtEntregados = null;
                if (_casa != null)
                    dtEntregados = _svc.ObtenerManoObraEntregada(_casa.Manzana, _casa.Lote, Tarea?.WBS);

                if (dtEntregados == null && dgvMano.DataSource is System.Data.DataTable dt)
                {
                    // build entregados from dt
                    var entregados = dt.Clone();
                    foreach (System.Data.DataRow r in dt.Rows)
                    {
                        try
                        {
                            var val = r["Entregado"];
                            bool b = false;
                            if (val != DBNull.Value && val != null) b = Convert.ToBoolean(val);
                            if (b) entregados.ImportRow(r);
                        }
                        catch { }
                    }
                    dtEntregados = entregados;
                }

                if (dtEntregados == null || dtEntregados.Rows.Count == 0)
                {
                    MessageBox.Show("No hay mano de obra entregada para generar el reporte.", "Reporte", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // generate temp pdf
                string carpetaTemp = Path.Combine(Path.GetTempPath(), "CalandriaReports");
                Directory.CreateDirectory(carpetaTemp);
                string tempFile = Path.Combine(carpetaTemp, $"ReporteMano_{Tarea.WBS ?? Tarea.Nombre}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                // reuse simple PDF layout: use clave, descripcion, importe
                var document = new PdfSharp.Pdf.PdfDocument();
                document.Info.Title = "Reporte Mano de Obra";
                var page = document.AddPage();
                var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
                var font = new PdfSharp.Drawing.XFont("Arial", 10, PdfSharp.Drawing.XFontStyle.Regular);
                double y = 40;
                gfx.DrawString("REPORTE MANO DE OBRA", new PdfSharp.Drawing.XFont("Arial", 12, PdfSharp.Drawing.XFontStyle.Bold), PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XRect(0, y, page.Width, 20), PdfSharp.Drawing.XStringFormats.TopCenter);
                y += 30;
                double xStart = 40;
                double[] colWidths = { 80, 260, 100 };
                double rowHeight = 20;
                double x = xStart;
                string[] headers = { "COD", "DESCRIPCION", "IMPORTE" };
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, x, y, colWidths[i], rowHeight);
                    gfx.DrawString(headers[i], font, PdfSharp.Drawing.XBrushes.Black, x + 3, y + 3);
                    x += colWidths[i];
                }
                y += rowHeight;

                for (int i = 0; i < dtEntregados.Rows.Count; i++)
                {
                    var r = dtEntregados.Rows[i];
                    x = xStart;
                    double rowY = y + rowHeight * i;
                    string clave = r.Table.Columns.Contains("Clave") ? r["Clave"].ToString() : "";
                    string desc = r.Table.Columns.Contains("Descripcion") ? r["Descripcion"].ToString() : r.Table.Columns.Contains("Descripci�n") ? r["Descripci�n"].ToString() : "";
                    string imp = r.Table.Columns.Contains("Importe") ? r["Importe"].ToString() : "";
                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, x, rowY, colWidths[0], rowHeight);
                    gfx.DrawString(clave, font, PdfSharp.Drawing.XBrushes.Black, x + 5, rowY + 5);
                    x += colWidths[0];
                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, x, rowY, colWidths[1], rowHeight);
                    gfx.DrawString(desc.Length > 100 ? desc.Substring(0, 100) + "..." : desc, font, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XRect(x + 3, rowY + 3, colWidths[1] - 6, rowHeight), PdfSharp.Drawing.XStringFormats.TopLeft);
                    x += colWidths[1];
                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, x, rowY, colWidths[2], rowHeight);
                    gfx.DrawString(imp, font, PdfSharp.Drawing.XBrushes.Black, x + 5, rowY + 5);
                }

                // calcular importe total
                decimal total = 0m;
                try
                {
                    foreach (System.Data.DataRow rr in dtEntregados.Rows)
                    {
                        if (dtEntregados.Columns.Contains("Importe"))
                        {
                            var obj = rr["Importe"];
                            if (obj != null && obj != DBNull.Value)
                            {
                                var s = obj.ToString();
                                decimal v;
                                if (!decimal.TryParse(s, NumberStyles.Currency | NumberStyles.Number, CultureInfo.CurrentCulture, out v))
                                    decimal.TryParse(s, NumberStyles.Currency | NumberStyles.Number, CultureInfo.InvariantCulture, out v);
                                total += v;
                            }
                        }
                    }
                }
                catch { }

                double totalY = y + rowHeight * (dtEntregados.Rows.Count + 1) + 8;
                gfx.DrawString("IMPORTE TOTAL", font, PdfSharp.Drawing.XBrushes.Black, xStart + 270, totalY);
                gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, xStart + 370, totalY - 5, 100, rowHeight);
                gfx.DrawString(total.ToString("C2", CultureInfo.CurrentCulture), font, PdfSharp.Drawing.XBrushes.Black, xStart + 380, totalY + 2);

                document.Save(tempFile);

                using (var preview = new FormPdfPreview(tempFile, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ReporteMano.pdf")))
                {
                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generando PDF mano de obra: " + ex.Message);
            }
        }
    }
}
