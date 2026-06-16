using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Reporte consolidado de nómina: muestra a todos los trabajadores y el monto
    /// total que deben recibir el sábado (la "raya"), sumando recibos de todas las
    /// cuadrillas en el periodo seleccionado.
    /// </summary>
    public class FormReporteNomina : Form
    {
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Button btnSemanaActual;
        private Button btnSemanaAnterior;
        private Button btnActualizar;
        private Button btnExportarPdf;
        private Button btnCerrar;
        private Label lblResumen;
        private TextBox txtBuscar;
        private ObjectListView olv;

        private List<NominaItem> _items = new List<NominaItem>();

        public FormReporteNomina()
        {
            BuildUI();
            ConfigurarLista();
            ThemeManager.AplicarTema(this);
            this.Load += (s, e) => CargarDatos();
        }

        #region UI

        private void BuildUI()
        {
            this.Text = "Reporte Semanal de Nómina";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(960, 620);
            this.ClientSize = new Size(1100, 720);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);

            Color colorPrimario = Color.FromArgb(41, 60, 88);
            Color colorAcento = Color.FromArgb(52, 152, 219);
            Color colorSuave = Color.FromArgb(248, 250, 253);
            Color colorBorde = Color.FromArgb(218, 224, 232);
            Color colorNeutro = Color.FromArgb(127, 140, 141);

            var banner = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = colorPrimario };
            banner.Controls.Add(new Label
            {
                Text = "LISTADO DE NÓMINA",
                Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 10)
            });
            banner.Controls.Add(new Label
            {
                Text = "Trabajadores y monto a recibir el sábado (raya)",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(189, 200, 215),
                AutoSize = true,
                Location = new Point(22, 38)
            });

            var filtros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = colorSuave,
                Padding = new Padding(20, 10, 20, 10)
            };

            int x = 22;
            filtros.Controls.Add(new Label
            {
                Text = "Desde",
                Location = new Point(x, 8),
                Size = new Size(80, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            });
            dtpDesde = new DateTimePicker
            {
                Location = new Point(x, 28),
                Size = new Size(140, 26),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F),
                Value = LunesDe(DateTime.Today)
            };
            filtros.Controls.Add(dtpDesde);

            x += 152;
            filtros.Controls.Add(new Label
            {
                Text = "Hasta",
                Location = new Point(x, 8),
                Size = new Size(80, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            });
            dtpHasta = new DateTimePicker
            {
                Location = new Point(x, 28),
                Size = new Size(140, 26),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F),
                Value = LunesDe(DateTime.Today).AddDays(5) // sábado
            };
            filtros.Controls.Add(dtpHasta);

            x += 152;
            btnSemanaActual = new Button
            {
                Text = "Semana actual",
                Location = new Point(x, 28),
                Size = new Size(120, 26),
                BackColor = Color.FromArgb(99, 110, 114),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSemanaActual.FlatAppearance.BorderSize = 0;
            btnSemanaActual.Click += (s, e) =>
            {
                dtpDesde.Value = LunesDe(DateTime.Today);
                dtpHasta.Value = LunesDe(DateTime.Today).AddDays(5);
                CargarDatos();
            };
            filtros.Controls.Add(btnSemanaActual);

            x += 130;
            btnSemanaAnterior = new Button
            {
                Text = "Semana anterior",
                Location = new Point(x, 28),
                Size = new Size(130, 26),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSemanaAnterior.FlatAppearance.BorderSize = 0;
            btnSemanaAnterior.Click += (s, e) =>
            {
                var lunes = LunesDe(DateTime.Today).AddDays(-7);
                dtpDesde.Value = lunes;
                dtpHasta.Value = lunes.AddDays(5);
                CargarDatos();
            };
            filtros.Controls.Add(btnSemanaAnterior);

            x += 140;
            btnActualizar = new Button
            {
                Text = "Actualizar",
                Location = new Point(x, 28),
                Size = new Size(110, 26),
                BackColor = colorAcento,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += (s, e) => CargarDatos();
            filtros.Controls.Add(btnActualizar);

            x += 120;
            filtros.Controls.Add(new Label
            {
                Text = "Buscar",
                Location = new Point(x, 8),
                Size = new Size(60, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            });
            txtBuscar = new TextBox
            {
                Location = new Point(x, 28),
                Size = new Size(280, 26),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtBuscar.TextChanged += (s, e) => AplicarFiltro();
            filtros.Controls.Add(txtBuscar);

            lblResumen = new Label
            {
                Text = "Sin datos cargados",
                Location = new Point(22, 64),
                Size = new Size(1050, 22),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = colorNeutro,
                AutoEllipsis = true
            };
            filtros.Controls.Add(lblResumen);

            var central = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 14, 20, 14), BackColor = Color.White };
            var marco = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = colorBorde,
                Padding = new Padding(1)
            };
            olv = new ObjectListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.25F),
                UseAlternatingBackColors = true,
                AlternateRowBackColor = Color.FromArgb(248, 250, 253),
                RowHeight = 26
            };
            marco.Controls.Add(olv);
            central.Controls.Add(marco);

            var botones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = colorSuave,
                Padding = new Padding(20, 12, 20, 12)
            };
            botones.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = colorBorde });

            btnExportarPdf = new Button
            {
                Text = "Exportar PDF",
                Location = new Point(22, 14),
                Size = new Size(160, 32),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExportarPdf.FlatAppearance.BorderSize = 0;
            btnExportarPdf.Click += BtnExportarPdf_Click;

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.ClientSize.Width - 130, 14),
                Size = new Size(108, 32),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => this.Close();

            botones.Controls.Add(btnExportarPdf);
            botones.Controls.Add(btnCerrar);

            this.Controls.Add(central);
            this.Controls.Add(botones);
            this.Controls.Add(filtros);
            this.Controls.Add(banner);
        }

        private void ConfigurarLista()
        {
            var colNo = new OLVColumn("#", "Indice")
            {
                Width = 50,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectGetter = o => (o as NominaItem)?.Indice ?? 0
            };
            var colClave = new OLVColumn("Clave", "Clave")
            {
                Width = 90,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectGetter = o => (o as NominaItem)?.Clave ?? ""
            };
            var colNombre = new OLVColumn("Trabajador", "Nombre")
            {
                Width = 280,
                IsEditable = false,
                FillsFreeSpace = true,
                AspectGetter = o => (o as NominaItem)?.Nombre ?? ""
            };
            var colRol = new OLVColumn("Rol", "Rol")
            {
                Width = 140,
                IsEditable = false,
                AspectGetter = o => (o as NominaItem)?.Rol ?? ""
            };
            var colCuadrillas = new OLVColumn("Cuadrillas", "Cuadrillas")
            {
                Width = 160,
                IsEditable = false,
                AspectGetter = o => (o as NominaItem)?.Cuadrillas ?? ""
            };
            var colRecibos = new OLVColumn("# Recibos", "NumRecibos")
            {
                Width = 80,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectGetter = o => (o as NominaItem)?.NumRecibos ?? 0
            };
            var colMonto = new OLVColumn("Monto a rayar", "Monto")
            {
                Width = 140,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectGetter = o => (o as NominaItem)?.Monto ?? 0m,
                AspectToStringConverter = v => ((decimal)v).ToString("C2", CultureInfo.CurrentCulture)
            };

            olv.AllColumns.AddRange(new[]
            {
                colNo, colClave, colNombre, colRol, colCuadrillas, colRecibos, colMonto
            });
            olv.RebuildColumns();

            olv.FormatRow += (s, e) =>
            {
                var it = e.Model as NominaItem;
                if (it == null) return;
                if (it.Monto == 0) e.Item.ForeColor = Color.FromArgb(127, 140, 141);
            };
        }

        #endregion

        #region Carga de datos

        private void CargarDatos()
        {
            DateTime desde = dtpDesde.Value.Date;
            DateTime hasta = dtpHasta.Value.Date.AddDays(1).AddSeconds(-1);
            if (desde > hasta)
            {
                MessageBox.Show("La fecha 'Desde' no puede ser posterior a 'Hasta'.",
                    "Filtro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Migrado a API: GET /api/nomina/reporte?desde=&hasta=
                _items = ApiClient.Get<List<NominaItem>>(
                    "/api/nomina/reporte"
                    + "?desde=" + desde.ToString("yyyy-MM-dd")
                    + "&hasta=" + dtpHasta.Value.Date.ToString("yyyy-MM-dd"));
                int i = 1;
                foreach (var it in _items) it.Indice = i++;
                olv.SetObjects(_items);
                ActualizarResumen(desde, dtpHasta.Value.Date);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar nómina:\n" + ex.Message,
                    "Reporte de Nómina", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarResumen(DateTime desde, DateTime hasta)
        {
            int trabajadores = _items.Count;
            int recibos = _items.Sum(x => x.NumRecibos);
            decimal total = _items.Sum(x => x.Monto);
            lblResumen.Text = string.Format(
                "Periodo: {0} a {1}   ·   Trabajadores: {2}   ·   Recibos: {3}   ·   Total a rayar: {4}",
                desde.ToString("dd/MM/yyyy"), hasta.ToString("dd/MM/yyyy"),
                trabajadores, recibos, total.ToString("C2", CultureInfo.CurrentCulture));
        }

        private void AplicarFiltro()
        {
            string t = (txtBuscar.Text ?? "").Trim();
            if (string.IsNullOrEmpty(t))
            {
                olv.ModelFilter = null;
                olv.UseFiltering = false;
                return;
            }
            olv.UseFiltering = true;
            olv.ModelFilter = new ModelFilter(o =>
            {
                var n = o as NominaItem;
                if (n == null) return false;
                return (n.Nombre ?? "").IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0
                    || (n.Clave ?? "").IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0
                    || (n.Cuadrillas ?? "").IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0
                    || (n.Rol ?? "").IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0;
            });
        }

        private static DateTime LunesDe(DateTime referencia)
        {
            int diff = (7 + (referencia.DayOfWeek - DayOfWeek.Monday)) % 7;
            return referencia.AddDays(-diff).Date;
        }

        #endregion

        #region Exportación PDF

        private void BtnExportarPdf_Click(object sender, EventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Exportar PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string archivo = Path.Combine(Path.GetTempPath(),
                $"ReporteNomina_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            try
            {
                using (var doc = new PdfDocument())
                {
                    doc.Info.Title = "Listado de Nómina";
                    var page = doc.AddPage();
                    DibujarPagina(doc, page);
                    doc.Save(archivo);
                }
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar PDF:\n" + ex.Message,
                    "Exportar PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DibujarPagina(PdfDocument doc, PdfPage page)
        {
            var gfx = XGraphics.FromPdfPage(page);
            var fontTitulo = new XFont("Segoe UI", 16, XFontStyle.Bold);
            var fontSub = new XFont("Segoe UI", 9, XFontStyle.Italic);
            var fontHeader = new XFont("Segoe UI", 9, XFontStyle.Bold);
            var fontTexto = new XFont("Segoe UI", 9, XFontStyle.Regular);
            var fontTotales = new XFont("Segoe UI", 11, XFontStyle.Bold);

            double margenIzq = 30;
            double margenDer = 30;
            double y = 30;
            double ancho = page.Width - margenIzq - margenDer;

            gfx.DrawString("LISTADO DE NÓMINA - RAYA SEMANAL",
                fontTitulo, XBrushes.Black,
                new XRect(margenIzq, y, ancho, 22), XStringFormats.TopLeft);
            y += 24;
            gfx.DrawString(lblResumen.Text, fontSub, XBrushes.DarkSlateGray,
                new XRect(margenIzq, y, ancho, 16), XStringFormats.TopLeft);
            y += 24;

            string[] headers = { "#", "Clave", "Trabajador", "Rol", "Cuadrillas", "Recibos", "Monto a rayar", "Firma" };
            double[] anchos = { 30, 60, 180, 90, 110, 50, 80, 130 };

            double x = margenIzq;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(41, 60, 88)),
                    new XRect(x, y, anchos[i], 22));
                gfx.DrawString(headers[i], fontHeader, XBrushes.White,
                    new XRect(x + 3, y + 4, anchos[i] - 4, 18),
                    i == 6 ? XStringFormats.TopRight : XStringFormats.TopLeft);
                x += anchos[i];
            }
            y += 22;

            decimal total = 0m;
            int n = 1;
            foreach (var it in _items)
            {
                if (y > page.Height - 70)
                {
                    page = doc.AddPage();
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 30;
                }
                x = margenIzq;
                string[] celdas =
                {
                    n.ToString(),
                    it.Clave ?? "",
                    it.Nombre ?? "",
                    it.Rol ?? "",
                    it.Cuadrillas ?? "",
                    it.NumRecibos.ToString(),
                    it.Monto.ToString("C2", CultureInfo.CurrentCulture),
                    ""
                };
                for (int i = 0; i < celdas.Length; i++)
                {
                    gfx.DrawRectangle(XPens.LightGray, new XRect(x, y, anchos[i], 22));
                    gfx.DrawString(Recortar(celdas[i], anchos[i] - 4, fontTexto, gfx),
                        fontTexto, XBrushes.Black,
                        new XRect(x + 3, y + 5, anchos[i] - 4, 18),
                        i == 6 ? XStringFormats.TopRight : XStringFormats.TopLeft);
                    x += anchos[i];
                }
                y += 22;
                total += it.Monto;
                n++;
            }

            y += 6;
            gfx.DrawString("TOTAL A RAYAR: " + total.ToString("C2", CultureInfo.CurrentCulture),
                fontTotales, XBrushes.Black,
                new XRect(margenIzq, y, ancho, 22),
                XStringFormats.TopRight);
            gfx.Dispose();
        }

        private static string Recortar(string texto, double ancho, XFont font, XGraphics gfx)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            var size = gfx.MeasureString(texto, font);
            if (size.Width <= ancho) return texto;
            string actual = texto;
            while (actual.Length > 0 && gfx.MeasureString(actual + "…", font).Width > ancho)
                actual = actual.Substring(0, actual.Length - 1);
            return actual + "…";
        }

        #endregion

        #region Modelo

        private class NominaItem
        {
            public int Indice { get; set; }
            public int? IdTrabajador { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Rol { get; set; }
            public string Cuadrillas { get; set; }
            public int NumRecibos { get; set; }
            public decimal Monto { get; set; }
        }

        #endregion
    }
}
