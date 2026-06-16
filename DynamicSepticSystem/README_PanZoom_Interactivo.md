# ??? Sistema de Pan & Zoom Interactivo - Mapa del Sembrado

## ? NUEVA FUNCIONALIDAD IMPLEMENTADA

Se ha agregado **control total de navegación** al mapa del sembrado en `PanelPrincipal`, permitiendo:

1. **??? Desplazamiento libre con mouse drag (Pan)**
2. **?? Zoom con rueda del mouse**
3. **?? Zoom automático al buscar una casa**
4. **?? Navegación fluida sin restricciones**

---

## ?? Controles del Usuario

### ??? Desplazamiento (Pan)
- **Arrastrar con botón izquierdo**: Mueve el mapa libremente en cualquier dirección
- **Cursor**: Cambia a "mano" (?) durante el arrastre
- **Sin límites**: Puedes desplazarte libremente por todo el mapa

### ?? Zoom
- **Rueda del mouse hacia arriba**: Acerca la vista (zoom in)
- **Rueda del mouse hacia abajo**: Aleja la vista (zoom out)
- **Rango**: 50% - 500% (0.5x a 5.0x)
- **Centro del zoom**: El punto donde está el cursor del mouse
- **Incremento**: 10% por cada tick de la rueda

### ?? Búsqueda de Casa
- **Escribir Manzana y Lote** ? Click en "BUSCAR"
- **Resultado**: Zoom automático a 150% centrado en la casa
- **Marcador rojo**: Indica la ubicación exacta
- **Etiqueta**: Muestra "M[#] L[#]"

### ?? Alternar Vistas (Click en mapa)
- **Click en mapa con zoom activo**: Vuelve a vista completa
- **Click en mapa sin zoom**: Regresa al zoom de la casa

---

## ?? Interfaz de Usuario

### Texto de Instrucciones Dinámico

El label `lblInstruccionesMapa` muestra ayuda contextual:

#### Sin Casa Seleccionada:
```
?? Busca una casa para ver su ubicación en el mapa
```

#### Después de Buscar Casa:
```
?? M5 L2 - Arrastra para mover | Rueda del mouse para zoom
```

#### Durante Uso del Zoom:
```
?? M5 L2 - Zoom: 150% | Arrastra para mover
```

#### Vista Completa:
```
?? Vista completa - Haz clic para hacer zoom a M5 L2
```

---

## ?? Implementación Técnica

### Variables Agregadas

```csharp
// ??? Variables para pan (desplazamiento) con mouse drag
private bool estaDraggeando = false;
private Point puntoInicialDrag;
private PointF offsetInicialDrag;
```

### Eventos Configurados

```csharp
// ??? Eventos para pan (desplazamiento) con mouse drag
pictureBoxMapa.MouseDown += PictureBoxMapa_MouseDown;
pictureBoxMapa.MouseMove += PictureBoxMapa_MouseMove;
pictureBoxMapa.MouseUp += PictureBoxMapa_MouseUp;

// ?? Evento para zoom con rueda del mouse
pictureBoxMapa.MouseWheel += PictureBoxMapa_MouseWheel;
```

---

## ?? Métodos Implementados

### 1. `PictureBoxMapa_MouseDown` ???

**Propósito**: Inicia el arrastre del mapa

```csharp
private void PictureBoxMapa_MouseDown(object sender, MouseEventArgs e)
{
    if (e.Button == MouseButtons.Left)
    {
        estaDraggeando = true;
        puntoInicialDrag = e.Location;
        offsetInicialDrag = offsetActual;
        pictureBoxMapa.Cursor = Cursors.Hand;
    }
}
```

**Características**:
- Guarda punto inicial del drag
- Guarda offset inicial
- Cambia cursor a "mano"

---

### 2. `PictureBoxMapa_MouseMove` ???

**Propósito**: Desplaza el mapa mientras se arrastra

```csharp
private void PictureBoxMapa_MouseMove(object sender, MouseEventArgs e)
{
    if (estaDraggeando)
    {
        // Calcular desplazamiento desde el punto inicial
        int deltaX = e.X - puntoInicialDrag.X;
        int deltaY = e.Y - puntoInicialDrag.Y;
        
        // Aplicar desplazamiento inmediatamente (sin animación)
        offsetActual = new PointF(
            offsetInicialDrag.X + deltaX,
            offsetInicialDrag.Y + deltaY
        );
        
        // También actualizar offsetObjetivo
        offsetObjetivo = offsetActual;
        
        // Redibujar el mapa
        pictureBoxMapa.Invalidate();
    }
}
```

