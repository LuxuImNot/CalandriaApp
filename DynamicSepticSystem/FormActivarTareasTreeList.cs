using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Formulario para activar/desactivar tareas del TreeList seg�n Manzana/Lote
    /// Similar a FormEstimacionConcepto pero para gestionar tareas de Calandria o Tunera
    /// </summary>
    public partial class FormActivarTareasTreeList : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        private List<ItemTareaActivacion> itemsTareas = new List<ItemTareaActivacion>();
        private string manzanaActual = "";
        private string loteActual = "";
        private string prototipoActual = "";
        private string rutaActual = ""; // "RutaTuneraDestajo" o "RutaCalandraDestajo"

        private ContextMenuStrip menuContextualManoObra;
        private ToolStripMenuItem menuItemAsignarNomina;
        private ContextMenuStrip menuContextualDestajo;
        private ToolStripMenuItem menuItemCambiarCuadrilla;
        private ToolStripMenuItem menuItemRegenerarPdf;
        private ToolStripMenuItem menuItemPropiedades;
        private ToolStripMenuItem menuItemDesactivar;
        private ToolStripMenuItem menuItemFinalizar;

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
            this.Text = "Gestión de Destajos por Casa";
            CargarManzanas();
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

            this.Text = $"Activar/Desactivar Tareas - M{manzanaActual} L{loteActual} ({prototipoActual})";

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
            return Global.UsuarioActual != null
                && !string.IsNullOrEmpty(Global.UsuarioActual.Nombre)
                && string.Equals(Global.UsuarioActual.Nombre, "admin", StringComparison.OrdinalIgnoreCase);
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
            menuContextualDestajo.Items.Add(menuItemCambiarCuadrilla);
            menuContextualDestajo.Items.Add(new ToolStripSeparator());
            menuContextualDestajo.Items.Add(menuItemFinalizar);
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
            var item = olvTareas.SelectedObject as ItemTareaActivacion;
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
            var item = olvTareas.SelectedObject as ItemTareaActivacion;
            if (item == null || item.Nivel != 1) return;

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
            var item = olvTareas.SelectedObject as ItemTareaActivacion;
            if (item == null || item.Nivel != 1) return;

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
            var item = olvTareas.SelectedObject as ItemTareaActivacion;
            if (item == null || item.Nivel != 1) return;

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

        private void MenuItemPropiedades_Click(object sender, EventArgs e)
        {
            var item = olvTareas.SelectedObject as ItemTareaActivacion;
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
            var item = olvTareas.SelectedObject as ItemTareaActivacion;
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
            itemsTareas.Clear();
            CargarTareasDesdeRuta();
            CargarActivacionesGuardadas();

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
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio' THEN c.Valor END), '0') AS PrecioUnitario
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

        private void ActualizarEstadisticas()
        {
            int totalDestajos = itemsTareas.Count(i => i.Nivel == 1);
            int destajosActivos = itemsTareas.Count(i => i.Nivel == 1 && i.Activa);
            int conCuadrilla = itemsTareas.Count(i => i.Nivel == 1 && i.Activa && !string.IsNullOrEmpty(i.CuadrillaAsignada));
            int completameteActivados = itemsTareas.Count(i => i.Nivel == 1 && i.DesatajoActivado);
            decimal montoActivo = itemsTareas.Where(i => i.Nivel == 2 && i.Activa).Sum(i => i.Total);

            lblEstadisticas.Text = string.Format(
                "Destajos activos: {0} de {1}   ·   Con cuadrilla: {2}   ·   Completamente activados: {3}   ·   Importe: {4}",
                destajosActivos,
                totalDestajos,
                conCuadrilla,
                completameteActivados,
                montoActivo.ToString("C2", CultureInfo.CurrentCulture));

            int max = totalDestajos > 0 ? totalDestajos : 1;
            progressBarActivacion.Maximum = max;
            progressBarActivacion.Value = Math.Min(completameteActivados, max);

            lblPorcentaje.Text = totalDestajos > 0
                ? $"{(completameteActivados * 100 / totalDestajos):F0}% completamente activados"
                : "—";

            // Reconstruir el control paso-a-paso con los estados actuales.
            RefrescarPasos();
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

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Flujo Activación → Cuadrilla → PDF

        private void EjecutarFlujoActivacionDestajo(ItemTareaActivacion destajo, bool esActivacion = true)
        {
            if (destajo == null) return;

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

            PersistirActivacionDestajo(destajo);
            olvTareas.RefreshObject(destajo);
            ActualizarEstadisticas();

            GenerarPdfDestajo(destajo);
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
            doc.Info.Author = "Calandria Residencial";

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
            var colorPrincipal = XColor.FromArgb(13, 71, 161);      // Azul oscuro
            var colorSecundario = XColor.FromArgb(33, 150, 243);    // Azul claro
            var colorBorde = XColor.FromArgb(189, 189, 189);       // Gris oscuro
            var colorTexto = XColor.FromArgb(33, 33, 33);          // Gris muy oscuro
            var colorGrisClaro = XColor.FromArgb(245, 245, 245);   // Gris muy claro

            var brochaPrincipal = new XSolidBrush(colorPrincipal);
            var brochaSecundario = new XSolidBrush(colorSecundario);
            var brochaTexto = new XSolidBrush(colorTexto);
            var brochaBlanca = XBrushes.White;
            var brochaGrisClaro = new XSolidBrush(colorGrisClaro);

            var penBorde = new XPen(colorBorde, 0.5);
            var penPrincipal = new XPen(colorPrincipal, 1.5);
            var penFino = new XPen(colorBorde, 0.3);

            double margenIzq = 12;
            double margenDer = 12;
            double margenSup = 10;
            double ancho = page.Width - margenIzq - margenDer;
            double y = margenSup;
            double espacioSeccion = 6;

            // === ENCABEZADO PRINCIPAL ===
            gfx.DrawRectangle(brochaPrincipal, margenIzq, y, ancho, 40);
            
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

                gfx.DrawRectangle(brochaGrisClaro, xCasa, y, colCasaAncho, altoCasa);
                gfx.DrawLine(penBorde, xCasa, y, xCasa, y + altoCasa);
                gfx.DrawLine(penBorde, xCasa + colCasaAncho, y, xCasa + colCasaAncho, y + altoCasa);
                gfx.DrawLine(penBorde, xCasa, y + 11, xCasa + colCasaAncho, y + 11);

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
            gfx.DrawRectangle(brochaSecundario, margenIzq, y, colDestajo, 16);
            gfx.DrawString("DESTAJO", fuenteSeccion, brochaBlanca,
                new XRect(margenIzq + 3, y + 1, colDestajo - 6, 13), XStringFormats.TopLeft);

            gfx.DrawRectangle(brochaGrisClaro, margenIzq, y + 16, colDestajo, altoDestajo - 16);
            gfx.DrawLine(penBorde, margenIzq, y + 16, margenIzq + colDestajo, y + 16);
            gfx.DrawLine(penBorde, margenIzq, y + 16, margenIzq, y + altoDestajo);
            gfx.DrawLine(penBorde, margenIzq + colDestajo, y + 16, margenIzq + colDestajo, y + altoDestajo);
            gfx.DrawLine(penBorde, margenIzq, y + altoDestajo, margenIzq + colDestajo, y + altoDestajo);

            gfx.DrawString($"ID: {destajo.ID}", fuenteNormal, brochaTexto,
                new XRect(margenIzq + 4, y + 20, colDestajo - 8, 8), XStringFormats.TopLeft);
            gfx.DrawString($"Nombre: {Truncar(destajo.Nombre ?? "-", 32)}", fuenteNormal, brochaTexto,
                new XRect(margenIzq + 4, y + 30, colDestajo - 8, 8), XStringFormats.TopLeft);
            gfx.DrawString($"Importe: {destajo.Total.ToString("C2", CultureInfo.CurrentCulture)}", fuenteNormal, brochaSecundario,
                new XRect(margenIzq + 4, y + 40, colDestajo - 8, 8), XStringFormats.TopLeft);

            // CUADRILLA
            double xCuadrilla = margenIzq + colDestajo + 1;
            gfx.DrawRectangle(brochaSecundario, xCuadrilla, y, colDestajo, 16);
            gfx.DrawString("CUADRILLA ASIGNADA", fuenteSeccion, brochaBlanca,
                new XRect(xCuadrilla + 3, y + 1, colDestajo - 6, 13), XStringFormats.TopLeft);

            gfx.DrawRectangle(brochaGrisClaro, xCuadrilla, y + 16, colDestajo, altoDestajo - 16);
            gfx.DrawLine(penBorde, xCuadrilla, y + 16, xCuadrilla + colDestajo, y + 16);
            gfx.DrawLine(penBorde, xCuadrilla, y + 16, xCuadrilla, y + altoDestajo);
            gfx.DrawLine(penBorde, xCuadrilla + colDestajo, y + 16, xCuadrilla + colDestajo, y + altoDestajo);
            gfx.DrawLine(penBorde, xCuadrilla, y + altoDestajo, xCuadrilla + colDestajo, y + altoDestajo);

            gfx.DrawString($"Código: {destajo.CuadrillaAsignada ?? "-"}", fuenteNormal, brochaTexto,
                new XRect(xCuadrilla + 4, y + 20, colDestajo - 8, 8), XStringFormats.TopLeft);
            gfx.DrawString($"Integrantes: {miembros.Count}", fuenteNormal, brochaTexto,
                new XRect(xCuadrilla + 4, y + 30, colDestajo - 8, 8), XStringFormats.TopLeft);

            string jefeInfo = miembros.FirstOrDefault(m => m.EsJefe)?.Nombre ?? "Sin asignar";
            gfx.DrawString($"Jefe: {Truncar(jefeInfo, 28)}", fuenteNormal, brochaSecundario,
                new XRect(xCuadrilla + 4, y + 40, colDestajo - 8, 8), XStringFormats.TopLeft);

            y += altoDestajo + espacioSeccion + 2;

            // === TABLA DE TAREAS ===
            gfx.DrawRectangle(brochaSecundario, margenIzq, y, ancho, 15);
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
            gfx.DrawRectangle(brochaSecundario, xTabla, y, anchoTotalLabel, altoFilaTabla);
            gfx.DrawString("TOTAL", fuenteEncabezado, brochaBlanca,
                new XRect(xTabla + 2, y, anchoTotalLabel - 4, altoFilaTabla), XStringFormats.CenterRight);

            xTabla += anchoTotalLabel;
            gfx.DrawRectangle(brochaSecundario, xTabla, y, colTotalAncho, altoFilaTabla);
            gfx.DrawString(totalGeneral.ToString("C2", CultureInfo.CurrentCulture), fuenteEncabezado, brochaBlanca,
                new XRect(xTabla + 2, y, colTotalAncho - 4, altoFilaTabla), XStringFormats.CenterRight);

            y += altoFilaTabla + espacioSeccion + 2;

            // === INTEGRANTES DE LA CUADRILLA ===
            if (miembros.Count > 0)
            {
                gfx.DrawRectangle(brochaSecundario, margenIzq, y, ancho, 15);
                gfx.DrawString("INTEGRANTES DE LA CUADRILLA", fuenteSeccion, brochaBlanca,
                    new XRect(margenIzq + 3, y + 1, ancho - 6, 12), XStringFormats.TopLeft);

                y += 15;

                foreach (var miembro in miembros)
                {
                    var colorFondo = miembro.EsJefe ? new XSolidBrush(XColor.FromArgb(255, 243, 224)) : brochaBlanca;
                    var colorBordeIntegrante = miembro.EsJefe ? new XPen(XColor.FromArgb(255, 152, 0), 1) : penBorde;

                    gfx.DrawRectangle(colorFondo, margenIzq, y, ancho, 12);
                    gfx.DrawRectangle(colorBordeIntegrante, margenIzq, y, ancho, 12);

                    string titulo = miembro.EsJefe ? "JEFE" : "Miembro";
                    string telefono = !string.IsNullOrEmpty(miembro.Telefono) ? $" • Tel: {miembro.Telefono}" : "";
                    string info = $"{titulo}: {miembro.Nombre} • {miembro.Rol}{telefono}";

                    gfx.DrawString(Truncar(info, 85), fuentePequena, brochaTexto,
                        new XRect(margenIzq + 3, y + 2, ancho - 6, 8), XStringFormats.TopLeft);

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

            // Firma 1
            gfx.DrawRectangle(new XPen(colorBorde, 0.5), xFirma1, y, anchoFirma, altoFirma);
            gfx.DrawLine(penFino, xFirma1 + 3, lineaFirma, xFirma1 + anchoFirma - 3, lineaFirma);
            gfx.DrawString("Residente/Propietario", fuenteMuyPequena, brochaTexto,
                new XRect(xFirma1, lineaFirma + 2, anchoFirma, 6), XStringFormats.Center);

            // Firma 2
            gfx.DrawRectangle(new XPen(colorBorde, 0.5), xFirma2, y, anchoFirma, altoFirma);
            gfx.DrawLine(penFino, xFirma2 + 3, lineaFirma, xFirma2 + anchoFirma - 3, lineaFirma);
            gfx.DrawString("Supervisor de Obra", fuenteMuyPequena, brochaTexto,
                new XRect(xFirma2, lineaFirma + 2, anchoFirma, 6), XStringFormats.Center);

            // Firma 3
            gfx.DrawRectangle(new XPen(colorBorde, 0.5), xFirma3, y, anchoFirma, altoFirma);
            gfx.DrawLine(penFino, xFirma3 + 3, lineaFirma, xFirma3 + anchoFirma - 3, lineaFirma);
            gfx.DrawString("Jefe de Proyecto", fuenteMuyPequena, brochaTexto,
                new XRect(xFirma3, lineaFirma + 2, anchoFirma, 6), XStringFormats.Center);

            // === PIE DE PÁGINA ===
            y = page.Height - 9;
            gfx.DrawLine(penFino, margenIzq, y, margenIzq + ancho, y);
            y += 1;

            string fechaGeneration = DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.CreateSpecificCulture("es-MX"));
            gfx.DrawString($"Generado: {fechaGeneration}", fuenteMuyPequena, brochaTexto,
                new XRect(margenIzq, y, ancho / 2, 5), XStringFormats.TopLeft);
            gfx.DrawString("© 2024 Calandria Residencial", fuenteMuyPequena, brochaTexto,
                new XRect(margenIzq + ancho / 2, y, ancho / 2, 5), XStringFormats.TopRight);

            doc.Save(archivo);
        }

        private static string Truncar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
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

            public decimal Total
            {
                get { return Cantidad * PrecioUnitario; }
            }
        }

        #endregion
    }
}
