using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Gestor de cuadrillas integrado con la tabla TRABAJADORES:
    /// los miembros se eligen de los trabajadores ya registrados y, si no existe,
    /// se abre el formulario de registro de trabajador.
    /// </summary>
    public partial class FormAsignarCuadrilla : Form
    {
        public PanelPrincipal.Cuadrilla CuadrillaAsignada { get; private set; }

        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private CuadrillaVisual _cuadrillaActual = new CuadrillaVisual();

        // ====== Paleta Calandria ======
        private static readonly Color ColorCafeOscuro = Color.FromArgb(88, 53, 23);
        private static readonly Color ColorCafe       = Color.FromArgb(179, 108, 46);
        private static readonly Color ColorCremaClara = Color.FromArgb(250, 246, 240);
        private static readonly Color ColorCremaFondo = Color.FromArgb(245, 240, 232);
        private static readonly Color ColorBorde      = Color.FromArgb(220, 210, 195);
        private static readonly Color ColorTextoSuave = Color.FromArgb(117, 117, 117);
        private static readonly Color ColorTextoFuerte = Color.FromArgb(60, 36, 15);
        private static readonly Color ColorVerdePrimario = Color.FromArgb(39, 174, 96);
        private static readonly Color ColorAzul       = Color.FromArgb(52, 152, 219);
        private static readonly Color ColorRojo       = Color.FromArgb(192, 57, 43);
        private static readonly Color ColorGris       = Color.FromArgb(127, 140, 141);

        // ====== Controles ======
        private Panel panelHeader;
        private Label lblTitulo;
        private Label lblSubtitulo;

        private Panel panelToolbar;
        private Label lblBuscar;
        private ComboBox cmbBuscarCuadrilla;
        private Button btnNuevaCuadrilla;

        private Label lblMatricula;
        private TextBox txtCodigoCuadrilla;

        private Panel panelFooter;

        private GroupBox grpAgregarMiembro;
        private Label lblTrabajador;
        private ComboBox cmbTrabajador;
        private Button btnRegistrarTrabajador;

        private GroupBox grpDatosTrabajador;
        private Label lblClaveVal;
        private Label lblNombreVal;
        private Label lblNacionalidadVal;
        private Label lblFechaNacVal;
        private Label lblCurpVal;
        private Label lblRfcVal;
        private Label lblIneVal;
        private Label lblNssVal;

        private Label lblTelefono;
        private TextBox txtTelefono;
        private Label lblRol;
        private ComboBox cmbRol;
        private CheckBox chkEsJefe;

        private Button btnAgregarMiembro;
        private Button btnActualizarMiembro;

        private GroupBox grpMiembros;
        private TreeListView treeCuadrillas;
        private Button btnEliminarMiembro;

        private Button btnGuardarCuadrilla;
        private Button btnAsignarCuadrilla;
        private Button btnCerrar;

        public FormAsignarCuadrilla() : this("DestajoX") { }

        public FormAsignarCuadrilla(string destajo)
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            ConfigurarTreeListView();

            EnsureColumnaIdTrabajador();
            CargarCodigosExistentes();
            CargarTrabajadoresDisponibles();

            cmbBuscarCuadrilla.SelectedIndexChanged += CmbBuscarCuadrilla_SelectedIndexChanged;
            cmbTrabajador.SelectedIndexChanged += CmbTrabajador_SelectedIndexChanged;
            treeCuadrillas.SelectedIndexChanged += TreeCuadrillas_SelectedIndexChanged;

            LimpiarFormularioMiembro();
            ActualizarBotonesMiembro(modoEdicion: false);
        }

        #region Construcción de UI

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Gestión de Cuadrillas";
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = ColorCremaClara;
            this.ClientSize = new Size(1020, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // ===== Header (banda café Calandria) =====
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = ColorCafeOscuro
            };
            lblTitulo = new Label
            {
                Text = "GESTIÓN DE CUADRILLAS",
                Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 8),
                AutoSize = true
            };
            lblSubtitulo = new Label
            {
                Text = "Crea cuadrillas, gestiona sus miembros y asígnalas a un destajo",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(230, 215, 195),
                Location = new Point(22, 36),
                AutoSize = true
            };
            panelHeader.Controls.AddRange(new Control[] { lblSubtitulo, lblTitulo });

            // ===== Toolbar (búsqueda + matrícula) =====
            panelToolbar = new Panel
            {
                Location = new Point(0, 62),
                Size = new Size(1020, 64),
                BackColor = ColorCremaFondo
            };
            panelToolbar.Paint += (s, e) =>
            {
                using (var pen = new Pen(ColorBorde))
                {
                    e.Graphics.DrawLine(pen, 0, panelToolbar.Height - 1,
                        panelToolbar.Width, panelToolbar.Height - 1);
                }
            };

            lblMatricula = new Label
            {
                Text = "MATRÍCULA",
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = ColorTextoSuave,
                Location = new Point(20, 8),
                AutoSize = true
            };
            txtCodigoCuadrilla = new TextBox
            {
                Location = new Point(20, 28),
                Size = new Size(220, 24),
                ReadOnly = true,
                BackColor = Color.White,
                ForeColor = ColorTextoFuerte,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10.5F, FontStyle.Bold),
                Text = "(se generará al guardar)"
            };

            lblBuscar = new Label
            {
                Text = "BUSCAR CUADRILLA",
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = ColorTextoSuave,
                Location = new Point(620, 8),
                AutoSize = true
            };
            cmbBuscarCuadrilla = new ComboBox
            {
                Location = new Point(620, 28),
                Size = new Size(240, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.75F),
                FlatStyle = FlatStyle.Flat
            };
            btnNuevaCuadrilla = new Button
            {
                Text = "+ NUEVA",
                Location = new Point(870, 27),
                Size = new Size(110, 28),
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold)
            };
            EstilizarBoton(btnNuevaCuadrilla, ColorCafe);
            btnNuevaCuadrilla.Click += (s, e) => NuevaCuadrilla();

            panelToolbar.Controls.AddRange(new Control[]
            {
                lblMatricula, txtCodigoCuadrilla,
                lblBuscar, cmbBuscarCuadrilla, btnNuevaCuadrilla
            });

            // ===== Grupo: Agregar Miembro =====
            grpAgregarMiembro = new GroupBox
            {
                Text = "  Datos del miembro  ",
                Location = new Point(15, 135),
                Size = new Size(440, 510),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = ColorTextoFuerte,
                BackColor = Color.White
            };

            lblTrabajador = new Label
            {
                Text = "TRABAJADOR",
                Location = new Point(14, 30),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = ColorTextoSuave
            };
            cmbTrabajador = new ComboBox
            {
                Location = new Point(14, 50),
                Size = new Size(300, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.75F),
                FlatStyle = FlatStyle.Flat
            };
            btnRegistrarTrabajador = new Button
            {
                Text = "+ REGISTRAR",
                Location = new Point(320, 48),
                Size = new Size(108, 28),
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold)
            };
            EstilizarBoton(btnRegistrarTrabajador, ColorVerdePrimario);
            btnRegistrarTrabajador.Click += (s, e) => AbrirRegistroTrabajador();

            grpDatosTrabajador = new GroupBox
            {
                Text = "  Información del trabajador  ",
                Location = new Point(14, 86),
                Size = new Size(414, 200),
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = ColorTextoSuave,
                BackColor = ColorCremaClara
            };

            int yInfo = 22;
            int xLabel = 12, xValor = 110;
            grpDatosTrabajador.Controls.Add(EtiquetaInfo("Clave:", xLabel, yInfo));
            grpDatosTrabajador.Controls.Add(lblClaveVal = ValorInfo(xValor, yInfo));

            yInfo += 22;
            grpDatosTrabajador.Controls.Add(EtiquetaInfo("Nombre:", xLabel, yInfo));
            grpDatosTrabajador.Controls.Add(lblNombreVal = ValorInfo(xValor, yInfo, true));

            yInfo += 22;
            grpDatosTrabajador.Controls.Add(EtiquetaInfo("Nacionalidad:", xLabel, yInfo));
            grpDatosTrabajador.Controls.Add(lblNacionalidadVal = ValorInfo(xValor, yInfo));

            yInfo += 22;
            grpDatosTrabajador.Controls.Add(EtiquetaInfo("F. Nacimiento:", xLabel, yInfo));
            grpDatosTrabajador.Controls.Add(lblFechaNacVal = ValorInfo(xValor, yInfo));

            yInfo += 22;
            grpDatosTrabajador.Controls.Add(EtiquetaInfo("CURP:", xLabel, yInfo));
            grpDatosTrabajador.Controls.Add(lblCurpVal = ValorInfo(xValor, yInfo));

            yInfo += 22;
            grpDatosTrabajador.Controls.Add(EtiquetaInfo("RFC:", xLabel, yInfo));
            grpDatosTrabajador.Controls.Add(lblRfcVal = ValorInfo(xValor, yInfo));

            yInfo += 22;
            grpDatosTrabajador.Controls.Add(EtiquetaInfo("INE:", xLabel, yInfo));
            grpDatosTrabajador.Controls.Add(lblIneVal = ValorInfo(xValor, yInfo));

            yInfo += 22;
            grpDatosTrabajador.Controls.Add(EtiquetaInfo("NSS:", xLabel, yInfo));
            grpDatosTrabajador.Controls.Add(lblNssVal = ValorInfo(xValor, yInfo));

            // Datos por-membresía
            lblTelefono = new Label
            {
                Text = "TELÉFONO (OPCIONAL)",
                Location = new Point(14, 300),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = ColorTextoSuave
            };
            txtTelefono = new TextBox
            {
                Location = new Point(14, 320),
                Size = new Size(200, 24),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.75F)
            };

            lblRol = new Label
            {
                Text = "ROL",
                Location = new Point(230, 300),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = ColorTextoSuave
            };
            cmbRol = new ComboBox
            {
                Location = new Point(230, 320),
                Size = new Size(180, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.75F),
                FlatStyle = FlatStyle.Flat
            };
            cmbRol.Items.AddRange(new object[] { "PEON", "AYUDANTE", "OFICIAL" });

            // "Es jefe" como badge sutil con marco dorado
            chkEsJefe = new CheckBox
            {
                Text = "  Marcar como JEFE de la cuadrilla",
                Location = new Point(14, 365),
                Size = new Size(396, 38),
                Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold),
                ForeColor = ColorCafeOscuro,
                BackColor = Color.FromArgb(252, 245, 230),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            chkEsJefe.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 195, 150)))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, chkEsJefe.Width - 1, chkEsJefe.Height - 1);
                }
            };

            btnAgregarMiembro = new Button
            {
                Text = "+ AGREGAR A CUADRILLA",
                Location = new Point(14, 420),
                Size = new Size(396, 44),
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            };
            EstilizarBoton(btnAgregarMiembro, ColorVerdePrimario);
            btnAgregarMiembro.Click += BtnAgregarMiembro_Click;

            btnActualizarMiembro = new Button
            {
                Text = "ACTUALIZAR MIEMBRO",
                Location = new Point(14, 420),
                Size = new Size(396, 44),
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                Visible = false
            };
            EstilizarBoton(btnActualizarMiembro, ColorAzul);
            btnActualizarMiembro.Click += BtnActualizarMiembro_Click;

            grpAgregarMiembro.Controls.AddRange(new Control[]
            {
                lblTrabajador, cmbTrabajador, btnRegistrarTrabajador,
                grpDatosTrabajador,
                lblTelefono, txtTelefono,
                lblRol, cmbRol,
                chkEsJefe,
                btnAgregarMiembro, btnActualizarMiembro
            });

            // ===== Grupo: Miembros de la cuadrilla =====
            grpMiembros = new GroupBox
            {
                Text = "  Miembros de la cuadrilla  ",
                Location = new Point(470, 135),
                Size = new Size(535, 510),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = ColorTextoFuerte,
                BackColor = Color.White
            };

            treeCuadrillas = new TreeListView
            {
                Location = new Point(14, 28),
                Size = new Size(507, 430),
                FullRowSelect = true,
                UseAlternatingBackColors = true,
                AlternateRowBackColor = ColorCremaClara,
                HideSelection = false,
                OwnerDraw = true,
                ShowGroups = false,
                UseCompatibleStateImageBehavior = false,
                View = View.Details,
                VirtualMode = true,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            btnEliminarMiembro = new Button
            {
                Text = "✕ ELIMINAR MIEMBRO",
                Location = new Point(14, 466),
                Size = new Size(220, 36),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };
            EstilizarBoton(btnEliminarMiembro, ColorRojo);
            btnEliminarMiembro.Click += BtnEliminarMiembro_Click;

            grpMiembros.Controls.Add(treeCuadrillas);
            grpMiembros.Controls.Add(btnEliminarMiembro);

            // ===== Footer (panel con jerarquía de acciones) =====
            panelFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = ColorCremaFondo
            };
            panelFooter.Paint += (s, e) =>
            {
                using (var pen = new Pen(ColorBorde))
                {
                    e.Graphics.DrawLine(pen, 0, 0, panelFooter.Width, 0);
                }
            };

            btnAsignarCuadrilla = new Button
            {
                Text = "✓ ASIGNAR CUADRILLA",
                Location = new Point(15, 12),
                Size = new Size(240, 42),
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            };
            EstilizarBoton(btnAsignarCuadrilla, ColorVerdePrimario);
            btnAsignarCuadrilla.Click += BtnAsignarCuadrilla_Click;

            btnGuardarCuadrilla = new Button
            {
                Text = "GUARDAR CAMBIOS",
                Location = new Point(265, 12),
                Size = new Size(200, 42),
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            };
            EstilizarBoton(btnGuardarCuadrilla, ColorCafe);
            btnGuardarCuadrilla.Click += BtnGuardarCuadrilla_Click;

            btnCerrar = new Button
            {
                Text = "CERRAR",
                Location = new Point(890, 12),
                Size = new Size(115, 42),
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            };
            EstilizarBoton(btnCerrar, ColorGris);
            btnCerrar.Click += (s, e) => this.Close();

            panelFooter.Controls.AddRange(new Control[]
            {
                btnAsignarCuadrilla, btnGuardarCuadrilla, btnCerrar
            });

            // === Orden de adición: Dock=Top en orden inverso al visual ===
            this.Controls.AddRange(new Control[]
            {
                grpAgregarMiembro, grpMiembros,
                panelFooter,
                panelToolbar,
                panelHeader
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        /// <summary>
        /// Aplica estilo Flat + cursor + fuente consistente a un botón.
        /// </summary>
        private static void EstilizarBoton(Button btn, Color fondo)
        {
            btn.BackColor = fondo;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.UseVisualStyleBackColor = false;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
        }

        private static Label EtiquetaInfo(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(95, 18),
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = ColorTextoSuave
            };
        }

        private static Label ValorInfo(int x, int y, bool ancho = false)
        {
            return new Label
            {
                Text = "—",
                Location = new Point(x, y),
                Size = new Size(ancho ? 290 : 200, 18),
                Font = new Font("Segoe UI", 9F),
                ForeColor = ColorTextoFuerte,
                AutoEllipsis = true
            };
        }

        private void ConfigurarTreeListView()
        {
            treeCuadrillas.CanExpandGetter = x => (x as CuadrillaVisual)?.Miembros?.Count > 0;
            treeCuadrillas.ChildrenGetter = x => (x as CuadrillaVisual)?.Miembros;

            treeCuadrillas.Columns.Clear();

            treeCuadrillas.Columns.Add(new OLVColumn("Matrícula", "")
            {
                AspectGetter = row => row is CuadrillaVisual cv ? cv.CodigoCuadrilla : "",
                Width = 110
            });
            treeCuadrillas.Columns.Add(new OLVColumn("Nombre", "Nombre") { Width = 200 });
            treeCuadrillas.Columns.Add(new OLVColumn("Rol", "Rol") { Width = 90 });
            treeCuadrillas.Columns.Add(new OLVColumn("Jefe", "")
            {
                AspectGetter = row => row is MiembroCuadrilla m && m.EsJefe ? "★" : "",
                Width = 50,
                TextAlign = HorizontalAlignment.Center
            });
            treeCuadrillas.Columns.Add(new OLVColumn("Teléfono", "Telefono") { Width = 110 });

            // Header café Calandria (consistente con FormAdministrativos)
            var headerStyle = new HeaderFormatStyle();
            headerStyle.Normal.BackColor = ColorCafeOscuro;
            headerStyle.Normal.ForeColor = Color.White;
            headerStyle.Normal.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            headerStyle.Hot.BackColor = ColorCafe;
            headerStyle.Hot.ForeColor = Color.White;
            treeCuadrillas.HeaderFormatStyle = headerStyle;
            treeCuadrillas.HeaderUsesThemes = false;
            treeCuadrillas.RowHeight = 26;

            // Resalta visualmente al jefe de cuadrilla
            treeCuadrillas.FormatRow += (s, e) =>
            {
                if (e.Model is MiembroCuadrilla m && m.EsJefe)
                {
                    e.Item.BackColor = Color.FromArgb(252, 245, 230);
                    e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                }
            };

            treeCuadrillas.SetObjects(new[] { _cuadrillaActual });
        }

        #endregion

        #region Carga de datos

        private void EnsureColumnaIdTrabajador()
        {
            // Garantiza la existencia de la tabla MiembrosCuadrilla y de la columna IdTrabajador.
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.MiembrosCuadrilla') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.MiembrosCuadrilla (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CodigoCuadrilla NVARCHAR(20) NOT NULL,
        IdTrabajador INT NULL,
        Nombre NVARCHAR(200) NOT NULL,
        Rol NVARCHAR(50) NOT NULL,
        EsJefe BIT NOT NULL DEFAULT(0),
        Telefono NVARCHAR(50) NULL,
        Foto VARBINARY(MAX) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
    );
END
ELSE IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IdTrabajador' AND Object_ID = Object_ID(N'dbo.MiembrosCuadrilla'))
BEGIN
    ALTER TABLE dbo.MiembrosCuadrilla ADD IdTrabajador INT NULL;
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
            catch
            {
                // Errores no fatales: los siguientes intentos lanzarán mensaje al usuario.
            }
        }

        private void CargarTrabajadoresDisponibles()
        {
            cmbTrabajador.Items.Clear();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TRABAJADORES') AND type IN (N'U'))
                            SELECT IdTrabajador, ClaveTrabajador, Nombre, Nacionalidad,
                                   FechaNacimiento, CURP, RFC, INE, NSS
                            FROM TRABAJADORES
                            ORDER BY Nombre", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbTrabajador.Items.Add(new TrabajadorOpcion
                            {
                                IdTrabajador = Convert.ToInt32(reader["IdTrabajador"]),
                                Clave = reader["ClaveTrabajador"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Nacionalidad = reader["Nacionalidad"].ToString(),
                                FechaNacimiento = reader["FechaNacimiento"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(reader["FechaNacimiento"]),
                                CURP = reader["CURP"] == DBNull.Value ? "" : reader["CURP"].ToString(),
                                RFC = reader["RFC"] == DBNull.Value ? "" : reader["RFC"].ToString(),
                                INE = reader["INE"] == DBNull.Value ? "" : reader["INE"].ToString(),
                                NSS = reader["NSS"] == DBNull.Value ? "" : reader["NSS"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar la lista de trabajadores:\n" + ex.Message,
                    "Trabajadores", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CargarCodigosExistentes()
        {
            cmbBuscarCuadrilla.Items.Clear();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT DISTINCT CodigoCuadrilla FROM MiembrosCuadrilla ORDER BY CodigoCuadrilla", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            cmbBuscarCuadrilla.Items.Add(reader.GetString(0));
                    }
                }
            }
            catch
            {
                // Silenciado: la tabla puede no existir aún en la primera ejecución.
            }
        }

        #endregion

        #region Eventos – selección de combos / árbol

        private void CmbBuscarCuadrilla_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBuscarCuadrilla.SelectedItem == null) return;
            CargarCuadrillaDesdeBD(cmbBuscarCuadrilla.SelectedItem.ToString());
        }

        private void CmbTrabajador_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = cmbTrabajador.SelectedItem as TrabajadorOpcion;
            if (t == null)
            {
                LimpiarPanelInfoTrabajador();
                return;
            }

            lblClaveVal.Text = t.Clave;
            lblNombreVal.Text = t.Nombre;
            lblNacionalidadVal.Text = string.IsNullOrEmpty(t.Nacionalidad) ? "—" : t.Nacionalidad;
            lblFechaNacVal.Text = t.FechaNacimiento.HasValue ? t.FechaNacimiento.Value.ToString("dd/MM/yyyy") : "—";
            lblCurpVal.Text = string.IsNullOrEmpty(t.CURP) ? "—" : t.CURP;
            lblRfcVal.Text = string.IsNullOrEmpty(t.RFC) ? "—" : t.RFC;
            lblIneVal.Text = string.IsNullOrEmpty(t.INE) ? "—" : t.INE;
            lblNssVal.Text = string.IsNullOrEmpty(t.NSS) ? "—" : t.NSS;
        }

        private void TreeCuadrillas_SelectedIndexChanged(object sender, EventArgs e)
        {
            var miembro = treeCuadrillas.SelectedObject as MiembroCuadrilla;
            if (miembro == null)
            {
                ActualizarBotonesMiembro(modoEdicion: false);
                return;
            }

            // Selecciona el trabajador correspondiente en el combo
            TrabajadorOpcion match = null;
            foreach (var item in cmbTrabajador.Items)
            {
                if (item is TrabajadorOpcion t &&
                    miembro.IdTrabajador.HasValue &&
                    t.IdTrabajador == miembro.IdTrabajador.Value)
                {
                    match = t;
                    break;
                }
            }
            cmbTrabajador.SelectedItem = match;

            txtTelefono.Text = miembro.Telefono ?? "";
            cmbRol.SelectedItem = miembro.Rol;
            chkEsJefe.Checked = miembro.EsJefe;

            ActualizarBotonesMiembro(modoEdicion: true);
        }

        private void LimpiarPanelInfoTrabajador()
        {
            lblClaveVal.Text = "—";
            lblNombreVal.Text = "—";
            lblNacionalidadVal.Text = "—";
            lblFechaNacVal.Text = "—";
            lblCurpVal.Text = "—";
            lblRfcVal.Text = "—";
            lblIneVal.Text = "—";
            lblNssVal.Text = "—";
        }

        private void LimpiarFormularioMiembro()
        {
            cmbTrabajador.SelectedIndex = -1;
            txtTelefono.Text = "";
            cmbRol.SelectedIndex = -1;
            chkEsJefe.Checked = false;
            LimpiarPanelInfoTrabajador();
        }

        private void ActualizarBotonesMiembro(bool modoEdicion)
        {
            btnAgregarMiembro.Visible = !modoEdicion;
            btnActualizarMiembro.Visible = modoEdicion;
        }

        #endregion

        #region Acciones – cuadrilla

        public void CargarCuadrillaDesdeBD(string codigo)
        {
            _cuadrillaActual = new CuadrillaVisual { CodigoCuadrilla = codigo };

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT m.IdTrabajador, m.Nombre, m.Rol, m.EsJefe, m.Telefono,
                               t.ClaveTrabajador
                        FROM MiembrosCuadrilla m
                        LEFT JOIN TRABAJADORES t ON t.IdTrabajador = m.IdTrabajador
                        WHERE m.CodigoCuadrilla = @codigo
                        ORDER BY m.EsJefe DESC, m.Nombre", conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigo);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _cuadrillaActual.Miembros.Add(new MiembroCuadrilla
                                {
                                    IdTrabajador = reader["IdTrabajador"] == DBNull.Value
                                        ? (int?)null
                                        : Convert.ToInt32(reader["IdTrabajador"]),
                                    Nombre = reader["Nombre"].ToString(),
                                    Rol = reader["Rol"].ToString(),
                                    EsJefe = Convert.ToBoolean(reader["EsJefe"]),
                                    Telefono = reader["Telefono"] == DBNull.Value ? "" : reader["Telefono"].ToString(),
                                    Clave = reader["ClaveTrabajador"] == DBNull.Value ? "" : reader["ClaveTrabajador"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la cuadrilla:\n" + ex.Message,
                    "Cuadrilla", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            txtCodigoCuadrilla.Text = codigo;
            treeCuadrillas.SetObjects(new[] { _cuadrillaActual });
            treeCuadrillas.ExpandAll();
            LimpiarFormularioMiembro();
            ActualizarBotonesMiembro(modoEdicion: false);
        }

        public void DesactivarEdicionCuadrilla()
        {
            cmbTrabajador.Enabled = false;
            txtTelefono.Enabled = false;
            cmbRol.Enabled = false;
            chkEsJefe.Enabled = false;
            btnAgregarMiembro.Enabled = false;
            btnActualizarMiembro.Enabled = false;
            btnEliminarMiembro.Enabled = false;
            btnGuardarCuadrilla.Enabled = false;
            btnRegistrarTrabajador.Enabled = false;
            btnNuevaCuadrilla.Enabled = false;
            cmbBuscarCuadrilla.Enabled = false;
            txtCodigoCuadrilla.Enabled = false;
        }

        private void NuevaCuadrilla()
        {
            _cuadrillaActual = new CuadrillaVisual();
            treeCuadrillas.SetObjects(new[] { _cuadrillaActual });
            txtCodigoCuadrilla.Text = "(se generará al guardar)";
            cmbBuscarCuadrilla.SelectedIndex = -1;
            LimpiarFormularioMiembro();
            ActualizarBotonesMiembro(modoEdicion: false);
        }

        private void AbrirRegistroTrabajador()
        {
            int? idAntesSeleccion = (cmbTrabajador.SelectedItem as TrabajadorOpcion)?.IdTrabajador;

            using (var form = new FormRegistrarTrabajador())
            {
                form.ShowDialog(this);
            }

            CargarTrabajadoresDisponibles();

            // Reposiciona la selección si seguimos teniendo el mismo trabajador
            if (idAntesSeleccion.HasValue)
            {
                foreach (var item in cmbTrabajador.Items)
                {
                    if (item is TrabajadorOpcion t && t.IdTrabajador == idAntesSeleccion.Value)
                    {
                        cmbTrabajador.SelectedItem = t;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Acciones – miembros

        private bool ValidarFormularioMiembro(out TrabajadorOpcion trabajador)
        {
            trabajador = cmbTrabajador.SelectedItem as TrabajadorOpcion;
            if (trabajador == null)
            {
                MessageBox.Show("Selecciona un trabajador.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbRol.SelectedIndex < 0)
            {
                MessageBox.Show("Selecciona un rol.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void BtnAgregarMiembro_Click(object sender, EventArgs e)
        {
            if (!ValidarFormularioMiembro(out TrabajadorOpcion trabajador)) return;

            if (chkEsJefe.Checked && _cuadrillaActual.Miembros.Any(m => m.EsJefe))
            {
                MessageBox.Show("Ya hay un jefe asignado en esta cuadrilla.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_cuadrillaActual.Miembros.Any(m => m.IdTrabajador == trabajador.IdTrabajador))
            {
                MessageBox.Show("Ese trabajador ya pertenece a la cuadrilla.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _cuadrillaActual.Miembros.Add(new MiembroCuadrilla
            {
                IdTrabajador = trabajador.IdTrabajador,
                Clave = trabajador.Clave,
                Nombre = trabajador.Nombre,
                Rol = cmbRol.SelectedItem.ToString(),
                EsJefe = chkEsJefe.Checked,
                Telefono = txtTelefono.Text.Trim()
            });

            treeCuadrillas.SetObjects(new[] { _cuadrillaActual });
            treeCuadrillas.ExpandAll();
            LimpiarFormularioMiembro();
        }

        private void BtnActualizarMiembro_Click(object sender, EventArgs e)
        {
            var seleccionado = treeCuadrillas.SelectedObject as MiembroCuadrilla;
            if (seleccionado == null)
            {
                ActualizarBotonesMiembro(modoEdicion: false);
                return;
            }
            if (!ValidarFormularioMiembro(out TrabajadorOpcion trabajador)) return;

            if (chkEsJefe.Checked && _cuadrillaActual.Miembros.Any(m => m.EsJefe && m != seleccionado))
            {
                MessageBox.Show("Ya hay un jefe asignado en esta cuadrilla.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            seleccionado.IdTrabajador = trabajador.IdTrabajador;
            seleccionado.Clave = trabajador.Clave;
            seleccionado.Nombre = trabajador.Nombre;
            seleccionado.Rol = cmbRol.SelectedItem.ToString();
            seleccionado.EsJefe = chkEsJefe.Checked;
            seleccionado.Telefono = txtTelefono.Text.Trim();

            treeCuadrillas.SetObjects(new[] { _cuadrillaActual });
            treeCuadrillas.ExpandAll();
            LimpiarFormularioMiembro();
            ActualizarBotonesMiembro(modoEdicion: false);
        }

        private void BtnEliminarMiembro_Click(object sender, EventArgs e)
        {
            var seleccionado = treeCuadrillas.SelectedObject as MiembroCuadrilla;
            if (seleccionado == null)
            {
                MessageBox.Show("Selecciona un miembro de la lista.", "Eliminar",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _cuadrillaActual.Miembros.Remove(seleccionado);
            treeCuadrillas.SetObjects(new[] { _cuadrillaActual });
            treeCuadrillas.ExpandAll();
            LimpiarFormularioMiembro();
            ActualizarBotonesMiembro(modoEdicion: false);
        }

        #endregion

        #region Guardar / Asignar

        private void BtnGuardarCuadrilla_Click(object sender, EventArgs e)
        {
            if (_cuadrillaActual.Miembros.Count == 0)
            {
                MessageBox.Show("Agrega al menos un miembro.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!_cuadrillaActual.Miembros.Any(m => m.EsJefe))
            {
                MessageBox.Show("Debe haber un miembro marcado como jefe de cuadrilla.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    bool esNueva = string.IsNullOrEmpty(_cuadrillaActual.CodigoCuadrilla);
                    string codigoFinal = esNueva
                        ? GenerarCodigoCuadrillaUnico(conn)
                        : _cuadrillaActual.CodigoCuadrilla;

                    using (var tx = conn.BeginTransaction())
                    {
                        // Si la cuadrilla ya existía, limpiamos sus miembros previos.
                        if (!esNueva)
                        {
                            using (var cmdDel = new SqlCommand(
                                "DELETE FROM MiembrosCuadrilla WHERE CodigoCuadrilla = @codigo", conn, tx))
                            {
                                cmdDel.Parameters.AddWithValue("@codigo", codigoFinal);
                                cmdDel.ExecuteNonQuery();
                            }
                        }

                        foreach (var m in _cuadrillaActual.Miembros)
                        {
                            using (var cmdIns = new SqlCommand(@"
                                INSERT INTO MiembrosCuadrilla
                                (CodigoCuadrilla, IdTrabajador, Nombre, Rol, EsJefe, Telefono, Foto)
                                VALUES (@codigo, @idTrab, @nombre, @rol, @esJefe, @tel, NULL)", conn, tx))
                            {
                                cmdIns.Parameters.AddWithValue("@codigo", codigoFinal);
                                cmdIns.Parameters.AddWithValue("@idTrab", (object)m.IdTrabajador ?? DBNull.Value);
                                cmdIns.Parameters.AddWithValue("@nombre", m.Nombre ?? "");
                                cmdIns.Parameters.AddWithValue("@rol", m.Rol ?? "");
                                cmdIns.Parameters.AddWithValue("@esJefe", m.EsJefe);
                                cmdIns.Parameters.AddWithValue("@tel",
                                    string.IsNullOrWhiteSpace(m.Telefono) ? (object)DBNull.Value : m.Telefono);
                                cmdIns.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }

                    _cuadrillaActual.CodigoCuadrilla = codigoFinal;
                    txtCodigoCuadrilla.Text = codigoFinal;

                    var jefe = _cuadrillaActual.Miembros.FirstOrDefault(m => m.EsJefe);
                    CuadrillaAsignada = new PanelPrincipal.Cuadrilla
                    {
                        CodigoCuadrilla = codigoFinal,
                        Nombre = jefe?.Nombre ?? "",
                        Telefono = jefe?.Telefono ?? "",
                        Rol = jefe?.Rol ?? "",
                        FotoPath = ""
                    };
                }

                CargarCodigosExistentes();
                MessageBox.Show($"Cuadrilla guardada con matrícula: {_cuadrillaActual.CodigoCuadrilla}",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar cuadrilla:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAsignarCuadrilla_Click(object sender, EventArgs e)
        {
            string codigo = cmbBuscarCuadrilla.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(codigo) && !string.IsNullOrEmpty(_cuadrillaActual.CodigoCuadrilla))
                codigo = _cuadrillaActual.CodigoCuadrilla;

            if (string.IsNullOrEmpty(codigo))
            {
                MessageBox.Show("Selecciona una cuadrilla existente o guarda la actual primero.",
                    "Asignar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Asegura que tenemos la versión más actual cargada en memoria.
            if (_cuadrillaActual.CodigoCuadrilla != codigo)
                CargarCuadrillaDesdeBD(codigo);

            var jefe = _cuadrillaActual.Miembros.FirstOrDefault(m => m.EsJefe);
            CuadrillaAsignada = new PanelPrincipal.Cuadrilla
            {
                CodigoCuadrilla = codigo,
                Nombre = jefe?.Nombre ?? "",
                Telefono = jefe?.Telefono ?? "",
                Rol = jefe?.Rol ?? "",
                FotoPath = ""
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string GenerarCodigoCuadrillaUnico(SqlConnection conn)
        {
            var rnd = new Random();
            while (true)
            {
                string codigo = "CD-" + rnd.Next(0, 1000000).ToString("D6");
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM MiembrosCuadrilla WHERE CodigoCuadrilla = @codigo", conn))
                {
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    int count = (int)cmd.ExecuteScalar();
                    if (count == 0) return codigo;
                }
            }
        }

        #endregion

        #region Modelos

        public class MiembroCuadrilla
        {
            public int? IdTrabajador { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Rol { get; set; }
            public bool EsJefe { get; set; }
            public string Telefono { get; set; }
        }

        public class CuadrillaVisual
        {
            public string CodigoCuadrilla { get; set; } = "";
            public List<MiembroCuadrilla> Miembros { get; set; } = new List<MiembroCuadrilla>();
        }

        public class TrabajadorOpcion
        {
            public int IdTrabajador { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Nacionalidad { get; set; }
            public DateTime? FechaNacimiento { get; set; }
            public string CURP { get; set; }
            public string RFC { get; set; }
            public string INE { get; set; }
            public string NSS { get; set; }
            public override string ToString() => $"{Clave} — {Nombre}";
        }

        #endregion
    }
}