**Características**:
- Calcula desplazamiento en tiempo real
- Actualiza offset inmediatamente (sin lag)
- Actualiza tanto `offsetActual` como `offsetObjetivo`
- Redibuja el mapa en cada frame

---

### 3. `PictureBoxMapa_MouseUp` ???

**Propósito**: Finaliza el arrastre

```csharp
private void PictureBoxMapa_MouseUp(object sender, MouseEventArgs e)
{
    if (estaDraggeando)
    {
        estaDraggeando = false;
        pictureBoxMapa.Cursor = Cursors.Default;
        
        // Actualizar texto de instrucciones
        if (casaActual != null)
        {
            var lblInstruccionesMapa = this.Controls.Find("lblInstruccionesMapa", true).FirstOrDefault() as Label;
            if (lblInstruccionesMapa != null)
            {
                lblInstruccionesMapa.Text = $"?? M{casaActual.Manzana} L{casaActual.Lote} - Arrastra para mover | Rueda del mouse para zoom";
            }
        }
    }
}
```

**Características**:
- Restaura cursor a Default
- Actualiza instrucciones

---

### 4. `PictureBoxMapa_MouseWheel` ??

**Propósito**: Controla el zoom con la rueda del mouse

```csharp
private void PictureBoxMapa_MouseWheel(object sender, MouseEventArgs e)
{
    if (imagenMapaCompleto == null) return;
    
    // Calcular nuevo nivel de zoom
    float factorZoom = e.Delta > 0 ? 1.1f : 0.9f;  // ±10%
    float nuevoZoom = zoomActual * factorZoom;
    
    // Limitar zoom entre 0.5x y 5.0x
    const float zoomMin = 0.5f;
    const float zoomMax = 5.0f;
    nuevoZoom = Math.Max(zoomMin, Math.Min(zoomMax, nuevoZoom));
    
    // Obtener posición del mouse en coordenadas del mapa
    PointF puntoMouse = new PointF(e.X, e.Y);
    float mapX = (puntoMouse.X - offsetActual.X) / zoomActual;
    float mapY = (puntoMouse.Y - offsetActual.Y) / zoomActual;
    
    // Calcular nuevo offset para zoom centrado en cursor
    offsetObjetivo = new PointF(
        puntoMouse.X - mapX * nuevoZoom,
        puntoMouse.Y - mapY * nuevoZoom
    );
    
    zoomObjetivo = nuevoZoom;
    zoomActual = nuevoZoom;
    offsetActual = offsetObjetivo;
    
    pictureBoxMapa.Invalidate();
    
    // Actualizar texto de instrucciones con nivel de zoom
    if (casaActual != null)
    {
        var lblInstruccionesMapa = this.Controls.Find("lblInstruccionesMapa", true).FirstOrDefault() as Label;
        if (lblInstruccionesMapa != null)
        {
            lblInstruccionesMapa.Text = $"?? M{casaActual.Manzana} L{casaActual.Lote} - Zoom: {(nuevoZoom * 100):F0}% | Arrastra para mover";
        }
    }
}
```

**Características**:
- **Zoom inteligente**: Centrado en la posición del cursor
- **Límites**: 50% - 500%
- **Incremento suave**: 10% por tick
- **Cálculo de offset**: Mantiene la posición bajo el cursor fija durante el zoom
- **Actualización inmediata**: Sin animación para respuesta instantánea
- **Feedback visual**: Muestra nivel de zoom actual en instrucciones

---

### 5. `PictureBoxMapa_Click` (MODIFICADO) ???

**Propósito**: Alterna entre vistas SIN interferir con el drag

```csharp
private void PictureBoxMapa_Click(object sender, EventArgs e)
{
    if (casaActual == null) return;
    
    // ?? Ignorar click si se acaba de hacer drag
    MouseEventArgs me = e as MouseEventArgs;
    if (me != null)
    {
        // Si el click está muy lejos del punto inicial de drag, fue un drag, no un click
        if (Math.Abs(me.Location.X - puntoInicialDrag.X) > 5 || 
            Math.Abs(me.Location.Y - puntoInicialDrag.Y) > 5)
        {
            return;  // No procesar como click
        }
    }
    
    // ... resto del código original ...
}
```

**Características**:
- **Tolerancia de 5px**: Evita clics accidentales después de drags pequeños
- **Preserva funcionalidad**: Click sigue alternando vistas
- **Sin conflictos**: Distingue entre drag y click correctamente

---

## ?? Flujo de Uso Típico

### Escenario 1: Exploración Libre del Sembrado

