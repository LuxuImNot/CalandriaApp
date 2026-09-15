using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Calandria.Api.Data;

namespace Calandria.Api.Services
{
    /// <summary>
    /// Aprovisiona la BD de una obra nueva: nombre único, CREATE DATABASE y aplica la
    /// plantilla de esquema. Compartido por ObrasController (obra nueva de un cliente
    /// existente) y ClientesController (primera obra de un cliente nuevo) -- no toca
    /// las tablas Obras/UsuarioObras de la BD maestra, eso lo hace el llamador, que ya
    /// sabe el ClienteId correcto.
    /// </summary>
    public static class ObraProvisioning
    {
        public static string CrearBaseDeDatos(SqlConnection master, string nombreObra)
        {
            string nombreBd = GenerarNombreBdUnico(master, nombreObra);

            using (var servidor = Db.AbrirServidor())
            using (var cmd = new SqlCommand($"CREATE DATABASE [{nombreBd}]", servidor))
                cmd.ExecuteNonQuery();

            using (var conn = new SqlConnection(Configuracion.CadenaConexionObra(nombreBd)))
            {
                conn.Open();
                foreach (var lote in CargarPlantillaPorLotes())
                using (var cmd = new SqlCommand(lote, conn))
                    cmd.ExecuteNonQuery();
            }

            return nombreBd;
        }

        private static string GenerarNombreBdUnico(SqlConnection master, string nombre)
        {
            string basecito = new string(nombre
                .Select(c => char.IsLetterOrDigit(c) ? c : '_')
                .ToArray())
                .Trim('_');
            if (basecito.Length == 0) basecito = "Obra";
            if (basecito.Length > 100) basecito = basecito.Substring(0, 100);

            string candidato = basecito;
            int sufijo = 1;
            while (ExisteBaseDeDatos(master, candidato))
                candidato = basecito + "_" + (++sufijo);
            return candidato;
        }

        private static bool ExisteBaseDeDatos(SqlConnection master, string nombreBd)
        {
            using (var cmd = new SqlCommand("SELECT 1 FROM Obras WHERE NombreBD = @nombreBd", master))
            {
                cmd.Parameters.AddWithValue("@nombreBd", nombreBd);
                return cmd.ExecuteScalar() != null;
            }
        }

        private static IEnumerable<string> CargarPlantillaPorLotes()
        {
            string ruta = Configuracion.SqlPlantillaObraRuta;
            if (!File.Exists(ruta))
                throw new InvalidOperationException("Falta la plantilla de esquema de obra: " + ruta);

            string texto = File.ReadAllText(ruta);
            return texto
                .Split(new[] { "\r\nGO\r\n", "\nGO\n", "\r\nGO", "GO\r\n" }, StringSplitOptions.None)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0);
        }
    }
}
