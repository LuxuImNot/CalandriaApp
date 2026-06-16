using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Visor administrativo del registro de errores (tabla LogErrores).
    /// Muestra los errores reportados por todos los equipos para diagnóstico.
    /// </summary>
    public class FormLogErrores : Form
    {
        private readonly string connectionString = ErrorLogger.ObtenerCadenaConexion();

        private DataGridView grid;
        private TextBox txtBuscar;
        private ComboBox cboRango;
        private Label lblInfo;

        public FormLogErrores()
        {
            ConstruirUI();
            ThemeManager.AplicarTema(this);
            this.Load += (s, e) => CargarDatos();
        }

        private void ConstruirUI()
        {
            this.Text = "Registro de Errores";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1000, 600);
            this.MinimumSize = new Size(700, 400);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };

            var btnRefrescar = new Button
            {
                Text = "🔄 Refrescar",
                Width = 110,
                Height = 30,
                Left = 8,
                Top = 9,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White
            };
            btnRefrescar.FlatAppearance.BorderSize = 0;
            btnRefrescar.Click += (s, e) => CargarDatos();

            var lblRango = new Label { Text = "Periodo:", Left = 130, Top = 15, Width = 55, TextAlign = ContentAlignment.MiddleLeft };
            cboRango = new ComboBox
            {
                Left = 185,
                Top = 11,
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboRango.Items.AddRange(new object[] { "Últimas 24 h", "Últimos 7 días", "Últimos 30 días", "Todos" });
            cboRango.SelectedIndex = 1;
            cboRango.SelectedIndexChanged += (s, e) => CargarDatos();

            var lblBuscar = new Label { Text = "Buscar:", Left = 330, Top = 15, Width = 50, TextAlign = ContentAlignment.MiddleLeft };
            txtBuscar = new TextBox { Left = 380, Top = 11, Width = 220 };
            txtBuscar.TextChanged += (s, e) => AplicarFiltro();

            var btnDetalle = new Button
            {
                Text = "Ver detalle",
                Width = 100,
                Height = 30,
                Left = 615,
                Top = 9,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White
            };
            btnDetalle.FlatAppearance.BorderSize = 0;
            btnDetalle.Click += (s, e) => MostrarDetalle();

            var btnLimpiar = new Button
            {
                Text = "Eliminar todo",
                Width = 110,
                Height = 30,
                Left = 720,
                Top = 9,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White
            };
            btnLimpiar.FlatAppearance.BorderSize = 0;
            btnLimpiar.Click += (s, e) => EliminarTodo();

            panelTop.Controls.AddRange(new Control[] { btnRefrescar, lblRango, cboRango, lblBuscar, txtBuscar, btnDetalle, btnLimpiar });

            lblInfo = new Label { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AllowUserToResizeRows = false,
                BackgroundColor = Color.White
            };
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) MostrarDetalle(); };

            this.Controls.Add(grid);
            this.Controls.Add(lblInfo);
            this.Controls.Add(panelTop);
        }

        private string ClausulaRango()
        {
            switch (cboRango.SelectedIndex)
            {
                case 0: return "WHERE Fecha >= DATEADD(HOUR, -24, GETDATE())";
                case 1: return "WHERE Fecha >= DATEADD(DAY, -7, GETDATE())";
                case 2: return "WHERE Fecha >= DATEADD(DAY, -30, GETDATE())";
                default: return "";
            }
        }

        private void CargarDatos()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                lblInfo.Text = "No se encontró la cadena de conexión 'CalandriaConn'.";
                return;
            }

            try
            {
                var tabla = new DataTable();
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Si la tabla aún no existe (nunca hubo errores), mostrar vacío sin fallar.
                    using (var check = new SqlCommand(
                        "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LogErrores') THEN 1 ELSE 0 END", conn))
                    {
                        if (Convert.ToInt32(check.ExecuteScalar()) == 0)
                        {
                            grid.DataSource = null;
                            lblInfo.Text = "Aún no hay errores registrados.";
                            return;
                        }
                    }

                    string sql = $@"
                        SELECT TOP 1000 ID, Fecha, Equipo, Usuario, Version, Origen,
                               TipoExcepcion, Mensaje, StackTrace
                        FROM LogErrores
                        {ClausulaRango()}
                        ORDER BY Fecha DESC";

                    using (var da = new SqlDataAdapter(sql, conn))
                    {
                        da.Fill(tabla);
                    }
                }

                grid.DataSource = tabla;
                ConfigurarColumnas();
                AplicarFiltro();
                lblInfo.Text = $"{tabla.Rows.Count} error(es) en el periodo seleccionado.";
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Error al cargar el registro: " + ex.Message;
            }
        }

        private void ConfigurarColumnas()
        {
            if (grid.Columns.Count == 0) return;

            SetCol("ID", "ID", 50);
            SetCol("Fecha", "Fecha", 130);
            SetCol("Equipo", "Equipo", 100);
            SetCol("Usuario", "Usuario", 100);
            SetCol("Version", "Versión", 60);
            SetCol("Origen", "Origen", 110);
            SetCol("TipoExcepcion", "Excepción", 150);
            SetCol("Mensaje", "Mensaje", 260);

            if (grid.Columns.Contains("StackTrace"))
                grid.Columns["StackTrace"].Visible = false; // se ve en "Ver detalle"

            if (grid.Columns.Contains("Fecha"))
                grid.Columns["Fecha"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
        }

        private void SetCol(string nombre, string titulo, int ancho)
        {
            if (!grid.Columns.Contains(nombre)) return;
            grid.Columns[nombre].HeaderText = titulo;
            grid.Columns[nombre].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns[nombre].Width = ancho;
        }

        private void AplicarFiltro()
        {
            var dt = grid.DataSource as DataTable;
            if (dt == null) return;

            string texto = txtBuscar.Text.Replace("'", "''").Trim();
            if (string.IsNullOrEmpty(texto))
            {
                dt.DefaultView.RowFilter = "";
            }
            else
            {
                dt.DefaultView.RowFilter =
                    $"Mensaje LIKE '%{texto}%' OR TipoExcepcion LIKE '%{texto}%' " +
                    $"OR Equipo LIKE '%{texto}%' OR Usuario LIKE '%{texto}%' OR Origen LIKE '%{texto}%'";
            }
        }

        private void MostrarDetalle()
        {
            if (grid.CurrentRow == null) return;

            string Get(string col) =>
                grid.Columns.Contains(col) && grid.CurrentRow.Cells[col].Value != null
                    ? grid.CurrentRow.Cells[col].Value.ToString()
                    : "";

            string detalle =
                $"Fecha:     {Get("Fecha")}\r\n" +
                $"Equipo:    {Get("Equipo")}\r\n" +
                $"Usuario:   {Get("Usuario")}\r\n" +
                $"Versión:   {Get("Version")}\r\n" +
                $"Origen:    {Get("Origen")}\r\n" +
                $"Excepción: {Get("TipoExcepcion")}\r\n" +
                $"Mensaje:   {Get("Mensaje")}\r\n\r\n" +
                "----- Stack trace -----\r\n" +
                Get("StackTrace");

            using (var dlg = new Form())
            {
                dlg.Text = "Detalle del error";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.Size = new Size(820, 560);
                var txt = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Both,
                    WordWrap = false,
                    Font = new Font("Consolas", 9F),
                    Text = detalle
                };
                var pnl = new Panel { Dock = DockStyle.Bottom, Height = 44 };
                var btnCopiar = new Button
                {
                    Text = "Copiar",
                    Width = 100,
                    Height = 30,
                    Top = 7,
                    Left = 8,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White
                };
                btnCopiar.FlatAppearance.BorderSize = 0;
                btnCopiar.Click += (s, e) => { try { Clipboard.SetText(detalle); } catch { } };
                var btnCerrar = new Button
                {
                    Text = "Cerrar",
                    Width = 100,
                    Height = 30,
                    Top = 7,
                    Left = 116,
                    DialogResult = DialogResult.OK,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(189, 195, 199),
                    ForeColor = Color.White
                };
                btnCerrar.FlatAppearance.BorderSize = 0;
                pnl.Controls.Add(btnCopiar);
                pnl.Controls.Add(btnCerrar);
                dlg.Controls.Add(txt);
                dlg.Controls.Add(pnl);
                dlg.ShowDialog(this);
            }
        }

        private void EliminarTodo()
        {
            if (string.IsNullOrEmpty(connectionString)) return;

            var r = MessageBox.Show(
                "¿Eliminar TODOS los errores registrados?\n\nEsta acción no se puede deshacer.",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "IF EXISTS (SELECT 1 FROM sys.tables WHERE name='LogErrores') DELETE FROM LogErrores", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo limpiar el registro:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
