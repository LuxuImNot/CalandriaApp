// Layout responsivo para FormAlmacen — diseño profesional con cards.
// Construye en runtime: header banner, footer status bar y wrappers tipo card
// para cada pestaña. Reparenta los controles existentes sin modificar el Designer.
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        // ===================================================================
        // CONSTANTES DE DISEÑO (sistema 4/8/12/16/24)
        // ===================================================================
        private const int HeaderHeight     = 64;
        private const int FooterHeight     = 28;
        private const int SidebarWidth     = 280;
        private const int SidebarPadding   = 16;
        private const int CardGap          = 12;
        private const int FieldSpacing     = 6;
        private const int SectionSpacing   = 14;
        private const int ActionBarHeight  = 64;

        // ===================================================================
        // CONTENEDORES CONSTRUIDOS EN RUNTIME
        // ===================================================================
        private Panel pnlHeader;
        private Label lblHeaderTitulo;
        private Label lblHeaderSubtitulo;
        private Panel pnlFooter;
        private Label lblFooterContexto;
        private Label lblFooterTimestamp;
        private Label lblFooterCredito;

        // Tab 1 (Entradas)
        private Panel pnlSidebarEntradas;
        private Panel pnlGridEntradas;
        private Panel pnlActionsEntradas;

        // Tab 2 (Salidas)
        private Panel pnlSidebarSalidas;
        private Panel pnlGridSalidas;
        private Panel pnlActionsSalidas;

        // Tab 3 (Inventario)
        private Panel pnlToolbarInventario;
        private Panel pnlGridInventarioContent;
        private Panel pnlActionsInventario;

        // Tab 4 (Consulta por casa)
        private Panel pnlToolbarConsulta;
        private Panel pnlGridConsultaContent;
        private Panel pnlActionsConsulta;
        // Controles creados en runtime para esta pestaña
        public Label lblRutaConsulta;
        public ComboBox cmbRutaConsulta;

        // Valores de filtro de ruta
        public const string RutaFiltroTodas = "Todas las rutas";
        public const string RutaFiltroTunera = "Ruta Tunera Destajo";
        public const string RutaFiltroCalandra = "Ruta Calandra Destajo";

        // Tab 5 (Historial)
        private Panel pnlHistorialContent;

        // Marcador para no construir dos veces
        private bool _layoutConstruido = false;

        // ===================================================================
        // PUNTO DE ENTRADA
        // ===================================================================
        public void ConfigurarLayoutResponsivo()
        {
            if (_layoutConstruido) return;
            _layoutConstruido = true;

            this.SuspendLayout();

            this.MinimumSize = new Size(1100, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeManager.ColorFondoApp;

            ConstruirHeader();
            ConstruirFooter();

            if (TabsControl != null)
            {
                TabsControl.Dock = DockStyle.Fill;
                TabsControl.BringToFront(); // garantizar que Fill se dockee de último
            }

            ConfigurarLayoutEntradas();
            ConfigurarLayoutSalidas();
            ConfigurarLayoutInventario();
            ConfigurarLayoutConsultaCasa();
            ConfigurarLayoutHistorial();

            this.ResumeLayout(true);
        }

        // ===================================================================
        // HEADER BANNER (corporativo)
        // ===================================================================
        private void ConstruirHeader()
        {
            pnlHeader = new Panel
            {
                Name = "pnlHeaderForm",
                Dock = DockStyle.Top,
                Height = HeaderHeight,
                BackColor = ThemeManager.ColorPrincipalMenuBar,
                Padding = new Padding(24, 10, 24, 10)
            };
            ThemeManager.HabilitarDoubleBuffer(pnlHeader);

            lblHeaderTitulo = new Label
            {
                Name = "lblHeaderTitulo",
                Text = "Gestión de Almacén",
                ForeColor = ThemeManager.ColorTextoClaro,
                Font = new Font(ThemeManager.FuenteTitulo.FontFamily, 15F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 8),
                BackColor = Color.Transparent
            };
            lblHeaderSubtitulo = new Label
            {
                Name = "lblHeaderSubtitulo",
                Text = "Entradas · Salidas · Inventario · Consulta · Historial",
                ForeColor = Color.FromArgb(245, 232, 215),
                Font = new Font(ThemeManager.FuenteRegular.FontFamily, 9F, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(25, 34),
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblHeaderSubtitulo);
            pnlHeader.Controls.Add(lblHeaderTitulo);

            // Línea de acento sutil en la parte inferior
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(60, 0, 0, 0), 1))
                    e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            this.Controls.Add(pnlHeader);
        }

        // ===================================================================
        // FOOTER STATUS BAR
        // ===================================================================
        private void ConstruirFooter()
        {
            pnlFooter = new Panel
            {
                Name = "pnlFooterForm",
                Dock = DockStyle.Bottom,
                Height = FooterHeight,
                BackColor = ThemeManager.ColorFondoAlterno,
                Padding = new Padding(20, 4, 20, 4)
            };
            ThemeManager.HabilitarDoubleBuffer(pnlFooter);

            lblFooterContexto = new Label
            {
                Name = "lblFooterContexto",
                Text = "Listo",
                ForeColor = ThemeManager.ColorTextoSecundario,
                Font = ThemeManager.FuenteEtiqueta,
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 400,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            lblFooterCredito = new Label
            {
                Name = "lblFooterCredito",
                Text = "A software by LuxuDev",
                ForeColor = ThemeManager.ColorTextoSutil,
                Font = new Font(ThemeManager.FuenteEtiqueta.FontFamily,
                                ThemeManager.FuenteEtiqueta.Size,
                                FontStyle.Italic),
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            lblFooterTimestamp = new Label
            {
                Name = "lblFooterTimestamp",
                Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                ForeColor = ThemeManager.ColorTextoSutil,
                Font = ThemeManager.FuenteEtiqueta,
                Dock = DockStyle.Right,
                AutoSize = false,
                Width = 220,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };

            pnlFooter.Controls.Add(lblFooterCredito);   // Fill primero
            pnlFooter.Controls.Add(lblFooterContexto);  // Left
            pnlFooter.Controls.Add(lblFooterTimestamp); // Right
            lblFooterCredito.BringToFront();

            // Línea divisoria arriba
            pnlFooter.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.ColorDivisor, 1))
                    e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            this.Controls.Add(pnlFooter);
        }

        /// <summary>
        /// Permite a otros módulos actualizar el texto del status bar.
        /// </summary>
        public void ActualizarStatusFooter(string mensaje)
        {
            if (lblFooterContexto != null)
                lblFooterContexto.Text = mensaje ?? string.Empty;
            if (lblFooterTimestamp != null)
                lblFooterTimestamp.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        // ===================================================================
        // FACTORY DE CARDS
        // ===================================================================
        private Panel CrearCard(string nombre, Padding? padding = null)
        {
            var card = new Panel
            {
                Name = nombre,
                BackColor = ThemeManager.ColorFondoCard,
                BorderStyle = BorderStyle.None,
                Padding = padding ?? new Padding(SidebarPadding)
            };
            ThemeManager.HabilitarDoubleBuffer(card);
            ThemeManager.DibujarBordeSuave(card);
            return card;
        }

        private Panel CrearActionBar(string nombre)
        {
            var bar = new Panel
            {
                Name = nombre,
                Dock = DockStyle.Bottom,
                Height = ActionBarHeight,
                BackColor = ThemeManager.ColorFondoCard,
                Padding = new Padding(16, 12, 16, 12)
            };
            ThemeManager.HabilitarDoubleBuffer(bar);
            bar.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.ColorBorde, 1))
                    e.Graphics.DrawLine(pen, 0, 0, bar.Width, 0);
            };
            return bar;
        }

        // ===================================================================
        // TAB 1 — ENTRADAS
        // ===================================================================
        private void ConfigurarLayoutEntradas()
        {
            if (tabPage1 == null) return;
            tabPage1.SuspendLayout();
            tabPage1.Padding = new Padding(CardGap);
            tabPage1.BackColor = ThemeManager.ColorFondoApp;

            pnlSidebarEntradas = CrearCard("pnlSidebarEntradas");
            pnlSidebarEntradas.Dock = DockStyle.Left;
            pnlSidebarEntradas.Width = SidebarWidth;

            pnlGridEntradas = CrearCard("pnlGridEntradas", new Padding(0));
            pnlGridEntradas.Dock = DockStyle.Fill;
            pnlGridEntradas.Padding = new Padding(0);

            // Spacer entre sidebar y grid
            var spacerEntradas = new Panel
            {
                Dock = DockStyle.Left,
                Width = CardGap,
                BackColor = ThemeManager.ColorFondoApp
            };

            // Reparentar grid
            if (dgvEntrada != null)
            {
                if (dgvEntrada.Parent != null) dgvEntrada.Parent.Controls.Remove(dgvEntrada);
                dgvEntrada.Dock = DockStyle.Fill;
                dgvEntrada.Margin = new Padding(0);
                pnlGridEntradas.Controls.Add(dgvEntrada);
            }

            // Llenar sidebar con secciones ordenadas
            int y = 0;

            AgregarSidebar(pnlSidebarEntradas, CrearTituloSeccion("CASA SELECCIONADA"), ref y);
            AgregarSidebar(pnlSidebarEntradas, lblCasa, ref y, SectionSpacing);

            AgregarSidebar(pnlSidebarEntradas, CrearTituloSeccion("ORDEN DE COMPRA"), ref y);
            ConfigurarTamanoCampo(cmbOrdenesCompra);
            AgregarSidebar(pnlSidebarEntradas, cmbOrdenesCompra, ref y, FieldSpacing);
            ConfigurarTamanoBoton(btnBuscarOrden, 36);
            AgregarSidebar(pnlSidebarEntradas, btnBuscarOrden, ref y, SectionSpacing);

            if (flwCasasAsociadas != null)
            {
                AgregarSidebar(pnlSidebarEntradas, CrearTituloSeccion("CASAS ASOCIADAS"), ref y);
                flwCasasAsociadas.Width = SidebarWidth - SidebarPadding * 2;
                flwCasasAsociadas.Height = 90;
                flwCasasAsociadas.AutoScroll = true;
                AgregarSidebar(pnlSidebarEntradas, flwCasasAsociadas, ref y, SectionSpacing);
            }

            AgregarSidebar(pnlSidebarEntradas, CrearTituloSeccion("BUSCAR EN ORDEN"), ref y);
            ConfigurarTamanoCampo(txtBuscarEntrada);
            AgregarSidebar(pnlSidebarEntradas, txtBuscarEntrada, ref y, SectionSpacing);

            ConfigurarTamanoBoton(btnEliminardeOrden, 36);
            AgregarSidebar(pnlSidebarEntradas, btnEliminardeOrden, ref y, SectionSpacing);

            // Action bar inferior con botón principal
            pnlActionsEntradas = CrearActionBar("pnlActionsEntradas");
            if (btnCapturarEntrada != null)
            {
                if (btnCapturarEntrada.Parent != null) btnCapturarEntrada.Parent.Controls.Remove(btnCapturarEntrada);
                btnCapturarEntrada.Dock = DockStyle.Right;
                btnCapturarEntrada.Width = 220;
                btnCapturarEntrada.Height = 40;
                pnlActionsEntradas.Controls.Add(btnCapturarEntrada);
            }

            // Limpiar labels descriptivos antiguos sueltos (label3, label6 ya no hacen falta como labels independientes)
            OcultarSiNoUsado(label3);
            OcultarSiNoUsado(label6);
            OcultarSiNoUsado(lblManzanaDisplay);
            OcultarSiNoUsado(lblLoteDisplay);

            // Orden de docking: el primero en agregarse se dockea primero
            // Sidebar izq, luego spacer izq, action bar abajo, finalmente grid (Fill)
            tabPage1.Controls.Add(pnlSidebarEntradas);
            tabPage1.Controls.Add(spacerEntradas);
            tabPage1.Controls.Add(pnlActionsEntradas);
            tabPage1.Controls.Add(pnlGridEntradas);
            pnlGridEntradas.BringToFront(); // Fill se procesa al final

            tabPage1.ResumeLayout(true);
        }

        // ===================================================================
        // TAB 2 — SALIDAS
        // ===================================================================
        private void ConfigurarLayoutSalidas()
        {
            if (tabPage2 == null) return;
            tabPage2.SuspendLayout();
            tabPage2.Padding = new Padding(CardGap);
            tabPage2.BackColor = ThemeManager.ColorFondoApp;

            pnlSidebarSalidas = CrearCard("pnlSidebarSalidas");
            pnlSidebarSalidas.Dock = DockStyle.Left;
            pnlSidebarSalidas.Width = SidebarWidth;

            pnlGridSalidas = CrearCard("pnlGridSalidas", new Padding(0));
            pnlGridSalidas.Dock = DockStyle.Fill;

            var spacerSalidas = new Panel
            {
                Dock = DockStyle.Left,
                Width = CardGap,
                BackColor = ThemeManager.ColorFondoApp
            };

            if (dgvInsumos != null)
            {
                if (dgvInsumos.Parent != null) dgvInsumos.Parent.Controls.Remove(dgvInsumos);
                dgvInsumos.Dock = DockStyle.Fill;
                dgvInsumos.Margin = new Padding(0);
                pnlGridSalidas.Controls.Add(dgvInsumos);
            }

            int y = 0;

            AgregarSidebar(pnlSidebarSalidas, CrearTituloSeccion("CASA DESTINO"), ref y);
            AgregarSidebar(pnlSidebarSalidas, lblCasaSalida, ref y, SectionSpacing);

            AgregarSidebar(pnlSidebarSalidas, CrearTituloSeccion("UBICACIÓN"), ref y);
            if (label2 != null) { label2.Text = "Manzana"; AgregarSidebar(pnlSidebarSalidas, label2, ref y, 2); }
            ConfigurarTamanoCampo(txtTrimManzanaSalida);
            AgregarSidebar(pnlSidebarSalidas, txtTrimManzanaSalida, ref y, FieldSpacing);
            if (label1 != null) { label1.Text = "Lote"; AgregarSidebar(pnlSidebarSalidas, label1, ref y, 2); }
            ConfigurarTamanoCampo(txtTrimLoteSalida);
            AgregarSidebar(pnlSidebarSalidas, txtTrimLoteSalida, ref y, FieldSpacing);
            ConfigurarTamanoBoton(btnSeleccionarCasaSalida, 36);
            AgregarSidebar(pnlSidebarSalidas, btnSeleccionarCasaSalida, ref y, SectionSpacing);

            AgregarSidebar(pnlSidebarSalidas, CrearTituloSeccion("BUSCAR INSUMO"), ref y);
            ConfigurarTamanoCampo(txtBuscarSalida);
            AgregarSidebar(pnlSidebarSalidas, txtBuscarSalida, ref y, SectionSpacing);

            if (lblSummary != null)
            {
                lblSummary.MaximumSize = new Size(SidebarWidth - SidebarPadding * 2, 0);
                lblSummary.AutoSize = true;
                AgregarSidebar(pnlSidebarSalidas, lblSummary, ref y, FieldSpacing);
            }

            // Action bar inferior: registrar (izq) y ver vales (der)
            pnlActionsSalidas = CrearActionBar("pnlActionsSalidas");
            if (btnRegistrarSalida != null)
            {
                if (btnRegistrarSalida.Parent != null) btnRegistrarSalida.Parent.Controls.Remove(btnRegistrarSalida);
                btnRegistrarSalida.Dock = DockStyle.Left;
                btnRegistrarSalida.Width = 220;
                btnRegistrarSalida.Height = 40;
                pnlActionsSalidas.Controls.Add(btnRegistrarSalida);
            }
            if (btnVerRepositorioVales != null)
            {
                if (btnVerRepositorioVales.Parent != null) btnVerRepositorioVales.Parent.Controls.Remove(btnVerRepositorioVales);
                btnVerRepositorioVales.Dock = DockStyle.Right;
                btnVerRepositorioVales.Width = 220;
                btnVerRepositorioVales.Height = 40;
                pnlActionsSalidas.Controls.Add(btnVerRepositorioVales);
            }

            OcultarSiNoUsado(label7);
            OcultarSiNoUsado(lblInstrucciones);

            tabPage2.Controls.Add(pnlSidebarSalidas);
            tabPage2.Controls.Add(spacerSalidas);
            tabPage2.Controls.Add(pnlActionsSalidas);
            tabPage2.Controls.Add(pnlGridSalidas);
            pnlGridSalidas.BringToFront();

            tabPage2.ResumeLayout(true);
        }

        // ===================================================================
        // TAB 3 — INVENTARIO (toolbar + grid + action bar)
        // ===================================================================
        private void ConfigurarLayoutInventario()
        {
            if (tabPageInventario == null) return;
            tabPageInventario.SuspendLayout();
            tabPageInventario.Padding = new Padding(CardGap);
            tabPageInventario.BackColor = ThemeManager.ColorFondoApp;

            pnlToolbarInventario = CrearCard("pnlToolbarInventario", new Padding(16, 12, 16, 12));
            pnlToolbarInventario.Dock = DockStyle.Top;
            pnlToolbarInventario.Height = 86;

            var spacerInv = new Panel
            {
                Dock = DockStyle.Top,
                Height = CardGap,
                BackColor = ThemeManager.ColorFondoApp
            };

            pnlGridInventarioContent = CrearCard("pnlGridInventarioContent", new Padding(0));
            pnlGridInventarioContent.Dock = DockStyle.Fill;

            pnlActionsInventario = CrearActionBar("pnlActionsInventario");

            // Toolbar layout
            if (lblBuscarInventario != null)
            {
                lblBuscarInventario.Text = "BUSCAR";
                lblBuscarInventario.Location = new Point(0, 6);
            }
            if (txtBuscarInventario != null)
            {
                if (txtBuscarInventario.Parent != null) txtBuscarInventario.Parent.Controls.Remove(txtBuscarInventario);
                txtBuscarInventario.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                txtBuscarInventario.Location = new Point(70, 0);
                txtBuscarInventario.Width = pnlToolbarInventario.ClientSize.Width - 70 - 32;
                txtBuscarInventario.Height = 28;
                pnlToolbarInventario.Controls.Add(txtBuscarInventario);
            }
            if (lblBuscarInventario != null)
            {
                if (lblBuscarInventario.Parent != null) lblBuscarInventario.Parent.Controls.Remove(lblBuscarInventario);
                pnlToolbarInventario.Controls.Add(lblBuscarInventario);
            }
            if (lblTotalInventario != null)
            {
                if (lblTotalInventario.Parent != null) lblTotalInventario.Parent.Controls.Remove(lblTotalInventario);
                lblTotalInventario.Location = new Point(0, 42);
                lblTotalInventario.AutoSize = true;
                pnlToolbarInventario.Controls.Add(lblTotalInventario);
            }

            // Grid
            if (dgvInventario != null)
            {
                if (dgvInventario.Parent != null) dgvInventario.Parent.Controls.Remove(dgvInventario);
                dgvInventario.Dock = DockStyle.Fill;
                dgvInventario.Margin = new Padding(0);
                pnlGridInventarioContent.Controls.Add(dgvInventario);
            }

            // Action bar
            if (btnActualizarInventario != null)
            {
                if (btnActualizarInventario.Parent != null) btnActualizarInventario.Parent.Controls.Remove(btnActualizarInventario);
                btnActualizarInventario.Dock = DockStyle.Left;
                btnActualizarInventario.Width = 180;
                btnActualizarInventario.Height = 40;
                pnlActionsInventario.Controls.Add(btnActualizarInventario);
            }
            if (btnExportarInventario != null)
            {
                if (btnExportarInventario.Parent != null) btnExportarInventario.Parent.Controls.Remove(btnExportarInventario);
                btnExportarInventario.Dock = DockStyle.Right;
                btnExportarInventario.Width = 220;
                btnExportarInventario.Height = 40;
                pnlActionsInventario.Controls.Add(btnExportarInventario);
            }

            tabPageInventario.Controls.Add(pnlToolbarInventario);
            tabPageInventario.Controls.Add(spacerInv);
            tabPageInventario.Controls.Add(pnlActionsInventario);
            tabPageInventario.Controls.Add(pnlGridInventarioContent);
            pnlGridInventarioContent.BringToFront();

            tabPageInventario.ResumeLayout(true);
        }

        // ===================================================================
        // TAB 4 — CONSULTA POR CASA
        // ===================================================================
        private void ConfigurarLayoutConsultaCasa()
        {
            if (tabPageConsultaCasa == null) return;
            tabPageConsultaCasa.SuspendLayout();
            tabPageConsultaCasa.Padding = new Padding(CardGap);
            tabPageConsultaCasa.BackColor = ThemeManager.ColorFondoApp;

            pnlToolbarConsulta = CrearCard("pnlToolbarConsulta", new Padding(16, 12, 16, 12));
            pnlToolbarConsulta.Dock = DockStyle.Top;
            pnlToolbarConsulta.Height = 130;

            var spacerCsq = new Panel
            {
                Dock = DockStyle.Top,
                Height = CardGap,
                BackColor = ThemeManager.ColorFondoApp
            };

            pnlGridConsultaContent = CrearCard("pnlGridConsultaContent", new Padding(0));
            pnlGridConsultaContent.Dock = DockStyle.Fill;

            pnlActionsConsulta = CrearActionBar("pnlActionsConsulta");

            // Fila 1: Ruta | Manzana | Lote | Buscar
            int filaY = 6;

            lblRutaConsulta = new Label
            {
                Name = "lblRutaConsulta",
                Text = "RUTA",
                Font = new Font(ThemeManager.FuenteSemi.FontFamily, 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.ColorTextoSecundario,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(0, filaY)
            };
            pnlToolbarConsulta.Controls.Add(lblRutaConsulta);

            cmbRutaConsulta = new ComboBox
            {
                Name = "cmbRutaConsulta",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = ThemeManager.FuenteRegular,
                Location = new Point(44, filaY - 2),
                Width = 180,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.ColorFondo,
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            cmbRutaConsulta.Items.AddRange(new object[]
            {
                RutaFiltroTodas, RutaFiltroTunera, RutaFiltroCalandra
            });
            cmbRutaConsulta.SelectedIndex = 0;
            pnlToolbarConsulta.Controls.Add(cmbRutaConsulta);

            if (lblManzanaConsulta != null)
            {
                lblManzanaConsulta.Text = "MANZANA";
                if (lblManzanaConsulta.Parent != null) lblManzanaConsulta.Parent.Controls.Remove(lblManzanaConsulta);
                lblManzanaConsulta.Location = new Point(234, filaY);
                pnlToolbarConsulta.Controls.Add(lblManzanaConsulta);
            }
            if (cmbManzanaConsulta != null)
            {
                if (cmbManzanaConsulta.Parent != null) cmbManzanaConsulta.Parent.Controls.Remove(cmbManzanaConsulta);
                cmbManzanaConsulta.Location = new Point(314, filaY - 2);
                cmbManzanaConsulta.Width = 110;
                pnlToolbarConsulta.Controls.Add(cmbManzanaConsulta);
            }
            if (lblLoteConsulta != null)
            {
                lblLoteConsulta.Text = "LOTE";
                if (lblLoteConsulta.Parent != null) lblLoteConsulta.Parent.Controls.Remove(lblLoteConsulta);
                lblLoteConsulta.Location = new Point(434, filaY);
                pnlToolbarConsulta.Controls.Add(lblLoteConsulta);
            }
            if (cmbLoteConsulta != null)
            {
                if (cmbLoteConsulta.Parent != null) cmbLoteConsulta.Parent.Controls.Remove(cmbLoteConsulta);
                cmbLoteConsulta.Location = new Point(472, filaY - 2);
                cmbLoteConsulta.Width = 110;
                pnlToolbarConsulta.Controls.Add(cmbLoteConsulta);
            }
            if (lblBuscarConsultaCasa != null)
            {
                lblBuscarConsultaCasa.Text = "BUSCAR";
                if (lblBuscarConsultaCasa.Parent != null) lblBuscarConsultaCasa.Parent.Controls.Remove(lblBuscarConsultaCasa);
                lblBuscarConsultaCasa.Location = new Point(592, filaY);
                pnlToolbarConsulta.Controls.Add(lblBuscarConsultaCasa);
            }
            if (txtBuscarConsultaCasa != null)
            {
                if (txtBuscarConsultaCasa.Parent != null) txtBuscarConsultaCasa.Parent.Controls.Remove(txtBuscarConsultaCasa);
                txtBuscarConsultaCasa.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                txtBuscarConsultaCasa.Location = new Point(650, filaY - 2);
                txtBuscarConsultaCasa.Width = pnlToolbarConsulta.ClientSize.Width - 650 - 16;
                pnlToolbarConsulta.Controls.Add(txtBuscarConsultaCasa);
            }

            // Fila 2: badge casa seleccionada + métricas
            if (lblCasaSeleccionada != null)
            {
                if (lblCasaSeleccionada.Parent != null) lblCasaSeleccionada.Parent.Controls.Remove(lblCasaSeleccionada);
                lblCasaSeleccionada.Location = new Point(0, 46);
                lblCasaSeleccionada.Width = 360;
                pnlToolbarConsulta.Controls.Add(lblCasaSeleccionada);
            }
            if (lblPrototipoConsulta != null)
            {
                if (lblPrototipoConsulta.Parent != null) lblPrototipoConsulta.Parent.Controls.Remove(lblPrototipoConsulta);
                lblPrototipoConsulta.Location = new Point(376, 50);
                lblPrototipoConsulta.AutoSize = true;
                pnlToolbarConsulta.Controls.Add(lblPrototipoConsulta);
            }
            if (lblTotalInsumosConsulta != null)
            {
                if (lblTotalInsumosConsulta.Parent != null) lblTotalInsumosConsulta.Parent.Controls.Remove(lblTotalInsumosConsulta);
                lblTotalInsumosConsulta.Location = new Point(0, 88);
                lblTotalInsumosConsulta.AutoSize = true;
                pnlToolbarConsulta.Controls.Add(lblTotalInsumosConsulta);
            }
            if (lblTotalImporteConsulta != null)
            {
                if (lblTotalImporteConsulta.Parent != null) lblTotalImporteConsulta.Parent.Controls.Remove(lblTotalImporteConsulta);
                lblTotalImporteConsulta.Location = new Point(220, 88);
                lblTotalImporteConsulta.AutoSize = true;
                pnlToolbarConsulta.Controls.Add(lblTotalImporteConsulta);
            }
            if (lblDiscrepanciasConsulta != null)
            {
                if (lblDiscrepanciasConsulta.Parent != null) lblDiscrepanciasConsulta.Parent.Controls.Remove(lblDiscrepanciasConsulta);
                lblDiscrepanciasConsulta.Location = new Point(460, 88);
                lblDiscrepanciasConsulta.AutoSize = true;
                pnlToolbarConsulta.Controls.Add(lblDiscrepanciasConsulta);
            }

            // Grid
            if (dgvConsultaCasa != null)
            {
                if (dgvConsultaCasa.Parent != null) dgvConsultaCasa.Parent.Controls.Remove(dgvConsultaCasa);
                dgvConsultaCasa.Dock = DockStyle.Fill;
                dgvConsultaCasa.Margin = new Padding(0);
                pnlGridConsultaContent.Controls.Add(dgvConsultaCasa);
            }

            // Action bar
            if (btnActualizarConsultaCasa != null)
            {
                if (btnActualizarConsultaCasa.Parent != null) btnActualizarConsultaCasa.Parent.Controls.Remove(btnActualizarConsultaCasa);
                btnActualizarConsultaCasa.Dock = DockStyle.Left;
                btnActualizarConsultaCasa.Width = 180;
                btnActualizarConsultaCasa.Height = 40;
                pnlActionsConsulta.Controls.Add(btnActualizarConsultaCasa);
            }
            if (btnExportarConsultaCasa != null)
            {
                if (btnExportarConsultaCasa.Parent != null) btnExportarConsultaCasa.Parent.Controls.Remove(btnExportarConsultaCasa);
                btnExportarConsultaCasa.Dock = DockStyle.Right;
                btnExportarConsultaCasa.Width = 220;
                btnExportarConsultaCasa.Height = 40;
                pnlActionsConsulta.Controls.Add(btnExportarConsultaCasa);
            }

            tabPageConsultaCasa.Controls.Add(pnlToolbarConsulta);
            tabPageConsultaCasa.Controls.Add(spacerCsq);
            tabPageConsultaCasa.Controls.Add(pnlActionsConsulta);
            tabPageConsultaCasa.Controls.Add(pnlGridConsultaContent);
            pnlGridConsultaContent.BringToFront();

            tabPageConsultaCasa.ResumeLayout(true);
        }

        // ===================================================================
        // TAB 5 — HISTORIAL
        // ===================================================================
        private void ConfigurarLayoutHistorial()
        {
            if (tabPageHistorial == null) return;
            tabPageHistorial.SuspendLayout();
            tabPageHistorial.Padding = new Padding(CardGap);
            tabPageHistorial.BackColor = ThemeManager.ColorFondoApp;

            pnlHistorialContent = CrearCard("pnlHistorialContent", new Padding(0));
            pnlHistorialContent.Dock = DockStyle.Fill;

            if (objectListViewHistorial != null)
            {
                if (objectListViewHistorial.Parent != null) objectListViewHistorial.Parent.Controls.Remove(objectListViewHistorial);
                objectListViewHistorial.Dock = DockStyle.Fill;
                pnlHistorialContent.Controls.Add(objectListViewHistorial);
            }

            tabPageHistorial.Controls.Add(pnlHistorialContent);
            tabPageHistorial.ResumeLayout(true);
        }

        // ===================================================================
        // UTILIDADES DE SIDEBAR
        // ===================================================================
        private static Label CrearTituloSeccion(string texto)
        {
            return new Label
            {
                Text = texto,
                Font = new Font(ThemeManager.FuenteSemi.FontFamily, 7.75F, FontStyle.Bold),
                ForeColor = ThemeManager.ColorTextoSutil,
                BackColor = Color.Transparent,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 4)
            };
        }

        private void AgregarSidebar(Panel sidebar, Control control, ref int y, int separacion = FieldSpacing)
        {
            if (sidebar == null || control == null) return;
            if (control.Parent != sidebar)
            {
                if (control.Parent != null) control.Parent.Controls.Remove(control);
                sidebar.Controls.Add(control);
            }

            int innerWidth = sidebar.Width - SidebarPadding * 2;
            control.Location = new Point(SidebarPadding, y + SidebarPadding);

            // Para controles full-width
            if (control is TextBox || control is ComboBox || control is Button || control is FlowLayoutPanel)
            {
                control.Width = innerWidth;
                control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }
            else if (control is Label lbl && !lbl.AutoSize)
            {
                control.Width = innerWidth;
                control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }
            else
            {
                if (control is Label lbl2)
                {
                    lbl2.MaximumSize = new Size(innerWidth, 0);
                }
            }

            y += control.Height + separacion;
        }

        private static void ConfigurarTamanoCampo(Control c)
        {
            if (c == null) return;
            c.Height = 28;
        }

        private static void ConfigurarTamanoBoton(Control c, int alto)
        {
            if (c == null) return;
            c.Height = alto;
        }

        private static void OcultarSiNoUsado(Control c)
        {
            if (c == null) return;
            c.Visible = false;
        }

        // ===================================================================
        // RESIZE — al cambiar tamaño del form, ajustar anchos relativos
        // ===================================================================
        public void FormAlmacen_Resize(object sender, EventArgs e)
        {
            // El layout es fundamentalmente basado en Dock + Anchor, así que el
            // ajuste es automático. Solo refinamos textboxes con Anchor manual.
            if (pnlToolbarInventario != null && txtBuscarInventario != null && txtBuscarInventario.Parent == pnlToolbarInventario)
            {
                txtBuscarInventario.Width = pnlToolbarInventario.ClientSize.Width - 70 - 32;
            }
            if (pnlToolbarConsulta != null && txtBuscarConsultaCasa != null && txtBuscarConsultaCasa.Parent == pnlToolbarConsulta)
            {
                txtBuscarConsultaCasa.Width = pnlToolbarConsulta.ClientSize.Width - 650 - 16;
            }
        }
    }
}
