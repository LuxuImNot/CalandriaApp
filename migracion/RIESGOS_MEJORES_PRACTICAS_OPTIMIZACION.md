# ?? RIESGOS, MEJORES PRÁCTICAS Y OPTIMIZACIÓN
## Dynamic Septic System - Migración WinForms ? WPF

---

## ?? PARTE 1: ANÁLISIS DE RIESGOS

### 1.1 Riesgos Técnicos

| # | Riesgo | Probabilidad | Impacto | Severidad | Mitigación |
|---|--------|------------|--------|-----------|-----------|
| 1 | Performance peor en WPF | ?? Media (50%) | ?? Alto | ?? CRÍTICO | Profiling temprano, virtualización DataGrid, caching |
| 2 | TreeListView ? TreeView (complejo) | ?? Media (40%) | ?? Alto | ?? CRÍTICO | Prototipo Week 1, considerar librería HierarchicalDataGrid |
| 3 | Controles custom incompatibles | ?? Media (45%) | ?? Alto | ?? ALTO | Reemplazar gradualmente, crear wrappers |
| 4 | API no disponible en migration | ?? Media (50%) | ?? Medio | ?? ALTO | Modo fallback a SQL, mock en desarrollo |
| 5 | Bugs en migración de datos | ?? Media (60%) | ?? Alto | ?? CRÍTICO | Testing riguroso, comparación datos antes/después |
| 6 | Token JWT expira sin refresh | ?? Baja (20%) | ?? Medio | ?? MEDIO | Implementar auto-refresh, queue de requests |
| 7 | Usuarios resisten cambio UI | ?? Alta (80%) | ?? Medio | ?? MEDIO | Training, UI similar, release gradual |
| 8 | Memory leaks en binding | ?? Baja (25%) | ?? Alto | ?? ALTO | Usar WeakEventManager, liberar recursos |
| 9 | ReportService no reutilizable | ?? Media (30%) | ?? Medio | ?? MEDIO | Extraer lógica agnóstica, tests unitarios |
| 10 | Database locks en concurrencia | ?? Media (40%) | ?? Alto | ?? CRÍTICO | Connection pooling, transaction timeout, async |

---

### 1.2 Riesgos de Arquitectura

#### **Riesgo: Acoplamiento a Framework**
```csharp
// ? MAL - Core referencia a WPF
public class WarehouseService : INotifyPropertyChanged
{
    // NUNCA hacer esto!
}

// ? BIEN - Core totalmente agnóstico
public class WarehouseService
{
    private readonly IRepository<Almacen> _repo;

    public void Save(Almacen almacen)
    {
        _repo.Add(almacen);
    }
}
```

**Mitigación:**
- Revisar que Core no importe `System.Windows`
- Tests unitarios sin UI

#### **Riesgo: Lógica de Negocio en ViewModels**
```csharp
// ? MAL
public class WarehouseViewModel
{
    public void Save()
    {
        // Lógica SQL aquí
        using (SqlConnection conn = new SqlConnection(...))
        {
            SqlCommand cmd = ...
            // ? Acoplamiento fuerte
        }
    }
}

// ? BIEN
public class WarehouseViewModel
{
    private readonly IWarehouseService _service;

    public WarehouseViewModel(IWarehouseService service)
    {
        _service = service;
    }

    public void Save()
    {
        _service.Save(_warehouse);
    }
}
```

**Mitigación:**
- Usar template: ViewModel llama Service, Service llama Repository
- Code reviews

#### **Riesgo: Binding ineficiente**
```csharp
// ? MAL - Binding collection grande sin virtualización
<DataGrid ItemsSource="{Binding AllItems}"/>

// ? BIEN - Virtualización activada
<DataGrid ItemsSource="{Binding AllItems}"
          VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling">

// ? MEJOR - Usar CollectionViewSource con filtrado
<DataGrid ItemsSource="{Binding Source={StaticResource cvs}}"/>
```

**Mitigación:**
- VirtualizingStackPanel siempre activado
- Usar ObservableCollection solo si necesario
- Pagination para listas grandes

---

### 1.3 Riesgos de Performance

#### **Problema: UI se congela con operaciones síncronas**

