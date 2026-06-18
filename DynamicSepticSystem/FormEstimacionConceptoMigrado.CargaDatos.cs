using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase parcial que contiene la carga de datos (manzanas, lotes, conceptos, partidas)
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        private void CargarManzanas()
        {
            try
            {
                cmbManzana.Items.Clear();
                var manzanas = ApiClient.Get<List<string>>("/api/avances/manzanas");
                if (manzanas != null)
                    foreach (var m in manzanas)
                        cmbManzana.Items.Add(m);
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
                cmbLote.Items.Clear();
                var lotes = ApiClient.Get<List<string>>(
                    $"/api/avances/lotes?manzana={Uri.EscapeDataString(cmbManzana.SelectedItem.ToString())}");
                if (lotes != null)
                    foreach (var l in lotes)
                        cmbLote.Items.Add(l);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar lotes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ActualizarLabelFolio();
        }

        private void CargarLotes(string manzana)
        {
            try
            {
                cmbLote.Items.Clear();
                cmbLote.Text = "";

                if (string.IsNullOrEmpty(manzana))
                    return;

                var lotes = ApiClient.Get<List<string>>(
                    $"/api/avances/lotes?manzana={Uri.EscapeDataString(manzana)}");
                if (lotes != null)
                    foreach (var l in lotes)
                        cmbLote.Items.Add(l);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar lotes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbLote_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarLabelFolio();
        }

        private void CargarEstimacionJerarquica(string manzana, string lote)
        {
            nodosRaiz = new List<NodoConcepto>();
            
            LimpiarEvidenciasSeleccionadas();
            avancesMetrosCuadrados.Clear();

            try
            {
                var resp = ApiClient.Get<EstimacionJerarquicaApi>(
                    $"/api/avances/estimacion-jerarquica?manzana={Uri.EscapeDataString(manzana ?? "")}" +
                    $"&lote={Uri.EscapeDataString(lote ?? "")}" +
                    $"&prototipo={Uri.EscapeDataString(prototipoActual ?? "")}");
                {
                    var avancesPartidas = new Dictionary<int, Tuple<double, double, DateTime?>>();
                    if (resp?.Avances != null)
                        foreach (var a in resp.Avances)
                        {
                            avancesPartidas[a.Wbs] = Tuple.Create(a.AvancePorcentaje, a.MontoEjecutado, a.FechaFinalizacion);
                            if (a.MetrosCuadrados > 0 && !avancesMetrosCuadrados.ContainsKey(a.Wbs))
                                avancesMetrosCuadrados[a.Wbs] = a.MetrosCuadrados;
                        }

                    var todasLasPartidas = new List<PartidaDinamica>();
                    if (resp?.Partidas != null)
                        foreach (var p in resp.Partidas)
                            todasLasPartidas.Add(new PartidaDinamica
                            {
                                WBS = p.Wbs,
                                Codigo = p.Codigo ?? "",
                                Padre = p.Padre ?? "",
                                Etapa = p.Etapa ?? "",
                                Partida = p.Partida ?? "",
                                Costo = p.Costo,
                                EsDinamica = p.EsDinamica,
                                ValorM2Tunera = p.ValorM2Tunera,
                                ValorM2Calandra = p.ValorM2Calandra,
                                LimiteM2 = p.LimiteM2,
                                LimiteM2Tunera = p.LimiteM2Tunera,
                                LimiteM2Calandra = p.LimiteM2Calandra,
                                PrototiposAplicables = p.PrototiposAplicables,
                                MetrosCuadrados = 0
                            });
                    
                    // Filtrar partidas por prototipo actual si está definido
                    if (!string.IsNullOrEmpty(prototipoActual))
                    {
                        int totalAntes = todasLasPartidas.Count;
                        todasLasPartidas = todasLasPartidas
                            .Where(p => p.AplicaAPrototipo(prototipoActual))
                            .ToList();
                        int totalDespues = todasLasPartidas.Count;
                        
                        if (totalAntes != totalDespues)
                        {
                            System.Diagnostics.Debug.WriteLine($"🎯 Filtrado por prototipo '{prototipoActual}': {totalAntes} -> {totalDespues} partidas");
                        }
                    }

                    // Agrupar partidas por Codigo (concepto)
                    var partidasPorCodigo = todasLasPartidas
                        .GroupBy(p => p.Codigo)
                        .ToDictionary(g => g.Key, g => g.OrderBy(p => p.WBS).ToList());

                    System.Diagnostics.Debug.WriteLine($"📋 Conceptos únicos encontrados: {partidasPorCodigo.Count}");
                    foreach (var kvp in partidasPorCodigo)
                    {
                        System.Diagnostics.Debug.WriteLine($"   Código '{kvp.Key}': {kvp.Value.Count} partidas");
                    }

                    // Crear nodos de concepto a partir de los datos de la BD
                    var conceptosTemporal = new SortedDictionary<int, NodoConcepto>();

                    foreach (var grupo in partidasPorCodigo)
                    {
                        string codigoStr = grupo.Key;
                        var partidasDelCodigo = grupo.Value;

                        if (string.IsNullOrEmpty(codigoStr) || partidasDelCodigo.Count == 0)
                            continue;

                        // Obtener el nombre del concepto (usar el Padre de la primera partida o generar uno)
                        string nombreConcepto = ObtenerNombreConcepto(codigoStr, partidasDelCodigo);

                        var nodoConcepto = new NodoConcepto
                        {
                            EsConcepto = true,
                            Codigo = codigoStr,
                            Nombre = nombreConcepto,
                            Total = 0,
                            Incluir = false,
                            Completado = false
                        };

                        // Crear nodos de partidas
                        List<NodoConcepto> nodosPartidas = new List<NodoConcepto>();
                        foreach (var partida in partidasDelCodigo)
                        {
                            var nodoPartida = CrearNodoPartidaDinamica(partida, avancesPartidas);
                            nodosPartidas.Add(nodoPartida);
                        }

                        nodoConcepto.Partidas = nodosPartidas;
                        RecalcularAvanceConcepto(nodoConcepto);
                        
                        System.Diagnostics.Debug.WriteLine($"✅ Concepto '{nodoConcepto.Nombre}' (Código: {codigoStr}): {nodosPartidas.Count} partidas");

                        // Cargar fecha de finalización del concepto si existe
                        if (!string.IsNullOrEmpty(codigoStr) && int.TryParse(codigoStr, out int codigoInt))
                        {
                            int codigoConcepto = -codigoInt;
                            if (avancesPartidas.TryGetValue(codigoConcepto, out var avanceConcepto))
                            {
                                if (avanceConcepto.Item3.HasValue)
                                {
                                    nodoConcepto.FechaFinalizacion = avanceConcepto.Item3;
                                }
                            }
                            
                            if (!conceptosTemporal.ContainsKey(codigoInt))
                            {
                                conceptosTemporal[codigoInt] = nodoConcepto;
                            }
                        }
                        else
                        {
                            // Si el código no es numérico, usar un valor alto
                            int codigoDefault = 900 + conceptosTemporal.Count;
                            conceptosTemporal[codigoDefault] = nodoConcepto;
                        }
                    }

                    nodosRaiz = conceptosTemporal.Values.ToList();
                    System.Diagnostics.Debug.WriteLine($"📊 Total conceptos finales: {nodosRaiz.Count}");
                    
                    // Log de resumen
                    int totalPartidas = nodosRaiz.Sum(c => c.Partidas.Count);
                    System.Diagnostics.Debug.WriteLine($"📊 Total partidas en TreeListView: {totalPartidas}");
                }

                if (nodosRaiz != null && nodosRaiz.Count > 0)
                {
                    olvEstimacionConceptos.Roots = nodosRaiz;
                    olvEstimacionConceptos.CollapseAll();
                    olvEstimacionConceptos.BuildList(true);
                }
                else
                {
                    olvEstimacionConceptos.ClearObjects();
                    System.Diagnostics.Debug.WriteLine("⚠️ No se encontraron conceptos para mostrar");
                }

                ActualizarTotales();
                ActualizarPreviewPDF();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en CargarEstimacionJerarquica: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error al cargar estimación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Obtiene el nombre del concepto basándose en el código y las partidas
        /// </summary>
        private string ObtenerNombreConcepto(string codigo, List<PartidaDinamica> partidas)
        {
            // Diccionario de nombres de conceptos por código
            var nombresConceptos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "1", "Preliminares" },
                { "2", "Cimentación" },
                { "3", "Estructura" },
                { "4", "Ins. Hidráulica, Sanitaria y Gas LP" },
                { "5", "Inst. Eléctrica" },
                { "6", "Albañilería" },
                { "7", "Acabados" },
                { "8", "Herrería, Aluminio y Vidrio" },
                { "9", "Carpintería y Cerrajería" },
                { "10", "Muebles y Accesorios" },
                { "11", "Inst especiales y Obra Exterior" },
                { "12", "Urbanización" }
            };

            // Intentar obtener nombre del diccionario
            if (nombresConceptos.TryGetValue(codigo, out string nombrePredefinido))
            {
                return nombrePredefinido;
            }

            // Si no está en el diccionario, usar el Padre de la primera partida
            if (partidas.Count > 0 && !string.IsNullOrEmpty(partidas[0].Padre))
            {
                return partidas[0].Padre;
            }

            // Fallback: usar el código como nombre
            return $"Concepto {codigo}";
        }

        private NodoConcepto CrearNodoPartidaDinamica(PartidaDinamica partida, 
            Dictionary<int, Tuple<double, double, DateTime?>> avances)
        {
            // Crear nombre descriptivo para la partida
            string nombrePartida;
            if (!string.IsNullOrEmpty(partida.Etapa) && !string.IsNullOrEmpty(partida.Partida))
            {
                nombrePartida = $"{partida.Etapa} > {partida.Partida}";
            }
            else if (!string.IsNullOrEmpty(partida.Partida))
            {
                nombrePartida = partida.Partida;
            }
            else if (!string.IsNullOrEmpty(partida.Etapa))
            {
                nombrePartida = partida.Etapa;
            }
            else
            {
                nombrePartida = $"Partida WBS {partida.WBS}";
            }

            var nodo = new NodoConcepto
            {
                EsConcepto = false,
                WBS = partida.WBS,
                Codigo = partida.WBS.ToString(),
                Nombre = nombrePartida,
                Total = partida.Costo,
                Incluir = false,
                Completado = false,
                EsDinamica = partida.EsDinamica,
                ValorM2Tunera = partida.ValorM2Tunera,
                ValorM2Calandra = partida.ValorM2Calandra,
                MetrosCuadrados = partida.MetrosCuadrados,
                LimiteM2 = partida.LimiteM2,
                LimiteM2Tunera = partida.LimiteM2Tunera,
                LimiteM2Calandra = partida.LimiteM2Calandra,
                PrototiposAplicables = partida.PrototiposAplicables
            };

            // Cargar m² desde BD si existe
            if (avancesMetrosCuadrados.ContainsKey(partida.WBS))
            {
                double m2Guardados = avancesMetrosCuadrados[partida.WBS];
                if (m2Guardados > 0)
                {
                    // Validar contra el límite al cargar
                    double limite = nodo.ObtenerLimiteEfectivo(prototipoActual);
                    if (limite > 0 && m2Guardados > limite)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"⚠️ WBS {partida.WBS}: m² guardados ({m2Guardados:F2}) excede límite ({limite:F2}), ajustando...");
                        m2Guardados = limite;
                    }
                    
                    nodo.MetrosCuadrados = m2Guardados;
                    
                    if (nodo.EsDinamica)
                    {
                        double valorM2 = !string.IsNullOrEmpty(prototipoActual) && prototipoActual.ToUpper().Contains("TUNERA") 
                            ? nodo.ValorM2Tunera 
                            : nodo.ValorM2Calandra;
                        nodo.Total = nodo.MetrosCuadrados * valorM2;
                    }
                }
            }

            // Cargar avances si existen
            if (avances.TryGetValue(partida.WBS, out var avance))
            {
                nodo.AvancePorcentaje = avance.Item1;
                nodo.MontoEjecutado = !double.IsNaN(avance.Item2) 
                    ? avance.Item2 
                    : nodo.Total * (nodo.AvancePorcentaje / 100.0);
                nodo.FechaFinalizacion = avance.Item3;
                nodo.Completado = nodo.AvancePorcentaje >= 100.0;
            }

            return nodo;
        }

        private string NormalizeForComparison(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string normalized = input.Trim().ToLowerInvariant();
            
            normalized = normalized
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("ñ", "n")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("-", " ")
                .Replace("_", " ");
            
            while (normalized.Contains("  "))
                normalized = normalized.Replace("  ", " ");

            return normalized.Trim();
        }
    }
}
