using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoLocales
    {
        private aftContext _context;
        public repoLocales(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<Locale>> ObtenerListadoLocales()
        {
            return await _context.Locales.Include(c => c.CubiculoNavigation).OrderBy(c => c.Nombre).ToListAsync().ConfigureAwait(false);
        }
        public bool LocalExists(Guid id)
        {
            return _context.Locales.Any(e => e.IdLocal == id);
        }
        public async Task<clResponse<Locale>> InsertarLocal(Locale local)
        {
            if (local == null)
                return new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = "El Local a insertar es incorrecto." };
            try
            {
                _context.Add(local);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Locale>() { SiSatisfactorio = true, Mensaje = "El Local ha sido guardado satisfactoriamente.", Resultado = local };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
            }
            catch (Exception ex)
            {
                return new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
            }
        }
        public async Task<clResponse<Locale>> ModificarLocal(Locale local)
        {
            if (local == null)
                return new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = "El Local a modificar no existe." };
            try
            {
                _context.Update(local);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Locale>() { SiSatisfactorio = true, Mensaje = "El Local ha sido modificado satisfactoriamente.", Resultado = local };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
            }
            catch (Exception ex)
            {
                return new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
            }
        }
        public async Task<clResponse<Locale>> EliminarLocal(Locale local)
        {
            if (local == null)
                return new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = "El Local a eliminar no existe." };
            try
            {
                _context.Locales.Remove(local);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Locale>() { SiSatisfactorio = true, Mensaje = "El Local ha sido eliminado satisfactoriamente." };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                    return new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message + " " + ex.InnerException.Message };
             }
            catch (Exception ex)
            {
                    return new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex1.Message + " " + ex1.InnerException.Message };
            }
        }
        public async Task<Locale> SeleccionarLocalesPorID(Guid? id)
        {
            var locales = await _context.Locales
                .FindAsync(id).ConfigureAwait(false);
            return locales;
        }
        public bool SiExisteNombreDelLocal(string nombre)
        {
            return _context.Locales.Any(e => e.Nombre == nombre);
        }
        public bool SiExisteNombreDelLocalAModificar(string nombre, Guid id)
        {
            bool siExiste = _context.Locales.Any(cg => cg.Nombre == nombre);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.Locales.Any(cg => cg.Nombre == nombre && cg.IdLocal == id);
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
        public bool SiExisteElLocalConElID(Guid? id)
        {
            bool siExiste = _context.Locales.Any(e => e.IdLocal == id);
            return siExiste;
        }
    }
}
