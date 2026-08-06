# MATRIZ DE FUNCIONALIDADES Y MÓDULOS

## ?? Matriz Principal de Módulos

| # | Módulo | Formularios | Servicios | BD Tables | Estado | Migración |
|---|--------|-------------|-----------|-----------|--------|-----------|
| 1 | **Autenticación** | FormLogin, FormRegistrarUsuario | PasswordHasher, ApiClient (JWT) | Usuarios | ? Completo | Hybrid (SQL+API) |
| 2 | **Inventario** | FormAlmacen* (7), FormAlmacen_ConsultaCasa | InventarioService | InventarioCasas, Insumos | ? Completo | SQL directo |
| 3 | **Almacén (CRUD)** | FormAlmacen_Entradas, Salidas, Historial, Inventario | SalidaAlmacenService | SalidasAlmacen, EntradasAlmacen | ? Completo | SQL directo |
| 4 | **Estimación** | FormEstimacionConceptoMigrado* (8), FormAgregarConcepto | FolioManager, Reportes | Estimaciones, DetallesEstimacion, Conceptos | ? Completo | SQL directo |
| 5 | **Conceptos/Partidas** | FormGestionarPartidas, FormPropiedadesConcepto | - | Conceptos, PartidaConcepto | ? Completo | SQL directo |
| 6 | **Destajos (Activación)** | FormActivarTareasTreeList* (5) | SalidaAlmacenService | Destajos, RutaTuneraDestajo, RutaCalandraDestajo | ? Completo | SQL directo |
| 7 | **Editar Rutas** | FormEditorTreeList* (4) | - | RutaTuneraDestajo, RutaCalandraDestajo | ? Completo | SQL directo |
| 8 | **Mano de Obra** | FormManoObra, FormRegistrarTrabajador, FormPerfilTrabajador | - | ManoObra, Trabajadores, Nomina | ? Completo | SQL directo |
| 9 | **Nómina** | FormDistribucionNomina, FormAsignarNomina, FormReporteNomina | - | Nomina, Trabajadores | ? Completo | SQL directo |
| 10 | **Insumos** | FormInsumos, FormAgregarInsumoCarrito, FormAgregarInsumoIndirecto | GestorFotosConcepto | Insumos, InsumosConcepto | ? Completo | SQL directo |
| 11 | **Órdenes de Compra** | FormCompraMulti*, FormCompraIndirecta*, FormGenerarOrden | FolioManager | OrdenesCompra, DetallesOrdenCompra | ? Completo | SQL directo |
| 12 | **Proveedores** | FormAgregarProveedor | - | Proveedores | ? Completo | SQL directo |
| 13 | **Evidencias Fotográficas** | FormEvidenciasFotograficas | GestorEvidencias | Evidencias | ?? Migrado | API (EvidenciasController) |
| 14 | **Avance de Obra** | FormAvanceObra* (2) | - | AvanceObra | ? Completo | SQL directo |
| 15 | **Avance de Concepto** | FormAvanceConcepto* (2) | - | AvanceConcepto | ? Completo | SQL directo |
| 16 | **Reportes** | FormReporte, FormReporteDestajosSemana, FormReporteNomina | Reportes | Varias | ? Completo | SQL directo |
| 17 | **Repositorio PDFs** | FormRepositorioPDFs, FormRepositorioPDFsOrdenesCompra | - | File System | ? Completo | File System |
| 18 | **Repositorio Vales** | FormRepositorioValesSalida | - | File System | ? Completo | File System |
| 19 | **Repositorio Destajos** | FormRepositorioDestajos | - | BD queries | ? Completo | SQL directo |
| 20 | **Ruta Crítica** | FormRutaCritica | - | TareasRutaCritica | ?? Parcial | WPF (experimental) |
| 21 | **Logging & Diagnóstico** | FormLogErrores, FormDiagnosticoConexion | ErrorLogger | LogErrores | ? Completo | SQL + File |
| 22 | **Actualización** | FormProgresoActualizacion | Actualizador, UpdateServer | - | ? Completo | GitHub API |
| 23 | **Tematización** | Todos | ThemeManager | - | ? Completo | In-memory |
| 24 | **Asignación Cuadrilla** | FormAsignarCuadrilla | - | Cuadrillas | ? Completo | SQL directo |
| 25 | **Destajos por Cuadrilla** | FormDestajosPorCuadrilla | - | Destajos+Cuadrillas | ? Completo | SQL directo |

