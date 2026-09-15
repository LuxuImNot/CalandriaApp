-- ============================================================================
-- Datos de ejemplo para el árbol de destajos en la BD de pruebas LOCAL (BD
-- CALANDRIA, ver README_BD_Local_Pruebas.md). Esas tablas quedaron vacías tras
-- aplicar ObraPlantilla.sql sobre el snapshot de jul-2025 (no existían todavía
-- en ese momento), así que Destajos / Administrativos / Nómina no tenían nada
-- que mostrar. Este script llena:
--   - RutaTuneraDestajo / RutaCalandraDestajo: catálogo de tareas (3 categorías
--     + tareas hoja cada una, mano de obra y material) — el mismo catálogo
--     compartido que usaría cualquier casa de ese prototipo.
--   - *_Columnas: Cantidad/Unidad/Precio de cada tarea hoja.
--   - ActivacionTareasRuta: activa un subconjunto del catálogo para 3 casas
--     reales de InventarioCasas (Mz1/Lt10 y Mz1/Lt11 — TUNERA — y Mz1/Lt15 —
--     CALANDRA), con estados variados (activado / con cuadrilla / finalizado)
--     para poder ver las tres reglas de negocio en acción.
--   - NominaTareasAsignada: nómina asignada a algunas de las tareas de mano de
--     obra (incluida una "con cuadrilla" pero sin finalizar, para mostrar que
--     el gasto de M.O. se reconoce al asignar nómina, no al finalizar).
--
-- OJO — ActivacionTareasRuta.NodoID tiene un FK (FK_ActivacionTareasRuta_NodoID)
-- que SIEMPRE apunta a RutaTuneraDestajo.ID, incluso para filas con
-- Ruta='RutaCalandraDestajo' (esquema heredado de ObraPlantilla.sql, no es algo
-- introducido aquí). Por eso el catálogo Calandra reusa los mismos números de ID
-- que ya existen en RutaTuneraDestajo (9001-9031) en vez de un rango propio: así
-- el FK encuentra el ID aunque la tabla "real" que describe la tarea sea
-- RutaCalandraDestajo (el JOIN de AdministrativosController usa la tabla correcta
-- según el prototipo, así que el contenido mostrado sigue siendo el de Calandra).
--
-- Ejecutar con: sqlcmd -S "localhost\SQLEXPRESS" -E -C -d CALANDRIA -i SQL_Local_SeedDestajos.sql
-- Idempotente: cada INSERT está guardado por ID explícito, se puede correr
-- varias veces sin duplicar nada. No toca InventarioCasas ni ninguna otra
-- tabla existente.
-- ============================================================================

USE CALANDRIA;
GO

-- ============================================================================
-- Catálogo RutaTuneraDestajo
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM RutaTuneraDestajo WHERE ID = 9001)
INSERT INTO RutaTuneraDestajo (ID, ParentID, Nombre, Descripcion, Orden, Nivel, FechaCreacion, UsuarioCreacion, TipoTarea) VALUES
(9001, NULL, 'CIMENTACIÓN',                     NULL, 1, 0, GETDATE(), 'seed-local', 0),
(9002, NULL, 'ALBAÑILERÍA',                      NULL, 2, 0, GETDATE(), 'seed-local', 0),
(9003, NULL, 'INSTALACIONES HIDROSANITARIAS',    NULL, 3, 0, GETDATE(), 'seed-local', 0),
(9011, 9001, 'Excavación de zapatas',                              'Excavación manual/mecánica a nivel de desplante', 1, 1, GETDATE(), 'seed-local', 2),
(9012, 9001, 'Concreto f''c=200 kg/cm2 para zapatas',               'Suministro y colado', 2, 1, GETDATE(), 'seed-local', 1),
(9013, 9001, 'Armado de acero de refuerzo',                        'Habilitado y armado', 3, 1, GETDATE(), 'seed-local', 2),
(9021, 9002, 'Muro de block hueco 12x20x40',                       NULL, 1, 1, GETDATE(), 'seed-local', 1),
(9022, 9002, 'Aplanado fino en muros',                              NULL, 2, 1, GETDATE(), 'seed-local', 2),
(9023, 9002, 'Firme de concreto en planta baja',                    NULL, 3, 1, GETDATE(), 'seed-local', 1),
(9031, 9003, 'Instalación de tubería hidráulica',                   NULL, 1, 1, GETDATE(), 'seed-local', 2),
(9032, 9003, 'Muebles de baño (WC, lavabo, regadera)',               NULL, 2, 1, GETDATE(), 'seed-local', 1);
GO

