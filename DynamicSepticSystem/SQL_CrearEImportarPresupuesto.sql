-- =============================================
-- Script para crear e importar datos de PresupuestoObra
-- Prototipos: CALANDRA y TUNERA
-- Fecha: 2025-01-11
-- =============================================

-- 1. Crear la tabla si no existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PresupuestoObra')
BEGIN
    CREATE TABLE PresupuestoObra (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Categoria VARCHAR(100) NOT NULL,
        Subcategoria VARCHAR(100),
        Descripcion VARCHAR(500),
        Unidad VARCHAR(50),
        Cantidad DECIMAL(18,4),
        PrecioUnitario MONEY,
        CostoTotal MONEY,
        Prototipo VARCHAR(50) NOT NULL,
        FechaCreacion DATETIME DEFAULT GETDATE()
    )
    
    CREATE INDEX IX_Prototipo ON PresupuestoObra(Prototipo)
    CREATE INDEX IX_Categoria ON PresupuestoObra(Categoria)
    
    PRINT '? Tabla PresupuestoObra creada correctamente'
END
ELSE
BEGIN
    PRINT '?? La tabla PresupuestoObra ya existe'
END

-- 2. Verificar si ya hay datos
DECLARE @TotalRegistros INT
SELECT @TotalRegistros = COUNT(*) FROM PresupuestoObra

IF @TotalRegistros > 0
BEGIN
    PRINT ''
    PRINT '?? ADVERTENCIA: La tabla ya contiene ' + CAST(@TotalRegistros AS VARCHAR(10)) + ' registros'
    PRINT ''
    PRINT 'Si quieres reemplazar los datos, descomenta y ejecuta:'
    PRINT '-- TRUNCATE TABLE PresupuestoObra'
    PRINT ''
    RETURN
END

PRINT ''
PRINT '==========================================='
PRINT '?? INSTRUCCIONES PARA IMPORTAR DESDE EXCEL'
PRINT '==========================================='
PRINT ''
PRINT '1. Abre tu archivo de Excel con los presupuestos'
PRINT ''
PRINT '2. Asegúrate de que tenga estas columnas:'
PRINT '   - Categoria'
PRINT '   - Subcategoria'
PRINT '   - Descripcion'
PRINT '   - Unidad'
PRINT '   - Cantidad'
PRINT '   - PrecioUnitario'
PRINT '   - CostoTotal'
PRINT '   - Prototipo (CALANDRA o TUNERA)'
PRINT ''
PRINT '3. OPCIÓN A: Importar desde Excel usando SQL Server Management Studio'
PRINT '   a) Clic derecho en la base de datos'
PRINT '   b) Tasks ? Import Data'
PRINT '   c) Selecciona tu archivo Excel'
PRINT '   d) Mapea las columnas a la tabla PresupuestoObra'
PRINT ''
PRINT '4. OPCIÓN B: Usar script de PowerShell (más rápido)'
PRINT '   Ver: SCRIPT_ImportarExcelASQL.ps1'
PRINT ''
PRINT '5. OPCIÓN C: Copiar desde Excel y pegar aquí (hasta 1000 filas)'
PRINT '   a) Selecciona tus datos en Excel'
PRINT '   b) Copia (Ctrl+C)'
PRINT '   c) Ejecuta el INSERT generado'
PRINT ''
PRINT '==========================================='

-- 3. Template de INSERT (ejemplo con 3 registros)
PRINT ''
PRINT '?? Ejemplo de INSERT (adapta con tus datos):'
PRINT ''

/*
INSERT INTO PresupuestoObra (Categoria, Subcategoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES
-- CALANDRA - Preliminares
('Preliminares', NULL, 'Limpieza del terreno', 'm2', 150.000, 15.50, 2325.00, 'CALANDRA'),
('Preliminares', NULL, 'Trazo y nivelación', 'm2', 150.000, 25.32, 3798.00, 'CALANDRA'),
('Preliminares', NULL, 'Excavación y relleno', 'm3', 35.000, 30.02, 2100.62, 'CALANDRA'),

-- CALANDRA - Cimentación
('Cimentación', NULL, 'Excavación para zapatas', 'm3', 45.000, 285.50, 12847.50, 'CALANDRA'),
('Cimentación', NULL, 'Plantilla en zapatas', 'm2', 28.000, 175.80, 4922.40, 'CALANDRA'),

-- TUNERA - Preliminares
('Preliminares', NULL, 'Limpieza del terreno', 'm2', 150.000, 15.50, 2325.00, 'TUNERA'),
('Preliminares', NULL, 'Trazo y nivelación', 'm2', 150.000, 25.32, 3798.00, 'TUNERA'),

-- ... continúa con todos tus datos
*/

-- 4. Verificar importación
PRINT ''
PRINT '==========================================='
PRINT '? DESPUÉS DE IMPORTAR, EJECUTA ESTO:'
PRINT '==========================================='
PRINT ''

SELECT 
    Prototipo,
    COUNT(*) AS TotalPartidas,
    SUM(CostoTotal) AS PresupuestoTotal
FROM PresupuestoObra
GROUP BY Prototipo

PRINT ''
PRINT 'Esperado:'
PRINT 'CALANDRA  | ~275 partidas | ~$2,662,190'
PRINT 'TUNERA    | ~275 partidas | ~$2,723,465'
