# Sistema de Órdenes de Compra Indirectas

## ?? Descripción

Este módulo permite crear órdenes de compra **sin necesidad de asociarlas a una Manzana/Lote específica**, ideal para compras administrativas, mantenimiento general, y otros gastos indirectos.

## ?? Archivos Nuevos Creados

1. **FormCompraIndirecta.cs** - Formulario principal de órdenes indirectas
2. **FormCompraIndirecta.Designer.cs** - Diseño del formulario
3. **FormCompraIndirecta.resx** - Recursos del formulario
4. **FormAgregarInsumoIndirecto.cs** - Diálogo para agregar nuevos insumos
5. **FormAgregarInsumoIndirecto.Designer.cs** - Diseño del diálogo
6. **FormAgregarInsumoIndirecto.resx** - Recursos del diálogo
7. **FormSeleccionarTitulo.cs** - Diálogo para elegir el título del PDF
8. **FormSeleccionarTitulo.Designer.cs** - Diseño del diálogo de título
9. **FormSeleccionarTitulo.resx** - Recursos del diálogo de título
10. **SQL_VerificarComprasIndirectas.sql** - Script SQL de verificación

## ??? Tabla SQL

Se crea automáticamente la tabla **COMPRASINDIRECTAS** con la siguiente estructura:

```sql
CREATE TABLE dbo.COMPRASINDIRECTAS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Clave NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500) NULL,
    Unidad NVARCHAR(50) NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
);
```

## ?? Cómo Usar

### 1. Acceder al módulo

Desde el **PanelPrincipal**, hacer clic en el menú **COMPRAS** y seleccionar:
- **Orden de Compra Indirecta**

### 2. Flujo de trabajo

#### A. Agregar Insumos al Catálogo (opcional)

1. Clic en **+ AGREGAR NUEVO INSUMO**
2. Llenar los campos:
   - **Clave**: Identificador único (se agregará "-A" automáticamente)
   - **Descripción**: Descripción detallada del insumo
   - **Unidad**: Unidad de medida (ej: PIEZA, KILO, LOTE, PAQUETE)
3. Guardar

#### B. Buscar y Seleccionar Insumos

1. Usar la **barra de búsqueda** para filtrar por clave o descripción
2. **Doble clic** en un insumo del catálogo para agregarlo al carrito

#### C. Editar Cantidades y Costos

1. En el **CARRITO**, hacer **clic simple** sobre las celdas de:
   - **Cantidad**: Especificar cuántas unidades necesitas
   - **Costo**: Precio unitario del insumo
2. El **Importe** se calcula automáticamente: `Cantidad × Costo`

#### D. Seleccionar Proveedor

1. Elegir el proveedor de los combos:
   - **Nombre**
   - **Código**
2. O hacer clic en **+ AGREGAR NUEVO PROVEEDOR** si no existe

#### E. Generar Orden de Compra

1. Clic en **GENERAR ORDEN DE COMPRA**
2. Seleccionar el tipo de título para el PDF:
   - **Orden de Compra Administrativa**
   - **Orden de Compra Indirecta**
3. Confirmar

### 3. Resultado

- Se genera un **PDF** con el formato seleccionado
- Se guarda en: `Mis Documentos\CALANDRIA RESIDENCIAL\PDFOrdenesCompra\`
- Se abre automáticamente la carpeta con el PDF
- Se guarda en la base de datos con:
  - **Folio**: `OC-IND-YYYYMMDD-###`
  - **TipoOrden**: `INDIRECTA`

## ?? Características Especiales

### ? Insumos Personalizados

- Los insumos agregados manualmente tienen **"-A"** al final de su clave
- Esto permite identificar qué insumos fueron creados específicamente
- Ejemplo: `ADMIN-001-A`, `MANT-HERR-A`

### ? Sin Manzana/Lote

- No requiere vincular la orden a una casa específica
- Ideal para gastos generales, administrativos, de mantenimiento, etc.

### ? Cálculo Automático

- El importe se calcula en tiempo real
- Costo × Cantidad = Importe
- Total acumulado en el PDF

### ? PDF Personalizado