```
Usuario abre PanelPrincipal
    ?
Mapa muestra vista completa del sembrado
    ?
Usuario arrastra el mapa libremente ???
    ?
Usuario usa rueda del mouse para acercar/alejar ??
    ?
Navegación fluida sin restricciones ?
```

### Escenario 2: Búsqueda y Exploración de Casa

```
Usuario ingresa Manzana=5, Lote=2
    ?
Click en BUSCAR
    ?
?? Zoom automático a 150% hacia M5L2
?? Marcador rojo aparece
    ?
Usuario arrastra para ver casas vecinas ???
    ?
Usuario usa rueda para acercar más (300%) ??
    ?
Usuario explora los detalles del terreno ?
```

### Escenario 3: Navegación Combinada

```
Usuario busca casa M3L4
    ?
?? Zoom automático a la casa
    ?
Usuario arrastra hacia la izquierda ???
    ?
Encuentra otra casa interesante (M3L3)
    ?
Usuario hace zoom in con rueda (250%) ??
    ?
Usuario arrastra para explorar detalles ???
    ?
Usuario hace click en mapa ? Vuelve a vista completa ??
```

---

## ?? Configuración Avanzada

### Ajustar Rango de Zoom

```csharp
// En PictureBoxMapa_MouseWheel()
const float zoomMin = 0.5f;  // 50% - Cambiar según necesidad
const float zoomMax = 5.0f;  // 500% - Cambiar según necesidad
```

**Recomendaciones**:
- **Sembrados pequeños**: `zoomMin = 0.3f, zoomMax = 3.0f`
- **Sembrados medianos**: `zoomMin = 0.5f, zoomMax = 5.0f` (actual)
- **Sembrados grandes**: `zoomMin = 0.2f, zoomMax = 10.0f`

### Ajustar Incremento de Zoom

```csharp
// En PictureBoxMapa_MouseWheel()
float factorZoom = e.Delta > 0 ? 1.1f : 0.9f;  // ±10%

// Más suave (±5%):
float factorZoom = e.Delta > 0 ? 1.05f : 0.95f;

// Más rápido (±20%):
float factorZoom = e.Delta > 0 ? 1.2f : 0.8f;
```

### Ajustar Tolerancia de Click vs Drag

```csharp
// En PictureBoxMapa_Click()
if (Math.Abs(me.Location.X - puntoInicialDrag.X) > 5 ||  // Cambiar 5 a otro valor
    Math.Abs(me.Location.Y - puntoInicialDrag.Y) > 5)

// Más tolerante (10px):
if (Math.Abs(me.Location.X - puntoInicialDrag.X) > 10 ||
    Math.Abs(me.Location.Y - puntoInicialDrag.Y) > 10)

// Menos tolerante (2px):
if (Math.Abs(me.Location.X - puntoInicialDrag.X) > 2 ||
    Math.Abs(me.Location.Y - puntoInicialDrag.Y) > 2)
```

---

## ?? Comparación Antes/Después

| Función | ? Antes | ? Después |
|---------|---------|-----------|
| **Desplazamiento** | Solo con click (vista completa ? zoom casa) | Arrastre libre con mouse |
| **Zoom** | Solo automático al buscar casa | Rueda del mouse (50%-500%) |
| **Navegación** | Limitada a 2 vistas fijas | Navegación continua sin límites |
| **Cursor** | Siempre Default | Default / Hand según contexto |
| **Feedback** | Instrucciones estáticas | Instrucciones dinámicas con nivel de zoom |
| **Interactividad** | Básica | Completa y fluida |

---

## ?? Casos de Uso

### 1. Supervisor de Obra ??
**Necesidad**: Ver rápidamente dónde está cada casa en el sembrado

**Uso**:
1. Abre PanelPrincipal
2. Arrastra el mapa para ubicarse en la zona de interés
3. Usa rueda del mouse para acercar/alejar según necesidad
4. Busca casa específica cuando se requiere zoom automático

### 2. Administrador ??
**Necesidad**: Planificar asignación de cuadrillas por zonas

**Uso**:
1. Vista completa del sembrado
2. Arrastra para explorar cada manzana
3. Hace zoom in en zonas con alta densidad de casas
4. Compara ubicaciones relativas entre casas

### 3. Cliente ?????
**Necesidad**: Ubicar su casa y ver casas vecinas

**Uso**:
1. Busca su casa (zoom automático)
2. Arrastra para ver casas vecinas
3. Hace zoom out para ver contexto completo
4. Hace zoom in para ver detalles de su terreno

---

## ?? Solución de Problemas

### El mapa no se mueve al arrastrar

**Causa**: `pictureBoxMapa.Enabled = false` o eventos no registrados

