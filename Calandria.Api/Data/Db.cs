using System;
using System.Data.SqlClient;

namespace Calandria.Api.Data
{
    /// <summary>
    /// Ayudante mínimo de acceso a datos (ADO.NET). Multi-obra: cada obra tiene
    /// su propia base de datos en el mismo servidor; Abrir() resuelve la cadena
    /// de la obra activa en la petición (fijada por JwtMessageHandler a partir
    /// de X-Obra-Id). Usuarios/Perfiles/Obras viven aparte, en la BD maestra.
    /// </summary>
    public static class Db
    {
        /// <summary>Conexión a la BD de la obra activa en esta petición.</summary>
        public static SqlConnection Abrir()
        {
            string cadena = ObraContext.CadenaActual;
            if (string.IsNullOrEmpty(cadena))
                throw new InvalidOperationException("No hay obra activa en esta petición (falta X-Obra-Id o sin acceso).");

            var conn = new SqlConnection(cadena);
            conn.Open();
            return conn;
        }

        /// <summary>Conexión a la BD maestra (Usuarios/Perfiles/Obras). Solo Auth/Perfiles/Obras la usan.</summary>
        public static SqlConnection AbrirMaestra()
        {
            var conn = new SqlConnection(Configuracion.CadenaConexionControl);
            conn.Open();
            return conn;
        }

        /// <summary>Conexión a "master" del mismo servidor que la BD de control, solo para CREATE DATABASE.</summary>
        public static SqlConnection AbrirServidor()
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(Configuracion.CadenaConexionControl)
            {
                InitialCatalog = "master"
            };
            var conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            return conn;
        }
    }
}
