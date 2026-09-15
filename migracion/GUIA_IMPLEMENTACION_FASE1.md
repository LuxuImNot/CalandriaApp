# ??? GUÍA TÉCNICA DE IMPLEMENTACIÓN - Fase 1
## Dynamic Septic System: WinForms ? WPF

---

## ?? FASE 1: CIMIENTOS (Semanas 1-2)

### ? CHECKLIST FASE 1

- [ ] Crear solución y proyectos
- [ ] Instalar paquetes NuGet
- [ ] Configurar App.xaml
- [ ] Setup inyección de dependencias
- [ ] Crear base MVVM (BaseViewModel, RelayCommand)
- [ ] Crear Material Design Themes
- [ ] Verificar compilación

---

## ?? PASO 1: Crear Solución y Proyectos

### 1.1 Estructura de Directorio

```bash
# Crear estructura limpia
mkdir DynamicSepticSystem.NewWPF
cd DynamicSepticSystem.NewWPF
```

### 1.2 Crear Proyectos

#### **Opción A: Con Visual Studio GUI**

1. File ? New ? Solution
2. Elegir "Blank Solution"
3. Nombre: `DynamicSepticSystem`
4. Agregar proyectos:
   - Right-click Solution ? Add ? New Project
   - **DynamicSepticSystem.WPF** ? WPF Application (.NET Framework 4.7.2)
   - **DynamicSepticSystem.Core** ? Class Library (.NET Framework 4.7.2)
   - **DynamicSepticSystem.Data** ? Class Library (.NET Framework 4.7.2)
   - **DynamicSepticSystem.API.Client** ? Class Library (.NET Framework 4.7.2)
   - **DynamicSepticSystem.Reports** ? Class Library (.NET Framework 4.7.2)
   - **Updater.WPF** ? WPF Application (.NET Framework 4.7.2)

#### **Opción B: Con PowerShell/CLI**

```powershell
# En carpeta del proyecto
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
# ... resto
```

### 1.3 Agregar Referencias Entre Proyectos

En Visual Studio:
- **DynamicSepticSystem.WPF** ? Referencia a Core, Data, Reports, API.Client
- **DynamicSepticSystem.Core** ? Referencia a Data (opcional, si Core necesita acceso a repos)
- **DynamicSepticSystem.Data** ? Sin referencias internas

```xml
<!-- En DynamicSepticSystem.WPF.csproj -->
<ItemGroup>
  <ProjectReference Include="..\DynamicSepticSystem.Core\DynamicSepticSystem.Core.csproj" />
  <ProjectReference Include="..\DynamicSepticSystem.Data\DynamicSepticSystem.Data.csproj" />
  <ProjectReference Include="..\DynamicSepticSystem.API.Client\DynamicSepticSystem.API.Client.csproj" />
  <ProjectReference Include="..\DynamicSepticSystem.Reports\DynamicSepticSystem.Reports.csproj" />
</ItemGroup>
```

---

## ?? PASO 2: Instalar Paquetes NuGet

### 2.1 Para DynamicSepticSystem.WPF

```bash
Install-Package MaterialDesignThemes -Version 4.9.0
Install-Package MaterialDesignColors -Version 2.1.4
Install-Package CommunityToolkit.Mvvm -Version 8.2.2
Install-Package Microsoft.Extensions.DependencyInjection -Version 8.0.0
Install-Package Microsoft.Extensions.Configuration -Version 8.0.0
Install-Package Microsoft.Extensions.Configuration.Json -Version 8.0.0
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package Serilog -Version 3.1.1
Install-Package Serilog.Sinks.File -Version 5.0.0
```

### 2.2 Para DynamicSepticSystem.Core

```bash
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package AutoMapper -Version 12.0.1
Install-Package CommunityToolkit.Mvvm -Version 8.2.2
Install-Package Serilog -Version 3.1.1
```

### 2.3 Para DynamicSepticSystem.Data

```bash
Install-Package Dapper -Version 2.0.123
Install-Package System.Data.SqlClient -Version 4.8.5
Install-Package Microsoft.Extensions.Configuration -Version 8.0.0
```

### 2.4 Para DynamicSepticSystem.API.Client

```bash
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package Microsoft.Extensions.Http -Version 8.0.0
```

### 2.5 Para DynamicSepticSystem.Reports

```bash
Install-Package PdfSharp -Version 6.1.0
Install-Package ClosedXML -Version 0.102.1
Install-Package Newtonsoft.Json -Version 13.0.3
```

---

## ?? PASO 3: Configurar App.xaml (Material Design)

