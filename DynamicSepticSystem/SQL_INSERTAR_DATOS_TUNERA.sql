-- ============================================================================
-- SOLUCIÓN RÁPIDA: Insertar datos de presupuesto para TUNERA
-- ============================================================================

USE CALANDRIA;
GO

PRINT '?? Insertando datos de presupuesto para TUNERA...';
PRINT '';

-- ============================================================================
-- 1. VERIFICAR SI YA EXISTEN DATOS PARA TUNERA
-- ============================================================================
IF EXISTS (SELECT * FROM dbo.PresupuestoObra WHERE Prototipo = 'TUNERA')
BEGIN
    PRINT '?? Ya existen datos para el prototipo TUNERA';
    PRINT '';
    PRINT '¿Deseas REEMPLAZAR los datos existentes?';
    PRINT '';
    PRINT 'Opciones:';
    PRINT '  A) Eliminar datos existentes y volver a insertar (recomendado si están en $0.00)';
    PRINT '  B) Actualizar solo los que están en $0.00';
    PRINT '  C) Cancelar';
    PRINT '';
    PRINT 'Para continuar, descomenta una de las opciones abajo:';
    PRINT '';
    
    -- Opción A: ELIMINAR Y REINSERTAR (descomenta para usar)
    /*
    DELETE FROM dbo.PresupuestoObra WHERE Prototipo = 'TUNERA';
    PRINT '? Datos anteriores eliminados';
    */
    
    -- Opción B: ACTUALIZAR SOLO LOS CERO (descomenta para usar)
    /*
    UPDATE dbo.PresupuestoObra
    SET CostoTotal = CASE Categoria
        WHEN 'Preliminares' THEN 45000.00
        WHEN 'Cimentación' THEN 120000.00
        WHEN 'Muros Planta Baja' THEN 180000.00
        WHEN 'Muros Planta Alta' THEN 150000.00
        WHEN 'Entrepiso' THEN 95000.00
        WHEN 'Losas' THEN 110000.00
        WHEN 'Azotea' THEN 85000.00
        WHEN 'Inst. Hidráulica, Sanitaria y Gas LP' THEN 75000.00
        WHEN 'Inst. Eléctrica' THEN 65000.00
        WHEN 'Albañilería Planta Baja' THEN 55000.00
        WHEN 'Albañilería Planta Alta' THEN 50000.00
        WHEN 'Acabados Interiores' THEN 125000.00
        WHEN 'Acabados Exteriores' THEN 95000.00
        WHEN 'Herrería, Aluminio y Vidrio' THEN 70000.00
        WHEN 'Carpintería y Cancelería' THEN 60000.00
        WHEN 'Muebles y Accesorios' THEN 45000.00
        WHEN 'Obra Exterior' THEN 80000.00
        WHEN 'Urbanización' THEN 55000.00
        ELSE CostoTotal
    END
    WHERE Prototipo = 'TUNERA' 
      AND (CostoTotal IS NULL OR CostoTotal = 0);
    
    PRINT '? Costos actualizados para categorías en $0.00';
    */
END
ELSE
BEGIN
    PRINT '? No existen datos previos, insertando datos frescos...';
    PRINT '';
END

-- ============================================================================
-- 2. INSERTAR DATOS DE PRESUPUESTO PARA TUNERA
-- ============================================================================

-- IMPORTANTE: Ajusta estos valores según tu proyecto real
-- Estos son valores de ejemplo típicos para una casa tipo TUNERA

BEGIN TRY
    BEGIN TRANSACTION;
    
    -- Insertar o actualizar usando MERGE
    MERGE INTO dbo.PresupuestoObra AS target
    USING (VALUES
        ('TUNERA', 'Preliminares', 45000.00),
        ('TUNERA', 'Cimentación', 120000.00),
        ('TUNERA', 'Muros Planta Baja', 180000.00),
        ('TUNERA', 'Muros Planta Alta', 150000.00),
        ('TUNERA', 'Entrepiso', 95000.00),
        ('TUNERA', 'Losas', 110000.00),
        ('TUNERA', 'Azotea', 85000.00),
        ('TUNERA', 'Inst. Hidráulica, Sanitaria y Gas LP', 75000.00),
        ('TUNERA', 'Inst. Eléctrica', 65000.00),
        ('TUNERA', 'Albañilería Planta Baja', 55000.00),
        ('TUNERA', 'Albañilería Planta Alta', 50000.00),
        ('TUNERA', 'Acabados Interiores', 125000.00),
        ('TUNERA', 'Acabados Exteriores', 95000.00),
        ('TUNERA', 'Herrería, Aluminio y Vidrio', 70000.00),
        ('TUNERA', 'Carpintería y Cancelería', 60000.00),
        ('TUNERA', 'Muebles y Accesorios', 45000.00),
        ('TUNERA', 'Obra Exterior', 80000.00),
        ('TUNERA', 'Urbanización', 55000.00)
    ) AS source (Prototipo, Categoria, CostoTotal)
    ON target.Prototipo = source.Prototipo 
       AND target.Categoria = source.Categoria
    WHEN MATCHED AND (target.CostoTotal IS NULL OR target.CostoTotal = 0) THEN
        UPDATE SET CostoTotal = source.CostoTotal
    WHEN NOT MATCHED THEN
        INSERT (Prototipo, Categoria, CostoTotal)
        VALUES (source.Prototipo, source.Categoria, source.CostoTotal);
    
    COMMIT TRANSACTION;
    
    PRINT '? Datos insertados/actualizados exitosamente';
    PRINT '';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '? ERROR: ' + ERROR_MESSAGE();
    PRINT '';
