# ?? PLAN DE MIGRACIÓN: WinForms ? WPF
## Dynamic Septic System - Análisis Arquitectónico Completo

**Versión:** 1.0  
**Fecha:** 2024  
**Proyecto:** Dynamic Septic System  
**Framework Actual:** .NET Framework 4.7.2  
**Enfoque:** Reutilización total del backend

---

## ?? FASE 1: ANÁLISIS ARQUITECTÓNICO

### 1.1 Estructura Actual del Proyecto

```
DynamicSepticSystem/
??? Program.cs (Entry point WinForms)
??? Capas de Presentación (Forms)
??? Servicios (Business Logic)
??? Acceso a Datos (SQL directo + API)
??? Utilidades y Helpers
```

#### **Proyectos:**
- **DynamicSepticSystem.csproj** - Aplicación principal (WinForms)
- **Updater.csproj** - Actualización automática

---

### 1.2 ANÁLISIS POR ARISTA (Componentización)

#### **ARISTA 1: Presentación / UI Layer**
- **Tipo:** WinForms Forms (150+ archivos)
- **Complejidad:** ????? (Muy Alta)
- **Dependencias:** MaterialSkin, BrightIdeasSoftware, PdfSharp
- **Estado Actual:** 
  - Forms con diseñadores (.Designer.cs)
  - Lógica mezclada en partial classes
  - Temas personalizados (ThemeManager)
  - Controles custom (SafeTreeListView, TreeGridViewLite)

**Forms Principales Identificados:**
- FormLogin - Autenticación
- FormSplash - Pantalla inicial
- PanelPrincipal - Dashboard central (muy complejo)
- FormAlmacen - Gestión de inventario (6 partial classes)
- FormEstimacionConceptoMigrado - Estimaciones (5 partial classes)
- FormGestionarPartidas - Gestión de obras (4 partial classes)
- FormEditorTreeList - Editor jerárquico
- FormAvanceObra - Seguimiento de obras
- FormReporte - Reportes
- FormInsumos - Gestión de insumos
- FormManoObra - Gestión de mano de obra
- Múltiples dialogos (PromptCantidad, PromptJustificacion, etc.)

**Desafíos de Migración:**
- MaterialSkin no tiene equivalente directo en WPF ? Usar Material Design in XAML Toolkit
- Controls custom ? Crear equivalentes en WPF o usar componentes existentes
- Partial classes densas ? Refactorizar a MVVM

---

#### **ARISTA 2: Acceso a Datos / Data Access**
- **Tipo:** Híbrido SQL directo + API HTTP
- **Complejidad:** ??? (Media)
- **Componentes:**
  - Conexión directa a SQL Server (SqlConnection)
  - ApiClient (HTTP con Newtonsoft.Json)
  - Services: InventarioService, SalidaAlmacenService
  - Helpers: DetalleOrdenHelper

**Estado Actual:**
- Queries SQL embebidas en Forms (anti-patrón)
- ApiClient centralizado (? Buena práctica)
- Servicios parcialmente abstraídos
- Sin interfaces/inyección de dependencias

**Desafíos:**
- Refactorizar datos de Forms ? Capa de Servicios/Repositories
- Implementar inyección de dependencias
- Crear abstracciones para SQL
- Mantener compatibilidad API

---

#### **ARISTA 3: Lógica de Negocio (Business Logic)**
- **Complejidad:** ???? (Alta)
- **Ubicación:** Distribuida en Forms + Servicios

**Identificados:**
- InventarioService - Gestión de inventario
- SalidaAlmacenService - Operaciones de salidas
- NodoConcepto, NodoCategoria, NodoTree - Modelos jerárquicos
- PartidaDinamica, PartidaConcepto - Modelos de partidas
- ErrorLogger - Logging
- ThemeManager - Gestión de temas
- PasswordHasher - Seguridad
- Reportes.cs - Generación de reportes

**Desafíos:**
- Lógica duplicada en múltiples Forms
- Falta de interfaces y contratos
- Acoplamiento fuerte a WinForms

