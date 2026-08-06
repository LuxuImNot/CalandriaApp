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

        // Evidencias fotográficas (EvidenciasDestajo)
        private Panel guiaCardEvidencias;
        private FlowLayoutPanel guiaEvidenciasFlow;
        private Label guiaEvidenciasEstado;
        private int _evidenciasToken = 0; // descarta cargas viejas si el usuario cambia de destajo antes de que respondan

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
                Height = 66,
                BackColor = GuiaPrimario
            };
            banner.Controls.Add(new Label
            {
                Text = "ASISTENTE DE DESTAJO",
                Font = new Font("Segoe UI Semibold", 12.5F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 11)
            });
            banner.Controls.Add(new Label
            {
                Text = "Te guía paso a paso por cada etapa",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(189, 200, 215),
                AutoSize = true,
                Location = new Point(17, 36)
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
            guiaCardEvidencias = ConstruirCardEvidencias();
            guiaCardSiguiente = ConstruirCardSiguiente();
            guiaCardAcciones = ConstruirCardAcciones();

            guiaContenido.Controls.Add(guiaCardVacio);
            guiaContenido.Controls.Add(guiaCardEtapa);
            guiaContenido.Controls.Add(guiaCardEvidencias);
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
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = GuiaTextoSuave,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 29,
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
            card.Height = 220;

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
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = GuiaTexto,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 28
            };

            var pasos = new Label
            {
                Text = "1.  Elige Manzana y Lote\n" +
                       "2.  Pulsa “Cargar Tareas”\n" +
                       "3.  En el panel de progreso elige un destajo\n" +
                       "4.  Aquí verás sus opciones paso a paso",
                Font = new Font("Segoe UI", 10F),
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
            card.Height = 150;

            guiaBadgeEtapa = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = GuiaTextoSuave
            };
            guiaBadgeTexto = new Label
            {
                Text = "○  SIN ACTIVAR",
                Font = new Font("Segoe UI Semibold", 15.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            guiaBadgeEtapa.Controls.Add(guiaBadgeTexto);

            guiaEtapaDescripcion = new Label
            {
                Text = "Marca el destajo para activarlo y asignarle cuadrilla.",
                Font = new Font("Segoe UI", 11F),
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
            card.Height = 172;

            guiaSiguienteTitulo = new Label
            {
                Text = "—",
                Font = new Font("Segoe UI Semibold", 12.5F, FontStyle.Bold),
                ForeColor = GuiaPrimario,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft
            };

            guiaSiguienteDescripcion = new Label
            {
                Text = "—",
                Font = new Font("Segoe UI", 10F),
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
                Height = 44,
                BackColor = GuiaAcento,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11.5F, FontStyle.Bold),
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

        /// <summary>
        /// Miniaturas de las evidencias fotográficas (tabla EvidenciasDestajo,
        /// api/evidencias-destajo) del destajo actualmente mostrado. Sólo consulta;
        /// la carga se hace en otra pantalla/dispositivo.
        /// </summary>
        private Panel ConstruirCardEvidencias()
        {
            Panel cuerpo;
            var card = ConstruirCard("EVIDENCIAS FOTOGRÁFICAS", out cuerpo);
            card.Height = 150;

            guiaEvidenciasEstado = new Label
            {
                Text = "—",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = GuiaTextoSuave,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft
            };

            guiaEvidenciasFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true
            };

            cuerpo.Controls.Add(guiaEvidenciasFlow);
            cuerpo.Controls.Add(guiaEvidenciasEstado);
            return card;
        }

        private Panel ConstruirCardAcciones()
        {
            Panel cuerpo;
            var card = ConstruirCard("ACCIONES DISPONIBLES", out cuerpo);
            card.Height = 195;

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
                Height = 29,
                Margin = new Padding(0, 0, 0, 4),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
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

            var item = ItemSeleccionado();
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
            guiaCardEvidencias.Visible = false;
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

            guiaSiguienteTitulo.Text = "Avanza por sus destajos";
            guiaSiguienteDescripcion.Text = "El panel de progreso muestra el destajo actual de esta categoría; ve a él para activarlo.";
            ConfigurarBotonPrimario("▸ Ir al destajo actual", GuiaAcento, () => SeleccionarDestajoActual(item));
        }

        private void MostrarEstadoSinActivar(ItemTareaActivacion item)
        {
            OcultarTodasLasCards();
            guiaCardEtapa.Visible = true;
            guiaCardSiguiente.Visible = true;

            PintarBadge(GuiaTextoSuave, "○  SIN ACTIVAR");
            guiaEtapaDescripcion.Text = $"{item.Nombre}\nAún sin cuadrilla. Actívalo para asignarle una.";

            guiaCardEvidencias.Visible = true;
            CargarEvidencias(item);

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

            guiaCardEvidencias.Visible = true;
            CargarEvidencias(item);

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

            guiaCardEvidencias.Visible = true;
            CargarEvidencias(item);

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
            if (EsUsuarioAdmin())
                guiaAccionesFlow.Controls.Add(CrearBotonAccion("↩️  Reabrir destajo (deshacer)",
                    GuiaAviso, (s, e) => MenuItemReabrir_Click(null, EventArgs.Empty)));
        }

        // ============================================================
        // Evidencias fotográficas (EvidenciasDestajo)
        // ============================================================

        /// <summary>
        /// Pide al API las evidencias del destajo indicado y las pinta como
        /// miniaturas. Corre en segundo plano para no congelar la UI mientras
        /// bajan las fotos.
        /// </summary>
        private void CargarEvidencias(ItemTareaActivacion item)
        {
            guiaEvidenciasFlow.Controls.Clear();
            guiaEvidenciasEstado.Text = "Cargando evidencias…";
            guiaEvidenciasEstado.Visible = true;

            int token = ++_evidenciasToken;
            string manzana = manzanaActual, lote = loteActual, ruta = rutaActual;
            int nodoId = item.ID;

            Task.Run(() =>
            {
                List<EvidenciaDestajoApi> lista;
                try
                {
                    lista = ApiClient.Get<List<EvidenciaDestajoApi>>(
                        "/api/evidencias-destajo?manzana=" + Uri.EscapeDataString(manzana ?? "") +
                        "&lote=" + Uri.EscapeDataString(lote ?? "") +
                        "&ruta=" + Uri.EscapeDataString(ruta ?? "") +
                        "&nodoId=" + nodoId) ?? new List<EvidenciaDestajoApi>();
                }
                catch
                {
                    lista = null; // error de red/API
                }

                BeginInvoke((Action)(() =>
                {
                    if (token != _evidenciasToken) return; // el usuario ya cambió de destajo

                    if (lista == null)
                    {
                        guiaEvidenciasEstado.Text = "No se pudieron cargar las evidencias.";
                        return;
                    }
                    if (lista.Count == 0)
                    {
                        guiaEvidenciasEstado.Text = "Sin evidencias fotográficas.";
                        return;
                    }

                    guiaEvidenciasEstado.Visible = false;
                    foreach (var ev in lista)
                        guiaEvidenciasFlow.Controls.Add(CrearMiniaturaEvidencia(ev));
                }));
            });
        }

        /// <summary>Miniatura clicable; la imagen se baja en segundo plano y se pinta al llegar.</summary>
        private Control CrearMiniaturaEvidencia(EvidenciaDestajoApi ev)
        {
            var box = new PictureBox
            {
                Width = 56,
                Height = 56,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 246, 248),
                Margin = new Padding(0, 0, 6, 6),
                Cursor = Cursors.Hand
            };
            var tt = new ToolTip();
            tt.SetToolTip(box, string.IsNullOrWhiteSpace(ev.Descripcion)
                ? ev.Fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture)
                : ev.Descripcion);
            box.Click += (s, e) => VerEvidenciaCompleta(ev);

            Task.Run(() =>
            {
                byte[] bytes = null;
                try { bytes = ApiClient.GetBytes($"/api/evidencias-destajo/{ev.Id}/foto"); } catch { }
                if (bytes == null || bytes.Length == 0) return;

                BeginInvoke((Action)(() =>
                {
                    try
                    {
                        using (var ms = new MemoryStream(bytes))
                        using (var tmp = Image.FromStream(ms))
                            box.Image = new Bitmap(tmp);
                    }
                    catch { /* miniatura ilegible, se deja el placeholder */ }
                }));
            });

            return box;
        }

        /// <summary>Abre la evidencia a tamaño completo; permite eliminarla si el usuario es Admin.</summary>
        private void VerEvidenciaCompleta(EvidenciaDestajoApi ev)
        {
            byte[] bytes;
            try
            {
                bytes = ApiClient.GetBytes($"/api/evidencias-destajo/{ev.Id}/foto");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar la evidencia:\n" + ex.Message,
                    "Evidencias", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (bytes == null || bytes.Length == 0)
            {
                MessageBox.Show("La evidencia no tiene imagen.", "Evidencias",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new Form
            {
                Text = string.IsNullOrWhiteSpace(ev.Descripcion) ? "Evidencia fotográfica" : ev.Descripcion,
                Size = new Size(760, 640),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.Black,
                ShowInTaskbar = false
            })
            {
                var pic = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
                using (var ms = new MemoryStream(bytes))
                using (var tmp = Image.FromStream(ms))
                    pic.Image = new Bitmap(tmp);

                var pie = new Label
                {
                    Dock = DockStyle.Bottom,
                    Height = 30,
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = $"{ev.Fecha:dd/MM/yyyy HH:mm} · {ev.Usuario}"
                };

                dlg.Controls.Add(pic);
                dlg.Controls.Add(pie);

                if (EsUsuarioAdmin())
                {
                    var btnEliminar = new Button
                    {
                        Text = "✕ Eliminar evidencia",
                        Dock = DockStyle.Top,
                        Height = 32,
                        BackColor = GuiaPeligro,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };
                    btnEliminar.FlatAppearance.BorderSize = 0;
                    btnEliminar.Click += (s, e) =>
                    {
                        if (MessageBox.Show("¿Eliminar esta evidencia? No se puede deshacer.",
                            "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                            return;
                        try
                        {
                            ApiClient.Post($"/api/evidencias-destajo/{ev.Id}/eliminar", null);
                            dlg.Close();
                            var actual = ItemSeleccionado();
                            if (actual != null) CargarEvidencias(actual);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("No se pudo eliminar:\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    dlg.Controls.Add(btnEliminar);
                }

                dlg.ShowDialog(this);
                pic.Image?.Dispose();
            }
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
            olvTareas.SelectedIndexChanged += (s, e) =>
            {
                // Si la selección viene del árbol real, sincroniza el destajo del asistente.
                var sel = olvTareas.SelectedObject as ItemTareaActivacion;
                if (sel != null) _destajoGuia = sel;
                ActualizarPanelGuia();
            };
            ActualizarPanelGuia();
        }
    }
}
