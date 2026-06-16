# ? VERIFICACIÓN: Sistema de Guardado de Órdenes de Compra

## ?? **Estado Actual del Sistema**

### ? **Archivos Implementados Correctamente**

| Archivo | Estado | Descripción |
|---------|--------|-------------|
| `FormCompraMulti.cs` | ? **OK** | Llama a `GuardarOrdenEnRepositorio` después de generar PDF |
| `FormCompraMulti_ExtensionPDFRepositorio.cs` | ? **OK** | Métodos completos para guardar en repositorio |
| `FormCompraIndirecta_ExtensionPDFRepositorio.cs` | ? **OK** | Métodos para órdenes indirectas |
| `FormRepositorioPDFsOrdenesCompra.cs` | ? **OK** | Formulario para visualizar repositorio |
| `SQL_CrearTablasRepositorioPDFsOrdenesCompra.sql` | ? **OK** | Script de creación de tablas |

---

## ?? **Verificación Paso a Paso**

### 1. ? FormCompraMulti.cs - Línea 193-206

```csharp
// Generar PDF con formato corporativo
try
{
    rutaPdf = GenerarPDFCorporativo(folioOC, lista);
    
    // ?? GUARDAR EN REPOSITORIO DE PDFs
    try
    {
        var casas = lstCasas.Items.Cast<CasaSeleccionada>().ToList();
        GuardarOrdenEnRepositorio(folioOC, rutaPdf, lista, casas);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"? Advertencia: La orden se generó correctamente pero no se pudo guardar en el repositorio de PDFs:\n\n{ex.Message}", 
            "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
```

**? CORRECTO**: El método `GuardarOrdenEnRepositorio` **SÍ se está llamando**.

---

### 2. ? FormCompraMulti_ExtensionPDFRepositorio.cs - Método Principal

```csharp
public void GuardarOrdenEnRepositorio(string folioOC, string rutaPdf, 
    System.Collections.Generic.List<InsumoOrdenCompra> insumos,
    System.Collections.Generic.List<CasaSeleccionada> casas = null)
{
    // Verificar que las tablas existan
    CrearTablasRepositorioSiNoExisten();
    
    // Calcular totales
    decimal subtotal = insumos.Sum(i => i.Importe);
    decimal iva = subtotal * 0.16m;
    decimal total = subtotal + iva;
    
    // 1. Guardar folio
    int folioId = GuardarFolioOrdenCompra(...);
    
    // 2. Guardar detalle de insumos
    GuardarDetalleOrdenCompra(folioId, insumos);
    
    // 3. Guardar casas incluidas
    GuardarCasasOrdenCompra(folioId, casas);
    
    // 4. Guardar PDF en BD
    GuardarPDFOrdenCompraEnBD(folioId, folioOC, rutaPdf, ...);
}
```

**? CORRECTO**: El método guarda:
- ? Folio en `FoliosOrdenCompra`
- ? Detalle en `FoliosOrdenCompraDetalle`
- ? Casas en `FoliosOrdenCompra_Casas`
- ? PDF en `PDFsOrdenCompra` (como VARBINARY)

---

## ??? **¿Por Qué No Se Están Guardando?**

### Posibles Causas:

#### 1. ? **Las tablas NO existen en la base de datos**

**Solución:**
```sql
-- Ejecutar el script SQL manualmente:
-- SQL_CrearTablasRepositorioPDFsOrdenesCompra.sql
```

#### 2. ? **Error silencioso en `GuardarOrdenEnRepositorio`**

**Diagnóstico:**
- El método tiene `try-catch` que captura errores
- Verifica si aparece el mensaje de éxito: "? Orden guardada en repositorio exitosamente"
- Si NO aparece, revisa el mensaje de error

#### 3. ? **connectionString incorrecta**

**Verificación:**
```csharp
// En FormCompraMulti_ExtensionPDFRepositorio.cs
// El connectionString se hereda de FormCompraMulti
string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"]?.ConnectionString;
```

---

## ?? **Pruebas de Diagnóstico**

### Test 1: Verificar si las tablas existen

```sql
-- Ejecutar en SQL Server Management Studio
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN (
    'FoliosOrdenCompra',
    'FoliosOrdenCompraDetalle',
    'FoliosOrdenCompra_Casas',
    'PDFsOrdenCompra'
)
ORDER BY TABLE_NAME;
```

