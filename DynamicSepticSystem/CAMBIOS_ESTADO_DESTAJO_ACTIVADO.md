# Cambios: Estado Activado/Desactivado para Destajos

## Resumen
Se ha agregado un nuevo estado **"Activado/Desactivado"** para los destajos (Nivel 1). Un destajo ahora se considera completamente **activado** cuando:
1. Está marcado como activo (checkbox marcado)
2. Tiene una cuadrilla asignada
3. El estado `DesatajoActivado = true`

## Cambios Implementados

### 1. Modelo de Datos
**Archivo**: `FormActivarTareasTreeList.cs` - Clase `ItemTareaActivacion`

```csharp
public bool DesatajoActivado { get; set; } = false;
```

- Nueva propiedad que indica si el destajo está completamente activado (con cuadrilla asignada)

### 2. Base de Datos
**Tabla**: `ActivacionTareasRuta`

Nueva columna:
```sql
DesatajoActivado BIT DEFAULT 0
```

Se agregan validaciones en `CargarActivacionesGuardadas()` para crear la columna automáticamente si no existe.

### 3. Interfaz de Usuario

#### Nueva Columna "Estado"
- Ancho: 90 píxeles
- Muestra:
  - `"? Activado"` cuando `DesatajoActivado = true`
  - `"? Desactivado"` cuando `DesatajoActivado = false`
  - Vacío para otros niveles

#### Colores Visuales (Nivel 1)
- **Verde claro** (`RGB(200, 230, 201)`): Destajo completamente activado
- **Amarillo suave** (`RGB(255, 243, 224)`): Destajo con cuadrilla pero no completamente activado
- **Gris azulado** (`RGB(243, 247, 252)`): Destajo sin cuadrilla

#### Estadísticas
Se muestra el contador de "Completamente activados" en la barra de estado:
```
"Destajos activos: X de Y   ·   Con cuadrilla: Z   ·   Completamente activados: W   ·   Importe: $$$"
```

### 4. Lógica de Flujo

#### Activación Manual de Destajo
1. Usuario marca checkbox de destajo (Nivel 1)
2. Se abre formulario `FormAsignarCuadrilla`
3. Si se asigna cuadrilla exitosamente:
   - `item.Activa = true`
   - `item.CuadrillaAsignada = "CODIGO_CUADRILLA"`
   - `item.DesatajoActivado = true` ? **NUEVO**
   - Se persiste en BD
   - Se genera PDF automáticamente

#### Desactivación de Destajo
Cuando el usuario desmarca un destajo:
```csharp
item.Activa = false;
item.CuadrillaAsignada = "";
item.DesatajoActivado = false;  // ? NUEVO
```

#### Limpieza por Nivel
Los botones "Desmarcar por Nivel" también limpian `DesatajoActivado = false`

### 5. Validaciones para PDF

#### En `GenerarPdfDestajo()`
```csharp
if (!destajo.DesatajoActivado || string.IsNullOrEmpty(destajo.CuadrillaAsignada))
{
    MessageBox.Show("El destajo no está completamente activado...");
    return;
}
```

**No se puede generar PDF si:**
- `DesatajoActivado = false`
- O la cuadrilla no está asignada

#### En `MenuItemRegenerarPdf_Click()`
Se valida primero que `DesatajoActivado = true` antes de permitir regenerar el PDF.

### 6. Persistencia

#### Guardar Destajo Individual
`PersistirActivacionDestajo()`:
```sql
INSERT INTO ActivacionTareasRuta
(..., DesatajoActivado, ...)
VALUES (..., @desatActivado, ...)
```

#### Guardar Todos los Destajos
`btnGuardar_Click()`:
```sql
INSERT INTO ActivacionTareasRuta
(..., DesatajoActivado, ...)
VALUES (..., @desatActivado, ...)
```

#### Cargar Destajos Guardados
`CargarActivacionesGuardadas()`:
```csharp
bool desatajoActivado = Convert.ToBoolean(reader["DesatajoActivado"]);
item.DesatajoActivado = desatajoActivado;
```

## Flujo de Estados (Destajo Nivel 1)

```
???????????????????????
?  INICIAL            ?
? Activa: false       ?
? DesatajoActivado: ? ?
? Cuadrilla: -        ?
???????????????????????
           ? Usuario marca checkbox
           ?
???????????????????????????????????
?  FormAsignarCuadrilla se abre   ?
???????????????????????????????????
           ?
    ???????????????
    ?             ?
    ? OK          ? Cancelar
????????????????????????  ????????????????????????
?  ACTIVADO            ?  ?  REVIERTE A INICIAL  ?
? Activa: true         ?  ? Activa: false        ?
? DesatajoActivado: ?  ?  ? DesatajoActivado: ?  ?
? Cuadrilla: CODIGO    ?  ? Cuadrilla: -         ?
????????????????????????  ????????????????????????
           ?
           ? Generar PDF automáticamente
           
????????????????????????????????????????
?  Destajo completamente OPERACIONAL   ?
?  ? Se puede regenerar PDF en cualquier momento
????????????????????????????????????????
           ? Usuario desmarca
           ?
????????????????????????
?  DESACTIVADO         ?
? Activa: false        ?
? DesatajoActivado: ?  ?
? Cuadrilla: -         ?
????????????????????????
```

## Beneficios

1. **Claridad**: Distingue entre "activo" (seleccionado) y "completamente activado" (con cuadrilla)
2. **Seguridad**: Impide generar PDF para destajos incompletos
3. **Trazabilidad**: Registra el estado exacto en BD
4. **Visualización**: Colores intuitivos muestran el estado de cada destajo
5. **Estadísticas**: Seguimiento de destajos completamente operacionales

## Testing

Para verificar el funcionamiento:

1. ? Marcar un destajo ? se abre FormAsignarCuadrilla
2. ? Cancelar ? se revierte a desactivado
3. ? Asignar cuadrilla ? se activa automáticamente y genera PDF
4. ? Verificar columna "Estado" muestra "? Activado"
5. ? Verificar color verde en la fila
6. ? Desmarca destajo ? se limpia todo (Activa, DesatajoActivado, Cuadrilla)
7. ? Intentar regenerar PDF de destajo desactivado ? muestra error
8. ? Guardar configuración ? persiste DesatajoActivado en BD
9. ? Cargar nuevamente casa ? carga correctamente los estados
