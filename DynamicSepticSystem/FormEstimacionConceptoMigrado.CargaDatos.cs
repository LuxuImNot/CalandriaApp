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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
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
                        using (var reader = cmd.ExecuteReader())
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

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Lote FROM InventarioCasas WHERE Manzana = @m ORDER BY Lote";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        using (var reader = cmd.ExecuteReader())
                        {
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Cargar avances primero (incluyendo m²)
                    var avancesPartidas = CargarAvancesPartidas(conn, manzana, lote);
                    System.Diagnostics.Debug.WriteLine($"📋 Avances cargados: {avancesPartidas.Count}");

                    // Cargar todas las partidas con su información completa
                    var todasLasPartidas = CargarTodasLasPartidas(conn);
                    System.Diagnostics.Debug.WriteLine($"📋 Total partidas en BD: {todasLasPartidas.Count}");
                    
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

        /// <summary>
        /// Carga todas las partidas de PresupuestoObra con toda la información necesaria
        /// </summary>
        private List<PartidaDinamica> CargarTodasLasPartidas(SqlConnection conn)
        {
            var partidas = new List<PartidaDinamica>();
            
            try
            {
                // Verificar columnas existentes
                var columnasExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (SqlCommand cmdCols = new SqlCommand(@"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'PresupuestoObra'", conn))
                using (var reader = cmdCols.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnasExistentes.Add(reader.GetString(0));
                    }
                }

                bool tieneWBS = columnasExistentes.Contains("WBS_Correcto");
                bool tieneCodigo = columnasExistentes.Contains("Codigo");
                bool tienePadre = columnasExistentes.Contains("Padre");
                bool tieneEtapa = columnasExistentes.Contains("Etapa");
                bool tienePartida = columnasExistentes.Contains("Partida");
                bool tieneCostoTunera = columnasExistentes.Contains("CostoTunera");
                bool tieneCostoCalandra = columnasExistentes.Contains("CostoCalandra");
                bool tieneEsDinamica = columnasExistentes.Contains("EsDinamica");
                bool tieneValorM2Tunera = columnasExistentes.Contains("ValorM2Tunera");
                bool tieneValorM2Calandra = columnasExistentes.Contains("ValorM2Calandra");
                bool tieneLimiteM2 = columnasExistentes.Contains("LimiteM2");
                bool tieneLimiteM2Tunera = columnasExistentes.Contains("LimiteM2Tunera");
                bool tieneLimiteM2Calandra = columnasExistentes.Contains("LimiteM2Calandra");
                bool tienePrototipos = columnasExistentes.Contains("PrototiposAplicables");

                System.Diagnostics.Debug.WriteLine($"📋 Columnas de PresupuestoObra detectadas:");
                System.Diagnostics.Debug.WriteLine($"   WBS_Correcto: {tieneWBS}, Codigo: {tieneCodigo}, Padre: {tienePadre}");
                System.Diagnostics.Debug.WriteLine($"   EsDinamica: {tieneEsDinamica}, Prototipos: {tienePrototipos}");

                if (!tieneWBS)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ La tabla PresupuestoObra no tiene columna WBS_Correcto");
                    return partidas;
                }

                // Construir query dinámicamente
                var columnas = new List<string> { "WBS_Correcto AS WBS" };
                
                if (tieneCodigo) columnas.Add("Codigo");
                if (tienePadre) columnas.Add("Padre");
                if (tieneEtapa) columnas.Add("Etapa");
                if (tienePartida) columnas.Add("Partida");
                if (tieneCostoTunera) columnas.Add("ISNULL(CostoTunera, 0) AS CostoTunera");
                if (tieneCostoCalandra) columnas.Add("ISNULL(CostoCalandra, 0) AS CostoCalandra");
                if (tieneEsDinamica) columnas.Add("ISNULL(EsDinamica, 0) AS EsDinamica");
                if (tieneValorM2Tunera) columnas.Add("ISNULL(ValorM2Tunera, 0) AS ValorM2Tunera");
                if (tieneValorM2Calandra) columnas.Add("ISNULL(ValorM2Calandra, 0) AS ValorM2Calandra");
                if (tieneLimiteM2) columnas.Add("ISNULL(LimiteM2, 0) AS LimiteM2");
                if (tieneLimiteM2Tunera) columnas.Add("ISNULL(LimiteM2Tunera, 0) AS LimiteM2Tunera");
                if (tieneLimiteM2Calandra) columnas.Add("ISNULL(LimiteM2Calandra, 0) AS LimiteM2Calandra");
                if (tienePrototipos) columnas.Add("PrototiposAplicables");

                string sql = $@"
                    SELECT {string.Join(", ", columnas)}
                    FROM PresupuestoObra
                    WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
                    ORDER BY {(tieneCodigo ? "Codigo, " : "")}WBS_Correcto";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var partida = new PartidaDinamica
                        {
                            WBS = Convert.ToInt32(reader["WBS"]),
                            Codigo = tieneCodigo ? (reader["Codigo"]?.ToString() ?? "") : "",
                            Padre = tienePadre ? (reader["Padre"]?.ToString() ?? "") : "",
                            Etapa = tieneEtapa ? (reader["Etapa"]?.ToString() ?? "") : "",
                            Partida = tienePartida ? (reader["Partida"]?.ToString() ?? "") : "",
                            Costo = tieneCostoTunera ? Convert.ToDouble(reader["CostoTunera"]) : 0,
                            EsDinamica = tieneEsDinamica && Convert.ToBoolean(reader["EsDinamica"]),
                            ValorM2Tunera = tieneValorM2Tunera ? Convert.ToDouble(reader["ValorM2Tunera"]) : 0,
                            ValorM2Calandra = tieneValorM2Calandra ? Convert.ToDouble(reader["ValorM2Calandra"]) : 0,
                            LimiteM2 = tieneLimiteM2 ? Convert.ToDouble(reader["LimiteM2"]) : 0,
                            LimiteM2Tunera = tieneLimiteM2Tunera ? Convert.ToDouble(reader["LimiteM2Tunera"]) : 0,
                            LimiteM2Calandra = tieneLimiteM2Calandra ? Convert.ToDouble(reader["LimiteM2Calandra"]) : 0,
                            PrototiposAplicables = tienePrototipos ? (reader["PrototiposAplicables"]?.ToString()) : null,
                            MetrosCuadrados = 0
                        };
                        
                        // Usar CostoCalandra si está usando prototipo Calandra
                        if (!string.IsNullOrEmpty(prototipoActual) && 
                            !prototipoActual.ToUpper().Contains("TUNERA") && 
                            tieneCostoCalandra)
                        {
                            partida.Costo = Convert.ToDouble(reader["CostoCalandra"]);
                        }
                        
                        partidas.Add(partida);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"📋 Partidas cargadas de BD: {partidas.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar partidas: {ex.Message}");
            }
            
            return partidas;
        }

        private List<Tuple<string, string>> CargarConceptos(SqlConnection conn)
        {
            var conceptos = new List<Tuple<string, string>>();
            
            try
            {
                string sql = @"
                    SELECT DISTINCT Codigo, Concepto
                    FROM [Estimacion(Concepto)]
                    WHERE Codigo IS NOT NULL AND Concepto IS NOT NULL
                    ORDER BY Codigo";
                
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string codigo = reader["Codigo"]?.ToString() ?? "";
                        string nombre = reader["Concepto"]?.ToString() ?? "";
                        
                        if (!string.IsNullOrEmpty(codigo) && int.TryParse(codigo, out int _))
                        {
                            conceptos.Add(new Tuple<string, string>(codigo, nombre));
                        }
                    }
                }
                
                conceptos = conceptos.OrderBy(c => int.TryParse(c.Item1, out int num) ? num : 999).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar conceptos: {ex.Message}");
            }
            
            return conceptos;
        }

        private List<PartidaDinamica> CargarPartidasConDinamicas(SqlConnection conn)
        {
            // Este método ahora delega a CargarTodasLasPartidas para evitar duplicación
            return CargarTodasLasPartidas(conn);
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

        private Dictionary<int, Tuple<double, double, DateTime?>> CargarAvancesPartidas(SqlConnection conn, string manzana, string lote)
        {
            var avances = new Dictionary<int, Tuple<double, double, DateTime?>>();

            try
            {
                // Verificar columnas existentes
                var columnasExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (SqlCommand cmdCols = new SqlCommand(@"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'AvanceManualObra'", conn))
                using (var reader = cmdCols.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnasExistentes.Add(reader.GetString(0));
                    }
                }

                bool tieneMetrosCuadrados = columnasExistentes.Contains("MetrosCuadrados");
                bool tieneFechaFinalizacion = columnasExistentes.Contains("FechaFinalizacion");

                string sql = "SELECT WBS, AvancePorcentaje, MontoEjecutado";
                if (tieneFechaFinalizacion) sql += ", FechaFinalizacion";
                if (tieneMetrosCuadrados) sql += ", MetrosCuadrados";
                sql += " FROM AvanceManualObra WHERE Manzana = @manzana AND Lote = @lote";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@manzana", manzana);
                    cmd.Parameters.AddWithValue("@lote", lote);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                string wbsString = reader["WBS"]?.ToString();
                                if (string.IsNullOrWhiteSpace(wbsString) || !int.TryParse(wbsString, out int wbs))
                                    continue;

                                double avancePorcentaje = 0;
                                double montoEjecutado = 0;
                                double metrosCuadrados = 0;
                
                                object avanceObj = reader["AvancePorcentaje"];
                                if (avanceObj != null && avanceObj != DBNull.Value)
                                    avancePorcentaje = Convert.ToDouble(avanceObj);

                                object montoObj = reader["MontoEjecutado"];
                                if (montoObj != null && montoObj != DBNull.Value)
                                    montoEjecutado = Convert.ToDouble(montoObj);

                                if (tieneMetrosCuadrados)
                                {
                                    object m2Obj = reader["MetrosCuadrados"];
                                    if (m2Obj != null && m2Obj != DBNull.Value)
                                        metrosCuadrados = Convert.ToDouble(m2Obj);
                                }

                                DateTime? fechaFinalizacion = null;
                                if (tieneFechaFinalizacion)
                                {
                                    object fechaObj = reader["FechaFinalizacion"];
                                    if (fechaObj != null && fechaObj != DBNull.Value)
                                        fechaFinalizacion = Convert.ToDateTime(fechaObj);
                                }

                                avances[wbs] = new Tuple<double, double, DateTime?>(avancePorcentaje, montoEjecutado, fechaFinalizacion);

                                if (metrosCuadrados > 0 && !avancesMetrosCuadrados.ContainsKey(wbs))
                                {
                                    avancesMetrosCuadrados[wbs] = metrosCuadrados;
                                }
                            }
                            catch (Exception exRow)
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ Error al procesar fila: {exRow.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar avances: {ex.Message}");
            }

            return avances;
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
