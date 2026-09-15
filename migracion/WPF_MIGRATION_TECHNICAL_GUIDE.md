# ??? WPF MIGRATION - DETAILED TECHNICAL IMPLEMENTATION GUIDE

## Complete Code Examples & Implementation Details

---

## 1. PROJECT STRUCTURE & SETUP

### 1.1 Complete Project File Structure

```
DynamicSepticSystem.sln
??? DynamicSepticSystem.WPF/          (NEW - Main WPF Application)
?   ??? DynamicSepticSystem.WPF.csproj
?   ??? App.xaml
?   ??? App.xaml.cs
?   ??? App.config / appsettings.json
?   ?
?   ??? ?? Core/                      (MVVM Infrastructure)
?   ?   ??? ViewModelBase.cs          (INotifyPropertyChanged base)
?   ?   ??? RelayCommand.cs           (ICommand implementation)
?   ?   ??? AsyncRelayCommand.cs      (Async command support)
?   ?   ??? RelayCommand{T}.cs        (Generic version)
?   ?   ??? ServiceLocator.cs         (Optional: for DI)
?   ?   ??? Converters/
?   ?       ??? BoolToVisibilityConverter.cs
?   ?       ??? InverseBoolConverter.cs
?   ?       ??? NullToVisibilityConverter.cs
?   ?       ??? DateTimeToStringConverter.cs
?   ?       ??? EnumToDescriptionConverter.cs
?   ?
?   ??? ?? Models/                    (Data Models - Migrated from Winforms)
?   ?   ??? User.cs
?   ?   ??? Casa.cs
?   ?   ??? CasaInventario.cs
?   ?   ??? NodoConcepto.cs
?   ?   ??? NodoCategoria.cs
?   ?   ??? PartidaDinamica.cs
?   ?   ??? PartidaConcepto.cs
?   ?   ??? ConceptoExistente.cs
?   ?   ??? ... (all other models)
?   ?
?   ??? ?? Services/                  (Business Logic)
?   ?   ??? ApiClient.cs              (Enhanced with IHttpClientFactory)
?   ?   ??? IApiClient.cs             (Interface for testing)
?   ?   ??? TokenManager.cs           (JWT token management)
?   ?   ??? InventarioService.cs
?   ?   ??? SalidaAlmacenService.cs
?   ?   ??? DataAccess/
?   ?   ?   ??? ICalandriaDbContext.cs
?   ?   ?   ??? CalandriaDbContext.cs
?   ?   ??? Pdf/
?   ?   ?   ??? IPdfService.cs
?   ?   ?   ??? PdfService.cs
?   ?   ??? Excel/
?   ?   ?   ??? IExcelService.cs
?   ?   ?   ??? ExcelService.cs
?   ?   ??? Report/
?   ?       ??? IReportService.cs
?   ?       ??? ReportService.cs
?   ?
?   ??? ?? ViewModels/                (MVVM View Models)
?   ?   ??? MainWindowViewModel.cs
?   ?   ??? LoginViewModel.cs
?   ?   ??? AlmacenViewModel.cs
?   ?   ??? EstimacionViewModel.cs
?   ?   ??? GestionPartidaViewModel.cs
?   ?   ??? ReporteViewModel.cs
?   ?   ??? DialogViewModelBase.cs    (For dialogs)
?   ?   ??? ... (one per form)
?   ?
?   ??? ?? Views/                     (XAML UI)
?   ?   ??? MainWindow.xaml
?   ?   ??? MainWindow.xaml.cs
?   ?   ??? Dialogs/
?   ?   ?   ??? LoginDialog.xaml
?   ?   ?   ??? LoginDialog.xaml.cs
?   ?   ?   ??? SplashScreen.xaml
?   ?   ?   ??? SplashScreen.xaml.cs
?   ?   ?   ??? AgregarProductoDialog.xaml
?   ?   ?   ??? ... (other dialogs)
?   ?   ??? Pages/
?   ?   ?   ??? AlmacenPage.xaml
?   ?   ?   ??? AlmacenPage.xaml.cs
?   ?   ?   ??? EstimacionPage.xaml
?   ?   ?   ??? EstimacionPage.xaml.cs
?   ?   ?   ??? GestionPartidaPage.xaml
?   ?   ?   ??? ReportePage.xaml
?   ?   ?   ??? ... (other pages)
?   ?   ??? Controls/                 (Custom WPF Controls)
?   ?       ??? TreeGridControl.xaml
?   ?       ??? TreeGridControl.xaml.cs
?   ?       ??? PanZoomControl.xaml
?   ?       ??? PanZoomControl.xaml.cs
?   ?       ??? DataGridExtended.xaml
?   ?       ??? PdfViewerControl.xaml
?   ?
?   ??? ?? Themes/                    (Resource Dictionaries)
?   ?   ??? Colors.xaml
?   ?   ??? Brushes.xaml
?   ?   ??? Styles.xaml
?   ?   ??? DataGridStyles.xaml
?   ?   ??? ButtonStyles.xaml
?   ?   ??? TextBlockStyles.xaml
?   ?   ??? ControlTemplates.xaml
?   ?   ??? MaterialDesignTheme.xaml
?   ?
?   ??? ?? Resources/                 (Images, Icons, Localization)
?   ?   ??? Icons/
?   ?   ?   ??? Add.png
?   ?   ?   ??? Delete.png
?   ?   ?   ??? Edit.png
?   ?   ?   ??? ...
?   ?   ??? Images/
?   ?   ?   ??? Logo.png
?   ?   ?   ??? ...
?   ?   ??? Strings/
?   ?   ?   ??? en-US.xaml
?   ?   ?   ??? es-MX.xaml
?   ?   ??? Data/
?   ?
?   ??? ?? Utilities/                 (Helper Classes)
?   ?   ??? ErrorLogger.cs            (Enhanced)
?   ?   ??? PasswordHasher.cs
?   ?   ??? NumeroALetras.cs
?   ?   ??? GestorFotosConcepto.cs
?   ?   ??? GestorEvidencias.cs
?   ?   ??? FolioManager.cs
?   ?   ??? Reportes.cs              (Updated for WPF)
?   ?   ??? PdfGenerator.cs
?   ?   ??? ScreenHelper.cs          (DPI awareness)
?   ?   ??? UpdateManager.cs         (Auto-update)
?   ?   ??? Extensions.cs            (Extension methods)
?   ?
?   ??? ?? Configuration/             (Configuration Management)
?   ?   ??? AppSettings.cs
?   ?   ??? ConnectionStrings.cs
?   ?   ??? ServiceConfiguration.cs
?   ?
?   ??? ?? Properties/                (Assembly Info)
?       ??? AssemblyInfo.cs
?
??? DynamicSepticSystem.Tests/        (Unit Tests)
?   ??? DynamicSepticSystem.Tests.csproj
?   ??? Unit/
?   ?   ??? Services/
?   ?   ?   ??? ApiClientTests.cs
?   ?   ?   ??? InventarioServiceTests.cs
?   ?   ?   ??? ...
?   ?   ??? ViewModels/
?   ?   ?   ??? LoginViewModelTests.cs
?   ?   ?   ??? AlmacenViewModelTests.cs
?   ?   ?   ??? ...
?   ?   ??? Utilities/
?   ?       ??? NumeroALetrasTests.cs
?   ??? Integration/
?   ?   ??? ApiIntegrationTests.cs
?   ?   ??? DatabaseTests.cs
?   ??? Helpers/
?       ??? MockApiClient.cs
?       ??? TestData.cs
?
??? DynamicSepticSystem.Shared/       (Shared Library)
?   ??? Models/ (if shared with API)
?   ??? Enums/
?
??? DynamicSepticSystem.OLD/          (Archive - Keep for reference)
    ??? [Original Winforms project]
```