```csharp
// ? MAL - UI Thread bloqueado
private void LoadData()
{
    var data = _service.GetAllItems(); // Espera aquí
    Items = new ObservableCollection<Item>(data);
}

// ? BIEN - Async/Await
private async Task LoadDataAsync()
{
    IsLoading = true;
    try
    {
        var data = await _service.GetAllItemsAsync();
        Items = new ObservableCollection<Item>(data);
    }
    finally
    {
        IsLoading = false;
    }
}

// En XAML
<Button Command="{Binding LoadCommand}"
        IsEnabled="{Binding IsLoading, Converter=...}">
    <TextBlock Text="{Binding IsLoading, 
               StringFormat='Loading...', 
               Converter=...}"/>
</Button>
```

**Mitigación:**
- Todas las operaciones I/O async
- Progress indicators
- Cancellation tokens

#### **Problema: Memory leaks en larga ejecución**

```csharp
// ? MAL - Event handler memory leak
public class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public ViewModel()
    {
        _service.DataChanged += (s, e) => 
        {
            // ? Referencia circular
            RefreshUI();
        };
    }
}

// ? BIEN - Desuscribirse o usar weak events
public class ViewModel : INotifyPropertyChanged, IDisposable
{
    private EventHandler _handler;

    public ViewModel()
    {
        _handler = (s, e) => RefreshUI();
        _service.DataChanged += _handler;
    }

    public void Dispose()
    {
        _service.DataChanged -= _handler;
    }
}
```

**Mitigación:**
- Implementar IDisposable en ViewModels
- WeakEventManager para eventos
- Profiling: dotTrace o Performance Profiler

---

## ?? PARTE 2: MEJORES PRÁCTICAS

### 2.1 MVVM Best Practices

#### **1. ViewModels deben ser testables sin UI**

```csharp
// ? Estructura correcta
public class WarehouseViewModel : INotifyPropertyChanged
{
    private readonly IWarehouseService _service;

    // Constructor solo con dependencias
    public WarehouseViewModel(IWarehouseService service)
    {
        _service = service;
    }

    // Propiedades observables
    private ObservableCollection<Almacen> _items;
    public ObservableCollection<Almacen> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    // Commands
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
}

// Test
[TestClass]
public class WarehouseViewModelTests
{
    [TestMethod]
    public async Task SaveCommand_Should_CallService()
    {
        // Arrange
        var mockService = new Mock<IWarehouseService>();
        var vm = new WarehouseViewModel(mockService.Object);

        // Act
        vm.SaveCommand.Execute(null);

        // Assert
        mockService.Verify(s => s.Save(It.IsAny<Almacen>()), Times.Once);
    }
}
```

#### **2. Usar MVVM Toolkit para simplificar**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

// Reduce boilerplate enormemente
public partial class WarehouseViewModel : ObservableObject
{
    private readonly IWarehouseService _service;

    [ObservableProperty]
    private ObservableCollection<Almacen> items;

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    public async Task Save(Almacen item)
    {
        IsLoading = true;
        try
        {
            await _service.SaveAsync(item);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

// En XAML
<Button Command="{Binding SaveCommand}" 
        CommandParameter="{Binding SelectedItem}"/>
```

#### **3. Usar Commands en lugar de Code-behind**

```xml
<!-- ? BIEN - MVVM -->
<Button Content="Save"
        Command="{Binding SaveCommand}"
        CommandParameter="{Binding}"/>

<!-- ? MAL - Code-behind -->
<Button Content="Save"
        Click="SaveButton_Click"/>
```

```csharp
// Code-behind NUNCA
public partial class WarehouseView : UserControl
{
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // ? Lógica aquí = no testeable
    }
}
```

---

### 2.2 Data Binding Best Practices

#### **1. UpdateSourceTrigger**

```xml
<!-- ? Problemas con LostFocus en entrada de números -->
<TextBox Text="{Binding Quantity}"/>

<!-- ? BIEN - Update inmediato -->
<TextBox Text="{Binding Quantity, 
                        UpdateSourceTrigger=PropertyChanged}"/>

<!-- ? Alternativa con delay -->
<TextBox x:Name="SearchBox"/>
<TextBlock Text="{Binding ElementName=SearchBox, 
                          Path=Text,
                          Delay=500}"/>
```

#### **2. Modo de Binding apropiado**

```xml
<!-- ? OneWay para datos de solo lectura -->
<TextBlock Text="{Binding UserName, Mode=OneWay}"/>

<!-- ? TwoWay para edición -->
<TextBox Text="{Binding UserName, Mode=TwoWay}"/>

