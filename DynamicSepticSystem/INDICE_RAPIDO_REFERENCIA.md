# ÍNDICE RÁPIDO DE REFERENCIA - DynamicSepticSystem

## ?? Ubicación Rápida de Componentes

### ?? Necesito encontrar...

#### Formulario Principal
- **Dashboard**: `PanelPrincipal.cs`
- **Login**: `FormLogin.cs`
- **Splash Screen**: `FormSplash.cs`

#### Estimación
- **Crear Estimación**: `FormEstimacionConceptoMigrado.cs` (8 archivos parciales)
  - Menú contextual: `FormEstimacionConceptoMigrado.MenuContextual.cs`
  - TreeListView: `FormEstimacionConceptoMigrado.TreeListView.cs`
  - Carga datos: `FormEstimacionConceptoMigrado.CargaDatos.cs`
  - PDF: `FormEstimacionConceptoMigrado_ExtensionPDF.cs`
  - Evidencias: `FormEstimacionConceptoMigrado_ExtensionEvidencias.cs`
  - Folios: `FormEstimacionConceptoMigrado_ExtensionFolios.cs`
- **Agregar Concepto**: `FormAgregarConcepto.cs`
- **Gestionar Partidas**: `FormGestionarPartidas.cs` (3 archivos parciales)
- **Propiedades Concepto**: `FormPropiedadesConcepto.cs`

#### Almacén
- **Principal**: `FormAlmacen.cs` (7 archivos parciales)
  - Core: `FormAlmacen_Core.cs`
  - Entradas: `FormAlmacen_Entradas.cs`
  - Salidas: `FormAlmacen_Salidas.cs`
  - Historial: `FormAlmacen_Historial.cs`
  - Inventario: `FormAlmacen_Inventario.cs`
  - Consulta Casa: `FormAlmacen_ConsultaCasa.cs`
  - Layout/Theme: `FormAlmacen_Layout.cs`, `FormAlmacen_Theme.cs`

#### Destajos/Tareas
- **Activar Tareas**: `FormActivarTareasTreeList.cs` (5 archivos parciales)
  - Panel Guía: `FormActivarTareasTreeList.PanelGuia.cs`
  - Panel Pasos: `FormActivarTareasTreeList.PanelPasos.cs`
  - Panel Insumos: `FormActivarTareasTreeList.Insumos.cs`
  - Panel Almacén: `FormActivarTareasTreeList.Almacen.cs`
- **Editar Rutas**: `FormEditorTreeList.cs` (4 archivos parciales)
  - Gestión de insumos: `FormEditorTreeList.GestorInsumos.cs`
  - Diálogos: `FormEditorTreeList.Dialogos.cs`
- **Propiedades Destajo**: `FormPropiedadesDestajo.cs`

#### Mano de Obra
- **Formulario**: `FormManoObra.cs`
- **Registrar Trabajador**: `FormRegistrarTrabajador.cs`
- **Perfil Trabajador**: `FormPerfilTrabajador.cs`
- **Distribuir Nómina**: `FormDistribucionNomina.cs`
- **Asignar Nómina**: `FormAsignarNomina.cs`
- **Reporte Nómina**: `FormReporteNomina.cs`
- **Visor Recibos**: `FormVisorRecibosNomina.cs`

#### Insumos
- **Formulario**: `FormInsumos.cs`
- **Agregar al Carrito**: `FormAgregarInsumoCarrito.cs`
- **Agregar Indirecto**: `FormAgregarInsumoIndirecto.cs`
- **Editar Explosiones**: `FormEditarExplosiones.cs`
- **Generar Explosión PDF**: `GenerarExplosionInsumosPDF.cs`

#### Compras
- **Compra Directa Multi**: `FormCompraMulti.cs` (2 archivos)
  - PDF Repositorio: `FormCompraMulti_ExtensionPDFRepositorio.cs`
- **Compra Indirecta**: `FormCompraIndirecta.cs` (2 archivos)
  - PDF Repositorio: `FormCompraIndirecta_ExtensionPDFRepositorio.cs`
- **Agregar Proveedor**: `FormAgregarProveedor.cs`
- **Generar Orden**: `FormGenerarOrden.cs`
- **Detalles Orden**: `FormDetalleOrdenCompra.cs`

