# ?? ACTUALIZACIÓN: Explosión de Insumos desde Carrito Manual

## ?? Descripción

Se ha implementado la funcionalidad para **actualizar automáticamente la explosión de insumos** cuando se agregan insumos nuevos manualmente desde el botón "**+ AGREGAR NUEVO INSUMO**" en `FormCompraMulti`.

## ? Funcionalidad Implementada

### Antes (?)
- Al agregar un insumo manualmente con `FormAgregarInsumoCarrito`, el insumo se agregaba **SOLO al carrito** de la orden actual.
- No se actualizaba en las tablas de explosión (`COMPRASCALANDRA`, `COMPRASTUNERA`).
- El insumo NO estaba disponible para futuras órdenes de compra de esos prototipos.

### Ahora (?)
- Al agregar un insumo manualmente, el sistema **pregunta al usuario** si desea agregarlo a la explosión de insumos.
- Si el usuario acepta, el insumo se agrega a las tablas de explosión de **todos los prototipos** de las casas seleccionadas.
- El insumo queda disponible permanentemente para futuras órdenes de compra.
- Se actualiza automáticamente el catálogo de insumos para reflejar los cambios.

## ?? Flujo de Trabajo

### 1?? Agregar Insumo Manual

```
Usuario hace clic en: [+ AGREGAR NUEVO INSUMO]
     ?
Se abre FormAgregarInsumoCarrito
     ?
Usuario ingresa:
  - Clave: NUEVO-001
  - Descripción: Insumo especial
  - Unidad: PZA
  - Cantidad: 10.000
  - Precio: $50.00
     ?
Clic en [? ACEPTAR]
```

### 2?? Diálogo de Confirmación

Si hay casas seleccionadas, aparece el siguiente mensaje:

```
???????????????????????????????????????????????????????????
?  ? Actualizar explosión de insumos                     ?
???????????????????????????????????????????????????????????
?                                                          ?
?  ¿Deseas agregar este insumo a la explosión de          ?
?  insumos de los prototipos seleccionados?               ?
?                                                          ?
?  Si seleccionas 'Sí', el insumo 'NUEVO-001' se          ?
?  agregará permanentemente a las tablas de explosión     ?
?  y estará disponible para futuras órdenes de compra     ?
?  de estos prototipos.                                   ?
?                                                          ?
?  Si seleccionas 'No', el insumo solo se agregará al    ?
?  carrito de esta orden.                                 ?
?                                                          ?
?         [  SÍ  ]    [  NO  ]    [ CANCELAR ]           ?
???????????????????????????????????????????????????????????
```

### 3?? Resultados según la Opción

#### ? Opción: **SÍ**
- El insumo se agrega a la(s) tabla(s) de explosión correspondiente(s)
- Se actualiza el catálogo de insumos
- El insumo se agrega al carrito
- Mensaje de confirmación:
  ```
  ? Insumo 'NUEVO-001' agregado exitosamente a 1 tabla(s) de explosión.
  
  Prototipos actualizados: CALANDRIA
  ```

#### ? Opción: **NO**
- El insumo se agrega **SOLO** al carrito de esta orden
- NO se modifica la explosión de insumos
- El insumo NO estará disponible en futuras órdenes

#### ?? Opción: **CANCELAR**
- No se realiza ninguna acción
- Se regresa al formulario

## ?? Detalles Técnicos

### Nuevo Método: `AgregarInsumoAExplosion()`

```csharp
private void AgregarInsumoAExplosion(string clave, string descripcion, string unidad, decimal cantidadPorCasa)
{
    // 1. Obtener prototipos únicos de las casas seleccionadas
    var prototiposUnicos = lstCasas.Items.Cast<CasaSeleccionada>()
        .Select(c => c.Prototipo)
        .Distinct()
        .ToList();

    // 2. Para cada prototipo:
    foreach (string prototipo in prototiposUnicos)
    {
        string tabla = GetExplosionTableForPrototipo(prototipo);
        
        // 3. Verificar si el insumo ya existe
        //    - Si existe: ACTUALIZAR cantidad, descripción, unidad
        //    - Si no existe: INSERTAR nuevo registro
        
        // 4. Marcar con Familia = "MANUAL"
    }
    
    // 5. Actualizar catálogo de insumos
    ActualizarCatalogoInsumos();
}
```

### Tablas Afectadas

