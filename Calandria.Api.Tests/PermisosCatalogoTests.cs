using Calandria.Api.Auth;
using Xunit;

namespace Calandria.Api.Tests
{
    public class PermisosCatalogoTests
    {
        [Fact]
        public void Existe_ClaveDelCatalogo_DevuelveTrue()
        {
            Assert.True(PermisosCatalogo.Existe("compras.editar"));
        }

        [Fact]
        public void Existe_ClaveInventada_DevuelveFalse()
        {
            Assert.False(PermisosCatalogo.Existe("modulo.que.no.existe"));
        }

        [Fact]
        public void Existe_Null_DevuelveFalseSinLanzar()
        {
            Assert.False(PermisosCatalogo.Existe(null));
        }
    }
}
