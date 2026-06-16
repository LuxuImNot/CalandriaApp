# ??? Sistema de Mapa con Pan & Zoom - PanelPrincipal

## ?? Descripción

Se implementó un sistema completo de **pan & zoom** en el `PanelPrincipal` que permite:

1. **Ver el sembrado completo** cuando no hay casa seleccionada
2. **Hacer zoom automático hacia una casa específica** al buscarla
3. **Animación suave** entre diferentes niveles de zoom
4. **Marcador visual** que indica la ubicación de la casa seleccionada
5. **Alternar entre vistas** haciendo clic en el mapa

---

## ?? Características Implementadas

### 1. **Nuevo PictureBox para el Mapa** (`pictureBoxMapa`)

- **Ubicación**: Lado izquierdo del panel de contenido
- **Tamaño**: 430x240 píxeles
- **GroupBox**: "Mapa del Sembrado"
- **Funcionalidad**: Muestra el plano completo del sembrado con zoom dinámico

### 2. **PictureBox de Evidencias** (`pictureBoxCasa`)

- **Ubicación**: Lado derecho del panel de contenido  
- **Tamaño**: 440x240 píxeles
- **GroupBox**: "Última Evidencia Fotográfica"
- **Funcionalidad**: Muestra la última foto subida en FormEvidenciasFotograficas (sin cambios)

### 3. **Sistema de Animación Suave**

```csharp
// Variables de control
private float zoomActual = 1.0f;        // Nivel de zoom actual
private float zoomObjetivo = 1.0f;      // Nivel de zoom objetivo
private PointF offsetActual;            // Desplazamiento actual
private PointF offsetObjetivo;          // Desplazamiento objetivo
private Timer timerZoom;                // Timer para interpolación suave (60 FPS)
```

- **Interpolación**: Transición suave entre estados (velocidad configurable)
- **FPS**: 60 frames por segundo para animaciones fluidas
- **Easing**: Movimiento natural sin saltos bruscos

---

## ?? Layout del Panel Principal

```
???????????????????????????????????????????????????????????????????
?  [Buscar Casa: Manzana [ ] Lote [ ] BUSCAR]                    ?
???????????????????????????????????????????????????????????????????
?  ?? Mapa Sembrado ???  ?? Última Evidencia Fotográfica ???    ?
?  ?                  ??  ?  [Foto]                          ?    ?
?  ?   [MAPA CON]     ??  ?                                  ?    ?
?  ?   [PAN&ZOOM]     ??  ?  M5 - L2                         ?    ?
?  ?                  ??  ?  Prototipo: LUNA                 ?    ?
?  ?  ?? Instrucciones??  ????????????????????????????????????    ?
?  ?????????????????????                                          ?
???????????????????????????????????????????????????????????????????
?  [Progreso de la Obra]                                          ?
?  [Estadísticas]                                                 ?
???????????????????????????????????????????????????????????????????
```

---

## ?? Métodos Principales

### **InicializarSistemaMapa()**
- Configura el timer de animación (60 FPS)
- Registra eventos Paint y Click
- Carga la imagen del mapa del sembrado
- Muestra la vista completa inicial

### **CargarImagenMapaSembrado()**
- **Prioridad 1**: Cargar desde `Resources/MapaSembrado.png`
- **Prioridad 2**: Cargar desde archivo externo
- **Fallback**: Crear mapa placeholder automático

### **MostrarVistaCompletaSembrado()**
- Calcula zoom para ajustar toda la imagen
- Centra el mapa en el control
- Inicia animación suave hacia el objetivo

### **HacerZoomACasa(manzana, lote)**
- Obtiene coordenadas de la casa en el mapa
- Aplica zoom de 3.0x para vista detallada
- Centra la casa en el control
- Inicia animación suave

### **ObtenerCoordenadasCasaEnMapa(manzana, lote)**
- Calcula la posición X,Y de una casa según su manzana y lote
- **?? IMPORTANTE**: Debes ajustar este método según tu plano real

### **PictureBoxMapa_Paint(sender, e)**
- Renderiza el mapa con transformaciones de zoom y pan
- Dibuja marcador rojo en la casa seleccionada
- Muestra etiqueta con Manzana y Lote

### **PictureBoxMapa_Click(sender, e)**
- **En zoom**: Vuelve a vista completa
- **En vista completa**: Hace zoom a la casa (si hay una seleccionada)
- Actualiza el texto de instrucciones

---

## ?? Imagen del Mapa

### Opción 1: Usar Plano Real (Recomendado)

