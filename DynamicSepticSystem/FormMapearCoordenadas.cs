using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DynamicSepticSystem
{
    public partial class FormMapearCoordenadas : Form
    {
        // Clase para almacenar coordenadas de una casa
        public class CoordenadasCasa
        {
            public string Manzana { get; set; }
            public string Lote { get; set; }
            public float X { get; set; }
            public float Y { get; set; }

            public string Key => $"{Manzana}_{Lote}";
        }

        // Diccionario de coordenadas guardadas
        private Dictionary<string, CoordenadasCasa> coordenadasGuardadas = new Dictionary<string, CoordenadasCasa>();
        
        // Imagen del mapa cargada
        private Image imagenMapa;
        
        // Coordenada temporal seleccionada con click
        private PointF? coordenadaSeleccionada = null;
        
        // Ruta del archivo de configuraci�n
        private readonly string archivoConfig = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "coordenadas_mapa.json"
        );

        public FormMapearCoordenadas()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);

            // Configurar el formulario como draggable desde el panel superior
            panelTop.MouseDown += PanelTop_MouseDown;
            panelTop.MouseMove += PanelTop_MouseMove;
            panelTop.MouseUp += PanelTop_MouseUp;
        }

        #region Drag & Drop del Form

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void PanelTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void PanelTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void PanelTop_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        #endregion

        private void FormMapearCoordenadas_Load(object sender, EventArgs e)
        {
            // Verificar permisos de admin
            if (!Global.EsAdmin)
            {
                MessageBox.Show(
                    "Esta funci�n solo est� disponible para el usuario administrador.",
                    "Acceso Restringido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                this.Close();
                return;
            }

            // Cargar imagen del mapa
            CargarImagenMapa();
            
            // Cargar coordenadas guardadas (si existen)
            CargarCoordenadasDesdeArchivo();
            
            // Actualizar UI
            ActualizarListViewCoordenadas();
            ActualizarStatusBar();
        }

        /// <summary>
        /// Carga la imagen del mapa del sembrado
        /// </summary>
        private void CargarImagenMapa()
        {
            try
            {
                string mapaPath = Path.Combine(Application.StartupPath, "Resources", "MapaSembrado.png");
                
                if (File.Exists(mapaPath))
                {
                    imagenMapa = Image.FromFile(mapaPath);
                    pictureBoxMapa.Image = imagenMapa;
                }
                else
                {
                    // Si no existe el mapa, crear uno de ejemplo
                    imagenMapa = CrearMapaEjemplo();
                    pictureBoxMapa.Image = imagenMapa;
                    
                    MessageBox.Show(
                        "No se encontr� MapaSembrado.png en Resources/\n\n" +
                        "Se mostrar� un mapa de ejemplo.\n\n" +
                        "Para usar tu plano real, col�calo en:\n" + mapaPath,
                        "Mapa No Encontrado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar el mapa:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                
                // Crear mapa de ejemplo como fallback
                imagenMapa = CrearMapaEjemplo();
                pictureBoxMapa.Image = imagenMapa;
            }
        }

        /// <summary>
        /// Crea un mapa de ejemplo (igual que en PanelPrincipal)
        /// </summary>
        private Image CrearMapaEjemplo()
        {
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
                
                for (int mz = 1; mz <= manzanas; mz++)
                {
                    int x = offsetX + ((mz - 1) % 3) * (anchoManzana + espacioX);
                    int y = offsetY + ((mz - 1) / 3) * (altoManzana + espacioY);
                    
                    // Dibujar rect�ngulo de manzana
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
                            
                            // Dibujar n�mero de lote
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
                        g.DrawString(textoManzana, font, textBrush, x + 5, y - 25);
                    }
                }
                
                // T�tulo del plano
                using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(88, 53, 23)))
                {
                    g.DrawString("CALANDRIA RESIDENCIAL - PLANO DE SEMBRADO", font, textBrush, 150, 20);
                }
            }
            
            return bmp;
        }

        /// <summary>
        /// Evento MouseMove en el mapa - Muestra coordenadas en tiempo real
        /// </summary>
        private void pictureBoxMapa_MouseMove(object sender, MouseEventArgs e)
        {
            if (imagenMapa == null) return;
            
            // Convertir coordenadas del control a coordenadas de la imagen
            PointF coordImagen = ConvertirPictureBoxAImagenCoords(e.Location);
            
            // Actualizar status bar
            toolStripStatusLabel.Text = $"Posici�n: X={coordImagen.X:F1}, Y={coordImagen.Y:F1} | {coordenadasGuardadas.Count} coordenadas guardadas";
        }

        /// <summary>
        /// Evento MouseClick en el mapa - Selecciona una coordenada
        /// </summary>
        private void pictureBoxMapa_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (imagenMapa == null) return;
            
            // Convertir coordenadas del control a coordenadas de la imagen
            coordenadaSeleccionada = ConvertirPictureBoxAImagenCoords(e.Location);
            
            // Actualizar label
            lblCoordenadaActual.Text = $"?? Seleccionada: X={coordenadaSeleccionada.Value.X:F1}, Y={coordenadaSeleccionada.Value.Y:F1}";
            lblCoordenadaActual.ForeColor = Color.FromArgb(46, 204, 113);
            
            // Habilitar bot�n de asignar si hay manzana y lote
            ValidarYHabilitarAsignar();
            
            // Redibujar para mostrar marcador temporal
            pictureBoxMapa.Invalidate();
        }

        /// <summary>
        /// Convierte coordenadas del PictureBox a coordenadas reales de la imagen
        /// </summary>
        private PointF ConvertirPictureBoxAImagenCoords(Point ptControl)
        {
            if (imagenMapa == null || pictureBoxMapa.Image == null)
                return new PointF(0, 0);
            
            // Calcular el ratio de zoom del PictureBox (modo Zoom)
            float ratioX = (float)imagenMapa.Width / pictureBoxMapa.ClientSize.Width;
            float ratioY = (float)imagenMapa.Height / pictureBoxMapa.ClientSize.Height;
            float ratio = Math.Max(ratioX, ratioY);
            
            // Calcular el tama�o escalado de la imagen
            int scaledWidth = (int)(imagenMapa.Width / ratio);
            int scaledHeight = (int)(imagenMapa.Height / ratio);
            
            // Calcular offset (centrado)
            int offsetX = (pictureBoxMapa.ClientSize.Width - scaledWidth) / 2;
            int offsetY = (pictureBoxMapa.ClientSize.Height - scaledHeight) / 2;
            
            // Convertir coordenadas
            float imgX = (ptControl.X - offsetX) * ratio;
            float imgY = (ptControl.Y - offsetY) * ratio;
            
            // Asegurar que est�n dentro de los l�mites de la imagen
            imgX = Math.Max(0, Math.Min(imagenMapa.Width, imgX));
            imgY = Math.Max(0, Math.Min(imagenMapa.Height, imgY));
            
            return new PointF(imgX, imgY);
        }

        /// <summary>
        /// Dibuja marcadores en el mapa
        /// </summary>
        private void pictureBoxMapa_Paint(object sender, PaintEventArgs e)
        {
            if (imagenMapa == null) return;
            
            // Dibujar coordenadas guardadas
            foreach (var coord in coordenadasGuardadas.Values)
            {
                DibujarMarcadorEnMapa(e.Graphics, new PointF(coord.X, coord.Y), coord.Key, Color.FromArgb(46, 204, 113), false);
            }
            
            // Dibujar coordenada temporal seleccionada
            if (coordenadaSeleccionada.HasValue)
            {
                string label = !string.IsNullOrEmpty(txtManzana.Text) && !string.IsNullOrEmpty(txtLote.Text)
                    ? $"M{txtManzana.Text}L{txtLote.Text}"
                    : "?";
                DibujarMarcadorEnMapa(e.Graphics, coordenadaSeleccionada.Value, label, Color.FromArgb(241, 196, 15), true);
            }
        }

        /// <summary>
        /// Dibuja un marcador en el mapa
        /// </summary>
        private void DibujarMarcadorEnMapa(Graphics g, PointF coordImagen, string label, Color color, bool esTemporal)
        {
            // Convertir coordenadas de imagen a coordenadas del control
            PointF coordControl = ConvertirImagenAPictureBoxCoords(coordImagen);
            
            // Si est� fuera del �rea visible, no dibujar
            if (coordControl.X < 0 || coordControl.Y < 0 || 
                coordControl.X > pictureBoxMapa.ClientSize.Width || 
                coordControl.Y > pictureBoxMapa.ClientSize.Height)
                return;
            
            float size = esTemporal ? 20 : 15;
            
            // Dibujar c�rculo
            using (Brush brush = new SolidBrush(Color.FromArgb(200, color)))
            using (Pen borderPen = new Pen(Color.White, 2))
            {
                g.FillEllipse(brush, coordControl.X - size/2, coordControl.Y - size/2, size, size);
                g.DrawEllipse(borderPen, coordControl.X - size/2, coordControl.Y - size/2, size, size);
            }
            
            // Dibujar etiqueta
            using (Font font = new Font("Segoe UI", esTemporal ? 9 : 7, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(color))
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
            {
                SizeF textSize = g.MeasureString(label, font);
                RectangleF bgRect = new RectangleF(
                    coordControl.X - textSize.Width / 2 - 3,
                    coordControl.Y - size - textSize.Height - 3,
                    textSize.Width + 6,
                    textSize.Height + 2
                );
                
                g.FillRectangle(bgBrush, bgRect);
                g.DrawString(label, font, textBrush,
                    coordControl.X - textSize.Width / 2,
                    coordControl.Y - size - textSize.Height - 2);
            }
        }

        /// <summary>
        /// Convierte coordenadas de imagen a coordenadas del PictureBox
        /// </summary>
        private PointF ConvertirImagenAPictureBoxCoords(PointF ptImagen)
        {
            if (imagenMapa == null) return new PointF(0, 0);
            
            float ratioX = (float)imagenMapa.Width / pictureBoxMapa.ClientSize.Width;
            float ratioY = (float)imagenMapa.Height / pictureBoxMapa.ClientSize.Height;
            float ratio = Math.Max(ratioX, ratioY);
            
            int scaledWidth = (int)(imagenMapa.Width / ratio);
            int scaledHeight = (int)(imagenMapa.Height / ratio);
            
            int offsetX = (pictureBoxMapa.ClientSize.Width - scaledWidth) / 2;
            int offsetY = (pictureBoxMapa.ClientSize.Height - scaledHeight) / 2;
            
            float ctrlX = (ptImagen.X / ratio) + offsetX;
            float ctrlY = (ptImagen.Y / ratio) + offsetY;
            
            return new PointF(ctrlX, ctrlY);
        }

        /// <summary>
        /// Valida y habilita el bot�n de asignar coordenada
        /// </summary>
        private void ValidarYHabilitarAsignar()
        {
            bool hayManzana = !string.IsNullOrWhiteSpace(txtManzana.Text);
            bool hayLote = !string.IsNullOrWhiteSpace(txtLote.Text);
            bool hayCoordenada = coordenadaSeleccionada.HasValue;
            
            btnAsignarCoordenada.Enabled = hayManzana && hayLote && hayCoordenada;
        }

        /// <summary>
        /// Evento TextChanged de los textboxes de manzana y lote
        /// </summary>
        private void txtCasaInfo_TextChanged(object sender, EventArgs e)
        {
            ValidarYHabilitarAsignar();
        }

        /// <summary>
        /// Asigna la coordenada seleccionada a la casa actual
        /// </summary>
        private void btnAsignarCoordenada_Click(object sender, EventArgs e)
        {
            if (!coordenadaSeleccionada.HasValue) return;
            
            string manzana = txtManzana.Text.Trim();
            string lote = txtLote.Text.Trim();
            
            if (string.IsNullOrEmpty(manzana) || string.IsNullOrEmpty(lote))
            {
                MessageBox.Show("Ingresa manzana y lote antes de asignar.", "Atenci�n",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var coord = new CoordenadasCasa
            {
                Manzana = manzana,
                Lote = lote,
                X = coordenadaSeleccionada.Value.X,
                Y = coordenadaSeleccionada.Value.Y
            };
            
            // Si ya existe, preguntar si desea reemplazar
            if (coordenadasGuardadas.ContainsKey(coord.Key))
            {
                var result = MessageBox.Show(
                    $"La casa M{manzana}L{lote} ya tiene coordenadas asignadas.\n\n" +
                    $"Actual: X={coordenadasGuardadas[coord.Key].X:F1}, Y={coordenadasGuardadas[coord.Key].Y:F1}\n" +
                    $"Nueva: X={coord.X:F1}, Y={coord.Y:F1}\n\n" +
                    "�Deseas reemplazarla?",
                    "Confirmar Reemplazo",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                
                if (result != DialogResult.Yes) return;
            }
            
            // Guardar coordenada
            coordenadasGuardadas[coord.Key] = coord;
            
            // Limpiar selecci�n temporal
            coordenadaSeleccionada = null;
            lblCoordenadaActual.Text = "Haz clic en el mapa para seleccionar";
            lblCoordenadaActual.ForeColor = Color.FromArgb(127, 140, 141);
            btnAsignarCoordenada.Enabled = false;
            
            // Incrementar lote autom�ticamente
            if (int.TryParse(lote, out int loteNum))
            {
                txtLote.Text = (loteNum + 1).ToString();
            }
            
            // Actualizar UI
            ActualizarListViewCoordenadas();
            ActualizarStatusBar();
            pictureBoxMapa.Invalidate();
            
            // Mensaje de �xito
            toolStripStatusLabel.Text = $"? M{manzana}L{lote} asignada | {coordenadasGuardadas.Count} coordenadas";
        }

        /// <summary>
        /// Actualiza el ListView con las coordenadas guardadas
        /// </summary>
        private void ActualizarListViewCoordenadas()
        {
            listViewCoordenadas.Items.Clear();
            
            foreach (var coord in coordenadasGuardadas.Values.OrderBy(c => int.TryParse(c.Manzana, out int m) ? m : 0)
                                                              .ThenBy(c => int.TryParse(c.Lote, out int l) ? l : 0))
            {
                var item = new ListViewItem(coord.Manzana);
                item.SubItems.Add(coord.Lote);
                item.SubItems.Add(coord.X.ToString("F1"));
                item.SubItems.Add(coord.Y.ToString("F1"));
                item.Tag = coord;
                
                listViewCoordenadas.Items.Add(item);
            }
        }

        /// <summary>
        /// Actualiza la barra de estado
        /// </summary>
        private void ActualizarStatusBar()
        {
            toolStripStatusLabel.Text = $"Listo | {coordenadasGuardadas.Count} coordenadas guardadas";
        }

        /// <summary>
        /// Evento de selecci�n en el ListView
        /// </summary>
        private void listViewCoordenadas_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEliminarSeleccionado.Enabled = listViewCoordenadas.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Elimina la coordenada seleccionada
        /// </summary>
        private void btnEliminarSeleccionado_Click(object sender, EventArgs e)
        {
            if (listViewCoordenadas.SelectedItems.Count == 0) return;
            
            var coord = listViewCoordenadas.SelectedItems[0].Tag as CoordenadasCasa;
            if (coord == null) return;
            
            var result = MessageBox.Show(
                $"�Eliminar coordenadas de M{coord.Manzana}L{coord.Lote}?",
                "Confirmar Eliminaci�n",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            
            if (result == DialogResult.Yes)
            {
                coordenadasGuardadas.Remove(coord.Key);
                ActualizarListViewCoordenadas();
                ActualizarStatusBar();
                pictureBoxMapa.Invalidate();
                
                toolStripStatusLabel.Text = $"? M{coord.Manzana}L{coord.Lote} eliminada | {coordenadasGuardadas.Count} coordenadas";
            }
        }

        /// <summary>
        /// Guarda la configuraci�n en archivo JSON
        /// </summary>
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Convertir a lista para serializaci�n
                var lista = coordenadasGuardadas.Values.ToList();
                
                // Serializar a JSON con formato indentado
                string json = JsonConvert.SerializeObject(lista, Formatting.Indented);
                
                // Guardar en archivo
                File.WriteAllText(archivoConfig, json);
                
                MessageBox.Show(
                    $"Configuraci�n guardada exitosamente.\n\n" +
                    $"Archivo: {archivoConfig}\n" +
                    $"Coordenadas: {coordenadasGuardadas.Count}",
                    "Guardado Exitoso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                
                toolStripStatusLabel.Text = $"? Configuraci�n guardada | {coordenadasGuardadas.Count} coordenadas";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar la configuraci�n:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Carga coordenadas desde archivo JSON
        /// </summary>
        private void CargarCoordenadasDesdeArchivo()
        {
            if (!File.Exists(archivoConfig)) return;
            
            try
            {
                string json = File.ReadAllText(archivoConfig);
                var lista = JsonConvert.DeserializeObject<List<CoordenadasCasa>>(json);
                
                if (lista != null)
                {
                    coordenadasGuardadas.Clear();
                    foreach (var coord in lista)
                    {
                        coordenadasGuardadas[coord.Key] = coord;
                    }
                    
                    toolStripStatusLabel.Text = $"? {coordenadasGuardadas.Count} coordenadas cargadas desde archivo";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar coordenadas:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        /// <summary>
        /// Limpia todas las coordenadas
        /// </summary>
        private void btnLimpiarTodo_Click(object sender, EventArgs e)
        {
            if (coordenadasGuardadas.Count == 0)
            {
                MessageBox.Show("No hay coordenadas para limpiar.", "Atenci�n",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var result = MessageBox.Show(
                $"�Eliminar TODAS las {coordenadasGuardadas.Count} coordenadas guardadas?\n\n" +
                "Esta acci�n NO se puede deshacer.",
                "Confirmar Limpieza Total",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            
            if (result == DialogResult.Yes)
            {
                coordenadasGuardadas.Clear();
                coordenadaSeleccionada = null;
                
                ActualizarListViewCoordenadas();
                ActualizarStatusBar();
                pictureBoxMapa.Invalidate();
                
                lblCoordenadaActual.Text = "Haz clic en el mapa para seleccionar";
                lblCoordenadaActual.ForeColor = Color.FromArgb(127, 140, 141);
                
                toolStripStatusLabel.Text = "? Todas las coordenadas eliminadas";
            }
        }

        /// <summary>
        /// Exporta coordenadas a archivo JSON externo
        /// </summary>
        private void btnExportarJSON_Click(object sender, EventArgs e)
        {
            if (coordenadasGuardadas.Count == 0)
            {
                MessageBox.Show("No hay coordenadas para exportar.", "Atenci�n",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivos JSON (*.json)|*.json|Todos los archivos (*.*)|*.*",
                FileName = $"coordenadas_mapa_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                Title = "Exportar Coordenadas"
            };
            
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var lista = coordenadasGuardadas.Values.ToList();
                    string json = JsonConvert.SerializeObject(lista, Formatting.Indented);
                    File.WriteAllText(sfd.FileName, json);
                    
                    MessageBox.Show(
                        $"Coordenadas exportadas exitosamente.\n\n" +
                        $"Archivo: {sfd.FileName}\n" +
                        $"Coordenadas: {coordenadasGuardadas.Count}",
                        "Exportaci�n Exitosa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    
                    toolStripStatusLabel.Text = $"? Exportado: {Path.GetFileName(sfd.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al exportar:\n\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        /// <summary>
        /// Importa coordenadas desde archivo JSON externo
        /// </summary>
        private void btnImportarJSON_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Archivos JSON (*.json)|*.json|Todos los archivos (*.*)|*.*",
                Title = "Importar Coordenadas"
            };
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string json = File.ReadAllText(ofd.FileName);
                    var lista = JsonConvert.DeserializeObject<List<CoordenadasCasa>>(json);
                    
                    if (lista == null || lista.Count == 0)
                    {
                        MessageBox.Show("El archivo no contiene coordenadas v�lidas.", "Atenci�n",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    // Preguntar si desea reemplazar o combinar
                    var result = MessageBox.Show(
                        $"Se encontraron {lista.Count} coordenadas en el archivo.\n\n" +
                        $"Coordenadas actuales: {coordenadasGuardadas.Count}\n\n" +
                        "�Deseas REEMPLAZAR las coordenadas actuales?\n\n" +
                        "S� = Reemplazar todo\n" +
                        "NO = Combinar (mantener actuales + agregar nuevas)",
                        "Modo de Importaci�n",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question
                    );
                    
                    if (result == DialogResult.Cancel) return;
                    
                    if (result == DialogResult.Yes)
                    {
                        // Reemplazar todo
                        coordenadasGuardadas.Clear();
                    }
                    
                    // Agregar/actualizar coordenadas
                    int agregadas = 0;
                    int actualizadas = 0;
                    
                    foreach (var coord in lista)
                    {
                        if (coordenadasGuardadas.ContainsKey(coord.Key))
                            actualizadas++;
                        else
                            agregadas++;
                        
                        coordenadasGuardadas[coord.Key] = coord;
                    }
                    
                    ActualizarListViewCoordenadas();
                    ActualizarStatusBar();
                    pictureBoxMapa.Invalidate();
                    
                    MessageBox.Show(
                        $"Importaci�n completada.\n\n" +
                        $"Agregadas: {agregadas}\n" +
                        $"Actualizadas: {actualizadas}\n" +
                        $"Total: {coordenadasGuardadas.Count}",
                        "Importaci�n Exitosa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    
                    toolStripStatusLabel.Text = $"? Importado: +{agregadas} nuevas, ~{actualizadas} actualizadas";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al importar:\n\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        /// <summary>
        /// Cierra el formulario
        /// </summary>
        private void btnCerrar_Click(object sender, EventArgs e)
        {
            // Verificar si hay cambios sin guardar
            if (coordenadasGuardadas.Count > 0)
            {
                var result = MessageBox.Show(
                    "�Deseas guardar los cambios antes de salir?",
                    "Confirmar Salida",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question
                );
                
                if (result == DialogResult.Cancel) return;
                
                if (result == DialogResult.Yes)
                {
                    btnGuardar_Click(sender, e);
                }
            }
            
            this.Close();
        }
    }
}
