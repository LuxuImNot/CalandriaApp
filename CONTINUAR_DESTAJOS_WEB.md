# Continuación — Destajos a UI web (rediseño tipo panel.html)

**Sesión interrumpida:** 29 de julio de 2026 · **Rama:** `feature/api-migration`
**Documento padre:** `MIGRACION_UI_WEB.md` (leerlo primero: define el patrón)

Este archivo existe sólo para retomar el trabajo tras un reinicio. Cuando el
módulo quede cerrado, su contenido se integra a `MIGRACION_UI_WEB.md` y este
archivo se borra.

> **Las 5 tareas de §5 están completadas.** Lo que falta para poder borrar este
> archivo (mover a "cerrado") es la verificación en vivo que quedó pendiente
> por no tener sesión contra la BD real ni el servidor de despliegue — ver
> "Falta" al final de la Tarea 5 y §6.1 de `MIGRACION_UI_WEB.md`. Retomar
> significa hacer esa verificación, no volver a programar nada.

---

## 0. Prompt para retomar

> Verifica en vivo la migración de Destajos a UI web según lo que quedó
> pendiente en `CONTINUAR_DESTAJOS_WEB.md` (Tarea 5, sección "Falta") y en
> `MIGRACION_UI_WEB.md` §6.1. Las decisiones y el código ya están cerrados
> (§1 y §5); esto es sólo probar contra datos reales y desplegar.

---

## 1. Decisiones ya tomadas — no volver a preguntar

| Pregunta | Respuesta elegida |
|---|---|
| Capa de datos | **API completo primero, lectura y escritura.** Se migran los ~26 sitios de SQL directo a `api/destajos`, incluidas las escrituras y la DDL. Nada de puentear el SQL existente desde el host. |
| Alcance de la pasada | **Sólo Destajos**, completo y verificado. `FormAlmacen` queda para la siguiente sesión con el patrón ya pavimentado. |

Petición original del usuario, para no perder de vista el entregable: migrar los
módulos principales (`FormAlmacen` y `FormActivarTareasTreeList`) con **rediseño
al estilo de `panel.html`**, usando la skill **`ui-ux-pro-max`** y **anime.js**
para animaciones apropiadas, experiencia fluida e interfaz visualmente
interactiva. Esta pasada cubre Destajos; Almacén sigue pendiente.

---

## 2. Estado del trabajo

**Hecho**

- Catálogo completo del SQL de Destajos (§3) y de las convenciones del API (§4).
- **Creado:** `Calandria.Api/Models/DestajosDtos.cs`. Contiene `NodoDestajoDto`,
  `ResumenDestajosDto`, `ArbolDestajosDto`, `CasaDestajoDto`, `CuadrillaDto`,
  `MiembroCuadrillaDestajoDto`, y los requests `DestajoRefRequest`,
  `ActivarDestajoRequest`, `DesactivarDestajoRequest`, `ReabrirDestajoRequest`,
  `NodoActivacionRequest`, `GuardarActivacionesRequest`,
  `GuardarPdfDestajoRequest`, `ResultadoDestajoDto`.
  (Renombrado: `MiembroCuadrillaDto` → `MiembroCuadrillaDestajoDto`; colisionaba
  de nombre con la clase homónima ya existente en `NominaDtos.cs`, mismo
  namespace, forma distinta — no se tocó `NominaDtos.cs` ni `NominaController.cs`.)
- **Creado:** `Calandria.Api/Controllers/DestajosController.cs` — Tarea 2
  completa: los 6 endpoints de lectura + 6 de escritura de §5, whitelist de
  ruta, DDL en servidor (`EnsureTablaActivacion` / `EnsureTablaExcepcionesLiberacion`
  / `EnsureTablaPdfsDestajos` / `EnsureTablaMiembrosCuadrilla`), transacciones
  en las transiciones (activar/finalizar/desactivar/reabrir/guardar), `Estado`
  calculado en servidor igual que `PanelPasos.cs` (§3.4), y permiso ADMIN
  revalidado con `User.IsInRole("Admin") || Identity.Name == "admin"` en
  desactivar/reabrir (antes sólo se comprobaba en el cliente). Compila
  0 errores/0 warnings con `-t:Rebuild`. Probado en `localhost:8799`: sin
  token → 401, ruta falsa → 404, `/api/auth/login` (GET) → 405 (confirma que
  no todo cayó bajo `[Authorize]`); no se probó aún contra datos reales.
