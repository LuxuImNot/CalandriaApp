using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using Calandria.Api;
using Calandria.Api.Controllers;
using Calandria.Api.Data;
using Calandria.Api.Models;
using Xunit;

namespace Calandria.Api.Tests
{
    /// <summary>
    /// Pruebas de integración contra la obra de prueba real "Prueba_numero_1"
    /// (mismo servidor que producción, BD aparte). Cada test abre su propia
    /// transacción y SIEMPRE hace Rollback en Dispose: nunca deja datos escritos,
    /// sin importar cuántas veces se corra.
    ///
    /// Requiere connectionStrings.config local (gitignorado, ver .example) con
    /// CalandriaControlConn apuntando al servidor real.
    /// </summary>
    public sealed class EditorTareasIntegrationTests : IDisposable
    {
        private const string ObraPrueba = "Prueba_numero_1";
        private readonly SqlConnection _conn;
        private readonly SqlTransaction _tx;

        public EditorTareasIntegrationTests()
        {
            ObraContext.CadenaActual = Configuracion.CadenaConexionObra(ObraPrueba);
            _conn = Db.Abrir();
            _tx = _conn.BeginTransaction();
        }

        public void Dispose()
        {
            _tx.Rollback();
            _conn.Dispose();
        }

        [Fact]
        public void DbAbrir_ConObraDePrueba_ConectaYPuedeLeerElArbol()
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM RutaTuneraDestajo", _conn, _tx))
            {
                object resultado = cmd.ExecuteScalar();

                Assert.NotNull(resultado);
            }
        }

        /// <summary>
        /// Regresión del bug ya documentado (memoria del proyecto:
        /// reference_multirow_insert_type_precedence): en un INSERT multi-fila,
        /// si una columna mezcla valores que parecen numéricos ("125000.50") con
        /// texto ("Bloque entero 12x20x40cm") entre distintas filas del mismo
        /// batch, AddWithValue sin tipo explícito fuerza precedencia numérica y
        /// truena con "overflow nvarchar to numeric". El fix ya está en
        /// SincronizarColumnasValor (SqlDbType.NVarChar explícito); esta prueba
        /// falla si alguien lo revierte a AddWithValue plano.
        /// </summary>
        [Fact]
        public void SincronizarColumnasValor_MezclaNumericoYTexto_NoTruenaYPersisteAmbos()
        {
            const int nodoIdPrueba = -999001;
            InsertarNodoTemporal(nodoIdPrueba);

            var nodos = new List<NodoEditorRequest>
            {
                new NodoEditorRequest
                {
                    Id = nodoIdPrueba,
                    Valores = new Dictionary<string, string>
                    {
                        ["Precio"] = "125000.50",
                        ["Notas"] = "Bloque entero 12x20x40cm"
                    }
                }
            };

            InvocarSincronizarColumnasValor(nodos, "RutaTuneraDestajo");

            Assert.Equal("125000.50", LeerValorColumna(nodoIdPrueba, "Precio"));
            Assert.Equal("Bloque entero 12x20x40cm", LeerValorColumna(nodoIdPrueba, "Notas"));
        }

        private void InsertarNodoTemporal(int id)
        {
            const string sql = @"INSERT INTO RutaTuneraDestajo
                (ID, ParentID, Nombre, Descripcion, Orden, Nivel, FechaCreacion, UsuarioCreacion, TipoTarea)
                VALUES (@id, NULL, @nombre, NULL, 0, 0, @fecha, @usuario, 0)";
            using (var cmd = new SqlCommand(sql, _conn, _tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nombre", "TEST_ponytail (no persiste, se hace rollback)");
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@usuario", "tests");
                cmd.ExecuteNonQuery();
            }
        }

        private void InvocarSincronizarColumnasValor(List<NodoEditorRequest> nodos, string tabla)
        {
            MethodInfo metodo = typeof(EditorTareasController).GetMethod(
                "SincronizarColumnasValor", BindingFlags.NonPublic | BindingFlags.Static);
            metodo.Invoke(null, new object[] { nodos, tabla, _conn, _tx });
        }

        private string LeerValorColumna(int nodoId, string nombreColumna)
        {
            const string sql = "SELECT Valor FROM RutaTuneraDestajo_Columnas WHERE NodoID = @nodoId AND NombreColumna = @nombre";
            using (var cmd = new SqlCommand(sql, _conn, _tx))
            {
                cmd.Parameters.AddWithValue("@nodoId", nodoId);
                cmd.Parameters.AddWithValue("@nombre", nombreColumna);
                return (string)cmd.ExecuteScalar();
            }
        }
    }
}