---

## ?? Dependencias de Módulos

```
NIVEL 0 (Core Infrastructure)
?? ErrorLogger (global)
?? ThemeManager (global)
?? PasswordHasher (security)
?? Global (state)
?? ApiClient (communication)

NIVEL 1 (Servicios de Acceso a Datos)
?? InventarioService
?? SalidaAlmacenService
?? GestorEvidencias
?? GestorFotosConcepto
?? FolioManager

NIVEL 2 (Formularios Principales - Lectura/Consulta)
?? PanelPrincipal (dashboard)
?? FormLogin (authentication)
?? FormAdministrativos (admin)
?? FormRepositorioPDFs (search)
?? FormRepositorioValesSalida (search)
?? FormRepositorioDestajos (search)

NIVEL 3 (Formularios CRUD - Modificación de Datos)
?? FormEstimacionConceptoMigrado (crear estimaciones)
?? FormAlmacen (entradas/salidas)
?? FormActivarTareasTreeList (activar destajos)
?? FormEditorTreeList (editar rutas)
?? FormCompraMulti (crear órdenes)
?? FormManoObra (asignar jornales)
?? FormEvidenciasFotograficas (subir fotos)

NIVEL 4 (Formularios Auxiliares - Pop-ups/Dialogs)
?? FormAgregarConcepto (desde FormEstimacionConceptoMigrado)
?? FormGenerarOrden (desde FormCompraMulti)
?? FormPdfPreview (desde varios)
?? PromptCantidad (desde varios)
?? PromptJustificacion (desde varios)
?? Etc.

DEPENDENCIAS TRANSVERSALES:
?? Todos los CRUD ? ErrorLogger
?? Todos los Forms ? ThemeManager
?? FormEstimacion* ? FormAgregarConcepto, FormPropiedadesConcepto
?? FormEstimacion* ? GestorEvidencias, GestorFotosConcepto
?? FormEstimacion* ? FolioManager
?? FormAlmacen* ? SalidaAlmacenService
?? FormActivarTareas ? SalidaAlmacenService (liberar automático)
?? FormActivarTareas ? FormEditorTreeList (consultar rutas)
?? FormActivarTareas ? FormManoObra (asignar)
?? FormCompraMulti ? FormGenerarOrden, FolioManager
?? FormLogin ? ApiClient (JWT opcional)
?? Actualización (Main) ? Actualizador ? GitHub API
```

---

## ?? Matriz de Funcionalidades por Formulario

### Estimación (5 formularios + 3 extensiones)

| Formulario | Funcionalidad Clave | Entrada | Salida | BD Escribe |
|---|---|---|---|---|
| FormEstimacionConceptoMigrado | Selector manzana/lote, treelist conceptos | Manzana, Lote, Prototipo | TreeListView jerárquico | No (draft) |
| FormEstimacionConceptoMigrado.MenuContextual | Menú derecho (agregar, editar concepto) | Click derecho | Opciones contextuales | No |
| FormEstimacionConceptoMigrado.TreeListView | Configuración visual del árbol | - | TreeListView renderizado | No |
| FormEstimacionConceptoMigrado.CargaDatos | QuerySQL conceptos y partidas | WBS, Prototipo | List<NodoConcepto> | No |
| FormEstimacionConceptoMigrado.Preview | Generación de vista previa PDF | Datos seleccionados | PDF en memoria | No |
| FormEstimacionConceptoMigrado.Avances | Cálculo de metros cuadrados dinámicos | Metros de entrada | Recálculo costos | No |
| FormEstimacionConceptoMigrado.AgregarConcepto | Crear nuevos conceptos en la estimación | Datos concepto | Nuevo NodoConcepto | No |
| FormEstimacionConceptoMigrado_ExtensionPDF | Generar PDF final | Estructura estimación | PDF file | Sí (folio) |
| FormEstimacionConceptoMigrado_ExtensionEvidencias | Selector de evidencias fotográficas | Foto WBS | Lista de fotos adjuntas | No (solo en BD al guardar) |
| FormEstimacionConceptoMigrado_ExtensionFolios | Asignación de folio y guardado | Datos completos | Folio asignado | Sí (INSERT Estimaciones) |

