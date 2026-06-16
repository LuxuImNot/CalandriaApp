using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// FormGestionarPartidas - Guardado de cambios en la base de datos
    /// </summary>
    public partial class FormGestionarPartidas
    {
        #region Guardado de cambios
        
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (conceptoSeleccionado == null)
            {
                MessageBox.Show(
                    "Selecciona un concepto primero.",
                    "Atención",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Verificar si se reordenaron partidas
            bool fueronReordenadas = RequiereReorganizacionPorReordenamiento();
            
            var result = MessageBox.Show(
                $"¿Deseas guardar los cambios en las partidas del concepto?\n\n" +
                $"Concepto: [{conceptoSeleccionado.Codigo}] {conceptoSeleccionado.Nombre}\n" +
                $"Total partidas: {partidasConcepto.Count}\n\n" +
                $"? Los cambios incluirán:\n" +
                $"   • Actualización de nombres (Etapa/Partida)\n" +
                $"   • Actualización de costos y límites por prototipo\n" +
                $"   • Actualización de prototipos aplicables\n" +
                $"   • Nuevas partidas agregadas\n" +
                $"   • Partidas eliminadas\n" +
                (fueronReordenadas ? $"   • ? Reordenamiento de partidas (WBS se reorganizará)\n" : "") +
                $"\n? El progreso/avance de las obras se preservará usando WBS_Correcto.",
                "Confirmar Guardado",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Verificar columnas existentes
                    var columnasExistentes = ObtenerColumnasExistentes(conn);
                    bool tieneWBS = columnasExistentes.Contains("WBS_Correcto");
                    bool tieneCamposDinamicos = columnasExistentes.Contains("EsDinamica") &&
                                               columnasExistentes.Contains("ValorM2Tunera") &&
                                               columnasExistentes.Contains("ValorM2Calandra");
                    bool tieneLimiteM2 = columnasExistentes.Contains("LimiteM2");
                    bool tieneLimitesPrototipo = columnasExistentes.Contains("LimiteM2Tunera") &&
                                                 columnasExistentes.Contains("LimiteM2Calandra");
                    bool tienePrototipos = columnasExistentes.Contains("PrototiposAplicables");
                    
                    Debug.WriteLine($"?? Tabla PresupuestoObra tiene WBS_Correcto: {tieneWBS}");
                    Debug.WriteLine($"?? Tabla PresupuestoObra tiene LimitesPrototipo: {tieneLimitesPrototipo}");
                    Debug.WriteLine($"?? Partidas fueron reordenadas: {fueronReordenadas}");

                    // Cargar partidas actuales de la BD (sin transacción, antes de iniciarla)
                    var partidasActualesBD = CargarPartidasDeBD(conn, null, tieneWBS, tieneCamposDinamicos, 
                        tieneLimiteM2, tieneLimitesPrototipo, tienePrototipos);
                    
                    Debug.WriteLine($"?? Partidas en BD: {partidasActualesBD.Count}");
                    Debug.WriteLine($"?? Partidas en memoria: {partidasConcepto.Count}");
                    
                    // Guardar WBS originales
                    var wbsOriginalesEnMemoria = partidasConcepto.ToDictionary(p => p, p => p.WBS);
                    
                    // Identificar partidas eliminadas
                    var wbsEnMemoria = new HashSet<int>(partidasConcepto.Where(p => p.WBS > 0 && p.WBS < 999000).Select(p => p.WBS));
                    var partidasAEliminar = partidasActualesBD.Where(p => p.WBS > 0 && !wbsEnMemoria.Contains(p.WBS)).ToList();
                    
                    Debug.WriteLine($"??? Partidas a eliminar: {partidasAEliminar.Count}");
                    
                    // Detectar si hay partidas nuevas y si necesita reorganización
                    bool hayPartidasNuevas = partidasConcepto.Any(p => p.WBS == 0);
                    bool reorganizarWBS = false;
                    
                    // Si las partidas fueron reordenadas, forzar reorganización de WBS
                    if (tieneWBS && fueronReordenadas)
                    {
                        var confirmReorg = MessageBox.Show(
                            "? Se detectó que reordenaste partidas.\n\n" +
                            "Para que el nuevo orden se refleje correctamente,\n" +
                            "es necesario reorganizar el WBS.\n\n" +
                            "¿Deseas reorganizar el WBS con el nuevo orden?",
                            "Reorganizar WBS por Reordenamiento",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        
                        reorganizarWBS = (confirmReorg == DialogResult.Yes);
                        
                        if (!reorganizarWBS)
                        {
                            MessageBox.Show(
                                "? Sin reorganización de WBS, el orden visual de las partidas\n" +
                                "podría no coincidir con el orden guardado en la base de datos.\n\n" +
                                "Se guardarán los demás cambios (costos, nombres, etc.)",
                                "Advertencia",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                    else if (tieneWBS && hayPartidasNuevas)
                    {
                        reorganizarWBS = PreguntarReorganizacionWBS();
                    }

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            int actualizadas = 0;
                            int agregadas = 0;
                            int eliminadas = 0;
                            
                            // Marcar partidas nuevas
                            var partidasNuevasMarcadas = new HashSet<PartidaConcepto>(
                                partidasConcepto.Where(p => p.WBS == 0).ToList()
                            );
                            
                            // 1. Eliminar partidas
                            eliminadas = EliminarPartidas(conn, transaction, tieneWBS, partidasAEliminar, partidasActualesBD);
                            
                            // 2. Reorganizar WBS si es necesario
                            if (reorganizarWBS && tieneWBS)
                            {
                                Debug.WriteLine("?? Iniciando reorganización WBS...");
                                
                                int wbsTemp = 999000;
                                foreach (var partidaNueva in partidasNuevasMarcadas)
                                {
                                    partidaNueva.WBS = wbsTemp++;
                                }
                                
                                if (!ReorganizarWBSConcepto(conn, transaction))
                                {
                                    transaction.Rollback();
                                    return;
                                }
                            }
                            
                            // 3. Actualizar partidas existentes
                            actualizadas = ActualizarPartidasExistentes(
                                conn, transaction, tieneWBS, tieneCamposDinamicos, 
                                tieneLimiteM2, tieneLimitesPrototipo, tienePrototipos, 
                                partidasNuevasMarcadas, partidasActualesBD);
                            
                            // 4. Agregar partidas nuevas
                            agregadas = AgregarPartidasNuevas(
                                conn, transaction, columnasExistentes, tieneWBS, 
                                tieneCamposDinamicos, tieneLimiteM2, tieneLimitesPrototipo, tienePrototipos, 
                                partidasNuevasMarcadas);
                            
                            // 5. Actualizar tabla Estimacion(Concepto)
                            ActualizarTablaEstimacion(conn, transaction);
                            
                            transaction.Commit();
                            
                            // Resetear indicador de reordenamiento después de guardar exitosamente
                            ResetearIndicadorReordenamiento();
                            
                            MostrarMensajeExitoGuardado(tieneWBS, reorganizarWBS, actualizadas, agregadas, eliminadas, fueronReordenadas);
                            
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Debug.WriteLine($"? Error durante guardado: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar cambios:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        #endregion
        
        #region Métodos auxiliares de guardado
        
        private HashSet<string> ObtenerColumnasExistentes(SqlConnection conn)
        {
            var columnasExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (SqlCommand cmdCheck = new SqlCommand(@"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'PresupuestoObra'", conn))
            using (SqlDataReader reader = cmdCheck.ExecuteReader())
            {
                while (reader.Read())
                {
                    columnasExistentes.Add(reader.GetString(0));
                }
            }
            return columnasExistentes;
        }
        
        /// <summary>
        /// Carga las partidas de la BD con soporte para transacciones
        /// </summary>
        private List<PartidaConcepto> CargarPartidasDeBD(
            SqlConnection conn, SqlTransaction transaction, bool tieneWBS, bool tieneCamposDinamicos, 
            bool tieneLimiteM2, bool tieneLimitesPrototipo, bool tienePrototipos)
        {
            var partidasActualesBD = new List<PartidaConcepto>();
            
            string sqlLoad = tieneWBS
                ? @"SELECT WBS_Correcto, Etapa, Partida, 
                   ISNULL(CostoTunera, 0) AS CostoTunera,
                   ISNULL(CostoCalandra, 0) AS CostoCalandra"
                           + (tieneCamposDinamicos ? @",
                   ISNULL(EsDinamica, 0) AS EsDinamica,
                   ISNULL(ValorM2Tunera, 0) AS ValorM2Tunera,
                   ISNULL(ValorM2Calandra, 0) AS ValorM2Calandra" : "")
                           + (tieneLimiteM2 ? @",
                   ISNULL(LimiteM2, 0) AS LimiteM2" : "")
                           + (tieneLimitesPrototipo ? @",
                   ISNULL(LimiteM2Tunera, 0) AS LimiteM2Tunera,
                   ISNULL(LimiteM2Calandra, 0) AS LimiteM2Calandra" : "")
                           + (tienePrototipos ? @",
                   PrototiposAplicables" : "") + @"
            FROM PresupuestoObra
            WHERE Codigo = @codigo"
                : @"SELECT Etapa, Partida, 
                   ISNULL(CostoTunera, 0) AS CostoTunera,
                   ISNULL(CostoCalandra, 0) AS CostoCalandra"
                           + (tieneCamposDinamicos ? @",
                   ISNULL(EsDinamica, 0) AS EsDinamica,
                   ISNULL(ValorM2Tunera, 0) AS ValorM2Tunera,
                   ISNULL(ValorM2Calandra, 0) AS ValorM2Calandra" : "")
                           + (tieneLimiteM2 ? @",
                   ISNULL(LimiteM2, 0) AS LimiteM2" : "")
                           + (tieneLimitesPrototipo ? @",
                   ISNULL(LimiteM2Tunera, 0) AS LimiteM2Tunera,
                   ISNULL(LimiteM2Calandra, 0) AS LimiteM2Calandra" : "")
                           + (tienePrototipos ? @",
                   PrototiposAplicables" : "") + @"
            FROM PresupuestoObra
            WHERE Codigo = @codigo";
            
            using (SqlCommand cmdLoad = new SqlCommand(sqlLoad, conn, transaction))
            {
                cmdLoad.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                using (SqlDataReader reader = cmdLoad.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var partida = new PartidaConcepto
                        {
                            Etapa = reader["Etapa"]?.ToString() ?? "",
                            Partida = reader["Partida"]?.ToString() ?? "",
                            CostoTunera = Convert.ToDouble(reader["CostoTunera"]),
                            CostoCalandra = Convert.ToDouble(reader["CostoCalandra"])
                        };
                        
                        if (tieneWBS && !reader.IsDBNull(reader.GetOrdinal("WBS_Correcto")))
                        {
                            partida.WBS = Convert.ToInt32(reader["WBS_Correcto"]);
                        }
                        
                        if (tieneCamposDinamicos)
                        {
                            partida.EsDinamica = Convert.ToBoolean(reader["EsDinamica"]);
                            partida.ValorM2Tunera = Convert.ToDouble(reader["ValorM2Tunera"]);
                            partida.ValorM2Calandra = Convert.ToDouble(reader["ValorM2Calandra"]);
                        }
                        
                        if (tieneLimiteM2)
                        {
                            partida.LimiteM2 = Convert.ToDouble(reader["LimiteM2"]);
                        }
                        
                        if (tieneLimitesPrototipo)
                        {
                            partida.LimiteM2Tunera = Convert.ToDouble(reader["LimiteM2Tunera"]);
                            partida.LimiteM2Calandra = Convert.ToDouble(reader["LimiteM2Calandra"]);
                        }
                        
                        if (tienePrototipos && !reader.IsDBNull(reader.GetOrdinal("PrototiposAplicables")))
                        {
                            partida.PrototiposAplicables = reader["PrototiposAplicables"]?.ToString();
                        }
                        
                        partidasActualesBD.Add(partida);
                    }
                }
            }
            
            return partidasActualesBD;
        }
        
        private bool PreguntarReorganizacionWBS()
        {
            var partidasConWBS = partidasConcepto.Where(p => p.WBS > 0).ToList();
            bool necesitaReorganizacion = false;
            
            for (int i = 0; i < partidasConWBS.Count - 1; i++)
            {
                if (partidasConWBS[i].WBS > partidasConWBS[i + 1].WBS)
                {
                    necesitaReorganizacion = true;
                    break;
                }
            }
            
            string mensajeReorganizacion = necesitaReorganizacion
                ? "? DETECCIÓN AUTOMÁTICA:\n\n" +
                  "El sistema detectó que las partidas no están en orden secuencial.\n" +
                  "Se recomienda reorganizar el WBS para mantener la coherencia.\n\n"
                : "?? HAY PARTIDAS NUEVAS:\n\n";
            
            mensajeReorganizacion +=
                "?? Opciones de Guardado:\n\n" +
                "• SÍ: Reorganizar WBS secuencialmente (RECOMENDADO)\n" +
                "     ? Mantiene orden lógico de partidas\n" +
                "     ? WBS consecutivos sin huecos\n" +
                "     ? Preserva el progreso de obras\n" +
                "     ? Proceso más lento pero seguro\n\n" +
                "• NO: Mantener WBS original\n" +
                "     ? Las nuevas partidas se agregarán al final\n" +
                "     ? Puede haber huecos en numeración WBS\n" +
                "     ? Proceso más rápido\n\n" +
                "¿Deseas reorganizar el WBS?";
            
            var reorganizarResult = MessageBox.Show(
                mensajeReorganizacion,
                necesitaReorganizacion ? "Reorganización Recomendada" : "Reorganizar WBS",
                MessageBoxButtons.YesNo,
                necesitaReorganizacion ? MessageBoxIcon.Warning : MessageBoxIcon.Question);
            
            return reorganizarResult == DialogResult.Yes;
        }
        
        private int EliminarPartidas(
            SqlConnection conn, SqlTransaction transaction, bool tieneWBS,
            List<PartidaConcepto> partidasAEliminar, List<PartidaConcepto> partidasActualesBD)
        {
            int eliminadas = 0;
            
            if (tieneWBS)
            {
                foreach (var partidaAEliminar in partidasAEliminar)
                {
                    using (SqlCommand cmdDelete = new SqlCommand(
                        "DELETE FROM PresupuestoObra WHERE WBS_Correcto = @wbs AND Codigo = @codigo", conn, transaction))
                    {
                        cmdDelete.Parameters.AddWithValue("@wbs", partidaAEliminar.WBS);
                        cmdDelete.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                        int rowsAffected = cmdDelete.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            eliminadas++;
                            Debug.WriteLine($"   ? Eliminada WBS: {partidaAEliminar.WBS} - '{partidaAEliminar.Partida}'");
                        }
                    }
                }
            }
            else
            {
                // Modo sin WBS: eliminar por Etapa+Partida
                foreach (var partidaVieja in partidasActualesBD)
                {
                    if (!partidasConcepto.Any(p =>
                        p.Etapa.Equals(partidaVieja.Etapa, StringComparison.OrdinalIgnoreCase) &&
                        p.Partida.Equals(partidaVieja.Partida, StringComparison.OrdinalIgnoreCase)))
                    {
                        using (SqlCommand cmdDelete = new SqlCommand(@"
                            DELETE FROM PresupuestoObra 
                            WHERE Codigo = @codigo AND Etapa = @etapa AND Partida = @partida", conn, transaction))
                        {
                            cmdDelete.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                            cmdDelete.Parameters.AddWithValue("@etapa", partidaVieja.Etapa);
                            cmdDelete.Parameters.AddWithValue("@partida", partidaVieja.Partida);
                            int rowsAffected = cmdDelete.ExecuteNonQuery();
                            if (rowsAffected > 0)
                                eliminadas++;
                        }
                    }
                }
            }
            
            Debug.WriteLine($"??? Total eliminadas: {eliminadas}");
            return eliminadas;
        }
        
        private int ActualizarPartidasExistentes(
            SqlConnection conn, SqlTransaction transaction, bool tieneWBS,
            bool tieneCamposDinamicos, bool tieneLimiteM2, bool tieneLimitesPrototipo, bool tienePrototipos,
            HashSet<PartidaConcepto> partidasNuevasMarcadas, List<PartidaConcepto> partidasActualesBD)
        {
            int actualizadas = 0;
            
            if (tieneWBS)
            {
                Debug.WriteLine("?? Modo con WBS_Correcto: actualizando partidas...");
                
                // Recargar partidas de BD después de eliminar (con transacción)
                var partidasActualizadasBD = CargarPartidasDeBD(
                    conn, transaction, tieneWBS, tieneCamposDinamicos, tieneLimiteM2, tieneLimitesPrototipo, tienePrototipos);
                
                foreach (var partidaEnMemoria in partidasConcepto.Where(p => p.WBS > 0 && p.WBS < 999000))
                {
                    if (partidasNuevasMarcadas.Contains(partidaEnMemoria))
                        continue;
                    
                    var partidaExistente = partidasActualizadasBD.FirstOrDefault(p => p.WBS == partidaEnMemoria.WBS);
                    
                    if (partidaExistente != null && HayCambiosEnPartida(
                        partidaExistente, partidaEnMemoria, tieneCamposDinamicos, tieneLimiteM2, tieneLimitesPrototipo, tienePrototipos))
                    {
                        ActualizarPartidaEnBD(conn, transaction, partidaEnMemoria, 
                            tieneCamposDinamicos, tieneLimiteM2, tieneLimitesPrototipo, tienePrototipos, true);
                        actualizadas++;
                    }
                }
            }
            else
            {
                Debug.WriteLine("?? Modo sin WBS_Correcto: solo actualizando costos...");
                
                foreach (var partidaEnMemoria in partidasConcepto)
                {
                    if (partidasNuevasMarcadas.Contains(partidaEnMemoria))
                        continue;
                    
                    var partidaExistente = partidasActualesBD.FirstOrDefault(p =>
                        p.Etapa.Equals(partidaEnMemoria.Etapa, StringComparison.OrdinalIgnoreCase) &&
                        p.Partida.Equals(partidaEnMemoria.Partida, StringComparison.OrdinalIgnoreCase));
                    
                    if (partidaExistente != null)
                    {
                        bool cambioCosto = Math.Abs(partidaExistente.CostoTunera - partidaEnMemoria.CostoTunera) > 0.01 ||
                                          Math.Abs(partidaExistente.CostoCalandra - partidaEnMemoria.CostoCalandra) > 0.01;
                        
                        bool cambioPrototipos = tienePrototipos && 
                            !(partidaExistente.PrototiposAplicables ?? "")
                                .Equals(partidaEnMemoria.PrototiposAplicables ?? "", StringComparison.OrdinalIgnoreCase);
                        
                        if (cambioCosto || cambioPrototipos)
                        {
                            ActualizarPartidaEnBD(conn, transaction, partidaEnMemoria, 
                                tieneCamposDinamicos, tieneLimiteM2, tieneLimitesPrototipo, tienePrototipos, false);
                            actualizadas++;
                        }
                    }
                }
            }
            
            return actualizadas;
        }
        
        private bool HayCambiosEnPartida(
            PartidaConcepto partidaExistente, PartidaConcepto partidaEnMemoria,
            bool tieneCamposDinamicos, bool tieneLimiteM2, bool tieneLimitesPrototipo, bool tienePrototipos)
        {
            bool cambioNombre = !partidaExistente.Etapa.Equals(partidaEnMemoria.Etapa, StringComparison.OrdinalIgnoreCase) ||
                               !partidaExistente.Partida.Equals(partidaEnMemoria.Partida, StringComparison.OrdinalIgnoreCase);
            
            bool cambioCosto = Math.Abs(partidaExistente.CostoTunera - partidaEnMemoria.CostoTunera) > 0.01 ||
                              Math.Abs(partidaExistente.CostoCalandra - partidaEnMemoria.CostoCalandra) > 0.01;
            
            bool cambioDinamico = tieneCamposDinamicos && (
                partidaExistente.EsDinamica != partidaEnMemoria.EsDinamica ||
                Math.Abs(partidaExistente.ValorM2Tunera - partidaEnMemoria.ValorM2Tunera) > 0.01 ||
                Math.Abs(partidaExistente.ValorM2Calandra - partidaEnMemoria.ValorM2Calandra) > 0.01);
            
            bool cambioLimiteM2 = tieneLimiteM2 && 
                Math.Abs(partidaExistente.LimiteM2 - partidaEnMemoria.LimiteM2) > 0.01;
            
            bool cambioLimitesPrototipo = tieneLimitesPrototipo && (
                Math.Abs(partidaExistente.LimiteM2Tunera - partidaEnMemoria.LimiteM2Tunera) > 0.01 ||
                Math.Abs(partidaExistente.LimiteM2Calandra - partidaEnMemoria.LimiteM2Calandra) > 0.01);
            
            bool cambioPrototipos = tienePrototipos && 
                !(partidaExistente.PrototiposAplicables ?? "")
                    .Equals(partidaEnMemoria.PrototiposAplicables ?? "", StringComparison.OrdinalIgnoreCase);
            
            return cambioNombre || cambioCosto || cambioDinamico || cambioLimiteM2 || cambioLimitesPrototipo || cambioPrototipos;
        }
        
        private void ActualizarPartidaEnBD(
            SqlConnection conn, SqlTransaction transaction, PartidaConcepto partida,
            bool tieneCamposDinamicos, bool tieneLimiteM2, bool tieneLimitesPrototipo, bool tienePrototipos, bool usarWBS)
        {
            string sqlUpdate;
            
            if (usarWBS)
            {
                sqlUpdate = @"
                    UPDATE PresupuestoObra
                    SET Etapa = @etapa,
                        Partida = @partida,
                        CostoTunera = @totalTunera,
                        CostoCalandra = @totalCalandra";
                
                if (tieneCamposDinamicos)
                {
                    sqlUpdate += @",
                        EsDinamica = @esDinamica,
                        ValorM2Tunera = @valorM2Tunera,
                        ValorM2Calandra = @valorM2Calandra";
                }
                
                if (tieneLimiteM2)
                {
                    sqlUpdate += @",
                        LimiteM2 = @limiteM2";
                }
                
                if (tieneLimitesPrototipo)
                {
                    sqlUpdate += @",
                        LimiteM2Tunera = @limiteM2Tunera,
                        LimiteM2Calandra = @limiteM2Calandra";
                }
                
                if (tienePrototipos)
                {
                    sqlUpdate += @",
                        PrototiposAplicables = @prototiposAplicables";
                }
                
                sqlUpdate += " WHERE WBS_Correcto = @wbs";
            }
            else
            {
                sqlUpdate = @"
                    UPDATE PresupuestoObra
                    SET CostoTunera = @totalTunera,
                        CostoCalandra = @totalCalandra";
                
                if (tienePrototipos)
                {
                    sqlUpdate += @",
                        PrototiposAplicables = @prototiposAplicables";
                }
                
                sqlUpdate += " WHERE Codigo = @codigo AND Etapa = @etapa AND Partida = @partida";
            }
            
            using (SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn, transaction))
            {
                if (usarWBS)
                {
                    cmdUpdate.Parameters.AddWithValue("@wbs", partida.WBS);
                }
                else
                {
                    cmdUpdate.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                }
                
                cmdUpdate.Parameters.AddWithValue("@etapa", partida.Etapa);
                cmdUpdate.Parameters.AddWithValue("@partida", partida.Partida);
                cmdUpdate.Parameters.AddWithValue("@totalTunera", partida.CostoTunera);
                cmdUpdate.Parameters.AddWithValue("@totalCalandra", partida.CostoCalandra);
                
                if (tieneCamposDinamicos && usarWBS)
                {
                    cmdUpdate.Parameters.AddWithValue("@esDinamica", partida.EsDinamica);
                    cmdUpdate.Parameters.AddWithValue("@valorM2Tunera", partida.ValorM2Tunera);
                    cmdUpdate.Parameters.AddWithValue("@valorM2Calandra", partida.ValorM2Calandra);
                }
                
                if (tieneLimiteM2 && usarWBS)
                {
                    cmdUpdate.Parameters.AddWithValue("@limiteM2", partida.LimiteM2);
                }
                
                if (tieneLimitesPrototipo && usarWBS)
                {
                    cmdUpdate.Parameters.AddWithValue("@limiteM2Tunera", partida.LimiteM2Tunera);
                    cmdUpdate.Parameters.AddWithValue("@limiteM2Calandra", partida.LimiteM2Calandra);
                }
                
                if (tienePrototipos)
                {
                    cmdUpdate.Parameters.AddWithValue("@prototiposAplicables",
                        (object)partida.PrototiposAplicables ?? DBNull.Value);
                }
                
                cmdUpdate.ExecuteNonQuery();
            }
        }
        
        private int AgregarPartidasNuevas(
            SqlConnection conn, SqlTransaction transaction, HashSet<string> columnasExistentes,
            bool tieneWBS, bool tieneCamposDinamicos, bool tieneLimiteM2, bool tieneLimitesPrototipo, bool tienePrototipos,
            HashSet<PartidaConcepto> partidasNuevasMarcadas)
        {
            int agregadas = 0;
            Debug.WriteLine($"? Insertando {partidasNuevasMarcadas.Count} partidas nuevas...");
            
            foreach (var partidaNueva in partidasNuevasMarcadas)
            {
                var columnas = new List<string> { "Codigo", "Padre", "Etapa", "Partida", "CostoTunera", "CostoCalandra" };
                var parametros = new List<string> { "@codigo", "@padre", "@etapa", "@partida", "@tunera", "@calandra" };
                
                if (columnasExistentes.Contains("Concepto"))
                {
                    columnas.Insert(1, "Concepto");
                    parametros.Insert(1, "@concepto");
                }
                
                if (tieneWBS && partidaNueva.WBS > 0)
                {
                    columnas.Add("WBS_Correcto");
                    parametros.Add("@wbs_correcto");
                }
                
                if (tieneCamposDinamicos)
                {
                    columnas.Add("EsDinamica");
                    columnas.Add("ValorM2Tunera");
                    columnas.Add("ValorM2Calandra");
                    parametros.Add("@esDinamica");
                    parametros.Add("@valorM2Tunera");
                    parametros.Add("@valorM2Calandra");
                }
                
                if (tieneLimiteM2)
                {
                    columnas.Add("LimiteM2");
                    parametros.Add("@limiteM2");
                }
                
                if (tieneLimitesPrototipo)
                {
                    columnas.Add("LimiteM2Tunera");
                    columnas.Add("LimiteM2Calandra");
                    parametros.Add("@limiteM2Tunera");
                    parametros.Add("@limiteM2Calandra");
                }
                
                if (tienePrototipos)
                {
                    columnas.Add("PrototiposAplicables");
                    parametros.Add("@prototiposAplicables");
                }
                
                string sqlInsert = $@"
                    INSERT INTO PresupuestoObra 
                    ({string.Join(", ", columnas)})
                    VALUES ({string.Join(", ", parametros)})";
                
                using (SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn, transaction))
                {
                    cmdInsert.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                    cmdInsert.Parameters.AddWithValue("@padre", conceptoSeleccionado.Nombre);
                    cmdInsert.Parameters.AddWithValue("@etapa", partidaNueva.Etapa);
                    cmdInsert.Parameters.AddWithValue("@partida", partidaNueva.Partida);
                    cmdInsert.Parameters.AddWithValue("@tunera", partidaNueva.CostoTunera);
                    cmdInsert.Parameters.AddWithValue("@calandra", partidaNueva.CostoCalandra);
                    
                    if (columnasExistentes.Contains("Concepto"))
                    {
                        cmdInsert.Parameters.AddWithValue("@concepto", conceptoSeleccionado.Nombre);
                    }
                    
                    if (tieneWBS && partidaNueva.WBS > 0)
                    {
                        cmdInsert.Parameters.AddWithValue("@wbs_correcto", partidaNueva.WBS);
                    }
                    
                    if (tieneCamposDinamicos)
                    {
                        cmdInsert.Parameters.AddWithValue("@esDinamica", partidaNueva.EsDinamica);
                        cmdInsert.Parameters.AddWithValue("@valorM2Tunera", partidaNueva.ValorM2Tunera);
                        cmdInsert.Parameters.AddWithValue("@valorM2Calandra", partidaNueva.ValorM2Calandra);
                    }
                    
                    if (tieneLimiteM2)
                    {
                        cmdInsert.Parameters.AddWithValue("@limiteM2", partidaNueva.LimiteM2);
                    }
                    
                    if (tieneLimitesPrototipo)
                    {
                        cmdInsert.Parameters.AddWithValue("@limiteM2Tunera", partidaNueva.LimiteM2Tunera);
                        cmdInsert.Parameters.AddWithValue("@limiteM2Calandra", partidaNueva.LimiteM2Calandra);
                    }
                    
                    if (tienePrototipos)
                    {
                        cmdInsert.Parameters.AddWithValue("@prototiposAplicables",
                            (object)partidaNueva.PrototiposAplicables ?? DBNull.Value);
                    }
                    
                    cmdInsert.ExecuteNonQuery();
                    agregadas++;
                }
            }
            
            return agregadas;
        }
        
        private void ActualizarTablaEstimacion(SqlConnection conn, SqlTransaction transaction)
        {
            // Verificar si existe la columna TOTAL
            bool tieneTOTAL = false;
            using (SqlCommand cmdCheck = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Estimacion(Concepto)' 
                AND COLUMN_NAME = 'TOTAL'", conn, transaction))
            {
                tieneTOTAL = (int)cmdCheck.ExecuteScalar() > 0;
            }
            
            string sqlUpdate = @"
                UPDATE [Estimacion(Concepto)]
                SET CostoTunera = @totalTunera,
                    CostoCalandra = @totalCalandra";
            
            if (tieneTOTAL)
            {
                sqlUpdate += ", TOTAL = @totalTunera";
            }
            
            sqlUpdate += " WHERE Codigo = @codigo AND Concepto = @concepto";
            
            double totalTunera = partidasConcepto.Sum(p => p.CostoTunera);
            double totalCalandra = partidasConcepto.Sum(p => p.CostoCalandra);
            
            using (SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn, transaction))
            {
                cmdUpdate.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                cmdUpdate.Parameters.AddWithValue("@concepto", conceptoSeleccionado.Nombre);
                cmdUpdate.Parameters.AddWithValue("@totalTunera", totalTunera);
                cmdUpdate.Parameters.AddWithValue("@totalCalandra", totalCalandra);
                
                cmdUpdate.ExecuteNonQuery();
            }
        }
        
        private void MostrarMensajeExitoGuardado(bool tieneWBS, bool reorganizarWBS, int actualizadas, int agregadas, int eliminadas, bool fueronReordenadas = false)
        {
            string mensaje = tieneWBS
                ? $"? Cambios guardados exitosamente usando WBS_Correcto\n\n" +
                  $"Concepto: [{conceptoSeleccionado.Codigo}] {conceptoSeleccionado.Nombre}\n\n" +
                  $"?? Resumen:\n" +
                  $"   • Partidas actualizadas: {actualizadas}\n" +
                  $"   • Partidas agregadas: {agregadas}\n" +
                  $"   • Partidas eliminadas: {eliminadas}\n" +
                  (reorganizarWBS ? $"   • WBS reorganizado: ?\n" : "") +
                  (fueronReordenadas && reorganizarWBS ? $"   • Nuevo orden aplicado: ?\n" : "")
                : $"? Cambios guardados exitosamente (modo compatible)\n\n" +
                  $"Concepto: [{conceptoSeleccionado.Codigo}] {conceptoSeleccionado.Nombre}\n\n" +
                  $"?? Resumen:\n" +
                  $"   • Partidas actualizadas: {actualizadas}\n" +
                  $"   • Partidas agregadas: {agregadas}\n" +
                  $"   • Partidas eliminadas: {eliminadas}";
            
            MessageBox.Show(mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        #endregion
    }
}
