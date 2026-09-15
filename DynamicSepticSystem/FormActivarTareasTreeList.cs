using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Formulario para activar/desactivar tareas del TreeList seg�n Manzana/Lote
    /// </summary>
    public partial class FormActivarTareasTreeList : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        private List<ItemTareaActivacion> itemsTareas = new List<ItemTareaActivacion>();
        private string manzanaActual = "";
        private string loteActual = "";
        private string prototipoActual = "";
        private string rutaActual = ""; // "RutaTuneraDestajo" o "RutaCalandraDestajo"

        // Cantidad ya SURTIDA (salidas de almacen) por clave y por nombre para la casa
        // actual. Sirve para colorear los insumos (verde = surtido, rojo = pendiente).
        // Por nombre cubre los insumos del destajo que no tienen clave de almacen.
        private Dictionary<string, decimal> _surtidoPorClave =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, decimal> _surtidoPorNombre =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        private ContextMenuStrip menuContextualManoObra;
        private ToolStripMenuItem menuItemAsignarNomina;
        private ContextMenuStrip menuContextualDestajo;
        private ToolStripMenuItem menuItemCambiarCuadrilla;
        private ToolStripMenuItem menuItemRegenerarPdf;
        private ToolStripMenuItem menuItemPropiedades;
        private ToolStripMenuItem menuItemDesactivar;
        private ToolStripMenuItem menuItemFinalizar;
        private ToolStripMenuItem menuItemReabrir;

        // Suprime el flujo de activación al refrescar el listado o al hacer rollback.
        private bool _suprimirFlujoActivacion = false;

        public FormActivarTareasTreeList()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            ConfigurarObjectListView();

            // Wire up events after InitializeComponent
            this.cmbManzana.SelectedIndexChanged += cmbManzana_SelectedIndexChanged;
            this.txtBuscar.TextChanged += txtBuscar_TextChanged;

            this.Load += FormActivarTareasTreeList_Load;

            // Conectar el panel lateral derecho (asistente de destajo)
            ConectarPanelGuia();
        }

        private void FormActivarTareasTreeList_Load(object sender, EventArgs e)
        {
            this.Text = "Activación de Destajos";
            panelStages.Paint += PanelStages_Paint;
            CargarManzanas();

            // Destajos híbrido: sustituye el panel paso-a-paso por la UI web servida
            // por el API. Si falla o WebView2 no está, deja el formulario clásico.
            // Se apaga con DestajosWeb=false en App.config.
            InicializarPanelWeb();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            string termino = txtBuscar.Text.Trim();
            if (string.IsNullOrEmpty(termino))
            {
                olvTareas.ModelFilter = null;
                olvTareas.UseFiltering = false;
            }
            else
            {
                olvTareas.UseFiltering = true;
                olvTareas.ModelFilter = new ModelFilter(o =>
                {
                    var item = o as ItemTareaActivacion;
                    if (item == null) return false;
                    return (item.Nombre ?? "").IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0
                        || (item.Descripcion ?? "").IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0
                        || (item.CuadrillaAsignada ?? "").IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0;
                });
            }
        }

        #region Carga de datos - Manzanas y Lotes

        private void CargarManzanas()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbManzana.Items.Clear();
                        while (reader.Read())
                        {
                            cmbManzana.Items.Add(reader["Manzana"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar manzanas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbManzana_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Lote FROM InventarioCasas WHERE Manzana = @m ORDER BY Lote";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", cmbManzana.SelectedItem.ToString());
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbLote.Items.Clear();
                            while (reader.Read())
                            {
                                cmbLote.Items.Add(reader["Lote"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar lotes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCargar_Click(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Por favor selecciona Manzana y Lote", "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            manzanaActual = cmbManzana.SelectedItem.ToString();
            loteActual = cmbLote.SelectedItem.ToString();

            prototipoActual = ObtenerPrototipo(manzanaActual, loteActual);
            if (string.IsNullOrEmpty(prototipoActual))
            {
                MessageBox.Show($"No se encontr� el prototipo para M{manzanaActual}-L{loteActual}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Determinar qu� ruta usar seg�n el prototipo
            rutaActual = prototipoActual.ToUpper().Contains("CALANDRA") ? "RutaCalandraDestajo" : "RutaTuneraDestajo";

            this.Text = $"Activación de Destajos — M{manzanaActual} L{loteActual} ({prototipoActual})";

            CargarTareas();
        }

        private string ObtenerPrototipo(string manzana, string lote)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region Configuraci�n de ObjectListView

        private void ConfigurarObjectListView()
        {
            // Convertir a TreeListView
            olvTareas.FullRowSelect = true;
            olvTareas.CellEditActivation = ObjectListView.CellEditActivateMode.None;
            olvTareas.UseAlternatingBackColors = true;
            olvTareas.AlternateRowBackColor = Color.FromArgb(240, 248, 255);
            olvTareas.CheckBoxes = true;
            olvTareas.CheckedAspectName = "Activa";

            // Configurar el TreeListView para mostrar la jerarqu�a
            olvTareas.CanExpandGetter = delegate(object x)
            {
                var item = x as ItemTareaActivacion;
                return item != null && item.Nivel < 2; // Los nodos de nivel 0 y 1 son expandibles
            };

            olvTareas.ChildrenGetter = delegate(object x)
            {
                var item = x as ItemTareaActivacion;
                if (item == null)
                    return new List<ItemTareaActivacion>();

                // Retornar los hijos directos de este nodo
                return itemsTareas.Where(i => i.ParentId == item.ID).ToList();
            };

            // Columna Checkbox (Activa)
            var colActiva = new OLVColumn("", "Activa")
            {
                Width = 30,
                IsEditable = false,
                CheckBoxes = true,
                Sortable = false
            };

            // Columna Contador
            var colContador = new OLVColumn("#", "Contador")
            {
                Width = 40,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                IsVisible = false
            };

            // Columna Nombre
            var colNombre = new OLVColumn("Nombre", "Nombre")
            {
                Width = 250,
                IsEditable = false,
                Sortable = false
            };

            // Columna Tipo
            var colTipo = new OLVColumn("Tipo", "Tipo")
            {
                Width = 100,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                IsVisible = false
            };

            // Columna Tipo Tarea
            var colTipoTarea = new OLVColumn("Tipo Tarea", "TipoTarea")
            {
                Width = 100,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false
            };

            // Columna Descripci�n
            var colDescripcion = new OLVColumn("Descripci�n", "Descripcion")
            {
                Width = 300,
                IsEditable = false,
                Sortable = false
            };

            olvTareas.AllColumns.AddRange(new[] { colActiva, colContador, colNombre, colTipo, colTipoTarea, colDescripcion });
            // Columna Cantidad
            var colCantidad = new OLVColumn("Cantidad", "Cantidad")
            {
                Width = 80,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                Sortable = false
            };

            // Columna Unidad
            var colUnidad = new OLVColumn("Unidad", "Unidad")
            {
                Width = 80,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false
            };

            // Columna Precio Unitario
            var colPrecioUnitario = new OLVColumn("Precio Unitario", "PrecioUnitario")
            {
                Width = 100,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                Sortable = false
            };

            // Columna Total
            var colTotal = new OLVColumn("Total", "Total")
            {
                Width = 100,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                Sortable = false
            };

            // Columna Cuadrilla asignada
            var colCuadrilla = new OLVColumn("Cuadrilla", "CuadrillaAsignada")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = obj =>
                {
                    var it = obj as ItemTareaActivacion;
                    return string.IsNullOrEmpty(it?.CuadrillaAsignada) ? "—" : it.CuadrillaAsignada;
                }
            };

            // Columna Estado Destajo (Pendiente / Activado / Finalizado)
            var colEstadoDestajo = new OLVColumn("Estado", "DesatajoActivado")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectGetter = obj =>
                {
                    var it = obj as ItemTareaActivacion;
                    if (it?.Nivel == 1)
                    {
                        if (it.Finalizado) return "■ Finalizado";
                        if (it.DesatajoActivado) return "✓ Activado";
                        return "◯ Desactivado";
                    }
                    return "";
                }
            };

            olvTareas.AllColumns.AddRange(new[] { colCantidad, colUnidad, colPrecioUnitario, colTotal, colCuadrilla, colEstadoDestajo });
            olvTareas.RebuildColumns();

            olvTareas.CellEditFinishing += (s, e) => { e.Cancel = true; };

            olvTareas.ItemChecked += OlvTareas_ItemChecked;
            olvTareas.FormatRow += OlvTareas_FormatRow;

            ConfigurarMenusContextuales();
        }

        private void OlvTareas_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var olvItem = e.Item as OLVListItem;
            var item = (olvItem != null ? olvItem.RowObject : olvTareas.GetModelObject(e.Item.Index)) as ItemTareaActivacion;
            if (item == null) return;

            bool estabaActivo = item.Activa;
            bool destajoEstabaActivado = item.DesatajoActivado;
            item.Activa = e.Item.Checked;

            if (_suprimirFlujoActivacion)
                return;

            // Solo el admin puede desactivar un destajo ya activado.
            if (!e.Item.Checked && item.Nivel == 1 && destajoEstabaActivado && !EsUsuarioAdmin())
            {
                MessageBox.Show(
                    "Solo el administrador puede desactivar un destajo ya activado.",
                    "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                _suprimirFlujoActivacion = true;
                try
                {
                    item.Activa = true;
                    e.Item.Checked = true;
                    olvTareas.RefreshObject(item);
                }
                finally
                {
                    _suprimirFlujoActivacion = false;
                }
                return;
            }

            // Activación manual de un destajo (Nivel 1) → flujo Cuadrilla + PDF
            if (e.Item.Checked && !estabaActivo && item.Nivel == 1)
            {
                BeginInvoke((Action)(() => EjecutarFlujoActivacionDestajo(item)));
            }
            else if (!e.Item.Checked && item.Nivel == 1)
            {
                // El check de admin ya se hizo arriba (líneas previas de este método)
                // antes de llegar aquí, así que no hace falta repetirlo como en los
                // Menu*_Click. DesactivarWeb hace su propio flujo de justificación
                // (vía el 400 del API) si el destajo ya liberó insumos.
                if (DestajosWebActivo)
                {
                    BeginInvoke((Action)(() => { _ = DesactivarWeb(item.ID); }));
                    ActualizarEstadisticas();
                    return;
                }

                // Si ya libero insumos, exige justificacion y registra excepcion.
                if (destajoEstabaActivado && !RegistrarExcepcionDesactivacionSiLiberado(item))
                {
                    _suprimirFlujoActivacion = true;
                    try
                    {
                        item.Activa = true;
                        e.Item.Checked = true;
                        olvTareas.RefreshObject(item);
                    }
                    finally { _suprimirFlujoActivacion = false; }
                    return;
                }

                // Al desactivar, limpia la cuadrilla asignada, finalización y el estado de activación
                item.CuadrillaAsignada = "";
                item.DesatajoActivado = false;
                item.Finalizado = false;
                item.FechaFinalizacion = null;
                item.FechaActivacion = null;
                olvTareas.RefreshObject(item);
            }

            ActualizarEstadisticas();
        }

        private ItemTareaActivacion LocalizarDestajoAncestro(ItemTareaActivacion hijo)
        {
            if (hijo == null) return null;
            var actual = hijo;
            int safety = 16;
            while (actual != null && actual.Nivel != 1 && safety-- > 0)
            {
                if (actual.ParentId == 0) return null;
                actual = itemsTareas.FirstOrDefault(x => x.ID == actual.ParentId);
            }
            return actual?.Nivel == 1 ? actual : null;
        }

        private static bool EsUsuarioAdmin()
        {
            return Global.EsAdmin;
        }

        private void ConfigurarMenusContextuales()
        {
            // ---- Menú para hijos Mano de Obra ----
            menuContextualManoObra = new ContextMenuStrip();
            menuItemAsignarNomina = new ToolStripMenuItem("Asignar Nómina...");
            menuItemAsignarNomina.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            menuItemAsignarNomina.Click += MenuItemAsignarNomina_Click;
            menuContextualManoObra.Items.Add(menuItemAsignarNomina);

            // ---- Menú para destajos (Nivel 1) ----
            menuContextualDestajo = new ContextMenuStrip();
            menuItemCambiarCuadrilla = new ToolStripMenuItem("Asignar / Cambiar Cuadrilla...");
            menuItemCambiarCuadrilla.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            menuItemCambiarCuadrilla.Click += MenuItemCambiarCuadrilla_Click;
            menuItemRegenerarPdf = new ToolStripMenuItem("Generar / Regenerar PDF");
            menuItemRegenerarPdf.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            menuItemRegenerarPdf.Click += MenuItemRegenerarPdf_Click;
            menuItemPropiedades = new ToolStripMenuItem("Propiedades...");
            menuItemPropiedades.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            menuItemPropiedades.Click += MenuItemPropiedades_Click;
            menuItemDesactivar = new ToolStripMenuItem("Desactivar destajo");
            menuItemDesactivar.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            menuItemDesactivar.ForeColor = Color.FromArgb(192, 57, 43);
            menuItemDesactivar.Click += MenuItemDesactivar_Click;
            menuItemFinalizar = new ToolStripMenuItem("Finalizar destajo");
            menuItemFinalizar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            menuItemFinalizar.ForeColor = Color.FromArgb(13, 71, 161);
            menuItemFinalizar.Click += MenuItemFinalizar_Click;
            menuItemReabrir = new ToolStripMenuItem("Reabrir destajo (volver a activado)");
            menuItemReabrir.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            menuItemReabrir.ForeColor = Color.FromArgb(243, 156, 18);
            menuItemReabrir.Click += MenuItemReabrir_Click;
            menuContextualDestajo.Items.Add(menuItemCambiarCuadrilla);
            menuContextualDestajo.Items.Add(new ToolStripSeparator());
            menuContextualDestajo.Items.Add(menuItemFinalizar);
            menuContextualDestajo.Items.Add(menuItemReabrir);
            menuContextualDestajo.Items.Add(menuItemRegenerarPdf);
            menuContextualDestajo.Items.Add(new ToolStripSeparator());
            menuContextualDestajo.Items.Add(menuItemPropiedades);
            menuContextualDestajo.Items.Add(new ToolStripSeparator());
            menuContextualDestajo.Items.Add(menuItemDesactivar);

            olvTareas.CellRightClick += OlvTareas_CellRightClick;
        }

        private void OlvTareas_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            var item = e.Model as ItemTareaActivacion;
            if (item == null) return;

            olvTareas.SelectedObject = item;

            if (item.Nivel == 1)
            {
                // Destajo: Asignar/Cambiar cuadrilla y PDF
                bool activado = item.DesatajoActivado;
                bool finalizado = item.Finalizado;

                menuItemPropiedades.Enabled = activado;
                menuItemPropiedades.Text = activado ? "Propiedades..." : "Propiedades... (destajo no activado)";

                menuItemDesactivar.Enabled = activado;
                menuItemDesactivar.Text = activado ? "Desactivar destajo" : "Desactivar destajo (no activado)";

                menuItemFinalizar.Enabled = activado && !finalizado;
                if (finalizado)
                    menuItemFinalizar.Text = "Finalizar destajo (ya finalizado)";
                else if (!activado)
                    menuItemFinalizar.Text = "Finalizar destajo (no activado)";
                else
                    menuItemFinalizar.Text = "Finalizar destajo";

                // Reabrir: sólo para destajos finalizados (lo restringe a admin el handler).
                menuItemReabrir.Visible = finalizado;
                menuItemReabrir.Enabled = finalizado;

                e.MenuStrip = menuContextualDestajo;
            }
            else if (item.TipoTareaEnum == TipoTarea.ManoDeObra)
            {
                // Hijo Mano de Obra: distribuir nómina
                e.MenuStrip = menuContextualManoObra;
            }
        }

        private void MenuItemCambiarCuadrilla_Click(object sender, EventArgs e)
        {
            var item = ItemSeleccionado();
            if (item == null || item.Nivel != 1) return;

            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
            {
                MessageBox.Show("Carga primero una casa antes de asignar cuadrilla.",
                    "Cuadrilla", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (item.Finalizado)
            {
                MessageBox.Show(
                    "No se puede cambiar la cuadrilla de un destajo ya finalizado.",
                    "Cuadrilla", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EjecutarFlujoActivacionDestajo(item, esActivacion: false);
        }

        private void MenuItemRegenerarPdf_Click(object sender, EventArgs e)
        {
            var item = ItemSeleccionado();
            if (item == null || item.Nivel != 1) return;

            // RegenerarPdfWeb (Web.cs) repite las mismas validaciones de abajo.
            if (DestajosWebActivo) { _ = RegenerarPdfWeb(item.ID); return; }

            if (!item.DesatajoActivado)
            {
                MessageBox.Show("Este destajo no está activado. Asegúrate de que tenga cuadrilla asignada y esté completamente configurado.",
                    "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(item.CuadrillaAsignada))
            {
                MessageBox.Show("El destajo no tiene cuadrilla asignada.",
                    "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            GenerarPdfDestajo(item);
        }

        private void MenuItemFinalizar_Click(object sender, EventArgs e)
        {
            var item = ItemSeleccionado();
            if (item == null || item.Nivel != 1) return;

            // FinalizarWeb (Web.cs) repite las mismas validaciones + confirmación de
            // abajo, así que se redirige antes de duplicar el diálogo de confirmación.
            if (DestajosWebActivo) { _ = FinalizarWeb(item.ID); return; }

            if (!item.DesatajoActivado)
            {
                MessageBox.Show(
                    "Sólo se puede finalizar un destajo previamente activado con cuadrilla.",
                    "Finalizar destajo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (item.Finalizado)
            {
                MessageBox.Show("Este destajo ya está finalizado.",
                    "Finalizar destajo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(item.CuadrillaAsignada))
            {
                MessageBox.Show("El destajo no tiene cuadrilla asignada.",
                    "Finalizar destajo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var rsp = MessageBox.Show(
                $"¿Finalizar el destajo \"{item.Nombre}\"?\n\n" +
                "Se generará el PDF de finalización y quedará habilitada la asignación de nómina.",
                "Finalizar destajo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rsp != DialogResult.Yes) return;

            item.Finalizado = true;
            item.FechaFinalizacion = DateTime.Now;

            PersistirActivacionDestajo(item);
            olvTareas.RefreshObject(item);
            ActualizarEstadisticas();

            GenerarPdfFinalizacionDestajo(item);
        }

        private void GenerarPdfFinalizacionDestajo(ItemTareaActivacion destajo)
        {
            if (destajo == null || destajo.Nivel != 1) return;

            try
            {
                var miembros = ObtenerMiembrosCuadrilla(destajo.CuadrillaAsignada);
                var hijos = itemsTareas.Where(i => i.ParentId == destajo.ID).ToList();

                string nombreSeguro = SanitizarNombreArchivo(destajo.Nombre);
                string nombreArchivo =
                    $"Finalizacion_M{manzanaActual}-L{loteActual}_{destajo.ID}_{nombreSeguro}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                string archivoTemp = Path.Combine(Path.GetTempPath(), nombreArchivo);
                CrearPdfDestajo(archivoTemp, destajo, miembros, hijos,
                    "ACTA DE FINALIZACIÓN", "Trabajos Terminados y Validados");

                GuardarPdfDestajoEnBD(destajo, nombreArchivo, archivoTemp);

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"Finalizacion_M{manzanaActual}-L{loteActual}_{destajo.Nombre}.pdf");

                using (var preview = new FormPdfPreview(archivoTemp, sugerido))
                {
                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generando el PDF de finalización:\n" + ex.Message,
                    "PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuItemDesactivar_Click(object sender, EventArgs e)
        {
            var item = ItemSeleccionado();
            if (item == null || item.Nivel != 1) return;

            // Se replica aquí el check de admin porque DesactivarWeb (llamado
            // directo, no vía DespacharMensaje) no lo hace por su cuenta — ese
            // chequeo normalmente lo pone DespacharMensaje antes de despachar.
            if (DestajosWebActivo)
            {
                if (!EsUsuarioAdmin())
                {
                    MessageBox.Show(
                        "Solo el administrador puede desactivar un destajo ya activado.",
                        "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _ = DesactivarWeb(item.ID);
                return;
            }

            if (!item.DesatajoActivado)
            {
                MessageBox.Show("Este destajo ya está desactivado.",
                    "Desactivar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!EsUsuarioAdmin())
            {
                MessageBox.Show(
                    "Solo el administrador puede desactivar un destajo ya activado.",
                    "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rsp = MessageBox.Show(
                $"¿Desactivar el destajo \"{item.Nombre}\"?\n\n" +
                "Se eliminará la cuadrilla asignada y se marcará como desactivado.",
                "Desactivar destajo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (rsp != DialogResult.Yes) return;

            // Si el destajo ya libero insumos al almacen, exige justificacion y
            // registra la excepcion (el stock NO se devuelve).
            if (!RegistrarExcepcionDesactivacionSiLiberado(item))
                return;

            _suprimirFlujoActivacion = true;
            try
            {
                item.Activa = false;
                item.CuadrillaAsignada = "";
                item.DesatajoActivado = false;
                item.Finalizado = false;
                item.FechaFinalizacion = null;

                var olvItem = olvTareas.ModelToItem(item);
                if (olvItem != null) olvItem.Checked = false;

                olvTareas.RefreshObject(item);
            }
            finally
            {
                _suprimirFlujoActivacion = false;
            }

            PersistirActivacionDestajo(item);
            ActualizarEstadisticas();
        }

        /// <summary>
        /// Deshace el progreso de un destajo finalizado, devolviéndolo al estado
        /// ACTIVADO (conserva su cuadrilla y fecha de activación). Sólo admin.
        /// Si el destajo tiene nómina asignada a su mano de obra, pregunta si
        /// borrarla o conservarla (los recibos ya emitidos nunca se eliminan).
        /// </summary>
        private void MenuItemReabrir_Click(object sender, EventArgs e)
        {
            var item = ItemSeleccionado();
            if (item == null || item.Nivel != 1) return;

            // Mismo motivo que en MenuItemDesactivar_Click: ReabrirWeb (llamado
            // directo) no valida admin por su cuenta, así que se replica aquí.
            if (DestajosWebActivo)
            {
                if (!EsUsuarioAdmin())
                {
                    MessageBox.Show(
                        "Sólo el administrador puede reabrir un destajo finalizado.",
                        "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _ = ReabrirWeb(item.ID);
                return;
            }

            if (!item.Finalizado)
            {
                MessageBox.Show("Sólo se puede reabrir un destajo que ya está finalizado.",
                    "Reabrir destajo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!EsUsuarioAdmin())
            {
                MessageBox.Show(
                    "Sólo el administrador puede reabrir un destajo finalizado.",
                    "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rsp = MessageBox.Show(
                $"¿Reabrir el destajo \"{item.Nombre}\"?\n\n" +
                "Volverá al estado ACTIVADO (conserva su cuadrilla) y podrás finalizarlo de nuevo.",
                "Reabrir destajo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rsp != DialogResult.Yes) return;

            // Detectar nómina asignada a los nodos de mano de obra de este destajo.
            var nodosConNomina = DetectarNodosConNominaAsignada(item);
            if (nodosConNomina.Count > 0)
            {
                var rspNom = MessageBox.Show(
                    "Este destajo tiene nómina asignada a su mano de obra.\n\n" +
                    "• Sí  → borrar la asignación de nómina al reabrir.\n" +
                    "• No  → conservar la asignación tal cual.\n\n" +
                    "(Los recibos ya emitidos no se eliminan en ningún caso.)",
                    "Nómina asignada", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (rspNom == DialogResult.Cancel) return;
                if (rspNom == DialogResult.Yes)
                {
                    // Si falla el borrado, abortamos para no dejar estado inconsistente.
                    if (!EliminarNominaAsignada(nodosConNomina))
                        return;
                }
            }

            item.Finalizado = false;
            item.FechaFinalizacion = null;
            // Permanece activado y con su cuadrilla asignada.

            PersistirActivacionDestajo(item);
            olvTareas.RefreshObject(item);
            ActualizarEstadisticas();
            ActualizarPanelGuia();
        }

        /// <summary>
        /// Devuelve los IDs de los nodos de mano de obra (hijos) del destajo que
        /// tienen una asignación de nómina guardada (NominaTareasAsignada).
        /// </summary>
        private List<int> DetectarNodosConNominaAsignada(ItemTareaActivacion destajo)
        {
            var resultado = new List<int>();
            if (destajo == null) return resultado;
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual)) return resultado;

            var hijosMano = itemsTareas
                .Where(i => i.ParentId == destajo.ID && i.TipoTareaEnum == TipoTarea.ManoDeObra)
                .ToList();

            foreach (var hijo in hijosMano)
            {
                try
                {
                    var asignacion = ApiClient.Get<AsignacionNominaApi>(
                        "/api/nomina/asignacion"
                        + "?manzana=" + Uri.EscapeDataString(manzanaActual)
                        + "&lote=" + Uri.EscapeDataString(loteActual)
                        + "&ruta=" + Uri.EscapeDataString(rutaActual ?? "")
                        + "&nodoId=" + hijo.ID);
                    if (asignacion != null && asignacion.Montos != null && asignacion.Montos.Count > 0)
                        resultado.Add(hijo.ID);
                }
                catch
                {
                    // Sin asignación o sin conexión: lo tratamos como "sin nómina".
                }
            }
            return resultado;
        }

        /// <summary>Borra vía API la asignación de nómina de los nodos indicados.</summary>
        private bool EliminarNominaAsignada(List<int> nodoIds)
        {
            if (nodoIds == null || nodoIds.Count == 0) return true;
            try
            {
                ApiClient.Post("/api/nomina/asignacion/eliminar", new EliminarAsignacionRequestApi
                {
                    Manzana = manzanaActual,
                    Lote = loteActual,
                    Ruta = rutaActual ?? "",
                    NodoIds = nodoIds
                });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo borrar la nómina asignada:\n" + ex.Message,
                    "Reabrir destajo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void MenuItemPropiedades_Click(object sender, EventArgs e)
        {
            var item = ItemSeleccionado();
            if (item == null || item.Nivel != 1) return;

            if (!item.DesatajoActivado)
            {
                MessageBox.Show(
                    "Las propiedades sólo están disponibles para destajos activados.",
                    "Propiedades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var hijos = itemsTareas.Where(i => i.ParentId == item.ID).ToList();
            using (var form = new FormPropiedadesDestajo(
                manzanaActual, loteActual, prototipoActual, rutaActual, item, hijos))
            {
                form.ShowDialog(this);
            }
        }

        private void MenuItemAsignarNomina_Click(object sender, EventArgs e)
        {
            var item = ItemSeleccionado();
            if (item == null) return;
            if (item.TipoTareaEnum != TipoTarea.ManoDeObra) return;

            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
            {
                MessageBox.Show("Carga primero una casa (Manzana / Lote) antes de asignar nómina.",
                    "Asignar Nómina", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Stage 3: la nómina sólo se puede asignar después de finalizar el destajo padre.
            var destajoPadre = LocalizarDestajoAncestro(item);
            if (destajoPadre == null || !destajoPadre.Finalizado)
            {
                MessageBox.Show(
                    "Para asignar la nómina primero debes finalizar el destajo padre " +
                    "(click derecho sobre el destajo → \"Finalizar destajo\").",
                    "Asignar Nómina", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (item.Total <= 0m)
            {
                MessageBox.Show("Esta tarea no tiene un total mayor a cero para distribuir.",
                    "Asignar Nómina", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new FormAsignarNomina(
                manzanaActual,
                loteActual,
                rutaActual,
                item.ID,
                item.Nombre,
                item.Total))
            {
                form.ShowDialog(this);
            }
        }

        private void OlvTareas_FormatRow(object sender, FormatRowEventArgs e)
        {
            var item = e.Model as ItemTareaActivacion;
            if (item == null) return;

            switch (item.Nivel)
            {
                case 0:
                    e.Item.BackColor = Color.FromArgb(220, 230, 245);
                    e.Item.ForeColor = Color.FromArgb(33, 47, 67);
                    e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                    break;
                case 1:
                    // Destajo: tonalidad según etapa
                    if (item.Finalizado)
                    {
                        // Azul: stage 2, finalizado
                        e.Item.BackColor = Color.FromArgb(187, 222, 251);
                    }
                    else if (item.DesatajoActivado && !string.IsNullOrEmpty(item.CuadrillaAsignada))
                    {
                        // Verde: stage 1, activado con cuadrilla
                        e.Item.BackColor = Color.FromArgb(200, 230, 201);
                    }
                    else if (!string.IsNullOrEmpty(item.CuadrillaAsignada))
                    {
                        // Amarillo suave: tiene cuadrilla pero no completamente activado
                        e.Item.BackColor = Color.FromArgb(255, 243, 224);
                    }
                    else
                    {
                        // Gris azulado: sin cuadrilla
                        e.Item.BackColor = Color.FromArgb(243, 247, 252);
                    }
                    e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                    break;
                case 2:
                    if (item.TipoTareaEnum == TipoTarea.Material)
                        e.Item.ForeColor = Color.FromArgb(192, 57, 43);
                    else if (item.TipoTareaEnum == TipoTarea.ManoDeObra)
                        e.Item.ForeColor = Color.FromArgb(39, 174, 96);
                    break;
            }
        }

        #endregion

        #region Carga de tareas

        private void CargarTareas()
        {
            _destajoGuia = null;
            _itemAccionOverride = null;
            _indiceVista = -1;
            itemsTareas.Clear();
            CargarTareasDesdeRuta();
            CargarActivacionesGuardadas();
            CargarSurtidoPorClave();

            // Obtener solo los nodos raíz (nivel 0) para mostrar el árbol
            var nodosRaiz = itemsTareas.Where(i => i.Nivel == 0).ToList();

            _suprimirFlujoActivacion = true;
            try
            {
                olvTareas.SetObjects(nodosRaiz);
                olvTareas.CollapseAll();
            }
            finally
            {
                _suprimirFlujoActivacion = false;
            }

            lblContextoCasa.Text = string.Format(
                "Casa M{0} L{1}  ·  Prototipo: {2}  ·  Ruta: {3}",
                manzanaActual, loteActual, prototipoActual, rutaActual);

            ActualizarEstadisticas();

            // Modo paso-a-paso (sin árbol): enfoca de una vez el destajo actual
            // para que el asistente muestre sus opciones sin tener que seleccionarlo.
            SeleccionarDestajoActual();
        }

        private void CargarTareasDesdeRuta()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Cargar todos los nodos del �rbol (Padre, Sub-Padre, Hijo)
                    // Se une con la tabla de columnas para obtener Cantidad, Unidad y PrecioUnitario
                    string sql = $@"
                        SELECT 
                            r.ID,
                            r.Nombre,
                            r.Descripcion,
                            r.Nivel,
                            r.Orden,
                            r.TipoTarea,
                            r.ParentId,
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad,
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad' THEN c.Valor END), '') AS Unidad,
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio' THEN c.Valor END), '0') AS PrecioUnitario,
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Clave' THEN c.Valor END), '') AS Clave
                        FROM {rutaActual} r
                        LEFT JOIN {rutaActual}_Columnas c ON r.ID = c.NodoID
                        GROUP BY r.ID, r.Nombre, r.Descripcion, r.Nivel, r.Orden, r.TipoTarea, r.ParentId
                        ORDER BY r.Nivel, r.Orden";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int contador = 0;
                            while (reader.Read())
                            {
                                int nivel = Convert.ToInt32(reader["Nivel"]);
                                int tipoTareaInt = reader["TipoTarea"] != System.DBNull.Value 
                                    ? Convert.ToInt32(reader["TipoTarea"]) 
                                    : 0;
                                TipoTarea tipoTarea = (TipoTarea)tipoTareaInt;
                                
                                int parentId = reader["ParentId"] != System.DBNull.Value 
                                    ? Convert.ToInt32(reader["ParentId"]) 
                                    : 0;

                                // Convertir valores desde string a decimal
                                decimal cantidad = 0;
                                if (!string.IsNullOrEmpty(reader["Cantidad"].ToString()))
                                {
                                    decimal.TryParse(reader["Cantidad"].ToString(), out cantidad);
                                }

                                string unidad = reader["Unidad"] != System.DBNull.Value
                                    ? reader["Unidad"].ToString()
                                    : "";

                                decimal precioUnitario = 0;
                                if (!string.IsNullOrEmpty(reader["PrecioUnitario"].ToString()))
                                {
                                    decimal.TryParse(reader["PrecioUnitario"].ToString(), out precioUnitario);
                                }

                                // Contar solo nodos de nivel 1 para el contador
                                if (nivel == 1)
                                    contador++;

                                var item = new ItemTareaActivacion
                                {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    ParentId = parentId,
                                    Nombre = reader["Nombre"].ToString(),
                                    Descripcion = reader["Descripcion"] != System.DBNull.Value ? reader["Descripcion"].ToString() : "",
                                    Clave = reader["Clave"] != System.DBNull.Value ? reader["Clave"].ToString().Trim() : "",
                                    Nivel = nivel,
                                    Contador = nivel == 1 ? contador : 0,
                                    Tipo = ObtenerTipoNodo(nivel),
                                    TipoTarea = ObtenerTipoTareaTexto(tipoTarea),
                                    TipoTareaEnum = tipoTarea,
                                    Activa = true, // Por defecto activa
                                    Cantidad = cantidad,
                                    Unidad = unidad,
                                    PrecioUnitario = precioUnitario
                                };

                                itemsTareas.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tareas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarActivacionesGuardadas()
        {
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Crear tabla si no existe + asegurar columnas
                    string sqlCheck = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivacionTareasRuta')
                        BEGIN
                            CREATE TABLE ActivacionTareasRuta (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Manzana NVARCHAR(10),
                                Lote NVARCHAR(10),
                                Prototipo NVARCHAR(50),
                                Ruta NVARCHAR(50),
                                NodoID INT,
                                NombreTarea NVARCHAR(200),
                                Activa BIT,
                                CuadrillaAsignada NVARCHAR(20) NULL,
                                DesatajoActivado BIT DEFAULT 0,
                                Finalizado BIT DEFAULT 0,
                                FechaFinalizacion DATETIME NULL,
                                FechaActualizacion DATETIME DEFAULT GETDATE()
                            );
                        END
                        IF NOT EXISTS (SELECT 1 FROM sys.columns
                                       WHERE Name = N'CuadrillaAsignada'
                                         AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
                        BEGIN
                            ALTER TABLE ActivacionTareasRuta ADD CuadrillaAsignada NVARCHAR(20) NULL;
                        END
                        IF NOT EXISTS (SELECT 1 FROM sys.columns
                                       WHERE Name = N'DesatajoActivado'
                                         AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
                        BEGIN
                            ALTER TABLE ActivacionTareasRuta ADD DesatajoActivado BIT DEFAULT 0;
                        END
                        IF NOT EXISTS (SELECT 1 FROM sys.columns
                                       WHERE Name = N'Finalizado'
                                         AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
                        BEGIN
                            ALTER TABLE ActivacionTareasRuta ADD Finalizado BIT DEFAULT 0;
                        END
                        IF NOT EXISTS (SELECT 1 FROM sys.columns
                                       WHERE Name = N'FechaFinalizacion'
                                         AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
                        BEGIN
                            ALTER TABLE ActivacionTareasRuta ADD FechaFinalizacion DATETIME NULL;
                        END
                        IF NOT EXISTS (SELECT 1 FROM sys.columns
                                       WHERE Name = N'FechaActivacion'
                                         AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
                        BEGIN
                            ALTER TABLE ActivacionTareasRuta ADD FechaActivacion DATETIME NULL;
                        END";
                    using (SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn))
                    {
                        cmdCheck.ExecuteNonQuery();
                    }

                    // Cargar activaciones guardadas
                    string sql = @"
                        SELECT NodoID, Activa, CuadrillaAsignada, DesatajoActivado,
                               ISNULL(Finalizado, 0) AS Finalizado, FechaFinalizacion,
                               FechaActivacion
                        FROM ActivacionTareasRuta
                        WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzanaActual);
                        cmd.Parameters.AddWithValue("@l", loteActual);
                        cmd.Parameters.AddWithValue("@ruta", rutaActual);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int nodoId = Convert.ToInt32(reader["NodoID"]);
                                bool activa = Convert.ToBoolean(reader["Activa"]);
                                string cuadrilla = reader["CuadrillaAsignada"] == DBNull.Value
                                    ? ""
                                    : reader["CuadrillaAsignada"].ToString();
                                bool desatajoActivado = reader["DesatajoActivado"] == DBNull.Value
                                    ? false
                                    : Convert.ToBoolean(reader["DesatajoActivado"]);
                                bool finalizado = reader["Finalizado"] == DBNull.Value
                                    ? false
                                    : Convert.ToBoolean(reader["Finalizado"]);
                                DateTime? fechaFin = reader["FechaFinalizacion"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(reader["FechaFinalizacion"]);
                                DateTime? fechaAct = reader["FechaActivacion"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(reader["FechaActivacion"]);

                                var item = itemsTareas.FirstOrDefault(i => i.ID == nodoId);
                                if (item != null)
                                {
                                    item.Activa = activa;
                                    item.CuadrillaAsignada = cuadrilla;
                                    item.DesatajoActivado = desatajoActivado;
                                    item.Finalizado = finalizado;
                                    item.FechaFinalizacion = fechaFin;
                                    item.FechaActivacion = fechaAct;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar activaciones: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Carga, por clave, la cantidad ya surtida (salidas de almacen) a la casa
        /// actual. Se usa para colorear los insumos (verde = surtido, rojo = pendiente).
        /// </summary>
        private void CargarSurtidoPorClave()
        {
            _surtidoPorClave = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            _surtidoPorNombre = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual)) return;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT ISNULL(Clave,'') AS Clave, ISNULL(Descripcion,'') AS Descripcion, SUM(Cantidad) AS Surtido
                        FROM dbo.SalidasAlmacen
                        WHERE LTRIM(RTRIM(Manzana)) = @m AND LTRIM(RTRIM(Lote)) = @l
                        GROUP BY ISNULL(Clave,''), ISNULL(Descripcion,'')", conn))
                    {
                        cmd.Parameters.AddWithValue("@m", (manzanaActual ?? "").Trim());
                        cmd.Parameters.AddWithValue("@l", (loteActual ?? "").Trim());
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                string clave = (rd["Clave"]?.ToString() ?? "").Trim();
                                string nombre = (rd["Descripcion"]?.ToString() ?? "").Trim();
                                decimal s = rd["Surtido"] != DBNull.Value ? Convert.ToDecimal(rd["Surtido"]) : 0m;
                                if (clave.Length > 0)
                                {
                                    _surtidoPorClave.TryGetValue(clave, out var a);
                                    _surtidoPorClave[clave] = a + s;
                                }
                                if (nombre.Length > 0)
                                {
                                    _surtidoPorNombre.TryGetValue(nombre, out var b);
                                    _surtidoPorNombre[nombre] = b + s;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Sin datos de surtido: se tratara todo como pendiente.
            }
        }

        /// <summary>
        /// Cantidad surtida (a la casa actual) para un insumo. Cuando el insumo del
        /// destajo no tiene clave de almacen, se coteja por nombre (Descripcion).
        /// </summary>
        private decimal SurtidoDeInsumo(string clave, string nombre)
        {
            if (!string.IsNullOrWhiteSpace(clave) && _surtidoPorClave != null
                && _surtidoPorClave.TryGetValue(clave.Trim(), out var v))
                return v;
            if (!string.IsNullOrWhiteSpace(nombre) && _surtidoPorNombre != null
                && _surtidoPorNombre.TryGetValue(nombre.Trim(), out var w))
                return w;
            return 0m;
        }

        private string ObtenerTipoNodo(int nivel)
        {
            switch (nivel)
            {
                case 0: return "Padre";
                case 1: return "Sub-Padre";
                case 2: return "Hijo";
                default: return $"Nivel {nivel}";
            }
        }

        private string ObtenerTipoTareaTexto(TipoTarea tipoTarea)
        {
            switch (tipoTarea)
            {
                case TipoTarea.Material: return "Material";
                case TipoTarea.ManoDeObra: return "Mano de Obra";
                default: return "-";
            }
        }

        #endregion

        #region Actualizaci�n de estad�sticas

        // Nombres de las 4 etapas (cada una representa un 25% del avance económico).
        private static readonly string[] StagesNombres =
            { "Obra Negra", "Albañilería", "Acabados", "Acabados Finales" };

        // Estado actual de la barra de etapas (lo consume PanelStages_Paint).
        private int _stageIdx = 0;
        private double _stagePct = 0d;

        private void ActualizarEstadisticas()
        {
            // Avance ECONÓMICO por destajos finalizados: lo "ganado" es el importe
            // (Insumos + Mano de Obra) de los Nivel 2 cuyo destajo padre ya está finalizado.
            var finIds = new HashSet<int>(
                itemsTareas.Where(i => i.Nivel == 1 && i.Finalizado).Select(i => i.ID));

            decimal totalEco = itemsTareas.Where(i => i.Nivel == 2).Sum(i => i.Total);
            var hijosFin = itemsTareas.Where(i => i.Nivel == 2 && finIds.Contains(i.ParentId)).ToList();
            decimal insumosFin = hijosFin.Where(i => i.TipoTareaEnum == TipoTarea.Material).Sum(i => i.Total);
            decimal manoFin = hijosFin.Where(i => i.TipoTareaEnum == TipoTarea.ManoDeObra).Sum(i => i.Total);
            decimal ganado = insumosFin + manoFin;

            double pct = totalEco > 0m ? (double)(ganado / totalEco) : 0d;
            if (pct < 0d) pct = 0d; else if (pct > 1d) pct = 1d;
            int stageIdx = Math.Min(3, (int)(pct * 4));

            int totalDestajos = itemsTareas.Count(i => i.Nivel == 1);
            int finalizados = finIds.Count;

            lblEstadisticas.Text = string.Format(
                "Avance económico: Insumos {0} + M.O. {1} = {2} de {3}   ·   Destajos terminados: {4} de {5}",
                insumosFin.ToString("C2", CultureInfo.CurrentCulture),
                manoFin.ToString("C2", CultureInfo.CurrentCulture),
                ganado.ToString("C2", CultureInfo.CurrentCulture),
                totalEco.ToString("C2", CultureInfo.CurrentCulture),
                finalizados,
                totalDestajos);

            progressBarActivacion.Maximum = 100;
            progressBarActivacion.Value = Math.Max(0, Math.Min(100, (int)Math.Round(pct * 100)));

            lblPorcentaje.Text = totalEco > 0m
                ? $"{pct:P0}  ·  Etapa: {StagesNombres[stageIdx]}"
                : "—";

            _stageIdx = stageIdx;
            _stagePct = pct;
            panelStages.Invalidate();

            // Reconstruir el control paso-a-paso con los estados actuales.
            RefrescarPasos();
        }

        /// <summary>
        /// Dibuja la barra de 4 etapas (Obra Negra / Albañilería / Acabados / Acabados
        /// Finales). Las etapas ya superadas se pintan en verde, la actual resaltada en
        /// azul acento y las futuras en gris. Cada etapa equivale a un 25% del avance.
        /// </summary>
        private void PanelStages_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color exito = Color.FromArgb(39, 174, 96);
            Color acento = Color.FromArgb(52, 152, 219);
            Color gris = Color.FromArgb(214, 221, 230);
            Color textoFuturo = Color.FromArgb(127, 140, 141);

            int n = StagesNombres.Length;
            int gap = 6;
            int w = panelStages.Width;
            int h = panelStages.Height;
            int segW = (w - gap * (n - 1)) / n;

            using (var fuente = new Font("Segoe UI Semibold", 8.25F, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i < n; i++)
                {
                    int x = i * (segW + gap);
                    var rect = new Rectangle(x, 1, segW, h - 2);

                    Color fondo = i < _stageIdx ? exito : (i == _stageIdx ? acento : gris);
                    bool actualOSuperado = i <= _stageIdx;
                    Color colorTexto = actualOSuperado ? Color.White : textoFuturo;

                    using (var b = new SolidBrush(fondo))
                        g.FillRectangle(b, rect);

                    string etiqueta = (i + 1) + ". " + StagesNombres[i];
                    using (var bt = new SolidBrush(colorTexto))
                        g.DrawString(etiqueta, fuente, bt, rect, sf);
                }
            }
        }

        #endregion

        #region Botones de acci�n

        private void btnMarcarTodos_Click(object sender, EventArgs e)
        {
            _suprimirFlujoActivacion = true;
            try
            {
                foreach (var item in itemsTareas) item.Activa = true;
                olvTareas.RefreshObjects(itemsTareas);
            }
            finally { _suprimirFlujoActivacion = false; }
            ActualizarEstadisticas();
        }

        private void btnDesmarcarTodos_Click(object sender, EventArgs e)
        {
            _suprimirFlujoActivacion = true;
            try
            {
                foreach (var item in itemsTareas)
                {
                    item.Activa = false;
                    if (item.Nivel == 1)
                    {
                        item.CuadrillaAsignada = "";
                        item.DesatajoActivado = false;
                    }
                }
                olvTareas.RefreshObjects(itemsTareas);
            }
            finally { _suprimirFlujoActivacion = false; }
            ActualizarEstadisticas();
        }

        private void btnMarcarPorNivel_Click(object sender, EventArgs e)
        {
            using (var dialog = new DialogSeleccionarNivel())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int nivelSeleccionado = dialog.NivelSeleccionado;
                    _suprimirFlujoActivacion = true;
                    try
                    {
                        foreach (var item in itemsTareas.Where(i => i.Nivel == nivelSeleccionado))
                            item.Activa = true;
                        olvTareas.RefreshObjects(itemsTareas);
                    }
                    finally { _suprimirFlujoActivacion = false; }
                    ActualizarEstadisticas();
                }
            }
        }

        private void btnDesmarcarPorNivel_Click(object sender, EventArgs e)
        {
            using (var dialog = new DialogSeleccionarNivel())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int nivelSeleccionado = dialog.NivelSeleccionado;
                    _suprimirFlujoActivacion = true;
                    try
                    {
                        foreach (var item in itemsTareas.Where(i => i.Nivel == nivelSeleccionado))
                        {
                            item.Activa = false;
                            if (item.Nivel == 1)
                            {
                                item.CuadrillaAsignada = "";
                                item.DesatajoActivado = false;
                            }
                        }
                        olvTareas.RefreshObjects(itemsTareas);
                    }
                    finally { _suprimirFlujoActivacion = false; }
                    ActualizarEstadisticas();
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
            {
                MessageBox.Show("Por favor selecciona una casa primero", "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // POST /api/destajos/guardar es el mismo reemplazo en bloque que el
            // codigo de abajo, pero con ValidarRuta, permiso destajos.editar y
            // transaccion — lo que faltaba al escribir directo desde este boton.
            if (DestajosWebActivo) { _ = GuardarWeb(); return; }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Limpiar activaciones anteriores para esta casa
                    string sqlDelete = @"
                        DELETE FROM ActivacionTareasRuta 
                        WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta";

                    using (SqlCommand cmdDelete = new SqlCommand(sqlDelete, conn))
                    {
                        cmdDelete.Parameters.AddWithValue("@m", manzanaActual);
                        cmdDelete.Parameters.AddWithValue("@l", loteActual);
                        cmdDelete.Parameters.AddWithValue("@ruta", rutaActual);
                        cmdDelete.ExecuteNonQuery();
                    }

                    // Guardar nuevas activaciones
                    foreach (var item in itemsTareas)
                    {
                        string sqlInsert = @"
                            INSERT INTO ActivacionTareasRuta
                            (Manzana, Lote, Prototipo, Ruta, NodoID, NombreTarea, Activa, CuadrillaAsignada, DesatajoActivado,
                             Finalizado, FechaFinalizacion, FechaActualizacion, FechaActivacion)
                            VALUES (@m, @l, @proto, @ruta, @nodoId, @nombre, @activa, @cuadrilla, @desatActivado,
                                    @finalizado, @fechaFin, GETDATE(), @fechaAct)";

                        // Si está activado y no tenía FechaActivacion, asignar ahora
                        if (item.DesatajoActivado && !item.FechaActivacion.HasValue)
                            item.FechaActivacion = DateTime.Now;
                        // Si se desactivó, limpiar la fecha
                        if (!item.DesatajoActivado)
                            item.FechaActivacion = null;

                        using (SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn))
                        {
                            cmdInsert.Parameters.AddWithValue("@m", manzanaActual);
                            cmdInsert.Parameters.AddWithValue("@l", loteActual);
                            cmdInsert.Parameters.AddWithValue("@proto", prototipoActual ?? (object)System.DBNull.Value);
                            cmdInsert.Parameters.AddWithValue("@ruta", rutaActual);
                            cmdInsert.Parameters.AddWithValue("@nodoId", item.ID);
                            cmdInsert.Parameters.AddWithValue("@nombre", item.Nombre ?? "");
                            cmdInsert.Parameters.AddWithValue("@activa", item.Activa);
                            cmdInsert.Parameters.AddWithValue("@cuadrilla",
                                string.IsNullOrEmpty(item.CuadrillaAsignada)
                                    ? (object)System.DBNull.Value
                                    : item.CuadrillaAsignada);
                            cmdInsert.Parameters.AddWithValue("@desatActivado", item.DesatajoActivado);
                            cmdInsert.Parameters.AddWithValue("@finalizado", item.Finalizado);
                            cmdInsert.Parameters.AddWithValue("@fechaFin",
                                item.FechaFinalizacion.HasValue
                                    ? (object)item.FechaFinalizacion.Value
                                    : (object)System.DBNull.Value);
                            cmdInsert.Parameters.AddWithValue("@fechaAct",
                                item.FechaActivacion.HasValue
                                    ? (object)item.FechaActivacion.Value
                                    : (object)System.DBNull.Value);

                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show($"? Configuraci�n guardada para M{manzanaActual}-L{loteActual}", 
                    "�xito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Rama web de btnGuardar_Click: mismo reemplazo en bloque, vía
        /// POST /api/destajos/guardar (DestajosController.cs, "reemplazo en bloque
        /// (btnGuardar de hoy)").</summary>
        private async Task GuardarWeb()
        {
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
            {
                MessageBox.Show("Por favor selecciona una casa primero", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var nodos = itemsTareas.Select(item => new
                {
                    NodoId = item.ID,
                    Nombre = item.Nombre ?? "",
                    Activa = item.Activa,
                    CuadrillaAsignada = item.CuadrillaAsignada,
                    DesatajoActivado = item.DesatajoActivado,
                    Finalizado = item.Finalizado,
                    FechaActivacion = item.FechaActivacion,
                    FechaFinalizacion = item.FechaFinalizacion
                }).ToList();

                await Task.Run(() => ApiClient.Post("/api/destajos/guardar", new
                {
                    Manzana = manzanaActual,
                    Lote = loteActual,
                    Ruta = rutaActual,
                    Prototipo = prototipoActual,
                    Nodos = nodos
                }));
                await RefrescarArbol();

                MessageBox.Show($"Configuración guardada para M{manzanaActual}-L{loteActual}",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ManejarErrorApi(ex, "No se pudo guardar la configuración"); }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Flujo Activación → Cuadrilla → PDF

        private void EjecutarFlujoActivacionDestajo(ItemTareaActivacion destajo, bool esActivacion = true)
        {
            if (destajo == null) return;

            // Con DestajosWeb activo, "Ver árbol completo" reutiliza este mismo
            // control (marcoArbol/olvTareas) re-parentado a un diálogo — sin este
            // gate, activar/cambiar cuadrilla desde ahí escribía por SQL directo en
            // paralelo al panel web, saltándose ValidarRuta/RequierePermiso/transacción.
            // ActivarWeb (FormActivarTareasTreeList.Web.cs) ya cubre activar y
            // cambiar-cuadrilla con el mismo diálogo de cuadrilla + PDF.
            if (DestajosWebActivo)
            {
                EjecutarFlujoActivacionDestajoWeb(destajo);
                return;
            }

            using (var formCuadrilla = new FormAsignarCuadrilla())
            {
                var dr = formCuadrilla.ShowDialog(this);
                if (dr != DialogResult.OK ||
                    formCuadrilla.CuadrillaAsignada == null ||
                    string.IsNullOrEmpty(formCuadrilla.CuadrillaAsignada.CodigoCuadrilla))
                {
                    if (esActivacion)
                    {
                        // Revertir activación
                        _suprimirFlujoActivacion = true;
                        try
                        {
                            destajo.Activa = false;
                            destajo.CuadrillaAsignada = "";
                            destajo.DesatajoActivado = false;
                            destajo.Finalizado = false;
                            destajo.FechaFinalizacion = null;
                            olvTareas.RefreshObject(destajo);
                        }
                        finally { _suprimirFlujoActivacion = false; }
                        ActualizarEstadisticas();
                    }
                    return;
                }

                destajo.Activa = true;
                destajo.CuadrillaAsignada = formCuadrilla.CuadrillaAsignada.CodigoCuadrilla;
                destajo.DesatajoActivado = true; // Marcar como activado al asignar cuadrilla
                if (!destajo.FechaActivacion.HasValue)
                    destajo.FechaActivacion = DateTime.Now;
            }

            // Activar SOLO asigna cuadrilla + PDF + marca el destajo como activado.
            // El surtido de materiales se hace por separado en Almacén → Salidas
            // (allí aparecen únicamente los insumos de destajos ya activados con
            // pendiente). Activar ya NO libera insumos ni toca el stock.
            PersistirActivacionDestajo(destajo);
            olvTareas.RefreshObject(destajo);
            ActualizarEstadisticas();

            GenerarPdfDestajo(destajo);
        }

        /// <summary>Rama web de EjecutarFlujoActivacionDestajo: activar y cambiar-cuadrilla
        /// son el mismo endpoint (ActivarWeb ya maneja ambos casos, ver el "case" de
        /// DespacharMensaje en FormActivarTareasTreeList.Web.cs). RefrescarArbol se llama
        /// dos veces a propósito: ActivarWeb ya la llama si tuvo éxito, pero si el usuario
        /// cancela el diálogo de cuadrilla, ActivarWeb vuelve sin refrescar y el checkbox
        /// se queda visualmente desincronizado — esta segunda llamada resincroniza siempre.</summary>
        private async void EjecutarFlujoActivacionDestajoWeb(ItemTareaActivacion destajo)
        {
            await ActivarWeb(destajo.ID);
            await RefrescarArbol();
        }

        private void PersistirActivacionDestajo(ItemTareaActivacion destajo)
        {
            if (destajo == null) return;
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual)) return;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (var cmdDel = new SqlCommand(@"
                        DELETE FROM ActivacionTareasRuta
                        WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta AND NodoID = @nodo", conn))
                    {
                        cmdDel.Parameters.AddWithValue("@m", manzanaActual);
                        cmdDel.Parameters.AddWithValue("@l", loteActual);
                        cmdDel.Parameters.AddWithValue("@ruta", rutaActual);
                        cmdDel.Parameters.AddWithValue("@nodo", destajo.ID);
                        cmdDel.ExecuteNonQuery();
                    }

                    // Asegurar FechaActivacion coherente con DesatajoActivado
                    if (destajo.DesatajoActivado && !destajo.FechaActivacion.HasValue)
                        destajo.FechaActivacion = DateTime.Now;
                    if (!destajo.DesatajoActivado)
                        destajo.FechaActivacion = null;

                    using (var cmdIns = new SqlCommand(@"
                        INSERT INTO ActivacionTareasRuta
                        (Manzana, Lote, Prototipo, Ruta, NodoID, NombreTarea, Activa, CuadrillaAsignada, DesatajoActivado,
                         Finalizado, FechaFinalizacion, FechaActualizacion, FechaActivacion)
                        VALUES (@m, @l, @proto, @ruta, @nodo, @nombre, @activa, @cuadrilla, @desatActivado,
                                @finalizado, @fechaFin, GETDATE(), @fechaAct)", conn))
                    {
                        cmdIns.Parameters.AddWithValue("@m", manzanaActual);
                        cmdIns.Parameters.AddWithValue("@l", loteActual);
                        cmdIns.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
                        cmdIns.Parameters.AddWithValue("@ruta", rutaActual);
                        cmdIns.Parameters.AddWithValue("@nodo", destajo.ID);
                        cmdIns.Parameters.AddWithValue("@nombre", destajo.Nombre ?? "");
                        cmdIns.Parameters.AddWithValue("@activa", destajo.Activa);
                        cmdIns.Parameters.AddWithValue("@cuadrilla",
                            string.IsNullOrEmpty(destajo.CuadrillaAsignada)
                                ? (object)DBNull.Value
                                : destajo.CuadrillaAsignada);
                        cmdIns.Parameters.AddWithValue("@desatActivado", destajo.DesatajoActivado);
                        cmdIns.Parameters.AddWithValue("@finalizado", destajo.Finalizado);
                        cmdIns.Parameters.AddWithValue("@fechaFin",
                            destajo.FechaFinalizacion.HasValue
                                ? (object)destajo.FechaFinalizacion.Value
                                : DBNull.Value);
                        cmdIns.Parameters.AddWithValue("@fechaAct",
                            destajo.FechaActivacion.HasValue
                                ? (object)destajo.FechaActivacion.Value
                                : DBNull.Value);
                        cmdIns.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar la activación del destajo:\n" + ex.Message,
                    "Activación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GenerarPdfDestajo(ItemTareaActivacion destajo)
        {
            if (destajo == null || destajo.Nivel != 1) return;
            
            // Validar que el destajo esté activado y tenga cuadrilla
            if (!destajo.DesatajoActivado || string.IsNullOrEmpty(destajo.CuadrillaAsignada))
            {
                MessageBox.Show("El destajo no está completamente activado. Debe tener cuadrilla asignada.",
                    "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var miembros = ObtenerMiembrosCuadrilla(destajo.CuadrillaAsignada);
                var hijos = itemsTareas.Where(i => i.ParentId == destajo.ID).ToList();

                string nombreSeguro = SanitizarNombreArchivo(destajo.Nombre);
                string nombreArchivo =
                    $"Destajo_M{manzanaActual}-L{loteActual}_{destajo.ID}_{nombreSeguro}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                string archivoTemp = Path.Combine(Path.GetTempPath(), nombreArchivo);
                CrearPdfDestajo(archivoTemp, destajo, miembros, hijos,
                    "ASIGNACIÓN DE DESTAJO", "Asignación de Cuadrilla y Trabajos");

                GuardarPdfDestajoEnBD(destajo, nombreArchivo, archivoTemp);

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"Asignacion_M{manzanaActual}-L{loteActual}_{destajo.Nombre}.pdf");

                using (var preview = new FormPdfPreview(archivoTemp, sugerido))
                {
                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generando el PDF:\n" + ex.Message, "PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<MiembroResumen> ObtenerMiembrosCuadrilla(string codigoCuadrilla)
        {
            var lista = new List<MiembroResumen>();
            if (string.IsNullOrEmpty(codigoCuadrilla)) return lista;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT m.Nombre, m.Rol, m.EsJefe, m.Telefono, t.ClaveTrabajador
                        FROM MiembrosCuadrilla m
                        LEFT JOIN TRABAJADORES t ON t.IdTrabajador = m.IdTrabajador
                        WHERE m.CodigoCuadrilla = @codigo
                        ORDER BY m.EsJefe DESC, m.Nombre", conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigoCuadrilla);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new MiembroResumen
                                {
                                    Clave = reader["ClaveTrabajador"] == DBNull.Value ? "" : reader["ClaveTrabajador"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    Rol = reader["Rol"].ToString(),
                                    EsJefe = Convert.ToBoolean(reader["EsJefe"]),
                                    Telefono = reader["Telefono"] == DBNull.Value ? "" : reader["Telefono"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                // sin miembros: el PDF se imprimirá igualmente con un mensaje.
            }
            return lista;
        }

        private void CrearPdfDestajo(
            string archivo,
            ItemTareaActivacion destajo,
            List<MiembroResumen> miembros,
            List<ItemTareaActivacion> hijos,
            string tituloPrincipal = "REPORTE DE DESTAJO",
            string subtituloPdf = "Asignación de Cuadrilla y Trabajos")
        {
            var doc = new PdfDocument();
            doc.Info.Title = tituloPrincipal;
            doc.Info.Author = "Pilaris";

            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            var gfx = XGraphics.FromPdfPage(page);

            // === DEFINIR FUENTES ===
            var fuenteSubtitulo = new XFont("Arial", 12, XFontStyle.Bold);
            var fuenteSeccion = new XFont("Arial", 11, XFontStyle.Bold);
            var fuenteNormal = new XFont("Arial", 10, XFontStyle.Regular);
            var fuenteEncabezado = new XFont("Arial", 9, XFontStyle.Bold);
            var fuentePequena = new XFont("Arial", 8, XFontStyle.Regular);
            var fuenteMuyPequena = new XFont("Arial", 7, XFontStyle.Regular);

            // === DEFINIR COLORES ===
            var colorPrincipal = XColor.FromArgb(11, 61, 145);      // Azul marino
            var colorSecundario = XColor.FromArgb(30, 136, 229);    // Azul claro
            var colorAcento = XColor.FromArgb(255, 152, 0);         // Ámbar (acentos)
            var colorBorde = XColor.FromArgb(214, 220, 228);        // Gris azulado suave
            var colorTexto = XColor.FromArgb(33, 33, 33);          // Gris muy oscuro
            var colorGrisClaro = XColor.FromArgb(238, 243, 250);   // Azul-gris muy claro

            var brochaPrincipal = new XSolidBrush(colorPrincipal);
            var brochaSecundario = new XSolidBrush(colorSecundario);
            var brochaAcento = new XSolidBrush(colorAcento);
            var brochaTexto = new XSolidBrush(colorTexto);
            var brochaBlanca = XBrushes.White;
            var brochaGrisClaro = new XSolidBrush(colorGrisClaro);

            var penBorde = new XPen(colorBorde, 0.6);
            var penPrincipal = new XPen(colorPrincipal, 1.5);
            var penFino = new XPen(colorBorde, 0.3);

            double margenIzq = 12;
            double margenDer = 12;
            double margenSup = 10;
            double ancho = page.Width - margenIzq - margenDer;
            double y = margenSup;
            double espacioSeccion = 6;

            // === ENCABEZADO PRINCIPAL ===
            DrawTarjeta(gfx, margenIzq, y, ancho, 40, 0, brochaPrincipal, null, null, 4);

            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var logo = XImage.FromStream(ms);
                    gfx.DrawImage(logo, margenIzq + 4, y + 2, 32, 32);
                }
            }
            catch { }

            gfx.DrawString("CALANDRIA RESIDENCIAL", fuenteSubtitulo, brochaBlanca,
                new XRect(margenIzq + 38, y + 2, ancho - 38, 12), XStringFormats.TopLeft);
            gfx.DrawRectangle(brochaAcento, margenIzq + 38, y + 13, 26, 1.4);
            gfx.DrawString(tituloPrincipal, fuenteSeccion, brochaBlanca,
                new XRect(margenIzq + 38, y + 15, ancho - 38, 11), XStringFormats.TopLeft);
            gfx.DrawString(subtituloPdf, fuentePequena, brochaBlanca,
                new XRect(margenIzq + 38, y + 27, ancho - 38, 8), XStringFormats.TopLeft);

            y += 42;
            y += espacioSeccion + 2;

            // === INFORMACIÓN DE LA CASA (3 columnas) ===
            double colCasaAncho = (ancho / 3) - 1.5;
            double xCasa = margenIzq;
            double altoCasa = 32;

            for (int i = 0; i < 3; i++)
            {
                if (i > 0) xCasa += colCasaAncho + 1.5;

                DrawTarjeta(gfx, xCasa, y, colCasaAncho, altoCasa, 0, brochaGrisClaro, null, penBorde, 3);
                var pathCasa = CrearRectRedondeado(xCasa, y, colCasaAncho, altoCasa, 3);
                gfx.Save();
                gfx.IntersectClip(pathCasa);
                gfx.DrawRectangle(brochaSecundario, xCasa, y, 2.2, altoCasa);
                gfx.Restore();

                string etiqueta = i == 0 ? "MANZANA" : (i == 1 ? "LOTE" : "PROTOTIPO");
                string valor = i == 0 ? $"M{manzanaActual}" : (i == 1 ? $"L{loteActual}" : (prototipoActual ?? "-"));

                gfx.DrawString(etiqueta, fuenteEncabezado, brochaSecundario,
                    new XRect(xCasa + 3, y + 2, colCasaAncho - 6, 8), XStringFormats.TopLeft);
                gfx.DrawString(valor, fuenteSubtitulo, brochaPrincipal,
                    new XRect(xCasa + 3, y + 13, colCasaAncho - 6, 16), XStringFormats.TopCenter);
            }

            y += altoCasa + espacioSeccion + 2;

            // === DESTAJO Y CUADRILLA (2 columnas) ===
            double colDestajo = (ancho / 2) - 1;
            double altoDestajo = 50;

            // DESTAJO
            DrawTarjeta(gfx, margenIzq, y, colDestajo, 16, altoDestajo - 16, brochaSecundario, brochaGrisClaro, penBorde, 3);
            gfx.DrawString("DESTAJO", fuenteSeccion, brochaBlanca,
                new XRect(margenIzq + 3, y + 1, colDestajo - 6, 13), XStringFormats.TopLeft);

            gfx.DrawString($"ID: {destajo.ID}", fuenteNormal, brochaTexto,
                new XRect(margenIzq + 4, y + 20, colDestajo - 8, 8), XStringFormats.TopLeft);
            gfx.DrawString($"Nombre: {Truncar(destajo.Nombre ?? "-", 32)}", fuenteNormal, brochaTexto,
                new XRect(margenIzq + 4, y + 30, colDestajo - 8, 8), XStringFormats.TopLeft);

            // CUADRILLA
            double xCuadrilla = margenIzq + colDestajo + 1;
            DrawTarjeta(gfx, xCuadrilla, y, colDestajo, 16, altoDestajo - 16, brochaSecundario, brochaGrisClaro, penBorde, 3);
            gfx.DrawString("CUADRILLA ASIGNADA", fuenteSeccion, brochaBlanca,
                new XRect(xCuadrilla + 3, y + 1, colDestajo - 6, 13), XStringFormats.TopLeft);

            gfx.DrawString($"Código: {destajo.CuadrillaAsignada ?? "-"}", fuenteNormal, brochaTexto,
                new XRect(xCuadrilla + 4, y + 20, colDestajo - 8, 8), XStringFormats.TopLeft);
            gfx.DrawString($"Integrantes: {miembros.Count}", fuenteNormal, brochaTexto,
                new XRect(xCuadrilla + 4, y + 30, colDestajo - 8, 8), XStringFormats.TopLeft);

            y += altoDestajo + espacioSeccion + 2;

            // === TABLA DE TAREAS ===
            DrawTarjeta(gfx, margenIzq, y, ancho, 15, 0, brochaSecundario, null, null, 3);
            gfx.DrawString("TAREAS / ÍTEMS", fuenteSeccion, brochaBlanca,
                new XRect(margenIzq + 3, y + 1, ancho - 6, 12), XStringFormats.TopLeft);

            y += 15;

            // Definir anchos de columnas
            double colNumAncho = 22;
            double colDescAncho = ancho - colNumAncho - 35 - 35 - 45;
            double colCantAncho = 35;
            double colUnidAncho = 35;
            double colTotalAncho = 45;

            double xTabla = margenIzq;
            double altoFilaTabla = 13;

            // Encabezados
            gfx.DrawRectangle(brochaPrincipal, xTabla, y, colNumAncho, altoFilaTabla);
            gfx.DrawString("Nº", fuenteEncabezado, brochaBlanca,
                new XRect(xTabla, y, colNumAncho, altoFilaTabla), XStringFormats.Center);

            xTabla += colNumAncho;
            gfx.DrawRectangle(brochaPrincipal, xTabla, y, colDescAncho, altoFilaTabla);
            gfx.DrawString("Descripción", fuenteEncabezado, brochaBlanca,
                new XRect(xTabla, y, colDescAncho, altoFilaTabla), XStringFormats.Center);

            xTabla += colDescAncho;
            gfx.DrawRectangle(brochaPrincipal, xTabla, y, colCantAncho, altoFilaTabla);
            gfx.DrawString("Cantidad", fuenteEncabezado, brochaBlanca,
                new XRect(xTabla, y, colCantAncho, altoFilaTabla), XStringFormats.Center);

            xTabla += colCantAncho;
            gfx.DrawRectangle(brochaPrincipal, xTabla, y, colUnidAncho, altoFilaTabla);
            gfx.DrawString("Unidad", fuenteEncabezado, brochaBlanca,
                new XRect(xTabla, y, colUnidAncho, altoFilaTabla), XStringFormats.Center);

            xTabla += colUnidAncho;
            gfx.DrawRectangle(brochaPrincipal, xTabla, y, colTotalAncho, altoFilaTabla);
            gfx.DrawString("Total", fuenteEncabezado, brochaBlanca,
                new XRect(xTabla, y, colTotalAncho, altoFilaTabla), XStringFormats.Center);

            y += altoFilaTabla;

            // Filas de datos (máximo 6 para dejar más espacio)
            decimal totalGeneral = 0;
            int maxFilas = 6;
            for (int i = 0; i < hijos.Count && i < maxFilas; i++)
            {
                xTabla = margenIzq;
                var item = hijos[i];
                var brochaFila = (i % 2 == 0) ? brochaBlanca : brochaGrisClaro;

                gfx.DrawRectangle(brochaFila, margenIzq, y, ancho, altoFilaTabla);
                gfx.DrawLine(penFino, margenIzq, y + altoFilaTabla, margenIzq + ancho, y + altoFilaTabla);

                // Nº
                gfx.DrawLine(penFino, xTabla + colNumAncho, y, xTabla + colNumAncho, y + altoFilaTabla);
                gfx.DrawString((i + 1).ToString(), fuenteNormal, brochaTexto,
                    new XRect(xTabla, y, colNumAncho, altoFilaTabla), XStringFormats.Center);

                xTabla += colNumAncho;

                // Descripción
                gfx.DrawLine(penFino, xTabla + colDescAncho, y, xTabla + colDescAncho, y + altoFilaTabla);
                gfx.DrawString(Truncar(item.Nombre ?? "", 38), fuentePequena, brochaTexto,
                    new XRect(xTabla + 2, y + 2, colDescAncho - 4, altoFilaTabla - 4), XStringFormats.TopLeft);

                xTabla += colDescAncho;

                // Cantidad
                gfx.DrawLine(penFino, xTabla + colCantAncho, y, xTabla + colCantAncho, y + altoFilaTabla);
                gfx.DrawString(item.Cantidad.ToString("F2"), fuentePequena, brochaTexto,
                    new XRect(xTabla, y, colCantAncho, altoFilaTabla), XStringFormats.Center);

                xTabla += colCantAncho;

                // Unidad
                gfx.DrawLine(penFino, xTabla + colUnidAncho, y, xTabla + colUnidAncho, y + altoFilaTabla);
                gfx.DrawString(item.Unidad ?? "-", fuentePequena, brochaTexto,
                    new XRect(xTabla, y, colUnidAncho, altoFilaTabla), XStringFormats.Center);

                xTabla += colUnidAncho;

                // Total
                string totalStr = item.Total.ToString("C2", CultureInfo.CurrentCulture);
                gfx.DrawString(totalStr, fuentePequena, brochaTexto,
                    new XRect(xTabla + 2, y, colTotalAncho - 4, altoFilaTabla), XStringFormats.CenterRight);

                totalGeneral += item.Total;
                y += altoFilaTabla;
            }

            // Fila de TOTAL
            xTabla = margenIzq;
            double anchoTotalLabel = colNumAncho + colDescAncho + colCantAncho + colUnidAncho;
            DrawTarjeta(gfx, xTabla, y, ancho, altoFilaTabla, 0, brochaSecundario, null, null, 3);
            gfx.DrawLine(new XPen(XColor.FromArgb(90, 255, 255, 255), 0.6),
                xTabla + anchoTotalLabel, y + 2, xTabla + anchoTotalLabel, y + altoFilaTabla - 2);
            gfx.DrawString("TOTAL", fuenteEncabezado, brochaBlanca,
                new XRect(xTabla + 2, y, anchoTotalLabel - 4, altoFilaTabla), XStringFormats.CenterRight);

            xTabla += anchoTotalLabel;
            gfx.DrawString(totalGeneral.ToString("C2", CultureInfo.CurrentCulture), fuenteEncabezado, brochaBlanca,
                new XRect(xTabla + 2, y, colTotalAncho - 4, altoFilaTabla), XStringFormats.CenterRight);

            y += altoFilaTabla + espacioSeccion + 2;

            // === INTEGRANTES DE LA CUADRILLA ===
            if (miembros.Count > 0)
            {
                DrawTarjeta(gfx, margenIzq, y, ancho, 15, 0, brochaSecundario, null, null, 3);
                gfx.DrawString("INTEGRANTES DE LA CUADRILLA", fuenteSeccion, brochaBlanca,
                    new XRect(margenIzq + 3, y + 1, ancho - 6, 12), XStringFormats.TopLeft);

                y += 15;

                foreach (var miembro in miembros)
                {
                    var colorFondo = miembro.EsJefe ? new XSolidBrush(XColor.FromArgb(255, 243, 224)) : brochaBlanca;
                    var colorAccentoFila = miembro.EsJefe ? brochaAcento : new XSolidBrush(colorSecundario);

                    gfx.DrawRectangle(colorFondo, margenIzq, y, ancho, 12);
                    gfx.DrawRectangle(colorAccentoFila, margenIzq, y, 2, 12);
                    gfx.DrawLine(penFino, margenIzq, y + 12, margenIzq + ancho, y + 12);

                    string titulo = miembro.EsJefe ? "JEFE" : "Miembro";
                    string telefono = !string.IsNullOrEmpty(miembro.Telefono) ? $" • Tel: {miembro.Telefono}" : "";
                    string info = $"{titulo}: {miembro.Nombre} • {miembro.Rol}{telefono}";

                    gfx.DrawString(Truncar(info, 85), fuentePequena, brochaTexto,
                        new XRect(margenIzq + 5, y + 2, ancho - 8, 8), XStringFormats.TopLeft);

                    y += 12;
                }

                y += espacioSeccion;
            }

            // === SECCIÓN DE FIRMAS ===
            y += 2;
            gfx.DrawLine(penPrincipal, margenIzq, y, margenIzq + ancho, y);
            y += 6;

            gfx.DrawString("FIRMAS Y VALIDACIONES", fuenteSeccion, brochaTexto,
                new XRect(margenIzq, y, ancho, 10), XStringFormats.TopLeft);

            y += 10;

            // Tres espacios para firmas
            double anchoFirma = (ancho / 3) - 1;
            double altoFirma = 30;
            double xFirma1 = margenIzq;
            double xFirma2 = margenIzq + anchoFirma + 1;
            double xFirma3 = margenIzq + (anchoFirma + 1) * 2;
            double lineaFirma = y + altoFirma - 8;

            var penLineaFirma = new XPen(colorSecundario, 0.7);

            // Firma 1
            DrawTarjeta(gfx, xFirma1, y, anchoFirma, altoFirma, 0, brochaBlanca, null, penBorde, 3);
            gfx.DrawLine(penLineaFirma, xFirma1 + 5, lineaFirma, xFirma1 + anchoFirma - 5, lineaFirma);
            gfx.DrawString("Residente/Propietario", fuenteMuyPequena, brochaTexto,
                new XRect(xFirma1, lineaFirma + 2, anchoFirma, 6), XStringFormats.Center);

            // Firma 2
            DrawTarjeta(gfx, xFirma2, y, anchoFirma, altoFirma, 0, brochaBlanca, null, penBorde, 3);
            gfx.DrawLine(penLineaFirma, xFirma2 + 5, lineaFirma, xFirma2 + anchoFirma - 5, lineaFirma);
            gfx.DrawString("Supervisor de Obra", fuenteMuyPequena, brochaTexto,
                new XRect(xFirma2, lineaFirma + 2, anchoFirma, 6), XStringFormats.Center);

            // Firma 3
            DrawTarjeta(gfx, xFirma3, y, anchoFirma, altoFirma, 0, brochaBlanca, null, penBorde, 3);
            gfx.DrawLine(penLineaFirma, xFirma3 + 5, lineaFirma, xFirma3 + anchoFirma - 5, lineaFirma);
            gfx.DrawString("Jefe de Proyecto", fuenteMuyPequena, brochaTexto,
                new XRect(xFirma3, lineaFirma + 2, anchoFirma, 6), XStringFormats.Center);

            // === PIE DE PÁGINA ===
            y = page.Height - 9;
            gfx.DrawLine(penFino, margenIzq, y, margenIzq + ancho, y);
            y += 1;

            string fechaGeneration = DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.CreateSpecificCulture("es-MX"));
            gfx.DrawString($"Generado: {fechaGeneration}", fuenteMuyPequena, brochaTexto,
                new XRect(margenIzq, y, ancho / 2, 5), XStringFormats.TopLeft);
            gfx.DrawString("© 2024 Pilaris", fuenteMuyPequena, brochaTexto,
                new XRect(margenIzq + ancho / 2, y, ancho / 2, 5), XStringFormats.TopRight);

            doc.Save(archivo);
        }

        private static string Truncar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }

        private static PdfSharp.Drawing.XGraphicsPath CrearRectRedondeado(double x, double y, double ancho, double alto, double radio)
        {
            var path = new PdfSharp.Drawing.XGraphicsPath();
            path.AddArc(x, y, radio * 2, radio * 2, 180, 90);
            path.AddArc(x + ancho - radio * 2, y, radio * 2, radio * 2, 270, 90);
            path.AddArc(x + ancho - radio * 2, y + alto - radio * 2, radio * 2, radio * 2, 0, 90);
            path.AddArc(x, y + alto - radio * 2, radio * 2, radio * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Tarjeta con esquinas redondeadas: franja de encabezado (color sólido) + cuerpo
        // (color claro), recortados al mismo contorno, con sombra suave y borde. altoBody = 0
        // dibuja solo el encabezado (para barras de título de sección).
        private static void DrawTarjeta(
            XGraphics gfx, double x, double y, double ancho, double altoHeader, double altoBody,
            XBrush colorHeader, XBrush colorBody, XPen colorBorde, double radio = 3)
        {
            double altoTotal = altoHeader + altoBody;
            var path = CrearRectRedondeado(x, y, ancho, altoTotal, radio);

            var pathSombra = CrearRectRedondeado(x + 0.6, y + 1, ancho, altoTotal, radio);
            gfx.DrawPath(new XSolidBrush(XColor.FromArgb(20, 0, 0, 0)), pathSombra);

            gfx.Save();
            gfx.IntersectClip(path);
            gfx.DrawRectangle(colorHeader, x, y, ancho, altoHeader);
            if (altoBody > 0)
                gfx.DrawRectangle(colorBody, x, y + altoHeader, ancho, altoBody);
            gfx.Restore();

            if (colorBorde != null)
                gfx.DrawPath(colorBorde, path);
        }

        private static string SanitizarNombreArchivo(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return "destajo";
            var invalidos = Path.GetInvalidFileNameChars();
            var limpio = new string(nombre.Select(c => invalidos.Contains(c) ? '_' : c).ToArray());
            return Truncar(limpio, 40).Trim();
        }

        public static void AsegurarTablaPDFsDestajos(SqlConnection conn)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PDFsDestajos')
                BEGIN
                    CREATE TABLE PDFsDestajos (
                        Id                INT IDENTITY(1,1) PRIMARY KEY,
                        Manzana           NVARCHAR(10)  NOT NULL,
                        Lote              NVARCHAR(10)  NOT NULL,
                        Prototipo         NVARCHAR(50)  NULL,
                        Ruta              NVARCHAR(50)  NULL,
                        NodoID            INT           NULL,
                        NombreDestajo     NVARCHAR(200) NULL,
                        CuadrillaAsignada NVARCHAR(20)  NULL,
                        NombreArchivo     NVARCHAR(255) NOT NULL,
                        ContenidoPDF      VARBINARY(MAX) NOT NULL,
                        TamanioBytes      BIGINT        NOT NULL,
                        Usuario           NVARCHAR(100) NULL,
                        FechaGeneracion   DATETIME      NOT NULL DEFAULT GETDATE()
                    );
                    CREATE INDEX IX_PDFsDestajos_ManzanaLote ON PDFsDestajos(Manzana, Lote);
                    CREATE INDEX IX_PDFsDestajos_Fecha       ON PDFsDestajos(FechaGeneracion DESC);
                    CREATE INDEX IX_PDFsDestajos_Ruta        ON PDFsDestajos(Ruta);
                END";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void GuardarPdfDestajoEnBD(ItemTareaActivacion destajo, string nombreArchivo, string archivoTemp)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(archivoTemp);

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    AsegurarTablaPDFsDestajos(conn);

                    const string sql = @"
                        INSERT INTO PDFsDestajos
                            (Manzana, Lote, Prototipo, Ruta, NodoID, NombreDestajo,
                             CuadrillaAsignada, NombreArchivo, ContenidoPDF, TamanioBytes,
                             Usuario, FechaGeneracion)
                        VALUES
                            (@m, @l, @proto, @ruta, @nodo, @nombre,
                             @cuadrilla, @archivo, @contenido, @tam,
                             @usuario, GETDATE())";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzanaActual);
                        cmd.Parameters.AddWithValue("@l", loteActual);
                        cmd.Parameters.AddWithValue("@proto", (object)prototipoActual ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ruta", (object)rutaActual ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@nodo", destajo.ID);
                        cmd.Parameters.AddWithValue("@nombre", (object)destajo.Nombre ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@cuadrilla",
                            string.IsNullOrEmpty(destajo.CuadrillaAsignada)
                                ? (object)DBNull.Value
                                : destajo.CuadrillaAsignada);
                        cmd.Parameters.AddWithValue("@archivo", nombreArchivo);
                        cmd.Parameters.Add("@contenido", System.Data.SqlDbType.VarBinary, -1).Value = bytes;
                        cmd.Parameters.AddWithValue("@tam", (long)bytes.Length);
                        cmd.Parameters.AddWithValue("@usuario", Environment.UserName ?? "");

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar el PDF en el repositorio (BD):\n" + ex.Message,
                    "Repositorio PDFs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRepositorio_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual))
            {
                MessageBox.Show("Selecciona y carga una casa primero (Manzana / Lote).",
                    "Repositorio de PDFs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new FormRepositorioDestajos(manzanaActual, loteActual, prototipoActual))
            {
                form.ShowDialog(this);
            }
        }

        private void btnDestajosPorCuadrilla_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            menu.Font = new Font("Segoe UI", 9.5F);

            var miSemana = new ToolStripMenuItem("Destajos por Semana (activados / terminados / pendientes)");
            miSemana.Click += (s, ev) =>
            {
                using (var f = new FormReporteDestajosSemana()) f.ShowDialog(this);
            };

            var miCuadrilla = new ToolStripMenuItem("Destajos Terminados por Cuadrilla");
            miCuadrilla.Click += (s, ev) =>
            {
                using (var f = new FormDestajosPorCuadrilla(manzanaActual, loteActual, rutaActual))
                    f.ShowDialog(this);
            };

            var miNomina = new ToolStripMenuItem("Listado de Nómina (raya semanal)");
            miNomina.Click += (s, ev) =>
            {
                using (var f = new FormReporteNomina()) f.ShowDialog(this);
            };

            var miRecibos = new ToolStripMenuItem("Recibos de Nómina (consultar / reimprimir)");
            miRecibos.Click += (s, ev) =>
            {
                using (var f = new FormVisorRecibosNomina()) f.ShowDialog(this);
            };

            menu.Items.Add(miSemana);
            menu.Items.Add(miCuadrilla);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(miNomina);
            menu.Items.Add(miRecibos);

            var btn = sender as Button;
            if (btn != null)
                menu.Show(btn, new Point(0, btn.Height));
            else
                menu.Show(Cursor.Position);
        }

        private class MiembroResumen
        {
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Rol { get; set; }
            public bool EsJefe { get; set; }
            public string Telefono { get; set; }
        }

        #endregion

        #region Modelo de datos

        /// <summary>
        /// Representa una tarea para activaci�n/desactivaci�n
        /// </summary>
        public class ItemTareaActivacion
        {
            public int ID { get; set; }
            public int ParentId { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Clave { get; set; }
            public int Nivel { get; set; }
            public int Contador { get; set; }
            public string Tipo { get; set; }
            public string TipoTarea { get; set; }
            public TipoTarea TipoTareaEnum { get; set; }
            public bool Activa { get; set; }
            public bool DesatajoActivado { get; set; } = false; // Destajo con cuadrilla asignada
            public bool Finalizado { get; set; } = false; // Stage 2: trabajos terminados y validados
            public DateTime? FechaFinalizacion { get; set; } = null;
            public DateTime? FechaActivacion { get; set; } = null;
            public decimal Cantidad { get; set; }
            public string Unidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public string CuadrillaAsignada { get; set; } = "";

            /// <summary>
            /// Cantidad ya surtida (sólo la llena el flujo web, desde el Surtido que
            /// ya trae api/destajos/arbol; el flujo clásico usa SurtidoDeInsumo()).
            /// </summary>
            public decimal Surtido { get; set; }

            public decimal Total
            {
                get { return Cantidad * PrecioUnitario; }
            }
        }

        #endregion
    }
}
