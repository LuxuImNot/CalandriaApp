using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Panel lateral derecho que guía al usuario según la etapa del destajo
    /// seleccionado: sin activar → activado → terminado. Reutiliza los handlers
    /// del menú contextual para no duplicar lógica.
    /// </summary>
    public partial class FormActivarTareasTreeList
    {
        // ====== Paleta ======
        private static readonly Color GuiaPrimario = Color.FromArgb(41, 60, 88);
        private static readonly Color GuiaSuave = Color.FromArgb(248, 250, 253);
        private static readonly Color GuiaBorde = Color.FromArgb(218, 224, 232);
        private static readonly Color GuiaTexto = Color.FromArgb(52, 73, 94);
        private static readonly Color GuiaTextoSuave = Color.FromArgb(127, 140, 141);
        private static readonly Color GuiaExito = Color.FromArgb(39, 174, 96);
        private static readonly Color GuiaPeligro = Color.FromArgb(192, 57, 43);
        private static readonly Color GuiaAviso = Color.FromArgb(243, 156, 18);
        private static readonly Color GuiaAcento = Color.FromArgb(52, 152, 219);

        // ====== Controles del panel ======
        private Panel panelGuia;
        private FlowLayoutPanel guiaContenido;

        // Etapa
        private Panel guiaCardEtapa;
        private Panel guiaBadgeEtapa;
        private Label guiaBadgeTexto;
        private Label guiaEtapaDescripcion;

        // Siguiente paso
        private Panel guiaCardSiguiente;
        private Label guiaSiguienteTitulo;
        private Label guiaSiguienteDescripcion;
        private Button guiaBtnPrimario;

        // Acciones
        private Panel guiaCardAcciones;
        private FlowLayoutPanel guiaAccionesFlow;

        // Sin selección
        private Panel guiaCardVacio;

        /// <summary>
        /// Crea y devuelve el panel lateral derecho. Llamar desde InitializeComponent
        /// para integrarlo en el layout.
        /// </summary>
        private Panel ConstruirPanelGuia()
        {
            panelGuia = new Panel
            {
                Dock = DockStyle.Right,
                Width = 360,
                BackColor = GuiaSuave,
                Padding = new Padding(0)
            };

            // Separador vertical fino del lado izquierdo del panel
            var separadorIzq = new Panel
            {
                Dock = DockStyle.Left,
                Width = 1,
                BackColor = GuiaBorde
            };

            // Banner del panel
            var banner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = GuiaPrimario
            };
            banner.Controls.Add(new Label
            {
                Text = "ASISTENTE DE DESTAJO",
                Font = new Font("Segoe UI Semibold", 11.5F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 10)
            });
            banner.Controls.Add(new Label
            {
                Text = "Te guía paso a paso por cada etapa",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(189, 200, 215),
                AutoSize = true,
                Location = new Point(17, 32)
            });

            // Contenido scrollable
            guiaContenido = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = GuiaSuave,
                Padding = new Padding(10, 10, 10, 10)
            };

            guiaCardVacio = ConstruirCardVacio();
            guiaCardEtapa = ConstruirCardEtapa();
            guiaCardSiguiente = ConstruirCardSiguiente();
            guiaCardAcciones = ConstruirCardAcciones();

            guiaContenido.Controls.Add(guiaCardVacio);
            guiaContenido.Controls.Add(guiaCardEtapa);
            guiaContenido.Controls.Add(guiaCardSiguiente);
            guiaContenido.Controls.Add(guiaCardAcciones);

            panelGuia.Controls.Add(guiaContenido);
            panelGuia.Controls.Add(banner);
            panelGuia.Controls.Add(separadorIzq);

            // Estado inicial: sin selección
            MostrarEstadoVacio();
            return panelGuia;
        }

        // ============================================================
        // Construcción de tarjetas (cards)
        // ============================================================

        private Panel ConstruirCard(string titulo, out Panel cuerpo)
        {
            var card = new Panel
            {
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 8),
                BorderStyle = BorderStyle.FixedSingle
            };

            int anchoCard = panelGuia.Width - guiaContenido.Padding.Horizontal - 22;
            card.Width = anchoCard;

            var lblTitulo = new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = GuiaTextoSuave,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 26,
                Padding = new Padding(12, 6, 12, 4),
                BackColor = Color.FromArgb(252, 253, 254)
            };

            var separador = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = GuiaBorde
            };

            cuerpo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 12)
            };

            card.Controls.Add(cuerpo);
            card.Controls.Add(separador);
            card.Controls.Add(lblTitulo);
            return card;
        }

        private Panel ConstruirCardVacio()
        {
            Panel cuerpo;
            var card = ConstruirCard("INICIO", out cuerpo);
            card.Height = 200;

            var icono = new Label
            {
                Text = "👈",
                Font = new Font("Segoe UI Emoji", 28F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                ForeColor = GuiaTextoSuave
            };

            var titulo = new Label
            {
                Text = "Selecciona un destajo",
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = GuiaTexto,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 26
            };

            var pasos = new Label
            {
                Text = "1.  Elige Manzana y Lote\n" +
                       "2.  Pulsa “Cargar Tareas”\n" +
                       "3.  Marca un destajo para activarlo\n" +
                       "4.  Selecciónalo aquí para ver opciones",
                Font = new Font("Segoe UI", 9F),
                ForeColor = GuiaTexto,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(4, 8, 4, 4)
            };

            cuerpo.Controls.Add(pasos);
            cuerpo.Controls.Add(titulo);
            cuerpo.Controls.Add(icono);
            return card;
        }

        private Panel ConstruirCardEtapa()
        {
            Panel cuerpo;
            var card = ConstruirCard("ETAPA ACTUAL", out cuerpo);
            card.Height = 138;

            guiaBadgeEtapa = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = GuiaTextoSuave
            };
            guiaBadgeTexto = new Label
            {
                Text = "○  SIN ACTIVAR",
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            guiaBadgeEtapa.Controls.Add(guiaBadgeTexto);

            guiaEtapaDescripcion = new Label
            {
                Text = "Marca el destajo para activarlo y asignarle cuadrilla.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = GuiaTexto,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(2, 8, 2, 0)
            };

            cuerpo.Controls.Add(guiaEtapaDescripcion);
            cuerpo.Controls.Add(guiaBadgeEtapa);
            return card;
        }

        private Panel ConstruirCardSiguiente()
        {
            Panel cuerpo;
            var card = ConstruirCard("SIGUIENTE PASO", out cuerpo);
            card.Height = 158;

            guiaSiguienteTitulo = new Label
            {
                Text = "—",
                Font = new Font("Segoe UI Semibold", 11.5F, FontStyle.Bold),
                ForeColor = GuiaPrimario,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft
            };

            guiaSiguienteDescripcion = new Label
            {
                Text = "—",
                Font = new Font("Segoe UI", 9F),
                ForeColor = GuiaTexto,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 2, 0, 4)
            };

            guiaBtnPrimario = new Button
            {
                Text = "Acción",
                Dock = DockStyle.Bottom,
                Height = 42,
                BackColor = GuiaAcento,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            guiaBtnPrimario.FlatAppearance.BorderSize = 0;
            guiaBtnPrimario.Click += GuiaBtnPrimario_Click;

            cuerpo.Controls.Add(guiaSiguienteDescripcion);
            cuerpo.Controls.Add(guiaSiguienteTitulo);
            cuerpo.Controls.Add(guiaBtnPrimario);
            return card;
        }

        private Panel ConstruirCardAcciones()
        {
            Panel cuerpo;
            var card = ConstruirCard("ACCIONES DISPONIBLES", out cuerpo);
            card.Height = 158;

            guiaAccionesFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false
            };
            cuerpo.Controls.Add(guiaAccionesFlow);
            return card;
        }

        private Button CrearBotonAccion(string texto, Color color, EventHandler handler)
        {
            int anchoBtn = guiaCardAcciones.Width - 30;
            var b = new Button
            {
                Text = texto,
                Width = anchoBtn,
                Height = 26,
                Margin = new Padding(0, 0, 0, 4),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.75F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += handler;
            return b;
        }

        // ============================================================
        // Actualización dinámica
        // ============================================================

        private enum EtapaDestajo
        {
            SinSeleccion,
            Categoria,
            SinActivar,
            Activado,
            Terminado,
            HijoManoObra,
            HijoOtro
        }

        /// <summary>Acción a ejecutar cuando se pulsa el botón principal.</summary>
        private Action _accionPrimaria;

        private void ActualizarPanelGuia()
        {
            if (panelGuia == null) return;

            var item = olvTareas.SelectedObject as ItemTareaActivacion;
            var etapa = DeterminarEtapa(item);

            switch (etapa)
            {
                case EtapaDestajo.SinSeleccion:
                    MostrarEstadoVacio();
                    return;
                case EtapaDestajo.Categoria:
                    MostrarEstadoCategoria(item);
                    return;
                case EtapaDestajo.SinActivar:
                    MostrarEstadoSinActivar(item);
                    return;
                case EtapaDestajo.Activado:
                    MostrarEstadoActivado(item);
                    return;
                case EtapaDestajo.Terminado:
                    MostrarEstadoTerminado(item);
                    return;
                case EtapaDestajo.HijoManoObra:
                    MostrarEstadoHijoManoObra(item);
                    return;
                case EtapaDestajo.HijoOtro:
                    MostrarEstadoHijoOtro(item);
                    return;
            }
        }

        private EtapaDestajo DeterminarEtapa(ItemTareaActivacion item)
        {
            if (item == null) return EtapaDestajo.SinSeleccion;
            if (item.Nivel == 0) return EtapaDestajo.Categoria;
            if (item.Nivel == 1)
            {
                if (item.Finalizado) return EtapaDestajo.Terminado;
                if (item.DesatajoActivado) return EtapaDestajo.Activado;
                return EtapaDestajo.SinActivar;
            }
            // Nivel 2
            if (item.TipoTareaEnum == TipoTarea.ManoDeObra) return EtapaDestajo.HijoManoObra;
            return EtapaDestajo.HijoOtro;
        }

        private void OcultarTodasLasCards()
        {
            guiaCardVacio.Visible = false;
            guiaCardEtapa.Visible = false;
            guiaCardSiguiente.Visible = false;
            guiaCardAcciones.Visible = false;
        }

        private void MostrarEstadoVacio()
        {
            OcultarTodasLasCards();
            guiaCardVacio.Visible = true;
            _accionPrimaria = null;
        }

        private void MostrarEstadoCategoria(ItemTareaActivacion item)
        {
            OcultarTodasLasCards();
            guiaCardEtapa.Visible = true;
            guiaCardSiguiente.Visible = true;

            int totalHijos = itemsTareas.Count(i => i.ParentId == item.ID);
            int activados = itemsTareas.Count(i => i.ParentId == item.ID && i.DesatajoActivado);
            int terminados = itemsTareas.Count(i => i.ParentId == item.ID && i.Finalizado);

            PintarBadge(GuiaAcento, "▣  CATEGORÍA");
            guiaEtapaDescripcion.Text = $"{item.Nombre}\n{totalHijos} destajo(s) · {activados} activados · {terminados} terminados.";

            guiaSiguienteTitulo.Text = "Expande la categoría";
            guiaSiguienteDescripcion.Text = "Abre los destajos hijos para activarlos uno por uno.";
            ConfigurarBotonPrimario("Expandir categoría", GuiaAcento, () => olvTareas.Expand(item));
        }

        private void MostrarEstadoSinActivar(ItemTareaActivacion item)
        {
            OcultarTodasLasCards();
            guiaCardEtapa.Visible = true;
            guiaCardSiguiente.Visible = true;

            PintarBadge(GuiaTextoSuave, "○  SIN ACTIVAR");
            guiaEtapaDescripcion.Text = $"{item.Nombre}\nAún sin cuadrilla. Actívalo para asignarle una.";

            guiaSiguienteTitulo.Text = "Activar y asignar cuadrilla";
            guiaSiguienteDescripcion.Text = "Al activar se abrirá el selector de cuadrilla y se generará el PDF de orden.";
            ConfigurarBotonPrimario("▸ Activar destajo", GuiaAcento, () =>
            {
                // Simular el flujo que ocurre al marcar el checkbox del destajo
                EjecutarFlujoActivacionDestajo(item);
                ActualizarPanelGuia();
            });
        }

        private void MostrarEstadoActivado(ItemTareaActivacion item)
        {
            OcultarTodasLasCards();
            guiaCardEtapa.Visible = true;
            guiaCardSiguiente.Visible = true;
            guiaCardAcciones.Visible = true;

            string cuadrilla = string.IsNullOrEmpty(item.CuadrillaAsignada) ? "—" : item.CuadrillaAsignada;
            string activado = item.FechaActivacion.HasValue
                ? item.FechaActivacion.Value.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture)
                : "—";

            PintarBadge(GuiaAviso, "●  ACTIVADO");
            guiaEtapaDescripcion.Text = $"{item.Nombre}\nCuadrilla {cuadrilla} · activado {activado}";

            guiaSiguienteTitulo.Text = "Cuando termine, finaliza";
            guiaSiguienteDescripcion.Text = "Al finalizar quedará disponible para distribuir nómina.";
            ConfigurarBotonPrimario("✓ Finalizar destajo", GuiaExito, () =>
            {
                MenuItemFinalizar_Click(null, EventArgs.Empty);
                ActualizarPanelGuia();
            });

            RellenarAccionesActivado();
        }

        private void MostrarEstadoTerminado(ItemTareaActivacion item)
        {
            OcultarTodasLasCards();
            guiaCardEtapa.Visible = true;
            guiaCardSiguiente.Visible = true;
            guiaCardAcciones.Visible = true;

            string cuadrilla = string.IsNullOrEmpty(item.CuadrillaAsignada) ? "—" : item.CuadrillaAsignada;
            string terminado = item.FechaFinalizacion.HasValue
                ? item.FechaFinalizacion.Value.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture)
                : "—";

            PintarBadge(GuiaExito, "✓  TERMINADO");
            guiaEtapaDescripcion.Text = $"{item.Nombre}\nCuadrilla {cuadrilla} · terminado {terminado}";

            guiaSiguienteTitulo.Text = "Distribuir nómina";
            guiaSiguienteDescripcion.Text = "Abre el reporte por cuadrilla y captura el monto y concepto de cada trabajador.";
            ConfigurarBotonPrimario("$  Generar distribución de nómina", GuiaExito, () =>
            {
                using (var f = new FormDestajosPorCuadrilla(manzanaActual, loteActual, rutaActual))
                    f.ShowDialog(this);
            });

            RellenarAccionesTerminado();
        }

        private void MostrarEstadoHijoManoObra(ItemTareaActivacion item)
        {
            OcultarTodasLasCards();
            guiaCardEtapa.Visible = true;
            guiaCardSiguiente.Visible = true;

            PintarBadge(Color.FromArgb(142, 68, 173), "👷  MANO DE OBRA");
            guiaEtapaDescripcion.Text = $"{item.Nombre}\nSe distribuye entre los miembros de la cuadrilla.";

            var padre = LocalizarDestajoAncestro(item);
            bool padreFinalizado = padre != null && padre.Finalizado;

            guiaSiguienteTitulo.Text = padreFinalizado
                ? "Asignar nómina"
                : "Esperando finalización del destajo padre";
            guiaSiguienteDescripcion.Text = padreFinalizado
                ? "Captura el monto que recibe cada trabajador y emite los recibos."
                : "Sólo puedes asignar nómina cuando el destajo padre esté finalizado.";

            if (padreFinalizado)
            {
                ConfigurarBotonPrimario("$  Asignar nómina", GuiaExito, () =>
                {
                    MenuItemAsignarNomina_Click(null, EventArgs.Empty);
                });
            }
            else
            {
                ConfigurarBotonPrimario("⌛  Aún no disponible", GuiaTextoSuave, null);
            }
        }

        private void MostrarEstadoHijoOtro(ItemTareaActivacion item)
        {
            OcultarTodasLasCards();
            guiaCardEtapa.Visible = true;
            guiaCardSiguiente.Visible = true;

            PintarBadge(GuiaAcento, "▤  ITEM DEL DESTAJO");
            guiaEtapaDescripcion.Text = $"{item.Nombre}\nLas acciones se ejecutan sobre el destajo padre.";

            var padre = LocalizarDestajoAncestro(item);
            guiaSiguienteTitulo.Text = "Selecciona el destajo padre";
            guiaSiguienteDescripcion.Text = padre != null
                ? $"Padre: {padre.Nombre}"
                : "No se encontró el destajo padre en el árbol.";
            ConfigurarBotonPrimario(
                padre != null ? "▲  Ir al destajo padre" : "—",
                padre != null ? GuiaAcento : GuiaTextoSuave,
                padre != null ? (Action)(() =>
                {
                    olvTareas.SelectedObject = padre;
                    olvTareas.EnsureModelVisible(padre);
                    ActualizarPanelGuia();
                }) : null);
        }

        // ============================================================
        // Pintado de elementos comunes
        // ============================================================

        private void PintarBadge(Color color, string texto)
        {
            guiaBadgeEtapa.BackColor = color;
            guiaBadgeTexto.Text = texto;
        }

        private void ConfigurarBotonPrimario(string texto, Color color, Action accion)
        {
            guiaBtnPrimario.Text = texto;
            guiaBtnPrimario.BackColor = color;
            guiaBtnPrimario.Enabled = accion != null;
            guiaBtnPrimario.Cursor = accion != null ? Cursors.Hand : Cursors.No;
            _accionPrimaria = accion;
        }

        private void GuiaBtnPrimario_Click(object sender, EventArgs e)
        {
            try
            {
                _accionPrimaria?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al ejecutar la acción:\n" + ex.Message,
                    "Asistente", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RellenarAccionesActivado()
        {
            guiaAccionesFlow.Controls.Clear();
            guiaAccionesFlow.Controls.Add(CrearBotonAccion("📄  Generar / Regenerar PDF",
                GuiaAcento, (s, e) => MenuItemRegenerarPdf_Click(null, EventArgs.Empty)));
            guiaAccionesFlow.Controls.Add(CrearBotonAccion("👥  Cambiar cuadrilla",
                Color.FromArgb(99, 110, 114), (s, e) => MenuItemCambiarCuadrilla_Click(null, EventArgs.Empty)));
            guiaAccionesFlow.Controls.Add(CrearBotonAccion("ℹ️  Ver propiedades",
                Color.FromArgb(127, 140, 141), (s, e) => MenuItemPropiedades_Click(null, EventArgs.Empty)));
            guiaAccionesFlow.Controls.Add(CrearBotonAccion("✕  Desactivar destajo",
                GuiaPeligro, (s, e) => MenuItemDesactivar_Click(null, EventArgs.Empty)));
        }

        private void RellenarAccionesTerminado()
        {
            guiaAccionesFlow.Controls.Clear();
            guiaAccionesFlow.Controls.Add(CrearBotonAccion("📊  Reporte de la semana",
                GuiaAcento, (s, e) =>
                {
                    using (var f = new FormReporteDestajosSemana()) f.ShowDialog(this);
                }));
            guiaAccionesFlow.Controls.Add(CrearBotonAccion("📋  Listado de nómina",
                Color.FromArgb(22, 160, 133), (s, e) =>
                {
                    using (var f = new FormReporteNomina()) f.ShowDialog(this);
                }));
            guiaAccionesFlow.Controls.Add(CrearBotonAccion("🧾  Recibos generados",
                Color.FromArgb(155, 89, 182), (s, e) =>
                {
                    using (var f = new FormVisorRecibosNomina()) f.ShowDialog(this);
                }));
            guiaAccionesFlow.Controls.Add(CrearBotonAccion("ℹ️  Ver propiedades",
                Color.FromArgb(127, 140, 141), (s, e) => MenuItemPropiedades_Click(null, EventArgs.Empty)));
        }

        // ============================================================
        // Conexión con el árbol
        // ============================================================

        /// <summary>
        /// Conecta el panel al árbol. Llamar al final del constructor.
        /// </summary>
        private void ConectarPanelGuia()
        {
            if (panelGuia == null || olvTareas == null) return;
            olvTareas.SelectedIndexChanged += (s, e) => ActualizarPanelGuia();
            ActualizarPanelGuia();
        }
    }
}
