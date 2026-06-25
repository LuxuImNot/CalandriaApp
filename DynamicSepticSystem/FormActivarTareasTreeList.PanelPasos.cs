using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
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
                Height = 44,
                BackColor = Color.White,
                Padding = new Padding(20, 8, 20, 0)
            };
            banner.Controls.Add(new Label
            {
                Text = "PROGRESO DE DESTAJOS",
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = GuiaPrimario,
                AutoSize = true,
                Location = new Point(20, 8)
            });
            banner.Controls.Add(new Label
            {
                Text = "Avanza un destajo a la vez · el siguiente se desbloquea al finalizar el anterior",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = GuiaTextoSuave,
                AutoSize = true,
                Location = new Point(280, 13)
            });

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

                var categorias = itemsTareas.Where(i => i.Nivel == 0).ToList();
                foreach (var cat in categorias)
                {
                    var destajos = itemsTareas
                        .Where(i => i.ParentId == cat.ID && i.Nivel == 1)
                        .ToList();

                    flowPasos.Controls.Add(CrearEncabezadoCategoria(cat, destajos, ancho));

                    for (int i = 0; i < destajos.Count; i++)
                    {
                        bool desbloqueado = (i == 0) || destajos[i - 1].Finalizado;
                        flowPasos.Controls.Add(CrearTarjetaPaso(destajos[i], i + 1, desbloqueado, ancho));
                    }
                }
            }
            finally
            {
                flowPasos.ResumeLayout();
                _refrescandoPasos = false;
            }
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
                Font = new Font("Segoe UI", 10F),
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
                Height = 40,
                BackColor = GuiaPrimario,
                Margin = new Padding(0, 6, 0, 6)
            };
            p.Controls.Add(new Label
            {
                Text = (cat.Nombre ?? "Categoría").ToUpper(),
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            });
            p.Controls.Add(new Label
            {
                Text = total > 0 ? $"{terminados}/{total} terminados   " : "sin destajos   ",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(189, 200, 215),
                AutoSize = false,
                Dock = DockStyle.Right,
                Width = 170,
                TextAlign = ContentAlignment.MiddleRight
            });
            return p;
        }

        private FlowLayoutPanel CrearTarjetaPaso(
            ItemTareaActivacion d, int numero, bool desbloqueado, int ancho)
        {
            EstadoPaso estado =
                !desbloqueado ? EstadoPaso.Bloqueado
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
                case EstadoPaso.Activado:
                    fondo = Color.FromArgb(255, 248, 236); borde = GuiaAviso;
                    badgeColor = GuiaAviso; badge = "● ACTIVADO"; break;
                case EstadoPaso.Terminado:
                    fondo = Color.FromArgb(240, 249, 243); borde = GuiaExito;
                    badgeColor = GuiaExito; badge = "✓ TERMINADO"; break;
                default:
                    fondo = Color.White; borde = GuiaAcento;
                    badgeColor = GuiaAcento; badge = "▸ DISPONIBLE"; break;
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
                Padding = new Padding(14, 10, 14, 12),
                Margin = new Padding(0, 0, 0, 8)
            };

            int innerWidth = ancho - 32;

            // --- Encabezado del paso ---
            card.Controls.Add(NuevaEtiqueta(
                $"PASO {numero}   ·   {badge}", "Segoe UI Semibold", 9F, FontStyle.Bold,
                badgeColor, innerWidth));
            card.Controls.Add(NuevaEtiqueta(
                d.Nombre ?? "(sin nombre)", "Segoe UI Semibold", 12F, FontStyle.Bold,
                GuiaTexto, innerWidth));
            card.Controls.Add(NuevaEtiqueta(
                ConstruirMetaPaso(d, estado), "Segoe UI", 9F, FontStyle.Regular,
                GuiaTextoSuave, innerWidth));

            // Click en la tarjeta → sincroniza el panel asistente lateral
            EventHandler seleccionar = (s, e) =>
            {
                if (olvTareas != null) olvTareas.SelectedObject = d;
                ActualizarPanelGuia();
            };
            card.Click += seleccionar;
            foreach (Control c in card.Controls) c.Click += seleccionar;

            if (estado == EstadoPaso.Bloqueado)
            {
                card.Controls.Add(NuevaEtiqueta(
                    "Termina el destajo anterior para desbloquear este paso.",
                    "Segoe UI", 9F, FontStyle.Italic, GuiaTextoSuave, innerWidth));
                return card;
            }

            // --- Insumos y mano de obra (hijos Nivel 2) ---
            var hijos = itemsTareas.Where(i => i.ParentId == d.ID).ToList();
            var insumos = hijos.Where(h => h.TipoTareaEnum == TipoTarea.Material).ToList();
            var mano = hijos.Where(h => h.TipoTareaEnum == TipoTarea.ManoDeObra).ToList();

            card.Controls.Add(CrearSeccionHijos("INSUMOS", insumos, d, false, innerWidth));
            card.Controls.Add(CrearSeccionHijos("MANO DE OBRA", mano, d,
                estado == EstadoPaso.Terminado, innerWidth));

            // --- Acciones ---
            card.Controls.Add(CrearAccionesPaso(d, estado, innerWidth));
            return card;
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
                titulo, "Segoe UI Semibold", 8.5F, FontStyle.Bold, GuiaTextoSuave, ancho));

            if (hijos.Count == 0)
            {
                seccion.Controls.Add(NuevaEtiqueta(
                    "— sin elementos —", "Segoe UI", 9F, FontStyle.Italic, GuiaTextoSuave, ancho));
                return seccion;
            }

            foreach (var h in hijos)
                seccion.Controls.Add(CrearFilaHijo(h, destajo, nominaHabilitada, ancho));

            return seccion;
        }

        private Control CrearFilaHijo(
            ItemTareaActivacion hijo, ItemTareaActivacion destajo, bool nominaHabilitada, int ancho)
        {
            string detalle =
                $"•  {hijo.Nombre}    {hijo.Cantidad:0.##} {hijo.Unidad}    {hijo.Total.ToString("C2", CultureInfo.CurrentCulture)}";

            bool conBoton = nominaHabilitada
                && hijo.TipoTareaEnum == TipoTarea.ManoDeObra
                && hijo.Total > 0m;

            if (!conBoton)
                return NuevaEtiqueta(detalle, "Segoe UI", 9F, FontStyle.Regular, GuiaTexto, ancho);

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
                detalle, "Segoe UI", 9F, FontStyle.Regular, GuiaTexto, ancho - 150));

            var btn = CrearBotonPaso("$ Asignar nómina", GuiaExito, 140);
            btn.Click += (s, e) => AccionDiferida(() =>
            {
                if (olvTareas != null) olvTareas.SelectedObject = hijo;
                MenuItemAsignarNomina_Click(null, EventArgs.Empty);
            });
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
                    AgregarBoton(acciones, "▸ Activar destajo", GuiaAcento, 170,
                        () => EjecutarFlujoActivacionDestajo(d));
                    break;

                case EstadoPaso.Activado:
                    AgregarBoton(acciones, "✓ Finalizar destajo", GuiaExito, 170,
                        () => MenuItemFinalizar_Click(null, EventArgs.Empty), d);
                    AgregarBoton(acciones, "📄 PDF", GuiaAcento, 90,
                        () => MenuItemRegenerarPdf_Click(null, EventArgs.Empty), d);
                    AgregarBoton(acciones, "👥 Cuadrilla", Color.FromArgb(99, 110, 114), 120,
                        () => MenuItemCambiarCuadrilla_Click(null, EventArgs.Empty), d);
                    AgregarBoton(acciones, "ℹ Propiedades", GuiaTextoSuave, 130,
                        () => MenuItemPropiedades_Click(null, EventArgs.Empty), d);
                    AgregarBoton(acciones, "✕ Desactivar", GuiaPeligro, 120,
                        () => MenuItemDesactivar_Click(null, EventArgs.Empty), d);
                    break;

                case EstadoPaso.Terminado:
                    AgregarBoton(acciones, "$ Distribución por cuadrilla", GuiaExito, 220,
                        () =>
                        {
                            using (var f = new FormDestajosPorCuadrilla(manzanaActual, loteActual, rutaActual))
                                f.ShowDialog(this);
                        });
                    AgregarBoton(acciones, "ℹ Propiedades", GuiaTextoSuave, 130,
                        () => MenuItemPropiedades_Click(null, EventArgs.Empty), d);
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
            b.Click += (s, e) => AccionDiferida(() =>
            {
                if (seleccionar != null && olvTareas != null)
                    olvTareas.SelectedObject = seleccionar;
                accion();
            });
            cont.Controls.Add(b);
        }

        private Button CrearBotonPaso(string texto, Color color, int ancho)
        {
            var b = new Button
            {
                Text = texto,
                Width = ancho,
                Height = 30,
                Margin = new Padding(0, 0, 6, 4),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.75F, FontStyle.Bold),
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
        private void AccionDiferida(Action accion)
        {
            BeginInvoke((Action)(() =>
            {
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
