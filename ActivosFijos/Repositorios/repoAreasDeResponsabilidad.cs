using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.AuxBD;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoAreasDeResponsabilidad : iClone<AreaDeResponsabilidad>
    {
        private aftContext _context;
        private readonly repoActivosFijos _repoActivosFijos;
        private readonly repoUtilesHerramientas _repoUTH;

        public repoAreasDeResponsabilidad(aftContext context, repoActivosFijos repoActivosFijos, repoUtilesHerramientas repoUTH)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repoActivosFijos = repoActivosFijos;
            _repoUTH = repoUTH;
        }
        public async Task<List<AreaDeResponsabilidad>> ListadoAreasDeResponsabilidad()
        {
            return await _context.AreaDeResponsabilidads.OrderBy(ar => ar.Nombre).ToListAsync();
        }
        public async Task<List<AreaDeResponsabilidad>> ListadoAreasDeResponsabilidad(DateTime fecha, bool siMovimiento = false)
        {
            try
            {
                IEnumerable<AreaDeResponsabilidad> areas = new List<AreaDeResponsabilidad>();

                if (!siMovimiento)
                    areas = _context.ActFijos.Include(act => act.AreaResponsabilidadNavigation)
                        .Where(act => act.AreaResponsabilidadNavigation != null && act.FechaSubmayor.Year == fecha.Year && act.FechaSubmayor.Month == fecha.Month)
                        .Select(act => act.AreaResponsabilidadNavigation)
                        .Distinct();

                else
                {                    
                    List<ExistenciaMovimiento> existMovs = new List<ExistenciaMovimiento>();
                    existMovs = await _repoActivosFijos.DeterminarSiExisteMovimientoDeActivosFijos(fecha);

                    DateTime fechaMov = new DateTime();
                    fechaMov = fecha.AddMonths(-1);

                    List<ActFijo> actFijosList = new List<ActFijo>();
                    actFijosList = await _context.ActFijos.Include(act => act.AreaResponsabilidadNavigation)
                        .Where(act => act.AreaResponsabilidadNavigation != null && act.FechaSubmayor.Year == fechaMov.Year
                            && act.FechaSubmayor.Month == fechaMov.Month)
                        .ToListAsync();

                    actFijosList = actFijosList.Where(c => existMovs.Any(ex => ex.NoInventario.Equals(c.NoInventario))).ToList();

                    areas = actFijosList
                        .Select(c => c.AreaResponsabilidadNavigation)
                        .Distinct();
                }

                return areas.OrderBy(act => act.Nombre).ToList();
            }
            catch (Exception ex)
            {
                return new List<AreaDeResponsabilidad>();
            }
        }
        public async Task<List<AreaDeResponsabilidad>> ListadoAreasDeResponsabilidadUTH(DateTime fecha, bool siMovimiento = false)
        {
            try
            {
                IEnumerable<AreaDeResponsabilidad> areas = new List<AreaDeResponsabilidad>();

                if (!siMovimiento)
                    areas = _context.UtilesHerramientas.Include(act => act.AreaResponsabilidadNavigation)
                        .Where(act => act.AreaResponsabilidadNavigation != null && act.FechaListadoUth.Year == fecha.Year && act.FechaListadoUth.Month == fecha.Month)
                        .Select(act => act.AreaResponsabilidadNavigation)
                        .Distinct();

                else
                {
                    List<ExistenciaMovimientoUTH> existMovs = new List<ExistenciaMovimientoUTH>();
                    existMovs = await _repoUTH.DeterminarSiExisteMovimientoDeUTH(fecha);

                    DateTime fechaMov = new DateTime();
                    fechaMov = fecha.AddMonths(-1);

                    List<UtilesHerramienta> uthList = new List<UtilesHerramienta>();
                    uthList = await _context.UtilesHerramientas.Include(act => act.AreaResponsabilidadNavigation)
                        .Where(act => act.AreaResponsabilidadNavigation != null && act.FechaListadoUth.Year == fechaMov.Year
                            && act.FechaListadoUth.Month == fechaMov.Month)
                        .ToListAsync();

                    uthList = uthList.Where(c => existMovs.Any(ex => ex.Discriminante.Equals(string.Concat(c.Codigo,"|",c.Responsable,"|",c.CentroCosto,"|",c.AreaResponsabilidad)))).ToList();

                    areas = uthList
                        .Select(c => c.AreaResponsabilidadNavigation)
                        .Distinct();
                }

                return areas.OrderBy(act => act.Nombre).ToList();
            }
            catch (Exception ex)
            {
                return new List<AreaDeResponsabilidad>();
            }
        }
        public async Task<AreaDeResponsabilidad> EncontrarAreaPorID(Guid? id)
        {
            var areaDeResponsabilidad = await _context.AreaDeResponsabilidads
                .FirstOrDefaultAsync(m => m.IdArea == id);

            return areaDeResponsabilidad ?? new AreaDeResponsabilidad();
        }
        public bool SiExisteNombreDelArea(string nombre)
        {
            return _context.AreaDeResponsabilidads.Any(e => e.Nombre == nombre);
        }
        public bool SiExisteNombreDelAreaEditada(string nombre, Guid id)
        {
            bool siExiste = _context.AreaDeResponsabilidads.Any(e => e.Nombre == nombre);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.AreaDeResponsabilidads.Any(e => e.Nombre == nombre && e.IdArea == id);
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
        public bool SiExisteElAreaConElID(Guid? idArea)
        {
            return _context.AreaDeResponsabilidads.Any(e => e.IdArea == idArea);
        }
        public async Task<clResponse<AreaDeResponsabilidad>> GuardarArea(AreaDeResponsabilidad area, bool clonar = false)
        {
            try
            {
                if (area == null)
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El Área a insertar no existe." };
                else
                {
                    _context.Add(area);
                    await _context.SaveChangesAsync();

                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = true, Mensaje = "El Área ha sido guardada satisfactoriamente.", Resultado = clonar ? Clonar(area) : area };
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteElAreaConElID(area.IdArea))
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El Área a insertar no existe." };
                }
                else
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido un error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteElAreaConElID(area.IdArea))
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El Área a insertar no existe." };
                }
                else
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<AreaDeResponsabilidad>> ModificarArea(AreaDeResponsabilidad area)
        {
            try
            {
                if (area == null)
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El Área a modificar no existe." };

                _context.Update(area);
                await _context.SaveChangesAsync();
                return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = true, Mensaje = "El Área ha sido modificado satisfactoriamente.", Resultado = area };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteElAreaConElID(area.IdArea))
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El Área a modificar no existe." };
                }
                else
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteElAreaConElID(area.IdArea))
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El Área a modificar no existe." };
                }
                else
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<AreaDeResponsabilidad>> EliminarArea(AreaDeResponsabilidad area)
        {
            if (area == null)
                return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El Área a eliminar es incorrecto." };
            try
            {
                _context.AreaDeResponsabilidads.Remove(area);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = true, Mensaje = "El Área ha sido eliminado satisfactoriamente." };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteElAreaConElID(area.IdArea))
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El Área a eliminar no existe." };
                }
                else
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message + " " + ex.InnerException.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteElAreaConElID(area.IdArea))
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El Área a eliminar no existe." };
                }
                else
                {
                    return new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex1.Message + " " + ex1.InnerException.Message };
                }
            }
        }
        public async Task<List<AreaDeResponsabilidad>> ObtenerListadoAreasSiMovimiento(List<ActivosFijosConMovimientos> existenciaDeMovimientos)
        {
            try
            {
                List<AreaDeResponsabilidad> guidsAreas = new List<AreaDeResponsabilidad>();
                foreach (var item in existenciaDeMovimientos)
                {
                    AreaDeResponsabilidad area = new AreaDeResponsabilidad();
                    area = _context.AreaDeResponsabilidads.Where(re => re.IdArea.Equals(item.idAreaAnterior)).FirstOrDefault();
                    if (area != null && !guidsAreas.Any(c => c.IdArea.Equals(item.idAreaAnterior)))
                        guidsAreas.Add(area);

                    area = _context.AreaDeResponsabilidads.Where(re => re.IdArea.Equals(item.idAreaActual)).FirstOrDefault();
                    if (area != null && !guidsAreas.Any(c => c.IdArea.Equals(item.idAreaActual)))
                        guidsAreas.Add(area);
                }

                return guidsAreas;
            }
            catch (Exception ex)
            {
                return new List<AreaDeResponsabilidad>();
            }
        }
        public async Task<List<AreaDeResponsabilidad>> ObtenerListadoAreasSiMovimiento(List<UTHConMovimientosParcial> existenciaDeMovimientos)
        {
            try
            {
                List<AreaDeResponsabilidad> guidsAreas = new List<AreaDeResponsabilidad>();
                foreach (var item in existenciaDeMovimientos)
                {
                    AreaDeResponsabilidad area = new AreaDeResponsabilidad();
                    area = _context.AreaDeResponsabilidads.Where(re => re.IdArea.Equals(item.Area)).FirstOrDefault();
                    if (area != null && !guidsAreas.Any(c => c.IdArea.Equals(item.Area)))
                        guidsAreas.Add(area);
                }

                return guidsAreas;
            }
            catch (Exception ex)
            {
                return new List<AreaDeResponsabilidad>();
            }
        }
        public async Task<AreaDeResponsabilidad> EncontrarAreaPorIdResponsable(Guid? id)
        {
            var areaDeResponsabilidad = await _context.Responsables.Include(m => m.AreaResponsabilidadNavigation).Where(m => m.IdResponsable.Equals(id))
                .Select(m => m.AreaResponsabilidadNavigation).FirstOrDefaultAsync();

            return areaDeResponsabilidad ?? new AreaDeResponsabilidad();
        }
        public AreaDeResponsabilidad Clonar(AreaDeResponsabilidad obj)
        {
            return new AreaDeResponsabilidad
            {
                IdArea = obj.IdArea,
                Nombre = obj.Nombre,
                ActFijos = obj.ActFijos,
                Descripcion = obj.Descripcion,
                Responsables = obj.Responsables
            };
        }
    }
}
