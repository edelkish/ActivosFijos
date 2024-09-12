using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.AuxBD;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Models.Vistas;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoResponsables : iClone<Responsable>
    {
        private aftContext _context;
        private readonly repoActivosFijos _repoActivosFijos;
        private readonly repoUtilesHerramientas _repoUTH;
        public repoResponsables(aftContext context, repoActivosFijos repoActivosFijos, repoUtilesHerramientas repoUTH)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repoActivosFijos = repoActivosFijos;
            _repoUTH = repoUTH;
        }
        public async Task<List<Responsable>> ObtenerListadoResponsables()
        {
            return await _context.Responsables.Include(c => c.CargoNavigation).Include(c => c.AreaResponsabilidadNavigation).OrderBy(c => c.Nombre).ToListAsync();
        }
        public async Task<List<Responsable>> ObtenerListadoResponsables(DateTime fecha, bool siMovimiento = false)
        {
            try
            {
                List<Responsable> responsables = new List<Responsable>();

                if (!siMovimiento)
                    responsables = await _context.Responsables
                        .Include(c => c.CargoNavigation)
                        .Include(c => c.AreaResponsabilidadNavigation)
                        .Where(c => c.ActFijos.Any(ac => ac.FechaSubmayor.Year.Equals(fecha.Year) && ac.FechaSubmayor.Month.Equals(fecha.Month))).OrderBy(c => c.Nombre)
                        .ToListAsync();
                else
                {
                    List<ExistenciaMovimiento> existMovs = new List<ExistenciaMovimiento>();
                    existMovs = await _repoActivosFijos.DeterminarSiExisteMovimientoDeActivosFijos(fecha);

                    List<ActFijo> actFijosList = new List<ActFijo>();
                    actFijosList = await _context.ActFijos.Include(act => act.ResponsableNavigation)
                        .Where(act => act.ResponsableNavigation != null && act.FechaSubmayor.Year == fecha.Year
                            && act.FechaSubmayor.Month == fecha.Month)
                        .ToListAsync();

                    actFijosList = actFijosList.Where(c => existMovs.Any(ex => ex.NoInventario.Equals(c.NoInventario))).ToList();

                    responsables = actFijosList.Select(c => c.ResponsableNavigation).Distinct().ToList();
                }

                return responsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }
        }

        public async Task<List<Responsable>> ObtenerListadoResponsablesUTH(DateTime fecha, bool siMovimiento = false)
        {
            try
            {
                List<Responsable> responsables = new List<Responsable>();

                if (!siMovimiento)
                    responsables = await _context.Responsables
                        .Include(c => c.CargoNavigation)
                        .Include(c => c.AreaResponsabilidadNavigation)
                        .Where(c => c.UtilesHerramienta.Any(ac => ac.FechaListadoUth.Year.Equals(fecha.Year) && ac.FechaListadoUth.Month.Equals(fecha.Month))).OrderBy(c => c.Nombre)
                        .ToListAsync();
                else
                {
                    List<ExistenciaMovimientoUTH> existMovs = new List<ExistenciaMovimientoUTH>();
                    existMovs = await _repoUTH.DeterminarSiExisteMovimientoDeUTH(fecha);

                    List<UtilesHerramienta> uthList = new List<UtilesHerramienta>();
                    uthList = await _context.UtilesHerramientas.Include(act => act.ResponsableNavigation)
                        .Where(act => act.ResponsableNavigation != null && act.FechaListadoUth.Year == fecha.Year
                            && act.FechaListadoUth.Month == fecha.Month)
                        .ToListAsync();

                    uthList = uthList.Where(c => existMovs.Any(ex => ex.Discriminante.Equals(string.Concat(c.Codigo, "|", c.Responsable, "|", c.CentroCosto, "|", c.AreaResponsabilidad)))).ToList();

                    responsables = uthList.Select(c => c.ResponsableNavigation).Distinct().ToList();
                }

                return responsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }
        }

        public async Task<List<Responsable>> ObtenerListadoResponsablesPorFechaYArea(DateTime fecha, Guid area, bool siMovimiento = false)
        {
            try
            {
                List<Responsable> responsables = new List<Responsable>();

                if (!siMovimiento)
                    responsables = await _context.Responsables
                        .Include(c => c.CargoNavigation)
                        .Include(c => c.AreaResponsabilidadNavigation)
                        .Where(c => c.ActFijos.Any(ac => ac.FechaSubmayor.Year.Equals(fecha.Year) && ac.FechaSubmayor.Month.Equals(fecha.Month) && ac.AreaResponsabilidad.Equals(area))).OrderBy(c => c.Nombre)
                        .ToListAsync();
                else
                {
                    List<ExistenciaMovimiento> existMovs = new List<ExistenciaMovimiento>();
                    existMovs = await _repoActivosFijos.DeterminarSiExisteMovimientoDeActivosFijos(fecha);

                    DateTime fechaMov = new DateTime();
                    fechaMov = fecha.AddMonths(-1);

                    List<ActFijo> actFijosList = new List<ActFijo>();

                    if (area.Equals(Guid.Empty))
                    {
                        actFijosList = await _context.ActFijos.Include(act => act.ResponsableNavigation)
                            .Where(act => act.ResponsableNavigation != null && act.FechaSubmayor.Year == fechaMov.Year
                                && act.FechaSubmayor.Month == fechaMov.Month)
                            .ToListAsync();
                    }
                    else
                    {
                        actFijosList = await _context.ActFijos.Include(act => act.ResponsableNavigation)
                            .Where(act => act.ResponsableNavigation != null && act.FechaSubmayor.Year == fechaMov.Year
                                && act.FechaSubmayor.Month == fechaMov.Month && act.AreaResponsabilidad.Equals(area))
                            .ToListAsync();

                    }
                    actFijosList = actFijosList.Where(c => existMovs.Any(ex => ex.NoInventario.Equals(c.NoInventario))).ToList();

                    responsables = actFijosList.Select(c => c.ResponsableNavigation).Distinct().ToList();
                }

                return responsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }
        }
        public async Task<List<Responsable>> ObtenerListadoResponsablesPorFechaYAreaUTH(DateTime fecha, Guid area, bool siMovimiento = false)
        {
            try
            {
                List<Responsable> responsables = new List<Responsable>();

                if (!siMovimiento)
                    responsables = await _context.Responsables
                        .Include(c => c.CargoNavigation)
                        .Include(c => c.AreaResponsabilidadNavigation)
                        .Where(c => c.UtilesHerramienta.Any(ac => ac.FechaListadoUth.Year.Equals(fecha.Year) && ac.FechaListadoUth.Month.Equals(fecha.Month) && ac.AreaResponsabilidad.Equals(area))).OrderBy(c => c.Nombre)
                        .ToListAsync();
                else
                {
                    List<ExistenciaMovimientoUTH> existMovs = new List<ExistenciaMovimientoUTH>();
                    existMovs = await _repoUTH.DeterminarSiExisteMovimientoDeUTH(fecha);

                    DateTime fechaMov = new DateTime();
                    fechaMov = fecha.AddMonths(-1);

                    List<UtilesHerramienta> uthList = new List<UtilesHerramienta>();

                    if (area.Equals(Guid.Empty))
                    {
                        uthList = await _context.UtilesHerramientas.Include(act => act.ResponsableNavigation)
                            .Where(act => act.ResponsableNavigation != null && act.FechaListadoUth.Year == fechaMov.Year
                                && act.FechaListadoUth.Month == fechaMov.Month)
                            .ToListAsync();
                    }
                    else
                    {
                        uthList = await _context.UtilesHerramientas.Include(act => act.ResponsableNavigation)
                            .Where(act => act.ResponsableNavigation != null && act.FechaListadoUth.Year == fechaMov.Year
                                && act.FechaListadoUth.Month == fechaMov.Month && act.AreaResponsabilidad.Equals(area))
                            .ToListAsync();

                    }
                    uthList = uthList.Where(c => existMovs.Any(ex => ex.Discriminante.Equals(string.Concat(c.Codigo, "|", c.Responsable, "|", c.CentroCosto, "|", c.AreaResponsabilidad)))).ToList();

                    responsables = uthList.Select(c => c.ResponsableNavigation).Distinct().ToList();
                }

                return responsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }
        }
        public bool ResponsablesExists(Guid id)
        {
            return _context.Responsables.Any(e => e.IdResponsable == id);
        }
        public async Task<clResponse<Responsable>> InsertarResponsables(Responsable responsable, bool clonar = false)
        {
            if (responsable == null)
                return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El Responsable a insertar es incorrecto." };
            try
            {
                _context.Add(responsable);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                return new clResponse<Responsable>() { SiSatisfactorio = true, Mensaje = "El Responsable ha sido guardado satisfactoriamente.", Resultado = clonar ? Clonar(responsable) : responsable };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ResponsablesExists(responsable.IdResponsable))
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El Responsable a insertar no existe." };
                }
                else
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "Ha ocurrido un error, no se pudo insertar el Responsable. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!ResponsablesExists(responsable.IdResponsable))
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El Responsable a insertar no existe." };
                }
                else
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<Responsable>> ModificarResponsable(Responsable responsable, bool clonar = false)
        {
            if (responsable == null)
                return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El Responsable a modificar es incorrecto." };
            try
            {
                _context.Update(responsable);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                return new clResponse<Responsable>() { SiSatisfactorio = true, Mensaje = "El Responsable ha sido modificado satisfactoriamente.", Resultado = clonar ? Clonar(responsable) : responsable };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ResponsablesExists(responsable.IdResponsable))
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El Responsable a modificar no existe." };
                }
                else
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!ResponsablesExists(responsable.IdResponsable))
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El Responsable a modificar no existe." };
                }
                else
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<Responsable>> EliminarResponsable(Responsable responsable)
        {
            if (responsable == null)
                return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El Responsable a eliminar es incorrecto." };
            try
            {
                _context.Responsables.Remove(responsable);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Responsable>() { SiSatisfactorio = true, Mensaje = "El Responsable ha sido eliminado satisfactoriamente." };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ResponsablesExists(responsable.IdResponsable))
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El Responsable a eliminar no existe." };
                }
                else
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message + " " + ex.InnerException.Message };
                }
            }
            catch (Exception ex)
            {
                if (!ResponsablesExists(responsable.IdResponsable))
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El Responsable a eliminar no existe." };
                }
                else
                {
                    return new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex1.Message + " " + ex1.InnerException.Message };
                }
            }
        }
        public async Task<Responsable> SeleccionarResponsablesPorID(Guid? id)
        {
            var responsable = new Responsable();

            responsable = await _context.Responsables
                .Include(c => c.CargoNavigation)
                .Include(c => c.AreaResponsabilidadNavigation)
                .Where(c => c.IdResponsable.Equals(id))
                .FirstOrDefaultAsync();

            return responsable;
        }
        public bool SiExisteNombreApellidosDelResponsable(string nombre, string apellidos)
        {
            return _context.Responsables.Any(e => e.Nombre == nombre && e.Apellidos.Equals(apellidos));
        }
        public bool SiExisteNombreApellidosDelResponsableAModificar(string nombre, string apellidos, Guid id)
        {
            bool siExiste = _context.Responsables.Any(r => r.Nombre == nombre && r.Apellidos.Equals(apellidos));
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.Responsables.Any(r => r.Nombre == nombre && r.Apellidos.Equals(apellidos) && r.IdResponsable == id);
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
        public bool SiExisteElReponsableConElID(Guid? id)
        {
            return _context.Responsables.Any(e => e.IdResponsable == id);
        }
        public async Task<List<Responsable>> ObtenerListadoResponsables(Guid area)
        {
            if (area.Equals(Guid.Empty))
            {
                return await _context.Responsables.OrderBy(c => c.Nombre).ToListAsync();
            }
            else
            {
                return await _context.Responsables.Where(res => res.AreaResponsabilidad == area).OrderBy(c => c.Nombre).ToListAsync();
            }
        }
        public async Task<List<Responsable>> ObtenerListadoResponsablesSiMovimiento(List<ActivosFijosConMovimientos> existenciaDeMovimientos)
        {
            try
            {
                List<Responsable> guidsResponsables = new List<Responsable>();
                foreach (var item in existenciaDeMovimientos)
                {
                    Responsable responsable = new Responsable();
                    responsable = _context.Responsables.Where(re => re.IdResponsable.Equals(item.idResponsableAnterior)).FirstOrDefault();
                    if (responsable != null && !guidsResponsables.Any(c => c.IdResponsable.Equals(item.idResponsableAnterior)))
                        guidsResponsables.Add(responsable);

                    responsable = _context.Responsables.Where(re => re.IdResponsable.Equals(item.idResponsableActual)).FirstOrDefault();
                    if (responsable != null && !guidsResponsables.Any(c => c.IdResponsable.Equals(item.idResponsableActual)))
                        guidsResponsables.Add(responsable);
                }

                return guidsResponsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }

        }
        public async Task<List<Responsable>> ObtenerListadoResponsablesSiMovimiento(List<UTHConMovimientosParcial> existenciaDeMovimientos)
        {
            try
            {
                List<Responsable> guidsResponsables = new List<Responsable>();
                foreach (var item in existenciaDeMovimientos)
                {
                    Responsable responsable = new Responsable();
                    responsable = _context.Responsables.Where(re => re.IdResponsable.Equals(item.Responsable)).FirstOrDefault();
                    if (responsable != null && !guidsResponsables.Any(c => c.IdResponsable.Equals(item.Responsable)))
                        guidsResponsables.Add(responsable);
                }

                return guidsResponsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }

        }
        public async Task<List<Responsable>> ObtenerListadoResponsablesSiMovimiento(List<ActivosFijosConMovimientos> existenciaDeMovimientos, Guid area)
        {
            try
            {
                List<Responsable> guidsResponsables = new List<Responsable>();
                foreach (var item in existenciaDeMovimientos)
                {
                    Responsable responsable = new Responsable();
                    responsable = area == Guid.Empty ? _context.Responsables.Where(re => re.IdResponsable.Equals(item.idResponsableAnterior)).FirstOrDefault() : _context.Responsables.Where(re => re.IdResponsable.Equals(item.idResponsableAnterior) && item.idAreaAnterior.Equals(area)).FirstOrDefault();
                    if (responsable != null && !guidsResponsables.Any(c => c.IdResponsable.Equals(item.idResponsableAnterior)))
                        guidsResponsables.Add(responsable);

                    responsable = area == Guid.Empty ? _context.Responsables.Where(re => re.IdResponsable.Equals(item.idResponsableActual)).FirstOrDefault() : _context.Responsables.Where(re => re.IdResponsable.Equals(item.idResponsableActual) && item.idAreaActual.Equals(area)).FirstOrDefault();
                    if (responsable != null && !guidsResponsables.Any(c => c.IdResponsable.Equals(item.idResponsableActual)))
                        guidsResponsables.Add(responsable);
                }

                return guidsResponsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }

        }
        public async Task<List<Responsable>> ObtenerListadoResponsablesSiMovimiento(List<UTHConMovimientosParcial> existenciaDeMovimientos, Guid area)
        {
            try
            {
                List<Responsable> guidsResponsables = new List<Responsable>();
                foreach (var item in existenciaDeMovimientos)
                {
                    Responsable responsable = new Responsable();
                    responsable = area == Guid.Empty ? _context.Responsables.Where(re => re.IdResponsable.Equals(item.Responsable)).FirstOrDefault() : _context.Responsables.Where(re => re.IdResponsable.Equals(item.Responsable) && item.Area.Equals(area)).FirstOrDefault();
                    if (responsable != null && !guidsResponsables.Any(c => c.IdResponsable.Equals(item.Responsable)))
                        guidsResponsables.Add(responsable);
                }

                return guidsResponsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }

        }
        public async Task<List<Responsable>> ObtenerListadoResponsablesSiMovimiento(List<ExistenciaMovimiento> existenciaMovimientos, DateTime fecha)
        {
            List<ActFijo> actFijos = new List<ActFijo>();
            foreach (var item in existenciaMovimientos)
            {
                ActFijo actFijo = new ActFijo();
                actFijo = await _context.ActFijos.Include(c => c.ResponsableNavigation)
                    .Include(c => c.AreaResponsabilidadNavigation)
                    .Include(c => c.CentroCostoNavigation)
                    .Where(c => c.FechaSubmayor.Year.Equals(fecha.Year) && c.FechaSubmayor.Month.Equals(fecha.Month) && c.NoInventario.Equals(item.NoInventario))
                    .FirstOrDefaultAsync();

                if (actFijo != null)
                {
                    actFijos.Add(actFijo);
                }
            }

            List<Responsable> responsables = new List<Responsable>();
            responsables = actFijos.Where(c => !c.ResponsableNavigation.Nombre.ToLower().Trim().Equals("<<S/R>>".ToLower().Trim())).Select(c => c.ResponsableNavigation).Distinct().ToList();

            return responsables;
        }
        public async Task<List<Responsable>> ObtenerListadoResponsablesSiMovimiento(List<ExistenciaMovimientoUTH> existenciaMovimientos, DateTime fecha)
        {
            List<UtilesHerramienta> listaUTH = new List<UtilesHerramienta>();
            foreach (var item in existenciaMovimientos)
            {
                UtilesHerramienta uth = new UtilesHerramienta();
                uth = await _context.UtilesHerramientas.Include(c => c.ResponsableNavigation)
                    .Include(c => c.AreaResponsabilidadNavigation)
                    .Include(c => c.CentroCostoNavigation)
                    .Where(c => c.FechaListadoUth.Year.Equals(fecha.Year) && c.FechaListadoUth.Month.Equals(fecha.Month) && string.Concat(c.Codigo, "|", c.Responsable, "|", c.CentroCosto, "|", c.AreaResponsabilidad).Equals(item.Discriminante))
                    .FirstOrDefaultAsync();

                if (uth != null)
                {
                    listaUTH.Add(uth);
                }
            }

            List<Responsable> responsables = new List<Responsable>();
            responsables = listaUTH.Where(c => !c.ResponsableNavigation.Nombre.ToLower().Trim().Equals("<<S/R>>".ToLower().Trim())).Select(c => c.ResponsableNavigation).Distinct().ToList();

            return responsables;
        }
        public async Task<List<Responsable>> ObtenerListadoResponsablesSiMovimiento(List<ActivosFijosConMovimientos> existenciaDeMovimientos, bool siActuales = true)
        {
            try
            {
                List<Responsable> guidsResponsables = new List<Responsable>();
                if (siActuales)
                {
                    foreach (var item in existenciaDeMovimientos)
                    {
                        Responsable responsable = new Responsable();
                        responsable = _context.Responsables.Where(re => re.IdResponsable.Equals(item.idResponsableActual)).FirstOrDefault();
                        if (responsable != null && !guidsResponsables.Any(c => c.IdResponsable.Equals(item.idResponsableActual)))
                        {
                            AreaDeResponsabilidad areaResp = new AreaDeResponsabilidad();
                            areaResp = _context.AreaDeResponsabilidads.Where(ar => ar.IdArea.Equals(item.idAreaActual)).FirstOrDefault();
                            responsable.AreaResponsabilidadNavigation = areaResp;
                            responsable.AreaResponsabilidad = areaResp.IdArea;
                            guidsResponsables.Add(responsable);
                        }
                    }
                }
                else
                {
                    foreach (var item in existenciaDeMovimientos)
                    {
                        Responsable responsable = new Responsable();
                        responsable = _context.Responsables.Where(re => re.IdResponsable.Equals(item.idResponsableAnterior)).FirstOrDefault();
                        if (responsable != null && !guidsResponsables.Any(c => c.IdResponsable.Equals(item.idResponsableAnterior)))
                        {
                            AreaDeResponsabilidad areaResp = new AreaDeResponsabilidad();
                            areaResp = _context.AreaDeResponsabilidads.Where(ar => ar.IdArea.Equals(item.idAreaAnterior)).FirstOrDefault();
                            responsable.AreaResponsabilidadNavigation = areaResp;
                            responsable.AreaResponsabilidad = areaResp.IdArea;
                            guidsResponsables.Add(responsable);
                        }
                    }
                }

                return guidsResponsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }

        }
        public async Task<List<Responsable>> ObtenerListadoResponsablesSiMovimiento(List<UTHConMovimientosParcial> existenciaDeMovimientos, bool siActuales = true)
        {
            try
            {
                List<Responsable> guidsResponsables = new List<Responsable>();
                foreach (var item in existenciaDeMovimientos)
                {
                    Responsable responsable = new Responsable();
                    responsable = _context.Responsables.Where(re => re.IdResponsable.Equals(item.Responsable)).FirstOrDefault();
                    if (responsable != null && !guidsResponsables.Any(c => c.IdResponsable.Equals(item.Responsable)))
                    {
                        AreaDeResponsabilidad areaResp = new AreaDeResponsabilidad();
                        areaResp = _context.AreaDeResponsabilidads.Where(ar => ar.IdArea.Equals(item.Area)).FirstOrDefault();
                        responsable.AreaResponsabilidadNavigation = areaResp;
                        responsable.AreaResponsabilidad = areaResp.IdArea;
                        guidsResponsables.Add(responsable);
                    }
                }

                return guidsResponsables;
            }
            catch (Exception ex)
            {
                return new List<Responsable>();
            }

        }
        public async Task<Responsable> ObtenerInfoDirector()
        {
            return await _context.Responsables.Include(res => res.CargoNavigation).SingleOrDefaultAsync(c => c.CargoNavigation.Nombre.ToLower().Equals("Director UEB Aplicaciones de Redes"));
        }
        public async Task<Responsable> ObtenerInfoEncargadoEconomia()
        {
            return await _context.Responsables.Include(res => res.CargoNavigation).SingleOrDefaultAsync(c => c.ResponsableDeEconomia);
        }
        public async Task<Responsable> SeleccionarResponsablePorArea(Guid area, Guid resp)
        {
            var responsable = await _context.Responsables
                .Include(c => c.CargoNavigation)
                .Include(c => c.AreaResponsabilidadNavigation)
                .Where(c => c.AreaResponsabilidad.Equals(area) && c.IdResponsable.Equals(resp))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            return responsable;
        }
        public async Task<Responsable> SeleccionarResponsableEPDelArea(Guid area)
        {
            var responsable = await _context.Responsables
                .Include(c => c.CargoNavigation)
                .Include(c => c.AreaResponsabilidadNavigation)
                .Where(c => c.AreaResponsabilidad.Equals(area) && c.ResponsableDelArea)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            return responsable;
        }
        public Responsable? Clonar(Responsable obj)
        {
            return obj != null ? new Responsable
            {
                IdResponsable = obj.IdResponsable,
                Nombre = obj.Nombre,
                Apellidos = obj.Apellidos,
                Cargo = obj.Cargo,
                AreaResponsabilidad = obj.AreaResponsabilidad,
                ActFijos = obj.ActFijos
            } : null;
        }
    }
}