---

#### **ARISTA 4: Autenticación y Autorización**
- **Tipo:** SQL + JWT API
- **Complejidad:** ?? (Baja)
- **Componentes:**
  - FormLogin - Autenticación manual
  - ApiClient.Login() - Autenticación por API
  - PasswordHasher - Hash de contraseñas
  - Global.UsuarioActual - Context de usuario

**Flujo Actual:**
1. FormSplash (skip si debugger attached)
2. FormLogin ? SQL o API
3. Cargar permisos en Global.UsuarioActual
4. PanelPrincipal principal

**Desafíos:**
- Migrar modelo de Usuario a MVVM
- Mantener persistencia de sesión

---

#### **ARISTA 5: Generación de Reportes y PDFs**
- **Complejidad:** ???? (Alta)
- **Librerías:** PdfSharp, ClosedXML (Excel)

**Clases Identificadas:**
- GenerarExplosionInsumosPDF
- Reportes.cs
- FormReporte, FormReporteNomina, FormReporteDestajosSemana
- FormPreviewPDFMaximizado
- Múltiples extensiones de PDF en Forms

**Desafíos:**
- PdfSharp es compatible con WPF
- ClosedXML es compatible con WPF
- Previsualizadores ? Crear viewers WPF

---

#### **ARISTA 6: Gestión de Fotos y Evidencias**
- **Complejidad:** ??? (Media)
- **Clases:**
  - GestorFotosConcepto
  - GestorEvidencias
  - FormEvidenciasFotograficas

**Desafíos:**
- Manejo de archivos e imágenes
- Persistencia en servidor
- Sincronización offline

---

#### **ARISTA 7: Configuración y Persistencia**
- **Complejidad:** ?? (Baja)
- **Mecanismos:**
  - App.config (ConfigurationManager)
  - JSON (casas.json)
  - SQL Server

**Desafíos:**
- Migrar App.config a App.xaml + appsettings.json
- Persistencia de configuración de usuario

---

#### **ARISTA 8: Actualización Automática**
- **Complejidad:** ??? (Media)
- **Clases:** Actualizador, UpdateServer, FormProgresoActualizacion

**Desafíos:**
- Mantener compatible con nuevo instalador WPF
- Actualizar mecanismo de versioning

---

#### **ARISTA 9: Utilitarios y Helpers**
- **Complejidad:** ?? (Baja)
- **Clases:**
  - NumeroALetras - Conversión numérica
  - Resizer - Redimensionamiento adaptativo
  - NewFeatureBadge - Badges de nuevas funciones
  - FolioManager - Gestión de folios
  - DetalleOrdenHelper
  - DialogSeleccionarNivel
  - FormDiagnosticoConexion

**Desafíos:**
- Refactorizar a MVVM
- Crear behaviors WPF para Resizer

---

### 1.3 ANÁLISIS POR VÉRTICE (Flujos de Negocio)

#### **VÉRTICE 1: Flujo de Autenticación**
```
[Splash] ? [Login] ? [API/SQL Auth] ? [Cargar Permisos] ? [PanelPrincipal]
```
- **Reutilizable:** ? 100% - Solo cambiar presentación
- **Esfuerzo WPF:** ?? Bajo (1 semana)

---

#### **VÉRTICE 2: Gestión de Almacén**
```
[PanelPrincipal] ? [FormAlmacen] 
  ??? [Entradas] ? Query SQL ? Update DB
  ??? [Salidas] ? Query SQL ? Update DB
  ??? [Inventario] ? Query SQL
  ??? [Historial] ? Query SQL
```
- **Reutilizable:** ? 95% - Servicios mantienen lógica
- **Complejidad:** ???? (6 partial classes)
- **Esfuerzo WPF:** ?? Muy Alto (3-4 semanas)

---