**Resultado esperado:** Debe devolver 4 filas

---

### Test 2: Generar una orden y verificar logs

1. **Generar orden de compra desde FormCompraMulti**
2. **Verificar mensajes:**
   - ? "Orden de compra generada exitosamente" (del FormCompraMulti)
   - ? "Orden guardada en repositorio exitosamente" (del método GuardarOrdenEnRepositorio)

3. **Si NO aparece el segundo mensaje:**
   - Ocurrió un error en `GuardarOrdenEnRepositorio`
   - Revisa el mensaje de advertencia que debería aparecer

---

### Test 3: Verificar datos guardados

```sql
-- Ver todas las órdenes guardadas
SELECT * FROM FoliosOrdenCompra ORDER BY FechaGeneracion DESC;

-- Ver detalle de la última orden
SELECT TOP 1
    f.Folio,
    f.TipoOrden,
    f.TotalConIVA,
    COUNT(d.Id) AS TotalInsumos,
    COUNT(c.Id) AS TotalCasas
FROM FoliosOrdenCompra f
LEFT JOIN FoliosOrdenCompraDetalle d ON f.Id = d.FolioId
LEFT JOIN FoliosOrdenCompra_Casas c ON f.Id = c.FolioId
GROUP BY f.Id, f.Folio, f.TipoOrden, f.TotalConIVA
ORDER BY f.FechaGeneracion DESC;

-- Ver PDFs almacenados
SELECT 
    Folio, 
    TipoOrden, 
    NombreArchivo,
    TamanioBytes / 1024.0 / 1024.0 AS TamanioMB,
    FechaAlmacenamiento
FROM PDFsOrdenCompra
ORDER BY FechaAlmacenamiento DESC;
```

---

## ?? **Soluciones Rápidas**

### Solución 1: Crear tablas manualmente

```sql
-- Ejecutar el script completo:
-- DynamicSepticSystem\SQL_CrearTablasRepositorioPDFsOrdenesCompra.sql

-- O ejecutar este script simplificado:
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosOrdenCompra')
BEGIN
    CREATE TABLE FoliosOrdenCompra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Folio NVARCHAR(50) NOT NULL UNIQUE,
        Manzana NVARCHAR(10) NULL,
        Lote NVARCHAR(10) NULL,
        FechaGeneracion DATETIME DEFAULT GETDATE(),
        TipoOrden NVARCHAR(20),
        NombreProveedor NVARCHAR(200),
        CodigoProveedor NVARCHAR(50),
        TotalSinIVA DECIMAL(18,2),
        IVA DECIMAL(18,2),
        TotalConIVA DECIMAL(18,2),
        NumeroOrden INT NULL,
        Usuario NVARCHAR(100),
        Estado NVARCHAR(20) DEFAULT 'PENDIENTE',
        Observaciones NVARCHAR(MAX)
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosOrdenCompraDetalle')
BEGIN
    CREATE TABLE FoliosOrdenCompraDetalle (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Clave NVARCHAR(100),
        Descripcion NVARCHAR(500),
        Unidad NVARCHAR(50),
        Cantidad DECIMAL(18,3),
        PrecioUnitario DECIMAL(18,2),
        ImporteTotal DECIMAL(18,2),
        Familia NVARCHAR(100),
        FOREIGN KEY (FolioId) REFERENCES FoliosOrdenCompra(Id) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosOrdenCompra_Casas')
BEGIN
    CREATE TABLE FoliosOrdenCompra_Casas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Manzana NVARCHAR(10),
        Lote NVARCHAR(10),
        Prototipo NVARCHAR(50),
        FOREIGN KEY (FolioId) REFERENCES FoliosOrdenCompra(Id) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PDFsOrdenCompra')
BEGIN
    CREATE TABLE PDFsOrdenCompra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Folio NVARCHAR(50) NOT NULL,
        Manzana NVARCHAR(10) NULL,
        Lote NVARCHAR(10) NULL,
        TipoOrden NVARCHAR(20),
        NombreArchivo NVARCHAR(255),
        ContenidoPDF VARBINARY(MAX) NOT NULL,
        TamanioBytes BIGINT,
        FechaAlmacenamiento DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (FolioId) REFERENCES FoliosOrdenCompra(Id) ON DELETE CASCADE
    );
END

PRINT '? Tablas creadas correctamente';
```

