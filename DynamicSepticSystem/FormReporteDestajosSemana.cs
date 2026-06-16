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
    /// Reporte semanal de destajos: muestra cuáles se activaron, cuáles se terminaron y
    /// cuáles siguen pendientes (activados pero no finalizados) dentro de un rango de fechas.
    /// </summary>
    public class FormReporteDestajosSemana : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private ComboBox cmbVista;
        private Button btnSemanaActual;
        private Button btnSemanaAnterior;
        private Button btnActualizar;
        private Button btnExportarPdf;
        private Button btnCerrar;
        private Label lblResumen;
        private TextBox txtBuscar;
        private TreeListView olvReporte;

        private List<RegistroSemana> _registros = new List<RegistroSemana>();

        public FormReporteDestajosSemana()
        {
            BuildUI();
            ConfigurarTree();
            ThemeManager.AplicarTema(this);
            this.Load += (s, e) => { AsegurarColumnaFechaActivacion(); CargarDatos(); };
        }

        #region UI

        private void BuildUI()
        {
            this.Text = "Reporte Semanal de Destajos";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(1000, 640);
            this.ClientSize = new Size(1180, 740);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            this.DoubleBuffered = true;

            Color colorPrimario = Color.FromArgb(41, 60, 88);
            Color colorAcento = Color.FromArgb(52, 152, 219);
            Color colorSuave = Color.FromArgb(248, 250, 253);
            Color colorBorde = Color.FromArgb(218, 224, 232);
            Color colorNeutro = Color.FromArgb(127, 140, 141);

            var panelBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = colorPrimario
            };
            panelBanner.Controls.Add(new Label
            {
                Text = "REPORTE SEMANAL DE DESTAJOS",
                Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 10)
            });
            panelBanner.Controls.Add(new Label
            {
                Text = "Activados, terminados y pendientes por semana",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(189, 200, 215),
                AutoSize = true,
                Location = new Point(22, 38)
            });

            var panelFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = colorSuave,
                Padding = new Padding(20, 12, 20, 10)
            };

            int x = 22;
            panelFiltros.Controls.Add(new Label
            {
                Text = "Desde",
                Location = new Point(x, 12),
                Size = new Size(80, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            });
            dtpDesde = new DateTimePicker
            {
                Location = new Point(x, 32),
                Size = new Size(140, 26),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F),
                Value = LunesDe(DateTime.Today)
            };
            panelFiltros.Controls.Add(dtpDesde);

            x += 152;
            panelFiltros.Controls.Add(new Label
            {
                Text = "Hasta",
                Location = new Point(x, 12),
                Size = new Size(80, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            });
            dtpHasta = new DateTimePicker
            {
                Location = new Point(x, 32),
                Size = new Size(140, 26),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F),
                Value = DateTime.Today
            };
            panelFiltros.Controls.Add(dtpHasta);

            x += 152;
            btnSemanaActual = new Button
            {
                Text = "Semana actual",
                Location = new Point(x, 32),
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
                dtpHasta.Value = DateTime.Today;
                CargarDatos();
            };
            panelFiltros.Controls.Add(btnSemanaActual);

            x += 130;
            btnSemanaAnterior = new Button
            {
                Text = "Semana anterior",
                Location = new Point(x, 32),
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
                dtpHasta.Value = lunes.AddDays(6);
                CargarDatos();
            };
            panelFiltros.Controls.Add(btnSemanaAnterior);

            x += 140;
            panelFiltros.Controls.Add(new Label
            {
                Text = "Mostrar",
                Location = new Point(x, 12),
                Size = new Size(80, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            });
            cmbVista = new ComboBox
            {
                Location = new Point(x, 32),
                Size = new Size(170, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F)
            };
            cmbVista.Items.AddRange(new object[]
            {
                "Todos (activados + terminados + pendientes)",
                "Solo activados en el periodo",
                "Solo terminados en el periodo",
                "Solo pendientes (sin terminar)"
            });
            cmbVista.SelectedIndex = 0;
            cmbVista.SelectedIndexChanged += (s, e) => ReconstruirArbol();
            panelFiltros.Controls.Add(cmbVista);

            x += 180;
            btnActualizar = new Button
            {
                Text = "Actualizar",
                Location = new Point(x, 32),
                Size = new Size(110, 26),
                BackColor = colorAcento,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += (s, e) => CargarDatos();
            panelFiltros.Controls.Add(btnActualizar);

            x += 120;
            panelFiltros.Controls.Add(new Label
            {
                Text = "Buscar",
                Location = new Point(x, 12),
                Size = new Size(60, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            });
            txtBuscar = new TextBox
            {
                Location = new Point(x, 33),
                Size = new Size(240, 26),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtBuscar.TextChanged += (s, e) => AplicarFiltro();
            panelFiltros.Controls.Add(txtBuscar);

            lblResumen = new Label
            {
                Text = "Sin datos cargados",
                Location = new Point(22, 78),
                Size = new Size(1100, 22),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = colorNeutro,
                AutoEllipsis = true
            };
            panelFiltros.Controls.Add(lblResumen);

            var panelCentro = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 14, 20, 14),
                BackColor = Color.White
            };
            var marco = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = colorBorde,
                Padding = new Padding(1)
            };
            olvReporte = new TreeListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.25F)
            };
            marco.Controls.Add(olvReporte);
            panelCentro.Controls.Add(marco);

            var panelBotones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = colorSuave,
                Padding = new Padding(20, 12, 20, 12)
            };
            panelBotones.Controls.Add(new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = colorBorde
            });

            var btnExpandir = new Button
            {
                Text = "Expandir todo",
                Location = new Point(22, 14),
                Size = new Size(130, 32),
                BackColor = Color.FromArgb(99, 110, 114),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExpandir.FlatAppearance.BorderSize = 0;
            btnExpandir.Click += (s, e) => olvReporte.ExpandAll();

            var btnContraer = new Button
            {
                Text = "Contraer todo",
                Location = new Point(158, 14),
                Size = new Size(130, 32),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnContraer.FlatAppearance.BorderSize = 0;
            btnContraer.Click += (s, e) => olvReporte.CollapseAll();

            btnExportarPdf = new Button
            {
                Text = "Exportar PDF",
                Location = new Point(296, 14),
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

            panelBotones.Controls.Add(btnExpandir);
            panelBotones.Controls.Add(btnContraer);
            panelBotones.Controls.Add(btnExportarPdf);
            panelBotones.Controls.Add(btnCerrar);

            this.Controls.Add(panelCentro);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelFiltros);
            this.Controls.Add(panelBanner);
        }

        private void ConfigurarTree()
        {
            olvReporte.CanExpandGetter = obj =>
            {
                var n = obj as NodoReporte;
                return n != null && n.Hijos != null && n.Hijos.Count > 0;
            };
            olvReporte.ChildrenGetter = obj =>
            {
                var n = obj as NodoReporte;
                return n?.Hijos ?? new List<NodoReporte>();
            };

            var colNombre = new OLVColumn("Estado / Casa / Destajo", "Etiqueta")
            {
                Width = 460,
                IsEditable = false,
                Sortable = false,
                AspectGetter = o => (o as NodoReporte)?.Etiqueta ?? ""
            };
            var colRuta = new OLVColumn("Ruta", "Ruta")
            {
                Width = 90,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = o => (o as NodoReporte)?.Ruta ?? ""
            };
            var colCuadrilla = new OLVColumn("Cuadrilla", "Cuadrilla")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = o => (o as NodoReporte)?.Cuadrilla ?? ""
            };
            var colFechaAct = new OLVColumn("Activado", "FechaActivacion")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = o => (o as NodoReporte)?.FechaActivacion?.ToString("dd/MM/yyyy") ?? ""
            };
            var colFechaFin = new OLVColumn("Terminado", "FechaFinalizacion")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = o => (o as NodoReporte)?.FechaFinalizacion?.ToString("dd/MM/yyyy") ?? ""
            };
            var colEstado = new OLVColumn("Estado", "Estado")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = o => (o as NodoReporte)?.EstadoTexto ?? ""
            };
            var colConteo = new OLVColumn("# Destajos", "Conteo")
            {
                Width = 90,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = o =>
                {
                    var n = o as NodoReporte;
                    if (n == null || n.Nivel == 2) return "";
                    return ContarHojas(n).ToString();
                }
            };

            olvReporte.AllColumns.AddRange(new[]
            {
                colNombre, colRuta, colCuadrilla, colFechaAct, colFechaFin, colEstado, colConteo
            });
            olvReporte.RebuildColumns();

            olvReporte.FormatRow += (s, e) =>
            {
                var n = e.Model as NodoReporte;
                if (n == null) return;
                switch (n.Nivel)
                {
                    case 0:
                        e.Item.BackColor = Color.FromArgb(41, 60, 88);
                        e.Item.ForeColor = Color.White;
                        e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                        break;
                    case 1:
                        e.Item.BackColor = Color.FromArgb(220, 230, 245);
                        e.Item.ForeColor = Color.FromArgb(33, 47, 67);
                        e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                        break;
                    case 2:
                        e.Item.BackColor = Color.White;
                        if (n.Estado == EstadoDestajo.Terminado)
                            e.Item.ForeColor = Color.FromArgb(34, 139, 34);
                        else if (n.Estado == EstadoDestajo.Pendiente)
                            e.Item.ForeColor = Color.FromArgb(192, 57, 43);
                        else
                            e.Item.ForeColor = Color.FromArgb(41, 128, 185);
                        break;
                }
            };
        }

        #endregion

        #region Carga de datos

        private void AsegurarColumnaFechaActivacion()
        {
            const string sql = @"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivacionTareasRuta')
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns
                                   WHERE Name = N'FechaActivacion'
                                     AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
                    BEGIN
                        ALTER TABLE ActivacionTareasRuta ADD FechaActivacion DATETIME NULL;
                    END
                END";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                        cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        private void CargarDatos()
        {
            DateTime desde = dtpDesde.Value.Date;
            DateTime hasta = dtpHasta.Value.Date.AddDays(1).AddSeconds(-1);
            if (desde > hasta)
            {
                MessageBox.Show("La fecha 'Desde' no puede ser posterior a 'Hasta'.",
                    "Filtro de fechas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _registros = ConsultarRegistros(desde, hasta);
                ReconstruirArbol();
                ActualizarResumen(desde, hasta);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar el reporte:\n" + ex.Message,
                    "Reporte", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<RegistroSemana> ConsultarRegistros(DateTime desde, DateTime hasta)
        {
            var lista = new List<RegistroSemana>();
            foreach (var ruta in new[] { "RutaCalandraDestajo", "RutaTuneraDestajo" })
            {
                if (!ExisteTabla(ruta)) continue;
                lista.AddRange(ConsultarRuta(ruta, desde, hasta));
            }
            return lista;
        }

        private bool ExisteTabla(string nombre)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = @t", conn))
                    {
                        cmd.Parameters.AddWithValue("@t", nombre);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch { return false; }
        }

        private List<RegistroSemana> ConsultarRuta(string ruta, DateTime desde, DateTime hasta)
        {
            var lista = new List<RegistroSemana>();

            // Trae todo lo que se haya activado o terminado en el periodo,
            // o todo lo que esté activo y sin terminar (pendientes vivos).
            string sql = $@"
                SELECT
                    a.Manzana,
                    a.Lote,
                    a.NodoID,
                    d.Nombre        AS DestajoNombre,
                    cat.Nombre      AS CategoriaNombre,
                    a.CuadrillaAsignada,
                    a.DesatajoActivado,
                    a.Finalizado,
                    a.FechaActivacion,
                    a.FechaActualizacion,
                    a.FechaFinalizacion
                FROM ActivacionTareasRuta a
                INNER JOIN {ruta} d   ON d.ID = a.NodoID
                LEFT  JOIN {ruta} cat ON cat.ID = d.ParentId
                WHERE a.Ruta = @ruta
                  AND a.DesatajoActivado = 1
                  AND (
                        ISNULL(a.FechaActivacion, a.FechaActualizacion) BETWEEN @desde AND @hasta
                     OR a.FechaFinalizacion BETWEEN @desde AND @hasta
                     OR a.Finalizado = 0
                  )";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@desde", desde);
                    cmd.Parameters.AddWithValue("@hasta", hasta);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime? fAct = reader["FechaActivacion"] == DBNull.Value
                                ? (reader["FechaActualizacion"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(reader["FechaActualizacion"]))
                                : Convert.ToDateTime(reader["FechaActivacion"]);
                            DateTime? fFin = reader["FechaFinalizacion"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["FechaFinalizacion"]);
                            bool finalizado = reader["Finalizado"] != DBNull.Value
                                && Convert.ToBoolean(reader["Finalizado"]);

                            lista.Add(new RegistroSemana
                            {
                                Manzana = reader["Manzana"].ToString(),
                                Lote = reader["Lote"].ToString(),
                                Ruta = ruta,
                                DestajoNombre = reader["DestajoNombre"].ToString(),
                                CategoriaNombre = reader["CategoriaNombre"] == DBNull.Value
                                    ? "(Sin categoría)"
                                    : reader["CategoriaNombre"].ToString(),
                                Cuadrilla = reader["CuadrillaAsignada"] == DBNull.Value
                                    ? "" : reader["CuadrillaAsignada"].ToString(),
                                FechaActivacion = fAct,
                                FechaFinalizacion = fFin,
                                Finalizado = finalizado
                            });
                        }
                    }
                }
            }
            return lista;
        }

        private void ReconstruirArbol()
        {
            DateTime desde = dtpDesde.Value.Date;
            DateTime hasta = dtpHasta.Value.Date.AddDays(1).AddSeconds(-1);
            int filtro = cmbVista.SelectedIndex;

            // Asignar Estado a cada registro
            foreach (var r in _registros)
            {
                bool terminadoEnPeriodo = r.Finalizado
                    && r.FechaFinalizacion.HasValue
                    && r.FechaFinalizacion.Value >= desde
                    && r.FechaFinalizacion.Value <= hasta;
                bool activadoEnPeriodo = r.FechaActivacion.HasValue
                    && r.FechaActivacion.Value >= desde
                    && r.FechaActivacion.Value <= hasta;

                if (terminadoEnPeriodo) r.Estado = EstadoDestajo.Terminado;
                else if (!r.Finalizado) r.Estado = activadoEnPeriodo
                    ? EstadoDestajo.ActivadoEnPeriodo
                    : EstadoDestajo.Pendiente;
                else r.Estado = EstadoDestajo.FueraDePeriodo;
            }

            IEnumerable<RegistroSemana> filtrados = _registros
                .Where(r => r.Estado != EstadoDestajo.FueraDePeriodo);

            switch (filtro)
            {
                case 1: filtrados = filtrados.Where(r => r.Estado == EstadoDestajo.ActivadoEnPeriodo); break;
                case 2: filtrados = filtrados.Where(r => r.Estado == EstadoDestajo.Terminado); break;
                case 3: filtrados = filtrados.Where(r => r.Estado == EstadoDestajo.Pendiente); break;
            }

            var grupos = filtrados
                .GroupBy(r => r.Estado)
                .OrderBy(g => OrdenEstado(g.Key));

            var raiz = new List<NodoReporte>();
            foreach (var g in grupos)
            {
                var nodoEstado = new NodoReporte
                {
                    Nivel = 0,
                    Estado = g.Key,
                    Etiqueta = TextoEstado(g.Key) + $"  ({g.Count()})",
                    Hijos = new List<NodoReporte>()
                };

                var porCasa = g.GroupBy(r => new { r.Manzana, r.Lote, r.Ruta })
                    .OrderBy(k => k.Key.Manzana).ThenBy(k => k.Key.Lote);

                foreach (var grupoCasa in porCasa)
                {
                    var nodoCasa = new NodoReporte
                    {
                        Nivel = 1,
                        Estado = g.Key,
                        Etiqueta = $"M{grupoCasa.Key.Manzana} - L{grupoCasa.Key.Lote}",
                        Ruta = grupoCasa.Key.Ruta == "RutaCalandraDestajo" ? "Calandra" : "Tunera",
                        Hijos = new List<NodoReporte>()
                    };
                    foreach (var r in grupoCasa.OrderBy(x => x.CategoriaNombre).ThenBy(x => x.DestajoNombre))
                    {
                        nodoCasa.Hijos.Add(new NodoReporte
                        {
                            Nivel = 2,
                            Estado = r.Estado,
                            Etiqueta = $"{r.CategoriaNombre} · {r.DestajoNombre}",
                            Ruta = r.Ruta == "RutaCalandraDestajo" ? "Calandra" : "Tunera",
                            Cuadrilla = r.Cuadrilla,
                            FechaActivacion = r.FechaActivacion,
                            FechaFinalizacion = r.FechaFinalizacion,
                            Hijos = new List<NodoReporte>()
                        });
                    }
                    nodoEstado.Hijos.Add(nodoCasa);
                }

                raiz.Add(nodoEstado);
            }

            olvReporte.SetObjects(raiz);
            olvReporte.ExpandAll();
            AplicarFiltro();
        }

        private void ActualizarResumen(DateTime desde, DateTime hasta)
        {
            int activados = _registros.Count(r => r.Estado == EstadoDestajo.ActivadoEnPeriodo);
            int terminados = _registros.Count(r => r.Estado == EstadoDestajo.Terminado);
            int pendientes = _registros.Count(r => r.Estado == EstadoDestajo.Pendiente);

            lblResumen.Text = string.Format(
                "Periodo: {0} a {1}   ·   Activados: {2}   ·   Terminados: {3}   ·   Pendientes (sin terminar): {4}",
                desde.ToString("dd/MM/yyyy"), dtpHasta.Value.Date.ToString("dd/MM/yyyy"),
                activados, terminados, pendientes);
        }

        private void AplicarFiltro()
        {
            string termino = (txtBuscar.Text ?? "").Trim();
            if (string.IsNullOrEmpty(termino))
            {
                olvReporte.ModelFilter = null;
                olvReporte.UseFiltering = false;
                return;
            }
            olvReporte.UseFiltering = true;
            olvReporte.ModelFilter = new ModelFilter(o => CoincideRecursivo(o as NodoReporte, termino));
        }

        private static bool CoincideRecursivo(NodoReporte n, string termino)
        {
            if (n == null) return false;
            if ((n.Etiqueta ?? "").IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if ((n.Cuadrilla ?? "").IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (n.Hijos != null)
                foreach (var h in n.Hijos)
                    if (CoincideRecursivo(h, termino)) return true;
            return false;
        }

        private static int ContarHojas(NodoReporte n)
        {
            if (n == null || n.Hijos == null) return 0;
            if (n.Nivel == 1) return n.Hijos.Count;
            int total = 0;
            foreach (var h in n.Hijos) total += ContarHojas(h);
            return total;
        }

        private static DateTime LunesDe(DateTime referencia)
        {
            int diff = (7 + (referencia.DayOfWeek - DayOfWeek.Monday)) % 7;
            return referencia.AddDays(-diff).Date;
        }

        private static int OrdenEstado(EstadoDestajo e)
        {
            switch (e)
            {
                case EstadoDestajo.Terminado: return 0;
                case EstadoDestajo.ActivadoEnPeriodo: return 1;
                case EstadoDestajo.Pendiente: return 2;
                default: return 9;
            }
        }

        private static string TextoEstado(EstadoDestajo e)
        {
            switch (e)
            {
                case EstadoDestajo.Terminado: return "TERMINADOS EN EL PERIODO";
                case EstadoDestajo.ActivadoEnPeriodo: return "ACTIVADOS EN EL PERIODO";
                case EstadoDestajo.Pendiente: return "PENDIENTES (activados sin terminar)";
                default: return "OTROS";
            }
        }

        #endregion

        #region Exportación PDF

        private void BtnExportarPdf_Click(object sender, EventArgs e)
        {
            if (_registros.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Exportar PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string archivo = Path.Combine(Path.GetTempPath(),
                $"ReporteDestajosSemana_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            try
            {
                using (var doc = new PdfDocument())
                {
                    doc.Info.Title = "Reporte Semanal de Destajos";
                    var page = doc.AddPage();
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    DibujarPaginaPdf(doc, page);
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

        private void DibujarPaginaPdf(PdfDocument doc, PdfPage page)
        {
            var gfx = XGraphics.FromPdfPage(page);
            var fontTitulo = new XFont("Segoe UI", 16, XFontStyle.Bold);
            var fontSub = new XFont("Segoe UI", 9, XFontStyle.Italic);
            var fontSeccion = new XFont("Segoe UI", 11, XFontStyle.Bold);
            var fontHeader = new XFont("Segoe UI", 8.5, XFontStyle.Bold);
            var fontTexto = new XFont("Segoe UI", 8.5, XFontStyle.Regular);

            double margenIzq = 30;
            double margenDer = 30;
            double y = 30;
            double ancho = page.Width - margenIzq - margenDer;

            gfx.DrawString("REPORTE SEMANAL DE DESTAJOS", fontTitulo, XBrushes.Black,
                new XRect(margenIzq, y, ancho, 22), XStringFormats.TopLeft);
            y += 24;
            gfx.DrawString(lblResumen.Text, fontSub, XBrushes.DarkSlateGray,
                new XRect(margenIzq, y, ancho, 16), XStringFormats.TopLeft);
            y += 22;

            string[] headers = { "Estado", "Casa", "Ruta", "Categoría · Destajo", "Cuadrilla", "Activado", "Terminado" };
            double[] anchos = { 90, 60, 60, 280, 80, 70, 70 };

            foreach (var estado in new[]
            {
                EstadoDestajo.Terminado,
                EstadoDestajo.ActivadoEnPeriodo,
                EstadoDestajo.Pendiente
            })
            {
                var deEsteEstado = _registros.Where(r => r.Estado == estado).ToList();
                if (deEsteEstado.Count == 0) continue;

                if (y > page.Height - 80)
                {
                    page = doc.AddPage();
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 30;
                }

                gfx.DrawRectangle(XBrushes.DarkSlateBlue,
                    new XRect(margenIzq, y, ancho, 18));
                gfx.DrawString($"{TextoEstado(estado)}  ({deEsteEstado.Count})",
                    fontSeccion, XBrushes.White,
                    new XRect(margenIzq + 4, y + 2, ancho, 16),
                    XStringFormats.TopLeft);
                y += 22;

                double x = margenIzq;
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(220, 230, 245)),
                        new XRect(x, y, anchos[i], 16));
                    gfx.DrawString(headers[i], fontHeader, XBrushes.Black,
                        new XRect(x + 3, y + 1, anchos[i] - 4, 14), XStringFormats.TopLeft);
                    x += anchos[i];
                }
                y += 18;

                foreach (var r in deEsteEstado
                    .OrderBy(rr => rr.Manzana).ThenBy(rr => rr.Lote)
                    .ThenBy(rr => rr.CategoriaNombre).ThenBy(rr => rr.DestajoNombre))
                {
                    if (y > page.Height - 30)
                    {
                        page = doc.AddPage();
                        page.Orientation = PdfSharp.PageOrientation.Landscape;
                        gfx.Dispose();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 30;
                    }
                    x = margenIzq;
                    string[] celdas =
                    {
                        TextoEstado(estado).Split(' ')[0],
                        $"M{r.Manzana}-L{r.Lote}",
                        r.Ruta == "RutaCalandraDestajo" ? "Calandra" : "Tunera",
                        $"{r.CategoriaNombre} · {r.DestajoNombre}",
                        r.Cuadrilla ?? "",
                        r.FechaActivacion?.ToString("dd/MM/yyyy") ?? "",
                        r.FechaFinalizacion?.ToString("dd/MM/yyyy") ?? ""
                    };
                    for (int i = 0; i < celdas.Length; i++)
                    {
                        gfx.DrawRectangle(XPens.LightGray,
                            new XRect(x, y, anchos[i], 16));
                        gfx.DrawString(Recortar(celdas[i], anchos[i] - 4, fontTexto, gfx),
                            fontTexto, XBrushes.Black,
                            new XRect(x + 3, y + 1, anchos[i] - 4, 14),
                            XStringFormats.TopLeft);
                        x += anchos[i];
                    }
                    y += 16;
                }
                y += 10;
            }
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

        #region Modelos

        private enum EstadoDestajo
        {
            FueraDePeriodo,
            ActivadoEnPeriodo,
            Terminado,
            Pendiente
        }

        private class RegistroSemana
        {
            public string Manzana { get; set; }
            public string Lote { get; set; }
            public string Ruta { get; set; }
            public string DestajoNombre { get; set; }
            public string CategoriaNombre { get; set; }
            public string Cuadrilla { get; set; }
            public DateTime? FechaActivacion { get; set; }
            public DateTime? FechaFinalizacion { get; set; }
            public bool Finalizado { get; set; }
            public EstadoDestajo Estado { get; set; }
        }

        private class NodoReporte
        {
            public int Nivel { get; set; }
            public string Etiqueta { get; set; }
            public string Ruta { get; set; }
            public string Cuadrilla { get; set; }
            public DateTime? FechaActivacion { get; set; }
            public DateTime? FechaFinalizacion { get; set; }
            public EstadoDestajo Estado { get; set; }
            public List<NodoReporte> Hijos { get; set; } = new List<NodoReporte>();

            public string EstadoTexto
            {
                get
                {
                    if (Nivel != 2) return "";
                    switch (Estado)
                    {
                        case EstadoDestajo.Terminado: return "Terminado";
                        case EstadoDestajo.ActivadoEnPeriodo: return "Activado";
                        case EstadoDestajo.Pendiente: return "Pendiente";
                        default: return "";
                    }
                }
            }
        }

        #endregion
    }
}
