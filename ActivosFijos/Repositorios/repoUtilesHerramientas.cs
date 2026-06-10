using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.AuxBD;
using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoUtilesHerramientas : iClone<UtilesHerramienta>
    {
        private aftContext _context;
        private aftContextAux _contextAux;

        public repoUtilesHerramientas(aftContext context, aftContextAux contextAux)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAux = contextAux ?? throw new ArgumentNullException(nameof(contextAux));
        }
        public async Task<List<mdMovimientosAFT>> ListadoActivosFijos()
        {
            List<mdMovimientosAFT> actFijos = await _context.ActFijos
                .GroupBy(c => new { c.NoInventario, c.Descripcion })
                .Select(g => new mdMovimientosAFT{ noInventario = g.Key.NoInventario, subNumero = g.Max(x => x.SubNumero), descripcion = g.Key.Descripcion})
                .ToListAsync();
            return actFijos;
        }
        public async Task<List<UtilesHerramienta>> ListadoUTH(DateTime fecha)
        {
            return await _context.UtilesHerramientas
                .Include(a => a.AreaResponsabilidadNavigation)
                .Include(a => a.CentroCostoNavigation)
                .Include(a => a.ResponsableNavigation)
                .Where(act => act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month)
                .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                    .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                    .ThenBy(act => act.Codigo)
                .ToListAsync();
        }
        public async Task<List<UtilesHerramienta>> ListadoUTH(DateTime fecha, Guid area, Guid responsable)
        {
            if (area == Guid.Empty && responsable == Guid.Empty)
            {
                return await _context.UtilesHerramientas
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month)
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.Codigo)
                    .ToListAsync();
            } else if (area == Guid.Empty && responsable != Guid.Empty)
            {
                return await _context.UtilesHerramientas
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month && act.Responsable.Equals(responsable))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.Codigo)
                    .ToListAsync();
            }
            else if (area != Guid.Empty && responsable == Guid.Empty)
            {
                return await _context.UtilesHerramientas
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month && act.AreaResponsabilidad.Equals(area))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.Codigo)
                    .ToListAsync();
            } else
            {
                return await _context.UtilesHerramientas
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month && act.Responsable.Equals(responsable) && act.AreaResponsabilidad.Equals(area))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.Codigo)
                    .ToListAsync();
            }
        }
        public List<UtilesHerramienta> UTHs(DateTime fecha, Guid area, Guid responsable)
        {
            if (area == Guid.Empty && responsable == Guid.Empty)
            {
                return _context.UtilesHerramientas
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month)
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.Codigo)
                    .ToList();
            }
            else if (area == Guid.Empty && responsable != Guid.Empty)
            {
                return _context.UtilesHerramientas
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month && act.Responsable.Equals(responsable))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.Codigo)
                    .ToList();
            }
            else if (area != Guid.Empty && responsable == Guid.Empty)
            {
                return _context.UtilesHerramientas
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month && act.AreaResponsabilidad.Equals(area))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.Codigo)
                    .ToList();
            }
            else
            {
                return _context.UtilesHerramientas
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month 
                        && act.Responsable.Equals(responsable) && act.AreaResponsabilidad.Equals(area))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.Codigo)
                    .ToList();
            }
        }
        public async Task<UtilesHerramienta> EncontrarUTHID(Guid? id)
        {
            var uth = await _context.UtilesHerramientas
                .Include(a => a.AreaResponsabilidadNavigation)
                .Include(a => a.CentroCostoNavigation)
                .Include(a => a.ResponsableNavigation)
                .FirstOrDefaultAsync(m => m.IdUth == id);

            return uth ?? new UtilesHerramienta();
        }
        public async Task<UtilesHerramienta> EncontrarUTHID(Guid? id, DateTime fecha)
        {
            var uth = await _context.UtilesHerramientas
                .Include(a => a.AreaResponsabilidadNavigation)
                .Include(a => a.CentroCostoNavigation)
                .Include(a => a.ResponsableNavigation)
                .SingleOrDefaultAsync(a => a.FechaListadoUth.Year.Equals(fecha.Year) && a.FechaListadoUth.Month.Equals(fecha.Month) && a.IdUth.Equals(id));

            return uth ?? new UtilesHerramienta();
        }
        public bool SiExisteActivoFijoConElID(long? id)
        {
            return _context.ActFijos.Any(e => e.NoInventario == id);
        }
        public bool SiExisteActivoFijoConNoInventario(long inventario)
        {
            return _context.ActFijos.Any(e => e.NoInventario == inventario);
        }
        public bool SiExisteActivoFijoConNoInventarioEditado(long noInventario)
        {
            bool siExiste = _context.ActFijos.Any(e => e.NoInventario == noInventario);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                //siExiste = _context.ActFijos.Any(e => e.NoInventario == noInventario && e.IdInventario == id);
                if (siExiste)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public async Task<clResponse<UtilesHerramienta>> GuardarActivoFijo(UtilesHerramienta uth, bool clonar = false)
        {
            try
            {
                if (uth == null)
                    return new clResponse<UtilesHerramienta>() { SiSatisfactorio = false, Mensaje = "El UTH a insertar no existe." };
                else
                {
                    _context.Add(uth);
                    await _context.SaveChangesAsync();

                    return new clResponse<UtilesHerramienta>() { SiSatisfactorio = true, Mensaje = $"El UTH con el Código {uth.Codigo} ha sido insertado satisfactoriamente.", Resultado = clonar ? Clonar(uth) : uth };
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteActivoFijoConElID(uth.No))
                {
                    return new clResponse<UtilesHerramienta>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a insertar no existe." };
                }
                else
                {
                    return new clResponse<UtilesHerramienta>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido un error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteActivoFijoConElID(uth.No))
                {
                    return new clResponse<UtilesHerramienta>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a insertar no existe." };
                }
                else
                {
                    return new clResponse<UtilesHerramienta>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<ActFijo>> ModificarActivoFijo(ActFijo actFijo)
        {
            try
            {
                if (actFijo == null)
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a modificar no existe." };

                _context.Update(actFijo);
                await _context.SaveChangesAsync();
                return new clResponse<ActFijo>() { SiSatisfactorio = true, Mensaje = "El Activo Fijo ha sido modificado satisfactoriamente.", Resultado = actFijo };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteActivoFijoConElID(actFijo.NoInventario))
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a modificar no existe." };
                }
                else
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteActivoFijoConElID(actFijo.NoInventario))
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a modificar no existe." };
                }
                else
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<ActFijo>> EliminarActivoFijo(ActFijo actFijo)
        {
            if (actFijo == null)
                return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a eliminar es incorrecto." };
            try
            {
                _context.ActFijos.Remove(actFijo);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<ActFijo>() { SiSatisfactorio = true, Mensaje = $"El Activo Fijo con el No. de Inventario {actFijo.NoInventario} ha sido eliminado satisfactoriamente." };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteActivoFijoConElID(actFijo.NoInventario))
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a eliminar no existe." };
                }
                else
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message + " " + ex.InnerException.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteActivoFijoConElID(actFijo.NoInventario))
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a eliminar no existe." };
                }
                else
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex1.Message + " " + ex1.InnerException.Message };
                }
            }
        }
        public async Task<List<ExistenciaMovimientoUTH>> DeterminarSiExisteMovimientoDeUTH(DateTime fecha) {
            try
            {
                string fechaMov = fecha.ToString("yyyy-MM-dd");
                List<ExistenciaMovimientoUTH> query = new List<ExistenciaMovimientoUTH>();
                query = await _contextAux.ExistenciaMovimientosUTH.FromSqlRaw($@"DECLARE @fecha DATETIME = CONVERT(DATETIME, '{fechaMov}')

                                                                                SELECT Tabla.codigo AS [Codigo],
                                                                                        CONCAT(Tabla.codigo, '|', Tabla.responsable, '|', Tabla.centroCosto, '|', Tabla.areaResponsabilidad) AS [Discriminante],
                                                                                        COUNT(*) AS [Cantidad]
                                                                                FROM
                                                                                (
                                                                                    SELECT codigo,
                                                                                            descripcion,
                                                                                            responsable,
                                                                                            centroCosto,
                                                                                            areaResponsabilidad
                                                                                    FROM dbo.UtilesHerramientas
                                                                                    WHERE YEAR(fechaListadoUTH) = YEAR(@fecha)
                                                                                            AND MONTH(fechaListadoUTH) = MONTH(DATEADD(MONTH, -1, @fecha))
                                                                                    UNION ALL
                                                                                    SELECT codigo,
                                                                                            descripcion,
                                                                                            responsable,
                                                                                            centroCosto,
                                                                                            areaResponsabilidad
                                                                                    FROM dbo.UtilesHerramientas
                                                                                    WHERE YEAR(fechaListadoUTH) = YEAR(@fecha)
                                                                                            AND MONTH(fechaListadoUTH) = MONTH(@fecha)
                                                                                ) AS Tabla
                                                                                GROUP BY Tabla.codigo,
                                                                                            Tabla.descripcion,
                                                                                            Tabla.responsable,
                                                                                            Tabla.centroCosto,
                                                                                            Tabla.areaResponsabilidad,
                                                                                            CONCAT(Tabla.codigo, '|', Tabla.responsable, '|', Tabla.centroCosto, '|', Tabla.areaResponsabilidad)
                                                                                HAVING COUNT(*) = 1;").ToListAsync();
                
                return query;
            }
            catch (Exception ex)
            {
                return new List<ExistenciaMovimientoUTH>();
            }
        }
        public async Task<List<UTHConMovimientosParcial>> ListadoUTHConMovimientos(DateTime fecha)
        {
            try
            {
                string fechaMov = fecha.ToString("yyyy-MM-dd");
                List<UTHConMovimientosParcial> query = new List<UTHConMovimientosParcial>();
                query = await _contextAux.UTHConMovimientosParcial.FromSqlRaw($@"DECLARE @fechaAnterior DATETIME,
                                                                                            @fechaActual DATETIME = CONVERT(DATETIME, '{fechaMov}');

                                                                                    DECLARE @fecha DATETIME = CONVERT(DATETIME, @fechaActual);

                                                                                    SET @fechaAnterior = DATEADD(MONTH, -1, @fecha);

                                                                                    DECLARE @tablaDeMovimientos TABLE
                                                                                    (
                                                                                        Codigo VARCHAR(100),
                                                                                        Descripcion VARCHAR(2000),
                                                                                        Responsable UNIQUEIDENTIFIER,
                                                                                        CentroCosto UNIQUEIDENTIFIER,
                                                                                        Area UNIQUEIDENTIFIER,
                                                                                        Discriminante VARCHAR(500)
                                                                                    );

                                                                                    INSERT @tablaDeMovimientos
                                                                                    SELECT UTH.Codigo,
                                                                                           UTH.Descripcion,
                                                                                           UTH.Responsable,
                                                                                           UTH.CentroCosto,
                                                                                           UTH.Area,
                                                                                           UTH.Discriminante
                                                                                    FROM
                                                                                    (
                                                                                        SELECT codigo,
                                                                                               descripcion,
                                                                                               responsable,
                                                                                               centroCosto,
                                                                                               areaResponsabilidad AS [Area],
                                                                                               CONCAT(codigo, '|', responsable, '|', centroCosto, '|', areaResponsabilidad) AS [Discriminante]
                                                                                        FROM dbo.UtilesHerramientas
                                                                                        WHERE YEAR(fechaListadoUTH) = YEAR(@fechaAnterior)
                                                                                              AND MONTH(fechaListadoUTH) = MONTH(@fechaAnterior)
                                                                                        GROUP BY codigo,
                                                                                                 descripcion,
                                                                                                 responsable,
                                                                                                 centroCosto,
                                                                                                 areaResponsabilidad,
                                                                                                 CONCAT(codigo, '|', responsable, '|', centroCosto, '|', areaResponsabilidad)
                                                                                        UNION ALL
                                                                                        SELECT codigo,
                                                                                               descripcion,
                                                                                               responsable,
                                                                                               centroCosto,
                                                                                               areaResponsabilidad AS [Area],
                                                                                               CONCAT(codigo, '|', responsable, '|', centroCosto, '|', areaResponsabilidad)
                                                                                        FROM dbo.UtilesHerramientas
                                                                                        WHERE YEAR(fechaListadoUTH) = YEAR(@fecha)
                                                                                              AND MONTH(fechaListadoUTH) = MONTH(@fecha)
                                                                                        GROUP BY codigo,
                                                                                                 descripcion,
                                                                                                 responsable,
                                                                                                 centroCosto,
                                                                                                 areaResponsabilidad,
                                                                                                 CONCAT(codigo, '|', responsable, '|', centroCosto, '|', areaResponsabilidad)
                                                                                    ) UTH
                                                                                    GROUP BY UTH.Codigo,
                                                                                             UTH.Descripcion,
                                                                                             UTH.Responsable,
                                                                                             UTH.CentroCosto,
                                                                                             UTH.Area,
                                                                                             UTH.Discriminante
                                                                                    HAVING COUNT(*) = 1;

                                                                                    SELECT MOV.codigo,
                                                                                           MOV.descripcion,
                                                                                           MOV.responsable,
                                                                                           MOV.centroCosto,
                                                                                           MOV.areaResponsabilidad as [area],
                                                                                           MOV.Discriminante,
                                                                                           MOV.CantidadAnterior,
                                                                                           MOV.Cantidad
                                                                                    FROM
                                                                                    (
                                                                                        SELECT codigo,
                                                                                               descripcion,
                                                                                               responsable,
                                                                                               centroCosto,
                                                                                               areaResponsabilidad,
                                                                                               CONCAT(codigo, '|', responsable, '|', centroCosto, '|', areaResponsabilidad) AS [Discriminante],
                                                                                               0 AS [CantidadAnterior],
                                                                                               COUNT(*) AS [Cantidad]
                                                                                        FROM dbo.UtilesHerramientas
                                                                                        WHERE YEAR(fechaListadoUTH) = YEAR(@fecha)
                                                                                              AND (MONTH(fechaListadoUTH) = MONTH(@fecha))
                                                                                              AND codigo IN
                                                                                                  (
                                                                                                      SELECT DISTINCT Codigo FROM @tablaDeMovimientos
                                                                                                  )
                                                                                        GROUP BY codigo,
                                                                                                 descripcion,
                                                                                                 responsable,
                                                                                                 centroCosto,
                                                                                                 areaResponsabilidad,
                                                                                                 CONCAT(codigo, '|', responsable, '|', centroCosto, '|', areaResponsabilidad)
                                                                                        UNION
                                                                                        SELECT codigo,
                                                                                               descripcion,
                                                                                               responsable,
                                                                                               centroCosto,
                                                                                               areaResponsabilidad,
                                                                                               CONCAT(codigo, '|', responsable, '|', centroCosto, '|', areaResponsabilidad) AS [Discriminante],
                                                                                               COUNT(*) AS [CantidadAnterior],
                                                                                               0 AS [Cantidad]
                                                                                        FROM dbo.UtilesHerramientas
                                                                                        WHERE YEAR(fechaListadoUTH) = YEAR(@fechaAnterior)
                                                                                              AND (MONTH(fechaListadoUTH) = MONTH(@fechaAnterior))
                                                                                              AND codigo IN
                                                                                                  (
                                                                                                      SELECT DISTINCT Codigo FROM @tablaDeMovimientos
                                                                                                  )
                                                                                        GROUP BY codigo,
                                                                                                 descripcion,
                                                                                                 responsable,
                                                                                                 centroCosto,
                                                                                                 areaResponsabilidad,
                                                                                                 CONCAT(codigo, '|', responsable, '|', centroCosto, '|', areaResponsabilidad)
                                                                                    ) MOV;").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<UTHConMovimientosParcial>();
            }
        }
        public async Task<List<ActivosFijosMovimientos>> ListadoActivosFijosMovimientos(long noInventario)
        {
            try
            {
                List<ActivosFijosMovimientos> query = new List<ActivosFijosMovimientos>();
                query = await _contextAux.ActivosFijosMovimientos.FromSqlRaw($@"DECLARE @tablaTemp TABLE
                                                                                (
                                                                                    noInventario BIGINT,
                                                                                    descripcion VARCHAR(2000),
                                                                                    responsable UNIQUEIDENTIFIER,
                                                                                    areaResponsabilidad UNIQUEIDENTIFIER,
                                                                                    fechaSubmayor DATETIME
                                                                                );

                                                                                DECLARE @contador INT;
                                                                                SET @contador = 1;

                                                                                INSERT @tablaTemp
                                                                                SELECT noInventario,
                                                                                        descripcion,
                                                                                        responsable,
                                                                                        areaResponsabilidad,
                                                                                        MIN(fechaSubmayor)
                                                                                FROM dbo.ActFijo
                                                                                WHERE noInventario = '{noInventario}'
                                                                                GROUP BY noInventario,
                                                                                            descripcion,
                                                                                            responsable,
                                                                                            areaResponsabilidad;

                                                                                /* declare variables */
                                                                                DECLARE @noInventario BIGINT,
                                                                                        @descripcion VARCHAR(2000),
                                                                                        @responsable UNIQUEIDENTIFIER,
                                                                                        @areaResponsabilidad UNIQUEIDENTIFIER,
		                                                                                @fechaSubmayor DATETIME;

                                                                                DECLARE movimientos CURSOR FAST_FORWARD READ_ONLY FOR
                                                                                SELECT noInventario,
                                                                                        descripcion,
                                                                                        responsable,
                                                                                        areaResponsabilidad,
                                                                                        fechaSubmayor
                                                                                FROM @tablaTemp
                                                                                ORDER BY fechaSubmayor ASC;

                                                                                OPEN movimientos;

                                                                                FETCH NEXT FROM movimientos
                                                                                INTO @noInventario,
                                                                                        @descripcion,
                                                                                        @responsable,
                                                                                        @areaResponsabilidad,
	                                                                                    @fechaSubmayor;

                                                                                WHILE @@FETCH_STATUS = 0
                                                                                BEGIN
                                                                                    IF (@contador = 1)
                                                                                    BEGIN
                                                                                        UPDATE @tablaTemp
                                                                                        SET fechaSubmayor =
                                                                                            (
                                                                                                SELECT MAX(fechaSubmayor)
                                                                                                FROM dbo.ActFijo
                                                                                                WHERE noInventario = @noInventario
                                                                                                        AND responsable = @responsable
                                                                                                        AND areaResponsabilidad = @areaResponsabilidad
                                                                                            )
                                                                                        WHERE [@tablaTemp].noInventario = @noInventario
                                                                                                AND [@tablaTemp].responsable = @responsable
                                                                                                AND [@tablaTemp].areaResponsabilidad = @areaResponsabilidad;

                                                                                        SET @contador = @contador + 1;
                                                                                    END;

                                                                                    FETCH NEXT FROM movimientos
                                                                                    INTO @noInventario,
                                                                                            @descripcion,
                                                                                            @responsable,
                                                                                            @areaResponsabilidad,
		                                                                                    @fechaSubmayor;
                                                                                END;

                                                                                CLOSE movimientos;
                                                                                DEALLOCATE movimientos;

                                                                                SELECT noInventario,
                                                                                        descripcion,
                                                                                        responsable,
                                                                                        (
                                                                                            SELECT CONCAT(nombre, ' ', apellidos)
                                                                                            FROM dbo.Responsable
                                                                                            WHERE idResponsable = [@tablaTemp].responsable
                                                                                        ) AS [nombreResponsable],
                                                                                        areaResponsabilidad,
                                                                                        (
                                                                                            SELECT Nombre
                                                                                            FROM dbo.AreaDeResponsabilidad
                                                                                            WHERE idArea = [@tablaTemp].areaResponsabilidad
                                                                                        ) AS [nombreAreaResponsabilidad],
                                                                                        fechaSubmayor
                                                                                FROM @tablaTemp
                                                                                ORDER BY fechaSubmayor DESC;").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ActivosFijosMovimientos>();
            }
        }
        public async Task<List<ActivosFijosMovimientosResponsables>> ListadoActivosFijosMovimientos(Guid responsable)
        {
            try
            {
                List<ActivosFijosMovimientosResponsables> query = new List<ActivosFijosMovimientosResponsables>();
                query = await _contextAux.ActivosFijosMovimientosResponsables.FromSqlRaw($@"DECLARE @tablaTemp TABLE
                                                                                (
                                                                                    noInventario BIGINT,
                                                                                    descripcion VARCHAR(2000),
                                                                                    responsable UNIQUEIDENTIFIER,
                                                                                    areaResponsabilidad UNIQUEIDENTIFIER,
                                                                                    fechaSubmayor DATETIME
                                                                                );

                                                                                DECLARE @contador INT;
                                                                                SET @contador = 1;

                                                                                INSERT @tablaTemp
                                                                                SELECT noInventario,
                                                                                       descripcion,
                                                                                       responsable,
                                                                                       areaResponsabilidad,
                                                                                       MIN(fechaSubmayor)
                                                                                FROM dbo.ActFijo
                                                                                GROUP BY noInventario,
                                                                                         descripcion,
                                                                                         responsable,
                                                                                         areaResponsabilidad;

                                                                                /* declare variables */
                                                                                DECLARE @noInventario BIGINT,
                                                                                        @descripcion VARCHAR(2000),
                                                                                        @responsable UNIQUEIDENTIFIER,
                                                                                        @areaResponsabilidad UNIQUEIDENTIFIER,
                                                                                        @fechaSubmayor DATETIME;

                                                                                DECLARE movimientos CURSOR FAST_FORWARD READ_ONLY FOR
                                                                                SELECT noInventario,
                                                                                       descripcion,
                                                                                       responsable,
                                                                                       areaResponsabilidad,
                                                                                       fechaSubmayor
                                                                                FROM @tablaTemp
                                                                                ORDER BY fechaSubmayor ASC;

                                                                                OPEN movimientos;

                                                                                FETCH NEXT FROM movimientos
                                                                                INTO @noInventario,
                                                                                     @descripcion,
                                                                                     @responsable,
                                                                                     @areaResponsabilidad,
                                                                                     @fechaSubmayor;

                                                                                WHILE @@FETCH_STATUS = 0
                                                                                BEGIN
                                                                                    IF (@contador = 1)
                                                                                    BEGIN
                                                                                        UPDATE @tablaTemp
                                                                                        SET fechaSubmayor =
                                                                                            (
                                                                                                SELECT MAX(fechaSubmayor)
                                                                                                FROM dbo.ActFijo
                                                                                                WHERE noInventario = @noInventario
                                                                                                      AND responsable = @responsable
                                                                                                      AND areaResponsabilidad = @areaResponsabilidad
                                                                                            )
                                                                                        WHERE [@tablaTemp].noInventario = @noInventario
                                                                                              AND [@tablaTemp].responsable = @responsable
                                                                                              AND [@tablaTemp].areaResponsabilidad = @areaResponsabilidad;

                                                                                        SET @contador = @contador + 1;
                                                                                    END;

                                                                                    FETCH NEXT FROM movimientos
                                                                                    INTO @noInventario,
                                                                                         @descripcion,
                                                                                         @responsable,
                                                                                         @areaResponsabilidad,
                                                                                         @fechaSubmayor;
                                                                                END;

                                                                                CLOSE movimientos;
                                                                                DEALLOCATE movimientos;

                                                                                SELECT Tablita.responsable,
                                                                                       Tablita.nombreResponsable,
                                                                                       Tablita.areaResponsabilidad,
                                                                                       Tablita.nombreAreaResponsabilidad,
                                                                                       Tablita.fechaSubmayor
                                                                                FROM
                                                                                (
                                                                                    SELECT responsable,
                                                                                           (
                                                                                               SELECT CONCAT(nombre, ' ', apellidos)
                                                                                               FROM dbo.Responsable
                                                                                               WHERE idResponsable = [@tablaTemp].responsable
                                                                                           ) AS [nombreResponsable],
                                                                                           areaResponsabilidad,
                                                                                           (
                                                                                               SELECT Nombre
                                                                                               FROM dbo.AreaDeResponsabilidad
                                                                                               WHERE idArea = [@tablaTemp].areaResponsabilidad
                                                                                           ) AS [nombreAreaResponsabilidad],
                                                                                           fechaSubmayor
                                                                                    FROM @tablaTemp
                                                                                    WHERE [@tablaTemp].responsable = '{responsable}'
                                                                                    GROUP BY responsable,
                                                                                             areaResponsabilidad,
                                                                                             fechaSubmayor
                                                                                ) AS Tablita
                                                                                ORDER BY Tablita.fechaSubmayor DESC;").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ActivosFijosMovimientosResponsables>();
            }
        }
        public UtilesHerramienta Clonar(UtilesHerramienta obj)
        {
            return new UtilesHerramienta
            {
                No = obj.No,
                Codigo = obj.Codigo,
                Descripcion = obj.Descripcion,
                AreaResponsabilidad = obj.AreaResponsabilidad,
                Responsable = obj.Responsable,
                CentroCosto = obj.CentroCosto,
                ValorContable = obj.ValorContable,
                ValorAdquisicion = obj.ValorAdquisicion,
                FechaListadoUth = obj.FechaListadoUth,
                IdUth= obj.IdUth
            };
        }
    }
}