1. **Guardar** el plano del sembrado como `MapaSembrado.png`
2. **Copiar** a la carpeta: `DynamicSepticSystem\Resources\`
3. **Agregar** al proyecto:
   - Clic derecho en Resources > Add > Existing Item
   - Seleccionar `MapaSembrado.png`
   - Build Action: Embedded Resource

### Opción 2: Mapa Placeholder Automático

Si no existe la imagen, el sistema genera automáticamente un plano de ejemplo con:
- 5 manzanas organizadas en grid
- 8 lotes por manzana (4x2)
- Etiquetas de M1L1, M1L2, etc.
- Colores modernos y profesionales

---

## ?? Ajustar Coordenadas del Mapa Real

Si usas tu propio plano, deberás modificar el método `ObtenerCoordenadasCasaEnMapa`:

```csharp
private PointF ObtenerCoordenadasCasaEnMapa(string manzana, string lote)
{
    // TODO: Ajustar según las coordenadas reales de tu plano
    
    // Ejemplo: Si tu plano tiene manzanas en posiciones específicas
    // Manzana 1: X=100, Y=200
    // Manzana 2: X=400, Y=200
    // etc.
    
    int mz = int.Parse(manzana);
    int lt = int.Parse(lote);
    
    // Coordenadas base de cada manzana
    Dictionary<int, PointF> basesManzanas = new Dictionary<int, PointF>
    {
        { 1, new PointF(100, 200) },
        { 2, new PointF(400, 200) },
        { 3, new PointF(700, 200) },
        // ... etc
    };
    
    PointF baseManzana = basesManzanas[mz];
    
    // Calcular offset del lote dentro de la manzana
    float offsetLoteX = ((lt - 1) % 4) * 50; // Ajustar según distribución
    float offsetLoteY = ((lt - 1) / 4) * 75; // Ajustar según distribución
    
    return new PointF(
        baseManzana.X + offsetLoteX,
        baseManzana.Y + offsetLoteY
    );
}
```

---

## ?? Flujo de Uso

### Escenario 1: Sin Casa Seleccionada

```
Usuario abre PanelPrincipal
    ?
Mapa muestra vista completa del sembrado
    ?
Mensaje: "?? Busca una casa para ver su ubicación en el mapa"
```

### Escenario 2: Buscar Casa

```
Usuario ingresa: Manzana = 5, Lote = 2
    ?
btnBuscarCasa_Click()
    ?
Se consulta BD y obtiene datos de la casa
    ?
pictureBoxCasa muestra última evidencia fotográfica
    ?
HacerZoomACasa("5", "2")
    ?
Animación suave de zoom 3.0x hacia M5L2
    ?
Marcador rojo y etiqueta "M5 L2" se dibuja en el mapa
    ?
Mensaje: "?? Vista de M5 L2 - Haz clic para volver a vista completa"
```

### Escenario 3: Click en Mapa

```
Usuario hace clic en pictureBoxMapa
    ?
¿Está en zoom?
    Sí ? MostrarVistaCompletaSembrado()
    No  ? HacerZoomACasa() (si hay casa seleccionada)
    ?
Animación suave de transición
```

---

## ?? Configuración de Animación

### Velocidad de Interpolación

```csharp
const float velocidad = 0.15f;
// 0.1  = Muy suave (lento)
// 0.15 = Suave (recomendado)
// 0.3  = Rápido
// 0.5  = Muy rápido
```

### Nivel de Zoom

```csharp
// Vista completa: Se calcula automáticamente para ajustar toda la imagen
float zoomCompleto = Math.Min(anchoControl / anchoImagen, altoControl / altoImagen) * 0.95f;

