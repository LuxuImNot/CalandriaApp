# ??? Guía: Cómo Agregar Tu Mapa Real del Sembrado

## ?? Pasos para Integrar Tu Plano

### Paso 1: Preparar la Imagen del Mapa

1. **Obtener el plano del sembrado**:
   - Puede ser un archivo CAD exportado a imagen
   - Fotografía aérea del terreno
   - Plano arquitectónico digitalizado
   - Formato recomendado: **PNG** (transparencia) o **JPG**

2. **Ajustar la resolución**:
   ```
   Resolución mínima: 1200 x 800 píxeles
   Resolución recomendada: 1920 x 1280 píxeles
   Resolución máxima: 3840 x 2560 píxeles (4K)
   ```

3. **Nombrar el archivo**:
   ```
   MapaSembrado.png
   ```

---

### Paso 2: Agregar la Imagen al Proyecto

#### Opción A: Como Recurso Embebido (Recomendado)

1. **Crear carpeta Resources** (si no existe):
   ```
   DynamicSepticSystem/Resources/
   ```

2. **Copiar la imagen**:
   ```
   Copiar "MapaSembrado.png" a "DynamicSepticSystem/Resources/"
   ```

3. **Agregar al proyecto**:
   - Abrir Visual Studio
   - En Solution Explorer, clic derecho en carpeta `Resources`
   - Add > Existing Item...
   - Seleccionar `MapaSembrado.png`
   - **Build Action**: `Embedded Resource` o `Content` + `Copy if newer`

4. **Verificar en Properties**:
   ```
   Build Action: Embedded Resource
   Copy to Output Directory: Copy if newer
   ```

#### Opción B: Como Archivo Externo

1. **Copiar directamente a la carpeta de salida**:
   ```
   DynamicSepticSystem/bin/Debug/Resources/MapaSembrado.png
   DynamicSepticSystem/bin/Release/Resources/MapaSembrado.png
   ```

2. El código ya está configurado para buscar en esta ruta.

---

### Paso 3: Mapear las Coordenadas de las Casas

Este es el paso **MÁS IMPORTANTE** para que el zoom funcione correctamente.

#### 3.1 Identificar Coordenadas de Referencia

1. **Abrir la imagen** del mapa en un editor (Paint, Photoshop, GIMP, etc.)

2. **Anotar coordenadas de algunas casas**:
   - Mover el cursor sobre cada casa
   - Anotar las coordenadas X,Y del píxel
   - Ejemplo:
     ```
     M1L1: X=120, Y=200
     M1L2: X=170, Y=200
     M1L3: X=220, Y=200
     M2L1: X=120, Y=350
     ...
     ```

3. **Crear tabla de referencia**:
   ```
   | Manzana | Lote | X (px) | Y (px) |
   |---------|------|--------|--------|
   | 1       | 1    | 120    | 200    |
   | 1       | 2    | 170    | 200    |
   | 1       | 3    | 220    | 200    |
   | 2       | 1    | 120    | 350    |
   | 2       | 2    | 170    | 350    |
   ```

#### 3.2 Actualizar el Código

Editar el método `ObtenerCoordenadasCasaEnMapa` en `PanelPrincipal.cs`:

```csharp
private PointF ObtenerCoordenadasCasaEnMapa(string manzana, string lote)
{
    if (!int.TryParse(manzana, out int mz) || !int.TryParse(lote, out int lt))
    {
        return new PointF(600, 400); // Centro como fallback
    }
    
    // ==============================================================
    // PERSONALIZAR SEGÚN TU MAPA REAL
    // ==============================================================
    
    // Opción 1: Coordenadas exactas por casa
    // (Si tienes pocas casas, puedes mapear cada una individualmente)
    var coordenadasExactas = new Dictionary<string, PointF>
    {
        { "1_1", new PointF(120, 200) },   // M1L1
        { "1_2", new PointF(170, 200) },   // M1L2
        { "1_3", new PointF(220, 200) },   // M1L3
        { "2_1", new PointF(120, 350) },   // M2L1
        { "2_2", new PointF(170, 350) },   // M2L2
        // ... agregar todas las casas
    };
    
    string key = $"{mz}_{lt}";
    if (coordenadasExactas.ContainsKey(key))
    {
        return coordenadasExactas[key];
    }
    
    // Opción 2: Cálculo algorítmico (si hay un patrón)
    // Ejemplo: Manzanas en grid, lotes dentro de cada manzana
    
    // Coordenadas base de cada manzana
    var basesManzanas = new Dictionary<int, PointF>
    {
        { 1, new PointF(100, 150) },
        { 2, new PointF(100, 350) },
        { 3, new PointF(100, 550) },
        { 4, new PointF(450, 150) },
        { 5, new PointF(450, 350) },
        // ... etc
    };
    
    if (!basesManzanas.ContainsKey(mz))
    {
        return new PointF(600, 400); // Fallback
    }
    
    PointF baseManzana = basesManzanas[mz];
    
    // Distribución de lotes dentro de la manzana
    // Ejemplo: Grid de 4 columnas x 2 filas
    int columnas = 4;
    int anchoLote = 50;  // Ajustar según tu mapa
    int altoLote = 75;   // Ajustar según tu mapa
    
    int col = (lt - 1) % columnas;
    int fila = (lt - 1) / columnas;
    
    float casaX = baseManzana.X + col * anchoLote + anchoLote / 2.0f;
    float casaY = baseManzana.Y + fila * altoLote + altoLote / 2.0f;
    
    return new PointF(casaX, casaY);
}
```

