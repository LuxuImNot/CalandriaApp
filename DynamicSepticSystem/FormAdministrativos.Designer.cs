namespace DynamicSepticSystem
{
    partial class FormAdministrativos
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // Header / filtros
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.panelFiltros = new System.Windows.Forms.Panel();
            this.lblManzana = new System.Windows.Forms.Label();
            this.cmbManzana = new System.Windows.Forms.ComboBox();
            this.lblLote = new System.Windows.Forms.Label();
            this.cmbLote = new System.Windows.Forms.ComboBox();
            this.btnConsultar = new System.Windows.Forms.Button();
            this.btnExportarPDF = new System.Windows.Forms.Button();
            this.lblCasaInfo = new System.Windows.Forms.Label();

            // Cuerpo (split entre detalle y HUD)
            this.panelBody = new System.Windows.Forms.Panel();
            this.panelDetalle = new System.Windows.Forms.Panel();
            this.tabPrincipal = new System.Windows.Forms.TabControl();
            this.tabEstimaciones = new System.Windows.Forms.TabPage();
            this.olvEstimaciones = new BrightIdeasSoftware.ObjectListView();
            this.colEtapa = new BrightIdeasSoftware.OLVColumn();
            this.colWBS = new BrightIdeasSoftware.OLVColumn();
            this.colCodigo = new BrightIdeasSoftware.OLVColumn();
            this.colPartida = new BrightIdeasSoftware.OLVColumn();
            this.colAvance = new BrightIdeasSoftware.OLVColumn();
            this.colMontoEjecutado = new BrightIdeasSoftware.OLVColumn();
            this.colFechaFin = new BrightIdeasSoftware.OLVColumn();
            this.tabDestajos = new System.Windows.Forms.TabPage();
            this.panelStepperDestajos = new System.Windows.Forms.Panel();
            this.olvDestajos = new BrightIdeasSoftware.ObjectListView();
            this.colDesCategoria = new BrightIdeasSoftware.OLVColumn();
            this.colDesID = new BrightIdeasSoftware.OLVColumn();
            this.colDesTipo = new BrightIdeasSoftware.OLVColumn();
            this.colDesTarea = new BrightIdeasSoftware.OLVColumn();
            this.colDesCantidad = new BrightIdeasSoftware.OLVColumn();
            this.colDesUnidad = new BrightIdeasSoftware.OLVColumn();
            this.colDesPrecioUnitario = new BrightIdeasSoftware.OLVColumn();
            this.colDesImporte = new BrightIdeasSoftware.OLVColumn();
            this.colDesGastado = new BrightIdeasSoftware.OLVColumn();
            this.colDesCuadrilla = new BrightIdeasSoftware.OLVColumn();
            this.colDesEstado = new BrightIdeasSoftware.OLVColumn();

            // HUD lateral
            this.panelHUD = new System.Windows.Forms.Panel();
            this.lblHudTitle = new System.Windows.Forms.Label();

            // Card: Estimaciones
            this.cardEstimaciones = new System.Windows.Forms.Panel();
            this.lblCardEstTitulo = new System.Windows.Forms.Label();
            this.lblCardEstValor = new System.Windows.Forms.Label();
            this.lblCardEstInfo = new System.Windows.Forms.Label();

            // Card: Destajos contenedor
            this.cardDestajos = new System.Windows.Forms.Panel();
            this.lblCardDesTitulo = new System.Windows.Forms.Label();
            this.lblCardDesValor = new System.Windows.Forms.Label();
            this.lblCardDesInfo = new System.Windows.Forms.Label();

            // Subcard: Mano de Obra
            this.cardMO = new System.Windows.Forms.Panel();
            this.lblMOTitulo = new System.Windows.Forms.Label();
            this.lblMOValor = new System.Windows.Forms.Label();
            this.lblMOInfo = new System.Windows.Forms.Label();
            this.barraMO = new System.Windows.Forms.Panel();

            // Subcard: Material
            this.cardMat = new System.Windows.Forms.Panel();
            this.lblMatTitulo = new System.Windows.Forms.Label();
            this.lblMatValor = new System.Windows.Forms.Label();
            this.lblMatInfo = new System.Windows.Forms.Label();
            this.barraMat = new System.Windows.Forms.Panel();

            // Footer / Gran Total
            this.panelFooter = new System.Windows.Forms.Panel();
            this.lblGranTotal = new System.Windows.Forms.Label();
            this.lblGranTotalTitulo = new System.Windows.Forms.Label();

            this.panelHeader.SuspendLayout();
            this.panelFiltros.SuspendLayout();
            this.panelBody.SuspendLayout();
            this.panelDetalle.SuspendLayout();
            this.tabPrincipal.SuspendLayout();
            this.tabEstimaciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvEstimaciones)).BeginInit();
            this.tabDestajos.SuspendLayout();
            this.panelStepperDestajos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvDestajos)).BeginInit();
            this.panelHUD.SuspendLayout();
            this.cardEstimaciones.SuspendLayout();
            this.cardDestajos.SuspendLayout();
            this.cardMO.SuspendLayout();
            this.cardMat.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();

            // =================== HEADER ===================
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(88, 53, 23);
            this.panelHeader.Controls.Add(this.lblSubtitulo);
            this.panelHeader.Controls.Add(this.lblTitulo);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1200, 70);
            this.panelHeader.TabIndex = 0;

            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(700, 30);
            this.lblTitulo.Text = "ADMINISTRATIVOS — Consulta por Casa";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(230, 215, 195);
            this.lblSubtitulo.Location = new System.Drawing.Point(22, 40);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(700, 22);
            this.lblSubtitulo.Text = "Concentrado financiero por Manzana / Lote · Estimaciones y Destajos";
            this.lblSubtitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // =================== FILTROS ===================
            this.panelFiltros.BackColor = System.Drawing.Color.FromArgb(245, 240, 232);
            this.panelFiltros.Controls.Add(this.lblCasaInfo);
            this.panelFiltros.Controls.Add(this.btnExportarPDF);
            this.panelFiltros.Controls.Add(this.btnConsultar);
            this.panelFiltros.Controls.Add(this.cmbLote);
            this.panelFiltros.Controls.Add(this.lblLote);
            this.panelFiltros.Controls.Add(this.cmbManzana);
            this.panelFiltros.Controls.Add(this.lblManzana);
            this.panelFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltros.Location = new System.Drawing.Point(0, 70);
            this.panelFiltros.Name = "panelFiltros";
            this.panelFiltros.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.panelFiltros.Size = new System.Drawing.Size(1200, 80);
            this.panelFiltros.TabIndex = 1;

            this.lblManzana.AutoSize = true;
            this.lblManzana.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblManzana.ForeColor = System.Drawing.Color.FromArgb(60, 36, 15);
            this.lblManzana.Location = new System.Drawing.Point(25, 18);
            this.lblManzana.Name = "lblManzana";
            this.lblManzana.Text = "Manzana:";

            this.cmbManzana.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManzana.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbManzana.FormattingEnabled = true;
            this.cmbManzana.Location = new System.Drawing.Point(25, 38);
            this.cmbManzana.Name = "cmbManzana";
            this.cmbManzana.Size = new System.Drawing.Size(110, 25);

            this.lblLote.AutoSize = true;
            this.lblLote.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLote.ForeColor = System.Drawing.Color.FromArgb(60, 36, 15);
            this.lblLote.Location = new System.Drawing.Point(150, 18);
            this.lblLote.Name = "lblLote";
            this.lblLote.Text = "Lote:";

            this.cmbLote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLote.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbLote.FormattingEnabled = true;
            this.cmbLote.Location = new System.Drawing.Point(150, 38);
            this.cmbLote.Name = "cmbLote";
            this.cmbLote.Size = new System.Drawing.Size(110, 25);

            this.btnConsultar.BackColor = System.Drawing.Color.FromArgb(179, 108, 46);
            this.btnConsultar.FlatAppearance.BorderSize = 0;
            this.btnConsultar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConsultar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnConsultar.ForeColor = System.Drawing.Color.White;
            this.btnConsultar.Location = new System.Drawing.Point(280, 36);
            this.btnConsultar.Name = "btnConsultar";
            this.btnConsultar.Size = new System.Drawing.Size(130, 30);
            this.btnConsultar.Text = "CONSULTAR";
            this.btnConsultar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConsultar.UseVisualStyleBackColor = false;
            this.btnConsultar.Click += new System.EventHandler(this.btnConsultar_Click);

            this.btnExportarPDF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportarPDF.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this.btnExportarPDF.FlatAppearance.BorderSize = 0;
            this.btnExportarPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportarPDF.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnExportarPDF.ForeColor = System.Drawing.Color.White;
            this.btnExportarPDF.Location = new System.Drawing.Point(1040, 36);
            this.btnExportarPDF.Name = "btnExportarPDF";
            this.btnExportarPDF.Size = new System.Drawing.Size(140, 30);
            this.btnExportarPDF.Text = "EXPORTAR PDF";
            this.btnExportarPDF.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportarPDF.UseVisualStyleBackColor = false;
            this.btnExportarPDF.Click += new System.EventHandler(this.btnExportarPDF_Click);

            this.lblCasaInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            this.lblCasaInfo.ForeColor = System.Drawing.Color.FromArgb(88, 53, 23);
            this.lblCasaInfo.Location = new System.Drawing.Point(430, 40);
            this.lblCasaInfo.Name = "lblCasaInfo";
            this.lblCasaInfo.Size = new System.Drawing.Size(500, 22);
            this.lblCasaInfo.Text = "(Seleccione Manzana y Lote y pulse Consultar)";
            this.lblCasaInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // =================== BODY (split detalle + HUD) ===================
            this.panelBody.Controls.Add(this.panelDetalle);
            this.panelBody.Controls.Add(this.panelHUD);
            this.panelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBody.Location = new System.Drawing.Point(0, 150);
            this.panelBody.Name = "panelBody";
            this.panelBody.Size = new System.Drawing.Size(1200, 500);

            // ----------- panel detalle (izquierda) -----------
            this.panelDetalle.BackColor = System.Drawing.Color.FromArgb(248, 245, 240);
            this.panelDetalle.Controls.Add(this.tabPrincipal);
            this.panelDetalle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDetalle.Padding = new System.Windows.Forms.Padding(8, 8, 4, 8);
            this.panelDetalle.Name = "panelDetalle";

            // tabPrincipal
            this.tabPrincipal.Controls.Add(this.tabEstimaciones);
            this.tabPrincipal.Controls.Add(this.tabDestajos);
            this.tabPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPrincipal.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.tabPrincipal.ItemSize = new System.Drawing.Size(190, 34);
            this.tabPrincipal.Padding = new System.Drawing.Point(20, 6);
            this.tabPrincipal.SelectedIndex = 0;
            this.tabPrincipal.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabPrincipal.Name = "tabPrincipal";

            // ---- tab ESTIMACIONES con ObjectListView agrupable ----
            this.tabEstimaciones.BackColor = System.Drawing.Color.White;
            this.tabEstimaciones.Controls.Add(this.olvEstimaciones);
            this.tabEstimaciones.Name = "tabEstimaciones";
            this.tabEstimaciones.Padding = new System.Windows.Forms.Padding(3);
            this.tabEstimaciones.Text = "ESTIMACIONES";

            this.olvEstimaciones.AllColumns.Add(this.colEtapa);
            this.olvEstimaciones.AllColumns.Add(this.colWBS);
            this.olvEstimaciones.AllColumns.Add(this.colCodigo);
            this.olvEstimaciones.AllColumns.Add(this.colPartida);
            this.olvEstimaciones.AllColumns.Add(this.colAvance);
            this.olvEstimaciones.AllColumns.Add(this.colMontoEjecutado);
            this.olvEstimaciones.AllColumns.Add(this.colFechaFin);
            this.olvEstimaciones.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                this.colWBS, this.colCodigo, this.colPartida, this.colAvance, this.colMontoEjecutado, this.colFechaFin
            });
            this.olvEstimaciones.BackColor = System.Drawing.Color.White;
            this.olvEstimaciones.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.olvEstimaciones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvEstimaciones.FullRowSelect = true;
            this.olvEstimaciones.HideSelection = false;
            this.olvEstimaciones.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.olvEstimaciones.Name = "olvEstimaciones";
            this.olvEstimaciones.RowHeight = 24;
            this.olvEstimaciones.UseAlternatingBackColors = true;
            this.olvEstimaciones.AlternateRowBackColor = System.Drawing.Color.FromArgb(250, 247, 242);
            this.olvEstimaciones.ShowGroups = true;
            this.olvEstimaciones.SortGroupItemsByPrimaryColumn = false;
            this.olvEstimaciones.View = System.Windows.Forms.View.Details;
            this.olvEstimaciones.UseCellFormatEvents = true;
            this.olvEstimaciones.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;

            // Columna "agrupadora" (no se muestra pero define el grupo)
            this.colEtapa.Text = "Etapa";
            this.colEtapa.AspectName = "Etapa";
            this.colEtapa.IsVisible = false;

            this.colWBS.Text = "WBS";
            this.colWBS.AspectName = "WBS";
            this.colWBS.Width = 60;
            this.colWBS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            this.colCodigo.Text = "Código";
            this.colCodigo.AspectName = "Codigo";
            this.colCodigo.Width = 90;

            this.colPartida.Text = "Partida";
            this.colPartida.AspectName = "Partida";
            this.colPartida.FillsFreeSpace = true;
            this.colPartida.MinimumWidth = 200;

            this.colAvance.Text = "Avance %";
            this.colAvance.AspectName = "AvancePorcentaje";
            this.colAvance.Width = 90;
            this.colAvance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colAvance.AspectToStringFormat = "{0:N2}";

            this.colMontoEjecutado.Text = "Monto Ejecutado";
            this.colMontoEjecutado.AspectName = "MontoEjecutado";
            this.colMontoEjecutado.Width = 140;
            this.colMontoEjecutado.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colMontoEjecutado.AspectToStringFormat = "{0:C2}";

            this.colFechaFin.Text = "Fecha Final.";
            this.colFechaFin.AspectName = "FechaFinalizacion";
            this.colFechaFin.Width = 110;
            this.colFechaFin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colFechaFin.AspectToStringFormat = "{0:dd/MM/yyyy}";

            // ---- tab DESTAJOS (ObjectListView agrupado por Categoría) ----
            this.tabDestajos.BackColor = System.Drawing.Color.White;
            // El OLV primero (Dock=Fill) y el stepper después (Dock=Top): el stepper
            // queda en la parte superior porque WinForms procesa los Dock=Top después.
            this.tabDestajos.Controls.Add(this.olvDestajos);
            this.tabDestajos.Controls.Add(this.panelStepperDestajos);
            this.tabDestajos.Name = "tabDestajos";
            this.tabDestajos.Padding = new System.Windows.Forms.Padding(3);
            this.tabDestajos.Text = "DESTAJOS";

            // ---- stepper visual del proceso de destajo (6 pasos) ----
            this.panelStepperDestajos.BackColor = System.Drawing.Color.FromArgb(250, 246, 240);
            this.panelStepperDestajos.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelStepperDestajos.Height = 130;
            this.panelStepperDestajos.Name = "panelStepperDestajos";
            this.panelStepperDestajos.Padding = new System.Windows.Forms.Padding(12, 8, 12, 10);
            this.panelStepperDestajos.Paint += new System.Windows.Forms.PaintEventHandler(this.PintarStepperDestajos);
            this.panelStepperDestajos.Resize += new System.EventHandler(this.panelStepperDestajos_Resize);

            this.olvDestajos.AllColumns.Add(this.colDesCategoria);
            this.olvDestajos.AllColumns.Add(this.colDesID);
            this.olvDestajos.AllColumns.Add(this.colDesTipo);
            this.olvDestajos.AllColumns.Add(this.colDesTarea);
            this.olvDestajos.AllColumns.Add(this.colDesCantidad);
            this.olvDestajos.AllColumns.Add(this.colDesUnidad);
            this.olvDestajos.AllColumns.Add(this.colDesPrecioUnitario);
            this.olvDestajos.AllColumns.Add(this.colDesImporte);
            this.olvDestajos.AllColumns.Add(this.colDesGastado);
            this.olvDestajos.AllColumns.Add(this.colDesCuadrilla);
            this.olvDestajos.AllColumns.Add(this.colDesEstado);
            // Orden alineado a FormEditorTreeList: Nombre → Tipo Tarea → (Cantidad/Unidad/Precio)
            // y enfocado en el proceso del destajo:
            // 1) Identificar (ID + Destajo) · 2) Clasificar (Tipo)
            // 3) Cuantificar (Cant. · Unidad · P.Unit · Importe)
            // 4) Asignar (Cuadrilla) · 5) Ejecutar (Gastado) · 6) Cerrar (Estado)
            this.olvDestajos.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                this.colDesID, this.colDesTarea, this.colDesTipo, this.colDesCantidad,
                this.colDesUnidad, this.colDesPrecioUnitario, this.colDesImporte,
                this.colDesCuadrilla, this.colDesGastado, this.colDesEstado
            });
            this.olvDestajos.BackColor = System.Drawing.Color.White;
            this.olvDestajos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.olvDestajos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvDestajos.FullRowSelect = true;
            this.olvDestajos.HideSelection = false;
            this.olvDestajos.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.olvDestajos.Name = "olvDestajos";
            this.olvDestajos.RowHeight = 24;
            this.olvDestajos.UseAlternatingBackColors = true;
            this.olvDestajos.AlternateRowBackColor = System.Drawing.Color.FromArgb(250, 247, 242);
            this.olvDestajos.ShowGroups = true;
            this.olvDestajos.SortGroupItemsByPrimaryColumn = false;
            this.olvDestajos.View = System.Windows.Forms.View.Details;
            this.olvDestajos.UseCellFormatEvents = true;
            this.olvDestajos.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;

            this.colDesCategoria.Text = "Categoría";
            this.colDesCategoria.AspectName = "Categoria";
            this.colDesCategoria.IsVisible = false;

            // Paso 1 — IDENTIFICAR
            this.colDesID.Text = "1 · ID";
            this.colDesID.AspectName = "NodoID";
            this.colDesID.Width = 55;
            this.colDesID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            this.colDesTarea.Text = "1 · Destajo / Tarea";
            this.colDesTarea.AspectName = "Nombre";
            this.colDesTarea.FillsFreeSpace = true;
            this.colDesTarea.MinimumWidth = 200;

            // Paso 2 — CLASIFICAR
            this.colDesTipo.Text = "2 · Tipo";
            this.colDesTipo.AspectName = "TipoTexto";
            this.colDesTipo.Width = 95;
            this.colDesTipo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // Paso 3 — CUANTIFICAR
            this.colDesCantidad.Text = "3 · Cant.";
            this.colDesCantidad.AspectName = "Cantidad";
            this.colDesCantidad.Width = 70;
            this.colDesCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

            this.colDesUnidad.Text = "3 · Unidad";
            this.colDesUnidad.AspectName = "Unidad";
            this.colDesUnidad.Width = 60;

            this.colDesPrecioUnitario.Text = "3 · P. Unit.";
            this.colDesPrecioUnitario.AspectName = "PrecioUnitario";
            this.colDesPrecioUnitario.Width = 100;
            this.colDesPrecioUnitario.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

            this.colDesImporte.Text = "3 · = Importe";
            this.colDesImporte.AspectName = "Importe";
            this.colDesImporte.Width = 120;
            this.colDesImporte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

            // Paso 4 — ASIGNAR
            this.colDesCuadrilla.Text = "4 · Cuadrilla";
            this.colDesCuadrilla.AspectName = "Cuadrilla";
            this.colDesCuadrilla.Width = 85;

            // Paso 5 — EJECUTAR
            this.colDesGastado.Text = "5 · Gastado";
            this.colDesGastado.AspectName = "MontoGastado";
            this.colDesGastado.Width = 120;
            this.colDesGastado.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

            // Paso 6 — CERRAR
            this.colDesEstado.Text = "6 · Estado";
            this.colDesEstado.AspectName = "EstadoTexto";
            this.colDesEstado.Width = 110;
            this.colDesEstado.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // ===================== HUD lateral =====================
            this.panelHUD.BackColor = System.Drawing.Color.FromArgb(245, 240, 232);
            this.panelHUD.Controls.Add(this.cardDestajos);
            this.panelHUD.Controls.Add(this.cardEstimaciones);
            this.panelHUD.Controls.Add(this.lblHudTitle);
            this.panelHUD.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelHUD.Padding = new System.Windows.Forms.Padding(12);
            this.panelHUD.Name = "panelHUD";
            this.panelHUD.Size = new System.Drawing.Size(320, 500);

            this.lblHudTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHudTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblHudTitle.ForeColor = System.Drawing.Color.FromArgb(60, 36, 15);
            this.lblHudTitle.Height = 30;
            this.lblHudTitle.Name = "lblHudTitle";
            this.lblHudTitle.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lblHudTitle.Text = "RESUMEN FINANCIERO";
            this.lblHudTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ---- card Estimaciones ----
            this.cardEstimaciones.BackColor = System.Drawing.Color.White;
            this.cardEstimaciones.Controls.Add(this.lblCardEstInfo);
            this.cardEstimaciones.Controls.Add(this.lblCardEstValor);
            this.cardEstimaciones.Controls.Add(this.lblCardEstTitulo);
            this.cardEstimaciones.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardEstimaciones.Height = 100;
            this.cardEstimaciones.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.cardEstimaciones.Name = "cardEstimaciones";
            this.cardEstimaciones.Padding = new System.Windows.Forms.Padding(14, 10, 14, 10);
            this.cardEstimaciones.Paint += new System.Windows.Forms.PaintEventHandler(this.PintarBordeCard);

            this.lblCardEstTitulo.AutoSize = false;
            this.lblCardEstTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCardEstTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblCardEstTitulo.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblCardEstTitulo.Height = 18;
            this.lblCardEstTitulo.Text = "ESTIMACIONES — Avance Valorizado";
            this.lblCardEstTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblCardEstValor.AutoSize = false;
            this.lblCardEstValor.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCardEstValor.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold);
            this.lblCardEstValor.ForeColor = System.Drawing.Color.FromArgb(46, 134, 75);
            this.lblCardEstValor.Height = 34;
            this.lblCardEstValor.Text = "$0.00";
            this.lblCardEstValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblCardEstInfo.AutoSize = false;
            this.lblCardEstInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCardEstInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardEstInfo.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblCardEstInfo.Height = 18;
            this.lblCardEstInfo.Text = "0 conceptos";
            this.lblCardEstInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ---- card Destajos (contenedor con subcards) ----
            this.cardDestajos.BackColor = System.Drawing.Color.White;
            this.cardDestajos.Controls.Add(this.cardMat);
            this.cardDestajos.Controls.Add(this.cardMO);
            this.cardDestajos.Controls.Add(this.lblCardDesInfo);
            this.cardDestajos.Controls.Add(this.lblCardDesValor);
            this.cardDestajos.Controls.Add(this.lblCardDesTitulo);
            this.cardDestajos.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardDestajos.Height = 310;
            this.cardDestajos.Margin = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.cardDestajos.Name = "cardDestajos";
            this.cardDestajos.Padding = new System.Windows.Forms.Padding(14, 10, 14, 10);
            this.cardDestajos.Paint += new System.Windows.Forms.PaintEventHandler(this.PintarBordeCard);

            this.lblCardDesTitulo.AutoSize = false;
            this.lblCardDesTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCardDesTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblCardDesTitulo.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblCardDesTitulo.Height = 18;
            this.lblCardDesTitulo.Text = "DESTAJOS — Gasto Reconocido";
            this.lblCardDesTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblCardDesValor.AutoSize = false;
            this.lblCardDesValor.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCardDesValor.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold);
            this.lblCardDesValor.ForeColor = System.Drawing.Color.FromArgb(46, 134, 75);
            this.lblCardDesValor.Height = 34;
            this.lblCardDesValor.Text = "$0.00";
            this.lblCardDesValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblCardDesInfo.AutoSize = false;
            this.lblCardDesInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCardDesInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCardDesInfo.ForeColor = System.Drawing.Color.FromArgb(117, 117, 117);
            this.lblCardDesInfo.Height = 22;
            this.lblCardDesInfo.Text = "0 destajos activados";
            this.lblCardDesInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ---- subcard Mano de Obra ----
            this.cardMO.BackColor = System.Drawing.Color.FromArgb(232, 245, 233);
            this.cardMO.Controls.Add(this.barraMO);
            this.cardMO.Controls.Add(this.lblMOInfo);
            this.cardMO.Controls.Add(this.lblMOValor);
            this.cardMO.Controls.Add(this.lblMOTitulo);
            this.cardMO.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardMO.Height = 90;
            this.cardMO.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.cardMO.Name = "cardMO";
            this.cardMO.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardMO.Paint += new System.Windows.Forms.PaintEventHandler(this.PintarBordeSubcardVerde);

            this.lblMOTitulo.AutoSize = false;
            this.lblMOTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMOTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMOTitulo.ForeColor = System.Drawing.Color.FromArgb(33, 99, 50);
            this.lblMOTitulo.Height = 16;
            this.lblMOTitulo.Text = "■  MANO DE OBRA";
            this.lblMOTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblMOValor.AutoSize = false;
            this.lblMOValor.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMOValor.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblMOValor.ForeColor = System.Drawing.Color.FromArgb(33, 99, 50);
            this.lblMOValor.Height = 26;
            this.lblMOValor.Text = "$0.00";
            this.lblMOValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblMOInfo.AutoSize = false;
            this.lblMOInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMOInfo.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblMOInfo.ForeColor = System.Drawing.Color.FromArgb(33, 99, 50);
            this.lblMOInfo.Height = 14;
            this.lblMOInfo.Text = "0 destajos · 0%";
            this.lblMOInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.barraMO.BackColor = System.Drawing.Color.FromArgb(46, 134, 75);
            this.barraMO.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barraMO.Height = 6;
            this.barraMO.Name = "barraMO";

            // ---- subcard Material ----
            this.cardMat.BackColor = System.Drawing.Color.FromArgb(253, 235, 232);
            this.cardMat.Controls.Add(this.barraMat);
            this.cardMat.Controls.Add(this.lblMatInfo);
            this.cardMat.Controls.Add(this.lblMatValor);
            this.cardMat.Controls.Add(this.lblMatTitulo);
            this.cardMat.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardMat.Height = 90;
            this.cardMat.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.cardMat.Name = "cardMat";
            this.cardMat.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.cardMat.Paint += new System.Windows.Forms.PaintEventHandler(this.PintarBordeSubcardRojo);

            this.lblMatTitulo.AutoSize = false;
            this.lblMatTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMatTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMatTitulo.ForeColor = System.Drawing.Color.FromArgb(155, 41, 28);
            this.lblMatTitulo.Height = 16;
            this.lblMatTitulo.Text = "■  MATERIAL";
            this.lblMatTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblMatValor.AutoSize = false;
            this.lblMatValor.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMatValor.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblMatValor.ForeColor = System.Drawing.Color.FromArgb(155, 41, 28);
            this.lblMatValor.Height = 26;
            this.lblMatValor.Text = "$0.00";
            this.lblMatValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblMatInfo.AutoSize = false;
            this.lblMatInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMatInfo.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblMatInfo.ForeColor = System.Drawing.Color.FromArgb(155, 41, 28);
            this.lblMatInfo.Height = 14;
            this.lblMatInfo.Text = "0 destajos · 0%";
            this.lblMatInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.barraMat.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this.barraMat.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barraMat.Height = 6;
            this.barraMat.Name = "barraMat";

            // ===================== FOOTER =====================
            this.panelFooter.BackColor = System.Drawing.Color.FromArgb(88, 53, 23);
            this.panelFooter.Controls.Add(this.lblGranTotal);
            this.panelFooter.Controls.Add(this.lblGranTotalTitulo);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(1200, 52);

            this.lblGranTotalTitulo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGranTotalTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblGranTotalTitulo.ForeColor = System.Drawing.Color.FromArgb(230, 215, 195);
            this.lblGranTotalTitulo.Location = new System.Drawing.Point(800, 14);
            this.lblGranTotalTitulo.Name = "lblGranTotalTitulo";
            this.lblGranTotalTitulo.Size = new System.Drawing.Size(220, 22);
            this.lblGranTotalTitulo.Text = "GRAN TOTAL (CASA):";
            this.lblGranTotalTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.lblGranTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGranTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblGranTotal.ForeColor = System.Drawing.Color.White;
            this.lblGranTotal.Location = new System.Drawing.Point(1025, 11);
            this.lblGranTotal.Name = "lblGranTotal";
            this.lblGranTotal.Size = new System.Drawing.Size(160, 30);
            this.lblGranTotal.Text = "$0.00";
            this.lblGranTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // ===================== FORM =====================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1200, 702);
            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.panelFiltros);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1000, 650);
            this.Name = "FormAdministrativos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Administrativos — Consulta por Casa";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.panelHeader.ResumeLayout(false);
            this.panelFiltros.ResumeLayout(false);
            this.panelFiltros.PerformLayout();
            this.panelBody.ResumeLayout(false);
            this.panelDetalle.ResumeLayout(false);
            this.tabPrincipal.ResumeLayout(false);
            this.tabEstimaciones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvEstimaciones)).EndInit();
            this.tabDestajos.ResumeLayout(false);
            this.panelStepperDestajos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvDestajos)).EndInit();
            this.panelHUD.ResumeLayout(false);
            this.cardEstimaciones.ResumeLayout(false);
            this.cardDestajos.ResumeLayout(false);
            this.cardMO.ResumeLayout(false);
            this.cardMat.ResumeLayout(false);
            this.panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelFiltros;
        private System.Windows.Forms.Label lblCasaInfo;
        private System.Windows.Forms.Button btnExportarPDF;
        private System.Windows.Forms.Button btnConsultar;
        private System.Windows.Forms.ComboBox cmbLote;
        private System.Windows.Forms.Label lblLote;
        private System.Windows.Forms.ComboBox cmbManzana;
        private System.Windows.Forms.Label lblManzana;
        private System.Windows.Forms.Panel panelBody;
        private System.Windows.Forms.Panel panelDetalle;
        private System.Windows.Forms.TabControl tabPrincipal;
        private System.Windows.Forms.TabPage tabEstimaciones;
        private BrightIdeasSoftware.ObjectListView olvEstimaciones;
        private BrightIdeasSoftware.OLVColumn colEtapa;
        private BrightIdeasSoftware.OLVColumn colWBS;
        private BrightIdeasSoftware.OLVColumn colCodigo;
        private BrightIdeasSoftware.OLVColumn colPartida;
        private BrightIdeasSoftware.OLVColumn colAvance;
        private BrightIdeasSoftware.OLVColumn colMontoEjecutado;
        private BrightIdeasSoftware.OLVColumn colFechaFin;
        private System.Windows.Forms.TabPage tabDestajos;
        private System.Windows.Forms.Panel panelStepperDestajos;
        private BrightIdeasSoftware.ObjectListView olvDestajos;
        private BrightIdeasSoftware.OLVColumn colDesCategoria;
        private BrightIdeasSoftware.OLVColumn colDesID;
        private BrightIdeasSoftware.OLVColumn colDesTipo;
        private BrightIdeasSoftware.OLVColumn colDesTarea;
        private BrightIdeasSoftware.OLVColumn colDesCantidad;
        private BrightIdeasSoftware.OLVColumn colDesUnidad;
        private BrightIdeasSoftware.OLVColumn colDesPrecioUnitario;
        private BrightIdeasSoftware.OLVColumn colDesImporte;
        private BrightIdeasSoftware.OLVColumn colDesGastado;
        private BrightIdeasSoftware.OLVColumn colDesCuadrilla;
        private BrightIdeasSoftware.OLVColumn colDesEstado;
        private System.Windows.Forms.Panel panelHUD;
        private System.Windows.Forms.Label lblHudTitle;
        private System.Windows.Forms.Panel cardEstimaciones;
        private System.Windows.Forms.Label lblCardEstTitulo;
        private System.Windows.Forms.Label lblCardEstValor;
        private System.Windows.Forms.Label lblCardEstInfo;
        private System.Windows.Forms.Panel cardDestajos;
        private System.Windows.Forms.Label lblCardDesTitulo;
        private System.Windows.Forms.Label lblCardDesValor;
        private System.Windows.Forms.Label lblCardDesInfo;
        private System.Windows.Forms.Panel cardMO;
        private System.Windows.Forms.Label lblMOTitulo;
        private System.Windows.Forms.Label lblMOValor;
        private System.Windows.Forms.Label lblMOInfo;
        private System.Windows.Forms.Panel barraMO;
        private System.Windows.Forms.Panel cardMat;
        private System.Windows.Forms.Label lblMatTitulo;
        private System.Windows.Forms.Label lblMatValor;
        private System.Windows.Forms.Label lblMatInfo;
        private System.Windows.Forms.Panel barraMat;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Label lblGranTotal;
        private System.Windows.Forms.Label lblGranTotalTitulo;
    }
}
