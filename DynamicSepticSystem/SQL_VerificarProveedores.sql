-- Script de verificación y mantenimiento para el sistema de proveedores CALANDRIA
-- Ejecutar en SQL Server Management Studio

-- ====================================================================================
-- 1. VERIFICAR ESTRUCTURA DE TABLA PROVEEDORESCALANDRIA
-- ====================================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PROVEEDORESCALANDRIA') AND type in (N'U'))
BEGIN
    PRINT '? La tabla PROVEEDORESCALANDRIA existe'
    
    -- Mostrar estructura
    SELECT 
        COLUMN_NAME AS Columna,
        DATA_TYPE AS TipoDato,
        CHARACTER_MAXIMUM_LENGTH AS Longitud,
        IS_NULLABLE AS Permite_NULL
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PROVEEDORESCALANDRIA'
    ORDER BY ORDINAL_POSITION
    
    -- Contar registros
    SELECT COUNT(*) AS TotalProveedores FROM PROVEEDORESCALANDRIA
    
    -- Mostrar proveedores existentes
    SELECT * FROM PROVEEDORESCALANDRIA ORDER BY ClaveUnica
END
ELSE
BEGIN
    PRINT '? La tabla PROVEEDORESCALANDRIA NO existe.'
    PRINT '   Se creará automáticamente al abrir FormAgregarProveedor'
END

GO

-- ====================================================================================
-- 2. AGREGAR COLUMNA ProveedorClave A OrdenesCompra (SI NO EXISTE)
-- ====================================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.OrdenesCompra') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.OrdenesCompra') AND name = 'ProveedorClave')
    BEGIN
        ALTER TABLE OrdenesCompra
        ADD ProveedorClave NVARCHAR(50) NULL
        
        PRINT '? Columna ProveedorClave agregada a OrdenesCompra'
    END
    ELSE
    BEGIN
        PRINT '? La columna ProveedorClave ya existe en OrdenesCompra'
    END
END

GO

-- ====================================================================================
-- 3. (OPCIONAL) INSERTAR PROVEEDORES DE EJEMPLO
-- ====================================================================================
-- Descomenta las siguientes líneas para insertar datos de prueba

/*
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PROVEEDORESCALANDRIA') AND type in (N'U'))
BEGIN
    -- Limpiar proveedores de ejemplo anteriores (opcional)
    -- DELETE FROM PROVEEDORESCALANDRIA WHERE ClaveUnica LIKE 'PROV-TEST-%'
    
    -- Insertar proveedores de prueba
    IF NOT EXISTS (SELECT 1 FROM PROVEEDORESCALANDRIA WHERE ClaveUnica = 'PROV-001')
    BEGIN
        INSERT INTO PROVEEDORESCALANDRIA (ClaveUnica, Nombre, RFC, Direccion, Telefono)
        VALUES 
            ('PROV-001', 'Materiales de Construcción del Norte S.A. de C.V.', 'MCN930415ABC', 
             'Blvd. Luis Donaldo Colosio 123, Col. Centro, Hermosillo, Sonora', '662-123-4567'),
            
            ('PROV-002', 'Ferretería y Acabados Industriales S.A.', 'FAI850220XYZ', 
             'Calle Revolución 456, Col. Industrial, Hermosillo, Sonora', '662-234-5678'),
            
            ('PROV-003', 'Suministros Eléctricos del Pacífico', 'SEP920815LMN', 
             'Av. Tecnológico 789, Col. San Benito, Hermosillo, Sonora', '662-345-6789'),
            
            ('PROV-004', 'Plomería y Sanitarios Profesionales', 'PSP880330PQR', 
             'Calle Progreso 321, Col. Villa de Seris, Hermosillo, Sonora', '662-456-7890'),
            
            ('PROV-005', 'Pinturas y Recubrimientos del Desierto', 'PRD910725STU', 
             'Blvd. Solidaridad 654, Col. Pimentel, Hermosillo, Sonora', '662-567-8901')
        
        PRINT '? Se insertaron 5 proveedores de prueba'
    END
    ELSE
    BEGIN
        PRINT '? Ya existen proveedores, no se insertaron datos de prueba'
    END
END
*/

GO

