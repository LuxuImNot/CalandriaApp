-- DIAGNÓSTICO: Ver todas las partidas y sus padres
-- Ejecuta esto para ver los nombres exactos en tu base de datos

SELECT DISTINCT 
    Padre,
    COUNT(*) as CantidadPartidas
FROM PresupuestoObra
GROUP BY Padre
ORDER BY Padre;

-- Ver partidas de instalaciones hidráulicas/sanitarias
SELECT 
    Padre,
    Etapa,
    Partida,
    CostoCalandra,
    CostoTunera
FROM PresupuestoObra
WHERE Padre LIKE '%hidr%' 
   OR Padre LIKE '%sanit%'
   OR Padre LIKE '%gas%'
   OR Padre LIKE '%Inst%'
ORDER BY Padre, Etapa, Partida;

-- Ver todos los conceptos
SELECT 
    Codigo,
    Concepto,
    CostoCalandra,
    CostoTunera
FROM [Estimacion(Concepto)]
ORDER BY Codigo;
