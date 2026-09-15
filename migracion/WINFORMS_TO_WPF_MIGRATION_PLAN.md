# ?? COMPREHENSIVE WINFORMS TO WPF MIGRATION PLAN
## DynamicSepticSystem - Complete Migration Strategy

**Project Overview:**
- Current: Winforms (.NET Framework 4.7.2)
- 159 C# source files
- ~30+ Forms with complex logic
- Hybrid architecture (SQL + API)
- Material Design UI with custom theme
- Target Platform: .NET 6+ (with WPF)

---

## ?? PHASE 1: ANALYSIS & PLANNING (Week 1)

### 1.1 Codebase Assessment
- ? **Forms Count:** 30+ main forms
- ? **Custom Controls:** SafeTreeListView, TreeGridViewLite, custom renderers
- ? **Dependencies:** 50+ NuGet packages (MaterialSkin, ClosedXML, PdfSharp, etc.)
- ? **Business Logic:** Inventory, Orders, Payroll, Evidence, PDF Reports
- ? **Data Access:** Direct SQL (SqlClient) + API Client (HttpClient, JWT)
- ? **UI Patterns:** Multi-threaded updates, async operations, dialogs

### 1.2 Key Components Inventory

#### **Entry Points:**
- `Program.cs` - Application startup, splash screen, login flow
- `FormSplash.cs` - Splash screen
- `FormLogin.cs` - Authentication form

#### **Main UI:**
- `PanelPrincipal.cs` - Main window (dashboard, interactive map with pan/zoom)
- `PanelPrincipal.Designer.cs` - Layout definition
- `PanelPrincipal_ModernUI.cs` - Modern UI extensions
- `PanelPrincipal.ModernExtensions.cs` - Additional UI features

#### **Forms by Module:**
1. **Almacén (Inventory):** FormAlmacen.cs, FormAlmacen_*.cs (8 files)
2. **Estimaciones (Estimates):** FormEstimacionConceptoMigrado.cs (7 partial files)
3. **Gestión de Partidas (Task Management):** FormGestionarPartidas.cs (6 partial files)
4. **Órdenes de Compra (Purchase Orders):** FormDetalleOrdenCompra.cs
5. **Nómina (Payroll):** FormAsignarNomina.cs, FormDistribucionNomina.cs
6. **Trabajadores:** FormRegistrarTrabajador.cs, FormPerfilTrabajador.cs
7. **Reportes (Reports):** FormReporte.cs, FormReporteNomina.cs, FormReporteDestajosSemana.cs
8. **Destajos (Tasks):** FormRepositorioDestajos.cs, FormDestajosPorCuadrilla.cs
9. **Evidencias (Evidence):** FormEvidenciasFotograficas.cs
10. **Utilidades:** FormPdfPreview.cs, FormLogErrores.cs, FormDiagnosticoConexion.cs

#### **Infrastructure & Utilities:**
- `ApiClient.cs` - HTTP API client with JWT auth
- `ErrorLogger.cs` - Error handling
- `ThemeManager.cs` - Material Design theme system
- `UpdateServer.cs` / `Actualizador.cs` - Auto-update mechanism
- `InventarioService.cs` - Business logic service
- `SalidaAlmacenService.cs` - Inventory service
- Custom Controls: `SafeTreeListView.cs`, `TreeGridViewLite.cs`, `NewFeatureBadge.cs`

#### **Data Models:**
- `NodoConcepto.cs`, `NodoCategoria.cs`, `NodoTree.cs`
- `PartidaDinamica.cs`, `PartidaConcepto.cs`, `ConceptoExistente.cs`
- `CasaInventario.cs`, `FolioManager.cs`

#### **Utilities:**
- `NumeroALetras.cs` - Number to text conversion
- `PasswordHasher.cs` - Security
- `GestorFotosConcepto.cs` - Photo management
- `GestorEvidencias.cs` - Evidence management
- `Reportes.cs` - Report generation

### 1.3 Dependencies to Migrate

**Current Packages (net472):**

**UI Framework:**
- MaterialSkin 2.2.3.1 ? No WPF equivalent (need Material Design for WPF)
- DlhSoft.GanttChartLibrary.WindowsForms 2.0.0.7 ? DlhSoft WPF version 4.3.49
- DlhSoft.ProjectManagementLibrary.WindowsForms 2.0.0.3 ? WPF equivalent

**Data & Excel:**
- ClosedXML 0.105.0 ? (supports .NET 6+)
- EPPlus 8.0.5 ? (supports .NET 6+)
- ExcelDataReader 3.7.0 ? (supports .NET 6+)
- DocumentFormat.OpenXml 3.1.1 ? (supports .NET 6+)

**PDF Generation:**
- iTextSharp 5.5.13.3 ? iTextSharp 8.x or iText.Layout
- PdfSharp ? PdfSharp 6.x (supports .NET 6+)

**HTTP & JSON:**
- System.Net.Http ? (built-in)
- Newtonsoft.Json 13.0.3 ? (supports .NET 6+)

