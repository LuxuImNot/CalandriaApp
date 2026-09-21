using BrightIdeasSoftware;
using ClosedXML.Excel;
using DynamicSepticSystem;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DynamicSepticSystem
{
    public partial class PanelPrincipal : Form
    {
        private InventarioService inventarioService = new InventarioService();
        List<CasaInventario> inventarioCasas = new List<CasaInventario>();
        private TreeGridController treeCtrl;
        private Dictionary<string, List<TareaRutaCritica>> rutasCriticasPorModelo;
        List<Casa> casas = new List<Casa>();
        Casa casaActual = null;

        // 🗺️ Variables para el sistema de pan & zoom del mapa (OPTIMIZADAS)
        private Image imagenMapaCompleto = null;
        private Bitmap imagenCacheada = null; // 🔥 Cache de la imagen escalada
        private Timer timerZoom;
        private Timer timerSesion;
        private float zoomActual = 1.0f;
        private float zoomObjetivo = 1.0f;
        private PointF offsetActual = new PointF(0, 0);
        private PointF offsetObjetivo = new PointF(0, 0);
        private bool estaAnimando = false;
        
        // 🔥 Optimización de renderizado
        private BufferedGraphicsContext contextoBuffer;
        private BufferedGraphics bufferGrafico;
        private bool needsRedraw = true;
        private Rectangle ultimaRegionDibujada = Rectangle.Empty;
        
        // 🖱️ Variables para pan (desplazamiento) con mouse drag
        private bool estaDraggeando = false;
        private Point puntoInicialDrag;
        private PointF offsetInicialDrag;

        public PanelPrincipal()
        {
            InitializeComponent();
            
            CargarInventarioAlInicio();
            MostrarPermisosEnLabel();
            
            rutasCriticasPorModelo = new Dictionary<string, List<TareaRutaCritica>>();

            if (File.Exists("casas.json"))
                casas = JsonConvert.DeserializeObject<List<Casa>>(File.ReadAllText("casas.json"));

            try
            {
                BuildMainMenu();
            }
            catch { }
            
            // Aplicar tema personalizado (SIN MaterialSkin)
            ThemeManager.AplicarTema(this);
            
            // 🎨 Configurar UI moderna
            ConfigurarUIModerna();

            // 🗺️ Inicializar sistema de mapa
            InicializarSistemaMapa();

            // ✨ Extensiones modernas: KPIs animados, overlay del mapa, pin pulsante
            ConfigurarExtensionesModernas();
            ResetearDashboardModerno();

            // 📝 Crédito discreto al pie del sidebar
            AgregarCreditoLuxuDev();

            // 🔐 Refresca el indicador de sesión (detecta vencimiento del token API)
            timerSesion = new Timer { Interval = 60000 };
            timerSesion.Tick += (s, e) => ActualizarInfoUsuario();
            timerSesion.Start();
        }

        /// <summary>
        /// Inserta un label "A software by LuxuDev" anclado al fondo del sidebar.
        /// Discreto, en tono crema sobre el café corporativo.
        /// </summary>
        private void AgregarCreditoLuxuDev()
        {
            if (panelSidebar == null) return;

            var lbl = new Label
            {
                Name = "lblCreditoLuxuDev",
                Text = "A software by LuxuDev",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 240, 220, 190),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Height = 18,
                Width = panelSidebar.Width - 4,
                Location = new Point(2, panelSidebar.Height - 22)
            };
            panelSidebar.Controls.Add(lbl);
            lbl.BringToFront();
        }
        
        /// <summary>
        /// Inicializa el sistema de pan & zoom para el mapa del sembrado (OPTIMIZADO)
        /// </summary>
        private void InicializarSistemaMapa()
        {
            // 🔥 Inicializar contexto de buffer para double buffering manual
            contextoBuffer = BufferedGraphicsManager.Current;
            
            // Configurar timer para animaciones suaves a 60 FPS
            timerZoom = new Timer();
            timerZoom.Interval = 16; // ~60 FPS (1000ms / 60 = 16.67ms)
            timerZoom.Tick += TimerZoom_Tick;
            
            // Configurar eventos del pictureBoxMapa
            pictureBoxMapa.Paint += PictureBoxMapa_Paint;
            pictureBoxMapa.Click += PictureBoxMapa_Click;
            pictureBoxMapa.Resize += PictureBoxMapa_Resize;
            pictureBoxMapa.SizeMode = PictureBoxSizeMode.Normal;
            
            // 🖱️ Eventos para pan (desplazamiento) con mouse drag
            pictureBoxMapa.MouseDown += PictureBoxMapa_MouseDown;
            pictureBoxMapa.MouseMove += PictureBoxMapa_MouseMove;
            pictureBoxMapa.MouseUp += PictureBoxMapa_MouseUp;
            
            // 🔍 Evento para zoom con rueda del mouse
            pictureBoxMapa.MouseWheel += PictureBoxMapa_MouseWheel;
            
            // 🔥 Habilitar DoubleBuffereding para evitar parpadeos
            typeof(PictureBox).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, pictureBoxMapa, new object[] { true });
            
            // Cargar imagen del mapa del sembrado
            CargarImagenMapaSembrado();
            
            // Mostrar vista completa del sembrado al inicio
            MostrarVistaCompletaSembrado();
        }
        
        /// <summary>
        /// Evento click en el mapa - Alterna entre vista completa y zoom a casa
        /// NOTA: Solo actúa si no se está draggeando (para evitar conflictos con pan)
        /// </summary>
        private void PictureBoxMapa_Click(object sender, EventArgs e)
        {
            // Si no hay casa seleccionada, no hacer nada
            if (casaActual == null) return;
            
            // 🔥 Ignorar click si se acaba de hacer drag (para evitar zoom accidental)
            MouseEventArgs me = e as MouseEventArgs;
            if (me != null)
            {
                // Si el click está muy lejos del punto inicial de drag, fue un drag, no un click
                if (Math.Abs(me.Location.X - puntoInicialDrag.X) > 5 || 
                    Math.Abs(me.Location.Y - puntoInicialDrag.Y) > 5)
                {
                    return;
                }
            }
            
            // 🔥 CORREGIDO: Calcular el zoom de vista completa para comparar correctamente
            float zoomVistaCompleta = Math.Max(
                (float)pictureBoxMapa.Width / imagenMapaCompleto.Width,
                (float)pictureBoxMapa.Height / imagenMapaCompleto.Height
            );
            
            // Si estamos en zoom a casa (zoom mayor que vista completa), volver a vista completa
            // Usar tolerancia de 0.2f para compensar por aproximaciones de float
            if (Math.Abs(zoomActual - zoomObjetivo) < 0.1f && zoomActual > (zoomVistaCompleta + 0.2f))
            {
                MostrarVistaCompletaSembrado();
                
                // Actualizar texto de instrucciones
                var lblInstruccionesMapa = this.Controls.Find("lblInstruccionesMapa", true).FirstOrDefault() as Label;
                if (lblInstruccionesMapa != null)
                {
                    lblInstruccionesMapa.Text = $"📍 Vista completa - Haz clic para hacer zoom a M{casaActual.Manzana} L{casaActual.Lote}";
                }
            }
            else
            {
                // Si estamos en vista completa (o animando), hacer zoom a la casa
                HacerZoomACasa(casaActual.Manzana, casaActual.Lote);
                
                // Actualizar texto de instrucciones
                var lblInstruccionesMapa = this.Controls.Find("lblInstruccionesMapa", true).FirstOrDefault() as Label;
                if (lblInstruccionesMapa != null)
                {
                    lblInstruccionesMapa.Text = $"📍 M{casaActual.Manzana} L{casaActual.Lote} - Arrastra para mover | Rueda del mouse para zoom";
                }
            }
        }
        
        /// <summary>
        /// Carga la imagen del mapa del sembrado desde recursos o archivo
        /// </summary>
        private void CargarImagenMapaSembrado()
        {
            try
            {
                string mapaPath = Path.Combine(Application.StartupPath, "Resources", "MapaSembrado.png");
                
                if (File.Exists(mapaPath))
                {
                    // 🔥 Cargar imagen optimizada para GDI+
                    using (var stream = new FileStream(mapaPath, FileMode.Open, FileAccess.Read))
                    {
                        imagenMapaCompleto = Image.FromStream(stream);
                    }
                }
                else
                {
                    // Crear imagen placeholder si no existe el mapa
                    imagenMapaCompleto = CrearMapaPlaceholder();
                }
                
                // 🔥 Pre-crear cache inicial
                ActualizarCacheImagen();
            }
            catch (Exception ex)
            {
                imagenMapaCompleto = CrearMapaPlaceholder();
                ActualizarCacheImagen();
                
                #if DEBUG
                MessageBox.Show($"No se pudo cargar la imagen del mapa:\n{ex.Message}\n\nSe mostrará un placeholder.", 
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                #endif
            }
        }
        
        /// <summary>
        /// 🔥 Actualiza el cache de la imagen escalada (llamar cuando cambia el zoom significativamente)
        /// </summary>
        private void ActualizarCacheImagen()
        {
            if (imagenMapaCompleto == null || pictureBoxMapa.Width <= 0 || pictureBoxMapa.Height <= 0)
                return;
            
            try
            {
                // Liberar cache anterior
                if (imagenCacheada != null)
                {
                    imagenCacheada.Dispose();
                    imagenCacheada = null;
                }
                
                // Calcular dimensiones del cache basadas en zoom actual
                // 🔥 Ajustado margen de 40% a 30% para zoom más bajo
                float zoomConMargen = zoomActual * 1.3f;
                int anchoCache = (int)(imagenMapaCompleto.Width * zoomConMargen);
                int altoCache = (int)(imagenMapaCompleto.Height * zoomConMargen);
                
                // Limitar tamaño máximo del cache para evitar out of memory
                const int maxDimension = 4096;
                if (anchoCache > maxDimension || altoCache > maxDimension)
                {
                    float escala = Math.Min((float)maxDimension / anchoCache, (float)maxDimension / altoCache);
                    anchoCache = (int)(anchoCache * escala);
                    altoCache = (int)(altoCache * escala);
                }
                
                // Crear nuevo cache
                imagenCacheada = new Bitmap(anchoCache, altoCache, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                
                using (Graphics g = Graphics.FromImage(imagenCacheada))
                {
                    // 🔥 Usar configuración óptima para calidad/velocidad
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    
                    g.DrawImage(imagenMapaCompleto, 0, 0, anchoCache, altoCache);
                }
                
                needsRedraw = true;
            }
            catch (OutOfMemoryException)
            {
                // Si falla por memoria, usar la original sin cache
                imagenCacheada = null;
            }
        }
        
        /// <summary>
        /// Crea una imagen placeholder para el mapa cuando no existe el archivo real
        /// </summary>
        private Image CrearMapaPlaceholder()
        {
            // Crear imagen de 1200x800 con plano simulado del sembrado
            Bitmap bmp = new Bitmap(1200, 800);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(245, 245, 245));
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Dibujar grid de manzanas (ejemplo: 5 manzanas)
                int manzanas = 5;
                int anchoManzana = 200;
                int altoManzana = 150;
                int espacioX = 20;
                int espacioY = 20;
                int offsetX = 100;
                int offsetY = 100;
                
                Random rnd = new Random(42); // Seed fijo para consistencia
                
                for (int mz = 1; mz <= manzanas; mz++)
                {
                    int x = offsetX + ((mz - 1) % 3) * (anchoManzana + espacioX);
                    int y = offsetY + ((mz - 1) / 3) * (altoManzana + espacioY);
                    
                    // Dibujar rectángulo de manzana
                    using (Brush brush = new SolidBrush(Color.FromArgb(220, 230, 240)))
                    using (Pen pen = new Pen(Color.FromArgb(100, 120, 140), 2))
                    {
                        g.FillRectangle(brush, x, y, anchoManzana, altoManzana);
                        g.DrawRectangle(pen, x, y, anchoManzana, altoManzana);
                    }
                    
                    // Dibujar lotes dentro de la manzana (4x2 lotes)
                    int lotesX = 4;
                    int lotesY = 2;
                    int anchoLote = anchoManzana / lotesX;
                    int altoLote = altoManzana / lotesY;
                    
                    for (int ly = 0; ly < lotesY; ly++)
                    {
                        for (int lx = 0; lx < lotesX; lx++)
                        {
                            int loteX = x + lx * anchoLote;
                            int loteY = y + ly * altoLote;
                            
                            // Dibujar borde del lote
                            using (Pen penLote = new Pen(Color.FromArgb(150, 160, 170), 1))
                            {
                                g.DrawRectangle(penLote, loteX, loteY, anchoLote, altoLote);
                            }
                            
                            // Dibujar número de lote
                            int loteNum = ly * lotesX + lx + 1;
                            string textoLote = $"M{mz}L{loteNum}";
                            using (Font font = new Font("Segoe UI", 7, FontStyle.Regular))
                            using (Brush textBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
                            {
                                SizeF textSize = g.MeasureString(textoLote, font);
                                float textX = loteX + (anchoLote - textSize.Width) / 2;
                                float textY = loteY + (altoLote - textSize.Height) / 2;
                                g.DrawString(textoLote, font, textBrush, textX, textY);
                            }
                        }
                    }
                    
                    // Etiqueta de manzana
                    using (Font font = new Font("Segoe UI", 12, FontStyle.Bold))
                    using (Brush textBrush = new SolidBrush(Color.FromArgb(60, 80, 100)))
                    {
                        string textoManzana = $"MANZANA {mz}";
                        SizeF textSize = g.MeasureString(textoManzana, font);
                        g.DrawString(textoManzana, font, textBrush, x + 5, y - 25);
                    }
                }
                
                // Título del plano
                using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(88, 53, 23)))
                {
                    g.DrawString("CALANDRIA RESIDENCIAL - PLANO DE SEMBRADO", font, textBrush, 150, 20);
                }
                
                // Nota informativa
                using (Font font = new Font("Segoe UI", 9, FontStyle.Italic))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    g.DrawString("💡 Este es un mapa de ejemplo. Coloca MapaSembrado.png en Resources para usar el plano real.", 
                        font, textBrush, 150, 720);
                }
            }
            
            return bmp;
        }
        
        /// <summary>
        /// Muestra la vista completa del sembrado (zoom out)
        /// </summary>
        private void MostrarVistaCompletaSembrado()
        {
            if (imagenMapaCompleto == null) return;
            
            // 🔥 CORREGIDO: Asegurar que el PictureBox tiene tamaño válido
            if (pictureBoxMapa.Width <= 0 || pictureBoxMapa.Height <= 0)
            {
                // Si el control aún no tiene tamaño, esperar al siguiente ciclo de layout
                pictureBoxMapa.BeginInvoke(new Action(() => MostrarVistaCompletaSembrado()));
                return;
            }
            
            // Calcular zoom para CONTENER toda la imagen al control (todo el mapa visible)
            float zoomX = (float)pictureBoxMapa.Width / imagenMapaCompleto.Width;
            float zoomY = (float)pictureBoxMapa.Height / imagenMapaCompleto.Height;

            // FIT MODE: usar el zoom mínimo para que toda la imagen sea visible (zoom 0 por defecto)
            zoomObjetivo = Math.Min(zoomX, zoomY);
            
            // Calcular offset para centrar la imagen
            float anchoEscalado = imagenMapaCompleto.Width * zoomObjetivo;
            float altoEscalado = imagenMapaCompleto.Height * zoomObjetivo;
            
            offsetObjetivo = new PointF(
                (pictureBoxMapa.Width - anchoEscalado) / 2.0f,
                (pictureBoxMapa.Height - altoEscalado) / 2.0f
            );
            
            // Iniciar animación suave
            IniciarAnimacionZoom();
        }
        
        /// <summary>
        /// Hace zoom hacia una casa específica en el mapa
        /// </summary>
        private void HacerZoomACasa(string manzana, string lote)
        {
            if (imagenMapaCompleto == null) return;
            
            // Obtener coordenadas de la casa en el mapa
            PointF coordenadasCasa = ObtenerCoordenadasCasaEnMapa(manzana, lote);
            
            // 🔥 Zoom más alejado para ver contexto alrededor de la casa (reducido de 2.5f a 1.5f)
            zoomObjetivo = 1.5f;
            
            // Calcular offset para centrar la casa
            float centroX = pictureBoxMapa.Width / 2.0f;
            float centroY = pictureBoxMapa.Height / 2.0f;
            
            offsetObjetivo = new PointF(
                centroX - coordenadasCasa.X * zoomObjetivo,
                centroY - coordenadasCasa.Y * zoomObjetivo
            );
            
            // Iniciar animación suave
            IniciarAnimacionZoom();
        }
        
        /// <summary>
        /// Obtiene las coordenadas de una casa en el mapa
        /// NOTA: Lee las coordenadas desde coordenadas_mapa.json si existe
        /// </summary>
        private PointF ObtenerCoordenadasCasaEnMapa(string manzana, string lote)
        {
            // Intentar cargar coordenadas desde el archivo JSON
            try
            {
                string archivoConfig = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, 
                    "coordenadas_mapa.json"
                );
                
                if (File.Exists(archivoConfig))
                {
                    string json = File.ReadAllText(archivoConfig);
                    var coordenadas = JsonConvert.DeserializeObject<List<CoordenadasCasa>>(json);
                    
                    if (coordenadas != null)
                    {
                        // Buscar la casa específica
                        string key = $"{manzana}_{lote}";
                        var coord = coordenadas.FirstOrDefault(c => c.Key == key);
                        
                        if (coord != null)
                        {
                            #if DEBUG
                            System.Diagnostics.Debug.WriteLine($"Coordenadas encontradas para M{manzana}L{lote}: X={coord.X}, Y={coord.Y}");
                            #endif
                            return new PointF(coord.X, coord.Y);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"Error al cargar coordenadas desde JSON: {ex.Message}");
                #endif
            }
            
            // Si no se encontraron coordenadas en el JSON, usar cálculo basado en placeholder
            if (!int.TryParse(manzana, out int mz) || !int.TryParse(lote, out int lt))
            {
                return new PointF(600, 400); // Centro del mapa como fallback
            }
            
            // Cálculo basado en el mapa placeholder (ajustar según mapa real)
            int offsetX = 100;
            int offsetY = 100;
            int anchoManzana = 200;
            int altoManzana = 150;
            int espacioX = 20;
            int espacioY = 20;
            
            // Posición de la manzana
            int mzX = offsetX + ((mz - 1) % 3) * (anchoManzana + espacioX);
            int mzY = offsetY + ((mz - 1) / 3) * (altoManzana + espacioY);
            
            // Posición del lote dentro de la manzana (4x2 grid)
            int lotesX = 4;
            int anchoLote = anchoManzana / lotesX;
            int altoLote = altoManzana / 2;
            
            int loteCol = (lt - 1) % lotesX;
            int loteRow = (lt - 1) / lotesX;
            
            float casaX = mzX + loteCol * anchoLote + anchoLote / 2.0f;
            float casaY = mzY + loteRow * altoLote + altoLote / 2.0f;
            
            #if DEBUG
            System.Diagnostics.Debug.WriteLine($"Usando coordenadas calculadas para M{manzana}L{lote}: X={casaX}, Y={casaY}");
            #endif
            
            return new PointF(casaX, casaY);
        }
        
        /// <summary>
        /// Clase para deserializar coordenadas desde JSON
        /// </summary>
        private class CoordenadasCasa
        {
            public string Manzana { get; set; }
            public string Lote { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            
            public string Key => $"{Manzana}_{Lote}";
        }
        
        /// <summary>
        /// Inicia la animación suave de zoom
        /// </summary>
        private void IniciarAnimacionZoom()
        {
            // 🔥 Determinar si necesitamos actualizar el cache
            float diferenciaZoom = Math.Abs(zoomObjetivo - zoomActual);
            if (diferenciaZoom > 0.3f) // Solo regenerar cache si el cambio es significativo
            {
                // Marcar que necesitamos actualizar cache, pero hacerlo después de la animación
                needsRedraw = true;
            }
            
            estaAnimando = true;
            timerZoom.Start();
        }
        
        /// <summary>
        /// Timer para animación suave de zoom (interpolación) - OPTIMIZADO A 60 FPS
        /// </summary>
        private void TimerZoom_Tick(object sender, EventArgs e)
        {
            const float velocidad = 0.20f; // Velocidad de interpolación optimizada
            
            // Interpolar zoom
            float difZoom = zoomObjetivo - zoomActual;
            if (Math.Abs(difZoom) > 0.01f)
            {
                zoomActual += difZoom * velocidad;
            }
            else
            {
                zoomActual = zoomObjetivo;
            }
            
            // Interpolar offset
            float difX = offsetObjetivo.X - offsetActual.X;
            float difY = offsetObjetivo.Y - offsetActual.Y;
            
            if (Math.Abs(difX) > 0.5f || Math.Abs(difY) > 0.5f)
            {
                offsetActual.X += difX * velocidad;
                offsetActual.Y += difY * velocidad;
            }
            else
            {
                offsetActual = offsetObjetivo;
            }
            
            // 🔥 Invalidar solo la región que cambió (más eficiente)
            pictureBoxMapa.Invalidate();
            ActualizarZoomLabel(zoomActual);

            // Detener animación si llegamos al objetivo
            if (Math.Abs(difZoom) <= 0.01f && Math.Abs(difX) <= 0.5f && Math.Abs(difY) <= 0.5f)
            {
                estaAnimando = false;
                timerZoom.Stop();

                // 🔥 Actualizar cache ahora que terminó la animación
                if (needsRedraw)
                {
                    ActualizarCacheImagen();
                    pictureBoxMapa.Invalidate();
                }
                ActualizarZoomLabel(zoomActual);
            }
        }
        
        /// <summary>
        /// Dibuja el mapa con zoom y pan aplicados - OPTIMIZADO PARA 60 FPS
        /// </summary>
        private void PictureBoxMapa_Paint(object sender, PaintEventArgs e)
        {
            if (imagenMapaCompleto == null) return;
            
            Graphics g = e.Graphics;
            
            // 🔥 Configuración óptima para velocidad durante animación
            if (estaAnimando)
            {
                // Modo rápido durante animación
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.Low; // Más rápido durante animación
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            }
            else
            {
                // Alta calidad cuando está quieto
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            }
            
            // 🔥 Usar imagen cacheada si está disponible, sino usar original
            Image imagenADibujar = imagenCacheada ?? imagenMapaCompleto;
            
            // Calcular dimensiones finales
            float anchoFinal = imagenMapaCompleto.Width * zoomActual;
            float altoFinal = imagenMapaCompleto.Height * zoomActual;
            
            // Aplicar transformación de zoom y pan
            g.TranslateTransform(offsetActual.X, offsetActual.Y);
            g.ScaleTransform(zoomActual, zoomActual);
            
            // Dibujar imagen del mapa
            g.DrawImage(imagenMapaCompleto, 0, 0, imagenMapaCompleto.Width, imagenMapaCompleto.Height);
            
            // Dibujar pin moderno (con halo pulsante)
            if (casaActual != null)
            {
                PointF coordenadas = ObtenerCoordenadasCasaEnMapa(casaActual.Manzana, casaActual.Lote);
                DibujarPinModerno(g, coordenadas, zoomActual, estaAnimando);

                // Etiqueta con manzana y lote (solo si no estamos animando)
                if (!estaAnimando)
                {
                    float pinSize = 22 / zoomActual;
                    float fontSize = 10 / zoomActual;
                    using (Font font = new Font("Segoe UI", fontSize, FontStyle.Bold))
                    using (Brush textBrush = new SolidBrush(Color.FromArgb(50, 30, 20)))
                    using (Brush bgBrush = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
                    using (Pen bgPen = new Pen(Color.FromArgb(220, 215, 207), 1f / zoomActual))
                    {
                        string texto = $"M{casaActual.Manzana} · L{casaActual.Lote}";
                        SizeF textSize = g.MeasureString(texto, font);

                        var bgRect = new RectangleF(
                            coordenadas.X - textSize.Width / 2 - 6,
                            coordenadas.Y - pinSize - textSize.Height - 6,
                            textSize.Width + 12,
                            textSize.Height + 6);
                        g.FillRectangle(bgBrush, bgRect);
                        g.DrawRectangle(bgPen, bgRect.X, bgRect.Y, bgRect.Width, bgRect.Height);

                        g.DrawString(texto, font, textBrush,
                            coordenadas.X - textSize.Width / 2,
                            coordenadas.Y - pinSize - textSize.Height - 3);
                    }
                }
            }
        }
        
        /// <summary>
        /// Configura la interfaz de usuario moderna con estilos profesionales
        /// </summary>
        private void ConfigurarUIModerna()
        {
            // Configurar panel sidebar (verificar si existe)
            var panelSidebar = this.Controls.Find("panelSidebar", true).FirstOrDefault() as Panel;
            if (panelSidebar != null)
            {
                panelSidebar.BackColor = ThemeManager.ColorPrincipal; // Café oscuro para mejor contraste
            }
            
            // Configurar panel de contenido (verificar si existe)
            var panelContenido = this.Controls.Find("panelContenido", true).FirstOrDefault() as Panel;
            if (panelContenido != null)
            {
                panelContenido.BackColor = Color.FromArgb(243, 244, 246); // Gris claro moderno
            }
            
            // Configurar labels del sidebar (verificar si existen)
            var lblUser = this.Controls.Find("lblUser", true).FirstOrDefault() as Label;
            if (lblUser != null)
            {
                lblUser.ForeColor = Color.White;
                lblUser.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            }
            
            var lblRol = this.Controls.Find("lblRol", true).FirstOrDefault() as Label;
            if (lblRol != null)
            {
                lblRol.ForeColor = Color.FromArgb(220, 220, 220);
                lblRol.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            }

            // Selector persistente de obra: un clic reabre la pantalla de selección.
            var lblObraActual = this.Controls.Find("lblObraActual", true).FirstOrDefault() as Label;
            if (lblObraActual != null)
                lblObraActual.Click += (s, e) => AbrirSelectorObra();

            // Configurar logo desde Resources
            var pictureBoxLogo = this.Controls.Find("pictureBoxLogo", true).FirstOrDefault() as PictureBox;
            if (pictureBoxLogo != null)
            {
                try
                {
                    // Intentar cargar desde Resources embebidos
                    if (Properties.Resources.Logo != null)
                    {
                        pictureBoxLogo.Image = Properties.Resources.Logo;
                        pictureBoxLogo.BackColor = Color.Transparent;
                    }
                    else
                    {
                        // Si no existe en Resources, intentar desde archivo
                        string logoPath = Path.Combine(Application.StartupPath, "Resources", "logo.png");
                        if (File.Exists(logoPath))
                        {
                            pictureBoxLogo.Image = Image.FromFile(logoPath);
                            pictureBoxLogo.BackColor = Color.Transparent;
                        }
                        else
                        {
                            // Crear un logo de texto si no existe imagen
                            pictureBoxLogo.BackColor = Color.White;
                        }
                    }
                }
                catch
                {
                    // En caso de error, mantener fondo blanco
                    pictureBoxLogo.BackColor = Color.White;
                }
            }
            
            // Actualizar información de usuario
            ActualizarInfoUsuario();
        }
        
        // Etiquetas cortas por módulo para no mostrar las claves crudas de permiso
        // (p. ej. "almacen.ver", "almacen.editar" -> una sola vez "Almacén").
        private static readonly Dictionary<string, string> EtiquetasModulo = new Dictionary<string, string>
        {
            { "sistema", "Sistema" },
            { "almacen", "Almacén" },
            { "compras", "Compras" },
            { "nomina", "Nómina" },
            { "trabajadores", "Trabajadores" },
            { "destajos", "Destajos" },
            { "estimaciones", "Estimaciones" },
            { "errores", "Errores" },
        };

        /// <summary>Agrupa los permisos por módulo (antes del primer punto) para un resumen corto.</summary>
        private static string CategorizarPermisos(List<string> permisos)
        {
            if (permisos == null || permisos.Count == 0)
                return "";

            return string.Join(", ", permisos
                .Select(p => p?.Split('.')[0] ?? "")
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct()
                .Select(m => EtiquetasModulo.TryGetValue(m, out var etiqueta) ? etiqueta : m)
                .OrderBy(m => m));
        }

        private ToolTip tooltipSesion;
        private bool clicSesionConectado;

        /// <summary>
        /// Vuelve a autenticar contra el API sin cerrar la app. La obra elegida y
        /// las ventanas abiertas se conservan: solo se renueva el token.
        /// </summary>
        private void ReloguearApi()
        {
            // WebView2: se abre con Show(), nunca con ShowDialog().
            var login = new FormLogin();
            login.LoginExitoso += (s, e) =>
            {
                login.Close();
                ActualizarInfoUsuario();
            };
            login.Show(this);
        }

        /// <summary>
        /// Actualiza la información del usuario en la interfaz
        /// </summary>
        private void ActualizarInfoUsuario()
        {
            var lblUser = this.Controls.Find("lblUser", true).FirstOrDefault() as Label;
            var lblRol = this.Controls.Find("lblRol", true).FirstOrDefault() as Label;
            var lblSesion = this.Controls.Find("lblSesion", true).FirstOrDefault() as Label;
            var lblObraActual = this.Controls.Find("lblObraActual", true).FirstOrDefault() as Label;
            var panelDevTools = this.Controls.Find("panelDevTools", true).FirstOrDefault() as Panel;

            // Solo responde al clic cuando el indicador está en naranja (sin sesión API).
            if (lblSesion != null && !clicSesionConectado)
            {
                clicSesionConectado = true;
                lblSesion.Click += (s, e) => { if (lblSesion.Cursor == Cursors.Hand) ReloguearApi(); };
            }

            if (lblObraActual != null)
                lblObraActual.Text = "🏗 " + (Global.ObraActualNombre ?? "Sin obra") + " (cambiar)";

            if (tooltipSesion == null)
                tooltipSesion = new ToolTip { AutoPopDelay = 8000, InitialDelay = 300 };

            if (Global.UsuarioActual != null)
            {
                if (lblUser != null)
                    lblUser.Text = $"▶ {Global.UsuarioActual.Nombre}";

                string categorias = CategorizarPermisos(Global.UsuarioActual.Permisos);

                if (lblRol != null)
                {
                    lblRol.Text = !string.IsNullOrEmpty(Global.UsuarioActual.Perfil)
                        ? $"► {Global.UsuarioActual.Perfil}"
                        : (string.IsNullOrEmpty(categorias) ? "► Sin perfil asignado" : $"► {categorias}");

                    string detalle = Global.UsuarioActual.Permisos != null && Global.UsuarioActual.Permisos.Count > 0
                        ? string.Join(", ", Global.UsuarioActual.Permisos)
                        : "Sin permisos asignados";
                    tooltipSesion.SetToolTip(lblRol, detalle);
                }

                // Mostrar/ocultar panel de DevTools según permisos
                bool esAdmin = Global.EsAdmin;
                if (panelDevTools != null)
                    panelDevTools.Visible = esAdmin;

                if (lblSesion != null)
                {
                    // El token puede seguir "presente" pero vencido: se trata como
                    // sesión API inactiva para que el indicador refleje la realidad.
                    bool apiActivo = ApiClient.Autenticado
                        && (!ApiClient.ExpiraUtc.HasValue || ApiClient.ExpiraUtc.Value > DateTime.UtcNow);

                    if (apiActivo)
                    {
                        string vence = ApiClient.ExpiraUtc.HasValue
                            ? $" · vence {ApiClient.ExpiraUtc.Value.ToLocalTime():HH:mm}"
                            : "";
                        lblSesion.Text = $"● API conectada{vence}";
                        lblSesion.ForeColor = Color.FromArgb(46, 204, 113);
                        lblSesion.Cursor = Cursors.Default;
                    }
                    else
                    {
                        // Sin sesión de API el relogueo se hace desde aquí: antes había
                        // que cerrar y volver a abrir la app cuando vencía el token.
                        lblSesion.Text = ApiClient.Autenticado
                            ? "○ Sesión vencida · clic para reconectar"
                            : "○ Sin conexión API · clic para reconectar";
                        lblSesion.ForeColor = Color.FromArgb(230, 126, 34);
                        lblSesion.Cursor = Cursors.Hand;
                    }
                }
            }
            else
            {
                if (lblUser != null)
                    lblUser.Text = "▶ Sin usuario";

                if (lblRol != null)
                {
                    lblRol.Text = "Sin permisos activos";
                    tooltipSesion.SetToolTip(lblRol, "");
                }

                if (lblSesion != null)
                    lblSesion.Text = "";

                if (panelDevTools != null)
                    panelDevTools.Visible = false;
            }
        }

        private void BuildMainMenu()
        {
            var menuStripGeneral = this.Controls.Find("menuStripGeneral", true).FirstOrDefault() as MenuStrip;
            
            if (menuStripGeneral == null)
                return;

            menuStripGeneral.Items.Clear();

            // Aplicar colores del tema al menú - Usar color menu bar
            menuStripGeneral.BackColor = ThemeManager.ColorPrincipalMenuBar;
            menuStripGeneral.ForeColor = ThemeManager.ColorTextoClaro;

            // Compras
            var miCompras = new ToolStripMenuItem("COMPRAS") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.ColorTextoClaro
            };
            var miCompraMulti = new ToolStripMenuItem("Orden de Compra Múltiple") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miCompraMulti.Click += (s, e) => AbrirFormCompraMulti();
            var miCompraIndirecta = new ToolStripMenuItem("Orden de Compra Indirecta") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miCompraIndirecta.Click += (s, e) => AbrirFormCompraIndirecta();
            var miConsultarOrdenes = new ToolStripMenuItem("Consultar Órdenes") // NUEVO
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miConsultarOrdenes.Click += (s, e) => AbrirRepositorioOrdenesCompra(); // Método nuevo para abrir repositorio
            miCompras.DropDownItems.Add(miCompraMulti);
            miCompras.DropDownItems.Add(miCompraIndirecta);
            miCompras.DropDownItems.Add(new ToolStripSeparator());
            miCompras.DropDownItems.Add(miConsultarOrdenes);

            // 📦 ALMACÉN - NUEVO
            var miAlmacen = new ToolStripMenuItem("ALMACÉN") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.ColorTextoClaro
            };
            var miGestionAlmacen = new ToolStripMenuItem("Gestión de Almacén") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miGestionAlmacen.Click += (s, e) => AbrirFormAlmacen();
            miAlmacen.DropDownItems.Add(miGestionAlmacen);

            // Obra
            var miObra = new ToolStripMenuItem("OBRA") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.ColorTextoClaro
            };
            var miRutaCritica = new ToolStripMenuItem("Ruta Crítica") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miRutaCritica.Click += (s, e) => AbrirFormRutaCritica();

            var miAvancePartidas = new ToolStripMenuItem("Avance por Partidas") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miAvancePartidas.Click += (s, e) => AbrirFormAvanceObra();

            var miAvanceConceptos = new ToolStripMenuItem("Avance por Conceptos") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miAvanceConceptos.Click += (s, e) => AbrirFormAvanceConcepto();

            var miEstimacionConcepto = new ToolStripMenuItem("Estimación (Concepto)") 
            { 
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miEstimacionConcepto.Click += (s, e) => AbrirFormEstimacionConcepto();

            // 🔒 HARD PROGRESS - Solo visible para admin
            ToolStripMenuItem miHardProgress = null;
            ToolStripMenuItem miAvanceMasivo = null;
            bool esAdmin = Global.EsAdmin;

            // EDITAR EXPLOSIONES - disponible para todos los usuarios ahora
            var miEditarExplosiones = new ToolStripMenuItem("Editar Explosiones")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miEditarExplosiones.Click += (s, e) => AbrirFormEditarExplosiones();

            if (esAdmin)
            {
                miHardProgress = new ToolStripMenuItem("Hard Progress [ADMIN]") 
                { 
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(231, 76, 60) // Rojo para indicar que es admin
                };
                miHardProgress.Click += (s, e) => AbrirFormHardProgress();

                miAvanceMasivo = new ToolStripMenuItem("Avance Masivo [ADMIN]")
                {
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(231, 76, 60)
                };
                miAvanceMasivo.Click += (s, e) => AbrirFormAvanceMasivo();

                // 🗺️ MAPEAR COORDENADAS - Solo para admin
                var miMapearCoordenadas = new ToolStripMenuItem("Mapear Coordenadas [ADMIN]") 
                { 
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(231, 76, 60) // Rojo para indicar que es admin
                };
                miMapearCoordenadas.Click += (s, e) => AbrirFormMapearCoordenadas();

                miObra.DropDownItems.Add(miMapearCoordenadas);
            }
            
            // Agregar la opción Editar Explosiones para todos
            miObra.DropDownItems.Add(miEditarExplosiones);

            miObra.DropDownItems.Add(miRutaCritica);
            miObra.DropDownItems.Add(new ToolStripSeparator());
            miObra.DropDownItems.Add(miAvancePartidas);
            miObra.DropDownItems.Add(miAvanceConceptos);
            
            // Agregar Hard Progress solo si es admin
            if (esAdmin && miHardProgress != null)
            {
                miObra.DropDownItems.Add(miHardProgress);
                miObra.DropDownItems.Add(miAvanceMasivo);
            }
            
            miObra.DropDownItems.Add(new ToolStripSeparator());
            miObra.DropDownItems.Add(miEstimacionConcepto);
            
            // 🌳 NUEVO: Editor de TreeList
            var miEditorTreeList = new ToolStripMenuItem("🌳 Editor de Tareas")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miEditorTreeList.Click += (s, e) => AbrirFormEditorTreeList();
            miObra.DropDownItems.Add(miEditorTreeList);

            // ⚙️ NUEVO: Activar/Desactivar Tareas (FormActivarTareasTreeList)
            var miActivarTareas = new ToolStripMenuItem("⚙️ Activar/Desactivar Tareas")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miActivarTareas.Click += (s, e) => AbrirFormActivarTareasTreeList();
            miObra.DropDownItems.Add(miActivarTareas);
            
            // 📷 Evidencias Fotográficas - dentro de OBRA
            var miEvidenciasFotograficas = new ToolStripMenuItem("📷 Evidencias Fotográficas")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miEvidenciasFotograficas.Click += (s, e) => AbrirFormEvidencias();
            miObra.DropDownItems.Add(miEvidenciasFotograficas);

            // 👷 NOMINA - Trabajadores y Cuadrillas
            var miPersonal = new ToolStripMenuItem("NOMINA")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.ColorTextoClaro
            };
            var miTrabajadores = new ToolStripMenuItem("Trabajadores")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miTrabajadores.Click += (s, e) => AbrirFormRegistrarTrabajador();
            var miCuadrillas = new ToolStripMenuItem("Cuadrillas")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miCuadrillas.Click += (s, e) => AbrirFormGestionCuadrillas();
            var miPerfiles = new ToolStripMenuItem("Perfiles / Recibos")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miPerfiles.Click += (s, e) => AbrirFormPerfilTrabajador();
            miPersonal.DropDownItems.Add(miTrabajadores);
            miPersonal.DropDownItems.Add(miCuadrillas);
            miPersonal.DropDownItems.Add(miPerfiles);

            // 💼 ADMINISTRATIVOS - Concentrado financiero por casa
            var miAdministrativos = new ToolStripMenuItem("ADMINISTRATIVOS")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.ColorTextoClaro
            };
            var miConsultaPorCasa = new ToolStripMenuItem("Consulta por Casa (Dinero)")
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.ColorTextoOscuro
            };
            miConsultaPorCasa.Click += (s, e) => AbrirFormAdministrativos();
            miAdministrativos.DropDownItems.Add(miConsultaPorCasa);

            // Solo el administrador ve/usa el Registro de Errores.
            if (Global.EsAdmin)
            {
                var miRegistroErrores = new ToolStripMenuItem("Registro de Errores [ADMIN]")
                {
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = ThemeManager.ColorTextoOscuro
                };
                miRegistroErrores.Click += (s, e) => AbrirFormLogErrores();
                miAdministrativos.DropDownItems.Add(miRegistroErrores);

                var miInversionGeneral = new ToolStripMenuItem("Inversión General [ADMIN]")
                {
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = ThemeManager.ColorTextoOscuro
                };
                miInversionGeneral.Click += (s, e) => AbrirFormInversionWeb();
                miAdministrativos.DropDownItems.Add(miInversionGeneral);

                var miPerfilesPermisos = new ToolStripMenuItem("Perfiles y Permisos [ADMIN]")
                {
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = ThemeManager.ColorTextoOscuro
                };
                miPerfilesPermisos.Click += (s, e) => AbrirFormPerfilesWeb();
                miAdministrativos.DropDownItems.Add(miPerfilesPermisos);
            }

            // Facturación IA: información de facturación del proveedor del sistema
            // (Pilaris), no del cliente — permiso propio, no basta con ser admin de obra.
            if (Global.UsuarioActual?.TienePermiso("sistema.facturacion") == true)
            {
                var miFacturacionIa = new ToolStripMenuItem("Facturación IA [ADMIN]")
                {
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = ThemeManager.ColorTextoOscuro
                };
                miFacturacionIa.Click += (s, e) => AbrirFormFacturacionWeb();
                miAdministrativos.DropDownItems.Add(miFacturacionIa);
            }

            // Alta de clientes nuevos: solo el operador de la plataforma
            // (secrets.config -> SuperAdmins en el servidor), nunca el admin de un cliente.
            if (Global.EsSuperAdmin)
            {
                var miClientes = new ToolStripMenuItem("Clientes [SUPERADMIN]")
                {
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = ThemeManager.ColorTextoOscuro
                };
                miClientes.Click += (s, e) => AbrirFormClientesWeb();
                miAdministrativos.DropDownItems.Add(miClientes);
            }

            menuStripGeneral.Items.AddRange(new ToolStripItem[] { miCompras, miAlmacen, miObra, miPersonal, miAdministrativos });

            menuStripGeneral.Visible = false; // reemplazado por ModernNavBar

            // ✨ Construir items del ModernNavBar a partir del MenuStrip (mantiene handlers)
            BuildModernNavBarFromMenu();
        }

        /// <summary>
        /// Convierte el árbol de menuStripGeneral a items del ModernNavBar,
        /// preservando todos los handlers de Click ya conectados.
        /// </summary>
        private void BuildModernNavBarFromMenu()
        {
            if (modernNavBar == null || menuStripGeneral == null) return;

            var navItems = new System.Collections.Generic.List<DynamicSepticSystem.ModernUI.ModernNavBar.NavItem>();
            foreach (ToolStripItem topItem in menuStripGeneral.Items)
            {
                if (!(topItem is ToolStripMenuItem topMi)) continue;

                var navItem = new DynamicSepticSystem.ModernUI.ModernNavBar.NavItem
                {
                    Text = topMi.Text
                };

                foreach (ToolStripItem sub in topMi.DropDownItems)
                {
                    if (sub is ToolStripSeparator)
                    {
                        navItem.Children.Add(new DynamicSepticSystem.ModernUI.ModernNavBar.NavSubItem
                        {
                            IsSeparator = true
                        });
                    }
                    else if (sub is ToolStripMenuItem subMi)
                    {
                        var captured = subMi;
                        bool isAdminItem = captured.Text != null && captured.Text.Contains("[ADMIN]");
                        navItem.Children.Add(new DynamicSepticSystem.ModernUI.ModernNavBar.NavSubItem
                        {
                            Text = captured.Text,
                            IsBold = isAdminItem,
                            AccentColor = isAdminItem ? Color.FromArgb(198, 65, 56) : (Color?)null,
                            OnClick = () => captured.PerformClick()
                        });
                    }
                }

                navItems.Add(navItem);
            }

            modernNavBar.SetItems(navItems);
        }

        string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        public List<TareaRutaCritica> ObtenerTareasLineales(List<TareaRutaCritica> jerarquicas)
        {
            List<TareaRutaCritica> resultado = new List<TareaRutaCritica>();
            void Recolectar(TareaRutaCritica t)
            {
                resultado.Add(t);
                foreach (var hijo in t.Hijos)
                    Recolectar(hijo);
            }

            foreach (var raiz in jerarquicas)
                Recolectar(raiz);

            return resultado;
        }

        private void CargarInventarioAlInicio()
        {
            try
            {
                inventarioCasas = inventarioService.LeerInventarioCasasSQL();
                // Opcional: Mostrar mensaje solo en debug
                #if DEBUG
                MessageBox.Show($"[OK] Inventario cargado: {inventarioCasas.Count} casas", 
                    "Carga exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                #endif
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[ERROR] Error al cargar el inventario desde SQL Server:\n\n{ex.Message}", 
                    "Error de carga", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void MostrarPermisosEnLabel()
        {
            ActualizarInfoUsuario();
        }


        private void MostrarLayoutCuadrilla(string codigoCuadrilla)
        {
            // El TreeListView/flow panel anterior fue removido del diseñador.
            // Mantener el método como no-op para evitar referencias en otras partes del código.
            return;
        }

        private Control CrearTarjetaMiembro(string nombre, byte[] fotoBytes, bool esJefe)
        {
            // Devuelve un control simple pero no lo añade a ninguna UI aquí (UI antigua eliminada).
            var panel = new Panel
            {
                Width = 90,
                Height = 110,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };

            var pic = new PictureBox
            {
                Width = 80,
                Height = 80,
                Top = 5,
                Left = 5,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            if (fotoBytes != null)
            {
                using (var ms = new MemoryStream(fotoBytes))
                {
                    pic.Image = Image.FromStream(ms);
                }
            }

            var lbl = new Label
            {
                Text = nombre,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 20
            };

            panel.Controls.Add(pic);
            panel.Controls.Add(lbl);

            return panel;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ActualizarInfoUsuario();
            
            string basePath = Application.StartupPath;
            
            try
            {
                BuildMainMenu();
            }
            catch { }

            // Panel híbrido: sustituye el área central por la UI web servida por el
            // API. Si falla o WebView2 no está, deja el panel clásico intacto.
            // Se apaga con PanelWeb=false en App.config.
            InicializarPanelWeb();
        }
        public List<TareaRutaCritica> ConstruirJerarquiaPorWBS(List<TareaRutaCritica> tareasPlanas)
        {
            var lookup = tareasPlanas.ToDictionary(t => t.WBS);
            var hijosAsociados = new HashSet<string>();
            List<TareaRutaCritica> raiz = new List<TareaRutaCritica>();

            foreach (var tarea in tareasPlanas)
            {
                bool asignado = false;

                // Buscar el primer padre válido de arriba hacia abajo
                var niveles = tarea.WBS.Split('.');
                for (int i = niveles.Length - 1; i >= 1; i--)
                {
                    var padreWBS = string.Join(".", niveles.Take(i));
                    if (lookup.TryGetValue(padreWBS, out var padre))
                    {
                        padre.Hijos.Add(tarea);
                        hijosAsociados.Add(tarea.WBS);
                        asignado = true;
                        break;
                    }
                }

                if (!asignado)
                    raiz.Add(tarea); // No encontró padre, va a raíz
            }

            return raiz.Where(t => !hijosAsociados.Contains(t.WBS)).ToList();
        }




        void AplicarProgresoRuta(List<TareaRutaCritica> tareasRaiz, List<string> wbsCompletados)
        {
            void Recursivo(TareaRutaCritica t)
            {
                t.Completado = wbsCompletados.Contains(t.WBS);
                foreach (var h in t.Hijos) Recursivo(h);
            }
            foreach (var t in tareasRaiz) Recursivo(t);
        }




        public List<DestajoInfo> LeerDestajosDeExcel(string rutaExcel, string nombreHoja = "Task_Table")
        {
            var destajos = new List<DestajoInfo>();
            using (var workbook = new XLWorkbook(rutaExcel))
            {
                var ws = workbook.Worksheet(nombreHoja);
                var rows = ws.RangeUsed().RowsUsed().Skip(1); // Salta encabezado

                foreach (var row in rows)
                {
                    int id = int.TryParse(row.Cell(1).GetValue<string>(), out int tmpId) ? tmpId : 0;
                    string nombre = row.Cell(2).GetValue<string>();
                    string duracionTxt = row.Cell(5).GetValue<string>().Replace("days", "").Trim();
                    double duracion = double.TryParse(duracionTxt, out double d) ? d : 0;

                    destajos.Add(new DestajoInfo
                    {
                        Id = id,
                        Nombre = nombre,
                        DuracionDias = duracion
                    });
                }
            }
            return destajos;
        }
        
        public List<TareaRutaCritica> FlattenTareas(List<TareaRutaCritica> tareas)
        {
            var flat = new List<TareaRutaCritica>();
            void Rec(TareaRutaCritica t)
            {
                flat.Add(t);
                foreach (var h in t.Hijos) Rec(h);
            }
            foreach (var t in tareas) Rec(t);
            return flat;
        }



        private void btnExplosionInsumosPDF_Click(object sender, EventArgs e)
        {
            // Selecciona el modelo current de la casa
            if (casaActual == null)
            {
                MessageBox.Show("Primero busca una casa para generar el reporte de insumos.");
                return;
            }
            var inventarioCasa = inventarioCasas.Find(c => c.Manzana == casaActual.Manzana && c.Lote == casaActual.Lote);
            if (inventarioCasa == null)
            {
                MessageBox.Show("No se encontró el prototipo de la casa en elinventario.");
                return;
            }
            string modelo = inventarioCasa.Prototipo;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF files (*.pdf)|*.pdf";
            sfd.FileName = $"ExplosionInsumos_{modelo}_{DateTime.Now:yyyyMMdd}.pdf";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
            }
        }
        public class Cuadrilla
        {
            public string CodigoCuadrilla { get; set; }
            public string Nombre { get; set; }
            public string Telefono { get; set; }
            public string Rol { get; set; }
            public string FotoPath { get; set; }
        }

        public class DestajoInfo
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public double DuracionDias { get; set; }
            public List<int> Predecesoras { get; set; } = new List<int>();
            public List<string> Recursos { get; set; } = new List<string>();
            public string Estado { get; set; } = "No iniciado";
        }


        public class Casa
        {
            public Dictionary<string, Cuadrilla> CuadrillasPorDestajo { get; set; } = new Dictionary<string, Cuadrilla>();
            public string Manzana { get; set; }
            public string Lote { get; set; }
            public string Prototipo { get; set; }
            public List<string> DestajosTerminadosWBS { get; set; } = new List<string>();
            public string Etapa { get; set; }
            public string MaterialesUsados { get; set; }
            public int DiasRestantes { get; set; }
            public string FotoPath { get; set; }
        }

        public Usuario UsuarioActual { get; set; }


        public class TareaRutaCritica
        {
            public int Id { get; set; }
            public string WBS { get; set; }
            public string Nombre { get; set; }
            public string Duracion { get; set; }
            public bool EsAssignment { get; set; }
            public bool InsumoDisponible { get; set; } // editable por el usuario
            public DateTime Inicio { get; set; }
            public DateTime Fin { get; set; }
            public string NombrePadre { get; set; }
            public bool ListoParaUso { get; set; } = false; // solo para los Assignments
            public string EstadoAssignment { get; set; } = "SIN ENTREGAR"; // 'ENTREGADO' | 'SIN ENTREGAR'

            public string Costo { get; set; }
            public string DuracionISO { get; set; }
            public string CuadrillaAsignada { get; set; } = "";
            public bool CompletadoManual { get; set; } = false;

            public string Work { get; set; }
            public List<TareaRutaCritica> Hijos { get; set; } = new List<TareaRutaCritica>();
            public bool Completado { get; set; }
            public int AvancePorcentaje => Completado ? 100 : 0;
            public List<string> Detalles { get; set; } = new List<string>(); // NUEVO
            public string CostoMoneda
            {
                get
                {
                    if (double.TryParse(Costo.Replace("$", "").Replace(",", ""), out double valor))
                        return valor.ToString("C2"); // "C2" para formato moneda
                    return Costo;
                }
            }
        }



        public class Usuario
        {
            public string Nombre { get; set; }
            public string Clave { get; set; }
            public string Perfil { get; set; }
            public List<string> Permisos { get; set; }
            public bool EsSuperAdmin { get; set; }

            public bool TienePermiso(string permiso)
            {
                return Permisos != null && Permisos.Contains(permiso);
            }
        }

        private bool temaOscuro = false; // inicia en modo claro


        private void OrdenCompraMultiple_Click(object sender, EventArgs e)
        {
            // Crear menú contextual con tres opciones
            ContextMenuStrip menuCompras = new ContextMenuStrip();
            
            // Aplicar tema al menú
            menuCompras.BackColor = ThemeManager.ColorFondo;
            menuCompras.ForeColor = ThemeManager.ColorTextoOscuro;
            menuCompras.Font = new Font("Segoe UI", 9F);
            
            // Opción 1: Orden de Compra Múltiple
            ToolStripMenuItem itemMultiple = new ToolStripMenuItem("Orden de Compra Múltiple (por casas)");
            itemMultiple.Click += (s, ev) => {
                FormCompraMulti f = new FormCompraMulti();
                f.ShowDialog();
            };
            
            // Opción 2: Orden de Compra Indirecta
            ToolStripMenuItem itemIndirecta = new ToolStripMenuItem("Orden de Compra Indirecta");
            itemIndirecta.Click += (s, ev) => {
                FormCompraIndirecta f = new FormCompraIndirecta();
                f.ShowDialog();
            };
            
            // Opción 3: Consultar Órdenes (Repositorio de PDFs) - NUEVO
            ToolStripMenuItem itemConsultar = new ToolStripMenuItem("Consultar Órdenes");
            itemConsultar.Click += (s, ev) => {
                try
                {
                    // Abrir el repositorio de órdenes de compra sin filtro específico
                    using (var formRepo = new FormRepositorioPDFsOrdenesCompra())
                    {
                        formRepo.ShowDialog(this);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al abrir el repositorio de órdenes:\n\n{ex.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            
            // Agregar separador visual
            menuCompras.Items.Add(itemMultiple);
            menuCompras.Items.Add(itemIndirecta);
            menuCompras.Items.Add(new ToolStripSeparator());
            menuCompras.Items.Add(itemConsultar);
            
            // Mostrar el menú en la posición del cursor
            menuCompras.Show(Cursor.Position);
        }

        private void btnRegistrarUsuario_Click(object sender, EventArgs e)
        {
            // FormRegistrarUsuario insertaba con SQL directo en la tabla Usuarios
            // de CALANDRIA, que ya no es la fuente de verdad (los usuarios viven en
            // CalandriaControl, ver SQL_CrearBDMaestraYMigrar.sql) — un usuario
            // creado ahí desaparecía silenciosamente del sistema real. El alta de
            // usuarios ya vive en "Perfiles y Permisos" (crea contra
            // api/perfiles/usuarios/crear, en la BD correcta), así que este botón
            // abre lo mismo.
            AbrirFormPerfilesWeb();
        }

        private void btnGestionPerfiles_Click(object sender, EventArgs e)
        {
            AbrirFormPerfilesWeb();
        }

        private void AbrirFormPerfilesWeb()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is FormPerfilesWeb)
                {
                    form.BringToFront();
                    form.Focus();
                    return;
                }
            }

            // Show(), no ShowDialog(): un WebView2 modal sobre el del panel
            // principal (también WebView2) cuelga EnsureCoreWebView2Async sin
            // lanzar excepción — ver AbrirFormTrabajadoresWeb.
            new FormPerfilesWeb().Show();
        }

        private void AbrirFormFacturacionWeb()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is FormFacturacionWeb)
                {
                    form.BringToFront();
                    form.Focus();
                    return;
                }
            }

            new FormFacturacionWeb().Show();
        }

        private void AbrirFormClientesWeb()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is FormClientesWeb)
                {
                    form.BringToFront();
                    form.Focus();
                    return;
                }
            }

            new FormClientesWeb().Show();
        }

        private void AbrirFormInversionWeb()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is FormInversionWeb)
                {
                    form.BringToFront();
                    form.Focus();
                    return;
                }
            }

            new FormInversionWeb().Show();
        }

        private void btnCambiarTema_Click(object sender, EventArgs e)
        {
            var skinManager = MaterialSkin.MaterialSkinManager.Instance;

            if (temaOscuro)
            {
                // Cambiar a tema claro
                skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
                skinManager.ColorScheme = new ColorScheme(
                    Primary.Blue600, Primary.Blue700,
                    Primary.Blue200, Accent.LightBlue200,
                    TextShade.BLACK
                );
            }
            else
            {
                // Cambiar a tema oscuro
                skinManager.Theme = MaterialSkinManager.Themes.DARK;
                skinManager.ColorScheme = new ColorScheme(
                    Primary.Blue800, Primary.Blue900,
                    Primary.Blue500, Accent.Blue200,
                    TextShade.WHITE
                );
            }

            temaOscuro = !temaOscuro;
        }

        private void btnAbrirCuadrilla_Click(object sender, EventArgs e)
        {
            var form = new FormAsignarCuadrilla("DestajoX"); // puedes pasar el nombre del destajo actual
            form.ShowDialog();
        }

        private void btnRegistrarTrabajador_Click(object sender, EventArgs e)
        {
            AbrirFormRegistrarTrabajador();
        }

        /// <summary>
        /// true = usar FormTrabajadoresWeb (registro, cuadrillas y perfil con
        /// recibos en una sola UI web); false = formularios WinForms clásicos,
        /// sin recompilar. Ver App.config "TrabajadoresWeb".
        /// </summary>
        private static bool TrabajadoresWebActivo
        {
            get
            {
                var v = ConfigurationManager.AppSettings["TrabajadoresWeb"];
                return string.IsNullOrWhiteSpace(v) || v.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void AbrirFormTrabajadoresWeb(string vista)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is FormTrabajadoresWeb)
                {
                    form.BringToFront();
                    form.Focus();
                    return;
                }
            }

            // Show(), no ShowDialog(): un WebView2 modal sobre el del panel
            // principal (también WebView2) aborta la inicialización con
            // COMException E_ABORT — ver reference_webview2_multi_instance.
            new FormTrabajadoresWeb(vista).Show();
        }

        private void AbrirFormRegistrarTrabajador()
        {
            try
            {
                if (TrabajadoresWebActivo) { AbrirFormTrabajadoresWeb("registro"); return; }

                using (var form = new FormRegistrarTrabajador())
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el registro de trabajadores:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormGestionCuadrillas()
        {
            try
            {
                if (TrabajadoresWebActivo) { AbrirFormTrabajadoresWeb("cuadrillas"); return; }

                using (var form = new FormAsignarCuadrilla())
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir la gestión de cuadrillas:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormPerfilTrabajador()
        {
            try
            {
                if (TrabajadoresWebActivo) { AbrirFormTrabajadoresWeb("perfil"); return; }

                using (var form = new FormPerfilTrabajador())
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el perfil de trabajadores:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGestionarCuadrillas_Click(object sender, EventArgs e)
        {
            AbrirFormGestionCuadrillas();
        }


        private void btnGenerarPdfDestajo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Selecciona un destajo desde la nueva interfaz para generar PDF.", 
                "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// true = usar FormComprasWeb (orden múltiple, indirecta y consulta de
        /// órdenes en una sola UI web); false = formularios WinForms clásicos,
        /// sin recompilar. Ver App.config "ComprasWeb".
        /// </summary>
        private static bool ComprasWebActivo
        {
            get
            {
                var v = ConfigurationManager.AppSettings["ComprasWeb"];
                return string.IsNullOrWhiteSpace(v) || v.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void AbrirFormCompraMulti(string manzana = null, string lote = null)
        {
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormCompraMulti || form is FormComprasWeb)
                    {
                        // Si el tablero traía casa, se agrega a la orden abierta.
                        (form as FormComprasWeb)?.PreseleccionarCasa(manzana, lote);
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                if (ComprasWebActivo)
                {
                    // Show(), no ShowDialog(): un WebView2 modal sobre el del panel
                    // principal (también WebView2) aborta la inicialización con
                    // COMException E_ABORT — ver reference_webview2_multi_instance.
                    var web = new FormComprasWeb("multi");
                    web.PreseleccionarCasa(manzana, lote);
                    web.Show();
                }
                else
                {
                    var frm = new FormCompraMulti();
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Orden de Compra Múltiple:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormCompraIndirecta()
        {
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormCompraIndirecta || form is FormComprasWeb)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                if (ComprasWebActivo)
                {
                    new FormComprasWeb("indirecta").Show();
                }
                else
                {
                    var frm = new FormCompraIndirecta();
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Orden de Compra Indirecta:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormRutaCritica()
        {
            try
            {
                MessageBox.Show("FormRutaCritica no está implementado aún.", 
                    "En desarrollo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Ruta Crítica:\n\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormAvanceObra()
        {
            try
            {
                // Permitir múltiples ventanas simultáneamente (modo no modal)
                var frm = new FormAvanceObra();
                frm.Show(); // No modal - permite múltiples ventanas
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Avance por Partidas:\n\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormAvanceConcepto()
        {
            try
            {
                // Permitir múltiples ventanas simultáneamente (modo no modal)
                var frm = new FormAvanceConcepto();
                frm.Show(); // No modal - permite múltiples ventanas
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Avance por Conceptos:\n\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormEstimacionConcepto()
        {
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormEstimacionConceptoMigrado)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                var frm = new FormEstimacionConceptoMigrado();
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Estimación por Concepto:\n\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// true = usar FormEvidenciasWeb (galería por casa en una sola UI web);
        /// false = formulario WinForms clásico, sin recompilar. Ver App.config
        /// "EvidenciasWeb".
        /// </summary>
        private static bool EvidenciasWebActivo
        {
            get
            {
                var v = ConfigurationManager.AppSettings["EvidenciasWeb"];
                return string.IsNullOrWhiteSpace(v) || v.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void AbrirFormEvidencias()
        {
            try
            {
                if (EvidenciasWebActivo)
                {
                    foreach (Form formAbierto in Application.OpenForms)
                    {
                        if (formAbierto is FormEvidenciasWeb)
                        {
                            formAbierto.BringToFront();
                            formAbierto.Focus();
                            return;
                        }
                    }

                    // Show(), no ShowDialog(): un WebView2 modal sobre el del panel
                    // principal (también WebView2) aborta la inicialización con
                    // COMException E_ABORT — ver reference_webview2_multi_instance.
                    new FormEvidenciasWeb().Show();
                    return;
                }

                // Permitir múltiples ventanas simultáneamente
                var frm = new FormEvidenciasFotograficas();
                frm.Show(); // No modal
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Evidencias Fotográficas:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirRepositorioOrdenesCompra()
        {
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormRepositorioPDFsOrdenesCompra || form is FormComprasWeb)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                if (ComprasWebActivo)
                {
                    new FormComprasWeb("consulta").Show();
                }
                else
                {
                    // Abrir el repositorio de órdenes de compra sin filtro específico
                    using (var formRepo = new FormRepositorioPDFsOrdenesCompra())
                    {
                        formRepo.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir repositorio de órdenes:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormHardProgress()
        {
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormHardProgress)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                var frm = new FormHardProgress();
                frm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Hard Progress:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormAvanceMasivo()
        {
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormAvanceMasivoWeb)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                var frm = new FormAvanceMasivoWeb();
                frm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Avance Masivo:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Abre el formulario para editar explosiones de insumos (solo admin)
        /// </summary>
        private void AbrirFormEditarExplosiones()
        {
            try
            {
                // Verificar que sea admin
                if (!Global.EsAdmin)
                {
                    MessageBox.Show(
                        "Esta función solo está disponible para el usuario administrador.",
                        "Acceso Restringido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Si ya está abierto, traer al frente
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormEditarExplosiones)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                // Abrir nuevo formulario en modo modal
                using (var frm = new FormEditarExplosiones())
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Editor de Explosiones:\n\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Abre el formulario de mapeo de coordenadas (solo admin)
        /// </summary>
        private void AbrirFormMapearCoordenadas()
        {
            try
            {
                // Verificar que sea admin
                if (!Global.EsAdmin)
                {
                    MessageBox.Show(
                        "Esta función solo está disponible para el usuario administrador.",
                        "Acceso Restringido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Verificar si ya está abierto
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormMapearCoordenadas)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                // Abrir nuevo formulario
                var frm = new FormMapearCoordenadas();
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir Mapear Coordenadas:\n\n{ex.Message}", 
                    "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error
                );
            }
        }
        
        /// <summary>
        /// Abre el formulario de Editor de TreeList (🌳 Editor de Tareas)
        /// </summary>
        private void AbrirFormEditorTreeList()
        {
            try
            {
                // Verificar si ya está abierto
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormEditorTreeList)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                // Abrir nuevo formulario
                var frm = new FormEditorTreeList(connectionString, "RutaTuneraDestajo");
                frm.Show(this); // Modo no modal
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir Editor de TreeList:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Abre el formulario de activación/desactivación de tareas (⚙️ Activar/Desactivar Tareas)
        /// </summary>
        private void AbrirFormActivarTareasTreeList(string manzana = null, string lote = null)
        {
            try
            {
                // Verificar si ya está abierto
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormActivarTareasTreeList abierto)
                    {
                        // Si el tablero cambió de casa, la ventana la sigue.
                        abierto.PreseleccionarCasa(manzana, lote);
                        abierto.BringToFront();
                        abierto.Focus();
                        return;
                    }
                }

                // No modal: WebView2 dentro de un ShowDialog (loop de mensajes
                // anidado, ventana dueña deshabilitada) es inestable con el
                // panel principal ya hospedando su propio WebView2 — ver
                // reference_webview2_multi_instance. Show() evita ese loop.
                var frm = new FormActivarTareasTreeList();
                frm.PreseleccionarCasa(manzana, lote);
                frm.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir Activar/Desactivar Tareas:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Abre el formulario Administrativos (consolidado financiero por casa)
        /// </summary>
        private void AbrirFormAdministrativos(string manzana = null, string lote = null)
        {
            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormAdministrativos abierto)
                    {
                        // Si el tablero cambió de casa, la consulta la sigue.
                        abierto.PreseleccionarCasa(manzana, lote);
                        abierto.BringToFront();
                        abierto.Focus();
                        return;
                    }
                }

                var frm = new FormAdministrativos();
                frm.PreseleccionarCasa(manzana, lote);
                frm.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir Administrativos:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Abre el visor del registro de errores (tabla LogErrores, central).
        /// </summary>
        private void AbrirFormLogErrores()
        {
            if (!Global.EsAdmin)
            {
                MessageBox.Show(
                    "Esta sección está disponible solo para el administrador.",
                    "Permiso denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormLogErrores)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                var frm = new FormLogErrores();
                frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir el Registro de Errores:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Abre el formulario de diagnóstico de conexión (solo admin)
        /// </summary>
        private void btnDiagnosticoConexion_Click(object sender, EventArgs e)
        {
            try
            {
                // Verificar que sea admin
                if (!Global.EsAdmin)
                {
                    MessageBox.Show(
                        "Esta función solo está disponible para el usuario administrador.",
                        "Acceso Restringido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Verificar si ya está abierto
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormDiagnosticoConexion)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                // Abrir nuevo formulario
                using (var formDiagnostico = new FormDiagnosticoConexion())
                {
                    formDiagnostico.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir Diagnóstico de Conexión:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Abre el formulario de gestión de almacén
        /// </summary>
        private void AbrirFormAlmacen()
        {
            try
            {
                // Verificar si ya está abierto
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormAlmacen)
                    {
                        form.BringToFront();
                        form.Focus();
                        return;
                    }
                }

                // Abrir nuevo formulario
                var frm = new FormAlmacen();
                
                // Si hay una casa seleccionada, pasarla al formulario
                if (casaActual != null)
                {
                    var inventarioCasa = inventarioCasas.FirstOrDefault(c => 
                        c.Manzana == casaActual.Manzana && c.Lote == casaActual.Lote);
                    
                    if (inventarioCasa != null)
                    {
                        frm.SetCasaActual(casaActual, inventarioCasa);
                    }
                }
                
                frm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir Gestión de Almacén:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Limpia el estado de datos ligado a la obra/casa activa (no toca la
        /// sesión de usuario). Se reutiliza al cerrar sesión y al cambiar de
        /// obra, para no duplicar el mismo reset en ambos lugares.
        /// </summary>
        private void LimpiarEstadoObra()
        {
            casaActual = null;
            casas.Clear();
            inventarioCasas.Clear();
            rutasCriticasPorModelo.Clear();

            var lblCasaActual = this.Controls.Find("lblCasaActual", true).FirstOrDefault() as Label;
            var lblManzanaLote = this.Controls.Find("lblManzanaLote", true).FirstOrDefault() as Label;
            var lblPrototipo = this.Controls.Find("lblPrototipo", true).FirstOrDefault() as Label;
            var pictureBoxCasa = this.Controls.Find("pictureBoxCasa", true).FirstOrDefault() as PictureBox;

            if (lblCasaActual != null)
                lblCasaActual.Text = "Sin casa seleccionada";
            if (lblManzanaLote != null)
                lblManzanaLote.Text = "Mz: - | Lote: -";
            if (lblPrototipo != null)
                lblPrototipo.Text = "Prototipo: -";
            if (pictureBoxCasa != null)
                pictureBoxCasa.Image = null;
            if (lblFotoEmpty != null)
                lblFotoEmpty.Visible = true;

            // Reset del dashboard moderno
            ResetearDashboardModerno();

            // 🗺️ Volver a vista completa del mapa
            MostrarVistaCompletaSembrado();
            var lblInstruccionesMapa = this.Controls.Find("lblInstruccionesMapa", true).FirstOrDefault() as Label;
            if (lblInstruccionesMapa != null)
            {
                lblInstruccionesMapa.Text = "💡 Busca una casa para ver su ubicación en el mapa";
            }
        }

        /// <summary>
        /// Reabre la pantalla de selección de obra sin cerrar sesión: el token
        /// de usuario sigue siendo válido, solo cambia la obra activa (header
        /// X-Obra-Id) con la que el API resuelve las siguientes consultas.
        /// </summary>
        private void AbrirSelectorObra()
        {
            // FormSeleccionObraWeb hospeda un WebView2 propio: no se abre con
            // ShowDialog() mientras este formulario (que también hospeda
            // WebView2) sigue vivo, aunque esté oculto (ver
            // reference_webview2_multi_instance).
            this.Hide();
            var seleccion = new FormSeleccionObraWeb();
            seleccion.ObraSeleccionada += (s2, e2) => seleccion.Close();
            seleccion.FormClosed += (s2, e2) =>
            {
                if (seleccion.SeSeleccionoObra)
                {
                    LimpiarEstadoObra();
                    CargarInventarioAlInicio();
                    // El mapa y el tema del panel web son por obra; sin esto se
                    // quedan mostrando la obra anterior hasta recargar la app.
                    EnviarDatosAlPanel();
                    _ = EnviarTemaAlPanel();
                }
                this.Show();
                ActualizarInfoUsuario();
            };
            seleccion.Show();
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            // Limpiar referencias a datos de usuario y casa actual
            Global.UsuarioActual = null;
            Global.ObraActualId = null;
            Global.ObraActualNombre = null;
            ApiClient.CerrarSesion(); // evita que el token del usuario anterior siga activo
            LimpiarEstadoObra();
            ActualizarInfoUsuario();

            // FormLogin hospeda un WebView2 propio: no se abre con ShowDialog()
            // mientras este formulario (que también hospeda WebView2) sigue vivo,
            // aunque esté oculto (ver reference_webview2_multi_instance).
            this.Hide();
            bool loginExitoso = false;
            var login = new FormLogin();
            login.LoginExitoso += (s2, e2) =>
            {
                loginExitoso = true;
                login.Close();
            };
            login.FormClosed += (s2, e2) =>
            {
                if (loginExitoso)
                {
                    // Multi-obra: tras un relogueo hay que volver a elegir obra,
                    // igual que en el arranque de la app (Program.cs).
                    var seleccion = new FormSeleccionObraWeb();
                    seleccion.ObraSeleccionada += (s3, e3) => seleccion.Close();
                    seleccion.FormClosed += (s3, e3) =>
                    {
                        if (seleccion.SeSeleccionoObra)
                        {
                            this.Show();
                            MostrarPermisosEnLabel();
                            // El panel web (WebView2) sigue vivo desde el primer login —
                            // sin esto se queda mostrando nombre/perfil/permisos del
                            // usuario anterior hasta recargar la app.
                            EnviarDatosAlPanel();
                            _ = EnviarTemaAlPanel();
                            CargarInventarioAlInicio();
                        }
                        else
                        {
                            this.Close();
                        }
                    };
                    seleccion.Show();
                }
                else
                {
                    this.Close();
                }

                // Libera recursos del usuario anterior.
                GC.Collect();
                GC.WaitForPendingFinalizers();
            };
            login.Show();
        }

        private void btnBuscarCasa_Click(object sender, EventArgs e)
        {
            var txtManzana = this.Controls.Find("txtManzana", true).FirstOrDefault() as TextBox;
            var txtLote = this.Controls.Find("txtLote", true).FirstOrDefault() as TextBox;
            
            string manzana = txtManzana?.Text.Trim() ?? "";
            string lote = txtLote?.Text.Trim() ?? "";

            if (string.IsNullOrEmpty(manzana) || string.IsNullOrEmpty(lote))
            {
                MessageBox.Show("Por favor ingresa manzana y lote.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Consultar SQL para obtener la casa
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Manzana, Lote, Prototipo, FotoPath FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            casaActual = new Casa
                            {
                                Manzana = reader["Manzana"].ToString(),
                                Lote = reader["Lote"].ToString(),
                                Prototipo = reader["Prototipo"].ToString(),
                                FotoPath = reader["FotoPath"] != DBNull.Value ? reader["FotoPath"].ToString() : null
                            };
                        }
                        else
                        {
                            MessageBox.Show($"No se encontró la casa M{manzana}-L{lote} en la base de datos.", 
                                "Casa no encontrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                }
            }

            // Actualizar UI con información de la casa
            var lblCasaActual = this.Controls.Find("lblCasaActual", true).FirstOrDefault() as Label;
            var lblManzanaLote = this.Controls.Find("lblManzanaLote", true).FirstOrDefault() as Label;
            var lblPrototipo = this.Controls.Find("lblPrototipo", true).FirstOrDefault() as Label;
            var pictureBoxCasa = this.Controls.Find("pictureBoxCasa", true).FirstOrDefault() as PictureBox;
            
            if (lblCasaActual != null)
            {
                lblCasaActual.Text = $"Casa M{casaActual.Manzana} · L{casaActual.Lote}";
                lblCasaActual.ForeColor = Color.FromArgb(48, 42, 36);
            }

            if (lblManzanaLote != null)
                lblManzanaLote.Text = $"Mz: {casaActual.Manzana}  ·  Lote: {casaActual.Lote}";
            if (lblPrototipo != null)
                lblPrototipo.Text = $"Prototipo: {casaActual.Prototipo}";
            
            // 🎯 Cargar última foto desde FormEvidenciasFotograficas
            if (pictureBoxCasa != null)
            {
                try
                {
                    var gestorEvidencias = new GestorEvidencias(connectionString);
                    var ultimaEvidencia = gestorEvidencias.ObtenerUltimaEvidencia(casaActual.Manzana, casaActual.Lote);
                    
                    if (ultimaEvidencia != null && ultimaEvidencia.FotoBytes != null && ultimaEvidencia.FotoBytes.Length > 0)
                    {
                        using (var ms = new MemoryStream(ultimaEvidencia.FotoBytes))
                        {
                            if (pictureBoxCasa.Image != null)
                            {
                                pictureBoxCasa.Image.Dispose();
                            }

                            pictureBoxCasa.Image = new Bitmap(Image.FromStream(ms));
                        }
                        if (lblFotoEmpty != null) lblFotoEmpty.Visible = false;
                    }
                    else
                    {
                        if (pictureBoxCasa.Image != null)
                        {
                            pictureBoxCasa.Image.Dispose();
                        }
                        pictureBoxCasa.Image = null;
                        if (lblFotoEmpty != null) lblFotoEmpty.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    if (pictureBoxCasa.Image != null)
                    {
                        pictureBoxCasa.Image.Dispose();
                    }
                    pictureBoxCasa.Image = null;
                    if (lblFotoEmpty != null) lblFotoEmpty.Visible = true;
                    
                    #if DEBUG
                    MessageBox.Show($"No se pudo cargar la última evidencia fotográfica:\n{ex.Message}", 
                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    #endif
                }
            }

            // 🗺️ HACER ZOOM A LA CASA EN EL MAPA
            HacerZoomACasa(casaActual.Manzana, casaActual.Lote);
            
            // Actualizar texto de instrucciones
            var lblInstruccionesMapa = this.Controls.Find("lblInstruccionesMapa", true).FirstOrDefault() as Label;
            if (lblInstruccionesMapa != null)
            {
                lblInstruccionesMapa.Text = $"📍 M{casaActual.Manzana} L{casaActual.Lote} - Arrastra para mover | Rueda del mouse para zoom";
            }

            // Copiar lista de destajos terminados desde el inventario cargado (si existe)
            var inv = inventarioCasas.FirstOrDefault(c => c.Manzana == casaActual.Manzana && c.Lote == casaActual.Lote);
            if (inv != null && inv.DestajosTerminadosWBS != null)
            {
                casaActual.DestajosTerminadosWBS = new List<string>(inv.DestajosTerminadosWBS);
            }

            // Cargar ruta crítica y asignar cuadrillas aquí...
            if (rutasCriticasPorModelo.TryGetValue("UNICA", out var ruta))
            {
                var listaLlana = FlattenTareas(ruta);

                foreach (var tarea in listaLlana)
                {
                    string claveDestajo = tarea.NombrePadre ?? tarea.Nombre;
                    if (casaActual.CuadrillasPorDestajo.TryGetValue(claveDestajo, out var cuadrilla))
                        tarea.CuadrillaAsignada = cuadrilla?.CodigoCuadrilla ?? "";
                }
            }
            
            // 🔥 Cargar resumen de progreso de la casa
            CargarResumenProgreso();
        }
        
        /// <summary>
        /// Carga el resumen de progreso de la casa actual.
        /// Ahora delega a CargarProgresoDesdeActivacion() que replica la lógica
        /// de FormActivarTareasTreeList (destajos activados/finalizados + monto activo).
        /// </summary>
        private void CargarResumenProgreso()
        {
            // ✨ Nuevo origen: ActivacionTareasRuta + Ruta{Calandra|Tunera}Destajo
            // Replica el cálculo de FormActivarTareasTreeList.ActualizarEstadisticas
            CargarProgresoDesdeActivacion();
        }

        /// <summary>
        /// Maneja el evento Resize del pictureBoxMapa
        /// </summary>
        private void PictureBoxMapa_Resize(object sender, EventArgs e)
        {
            if (imagenMapaCompleto == null) return;
            
            // Si no hay casa seleccionada, mostrar vista completa ajustada al nuevo tamaño
            if (casaActual == null)
            {
                MostrarVistaCompletaSembrado();
            }
            else
            {
                // Si hay casa seleccionada, recalcular zoom manteniendo la casa centrada
                HacerZoomACasa(casaActual.Manzana, casaActual.Lote);
            }
        }
        
        /// <summary>
        /// 🖱️ Inicia el arrastre del mapa con el mouse (pan)
        /// </summary>
        private void PictureBoxMapa_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                estaDraggeando = true;
                puntoInicialDrag = e.Location;
                offsetInicialDrag = offsetActual;
                pictureBoxMapa.Cursor = Cursors.Hand;
            }
        }
        
        /// <summary>
        /// 🖱️ Mueve el mapa mientras se arrastra con el mouse (pan)
        /// </summary>
        private void PictureBoxMapa_MouseMove(object sender, MouseEventArgs e)
        {
            if (estaDraggeando)
            {
                // Calcular desplazamiento desde el punto inicial
                int deltaX = e.X - puntoInicialDrag.X;
                int deltaY = e.Y - puntoInicialDrag.Y;

                offsetActual = new PointF(
                    offsetInicialDrag.X + deltaX,
                    offsetInicialDrag.Y + deltaY
                );
                offsetObjetivo = offsetActual;

                // 🔥 Throttling: limita re-renders durante drag para mejor performance
                if (DebeRenderizarMouseMove())
                    pictureBoxMapa.Invalidate();
            }
        }
        
        /// <summary>
        /// 🖱️ Finaliza el arrastre del mapa (pan)
        /// </summary>
        private void PictureBoxMapa_MouseUp(object sender, MouseEventArgs e)
        {
            if (estaDraggeando)
            {
                estaDraggeando = false;
                pictureBoxMapa.Cursor = Cursors.Default;
                
                // Actualizar texto de instrucciones si hay casa seleccionada
                if (casaActual != null)
                {
                    var lblInstruccionesMapa = this.Controls.Find("lblInstruccionesMapa", true).FirstOrDefault() as Label;
                    if (lblInstruccionesMapa != null)
                    {
                        lblInstruccionesMapa.Text = $"📍 M{casaActual.Manzana} L{casaActual.Lote} - Arrastra para mover | Rueda del mouse para zoom";
                    }
                }
            }
        }
        
        /// <summary>
        /// 🔍 Maneja el zoom con la rueda del mouse
        /// </summary>
        private void PictureBoxMapa_MouseWheel(object sender, MouseEventArgs e)
        {
            if (imagenMapaCompleto == null) return;
            
            // Calcular nuevo nivel de zoom
            // Cada tick de la rueda del mouse cambia el zoom en 10%
            float factorZoom = e.Delta > 0 ? 1.1f : 0.9f;
            float nuevoZoom = zoomActual * factorZoom;
            
            // Limitar zoom entre 0.5x y 5.0x
            const float zoomMin = 0.5f;
            const float zoomMax = 5.0f;
            nuevoZoom = Math.Max(zoomMin, Math.Min(zoomMax, nuevoZoom));
            
            // Obtener la posición del mouse en coordenadas del mapa (antes del zoom)
            // Esto permite hacer zoom hacia donde apunta el cursor
            PointF puntoMouse = new PointF(e.X, e.Y);
            
            // Convertir punto del mouse a coordenadas del mapa
            float mapX = (puntoMouse.X - offsetActual.X) / zoomActual;
            float mapY = (puntoMouse.Y - offsetActual.Y) / zoomActual;
            
            // Calcular nuevo offset para mantener el punto del mouse en la misma posición
            // (zoom centrado en el cursor)
            offsetObjetivo = new PointF(
                puntoMouse.X - mapX * nuevoZoom,
                puntoMouse.Y - mapY * nuevoZoom
            );
            
            zoomObjetivo = nuevoZoom;
            
            // Actualizar valores inmediatamente para respuesta instantánea
            zoomActual = nuevoZoom;
            offsetActual = offsetObjetivo;
            
            // Actualizar cache si el cambio de zoom es significativo
            if (Math.Abs(nuevoZoom - zoomActual) > 0.1f)
            {
                needsRedraw = true;
            }
            
            // Redibujar
            pictureBoxMapa.Invalidate();
            
            // Actualizar texto de instrucciones si hay casa seleccionada
            if (casaActual != null)
            {
                var lblInstruccionesMapa = this.Controls.Find("lblInstruccionesMapa", true).FirstOrDefault() as Label;
                if (lblInstruccionesMapa != null)
                {
                    lblInstruccionesMapa.Text = $"📍 M{casaActual.Manzana} · L{casaActual.Lote} — Rueda = zoom · Arrastra = mover";
                }
            }
            ActualizarZoomLabel(nuevoZoom);
        }
    }
}