// Vista de casa: Fijo en 3.0x
float zoomCasa = 3.0f; // Puedes ajustar este valor
```

---

## ?? Personalización del Marcador

### Color y Tamaño del Pin

```csharp
// En PictureBoxMapa_Paint()
float pinSize = 20; // Tamaño del pin
Color pinColor = Color.FromArgb(200, 231, 76, 60); // Rojo semi-transparente
Color pinBorder = Color.White; // Borde blanco
```

### Etiqueta de la Casa

```csharp
Font font = new Font("Segoe UI", 10, FontStyle.Bold);
Color textColor = Color.FromArgb(231, 76, 60); // Rojo
Color bgColor = Color.FromArgb(230, 255, 255, 255); // Blanco semi-transparente
```

---

## ?? Controles de Usuario

| Acción | Resultado |
|--------|-----------|
| Buscar casa | Zoom automático a la casa |
| Click en mapa (con zoom) | Volver a vista completa |
| Click en mapa (sin zoom) | Zoom a casa actual |
| Cerrar sesión | Restablecer vista completa |

---

## ? Optimizaciones Implementadas

### Rendimiento

1. **DoubleBuffering**: PictureBox usa `DoubleBuffered = true` (automático)
2. **Interpolación suave**: Solo redibuja cuando hay cambios
3. **Timer eficiente**: Se detiene automáticamente al llegar al objetivo
4. **Cálculos optimizados**: Se evitan cálculos innecesarios en cada frame

### Calidad Visual

1. **InterpolationMode.HighQualityBicubic**: Zoom de alta calidad
2. **SmoothingMode.AntiAlias**: Bordes suaves en gráficos
3. **Marcadores semi-transparentes**: Integración visual elegante

---

## ?? Troubleshooting

### Problema: El mapa no se muestra

**Solución**:
- Verificar que `MapaSembrado.png` existe en `Resources/`
- Si no existe, el sistema creará un placeholder automáticamente

### Problema: Las coordenadas están mal ubicadas

**Solución**:
- Ajustar el método `ObtenerCoordenadasCasaEnMapa()` según tu plano real
- Medir las coordenadas reales de algunas casas en tu imagen
- Actualizar los cálculos de offset

### Problema: La animación es muy lenta/rápida

**Solución**:
```csharp
// En TimerZoom_Tick()
const float velocidad = 0.15f; // Ajustar este valor
```

### Problema: El zoom está muy cerca/lejos

**Solución**:
```csharp
// En HacerZoomACasa()
zoomObjetivo = 3.0f; // Ajustar este valor (2.0 - 5.0 recomendado)
```

---

## ?? Referencias de Código

### Archivos Modificados

- ? `PanelPrincipal.Designer.cs` - Nuevo control `pictureBoxMapa`
- ? `PanelPrincipal.cs` - Lógica de pan & zoom
- ? `btnBuscarCasa_Click()` - Integración con zoom
- ? `btnCerrarSesion_Click()` - Resetear vista

### Nuevos Métodos

1. `InicializarSistemaMapa()`
2. `CargarImagenMapaSembrado()`
3. `CrearMapaPlaceholder()`
4. `MostrarVistaCompletaSembrado()`
5. `HacerZoomACasa(manzana, lote)`
6. `ObtenerCoordenadasCasaEnMapa(manzana, lote)`
7. `IniciarAnimacionZoom()`
8. `TimerZoom_Tick(sender, e)`
9. `PictureBoxMapa_Paint(sender, e)`
10. `PictureBoxMapa_Click(sender, e)`

---

## ?? Ejemplo de Uso Completo

```csharp
// 1. Al iniciar la aplicación
PanelPrincipal form = new PanelPrincipal();
// ? InicializarSistemaMapa() se ejecuta automáticamente
// ? Se carga el mapa completo
// ? Vista inicial: Zoom 1.0x, centrado

// 2. Usuario busca casa M5L2
txtManzana.Text = "5";
txtLote.Text = "2";
btnBuscarCasa.PerformClick();
// ? Se consulta BD
// ? Se carga evidencia en pictureBoxCasa
// ? HacerZoomACasa("5", "2")
// ? Animación suave hacia M5L2
// ? Marcador rojo aparece en el mapa

// 3. Usuario hace clic en el mapa
pictureBoxMapa.PerformClick();
// ? Detecta que está en zoom
// ? MostrarVistaCompletaSembrado()
// ? Animación suave de regreso

// 4. Usuario hace clic nuevamente
pictureBoxMapa.PerformClick();
// ? Detecta que está en vista completa
// ? HacerZoomACasa("5", "2")
// ? Vuelve al zoom de la casa
```

---

## ? Mejoras Futuras Sugeridas

1. **Zoom con rueda del mouse**: Permitir zoom manual
2. **Pan con arrastre**: Mover el mapa arrastrando con el mouse
3. **Mapa interactivo**: Click en casas para seleccionarlas
4. **Búsqueda visual**: Resaltar todas las casas de una manzana
5. **Filtros**: Mostrar solo casas con cierto % de progreso
6. **Capas**: Alternar entre diferentes vistas (plano, satélite, etc.)
7. **Minimapa**: Pequeño mapa en esquina mostrando posición actual
8. **Historial**: Navegación entre casas visitadas recientemente

---

## ?? Notas Finales

- El sistema es **completamente independiente** del pictureBoxCasa
- La animación se ejecuta en **hilo de UI** (no requiere threading adicional)
- Compatible con **.NET Framework 4.7.2** (C# 7.3)
- **Sin dependencias externas**: Solo usa System.Drawing
- **Diseño modular**: Fácil de extender y personalizar

---

**Autor**: Sistema de Gestión de Obra - Calandria Residencial  
**Fecha**: 2024  
**Versión**: 1.0.0