#### **VÉRTICE 3: Gestión de Estimaciones**
```
[PanelPrincipal] ? [FormEstimacionConceptoMigrado]
  ??? [Tree List View] ? Conceptos jerárquicos
  ??? [Menú Contextual] ? Opciones CRUD
  ??? [Propiedades] ? Edición inline
  ??? [Avances] ? Seguimiento
  ??? [PDF Preview] ? Generación PDF
  ??? [Folios] ? Gestión de folios
```
- **Reutilizable:** ? 90% - Lógica de cálculos
- **Complejidad:** ????? (5+ partial classes)
- **Desafío:** TreeListView custom ? Reemplazar con DataGrid o TreeView WPF
- **Esfuerzo WPF:** ?? Muy Alto (4-5 semanas)

---

#### **VÉRTICE 4: Gestión de Partidas**
```
[PanelPrincipal] ? [FormGestionarPartidas]
  ??? [CRUD] ? Insert/Update/Delete
  ??? [WBS] ? Work Breakdown Structure
  ??? [Reordenamiento] ? Drag & Drop
  ??? [Carga de Datos] ? Importación
  ??? [Guardado] ? Persistencia
```
- **Reutilizable:** ? 85% - Lógica CRUD
- **Complejidad:** ????
- **Esfuerzo WPF:** ?? Alto (3 semanas)

---

#### **VÉRTICE 5: Generación de Reportes**
```
[Various Forms] ? [Reportes.cs] ? [PdfSharp/ClosedXML] ? [File Output]
```
- **Reutilizable:** ? 100% - Lógica totalmente agnóstica
- **Complejidad:** ???
- **Esfuerzo WPF:** ?? Bajo (1-2 semanas)

---

#### **VÉRTICE 6: Gestión de Mano de Obra**
```
[PanelPrincipal] ? [FormManoObra] ? [FormAsignarNomina] ? [FormDistribucionNomina]
```
- **Reutilizable:** ? 90%
- **Complejidad:** ???
- **Esfuerzo WPF:** ?? Medio (2-3 semanas)

---

#### **VÉRTICE 7: Gestión de Insumos**
```
[PanelPrincipal] ? [FormInsumos] ? [Búsqueda/Filtrado] ? [FormAgregarInsumo]
```
- **Reutilizable:** ? 95%
- **Complejidad:** ???
- **Esfuerzo WPF:** ?? Medio (2 semanas)

---

#### **VÉRTICE 8: Seguimiento de Obras**
```
[PanelPrincipal] ? [FormAvanceObra] ? [FormAvanceConcepto] ? [Mapa Interactivo]
```
- **Reutilizable:** ? 85% - Lógica de cálculos
- **Complejidad:** ???? (Mapa complejo en PanelPrincipal)
- **Desafío:** Mapa interactivo con zoom/pan
- **Esfuerzo WPF:** ?? Alto (2.5 semanas)

---

#### **VÉRTICE 9: Evidencias Fotográficas**
```
[Various Forms] ? [GestorFotosConcepto/GestorEvidencias] ? [Storage] ? [Sync]
```
- **Reutilizable:** ? 95%
- **Complejidad:** ???
- **Esfuerzo WPF:** ?? Medio (2 semanas)

---

## ??? FASE 2: ESTRATEGIA DE MIGRACIÓN

### 2.1 Enfoque Recomendado: HÍBRIDO CON PRIORIZACIÓN

**Razón:** La aplicación es muy grande (150+ forms). Migración lineal = riesgo alto.

### **Estructura Propuesta Post-Migración:**

