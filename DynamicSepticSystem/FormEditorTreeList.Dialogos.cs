using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Diálogo para agregar o renombrar un nodo
    /// </summary>
    public class DialogAgregarNodo : Form
    {
        private TextBox txtNombre;
        private TextBox txtDescripcion;
        private Button btnAceptar;
        private Button btnCancelar;
        private Label lblNombre;
        private Label lblDescripcion;

        public string NombreNodo
        {
            get { return txtNombre.Text.Trim(); }
            set { txtNombre.Text = value; }
        }

        public string DescripcionNodo
        {
            get { return txtDescripcion.Text.Trim(); }
            set { txtDescripcion.Text = value; }
        }

        public DialogAgregarNodo(string titulo)
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            this.Text = titulo;
        }

        private void InitializeComponent()
        {
            this.lblNombre = new Label();
            this.lblDescripcion = new Label();
            this.txtNombre = new TextBox();
            this.txtDescripcion = new TextBox();
            this.btnAceptar = new Button();
            this.btnCancelar = new Button();
            
            this.SuspendLayout();
            
            // lblNombre
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new Font("Segoe UI", 9F);
            this.lblNombre.Location = new Point(12, 15);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new Size(56, 15);
            this.lblNombre.TabIndex = 0;
            this.lblNombre.Text = "Nombre:";
            
            // txtNombre
            this.txtNombre.Font = new Font("Segoe UI", 9F);
            this.txtNombre.Location = new Point(12, 33);
            this.txtNombre.MaxLength = 255;
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new Size(360, 23);
            this.txtNombre.TabIndex = 1;
            
            // lblDescripcion
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Font = new Font("Segoe UI", 9F);
            this.lblDescripcion.Location = new Point(12, 69);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new Size(72, 15);
            this.lblDescripcion.TabIndex = 2;
            this.lblDescripcion.Text = "Descripción:";
            
            // txtDescripcion
            this.txtDescripcion.Font = new Font("Segoe UI", 9F);
            this.txtDescripcion.Location = new Point(12, 87);
            this.txtDescripcion.MaxLength = 500;
            this.txtDescripcion.Multiline = true;
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new Size(360, 80);
            this.txtDescripcion.TabIndex = 3;
            
            // btnAceptar
            this.btnAceptar.BackColor = Color.FromArgb(46, 204, 113);
            this.btnAceptar.FlatStyle = FlatStyle.Flat;
            this.btnAceptar.Font = new Font("Segoe UI", 9F);
            this.btnAceptar.ForeColor = Color.White;
            this.btnAceptar.Location = new Point(197, 183);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new Size(85, 35);
            this.btnAceptar.TabIndex = 4;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = false;
            this.btnAceptar.Click += new EventHandler(this.btnAceptar_Click);
            
            // btnCancelar
            this.btnCancelar.BackColor = Color.FromArgb(189, 195, 199);
            this.btnCancelar.DialogResult = DialogResult.Cancel;
            this.btnCancelar.FlatStyle = FlatStyle.Flat;
            this.btnCancelar.Font = new Font("Segoe UI", 9F);
            this.btnCancelar.Location = new Point(288, 183);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new Size(85, 35);
            this.btnCancelar.TabIndex = 5;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            
            // DialogAgregarNodo
            this.AcceptButton = this.btnAceptar;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new Size(384, 230);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblNombre);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DialogAgregarNodo";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Agregar Nodo";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    /// <summary>
    /// Diálogo para gestionar las columnas personalizadas
    /// </summary>
    public class DialogGestionarColumnas : Form
    {
        private readonly List<ColumnaTreeList> columnas;
        /// <summary>Definiciones resultantes; el editor las aplica al cerrar con OK.</summary>
        public List<ColumnaTreeList> Columnas { get { return columnas; } }
        
        private ListView listViewColumnas;
        private Button btnAgregar;
        private Button btnEditar;
        private Button btnEliminar;
        private Button btnSubir;
        private Button btnBajar;
        private Button btnCerrar;
        private Label lblTitulo;
        private Label lblInfo;

        /// <summary>
        /// Las definiciones se editan EN MEMORIA y las persiste el Guardar del editor
        /// (un solo POST a api/editor-tareas con arbol + columnas). Antes este dialogo
        /// escribia SQL directo -sin transaccion y contra la cadena fija del cliente-,
        /// que es justo lo que dejaba al editor clasico apuntando a otra base que el
        /// editor web. Devuelve la lista en <see cref="Columnas"/> al cerrar con OK.
        /// </summary>
        public DialogGestionarColumnas(List<ColumnaTreeList> columnas)
        {
            this.columnas = new List<ColumnaTreeList>(columnas);

            InitializeComponent();
            ThemeManager.AplicarTema(this);
            CargarColumnas();
        }

        private void InitializeComponent()
        {
            this.lblTitulo = new Label();
            this.lblInfo = new Label();
            this.listViewColumnas = new ListView();
            this.btnAgregar = new Button();
            this.btnEditar = new Button();
            this.btnEliminar = new Button();
            this.btnSubir = new Button();
            this.btnBajar = new Button();
            this.btnCerrar = new Button();
            
            this.SuspendLayout();
            
            // lblTitulo
            this.lblTitulo.BackColor = Color.FromArgb(88, 53, 23);
            this.lblTitulo.Dock = DockStyle.Top;
            this.lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblTitulo.ForeColor = Color.White;
            this.lblTitulo.Location = new Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new Size(684, 50);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Gestor de Columnas Personalizadas";
            this.lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            
            // lblInfo
            this.lblInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            this.lblInfo.ForeColor = Color.FromArgb(127, 140, 141);
            this.lblInfo.Location = new Point(12, 52);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new Size(540, 35);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "Puedes cambiar el orden de las columnas aquí usando Subir/Bajar,\n" +
                               "o arrastrándolas directamente en el TreeListView principal.";
            
            // listViewColumnas
            this.listViewColumnas.Font = new Font("Segoe UI", 9F);
            this.listViewColumnas.FullRowSelect = true;
            this.listViewColumnas.GridLines = true;
            this.listViewColumnas.HideSelection = false;
            this.listViewColumnas.Location = new Point(12, 90);
            this.listViewColumnas.MultiSelect = false;
            this.listViewColumnas.Name = "listViewColumnas";
            this.listViewColumnas.Size = new Size(540, 318);
            this.listViewColumnas.TabIndex = 2;
            this.listViewColumnas.UseCompatibleStateImageBehavior = false;
            this.listViewColumnas.View = View.Details;
            this.listViewColumnas.Columns.Add("Nombre", 120);
            this.listViewColumnas.Columns.Add("Título", 150);
            this.listViewColumnas.Columns.Add("Tipo", 100);
            this.listViewColumnas.Columns.Add("Ancho", 60);
            this.listViewColumnas.Columns.Add("Editable", 70);
            this.listViewColumnas.SelectedIndexChanged += new EventHandler(this.listViewColumnas_SelectedIndexChanged);
            
            // btnAgregar
            this.btnAgregar.BackColor = Color.FromArgb(46, 204, 113);
            this.btnAgregar.FlatStyle = FlatStyle.Flat;
            this.btnAgregar.Font = new Font("Segoe UI", 9F);
            this.btnAgregar.ForeColor = Color.White;
            this.btnAgregar.Location = new Point(558, 90);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new Size(114, 40);
            this.btnAgregar.TabIndex = 3;
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += new EventHandler(this.btnAgregar_Click);
            
            // btnEditar
            this.btnEditar.BackColor = Color.FromArgb(241, 196, 15);
            this.btnEditar.FlatStyle = FlatStyle.Flat;
            this.btnEditar.Font = new Font("Segoe UI", 9F);
            this.btnEditar.ForeColor = Color.White;
            this.btnEditar.Location = new Point(558, 136);
            this.btnEditar.Name = "btnEditar";
            this.btnEditar.Size = new Size(114, 40);
            this.btnEditar.TabIndex = 4;
            this.btnEditar.Text = "Editar";
            this.btnEditar.UseVisualStyleBackColor = false;
            this.btnEditar.Click += new EventHandler(this.btnEditar_Click);
            
            // btnEliminar
            this.btnEliminar.BackColor = Color.FromArgb(231, 76, 60);
            this.btnEliminar.FlatStyle = FlatStyle.Flat;
            this.btnEliminar.Font = new Font("Segoe UI", 9F);
            this.btnEliminar.ForeColor = Color.White;
            this.btnEliminar.Location = new Point(558, 182);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new Size(114, 40);
            this.btnEliminar.TabIndex = 5;
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = false;
            this.btnEliminar.Click += new EventHandler(this.btnEliminar_Click);
            
            // btnSubir
            this.btnSubir.BackColor = Color.FromArgb(243, 156, 18);
            this.btnSubir.FlatStyle = FlatStyle.Flat;
            this.btnSubir.Font = new Font("Segoe UI", 9F);
            this.btnSubir.ForeColor = Color.White;
            this.btnSubir.Location = new Point(558, 248);
            this.btnSubir.Name = "btnSubir";
            this.btnSubir.Size = new Size(114, 40);
            this.btnSubir.TabIndex = 6;
            this.btnSubir.Text = "Subir";
            this.btnSubir.UseVisualStyleBackColor = false;
            this.btnSubir.Click += new EventHandler(this.btnSubir_Click);
            
            // btnBajar
            this.btnBajar.BackColor = Color.FromArgb(243, 156, 18);
            this.btnBajar.FlatStyle = FlatStyle.Flat;
            this.btnBajar.Font = new Font("Segoe UI", 9F);
            this.btnBajar.ForeColor = Color.White;
            this.btnBajar.Location = new Point(558, 294);
            this.btnBajar.Name = "btnBajar";
            this.btnBajar.Size = new Size(114, 40);
            this.btnBajar.TabIndex = 7;
            this.btnBajar.Text = "Bajar";
            this.btnBajar.UseVisualStyleBackColor = false;
            this.btnBajar.Click += new EventHandler(this.btnBajar_Click);
            
            // btnCerrar
            this.btnCerrar.BackColor = Color.FromArgb(52, 152, 219);
            this.btnCerrar.DialogResult = DialogResult.OK;
            this.btnCerrar.FlatStyle = FlatStyle.Flat;
            this.btnCerrar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnCerrar.ForeColor = Color.White;
            this.btnCerrar.Location = new Point(558, 368);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new Size(114, 40);
            this.btnCerrar.TabIndex = 8;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = false;
            
            // DialogGestionarColumnas
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(684, 420);
            this.Controls.Add(this.btnCerrar);
            this.Controls.Add(this.btnBajar);
            this.Controls.Add(this.btnSubir);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.btnEditar);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.listViewColumnas);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblTitulo);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DialogGestionarColumnas";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Gestor de Columnas";
            this.ResumeLayout(false);
        }

        private void CargarColumnas()
        {
            listViewColumnas.Items.Clear();
            
            foreach (var col in columnas)
            {
                var item = new ListViewItem(col.Nombre);
                item.SubItems.Add(col.Titulo);
                item.SubItems.Add(col.TipoDato.Name);
                item.SubItems.Add(col.Ancho.ToString());
                item.SubItems.Add(col.EsEditable ? "Sí" : "No");
                item.Tag = col;
                
                listViewColumnas.Items.Add(item);
            }
            
            ActualizarBotones();
        }

        private void listViewColumnas_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarBotones();
        }

        private void ActualizarBotones()
        {
            bool haySeleccion = listViewColumnas.SelectedItems.Count > 0;
            int indice = haySeleccion ? listViewColumnas.SelectedIndices[0] : -1;
            
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
            btnSubir.Enabled = haySeleccion && indice > 0;
            btnBajar.Enabled = haySeleccion && indice < listViewColumnas.Items.Count - 1;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            using (var dialog = new DialogEditarColumna(null, columnas))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    columnas.Add(dialog.Columna);
                    CargarColumnas();
                }
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (listViewColumnas.SelectedItems.Count == 0) return;
            
            var columna = listViewColumnas.SelectedItems[0].Tag as ColumnaTreeList;
            
            using (var dialog = new DialogEditarColumna(columna, columnas))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    CargarColumnas();
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (listViewColumnas.SelectedItems.Count == 0) return;
            
            var columna = listViewColumnas.SelectedItems[0].Tag as ColumnaTreeList;
            
            var resultado = MessageBox.Show(
                $"¿Estás seguro de eliminar la columna '{columna.Titulo}'?\n\n" +
                "Se perderán todos los datos de esta columna en todos los nodos.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (resultado == DialogResult.Yes)
            {
                columnas.Remove(columna);
                CargarColumnas();
            }
        }

        private void btnSubir_Click(object sender, EventArgs e)
        {
            if (listViewColumnas.SelectedItems.Count == 0) return;
            
            int indice = listViewColumnas.SelectedIndices[0];
            if (indice > 0)
            {
                var temp = columnas[indice];
                columnas[indice] = columnas[indice - 1];
                columnas[indice - 1] = temp;
                
                CargarColumnas();
                listViewColumnas.Items[indice - 1].Selected = true;
            }
        }

        private void btnBajar_Click(object sender, EventArgs e)
        {
            if (listViewColumnas.SelectedItems.Count == 0) return;
            
            int indice = listViewColumnas.SelectedIndices[0];
            if (indice < columnas.Count - 1)
            {
                var temp = columnas[indice];
                columnas[indice] = columnas[indice + 1];
                columnas[indice + 1] = temp;
                
                CargarColumnas();
                listViewColumnas.Items[indice + 1].Selected = true;
            }
        }

    }

    /// <summary>
    /// Diálogo para editar una columna personalizada
    /// </summary>
    public class DialogEditarColumna : Form
    {
        private TextBox txtNombre;
        private TextBox txtTitulo;
        private NumericUpDown numAncho;
        private ComboBox cboTipoDato;
        private CheckBox chkEsEditable;
        private TextBox txtFormato;
        private CheckBox chkEsCalculada;
        private ComboBox cboTipoOperacion;
        private ComboBox cboColumnaOrigen1;
        private ComboBox cboColumnaOrigen2;
        private Label lblColumnaOrigen1;
        private Label lblColumnaOrigen2;
        private Label lblTipoOperacion;
        private Button btnAceptar;
        private Button btnCancelar;
        private Label lblExplicacionTipo;
        private Panel pnlCalculada;
        
        private List<ColumnaTreeList> columnasDisponibles;
        
        public ColumnaTreeList Columna { get; private set; }

        public DialogEditarColumna(ColumnaTreeList columna, List<ColumnaTreeList> columnasExistentes = null)
        {
            this.columnasDisponibles = columnasExistentes ?? new List<ColumnaTreeList>();
            
            this.Columna = columna ?? new ColumnaTreeList
            {
                Nombre = "",
                Titulo = "",
                Ancho = 100,
                TipoDato = typeof(string),
                EsEditable = true,
                Formato = null,
                EsCalculada = false,
                TipoOperacion = TipoOperacion.Ninguna
            };
            
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            CargarDatos();
        }

        private void InitializeComponent()
        {
            var lblNombre = new Label();
            var lblTitulo = new Label();
            var lblAncho = new Label();
            var lblTipoDato = new Label();
            var lblFormato = new Label();
            
            this.txtNombre = new TextBox();
            this.txtTitulo = new TextBox();
            this.numAncho = new NumericUpDown();
            this.cboTipoDato = new ComboBox();
            this.chkEsEditable = new CheckBox();
            this.txtFormato = new TextBox();
            this.chkEsCalculada = new CheckBox();
            this.pnlCalculada = new Panel();
            this.lblTipoOperacion = new Label();
            this.cboTipoOperacion = new ComboBox();
            this.lblColumnaOrigen1 = new Label();
            this.cboColumnaOrigen1 = new ComboBox();
            this.lblColumnaOrigen2 = new Label();
            this.cboColumnaOrigen2 = new ComboBox();
            this.btnAceptar = new Button();
            this.btnCancelar = new Button();
            this.lblExplicacionTipo = new Label();
            
            this.SuspendLayout();
            
            int y = 12;
            
            // Nombre
            lblNombre.Location = new Point(12, y);
            lblNombre.Size = new Size(100, 23);
            lblNombre.Text = "Nombre:";
            lblNombre.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(lblNombre);
            
            txtNombre.Location = new Point(120, y);
            txtNombre.Size = new Size(252, 23);
            txtNombre.MaxLength = 50;
            txtNombre.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(txtNombre);
            
            y += 35;
            
            // Título
            lblTitulo.Location = new Point(12, y);
            lblTitulo.Size = new Size(100, 23);
            lblTitulo.Text = "Título:";
            lblTitulo.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(lblTitulo);
            
            txtTitulo.Location = new Point(120, y);
            txtTitulo.Size = new Size(252, 23);
            txtTitulo.MaxLength = 100;
            txtTitulo.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(txtTitulo);
            
            y += 35;
            
            // Ancho
            lblAncho.Location = new Point(12, y);
            lblAncho.Size = new Size(100, 23);
            lblAncho.Text = "Ancho (px):";
            lblAncho.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(lblAncho);
            
            numAncho.Location = new Point(120, y);
            numAncho.Size = new Size(100, 23);
            numAncho.Minimum = 50;
            numAncho.Maximum = 500;
            numAncho.Value = 100;
            numAncho.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(numAncho);
            
            y += 35;
            
            // Es Calculada
            chkEsCalculada.Location = new Point(120, y);
            chkEsCalculada.Size = new Size(252, 30);
            chkEsCalculada.Text = "✓ Es una columna calculada\n(resultado de una operación entre dos columnas)";
            chkEsCalculada.Checked = false;
            chkEsCalculada.Font = new Font("Segoe UI", 9F);
            chkEsCalculada.CheckedChanged += chkEsCalculada_CheckedChanged;
            this.Controls.Add(chkEsCalculada);
            
            y += 40;
            
            // Panel de columnas calculadas
            pnlCalculada.Location = new Point(12, y);
            pnlCalculada.Size = new Size(360, 160);
            pnlCalculada.BackColor = Color.FromArgb(240, 248, 255);
            pnlCalculada.BorderStyle = BorderStyle.FixedSingle;
            pnlCalculada.Visible = false;
            
            int yPanel = 8;
            
            // Tipo de Operación
            lblTipoOperacion.Location = new Point(8, yPanel);
            lblTipoOperacion.Size = new Size(100, 23);
            lblTipoOperacion.Text = "Operación:";
            lblTipoOperacion.Font = new Font("Segoe UI", 9F);
            pnlCalculada.Controls.Add(lblTipoOperacion);
            
            cboTipoOperacion.Location = new Point(108, yPanel);
            cboTipoOperacion.Size = new Size(240, 23);
            cboTipoOperacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoOperacion.Font = new Font("Segoe UI", 9F);
            cboTipoOperacion.Items.AddRange(new object[] {
                "× Multiplicación",
                "+ Suma",
                "- Resta",
                "÷ División"
            });
            cboTipoOperacion.SelectedIndex = 0;
            pnlCalculada.Controls.Add(cboTipoOperacion);
            
            yPanel += 35;
            
            // Primera Columna
            lblColumnaOrigen1.Location = new Point(8, yPanel);
            lblColumnaOrigen1.Size = new Size(100, 23);
            lblColumnaOrigen1.Text = "Columna 1:";
            lblColumnaOrigen1.Font = new Font("Segoe UI", 9F);
            pnlCalculada.Controls.Add(lblColumnaOrigen1);
            
            cboColumnaOrigen1.Location = new Point(108, yPanel);
            cboColumnaOrigen1.Size = new Size(240, 23);
            cboColumnaOrigen1.DropDownStyle = ComboBoxStyle.DropDownList;
            cboColumnaOrigen1.Font = new Font("Segoe UI", 9F);
            pnlCalculada.Controls.Add(cboColumnaOrigen1);
            
            yPanel += 35;
            
            // Segunda Columna
            lblColumnaOrigen2.Location = new Point(8, yPanel);
            lblColumnaOrigen2.Size = new Size(100, 23);
            lblColumnaOrigen2.Text = "Columna 2:";
            lblColumnaOrigen2.Font = new Font("Segoe UI", 9F);
            pnlCalculada.Controls.Add(lblColumnaOrigen2);
            
            cboColumnaOrigen2.Location = new Point(108, yPanel);
            cboColumnaOrigen2.Size = new Size(240, 23);
            cboColumnaOrigen2.DropDownStyle = ComboBoxStyle.DropDownList;
            cboColumnaOrigen2.Font = new Font("Segoe UI", 9F);
            pnlCalculada.Controls.Add(cboColumnaOrigen2);
            
            yPanel += 35;
            
            // Ejemplo
            var lblEjemplo = new Label();
            lblEjemplo.Location = new Point(8, yPanel);
            lblEjemplo.Size = new Size(340, 40);
            lblEjemplo.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblEjemplo.ForeColor = Color.FromArgb(52, 73, 94);
            lblEjemplo.Text = "💡 Ejemplo: Precio × Cantidad = Total\nLa columna se actualizará automáticamente cuando cambien\nlos valores de las columnas de origen.";
            pnlCalculada.Controls.Add(lblEjemplo);
            
            this.Controls.Add(pnlCalculada);
            
            y += 165;
            
            // Tipo de Dato
            lblTipoDato.Location = new Point(12, y);
            lblTipoDato.Size = new Size(100, 23);
            lblTipoDato.Text = "Tipo de Dato:";
            lblTipoDato.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(lblTipoDato);
            
            cboTipoDato.Location = new Point(120, y);
            cboTipoDato.Size = new Size(252, 23);
            cboTipoDato.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoDato.Font = new Font("Segoe UI", 9F);
            cboTipoDato.Items.AddRange(new object[] {
                "Texto - Para nombres, descripciones, etc.",
                "Número entero - Para cantidades, unidades, etc.",
                "Número decimal - Para precios, medidas, porcentajes",
                "Fecha - Para fechas y horas",
                "Sí/No - Para opciones verdadero/falso"
            });
            cboTipoDato.SelectedIndex = 0;
            cboTipoDato.SelectedIndexChanged += cboTipoDato_SelectedIndexChanged;
            this.Controls.Add(cboTipoDato);
            
            y += 30;
            
            // Explicación del tipo de dato
            lblExplicacionTipo.Location = new Point(120, y);
            lblExplicacionTipo.Size = new Size(252, 50);
            lblExplicacionTipo.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblExplicacionTipo.ForeColor = Color.FromArgb(127, 140, 141);
            lblExplicacionTipo.Text = "Permite escribir cualquier tipo de texto.";
            this.Controls.Add(lblExplicacionTipo);
            
            y += 55;
            
            // Es Editable
            chkEsEditable.Location = new Point(120, y);
            chkEsEditable.Size = new Size(252, 40);
            chkEsEditable.Text = "La columna es editable en todos los niveles\n(Padre, Sub-Padre e Hijo)";
            chkEsEditable.Checked = true;
            chkEsEditable.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(chkEsEditable);
            
            y += 50;
            
            // Formato
            lblFormato.Location = new Point(12, y);
            lblFormato.Size = new Size(100, 23);
            lblFormato.Text = "Formato:";
            lblFormato.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(lblFormato);
            
            txtFormato.Location = new Point(120, y);
            txtFormato.Size = new Size(252, 23);
            txtFormato.MaxLength = 50;
            txtFormato.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(txtFormato);
            
            y += 45;
            
            // Botones
            btnCancelar.Location = new Point(287, y);
            btnCancelar.Size = new Size(85, 35);
            btnCancelar.Text = "Cancelar";
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.BackColor = Color.FromArgb(189, 195, 199);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(btnCancelar);
            
            btnAceptar.Location = new Point(196, y);
            btnAceptar.Size = new Size(85, 35);
            btnAceptar.Text = "Aceptar";
            btnAceptar.BackColor = Color.FromArgb(46, 204, 113);
            btnAceptar.ForeColor = Color.White;
            btnAceptar.FlatStyle = FlatStyle.Flat;
            btnAceptar.Font = new Font("Segoe UI", 9F);
            btnAceptar.Click += btnAceptar_Click;
            this.Controls.Add(btnAceptar);
            
            // Form
            this.AcceptButton = btnAceptar;
            this.CancelButton = btnCancelar;
            this.ClientSize = new Size(384, y + 50);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Editar Columna";
            this.Font = new Font("Segoe UI", 9F);
            
            this.ResumeLayout(false);
        }

        private void chkEsCalculada_CheckedChanged(object sender, EventArgs e)
        {
            pnlCalculada.Visible = chkEsCalculada.Checked;
            chkEsEditable.Enabled = !chkEsCalculada.Checked;
            
            if (chkEsCalculada.Checked)
            {
                chkEsEditable.Checked = false; // Las columnas calculadas no son editables
                cboTipoDato.SelectedIndex = 2; // Número decimal por defecto
                
                // Cargar columnas disponibles (excluyendo la actual si está en edición)
                cboColumnaOrigen1.Items.Clear();
                cboColumnaOrigen2.Items.Clear();
                
                foreach (var col in columnasDisponibles)
                {
                    // Solo agregar columnas que no sean calculadas y que sean numéricas
                    if (!col.EsCalculada && 
                        (col.TipoDato == typeof(int) || col.TipoDato == typeof(decimal) || col.TipoDato == typeof(double)) &&
                        col.Nombre != Columna.Nombre)
                    {
                        cboColumnaOrigen1.Items.Add($"{col.Titulo} ({col.Nombre})");
                        cboColumnaOrigen2.Items.Add($"{col.Titulo} ({col.Nombre})");
                    }
                }
                
                if (cboColumnaOrigen1.Items.Count > 0)
                {
                    cboColumnaOrigen1.SelectedIndex = 0;
                    if (cboColumnaOrigen2.Items.Count > 1)
                        cboColumnaOrigen2.SelectedIndex = 1;
                    else if (cboColumnaOrigen2.Items.Count > 0)
                        cboColumnaOrigen2.SelectedIndex = 0;
                }
            }
            
            // Ajustar altura del formulario
            AjustarAlturaFormulario();
        }
        
        private void AjustarAlturaFormulario()
        {
            int alturaExtra = chkEsCalculada.Checked ? 165 : 0;
            this.ClientSize = new Size(384, 520 + alturaExtra);
        }

        private void CargarDatos()
        {
            txtNombre.Text = Columna.Nombre;
            txtTitulo.Text = Columna.Titulo;
            numAncho.Value = Columna.Ancho;
            chkEsEditable.Checked = Columna.EsEditable;
            txtFormato.Text = Columna.Formato ?? "";
            chkEsCalculada.Checked = Columna.EsCalculada;
            
            // Seleccionar tipo de operación
            if (Columna.EsCalculada)
            {
                switch (Columna.TipoOperacion)
                {
                    case TipoOperacion.Multiplicacion:
                        cboTipoOperacion.SelectedIndex = 0;
                        break;
                    case TipoOperacion.Suma:
                        cboTipoOperacion.SelectedIndex = 1;
                        break;
                    case TipoOperacion.Resta:
                        cboTipoOperacion.SelectedIndex = 2;
                        break;
                    case TipoOperacion.Division:
                        cboTipoOperacion.SelectedIndex = 3;
                        break;
                }
                
                // Cargar columnas origen
                chkEsCalculada_CheckedChanged(null, EventArgs.Empty);
                
                // Seleccionar columnas de origen si están definidas
                if (!string.IsNullOrEmpty(Columna.ColumnaOrigen1))
                {
                    for (int i = 0; i < cboColumnaOrigen1.Items.Count; i++)
                    {
                        if (cboColumnaOrigen1.Items[i].ToString().Contains(Columna.ColumnaOrigen1))
                        {
                            cboColumnaOrigen1.SelectedIndex = i;
                            break;
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(Columna.ColumnaOrigen2))
                {
                    for (int i = 0; i < cboColumnaOrigen2.Items.Count; i++)
                    {
                        if (cboColumnaOrigen2.Items[i].ToString().Contains(Columna.ColumnaOrigen2))
                        {
                            cboColumnaOrigen2.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            
            // Seleccionar tipo de dato
            if (Columna.TipoDato == typeof(string))
                cboTipoDato.SelectedIndex = 0;
            else if (Columna.TipoDato == typeof(int))
                cboTipoDato.SelectedIndex = 1;
            else if (Columna.TipoDato == typeof(decimal))
                cboTipoDato.SelectedIndex = 2;
            else if (Columna.TipoDato == typeof(DateTime))
                cboTipoDato.SelectedIndex = 3;
            else if (Columna.TipoDato == typeof(bool))
                cboTipoDato.SelectedIndex = 4;
            else
                cboTipoDato.SelectedIndex = 0;
        }

        private void cboTipoDato_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboTipoDato.SelectedIndex)
            {
                case 0: // Texto
                    lblExplicacionTipo.Text = "Permite escribir cualquier tipo de texto.\nEjemplo: nombres, descripciones, direcciones.";
                    break;
                case 1: // Número entero
                    lblExplicacionTipo.Text = "Solo números sin decimales.\nEjemplo: 1, 25, 100, 1500.";
                    break;
                case 2: // Número decimal
                    lblExplicacionTipo.Text = "Números con decimales.\nEjemplo: 12.50, 3.14, 99.99.";
                    break;
                case 3: // Fecha
                    lblExplicacionTipo.Text = "Fechas y horas.\nEjemplo: 15/03/2024, 01/01/2024 10:30 AM.";
                    lblExplicacionTipo.Text = "💡 Ejemplo: Precio × Cantidad = Total\nLa columna se actualizará automáticamente cuando cambien\nlos valores de las columnas de origen.";
                    break;
                case 4: // Sí/No
                    lblExplicacionTipo.Text = "Solo dos opciones: Sí o No.\nEjemplo: Activo, Completado, Aprobado.";
                    break;
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtTitulo.Text))
            {
                MessageBox.Show("El título es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitulo.Focus();
                return;
            }
            
            // Validar columnas calculadas
            if (chkEsCalculada.Checked)
            {
                if (cboColumnaOrigen1.SelectedIndex < 0 || cboColumnaOrigen2.SelectedIndex < 0)
                {
                    MessageBox.Show("Debes seleccionar ambas columnas de origen para el cálculo.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            
            Columna.Nombre = txtNombre.Text.Trim();
            Columna.Titulo = txtTitulo.Text.Trim();
            Columna.Ancho = (int)numAncho.Value;
            Columna.EsEditable = chkEsEditable.Checked;
            Columna.Formato = string.IsNullOrWhiteSpace(txtFormato.Text) ? null : txtFormato.Text.Trim();
            Columna.EsCalculada = chkEsCalculada.Checked;
            
            if (Columna.EsCalculada)
            {
                // Asignar tipo de operación
                switch (cboTipoOperacion.SelectedIndex)
                {
                    case 0:
                        Columna.TipoOperacion = TipoOperacion.Multiplicacion;
                        break;
                    case 1:
                        Columna.TipoOperacion = TipoOperacion.Suma;
                        break;
                    case 2:
                        Columna.TipoOperacion = TipoOperacion.Resta;
                        break;
                    case 3:
                        Columna.TipoOperacion = TipoOperacion.Division;
                        break;
                }
                
                // Extraer nombres de columnas desde el ComboBox (formato: "Título (Nombre)")
                string item1 = cboColumnaOrigen1.SelectedItem.ToString();
                string item2 = cboColumnaOrigen2.SelectedItem.ToString();
                
                Columna.ColumnaOrigen1 = ExtraerNombreColumna(item1);
                Columna.ColumnaOrigen2 = ExtraerNombreColumna(item2);
                
                // Para columnas calculadas, el tipo de dato siempre es decimal
                Columna.TipoDato = typeof(decimal);
            }
            else
            {
                // Asignar tipo de dato normal
                switch (cboTipoDato.SelectedIndex)
                {
                    case 0:
                        Columna.TipoDato = typeof(string);
                        break;
                    case 1:
                        Columna.TipoDato = typeof(int);
                        break;
                    case 2:
                        Columna.TipoDato = typeof(decimal);
                        break;
                    case 3:
                        Columna.TipoDato = typeof(DateTime);
                        break;
                    case 4:
                        Columna.TipoDato = typeof(bool);
                        break;
                    default:
                        Columna.TipoDato = typeof(string);
                        break;
                }
                
                Columna.TipoOperacion = TipoOperacion.Ninguna;
                Columna.ColumnaOrigen1 = null;
                Columna.ColumnaOrigen2 = null;
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        
        private string ExtraerNombreColumna(string item)
        {
            // Formato: "Título (Nombre)"
            int inicioParentesis = item.LastIndexOf('(');
            int finParentesis = item.LastIndexOf(')');
            
            if (inicioParentesis > 0 && finParentesis > inicioParentesis)
            {
                return item.Substring(inicioParentesis + 1, finParentesis - inicioParentesis - 1);
            }
            
            return item;
        }
    }
    
    /// <summary>
    /// Diálogo para asignar tipo de tarea a un nodo hijo
    /// </summary>
    public class DialogAsignarTipoTarea : Form
    {
        private RadioButton rbMaterial;
        private RadioButton rbManoDeObra;
        private RadioButton rbNinguno;
        private Button btnAceptar;
        private Button btnCancelar;
        private Label lblTitulo;
        private Label lblDescripcion;
        
        public TipoTarea TipoTareaSeleccionado { get; private set; }
        
        public DialogAsignarTipoTarea(TipoTarea tipoActual)
        {
            this.TipoTareaSeleccionado = tipoActual;
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            
            // Seleccionar el radio button correspondiente
            switch (tipoActual)
            {
                case TipoTarea.Material:
                    rbMaterial.Checked = true;
                    break;
                case TipoTarea.ManoDeObra:
                    rbManoDeObra.Checked = true;
                    break;
                default:
                    rbNinguno.Checked = true;
                    break;
            }
        }
        
        private void InitializeComponent()
        {
            this.lblTitulo = new Label();
            this.lblDescripcion = new Label();
            this.rbMaterial = new RadioButton();
            this.rbManoDeObra = new RadioButton();
            this.rbNinguno = new RadioButton();
            this.btnAceptar = new Button();
            this.btnCancelar = new Button();
            
            this.SuspendLayout();
            
            // lblTitulo
            this.lblTitulo.BackColor = Color.FromArgb(88, 53, 23);
            this.lblTitulo.Dock = DockStyle.Top;
            this.lblTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lblTitulo.ForeColor = Color.White;
            this.lblTitulo.Location = new Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new Size(400, 45);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Asignar Tipo de Tarea";
            this.lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            
            // lblDescripcion
            this.lblDescripcion.Font = new Font("Segoe UI", 9F);
            this.lblDescripcion.ForeColor = Color.FromArgb(127, 140, 141);
            this.lblDescripcion.Location = new Point(12, 55);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new Size(376, 35);
            this.lblDescripcion.TabIndex = 1;
            this.lblDescripcion.Text = "Selecciona el tipo de tarea para esta actividad.\nEsto afectará cómo se visualiza en el árbol.";
            
            // rbMaterial
            this.rbMaterial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.rbMaterial.ForeColor = Color.FromArgb(192, 57, 43);
            this.rbMaterial.Location = new Point(30, 100);
            this.rbMaterial.Name = "rbMaterial";
            this.rbMaterial.Size = new Size(340, 30);
            this.rbMaterial.TabIndex = 2;
            this.rbMaterial.Text = "Material";
            this.rbMaterial.UseVisualStyleBackColor = true;
            
            // rbManoDeObra
            this.rbManoDeObra.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.rbManoDeObra.ForeColor = Color.FromArgb(39, 174, 96);
            this.rbManoDeObra.Location = new Point(30, 136);
            this.rbManoDeObra.Name = "rbManoDeObra";
            this.rbManoDeObra.Size = new Size(340, 30);
            this.rbManoDeObra.TabIndex = 3;
            this.rbManoDeObra.Text = "Mano de Obra";
            this.rbManoDeObra.UseVisualStyleBackColor = true;
            
            // rbNinguno
            this.rbNinguno.Font = new Font("Segoe UI", 9F);
            this.rbNinguno.ForeColor = Color.FromArgb(127, 140, 141);
            this.rbNinguno.Location = new Point(30, 172);
            this.rbNinguno.Name = "rbNinguno";
            this.rbNinguno.Size = new Size(340, 30);
            this.rbNinguno.TabIndex = 4;
            this.rbNinguno.Text = "Sin especificar";
            this.rbNinguno.UseVisualStyleBackColor = true;
            
            // btnAceptar
            this.btnAceptar.BackColor = Color.FromArgb(46, 204, 113);
            this.btnAceptar.FlatStyle = FlatStyle.Flat;
            this.btnAceptar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnAceptar.ForeColor = Color.White;
            this.btnAceptar.Location = new Point(212, 220);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new Size(85, 35);
            this.btnAceptar.TabIndex = 5;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = false;
            this.btnAceptar.Click += btnAceptar_Click;
            
            // btnCancelar
            this.btnCancelar.BackColor = Color.FromArgb(189, 195, 199);
            this.btnCancelar.DialogResult = DialogResult.Cancel;
            this.btnCancelar.FlatStyle = FlatStyle.Flat;
            this.btnCancelar.Font = new Font("Segoe UI", 9F);
            this.btnCancelar.Location = new Point(303, 220);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new Size(85, 35);
            this.btnCancelar.TabIndex = 6;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            
            // DialogAsignarTipoTarea
            this.AcceptButton = this.btnAceptar;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new Size(400, 270);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.rbNinguno);
            this.Controls.Add(this.rbManoDeObra);
            this.Controls.Add(this.rbMaterial);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.lblTitulo);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DialogAsignarTipoTarea";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Tipo de Tarea";
            this.ResumeLayout(false);
        }
        
        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (rbMaterial.Checked)
                TipoTareaSeleccionado = TipoTarea.Material;
            else if (rbManoDeObra.Checked)
                TipoTareaSeleccionado = TipoTarea.ManoDeObra;
            else
                TipoTareaSeleccionado = TipoTarea.Ninguno;
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