IF NOT EXISTS (SELECT 1 FROM RutaTuneraDestajo_Columnas WHERE NodoID = 9011 AND NombreColumna = 'Cantidad')
INSERT INTO RutaTuneraDestajo_Columnas (NodoID, NombreColumna, Valor) VALUES
(9011, 'Cantidad', '45'),   (9011, 'Unidad', 'M3'),   (9011, 'Precio', '180'),
(9012, 'Cantidad', '12'),   (9012, 'Unidad', 'M3'),   (9012, 'Precio', '2450'),
(9013, 'Cantidad', '850'),  (9013, 'Unidad', 'KG'),   (9013, 'Precio', '12'),
(9021, 'Cantidad', '210'),  (9021, 'Unidad', 'M2'),   (9021, 'Precio', '185'),
(9022, 'Cantidad', '380'),  (9022, 'Unidad', 'M2'),   (9022, 'Precio', '65'),
(9023, 'Cantidad', '95'),   (9023, 'Unidad', 'M2'),   (9023, 'Precio', '210'),
(9031, 'Cantidad', '1'),    (9031, 'Unidad', 'LOTE'), (9031, 'Precio', '8500'),
(9032, 'Cantidad', '1'),    (9032, 'Unidad', 'LOTE'), (9032, 'Precio', '14200');
GO

-- ============================================================================
-- Catálogo RutaCalandraDestajo
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM RutaCalandraDestajo WHERE ID = 9001)
INSERT INTO RutaCalandraDestajo (ID, ParentID, Nombre, Descripcion, Orden, Nivel, FechaCreacion, UsuarioCreacion, TipoTarea) VALUES
(9001, NULL, 'CIMENTACIÓN',              NULL, 1, 0, GETDATE(), 'seed-local', 0),
(9002, NULL, 'ESTRUCTURA DE PANELES',    NULL, 2, 0, GETDATE(), 'seed-local', 0),
(9003, NULL, 'ACABADOS',                 NULL, 3, 0, GETDATE(), 'seed-local', 0),
(9011, 9001, 'Excavación y plantilla',                     NULL, 1, 1, GETDATE(), 'seed-local', 2),
(9012, 9001, 'Losa de cimentación',                          NULL, 2, 1, GETDATE(), 'seed-local', 1),
(9013, 9002, 'Montaje de paneles estructurales',             NULL, 1, 1, GETDATE(), 'seed-local', 2),
(9021, 9002, 'Suministro de paneles prefabricados',           NULL, 2, 1, GETDATE(), 'seed-local', 1),
(9022, 9002, 'Losa de entrepiso',                             NULL, 3, 1, GETDATE(), 'seed-local', 1),
(9023, 9003, 'Pintura e impermeabilización',                  NULL, 1, 1, GETDATE(), 'seed-local', 2),
(9031, 9003, 'Instalación de cancelería',                     NULL, 2, 1, GETDATE(), 'seed-local', 1);
GO

IF NOT EXISTS (SELECT 1 FROM RutaCalandraDestajo_Columnas WHERE NodoID = 9011 AND NombreColumna = 'Cantidad')
INSERT INTO RutaCalandraDestajo_Columnas (NodoID, NombreColumna, Valor) VALUES
(9011, 'Cantidad', '42'),  (9011, 'Unidad', 'M3'),   (9011, 'Precio', '190'),
(9012, 'Cantidad', '1'),   (9012, 'Unidad', 'LOTE'), (9012, 'Precio', '68000'),
(9013, 'Cantidad', '1'),   (9013, 'Unidad', 'LOTE'), (9013, 'Precio', '45000'),
(9021, 'Cantidad', '1'),   (9021, 'Unidad', 'LOTE'), (9021, 'Precio', '185000'),
(9022, 'Cantidad', '110'), (9022, 'Unidad', 'M2'),   (9022, 'Precio', '320'),
(9023, 'Cantidad', '310'), (9023, 'Unidad', 'M2'),   (9023, 'Precio', '48'),
(9031, 'Cantidad', '8'),   (9031, 'Unidad', 'PZA'),  (9031, 'Precio', '3200');
GO

-- ============================================================================
-- Activación por casa (InventarioCasas reales: Mz1/Lt10 y Mz1/Lt11 = TUNERA,
-- Mz1/Lt15 = CALANDRA — ver README_BD_Local_Pruebas.md)
-- ============================================================================

