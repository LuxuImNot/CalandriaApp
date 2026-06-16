# ?? TEMATIZACIÓN APLICADA - Sistema Calandria

## ?? Resumen de Cambios

Se ha implementado un sistema de tematización centralizado para toda la aplicación con los siguientes colores:

### ?? Paleta de Colores

#### Colores Principales
- **Color Principal**: `#B36C2E` (RGB: 179, 108, 46) - Marrón/Naranja
- **Color Secundario**: `#686A6E` (RGB: 104, 106, 110) - Gris
- **Fondo**: `#FFFFFF` (Blanco)
- **Fondo Alterno**: `#FAFAFA` (Gris muy claro)

#### Colores Derivados
- **Principal Claro**: `#C88E4E` (RGB: 200, 142, 78)
- **Principal Oscuro**: `#8F5625` (RGB: 143, 86, 37)
- **Secundario Claro**: `#96989A` (RGB: 150, 152, 154)

#### Colores de Estado
- **Éxito**: `#2ECC71` (RGB: 46, 204, 113) - Verde
- **Advertencia**: `#F39C12` (RGB: 243, 156, 18) - Amarillo/Naranja
- **Error**: `#E74C3C` (RGB: 231, 76, 60) - Rojo
- **Info**: `#3498DB` (RGB: 52, 152, 219) - Azul

#### Colores de Texto
- **Texto Oscuro**: `#212121` (RGB: 33, 33, 33)
- **Texto Claro**: `#FFFFFF` (Blanco)
- **Texto Secundario**: `#757575` (RGB: 117, 117, 117)

---

## ??? Arquitectura del Sistema de Temas

### Clase Principal: `ThemeManager`

Ubicación: `DynamicSepticSystem\ThemeManager.cs`

#### Métodos Principales

1. **AplicarTema(Form form)**
   - Aplica el tema a todo un formulario y sus controles
   - Uso: `ThemeManager.AplicarTema(this);` en el constructor del formulario

2. **AplicarTemaControl(Control control)**
   - Aplica el tema a un control específico
   - Se llama automáticamente para cada control del formulario

3. **Métodos de Estilización Específicos**
   - `EstilizarBoton(Button btn)` - Botón principal
   - `EstilizarBotonSecundario(Button btn)` - Botón secundario
   - `EstilizarBotonOutline(Button btn)` - Botón con borde
   - `EstilizarBotonExito(Button btn)` - Botón de éxito/confirmar
   - `EstilizarBotonPeligro(Button btn)` - Botón de cancelar/eliminar

---

## ?? Formularios Actualizados

### ? FormAvanceObra
**Cambios aplicados:**
- Tematización general aplicada en constructor
- Gráficas actualizadas con colores del tema
- Labels de avance con colores de estado dinámicos
- ProgressBar con estilo personalizado

```csharp
public FormAvanceObra()
{
    InitializeComponent();
    ConfigurarTreeListView();
    CargarManzanas();
    this.Load += FormAvanceObra_Load;
    
    // Aplicar tema
    ThemeManager.AplicarTema(this);
}
```

### ? PanelPrincipal
**Cambios aplicados:**
- Tema Material personalizado con colores corporativos
- MenuStrip con colores del tema
- Labels y botones actualizados
- Integración con MaterialSkin manteniendo colores personalizados

```csharp
private void AplicarTemaMaterialPersonalizado()
{
    var skinManager = MaterialSkinManager.Instance;
    skinManager.AddFormToManage(this);
    skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
    
    skinManager.ColorScheme = new ColorScheme(...);
    ThemeManager.AplicarTema(this);
}
```

### ? FormLogin
**Cambios aplicados:**
- Material Design con colores personalizados
- Labels de versión con colores de estado
- Botones y controles estilizados

---

## ?? Comportamientos Especiales