Dependiendo del prototipo de las casas seleccionadas:

| Prototipo | Tabla Actualizada |
|-----------|-------------------|
| CALANDRIA | `COMPRASCALANDRA` |
| TUNERA    | `COMPRASTUNERA`   |
| Otro      | `COMPRASCALANDRA` (fallback) |

### Campos Insertados/Actualizados

```sql
INSERT INTO [TABLA] (Clave, Descripcion, Unidad, Cantidad, Familia)
VALUES (@clave, @desc, @unidad, @cantidad, 'MANUAL')

-- O si ya existe:
UPDATE [TABLA] 
SET Descripcion = @desc, 
    Unidad = @unidad, 
    Cantidad = @cantidad, 
    Familia = 'MANUAL'
WHERE Clave = @clave
```

## ?? Ejemplo Práctico

### Escenario:
```
Casas seleccionadas:
  - M5-L1 (CALANDRIA)
  - M5-L2 (CALANDRIA)
  - M3-L5 (TUNERA)
```

### Acción:
```
Agregar insumo manual:
  Clave: PINTURA-ESP-01
  Descripción: Pintura especial anti-humedad
  Unidad: LITRO
  Cantidad: 5.5
  Precio: $250.00
```

### Resultado (si usuario elige "SÍ"):
```
? Tabla COMPRASCALANDRA:
   - Se agrega/actualiza PINTURA-ESP-01
   - Cantidad por casa: 5.5 LITRO
   - Familia: MANUAL

? Tabla COMPRASTUNERA:
   - Se agrega/actualiza PINTURA-ESP-01
   - Cantidad por casa: 5.5 LITRO
   - Familia: MANUAL

? Catálogo actualizado:
   - PINTURA-ESP-01 aparece con cantidad total: 16.5
     (5.5 × 2 casas CALANDRIA + 5.5 × 1 casa TUNERA)

? Carrito:
   - PINTURA-ESP-01 agregado con cantidad 16.5 y precio $250.00
```

## ?? Consideraciones Importantes

### 1. Cantidad por Casa vs Cantidad Total

- **En la explosión**: Se guarda la cantidad **por casa individual**
  ```
  Ejemplo: 5.5 litros por casa
  ```

- **En el carrito**: Se muestra la cantidad **total** según las casas seleccionadas
  ```
  Ejemplo: 5.5 × 3 casas = 16.5 litros
  ```

### 2. Actualizaciones vs Inserciones

- Si el insumo **ya existe** en la tabla de explosión:
  - Se **ACTUALIZA** la cantidad, descripción y unidad
  - ?? **Cuidado**: Esto afecta a TODAS las órdenes futuras de ese prototipo

- Si el insumo **no existe**:
  - Se **INSERTA** como nuevo registro
  - Marcado con `Familia = 'MANUAL'`

### 3. Identificación de Insumos Manuales

Todos los insumos agregados manualmente tienen:
```sql
Familia = 'MANUAL'
```

Esto permite:
- Identificar insumos agregados manualmente
- Filtrar en reportes o análisis
- Distinguir de insumos originales del prototipo

### 4. Recalculo Automático

Después de agregar a la explosión, se ejecuta:
```csharp
ActualizarCatalogoInsumos();
```

Esto recalcula:
- Cantidades totales según casas seleccionadas
- Sincroniza límites máximos del carrito
- Actualiza la visualización en el `olvCatalogo`

## ?? Manejo de Errores

### Error: Tabla de explosión no encontrada
```
? Errores encontrados:
Error en COMPRASCALANDRA: Invalid object name 'COMPRASCALANDRA'
```

**Solución**: Verificar que la tabla exista en la base de datos.

### Error: Violación de clave única
```
? Errores encontrados:
Error en COMPRASTUNERA: Cannot insert duplicate key
```

**Solución**: El sistema detecta esto y actualiza en lugar de insertar.

### Error: Permiso denegado
```
? Errores encontrados:
Error en COMPRASCALANDRA: INSERT permission denied
```

**Solución**: Verificar permisos del usuario en la base de datos.

## ? Pruebas Recomendadas

### Test 1: Agregar insumo nuevo
1. Seleccionar 2 casas del mismo prototipo
2. Agregar insumo manual
3. Elegir "SÍ" para agregar a explosión
4. ? Verificar que aparece en catálogo con cantidad × 2

