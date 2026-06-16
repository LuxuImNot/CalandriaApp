// Core functionality and initialization for FormAlmacen
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Configuration;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        private PanelPrincipal.Casa casaActual;
        private DynamicSepticSystem.CasaInventario casaInventario;
        string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        private Dictionary<string, double> explosion;

        public FormAlmacen()
        {
            InitializeComponent();
            ConfigurarColumnasHistorial();
            CargarHistorial();
            this.dgvEntrada.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dgvEntrada_RowPrePaint);
            cmbOrdenesCompra.DropDownStyle = ComboBoxStyle.DropDown; // <== permite escribir
            cmbOrdenesCompra.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbOrdenesCompra.AutoCompleteSource = AutoCompleteSource.CustomSource;
            cmbOrdenesCompra.SelectedIndexChanged += (s, e) => CargarInsumosDesdeOrdenSeleccionada();
            btnRegistrarSalida.Click += (s, e) => RegistrarSalida();
            dgvEntrada.CurrentCellDirtyStateChanged += (s, ev) =>
            {
                if (dgvEntrada.IsCurrentCellDirty)
                {
                    dgvEntrada.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    ActualizarResumenOrden();
                }
            };
            btnCapturarEntrada.Click += BtnCapturarEntrada_Click;
            btnBuscarOrden.Click += (s, e) =>
            {
                CargarOrdenesDeCompra();
                if (casaActual != null && casaInventario != null)
                    lblCasa.Text = $"?? M{casaActual.Manzana}-L{casaActual.Lote} ({casaInventario.Prototipo})";
                else
                    lblCasa.Text = "Sin casa seleccionada";
            };

            // ?? APLICAR TEMA CORPORATIVO
            AplicarTemaCorporativo();
            
            // ?? CONFIGURAR LAYOUT RESPONSIVO (después de InitializeComponent)
            ConfigurarLayoutResponsivo();
            
            // Manejar evento de redimensionamiento
            this.Resize += FormAlmacen_Resize;
            this.Load += FormAlmacen_Load;
        }

        /// <summary>
        /// Evento Load para configurar el layout inicial
        /// </summary>
        private void FormAlmacen_Load(object sender, EventArgs e)
        {
            // Forzar ajuste inicial del layout
            FormAlmacen_Resize(this, EventArgs.Empty);
            
            // ? CARGAR AUTOMÁTICAMENTE LAS ÓRDENES DE COMPRA AL INICIAR
            CargarOrdenesDeCompra();
            
            // Si hay órdenes disponibles, seleccionar la primera automáticamente
            if (cmbOrdenesCompra.Items.Count > 0)
            {
                cmbOrdenesCompra.SelectedIndex = 0;
                // Cargar automáticamente los insumos de la primera orden
                CargarInsumosDesdeOrdenSeleccionada();
            }
            
            // ? CARGAR INVENTARIO COMPLETO AL ABRIR LA PESTAÑA DE INVENTARIO
            CargarInventarioCompleto();
        }

        public void SetCasaActual(PanelPrincipal.Casa casa, DynamicSepticSystem.CasaInventario inventario)
        {
            casaActual = casa;
            casaInventario = inventario;
            explosion = CargarExplosionDesdeSQL(casaInventario.Prototipo);
            lblCasa.Text = $"? M{casa.Manzana}-L{casa.Lote} ({casaInventario.Prototipo})";
            lblCasa.ForeColor = ThemeManager.ColorExito;
            CargarOrdenesDeCompra();
        }

        private void ActualizarResumenOrden()
        {
            if (dgvEntrada.DataSource is DataTable dt)
            {
                int seleccionados = dt.AsEnumerable().Count(r =>
                    dt.Columns.Contains("CantidadRecibida") &&
                    r["CantidadRecibida"] != DBNull.Value &&
                    !string.IsNullOrWhiteSpace(r["CantidadRecibida"].ToString()));

                lblSummary.Text = $"Insumos con cantidad capturada: {seleccionados}";
            }
        }

        public void CargarExplosionParaSalida(string prototipo)
        {
            explosion = new Dictionary<string, double>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT Clave, Cantidad FROM ExplosionInsumos WHERE Prototipo = @Prototipo", conn))
            {
                cmd.Parameters.AddWithValue("@Prototipo", prototipo);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string clave = reader.GetString(0);
                        double cantidad = reader.IsDBNull(1) ? 0 : Convert.ToDouble(reader[1]);
                        explosion[clave] = cantidad;
                    }
                }
            }

            CargarInsumosDisponiblesSalida();
        }

        private Dictionary<string, double> CargarExplosionDesdeSQL(string prototipo)
        {
            var mapa = new Dictionary<string, double>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT Clave, Cantidad FROM ExplosionInsumos WHERE Prototipo = @Prototipo", conn))
            {
                cmd.Parameters.AddWithValue("@Prototipo", prototipo);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string clave = reader.GetString(0);
                        double cantidad = reader.IsDBNull(1) ? 0 : Convert.ToDouble(reader[1]);
                        mapa[clave] = cantidad;
                    }
                }
            }
            return mapa;
        }
    }
}
