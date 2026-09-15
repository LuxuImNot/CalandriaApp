using System;
using System.Data.SqlClient;
using System.Reflection;
using Calandria.Api.Controllers;
using Calandria.Api.Data;
using Xunit;

namespace Calandria.Api.Tests
{
    /// <summary>
    /// Integración contra la obra de prueba real "Prueba_numero_1". Cada test abre
    /// su propia transacción y hace Rollback en Dispose: nunca deja proveedores de
    /// prueba escritos en PROVEEDORESCALANDRIA.
    /// </summary>
    public sealed class ProveedoresIntegrationTests : IDisposable
    {
        private const string ObraPrueba = "Prueba_numero_1";
        private readonly SqlConnection _conn;
        private readonly SqlTransaction _tx;

        public ProveedoresIntegrationTests()
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
        public void GenerarFolio_TrasUnAlta_AvanzaElConsecutivoEnUno()
        {
            int antes = ExtraerConsecutivo(InvocarGenerarFolio());

            InsertarProveedorFake("PONYTAIL-" + Guid.NewGuid().ToString("N").Substring(0, 10));

            int despues = ExtraerConsecutivo(InvocarGenerarFolio());

            Assert.Equal(antes + 1, despues);
        }

        private static int ExtraerConsecutivo(string folio) =>
            int.Parse(folio.Substring("PROV-".Length));

        private void InsertarProveedorFake(string claveUnica)
        {
            const string sql = @"INSERT INTO PROVEEDORESCALANDRIA (ClaveUnica, Nombre, RFC)
                VALUES (@clave, @nombre, @rfc)";
            using (var cmd = new SqlCommand(sql, _conn, _tx))
            {
                cmd.Parameters.AddWithValue("@clave", claveUnica);
                cmd.Parameters.AddWithValue("@nombre", "TEST_ponytail (no persiste, se hace rollback)");
                cmd.Parameters.AddWithValue("@rfc", "XAXX010101000");
                cmd.ExecuteNonQuery();
            }
        }

        private string InvocarGenerarFolio()
        {
            MethodInfo metodo = typeof(ProveedoresController).GetMethod(
                "GenerarFolio", BindingFlags.NonPublic | BindingFlags.Static);
            return (string)metodo.Invoke(null, new object[] { _conn, _tx });
        }
    }
}
