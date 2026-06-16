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

        public void ActualizarDestajosTerminados(CasaInventario casa)
        {
            string sql = "UPDATE InventarioCasas SET DestajosTerminadosWBS = @list WHERE Manzana = @manzana AND Lote = @lote";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@list", JsonConvert.SerializeObject(casa.DestajosTerminadosWBS ?? new List<string>()));
                    cmd.Parameters.AddWithValue("@manzana", casa.Manzana);
                    cmd.Parameters.AddWithValue("@lote", casa.Lote);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public CasaInventario ObtenerCasaPorManzanaYLote(string manzana, string lote)
        {
            CasaInventario casa = null;
            string sql = "SELECT Manzana, Lote, Prototipo, FotoPath, DestajosTerminadosWBS FROM dbo.InventarioCasas WHERE Manzana = @m AND Lote = @l";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            casa = new CasaInventario
                            {
                                Manzana = reader["Manzana"]?.ToString(),
                                Lote = reader["Lote"]?.ToString(),
                                Prototipo = reader["Prototipo"]?.ToString(),
                                FotoPath = reader["FotoPath"]?.ToString(),
                                DestajosTerminadosWBS = new List<string>()
                            };

                            try
                            {
                                var json = reader["DestajosTerminadosWBS"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(json))
                                    casa.DestajosTerminadosWBS = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                            }
                            catch { }
                        }
                    }
                }
            }

            return casa;
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

        /// <summary>
        /// Ensure the table for storing destajo insumos exists.
        /// </summary>
        public void EnsureDestajoInsumosTable()
        {
            string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.DestajoInsumos') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.DestajoInsumos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(50) NOT NULL,
        Lote NVARCHAR(50) NOT NULL,
        WBS NVARCHAR(100) NULL,
        InsumosJson NVARCHAR(MAX) NULL,
        Fecha DATETIME NOT NULL DEFAULT(GETDATE())
    );
    CREATE INDEX IX_DestajoInsumos_Key ON dbo.DestajoInsumos(Manzana, Lote, WBS);
END
";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Guarda o actualiza los insumos entregados para un destajo (manzana+lote+wbs).
        /// </summary>
        public void GuardarInsumosEntregados(string manzana, string lote, string wbs, DataTable dtInsumos)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote) || dtInsumos == null) return;

            EnsureDestajoInsumosTable();

            string json = JsonConvert.SerializeObject(dtInsumos);

            string sqlCheck = "SELECT Id FROM dbo.DestajoInsumos WHERE Manzana = @m AND Lote = @l AND (WBS = @w OR (@w IS NULL AND WBS IS NULL))";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                int? existingId = null;
                using (var cmd = new SqlCommand(sqlCheck, conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                    cmd.Parameters.AddWithValue("@w", (object)wbs ?? DBNull.Value);
                    var res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) existingId = Convert.ToInt32(res);
                }

                if (existingId.HasValue)
                {
                    string sqlUpd = "UPDATE dbo.DestajoInsumos SET InsumosJson = @j, Fecha = GETDATE() WHERE Id = @id";
                    using (var cmd = new SqlCommand(sqlUpd, conn))
                    {
                        cmd.Parameters.AddWithValue("@j", json ?? string.Empty);
                        cmd.Parameters.AddWithValue("@id", existingId.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string sqlIns = "INSERT INTO dbo.DestajoInsumos (Manzana, Lote, WBS, InsumosJson, Fecha) VALUES (@m, @l, @w, @j, GETDATE())";
                    using (var cmd = new SqlCommand(sqlIns, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);
                        cmd.Parameters.AddWithValue("@w", (object)wbs ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@j", json ?? string.Empty);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene la tabla de insumos entregados para un destajo o null si no existe.
        /// </summary>
        public DataTable ObtenerInsumosEntregados(string manzana, string lote, string wbs)
        {
            try
            {
                EnsureDestajoInsumosTable();
                string sql = "SELECT InsumosJson FROM dbo.DestajoInsumos WHERE Manzana = @m AND Lote = @l AND (WBS = @w OR (@w IS NULL AND WBS IS NULL))";
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                    cmd.Parameters.AddWithValue("@w", (object)wbs ?? DBNull.Value);
                    var res = cmd.ExecuteScalar() as string;
                    if (string.IsNullOrWhiteSpace(res)) return null;
                    var dt = JsonConvert.DeserializeObject<DataTable>(res);
                    return dt;
                }
            }
            catch { return null; }
        }

        /// <summary>
        /// Ensure table for delivered mano de obra exists.
        /// </summary>
        public void EnsureDestajoManoObraTable()
        {
            string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.DestajoManoObra') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.DestajoManoObra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(50) NOT NULL,
        Lote NVARCHAR(50) NOT NULL,
        WBS NVARCHAR(100) NULL,
        ManoJson NVARCHAR(MAX) NULL,
        Fecha DATETIME NOT NULL DEFAULT(GETDATE())
    );
    CREATE INDEX IX_DestajoMano_Key ON dbo.DestajoManoObra(Manzana, Lote, WBS);
END
";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void GuardarManoObraEntregada(string manzana, string lote, string wbs, DataTable dt)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote) || dt == null) return;
            EnsureDestajoManoObraTable();
            string json = JsonConvert.SerializeObject(dt);
            string sqlCheck = "SELECT Id FROM dbo.DestajoManoObra WHERE Manzana = @m AND Lote = @l AND (WBS = @w OR (@w IS NULL AND WBS IS NULL))";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                int? existingId = null;
                using (var cmd = new SqlCommand(sqlCheck, conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                    cmd.Parameters.AddWithValue("@w", (object)wbs ?? DBNull.Value);
                    var res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) existingId = Convert.ToInt32(res);
                }
                if (existingId.HasValue)
                {
                    string sqlUpd = "UPDATE dbo.DestajoManoObra SET ManoJson = @j, Fecha = GETDATE() WHERE Id = @id";
                    using (var cmd = new SqlCommand(sqlUpd, conn))
                    {
                        cmd.Parameters.AddWithValue("@j", json ?? string.Empty);
                        cmd.Parameters.AddWithValue("@id", existingId.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string sqlIns = "INSERT INTO dbo.DestajoManoObra (Manzana, Lote, WBS, ManoJson, Fecha) VALUES (@m, @l, @w, @j, GETDATE())";
                    using (var cmd = new SqlCommand(sqlIns, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);
                        cmd.Parameters.AddWithValue("@w", (object)wbs ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@j", json ?? string.Empty);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public DataTable ObtenerManoObraEntregada(string manzana, string lote, string wbs)
        {
            try
            {
                EnsureDestajoManoObraTable();
                string sql = "SELECT ManoJson FROM dbo.DestajoManoObra WHERE Manzana = @m AND Lote = @l AND (WBS = @w OR (@w IS NULL AND WBS IS NULL))";
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                    cmd.Parameters.AddWithValue("@w", (object)wbs ?? DBNull.Value);
                    var res = cmd.ExecuteScalar() as string;
                    if (string.IsNullOrWhiteSpace(res)) return null;
                    var dt = JsonConvert.DeserializeObject<DataTable>(res);
                    return dt;
                }
            }
            catch { return null; }
        }
    }
}