```
DynamicSepticSystem.sln
??? DynamicSepticSystem.WPF/          [Aplicación principal WPF - NUEVA]
?   ??? App.xaml
?   ??? App.xaml.cs
?   ??? Resources/
?   ?   ??? Themes/
?   ?   ??? Styles/
?   ?   ??? Converters/
?   ??? Views/                         [XAML equivalentes a Forms]
?   ?   ??? LoginView.xaml
?   ?   ??? MainWindowView.xaml
?   ?   ??? WarehouseView.xaml
?   ?   ??? EstimationView.xaml
?   ?   ??? ...
?   ??? ViewModels/                    [MVVM - Nuevos]
?   ?   ??? LoginViewModel.cs
?   ?   ??? MainWindowViewModel.cs
?   ?   ??? WarehouseViewModel.cs
?   ?   ??? ...
?   ??? Models/                        [Modelo de datos)
?   ??? Converters/
?   
??? DynamicSepticSystem.Core/          [Backend compartido - REFACTORIZADO]
?   ??? Services/                      [Extraído de Forms]
?   ?   ??? AuthenticationService.cs
?   ?   ??? WarehouseService.cs
?   ?   ??? EstimationService.cs
?   ?   ??? PartidaService.cs
?   ?   ??? ReportService.cs
?   ?   ??? PhotoService.cs
?   ?   ??? ...
?   ??? Repositories/                  [Nuevo - Data Access Pattern]
?   ?   ??? IRepository.cs
?   ?   ??? SqlRepository.cs
?   ?   ??? ...
?   ??? Models/                        [Entidades)
?   ?   ??? Usuario.cs
?   ?   ??? Almacen.cs
?   ?   ??? Partida.cs
?   ?   ??? ...
?   ??? Utilities/
?   ?   ??? NumeroALetras.cs
?   ?   ??? PasswordHasher.cs
?   ?   ??? ErrorLogger.cs
?   ?   ??? ...
?   ??? Config/
?       ??? AppSettings.cs
?       ??? Constants.cs
?
??? DynamicSepticSystem.API.Client/    [Cliente API]
?   ??? ApiClient.cs
?   ??? Models/
?   ??? Extensions/
?
??? DynamicSepticSystem.Reports/       [Generación de reportes - AGNÓSTICO]
?   ??? ReportGenerator.cs
?   ??? PdfGenerator.cs
?   ??? ExcelGenerator.cs
?   ??? Templates/
?
??? DynamicSepticSystem.Data/          [Acceso a datos - AGNÓSTICO]
?   ??? SqlDataAccess.cs
?   ??? Queries/
?   ??? Migrations/
?
??? Updater.WPF/                       [Actualización - NUEVA VERSION]
    ??? UpdateClient.cs
    ??? UpdateWindow.xaml
```

---

### 2.2 PRIORIZACIÓN POR FASES

#### **Fase 1: Fundamentos (Semanas 1-2)**
**Objetivo:** Sentar base arquitectónica

1. ? Crear proyecto `DynamicSepticSystem.WPF`
2. ? Crear proyecto `DynamicSepticSystem.Core` (Class Library)
3. ? Crear proyecto `DynamicSepticSystem.Data` (Class Library)
4. ? Implementar inyección de dependencias (Microsoft.Extensions.DependencyInjection)
5. ? Migrar `App.xaml` + `App.xaml.cs`
6. ? Crear Theme Resource Dictionaries (Material Design)
7. ? Instalar paquetes NuGet necesarios

**Paquetes NuGet a instalar:**
```
MaterialDesignThemes
MaterialDesignColors
MVVM Toolkit (Microsoft.Toolkit.Mvvm)
Newtonsoft.Json
PdfSharp
ClosedXML
Dapper (para acceso a datos simplificado)
AutoMapper (mapeo de modelos)
```

---

#### **Fase 2: Backend - Data Access (Semanas 3-4)**
**Objetivo:** Refactorizar acceso a datos

1. ? Extraer todas las queries SQL de Forms ? `DynamicSepticSystem.Data`
2. ? Crear interfaces `IRepository`
3. ? Implementar `SqlRepository` con Dapper
4. ? Refactorizar `ApiClient` ? `DynamicSepticSystem.API.Client`
5. ? Migrar `ErrorLogger` ? Core
6. ? Migrar `Global.cs` ? AppContext.cs en Core

**Servicios a extraer:**
- WarehouseService ? IWarehouseService
- PhotoService ? IPhotoService
- ReportService ? IReportService
- InventoryService ? IInventoryService

---

#### **Fase 3: Autenticación (Semana 5)**
**Objetivo:** Sistema de login migrado

