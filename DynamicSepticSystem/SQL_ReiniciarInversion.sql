-- Reinicia la bitácora de Inversión general: borra todos los movimientos y
-- deja el Aplicado de cada categoría en 0. El presupuesto (Importe) NO se toca.
-- Córrelo en la BD de la obra (CALANDRIA u otra), no en la master.
--
-- Revisa antes con el SELECT de abajo; cuando estés conforme, quita el ROLLBACK
-- y deja el COMMIT.

BEGIN TRANSACTION;

DELETE FROM dbo.InversionMovimientos;

UPDATE dbo.InversionObra
   SET Aplicado = 0,
       FechaActualizacion = GETDATE(),
       UsuarioModificacion = 'reinicio';

SELECT Categoria, Orden, Importe, Aplicado FROM dbo.InversionObra ORDER BY Orden;

ROLLBACK TRANSACTION;   -- cámbialo por COMMIT TRANSACTION para aplicar
