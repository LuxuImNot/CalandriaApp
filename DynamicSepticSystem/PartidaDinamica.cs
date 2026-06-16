using System;
using System.Linq;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase que representa una partida con datos dinámicos cargada desde la base de datos
    /// </summary>
    public class PartidaDinamica
    {
        public int WBS { get; set; }
        
        /// <summary>
        /// Código del concepto al que pertenece esta partida
        /// </summary>
        public string Codigo { get; set; }
        
        public string Padre { get; set; }
        public string Etapa { get; set; }
        public string Partida { get; set; }
        public double Costo { get; set; }
        public bool EsDinamica { get; set; }
        public double ValorM2Tunera { get; set; }
        public double ValorM2Calandra { get; set; }
        public double MetrosCuadrados { get; set; }
        
        /// <summary>
        /// Límite máximo de m² para partidas dinámicas (legacy).
        /// Si es 0 o negativo, no hay límite (usa todos los m² del proyecto).
        /// </summary>
        public double LimiteM2 { get; set; }
        
        /// <summary>
        /// Límite máximo de m² para Tunera en partidas dinámicas.
        /// Si es 0 o negativo, usa el límite legacy (LimiteM2).
        /// </summary>
        public double LimiteM2Tunera { get; set; }
        
        /// <summary>
        /// Límite máximo de m² para Calandra en partidas dinámicas.
        /// Si es 0 o negativo, usa el límite legacy (LimiteM2).
        /// </summary>
        public double LimiteM2Calandra { get; set; }
        
        /// <summary>
        /// Lista de prototipos a los que aplica esta partida.
        /// Almacenado como string separado por comas (ej: "TUNERA,CALANDRA").
        /// Si es null o vacío, aplica a todos los prototipos.
        /// </summary>
        public string PrototiposAplicables { get; set; }
        
        /// <summary>
        /// Lista de prototipos como array para facilitar el manejo
        /// </summary>
        public string[] PrototiposArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PrototiposAplicables))
                    return new[] { "TUNERA", "CALANDRA" }; // Por defecto aplica a todos
                
                return PrototiposAplicables
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().ToUpperInvariant())
                    .ToArray();
            }
        }
        
        /// <summary>
        /// Verifica si la partida aplica a un prototipo específico
        /// </summary>
        /// <param name="prototipo">Nombre del prototipo a verificar</param>
        /// <returns>True si la partida aplica al prototipo, False en caso contrario</returns>
        public bool AplicaAPrototipo(string prototipo)
        {
            if (string.IsNullOrWhiteSpace(prototipo))
                return true;
            
            if (string.IsNullOrWhiteSpace(PrototiposAplicables))
                return true; // Si no se especifica, aplica a todos
            
            string prototipoNormalizado = prototipo.Trim().ToUpperInvariant();
            return PrototiposArray.Contains(prototipoNormalizado);
        }
        
        /// <summary>
        /// Obtiene una descripción de los prototipos aplicables
        /// </summary>
        public string PrototiposDescripcion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PrototiposAplicables))
                    return "Todos";
                
                var prototipos = PrototiposArray;
                if (prototipos.Length == 0)
                    return "Todos";
                
                if (prototipos.Length == 1)
                    return $"Solo {prototipos[0]}";
                
                return string.Join(", ", prototipos);
            }
        }
        
        /// <summary>
        /// Obtiene el límite efectivo para un prototipo dado
        /// </summary>
        /// <param name="prototipo">Nombre del prototipo (TUNERA o CALANDRA)</param>
        /// <returns>Límite efectivo de m²</returns>
        public double ObtenerLimiteEfectivo(string prototipo)
        {
            if (string.IsNullOrEmpty(prototipo))
                return LimiteM2;
            
            if (prototipo.ToUpper().Contains("TUNERA"))
            {
                return LimiteM2Tunera > 0 ? LimiteM2Tunera : LimiteM2;
            }
            else
            {
                return LimiteM2Calandra > 0 ? LimiteM2Calandra : LimiteM2;
            }
        }
        
        /// <summary>
        /// Calcula el costo para un prototipo dado usando el límite si aplica
        /// </summary>
        /// <param name="valorM2">Valor por metro cuadrado</param>
        /// <param name="metrosCuadradosProyecto">Metros cuadrados totales del proyecto</param>
        /// <returns>Costo calculado</returns>
        public double CalcularCostoDinamico(double valorM2, double metrosCuadradosProyecto)
        {
            if (!EsDinamica || valorM2 <= 0)
                return Costo;
            
            // Aplicar límite si está definido
            double m2Efectivos = (LimiteM2 > 0 && metrosCuadradosProyecto > LimiteM2)
                ? LimiteM2
                : metrosCuadradosProyecto;
            
            return valorM2 * m2Efectivos;
        }
        
        /// <summary>
        /// Calcula el costo para un prototipo específico usando el límite del prototipo
        /// </summary>
        /// <param name="prototipo">Nombre del prototipo</param>
        /// <param name="metrosCuadradosProyecto">Metros cuadrados totales del proyecto</param>
        /// <returns>Costo calculado</returns>
        public double CalcularCostoDinamicoParaPrototipo(string prototipo, double metrosCuadradosProyecto)
        {
            if (!EsDinamica)
                return Costo;
            
            double valorM2 = prototipo.ToUpper().Contains("TUNERA") ? ValorM2Tunera : ValorM2Calandra;
            double limite = ObtenerLimiteEfectivo(prototipo);
            
            if (valorM2 <= 0)
                return Costo;
            
            // Aplicar límite si está definido
            double m2Efectivos = (limite > 0 && metrosCuadradosProyecto > limite)
                ? limite
                : metrosCuadradosProyecto;
            
            return valorM2 * m2Efectivos;
        }
    }
}
