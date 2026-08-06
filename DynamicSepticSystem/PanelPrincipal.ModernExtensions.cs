using DynamicSepticSystem.ModernUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class PanelPrincipal
    {
        // ---- estado para throttling y animaciones modernas ----
        private DateTime _lastMouseMoveRender = DateTime.MinValue;
        private const int MouseMoveThrottleMs = 12; // ~80fps cap durante drag
        private bool _modernExtensionsReady = false;

        /// <summary>
        /// Configura los overlay buttons del mapa, el pin pulsante,
        /// la fecha actual del header y los handlers complementarios.
        /// Llamar después de InicializarSistemaMapa() en el constructor.
        /// </summary>
        private void ConfigurarExtensionesModernas()
        {
            if (_modernExtensionsReady) return;
            _modernExtensionsReady = true;

            // Wire overlay buttons del mapa (definidos en el Designer)
            if (btnMapaZoomIn != null) btnMapaZoomIn.Click += (s, e) => ZoomMapa(1.25f);
            if (btnMapaZoomOut != null) btnMapaZoomOut.Click += (s, e) => ZoomMapa(0.8f);
            if (btnMapaReset != null) btnMapaReset.Click += (s, e) => MostrarVistaCompletaSembrado();

            // Pin pulse timer
            timerPin = new Timer { Interval = 50 };
            timerPin.Tick += (s, e) =>
            {
                pinPulsePhase += 0.18f;
                if (pinPulsePhase > Math.PI * 2) pinPulsePhase -= (float)(Math.PI * 2);
                if (casaActual != null) pictureBoxMapa.Invalidate();
            };
            timerPin.Start();

            // Refrescar lblFotoEmpty visibility según contenido del pictureBox
            if (pictureBoxCasa != null && lblFotoEmpty != null)
            {
                pictureBoxCasa.Resize += (s, e) =>
                {
                    lblFotoEmpty.Bounds = pictureBoxCasa.Bounds;
                };
                lblFotoEmpty.Bounds = pictureBoxCasa.Bounds;
                lblFotoEmpty.Visible = (pictureBoxCasa.Image == null);
            }

            // Re-aplicar estilos visuales que ThemeManager pudo haber reseteado
            ReaplicarEstilosLabels();

            // Forzar pintado limpio de las cards (Padding correcto post-theme)
            foreach (Control c in panelContenido.Controls)
            {
                if (c is ModernUI.ModernCard mc)
                    mc.Invalidate();
            }

            // Fade-in del panel de contenido (opacity simulada con timer y backcolor)
            FadeInPanel(panelContenido);
        }

        private void ReaplicarEstilosLabels()
        {
            // Header
            if (lblHeaderTitulo != null)
            {
                lblHeaderTitulo.Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
                lblHeaderTitulo.ForeColor = Color.FromArgb(60, 36, 15);
                lblHeaderTitulo.BackColor = Color.Transparent;
            }
            if (lblHeaderSubtitulo != null)
            {
                lblHeaderSubtitulo.Font = new Font("Segoe UI", 9.75F);
                lblHeaderSubtitulo.ForeColor = Color.FromArgb(140, 134, 124);
                lblHeaderSubtitulo.BackColor = Color.Transparent;
            }
            if (lblFechaActual != null)
            {
                lblFechaActual.Font = new Font("Segoe UI", 9.5F);
                lblFechaActual.ForeColor = Color.FromArgb(140, 134, 124);
                lblFechaActual.BackColor = Color.Transparent;
            }

            // Búsqueda labels
            if (lblManzanaLabel != null)
            {
                lblManzanaLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
                lblManzanaLabel.ForeColor = Color.FromArgb(115, 110, 102);
                lblManzanaLabel.BackColor = Color.Transparent;
            }
            if (lblLoteLabel != null)
            {
                lblLoteLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
                lblLoteLabel.ForeColor = Color.FromArgb(115, 110, 102);
                lblLoteLabel.BackColor = Color.Transparent;
            }

            // Info casa
            if (lblCasaActual != null)
            {
                lblCasaActual.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
                lblCasaActual.ForeColor = Color.FromArgb(48, 42, 36);
                lblCasaActual.BackColor = Color.Transparent;
            }
            if (lblManzanaLote != null)
            {
                lblManzanaLote.Font = new Font("Segoe UI", 9F);
                lblManzanaLote.ForeColor = Color.FromArgb(115, 110, 102);
                lblManzanaLote.BackColor = Color.Transparent;
            }
            if (lblPrototipo != null)
            {
                lblPrototipo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                lblPrototipo.ForeColor = Color.FromArgb(179, 108, 46);
                lblPrototipo.BackColor = Color.Transparent;
            }

            // Progreso
            if (lblProgresoGeneral != null)
            {
                lblProgresoGeneral.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                lblProgresoGeneral.ForeColor = Color.FromArgb(115, 110, 102);
                lblProgresoGeneral.BackColor = Color.Transparent;
            }
            if (lblProgresoPartidas != null)
            {
                lblProgresoPartidas.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                lblProgresoPartidas.ForeColor = Color.FromArgb(115, 110, 102);
                lblProgresoPartidas.BackColor = Color.Transparent;
            }
            if (lblProgresoGeneralValor != null)
            {
                lblProgresoGeneralValor.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                lblProgresoGeneralValor.ForeColor = Color.FromArgb(46, 160, 89);
                lblProgresoGeneralValor.BackColor = Color.Transparent;
            }
            if (lblProgresoPartidasValor != null)
            {
                lblProgresoPartidasValor.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                lblProgresoPartidasValor.ForeColor = Color.FromArgb(45, 130, 192);
                lblProgresoPartidasValor.BackColor = Color.Transparent;
            }

            // Estadísticas
            if (lblTotalDestajos != null)
            {
                lblTotalDestajos.Font = new Font("Segoe UI", 9F);
                lblTotalDestajos.ForeColor = Color.FromArgb(48, 42, 36);
                lblTotalDestajos.BackColor = Color.Transparent;
            }
            if (lblDestajosCompletados != null)
            {
                lblDestajosCompletados.Font = new Font("Segoe UI", 9F);
                lblDestajosCompletados.ForeColor = Color.FromArgb(46, 160, 89);
                lblDestajosCompletados.BackColor = Color.Transparent;
            }
            if (lblDestajosPendientes != null)
            {
                lblDestajosPendientes.Font = new Font("Segoe UI", 9F);
                lblDestajosPendientes.ForeColor = Color.FromArgb(217, 142, 24);
                lblDestajosPendientes.BackColor = Color.Transparent;
            }
            if (lblUltimaActualizacion != null)
            {
                lblUltimaActualizacion.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic);
                lblUltimaActualizacion.ForeColor = Color.FromArgb(140, 134, 124);
                lblUltimaActualizacion.BackColor = Color.Transparent;
            }

            // Mapa
            if (lblInstruccionesMapa != null)
            {
                lblInstruccionesMapa.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic);
                lblInstruccionesMapa.ForeColor = Color.FromArgb(140, 134, 124);
                lblInstruccionesMapa.BackColor = Color.Transparent;
            }
            if (lblZoomNivel != null)
            {
                lblZoomNivel.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
                lblZoomNivel.ForeColor = Color.FromArgb(60, 50, 40);
                lblZoomNivel.BackColor = Color.FromArgb(235, 255, 255, 255);
            }

            // Empty placeholder de la foto
            if (lblFotoEmpty != null)
            {
                lblFotoEmpty.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
                lblFotoEmpty.ForeColor = Color.FromArgb(160, 154, 144);
                lblFotoEmpty.BackColor = Color.Transparent;
            }

            // TextBoxes redondeados: forzar border None (ThemeManager pone FixedSingle)
            if (txtManzana != null)
            {
                txtManzana.BorderStyle = BorderStyle.None;
                txtManzana.Font = new Font("Segoe UI", 10.5F);
                txtManzana.BackColor = Color.White;
            }
            if (txtLote != null)
            {
                txtLote.BorderStyle = BorderStyle.None;
                txtLote.Font = new Font("Segoe UI", 10.5F);
                txtLote.BackColor = Color.White;
            }
        }

        /// <summary>
        /// Zoom in/out incremental centrado en el centro del mapa.
        /// </summary>
        private void ZoomMapa(float factor)
        {
            if (imagenMapaCompleto == null) return;

            float nuevoZoom = zoomActual * factor;
            const float zoomMin = 0.3f;
            const float zoomMax = 6.0f;
            nuevoZoom = Math.Max(zoomMin, Math.Min(zoomMax, nuevoZoom));

            // Centrar zoom en el centro del control
            float centroX = pictureBoxMapa.Width / 2.0f;
            float centroY = pictureBoxMapa.Height / 2.0f;

            float mapX = (centroX - offsetActual.X) / zoomActual;
            float mapY = (centroY - offsetActual.Y) / zoomActual;

            offsetObjetivo = new PointF(centroX - mapX * nuevoZoom, centroY - mapY * nuevoZoom);
            zoomObjetivo = nuevoZoom;

            IniciarAnimacionZoomOptimizada();
            ActualizarZoomLabel(nuevoZoom);
        }

        private void IniciarAnimacionZoomOptimizada()
        {
            float diferenciaZoom = Math.Abs(zoomObjetivo - zoomActual);
            // Solo regenerar cache si cambio significativo (>50%)
            if (diferenciaZoom > 0.5f) needsRedraw = true;

            estaAnimando = true;
            if (timerZoom != null && !timerZoom.Enabled) timerZoom.Start();
        }

        private void ActualizarZoomLabel(float zoom)
        {
            if (lblZoomNivel != null)
            {
                lblZoomNivel.Text = $"{zoom * 100:F0}%";
            }
        }

        /// <summary>
        /// Actualiza KPIs y barras animadas con los valores calculados.
        /// Centraliza todo el wiring del nuevo dashboard.
        /// </summary>
        private void ActualizarDashboardModerno(
            decimal avanceFisicoPct,
            decimal totalEjecutado,
            decimal totalPresupuestado,
            int totalPartidas,
            int partidasCompletadas,
            int partidasPendientes,
            DateTime? ultimaActualizacion)
        {
            // ---- KPI 1: Avance Físico ----
            if (kpiAvanceFisico != null)
            {
                kpiAvanceFisico.SetValueAnimated((float)avanceFisicoPct, "F1", "%");
                kpiAvanceFisico.Trend = casaActual != null
                    ? $"M{casaActual.Manzana} · L{casaActual.Lote}"
                    : "Sin casa seleccionada";
            }

            // ---- KPI 2: Avance Económico ----
            if (kpiAvanceEconomico != null)
            {
                string val = FormatCurrencyShort(totalEjecutado);
                kpiAvanceEconomico.ValueText = val;
                kpiAvanceEconomico.Trend = totalPresupuestado > 0
                    ? $"de {FormatCurrencyShort(totalPresupuestado)} presupuestado"
                    : "Sin presupuesto";
            }

            // ---- KPI 3: Total Partidas ----
            if (kpiPartidasTotal != null)
            {
                kpiPartidasTotal.SetValueAnimated(totalPartidas, "F0", "");
                kpiPartidasTotal.Trend = casaActual?.Prototipo != null
                    ? $"Prototipo: {casaActual.Prototipo}"
                    : "Filtradas por prototipo";
            }

            // ---- KPI 4: Completadas ----
            if (kpiPartidasCompletadas != null)
            {
                kpiPartidasCompletadas.SetValueAnimated(partidasCompletadas, "F0", "");
                kpiPartidasCompletadas.Trend = $"{partidasPendientes} pendientes";
            }

            // ---- Barras animadas ----
            if (animProgresoGeneral != null)
                animProgresoGeneral.SetValueAnimated((float)avanceFisicoPct);

            if (animProgresoPartidas != null)
            {
                float econPct = totalPresupuestado > 0
                    ? (float)(totalEjecutado / totalPresupuestado) * 100f
                    : 0f;
                econPct = Math.Max(0, Math.Min(100, econPct));
                animProgresoPartidas.SetValueAnimated(econPct);
            }

            // ---- Labels descriptivos ----
            if (lblProgresoGeneralValor != null)
                lblProgresoGeneralValor.Text = $"{avanceFisicoPct:F1}%";

            if (lblProgresoPartidasValor != null)
                lblProgresoPartidasValor.Text = totalPresupuestado > 0
                    ? $"{totalEjecutado:C2}  /  {totalPresupuestado:C2}"
                    : $"{totalEjecutado:C2}";

            if (lblTotalDestajos != null)
                lblTotalDestajos.Text = $"Total de Partidas: {totalPartidas}";
            if (lblDestajosCompletados != null)
                lblDestajosCompletados.Text = $"Partidas Completadas: {partidasCompletadas}";
            if (lblDestajosPendientes != null)
                lblDestajosPendientes.Text = $"Partidas Pendientes: {partidasPendientes}";
            if (lblUltimaActualizacion != null)
            {
                lblUltimaActualizacion.Text = ultimaActualizacion.HasValue
                    ? $"Última actualización: {ultimaActualizacion.Value:dd/MM/yyyy HH:mm}"
                    : "Última actualización: Sin datos";
            }

            // ---- Refresca placeholder de la foto ----
            if (lblFotoEmpty != null && pictureBoxCasa != null)
                lblFotoEmpty.Visible = (pictureBoxCasa.Image == null);
        }

        /// <summary>
        /// Resetea el dashboard a valores vacíos (cuando no hay casa).
        /// </summary>
        private void ResetearDashboardModerno()
        {
            ActualizarDashboardModerno(0m, 0m, 0m, 0, 0, 0, null);
            if (kpiAvanceFisico != null) kpiAvanceFisico.Trend = "Sin casa seleccionada";
            if (kpiAvanceEconomico != null) kpiAvanceEconomico.Trend = "Monto total ejecutado";
            if (kpiPartidasTotal != null) kpiPartidasTotal.Trend = "Filtradas por prototipo";
            if (kpiPartidasCompletadas != null) kpiPartidasCompletadas.Trend = "0 pendientes";

            if (lblCasaActual != null)
            {
                lblCasaActual.Text = "Sin casa seleccionada";
                lblCasaActual.ForeColor = Color.FromArgb(115, 110, 102);
            }
            if (lblManzanaLote != null) lblManzanaLote.Text = "Mz: -  ·  Lote: -";
            if (lblPrototipo != null) lblPrototipo.Text = "Prototipo: -";
            if (lblFotoEmpty != null) lblFotoEmpty.Visible = true;
        }

        /// <summary>
        /// Formatea decimal a "$1.2M" / "$345K" / "$1,234".
        /// </summary>
        private static string FormatCurrencyShort(decimal value)
        {
            if (Math.Abs(value) >= 1_000_000m)
                return $"${(value / 1_000_000m):F2}M";
            if (Math.Abs(value) >= 10_000m)
                return $"${(value / 1_000m):F1}K";
            return $"${value:N0}";
        }

        /// <summary>
        /// Fade-in suave de un panel cambiando BackColor desde fondo a transparente
        /// (simulación porque WinForms Panel no tiene opacity directa).
        /// </summary>
        private void FadeInPanel(Panel panel)
        {
            if (panel == null) return;
            Color targetColor = panel.BackColor;
            // Inicia con un overlay blanco
            using (var t = new Timer { Interval = 16 })
            {
                int step = 0;
                int totalSteps = 14;
                Color startColor = Color.FromArgb(targetColor.R, targetColor.G, targetColor.B);
                Color overlay = Color.White;
                panel.BackColor = overlay;
                Timer tt = new Timer { Interval = 16 };
                tt.Tick += (s, e) =>
                {
                    step++;
                    float p = step / (float)totalSteps;
                    int r = (int)(overlay.R + (targetColor.R - overlay.R) * p);
                    int g = (int)(overlay.G + (targetColor.G - overlay.G) * p);
                    int b = (int)(overlay.B + (targetColor.B - overlay.B) * p);
                    panel.BackColor = Color.FromArgb(r, g, b);
                    if (step >= totalSteps)
                    {
                        panel.BackColor = targetColor;
                        tt.Stop();
                        tt.Dispose();
                    }
                };
                tt.Start();
            }
        }

        /// <summary>
        /// Dibuja un pin pulsante moderno (drop pin con sombra y halo animado).
        /// Reemplaza la lógica de dibujo del pin en PictureBoxMapa_Paint.
        /// </summary>
        internal void DibujarPinModerno(Graphics g, PointF coordenadas, float zoom, bool animando)
        {
            float pinSize = 22 / zoom;
            float halo = pinSize * (1.5f + 0.3f * (float)Math.Sin(pinPulsePhase));

            // Halo pulsante
            if (!animando)
            {
                int alpha = (int)(60 + 40 * Math.Sin(pinPulsePhase));
                alpha = Math.Max(20, Math.Min(110, alpha));
                using (var haloBrush = new SolidBrush(Color.FromArgb(alpha, 231, 76, 60)))
                {
                    g.FillEllipse(haloBrush,
                        coordenadas.X - halo / 2,
                        coordenadas.Y - halo / 2,
                        halo, halo);
                }
            }

            // Sombra del pin
            using (var shadow = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
            {
                g.FillEllipse(shadow,
                    coordenadas.X - pinSize / 2 + 1.5f / zoom,
                    coordenadas.Y - pinSize / 2 + 2 / zoom,
                    pinSize, pinSize);
            }

            // Pin exterior (drop)
            using (var pinBrush = new LinearGradientBrush(
                new RectangleF(coordenadas.X - pinSize / 2, coordenadas.Y - pinSize / 2, pinSize, pinSize),
                Color.FromArgb(231, 76, 60),
                Color.FromArgb(192, 57, 43),
                LinearGradientMode.Vertical))
            {
                g.FillEllipse(pinBrush,
                    coordenadas.X - pinSize / 2,
                    coordenadas.Y - pinSize / 2,
                    pinSize, pinSize);
            }
            using (var border = new Pen(Color.White, 2.5f / zoom))
            {
                g.DrawEllipse(border,
                    coordenadas.X - pinSize / 2,
                    coordenadas.Y - pinSize / 2,
                    pinSize, pinSize);
            }

            // Punto blanco interior
            float innerSize = pinSize * 0.4f;
            using (var inner = new SolidBrush(Color.White))
            {
                g.FillEllipse(inner,
                    coordenadas.X - innerSize / 2,
                    coordenadas.Y - innerSize / 2,
                    innerSize, innerSize);
            }
        }

        /// <summary>
        /// Throttle para MouseMove durante drag - reduce número de re-renders.
        /// Devuelve true si se debe procesar este movimiento.
        /// </summary>
        internal bool DebeRenderizarMouseMove()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastMouseMoveRender).TotalMilliseconds < MouseMoveThrottleMs)
                return false;
            _lastMouseMoveRender = now;
            return true;
        }

        /// <summary>
        /// Calcula los avances físico y económico replicando la lógica de
        /// FormActivarTareasTreeList.ActualizarEstadisticas. Lee de las tablas
        /// dinámicas {Ruta}, {Ruta}_Columnas y ActivacionTareasRuta.
        ///
        ///   • Avance físico (%) = destajos finalizados / total destajos × 100
        ///   • Avance económico ($) = suma(Total) de subtareas Nivel==2 con Activa
        ///       (Total = Cantidad × PrecioUnitario, igual que ItemTareaActivacion)
        ///   • Total presupuestado ($) = suma(Total) de TODAS las subtareas Nivel==2
        /// </summary>
        private void CargarProgresoDesdeActivacion()
        {
            if (casaActual == null)
            {
                ResetearDashboardModerno();
                return;
            }

            // Determinar ruta según prototipo (igual que FormActivarTareasTreeList)
            string rutaActual = (!string.IsNullOrEmpty(casaActual.Prototipo) &&
                                 casaActual.Prototipo.ToUpper().Contains("CALANDRA"))
                ? "RutaCalandraDestajo"
                : "RutaTuneraDestajo";

            string connStr;
            try
            {
                connStr = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            }
            catch
            {
                ResetearDashboardModerno();
                return;
            }

            try
            {
                // 1) Cargar nodos del árbol con Cantidad y PrecioUnitario
                var nodos = new List<NodoActivacion>(256);
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Verificar que la tabla exista
                    bool tablaExiste = false;
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = @t", conn))
                    {
                        cmd.Parameters.AddWithValue("@t", rutaActual);
                        tablaExiste = ((int)cmd.ExecuteScalar()) > 0;
                    }

                    if (!tablaExiste)
                    {
                        // No hay ruta cargada → resetear dashboard
                        ActualizarDashboardModerno(0m, 0m, 0m, 0, 0, 0, null);
                        if (kpiAvanceFisico != null) kpiAvanceFisico.Trend = "Sin ruta crítica cargada";
                        return;
                    }

                    string sqlNodos = $@"
                        SELECT
                            r.ID,
                            r.ParentId,
                            r.Nivel,
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad,
                            ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS Precio
                        FROM {rutaActual} r
                        LEFT JOIN {rutaActual}_Columnas c ON r.ID = c.NodoID
                        GROUP BY r.ID, r.ParentId, r.Nivel";

                    using (var cmd = new SqlCommand(sqlNodos, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["ID"]);
                            int parentId = reader["ParentId"] != DBNull.Value
                                ? Convert.ToInt32(reader["ParentId"]) : 0;
                            int nivel = Convert.ToInt32(reader["Nivel"]);

                            decimal cant = 0, prec = 0;
                            decimal.TryParse(reader["Cantidad"]?.ToString(),
                                NumberStyles.Any, CultureInfo.InvariantCulture, out cant);
                            decimal.TryParse(reader["Precio"]?.ToString(),
                                NumberStyles.Any, CultureInfo.InvariantCulture, out prec);

                            nodos.Add(new NodoActivacion
                            {
                                Id = id,
                                ParentId = parentId,
                                Nivel = nivel,
                                Cantidad = cant,
                                Precio = prec
                            });
                        }
                    }

                    // 2) Cargar estado de activación de los nodos para esta casa
                    var activaciones = new Dictionary<int, EstadoActivacion>(nodos.Count);

                    // Asegurar que la tabla exista (idempotente)
                    string sqlEnsure = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivacionTareasRuta')
                        BEGIN
                            CREATE TABLE ActivacionTareasRuta (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Manzana NVARCHAR(10), Lote NVARCHAR(10),
                                Prototipo NVARCHAR(50), Ruta NVARCHAR(50),
                                NodoID INT, NombreTarea NVARCHAR(200),
                                Activa BIT, CuadrillaAsignada NVARCHAR(20) NULL,
                                DesatajoActivado BIT DEFAULT 0, Finalizado BIT DEFAULT 0,
                                FechaFinalizacion DATETIME NULL,
                                FechaActualizacion DATETIME DEFAULT GETDATE()
                            );
                        END";
                    using (var cmd = new SqlCommand(sqlEnsure, conn)) cmd.ExecuteNonQuery();

                    string sqlAct = @"
                        SELECT NodoID, Activa,
                               ISNULL(DesatajoActivado, 0) AS DesatajoActivado,
                               ISNULL(Finalizado, 0)       AS Finalizado,
                               FechaActualizacion
                        FROM ActivacionTareasRuta
                        WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta";

                    DateTime? ultimaActualizacion = null;

                    using (var cmd = new SqlCommand(sqlAct, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", casaActual.Manzana);
                        cmd.Parameters.AddWithValue("@l", casaActual.Lote);
                        cmd.Parameters.AddWithValue("@ruta", rutaActual);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = Convert.ToInt32(reader["NodoID"]);
                                activaciones[id] = new EstadoActivacion
                                {
                                    Activa = Convert.ToBoolean(reader["Activa"]),
                                    DesatajoActivado = Convert.ToBoolean(reader["DesatajoActivado"]),
                                    Finalizado = Convert.ToBoolean(reader["Finalizado"])
                                };

                                if (reader["FechaActualizacion"] != DBNull.Value)
                                {
                                    var f = Convert.ToDateTime(reader["FechaActualizacion"]);
                                    if (!ultimaActualizacion.HasValue || f > ultimaActualizacion.Value)
                                        ultimaActualizacion = f;
                                }
                            }
                        }
                    }

                    // 3) Calcular métricas (idéntico a FormActivarTareasTreeList.ActualizarEstadisticas)
                    var nivel1 = nodos.Where(n => n.Nivel == 1).ToList();
                    var nivel2 = nodos.Where(n => n.Nivel == 2).ToList();

                    int totalDestajos = nivel1.Count;
                    int destajosActivados = nivel1.Count(n =>
                        activaciones.TryGetValue(n.Id, out var a) && a.DesatajoActivado);
                    int destajosFinalizados = nivel1.Count(n =>
                        activaciones.TryGetValue(n.Id, out var a) && a.Finalizado);

                    // Monto activo (presupuesto vivo) = Sum(Total) de subtareas Nivel==2 con Activa=true
                    decimal montoActivo = nivel2
                        .Where(n => activaciones.TryGetValue(n.Id, out var a) && a.Activa)
                        .Sum(n => n.Cantidad * n.Precio);

                    // Monto presupuestado total (todo el catálogo Nivel==2)
                    decimal montoTotal = nivel2.Sum(n => n.Cantidad * n.Precio);

                    // Monto ejecutado: subtareas Nivel==2 cuyo destajo padre (Nivel==1) está Finalizado
                    // Resolvemos padre directo y subimos hasta nivel 1
                    var nodoPorId = nodos.ToDictionary(n => n.Id);
                    decimal montoEjecutado = 0m;
                    foreach (var sub in nivel2)
                    {
                        int padreId = sub.ParentId;
                        while (padreId != 0 && nodoPorId.TryGetValue(padreId, out var padre))
                        {
                            if (padre.Nivel == 1)
                            {
                                if (activaciones.TryGetValue(padre.Id, out var aP) && aP.Finalizado)
                                    montoEjecutado += sub.Cantidad * sub.Precio;
                                break;
                            }
                            padreId = padre.ParentId;
                        }
                    }

                    // Avance físico % = destajos finalizados / totales
                    decimal avanceFisico = totalDestajos > 0
                        ? (destajosFinalizados * 100m / totalDestajos)
                        : 0m;

                    int pendientes = totalDestajos - destajosFinalizados;

                    // 4) Refrescar dashboard moderno
                    ActualizarDashboardModerno(
                        avanceFisicoPct: avanceFisico,
                        totalEjecutado: montoEjecutado,
                        totalPresupuestado: montoTotal,
                        totalPartidas: totalDestajos,
                        partidasCompletadas: destajosFinalizados,
                        partidasPendientes: pendientes,
                        ultimaActualizacion: ultimaActualizacion);

                    // Enriquecer KPIs con info adicional de FormActivar
                    if (kpiAvanceFisico != null)
                        kpiAvanceFisico.Trend =
                            $"{destajosActivados} activados · {destajosFinalizados} finalizados";

                    if (kpiAvanceEconomico != null)
                        kpiAvanceEconomico.Trend =
                            $"Activo: {FormatCurrencyShort(montoActivo)} de {FormatCurrencyShort(montoTotal)}";

                    if (kpiPartidasTotal != null)
                        kpiPartidasTotal.Trend =
                            $"Ruta: {(rutaActual == "RutaCalandraDestajo" ? "Calandra" : "Tunera")}";

                    if (kpiPartidasCompletadas != null)
                        kpiPartidasCompletadas.Trend = $"{pendientes} pendientes";

                    // Etiqueta del monto: mostrar activo / total para reflejar mejor el form de activar
                    if (lblProgresoPartidasValor != null)
                        lblProgresoPartidasValor.Text =
                            $"{montoEjecutado:C2}  ·  Activo: {montoActivo:C2}";
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(
                    $"Error al cargar progreso desde activación:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                System.Diagnostics.Debug.WriteLine(
                    $"Error al cargar progreso desde activación: {ex.Message}");
#endif
                ResetearDashboardModerno();
            }
        }

        private class NodoActivacion
        {
            public int Id;
            public int ParentId;
            public int Nivel;
            public decimal Cantidad;
            public decimal Precio;
        }

        private class EstadoActivacion
        {
            public bool Activa;
            public bool DesatajoActivado;
            public bool Finalizado;
        }
    }
}