- Elige entre dos títulos:
  - "ORDEN DE COMPRA ADMINISTRATIVA"
  - "ORDEN DE COMPRA INDIRECTA"
- Formato profesional con logo de la empresa
- Incluye tabla detallada con todos los insumos

## ?? Base de Datos

### Tablas Utilizadas

1. **COMPRASINDIRECTAS**: Catálogo de insumos indirectos
2. **OrdenesCompra**: Registro de órdenes (TipoOrden = 'INDIRECTA')
3. **OrdenesCompraDetalle**: Detalle de insumos por orden
4. **Proveedores**: Catálogo de proveedores

### Consultas Útiles

```sql
-- Ver todas las órdenes indirectas
SELECT * FROM OrdenesCompra 
WHERE TipoOrden = 'INDIRECTA' 
ORDER BY Fecha DESC

-- Ver detalles de una orden específica
SELECT d.* 
FROM OrdenesCompraDetalle d
WHERE d.FolioOC = 'OC-IND-20240101-001'

-- Ver todos los insumos personalizados (con -A)
SELECT * FROM COMPRASINDIRECTAS 
WHERE Clave LIKE '%-A'
ORDER BY FechaCreacion DESC
```

## ?? Modificaciones al Código Existente

### PanelPrincipal.cs

Se modificó el método `OrdenCompraMultiple_Click` para mostrar un menú con dos opciones:
1. Orden de Compra Múltiple (por casas)
2. **Orden de Compra Indirecta** ? NUEVA

```csharp
private void OrdenCompraMultiple_Click(object sender, EventArgs e)
{
    ContextMenuStrip menuCompras = new ContextMenuStrip();
    
    ToolStripMenuItem itemMultiple = new ToolStripMenuItem("Orden de Compra Múltiple (por casas)");
    itemMultiple.Click += (s, ev) => {
        FormCompraMulti f = new FormCompraMulti();
        f.ShowDialog();
    };
    
    ToolStripMenuItem itemIndirecta = new ToolStripMenuItem("Orden de Compra Indirecta");
    itemIndirecta.Click += (s, ev) => {
        FormCompraIndirecta f = new FormCompraIndirecta();
        f.ShowDialog();
    };
    
    menuCompras.Items.Add(itemMultiple);
    menuCompras.Items.Add(itemIndirecta);
    menuCompras.Show(Cursor.Position);
}
```

## ? Preguntas Frecuentes

### ¿Puedo usar los mismos insumos que las órdenes de casas?

No, las órdenes indirectas tienen su propio catálogo (COMPRASINDIRECTAS). Esto permite mantener separados los insumos de construcción de los gastos administrativos/indirectos.

### ¿Puedo editar un insumo después de agregarlo?

Por ahora solo puedes eliminar (doble clic) y agregar de nuevo. Las cantidades y costos se editan directamente en el carrito.

### ¿Qué pasa si la clave ya existe?

El sistema detectará claves duplicadas y mostrará un error. Debes usar una clave única.

### ¿Puedo convertir una orden indirecta a normal?

No directamente. Son sistemas separados. Deberías crear una nueva orden según el tipo correcto.

## ?? Notas de Desarrollo

- Framework: .NET Framework 4.7.2
- Base de datos: SQL Server
- Librerías externas: BrightIdeasSoftware (ObjectListView), PdfSharp
- Patrón: Windows Forms con separación de concerns

## ?? Solución de Problemas

### Error: "Cadena de conexión no encontrada"

Verifica que `App.config` tenga configurado correctamente `CalandriaConn`:

```xml
<connectionStrings>
    <add name="CalandriaConn" 
         connectionString="Data Source=...;Initial Catalog=...;..." 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Error: "La tabla COMPRASINDIRECTAS no existe"

La tabla se crea automáticamente al abrir el formulario. Si falla, ejecuta manualmente el script SQL incluido.

### Los insumos no aparecen en el catálogo

Verifica que la tabla COMPRASINDIRECTAS tenga registros:
```sql
SELECT COUNT(*) FROM COMPRASINDIRECTAS
```

Si está vacía, agrega insumos desde el formulario o inserta manualmente en SQL.

## ?? Soporte

Para más información o reportar problemas, contacta al equipo de desarrollo.
