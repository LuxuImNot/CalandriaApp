-- ====================================================================
-- SCRIPT: Datos de Ejemplo para FormAvanceObra
-- PROPÓSITO: Crear presupuesto de ejemplo para probar el formulario
-- FECHA: Enero 2025
-- ====================================================================

USE CALANDRIA;
GO

-- ====================================================================
-- PASO 1: Verificar/Crear tabla PresupuestoObra
-- ====================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PresupuestoObra')
BEGIN
    CREATE TABLE PresupuestoObra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Prototipo NVARCHAR(50) NOT NULL,
        WBS NVARCHAR(50) NOT NULL,
        Concepto NVARCHAR(500) NOT NULL,
        Unidad NVARCHAR(20),
        Cantidad DECIMAL(18,2),
        PrecioUnitario DECIMAL(18,2),
        ImporteTotal DECIMAL(18,2)
    );
    PRINT '? Tabla PresupuestoObra creada';
END
ELSE
BEGIN
    PRINT '? Tabla PresupuestoObra ya existe';
END
GO

-- ====================================================================
-- PASO 2: Limpiar datos de ejemplo previos (opcional)
-- ====================================================================

-- Descomentar si quieres empezar desde cero
-- DELETE FROM PresupuestoObra WHERE Prototipo = 'EJEMPLO_TUNERA';

-- ====================================================================
-- PASO 3: Insertar Presupuesto de Ejemplo - Prototipo TUNERA
-- ====================================================================

-- 1. PRELIMINARES
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '1', '1. PRELIMINARES', 'LOTE', 1, 45000, 45000),
('EJEMPLO_TUNERA', '1.1', 'Limpieza y trazo', 'm²', 120, 35, 4200),
('EJEMPLO_TUNERA', '1.2', 'Excavaciones', 'm³', 45, 280, 12600),
('EJEMPLO_TUNERA', '1.3', 'Rellenos', 'm³', 30, 180, 5400),
('EJEMPLO_TUNERA', '1.4', 'Acarreos', 'Viaje', 12, 850, 10200),
('EJEMPLO_TUNERA', '1.5', 'Instalaciones provisionales', 'Lote', 1, 12600, 12600);

-- 2. CIMENTACIÓN
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '2', '2. CIMENTACIÓN', 'LOTE', 1, 120000, 120000),
('EJEMPLO_TUNERA', '2.1', 'Zapatas corridas', 'm³', 8.5, 4200, 35700),
('EJEMPLO_TUNERA', '2.2', 'Cadenas de desplante', 'm³', 6.2, 3800, 23560),
('EJEMPLO_TUNERA', '2.3', 'Contratrabes', 'm³', 5.8, 4100, 23780),
('EJEMPLO_TUNERA', '2.4', 'Firme de concreto', 'm²', 85, 420, 35700),
('EJEMPLO_TUNERA', '2.5', 'Impermeabilización', 'm²', 85, 75, 6375);

-- 3. MUROS PLANTA BAJA
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '3', '3. MUROS PLANTA BAJA', 'LOTE', 1, 180000, 180000),
('EJEMPLO_TUNERA', '3.1', 'Muro de block 15cm', 'm²', 165, 420, 69300),
('EJEMPLO_TUNERA', '3.2', 'Castillos 15x15', 'Pza', 24, 680, 16320),
('EJEMPLO_TUNERA', '3.3', 'Dalas de cerramiento', 'm', 68, 550, 37400),
('EJEMPLO_TUNERA', '3.4', 'Columnas estructurales', 'Pza', 8, 2850, 22800),
('EJEMPLO_TUNERA', '3.5', 'Trabes', 'm', 32, 1070, 34240);

-- 4. ENTREPISO
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '4', '4. ENTREPISO', 'LOTE', 1, 95000, 95000),
('EJEMPLO_TUNERA', '4.1', 'Losa entrepiso 10cm', 'm²', 75, 850, 63750),
('EJEMPLO_TUNERA', '4.2', 'Escaleras', 'Pza', 1, 18500, 18500),
('EJEMPLO_TUNERA', '4.3', 'Cimbra y descimbra', 'm²', 75, 170, 12750);

