using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Sistema de tematizacion corporativo Calandria.
    /// Paleta cafe refinada + superficies calidas + tipografia Segoe UI + render suavizado.
    /// </summary>
    public static class ThemeManager
    {
        // =====================================================================
        // PALETA CORPORATIVA - CAFE REFINADO
        // =====================================================================

        // --- Cafe corporativo ---
        public static readonly Color ColorPrincipal        = Color.FromArgb(88, 53, 23);    // #583517
        public static readonly Color ColorPrincipalMenuBar = Color.FromArgb(179, 108, 46);  // #B36C2E
        public static readonly Color ColorPrincipalClaro   = Color.FromArgb(196, 138, 84);
        public static readonly Color ColorPrincipalOscuro  = Color.FromArgb(60, 36, 15);
        public static readonly Color ColorPrincipalHover   = Color.FromArgb(155, 92, 38);
        public static readonly Color ColorPrincipalPressed = Color.FromArgb(132, 78, 32);

        // --- Neutros calidos (armonizan con cafe) ---
        public static readonly Color ColorSecundario      = Color.FromArgb(104, 106, 110);
        public static readonly Color ColorSecundarioClaro = Color.FromArgb(150, 152, 154);
        public static readonly Color ColorBorde           = Color.FromArgb(224, 220, 213);
        public static readonly Color ColorBordeFuerte     = Color.FromArgb(196, 190, 180);
        public static readonly Color ColorDivisor         = Color.FromArgb(238, 234, 226);

        // --- Superficies ---
        public static readonly Color ColorFondo        = Color.White;
        public static readonly Color ColorFondoAlterno = Color.FromArgb(252, 250, 246);
        public static readonly Color ColorFondoApp     = Color.FromArgb(248, 246, 242);
        public static readonly Color ColorFondoHover   = Color.FromArgb(245, 238, 228);
        public static readonly Color ColorFondoCard    = Color.White;

        // --- Estados (refinados, profesionales) ---
        public static readonly Color ColorExito            = Color.FromArgb(46, 160, 89);
        public static readonly Color ColorExitoSuave       = Color.FromArgb(232, 245, 233);
        public static readonly Color ColorAdvertencia      = Color.FromArgb(217, 142, 24);
        public static readonly Color ColorAdvertenciaSuave = Color.FromArgb(253, 244, 224);
        public static readonly Color ColorError            = Color.FromArgb(198, 65, 56);
        public static readonly Color ColorErrorSuave       = Color.FromArgb(251, 232, 229);
        public static readonly Color ColorInfo             = Color.FromArgb(45, 130, 192);
        public static readonly Color ColorInfoSuave        = Color.FromArgb(228, 240, 250);

        // --- Texto ---
        public static readonly Color ColorTextoOscuro     = Color.FromArgb(38, 32, 26);
        public static readonly Color ColorTextoClaro      = Color.White;
        public static readonly Color ColorTextoSecundario = Color.FromArgb(115, 110, 102);
        public static readonly Color ColorTextoSutil      = Color.FromArgb(160, 154, 144);

        // =====================================================================
        // TIPOGRAFIA
        // =====================================================================
        private static readonly FontFamily FamiliaBase = SystemFonts.MessageBoxFont != null
            ? SystemFonts.MessageBoxFont.FontFamily
            : new FontFamily("Segoe UI");

        public static readonly Font FuenteRegular    = new Font(FamiliaBase,  9.0F, FontStyle.Regular);
        public static readonly Font FuenteSemi       = new Font(FamiliaBase,  9.0F, FontStyle.Bold);
        public static readonly Font FuenteBold       = new Font(FamiliaBase,  9.5F, FontStyle.Bold);
        public static readonly Font FuenteTitulo     = new Font(FamiliaBase, 16.0F, FontStyle.Bold);
        public static readonly Font FuenteSubtitulo  = new Font(FamiliaBase, 11.0F, FontStyle.Bold);
        public static readonly Font FuenteEtiqueta   = new Font(FamiliaBase,  8.25F, FontStyle.Regular);
        public static readonly Font FuenteBotones    = new Font(FamiliaBase,  9.25F, FontStyle.Bold);
        public static readonly Font FuenteGrilla     = new Font(FamiliaBase,  9.0F, FontStyle.Regular);
        public static readonly Font FuenteCabecera   = new Font(FamiliaBase,  9.25F, FontStyle.Bold);

        // =====================================================================
        // MARCADORES INTERNOS (evita doble-aplicacion de handlers)
        // =====================================================================
        private const string MarcaTema     = "__theme_applied__";
        private const string MarcaBotonHov = "__theme_btn_hover__";
        private const string MarcaTabPaint = "__theme_tab_paint__";
        private const string MarcaTxtPaint = "__theme_txt_paint__";

        // =====================================================================
        // INICIALIZACION GLOBAL DE LA APLICACION
        // =====================================================================
        /// <summary>
        /// Configura defaults globales (texto suavizado, render). Llamar una vez en Program.Main.
        /// </summary>
        public static void InicializarAplicacion()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
            }
            catch { /* ya inicializado */ }
        }

        // =====================================================================
        // ENTRY POINTS - MATERIAL FORM
        // =====================================================================
        public static void AplicarTemaMaterial(MaterialForm form)
        {
            if (form == null) return;

            var skin = MaterialSkinManager.Instance;
            skin.AddFormToManage(form);
            skin.Theme = MaterialSkinManager.Themes.LIGHT;

            skin.ColorScheme = new ColorScheme(
                Primary.Brown800,
                Primary.Brown900,
                Primary.Brown600,
                Accent.Orange200,
                TextShade.WHITE);

            AplicarTema(form);
        }

        // =====================================================================
        // ENTRY POINT - FORMULARIO NORMAL
        // =====================================================================
        public static void AplicarTema(Form form)
        {
            if (form == null) return;

            // Atributos a nivel de form
            form.BackColor = ColorFondoApp;
            form.ForeColor = ColorTextoOscuro;
            try { form.Font = FuenteRegular; } catch { }

            HabilitarDoubleBuffer(form);

            AplicarTemaRecursivo(form);
        }

        private static void AplicarTemaRecursivo(Control contenedor)
        {
            if (contenedor == null) return;
            foreach (Control control in contenedor.Controls)
            {
                AplicarTemaControl(control);
                if (control.HasChildren) AplicarTemaRecursivo(control);
            }
        }

        // =====================================================================
        // DISPATCH POR TIPO DE CONTROL
        // =====================================================================
        public static void AplicarTemaControl(Control control)
        {
            if (control == null) return;

            switch (control)
            {
                case Button btn:           EstilizarBoton(btn); break;
                case CheckBox chk:         EstilizarCheckBox(chk); break;
                case RadioButton rad:      EstilizarRadioButton(rad); break;
                case Panel panel:          EstilizarPanel(panel); break;
                case GroupBox grp:         EstilizarGroupBox(grp); break;
                case LinkLabel lnk:        EstilizarLinkLabel(lnk); break;
                case Label lbl:            EstilizarLabel(lbl); break;
                case TextBox txt:          EstilizarTextBox(txt); break;
                case ComboBox cbo:         EstilizarComboBox(cbo); break;
                case ListBox lst:          EstilizarListBox(lst); break;
                case ListView lv:          EstilizarListView(lv); break;
                case TreeView tv:          EstilizarTreeView(tv); break;
                case DataGridView dgv:     EstilizarDataGridView(dgv); break;
                case ProgressBar pb:       EstilizarProgressBar(pb); break;
                case MenuStrip ms:         EstilizarMenuStrip(ms); break;
                case StatusStrip ss:       EstilizarStatusStrip(ss); break;
                case ToolStrip ts:         EstilizarToolStrip(ts); break;
                case TabControl tc:        EstilizarTabControl(tc); break;
                case NumericUpDown nud:    EstilizarNumericUpDown(nud); break;
                case DateTimePicker dtp:   EstilizarDateTimePicker(dtp); break;
                case SplitContainer sp:    EstilizarSplitContainer(sp); break;
            }
        }

        // =====================================================================
        // BOTONES
        // =====================================================================
        private static void EstilizarBoton(Button btn)
        {
            // Respetar botones ya estilizados como secundario/exito/peligro/outline
            if (btn.Tag is string tag &&
                (tag == "secundario" || tag == "exito" || tag == "peligro" ||
                 tag == "outline" || tag == "icono" || tag == "ghost"))
                return;

            btn.BackColor = ColorPrincipalMenuBar;
            btn.ForeColor = ColorTextoClaro;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ColorPrincipalHover;
            btn.FlatAppearance.MouseDownBackColor = ColorPrincipalPressed;
            btn.Cursor = Cursors.Hand;
            btn.Font = FuenteBotones;
            btn.UseVisualStyleBackColor = false;
            // Padding interno para sensacion mas aireada
            try { btn.Padding = new Padding(btn.Padding.Left, btn.Padding.Top, btn.Padding.Right, btn.Padding.Bottom); } catch { }
        }

        private static void EstilizarCheckBox(CheckBox chk)
        {
            chk.ForeColor = ColorTextoOscuro;
            chk.BackColor = Color.Transparent;
            chk.Font = FuenteRegular;
            chk.FlatStyle = FlatStyle.Flat;
            chk.FlatAppearance.CheckedBackColor = ColorPrincipalSuaveValor;
            chk.FlatAppearance.BorderColor = ColorBorde;
            chk.Cursor = Cursors.Hand;
        }

        private static void EstilizarRadioButton(RadioButton rad)
        {
            rad.ForeColor = ColorTextoOscuro;
            rad.BackColor = Color.Transparent;
            rad.Font = FuenteRegular;
            rad.FlatStyle = FlatStyle.Flat;
            rad.FlatAppearance.CheckedBackColor = ColorPrincipalSuaveValor;
            rad.FlatAppearance.BorderColor = ColorBorde;
            rad.Cursor = Cursors.Hand;
        }

        // Internal helper (kept as method to avoid forward-init issue with readonly Color)
        private static Color ColorPrincipalSuaveValor => Color.FromArgb(247, 240, 232);

        private static bool EsColorOscuro(Color c)
        {
            // Luminancia perceptual ITU-R BT.601
            double luma = (c.R * 0.299) + (c.G * 0.587) + (c.B * 0.114);
            return luma < 128;
        }

        // =====================================================================
        // PANELES (sensibles a nombre: Header/Footer/Sidebar/Card/Surface)
        // =====================================================================
        private static void EstilizarPanel(Panel panel)
        {
            string n = panel.Name ?? string.Empty;

            // Heuristica: panel anclado al tope con fondo oscuro = banner/header
            bool esBannerOscuro = panel.Dock == DockStyle.Top
                                  && panel.Height <= 90
                                  && EsColorOscuro(panel.BackColor)
                                  && panel.BackColor != ColorPrincipalMenuBar
                                  && panel.BackColor != ColorPrincipal;

            if (n.IndexOf("Header", StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("Title",  StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("TopBar", StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("Banner", StringComparison.OrdinalIgnoreCase) >= 0
                || (n.IndexOf("Top",   StringComparison.OrdinalIgnoreCase) >= 0 && n.Length <= 8)
                || esBannerOscuro)
            {
                panel.BackColor = ColorPrincipalMenuBar;
                panel.ForeColor = ColorTextoClaro;
            }
            else if (n.IndexOf("Footer", StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Bottom", StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Status", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                panel.BackColor = ColorFondoAlterno;
                panel.ForeColor = ColorTextoSecundario;
            }
            else if (n.IndexOf("Sidebar", StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Nav",     StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Menu",    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                panel.BackColor = ColorPrincipal;
                panel.ForeColor = ColorTextoClaro;
            }
            else if (n.IndexOf("Card",    StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Surface", StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Tarjeta", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                EstilizarCard(panel);
                return;
            }
            else if (n.IndexOf("Toolbar", StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Acciones",StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Actions", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                panel.BackColor = ColorFondo;
                panel.ForeColor = ColorTextoOscuro;
            }
            else
            {
                // Panel generico: hereda fondo de app para sensacion fluida
                if (panel.BackColor == SystemColors.Control)
                    panel.BackColor = ColorFondoApp;
                panel.ForeColor = ColorTextoOscuro;
            }

            HabilitarDoubleBuffer(panel);
        }

        // =====================================================================
        // GROUPBOX
        // =====================================================================
        private static void EstilizarGroupBox(GroupBox groupBox)
        {
            groupBox.ForeColor = ColorPrincipalMenuBar;
            groupBox.BackColor = Color.Transparent;
            groupBox.Font = FuenteSubtitulo;
        }

        // =====================================================================
        // LABELS
        // =====================================================================
        private static void EstilizarLabel(Label label)
        {
            // Texto en superficies oscuras
            if (label.Parent is Panel p &&
                (p.BackColor == ColorPrincipal ||
                 p.BackColor == ColorPrincipalMenuBar ||
                 p.BackColor == ColorPrincipalOscuro))
            {
                label.ForeColor = ColorTextoClaro;
                label.BackColor = Color.Transparent;
            }
            else
            {
                label.ForeColor = ColorTextoOscuro;
                label.BackColor = Color.Transparent;
            }

            string n = label.Name ?? string.Empty;
            if (n.IndexOf("Titulo", StringComparison.OrdinalIgnoreCase) >= 0
             || n.IndexOf("Title",  StringComparison.OrdinalIgnoreCase) >= 0
             || n.IndexOf("Header", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (label.Parent is Panel pp && pp.BackColor == ColorPrincipalMenuBar)
                {
                    label.ForeColor = ColorTextoClaro;
                    label.Font = FuenteTitulo;
                }
                else
                {
                    label.ForeColor = ColorPrincipalOscuro;
                    label.Font = FuenteTitulo;
                }
            }
            else if (n.IndexOf("Subtitulo", StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Subtitle",  StringComparison.OrdinalIgnoreCase) >= 0)
            {
                label.ForeColor = ColorTextoSecundario;
                label.Font = FuenteSubtitulo;
            }
            else if (n.IndexOf("Hint",  StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Help",  StringComparison.OrdinalIgnoreCase) >= 0
                  || n.IndexOf("Sutil", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                label.ForeColor = ColorTextoSutil;
                label.Font = FuenteEtiqueta;
            }
        }

        private static void EstilizarLinkLabel(LinkLabel lnk)
        {
            lnk.LinkColor = ColorPrincipalMenuBar;
            lnk.ActiveLinkColor = ColorPrincipalHover;
            lnk.VisitedLinkColor = ColorPrincipalOscuro;
            lnk.LinkBehavior = LinkBehavior.HoverUnderline;
            lnk.BackColor = Color.Transparent;
            lnk.Font = FuenteRegular;
            lnk.Cursor = Cursors.Hand;
        }

        // =====================================================================
        // CAMPOS DE TEXTO
        // =====================================================================
        private static void EstilizarTextBox(TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = ColorFondo;
            textBox.ForeColor = ColorTextoOscuro;
            textBox.Font = FuenteRegular;

            // Focus highlight via Enter/Leave (sin doble registro)
            if (textBox.Tag as string != MarcaTxtPaint)
            {
                textBox.Enter += (s, e) =>
                {
                    if (s is TextBox t) t.BackColor = Color.FromArgb(252, 248, 240);
                };
                textBox.Leave += (s, e) =>
                {
                    if (s is TextBox t) t.BackColor = ColorFondo;
                };
                textBox.Tag = MarcaTxtPaint;
            }
        }

        private static void EstilizarComboBox(ComboBox comboBox)
        {
            comboBox.BackColor = ColorFondo;
            comboBox.ForeColor = ColorTextoOscuro;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.Font = FuenteRegular;
        }

        private static void EstilizarListBox(ListBox lst)
        {
            lst.BorderStyle = BorderStyle.FixedSingle;
            lst.BackColor = ColorFondo;
            lst.ForeColor = ColorTextoOscuro;
            lst.Font = FuenteRegular;
        }

        private static void EstilizarListView(ListView lv)
        {
            lv.BackColor = ColorFondo;
            lv.ForeColor = ColorTextoOscuro;
            lv.Font = FuenteRegular;
            lv.GridLines = false;
            lv.FullRowSelect = true;
            lv.BorderStyle = BorderStyle.FixedSingle;
            HabilitarDoubleBuffer(lv);
        }

        private static void EstilizarTreeView(TreeView tv)
        {
            tv.BackColor = ColorFondo;
            tv.ForeColor = ColorTextoOscuro;
            tv.Font = FuenteRegular;
            tv.BorderStyle = BorderStyle.FixedSingle;
            tv.ShowLines = false;
            tv.ShowPlusMinus = true;
            tv.HideSelection = false;
            HabilitarDoubleBuffer(tv);
        }

        private static void EstilizarNumericUpDown(NumericUpDown nud)
        {
            nud.BorderStyle = BorderStyle.FixedSingle;
            nud.BackColor = ColorFondo;
            nud.ForeColor = ColorTextoOscuro;
            nud.Font = FuenteRegular;
        }

        private static void EstilizarDateTimePicker(DateTimePicker dtp)
        {
            dtp.CalendarMonthBackground = ColorFondo;
            dtp.CalendarTitleBackColor = ColorPrincipalMenuBar;
            dtp.CalendarTitleForeColor = ColorTextoClaro;
            dtp.CalendarTrailingForeColor = ColorTextoSutil;
            dtp.Font = FuenteRegular;
        }

        // =====================================================================
        // DATAGRIDVIEW (joya de la corona - donde mas se siente "profesional")
        // =====================================================================
        private static void EstilizarDataGridView(DataGridView dgv)
        {
            HabilitarDoubleBuffer(dgv);

            dgv.BackgroundColor = ColorFondo;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = ColorDivisor;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.Font = FuenteGrilla;

            // Cabecera
            dgv.ColumnHeadersHeight = 38;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            var ch = dgv.ColumnHeadersDefaultCellStyle;
            ch.BackColor = ColorPrincipalMenuBar;
            ch.ForeColor = ColorTextoClaro;
            ch.SelectionBackColor = ColorPrincipalMenuBar;
            ch.SelectionForeColor = ColorTextoClaro;
            ch.Font = FuenteCabecera;
            ch.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ch.Padding = new Padding(8, 0, 8, 0);
            ch.WrapMode = DataGridViewTriState.False;

            // Filas
            dgv.RowTemplate.Height = 32;
            var rc = dgv.DefaultCellStyle;
            rc.BackColor = ColorFondo;
            rc.ForeColor = ColorTextoOscuro;
            rc.SelectionBackColor = Color.FromArgb(232, 220, 200);
            rc.SelectionForeColor = ColorPrincipalOscuro;
            rc.Font = FuenteGrilla;
            rc.Padding = new Padding(8, 0, 8, 0);
            rc.WrapMode = DataGridViewTriState.False;

            var ar = dgv.AlternatingRowsDefaultCellStyle;
            ar.BackColor = ColorFondoAlterno;
            ar.ForeColor = ColorTextoOscuro;
            ar.SelectionBackColor = Color.FromArgb(232, 220, 200);
            ar.SelectionForeColor = ColorPrincipalOscuro;
        }

        // =====================================================================
        // PROGRESS BAR
        // =====================================================================
        private static void EstilizarProgressBar(ProgressBar pb)
        {
            pb.ForeColor = ColorPrincipalMenuBar;
            pb.BackColor = ColorDivisor;
        }

        // =====================================================================
        // MENU STRIP / TOOL STRIP / STATUS STRIP
        // =====================================================================
        private static void EstilizarMenuStrip(MenuStrip ms)
        {
            ms.BackColor = ColorPrincipalMenuBar;
            ms.ForeColor = ColorTextoClaro;
            ms.Font = FuenteSemi;
            ms.RenderMode = ToolStripRenderMode.Professional;
            ms.Renderer = new TemaToolStripRenderer();

            foreach (ToolStripMenuItem item in ms.Items)
                EstilizarMenuStripItem(item);
        }

        private static void EstilizarMenuStripItem(ToolStripMenuItem item)
        {
            item.BackColor = ColorPrincipalMenuBar;
            item.ForeColor = ColorTextoClaro;
            item.Font = FuenteSemi;

            foreach (ToolStripItem sub in item.DropDownItems)
            {
                sub.BackColor = ColorFondo;
                sub.ForeColor = ColorTextoOscuro;
                sub.Font = FuenteRegular;
                if (sub is ToolStripMenuItem subItem)
                    EstilizarMenuStripItem(subItem);
            }
        }

        private static void EstilizarToolStrip(ToolStrip ts)
        {
            ts.BackColor = ColorFondo;
            ts.ForeColor = ColorTextoOscuro;
            ts.Font = FuenteRegular;
            ts.RenderMode = ToolStripRenderMode.Professional;
            ts.Renderer = new TemaToolStripRenderer();
            ts.GripStyle = ToolStripGripStyle.Hidden;
        }

        private static void EstilizarStatusStrip(StatusStrip ss)
        {
            ss.BackColor = ColorFondoAlterno;
            ss.ForeColor = ColorTextoSecundario;
            ss.Font = FuenteEtiqueta;
            ss.SizingGrip = false;
        }

        // =====================================================================
        // TAB CONTROL (custom painting con acento corporativo)
        // =====================================================================
        private static void EstilizarTabControl(TabControl tabControl)
        {
            if (tabControl.Tag as string == MarcaTabPaint) return; // evita doble-registro
            tabControl.Tag = MarcaTabPaint;

            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.SizeMode = TabSizeMode.Normal;
            tabControl.ItemSize = new Size(140, 34);
            tabControl.Padding = new Point(14, 6);
            tabControl.Font = FuenteSemi;

            tabControl.DrawItem += (s, e) =>
            {
                if (!(s is TabControl tc)) return;
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                TabPage tp = tc.TabPages[e.Index];
                Rectangle bounds = tc.GetTabRect(e.Index);
                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

                // Fondo de la pestana
                Color bg = selected ? ColorFondo : ColorFondoApp;
                using (var b = new SolidBrush(bg))
                    g.FillRectangle(b, bounds);

                // Acento inferior cafe en seleccion
                if (selected)
                {
                    using (var b = new SolidBrush(ColorPrincipalMenuBar))
                        g.FillRectangle(b, bounds.Left, bounds.Bottom - 3, bounds.Width, 3);
                }
                else
                {
                    using (var p = new Pen(ColorDivisor))
                        g.DrawLine(p, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
                }

                // Texto
                Color textColor = selected ? ColorPrincipalOscuro : ColorTextoSecundario;
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using (var tb = new SolidBrush(textColor))
                    g.DrawString(tp.Text, tc.Font, tb, bounds, sf);
            };

            // Color de fondo del area del tab para que se funda con la pestana seleccionada
            foreach (TabPage page in tabControl.TabPages)
            {
                page.BackColor = ColorFondo;
                page.ForeColor = ColorTextoOscuro;
                page.Padding = new Padding(12);
            }
        }

        // =====================================================================
        // SPLIT CONTAINER
        // =====================================================================
        private static void EstilizarSplitContainer(SplitContainer sp)
        {
            sp.BackColor = ColorDivisor;
            sp.Panel1.BackColor = ColorFondoApp;
            sp.Panel2.BackColor = ColorFondoApp;
            sp.SplitterWidth = 4;
        }

        // =====================================================================
        // HELPERS PUBLICOS - VARIANTES DE BOTON
        // =====================================================================
        public static void EstilizarBotonSecundario(Button btn)
        {
            btn.Tag = "secundario";
            btn.BackColor = ColorFondo;
            btn.ForeColor = ColorPrincipalOscuro;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = ColorBordeFuerte;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = ColorFondoHover;
            btn.FlatAppearance.MouseDownBackColor = ColorPrincipalSuaveValor;
            btn.Cursor = Cursors.Hand;
            btn.Font = FuenteBotones;
            btn.UseVisualStyleBackColor = false;
        }

        public static void EstilizarBotonOutline(Button btn)
        {
            btn.Tag = "outline";
            btn.BackColor = ColorFondo;
            btn.ForeColor = ColorPrincipalMenuBar;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = ColorPrincipalMenuBar;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = ColorPrincipalSuaveValor;
            btn.FlatAppearance.MouseDownBackColor = ColorFondoHover;
            btn.Cursor = Cursors.Hand;
            btn.Font = FuenteBotones;
            btn.UseVisualStyleBackColor = false;
        }

        public static void EstilizarBotonExito(Button btn)
        {
            btn.Tag = "exito";
            btn.BackColor = ColorExito;
            btn.ForeColor = ColorTextoClaro;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 175, 106);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(36, 138, 74);
            btn.Cursor = Cursors.Hand;
            btn.Font = FuenteBotones;
            btn.UseVisualStyleBackColor = false;
        }

        public static void EstilizarBotonPeligro(Button btn)
        {
            btn.Tag = "peligro";
            btn.BackColor = ColorError;
            btn.ForeColor = ColorTextoClaro;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(214, 86, 78);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(172, 56, 48);
            btn.Cursor = Cursors.Hand;
            btn.Font = FuenteBotones;
            btn.UseVisualStyleBackColor = false;
        }

        public static void EstilizarBotonGhost(Button btn)
        {
            btn.Tag = "ghost";
            btn.BackColor = Color.Transparent;
            btn.ForeColor = ColorPrincipalOscuro;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ColorFondoHover;
            btn.FlatAppearance.MouseDownBackColor = ColorPrincipalSuaveValor;
            btn.Cursor = Cursors.Hand;
            btn.Font = FuenteBotones;
            btn.UseVisualStyleBackColor = false;
        }

        // =====================================================================
        // HELPERS - PANELES Y SUPERFICIES
        // =====================================================================
        /// <summary>
        /// Convierte un Panel en tarjeta blanca con borde calido y padding interno.
        /// </summary>
        public static void EstilizarCard(Panel panel)
        {
            if (panel == null) return;
            panel.BackColor = ColorFondoCard;
            panel.ForeColor = ColorTextoOscuro;
            panel.BorderStyle = BorderStyle.None;
            panel.Padding = new Padding(16);
            HabilitarDoubleBuffer(panel);
            DibujarBordeSuave(panel);
        }

        /// <summary>
        /// Panel de encabezado: fondo cafe claro corporativo y texto blanco.
        /// </summary>
        public static void EstilizarHeader(Panel panel)
        {
            if (panel == null) return;
            panel.BackColor = ColorPrincipalMenuBar;
            panel.ForeColor = ColorTextoClaro;
            panel.Padding = new Padding(20, 12, 20, 12);
            HabilitarDoubleBuffer(panel);
        }

        /// <summary>
        /// Panel de barra lateral: fondo cafe oscuro.
        /// </summary>
        public static void EstilizarSidebar(Panel panel)
        {
            if (panel == null) return;
            panel.BackColor = ColorPrincipal;
            panel.ForeColor = ColorTextoClaro;
            panel.Padding = new Padding(0, 12, 0, 12);
            HabilitarDoubleBuffer(panel);
        }

        /// <summary>
        /// Convierte un Label en separador horizontal (linea + texto opcional).
        /// </summary>
        public static void EstilizarSeparador(Label label)
        {
            if (label == null) return;
            label.Height = 1;
            label.BackColor = ColorDivisor;
            label.Text = string.Empty;
            label.BorderStyle = BorderStyle.None;
        }

        /// <summary>
        /// Dibuja un borde suave de 1px del color corporativo neutro alrededor del panel.
        /// </summary>
        public static void DibujarBordeSuave(Panel panel)
        {
            if (panel == null) return;
            panel.Paint -= PanelBordeSuavePaint;
            panel.Paint += PanelBordeSuavePaint;
        }

        private static void PanelBordeSuavePaint(object sender, PaintEventArgs e)
        {
            if (!(sender is Panel p)) return;
            using (var pen = new Pen(ColorBorde, 1))
            {
                Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                e.Graphics.DrawRectangle(pen, r);
            }
        }

        // =====================================================================
        // PINTAR BADGES DE ESTADO (en una Label o Panel)
        // =====================================================================
        public static void EstilizarBadgeExito(Label lbl)        => EstilizarBadge(lbl, ColorExitoSuave,       ColorExito);
        public static void EstilizarBadgeAdvertencia(Label lbl)  => EstilizarBadge(lbl, ColorAdvertenciaSuave, ColorAdvertencia);
        public static void EstilizarBadgeError(Label lbl)        => EstilizarBadge(lbl, ColorErrorSuave,       ColorError);
        public static void EstilizarBadgeInfo(Label lbl)         => EstilizarBadge(lbl, ColorInfoSuave,        ColorInfo);

        private static void EstilizarBadge(Label lbl, Color fondo, Color texto)
        {
            if (lbl == null) return;
            lbl.BackColor = fondo;
            lbl.ForeColor = texto;
            lbl.Font = FuenteSemi;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Padding = new Padding(8, 3, 8, 3);
            lbl.AutoSize = false;
        }

        // =====================================================================
        // RENDIMIENTO - DOUBLE BUFFER VIA REFLECTION
        // =====================================================================
        /// <summary>
        /// Habilita double-buffering en cualquier Control (incluso DataGridView)
        /// para evitar flicker en scroll/redraw, esencial para sensacion "fluida".
        /// </summary>
        public static void HabilitarDoubleBuffer(Control control)
        {
            if (control == null) return;
            if (SystemInformation.TerminalServerSession) return; // no DB en RDP
            try
            {
                PropertyInfo prop = typeof(Control).GetProperty(
                    "DoubleBuffered",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                prop?.SetValue(control, true, null);
            }
            catch { /* ignore */ }
        }

        // =====================================================================
        // RENDERER PERSONALIZADO PARA MENU/TOOLSTRIP
        // =====================================================================
        private class TemaToolStripRenderer : ToolStripProfessionalRenderer
        {
            public TemaToolStripRenderer() : base(new TemaToolStripColors()) { }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Selected
                    ? ColorPrincipalOscuro
                    : (e.Item.Owner is MenuStrip ? ColorTextoClaro : ColorTextoOscuro);
                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // Sin borde para mantener limpieza
            }
        }

        private class TemaToolStripColors : ProfessionalColorTable
        {
            public override Color MenuItemSelected               => ColorPrincipalSuaveValor;
            public override Color MenuItemSelectedGradientBegin  => ColorPrincipalSuaveValor;
            public override Color MenuItemSelectedGradientEnd    => ColorPrincipalSuaveValor;
            public override Color MenuItemBorder                 => ColorPrincipalMenuBar;
            public override Color MenuItemPressedGradientBegin   => ColorFondoHover;
            public override Color MenuItemPressedGradientEnd     => ColorFondoHover;
            public override Color ImageMarginGradientBegin       => ColorFondo;
            public override Color ImageMarginGradientMiddle      => ColorFondo;
            public override Color ImageMarginGradientEnd         => ColorFondo;
            public override Color ToolStripDropDownBackground    => ColorFondo;
            public override Color MenuBorder                     => ColorBorde;
            public override Color SeparatorDark                  => ColorDivisor;
            public override Color SeparatorLight                 => ColorDivisor;
            public override Color ToolStripBorder                => ColorBorde;
            public override Color ToolStripGradientBegin         => ColorFondo;
            public override Color ToolStripGradientMiddle        => ColorFondo;
            public override Color ToolStripGradientEnd           => ColorFondo;
            public override Color ButtonSelectedHighlight        => ColorFondoHover;
            public override Color ButtonSelectedBorder           => ColorBordeFuerte;
            public override Color ButtonPressedHighlight         => ColorPrincipalSuaveValor;
            public override Color ButtonPressedBorder            => ColorPrincipalMenuBar;
        }
    }
}
