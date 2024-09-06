using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoCargos: iClone<Cargo>
    {
        private aftContext _context;
        public repoCargos(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<Cargo>> ObtenerListadoCargos()
        {
            return await _context.Cargos.OrderBy(c => c.Nombre).ToListAsync().ConfigureAwait(false);
        }
        public bool CargosExists(Guid id)
        {
            return _context.Cargos.Any(e => e.IdCargo == id);
        }
        public async Task<clResponse<Cargo>> InsertarCargos(Cargo cargos, bool clonar = false)
        {
            if (cargos == null)
                return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El Cargo a insertar es incorrecto." };
            try
            {
                _context.Add(cargos);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Cargo>() { SiSatisfactorio = true, Mensaje = "El Cargo ha sido guardado satisfactoriamente.", Resultado = clonar ? Clonar(cargos) : cargos };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CargosExists(cargos.IdCargo))
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El Cargo a insertar no existe." };
                }
                else
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!CargosExists(cargos.IdCargo))
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El Cargo a insertar no existe." };
                }
                else
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<Cargo>> ModificarCargo(Cargo cargos)
        {
            if (cargos == null)
                return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El Cargo a modificar es incorrecto." };
            try
            {
                _context.Update(cargos);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Cargo>() { SiSatisfactorio = true, Mensaje = "El Cargo ha sido modificado satisfactoriamente.", Resultado = cargos };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CargosExists(cargos.IdCargo))
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El Cargo a modificar no existe." };
                }
                else
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!CargosExists(cargos.IdCargo))
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El Cargo a modificar no existe." };
                }
                else
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<Cargo>> EliminarCargo(Cargo cargo)
        {
            if (cargo == null)
                return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El Cargo a eliminar es incorrecto." };
            try
            {
                _context.Cargos.Remove(cargo);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Cargo>() { SiSatisfactorio = true, Mensaje = "El Cargo ha sido eliminado satisfactoriamente." };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CargosExists(cargo.IdCargo))
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El Cargo a eliminar no existe." };
                }
                else
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message + " " + ex.InnerException.Message };
                }
            }
            catch (Exception ex)
            {
                if (!CargosExists(cargo.IdCargo))
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El Cargo a eliminar no existe." };
                }
                else
                {
                    return new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex1.Message + " " + ex1.InnerException.Message };
                }
            }
        }
        public async Task<Cargo> SeleccionarCargosPorID(Guid? id)
        {
            var cargos = await _context.Cargos
                .FindAsync(id).ConfigureAwait(false);
            return cargos;
        }
        public bool SiExisteNombreDelCargo(string nombre)
        {
            return _context.Cargos.Any(e => e.Nombre == nombre);
        }
        public bool SiExisteNombreDelCargoAModificar(string nombre, Guid id)
        {
            bool siExiste = _context.Cargos.Any(cg => cg.Nombre == nombre);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.Cargos.Any(cg => cg.Nombre == nombre && cg.IdCargo == id);
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
        public bool SiExisteElCargoConElID(Guid? id)
        {
            bool siExiste = _context.Cargos.Any(e => e.IdCargo == id);
            return siExiste;
        }
        public Cargo Clonar(Cargo obj)
        {
            return obj != null ? new Cargo
            {
                IdCargo = obj.IdCargo,
                Nombre = obj.Nombre,
                Descripcion = obj.Descripcion
            } : null;
        }
    }
}