#### Repositorios
- **PDFs General**: `FormRepositorioPDFs.cs`
- **PDFs Órdenes Compra**: `FormRepositorioPDFsOrdenesCompra.cs`
- **Vales Salida**: `FormRepositorioValesSalida.cs`
- **Destajos**: `FormRepositorioDestajos.cs`
- **Evidencias Fotográficas**: `FormEvidenciasFotograficas.cs`

#### Avance
- **Avance Obra**: `FormAvanceObra.cs` (2 archivos)
- **Avance Concepto**: `FormAvanceConcepto.cs` (2 archivos)

#### Reportes
- **General**: `FormReporte.cs`
- **Destajos Semana**: `FormReporteDestajosSemana.cs`
- **Generador**: `Reportes.cs`

#### Administración
- **Panel Admin**: `FormAdministrativos.cs`
- **Registrar Usuario**: `FormRegistrarUsuario.cs`
- **Log Errores**: `FormLogErrores.cs`
- **Diagnóstico**: `FormDiagnosticoConexion.cs`
- **Progreso Actualización**: `FormProgresoActualizacion.cs`

#### Ruta Crítica
- **Gantt**: `FormRutaCritica.cs`

---

## ?? Servicios y Helpers

### Acceso a Datos
- **Inventario**: `InventarioService.cs`
- **Salidas Almacén**: `SalidaAlmacenService.cs`

### Comunicación
- **API Client**: `ApiClient.cs` (estático)
- **Update Server**: `UpdateServer.cs`

### Gestión de Archivos
- **Evidencias**: `GestorEvidencias.cs`
- **Fotos Concepto**: `GestorFotosConcepto.cs`
- **Folios**: `FolioManager.cs`

### Sistema
- **Error Logger**: `ErrorLogger.cs` (estático)
- **Actualizador**: `Actualizador.cs` (estático)
- **Theme Manager**: `ThemeManager.cs` (estático)
- **Password Hasher**: `PasswordHasher.cs` (estático)

### Utilidades
- **Conversión Números**: `NumeroALetras.cs`
- **Redimensionamiento**: `Resizer.cs`
- **Badge Feature**: `NewFeatureBadge.cs`
- **Detalles Orden Helper**: `DetalleOrdenHelper.cs`
- **Reportes**: `Reportes.cs`

---

## ??? Modelos (Data Models)

- **Casa**: `Casa.cs`
- **CasaInventario**: `CasaInventario.cs`
- **NodoConcepto**: `NodoConcepto.cs`
- **PartidaConcepto**: `PartidaConcepto.cs`
- **ConceptoExistente**: `ConceptoExistente.cs`
- **PartidaDinamica**: `PartidaDinamica.cs`
- **NodoTree**: `NodoTree.cs`
- **NodoCategoria**: `NodoCategoria.cs`
- **TareaRutaCritica**: `TareaRutaCritica.cs`

---

## ?? Controles y Extensiones

- **SafeTreeListView**: `SafeTreeListView.cs` (wrapper seguro)
- **TreeGridViewLite**: `TreeGridViewLite.cs` (tree grid ligero)
- **Panel Principal Extensiones**: `PanelPrincipal.ModernExtensions.cs`
- **Panel Principal Designer**: `PanelPrincipal.Designer.cs`

---

## ?? Configuración

- **Archivo Configuración**: `App.config`
  - Connection String: `CalandriaConn`
  - API Base URL: `ApiBaseUrl`
  - GitHub Token: `GitHubToken`
  - Actualizador: `RequireUpdateHash`, `UpdateSignerThumbprint`

- **Assembly Info**: `Properties/AssemblyInfo.cs`
  - Versión: 1.3.7.0

- **Paquetes**: `packages.config` (60+ dependencias)

---

## ?? Diálogos y Prompts

- **Prompt Cantidad**: `PromptCantidad.cs`
- **Prompt Justificación**: `PromptJustificacion.cs`
- **Seleccionar Título**: `FormSeleccionarTitulo.cs`
- **Seleccionar Nivel**: `DialogSeleccionarNivel.cs`
- **Mapear Coordenadas**: `FormMapearCoordenadas.cs`
- **PDF Preview**: `FormPdfPreview.cs`, `FormPreviewPDFMaximizado.cs`
- **Hard Progress**: `FormHardProgress.cs`

---

## ?? Global y Punto de Entrada

