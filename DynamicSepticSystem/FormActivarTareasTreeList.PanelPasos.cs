using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Vista principal "paso a paso" de los destajos. Sustituye al árbol como
    /// supervisión primaria: muestra, por categoría, la secuencia de destajos donde
    /// cada paso se desbloquea sólo cuando el anterior queda Finalizado. El destajo
    /// actual despliega sus insumos y mano de obra (hijos Nivel 2 ya cargados) y sus
    /// acciones. Toda la lógica de etapas se reutiliza de FormActivarTareasTreeList.cs
    /// seleccionando el destajo en olvTareas y llamando a los handlers existentes.
    /// El árbol completo se abre en ventana aparte (MostrarArbolCompleto).
    /// </summary>
    public partial class FormActivarTareasTreeList
    {
        private Panel panelPasos;
        private FlowLayoutPanel flowPasos;
        private bool _refrescandoPasos = false;

        // Destajo "seleccionado" para el asistente y las acciones, independiente del
        // árbol (olvTareas no tiene ParentGetter, por lo que SelectedObject no es
        // fiable con categorías colapsadas en el modo paso-a-paso).
        private ItemTareaActivacion _itemAccionOverride; // transitorio, durante una acción
        private ItemTareaActivacion _destajoGuia;        // persistente, lo que muestra el asistente
        private int _indiceVista = -1;                   // destajo mostrado en el panel (navegación ▲▼)
        private int _fotoRecienteToken = 0;               // descarta descargas de foto viejas al cambiar de destajo

        /// <summary>Destajo objetivo de acciones/asistente, sin depender del árbol.</summary>
        private ItemTareaActivacion ItemSeleccionado()
            => _itemAccionOverride
               ?? _destajoGuia
               ?? (olvTareas != null ? olvTareas.SelectedObject as ItemTareaActivacion : null);

        private enum EstadoPaso { Bloqueado, Disponible, Activado, Terminado }

        // ============================================================
        // Construcción del panel (llamado desde el Designer)
        // ============================================================

        private Panel ConstruirPanelPasos()
        {
            panelPasos = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var banner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(20, 8, 20, 0)
            };
            banner.Controls.Add(new Label
            {
                Text = "PROGRESO DE DESTAJOS",
                Font = new Font("Segoe UI Semibold", 13.5F, FontStyle.Bold),
                ForeColor = GuiaPrimario,
                AutoSize = true,
                Location = new Point(20, 10)
            });
            banner.Controls.Add(new Label
            {
                Text = "Sólo el destajo actual a detalle · debajo, un vistazo del que sigue",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = GuiaTextoSuave,
                AutoSize = true,
                Location = new Point(310, 16)
            });

            var btnHistorial = new Button
            {
                Text = "🕘 Historial",
                Dock = DockStyle.Right,
                Width = 132,
                Height = 34,
                Margin = new Padding(0),
                BackColor = GuiaAcento,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnHistorial.FlatAppearance.BorderSize = 0;
            btnHistorial.Click += (s, e) => MostrarHistorial();
            banner.Controls.Add(btnHistorial);

            var btnInsumos = new Button
            {
                Text = "📦 Insumos",
                Dock = DockStyle.Right,
                Width = 132,
                Height = 34,
                Margin = new Padding(0),
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnInsumos.FlatAppearance.BorderSize = 0;
            btnInsumos.Click += (s, e) => MostrarInsumos();
            banner.Controls.Add(btnInsumos);

            flowPasos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = GuiaSuave,
                Padding = new Padding(16, 12, 16, 16)
            };
            flowPasos.Resize += (s, e) => RefrescarPasos();

            panelPasos.Controls.Add(flowPasos);
            panelPasos.Controls.Add(banner);
            return panelPasos;
        }

        // ============================================================
        // Reconstrucción (la llama ActualizarEstadisticas en cada cambio)
        // ============================================================

        private void RefrescarPasos()
        {
            if (panelPasos == null || flowPasos == null) return;
            if (_refrescandoPasos) return;
            _refrescandoPasos = true;
            try
            {
                flowPasos.SuspendLayout();

                var viejos = flowPasos.Controls.Cast<Control>().ToList();
                flowPasos.Controls.Clear();
                foreach (var c in viejos) c.Dispose();

                int ancho = Math.Max(360, flowPasos.ClientSize.Width - 24);

                if (itemsTareas == null || itemsTareas.Count == 0)
                {
                    flowPasos.Controls.Add(CrearMensajeVacio(ancho));
                    return;
                }

                // Navegación ▲▼ por todos los destajos (en orden de categoría y paso).
                var lista = ListaDestajos();
                if (lista.Count == 0)
                {
                    flowPasos.Controls.Add(CrearMensajeVacio(ancho));
                    return;
                }

                // Índice por defecto: el destajo "actual" (primer no finalizado).
                if (_indiceVista < 0 || _indiceVista >= lista.Count)
                {
                    int actualIdx = lista.FindIndex(d => !d.Finalizado);
                    _indiceVista = actualIdx >= 0 ? actualIdx : lista.Count - 1;
                }

                var visto = lista[_indiceVista];
                var cat = itemsTareas.FirstOrDefault(i => i.Nivel == 0 && i.ID == visto.ParentId);
                var destajosCat = itemsTareas
                    .Where(i => i.ParentId == visto.ParentId && i.Nivel == 1)
                    .ToList();
                int idxEnCat = destajosCat.FindIndex(x => x.ID == visto.ID);
                bool desbloqueado = idxEnCat <= 0 || destajosCat[idxEnCat - 1].Finalizado;
                bool esActual = _indiceVista == lista.FindIndex(d => !d.Finalizado);

                if (cat != null)
                    flowPasos.Controls.Add(CrearEncabezadoCategoria(cat, destajosCat, ancho));

                flowPasos.Controls.Add(CrearNavegador(lista, ancho));

                flowPasos.Controls.Add(CrearTarjetaPaso(
                    visto, idxEnCat + 1, destajosCat.Count, desbloqueado, esActual, ancho));

                flowPasos.Controls.Add(CrearCardFotoReciente(visto, ancho));
            }
            finally
            {
                flowPasos.ResumeLayout();
                _refrescandoPasos = false;
            }
        }

        /// <summary>
        /// Categoría activa = la primera (en orden) que todavía tiene algún destajo
        /// sin finalizar. Devuelve null cuando todas las categorías están completas.
        /// </summary>
        private ItemTareaActivacion CategoriaActiva()
        {
            if (itemsTareas == null) return null;
            return itemsTareas
                .Where(i => i.Nivel == 0)
                .FirstOrDefault(cat => itemsTareas.Any(
                    i => i.ParentId == cat.ID && i.Nivel == 1 && !i.Finalizado));
        }

        /// <summary>
        /// Enfoca en el asistente el "destajo actual" (primer pendiente) de la
        /// categoría indicada o, si no se da, de la categoría activa. Sustituye al
        /// antiguo flujo de selección en el árbol: aquí no hay treelist visible.
        /// </summary>
        private void SeleccionarDestajoActual(ItemTareaActivacion categoria = null)
        {
            if (olvTareas == null || itemsTareas == null || itemsTareas.Count == 0) return;

            var cat = categoria ?? CategoriaActiva();
            ItemTareaActivacion destajo = null;

            if (cat != null)
            {
                var destajos = itemsTareas
                    .Where(i => i.ParentId == cat.ID && i.Nivel == 1)
                    .ToList();
                destajo = destajos.FirstOrDefault(d => !d.Finalizado) ?? destajos.LastOrDefault();
            }
            else
            {
                // Todas las categorías terminadas: enfoca el último destajo.
                destajo = itemsTareas.Where(i => i.Nivel == 1).LastOrDefault();
            }

            if (destajo == null) return;
            _destajoGuia = destajo;
            _indiceVista = ListaDestajos().FindIndex(x => x.ID == destajo.ID);
            try { olvTareas.EnsureModelVisible(destajo); olvTareas.SelectedObject = destajo; } catch { }
            ActualizarPanelGuia();
        }

        /// <summary>
        /// Todos los destajos (Nivel 1) en orden: por categoría y luego por paso.
        /// Es la secuencia que recorren las flechas ▲▼.
        /// </summary>
        private List<ItemTareaActivacion> ListaDestajos()
        {
            var lista = new List<ItemTareaActivacion>();
            if (itemsTareas == null) return lista;
            foreach (var cat in itemsTareas.Where(i => i.Nivel == 0))
                lista.AddRange(itemsTareas.Where(i => i.ParentId == cat.ID && i.Nivel == 1));
            return lista;
        }

        /// <summary>
        /// Barra de navegación con flechas ▲ (anterior) y ▼ (siguiente) para recorrer
        /// los destajos, y un indicador de posición/progreso.
        /// </summary>
        private Control CrearNavegador(List<ItemTareaActivacion> lista, int ancho)
        {
            int total = lista.Count;
            int pos = _indiceVista + 1;
            int terminados = lista.Count(d => d.Finalizado);

            var barra = new Panel
            {
                Width = ancho,
                Height = 48,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 8)
            };

            Button Flecha(string texto, bool habilitado, int delta)
            {
                var b = new Button
                {
                    Text = texto,
                    Width = 118,
                    Dock = delta < 0 ? DockStyle.Left : DockStyle.Right,
                    Enabled = habilitado,
                    BackColor = habilitado ? GuiaAcento : Color.FromArgb(225, 229, 234),
                    ForeColor = habilitado ? Color.White : GuiaTextoSuave,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = habilitado ? Cursors.Hand : Cursors.Default
                };
                b.FlatAppearance.BorderSize = 0;
                if (habilitado) b.Click += (s, e) => NavegarDestajo(delta);
                return b;
            }

            // ▲ anterior (arriba) a la izquierda, ▼ siguiente (abajo) a la derecha.
            barra.Controls.Add(new Label
            {
                Text = total > 0
                    ? $"Destajo {pos} de {total}   ·   ✓ {terminados} terminados"
                    : "Sin destajos",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                ForeColor = GuiaTexto
            });
            barra.Controls.Add(Flecha("▼ Siguiente", _indiceVista < total - 1, +1));
            barra.Controls.Add(Flecha("▲ Anterior", _indiceVista > 0, -1));
            return barra;
        }

        /// <summary>Mueve la vista al destajo anterior/siguiente y refresca.</summary>
        private void NavegarDestajo(int delta)
        {
            var lista = ListaDestajos();
            if (lista.Count == 0) return;
            if (_indiceVista < 0) _indiceVista = 0;

            int nuevo = Math.Max(0, Math.Min(lista.Count - 1, _indiceVista + delta));
            if (nuevo == _indiceVista) return;

            _indiceVista = nuevo;
            _destajoGuia = lista[_indiceVista];

            // Diferido: RefrescarPasos reconstruye —y libera— el botón que originó el clic.
            BeginInvoke((Action)(() =>
            {
                RefrescarPasos();
                ActualizarPanelGuia();
            }));
        }

        // ============================================================
        // Historial: todas las categorías en una ventana aparte
        // ============================================================

        private void MostrarHistorial()
        {
            if (itemsTareas == null || itemsTareas.Count == 0)
            {
                MessageBox.Show("Carga las tareas de una Manzana y Lote para ver el historial.",
                    "Historial", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string titulo = string.IsNullOrEmpty(manzanaActual)
                ? "Historial de destajos — todas las categorías"
                : $"Historial de destajos — M{manzanaActual} L{loteActual}";

            using (var dlg = new Form
            {
                Text = titulo,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(720, 760),
                MinimumSize = new Size(520, 480),
                ShowInTaskbar = false,
                Font = new Font("Segoe UI", 9F)
            })
            {
                var flow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = true,
                    BackColor = GuiaSuave,
                    Padding = new Padding(16, 12, 16, 16)
                };

                int ancho = 660;
                Action recargar = null;
                recargar = () =>
                {
                    flow.SuspendLayout();
                    foreach (Control c in flow.Controls.Cast<Control>().ToList()) c.Dispose();
                    flow.Controls.Clear();

                    var categorias = itemsTareas.Where(i => i.Nivel == 0).ToList();
                    if (categorias.Count == 0)
                        flow.Controls.Add(NuevaEtiqueta(
                            "No hay categorías cargadas.", "Segoe UI", 10F, FontStyle.Italic,
                            GuiaTextoSuave, ancho));

                    foreach (var cat in categorias)
                    {
                        var destajos = itemsTareas
                            .Where(i => i.ParentId == cat.ID && i.Nivel == 1)
                            .ToList();

                        flow.Controls.Add(CrearEncabezadoCategoria(cat, destajos, ancho));
                        for (int i = 0; i < destajos.Count; i++)
                            flow.Controls.Add(CrearFilaHistorial(destajos[i], i + 1, ancho, recargar));
                    }
                    flow.ResumeLayout();
                };
                recargar();

                var toolbar = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 56,
                    BackColor = GuiaSuave,
                    Padding = new Padding(12, 11, 12, 11)
                };
                toolbar.Controls.Add(new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 1,
                    BackColor = GuiaBorde
                });

                var btnCerrar = new Button
                {
                    Text = "Cerrar",
                    Size = new Size(100, 32),
                    Location = new Point(toolbar.Width - 112, 12),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.FromArgb(99, 110, 114),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.OK
                };
                btnCerrar.FlatAppearance.BorderSize = 0;
                toolbar.Controls.Add(btnCerrar);

                dlg.Controls.Add(flow);
                dlg.Controls.Add(toolbar);
                dlg.AcceptButton = btnCerrar;
                dlg.ShowDialog(this);
            }
        }

        /// <summary>
        /// Fila compacta de historial: un renglón por destajo con su estado e importe.
        /// </summary>
        private Control CrearFilaHistorial(ItemTareaActivacion d, int numero, int ancho, Action onCambio)
        {
            Color badgeColor;
            string badge;
            if (d.Finalizado) { badgeColor = GuiaExito; badge = "✓ TERMINADO"; }
            else if (d.DesatajoActivado) { badgeColor = GuiaAviso; badge = "● EN PROCESO"; }
            else { badgeColor = GuiaTextoSuave; badge = "○ PENDIENTE"; }

            decimal importe = itemsTareas.Where(i => i.ParentId == d.ID).Sum(i => i.Total);
            string imp = importe.ToString("C2", CultureInfo.CurrentCulture);

            var fila = new Panel
            {
                Width = ancho,
                Height = 30,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 3)
            };
            fila.Controls.Add(new Label
            {
                Text = $"  {numero}.  {d.Nombre}",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = GuiaTexto,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });
            fila.Controls.Add(new Label
            {
                Text = imp + "    ",
                Font = new Font("Segoe UI", 8.75F, FontStyle.Regular),
                ForeColor = GuiaTextoSuave,
                Dock = DockStyle.Right,
                Width = 110,
                TextAlign = ContentAlignment.MiddleRight
            });
            fila.Controls.Add(new Label
            {
                Text = badge,
                Font = new Font("Segoe UI Semibold", 8.25F, FontStyle.Bold),
                ForeColor = badgeColor,
                Dock = DockStyle.Right,
                Width = 120,
                TextAlign = ContentAlignment.MiddleRight
            });

            // Desactivar disponible solo para destajos en proceso (activados, no finalizados).
            if (d.DesatajoActivado && !d.Finalizado)
            {
                fila.Height = 34;
                var btnDes = new Button
                {
                    Text = "Desactivar",
                    Dock = DockStyle.Right,
                    Width = 104,
                    BackColor = GuiaPeligro,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnDes.FlatAppearance.BorderSize = 0;
                btnDes.Click += (s, e) => BeginInvoke((Action)(() =>
                {
                    var prev = _itemAccionOverride;
                    _itemAccionOverride = d;
                    _destajoGuia = d;
                    try { MenuItemDesactivar_Click(null, EventArgs.Empty); }
                    finally { _itemAccionOverride = prev; }
                    onCambio?.Invoke();
                }));
                fila.Controls.Add(btnDes);
            }

            return fila;
        }

        // ============================================================
        // Tarjetas
        // ============================================================

        private Control CrearMensajeVacio(int ancho)
        {
            var p = new Panel
            {
                Width = ancho,
                Height = 120,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 8)
            };
            p.Controls.Add(new Label
            {
                Text = "Selecciona Manzana y Lote y pulsa “Cargar Tareas” para ver el progreso de los destajos.",
                Font = new Font("Segoe UI", 11F),
                ForeColor = GuiaTexto,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(20)
            });
            return p;
        }

        private Control CrearEncabezadoCategoria(
            ItemTareaActivacion cat, List<ItemTareaActivacion> destajos, int ancho)
        {
            int total = destajos.Count;
            int terminados = destajos.Count(d => d.Finalizado);

            var p = new Panel
            {
                Width = ancho,
                Height = 46,
                BackColor = GuiaPrimario,
                Margin = new Padding(0, 6, 0, 6)
            };
            p.Controls.Add(new Label
            {
                Text = (cat.Nombre ?? "Categoría").ToUpper(),
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            });
            p.Controls.Add(new Label
            {
                Text = total > 0 ? $"{terminados}/{total} terminados   " : "sin destajos   ",
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(189, 200, 215),
                AutoSize = false,
                Dock = DockStyle.Right,
                Width = 190,
                TextAlign = ContentAlignment.MiddleRight
            });
            return p;
        }

        private FlowLayoutPanel CrearTarjetaPaso(
            ItemTareaActivacion d, int numero, int totalPasos, bool desbloqueado, bool esActual, int ancho)
        {
            EstadoPaso estado =
                (!desbloqueado && !d.Finalizado && !d.DesatajoActivado) ? EstadoPaso.Bloqueado
                : d.Finalizado ? EstadoPaso.Terminado
                : d.DesatajoActivado ? EstadoPaso.Activado
                : EstadoPaso.Disponible;

            Color borde, badgeColor, fondo;
            string badge;
            switch (estado)
            {
                case EstadoPaso.Bloqueado:
                    fondo = Color.FromArgb(245, 246, 248); borde = GuiaBorde;
                    badgeColor = GuiaTextoSuave; badge = "🔒 BLOQUEADO"; break;
                case EstadoPaso.Terminado:
                    fondo = Color.FromArgb(240, 249, 243); borde = GuiaExito;
                    badgeColor = GuiaExito; badge = "✓ TERMINADO"; break;
                case EstadoPaso.Activado:
                    fondo = Color.FromArgb(255, 248, 236); borde = GuiaAviso;
                    badgeColor = GuiaAviso; badge = "● EN PROCESO"; break;
                default:
                    fondo = Color.White; borde = GuiaAcento;
                    badgeColor = GuiaAcento; badge = "▸ POR ACTIVAR"; break;
            }

            var card = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(ancho, 0),
                MaximumSize = new Size(ancho, 0),
                BackColor = fondo,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(14, 0, 14, 12),
                Margin = new Padding(0, 0, 0, 8)
            };

            int innerWidth = ancho - 32;

            // Franja superior de color según el estado.
            card.Controls.Add(new Panel
            {
                BackColor = badgeColor,
                Margin = new Padding(0, 0, 0, 10),
                MinimumSize = new Size(innerWidth, 4),
                MaximumSize = new Size(innerWidth, 4),
                Height = 4,
                Width = innerWidth
            });

            // --- Encabezado del paso ---
            string prefijo = esActual ? "DESTAJO ACTUAL  ·  " : "";
            card.Controls.Add(NuevaEtiqueta(
                $"{prefijo}PASO {numero} DE {totalPasos}  ·  {badge}",
                "Segoe UI Semibold", 10F, FontStyle.Bold, badgeColor, innerWidth));
            card.Controls.Add(NuevaEtiqueta(
                d.Nombre ?? "(sin nombre)", "Segoe UI Semibold", 15.5F, FontStyle.Bold,
                GuiaTexto, innerWidth));
            card.Controls.Add(NuevaEtiqueta(
                ConstruirMetaPaso(d, estado), "Segoe UI", 10F, FontStyle.Regular,
                GuiaTextoSuave, innerWidth));

            // Click en la tarjeta → sincroniza el panel asistente lateral
            EventHandler seleccionar = (s, e) =>
            {
                _destajoGuia = d;
                if (olvTareas != null)
                {
                    try { olvTareas.EnsureModelVisible(d); olvTareas.SelectedObject = d; } catch { }
                }
                ActualizarPanelGuia();
            };
            card.Click += seleccionar;
            foreach (Control c in card.Controls) c.Click += seleccionar;

            if (estado == EstadoPaso.Bloqueado)
                card.Controls.Add(NuevaEtiqueta(
                    "Termina el destajo anterior para desbloquear este paso.",
                    "Segoe UI", 10F, FontStyle.Italic, GuiaTextoSuave, innerWidth));

            // --- Insumos y mano de obra (hijos Nivel 2) ---
            var hijos = itemsTareas.Where(i => i.ParentId == d.ID).ToList();
            var insumos = hijos.Where(h => h.TipoTareaEnum == TipoTarea.Material).ToList();
            var mano = hijos.Where(h => h.TipoTareaEnum == TipoTarea.ManoDeObra).ToList();

            // --- Resumen en cifras (insumos / mano de obra / total) ---
            card.Controls.Add(CrearResumenCifras(insumos, mano, innerWidth));

            card.Controls.Add(CrearSeccionHijos("INSUMOS", insumos, d, false, innerWidth));

            // Recordatorio: surtir los materiales se hace en Almacén → Salidas.
            if (insumos.Count > 0 &&
                (estado == EstadoPaso.Disponible || estado == EstadoPaso.Activado))
            {
                string nota = estado == EstadoPaso.Disponible
                    ? "ℹ Al activar solo se asigna la cuadrilla. Los materiales se surten en Almacén → Salidas."
                    : "📦 Surte los materiales en Almacén → Salidas (aparecen los insumos pendientes de este destajo).";
                card.Controls.Add(NuevaEtiqueta(
                    nota, "Segoe UI", 9.5F, FontStyle.Italic, GuiaAcento, innerWidth));
            }

            card.Controls.Add(CrearSeccionHijos("MANO DE OBRA", mano, d,
                estado == EstadoPaso.Terminado, innerWidth));

            // --- Acciones (el destajo bloqueado solo se puede ver) ---
            if (estado != EstadoPaso.Bloqueado)
                card.Controls.Add(CrearAccionesPaso(d, estado, innerWidth));
            return card;
        }

        /// <summary>
        /// Tarjeta con la foto más reciente subida para el destajo (tabla
        /// EvidenciasDestajo, api/evidencias-destajo). Se recarga cada vez que se
        /// navega a otro destajo; la descarga corre en segundo plano.
        /// </summary>
        private Control CrearCardFotoReciente(ItemTareaActivacion d, int ancho)
        {
            var card = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(ancho, 0),
                MaximumSize = new Size(ancho, 0),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(14, 0, 14, 12),
                Margin = new Padding(0, 0, 0, 8)
            };

            int innerWidth = ancho - 32;

            card.Controls.Add(new Panel
            {
                BackColor = GuiaAcento,
                Margin = new Padding(0, 0, 0, 10),
                MinimumSize = new Size(innerWidth, 4),
                MaximumSize = new Size(innerWidth, 4),
                Height = 4,
                Width = innerWidth
            });

            card.Controls.Add(NuevaEtiqueta(
                "📷 FOTO MÁS RECIENTE", "Segoe UI Semibold", 10F, FontStyle.Bold, GuiaAcento, innerWidth));

            var marco = new Panel
            {
                Width = innerWidth,
                Height = 260,
                BackColor = Color.FromArgb(245, 246, 248),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 6, 0, 8)
            };
            var foto = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(245, 246, 248),
                Cursor = Cursors.Hand
            };
            marco.Controls.Add(foto);
            card.Controls.Add(marco);

            var info = NuevaEtiqueta(
                "Cargando…", "Segoe UI", 9.5F, FontStyle.Italic, GuiaTextoSuave, innerWidth);
            card.Controls.Add(info);

            CargarFotoReciente(d, foto, info);
            return card;
        }

        /// <summary>Baja en segundo plano la última evidencia del destajo y la pinta al llegar.</summary>
        private void CargarFotoReciente(ItemTareaActivacion d, PictureBox foto, Label info)
        {
            int token = ++_fotoRecienteToken;
            string manzana = manzanaActual, lote = loteActual, ruta = rutaActual;
            int nodoId = d.ID;

            Task.Run(() =>
            {
                EvidenciaDestajoApi ultima = null;
                byte[] bytes = null;
                try
                {
                    var lista = ApiClient.Get<List<EvidenciaDestajoApi>>(
                        "/api/evidencias-destajo?manzana=" + Uri.EscapeDataString(manzana ?? "") +
                        "&lote=" + Uri.EscapeDataString(lote ?? "") +
                        "&ruta=" + Uri.EscapeDataString(ruta ?? "") +
                        "&nodoId=" + nodoId);
                    // El listado viene ordenado ASC por fecha: la última posición es la más reciente.
                    ultima = lista?.LastOrDefault();
                    if (ultima != null)
                        bytes = ApiClient.GetBytes($"/api/evidencias-destajo/{ultima.Id}/foto");
                }
                catch { /* sin conexión o error de API: se muestra "sin evidencias" */ }

                BeginInvoke((Action)(() =>
                {
                    if (token != _fotoRecienteToken) return; // el usuario ya cambió de destajo

                    if (ultima == null || bytes == null || bytes.Length == 0)
                    {
                        info.Text = "Sin evidencias fotográficas para este destajo.";
                        return;
                    }

                    try
                    {
                        using (var ms = new MemoryStream(bytes))
                        using (var tmp = Image.FromStream(ms))
                            foto.Image = new Bitmap(tmp);
                    }
                    catch
                    {
                        info.Text = "No se pudo leer la imagen.";
                        return;
                    }

                    foto.Click += (s, e) => VerEvidenciaCompleta(ultima);

                    string desc = string.IsNullOrWhiteSpace(ultima.Descripcion) ? "" : $"“{ultima.Descripcion}”  ·  ";
                    info.Text = $"{desc}{ultima.Fecha:dd/MM/yyyy HH:mm}  ·  {ultima.Usuario}  ·  {ultima.TamanioKB:0} KB";
                    info.ForeColor = GuiaTexto;
                    info.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
                }));
            });
        }

        /// <summary>
        /// Fila de tres "chips" con las cifras clave del destajo actual:
        /// importe de insumos, importe de mano de obra y total.
        /// </summary>
        private Control CrearResumenCifras(
            List<ItemTareaActivacion> insumos, List<ItemTareaActivacion> mano, int ancho)
        {
            decimal totalIns = insumos.Sum(h => h.Total);
            decimal totalMano = mano.Sum(h => h.Total);
            decimal total = totalIns + totalMano;

            var fila = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(ancho, 0),
                MaximumSize = new Size(ancho, 0),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 10, 0, 2)
            };

            int chip = (ancho - 12) / 3;
            fila.Controls.Add(CrearChipCifra(
                $"INSUMOS ({insumos.Count})", totalIns, GuiaAcento, chip));
            fila.Controls.Add(CrearChipCifra(
                $"MANO DE OBRA ({mano.Count})", totalMano, Color.FromArgb(142, 68, 173), chip));
            fila.Controls.Add(CrearChipCifra(
                "TOTAL", total, GuiaPrimario, ancho - 2 * chip - 12));
            return fila;
        }

        private Control CrearChipCifra(string titulo, decimal monto, Color color, int ancho)
        {
            var chip = new Panel
            {
                Width = ancho,
                Height = 56,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 6, 0)
            };
            chip.Controls.Add(new Label
            {
                Text = monto.ToString("C2", CultureInfo.CurrentCulture),
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = color,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 16, 4, 0)
            });
            chip.Controls.Add(new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
                ForeColor = GuiaTextoSuave,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 18,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 4, 4, 0)
            });
            return chip;
        }

        private string ConstruirMetaPaso(ItemTareaActivacion d, EstadoPaso estado)
        {
            decimal importe = itemsTareas.Where(i => i.ParentId == d.ID).Sum(i => i.Total);
            string imp = importe.ToString("C2", CultureInfo.CurrentCulture);
            string cuad = string.IsNullOrEmpty(d.CuadrillaAsignada) ? "—" : d.CuadrillaAsignada;

            switch (estado)
            {
                case EstadoPaso.Activado:
                    string fa = d.FechaActivacion.HasValue
                        ? d.FechaActivacion.Value.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture) : "—";
                    return $"Cuadrilla {cuad}  ·  activado {fa}  ·  Importe {imp}";
                case EstadoPaso.Terminado:
                    string ff = d.FechaFinalizacion.HasValue
                        ? d.FechaFinalizacion.Value.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture) : "—";
                    return $"Cuadrilla {cuad}  ·  terminado {ff}  ·  Importe {imp}";
                default:
                    return $"Sin activar  ·  Importe {imp}";
            }
        }

        private Control CrearSeccionHijos(
            string titulo, List<ItemTareaActivacion> hijos,
            ItemTareaActivacion destajo, bool nominaHabilitada, int ancho)
        {
            var seccion = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(ancho, 0),
                MaximumSize = new Size(ancho, 0),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 8, 0, 0)
            };

            seccion.Controls.Add(NuevaEtiqueta(
                titulo, "Segoe UI Semibold", 9.5F, FontStyle.Bold, GuiaTextoSuave, ancho));

            if (hijos.Count == 0)
            {
                seccion.Controls.Add(NuevaEtiqueta(
                    "— sin elementos —", "Segoe UI", 10F, FontStyle.Italic, GuiaTextoSuave, ancho));
                return seccion;
            }

            foreach (var h in hijos)
                seccion.Controls.Add(CrearFilaHijo(h, destajo, nominaHabilitada, ancho));

            return seccion;
        }

        private Control CrearFilaHijo(
            ItemTareaActivacion hijo, ItemTareaActivacion destajo, bool nominaHabilitada, int ancho)
        {
            // Para INSUMOS (Material): verde si ya fue surtido a la casa, rojo si pendiente.
            Color colorTexto = GuiaTexto;
            string vineta = "•";
            if (hijo.TipoTareaEnum == TipoTarea.Material && hijo.Cantidad > 0m)
            {
                bool surtido = SurtidoDeInsumo(hijo.Clave, hijo.Nombre) >= hijo.Cantidad;
                colorTexto = surtido ? GuiaExito : GuiaPeligro;
                vineta = surtido ? "✓" : "○";
            }

            string detalle =
                $"{vineta}  {hijo.Nombre}    {hijo.Cantidad:0.##} {hijo.Unidad}    {hijo.Total.ToString("C2", CultureInfo.CurrentCulture)}";

            bool conBoton = nominaHabilitada
                && hijo.TipoTareaEnum == TipoTarea.ManoDeObra
                && hijo.Total > 0m;

            if (!conBoton)
                return NuevaEtiqueta(detalle, "Segoe UI", 10F, FontStyle.Regular, colorTexto, ancho);

            var fila = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(ancho, 0),
                MaximumSize = new Size(ancho, 0),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 1, 0, 1)
            };
            fila.Controls.Add(NuevaEtiqueta(
                detalle, "Segoe UI", 10F, FontStyle.Regular, GuiaTexto, ancho - 165));

            var btn = CrearBotonPaso("$ Asignar nómina", GuiaExito, 155);
            btn.Click += (s, e) => AccionDiferida(
                () => MenuItemAsignarNomina_Click(null, EventArgs.Empty), hijo);
            fila.Controls.Add(btn);
            return fila;
        }

        private Control CrearAccionesPaso(ItemTareaActivacion d, EstadoPaso estado, int ancho)
        {
            var acciones = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(ancho, 0),
                MaximumSize = new Size(ancho, 0),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 10, 0, 0)
            };

            switch (estado)
            {
                case EstadoPaso.Disponible:
                    AgregarBoton(acciones, "▸ Activar destajo", GuiaAcento, 190,
                        () => EjecutarFlujoActivacionDestajo(d));
                    break;

                case EstadoPaso.Activado:
                    AgregarBoton(acciones, "✓ Finalizar destajo", GuiaExito, 190,
                        () => MenuItemFinalizar_Click(null, EventArgs.Empty), d);
                    AgregarBoton(acciones, "📄 PDF", GuiaAcento, 100,
                        () => MenuItemRegenerarPdf_Click(null, EventArgs.Empty), d);
                    AgregarBoton(acciones, "👥 Cuadrilla", Color.FromArgb(99, 110, 114), 135,
                        () => MenuItemCambiarCuadrilla_Click(null, EventArgs.Empty), d);
                    AgregarBoton(acciones, "ℹ Propiedades", GuiaTextoSuave, 145,
                        () => MenuItemPropiedades_Click(null, EventArgs.Empty), d);
                    AgregarBoton(acciones, "✕ Desactivar", GuiaPeligro, 135,
                        () => MenuItemDesactivar_Click(null, EventArgs.Empty), d);
                    break;

                case EstadoPaso.Terminado:
                    AgregarBoton(acciones, "$ Distribución por cuadrilla", GuiaExito, 245,
                        () =>
                        {
                            using (var f = new FormDestajosPorCuadrilla(manzanaActual, loteActual, rutaActual))
                                f.ShowDialog(this);
                        });
                    AgregarBoton(acciones, "ℹ Propiedades", GuiaTextoSuave, 145,
                        () => MenuItemPropiedades_Click(null, EventArgs.Empty), d);
                    if (EsUsuarioAdmin())
                        AgregarBoton(acciones, "↩ Reabrir destajo", GuiaAviso, 180,
                            () => MenuItemReabrir_Click(null, EventArgs.Empty), d);
                    break;
            }
            return acciones;
        }

        // ============================================================
        // Helpers de UI / ejecución de acciones
        // ============================================================

        private void AgregarBoton(
            FlowLayoutPanel cont, string texto, Color color, int ancho,
            Action accion, ItemTareaActivacion seleccionar = null)
        {
            var b = CrearBotonPaso(texto, color, ancho);
            b.Click += (s, e) => AccionDiferida(accion, seleccionar);
            cont.Controls.Add(b);
        }

        private Button CrearBotonPaso(string texto, Color color, int ancho)
        {
            var b = new Button
            {
                Text = texto,
                Width = ancho,
                Height = 33,
                Margin = new Padding(0, 0, 6, 4),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Label NuevaEtiqueta(
            string texto, string fuente, float tam, FontStyle estilo, Color color, int ancho)
        {
            return new Label
            {
                Text = texto,
                Font = new Font(fuente, tam, estilo),
                ForeColor = color,
                AutoSize = true,
                MaximumSize = new Size(ancho, 0),
                Margin = new Padding(0, 1, 0, 1)
            };
        }

        /// <summary>
        /// Ejecuta la acción fuera del handler de Click actual (BeginInvoke), porque
        /// la acción dispara RefrescarPasos() que reconstruye —y libera— las tarjetas
        /// del FlowLayoutPanel, incluido el botón que originó el evento.
        /// </summary>
        private void AccionDiferida(Action accion, ItemTareaActivacion objetivo = null)
        {
            BeginInvoke((Action)(() =>
            {
                var prev = _itemAccionOverride;
                if (objetivo != null)
                    _itemAccionOverride = objetivo;
                try
                {
                    accion();
                    ActualizarPanelGuia();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al ejecutar la acción:\n" + ex.Message,
                        "Paso a paso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _itemAccionOverride = prev;
                }
            }));
        }

        // ============================================================
        // Árbol completo en ventana aparte (re-parenting de marcoArbol)
        // ============================================================

        private void btnArbol_Click(object sender, EventArgs e)
        {
            MostrarArbolCompleto();
        }

        private void MostrarArbolCompleto()
        {
            if (marcoArbol == null) return;

            string titulo = string.IsNullOrEmpty(manzanaActual)
                ? "Árbol completo de destajos"
                : $"Árbol completo de destajos — M{manzanaActual} L{loteActual}";

            using (var dlg = new Form
            {
                Text = titulo,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(1120, 760),
                MinimumSize = new Size(820, 520),
                ShowInTaskbar = false,
                Font = new Font("Segoe UI", 9F)
            })
            {
                var host = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10),
                    BackColor = Color.White
                };

                var toolbar = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 56,
                    BackColor = GuiaSuave,
                    Padding = new Padding(12, 11, 12, 11)
                };
                toolbar.Controls.Add(new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 1,
                    BackColor = GuiaBorde
                });

                AgregarBotonBarra(toolbar, "Marcar Todo", GuiaExito, 12, 120, btnMarcarTodos_Click);
                AgregarBotonBarra(toolbar, "Desmarcar Todo", GuiaPeligro, 138, 130, btnDesmarcarTodos_Click);
                AgregarBotonBarra(toolbar, "+ Por Nivel", GuiaAcento, 274, 100, btnMarcarPorNivel_Click);
                AgregarBotonBarra(toolbar, "− Por Nivel", GuiaTextoSuave, 380, 100, btnDesmarcarPorNivel_Click);
                AgregarBotonBarra(toolbar, "Guardar", Color.FromArgb(155, 89, 182), 492, 110, btnGuardar_Click);

                var btnCerrarDlg = new Button
                {
                    Text = "Cerrar",
                    Size = new Size(100, 32),
                    Location = new Point(toolbar.Width - 112, 12),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.FromArgb(99, 110, 114),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.OK
                };
                btnCerrarDlg.FlatAppearance.BorderSize = 0;
                toolbar.Controls.Add(btnCerrarDlg);

                // Re-parent del árbol al diálogo
                panelCentralOculto.Controls.Remove(marcoArbol);
                host.Controls.Add(marcoArbol);

                dlg.Controls.Add(host);
                dlg.Controls.Add(toolbar);
                dlg.AcceptButton = btnCerrarDlg;

                try
                {
                    dlg.ShowDialog(this);
                }
                finally
                {
                    // Devolver el árbol a su panel oculto
                    host.Controls.Remove(marcoArbol);
                    panelCentralOculto.Controls.Add(marcoArbol);
                    RefrescarPasos();
                    ActualizarPanelGuia();
                }
            }
        }

        private void AgregarBotonBarra(
            Panel barra, string texto, Color color, int x, int ancho, EventHandler handler)
        {
            var b = new Button
            {
                Text = texto,
                Location = new Point(x, 12),
                Size = new Size(ancho, 32),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += handler;
            barra.Controls.Add(b);
        }
    }
}