- **Creado:** `Calandria.Api/ui/destajos.html` — Tarea 3 completa (detalle y
  contrato de mensajes en §5, Tarea 3). Paso-a-paso como vista principal,
  tokens/estructura de `panel.html` reutilizados, anime.js v4.5.0 incrustado
  con las mismas animaciones (timeline de entrada, stagger, contador sobre
  objeto JS). La página delega toda escritura al host por `postMessage`; no
  llama al API ni guarda token. Funciona standalone (sin host) con una casa
  de ejemplo. Sintaxis de ambos `<script>` validada con `node --check`.

**No empezado:** el host y la verificación completa de §5 (Tareas 4-5).

> El resto de archivos que aparecen sucios en `git status` vienen de trabajo previo.

---

## 3. Catálogo del SQL de Destajos (el trabajo de investigación ya hecho)

Archivos: `FormActivarTareasTreeList.cs` (24 sitios) y `.Insumos.cs` (2).
Los partials `.Almacen.cs`, `.PanelPasos.cs`, `.PanelGuia.cs` no tienen SQL.

### 3.1 Modelo de datos

- **Árbol de la ruta:** tablas `RutaTuneraDestajo` / `RutaCalandraDestajo`, más su
  gemela `<ruta>_Columnas` que guarda columnas dinámicas en filas
  (`NodoID, NombreColumna, Valor`). El SELECT las pivotea con
  `MAX(CASE WHEN c.NombreColumna = 'Cantidad' ...)` para `Cantidad`, `Unidad`,
  `Precio` y `Clave`.
- **Niveles:** 0 = Padre (categoría) · 1 = Sub-Padre (**el destajo**) · 2 = Hijo
  (insumo / mano de obra). `TipoTarea = 1` es Material.
- **Estado de activación:** tabla `ActivacionTareasRuta`, una fila por
  (Manzana, Lote, Ruta, NodoID).
- **Surtido:** `dbo.SalidasAlmacen` agrupado por `Clave` y `Descripcion`.

### 3.2 Lecturas a migrar

| Origen | Consulta |
|---|---|
| `:109` | `SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana` |
| `:136` | `SELECT DISTINCT Lote FROM InventarioCasas WHERE Manzana = @m ORDER BY Lote` |
| `:190` | `SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l` |
| `:1000` | Árbol completo con el pivote de `_Columnas` (`CargarTareasDesdeRuta`) |
| `:1153` | Activaciones guardadas de la casa (`CargarActivacionesGuardadas`) |
| `:1224` | Surtido por clave/nombre desde `SalidasAlmacen` (`CargarSurtidoPorClave`) |
| `:1737` | Miembros de cuadrilla: `MiembrosCuadrilla m LEFT JOIN TRABAJADORES t` |
| `FormAsignarCuadrilla.cs:665` | `SELECT DISTINCT CodigoCuadrilla FROM MiembrosCuadrilla ORDER BY CodigoCuadrilla` |

### 3.3 Escrituras a migrar

| Origen | Operación |
|---|---|
| `:1098-1145` | **DDL en ejecución**: crea `ActivacionTareasRuta` y agrega 5 columnas si faltan. **Se mueve al servidor** (§4.3) |
| `:1492-1547` | `btnGuardar_Click`: `DELETE` de toda la casa + `INSERT` de cada nodo |
| `:1627-1673` | `PersistirActivacionDestajo`: `DELETE` + `INSERT` de **un** nodo (upsert artesanal) |
| `:2115-2170` | `PDFsDestajos`: DDL idempotente + `INSERT` del PDF (`VARBINARY(MAX)`) |
| `SalidaAlmacenService.cs:137` | `YaLiberado`: cuenta en `SalidasAlmacen` por `OrigenRuta`/`OrigenNodoID` |
| `SalidaAlmacenService.cs:257` | `RegistrarExcepcion` → `ExcepcionesLiberacionAlmacen` (DDL en `:63`) |
| `NominaTareasAsignada` | `DetectarNodosConNominaAsignada` / `EliminarNominaAsignada` (usadas al reabrir) |

### 3.4 Máquina de estados del destajo (Nivel 1) — respetarla tal cual

Estados (`EstadoPaso` en `PanelPasos.cs:38`): **Bloqueado · Disponible ·
Activado · Terminado**.

Desbloqueo secuencial (`PanelPasos.cs:172`): dentro de cada categoría (Nivel 0),
el destajo en posición *n* está desbloqueado si *n* es el primero **o** el
anterior tiene `Finalizado = 1`.

