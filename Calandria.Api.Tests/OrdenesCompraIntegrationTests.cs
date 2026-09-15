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
        public void GenerarFolio_ConDosOrdenesPrevias_DevuelveElConsecutivo000003()
        {
            string prefijo = "OC-PONYTAIL-DOS-";
            InsertarOrdenFake(prefijo + "000001");
            InsertarOrdenFake(prefijo + "000002");

            string folio = InvocarGenerarFolio(prefijo);

            Assert.Equal(prefijo + "000003", folio);
        }

        [Fact]
        public void GenerarFolio_SinOrdenesPrevias_EmpiezaEn000001()
        {
            // Prefijo que no puede colisionar con folios reales de otra prueba/uso.
            string prefijo = "OC-PONYTAIL-VACIO-";

            string folio = InvocarGenerarFolio(prefijo);

            Assert.Equal(prefijo + "000001", folio);
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

        // Requiere que la obra de prueba ya tenga la tabla ConciliacionesIaLog
        // (8_APLICAR_ConciliacionesIaLog.sql) — si no, este test falla hasta que se
        // corra ese script contra "Prueba_numero_1".
        [Fact]
        public void RegistrarConciliacionIaLog_MismoUuidDosVeces_NoDuplica()
        {
            string uuid = Guid.NewGuid().ToString();

            InvocarRegistrarConciliacionIaLog(uuid, "OC-TEST-001");
            InvocarRegistrarConciliacionIaLog(uuid, "OC-TEST-001"); // reintento del mismo CFDI: no debe duplicar

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM ConciliacionesIaLog WHERE Uuid = @uuid", _conn, _tx))
            {
                cmd.Parameters.AddWithValue("@uuid", uuid);
                Assert.Equal(1, (int)cmd.ExecuteScalar());
            }
        }

        private void InvocarRegistrarConciliacionIaLog(string uuid, string folioOC)
        {
            MethodInfo metodo = typeof(OrdenesCompraController).GetMethod(
                "RegistrarConciliacionIaLog", BindingFlags.NonPublic | BindingFlags.Static);
            metodo.Invoke(null, new object[] { _conn, _tx, uuid, folioOC, "tests" });
        }
    }
}