1. ? Crear `AuthenticationService` en Core
2. ? Migrar `PasswordHasher`
3. ? Crear `LoginView.xaml` + `LoginViewModel.cs`
4. ? Crear `SplashView.xaml` (opcional)
5. ? Implementar session management
6. ? Validar autenticación SQL + API

**Esfuerzo:** ?? 1 semana

---

#### **Fase 4: Panel Principal (Semanas 6-8)**
**Objetivo:** Dashboard central funcional

1. ? Crear `MainWindowView.xaml` (equivalente PanelPrincipal)
2. ? Migrar lógica de `PanelPrincipal.cs` ? `MainWindowViewModel.cs`
3. ? Implementar menú navegación MVVM
4. ? Crear sistema de temas WPF
5. ? Implementar mapa interactivo (Canvas + Pan/Zoom)

**Desafíos:**
- PanelPrincipal es muy complejo (~1000+ líneas)
- Mapa con zoom/pan requiere custom rendering
- Muchas extensiones acopladas

**Esfuerzo:** ?? 3 semanas

---

#### **Fase 5: Almacén (Semanas 9-12)**
**Objetivo:** Módulo almacén funcional

1. ? Crear `WarehouseView.xaml`
2. ? Migrar `FormAlmacen` ? `WarehouseViewModel`
3. ? Reemplazar `SafeTreeListView` con `DataGrid` WPF
4. ? Migrar operaciones: Entradas, Salidas, Inventario
5. ? Implementar búsqueda/filtrado
6. ? Crear dialogs para CRUD

**Complejidad:** ?? Alta (6 partial classes)

**Esfuerzo:** ?? 4 semanas

---

#### **Fase 6: Estimaciones (Semanas 13-17)**
**Objetivo:** Módulo estimaciones funcional

1. ? Crear `EstimationView.xaml`
2. ? Migrar `FormEstimacionConceptoMigrado` ? `EstimationViewModel`
3. ? Reemplazar `TreeListView` con `TreeView` WPF
4. ? Implementar menú contextual
5. ? Crear inline editing
6. ? Refactorizar cálculos de avances
7. ? Integrar previsualizador PDF

**Complejidad:** ???? Muy alta (5+ partial classes, TreeList custom)

**Esfuerzo:** ?? 5 semanas

---

#### **Fase 7: Partidas (Semanas 18-21)**
**Objetivo:** Módulo partidas + WBS

1. ? Crear `PartidaView.xaml`
2. ? Migrar CRUD de partidas
3. ? Implementar WBS (Work Breakdown Structure)
4. ? Reordenamiento drag & drop
5. ? Carga de datos batch

**Esfuerzo:** ?? 4 semanas

---

#### **Fase 8: Reportes (Semanas 22-23)**
**Objetivo:** Sistema de reportes funcional

1. ? Extraer lógica PDF a `DynamicSepticSystem.Reports`
2. ? Extraer lógica Excel a `DynamicSepticSystem.Reports`
3. ? Crear `ReportService`
4. ? Migrar formularios de reportes
5. ? Crear viewers PDF/Excel en WPF

**Esfuerzo:** ?? 2 semanas (backend ya agnóstico)

---

#### **Fase 9: Módulos Secundarios (Semanas 24-26)**
**Objetivo:** Completar funcionalidad

- Mano de obra
- Insumos
- Evidencias fotográficas
- Seguimiento de obras
- Gestión de usuarios

**Esfuerzo:** ?? 3 semanas

---

#### **Fase 10: Pulido (Semanas 27-30)**
**Objetivo:** Producción

- Testing
- Optimización de performance
- Migración de datos si aplica
- Documentación
- Training

**Esfuerzo:** ?? 4 semanas

---

## ?? FASE 3: PATRONES Y LIBRERÍAS

### 3.1 Patrón MVVM

