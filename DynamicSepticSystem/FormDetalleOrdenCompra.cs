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
        private string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"]?.ConnectionString;

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
            olvDetalle.Columns.Add(new OLVColumn("Descripción", "Descripcion") { Width = 300 });
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Cargar información del folio
                    string sqlFolio = @"
                        SELECT 
                            Folio, Manzana, Lote, FechaGeneracion, TipoOrden,
                            NombreProveedor, CodigoProveedor, TotalSinIVA, IVA, TotalConIVA,
                            NumeroOrden, Usuario, Estado, Observaciones
                        FROM FoliosOrdenCompra
                        WHERE Id = @id";

                    using (SqlCommand cmd = new SqlCommand(sqlFolio, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", folioId);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblFolio.Text = $"Folio: {reader["Folio"]}";
                                lblTipo.Text = $"Tipo: {reader["TipoOrden"]}";
                                lblFecha.Text = $"Fecha: {Convert.ToDateTime(reader["FechaGeneracion"]):dd/MM/yyyy HH:mm}";
                                lblUsuario.Text = $"Usuario: {reader["Usuario"]}";
                                lblEstado.Text = $"Estado: {reader["Estado"]}";
                                
                                string manzana = reader.IsDBNull(reader.GetOrdinal("Manzana")) ? "N/A" : reader.GetString(reader.GetOrdinal("Manzana"));
                                string lote = reader.IsDBNull(reader.GetOrdinal("Lote")) ? "N/A" : reader.GetString(reader.GetOrdinal("Lote"));
                                lblManzanaLote.Text = $"Manzana/Lote: {manzana}/{lote}";
                                
                                lblProveedor.Text = $"Proveedor: {reader["NombreProveedor"]} ({reader["CodigoProveedor"]})";
                                
                                decimal subtotal = reader.IsDBNull(reader.GetOrdinal("TotalSinIVA")) ? 0 : Convert.ToDecimal(reader["TotalSinIVA"]);
                                decimal iva = reader.IsDBNull(reader.GetOrdinal("IVA")) ? 0 : Convert.ToDecimal(reader["IVA"]);
                                decimal total = reader.IsDBNull(reader.GetOrdinal("TotalConIVA")) ? 0 : Convert.ToDecimal(reader["TotalConIVA"]);
                                
                                lblSubtotal.Text = $"Subtotal: {subtotal:C2}";
                                lblIVA.Text = $"IVA: {iva:C2}";
                                lblTotal.Text = $"TOTAL: {total:C2}";
                                lblTotal.Font = new Font(lblTotal.Font.FontFamily, 12F, FontStyle.Bold);
                            }
                        }
                    }

                    // 2. Cargar casas incluidas (si es orden múltiple)
                    List<string> casas = new List<string>();
                    string sqlCasas = @"
                        SELECT Manzana, Lote, Prototipo
                        FROM FoliosOrdenCompra_Casas
                        WHERE FolioId = @id
                        ORDER BY Manzana, Lote";

                    using (SqlCommand cmd = new SqlCommand(sqlCasas, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", folioId);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string manzana = reader.GetString(0);
                                string lote = reader.GetString(1);
                                string prototipo = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                casas.Add($"M{manzana}-L{lote} ({prototipo})");
                            }
                        }
                    }

                    if (casas.Count > 0)
                    {
                        txtCasas.Text = string.Join(Environment.NewLine, casas);
                        groupBoxCasas.Visible = true;
                    }
                    else
                    {
                        groupBoxCasas.Visible = false;
                    }

                    // 3. Cargar detalle de insumos
                    List<DetalleInsumo> insumos = new List<DetalleInsumo>();
                    string sqlDetalle = @"
                        SELECT Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Familia
                        FROM FoliosOrdenCompraDetalle
                        WHERE FolioId = @id
                        ORDER BY Clave";

                    using (SqlCommand cmd = new SqlCommand(sqlDetalle, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", folioId);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                insumos.Add(new DetalleInsumo
                                {
                                    Clave = reader.GetString(0),
                                    Descripcion = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    Unidad = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    Cantidad = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3)),
                                    PrecioUnitario = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader.GetValue(4)),
                                    ImporteTotal = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5)),
                                    Familia = reader.IsDBNull(6) ? "" : reader.GetString(6)
                                });
                            }
                        }
                    }

                    olvDetalle.SetObjects(insumos);
                    lblTotalInsumos.Text = $"Total de insumos: {insumos.Count}";
                }
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