---

### Paso 4: Ajustar Parámetros Visuales

#### 4.1 Nivel de Zoom para Casa Específica

```csharp
// En HacerZoomACasa()
zoomObjetivo = 3.0f; // Probar valores entre 2.0 y 5.0

// Valores sugeridos:
// 2.0 = Vista amplia (muestra manzana completa)
// 3.0 = Vista media (muestra casa + vecinos)
// 4.0 = Vista cercana (solo la casa)
// 5.0 = Vista muy cercana (detalles de la casa)
```

#### 4.2 Tamaño del Marcador

```csharp
// En PictureBoxMapa_Paint()
float pinSize = 20; // Probar valores entre 15 y 30

// Valores sugeridos según zoom:
// Zoom 2.0: pinSize = 15
// Zoom 3.0: pinSize = 20
// Zoom 4.0: pinSize = 25
// Zoom 5.0: pinSize = 30
```

#### 4.3 Velocidad de Animación

```csharp
// En TimerZoom_Tick()
const float velocidad = 0.15f;

// Valores sugeridos:
// 0.08 = Muy suave (2-3 segundos)
// 0.15 = Suave (1-2 segundos) ? Recomendado
// 0.25 = Rápido (0.5-1 segundo)
// 0.40 = Muy rápido (0.3-0.5 segundos)
```

---

### Paso 5: Probar y Ajustar

1. **Compilar el proyecto**:
   ```
   Build > Rebuild Solution
   ```

2. **Ejecutar la aplicación**:
   ```
   Debug > Start Debugging (F5)
   ```

3. **Verificar mapa**:
   - Al abrir, debe mostrar el mapa completo
   - Si no aparece, revisar que `MapaSembrado.png` esté en `Resources/`

4. **Probar zoom**:
   - Buscar una casa conocida (ej: M1L1)
   - Verificar que el zoom vaya a la ubicación correcta
   - Si no coincide, ajustar coordenadas en `ObtenerCoordenadasCasaEnMapa`

5. **Probar click**:
   - Click en el mapa ? Debe volver a vista completa
   - Click nuevamente ? Debe hacer zoom a la casa

---

## ?? Ejemplos de Configuración

### Ejemplo 1: Manzanas en Línea Horizontal

```csharp
// Manzanas: M1, M2, M3 alineadas horizontalmente
var basesManzanas = new Dictionary<int, PointF>
{
    { 1, new PointF(200, 300) },
    { 2, new PointF(500, 300) },
    { 3, new PointF(800, 300) }
};

// Lotes: 4 lotes en fila horizontal dentro de cada manzana
int anchoLote = 60;
float casaX = baseManzana.X + (lt - 1) * anchoLote;
float casaY = baseManzana.Y;
```

### Ejemplo 2: Manzanas en Grid 3x3

```csharp
// 9 manzanas en grid de 3x3
int manzanaCol = (mz - 1) % 3;
int manzanaFila = (mz - 1) / 3;

float baseX = 150 + manzanaCol * 350;
float baseY = 150 + manzanaFila * 250;

PointF baseManzana = new PointF(baseX, baseY);

// Lotes: Grid de 3x3 dentro de cada manzana
int loteCol = (lt - 1) % 3;
int loteFila = (lt - 1) / 3;

float casaX = baseManzana.X + loteCol * 100 + 50;
float casaY = baseManzana.Y + loteFila * 70 + 35;
```

