using System;
using System.Globalization;
using System.Text;

namespace DynamicSepticSystem
{
    public static class NumeroALetras
    {
        public static string Convertir(decimal monto, string moneda = "PESOS", string fraccion = "M.N.")
        {
            if (monto < 0) return "MENOS " + Convertir(-monto, moneda, fraccion);

            long entero = (long)Math.Truncate(monto);
            int centavos = (int)Math.Round((monto - entero) * 100m, MidpointRounding.AwayFromZero);
            if (centavos == 100) { entero++; centavos = 0; }

            string letras = EnteroALetras(entero).Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(letras)) letras = "CERO";

            string fraccionTxt = centavos.ToString("D2", CultureInfo.InvariantCulture) + "/100";

            var sb = new StringBuilder();
            sb.Append(letras);
            sb.Append(' ');
            sb.Append(moneda);
            sb.Append(' ');
            sb.Append(fraccionTxt);
            if (!string.IsNullOrEmpty(fraccion))
            {
                sb.Append(' ');
                sb.Append(fraccion);
            }
            return sb.ToString();
        }

        private static string EnteroALetras(long numero)
        {
            if (numero == 0) return "CERO";
            if (numero < 0) return "MENOS " + EnteroALetras(-numero);

            if (numero >= 1_000_000)
            {
                long millones = numero / 1_000_000;
                long resto = numero % 1_000_000;
                string prefijo = millones == 1
                    ? "UN MILLON"
                    : EnteroALetras(millones).Trim() + " MILLONES";
                return resto == 0 ? prefijo : prefijo + " " + EnteroALetras(resto).Trim();
            }

            if (numero >= 1_000)
            {
                long miles = numero / 1_000;
                long resto = numero % 1_000;
                string prefijo = miles == 1 ? "MIL" : EnteroALetras(miles).Trim() + " MIL";
                return resto == 0 ? prefijo : prefijo + " " + EnteroALetras(resto).Trim();
            }

            if (numero >= 100)
            {
                long centenas = numero / 100;
                long resto = numero % 100;
                string prefijo = Centenas(centenas, resto == 0);
                return resto == 0 ? prefijo : prefijo + " " + EnteroALetras(resto).Trim();
            }

            if (numero >= 30)
            {
                long decenas = numero / 10;
                long unidades = numero % 10;
                string prefijo = DecenasMayoresA20(decenas);
                return unidades == 0 ? prefijo : prefijo + " Y " + Unidades(unidades);
            }

            if (numero >= 20)
            {
                if (numero == 20) return "VEINTE";
                return "VEINTI" + UnidadesUnida(numero - 20);
            }

            if (numero >= 10)
            {
                switch (numero)
                {
                    case 10: return "DIEZ";
                    case 11: return "ONCE";
                    case 12: return "DOCE";
                    case 13: return "TRECE";
                    case 14: return "CATORCE";
                    case 15: return "QUINCE";
                    case 16: return "DIECISEIS";
                    case 17: return "DIECISIETE";
                    case 18: return "DIECIOCHO";
                    case 19: return "DIECINUEVE";
                }
            }

            return Unidades(numero);
        }

        private static string Centenas(long centenas, bool exacto)
        {
            switch (centenas)
            {
                case 1: return exacto ? "CIEN" : "CIENTO";
                case 2: return "DOSCIENTOS";
                case 3: return "TRESCIENTOS";
                case 4: return "CUATROCIENTOS";
                case 5: return "QUINIENTOS";
                case 6: return "SEISCIENTOS";
                case 7: return "SETECIENTOS";
                case 8: return "OCHOCIENTOS";
                case 9: return "NOVECIENTOS";
                default: return "";
            }
        }

        private static string DecenasMayoresA20(long decenas)
        {
            switch (decenas)
            {
                case 3: return "TREINTA";
                case 4: return "CUARENTA";
                case 5: return "CINCUENTA";
                case 6: return "SESENTA";
                case 7: return "SETENTA";
                case 8: return "OCHENTA";
                case 9: return "NOVENTA";
                default: return "";
            }
        }

        private static string Unidades(long n)
        {
            switch (n)
            {
                case 1: return "UNO";
                case 2: return "DOS";
                case 3: return "TRES";
                case 4: return "CUATRO";
                case 5: return "CINCO";
                case 6: return "SEIS";
                case 7: return "SIETE";
                case 8: return "OCHO";
                case 9: return "NUEVE";
                default: return "";
            }
        }

        private static string UnidadesUnida(long n)
        {
            switch (n)
            {
                case 1: return "UNO";
                case 2: return "DOS";
                case 3: return "TRES";
                case 4: return "CUATRO";
                case 5: return "CINCO";
                case 6: return "SEIS";
                case 7: return "SIETE";
                case 8: return "OCHO";
                case 9: return "NUEVE";
                default: return "";
            }
        }
    }
}