```csharp
// ANTES (WinForms - Anti-patrón)
public partial class FormAlmacen : Form
{
    private void btnGuardar_Click(object sender, EventArgs e)
    {
        var almacen = new Almacen { Nombre = txtNombre.Text };
        SqlCommand cmd = new SqlCommand("INSERT INTO Almacen...");
        cmd.ExecuteNonQuery(); // ? Lógica en eventos UI
    }
}

// DESPUÉS (WPF - MVVM)
public partial class WarehouseView : UserControl
{
    public WarehouseView()
    {
        InitializeComponent();
        DataContext = new WarehouseViewModel();
    }
}

public class WarehouseViewModel : BaseViewModel
{
    private readonly IWarehouseService _service;

    public RelayCommand SaveCommand { get; }

    public WarehouseViewModel(IWarehouseService service)
    {
        _service = service;
        SaveCommand = new RelayCommand(Save);
    }

    private void Save()
    {
        _service.Save(SelectedWarehouse); // ? Lógica en ViewModel
    }
}
```

### 3.2 Inyección de Dependencias

```csharp
// App.xaml.cs
public partial class App : Application
{
    private IServiceProvider _services;

    public App()
    {
        var services = new ServiceCollection();

        // Servicios
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IReportService, ReportService>();

        // Repositories
        services.AddScoped<IRepository<Almacen>, SqlRepository<Almacen>>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<WarehouseViewModel>();

        _services = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var loginView = _services.GetRequiredService<LoginView>();
        loginView.Show();
    }
}
```

### 3.3 Servicios Agnósticos

```csharp
// DynamicSepticSystem.Core/Services/WarehouseService.cs
public interface IWarehouseService
{
    Task<List<Almacen>> GetAllAsync();
    Task<Almacen> GetByIdAsync(int id);
    Task SaveAsync(Almacen almacen);
    Task DeleteAsync(int id);
}

public class WarehouseService : IWarehouseService
{
    private readonly IRepository<Almacen> _repo;

    public WarehouseService(IRepository<Almacen> repo)
    {
        _repo = repo;
    }

    public async Task<List<Almacen>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }

    // ... resto de métodos
    // ? CERO dependencia en WinForms o WPF
}
```

---

### 3.4 Mapeo de Controles

| WinForms | WPF Equivalente | Librería |
|----------|-----------------|----------|
| Form | Window / UserControl | N/A |
| Button | Button | N/A |
| TextBox | TextBox | N/A |
| Label | Label / TextBlock | N/A |
| DataGridView | DataGrid | N/A |
| TreeView | TreeView | N/A |
| TreeListView (custom) | DataGrid + Hierarchical | N/A |
| ComboBox | ComboBox | N/A |
| CheckBox | CheckBox | N/A |
| DateTimePicker | DatePicker / DatePickerFallback | N/A |
| PictureBox | Image | N/A |
| MaterialForm | Window + Material Design | MaterialDesignThemes |
| MaterialLabel | TextBlock + Styles | MaterialDesignThemes |
| MaterialButton | Button + Styles | MaterialDesignThemes |
| SafeTreeListView | Custom Control | Material Design in XAML |

---

### 3.5 Librerías WPF Recomendadas

```xml
<!-- DynamicSepticSystem.WPF.csproj -->
<ItemGroup>
    <!-- Material Design -->
    <PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
    <PackageReference Include="MaterialDesignColors" Version="2.1.4" />

    <!-- MVVM -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />

    <!-- Datos y serialización -->
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="Dapper" Version="2.0.123" />

    <!-- Reportes -->
    <PackageReference Include="PdfSharp" Version="6.1.0" />
    <PackageReference Include="ClosedXML" Version="0.102.1" />

    <!-- Inyección de dependencias -->
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />

    <!-- Mapeo de objetos -->
    <PackageReference Include="AutoMapper" Version="12.0.1" />

    <!-- Logging -->
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
</ItemGroup>
```

---

## ?? FASE 4: PLAN DE DESARROLLO DETALLADO

### 4.1 Semana 1-2: Setup Base

