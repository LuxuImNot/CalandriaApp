# ?? ANÁLISIS DE DISCREPANCIA EN CÁLCULO DE AVANCES

## ?? Problema Identificado

Los avances mostrados en **PanelPrincipal** y **FormEstimacionConceptoMigrado** **NO son iguales** a pesar de que el código indica que deberían calcularse de la misma manera.

---

## ?? Comparación de Métodos

### 1?? **FormEstimacionConceptoMigrado** (CORRECTO)

#### `CargarEstimacionJerarquica` ? `ActualizarTotales`

```csharp
// FormEstimacionConceptoMigrado.CargaDatos.cs
private void CargarEstimacionJerarquica(string manzana, string lote)
{
    // 1. Cargar avances desde BD (con MontoEjecutado)
    var avancesPartidas = CargarAvancesPartidas(conn, manzana, lote);
    
    // 2. Cargar TODAS las partidas de PresupuestoObra
    var todasLasPartidas = CargarTodasLasPartidas(conn);
    
    // 3. FILTRAR por prototipo
    todasLasPartidas = todasLasPartidas
        .Where(p => p.AplicaAPrototipo(prototipoActual))
        .ToList();
    
    // 4. Crear nodos con avances cargados
    var nodoPartida = CrearNodoPartidaDinamica(partida, avancesPartidas);
    
    // 5. Calcular totales
    RecalcularAvanceConcepto(concepto);
}
```

```csharp
// FormEstimacionConceptoMigrado.Avances.cs
private void ActualizarTotales()
{
    // Recalcular conceptos
    foreach (var concepto in nodosRaiz)
    {
        RecalcularAvanceConcepto(concepto);
    }
    
    // ? CÁLCULO CORRECTO
    double totalPresupuestado = nodosRaiz.Sum(c => c.Total);
    double totalEjecutado = nodosRaiz.Sum(c => c.MontoEjecutado);
    double avanceGeneral = totalPresupuestado > 0 
        ? (totalEjecutado / totalPresupuestado) * 100 
        : 0;
}
```

#### `RecalcularAvanceConcepto`

```csharp
private void RecalcularAvanceConcepto(NodoConcepto concepto)
{
    // ? USA ObtenerTotalPartida para partidas dinámicas
    double totalPartidas = concepto.Partidas.Sum(p => ObtenerTotalPartida(p));
    double ejecutadoPartidas = concepto.Partidas.Sum(p => p.MontoEjecutado);
    
    concepto.Total = totalPartidas;
    concepto.MontoEjecutado = ejecutadoPartidas;
    concepto.AvancePorcentaje = totalPartidas > 0 
        ? (ejecutadoPartidas / totalPartidas) * 100.0 
        : 0;
}
```

---

### 2?? **PanelPrincipal.CargarResumenProgreso** (INCORRECTO - ACTUALIZADO)

#### Código Actual (SUPUESTAMENTE CORREGIDO)

```csharp
// PanelPrincipal.cs - CargarResumenProgreso
private void CargarResumenProgreso()
{
    // ?????? PASO 1: DETECTAR COLUMNA DE COSTO según prototipo
    string columnaCosto = "CostoCalandra"; // Default
    if (casaActual.Prototipo.ToUpper().Contains("TUNERA"))
    {
        columnaCosto = "CostoTunera";
    }
    
    // ?????? PASO 2: CARGAR TODAS LAS PARTIDAS
    string sqlPartidas = $@"
        SELECT 
            WBS_Correcto AS WBS,
            ISNULL([{columnaCosto}], 0) AS Costo,
            PrototiposAplicables
        FROM PresupuestoObra
        WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
        ORDER BY WBS_Correcto";
    
    // ?? PROBLEMA: Solo filtra por prototipo, pero NO considera partidas dinámicas
    var costosPorWBS = new Dictionary<int, decimal>();
    
    // ?? APLICAR FILTRO DE PROTOTIPO
    bool aplicaAProto = true;
    if (!string.IsNullOrEmpty(prototipos) && !string.IsNullOrEmpty(casaActual.Prototipo))
    {
        // ... filtrado de prototipo ...
    }
    
    if (aplicaAProto)
    {
        costosPorWBS[wbs] = costo; // ?? USA COSTO FIJO, NO DINÁMICO
        totalPresupuestado += costo;
    }
    
    // ?????? PASO 3: CARGAR AVANCES
    string sqlAvances = @"
        SELECT WBS, AvancePorcentaje, MontoEjecutado
        FROM AvanceManualObra
        WHERE Manzana = @manzana AND Lote = @lote";
    
    // ?? PROBLEMA: No recalcula MontoEjecutado para partidas dinámicas
    totalEjecutado += (decimal)montoEjecutado;
    
    // ?? PASO 4: CALCULAR AVANCE GENERAL
    decimal avanceGeneral = 0;
    if (totalPresupuestado > 0)
    {
        avanceGeneral = (totalEjecutado / totalPresupuestado) * 100;
    }
}
```

