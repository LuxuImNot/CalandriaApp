; Instalador de Calandria Residencial (Inno Setup).
; Se compila con ISCC.exe (parte de Inno Setup: https://jrsoftware.org/isdl.php),
; normalmente invocado por publicar-release.ps1, que le pasa la version real con
; /DMyAppVersion=X.X.X.X. Si lo compilas a mano sin ese define, usa "0.0.0.0".
;
; Ojo con [Files]: excluye secrets.config y los *.example a proposito (ver abajo).

#define MyAppName "Pilaris"
#define MyAppPublisher "Calandria"
#define MyAppExeName "DynamicSepticSystem.exe"
#define MyAppSourceDir "DynamicSepticSystem\bin\Release"
#ifndef MyAppVersion
  #define MyAppVersion "0.0.0.0"
#endif

[Setup]
; GUID fijo: NO cambiar entre versiones (identifica la app para que "instalar
; encima" actualice en vez de duplicar). Generado una sola vez para este proyecto.
AppId={{7C6C7A3E-8C39-4B7B-9C4E-2C6B7B0C6A2D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\CalandriaResidencial
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=.
OutputBaseFilename=CalandriaSetup-v{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardStyle=modern
; No pide elevacion salvo que Program Files la requiera (la pide Windows solo).
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el Escritorio"; GroupDescription: "Accesos directos:"

[Files]
; Todo bin\Release EXCEPTO secrets.config y los *.example: ese archivo trae el
; token personal de GitHub del desarrollador que compilo el instalador, y la
; app funciona sin el (solo baja el limite de peticiones a la API de GitHub).
; connectionStrings.config SI se incluye: la app todavia lo necesita mientras
; dure la migracion a la Web API (formularios que aun hacen SQL directo).
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "secrets.config,secrets.config.example,connectionStrings.config.example"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar {#MyAppName}"; Flags: nowait postinstall skipifsilent