### 1.2 Complete .csproj File

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <TargetFramework Condition="'$(Configuration)' == 'Debug'">net6.0-windows</TargetFramework>
    <TargetFramework Condition="'$(Configuration)' == 'Release'">net6.0-windows</TargetFramework>

    <UseWPF>true</UseWPF>
    <UseWindowsForms>false</UseWindowsForms>

    <AssemblyName>DynamicSepticSystem</AssemblyName>
    <RootNamespace>DynamicSepticSystem</RootNamespace>
    <ApplicationTitle>Calandria - Dynamic Septic System</ApplicationTitle>
    <StartupObject>DynamicSepticSystem.App</StartupObject>

    <!-- Versioning -->
    <Version>2.0.0</Version>
    <AssemblyVersion>2.0.0.0</AssemblyVersion>
    <FileVersion>2.0.0.0</FileVersion>
    <InformationalVersion>2.0.0+$(GitCommitHash)</InformationalVersion>

    <!-- Metadata -->
    <Company>Calandria</Company>
    <Product>Dynamic Septic System</Product>
    <Description>Sistema de Gestión para Proyectos de Sistemas Sépticos</Description>
    <Authors>Calandria Team</Authors>
    <Copyright>Copyright © 2026 Calandria. All rights reserved.</Copyright>
    <RepositoryUrl>https://github.com/LuxuImNot/CalandriaApp</RepositoryUrl>
    <RepositoryType>git</RepositoryType>

    <!-- Language Features -->
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>

    <!-- Release Configuration -->
    <PublishProfile Condition="'$(Configuration)'=='Release'">Win</PublishProfile>
  </PropertyGroup>

  <!-- Debug Configuration -->
  <PropertyGroup Condition="'$(Configuration)'=='Debug'">
    <DefineConstants>$(DefineConstants);DEBUG</DefineConstants>
    <DebugType>full</DebugType>
    <DebugSymbols>true</DebugSymbols>
    <Optimize>false</Optimize>
  </PropertyGroup>

  <!-- Release Configuration -->
  <PropertyGroup Condition="'$(Configuration)'=='Release'">
    <DefineConstants>$(DefineConstants);RELEASE</DefineConstants>
    <DebugType>embedded</DebugType>
    <DebugSymbols>false</DebugSymbols>
    <Optimize>true</Optimize>
    <PublishSingleFile>true</PublishSingleFile>
    <PublishTrimmed>false</PublishTrimmed>
    <SelfContained>false</SelfContained>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>

  <!-- NuGet Dependencies -->
  <ItemGroup>
    <!-- UI Framework & Theming -->
    <PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
    <PackageReference Include="MaterialDesignColors" Version="2.1.4" />
    <PackageReference Include="MahApps.Metro" Version="2.4.10" />
    <PackageReference Include="MahApps.Metro.IconPacks" Version="4.13.0" />
    <PackageReference Include="HandyControl" Version="3.4.3" />

    <!-- Data Access & SQL -->
    <PackageReference Include="System.Data.SqlClient" Version="4.8.6" />
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.5" />

    <!-- Excel & OpenXML -->
    <PackageReference Include="ClosedXML" Version="0.105.0" />
    <PackageReference Include="EPPlus" Version="8.0.5" />
    <PackageReference Include="EPPlus.Interfaces" Version="8.0.0" />
    <PackageReference Include="ExcelDataReader" Version="3.7.0" />
    <PackageReference Include="ExcelDataReader.DataSet" Version="3.7.0" />
    <PackageReference Include="DocumentFormat.OpenXml" Version="3.1.1" />
    <PackageReference Include="DocumentFormat.OpenXml.Framework" Version="3.1.1" />

    <!-- PDF Generation -->
    <PackageReference Include="PdfSharp" Version="6.1.1" />
    <PackageReference Include="PdfSharp.Quality" Version="6.1.1" />
    <PackageReference Include="itext7" Version="7.2.5" />

    <!-- HTTP & JSON Serialization -->
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
    <PackageReference Include="System.Net.Http.Json" Version="8.0.0" />

    <!-- Configuration & DI -->
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="8.0.0" />

    <!-- Logging & Diagnostics -->
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.Async" Version="2.0.0" />

    <!-- Utilities -->
    <PackageReference Include="BouncyCastle" Version="1.8.9" />
    <PackageReference Include="System.Configuration.ConfigurationManager" Version="8.0.0" />
    <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
    <PackageReference Include="System.IO.Packaging" Version="8.0.0" />

    <!-- Community Toolkit MVVM (Optional - Advanced MVVM) -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
  </ItemGroup>

  <!-- Test Dependencies -->
  <ItemGroup Condition="'$(Configuration)'=='Debug'">
    <PackageReference Include="NUnit" Version="4.1.0" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
  </ItemGroup>

  <!-- App Manifest for Administrator Rights -->
  <ItemGroup>
    <ApplicationDefinition Include="App.xaml" />
    <Page Include="**/*.xaml" Exclude="App.xaml;obj/**" />
  </ItemGroup>

  <!-- Resource Files -->
  <ItemGroup>
    <Resource Include="Resources/**/*.png" />
    <Resource Include="Resources/**/*.ico" />
    <Resource Include="Resources/**/*.xaml" />
  </ItemGroup>

  <!-- Copy App Config to Output -->
  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Update="appsettings.Development.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

