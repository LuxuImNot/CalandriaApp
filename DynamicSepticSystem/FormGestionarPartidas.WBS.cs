using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// FormGestionarPartidas - Reorganización de WBS
    /// </summary>
    public partial class FormGestionarPartidas
    {
        #region Reorganización de WBS
        
        /// <summary>
        /// Reorganiza el WBS de un concepto completo preservando el progreso de obras
        /// ADVERTENCIA: Esta operación es compleja y debe usarse con precaución
        /// </summary>
        private bool ReorganizarWBSConcepto(SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                Debug.WriteLine("?? Iniciando reorganización WBS...");
                
                // Detectar tipo de dato de WBS_Correcto
                string tipoWBS = "INT";
                using (SqlCommand cmdTipo = new SqlCommand(@"
                    SELECT DATA_TYPE 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'PresupuestoObra' 
                    AND COLUMN_NAME = 'WBS_Correcto'", conn, transaction))
                {
                    var result = cmdTipo.ExecuteScalar();
                    if (result != null)
                    {
                        tipoWBS = result.ToString().ToUpper();
                        Debug.WriteLine($"?? Tipo de WBS_Correcto: {tipoWBS}");
                    }
                }
                
                bool wbsEsTexto = tipoWBS.Contains("VARCHAR") || tipoWBS.Contains("CHAR");
                
                // Detectar si existe IdPresupuestoObra en AvanceManualObra (vínculo permanente)
                bool tieneVinculoPermanente = VerificarExisteVinculoPermanente(conn, transaction);
                Debug.WriteLine($"?? Usa vínculo permanente (IdPresupuestoObra): {tieneVinculoPermanente}");
                
                // 1. Obtener el WBS base del concepto
                int wbsBase = ObtenerWBSBase(conn, transaction, wbsEsTexto);
                Debug.WriteLine($"?? WBS Base: {wbsBase}");
                
                // 2. Verificar si hay progreso registrado
                bool hayProgreso = VerificarProgresoRegistrado(conn, transaction, wbsEsTexto);
                Debug.WriteLine($"?? Hay progreso registrado: {hayProgreso}");
                
                if (hayProgreso)
                {
                    string mensajeAdvertencia = tieneVinculoPermanente
                        ? "?? ADVERTENCIA: Existen obras con progreso registrado.\n\n" +
                          "El sistema usará IdPresupuestoObra (vínculo permanente):\n" +
                          "• ? El progreso NUNCA se perderá aunque el WBS cambie\n" +
                          "• ? La relación entre partidas y avances es inmutable\n" +
                          "• Solo se actualizará el WBS para orden visual\n\n" +
                          "¿Deseas continuar con la reorganización segura?"
                        : "?? ADVERTENCIA: Existen obras con progreso registrado.\n\n" +
                          "Si reorganizas el WBS:\n" +
                          "• Se preservará el progreso usando mapeo de 2 fases\n" +
                          "• Primero se asignarán WBS temporales (evita conflictos)\n" +
                          "• Luego se asignarán los WBS finales\n" +
                          "• Las partidas mantendrán su vinculación con las obras\n\n" +
                          "?? RECOMENDACIÓN: Ejecuta el script SQL\n" +
                          "'CrearVinculoPermanenteWBS.sql' para mayor seguridad.\n\n" +
                          "¿Deseas continuar con la reorganización?";
                    
                    var confirmResult = MessageBox.Show(
                        mensajeAdvertencia,
                        tieneVinculoPermanente ? "Reorganización con Vínculo Permanente" : "Confirmar Reorganización con Progreso",
                        MessageBoxButtons.YesNo,
                        tieneVinculoPermanente ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    
                    if (confirmResult != DialogResult.Yes)
                        return false;
                }
                
                // 3. Crear tabla temporal
                CrearTablaTemporal(conn, transaction, tieneVinculoPermanente);
                Debug.WriteLine("? Tabla temporal #MapeoWBS creada");
                
                // 4. Guardar WBS originales y Id si existe vínculo permanente
                var wbsOriginales = new Dictionary<PartidaConcepto, int>();
                var idPresupuestoOriginales = new Dictionary<PartidaConcepto, int>();
                
                if (tieneVinculoPermanente)
                {
                    CargarIdPresupuestoObra(conn, transaction, idPresupuestoOriginales);
                }
                
                foreach (var partida in partidasConcepto)
                {
                    wbsOriginales[partida] = partida.WBS;
                }
                
                // 5. FASE 1: Mover todos los WBS existentes a valores temporales muy altos
                // Esto evita conflictos cuando los nuevos WBS coinciden con los viejos
                Debug.WriteLine("?? FASE 1: Moviendo WBS a valores temporales...");
                MoverWBSATemporales(conn, transaction, wbsEsTexto, wbsOriginales, tieneVinculoPermanente);
                
                // 6. FASE 2: Asignar nuevos WBS secuenciales
                Debug.WriteLine("?? FASE 2: Asignando WBS finales...");
                var resultado = AsignarNuevosWBS(conn, transaction, wbsBase, wbsOriginales, tieneVinculoPermanente, idPresupuestoOriginales);
                
                // 7. Actualizar tablas de la base de datos (de temporal a final)
                if (tieneVinculoPermanente)
                {
                    // Con vínculo permanente: actualizar WBS usando el Id como referencia (más seguro)
                    ActualizarWBSUsandoIdPresupuesto(conn, transaction, wbsEsTexto);
                }
                else
                {
                    // Sin vínculo permanente: usar mapeo por WBS temporal (método legacy)
                    ActualizarWBSEnAvanceManualObra(conn, transaction, wbsEsTexto);
                }
                
                ActualizarWBSEnPresupuestoObra(conn, transaction, wbsEsTexto, tieneVinculoPermanente);
                
                // 8. Limpiar tabla temporal
                using (SqlCommand cmdDrop = new SqlCommand("DROP TABLE #MapeoWBS", conn, transaction))
                {
                    cmdDrop.ExecuteNonQuery();
                }
                
                Debug.WriteLine(
                    $"? WBS reorganizado:\n" +
                    $"   - Partidas existentes reorganizadas: {resultado.PartidasExistentes}\n" +
                    $"   - Partidas nuevas con WBS asignado: {resultado.PartidasNuevas}\n" +
                    $"   - WBS base: {wbsBase}\n" +
                    $"   - WBS final: {resultado.WBSFinal}\n" +
                    $"   - Método: {(tieneVinculoPermanente ? "Id (permanente)" : "WBS temporal (legacy)")}");
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error en ReorganizarWBSConcepto: {ex.Message}");
                Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                
                MessageBox.Show(
                    $"? Error al reorganizar WBS:\n\n{ex.Message}\n\nVer Output para detalles técnicos.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Verifica si existe la columna IdPresupuestoObra en AvanceManualObra
        /// y la columna Id en PresupuestoObra (vínculo permanente)
        /// </summary>
        private bool VerificarExisteVinculoPermanente(SqlConnection conn, SqlTransaction transaction)
        {
            bool existeIdEnPresupuesto = false;
            bool existeIdPresupuestoEnAvance = false;
            
            // Verificar que existe Id en PresupuestoObra
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Id'", conn, transaction))
            {
                existeIdEnPresupuesto = (int)cmd.ExecuteScalar() > 0;
            }
            
            // Verificar que existe IdPresupuestoObra en AvanceManualObra
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'IdPresupuestoObra'", conn, transaction))
            {
                existeIdPresupuestoEnAvance = (int)cmd.ExecuteScalar() > 0;
            }
            
            return existeIdEnPresupuesto && existeIdPresupuestoEnAvance;
        }
        
        /// <summary>
        /// Carga los Id de PresupuestoObra para las partidas del concepto actual
        /// </summary>
        private void CargarIdPresupuestoObra(SqlConnection conn, SqlTransaction transaction, 
            Dictionary<PartidaConcepto, int> idPresupuestoOriginales)
        {
            string sql = @"
                SELECT WBS_Correcto, Id 
                FROM PresupuestoObra 
                WHERE Codigo = @codigo 
                AND Id IS NOT NULL";
            
            var mapeoWBStoId = new Dictionary<int, int>();
            
            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                        {
                            int wbs = Convert.ToInt32(reader[0]);
                            int id = Convert.ToInt32(reader[1]);
                            mapeoWBStoId[wbs] = id;
                        }
                    }
                }
            }
            
            foreach (var partida in partidasConcepto)
            {
                if (partida.WBS > 0 && mapeoWBStoId.TryGetValue(partida.WBS, out int idPresup))
                {
                    idPresupuestoOriginales[partida] = idPresup;
                }
            }
            
            Debug.WriteLine($"?? Cargados {idPresupuestoOriginales.Count} Id de PresupuestoObra");
        }
        
        /// <summary>
        /// FASE 1: Mueve todos los WBS existentes a valores temporales (800000+)
        /// para evitar conflictos durante la reorganización
        /// </summary>
        private void MoverWBSATemporales(
            SqlConnection conn, 
            SqlTransaction transaction, 
            bool wbsEsTexto,
            Dictionary<PartidaConcepto, int> wbsOriginales,
            bool tieneVinculoPermanente)
        {
            int wbsTemporal = 800000;
            
            // Obtener WBS existentes del concepto que necesitan moverse
            var wbsExistentes = wbsOriginales
                .Where(kv => kv.Value > 0 && kv.Value < 800000)
                .OrderBy(kv => kv.Value)
                .ToList();
            
            if (wbsExistentes.Count == 0)
            {
                Debug.WriteLine("   No hay WBS existentes que mover a temporales");
                return;
            }
            
            Debug.WriteLine($"   Moviendo {wbsExistentes.Count} WBS existentes a temporales...");
            
            // Verificar columna WBS en AvanceManualObra
            bool tieneWBSCorrectoAvance = false;
            using (SqlCommand cmdCheck = new SqlCommand(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'WBS_Correcto'", conn, transaction))
            {
                tieneWBSCorrectoAvance = (int)cmdCheck.ExecuteScalar() > 0;
            }
            string colWBSAvance = tieneWBSCorrectoAvance ? "WBS_Correcto" : "WBS";
            
            // Detectar tipo de WBS en AvanceManualObra
            string tipoWBSAvance = "INT";
            using (SqlCommand cmdTipoAvance = new SqlCommand($@"
                SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = '{colWBSAvance}'", conn, transaction))
            {
                var result = cmdTipoAvance.ExecuteScalar();
                if (result != null) tipoWBSAvance = result.ToString().ToUpper();
            }
            bool wbsAvanceEsTexto = tipoWBSAvance.Contains("VARCHAR") || tipoWBSAvance.Contains("CHAR");
            
            foreach (var kv in wbsExistentes)
            {
                int wbsViejo = kv.Value;
                int wbsTemp = wbsTemporal++;
                
                // Actualizar PresupuestoObra
                string sqlPresup = "UPDATE PresupuestoObra SET WBS_Correcto = @nuevo WHERE WBS_Correcto = @viejo AND Codigo = @codigo";
                
                using (SqlCommand cmd = new SqlCommand(sqlPresup, conn, transaction))
                {
                    if (wbsEsTexto)
                    {
                        cmd.Parameters.AddWithValue("@nuevo", wbsTemp.ToString());
                        cmd.Parameters.AddWithValue("@viejo", wbsViejo.ToString());
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@nuevo", wbsTemp);
                        cmd.Parameters.AddWithValue("@viejo", wbsViejo);
                    }
                    cmd.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                    cmd.ExecuteNonQuery();
                }
                
                // Si NO tiene vínculo permanente, actualizar AvanceManualObra por WBS
                // Si tiene vínculo permanente, no necesitamos actualizar aquí porque el vínculo es por Id
                if (!tieneVinculoPermanente)
                {
                    string sqlAvance = wbsAvanceEsTexto
                        ? $"UPDATE AvanceManualObra SET {colWBSAvance} = @nuevo WHERE {colWBSAvance} = @viejo"
                        : $"UPDATE AvanceManualObra SET {colWBSAvance} = @nuevo WHERE {colWBSAvance} = @viejo";
                    
                    using (SqlCommand cmd = new SqlCommand(sqlAvance, conn, transaction))
                    {
                        if (wbsAvanceEsTexto)
                        {
                            cmd.Parameters.AddWithValue("@nuevo", wbsTemp.ToString());
                            cmd.Parameters.AddWithValue("@viejo", wbsViejo.ToString());
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@nuevo", wbsTemp);
                            cmd.Parameters.AddWithValue("@viejo", wbsViejo);
                        }
                        int filas = cmd.ExecuteNonQuery();
                        if (filas > 0)
                        {
                            Debug.WriteLine($"   WBS {wbsViejo} ? temporal {wbsTemp} ({filas} avances actualizados)");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"   WBS {wbsViejo} ? temporal {wbsTemp} (AvanceManualObra usa IdPresupuestoObra)");
                }
                
                // Actualizar el mapeo en memoria
                kv.Key.WBS = wbsTemp;
            }
        }
        
        private int ObtenerWBSBase(SqlConnection conn, SqlTransaction transaction, bool wbsEsTexto)
        {
            int wbsBase = 0;
            string sqlBase = wbsEsTexto 
                ? @"SELECT MIN(CAST(WBS_Correcto AS INT))
                    FROM PresupuestoObra 
                    WHERE Codigo = @codigo 
                      AND WBS_Correcto IS NOT NULL 
                      AND ISNUMERIC(WBS_Correcto) = 1
                      AND CAST(WBS_Correcto AS INT) < 800000"
                : @"SELECT MIN(WBS_Correcto) 
                    FROM PresupuestoObra 
                    WHERE Codigo = @codigo 
                      AND WBS_Correcto IS NOT NULL
                      AND WBS_Correcto < 800000";
            
            using (SqlCommand cmdBase = new SqlCommand(sqlBase, conn, transaction))
            {
                cmdBase.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                var result = cmdBase.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    wbsBase = Convert.ToInt32(result);
            }
            
            if (wbsBase == 0)
            {
                // Si no hay partidas existentes, obtener el siguiente WBS disponible
                string sqlMaxWBS = wbsEsTexto
                    ? @"SELECT ISNULL(MAX(CAST(WBS_Correcto AS INT)), 0) + 1
                        FROM PresupuestoObra 
                        WHERE WBS_Correcto IS NOT NULL 
                          AND ISNUMERIC(WBS_Correcto) = 1
                          AND CAST(WBS_Correcto AS INT) < 800000"
                    : @"SELECT ISNULL(MAX(WBS_Correcto), 0) + 1
                        FROM PresupuestoObra 
                        WHERE WBS_Correcto IS NOT NULL
                          AND WBS_Correcto < 800000";
                
                using (SqlCommand cmdMax = new SqlCommand(sqlMaxWBS, conn, transaction))
                {
                    var result = cmdMax.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        wbsBase = Convert.ToInt32(result);
                    else
                        wbsBase = 1;
                }
                
                Debug.WriteLine($"?? No hay WBS base existente, usando siguiente disponible: {wbsBase}");
            }
            
            return wbsBase;
        }
        
        private bool VerificarProgresoRegistrado(SqlConnection conn, SqlTransaction transaction, bool wbsEsTexto)
        {
            bool existeTabla = false;
            using (SqlCommand cmdCheckTable = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'AvanceManualObra'", conn, transaction))
            {
                existeTabla = (int)cmdCheckTable.ExecuteScalar() > 0;
            }
            
            Debug.WriteLine($"?? Tabla AvanceManualObra existe: {existeTabla}");
            
            if (!existeTabla)
                return false;
            
            // Verificar si existe columna WBS_Correcto en AvanceManualObra
            bool tieneWBSCorrectoAvance = false;
            using (SqlCommand cmdCheckWBSCol = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' 
                AND COLUMN_NAME = 'WBS_Correcto'", conn, transaction))
            {
                tieneWBSCorrectoAvance = (int)cmdCheckWBSCol.ExecuteScalar() > 0;
            }
            
            string nombreColumnaWBSAvance = tieneWBSCorrectoAvance ? "WBS_Correcto" : "WBS";
            
            string tipoWBSAvance = "INT";
            using (SqlCommand cmdTipoAvance = new SqlCommand($@"
                SELECT DATA_TYPE 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' 
                AND COLUMN_NAME = '{nombreColumnaWBSAvance}'", conn, transaction))
            {
                var result = cmdTipoAvance.ExecuteScalar();
                if (result != null)
                    tipoWBSAvance = result.ToString().ToUpper();
            }
            
            bool wbsAvanceEsTexto = tipoWBSAvance.Contains("VARCHAR") || tipoWBSAvance.Contains("CHAR");
            
            Debug.WriteLine($"?? Columna WBS en AvanceManualObra: {nombreColumnaWBSAvance} (tipo: {tipoWBSAvance})");
            
            // Detectar columnas disponibles
            bool tieneMontoEjecutado = false;
            bool tieneImporteEjecutado = false;
            bool tieneIdPresupuestoObra = false;
            
            using (SqlCommand cmdCheckCols = new SqlCommand(@"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' 
                AND COLUMN_NAME IN ('MontoEjecutado', 'ImporteEjecutado', 'IdPresupuestoObra')", conn, transaction))
            using (SqlDataReader reader = cmdCheckCols.ExecuteReader())
            {
                while (reader.Read())
                {
                    string colName = reader.GetString(0);
                    if (colName == "MontoEjecutado")
                        tieneMontoEjecutado = true;
                    else if (colName == "ImporteEjecutado")
                        tieneImporteEjecutado = true;
                    else if (colName == "IdPresupuestoObra")
                        tieneIdPresupuestoObra = true;
                }
            }
            
            // Construir JOIN compatible - preferir IdPresupuestoObra si existe
            string joinCondition;
            if (tieneIdPresupuestoObra)
            {
                joinCondition = "amo.IdPresupuestoObra = p.Id";
            }
            else if (wbsEsTexto && wbsAvanceEsTexto)
            {
                joinCondition = $"amo.{nombreColumnaWBSAvance} = p.WBS_Correcto";
            }
            else if (wbsEsTexto && !wbsAvanceEsTexto)
            {
                joinCondition = $"amo.{nombreColumnaWBSAvance} = CAST(p.WBS_Correcto AS INT)";
            }
            else if (!wbsEsTexto && wbsAvanceEsTexto)
            {
                joinCondition = $"CAST(amo.{nombreColumnaWBSAvance} AS INT) = p.WBS_Correcto";
            }
            else
            {
                joinCondition = $"amo.{nombreColumnaWBSAvance} = p.WBS_Correcto";
            }
            
            string columnaMonto = tieneMontoEjecutado ? "MontoEjecutado" 
                                : tieneImporteEjecutado ? "ImporteEjecutado" 
                                : null;
            
            string sqlCheck = columnaMonto != null
                ? $@"SELECT COUNT(*)
                    FROM AvanceManualObra amo
                    INNER JOIN PresupuestoObra p ON {joinCondition}
                    WHERE p.Codigo = @codigo 
                    AND (amo.AvancePorcentaje > 0 OR amo.{columnaMonto} > 0)"
                : $@"SELECT COUNT(*)
                    FROM AvanceManualObra amo
                    INNER JOIN PresupuestoObra p ON {joinCondition}
                    WHERE p.Codigo = @codigo 
                    AND amo.AvancePorcentaje > 0";
            
            using (SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn, transaction))
            {
                cmdCheck.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                return (int)cmdCheck.ExecuteScalar() > 0;
            }
        }
        
        private void CrearTablaTemporal(SqlConnection conn, SqlTransaction transaction, bool tieneVinculoPermanente)
        {
            string sql = tieneVinculoPermanente
                ? @"CREATE TABLE #MapeoWBS (
                    IdPresupuesto INT,
                    WBS_Viejo INT,
                    WBS_Temporal INT,
                    WBS_Nuevo INT,
                    Etapa NVARCHAR(255),
                    Partida NVARCHAR(255),
                    EsNueva BIT DEFAULT 0
                )"
                : @"CREATE TABLE #MapeoWBS (
                    WBS_Viejo INT,
                    WBS_Temporal INT,
                    WBS_Nuevo INT,
                    Etapa NVARCHAR(255),
                    Partida NVARCHAR(255),
                    EsNueva BIT DEFAULT 0
                )";
            
            using (SqlCommand cmdTemp = new SqlCommand(sql, conn, transaction))
            {
                cmdTemp.ExecuteNonQuery();
            }
        }
        
        private class ResultadoAsignacionWBS
        {
            public int PartidasExistentes { get; set; }
            public int PartidasNuevas { get; set; }
            public int WBSFinal { get; set; }
        }
        
        private ResultadoAsignacionWBS AsignarNuevosWBS(
            SqlConnection conn, 
            SqlTransaction transaction, 
            int wbsBase, 
            Dictionary<PartidaConcepto, int> wbsOriginales,
            bool tieneVinculoPermanente,
            Dictionary<PartidaConcepto, int> idPresupuestoOriginales)
        {
            int wbsActual = wbsBase;
            int partidasExistentes = 0;
            int partidasNuevas = 0;
            var wbsUsados = new HashSet<int>();
            
            // Primero verificar qué WBS están ocupados por OTROS conceptos
            using (SqlCommand cmdOcupados = new SqlCommand(@"
                SELECT DISTINCT WBS_Correcto 
                FROM PresupuestoObra 
                WHERE Codigo != @codigo 
                AND WBS_Correcto IS NOT NULL 
                AND WBS_Correcto > 0 
                AND WBS_Correcto < 800000", conn, transaction))
            {
                cmdOcupados.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                using (var reader = cmdOcupados.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            int wbsOcupado = Convert.ToInt32(reader[0]);
                            wbsUsados.Add(wbsOcupado);
                        }
                    }
                }
            }
            
            Debug.WriteLine($"   WBS ocupados por otros conceptos: {wbsUsados.Count}");
            
            foreach (var partida in partidasConcepto)
            {
                // El WBS original antes de mover a temporal
                int wbsOriginalReal = wbsOriginales.FirstOrDefault(x => x.Key == partida).Value;
                int wbsActualPartida = partida.WBS; // Puede ser temporal (800000+) o 0 para nuevas
                int idPresupuesto = 0;
                
                if (tieneVinculoPermanente)
                {
                    idPresupuestoOriginales.TryGetValue(partida, out idPresupuesto);
                }
                
                bool esNueva = (wbsOriginalReal == 0 || wbsOriginalReal >= 999000);
                bool esTemporal = (wbsActualPartida >= 800000 && wbsActualPartida < 999000);
                
                // Asegurar que wbsActual no está duplicado
                while (wbsUsados.Contains(wbsActual))
                {
                    wbsActual++;
                }
                
                if (esNueva)
                {
                    string sqlInsert = tieneVinculoPermanente
                        ? @"INSERT INTO #MapeoWBS (IdPresupuesto, WBS_Viejo, WBS_Temporal, WBS_Nuevo, Etapa, Partida, EsNueva)
                            VALUES (NULL, 0, @temporal, @nuevo, @etapa, @partida, 1)"
                        : @"INSERT INTO #MapeoWBS (WBS_Viejo, WBS_Temporal, WBS_Nuevo, Etapa, Partida, EsNueva)
                            VALUES (0, @temporal, @nuevo, @etapa, @partida, 1)";
                    
                    using (SqlCommand cmdMap = new SqlCommand(sqlInsert, conn, transaction))
                    {
                        cmdMap.Parameters.AddWithValue("@temporal", wbsActualPartida > 0 ? wbsActualPartida : (object)DBNull.Value);
                        cmdMap.Parameters.AddWithValue("@nuevo", wbsActual);
                        cmdMap.Parameters.AddWithValue("@etapa", partida.Etapa);
                        cmdMap.Parameters.AddWithValue("@partida", partida.Partida);
                        cmdMap.ExecuteNonQuery();
                    }
                    
                    partida.WBS = wbsActual;
                    wbsUsados.Add(wbsActual);
                    partidasNuevas++;
                    
                    Debug.WriteLine($"  ? Nueva partida: '{partida.Partida}' -> WBS: {wbsActual}");
                }
                else
                {
                    string sqlInsert = tieneVinculoPermanente
                        ? @"INSERT INTO #MapeoWBS (IdPresupuesto, WBS_Viejo, WBS_Temporal, WBS_Nuevo, Etapa, Partida, EsNueva)
                            VALUES (@idPresup, @viejo, @temporal, @nuevo, @etapa, @partida, 0)"
                        : @"INSERT INTO #MapeoWBS (WBS_Viejo, WBS_Temporal, WBS_Nuevo, Etapa, Partida, EsNueva)
                            VALUES (@viejo, @temporal, @nuevo, @etapa, @partida, 0)";
                    
                    using (SqlCommand cmdMap = new SqlCommand(sqlInsert, conn, transaction))
                    {
                        if (tieneVinculoPermanente)
                        {
                            cmdMap.Parameters.AddWithValue("@idPresup", idPresupuesto > 0 ? idPresupuesto : (object)DBNull.Value);
                        }
                        cmdMap.Parameters.AddWithValue("@viejo", wbsOriginalReal);
                        cmdMap.Parameters.AddWithValue("@temporal", esTemporal ? wbsActualPartida : (object)DBNull.Value);
                        cmdMap.Parameters.AddWithValue("@nuevo", wbsActual);
                        cmdMap.Parameters.AddWithValue("@etapa", partida.Etapa);
                        cmdMap.Parameters.AddWithValue("@partida", partida.Partida);
                        cmdMap.ExecuteNonQuery();
                    }
                    
                    Debug.WriteLine($"  ? Existente: '{partida.Partida}' WBS {wbsOriginalReal} ? temp {wbsActualPartida} ? final {wbsActual}" +
                        (tieneVinculoPermanente ? $" (Id: {idPresupuesto})" : ""));
                    
                    partida.WBS = wbsActual;
                    wbsUsados.Add(wbsActual);
                    partidasExistentes++;
                }
                
                wbsActual++;
            }
            
            Debug.WriteLine($"?? Mapeo completo: {partidasExistentes} existentes, {partidasNuevas} nuevas");
            
            return new ResultadoAsignacionWBS
            {
                PartidasExistentes = partidasExistentes,
                PartidasNuevas = partidasNuevas,
                WBSFinal = wbsActual - 1
            };
        }
        
        /// <summary>
        /// Actualiza WBS en AvanceManualObra usando IdPresupuestoObra como referencia (método más seguro)
        /// </summary>
        private void ActualizarWBSUsandoIdPresupuesto(SqlConnection conn, SqlTransaction transaction, bool wbsEsTexto)
        {
            bool existeTabla = false;
            using (SqlCommand cmdCheckTable = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'AvanceManualObra'", conn, transaction))
            {
                existeTabla = (int)cmdCheckTable.ExecuteScalar() > 0;
            }
            
            if (!existeTabla)
                return;
            
            // Verificar columna WBS en AvanceManualObra
            bool tieneWBSCorrecto = false;
            using (SqlCommand cmdCheckWBSCol = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' 
                AND COLUMN_NAME = 'WBS_Correcto'", conn, transaction))
            {
                tieneWBSCorrecto = (int)cmdCheckWBSCol.ExecuteScalar() > 0;
            }
            
            string nombreColumnaWBS = tieneWBSCorrecto ? "WBS_Correcto" : "WBS";
            
            // Detectar tipo de WBS en AvanceManualObra
            string tipoWBSAvance = "INT";
            using (SqlCommand cmdTipoAvance = new SqlCommand($@"
                SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = '{nombreColumnaWBS}'", conn, transaction))
            {
                var result = cmdTipoAvance.ExecuteScalar();
                if (result != null) tipoWBSAvance = result.ToString().ToUpper();
            }
            bool wbsAvanceEsTexto = tipoWBSAvance.Contains("VARCHAR") || tipoWBSAvance.Contains("CHAR");
            
            Debug.WriteLine($"?? Actualizando columna '{nombreColumnaWBS}' en AvanceManualObra usando IdPresupuestoObra");
            
            // Actualizar WBS en AvanceManualObra usando IdPresupuestoObra como JOIN
            // Esto es MÁS SEGURO porque IdPresupuestoObra nunca cambia
            string sqlUpdateAvance;
            if (wbsAvanceEsTexto)
            {
                sqlUpdateAvance = $@"
                    UPDATE amo
                    SET amo.{nombreColumnaWBS} = CAST(m.WBS_Nuevo AS NVARCHAR(50))
                    FROM AvanceManualObra amo
                    INNER JOIN #MapeoWBS m ON amo.IdPresupuestoObra = m.IdPresupuesto
                    WHERE m.EsNueva = 0 AND m.IdPresupuesto IS NOT NULL";
            }
            else
            {
                sqlUpdateAvance = $@"
                    UPDATE amo
                    SET amo.{nombreColumnaWBS} = m.WBS_Nuevo
                    FROM AvanceManualObra amo
                    INNER JOIN #MapeoWBS m ON amo.IdPresupuestoObra = m.IdPresupuesto
                    WHERE m.EsNueva = 0 AND m.IdPresupuesto IS NOT NULL";
            }
            
            using (SqlCommand cmdAvance = new SqlCommand(sqlUpdateAvance, conn, transaction))
            {
                int filasAvance = cmdAvance.ExecuteNonQuery();
                Debug.WriteLine($"? Actualizado {nombreColumnaWBS} en {filasAvance} registros de AvanceManualObra (vía IdPresupuestoObra)");
            }
        }
        
        private void ActualizarWBSEnAvanceManualObra(SqlConnection conn, SqlTransaction transaction, bool wbsEsTexto)
        {
            bool existeTabla = false;
            using (SqlCommand cmdCheckTable = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'AvanceManualObra'", conn, transaction))
            {
                existeTabla = (int)cmdCheckTable.ExecuteScalar() > 0;
            }
            
            if (!existeTabla)
                return;
            
            bool tieneWBSCorrecto = false;
            using (SqlCommand cmdCheckWBSCol = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' 
                AND COLUMN_NAME = 'WBS_Correcto'", conn, transaction))
            {
                tieneWBSCorrecto = (int)cmdCheckWBSCol.ExecuteScalar() > 0;
            }
            
            string nombreColumnaWBS = tieneWBSCorrecto ? "WBS_Correcto" : "WBS";
            
            // Detectar tipo de WBS en AvanceManualObra
            string tipoWBSAvance = "INT";
            using (SqlCommand cmdTipoAvance = new SqlCommand($@"
                SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = '{nombreColumnaWBS}'", conn, transaction))
            {
                var result = cmdTipoAvance.ExecuteScalar();
                if (result != null) tipoWBSAvance = result.ToString().ToUpper();
            }
            bool wbsAvanceEsTexto = tipoWBSAvance.Contains("VARCHAR") || tipoWBSAvance.Contains("CHAR");
            
            Debug.WriteLine($"?? Actualizando columna '{nombreColumnaWBS}' en AvanceManualObra (tipo: {tipoWBSAvance})");
            
            // Actualizar de WBS temporal a WBS nuevo (para partidas existentes)
            string sqlUpdateAvance;
            if (wbsAvanceEsTexto)
            {
                sqlUpdateAvance = $@"
                    UPDATE amo
                    SET amo.{nombreColumnaWBS} = CAST(m.WBS_Nuevo AS NVARCHAR(50))
                    FROM AvanceManualObra amo
                    INNER JOIN #MapeoWBS m ON CAST(amo.{nombreColumnaWBS} AS INT) = m.WBS_Temporal
                    WHERE m.EsNueva = 0 AND m.WBS_Temporal IS NOT NULL";
            }
            else
            {
                sqlUpdateAvance = $@"
                    UPDATE amo
                    SET amo.{nombreColumnaWBS} = m.WBS_Nuevo
                    FROM AvanceManualObra amo
                    INNER JOIN #MapeoWBS m ON amo.{nombreColumnaWBS} = m.WBS_Temporal
                    WHERE m.EsNueva = 0 AND m.WBS_Temporal IS NOT NULL";
            }
            
            using (SqlCommand cmdAvance = new SqlCommand(sqlUpdateAvance, conn, transaction))
            {
                int filasAvance = cmdAvance.ExecuteNonQuery();
                Debug.WriteLine($"? Actualizado {nombreColumnaWBS} en {filasAvance} registros de AvanceManualObra");
            }
        }
        
        private void ActualizarWBSEnPresupuestoObra(SqlConnection conn, SqlTransaction transaction, bool wbsEsTexto, bool tieneVinculoPermanente)
        {
            string sqlUpdatePresup;
            
            if (tieneVinculoPermanente)
            {
                // Usar Id para el JOIN (más seguro)
                if (wbsEsTexto)
                {
                    sqlUpdatePresup = @"
                        UPDATE p
                        SET p.WBS_Correcto = CAST(m.WBS_Nuevo AS NVARCHAR(50))
                        FROM PresupuestoObra p
                        INNER JOIN #MapeoWBS m ON p.Id = m.IdPresupuesto
                        WHERE m.EsNueva = 0 AND m.IdPresupuesto IS NOT NULL";
                }
                else
                {
                    sqlUpdatePresup = @"
                        UPDATE p
                        SET p.WBS_Correcto = m.WBS_Nuevo
                        FROM PresupuestoObra p
                        INNER JOIN #MapeoWBS m ON p.Id = m.IdPresupuesto
                        WHERE m.EsNueva = 0 AND m.IdPresupuesto IS NOT NULL";
                }
            }
            else
            {
                // Usar WBS_Temporal para el JOIN (método legacy)
                if (wbsEsTexto)
                {
                    sqlUpdatePresup = @"
                        UPDATE p
                        SET p.WBS_Correcto = CAST(m.WBS_Nuevo AS NVARCHAR(50))
                        FROM PresupuestoObra p
                        INNER JOIN #MapeoWBS m ON CAST(p.WBS_Correcto AS INT) = m.WBS_Temporal
                        WHERE m.EsNueva = 0 AND m.WBS_Temporal IS NOT NULL";
                }
                else
                {
                    sqlUpdatePresup = @"
                        UPDATE p
                        SET p.WBS_Correcto = m.WBS_Nuevo
                        FROM PresupuestoObra p
                        INNER JOIN #MapeoWBS m ON p.WBS_Correcto = m.WBS_Temporal
                        WHERE m.EsNueva = 0 AND m.WBS_Temporal IS NOT NULL";
                }
            }
            
            using (SqlCommand cmdPresup = new SqlCommand(sqlUpdatePresup, conn, transaction))
            {
                int filasPresup = cmdPresup.ExecuteNonQuery();
                Debug.WriteLine($"? Actualizado WBS en {filasPresup} registros de PresupuestoObra" +
                    (tieneVinculoPermanente ? " (vía Id)" : ""));
            }
        }
        
        #endregion
    }
}
