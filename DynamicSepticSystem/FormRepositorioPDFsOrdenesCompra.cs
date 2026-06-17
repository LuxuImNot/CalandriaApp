using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using System.Configuration;
using System.Diagnostics;

namespace DynamicSepticSystem
{
    public partial class FormRepositorioPDFsOrdenesCompra : Form
    {
        private string manzanaFiltro;
        private string loteFiltro;
        private bool soloIndirectas;

        public FormRepositorioPDFsOrdenesCompra(string manzana = null, string lote = null, bool soloIndirectas = false)
        {
            InitializeComponent();
            this.manzanaFiltro = manzana;
            this.loteFiltro = lote;
            this.soloIndirectas = soloIndirectas;
            
            ConfigurarLista();
            AplicarTema();
            CargarOrdenes();
            
            // Actualizar t�tulo seg�n el filtro
            if (soloIndirectas)
            {
                this.Text = "Repositorio de �rdenes Indirectas/Administrativas";
                lblTitulo.Text = "�RDENES INDIRECTAS Y ADMINISTRATIVAS";
            }
            else if (!string.IsNullOrEmpty(manzana) && !string.IsNullOrEmpty(lote))
            {
                this.Text = $"Repositorio de �rdenes - M{manzana} L{lote}";
                lblTitulo.Text = $"�RDENES DE COMPRA - MANZANA {manzana} LOTE {lote}";
            }
            else
            {
                this.Text = "Repositorio de �rdenes de Compra";
                lblTitulo.Text = "TODAS LAS �RDENES DE COMPRA";
            }
        }

        private void AplicarTema()
        {
            this.BackColor = ThemeManager.ColorFondo;
            ThemeManager.AplicarTema(this);
            
            // Panel superior
            panelSuperior.BackColor = ThemeManager.ColorPrincipalMenuBar;
            lblTitulo.ForeColor = ThemeManager.ColorTextoClaro;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            
            // Botones - aplicar tema b�sico y personalizar
            btnVerPDF.BackColor = ThemeManager.ColorPrincipalMenuBar;
            btnVerPDF.ForeColor = ThemeManager.ColorTextoClaro;
            btnVerPDF.FlatStyle = FlatStyle.Flat;
            btnVerPDF.FlatAppearance.BorderSize = 0;
            
            btnDetalle.BackColor = ThemeManager.ColorInfo;
            btnDetalle.ForeColor = ThemeManager.ColorTextoClaro;
            btnDetalle.FlatStyle = FlatStyle.Flat;
            btnDetalle.FlatAppearance.BorderSize = 0;
            
            btnActualizar.BackColor = ThemeManager.ColorInfo;
            btnActualizar.ForeColor = ThemeManager.ColorTextoClaro;
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.FlatAppearance.BorderSize = 0;
            
            ThemeManager.EstilizarBotonPeligro(btnEliminar);
            ThemeManager.EstilizarBotonSecundario(btnCerrar);
            
            // ObjectListView
            EstilizarObjectListView(olvOrdenes);
            
            // Panel de filtros
            panelFiltros.BackColor = ThemeManager.ColorFondoAlterno;
            panelFiltros.BorderStyle = BorderStyle.FixedSingle;
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
            olvOrdenes.FullRowSelect = true;
            olvOrdenes.ShowGroups = false;
            olvOrdenes.Columns.Clear();
            
            // Configurar columnas
            olvOrdenes.Columns.Add(new OLVColumn("Folio", "Folio") { Width = 150 });
            olvOrdenes.Columns.Add(new OLVColumn("Tipo", "TipoOrden") { Width = 100 });
            olvOrdenes.Columns.Add(new OLVColumn("Fecha", "FechaGeneracion") 
            { 
                Width = 120,
                AspectToStringFormat = "{0:dd/MM/yyyy HH:mm}"
            });
            olvOrdenes.Columns.Add(new OLVColumn("Manzana", "Manzana") { Width = 70 });
            olvOrdenes.Columns.Add(new OLVColumn("Lote", "Lote") { Width = 70 });
            olvOrdenes.Columns.Add(new OLVColumn("Proveedor", "NombreProveedor") { Width = 200 });
            olvOrdenes.Columns.Add(new OLVColumn("Total", "TotalConIVA") 
            { 
                Width = 100,
                AspectToStringFormat = "{0:C2}",
                TextAlign = HorizontalAlignment.Right
            });
            olvOrdenes.Columns.Add(new OLVColumn("Insumos", "TotalInsumos") 
            { 
                Width = 70,
                TextAlign = HorizontalAlignment.Center
            });
            olvOrdenes.Columns.Add(new OLVColumn("Casas", "CasasIncluidas") { Width = 200 });
            olvOrdenes.Columns.Add(new OLVColumn("Estado", "Estado") 
            { 
                Width = 90,
                TextAlign = HorizontalAlignment.Center
            });
            
            // Doble clic para ver PDF
            olvOrdenes.DoubleClick += (s, e) => VerPDF();
            
            // Formato condicional
            olvOrdenes.FormatRow += (s, e) =>
            {
                var orden = e.Model as OrdenCompraInfo;
                if (orden != null)
                {
                    if (orden.Estado == "COMPLETADA")
                        e.Item.ForeColor = Color.Green;
                    else if (orden.Estado == "CANCELADA")
                        e.Item.ForeColor = Color.Red;
                    else if (orden.TipoOrden == "INDIRECTA" || orden.TipoOrden == "ADMINISTRATIVA")
                        e.Item.BackColor = Color.FromArgb(255, 250, 205); // Amarillo claro
                }
            };
        }

