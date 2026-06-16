# Sistema de Proveedores Mejorado - CALANDRIA

## ?? Cambios Implementados

Se ha actualizado completamente el sistema de proveedores para incluir información fiscal y de contacto completa, reemplazando el sistema anterior de solo código y nombre.

## ??? Nueva Estructura de Base de Datos

### Tabla: PROVEEDORESCALANDRIA

```sql
CREATE TABLE dbo.PROVEEDORESCALANDRIA (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ClaveUnica NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(200) NOT NULL,
    RFC NVARCHAR(13) NOT NULL,
    Direccion NVARCHAR(500) NULL,
    Telefono NVARCHAR(20) NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
);
```

### Campos:

- **ClaveUnica** *(obligatorio)*: Identificador único del proveedor (ej: `PROV-001`, `FERRE-HMO`)
- **Nombre** *(obligatorio)*: Razón social o nombre comercial completo
- **RFC** *(obligatorio)*: Registro Federal de Contribuyentes (12-13 caracteres)
- **Direccion** *(opcional)*: Dirección física completa del proveedor
- **Telefono** *(opcional)*: Número de contacto
- **FechaCreacion**: Fecha automática de registro

### Modificación a OrdenesCompra

Se agregó una columna para vincular órdenes con proveedores:

```sql
ALTER TABLE OrdenesCompra
ADD ProveedorClave NVARCHAR(50) NULL
```

## ? Nuevas Funcionalidades

### 1. Formulario de Agregar Proveedor (FormAgregarProveedor)

#### Campos Actualizados:
- **Clave Única*** - Se convierte automáticamente a mayúsculas
- **Nombre*** - Razón social completa
- **RFC*** - Validación de longitud (12-13 caracteres)
- **Dirección** - Texto multilínea para dirección completa
- **Teléfono** - Número de contacto

*(* = campos obligatorios)*

#### Validaciones:
- Clave única no duplicada
- RFC con formato válido (12-13 caracteres)
- Todos los campos obligatorios completos
- Conversión automática a mayúsculas para Clave y RFC

### 2. Integración en FormCompraIndirecta

#### Selección de Proveedor:
- **ComboBox con claves**: Muestra solo las claves únicas de proveedores
- **Información automática**: Al seleccionar una clave, se muestran automáticamente en labels:
  - Nombre del proveedor
  - RFC
  - Dirección
  - Teléfono

#### Interfaz:
```
?? PROVEEDOR ?????????????????????????????????
? Clave Única: [PROV-001 ?]                 ?
? Nombre: Materiales de Construcción Norte  ?
? RFC: MCN930415ABC                          ?
? Dirección: Blvd. Luis Donaldo Colosio 123?
? Teléfono: 662-123-4567                    ?
? [+ AGREGAR NUEVO PROVEEDOR]               ?
??????????????????????????????????????????????
```

### 3. PDF Actualizado

El PDF de órdenes de compra ahora incluye:

```
ORDEN DE COMPRA INDIRECTA
Folio: OC-IND-20240115-001    Fecha: 15/01/2024

Proveedor: Materiales de Construcción del Norte S.A. de C.V.
Clave: PROV-001                RFC: MCN930415ABC
Dirección: Blvd. Luis Donaldo Colosio 123, Col. Centro
Teléfono: 662-123-4567
```

## ?? Cómo Usar

### Agregar un Nuevo Proveedor

1. Desde **FormCompraIndirecta**, clic en **"+ AGREGAR NUEVO PROVEEDOR"**
2. Llenar el formulario:
   ```
   Clave Única*: PROV-001
   Nombre*:      Ferretería El Martillo S.A. de C.V.
   RFC*:         FEM910520XYZ
   Dirección:    Calle Revolución 456, Col. Centro
   Teléfono:     662-234-5678
   ```
3. Clic en **GUARDAR**

### Seleccionar Proveedor para Orden

1. Abrir **FormCompraIndirecta**
2. En la sección **PROVEEDOR**, desplegar el ComboBox
3. Seleccionar la clave del proveedor
4. Verificar que la información se muestre correctamente
5. Continuar con la orden de compra

### Ver Información de Proveedor

La información del proveedor se muestra automáticamente al seleccionar su clave en el ComboBox, sin necesidad de acciones adicionales.

## ?? Archivos Modificados