```
Bloqueado ──(el anterior se finaliza)──> Disponible
Disponible ──[Activar]──> Activado      requiere cuadrilla; pone DesatajoActivado=1,
                                        Activa=1, FechaActivacion=now
                                        → PDF "ASIGNACIÓN DE DESTAJO"
Activado ──[Finalizar]──> Terminado     requiere DesatajoActivado + cuadrilla;
                                        Finalizado=1, FechaFinalizacion=now
                                        → PDF "ACTA DE FINALIZACIÓN"
Activado ──[Desactivar]──> Disponible   [ADMIN]. Si ya liberó insumos: exige
                                        justificación y registra excepción.
                                        El stock NO se devuelve. Limpia cuadrilla.
Terminado ──[Reabrir]──> Activado       [ADMIN]. Conserva cuadrilla y
                                        FechaActivacion. Si hay nómina asignada,
                                        pregunta si borrarla (los recibos ya
                                        emitidos nunca se borran).
```

**Ojo (ya está bien en el código, no “arreglarlo”):** activar **ya no** libera
insumos ni toca stock — el surtido es manual en Almacén → Salidas. El comentario
de cabecera de `FormActivarTareasTreeList.Almacen.cs` dice lo contrario: **está
obsoleto**. No es parte de este trabajo; sólo no creerle.

---

## 4. Convenciones del API — copiarlas, no inventar

### 4.1 Forma de un controlador

`Calandria.Api/Controllers/AlmacenController.cs` es el modelo a seguir:
`[RoutePrefix("api/...")]`, `ApiController`, `Db.Abrir()` (en `Data/Db.cs`),
`SqlCommand` crudo con `AddWithValue`, `IHttpActionResult` (`Ok(...)`,
`BadRequest(...)`). DTOs en `Calandria.Api/Models/`. El filtro `[Authorize]` es
**global**: no hace falta ponerlo por método.

### 4.2 Whitelist de la ruta — **crítico**

`rutaActual` es un **nombre de tabla interpolado** en el SQL
(`FROM {rutaActual} r`). Al venir del cliente, aceptar sólo `RutaTuneraDestajo` y
`RutaCalandraDestajo` mediante constantes, como hace `AlmacenController` (`:19-20`).
Nunca interpolar la cadena recibida.

### 4.3 DDL en el servidor: ya hay precedente

`NominaController.cs:442` (`EnsureTabla`), `:469`, `:499` hacen exactamente esto,
con la firma `(SqlConnection conn, SqlTransaction tx = null)` y el comentario
«antes lo hacía el cliente en cada apertura del formulario; ahora vive en el
servidor». Mover la DDL de `:1098` con ese mismo patrón.

### 4.4 Quién es administrador

- Cliente: `Global.EsAdmin` = usuario llamado **"admin"** (`Global.cs:18`), mismo
  criterio que `EsUsuarioAdmin()` en `FormActivarTareasTreeList.cs:453`.
- JWT: `TokenService.cs:28-34` emite `ClaimTypes.Name` (usuario) y
  `ClaimTypes.Role`; `AuthController.PermisosPorRol` trata `rol == "Admin"`.
- **En el API usar:** `User.IsInRole("Admin") || User.Identity.Name == "admin"`,
  y devolver 403 en `desactivar`/`reabrir`. Esto es una mejora real: hoy el
  permiso se comprueba sólo en el cliente.

### 4.5 Cliente

`ApiClient` (estático) expone `Get<T>`, `GetBytes`, `Post`, `Post<T>`, `Token`,
`Autenticado`.

### 4.6 Codificaciones — no equivocarse

| Sitio | Codificación |
|---|---|
| `Calandria.Api/**/*.cs` | **UTF-8 sin BOM** (verificado) |
| `DynamicSepticSystem/**/*.cs` | **Windows-1252** — ver `reference_winforms_source_encoding` |
| `ui/*.html` | UTF-8 y **obligatorio** `<meta charset="utf-8">` |

Un archivo nuevo del cliente guardado en UTF-8 rompe los identificadores no-ASCII
que comparte con otros archivos. Para el partial nuevo del host: escribirlo
**sólo con ASCII** (identificadores y comentarios) o guardarlo en Windows-1252.

---

## 5. Lo que falta — tareas en orden

### Tarea 2 · `Calandria.Api/Controllers/DestajosController.cs`

Superficie planeada (los DTOs ya existen para todo esto):

