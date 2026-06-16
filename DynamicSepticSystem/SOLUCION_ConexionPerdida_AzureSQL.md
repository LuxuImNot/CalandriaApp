# ?? SOLUCIÓN: Pérdida de Conexión a Azure SQL Database

## ?? Problema
La aplicación no puede conectarse a Azure SQL Database (servidor `calandria-sqlserver.database.windows.net`).

---

## ?? Causas Más Comunes y Soluciones

### 1. ?? IP Bloqueada por Firewall de Azure
**Causa:** Tu dirección IP pública no está en la lista blanca del firewall de Azure SQL.

**Solución:**
1. Ve a [Azure Portal](https://portal.azure.com)
2. Navega a: **SQL databases** ? **CALANDRIA** ? **Set server firewall**
3. Haz clic en **"Add client IP"** para agregar tu IP actual
4. Guarda los cambios
5. Espera 1-2 minutos y reintenta la conexión

**Comando PowerShell para obtener tu IP:**
```powershell
(Invoke-WebRequest -uri "https://api.ipify.org").Content
```

---

### 2. ?? Problemas de Conectividad de Red
**Causa:** Sin conexión a Internet o problemas de red.

**Solución:**
1. Verifica tu conexión a Internet
2. Prueba hacer ping a Azure:
   ```cmd
   ping calandria-sqlserver.database.windows.net
   ```
3. Si no responde, revisa:
   - Router/modem
   - Firewall local
   - VPN (si aplica)

---

### 3. ?? Credenciales Incorrectas
**Causa:** Usuario o contraseña incorrectos.

**Solución:**
1. Verifica en `App.config`:
   ```xml
   User ID=CloudSA9f45f232;Password=<CONTRASENA-PURGADA>
   ```
2. Si la contraseña ha cambiado:
   - Ve a Azure Portal ? SQL Server ? Settings ? **Reset password**
   - Actualiza `App.config` con la nueva contraseña
   - Recompila la aplicación

---

### 4. ?? Timeout de Conexión
**Causa:** La conexión tarda demasiado tiempo.

**Solución:**
1. Aumenta el timeout en `App.config`:
   ```xml
   Connection Timeout=60;
   ```
2. Verifica la velocidad de tu conexión a Internet
3. Considera usar una red más estable

---

### 5. ??? Problemas de SSL/TLS
**Causa:** Certificados SSL no válidos o configuración incorrecta.

**Solución:**
1. Asegúrate de que `App.config` tenga:
   ```xml
   Encrypt=True;TrustServerCertificate=False;
   ```
2. Actualiza Windows Update (puede incluir certificados raíz)

---

### 6. ?? Base de Datos Pausada o No Disponible
**Causa:** Azure SQL Database pausada o en mantenimiento.

**Solución:**
1. Ve a Azure Portal ? SQL databases ? **CALANDRIA**
2. Verifica el estado (debe estar **Online**)
3. Si está pausada, haz clic en **Resume**
4. Espera 2-3 minutos y reintenta

---

## ?? Herramienta de Diagnóstico Automático

### Ejecutar Diagnóstico desde la Aplicación:
1. En la pantalla de login, haz clic en **"?? Diagnóstico"**
2. La herramienta ejecutará automáticamente:
   - ? Prueba de conectividad de red
   - ? Prueba de conexión a Azure SQL
   - ? Verificación de configuración
   - ? Diagnóstico de errores comunes

### Interpretar Resultados:

| Símbolo | Significado |
|---------|-------------|
| ? | Prueba exitosa |
| ? | Error detectado |
| ?? | Advertencia |
| ?? | Sugerencia de solución |

---

## ?? Errores SQL Comunes

### Error 53 / -1: No se puede conectar al servidor
```
? A network-related or instance-specific error occurred
```
**Solución:** Revisa firewall de Azure (Causa #1)

---

### Error 18456: Login failed
```
? Login failed for user 'CloudSA9f45f232'
```
**Solución:** Verifica usuario/contraseña (Causa #3)

---

### Error 40613: Database unavailable
```
? Database 'CALANDRIA' on server 'calandria-sqlserver' is not currently available
```
**Solución:** Verifica que la BD esté online (Causa #6)

---

### Error 4060: Cannot open database
```
? Cannot open database "CALANDRIA" requested by the login
```
**Solución:** Verifica el nombre de la base de datos en `App.config`

---

## ??? Solución de Emergencia: Conexión Local

Si Azure SQL no está disponible, puedes usar SQL Server Express local:

### 1. Instalar SQL Server Express
```powershell
# Descargar desde: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
```

### 2. Cambiar Connection String
```xml
<connectionStrings>
  <add name="CalandriaConn" 
       connectionString="Server=.\SQLEXPRESS;Database=CALANDRIA;Integrated Security=True;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 3. Restaurar Backup
```sql
RESTORE DATABASE CALANDRIA 
FROM DISK = 'C:\Backup\CALANDRIA.bak'
WITH REPLACE;
```

---

## ?? Verificar Estado de Azure SQL

### PowerShell Script para Verificar Estado:
```powershell
# Instalar módulo de Azure
Install-Module -Name Az -AllowClobber -Scope CurrentUser

# Conectar a Azure
Connect-AzAccount

# Verificar estado de SQL Database
$resourceGroup = "CalandriaResidencial"
$serverName = "calandria-sqlserver"
$databaseName = "CALANDRIA"

Get-AzSqlDatabase -ResourceGroupName $resourceGroup `
                  -ServerName $serverName `
                  -DatabaseName $databaseName | 
    Select-Object DatabaseName, Status, CurrentServiceObjectiveName
```

---

## ?? Reintentar Conexión Automáticamente

Para agregar reintentos automáticos, modifica el código:

```csharp
using Polly;

// Política de reintentos con espera exponencial
var retryPolicy = Policy
    .Handle<SqlException>()
    .WaitAndRetry(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            Console.WriteLine($"Reintento {retryCount} después de {timeSpan.TotalSeconds}s");
        }
    );

// Ejecutar con reintentos
retryPolicy.Execute(() =>
{
    using (var conn = new SqlConnection(connectionString))
    {
        conn.Open();
        // Tu código aquí
    }
});
```

---

## ?? Registro de Cambios

### Versión 1.6.1-G (Actual)
- ? Agregado FormDiagnosticoConexion
- ? Botón de diagnóstico en FormLogin
- ? Manejo mejorado de errores de conexión
- ? Sugerencias contextuales según tipo de error

---

## ?? Contacto de Soporte

Si ninguna solución funciona:

1. **Copia el log de diagnóstico:**
   - Ejecuta diagnóstico
   - Haz clic en "?? Copiar Log"
   - Pega en un correo

2. **Envía a soporte:**
   - Email: soporte@calandriaresidencial.com
   - Incluye: Log, captura de pantalla, descripción del problema

3. **Información a incluir:**
   - Versión de la aplicación
   - Sistema operativo
   - Tipo de conexión a Internet
   - Hora exacta del error

---

## ? Checklist de Verificación Rápida

Antes de contactar soporte, verifica:

- [ ] ¿Tienes conexión a Internet?
- [ ] ¿Puedes hacer ping a `calandria-sqlserver.database.windows.net`?
- [ ] ¿Tu IP está en el firewall de Azure?
- [ ] ¿Las credenciales son correctas?
- [ ] ¿La base de datos está online en Azure Portal?
- [ ] ¿Ejecutaste el diagnóstico automático?
- [ ] ¿Probaste reiniciar la aplicación?
- [ ] ¿Probaste desde otra red (ej: datos móviles)?

---

**Última actualización:** 2025-01-XX  
**Autor:** LuxuDev  
**Versión documento:** 1.0