-- 5. MUROS PLANTA ALTA
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '5', '5. MUROS PLANTA ALTA', 'LOTE', 1, 150000, 150000),
('EJEMPLO_TUNERA', '5.1', 'Muro de block 15cm', 'm²', 145, 420, 60900),
('EJEMPLO_TUNERA', '5.2', 'Castillos 15x15', 'Pza', 20, 680, 13600),
('EJEMPLO_TUNERA', '5.3', 'Dalas de cerramiento', 'm', 62, 550, 34100),
('EJEMPLO_TUNERA', '5.4', 'Pretiles', 'm', 28, 480, 13440),
('EJEMPLO_TUNERA', '5.5', 'Refuerzos estructurales', 'Lote', 1, 27960, 27960);

-- 6. LOSA AZOTEA
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '6', '6. LOSA AZOTEA', 'LOTE', 1, 85000, 85000),
('EJEMPLO_TUNERA', '6.1', 'Losa maciza 10cm', 'm²', 80, 850, 68000),
('EJEMPLO_TUNERA', '6.2', 'Cimbra y descimbra', 'm²', 80, 170, 13600),
('EJEMPLO_TUNERA', '6.3', 'Impermeabilización', 'm²', 80, 120, 9600);

-- 7. INSTALACIONES
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '7', '7. INSTALACIONES', 'LOTE', 1, 140000, 140000),
('EJEMPLO_TUNERA', '7.1', 'Instalación hidráulica', 'Lote', 1, 35000, 35000),
('EJEMPLO_TUNERA', '7.2', 'Instalación sanitaria', 'Lote', 1, 28000, 28000),
('EJEMPLO_TUNERA', '7.3', 'Instalación eléctrica', 'Lote', 1, 52000, 52000),
('EJEMPLO_TUNERA', '7.4', 'Instalación de gas', 'Lote', 1, 15000, 15000),
('EJEMPLO_TUNERA', '7.5', 'Cisterna y tinacos', 'Lote', 1, 10000, 10000);

-- 8. ALBAÑILERÍA
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '8', '8. ALBAÑILERÍA', 'LOTE', 1, 105000, 105000),
('EJEMPLO_TUNERA', '8.1', 'Aplanados interiores', 'm²', 320, 145, 46400),
('EJEMPLO_TUNERA', '8.2', 'Aplanados exteriores', 'm²', 180, 165, 29700),
('EJEMPLO_TUNERA', '8.3', 'Enjarres', 'm²', 85, 95, 8075),
('EJEMPLO_TUNERA', '8.4', 'Chaflanes', 'm', 120, 55, 6600),
('EJEMPLO_TUNERA', '8.5', 'Firmes en baños', 'm²', 18, 420, 7560),
('EJEMPLO_TUNERA', '8.6', 'Registros', 'Pza', 8, 320, 2560);

-- 9. ACABADOS
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '9', '9. ACABADOS', 'LOTE', 1, 220000, 220000),
('EJEMPLO_TUNERA', '9.1', 'Pisos cerámicos', 'm²', 95, 580, 55100),
('EJEMPLO_TUNERA', '9.2', 'Azulejos baños', 'm²', 42, 650, 27300),
('EJEMPLO_TUNERA', '9.3', 'Pintura vinílica', 'm²', 450, 85, 38250),
('EJEMPLO_TUNERA', '9.4', 'Pintura exterior', 'm²', 180, 125, 22500),
('EJEMPLO_TUNERA', '9.5', 'Muebles de baño', 'Juego', 2, 12500, 25000),
('EJEMPLO_TUNERA', '9.6', 'Muebles de cocina', 'Juego', 1, 38000, 38000),
('EJEMPLO_TUNERA', '9.7', 'Closets', 'Pza', 3, 4450, 13350);

-- 10. HERRERÍA Y CARPINTERÍA
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '10', '10. HERRERÍA Y CARPINTERÍA', 'LOTE', 1, 115000, 115000),
('EJEMPLO_TUNERA', '10.1', 'Ventanas de aluminio', 'm²', 18, 2200, 39600),
('EJEMPLO_TUNERA', '10.2', 'Puertas principales', 'Pza', 1, 12500, 12500),
('EJEMPLO_TUNERA', '10.3', 'Puertas interiores', 'Pza', 7, 3200, 22400),
('EJEMPLO_TUNERA', '10.4', 'Puerta cochera', 'Pza', 1, 18500, 18500),
('EJEMPLO_TUNERA', '10.5', 'Barandales', 'm', 12, 950, 11400),
('EJEMPLO_TUNERA', '10.6', 'Cancelería baños', 'Pza', 2, 5300, 10600);

