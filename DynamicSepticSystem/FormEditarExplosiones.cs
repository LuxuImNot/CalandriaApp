using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public class FormEditarExplosiones : Form
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CalandriaConn"]?.ConnectionString;
        private ComboBox cmbPrototipos;
        private ListView lvExplosiones;
        private Button btnCargar;
        private Button btnGuardar;
        private Button btnAgregar;
        private Button btnEliminar;
        private TextBox txtClave;
        private TextBox txtDescripcion;
        private TextBox txtUnidad;
        private NumericUpDown nudCantidad;
        private NumericUpDown nudCosto;
        private Label lblStatus;

        public FormEditarExplosiones()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
        }

        private void InitializeComponent()
        {
            this.Text = "Editor de Explosiones de Insumos";
            this.Size = new Size(1050, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            cmbPrototipos = new ComboBox { Left = 10, Top = 10, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            btnCargar = new Button { Left = 320, Top = 8, Width = 100, Text = "Cargar" };
            btnCargar.Click += BtnCargar_Click;

            lvExplosiones = new ListView { Left = 10, Top = 40, Width = 910, Height = 350, View = View.Details, FullRowSelect = true }; 
            lvExplosiones.Columns.Add("Clave", 100);
            lvExplosiones.Columns.Add("Descripcion", 280);
            lvExplosiones.Columns.Add("Unidad", 70);
            lvExplosiones.Columns.Add("Cantidad", 80);
            lvExplosiones.Columns.Add("Costo", 90);
            lvExplosiones.Columns.Add("Familia", 100);
            lvExplosiones.SelectedIndexChanged += LvExplosiones_SelectedIndexChanged;

            // Fila de edici�n con etiquetas
            Label lblClave = new Label { Left = 10, Top = 400, Width = 50, Text = "Clave:", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            txtClave = new TextBox { Left = 65, Top = 398, Width = 100 };
            
            Label lblDesc = new Label { Left = 170, Top = 400, Width = 70, Text = "Descripci�n:", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            txtDescripcion = new TextBox { Left = 245, Top = 398, Width = 250 };
            
            Label lblUnidad = new Label { Left = 500, Top = 400, Width = 50, Text = "Unidad:", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            txtUnidad = new TextBox { Left = 555, Top = 398, Width = 70 };
            
            Label lblCantidad = new Label { Left = 630, Top = 400, Width = 60, Text = "Cantidad:", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            nudCantidad = new NumericUpDown { Left = 695, Top = 398, Width = 80, DecimalPlaces = 3, Maximum = 1000000 };

            Label lblCostoLbl = new Label { Left = 780, Top = 400, Width = 50, Text = "Costo:", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            nudCosto = new NumericUpDown { Left = 835, Top = 398, Width = 85, DecimalPlaces = 2, Maximum = 9999999 };

            btnAgregar = new Button { Left = 730, Top = 448, Width = 100, Text = "Agregar/Actualizar" };
            btnAgregar.Click += BtnAgregar_Click;

            btnEliminar = new Button { Left = 835, Top = 448, Width = 85, Text = "Eliminar" };
            btnEliminar.Click += BtnEliminar_Click;

            btnGuardar = new Button { Left = 10, Top = 500, Width = 120, Text = "Guardar Cambios" };
            btnGuardar.Click += BtnGuardar_Click;

            lblStatus = new Label { Left = 140, Top = 504, Width = 600, ForeColor = Color.DarkGreen };

            this.Controls.Add(cmbPrototipos);
            this.Controls.Add(btnCargar);
            this.Controls.Add(lvExplosiones);
            this.Controls.Add(lblClave);
            this.Controls.Add(txtClave);
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescripcion);
            this.Controls.Add(lblUnidad);
            this.Controls.Add(txtUnidad);
            this.Controls.Add(lblCantidad);
            this.Controls.Add(nudCantidad);
            this.Controls.Add(lblCostoLbl);
            this.Controls.Add(nudCosto);
            this.Controls.Add(btnAgregar);
            this.Controls.Add(btnEliminar);
            this.Controls.Add(btnGuardar);
            this.Controls.Add(lblStatus);

            LoadPrototipos();
        }

        private void LoadPrototipos()
        {
            // Leer prototipos desde InventarioCasas distinct
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT Prototipo FROM InventarioCasas ORDER BY Prototipo", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbPrototipos.Items.Add(reader[0].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando prototipos: " + ex.Message);
            }
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            if (cmbPrototipos.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un prototipo.");
                return;
            }

            string prot = cmbPrototipos.SelectedItem.ToString();
            string tabla = GetExplosionTableForPrototipo(prot);

            lvExplosiones.Items.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand($"SELECT Clave, Descripcion, Unidad, Cantidad, Familia, ISNULL(Costo,0) AS Costo FROM {tabla} ORDER BY Clave", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var it = new ListViewItem(reader["Clave"].ToString());
                            it.SubItems.Add(reader["Descripcion"].ToString());
                            it.SubItems.Add(reader["Unidad"].ToString());
                            it.SubItems.Add(reader["Cantidad"].ToString());
                            it.SubItems.Add(reader["Costo"].ToString());
                            it.SubItems.Add(reader["Familia"].ToString());
                            it.Tag = reader["Clave"].ToString();
                            lvExplosiones.Items.Add(it);
                        }
                    }
                }
                lblStatus.Text = $"Cargados {lvExplosiones.Items.Count} registros de {tabla}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando explosiones: " + ex.Message);
            }
        }

        private void LvExplosiones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvExplosiones.SelectedItems.Count == 0) return;
            var it = lvExplosiones.SelectedItems[0];
            txtClave.Text = it.SubItems[0].Text;
            txtDescripcion.Text = it.SubItems[1].Text;
            txtUnidad.Text = it.SubItems[2].Text;
            if (decimal.TryParse(it.SubItems[3].Text, out decimal c)) nudCantidad.Value = c;
            if (decimal.TryParse(it.SubItems[4].Text, out decimal costo)) nudCosto.Value = costo;
        }

        private string GetExplosionTableForPrototipo(string prototipo)
        {
            if (string.IsNullOrWhiteSpace(prototipo)) return "COMPRASCALANDRA";
            var p = prototipo.ToUpperInvariant();
            if (p.Contains("TUNERA")) return "COMPRASTUNERA";
            return "COMPRASCALANDRA";
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            if (cmbPrototipos.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un prototipo.");
                return;
            }
            string prot = cmbPrototipos.SelectedItem.ToString();
            string tabla = GetExplosionTableForPrototipo(prot);
            string clave = txtClave.Text.Trim();
            if (string.IsNullOrEmpty(clave)) { MessageBox.Show("Clave requerida"); return; }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmdCheck = new SqlCommand($"SELECT COUNT(*) FROM {tabla} WHERE Clave=@c", conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@c", clave);
                        int existe = (int)cmdCheck.ExecuteScalar();
                        if (existe > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand($"UPDATE {tabla} SET Descripcion=@desc, Unidad=@unidad, Cantidad=@cant, Costo=@costo, Familia=@fam WHERE Clave=@c", conn))
                            {
                                cmd.Parameters.AddWithValue("@desc", txtDescripcion.Text);
                                cmd.Parameters.AddWithValue("@unidad", txtUnidad.Text);
                                cmd.Parameters.AddWithValue("@cant", nudCantidad.Value);
                                cmd.Parameters.AddWithValue("@costo", nudCosto.Value);
                                cmd.Parameters.AddWithValue("@fam", "MANUAL");
                                cmd.Parameters.AddWithValue("@c", clave);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (SqlCommand cmd = new SqlCommand($"INSERT INTO {tabla} (Clave, Descripcion, Unidad, Cantidad, Costo, Familia) VALUES (@c, @desc, @unidad, @cant, @costo, @fam)", conn))
                            {
                                cmd.Parameters.AddWithValue("@c", clave);
                                cmd.Parameters.AddWithValue("@desc", txtDescripcion.Text);
                                cmd.Parameters.AddWithValue("@unidad", txtUnidad.Text);
                                cmd.Parameters.AddWithValue("@cant", nudCantidad.Value);
                                cmd.Parameters.AddWithValue("@costo", nudCosto.Value);
                                cmd.Parameters.AddWithValue("@fam", "MANUAL");
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                BtnCargar_Click(null, null);
                lblStatus.Text = "Registro agregado/actualizado";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar/actualizar: " + ex.Message);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (cmbPrototipos.SelectedItem == null) { MessageBox.Show("Selecciona un prototipo."); return; }
            if (lvExplosiones.SelectedItems.Count == 0) { MessageBox.Show("Selecciona registro a eliminar."); return; }
            string prot = cmbPrototipos.SelectedItem.ToString();
            string tabla = GetExplosionTableForPrototipo(prot);
            string clave = lvExplosiones.SelectedItems[0].SubItems[0].Text;

            var conf = MessageBox.Show($"Eliminar {clave} de {tabla}?","Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (conf != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand($"DELETE FROM {tabla} WHERE Clave=@c", conn))
                    {
                        cmd.Parameters.AddWithValue("@c", clave);
                        cmd.ExecuteNonQuery();
                    }
                }
                BtnCargar_Click(null, null);
                lblStatus.Text = "Registro eliminado";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar: " + ex.Message);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Actualmente las operaciones se ejecutan directamente en BD, as� que aqu� solo refrescamos
            BtnCargar_Click(null, null);
            lblStatus.Text = "Guardado/Refrescado";
        }
    }
}