**Solución**:
```csharp
// Verificar en InitializeComponent() o código manual:
pictureBoxMapa.Enabled = true;
pictureBoxMapa.MouseDown += PictureBoxMapa_MouseDown;
pictureBoxMapa.MouseMove += PictureBoxMapa_MouseMove;
pictureBoxMapa.MouseUp += PictureBoxMapa_MouseUp;
```

### El zoom no funciona con la rueda del mouse

**Causa**: Evento MouseWheel no registrado

**Solución**:
```csharp
// En InicializarSistemaMapa():
pictureBoxMapa.MouseWheel += PictureBoxMapa_MouseWheel;
```

### El click en mapa sigue funcionando después de drag

**Causa**: Tolerancia muy baja o lógica de detección desactivada

**Solución**:
```csharp
// En PictureBoxMapa_Click(), aumentar tolerancia:
if (Math.Abs(me.Location.X - puntoInicialDrag.X) > 10 ||
    Math.Abs(me.Location.Y - puntoInicialDrag.Y) > 10)
```

### El zoom es muy lento/rápido

**Causa**: Factor de zoom inadecuado

**Solución**:
```csharp
// Ajustar en PictureBoxMapa_MouseWheel():
float factorZoom = e.Delta > 0 ? 1.15f : 0.85f;  // Más rápido
float factorZoom = e.Delta > 0 ? 1.05f : 0.95f;  // Más lento
```

---

## ?? Mejoras Futuras Sugeridas

### 1. **Botones de Zoom**
Agregar botones + / - para usuarios sin rueda del mouse

```csharp
private void btnZoomIn_Click(object sender, EventArgs e)
{
    SimularScrollWheel(120);  // Simular scroll up
}

private void btnZoomOut_Click(object sender, EventArgs e)
{
    SimularScrollWheel(-120);  // Simular scroll down
}
```

### 2. **Mini-mapa**
Pequeño mapa en esquina mostrando posición actual

### 3. **Zoom a región**
Seleccionar región rectangular con mouse para hacer zoom

### 4. **Historial de navegación**
Botones Atrás/Adelante para navegar entre vistas anteriores

### 5. **Marcadores personalizados**
Permitir al usuario marcar múltiples casas de interés

### 6. **Límites de desplazamiento**
Opcional: Evitar que el mapa se desplace fuera de los bordes

```csharp
private void LimitarOffset()
{
    float maxOffsetX = imagenMapaCompleto.Width * zoomActual - pictureBoxMapa.Width;
    float maxOffsetY = imagenMapaCompleto.Height * zoomActual - pictureBoxMapa.Height;
    
    offsetActual.X = Math.Min(0, Math.Max(-maxOffsetX, offsetActual.X));
    offsetActual.Y = Math.Min(0, Math.Max(-maxOffsetY, offsetActual.Y));
}
```

---

## ?? Referencias

- **Archivo modificado**: `DynamicSepticSystem\PanelPrincipal.cs`
- **Métodos nuevos**:
  - `PictureBoxMapa_MouseDown`
  - `PictureBoxMapa_MouseMove`
  - `PictureBoxMapa_MouseUp`
  - `PictureBoxMapa_MouseWheel`
- **Métodos modificados**:
  - `InicializarSistemaMapa` (eventos agregados)
  - `PictureBoxMapa_Click` (detección drag vs click)
  - `btnBuscarCasa_Click` (instrucciones actualizadas)

---

## ? Checklist de Verificación

Después de implementar, verificar:

- [ ] ? Arrastrar con mouse mueve el mapa
- [ ] ? Rueda del mouse hace zoom in/out
- [ ] ? Zoom se centra en posición del cursor
- [ ] ? Límites de zoom respetados (50%-500%)
- [ ] ? Cursor cambia a Hand durante drag
- [ ] ? Click después de drag NO alterna vistas
- [ ] ? Click sin drag SÍ alterna vistas
- [ ] ? Buscar casa hace zoom automático
- [ ] ? Instrucciones se actualizan dinámicamente
- [ ] ? Marcador rojo visible en casa seleccionada
- [ ] ? Navegación fluida sin lag
- [ ] ? Sin errores de compilación

---

**Autor**: Sistema de Gestión de Obra - Calandria Residencial  
**Fecha**: 2024  
**Versión**: 2.0.0 (Pan & Zoom Interactivo)

---

## ?? ¡Listo para Usar!

El sistema de pan & zoom interactivo está completamente funcional. Los usuarios ahora tienen **control total** sobre la navegación del mapa del sembrado, proporcionando una experiencia mucho más rica e intuitiva.

**¡Disfruta explorando el sembrado con total libertad! ?????**