---

### Solución 2: Verificar que el archivo SQL exista

**Ubicación esperada:**
```
C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\ProgramC#\DynamicSepticSystem\DynamicSepticSystem\bin\Debug\SQL_CrearTablasRepositorioPDFsOrdenesCompra.sql
```

**Si NO existe:**
1. Copia el archivo desde el proyecto a la carpeta bin\Debug
2. O modifica `CrearTablasRepositorioSiNoExisten()` para crear las tablas directamente sin script

---

### Solución 3: Agregar logging detallado

Modifica `GuardarOrdenEnRepositorio` para ver exactamente dónde falla:

```csharp
public void GuardarOrdenEnRepositorio(string folioOC, string rutaPdf, 
    System.Collections.Generic.List<InsumoOrdenCompra> insumos,
    System.Collections.Generic.List<CasaSeleccionada> casas = null)
{
    try
    {
        MessageBox.Show("INICIO: GuardarOrdenEnRepositorio", "Debug");
        
        // Verificar que las tablas existan
        CrearTablasRepositorioSiNoExisten();
        MessageBox.Show("Tablas verificadas", "Debug");
        
        // Calcular totales
        decimal subtotal = insumos.Sum(i => i.Importe);
        decimal iva = subtotal * 0.16m;
        decimal total = subtotal + iva;
        MessageBox.Show($"Totales calculados: {total:C2}", "Debug");
        
        // ... resto del código
        
        // 1. Guardar folio
        int folioId = GuardarFolioOrdenCompra(...);
        MessageBox.Show($"Folio guardado con ID: {folioId}", "Debug");
        
        // ... etc
    }
    catch (Exception ex)
    {
        MessageBox.Show($"ERROR COMPLETO:\n{ex.ToString()}", "Error Detallado", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

---

## ?? **Checklist de Verificación**

### Antes de generar una orden:

- [ ] Las tablas existen en SQL Server
- [ ] El connectionString es correcto
- [ ] Tienes permisos para insertar en las tablas
- [ ] El archivo SQL está en bin\Debug (si se usa)

### Al generar una orden:

- [ ] Aparece "Orden de compra generada exitosamente"
- [ ] Aparece "Orden guardada en repositorio exitosamente"
- [ ] NO aparece mensaje de error/advertencia

### Después de generar:

- [ ] Ejecutar: `SELECT * FROM FoliosOrdenCompra`
- [ ] Ejecutar: `SELECT * FROM PDFsOrdenCompra`
- [ ] Ambas consultas devuelven al menos 1 fila

---

## ?? **Próximos Pasos**

### Si las órdenes NO se guardan:

1. **Ejecutar Test 1** - Verificar tablas
2. **Ejecutar Solución 1** - Crear tablas manualmente
3. **Generar orden de prueba**
4. **Ejecutar Test 3** - Verificar datos
5. **Si persiste**: Agregar logging (Solución 3)

### Si las órdenes SÍ se guardan:

1. **Abrir repositorio** desde FormCompraMulti (agregar botón)
2. **Verificar visualización** de órdenes
3. **Probar Ver PDF** desde repositorio
4. **Probar Eliminar orden**

---

## ?? **Notas Importantes**

1. **El código YA ESTÁ IMPLEMENTADO correctamente** ?
2. **La compilación es exitosa** ?
3. **El problema puede ser:**
   - Tablas no creadas en BD
   - Error silencioso en try-catch
   - connectionString incorrecta

4. **Revisar primero:** Ejecutar Test 1 para verificar tablas

---

## ?? **Si Necesitas Ayuda**

### Información a proporcionar:

1. **Resultado de Test 1** (¿Existen las tablas?)
2. **Mensajes que aparecen** al generar orden
3. **Resultado de Test 3** (¿Hay datos en las tablas?)
4. **Mensaje de error completo** (si aparece)

---

**Fecha de verificación:** Enero 2025  
**Estado del código:** ? IMPLEMENTADO CORRECTAMENTE  
**Acción requerida:** ?? VERIFICAR BASE DE DATOS
