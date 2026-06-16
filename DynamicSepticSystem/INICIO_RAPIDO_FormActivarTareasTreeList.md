# ?? INICIO RÁPIDO - FormActivarTareasTreeList

## Instalación en 5 Minutos

### ? Paso 1: Ejecutar Script SQL (1 minuto)

```bash
# Opción A: SQL Server Management Studio
1. Abrir SSMS
2. Conectar a tu base de datos
3. Abrir archivo: SQL_SCRIPTS\CrearTablaActivacionTareasRuta.sql
4. Ejecutar (F5)
```

```bash
# Opción B: Línea de comandos
sqlcmd -S SERVER -d DATABASE -i "SQL_SCRIPTS\CrearTablaActivacionTareasRuta.sql"
```

### ? Paso 2: Copiar Archivos (1 minuto)

```
Copiar estos archivos al proyecto:
? FormActivarTareasTreeList.cs
? FormActivarTareasTreeList.Designer.cs
? DialogSeleccionarNivel.cs
```

### ? Paso 3: Compilar (1 minuto)

```bash
dotnet build
# O Ctrl+Shift+B en Visual Studio
```

### ? Paso 4: Agregar al Menú (2 minutos)

En `PanelPrincipal.cs` o donde tengas los botones:

```csharp
// Botón para abrir FormActivarTareasTreeList
var btnActivarTareas = new Button
{
    Text = "?? Activar Tareas por Casa",
    Width = 200,
    Height = 40,
    BackColor = Color.FromArgb(52, 152, 219),
    ForeColor = Color.White,
    Font = new Font("Segoe UI", 10, FontStyle.Bold)
};

btnActivarTareas.Click += (s, e) =>
{
    using (var frm = new FormActivarTareasTreeList())
        frm.ShowDialog(this);
};

this.Controls.Add(btnActivarTareas);
```

### ? Paso 5: Probar (Gratuito)

```
1. Abre la aplicación
2. Haz clic en "?? Activar Tareas por Casa"
3. Selecciona Manzana: 5
4. Selecciona Lote: 12
5. Haz clic "?? Cargar Tareas"
6. Marca/desmarca tareas
7. Haz clic "?? Guardar"
? ¡Listo!
```

---

## ?? Uso Básico

### Escenario: Activar solo Sub-Padres

```
1. Selecciona Manzana y Lote
2. Haz clic "?? Cargar Tareas"
   ? Se cargan todos los nodos
   
3. Haz clic "? Desmarcar Todo"
   ? Todos pasan a inactivos
   
4. Haz clic "+ Por Nivel"
   ? Se abre diálogo
   
5. Selecciona "Sub-Padres (Nivel 1)"
   ? Haz clic "? Aceptar"
   ? Solo Sub-Padres están activos
   
6. Haz clic "?? Guardar"
   ? Configuración guardada
```

---

## ?? Verificar en Base de Datos

```sql
-- Ver configuración guardada
SELECT * FROM ActivacionTareasRuta 
WHERE Manzana = '5' AND Lote = '12'
ORDER BY Ruta, NodoID;

-- Ver tareas activas
SELECT * FROM vw_TareasActivasPorCasa
WHERE Manzana = '5' AND Lote = '12';

-- Contar activas vs inactivas
SELECT 
    Manzana, Lote,
    COUNT(*) Total,
    SUM(CASE WHEN Activa = 1 THEN 1 ELSE 0 END) Activas,
    SUM(CASE WHEN Activa = 0 THEN 1 ELSE 0 END) Inactivas
FROM ActivacionTareasRuta
GROUP BY Manzana, Lote;
```

---

## ?? Ejemplos de Uso

### Ejemplo 1: Configuración completa
```
Casa M5-L12 (Calandria)
? Cargar Tareas
? Marcar Todo
? Guardar
Resultado: Todas las tareas activas
```

### Ejemplo 2: Solo materiales
```
Casa M3-L8 (Tunera)
? Cargar Tareas
? Desmarcar Todo
? Click en cada hijo "Material"
? Guardar
Resultado: Solo tareas de material activas
```

### Ejemplo 3: Por niveles
```
Casa M7-L15 (Calandria)
? Cargar Tareas
? Desmarcar Todo
? + Por Nivel ? Sub-Padres
? Guardar
Resultado: Solo Sub-Padres activos
```

---

## ? Preguntas Frecuentes

### ¿Dónde se guardan los datos?
**En la tabla `ActivacionTareasRuta` de la BD**

### ¿Se pierde la configuración al cerrar?
**No, se guarda en BD. Al reabrir la misma casa, se cargan los datos.**

### ¿Puedo cambiar la configuración después?
**Sí, simplemente carga la casa nuevamente y modifica.**

### ¿Es diferente para Calandria y Tunera?
**Sí, se detecta automáticamente según el prototipo de la casa.**

### ¿Se puede exportar la configuración?
**No incluido, pero es fácil de agregar. Ver GUIA_INTEGRACION.**

