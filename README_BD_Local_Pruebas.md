# BD de pruebas local (sin Tailscale)

Entorno local completo para desarrollar sin depender de la conexión Tailscale
al servidor de producción: SQL Server Express local + Calandria.Api local +
cliente apuntando a ambos.

## Qué se creó

- **`CalandriaControl`** (BD maestra, en `localhost\SQLEXPRESS`): usuarios,
  perfiles y obras. Creada con `SQL_Local_CrearBDMaestra.sql` (raíz del repo).
  Usuario semilla: **`admin` / `admin123`** (perfil "Admin" con todos los
  permisos). Cámbiala si vas a dejar este entorno corriendo un tiempo.
- **Obra "Calandria Residencial (local)"**: reutiliza tu BD local `CALANDRIA`
  que ya existía en SQLEXPRESS (creada 10-jul-2025, con datos reales: 118
  casas, presupuesto, avance, proveedores). Se le aplicó
  `Calandria.Api/Sql/ObraPlantilla.sql` (el mismo script que usa
  `ObrasController.Crear` para aprovisionar obras nuevas) para ponerla al día
  con las tablas agregadas después de julio 2025 (`RutaCalandraDestajo`,
  `RutaTuneraDestajo`, `SalidasAlmacen`, `NominaTareasAsignada`, `OrdenesCompra*`,
  `ConciliacionesIaLog`, etc.). El script solo crea lo que faltaba (CREATE
  TABLE/ALTER ADD/INSERT de catálogo, nada de DROP/TRUNCATE/DELETE); tus 118
  casas y el resto de datos existentes no se tocaron. El árbol de destajos
  queda vacío porque esas tablas no existían en el snapshot original — no hay
  forma de recuperar ese dato, es información nueva que se irá llenando al
  usar la app.
- **`connectionStrings.config`** (Calandria.Api y DynamicSepticSystem):
  apuntan a `localhost\SQLEXPRESS` con autenticación de Windows. El bloque
  original de producción quedó comentado justo debajo, en el mismo archivo.
- **`secrets.config`** (Calandria.Api): `BaseUrl` sobreescrito a
  `http://localhost:8733` (loopback puro, no requiere permisos de admin —
  el de `App.config`, `http://+:8733`, sí los pide).
- **`secrets.config`** (DynamicSepticSystem): `ApiBaseUrl` sobreescrito a
  `http://localhost:8733`.

Ninguno de estos tres `.config` está en git (ver `.gitignore`), así que nada
de esto afecta al resto del equipo ni a producción.

## Cómo usarlo

1. Levanta el API local (deja la consola abierta):
   ```
   cd Calandria.Api
   dotnet run
   ```
   Debe imprimir `Calandria API en ejecución en http://localhost:8733`.
2. Corre el cliente normal (`DynamicSepticSystem.exe` / F5 en Visual Studio) e
   inicia sesión con `admin` / `admin123`.

## Cómo volver a producción

En cada uno de estos 4 archivos, comenta el bloque "local" y descomenta el de
producción (o borra las líneas que agregó esta sesión):

- `Calandria.Api/connectionStrings.config`
- `DynamicSepticSystem/connectionStrings.config`
- `Calandria.Api/secrets.config` (quita el `<remove>`/`<add>` de `BaseUrl`)
- `DynamicSepticSystem/secrets.config` (quita el `<remove>`/`<add>` de `ApiBaseUrl`)

## Re-crear desde cero

Si algún día quieres tirar `CalandriaControl` y rehacerla:
```
sqlcmd -S "localhost\SQLEXPRESS" -E -C -Q "DROP DATABASE CalandriaControl"
sqlcmd -S "localhost\SQLEXPRESS" -E -C -i SQL_Local_CrearBDMaestra.sql
```
(Es idempotente, así que también puedes simplemente volver a correr el script
sin borrar nada.) Luego vuelve a registrar la obra:
```sql
USE CalandriaControl;
INSERT INTO Obras (Nombre, NombreBD, Activa) VALUES ('Calandria Residencial (local)', 'CALANDRIA', 1);
INSERT INTO UsuarioObras (Usuario, ObraId) SELECT 'admin', Id FROM Obras WHERE NombreBD='CALANDRIA';
```
