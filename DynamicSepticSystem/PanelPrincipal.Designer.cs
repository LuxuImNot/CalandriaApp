using DynamicSepticSystem.ModernUI;

namespace DynamicSepticSystem
{
    partial class PanelPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (timerZoom != null)
                {
                    timerZoom.Stop();
                    timerZoom.Tick -= TimerZoom_Tick;
                    timerZoom.Dispose();
                    timerZoom = null;
                }
                if (timerPin != null)
                {
                    timerPin.Stop();
                    timerPin.Dispose();
                    timerPin = null;
                }
                if (imagenCacheada != null)
                {
                    imagenCacheada.Dispose();
                    imagenCacheada = null;
                }
                if (imagenMapaCompleto != null)
                {
                    imagenMapaCompleto.Dispose();
                    imagenMapaCompleto = null;
                }
                if (bufferGrafico != null)
                {
                    bufferGrafico.Dispose();
                    bufferGrafico = null;
                }
                if (pictureBoxMapa != null)
                {
                    pictureBoxMapa.Paint -= PictureBoxMapa_Paint;
                    pictureBoxMapa.Click -= PictureBoxMapa_Click;
                    pictureBoxMapa.Resize -= PictureBoxMapa_Resize;
                }
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // ============ INSTANCIAS ============
            this.menuStripGeneral = new System.Windows.Forms.MenuStrip();
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.panelDevTools = new System.Windows.Forms.Panel();
            this.btnDiagnosticoConexion = new System.Windows.Forms.Button();
            this.btnRegistrarUsuario = new System.Windows.Forms.Button();
            this.btnRegistrarTrabajador = new System.Windows.Forms.Button();
            this.btnGestionarCuadrillas = new System.Windows.Forms.Button();
            this.btnCerrarSesion = new System.Windows.Forms.Button();
            this.lblRol = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();

            this.panelContenido = new System.Windows.Forms.Panel();

            // Modern Nav Bar (reemplaza al MenuStrip + header)
            this.modernNavBar = new ModernNavBar();

            // Header banner (DEPRECATED - reservado por backwards-compat, no se agrega al árbol)
            this.panelHeaderHero = new System.Windows.Forms.Panel();
            this.lblHeaderTitulo = new System.Windows.Forms.Label();
            this.lblHeaderSubtitulo = new System.Windows.Forms.Label();
            this.lblFechaActual = new System.Windows.Forms.Label();

            // Búsqueda
            this.cardBusqueda = new ModernCard();
            this.lblManzanaLabel = new System.Windows.Forms.Label();
            this.lblLoteLabel = new System.Windows.Forms.Label();
            this.txtManzanaPanel = new RoundedTextBoxPanel();
            this.txtLotePanel = new RoundedTextBoxPanel();
            this.btnBuscarCasaModern = new PrimaryButton();
            this.txtManzana = this.txtManzanaPanel.Inner;
            this.txtLote = this.txtLotePanel.Inner;
            this.btnBuscarCasa = this.btnBuscarCasaModern;

            // KPI row
            this.kpiAvanceFisico = new KpiCard();
            this.kpiAvanceEconomico = new KpiCard();
            this.kpiPartidasTotal = new KpiCard();
            this.kpiPartidasCompletadas = new KpiCard();

            // Mapa
            this.cardMapa = new ModernCard();
            this.pictureBoxMapa = new System.Windows.Forms.PictureBox();
            this.lblInstruccionesMapa = new System.Windows.Forms.Label();
            this.btnMapaZoomIn = new CircularIconButton();
            this.btnMapaZoomOut = new CircularIconButton();
            this.btnMapaReset = new CircularIconButton();
            this.lblZoomNivel = new System.Windows.Forms.Label();

            // Info casa
            this.cardInfoCasa = new ModernCard();
            this.lblCasaActual = new System.Windows.Forms.Label();
            this.lblManzanaLote = new System.Windows.Forms.Label();
            this.lblPrototipo = new System.Windows.Forms.Label();
            this.pictureBoxCasa = new System.Windows.Forms.PictureBox();
            this.lblFotoEmpty = new System.Windows.Forms.Label();

            // Progreso
            this.cardProgreso = new ModernCard();
            this.lblProgresoGeneral = new System.Windows.Forms.Label();
            this.animProgresoGeneral = new AnimatedProgressBar();
            this.progressBarGeneral = new System.Windows.Forms.ProgressBar(); // mantenido para back-compat (oculto)
            this.lblProgresoGeneralValor = new System.Windows.Forms.Label();
            this.lblProgresoPartidas = new System.Windows.Forms.Label();
            this.animProgresoPartidas = new AnimatedProgressBar();
            this.progressBarPartidas = new System.Windows.Forms.ProgressBar(); // back-compat (oculto)
            this.lblProgresoPartidasValor = new System.Windows.Forms.Label();

