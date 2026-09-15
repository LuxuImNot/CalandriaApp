using System.Text;
using Calandria.Api.Services;
using Xunit;

namespace Calandria.Api.Tests
{
    public class CifradoDocumentosTests
    {
        [Fact]
        public void CifrarYDescifrar_DevuelveElContenidoOriginal()
        {
            byte[] claro = Encoding.UTF8.GetBytes("contenido de un PDF de prueba");

            byte[] cifrado = CifradoDocumentos.Cifrar(claro);
            byte[] descifrado = CifradoDocumentos.Descifrar(cifrado);

            Assert.NotEqual(claro, cifrado);
            Assert.Equal(claro, descifrado);
        }

        [Fact]
        public void Cifrar_MismoContenidoDosVeces_ProduceBlobsDistintos()
        {
            byte[] claro = Encoding.UTF8.GetBytes("mismo contenido");

            byte[] cifrado1 = CifradoDocumentos.Cifrar(claro);
            byte[] cifrado2 = CifradoDocumentos.Cifrar(claro);

            Assert.NotEqual(cifrado1, cifrado2); // IV aleatorio por valor
        }

        [Fact]
        public void Descifrar_PdfSinCifrarPrevio_LoDevuelveTalCual()
        {
            byte[] pdfLegacy = Encoding.ASCII.GetBytes("%PDF-1.4 contenido legacy sin cifrar");

            byte[] resultado = CifradoDocumentos.Descifrar(pdfLegacy);

            Assert.Equal(pdfLegacy, resultado);
        }

        [Fact]
        public void CifrarYDescifrar_NuloOVacio_NoFalla()
        {
            Assert.Null(CifradoDocumentos.Cifrar(null));
            Assert.Empty(CifradoDocumentos.Descifrar(new byte[0]));
        }
    }
}