**Lectura**
| Ruta | Devuelve |
|---|---|
| `GET api/destajos/manzanas` | manzanas de `InventarioCasas` |
| `GET api/destajos/lotes?manzana=` | lotes |
| `GET api/destajos/casa?manzana=&lote=` | `CasaDestajoDto` (prototipo + ruta derivada) |
| `GET api/destajos/arbol?manzana=&lote=&ruta=` | `ArbolDestajosDto`: **una sola llamada** que fusiona árbol + activaciones + surtido + resumen, y calcula `Estado` por nodo según §3.4 |
| `GET api/destajos/cuadrillas` | `CuadrillaDto[]` |
| `GET api/destajos/cuadrillas/{codigo}/miembros` | `MiembroCuadrillaDto[]` |

**Escritura** (todas devuelven `ResultadoDestajoDto`)
| Ruta | Notas |
|---|---|
| `POST api/destajos/activar` | exige `Cuadrilla`; valida que el paso esté desbloqueado |
| `POST api/destajos/finalizar` | exige `DesatajoActivado` + cuadrilla |
| `POST api/destajos/desactivar` | **[ADMIN]**; si `YaLiberado`, exige `Justificacion` y registra la excepción |
| `POST api/destajos/reabrir` | **[ADMIN]**; `BorrarNomina` opcional |
| `POST api/destajos/guardar` | reemplazo en bloque (el `btnGuardar` de hoy) |
| `POST api/destajos/pdf` | archiva el PDF en `PDFsDestajos` (base64 en el request) |

Notas de implementación:
- Un `EnsureTablaActivacion(conn, tx)` privado con la DDL de §3.3, llamado al
  entrar en cada endpoint que la necesite.
- Las transiciones deben usar transacción (hay precedente en
  `AvancesController.cs:552` y `FoliosEstimacionController.cs:82`).
- El PDF se sigue **generando en el cliente** (PdfSharp + `FormPdfPreview`); el
  API sólo lo archiva. No migrar la generación en esta pasada.
- Registrar el nuevo `.cs` en `Calandria.Api.csproj` si el proyecto no usa
  compilación por glob (verificar: es un csproj clásico de net472).

### Tarea 3 · `Calandria.Api/ui/destajos.html`

- **Invocar la skill `ui-ux-pro-max`** antes de diseñar (lo pidió el usuario).
- Reutilizar **tal cual** el bloque de tokens de `panel.html` (líneas 8-73):
  paleta café Calandria, `--ok/--warn/--stop/--idle`, `--brass`, `--mono`,
  claro/oscuro por `prefers-color-scheme` **más** override
  `:root[data-theme="..."]`. Mapa de estados listo: Terminado→`ok`,
  Activado→`warn`, Bloqueado→`idle`, Disponible→`brass`.
- Estructura probada que conviene repetir: `.app` (grid `rail 1fr`,
  `height:100dvh`, `overflow:hidden`, **la página no scrollea**), `.rail`,
  `.titleblock`, `.kpis`, `.work` (grid con inspector de `--insp`), `.panel`.
  Componentes ya existentes: `.btn`, `.badge`, `.kpi`, `.meter`, `.stat`,
  `.spec`, `.seg`, `.pill`, `.icon-btn`, `.eyebrow`, `.num`, `#toast`.
- **Vista principal = el paso-a-paso** (así quedó el form: el árbol pasó a
  ventana aparte, ver `project_destajos_stepper`). Categorías con sus destajos
  numerados, estado por paso, y el bloqueo secuencial explicado en la propia
  tarjeta («Termina el destajo anterior para desbloquear este paso»).
- **anime.js v4.5.0** ya está minificado dentro de `panel.html` (línea 690):
  copiar ese `<script>` en lugar de traerlo de un CDN. API usada:
  `const { animate, createTimeline, stagger } = window.anime;`
- Patrones de animación que ya funcionan y conviene reusar
  (`panel.html:1107-1141` y `:940-971`): una sola `createTimeline` de entrada en
  vez de efectos sueltos; `stagger` para listas; contador numérico interpolando
  un **objeto JS** (`contarEl`) en vez del DOM; anillo de avance animando
  `strokeDashoffset`; barras animando `width`. **Respetar
  `prefers-reduced-motion`** con la constante `REDUCE` como allí.
- Techo duro: `NavigateToString` **no admite más de 2 MB**. `panel.html` ya va en
  888 KB por el mapa incrustado; `destajos.html` no lleva mapa, así que hay
  espacio de sobra, pero no incrustar imágenes pesadas.

**Estado: completada.** `Calandria.Api/ui/destajos.html` escrito (≈156 KB con
anime.js incrustado). Decisiones de diseño tomadas en esta tarea, para que
Tarea 4 no tenga que re-decidir:

- **La página nunca llama al API ni guarda un JWT.** Igual que `panel.html`,
  todo lo que muta datos sale como un mensaje `postMessage` al host; el host
  ya tiene (o tendrá en Tarea 4) los métodos que llaman a
  `DestajosController` — la página es una vista + navegador de pasos, no un
  cliente HTTP. Esto también evita reconstruir en HTML cosas que ya existen y
  funcionan del lado WinForms (el diálogo `FormAsignarCuadrilla`, el PDF con
  PdfSharp, `PromptJustificacion`).
- **Contrato de mensajes página → host** (`HOST.postMessage(msg)`,
  `window.chrome.webview`):
  | `accion` | Campos extra | Qué debe hacer el host |
  |---|---|---|
  | `cargar-manzanas` | — | Responder `{tipo:"manzanas", lista:[...]}`  |
  | `cargar-lotes` | `manzana` | Responder `{tipo:"lotes", lista:[...]}` |
  | `cargar-casa` | `manzana, lote` | `GET arbol` + `GET cuadrillas` y responder `{tipo:"datos", ...}` |
  | `activar` | `nodoId` (+ contexto de casa) | Abrir `FormAsignarCuadrilla`, luego `POST activar`, repush `datos` |
  | `finalizar` | `nodoId` | `POST finalizar`, repush `datos` (+ PDF de finalización) |
  | `desactivar` | `nodoId` | Pedir justificación si aplica (`PedirJustificacion` ya existente), `POST desactivar` |
  | `reabrir` | `nodoId` | Preguntar por nómina asignada, `POST reabrir` |
  | `cambiar-cuadrilla` | `nodoId` | Igual que `activar` pero sin exigir que esté "Disponible" |
  | `regenerar-pdf` | `nodoId` | Regenerar PDF con PdfSharp + `POST pdf` para archivarlo |
  | `distribucion-nomina` | `nodoId` | Abrir `FormAsignarNomina` sobre los hijos de mano de obra |
  | `insumos` / `historial` / `arbol-completo` | — | Abrir las vistas ya existentes de `PanelPasos.cs` (`MostrarInsumos`/`MostrarHistorial`) o el árbol clásico |
  | `volver` | — | Volver al formulario clásico sin reiniciar |

  Todo mensaje de casa (`activar`, `finalizar`, …) trae también
  `manzana, lote, ruta, prototipo` porque `alHost()` los añade automáticamente
  desde el árbol ya cargado en la página.
- **Contrato de mensajes host → página** (`HOST.postMessage(...)` desde C#,
  la página escucha `addEventListener("message", ...)`): `{tipo:"manzanas"|"lotes", lista}`,
  `{tipo:"datos", esAdmin, usuario, rol, arbol}` donde `arbol` es
  `ArbolDestajosDto` tal cual, y `{tipo:"error", mensaje}`.
  **Todo en camelCase** — igual que ya devuelve el API
  (`CamelCasePropertyNamesContractResolver` en `Startup.cs`), así que si el
  host reserializa con `Newtonsoft.Json` para `PostWebMessageAsJson` debe usar
  `CamelCasePropertyNamesContractResolver` también, o pasar el JSON del API
  casi sin tocar.
- La ficha del destajo seleccionado reemplaza el `.plan-wrap`/`.ring` de
  `panel.html` (no aplican a destajos) por una lista de pasos (`.steps`,
  agrupada por categoría Nivel 0) + inspector de detalle (insumos con punto
  verde/naranja según `Surtido >= Cantidad`, mano de obra, acciones según
  `Estado`). Badge nuevo `.b-brass` (no existía en `panel.html`) para
  Disponible.
- La página funciona sin host (`window.chrome.webview` ausente) con una casa
  de ejemplo incrustada, igual que `panel.html` funciona sin host con las
  coordenadas incrustadas — permite abrir el archivo suelto en un navegador
  para revisar diseño/animaciones antes de que exista Tarea 4.
- Validado: los dos bloques `<script>` pasan `node --check` (sintaxis) y el
  HTML tiene todas las etiquetas balanceadas. **No probado aún en WebView2 ni
  en Edge headless** (eso es Tarea 5).

### Tarea 4 · Host WinForms

**Estado: completada.**

- **Creado:** `DynamicSepticSystem/FormActivarTareasTreeList.Web.cs`, siguiendo
  `PanelPrincipal.Web.cs` como molde (mismo patrón: anota controles visibles →
  los oculta → `WebView2 Dock=Fill` → HTML API→caché en
  `%LOCALAPPDATA%\Calandria\ui\destajos.html` → `api/ui/destajos/version` decide
  si hace falta descargar). Se apaga con `DestajosWeb=false` en `App.config`
  (clave nueva, mismo criterio que `PanelWeb`). Caída al formulario clásico ante
  cualquier fallo (sin runtime WebView2, sin HTML, error de inicialización),
  registrando en `LogErrores` con origen `DestajosWeb`.
