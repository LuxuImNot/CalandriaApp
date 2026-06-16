-- =============================================
-- Script SIMPLIFICADO para importar presupuesto
-- Solo categorías y costos totales (sin partidas individuales)
-- =============================================

-- 1. Crear tabla simplificada
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PresupuestoObra')
BEGIN
    DROP TABLE PresupuestoObra
    PRINT '?? Tabla PresupuestoObra anterior eliminada'
END

CREATE TABLE PresupuestoObra (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Categoria VARCHAR(100) NOT NULL,
    CostoTotal MONEY NOT NULL,
    Prototipo VARCHAR(50) NOT NULL,
    FechaCreacion DATETIME DEFAULT GETDATE()
)

CREATE INDEX IX_Prototipo ON PresupuestoObra(Prototipo)
CREATE INDEX IX_Categoria ON PresupuestoObra(Categoria)

PRINT '? Tabla PresupuestoObra creada (versión simplificada)'

-- 2. Crear tabla temporal para importar desde Excel
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TempExcel')
BEGIN
    DROP TABLE TempExcel
    PRINT '?? Tabla TempExcel anterior eliminada'
END

CREATE TABLE TempExcel (
    Padre VARCHAR(100),
    Etapa VARCHAR(100),
    Partida VARCHAR(500),
    CostoCalandra MONEY,
    CostoTunera MONEY
)

PRINT '? Tabla TempExcel creada'
PRINT ''
PRINT '==========================================='
PRINT '?? SIGUIENTE PASO: IMPORTAR EXCEL'
PRINT '==========================================='
PRINT ''
PRINT '1. En SQL Server Management Studio:'
PRINT '   - Clic derecho en tu base de datos'
PRINT '   - Tasks ? Import Data'
PRINT ''
PRINT '2. Data Source: Microsoft Excel'
PRINT '   - Selecciona tu archivo Excel'
PRINT '   - First row has column names: ?'
PRINT ''
PRINT '3. Destination: SQL Server Native Client'
PRINT '   - Table: [dbo].[TempExcel]'
PRINT ''
PRINT '4. Mapea las columnas:'
PRINT '   Excel ? SQL'
PRINT '   Padre ? Padre'
PRINT '   Etapa ? Etapa'
PRINT '   Partida ? Partida'
PRINT '   CostoCalandra ? CostoCalandra'
PRINT '   CostoTunera ? CostoTunera'
PRINT ''
PRINT '5. Después de importar, ejecuta el siguiente script:'
PRINT '   (SQL_TRANSFORMAR_A_PRESUPUESTO.sql)'
PRINT '==========================================='
