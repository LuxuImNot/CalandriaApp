using System;
using Calandria.Api.Auth;
using Xunit;

namespace Calandria.Api.Tests
{
    // LoginThrottle guarda estado estático por usuario; cada test usa un nombre
    // de usuario único (Guid) para no interferir entre sí.
    public class LoginThrottleTests
    {
        private static string UsuarioUnico() => "test-" + Guid.NewGuid().ToString("N");

        [Fact]
        public void UsuarioSinIntentosPrevios_NoEstaBloqueado()
        {
            Assert.False(LoginThrottle.Bloqueado(UsuarioUnico()));
        }

        [Fact]
        public void MenosDeCincoFallos_NoBloquea()
        {
            string usuario = UsuarioUnico();
            for (int i = 0; i < 4; i++)
                LoginThrottle.RegistrarFallo(usuario);

            Assert.False(LoginThrottle.Bloqueado(usuario));
        }

        [Fact]
        public void CincoFallosSeguidos_Bloquea()
        {
            string usuario = UsuarioUnico();
            for (int i = 0; i < 5; i++)
                LoginThrottle.RegistrarFallo(usuario);

            Assert.True(LoginThrottle.Bloqueado(usuario));
        }

        [Fact]
        public void LoginExitoso_LimpiaElBloqueo()
        {
            string usuario = UsuarioUnico();
            for (int i = 0; i < 5; i++)
                LoginThrottle.RegistrarFallo(usuario);
            Assert.True(LoginThrottle.Bloqueado(usuario));

            LoginThrottle.RegistrarExito(usuario);

            Assert.False(LoginThrottle.Bloqueado(usuario));
        }
    }
}