### Almacén (7 formularios)

| Formulario | Funcionalidad | Entrada | Salida | BD Escribe |
|---|---|---|---|---|
| FormAlmacen_Core | Tabs coordinator | User clicks | Switch tabs | No |
| FormAlmacen_Entradas | Registrar entrada de insumos | Orden, cantidad | ComprobantePDF | Sí (EntradasAlmacen, actualizar Insumos) |
| FormAlmacen_Salidas | Manual vale de salida | Manzana, Lote, WBS, insumos | ValePDF | Sí (SalidasAlmacen, actualizar Insumos) |
| FormAlmacen_Historial | Consultar movimientos pasados | Filtros (fecha, insumo) | Tabla histórico | No |
| FormAlmacen_Inventario | Ver stock actual | - | DataGridView stock | No |
| FormAlmacen_ConsultaCasa | Qué se sacó para esta casa | Manzana, Lote | Movimientos por casa | No |
| FormAlmacen_Layout & Theme | Configuración visual | - | Estilos aplicados | No |

### Activación de Destajos (5 formularios)

| Formulario | Funcionalidad | Entrada | Salida | BD Escribe |
|---|---|---|---|---|
| FormActivarTareasTreeList | Principal: cargar y listar destajos | Manzana/Lote selector | TreeListView destajos | No (dispatch a servicios) |
| FormActivarTareasTreeList.PanelGuia | Panel lateral: descripción destajo | Click en destajo | Texto descriptivo | No |
| FormActivarTareasTreeList.PanelPasos | Panel lateral: pasos del destajo | - | Secuencia de tareas | No |
| FormActivarTareasTreeList.Insumos | Panel lateral: insumos requeridos | - | Tabla insumos WBS | No |
| FormActivarTareasTreeList.Almacen | Botón "Liberar insumos" | Click | SalidaAlmacenService.LiberarInsumosAlActivarDestajo() | Sí (indirecto) |

### Compra (4 formularios + 2 extensiones)

| Formulario | Funcionalidad | Entrada | Salida | BD Escribe |
|---|---|---|---|---|
| FormCompraMulti | Selector proveedor, agregar insumos | Manzana/Lote, insumo, cantidad | Carrito compra | No |
| FormCompraMulti_ExtensionPDFRepositorio | Generar PDF orden | Carrito | OrdenPDF | Sí (INSERT OrdenesCompra + detalles) |
| FormCompraIndirecta | Similares a FormCompraMulti | Insumo, cantidad (sin WBS) | Carrito | No |
| FormCompraIndirecta_ExtensionPDFRepositorio | Generar PDF orden indirecta | Carrito | OrdenPDF | Sí (INSERT OrdenesCompra) |
| FormGenerarOrden | Preview + confirmación | Datos orden | Folio asignado | Sí (FolioManager) |
| FormAgregarProveedor | Crear nuevo proveedor | Nombre, contacto | Proveedor en DD | Sí (INSERT Proveedores) |

### Editar Rutas (4 formularios)

| Formulario | Funcionalidad | Entrada | Salida | BD Escribe |
|---|---|---|---|---|
| FormEditorTreeList | Principal: combos tabla + treelist | Combo RutaTunera/Calandra | TreeListView jerárquico | No (draft) |
| FormEditorTreeList.Dialogos | Agregar/Editar/Eliminar nodos | Form datos | Cambios en memoria | No |
| FormEditorTreeList.GestorInsumos | Editor insumos por nodo | Insumos lista | NodoTree.Insumos actualizado | No |
| Guardado | Guardar TODO a BD | Click Guardar | Confirmación | Sí (UPDATE/INSERT/DELETE múltiples) |

### Mano de Obra (7 formularios)