---

## ?? Problemas Comunes

### Error: "La tabla ActivacionTareasRuta no existe"
```
Solución:
1. Abre SQL Server Management Studio
2. Ejecuta: SQL_SCRIPTS\CrearTablaActivacionTareasRuta.sql
3. Recarga la aplicación
```

### Error: "No se encuentran tareas"
```
Solución:
1. Verifica que la casa (Manzana/Lote) exista
2. Selecciona primero Manzana, luego Lote
3. Asegúrate de que tenga Prototipo asignado
```

### Botón "Cargar Tareas" deshabilitado
```
Solución:
1. Selecciona una Manzana del combo
2. Selecciona un Lote del combo
3. Ahora el botón debe estar habilitado
```

### No guarda los cambios
```
Solución:
1. Abre el formulario
2. Modifica tareas
3. Haz clic específicamente en "?? Guardar"
4. Debe aparecer: "? Configuración guardada para M5-L12"
```

---

## ?? Interfaz Visual

```
??????????????????????????????????????????????????????????
?  Activar/Desactivar Tareas - Sistema Calandria         ?
??????????????????????????????????????????????????????????
?                                                         ?
? Manzana: [5      ?] Lote: [12    ?] [?? Cargar]      ?
?                                                         ?
???????????????????????????????????????????????????????????
?                                                         ?
? ? | # | Nombre          | Tipo      | Tarea   | Desc. ?
? ? | 1 | Excavación      | Sub-Padre | Mater. | ... ?
? ? |   | Escavación 1.0  | Hijo      | Mater. | ... ?
? ? | 2 | Tuberías        | Sub-Padre | M.Obra | ... ?
? ? |   | Colocación 50mm | Hijo      | Mater. | ... ?
? ? |   | Colocación 75mm | Hijo      | Mater. | ... ?
?                                                         ?
???????????????????????????????????????????????????????????
?                                                         ?
? [? Marcar] [? Desmarcar] [+ Nivel] [- Nivel]        ?
?                                                         ?
? Activas: 5 | Inactivas: 0 | Padres: 0 | Sub-Padres: 2?
? [???????????] 80% activas                              ?
?                                                         ?
? [?? Guardar]                              [? Cerrar] ?
?                                                         ?
??????????????????????????????????????????????????????????
```

---

## ?? Tips Útiles

### Tip 1: Marcar Todo y luego Desmarcar Por Nivel
```
? Marcar Todo
? - Por Nivel ? Hijos
? Resultado: Solo Padres y Sub-Padres
```

### Tip 2: Usar el contador
```
El contador (#) solo aparece en Sub-Padres
Te ayuda a saber el orden global
Ej: #1, #2, #3, etc.
```

### Tip 3: Colores para identificar
```
Azul Oscuro = Padre (nivel 0)
Azul Claro = Sub-Padre (nivel 1)
Rojo = Hijo Material (nivel 2)
Verde = Hijo Mano de Obra (nivel 2)
```

### Tip 4: Barra de progreso
```
La barra muestra visualmente:
- Cuántas tareas están activas
- El porcentaje en tiempo real
- Se actualiza al hacer click
```

---

## ? Checklist Final

- [ ] Script SQL ejecutado
- [ ] Archivos copiados al proyecto
- [ ] Proyecto compilado sin errores
- [ ] Botón agregado en menú
- [ ] Formulario abre correctamente
- [ ] Se carga casa y tareas
- [ ] Botones Marcar/Desmarcar funcionan
- [ ] Se guardan cambios en BD
- [ ] Al reabrir, se cargan los cambios
- [ ] Documentación leída

---

## ?? Próximos Pasos

### Después de instalar:
1. **Explorar**: Usa el formulario con diferentes casas
2. **Documentar**: Guarda la configuración por prototipo
3. **Integrar**: Agrégalo a tu flujo de trabajo diario
4. **Extender**: Si necesitas más funciones, ver GUIA_INTEGRACION

### Posibles mejoras futuras:
- Exportar a Excel
- Importar desde Excel
- Duplicar configuración de una casa a otra
- Reportes de activaciones por fecha

---

## ?? Ayuda Rápida

| Necesito... | Archivo |
|---|---|
| Cómo usar | `DOCUMENTACION_FormActivarTareasTreeList.md` |
| Cómo integrar | `GUIA_INTEGRACION_FormActivarTareasTreeList.md` |
| Información de BD | `CrearTablaActivacionTareasRuta.sql` |
| Código | `FormActivarTareasTreeList.cs` |
| Resumen | `RESUMEN_FormActivarTareasTreeList.md` |

---

## ?? ¡Listo para usar!

```
? Descargado
? Instalado
? Compilado
? Probado
? Documentado

? ¡A disfrutar! ??
```

---

**Tiempo total de instalación**: ~5 minutos
**Complejidad**: Baja
**Soporte**: Completo