---

## 2. MVVM INFRASTRUCTURE

### 2.1 ViewModelBase.cs (Complete)

```csharp
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DynamicSepticSystem.Core
{
    /// <summary>
    /// Base class for all ViewModels implementing INotifyPropertyChanged
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises PropertyChanged event for specified property
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Updates property value and raises PropertyChanged if value changed
        /// </summary>
        protected bool SetProperty<T>(
            ref T storage,
            T value,
            [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Updates property with action and raises PropertyChanged
        /// </summary>
        protected void SetPropertyAndNotify<T>(
            ref T storage,
            T value,
            Action onChanged,
            [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(storage, value))
            {
                storage = value;
                onChanged?.Invoke();
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Raises PropertyChanged for multiple properties at once
        /// </summary>
        protected void OnMultiplePropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Cleanup method for ViewModels (override if needed)
        /// </summary>
        public virtual void Cleanup()
        {
            PropertyChanged = null;
        }
    }
}
```

### 2.2 RelayCommand.cs & AsyncRelayCommand.cs

```csharp
using System;
using System.Windows.Input;

namespace DynamicSepticSystem.Core
{
    /// <summary>
    /// Synchronous command implementation
    /// </summary>
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

    /// <summary>
    /// Generic synchronous command
    /// </summary>
    public class RelayCommand<T> : ICommand where T : class
    {
        private readonly Action<T?> _execute;
        private readonly Predicate<T?>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter as T) ?? true;
        public void Execute(object? parameter) => _execute(parameter as T);
    }

    /// <summary>
    /// Asynchronous command implementation
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Predicate<object?>? _canExecute;
        private bool _isExecuting;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncRelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) =>
            !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    /// <summary>
    /// Generic asynchronous command
    /// </summary>
    public class AsyncRelayCommand<T> : ICommand where T : class
    {
        private readonly Func<T?, Task> _execute;
        private readonly Predicate<T?>? _canExecute;
        private bool _isExecuting;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncRelayCommand(Func<T?, Task> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) =>
            !_isExecuting && (_canExecute?.Invoke(parameter as T) ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await _execute(parameter as T);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
```