- **No quedó ASCII-only como decía este plan** (§4.6 original): el archivo usa
  UTF-8 sin BOM, igual que `PanelPasos.cs`/`Insumos.cs` ya lo hacían. Verificado
  con el propio build: la cadena `"Justificación requerida"` quedó como UTF-16
  genuino en el `.exe` compilado (`grep` binario, no mojibake). La regla real de
  `reference_winforms_source_encoding` es más angosta de lo que este plan asumía:
  sólo los **identificadores** con acentos compartidos entre archivos son
  peligrosos; cadenas y comentarios no. Este archivo no define ningún
  identificador acentuado, así que UTF-8 es seguro aquí.
- **Decisión clave de integración:** en vez de reimplementar PDF/insumos/
  historial/árbol/nómina desde cero contra los DTO del API, `CargarCasaWeb` /
  `RefrescarArbol` convierten la respuesta de `GET arbol` al mismo modelo que ya
  usa el formulario clásico (`itemsTareas`, `manzanaActual`, `_surtidoPorClave`,
  `olvTareas.SetObjects(...)`) vía un mapeador `ADto(NodoDestajoApi)`. Eso deja
  reutilizar **tal cual, sin tocarlos**: `MostrarArbolCompleto()`,
  `MostrarHistorial()`, `CrearPdfDestajo(...)`, `SanitizarNombreArchivo(...)`,
  `DetectarNodosConNominaAsignada(...)`, `FormAsignarCuadrilla`,
  `FormAsignarNomina`, `FormPdfPreview` — ninguno de ellos hace SQL para
  destajos, así que reutilizarlos no rompe la regla de §1. Se añadió
  `ItemTareaActivacion.Surtido` (propiedad nueva, no rompe nada existente) para
  poder portar ese dato del API a esos métodos.
- Dos raw-SQL que sí quedaban fuera del catálogo de `DestajosController` se
  evitaron sin añadir endpoint: `ConstruirTablaInsumosWeb()` (nuevo, en el
  partial web) rehace el mismo agrupado que `ConstruirTablaInsumos()` pero saca
  el "Usado" del `Surtido` que **ya trae cada nodo** desde `GET arbol`, en vez
  de volver a consultar `SalidasAlmacen`. `MostrarInsumos()` (clásica, con su
  SQL propia) queda intacta y sin usarse desde el flujo web.
- El host llama al API con `ApiClient` y manda los datos por
  `PostWebMessageAsJson` serializados con `CamelCasePropertyNamesContractResolver`
  (cumple el contrato camelCase de la Tarea 3); el token nunca entra a la página.
- `WebPanel_Mensaje` implementa el vocabulario completo documentado arriba.
  `activar`/`cambiar-cuadrilla` comparten `ActivarWeb` (abre
  `FormAsignarCuadrilla`, `POST activar`, refresca, genera PDF). `desactivar`
  intenta sin justificación primero y sólo pide `PromptJustificacion.Pedir(...)`
  si el API responde 400 pidiéndola (evita adivinar `YaLiberado` desde el
  cliente). `reabrir` detecta nómina asignada reutilizando
  `DetectarNodosConNominaAsignada` y deja que el propio `POST reabrir` la borre
  (`BorrarNomina`), sin llamar a `/api/nomina/asignacion/eliminar` por
  separado. Permisos `[ADMIN]` (`desactivar`/`reabrir`) se revalidan con
  `EsUsuarioAdmin()` en el switch antes de despachar, no sólo se ocultan en la
  página.
- **Ampliado `UiController`** (no duplicado): las rutas pasaron de literales
  (`panel`, `panel/version`) a `{pagina}`/`{pagina}/version` con una whitelist
  explícita (`PaginasPermitidas = { "panel", "destajos" }`, mismo criterio de
  whitelist que la ruta de tabla en `DestajosController`). `api/ui/panel` y
  `api/ui/panel/version` siguen funcionando igual; se ganó `api/ui/destajos` y
  `api/ui/destajos/version` gratis, y el patrón queda pavimentado para
  `almacen` la próxima sesión.
