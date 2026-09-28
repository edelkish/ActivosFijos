# Lógica del Muestreo 10% de AFT

## 1. Período objetivo (mes anterior)

`MuestreoDiezPorCiento()` (ctActFijosController.cs:2203) siempre trabaja sobre **el mes anterior** al actual, no sobre el del calendario de trabajo:

- `mes = hoy.Month - 1`; si es 0, pasa a diciembre del año anterior (línea 2205-2207).

Esto permite muestrear el submayor del mes cerrado justo después de cerrarlo.

## 2. Ejecución — `EjecutarMuestreo()` POST (línea 2456)

Es la pieza central (en C#, no usa el SP):

**a) Universo**: todos los ActFijo del mes objetivo (`activosDelMes`, línea 2464).

**b) Guardas**: rechaza ejecutar si el mes no tiene activos, si ya hay muestreo confirmado en BD para ese mes (`MuestreoMensualDiezPorCientos` con anno+mes), o si todos los activos ya fueron muestreados en el año.

**c) Exclusión de ya muestreados**: `yaMuestreados` = todos los `noInventario/subNumero` que ya participaron en **cualquier** muestreo del año (líneas 2488-2499). Se quitan del universo → nunca se vuelve a muestrear el mismo activo en el año.

**d) Tamaño de la muestra**: `limiteMuestra = max(1, floor(totalActivosDistintos * 0.10))` (línea 2509). Es 10% *con piso de 1*.

**e) Distribución por área con método de residuos** (líneas 2511-2536):

- Se agrupan los activos por `AreaResponsabilidad`.
- Para cada área se calcula `cuota = count_area * limiteMuestra / totalActivos` → se toma `floor` como cuota base (los restos decimales se guardan en `residuals`).
- La diferencia `limiteMuestra - sumaBase` es el remanente; se reparte **una unidad extra por área**, ordenando por residuo mayor (y desempate por cuota mayor). Es el *método del mayor residuo* para que las áreas grandes no se queden sin representación.

**f) Selección aleatoria**: por cada área, `OrderBy(_ => rnd.Next()).Take(quota)` de los activos *disponibles* del área (líneas 2546-2550). La muestra es aleatoria dentro del cupo de cada área.

**g) Persistencia provisional**: la muestra se guarda en **Session** como JSON `MuestraPendiente_{anio}_{mes}` (`mdMuestreoPendiente`), aún no en BD (líneas 2571-2579).

## 3. Flujo pendiente → confirmado

- La vista `vMuestreoDiezPorCiento` permite **editar inline** cada fila: marcar `SiExiste` (el activo se encontró físicamente o no) y escribir observaciones → `ActualizarSiExiste()` (línea 2995) actualiza la Session (o la BD si ya estaba confirmado).
- `ConfirmarMuestreo()` POST (línea 2933): recibe los items desde la sesión y los **inserta en BD** (`MuestreoMensualDiezPorCientos`) con `SiExiste = true`, luego elimina la Session.
- Si ya existían registros del mes (reconfirmación) solo resetea `SiExiste`/`Observaciones` de la muestra confirmada.

## 4. Consulta y edición posterior

`ConsultaMuestreoDiezPorCiento()` (línea 2344) recalcula en memoria el mismo modelo de distribución (tamaño 10%, cuotas por área con residuos) para **reconstruir el resumen por área** e identificar qué inventarios de la muestra confirmada caen dentro del cupo esperado. Solo se edita el mes anterior (`editable`, línea 2446-2449).

## 5. Exportación del acta

`ExportarActaMuestreo()` (línea 2598) llena `PlantillaActaMuestreo.xlsx` agrupando la muestra por centro de costo, calculando subtotales/totales por grupo y ajustando filas en blanco para que cada grupo "llene" la página del acta.

## Detalle: el SP es código muerto

`repoActivosFijos.EjecutarMuestreoDiezPorCiento()` (repoActivosFijos.cs:892) ejecuta `EXEC sp_MuestreoMensualDiezPorCiento_ActFijos` (una versión más antigua del muestreo con priorización por área y `NEWID()`), pero **no se invoca desde ningún controller** — la lógica activa es la C# del POST `EjecutarMuestreo`.