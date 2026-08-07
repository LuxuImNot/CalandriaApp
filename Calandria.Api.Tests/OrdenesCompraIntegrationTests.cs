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
    /// su propia transacción y hace Rollback en Dispose: nunca deja folios de
    /// prueba escritos en OrdenesCompra.
    /// </summary>
    public sealed class OrdenesCompraIntegrationTests : IDisposable
    {
        private const string ObraPrueba = "Prueba_numero_1";
        private readonly SqlConnection _conn;
        private readonly SqlTransaction _tx;

        public OrdenesCompraIntegrationTests()
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
        public void GenerarFolio_ConDosOrdenesDelDia_DevuelveElConsecutivo003()
        {
            string prefijo = "OC-MULTI-";
            string baseHoy = prefijo + DateTime.Now.ToString("yyyyMMdd") + "-";
            InsertarOrdenFake(baseHoy + "001");
            InsertarOrdenFake(baseHoy + "002");

            string folio = InvocarGenerarFolio(prefijo);

            Assert.Equal(baseHoy + "003", folio);
        }

        [Fact]
        public void GenerarFolio_SinOrdenesDelDia_EmpiezaEn001()
        {
            // Prefijo que no puede colisionar con folios reales de otra prueba/uso.
            string prefijo = "OC-PONYTAIL-VACIO-";

            string folio = InvocarGenerarFolio(prefijo);

            Assert.Equal(prefijo + DateTime.Now.ToString("yyyyMMdd") + "-001", folio);
        }

        private void InsertarOrdenFake(string folio)
        {
            const string sql = @"INSERT INTO OrdenesCompra (FolioOC, Fecha, Usuario, TipoOrden, NombreOrden)
                VALUES (@f, @fecha, @u, @t, @n)";
            using (var cmd = new SqlCommand(sql, _conn, _tx))
            {
                cmd.Parameters.AddWithValue("@f", folio);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@u", "tests");
                cmd.Parameters.AddWithValue("@t", "MULTIPLE");
                cmd.Parameters.AddWithValue("@n", "TEST_ponytail (no persiste, se hace rollback)");
                cmd.ExecuteNonQuery();
            }
        }

        private string InvocarGenerarFolio(string prefijo)
        {
            MethodInfo metodo = typeof(OrdenesCompraController).GetMethod(
                "GenerarFolio", BindingFlags.NonPublic | BindingFlags.Static);
            return (string)metodo.Invoke(null, new object[] { _conn, _tx, prefijo });
        }
    }
}