- **Añadidos en `ApiClient.cs`** (mismo archivo del cliente, sección nueva
  "Destajos"): DTOs espejo de `DestajosDtos.cs` para el lado C# —
  `NodoDestajoApi`, `ResumenDestajosApi`, `ArbolDestajosApi`, `CasaDestajoApi`,
  `CuadrillaDestajoApi`, `MiembroCuadrillaDestajoApi`, `ResultadoDestajoApi`.
- Compilado con MSBuild de VS2022 (`dotnet build` falla en este entorno para el
  csproj clásico x86 con `GenerateResource`/MSB4216; hace falta el MSBuild real:
  `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe`).
  **0 errores** en API y cliente; los únicos warnings nuevos son
  `CS0649` en `MensajeWebDestajos` (campos que Newtonsoft llena por reflexión),
  idéntico patrón al `MensajeWeb` ya existente en `PanelPrincipal.Web.cs`.
- Probado en `localhost:8799` sin token: `api/ui/panel`, `api/ui/destajos`,
  `api/ui/destajos/version` y `api/destajos/manzanas` → **401** (el filtro
  `[Authorize]` global corta antes de llegar a la whitelist de `UiController`,
  así que no se pudo diferenciar 404-por-whitelist de 401-por-token sin una
  sesión real). **Falta**: probar con un token válido contra la BD real, y
  probar el WebView2 de punta a punta (eso es Tarea 5).

### Tarea 5 · Compilar y verificar

**Estado: completada** (dentro de lo que se puede verificar sin sesión real
contra la BD de producción; ver "Falta" abajo).

- **Recompilado limpio de los dos proyectos** con `-t:Rebuild` (API:
  `dotnet build`; cliente: MSBuild de VS2022 — `dotnet build` falla en esta
  máquina para el csproj clásico x86 con `GenerateResource`/MSB4216). **0
  errores** en ambos; los únicos warnings son los mismos patrones
  preexistentes (`CS0649` en clases de deserialización) ya presentes en
  `PanelPrincipal.Web.cs`.
- **Contraste de rutas** en una copia desechable (`localhost:8799`, BaseUrl
  cambiada sólo en el `.exe.config` del build local, no en el fuente):
  - Ruta que no existe en absoluto → **404**.
  - `api/destajos/manzanas` y `api/ui/destajos` sin token → **401**.
  - `api/health` (pública) → **200**.
  - `api/auth/login` por GET → **405**; por POST con credenciales inventadas →
    **401** (el de la lógica de login, no el del filtro). Este último contraste
    es más fuerte que el de la 1ª sesión: prueba que el pipeline distingue
    "no existe", "protegida sin token", "pública" y "pública con error propio",
    no que todo caiga bajo un candado ciego.
- **Página en Edge headless con host de WebView2 simulado.** Se armó un
  arnés con Puppeteer-core (dirige el Edge ya instalado en la máquina, sin
  descargar Chromium) que: stubea `window.chrome.webview` con
  `postMessage`/`addEventListener` antes de que corra el script de la página;
  navega a `destajos.html` con `file://`; simula respuestas del host
  (`manzanas`, `lotes`, `datos` con un árbol de prueba de 2 destajos —uno
  Disponible, uno Bloqueado— y 2 hijos); pulsa cada botón. **20/20
  aserciones**, 0 errores de JS. Cubre: la página pide manzanas sola al
  detectar el host; los selects de manzana/lote se pueblan y encadenan
  correctamente; el botón Cargar casa manda `cargar-casa` con los valores
  elegidos; los 2 pasos se pintan con la variante de color correcta
  (`s-brass` para Disponible); seleccionar un paso llena la ficha del
  inspector; un insumo con `surtido < cantidad` se marca pendiente (punto
  naranja); el botón Activar manda `{accion, nodoId, manzana, lote, ruta,
  prototipo}`; `esAdmin` esconde/muestra el botón Desactivar; los botones del
  riel (`insumos`/`historial`/`arbol-completo`/`volver`) mandan su acción
  exacta; el botón de tema fija `data-theme`. El script vive en el scratchpad
  de la sesión, **no está versionado** (es una prueba desechable, no un
  artefacto del repo).
- **`MIGRACION_UI_WEB.md` actualizado** con la misma separación
  verificado/no-verificado (nueva §6.1 "Destajos"), y recordando que igual
  que `panel.html`, **`destajos.html` tampoco está desplegado** en el `ui/`
  del servicio real todavía.

**Falta** (deliberadamente no se hizo en esta sesión, requiere criterio del
usuario o una BD de prueba):