### Nuevos/Actualizados:
1. **FormAgregarProveedor.cs** - Lógica completa del formulario
2. **FormAgregarProveedor.Designer.cs** - Nuevos campos (RFC, Dirección, Teléfono)
3. **FormCompraIndirecta.cs** - Integración con nuevo sistema de proveedores
4. **FormCompraIndirecta.Designer.cs** - Nueva interfaz de proveedor con labels

### Scripts SQL:
5. **SQL_VerificarProveedores.sql** - Script completo de verificación y mantenimiento

## ?? Consultas SQL Útiles

### Ver todos los proveedores
```sql
SELECT ClaveUnica, Nombre, RFC, Telefono 
FROM PROVEEDORESCALANDRIA 
ORDER BY Nombre
```

### Ver órdenes con su proveedor
```sql
SELECT 
    o.FolioOC,
    o.Fecha,
    o.TipoOrden,
    p.ClaveUnica,
    p.Nombre AS Proveedor,
    p.RFC
FROM OrdenesCompra o
LEFT JOIN PROVEEDORESCALANDRIA p ON o.ProveedorClave = p.ClaveUnica
WHERE o.TipoOrden = 'INDIRECTA'
ORDER BY o.Fecha DESC
```

### Buscar proveedor por RFC
```sql
SELECT * FROM PROVEEDORESCALANDRIA 
WHERE RFC = 'MCN930415ABC'
```

## ?? Notas Importantes

### Validación de RFC
- El sistema valida que el RFC tenga entre 12 y 13 caracteres
- Se convierte automáticamente a mayúsculas
- Para personas físicas: 13 caracteres
- Para personas morales: 12 caracteres

### Claves Únicas
- Deben ser descriptivas y fáciles de identificar
- Ejemplos recomendados:
  - `FERRE-HMO` (Ferretería Hermosillo)
  - `MAT-NORTE` (Materiales del Norte)
  - `PROV-001` (Proveedor numerado)
- Se convierten automáticamente a mayúsculas
- No se permiten duplicados

### Datos Opcionales
- **Dirección** y **Teléfono** son opcionales pero se recomienda capturarlos
- Si no se proporcionan, se mostrará "N/A" en los labels y se omitirán del PDF

## ?? Migración desde Sistema Anterior

Si tienes proveedores en una tabla anterior llamada `Proveedores`, puedes migrar los datos ejecutando la sección de migración en `SQL_VerificarProveedores.sql`:

```sql
INSERT INTO PROVEEDORESCALANDRIA (ClaveUnica, Nombre, RFC, Direccion, Telefono)
SELECT 
    Codigo AS ClaveUnica,
    Nombre,
    'RFC000000XXX' AS RFC, -- Deberás actualizar manualmente
    '' AS Direccion,
    '' AS Telefono
FROM Proveedores
```

**IMPORTANTE**: Después de la migración, actualiza los RFCs manualmente con los datos correctos.

## ?? Solución de Problemas

### Error: "Ya existe un proveedor con esa Clave Única"
**Causa**: La clave ingresada ya está registrada  
**Solución**: Usar una clave diferente o editar el proveedor existente

### Error: "El RFC debe tener 12 o 13 caracteres"
**Causa**: RFC inválido  
**Solución**: Verificar el RFC correcto del proveedor

### La información del proveedor no se muestra
**Causa**: Proveedor no seleccionado o error en la base de datos  
**Solución**: Verificar que la tabla PROVEEDORESCALANDRIA existe y tiene datos

### El PDF no muestra la información del proveedor
**Causa**: ProveedorClave no guardado en OrdenesCompra  
**Solución**: Verificar que se seleccionó un proveedor antes de generar la orden

## ?? Próximas Mejoras (Sugeridas)

1. **Edición de proveedores**: Formulario para modificar datos existentes
2. **Historial**: Ver todas las órdenes por proveedor
3. **Estadísticas**: Reportes de compras por proveedor
4. **Múltiples contactos**: Agregar varios teléfonos/emails por proveedor
5. **Calificación**: Sistema de evaluación de proveedores
6. **Integración fiscal**: Validación automática de RFC con SAT

## ?? Soporte

Para problemas o sugerencias relacionadas con el sistema de proveedores, contacta al equipo de desarrollo.

---

**Última actualización**: Enero 2024  
**Versión del sistema**: 2.0 - Sistema de Proveedores Mejorado
