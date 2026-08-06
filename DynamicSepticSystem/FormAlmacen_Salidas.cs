// Database operations for exits (salidas) - FormAlmacen
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        // Vista de SALIDAS en TARJETAS (no grilla). El DataTable sigue siendo el modelo
        // (tambien asignado a dgvInsumos, oculto) que usa RegistrarSalida y el vale PDF.
        private DataTable _dtSalida;
        private FlowLayoutPanel flpSalidas;
        private Panel pnlHeaderSalidas;
        private Label lblHeaderSalidas;
        private string _resumenSalida = "";
        private int _anchoUltimoRenderSalida = -1; // ancho con el que se pintaron las bandas de grupo

        private void TxtBuscarSalida_TextChanged(object sender, EventArgs e)
        {
            // Filtra las tarjetas (no la grilla) por destajo / clave / insumo.
            RenderTarjetasSalida(txtBuscarSalida?.Text);
        }

        /// <summary>
        /// Surtir Todo: pone en cada insumo pendiente la cantidad solicitada al MÁXIMO
        /// (lo pendiente). Escribe en el DataTable (modelo de RegistrarSalida) y vuelve a
        /// pintar para que los NumericUpDown reflejen el tope. No registra: solo prellena.
        /// </summary>
        private void BtnSurtirTodo_Click(object sender, EventArgs e)
        {
            if (_dtSalida == null || _dtSalida.Rows.Count == 0 || !_dtSalida.Columns.Contains("MaximoPermitido"))
            {
                MessageBox.Show("Selecciona una casa con insumos por surtir primero.",
                    "Surtir Todo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int maxeados = 0;
            foreach (DataRow row in _dtSalida.Rows)
            {
                decimal pendiente = row["MaximoPermitido"] != DBNull.Value
                    ? Convert.ToDecimal(row["MaximoPermitido"]) : 0m;
                if (pendiente <= 0m) continue;

                decimal precio = row["PrecioUnitario"] != DBNull.Value
                    ? Convert.ToDecimal(row["PrecioUnitario"]) : 0m;
                row["CantidadSolicitada"] = pendiente;
                row["Importe"] = pendiente * precio;
                maxeados++;
            }

            if (maxeados == 0)
            {
                MessageBox.Show("No hay insumos pendientes: todo está surtido.",
                    "Surtir Todo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Refresca las tarjetas para que los NumericUpDown muestren el máximo.
            RenderTarjetasSalida(txtBuscarSalida?.Text);
        }

        /// <summary>
        /// Muestra un mensaje inicial en el DataGridView de salidas pidiendo seleccionar insumos
        /// </summary>
        private void MostrarMensajeSeleccionInsumosSalida()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Mensaje", typeof(string));
            
            DataRow row = dt.NewRow();
            row["Mensaje"] = "\U0001F4E6 Seleccione insumo(s) a dirigir a esta casa...";
            dt.Rows.Add(row);
            
            dgvInsumos.DataSource = dt;
            
            // Estilizar el mensaje
            if (dgvInsumos.Columns.Count > 0)
            {
                dgvInsumos.Columns[0].HeaderText = "";
                dgvInsumos.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvInsumos.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvInsumos.Columns[0].DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                dgvInsumos.Columns[0].DefaultCellStyle.ForeColor = ThemeManager.ColorPrincipalMenuBar;
                dgvInsumos.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 240);
                dgvInsumos.RowTemplate.Height = 60;
            }
            
            dgvInsumos.ClearSelection();
            dgvInsumos.AllowUserToAddRows = false;
            dgvInsumos.ReadOnly = true;

            // Vista de tarjetas: estado vacio.
            _dtSalida = null;
            _resumenSalida = "";
            RenderTarjetasSalida();
        }

        // ===================================================================
        // Vista de SALIDAS en TARJETAS
        // ===================================================================

        /// <summary>
        /// Repinta la galeria de tarjetas de salida desde <see cref="_dtSalida"/>,
        /// opcionalmente filtrada por texto (destajo / clave / insumo).
        /// </summary>
        private void RenderTarjetasSalida(string filtro = null)
        {
            if (flpSalidas == null) return;
            _anchoUltimoRenderSalida = flpSalidas.ClientSize.Width;

            flpSalidas.SuspendLayout();
            foreach (Control c in flpSalidas.Controls.Cast<Control>().ToList()) c.Dispose();
            flpSalidas.Controls.Clear();

            if (lblHeaderSalidas != null)
                lblHeaderSalidas.Text = string.IsNullOrEmpty(_resumenSalida)
                    ? "Selecciona una casa destino para ver los insumos por surtir de sus destajos activados."
                    : _resumenSalida;

            if (_dtSalida == null || _dtSalida.Rows.Count == 0 || !_dtSalida.Columns.Contains("MaximoPermitido"))
            {
                flpSalidas.Controls.Add(CrearTarjetaVaciaSalida(null));
                flpSalidas.ResumeLayout();
                return;
            }

            string f = (filtro ?? "").Trim();

            // Filtra primero y luego AGRUPA por destajo: cada grupo lleva una banda de
            // encabezado (ancho completo, con salto de fila) y debajo sus tarjetas. Así
            // los insumos de un mismo destajo quedan juntos sin cambiar la tarjeta.
            var filas = new List<DataRow>();
            foreach (DataRow row in _dtSalida.Rows)
            {
                if (f.Length > 0)
                {
                    string hay = (row["Destajo"]?.ToString() ?? "") + " " +
                                 (row["Clave"]?.ToString() ?? "") + " " +
                                 (row["Descripcion"]?.ToString() ?? "");
                    if (hay.IndexOf(f, StringComparison.OrdinalIgnoreCase) < 0) continue;
                }
                filas.Add(row);
            }

            if (filas.Count == 0)
            {
                flpSalidas.Controls.Add(CrearTarjetaVaciaSalida("Sin coincidencias para la búsqueda."));
                flpSalidas.ResumeLayout();
                return;
            }

            var grupos = filas
                .GroupBy(r => (r["Destajo"]?.ToString() ?? "—").Trim())
                .OrderBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase);

            foreach (var g in grupos)
            {
                var lista = g.ToList();

                var encabezado = CrearEncabezadoGrupoSalida(g.Key, lista);
                flpSalidas.Controls.Add(encabezado);
                flpSalidas.SetFlowBreak(encabezado, true); // las tarjetas empiezan debajo

                Control ultima = null;
                foreach (DataRow row in lista)
                {
                    var card = CrearTarjetaSalida(row);
                    flpSalidas.Controls.Add(card);
                    ultima = card;
                }
                // Cierra el grupo: el encabezado del siguiente destajo arranca en su fila.
                if (ultima != null) flpSalidas.SetFlowBreak(ultima, true);
            }

            flpSalidas.ResumeLayout();
        }

        /// <summary>
        /// Banda de encabezado de un grupo de destajo: ocupa el ancho del panel (salto de
        /// fila) y rotula el destajo + cuántos insumos lleva y cuántos siguen pendientes.
        /// Tono suave (verde si todo surtido, ámbar si hay pendientes) para no ser intrusivo.
        /// </summary>
        private Control CrearEncabezadoGrupoSalida(string destajo, List<DataRow> filas)
        {
            int total = filas.Count;
            int pendientes = filas.Count(r =>
                r["MaximoPermitido"] != DBNull.Value && Convert.ToDecimal(r["MaximoPermitido"]) > 0m);

            bool todoSurtido = pendientes == 0;
            Color verde = Color.FromArgb(39, 174, 96);
            Color ambar = Color.FromArgb(211, 84, 0);
            Color accent = todoSurtido ? verde : ambar;
            Color banda = todoSurtido ? Color.FromArgb(237, 247, 240) : Color.FromArgb(253, 246, 236);
            Color tinta = Color.FromArgb(44, 62, 80);

            int ancho = Math.Max(360, flpSalidas.ClientSize.Width - 24);

            var header = new Panel
            {
                Width = ancho,
                Height = 34,
                Margin = new Padding(8, 10, 8, 2),
                BackColor = banda
            };
            header.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent });

            header.Controls.Add(new Label
            {
                AutoSize = false,
                Location = new Point(16, 0),
                Size = new Size(ancho - 220, 34),
                Text = "\U0001F3D7 " + (string.IsNullOrEmpty(destajo) ? "—" : destajo).ToUpperInvariant(),
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = tinta,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            });

            string resumen = todoSurtido
                ? $"✓ {total} insumo(s) · completo"
                : $"{total} insumo(s) · {pendientes} por surtir";
            header.Controls.Add(new Label
            {
                AutoSize = false,
                Location = new Point(ancho - 210, 0),
                Size = new Size(200, 34),
                Text = resumen,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleRight
            });

            return header;
        }

        private Control CrearTarjetaVaciaSalida(string msg)
        {
            var p = new Panel
            {
                Width = 460,
                Height = 96,
                BackColor = Color.White,
                Margin = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle
            };
            p.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.FromArgb(127, 140, 141),
                Padding = new Padding(16),
                Text = msg ?? "\U0001F4E6 Selecciona una casa destino: se mostraran los insumos de sus destajos activados."
            });
            return p;
        }

        /// <summary>
        /// Tarjeta de un insumo: destajo, insumo, clave/unidad, precio, disponible,
        /// pendiente y captura de cantidad. Verde si ya surtido, rojo si pendiente.
        /// El NumericUpDown escribe directo en la fila del DataTable (modelo de
        /// RegistrarSalida), por lo que no hace falta una grilla.
        /// </summary>
        private Control CrearTarjetaSalida(DataRow row)
        {
            string destajo = row["Destajo"]?.ToString() ?? "—";
            string clave = row["Clave"]?.ToString() ?? "";
            string insumo = row["Descripcion"]?.ToString() ?? "";
            string unidad = row["Unidad"]?.ToString() ?? "";
            decimal precio = row["PrecioUnitario"] != DBNull.Value ? Convert.ToDecimal(row["PrecioUnitario"]) : 0m;
            decimal disponible = row["Disponible"] != DBNull.Value ? Convert.ToDecimal(row["Disponible"]) : 0m;
            decimal pendiente = row["MaximoPermitido"] != DBNull.Value ? Convert.ToDecimal(row["MaximoPermitido"]) : 0m;
            decimal solicitada = row["CantidadSolicitada"] != DBNull.Value ? Convert.ToDecimal(row["CantidadSolicitada"]) : 0m;

            bool surtido = pendiente <= 0m;
            Color verde = Color.FromArgb(39, 174, 96);
            Color rojo = Color.FromArgb(231, 76, 60);
            Color gris = Color.FromArgb(127, 140, 141);
            Color tinta = Color.FromArgb(44, 62, 80);
            Color accent = surtido ? verde : rojo;
            Color fondo = surtido ? Color.FromArgb(244, 250, 245) : Color.White;

            var card = new Panel
            {
                Width = 360,
                Height = 168,
                Margin = new Padding(8),
                BackColor = fondo,
                BorderStyle = BorderStyle.FixedSingle
            };
            card.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent });

            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), BackColor = Color.Transparent };
            card.Controls.Add(content);
            content.BringToFront();

            content.Controls.Add(new Label
            {
                AutoSize = false, Location = new Point(12, 8), Size = new Size(326, 16),
                Text = "\U0001F3D7 " + destajo.ToUpperInvariant(),
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold), ForeColor = accent, AutoEllipsis = true
            });
            content.Controls.Add(new Label
            {
                AutoSize = false, Location = new Point(12, 26), Size = new Size(326, 36),
                Text = string.IsNullOrEmpty(insumo) ? "(sin nombre)" : insumo,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold), ForeColor = tinta, AutoEllipsis = true
            });
            content.Controls.Add(new Label
            {
                AutoSize = false, Location = new Point(12, 62), Size = new Size(326, 16),
                Text = $"Código: {(string.IsNullOrEmpty(clave) ? "— sin clave —" : clave)}   ·   Unidad: {(string.IsNullOrEmpty(unidad) ? "-" : unidad)}",
                Font = new Font("Segoe UI", 8.25F), ForeColor = gris, AutoEllipsis = true
            });

            AgregarMiniDato(content, 12, 84, "PRECIO", precio.ToString("C2", CultureInfo.CurrentCulture), tinta);
            AgregarMiniDato(content, 124, 84, "ALMACÉN", disponible.ToString("N2"), disponible > 0m ? verde : rojo);
            AgregarMiniDato(content, 236, 84, "PENDIENTE", pendiente.ToString("N2"), surtido ? verde : Color.FromArgb(211, 84, 0));

            if (surtido)
            {
                content.Controls.Add(new Label
                {
                    AutoSize = false, Location = new Point(12, 126), Size = new Size(326, 28),
                    Text = "✓ SURTIDO COMPLETO",
                    Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), ForeColor = verde,
                    TextAlign = ContentAlignment.MiddleLeft
                });
            }
            else
            {
                content.Controls.Add(new Label
                {
                    AutoSize = true, Location = new Point(12, 130), Text = "Surtir:",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = tinta
                });

                var nud = new NumericUpDown
                {
                    Location = new Point(64, 127), Width = 86,
                    DecimalPlaces = 2, Minimum = 0m, Maximum = pendiente, Increment = 1m,
                    Value = Math.Max(0m, Math.Min(solicitada, pendiente)),
                    Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(255, 255, 220),
                    TextAlign = HorizontalAlignment.Right
                };
                var lblImporte = new Label
                {
                    AutoSize = false, Location = new Point(158, 130), Size = new Size(180, 22),
                    Text = "= " + (nud.Value * precio).ToString("C2", CultureInfo.CurrentCulture),
                    Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), ForeColor = accent,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                nud.ValueChanged += (s, e) =>
                {
                    row["CantidadSolicitada"] = nud.Value;
                    row["Importe"] = nud.Value * precio;
                    lblImporte.Text = "= " + (nud.Value * precio).ToString("C2", CultureInfo.CurrentCulture);
                };
                content.Controls.Add(nud);
                content.Controls.Add(lblImporte);
            }

            return card;
        }

        private void AgregarMiniDato(Panel parent, int x, int y, string titulo, string valor, Color colorValor)
        {
            parent.Controls.Add(new Label
            {
                AutoSize = false, Location = new Point(x, y), Size = new Size(108, 14),
                Text = titulo, Font = new Font("Segoe UI Semibold", 7F, FontStyle.Bold),
                ForeColor = Color.FromArgb(127, 140, 141)
            });
            parent.Controls.Add(new Label
            {
                AutoSize = false, Location = new Point(x, y + 15), Size = new Size(108, 18),
                Text = valor, Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = colorValor
            });
        }

        /// <summary>Acumulador por clave de los insumos Material de destajos activados.</summary>
        private class InsumoSurtir
        {
            public string Clave;
            public string Descripcion;
            public string Unidad;
            public decimal Precio;
            public decimal Programado;
            public readonly HashSet<string> Destajos =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normaliza un nombre de insumo para cotejarlo entre fuentes (catalogo
        /// TuneraEXP/CalandraEXP, almacen y destajos): recorta, colapsa espacios,
        /// pasa a MAYUSCULAS y quita acentos. Asi "Cemento Gris" == "cemento  gris".
        /// </summary>
        private static string NormNombre(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            string t = s.Trim().ToUpperInvariant();
            var sb = new StringBuilder(t.Length);
            bool espacioPrevio = false;
            foreach (char ch in t.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                    continue; // quita el acento
                if (char.IsWhiteSpace(ch))
                {
                    if (!espacioPrevio && sb.Length > 0) sb.Append(' ');
                    espacioPrevio = true;
                }
                else { sb.Append(ch); espacioPrevio = false; }
            }
            return sb.ToString().Trim().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Carga los insumos PENDIENTES de surtir para esta casa: unicamente los
        /// insumos Material de destajos ya ACTIVADOS cuyo surtido aun no se completa
        /// (programado - ya surtido > 0). El surtido se desacoplo de la activacion:
        /// activar un destajo ya no libera insumos; eso se hace aqui, en Salidas.
        /// </summary>
        private void CargarInsumosDisponiblesSalida()
        {
            if (casaActual == null || casaInventario == null)
            {
                MostrarMensajeSeleccionInsumosSalida();
                return;
            }

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Destajo", typeof(string));
                dt.Columns.Add("Clave", typeof(string));
                dt.Columns.Add("Descripcion", typeof(string));
                dt.Columns.Add("Unidad", typeof(string));
                dt.Columns.Add("PrecioUnitario", typeof(decimal));
                dt.Columns.Add("Importe", typeof(decimal));
                dt.Columns.Add("Disponible", typeof(decimal));
                dt.Columns.Add("MaximoPermitido", typeof(decimal));
                dt.Columns.Add("CantidadSolicitada", typeof(decimal));

                // Ruta del arbol de destajos segun prototipo: MISMA logica que
                // FormActivarTareasTreeList, para coincidir con ActivacionTareasRuta.
                string ruta = casaInventario.Prototipo.ToUpper().Contains("CALANDRA")
                    ? "RutaCalandraDestajo"
                    : "RutaTuneraDestajo";

                var programado = new Dictionary<string, InsumoSurtir>(StringComparer.OrdinalIgnoreCase);

                // Estadistica de resolucion de clave (insumos capturados sin clave).
                int sinClaveTotal = 0, resueltosCatalogo = 0, resueltosAlmacen = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // M/L del destajo se guardan desde el combo (pueden venir con
                    // relleno si la columna es CHAR); se comparan sin espacios.
                    string mz = (casaActual.Manzana ?? "").Trim();
                    string lt = (casaActual.Lote ?? "").Trim();

                    // 1) Insumos Material (TipoTarea=1) de los destajos ACTIVADOS de esta
                    //    casa. Los insumos del destajo pueden NO tener clave de almacen;
                    //    en ese caso se identifican y cotejan por NOMBRE (Descripcion).
                    int filasMaterial = 0;
                    string sqlProg = $@"
                        SELECT
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Clave'    THEN c.Valor END), '') AS Clave,
                            r.Nombre AS Descripcion,
                            MAX(ISNULL(a.NombreTarea, '')) AS Destajo,
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad'   THEN c.Valor END), '') AS Unidad,
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad,
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS Precio
                        FROM {ruta} r
                        INNER JOIN ActivacionTareasRuta a
                            ON a.NodoID = r.ParentId
                           AND LTRIM(RTRIM(a.Manzana)) = @m
                           AND LTRIM(RTRIM(a.Lote)) = @l
                           AND LTRIM(RTRIM(a.Ruta)) = @ruta
                           AND a.DesatajoActivado = 1
                        LEFT JOIN {ruta}_Columnas c ON c.NodoID = r.ID
                        WHERE r.TipoTarea = 1
                        GROUP BY r.ID, r.Nombre";

                    using (SqlCommand cmd = new SqlCommand(sqlProg, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", mz);
                        cmd.Parameters.AddWithValue("@l", lt);
                        cmd.Parameters.AddWithValue("@ruta", ruta);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                filasMaterial++;
                                string clave = (rd["Clave"]?.ToString() ?? "").Trim();
                                string nombre = (rd["Descripcion"]?.ToString() ?? "").Trim();
                                string destajo = (rd["Destajo"]?.ToString() ?? "").Trim();
                                if (clave.Length == 0 && nombre.Length == 0) continue;

                                decimal cant; decimal.TryParse(rd["Cantidad"]?.ToString(), out cant);
                                decimal precio; decimal.TryParse(rd["Precio"]?.ToString(), out precio);

                                // Identidad: por clave si la hay; si no, por nombre normalizado.
                                string key = clave.Length > 0 ? "C:" + clave.ToUpperInvariant()
                                                              : "N:" + NormNombre(nombre);
                                if (!programado.TryGetValue(key, out var ins))
                                {
                                    ins = new InsumoSurtir
                                    {
                                        Clave = clave,
                                        Descripcion = nombre,
                                        Unidad = (rd["Unidad"]?.ToString() ?? "").Trim(),
                                        Precio = precio
                                    };
                                    programado[key] = ins;
                                }
                                ins.Programado += cant;
                                if (ins.Precio == 0m && precio > 0m) ins.Precio = precio;
                                if (destajo.Length > 0) ins.Destajos.Add(destajo);
                            }
                        }
                    }

                    if (programado.Count == 0)
                    {
                        // Diagnostico: M/L/ruta sin destajos activados, o activados sin Material.
                        int activados = 0;
                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT COUNT(*) FROM ActivacionTareasRuta
                              WHERE LTRIM(RTRIM(Manzana)) = @m AND LTRIM(RTRIM(Lote)) = @l
                                AND LTRIM(RTRIM(Ruta)) = @ruta AND DesatajoActivado = 1", conn))
                        {
                            cmd.Parameters.AddWithValue("@m", mz);
                            cmd.Parameters.AddWithValue("@l", lt);
                            cmd.Parameters.AddWithValue("@ruta", ruta);
                            activados = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        string msg = activados == 0
                            ? $"No se encontraron destajos ACTIVADOS para M{mz}-L{lt} en la ruta \"{ruta}\".\n\n" +
                              "Verifica que activaste los destajos de ESTA casa (mismo prototipo) en \"Activación de Destajos\"."
                            : $"Hay {activados} destajo(s) activado(s), pero sus tareas no incluyen insumos de tipo Material.";

                        MessageBox.Show("ℹ️ " + msg, "Sin insumos por surtir",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        MostrarMensajeSeleccionInsumosSalida();
                        return;
                    }

                    // 2) Inventario de almacen (disponible global) por CLAVE y por NOMBRE,
                    //    para cotejar tambien los insumos del destajo SIN clave.
                    var dispClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    var dispNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    var nombreAClave = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    CargarInventarioAlmacen(conn, dispClave, dispNombre, nombreAClave);

                    // 2b) Catalogo maestro (InsumosTuneraEXP / InsumosCalandraEXP): mapa
                    //     NOMBRE normalizado -> Clave. Es la fuente preferente para
                    //     resolver la clave de un insumo de destajo capturado sin clave.
                    var catalogoNombreAClave = CargarCatalogoClavesPorNombre(conn, ruta);

                    // 3) Ya surtido a ESTA casa, por clave y por nombre.
                    var surtClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    var surtNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT ISNULL(Clave,'') AS Clave, ISNULL(Descripcion,'') AS Descripcion, SUM(Cantidad) AS Surtido
                        FROM SalidasAlmacen
                        WHERE LTRIM(RTRIM(Manzana)) = @m AND LTRIM(RTRIM(Lote)) = @l
                        GROUP BY ISNULL(Clave,''), ISNULL(Descripcion,'')", conn))
                    {
                        cmd.Parameters.AddWithValue("@m", mz);
                        cmd.Parameters.AddWithValue("@l", lt);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                string clv = (rd["Clave"]?.ToString() ?? "").Trim().ToUpperInvariant();
                                string nom = NormNombre(rd["Descripcion"]?.ToString());
                                decimal s = rd["Surtido"] != DBNull.Value ? Convert.ToDecimal(rd["Surtido"]) : 0m;
                                if (clv.Length > 0) { surtClave.TryGetValue(clv, out var a); surtClave[clv] = a + s; }
                                if (nom.Length > 0) { surtNombre.TryGetValue(nom, out var b); surtNombre[nom] = b + s; }
                            }
                        }
                    }

                    // 4) Construir filas: pendiente = programado - surtido (verde si ya
                    //    surtido, rojo si pendiente). Para insumos sin clave se resuelve la
                    //    clave de almacen por nombre, para descontar bien el stock al surtir.
                    foreach (var ins in programado.Values)
                    {
                        string nombreKey = NormNombre(ins.Descripcion);

                        // Resolver la clave de almacen: 1) la del insumo; 2) por nombre en
                        // el catalogo TuneraEXP/CalandraEXP; 3) por nombre en EntradasAlmacen.
                        string claveAlmacen = ins.Clave ?? "";
                        if (string.IsNullOrEmpty(claveAlmacen))
                        {
                            sinClaveTotal++;
                            if (catalogoNombreAClave.TryGetValue(nombreKey, out var ckCat) && ckCat.Length > 0)
                            { claveAlmacen = ckCat; resueltosCatalogo++; }
                            else if (nombreAClave.TryGetValue(nombreKey, out var ckAlm) && ckAlm.Length > 0)
                            { claveAlmacen = ckAlm; resueltosAlmacen++; }
                        }

                        bool hayClave = !string.IsNullOrEmpty(claveAlmacen);
                        string claveKey = claveAlmacen.ToUpperInvariant();

                        decimal yaSurtido = hayClave
                            ? (surtClave.TryGetValue(claveKey, out var sc) ? sc
                               : (surtNombre.TryGetValue(nombreKey, out var sn0) ? sn0 : 0m))
                            : (surtNombre.TryGetValue(nombreKey, out var sn) ? sn : 0m);

                        decimal disponible = hayClave
                            ? (dispClave.TryGetValue(claveKey, out var dc) ? dc : 0m)
                            : (dispNombre.TryGetValue(nombreKey, out var dn) ? dn : 0m);

                        decimal pendiente = ins.Programado - yaSurtido;

                        DataRow row = dt.NewRow();
                        row["Destajo"] = ins.Destajos.Count > 0 ? string.Join(", ", ins.Destajos) : "—";
                        row["Clave"] = claveAlmacen;
                        row["Descripcion"] = string.IsNullOrEmpty(ins.Descripcion) ? "Sin descripcion" : ins.Descripcion;
                        row["Unidad"] = string.IsNullOrEmpty(ins.Unidad) ? "PZA" : ins.Unidad;
                        row["PrecioUnitario"] = ins.Precio;
                        row["Importe"] = 0m;
                        row["Disponible"] = disponible;
                        row["MaximoPermitido"] = pendiente > 0m ? pendiente : 0m; // tope a surtir
                        row["CantidadSolicitada"] = 0m;
                        dt.Rows.Add(row);
                    }
                }

                // Ordenar: primero los pendientes (rojo) y luego los surtidos (verde).
                dt.DefaultView.Sort = "MaximoPermitido DESC, Descripcion ASC";
                dt = dt.DefaultView.ToTable();

                _dtSalida = dt;
                dgvInsumos.DataSource = dt; // modelo oculto para RegistrarSalida y vale PDF

                int pendientes = dt.AsEnumerable().Count(r => Convert.ToDecimal(r["MaximoPermitido"]) > 0m);
                string resumen = $"M{casaActual.Manzana}-L{casaActual.Lote}:  {pendientes} pendientes (rojo)  ·  {dt.Rows.Count - pendientes} surtidos (verde)";
                if (sinClaveTotal > 0)
                {
                    int sinResolver = sinClaveTotal - resueltosCatalogo - resueltosAlmacen;
                    resumen += $"   ·   {sinClaveTotal} sin clave: {resueltosCatalogo} por catálogo, {resueltosAlmacen} por almacén, {sinResolver} sin coincidencia";
                }
                _resumenSalida = resumen;
                RenderTarjetasSalida(txtBuscarSalida?.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al cargar insumos por surtir:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Carga el disponible del almacen (Entradas - Salidas) por CLAVE y por NOMBRE,
        /// y un mapa NOMBRE -> clave representativa (la de mayor existencia). Permite
        /// surtir insumos de destajo que no traen clave, cotejandolos por nombre.
        /// </summary>
        private void CargarInventarioAlmacen(
            SqlConnection conn,
            Dictionary<string, decimal> dispClave,
            Dictionary<string, decimal> dispNombre,
            Dictionary<string, string> nombreAClave)
        {
            var entClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var entNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var mejorEntradaPorNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(Clave,'') AS Clave, ISNULL(Descripcion,'') AS Descripcion, SUM(Cantidad) AS Entrada
                FROM EntradasAlmacen
                GROUP BY ISNULL(Clave,''), ISNULL(Descripcion,'')", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    string clv = (rd["Clave"]?.ToString() ?? "").Trim();
                    string nom = (rd["Descripcion"]?.ToString() ?? "").Trim();
                    decimal ent = rd["Entrada"] != DBNull.Value ? Convert.ToDecimal(rd["Entrada"]) : 0m;
                    string clvK = clv.ToUpperInvariant();
                    string nomK = NormNombre(nom);

                    if (clvK.Length > 0) { entClave.TryGetValue(clvK, out var a); entClave[clvK] = a + ent; }
                    if (nomK.Length > 0) { entNombre.TryGetValue(nomK, out var b); entNombre[nomK] = b + ent; }

                    // Mejor clave para un nombre = la de mayor entrada (con clave no vacia).
                    if (nomK.Length > 0 && clv.Length > 0 &&
                        (!mejorEntradaPorNombre.TryGetValue(nomK, out var best) || ent > best))
                    {
                        mejorEntradaPorNombre[nomK] = ent;
                        nombreAClave[nomK] = clv;
                    }
                }
            }

            var salClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var salNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(Clave,'') AS Clave, ISNULL(Descripcion,'') AS Descripcion, SUM(Cantidad) AS Salida
                FROM SalidasAlmacen
                GROUP BY ISNULL(Clave,''), ISNULL(Descripcion,'')", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    string clv = (rd["Clave"]?.ToString() ?? "").Trim().ToUpperInvariant();
                    string nom = NormNombre(rd["Descripcion"]?.ToString());
                    decimal sal = rd["Salida"] != DBNull.Value ? Convert.ToDecimal(rd["Salida"]) : 0m;
                    if (clv.Length > 0) { salClave.TryGetValue(clv, out var a); salClave[clv] = a + sal; }
                    if (nom.Length > 0) { salNombre.TryGetValue(nom, out var b); salNombre[nom] = b + sal; }
                }
            }

            foreach (var kv in entClave)
                dispClave[kv.Key] = kv.Value - (salClave.TryGetValue(kv.Key, out var s) ? s : 0m);
            foreach (var kv in entNombre)
                dispNombre[kv.Key] = kv.Value - (salNombre.TryGetValue(kv.Key, out var s) ? s : 0m);
        }

        /// <summary>
        /// Mapa NOMBRE normalizado -> Clave a partir del catalogo maestro de insumos
        /// (InsumosTuneraEXP / InsumosCalandraEXP segun la ruta). Es la fuente preferente
        /// para resolver la clave de un insumo de destajo capturado sin clave.
        /// </summary>
        private Dictionary<string, string> CargarCatalogoClavesPorNombre(SqlConnection conn, string ruta)
        {
            var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string tabla = (ruta ?? "").ToUpperInvariant().Contains("CALANDRA")
                ? "InsumosCalandraEXP" : "InsumosTuneraEXP";
            try
            {
                using (var cmd = new SqlCommand($"SELECT Clave, [Descripción] AS Descripcion FROM dbo.{tabla}", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string clave = (rd["Clave"]?.ToString() ?? "").Trim();
                        string nom = NormNombre(rd["Descripcion"]?.ToString());
                        if (nom.Length == 0 || clave.Length == 0) continue;
                        if (!mapa.ContainsKey(nom)) mapa[nom] = clave; // primera coincidencia
                    }
                }
            }
            catch
            {
                // Sin catalogo: se cae a EntradasAlmacen por nombre.
            }
            return mapa;
        }

        /// <summary>
        /// Colorea cada fila de la grilla de salidas: VERDE si el insumo ya fue surtido
        /// por completo (pendiente == 0) y ROJO si aun esta pendiente. En las filas ya
        /// surtidas bloquea la captura; en las pendientes conserva el resaltado editable.
        /// </summary>
        private void ColorearFilasPorSurtido()
        {
            if (dgvInsumos == null || !dgvInsumos.Columns.Contains("MaximoPermitido")) return;

            Color verdeFondo = Color.FromArgb(232, 245, 233);
            Color verdeTexto = Color.FromArgb(27, 94, 32);
            Color rojoFondo = Color.FromArgb(253, 236, 234);
            Color rojoTexto = Color.FromArgb(183, 28, 28);
            Color amarilloEdit = Color.FromArgb(255, 255, 220);

            foreach (DataGridViewRow row in dgvInsumos.Rows)
            {
                if (row.IsNewRow) continue;

                var val = row.Cells["MaximoPermitido"].Value;
                decimal pendiente = (val != null && val != DBNull.Value) ? Convert.ToDecimal(val) : 0m;
                bool surtido = pendiente <= 0m;

                row.DefaultCellStyle.BackColor = surtido ? verdeFondo : rojoFondo;
                row.DefaultCellStyle.ForeColor = surtido ? verdeTexto : rojoTexto;

                if (dgvInsumos.Columns.Contains("CantidadSolicitada"))
                {
                    var celda = row.Cells["CantidadSolicitada"];
                    celda.ReadOnly = surtido;                 // ya surtido: no se puede pedir mas
                    celda.Style.BackColor = surtido ? verdeFondo : amarilloEdit;
                }
            }
        }

        /// <summary>
        /// Calcula el importe cuando cambia la cantidad solicitada
        /// </summary>
        private void DgvInsumos_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            
            if (dgvInsumos.Columns[e.ColumnIndex].Name == "CantidadSolicitada")
            {
                var row = dgvInsumos.Rows[e.RowIndex];
                
                if (row.Cells["CantidadSolicitada"].Value != null && 
                    row.Cells["PrecioUnitario"].Value != null)
                {
                    decimal cantidad = 0;
                    decimal precio = 0;
                    
                    if (decimal.TryParse(row.Cells["CantidadSolicitada"].Value.ToString(), out cantidad) &&
                        decimal.TryParse(row.Cells["PrecioUnitario"].Value.ToString(), out precio))
                    {
                        row.Cells["Importe"].Value = cantidad * precio;
                    }
                }
            }
        }

        /// <summary>
        /// Configura las columnas del DataGridView de salidas
        /// </summary>
        private void ConfigurarColumnasSalida()
        {
            if (dgvInsumos.Columns.Count == 0) return;

            dgvInsumos.AllowUserToAddRows = false;
            dgvInsumos.ReadOnly = false;
            dgvInsumos.RowTemplate.Height = 30;

            // Establecer el orden de las columnas
            int displayIndex = 0;

            // 0. Destajo (de que destajo proviene el insumo)
            if (dgvInsumos.Columns.Contains("Destajo"))
            {
                dgvInsumos.Columns["Destajo"].HeaderText = "\U0001F3D7 Destajo";
                dgvInsumos.Columns["Destajo"].Width = 180;
                dgvInsumos.Columns["Destajo"].ReadOnly = true;
                dgvInsumos.Columns["Destajo"].DefaultCellStyle.Font = new Font(dgvInsumos.Font, FontStyle.Bold);
                dgvInsumos.Columns["Destajo"].DisplayIndex = displayIndex++;
            }

            // 1. Clave (C�digo)
            if (dgvInsumos.Columns.Contains("Clave"))
            {
                dgvInsumos.Columns["Clave"].HeaderText = "\U0001F511 C�digo";
                dgvInsumos.Columns["Clave"].Width = 100;
                dgvInsumos.Columns["Clave"].ReadOnly = true;
                dgvInsumos.Columns["Clave"].DisplayIndex = displayIndex++;
            }

            // 2. Descripcion (Insumo)
            if (dgvInsumos.Columns.Contains("Descripcion"))
            {
                dgvInsumos.Columns["Descripcion"].HeaderText = "\U0001F4E6 Insumo";
                dgvInsumos.Columns["Descripcion"].Width = 280;
                dgvInsumos.Columns["Descripcion"].ReadOnly = true;
                dgvInsumos.Columns["Descripcion"].DisplayIndex = displayIndex++;
            }

            // 3. Unidad
            if (dgvInsumos.Columns.Contains("Unidad"))
            {
                dgvInsumos.Columns["Unidad"].HeaderText = "\U0001F4CF Unidad";
                dgvInsumos.Columns["Unidad"].Width = 70;
                dgvInsumos.Columns["Unidad"].ReadOnly = true;
                dgvInsumos.Columns["Unidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvInsumos.Columns["Unidad"].DisplayIndex = displayIndex++;
            }

            // 4. Precio Unitario
            if (dgvInsumos.Columns.Contains("PrecioUnitario"))
            {
                dgvInsumos.Columns["PrecioUnitario"].HeaderText = "\U0001F4B5 Precio Unitario";
                dgvInsumos.Columns["PrecioUnitario"].Width = 110;
                dgvInsumos.Columns["PrecioUnitario"].ReadOnly = true;
                dgvInsumos.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
                dgvInsumos.Columns["PrecioUnitario"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["PrecioUnitario"].DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
                dgvInsumos.Columns["PrecioUnitario"].DisplayIndex = displayIndex++;
            }

            // 5. Importe
            if (dgvInsumos.Columns.Contains("Importe"))
            {
                dgvInsumos.Columns["Importe"].HeaderText = "\U0001F4B0 Importe";
                dgvInsumos.Columns["Importe"].Width = 100;
                dgvInsumos.Columns["Importe"].ReadOnly = true;
                dgvInsumos.Columns["Importe"].DefaultCellStyle.Format = "C2";
                dgvInsumos.Columns["Importe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["Importe"].DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
                dgvInsumos.Columns["Importe"].DefaultCellStyle.Font = new Font(dgvInsumos.Font, FontStyle.Bold);
                dgvInsumos.Columns["Importe"].DisplayIndex = displayIndex++;
            }

            // 6. Disponible (Almac�n)
            if (dgvInsumos.Columns.Contains("Disponible"))
            {
                dgvInsumos.Columns["Disponible"].HeaderText = "\U0001F4CA Almac�n";
                dgvInsumos.Columns["Disponible"].Width = 90;
                dgvInsumos.Columns["Disponible"].ReadOnly = true;
                dgvInsumos.Columns["Disponible"].DefaultCellStyle.Format = "N2";
                dgvInsumos.Columns["Disponible"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["Disponible"].DisplayIndex = displayIndex++;
            }

            // 7. M�ximo Permitido
            if (dgvInsumos.Columns.Contains("MaximoPermitido"))
            {
                dgvInsumos.Columns["MaximoPermitido"].HeaderText = "Pendiente por surtir";
                dgvInsumos.Columns["MaximoPermitido"].Width = 120;
                dgvInsumos.Columns["MaximoPermitido"].ReadOnly = true;
                dgvInsumos.Columns["MaximoPermitido"].DefaultCellStyle.Format = "N2";
                dgvInsumos.Columns["MaximoPermitido"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["MaximoPermitido"].DefaultCellStyle.ForeColor = Color.OrangeRed;
                dgvInsumos.Columns["MaximoPermitido"].DefaultCellStyle.Font = new Font(dgvInsumos.Font, FontStyle.Bold);
                dgvInsumos.Columns["MaximoPermitido"].DisplayIndex = displayIndex++;
            }

            // 8. Cantidad a Solicitar
            if (dgvInsumos.Columns.Contains("CantidadSolicitada"))
            {
                dgvInsumos.Columns["CantidadSolicitada"].HeaderText = "\u270F\uFE0F Cantidad a Solicitar";
                dgvInsumos.Columns["CantidadSolicitada"].Width = 140;
                dgvInsumos.Columns["CantidadSolicitada"].ReadOnly = false;
                dgvInsumos.Columns["CantidadSolicitada"].DefaultCellStyle.Format = "N2";
                dgvInsumos.Columns["CantidadSolicitada"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["CantidadSolicitada"].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 220);
                dgvInsumos.Columns["CantidadSolicitada"].DisplayIndex = displayIndex++;
            }
        }

        private void RegistrarSalida()
        {
            if (casaActual == null || casaInventario == null)
            {
                MessageBox.Show("\u26A0\uFE0F Primero selecciona una casa destino.", 
                    "Casa requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable dt = dgvInsumos.DataSource as DataTable;
            if (dt == null || dt.Columns.Count == 1) // Si solo tiene la columna de mensaje
            {
                MessageBox.Show("\u26A0\uFE0F No hay insumos cargados para registrar salida.", 
                    "Sin insumos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var usuario = Global.UsuarioActual?.Nombre ?? Environment.UserName;
            bool seRegistroAlgo = false;
            int insumosValidados = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                
                foreach (DataRow row in dt.Rows)
                {
                    if (!dt.Columns.Contains("CantidadSolicitada") || row.IsNull("CantidadSolicitada"))
                        continue;

                    decimal solicitada = Convert.ToDecimal(row["CantidadSolicitada"]);
                    
                    // Ignorar si no se solicit� nada
                    if (solicitada <= 0)
                        continue;

                    string clave = row["Clave"].ToString();
                    string descripcion = row["Descripcion"].ToString();
                    string unidad = row["Unidad"].ToString();
                    decimal disponible = Convert.ToDecimal(row["Disponible"]);
                    decimal maximoPermitido = Convert.ToDecimal(row["MaximoPermitido"]);
                    decimal precioUnitario = Convert.ToDecimal(row["PrecioUnitario"]);
                    decimal importe = solicitada * precioUnitario;

                    insumosValidados++;

                    // Validar que no exceda el m�ximo permitido (si est� definido)
                    if (maximoPermitido < 999999 && solicitada > maximoPermitido)
                    {
                        MessageBox.Show(
                            $"\u26A0\uFE0F La cantidad solicitada para {clave} excede el m�ximo permitido.\n\n" +
                            $"Solicitado: {solicitada:N2}\n" +
                            $"M�ximo permitido: {maximoPermitido:N2}", 
                            "Cantidad excedida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    // Validar que haya suficiente disponible
                    if (solicitada > disponible)
                    {
                        var result = MessageBox.Show(
                            $"\u26A0\uFE0F La cantidad solicitada para {clave} excede el inventario disponible.\n\n" +
                            $"Solicitado: {solicitada:N2}\n" +
                            $"Disponible: {disponible:N2}\n\n" +
                            $"�Desea registrar la salida de todos modos?", 
                            "Inventario insuficiente", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        
                        if (result != DialogResult.Yes)
                            continue;
                    }

                    // Insertar en SalidasAlmacen CON PRECIO E IMPORTE
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO SalidasAlmacen 
                        (Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, FechaSalida, Manzana, Lote, Prototipo, Justificacion) 
                        VALUES (@Clave, @Descripcion, @Unidad, @Cantidad, @PrecioUnitario, @Importe, GETDATE(), @Manzana, @Lote, @Prototipo, NULL)", conn))
                    {
                        cmd.Parameters.AddWithValue("@Clave", clave);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                        cmd.Parameters.AddWithValue("@Unidad", unidad);
                        cmd.Parameters.AddWithValue("@Cantidad", solicitada);
                        cmd.Parameters.AddWithValue("@PrecioUnitario", precioUnitario);
                        cmd.Parameters.AddWithValue("@Importe", importe);
                        cmd.Parameters.AddWithValue("@Manzana", casaActual.Manzana);
                        cmd.Parameters.AddWithValue("@Lote", casaActual.Lote);
                        cmd.Parameters.AddWithValue("@Prototipo", casaInventario.Prototipo);
                        cmd.ExecuteNonQuery();
                    }

                    // Insertar en HistorialMovimientos CON PRECIO E IMPORTE
                    using (SqlCommand hist = new SqlCommand(@"
                        INSERT INTO HistorialMovimientos 
                        (Fecha, TipoMovimiento, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, Usuario, Manzana, Lote, Prototipo, Justificacion) 
                        VALUES (GETDATE(), 'Salida', @Clave, @Descripcion, @Unidad, @Cantidad, @PrecioUnitario, @Importe, @Usuario, @Manzana, @Lote, @Prototipo, NULL)", conn))
                    {
                        hist.Parameters.AddWithValue("@Clave", clave);
                        hist.Parameters.AddWithValue("@Descripcion", descripcion);
                        hist.Parameters.AddWithValue("@Unidad", unidad);
                        hist.Parameters.AddWithValue("@Cantidad", solicitada);
                        hist.Parameters.AddWithValue("@PrecioUnitario", precioUnitario);
                        hist.Parameters.AddWithValue("@Importe", importe);
                        hist.Parameters.AddWithValue("@Usuario", usuario);
                        hist.Parameters.AddWithValue("@Manzana", casaActual.Manzana);
                        hist.Parameters.AddWithValue("@Lote", casaActual.Lote);
                        hist.Parameters.AddWithValue("@Prototipo", casaInventario.Prototipo);
                        hist.ExecuteNonQuery();
                    }

                    seRegistroAlgo = true;
                }
            }

            if (seRegistroAlgo)
            {
                // ?? GENERAR PDF DEL VALE DE SALIDA
                try
                {
                    GenerarValeSalidaPDF();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"?? La salida se registr� correctamente pero hubo un error al generar el vale PDF:\n\n{ex.Message}",
                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                CargarHistorial();
                
                // Limpiar el formulario
                txtTrimManzanaSalida.Clear();
                txtTrimLoteSalida.Clear();
                casaActual = null;
                casaInventario = null;
                explosion = null;
                lblCasaSalida.Text = "";
                lblInstrucciones.Text = "\U0001F3E0 SELECCIONA CASA DESTINO";
                lblInstrucciones.ForeColor = ThemeManager.ColorPrincipalMenuBar;
                
                MostrarMensajeSeleccionInsumosSalida();
            }
            else
            {
                if (insumosValidados == 0)
                {
                    MessageBox.Show("\u2139\uFE0F No se captur� ninguna cantidad para registrar salida.", 
                        "Sin cantidades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("\u26A0\uFE0F No se registr� ninguna salida debido a validaciones.", 
                        "Sin salidas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void BtnSeleccionarCasaSalida_Click(object sender, EventArgs e)
        {
            string manzana = txtTrimManzanaSalida.Text.Trim();
            string lote = txtTrimLoteSalida.Text.Trim();

            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
            {
                MessageBox.Show("\u26A0\uFE0F Debes ingresar manzana y lote.", 
                    "Datos requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Primero mostrar el mensaje de selecci�n
            MostrarMensajeSeleccionInsumosSalida();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @Manzana AND Lote = @Lote", conn))
                    {
                        cmd.Parameters.AddWithValue("@Manzana", manzana);
                        cmd.Parameters.AddWithValue("@Lote", lote);

                        var prototipo = cmd.ExecuteScalar()?.ToString();
                        if (string.IsNullOrEmpty(prototipo))
                        {
                            MessageBox.Show(
                                $"\u274C No se encontr� la casa M{manzana}-L{lote} en el inventario.", 
                                "Casa no encontrada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Configurar casaActual
                        casaActual = new PanelPrincipal.Casa { Manzana = manzana, Lote = lote };
                        casaInventario = new DynamicSepticSystem.CasaInventario { Prototipo = prototipo };
                        
                        // Actualizar labels
                        lblCasaSalida.Text = $"\U0001F3E0 M{casaActual.Manzana}-L{casaActual.Lote} ({casaInventario.Prototipo})";
                        lblCasaSalida.ForeColor = ThemeManager.ColorExito;
                        lblCasaSalida.Font = new Font(lblCasaSalida.Font, FontStyle.Bold);
                        
                        // Cargar explosi�n de insumos (opcional, para los m�ximos permitidos)
                        CargarExplosionParaSalida(prototipo);
                        
                        // Cargar el inventario actual disponible
                        CargarInsumosDisponiblesSalida();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"\u274C Error al seleccionar casa:\n\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Abre el repositorio de vales de salida
        /// </summary>
        private void BtnVerRepositorioVales_Click(object sender, EventArgs e)
        {
            try
            {
                // Si hay una casa seleccionada, abrir el repositorio filtrado por esa casa
                if (casaActual != null && !string.IsNullOrWhiteSpace(casaActual.Manzana) && !string.IsNullOrWhiteSpace(casaActual.Lote))
                {
                    AbrirRepositorioValesSalida(casaActual.Manzana, casaActual.Lote);
                }
                else
                {
                    // Abrir el repositorio mostrando todos los vales
                    AbrirRepositorioValesSalida();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir repositorio de vales: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvInsumos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Evento para futuras funcionalidades
        }
    }
}
