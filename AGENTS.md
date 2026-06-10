# ActivosFijos — guia para OpenCode

## Build, run, watch
- `dotnet build ActivosFijos.sln`
- `dotnet run --project ActivosFijos` (https://localhost:7074, http://localhost:5074)
- `dotnet watch run --project ActivosFijos.sln`

## Base de datos
- SQL Server `.\SQLSERVER2022`, db `ActivosFijos_I`, user `sa`/`sa` — ver `appsettings.json:ConnectionStrings:aftConnection`
- Migrations EF Core ya aplicadas en `Migrations/`. Tool `dotnet-ef` 8.0.8 en `.config/dotnet-tools.json`
- Timeout de queries: 10 minutos (hardcodeado en `Program.cs`)
- NO hay proyecto de test ni CI configurado

## Arquitectura
- MVC con capa de repositorios: `Controllers/` -> `Repositorios/` -> `Models/Contexts/` (EF Core)
- Un solo proyecto net8.0 en la solucion. Sin tests.
- DbContexts duplicados: `aftContext` y `aftContextAux` — ambos apuntan a la misma conexion SQL Server

## Convenciones del codigo
- Controladores con prefijo `ct` (ej. `ctAutenticacionController`). Vistas organizadas por nombre sin sufijo `Controller`
- Repositorios con prefijo `repo`, modelos de vista con prefijo `md`
- Estado global via clase estatica `Clases/clGlobal.cs` (propiedades estaticas compartidas entre requests)
- Ruta default: `{controller=ctAutenticacion}/{action=Autenticacion}/{id?}`

## Auth & seguridad
- Cookie auth con esquema personalizado `"Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme`
- Login: `/ctAutenticacion`, claim `UserData` = nombre completo
- Anti-forgery global via cabecera `XSRF-TOKEN` (config en `Program.cs`)
- Sesion: 30 min timeout, HttpOnly, Essential

## Librerias clave
- **EPPlus** 8.0.6 — licencia `NonCommercial` seteada en `appsettings.json:EPPlus:LicenseContext`
- **OpenXml** — documentos Word (plantillas en `wwwroot/plantillas/`)
- **DbfDataReader** — importacion archivos DBF
- **Swashbuckle** — Swagger disponible

## Deploy
- Perfil `FolderProfile.pubxml` -> `E:\Publicaciones\Control de activos`, config Release
