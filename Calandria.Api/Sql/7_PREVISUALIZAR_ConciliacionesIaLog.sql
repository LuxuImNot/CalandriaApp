-- ============================================================================
-- SOLO LECTURA. No modifica nada. Corre esto primero contra la BD de una obra
-- existente para confirmar si ya tiene la tabla ConciliacionesIaLog antes de
-- correr 8_APLICAR_ConciliacionesIaLog.sql en esa obra.
--
-- Qué hace: solo indica si la tabla ya existe. Nada más.
-- ============================================================================

SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ConciliacionesIaLog')
            THEN 'Ya existe: no hace falta correr 8_APLICAR_ConciliacionesIaLog.sql en esta obra.'
            ELSE 'No existe: hace falta correr 8_APLICAR_ConciliacionesIaLog.sql en esta obra.'
       END AS Estado;
