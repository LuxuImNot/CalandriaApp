-- ============================================================================
-- Personalización por obra: logo + paleta de 3 colores sugerida a partir del
-- logo (ExtractorColoresLogo), usada como acento en las pantallas web de esa
-- obra. Corre contra CalandriaControl (la BD maestra, tabla Obras).
-- Idempotente: se puede correr varias veces sin duplicar nada.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Obras') AND name = 'LogoBytes')
    ALTER TABLE Obras ADD LogoBytes VARBINARY(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Obras') AND name = 'LogoExtension')
    ALTER TABLE Obras ADD LogoExtension NVARCHAR(10) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Obras') AND name = 'ColorPrimario')
    ALTER TABLE Obras ADD ColorPrimario NVARCHAR(9) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Obras') AND name = 'ColorSecundario')
    ALTER TABLE Obras ADD ColorSecundario NVARCHAR(9) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Obras') AND name = 'ColorSuave')
    ALTER TABLE Obras ADD ColorSuave NVARCHAR(9) NULL;
