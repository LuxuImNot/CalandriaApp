using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase que representa una partida dentro de un concepto
    /// Usado al agregar nuevos conceptos con sus partidas asociadas
    /// </summary>
    public class PartidaConcepto
    {
        /// <summary>
        /// ID único de la partida (WBS_Correcto de la BD)
        /// </summary>
        public int WBS { get; set; }
        
        /// <summary>
        /// Nombre de la etapa de la partida (ej: "Preliminares", "Cimentación")
        /// </summary>
        public string Etapa { get; set; }
        
        /// <summary>
        /// Nombre descriptivo de la partida
        /// </summary>
        public string Partida { get; set; }
        
        /// <summary>
        /// Costo de la partida para el prototipo Tunera
        /// </summary>
        public double CostoTunera { get; set; }
        
        /// <summary>
        /// Costo de la partida para el prototipo Calandra
        /// </summary>
        public double CostoCalandra { get; set; }
        
        /// <summary>
        /// Indica si esta partida usa cálculo dinámico por m²
        /// </summary>
        public bool EsDinamica { get; set; }
        
        /// <summary>
        /// Valor por m² para Tunera (cuando EsDinamica = true)
        /// </summary>
        public double ValorM2Tunera { get; set; }
        
        /// <summary>
        /// Valor por m² para Calandra (cuando EsDinamica = true)
        /// </summary>
        public double ValorM2Calandra { get; set; }
        
        /// <summary>
        /// NUEVO: Metros cuadrados específicos para esta partida (input del usuario)
        /// Solo aplica cuando EsDinamica = true
        /// </summary>
        public double MetrosCuadrados { get; set; }
        
        /// <summary>
        /// Límite máximo de m² para partidas dinámicas (LEGACY - para compatibilidad).
        /// Se usa cuando LimiteM2Tunera y LimiteM2Calandra no están definidos.
        /// Si es 0 o negativo, no hay límite.
        /// </summary>
        public double LimiteM2 { get; set; }
        
        /// <summary>
        /// Límite máximo de m² para Tunera en partidas dinámicas.
        /// Si es 0 o negativo, no hay límite.
        /// El costo se calculará con el mínimo entre m² del proyecto y este límite.
        /// </summary>
        public double LimiteM2Tunera { get; set; }
        
        /// <summary>
        /// Límite máximo de m² para Calandra en partidas dinámicas.
        /// Si es 0 o negativo, no hay límite.
        /// El costo se calculará con el mínimo entre m² del proyecto y este límite.
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
        /// Obtiene el límite efectivo para Tunera (considera el límite legacy si no hay específico)
        /// </summary>
        public double LimiteEfectivoTunera
        {
            get
            {
                // Si hay límite específico para Tunera, usarlo
                if (LimiteM2Tunera > 0)
                    return LimiteM2Tunera;
                
                // Si no, usar el límite legacy
                return LimiteM2;
            }
        }
        
        /// <summary>
        /// Obtiene el límite efectivo para Calandra (considera el límite legacy si no hay específico)
        /// </summary>
        public double LimiteEfectivoCalandra
        {
            get
            {
                // Si hay límite específico para Calandra, usarlo
                if (LimiteM2Calandra > 0)
                    return LimiteM2Calandra;
                
                // Si no, usar el límite legacy
                return LimiteM2;
            }
        }
        
        /// <summary>
        /// Calcula el costo total para Tunera según m² si es dinámica
        /// </summary>
        /// <param name="metrosCuadrados">Cantidad de metros cuadrados del proyecto</param>
        /// <returns>Costo calculado o costo fijo según tipo de partida</returns>
        public double CalcularCostoTunera(double metrosCuadrados)
        {
            if (!EsDinamica)
                return CostoTunera;
            
            // Aplicar límite específico de Tunera
            double limite = LimiteEfectivoTunera;
            double m2Efectivos = (limite > 0 && metrosCuadrados > limite) 
                ? limite 
                : metrosCuadrados;
            
            return ValorM2Tunera * m2Efectivos;
        }
        
        /// <summary>
        /// Calcula el costo total para Calandra según m² si es dinámica
        /// </summary>
        /// <param name="metrosCuadrados">Cantidad de metros cuadrados del proyecto</param>
        /// <returns>Costo calculado o costo fijo según tipo de partida</returns>
        public double CalcularCostoCalandra(double metrosCuadrados)
        {
            if (!EsDinamica)
                return CostoCalandra;
            
            // Aplicar límite específico de Calandra
            double limite = LimiteEfectivoCalandra;
            double m2Efectivos = (limite > 0 && metrosCuadrados > limite) 
                ? limite 
                : metrosCuadrados;
            
            return ValorM2Calandra * m2Efectivos;
        }
        
        /// <summary>
        /// NUEVO: Calcula el costo usando los m² almacenados en la partida
        /// </summary>
        public double CalcularCostoTuneraConM2()
        {
            return CalcularCostoTunera(MetrosCuadrados);
        }
        
        /// <summary>
        /// NUEVO: Calcula el costo usando los m² almacenados en la partida
        /// </summary>
        public double CalcularCostoCalandraConM2()
        {
            return CalcularCostoCalandra(MetrosCuadrados);
        }
        
        /// <summary>
        /// Obtiene una descripción del tipo de costo de la partida
        /// </summary>
        public string TipoCosto 
        { 
            get 
            { 
                if (!EsDinamica)
                    return "Fijo";
                
                double limTunera = LimiteEfectivoTunera;
                double limCalandra = LimiteEfectivoCalandra;
                
                // Si ambos límites son iguales o solo hay uno definido
                if (limTunera == limCalandra)
                {
                    return limTunera > 0 
                        ? $"Dinámico (máx {limTunera:N0} m²)" 
                        : "Dinámico ($/m²)";
                }
                
                // Si hay límites diferentes
                var partes = new List<string>();
                if (limTunera > 0)
                    partes.Add($"T:{limTunera:N0}");
                if (limCalandra > 0)
                    partes.Add($"C:{limCalandra:N0}");
                
                if (partes.Count > 0)
                    return $"Dinámico ({string.Join(" ", partes)} m²)";
                
                return "Dinámico ($/m²)";
            } 
        }
        
        /// <summary>
        /// Obtiene una descripción del límite de m² para mostrar en el grid
        /// </summary>
        public string LimiteM2Descripcion
        {
            get
            {
                if (!EsDinamica)
                    return "-";
                
                double limTunera = LimiteEfectivoTunera;
                double limCalandra = LimiteEfectivoCalandra;
                
                // Si no hay límites
                if (limTunera <= 0 && limCalandra <= 0)
                    return "Sin límite";
                
                // Si ambos límites son iguales
                if (Math.Abs(limTunera - limCalandra) < 0.01)
                    return $"{limTunera:N2}";
                
                // Si hay límites diferentes
                var partes = new List<string>();
                partes.Add(limTunera > 0 ? $"T:{limTunera:N0}" : "T:?");
                partes.Add(limCalandra > 0 ? $"C:{limCalandra:N0}" : "C:?");
                
                return string.Join(" ", partes);
            }
        }
    }
}
