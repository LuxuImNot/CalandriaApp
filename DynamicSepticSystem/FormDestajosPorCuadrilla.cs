using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Muestra los destajos finalizados agrupados por Cuadrilla → Categoría → Destajo.
    /// Sirve para distribuir nómina conociendo la carga real de trabajo por cuadrilla.
    /// </summary>
    public class FormDestajosPorCuadrilla : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private readonly string _manzanaActual;
        private readonly string _loteActual;
        private readonly string _rutaActual;
        private readonly bool _tieneCasaCargada;

        private TreeListView olvDestajos;
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private CheckBox chkSoloCasaActual;
        private ComboBox cmbEstadoPago;
        private Label lblEstadoPago;
        private Button btnActualizar;
        private Button btnExpandirTodo;
        private Button btnContraerTodo;
        private Button btnGenerarDistribucion;
        private Button btnCerrar;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblResumen;
        private TextBox txtBuscar;

        private List<NodoReporte> _nodosCuadrilla = new List<NodoReporte>();
        private List<RegistroDestajo> _registrosCache;

        // Opciones del filtro de estado de pago (orden importa: el índice 0 es el default)
        public const string EstadoPagoSoloPendientes   = "Solo pendientes de distribuir";
        public const string EstadoPagoSoloDistribuidas = "Solo distribuidas";
        public const string EstadoPagoTodas            = "Todas";

        public FormDestajosPorCuadrilla(string manzanaActual, string loteActual, string rutaActual)
        {
            _manzanaActual = manzanaActual ?? "";
            _loteActual = loteActual ?? "";
            _rutaActual = rutaActual ?? "";
            _tieneCasaCargada = !string.IsNullOrEmpty(_manzanaActual)
                                && !string.IsNullOrEmpty(_loteActual);

            BuildUI();
            ConfigurarTreeListView();
            ThemeManager.AplicarTema(this);

            this.Load += (s, e) => CargarDatos();
        }

        #region UI

        private void BuildUI()
        {
            this.Text = "Destajos Finalizados por Cuadrilla";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(960, 620);
            this.ClientSize = new Size(1100, 720);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            this.DoubleBuffered = true;

            Color colorPrimario = Color.FromArgb(41, 60, 88);
            Color colorAcento = Color.FromArgb(52, 152, 219);
            Color colorSuave = Color.FromArgb(248, 250, 253);
            Color colorBorde = Color.FromArgb(218, 224, 232);
            Color colorNeutro = Color.FromArgb(127, 140, 141);

            // ===== Banner =====
            var panelBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = colorPrimario
            };

            lblTitulo = new Label
            {
                Text = "DESTAJOS FINALIZADOS POR CUADRILLA",
                Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 10)
            };

            lblSubtitulo = new Label
            {
                Text = "Vista consolidada para asignación y distribución de nómina",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(189, 200, 215),
                AutoSize = true,
                Location = new Point(22, 38)
            };

            panelBanner.Controls.Add(lblSubtitulo);
            panelBanner.Controls.Add(lblTitulo);

            // ===== Filtros =====
            var panelFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = colorSuave,
                Padding = new Padding(20, 12, 20, 10)
            };

            var lblDesde = new Label
            {
                Text = "Desde",
                Location = new Point(22, 12),
                Size = new Size(80, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            };
            dtpDesde = new DateTimePicker
            {
                Location = new Point(22, 32),
                Size = new Size(150, 26),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F),
                Value = ObtenerLunesDeEstaSemana(DateTime.Today)
            };

            var lblHasta = new Label
            {
                Text = "Hasta",
                Location = new Point(186, 12),
                Size = new Size(80, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            };
            dtpHasta = new DateTimePicker
            {
                Location = new Point(186, 32),
                Size = new Size(150, 26),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F),
                Value = DateTime.Today
            };

            chkSoloCasaActual = new CheckBox
            {
                Text = _tieneCasaCargada
                    ? $"Solo casa actual (M{_manzanaActual} - L{_loteActual})"
                    : "Solo casa actual (no hay casa cargada)",
                Location = new Point(360, 36),
                Size = new Size(320, 22),
                Font = new Font("Segoe UI", 9.5F),
                Checked = _tieneCasaCargada,
                Enabled = _tieneCasaCargada
            };

            btnActualizar = new Button
            {
                Text = "Actualizar",
                Location = new Point(694, 31),
                Size = new Size(120, 28),
                BackColor = colorAcento,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += (s, e) => CargarDatos();

            var lblBuscar = new Label
            {
                Text = "Buscar",
                Location = new Point(828, 12),
                Size = new Size(60, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            };
            txtBuscar = new TextBox
            {
                Location = new Point(828, 33),
                Size = new Size(230, 26),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtBuscar.TextChanged += (s, e) => AplicarFiltroBusqueda();

            // ===== Fila 2: filtro de estado de pago =====
            lblEstadoPago = new Label
            {
                Text = "Estado de pago",
                Location = new Point(22, 68),
                Size = new Size(120, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorNeutro
            };
            cmbEstadoPago = new ComboBox
            {
                Location = new Point(22, 88),
                Size = new Size(220, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat
            };
            cmbEstadoPago.Items.AddRange(new object[]
            {
                EstadoPagoSoloPendientes,   // índice 0 → default
                EstadoPagoSoloDistribuidas,
                EstadoPagoTodas
            });
            cmbEstadoPago.SelectedIndex = 0;
            cmbEstadoPago.SelectedIndexChanged += (s, e) =>
            {
                if (_registrosCache != null) RefrescarVista();
            };

            lblResumen = new Label
            {
                Text = "Sin datos cargados",
                Location = new Point(260, 92),
                Size = new Size(820, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = colorNeutro
            };

            panelFiltros.Controls.Add(lblDesde);
            panelFiltros.Controls.Add(dtpDesde);
            panelFiltros.Controls.Add(lblHasta);
            panelFiltros.Controls.Add(dtpHasta);
            panelFiltros.Controls.Add(chkSoloCasaActual);
            panelFiltros.Controls.Add(btnActualizar);
            panelFiltros.Controls.Add(lblBuscar);
            panelFiltros.Controls.Add(txtBuscar);
            panelFiltros.Controls.Add(lblEstadoPago);
            panelFiltros.Controls.Add(cmbEstadoPago);
            panelFiltros.Controls.Add(lblResumen);

            // ===== TreeListView =====
            var panelCentral = new Panel
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

            olvDestajos = new TreeListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.25F),
                UseAlternatingBackColors = false
            };

            marco.Controls.Add(olvDestajos);
            panelCentral.Controls.Add(marco);

            // ===== Botones inferiores =====
            var panelBotones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = colorSuave,
                Padding = new Padding(20, 12, 20, 12)
            };

            var sepTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = colorBorde
            };
            panelBotones.Controls.Add(sepTop);

            btnExpandirTodo = new Button
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
            btnExpandirTodo.FlatAppearance.BorderSize = 0;
            btnExpandirTodo.Click += (s, e) => olvDestajos.ExpandAll();

            btnContraerTodo = new Button
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
            btnContraerTodo.FlatAppearance.BorderSize = 0;
            btnContraerTodo.Click += (s, e) => olvDestajos.CollapseAll();

            btnGenerarDistribucion = new Button
            {
                Text = "Generar Distribución de Nómina",
                Location = new Point(296, 14),
                Size = new Size(240, 32),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerarDistribucion.FlatAppearance.BorderSize = 0;
            btnGenerarDistribucion.Click += BtnGenerarDistribucion_Click;

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(this.ClientSize.Width - 130, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(108, 32),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => this.Close();

            panelBotones.Controls.Add(btnExpandirTodo);
            panelBotones.Controls.Add(btnContraerTodo);
            panelBotones.Controls.Add(btnGenerarDistribucion);
            panelBotones.Controls.Add(btnCerrar);

            // ===== Ensamblar =====
            this.Controls.Add(panelCentral);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelFiltros);
            this.Controls.Add(panelBanner);
        }

        private void ConfigurarTreeListView()
        {
            olvDestajos.CanExpandGetter = obj =>
            {
                var n = obj as NodoReporte;
                return n != null && n.Hijos != null && n.Hijos.Count > 0;
            };
            olvDestajos.ChildrenGetter = obj =>
            {
                var n = obj as NodoReporte;
                return n?.Hijos ?? new List<NodoReporte>();
            };

            var colNombre = new OLVColumn("Concepto / Cuadrilla", "Etiqueta")
            {
                Width = 420,
                IsEditable = false,
                Sortable = false,
                AspectGetter = obj => (obj as NodoReporte)?.Etiqueta ?? ""
            };

            var colCasa = new OLVColumn("Casa", "Casa")
            {
                Width = 80,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = obj => (obj as NodoReporte)?.Casa ?? ""
            };

            var colRuta = new OLVColumn("Ruta", "Ruta")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = obj => (obj as NodoReporte)?.RutaCorta ?? ""
            };

            var colFecha = new OLVColumn("Finalización", "FechaFinalizacion")
            {
                Width = 120,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = obj =>
                {
                    var n = obj as NodoReporte;
                    return n?.FechaFinalizacion.HasValue == true
                        ? n.FechaFinalizacion.Value.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture)
                        : "";
                }
            };

            var colDestajos = new OLVColumn("# Destajos", "Conteo")
            {
                Width = 90,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = obj =>
                {
                    var n = obj as NodoReporte;
                    if (n == null) return "";
                    if (n.Nivel == 0) return ContarDestajos(n).ToString();
                    if (n.Nivel == 1) return n.Hijos.Count.ToString();
                    return "";
                }
            };

            var colImporte = new OLVColumn("Importe", "Importe")
            {
                Width = 130,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                Sortable = false,
                AspectGetter = obj =>
                {
                    var n = obj as NodoReporte;
                    return n?.Importe.ToString("C2", CultureInfo.CurrentCulture) ?? "";
                }
            };

            var colPago = new OLVColumn("Pago", "Pago")
            {
                Width = 130,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = obj =>
                {
                    var n = obj as NodoReporte;
                    if (n == null || n.Nivel != 2) return "";
                    return n.NominaDistribuida
                        ? (n.FechaDistribucionNomina.HasValue
                            ? "✓ Distribuida " + n.FechaDistribucionNomina.Value.ToString("dd/MM/yyyy")
                            : "✓ Distribuida")
                        : "● Pendiente";
                }
            };

            olvDestajos.AllColumns.AddRange(new[]
            {
                colNombre, colCasa, colRuta, colFecha, colDestajos, colImporte, colPago
            });
            olvDestajos.RebuildColumns();

            olvDestajos.FormatRow += (s, e) =>
            {
                var n = e.Model as NodoReporte;
                if (n == null) return;
                switch (n.Nivel)
                {
                    case 0: // Cuadrilla
                        e.Item.BackColor = Color.FromArgb(41, 60, 88);
                        e.Item.ForeColor = Color.White;
                        e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                        break;
                    case 1: // Categoría
                        e.Item.BackColor = Color.FromArgb(220, 230, 245);
                        e.Item.ForeColor = Color.FromArgb(33, 47, 67);
                        e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                        break;
                    case 2: // Destajo
                        if (n.NominaDistribuida)
                        {
                            // Atenuar las filas ya distribuidas para que las pendientes
                            // resalten cuando se ve la vista "Todas".
                            e.Item.BackColor = Color.FromArgb(241, 243, 246);
                            e.Item.ForeColor = Color.FromArgb(120, 130, 145);
                            e.Item.Font = new Font(e.Item.Font, FontStyle.Italic);
                        }
                        else
                        {
                            e.Item.BackColor = Color.White;
                            e.Item.ForeColor = Color.FromArgb(40, 50, 65);
                        }
                        break;
                }
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
                    "Filtro de fechas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool soloCasa = chkSoloCasaActual.Checked && _tieneCasaCargada;

            try
            {
                // Asegurar columnas de marcado de nómina distribuida antes de consultar.
                FormDistribucionNomina.EnsureColumnasNominaDistribuida(_connectionString);

                _registrosCache = ConsultarRegistros(desde, hasta, soloCasa);
                RefrescarVista();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar destajos finalizados:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Reaplica el filtro de estado de pago sobre los registros ya consultados,
        /// re-arma el árbol y refresca el grid. Se invoca al cargar y al cambiar
        /// cmbEstadoPago, sin volver a tocar la base de datos.
        /// </summary>
        private void RefrescarVista()
        {
            if (_registrosCache == null) return;

            string filtroPago = cmbEstadoPago?.SelectedItem?.ToString() ?? EstadoPagoSoloPendientes;
            IEnumerable<RegistroDestajo> filtrados;
            switch (filtroPago)
            {
                case EstadoPagoSoloDistribuidas:
                    filtrados = _registrosCache.Where(r => r.NominaDistribuida);
                    break;
                case EstadoPagoTodas:
                    filtrados = _registrosCache;
                    break;
                default:
                    filtrados = _registrosCache.Where(r => !r.NominaDistribuida);
                    break;
            }

            _nodosCuadrilla = AgruparRegistros(filtrados.ToList());
            olvDestajos.SetObjects(_nodosCuadrilla);
            olvDestajos.ExpandAll();

            DateTime desde = dtpDesde.Value.Date;
            DateTime hasta = dtpHasta.Value.Date.AddDays(1).AddSeconds(-1);
            bool soloCasa = chkSoloCasaActual.Checked && _tieneCasaCargada;
            ActualizarResumen(soloCasa, desde, hasta);
            AplicarFiltroBusqueda();
        }

        private List<RegistroDestajo> ConsultarRegistros(
            DateTime desde, DateTime hasta, bool soloCasa)
        {
            var registros = new List<RegistroDestajo>();

            // Cada ruta tiene su propia tabla; consultamos las dos posibles.
            foreach (var ruta in new[] { "RutaCalandraDestajo", "RutaTuneraDestajo" })
            {
                if (soloCasa && !string.IsNullOrEmpty(_rutaActual) && _rutaActual != ruta)
                    continue;

                if (!ExisteTabla(ruta))
                    continue;

                registros.AddRange(ConsultarRegistrosDeRuta(ruta, desde, hasta, soloCasa));
            }

            return registros;
        }

        private bool ExisteTabla(string nombreTabla)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = @t", conn))
                    {
                        cmd.Parameters.AddWithValue("@t", nombreTabla);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private List<RegistroDestajo> ConsultarRegistrosDeRuta(
            string ruta, DateTime desde, DateTime hasta, bool soloCasa)
        {
            var lista = new List<RegistroDestajo>();

            // Importes se calculan por hijos (Nivel 2) con TRY_CAST para tolerar
            // valores no numéricos en la tabla de columnas dinámicas.
            string sql = $@"
                SELECT
                    a.CuadrillaAsignada,
                    a.Manzana,
                    a.Lote,
                    a.NodoID                AS DestajoID,
                    d.Nombre                AS DestajoNombre,
                    a.FechaFinalizacion,
                    cat.ID                  AS CategoriaID,
                    cat.Nombre              AS CategoriaNombre,
                    cat.Orden               AS CategoriaOrden,
                    d.Orden                 AS DestajoOrden,
                    ISNULL(imp.Importe, 0)  AS Importe,
                    ISNULL(a.NominaDistribuida, 0)   AS NominaDistribuida,
                    a.FechaDistribucionNomina        AS FechaDistribucionNomina
                FROM ActivacionTareasRuta a
                INNER JOIN {ruta} d   ON d.ID = a.NodoID
                INNER JOIN {ruta} cat ON cat.ID = d.ParentId
                OUTER APPLY (
                    SELECT SUM(
                        ISNULL(TRY_CAST(cant.Valor AS DECIMAL(18,4)), 0) *
                        ISNULL(TRY_CAST(prec.Valor AS DECIMAL(18,4)), 0)
                    ) AS Importe
                    FROM {ruta} h
                    LEFT JOIN {ruta}_Columnas cant
                        ON cant.NodoID = h.ID AND cant.NombreColumna = 'Cantidad'
                    LEFT JOIN {ruta}_Columnas prec
                        ON prec.NodoID = h.ID AND prec.NombreColumna = 'Precio'
                    WHERE h.ParentId = d.ID
                ) imp
                WHERE a.Finalizado = 1
                  AND a.Ruta = @ruta
                  AND a.CuadrillaAsignada IS NOT NULL
                  AND LTRIM(RTRIM(a.CuadrillaAsignada)) <> ''
                  AND a.FechaFinalizacion BETWEEN @desde AND @hasta
                  {(soloCasa ? "AND a.Manzana = @m AND a.Lote = @l" : "")}
                ORDER BY a.CuadrillaAsignada, cat.Orden, d.Orden, a.Manzana, a.Lote;";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@desde", desde);
                    cmd.Parameters.AddWithValue("@hasta", hasta);
                    if (soloCasa)
                    {
                        cmd.Parameters.AddWithValue("@m", _manzanaActual);
                        cmd.Parameters.AddWithValue("@l", _loteActual);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new RegistroDestajo
                            {
                                Cuadrilla = reader["CuadrillaAsignada"].ToString(),
                                Manzana = reader["Manzana"].ToString(),
                                Lote = reader["Lote"].ToString(),
                                DestajoID = Convert.ToInt32(reader["DestajoID"]),
                                DestajoNombre = reader["DestajoNombre"].ToString(),
                                FechaFinalizacion = reader["FechaFinalizacion"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(reader["FechaFinalizacion"]),
                                CategoriaID = Convert.ToInt32(reader["CategoriaID"]),
                                CategoriaNombre = reader["CategoriaNombre"].ToString(),
                                Importe = reader["Importe"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["Importe"]),
                                Ruta = ruta,
                                NominaDistribuida = reader["NominaDistribuida"] != DBNull.Value
                                    && Convert.ToBoolean(reader["NominaDistribuida"]),
                                FechaDistribucionNomina = reader["FechaDistribucionNomina"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(reader["FechaDistribucionNomina"])
                            });
                        }
                    }
                }
            }

            return lista;
        }

        private List<NodoReporte> AgruparRegistros(List<RegistroDestajo> registros)
        {
            var nodosCuadrilla = new List<NodoReporte>();

            var porCuadrilla = registros
                .GroupBy(r => r.Cuadrilla ?? "")
                .OrderBy(g => g.Key);

            foreach (var grupoC in porCuadrilla)
            {
                var nodoC = new NodoReporte
                {
                    Nivel = 0,
                    Etiqueta = $"Cuadrilla {grupoC.Key}",
                    Hijos = new List<NodoReporte>(),
                    Importe = grupoC.Sum(r => r.Importe)
                };

                var porCategoria = grupoC
                    .GroupBy(r => new { r.CategoriaID, r.CategoriaNombre })
                    .OrderBy(g => g.Key.CategoriaNombre);

                foreach (var grupoCat in porCategoria)
                {
                    var nodoCat = new NodoReporte
                    {
                        Nivel = 1,
                        Etiqueta = grupoCat.Key.CategoriaNombre ?? "(Sin categoría)",
                        Hijos = new List<NodoReporte>(),
                        Importe = grupoCat.Sum(r => r.Importe)
                    };

                    foreach (var r in grupoCat.OrderBy(x => x.Manzana).ThenBy(x => x.Lote))
                    {
                        nodoCat.Hijos.Add(new NodoReporte
                        {
                            Nivel = 2,
                            Etiqueta = r.DestajoNombre,
                            Casa = $"M{r.Manzana}-L{r.Lote}",
                            Manzana = r.Manzana,
                            Lote = r.Lote,
                            Ruta = r.Ruta,
                            RutaCorta = r.Ruta == "RutaCalandraDestajo" ? "Calandra" : "Tunera",
                            FechaFinalizacion = r.FechaFinalizacion,
                            Importe = r.Importe,
                            DestajoID = r.DestajoID,
                            NominaDistribuida = r.NominaDistribuida,
                            FechaDistribucionNomina = r.FechaDistribucionNomina,
                            Hijos = new List<NodoReporte>()
                        });
                    }

                    nodoC.Hijos.Add(nodoCat);
                }

                nodosCuadrilla.Add(nodoC);
            }

            return nodosCuadrilla;
        }

        private int ContarDestajos(NodoReporte cuadrilla)
        {
            if (cuadrilla?.Hijos == null) return 0;
            return cuadrilla.Hijos.Sum(c => c.Hijos?.Count ?? 0);
        }

        #endregion

        #region Resumen / búsqueda

        private void ActualizarResumen(bool soloCasa, DateTime desde, DateTime hasta)
        {
            int totalCuadrillas = _nodosCuadrilla.Count;
            int totalDestajos = _nodosCuadrilla.Sum(c => ContarDestajos(c));
            decimal montoTotal = _nodosCuadrilla.Sum(c => c.Importe);

            string alcance = soloCasa
                ? $"Casa M{_manzanaActual}-L{_loteActual}"
                : "Todas las casas";

            lblResumen.Text = string.Format(
                "{0}  ·  Periodo: {1} a {2}  ·  Cuadrillas: {3}  ·  Destajos finalizados: {4}  ·  Importe total: {5}",
                alcance,
                desde.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture),
                hasta.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture),
                totalCuadrillas,
                totalDestajos,
                montoTotal.ToString("C2", CultureInfo.CurrentCulture));
        }

        private void AplicarFiltroBusqueda()
        {
            string termino = (txtBuscar.Text ?? "").Trim();
            if (string.IsNullOrEmpty(termino))
            {
                olvDestajos.ModelFilter = null;
                olvDestajos.UseFiltering = false;
                return;
            }

            olvDestajos.UseFiltering = true;
            olvDestajos.ModelFilter = new ModelFilter(o =>
            {
                var n = o as NodoReporte;
                if (n == null) return false;
                return CoincideRecursivo(n, termino);
            });
        }

        private static bool CoincideRecursivo(NodoReporte n, string termino)
        {
            if (n == null) return false;
            if ((n.Etiqueta ?? "").IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if ((n.Casa ?? "").IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (n.Hijos != null)
            {
                foreach (var h in n.Hijos)
                    if (CoincideRecursivo(h, termino)) return true;
            }
            return false;
        }

        private static DateTime ObtenerLunesDeEstaSemana(DateTime referencia)
        {
            int diff = (7 + (referencia.DayOfWeek - DayOfWeek.Monday)) % 7;
            return referencia.AddDays(-diff).Date;
        }

        #endregion

        #region Distribución de Nómina

        private void BtnGenerarDistribucion_Click(object sender, EventArgs e)
        {
            NodoReporte nodoCuadrilla = ObtenerCuadrillaSeleccionada();
            if (nodoCuadrilla == null)
            {
                MessageBox.Show(
                    "Selecciona una cuadrilla (o cualquier nodo dentro de ella) para generar la distribución.",
                    "Distribución de Nómina", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string codigo = ExtraerCodigoCuadrilla(nodoCuadrilla.Etiqueta);
            var destajos = AplanarDestajosDe(nodoCuadrilla);
            if (destajos.Count == 0)
            {
                MessageBox.Show("La cuadrilla seleccionada no tiene destajos finalizados en el periodo.",
                    "Distribución de Nómina", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Bloquear doble pago: si la selección incluye destajos cuya nómina ya
            // fue distribuida, pedir confirmación explícita antes de continuar.
            var yaDistribuidos = destajos.Where(d => d.NominaDistribuida).ToList();
            if (yaDistribuidos.Count > 0)
            {
                var resumen = string.Join(
                    Environment.NewLine,
                    yaDistribuidos.Take(5).Select(d =>
                        $"  • {d.Nombre}  (M{d.Manzana}-L{d.Lote}, distribuido el {d.FechaDistribucionNomina:dd/MM/yyyy})"));
                if (yaDistribuidos.Count > 5)
                    resumen += Environment.NewLine + $"  • ... y {yaDistribuidos.Count - 5} más";

                var resp = MessageBox.Show(
                    $"{yaDistribuidos.Count} destajo(s) ya tienen nómina distribuida:\n\n{resumen}\n\n" +
                    "¿Continuar y volver a generar recibos para ellos?\n\n" +
                    "Esto sobrescribirá la fecha de distribución y emitirá nuevos PDFs.",
                    "Nómina ya distribuida",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (resp != DialogResult.Yes) return;
            }

            using (var form = new FormDistribucionNomina(
                codigo,
                dtpDesde.Value.Date,
                dtpHasta.Value.Date,
                destajos))
            {
                form.ShowDialog(this);
                // Tras cerrar, refrescar la vista para reflejar las nuevas marcas.
                CargarDatos();
            }
        }

        private NodoReporte ObtenerCuadrillaSeleccionada()
        {
            var sel = olvDestajos.SelectedObject as NodoReporte;
            if (sel == null)
            {
                if (_nodosCuadrilla.Count == 1) return _nodosCuadrilla[0];
                return null;
            }
            if (sel.Nivel == 0) return sel;

            foreach (var c in _nodosCuadrilla)
            {
                if (ContieneNodo(c, sel)) return c;
            }
            return null;
        }

        private static bool ContieneNodo(NodoReporte raiz, NodoReporte buscado)
        {
            if (raiz == buscado) return true;
            if (raiz.Hijos == null) return false;
            foreach (var h in raiz.Hijos)
                if (ContieneNodo(h, buscado)) return true;
            return false;
        }

        private static string ExtraerCodigoCuadrilla(string etiqueta)
        {
            if (string.IsNullOrEmpty(etiqueta)) return "";
            const string prefijo = "Cuadrilla ";
            return etiqueta.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase)
                ? etiqueta.Substring(prefijo.Length).Trim()
                : etiqueta.Trim();
        }

        private static List<FormDistribucionNomina.DestajoTerminado> AplanarDestajosDe(NodoReporte cuadrilla)
        {
            var lista = new List<FormDistribucionNomina.DestajoTerminado>();
            if (cuadrilla?.Hijos == null) return lista;

            foreach (var cat in cuadrilla.Hijos)
            {
                if (cat?.Hijos == null) continue;
                foreach (var d in cat.Hijos)
                {
                    lista.Add(new FormDistribucionNomina.DestajoTerminado
                    {
                        Cuadrilla = ExtraerCodigoCuadrilla(cuadrilla.Etiqueta),
                        Casa = d.Casa,
                        Manzana = d.Manzana,
                        Lote = d.Lote,
                        Nombre = d.Etiqueta,
                        Categoria = cat.Etiqueta,
                        FechaFinalizacion = d.FechaFinalizacion,
                        Importe = d.Importe,
                        Ruta = d.Ruta,
                        NodoID = d.DestajoID,
                        NominaDistribuida = d.NominaDistribuida,
                        FechaDistribucionNomina = d.FechaDistribucionNomina
                    });
                }
            }
            return lista;
        }

        #endregion

        #region Modelos

        private class RegistroDestajo
        {
            public string Cuadrilla { get; set; }
            public string Manzana { get; set; }
            public string Lote { get; set; }
            public int DestajoID { get; set; }
            public string DestajoNombre { get; set; }
            public DateTime? FechaFinalizacion { get; set; }
            public int CategoriaID { get; set; }
            public string CategoriaNombre { get; set; }
            public decimal Importe { get; set; }
            public string Ruta { get; set; }
            public bool NominaDistribuida { get; set; }
            public DateTime? FechaDistribucionNomina { get; set; }
        }

        public class NodoReporte
        {
            public int Nivel { get; set; } // 0=Cuadrilla, 1=Categoría, 2=Destajo
            public string Etiqueta { get; set; } = "";
            public string Casa { get; set; } = "";
            public string Manzana { get; set; } = "";
            public string Lote { get; set; } = "";
            public string Ruta { get; set; } = "";
            public string RutaCorta { get; set; } = "";
            public DateTime? FechaFinalizacion { get; set; }
            public decimal Importe { get; set; }
            public int DestajoID { get; set; }
            public bool NominaDistribuida { get; set; }
            public DateTime? FechaDistribucionNomina { get; set; }
            public List<NodoReporte> Hijos { get; set; } = new List<NodoReporte>();
        }

        #endregion
    }
}
