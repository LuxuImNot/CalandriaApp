-- ============================================================================
-- Script para inicializar datos de prueba del Sistema de Avance de Obra
-- ============================================================================

USE CALANDRIA;
GO

-- ============================================================================
-- 1. CREAR TABLA PresupuestoObra (si no existe)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PresupuestoObra') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.PresupuestoObra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Categoria NVARCHAR(100) NOT NULL,
        Subcategoria NVARCHAR(200),
        Descripcion NVARCHAR(500),
        Unidad NVARCHAR(50),
        Cantidad DECIMAL(18,4),
        PrecioUnitario DECIMAL(18,2),
        CostoTotal DECIMAL(18,2),
        Prototipo NVARCHAR(50),
        FechaCreacion DATETIME DEFAULT GETDATE()
    );
    PRINT '? Tabla PresupuestoObra creada';
END
ELSE
    PRINT '- Tabla PresupuestoObra ya existe';
GO

-- ============================================================================
-- 2. AGREGAR COLUMNAS A TABLAS EXISTENTES (si no existen)
-- ============================================================================

-- Agregar columnas Manzana y Lote a OrdenesCompra
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.OrdenesCompra') AND name = 'Manzana')
BEGIN
    ALTER TABLE dbo.OrdenesCompra ADD Manzana NVARCHAR(50);
    PRINT '? Columna Manzana agregada a OrdenesCompra';
END
ELSE
    PRINT '- Columna Manzana ya existe en OrdenesCompra';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.OrdenesCompra') AND name = 'Lote')
BEGIN
    ALTER TABLE dbo.OrdenesCompra ADD Lote NVARCHAR(50);
    PRINT '? Columna Lote agregada a OrdenesCompra';
END
ELSE
    PRINT '- Columna Lote ya existe en OrdenesCompra';

-- Agregar columna Categoria a OrdenesCompraDetalle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.OrdenesCompraDetalle') AND name = 'Categoria')
BEGIN
    ALTER TABLE dbo.OrdenesCompraDetalle ADD Categoria NVARCHAR(100);
    PRINT '? Columna Categoria agregada a OrdenesCompraDetalle';
END
ELSE
    PRINT '- Columna Categoria ya existe en OrdenesCompraDetalle';
GO

-- ============================================================================
-- 3. INSERTAR DATOS DE PRESUPUESTO (basado en tu documento)
-- ============================================================================

PRINT '';
PRINT '?? Insertando presupuesto de obra...';

-- Limpiar datos anteriores de prueba (opcional)
-- DELETE FROM PresupuestoObra WHERE Prototipo = 'PRUEBA';

-- PRELIMINARES (Total: $8,223.62)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Preliminares', 'Limpieza del terreno', 'm2', 150, 15.50, 2325.00, 'PLANTA BAJA'),
    ('Preliminares', 'Trazo y nivelación', 'm2', 150, 25.32, 3798.00, 'PLANTA BAJA'),
    ('Preliminares', 'Excavación y relleno', 'm3', 35, 30.02, 2100.62, 'PLANTA BAJA');

-- CIMENTACIÓN (Total: $55,324.17)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Cimentación', 'Excavación para zapatas', 'm3', 45, 285.50, 12847.50, 'PLANTA BAJA'),
    ('Cimentación', 'Plantilla en zapatas', 'm2', 28, 175.80, 4922.40, 'PLANTA BAJA'),
    ('Cimentación', 'Concreto en zapatas', 'm3', 12, 3539.72, 42476.64, 'PLANTA BAJA'),
    ('Cimentación', 'Acero en zapatas', 'ton', 0.8, 15000.00, 12000.00, 'PLANTA BAJA');

-- MUROS PLANTA BAJA (Total: $61,540.18)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Muros Planta Baja', 'Block de 15x20x40cm', 'pza', 2500, 12.50, 31250.00, 'PLANTA BAJA'),
    ('Muros Planta Baja', 'Mortero para pegar block', 'm3', 8, 1200.00, 9600.00, 'PLANTA BAJA'),
    ('Muros Planta Baja', 'Castillos y dalas', 'ml', 120, 165.75, 19890.00, 'PLANTA BAJA'),
    ('Muros Planta Baja', 'Cadena cerramiento', 'ml', 45, 180.02, 8100.90, 'PLANTA BAJA');

-- MUROS PLANTA ALTA (Total: $30,197.35)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Muros Planta Alta', 'Block de 15x20x40cm', 'pza', 1200, 12.50, 15000.00, 'PLANTA ALTA'),
    ('Muros Planta Alta', 'Castillos y dalas', 'ml', 80, 165.75, 13260.00, 'PLANTA ALTA'),
    ('Muros Planta Alta', 'Pretiles', 'ml', 25, 145.89, 3647.25, 'PLANTA ALTA');

