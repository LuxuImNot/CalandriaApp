using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Editor de TreeList para gestionar estructuras jer�rquicas de 3 niveles
    /// Padre -> Sub-Padre -> Hijo
    /// </summary>
    public partial class FormEditorTreeList : Form
    {
        #region Campos privados

        private readonly string connectionString;
        private string nombreTabla;
        private List<NodoTree> nodosRaiz;
        private List<ColumnaTreeList> columnasPersonalizadas;
        private bool cambiosPendientes;
        private int siguienteId = 1;

        // Guardado asincrono: evita reentradas y permite posponer el cierre del form
        private bool guardando;
        private bool cerrarForzado;

        // Campos para drag & drop
        private List<NodoTree> nodosArrastrados;
        private bool dragEnProgreso;
        // �ltimo nodo resaltado durante el arrastre, para no re-seleccionar (ni
        // repintar el �rbol virtual) en cada evento DragOver.
        private NodoTree nodoDestinoResaltado;

        #endregion

        #region Constructor

        public FormEditorTreeList(string connectionString, string nombreTablaInicial = "RutaTuneraDestajo")
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            RestaurarEstilosBotonesPanel();

            this.connectionString = connectionString;
            this.nombreTabla = nombreTablaInicial;
            this.nodosRaiz = new List<NodoTree>();
            this.columnasPersonalizadas = new List<ColumnaTreeList>();
            this.cambiosPendientes = false;
            this.dragEnProgreso = false;
            this.nodosArrastrados = new List<NodoTree>();

            ConfigurarComboBoxRutas();
            ConfigurarTreeListView();
            ConfigurarDragDrop();
            CargarDatosIniciales();
            ActualizarTitulo();
            ActualizarEstadoBotones();
            InicializarPanelWeb();
        }

        // ThemeManager.AplicarTema sobreescribe BackColor/ForeColor/Font de todos los Button.
        // Aquí restauramos los colores semánticos y la fuente compacta del panel izquierdo.
        private void RestaurarEstilosBotonesPanel()
        {
            var fuenteBoton = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            var fuenteAccion = new Font("Segoe UI", 9F, FontStyle.Bold);
            var btns = new (Button btn, Color back, Font font)[]
            {
                (btnAgregarPadre,    Color.FromArgb(46, 204, 113),  fuenteBoton),
                (btnAgregarSubPadre, Color.FromArgb(52, 152, 219),  fuenteBoton),
                (btnAgregarHijo,     Color.FromArgb(155, 89, 182),  fuenteBoton),
                (btnRenombrar,       Color.FromArgb(241, 196, 15),  fuenteBoton),
                (btnEliminar,        Color.FromArgb(231, 76, 60),   fuenteBoton),
                (btnAsignarTipo,     Color.FromArgb(142, 68, 173),  fuenteBoton),
                (btnSubir,           Color.FromArgb(243, 156, 18),  fuenteBoton),
                (btnBajar,           Color.FromArgb(243, 156, 18),  fuenteBoton),
                (btnSubirJerarquia,  Color.FromArgb(26, 188, 156),  fuenteBoton),
                (btnBajarJerarquia,  Color.FromArgb(26, 188, 156),  fuenteBoton),
                (btnConvertirAPadre, Color.FromArgb(22, 160, 133),  fuenteBoton),
                (btnExpandirTodo,    Color.FromArgb(52, 73, 94),    fuenteBoton),
                (btnColapsarTodo,    Color.FromArgb(52, 73, 94),    fuenteBoton),
                (btnGestionarColumnas, Color.FromArgb(52, 152, 219), fuenteBoton),
                (btnGestorInsumos,   Color.FromArgb(155, 89, 182),  fuenteBoton),
                (btnGuardar,         Color.FromArgb(46, 204, 113),  fuenteAccion),
                (btnCerrar,          Color.FromArgb(189, 195, 199), fuenteAccion),
            };
            foreach (var (btn, back, font) in btns)
            {
                if (btn == null) continue;
                btn.BackColor = back;
                btn.ForeColor = Color.White;
                btn.Font = font;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.UseVisualStyleBackColor = false;
            }
        }

        #endregion

        #region Configuraci�n del TreeListView

        private void ConfigurarComboBoxRutas()
        {
            cboRutas.Items.Clear();
            cboRutas.Items.Add("Ruta Tunera Destajo");
            cboRutas.Items.Add("Ruta Calandra Destajo");
            
            // Seleccionar el item correspondiente a la tabla actual
            if (nombreTabla == "RutaCalandraDestajo")
                cboRutas.SelectedIndex = 1;
            else
                cboRutas.SelectedIndex = 0;
            
            cboRutas.SelectedIndexChanged += cboRutas_SelectedIndexChanged;
        }

        private async void cboRutas_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Verificar si hay cambios pendientes antes de cambiar
            if (cambiosPendientes)
            {
                var resultado = MessageBox.Show(
                    "Hay cambios sin guardar.\n\n�Deseas guardar antes de cambiar de ruta?",
                    "Cambios sin guardar",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    // Esperar a que termine el guardado; si falla, no cambiar de ruta
                    bool guardado = await GuardarCambiosAsync();
                    if (!guardado)
                    {
                        cboRutas.SelectedIndexChanged -= cboRutas_SelectedIndexChanged;
                        cboRutas.SelectedIndex = nombreTabla == "RutaCalandraDestajo" ? 1 : 0;
                        cboRutas.SelectedIndexChanged += cboRutas_SelectedIndexChanged;
                        return;
                    }
                }
                else if (resultado == DialogResult.Cancel)
                {
                    // Restaurar selecci�n previa
                    cboRutas.SelectedIndexChanged -= cboRutas_SelectedIndexChanged;
                    if (nombreTabla == "RutaCalandraDestajo")
                        cboRutas.SelectedIndex = 1;
                    else
                        cboRutas.SelectedIndex = 0;
                    cboRutas.SelectedIndexChanged += cboRutas_SelectedIndexChanged;
                    return;
                }
            }
            
            // Cambiar a la nueva tabla
            string nuevaTabla = cboRutas.SelectedIndex == 0 ? "RutaTuneraDestajo" : "RutaCalandraDestajo";
            
            if (nuevaTabla != nombreTabla)
            {
                nombreTabla = nuevaTabla;
                cambiosPendientes = false;
                
                // Recargar datos de la nueva tabla
                CargarDatosIniciales();
                ActualizarTitulo();
                ActualizarEstadoBotones();
            }
        }

        private void ActualizarTitulo()
        {
            string nombreRuta = cboRutas.SelectedIndex == 0 ? "Ruta Tunera Destajo" : "Ruta Calandra Destajo";
            this.Text = $"Editor de TreeList - {nombreRuta}";
            lblTitulo.Text = $"Editor TreeList\n{nombreRuta}";
        }

        private void ConfigurarTreeListView()
        {
            treeListView.CanExpandGetter = delegate(object x)
            {
                return ((NodoTree)x).TieneHijos;
            };
            
            treeListView.ChildrenGetter = delegate(object x)
            {
                return ((NodoTree)x).Hijos;
            };
            
            // Columna de contador global
            var colContador = new OLVColumn("# Destajo", null)
            {
                Width = 80,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectGetter = delegate(object x)
                {
                    var nodo = x as NodoTree;
                    if (nodo == null || nodo.Nivel != 1) return "";
                    
                    // Calcular el �ndice global del nodo
                    int indice = ObtenerIndiceGlobal(nodo);
                    return indice > 0 ? indice.ToString() : "";
                }
            };
            
            var colNombre = new OLVColumn("Nombre", "Nombre")
            {
                Width = 250,
                IsEditable = true
            };
            
            treeListView.TreeColumnRenderer.Column = colNombre;
            
            var colTipo = new OLVColumn("Tipo", "TipoNodo")
            {
                Width = 100,
                IsEditable = false
            };
            
            var colTipoTarea = new OLVColumn("Tipo Tarea", "TipoTareaTexto")
            {
                Width = 100,
                IsEditable = false
            };
            
            var colDescripcion = new OLVColumn("Descripci�n", "Descripcion")
            {
                Width = 300,
                IsEditable = true
            };
            
            var colOrden = new OLVColumn("Orden", "Orden")
            {
                Width = 60,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center
            };
            
            treeListView.AllColumns.Clear();
            treeListView.AllColumns.Add(colContador);
            treeListView.AllColumns.Add(colNombre);
            treeListView.AllColumns.Add(colTipo);
            treeListView.AllColumns.Add(colTipoTarea);
            treeListView.AllColumns.Add(colDescripcion);
            treeListView.AllColumns.Add(colOrden);
            
            treeListView.RebuildColumns();
            
            treeListView.CellEditFinishing += TreeListView_CellEditFinishing;
            treeListView.SelectionChanged += TreeListView_SelectionChanged;
            treeListView.FormatRow += TreeListView_FormatRow;
            treeListView.FormatCell += TreeListView_FormatCell;
            treeListView.KeyDown += TreeListView_KeyDown;
            treeListView.ColumnReordered += TreeListView_ColumnReordered;
            treeListView.CellClick += TreeListView_CellClick;
            
            treeListView.UseAlternatingBackColors = true;
            treeListView.AlternateRowBackColor = Color.FromArgb(250, 250, 250);
            treeListView.FullRowSelect = true;
            treeListView.ShowGroups = false;
            treeListView.UseCompatibleStateImageBehavior = false;
            treeListView.View = View.Details;
            
            // Habilitar selecci�n m�ltiple
            treeListView.MultiSelect = true;
            
            // Permitir reordenar columnas arrastr�ndolas con el mouse
            treeListView.AllowColumnReorder = true;
            
            // Configurar edici�n de celdas al hacer doble clic
            treeListView.CellEditActivation = ObjectListView.CellEditActivateMode.DoubleClick;
        }

        private void TreeListView_CellClick(object sender, CellClickEventArgs e)
        {
            // Si se hace doble clic en una celda editable, la edici�n se inicia autom�ticamente
            // gracias a CellEditActivation = DoubleClick
        }

        private void TreeListView_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            // Este evento se dispara cuando el usuario arrastra y suelta una columna
            // para cambiar su posici�n en el TreeListView
            // Nota: En ObjectListView, el orden visual puede no coincidir exactamente con AllColumns
            // por lo que no necesitamos hacer nada especial aqu� - el orden visual se mantiene autom�ticamente
        }

        private void TreeListView_KeyDown(object sender, KeyEventArgs e)
        {
            // Detectar tecla Supr (Delete)
            if (e.KeyCode == Keys.Delete)
            {
                // Llamar al m�todo de eliminar
                btnEliminar_Click(sender, e);
                e.Handled = true;
            }
        }

        private void TreeListView_FormatRow(object sender, FormatRowEventArgs e)
        {
            var nodo = e.Model as NodoTree;
            if (nodo == null) return;
            
            // Aplicar color de fondo seg�n nivel
            switch (nodo.Nivel)
            {
                case 0:
                    e.Item.BackColor = Color.FromArgb(200, 230, 255);
                    e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                    break;
                case 1:
                    e.Item.BackColor = Color.FromArgb(230, 245, 255);
                    e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                    break;
                case 2:
                    // Para nodos hijo, aplicar color de texto seg�n tipo de tarea
                    switch (nodo.TipoTarea)
                    {
                        case TipoTarea.Material:
                            e.Item.ForeColor = Color.FromArgb(192, 57, 43);
                            e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                            break;
                        case TipoTarea.ManoDeObra:
                            e.Item.ForeColor = Color.FromArgb(39, 174, 96);
                            e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                            break;
                    }
                    break;
            }
        }
        
        private void TreeListView_FormatCell(object sender, FormatCellEventArgs e)
        {
            var nodo = e.Model as NodoTree;
            if (nodo == null || nodo.Nivel != 2) return;
            
            // Aplicar color seg�n el tipo de tarea para todas las celdas del nodo hijo
            switch (nodo.TipoTarea)
            {
                case TipoTarea.Material:
                    e.SubItem.ForeColor = Color.FromArgb(192, 57, 43);
                    break;
                case TipoTarea.ManoDeObra:
                    e.SubItem.ForeColor = Color.FromArgb(39, 174, 96);
                    break;
            }
        }

        private void TreeListView_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            if (e.Cancel) return;
            cambiosPendientes = true;
            ActualizarEstadoBotones();
        }

        private void TreeListView_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        #endregion

        #region Configuraci�n de Drag & Drop

        private void ConfigurarDragDrop()
        {
            treeListView.AllowDrop = true;
            treeListView.ItemDrag += TreeListView_ItemDrag;
            treeListView.DragEnter += TreeListView_DragEnter;
            treeListView.DragOver += TreeListView_DragOver;
            treeListView.DragDrop += TreeListView_DragDrop;
            treeListView.DragLeave += TreeListView_DragLeave;
        }

        private void TreeListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                nodosArrastrados.Clear();
                nodoDestinoResaltado = null;
                
                // Obtener todos los nodos seleccionados
                foreach (var obj in treeListView.SelectedObjects)
                {
                    var nodo = obj as NodoTree;
                    if (nodo != null)
                    {
                        nodosArrastrados.Add(nodo);
                    }
                }
                
                if (nodosArrastrados.Count > 0)
                {
                    // Validar que todos los nodos seleccionados sean del mismo nivel
                    if (!TodosMismoNivel(nodosArrastrados))
                    {
                        MessageBox.Show(
                            "Solo puedes arrastrar nodos del mismo nivel (Padre, Sub-Padre o Hijo) simult�neamente.",
                            "Selecci�n no v�lida",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        nodosArrastrados.Clear();
                        return;
                    }
                    
                    // Validar que los nodos seleccionados sean hermanos (mismo padre)
                    if (!TodosHermanos(nodosArrastrados))
                    {
                        MessageBox.Show(
                            "Solo puedes arrastrar nodos que sean hermanos (mismo nodo padre) simult�neamente.",
                            "Selecci�n no v�lida",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        nodosArrastrados.Clear();
                        return;
                    }
                    
                    dragEnProgreso = true;
                    treeListView.DoDragDrop(nodosArrastrados, DragDropEffects.Move);
                }
            }
        }

        private void TreeListView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(List<NodoTree>)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TreeListView_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(List<NodoTree>)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            Point point = treeListView.PointToClient(new Point(e.X, e.Y));
            OLVListItem targetItem = treeListView.GetItemAt(point.X, point.Y) as OLVListItem;

            if (targetItem != null)
            {
                NodoTree nodoDestino = targetItem.RowObject as NodoTree;
                
                if (nodoDestino != null && nodosArrastrados.Count > 0)
                {
                    // Validar si el drop es permitido para todos los nodos
                    if (TodosDropPermitidos(nodosArrastrados, nodoDestino))
                    {
                        e.Effect = DragDropEffects.Move;

                        // Resaltar el item de destino solo cuando cambia, para no
                        // re-seleccionar ni repintar el árbol virtual en cada DragOver.
                        if (!ReferenceEquals(nodoDestinoResaltado, nodoDestino))
                        {
                            nodoDestinoResaltado = nodoDestino;
                            treeListView.SelectedObject = nodoDestino;
                        }
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TreeListView_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(List<NodoTree>)))
                return;

            Point point = treeListView.PointToClient(new Point(e.X, e.Y));
            OLVListItem targetItem = treeListView.GetItemAt(point.X, point.Y) as OLVListItem;

            if (targetItem == null)
                return;

            NodoTree nodoDestino = targetItem.RowObject as NodoTree;

            if (nodoDestino == null || nodosArrastrados.Count == 0)
                return;

            if (!TodosDropPermitidos(nodosArrastrados, nodoDestino))
            {
                MessageBox.Show(
                    "No se puede realizar esta operación.\n\n" +
                    "Reglas:\n" +
                    "• Un Padre puede convertirse en Sub-Padre de otro Padre\n" +
                    "• Un Sub-Padre puede convertirse en Hijo de otro Sub-Padre\n" +
                    "• No puedes arrastrar un nodo sobre sí mismo o sobre sus descendientes\n" +
                    "• Todos los nodos deben cumplir las mismas reglas",
                    "Operación no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            RealizarDragDropMultiple(nodosArrastrados, nodoDestino);

            dragEnProgreso = false;
            nodosArrastrados.Clear();
            nodoDestinoResaltado = null;
        }

        private void TreeListView_DragLeave(object sender, EventArgs e)
        {
            dragEnProgreso = false;
            nodoDestinoResaltado = null;
        }

        private bool TodosMismoNivel(List<NodoTree> nodos)
        {
            if (nodos.Count == 0) return true;
            
            int nivel = nodos[0].Nivel;
            return nodos.All(n => n.Nivel == nivel);
        }

        private bool TodosHermanos(List<NodoTree> nodos)
        {
            if (nodos.Count == 0) return true;
            
            int? parentId = nodos[0].ParentID;
            return nodos.All(n => n.ParentID == parentId);
        }

        private bool TodosDropPermitidos(List<NodoTree> origenes, NodoTree destino)
        {
            return origenes.All(origen => EsDropPermitido(origen, destino));
        }

        private bool EsDropPermitido(NodoTree origen, NodoTree destino)
        {
            // No se puede arrastrar sobre s� mismo
            if (origen.ID == destino.ID)
                return false;

            // No se puede arrastrar sobre un descendiente
            if (EsDescendiente(destino, origen))
                return false;

            // Regla 1: Padre (nivel 0) -> Otro Padre (nivel 0) = Se convierte en Sub-Padre
            if (origen.Nivel == 0 && destino.Nivel == 0)
                return true;

            // Regla 2: Sub-Padre (nivel 1) -> Otro Sub-Padre (nivel 1) = Se convierte en Hijo
            if (origen.Nivel == 1 && destino.Nivel == 1)
                return true;

            // Regla 3: Hijo (nivel 2) -> Sub-Padre (nivel 1) = Cambio de padre
            if (origen.Nivel == 2 && destino.Nivel == 1)
                return true;

            // Regla 4: Sub-Padre (nivel 1) -> Padre (nivel 0) = Cambio de padre
            if (origen.Nivel == 1 && destino.Nivel == 0)
                return true;

            return false;
        }

        private bool EsDescendiente(NodoTree posibleDescendiente, NodoTree ancestro)
        {
            foreach (var hijo in ancestro.Hijos)
            {
                if (hijo.ID == posibleDescendiente.ID)
                    return true;
                
                if (EsDescendiente(posibleDescendiente, hijo))
                    return true;
            }
            
            return false;
        }

        private void RealizarDragDropMultiple(List<NodoTree> origenes, NodoTree destino)
        {
            if (origenes.Count == 0) return;
            
            int nodosMovidos = 0;
            List<string> nombresMovidos = new List<string>();
            
            // Ordenar los nodos por su orden original para mantener la secuencia
            var nodosOrdenados = origenes.OrderBy(n => n.Orden).ToList();
            
            foreach (var origen in nodosOrdenados)
            {
                try
                {
                    // Guardar informaci�n antes de mover
                    NodoTree padreOriginal = origen.ParentID.HasValue ? BuscarNodoPorId(origen.ParentID.Value) : null;
                    List<NodoTree> hermanosOriginales = ObtenerHermanos(origen);

                    // Remover del padre original
                    if (padreOriginal != null)
                    {
                        padreOriginal.EliminarHijo(origen);
                    }
                    else
                    {
                        nodosRaiz.Remove(origen);
                    }

                    // Actualizar �rdenes de los hermanos originales
                    for (int i = 0; i < hermanosOriginales.Count; i++)
                    {
                        hermanosOriginales[i].Orden = i;
                    }

                    // Determinar el nuevo nivel y padre seg�n las reglas
                    if (origen.Nivel == 0 && destino.Nivel == 0)
                    {
                        // Padre -> Padre = Se convierte en Sub-Padre del destino
                        CambiarNodoASubPadre(origen, destino);
                    }
                    else if (origen.Nivel == 1 && destino.Nivel == 1)
                    {
                        // Sub-Padre -> Sub-Padre = Se convierte en Hijo del destino
                        CambiarNodoAHijo(origen, destino);
                    }
                    else if (origen.Nivel == 1 && destino.Nivel == 0)
                    {
                        // Sub-Padre -> Padre = Cambio de padre, sigue siendo Sub-Padre
                        CambiarPadre(origen, destino, 1);
                    }
                    else if (origen.Nivel == 2 && destino.Nivel == 1)
                    {
                        // Hijo -> Sub-Padre = Cambio de padre, sigue siendo Hijo
                        CambiarPadre(origen, destino, 2);
                    }
                    
                    nodosMovidos++;
                    nombresMovidos.Add(origen.Nombre);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al mover el nodo '{origen.Nombre}':\n\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            // Marcar cambios pendientes y actualizar vista
            if (nodosMovidos > 0)
            {
                cambiosPendientes = true;
                ActualizarTreeListView();
                ActualizarEstadoBotones();

                // Expandir el nodo destino para mostrar los nuevos hijos
                treeListView.Expand(destino);
                
                // Seleccionar los nodos movidos
                treeListView.SelectedObjects = nodosOrdenados;

                string mensajeNodos = nodosMovidos == 1 
                    ? $"'{nombresMovidos[0]}'" 
                    : $"{nodosMovidos} nodos:\n� " + string.Join("\n� ", nombresMovidos);

                MessageBox.Show(
                    $"? Operaci�n completada exitosamente.\n\n" +
                    $"{mensajeNodos}\n\n" +
                    $"ahora son {nodosOrdenados[0].TipoNodo} de '{destino.Nombre}'",
                    "�xito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void CambiarNodoASubPadre(NodoTree nodo, NodoTree nuevoPadre)
        {
            // Cambiar nivel del nodo y todos sus descendientes
            AjustarNivelesRecursivo(nodo, 1);
            
            nodo.ParentID = nuevoPadre.ID;
            nodo.Orden = nuevoPadre.Hijos.Count;
            nodo.FechaModificacion = DateTime.Now;
            
            nuevoPadre.AgregarHijo(nodo);
        }

        private void CambiarNodoAHijo(NodoTree nodo, NodoTree nuevoPadre)
        {
            // Cambiar nivel del nodo y todos sus descendientes
            AjustarNivelesRecursivo(nodo, 2);
            
            nodo.ParentID = nuevoPadre.ID;
            nodo.Orden = nuevoPadre.Hijos.Count;
            nodo.FechaModificacion = DateTime.Now;
            
            nuevoPadre.AgregarHijo(nodo);
        }

        private void CambiarPadre(NodoTree nodo, NodoTree nuevoPadre, int nuevoNivel)
        {
            // Cambiar nivel del nodo y todos sus descendientes
            AjustarNivelesRecursivo(nodo, nuevoNivel);
            
            nodo.ParentID = nuevoPadre.ID;
            nodo.Orden = nuevoPadre.Hijos.Count;
            nodo.FechaModificacion = DateTime.Now;
            
            nuevoPadre.AgregarHijo(nodo);
        }

        private void AjustarNivelesRecursivo(NodoTree nodo, int nuevoNivel)
        {
            int diferenciaNivel = nuevoNivel - nodo.Nivel;
            nodo.Nivel = nuevoNivel;
            
            // Ajustar niveles de todos los hijos recursivamente
            foreach (var hijo in nodo.Hijos)
            {
                AjustarNivelesRecursivo(hijo, hijo.Nivel + diferenciaNivel);
            }
        }

        #endregion
        
        #region Carga de datos

        private void CargarDatosIniciales()
        {
            try
            {
                CargarColumnasPersonalizadas();
                CargarNodos();
                ActualizarTreeListView();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarColumnasPersonalizadas()
        {
            columnasPersonalizadas.Clear();
            
            // Limpiar columnas personalizadas existentes del TreeView
            var columnasBase = treeListView.AllColumns.Count;
            while (treeListView.AllColumns.Count > 6) // Mantener solo las 6 columnas base (# Destajo, Nombre, Tipo, Tipo Tarea, Descripci�n, Orden)
            {
                treeListView.AllColumns.RemoveAt(6);
            }
            
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    string sql = $@"
                        SELECT Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato,
                               EsCalculada, TipoOperacion, ColumnaOrigen1, ColumnaOrigen2
                        FROM {nombreTabla}_ColumnasDefinicion
                        ORDER BY ID";
                    
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var columna = new ColumnaTreeList
                            {
                                Nombre = reader["Nombre"].ToString(),
                                Titulo = reader["Titulo"].ToString(),
                                Ancho = Convert.ToInt32(reader["Ancho"]),
                                TipoDato = Type.GetType(reader["TipoDato"].ToString()) ?? typeof(string),
                                EsEditable = Convert.ToBoolean(reader["EsEditable"]),
                                Formato = reader["Formato"] != DBNull.Value ? reader["Formato"].ToString() : null,
                                EsCalculada = reader["EsCalculada"] != DBNull.Value ? Convert.ToBoolean(reader["EsCalculada"]) : false,
                                TipoOperacion = reader["TipoOperacion"] != DBNull.Value ? (TipoOperacion)Convert.ToInt32(reader["TipoOperacion"]) : TipoOperacion.Ninguna,
                                ColumnaOrigen1 = reader["ColumnaOrigen1"] != DBNull.Value ? reader["ColumnaOrigen1"].ToString() : null,
                                ColumnaOrigen2 = reader["ColumnaOrigen2"] != DBNull.Value ? reader["ColumnaOrigen2"].ToString() : null
                            };
                            
                            columnasPersonalizadas.Add(columna);
                            AgregarColumnaAlTreeView(columna);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Si la tabla no existe, mostrar mensaje informativo
                if (ex.Message.Contains("Invalid object name"))
                {
                    MessageBox.Show(
                        $"La tabla '{nombreTabla}_ColumnasDefinicion' no existe en la base de datos.\n\n" +
                        "Por favor, ejecuta el script SQL correspondiente:\n" +
                        "- Para RutaTuneraDestajo: SQL_SCRIPTS\\CrearTablasRutaCalandraDestajo.sql\n" +
                        "- Para RutaCalandraDestajo: SQL_SCRIPTS\\CrearTablasRutaCalandraDestajo.sql",
                        "Tabla no encontrada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else if (ex.Message.Contains("Invalid column name"))
                {
                    MessageBox.Show(
                        $"La tabla '{nombreTabla}_ColumnasDefinicion' necesita ser actualizada.\n\n" +
                        "Por favor, ejecuta el script SQL para agregar los campos de columnas calculadas:\n" +
                        "SQL_SCRIPTS\\AgregarCamposColumnasCalculadas.sql",
                        "Tabla desactualizada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    throw;
                }
            }
            
            treeListView.RebuildColumns();
        }

        private void AgregarColumnaAlTreeView(ColumnaTreeList columna)
        {
            var col = new OLVColumn(columna.Titulo, null)
            {
                Width = columna.Ancho,
                IsEditable = columna.EsCalculada ? false : columna.EsEditable, // Las columnas calculadas no son editables
                AspectGetter = delegate(object x)
                {
                    var nodo = x as NodoTree;
                    if (nodo == null) return null;
                    
                    // Si es una columna calculada, calcular el valor
                    if (columna.EsCalculada)
                    {
                        return nodo.CalcularValorColumna(columna);
                    }
                    
                    // Si no, obtener el valor normal
                    return nodo.ObtenerValorColumna(columna.Nombre);
                },
                AspectPutter = delegate(object x, object newValue)
                {
                    var nodo = x as NodoTree;
                    if (nodo != null && !columna.EsCalculada) // Solo establecer si no es calculada
                    {
                        // Validar/normalizar antes de guardar. Si la entrada no es válida
                        // para una columna numérica, se rechaza y se conserva el valor
                        // anterior (no se ensucia el dato ni se rompen los cálculos).
                        if (!ValidarYNormalizarValor(columna, newValue, out object valorFinal))
                            return;

                        nodo.EstablecerValorColumna(columna.Nombre, valorFinal);
                        cambiosPendientes = true;
                        ActualizarEstadoBotones();

                        // Refrescar todas las columnas calculadas que dependan de esta
                        RefrescarColumnasCalculadasDependientes(columna.Nombre);
                    }
                }
            };
            
            if (!string.IsNullOrEmpty(columna.Formato))
            {
                col.AspectToStringFormat = columna.Formato;
            }
            
            treeListView.AllColumns.Add(col);
            treeListView.RebuildColumns();
        }

        /// <summary>
        /// Indica si una columna almacena valores numéricos según su TipoDato.
        /// </summary>
        private static bool EsColumnaNumerica(ColumnaTreeList columna)
        {
            var t = columna?.TipoDato;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float)
                || t == typeof(int) || t == typeof(long);
        }

        /// <summary>
        /// Valida y normaliza el valor tecleado en una celda antes de guardarlo en el
        /// modelo. Para columnas numéricas: rechaza texto no numérico (avisa al
        /// usuario) y guarda el número normalizado con punto decimal invariante, de
        /// modo que el guardado nunca mezcle tipos y los cálculos no fallen. Para
        /// columnas de texto deja el valor tal cual (vacío => null para limpiar).
        /// Devuelve false si la entrada es inválida (no se debe guardar).
        /// </summary>
        private bool ValidarYNormalizarValor(ColumnaTreeList columna, object valor, out object normalizado)
        {
            string texto = valor?.ToString()?.Trim();

            // Celda vacía: se permite y limpia el valor.
            if (string.IsNullOrEmpty(texto))
            {
                normalizado = null;
                return true;
            }

            if (EsColumnaNumerica(columna))
            {
                // Acepta separador de miles y punto decimal (formato invariante,
                // p.ej. "1,731.81" o "95.68035"). Guarda sin separadores: "1731.81".
                if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal d)
                    && !decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out d))
                {
                    MessageBox.Show(
                        $"\"{texto}\" no es un número válido para la columna \"{columna.Titulo}\".\n\n" +
                        "Escribe solo dígitos y, si aplica, un punto decimal.",
                        "Valor inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    normalizado = null;
                    return false;
                }

                normalizado = d.ToString(CultureInfo.InvariantCulture);
                return true;
            }

            normalizado = texto;
            return true;
        }

        /// <summary>
        /// Refresca todas las columnas calculadas que dependen de una columna específica.
        /// Esta es la versión correcta que funciona con ObjectListView + NodoTree.
        /// </summary>
        private void RefrescarColumnasCalculadasDependientes(string nombreColumna)
        {
            if (string.IsNullOrWhiteSpace(nombreColumna))
                return;

            // Buscar columnas calculadas que usen esta columna como origen
            var columnasQueDependenDe = columnasPersonalizadas
                .Where(col => col.EsCalculada && 
                    (col.ColumnaOrigen1 == nombreColumna || col.ColumnaOrigen2 == nombreColumna))
                .ToList();

            // Si hay columnas calculadas dependientes, refrescar todo el árbol
            if (columnasQueDependenDe.Count > 0)
            {
                // RefreshObjects recalcula los AspectGetter de TODAS las columnas
                // (tanto calculadas como normales), lo que dispara CalcularValorColumna()
                var todosLosObjetos = treeListView.Objects.Cast<object>().ToList();
                treeListView.RefreshObjects(todosLosObjetos);
            }
        }

        /// <summary>
        /// Carga los nodos de la base de datos a la lista de nodos en memoria.
        /// Se llama al iniciar y al cambiar de tabla.
        /// </summary>
        private void CargarNodos()
        {
            nodosRaiz.Clear();
            var todosLosNodos = new Dictionary<int, NodoTree>();
            
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                
                string sql = $@"
                    SELECT ID, ParentID, Nombre, Descripcion, Orden, Nivel,
                           FechaCreacion, FechaModificacion, UsuarioCreacion, TipoTarea
                    FROM {nombreTabla}
                    ORDER BY Nivel, Orden";
                
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var nodo = new NodoTree
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            ParentID = reader["ParentID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ParentID"]) : null,
                            Nombre = reader["Nombre"].ToString(),
                            Descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : null,
                            Orden = Convert.ToInt32(reader["Orden"]),
                            Nivel = Convert.ToInt32(reader["Nivel"]),
                            FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                            FechaModificacion = reader["FechaModificacion"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["FechaModificacion"]) : null,
                            UsuarioCreacion = reader["UsuarioCreacion"] != DBNull.Value ? reader["UsuarioCreacion"].ToString() : null,
                            TipoTarea = reader["TipoTarea"] != DBNull.Value ? (TipoTarea)Convert.ToInt32(reader["TipoTarea"]) : TipoTarea.Ninguno
                        };
                        
                        todosLosNodos[nodo.ID] = nodo;
                        
                        if (nodo.ID >= siguienteId)
                            siguienteId = nodo.ID + 1;
                    }
                }
                
                sql = $@"SELECT NodoID, NombreColumna, Valor FROM {nombreTabla}_Columnas";
                
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int nodoId = Convert.ToInt32(reader["NodoID"]);
                        string columna = reader["NombreColumna"].ToString();
                        object valor = reader["Valor"] != DBNull.Value ? reader["Valor"] : null;
                        
                        if (todosLosNodos.ContainsKey(nodoId))
                        {
                            todosLosNodos[nodoId].EstablecerValorColumna(columna, valor);
                        }
                    }
                }
            }
            
            foreach (var nodo in todosLosNodos.Values)
            {
                if (nodo.ParentID == null)
                {
                    nodosRaiz.Add(nodo);
                }
                else if (todosLosNodos.ContainsKey(nodo.ParentID.Value))
                {
                    todosLosNodos[nodo.ParentID.Value].AgregarHijo(nodo);
                }
            }
        }

        private void ActualizarTreeListView()
        {
            // 1) Capturar qu� nodos est�n expandidos. Tomamos una copia (ToList) de
            //    treeListView.Objects porque a continuaci�n vamos a reconstruir el
            //    �rbol, y NUNCA debemos enumerar la colecci�n viva del control
            //    mientras la mutamos (eso desincroniza el conteo de items virtuales
            //    y provoca IndexOutOfRange en OnRetrieveVirtualItem).
            var nodosExpandidos = new HashSet<int>();
            foreach (var obj in treeListView.Objects.Cast<object>().ToList())
            {
                var nodo = obj as NodoTree;
                if (nodo != null && treeListView.IsExpanded(nodo))
                    nodosExpandidos.Add(nodo.ID);
            }

            // 2) Reconstruir y restaurar la expansi�n dentro de BeginUpdate/EndUpdate
            //    para que el control no repinte (ni dispare RetrieveVirtualItem) en
            //    un estado intermedio inconsistente.
            treeListView.BeginUpdate();
            try
            {
                treeListView.Roots = nodosRaiz;

                // Restaurar la expansi�n recorriendo el MODELO de arriba hacia abajo
                // (un padre se expande antes que sus hijos), sin enumerar la
                // colecci�n viva del TreeListView mientras llamamos a Expand().
                RestaurarExpansionDesdeModelo(nodosRaiz, nodosExpandidos);
            }
            finally
            {
                treeListView.EndUpdate();
            }
        }

        private void RestaurarExpansionDesdeModelo(IEnumerable<NodoTree> nodos, HashSet<int> nodosExpandidos)
        {
            foreach (var nodo in nodos)
            {
                if (nodosExpandidos.Contains(nodo.ID))
                    treeListView.Expand(nodo);

                if (nodo.TieneHijos)
                    RestaurarExpansionDesdeModelo(nodo.Hijos, nodosExpandidos);
            }
        }

        #endregion

        #region Gesti�n de nodos

        private void btnAgregarPadre_Click(object sender, EventArgs e)
        {
            using (var dialog = new DialogAgregarNodo("Agregar Padre"))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var nuevoPadre = new NodoTree
                    {
                        ID = siguienteId++,
                        EsNuevo = true,
                        Nombre = dialog.NombreNodo,
                        Descripcion = dialog.DescripcionNodo,
                        Nivel = 0,
                        Orden = nodosRaiz.Count,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = Global.UsuarioActual?.Nombre ?? "Sistema"
                    };
                    
                    nodosRaiz.Add(nuevoPadre);
                    ActualizarTreeListView();
                    
                    cambiosPendientes = true;
                    ActualizarEstadoBotones();
                }
            }
        }

        private void btnAgregarSubPadre_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = treeListView.SelectedObject as NodoTree;
            
            if (nodoSeleccionado == null || nodoSeleccionado.Nivel != 0)
            {
                MessageBox.Show("Por favor, selecciona un nodo PADRE para agregar un sub-padre.",
                    "Selecci�n requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            using (var dialog = new DialogAgregarNodo("Agregar Sub-Padre"))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var nuevoSubPadre = new NodoTree
                    {
                        ID = siguienteId++,
                        EsNuevo = true,
                        Nombre = dialog.NombreNodo,
                        Descripcion = dialog.DescripcionNodo,
                        Nivel = 1,
                        ParentID = nodoSeleccionado.ID,
                        Orden = nodoSeleccionado.Hijos.Count,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = Global.UsuarioActual?.Nombre ?? "Sistema"
                    };
                    
                    nodoSeleccionado.AgregarHijo(nuevoSubPadre);
                    treeListView.RefreshObject(nodoSeleccionado);
                    treeListView.Expand(nodoSeleccionado);
                    
                    cambiosPendientes = true;
                    ActualizarEstadoBotones();
                }
            }
        }

        private void btnAgregarHijo_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = treeListView.SelectedObject as NodoTree;
            
            if (nodoSeleccionado == null || nodoSeleccionado.Nivel != 1)
            {
                MessageBox.Show("Por favor, selecciona un nodo SUB-PADRE para agregar un hijo.",
                    "Selecci�n requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            using (var dialog = new DialogAgregarNodo("Agregar Hijo"))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var nuevoHijo = new NodoTree
                    {
                        ID = siguienteId++,
                        EsNuevo = true,
                        Nombre = dialog.NombreNodo,
                        Descripcion = dialog.DescripcionNodo,
                        Nivel = 2,
                        ParentID = nodoSeleccionado.ID,
                        Orden = nodoSeleccionado.Hijos.Count,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = Global.UsuarioActual?.Nombre ?? "Sistema"
                    };
                    
                    nodoSeleccionado.AgregarHijo(nuevoHijo);
                    treeListView.RefreshObject(nodoSeleccionado);
                    treeListView.Expand(nodoSeleccionado);
                    
                    cambiosPendientes = true;
                    ActualizarEstadoBotones();
                }
            }
        }

        private void btnRenombrar_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = treeListView.SelectedObject as NodoTree;
            
            if (nodoSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un nodo para renombrar.",
                    "Selecci�n requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            using (var dialog = new DialogAgregarNodo($"Renombrar {nodoSeleccionado.TipoNodo}"))
            {
                dialog.NombreNodo = nodoSeleccionado.Nombre;
                dialog.DescripcionNodo = nodoSeleccionado.Descripcion;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    nodoSeleccionado.Nombre = dialog.NombreNodo;
                    nodoSeleccionado.Descripcion = dialog.DescripcionNodo;
                    nodoSeleccionado.FechaModificacion = DateTime.Now;
                    
                    treeListView.RefreshObject(nodoSeleccionado);
                    
                    cambiosPendientes = true;
                    ActualizarEstadoBotones();
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            // Obtener todos los nodos seleccionados
            var nodosSeleccionados = new List<NodoTree>();
            foreach (var obj in treeListView.SelectedObjects)
            {
                var nodo = obj as NodoTree;
                if (nodo != null)
                {
                    nodosSeleccionados.Add(nodo);
                }
            }
            
            if (nodosSeleccionados.Count == 0)
            {
                return;
            }
            
            // Eliminar m�ltiples nodos
            if (nodosSeleccionados.Count > 1)
            {
                EliminarNodosMultiples(nodosSeleccionados);
            }
            else
            {
                // Eliminar un solo nodo (comportamiento original)
                EliminarNodoIndividual(nodosSeleccionados[0]);
            }
        }

        private void EliminarNodoIndividual(NodoTree nodoSeleccionado)
        {
            if (nodoSeleccionado.ParentID == null)
            {
                nodosRaiz.Remove(nodoSeleccionado);
            }
            else
            {
                var padre = BuscarNodoPorId(nodoSeleccionado.ParentID.Value);
                if (padre != null)
                {
                    padre.EliminarHijo(nodoSeleccionado);
                }
            }
            
            ActualizarTreeListView();
            
            cambiosPendientes = true;
            ActualizarEstadoBotones();
        }

        private void EliminarNodosMultiples(List<NodoTree> nodosSeleccionados)
        {
            // Validar que no se est�n eliminando nodos padre-hijo
            if (ContieneNodoYDescendiente(nodosSeleccionados))
            {
                return;
            }
            
            // Realizar eliminaci�n
            foreach (var nodo in nodosSeleccionados)
            {
                try
                {
                    if (nodo.ParentID == null)
                    {
                        nodosRaiz.Remove(nodo);
                    }
                    else
                    {
                        var padre = BuscarNodoPorId(nodo.ParentID.Value);
                        if (padre != null)
                        {
                            padre.EliminarHijo(nodo);
                        }
                    }
                }
                catch
                {
                    // Ignorar errores y continuar con el siguiente nodo
                }
            }
            
            // Actualizar vista
            ActualizarTreeListView();
            cambiosPendientes = true;
            ActualizarEstadoBotones();
        }

        private bool ContieneNodoYDescendiente(List<NodoTree> nodos)
        {
            // Verificar si alg�n nodo de la lista es descendiente de otro nodo de la lista
            for (int i = 0; i < nodos.Count; i++)
            {
                for (int j = 0; j < nodos.Count; j++)
                {
                    if (i != j)
                    {
                        // Verificar si nodos[j] es descendiente de nodos[i]
                        if (EsDescendiente(nodos[j], nodos[i]))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        #endregion

        #region Reordenamiento

        private void btnSubir_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = treeListView.SelectedObject as NodoTree;
            
            if (nodoSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un nodo para mover.",
                    "Selecci�n requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            List<NodoTree> hermanos = ObtenerHermanos(nodoSeleccionado);
            int indice = hermanos.IndexOf(nodoSeleccionado);
            
            if (indice > 0)
            {
                hermanos[indice].Orden = indice - 1;
                hermanos[indice - 1].Orden = indice;
                
                hermanos.Reverse(indice - 1, 2);
                
                ActualizarTreeListView();
                treeListView.SelectedObject = nodoSeleccionado;
                
                cambiosPendientes = true;
                ActualizarEstadoBotones();
            }
        }

        private void btnBajar_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = treeListView.SelectedObject as NodoTree;
            
            if (nodoSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un nodo para mover.",
                    "Selecci�n requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            List<NodoTree> hermanos = ObtenerHermanos(nodoSeleccionado);
            int indice = hermanos.IndexOf(nodoSeleccionado);
            
            if (indice < hermanos.Count - 1)
            {
                hermanos[indice].Orden = indice + 1;
                hermanos[indice + 1].Orden = indice;
                
                hermanos.Reverse(indice, 2);
                
                ActualizarTreeListView();
                treeListView.SelectedObject = nodoSeleccionado;
                
                cambiosPendientes = true;
                ActualizarEstadoBotones();
            }
        }

        /// <summary>
        /// Subir un nodo un nivel en la jerarqu�a (junto con todo su contenido)
        /// </summary>
        private void btnSubirJerarquia_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = treeListView.SelectedObject as NodoTree;
            
            if (nodoSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un nodo para mover de jerarqu�a.",
                    "Selecci�n requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Validar que no sea un nodo ra�z (nivel 0)
            if (nodoSeleccionado.Nivel == 0)
            {
                MessageBox.Show("No se puede subir de jerarqu�a un nodo PADRE (nivel 0).",
                    "Operaci�n no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Obtener el nodo padre
            NodoTree nodoPadre = nodoSeleccionado.ParentID.HasValue 
                ? BuscarNodoPorId(nodoSeleccionado.ParentID.Value) 
                : null;
            
            if (nodoPadre == null)
            {
                MessageBox.Show("No se puede obtener el nodo padre.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Obtener el abuelo (padre del padre)
            NodoTree nodoAbuelo = nodoPadre.ParentID.HasValue 
                ? BuscarNodoPorId(nodoPadre.ParentID.Value) 
                : null;
            
            if (nodoAbuelo == null && nodoPadre.Nivel != 0)
            {
                MessageBox.Show("No se puede subir m�s de jerarqu�a.",
                    "Operaci�n no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                // Guardar informaci�n antes de mover
                var hermanosOriginales = ObtenerHermanos(nodoSeleccionado);
                
                // Remover del padre original
                nodoPadre.EliminarHijo(nodoSeleccionado);
                
                // Actualizar �rdenes de los hermanos originales
                for (int i = 0; i < hermanosOriginales.Count; i++)
                {
                    hermanosOriginales[i].Orden = i;
                }
                
                // Cambiar nivel del nodo y todos sus descendientes
                AjustarNivelesRecursivo(nodoSeleccionado, nodoSeleccionado.Nivel - 1);
                
                // Agregar como hermano del padre original
                if (nodoAbuelo != null)
                {
                    nodoSeleccionado.ParentID = nodoAbuelo.ID;
                    nodoSeleccionado.Orden = nodoAbuelo.Hijos.Count;
                    nodoAbuelo.AgregarHijo(nodoSeleccionado);
                }
                else
                {
                    // Si el padre es ra�z, agregar a ra�z
                    nodoSeleccionado.ParentID = null;
                    nodoSeleccionado.Orden = nodosRaiz.Count;
                    nodosRaiz.Add(nodoSeleccionado);
                }
                
                nodoSeleccionado.FechaModificacion = DateTime.Now;
                
                cambiosPendientes = true;
                ActualizarTreeListView();
                treeListView.SelectedObject = nodoSeleccionado;
                ActualizarEstadoBotones();
                
                MessageBox.Show(
                    $"? '{nodoSeleccionado.Nombre}' ha subido de jerarqu�a correctamente.\n\n" +
                    $"Ahora es {nodoSeleccionado.TipoNodo} en lugar de {nodoSeleccionado.TipoNodo}",
                    "�xito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al subir de jerarqu�a:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Bajar un nodo un nivel en la jerarqu�a (junto con todo su contenido)
        /// </summary>
        private void btnBajarJerarquia_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = treeListView.SelectedObject as NodoTree;
            
            if (nodoSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un nodo para mover de jerarqu�a.",
                    "Selecci�n requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Validar que no sea un nodo hijo (nivel 2)
            if (nodoSeleccionado.Nivel >= 2)
            {
                MessageBox.Show("No se puede bajar de jerarqu�a un nodo HIJO (nivel 2).",
                    "Operaci�n no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Obtener los hermanos
            List<NodoTree> hermanos = ObtenerHermanos(nodoSeleccionado);
            int indiceActual = hermanos.IndexOf(nodoSeleccionado);
            
            // Buscar el hermano anterior que existir� como nuevo padre
            NodoTree nuevopadre = null;
            
            if (indiceActual > 0)
            {
                // El nuevo padre es el hermano anterior
                nuevopadre = hermanos[indiceActual - 1];
            }
            else
            {
                MessageBox.Show("No hay un hermano anterior para usar como nuevo padre.\n" +
                    "El nodo debe estar precedido por otro nodo del mismo nivel para poder bajar de jerarqu�a.",
                    "Operaci�n no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                // Obtener el padre actual
                NodoTree padrePadre = nodoSeleccionado.ParentID.HasValue 
                    ? BuscarNodoPorId(nodoSeleccionado.ParentID.Value) 
                    : null;
                
                // Remover del padre original
                if (padrePadre != null)
                {
                    padrePadre.EliminarHijo(nodoSeleccionado);
                }
                else
                {
                    nodosRaiz.Remove(nodoSeleccionado);
                }
                
                // Actualizar �rdenes de los hermanos
                for (int i = 0; i < hermanos.Count; i++)
                {
                    hermanos[i].Orden = i;
                }
                
                // Cambiar nivel del nodo y todos sus descendientes
                AjustarNivelesRecursivo(nodoSeleccionado, nodoSeleccionado.Nivel + 1);
                
                // Agregar como hijo del nuevo padre
                nodoSeleccionado.ParentID = nuevopadre.ID;
                nodoSeleccionado.Orden = nuevopadre.Hijos.Count;
                nuevopadre.AgregarHijo(nodoSeleccionado);
                nodoSeleccionado.FechaModificacion = DateTime.Now;
                
                cambiosPendientes = true;
                ActualizarTreeListView();
                treeListView.SelectedObject = nodoSeleccionado;
                treeListView.Expand(nuevopadre);
                ActualizarEstadoBotones();
                
                MessageBox.Show(
                    $"? '{nodoSeleccionado.Nombre}' ha bajado de jerarqu�a correctamente.\n\n" +
                    $"Ahora es {nodoSeleccionado.TipoNodo} de '{nuevopadre.Nombre}'",
                    "�xito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al bajar de jerarqu�a:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Convierte el nodo seleccionado (Sub-Padre, nivel 1) en un nodo Padre raiz
        /// (nivel 0), subiendo todo su subarbol un nivel. A diferencia de
        /// "Subir Jerarquia", funciona aunque el Sub-Padre haya quedado huerfano
        /// (Nivel=1 con ParentID=null), que es el estado en el que no habia forma
        /// de regresar un Padre que se convirtio en Sub-Padre por accidente.
        /// </summary>
        private void btnConvertirAPadre_Click(object sender, EventArgs e)
        {
            var nodo = treeListView.SelectedObject as NodoTree;

            if (nodo == null)
            {
                MessageBox.Show("Por favor, selecciona un nodo para convertir a Padre.",
                    "Seleccion requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nodo.Nivel == 0)
            {
                MessageBox.Show("El nodo seleccionado ya es un Padre (nivel raiz).",
                    "Sin cambios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (nodo.Nivel != 1)
            {
                MessageBox.Show(
                    "Solo un Sub-Padre puede convertirse directamente en Padre.\n\n" +
                    "Para un nodo Hijo, primero subelo a Sub-Padre con 'Subir'.",
                    "Operacion no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmar = MessageBox.Show(
                $"Convertir '{nodo.Nombre}' en un nodo Padre (raiz)?\n\n" +
                "Subira un nivel junto con todo su contenido.",
                "Convertir a Padre", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmar != DialogResult.Yes)
                return;

            try
            {
                // Quitar del padre actual (o de la raiz si quedo huerfano con Nivel 1)
                var padreActual = nodo.ParentID.HasValue ? BuscarNodoPorId(nodo.ParentID.Value) : null;
                var hermanosOriginales = ObtenerHermanos(nodo);

                if (padreActual != null)
                    padreActual.EliminarHijo(nodo);
                else
                    nodosRaiz.Remove(nodo);

                // Reindexar el orden de los hermanos que se quedaron
                for (int i = 0; i < hermanosOriginales.Count; i++)
                    hermanosOriginales[i].Orden = i;

                // Subir el nodo y todo su subarbol un nivel (Sub-Padres -> Sub-Padre,
                // Hijos -> Hijo, etc.; AjustarNivelesRecursivo respeta las diferencias)
                AjustarNivelesRecursivo(nodo, 0);

                // Insertarlo como nodo raiz
                nodo.ParentID = null;
                nodo.Orden = nodosRaiz.Count;
                nodo.FechaModificacion = DateTime.Now;
                nodosRaiz.Add(nodo);

                cambiosPendientes = true;
                ActualizarTreeListView();
                treeListView.SelectedObject = nodo;
                treeListView.Expand(nodo);
                ActualizarEstadoBotones();

                MessageBox.Show(
                    $"'{nodo.Nombre}' ahora es un nodo Padre.",
                    "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al convertir a Padre:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<NodoTree> ObtenerHermanos(NodoTree nodo)
        {
            if (nodo.ParentID == null)
            {
                return nodosRaiz;
            }
            else
            {
                var padre = BuscarNodoPorId(nodo.ParentID.Value);
                return padre?.Hijos ?? new List<NodoTree>();
            }
        }

        #endregion

        #region Gesti�n de columnas

        private void btnAsignarTipo_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = treeListView.SelectedObject as NodoTree;
            
            if (nodoSeleccionado == null || nodoSeleccionado.Nivel != 2)
            {
                MessageBox.Show("Por favor, selecciona un nodo HIJO para asignar un tipo de tarea.",
                    "Selecci�n requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            using (var dialog = new DialogAsignarTipoTarea(nodoSeleccionado.TipoTarea))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    nodoSeleccionado.TipoTarea = dialog.TipoTareaSeleccionado;
                    nodoSeleccionado.FechaModificacion = DateTime.Now;
                    
                    treeListView.RefreshObject(nodoSeleccionado);
                    
                    cambiosPendientes = true;
                    ActualizarEstadoBotones();
                }
            }
        }

        private void btnGestionarColumnas_Click(object sender, EventArgs e)
        {
            using (var dialog = new DialogGestionarColumnas(columnasPersonalizadas, connectionString, nombreTabla))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    CargarColumnasPersonalizadas();
                    cambiosPendientes = true;
                    ActualizarEstadoBotones();
                }
            }
        }

        /// <summary>
        /// Abre el gestor central de insumos. Agrupa todos los nodos hoja (insumos)
        /// por nombre y permite editar sus columnas (precio/unidad/etc.) una sola vez;
        /// los cambios se propagan a TODAS las apariciones del insumo en el árbol.
        /// </summary>
        private void btnGestorInsumos_Click(object sender, EventArgs e)
        {
            if (columnasPersonalizadas.Count == 0)
            {
                MessageBox.Show(
                    "No hay columnas personalizadas definidas (precio, unidad, etc.).\n\n" +
                    "Usa primero \"Gestionar Columnas\" para crearlas.",
                    "Sin columnas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new DialogGestorInsumos(nodosRaiz, columnasPersonalizadas))
            {
                dialog.ShowDialog();

                if (dialog.HuboCambios)
                {
                    // Recalcular/refrescar todo el árbol: las columnas calculadas
                    // (p.ej. Total = Precio × Cantidad) se recomputan vía AspectGetter.
                    treeListView.RefreshObjects(treeListView.Objects.Cast<object>().ToList());
                    cambiosPendientes = true;
                    ActualizarEstadoBotones();
                }
            }
        }

        #endregion

        #region Guardado

        /// <summary>
        /// Guarda los cambios del árbol con un upsert por nodo, preservando el
        /// progreso por casa (ActivacionTareasRuta). Antes hacía DELETE+INSERT
        /// masivo, lo cual disparaba el ON DELETE CASCADE del FK de
        /// ActivacionTareasRuta y borraba todas las activaciones/finalizaciones.
        /// Ahora solo elimina nodos que el usuario realmente removió.
        /// </summary>
        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            await GuardarCambiosAsync();
        }

        /// <summary>
        /// Orquesta el guardado SIN congelar la UI: toma una instantánea del árbol
        /// en el hilo de UI, deshabilita la interacción y corre el trabajo de BD en
        /// un hilo de fondo (Task.Run). En máquinas lentas esto evita el "no
        /// responde". Devuelve true si se guardó correctamente.
        /// </summary>
        private async Task<bool> GuardarCambiosAsync()
        {
            // Reentrada: si ya hay un guardado en curso, no lanzar otro.
            if (guardando)
                return false;

            // Instantánea en el hilo de UI (NUNCA recorrer el árbol vivo desde el
            // hilo de fondo). Los NodoTree se comparten, pero la UI queda bloqueada.
            var nodosMemoria = AplanarTopDown(nodosRaiz);

            guardando = true;
            var cursorPrevio = this.Cursor;
            var textoPrevio = btnGuardar.Text;
            this.Cursor = Cursors.WaitCursor;
            btnGuardar.Text = "Guardando...";
            EstablecerInteraccion(false);

            try
            {
                await Task.Run(() => GuardarArbolEnDb(nodosMemoria));

                cambiosPendientes = false;
                MessageBox.Show(
                    "Datos guardados correctamente. El progreso por casa se preservó.",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                // Registrar SIEMPRE el detalle completo (mensaje + inner + stack) en el
                // log central para no depender de lo que alcance a leerse en el diálogo.
                ErrorLogger.Registrar(ex, $"FormEditorTreeList.GuardarCambiosAsync [{nombreTabla}]");

                string detalle = ex.Message;
                if (ex.InnerException != null)
                    detalle += "\n\nCausa: " + ex.InnerException.Message;

                MessageBox.Show($"Error al guardar datos:\n\n{detalle}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                guardando = false;
                btnGuardar.Text = textoPrevio;
                EstablecerInteraccion(true);
                this.Cursor = cursorPrevio;
                ActualizarEstadoBotones();
            }
        }

        /// <summary>
        /// Habilita/deshabilita la interacción del usuario durante el guardado.
        /// </summary>
        private void EstablecerInteraccion(bool habilitado)
        {
            panelSidebar.Enabled = habilitado;
            treeListView.Enabled = habilitado;
        }

        /// <summary>
        /// Trabajo de BD del guardado (corre en hilo de fondo). Mantiene el upsert
        /// por nodo de la tabla principal para preservar el progreso por casa
        /// (ActivacionTareasRuta), pero sincroniza las columnas en BLOQUE en vez de
        /// un DELETE+INSERT por cada nodo, que era la mayor fuente de round-trips.
        /// _Columnas no tiene FK saliente hacia ActivacionTareasRuta, así que el
        /// borrado total es seguro (se reinsertan las columnas vigentes en memoria).
        /// </summary>
        private void GuardarArbolEnDb(List<NodoTree> nodosMemoria)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) Leer IDs existentes en DB
                        var idsExistentes = LeerIdsExistentes(conn, transaction);

                        // 1.1) Re-asignar IDs de nodos NUEVOS que choquen con filas que
                        //      otra sesión insertó mientras tanto. Evita que un nodo nuevo
                        //      se confunda con uno existente (sobrescritura) o reviente la
                        //      PK. Debe ir ANTES de calcular idsMemoria y del upsert.
                        ReasignarIdsEnConflicto(nodosMemoria, idsExistentes);

                        // 1.2) Guardia: ningún ID duplicado en memoria (el árbol se
                        //      corrompería: un nodo pisaría a otro en la tabla).
                        var dup = nodosMemoria.GroupBy(n => n.ID).FirstOrDefault(g => g.Count() > 1);
                        if (dup != null)
                            throw new InvalidOperationException(
                                $"Hay nodos con el mismo ID={dup.Key} en memoria " +
                                $"('{string.Join("', '", dup.Select(n => n.Nombre))}'). " +
                                "No se guardó para no corromper el árbol.");

                        var idsMemoria = new HashSet<int>(nodosMemoria.Select(n => n.ID));

                        // 2) Upsert recorriendo top-down (los padres existen antes
                        //    de tocar a sus hijos, así no se rompe el FK self-ref)
                        foreach (var nodo in nodosMemoria)
                        {
                            try
                            {
                                if (idsExistentes.Contains(nodo.ID))
                                    ActualizarNodo(nodo, conn, transaction);
                                else
                                    InsertarNodo(nodo, conn, transaction);
                            }
                            catch (Exception exNodo)
                            {
                                // Adjuntar QUÉ nodo falló para no tener que adivinar.
                                string ctx = $"[{nombreTabla}] Falló {(idsExistentes.Contains(nodo.ID) ? "UPDATE" : "INSERT")} " +
                                             $"del nodo ID={nodo.ID} ParentID={(nodo.ParentID?.ToString() ?? "null")} " +
                                             $"Nivel={nodo.Nivel} Orden={nodo.Orden} Tipo={nodo.TipoTarea} " +
                                             $"Nombre='{nodo.Nombre}'";
                                ErrorLogger.RegistrarMensaje("FormEditorTreeList.GuardarArbolEnDb", ctx + "\n" + exNodo);
                                throw new Exception(ctx + "\n\nDetalle: " + exNodo.Message, exNodo);
                            }
                        }

                        // 3) Sincronizar las columnas adicionales en bloque.
                        SincronizarColumnasBulk(nodosMemoria, conn, transaction);

                        // 4) Eliminar solo los nodos que ya no están en memoria.
                        //    El CASCADE del FK eliminará únicamente sus activaciones,
                        //    preservando el progreso del resto de la casa.
                        var idsAEliminar = idsExistentes
                            .Where(id => !idsMemoria.Contains(id))
                            .ToList();
                        foreach (var id in idsAEliminar)
                            EliminarNodo(id, conn, transaction);

                        transaction.Commit();

                        // Ya persistidos: dejan de ser "nuevos" para que un segundo
                        // guardado los trate como existentes (UPDATE) y no se re-keyen.
                        foreach (var nodo in nodosMemoria)
                            nodo.EsNuevo = false;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Devuelve los nodos del árbol en orden top-down (raíces, luego hijos),
        /// de forma que un padre siempre aparece antes que sus descendientes.
        /// </summary>
        private static List<NodoTree> AplanarTopDown(List<NodoTree> raices)
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

        /// <summary>
        /// Re-asigna el ID de los nodos NUEVOS cuyo número ya exista en la BD (porque
        /// otra sesión insertó nodos mientras este editor estaba abierto). Sin esto, el
        /// upsert confundiría el nodo nuevo con el existente y lo sobrescribiría, o
        /// rompería la PK. Reapunta también el ParentID de los hijos afectados.
        /// </summary>
        private void ReasignarIdsEnConflicto(List<NodoTree> nodos, HashSet<int> idsExistentes)
        {
            // IDs ya ocupados = los de la BD + los que usan los nodos en memoria.
            var ocupados = new HashSet<int>(idsExistentes);
            foreach (var n in nodos) ocupados.Add(n.ID);
            int siguiente = ocupados.Count > 0 ? ocupados.Max() + 1 : 1;

            var remap = new Dictionary<int, int>();
            foreach (var n in nodos)
            {
                if (n.EsNuevo && idsExistentes.Contains(n.ID))
                {
                    int nuevo = siguiente++;
                    while (ocupados.Contains(nuevo)) nuevo = siguiente++;
                    ocupados.Add(nuevo);
                    remap[n.ID] = nuevo;
                    n.ID = nuevo;
                }
            }

            if (remap.Count == 0) return;

            // Reapuntar los hijos cuyo padre fue re-keyado (remap usa el ID viejo).
            foreach (var n in nodos)
                if (n.ParentID.HasValue && remap.TryGetValue(n.ParentID.Value, out int nuevoPadre))
                    n.ParentID = nuevoPadre;

            ErrorLogger.RegistrarMensaje("FormEditorTreeList.ReasignarIdsEnConflicto",
                $"[{nombreTabla}] Se re-asignaron {remap.Count} ID(s) de nodos nuevos por colisión " +
                $"con la BD: {string.Join(", ", remap.Select(kv => kv.Key + "->" + kv.Value))}");
        }

        private HashSet<int> LeerIdsExistentes(SqlConnection conn, SqlTransaction transaction)
        {
            var ids = new HashSet<int>();
            using (var cmd = new SqlCommand($"SELECT ID FROM {nombreTabla}", conn, transaction))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    ids.Add(reader.GetInt32(0));
            }
            return ids;
        }

        private void InsertarNodo(NodoTree nodo, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = $@"
                INSERT INTO {nombreTabla}
                    (ID, ParentID, Nombre, Descripcion, Orden, Nivel,
                     FechaCreacion, FechaModificacion, UsuarioCreacion, TipoTarea)
                VALUES
                    (@id, @parentId, @nombre, @descripcion, @orden, @nivel,
                     @fechaCreacion, @fechaModificacion, @usuarioCreacion, @tipoTarea)";

            using (var cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@id", nodo.ID);
                cmd.Parameters.AddWithValue("@parentId", (object)nodo.ParentID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nombre", nodo.Nombre ?? string.Empty);
                cmd.Parameters.AddWithValue("@descripcion", (object)nodo.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@orden", nodo.Orden);
                cmd.Parameters.AddWithValue("@nivel", nodo.Nivel);
                cmd.Parameters.AddWithValue("@fechaCreacion", nodo.FechaCreacion);
                cmd.Parameters.AddWithValue("@fechaModificacion", (object)nodo.FechaModificacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@usuarioCreacion", (object)nodo.UsuarioCreacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tipoTarea", (int)nodo.TipoTarea);
                cmd.ExecuteNonQuery();
            }
        }

        private void ActualizarNodo(NodoTree nodo, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = $@"
                UPDATE {nombreTabla}
                   SET ParentID          = @parentId,
                       Nombre            = @nombre,
                       Descripcion       = @descripcion,
                       Orden             = @orden,
                       Nivel             = @nivel,
                       FechaModificacion = @fechaModificacion,
                       UsuarioCreacion   = @usuarioCreacion,
                       TipoTarea         = @tipoTarea
                 WHERE ID = @id";

            using (var cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@id", nodo.ID);
                cmd.Parameters.AddWithValue("@parentId", (object)nodo.ParentID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nombre", nodo.Nombre ?? string.Empty);
                cmd.Parameters.AddWithValue("@descripcion", (object)nodo.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@orden", nodo.Orden);
                cmd.Parameters.AddWithValue("@nivel", nodo.Nivel);
                cmd.Parameters.AddWithValue("@fechaModificacion", DateTime.Now);
                cmd.Parameters.AddWithValue("@usuarioCreacion", (object)nodo.UsuarioCreacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tipoTarea", (int)nodo.TipoTarea);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Re-sincroniza TODAS las columnas adicionales (Cantidad / Unidad / Precio /
        /// etc.) en bloque: un único DELETE y luego INSERT por lotes. Antes se hacía
        /// un DELETE + N INSERT por cada nodo, lo que en árboles grandes generaba
        /// cientos/miles de round-trips y era la causa principal de la lentitud.
        /// _Columnas no tiene FK saliente hacia ActivacionTareasRuta, así que el
        /// borrado total es seguro (se reinsertan las columnas vigentes en memoria).
        /// </summary>
        private void SincronizarColumnasBulk(List<NodoTree> nodos, SqlConnection conn, SqlTransaction transaction)
        {
            // 1) Limpiar de una sola vez. Las filas de nodos eliminados desaparecen
            //    aquí y simplemente no se vuelven a insertar.
            using (var cmdDel = new SqlCommand($"DELETE FROM {nombreTabla}_Columnas", conn, transaction))
            {
                cmdDel.ExecuteNonQuery();
            }

            // 2) Aplanar todas las filas a insertar (NodoID, NombreColumna, Valor).
            var filas = new List<(int NodoId, string Nombre, object Valor)>();
            foreach (var nodo in nodos)
            {
                if (nodo.ColumnasAdicionales == null) continue;
                foreach (var kvp in nodo.ColumnasAdicionales)
                    filas.Add((nodo.ID, kvp.Key, kvp.Value));
            }

            if (filas.Count == 0) return;

            // 3) Insertar en lotes con un INSERT multi-fila. Cada fila usa 3
            //    parámetros; el límite de SQL Server es 2100, así que 500 filas
            //    (1500 parámetros) deja margen de sobra.
            const int filasPorLote = 500;
            for (int inicio = 0; inicio < filas.Count; inicio += filasPorLote)
            {
                int conteo = Math.Min(filasPorLote, filas.Count - inicio);
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO {nombreTabla}_Columnas (NodoID, NombreColumna, Valor) VALUES ");

                using (var cmd = new SqlCommand { Connection = conn, Transaction = transaction })
                {
                    for (int j = 0; j < conteo; j++)
                    {
                        var fila = filas[inicio + j];
                        if (j > 0) sb.Append(',');
                        sb.Append($"(@n{j},@c{j},@v{j})");
                        cmd.Parameters.AddWithValue($"@n{j}", fila.NodoId);
                        cmd.Parameters.AddWithValue($"@c{j}", fila.Nombre);
                        // IMPORTANTE: tipar SIEMPRE @v como NVarChar. La columna Valor es
                        // nvarchar(max), pero AddWithValue infiere el tipo del objeto .NET
                        // (decimal para columnas calculadas, string para texto, etc.). Al
                        // mezclar tipos distintos en la MISMA columna de un INSERT
                        // multi-fila, SQL Server aplica precedencia de tipos y convierte
                        // TODA la columna a numeric, reventando con "Arithmetic overflow
                        // converting nvarchar to numeric" en cuanto una fila trae texto o un
                        // número grande. Forzar nvarchar evita esa coerción.
                        var pv = cmd.Parameters.Add($"@v{j}", SqlDbType.NVarChar, -1);
                        pv.Value = (object)ValorComoTexto(fila.Valor) ?? DBNull.Value;
                    }
                    cmd.CommandText = sb.ToString();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception exLote)
                    {
                        // El INSERT multi-fila no dice CUÁL fila falló. Reinsertamos el
                        // lote fila por fila para localizar exactamente el valor culpable
                        // (columna, valor y su tipo .NET, que suele ser la causa de las
                        // conversiones numéricas inesperadas).
                        var culpable = LocalizarFilaCulpable(filas, inicio, conteo, conn, transaction);
                        ErrorLogger.RegistrarMensaje("FormEditorTreeList.SincronizarColumnasBulk",
                            $"[{nombreTabla}_Columnas] Falló INSERT de columnas. {culpable}\n" + exLote);
                        throw new Exception(
                            $"Error guardando columnas en {nombreTabla}_Columnas.\n{culpable}\n\nDetalle: " + exLote.Message,
                            exLote);
                    }
                }
            }
        }

        /// <summary>
        /// Convierte el valor de una columna a su representación de texto para
        /// almacenarlo en la columna nvarchar(max) Valor. Los tipos numéricos se
        /// formatean con InvariantCulture (punto decimal) para no introducir comas
        /// según la configuración regional del equipo y mantener el round-trip con
        /// los valores ya guardados.
        /// </summary>
        private static string ValorComoTexto(object valor)
        {
            if (valor == null || valor == DBNull.Value) return null;
            if (valor is string s) return s;
            if (valor is IFormattable f) return f.ToString(null, CultureInfo.InvariantCulture);
            return valor.ToString();
        }

        /// <summary>
        /// Reinserta un lote fila por fila (en un savepoint que se revierte siempre)
        /// para identificar exactamente qué (NodoID, NombreColumna, Valor) provoca el
        /// fallo del INSERT en bloque. Devuelve una descripción legible de la fila
        /// culpable, incluyendo el tipo .NET del valor.
        /// </summary>
        private string LocalizarFilaCulpable(
            List<(int NodoId, string Nombre, object Valor)> filas, int inicio, int conteo,
            SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                for (int j = 0; j < conteo; j++)
                {
                    var fila = filas[inicio + j];
                    const string sp = "sp_DiagColumna";
                    try
                    {
                        transaction.Save(sp);
                        using (var cmd = new SqlCommand(
                            $"INSERT INTO {nombreTabla}_Columnas (NodoID, NombreColumna, Valor) VALUES (@n,@c,@v)",
                            conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@n", fila.NodoId);
                            cmd.Parameters.AddWithValue("@c", fila.Nombre);
                            var pv = cmd.Parameters.Add("@v", SqlDbType.NVarChar, -1);
                            pv.Value = (object)ValorComoTexto(fila.Valor) ?? DBNull.Value;
                            cmd.ExecuteNonQuery();
                        }
                        // No nos interesa conservar la fila de prueba.
                        transaction.Rollback(sp);
                    }
                    catch
                    {
                        try { transaction.Rollback(sp); } catch { /* la tx pudo quedar abortada */ }
                        string tipo = fila.Valor?.GetType().Name ?? "null";
                        string valTxt = fila.Valor?.ToString() ?? "NULL";
                        if (valTxt.Length > 100) valTxt = valTxt.Substring(0, 100) + "…";
                        return $"Fila culpable -> NodoID={fila.NodoId}, Columna='{fila.Nombre}', " +
                               $"Valor='{valTxt}' (tipo .NET={tipo})";
                    }
                }
            }
            catch
            {
                // El aislamiento es solo diagnóstico: nunca debe enmascarar el error real.
                return "No se pudo aislar la fila culpable (la transacción quedó abortada).";
            }
            return "No se pudo aislar una sola fila culpable (¿error a nivel de lote/tabla?).";
        }

        private void EliminarNodo(int id, SqlConnection conn, SqlTransaction transaction)
        {
            // El ON DELETE CASCADE del FK encadenará el borrado solamente para los
            // descendientes en DB de este nodo (que también están fuera de memoria)
            // y para sus filas correspondientes en ActivacionTareasRuta.
            using (var cmd = new SqlCommand(
                $"DELETE FROM {nombreTabla} WHERE ID = @id", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region M�todos auxiliares

        private NodoTree BuscarNodoPorId(int id)
        {
            foreach (var nodo in nodosRaiz)
            {
                if (nodo.ID == id)
                    return nodo;
                
                var encontrado = BuscarNodoPorIdRecursivo(nodo, id);
                if (encontrado != null)
                    return encontrado;
            }
            
            return null;
        }

        private NodoTree BuscarNodoPorIdRecursivo(NodoTree nodo, int id)
        {
            foreach (var hijo in nodo.Hijos)
            {
                if (hijo.ID == id)
                    return hijo;
                
                var encontrado = BuscarNodoPorIdRecursivo(hijo, id);
                if (encontrado != null)
                    return encontrado;
            }
            
            return null;
        }

        private void ActualizarEstadoBotones()
        {
            var nodoSeleccionado = treeListView.SelectedObject as NodoTree;
            
            btnAgregarPadre.Enabled = true;
            btnAgregarSubPadre.Enabled = nodoSeleccionado != null && nodoSeleccionado.Nivel == 0;
            btnAgregarHijo.Enabled = nodoSeleccionado != null && nodoSeleccionado.Nivel == 1;
            btnAsignarTipo.Enabled = nodoSeleccionado != null && nodoSeleccionado.Nivel == 2;
            btnRenombrar.Enabled = nodoSeleccionado != null;
            btnEliminar.Enabled = nodoSeleccionado != null;
            
            if (nodoSeleccionado != null)
            {
                var hermanos = ObtenerHermanos(nodoSeleccionado);
                int indice = hermanos.IndexOf(nodoSeleccionado);
                
                btnSubir.Enabled = indice > 0;
                btnBajar.Enabled = indice < hermanos.Count - 1;
                
                // Actualizar estado de botones de jerarqu�a
                btnSubirJerarquia.Enabled = nodoSeleccionado.Nivel > 0; // No subir si ya es ra�z
                btnBajarJerarquia.Enabled = nodoSeleccionado.Nivel < 2 && indice > 0; // No bajar si es nivel 2, y necesita hermano anterior
                btnConvertirAPadre.Enabled = nodoSeleccionado.Nivel == 1; // Solo un Sub-Padre se convierte en Padre
            }
            else
            {
                btnSubir.Enabled = false;
                btnBajar.Enabled = false;
                btnSubirJerarquia.Enabled = false;
                btnBajarJerarquia.Enabled = false;
                btnConvertirAPadre.Enabled = false;
            }
            
            btnGuardar.Enabled = cambiosPendientes;
            btnGuardar.BackColor = cambiosPendientes ? Color.FromArgb(46, 204, 113) : SystemColors.Control;
        }

        private int ObtenerIndiceGlobal(NodoTree nodo)
        {
            if (nodo == null) return 0;
            
            // Solo contar nodos de nivel 1 (Sub-Padre)
            if (nodo.Nivel != 1)
                return 0;
            
            int contador = 0;
            
            // Recorrer todos los nodos en orden y contar solo los de nivel 1 hasta encontrar el nodo buscado
            foreach (var root in nodosRaiz)
            {
                contador = ObtenerIndiceGlobalRecursivo(root, nodo, contador);
                if (contador < 0) // Nodo encontrado (se devuelve negativo para se�alizar)
                    return -contador;
            }
            
            return contador;
        }
        
        private int ObtenerIndiceGlobalRecursivo(NodoTree actual, NodoTree buscado, int contador)
        {
            foreach (var hijo in actual.Hijos)
            {
                // Solo contar nodos de nivel 1
                if (hijo.Nivel == 1)
                {
                    contador++;
                    if (hijo.ID == buscado.ID)
                        return -contador; // Retornar negativo para se�alizar que se encontr�
                }
                
                // Continuar buscando recursivamente en los descendientes
                int resultado = ObtenerIndiceGlobalRecursivo(hijo, buscado, contador);
                if (resultado < 0) // Ya se encontr�
                    return resultado;
                
                contador = resultado;
            }
            
            return contador;
        }

        #endregion

        #region Eventos del formulario

        private async void FormEditorTreeList_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cierre ya confirmado tras guardar (segunda pasada): dejar pasar.
            if (cerrarForzado)
                return;

            // No permitir cerrar mientras un guardado está en curso.
            if (guardando)
            {
                e.Cancel = true;
                return;
            }

            // El editor web (FormEditorTreeList.Web.cs) mantiene su propio estado de
            // cambios sin guardar, separado de cambiosPendientes (que sólo aplica al
            // árbol clásico en memoria, intacto mientras se usa el panel web).
            if (_webDirty)
            {
                var resultadoWeb = MessageBox.Show(
                    "Hay cambios sin guardar en el editor web.\n\n¿Salir de todas formas? Los cambios no guardados se perderán.",
                    "Cambios sin guardar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (resultadoWeb != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (cambiosPendientes)
            {
                var resultado = MessageBox.Show(
                    "Hay cambios sin guardar.\n\n�Deseas guardar antes de salir?",
                    "Cambios sin guardar",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    // Posponer el cierre: guardamos en segundo plano y, si todo sale
                    // bien, cerramos programáticamente.
                    e.Cancel = true;
                    bool guardado = await GuardarCambiosAsync();
                    if (guardado)
                    {
                        cerrarForzado = true;
                        this.Close();
                    }
                }
                else if (resultado == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                // Si es "No", se cierra sin guardar (no se cancela el evento).
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Gesti�n de Vista

        private void btnExpandirTodo_Click(object sender, EventArgs e)
        {
            treeListView.ExpandAll();
        }

        private void btnColapsarTodo_Click(object sender, EventArgs e)
        {
            treeListView.CollapseAll();
        }

        #endregion
    }
}