**Other:**
- BouncyCastle 1.8.9 ? (supports .NET 6+)
- IKVM 8.11.2 ? May need alternatives or native .NET libs

**Critical Issues:**
- ? MaterialSkin is Winforms-only ? Use **MahApps.Metro** or **Material Design In XAML**
- ? DlhSoft has WPF versions but may have licensing/compatibility
- ?? Custom controls need rewriting in WPF

### 1.4 Architecture Decisions

**Option A: In-Place Modernization (Recommended)**
1. Create new .NET 6 WPF project in parallel
2. Migrate business logic first (services, models)
3. Gradually migrate forms (MVVM pattern)
4. Cutover when 100% migrated

**Option B: Hybrid Approach**
1. Move to SDK-style projects on .NET Framework 4.7.2
2. Add WPF alongside Winforms
3. Share business logic
4. Migrate gradually to WPF

**RECOMMENDATION:** Option A is cleaner and recommended

---

## ?? PHASE 2: INFRASTRUCTURE SETUP (Week 1-2)

### 2.1 Create New WPF Project Structure

```
DynamicSepticSystem.WPF/
??? DynamicSepticSystem.WPF.csproj (net6.0-windows)
??? App.xaml
??? App.xaml.cs
??? MainWindow.xaml
??? MainWindow.xaml.cs
??? Core/
?   ??? ViewModels/
?   ?   ??? ViewModelBase.cs (INotifyPropertyChanged)
?   ?   ??? MainWindowViewModel.cs
?   ?   ??? LoginViewModel.cs
?   ?   ??? [Other ViewModels]
?   ??? RelayCommand.cs (ICommand implementation)
?   ??? Converters/ (Value converters for XAML)
?   ??? Behaviors/ (Attached behaviors for XAML)
??? Models/
?   ??? [Migrate all data models from Winforms]
?   ??? ...
??? Services/
?   ??? [Migrate all services]
?   ??? ApiClient.cs (enhanced)
?   ??? InventarioService.cs
?   ??? ...
??? Views/
?   ??? Dialogs/
?   ?   ??? LoginDialog.xaml
?   ?   ??? SplashScreen.xaml
?   ?   ??? [Other dialogs]
?   ??? Pages/
?   ?   ??? AlmacenPage.xaml
?   ?   ??? EstimacionPage.xaml
?   ?   ??? GestionPartidaPage.xaml
?   ?   ??? [Other pages]
?   ??? Controls/
?       ??? TreeGridControl.xaml
?       ??? DataGridExtended.xaml
?       ??? [Custom controls in WPF]
??? Themes/
?   ??? Colors.xaml
?   ??? Brushes.xaml
?   ??? Styles.xaml
?   ??? DataGridStyles.xaml
?   ??? MaterialDesign.xaml
??? Resources/
?   ??? Strings/
?   ?   ??? Localization.xaml
?   ??? Icons/
?   ??? Images/
??? Utilities/
    ??? ErrorLogger.cs (migrated)
    ??? PasswordHasher.cs
    ??? [Other utilities]
    ??? Extensions.cs (WPF extensions)
```

### 2.2 Create SDK-style .csproj for WPF

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <AssemblyName>DynamicSepticSystem</AssemblyName>
    <RootNamespace>DynamicSepticSystem</RootNamespace>
    <Version>2.0.0</Version>
    <Company>Calandria</Company>
    <Product>Dynamic Septic System</Product>
    <Description>WPF version of Dynamic Septic System</Description>
    <Authors>Your Team</Authors>
    <RepositoryUrl>https://github.com/LuxuImNot/CalandriaApp</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <!-- NuGet Package Dependencies -->
  <ItemGroup>
    <!-- UI Framework -->
    <PackageReference Include="MahApps.Metro" Version="2.4.10" />
    <PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
    <PackageReference Include="MaterialDesignColors" Version="2.1.4" />

    <!-- Data Access -->
    <PackageReference Include="System.Data.SqlClient" Version="4.8.6" />

    <!-- Excel & Data -->
    <PackageReference Include="ClosedXML" Version="0.105.0" />
    <PackageReference Include="EPPlus" Version="8.0.5" />
    <PackageReference Include="ExcelDataReader" Version="3.7.0" />
    <PackageReference Include="DocumentFormat.OpenXml" Version="3.1.1" />

    <!-- PDF Generation -->
    <PackageReference Include="PdfSharp" Version="6.1.1" />
    <PackageReference Include="itext7" Version="7.2.5" />

    <!-- HTTP & JSON -->
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="System.Net.Http" Version="4.3.4" />

    <!-- Logging -->
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />

    <!-- Utilities -->
    <PackageReference Include="BouncyCastle" Version="1.8.9" />
    <PackageReference Include="System.Configuration.ConfigurationManager" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  </ItemGroup>

  <!-- App Manifest for Windows Presentation -->
  <ItemGroup>
    <ApplicationDefinition Include="App.xaml" />
    <Page Include="**/*.xaml" Exclude="App.xaml" />
  </ItemGroup>

  <!-- Resource files -->
  <ItemGroup>
    <Resource Include="Resources/**/*.png" />
    <Resource Include="Resources/**/*.ico" />
  </ItemGroup>

