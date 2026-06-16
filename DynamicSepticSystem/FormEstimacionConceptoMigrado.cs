using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Formulario principal para estimación por conceptos (versión migrada)
    /// 
    /// Este formulario está dividido en múltiples archivos parciales para facilitar el mantenimiento:
    /// - FormEstimacionConceptoMigrado.cs (este archivo): Constructor e inicialización
    /// - FormEstimacionConceptoMigrado.MenuContextual.cs: Menú contextual y handlers
    /// - FormEstimacionConceptoMigrado.TreeListView.cs: Configuración del TreeListView
    /// - FormEstimacionConceptoMigrado.CargaDatos.cs: Carga de manzanas, lotes, conceptos y partidas
    /// - FormEstimacionConceptoMigrado.Preview.cs: Vista previa del PDF
    /// - FormEstimacionConceptoMigrado.Avances.cs: Manejo de avances y guardado en BD
    /// - FormEstimacionConceptoMigrado.AgregarConcepto.cs: Funcionalidad de agregar conceptos
    /// - FormEstimacionConceptoMigrado_ExtensionFolios.cs: Manejo de folios y generación PDF
    /// - FormEstimacionConceptoMigrado_ExtensionPDF.cs: Formato del PDF
    /// - FormEstimacionConceptoMigrado_ExtensionEvidencias.cs: Evidencias fotográficas
    /// </summary>
    public partial class FormEstimacionConceptoMigrado : Form
    {
        #region Campos privados
        
        private string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        private List<NodoConcepto> nodosRaiz = new List<NodoConcepto>();
        private string prototipoActual = "";
        private System.Windows.Forms.Timer timerPreview;
        private ToolTip tooltipConceptos;
        private Label lblFolioGenerar;
        
        // Lista de evidencias fotográficas seleccionadas para incluir en el PDF
        private List<EvidenciaInfo> evidenciasSeleccionadas = new List<EvidenciaInfo>();
        private GestorEvidencias gestorEvidencias;
        
        // Diccionario temporal para metros cuadrados cargados desde BD
        private Dictionary<int, double> avancesMetrosCuadrados = new Dictionary<int, double>();
        
        // Menú contextual para partidas dinámicas
        private ContextMenuStrip contextMenuPartidas;

        #endregion

        #region Constructor

        public FormEstimacionConceptoMigrado()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            ConfigurarTreeListView();
            CargarManzanas();
            this.Load += FormEstimacionConcepto_Load;
            
            // Configurar timer para actualización del preview
            timerPreview = new System.Windows.Forms.Timer();
            timerPreview.Interval = 500;
            timerPreview.Tick += TimerPreview_Tick;
            
            // Configurar tooltip
            tooltipConceptos = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 300,
                ReshowDelay = 100,
                ShowAlways = true,
                IsBalloon = false
            };
            
            // Crear label de folio
            CrearLabelFolio();
            
            // Inicializar gestor de evidencias
            gestorEvidencias = new GestorEvidencias(connectionString);
            
            // Configurar menú contextual para partidas
            ConfigurarMenuContextual();
            
            // NUEVO: Configurar evento de clic en el preview del PDF
            ConfigurarEventoClickPreview();

            // NUEVO: Configurar la miniatura flotante de fotos al pasar el mouse
            ConfigurarPreviewFotosHover();

            // Liberar recursos del hover al cerrar el formulario
            this.FormClosed += (s, e) =>
            {
                OcultarPreviewFoto();
                _fotoPopup?.Dispose();
                LimpiarCacheThumbs();
            };
        }

        #endregion

        #region Inicialización

        private void CrearLabelFolio()
        {
            lblFolioGenerar = new Label
            {
                AutoSize = false,
                Width = 300,
                Height = 25,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Folio: (Selecciona Manzana y Lote)"
            };
            
            // Posicionar en el panel de filtros, después de los combos
            if (this.Controls.ContainsKey("panelFiltros"))
            {
                var panel = this.Controls["panelFiltros"];
                lblFolioGenerar.Location = new Point(870, 20);
                panel.Controls.Add(lblFolioGenerar);
            }
        }

        private void ConfigurarEventoClickPreview()
        {
            // Configurar el evento de clic en el PictureBox del preview
            if (pictureBoxPreviewPDF != null)
            {
                pictureBoxPreviewPDF.Cursor = Cursors.Hand;
                pictureBoxPreviewPDF.Click += PictureBoxPreviewPDF_Click;
                
                // Agregar tooltip para indicar que se puede hacer clic
                tooltipConceptos.SetToolTip(pictureBoxPreviewPDF, 
                    "Haz clic para ver el PDF en tamaño completo");
            }
        }

        private void ActualizarLabelFolio()
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                lblFolioGenerar.Text = "Folio: (Selecciona Manzana y Lote)";
                lblFolioGenerar.ForeColor = Color.Gray;
                return;
            }

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();
            int numEstimacion = ObtenerSiguienteNumeroEstimacion(manzana, lote);
            string folio = GenerarFolio(manzana, lote, numEstimacion);
            
            lblFolioGenerar.Text = $"Folio a Generar: {folio}";
            lblFolioGenerar.ForeColor = Color.FromArgb(41, 128, 185);
        }

        private void FormEstimacionConcepto_Load(object sender, EventArgs e)
        {
            this.Text = "Estimación por Conceptos - Sistema Calandria (Migrado)";
            ActualizarPreviewPDF();
        }

        #endregion

        #region Evento Click Preview PDF

        private void PictureBoxPreviewPDF_Click(object sender, EventArgs e)
        {
            // Verificar que hay una imagen de preview
            if (pictureBoxPreviewPDF.Image == null)
            {
                MessageBox.Show("No hay vista previa disponible. Selecciona una casa y carga los conceptos primero.", 
                    "Sin Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Crear y mostrar el formulario maximizado
                using (var formMaximizado = new FormPreviewPDFMaximizado(pictureBoxPreviewPDF.Image))
                {
                    formMaximizado.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar preview maximizado: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}

