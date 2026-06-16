using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Captura el monto y concepto por trabajador de una cuadrilla y genera un PDF
    /// "Distribución de Nómina" con (1) el listado de destajos finalizados y
    /// (2) un recibo de nómina para cada miembro. Cada recibo se persiste en la
    /// tabla RecibosNomina para su consulta posterior desde el perfil del trabajador.
    /// </summary>
    public class FormDistribucionNomina : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private readonly string _codigoCuadrilla;
        private readonly DateTime _desde;
        private readonly DateTime _hasta;
        private readonly List<DestajoTerminado> _destajos;
        private readonly decimal _totalCuadrilla;

        private readonly BindingList<MiembroRecibo> _miembros = new BindingList<MiembroRecibo>();

        private Label lblTitulo;
        private Label lblCuadrilla;
        private Label lblPeriodo;
        private Label lblTotal;
        private Label lblDestajos;
        private DataGridView dgvMiembros;
        private Label lblResumen;
        private Button btnDistribuirIgual;
        private Button btnConceptoAuto;
        private Button btnGenerar;
        private Button btnCerrar;

        public FormDistribucionNomina(
            string codigoCuadrilla,
            DateTime desde,
            DateTime hasta,
            List<DestajoTerminado> destajos)
        {
            _codigoCuadrilla = codigoCuadrilla ?? "";
            _desde = desde.Date;
            _hasta = hasta.Date;
            _destajos = destajos ?? new List<DestajoTerminado>();
            _totalCuadrilla = _destajos.Sum(d => d.Importe);

            BuildUI();
            ThemeManager.AplicarTema(this);
            EnsureTablaRecibos();
            EnsureColumnasNominaDistribuida(_connectionString);
            CargarMiembrosCuadrilla();
            ActualizarResumen();

            this.Load += (s, e) => { if (_miembros.Count > 0) DistribuirIgual(); };
        }

        #region UI

        private void BuildUI()
        {
            this.Text = "Distribución de Nómina por Cuadrilla";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(960, 620);
            this.ClientSize = new Size(1040, 680);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);

            var panelBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(41, 60, 88)
            };
            lblTitulo = new Label
            {
                Text = "DISTRIBUCIÓN DE NÓMINA",
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 8)
            };
            var lblSub = new Label
            {
                Text = "Captura monto y concepto por trabajador para emitir los recibos",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(189, 200, 215),
                AutoSize = true,
                Location = new Point(22, 34)
            };
            panelBanner.Controls.Add(lblTitulo);
            panelBanner.Controls.Add(lblSub);

            // ===== Datos cabecera =====
            lblCuadrilla = new Label
            {
                Text = "Cuadrilla: " + _codigoCuadrilla,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 47, 67),
                Location = new Point(22, 78),
                AutoSize = true
            };
            lblPeriodo = new Label
            {
                Text = string.Format("Periodo: {0:dd/MM/yyyy} - {1:dd/MM/yyyy}",
                    _desde, _hasta),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(99, 110, 114),
                Location = new Point(22, 102),
                AutoSize = true
            };
            lblDestajos = new Label
            {
                Text = "Destajos finalizados: " + _destajos.Count.ToString(CultureInfo.InvariantCulture),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(99, 110, 114),
                Location = new Point(280, 102),
                AutoSize = true
            };
            lblTotal = new Label
            {
                Text = "Total destajos: " +
                    _totalCuadrilla.ToString("C2", CultureInfo.GetCultureInfo("es-MX")),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96),
                Location = new Point(22, 128),
                AutoSize = true
            };

            // ===== Grid trabajadores =====
            dgvMiembros = new DataGridView
            {
                Location = new Point(22, 160),
                Size = new Size(996, 410),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EditMode = DataGridViewEditMode.EditOnEnter
            };
            dgvMiembros.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvMiembros.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvMiembros.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvMiembros.EnableHeadersVisualStyles = false;
            dgvMiembros.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            dgvMiembros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Trabajador",
                DataPropertyName = "Nombre",
                Width = 260,
                ReadOnly = true
            });
            dgvMiembros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Rol",
                DataPropertyName = "Rol",
                Width = 110,
                ReadOnly = true
            });
            dgvMiembros.Columns.Add(new DataGridViewCheckBoxColumn
            {
                HeaderText = "Jefe",
                DataPropertyName = "EsJefe",
                Width = 50,
                ReadOnly = true
            });
            dgvMiembros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Monto",
                DataPropertyName = "Monto",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(255, 252, 220)
                }
            });
            dgvMiembros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Concepto (texto del recibo)",
                DataPropertyName = "Concepto",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(255, 252, 220),
                    WrapMode = DataGridViewTriState.True
                }
            });

            dgvMiembros.DataSource = _miembros;
            dgvMiembros.CellValidating += DgvMiembros_CellValidating;
            dgvMiembros.CellEndEdit += (s, e) => ActualizarResumen();

            // ===== Resumen + Botones =====
            lblResumen = new Label
            {
                Location = new Point(22, 580),
                Size = new Size(700, 22),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnDistribuirIgual = new Button
            {
                Text = "Distribuir Igualmente",
                Location = new Point(22, 612),
                Size = new Size(160, 38),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnDistribuirIgual.Click += (s, e) => { DistribuirIgual(); ActualizarResumen(); };

            btnConceptoAuto = new Button
            {
                Text = "Concepto desde destajos",
                Location = new Point(190, 612),
                Size = new Size(180, 38),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnConceptoAuto.Click += (s, e) => { LlenarConceptoAuto(); RefrescarGrid(); };

            btnGenerar = new Button
            {
                Text = "Generar PDF",
                Location = new Point(800, 612),
                Size = new Size(140, 38),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnGenerar.Click += BtnGenerar_Click;

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(946, 612),
                Size = new Size(72, 38),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.Add(dgvMiembros);
            this.Controls.Add(lblResumen);
            this.Controls.Add(btnDistribuirIgual);
            this.Controls.Add(btnConceptoAuto);
            this.Controls.Add(btnGenerar);
            this.Controls.Add(btnCerrar);
            this.Controls.Add(lblCuadrilla);
            this.Controls.Add(lblPeriodo);
            this.Controls.Add(lblDestajos);
            this.Controls.Add(lblTotal);
            this.Controls.Add(panelBanner);
        }

        #endregion

        #region Carga de datos

        private void CargarMiembrosCuadrilla()
        {
            _miembros.Clear();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT IdTrabajador, Nombre, Rol, EsJefe, Telefono
                        FROM MiembrosCuadrilla
                        WHERE CodigoCuadrilla = @codigo
                        ORDER BY EsJefe DESC, Nombre", conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", _codigoCuadrilla);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _miembros.Add(new MiembroRecibo
                                {
                                    IdTrabajador = reader["IdTrabajador"] == DBNull.Value
                                        ? (int?)null
                                        : Convert.ToInt32(reader["IdTrabajador"]),
                                    Nombre = reader["Nombre"].ToString(),
                                    Rol = reader["Rol"].ToString(),
                                    EsJefe = Convert.ToBoolean(reader["EsJefe"]),
                                    Telefono = reader["Telefono"].ToString(),
                                    Monto = 0m,
                                    Concepto = ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando miembros de la cuadrilla: " + ex.Message,
                    "Cuadrilla", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            LlenarConceptoAuto();
            RefrescarGrid();
        }

        private void LlenarConceptoAuto()
        {
            string conceptoAuto = ConstruirConceptoDesdeDestajos();
            foreach (var m in _miembros)
            {
                if (string.IsNullOrWhiteSpace(m.Concepto))
                    m.Concepto = conceptoAuto;
            }
        }

        private string ConstruirConceptoDesdeDestajos()
        {
            if (_destajos == null || _destajos.Count == 0) return "";
            var nombres = _destajos
                .Select(d => (d.Nombre ?? "").Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToList();
            return string.Join(", ", nombres);
        }

        #endregion

        #region Distribución y validación

        private void DistribuirIgual()
        {
            if (_miembros.Count == 0) return;

            decimal porPersona = Math.Round(_totalCuadrilla / _miembros.Count, 2);
            decimal asignado = porPersona * _miembros.Count;
            decimal diferencia = _totalCuadrilla - asignado;

            for (int i = 0; i < _miembros.Count; i++)
                _miembros[i].Monto = porPersona;

            if (diferencia != 0m && _miembros.Count > 0)
            {
                int idx = 0;
                for (int i = 0; i < _miembros.Count; i++)
                {
                    if (_miembros[i].EsJefe) { idx = i; break; }
                }
                _miembros[idx].Monto += diferencia;
            }

            RefrescarGrid();
        }

        private void DgvMiembros_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var col = dgvMiembros.Columns[e.ColumnIndex];
            if (col.DataPropertyName != "Monto") return;

            string txt = (e.FormattedValue ?? "").ToString().Trim();
            if (string.IsNullOrEmpty(txt))
            {
                dgvMiembros.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0m;
                return;
            }
            decimal valor;
            if (!decimal.TryParse(txt, NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
                    CultureInfo.CurrentCulture, out valor) &&
                !decimal.TryParse(txt, NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
                    CultureInfo.InvariantCulture, out valor))
            {
                MessageBox.Show("El monto debe ser un número válido.", "Monto inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }
            if (valor < 0)
            {
                MessageBox.Show("El monto no puede ser negativo.", "Monto inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        }

        private void ActualizarResumen()
        {
            decimal asignado = _miembros.Sum(m => m.Monto);
            decimal dif = _totalCuadrilla - asignado;
            var cul = CultureInfo.GetCultureInfo("es-MX");
            lblResumen.Text = string.Format(
                "Asignado: {0}   |   Total destajos: {1}   |   Diferencia: {2}",
                asignado.ToString("C2", cul),
                _totalCuadrilla.ToString("C2", cul),
                dif.ToString("C2", cul));

            if (Math.Abs(dif) < 0.01m) lblResumen.ForeColor = Color.FromArgb(39, 174, 96);
            else if (asignado > _totalCuadrilla) lblResumen.ForeColor = Color.FromArgb(192, 57, 43);
            else lblResumen.ForeColor = Color.FromArgb(211, 84, 0);
        }

        private void RefrescarGrid()
        {
            dgvMiembros.DataSource = null;
            dgvMiembros.DataSource = _miembros;
        }

        #endregion

        #region Persistencia

        private void EnsureTablaRecibos()
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RecibosNomina')
BEGIN
    CREATE TABLE RecibosNomina (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdTrabajador INT NULL,
        NombreTrabajador NVARCHAR(200) NOT NULL,
        Rol NVARCHAR(80) NULL,
        CodigoCuadrilla NVARCHAR(20) NULL,
        Concepto NVARCHAR(MAX) NULL,
        Monto DECIMAL(18,2) NOT NULL,
        FechaRecibo DATE NOT NULL,
        PeriodoDesde DATE NULL,
        PeriodoHasta DATE NULL,
        TotalCuadrilla DECIMAL(18,2) NULL,
        Pdf VARBINARY(MAX) NULL,
        Usuario NVARCHAR(120) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
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
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo preparar la tabla de recibos: " + ex.Message,
                    "Recibos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Agrega de forma idempotente las columnas NominaDistribuida y
        /// FechaDistribucionNomina a ActivacionTareasRuta. Se usa como bandera
        /// para distinguir destajos cuya nómina ya fue distribuida (con recibos
        /// generados) de los que siguen pendientes de pago.
        /// </summary>
        public static void EnsureColumnasNominaDistribuida(string connectionString)
        {
            const string sql = @"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ActivacionTareasRuta')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE Name = N'NominaDistribuida'
                     AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    BEGIN
        ALTER TABLE ActivacionTareasRuta ADD NominaDistribuida BIT NOT NULL DEFAULT 0;
    END
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE Name = N'FechaDistribucionNomina'
                     AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    BEGIN
        ALTER TABLE ActivacionTareasRuta ADD FechaDistribucionNomina DATETIME NULL;
    END
END";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                        cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Migración silenciosa: si falla, el resto del flujo seguirá
                // funcionando aunque sin el marcado de nómina distribuida.
            }
        }

        /// <summary>
        /// Marca como distribuida la nómina de los destajos pasados, dejando
        /// también la fecha en que se generó la distribución. Esto evita doble
        /// pago y permite separar visualmente pendientes vs distribuidas en
        /// FormDestajosPorCuadrilla.
        /// </summary>
        private void MarcarNominaDistribuida(IEnumerable<DestajoTerminado> destajos)
        {
            if (destajos == null) return;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    foreach (var d in destajos)
                    {
                        if (d.NodoID <= 0) continue;
                        using (var cmd = new SqlCommand(@"
                            UPDATE ActivacionTareasRuta
                               SET NominaDistribuida = 1,
                                   FechaDistribucionNomina = GETDATE()
                             WHERE Manzana = @m
                               AND Lote    = @l
                               AND Ruta    = @r
                               AND NodoID  = @nodo", conn))
                        {
                            cmd.Parameters.AddWithValue("@m", d.Manzana ?? "");
                            cmd.Parameters.AddWithValue("@l", d.Lote ?? "");
                            cmd.Parameters.AddWithValue("@r", d.Ruta ?? "");
                            cmd.Parameters.AddWithValue("@nodo", d.NodoID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Los recibos se guardaron pero no se pudo marcar la nómina como distribuida:\n" + ex.Message,
                    "Marcado de nómina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GuardarRecibo(MiembroRecibo m, byte[] pdfRecibo)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        INSERT INTO RecibosNomina
                        (IdTrabajador, NombreTrabajador, Rol, CodigoCuadrilla, Concepto, Monto,
                         FechaRecibo, PeriodoDesde, PeriodoHasta, TotalCuadrilla, Pdf, Usuario)
                        VALUES
                        (@id, @nombre, @rol, @codigo, @concepto, @monto,
                         @fecha, @desde, @hasta, @total, @pdf, @usuario)", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", (object)m.IdTrabajador ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@nombre", m.Nombre ?? "");
                        cmd.Parameters.AddWithValue("@rol", (object)m.Rol ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@codigo", _codigoCuadrilla ?? "");
                        cmd.Parameters.AddWithValue("@concepto", m.Concepto ?? "");
                        cmd.Parameters.AddWithValue("@monto", m.Monto);
                        cmd.Parameters.AddWithValue("@fecha", DateTime.Today);
                        cmd.Parameters.AddWithValue("@desde", _desde);
                        cmd.Parameters.AddWithValue("@hasta", _hasta);
                        cmd.Parameters.AddWithValue("@total", _totalCuadrilla);
                        var pPdf = cmd.Parameters.Add("@pdf", System.Data.SqlDbType.VarBinary, -1);
                        pPdf.Value = (pdfRecibo != null && pdfRecibo.Length > 0)
                            ? (object)pdfRecibo
                            : DBNull.Value;
                        cmd.Parameters.AddWithValue("@usuario",
                            (object)(Global.UsuarioActual?.Nombre) ?? Environment.UserName);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "El PDF se generó pero no se pudo guardar el recibo de " + m.Nombre +
                    " en la base de datos:\n" + ex.Message,
                    "Recibos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Generación de PDF

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            if (_miembros.Count == 0)
            {
                MessageBox.Show("La cuadrilla no tiene miembros para generar recibos.",
                    "Distribución de Nómina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var m in _miembros)
            {
                if (string.IsNullOrWhiteSpace(m.Concepto))
                {
                    MessageBox.Show(
                        "Falta el concepto del recibo para " + m.Nombre + ".",
                        "Distribución de Nómina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                string nombreArchivo = string.Format(
                    "DistribucionNomina_{0}_{1:yyyyMMdd_HHmmss}.pdf",
                    SanitizarNombre(_codigoCuadrilla), DateTime.Now);
                string archivoTemp = Path.Combine(Path.GetTempPath(), nombreArchivo);

                CrearPdfDistribucion(archivoTemp);

                foreach (var m in _miembros)
                {
                    byte[] pdfRecibo = CrearPdfReciboIndividual(m);
                    GuardarRecibo(m, pdfRecibo);
                }

                // Una vez generados todos los recibos, marcar los destajos
                // correspondientes como nómina distribuida.
                MarcarNominaDistribuida(_destajos);

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    nombreArchivo);

                using (var preview = new FormPdfPreview(archivoTemp, sugerido))
                {
                    preview.ShowDialog(this);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generando el PDF de distribución:\n" + ex.Message,
                    "Distribución de Nómina", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string SanitizarNombre(string s)
        {
            if (string.IsNullOrEmpty(s)) return "Cuadrilla";
            var sb = new StringBuilder();
            foreach (var c in s)
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            return sb.ToString();
        }

        private void CrearPdfDistribucion(string archivo)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Distribución de Nómina - " + _codigoCuadrilla;
            doc.Info.Author = "Calandria Residencial";

            DibujarPaginaListadoDestajos(doc);

            foreach (var m in _miembros)
                DibujarPaginaRecibo(doc, m);

            doc.Save(archivo);
        }

        private byte[] CrearPdfReciboIndividual(MiembroRecibo m)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Recibo de Nómina - " + m.Nombre;
            doc.Info.Author = "Calandria Residencial";
            DibujarPaginaRecibo(doc, m);

            using (var ms = new MemoryStream())
            {
                doc.Save(ms, false);
                return ms.ToArray();
            }
        }

        private void DibujarPaginaListadoDestajos(PdfDocument doc)
        {
            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            var gfx = XGraphics.FromPdfPage(page);

            var fuenteTitulo = new XFont("Arial", 16, XFontStyle.Bold);
            var fuenteSubtitulo = new XFont("Arial", 11, XFontStyle.Bold);
            var fuenteSeccion = new XFont("Arial", 10, XFontStyle.Bold);
            var fuenteEnc = new XFont("Arial", 9, XFontStyle.Bold);
            var fuenteNorm = new XFont("Arial", 9, XFontStyle.Regular);
            var fuentePeq = new XFont("Arial", 8, XFontStyle.Regular);

            var brochaAzul = new XSolidBrush(XColor.FromArgb(13, 71, 161));
            var brochaCelta = new XSolidBrush(XColor.FromArgb(33, 150, 243));
            var brochaTexto = new XSolidBrush(XColor.FromArgb(33, 33, 33));
            var brochaGris = new XSolidBrush(XColor.FromArgb(245, 245, 245));
            var brochaVerde = new XSolidBrush(XColor.FromArgb(39, 174, 96));
            var brochaBlanca = XBrushes.White;
            var penBorde = new XPen(XColor.FromArgb(189, 189, 189), 0.5);

            double margen = 30;
            double ancho = page.Width - 2 * margen;
            double y = margen;

            // Encabezado
            gfx.DrawRectangle(brochaAzul, margen, y, ancho, 50);
            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(
                        ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var logo = XImage.FromStream(ms);
                    gfx.DrawImage(logo, margen + 6, y + 6, 38, 38);
                }
            }
            catch { }
            gfx.DrawString("CAMANEY DESARROLLO Y CONSTRUCCIÓN SA DE CV",
                fuenteSubtitulo, brochaBlanca,
                new XRect(margen + 50, y + 6, ancho - 50, 14), XStringFormats.TopLeft);
            gfx.DrawString("DISTRIBUCIÓN DE NÓMINA", fuenteTitulo, brochaBlanca,
                new XRect(margen + 50, y + 22, ancho - 50, 18), XStringFormats.TopLeft);
            y += 60;

            // Info cuadrilla
            gfx.DrawRectangle(brochaGris, margen, y, ancho, 50);
            gfx.DrawRectangle(penBorde, margen, y, ancho, 50);
            var cul = CultureInfo.GetCultureInfo("es-MX");
            gfx.DrawString("Cuadrilla: " + _codigoCuadrilla, fuenteSubtitulo, brochaTexto,
                new XRect(margen + 6, y + 6, ancho - 12, 14), XStringFormats.TopLeft);
            gfx.DrawString(string.Format("Periodo: {0:dd/MM/yyyy} al {1:dd/MM/yyyy}", _desde, _hasta),
                fuenteNorm, brochaTexto,
                new XRect(margen + 6, y + 22, ancho - 12, 12), XStringFormats.TopLeft);
            gfx.DrawString(string.Format("Miembros: {0}    Destajos finalizados: {1}",
                _miembros.Count, _destajos.Count),
                fuenteNorm, brochaTexto,
                new XRect(margen + 6, y + 36, ancho - 12, 12), XStringFormats.TopLeft);
            y += 60;

            // Sección de destajos
            gfx.DrawRectangle(brochaCelta, margen, y, ancho, 16);
            gfx.DrawString("DESTAJOS FINALIZADOS DE LA CUADRILLA", fuenteSeccion, brochaBlanca,
                new XRect(margen + 4, y + 2, ancho - 8, 13), XStringFormats.TopLeft);
            y += 16;

            double colCasa = ancho * 0.12;
            double colDestajo = ancho * 0.46;
            double colCategoria = ancho * 0.20;
            double colFecha = ancho * 0.12;
            double colImporte = ancho - colCasa - colDestajo - colCategoria - colFecha;

            gfx.DrawRectangle(brochaAzul, margen, y, ancho, 14);
            double xCol = margen;
            gfx.DrawString("Casa", fuenteEnc, brochaBlanca,
                new XRect(xCol + 4, y + 2, colCasa - 4, 10), XStringFormats.TopLeft);
            xCol += colCasa;
            gfx.DrawString("Destajo", fuenteEnc, brochaBlanca,
                new XRect(xCol + 4, y + 2, colDestajo - 4, 10), XStringFormats.TopLeft);
            xCol += colDestajo;
            gfx.DrawString("Categoría", fuenteEnc, brochaBlanca,
                new XRect(xCol + 4, y + 2, colCategoria - 4, 10), XStringFormats.TopLeft);
            xCol += colCategoria;
            gfx.DrawString("Fecha", fuenteEnc, brochaBlanca,
                new XRect(xCol + 4, y + 2, colFecha - 4, 10), XStringFormats.TopLeft);
            xCol += colFecha;
            gfx.DrawString("Importe", fuenteEnc, brochaBlanca,
                new XRect(xCol + 4, y + 2, colImporte - 8, 10), XStringFormats.TopRight);
            y += 14;

            bool alt = false;
            decimal total = 0m;
            foreach (var d in _destajos.OrderBy(x => x.Categoria).ThenBy(x => x.Casa))
            {
                if (y > page.Height - 100)
                {
                    page = doc.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margen;
                }

                var brochaFila = alt ? brochaGris : new XSolidBrush(XColor.FromArgb(255, 255, 255));
                gfx.DrawRectangle(brochaFila, margen, y, ancho, 14);
                gfx.DrawRectangle(penBorde, margen, y, ancho, 14);

                xCol = margen;
                gfx.DrawString(Truncar(d.Casa ?? "-", 14), fuenteNorm, brochaTexto,
                    new XRect(xCol + 4, y + 2, colCasa - 4, 10), XStringFormats.TopLeft);
                xCol += colCasa;
                gfx.DrawString(Truncar(d.Nombre ?? "-", 60), fuenteNorm, brochaTexto,
                    new XRect(xCol + 4, y + 2, colDestajo - 4, 10), XStringFormats.TopLeft);
                xCol += colDestajo;
                gfx.DrawString(Truncar(d.Categoria ?? "-", 26), fuenteNorm, brochaTexto,
                    new XRect(xCol + 4, y + 2, colCategoria - 4, 10), XStringFormats.TopLeft);
                xCol += colCategoria;
                gfx.DrawString(d.FechaFinalizacion.HasValue
                    ? d.FechaFinalizacion.Value.ToString("dd/MM/yyyy", cul) : "-",
                    fuenteNorm, brochaTexto,
                    new XRect(xCol + 4, y + 2, colFecha - 4, 10), XStringFormats.TopLeft);
                xCol += colFecha;
                gfx.DrawString(d.Importe.ToString("C2", cul), fuenteNorm, brochaTexto,
                    new XRect(xCol + 4, y + 2, colImporte - 8, 10), XStringFormats.TopRight);

                y += 14;
                total += d.Importe;
                alt = !alt;
            }

            // Fila total
            gfx.DrawRectangle(brochaVerde, margen, y, ancho, 16);
            gfx.DrawString("TOTAL DESTAJOS", fuenteEnc, brochaBlanca,
                new XRect(margen + 4, y + 3, ancho - colImporte - 8, 12), XStringFormats.TopLeft);
            gfx.DrawString(total.ToString("C2", cul), fuenteEnc, brochaBlanca,
                new XRect(margen + ancho - colImporte + 4, y + 3, colImporte - 8, 12),
                XStringFormats.TopRight);
            y += 24;

            // Sección distribución por trabajador
            if (y > page.Height - 200)
            {
                page = doc.AddPage();
                page.Size = PdfSharp.PageSize.Letter;
                gfx = XGraphics.FromPdfPage(page);
                y = margen;
            }

            gfx.DrawRectangle(brochaCelta, margen, y, ancho, 16);
            gfx.DrawString("DISTRIBUCIÓN POR TRABAJADOR", fuenteSeccion, brochaBlanca,
                new XRect(margen + 4, y + 2, ancho - 8, 13), XStringFormats.TopLeft);
            y += 16;

            double cTrab = ancho * 0.40;
            double cRol = ancho * 0.18;
            double cConcepto = ancho * 0.27;
            double cMonto = ancho - cTrab - cRol - cConcepto;

            gfx.DrawRectangle(brochaAzul, margen, y, ancho, 14);
            xCol = margen;
            gfx.DrawString("Trabajador", fuenteEnc, brochaBlanca,
                new XRect(xCol + 4, y + 2, cTrab - 4, 10), XStringFormats.TopLeft);
            xCol += cTrab;
            gfx.DrawString("Rol", fuenteEnc, brochaBlanca,
                new XRect(xCol + 4, y + 2, cRol - 4, 10), XStringFormats.TopLeft);
            xCol += cRol;
            gfx.DrawString("Concepto", fuenteEnc, brochaBlanca,
                new XRect(xCol + 4, y + 2, cConcepto - 4, 10), XStringFormats.TopLeft);
            xCol += cConcepto;
            gfx.DrawString("Monto", fuenteEnc, brochaBlanca,
                new XRect(xCol + 4, y + 2, cMonto - 8, 10), XStringFormats.TopRight);
            y += 14;

            alt = false;
            decimal asignado = 0m;
            foreach (var m in _miembros)
            {
                if (y > page.Height - 80)
                {
                    page = doc.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margen;
                }
                var brochaFila = m.EsJefe
                    ? new XSolidBrush(XColor.FromArgb(255, 243, 224))
                    : (alt ? brochaGris : new XSolidBrush(XColor.FromArgb(255, 255, 255)));
                gfx.DrawRectangle(brochaFila, margen, y, ancho, 14);
                gfx.DrawRectangle(penBorde, margen, y, ancho, 14);

                xCol = margen;
                string prefijo = m.EsJefe ? "[JEFE] " : "";
                gfx.DrawString(Truncar(prefijo + (m.Nombre ?? "-"), 52), fuenteNorm, brochaTexto,
                    new XRect(xCol + 4, y + 2, cTrab - 4, 10), XStringFormats.TopLeft);
                xCol += cTrab;
                gfx.DrawString(Truncar(m.Rol ?? "-", 22), fuenteNorm, brochaTexto,
                    new XRect(xCol + 4, y + 2, cRol - 4, 10), XStringFormats.TopLeft);
                xCol += cRol;
                gfx.DrawString(Truncar(m.Concepto ?? "-", 38), fuenteNorm, brochaTexto,
                    new XRect(xCol + 4, y + 2, cConcepto - 4, 10), XStringFormats.TopLeft);
                xCol += cConcepto;
                gfx.DrawString(m.Monto.ToString("C2", cul), fuenteNorm, brochaTexto,
                    new XRect(xCol + 4, y + 2, cMonto - 8, 10), XStringFormats.TopRight);

                y += 14;
                asignado += m.Monto;
                alt = !alt;
            }

            gfx.DrawRectangle(brochaAzul, margen, y, ancho, 16);
            gfx.DrawString("TOTAL ASIGNADO", fuenteEnc, brochaBlanca,
                new XRect(margen + 4, y + 3, ancho - cMonto - 8, 12), XStringFormats.TopLeft);
            gfx.DrawString(asignado.ToString("C2", cul), fuenteEnc, brochaBlanca,
                new XRect(margen + ancho - cMonto + 4, y + 3, cMonto - 8, 12), XStringFormats.TopRight);

            // Pie
            double yPie = page.Height - margen - 10;
            gfx.DrawLine(new XPen(XColor.FromArgb(13, 71, 161), 1.5),
                margen, yPie, margen + ancho, yPie);
            string usuario = Global.UsuarioActual?.Nombre ?? Environment.UserName;
            gfx.DrawString(string.Format("Generado por: {0} · {1:dd/MM/yyyy HH:mm}",
                usuario, DateTime.Now),
                fuentePeq, brochaTexto,
                new XRect(margen, yPie + 2, ancho / 2, 10), XStringFormats.TopLeft);
            gfx.DrawString("© Calandria Residencial",
                fuentePeq, brochaTexto,
                new XRect(margen + ancho / 2, yPie + 2, ancho / 2, 10), XStringFormats.TopRight);
        }

        private void DibujarPaginaRecibo(PdfDocument doc, MiembroRecibo m)
        {
            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            var gfx = XGraphics.FromPdfPage(page);

            var fuenteTitulo = new XFont("Times New Roman", 16, XFontStyle.Bold);
            var fuenteCuerpo = new XFont("Times New Roman", 12, XFontStyle.Regular);
            var fuenteCuerpoNeg = new XFont("Times New Roman", 12, XFontStyle.Bold);
            var fuentePie = new XFont("Times New Roman", 10, XFontStyle.Regular);

            var brochaTexto = new XSolidBrush(XColor.FromArgb(20, 20, 20));
            var penFirma = new XPen(XColor.FromArgb(80, 80, 80), 0.8);

            double margen = 60;
            double ancho = page.Width - 2 * margen;
            double y = margen;

            // Logo discreto arriba derecha
            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(
                        ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var logo = XImage.FromStream(ms);
                    gfx.DrawImage(logo, margen + ancho - 60, y, 60, 60);
                }
            }
            catch { }

            string fechaTxt = string.Format(CultureInfo.GetCultureInfo("es-MX"),
                "Hermosillo, Sonora. {0:dd} de {0:MMMM} de {0:yyyy}", DateTime.Today);
            gfx.DrawString(fechaTxt, fuenteCuerpo, brochaTexto,
                new XRect(margen, y, ancho - 70, 18), XStringFormats.TopLeft);
            y += 36;

            gfx.DrawString("Recibo de Nómina", fuenteTitulo, brochaTexto,
                new XRect(margen, y, ancho, 22), XStringFormats.TopCenter);
            y += 48;

            string monto = m.Monto.ToString("N2", CultureInfo.GetCultureInfo("es-MX"));
            string letras = NumeroALetras.Convertir(m.Monto, "PESOS", "M.N.");
            string concepto = (m.Concepto ?? "").Trim();

            string cuerpo = string.Format(
                "Yo, {0}, declaro recibir la suma en efectivo de ${1} ({2}) por servicios prestados " +
                "por concepto de {3}, de la empresa CAMANEY DESARROLLO Y CONSTRUCCIÓN SA DE CV.",
                (m.Nombre ?? "").Trim(), monto, letras, concepto);

            y = DibujarParrafo(gfx, cuerpo, fuenteCuerpo, brochaTexto,
                margen, y, ancho, 18);

            // Periodo / cuadrilla (info auxiliar)
            y += 22;
            gfx.DrawString(string.Format(
                "Cuadrilla: {0}     Periodo: {1:dd/MM/yyyy} al {2:dd/MM/yyyy}",
                _codigoCuadrilla, _desde, _hasta),
                fuentePie, brochaTexto,
                new XRect(margen, y, ancho, 14), XStringFormats.TopLeft);

            // Firma
            y = page.Height - margen - 90;
            double anchoFirma = 260;
            double xFirma = margen + (ancho - anchoFirma) / 2;
            gfx.DrawLine(penFirma, xFirma, y, xFirma + anchoFirma, y);
            y += 6;
            gfx.DrawString((m.Nombre ?? "").Trim(), fuenteCuerpoNeg, brochaTexto,
                new XRect(margen, y, ancho, 18), XStringFormats.TopCenter);
            y += 18;
            if (!string.IsNullOrEmpty(m.Rol))
            {
                gfx.DrawString(m.Rol, fuentePie, brochaTexto,
                    new XRect(margen, y, ancho, 14), XStringFormats.TopCenter);
            }
        }

        private static double DibujarParrafo(
            XGraphics gfx, string texto, XFont fuente, XBrush brocha,
            double x, double y, double ancho, double interlineado)
        {
            if (string.IsNullOrEmpty(texto)) return y;
            var palabras = texto.Split(' ');
            var lineaActual = new StringBuilder();
            double yActual = y;

            foreach (var palabra in palabras)
            {
                string prueba = lineaActual.Length == 0
                    ? palabra
                    : lineaActual + " " + palabra;
                var size = gfx.MeasureString(prueba, fuente);
                if (size.Width > ancho && lineaActual.Length > 0)
                {
                    gfx.DrawString(lineaActual.ToString(), fuente, brocha,
                        new XRect(x, yActual, ancho, interlineado), XStringFormats.TopLeft);
                    yActual += interlineado;
                    lineaActual.Clear();
                    lineaActual.Append(palabra);
                }
                else
                {
                    lineaActual.Length = 0;
                    lineaActual.Append(prueba);
                }
            }
            if (lineaActual.Length > 0)
            {
                gfx.DrawString(lineaActual.ToString(), fuente, brocha,
                    new XRect(x, yActual, ancho, interlineado), XStringFormats.TopLeft);
                yActual += interlineado;
            }
            return yActual;
        }

        private static string Truncar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }

        #endregion

        #region Modelos

        public class DestajoTerminado
        {
            public string Cuadrilla { get; set; }
            public string Manzana { get; set; }
            public string Lote { get; set; }
            public string Casa { get; set; }
            public string Nombre { get; set; }
            public string Categoria { get; set; }
            public DateTime? FechaFinalizacion { get; set; }
            public decimal Importe { get; set; }
            public string Ruta { get; set; }
            /// <summary>ID del nodo en RutaTuneraDestajo/RutaCalandraDestajo. Necesario
            /// para marcar la nómina como distribuida tras generar los recibos.</summary>
            public int NodoID { get; set; }
            public bool NominaDistribuida { get; set; }
            public DateTime? FechaDistribucionNomina { get; set; }
        }

        private class MiembroRecibo
        {
            public int? IdTrabajador { get; set; }
            public string Nombre { get; set; }
            public string Rol { get; set; }
            public bool EsJefe { get; set; }
            public string Telefono { get; set; }
            public decimal Monto { get; set; }
            public string Concepto { get; set; }
        }

        #endregion
    }
}