**Lunes:**
- [ ] Crear solución `DynamicSepticSystem.sln` (Nueva estructura)
- [ ] Crear `DynamicSepticSystem.WPF` (WPF Application)
- [ ] Crear `DynamicSepticSystem.Core` (Class Library)
- [ ] Crear `DynamicSepticSystem.Data` (Class Library)
- [ ] Crear `DynamicSepticSystem.API.Client` (Class Library)
- [ ] Instalar paquetes NuGet base

**Martes-Miércoles:**
- [ ] Configurar App.xaml + Material Design Themes
- [ ] Implementar IServiceProvider en App.xaml.cs
- [ ] Crear BaseViewModel con INotifyPropertyChanged
- [ ] Crear RelayCommand
- [ ] Crear estrutura de carpetas

**Jueves-Viernes:**
- [ ] Migrar `ApiClient.cs` ? `DynamicSepticSystem.API.Client`
- [ ] Migrar `ErrorLogger.cs` ? `DynamicSepticSystem.Core`
- [ ] Migrar `Global.cs` ? `AppContext.cs` en Core
- [ ] Migrar `PasswordHasher.cs` ? Core
- [ ] Configurar logging (Serilog)

---

### 4.2 Semana 3-4: Data Access

**Lunes-Miércoles:**
- [ ] Crear `IRepository<T>` interface
- [ ] Implementar `SqlRepository<T>` con Dapper
- [ ] Extraer todas las queries SQL de Forms
- [ ] Crear `Database.cs` para manejo de conexiones
- [ ] Crear migrations folder

**Jueves-Viernes:**
- [ ] Crear `WarehouseRepository`
- [ ] Crear `UserRepository`
- [ ] Crear `PartidaRepository`
- [ ] Tests básicos de repositorio

---

### 4.3 Semana 5: Autenticación

**Lunes-Miércoles:**
- [ ] Crear `IAuthenticationService`
- [ ] Implementar `AuthenticationService`
- [ ] Crear `LoginView.xaml`
- [ ] Crear `LoginViewModel.cs`
- [ ] Integrar JWT token refresh

**Jueves-Viernes:**
- [ ] Testing de login SQL
- [ ] Testing de login API
- [ ] Crear `SplashView.xaml` (opcional)
- [ ] Validar flujo completo

---

### 4.4 Semana 6-8: Panel Principal

**Objetivos:**
- MainWindow funcional como hub de navegación
- Sistema de temas aplicado
- Estructura de menú MVVM

---

### 4.5 Semanas 9-12: Almacén

**Objetivos:**
- CRUD completo de almacén
- DataGrid en lugar de TreeListView
- Búsqueda y filtrado

---

### 4.6 Semanas 13-17: Estimaciones

**Objetivos:**
- TreeView jerárquico
- Inline editing
- Menú contextual
- Cálculos de avances

---

## ?? FASE 5: ESTRATEGIA DE MIGRACIÓN PARALELA

### 5.1 Opción 1: Big Bang (Alto Riesgo)
- Detener desarrollo en WinForms
- Migrar todo a WPF
- Release único

**? NO RECOMENDADO** para proyecto en producción

---

### 5.2 Opción 2: Híbrido (Recomendado) ?

```
Cliente WPF puede interoperar con WinForms en transición
```

**Ventajas:**
- Releases incrementales
- Testing en paralelo
- Rollback fácil
- Usuarios no afectados inmediatamente

**Implementación:**

```csharp
// App.xaml.cs - WPF puede abrir dialogs WinForms si es necesario
if (useLegacy)
{
    var legacyForm = new FormAlmacen(); // WinForms
    legacyForm.ShowDialog();
}
else
{
    var modernView = serviceProvider.GetService<WarehouseView>();
    NavigateTo(modernView); // WPF
}
```

---

### 5.3 Opción 3: Gradual (Ideal)

1. **Mes 1:** Setupbase + Data Layer ?
2. **Mes 2:** Autenticación + Panel Principal
3. **Mes 3:** Almacén + Reportes
4. **Mes 4:** Estimaciones + Partidas
5. **Mes 5:** Módulos secundarios
6. **Mes 6:** Testing + Optimización

