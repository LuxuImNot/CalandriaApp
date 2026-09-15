using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml;
using Calandria.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Calandria.Api.Services
{
    /// <summary>
    /// Cabecera + conceptos de un CFDI ya parseado. Los datos salen EXACTOS del XML
    /// (directo del XML, sin OCR); el SAT obliga a que toda factura tenga su XML.
    /// </summary>
    public sealed class CfdiParseado
    {
        public string Emisor;
        public string Rfc;
        public string Uuid;
        public decimal Total;
        public List<FacturaConceptoDto> Conceptos = new List<FacturaConceptoDto>();
    }

    /// <summary>
    /// Parser de CFDI agnóstico de versión (3.3 / 4.0): usa XPath con local-name()
    /// para no depender del namespace cfdi:. Lee Comprobante (Total), Emisor
    /// (Nombre/Rfc), el UUID del TimbreFiscalDigital y los Conceptos.
    /// </summary>
    public static class CfdiParser
    {
        public static CfdiParseado Parse(string xml)
        {
            // Un CFDI real no trae DOCTYPE: prohibirlo bloquea XXE y expansión de
            // entidades (billion laughs) sin afectar el parseo normal.
            var doc = new XmlDocument { XmlResolver = null };
            var readerSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            using (var stringReader = new System.IO.StringReader(xml))
            using (var xmlReader = XmlReader.Create(stringReader, readerSettings))
            {
                doc.Load(xmlReader);
            }

            var r = new CfdiParseado();

            var comprobante = doc.SelectSingleNode("/*[local-name()='Comprobante']");
            r.Total = AttrDecimal(comprobante, "Total");

            var emisor = doc.SelectSingleNode("//*[local-name()='Emisor']");
            r.Emisor = AttrStr(emisor, "Nombre");
            r.Rfc = AttrStr(emisor, "Rfc");

            var tfd = doc.SelectSingleNode("//*[local-name()='TimbreFiscalDigital']");
            r.Uuid = AttrStr(tfd, "UUID");

            foreach (XmlNode c in doc.SelectNodes("//*[local-name()='Concepto']"))
            {
                r.Conceptos.Add(new FacturaConceptoDto
                {
                    NoIdentificacion = AttrStr(c, "NoIdentificacion"),
                    ClaveProdServ = AttrStr(c, "ClaveProdServ"),
                    Descripcion = AttrStr(c, "Descripcion"),
                    Unidad = !string.IsNullOrEmpty(AttrStr(c, "Unidad"))
                        ? AttrStr(c, "Unidad") : AttrStr(c, "ClaveUnidad"),
                    Cantidad = AttrDecimal(c, "Cantidad"),
                    ValorUnitario = AttrDecimal(c, "ValorUnitario"),
                    Importe = AttrDecimal(c, "Importe")
                });
            }
            return r;
        }

        private static string AttrStr(XmlNode node, string attr)
        {
            if (node?.Attributes == null) return "";
            var a = node.Attributes[attr];
            return a == null ? "" : (a.Value ?? "").Trim();
        }

        private static decimal AttrDecimal(XmlNode node, string attr)
        {
            string s = AttrStr(node, attr);
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v)) return v;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal v2)) return v2;
            return 0m;
        }
    }

    /// <summary>Una línea de la OC a emparejar (lo mínimo para el cotejo).</summary>
    public sealed class OcLinea
    {
        public int IdDetalle;
        public string Clave;
        public string Descripcion;
        public string Unidad;
        public decimal Cantidad;
    }

    /// <summary>
    /// Empareja conceptos de factura con líneas de la OC. Primero determinista
    /// (clave exacta → nombre normalizado) y, para lo que quede dudoso, un
    /// emparejador automático opcional (configurable en App.config).
    /// </summary>
    public static class ConciliadorFactura
    {
        public static ConciliarFacturaResponse Conciliar(
            string folioOC, CfdiParseado cfdi, List<OcLinea> lineasOC)
        {
            var resp = new ConciliarFacturaResponse
            {
                FolioOC = folioOC,
                Emisor = cfdi.Emisor,
                Rfc = cfdi.Rfc,
                Uuid = cfdi.Uuid,
                TotalFactura = cfdi.Total
            };

            // Suma de cantidades facturadas por línea de OC, según el concepto emparejado.
            var facturadoPorLinea = new Dictionary<int, decimal>();
            var origenPorLinea = new Dictionary<int, string>();
            var descFacturaPorLinea = new Dictionary<int, string>();
            var confianzaPorLinea = new Dictionary<int, double>();

            var conceptosLibres = new List<FacturaConceptoDto>(cfdi.Conceptos);

            // 1) Por CLAVE exacta (NoIdentificacion == Clave de la OC).
            foreach (var oc in lineasOC)
            {
                if (string.IsNullOrWhiteSpace(oc.Clave)) continue;
                var match = conceptosLibres.FirstOrDefault(c =>
                    !string.IsNullOrWhiteSpace(c.NoIdentificacion) &&
                    string.Equals(c.NoIdentificacion.Trim(), oc.Clave.Trim(), StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    Asignar(oc, match, "Clave", 1.0, facturadoPorLinea, origenPorLinea, descFacturaPorLinea, confianzaPorLinea, conceptosLibres);
            }

            // 2) Por NOMBRE normalizado (descripción).
            foreach (var oc in lineasOC)
            {
                if (facturadoPorLinea.ContainsKey(oc.IdDetalle)) continue;
                string normOc = NormNombre(oc.Descripcion);
                if (normOc.Length == 0) continue;
                var match = conceptosLibres.FirstOrDefault(c => NormNombre(c.Descripcion) == normOc);
                if (match != null)
                    Asignar(oc, match, "Nombre", 0.9, facturadoPorLinea, origenPorLinea, descFacturaPorLinea, confianzaPorLinea, conceptosLibres);
            }

            // 3) Emparejamiento automático (proveedor configurado) para lo que quede dudoso.
            var ocPendientes = lineasOC.Where(o => !facturadoPorLinea.ContainsKey(o.IdDetalle)).ToList();
            if (ocPendientes.Count > 0 && conceptosLibres.Count > 0)
            {
                var pares = EmparejadorIA.Emparejar(ocPendientes, conceptosLibres, out bool usoIA, out string aviso);
                resp.UsoIA = usoIA;
                if (!string.IsNullOrEmpty(aviso)) resp.Aviso = aviso;
                foreach (var par in pares)
                {
                    var oc = ocPendientes.FirstOrDefault(o => o.IdDetalle == par.IdDetalle);
                    var concepto = conceptosLibres.FirstOrDefault(c => ReferenceEquals(c, par.Concepto));
                    if (oc != null && concepto != null)
                        Asignar(oc, concepto, "Auto", par.Confianza, facturadoPorLinea, origenPorLinea, descFacturaPorLinea, confianzaPorLinea, conceptosLibres);
                }
            }

            // Armar líneas de salida.
            foreach (var oc in lineasOC)
            {
                decimal facturada = facturadoPorLinea.TryGetValue(oc.IdDetalle, out var f) ? f : 0m;
                resp.Lineas.Add(new LineaConciliacionDto
                {
                    IdDetalle = oc.IdDetalle,
                    Clave = oc.Clave,
                    Descripcion = oc.Descripcion,
                    Unidad = oc.Unidad,
                    CantidadOC = oc.Cantidad,
                    CantidadFacturada = facturada,
                    Diferencia = facturada - oc.Cantidad,
                    DescripcionFactura = descFacturaPorLinea.TryGetValue(oc.IdDetalle, out var d) ? d : "",
                    Origen = origenPorLinea.TryGetValue(oc.IdDetalle, out var o) ? o : "SinMatch",
                    Confianza = confianzaPorLinea.TryGetValue(oc.IdDetalle, out var cf) ? cf : 0.0
                });
            }

            // Conceptos de la factura que no se asignaron a ninguna línea.
            resp.SinAsignar.AddRange(conceptosLibres);
            return resp;
        }

        private static void Asignar(OcLinea oc, FacturaConceptoDto concepto, string origen, double confianza,
            Dictionary<int, decimal> facturado, Dictionary<int, string> origenMap,
            Dictionary<int, string> descMap, Dictionary<int, double> confMap, List<FacturaConceptoDto> libres)
        {
            facturado[oc.IdDetalle] = (facturado.TryGetValue(oc.IdDetalle, out var v) ? v : 0m) + concepto.Cantidad;
            origenMap[oc.IdDetalle] = origen;
            descMap[oc.IdDetalle] = concepto.Descripcion;
            confMap[oc.IdDetalle] = confianza;
            libres.Remove(concepto);
        }

        /// <summary>Sin acentos, espacios colapsados, MAYÚSCULAS (igual que el resto del sistema).</summary>
        public static string NormNombre(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            string t = s.Trim().ToUpperInvariant();
            var sb = new StringBuilder(t.Length);
            bool espacioPrevio = false;
            foreach (char ch in t.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsWhiteSpace(ch))
                {
                    if (!espacioPrevio && sb.Length > 0) sb.Append(' ');
                    espacioPrevio = true;
                }
                else { sb.Append(ch); espacioPrevio = false; }
            }
            return sb.ToString().Trim().Normalize(NormalizationForm.FormC);
        }
    }

    public sealed class ParEmparejado
    {
        public int IdDetalle;
        public FacturaConceptoDto Concepto;
        public double Confianza;
    }

    /// <summary>
    /// Emparejador automático: usa el proveedor configurado solo para las líneas que el
    /// cotejo determinista no resolvió. Si está desactivado o no disponible, no rompe
    /// nada (usoIA=false) y el sistema sigue con clave/nombre.
    /// </summary>
    public static class EmparejadorIA
    {
        // Timeout amplio: la primera petición a Ollama carga el modelo en RAM (puede tardar).
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };

        public static List<ParEmparejado> Emparejar(
            List<OcLinea> ocPendientes, List<FacturaConceptoDto> conceptosLibres,
            out bool usoIA, out string aviso)
        {
            usoIA = false;
            aviso = null;
            var resultado = new List<ParEmparejado>();

            string proveedor = (Configuracion.IaProveedor ?? "").Trim().ToLowerInvariant();
            if (proveedor == "" || proveedor == "none" || proveedor == "ninguno")
            {
                aviso = "Líneas no emparejadas automáticamente (emparejador automático desactivado).";
                return resultado;
            }

            string prompt = ConstruirPrompt(ocPendientes, conceptosLibres);

            try
            {
                string texto =
                    proveedor == "ollama" ? LlamarOllama(prompt, out aviso)
                                          : LlamarGemini(prompt, out aviso);
                if (string.IsNullOrWhiteSpace(texto)) return resultado; // aviso ya seteado

                foreach (var par in Parsear(texto, ocPendientes, conceptosLibres))
                    resultado.Add(par);
                usoIA = true;
            }
            catch (Exception ex)
            {
                aviso = "Emparejador automático omitido: " + ex.Message;
            }
            return resultado;
        }

        /// <summary>Prompt compartido por todos los proveedores (índices estables).</summary>
        private static string ConstruirPrompt(List<OcLinea> oc, List<FacturaConceptoDto> fc)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Eres un conciliador de facturas de construcción en México.");
            sb.AppendLine("Empareja cada PARTIDA de la orden de compra con el CONCEPTO de la factura que se refiera al MISMO material, aunque la redacción difiera.");
            sb.AppendLine("Responde SOLO JSON con esta forma exacta: {\"pares\":[{\"oc\":<indice>,\"factura\":<indice>,\"confianza\":<0..1>}]}. Omite las que no tengan una contraparte clara (confianza < 0.6). No agregues texto fuera del JSON.");
            sb.AppendLine();
            sb.AppendLine("PARTIDAS_OC:");
            for (int i = 0; i < oc.Count; i++)
                sb.AppendLine($"{i}: {oc[i].Descripcion} (unidad {oc[i].Unidad})");
            sb.AppendLine();
            sb.AppendLine("CONCEPTOS_FACTURA:");
            for (int i = 0; i < fc.Count; i++)
                sb.AppendLine($"{i}: {fc[i].Descripcion} (unidad {fc[i].Unidad})");
            return sb.ToString();
        }

        /// <summary>Acepta {"pares":[...]} o un arreglo suelto [...] y valida índices.</summary>
        private static List<ParEmparejado> Parsear(
            string texto, List<OcLinea> oc, List<FacturaConceptoDto> fc)
        {
            var res = new List<ParEmparejado>();
            if (string.IsNullOrWhiteSpace(texto)) return res;

            JArray arr = null;
            var token = JToken.Parse(texto);
            if (token is JArray a) arr = a;
            else if (token is JObject o)
                arr = (o["pares"] as JArray) ?? (o["matches"] as JArray) ?? (o["resultado"] as JArray);
            if (arr == null) return res;

            foreach (var item in arr)
            {
                int oi = item["oc"]?.ToObject<int>() ?? -1;
                int fi = item["factura"]?.ToObject<int>() ?? -1;
                double conf = item["confianza"]?.ToObject<double>() ?? 0.0;
                if (conf < 0.6) continue;
                if (oi < 0 || oi >= oc.Count) continue;
                if (fi < 0 || fi >= fc.Count) continue;
                res.Add(new ParEmparejado
                {
                    IdDetalle = oc[oi].IdDetalle,
                    Concepto = fc[fi],
                    Confianza = conf
                });
            }
            return res;
        }

        // ---- Ollama (local) ----
        /// <summary>
        /// POST a {OllamaUrl}/api/chat con format:"json" y temperature:0. Devuelve el texto
        /// JSON del modelo, o null + aviso si Ollama no responde / falla.
        /// </summary>
        private static string LlamarOllama(string prompt, out string aviso)
        {
            aviso = null;
            string url = (Configuracion.OllamaUrl ?? "http://localhost:11434").TrimEnd('/') + "/api/chat";
            string modelo = Configuracion.OllamaModel;

            string body = JsonConvert.SerializeObject(new
            {
                model = modelo,
                messages = new[] { new { role = "user", content = prompt } },
                stream = false,
                format = "json",
                options = new { temperature = 0 }
            });

            try
            {
                using (var content = new StringContent(body, Encoding.UTF8, "application/json"))
                using (var resp = Http.PostAsync(url, content).GetAwaiter().GetResult())
                {
                    string json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!resp.IsSuccessStatusCode)
                    {
                        aviso = "Emparejador automático no disponible (HTTP " + (int)resp.StatusCode + ")" +
                                ExtraerMensajeError(json) + "; se concilió solo lo determinista.";
                        return null;
                    }
                    var jo = JObject.Parse(json);
                    return jo["message"]?["content"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                aviso = "No se pudo conectar al emparejador automático en " + url +
                        " (¿está activo el servicio y disponible el modelo '" + modelo +
                        "'?): " + ex.Message + "; se concilió solo lo determinista.";
                return null;
            }
        }

        // ---- Gemini ----
        private static string LlamarGemini(string prompt, out string aviso)
        {
            aviso = null;
            string apiKey = Configuracion.GeminiApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                aviso = "Emparejador automático no configurado; se concilió solo lo determinista.";
                return null;
            }

            string body = JsonConvert.SerializeObject(new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0, responseMimeType = "application/json" }
            });

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{Configuracion.GeminiModel}:generateContent?key={apiKey}";

            // 429 (cuota) y 503 (sobrecarga) son transitorios en el free tier: backoff corto.
            int[] esperasMs = { 0, 1500, 3500 };
            System.Net.HttpStatusCode codigo = 0;
            string json = null;

            for (int intento = 0; intento < esperasMs.Length; intento++)
            {
                if (esperasMs[intento] > 0) System.Threading.Thread.Sleep(esperasMs[intento]);
                using (var content = new StringContent(body, Encoding.UTF8, "application/json"))
                using (var httpResp = Http.PostAsync(url, content).GetAwaiter().GetResult())
                {
                    codigo = httpResp.StatusCode;
                    json = httpResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (httpResp.IsSuccessStatusCode) break;
                    if (codigo != (System.Net.HttpStatusCode)429 &&
                        codigo != System.Net.HttpStatusCode.ServiceUnavailable) break;
                }
            }

            if (codigo != System.Net.HttpStatusCode.OK)
            {
                aviso = "Emparejador automático no disponible (HTTP " + (int)codigo + ")" + ExtraerMensajeError(json) +
                        "; se concilió solo lo determinista.";
                return null;
            }

            var jo = JObject.Parse(json);
            return jo["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
        }

        /// <summary>Extrae el mensaje de error del cuerpo JSON (Gemina: error.message; Ollama: error).</summary>
        private static string ExtraerMensajeError(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json)) return "";
                var jo = JObject.Parse(json);
                // Ollama devuelve {"error":"texto"}; Gemini {"error":{"message":"texto"}}.
                string msg = (jo["error"] as JObject)?["message"]?.ToString();
                if (string.IsNullOrWhiteSpace(msg) && jo["error"]?.Type == JTokenType.String)
                    msg = jo["error"].ToString();
                if (string.IsNullOrWhiteSpace(msg)) return "";
                if (msg.Length > 160) msg = msg.Substring(0, 160) + "...";
                return ": " + msg;
            }
            catch { return ""; }
        }
    }
}
