/*
  Importa explosiontunera.xlsx al catalogo InsumosTuneraEXP (UPSERT por Clave).
  - Crea la tabla InsumosTuneraEXP si no existe.
  - Inserta/actualiza 252 insumos (se excluyen filas de subtotal/total y duplicados por Clave).
  - Agrega columnas Tipo/Familia si faltan.
  - Asocia por nombre cada insumo del catalogo con los insumos capturados en el
    treelist (nodos Nivel 2 de RutaTuneraDestajo): rellena su 'Clave' por coincidencia
    de Nombre = Descripcion.
  Archivo UTF-8 con BOM.
*/
USE [BaseDatosCalandria];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Crear el catalogo si no existe (con todas las columnas, incluidas Tipo/Familia).
IF OBJECT_ID(N'dbo.InsumosTuneraEXP', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InsumosTuneraEXP(
        [Clave]        NVARCHAR(50)   NOT NULL,
        [Descripción]  NVARCHAR(1000) NULL,
        [Unidad]       NVARCHAR(20)   NULL,
        [Cantidad]     DECIMAL(18,6)  NULL,
        [Costo]        DECIMAL(18,4)  NULL,
        [Importe]      DECIMAL(18,4)  NULL,
        [Porcentaje]   DECIMAL(18,12) NULL,
        [Tipo]         NVARCHAR(50)   NULL,
        [Familia]      NVARCHAR(80)   NULL,
        CONSTRAINT [PK_InsumosTuneraEXP] PRIMARY KEY CLUSTERED ([Clave] ASC)
    );
    PRINT 'Tabla InsumosTuneraEXP creada.';
END
GO

-- Por compatibilidad: si la tabla ya existia sin estas columnas, agregarlas.
IF COL_LENGTH('dbo.InsumosTuneraEXP',N'Tipo') IS NULL
    ALTER TABLE dbo.InsumosTuneraEXP ADD [Tipo] NVARCHAR(50) NULL;
GO
IF COL_LENGTH('dbo.InsumosTuneraEXP',N'Familia') IS NULL
    ALTER TABLE dbo.InsumosTuneraEXP ADD [Familia] NVARCHAR(80) NULL;
GO

MERGE dbo.InsumosTuneraEXP AS t
USING (VALUES
    (N'04-02-024',N'Cinta aislante Temflex 10600 mca 3M rollo 19mm x 18 m',N'rollo',6.999993,46.9,328.28,0.0003490619,N'Materiales',N'Electricidad'),
    (N'04-11-027',N'Cable THW watts Marca Viakon calibre 12',N'rollo',0.6,945,567,0.0006028942,N'Materiales',N'Electricidad'),
    (N'AACE-0233',N'Placa comercial A36 espesor de 6.4 x 76.2 mm (½" x 3'')',N'kg',18.406,23.17,426.47,0.0004534679,N'Materiales',N'Acero'),
    (N'AACE-0296',N'Soldadura eléctrica electrodo 6013-3 mm de 1/8" verde Infra',N'kg',0.5,79.84,39.92,0.0000424472,N'Materiales',N'Herreria'),
    (N'AACE-0298',N'Soldadura eléctrica electrodo 7018-3 mm de 1/8"',N'kg',7.121482,54.28,386.55,0.0004110207,N'Materiales',N'Cementos'),
    (N'AACF-4214',N'Perfil CF formado en frio 4" x 2" x cal 14 (102 x 51 x 14) peso 3.34 kg/ml',N'kg',18.230403,32.45,591.58,0.0006290302,N'Materiales',N'Acero'),
    (N'ACCH-002',N'Varilla corrugada acero de refuerzo del # 3 (⅜") f''y = 4,200 kg/cm²',N'kg',850.75016,18.1,15398.56,0.0163733719,N'Materiales',N'Acero'),
    (N'ACCH-003',N'Varilla corrugada acero de refuerzo del # 4 (½") fy=4,200 kg/cm²") f''y = 4,200 kg/cm²',N'kg',328.098662,18.1,5938.62,0.0063145667,N'Materiales',N'Acero'),
    (N'ACCH-004',N'Varilla corrugada acero de refuerzo del # 5 ( 5/8") f''y = 4,200 kg/cm²',N'kg',65.764759,18.1,1190.35,0.0012657056,N'Materiales',N'Acero'),
    (N'ACCH-009',N'Cemento gris',N'ton',9.206064,5250,48331.88,0.0513915488,N'Materiales',N'Polvos'),
    (N'ACCH-026',N'Alambre recocido calibre 18',N'kg',84.93148,25.69,2181.85,0.0023199729,N'Materiales',N'Acero'),
    (N'ACE AR1220',N'Armex 12 x 20-4',N'm',70.990488,21.82,1549.01,0.0016470707,N'Materiales',N'Acero'),
    (N'ACEITE',N'Aceite quemado',N'litro',5.437743,8.33,45.3,0.0000481677,N'Materiales',N'Consumible'),
    (N'ACEL-005',N'Clavo con cabeza de 1"',N'kg',0,90,0,0,N'Materiales',N'Acero'),
    (N'ACEL-010',N'Clavo con cabeza de 3 ½"',N'kg',52.926773,27.59,1460.26,0.0015527023,N'Materiales',N'Acero'),
    (N'ACERO CF 102-51-14',N'Perfil CF formado en frio 102 x 51 x 14 mm (4 x 2 14)',N'kg',39.252682,32.5,1275.71,0.0013564693,N'Materiales',N'Herreria'),
    (N'ACES-001',N'Malla electrosoldada 66-10-10 (1.02 kg/m²).',N'm2',17.71,24.67,436.91,0.0004645688,N'Materiales',N'Acero'),
    (N'ACES-002',N'Malla electrosoldada 66-88 (5.22  kg/m²).',N'm2',131.596859,26.72,3516.27,0.0037388689,N'Materiales',N'Acero'),
    (N'ACES-003',N'Malla electrosoldada 66-66 (2.05 kg/m2).',N'm2',53.802,41,2205.88,0.0023455241,N'Materiales',N'Acero'),
    (N'ACES-004',N'Malla electrosoldada 66-44 (2.83 kg/m2).',N'm2',6.3492,63.76,404.82,0.0004304473,N'Materiales',N'Acero'),
    (N'ADHE GRIS',N'Ahesivo maestro gris 20 kg',N'saco',71.206703,99.59,7091.48,0.007540409,N'Materiales',N'Polvos'),
    (N'ADHESIVO UNI UNIBLOCK',N'Adhesivo universal Uniblock 20 kg',N'saco',4.183332,181.03,757.31,0.0008052518,N'Materiales',N'Polvos'),
    (N'AGLU-003',N'Yeso',N'ton',6.054399,3517.2,21294.53,0.0226425887,N'Materiales',N'Polvos'),
    (N'AGRE-001',N'Arena en camión de 6 m3.',N'm3',16.76076,368.9,6183.04,0.0065744598,N'Materiales',N'Agregados'),
    (N'AGRE-002',N'Grava de 3/4" (19 mm) en camión de 6 m3.',N'm3',12.765776,412.3,5263.31,0.0055965059,N'Materiales',N'Agregados'),
    (N'AGRE-010',N'Adhesivo blanco premier antideslizamiento, contenido 20 kg x saco, marca Interceramic',N'sac',2.680001,115.62,309.86,0.0003294758,N'Materiales',N'Polvos'),
    (N'AGRE-014',N'Calhidra.',N'ton',0.028836,3862,111.37,0.0001184203,N'Materiales',N'Polvos'),
    (N'AGRE-016',N'Agua potable',N'm3',20.484808,190,3892.14,0.0041385335,N'Materiales',N'Agregados'),
    (N'BLOCK 12-20-40',N'Block entero gris hueco 12 x 20 x 40 cm',N'pza',2744.080032,14.91,40914.24,0.0435043322,N'Materiales',N'Block'),
    (N'BLOCK MITAD 12-20-20',N'Block mitad gris hueco 12 x 20 x 20 cm',N'pza',1010.025004,9.49,9585.13,0.0101919205,N'Materiales',N'Block'),
    (N'BLOCK PIÑA 12-20-40',N'Block piña entero gris 12 x 10 x 40 cm',N'pza',125.999055,10.38,1307.87,0.0013906652,N'Materiales',N'Block'),
    (N'BOQUILLA ANTIQUE S/A',N'Boquilla sin arena ANTIQUE saco 5 kg',N'Caja',1.185829,120.08,142.4,0.0001514147,N'Materiales',N'Polvos'),
    (N'BOQUILLA SELLADOR IVORY',N'Boquilla sellador Int. IVORY(saco 10 kg)',N'saco',8.049967,103.52,833.33,0.0008860843,N'Materiales',N'Polvos'),
    (N'BOVE POLI 15',N'Bovedilla poliestireno 15 x 61 x 122 cm',N'pza',52.186816,117.5,6131.95,0.0065201355,N'Materiales',N'Prefabricados'),
    (N'BOVE POLI 16',N'Bovedilla poliestireno 16 x 61 x 122 cm',N'pza',71.21038,137.08,9761.51,0.0103794663,N'Materiales',N'Prefabricados'),
    (N'BRAZO 2303',N'Brazo de regadera a pared tubular 40 cm',N'pza',2,710.84,1421.68,0.00151168,N'Materiales',N'General'),
    (N'CAJA META 5',N'Caja octagonal 4"  mca ITSA reforzada',N'pza',25,26.5,662.5,0.0007044398,N'Materiales',N'Electricidad'),
    (N'CAMI-049',N'Tornillo 2.5 cm 6 x 1" (punta fina) para tablaroca paquete con 100 piezas',N'pqte',8.4896,119,1010.26,0.0010742149,N'Materiales',N'Tabla roca'),
    (N'CAMI-054',N'Tornillo para madera de 10 x 38 en presentación de caja de 144 piezas',N'pqte',0.651864,124.49,81.15,0.0000862872,N'Materiales',N'Tabla roca'),
    (N'CAMI-060',N'Cinta cubrejuntas de papel de 5 x 75 m',N'pza',1.34015,61.63,82.59,0.0000878184,N'Materiales',N'Tabla roca'),
    (N'CANTERA BCO LIMON 40X60',N'Cantera blanco limón 40 x 60 cm',N'm2',5.02,628.58,3155.47,0.0033552283,N'Materiales',N'Cantera'),
    (N'CC QO120L125PG',N'Centro de Carga QO124L125PG, 1 fase, 24 circuitos, 125A agarraderas principales convertibles, NEMA1, UL; incluye tapa',N'pza',1,2828,2828,0.0030070277,N'Materiales',N'Electricidad'),
    (N'CC TAPA SQD',N'Frente P/C.C.24  circuitos.SQD empotrar.',N'pza',1,678,678,0.0007209211,N'Materiales',N'Electricidad'),
    (N'CERRA COMBO',N'Combo de cerradura de bola y chapa de seguridad con llave acero inoxidable',N'Combo',2,290,580,0.0006167171,N'Materiales',N'Carpinteria'),
    (N'CERRADURA GATILLO',N'Cerradura  gatiilo combo / manija satinado',N'Combo',1,1200,1200,0.0012759665,N'Materiales',N'Carpinteria'),
    (N'CLAVO ARANDELA',N'Clavo de 1" con arandela',N'pza',19.99956,2.1,42,0.0000446588,N'Materiales',N'Acero'),
    (N'CMC-01006',N'Conector cespol de 32 x 50 mm',N'pza',4,4.06,16.24,0.0000172681,N'Materiales',N'Plomeria'),
    (N'CMC-01384',N'Niple galvanizado de 13 x 200 mm marca Cifunsa',N'pza',3,13.58,40.74,0.0000433191,N'Materiales',N'Plomeria'),
    (N'CMC-02864',N'Llave angular vac-13',N'pza',6,110.5,663,0.0007049715,N'Materiales',N'Accesorios B'),
    (N'CMC-21505',N'Manguera flexible lavabo 40 cm',N'pza',4,47.79,191.16,0.0002032615,N'Materiales',N'Accesorios B'),
    (N'CMC-21564',N'Lubricante para anger de PVC contenido bote de 500 grs marca Tubos flexibles',N'pza',0.05,102.54,5.13,0.0000054548,N'Materiales',N'Plomeria'),
    (N'CMC-21565',N'Cemento PVC bote 460 gr. Tubos flex, marca Tubos flexibles duralón',N'pza',0.1,191.28,19.13,0.000020341,N'Materiales',N'Plomeria'),
    (N'CMC-21726',N'Soldadura 95x5 mca Omega, peso158 gr, largo 3 m',N'pza',0,399,0,0,N'Materiales',N'Plomeria'),
    (N'CMC-24500',N'Tinaco mca Rotoplas capacidad 1,100Lt c/paso 2 y accesorios marca Rotoplas con accesorios incluidos: válvula de llenado, multiconector con válvula esfera y tuerca unión,
fllotador #5, jarro de aire, filtro sstándar.',N'pza',1,3699,3699,0.0039331667,N'Materiales',N'General'),
    (N'CMC-39600',N'Clavo de 2 ½"',N'kg',30.928726,26.72,826.42,0.0008787368,N'Materiales',N'Acero'),
    (N'CMC-58966',N'Armex 15x15-4',N'm ',21.320334,21.82,465.21,0.0004946603,N'Materiales',N'Acero'),
    (N'CMC-58967',N'Armex 15-20-4',N'm',39.332239,21.82,858.23,0.0009125606,N'Materiales',N'Acero'),
    (N'CMC-59121',N'Brida flexible corta PB-200, marca Coflex',N'pza',2,116,232,0.0002466869,N'Materiales',N'Plomeria'),
    (N'COMB-006',N'Diesel, no incluye IVA ni IEPYS (29.88 centavos por litro).',N'lt',8.014491,24.02,192.51,0.0002046969,N'Materiales',N'Consumible'),
    (N'CONC PRE AZOTEA',N'Concreto premezclado tipo bombeable fc=200 kg/cm², para losa de azotea normal clase I, TMA 20 mm. Rev. 14 ± 2.5 cm',N'm3',5.65,2510,14181.5,0.0150792655,N'Materiales',N'Prefabricados'),
    (N'CONC PRE ENTREPISO',N'Concreto premezclado tipo bombeable fc=200 kg/cm², para losa de entrepiso normal clase I, TMA 20 mm. Rev. 14 ± 2.5 cm',N'm3',5.502,2510,13810.02,0.0146842688,N'Materiales',N'Prefabricados'),
    (N'CPVC LIMPIADOR',N'Limpiador PVC y CPVC primer morado 473 ml mca Oatey',N'pza',1.33861,130,174.02,0.0001850364,N'Materiales',N'Plomeria'),
    (N'CRUZA-003',N'Concreto premezclado no bombeable fc=200 kg/cm², normal clase I, TMA 20 mm. revenimiento hasta 10 ± 2.5 cm',N'm3',0.90909,2510,2281.82,0.0024262715,N'Materiales',N'Prefabricados'),
    (N'CRUZA-025',N'Concreto premezclado bombeable f''c=200 kg/cm², normal clase I, TMA 20 mm, revenimiento hasta 14 ± 3.5 cm',N'm3',7.01,2510,17595.1,0.0187089648,N'Materiales',N'Prefabricados'),
    (N'DESMOL RHEO 255',N'RHEOFINISH® 255 (19 litros x cubeta)',N'cubeta',0.114194,1650.68,188.49,0.0002004224,N'Materiales',N'General'),
    (N'DICA 4420',N'Llave mezcaldora monomando para lavabo',N'pza',2,659.1,1318.2,0.0014016492,N'Materiales',N'Accesorios B'),
    (N'FORRO ACUS R13',N'Aislacustic M65 R 13 medidas de 8.9 x 61 x 244 cm mca Owens Corning ; incluye 12 piezas.',N'paquete',1.839852,2319,4266.62,0.0045367201,N'Materiales',N'Tabla roca'),
    (N'GARP-341',N'Compuerta roscable de 13 mm, marca Urrea',N'pza',1,120,120,0.0001275966,N'Materiales',N'General'),
    (N'HELVEX 105',N'Toallero barra modelo 105 cromo marca HELVEX',N'jgo',2,295,590,0.0006273502,N'Materiales',N'Accesorios B'),
    (N'HELVEX 108',N'Jabonera lavabo modelo 108 cromo marca HELVEX.',N'jgo',2,271.8,543.6,0.0005780128,N'Materiales',N'Accesorios B'),
    (N'HELVEX 115',N'Porta papel tubo anti robo modelo 115 mca Helvex',N'pza',2,287.59,575.18,0.000611592,N'Materiales',N'Accesorios B'),
    (N'HELVEX H200',N'Regadera de plato rectangular',N'jgo',2,527.82,1055.64,0.0011224677,N'Materiales',N'Accesorios B'),
    (N'INME-011',N'Acero de refuerzo no. 2, (¼") fyp = 6,000 kg/cm²',N'kg',51.463475,22.3,1147.63,0.0012202812,N'Materiales',N'Acero'),
    (N'INME-160',N'Segueta diente fino bimetalica marca Hercort',N'pza',0.06,24.5,1.47,0.0000015631,N'Materiales',N'General'),
    (N'INME-189',N'Caja chalupa profunda 2x4" galvanizada reforzada',N'pza',44,11.5,506,0.0005380325,N'Materiales',N'Electricidad'),
    (N'INME-286',N'Jabón en polvo (detergente)',N'kg',6.4815,25,162.04,0.000172298,N'Materiales',N'Consumible'),
    (N'INTER ALTAMURA BCO 60X60',N'Altamura blanco 60 x 60 cm primera',N'm2',101.43,147.88,14999.47,0.0159490174,N'Materiales',N'General'),
    (N'INTER SPIRIT WHITE 25X40',N'Spirit white 25x40 cm primera',N'm2',10.7205,185.35,1987.04,0.0021128304,N'Materiales',N'General'),
    (N'INTER VALPARAISO',N'Antiderrapante mod Valparaiso formato 20 x 20 cm blanco mca Interceramic.',N'm2',4.221,186.85,788.69,0.0008386183,N'Materiales',N'General'),
    (N'KURODA FVIE402',N'Fuente Vienna 4 blanco, mca Corona',N'pza',2,709.41,1418.82,0.001508639,N'Materiales',N'General'),
    (N'KURODA PVIE02',N'Pedestal modelo Vienna blanco mca Corona',N'pza',2,472.94,945.88,0.0010057593,N'Materiales',N'General'),
    (N'LBJC-273',N'Cable THW calibre 14 awg, marca Iusa',N'roll',5.1,840.31,4285.58,0.0045568803,N'Materiales',N'Electricidad'),
    (N'LBJC-274',N'Cable THW calibre 12 awg, marca Iusa',N'roll',8.1,1179.73,9555.83,0.0101607656,N'Materiales',N'Electricidad'),
    (N'LLAVE JARDIN 2-A RUGO',N'Llave con rosca p/manguera mca Rugo sin pulir, cierre compresión 13mm, conexión macho',N'pza',2,52.95,105.9,0.000112604,N'Materiales',N'General'),
    (N'MADE 0110',N'Tuino 1"',N'pt',0,2.88,0,0,N'Materiales',N'Madera'),
    (N'MADE 0204',N'Barrote de madera 2 x 4 x 8',N'pt',281.503742,13.58,3822.82,0.0040648251,N'Materiales',N'Madera'),
    (N'MADE 0303',N'Barrote de madera 3 x 3 x 8',N'pt',320.025168,20.25,6480.51,0.0068907613,N'Materiales',N'Madera'),
    (N'MADE 0304',N'Barrote de madera 4 x 4 x 8',N'pt',158.305399,27.04,4280.58,0.0045515638,N'Materiales',N'Madera'),
    (N'MADE-0104',N'Duela de madera 1 x 4 x 8''',N'pt',586.81441,10.65,6249.59,0.0066452228,N'Materiales',N'Madera'),
    (N'MADE-152',N'Madera 1½" x 2" x 8',N'pt',8.634821,8.33,71.93,0.0000764836,N'Materiales',N'Madera'),
    (N'MAFI-481',N'Taquete de plástico 3/8"',N'pza',8,0.99,7.92,0.0000084214,N'Materiales',N'Consumible'),
    (N'MAFI-524',N'Clavo para concreto de 2 ½"',N'kg',0.689763,50,34.49,0.0000366734,N'Materiales',N'Acero'),
    (N'MALLA BLINDOTEX',N'Malla fibra de vidrio marca BLINDOTEX 145gr de 0.97 x 45.7 m cal 40 (rollo=1x45 m)',N'rollo',3.263707,672.42,2194.59,0.0023335194,N'Materiales',N'Consumible'),
    (N'MALLA SW 4 PL',N'Malla SW fibra de vidrio de 4 plg',N'rollo',0.999995,61.55,61.55,0.0000654464,N'Materiales',N'Consumible'),
    (N'MANIJA RECTA',N'Cerradura manija recta sin llave satinada',N'pza',4,250,1000,0.0010633054,N'Materiales',N'Carpinteria'),
    (N'MARCO FV',N'Marco de fibra de vidrio naturaal con sello',N'pza',1,2200,2200,0.0023392719,N'Materiales',N'Consumible'),
    (N'MARCO INT MADERA',N'Marco de madera color chocolate',N'pza',4,2070,8280,0.0088041687,N'Materiales',N'Carpinteria'),
    (N'MARCO MET CHOCOLATE',N'Marco metalico universal chocolate 90 cm',N'jgo',2,440,880,0.0009357087,N'Materiales',N'Carpinteria'),
    (N'MATR-001',N'Tablaroca normal de 2.44 m de largo x 1.22 m de ancho x 9.6 mm. espesor, de la línea USG.',N'pza',22.484127,166.29,3738.89,0.0039755819,N'Materiales',N'Tabla roca'),
    (N'MATR-045',N'Redimix compuesto multiusos en cubeta de 28 kg de la línea de USG',N'cb',2.584575,217,560.86,0.0005963655,N'Materiales',N'Tabla roca'),
    (N'MATR-055',N'Basecoat cemento flexible, para tabla de durock en saco de 22.78 kg., de la línea de USG',N'saco',0.99992,468.49,468.45,0.0004981054,N'Materiales',N'Polvos'),
    (N'MATR-062-1',N'Tornillo tipo framer caja=100 pzas',N'Caja',11.803426,20,236.07,0.0002510145,N'Materiales',N'Consumible'),
    (N'MEAC-036',N'Cable THW-LS 90°C 600v calibre 8 AWG, marca Condulac',N'm',22.533,30.04,676.89,0.0007197408,N'Materiales',N'Electricidad'),
    (N'MEAC-037',N'Cable THW-LS/THW-LS 90°C 600v calibre 6 AWG, marca Condulac',N'm',69.489,42.61,2960.92,0.0031483622,N'Materiales',N'General'),
    (N'MHPV-227',N'Tubo de PVC hidraulico de cementar extremos lisos RD 13.5 con una presión de trabajo de 22.4 kg/cm2 de 3/4" (19 mm) de diámetro',N'm',21.567,15.14,326.52,0.0003471905,N'Materiales',N'Plomeria'),
    (N'MHPV-283',N'Codo PVC hidraulico de 90°x¾" (90°x19 mm) a cementar sistema inglés',N'pza',12.08,4.15,50.13,0.0000533035,N'Materiales',N'Plomeria'),
    (N'MHPV-287',N'Codo de 90° PVC a cementar (hh) (90º x 50 mm)',N'pza',17,5.57,94.69,0.0001006844,N'Materiales',N'Plomeria'),
    (N'MHPV-390',N'Codo de 90° cobre de 13 mm, marca Nacobre',N'pza',0,12.31,0,0,N'Materiales',N'Plomeria'),
    (N'MHSV-0084',N'Tubo sanitario de extremos lisos Anger de PVC de 50 mm de diámetro',N'tramo',5.72075,82.8,473.68,0.0005036665,N'Materiales',N'Plomeria'),
    (N'MHSV-0086',N'Tubo PVC Ø=4" sanitario norma extremos lisos (100 mm) de diámetro',N'tramo',7.00525,178.2,1248.34,0.0013273667,N'Materiales',N'Plomeria'),
    (N'MIFE-050',N'FEXPAN medida 1.22 x 1.22 m, espesor 13mm mca Fester',N'pza',0.201058,475,95.51,0.0001015563,N'Materiales',N'Consumible'),
    (N'MIFE-204',N'Curafest MC-320 blanco, marca FESTER cubeta 19 litros',N'cb',0.390209,1268.4,494.94,0.0005262724,N'Materiales',N'Consumible'),
    (N'MPPL-0132',N'Cople CPVC-cts a cementar rosca código (Hh) de ½" (13mm)',N'pza',33,1.86,61.38,0.0000652657,N'Materiales',N'Plomeria'),
    (N'MPPL-0158',N'Adaptador macho de CPVC/cts cementar/rosca de ½" (13 mm)',N'pza',64.7,4.96,320.91,0.0003412253,N'Materiales',N'Plomeria'),
    (N'MPPL-0176',N'Tee CPVC/cts cementar (Hhh) de ½" (13mm)',N'pza',40,2.99,119.6,0.0001271713,N'Materiales',N'Plomeria'),
    (N'MPPL-0186',N'Tapón capa CPVC/cts cementar (Hh) de ½" (13 mm)',N'pza',38,1.31,49.78,0.0000529313,N'Materiales',N'Plomeria'),
    (N'MPPL-0209',N'Codo de 90° x ½" CPVC cts boc. Esp. a cementar (Hh) de ½" (13mm)',N'pza',52,2.27,118.04,0.0001255126,N'Materiales',N'Plomeria'),
    (N'MPPL-0211 METAL',N'Codo 90° x ½" CPVC cts cementar a rosca (Hh) con oreja insercio de metal',N'pza',3,55.43,166.29,0.0001768171,N'Materiales',N'Plomeria'),
    (N'MPPL-0215',N'Tubo CPVC/cts cementar Ø=½" (12.6mm) tramo de 3.05 m',N'tramo',39.256084,39.19,1538.46,0.0016358528,N'Materiales',N'Plomeria'),
    (N'MPPL-0611',N'Codo de 90° x ½"  hierro galvanizado rosca (Hh)',N'pza',4,6.29,25.16,0.0000267528,N'Materiales',N'Plomeria'),
    (N'MPPL-0612',N'Codo de 90° x ¾  hierro galvanizado rosca (Hh)',N'pza',2,9.36,18.72,0.0000199051,N'Materiales',N'Plomeria'),
    (N'MPPL-0622',N'Tee fo go (Hhh) de 1/2" (13mm)',N'pza',3,9.65,28.95,0.0000307827,N'Materiales',N'Plomeria'),
    (N'MPPL-0623',N'Tee fo go (Hhh) de 3/4" (19 mm)',N'pza',2,11.85,23.7,0.0000252003,N'Materiales',N'Plomeria'),
    (N'MPPL-0679',N'Reducción bushing hierro galvanizado de rosca (Mh) de 3/4 x 1/2"',N'pza',2,11.68,23.36,0.0000248388,N'Materiales',N'Plomeria'),
    (N'MPPL-0758',N'Tubo fo go con rosca de Ø=½" (13mm) ced 40',N'tramo',1.224,418.75,512.55,0.0005449972,N'Materiales',N'Plomeria'),
    (N'MPPL-0759',N'Tubo fo go con rosca Ø=¾" (19 mm) ced 40',N'tramo',2.344,593.75,1391.75,0.0014798553,N'Materiales',N'Plomeria'),
    (N'MPPL-1979',N'Tubo de PVC hidraulico con campana RD 13.5 con una presión de trabajo de 22.4 kg/cm2 de Ø=¾" (19 mm) de diámetro.',N'm',29.56,10.95,323.68,0.0003441707,N'Materiales',N'Plomeria'),
    (N'MPPL-2018',N'Cople PVC  hidráulico cédula 40 a cementar código (Hh) de 3/4"',N'pza',4,3.15,12.6,0.0000133976,N'Materiales',N'Plomeria'),
    (N'MPPL-4020',N'Cople - PVC sanitario - cementar (Hh) 2''''.',N'pza',15,2.81,42.15,0.0000448183,N'Materiales',N'Plomeria'),
    (N'MPPL-4022',N'Cople PVC sanitario - cementar (Hh) 4''''.',N'pza',9,11.22,100.98,0.0001073726,N'Materiales',N'Plomeria'),
    (N'MPPL-4035',N'Reducción bushing PVC sanitario a cementar (Hh) de 4 x 2"',N'pza',1,23.75,23.75,0.0000252535,N'Materiales',N'Plomeria'),
    (N'MPPL-4043',N'Codo 45ºx2" - PVC sanitario - cementar (Hh).',N'pza',10,4.97,49.7,0.0000528463,N'Materiales',N'Plomeria'),
    (N'MPPL-4044',N'Codo 45º - pvc sanitario - cementar (Hh) 3''''.',N'pza',0,15.16,0,0,N'Materiales',N'Plomeria'),
    (N'MPPL-4045',N'Codo 45º x 4"- PVC sanitario - cementar (Hh)',N'pza',9,23.14,208.26,0.000221444,N'Materiales',N'Plomeria'),
    (N'MPPL-4062',N'Tee PVC sanitario cementar (Hhh) 4 x 4 x 4"',N'pza',2,26.3,52.6,0.0000559299,N'Materiales',N'Plomeria'),
    (N'MPPL-4067',N'Tee reducción central PVC sanitario cementar (Hhh) 4 x 4 x 2''''',N'pza',2,28.94,57.88,0.0000615441,N'Materiales',N'Plomeria'),
    (N'MPPL-4074',N'Yee PVC sanitario cementar (hhh) de 4 x 4 x 4" (100mm)',N'pza',2,52.1,104.2,0.0001107964,N'Materiales',N'Plomeria'),
    (N'MPPL-4080',N'Yee reducción PVC sanitario cementar (hhh) 4 x 4 x 2''''',N'pza',4,33.63,134.52,0.0001430358,N'Materiales',N'Plomeria'),
    (N'MPPL-4108',N'Codo de 90° x 4" PVC sanitario a cementar (Hhhh) salida lateral-trasera de 4 x 2"',N'pza',2,46.33,92.66,0.0000985259,N'Materiales',N'Plomeria'),
    (N'MPPL-4111',N'Codo 90° x4" PVC sanitario a cementar (Hhh) salida trasera de 4 x 2"',N'pza',1,46.33,46.33,0.0000492629,N'Materiales',N'Plomeria'),
    (N'MPPL-4115-PLATO',N'Coladera PVC de plato salida lateral entrepiso anti goteras mca Duralon largo x ancho 21 cm x 12 cm; Ø=19 cm',N'pza',2,380,760,0.0008081121,N'Materiales',N'Plomeria'),
    (N'MPPL-4196',N'Tubo PVC sanitario extremos lisos anger de 3" (75mm) presentación en tramos 6.10 m',N'm',8.76,28.62,250.71,0.0002665813,N'Materiales',N'Plomeria'),
    (N'MPPL-4551-4',N'Codo 90° x4" PVC sanitario a cementar (Hh)',N'pza',12,22.61,271.32,0.000288496,N'Materiales',N'Plomeria'),
    (N'MXCH-199',N'Adaptador hembra CPVC ½"',N'pza',2,5.53,11.06,0.0000117602,N'Materiales',N'Plomeria'),
    (N'OATEY 946 ML',N'Cemento CPVC FlowGuard® color blanco 946 ml',N'Bote',1.33861,417.15,558.4,0.0005937497,N'Materiales',N'Consumible'),
    (N'ONE PIECE ATLANTICA II',N'Sanitario one piece Atlantica II mca Orion Residencial; incluye asiento.',N'pza',2,2099,4198,0.0044637561,N'Materiales',N'General'),
    (N'OR 1½-1½ CAL14',N'Perfil tubular redondo (OR) de 48.26 mm (1½") calibre 14 (2.24 kg/m)',N'kg',80.64,23.17,1868.43,0.0019867117,N'Materiales',N'Acero'),
    (N'OR 2-2 CAL 14',N'Perfil tubular redondo (OR) de 60.22 mm (2") calibre 14 (2.99 kg/m).',N'kg',32.52,23.17,753.49,0.00080119,N'Materiales',N'Acero'),
    (N'PARA-030',N'Conector para varilla cooperweld t/burndy',N'pza',1,13.92,13.92,0.0000148012,N'Materiales',N'Electricidad'),
    (N'PARA-053',N'Varilla tipo cooperweld 5/8" x 1.50 m',N'pza',1,165.67,165.67,0.0001761578,N'Materiales',N'Electricidad'),
    (N'PL 10X10 1/4',N'Placa de acero A36 10x10 cm (espesor ¼")',N'pza',3,23.17,69.51,0.0000739104,N'Materiales',N'Acero'),
    (N'PLASTICO NE 600',N'Plástcio negro  No. 600 (6x1m=1 kg)',N'kg',11.920524,43.89,523.19,0.0005563108,N'Materiales',N'Consumible'),
    (N'POLIDUCTO 13MM',N'Tubería poliducto naranja reforzado de ½" (13mm)',N'm ',579.3835,3.35,1940.96,0.0020638332,N'Materiales',N'Electricidad'),
    (N'POLIESTIRENO 122-60-2.5',N'Placa poliestireno blanco 122 x 61 x 2.5 cm',N'pza',13.798697,76.67,1057.95,0.0011249239,N'Materiales',N'Prefabricados'),
    (N'POLIMEX TPN HEMBRA13',N'Tapón hembra ½" (13mm) mca POLIMEX 157',N'pza',32,6.15,196.8,0.0002092585,N'Materiales',N'Plomeria'),
    (N'PREY-014',N'Panel de Yeso para exteriores Glass Rey de 2.44 m. de largo x 1.22 m. de ancho x 12.7mm. de espesor, de la linea Panel Rey.',N'pza',0.999995,639.61,639.61,0.0006801008,N'Materiales',N'Tabla roca'),
    (N'PREY-021',N'Poste metálico 9.20 cm (3 5/8") calibre 26 de 3.05 m de longitud, de la línea Perfirey',N'pza',22.716848,67.25,1527.7,0.0016244117,N'Materiales',N'Tabla roca'),
    (N'PREY-029',N'Canal amarre 9.20 cm (3 5/8"), calibre 26 de 3.05 m de longitud, de la línea Perfirey',N'pza',11.433599,60.14,687.62,0.0007311501,N'Materiales',N'Tabla roca'),
    (N'PREY-034',N'Esquinero metálico calibre 26 de 3.05 m de longitud, de la linea Perfirey',N'pza',12.653468,26.51,335.45,0.0003566858,N'Materiales',N'Tabla roca'),
    (N'PREY-090',N'Fulminante para pistola Hilti o similar',N'pza',74.056605,1.91,141.45,0.0001504045,N'Materiales',N'Consumible'),
    (N'PTA ACE 90-206 LISA',N'Puerta metalica modelo Lisa color blanco 90 x 206 cm para exteriores',N'pza',2,1850,3700,0.00393423,N'Materiales',N'Carpinteria'),
    (N'PTA FV 90-206 AINOX',N'Puerta fibra de vidrio  modelo Elite color chocolate 90 x 206 cm para exteriores',N'pza',1,4500,4500,0.0047848743,N'Materiales',N'Carpinteria'),
    (N'PTA MADERA INT',N'Puerta de madera modelo Amparo texturizada',N'pza',4,840,3360,0.0035727061,N'Materiales',N'Carpinteria'),
    (N'PTR 4-1½ CAL 18',N'PTR Perfil tubular rectangular 101.6 x 38.1 mm (4 x 1½") calibre 18 (espesor 1,214 mm) peso 2.62 kg/m',N'tramo',6,501.09,3006.54,0.0031968702,N'Materiales',N'Acero'),
    (N'RECO-025',N'Pintura vinimex ultra (uv) 19 lt vinil acrílica especialmente diseñada para pigmentos resistentes a la decoloración y magnifica resistencia a la alcalinidad así como excelente inhibidor al crecimiento de hongos y algas',N'cb',9.282167,1249,11593.43,0.0123273567,N'Materiales',N'Pinturas'),
    (N'RECO-070',N'Sellador 5 x 1 reforzado, reductor recomendado, agua limpia en una proporción de 3 partes de agua por 1 de sellador, en cubeta de 19 lt.',N'cb',1.679887,1249,2098.17,0.0022309955,N'Materiales',N'Pinturas'),
    (N'RECO-126',N'Primario anticorrosivo alquidalico base agua 1 lt color gris claro línea acqua 100 primer',N'lt',1,399,399,0.0004242589,N'Materiales',N'Pinturas'),
    (N'RECU-066',N'Estopa blanca',N'kg',0.012,54.78,0.66,0.0000007018,N'Materiales',N'Consumible'),
    (N'RECU-077',N'Lija para agua 600 marca Fandeli',N'pza',0,14.34,0,0,N'Materiales',N'Consumible'),
    (N'RECU-082',N'Primario para metal color gris',N'lt',0.842868,193,162.67,0.0001729679,N'Materiales',N'Pinturas'),
    (N'ROCA-408',N'Block dala gris hueco 12 x 20 x 40 cm',N'pza',609.502425,16.87,10282.29,0.0109332144,N'Materiales',N'Block'),
    (N'ROCA-410',N'Block entero gris hueco 20 x 20 x 40 cm',N'pza',30.735001,20.72,636.83,0.0006771448,N'Materiales',N'Block'),
    (N'ROSETA 4',N'Portalámpara 4"  de cerámica color blanco mca Igesa casquillo E27',N'pza',25,19,475,0.0005050701,N'Materiales',N'Electricidad'),
    (N'SAR2-1079',N'Cespol para lavabo acabado cromo',N'pza',2,250,500,0.0005316527,N'Materiales',N'Plomeria'),
    (N'SAR2-1995',N'Monomando para regadera Miura marca Helvex',N'pza',3,2298,6894,0.0073304274,N'Materiales',N'Accesorios B'),
    (N'SEPARADOR 3MM',N'Separador 3 mm mca Interceramic 200 pzas',N'bolsa',4.83,44.82,216.48,0.0002301844,N'Materiales',N'Consumible'),
    (N'SIMON GFCI 2502425-060',N'Interruptor de circuito por falla a tierra GFCI con auto test; incluye chasis y placa mca Simon serie 25',N'pza',2,263.91,527.82,0.0005612339,N'Materiales',N'Electricidad'),
    (N'SIMON INT 25101-30',N'Interruptor sencillo 10 Amp 127 V serie 25, mca Simon',N'pza',19,18.89,358.91,0.0003816309,N'Materiales',N'Electricidad'),
    (N'SIMON INT ESC 25201-30',N'Interruptor de escalera 3 vias 10 Amp 127 V mca Simon serie 25',N'pza',4,24.72,98.88,0.0001051396,N'Materiales',N'Electricidad'),
    (N'SIMON PL 2500098-035',N'Placa con tapa para intemperie IP44 con chasis incluido, mca Simon 25',N'pza',2,62.18,124.36,0.0001322327,N'Materiales',N'Electricidad'),
    (N'SIMON PL 25611-30',N'Placa de 1V con chasis mca Simon serie 25',N'pza',5,17.81,89.05,0.0000946873,N'Materiales',N'Electricidad'),
    (N'SIMON PL 25612-30',N'Placa 2V con chasis color blanco  mca Simon serie 25',N'pza',29,17.81,516.49,0.0005491866,N'Materiales',N'Electricidad'),
    (N'SIMON TM25424-30',N'Toma de corriente 2P+ T 15 A color blanco  mca Simon serie 25',N'pza',22,29.68,652.96,0.0006942959,N'Materiales',N'Electricidad'),
    (N'SOLVER 100',N'Pasta para soldar 100 gr grupo Solver | S5 603',N'pza',0,113,0,0,N'Materiales',N'Plomeria'),
    (N'TAEL-060',N'Interruptor derivado tipo QO termomagnético 1Px20 Amp marca Square D',N'pza',9,185.84,1672.56,0.0017784421,N'Materiales',N'Electricidad'),
    (N'TAPA MOD R52-88014-W',N'Placa ciega color blanco mca Leviton',N'pza',6,11,66,0.0000701782,N'Materiales',N'Electricidad'),
    (N'TARE BRONCE',N'Tapon registro PVC Sanitario de 4" con tapa de bronce ajustable',N'pza',5,410,2050,0.0021797761,N'Materiales',N'Plomeria'),
    (N'TEFLON 19MM GARLOCK',N'Cinta teflón PTFE premium PLASTITHREAD 3/4" (19mm )x13.20 m',N'Carrete',2.901498,119.65,347.19,0.000369169,N'Materiales',N'Plomeria'),
    (N'TRAMPA FREGADERO',N'Trampa para lavabo fregadero de 16 x 6.5 cm blanco',N'jgo',1,45,45,0.0000478487,N'Materiales',N'Plomeria'),
    (N'TRIM',N'Perfil de aluminio mca Intercderamic curvo plata (2.40 m)',N'pieza',7.578487,180.17,1365.42,0.0014518585,N'Materiales',N'Ceramicos'),
    (N'TUMO-2005',N'Manguera flexible para w.c. de 35cm',N'pza',2,48.56,97.12,0.0001032682,N'Materiales',N'Plomeria'),
    (N'TUMO-2014',N'Pija para w.c',N'pza',2,11.7,23.4,0.0000248813,N'Materiales',N'Consumible'),
    (N'TUPO-100',N'Tubería poliflex naranja con guía de 1¼" (32mm) para uso eléctrico rollo 50 m marca Poliflex.',N'rollo',0.59388,1949.88,1158,0.0012313076,N'Materiales',N'Electricidad'),
    (N'UNIBLOCK STUCCO',N'Stucco (plaster uinblock) marca Uniblock, color blanco 25 kg',N'saco',48.893284,112.07,5479.47,0.00582635,N'Materiales',N'Polvos'),
    (N'V11-30',N'Vigueta V11-30 mca Trabis',N'm ',70.347173,125,8793.4,0.0093500697,N'Materiales',N'Prefabricados'),
    (N'V11-40',N'Vigueta V11-40 mca Trabis',N'm ',39.841428,131,5219.23,0.0055496354,N'Materiales',N'Prefabricados'),
    (N'V16-50',N'Vigueta V16-50 mca Trabis',N'm ',57.99198,170,9858.64,0.0104827451,N'Materiales',N'Prefabricados'),
    (N'VALSAPLAS 71',N'Lavadero con pileta derecha con taquetes y tornillos incluidos mca Valsaplas color gris medidas 71 x 65 x 25 cm',N'pza',1,1100,1100,0.0011696359,N'Materiales',N'Prefabricados'),
    (N'VALV CHECK COLUMPIO 13',N'Válvula check de columpio mca Rugo 79 13 mm (½'''')',N'pza',1,163,163,0.0001733188,N'Materiales',N'Plomeria'),
    (N'VALV ESF GAS LP 19 MM ROS',N'Valvula de control ¾" (19 mm) para gas LP roscable mca Val-Mex',N'pza',1,176.75,176.75,0.0001879392,N'Materiales',N'Plomeria'),
    (N'VALV RET 1/2 PL',N'Válvula check de retención de 1/2"',N'pza',1,58.62,58.62,0.000062331,N'Materiales',N'Plomeria'),
    (N'WELD ON 704 PVC',N'Cemento PVC Weld-On® 704™ bote (946 ml) 1 quart',N'Bote',3.590201,417.15,1497.63,0.0015924381,N'Materiales',N'Plomeria'),
    (N'ZANORI-0101',N'Impermeabilizante asfáltico fibratado emulsión fibratada mca Zahori',N'cubeta',0.900512,525,472.77,0.0005026989,N'Materiales',N'Impermeabilizante'),
    (N'MOCA-001',N'Peón',N'jor',38.783783,642.99,24937.59,0.026516274,N'Mano de Obra',N'Peones'),
    (N'MOCA-002',N'Ayudante general',N'jor',154.875275,691.39,107079.28,0.1138579763,N'Mano de Obra',N'Ayudantes'),
    (N'MOCA-003',N'Cadenero',N'jor',0.03928,642.99,25.26,0.0000268591,N'Mano de Obra',N'Oficiales de albañilería'),
    (N'MOCA-004',N'Operador equipo menor',N'jor',0.05426,880.94,47.8,0.000050826,N'Mano de Obra',N'Operadores'),
    (N'MOCA-008',N'Ayudante montador y soldador',N'jor',0.837487,681.72,570.93,0.0006070729,N'Mano de Obra',N'Ayudantes'),
    (N'MOCA-009',N'Pintor',N'jor',7.347751,1036.84,7618.44,0.0081007284,N'Mano de Obra',N'Oficiales de albañilería'),
    (N'MOCA-013',N'Cabo de oficios',N'jor',20.921587,1144.03,23934.94,0.0254501509,N'Mano de Obra',N'Supervisores'),
    (N'MOCA-014',N'Albañil',N'jor',55.678527,1056.35,58815.98,0.0625393489,N'Mano de Obra',N'Oficiales de albañilería'),
    (N'MOCA-015',N'Electricista',N'jor',26.336195,1134.29,29872.88,0.0317639945,N'Mano de Obra',N'Oficiales calificados'),
    (N'MOCA-016',N'Plomero',N'jor',16.089323,1134.29,18249.89,0.0194052065,N'Mano de Obra',N'Oficiales de albañilería'),
    (N'MOCA-018',N'Carpintero obra negra',N'jor',13.494444,1114.82,15043.86,0.0159962175,N'Mano de Obra',N'Oficiales de albañilería'),
    (N'MOCA-019',N'Yesero',N'jor',28.412714,1036.84,29459.45,0.0313243922,N'Mano de Obra',N'Oficiales de albañilería'),
    (N'MOCA-020',N'Tablaroquero',N'jor',4.528329,1064.12,4818.69,0.0051237391,N'Mano de Obra',N'Oficiales de albañilería'),
    (N'MOCA-021',N'Colocador',N'jor',13.319034,1036.84,13809.71,0.0146839392,N'Mano de Obra',N'Oficiales de albañilería'),
    (N'MOCA-022',N'Ebanista',N'jor',3.205425,1144.03,3667.1,0.0038992472,N'Mano de Obra',N'Oficiales calificados'),
    (N'MOCA-023',N'Herrero',N'jor',4.147754,1110.91,4607.78,0.0048994773,N'Mano de Obra',N'Oficiales calificados'),
    (N'MOCA-027',N'Fierrero',N'jor',12.58828,1085.59,13665.69,0.0145308019,N'Mano de Obra',N'Oficiales de albañilería'),
    (N'MOCA-032',N'Topógrafo',N'jor',0.03928,1231.76,48.38,0.0000514427,N'Mano de Obra',N'Oficiales calificados'),
    (N'MOCA-033',N'Soldador calificado',N'jor',0.837487,1231.76,1031.57,0.0010968739,N'Mano de Obra',N'Oficiales calificados'),
    (N'MOCA-035',N'Oficial de instalaciones de gas',N'jor',0.859212,1134.29,974.59,0.0010362868,N'Mano de Obra',N'Oficiales calificados'),
    (N'MOCA-037',N'Ayudante electricista',N'jor',26.336195,717.53,18897.02,0.0200933033,N'Mano de Obra',N'Ayudantes'),
    (N'MOCA-038',N'Ayudante plomero',N'jor',16.089323,702.05,11295.38,0.0120104385,N'Mano de Obra',N'Ayudantes'),
    (N'MOCA-039',N'Ayudante tablaroquero',N'jor',4.528329,681.72,3087.05,0.0032824769,N'Mano de Obra',N'Ayudantes'),
    (N'MOCA-041',N'Ayudante de instalaciones de gas',N'jor',0.859212,681.72,585.74,0.0006228205,N'Mano de Obra',N'Ayudantes'),
    (N'MOCA-061',N'Of Colador',N'jor',1.831388,1057.11,1935.97,0.0020585273,N'Mano de Obra',N'General'),
    (N'FACHEME',N'Herramienta menor',N'(%)mo',0.03,392695.16,11780.85,0.0125266414,N'Herramienta',N'General'),
    (N'HEMN-117',N'Andamio metálico a base tubular y hasta 4.0 m de altura.',N'rta/día',92.856623,19.14,1777.29,0.001889802,N'Herramienta',N'General'),
    (N'HESEG-001',N'Porcentaje de equipo de seguridad',N'(%)mo',0.02,392695.16,7853.9,0.0083510943,N'Herramienta',N'General'),
    (N'MATMAN001',N'Materiales menores limpieza',N'(%)mo',0.1,10587.64,1058.76,0.0011257852,N'Herramienta',N'General'),
    (N'AMAIN-001',N'Revolvedora para concreto marca Cipsa modelo R10 de un saco tipo trompo, cap. 5 m3/hr, motor a gasolina marca Kohler de 8 HP, con reductor, montada sobre ruedas tipo B78X-13, peso de la máquina con motor 363 kg.',N'hora',11.81215,182.73,2158.43,0.0022950703,N'Equipo',N'General'),
    (N'AMAIN-006',N'Compresor portatil Ingerson Rand modelo P185',N'hora',0.5,296.7,148.36,0.000157752,N'Equipo',N'General'),
    (N'AMAIN-010',N'Soldadora marca Lincoln modelo SAE 300 cap. 300 Amp.',N'hora',3.712149,463.02,1718.8,0.0018276093,N'Equipo',N'General'),
    (N'AMAIN-013',N'Equipo oxí-acetileno',N'hora',0.712149,113.16,80.57,0.0000856705,N'Equipo',N'General'),
    (N'AMALI-014',N'Bailarina neumática marca Wacker modelo BS 502i de gasolinaxde 700g/m y penetracion de hasta 51 cm con zapata de 28 x 33 cms',N'hora',1.429121,84.58,120.88,0.0001285324,N'Equipo',N'General'),
    (N'AMALI-017',N'Vibrador de gasolina marca Felsa modelo vibromax cap. 12000 VPM, con manguera  de 4.00 m, y cabezal de por 38 mm ( 1½"), con motor de gasolina de 4 HP',N'hora',9.533289,163.54,1559.09,0.0016577888,N'Equipo',N'General'),
    (N'AMAPE-033',N'Retroexcavadora cargadora Caterpillar 420 E 93 H.P',N'hora',0.50177,862.39,432.72,0.0004601135,N'Equipo',N'General'),
    (N'AMAPE-039',N'Camión de volteo marca DINA de 7 m3 de capacidad.',N'hora',2.643568,877.86,2320.69,0.0024676022,N'Equipo',N'General'),
    (N'AMAPE-233',N'Vibrador Stow AW 1680 de 8 hp, flecha flexible 20 ft sin operador.',N'hora',1.250002,89.12,111.4,0.0001184522,N'Equipo',N'General'),
    (N'AMAPE-329',N'Nivel automatico (+/-) 2mm de precisión, con imagen directa con apertura del objetivo de 40 mm marca BOIF, Incluye: Estuche de transporte, Manual de operación, kit de herramientas y Tripíe de aluminio.',N'hora',0.01964,53.01,1.04,0.0000011058,N'Equipo',N'SERVICIOS'),
    (N'CATER 0110',N'Retroexcavadora Cargadora Caterpillar 416F, de 87 H.P., profundidad estandar de excavación 4.36 m, cucharón de 0.96 m3',N'hora',0.754698,794.4,599.53,0.0006374835,N'Equipo',N'General'),
    (N'BASTIDOR TU',N'Bastidor de aluminio',N'pza',2,474.14,948.28,0.0010083112,N'Trabajos',N'General'),
    (N'ELASTOMERICO TUNERA',N'Suministro y aplicación de impermeabilizante acrilico elastomerico  base aguas aplicado en azoteas, incluye: limpieza, sellado, materiales, mano de obra y herramienta.',N'm2',60.98,92.8,5658.94,0.0060171814,N'Trabajos',N'General'),
    (N'FUMI TERMITA',N'fumigación antitermita con producto Clorpirifos equivalente a Termidel en parte bajo firme; incluye: material, mano de obra, herramienta, equipo y todos los elementos necesarios para su correcta ejecución.',N'm2',51.66,20.88,1078.66,0.001146945,N'Trabajos',N'General'),
    (N'POLIURETANO 1PLG',N'Poliuretano espreado rigido, espesor de 1 pulgada  con una densidad de 35 kg/m3; producto ecológico  aplicado en azoteas, incluye: limpieza, sellado, materiales, mano de obra y herramienta.',N'm2',60.98,162.4,9903.15,0.0105300728,N'Trabajos',N'General'),
    (N'VENTA TU 0.90-0.62',N'Ventana corrediza XO de 0.90m x 0.62m en aluminio de 1.5"
color natural con vidrio opaco de 4mm',N'pza',2,1777.25,3554.5,0.003779519,N'Trabajos',N'General'),
    (N'VENTA TU 1.00-1.26',N'Ventana fija de 1.00m x 1.26m en aluminio de 1.5" color
natural con vidrio claro de 3mm',N'pza',1,3009.97,3009.97,0.0032005173,N'Trabajos',N'General'),
    (N'VENTA TU 1.24-1.0',N'Ventana corrediza XO de 1.24m x 1.00m en aluminio de 1.5"
color natural con vidrio claro de 3mm',N'pza',1,2852.64,2852.64,0.0030332275,N'Trabajos',N'General'),
    (N'VENTA TU 1.24-1.26',N'Ventana corrediza XO de 1.24m x 1.26m en aluminio de 1.5"
color natural con vidrio claro de 3mm',N'pza',2,3652.65,7305.3,0.0077677649,N'Trabajos',N'General'),
    (N'VENTA TU 1.68-1.24 MOD',N'Ventana compuesta XO/O de 1.68m x 1.24m en aluminio de. 1.5" color natural y vidrio claro de 3mm.',N'pza',2,5246.2,10492.4,0.0111566255,N'Trabajos',N'General')
) AS s([Clave],[Descripción],[Unidad],[Cantidad],[Costo],[Importe],[Porcentaje],[Tipo],[Familia])
ON t.[Clave] = s.[Clave]
WHEN MATCHED THEN UPDATE SET
    t.[Descripción]=s.[Descripción], t.[Unidad]=s.[Unidad], t.[Cantidad]=s.[Cantidad],
    t.[Costo]=s.[Costo], t.[Importe]=s.[Importe], t.[Porcentaje]=s.[Porcentaje],
    t.[Tipo]=s.[Tipo], t.[Familia]=s.[Familia]
WHEN NOT MATCHED THEN INSERT
    ([Clave],[Descripción],[Unidad],[Cantidad],[Costo],[Importe],[Porcentaje],[Tipo],[Familia])
    VALUES (s.[Clave],s.[Descripción],s.[Unidad],s.[Cantidad],s.[Costo],s.[Importe],s.[Porcentaje],s.[Tipo],s.[Familia]);
GO

PRINT 'Upsert InsumosTuneraEXP completado.';
GO

-- ============================================================================
-- Asociar por nombre el catalogo con los insumos capturados en el treelist.
-- Para cada nodo Nivel 2 cuyo Nombre coincide (sin espacios) con la Descripcion
-- de un insumo del catalogo, se escribe/actualiza su valor de columna 'Clave'.
-- Detecta el nombre real de la tabla (renombrada RutaTuneraDestajo o la antigua
-- TreeListData) y trabaja por SQL dinamico.
-- ============================================================================
SET XACT_ABORT ON;

DECLARE @nodos SYSNAME = NULL, @cols SYSNAME = NULL;
IF OBJECT_ID(N'dbo.RutaTuneraDestajo', N'U') IS NOT NULL
       AND OBJECT_ID(N'dbo.RutaTuneraDestajo_Columnas', N'U') IS NOT NULL
BEGIN
    SET @nodos = N'RutaTuneraDestajo';
    SET @cols  = N'RutaTuneraDestajo_Columnas';
END
ELSE IF OBJECT_ID(N'dbo.TreeListData', N'U') IS NOT NULL
            AND OBJECT_ID(N'dbo.TreeListData_Columnas', N'U') IS NOT NULL
BEGIN
    SET @nodos = N'TreeListData';
    SET @cols  = N'TreeListData_Columnas';
END

IF @nodos IS NULL
    PRINT 'No se encontro la tabla del treelist (RutaTuneraDestajo/TreeListData); se omite la asociacion.';
ELSE
BEGIN
    DECLARE @afect INT = 0;
    DECLARE @sql NVARCHAR(MAX) = N'
    ;WITH cat AS (
        SELECT Descripcion = LTRIM(RTRIM([Descripción])),
               Clave,
               rn = ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM([Descripción])) ORDER BY Clave)
        FROM dbo.InsumosTuneraEXP
        WHERE [Descripción] IS NOT NULL AND LEN(LTRIM(RTRIM([Descripción]))) > 0
    ),
    nodo AS (
        SELECT n.ID, c.Clave
        FROM dbo.' + QUOTENAME(@nodos) + N' n
        INNER JOIN cat c ON c.rn = 1 AND LTRIM(RTRIM(n.Nombre)) = c.Descripcion
        WHERE n.Nivel = 2
    )
    MERGE dbo.' + QUOTENAME(@cols) + N' AS t
    USING nodo AS s ON t.NodoID = s.ID AND t.NombreColumna = N''Clave''
    WHEN MATCHED AND ISNULL(t.Valor, N'''') <> s.Clave
        THEN UPDATE SET t.Valor = s.Clave
    WHEN NOT MATCHED BY TARGET
        THEN INSERT (NodoID, NombreColumna, Valor) VALUES (s.ID, N''Clave'', s.Clave);
    SET @afect = @@ROWCOUNT;';

    EXEC sp_executesql @sql, N'@afect INT OUTPUT', @afect = @afect OUTPUT;
    PRINT 'Insumos del treelist asociados al catalogo por nombre (insertados/actualizados): '
          + CAST(@afect AS NVARCHAR(10)) + ' en ' + @nodos + '.';
END
GO