### Paneles
- **Nombres con "Header", "Title", "Top"** ? Fondo principal (#B36C2E)
- **Nombres con "Footer", "Bottom"** ? Fondo secundario (#686A6E)
- **Otros paneles** ? Fondo blanco

### Labels
- **En paneles oscuros** ? Texto claro (blanco)
- **En paneles claros** ? Texto oscuro
- **Con "Title", "Header", "lblTitulo"** ? Color principal y tamaño aumentado

### DataGridView
- **Encabezados** ? Fondo principal, texto blanco
- **Filas alternas** ? Fondo alterno claro
- **Selección** ? Principal claro
- **Bordes** ? Gris claro

### MenuStrip
- **Fondo** ? Color principal
- **Items principales** ? Texto blanco
- **Submenús** ? Fondo blanco, texto oscuro

### TabControl
- **Pestaña seleccionada** ? Color principal
- **Pestañas inactivas** ? Color secundario
- **Texto** ? Blanco en ambos casos

---

## ?? Gráficas y Visualizaciones

### FormAvanceObra - Gráficas
- **Gráfica de pastel**: Ejecutado (principal), Restante (gris claro)
- **Gráfica de barras**: Barras ejecutadas (principal), fondo (gris claro)
- **Porcentajes**: Texto oscuro, bordes secundarios

---

## ?? Cómo Usar el Sistema de Temas

### Para Nuevos Formularios

```csharp
public FormEjemplo()
{
    InitializeComponent();
    
    // Aplicar tema automáticamente
    ThemeManager.AplicarTema(this);
}
```

### Para Botones Específicos

```csharp
// Botón secundario (gris)
ThemeManager.EstilizarBotonSecundario(btnCancelar);

// Botón de éxito (verde)
ThemeManager.EstilizarBotonExito(btnGuardar);

// Botón de peligro (rojo)
ThemeManager.EstilizarBotonPeligro(btnEliminar);

// Botón outline (borde)
ThemeManager.EstilizarBotonOutline(btnOpcional);
```

### Para Controles Individuales

```csharp
// Aplicar tema a un control específico
ThemeManager.AplicarTemaControl(miControl);
```

---

## ?? Convenciones de Nomenclatura

Para que el tema se aplique correctamente automáticamente:

### Paneles
- `panelTop`, `panelHeader`, `panelTitle` ? Fondo principal
- `panelFooter`, `panelBottom` ? Fondo secundario
- `panelMain`, `panelContent` ? Fondo blanco

### Labels
- `lblTitulo`, `lblTitle`, `lblHeader` ? Color principal, negrita, tamaño +2

---

## ? Lista de Verificación

- [x] Clase ThemeManager creada
- [x] FormAvanceObra actualizado
- [x] PanelPrincipal actualizado  
- [x] FormLogin actualizado
- [ ] FormAvanceConcepto (por aplicar)
- [ ] FormEvidenciasFotograficas (por aplicar)
- [ ] FormCompraMulti (por aplicar)
- [ ] FormCompraIndirecta (por aplicar)
- [ ] FormEstimacionConceptoMigrado (por aplicar)
- [ ] Otros formularios (por aplicar)

---

## ?? Notas Importantes

1. **Material Design**: Los formularios que heredan de `MaterialForm` requieren configuración especial del `MaterialSkinManager` además del `ThemeManager`.

2. **Fondo Blanco**: El fondo principal se mantiene blanco para legibilidad. Los colores principales se usan en encabezados, botones y acentos.

3. **Accesibilidad**: Los contrastes de color cumplen con estándares WCAG 2.1 AA.

4. **Consistencia**: Todos los formularios deben llamar a `ThemeManager.AplicarTema(this)` en su constructor.

---

## ?? Próximos Pasos

1. Aplicar tema a formularios restantes
2. Actualizar gráficos PDF con colores del tema
3. Crear documentación visual de la paleta
4. Implementar tema oscuro (opcional)

---

**Última actualización**: ${new Date().toLocaleDateString('es-MX')}