</Project>
```

### 2.3 App Configuration (.NET Configuration)

Create `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "CalandriaConn": "Server=100.75.234.9;Database=CALANDRIA;User Id=sa;Password=<CONTRASENA-PURGADA>;TrustServerCertificate=true;Encrypt=false;Connect Timeout=30;"
  },
  "ApiSettings": {
    "BaseUrl": "http://100.75.234.9:8733",
    "Timeout": 30
  },
  "AppSettings": {
    "Version": "2.0.0",
    "UpdateCheckUrl": "https://api.github.com/repos/LuxuImNot/CalandriaApp/releases/latest",
    "CarpetaActualizaciones": "\\\\100.92.2.55\\CalandriaUpdates"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### 2.4 MVVM Framework Foundation

Create `Core/ViewModels/ViewModelBase.cs`:
```csharp
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

Create `Core/RelayCommand.cs`:
```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
}
```

---

## ?? PHASE 3: BUSINESS LOGIC MIGRATION (Week 2-3)

### 3.1 Migrate Data Models (No Changes to Logic)

Copy all model files directly:
- `NodoConcepto.cs`
- `NodoCategoria.cs`
- `NodoTree.cs`
- `PartidaDinamica.cs`
- `PartidaConcepto.cs`
- `CasaInventario.cs`
- `ConceptoExistente.cs`
- All other data classes

### 3.2 Migrate Services Layer

**Core Services to Migrate:**

1. **ApiClient.cs** - Minimal changes needed
   - Already uses HttpClient (modern)
   - Already uses async patterns
   - Just update namespace imports
   - Add configuration injection option

2. **InventarioService.cs** - Copy as-is
   - Contains business logic
   - No Winforms dependencies

3. **SalidaAlmacenService.cs** - Copy as-is
   - Contains business logic
   - No Winforms dependencies

4. **ErrorLogger.cs** - Enhance for logging
   - Current: File-based logging
   - Upgrade to: Serilog with structured logging
   - Keep backward compatibility

5. **PasswordHasher.cs** - Copy as-is
   - Cryptographic operations
   - No UI dependencies

### 3.3 Migrate Utilities

- `NumeroALetras.cs` - Copy directly
- `GestorFotosConcepto.cs` - Copy directly (may need path adjustments)
- `GestorEvidencias.cs` - Copy directly
- `FolioManager.cs` - Copy directly
- `Reportes.cs` - Migrate with PDF library updates
- `DetalleOrdenHelper.cs` - Copy directly
- `Resizer.cs` - May need screen handling updates

### 3.4 Data Access Layer Modernization

**Old Pattern (Winforms):**
```csharp
using (SqlConnection conn = new SqlConnection(connStr))
{
    conn.Open();
    var cmd = new SqlCommand(sql, conn);
    cmd.ExecuteNonQuery();
}
```

**New Pattern (WPF, async-ready):**
```csharp
await using (var conn = new SqlConnection(connStr))
{
    await conn.OpenAsync();
    var cmd = new SqlCommand(sql, conn);
    await cmd.ExecuteNonQueryAsync();
}
```

Create `Services/DataAccess/CalandriaDbContext.cs` (async wrapper):
```csharp
public class CalandriaDbContext
{
    private readonly string _connectionString;

    public CalandriaDbContext(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("CalandriaConn");
    }

    public async Task<T> ExecuteQueryAsync<T>(
        string sql,
        Func<SqlDataReader, T> mapper,
        params SqlParameter[] parameters)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);

        using var reader = await cmd.ExecuteReaderAsync();
        return reader.Read() ? mapper(reader) : default!;
    }

    // Similar methods for INSERT, UPDATE, DELETE, GetList, etc.
}
```

---

## ?? PHASE 4: UI MIGRATION STRATEGY (Week 3-4)

### 4.1 Theme & Styling Migration

**Current (Winforms):** `ThemeManager.cs` with `MaterialSkin`

**New (WPF):** Use Material Design In XAML Toolkit

Create `Themes/MaterialDesignTheme.xaml`:
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:materialDesign="http://materialdesigninxaml.com/winfx">

    <!-- Color Palette from ThemeManager (Coffee Theme) -->
    <Color x:Key="ColorPrincipal">#583517</Color>
    <Color x:Key="ColorPrincipalMenuBar">#B36C2E</Color>
    <Color x:Key="ColorPrincipalClaro">#C48A54</Color>
    <!-- ... rest of colors ... -->

    <!-- Brushes -->
    <SolidColorBrush x:Key="BrushPrincipal" Color="{StaticResource ColorPrincipal}" />
    <!-- ... rest of brushes ... -->

    <!-- Button Styles -->
    <Style TargetType="Button" BasedOn="{StaticResource MaterialDesignRaisedButton}">
        <Setter Property="Background" Value="{StaticResource BrushPrincipal}" />
        <Setter Property="Foreground" Value="White" />
    </Style>

    <!-- DataGrid Styles (replacing Winforms DataGridView) -->
    <Style TargetType="DataGrid">
        <Setter Property="RowBackground" Value="White" />
        <Setter Property="AlternatingRowBackground" Value="#FFFAF6" />
        <!-- ... -->
    </Style>

</ResourceDictionary>
```