-- Mz1/Lt10 (TUNERA) — las 8 tareas activadas, con variedad de estados.
IF NOT EXISTS (SELECT 1 FROM ActivacionTareasRuta WHERE Manzana='1' AND Lote='10' AND Ruta='RutaTuneraDestajo' AND NodoID=9011)
INSERT INTO ActivacionTareasRuta (Manzana, Lote, Prototipo, Ruta, NodoID, NombreTarea, Activa, FechaActivacion, DesatajoActivado, Finalizado, FechaFinalizacion, CuadrillaAsignada, NominaDistribuida) VALUES
('1','10','TUNERA','RutaTuneraDestajo', 9011, 'Excavación de zapatas',                    1, DATEADD(DAY,-20,GETDATE()), 1, 1, DATEADD(DAY,-14,GETDATE()), 'CUAD-01', 1),
('1','10','TUNERA','RutaTuneraDestajo', 9012, 'Concreto f''c=200 kg/cm2 para zapatas',     1, DATEADD(DAY,-18,GETDATE()), 1, 1, DATEADD(DAY,-12,GETDATE()), NULL,      0),
('1','10','TUNERA','RutaTuneraDestajo', 9013, 'Armado de acero de refuerzo',               1, DATEADD(DAY,-15,GETDATE()), 1, 0, NULL,                       'CUAD-01', 0),
('1','10','TUNERA','RutaTuneraDestajo', 9021, 'Muro de block hueco 12x20x40',              1, DATEADD(DAY,-10,GETDATE()), 1, 1, DATEADD(DAY,-3,GETDATE()),  NULL,      0),
('1','10','TUNERA','RutaTuneraDestajo', 9022, 'Aplanado fino en muros',                    1, DATEADD(DAY,-6,GETDATE()),  1, 0, NULL,                       'CUAD-02', 0),
('1','10','TUNERA','RutaTuneraDestajo', 9023, 'Firme de concreto en planta baja',          1, DATEADD(DAY,-4,GETDATE()),  1, 0, NULL,                       NULL,      0),
('1','10','TUNERA','RutaTuneraDestajo', 9031, 'Instalación de tubería hidráulica',         1, DATEADD(DAY,-12,GETDATE()), 1, 1, DATEADD(DAY,-5,GETDATE()),  'CUAD-02', 1),
('1','10','TUNERA','RutaTuneraDestajo', 9032, 'Muebles de baño (WC, lavabo, regadera)',    1, DATEADD(DAY,-2,GETDATE()),  1, 0, NULL,                       NULL,      0);
GO

-- Mz1/Lt11 (TUNERA) — solo 5 de las 8 activadas, para mostrar que no toda casa
-- tiene el mismo avance.
IF NOT EXISTS (SELECT 1 FROM ActivacionTareasRuta WHERE Manzana='1' AND Lote='11' AND Ruta='RutaTuneraDestajo' AND NodoID=9011)
INSERT INTO ActivacionTareasRuta (Manzana, Lote, Prototipo, Ruta, NodoID, NombreTarea, Activa, FechaActivacion, DesatajoActivado, Finalizado, FechaFinalizacion, CuadrillaAsignada, NominaDistribuida) VALUES
('1','11','TUNERA','RutaTuneraDestajo', 9011, 'Excavación de zapatas',                 1, DATEADD(DAY,-9,GETDATE()), 1, 1, DATEADD(DAY,-2,GETDATE()), 'CUAD-03', 1),
('1','11','TUNERA','RutaTuneraDestajo', 9012, 'Concreto f''c=200 kg/cm2 para zapatas',  1, DATEADD(DAY,-7,GETDATE()), 1, 0, NULL,                      NULL,      0),
('1','11','TUNERA','RutaTuneraDestajo', 9021, 'Muro de block hueco 12x20x40',          1, DATEADD(DAY,-5,GETDATE()), 1, 0, NULL,                      'CUAD-03', 0),
('1','11','TUNERA','RutaTuneraDestajo', 9022, 'Aplanado fino en muros',                1, DATEADD(DAY,-3,GETDATE()), 1, 1, DATEADD(DAY,-1,GETDATE()), NULL,      0),
('1','11','TUNERA','RutaTuneraDestajo', 9031, 'Instalación de tubería hidráulica',     1, DATEADD(DAY,-3,GETDATE()), 1, 0, NULL,                      NULL,      0);
GO