        private void CargarOrdenes()
        {
            try
            {
                var query = "/api/repositorio-ordenes?soloIndirectas=" + (soloIndirectas ? "true" : "false");
                if (!soloIndirectas && !string.IsNullOrEmpty(manzanaFiltro) && !string.IsNullOrEmpty(loteFiltro))
                {
                    query += "&manzana=" + Uri.EscapeDataString(manzanaFiltro) +
                             "&lote=" + Uri.EscapeDataString(loteFiltro);
                }

                var ordenes = ApiClient.Get<List<OrdenCompraInfo>>(query) ?? new List<OrdenCompraInfo>();

                olvOrdenes.SetObjects(ordenes);
                ActualizarContadores(ordenes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar �rdenes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarContadores(List<OrdenCompraInfo> ordenes)
        {
            int total = ordenes.Count;
            decimal sumaTotales = ordenes.Sum(o => o.TotalConIVA);
            
            lblContador.Text = $"Total: {total} orden(es) | Suma: {sumaTotales:C2}";
        }

        private void VerPDF()
        {
            var seleccionada = olvOrdenes.SelectedObject as OrdenCompraInfo;
            if (seleccionada == null)
            {
                MessageBox.Show("Selecciona una orden de la lista", "Atenci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                byte[] pdfBytes = ApiClient.GetBytes($"/api/repositorio-ordenes/{seleccionada.Id}/pdf");
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    MessageBox.Show("No se encontr� el PDF almacenado para esta orden", "Sin PDF",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Guardar temporalmente y abrir
                string nombreArchivo = $"OrdenCompra_{seleccionada.Folio}.pdf";
                string tempPath = Path.Combine(Path.GetTempPath(), nombreArchivo);
                File.WriteAllBytes(tempPath, pdfBytes);

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo(tempPath) { UseShellExecute = true };
                    Process.Start(psi);
                }
                catch
                {
                    MessageBox.Show($"PDF guardado en: {tempPath}\n\n�brelo manualmente si no se abri� autom�ticamente.",
                        "PDF Extra�do", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir PDF: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarDetalle()
        {
            var seleccionada = olvOrdenes.SelectedObject as OrdenCompraInfo;
            if (seleccionada == null)
            {
                MessageBox.Show("Selecciona una orden de la lista", "Atenci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var formDetalle = new FormDetalleOrdenCompra(seleccionada.Id, seleccionada.Folio))
                {
                    formDetalle.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar detalle: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EliminarOrden()
        {
            var seleccionada = olvOrdenes.SelectedObject as OrdenCompraInfo;
            if (seleccionada == null)
            {
                MessageBox.Show("Selecciona una orden de la lista", "Atenci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"�Est�s seguro de eliminar esta orden?\n\n" +
                $"Folio: {seleccionada.Folio}\n" +
                $"Tipo: {seleccionada.TipoOrden}\n" +
                $"Proveedor: {seleccionada.NombreProveedor}\n" +
                $"Total: {seleccionada.TotalConIVA:C2}\n\n" +
                $"Esta acci�n NO SE PUEDE DESHACER.\n" +
                $"Se eliminar� el folio, detalle y PDF almacenado.",
                "Confirmar Eliminaci�n",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                // La eliminaci�n en cascada (FK) se encarga del detalle, casas y PDF
                int rowsAffected = ApiClient.Post<int>($"/api/repositorio-ordenes/{seleccionada.Id}/eliminar", new { });

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Orden eliminada exitosamente", "�xito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Recargar lista
                    CargarOrdenes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar orden: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltros()
        {
            string textoBusqueda = txtBuscar.Text.Trim().ToLower();
            DateTime? fechaDesde = chkFechaDesde.Checked ? dtpDesde.Value.Date : (DateTime?)null;
            DateTime? fechaHasta = chkFechaHasta.Checked ? dtpHasta.Value.Date.AddDays(1).AddSeconds(-1) : (DateTime?)null;

            var todasOrdenes = olvOrdenes.Objects.Cast<OrdenCompraInfo>().ToList();
            var ordenesFilteradas = todasOrdenes.Where(o =>
            {
                bool cumpleBusqueda = string.IsNullOrEmpty(textoBusqueda) ||
                    o.Folio.ToLower().Contains(textoBusqueda) ||
                    (o.NombreProveedor != null && o.NombreProveedor.ToLower().Contains(textoBusqueda)) ||
                    (o.CasasIncluidas != null && o.CasasIncluidas.ToLower().Contains(textoBusqueda));

                bool cumpleFechaDesde = !fechaDesde.HasValue || o.FechaGeneracion >= fechaDesde.Value;
                bool cumpleFechaHasta = !fechaHasta.HasValue || o.FechaGeneracion <= fechaHasta.Value;

                return cumpleBusqueda && cumpleFechaDesde && cumpleFechaHasta;
            }).ToList();

            olvOrdenes.SetObjects(ordenesFilteradas);
            ActualizarContadores(ordenesFilteradas);
        }

        private void btnVerPDF_Click(object sender, EventArgs e) => VerPDF();
        private void btnDetalle_Click(object sender, EventArgs e) => MostrarDetalle();
        private void btnEliminar_Click(object sender, EventArgs e) => EliminarOrden();
        private void btnActualizar_Click(object sender, EventArgs e) => CargarOrdenes();
        private void btnCerrar_Click(object sender, EventArgs e) => this.Close();
        private void txtBuscar_TextChanged(object sender, EventArgs e) => AplicarFiltros();
        private void chkFechaDesde_CheckedChanged(object sender, EventArgs e) => AplicarFiltros();
        private void chkFechaHasta_CheckedChanged(object sender, EventArgs e) => AplicarFiltros();
        private void dtpDesde_ValueChanged(object sender, EventArgs e) => AplicarFiltros();
        private void dtpHasta_ValueChanged(object sender, EventArgs e) => AplicarFiltros();
    }

    public class OrdenCompraInfo
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string TipoOrden { get; set; }
        public string NombreProveedor { get; set; }
        public string CodigoProveedor { get; set; }
        public decimal TotalConIVA { get; set; }
        public int? NumeroOrden { get; set; }
        public string Estado { get; set; }
        public int TotalInsumos { get; set; }
        public string CasasIncluidas { get; set; }
    }
}