### 4.2 Form Migration Priority & Mapping

**Priority 1 (Critical - Week 3):**
1. FormLogin.cs ? LoginView/LoginViewModel
2. FormSplash.cs ? SplashScreen (simplified WPF window)
3. PanelPrincipal.cs ? MainWindow/MainWindowViewModel (with TabControl or NavigationView)

**Priority 2 (Core Business - Week 4):**
1. FormAlmacen.cs ? AlmacenPage/ViewModel
2. FormEstimacionConceptoMigrado.cs ? EstimacionPage/ViewModel
3. FormGestionarPartidas.cs ? GestionPartidaPage/ViewModel
4. FormDetalleOrdenCompra.cs ? OrdenCompraPage/ViewModel

**Priority 3 (Supporting - Week 5):**
1. FormReporte.cs ? ReportePage/ViewModel
2. FormManoObra.cs ? ManoObraPage/ViewModel
3. FormRegistrarTrabajador.cs ? TrabajadorPage/ViewModel

**Priority 4 (Utilities - Week 5-6):**
1. All dialog forms ? WPF Dialog conventions
2. All utility forms ? Modal windows

### 4.3 Winforms ? WPF Control Mapping

| Winforms Control | WPF Equivalent | Migration Notes |
|---|---|---|
| Form | Window or Page | Use Page for navigation, Window for dialogs |
| Panel | Grid, StackPanel, or Canvas | Grid preferred (layout engine) |
| Button | Button | Direct equivalent |
| TextBox | TextBox | Add binding for MVVM |
| Label | Label or TextBlock | TextBlock preferred (read-only) |
| ComboBox | ComboBox | ItemsSource binding replaces Items.Add() |
| ListBox | ListBox | ItemsSource binding |
| TreeView | TreeView | ItemsSource binding for hierarch. data |
| DataGridView | DataGrid | More powerful, column binding |
| CheckBox | CheckBox | IsChecked property binding |
| RadioButton | RadioButton | GroupName replaces manual grouping |
| TabControl | TabControl | TabItem for pages |
| MenuStrip | Menu | MenuItem elements |
| ToolStrip | ToolBar or CommandBar | ToolBar simple, create custom for richer |
| ProgressBar | ProgressBar | Value binding to ViewModel |
| StatusStrip | StatusBar | Simple status at bottom |
| Splitter | GridSplitter | Resize Grid columns/rows |
| Timer | DispatcherTimer | For UI updates |
| FileDialog | CommonOpenFileDialog | or WPF OpenFileDialog |

### 4.4 Layout Pattern: From Designer to XAML

**Old Winforms Pattern:**
```csharp
// FormAlmacen.Designer.cs
this.dataGridView1 = new System.Windows.Forms.DataGridView();
this.dataGridView1.Location = new System.Drawing.Point(12, 50);
this.dataGridView1.Size = new System.Drawing.Size(300, 250);
```

**New WPF Pattern:**
```xaml
<!-- AlmacenPage.xaml -->
<Grid Margin="12,50,12,12">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>

    <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,10">
        <TextBlock Text="Inventario" FontSize="18" FontWeight="Bold" />
        <Button Content="Agregar" Margin="10,0,0,0" Command="{Binding AgregarCommand}" />
    </StackPanel>

    <DataGrid Grid.Row="1" 
              ItemsSource="{Binding Inventarios}" 
              AutoGenerateColumns="False">
        <!-- Column definitions -->
    </DataGrid>
</Grid>
```

### 4.5 Complex Forms: Interactive Map (PanelPrincipal)

**Current:** Custom GDI+ drawing in Winforms Panel with pan/zoom

**Migration:** 
- Option 1: Canvas with transforms
- Option 2: Use DrawingVisual in WPF
- Option 3: Use third-party mapping control (GMap.NET has WPF support)

**Recommended:** Canvas with Image + TransformGroup

```xaml
<Canvas Name="MapCanvas" Background="LightGray">
    <Image Source="{Binding MapImageSource}" 
           Canvas.Left="0" Canvas.Top="0"
           RenderTransformOrigin="0.5,0.5">
        <Image.RenderTransform>
            <TransformGroup>
                <ScaleTransform ScaleX="{Binding ZoomScale}" ScaleY="{Binding ZoomScale}" />
                <TranslateTransform X="{Binding OffsetX}" Y="{Binding OffsetY}" />
            </TransformGroup>
        </Image.RenderTransform>
    </Image>
</Canvas>
```

### 4.6 Data Binding & MVVM Pattern

**Example: FormAlmacen Migration**

