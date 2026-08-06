using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace DynamicSepticSystem
{
    public class InventarioService
    {
        private readonly string connectionString;

        public InventarioService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        }

        public List<CasaInventario> LeerInventarioCasasSQL()
        {
            var casasInv = new List<CasaInventario>();
            string sql = "SELECT Manzana, Lote, Prototipo, FotoPath, DestajosTerminadosWBS FROM dbo.InventarioCasas";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var casa = new CasaInventario
                        {
                            Manzana = reader["Manzana"]?.ToString(),
                            Lote = reader["Lote"]?.ToString(),
                            Prototipo = reader["Prototipo"]?.ToString(),
                            FotoPath = reader["FotoPath"]?.ToString()
                        };

                        try
                        {
                            var json = reader["DestajosTerminadosWBS"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                casa.DestajosTerminadosWBS = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                            }
                        }
                        catch { }

                        casasInv.Add(casa);
                    }
                }
            }

            return casasInv;
        }

        public void InsertarCasaEnBD(CasaInventario casa)
        {
            string sql = "INSERT INTO InventarioCasas (Manzana, Lote, Prototipo, FotoPath) VALUES (@manzana, @lote, @prototipo, @fotoPath)";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@manzana", casa.Manzana);
                    cmd.Parameters.AddWithValue("@lote", casa.Lote);
                    cmd.Parameters.AddWithValue("@prototipo", casa.Prototipo);
                    cmd.Parameters.AddWithValue("@fotoPath", casa.FotoPath ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Intenta abrir una conexión a la base de datos y devuelve true si funciona.
        // En caso de fallo, devuelve false y el mensaje de error en el parámetro out.
        public bool TestConnection(out string error)
        {
            error = null;
            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    error = "Cadena de conexión 'CalandriaConn' no encontrada o vacía.";
                    return false;
                }

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // opcional: comprobar estado
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                        return true;
                    }
                    else
                    {
                        error = "No se pudo abrir la conexión (estado no abierto).";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

    }
}
