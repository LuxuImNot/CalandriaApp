using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormInsumos : Form
    {
        public Tarea Tarea { get; private set; }
        // Expose the grid so callers can inspect data after dialog
        public System.Windows.Forms.DataGridView GridInsumos => dgvInsumos;
        private PanelPrincipal.Casa _casa;
        private CasaInventario _inventario;
        private string _connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        private readonly InventarioService _svc = new InventarioService();

        public FormInsumos()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
        }

        public FormInsumos(Tarea tarea) : this()
        {
            Tarea = tarea;
            lblTitle.Text = "Formulario Insumos para: " + Tarea?.Nombre;
            this.Text = "Insumos - " + (Tarea?.Nombre ?? "");
        }

        public void SetCasa(PanelPrincipal.Casa casa, CasaInventario inventario)
        {
            _casa = casa;
            _inventario = inventario;
            LoadInsumosForPrototipo(inventario?.Prototipo);

            // try to load previously saved entregados for this destajo
            try
            {
                if (_casa != null)
                {
                    var dtSaved = _svc.ObtenerInsumosEntregados(_casa.Manzana, _casa.Lote, Tarea?.WBS);
                    if (dtSaved != null && dgvInsumos.DataSource is System.Data.DataTable dtCurrent)
                    {
                        // For each saved row, try to find matching Clave and mark Entregado
                        var keys = dtSaved.Columns.Contains("Clave") ? dtSaved.AsEnumerable().Select(r => r["Clave"]?.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() : null;
                        if (keys != null && keys.Count > 0)
                        {
                            foreach (System.Data.DataRow row in dtCurrent.Rows)
                            {
                                var clave = row.Table.Columns.Contains("Clave") ? row["Clave"]?.ToString() : null;
                                if (!string.IsNullOrWhiteSpace(clave) && keys.Contains(clave)) row["Entregado"] = true;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void LoadInsumosForPrototipo(string prototipo)
        {
            if (string.IsNullOrWhiteSpace(prototipo))
            {
                MessageBox.Show("Prototipo inv�lido.");
                return;
            }

            string tabla = prototipo.ToUpper().Contains("TUNERA") ? "InsumosTuneraEXP" : "InsumosCalandraEXP";

            string sql = $@"SELECT Clave, Descripci�n, Unidad, Cantidad, Costo, Importe, Porcentaje FROM dbo.{tabla} ORDER BY Clave";

            var dt = new DataTable();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener insumos: " + ex.Message);
                return;
            }

            // A�adir columna local "Entregado" (checkbox)
            if (!dt.Columns.Contains("Entregado"))
                dt.Columns.Add("Entregado", typeof(bool));

            // Asegurar columnas en nombres esperados
            foreach (DataRow r in dt.Rows)
            {
                if (r["Cantidad"] == DBNull.Value) r["Cantidad"] = 0;
                if (r["Costo"] == DBNull.Value) r["Costo"] = 0;
                if (r["Importe"] == DBNull.Value) r["Importe"] = 0;
                if (r["Porcentaje"] == DBNull.Value) r["Porcentaje"] = 0;
            }

            dgvInsumos.DataSource = dt;

            // Cambiar tipo de columna Entregado a DataGridViewCheckBoxColumn
            if (!(dgvInsumos.Columns["Entregado"] is DataGridViewCheckBoxColumn))
            {
                var idx = dt.Columns.IndexOf("Entregado");
                dgvInsumos.Columns.Remove("Entregado");
                var chk = new DataGridViewCheckBoxColumn { Name = "Entregado", HeaderText = "Entregado", DataPropertyName = "Entregado", Width = 60 };
                dgvInsumos.Columns.Insert(0, chk);
            }

            // Formatear algunas columnas
            if (dgvInsumos.Columns.Contains("Cantidad")) dgvInsumos.Columns["Cantidad"].DefaultCellStyle.Format = "0.##";
            if (dgvInsumos.Columns.Contains("Costo")) dgvInsumos.Columns["Costo"].DefaultCellStyle.Format = "C2";
            if (dgvInsumos.Columns.Contains("Importe")) dgvInsumos.Columns["Importe"].DefaultCellStyle.Format = "C2";
            if (dgvInsumos.Columns.Contains("Porcentaje")) dgvInsumos.Columns["Porcentaje"].DefaultCellStyle.Format = "0.##";

            dgvInsumos.Refresh();
        }

        // Devuelve una tabla con los insumos marcados como entregados en el grid actual
        public System.Data.DataTable GetEntregadosTable()
        {
            var dt = dgvInsumos.DataSource as System.Data.DataTable;
            if (dt == null) return null;
            var entregados = dt.Clone();
            foreach (System.Data.DataRow r in dt.Rows)
            {
                try
                {
                    var val = r["Entregado"];
                    bool b = false;
                    if (val != DBNull.Value && val != null)
                        b = Convert.ToBoolean(val);
                    if (b) entregados.ImportRow(r);
                }
                catch { }
            }
            return entregados;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                // persist entregados via service if we have casa
                if (_casa != null && dgvInsumos.DataSource is System.Data.DataTable dt)
                {
                    _svc.GuardarInsumosEntregados(_casa.Manzana, _casa.Lote, Tarea?.WBS, GetEntregadosTable());
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error guardando insumos entregados: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
