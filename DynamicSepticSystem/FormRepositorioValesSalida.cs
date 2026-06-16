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
    public partial class FormRepositorioValesSalida : Form
    {
        private string manzanaFiltro;
        private string loteFiltro;
        private string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"]?.ConnectionString;

        public FormRepositorioValesSalida(string manzana = null, string lote = null)
        {
            InitializeComponent();
            this.manzanaFiltro = manzana;
            this.loteFiltro = lote;
            
            ConfigurarLista();
            AplicarTema();
            CargarVales();
            
            // Actualizar título según el filtro
            if (!string.IsNullOrEmpty(manzana) && !string.IsNullOrEmpty(lote))
            {
                this.Text = $"Repositorio de Vales de Salida - M{manzana} L{lote}";
                lblTitulo.Text = $"VALES DE SALIDA - MANZANA {manzana} LOTE {lote}";
            }
            else
            {
                this.Text = "Repositorio de Vales de Salida";
                lblTitulo.Text = "TODOS LOS VALES DE SALIDA DE ALMACÉN";
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
            
            // Botones
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
            EstilizarObjectListView(olvVales);
            
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
            olvVales.FullRowSelect = true;
            olvVales.ShowGroups = false;
            olvVales.Columns.Clear();
            
            // Configurar columnas
            olvVales.Columns.Add(new OLVColumn("Folio", "Folio") { Width = 180 });
            olvVales.Columns.Add(new OLVColumn("Fecha", "FechaSolicitud") 
            { 
                Width = 120,
                AspectToStringFormat = "{0:dd/MM/yyyy HH:mm}"
            });
            olvVales.Columns.Add(new OLVColumn("Manzana", "Manzana") { Width = 70 });
            olvVales.Columns.Add(new OLVColumn("Lote", "Lote") { Width = 70 });
            olvVales.Columns.Add(new OLVColumn("Prototipo", "Prototipo") { Width = 100 });
            olvVales.Columns.Add(new OLVColumn("Solicitante", "Solicitante") { Width = 150 });
            olvVales.Columns.Add(new OLVColumn("Total", "TotalImporte") 
            { 
                Width = 100,
                AspectToStringFormat = "{0:C2}",
                TextAlign = HorizontalAlignment.Right
            });
            olvVales.Columns.Add(new OLVColumn("Insumos", "TotalInsumos") 
            { 
                Width = 70,
                TextAlign = HorizontalAlignment.Center
            });
            olvVales.Columns.Add(new OLVColumn("Estado", "Estado") 
            { 
                Width = 90,
                TextAlign = HorizontalAlignment.Center
            });
            
            // Doble clic para ver PDF
            olvVales.DoubleClick += (s, e) => VerPDF();
            
            // Formato condicional
            olvVales.FormatRow += (s, e) =>
            {
                var vale = e.Model as ValeSalidaInfo;
                if (vale != null)
                {
                    if (vale.Estado == "ENTREGADO")
                        e.Item.ForeColor = Color.Green;
                    else if (vale.Estado == "CANCELADO")
                        e.Item.ForeColor = Color.Red;
                    else if (vale.Estado == "PENDIENTE")
                        e.Item.BackColor = Color.FromArgb(255, 250, 205); // Amarillo claro
                }
            };
        }

        private void CargarVales()
        {
            try
            {
                List<ValeSalidaInfo> vales = new List<ValeSalidaInfo>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            f.Id,
                            f.Folio,
                            f.Manzana,
                            f.Lote,
                            f.Prototipo,
                            f.Obra,
                            f.FechaSolicitud,
                            f.TotalImporte,
                            f.NumeroSalida,
                            f.Usuario,
                            f.Solicitante,
                            f.ResidenteObra,
                            f.EncargadoAlmacen,
                            f.Estado,
                            COUNT(DISTINCT d.Id) AS TotalInsumos
                        FROM FoliosSalidaAlmacen f
                        LEFT JOIN FoliosSalidaAlmacenDetalle d ON f.Id = d.FolioId
                        WHERE 1=1";

                    // Aplicar filtros
                    if (!string.IsNullOrEmpty(manzanaFiltro) && !string.IsNullOrEmpty(loteFiltro))
                    {
                        sql += " AND f.Manzana = @manzana AND f.Lote = @lote";
                    }

                    sql += @"
                        GROUP BY f.Id, f.Folio, f.Manzana, f.Lote, f.Prototipo, f.Obra, f.FechaSolicitud,
                                 f.TotalImporte, f.NumeroSalida, f.Usuario, f.Solicitante, 
                                 f.ResidenteObra, f.EncargadoAlmacen, f.Estado
                        ORDER BY f.FechaSolicitud DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(manzanaFiltro) && !string.IsNullOrEmpty(loteFiltro))
                        {
                            cmd.Parameters.AddWithValue("@manzana", manzanaFiltro);
                            cmd.Parameters.AddWithValue("@lote", loteFiltro);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                vales.Add(new ValeSalidaInfo
                                {
                                    Id = reader.GetInt32(0),
                                    Folio = reader.GetString(1),
                                    Manzana = reader.GetString(2),
                                    Lote = reader.GetString(3),
                                    Prototipo = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    Obra = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    FechaSolicitud = reader.GetDateTime(6),
                                    TotalImporte = reader.IsDBNull(7) ? 0 : Convert.ToDecimal(reader.GetValue(7)),
                                    NumeroSalida = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                                    Usuario = reader.IsDBNull(9) ? "" : reader.GetString(9),
                                    Solicitante = reader.IsDBNull(10) ? "" : reader.GetString(10),
                                    ResidenteObra = reader.IsDBNull(11) ? "" : reader.GetString(11),
                                    EncargadoAlmacen = reader.IsDBNull(12) ? "" : reader.GetString(12),
                                    Estado = reader.IsDBNull(13) ? "PENDIENTE" : reader.GetString(13),
                                    TotalInsumos = reader.GetInt32(14)
                                });
                            }
                        }
                    }
                }

                olvVales.SetObjects(vales);
                ActualizarContadores(vales);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar vales: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarContadores(List<ValeSalidaInfo> vales)
        {
            int total = vales.Count;
            decimal sumaTotales = vales.Sum(v => v.TotalImporte);
            
            lblContador.Text = $"Total: {total} vale(s) | Suma: {sumaTotales:C2}";
        }

        private void VerPDF()
        {
            var seleccionado = olvVales.SelectedObject as ValeSalidaInfo;
            if (seleccionado == null)
            {
                MessageBox.Show("Selecciona un vale de la lista", "Atención", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT ContenidoPDF, NombreArchivo 
                        FROM PDFsSalidaAlmacen 
                        WHERE FolioId = @folioId";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@folioId", seleccionado.Id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                byte[] pdfBytes = (byte[])reader["ContenidoPDF"];
                                string nombreArchivo = reader["NombreArchivo"].ToString();

                                // Guardar temporalmente y abrir
                                string tempPath = Path.Combine(Path.GetTempPath(), nombreArchivo);
                                File.WriteAllBytes(tempPath, pdfBytes);

                                try
                                {
                                    ProcessStartInfo psi = new ProcessStartInfo(tempPath) { UseShellExecute = true };
                                    Process.Start(psi);
                                }
                                catch
                                {
                                    MessageBox.Show($"PDF guardado en: {tempPath}\n\nÁbrelo manualmente si no se abrió automáticamente.", 
                                        "PDF Extraído", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show("No se encontró el PDF almacenado para este vale", "Sin PDF", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
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
            var seleccionado = olvVales.SelectedObject as ValeSalidaInfo;
            if (seleccionado == null)
            {
                MessageBox.Show("Selecciona un vale de la lista", "Atención", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var formDetalle = new FormDetalleValeSalida(seleccionado.Id, seleccionado.Folio))
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

        private void EliminarVale()
        {
            var seleccionado = olvVales.SelectedObject as ValeSalidaInfo;
            if (seleccionado == null)
            {
                MessageBox.Show("Selecciona un vale de la lista", "Atención", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"¿Estás seguro de eliminar este vale de salida?\n\n" +
                $"Folio: {seleccionado.Folio}\n" +
                $"Casa: M{seleccionado.Manzana}-L{seleccionado.Lote}\n" +
                $"Solicitante: {seleccionado.Solicitante}\n" +
                $"Total: {seleccionado.TotalImporte:C2}\n\n" +
                $"Esta acción NO SE PUEDE DESHACER.\n" +
                $"Se eliminará el folio, detalle y PDF almacenado.",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // La eliminación en cascada se encargará del detalle y PDF
                    string sql = "DELETE FROM FoliosSalidaAlmacen WHERE Id = @id";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", seleccionado.Id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Vale eliminado exitosamente", "Éxito", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            // Recargar lista
                            CargarVales();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar vale: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltros()
        {
            string textoBusqueda = txtBuscar.Text.Trim().ToLower();
            DateTime? fechaDesde = chkFechaDesde.Checked ? dtpDesde.Value.Date : (DateTime?)null;
            DateTime? fechaHasta = chkFechaHasta.Checked ? dtpHasta.Value.Date.AddDays(1).AddSeconds(-1) : (DateTime?)null;

            var todosVales = olvVales.Objects.Cast<ValeSalidaInfo>().ToList();
            var valesFiltrados = todosVales.Where(v =>
            {
                bool cumpleBusqueda = string.IsNullOrEmpty(textoBusqueda) ||
                    v.Folio.ToLower().Contains(textoBusqueda) ||
                    (v.Solicitante != null && v.Solicitante.ToLower().Contains(textoBusqueda)) ||
                    (v.Obra != null && v.Obra.ToLower().Contains(textoBusqueda));

                bool cumpleFechaDesde = !fechaDesde.HasValue || v.FechaSolicitud >= fechaDesde.Value;
                bool cumpleFechaHasta = !fechaHasta.HasValue || v.FechaSolicitud <= fechaHasta.Value;

                return cumpleBusqueda && cumpleFechaDesde && cumpleFechaHasta;
            }).ToList();

            olvVales.SetObjects(valesFiltrados);
            ActualizarContadores(valesFiltrados);
        }

        private void btnVerPDF_Click(object sender, EventArgs e) => VerPDF();
        private void btnDetalle_Click(object sender, EventArgs e) => MostrarDetalle();
        private void btnEliminar_Click(object sender, EventArgs e) => EliminarVale();
        private void btnActualizar_Click(object sender, EventArgs e) => CargarVales();
        private void btnCerrar_Click(object sender, EventArgs e) => this.Close();
        private void txtBuscar_TextChanged(object sender, EventArgs e) => AplicarFiltros();
        private void chkFechaDesde_CheckedChanged(object sender, EventArgs e) => AplicarFiltros();
        private void chkFechaHasta_CheckedChanged(object sender, EventArgs e) => AplicarFiltros();
        private void dtpDesde_ValueChanged(object sender, EventArgs e) => AplicarFiltros();
        private void dtpHasta_ValueChanged(object sender, EventArgs e) => AplicarFiltros();
    }

    public class ValeSalidaInfo
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string Obra { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public decimal TotalImporte { get; set; }
        public int? NumeroSalida { get; set; }
        public string Usuario { get; set; }
        public string Solicitante { get; set; }
        public string ResidenteObra { get; set; }
        public string EncargadoAlmacen { get; set; }
        public string Estado { get; set; }
        public int TotalInsumos { get; set; }
    }
}