- **Program.cs**: Punto de entrada Main
  - Inicializar ErrorLogger
  - Aplicar tema
  - Mostrar splash
  - Verificar actualizaciones
  - Mostrar login o admin bypass (DEBUG)
  - Abrir PanelPrincipal

- **Global.cs**: Variables globales
  - `Global.UsuarioActual`: Usuario en sesión
  - `Global.EsAdmin`: Verificar si es admin

---

## ?? Tablas de Base de Datos Principales

```sql
SELECT * FROM Usuarios
SELECT * FROM LogErrores
SELECT * FROM InventarioCasas
SELECT * FROM Conceptos
SELECT * FROM RutaTuneraDestajo
SELECT * FROM RutaCalandraDestajo
SELECT * FROM Destajos
SELECT * FROM Insumos
SELECT * FROM SalidasAlmacen
SELECT * FROM ManoObra
SELECT * FROM Trabajadores
SELECT * FROM Nomina
SELECT * FROM OrdenesCompra
SELECT * FROM DetallesOrdenCompra
SELECT * FROM Estimaciones
SELECT * FROM DetallesEstimacion
SELECT * FROM Evidencias
```

---

## ?? Flujos Clave (A Dónde Van)

### Usuario abre app
? `Program.cs` ? `FormSplash` ? `FormLogin` ? `PanelPrincipal`

### Usuario va a Estimación
? `PanelPrincipal` (menú) ? `FormEstimacionConceptoMigrado` ? PDF ? `FormPdfPreview`

### Usuario activa destajo
? `PanelPrincipal` (menú) ? `FormActivarTareasTreeList` ? `SalidaAlmacenService.LiberarInsumosAlActivarDestajo()`

### Usuario genera orden de compra
? `PanelPrincipal` (menú) ? `FormCompraMulti` ? `FormGenerarOrden` ? PDF ? repositorio

### Admin revisa errores
? `PanelPrincipal` (Admin) ? `FormLogErrores` ? QuerySQL LogErrores

### Sistema se actualiza
? `Program.Main()` ? `Actualizador.VerificarYActualizarAsync()` ? GitHub ? Updater.exe

---

## ?? API Client Endpoints (Migración)

```
POST   /api/auth/login                    ? LoginResponseApi
GET    /api/almacen/manzanas              ? List<Manzana>
GET    /api/evidencias/{manzana}/{lote}   ? List<EvidenciaInfo>
POST   /api/evidencias                    ? GuardarEvidencia
DELETE /api/evidencias/{id}               ? EliminarEvidencia
```

---

## ?? Autenticación

- **SQL Server**: Tabla `Usuarios` (hash ARGON2)
- **API JWT** (opcional): Calandria.Api /api/auth/login
- **DEBUG Bypass**: Credenciales en App.config (DebugUser, DebugPass)

---

## ?? Estructura de Proyecto

```
DynamicSepticSystem/
??? *.cs (Formularios, modelos, servicios)
??? Properties/
?   ??? AssemblyInfo.cs
?   ??? Resources.Designer.cs
?   ??? Settings.Designer.cs
??? WPF/
?   ??? PanelPrincipalWPF.xaml.cs (no activo)
??? obj/
?   ??? (compilación)
??? App.config
??? packages.config

Updater/
??? ProgramFramework.cs
??? Properties/AssemblyInfo.cs
??? (minimalist)
```

---

## ?? Puntos de Extensión Futura

1. **Migración a WPF**: Existe `PanelPrincipalWPF.xaml.cs` (experimental)
2. **API Migration**: Arquitectura híbrida ya lista (SQL + REST)
3. **Modularización**: FormAlmacen, FormEstimacionConceptoMigrado ya usan partial classes
4. **Asincronía**: Actualizador usa async/await
5. **Cloud**: Integración Tailscale ya presente (IPs 100.x.x.x)

---

## ?? Compilación y Deploy

- **Target Framework**: .NET Framework 4.7.2
- **Release**: Updater descarga desde GitHub (CalandriaApp)
- **Versión**: Se lee de `version.txt` o fallback `1.9.3.1-K`
- **Ejecutable**: `DynamicSepticSystem.exe`
- **Updater**: `Updater.exe` (reemplaza archivos, reinicia)

---

## ?? Contacto Código

- **Desarrollador**: LuxuImNot (GitHub)
- **Crédito**: "A software by LuxuDev" (pie del sidebar)
- **Licencia**: No especificada en código
- **Rama Activa**: `feature/api-migration`

