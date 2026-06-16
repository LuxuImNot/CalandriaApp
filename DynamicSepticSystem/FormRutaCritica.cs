using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using OfficeOpenXml;
using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    public partial class FormRutaCritica : Form
    {
        private readonly string _rutaInicial;
        private readonly PanelPrincipal.Casa _casa;
        private readonly InventarioService _inventarioService;
        // Nombre del último destajo sobre el que el usuario hizo click (preferido para mostrar)
        private string _lastClickedNombre;

        public FormRutaCritica(string rutaInicial = null, PanelPrincipal.Casa casa = null, InventarioService inventarioService = null)
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);

            // Configuración del TreeListView
            treeListView1.CanExpandGetter = x => ((Tarea)x).Children.Count > 0;
            treeListView1.ChildrenGetter = x => ((Tarea)x).Children;
            treeListView1.ShowGroups = false;
            // Mostrar imágenes en subitems (necesario para ImageGetter en columnas)
            treeListView1.ShowImagesOnSubItems = true;

            this.Load += FormRutaCritica_Load;
            _rutaInicial = rutaInicial;
            _casa = casa;
            // If caller didn't provide an InventarioService, try to create one.
            // Creation may fail if configuration is missing; swallow exceptions and leave null in that case.
            if (inventarioService != null)
                _inventarioService = inventarioService;
            else
                _inventarioService = CreateInventarioServiceSafe();

            // Context menu para marcar entregado
            var cms = new ContextMenuStrip();
            var miMarcar = new ToolStripMenuItem("Marcar/Desmarcar entregado");
            miMarcar.Click += (s, e) => ToggleEntregadoSeleccionado();
            cms.Items.Add(miMarcar);
            treeListView1.ContextMenuStrip = cms;

            // Formateo de fila para colorear entregados
            treeListView1.FormatRow += TreeListView1_FormatRow;
            // Asegurar que al hacer click derecho se seleccione el item bajo el cursor
            treeListView1.MouseDown += TreeListView1_MouseDown;

            // Botón para guardar estado de entregados en la BD
            var btnGuardarEntregados = new Button();
            btnGuardarEntregados.Text = "Guardar entregados";
            btnGuardarEntregados.Width = 140;
            btnGuardarEntregados.Height = 30;
            btnGuardarEntregados.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            // Posicionar cerca de la esquina inferior derecha
            btnGuardarEntregados.Left = Math.Max(10, this.ClientSize.Width - btnGuardarEntregados.Width - 16);
            btnGuardarEntregados.Top = Math.Max(10, this.ClientSize.Height - btnGuardarEntregados.Height - 40);
            btnGuardarEntregados.Click += SaveEntregados_Click;
            this.Controls.Add(btnGuardarEntregados);
        }

        private void FormRutaCritica_Load(object sender, EventArgs e)
        {
            // Prioridad de carga de archivo Excel:
            // 1) ruta pasada en _rutaInicial (si existe)
            // 2) archivo por defecto junto al ejecutable: RUTACRITICAFINALTREELIST.xlsx
            // 3) archivo por defecto en el directorio de trabajo actual
            // 4) solicitar al usuario mediante OpenFileDialog

            // 1) ruta inicial pasada por el constructor
            if (!string.IsNullOrWhiteSpace(_rutaInicial) && File.Exists(_rutaInicial))
            {
                try
                {
                    var tareas = LeerExcel(_rutaInicial);
                    PoblarTreeListView(tareas);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la ruta crítica automática:\n" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // 2) archivo por defecto junto al ejecutable
            string defaultFileName = "RUTACRITICAFINALTREELIST.xlsx";
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultFileName);
            if (File.Exists(defaultPath))
            {
                try
                {
                    var tareas = LeerExcel(defaultPath);
                    PoblarTreeListView(tareas);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar archivo por defecto (" + defaultFileName + "):\n" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // continue to next fallback
                }
            }

            // 3) archivo por defecto en el directorio de trabajo actual
            string cwdPath = Path.Combine(Directory.GetCurrentDirectory(), defaultFileName);
            if (File.Exists(cwdPath))
            {
                try
                {
                    var tareas = LeerExcel(cwdPath);
                    PoblarTreeListView(tareas);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar archivo por defecto (" + defaultFileName + "):\n" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // 4) fallback: pedir al usuario que seleccione el archivo
            CargarExcel();

            // Si tenemos casa e inventarioService, refrescar estado de entregados desde la base de datos
            try
            {
                RefreshFromCasa();
            }
            catch { }
        }

        // Public helper para recargar DestajosTerminados desde la BD y refrescar la vista
        public void RefreshFromCasa()
        {
            if (_casa == null || _inventarioService == null) return;

            // Obtener la casa actual desde la BD para asegurar datos más recientes
            var ci = _inventarioService.ObtenerCasaPorManzanaYLote(_casa.Manzana, _casa.Lote);
            if (ci == null) return;

            // Copiar lista y aplicar marcas
            _casa.DestajosTerminadosWBS = ci.DestajosTerminadosWBS ?? new List<string>();

            // Recorrer todas tareas actuales en el TreeListView y aplicar Entregado cuando corresponda
            var roots = treeListView1.Objects.Cast<Tarea>().ToList();
            var all = roots.SelectMany(r => FlattenTareasTree(r));

            var entregadosSet = new HashSet<string>(_casa.DestajosTerminadosWBS.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));

            foreach (var t in all)
            {
                t.Entregado = !string.IsNullOrWhiteSpace(t.WBS) && entregadosSet.Contains(t.WBS.Trim());
            }

            treeListView1.RefreshObjects(roots);
        }

        private void CargarExcel()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files|*.xlsx;*.xls";
                ofd.Title = "Selecciona el archivo CALANDRA";

                if (ofd.ShowDialog() != DialogResult.OK) return;

                var archivo = ofd.FileName;

                var tareas = LeerExcel(archivo);

                PoblarTreeListView(tareas);
            }
        }

        private List<Tarea> LeerExcel(string ruta)
        {
            var lista = new List<Tarea>();

            try
            {
                // EPPlus 5+ requires setting LicenseContext
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("René");
                }
                catch { /* ignore if running older EPPlus */ }

                using (var package = new ExcelPackage(new FileInfo(ruta)))
                {
                    if (package.Workbook == null || package.Workbook.Worksheets == null || package.Workbook.Worksheets.Count == 0)
                        throw new ArgumentException("El archivo Excel no contiene hojas. Verifica el archivo.");

                    var sheet = package.Workbook.Worksheets.First();
                    int fila = 2; // saltamos encabezados

                    while (true)
                    {
                        var cell1 = sheet.Cells[fila, 1];
                        var cell2 = sheet.Cells[fila, 2];
                        if ((cell1 == null || cell1.Value == null || string.IsNullOrWhiteSpace(cell1.Text)) &&
                            (cell2 == null || cell2.Value == null || string.IsNullOrWhiteSpace(cell2.Text)))
                        {
                            break; // fin de datos
                        }

                        string id = cell1?.Text?.Trim() ?? string.Empty;
                        string wbs = cell2?.Text?.Trim() ?? string.Empty;
                        string nombre = sheet.Cells[fila, 3]?.Text?.Trim() ?? string.Empty;
                        string costo = sheet.Cells[fila, 4]?.Text?.Trim() ?? string.Empty;
                        string work = sheet.Cells[fila, 5]?.Text?.Trim() ?? string.Empty;

                        if (!string.IsNullOrEmpty(nombre))
                        {
                            lista.Add(new Tarea
                            {
                                ID = id,
                                WBS = wbs,
                                Nombre = nombre,
                                Costo = costo,
                                Work = work
                            });
                        }

                        fila++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Proveer mensaje más claro al usuario
                throw new Exception($"Error leyendo archivo Excel: {ex.Message}", ex);
            }

            return lista;
        }

        private void PoblarTreeListView(List<Tarea> tareas)
        {
            // Limpiar objetos antiguos en el control
            treeListView1.ClearObjects();

            // Normalizar: limpiar relaciones previas para evitar duplicados si se recarga
            foreach (var t in tareas)
            {
                t.Parent = null;
                if (t.Children == null)
                    t.Children = new List<Tarea>();
                else
                    t.Children.Clear();

                // inicializar progress fields
                t.HijosCount = 0;
                t.HijosCompletados = 0;
                t.HijosPct = 0;
            }

            // Diccionario de nodos por WBS y seguimiento del último visto
            var nodosPorWBS = new Dictionary<string, Tarea>();
            var lastSeenByWBS = new Dictionary<string, Tarea>();
            Tarea ultimoConWBS = null;

            for (int i = 0; i < tareas.Count; i++)
            {
                var tarea = tareas[i];
                var wbs = (tarea.WBS ?? "").Trim();

                if (!string.IsNullOrWhiteSpace(wbs) && wbs != "0")
                {
                    var partes = wbs.Split('.');
                    if (partes.Length > 1)
                    {
                        var parentWBS = string.Join(".", partes.Take(partes.Length - 1));
                        // Preferir el padre que apareció antes en el archivo
                        if (lastSeenByWBS.TryGetValue(parentWBS, out var padre))
                        {
                            tarea.Parent = padre;
                            if (!padre.Children.Contains(tarea)) padre.Children.Add(tarea);
                        }
                        else if (nodosPorWBS.TryGetValue(parentWBS, out var padreFallback))
                        {
                            tarea.Parent = padreFallback;
                            if (!padreFallback.Children.Contains(tarea)) padreFallback.Children.Add(tarea);
                        }
                    }

                    // registrar este WBS como el último visto en este punto
                    lastSeenByWBS[wbs] = tarea;
                    nodosPorWBS[wbs] = tarea;
                    ultimoConWBS = tarea;
                }
                else
                {
                    // Sin WBS: asignar al último con WBS visto (ultimoConWBS)
                    if (ultimoConWBS != null)
                    {
                        tarea.Parent = ultimoConWBS;
                        if (!ultimoConWBS.Children.Contains(tarea)) ultimoConWBS.Children.Add(tarea);
                    }
                    else
                    {
                        // Si no hay todavía un ultimoConWBS, buscar hacia atrás en el archivo
                        Tarea encontrado = null;
                        for (int j = i - 1; j >= 0; j--)
                        {
                            var candidato = tareas[j];
                            if (!string.IsNullOrWhiteSpace(candidato.WBS) && candidato.WBS.Trim() != "0")
                            {
                                encontrado = candidato;
                                break;
                            }
                        }

                        if (encontrado != null)
                        {
                            tarea.Parent = encontrado;
                            if (!encontrado.Children.Contains(tarea)) encontrado.Children.Add(tarea);
                        }
                        else
                        {
                            tarea.Parent = null; // raíz si no hay WBS previo
                        }
                    }
                }
            }

            // Tomar solo las tareas que no tienen padre (raíces) y pasarlas al TreeListView
            var raices = tareas.Where(t => t.Parent == null).ToList();

            // Calcular máximo de hijos para escalar la barra de progreso (evita división/porcentajes erróneos)
            int maxChildren = 0;
            try
            {
                maxChildren = tareas.Select(t => t.Children?.Count ?? 0).DefaultIfEmpty(0).Max();
            }
            catch { maxChildren = 0; }
            if (maxChildren < 1) maxChildren = 1;

            // Configurar columnas del TreeListView para mostrar columnas similares a la hoja
            try
            {
                // Limpiar columnas previas
                treeListView1.AllColumns.Clear();
                treeListView1.Columns.Clear();

                var colConcepto = new BrightIdeasSoftware.OLVColumn("Concepto", "Concepto") { Text = "Concepto", Width = 200 };
                // Mostrar el nombre solo para nodos que son padres (tienen hijos)
                colConcepto.AspectGetter = row =>
                {
                    var t = row as Tarea;
                    if (t == null) return "";
                    return (t.Children != null && t.Children.Count > 0) ? t.Nombre : "";
                };

                var colNumero = new BrightIdeasSoftware.OLVColumn("WBS", "WBS") { AspectName = "WBS", Text = "Número de destajo", Width = 80 };
                var colNombre = new BrightIdeasSoftware.OLVColumn("Nombre", "Nombre") { Text = "Nombre del Destajo", Width = 420 };
                // Mostrar el nombre solo para nodos hoja (no padres)
                colNombre.AspectGetter = row =>
                {
                    var t = row as Tarea;
                    if (t == null) return "";
                    return (t.Children == null || t.Children.Count == 0) ? t.Nombre : "";
                };

                var colMano = new BrightIdeasSoftware.OLVColumn("Work", "Work") { AspectName = "Work", Text = "MANO DE OBRA", Width = 140 };
                var colMaterial = new BrightIdeasSoftware.OLVColumn("Costo", "Costo") { AspectName = "Costo", Text = "MATERIAL REQUERIDO", Width = 160 };
                var colDuracion = new BrightIdeasSoftware.OLVColumn("Duracion", "Duracion") { AspectName = "Duracion", Text = "Duración", Width = 80 };
                var colComienzo = new BrightIdeasSoftware.OLVColumn("Inicio", "Inicio") { AspectName = "Inicio", Text = "Comienzo", Width = 90 };
                var colFin = new BrightIdeasSoftware.OLVColumn("Fin", "Fin") { AspectName = "Fin", Text = "Fin", Width = 90 };
                var colTiempoReal = new BrightIdeasSoftware.OLVColumn("TiempoReal", "TiempoReal") { AspectName = "TiempoReal", Text = "Tiempo Real", Width = 90 };

                var colObserv = new BrightIdeasSoftware.OLVColumn("Observaciones", "Observaciones") { AspectName = "Observaciones", Text = "Observaciones", Width = 120 };

                // Action buttons as separate small columns (shown for leaf tasks)
                var colAccMano = new BrightIdeasSoftware.OLVColumn("AccMano", "AccMano") { Text = "MANO", Width = 120 };
                colAccMano.Name = "AccMano";
                colAccMano.AspectGetter = row =>
                {
                    var t = row as Tarea;
                    if (t == null) return "";
                    return (t.Children == null || t.Children.Count == 0) ? "MANO DE OBRA" : "";
                };
                // Render as button image
                colAccMano.Renderer = new BrightIdeasSoftware.ImageRenderer();
                colAccMano.ImageGetter = row =>
                {
                    var t = row as Tarea;
                    if (t == null) return null;
                    if (t.Children != null && t.Children.Count > 0) return null;
                    return CreateButtonBitmap("MANO DE OBRA", colAccMano.Width - 8, 20);
                };

                var colAccInsumos = new BrightIdeasSoftware.OLVColumn("AccInsumos", "AccInsumos") { Text = "INSUMOS", Width = 80 };
                colAccInsumos.Name = "AccInsumos";
                colAccInsumos.AspectGetter = row =>
                {
                    var t = row as Tarea;
                    if (t == null) return "";
                    return (t.Children == null || t.Children.Count == 0) ? "INSUMOS" : "";
                };
                colAccInsumos.Renderer = new BrightIdeasSoftware.ImageRenderer();
                colAccInsumos.ImageGetter = row =>
                {
                    var t = row as Tarea;
                    if (t == null) return null;
                    if (t.Children != null && t.Children.Count > 0) return null;
                    return CreateButtonBitmap("INSUMOS", colAccInsumos.Width - 8, 20);
                };

                var colAccReporte = new BrightIdeasSoftware.OLVColumn("AccReporte", "AccReporte") { Text = "REPORTE", Width = 80 };
                colAccReporte.Name = "AccReporte";
                colAccReporte.AspectGetter = row =>
                {
                    var t = row as Tarea;
                    if (t == null) return "";
                    return (t.Children == null || t.Children.Count == 0) ? "REPORTE" : "";
                };
                colAccReporte.Renderer = new BrightIdeasSoftware.ImageRenderer();
                colAccReporte.ImageGetter = row =>
                {
                    var t = row as Tarea;
                    if (t == null) return null;
                    if (t.Children != null && t.Children.Count > 0) return null;
                    // Show red button when tarea is NOT entregada
                    return CreateButtonBitmap("REPORTE", colAccReporte.Width - 8, 20, t.Entregado);
                };

                // NUEVA COLUMNA: Checkbox para marcar Entregado (texto unicode, clickable via CellClick)
                var colCheck = new BrightIdeasSoftware.OLVColumn("EntregadoChk", "Entregado") { Text = "", Width = 30 };
                colCheck.Name = "EntregadoChk";
                colCheck.IsEditable = false;
                colCheck.AspectGetter = row =>
                {
                    var t = row as Tarea;
                    if (t == null) return string.Empty;
                    return t.Entregado ? "☑" : "☐"; // Unicode checkbox visuals
                };

                // Insert checkbox column at the beginning (far left)
                treeListView1.AllColumns.Add(colCheck);

                // add other columns
                treeListView1.AllColumns.Add(colConcepto);
                treeListView1.AllColumns.Add(colNumero);
                treeListView1.AllColumns.Add(colNombre);

                // add action columns
                treeListView1.AllColumns.Add(colAccMano);
                treeListView1.AllColumns.Add(colAccInsumos);
                treeListView1.AllColumns.Add(colAccReporte);

                treeListView1.AllColumns.Add(colMano);
                treeListView1.AllColumns.Add(colMaterial);
                treeListView1.AllColumns.Add(colDuracion);
                treeListView1.AllColumns.Add(colComienzo);
                treeListView1.AllColumns.Add(colFin);
                treeListView1.AllColumns.Add(colTiempoReal);

                treeListView1.AllColumns.Add(colObserv);

                treeListView1.RebuildColumns();

                // Handle clicks to toggle the unicode checkbox
                treeListView1.CellClick += TreeListView1_CellClick;
            }
            catch { }

            // Recalculate progress fields before showing
            RecalculateProgressForRoots(raices);

            treeListView1.SetObjects(raices);
            // Start with all nodes collapsed
            try { treeListView1.CollapseAll(); } catch { }

            // Si tenemos casaActual, inicializar estado Entregado desde su lista persistida
            if (_casa != null && _casa.DestajosTerminadosWBS != null && _casa.DestajosTerminadosWBS.Count > 0)
            {
                // Crear diccionario seguro evitando claves vacías y duplicados
                var todos = tareas
                    .Where(t => !string.IsNullOrWhiteSpace(t.WBS))
                    .GroupBy(t => t.WBS.Trim())
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var w in _casa.DestajosTerminadosWBS)
                {
                    if (string.IsNullOrWhiteSpace(w)) continue;
                    if (todos.TryGetValue(w.Trim(), out var tar)) tar.Entregado = true;
                }

                // Recalculate again after applying entregados
                RecalculateProgressForRoots(raices);
                treeListView1.RefreshObjects(raices);
            }

            // Actualizar progreso general tras cargar marcas
            UpdateGeneralProgress();
        }

        // Actualiza la barra de progreso general y la etiqueta del último destajo completado.
        private void UpdateGeneralProgress()
        {
            try
            {
                var roots = treeListView1.Objects.Cast<Tarea>().ToList();
                if (roots == null || roots.Count == 0)
                {
                    if (progressBarGenCon.InvokeRequired)
                        progressBarGenCon.Invoke((Action)(() => progressBarGenCon.Value = 0));
                    else
                        progressBarGenCon.Value = 0;

                    if (lblLastDestajo.InvokeRequired)
                        lblLastDestajo.Invoke((Action)(() => lblLastDestajo.Text = string.Empty));
                    else
                        lblLastDestajo.Text = string.Empty;
                    return;
                }

                var all = roots.SelectMany(r => FlattenTareasTree(r)).ToList();
                var tasks = all.Where(t => !string.IsNullOrWhiteSpace(t.Nombre)).ToList();
                int total = tasks.Count;
                if (total == 0) total = 1;

                int completed = tasks.Count(t => t.Entregado);
                int percent = (int)Math.Round(completed * 100.0 / total);
                percent = Math.Max(0, Math.Min(100, percent));

                if (progressBarGenCon.InvokeRequired)
                    progressBarGenCon.Invoke((Action)(() => progressBarGenCon.Value = percent));
                else
                    progressBarGenCon.Value = percent;

                // Update numeric label next to progress bar if present
                try
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke((Action)(() =>
                        {
                            var pctCtrl = this.Controls.Find("lblPorcentajeGEN", true).FirstOrDefault() as System.Windows.Forms.Label;
                            if (pctCtrl != null) pctCtrl.Text = percent + "%";

                            // Prefer the last task the user clicked on; otherwise show last completed
                            string lastText = !string.IsNullOrWhiteSpace(_lastClickedNombre)
                                ? _lastClickedNombre
                                : (tasks.LastOrDefault(t => t.Entregado)?.Nombre ?? string.Empty);
                            var lastCtrl = this.Controls.Find("lblLastDestajo", true).FirstOrDefault() as MaterialSkin.Controls.MaterialLabel;
                            if (lastCtrl != null) lastCtrl.Text = lastText;
                        }));
                    }
                    else
                    {
                        var pctCtrl = this.Controls.Find("lblPorcentajeGEN", true).FirstOrDefault() as System.Windows.Forms.Label;
                        if (pctCtrl != null) pctCtrl.Text = percent + "%";

                        string lastText = !string.IsNullOrWhiteSpace(_lastClickedNombre)
                            ? _lastClickedNombre
                            : (tasks.LastOrDefault(t => t.Entregado)?.Nombre ?? string.Empty);
                        lblLastDestajo.Text = lastText;
                    }
                }
                catch { }
            }
            catch { }
        }

        private void TreeListView1_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            var tarea = e.Model as Tarea;
            if (tarea != null && tarea.Entregado)
            {
                e.Item.BackColor = Color.LightGreen;
            }
            else
            {
                e.Item.BackColor = Color.White;
            }
        }

        private void TreeListView1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Record last clicked item for left-click (user preference). Also handle right-click selection as before.
                var lviAny = treeListView1.GetItemAt(e.X, e.Y);
                if (lviAny != null)
                {
                    var olvItemAny = lviAny as BrightIdeasSoftware.OLVListItem;
                    var modelAny = olvItemAny?.RowObject as Tarea;
                    if (modelAny != null && !string.IsNullOrWhiteSpace(modelAny.Nombre))
                        _lastClickedNombre = modelAny.Nombre;
                }

                // Only change selection on right-click so normal left-click behavior is preserved
                if (e.Button == MouseButtons.Right)
                {
                    // Ensure the control has focus (helps SelectedObject behave consistently)
                    treeListView1.Focus();

                    // Prefer the model under the cursor; use GetItemAt and cast to OLVListItem to obtain the RowObject
                    var lvi = treeListView1.GetItemAt(e.X, e.Y);
                    if (lvi != null)
                    {
                        var olvItem = lvi as BrightIdeasSoftware.OLVListItem;
                        var model = olvItem?.RowObject;
                        if (model != null)
                            treeListView1.SelectObject(model);
                        else
                            lvi.Selected = true; // fallback
                    }
                    else
                    {
                        // Clicked on empty space -> clear selection so context menu actions don't act on stale selection
                        treeListView1.SelectedObject = null;
                    }
                }
            }
            catch { }
        }

        private void SaveEntregados_Click(object sender, EventArgs e)
        {
            // Recolectar todos los WBS marcados como entregado (incluye nodos hijos)
            var roots = treeListView1.Objects.Cast<Tarea>();
            var all = roots.SelectMany(r => FlattenTareasTree(r));
            var entregados = all.Where(t => t.Entregado && !string.IsNullOrWhiteSpace(t.WBS)).Select(t => t.WBS).Distinct().ToList();

            // If we don't have both casa and a usable service, fall back to saving locally
            InventarioService service = _inventarioService ?? CreateInventarioServiceSafe();
            if (_casa == null || service == null)
            {
                try
                {
                    string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CALANDRIA RESIDENCIAL", "RutaCritica");
                    Directory.CreateDirectory(carpeta);
                    string nombreArchivo = Path.Combine(carpeta, $"DestajosTerminados_{DateTime.Now:yyyyMMdd_Hmmss}.json");
                    // Guardar objeto con metadatos si hay casa
                    var payload = new
                    {
                        Manzana = _casa?.Manzana,
                        Lote = _casa?.Lote,
                        Fecha = DateTime.Now,
                        DestajosTerminadosWBS = entregados
                    };
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(nombreArchivo, json);
                    MessageBox.Show($"No hay casa o servicio disponible. Lista guardada localmente en:\n{nombreArchivo}", "Guardado local", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar localmente: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // Tenemos service y casa: persistir en base de datos
            _casa.DestajosTerminadosWBS = entregados;
            try
            {
                var ci = new CasaInventario { Manzana = _casa.Manzana, Lote = _casa.Lote, DestajosTerminadosWBS = entregados };
                service.ActualizarDestajosTerminados(ci);
                MessageBox.Show("Estados guardados correctamente.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar en la base de datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SyncCasaEntregadosFromTree()
        {
            if (_casa == null) return;
            var roots = treeListView1.Objects.Cast<Tarea>();
            var all = roots.SelectMany(r => FlattenTareasTree(r));
            var entregados = all.Where(t => t.Entregado && !string.IsNullOrWhiteSpace(t.WBS)).Select(t => t.WBS).Distinct().ToList();
            _casa.DestajosTerminadosWBS = entregados;
            if (_inventarioService != null)
            {
                try
                {
                    var ci = new CasaInventario { Manzana = _casa.Manzana, Lote = _casa.Lote, DestajosTerminadosWBS = entregados };
                    _inventarioService.ActualizarDestajosTerminados(ci);
                }
                catch { }
            }
        }

        private void UpdateAncestorsAfterChange(Tarea node)
        {
            var parent = node?.Parent;
            while (parent != null)
            {
                bool allChildrenDone = parent.Children != null && parent.Children.Count > 0 && parent.Children.All(c => c.Entregado);
                // If parent has no children, don't change its state here
                parent.Entregado = allChildrenDone;
                parent = parent.Parent;
            }
        }

        private IEnumerable<Tarea> FlattenTareasTree(Tarea root)
        {
            if (root == null) yield break;
            yield return root;
            if (root.Children == null) yield break;
            foreach (var c in root.Children)
            {
                foreach (var sub in FlattenTareasTree(c))
                    yield return sub;
            }
        }

        private InventarioService CreateInventarioServiceSafe()
        {
            try
            {
                return new InventarioService();
            }
            catch
            {
                // Could not create service (missing config, etc.). Return null and let callers handle absence.
                return null;
            }
        }

        /// <summary>
        /// Recalculate children progress (immediate children only) for all provided roots.
        /// This sets Tarea.HijosCount, HijosCompletados and HijosPct (0..100, integer).
        /// </summary>
        private void RecalculateProgressForRoots(IEnumerable<Tarea> roots)
        {
            if (roots == null) return;
            foreach (var r in roots)
                RecalculateProgressRecursive(r);
        }

        /// <summary>
        /// Post-order traversal to ensure children states are processed before parents.
        /// Progress is computed using immediate children only: percent = (completed children / total children) * 100.
        /// </summary>
        private void RecalculateProgressRecursive(Tarea node)
        {
            if (node == null) return;
            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var c in node.Children)
                    RecalculateProgressRecursive(c);

                node.HijosCount = node.Children.Count;
                node.HijosCompletados = node.Children.Count(ch => ch.Entregado);
                node.HijosPct = node.HijosCount > 0 ? (int)(node.HijosCompletados * 100 / node.HijosCount) : 0;
            }
            else
            {
                node.HijosCount = 0;
                node.HijosCompletados = 0;
                node.HijosPct = 0;
            }
        }

        /// <summary>
        /// Update progress for ancestors after a child toggle and refresh the UI for affected nodes.
        /// </summary>
        private void UpdateProgressAfterToggle(Tarea child)
        {
            if (child == null) return;

            // Recalculate this child and all ancestors
            RecalculateProgressRecursive(child);
            var parent = child.Parent;
            while (parent != null)
            {
                RecalculateProgressRecursive(parent);
                parent = parent.Parent;
            }

            // Refresh this node and its ancestors visually
            try
            {
                treeListView1.RefreshObject(child);
                parent = child.Parent;
                while (parent != null)
                {
                    treeListView1.RefreshObject(parent);
                    parent = parent.Parent;
                }
            }
            catch
            {
                // fallback: refresh all roots
                try { treeListView1.RefreshObjects(treeListView1.Objects.Cast<Tarea>().ToList()); } catch { }
            }
        }

        private Image CreateProgressBitmap(int percent, int width, int height)
        {
            if (width <= 0) width = 50;
            if (height <= 0) height = 12;
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // background
                using (var back = new SolidBrush(Color.LightGray))
                    g.FillRectangle(back, 0, 0, width, height);
                // filled part
                int fill = Math.Max(0, Math.Min(100, percent)) * width / 100;
                using (var fillBrush = new SolidBrush(Color.FromArgb(100, 176, 76)))
                    g.FillRectangle(fillBrush, 0, 0, fill, height);
                // border
                using (var pen = new Pen(Color.DarkGray))
                    g.DrawRectangle(pen, 0, 0, width - 1, height - 1);
            }
            return bmp;
        }

        private Image CreateButtonBitmap(string text, int width, int height, bool enabled = true)
        {
            if (width <= 0) width = 80;
            if (height <= 0) height = 20;
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // background
                using (var back = new SolidBrush(Color.FromArgb(230, 230, 230)))
                    g.FillRectangle(back, 0, 0, width, height);
                // button fill: blue when enabled, red when disabled
                var fillColor = enabled ? Color.FromArgb(70, 130, 180) : Color.FromArgb(200, 60, 60);
                using (var fill = new SolidBrush(fillColor))
                    g.FillRectangle(fill, 2, 2, width - 4, height - 4);
                // text
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var font = new Font(SystemFonts.DefaultFont.FontFamily, 8, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(text, font, brush, new RectangleF(2, 2, width - 4, height - 4), sf);
                }
                // border color depends on enabled
                var borderColor = enabled ? Color.DarkBlue : Color.DarkRed;
                using (var pen = new Pen(borderColor))
                    g.DrawRectangle(pen, 1, 1, width - 3, height - 3);
            }
            return bmp;
        }

        private void TreeListView1_CellClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.Column == null) return;

                // Remember last clicked task when user clicks any cell in a row
                var tareaForClick = e.Model as Tarea;
                if (tareaForClick != null && !string.IsNullOrWhiteSpace(tareaForClick.Nombre))
                    _lastClickedNombre = tareaForClick.Nombre;

                // Action columns
                if (e.Column.Name == "AccMano" || e.Column.Name == "AccInsumos" || e.Column.Name == "AccReporte")
                {
                    var tarea = e.Model as Tarea;
                    if (tarea == null) return;

                    // Only for leaf tasks
                    if (tarea.Children != null && tarea.Children.Count > 0) return;

                    if (e.Column.Name == "AccMano")
                    {
                        // Open Mano de Obra form and provide current casa/prototipo if available
                        using (var f = new FormManoObra(tarea))
                        {
                            try
                            {
                                if (_casa != null)
                                {
                                    CasaInventario ci = null;
                                    var svc = _inventarioService ?? CreateInventarioServiceSafe();
                                    if (svc != null)
                                        ci = svc.ObtenerCasaPorManzanaYLote(_casa.Manzana, _casa.Lote);

                                    f.SetCasa(_casa, ci ?? new CasaInventario { Manzana = _casa.Manzana, Lote = _casa.Lote, Prototipo = ci?.Prototipo });
                                }
                            }
                            catch { }
                            f.ShowDialog(this);
                        }
                    }
                    else if (e.Column.Name == "AccInsumos")
                    {
                        // Open Insumos form and provide current casa/prototipo if available
                        using (var f = new FormInsumos(tarea))
                        {
                            try
                            {
                                if (_casa != null)
                                {
                                    CasaInventario ci = null;
                                    var svc = _inventarioService ?? CreateInventarioServiceSafe();
                                    if (svc != null)
                                        ci = svc.ObtenerCasaPorManzanaYLote(_casa.Manzana, _casa.Lote);

                                    f.SetCasa(_casa, ci ?? new CasaInventario { Manzana = _casa.Manzana, Lote = _casa.Lote, Prototipo = ci?.Prototipo });
                                }
                            }
                            catch { }
                            f.ShowDialog(this);
                        }
                    }
                    else if (e.Column.Name == "AccReporte")
                    {
                        try
                        {
                            // Do not allow generating report if the destajo is not marked as entregado
                            if (!tarea.Entregado)
                            {
                                MessageBox.Show("El destajo no está marcado como terminado. Marca como entregado antes de generar el reporte.", "Acción no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                             // Try to get previously saved entregados from DB
                             System.Data.DataTable dtEntregados = null;
                             try
                             {
                                 if (_casa != null && _inventarioService != null)
                                     dtEntregados = _inventarioService.ObtenerInsumosEntregados(_casa.Manzana, _casa.Lote, tarea?.WBS);
                             }
                             catch { dtEntregados = null; }

                             // If none saved, let user open FormInsumos to mark and save
                             if (dtEntregados == null)
                             {
                                 using (var f = new FormInsumos(tarea))
                                 {
                                     try
                                     {
                                         if (_casa != null)
                                         {
                                             CasaInventario ci = null;
                                             var svc = _inventarioService ?? CreateInventarioServiceSafe();
                                             if (svc != null) ci = svc.ObtenerCasaPorManzanaYLote(_casa.Manzana, _casa.Lote);
                                             f.SetCasa(_casa, ci ?? new CasaInventario { Manzana = _casa.Manzana, Lote = _casa.Lote, Prototipo = ci?.Prototipo });
                                         }
                                     }
                                     catch { }

                                     if (f.ShowDialog(this) == DialogResult.OK)
                                     {
                                         dtEntregados = f.GetEntregadosTable();
                                     }
                                     else
                                     {
                                         // user cancelled
                                         return;
                                     }
                                 }
                             }

                             // Generate temp PDF and show preview
                             string carpetaTemp = Path.Combine(Path.GetTempPath(), "CalandriaReports");
                             Directory.CreateDirectory(carpetaTemp);
                             string tempFile = Path.Combine(carpetaTemp, $"ReporteDestajos_{tarea.WBS ?? tarea.Nombre}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                             GenerateReportePdfToFile(tarea, dtEntregados, tempFile);

                             using (var preview = new FormPdfPreview(tempFile, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ReporteDestajos.pdf")))
                             {
                                 preview.ShowDialog(this);
                                 // If user saved copy, nothing further needed (file already copied). If canceled, temp removed by preview.
                             }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error preparando reporte: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        return;
                    }

                    return;
                }

                // First column is the checkbox (unicode)
                if (e.Column.Name == "EntregadoChk")
                {
                    var tarea = e.Model as Tarea;
                    if (tarea == null) return;

                    // Toggle state for this node (and its descendants)
                    bool nuevoEstado = !tarea.Entregado;
                    var afectados = FlattenTareasTree(tarea).ToList();
                    foreach (var t in afectados) t.Entregado = nuevoEstado;

                    // Update ancestors: if all children done, mark parent(s)
                    UpdateAncestorsAfterChange(tarea);

                    // Recalculate progress fields and sync casa
                    RecalculateProgressForRoots(treeListView1.Objects.Cast<Tarea>());
                    SyncCasaEntregadosFromTree();

                    // Refresh entire tree to ensure parents and children updated
                    try { treeListView1.RefreshObjects(treeListView1.Objects.Cast<Tarea>().ToList()); } catch { treeListView1.RefreshObjects(afectados); }

                    // Actualizar progreso general y etiqueta del último destajo
                    UpdateGeneralProgress();

                    return;
                }
            }
            catch { }
        }

        // Toggle selected task entregado state via context menu
        private void ToggleEntregadoSeleccionado()
        {
            var tarea = treeListView1.SelectedObject as Tarea;
            if (tarea == null) return;

            bool nuevoEstado = !tarea.Entregado;
            var afectados = FlattenTareasTree(tarea).ToList();
            foreach (var t in afectados) t.Entregado = nuevoEstado;

            // Update casa in-memory list if present
            if (_casa != null)
            {
                if (_casa.DestajosTerminadosWBS == null)
                    _casa.DestajosTerminadosWBS = new List<string>();

                if (nuevoEstado)
                {
                    foreach (var t in afectados)
                    {
                        if (!string.IsNullOrWhiteSpace(t.WBS) && !_casa.DestajosTerminadosWBS.Contains(t.WBS))
                            _casa.DestajosTerminadosWBS.Add(t.WBS);
                    }
                }
                else
                {
                    foreach (var t in afectados)
                        if (!string.IsNullOrWhiteSpace(t.WBS))
                            _casa.DestajosTerminadosWBS.RemoveAll(x => x == t.WBS);
                }
            }

            // Persist if possible
            if (_casa != null && _inventarioService != null)
            {
                try
                {
                    var ci = new CasaInventario { Manzana = _casa.Manzana, Lote = _casa.Lote, DestajosTerminadosWBS = _casa.DestajosTerminadosWBS };
                    _inventarioService.ActualizarDestajosTerminados(ci);
                }
                catch { }
            }

            // Recalculate and refresh
            try { RecalculateProgressForRoots(treeListView1.Objects.Cast<Tarea>()); } catch { }
            try { treeListView1.RefreshObjects(afectados); } catch { treeListView1.RefreshObject(tarea); }

            UpdateGeneralProgress();
        }

        private void GenerateReportePdfToFile(Tarea tarea, System.Data.DataTable dtEntregados, string outputFile)
        {
            try
            {
                // Build PDF similar to ReporteDestajos sample
                var document = new PdfSharp.Pdf.PdfDocument();
                document.Info.Title = "Reporte de Destajos Terminados";
                var page = document.AddPage();
                var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);

                var fontTitulo = new PdfSharp.Drawing.XFont("Arial", 12, PdfSharp.Drawing.XFontStyle.Bold);
                var fontSub = new PdfSharp.Drawing.XFont("Arial", 10, PdfSharp.Drawing.XFontStyle.Regular);
                var fontTabla = new PdfSharp.Drawing.XFont("Arial", 9, PdfSharp.Drawing.XFontStyle.Regular);
                var fontTablaBold = new PdfSharp.Drawing.XFont("Arial", 9, PdfSharp.Drawing.XFontStyle.Bold);

                double y = 40;

                // Logo (try resources)
                try
                {
                    var res = DynamicSepticSystem.Properties.Resources.Logo;
                    using (var ms = new System.IO.MemoryStream())
                    {
                        res.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Position = 0;
                        var img = PdfSharp.Drawing.XImage.FromStream(ms);
                        gfx.DrawImage(img, 30, 20, 70, 40);
                    }
                }
                catch { }

                gfx.DrawString("DESARROLLADORA DE CASAS CAMANEY", fontTitulo, PdfSharp.Drawing.XBrushes.Black,
                    new PdfSharp.Drawing.XRect(0, y, page.Width, 20), PdfSharp.Drawing.XStringFormats.TopCenter);
                y += 20;
                gfx.DrawString("REPORTE DE DESTAJOS TERMINADOS", fontTitulo, PdfSharp.Drawing.XBrushes.Black,
                    new PdfSharp.Drawing.XRect(0, y, page.Width, 20), PdfSharp.Drawing.XStringFormats.TopCenter);
                y += 30;

                // Header
                gfx.DrawString("OBRA: ____________________________", fontSub, PdfSharp.Drawing.XBrushes.Black, 50, y);
                gfx.DrawString("Edificación: ____", fontSub, PdfSharp.Drawing.XBrushes.Black, 300, y);
                gfx.DrawString("Urbanización: ____", fontSub, PdfSharp.Drawing.XBrushes.Black, 430, y);
                y += 20;
                gfx.DrawString("RESIDENTE: ____________________________", fontSub, PdfSharp.Drawing.XBrushes.Black, 50, y);
                gfx.DrawString("FECHA: " + DateTime.Now.ToShortDateString(), fontSub, PdfSharp.Drawing.XBrushes.Black, 400, y);
                y += 20;
                gfx.DrawString("FOLIO: ____________", fontSub, PdfSharp.Drawing.XBrushes.Black, 400, y);
                y += 20;

                // Table layout
                double xStart = 40;
                double[] colWidths = { 80, 260, 100, 120 };
                string[] headers = { "N° destajo", "DESCRIPCIÓN DEL DESTAJO", "PRECIO", "OBSERVACIONES" };
                double tableTop = y + 10;
                double rowHeight = 20;

                double x = xStart;
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, PdfSharp.Drawing.XBrushes.LightGray, x, tableTop, colWidths[i], rowHeight);
                    gfx.DrawString(headers[i], fontTablaBold, PdfSharp.Drawing.XBrushes.Black,
                        new PdfSharp.Drawing.XRect(x + 3, tableTop + 5, colWidths[i], rowHeight), PdfSharp.Drawing.XStringFormats.TopLeft);
                    x += colWidths[i];
                }

                int filas = (dtEntregados != null) ? dtEntregados.Rows.Count : 0;
                for (int i = 0; i < filas; i++)
                {
                    x = xStart;
                    double rowY = tableTop + rowHeight * (i + 1);

                    var r = dtEntregados.Rows[i];
                    string clave = r.Table.Columns.Contains("Clave") ? r["Clave"].ToString() : string.Empty;
                    string desc = r.Table.Columns.Contains("Descripción") ? r["Descripción"].ToString() : r.Table.Columns.Contains("Descripcion") ? r["Descripcion"].ToString() : string.Empty;
                    string precio = r.Table.Columns.Contains("Importe") ? r["Importe"].ToString() : string.Empty;
                    // N° destajo -> usar tarea.WBS o clave
                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, x, rowY, colWidths[0], rowHeight);
                    gfx.DrawString(tarea.WBS ?? clave, fontTabla, PdfSharp.Drawing.XBrushes.Black, x + 5, rowY + 5);
                    x += colWidths[0];

                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, x, rowY, colWidths[1], rowHeight);
                    gfx.DrawString(desc.Length > 100 ? desc.Substring(0, 100) + "..." : desc, fontTabla, PdfSharp.Drawing.XBrushes.Black, new PdfSharp.Drawing.XRect(x + 3, rowY + 3, colWidths[1] - 6, rowHeight), PdfSharp.Drawing.XStringFormats.TopLeft);
                    x += colWidths[1];

                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, x, rowY, colWidths[2], rowHeight);
                    gfx.DrawString((string.IsNullOrWhiteSpace(precio) ? "-" : precio), fontTabla, PdfSharp.Drawing.XBrushes.Black, x + 5, rowY + 5);
                    x += colWidths[2];

                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, x, rowY, colWidths[3], rowHeight);
                }

                double totalY = tableTop + rowHeight * (filas + 1) + 5;
                gfx.DrawString("IMPORTE TOTAL", fontTablaBold, PdfSharp.Drawing.XBrushes.Black, xStart + 270, totalY);
                gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, xStart + 370, totalY - 5, 100, rowHeight);
                gfx.DrawString("$ -", fontTabla, PdfSharp.Drawing.XBrushes.Black, xStart + 380, totalY + 2);

                // Save to provided outputFile
                document.Save(outputFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generando PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class Tarea
    {
        public string ID { get; set; }
        public string WBS { get; set; }
        public string Nombre { get; set; }
        public string Costo { get; set; }
        public string Work { get; set; }

        // Campos adicionales para mostrar en columnas
        public string Duracion { get; set; }
        public string Inicio { get; set; }
        public string Fin { get; set; }
        public string TiempoReal { get; set; }
        public string Observaciones { get; set; }
        public bool Entregado { get; set; } // Nuevo campo para estado de entrega

        // Progress fields (immediate children)
        public int HijosCount { get; set; }
        public int HijosCompletados { get; set; }
        public int HijosPct { get; set; } // 0..100 integer percent, used by ProgressBarRenderer

        public Tarea Parent { get; set; }
        public List<Tarea> Children { get; set; } = new List<Tarea>();
    }
}
