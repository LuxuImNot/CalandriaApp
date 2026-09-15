using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Calandria.Api.Services
{
    /// <summary>
    /// Sugiere una paleta de 3 colores (primario/secundario/suave) a partir del
    /// logo de una obra, para acentuar las pantallas web de esa obra sin perder
    /// el look profesional del tema base (mismos roles que --brass/--brass-2/
    /// --brass-soft en las páginas: acento principal, hover más oscuro, fondo
    /// suave de tinte).
    /// </summary>
    public static class ExtractorColoresLogo
    {
        public sealed class Paleta
        {
            public string ColorPrimario;
            public string ColorSecundario;
            public string ColorSuave;
        }

        public static Paleta Sugerir(byte[] logoBytes)
        {
            var dominantes = ColoresDominantes(logoBytes);
            return AsignarRoles(dominantes);
        }

        /// <summary>
        /// Reduce el logo a un lienzo pequeño (promedia por downsampling) y agrupa
        /// los píxeles en cubos de color para encontrar los tonos más frecuentes,
        /// descartando fondo (blanco/negro casi puro) y píxeles transparentes.
        /// </summary>
        private static List<Color> ColoresDominantes(byte[] logoBytes)
        {
            const int lado = 48;
            const int cubo = 24; // tamaño del cubo de cuantización por canal

            using (var original = new Bitmap(new MemoryStream(logoBytes)))
            using (var chico = new Bitmap(lado, lado))
            using (var g = Graphics.FromImage(chico))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, lado, lado);

                var buckets = new Dictionary<int, (long r, long g, long b, int n)>();

                for (int y = 0; y < lado; y++)
                {
                    for (int x = 0; x < lado; x++)
                    {
                        Color p = chico.GetPixel(x, y);
                        if (p.A < 128) continue; // transparente: no es color de marca
                        if (p.R > 240 && p.G > 240 && p.B > 240) continue; // blanco de fondo
                        if (p.R < 15 && p.G < 15 && p.B < 15) continue;    // negro de fondo/trazo

                        int key = ((p.R / cubo) << 16) | ((p.G / cubo) << 8) | (p.B / cubo);
                        buckets.TryGetValue(key, out var acc);
                        buckets[key] = (acc.r + p.R, acc.g + p.G, acc.b + p.B, acc.n + 1);
                    }
                }

                var candidatos = buckets.Values
                    .OrderByDescending(v => v.n)
                    .Select(v => Color.FromArgb((int)(v.r / v.n), (int)(v.g / v.n), (int)(v.b / v.n)))
                    .ToList();

                // Greedy: toma los más frecuentes pero exige que sean distinguibles
                // entre sí (si no, todo el logo colapsa en variaciones del mismo tono).
                var elegidos = new List<Color>();
                foreach (var c in candidatos)
                {
                    if (elegidos.Count >= 3) break;
                    if (elegidos.All(e => DistanciaColor(e, c) > 40))
                        elegidos.Add(c);
                }
                return elegidos;
            }
        }

        private static double DistanciaColor(Color a, Color b)
        {
            double dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
            return Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        /// <summary>
        /// Convierte los tonos dominantes encontrados en una paleta de 3 roles
        /// usable en botones/acentos: si el logo no trae suficientes tonos
        /// distinguibles (logos de un solo color, o casi monocromos), completa los
        /// que falten a partir del más fuerte, ajustando brillo por HSL.
        /// </summary>
        private static Paleta AsignarRoles(List<Color> dominantes)
        {
            // El "primario" es el candidato más usable como fondo de botón: ni
            // demasiado claro (se pierde contra fondo blanco) ni casi negro.
            Color primario = dominantes
                .OrderByDescending(c => Usabilidad(c))
                .FirstOrDefault();
            if (primario.IsEmpty) primario = Color.FromArgb(122, 90, 18); // fallback: brass actual

            var restantes = dominantes.Where(c => c != primario).ToList();

            // El secundario es el hover: tiene que ser más oscuro que el primario,
            // si no ningún candidato restante sirve y hay que sintetizarlo.
            Color secundario = restantes.FirstOrDefault(c => Brillo(c) < Brillo(primario) - 0.06);
            if (secundario.IsEmpty)
                secundario = AjustarBrillo(primario, -0.14);

            Color suave = restantes.FirstOrDefault(c => Brillo(c) > 0.78 && c != secundario);
            if (suave.IsEmpty)
                suave = AjustarBrillo(primario, +0.42, saturacionMax: 0.30);

            return new Paleta
            {
                ColorPrimario = Hex(primario),
                ColorSecundario = Hex(secundario),
                ColorSuave = Hex(suave)
            };
        }

        /// <summary>Puntaje simple: favorece tonos de brillo medio y con algo de saturación (evita grises apagados).</summary>
        private static double Usabilidad(Color c)
        {
            double brillo = Brillo(c);
            double saturacion = c.GetSaturation();
            double penalizacionBrillo = Math.Abs(brillo - 0.42); // ideal ~42% de luminosidad
            return saturacion * 2.0 - penalizacionBrillo * 3.0;
        }

        private static double Brillo(Color c) => c.GetBrightness();

        /// <summary>Sube o baja la luminosidad (HSL) del color manteniendo tono/saturación.</summary>
        private static Color AjustarBrillo(Color c, double deltaL, double? saturacionMax = null)
        {
            float h = c.GetHue();
            float s = c.GetSaturation();
            float l = Clamp01(c.GetBrightness() + (float)deltaL);
            if (saturacionMax.HasValue) s = Math.Min(s, (float)saturacionMax.Value);
            return DesdeHsl(h, s, l);
        }

        private static float Clamp01(float v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        private static Color DesdeHsl(float h, float s, float l)
        {
            if (s <= 0.0001f)
            {
                int gris = (int)Math.Round(l * 255);
                return Color.FromArgb(gris, gris, gris);
            }

            float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
            float p = 2 * l - q;
            float hk = h / 360f;

            double r = HueAChannel(p, q, hk + 1.0 / 3.0);
            double g = HueAChannel(p, q, hk);
            double b = HueAChannel(p, q, hk - 1.0 / 3.0);

            return Color.FromArgb(
                (int)Math.Round(r * 255),
                (int)Math.Round(g * 255),
                (int)Math.Round(b * 255));
        }

        private static double HueAChannel(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }

        private static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }
}
