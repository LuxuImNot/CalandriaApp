using System;
using System.ServiceProcess;
using Microsoft.Owin.Hosting;

namespace Calandria.Api
{
    /// <summary>
    /// Servicio de Windows que levanta el host OWIN de Web API.
    /// </summary>
    public sealed class CalandriaService : ServiceBase
    {
        private IDisposable _host;

        public CalandriaService()
        {
            ServiceName = "CalandriaApi";
        }

        protected override void OnStart(string[] args) => Iniciar();
        protected override void OnStop() => Detener();

        // Entradas para el modo consola (depuración).
        public void IniciarConsola() => Iniciar();
        public void DetenerConsola() => Detener();

        private void Iniciar()
        {
            _host = WebApp.Start<Startup>(Configuracion.BaseUrl);
        }

        private void Detener()
        {
            _host?.Dispose();
            _host = null;
        }
    }
}
