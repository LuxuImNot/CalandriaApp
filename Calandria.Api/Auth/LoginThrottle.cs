using System;
using System.Collections.Concurrent;

namespace Calandria.Api.Auth
{
    /// <summary>
    /// Bloqueo temporal por intentos fallidos de login, para frenar fuerza bruta
    /// contra un usuario. Estado en memoria del proceso (suficiente para un solo
    /// servicio; se reinicia si el servicio reinicia).
    /// </summary>
    public static class LoginThrottle
    {
        private const int MaxIntentos = 5;
        private static readonly TimeSpan Ventana = TimeSpan.FromMinutes(15);

        private sealed class Estado
        {
            public int Intentos;
            public DateTime PrimerIntentoUtc;
        }

        private static readonly ConcurrentDictionary<string, Estado> _estados =
            new ConcurrentDictionary<string, Estado>(StringComparer.OrdinalIgnoreCase);

        public static bool Bloqueado(string usuario)
        {
            if (_estados.TryGetValue(usuario, out var e))
            {
                if (DateTime.UtcNow - e.PrimerIntentoUtc > Ventana)
                {
                    _estados.TryRemove(usuario, out _);
                    return false;
                }
                return e.Intentos >= MaxIntentos;
            }
            return false;
        }

        public static void RegistrarFallo(string usuario)
        {
            _estados.AddOrUpdate(usuario,
                _ => new Estado { Intentos = 1, PrimerIntentoUtc = DateTime.UtcNow },
                (_, e) =>
                {
                    if (DateTime.UtcNow - e.PrimerIntentoUtc > Ventana)
                    {
                        e.Intentos = 1;
                        e.PrimerIntentoUtc = DateTime.UtcNow;
                    }
                    else
                    {
                        e.Intentos++;
                    }
                    return e;
                });
        }

        public static void RegistrarExito(string usuario)
        {
            _estados.TryRemove(usuario, out _);
        }
    }
}
