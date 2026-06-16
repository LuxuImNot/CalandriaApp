
-- ?? DIAGNÓSTICO: Verificar conceptos guardados para M5-L2
-- Base de datos: CALANDRIA

USE CALANDRIA;
GO

-- 1?? Ver TODOS los registros de AvanceManualObra para esta casa
SELECT 
    Manzana,
    Lote,
    WBS,
    CAST(WBS AS INT) AS WBS_Numero,
    Concepto,
    AvancePorcentaje,
    MontoEjecutado,
    FechaFinalizacion,
    FechaActualizacion,
    CASE 
        WHEN TRY_CAST(WBS AS INT) < 0 THEN '?? CONCEPTO'
        WHEN TRY_CAST(WBS AS INT) > 0 THEN '?? PARTIDA'
        ELSE '? DESCONOCIDO'
    END AS TipoRegistro
FROM dbo.AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
ORDER BY TRY_CAST(WBS AS INT);

-- 2?? Contar conceptos (WBS negativos) completados
SELECT 
    COUNT(*) AS TotalConceptosGuardados,
    COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS ConceptosCompletados
FROM dbo.AvanceManualObra
WHERE Manzana = '5' 
  AND Lote = '2'
  AND TRY_CAST(WBS AS INT) < 0;

-- 3?? Listar solo los conceptos (WBS negativos)
SELECT 
    WBS,
    Concepto,
    AvancePorcentaje,
    MontoEjecutado,
    FechaFinalizacion,
    FechaActualizacion
FROM dbo.AvanceManualObra
WHERE Manzana = '5' 
  AND Lote = '2'
  AND TRY_CAST(WBS AS INT) < 0
ORDER BY TRY_CAST(WBS AS INT);

-- 4?? Ver partidas completadas (para verificar si hay conceptos que deberían estar completos)
SELECT 
    WBS,
    Concepto,
    AvancePorcentaje,
    FechaFinalizacion
FROM dbo.AvanceManualObra
WHERE Manzana = '5' 
  AND Lote = '2'
  AND TRY_CAST(WBS AS INT) > 0
  AND AvancePorcentaje = 100
ORDER BY TRY_CAST(WBS AS INT);

-- 5?? DIAGNÓSTICO: ¿Por qué no se ven los conceptos?
-- Posibles causas:
-- a) No hay registros con WBS negativos en la tabla
-- b) Los conceptos no están al 100%
-- c) El PanelPrincipal no está leyendo correctamente

-- Verificar si existe la columna WBS
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AvanceManualObra' 
  AND COLUMN_NAME = 'WBS';

-- 6?? SOLUCIÓN TEMPORAL: Si NO hay conceptos guardados, insertar manualmente para probar
-- NOTA: Solo ejecutar si la consulta 3?? no devuelve resultados

/*
-- Ejemplo: Insertar concepto "Preliminares" completado
INSERT INTO dbo.AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, AvancePorcentaje, MontoEjecutado, FechaFinalizacion)
VALUES ('5', '2', 'TUNERA', '-1', 'Preliminares', 100.0, 1980.36, GETDATE());

-- Ejemplo: Insertar concepto "Cimentación" completado
INSERT INTO dbo.AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, AvancePorcentaje, MontoEjecutado, FechaFinalizacion)
VALUES ('5', '2', 'TUNERA', '-2', 'Cimentación', 100.0, 38359.65, GETDATE());
*/

-- 7?? Verificar el cálculo que hace PanelPrincipal
SELECT 
    COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS ConceptosCompletados,
    12 AS TotalConceptos,
    CAST(COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS FLOAT) / 12 * 100 AS PorcentajeProgreso
FROM dbo.AvanceManualObra
WHERE Manzana = '5' 
  AND Lote = '2'
  AND TRY_CAST(WBS AS INT) < 0;

-- 8?? Ver estructura completa de la tabla
SELECT TOP 5 * FROM dbo.AvanceManualObra WHERE Manzana = '5' AND Lote = '2';
