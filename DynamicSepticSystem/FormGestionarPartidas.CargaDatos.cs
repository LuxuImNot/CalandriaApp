using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// FormGestionarPartidas - Carga de datos y actualización del grid
    /// </summary>
    public partial class FormGestionarPartidas
    {
        #region Carga de partidas
        
        private void CargarPartidasConcepto()
        {
            if (conceptoSeleccionado == null)
                return;
            
            partidasConcepto.Clear();
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Verificar si existe la columna WBS_Correcto
                    bool tieneWBS = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'PresupuestoObra' 
                        AND COLUMN_NAME = 'WBS_Correcto'", conn))
                    {
                        tieneWBS = (int)cmdCheck.ExecuteScalar() > 0;
                    }
                    
                    // Verificar si existen columnas de partida dinámica
                    bool tieneDinamica = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'PresupuestoObra' 
                        AND COLUMN_NAME IN ('EsDinamica', 'ValorM2Tunera', 'ValorM2Calandra')", conn))
                    {
                        tieneDinamica = (int)cmdCheck.ExecuteScalar() >= 3;
                    }
                    
                    // Verificar si existe columna LimiteM2 (legacy)
                    bool tieneLimiteM2 = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'PresupuestoObra' 
                        AND COLUMN_NAME = 'LimiteM2'", conn))
                    {
                        tieneLimiteM2 = (int)cmdCheck.ExecuteScalar() > 0;
                    }
                    
                    // Verificar si existen columnas LimiteM2Tunera y LimiteM2Calandra
                    bool tieneLimitesPrototipo = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'PresupuestoObra' 
                        AND COLUMN_NAME IN ('LimiteM2Tunera', 'LimiteM2Calandra')", conn))
                    {
                        tieneLimitesPrototipo = (int)cmdCheck.ExecuteScalar() >= 2;
                    }
                    
                    // Verificar si existe columna PrototiposAplicables
                    bool tienePrototipos = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'PresupuestoObra' 
                        AND COLUMN_NAME = 'PrototiposAplicables'", conn))
                    {
                        tienePrototipos = (int)cmdCheck.ExecuteScalar() > 0;
                    }
                    
                    string sql = @"
                        SELECT 
                            " + (tieneWBS ? "WBS_Correcto," : "") + @"
                            Etapa,
                            Partida,
                            ISNULL(CostoTunera, 0) AS CostoTunera,
                            ISNULL(CostoCalandra, 0) AS CostoCalandra"
                            + (tieneDinamica ? @",
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
                        WHERE Codigo = @codigo
                        ORDER BY " + (tieneWBS ? "WBS_Correcto" : "Etapa, Partida");
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", conceptoSeleccionado.Codigo.ToString());
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
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
                                
                                if (tieneDinamica)
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
                                
                                partidasConcepto.Add(partida);
                            }
                        }
                    }
                }
                
                ActualizarContador();
                
                Debug.WriteLine(
                    $"✓ Cargadas {partidasConcepto.Count} partidas del concepto [{conceptoSeleccionado.Codigo}] {conceptoSeleccionado.Nombre}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar partidas:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        #endregion
        
        #region Actualización del Grid
        
        private void ActualizarGrid()
        {
            // El BindingList se actualiza automáticamente
            dgvPartidas.Refresh();
        }
        
        private void ActualizarContador()
        {
            lblContador.Text = $"Total: {partidasConcepto.Count} partida(s)";
        }
        
        #endregion
    }
}