| Formulario | Funcionalidad | Entrada | Salida | BD Escribe |
|---|---|---|---|---|
| FormManoObra | Selector trabajadores para destajo | Destajo WBS | DataGridView trabajadores | No |
| FormManoObra (Guardar) | Persistir asignación | Trabajadores marcados | Confirmación | Sí (INSERT ManoObra) |
| FormRegistrarTrabajador | CRUD trabajadores | Datos completos | Confirmación | Sí (INSERT/UPDATE/DELETE Trabajadores) |
| FormPerfilTrabajador | Ver/editar perfil | Clave trabajador | Form datos | Sí (UPDATE Trabajadores) |
| FormDistribucionNomina | Calcular jornales por período | Rango fechas, trabajador | Tabla nómina calculada | No |
| FormAsignarNomina | Asignar jornal específico | Trabajador, monto, fecha | Confirmación | Sí (INSERT Nomina) |
| FormReporteNomina | Generar reporte | Período | NominaPDF | No (PDF en file system) |

### Reportes (4 formularios)

| Formulario | Funcionalidad | Entrada | Salida | BD Escribe |
|---|---|---|---|---|
| FormReporte | Selector criterios genérico | Tipo reporte, filtros | ReportePDF/Excel | No |
| FormReporteDestajosSemana | Destajos finalizados últimos 7 días | - | Tabla + PDF | No |
| FormReporteNomina | Nómina por período | Rango fechas, trabajador | NominaPDF | No |
| FormVisorRecibosNomina | Descargar recibos históricos | Clave trabajador, mes | ReciboPDF (descargable) | No |

---

## ?? Matriz de Permisos

| Operación | Admin | Supervisor | Usuario | Visualizador |
|---|---|---|---|---|
| Login | ? | ? | ? | ? |
| Ver Dashboard | ? | ? | ? | ? |
| Crear Estimación | ? | ? | ? | ? |
| Editar Estimación | ? | ? | ? (propia) | ? |
| Eliminar Estimación | ? | ? | ? | ? |
| Ver Almacén | ? | ? | ? | ? |
| Entrada Almacén | ? | ? | ? | ? |
| Salida Almacén (Manual) | ? | ? | ? | ? |
| Activar Destajo | ? | ? | ? | ? |
| Finalizar Destajo | ? | ? | ? | ? |
| Reabrir Destajo | ? | ? | ? | ? |
| Editar Rutas (Destajos) | ? | ? | ? | ? |
| Crear Orden de Compra | ? | ? | ? | ? |
| Registrar Trabajador | ? | ? | ? | ? |
| Asignar Nómina | ? | ? | ? | ? |
| Generar Reportes | ? | ? | ? | ? |
| Ver Logs de Error | ? | ? | ? | ? |
| Diagnóstico Sistema | ? | ? | ? | ? |
| Actualizar Aplicación (manual) | ? | ? | ? | ? |
| Consultar Repositorios | ? | ? | ? | ? |
| Eliminar de Repositorio | ? | ? | ? | ? |

---

## ?? Flujo de Datos por Funcionalidad

### Funcionalidad: Crear y Generar Estimación

```
Iniciador: Usuario admin/supervisor/usuario normal
Tiempo estimado: 20-30 minutos

Pasos:
1. PanelPrincipal ? Click "Estimación"
2. FormEstimacionConceptoMigrado.Show()
3. Seleccionar Manzana/Lote
4. QuerySQL ? Cargar conceptos/partidas
5. TreeListView: marcar partidas
6. (Opcional) FormAgregarConcepto: crear nuevo concepto
7. Click "Generar PDF"
8. FormEstimacionConceptoMigrado_ExtensionPDF:
   ?? Construir documento en memoria
   ?? PDFsharp rendering
   ?? FolioManager.GenerarFolio("Estimacion")
   ?? Guardar en file system
9. FormPdfPreview.Show()
10. Usuario revisa PDF
11. Click "Guardar en BD"
12. InsertSQL Estimaciones (header)
13. InsertSQL DetallesEstimacion (líneas)
14. FormRepositorioPDFs: PDF disponible para consultar después

BD Modificadas: Estimaciones, DetallesEstimacion, Folios
Archivos Modificados: Estimacion_*.pdf
Permisos Requeridos: Crear Estimación
Errores Posibles: BD sin conexión, sin insumos disponibles
```