### 3.1 Modificar App.xaml

```xml
<Application x:Class="DynamicSepticSystem.WPF.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:DynamicSepticSystem.WPF"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/materialdesign"
             StartupUri="Views/LoginView.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <!-- Material Design Themes -->
            <ResourceDictionary.MergedDictionaries>
                <!-- Material Design -->
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.Indigo.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.DeepOrange.xaml" />

                <!-- Custom Themes -->
                <ResourceDictionary Source="Resources/Themes/CorporateColors.xaml" />
                <ResourceDictionary Source="Resources/Styles/ButtonStyles.xaml" />
                <ResourceDictionary Source="Resources/Styles/TextBoxStyles.xaml" />
                <ResourceDictionary Source="Resources/Styles/DataGridStyles.xaml" />
            </ResourceDictionary.MergedDictionaries>

            <!-- Converters locales -->
            <local:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
            <local:EnumToStringConverter x:Key="EnumToStringConverter"/>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### 3.2 Crear Carpeta de Resources

En `DynamicSepticSystem.WPF` crear:
```
Resources/
  ?? Themes/
  ?  ?? CorporateColors.xaml
  ?? Styles/
  ?  ?? ButtonStyles.xaml
  ?  ?? TextBoxStyles.xaml
  ?  ?? DataGridStyles.xaml
  ?? Converters/
     ?? BoolToVisibilityConverter.cs
     ?? EnumToStringConverter.cs
```

### 3.3 Crear Resources/Themes/CorporateColors.xaml

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/materialdesign">

    <!-- Colores Corporativos -->
    <Color x:Key="CorporateBlue">#003D82</Color>
    <Color x:Key="CorporateOrange">#FF8C42</Color>
    <Color x:Key="CorporateLightBlue">#E8F4F8</Color>
    <Color x:Key="CorporateDarkText">#2C3E50</Color>
    <Color x:Key="CorporateLightText">#ECF0F1</Color>

    <!-- Brushes -->
    <SolidColorBrush x:Key="CorporateBlueBrush" Color="{StaticResource CorporateBlue}"/>
    <SolidColorBrush x:Key="CorporateOrangeBrush" Color="{StaticResource CorporateOrange}"/>
    <SolidColorBrush x:Key="CorporateLightBrush" Color="{StaticResource CorporateLightBlue}"/>

    <!-- Override de Material Design -->
    <SolidColorBrush x:Key="PrimaryHueMidBrush" Color="{StaticResource CorporateBlue}"/>
    <SolidColorBrush x:Key="PrimaryHueLightBrush" Color="{StaticResource CorporateLightBlue}"/>
    <SolidColorBrush x:Key="SecondaryHueMidBrush" Color="{StaticResource CorporateOrange}"/>

</ResourceDictionary>
```

---

## ??? PASO 4: Setup de Inyección de Dependencias

### 4.1 Modificar App.xaml.cs

```csharp
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using DynamicSepticSystem.Core.Services;
using DynamicSepticSystem.Data.Repositories;
using DynamicSepticSystem.API.Client;
using DynamicSepticSystem.WPF.ViewModels;
using DynamicSepticSystem.WPF.Views;

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
                MessageBox.Show($"Error al iniciar: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            (ServiceProvider as IDisposable)?.Dispose();
            base.OnExit(e);
        }
    }
}
```

### 4.2 Crear Extension Methods para Configuración

Crear `DynamicSepticSystem.WPF/ServiceCollectionExtensions.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using DynamicSepticSystem.Core.Services;
using DynamicSepticSystem.Core.Services.Implementations;
using DynamicSepticSystem.Data.Repositories;
using DynamicSepticSystem.API.Client;
using DynamicSepticSystem.WPF.ViewModels;
using DynamicSepticSystem.WPF.Views;

namespace DynamicSepticSystem.WPF
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            return services;
        }

        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
            services.AddScoped(typeof(IRepository<>), typeof(SqlRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IEstimationService, EstimationService>();
            services.AddScoped<IPartidaService, PartidaService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IPhotoService, PhotoService>();

            // API Client
            services.AddScoped<IApiClient, ApiClient.ApiClient>();

            return services;
        }

        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<WarehouseViewModel>();
            services.AddTransient<EstimationViewModel>();
            // ... más ViewModels
            return services;
        }

        public static IServiceCollection AddViews(this IServiceCollection services)
        {
            services.AddTransient<LoginView>();
            services.AddTransient<MainWindow>();
            services.AddTransient<WarehouseView>();
            services.AddTransient<EstimationView>();
            // ... más Views
            return services;
        }
    }
}
```

