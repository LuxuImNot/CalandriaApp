using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase parcial que contiene la configuración del TreeListView y sus columnas
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        // Referencia a la columna WBS para poder mostrar/ocultar
        private OLVColumn colWBS;
        
        /// <summary>
        /// Verifica si el usuario actual es administrador
        /// </summary>
        private bool EsUsuarioAdmin()
        {
            return Global.UsuarioActual != null && 
                   Global.UsuarioActual.Nombre != null &&
                   Global.UsuarioActual.Nombre.Equals("admin", StringComparison.OrdinalIgnoreCase);
        }

        private void ConfigurarTreeListView()
        {
            olvEstimacionConceptos.CanExpandGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                return nodo.EsConcepto && nodo.Partidas.Count > 0;
            };
            
            olvEstimacionConceptos.ChildrenGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                return nodo.Partidas;
            };

            olvEstimacionConceptos.FullRowSelect = true;
            olvEstimacionConceptos.UseAlternatingBackColors = true;
            olvEstimacionConceptos.AlternateRowBackColor = Color.FromArgb(240, 248, 255);
            
            olvEstimacionConceptos.CheckBoxes = true;
            olvEstimacionConceptos.TriStateCheckBoxes = false;
            olvEstimacionConceptos.CheckedAspectName = "Incluir";
            
            olvEstimacionConceptos.ShowSortIndicators = false;
            olvEstimacionConceptos.Sorting = System.Windows.Forms.SortOrder.None;
            
            olvEstimacionConceptos.BooleanCheckStateGetter = delegate(object rowObject) {
                return ((NodoConcepto)rowObject).Incluir;
            };
            
            olvEstimacionConceptos.BooleanCheckStatePutter = delegate(object rowObject, bool newValue) {
                var nodo = (NodoConcepto)rowObject;
                if (nodo.Completado && newValue)
                    return false;
                
                // Si es una partida dinámica y se está marcando, solicitar m²
                if (!nodo.EsConcepto && nodo.EsDinamica && newValue && nodo.MetrosCuadrados == 0)
                {
                    if (!MostrarDialogoM2EnCheck(nodo))
                    {
                        return false;
                    }
                }
                    
                nodo.Incluir = newValue;
                
                // Propagar cambios a hijos o padre
                if (nodo.EsConcepto)
                {
                    foreach (var partida in nodo.Partidas)
                    {
                        if (!partida.Completado)
                        {
                            partida.Incluir = newValue;
                        }
                    }
                }
                else
                {
                    var padre = EncontrarPadre(nodo);
                    if (padre != null)
                    {
                        bool todasMarcadas = padre.Partidas.All(p => p.Incluir || p.Completado);
                        bool ningunaMarcada = padre.Partidas.All(p => !p.Incluir || p.Completado);
                        
                        if (todasMarcadas)
                            padre.Incluir = true;
                        else if (ningunaMarcada)
                            padre.Incluir = false;
                    }
                }
                
                // Reconstruir árbol
                this.BeginInvoke(new Action(() => {
                    try
                    {
                        var expandidos = new HashSet<string>();
                        foreach (var item in olvEstimacionConceptos.Objects)
                        {
                            var conceptoRaiz = item as NodoConcepto;
                            if (conceptoRaiz != null && conceptoRaiz.EsConcepto)
                            {
                                if (olvEstimacionConceptos.IsExpanded(conceptoRaiz))
                                {
                                    expandidos.Add(conceptoRaiz.Codigo);
                                }
                            }
                        }
                        
                        olvEstimacionConceptos.RebuildAll(true);
                        
                        foreach (var item in olvEstimacionConceptos.Objects)
                        {
                            var conceptoRaiz = item as NodoConcepto;
                            if (conceptoRaiz != null && conceptoRaiz.EsConcepto)
                            {
                                if (expandidos.Contains(conceptoRaiz.Codigo))
                                {
                                    olvEstimacionConceptos.Expand(conceptoRaiz);
                                }
                            }
                        }
                        
                        OnDatosChanged(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en refresh: {ex.Message}");
                    }
                }));
                
                return newValue;
            };

            // Configurar columnas
            ConfigurarColumnasTreeListView();

            // Eventos de formato
            ConfigurarEventosFormato();
            
            olvEstimacionConceptos.BeforeSorting += (s, e) => e.Canceled = true;
            
            // Configurar evento KeyDown para guardar al presionar Enter
            olvEstimacionConceptos.KeyDown += OlvEstimacionConceptos_KeyDown;
        }

        /// <summary>
        /// Configura las columnas del TreeListView
        /// </summary>
        private void ConfigurarColumnasTreeListView()
        {
            var colNombre = new OLVColumn("Concepto / Partida", "Nombre") 
            { 
                Width = 250,
                IsEditable = false, 
                Sortable = false,
                FillsFreeSpace = true
            };

            // COLUMNA: WBS (solo visible para admin)
            colWBS = new OLVColumn("WBS", "WBS")
            {
                Width = 50,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                // ?? SOLO VISIBLE PARA ADMIN
                IsVisible = EsUsuarioAdmin()
            };

            colWBS.AspectGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                if (!nodo.EsConcepto)
                {
                    return nodo.WBS > 0 ? nodo.WBS.ToString() : "";
                }
                return "";
            };

            // COLUMNA: m² (solo para partidas dinámicas)
            var colM2 = new OLVColumn("m²", "MetrosCuadrados")
            {
                Width = 70,
                IsEditable = true,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false
            };

            colM2.AspectGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                if (!nodo.EsConcepto && nodo.EsDinamica)
                {
                    return nodo.MetrosCuadrados > 0 ? nodo.MetrosCuadrados.ToString("F2") : "";
                }
                return "";
            };

            colM2.AspectPutter = delegate(object rowObject, object newValue) {
                var nodo = (NodoConcepto)rowObject;
                
                if (!nodo.EsConcepto && nodo.EsDinamica && !nodo.Completado)
                {
                    if (newValue != null && double.TryParse(newValue.ToString(), out double m2))
                    {
                        if (m2 < 0)
                        {
                            MessageBox.Show("Los metros cuadrados no pueden ser negativos", "Validación", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Validar límite de m²
                        double limite = nodo.ObtenerLimiteEfectivo(prototipoActual);
                        if (limite > 0 && m2 > limite)
                        {
                            MessageBox.Show(
                                $"? No se puede exceder el límite de m²\n\n" +
                                $"Valor ingresado: {m2:F2} m²\n" +
                                $"Límite máximo: {limite:F2} m²\n\n" +
                                $"El valor se ajustará al límite máximo permitido.",
                                "Límite de m² excedido", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            m2 = limite;
                        }

                        nodo.MetrosCuadrados = m2;
                        
                        double valorM2 = ObtenerValorM2(nodo);
                        nodo.Total = nodo.MetrosCuadrados * valorM2;
                        
                        System.Diagnostics.Debug.WriteLine(
                            $"m² editados: WBS={nodo.WBS}, m²={m2:F2}, " +
                            $"Valor/m²={valorM2:C2}, Total={nodo.Total:C2}" +
                            (limite > 0 ? $", Límite={limite:F2}" : ""));
                        
                        var padre = EncontrarPadre(nodo);
                        if (padre != null)
                        {
                            RecalcularAvanceConcepto(padre);
                        }
                        
                        olvEstimacionConceptos.RefreshObject(nodo);
                        if (padre != null)
                        {
                            olvEstimacionConceptos.RefreshObject(padre);
                        }
                        
                        ActualizarTotales();
                        OnDatosChanged(this, EventArgs.Empty);
                    }
                }
            };

            // COLUMNA: Avance % (basado en m² / límite m²)
            var colAvance = new OLVColumn("Avance", "AvanceDinamico")
            {
                Width = 80,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false
            };

            colAvance.AspectGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                
                if (nodo.EsConcepto)
                {
                    // Para conceptos, mostrar el avance promedio de las partidas dinámicas con límite
                    var partidasConAvance = nodo.Partidas
                        .Where(p => p.EsDinamica && p.ObtenerLimiteEfectivo(prototipoActual) > 0)
                        .ToList();
                    
                    if (partidasConAvance.Count == 0)
                        return "";
                    
                    double sumaAvances = 0;
                    int count = 0;
                    foreach (var p in partidasConAvance)
                    {
                        double av = p.CalcularAvancePorLimite(prototipoActual);
                        if (av >= 0)
                        {
                            sumaAvances += av;
                            count++;
                        }
                    }
                    
                    if (count == 0)
                        return "";
                    
                    double promedioAvance = sumaAvances / count;
                    return $"{promedioAvance:F1}%";
                }
                else if (nodo.EsDinamica)
                {
                    double limite = nodo.ObtenerLimiteEfectivo(prototipoActual);
                    
                    if (limite <= 0)
                        return "?"; // Sin límite
                    
                    double avance = nodo.CalcularAvancePorLimite(prototipoActual);
                    
                    if (avance < 0)
                        return "-";
                    
                    return $"{avance:F1}%";
                }
                
                return "";
            };

            // COLUMNA: Límite m² (solo para partidas dinámicas)
            var colLimite = new OLVColumn("Límite m²", "LimiteM2")
            {
                Width = 80,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false
            };

            colLimite.AspectGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                if (!nodo.EsConcepto && nodo.EsDinamica)
                {
                    double limite = nodo.ObtenerLimiteEfectivo(prototipoActual);
                    return limite > 0 ? limite.ToString("F2") : "?";
                }
                return "";
            };

            // COLUMNA: Total Presupuestado
            var colTotalPresupuestado = new OLVColumn("Total Presupuestado", "Total")
            {
                Width = 130,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}",
                Sortable = false
            };

            colTotalPresupuestado.AspectGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                if (nodo.EsConcepto)
                {
                    return nodo.Partidas.Sum(p => ObtenerTotalPartida(p));
                }
                else
                {
                    return ObtenerTotalPartida(nodo);
                }
            };

            // COLUMNA: Monto
            var colMonto = new OLVColumn("Monto", "MontoEjecutado") 
            { 
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right, 
                AspectToStringFormat = "{0:C2}", 
                Sortable = false
            };
            
            colMonto.AspectGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                if (nodo.EsConcepto)
                {
                    var partidasMarcadas = nodo.Partidas.Where(p => p.Incluir).ToList();
                    if (partidasMarcadas.Any())
                    {
                        return partidasMarcadas.Sum(p => ObtenerTotalPartida(p));
                    }
                    return 0.0;
                }
                else
                {
                    if (nodo.Incluir)
                    {
                        return ObtenerTotalPartida(nodo);
                    }
                    else if (nodo.Completado)
                        return nodo.MontoEjecutado;
                    else
                        return 0.0;
                }
            };

            var colEstado = new OLVColumn("Estado", "Completado")
            {
                Width = 100,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false
            };
            
            colEstado.AspectGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                
                if (nodo.EsConcepto)
                {
                    var partidasCompletadas = nodo.Partidas.Where(p => p.Completado).Count();
                    var totalPartidas = nodo.Partidas.Count;
                    
                    if (partidasCompletadas == 0)
                        return "";
                    else if (partidasCompletadas == totalPartidas)
                        return "TERMINADO";
                    else
                        return "ACTIVADO";
                }
                else
                {
                    return nodo.Completado ? "TERMINADO" : "";
                }
            };

            var colFechaFin = new OLVColumn("Fecha Fin", "FechaFinalizacion")
            {
                Width = 100,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                Sortable = false,
                AspectToStringFormat = "{0:dd/MM/yyyy}"
            };
            
            colFechaFin.AspectGetter = delegate(object x) {
                var nodo = (NodoConcepto)x;
                return nodo.FechaFinalizacion.HasValue ? nodo.FechaFinalizacion.Value.ToString("dd/MM/yyyy") : "";
            };

            // Agregar columnas (WBS solo visible para admin)
            olvEstimacionConceptos.AllColumns.AddRange(new[] { 
                colNombre, colWBS, colM2, colLimite, colAvance, 
                colTotalPresupuestado, colMonto, colEstado, colFechaFin 
            });
            olvEstimacionConceptos.RebuildColumns();
        }

        /// <summary>
        /// Obtiene el valor por m² según el prototipo actual
        /// </summary>
        private double ObtenerValorM2(NodoConcepto nodo)
        {
            if (string.IsNullOrEmpty(prototipoActual))
                return nodo.ValorM2Tunera; // Default a Tunera
            
            return prototipoActual.ToUpper().Contains("TUNERA") 
                ? nodo.ValorM2Tunera 
                : nodo.ValorM2Calandra;
        }

        /// <summary>
        /// Obtiene el total de una partida considerando si es dinámica
        /// </summary>
        private double ObtenerTotalPartida(NodoConcepto partida)
        {
            if (partida.EsDinamica && partida.MetrosCuadrados > 0)
            {
                double valorM2 = ObtenerValorM2(partida);
                return partida.MetrosCuadrados * valorM2;
            }
            return partida.Total;
        }

        /// <summary>
        /// Configura los eventos de formato del TreeListView
        /// </summary>
        private void ConfigurarEventosFormato()
        {
            olvEstimacionConceptos.FormatRow += (s, e) =>
            {
                var nodo = e.Model as NodoConcepto;
                if (nodo != null)
                {
                    if (nodo.Completado)
                    {
                        e.Item.ForeColor = Color.Gray;
                    }
                    else if (nodo.EsConcepto)
                    {
                        var partidasCompletadas = nodo.Partidas.Where(p => p.Completado).Count();
                        if (partidasCompletadas > 0 && partidasCompletadas < nodo.Partidas.Count)
                        {
                            e.Item.BackColor = Color.FromArgb(255, 250, 220);
                        }
                    }
                    else if (!nodo.EsConcepto && nodo.EsDinamica)
                    {
                        if (!nodo.Completado)
                        {
                            // Colorear según el avance
                            double avance = nodo.CalcularAvancePorLimite(prototipoActual);
                            if (avance >= 100)
                            {
                                e.Item.BackColor = Color.FromArgb(220, 255, 220); // Verde claro - 100%
                            }
                            else if (avance >= 50)
                            {
                                e.Item.BackColor = Color.FromArgb(255, 255, 200); // Amarillo claro - 50-99%
                            }
                            else if (avance > 0)
                            {
                                e.Item.BackColor = Color.FromArgb(255, 235, 200); // Naranja claro - 1-49%
                            }
                            else
                            {
                                e.Item.BackColor = Color.FromArgb(232, 245, 255); // Azul claro - sin avance
                            }
                        }
                    }
                }
            };

            // Formatear celda de Avance con colores
            olvEstimacionConceptos.FormatCell += (s, e) =>
            {
                if (e.Column.Text == "Avance")
                {
                    var nodo = e.Model as NodoConcepto;
                    if (nodo != null && !nodo.EsConcepto && nodo.EsDinamica)
                    {
                        double avance = nodo.CalcularAvancePorLimite(prototipoActual);
                        if (avance >= 100)
                        {
                            e.SubItem.ForeColor = Color.FromArgb(39, 174, 96); // Verde
                            e.SubItem.Font = new Font(e.SubItem.Font, FontStyle.Bold);
                        }
                        else if (avance >= 50)
                        {
                            e.SubItem.ForeColor = Color.FromArgb(243, 156, 18); // Naranja
                        }
                        else if (avance > 0)
                        {
                            e.SubItem.ForeColor = Color.FromArgb(231, 76, 60); // Rojo
                        }
                    }
                }
            };

            olvEstimacionConceptos.CellToolTipGetter = delegate(OLVColumn column, object modelObject) {
                var nodo = modelObject as NodoConcepto;
                if (nodo != null)
                {
                    if (nodo.EsConcepto)
                    {
                        double totalPresupuestado = nodo.Partidas.Sum(p => ObtenerTotalPartida(p));
                        double totalEjecutado = nodo.Partidas.Sum(p => p.MontoEjecutado);
                        double porcentaje = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;
                        
                        int partidasCompletadas = nodo.Partidas.Count(p => p.Completado);
                        int totalPartidas = nodo.Partidas.Count;
                        int partidasDinamicas = nodo.Partidas.Count(p => p.EsDinamica);
                        
                        string tooltip = $"Código: {nodo.Codigo}\n" +
                               $"Total Presupuestado: {totalPresupuestado:C2}\n" +
                               $"Total Ejecutado: {totalEjecutado:C2}\n" +
                               $"Avance: {porcentaje:F1}%\n" +
                               $"Partidas: {partidasCompletadas}/{totalPartidas} completadas";
                        
                        if (partidasDinamicas > 0)
                        {
                            tooltip += $"\nPartidas dinámicas: {partidasDinamicas}";
                        }
                        
                        return tooltip;
                    }
                    else
                    {
                        string tooltip = $"WBS: {nodo.WBS}\n";
                        
                        if (nodo.EsDinamica)
                        {
                            double valorM2 = ObtenerValorM2(nodo);
                            double limite = nodo.ObtenerLimiteEfectivo(prototipoActual);
                            
                            tooltip += $"PARTIDA DINÁMICA\n\n" +
                                      $"Valor/m²: {valorM2:C2}\n";
                            

                            if (limite > 0)
                            {
                                tooltip += $"Límite m²: {limite:F2}\n";
                            }
                            else
                            {
                                tooltip += $"Límite m²: Sin límite\n";
                            }
                            

                            if (nodo.MetrosCuadrados > 0)
                            {
                                tooltip += $"\nMetros cuadrados: {nodo.MetrosCuadrados:F2} m²\n" +
                                          $"Costo calculado: {(nodo.MetrosCuadrados * valorM2):C2}";

                                if (limite > 0)
                                {
                                    double avance = nodo.CalcularAvancePorLimite(prototipoActual);
                                    tooltip += $"\n\nAvance: {avance:F1}%";
                                    if (avance >= 100)
                                        tooltip += " ? COMPLETO";
                                }
                            }
                            else
                            {
                                tooltip += $"\nMarque para ingresar m²";
                            }
                            
                            tooltip += $"\n\nPresione Enter después de editar\npara guardar automáticamente";
                        }
                        else
                        {
                            tooltip += $"Costo: {nodo.Total:C2}";
                        }
                        
                        return tooltip;
                    }
                }
                return null;
            };
        }

        /// <summary>
        /// Maneja el evento KeyDown para guardar al presionar Enter
        /// </summary>
        private void OlvEstimacionConceptos_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                
                // Obtener el objeto seleccionado
                var nodo = olvEstimacionConceptos.SelectedObject as NodoConcepto;
                // ? CAMBIADO: Ahora guarda incluso con 0 m² (>= 0 en lugar de > 0)
                if (nodo != null && !nodo.EsConcepto && nodo.EsDinamica && nodo.MetrosCuadrados >= 0)
                {
                    GuardarAvancePartidaDinamica(nodo);
                }
            }
        }

        /// <summary>
        /// Guarda el avance de una partida dinámica en la base de datos
        /// </summary>
        private void GuardarAvancePartidaDinamica(NodoConcepto partida)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                System.Diagnostics.Debug.WriteLine("No se puede guardar: Manzana o Lote no seleccionados");
                return;
            }
            
            try
            {
                // Validar límite de m² antes de guardar
                double limite = partida.ObtenerLimiteEfectivo(prototipoActual);
                if (limite > 0 && partida.MetrosCuadrados > limite)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"?? Ajustando m² de {partida.MetrosCuadrados:F2} a límite {limite:F2}");
                    partida.MetrosCuadrados = limite;
                }
                
                // Calcular el monto ejecutado
                double valorM2 = ObtenerValorM2(partida);
                double montoCalculado = partida.MetrosCuadrados * valorM2;
                
                // Actualizar el monto ejecutado en el nodo
                partida.MontoEjecutado = montoCalculado;
                partida.Total = montoCalculado;
                
                // Calcular el avance por límite
                double avance = partida.CalcularAvancePorLimite(prototipoActual);
                if (avance >= 0)
                {
                    partida.AvancePorcentaje = avance;
                }
                
                // Guardar en la BD
                GuardarAvancePartidaEnBD(partida);
                
                // Actualizar el padre
                var padre = EncontrarPadre(partida);
                if (padre != null)
                {
                    RecalcularAvanceConcepto(padre);
                    olvEstimacionConceptos.RefreshObject(padre);
                }
                
                // Actualizar la UI
                olvEstimacionConceptos.RefreshObject(partida);
                ActualizarTotales();
                
                // Mostrar confirmación visual sutil
                System.Diagnostics.Debug.WriteLine(
                    $"Guardado: WBS={partida.WBS}, m²={partida.MetrosCuadrados:F2}, " +
                    $"Monto={montoCalculado:C2}, Avance={avance:F1}%");
                
                // Mostrar mensaje breve
                MostrarMensajeGuardado(partida);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar avance: {ex.Message}");
                MessageBox.Show($"Error al guardar avance: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Muestra un mensaje temporal de confirmación de guardado
        /// </summary>
        private void MostrarMensajeGuardado(NodoConcepto partida)
        {
            try
            {
                string mensaje;
                
                // ? CAMBIADO: Mensaje diferente para 0 m²
                if (partida.MetrosCuadrados == 0)
                {
                    mensaje = "? Guardado: 0 m² (reseteado)";
                }
                else
                {
                    double avance = partida.CalcularAvancePorLimite(prototipoActual);
                    mensaje = avance >= 0 
                        ? $"? Guardado: {partida.MetrosCuadrados:F2} m² ({avance:F1}%)"
                        : $"? Guardado: {partida.MetrosCuadrados:F2} m²";
                }
                
                // Crear un tooltip temporal
                var tooltipGuardado = new ToolTip
                {
                    IsBalloon = true,
                    ToolTipIcon = ToolTipIcon.Info,
                    ToolTipTitle = "Avance Guardado"
                };
                
                // Mostrar tooltip cerca del control
                tooltipGuardado.Show(mensaje, olvEstimacionConceptos, 
                    olvEstimacionConceptos.Width / 2, 20, 2000);
                
                // Limpiar después de mostrar
                var timer = new System.Windows.Forms.Timer { Interval = 2500 };
                timer.Tick += (s, ev) =>
                {
                    tooltipGuardado.Dispose();
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
            catch
            {
                // Ignorar errores en la visualización del mensaje
            }
        }

        /// <summary>
        /// Muestra el diálogo de m² cuando se marca el checkbox de una partida dinámica
        /// </summary>
        private bool MostrarDialogoM2EnCheck(NodoConcepto nodo)
        {
            double limite = nodo.ObtenerLimiteEfectivo(prototipoActual);
            double valorM2 = ObtenerValorM2(nodo);
            
            using (var formM2 = new Form
            {
                Text = "Partida Dinámica - Ingresar m²",
                Size = new Size(450, 280),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblTitulo = new Label
                {
                    Text = "Esta es una partida con costo dinámico",
                    Location = new Point(20, 20),
                    AutoSize = false,
                    Width = 400,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(41, 128, 185)
                };

                var lblPartida = new Label
                {
                    Text = $"Partida: {nodo.Nombre}",
                    Location = new Point(20, 50),
                    AutoSize = false,
                    Width = 400,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular)
                };

                var lblInfo = new Label
                {
                    Text = $"Valor/m²: {valorM2:C2}" + (limite > 0 ? $"  |  Límite: {limite:F2} m²" : ""),
                    Location = new Point(20, 75),
                    AutoSize = false,
                    Width = 400,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(39, 174, 96)
                };

                var lblM2 = new Label
                {
                    Text = "Ingrese los metros cuadrados (m²):",
                    Location = new Point(20, 110),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };

                var numM2 = new NumericUpDown
                {
                    Location = new Point(20, 135),
                    Width = 200,
                    DecimalPlaces = 2,
                    Minimum = 0.01M,
                    Maximum = limite > 0 ? (decimal)limite : 99999M,
                    Value = 1M,
                    Font = new Font("Segoe UI", 10F)
                };

                var lblCostoCalculado = new Label
                {
                    Location = new Point(240, 135),
                    AutoSize = false,
                    Width = 180,
                    Height = 30,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(230, 126, 34),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Label para mostrar el avance
                var lblAvance = new Label
                {
                    Location = new Point(20, 165),
                    AutoSize = false,
                    Width = 400,
                    Height = 25,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(41, 128, 185),
                    Visible = limite > 0
                };

                EventHandler actualizarCosto = (s, ev) =>
                {
                    double costoCalculado = (double)numM2.Value * valorM2;
                    lblCostoCalculado.Text = $"= {costoCalculado:C2}";
                    
                    if (limite > 0)
                    {
                        double avanceCalc = ((double)numM2.Value / limite) * 100;
                        avanceCalc = Math.Min(avanceCalc, 100);
                        lblAvance.Text = $"Avance: {avanceCalc:F1}%";
                        
                        if (avanceCalc >= 100)
                        {
                            lblAvance.ForeColor = Color.FromArgb(39, 174, 96);
                            lblAvance.Text += " ? COMPLETO";
                        }
                        else if (avanceCalc >= 50)
                        {
                            lblAvance.ForeColor = Color.FromArgb(243, 156, 18);
                        }
                        else
                        {
                            lblAvance.ForeColor = Color.FromArgb(231, 76, 60);
                        }
                    }
                };
                numM2.ValueChanged += actualizarCosto;
                actualizarCosto(null, EventArgs.Empty);

                var btnOk = new Button
                {
                    Text = "Aceptar",
                    Location = new Point(150, 200),
                    Width = 100,
                    Height = 35,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "Cancelar",
                    Location = new Point(260, 200),
                    Width = 100,
                    Height = 35,
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(231, 76, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                formM2.Controls.AddRange(new Control[] {
                    lblTitulo, lblPartida, lblInfo, lblM2, numM2, lblCostoCalculado, lblAvance, btnOk, btnCancel
                });

                formM2.AcceptButton = btnOk;
                formM2.CancelButton = btnCancel;

                if (formM2.ShowDialog() == DialogResult.OK)
                {
                    nodo.MetrosCuadrados = (double)numM2.Value;
                    nodo.Total = nodo.MetrosCuadrados * valorM2;

                    System.Diagnostics.Debug.WriteLine(
                        $"Partida dinámica: {nodo.Nombre}\n" +
                        $"   m²: {nodo.MetrosCuadrados:F2}\n" +
                        $"   Valor/m²: {valorM2:C2}\n" +
                        $"   Total: {nodo.Total:C2}");
                    
                    // Guardar automáticamente al aceptar el diálogo
                    GuardarAvancePartidaDinamica(nodo);
                    
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
