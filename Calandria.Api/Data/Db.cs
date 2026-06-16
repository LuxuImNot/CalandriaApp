using System;
using System.Data.SqlClient;

namespace Calandria.Api.Data
{
    /// <summary>
    /// Ayudante mínimo de acceso a datos (ADO.NET). Misma cadena de conexión que
    /// usaba el cliente, pero ahora vive solo en el servidor.
    /// </summary>
    public static class Db
    {
        public static SqlConnection Abrir()
        {
            var conn = new SqlConnection(Configuracion.CadenaConexion);
            conn.Open();
            return conn;
        }
    }
}