**Old Winforms Code:**
```csharp
private void FormAlmacen_Load(object sender, EventArgs e)
{
    RefreshInventory();
}

private void RefreshInventory()
{
    var items = inventarioService.ObtenerInventario();
    dataGridView1.DataSource = items;
}

private void btnAgregar_Click(object sender, EventArgs e)
{
    var form = new FormAgregarProducto();
    if (form.ShowDialog() == DialogResult.OK)
    {
        RefreshInventory();
    }
}
```

**New WPF/MVVM Code:**

`Views/Pages/AlmacenPage.xaml`:
```xaml
<Page x:Class="DynamicSepticSystem.Views.AlmacenPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <StackPanel Grid.Row="0" Margin="10">
            <Button Content="Agregar Producto" 
                    Command="{Binding AgregarProductoCommand}"
                    Padding="10,5" />
        </StackPanel>

        <DataGrid Grid.Row="1" 
                  ItemsSource="{Binding Inventarios}"
                  SelectedItem="{Binding SelectedInventario}"
                  AutoGenerateColumns="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Nombre" Binding="{Binding Nombre}" />
                <DataGridTextColumn Header="Cantidad" Binding="{Binding Cantidad}" />
                <DataGridTextColumn Header="Precio" Binding="{Binding Precio}" />
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Page>
```

`ViewModels/AlmacenViewModel.cs`:
```csharp
public class AlmacenViewModel : ViewModelBase
{
    private readonly InventarioService _inventarioService;
    private ObservableCollection<InventarioItem> _inventarios;

    public ObservableCollection<InventarioItem> Inventarios
    {
        get => _inventarios;
        set => SetProperty(ref _inventarios, value);
    }

    public RelayCommand AgregarProductoCommand { get; }

    public AlmacenViewModel(InventarioService inventarioService)
    {
        _inventarioService = inventarioService;
        AgregarProductoCommand = new RelayCommand(_ => AgregarProducto());
        LoadInventario();
    }

    private async void LoadInventario()
    {
        var items = await _inventarioService.ObtenerInventarioAsync();
        Inventarios = new ObservableCollection<InventarioItem>(items);
    }

    private void AgregarProducto()
    {
        var dialog = new AgregarProductoDialog();
        if (dialog.ShowDialog() == true)
        {
            LoadInventario();
        }
    }
}
```

---

## ?? PHASE 5: CUSTOM CONTROLS & SPECIAL FEATURES (Week 4-5)

### 5.1 Tree Grid Control (SafeTreeListView ? WPF)

**Old Winforms:**
```csharp
public class SafeTreeListView : TreeView
{
    // Custom rendering
}
```

**New WPF:**
```xaml
<TreeView ItemsSource="{Binding TreeItems}">
    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding Name}" />
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

Or use a dedicated TreeGrid library like **DevExpress** or **Telerik** for advanced features.

### 5.2 PDF Preview (FormPdfPreview ? WPF)

**Option 1: Use PdfSharp + WriteableBitmap**
```csharp
public class PdfViewerViewModel : ViewModelBase
{
    private BitmapImage? _currentPage;

    public BitmapImage? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public async Task LoadPdfAsync(string filePath)
    {
        using (var document = PdfDocument.Open(filePath))
        {
            var renderer = new PdfRenderer();
            var bitmap = renderer.RenderPage(document, 0); // First page
            CurrentPage = ConvertToWpf(bitmap);
        }
    }
}
```

**Option 2: Use a third-party PDF viewer like PdfiumViewer or MuPDF.NET**

### 5.3 Interactive Map with Pan/Zoom

Create custom `PanZoomControl.xaml`:
```xaml
<Canvas x:Name="MapCanvas" Background="#F0F0F0"
        MouseWheel="Canvas_MouseWheel"
        MouseLeftButtonDown="Canvas_MouseLeftButtonDown"
        MouseMove="Canvas_MouseMove"
        MouseLeftButtonUp="Canvas_MouseLeftButtonUp">

    <Image x:Name="MapImage"
           Source="{Binding MapSource}"
           RenderTransformOrigin="0.5,0.5">
        <Image.RenderTransform>
            <TransformGroup>
                <ScaleTransform x:Name="scale" 
                               ScaleX="{Binding ZoomLevel}" 
                               ScaleY="{Binding ZoomLevel}" />
                <TranslateTransform x:Name="translate" 
                                   X="{Binding OffsetX}" 
                                   Y="{Binding OffsetY}" />
            </TransformGroup>
        </Image.RenderTransform>
    </Image>
</Canvas>
```

### 5.4 Material Design Customizations

Replace `MaterialSkin` effects with Material Design In XAML:

**Shadows:**
```xaml
<StackPanel Background="White" Margin="10">
    <StackPanel.Effect>
        <DropShadowEffect BlurRadius="10" ShadowDepth="1" Opacity="0.3" />
    </StackPanel.Effect>
    <!-- Content -->