-- LOSAS (Total: $100,484.52)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Losas', 'Losa entrepiso', 'm2', 85, 850.50, 72292.50, 'PLANTA BAJA'),
    ('Losas', 'Losa azotea', 'm2', 35, 805.20, 28182.00, 'PLANTA ALTA');

-- AZOTEA (Total: $9,240.85)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Azotea', 'Impermeabilizante', 'm2', 35, 185.50, 6492.50, 'PLANTA ALTA'),
    ('Azotea', 'Pretiles y remates', 'ml', 28, 98.15, 2748.20, 'PLANTA ALTA');

-- INSTALACIONES HIDRÁULICAS (Total: $27,625.27)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Inst. Hidráulica, Sanitaria y Gas LP', 'Tubería PVC hidráulica', 'ml', 85, 125.50, 10667.50, 'PLANTA BAJA'),
    ('Inst. Hidráulica, Sanitaria y Gas LP', 'Tubería PVC sanitaria', 'ml', 65, 145.80, 9477.00, 'PLANTA BAJA'),
    ('Inst. Hidráulica, Sanitaria y Gas LP', 'Instalación Gas LP', 'salida', 3, 1826.92, 5480.76, 'PLANTA BAJA');

-- INSTALACIÓN ELÉCTRICA (Total: $33,915.84)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Inst. Eléctrica', 'Centro de carga', 'pza', 1, 2850.50, 2850.50, 'PLANTA BAJA'),
    ('Inst. Eléctrica', 'Apagadores y contactos', 'pza', 35, 185.50, 6492.50, 'PLANTA BAJA'),
    ('Inst. Eléctrica', 'Cableado eléctrico', 'ml', 250, 98.29, 24572.50, 'PLANTA BAJA');

-- ALBAÑILERÍA PLANTA BAJA (Total: $85,521.06)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Albañilería Planta Baja', 'Aplanado en muros', 'm2', 280, 125.50, 35140.00, 'PLANTA BAJA'),
    ('Albañilería Planta Baja', 'Aplanado en plafones', 'm2', 85, 145.80, 12393.00, 'PLANTA BAJA'),
    ('Albañilería Planta Baja', 'Firme de concreto', 'm2', 85, 445.60, 37876.00, 'PLANTA BAJA');

-- ALBAÑILERÍA PLANTA ALTA (Total: $57,220.55)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Albañilería Planta Alta', 'Aplanado en muros', 'm2', 180, 125.50, 22590.00, 'PLANTA ALTA'),
    ('Albañilería Planta Alta', 'Aplanado en plafones', 'm2', 85, 145.80, 12393.00, 'PLANTA ALTA'),
    ('Albañilería Planta Alta', 'Firme azotea', 'm2', 35, 636.50, 22277.50, 'PLANTA ALTA');

-- ACABADOS INTERIORES (Total: $206,013.17)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Acabados Interiores', 'Piso cerámico', 'm2', 120, 385.50, 46260.00, 'PLANTA BAJA'),
    ('Acabados Interiores', 'Azulejo en baños', 'm2', 45, 425.80, 19161.00, 'PLANTA BAJA'),
    ('Acabados Interiores', 'Pintura vinílica', 'm2', 450, 85.50, 38475.00, 'PLANTA BAJA'),
    ('Acabados Interiores', 'Pasta en muros', 'm2', 450, 55.20, 24840.00, 'PLANTA BAJA'),
    ('Acabados Interiores', 'Cancelería de aluminio', 'm2', 18, 1850.50, 33309.00, 'PLANTA BAJA'),
    ('Acabados Interiores', 'Puertas de madera', 'pza', 8, 5496.02, 43968.16, 'PLANTA BAJA');

-- ACABADOS EXTERIORES (Total: $9,074.29)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Acabados Exteriores', 'Pintura exterior', 'm2', 85, 95.50, 8117.50, 'PLANTA BAJA'),
    ('Acabados Exteriores', 'Remates y molduras', 'ml', 12, 79.73, 956.76, 'PLANTA BAJA');

-- HERRERÍA, ALUMINIO Y VIDRIO (Total: $9,363.12)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Herrería, Aluminio y Vidrio', 'Ventanas de aluminio', 'm2', 12, 585.50, 7026.00, 'PLANTA BAJA'),
    ('Herrería, Aluminio y Vidrio', 'Protecciones metálicas', 'pza', 6, 389.52, 2337.12, 'PLANTA BAJA');