-- Mz1/Lt15 (CALANDRA) — las 7 tareas del catálogo Calandra (IDs compartidos con
-- el catálogo Tunera por el FK descrito arriba; el contenido mostrado es el de
-- RutaCalandraDestajo porque AdministrativosController hace el JOIN contra esa
-- tabla cuando el prototipo de la casa es CALANDRA).
IF NOT EXISTS (SELECT 1 FROM ActivacionTareasRuta WHERE Manzana='1' AND Lote='15' AND Ruta='RutaCalandraDestajo' AND NodoID=9011)
INSERT INTO ActivacionTareasRuta (Manzana, Lote, Prototipo, Ruta, NodoID, NombreTarea, Activa, FechaActivacion, DesatajoActivado, Finalizado, FechaFinalizacion, CuadrillaAsignada, NominaDistribuida) VALUES
('1','15','CALANDRA','RutaCalandraDestajo', 9011, 'Excavación y plantilla',                 1, DATEADD(DAY,-16,GETDATE()), 1, 1, DATEADD(DAY,-10,GETDATE()), 'CUAD-04', 1),
('1','15','CALANDRA','RutaCalandraDestajo', 9012, 'Losa de cimentación',                     1, DATEADD(DAY,-14,GETDATE()), 1, 1, DATEADD(DAY,-8,GETDATE()),  NULL,      0),
('1','15','CALANDRA','RutaCalandraDestajo', 9013, 'Montaje de paneles estructurales',        1, DATEADD(DAY,-8,GETDATE()),  1, 0, NULL,                       'CUAD-04', 0),
('1','15','CALANDRA','RutaCalandraDestajo', 9021, 'Suministro de paneles prefabricados',     1, DATEADD(DAY,-8,GETDATE()),  1, 1, DATEADD(DAY,-6,GETDATE()),  NULL,      0),
('1','15','CALANDRA','RutaCalandraDestajo', 9022, 'Losa de entrepiso',                       1, DATEADD(DAY,-5,GETDATE()),  1, 0, NULL,                       NULL,      0),
('1','15','CALANDRA','RutaCalandraDestajo', 9023, 'Pintura e impermeabilización',            1, DATEADD(DAY,-2,GETDATE()),  1, 0, NULL,                       'CUAD-05', 0),
('1','15','CALANDRA','RutaCalandraDestajo', 9031, 'Instalación de cancelería',               1, DATEADD(DAY,-1,GETDATE()),  1, 0, NULL,                       NULL,      0);
GO

-- ============================================================================
-- Nómina asignada — algunas tareas de mano de obra, incluida una "con
-- cuadrilla" sin finalizar (9013 en Mz1/Lt10): el gasto de M.O. se reconoce al
-- asignar nómina, no al finalizar la tarea (ver RegistroDestajo.MontoGastado).
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM NominaTareasAsignada WHERE Manzana='1' AND Lote='10' AND Ruta='RutaTuneraDestajo' AND NodoID=9011)
INSERT INTO NominaTareasAsignada (Manzana, Lote, Ruta, NodoID, NombreTarea, CodigoCuadrilla, IdTrabajador, NombreTrabajador, Rol, EsJefe, Monto, TotalAsignado, FechaActualizacion) VALUES
('1','10','RutaTuneraDestajo', 9011, 'Excavación de zapatas',           'CUAD-01', NULL, 'Juan Pérez López',       'Jefe de cuadrilla', 1, 4500, 8100, DATEADD(DAY,-14,GETDATE())),
('1','10','RutaTuneraDestajo', 9011, 'Excavación de zapatas',           'CUAD-01', NULL, 'Miguel Ángel Torres',    'Peón',               0, 3600, 8100, DATEADD(DAY,-14,GETDATE())),
('1','10','RutaTuneraDestajo', 9013, 'Armado de acero de refuerzo',     'CUAD-01', NULL, 'Miguel Ángel Torres',    'Fierrero',           1, 3000, 3000, DATEADD(DAY,-2,GETDATE())),
('1','10','RutaTuneraDestajo', 9031, 'Instalación de tubería hidráulica','CUAD-02', NULL, 'Roberto Sánchez Díaz',  'Jefe de cuadrilla', 1, 8500, 8500, DATEADD(DAY,-5,GETDATE()));
GO

IF NOT EXISTS (SELECT 1 FROM NominaTareasAsignada WHERE Manzana='1' AND Lote='11' AND Ruta='RutaTuneraDestajo' AND NodoID=9011)
INSERT INTO NominaTareasAsignada (Manzana, Lote, Ruta, NodoID, NombreTarea, CodigoCuadrilla, IdTrabajador, NombreTrabajador, Rol, EsJefe, Monto, TotalAsignado, FechaActualizacion) VALUES
('1','11','RutaTuneraDestajo', 9011, 'Excavación de zapatas', 'CUAD-03', NULL, 'Juan Pérez López', 'Jefe de cuadrilla', 1, 8100, 8100, DATEADD(DAY,-2,GETDATE()));
GO

IF NOT EXISTS (SELECT 1 FROM NominaTareasAsignada WHERE Manzana='1' AND Lote='15' AND Ruta='RutaCalandraDestajo' AND NodoID=9011)
INSERT INTO NominaTareasAsignada (Manzana, Lote, Ruta, NodoID, NombreTarea, CodigoCuadrilla, IdTrabajador, NombreTrabajador, Rol, EsJefe, Monto, TotalAsignado, FechaActualizacion) VALUES
('1','15','RutaCalandraDestajo', 9011, 'Excavación y plantilla', 'CUAD-04', NULL, 'Miguel Ángel Torres', 'Jefe de cuadrilla', 1, 7980, 7980, DATEADD(DAY,-10,GETDATE()));
GO

PRINT 'Destajos sembrados: RutaTuneraDestajo (11 nodos) + RutaCalandraDestajo (10 nodos), activados en Mz1/Lt10, Mz1/Lt11 y Mz1/Lt15, con nómina parcial asignada.';
GO