### Funcionalidad: Activar Destajo (Con Liberación Automática)

```
Iniciador: Usuario admin/supervisor/usuario normal
Tiempo estimado: 5-10 minutos

Pasos:
1. PanelPrincipal ? Click "Tareas"
2. FormActivarTareasTreeList.Show()
3. Seleccionar Manzana/Lote
4. QuerySQL ? Cargar destajos activos/inactivos
5. TreeListView: mostrar lista
6. Usuario clic en destajo
7. Panel Lateral: muestra guía, pasos, insumos requeridos
8. Usuario: Click "Activar"
9. UpdateSQL: estado Destajo = "Activo"
10. SalidaAlmacenService.LiberarInsumosAlActivarDestajo():
    ?? QuerySQL: SELECT insumos WHERE WBS = @wbs
    ?? FOR EACH insumo:
    ?  ?? InsertSQL SalidasAlmacen (vale)
    ?  ?? UpdateSQL Insumos (cantidad -= liberada)
    ?  ?? GenerarPDF vale
    ?  ?? Guardar PDF en file system
    ?? ReturnSQL lista de vales
11. Panel Lateral: Tab "Almacén" muestra vales creados
12. Usuario: Asignar mano de obra (FormManoObra)
13. Usuario: Registrar evidencias fotográficas
14. Cuando complete: Click "Finalizar"
15. UpdateSQL: estado Destajo = "Finalizado"
16. UpdateSQL InventarioCasas.DestajosTerminadosWBS (add WBS to JSON)

BD Modificadas: Destajos, SalidasAlmacen, Insumos, InventarioCasas, ManoObra, Evidencias
Archivos Modificados: Vale_*.pdf
Permisos Requeridos: Activar Destajo
Errores Posibles: Insumos insuficientes, BD sin conexión
```

### Funcionalidad: Generar Orden de Compra

```
Iniciador: Usuario admin/supervisor/usuario normal
Tiempo estimado: 10-15 minutos

Pasos:
1. PanelPrincipal ? Click "Compra Directa"
2. FormCompraMulti.Show()
3. Selector Manzana/Lote (opcional si compra indirecta)
4. Selector Proveedor
5. Click "Agregar Insumo" ? FormAgregarInsumoCarrito
6. Seleccionar insumo, cantidad, precio
7. Agregar más insumos (repetir 5-6)
8. Mostrar carrito con subtotal
9. Click "Generar Orden"
10. FormGenerarOrden.Show() (preview)
11. Revisar detalles, confirmar
12. Click "Generar PDF"
13. FormCompraMulti_ExtensionPDFRepositorio:
    ?? PDFsharp ? Orden PDF
    ?? FolioManager.GenerarFolio("OrdenCompra")
    ?? InsertSQL OrdenesCompra
    ?? InsertSQL DetallesOrdenCompra (líneas)
    ?? Guardar PDF en file system
14. FormPdfPreview.Show()
15. Usuario: Enviar a proveedor (manual)
16. Esperar entrega
17. FormAlmacen ? Tab "Entradas"
18. Registrar entrada: seleccionar orden, cantidad recibida
19. UpdateSQL OrdenesCompra.estado = "Entregada"
20. UpdateSQL Insumos (cantidad += recibida)

BD Modificadas: OrdenesCompra, DetallesOrdenCompra, Insumos
Archivos Modificados: Orden_*.pdf
Permisos Requeridos: Crear Orden de Compra
Errores Posibles: Proveedor no encontrado, insumos no existen
```

---

## ??? Estructura de Carpetas Esperada