### Ejemplo 3: Coordenadas Exactas Importadas

```csharp
// Si tienes un archivo CSV con coordenadas exactas
// casas_coordenadas.csv:
// Manzana,Lote,X,Y
// 1,1,120.5,200.3
// 1,2,175.2,198.7
// ...

private Dictionary<string, PointF> CargarCoordenadasDesdeCSV()
{
    var coords = new Dictionary<string, PointF>();
    string csvPath = Path.Combine(Application.StartupPath, "casas_coordenadas.csv");
    
    if (!File.Exists(csvPath)) return coords;
    
    foreach (string line in File.ReadAllLines(csvPath).Skip(1))
    {
        var parts = line.Split(',');
        string key = $"{parts[0]}_{parts[1]}";
        float x = float.Parse(parts[2]);
        float y = float.Parse(parts[3]);
        coords[key] = new PointF(x, y);
    }
    
    return coords;
}
```

---

## ?? Debugging y Troubleshooting

### Ver Coordenadas en Tiempo Real

Agregar temporalmente al evento `MouseMove` del pictureBox:

```csharp
pictureBoxMapa.MouseMove += (s, e) => {
    // Convertir coordenadas de control a coordenadas de imagen
    float imgX = (e.X - offsetActual.X) / zoomActual;
    float imgY = (e.Y - offsetActual.Y) / zoomActual;
    
    // Mostrar en label temporal
    lblDebug.Text = $"Mouse: ({e.X}, {e.Y}) | Imagen: ({imgX:F1}, {imgY:F1}) | Zoom: {zoomActual:F2}x";
};
```

### Dibujar Grid de Coordenadas

Agregar temporalmente en `PictureBoxMapa_Paint`:

```csharp
// Dibujar grid cada 100 píxeles
using (Pen gridPen = new Pen(Color.FromArgb(100, 255, 0, 0), 1))
{
    for (int x = 0; x < imagenMapaCompleto.Width; x += 100)
    {
        g.DrawLine(gridPen, x, 0, x, imagenMapaCompleto.Height);
        g.DrawString(x.ToString(), SystemFonts.DefaultFont, Brushes.Red, x, 10);
    }
    
    for (int y = 0; y < imagenMapaCompleto.Height; y += 100)
    {
        g.DrawLine(gridPen, 0, y, imagenMapaCompleto.Width, y);
        g.DrawString(y.ToString(), SystemFonts.DefaultFont, Brushes.Red, 10, y);
    }
}
```

### Marcar Todas las Casas en el Mapa

Para verificar que las coordenadas están bien:

```csharp
// En PictureBoxMapa_Paint, agregar temporalmente:
for (int mz = 1; mz <= 5; mz++)
{
    for (int lt = 1; lt <= 8; lt++)
    {
        var coords = ObtenerCoordenadasCasaEnMapa(mz.ToString(), lt.ToString());
        
        using (Brush brush = new SolidBrush(Color.FromArgb(150, 0, 255, 0)))
        {
            g.FillEllipse(brush, coords.X - 5, coords.Y - 5, 10, 10);
        }
        
        g.DrawString($"M{mz}L{lt}", SystemFonts.DefaultFont, Brushes.Green, coords.X, coords.Y);
    }
}
```

---

## ? Checklist Final

Antes de considerar completada la integración:

- [ ] Imagen `MapaSembrado.png` copiada a `Resources/`
- [ ] Imagen agregada al proyecto con Build Action correcta
- [ ] Coordenadas de al menos 3 casas verificadas manualmente
- [ ] Método `ObtenerCoordenadasCasaEnMapa` actualizado
- [ ] Nivel de zoom ajustado (`zoomObjetivo`)
- [ ] Tamaño del marcador ajustado (`pinSize`)
- [ ] Velocidad de animación ajustada (`velocidad`)
- [ ] Probado zoom a varias casas diferentes
- [ ] Probado click para volver a vista completa
- [ ] Verificado que funciona en Debug y Release

---

## ?? Soporte

Si tienes problemas con la integración:

1. **Revisar logs**: Buscar mensajes de error en Output Window
2. **Verificar rutas**: Confirmar que la imagen existe en la ruta esperada
3. **Probar con placeholder**: Comentar temporalmente la carga de imagen real para verificar que el sistema funciona
4. **Ajustar coordenadas**: Usar las herramientas de debugging para ver dónde realmente están las casas

---

**¡Éxito con tu integración!** ??