---

## ?? PROBLEMA CRÍTICO IDENTIFICADO

### ? **PanelPrincipal NO considera partidas dinámicas correctamente**

1. **Carga costos FIJOS desde PresupuestoObra**:
   ```csharp
   costosPorWBS[wbs] = costo; // ?? Costo fijo, NO actualizado por m²
   ```

2. **NO recalcula el costo total para partidas dinámicas**:
   - FormEstimacionConceptoMigrado usa `ObtenerTotalPartida(p)` que considera:
     - `EsDinamica`
     - `MetrosCuadrados`
     - `ValorM2Tunera` / `ValorM2Calandra`
   - PanelPrincipal usa el costo directo de la BD sin ajustes

3. **NO recalcula MontoEjecutado para partidas dinámicas**:
   - FormEstimacionConceptoMigrado actualiza `MontoEjecutado` cuando cambian m²
   - PanelPrincipal usa el valor guardado sin verificar si es correcto

---

## ? SOLUCIÓN PROPUESTA

### Opción 1: **Replicar lógica de partidas dinámicas en PanelPrincipal**

Modificar `CargarResumenProgreso` para:

1. Cargar información completa de partidas (incluyendo `EsDinamica`, `ValorM2`, `LimiteM2`)
2. Cargar m² desde `AvanceManualObra.MetrosCuadrados`
3. Recalcular costo total para partidas dinámicas:
   ```csharp
   if (esDinamica && metrosCuadrados > 0)
   {
       double valorM2 = esPrototipoTunera ? valorM2Tunera : valorM2Calandra;
       costoReal = (decimal)(metrosCuadrados * valorM2);
   }
   ```
4. Usar el costo recalculado en lugar del fijo

### Opción 2: **Delegar cálculo a FormEstimacionConceptoMigrado**

Extraer la lógica de cálculo a una clase compartida:

```csharp
public class CalculadorAvances
{
    public static (decimal totalPresupuestado, decimal totalEjecutado, decimal avanceGeneral)
        CalcularAvance(string manzana, string lote, string prototipo, string connectionString)
    {
        // Misma lógica que FormEstimacionConceptoMigrado
    }
}
```

---

## ?? IMPLEMENTACIÓN RECOMENDADA

**Usar Opción 1** porque:
- Evita duplicación de lógica de carga de árbol completo
- PanelPrincipal solo necesita el resumen, no el árbol completo
- Más eficiente en términos de performance

---

## ?? CÓDIGO CORREGIDO PARA PanelPrincipal