```
C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\ProgramC#\DynamicSepticSystem\
?
??? DynamicSepticSystem\
?   ??? *.cs (formularios, modelos, servicios)
?   ??? Properties\
?   ?   ??? AssemblyInfo.cs
?   ?   ??? Resources.Designer.cs
?   ?   ??? Settings.Designer.cs
?   ??? WPF\
?   ?   ??? PanelPrincipalWPF.xaml.cs
?   ??? obj\
?   ?   ??? (build artifacts)
?   ??? bin\
?   ?   ??? (executables)
?   ??? App.config
?   ??? packages.config
?   ??? DynamicSepticSystem.csproj
?   ?
?   ??? DOCUMENTACION\
?       ??? DOCUMENTACION_ARQUITECTURA_COMPLETA.md ??? ESTE ARCHIVO
?       ??? INDICE_RAPIDO_REFERENCIA.md
?       ??? DIAGRAMA_ARQUITECTURA_VISUAL.md
?       ??? MATRIZ_FUNCIONALIDADES_MODULOS.md
?       ??? (otros .md de documentación existente)
?       ??? README.md (índice general)
?
??? Updater\
?   ??? ProgramFramework.cs
?   ??? Properties\
?   ?   ??? AssemblyInfo.cs
?   ??? obj\
?   ?   ??? (build artifacts)
?   ??? bin\
?   ?   ??? (Updater.exe)
?   ??? Updater.csproj
?
??? .git\
?   ??? (Git repository - feature/api-migration branch)
?
??? README.md (raíz)
```

---

## ?? Matriz de Versiones y Compatibilidad

```
??????????????????????????????????????????????????????????????
? Componente                 ? Versión  ? Compatibilidad   ?
??????????????????????????????????????????????????????????????
? .NET Framework             ? 4.7.2    ? Windows 7+       ?
? C#                         ? 7.3+     ? Framework 4.7.2  ?
? SQL Server                 ? 2016+    ? Production       ?
? Web API (Calandria.Api)    ? -        ? .NET/ASP.NET     ?
? WinForms                   ? Built-in ? Framework 4.7.2  ?
? WPF (experimental)         ? Built-in ? Framework 4.7.2  ?
? MaterialSkin.2             ? 2.3.1    ? .NET 4.7.2       ?
? ObjectListView             ? 2.7.1.5  ? .NET 2.0+        ?
? PDFsharp                   ? 1.50.5147? .NET 2.0+        ?
? iTextSharp                 ? 5.5.13.3 ? .NET 2.0+        ?
? Newtonsoft.Json            ? 13.0.3   ? .NET 4.5+        ?
? ClosedXML                  ? 0.105.0  ? .NET 4.5+        ?
? EPPlus                     ? 8.0.5    ? .NET 4.6.1+      ?
? DlhSoft Gantt (WinForms)   ? 2.0.0.7  ? .NET 2.0+        ?
? DlhSoft Gantt (WPF)        ? 4.3.49   ? .NET 3.5+        ?
?                                                          ?
? GitHub Releases API        ? v3       ? Public/Private   ?
? Tailscale Network          ? -        ? VPN (IPs 100.x)  ?
?                                                          ?
? Versión Aplicación         ? 1.3.7.0  ? Production       ?
? Versión API                ? ?        ? In dev           ?
? Rama Git Activa            ? feature/ ? migration        ?
?                            ? api-migr ? in progress      ?
??????????????????????????????????????????????????????????????
```

---

## ?? Mapa de Decisiones Arquitectónicas

### Por qué Híbrida (SQL + API)?
- **Legacy**: Datos históricos en SQL Server
- **Futuro**: Migración gradual a Web API
- **Flexibilidad**: Módulos pueden usar cualquier fuente
- **No interrumpe**: Usuarios continúan trabajando durante migración

### Por qué WinForms?
- **Existente**: Código base WinForms desde inicio
- **Performance**: Renderizado rápido local
- **No requiere browser**: Independiente de web
- **Datos sensibles**: Mejor control local vs cloud

### Por qué MaterialSkin?
- **Moderno**: Material Design en WinForms
- **Corporate**: Colores personalizables (café corporativo)
- **Accesible**: UI moderna sin reescribir WinForms

### Por qué PDFsharp + iTextSharp?
- **Independencia**: Sin depender de Adobe
- **Generación en tiempo real**: PDF dinámico
- **Reportes**: Flexibilidad en formato

### Por qué Actualización desde GitHub?
- **SaaS-ready**: Deployable sin servidor corporativo
- **Transparencia**: Releases públicos (o privados con token)
- **Control**: Versioning simple
- **Seguridad**: Verificación opcional SHA256 + Authenticode