END CATCH

-- ============================================================================
-- 3. VERIFICAR DATOS INSERTADOS
-- ============================================================================
PRINT '========================================';
PRINT '  ?? VERIFICACIÓN DE DATOS';
PRINT '========================================';
PRINT '';

SELECT 
    Categoria,
    CostoTotal,
    CAST(CostoTotal AS MONEY) AS CostoFormateado
FROM dbo.PresupuestoObra
WHERE Prototipo = 'TUNERA'
ORDER BY 
    CASE Categoria
        WHEN 'Preliminares' THEN 1
        WHEN 'Cimentación' THEN 2
        WHEN 'Muros Planta Baja' THEN 3
        WHEN 'Muros Planta Alta' THEN 4
        WHEN 'Entrepiso' THEN 5
        WHEN 'Losas' THEN 6
        WHEN 'Azotea' THEN 7
        WHEN 'Inst. Hidráulica, Sanitaria y Gas LP' THEN 8
        WHEN 'Inst. Eléctrica' THEN 9
        WHEN 'Albañilería Planta Baja' THEN 10
        WHEN 'Albañilería Planta Alta' THEN 11
        WHEN 'Acabados Interiores' THEN 12
        WHEN 'Acabados Exteriores' THEN 13
        WHEN 'Herrería, Aluminio y Vidrio' THEN 14
        WHEN 'Carpintería y Cancelería' THEN 15
        WHEN 'Muebles y Accesorios' THEN 16
        WHEN 'Obra Exterior' THEN 17
        WHEN 'Urbanización' THEN 18
        ELSE 99
    END;

PRINT '';

-- Total general
DECLARE @totalGeneral DECIMAL(18,2);
SELECT @totalGeneral = SUM(CostoTotal) 
FROM dbo.PresupuestoObra 
WHERE Prototipo = 'TUNERA';

PRINT '========================================';
PRINT 'TOTAL PRESUPUESTO TUNERA: ' + CAST(@totalGeneral AS NVARCHAR(20));
PRINT '========================================';
PRINT '';

-- ============================================================================
-- 4. VERIFICAR CASA EN INVENTARIO
-- ============================================================================
PRINT '========================================';
PRINT '  ?? VERIFICACIÓN DE CASA';
PRINT '========================================';
PRINT '';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.InventarioCasas') AND type in (N'U'))
BEGIN
    PRINT 'Casa M5-L45:';
    SELECT Manzana, Lote, Prototipo 
    FROM InventarioCasas 
    WHERE Manzana = '5' AND Lote = '45';
    
    -- Verificar si el prototipo coincide
    DECLARE @prototipoActual NVARCHAR(50);
    SELECT @prototipoActual = Prototipo 
    FROM InventarioCasas 
    WHERE Manzana = '5' AND Lote = '45';
    
    IF @prototipoActual = 'TUNERA'
    BEGIN
        PRINT '';
        PRINT '? El prototipo de la casa es TUNERA';
        PRINT '? Los costos deberían aparecer ahora en la aplicación';
    END
    ELSE IF @prototipoActual IS NULL OR @prototipoActual = ''
    BEGIN
        PRINT '';
        PRINT '?? La casa NO tiene prototipo asignado';
        PRINT '';
        PRINT '?? SOLUCIÓN: Ejecuta esto:';
        PRINT '';
        PRINT 'UPDATE InventarioCasas ';
        PRINT 'SET Prototipo = ''TUNERA'' ';
        PRINT 'WHERE Manzana = ''5'' AND Lote = ''45'';';
        PRINT '';
        
        -- Ofrecer actualizar automáticamente
        -- DESCOMENTA ESTA LÍNEA PARA ACTUALIZAR AUTOMÁTICAMENTE:
        -- UPDATE InventarioCasas SET Prototipo = 'TUNERA' WHERE Manzana = '5' AND Lote = '45';
    END
    ELSE
    BEGIN
        PRINT '';
        PRINT '?? La casa tiene prototipo: ' + @prototipoActual + ' (NO es TUNERA)';
        PRINT '';
        PRINT '?? OPCIONES:';
        PRINT '';
        PRINT 'Opción A: Cambiar el prototipo de la casa a TUNERA:';
        PRINT 'UPDATE InventarioCasas SET Prototipo = ''TUNERA'' WHERE Manzana = ''5'' AND Lote = ''45'';';
        PRINT '';
        PRINT 'Opción B: Insertar datos para el prototipo ' + @prototipoActual + ':';
        PRINT 'Copia este script y reemplaza ''TUNERA'' por ''' + @prototipoActual + '''';
    END
END

PRINT '';
PRINT '========================================';
PRINT '  ? PROCESO COMPLETADO';
PRINT '========================================';
PRINT '';
PRINT '?? SIGUIENTE PASO:';
PRINT '   1. Cierra la aplicación DynamicSepticSystem';
PRINT '   2. Vuelve a abrirla';
PRINT '   3. Ve a OBRA ? Avance de Obra por Casa';
PRINT '   4. Selecciona Manzana: 5, Lote: 45';
PRINT '   5. Haz clic en "Cargar Avance"';
PRINT '';
PRINT '   Deberías ver los costos correctos en lugar de $0.00';
PRINT '';

GO
