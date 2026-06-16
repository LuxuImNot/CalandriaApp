using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    public partial class FormDetalleValeSalida : Form
    {
        private int folioId;
        private string folio;
        private string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"]?.ConnectionString;

        public FormDetalleValeSalida(int folioId, string folio)
        {
            InitializeComponent();
            this.folioId = folioId;
            this.folio = folio;
            
            AplicarTema();
            CargarDetalle();
        }

        private void AplicarTema()
        {
            this.BackColor = ThemeManager.ColorFondo;
            ThemeManager.AplicarTema(this);
            
            panelInfo.BackColor = ThemeManager.ColorFondoAlterno;
            panelInfo.BorderStyle = BorderStyle.FixedSingle;
            
            ThemeManager.EstilizarBotonSecundario(btnCerrar);
            
            EstilizarObjectListView(olvDetalle);
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

        private void CargarDetalle()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Cargar información general
                    string sqlInfo = @"
                        SELECT 
                            Folio, Manzana, Lote, Prototipo, Obra,
                            FechaSolicitud, TotalImporte, NumeroSalida,
                            Usuario, Solicitante, ResidenteObra, 
                            EncargadoAlmacen, Estado, Observaciones
                        FROM FoliosSalidaAlmacen
                        WHERE Id = @id";

                    using (SqlCommand cmd = new SqlCommand(sqlInfo, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", folioId);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblFolio.Text = "Folio: " + reader["Folio"].ToString();
                                lblObra.Text = "Obra: " + reader["Obra"].ToString();
                                lblPrototipo.Text = "Prototipo: " + (reader["Prototipo"] != DBNull.Value ? reader["Prototipo"].ToString() : "N/A");
                                lblFecha.Text = "Fecha: " + Convert.ToDateTime(reader["FechaSolicitud"]).ToString("dd/MM/yyyy HH:mm");
                                lblTotal.Text = "Total: " + Convert.ToDecimal(reader["TotalImporte"]).ToString("C2");
                                lblSolicitante.Text = "Solicitante: " + (reader["Solicitante"] != DBNull.Value ? reader["Solicitante"].ToString() : "N/A");
                                lblResidente.Text = "Residente: " + (reader["ResidenteObra"] != DBNull.Value ? reader["ResidenteObra"].ToString() : "N/A");
                                lblEncargado.Text = "Encargado: " + (reader["EncargadoAlmacen"] != DBNull.Value ? reader["EncargadoAlmacen"].ToString() : "N/A");
                                lblEstado.Text = "Estado: " + (reader["Estado"] != DBNull.Value ? reader["Estado"].ToString() : "PENDIENTE");
                            }
                        }
                    }

                    // Cargar insumos
                    string sqlDetalle = @"
                        SELECT 
                            NumeroFila, Codigo, Descripcion, Unidad,
                            Cantidad, PrecioUnitario, Importe, Observaciones
                        FROM FoliosSalidaAlmacenDetalle
                        WHERE FolioId = @id
                        ORDER BY NumeroFila";

                    using (SqlCommand cmd = new SqlCommand(sqlDetalle, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", folioId);
                        
                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        
                        olvDetalle.SetObjects(dt.AsEnumerable());
                        
                        // Configurar columnas
                        olvDetalle.Columns.Clear();
                        olvDetalle.Columns.Add(new OLVColumn("Nº", "NumeroFila") { Width = 40, TextAlign = HorizontalAlignment.Center });
                        olvDetalle.Columns.Add(new OLVColumn("Código", "Codigo") { Width = 100 });
                        olvDetalle.Columns.Add(new OLVColumn("Descripción", "Descripcion") { Width = 280 });
                        olvDetalle.Columns.Add(new OLVColumn("Unidad", "Unidad") { Width = 70, TextAlign = HorizontalAlignment.Center });
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
                        olvDetalle.Columns.Add(new OLVColumn("Importe", "Importe") 
                        { 
                            Width = 100, 
                            AspectToStringFormat = "{0:C2}",
                            TextAlign = HorizontalAlignment.Right 
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalle: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
