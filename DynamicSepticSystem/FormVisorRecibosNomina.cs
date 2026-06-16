using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
    /// Visor de recibos de nómina ya emitidos: permite filtrar por periodo, trabajador
    /// o cuadrilla, abrir el PDF original o reimprimir un recibo si el PDF no se guardó.
    /// </summary>
    public class FormVisorRecibosNomina : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private TextBox txtBuscar;
        private Button btnActualizar;
        private Button btnAbrirPdf;
        private Button btnReimprimir;
        private Button btnGuardarComo;
        private Button btnCerrar;
        private Label lblResumen;
        private ObjectListView olv;

        private List<ReciboInfo> _recibos = new List<ReciboInfo>();

        public FormVisorRecibosNomina()
        {
            BuildUI();
            ConfigurarLista();
            ThemeManager.AplicarTema(this);
            this.Load += (s, e) => CargarDatos();
        }

        #region UI

        private void BuildUI()
        {
            this.Text = "Recibos de Nómina";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(1000, 620);
            this.ClientSize = new Size(1180, 720);
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
                Text = "RECIBOS DE NÓMINA",
                Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 10)
            });
            banner.Controls.Add(new Label
            {
                Text = "Consulta, abre y reimprime los recibos emitidos",
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
                Value = DateTime.Today.AddDays(-30)
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
                Value = DateTime.Today
            };
            filtros.Controls.Add(dtpHasta);

            x += 152;
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

            x += 124;
            filtros.Controls.Add(new Label
            {
                Text = "Buscar (trabajador, cuadrilla, concepto)",
                Location = new Point(x, 8),
                Size = new Size(300, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            });
            txtBuscar = new TextBox
            {
                Location = new Point(x, 28),
                Size = new Size(360, 26),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtBuscar.TextChanged += (s, e) => AplicarFiltro();
            filtros.Controls.Add(txtBuscar);

            lblResumen = new Label
            {
                Text = "Sin datos cargados",
                Location = new Point(22, 64),
                Size = new Size(1100, 22),
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
            olv.DoubleClick += (s, e) => AbrirOReimprimirSeleccionado();
            olv.SelectionChanged += (s, e) => ActualizarBotones();
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

            btnAbrirPdf = new Button
            {
                Text = "Abrir PDF",
                Location = new Point(22, 14),
                Size = new Size(120, 32),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAbrirPdf.FlatAppearance.BorderSize = 0;
            btnAbrirPdf.Click += (s, e) => AbrirSeleccionado();

            btnReimprimir = new Button
            {
                Text = "Reimprimir recibo",
                Location = new Point(150, 14),
                Size = new Size(160, 32),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReimprimir.FlatAppearance.BorderSize = 0;
            btnReimprimir.Click += (s, e) => ReimprimirSeleccionado();

            btnGuardarComo = new Button
            {
                Text = "Guardar como...",
                Location = new Point(318, 14),
                Size = new Size(140, 32),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardarComo.FlatAppearance.BorderSize = 0;
            btnGuardarComo.Click += (s, e) => GuardarComoSeleccionado();

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

            botones.Controls.Add(btnAbrirPdf);
            botones.Controls.Add(btnReimprimir);
            botones.Controls.Add(btnGuardarComo);
            botones.Controls.Add(btnCerrar);

            this.Controls.Add(central);
            this.Controls.Add(botones);
            this.Controls.Add(filtros);
            this.Controls.Add(banner);
        }

        private void ConfigurarLista()
        {
            var colId = new OLVColumn("ID", "Id")
            {
                Width = 60,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectGetter = o => (o as ReciboInfo)?.Id ?? 0
            };
            var colFecha = new OLVColumn("Fecha recibo", "FechaRecibo")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectGetter = o => (o as ReciboInfo)?.FechaRecibo.ToString("dd/MM/yyyy") ?? ""
            };
            var colTrab = new OLVColumn("Trabajador", "NombreTrabajador")
            {
                Width = 240,
                IsEditable = false,
                AspectGetter = o => (o as ReciboInfo)?.NombreTrabajador ?? ""
            };
            var colRol = new OLVColumn("Rol", "Rol")
            {
                Width = 130,
                IsEditable = false,
                AspectGetter = o => (o as ReciboInfo)?.Rol ?? ""
            };
            var colCuadrilla = new OLVColumn("Cuadrilla", "CodigoCuadrilla")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectGetter = o => (o as ReciboInfo)?.CodigoCuadrilla ?? ""
            };
            var colConcepto = new OLVColumn("Concepto", "Concepto")
            {
                Width = 260,
                IsEditable = false,
                FillsFreeSpace = true,
                AspectGetter = o => (o as ReciboInfo)?.Concepto ?? ""
            };
            var colMonto = new OLVColumn("Monto", "Monto")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectGetter = o => (o as ReciboInfo)?.Monto ?? 0m,
                AspectToStringConverter = v => ((decimal)v).ToString("C2", CultureInfo.CurrentCulture)
            };
            var colPdf = new OLVColumn("PDF", "TienePdf")
            {
                Width = 60,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectGetter = o => (o as ReciboInfo)?.TienePdf == true ? "Sí" : "—"
            };

            olv.AllColumns.AddRange(new[] { colId, colFecha, colTrab, colRol, colCuadrilla, colConcepto, colMonto, colPdf });
            olv.RebuildColumns();
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
                if (!ExisteTablaRecibos())
                {
                    _recibos.Clear();
                    olv.SetObjects(_recibos);
                    lblResumen.Text = "No existe la tabla RecibosNomina aún.";
                    ActualizarBotones();
                    return;
                }

                _recibos = ConsultarRecibos(desde, hasta);
                olv.SetObjects(_recibos);
                lblResumen.Text = string.Format(
                    "Periodo: {0} a {1}   ·   Recibos: {2}   ·   Total: {3}",
                    desde.ToString("dd/MM/yyyy"), dtpHasta.Value.Date.ToString("dd/MM/yyyy"),
                    _recibos.Count, _recibos.Sum(r => r.Monto).ToString("C2", CultureInfo.CurrentCulture));
                ActualizarBotones();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar recibos:\n" + ex.Message,
                    "Recibos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ExisteTablaRecibos()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM sys.tables WHERE name = 'RecibosNomina'", conn))
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private List<ReciboInfo> ConsultarRecibos(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT Id, IdTrabajador, NombreTrabajador, Rol, CodigoCuadrilla, Concepto, Monto,
                       FechaRecibo, PeriodoDesde, PeriodoHasta, TotalCuadrilla,
                       CASE WHEN Pdf IS NULL THEN 0 ELSE 1 END AS TienePdf
                FROM RecibosNomina
                WHERE FechaRecibo BETWEEN @d AND @h
                   OR (PeriodoDesde IS NOT NULL AND PeriodoHasta IS NOT NULL
                       AND PeriodoDesde <= @h AND PeriodoHasta >= @d)
                ORDER BY FechaRecibo DESC, NombreTrabajador";

            var lista = new List<ReciboInfo>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@d", desde);
                    cmd.Parameters.AddWithValue("@h", hasta);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ReciboInfo
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IdTrabajador = reader["IdTrabajador"] == DBNull.Value
                                    ? (int?)null : Convert.ToInt32(reader["IdTrabajador"]),
                                NombreTrabajador = reader["NombreTrabajador"].ToString(),
                                Rol = reader["Rol"] == DBNull.Value ? "" : reader["Rol"].ToString(),
                                CodigoCuadrilla = reader["CodigoCuadrilla"] == DBNull.Value
                                    ? "" : reader["CodigoCuadrilla"].ToString(),
                                Concepto = reader["Concepto"] == DBNull.Value
                                    ? "" : reader["Concepto"].ToString(),
                                Monto = Convert.ToDecimal(reader["Monto"]),
                                FechaRecibo = Convert.ToDateTime(reader["FechaRecibo"]),
                                PeriodoDesde = reader["PeriodoDesde"] == DBNull.Value
                                    ? (DateTime?)null : Convert.ToDateTime(reader["PeriodoDesde"]),
                                PeriodoHasta = reader["PeriodoHasta"] == DBNull.Value
                                    ? (DateTime?)null : Convert.ToDateTime(reader["PeriodoHasta"]),
                                TotalCuadrilla = reader["TotalCuadrilla"] == DBNull.Value
                                    ? 0m : Convert.ToDecimal(reader["TotalCuadrilla"]),
                                TienePdf = Convert.ToInt32(reader["TienePdf"]) == 1
                            });
                        }
                    }
                }
            }
            return lista;
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
                var r = o as ReciboInfo;
                if (r == null) return false;
                return (r.NombreTrabajador ?? "").IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0
                    || (r.CodigoCuadrilla ?? "").IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0
                    || (r.Concepto ?? "").IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0
                    || (r.Rol ?? "").IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0;
            });
        }

        private void ActualizarBotones()
        {
            var sel = olv.SelectedObject as ReciboInfo;
            btnAbrirPdf.Enabled = sel?.TienePdf == true;
            btnReimprimir.Enabled = sel != null;
            btnGuardarComo.Enabled = sel != null;
        }

        #endregion

        #region Acciones sobre recibo

        private void AbrirOReimprimirSeleccionado()
        {
            var sel = olv.SelectedObject as ReciboInfo;
            if (sel == null) return;
            if (sel.TienePdf) AbrirSeleccionado();
            else ReimprimirSeleccionado();
        }

        private byte[] LeerPdf(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT Pdf FROM RecibosNomina WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    var obj = cmd.ExecuteScalar();
                    if (obj == null || obj == DBNull.Value) return null;
                    return (byte[])obj;
                }
            }
        }

        private void AbrirSeleccionado()
        {
            var sel = olv.SelectedObject as ReciboInfo;
            if (sel == null || !sel.TienePdf) return;
            try
            {
                byte[] bytes = LeerPdf(sel.Id);
                if (bytes == null)
                {
                    MessageBox.Show("El PDF está vacío en la base de datos.",
                        "Abrir PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string archivo = Path.Combine(Path.GetTempPath(),
                    string.Format("Recibo_{0}_{1}.pdf", sel.Id, SanitizarNombre(sel.NombreTrabajador)));
                File.WriteAllBytes(archivo, bytes);
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el PDF:\n" + ex.Message,
                    "Abrir PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarComoSeleccionado()
        {
            var sel = olv.SelectedObject as ReciboInfo;
            if (sel == null) return;

            byte[] bytes;
            if (sel.TienePdf)
            {
                bytes = LeerPdf(sel.Id);
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    GenerarReciboPdf(sel, ms);
                    bytes = ms.ToArray();
                }
            }

            if (bytes == null || bytes.Length == 0)
            {
                MessageBox.Show("No se pudo obtener el PDF.", "Guardar PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = string.Format("Recibo_{0}_{1:yyyyMMdd}.pdf",
                    SanitizarNombre(sel.NombreTrabajador), sel.FechaRecibo);
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllBytes(sfd.FileName, bytes);
                    MessageBox.Show("Guardado en:\n" + sfd.FileName,
                        "Guardar PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo guardar:\n" + ex.Message,
                        "Guardar PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ReimprimirSeleccionado()
        {
            var sel = olv.SelectedObject as ReciboInfo;
            if (sel == null) return;
            try
            {
                string archivo = Path.Combine(Path.GetTempPath(),
                    string.Format("ReciboReimpreso_{0}_{1:yyyyMMdd_HHmmss}.pdf",
                        SanitizarNombre(sel.NombreTrabajador), DateTime.Now));
                using (var fs = new FileStream(archivo, FileMode.Create, FileAccess.Write))
                    GenerarReciboPdf(sel, fs);
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo reimprimir:\n" + ex.Message,
                    "Reimprimir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarReciboPdf(ReciboInfo r, Stream destino)
        {
            using (var doc = new PdfDocument())
            {
                doc.Info.Title = "Recibo de Nómina";
                var page = doc.AddPage();
                page.Size = PdfSharp.PageSize.Letter;

                var gfx = XGraphics.FromPdfPage(page);
                var fontTit = new XFont("Segoe UI", 20, XFontStyle.Bold);
                var fontSub = new XFont("Segoe UI", 11, XFontStyle.Italic);
                var fontLbl = new XFont("Segoe UI", 11, XFontStyle.Bold);
                var fontTxt = new XFont("Segoe UI", 11, XFontStyle.Regular);
                var fontMonto = new XFont("Segoe UI", 22, XFontStyle.Bold);
                var fontLetras = new XFont("Segoe UI", 10, XFontStyle.Italic);

                double mIzq = 50;
                double mDer = 50;
                double ancho = page.Width - mIzq - mDer;
                double y = 50;

                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(41, 60, 88)),
                    new XRect(mIzq, y, ancho, 50));
                gfx.DrawString("RECIBO DE NÓMINA", fontTit, XBrushes.White,
                    new XRect(mIzq + 12, y + 14, ancho - 24, 30), XStringFormats.TopLeft);
                gfx.DrawString("Calandria Residencial", fontSub, XBrushes.LightGray,
                    new XRect(mIzq + 12, y + 32, ancho - 24, 16), XStringFormats.TopLeft);
                y += 64;

                gfx.DrawString("Folio: " + r.Id.ToString("D6"),
                    fontTxt, XBrushes.Black,
                    new XRect(mIzq, y, ancho, 18),
                    XStringFormats.TopRight);
                gfx.DrawString("Fecha de pago: " + r.FechaRecibo.ToString("dd/MM/yyyy"),
                    fontTxt, XBrushes.Black,
                    new XRect(mIzq, y, ancho, 18),
                    XStringFormats.TopLeft);
                y += 26;

                DrawCampo(gfx, mIzq, ref y, ancho, "Trabajador:", r.NombreTrabajador, fontLbl, fontTxt);
                DrawCampo(gfx, mIzq, ref y, ancho, "Rol:", r.Rol ?? "—", fontLbl, fontTxt);
                DrawCampo(gfx, mIzq, ref y, ancho, "Cuadrilla:", r.CodigoCuadrilla ?? "—", fontLbl, fontTxt);
                if (r.PeriodoDesde.HasValue && r.PeriodoHasta.HasValue)
                {
                    DrawCampo(gfx, mIzq, ref y, ancho, "Periodo:",
                        r.PeriodoDesde.Value.ToString("dd/MM/yyyy") + " a " +
                        r.PeriodoHasta.Value.ToString("dd/MM/yyyy"), fontLbl, fontTxt);
                }
                DrawCampo(gfx, mIzq, ref y, ancho, "Concepto:", r.Concepto ?? "", fontLbl, fontTxt);

                y += 14;
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(245, 247, 250)),
                    new XRect(mIzq, y, ancho, 80));
                gfx.DrawString("IMPORTE A RECIBIR", fontLbl, XBrushes.DarkSlateGray,
                    new XRect(mIzq + 14, y + 8, ancho - 28, 18), XStringFormats.TopLeft);
                gfx.DrawString(r.Monto.ToString("C2", CultureInfo.CurrentCulture),
                    fontMonto, XBrushes.Black,
                    new XRect(mIzq + 14, y + 28, ancho - 28, 38), XStringFormats.TopRight);

                string letras = "(" + NumeroALetras.Convertir(r.Monto) + ")";
                gfx.DrawString(letras, fontLetras, XBrushes.DarkSlateGray,
                    new XRect(mIzq + 14, y + 60, ancho - 28, 16), XStringFormats.TopLeft);
                y += 96;

                y += 70;
                double anchoFirma = (ancho - 40) / 2;
                gfx.DrawLine(XPens.Black, mIzq, y, mIzq + anchoFirma, y);
                gfx.DrawLine(XPens.Black, mIzq + anchoFirma + 40, y, mIzq + ancho, y);
                gfx.DrawString("Firma del trabajador", fontTxt, XBrushes.Black,
                    new XRect(mIzq, y + 4, anchoFirma, 18), XStringFormats.TopCenter);
                gfx.DrawString("Entrega / Recibido por", fontTxt, XBrushes.Black,
                    new XRect(mIzq + anchoFirma + 40, y + 4, anchoFirma, 18),
                    XStringFormats.TopCenter);

                gfx.Dispose();
                doc.Save(destino, false);
            }
        }

        private static void DrawCampo(XGraphics gfx, double mIzq, ref double y, double ancho,
            string etiqueta, string valor, XFont fontLbl, XFont fontTxt)
        {
            gfx.DrawString(etiqueta, fontLbl, XBrushes.Black,
                new XRect(mIzq, y, 110, 20), XStringFormats.TopLeft);
            gfx.DrawString(valor ?? "", fontTxt, XBrushes.Black,
                new XRect(mIzq + 110, y, ancho - 110, 20), XStringFormats.TopLeft);
            y += 22;
        }

        private static string SanitizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "recibo";
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new System.Text.StringBuilder();
            foreach (var c in nombre)
                sb.Append(Array.IndexOf(invalid, c) >= 0 || c == ' ' ? '_' : c);
            return sb.ToString();
        }

        #endregion

        #region Modelo

        private class ReciboInfo
        {
            public int Id { get; set; }
            public int? IdTrabajador { get; set; }
            public string NombreTrabajador { get; set; }
            public string Rol { get; set; }
            public string CodigoCuadrilla { get; set; }
            public string Concepto { get; set; }
            public decimal Monto { get; set; }
            public DateTime FechaRecibo { get; set; }
            public DateTime? PeriodoDesde { get; set; }
            public DateTime? PeriodoHasta { get; set; }
            public decimal TotalCuadrilla { get; set; }
            public bool TienePdf { get; set; }
        }

        #endregion
    }
}