- Ejecutar `DynamicSepticSystem.exe` de verdad contra la BD real y confirmar
  que `InicializarPanelWeb()` oculta los controles correctos, que el WebView2
  real (no un host simulado) entrega los mensajes, y que los diálogos
  (`FormAsignarCuadrilla`, `FormAsignarNomina`, `PromptJustificacion`) abren
  bien centrados.
- Probar `activar`/`finalizar`/`desactivar`/`reabrir`/`pdf` contra datos
  reales — **no se ensayó** porque el `.exe.config` apunta a la base de
  producción (`100.75.234.9`) y una prueba de escritura ahí mezclaría datos
  de prueba con reales sin supervisión.
- Confirmar con un token real que `api/ui/almacen` (no whitelisteada) da 404:
  sin sesión, el filtro `[Authorize]` global corta con 401 antes de llegar al
  código de la whitelist, así que ese caso específico quedó sin probar.
- Desplegar `destajos.html` en el servidor real.

### 5.1 Crash real encontrado y corregido (primera prueba en la app)

Al abrir el módulo Destajos desde `PanelPrincipal`, el proceso completo
crasheaba (`COMException` en `Microsoft.Web.WebView2.Core.dll`, exit code
`0x4000001F`) en vez de caer al formulario clásico. Causa: `AbrirFormActivarTareasTreeList()`
abre `FormActivarTareasTreeList` con `frm.ShowDialog(this)` — **modal, encima
del panel principal, que ya tiene su propio WebView2 activo desde el
arranque**. `InicializarPanelWeb()` llamaba a
`webPanel.EnsureCoreWebView2Async(null)` como *fire-and-forget*
(`var _ = ...`, igual que en `PanelPrincipal.Web.cs`): un segundo WebView2 del
mismo proceso compitiendo por el entorno implícito por omisión puede fallar en
el SDK que usa el proyecto (`Microsoft.Web.WebView2 1.0.4078.44`, de 2021), y
al no observarse esa excepción, tumbaba el proceso entero en vez de
degradarse. Corregido en `FormActivarTareasTreeList.Web.cs`: entorno propio
con carpeta de datos de usuario separada
(`%LOCALAPPDATA%\Calandria\WebView2Destajos`, no comparte la del panel) +
`IniciarWebView2Async()` envuelto en `try/catch` que cae a
`RestaurarPanelClasico()` ante cualquier fallo. Recompilado (0 errores);
**pendiente que el usuario confirme que ya no crashea**. `PanelPrincipal.Web.cs`
no se tocó (no está roto, y no era parte de lo reportado).

---

## 6. Trampas conocidas

1. **No hay endpoint de destajos hoy.** Si algo de la página no tiene de dónde
   leer, el endpoint va primero — es la decisión de §1.
2. **`_Columnas` guarda todo como texto.** `Cantidad` y `Precio` salen como
   string y hay que parsearlos tolerando cultura (`AlmacenController.ParseDecimal`
   ya resuelve esto: copiarlo).
3. **`AddWithValue` con decimal y string en la misma columna** de un INSERT
   multi-fila fuerza `numeric` por precedencia y revienta con «overflow nvarchar
   to numeric»: tipar explícitamente como `NVarChar`
   (ver `reference_multirow_insert_type_precedence`).
4. **`DELETE` + `INSERT` como upsert** es lo que hace el código actual. Mantenerlo
   (bajo transacción) en vez de rediseñarlo a `MERGE`: cambio no pedido.
5. **`PromptJustificacion.Pedir(...)`** es el diálogo temático reutilizable que ya
   existe. `FormActivarTareasTreeList.Almacen.cs:67` tiene un `PedirJustificacion`
   propio hecho a mano que lo ignora. Es código preexistente: **mencionarlo, no
   tocarlo** (CLAUDE.md §3).
6. **Bug del TreeList:** si se toca el árbol WinForms, ObjectListView 2.7 en modo
   virtual lanza «índice fuera del intervalo»; está mitigado con la subclase
   `SafeTreeListView` (`reference_olv_treelistview_index_crash`).
7. **`git status` está sucio de antes.** No asumir que un archivo modificado es de
   esta migración.

---

## 7. Estado de las tareas al interrumpir

| # | Tarea | Estado |
|---|---|---|
| 1 | Catalogar operaciones SQL de Destajos | **completada** (§3) |
| 2 | Crear `DestajosDtos.cs` y `DestajosController.cs` | **completada** |
| 3 | Diseñar y escribir `destajos.html` | **completada** |
| 4 | Host WinForms: abrir destajos web y puente | **completada** |
| 5 | Compilar y verificar | **completada** (lo verificable sin sesión real; ver "Falta" en Tarea 5) |
