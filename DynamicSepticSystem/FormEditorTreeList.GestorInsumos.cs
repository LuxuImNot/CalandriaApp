using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Gestor central de insumos para el editor de TreeList.
    ///
    /// Agrupa todos los nodos hoja (insumos, nivel 2) por nombre y muestra una fila
    /// por insumo distinto con sus columnas editables (precio, unidad, etc.). Editar
    /// el valor en una sola fila lo PROPAGA a todas las apariciones de ese insumo a
    /// lo largo del árbol, de modo que actualizar un precio se refleja en todos lados.
    ///
    /// Solo se propagan las celdas que el usuario realmente modifica: las columnas que
    /// varían por uso (p.ej. Cantidad) se muestran en blanco con marca "valores
    /// distintos" y, si no se tocan, no se sobrescriben.
    /// </summary>
    public class DialogGestorInsumos : Form
    {
        #region Modelo interno

        /// <summary>
        /// Un insumo distinto (clave = nombre) con todas las apariciones en el árbol.
        /// </summary>
        private class GrupoInsumo
        {
            public string Nombre;
            public string TipoTexto;
            public List<NodoTree> Nodos = new List<NodoTree>();

            // Valor común mostrado por columna (string ya normalizado para display).
            public Dictionary<string, string> DisplayOriginal = new Dictionary<string, string>();
            // Valor actual en edición por columna (arranca igual a DisplayOriginal).
            public Dictionary<string, string> Valores = new Dictionary<string, string>();
            // True si las apariciones tenían valores distintos para esa columna.
            public Dictionary<string, bool> EsMixto = new Dictionary<string, bool>();
        }

        #endregion

        #region Campos

        private readonly List<ColumnaTreeList> columnasEditables;
        private readonly List<GrupoInsumo> grupos;

        private Label lblTitulo;
        private Label lblInfo;
        private TextBox txtBuscar;
        private Label lblBuscar;
        private DataGridView grid;
        private Button btnAceptar;
        private Button btnCancelar;
        private Panel pnlBuscar;
        private Panel pnlBotones;

        /// <summary>
        /// True si al cerrar se propagó algún cambio a los nodos del árbol.
        /// </summary>
        public bool HuboCambios { get; private set; }

        #endregion

        #region Constructor

        public DialogGestorInsumos(List<NodoTree> nodosRaiz, List<ColumnaTreeList> columnas)
        {
            // Solo columnas que el usuario puede editar (las calculadas se derivan).
            columnasEditables = columnas
                .Where(c => c.EsEditable && !c.EsCalculada)
                .ToList();

            grupos = ConstruirGrupos(nodosRaiz);

            InitializeComponent();
            ThemeManager.AplicarTema(this);
            ConfigurarGrid();
            PoblarGrid(null);
        }

        #endregion

        #region Construcción de grupos

        private List<GrupoInsumo> ConstruirGrupos(List<NodoTree> nodosRaiz)
        {
            // Aplanar y quedarnos con los nodos hoja (insumos = nivel 2).
            var insumos = Aplanar(nodosRaiz).Where(n => n.Nivel == 2).ToList();

            var lista = insumos
                .GroupBy(n => (n.Nombre ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var grupo = new GrupoInsumo
                    {
                        Nombre = g.First().Nombre,
                        Nodos = g.ToList()
                    };

                    // Tipo de tarea: si todas coinciden, lo mostramos; si no, "—".
                    var tipos = grupo.Nodos.Select(n => n.TipoTarea).Distinct().ToList();
                    grupo.TipoTexto = tipos.Count == 1 ? grupo.Nodos[0].TipoTareaTexto : "—";

                    // Valor común por columna editable.
                    foreach (var col in columnasEditables)
                    {
                        var valores = grupo.Nodos
                            .Select(n => NormalizarParaDisplay(n.ObtenerValorColumna(col.Nombre)))
                            .Distinct()
                            .ToList();

                        bool mixto = valores.Count > 1;
                        string comun = mixto ? "" : (valores.Count == 1 ? valores[0] : "");

                        grupo.EsMixto[col.Nombre] = mixto;
                        grupo.DisplayOriginal[col.Nombre] = comun;
                        grupo.Valores[col.Nombre] = comun;
                    }

                    return grupo;
                })
                .ToList();

            return lista;
        }

        private static List<NodoTree> Aplanar(List<NodoTree> raices)
        {
            var lista = new List<NodoTree>();
            void Recurse(List<NodoTree> nodos)
            {
                if (nodos == null) return;
                foreach (var n in nodos)
                {
                    lista.Add(n);
                    if (n.TieneHijos) Recurse(n.Hijos);
                }
            }
            Recurse(raices);
            return lista;
        }

        private static string NormalizarParaDisplay(object valor)
        {
            return valor == null ? "" : valor.ToString().Trim();
        }

        #endregion

        #region UI

        private void InitializeComponent()
        {
            this.lblTitulo = new Label();
            this.lblInfo = new Label();
            this.pnlBuscar = new Panel();
            this.lblBuscar = new Label();
            this.txtBuscar = new TextBox();
            this.grid = new DataGridView();
            this.pnlBotones = new Panel();
            this.btnAceptar = new Button();
            this.btnCancelar = new Button();

            this.SuspendLayout();

            // lblTitulo
            this.lblTitulo.BackColor = Color.FromArgb(88, 53, 23);
            this.lblTitulo.Dock = DockStyle.Top;
            this.lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblTitulo.ForeColor = Color.White;
            this.lblTitulo.Height = 48;
            this.lblTitulo.Text = "Gestor Central de Insumos";
            this.lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

            // lblInfo
            this.lblInfo.Dock = DockStyle.Top;
            this.lblInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            this.lblInfo.ForeColor = Color.FromArgb(127, 140, 141);
            this.lblInfo.Height = 38;
            this.lblInfo.Padding = new Padding(10, 4, 10, 0);
            this.lblInfo.Text = "Edita precio/unidad de un insumo una sola vez: el cambio se aplica a todas sus " +
                                "apariciones en el árbol.\nLas celdas en amarillo tienen valores distintos entre " +
                                "usos; si las dejas en blanco no se modifican.";

            // pnlBuscar
            this.pnlBuscar.Dock = DockStyle.Top;
            this.pnlBuscar.Height = 36;
            this.pnlBuscar.Padding = new Padding(10, 6, 10, 6);

            // lblBuscar
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Font = new Font("Segoe UI", 9F);
            this.lblBuscar.Location = new Point(12, 9);
            this.lblBuscar.Text = "Buscar insumo:";

            // txtBuscar
            this.txtBuscar.Font = new Font("Segoe UI", 9F);
            this.txtBuscar.Location = new Point(108, 6);
            this.txtBuscar.Size = new Size(260, 23);
            this.txtBuscar.TextChanged += (s, e) => PoblarGrid(txtBuscar.Text);
            this.pnlBuscar.Controls.Add(this.txtBuscar);
            this.pnlBuscar.Controls.Add(this.lblBuscar);

            // grid
            this.grid.Dock = DockStyle.Fill;
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToResizeRows = false;
            this.grid.RowHeadersVisible = false;
            this.grid.BackgroundColor = Color.White;
            this.grid.BorderStyle = BorderStyle.None;
            this.grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            this.grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            this.grid.Font = new Font("Segoe UI", 9F);
            this.grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.grid.CellEndEdit += grid_CellEndEdit;

            // pnlBotones
            this.pnlBotones.Dock = DockStyle.Bottom;
            this.pnlBotones.Height = 56;
            this.pnlBotones.Padding = new Padding(10);

            // btnAceptar
            this.btnAceptar.Text = "Aplicar cambios";
            this.btnAceptar.Size = new Size(150, 36);
            this.btnAceptar.BackColor = Color.FromArgb(46, 204, 113);
            this.btnAceptar.ForeColor = Color.White;
            this.btnAceptar.FlatStyle = FlatStyle.Flat;
            this.btnAceptar.FlatAppearance.BorderSize = 0;
            this.btnAceptar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnAceptar.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnAceptar.Click += btnAceptar_Click;

            // btnCancelar
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Size = new Size(100, 36);
            this.btnCancelar.BackColor = Color.FromArgb(189, 195, 199);
            this.btnCancelar.ForeColor = Color.White;
            this.btnCancelar.FlatStyle = FlatStyle.Flat;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.Font = new Font("Segoe UI", 9F);
            this.btnCancelar.DialogResult = DialogResult.Cancel;
            this.btnCancelar.Anchor = AnchorStyles.Right | AnchorStyles.Top;

            this.pnlBotones.Controls.Add(this.btnAceptar);
            this.pnlBotones.Controls.Add(this.btnCancelar);
            this.pnlBotones.Resize += (s, e) => ReubicarBotones();

            // Form
            this.Text = "Gestor de Insumos";
            this.ClientSize = new Size(820, 540);
            this.MinimumSize = new Size(640, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9F);
            this.CancelButton = this.btnCancelar;

            // Orden de apilado (Fill primero, luego los Dock.Top en orden inverso).
            this.Controls.Add(this.grid);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.pnlBotones);

            this.ResumeLayout(false);
            this.ReubicarBotones();
        }

        private void ReubicarBotones()
        {
            btnAceptar.Location = new Point(pnlBotones.ClientSize.Width - btnAceptar.Width - 10, 10);
            btnCancelar.Location = new Point(btnAceptar.Left - btnCancelar.Width - 8, 10);
        }

        private void ConfigurarGrid()
        {
            grid.Columns.Clear();

            var colNombre = new DataGridViewTextBoxColumn
            {
                HeaderText = "Insumo",
                Name = "Insumo",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 240
            };
            colNombre.DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            grid.Columns.Add(colNombre);

            var colTipo = new DataGridViewTextBoxColumn
            {
                HeaderText = "Tipo",
                Name = "Tipo",
                ReadOnly = true,
                Width = 90
            };
            colTipo.DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            grid.Columns.Add(colTipo);

            var colUsos = new DataGridViewTextBoxColumn
            {
                HeaderText = "# Usos",
                Name = "Usos",
                ReadOnly = true,
                Width = 60
            };
            colUsos.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colUsos.DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            grid.Columns.Add(colUsos);

            foreach (var col in columnasEditables)
            {
                var dgc = new DataGridViewTextBoxColumn
                {
                    HeaderText = col.Titulo,
                    Tag = col,
                    Width = Math.Max(70, col.Ancho)
                };
                if (EsNumerica(col))
                    dgc.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                grid.Columns.Add(dgc);
            }
        }

        private void PoblarGrid(string filtro)
        {
            filtro = (filtro ?? "").Trim();

            grid.Rows.Clear();

            foreach (var grupo in grupos)
            {
                if (filtro.Length > 0 &&
                    (grupo.Nombre ?? "").IndexOf(filtro, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                int idx = grid.Rows.Add();
                var fila = grid.Rows[idx];
                fila.Tag = grupo;

                fila.Cells["Insumo"].Value = grupo.Nombre;
                fila.Cells["Tipo"].Value = grupo.TipoTexto;
                fila.Cells["Usos"].Value = grupo.Nodos.Count;

                for (int c = 3; c < grid.Columns.Count; c++)
                {
                    var col = grid.Columns[c].Tag as ColumnaTreeList;
                    if (col == null) continue;

                    string valor = grupo.Valores[col.Nombre];
                    fila.Cells[c].Value = valor;

                    AplicarEstiloMixto(fila.Cells[c], grupo, col);
                }
            }
        }

        private void AplicarEstiloMixto(DataGridViewCell celda, GrupoInsumo grupo, ColumnaTreeList col)
        {
            bool sigueMixto = grupo.EsMixto[col.Nombre] &&
                              string.IsNullOrEmpty(grupo.Valores[col.Nombre]);

            if (sigueMixto)
            {
                celda.Style.BackColor = Color.FromArgb(255, 249, 196);
                celda.ToolTipText = "Valores distintos entre los usos de este insumo. " +
                                    "Escribe un valor para unificarlo en todos.";
            }
            else
            {
                celda.Style.BackColor = Color.White;
                celda.ToolTipText = "";
            }
        }

        private void grid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 3) return;

            var fila = grid.Rows[e.RowIndex];
            var grupo = fila.Tag as GrupoInsumo;
            var col = grid.Columns[e.ColumnIndex].Tag as ColumnaTreeList;
            if (grupo == null || col == null) return;

            string val = (fila.Cells[e.ColumnIndex].Value?.ToString() ?? "").Trim();
            grupo.Valores[col.Nombre] = val;

            AplicarEstiloMixto(fila.Cells[e.ColumnIndex], grupo, col);
        }

        #endregion

        #region Aplicar cambios

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            // Asegurar que la edición en curso quede confirmada en la celda.
            grid.EndEdit();

            int celdasCambiadas = 0;
            int insumosAfectados = 0;

            foreach (var grupo in grupos)
            {
                bool grupoTocado = false;

                foreach (var col in columnasEditables)
                {
                    string original = grupo.DisplayOriginal[col.Nombre] ?? "";
                    string actual = grupo.Valores[col.Nombre] ?? "";

                    // Sin cambios respecto a lo mostrado: no tocar (protege columnas
                    // que varían por uso, p.ej. Cantidad, dejadas en blanco).
                    if (string.Equals(actual, original, StringComparison.Ordinal))
                        continue;

                    if (!NormalizarValor(col, actual, out object valorFinal))
                    {
                        SeleccionarCelda(grupo, col);
                        MessageBox.Show(
                            $"\"{actual}\" no es un número válido para la columna \"{col.Titulo}\" " +
                            $"del insumo \"{grupo.Nombre}\".\n\n" +
                            "Escribe solo dígitos y, si aplica, un punto decimal.",
                            "Valor inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Propagar a TODAS las apariciones del insumo.
                    foreach (var nodo in grupo.Nodos)
                    {
                        nodo.EstablecerValorColumna(col.Nombre, valorFinal);
                        nodo.FechaModificacion = DateTime.Now;
                    }

                    // El nuevo valor pasa a ser el "original" para futuras comparaciones.
                    grupo.DisplayOriginal[col.Nombre] = actual;
                    grupo.EsMixto[col.Nombre] = false;

                    celdasCambiadas++;
                    grupoTocado = true;
                }

                if (grupoTocado) insumosAfectados++;
            }

            HuboCambios = celdasCambiadas > 0;

            if (HuboCambios)
            {
                MessageBox.Show(
                    $"Se actualizaron {celdasCambiadas} valor(es) en {insumosAfectados} insumo(s).\n\n" +
                    "Los cambios se aplicaron a todas sus apariciones en el árbol. " +
                    "Recuerda Guardar para persistirlos.",
                    "Insumos actualizados", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SeleccionarCelda(GrupoInsumo grupo, ColumnaTreeList col)
        {
            foreach (DataGridViewRow fila in grid.Rows)
            {
                if (!ReferenceEquals(fila.Tag, grupo)) continue;
                for (int c = 3; c < grid.Columns.Count; c++)
                {
                    if (ReferenceEquals(grid.Columns[c].Tag, col))
                    {
                        grid.CurrentCell = fila.Cells[c];
                        return;
                    }
                }
            }
        }

        #endregion

        #region Validación numérica (alineada con el editor)

        private static bool EsNumerica(ColumnaTreeList col)
        {
            var t = col?.TipoDato;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float)
                || t == typeof(int) || t == typeof(long);
        }

        /// <summary>
        /// Misma normalización que usa el editor al editar una celda: vacío => null;
        /// columnas numéricas se validan y guardan con punto decimal invariante.
        /// </summary>
        private static bool NormalizarValor(ColumnaTreeList col, string texto, out object valorFinal)
        {
            texto = (texto ?? "").Trim();

            if (texto.Length == 0)
            {
                valorFinal = null;
                return true;
            }

            if (EsNumerica(col))
            {
                if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal d)
                    && !decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out d))
                {
                    valorFinal = null;
                    return false;
                }

                valorFinal = d.ToString(CultureInfo.InvariantCulture);
                return true;
            }

            valorFinal = texto;
            return true;
        }

        #endregion
    }
}
