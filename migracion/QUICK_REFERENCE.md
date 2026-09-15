# ?? QUICK REFERENCE - Comandos y Snippets
## Dynamic Septic System - Migración WPF

---

## ?? ÍNDICE RÁPIDO

1. [Setup Inicial](#setup-inicial)
2. [Crear Estructura](#crear-estructura)
3. [Snippets MVVM](#snippets-mvvm)
4. [Configuración DI](#configuración-di)
5. [XAML Patterns](#xaml-patterns)
6. [Testing Snippets](#testing-snippets)
7. [Commands Útiles](#commands-útiles)

---

## ?? SETUP INICIAL

### 1. Crear solución desde cero

```bash
# PowerShell
cd C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\ProgramC#

mkdir DynamicSepticSystem.WPF.New
cd DynamicSepticSystem.WPF.New

# Crear solución
dotnet new sln -n DynamicSepticSystem

# Crear proyectos
dotnet new wpf -n DynamicSepticSystem.WPF -f net472
dotnet new classlib -n DynamicSepticSystem.Core -f net472
dotnet new classlib -n DynamicSepticSystem.Data -f net472
dotnet new classlib -n DynamicSepticSystem.API.Client -f net472
dotnet new classlib -n DynamicSepticSystem.Reports -f net472
dotnet new wpf -n Updater.WPF -f net472

# Agregar a solución
dotnet sln DynamicSepticSystem.sln add DynamicSepticSystem.WPF/DynamicSepticSystem.WPF.csproj
dotnet sln DynamicSepticSystem.sln add DynamicSepticSystem.Core/DynamicSepticSystem.Core.csproj
dotnet sln DynamicSepticSystem.sln add DynamicSepticSystem.Data/DynamicSepticSystem.Data.csproj
dotnet sln DynamicSepticSystem.sln add DynamicSepticSystem.API.Client/DynamicSepticSystem.API.Client.csproj
dotnet sln DynamicSepticSystem.sln add DynamicSepticSystem.Reports/DynamicSepticSystem.Reports.csproj
dotnet sln DynamicSepticSystem.sln add Updater.WPF/Updater.WPF.csproj
```

### 2. Instalar paquetes NuGet

```bash
# En carpeta raíz (ejecutar 6 veces, uno por proyecto)

# DynamicSepticSystem.WPF
cd DynamicSepticSystem.WPF
dotnet package add MaterialDesignThemes
dotnet package add MaterialDesignColors
dotnet package add CommunityToolkit.Mvvm
dotnet package add Microsoft.Extensions.DependencyInjection
dotnet package add Microsoft.Extensions.Configuration
dotnet package add Microsoft.Extensions.Configuration.Json
dotnet package add Newtonsoft.Json
dotnet package add Serilog
dotnet package add Serilog.Sinks.File
cd ..

# DynamicSepticSystem.Core
cd DynamicSepticSystem.Core
dotnet package add Newtonsoft.Json
dotnet package add AutoMapper
dotnet package add CommunityToolkit.Mvvm
dotnet package add Serilog
cd ..

# DynamicSepticSystem.Data
cd DynamicSepticSystem.Data
dotnet package add Dapper
dotnet package add System.Data.SqlClient
dotnet package add Microsoft.Extensions.Configuration
cd ..

# DynamicSepticSystem.API.Client
cd DynamicSepticSystem.API.Client
dotnet package add Newtonsoft.Json
cd ..

# DynamicSepticSystem.Reports
cd DynamicSepticSystem.Reports
dotnet package add PdfSharp
dotnet package add ClosedXML
dotnet package add Newtonsoft.Json
cd ..
```

---

## ?? CREAR ESTRUCTURA

### Carpetas en DynamicSepticSystem.WPF

```bash
mkdir Resources\Themes
mkdir Resources\Styles
mkdir Resources\Converters
mkdir Views
mkdir ViewModels
mkdir Models
mkdir Commands
mkdir Behaviors
mkdir Helpers
```

### Carpetas en DynamicSepticSystem.Core

```bash
mkdir Services
mkdir Models
mkdir Config
mkdir Helpers
```

### Carpetas en DynamicSepticSystem.Data

```bash
mkdir Repositories
mkdir Database
mkdir Queries
mkdir Migrations
```

---

## ?? SNIPPETS MVVM

### 1. ViewModel Base (Simple)

```csharp
using System;
using System.Windows.Input;

namespace DynamicSepticSystem.WPF.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}
```

### 2. ViewModel con MVVM Toolkit

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class WarehouseViewModel : ObservableObject
{
    private readonly IWarehouseService _service;

    [ObservableProperty]
    private ObservableCollection<Almacen> items;

    [ObservableProperty]
    private bool isLoading;

    public WarehouseViewModel(IWarehouseService service)
    {
        _service = service;
    }

    [RelayCommand]
    public async Task Load()
    {
        IsLoading = true;
        try
        {
            var data = await _service.GetAllAsync();
            Items = new ObservableCollection<Almacen>(data);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### 3. RelayCommand

```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object parameter) => _execute?.Invoke(parameter);
}
```

---

## ?? CONFIGURACIÓN DI

### App.xaml.cs Completo

```csharp
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DynamicSepticSystem.WPF
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            ServiceProvider = new ServiceCollection()
                .AddConfiguration()
                .AddDataServices()
                .AddBusinessServices()
                .AddViewModels()
                .AddViews()
                .BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                var loginView = ServiceProvider.GetRequiredService<LoginView>();
                loginView.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                Shutdown();
            }
        }
    }
}
```

### ServiceCollectionExtensions

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        return services.AddSingleton<IConfiguration>(config);
    }

    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IDbConnection>(
            sp => new SqlConnection(
                sp.GetRequiredService<IConfiguration>()
                    .GetConnectionString("CalandriaConn")));

        services.AddScoped(typeof(IRepository<>), typeof(SqlRepository<>));
        return services;
    }

    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        return services;
    }

    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        return services;
    }

    public static IServiceCollection AddViews(this IServiceCollection services)
    {
        services.AddTransient<LoginView>();
        services.AddTransient<MainWindow>();
        return services;
    }
}
```

---

## ?? XAML PATTERNS

### 1. View con DataContext en Code-behind

```xaml
<Window x:Class="DynamicSepticSystem.WPF.Views.WarehouseView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/materialdesign"
        TextElement.Foreground="{DynamicResource MaterialDesignBody}"
        TextElement.FontWeight="Regular"
        Background="{DynamicResource MaterialDesignPaper}"
        Title="Warehouse" Height="600" Width="800">

    <Grid>
        <!-- Content aquí -->
    </Grid>
</Window>
```

```csharp
public partial class WarehouseView : Window
{
    public WarehouseView(WarehouseViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

### 2. DataGrid con virtualización

```xaml
<DataGrid ItemsSource="{Binding Items}"
          VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling"
          EnableRowVirtualization="True"
          AutoGenerateColumns="False">

    <DataGrid.Columns>
        <DataGridTextColumn Header="Nombre" Binding="{Binding Nombre}"/>
        <DataGridTextColumn Header="Ubicación" Binding="{Binding Ubicacion}"/>
        <DataGridTextColumn Header="Capacidad" Binding="{Binding Capacidad}"/>
    </DataGrid.Columns>
</DataGrid>
```

### 3. Binding con Converter

```xaml
<!-- En App.xaml -->
<Application.Resources>
    <local:BoolToVisibilityConverter x:Key="BoolToVisConverter"/>
</Application.Resources>

<!-- En View -->
<TextBlock Text="{Binding ErrorMessage}"
           Visibility="{Binding HasError, 
                               Converter={StaticResource BoolToVisConverter}}"/>
```

### 4. Input Validation

```xaml
<TextBox Text="{Binding ItemName, 
                        UpdateSourceTrigger=PropertyChanged,
                        ValidatesOnDataErrors=True,
                        ValidatesOnExceptions=True}"
         Validation.ErrorTemplate="{StaticResource ErrorTemplate}"/>
```

### 5. Material Design Button

```xaml
<Button Content="Guardar"
        Style="{StaticResource MaterialDesignRaisedButton}"
        Background="{StaticResource PrimaryHueMidBrush}"
        Foreground="White"
        Command="{Binding SaveCommand}"
        IsEnabled="{Binding IsSaving, Converter={StaticResource InvertBoolConverter}}"/>
```

---

## ?? TESTING SNIPPETS

### 1. Unit Test ViewModel

```csharp
[TestClass]
public class WarehouseViewModelTests
{
    [TestMethod]
    public async Task LoadCommand_Should_PopulateItems()
    {
        // Arrange
        var mockService = new Mock<IWarehouseService>();
        var items = new List<Almacen>
        {
            new Almacen { Id = 1, Nombre = "Almacén 1" },
            new Almacen { Id = 2, Nombre = "Almacén 2" }
        };
        mockService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(items);

        var vm = new WarehouseViewModel(mockService.Object);

        // Act
        await vm.LoadCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(2, vm.Items.Count);
        mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }
}
```

### 2. Mock Service

```csharp
public class MockWarehouseService : IWarehouseService
{
    private readonly List<Almacen> _items = new();

    public Task<List<Almacen>> GetAllAsync()
    {
        return Task.FromResult(_items);
    }

    public Task SaveAsync(Almacen almacen)
    {
        _items.Add(almacen);
        return Task.CompletedTask;
    }

    // ... otros métodos
}
```

---

## ?? COMMANDS ÚTILES

### Compilar

```bash
dotnet build                           # Build debug
dotnet build -c Release                # Build release
dotnet build DynamicSepticSystem.WPF   # Build proyecto específico
```

### Ejecutar

```bash
dotnet run --project DynamicSepticSystem.WPF
```

### Limpiar

```bash
dotnet clean                           # Limpiar todo
dotnet clean DynamicSepticSystem.WPF   # Limpiar proyecto
```

### Testing

```bash
dotnet test                            # Ejecutar todos los tests
dotnet test --verbosity:detailed       # Tests con detalles
dotnet test --filter "WarehouseTests"  # Tests específicos
```

### NuGet

```bash
dotnet package add <package>           # Instalar paquete
dotnet package update                  # Actualizar todos
dotnet package search <term>           # Buscar paquete
dotnet package remove <package>        # Desinstalar
```

### Git

```bash
git init                               # Inicializar repo
git add .                              # Agregar archivos
git commit -m "Initial commit"         # Commit
git push origin main                   # Push
git branch -a                          # Ver branches
git checkout -b feature/almacen        # Nueva rama
```

---

## ?? CHECKLIST RÁPIDA

### Semana 1 - Setup

- [ ] Crear solución y proyectos
- [ ] Instalar NuGet packages
- [ ] Configurar App.xaml (Material Design)
- [ ] Crear BaseViewModel y RelayCommand
- [ ] Setup DI en App.xaml.cs
- [ ] Crear appsettings.json
- [ ] Build exitoso

### Semana 2 - Base MVVM

- [ ] Crear interfaces de servicios en Core
- [ ] Crear interfaces de repositories en Data
- [ ] Crear LoginView.xaml
- [ ] Crear LoginViewModel.cs
- [ ] Crear converters básicos
- [ ] Primer test unitario
- [ ] Build + Tests exitosos

### Semana 3 - Autenticación

- [ ] Implementar AuthenticationService
- [ ] Integrar API Client
- [ ] Integrar SQL login
- [ ] Token management
- [ ] Logout functionality
- [ ] Error handling
- [ ] Testing de flows

### Semana 4 - Dashboard

- [ ] Crear MainWindow.xaml
- [ ] MainWindowViewModel
- [ ] Sistema de navegación
- [ ] Menú dinámico
- [ ] Temas aplicados
- [ ] Tests de navegación

---

## ?? QUICK COMMANDS

```bash
# Ir a carpeta proyecto
cd .\DynamicSepticSystem.WPF\

# Build rápido
dotnet build

# Limpiar + Build
dotnet clean && dotnet build

# Build release
dotnet build -c Release

# Run
dotnet run

# Crear nueva clase
New-Item -ItemType File -Name "MyViewModel.cs"

# Buscar archivo
Get-ChildItem -Recurse -Filter "*ViewModel*"

# Contar líneas de código
(Get-ChildItem -Recurse -Filter "*.cs" | Measure-Object -Line).Lines
```

---

## ?? TROUBLESHOOTING RÁPIDO

| Problema | Solución |
|----------|----------|
| XAML no se resuelve | Clean ? Rebuild |
| Material Design no funciona | Verificar App.xaml ResourceDictionaries |
| DI no encuentra servicio | Verificar ServiceCollectionExtensions |
| Binding no funciona | Verificar DataContext en code-behind |
| UI congelada | Convertir a async/await |
| Memory leak | Implementar IDisposable |

---

**Este documento es tu referencia rápida durante desarrollo. ¡Usa Ctrl+F para buscar!**