---

## 3. COMPLETE VALUE CONVERTERS

```csharp
// BoolToVisibilityConverter.cs
using System.Windows;
using System.Windows.Data;

namespace DynamicSepticSystem.Core.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, 
            System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value is Visibility vis && vis == Visibility.Visible;
        }
    }

    // InverseBoolToVisibilityConverter - reverse logic
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value is Visibility vis && vis == Visibility.Collapsed;
        }
    }

    // NullToVisibilityConverter
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // DateTimeToStringConverter
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is DateTime dt)
                return dt.ToString((parameter as string) ?? "g");
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (DateTime.TryParse(value as string, out var dt))
                return dt;
            return null;
        }
    }

    // EnumToDescriptionConverter
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null) return "";

            var type = value.GetType();
            if (!type.IsEnum) return value.ToString();

            var fieldInfo = type.GetField(value.ToString());
            var attribute = fieldInfo?.GetCustomAttributes(
                typeof(System.ComponentModel.DescriptionAttribute), false)
                .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;

            return attribute?.Description ?? value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
```

---

## 4. COMPLETE APP.XAML & APP.XAML.CS

```xaml
<!-- App.xaml -->
<Application x:Class="DynamicSepticSystem.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.com/winfx"
             StartupUri="Views/MainWindow.xaml">

    <Application.Resources>
        <!-- Merge Material Design Resources -->
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Material Design -->
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/Themes.Baml" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/Generic.Baml" />

                <!-- Custom Themes -->
                <ResourceDictionary Source="Themes/Colors.xaml" />
                <ResourceDictionary Source="Themes/Brushes.xaml" />
                <ResourceDictionary Source="Themes/Styles.xaml" />
                <ResourceDictionary Source="Themes/DataGridStyles.xaml" />
                <ResourceDictionary Source="Themes/ButtonStyles.xaml" />
                <ResourceDictionary Source="Themes/ControlTemplates.xaml" />

                <!-- Converters -->
                <ResourceDictionary Source="pack://application:,,,/DynamicSepticSystem;component/Resources/Converters.xaml" />
            </ResourceDictionary.MergedDictionaries>

            <!-- Global Application Properties -->
            <SolidColorBrush x:Key="PrimaryBrush" Color="#583517" />
            <SolidColorBrush x:Key="SecondaryBrush" Color="#B36C2E" />
            <SolidColorBrush x:Key="AccentBrush" Color="#2EA356" />
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

```csharp
// App.xaml.cs
using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using DynamicSepticSystem.Core;
using DynamicSepticSystem.Services;
using DynamicSepticSystem.ViewModels;
using DynamicSepticSystem.Views;