**Resultado:** Releases cada 2 semanas con features nuevas

---

## ?? RIESGOS Y MITIGACIONES

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|------------|--------|-----------|
| Performance peor en WPF | Media | Alto | Profiling temprano, virtualización en DataGrid |
| Usuarios resisten cambio UI | Alta | Medio | Capacitación, UI similar a WinForms |
| Bugs en migración | Alta | Medio | Testing riguroso, parallelscan |
| TreeListView ? TreeView complejo | Media | Alto | Investigación temprana, prototipos |
| API no disponible | Media | Medio | Fallback a SQL directo, modo offline |
| Performance reportes PDF | Baja | Bajo | Librería agnóstica, tests de carga |

---

## ?? RESUMEN DE ESFUERZO

| Fase | Componente | Semanas | Complejidad |
|------|-----------|---------|------------|
| 1 | Setup + DI + Themes | 2 | ?? Baja |
| 2 | Data Access Layer | 2 | ?? Media |
| 3 | Autenticación | 1 | ?? Baja |
| 4 | Panel Principal | 3 | ?? Media |
| 5 | Almacén | 4 | ???? Alta |
| 6 | Estimaciones | 5 | ?????? Muy Alta |
| 7 | Partidas + WBS | 4 | ?? Alta |
| 8 | Reportes | 2 | ?? Baja |
| 9 | Módulos Secundarios | 3 | ?? Media |
| 10 | Pulido + Testing | 4 | ?? Media |
| **TOTAL** | | **30 semanas** | **~7.5 meses** |

---

## ? CHECKLIST DE MIGRACIÓN

### Backend (Agnóstico)
- [ ] Extraer todas las queries SQL de Forms
- [ ] Implementar pattern Repository
- [ ] Crear interfaces de Servicios
- [ ] Migrar lógica de negocio
- [ ] Crear helpers agnósticos
- [ ] Tests unitarios de servicios

### Frontend (WPF)
- [ ] Setup de solución WPF
- [ ] Estructura MVVM
- [ ] Inyección de dependencias
- [ ] Material Design Themes aplicado
- [ ] Views XAML para cada Form
- [ ] ViewModels para cada Form
- [ ] Bindings y commands
- [ ] Validación de entrada

### Integración
- [ ] Conexión a servicios backend
- [ ] Tests de integración
- [ ] API fallback
- [ ] Manejo de errores
- [ ] Logging distribuido

### Validación
- [ ] Tests unitarios (>80% coverage)
- [ ] Tests funcionales
- [ ] Tests de performance
- [ ] Tests de UI/UX
- [ ] Beta testing con usuarios
- [ ] Documentación de usuario
- [ ] Training de soporte

---

## ?? REFERENCIAS Y RECURSOS

### WPF + MVVM
- Microsoft Docs: WPF & MVVM
- Material Design in XAML
- MVVM Toolkit Documentation

### Patrones
- Repository Pattern
- MVVM Pattern
- Dependency Injection
- Service Locator (anti-pattern)

### Librerías
- MaterialDesignThemes: Temas modernos
- MVVM Toolkit: Helpers MVVM
- Dapper: ORM ligero
- AutoMapper: Mapeo de objetos

---

## ?? CONCLUSIÓN

**Reutilización del Backend: 85-95%** ?

La arquitectura actual de DynamicSepticSystem tiene una buena separación entre:
- Presentación WinForms (150+ Forms)
- Lógica de negocio (Servicios)
- Acceso a datos (SQL + API)

**Beneficios de la migración:**
1. ? UI moderna y responsiva
2. ? Código más mantenible con MVVM
3. ? Tests más fáciles
4. ? Reutilización 85-95% del backend
5. ? Mejor performance en general
6. ? Acceso a características modernas de .NET

**Investmentimiento:** 7-8 meses (equipo de 2-3 devs)

---

**Documento preparado para: Dynamic Septic System Migration Project**