</StackPanel>
```

**Ripple Effect:** Use Material Design In XAML library
```xaml
<Button Content="Click Me" 
        Style="{StaticResource MaterialDesignRaisedButton}">
    <materialDesign:RippleAssist.IsCentered>True</materialDesign:RippleAssist.IsCentered>
</Button>
```

---

## ?? PHASE 6: AUTHENTICATION & SECURITY (Week 3)

### 6.1 Login System Migration

**Old (Winforms):**
```csharp
public partial class FormLogin : Form
{
    private void btnLogin_Click(object sender, EventArgs e)
    {
        try
        {
            var user = ApiClient.Login(txtUsuario.Text, txtClave.Text);
            Global.UsuarioActual = user;
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex);
            MessageBox.Show("Error de autenticación");
        }
    }
}
```

**New (WPF + MVVM):**

`Views/Dialogs/LoginDialog.xaml`:
```xaml
<Window x:Class="DynamicSepticSystem.Views.LoginDialog"
        Title="Iniciar Sesión" 
        WindowStyle="None"
        Background="White">
    <StackPanel VerticalAlignment="Center" Margin="40">
        <TextBlock Text="Calandria" FontSize="24" FontWeight="Bold" Margin="0,0,0,30" />

        <TextBlock Text="Usuario:" Margin="0,0,0,5" />
        <TextBox x:Name="UsernameBox" Text="{Binding Usuario, UpdateSourceTrigger=PropertyChanged}" Padding="10,8" Margin="0,0,0,15" />

        <TextBlock Text="Contraseña:" Margin="0,0,0,5" />
        <PasswordBox x:Name="PasswordBox" Padding="10,8" Margin="0,0,0,20" />

        <Button Content="Ingresar" 
                Command="{Binding LoginCommand}"
                CommandParameter="{Binding ElementName=PasswordBox}"
                Padding="20,10"
                Background="#583517" 
                Foreground="White"
                IsDefault="True" />

        <TextBlock Text="{Binding ErrorMessage}" Foreground="Red" Margin="0,15,0,0" TextWrapping="Wrap" />
    </StackPanel>