-- ====================================================================================
-- 4. CONSULTAS ÚTILES
-- ====================================================================================

-- Ver todos los proveedores con su información completa
SELECT 
    ClaveUnica AS Clave,
    Nombre,
    RFC,
    Direccion,
    Telefono,
    CONVERT(VARCHAR, FechaCreacion, 103) AS FechaRegistro
FROM PROVEEDORESCALANDRIA
ORDER BY Nombre

GO

-- Ver órdenes de compra con proveedor
SELECT 
    o.FolioOC,
    o.Fecha,
    o.TipoOrden,
    o.ProveedorClave,
    p.Nombre AS NombreProveedor,
    p.RFC
FROM OrdenesCompra o
LEFT JOIN PROVEEDORESCALANDRIA p ON o.ProveedorClave = p.ClaveUnica
ORDER BY o.Fecha DESC

GO

-- Ver órdenes de compra sin proveedor asignado
SELECT 
    FolioOC,
    Fecha,
    TipoOrden,
    Usuario
FROM OrdenesCompra
WHERE ProveedorClave IS NULL
ORDER BY Fecha DESC

GO

-- ====================================================================================
-- 5. MIGRACIÓN DE DATOS (SI EXISTE TABLA ANTIGUA "Proveedores")
-- ====================================================================================
/*
-- Si tienes una tabla antigua "Proveedores" con solo Codigo y Nombre,
-- puedes migrar los datos con este script (ajusta según tus necesidades)

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Proveedores') AND type in (N'U'))
AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PROVEEDORESCALANDRIA') AND type in (N'U'))
BEGIN
    INSERT INTO PROVEEDORESCALANDRIA (ClaveUnica, Nombre, RFC, Direccion, Telefono)
    SELECT 
        Codigo AS ClaveUnica,
        Nombre,
        'RFC000000XXX' AS RFC, -- RFC genérico, deberás actualizarlo manualmente
        '' AS Direccion,
        '' AS Telefono
    FROM Proveedores
    WHERE Codigo NOT IN (SELECT ClaveUnica FROM PROVEEDORESCALANDRIA)
    
    PRINT '? Datos migrados desde tabla Proveedores antigua'
END
*/

GO

-- ====================================================================================
-- 6. VALIDAR INTEGRIDAD
-- ====================================================================================

-- Verificar proveedores con RFC inválido o genérico
SELECT 
    ClaveUnica,
    Nombre,
    RFC,
    CASE 
        WHEN LEN(RFC) < 12 THEN '? RFC muy corto'
        WHEN LEN(RFC) > 13 THEN '? RFC muy largo'
        WHEN RFC LIKE 'RFC%' THEN '? RFC genérico'
        ELSE '? RFC válido'
    END AS EstadoRFC
FROM PROVEEDORESCALANDRIA
WHERE LEN(RFC) < 12 OR LEN(RFC) > 13 OR RFC LIKE 'RFC%'

GO

-- Verificar proveedores sin datos completos
SELECT 
    ClaveUnica,
    Nombre,
    CASE WHEN Direccion IS NULL OR Direccion = '' THEN '? Sin dirección' ELSE '?' END AS Direccion,
    CASE WHEN Telefono IS NULL OR Telefono = '' THEN '? Sin teléfono' ELSE '?' END AS Telefono
FROM PROVEEDORESCALANDRIA
WHERE (Direccion IS NULL OR Direccion = '') OR (Telefono IS NULL OR Telefono = '')

GO

-- ====================================================================================
-- 7. ESTADÍSTICAS
-- ====================================================================================
SELECT 
    'Total de Proveedores' AS Concepto,
    COUNT(*) AS Cantidad
FROM PROVEEDORESCALANDRIA

UNION ALL

SELECT 
    'Órdenes con Proveedor Asignado' AS Concepto,
    COUNT(*) AS Cantidad
FROM OrdenesCompra
WHERE ProveedorClave IS NOT NULL

UNION ALL

SELECT 
    'Órdenes sin Proveedor' AS Concepto,
    COUNT(*) AS Cantidad
FROM OrdenesCompra
WHERE ProveedorClave IS NULL

GO

PRINT '========================================='
PRINT 'Script de verificación completado'
PRINT '========================================='