-- CARPINTERÍA Y CANCELERÍA (Total: $21,676.37)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Carpintería y Cancelería', 'Puerta principal', 'pza', 1, 8500.00, 8500.00, 'PLANTA BAJA'),
    ('Carpintería y Cancelería', 'Puertas interiores', 'pza', 6, 2196.06, 13176.36, 'PLANTA BAJA');

-- MUEBLES Y ACCESORIOS (Total: $39,705.35)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Muebles y Accesorios', 'Mueble de cocina', 'ml', 4.5, 3850.50, 17327.25, 'PLANTA BAJA'),
    ('Muebles y Accesorios', 'Mueble de baño', 'pza', 2, 4896.50, 9793.00, 'PLANTA BAJA'),
    ('Muebles y Accesorios', 'Accesorios de baño', 'lote', 2, 6292.55, 12585.10, 'PLANTA BAJA');

-- OBRA EXTERIOR (Total: $21,653.13)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Obra Exterior', 'Banqueta perimetral', 'm2', 25, 385.50, 9637.50, 'PLANTA BAJA'),
    ('Obra Exterior', 'Jardineras', 'm2', 15, 496.38, 7445.70, 'PLANTA BAJA'),
    ('Obra Exterior', 'Portón de acceso', 'pza', 1, 4569.93, 4569.93, 'PLANTA BAJA');

-- URBANIZACIÓN (futuro)
INSERT INTO PresupuestoObra (Categoria, Descripcion, Unidad, Cantidad, PrecioUnitario, CostoTotal, Prototipo)
VALUES 
    ('Urbanización', 'Pendiente', 'lote', 0, 0.00, 0.00, 'PLANTA BAJA');

PRINT '? Presupuesto insertado exitosamente';
GO

-- ============================================================================
-- 4. INSERTAR ÓRDENES DE COMPRA DE PRUEBA (simulando avance)
-- ============================================================================

PRINT '';
PRINT '?? Insertando órdenes de compra de prueba...';

-- Asegurar que existen proveedores
IF NOT EXISTS (SELECT 1 FROM PROVEEDORESCALANDRIA WHERE ClaveUnica = 'PROV-TEST-001')
BEGIN
    INSERT INTO PROVEEDORESCALANDRIA (ClaveUnica, Nombre, RFC, Direccion, Telefono)
    VALUES ('PROV-TEST-001', 'Materiales del Norte SA', 'MDN980101ABC', 'Calle Principal #123', '662-123-4567');
    PRINT '? Proveedor de prueba creado';
END

-- Orden 1: Preliminares y Cimentación para M1-L1 (Casa con buen avance)
IF NOT EXISTS (SELECT 1 FROM OrdenesCompra WHERE FolioOC = 'OC-TEST-001')
BEGIN
    INSERT INTO OrdenesCompra (FolioOC, Fecha, Usuario, TipoOrden, ProveedorClave, Manzana, Lote)
    VALUES ('OC-TEST-001', GETDATE()-30, 'admin', 'MULTIPLE', 'PROV-TEST-001', 'M1', 'L1');

    INSERT INTO OrdenesCompraDetalle (FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Categoria)
    VALUES 
        ('OC-TEST-001', 'MAT-001', 'Limpieza de terreno', 'm2', 150, 15.50, 2325.00, 'Preliminares'),
        ('OC-TEST-001', 'MAT-002', 'Trazo y nivelación', 'm2', 150, 25.32, 3798.00, 'Preliminares'),
        ('OC-TEST-001', 'MAT-003', 'Excavación', 'm3', 45, 285.50, 12847.50, 'Cimentación'),
        ('OC-TEST-001', 'MAT-004', 'Concreto zapatas', 'm3', 12, 3539.72, 42476.64, 'Cimentación');
    
    PRINT '? Orden OC-TEST-001 creada (M1-L1: Preliminares y Cimentación)';
END

-- Orden 2: Muros Planta Baja para M1-L1
IF NOT EXISTS (SELECT 1 FROM OrdenesCompra WHERE FolioOC = 'OC-TEST-002')
BEGIN
    INSERT INTO OrdenesCompra (FolioOC, Fecha, Usuario, TipoOrden, ProveedorClave, Manzana, Lote)
    VALUES ('OC-TEST-002', GETDATE()-20, 'admin', 'MULTIPLE', 'PROV-TEST-001', 'M1', 'L1');

    INSERT INTO OrdenesCompraDetalle (FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Categoria)
    VALUES 
        ('OC-TEST-002', 'MAT-005', 'Block 15x20x40', 'pza', 2500, 12.50, 31250.00, 'Muros Planta Baja'),
        ('OC-TEST-002', 'MAT-006', 'Mortero', 'm3', 8, 1200.00, 9600.00, 'Muros Planta Baja'),
        ('OC-TEST-002', 'MAT-007', 'Castillos', 'ml', 120, 165.75, 19890.00, 'Muros Planta Baja');
    
    PRINT '? Orden OC-TEST-002 creada (M1-L1: Muros Planta Baja)';