<!-- ? OneWayToSource raro pero útil -->
<TextBox Text="{Binding SearchCriteria, Mode=OneWayToSource}"/>

<!-- ? OneTime para datos que no cambian -->
<TextBlock Text="{Binding Version, Mode=OneTime}"/>
```

#### **3. Data Validation**

```csharp
// ? IDataErrorInfo en ViewModel
public class WarehouseViewModel : BaseViewModel, IDataErrorInfo
{
    private string _itemName;
    public string ItemName
    {
        get => _itemName;
        set => SetProperty(ref _itemName, value);
    }

    public string Error => null;

    public string this[string columnName]
    {
        get
        {
            if (columnName == nameof(ItemName))
            {
                if (string.IsNullOrEmpty(ItemName))
                    return "Item name is required";
                if (ItemName.Length < 3)
                    return "Item name must be at least 3 characters";
            }
            return null;
        }
    }
}
```

```xml
<!-- ? Validación en XAML -->
<TextBox Text="{Binding ItemName, 
                        UpdateSourceTrigger=PropertyChanged,
                        ValidatesOnDataErrors=True}"
         Validation.ErrorTemplate="{StaticResource ErrorTemplate}"/>
```

---

### 2.3 Patrones de Arquitectura

#### **1. Repository Pattern correcto**

```csharp
// ? Interfaz genérica
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveAsync();
}

// ? Implementación agnóstica
public class SqlRepository<T> : IRepository<T> where T : class
{
    private readonly IDbConnection _connection;

    public async Task<T> GetByIdAsync(int id)
    {
        const string sql = $"SELECT * FROM {typeof(T).Name}s WHERE Id = @Id";
        return await _connection.QuerySingleOrDefaultAsync<T>(sql, new { Id = id });
    }

    // ... resto
}
```

#### **2. Unit of Work Pattern**

```csharp
// ? Transacciones coordinadas
public interface IUnitOfWork : IDisposable
{
    IRepository<Almacen> Almacenes { get; }
    IRepository<Partida> Partidas { get; }
    Task<int> SaveAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;

    public IRepository<Almacen> Almacenes { get; }
    public IRepository<Partida> Partidas { get; }

    public UnitOfWork(IDbConnection connection)
    {
        _connection = connection;
        Almacenes = new SqlRepository<Almacen>(connection);
        Partidas = new SqlRepository<Partida>(connection);
    }