```csharp
private void CargarResumenProgreso()
{
    // ... código existente ...
    
    try
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            
            // 1?? DETECTAR COLUMNA DE COSTO
            string columnaCosto = "CostoCalandra";
            bool esPrototipoTunera = false;
            if (!string.IsNullOrEmpty(casaActual.Prototipo))
            {
                if (casaActual.Prototipo.ToUpper().Contains("TUNERA"))
                {
                    columnaCosto = "CostoTunera";
                    esPrototipoTunera = true;
                }
            }
            
            // 2?? CARGAR PARTIDAS CON INFO COMPLETA (incluyendo EsDinamica, ValorM2)
            string sqlPartidas = $@"
                SELECT 
                    WBS_Correcto AS WBS,
                    ISNULL([{columnaCosto}], 0) AS Costo,
                    ISNULL(EsDinamica, 0) AS EsDinamica,
                    ISNULL(ValorM2Tunera, 0) AS ValorM2Tunera,
                    ISNULL(ValorM2Calandra, 0) AS ValorM2Calandra,
                    PrototiposAplicables
                FROM PresupuestoObra
                WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
                ORDER BY WBS_Correcto";
            
            var partidasInfo = new Dictionary<int, (decimal costo, bool esDinamica, double valorM2Tunera, double valorM2Calandra)>();
            decimal totalPresupuestado = 0;
            int totalPartidas = 0;
            
            using (SqlCommand cmd = new SqlCommand(sqlPartidas, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int wbs = Convert.ToInt32(reader["WBS"]);
                    decimal costo = Convert.ToDecimal(reader["Costo"]);
                    bool esDinamica = Convert.ToBoolean(reader["EsDinamica"]);
                    double valorM2Tunera = Convert.ToDouble(reader["ValorM2Tunera"]);
                    double valorM2Calandra = Convert.ToDouble(reader["ValorM2Calandra"]);
                    string prototipos = reader["PrototiposAplicables"] != DBNull.Value 
                        ? reader["PrototiposAplicables"].ToString() 
                        : null;
                    
                    // Filtro de prototipo
                    bool aplicaAProto = true;
                    if (!string.IsNullOrEmpty(prototipos) && !string.IsNullOrEmpty(casaActual.Prototipo))
                    {
                        var protosAplicables = prototipos.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim().ToUpperInvariant())
                            .ToList();
                        
                        if (protosAplicables.Count > 0)
                        {
                            string protoActual = casaActual.Prototipo.ToUpperInvariant();
                            aplicaAProto = protosAplicables.Any(p => protoActual.Contains(p) || p.Contains(protoActual));
                        }
                    }
                    
                    if (aplicaAProto)
                    {
                        partidasInfo[wbs] = (costo, esDinamica, valorM2Tunera, valorM2Calandra);
                        
                        // ? CALCULAR COSTO PRESUPUESTADO (sin m² aún, será actualizado después)
                        totalPresupuestado += costo;
                        totalPartidas++;
                    }
                }
            }
            
            // 3?? CARGAR AVANCES Y METROS CUADRADOS
            decimal totalEjecutado = 0;
            int partidasCompletadas = 0;
            DateTime? ultimaActualizacion = null;
            
            // ? DICCIONARIO PARA AJUSTAR PRESUPUESTO POR PARTIDAS DINÁMICAS
            var ajustesDinamicos = new Dictionary<int, decimal>();
            
            string sqlAvances = @"
                SELECT WBS, AvancePorcentaje, MontoEjecutado, MetrosCuadrados, FechaActualizacion
                FROM AvanceManualObra
                WHERE Manzana = @manzana 
                  AND Lote = @lote
                  AND TRY_CAST(WBS AS INT) > 0";
            
            using (SqlCommand cmd = new SqlCommand(sqlAvances, conn))
            {
                cmd.Parameters.AddWithValue("@manzana", casaActual.Manzana);
                cmd.Parameters.AddWithValue("@lote", casaActual.Lote);
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            string wbsString = reader["WBS"]?.ToString();
                            if (string.IsNullOrWhiteSpace(wbsString) || !int.TryParse(wbsString, out int wbs))
                                continue;
                            
                            // Solo procesar si esta partida está en el presupuesto filtrado
                            if (!partidasInfo.ContainsKey(wbs))
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
                            
                            object m2Obj = reader["MetrosCuadrados"];
                            if (m2Obj != null && m2Obj != DBNull.Value)
                                metrosCuadrados = Convert.ToDouble(m2Obj);
                            
                            var info = partidasInfo[wbs];
                            
                            // ? RECALCULAR PRESUPUESTO PARA PARTIDAS DINÁMICAS
                            if (info.esDinamica && metrosCuadrados > 0)
                            {
                                double valorM2 = esPrototipoTunera ? info.valorM2Tunera : info.valorM2Calandra;
                                decimal costoRecalculado = (decimal)(metrosCuadrados * valorM2);
                                
                                // Guardar diferencia para ajustar total presupuestado
                                decimal diferencia = costoRecalculado - info.costo;
                                ajustesDinamicos[wbs] = diferencia;
                                
                                // ? RECALCULAR MONTO EJECUTADO si es necesario
                                if (montoEjecutado == 0 && avancePorcentaje > 0)
                                {
                                    montoEjecutado = (double)(costoRecalculado * (decimal)(avancePorcentaje / 100.0));
                                }
                            }
                            else
                            {
                                // Para partidas NO dinámicas, calcular desde porcentaje si no hay monto
                                if (montoEjecutado == 0 && avancePorcentaje > 0)
                                {
                                    montoEjecutado = (double)(info.costo * (decimal)(avancePorcentaje / 100.0));
                                }
                            }
                            
                            totalEjecutado += (decimal)montoEjecutado;
                            
                            if (avancePorcentaje >= 100)
                                partidasCompletadas++;
                            
                            // Capturar última fecha
                            object fechaObj = reader["FechaActualizacion"];
                            if (fechaObj != null && fechaObj != DBNull.Value)
                            {
                                DateTime fecha = Convert.ToDateTime(fechaObj);
                                if (!ultimaActualizacion.HasValue || fecha > ultimaActualizacion.Value)
                                    ultimaActualizacion = fecha;
                            }
                        }
                        catch (Exception exRow)
                        {
                            System.Diagnostics.Debug.WriteLine($"? Error al procesar avance: {exRow.Message}");
                        }
                    }
                }
            }
            
            // ? AJUSTAR TOTAL PRESUPUESTADO POR PARTIDAS DINÁMICAS
            foreach (var ajuste in ajustesDinamicos.Values)
            {
                totalPresupuestado += ajuste;
            }
            
            int partidasPendientes = totalPartidas - partidasCompletadas;
            
            // 4?? CALCULAR AVANCE GENERAL
            decimal avanceGeneral = 0;
            if (totalPresupuestado > 0)
            {
                avanceGeneral = (totalEjecutado / totalPresupuestado) * 100;
                if (avanceGeneral > 100) avanceGeneral = 100;
            }
            
            // 5?? ACTUALIZAR UI
            if (progressBarGeneral != null)
            {
                progressBarGeneral.Style = ProgressBarStyle.Blocks;
                progressBarGeneral.Value = Math.Min(100, Math.Max(0, (int)avanceGeneral));
            }
            if (lblProgresoGeneralValor != null)
                lblProgresoGeneralValor.Text = $"{avanceGeneral:F1}%";
            
            if (progressBarPartidas != null)
            {
                progressBarPartidas.Style = ProgressBarStyle.Blocks;
                progressBarPartidas.Value = Math.Min(100, Math.Max(0, (int)avanceGeneral));
            }
            if (lblProgresoPartidasValor != null)
                lblProgresoPartidasValor.Text = $"{totalEjecutado:C2}";
            
            if (lblTotalDestajos != null)
                lblTotalDestajos.Text = $"Total de Partidas: {totalPartidas}";
            if (lblDestajosCompletados != null)
                lblDestajosCompletados.Text = $"Partidas Completadas: {partidasCompletadas}";
            if (lblDestajosPendientes != null)
                lblDestajosPendientes.Text = $"Partidas Pendientes: {partidasPendientes}";
            if (lblUltimaActualizacion != null)
            {
                lblUltimaActualizacion.Text = ultimaActualizacion.HasValue ? 
                    $"Última Actualización: {ultimaActualizacion.Value:dd/MM/yyyy HH:mm}" : 
                    "Última Actualización: Sin datos";
            }
            
            // ?? DEBUG
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("+----------------------------------------------------------------------+");
            System.Diagnostics.Debug.WriteLine("?  ?????? RESUMEN DE PROGRESO (PanelPrincipal) - CORREGIDO             ?");
            System.Diagnostics.Debug.WriteLine("?----------------------------------------------------------------------?");
            System.Diagnostics.Debug.WriteLine($"?  Casa: M{casaActual.Manzana}-L{casaActual.Lote}");
            System.Diagnostics.Debug.WriteLine($"?  Prototipo: {casaActual.Prototipo}");
            System.Diagnostics.Debug.WriteLine($"?  Columna de costo usada: {columnaCosto}");
            System.Diagnostics.Debug.WriteLine("?----------------------------------------------------------------------?");
            System.Diagnostics.Debug.WriteLine($"?  ?? Total Presupuestado: {totalPresupuestado:C2} (ajustado por dinámicas)");
            System.Diagnostics.Debug.WriteLine($"?  ? Total Ejecutado:     {totalEjecutado:C2}");
            System.Diagnostics.Debug.WriteLine($"?  ?? Avance General:      {avanceGeneral:F1}%");
            System.Diagnostics.Debug.WriteLine("?----------------------------------------------------------------------?");
            System.Diagnostics.Debug.WriteLine($"?  ?? Partidas dinámicas ajustadas: {ajustesDinamicos.Count}");
            if (ajustesDinamicos.Count > 0)
            {
                decimal totalAjuste = ajustesDinamicos.Values.Sum();
                System.Diagnostics.Debug.WriteLine($"?  ?? Total ajuste dinámico: {totalAjuste:C2}");
            }
            System.Diagnostics.Debug.WriteLine("+----------------------------------------------------------------------+");
            #endif
        }
    }
    catch (Exception ex)
    {
        // ... manejo de error existente ...
    }
}
```

---

## ?? RESULTADO ESPERADO

Después de aplicar la corrección:

- ? **PanelPrincipal** y **FormEstimacionConceptoMigrado** mostrarán **EXACTAMENTE** los mismos valores
- ? Partidas dinámicas se consideran correctamente en ambos lugares
- ? Total presupuestado se ajusta según m² reales
- ? Monto ejecutado se recalcula para partidas dinámicas

---

## ?? PRUEBA DE VALIDACIÓN

Para verificar que la corrección funciona:

1. Seleccionar una casa en PanelPrincipal
2. Anotar el avance mostrado
3. Abrir FormEstimacionConceptoMigrado con la misma casa
4. Comparar:
   - ? Total Presupuestado
   - ? Total Ejecutado
   - ? Avance General %

**Deben ser IDÉNTICOS** ± 0.1% de diferencia por redondeos.

---

## ?? Fecha de Análisis
2024-12-XX

## ?? Autor del Análisis
GitHub Copilot - Asistente AI
