using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.AuxBD;
using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoActivosFijos : iClone<ActFijo>
    {
        private aftContext _context;
        private aftContextAux _contextAux;

        public repoActivosFijos(aftContext context, aftContextAux contextAux)
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
        public async Task<List<ActFijo>> ListadoActivosFijos(DateTime fecha)
        {
            return await _context.ActFijos
                .Include(a => a.AreaResponsabilidadNavigation)
                .Include(a => a.CentroCostoNavigation)
                .Include(a => a.ResponsableNavigation)
                .Where(act => act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month)
                .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                    .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                    .ThenBy(act => act.NoInventario)
                .ToListAsync();
        }
        public async Task<List<ActFijo>> ListadoActivosFijos(DateTime fecha, Guid area, Guid responsable)
        {
            if (area == Guid.Empty && responsable == Guid.Empty)
            {
                return await _context.ActFijos
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month)
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.NoInventario)
                    .ToListAsync();
            } else if (area == Guid.Empty && responsable != Guid.Empty)
            {
                return await _context.ActFijos
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month && act.Responsable.Equals(responsable))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.NoInventario)
                    .ToListAsync();
            }
            else if (area != Guid.Empty && responsable == Guid.Empty)
            {
                return await _context.ActFijos
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month && act.AreaResponsabilidad.Equals(area))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.NoInventario)
                    .ToListAsync();
            } else
            {
                return await _context.ActFijos
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month && act.Responsable.Equals(responsable) && act.AreaResponsabilidad.Equals(area))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.NoInventario)
                    .ToListAsync();
            }
        }
        public List<ActFijo> ActivosFijos(DateTime fecha, Guid area, Guid responsable)
        {
            if (area == Guid.Empty && responsable == Guid.Empty)
            {
                return _context.ActFijos
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month)
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.NoInventario)
                    .ToList();
            }
            else if (area == Guid.Empty && responsable != Guid.Empty)
            {
                return _context.ActFijos
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month && act.Responsable.Equals(responsable))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.NoInventario)
                    .ToList();
            }
            else if (area != Guid.Empty && responsable == Guid.Empty)
            {
                return _context.ActFijos
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month && act.AreaResponsabilidad.Equals(area))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.NoInventario)
                    .ToList();
            }
            else
            {
                return _context.ActFijos
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Where(act => act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month 
                        && act.Responsable.Equals(responsable) && act.AreaResponsabilidad.Equals(area))
                    .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                        .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                        .ThenBy(act => act.NoInventario)
                    .ToList();
            }
        }
        public async Task<ActFijo> EncontrarActivoFijoID(long? id)
        {
            var actFijo = await _context.ActFijos
                .Include(a => a.AreaResponsabilidadNavigation)
                .Include(a => a.CentroCostoNavigation)
                .Include(a => a.ResponsableNavigation)
                .FirstOrDefaultAsync(m => m.NoInventario == id);

            return actFijo ?? new ActFijo();
        }
        public async Task<ActFijo> EncontrarActivoFijoID(long? id, DateTime fecha)
        {
            var actFijo = await _context.ActFijos
                .Include(a => a.AreaResponsabilidadNavigation)
                .Include(a => a.CentroCostoNavigation)
                .Include(a => a.ResponsableNavigation)
                .SingleOrDefaultAsync(a => a.FechaSubmayor.Year.Equals(fecha.Year) && a.FechaSubmayor.Month.Equals(fecha.Month) && a.NoInventario.Equals(id));

            return actFijo ?? new ActFijo();
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
        public async Task<clResponse<ActFijo>> GuardarActivoFijo(ActFijo actFijo, bool clonar = false)
        {
            try
            {
                if (actFijo == null)
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a insertar no existe." };
                else
                {
                    _context.Add(actFijo);
                    await _context.SaveChangesAsync();

                    return new clResponse<ActFijo>() { SiSatisfactorio = true, Mensaje = $"El Activo Fijo con el No. Inventario {actFijo.NoInventario} ha sido insertado satisfactoriamente.", Resultado = clonar ? Clonar(actFijo) : actFijo };
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteActivoFijoConElID(actFijo.NoInventario))
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a insertar no existe." };
                }
                else
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido un error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteActivoFijoConElID(actFijo.NoInventario))
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El Activo Fijo a insertar no existe." };
                }
                else
                {
                    return new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
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
        public async Task<List<ExistenciaMovimiento>> DeterminarSiExisteMovimientoDeActivosFijos(DateTime fecha) {
            try
            {
                string fechaMov = fecha.ToString("yyyy-MM-dd");
                List<ExistenciaMovimiento> query = new List<ExistenciaMovimiento>();
                query = await _contextAux.ExistenciaMovimientos.FromSqlRaw($@"DECLARE @fecha DATETIME = CONVERT(DATETIME, '{fechaMov}')

                                                                            SELECT Tabla.noInventario AS [NoInventario], COUNT(*) AS [Cantidad]
                                                                            FROM
                                                                            (
                                                                                SELECT noInventario,
                                                                                       descripcion,
                                                                                       responsable,
                                                                                       areaResponsabilidad
                                                                                FROM dbo.ActFijo
                                                                                WHERE YEAR(fechaSubmayor) = YEAR(@fecha)
                                                                                      AND MONTH(fechaSubmayor) = MONTH(DATEADD(MONTH, -1, @fecha))
                                                                                UNION
                                                                                SELECT noInventario,
                                                                                       descripcion,
                                                                                       responsable,
                                                                                       areaResponsabilidad
                                                                                FROM dbo.ActFijo
                                                                                WHERE YEAR(fechaSubmayor) = YEAR(@fecha)
                                                                                      AND MONTH(fechaSubmayor) = MONTH(@fecha)
                                                                            ) AS Tabla
                                                                            GROUP BY noInventario
                                                                            HAVING COUNT(*) > 1;").ToListAsync();
                
                return query;
            }
            catch (Exception ex)
            {
                return new List<ExistenciaMovimiento>();
            }
        }
        public async Task<List<ActivosFijosConMovimientos>> ListadoActivosFijosConMovimientos(DateTime fecha)
        {
            try
            {
                string fechaMov = fecha.ToString("yyyy-MM-dd");
                List<ActivosFijosConMovimientos> query = new List<ActivosFijosConMovimientos>();
                query = await _contextAux.ActivosFijosConMovimientos.FromSqlRaw($@"DECLARE @fechaAnterior DATETIME,
                                                                                            @fechaActual DATETIME = CONVERT(DATETIME, '{fechaMov}');

                                                                                    DECLARE @tablaDeMovimientos TABLE
                                                                                    (
                                                                                        NoInventario BIGINT,
                                                                                        SubNumero INT,
                                                                                        Cantidad INT,
                                                                                        Descripcion VARCHAR(2000),
                                                                                        AreaAnterior UNIQUEIDENTIFIER,
                                                                                        AreaActual UNIQUEIDENTIFIER,
                                                                                        ResponsableAnterior UNIQUEIDENTIFIER,
                                                                                        ResponsableActual UNIQUEIDENTIFIER
                                                                                    );

                                                                                    DECLARE @fecha DATETIME = CONVERT(DATETIME, @fechaActual);

                                                                                    SET @fechaAnterior = DATEADD(MONTH, -1, @fecha);

                                                                                    INSERT @tablaDeMovimientos
                                                                                    SELECT DISTINCT
                                                                                           MOV.NoInventario,
                                                                                           MOV.SubNumero,
                                                                                           0,
                                                                                           MOV.Descripcion,
                                                                                           NULL,
                                                                                           NULL,
                                                                                           NULL,
                                                                                           NULL
                                                                                    FROM
                                                                                    (
                                                                                        SELECT AFT.NoInventario,
                                                                                               AFT.SubNumero,
                                                                                               AFT.Descripcion,
                                                                                               AFT.responsable,
                                                                                               AFT.areaResponsabilidad,
                                                                                               COUNT(*) AS [Cantidad]
                                                                                        FROM
                                                                                        (
                                                                                            SELECT noInventario,
                                                                                                   subNumero,
                                                                                                   descripcion,
                                                                                                   responsable,
                                                                                                   areaResponsabilidad
                                                                                            FROM dbo.ActFijo
                                                                                            WHERE YEAR(fechaSubmayor) = YEAR(@fechaAnterior)
                                                                                                  AND MONTH(fechaSubmayor) = MONTH(@fechaAnterior)
                                                                                            UNION ALL
                                                                                            SELECT noInventario,
                                                                                                   subNumero,
                                                                                                   descripcion,
                                                                                                   responsable,
                                                                                                   areaResponsabilidad
                                                                                            FROM dbo.ActFijo
                                                                                            WHERE YEAR(fechaSubmayor) = YEAR(@fecha)
                                                                                                  AND MONTH(fechaSubmayor) = MONTH(@fecha)
                                                                                        ) AS AFT
                                                                                        GROUP BY AFT.NoInventario,
                                                                                                 AFT.SubNumero,
                                                                                                 AFT.Descripcion,
                                                                                                 AFT.responsable,
                                                                                                 AFT.areaResponsabilidad
                                                                                        HAVING COUNT(*) = 1
                                                                                    ) MOV;

                                                                                    UPDATE @tablaDeMovimientos
                                                                                    SET AreaAnterior =
                                                                                        (
                                                                                            SELECT areaResponsabilidad
                                                                                            FROM dbo.ActFijo
                                                                                             WHERE noInventario = [@tablaDeMovimientos].NoInventario
                                                                                                   AND subNumero = [@tablaDeMovimientos].SubNumero
                                                                                                   AND YEAR(fechaSubmayor) = YEAR(@fechaAnterior)
                                                                                                   AND MONTH(fechaSubmayor) = MONTH(@fechaAnterior)
                                                                                         ),
                                                                                         AreaActual =
                                                                                         (
                                                                                             SELECT areaResponsabilidad
                                                                                             FROM dbo.ActFijo
                                                                                             WHERE noInventario = [@tablaDeMovimientos].NoInventario
                                                                                                   AND subNumero = [@tablaDeMovimientos].SubNumero
                                                                                                   AND YEAR(fechaSubmayor) = YEAR(@fecha)
                                                                                                   AND MONTH(fechaSubmayor) = MONTH(@fecha)
                                                                                         ),
                                                                                         ResponsableAnterior =
                                                                                         (
                                                                                             SELECT responsable
                                                                                             FROM dbo.ActFijo
                                                                                             WHERE noInventario = [@tablaDeMovimientos].NoInventario
                                                                                                   AND subNumero = [@tablaDeMovimientos].SubNumero
                                                                                                   AND YEAR(fechaSubmayor) = YEAR(@fechaAnterior)
                                                                                                   AND MONTH(fechaSubmayor) = MONTH(@fechaAnterior)
                                                                                         ),
                                                                                         ResponsableActual =
                                                                                         (
                                                                                             SELECT responsable
                                                                                             FROM dbo.ActFijo
                                                                                             WHERE noInventario = [@tablaDeMovimientos].NoInventario
                                                                                                   AND subNumero = [@tablaDeMovimientos].SubNumero
                                                                                                   AND YEAR(fechaSubmayor) = YEAR(@fecha)
                                                                                        );

                                                                                    SELECT NoInventario,
                                                                                           SubNumero,
                                                                                           Cantidad,
                                                                                           Descripcion,
                                                                                           AreaAnterior AS [idAreaAnterior],
                                                                                           (
                                                                                               SELECT Nombre FROM dbo.AreaDeResponsabilidad WHERE idArea = AreaAnterior
                                                                                           ) AS [AreaAnterior],
                                                                                           AreaActual AS [idAreaActual],
                                                                                           (
                                                                                               SELECT Nombre FROM dbo.AreaDeResponsabilidad WHERE idArea = AreaActual
                                                                                           ) AS [AreaActual],
                                                                                           ResponsableAnterior AS [idResponsableAnterior],
                                                                                           (
                                                                                               SELECT CASE nombre
                                                                                                          WHEN '<<S/R>>' THEN
                                                                                                              nombre
                                                                                                          ELSE
                                                                                                              nombre + ' ' + apellidos
                                                                                                      END
                                                                                               FROM dbo.Responsable
                                                                                               WHERE idResponsable = ResponsableAnterior
                                                                                           ) AS [ResponsableAnterior],
                                                                                           ResponsableActual AS [idResponsableActual],
                                                                                           (
                                                                                               SELECT CASE nombre
                                                                                                          WHEN '<<S/R>>' THEN
                                                                                                              nombre
                                                                                                          ELSE
                                                                                                              nombre + ' ' + apellidos
                                                                                                      END
                                                                                               FROM dbo.Responsable
                                                                                               WHERE idResponsable = ResponsableActual
                                                                                           ) AS [ResponsableActual]
                                                                                    FROM @tablaDeMovimientos
                                                                                    ORDER BY NoInventario ASC;").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ActivosFijosConMovimientos>();
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
        public async Task<List<ActivosFijosSinClasificar>> ListadoActivosFijosSinClasificar()
        {
            try
            {
                List<ActivosFijosSinClasificar> query = new List<ActivosFijosSinClasificar>();
                query = await _contextAux.ActivosFijosSinClasificar.FromSqlRaw($@"DECLARE @v_fecha DATETIME =
                                                                                            (
                                                                                                SELECT MAX(fechaSubmayor) FROM dbo.ActFijo
                                                                                            );

                                                                                    SELECT noInventario,
                                                                                            AFT.descripcion,
                                                                                            centroCosto,
                                                                                            CC.nombre,
	                                                                                        CC.numero
                                                                                    FROM dbo.ActFijo AFT
                                                                                        LEFT JOIN dbo.CentroCosto CC
                                                                                            ON CC.idCentroCosto = AFT.centroCosto
                                                                                        LEFT JOIN dbo.Clasificador CLASIF
                                                                                                ON CLASIF.idClasificador = AFT.clasificador
                                                                                    WHERE AFT.fechaSubmayor = @v_fecha AND (LOWER(CLASIF.Nombre) = LOWER('Sin Clasificar') OR AFT.clasificador IS NULL);").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ActivosFijosSinClasificar>();
            }
        }
        public async Task<List<ActivosFijosClasificados>> ListadoActivosFijosClasificados()
        {
            try
            {
                List<ActivosFijosClasificados> query = new List<ActivosFijosClasificados>();
                query = await _contextAux.ActivosFijosClasificados.FromSqlRaw($@"DECLARE @v_fecha DATETIME =
                                                                                            (
                                                                                                SELECT MAX(fechaSubmayor) FROM dbo.ActFijo
                                                                                            );

                                                                                    SELECT noInventario,
                                                                                           AFT.descripcion,
                                                                                           centroCosto,
                                                                                           CC.nombre,
                                                                                           CC.numero,
                                                                                           CLASIF.Nombre AS [clasificador]
                                                                                    FROM dbo.ActFijo AFT
                                                                                        INNER JOIN dbo.CentroCosto CC
                                                                                            ON CC.idCentroCosto = AFT.centroCosto
                                                                                        INNER JOIN dbo.Clasificador CLASIF
                                                                                            ON CLASIF.idClasificador = AFT.clasificador
                                                                                    WHERE AFT.fechaSubmayor = @v_fecha
                                                                                          AND LOWER(CLASIF.Nombre) <> LOWER('Sin Clasificar');").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ActivosFijosClasificados>();
            }
        }
        public async Task<List<ActivosFijosClasificados>> ListadoActivosFijosClasificados(Guid centroCosto)
        {
            try
            {
                List<ActivosFijosClasificados> query = new List<ActivosFijosClasificados>();
                query = await _contextAux.ActivosFijosClasificados.FromSqlRaw($@"DECLARE @v_fecha DATETIME =
                                                                                            (
                                                                                                SELECT MAX(fechaSubmayor) FROM dbo.ActFijo
                                                                                            ), @v_centroCosto UNIQUEIDENTIFIER;
                                                                                    SET @v_centroCosto = '{centroCosto}';

                                                                                    SELECT noInventario,
                                                                                           AFT.descripcion,
                                                                                           centroCosto,
                                                                                           CC.nombre,
                                                                                           CC.numero,
                                                                                           CLASIF.Nombre AS [clasificador]
                                                                                    FROM dbo.ActFijo AFT
                                                                                        INNER JOIN dbo.CentroCosto CC
                                                                                            ON CC.idCentroCosto = AFT.centroCosto
                                                                                        INNER JOIN dbo.Clasificador CLASIF
                                                                                            ON CLASIF.idClasificador = AFT.clasificador
                                                                                    WHERE AFT.fechaSubmayor = @v_fecha AND AFT.centroCosto = @v_centroCosto
                                                                                          AND LOWER(CLASIF.Nombre) <> LOWER('Sin Clasificar');").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ActivosFijosClasificados>();
            }
        }
        public async Task<List<ActivosFijosClasificados>> ListadoActivosFijosClasificados(Guid clasificador, bool siClasificador)
        {
            try
            {
                List<ActivosFijosClasificados> query = new List<ActivosFijosClasificados>();
                query = await _contextAux.ActivosFijosClasificados.FromSqlRaw($@"DECLARE @v_fecha DATETIME =
                                                                                            (
                                                                                                SELECT MAX(fechaSubmayor) FROM dbo.ActFijo
                                                                                            ), @v_clasificador UNIQUEIDENTIFIER;
                                                                                    SET @v_clasificador = '{clasificador}';

                                                                                    SELECT noInventario,
                                                                                           AFT.descripcion,
                                                                                           centroCosto,
                                                                                           CC.nombre,
                                                                                           CC.numero,
                                                                                           CLASIF.Nombre AS [clasificador]
                                                                                    FROM dbo.ActFijo AFT
                                                                                        INNER JOIN dbo.CentroCosto CC
                                                                                            ON CC.idCentroCosto = AFT.centroCosto
                                                                                        INNER JOIN dbo.Clasificador CLASIF
                                                                                            ON CLASIF.idClasificador = AFT.clasificador
                                                                                    WHERE AFT.fechaSubmayor = @v_fecha AND AFT.clasificador = @v_clasificador
                                                                                          AND LOWER(CLASIF.Nombre) <> LOWER('Sin Clasificar');").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ActivosFijosClasificados>();
            }
        }
        public async Task<List<ActivosFijosClasificados>> ListadoActivosFijosClasificados(Guid centroCosto, Guid clasificador)
        {
            try
            {
                List<ActivosFijosClasificados> query = new List<ActivosFijosClasificados>();
                query = await _contextAux.ActivosFijosClasificados.FromSqlRaw($@"DECLARE @v_fecha DATETIME =
                                                                                            (
                                                                                                SELECT MAX(fechaSubmayor) FROM dbo.ActFijo
                                                                                            ), @v_centroCosto UNIQUEIDENTIFIER, @v_clasificador UNIQUEIDENTIFIER;
                                                                                    SET @v_centroCosto = '{centroCosto}';
                                                                                    SET @v_clasificador = '{clasificador}';

                                                                                    SELECT noInventario,
                                                                                           AFT.descripcion,
                                                                                           centroCosto,
                                                                                           CC.nombre,
                                                                                           CC.numero,
                                                                                           CLASIF.Nombre AS [clasificador]
                                                                                    FROM dbo.ActFijo AFT
                                                                                        INNER JOIN dbo.CentroCosto CC
                                                                                            ON CC.idCentroCosto = AFT.centroCosto
                                                                                        INNER JOIN dbo.Clasificador CLASIF
                                                                                            ON CLASIF.idClasificador = AFT.clasificador
                                                                                    WHERE AFT.fechaSubmayor = @v_fecha AND AFT.centroCosto = @v_centroCosto AND AFT.clasificador = @v_clasificador
                                                                                          AND LOWER(CLASIF.Nombre) <> LOWER('Sin Clasificar');").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ActivosFijosClasificados>();
            }
        }
        public async Task<DateTime> UltimoSubmayor()
        {
            return await _context.ActFijos.MaxAsync(x => x.FechaSubmayor);
        }
        public ActFijo Clonar(ActFijo obj)
        {
            return new ActFijo
            {
                NoInventario = obj.NoInventario,
                Descripcion = obj.Descripcion,
                AreaResponsabilidad = obj.AreaResponsabilidad,
                Responsable = obj.Responsable,
                CentroCosto = obj.CentroCosto,
                ValorContable = obj.ValorContable,
                DepreciacionAcum = obj.DepreciacionAcum,
                ValorAdquisicion = obj.ValorAdquisicion,
                ValorResidual = obj.ValorResidual,
                FechaSubmayor = obj.FechaSubmayor
            };
        }
    }
}