    public async Task<int> SaveAsync()
    {
        using (var transaction = _connection.BeginTransaction())
        {
            try
            {
                // Guardar cambios
                await Almacenes.SaveAsync();
                await Partidas.SaveAsync();
                transaction.Commit();
                return 1;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
```

#### **3. Service Locator ANTI-PATTERN**

```csharp
// ? NUNCA hacer esto
public class WarehouseService
{
    public void Save(Almacen almacen)
    {
        var repo = ServiceLocator.GetService<IRepository<Almacen>>();
        // ? Dependencia oculta, no testeable
    }
}

// ? Constructor Injection siempre
public class WarehouseService
{
    private readonly IRepository<Almacen> _repo;

    public WarehouseService(IRepository<Almacen> repo)
    {
        _repo = repo; // ? Explícito, testeable
    }
}
```

---

### 2.4 Testing Best Practices

#### **1. Estructura AAA (Arrange-Act-Assert)**

```csharp
[TestClass]
public class WarehouseServiceTests
{
    [TestMethod]
    public async Task SaveAlmacen_Should_InsertIntoDatabase()
    {
        // ARRANGE - Preparar mocks y datos
        var mockRepo = new Mock<IRepository<Almacen>>();
        var service = new WarehouseService(mockRepo.Object);
        var almacen = new Almacen { Nombre = "Test" };

        // ACT - Ejecutar operación
        await service.SaveAsync(almacen);

        // ASSERT - Verificar resultado
        mockRepo.Verify(r => r.AddAsync(almacen), Times.Once);
    }
}
```

#### **2. Test Async/Await correctamente**

```csharp
// ? BIEN
[TestMethod]
public async Task LoadData_Should_PopulateItems()
{
    // Arrange
    var vm = new WarehouseViewModel(mockService.Object);

    // Act
    await vm.LoadDataCommand.ExecuteAsync(null);

    // Assert
    Assert.AreEqual(3, vm.Items.Count);
}

// ? MAL - No espera el async
[TestMethod]
public void LoadData_Should_PopulateItems()
{
    var vm = new WarehouseViewModel(mockService.Object);
    vm.LoadDataCommand.Execute(null); // No espera!
    Assert.AreEqual(3, vm.Items.Count); // Falla!
}
```

#### **3. Stubbing y Mocking**

```csharp
[TestMethod]
public void Save_Should_HandleException()
{
    // ARRANGE
    var mockService = new Mock<IWarehouseService>();
    mockService
        .Setup(s => s.SaveAsync(It.IsAny<Almacen>()))
        .ThrowsAsync(new ArgumentException("Invalid"));

    var vm = new WarehouseViewModel(mockService.Object);

    // ACT & ASSERT
    Assert.ThrowsExceptionAsync<ArgumentException>(
        () => vm.SaveCommand.ExecuteAsync(new Almacen())
    );
}
```

---

## ? PARTE 3: OPTIMIZACIÓN DE PERFORMANCE

### 3.1 UI Performance

#### **1. Virtualización**

```xml
<!-- ? DataGrid con virtualización -->
<DataGrid ItemsSource="{Binding Items}"
          VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling"
          EnableRowVirtualization="True"
          EnableColumnVirtualization="True">
</DataGrid>

<!-- ? ListBox con virtualización -->
<ListBox ItemsSource="{Binding Items}"
         VirtualizingPanel.IsVirtualizing="True"
         ScrollViewer.CanContentScroll="True">
</ListBox>
```

#### **2. Lazy Loading**

```csharp
// ? Cargar datos bajo demanda
public class WarehouseViewModel : BaseViewModel
{
    private ObservableCollection<Almacen> _items;

    [RelayCommand]
    public async Task LoadMore()
    {
        IsLoading = true;
        try
        {
            var nextBatch = await _service.GetItemsAsync(
                pageNumber: CurrentPage++,
                pageSize: 50
            );

            foreach (var item in nextBatch)
                Items.Add(item);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

// En XAML
<ListBox ItemsSource="{Binding Items}">
    <i:Interaction.Behaviors>
        <behaviors:ScrollViewerEndBehavior 
            Command="{Binding LoadMoreCommand}"/>
    </i:Interaction.Behaviors>
</ListBox>
```

#### **3. Caching**

```csharp
// ? Cache simple en ViewModel
public class WarehouseViewModel : BaseViewModel
{
    private Dictionary<int, Almacen> _cache;

    public async Task<Almacen> GetWarehouseAsync(int id)
    {
        if (_cache.TryGetValue(id, out var cached))
            return cached;

        var item = await _service.GetByIdAsync(id);
        _cache[id] = item;
        return item;
    }
}

// ? Invalidar cache cuando sea necesario
public async Task UpdateWarehouseAsync(Almacen almacen)
{
    await _service.UpdateAsync(almacen);
    _cache.Remove(almacen.Id); // Invalidar
}
```

#### **4. Binding Performance**

```xml
<!-- ? MAL - Binding complejo -->
<TextBlock Text="{Binding Items.Count}"/>

<!-- ? BIEN - Binding simple con Converter -->
<TextBlock Text="{Binding ItemCount}"/>

<!-- ? MAL - Convertidor pesado en cada render -->
<TextBlock Text="{Binding Date, Converter={StaticResource DateConverter}}"/>

<!-- ? BIEN - Formato predeterminado -->
<TextBlock Text="{Binding Date, StringFormat=dd/MM/yyyy}"/>
```

---

### 3.2 Data Access Performance

#### **1. Async todas las operaciones I/O**

```csharp
// ? Async/Await
public async Task<List<Almacen>> GetAllAsync()
{
    using (var connection = _connectionFactory.CreateConnection())
    {
        await connection.OpenAsync();
        return (await connection.QueryAsync<Almacen>(...)).ToList();
    }
}

// ? Sin bloquear UI Thread
public async Task LoadDataAsync()
{
    var data = await _service.GetAllAsync();
    Items = new ObservableCollection<Almacen>(data);
}
```

#### **2. Connection Pooling**

```xml
<!-- appsettings.json -->
{
  "ConnectionStrings": {
    "CalandriaConn": "Server=SERVER;
                     Database=Calandria;
                     User Id=sa;
                     Password=PASSWORD;
                     Min Pool Size=5;
                     Max Pool Size=100;
                     Connection Lifetime=300;"
  }
}
```

#### **3. Proyecciones en Queries**

```csharp
// ? MAL - Traer todo
public List<Almacen> GetAllAlmacenes()
{
    return context.Almacenes.ToList(); // ? Todas las columnas
}

// ? BIEN - Solo lo necesario
public async Task<List<AlmacenDto>> GetAllAlmacenesAsync()
{
    const string sql = @"
        SELECT Id, Nombre, Ubicacion, Capacidad 
        FROM Almacen";

    return (await connection.QueryAsync<AlmacenDto>(sql)).ToList();
}
```

#### **4. Batch Operations**

```csharp
// ? Insertar múltiples registros eficientemente
public async Task BulkInsertAsync(IEnumerable<Almacen> items)
{
    const string sql = @"
        INSERT INTO Almacen (Nombre, Ubicacion, Capacidad)
        VALUES (@Nombre, @Ubicacion, @Capacidad)";

    await connection.ExecuteAsync(sql, items);
}
```

---

### 3.3 Memory Management

#### **1. Usar WeakReference para eventos**

```csharp
// ? Para collections grandes con cambios frecuentes
public class WeakEventManager
{
    private readonly List<WeakReference<EventHandler>> _handlers;

    public void Subscribe(EventHandler handler)
    {
        _handlers.Add(new WeakReference<EventHandler>(handler));
    }

    public void RaiseEvent()
    {
        _handlers.RemoveAll(wr => !wr.TryGetTarget(out _));

        foreach (var handler in _handlers
            .Where(wr => wr.TryGetTarget(out _))
            .Select(wr => { wr.TryGetTarget(out EventHandler h); return h; }))
        {
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
```

#### **2. ObservableCollection optimizado**

```csharp
// ? Deshabilitar notificaciones durante bulk operations
public class OptimizedObservableCollection<T> : ObservableCollection<T>
{
    private bool _notificationEnabled = true;

    public void BeginBulkOperation()
    {
        _notificationEnabled = false;
    }

    public void EndBulkOperation()
    {
        _notificationEnabled = true;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset));
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (_notificationEnabled)
            base.OnCollectionChanged(e);
    }
}

// Uso
var items = new OptimizedObservableCollection<Item>();
items.BeginBulkOperation();
foreach (var item in largeList)
    items.Add(item); // Sin notificaciones
items.EndBulkOperation(); // Una sola notificación
```

---

## ?? PARTE 4: CHECKLIST DE CALIDAD

### 4.1 Code Quality

- [ ] Sin warnings de compilación
- [ ] StyleCop configurado (CSharp code analysis)
- [ ] Code coverage > 70%
- [ ] Reporte de Code Analysis sin errores críticos
- [ ] Sin TODOs pendientes de resolver
- [ ] Documentación XML completa en públicos

### 4.2 Performance

- [ ] Startup time < 2 segundos
- [ ] UI responsiva con 10K+ items en grillas
- [ ] Memory leak test (larga ejecución)
- [ ] Profiling con dotTrace
- [ ] Queries optimizadas (índices DB)

### 4.3 Testing

- [ ] Unit tests para todos los servicios
- [ ] Integration tests para repos
- [ ] UI tests para flujos críticos
- [ ] Tests de async/await
- [ ] Tests de excepciones

### 4.4 Security

- [ ] Contraseñas hasheadas (no plaintext)
- [ ] SQL injection prevención (parameterized)
- [ ] CORS configurado si aplica
- [ ] Secrets en appsettings.Development.json
- [ ] HTTPS en API calls

### 4.5 Usabilidad

- [ ] Temas aplicados uniformemente
- [ ] Dark mode opcional
- [ ] Accesibilidad (Tab order, labels)
- [ ] Atajos de teclado principales
- [ ] Mensajes de error claros

---

## ?? CONCLUSIÓN

La clave del éxito está en:

1. **Arquitectura clara:** Separación nítida entre capas
2. **Testing temprano:** Unit tests desde Fase 1
3. **Performance consciousness:** Profiling, no asumir
4. **Security first:** Validación, hashing, parameterized queries
5. **User-centric:** Mantener experiencia similar a WinForms
6. **Documentation:** Especialmente en puntos complejos

---

**Documento de referencia durante toda la migración**
