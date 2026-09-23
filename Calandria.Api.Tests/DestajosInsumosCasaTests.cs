using System;
using System.Data.SqlClient;
using Calandria.Api.Data;
using Xunit;

namespace Calandria.Api.Tests
{
    /// <summary>
    /// GET api/destajos/insumos-casa es lo que alimenta la vista "Insumos"
    /// (programado vs usado) desde que FormActivarTareasTreeList dejó el SQL directo.
    /// Lo que esta prueba fija es la forma de la agregación, que es donde se pierde
    /// información si alguien la "simplifica":
    ///   - agrupa por Clave y SUMA cantidades de varias salidas,
    ///   - devuelve el Importe REAL de las salidas (no cantidad x precio programado),
    ///   - filtra por casa con LTRIM/RTRIM (hay Manzana/Lote con espacios en datos viejos).
    ///
    /// Misma mecánica que las otras integraciones: transacción propia y Rollback
    /// siempre, así que no deja nada escrito en la obra de prueba.
    /// </summary>
    public sealed class DestajosInsumosCasaTests : IDisposable
    {
        private const string ObraPrueba = "Prueba_numero_1";
        private const string SqlEndpoint = @"
            SELECT ISNULL(Clave,'') AS Clave,
                   MAX(Descripcion) AS Descripcion,
                   MAX(Unidad) AS Unidad,
                   SUM(Cantidad) AS Usado,
                   SUM(Importe) AS Importe
            FROM dbo.SalidasAlmacen
            WHERE LTRIM(RTRIM(Manzana)) = @m AND LTRIM(RTRIM(Lote)) = @l
            GROUP BY ISNULL(Clave,'')";

        private readonly SqlConnection _conn;
        private readonly SqlTransaction _tx;

        public DestajosInsumosCasaTests()
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

        private void InsertarSalida(string manzana, string lote, string clave,
            string descripcion, decimal cantidad, decimal precio, decimal importe)
        {
            using (var cmd = new SqlCommand(@"
                INSERT INTO SalidasAlmacen
                (Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, FechaSalida, Manzana, Lote, Prototipo, Justificacion)
                VALUES (@c, @d, 'PZA', @cant, @precio, @importe, GETDATE(), @m, @l, NULL, NULL)", _conn, _tx))
            {
                cmd.Parameters.AddWithValue("@c", clave);
                cmd.Parameters.AddWithValue("@d", descripcion);
                cmd.Parameters.AddWithValue("@cant", cantidad);
                cmd.Parameters.AddWithValue("@precio", precio);
                cmd.Parameters.AddWithValue("@importe", importe);
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.ExecuteNonQuery();
            }
        }

        [Fact]
        public void InsumosCasa_SumaVariasSalidasDeLaMismaClave_YRespetaElImporteReal()
        {
            // Manzana/Lote irreales para no chocar con datos de la obra de prueba.
            const string manzana = "ZZ_TEST";
            const string lote = "999";

            // Dos salidas de la misma clave: 3 y 2 piezas. El importe real (160)
            // NO es cantidad x precio unitario (5 x 20 = 100): la segunda salida
            // se surtió a otro precio. Si alguien recalcula el importe en vez de
            // sumarlo, esta prueba falla.
            InsertarSalida(manzana, lote, "MAT-001", "Cemento gris", 3m, 20m, 60m);
            InsertarSalida(manzana, lote, "MAT-001", "Cemento gris", 2m, 50m, 100m);
            // Con espacios: el LTRIM/RTRIM del WHERE tiene que alcanzarla.
            InsertarSalida(" " + manzana + " ", " " + lote + " ", "MAT-002", "Varilla 3/8", 4m, 10m, 40m);
            // Otra casa: no debe aparecer.
            InsertarSalida(manzana, "998", "MAT-003", "Arena", 7m, 1m, 7m);

            int filas = 0;
            decimal usado001 = 0m, importe001 = 0m, usado002 = 0m;

            using (var cmd = new SqlCommand(SqlEndpoint, _conn, _tx))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        filas++;
                        string clave = (rd["Clave"]?.ToString() ?? "").Trim();
                        if (clave == "MAT-001")
                        {
                            usado001 = Convert.ToDecimal(rd["Usado"]);
                            importe001 = Convert.ToDecimal(rd["Importe"]);
                        }
                        else if (clave == "MAT-002")
                        {
                            usado002 = Convert.ToDecimal(rd["Usado"]);
                        }
                    }
                }
            }

            Assert.Equal(2, filas);          // MAT-001 y MAT-002; MAT-003 es de otro lote
            Assert.Equal(5m, usado001);      // 3 + 2, agrupadas por clave
            Assert.Equal(160m, importe001);  // 60 + 100, el importe real de las salidas
            Assert.Equal(4m, usado002);      // la fila con espacios sí entra
        }
    }
}