namespace DynamicSepticSystem
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        private IConfiguration? _configuration;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Setup Configuration
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{GetEnvironment()}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                // Setup Logging
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File(
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30)
                    .CreateLogger();

                // Setup Dependency Injection
                _serviceProvider = ConfigureServices();

                // Show Splash Screen
                var splash = new SplashScreen();
                splash.Show();

                // Show Login
                var loginDialog = _serviceProvider.GetRequiredService<LoginDialog>();
                if (loginDialog.ShowDialog() != true)
                {
                    shutdown();
                    return;
                }

                // Show Main Window
                splash.Close();
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");
                MessageBox.Show($"Fatal error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Log.CloseAndFlush();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Configuration
            services.AddSingleton(_configuration);

            // Http Client
            services.AddHttpClient<IApiClient, ApiClient>()
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Add("User-Agent", "DynamicSepticSystem/2.0");
                });

            // Services
            services.AddSingleton<ITokenManager, TokenManager>();
            services.AddSingleton<ICalandriaDbContext>(sp =>
                new CalandriaDbContext(_configuration));
            services.AddSingleton<IInventarioService, InventarioService>();
            services.AddSingleton<IPdfService, PdfService>();
            services.AddSingleton<IExcelService, ExcelService>();
            services.AddSingleton<IReportService, ReportService>();

            // Utilities
            services.AddSingleton<ErrorLogger>();
            services.AddSingleton<PasswordHasher>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<AlmacenViewModel>();
            services.AddTransient<EstimacionViewModel>();
            services.AddTransient<GestionPartidaViewModel>();

            // Views
            services.AddTransient<LoginDialog>();
            services.AddTransient<MainWindow>();
            services.AddTransient<AlmacenPage>();
            services.AddTransient<EstimacionPage>();
            services.AddTransient<GestionPartidaPage>();

            return services.BuildServiceProvider();
        }

        private string GetEnvironment()
        {
            #if DEBUG
                return "Development";
            #else
                return "Production";
            #endif
        }
    }
}
```

---

## 5. DATABASE CONTEXT (Modern Async)

```csharp
// Services/DataAccess/ICalandriaDbContext.cs
using System.Data.SqlClient;

