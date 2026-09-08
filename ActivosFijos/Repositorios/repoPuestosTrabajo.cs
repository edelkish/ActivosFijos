using ActivosFijos.Clases;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoPuestosTrabajo
    {
        private aftContext _context;
        public repoPuestosTrabajo(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<PuestoTrabajo>> ObtenerListadoPuestos()
        {
            return await _context.PuestosTrabajo
                .Include(p => p.LocalNavigation)
                .OrderBy(p => p.Nombre)
                .ToListAsync().ConfigureAwait(false);
        }
        public async Task<List<PuestoTrabajo>> ObtenerPuestosPorLocal(Guid idLocal)
        {
            return await _context.PuestosTrabajo
                .Where(p => p.IdLocal == idLocal)
                .OrderBy(p => p.Nombre)
                .ToListAsync().ConfigureAwait(false);
        }
        public bool PuestoTrabajoExists(Guid id)
        {
            return _context.PuestosTrabajo.Any(e => e.IdPuesto == id);
        }
        public async Task<PuestoTrabajo?> SeleccionarPuestoPorID(Guid? id)
        {
            return await _context.PuestosTrabajo
                .Include(p => p.LocalNavigation)
                .FirstOrDefaultAsync(p => p.IdPuesto == id).ConfigureAwait(false);
        }
        public async Task<clResponse<PuestoTrabajo>> InsertarPuestoTrabajo(PuestoTrabajo puestoTrabajo)
        {
            if (puestoTrabajo == null)
                return new clResponse<PuestoTrabajo>() { SiSatisfactorio = false, Mensaje = "El Puesto de Trabajo a insertar es incorrecto." };
            try
            {
                _context.Add(puestoTrabajo);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<PuestoTrabajo>() { SiSatisfactorio = true, Mensaje = "El Puesto de Trabajo ha sido guardado satisfactoriamente.", Resultado = puestoTrabajo };
            }
            catch (Exception ex)
            {
                return new clResponse<PuestoTrabajo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
        public async Task<clResponse<PuestoTrabajo>> ModificarPuestoTrabajo(PuestoTrabajo puestoTrabajo)
        {
            if (puestoTrabajo == null)
                return new clResponse<PuestoTrabajo>() { SiSatisfactorio = false, Mensaje = "El Puesto de Trabajo a modificar no existe." };
            try
            {
                _context.Update(puestoTrabajo);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<PuestoTrabajo>() { SiSatisfactorio = true, Mensaje = "El Puesto de Trabajo ha sido modificado satisfactoriamente.", Resultado = puestoTrabajo };
            }
            catch (Exception ex)
            {
                return new clResponse<PuestoTrabajo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
        public async Task<clResponse<PuestoTrabajo>> EliminarPuestoTrabajo(PuestoTrabajo puestoTrabajo)
        {
            if (puestoTrabajo == null)
                return new clResponse<PuestoTrabajo>() { SiSatisfactorio = false, Mensaje = "El Puesto de Trabajo a eliminar no existe." };
            try
            {
                _context.PuestosTrabajo.Remove(puestoTrabajo);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<PuestoTrabajo>() { SiSatisfactorio = true, Mensaje = "El Puesto de Trabajo ha sido eliminado satisfactoriamente." };
            }
            catch (Exception ex)
            {
                return new clResponse<PuestoTrabajo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
        public bool SiExisteNombreDelPuesto(string nombre, Guid idLocal)
        {
            return _context.PuestosTrabajo.Any(e => e.Nombre == nombre && e.IdLocal == idLocal);
        }
        public bool SiExisteNombreDelPuestoAModificar(string nombre, Guid idLocal, Guid idPuesto)
        {
            bool siExiste = _context.PuestosTrabajo.Any(p => p.Nombre == nombre && p.IdLocal == idLocal);
            if (!siExiste)
                return false;
            return !_context.PuestosTrabajo.Any(p => p.Nombre == nombre && p.IdLocal == idLocal && p.IdPuesto == idPuesto);
        }
    }
}