END

-- Orden 3: Casa M1-L2 con menor avance
IF NOT EXISTS (SELECT 1 FROM OrdenesCompra WHERE FolioOC = 'OC-TEST-003')
BEGIN
    INSERT INTO OrdenesCompra (FolioOC, Fecha, Usuario, TipoOrden, ProveedorClave, Manzana, Lote)
    VALUES ('OC-TEST-003', GETDATE()-10, 'admin', 'MULTIPLE', 'PROV-TEST-001', 'M1', 'L2');

    INSERT INTO OrdenesCompraDetalle (FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Categoria)
    VALUES 
        ('OC-TEST-003', 'MAT-001', 'Limpieza', 'm2', 150, 15.50, 2325.00, 'Preliminares'),
        ('OC-TEST-003', 'MAT-003', 'Excavación', 'm3', 45, 285.50, 12847.50, 'Cimentación');
    
    PRINT '? Orden OC-TEST-003 creada (M1-L2: Preliminares y Cimentación parcial)';
END

-- Orden 4: Casa M2-L1 recién iniciada
IF NOT EXISTS (SELECT 1 FROM OrdenesCompra WHERE FolioOC = 'OC-TEST-004')
BEGIN
    INSERT INTO OrdenesCompra (FolioOC, Fecha, Usuario, TipoOrden, ProveedorClave, Manzana, Lote)
    VALUES ('OC-TEST-004', GETDATE()-5, 'admin', 'MULTIPLE', 'PROV-TEST-001', 'M2', 'L1');

    INSERT INTO OrdenesCompraDetalle (FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Categoria)
    VALUES 
        ('OC-TEST-004', 'MAT-001', 'Limpieza', 'm2', 150, 15.50, 2325.00, 'Preliminares');
    
    PRINT '? Orden OC-TEST-004 creada (M2-L1: Solo Preliminares)';
END

PRINT '? Órdenes de compra de prueba insertadas';
GO

-- ============================================================================
-- 5. VERIFICAR DATOS INSERTADOS
-- ============================================================================

PRINT '';
PRINT '========================================';
PRINT '  RESUMEN DE DATOS INSERTADOS';
PRINT '========================================';
PRINT '';

-- Contar presupuesto por categoría
PRINT '?? Presupuesto por Categoría:';
SELECT 
    Categoria,
    COUNT(*) AS Items,
    SUM(CostoTotal) AS TotalPresupuesto
FROM PresupuestoObra
GROUP BY Categoria
ORDER BY Categoria;

PRINT '';
PRINT '?? Órdenes de Compra:';
SELECT 
    FolioOC,
    Manzana,
    Lote,
    FORMAT(Fecha, 'dd/MM/yyyy') AS Fecha,
    ProveedorClave
FROM OrdenesCompra
WHERE FolioOC LIKE 'OC-TEST-%'
ORDER BY Manzana, Lote;

PRINT '';
PRINT '?? Total Ejecutado por Casa:';
SELECT 
    o.Manzana,
    o.Lote,
    COUNT(DISTINCT o.FolioOC) AS NumOrdenes,
    SUM(d.ImporteTotal) AS TotalEjecutado
FROM OrdenesCompra o
INNER JOIN OrdenesCompraDetalle d ON o.FolioOC = d.FolioOC
WHERE o.FolioOC LIKE 'OC-TEST-%'
GROUP BY o.Manzana, o.Lote
ORDER BY o.Manzana, o.Lote;

PRINT '';
PRINT '========================================';
PRINT '  ? SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '========================================';
PRINT '';
PRINT 'Ahora puedes abrir el FormAvanceObra y seleccionar:';
PRINT '  • Manzana: M1, M2';
PRINT '  • Lotes: L1, L2, L3, L4, L5';
PRINT '';
PRINT 'Casas con datos de prueba:';
PRINT '  • M1-L1: Avance ~80% (Preliminares, Cimentación, Muros completos)';
PRINT '  • M1-L2: Avance ~25% (Preliminares y Cimentación parcial)';
PRINT '  • M2-L1: Avance ~10% (Solo Preliminares)';
GO
