# Plan — Generación de Anexos A3 (Movimientos Mensuales)

## Objetivo
Generar, desde la pantalla "Detalles de Movimientos", un documento Word **Anexo 3 (SC-1-01)** por cada movimiento mensual de activos fijos, empaquetados en un ZIP descargable, rellenando los campos de la plantilla `Plantilla Anexo 3 Modelo de Mov de AFT. MODELO.docx`.

## Decisiones confirmadas
| Tema | Decisión |
|---|---|
| Formato de salida | ZIP con **1 Anexo por movimiento** (`Anexos Movimientos (<mes>).zip`) |
| `ENTIDAD_`/`DIRECCION_`/`UEB_`/`DIRECCION_UEB_` | Maestros desde **pantalla Configuraciones** (tabla `Parametro`) |
| Tipo de movimiento | **Nomenclador configurable**, default "Traslado Interno"; se marca con `X` en doc |
| `CODG`, `711`, `Tiempo`, `Devolución`, Subcuenta | **Sin tocar** (se llenan a mano); solo se autorrellena `NOINVENTARIO_` y Valor $ |
| `DESCRIPCION_` | Descripción del AFT (`mv.Descripcion`) |
| `NOMBRE_`/`CARGO_` (informe técnico) | Modal con **listas desplegables** (Nombres, Cargos) + **textarea** informe técnico |
| `FUNDAMENTACIONOPE_` | Textarea en el modal de generación |
| Comprobante No. | Input en el modal (con autogenerado opcional) |

## Mapeo de campos de la plantilla
| Campo plantilla | Fuente |
|---|---|
| `ENTIDAD_`, `DIRECCION_`, `UEB_`, `DIRECCION_UEB_` | `Parametro` (Configuraciones) |
| `AREA_` | `AreaAnterior` |
| `AREARECEP_`, `NOMBRERECEP_` | `AreaActual`, `ResponsableActual` |
| `DESCRIPCION_` | `Descripcion` del AFT |
| `NOINVENTARIO_` | `NoInventario` |
| Valor $ | `ValorContable`/`ValorAdquisicion` del mes actual |
| Fechas D/M/A | Día/mes/año de `FechaSubmayor` |
| `FUNDAMENTACIONOPE_` | Modal (textarea) |
| `INFORMETECNICO_` | Modal (textarea) |
| `NOMBRE_` / `CARGO_` | Modal (dropdowns; fuentes: `Usuarios`, `Cargos`) |
| Comprobante No. | Modal |
| `HECHOPOR_` | Usuario logueado (claim `UserData`) |
| `APROBADOPOR_` | `ObtenerInfoDirector()` |
| `AUTORIZADOPOR_` | `ObtenerInfoEncargadoEconomia()` |
| Tipo de movimiento | Nomenclador (X en el elegido, O en el resto) |

## Pasos de implementación

### 1. Entidad `TipoMovimientoAFT` (nomenclador)
- Nuevo modelo (`Id` Guid, `Nombre`, `Descripcion`, `Orden`, `SiActivo`).
- `DbSet` en `aftContext` + migración + **seed de los 13 tipos** de la plantilla (Compra AFT Nuevo, Compra AFT de Uso, Traspaso recibido, Ajuste Inv. Alta, Venta, Traslado Interno, Activo-Ocioso, Retiro, Pérdida, Traspaso efect., Ajuste Baja, Enviado a reparar, Ocioso-Activo).

### 2. Entidad `Parametro` (maestros del Anexo)
- Nuevo modelo (`Id` Guid, `Nombre`, `Valor`, `Grupo`).
- `DbSet` + migración. CRUD en la **pantalla Configuraciones** (patrón de `ctConfiguracion`).
- Registra por defecto: Entidad, Dirección, UEB, Dirección UEB.

### 3. Nomenclador de tipos de movimiento (CRUD + UI)
- `ctTipoMovimientoController` + `repoTipoMovimiento` + vistas (listado/crear/editar/eliminar) replicando el patrón existente.
- Registro DI en `Program.cs` + entrada de menú.

### 4. Generador DOCX (`repoServcioActas` o nuevo `repoAnexoMovimientos`)
- `ReemplazarMarcador(párrafo, token, valor)`: concatena runs del párrafo, localiza `XXX_` y escribe el valor fusionando runs fragmentados (necesario: `ENTIDAD_` viene partido `ENTIDAD` + `_`).
- `MarcarTipoMovimiento(doc, tipo)`: `X` en el tipo elegido, `O` en los demás.
- `ExportarAnexo3(...)`: abre la plantilla con `WordprocessingDocument`, aplica el mapeo de arriba y devuelve el docx en memoria.

### 5. Datos del movimiento
- Extender `ListadoActivosFijosConMovimientos` (o join en repo) para traer `ValorContable`, `ValorAdquisicion`, `DepreciacionAcum`, `Clasificador` y fecha.

### 6. Endpoint de generación
- `ctActFijos/GenerarAnexosMovimientos` (POST): filtra por fecha/área/responsable (misma lógica que `GenerarActaAFT`), genera 1 Anexo por movimiento y devuelve ZIP. Sin movimientos → mensaje "No existen movimientos en la fecha seleccionada."

### 7. UI del botón y modal
- En `vDetallesDeMovimientos.cshtml`: botón **"Generar Anexo 3"** + modal con: tipo de movimiento (select, default Traslado Interno), fundamentación (textarea), informe técnico (textarea), nombre (dropdown), cargo (dropdown), comprobante (input), fecha (default).

## Verificación
1. `dotnet build ActivosFijos.sln` tras cada paso.
2. Prueba manual: mes con movimientos → generar ZIP → abrir Anexo → confirmar `_` rellenados, tipo marcado con X, inventario y valor presentes.