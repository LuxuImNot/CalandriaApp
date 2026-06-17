using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using System.Configuration;

namespace DynamicSepticSystem
{
    public partial class FormDetalleOrdenCompra : Form
    {
        private int folioId;
        private string folio;

        public FormDetalleOrdenCompra(int folioId, string folio)
        {
            InitializeComponent();
            this.folioId = folioId;
            this.folio = folio;
            
            this.Text = $"Detalle de Orden - {folio}";
            ConfigurarLista();
            AplicarTema();
            CargarDatos();
        }

        private void AplicarTema()
        {
            this.BackColor = ThemeManager.ColorFondo;
            ThemeManager.AplicarTema(this);
            
            panelSuperior.BackColor = ThemeManager.ColorPrincipalMenuBar;
            lblTitulo.ForeColor = ThemeManager.ColorTextoClaro;
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            
            groupBoxInfo.ForeColor = ThemeManager.ColorPrincipalMenuBar;
            groupBoxInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            groupBoxCasas.ForeColor = ThemeManager.ColorPrincipalMenuBar;
            groupBoxCasas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            EstilizarObjectListView(olvDetalle);
            
            ThemeManager.EstilizarBotonSecundario(btnCerrar);
        }

        private void EstilizarObjectListView(ObjectListView olv)
        {
            olv.BackColor = ThemeManager.ColorFondo;
            olv.ForeColor = ThemeManager.ColorTextoOscuro;
            olv.BorderStyle = BorderStyle.FixedSingle;
            olv.FullRowSelect = true;
            olv.GridLines = true;
            olv.Font = new Font("Segoe UI", 9F);
            
            olv.HeaderFormatStyle = new HeaderFormatStyle
            {
                Hot = new HeaderStateStyle
                {
                    BackColor = ThemeManager.ColorPrincipalClaro,
                    ForeColor = ThemeManager.ColorTextoClaro
                },
                Normal = new HeaderStateStyle
                {
                    BackColor = ThemeManager.ColorPrincipalMenuBar,
                    ForeColor = ThemeManager.ColorTextoClaro
                },
                Pressed = new HeaderStateStyle
                {
                    BackColor = ThemeManager.ColorPrincipalOscuro,
                    ForeColor = ThemeManager.ColorTextoClaro
                }
            };
            
            olv.UseAlternatingBackColors = true;
            olv.AlternateRowBackColor = ThemeManager.ColorFondoAlterno;
        }

        private void ConfigurarLista()
        {
            olvDetalle.FullRowSelect = true;
            olvDetalle.ShowGroups = false;
            olvDetalle.Columns.Clear();
            
            olvDetalle.Columns.Add(new OLVColumn("Clave", "Clave") { Width = 120 });
            olvDetalle.Columns.Add(new OLVColumn("Descripci�n", "Descripcion") { Width = 300 });
            olvDetalle.Columns.Add(new OLVColumn("Unidad", "Unidad") { Width = 80 });
            olvDetalle.Columns.Add(new OLVColumn("Cantidad", "Cantidad") 
            { 
                Width = 90,
                AspectToStringFormat = "{0:N2}",
                TextAlign = HorizontalAlignment.Right
            });
            olvDetalle.Columns.Add(new OLVColumn("Precio Unit.", "PrecioUnitario") 
            { 
                Width = 100,
                AspectToStringFormat = "{0:C2}",
                TextAlign = HorizontalAlignment.Right
            });
            olvDetalle.Columns.Add(new OLVColumn("Importe", "ImporteTotal") 
            { 
                Width = 120,
                AspectToStringFormat = "{0:C2}",
                TextAlign = HorizontalAlignment.Right
            });
            olvDetalle.Columns.Add(new OLVColumn("Familia", "Familia") { Width = 100 });
        }

        private void CargarDatos()
        {
            try
            {
                var detalle = ApiClient.Get<DetalleOrdenRepoApi>($"/api/repositorio-ordenes/{folioId}/detalle");
                if (detalle == null)
                    return;

                // 1. Información del folio
                lblFolio.Text = $"Folio: {detalle.Folio}";
                lblTipo.Text = $"Tipo: {detalle.TipoOrden}";
                lblFecha.Text = $"Fecha: {detalle.FechaGeneracion:dd/MM/yyyy HH:mm}";
                lblUsuario.Text = $"Usuario: {detalle.Usuario}";
                lblEstado.Text = $"Estado: {detalle.Estado}";
                lblManzanaLote.Text = $"Manzana/Lote: {detalle.Manzana ?? "N/A"}/{detalle.Lote ?? "N/A"}";
                lblProveedor.Text = $"Proveedor: {detalle.NombreProveedor} ({detalle.CodigoProveedor})";
                lblSubtotal.Text = $"Subtotal: {detalle.TotalSinIVA:C2}";
                lblIVA.Text = $"IVA: {detalle.IVA:C2}";
                lblTotal.Text = $"TOTAL: {detalle.TotalConIVA:C2}";
                lblTotal.Font = new Font(lblTotal.Font.FontFamily, 12F, FontStyle.Bold);

                // 2. Casas incluidas (si es orden múltiple)
                var casas = (detalle.Casas ?? new List<CasaRepoApi>())
                    .Select(c => $"M{c.Manzana}-L{c.Lote} ({c.Prototipo})")
                    .ToList();

                if (casas.Count > 0)
                {
                    txtCasas.Text = string.Join(Environment.NewLine, casas);
                    groupBoxCasas.Visible = true;
                }
                else
                {
                    groupBoxCasas.Visible = false;
                }

                // 3. Detalle de insumos
                var insumos = detalle.Insumos ?? new List<DetalleInsumo>();
                olvDetalle.SetObjects(insumos);
                lblTotalInsumos.Text = $"Total de insumos: {insumos.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class DetalleInsumo
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal ImporteTotal { get; set; }
        public string Familia { get; set; }
    }
}
