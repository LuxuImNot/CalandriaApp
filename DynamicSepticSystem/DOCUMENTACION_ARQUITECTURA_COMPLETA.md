# DOCUMENTACIÓN COMPLETA: SISTEMA DINÁMICO DE CONSTRUCCIÓN RESIDENCIAL

## ?? Índice General

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Inventario de Formularios](#inventario-de-formularios)
3. [Inventario de Clases y Modelos](#inventario-de-clases-y-modelos)
4. [Inventario de Servicios](#inventario-de-servicios)
5. [Inventario de Dependencias](#inventario-de-dependencias)
6. [Arquitectura de Base de Datos](#arquitectura-de-base-de-datos)
7. [Flujo de Navegación](#flujo-de-navegación)
8. [Módulos y Capas](#módulos-y-capas)

---

## Resumen Ejecutivo

**Aplicación**: DynamicSepticSystem  
**Tipo**: Aplicación WinForms (.NET Framework 4.7.2)  
**Versión**: 1.3.7.0 (en rama feature/api-migration)  
**Propósito**: Sistema de gestión y seguimiento de proyectos de construcción residencial (Calandria)  
**Arquitectura**: Híbrida (SQL directo + Web API REST)  
**Ubicación BD**: SQL Server 100.75.234.9 (CALANDRIA database)  
**API Base**: http://100.75.234.9:8733

### Características Principales
- ? Gestión de inventario de propiedades (manzanas, lotes, prototipos)
- ? Estimación de conceptos y partidas de obra
- ? Almacén: entradas, salidas e inventario
- ? Activación de destajos y tareas
- ? Mano de obra y jornales
- ? Generación de PDF (estimaciones, órdenes de compra, vales)
- ? Evidencias fotográficas
- ? Sistema de usuarios con permisos
- ? Actualización automática desde GitHub
- ? Gestión de rutas críticas (Gantt)

---

## Inventario de Formularios

### ?? Formularios Principales (Panel Central)

| Formulario | Clase | Archivo | Propósito |
|---|---|---|---|
| **Panel Principal** | `PanelPrincipal` | PanelPrincipal.cs | Dashboard/Home del sistema |
| **Estimación de Conceptos (Migrado)** | `FormEstimacionConceptoMigrado` | FormEstimacionConceptoMigrado.cs | Crear y gestionar estimaciones por conceptos |
| **Almacén** | `FormAlmacen` | FormAlmacen.cs | Gestión de entradas, salidas e inventario |
| **Activar Tareas TreeList** | `FormActivarTareasTreeList` | FormActivarTareasTreeList.cs | Activar/desactivar destajos con vista jerárquica |
| **Editor TreeList** | `FormEditorTreeList` | FormEditorTreeList.cs | Editar rutas de destajos (RutaTuneraDestajo, RutaCalandraDestajo) |
| **Gestionar Partidas** | `FormGestionarPartidas` | FormGestionarPartidas.cs | Crear/editar partidas de conceptos existentes |
| **Mano de Obra** | `FormManoObra` | FormManoObra.cs | Registro de mano de obra por destajo |
| **Insumos** | `FormInsumos` | FormInsumos.cs | Gestión de insumos por destajo |
| **Avance de Obra** | `FormAvanceObra` | FormAvanceObra.cs | Seguimiento de avance por casa |
| **Avance de Concepto** | `FormAvanceConcepto` | FormAvanceConcepto.cs | Seguimiento de avance por concepto |

### ?? Formularios de Autenticación y Seguridad

| Formulario | Clase | Archivo | Propósito |
|---|---|---|---|
| **Login** | `FormLogin` | FormLogin.cs | Autenticación de usuarios (SQL + opcional API JWT) |
| **Registrar Usuario** | `FormRegistrarUsuario` | FormRegistrarUsuario.cs | Crear nuevos usuarios (admin) |
| **Registrar Trabajador** | `FormRegistrarTrabajador` | FormRegistrarTrabajador.cs | Registro de trabajadores en base de datos |
| **Perfil Trabajador** | `FormPerfilTrabajador` | FormPerfilTrabajador.cs | Ver/editar información del trabajador |

### ?? Formularios de Compra e Insumos

| Formulario | Clase | Archivo | Propósito |
|---|---|---|---|
| **Compra Directa (Multi)** | `FormCompraMulti` | FormCompraMulti.cs | Generar órdenes de compra a múltiples proveedores |
| **Compra Indirecta** | `FormCompraIndirecta` | FormCompraIndirecta.cs | Registro de compras no planificadas |
| **Agregar Proveedor** | `FormAgregarProveedor` | FormAgregarProveedor.cs | Crear nuevos proveedores |
| **Agregar Concepto** | `FormAgregarConcepto` | FormAgregarConcepto.cs | Agregar nuevo concepto a una manzana/lote |
| **Agregar Insumo Carrito** | `FormAgregarInsumoCarrito` | FormAgregarInsumoCarrito.cs | Interfaz para seleccionar insumos a comprar |
| **Agregar Insumo Indirecto** | `FormAgregarInsumoIndirecto` | FormAgregarInsumoIndirecto.cs | Registrar insumos de compra indirecta |

### ?? Formularios de Consultas y Reportes

| Formulario | Clase | Archivo | Propósito |
|---|---|---|---|
| **Detalles de Estimación** | `FormDetalleEstimacion` | FormDetalleEstimacion.cs | Ver detalles de una estimación guardada |
| **Detalles de Orden de Compra** | `FormDetalleOrdenCompra` | FormDetalleOrdenCompra.cs | Ver detalles de una orden de compra |
| **Detalles de Vale Salida** | `FormDetalleValeSalida` | FormDetalleValeSalida.cs | Ver detalles de un vale de salida de almacén |
| **Almacén: Consulta Casa** | `FormAlmacen_ConsultaCasa` | FormAlmacen_ConsultaCasa.cs | Consultar inventario de una casa específica |
| **Reporte** | `FormReporte` | FormReporte.cs | Generador de reportes personalizados |
| **Reporte Destajos Semana** | `FormReporteDestajosSemana` | FormReporteDestajosSemana.cs | Reporte de destajos completados en la semana |
| **Reporte Nómina** | `FormReporteNomina` | FormReporteNomina.cs | Reporte de nómina de trabajadores |
| **Visor Recibos Nómina** | `FormVisorRecibosNomina` | FormVisorRecibosNomina.cs | Consultar y descargar recibos de nómina |

### ?? Formularios de Repositorios (Historial y Archivos)

| Formulario | Clase | Archivo | Propósito |
|---|---|---|---|
| **Repositorio PDFs** | `FormRepositorioPDFs` | FormRepositorioPDFs.cs | Gestionar histórico de PDFs (estimaciones, órdenes) |
| **Repositorio PDFs Órdenes de Compra** | `FormRepositorioPDFsOrdenesCompra` | FormRepositorioPDFsOrdenesCompra.cs | Repositorio específico de órdenes de compra |
| **Repositorio Vales Salida** | `FormRepositorioValesSalida` | FormRepositorioValesSalida.cs | Histórico de vales de salida |
| **Repositorio Destajos** | `FormRepositorioDestajos` | FormRepositorioDestajos.cs | Histórico y búsqueda de destajos |
| **Evidencias Fotográficas** | `FormEvidenciasFotograficas` | FormEvidenciasFotograficas.cs | Galería de evidencias de avance |

### ??? Formularios Utilitarios y Auxiliares

| Formulario | Clase | Archivo | Propósito |
|---|---|---|---|
| **Splash Screen** | `FormSplash` | FormSplash.cs | Pantalla de carga inicial |
| **Preview PDF** | `FormPdfPreview` | FormPdfPreview.cs | Visualizador de PDF simple |
| **Preview PDF Maximizado** | `FormPreviewPDFMaximizado` | FormPreviewPDFMaximizado.cs | Visualizador PDF en pantalla completa |
| **Prompt Cantidad** | `PromptCantidad` | PromptCantidad.cs | Diálogo para ingresar cantidad |
| **Prompt Justificación** | `PromptJustificacion` | PromptJustificacion.cs | Diálogo para ingresar justificación de operación |
| **Seleccionar Título** | `FormSeleccionarTitulo` | FormSeleccionarTitulo.cs | Selector de título dinámico |
| **Seleccionar Nivel** | `DialogSeleccionarNivel` | DialogSeleccionarNivel.cs | Selector de nivel en estructura jerárquica |
| **Mapear Coordenadas** | `FormMapearCoordenadas` | FormMapearCoordenadas.cs | Editor de coordenadas geográficas |
| **Progreso Actualización** | `FormProgresoActualizacion` | FormProgresoActualizacion.cs | Barra de progreso de actualización |
| **Hard Progress** | `FormHardProgress` | FormHardProgress.cs | Diálogo de progreso genérico |
| **Log de Errores** | `FormLogErrores` | FormLogErrores.cs | Consulta de errores registrados en BD |
| **Diagnóstico Conexión** | `FormDiagnosticoConexion` | FormDiagnosticoConexion.cs | Verificar conectividad BD y API |
| **Administrativos** | `FormAdministrativos` | FormAdministrativos.cs | Panel de administración (no especificado) |
| **Ruta Crítica** | `FormRutaCritica` | FormRutaCritica.cs | Visualización de ruta crítica (Gantt) |
| **Distribuir Nómina** | `FormDistribucionNomina` | FormDistribucionNomina.cs | Asignar nómina a trabajadores |
| **Asignar Nómina** | `FormAsignarNomina` | FormAsignarNomina.cs | Asignar jornal a un trabajador |
| **Asignar Cuadrilla** | `FormAsignarCuadrilla` | FormAsignarCuadrilla.cs | Asignar grupo de trabajo a un destajo |
| **Destajos por Cuadrilla** | `FormDestajosPorCuadrilla` | FormDestajosPorCuadrilla.cs | Consultar destajos de una cuadrilla |
| **Generar Orden** | `FormGenerarOrden` | FormGenerarOrden.cs | Generar orden de compra interactivamente |
| **Propiedades Concepto** | `FormPropiedadesConcepto` | FormPropiedadesConcepto.cs | Editar propiedades de un concepto |
| **Propiedades Destajo** | `FormPropiedadesDestajo` | FormPropiedadesDestajo.cs | Editar propiedades de un destajo |

### ?? Formularios WPF Experimentales

| Formulario | Clase | Archivo | Propósito |
|---|---|---|---|
| **Panel Principal WPF** | `PanelPrincipalWPF` | WPF/PanelPrincipalWPF.xaml.cs | Versión experimental en WPF (no activa) |

### ?? Formularios Especializados de Edición

| Formulario | Clase | Archivo | Propósito |
|---|---|---|---|
| **Editar Explosiones** | `FormEditarExplosiones` | FormEditarExplosiones.cs | Editar matrices de explosión de insumos |
| **Generar Explosión PDF** | (Helper) | GenerarExplosionInsumosPDF.cs | Generar reporte PDF de explosión de insumos |

---

## Inventario de Clases y Modelos

### ?? Modelos de Dominio (Data Models)

#### Casa y Inventario
```
Casa
??? Manzana: string
??? Lote: string
??? Prototipo: string
??? FotoPath: string

CasaInventario
??? Manzana: string
??? Lote: string
??? Prototipo: string
??? FotoPath: string
??? DestajosTerminadosWBS: List<string>
```

#### Conceptos y Partidas
```
NodoConcepto
??? EsConcepto: bool (true = concepto, false = partida)
??? WBS: int (código numérico único)
??? Codigo: string
??? Nombre: string
??? Total: double
??? EsActivo: bool
??? MetrosCuadradosPresupuestados: double
??? (propiedades derivadas)

PartidaConcepto
??? WBS: int
??? Etapa: string
??? Partida: string
??? CostoTunera: double
??? CostoCalandra: double
??? EsDinamica: bool
??? ValorM2Tunera: double
??? ValorM2Calandra: double

ConceptoExistente
??? WBS: int
??? Codigo: string
??? Nombre: string
??? (usado en formularios de edición)

PartidaDinamica
??? WBS: int
??? (propiedades para cálculo dinámico de costos)

NodoTree
??? Nivel: int (0=Padre, 1=SubPadre, 2=Hijo)
??? Datos: Dictionary<string, object>
??? Hijos: List<NodoTree>
??? (usado en FormEditorTreeList)
```

#### Tareas y Destajos
```
TareaRutaCritica
??? Id: string
??? Nombre: string
??? Descripcion: string

Tarea
??? WBS: int
??? Nombre: string
??? (propiedades extendidas según contexto)
```

#### Usuario y Seguridad
```
PanelPrincipal.Usuario (interno)
??? Nombre: string
??? Clave: string (no almacenada en memoria, solo validación)
??? Permisos: List<string>
??? Rol: string (admin, supervisor, etc.)
```

#### Evidencias y Fotos
```
EvidenciaInfo
??? Id: int
??? Manzana: string
??? Lote: string
??? Titulo: string
??? FechaCaptura: DateTime
??? Foto: byte[]
??? Extension: string
??? Usuario: string
```

### ?? Clases Auxiliares y Utilidades

| Clase | Archivo | Propósito |
|---|---|---|
| `ErrorLogger` | ErrorLogger.cs | Manejador global de excepciones y registro en BD |
| `PasswordHasher` | PasswordHasher.cs | Hash y verificación de contraseñas (ARGON2) |
| `ApiClient` | ApiClient.cs | Cliente HTTP centralizado hacia Web API |
| `UpdateServer` | UpdateServer.cs | Manejo de actualizaciones del servidor |
| `Actualizador` | Actualizador.cs | Verificación y descarga de actualizaciones desde GitHub |
| `Global` | Global.cs | Variables globales (usuario actual, estado app) |
| `ThemeManager` | ThemeManager.cs | Gestión de temas y estilos corporativos |
| `NumeroALetras` | NumeroALetras.cs | Conversión de números a palabras |
| `Resizer` | Resizer.cs | Utilidad de redimensionamiento responsive |
| `NewFeatureBadge` | NewFeatureBadge.cs | Badge visual para nuevas características |
| `FolioManager` | FolioManager.cs | Gestión de folios de documentos |
| `DetalleOrdenHelper` | DetalleOrdenHelper.cs | Helper para manejo de detalles de órdenes |
| `SafeTreeListView` | SafeTreeListView.cs | Wrapper seguro de TreeListView |
| `TreeGridViewLite` | TreeGridViewLite.cs | Control de tree grid ligero |
| `Reportes` | Reportes.cs | Generador de reportes genérico |

### ?? Gestores Especializados

| Clase | Archivo | Propósito |
|---|---|---|
| `GestorFotosConcepto` | GestorFotosConcepto.cs | Gestión de fotos asociadas a conceptos |
| `GestorEvidencias` | GestorEvidencias.cs | Gestión de evidencias fotográficas de avance |
| `GenerarExplosionInsumosPDF` | GenerarExplosionInsumosPDF.cs | Generación de PDF de explosión de insumos |

### ?? Enumeraciones y Tipos Especiales

```
NodoCategoria
??? (estructura para categorías de árbol)

ConceptoExistente
??? (modelo para conceptos en BD)
```

---

## Inventario de Servicios

### ?? Servicios de Acceso a Datos

#### InventarioService
**Archivo**: `InventarioService.cs`

**Propósito**: Acceso centralizado a datos de inventario de casas

**Métodos principales**:
- `LeerInventarioCasasSQL()` ? List<CasaInventario>
- `InsertarCasaEnBD(CasaInventario casa)` ? void
- `ObtenerManoObraEntregada(string manzana, string lote, int wbs)` ? DataTable
- `ObtenerInsumosEntregados(string manzana, string lote, int wbs)` ? DataTable
- `ActualizarDestajosTerminados(string manzana, string lote, List<string> wbsTerminados)` ? void

**Dependencias**:
- SQL Server (CalandriaConn connection string)
- Newtonsoft.Json (para deserializar JSON en BD)

---

#### SalidaAlmacenService
**Archivo**: `SalidaAlmacenService.cs`

**Propósito**: Gestión centralizada de salidas de almacén y liberación automática de insumos

**Métodos principales**:
- `EnsureEsquema()` / `EnsureEsquema(SqlConnection)` ? void (crea tablas si no existen)
- `GenerarVale(...)` ? void (crea vale de salida)
- `LiberarInsumosAlActivarDestajo(...)` ? void (flujo automático)
- `ObtenerHistorialSalidas(...)` ? List<...>

**Dependencias**:
- SQL Server
- PdfSharp (generación de PDFs)

---

### ?? Servicios de Comunicación

#### ApiClient (Estático)
**Archivo**: `ApiClient.cs`

**Propósito**: Cliente centralizado HTTP hacia Web API (Calandria.Api)

**Métodos principales**:
- `Login(string usuario, string clave)` ? LoginResponseApi (retorna JWT)
- `Get<T>(string rutaRelativa)` ? T (GET tipado)
- `GetBytes(string rutaRelativa)` ? byte[] (GET binario)
- `Post<T>(string ruta, object body)` ? T
- `Put<T>(string ruta, object body)` ? T
- `Delete(string ruta)` ? void

**Propiedades**:
- `BaseUrl` (desde App.config: http://100.75.234.9:8733)
- `Token` (JWT de sesión actual)
- `Autenticado` (bool)

**Nota**: App es híbrida (SQL directo + API). Si API no disponible, solo pantallas migradas se ven afectadas.

---

### ?? Servicios de Gestión de Archivos

#### GestorEvidencias
**Archivo**: `GestorEvidencias.cs`

**Propósito**: Gestión de evidencias fotográficas de avance

**Métodos principales**:
- `GuardarEvidencia(string manzana, string lote, string titulo, byte[] foto, string extension)` ? bool
- `ObtenerEvidencias(string manzana, string lote)` ? List<EvidenciaInfo>
- `EliminarEvidencia(int id)` ? bool

**Nota**: Migrado a Web API (acceso vía EvidenciasController). Métodos conservan firma por compatibilidad.

---

#### GestorFotosConcepto
**Archivo**: `GestorFotosConcepto.cs`

**Propósito**: Gestión de fotos asociadas a conceptos específicos

**Métodos principales** (helpers de imagen):
- Manipulación de imágenes (escalado, compresión)
- Caché de thumbnails

---

#### FolioManager
**Archivo**: `FolioManager.cs`

**Propósito**: Gestión de folios (secuencias numeradas de documentos)

**Métodos principales**:
- `GenerarFolio(string tipo)` ? string
- `RegisterFolio(string tipo, string numero)` ? void
- `ObtenerProximoFolio(string tipo)` ? int

---

### ?? Servicios de Sistema y Actualización

#### Actualizador
**Archivo**: `Actualizador.cs`

**Propósito**: Verificación y descarga de actualizaciones desde GitHub

**Métodos principales**:
- `VerificarYActualizarAsync()` ? Task (llamado en Main)
- `VersionLocal` (property)

**Configuración** (en App.config):
- GitHubOwner: "LuxuImNot"
- GitHubRepo: "CalandriaApp"
- GitHubToken: (token PAT)
- RequireUpdateHash: false (verificar firma)
- UpdateSignerThumbprint: (opcional)

---

#### UpdateServer
**Archivo**: `UpdateServer.cs`

**Propósito**: Lado servidor de sistema de actualización (complementario)

---

### ?? Servicios de Logging y Diagnóstico

#### ErrorLogger (Estático)
**Archivo**: `ErrorLogger.cs`

**Propósito**: Registro centralizado de excepciones no manejadas

**Métodos principales**:
- `Inicializar()` (llamar una sola vez en Main)
- `Manejar(Exception ex, string origen)` ? void
- `Registrar(Exception ex, string origen)` ? void

**Almacenamiento**:
- Base de datos: tabla `LogErrores`
- Archivo local: `%LocalAppData%/DynamicSepticSystem/logs/errores.log`

**Antispam**: Evita repetir registro/diálogo de misma excepción

---

### ?? Servicios de Seguridad

#### PasswordHasher
**Archivo**: `PasswordHasher.cs`

**Propósito**: Hash y verificación de contraseñas con ARGON2

**Métodos principales**:
- `HashearContraseña(string clave)` ? string
- `Verificar(string clave, string hash, out bool necesitaRehash)` ? bool

---

### ?? Servicios de Presentación

#### ThemeManager
**Archivo**: `ThemeManager.cs`

**Propósito**: Aplicación centralizada de temas y estilos corporativos

**Métodos principales**:
- `InicializarAplicacion()` (Main)
- `AplicarTema(Form form)` (aplicar tema a form)
- `AplicarTemaMaterial(Form form)` (MaterialSkin)

**Colores corporativos**: Café, crema, tonos tierra

---

### ?? Servicios de Reporte

#### Reportes
**Archivo**: `Reportes.cs`

**Propósito**: Generador genérico de reportes (PDF, Excel, etc.)

---

#### GenerarExplosionInsumosPDF
**Archivo**: `GenerarExplosionInsumosPDF.cs`

**Propósito**: Generación especializada de PDF de explosión de insumos

---

---

## Inventario de Dependencias

### ?? NuGet Packages (packages.config)

#### Serialización y JSON
- **Newtonsoft.Json** 13.0.3: Serialización/deserialización JSON

#### Generación de Reportes PDF
- **PDFsharp** 1.50.5147: Generación de PDF vectorial
- **iTextSharp** 5.5.13.3: Generación alternativa de PDF
- **PdfiumViewer** 2.13.0.0: Visualizador de PDF (viewer)

#### Excel y Hojas de Cálculo
- **ClosedXML** 0.105.0: Lectura/escritura Excel (.xlsx)
- **ClosedXML.Parser** 2.0.0: Parser de formatos
- **EPPlus** 8.0.5: Alternativa Excel
- **EPPlus.Interfaces** 8.0.0
- **ExcelDataReader** 3.7.0: Lectura de Excel
- **ExcelDataReader.DataSet** 3.7.0
- **ExcelNumberFormat** 1.1.0
- **DocumentFormat.OpenXml** 3.1.1: Formatos Office XML

#### UI Controles WinForms
- **MaterialSkin.2** 2.3.1: Material Design para WinForms
- **ObjectListView** 2.7.1.5: TreeListView avanzado (BrightIdeasSoftware)

#### Gestión de Proyectos y Gantt
- **DlhSoft.GanttChartLibrary.WindowsForms** 2.0.0.7: Gantt WinForms
- **DlhSoft.GanttChartLibrary.wpf** 4.3.49: Gantt WPF
- **DlhSoft.GanttChartLightLibrary** 4.3.49
- **DlhSoft.HierarchicalDataLightLibrary** 4.0.10.8
- **DlhSoft.ProjectManagementFramework** 5.0.7.7
- **DlhSoft.ProjectManagementLibrary.WindowsForms** 2.0.0.3

#### Importación/Exportación de Proyectos
- **MPXJ.Net** 14.5.1: Importar/exportar archivos de proyecto (MS Project, etc.)

#### Conversión de Bytecode Java
- **IKVM** 8.11.2: Java bytecode a .NET (usado por algunas librerías)
- Múltiples runtimes IKVM para diferentes plataformas

#### Seguridad Criptográfica
- **BouncyCastle** 1.8.9: Librería criptográfica avanzada
- **System.Security.Cryptography.Xml** 8.0.2: XML signing/encryption

#### Geometría y Estructuras de Datos
- **RBush.Signed** 4.0.0: Spatial indexing (búsquedas espaciales)
- **SixLabors.Fonts** 1.0.0: Renderizado de fuentes

#### APIs del Sistema
- **System.Runtime.InteropServices.RuntimeInformation** 4.3.0
- **System.Text.Encoding.CodePages** 9.0.5: Soporte de Code Pages

#### Utilidades Asincrónicas y Primitivas
- **System.Threading.Tasks.Extensions** 4.5.4
- **System.ValueTuple** 4.5.0
- **System.Memory** 4.6.0
- **System.Buffers** 4.6.0
- **System.Numerics.Vectors** 4.6.0
- **System.Collections.Immutable** 8.0.0
- **System.Reflection.Metadata** 8.0.1
- **System.Runtime.CompilerServices.Unsafe** 6.1.0
- **System.Bcl.AsyncInterfaces** 8.0.0
- **System.Bcl.HashCode** 1.1.1
- **System.ComponentModel.Annotations** 5.0.0
- **System.Text.Json** 8.0.5: JSON system (alternativa a Newtonsoft)
- **System.Text.Encodings.Web** 8.0.0

#### Utilidades de Memoria
- **Microsoft.IO.RecyclableMemoryStream** 3.0.1: Pool de memoria eficiente

#### Fechas/Horas (Portabilidad)
- **Portable.System.DateTimeOnly** 9.0.0: Tipos DateOnly/TimeOnly

#### Build/IDE
- **DlhSoft.Maven.Sdk** 1.9.3: Herramientas Maven (dev dependency)
- **DlhSoft.MSBuild** 8.11.2: Herramientas MSBuild

### ??? Referencias del Framework
- **.NET Framework 4.7.2** (targetFramework)
- **System.Data.SqlClient**: Acceso a SQL Server (built-in)
- **System.Configuration**: Lectura App.config (built-in)
- **System.Net.Http**: Cliente HTTP (built-in)
- **System.Net.Http.Headers**: Headers HTTP (built-in)

### ?? Servicios Externos
- **SQL Server 100.75.234.9**: Base de datos central (CALANDRIA)
- **Web API http://100.75.234.9:8733**: Calandria.Api (REST)
- **GitHub API**: Descarga de actualizaciones (feature/api-migration)
- **Tailscale Network**: VPN privada (IPs 100.x.x.x)

---

## Arquitectura de Base de Datos

### ?? Ubicación y Conexión
```
Servidor: 100.75.234.9
Database: CALANDRIA
Port: Default (1433)
Autenticación: SQL Server (sa / password)
Encryption: TrustServerCertificate=true, Encrypt=false
ConnectionString (App.config): CalandriaConn
```

### ??? Tablas Principales Identificadas

#### 1. Usuarios y Seguridad
```
Usuarios
??? Nombre: string (PK)
??? ClaveHash: string (ARGON2)
??? Rol: string
??? FechaCreacion: DateTime

LogErrores
??? Id: int (PK)
??? Aplicacion: string
??? Mensaje: string
??? Stack: string
??? Equipo: string
??? Fecha: DateTime
??? Usuario: string
```

#### 2. Inventario
```
InventarioCasas
??? Manzana: string
??? Lote: string
??? Prototipo: string
??? FotoPath: string
??? DestajosTerminadosWBS: string (JSON)
??? FechaCreacion: DateTime

Casa (mapping)
??? Manzana
??? Lote
??? Prototipo
??? FotoPath
```

#### 3. Conceptos y Partidas
```
Conceptos
??? WBS: int (PK)
??? Codigo: string
??? Nombre: string
??? Etapa: string
??? CostoTunera: decimal
??? CostoCalandra: decimal
??? EsDinamica: bool
??? ValorM2Tunera: decimal
??? ValorM2Calandra: decimal
??? Activo: bool

Partidas (subgrupo de Conceptos)
??? (mismo esquema con nivel jerárquico)
```

#### 4. Destajos y Tareas
```
RutaTuneraDestajo
??? Nivel: int (0=Etapa, 1=SubEtapa, 2=Tarea)
??? Orden: int
??? Codigo: string
??? Nombre: string
??? Descripcion: string
??? ...

RutaCalandraDestajo
??? (mismo esquema para prototipo Calandra)

Destajos (instancias de tareas por casa)
??? Manzana: string
??? Lote: string
??? WBS: int
??? Estado: string (Activo/Inactivo/Finalizado)
??? ...
```

#### 5. Almacén
```
Insumos
??? Clave: string (PK)
??? Nombre: string
??? Cantidad: decimal
??? UnidadMedida: string
??? ...

SalidasAlmacen (tabla dinámica)
??? Id: int (PK)
??? Manzana: string
??? Lote: string
??? WBS: int
??? Clave: string
??? Cantidad: decimal
??? FechaSalida: DateTime
??? NumeroVale: int
??? Usuario: string

EntradasAlmacen
??? (similar a SalidasAlmacen)
```

#### 6. Mano de Obra y Nómina
```
ManoObra
??? Id: int (PK)
??? Manzana: string
??? Lote: string
??? WBS: int
??? Clave: string (trabajador)
??? Nombre: string
??? Cantidad: decimal
??? ...

Trabajadores
??? Clave: string (PK)
??? Nombre: string
??? Apellido: string
??? TipoIdentificacion: string
??? Identificacion: string
??? Telefono: string
??? Direccion: string
??? FechaRegistro: DateTime

Nomina
??? Id: int (PK)
??? Trabajador: string (FK)
??? Monto: decimal
??? Fecha: DateTime
??? ...
```

#### 7. Órdenes de Compra
```
OrdenesCompra
??? NumeroOrden: int (PK)
??? Folio: string
??? Proveedor: string
??? Manzana: string
??? Lote: string
??? FechaCreacion: DateTime
??? EstadoOrden: string
??? ...

DetallesOrdenCompra
??? NumeroOrden: int (FK)
??? LineaItem: int
??? Insumo: string
??? Cantidad: decimal
??? PrecioUnitario: decimal
??? Subtotal: decimal
```

#### 8. Estimaciones
```
Estimaciones
??? Id: int (PK)
??? Manzana: string
??? Lote: string
??? Folio: string
??? Prototipo: string
??? FechaCreacion: DateTime
??? UsuarioCreo: string
??? ContenidoJSON: string (estructura completa)

DetallesEstimacion
??? EstimacionId: int (FK)
??? ConceptoWBS: int
??? Cantidad: decimal
??? Precio: decimal
??? Subtotal: decimal
```

#### 9. Evidencias Fotográficas
```
Evidencias
??? Id: int (PK)
??? Manzana: string
??? Lote: string
??? Titulo: string
??? FechaCaptura: DateTime
??? FotoBlob: varbinary(max)
??? Extension: string (jpg, png, etc.)
??? UsuarioCaptura: string
??? Descripcion: string
```

#### 10. Ruta Crítica
```
TareasRutaCritica
??? Id: string (PK)
??? Nombre: string
??? Descripcion: string
??? FechaInicio: DateTime
??? FechaFin: DateTime
??? Duracion: int
??? Dependencia: string (tarea predecesora)
??? Prototipo: string (Tunera o Calandra)
??? ...
```

### ?? Relaciones Principales
- Conceptos ? Partidas (jerarquía WBS)
- InventarioCasas ? Manzana+Lote (PK compuesta)
- Destajos ? Conceptos (WBS)
- SalidasAlmacen ? Insumos (Clave)
- Órdenes de Compra ? Detalles (1:N)
- Estimaciones ? Detalles (1:N)
- Trabajadores ? Nómina (1:N)
- Mano de Obra ? Trabajadores (por Clave)

### ?? Patrones de Persistencia
- **JSON en BD**: DestajosTerminadosWBS (CasaInventario), ContenidoJSON (Estimaciones)
- **Binary Large Object**: Fotos en Evidencias (varbinary)
- **Enumaciones**: Como strings (Estados de destajo: Activo/Inactivo/Finalizado)
- **Audit**: Campos FechaCreacion, UsuarioCreo generalmente presentes

---

## Flujo de Navegación

### ?? Flujo de Inicio (Main ? PanelPrincipal)

```
Program.Main()
    ?
ErrorLogger.Inicializar()
    ?
ThemeManager.InicializarAplicacion()
    ?
FormSplash (pantalla de carga)
    ?
Actualizador.VerificarYActualizarAsync() 
    (descarga de GitHub si hay update)
    ?
FormLogin (si no es DEBUG)
    ? (autenticación SQL + opcional API JWT)
    ?
PanelPrincipal (Dashboard principal)
    ?
App ejecutándose
```

### ?? Flujo de Autenticación

```
FormLogin.btnLogin_Click()
    ?
if (DEBUG && usuario/clave en App.config)
    ? Bypassear login, crear Global.UsuarioActual
else
    ?
QuerySQL("SELECT ClaveHash, Rol FROM Usuarios WHERE Nombre = @usuario")
    ?
PasswordHasher.Verificar(clave, hashAlmacenado)
    ?? if (válida) ? Guardar Global.UsuarioActual
    ?? if (necesitaRehash) ? Actualizar hash ARGON2
    ?? else ? mostrar error
    ?
(Opcional) ApiClient.Login(usuario, clave)
    ? obtener JWT Token
    ?
Abrir PanelPrincipal
```

### ?? Flujo Principal del Dashboard (PanelPrincipal)

```
PanelPrincipal._Load()
    ?
CargarInventarioAlInicio()
    ?? InventarioService.LeerInventarioCasasSQL()
    ?? Llenar árbol de manzanas/lotes/casas
    ?
BuildMainMenu()
    ?? Menú: Estimación, Almacén, Tareas, etc.
    ?? Conectar event handlers
    ?
ConfigurarUIModerna()
    ?? KPIs animados
    ?? Overlay del mapa
    ?? Pin pulsante
    ?
InicializarSistemaMapa()
    ?? Cargar imagen mapa.jpg
    ?? Configurar zoom & pan
    ?? Renderizado optimizado
    ?
Usuario selecciona acción:
    ?? Seleccionar Manzana/Lote
    ?   ? cmbManzana_SelectedIndexChanged()
    ?   ?? Refrescar vistas (pintar casas en mapa)
    ?? Clic en Botón Menú (Estimación, Almacén, etc.)
    ?   ? Ver flujos específicos abajo
    ?? Clic en Casa en Mapa
        ? pintarCasasEnMapa_MouseClick()
        ?? Abrir detalles de casa
```

### ?? Flujo: ESTIMACIÓN DE CONCEPTOS

```
PanelPrincipal ? Botón "Estimación"
    ?
FormEstimacionConceptoMigrado()
    ?
CargarManzanas() ? Llenar combo
    ?
Usuario selecciona Manzana/Lote
    ?
CargarConceptosPartidas()
    ?? QuerySQL conceptos/partidas por prototipo
    ?? Llenar TreeListView jerárquico
    ?
Usuario marca partidas a incluir en estimación
    ?
Usuario puede:
    ?? AgregarConcepto()
    ?   ? FormAgregarConcepto ? nuevo concepto
    ?? EditarConcepto()
    ?   ? FormPropiedadesConcepto ? editar propiedades
    ?? AgregarEvidencias()
    ?   ? FormEvidenciasFotograficas ? adjuntar fotos
    ?? GenerarPDF()
        ? FormEstimacionConceptoMigrado_ExtensionPDF
        ?? Construir XML/PDF de estimación
        ?? Guardar folio
        ?? Guardar en BD (tabla Estimaciones)
        ?? Mostrar preview (FormPdfPreview)
    ?
Usuario puede:
    ?? Guardar en BD (sin PDF)
    ?? Generar PDF
    ?? Ver preview
    ?? Cerrar
```

### ?? Flujo: ALMACÉN

```
PanelPrincipal ? Botón "Almacén"
    ?
FormAlmacen()
    ?
Usuario selecciona:
    ?? ENTRADAS (compras recibidas)
    ?   ? FormAlmacen_Entradas.cs
    ?   ?? Seleccionar orden de compra
    ?   ?? Ingresar cantidad recibida
    ?   ?? Actualizar inventario
    ?   ?? Generar comprobante
    ?? SALIDAS (liberación para destajos)
    ?   ?? MANUAL (FormAlmacen_Salidas.cs)
    ?   ?   ?? Seleccionar manzana/lote/destajo
    ?   ?   ?? Seleccionar insumos
    ?   ?   ?? Ingresar cantidades
    ?   ?   ?? SalidaAlmacenService.GenerarVale()
    ?   ?   ?? Generar PDF vale
    ?   ?? AUTOMÁTICA (desde FormActivarTareasTreeList)
    ?       ?? Al activar destajo
    ?       ?? SalidaAlmacenService.LiberarInsumosAlActivarDestajo()
    ?       ?? Crear vales automáticamente
    ?? INVENTARIO (consulta actual)
    ?   ? FormAlmacen_Inventario.cs
    ?   ?? Vista de stock por insumo
    ?   ?? Alertas de bajo stock
    ?? HISTORIAL (movimientos pasados)
    ?   ? FormAlmacen_Historial.cs
    ?   ?? QuerySQL historial completo
    ?   ?? Filtros por fecha, insumo, etc.
    ?? CONSULTA POR CASA
        ? FormAlmacen_ConsultaCasa.cs
        ?? Ver qué se sacó para esta casa
        ?? Detalle de vales por destajo
```

### ?? Flujo: ACTIVACIÓN DE DESTAJOS (Tareas)

```
PanelPrincipal ? Botón "Tareas"
    ?
FormActivarTareasTreeList()
    ?
CargarManzanas() ? Llenar combo
    ?
Usuario selecciona Manzana/Lote/Prototipo
    ?
CargarDestajosPorCasa()
    ?? QuerySQL destajos activos/inactivos
    ?? Llenar TreeListView
    ?
Usuario interactúa:
    ?? Click en destajo ? Panel lateral abre asistente
    ?   ?? PanelGuia: Mostrar descripción
    ?   ?? PanelPasos: Mostrar secuencia de tareas
    ?   ?? PanelInsumos/Mano: Detalles requeridos
    ?? Botón "Activar" en destajo
    ?   ?? Cambiar estado BD a "Activo"
    ?   ?? SalidaAlmacenService.LiberarInsumosAlActivarDestajo()
    ?   ?   ?? Crear vales de salida automáticamente
    ?   ?? Mostrar asistente detallado
    ?? Botón "Finalizar" en destajo
    ?   ?? Cambiar estado BD a "Finalizado"
    ?   ?? Registrar fecha finalización
    ?   ?? Actualizar InventarioCasas.DestajosTerminadosWBS
    ?? Botón "Reabrir" en destajo
        ?? Si fue finalizado
        ?? Cambiar estado de vuelta a "Activo"
        ?? Posibilidad de reversión de vales
    ?
FormActivarTareasTreeList cierra
```

### ?? Flujo: MANO DE OBRA Y JORNALES

```
FormActivarTareasTreeList ? Clic en Destajo
    ?
Panel Lateral ? Tab "Insumos" o "Mano"
    ?
Botón "Asignar Mano de Obra"
    ?
FormManoObra()
    ?
CargarManoObraForPrototipo()
    ?? QuerySQL mano obra estándar por prototipo
    ?? Llenar DataGridView
    ?
Usuario:
    ?? Selecciona trabajadores a asignar
    ?? Ingresa cantidad (días/horas)
    ?? Presiona OK
    ?
GuardarManoObra()
    ?? InsertSQL a tabla ManoObra
    ?? Actualizar InventarioService.ObtenerManoObraEntregada()
    ?
FormManoObra cierra
    ?
(Posterior) FormDistribucionNomina
    ?? Agrupar mano obra por trabajador
    ?? Calcular jornal total
    ?? Generar nómina (PDF)
```

### ?? Flujo: COMPRAS Y ÓRDENES

```
PanelPrincipal ? Botón "Compra Directa" o "Indirecta"
    ?
FormCompraMulti (Multi) o FormCompraIndirecta (Indirecta)
    ?
Compra Directa:
    ?? Seleccionar manzana/lote/destajo
    ?? Agregar insumos ? FormAgregarInsumoCarrito()
    ?? Especificar proveedor y cantidad
    ?? Botón "Generar Orden"
    ?   ? FormGenerarOrden()
    ?   ?? Mostrar resumen
    ?   ?? Generar PDF orden
    ?   ?? FolioManager.GenerarFolio("OrdenCompra")
    ?   ?? InsertSQL a OrdenesCompra
    ?   ?? Guardar PDF en repositorio
    ?   ?? Mostrar preview
    ?? Cierre
    ?
Compra Indirecta:
    ?? Similares, pero sin WBS específico
    ?? Registro más libre
    ?? Usar FormAgregarInsumoIndirecto()
```

### ?? Flujo: REPORTES

```
PanelPrincipal ? Botón "Reportes"
    ?
Opciones:
    ?? Reporte General
    ?   ? FormReporte()
    ?   ?? Selector de criterios
    ?   ?? Generar PDF/Excel
    ?? Reporte Destajos Semana
    ?   ? FormReporteDestajosSemana()
    ?   ?? Destajos completados en últimos 7 días
    ?   ?? Detalle de horas/mano de obra
    ?? Reporte Nómina
    ?   ? FormReporteNomina()
    ?   ?? Jornales por período
    ?   ?? Salario calculado
    ?? Visor Recibos Nómina
        ? FormVisorRecibosNomina()
        ?? Histórico de recibos PDF
        ?? Descarga individual
```

### ?? Flujo: REPOSITORIOS (Búsqueda Histórica)

```
PanelPrincipal ? Botón "Repositorio"
    ?
Seleccionar tipo:
    ?? Estimaciones ? FormRepositorioPDFs()
    ?? Órdenes de Compra ? FormRepositorioPDFsOrdenesCompra()
    ?? Vales Salida ? FormRepositorioValesSalida()
    ?? Destajos ? FormRepositorioDestajos()
    ?? Evidencias ? FormEvidenciasFotograficas()
    ?
Búsqueda:
    ?? Filtrar por fecha/manzana/lote/número
    ?? Listar coincidencias
    ?? Clic en item ? descargar/ver PDF
    ?? Opciones: descargar, eliminar (si permisos)
```

### ?? Flujo: EDICIÓN DE RUTAS (Destajos Personalizados)

```
PanelPrincipal ? Botón "Editar Rutas"
    ?
FormEditorTreeList()
    ?
ComboBoxRutas: Seleccionar tabla
    ?? RutaTuneraDestajo
    ?? RutaCalandraDestajo
    ?
CargarNodosDesdeSQL()
    ?? Estructura de 3 niveles: Etapa ? SubEtapa ? Tarea
    ?? Llenar TreeListView
    ?
Usuario puede:
    ?? Agregar nodo
    ?   ?? Clic derecha ? "Nuevo hijo"
    ?   ?? Ingresar nombre/propiedades
    ?   ?? InsertSQL
    ?? Editar nodo
    ?   ?? Doble clic
    ?   ?? Modificar campos
    ?   ?? UpdateSQL
    ?? Eliminar nodo
    ?   ?? Clic derecha ? "Eliminar"
    ?   ?? DeleteSQL (si no hay dependencias)
    ?? Reordenar (drag & drop)
    ?   ?? Arrastrar nodo
    ?   ?? UpdateSQL (orden)
    ?? Guardar
        ?? GuardarTodoAsincrónico()
        ?? Cierre
```

### ?? Flujo: ADMINISTRACIÓN

```
PanelPrincipal ? Botón "Admin" (si es admin)
    ?
FormAdministrativos() o menú contextual
    ?
Opciones (según permisos):
    ?? Gestionar usuarios ? FormRegistrarUsuario()
    ?? Ver logs ? FormLogErrores()
    ?? Diagnóstico ? FormDiagnosticoConexion()
    ?   ?? Probar conexión SQL
    ?   ?? Probar conexión API
    ?   ?? Mostrar estado
    ?? Actualizar aplicación ? Manual trigger Actualizador
    ?? Configuración ? App.config (requiere reinicio)
```

### ?? Flujo: ACTUALIZACIÓN AUTOMÁTICA

```
Program.Main()
    ?
Actualizador.VerificarYActualizarAsync()
    ?? Leer VersionLocal (version.txt o hardcoded)
    ?? QueryAPI GitHub releases
    ?? Comparar versiones
    ?? if (hay update)
    ?   ?? Descargar .zip desde GitHub
    ?   ?? Verificar hash (si RequireUpdateHash=true)
    ?   ?? Verificar firma digital (si thumbprint especificado)
    ?   ?? Extraer a carpeta temporal
    ?   ?? Lanzar Updater.exe
    ?   ?   ?? (Updater reemplaza archivos, reinicia app)
    ?   ?? Esperar actualización
    ?? Continuar ejecución o mostrar FormProgresoActualizacion()
```

---

## Módulos y Capas

### ??? Arquitectura en Capas

```
???????????????????????????????????????????
?    PRESENTATION LAYER (WinForms UI)     ?
?  - Formularios (.cs + .Designer.cs)     ?
?  - Controles customizados                ?
?  - Validación en UI                      ?
???????????????????????????????????????????
               ?
???????????????????????????????????????????
?    BUSINESS LOGIC LAYER (Servicios)     ?
?  - InventarioService                    ?
?  - SalidaAlmacenService                 ?
?  - GestorEvidencias                     ?
?  - GestorFotosConcepto                  ?
?  - FolioManager                          ?
?  - Reportes                              ?
?  - ApiClient (híbrido)                   ?
???????????????????????????????????????????
               ?
???????????????????????????????????????????
?    DATA ACCESS LAYER (SQL + API)        ?
?  - SqlConnection directo (legacy)        ?
?  - ApiClient (HttpClient)               ?
?  - Query builders                        ?
???????????????????????????????????????????
               ?
???????????????????????????????????????????
?    DATA SOURCES                         ?
?  - SQL Server 100.75.234.9              ?
?  - Web API http://100.75.234.9:8733    ?
?  - File system (fotos, PDFs, logs)      ?
???????????????????????????????????????????
```

### ?? Módulos Funcionales

#### Módulo 1: AUTENTICACIÓN Y SEGURIDAD
**Responsables**:
- FormLogin.cs
- PasswordHasher.cs
- Global.cs
- ErrorLogger.cs (logging de intentos)

**Flujo**:
1. Validar credenciales en BD (Usuarios tabla)
2. Hash ARGON2 de contraseña
3. (Opcional) Obtener JWT del API
4. Establecer Global.UsuarioActual
5. Cargar permisos

---

#### Módulo 2: INVENTARIO Y ALMACÉN
**Responsables**:
- InventarioService.cs
- SalidaAlmacenService.cs
- FormAlmacen*.cs (7 archivos)
- FormAlmacen_Entradas.cs
- FormAlmacen_Salidas.cs
- FormAlmacen_Historial.cs
- FormAlmacen_Inventario.cs
- FormAlmacen_ConsultaCasa.cs

**Tablas BD**:
- InventarioCasas
- Insumos
- SalidasAlmacen
- EntradasAlmacen

**Flujo**: Entrada ? Stock ? Salida ? Histórico

---

#### Módulo 3: ESTIMACIÓN Y CONCEPTOS
**Responsables**:
- FormEstimacionConceptoMigrado*.cs (8 archivos)
- FormAgregarConcepto.cs
- FormGestionarPartidas.cs
- FormPropiedadesConcepto.cs
- FolioManager.cs

**Tablas BD**:
- Conceptos
- Estimaciones
- DetallesEstimacion

**Flujo**: Seleccionar manzana/lote ? Elegir partidas ? Generar PDF ? Guardar folio

---

#### Módulo 4: ACTIVACIÓN DE DESTAJOS
**Responsables**:
- FormActivarTareasTreeList*.cs (5 archivos)
- FormEditorTreeList*.cs (4 archivos)
- SalidaAlmacenService.cs (liberación automática)
- FormPropiedadesDestajo.cs

**Tablas BD**:
- RutaTuneraDestajo / RutaCalandraDestajo
- Destajos
- SalidasAlmacen (automáticas)

**Flujo**: Seleccionar casa ? Mostrar destajos ? Activar ? Liberar insumos ? Asignar mano de obra

---

#### Módulo 5: MANO DE OBRA Y NÓMINA
**Responsables**:
- FormManoObra.cs
- FormRegistrarTrabajador.cs
- FormPerfilTrabajador.cs
- FormDistribucionNomina.cs
- FormAsignarNomina.cs
- FormReporteNomina.cs

**Tablas BD**:
- ManoObra
- Trabajadores
- Nomina

**Flujo**: Registrar trabajador ? Asignar a destajo ? Calcular jornal ? Generar nómina

---

#### Módulo 6: COMPRAS Y ÓRDENES
**Responsables**:
- FormCompraMulti*.cs (2 archivos)
- FormCompraIndirecta*.cs (2 archivos)
- FormAgregarProveedor.cs
- FormGenerarOrden.cs
- FormDetalleOrdenCompra.cs
- FormRepositorioPDFsOrdenesCompra.cs

**Tablas BD**:
- OrdenesCompra
- DetallesOrdenCompra
- Proveedores

**Flujo**: Crear orden ? Especificar insumos ? Generar PDF ? Guardar en repositorio

---

#### Módulo 7: EVIDENCIAS Y FOTOS
**Responsables**:
- GestorEvidencias.cs
- FormEvidenciasFotograficas.cs
- GestorFotosConcepto.cs
- FormEstimacionConceptoMigrado_ExtensionEvidencias.cs

**Tablas BD**:
- Evidencias

**API**: EvidenciasController (Calandria.Api)

**Flujo**: Capturar foto ? Asociar a manzana/lote ? Guardar en BD ? Consultar en repositorio

---

#### Módulo 8: REPORTES Y ANÁLISIS
**Responsables**:
- Reportes.cs
- FormReporte.cs
- FormReporteDestajosSemana.cs
- FormReporteNomina.cs
- FormVisorRecibosNomina.cs
- GenerarExplosionInsumosPDF.cs

**Salidas**: PDF, Excel

**Flujo**: Especificar criterios ? Generar ? Descargar

---

#### Módulo 9: RUTA CRÍTICA Y GANTT
**Responsables**:
- FormRutaCritica.cs
- TareaRutaCritica.cs (modelo)
- Controles DlhSoft.* (Gantt WPF)

**Propósito**: Visualización de cronograma del proyecto

**Nota**: Usa WPF embebido en WinForms

---

#### Módulo 10: SISTEMA DE ACTUALIZACIONES
**Responsables**:
- Actualizador.cs
- UpdateServer.cs
- FormProgresoActualizacion.cs

**Fuente**: GitHub (CalandriaApp repo)

**Mecanismo**:
1. Verificar releases en GitHub API
2. Descargar .zip
3. Verificar hash (opcional)
4. Verificar firma Authenticode (opcional)
5. Lanzar Updater.exe
6. Reemplazar archivos
7. Reiniciar app

---

#### Módulo 11: LOGGING Y DIAGNÓSTICO
**Responsables**:
- ErrorLogger.cs
- FormLogErrores.cs
- FormDiagnosticoConexion.cs

**Almacenamiento**:
- Tabla LogErrores (BD)
- Archivo errores.log (%LocalAppData%/...)

**Propósito**: Registrar excepciones no manejadas, diagnosticar conectividad

---

#### Módulo 12: TEMATIZACIÓN Y UI
**Responsables**:
- ThemeManager.cs
- PanelPrincipal.ModernExtensions.cs
- FormAlmacen_Theme.cs
- Diversos *.Designer.cs

**Propósito**:
- MaterialSkin (Material Design)
- Colores corporativos (café, crema)
- Responsive layout
- Animaciones (zoom, pan, KPIs)

---

### ?? Dependencias Entre Módulos

```
Autenticación (1)
    ?
Panel Principal (Dashboard)
    ??? Inventario/Almacén (2)
    ?   ?? Destajos (4)
    ?       ??? Mano de Obra (5)
    ?       ??? Insumos (relacionado 2)
    ??? Estimación (3)
    ?   ??? Conceptos/Partidas
    ?   ??? Evidencias (7)
    ?   ??? Fotos
    ??? Compras (6)
    ?   ??? Órdenes
    ??? Reportes (8)
    ??? Ruta Crítica (9)
    ??? Administración (Logging 11, UI 12)

Actualización (10) - independiente, llama en Main
```

---

### ?? Estados y Transiciones

#### Ciclo de Vida de un Destajo

```
INACTIVO (creado, pero no iniciado)
    ? [Botón Activar]
ACTIVO (en progreso)
    ?? Se liberan insumos automáticamente
    ?? Se asigna mano de obra
    ?? Se registran evidencias
    ? [Botón Finalizar]
FINALIZADO (completado)
    ?? Marcado en InventarioCasas.DestajosTerminadosWBS
    ? [Botón Reabrir] (si permisos)
ACTIVO (de vuelta)
```

#### Ciclo de Vida de una Orden de Compra

```
BORRADOR (creada, sin PDF)
    ? [Generar PDF]
PENDIENTE (PDF generado, en repositorio)
    ?? Esperando entrega
    ?? Envío a proveedor
    ?? Registro de entrada
    ? [Registrar Entrada]
ENTREGADA (insumos recibidos)
    ? [Surtida] (insumos liberados a destajos)
CERRADA
```

---

## ?? Resumen de Estadísticas

- **Formularios**: 60+ formularios
- **Clases de Dominio**: 10+ modelos principales
- **Servicios**: 12+ servicios principales
- **Dependencias NuGet**: 60+ paquetes
- **Tablas BD**: 15+ tablas identificadas
- **Proyectos**: 2 (DynamicSepticSystem + Updater)

---

## ?? Conclusión

El sistema **DynamicSepticSystem** es una aplicación empresarial compleja de gestión de proyectos residenciales, construida sobre .NET Framework 4.7.2 con una arquitectura híbrida que integra:

1. **SQL Server directo** para datos legacy
2. **Web API REST** para módulos migrados
3. **WinForms** para interfaz de usuario
4. **Generación de PDF** para reportes
5. **Autenticación JWT** (opcional)
6. **Actualización automática** desde GitHub

La aplicación está bien estructurada en módulos funcionales, con clara separación de capas y responsabilidades, facilitando mantenimiento, testing y futuras migraciones.