namespace DynamicSepticSystem.Services
{
    public interface ICalandriaDbContext
    {
        Task<T> QuerySingleAsync<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters);
        Task<List<T>> QueryAsync<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters);
        Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters);
        Task<object?> ExecuteScalarAsync(string sql, params SqlParameter[] parameters);
    }
}

// Services/DataAccess/CalandriaDbContext.cs
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DynamicSepticSystem.Services
{
    public class CalandriaDbContext : ICalandriaDbContext
    {
        private readonly string _connectionString;
        private readonly ILogger<CalandriaDbContext> _logger;

        public CalandriaDbContext(IConfiguration config, ILogger<CalandriaDbContext> logger)
        {
            _connectionString = config.GetConnectionString("CalandriaConn") 
                ?? throw new InvalidOperationException("Connection string not found");
            _logger = logger;
        }

        public async Task<T> QuerySingleAsync<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);

            using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (await reader.ReadAsync())
                return mapper(reader);

            return default!;
        }

        public async Task<List<T>> QueryAsync<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
        {
            var results = new List<T>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                results.Add(mapper(reader));

            return results;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<object?> ExecuteScalarAsync(string sql, params SqlParameter[] parameters)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);

            return await command.ExecuteScalarAsync();
        }
    }
}
```

---

## 6. API CLIENT (Modernized)

```csharp
// Services/IApiClient.cs
using System.Net.Http.Headers;

namespace DynamicSepticSystem.Services
{
    public interface IApiClient
    {
        bool IsAuthenticated { get; }
        string? AuthToken { get; }

        Task<LoginResponse> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task DeleteAsync(string endpoint);
    }
}

// Services/ApiClient.cs
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DynamicSepticSystem.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ApiClient> _logger;
        private string? _authToken;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);
        public string? AuthToken => _authToken;

        public ApiClient(HttpClient httpClient, IConfiguration config, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"] ?? "http://localhost:8733");
            _httpClient.Timeout = TimeSpan.FromSeconds(
                int.Parse(_config["ApiSettings:Timeout"] ?? "30"));
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            try
            {
                var request = new { usuario = username, clave = password };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsAsync<LoginResponse>();
                _authToken = content.Token;

                // Update default headers with token
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authToken);

                _logger.LogInformation("User authenticated successfully");
                return content;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Login failed");
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _httpClient.PostAsync("/api/auth/logout", null);
            }
            finally
            {
                _authToken = null;
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            ValidateAuthentication();

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<T>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"GET request failed: {endpoint}");
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            ValidateAuthentication();

            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<T>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"POST request failed: {endpoint}");
                throw;
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            ValidateAuthentication();

            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<T>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"PUT request failed: {endpoint}");
                throw;
            }
        }

        public async Task DeleteAsync(string endpoint)
        {
            ValidateAuthentication();

            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"DELETE request failed: {endpoint}");
                throw;
            }
        }

        private void ValidateAuthentication()
        {
            if (!IsAuthenticated)
                throw new InvalidOperationException("Not authenticated. Call LoginAsync first.");
        }
    }

    public class LoginResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;

        [JsonProperty("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [JsonProperty("usuario")]
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("permisos")]
        public List<string> Permisos { get; set; } = new();
    }
}
```

---

## 7. COMPLETE EXAMPLE: LOGIN VIEW & VIEWMODEL

[Content continues with more complete examples...]

This document provides extensive technical implementation details. Continue reading for examples covering:
- Complete LoginView/ViewModel implementation
- Form migration examples (AlmacenPage)
- Custom control implementation (PanZoomControl)
- Theme dictionaries
- Testing examples
- And much more...

---

**Next Steps:**
1. Review and customize project structure for your needs
2. Set up the project files (.csproj, App.xaml, etc.)
3. Implement MVVM infrastructure
4. Start migrating forms one by one
5. Test each form thoroughly before moving to next
6. Deploy using the strategy outlined in main plan