            // Estadísticas
            this.cardEstadisticas = new ModernCard();
            this.lblTotalDestajos = new System.Windows.Forms.Label();
            this.lblDestajosCompletados = new System.Windows.Forms.Label();
            this.lblDestajosPendientes = new System.Windows.Forms.Label();
            this.lblUltimaActualizacion = new System.Windows.Forms.Label();

            // ============ Suspend ============
            this.panelSidebar.SuspendLayout();
            this.panelDevTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.panelContenido.SuspendLayout();
            this.cardBusqueda.SuspendLayout();
            this.cardMapa.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMapa)).BeginInit();
            this.cardInfoCasa.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCasa)).BeginInit();
            this.cardProgreso.SuspendLayout();
            this.cardEstadisticas.SuspendLayout();
            this.SuspendLayout();

            // ============ MenuStrip (DEPRECATED, oculto - ya no se usa) ============
            this.menuStripGeneral.AutoSize = false;
            this.menuStripGeneral.BackColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.menuStripGeneral.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.menuStripGeneral.Location = new System.Drawing.Point(0, 0);
            this.menuStripGeneral.Name = "menuStripGeneral";
            this.menuStripGeneral.Size = new System.Drawing.Size(1, 1);
            this.menuStripGeneral.TabIndex = 0;
            this.menuStripGeneral.Text = "menuStripGeneral";
            this.menuStripGeneral.Visible = false;

            // ============ ModernNavBar ============
            this.modernNavBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.modernNavBar.Height = 48;
            this.modernNavBar.Name = "modernNavBar";
            this.modernNavBar.TabIndex = 50;
            this.modernNavBar.AccentColor = System.Drawing.Color.FromArgb(217, 142, 24);

            // ============ Sidebar ============
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(88, 53, 23);
            this.panelSidebar.Controls.Add(this.panelDevTools);
            this.panelSidebar.Controls.Add(this.btnCerrarSesion);
            this.panelSidebar.Controls.Add(this.lblRol);
            this.panelSidebar.Controls.Add(this.lblUser);
            this.panelSidebar.Controls.Add(this.pictureBoxLogo);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Location = new System.Drawing.Point(0, 48);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Size = new System.Drawing.Size(250, 800);
            this.panelSidebar.TabIndex = 1;

            // panelDevTools
            this.panelDevTools.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.panelDevTools.Controls.Add(this.btnDiagnosticoConexion);
            this.panelDevTools.Controls.Add(this.btnRegistrarUsuario);
            this.panelDevTools.Controls.Add(this.btnRegistrarTrabajador);
            this.panelDevTools.Controls.Add(this.btnGestionarCuadrillas);
            this.panelDevTools.Location = new System.Drawing.Point(20, 583);
            this.panelDevTools.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.panelDevTools.Name = "panelDevTools";
            this.panelDevTools.Size = new System.Drawing.Size(210, 190);
            this.panelDevTools.TabIndex = 4;
            this.panelDevTools.Visible = false;

            ConfigSidebarBtn(this.btnDiagnosticoConexion, "🔧 Diagnóstico Conexión",
                System.Drawing.Color.FromArgb(36, 41, 46),
                System.Drawing.Color.FromArgb(20, 25, 30),
                System.Drawing.Color.FromArgb(50, 55, 60), 10);
            this.btnDiagnosticoConexion.Click += new System.EventHandler(this.btnDiagnosticoConexion_Click);

            ConfigSidebarBtn(this.btnRegistrarUsuario, "Registrar Usuario",
                System.Drawing.Color.FromArgb(52, 152, 219),
                System.Drawing.Color.FromArgb(41, 128, 185),
                System.Drawing.Color.FromArgb(70, 160, 220), 54);
            this.btnRegistrarUsuario.Click += new System.EventHandler(this.btnRegistrarUsuario_Click);

            ConfigSidebarBtn(this.btnRegistrarTrabajador, "Registrar Trabajador",
                System.Drawing.Color.FromArgb(46, 204, 113),
                System.Drawing.Color.FromArgb(36, 160, 89),
                System.Drawing.Color.FromArgb(60, 220, 130), 98);
            this.btnRegistrarTrabajador.Click += new System.EventHandler(this.btnRegistrarTrabajador_Click);

            ConfigSidebarBtn(this.btnGestionarCuadrillas, "Gestión de Cuadrillas",
                System.Drawing.Color.FromArgb(155, 89, 182),
                System.Drawing.Color.FromArgb(125, 60, 152),
                System.Drawing.Color.FromArgb(175, 110, 200), 142);
            this.btnGestionarCuadrillas.Click += new System.EventHandler(this.btnGestionarCuadrillas_Click);

            // btnCerrarSesion
            this.btnCerrarSesion.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnCerrarSesion.BackColor = System.Drawing.Color.FromArgb(231, 76, 60);
            this.btnCerrarSesion.FlatAppearance.BorderSize = 0;
            this.btnCerrarSesion.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this.btnCerrarSesion.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(235, 87, 87);
            this.btnCerrarSesion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrarSesion.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrarSesion.ForeColor = System.Drawing.Color.White;
            this.btnCerrarSesion.Location = new System.Drawing.Point(20, 780);
            this.btnCerrarSesion.Name = "btnCerrarSesion";
            this.btnCerrarSesion.Size = new System.Drawing.Size(210, 45);
            this.btnCerrarSesion.TabIndex = 3;
            this.btnCerrarSesion.Text = "Cerrar Sesión";
            this.btnCerrarSesion.UseVisualStyleBackColor = false;
            this.btnCerrarSesion.Click += new System.EventHandler(this.btnCerrarSesion_Click);

            // lblRol
            this.lblRol.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRol.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.lblRol.Location = new System.Drawing.Point(15, 265);
            this.lblRol.Name = "lblRol";
            this.lblRol.Size = new System.Drawing.Size(220, 60);
            this.lblRol.TabIndex = 2;
            this.lblRol.Text = "Sin permisos activos";

            // lblUser
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblUser.ForeColor = System.Drawing.Color.White;
            this.lblUser.Location = new System.Drawing.Point(15, 235);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(220, 25);
            this.lblUser.TabIndex = 1;
            this.lblUser.Text = "▶ Sin usuario";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // pictureBoxLogo
            this.pictureBoxLogo.BackColor = System.Drawing.Color.White;
            this.pictureBoxLogo.Location = new System.Drawing.Point(25, 20);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(200, 200);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxLogo.TabIndex = 0;
            this.pictureBoxLogo.TabStop = false;

            // ============ panelContenido ============
            this.panelContenido.AutoScroll = true;
            this.panelContenido.BackColor = System.Drawing.Color.FromArgb(244, 241, 235);
            this.panelContenido.Controls.Add(this.cardBusqueda);
            this.panelContenido.Controls.Add(this.kpiAvanceFisico);
            this.panelContenido.Controls.Add(this.kpiAvanceEconomico);
            this.panelContenido.Controls.Add(this.kpiPartidasTotal);
            this.panelContenido.Controls.Add(this.kpiPartidasCompletadas);
            this.panelContenido.Controls.Add(this.cardMapa);
            this.panelContenido.Controls.Add(this.cardInfoCasa);
            this.panelContenido.Controls.Add(this.cardProgreso);
            this.panelContenido.Controls.Add(this.cardEstadisticas);
            this.panelContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContenido.Location = new System.Drawing.Point(250, 48);
            this.panelContenido.Name = "panelContenido";
            this.panelContenido.Padding = new System.Windows.Forms.Padding(20, 20, 20, 20);
            this.panelContenido.Size = new System.Drawing.Size(1242, 800);
            this.panelContenido.TabIndex = 2;

            // panelHeaderHero / labels: ELIMINADOS del árbol visible.
            // Las instancias se mantienen para no romper código existente que las referencie.

            // ============ cardBusqueda ============
            this.cardBusqueda.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.cardBusqueda.AccentColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.cardBusqueda.CardTitle = ""; // Sin header para mantenerlo compacto
            this.cardBusqueda.CardSubtitle = "";
            this.cardBusqueda.Controls.Add(this.lblManzanaLabel);
            this.cardBusqueda.Controls.Add(this.txtManzanaPanel);
            this.cardBusqueda.Controls.Add(this.lblLoteLabel);
            this.cardBusqueda.Controls.Add(this.txtLotePanel);
            this.cardBusqueda.Controls.Add(this.btnBuscarCasaModern);
            this.cardBusqueda.Location = new System.Drawing.Point(20, 20);
            this.cardBusqueda.Name = "cardBusqueda";
            this.cardBusqueda.Size = new System.Drawing.Size(1196, 90);
            this.cardBusqueda.TabIndex = 1;

            // Layout horizontal compacto: header a la izquierda, inputs en línea, botón a la derecha
            this.lblManzanaLabel.AutoSize = true;
            this.lblManzanaLabel.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblManzanaLabel.ForeColor = System.Drawing.Color.FromArgb(115, 110, 102);
            this.lblManzanaLabel.Location = new System.Drawing.Point(20, 18);
            this.lblManzanaLabel.Name = "lblManzanaLabel";
            this.lblManzanaLabel.Text = "MANZANA";

            this.txtManzanaPanel.Location = new System.Drawing.Point(20, 36);
            this.txtManzanaPanel.Name = "txtManzanaPanel";
            this.txtManzanaPanel.Size = new System.Drawing.Size(160, 32);
            this.txtManzanaPanel.Placeholder = "Ej. 5";
            this.txtManzana.Name = "txtManzana";
            this.txtManzana.TabIndex = 0;

            this.lblLoteLabel.AutoSize = true;
            this.lblLoteLabel.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblLoteLabel.ForeColor = System.Drawing.Color.FromArgb(115, 110, 102);
            this.lblLoteLabel.Location = new System.Drawing.Point(196, 18);
            this.lblLoteLabel.Name = "lblLoteLabel";
            this.lblLoteLabel.Text = "LOTE";

            this.txtLotePanel.Location = new System.Drawing.Point(196, 36);
            this.txtLotePanel.Name = "txtLotePanel";
            this.txtLotePanel.Size = new System.Drawing.Size(160, 32);
            this.txtLotePanel.Placeholder = "Ej. 12";
            this.txtLote.Name = "txtLote";
            this.txtLote.TabIndex = 1;

            this.btnBuscarCasaModern.Location = new System.Drawing.Point(372, 36);
            this.btnBuscarCasaModern.Name = "btnBuscarCasa";
            this.btnBuscarCasaModern.Size = new System.Drawing.Size(140, 32);
            this.btnBuscarCasaModern.TabIndex = 2;
            this.btnBuscarCasaModern.Text = "BUSCAR";
            this.btnBuscarCasaModern.Click += new System.EventHandler(this.btnBuscarCasa_Click);

            // ============ KPI Cards ============
            int kpiTop = 122; // debajo del card de búsqueda (20 + 90 + 12)
            int kpiHeight = 88;
            int kpiGap = 14;

            int kpiWidth = 288;

            this.kpiAvanceFisico.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.kpiAvanceFisico.AccentColor = System.Drawing.Color.FromArgb(46, 160, 89);
            this.kpiAvanceFisico.LabelText = "Avance Físico";
            this.kpiAvanceFisico.ValueText = "0.0";
            this.kpiAvanceFisico.Suffix = "%";
            this.kpiAvanceFisico.Trend = "Sin casa seleccionada";
            this.kpiAvanceFisico.Location = new System.Drawing.Point(20, kpiTop);
            this.kpiAvanceFisico.Name = "kpiAvanceFisico";
            this.kpiAvanceFisico.Size = new System.Drawing.Size(kpiWidth, kpiHeight);
            this.kpiAvanceFisico.TabIndex = 10;

            this.kpiAvanceEconomico.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.kpiAvanceEconomico.AccentColor = System.Drawing.Color.FromArgb(45, 130, 192);
            this.kpiAvanceEconomico.LabelText = "Avance Económico";
            this.kpiAvanceEconomico.ValueText = "$0";
            this.kpiAvanceEconomico.Suffix = "";
            this.kpiAvanceEconomico.Trend = "Monto total ejecutado";
            this.kpiAvanceEconomico.Location = new System.Drawing.Point(20 + (kpiWidth + kpiGap), kpiTop);
            this.kpiAvanceEconomico.Name = "kpiAvanceEconomico";
            this.kpiAvanceEconomico.Size = new System.Drawing.Size(kpiWidth, kpiHeight);
            this.kpiAvanceEconomico.TabIndex = 11;

            this.kpiPartidasTotal.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.kpiPartidasTotal.AccentColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.kpiPartidasTotal.LabelText = "Total Partidas";
            this.kpiPartidasTotal.ValueText = "0";
            this.kpiPartidasTotal.Suffix = "";
            this.kpiPartidasTotal.Trend = "Filtradas por prototipo";
            this.kpiPartidasTotal.Location = new System.Drawing.Point(20 + 2 * (kpiWidth + kpiGap), kpiTop);
            this.kpiPartidasTotal.Name = "kpiPartidasTotal";
            this.kpiPartidasTotal.Size = new System.Drawing.Size(kpiWidth, kpiHeight);
            this.kpiPartidasTotal.TabIndex = 12;

            this.kpiPartidasCompletadas.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.kpiPartidasCompletadas.AccentColor = System.Drawing.Color.FromArgb(217, 142, 24);
            this.kpiPartidasCompletadas.LabelText = "Completadas";
            this.kpiPartidasCompletadas.ValueText = "0";
            this.kpiPartidasCompletadas.Suffix = "";
            this.kpiPartidasCompletadas.Trend = "0 pendientes";
            this.kpiPartidasCompletadas.Location = new System.Drawing.Point(20 + 3 * (kpiWidth + kpiGap), kpiTop);
            this.kpiPartidasCompletadas.Name = "kpiPartidasCompletadas";
            this.kpiPartidasCompletadas.Size = new System.Drawing.Size(kpiWidth, kpiHeight);
            this.kpiPartidasCompletadas.TabIndex = 13;

            // ============ cardMapa ============
            int mapTop = kpiTop + kpiHeight + 14;

            this.cardMapa.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right;
            this.cardMapa.AccentColor = System.Drawing.Color.FromArgb(45, 130, 192);
            this.cardMapa.CardTitle = "Mapa del Sembrado";
            this.cardMapa.CardSubtitle = "Pan & zoom interactivo";
            this.cardMapa.Controls.Add(this.pictureBoxMapa);
            this.cardMapa.Controls.Add(this.btnMapaZoomIn);
            this.cardMapa.Controls.Add(this.btnMapaZoomOut);
            this.cardMapa.Controls.Add(this.btnMapaReset);
            this.cardMapa.Controls.Add(this.lblZoomNivel);
            this.cardMapa.Controls.Add(this.lblInstruccionesMapa);
            this.cardMapa.Location = new System.Drawing.Point(20, mapTop);
            this.cardMapa.MinimumSize = new System.Drawing.Size(400, 320);
            this.cardMapa.Name = "cardMapa";
            this.cardMapa.Size = new System.Drawing.Size(836, 400);
            this.cardMapa.TabIndex = 20;

            this.pictureBoxMapa.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right;
            this.pictureBoxMapa.BackColor = System.Drawing.Color.FromArgb(248, 246, 240);
            this.pictureBoxMapa.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxMapa.Location = new System.Drawing.Point(14, 52);
            this.pictureBoxMapa.Name = "pictureBoxMapa";
            this.pictureBoxMapa.Size = new System.Drawing.Size(808, 320);
            this.pictureBoxMapa.TabIndex = 0;
            this.pictureBoxMapa.TabStop = false;

            // Overlay buttons (top-right del mapa, dentro del área del mapa) - más compactos
            this.btnMapaZoomIn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnMapaZoomIn.Glyph = "+";
            this.btnMapaZoomIn.Location = new System.Drawing.Point(792, 62);
            this.btnMapaZoomIn.Name = "btnMapaZoomIn";
            this.btnMapaZoomIn.Size = new System.Drawing.Size(28, 28);

            this.btnMapaZoomOut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnMapaZoomOut.Glyph = "−";
            this.btnMapaZoomOut.Location = new System.Drawing.Point(792, 94);
            this.btnMapaZoomOut.Name = "btnMapaZoomOut";
            this.btnMapaZoomOut.Size = new System.Drawing.Size(28, 28);

            this.btnMapaReset.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnMapaReset.Glyph = "⛶";
            this.btnMapaReset.Location = new System.Drawing.Point(792, 126);
            this.btnMapaReset.Name = "btnMapaReset";
            this.btnMapaReset.Size = new System.Drawing.Size(28, 28);

            // lblZoomNivel (bottom-right del mapa, sobre la imagen)
            this.lblZoomNivel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.lblZoomNivel.AutoSize = true;
            this.lblZoomNivel.BackColor = System.Drawing.Color.FromArgb(235, 255, 255, 255);
            this.lblZoomNivel.Font = new System.Drawing.Font("Segoe UI", 7.75F, System.Drawing.FontStyle.Bold);
            this.lblZoomNivel.ForeColor = System.Drawing.Color.FromArgb(60, 50, 40);
            this.lblZoomNivel.Location = new System.Drawing.Point(778, 345);
            this.lblZoomNivel.Name = "lblZoomNivel";
            this.lblZoomNivel.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
            this.lblZoomNivel.Text = "100%";

            this.lblInstruccionesMapa.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.lblInstruccionesMapa.AutoSize = true;
            this.lblInstruccionesMapa.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Italic);
            this.lblInstruccionesMapa.ForeColor = System.Drawing.Color.FromArgb(140, 134, 124);
            this.lblInstruccionesMapa.Location = new System.Drawing.Point(14, 378);
            this.lblInstruccionesMapa.Name = "lblInstruccionesMapa";
            this.lblInstruccionesMapa.Text = "💡 Busca una casa · Rueda = zoom · Arrastra = mover";

            // ============ cardInfoCasa ============
            this.cardInfoCasa.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Right;
            this.cardInfoCasa.AccentColor = System.Drawing.Color.FromArgb(46, 160, 89);
            this.cardInfoCasa.CardTitle = "Última Evidencia";
            this.cardInfoCasa.CardSubtitle = "Foto más reciente";
            this.cardInfoCasa.Controls.Add(this.lblCasaActual);
            this.cardInfoCasa.Controls.Add(this.lblManzanaLote);
            this.cardInfoCasa.Controls.Add(this.lblPrototipo);
            this.cardInfoCasa.Controls.Add(this.pictureBoxCasa);
            this.cardInfoCasa.Controls.Add(this.lblFotoEmpty);
            this.cardInfoCasa.Location = new System.Drawing.Point(872, mapTop);
            this.cardInfoCasa.MinimumSize = new System.Drawing.Size(280, 320);
            this.cardInfoCasa.Name = "cardInfoCasa";
            this.cardInfoCasa.Size = new System.Drawing.Size(344, 400);
            this.cardInfoCasa.TabIndex = 21;

            this.lblCasaActual.AutoSize = true;
            this.lblCasaActual.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblCasaActual.ForeColor = System.Drawing.Color.FromArgb(48, 42, 36);
            this.lblCasaActual.Location = new System.Drawing.Point(14, 52);
            this.lblCasaActual.Name = "lblCasaActual";
            this.lblCasaActual.Text = "Sin casa seleccionada";

            this.lblManzanaLote.AutoSize = true;
            this.lblManzanaLote.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblManzanaLote.ForeColor = System.Drawing.Color.FromArgb(115, 110, 102);
            this.lblManzanaLote.Location = new System.Drawing.Point(14, 73);
            this.lblManzanaLote.Name = "lblManzanaLote";
            this.lblManzanaLote.Text = "Mz: -  ·  Lote: -";

            this.lblPrototipo.AutoSize = true;
            this.lblPrototipo.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblPrototipo.ForeColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.lblPrototipo.Location = new System.Drawing.Point(14, 89);
            this.lblPrototipo.Name = "lblPrototipo";
            this.lblPrototipo.Text = "Prototipo: -";

            this.pictureBoxCasa.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right;
            this.pictureBoxCasa.BackColor = System.Drawing.Color.FromArgb(248, 246, 240);
            this.pictureBoxCasa.Location = new System.Drawing.Point(14, 112);
            this.pictureBoxCasa.Name = "pictureBoxCasa";
            this.pictureBoxCasa.Size = new System.Drawing.Size(316, 274);
            this.pictureBoxCasa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxCasa.TabIndex = 3;
            this.pictureBoxCasa.TabStop = false;

            this.lblFotoEmpty.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right;
            this.lblFotoEmpty.BackColor = System.Drawing.Color.Transparent;
            this.lblFotoEmpty.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic);
            this.lblFotoEmpty.ForeColor = System.Drawing.Color.FromArgb(160, 154, 144);
            this.lblFotoEmpty.Location = new System.Drawing.Point(14, 112);
            this.lblFotoEmpty.Name = "lblFotoEmpty";
            this.lblFotoEmpty.Size = new System.Drawing.Size(316, 274);
            this.lblFotoEmpty.Text = "🖼  Sin evidencia fotográfica";
            this.lblFotoEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblFotoEmpty.BringToFront();

            // ============ cardProgreso ============
            int progresoTop = mapTop + 400 + 12;

            this.cardProgreso.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right;
            this.cardProgreso.AccentColor = System.Drawing.Color.FromArgb(217, 142, 24);
            this.cardProgreso.CardTitle = "Progreso de la Obra";
            this.cardProgreso.CardSubtitle = "Físico y económico en tiempo real";
            this.cardProgreso.Controls.Add(this.lblProgresoGeneral);
            this.cardProgreso.Controls.Add(this.animProgresoGeneral);
            this.cardProgreso.Controls.Add(this.lblProgresoGeneralValor);
            this.cardProgreso.Controls.Add(this.lblProgresoPartidas);
            this.cardProgreso.Controls.Add(this.animProgresoPartidas);
            this.cardProgreso.Controls.Add(this.lblProgresoPartidasValor);
            this.cardProgreso.Location = new System.Drawing.Point(20, progresoTop);
            this.cardProgreso.MinimumSize = new System.Drawing.Size(600, 130);
            this.cardProgreso.Name = "cardProgreso";
            this.cardProgreso.Size = new System.Drawing.Size(836, 130);
            this.cardProgreso.TabIndex = 30;

            this.lblProgresoGeneral.AutoSize = true;
            this.lblProgresoGeneral.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblProgresoGeneral.ForeColor = System.Drawing.Color.FromArgb(115, 110, 102);
            this.lblProgresoGeneral.Location = new System.Drawing.Point(18, 50);
            this.lblProgresoGeneral.Name = "lblProgresoGeneral";
            this.lblProgresoGeneral.Text = "AVANCE FÍSICO";

            this.animProgresoGeneral.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.animProgresoGeneral.Location = new System.Drawing.Point(18, 66);
            this.animProgresoGeneral.Name = "animProgresoGeneral";
            this.animProgresoGeneral.Size = new System.Drawing.Size(700, 14);
            this.animProgresoGeneral.GradientStart = System.Drawing.Color.FromArgb(46, 160, 89);
            this.animProgresoGeneral.GradientEnd = System.Drawing.Color.FromArgb(74, 188, 124);
            this.animProgresoGeneral.Value = 0;
            this.animProgresoGeneral.ShowPercent = true;

            this.lblProgresoGeneralValor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblProgresoGeneralValor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProgresoGeneralValor.ForeColor = System.Drawing.Color.FromArgb(46, 160, 89);
            this.lblProgresoGeneralValor.Location = new System.Drawing.Point(728, 62);
            this.lblProgresoGeneralValor.Name = "lblProgresoGeneralValor";
            this.lblProgresoGeneralValor.Size = new System.Drawing.Size(90, 18);
            this.lblProgresoGeneralValor.Text = "0%";
            this.lblProgresoGeneralValor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.lblProgresoPartidas.AutoSize = true;
            this.lblProgresoPartidas.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblProgresoPartidas.ForeColor = System.Drawing.Color.FromArgb(115, 110, 102);
            this.lblProgresoPartidas.Location = new System.Drawing.Point(18, 92);
            this.lblProgresoPartidas.Name = "lblProgresoPartidas";
            this.lblProgresoPartidas.Text = "AVANCE ECONÓMICO";

            this.animProgresoPartidas.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.animProgresoPartidas.Location = new System.Drawing.Point(18, 108);
            this.animProgresoPartidas.Name = "animProgresoPartidas";
            this.animProgresoPartidas.Size = new System.Drawing.Size(500, 14);
            this.animProgresoPartidas.GradientStart = System.Drawing.Color.FromArgb(45, 130, 192);
            this.animProgresoPartidas.GradientEnd = System.Drawing.Color.FromArgb(73, 165, 220);
            this.animProgresoPartidas.Value = 0;
            this.animProgresoPartidas.ShowPercent = true;

            this.lblProgresoPartidasValor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblProgresoPartidasValor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProgresoPartidasValor.ForeColor = System.Drawing.Color.FromArgb(45, 130, 192);
            this.lblProgresoPartidasValor.Location = new System.Drawing.Point(528, 104);
            this.lblProgresoPartidasValor.Name = "lblProgresoPartidasValor";
            this.lblProgresoPartidasValor.Size = new System.Drawing.Size(290, 18);
            this.lblProgresoPartidasValor.Text = "$0.00";
            this.lblProgresoPartidasValor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // ProgressBars nativas back-compat (no se agregan al árbol visible)
            this.progressBarGeneral.Name = "progressBarGeneral";
            this.progressBarGeneral.Visible = false;
            this.progressBarGeneral.Size = new System.Drawing.Size(1, 1);
            this.progressBarPartidas.Name = "progressBarPartidas";
            this.progressBarPartidas.Visible = false;
            this.progressBarPartidas.Size = new System.Drawing.Size(1, 1);

            // ============ cardEstadisticas ============
            this.cardEstadisticas.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Right;
            this.cardEstadisticas.AccentColor = System.Drawing.Color.FromArgb(155, 89, 182);
            this.cardEstadisticas.CardTitle = "Estadísticas";
            this.cardEstadisticas.CardSubtitle = "";
            this.cardEstadisticas.Controls.Add(this.lblTotalDestajos);
            this.cardEstadisticas.Controls.Add(this.lblDestajosCompletados);
            this.cardEstadisticas.Controls.Add(this.lblDestajosPendientes);
            this.cardEstadisticas.Controls.Add(this.lblUltimaActualizacion);
            this.cardEstadisticas.Location = new System.Drawing.Point(872, progresoTop);
            this.cardEstadisticas.MinimumSize = new System.Drawing.Size(280, 130);
            this.cardEstadisticas.Name = "cardEstadisticas";
            this.cardEstadisticas.Size = new System.Drawing.Size(344, 130);
            this.cardEstadisticas.TabIndex = 31;

            this.lblTotalDestajos.AutoSize = true;
            this.lblTotalDestajos.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblTotalDestajos.ForeColor = System.Drawing.Color.FromArgb(48, 42, 36);
            this.lblTotalDestajos.Location = new System.Drawing.Point(18, 50);
            this.lblTotalDestajos.Name = "lblTotalDestajos";
            this.lblTotalDestajos.Text = "Total de Partidas: 0";

            this.lblDestajosCompletados.AutoSize = true;
            this.lblDestajosCompletados.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblDestajosCompletados.ForeColor = System.Drawing.Color.FromArgb(46, 160, 89);
            this.lblDestajosCompletados.Location = new System.Drawing.Point(18, 68);
            this.lblDestajosCompletados.Name = "lblDestajosCompletados";
            this.lblDestajosCompletados.Text = "Partidas Completadas: 0";

            this.lblDestajosPendientes.AutoSize = true;
            this.lblDestajosPendientes.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblDestajosPendientes.ForeColor = System.Drawing.Color.FromArgb(217, 142, 24);
            this.lblDestajosPendientes.Location = new System.Drawing.Point(18, 86);
            this.lblDestajosPendientes.Name = "lblDestajosPendientes";
            this.lblDestajosPendientes.Text = "Partidas Pendientes: 0";

            this.lblUltimaActualizacion.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblUltimaActualizacion.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Italic);
            this.lblUltimaActualizacion.ForeColor = System.Drawing.Color.FromArgb(140, 134, 124);
            this.lblUltimaActualizacion.Location = new System.Drawing.Point(18, 110);
            this.lblUltimaActualizacion.Name = "lblUltimaActualizacion";
            this.lblUltimaActualizacion.Size = new System.Drawing.Size(308, 16);
            this.lblUltimaActualizacion.Text = "Última Actualización: -";

            // ============ PanelPrincipal ============
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1492, 848);
            this.Controls.Add(this.panelContenido);
            this.Controls.Add(this.panelSidebar);
            this.Controls.Add(this.menuStripGeneral);
            this.Controls.Add(this.modernNavBar); // último → toma Dock=Top primero
            this.MinimumSize = new System.Drawing.Size(1500, 830);
            this.Name = "PanelPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sistema de Gestión de Obra - Calandria Residencial";
            this.Load += new System.EventHandler(this.Form1_Load);

            this.panelSidebar.ResumeLayout(false);
            this.panelDevTools.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.panelContenido.ResumeLayout(false);
            this.cardBusqueda.ResumeLayout(false);
            this.cardBusqueda.PerformLayout();
            this.cardMapa.ResumeLayout(false);
            this.cardMapa.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMapa)).EndInit();
            this.cardInfoCasa.ResumeLayout(false);
            this.cardInfoCasa.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCasa)).EndInit();
            this.cardProgreso.ResumeLayout(false);
            this.cardProgreso.PerformLayout();
            this.cardEstadisticas.ResumeLayout(false);
            this.cardEstadisticas.PerformLayout();
            this.ResumeLayout(false);
        }

        private static void ConfigSidebarBtn(System.Windows.Forms.Button btn, string text,
            System.Drawing.Color bg, System.Drawing.Color down, System.Drawing.Color over, int top)
        {
            btn.BackColor = bg;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseDownBackColor = down;
            btn.FlatAppearance.MouseOverBackColor = over;
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btn.ForeColor = System.Drawing.Color.White;
            btn.Location = new System.Drawing.Point(0, top);
            btn.Size = new System.Drawing.Size(210, 38);
            btn.Text = text;
            btn.UseVisualStyleBackColor = false;
            btn.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        #endregion

        // ============ Fields ============
        private System.Windows.Forms.MenuStrip menuStripGeneral;
        private System.Windows.Forms.Panel panelSidebar;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label lblRol;
        private System.Windows.Forms.Button btnCerrarSesion;
        private System.Windows.Forms.Panel panelDevTools;
        private System.Windows.Forms.Button btnDiagnosticoConexion;
        private System.Windows.Forms.Button btnRegistrarUsuario;
        private System.Windows.Forms.Button btnRegistrarTrabajador;
        private System.Windows.Forms.Button btnGestionarCuadrillas;

        private System.Windows.Forms.Panel panelContenido;

        // Modern Nav Bar (reemplaza al MenuStrip)
        private ModernNavBar modernNavBar;

        // Hero / Header (DEPRECATED, kept for backwards-compat)
        private System.Windows.Forms.Panel panelHeaderHero;
        private System.Windows.Forms.Label lblHeaderTitulo;
        private System.Windows.Forms.Label lblHeaderSubtitulo;
        private System.Windows.Forms.Label lblFechaActual;

        // Búsqueda
        private ModernCard cardBusqueda;
        private System.Windows.Forms.Label lblManzanaLabel;
        private System.Windows.Forms.Label lblLoteLabel;
        private RoundedTextBoxPanel txtManzanaPanel;
        private RoundedTextBoxPanel txtLotePanel;
        private PrimaryButton btnBuscarCasaModern;
        private System.Windows.Forms.TextBox txtManzana;
        private System.Windows.Forms.TextBox txtLote;
        private System.Windows.Forms.Button btnBuscarCasa;

        // KPIs
        private KpiCard kpiAvanceFisico;
        private KpiCard kpiAvanceEconomico;
        private KpiCard kpiPartidasTotal;
        private KpiCard kpiPartidasCompletadas;

        // Mapa
        private ModernCard cardMapa;
        private System.Windows.Forms.PictureBox pictureBoxMapa;
        private System.Windows.Forms.Label lblInstruccionesMapa;
        private CircularIconButton btnMapaZoomIn;
        private CircularIconButton btnMapaZoomOut;
        private CircularIconButton btnMapaReset;
        private System.Windows.Forms.Label lblZoomNivel;

        // Info casa
        private ModernCard cardInfoCasa;
        private System.Windows.Forms.Label lblCasaActual;
        private System.Windows.Forms.Label lblManzanaLote;
        private System.Windows.Forms.Label lblPrototipo;
        private System.Windows.Forms.PictureBox pictureBoxCasa;
        private System.Windows.Forms.Label lblFotoEmpty;

        // Progreso
        private ModernCard cardProgreso;
        private System.Windows.Forms.Label lblProgresoGeneral;
        private AnimatedProgressBar animProgresoGeneral;
        private System.Windows.Forms.ProgressBar progressBarGeneral; // back-compat
        private System.Windows.Forms.Label lblProgresoGeneralValor;
        private System.Windows.Forms.Label lblProgresoPartidas;
        private AnimatedProgressBar animProgresoPartidas;
        private System.Windows.Forms.ProgressBar progressBarPartidas; // back-compat
        private System.Windows.Forms.Label lblProgresoPartidasValor;

        // Estadísticas
        private ModernCard cardEstadisticas;
        private System.Windows.Forms.Label lblTotalDestajos;
        private System.Windows.Forms.Label lblDestajosCompletados;
        private System.Windows.Forms.Label lblDestajosPendientes;
        private System.Windows.Forms.Label lblUltimaActualizacion;

        // Timer pin (mapa)
        private System.Windows.Forms.Timer timerPin;
        private float pinPulsePhase = 0f;
    }
}