### Test 2: Actualizar insumo existente
1. Agregar insumo que YA existe en la explosión
2. Cambiar la cantidad
3. Elegir "SÍ"
4. ? Verificar que se actualizó la cantidad en la BD

### Test 3: Múltiples prototipos
1. Seleccionar casas de CALANDRIA y TUNERA
2. Agregar insumo manual
3. Elegir "SÍ"
4. ? Verificar en SQL que se agregó a ambas tablas

### Test 4: Solo carrito (no explosión)
1. Agregar insumo manual
2. Elegir "NO"
3. ? Verificar que está en carrito pero NO en explosión

### Test 5: Cancelar operación
1. Agregar insumo manual
2. Elegir "CANCELAR"
3. ? Verificar que no se agregó nada

## ?? SQL para Verificación

### Ver insumos manuales en explosión
```sql
-- Insumos manuales en CALANDRIA
SELECT * FROM COMPRASCALANDRA WHERE Familia = 'MANUAL'

-- Insumos manuales en TUNERA
SELECT * FROM COMPRASTUNERA WHERE Familia = 'MANUAL'
```

### Verificar insumo específico
```sql
SELECT 
    'CALANDRIA' AS Prototipo,
    Clave, 
    Descripcion, 
    Unidad, 
    Cantidad, 
    Familia
FROM COMPRASCALANDRA
WHERE Clave = 'PINTURA-ESP-01'

UNION ALL

SELECT 
    'TUNERA' AS Prototipo,
    Clave, 
    Descripcion, 
    Unidad, 
    Cantidad, 
    Familia
FROM COMPRASTUNERA
WHERE Clave = 'PINTURA-ESP-01'
```

### Eliminar insumo manual de explosión
```sql
-- PRECAUCIÓN: Esto eliminará el insumo de todas las explosiones futuras
DELETE FROM COMPRASCALANDRA WHERE Clave = 'PINTURA-ESP-01' AND Familia = 'MANUAL'
DELETE FROM COMPRASTUNERA WHERE Clave = 'PINTURA-ESP-01' AND Familia = 'MANUAL'
```

## ?? Casos de Uso

### Caso 1: Insumo Especial para Obra Específica
**Situación**: Necesitas un material especial que no está en el catálogo estándar.

**Solución**: Agregar manualmente y elegir "NO" para que solo aparezca en esta orden.

### Caso 2: Nuevo Insumo Estándar
**Situación**: Descubres que falta un insumo que debería estar en todas las casas de este prototipo.

**Solución**: Agregar manualmente y elegir "SÍ" para que se incluya en la explosión y aparezca en futuras órdenes.

### Caso 3: Corrección de Cantidad
**Situación**: La cantidad en la explosión está incorrecta.

**Solución**: Agregar el insumo con la cantidad correcta y elegir "SÍ" para actualizar la explosión.

## ?? Flujo Completo Actualizado

```
FormCompraMulti
    ?
[+ AGREGAR NUEVO INSUMO] ? Usuario hace clic
    ?
FormAgregarInsumoCarrito ? Se abre
    ?
Usuario llena campos y confirma
    ?
¿Existe en carrito? 
    ?? SÍ ? Mostrar error "Ya existe"
    ?? NO ? Continuar
         ?
¿Hay casas seleccionadas?
    ?? NO ? Agregar solo al carrito
    ?? SÍ ? Mostrar diálogo de confirmación
         ?
    ¿Agregar a explosión?
         ?? CANCELAR ? No hacer nada
         ?? NO ? Agregar solo al carrito
         ?? SÍ ? AgregarInsumoAExplosion()
              ?
         Obtener prototipos únicos
              ?
         Para cada prototipo:
              ?? GetExplosionTableForPrototipo()
              ?? Verificar si existe
              ?? INSERT o UPDATE
              ?? Marcar Familia = 'MANUAL'
              ?
         ActualizarCatalogoInsumos()
              ?
         Mostrar resultado
              ?
Agregar al carrito
    ?
Mostrar confirmación
```

## ?? Soporte

Si encuentras algún problema con esta funcionalidad:
1. Verifica los permisos de base de datos
2. Revisa que las tablas de explosión existan
3. Consulta los logs de error
4. Contacta al equipo de desarrollo

---

**Versión**: 1.0  
**Fecha**: 2024  
**Desarrollador**: Sistema Calandria  
**Framework**: .NET Framework 4.7.2
