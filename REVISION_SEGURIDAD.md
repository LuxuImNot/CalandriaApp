# Revisión de seguridad — CalandriaApp / DynamicSepticSystem

> Documento de continuidad para retomar el trabajo de seguridad.
> Fecha del análisis: 2026-06-09 · App: WinForms (.NET 4.7.2) + SQL Server.

## Contexto / decisiones ya tomadas

- **El servidor SQL (`100.75.234.9`) solo es accesible dentro del Tailnet (Tailscale/VPN privada).**
  Esto baja la severidad de varios hallazgos: un atacante necesita estar dentro de la red privada
  para alcanzar el SQL. Tailscale ya cifra el tráfico (WireGuard).
- **No se usará una API** por ahora. Para una app interna sobre Tailscale es sobreingeniería.
  Camino elegido: **Opción B = usuario SQL de bajo privilegio + DPAPI** (Windows Auth/Opción A
  no aplica porque no hay Active Directory).
- Reconsiderar una API solo si la app sale de la red privada, o si se agrega versión web/móvil.

## Estado de remediación

| # | Hallazgo | Severidad (con Tailscale) | Estado |
|---|----------|---------------------------|--------|
| 1 | Credenciales `sa` embebidas en `App.config`, distribuidas en cada ZIP de release | Alto (contenido por Tailnet) | ⬜ Pendiente |
| 2 | Token de GitHub (PAT) embebido en `App.config` | **Crítico** (funciona desde cualquier internet) | ⬜ Pendiente |
| 3 | Actualizador sin verificación de firma/hash + Zip Slip (path traversal) | Alto | ⬜ Pendiente |
| 4 | Conexión `Encrypt=false` / `TrustServerCertificate=true` | Bajo (Tailscale ya cifra) | ⬜ Pendiente |
| 5 | Hash de contraseñas SHA-256 sin sal | Alto | ⬜ Pendiente |
| 6 | Autorización del lado del cliente (todos comparten conexión `sa`) | Medio (se resuelve con #1) | ⬜ Pendiente |
| 7 | Fuga de info en mensajes de error (`ex.Message` al usuario) | Bajo | ⬜ Pendiente |
| 8 | Login sin límite de intentos (fuerza bruta) | Bajo | ⬜ Pendiente |

## Orden de urgencia acordado

1. **Revocar el PAT de GitHub** (ya — es lo único expuesto a todo internet).
2. **Reemplazar `sa`** por usuario SQL de bajo privilegio.
3. **Cifrar la cadena de conexión** con DPAPI.
4. **Actualizador**: guard anti–Zip Slip + verificación de hash.
5. **Migrar hashing** de contraseñas a PBKDF2.

---

## Detalle técnico por hallazgo

### 1. Credenciales `sa` embebidas
`DynamicSepticSystem/App.config:5`
```
Server=100.75.234.9;Database=CALANDRIA;User Id=sa;Password=<CONTRASENA-PURGADA>;TrustServerCertificate=true;Encrypt=false;Connect Timeout=30;
```
- Es la cuenta `sa` (admin total del servidor). Se compila a `DynamicSepticSystem.exe.config`
  y viaja dentro de cada `CalandriaApp-v1.x.zip`.
- **Acción:** crear usuario `calandria_app` con permisos mínimos (solo CRUD sobre las tablas
  que usa la app; nunca `sysadmin`/`xp_cmdshell`). Rotar la contraseña de `sa`.

```sql
-- En el servidor SQL:
CREATE LOGIN calandria_app WITH PASSWORD = 'una-contraseña-fuerte-nueva';
USE CALANDRIA;
CREATE USER calandria_app FOR LOGIN calandria_app;
-- Dar solo lo necesario (ajustar según tablas reales):
ALTER ROLE db_datareader ADD MEMBER calandria_app;
ALTER ROLE db_datawriter ADD MEMBER calandria_app;
-- Si se usan procedimientos almacenados, conceder EXECUTE puntual en lugar de datawriter amplio.
-- Rotar sa:
ALTER LOGIN sa WITH PASSWORD = 'otra-contraseña-fuerte';
```

### 2. Token de GitHub (PAT) embebido — CRÍTICO
`DynamicSepticSystem/App.config:12`
```
GitHubToken = <TOKEN-REVOCADO-PURGADO>
```
- Funciona desde cualquier parte de internet (no depende del Tailnet).
- **Acción inmediata:** revocar en GitHub → Settings → Developer settings → Personal access tokens.
- Si el repo de releases es **público**, el cliente no necesita token para leer releases:
  eliminar la clave `GitHubToken` del config y el header `Authorization` en
  `FormLogin.cs:156-159` y en `Actualizador.cs`.
- Si debe ser privado, mover la verificación de versión detrás de un endpoint propio.

### 3. Actualizador: sin verificación + Zip Slip
`DynamicSepticSystem/Actualizador.cs:485-519` (bucle de extracción principal)
- El ZIP descargado reemplaza el ejecutable **sin verificar firma ni hash**.
- `Path.Combine(tempDir, entry.FullName)` solo hace `TrimStart` de separadores; **no valida**
  que la ruta quede dentro de `tempDir` → una entrada con `..\..\` escribe fuera (Zip Slip).
- El `StartsWith` de `Actualizador.cs:894` es de `GetRelativePath`, NO protege la extracción.

**Fix Zip Slip** (dentro del `foreach` de extracción, antes de escribir):
```csharp
var fullPath = Path.GetFullPath(Path.Combine(tempDir, entryPath));
var tempDirFull = Path.GetFullPath(tempDir) + Path.DirectorySeparatorChar;
if (!fullPath.StartsWith(tempDirFull, StringComparison.OrdinalIgnoreCase))
    throw new System.Security.SecurityException($"Entrada de ZIP fuera de ruta: {entry.FullName}");
```

**Fix integridad:** publicar un SHA-256 del ZIP (idealmente firmado) y verificarlo tras
descargar y antes de extraer. A futuro, firmar el `.exe` con certificado Authenticode.

### 4. Conexión sin cifrado — bajo (Tailscale ya cifra)
`App.config:5` — `Encrypt=false;TrustServerCertificate=true`.
Buena práctica: `Encrypt=true` con certificado válido en el servidor. No urgente sobre Tailscale.

### 5. Hash de contraseñas débil
`FormLogin.cs:112` y `FormRegistrarUsuario.cs:61` — SHA-256 sin sal, una pasada.
Además `txtClave.Text.Trim()` recorta la contraseña.
**Fix:** PBKDF2 (`Rfc2898DeriveBytes`, ≥100k iteraciones, sal por usuario), bcrypt o Argon2.
Requiere migrar hashes existentes (re-hash en próximo login válido o forzar reset).
No hacer `Trim()` a la contraseña.

### 6. Autorización del lado del cliente
`FormLogin.cs:62-69` — permisos calculados en el cliente; todos comparten conexión `sa`,
así que la BD no distingue usuarios. Se mitiga al imponer permisos por usuario en SQL (#1).
El rol del cliente debe controlar solo la UI, no ser la única barrera.

### 7. Fuga de info en errores
Muchos `MessageBox.Show("Error: " + ex.Message)` y `FormLogin.cs:94` muestran `SqlException`
crudo. Registrar con `ErrorLogger` (tabla `LogErrores`) y mostrar mensaje genérico al usuario.

### 8. Login sin límite de intentos
`FormLogin.cs:30` — sin lockout ni retardo tras fallos. Añadir rate-limiting / bloqueo temporal.

---

## Notas / cosas verificadas (sin problema)

- **Inyección SQL clásica:** la mayoría de consultas usan parámetros (`AddWithValue`). Bien.
- **Nombres de tabla dinámicos** (`{tabla}`, `{nombreTabla}` en `FormCompraMulti.cs`,
  `FormEditarExplosiones.cs`): provienen de `GetExplosionTableForPrototipo()` que devuelve
  **constantes en lista blanca** (`COMPRASTUNERA`/`COMPRASCALANDRA`). No inyectable.
- **`Replace("'","''")`** en `FormAlmacen_Entradas.cs:35`, `_Salidas.cs:17`, `_ConsultaCasa.cs:463`:
  se usa en `DataTable.DefaultView.RowFilter` (filtro en memoria, no SQL del servidor). Riesgo bajo.
- `FormGenerarOrden.cs:366` tiene una conexión `(localdb)\CALANDRIA` hardcodeada e inconsistente;
  revisar si es código muerto.

## Próximo paso sugerido al retomar

Empezar por los cambios de código contenidos (no requieren infra):
- **Actualizador**: guard anti–Zip Slip + verificación de hash (`Actualizador.cs`).
- **Hashing PBKDF2** (`FormLogin.cs`, `FormRegistrarUsuario.cs`).

En paralelo, las acciones de infraestructura del usuario:
- Revocar PAT de GitHub (#2).
- Crear usuario SQL `calandria_app` y rotar `sa` (#1).
- Cifrar connection string con DPAPI (#3).
