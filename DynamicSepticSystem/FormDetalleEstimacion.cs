using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    public partial class FormDetalleEstimacion : Form
    {
        private EstimacionInfo estimacion;
        private List<DetalleEstimacion> detalles;

        public FormDetalleEstimacion(EstimacionInfo est, List<DetalleEstimacion> det)
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            estimacion = est;
            detalles = det;
            this.Load += FormDetalleEstimacion_Load;
        }

        private void FormDetalleEstimacion_Load(object sender, EventArgs e)
        {
            this.Text = $"Detalle de Estimaci�n - {estimacion.Folio}";
            CargarDatosGenerales();
            ConfigurarListView();
            CargarDetalles();
        }

        private void CargarDatosGenerales()
        {
            lblFolio.Text = $"Folio: {estimacion.Folio}";
            lblFecha.Text = $"Fecha: {estimacion.FechaGeneracion:dd/MM/yyyy HH:mm}";
            lblManzanaLote.Text = $"M{estimacion.Manzana} L{estimacion.Lote} - {estimacion.Prototipo}";
            lblProveedor.Text = $"Proveedor: {estimacion.Proveedor}";
            lblDescripcion.Text = $"Descripci�n: {estimacion.Descripcion}";
            lblTotal.Text = $"Total: {estimacion.TotalEstimacion:C2}";
            lblNumeroEstimacion.Text = $"Estimaci�n No. {estimacion.NumeroEstimacion}";
        }

        private void ConfigurarListView()
        {
            olvDetalles.FullRowSelect = true;
            olvDetalles.UseAlternatingBackColors = true;
            olvDetalles.AlternateRowBackColor = Color.FromArgb(240, 248, 255);
            olvDetalles.GridLines = true;
            olvDetalles.View = View.Details;

            var colCodigo = new OLVColumn("C�digo", "CodigoConcepto") { Width = 60, IsEditable = false };
            var colConcepto = new OLVColumn("Concepto", "NombreConcepto") { Width = 200, IsEditable = false };
            var colWBS = new OLVColumn("WBS", "WBS") { Width = 60, IsEditable = false, TextAlign = HorizontalAlignment.Center };
            var colPartida = new OLVColumn("Partida", "NombrePartida") { Width = 300, IsEditable = false, FillsFreeSpace = true };
            var colPresupuesto = new OLVColumn("Presupuestado", "MontoPresupuestado") 
            { 
                Width = 100, 
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}"
            };
            var colEjecutado = new OLVColumn("Ejecutado", "MontoEjecutado") 
            { 
                Width = 100, 
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}"
            };
            var colAvance = new OLVColumn("Avance %", "AvancePorcentaje") 
            { 
                Width = 80, 
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:F1}%"
            };

            olvDetalles.AllColumns.AddRange(new[] { colCodigo, colConcepto, colWBS, colPartida, colPresupuesto, colEjecutado, colAvance });
            olvDetalles.RebuildColumns();
        }

        private void CargarDetalles()
        {
            // Agrupar por concepto para mostrar jer�rquicamente
            var agrupados = detalles
                .GroupBy(d => new { d.CodigoConcepto, d.NombreConcepto })
                .OrderBy(g => g.Key.CodigoConcepto)
                .ToList();

            var items = new List<DetalleEstimacion>();

            foreach (var grupo in agrupados)
            {
                // Agregar encabezado de concepto
                items.Add(new DetalleEstimacion
                {
                    CodigoConcepto = grupo.Key.CodigoConcepto,
                    NombreConcepto = grupo.Key.NombreConcepto,
                    WBS = 0,
                    NombrePartida = "",
                    MontoPresupuestado = grupo.Sum(d => d.MontoPresupuestado),
                    MontoEjecutado = grupo.Sum(d => d.MontoEjecutado),
                    AvancePorcentaje = 0
                });

                // Agregar partidas
                items.AddRange(grupo.OrderBy(d => d.WBS));
            }

            olvDetalles.SetObjects(items);
            
            // Aplicar formato especial a los encabezados
            olvDetalles.FormatRow += (s, e) =>
            {
                var det = e.Model as DetalleEstimacion;
                if (det != null && det.WBS == 0)
                {
                    e.Item.BackColor = Color.FromArgb(230, 126, 34);
                    e.Item.ForeColor = Color.White;
                    e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                }
            };

            lblTotalPartidas.Text = $"Total de partidas: {detalles.Count}";
            lblTotalMonto.Text = $"Total ejecutado: {detalles.Sum(d => d.MontoEjecutado):C2}";
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