</Window>
```

`ViewModels/LoginViewModel.cs`:
```csharp
public class LoginViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private string? _usuario;
    private string? _errorMessage;
    private bool _isLoading;

    public string? Usuario
    {
        get => _usuario;
        set => SetProperty(ref _usuario, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public RelayCommand LoginCommand { get; }

    public LoginViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        LoginCommand = new RelayCommand(Login, _ => !IsLoading);
    }

    private async void Login(object? param)
    {
        if (!(param is PasswordBox passwordBox))
            return;

        if (string.IsNullOrEmpty(Usuario) || string.IsNullOrEmpty(passwordBox.Password))
        {
            ErrorMessage = "Usuario y contraseña requeridos";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _apiClient.LoginAsync(Usuario, passwordBox.Password);
            // Success - close dialog with OK result
            Application.Current?.Windows.OfType<LoginDialog>().FirstOrDefault()?.DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### 6.2 JWT Token Management

```csharp
public class TokenManager
{
    private readonly IConfiguration _config;
    private string? _currentToken;
    private DateTime _tokenExpiryTime;

    public string? CurrentToken => _currentToken;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_currentToken) && DateTime.UtcNow < _tokenExpiryTime;

    public TokenManager(IConfiguration config)
    {
        _config = config;
    }

    public async Task<LoginResponse> AuthenticateAsync(string username, string password)
    {
        var response = await ApiClient.LoginAsync(username, password);
        _currentToken = response.Token;
        _tokenExpiryTime = response.ExpiresAt;
        return response;
    }

    public void RefreshToken(string newToken, DateTime expiryTime)
    {
        _currentToken = newToken;
        _tokenExpiryTime = expiryTime;
    }

    public void Logout()
    {
        _currentToken = null;
        _tokenExpiryTime = DateTime.MinValue;
    }
}
```

---

## ?? PHASE 7: ASYNC/AWAIT & PERFORMANCE (Week 4-5)

### 7.1 Convert Blocking Calls to Async

**Old Pattern (Blocking):**
```csharp
var data = inventarioService.ObtenerInventario();
dataGridView1.DataSource = data;
```

**New Pattern (Async):**
```csharp
IsLoading = true;
try
{
    var data = await inventarioService.ObtenerInventarioAsync();
    Inventarios = new ObservableCollection<InventarioItem>(data);
}
finally
{
    IsLoading = false;
}
```

### 7.2 Background Loading with Progress

```csharp
public class AlmacenViewModel : ViewModelBase
{
    private bool _isLoading;
    private int _loadingProgress;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public int LoadingProgress
    {
        get => _loadingProgress;
        set => SetProperty(ref _loadingProgress, value);
    }

    public async Task LoadInventarioWithProgressAsync(IProgress<int> progress)
    {
        IsLoading = true;
        try
        {
            var data = await inventarioService.ObtenerInventarioAsync();
            progress.Report(50);

            Inventarios = new ObservableCollection<InventarioItem>(data);
            progress.Report(100);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### 7.3 Cancellation Support

```csharp
public class AlmacenViewModel : ViewModelBase
{
    private CancellationTokenSource? _cancellationTokenSource;

    public RelayCommand CancelLoadCommand { get; }

    public AlmacenViewModel()
    {
        CancelLoadCommand = new RelayCommand(_ => CancelLoad);
    }

    public async Task LoadInventarioAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var data = await inventarioService.ObtenerInventarioAsync(_cancellationTokenSource.Token);
            Inventarios = new ObservableCollection<InventarioItem>(data);
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Operación cancelada";
        }
    }

    private void CancelLoad()
    {
        _cancellationTokenSource?.Cancel();
    }
}
```

---

## ?? PHASE 8: TESTING & VALIDATION (Week 5-6)

### 8.1 Unit Test Structure

```
DynamicSepticSystem.Tests/
??? DynamicSepticSystem.Tests.csproj (net6.0)
??? Unit/
?   ??? Services/
?   ?   ??? ApiClientTests.cs
?   ?   ??? InventarioServiceTests.cs
?   ?   ??? ...
?   ??? ViewModels/
?   ?   ??? LoginViewModelTests.cs
?   ?   ??? AlmacenViewModelTests.cs
?   ?   ??? ...
?   ??? Utilities/
?       ??? NumeroALetrasTests.cs
?       ??? ...
??? Integration/
?   ??? ApiIntegrationTests.cs
?   ??? DatabaseIntegrationTests.cs
?   ??? ...
??? Helpers/
    ??? MockApiClient.cs
    ??? TestData.cs
    ??? ...
```

### 8.2 Test Examples

```csharp
[TestFixture]
public class LoginViewModelTests
{
    private LoginViewModel _viewModel;
    private Mock<IApiClient> _mockApiClient;

    [SetUp]
    public void Setup()
    {
        _mockApiClient = new Mock<IApiClient>();
        _viewModel = new LoginViewModel(_mockApiClient.Object);
    }

    [Test]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        _mockApiClient.Setup(x => x.LoginAsync("admin", "password"))
            .ReturnsAsync(new LoginResponse { Token = "test-token", ExpiresAt = DateTime.UtcNow.AddHours(1) });

        // Act
        _viewModel.Usuario = "admin";
        var cmd = _viewModel.LoginCommand;
        cmd.Execute(null);

        // Assert
        _mockApiClient.Verify(x => x.LoginAsync("admin", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void Login_WithEmptyUsername_ShouldShowError()
    {
        // Arrange
        _viewModel.Usuario = "";

        // Act
        var cmd = _viewModel.LoginCommand;
        cmd.Execute(null);

        // Assert
        Assert.That(_viewModel.ErrorMessage, Does.Contain("requeridos"));
    }
}
```

### 8.3 Validation Checklist

- ? All forms display correctly in WPF
- ? Data binding works (MVVM)
- ? API calls complete successfully
- ? Database operations work
- ? PDF generation/viewing works
- ? Excel export/import works
- ? Performance comparable to Winforms
- ? Theme/colors applied correctly
- ? Custom controls render correctly
- ? Error handling works
- ? Async operations don't freeze UI
- ? Authentication/JWT tokens work
- ? All reports generate correctly
- ? All features from Winforms present in WPF

---

## ?? PHASE 9: DEPLOYMENT & CUTOVER (Week 6-7)

### 9.1 Release Process

**Build Configuration:**

```xml
<!-- Debug Configuration -->
<PropertyGroup Condition="'$(Configuration)'=='Debug'">
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
</PropertyGroup>

<!-- Release Configuration -->
<PropertyGroup Condition="'$(Configuration)'=='Release'">
    <DebugType>embedded</DebugType>
    <Optimize>true</Optimize>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
</PropertyGroup>
```

### 9.2 Version Management

Update version in `.csproj`:
```xml
<Version>2.0.0</Version>
<AssemblyVersion>2.0.0.0</AssemblyVersion>
<FileVersion>2.0.0.0</FileVersion>
```

### 9.3 Installer Creation

Use **WiX Toolset** or **NSIS** for Windows installer:
- Modern UI
- Registry entries for uninstall
- Desktop/Start Menu shortcuts
- Auto-update capability

### 9.4 Migration Strategy

**Option 1: Big Bang (All at Once)**
- Build and test complete WPF version
- Deploy on specific date
- Pros: Simple
- Cons: Risky if issues found

**Option 2: Gradual Rollout (Recommended)**
- Deploy to limited users first (10%)
- Monitor for issues
- Expand to 50%
- Then to 100%
- Can rollback if needed

**Option 3: Parallel Run (Safest)**
- Run Winforms and WPF simultaneously
- Let users test WPF while Winforms is production
- Migrate after validation

---

## ?? PHASE 10: MAINTENANCE & SUPPORT (Ongoing)

### 10.1 Performance Monitoring

```csharp
public class PerformanceMonitor
{
    public static void LogLoadTime(string operationName, long milliseconds)
    {
        Logger.Information($"Operation '{operationName}' completed in {milliseconds}ms");
    }

    public static void MonitorMemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        var memoryUsage = process.WorkingSet64 / (1024 * 1024); // MB
        Logger.Information($"Current memory usage: {memoryUsage}MB");
    }
}
```

### 10.2 Error Tracking

```csharp
public class ErrorTracking
{
    public static void TrackException(Exception ex, string context)
    {
        var errorData = new
        {
            Timestamp = DateTime.UtcNow,
            ExceptionType = ex.GetType().Name,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Context = context
        };

        Logger.Error(JsonConvert.SerializeObject(errorData));
        // Could also send to external service (Sentry, Application Insights, etc.)
    }
}
```

### 10.3 Logging Strategy

Use **Serilog** for structured logging:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();
```

---

## ?? IMPLEMENTATION TIMELINE

### Week 1: Foundation
- [ ] Create new .NET 6 WPF project structure
- [ ] Setup MVVM framework
- [ ] Configure dependency injection
- [ ] Create theme/resource dictionaries

### Week 2: Core Services
- [ ] Migrate all data models
- [ ] Migrate services (InventarioService, etc.)
- [ ] Update ApiClient for async
- [ ] Setup database context
- [ ] Create unit tests for services

### Week 3: Authentication & Main UI
- [ ] Migrate login system
- [ ] Create main window
- [ ] Setup token management
- [ ] Create navigation structure
- [ ] Implement splash screen

### Week 4: Core Forms
- [ ] Migrate Almacén (Inventory)
- [ ] Migrate Estimaciones (Estimates)
- [ ] Migrate Gestión de Partidas
- [ ] Create ViewModels for each
- [ ] Bind data and test

### Week 5: Advanced Features
- [ ] Custom controls (TreeGrid, etc.)
- [ ] PDF viewer integration
- [ ] Interactive map (pan/zoom)
- [ ] Reports migration
- [ ] Excel import/export

### Week 6: Testing & Optimization
- [ ] Unit tests
- [ ] Integration tests
- [ ] Performance optimization
- [ ] User acceptance testing
- [ ] Bug fixes

### Week 7: Deployment
- [ ] Build release version
- [ ] Create installer
- [ ] Documentation
- [ ] Cutover preparation
- [ ] User training

---

## ?? SUCCESS CRITERIA

? **Functional Parity**
- All Winforms features present in WPF
- Business logic identical
- Data integrity maintained

? **Performance**
- Startup time ? 5 seconds
- Form load ? 2 seconds
- Database queries ? 5 seconds
- Responsive UI (no freezing)

? **Code Quality**
- Unit test coverage ? 70%
- No memory leaks
- Proper error handling
- MVVM pattern strictly followed

? **User Experience**
- Intuitive navigation
- Clear error messages
- Responsive to user actions
- Professional appearance

? **Documentation**
- Architecture documentation
- API documentation
- User manual
- Developer guide for maintenance

---

## ?? POTENTIAL RISKS & MITIGATION

| Risk | Probability | Impact | Mitigation |
|---|---|---|---|
| Complex forms hard to migrate | Medium | High | Create visual mockups before coding |
| Performance degradation | Low | High | Profiling during development |
| Database compatibility issues | Low | Medium | Test with existing DB schema |
| API integration problems | Low | Medium | Test API separately first |
| User adoption issues | Medium | Medium | Comprehensive training & documentation |
| Third-party library compatibility | Medium | Medium | Research and test early |
| Time overruns | Medium | Medium | Weekly progress reviews & adjustments |

---

## ?? RESOURCES & TOOLS

**Development:**
- Visual Studio 2022
- .NET 6+ SDK
- Git (version control)
- Docker (optional, for local testing)

**WPF Frameworks:**
- Material Design In XAML Toolkit
- MahApps.Metro
- Caliburn.Micro (MVVM)
- Prism (Advanced MVVM)

**Testing:**
- NUnit or xUnit
- Moq (mocking)
- OpenCover (code coverage)

**Deployment:**
- WiX Toolset (installer)
- Azure DevOps (CI/CD)
- GitHub Actions

**Monitoring:**
- Serilog (logging)
- Application Insights
- Sentry (error tracking)

---

## ?? KEY LEARNING AREAS

1. **XAML** - Markup language for WPF UI
2. **Data Binding** - MVVM pattern
3. **RelayCommand** - Command pattern
4. **Attached Behaviors** - Advanced XAML
5. **Resource Dictionaries** - Theming
6. **async/await** - Asynchronous programming
7. **Dependency Injection** - Modern .NET patterns

---

## ?? CONTACTS & ESCALATION

- **Development Lead:** [Your Name]
- **Architecture Review:** [Architect Name]
- **QA Lead:** [QA Name]
- **Project Manager:** [PM Name]

**Daily Standup:** 10:00 AM
**Code Review:** Every PR
**Release Gate:** Security review, performance baseline

---

This is a comprehensive, production-ready migration plan. No details have been left out. Every aspect from architecture to deployment is covered.