---

## ?? PASO 5: Crear Base MVVM

### 5.1 BaseViewModel (MVVM Toolkit)

Crear `DynamicSepticSystem.WPF/ViewModels/BaseViewModel.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace DynamicSepticSystem.WPF.ViewModels
{
    /// <summary>
    /// Clase base para todos los ViewModels
    /// Usa CommunityToolkit.Mvvm para INotifyPropertyChanged
    /// </summary>
    public class BaseViewModel : ObservableObject
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public virtual void OnViewLoaded()
        {
            // Override para cargar datos al mostrar la vista
        }

        public virtual void OnViewUnloaded()
        {
            // Override para limpiar recursos
        }

        protected void ShowStatus(string message, int durationMs = 3000)
        {
            StatusMessage = message;
            // Opcionalmente desaparecer después de X ms
        }

        protected void ShowError(string message)
        {
            ErrorMessage = message;
        }

        protected void ClearError()
        {
            ErrorMessage = null;
        }
    }
}
```

### 5.2 RelayCommand

Crear `DynamicSepticSystem.WPF/Commands/RelayCommand.cs`:

```csharp
using System;
using System.Windows.Input;

namespace DynamicSepticSystem.WPF.Commands
{
    /// <summary>
    /// Implementación genérica de ICommand para usar en ViewModels
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(
                _ => execute(),
                _ => canExecute?.Invoke() ?? true)
        {
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _execute(parameter);
        }
    }

    /// <summary>
    /// RelayCommand genérico para pasajes parámetros con tipo
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
```

### 5.3 Converters

Crear `DynamicSepticSystem.WPF/Resources/Converters/BoolToVisibilityConverter.cs`:

```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DynamicSepticSystem.WPF
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }
            return true;
        }
    }
}
```

---

## ?? PASO 6: Crear Interfaces Base en Core

### 6.1 Servicios Base en `DynamicSepticSystem.Core/Services/`

Crear `IAuthenticationService.cs`:

```csharp
using System.Threading.Tasks;

namespace DynamicSepticSystem.Core.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(string usuario, string contraseña);
        Task LogoutAsync();
        Task<bool> RefreshTokenAsync();
        bool IsAuthenticated { get; }
        string CurrentUserName { get; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
    }
}
```

---

## ? PASO 7: Crear appsettings.json

En la raíz de `DynamicSepticSystem.WPF`:

```json
{
  "ConnectionStrings": {
    "CalandriaConn": "Server=YOUR_SERVER;Database=Calandria;User Id=sa;Password=YOUR_PASSWORD;"
  },
  "ApiSettings": {
    "BaseUrl": "http://localhost:8733",
    "ApiVersion": "v1",
    "Timeout": 30
  },
  "Application": {
    "Title": "Dynamic Septic System",
    "Version": "2.0.0",
    "Environment": "Development"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**Importante:** Copiar `appsettings.json` a carpeta output.

En `.csproj`:

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

---

## ?? PASO 8: Verificar Compilación

```bash
# En carpeta raíz de la solución
dotnet build

# Si hay errores, verificar:
# 1. Todas las referencias entre proyectos
# 2. Versiones de paquetes NuGet
# 3. Target Framework (.NET Framework 4.7.2)
```

---

## ?? CHECKLIST COMPLETADO

- [x] Crear solución y proyectos
- [x] Instalar paquetes NuGet
- [x] Configurar App.xaml
- [x] Setup inyección de dependencias
- [x] Crear base MVVM
- [x] Crear Material Design Themes
- [ ] **Siguiente: PASO 9 - LoginView**

---

## ?? PRÓXIMO PASO

Una vez completada la Fase 1, procede con:

**PASO 9: Crear LoginView.xaml + LoginViewModel.cs**

Esto incluirá:
- [ ] Login UI con Material Design
- [ ] Validación de entrada
- [ ] Autenticación (SQL + API)
- [ ] Manejo de errores
- [ ] Navegación a MainWindow

---

## ?? TROUBLESHOOTING

### Error: "Type not found in assembly"
**Solución:** Verificar referencias entre proyectos en `.csproj`

### Error: "Missing dependencies"
**Solución:** Ejecutar `dotnet restore` o instalar paquetes nuevamente

### Error: "XAML no se resuelve"
**Solución:** Reconstruir solución (Clean ? Build)

### MaterialDesignThemes no funciona
**Solución:** Verificar que `App.xaml` incluye ResourceDictionaries correctamente

---

**Fin de FASE 1 - Cimientos**