-- 11. OBRA EXTERIOR
INSERT INTO PresupuestoObra (Prototipo, WBS, Concepto, Unidad, Cantidad, PrecioUnitario, ImporteTotal)
VALUES 
('EJEMPLO_TUNERA', '11', '11. OBRA EXTERIOR', 'LOTE', 1, 80000, 80000),
('EJEMPLO_TUNERA', '11.1', 'Banquetas', 'm²', 35, 420, 14700),
('EJEMPLO_TUNERA', '11.2', 'Rampa de acceso', 'm²', 12, 550, 6600),
('EJEMPLO_TUNERA', '11.3', 'Jardinería', 'm²', 45, 280, 12600),
('EJEMPLO_TUNERA', '11.4', 'Patio de servicio', 'm²', 18, 380, 6840),
('EJEMPLO_TUNERA', '11.5', 'Cercado perimetral', 'm', 42, 920, 38640);

GO

-- ====================================================================
-- PASO 4: Verificar Datos Insertados
-- ====================================================================

PRINT '====================================================================';
PRINT 'RESUMEN DE PRESUPUESTO INSERTADO';
PRINT '====================================================================';

SELECT 
    WBS,
    Concepto,
    ImporteTotal,
    Unidad,
    Cantidad
FROM PresupuestoObra
WHERE Prototipo = 'EJEMPLO_TUNERA'
ORDER BY WBS;

PRINT '';
PRINT '====================================================================';
PRINT 'TOTALES POR CATEGORÍA';
PRINT '====================================================================';

SELECT 
    LEFT(WBS, CHARINDEX('.', WBS + '.') - 1) AS Categoria,
    SUM(ImporteTotal) AS Total
FROM PresupuestoObra
WHERE Prototipo = 'EJEMPLO_TUNERA'
  AND LEN(WBS) - LEN(REPLACE(WBS, '.', '')) = 0  -- Solo categorías principales
GROUP BY LEFT(WBS, CHARINDEX('.', WBS + '.') - 1)
ORDER BY Categoria;

PRINT '';
PRINT '====================================================================';
PRINT 'TOTAL GENERAL DEL PRESUPUESTO';
PRINT '====================================================================';

SELECT 
    Prototipo,
    COUNT(*) AS TotalPartidas,
    SUM(ImporteTotal) AS TotalPresupuesto
FROM PresupuestoObra
WHERE Prototipo = 'EJEMPLO_TUNERA'
GROUP BY Prototipo;

GO

-- ====================================================================
-- PASO 5: Asignar Prototipo de Ejemplo a una Casa de Prueba
-- ====================================================================

-- Actualizar una casa existente para que use este prototipo
-- AJUSTA LA MANZANA Y LOTE SEGÚN TUS DATOS

UPDATE InventarioCasas 
SET Prototipo = 'EJEMPLO_TUNERA' 
WHERE Manzana = 'M5' AND Lote = 'L45';

-- Verificar
SELECT Manzana, Lote, Prototipo 
FROM InventarioCasas 
WHERE Prototipo = 'EJEMPLO_TUNERA';

PRINT '';
PRINT '====================================================================';
PRINT '? DATOS DE EJEMPLO CREADOS EXITOSAMENTE';
PRINT '====================================================================';
PRINT 'Prototipo: EJEMPLO_TUNERA';
PRINT 'Total Presupuesto: $1,535,000.00';
PRINT 'Total Partidas: 62';
PRINT '';
PRINT '?? PRÓXIMOS PASOS:';
PRINT '1. Abre la aplicación DynamicSepticSystem';
PRINT '2. Ve a: OBRA ? Avance de Obra por Casa';
PRINT '3. Selecciona la casa con prototipo EJEMPLO_TUNERA';
PRINT '4. Haz clic en "Cargar Avance"';
PRINT '5. Edita los porcentajes y prueba las gráficas';
PRINT '====================================================================';
GO
