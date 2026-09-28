using ActivosFijos.Clases;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoParametro
    {
        private aftContext _context;
        public repoParametro(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<Parametro>> ObtenerListadoDeParametros()
        {
            return await _context.Parametros.OrderBy(p => p.Grupo).ThenBy(p => p.Nombre).ToListAsync().ConfigureAwait(false);
        }
        public async Task<Parametro?> SeleccionarParametroPorNombre(string nombre)
        {
            return await _context.Parametros.FirstOrDefaultAsync(p => p.Nombre == nombre).ConfigureAwait(false);
        }
        public async Task<List<Parametro>> SeleccionarParametrosPorGrupo(string grupo)
        {
            return await _context.Parametros.Where(p => p.Grupo == grupo).ToListAsync().ConfigureAwait(false);
        }
        public bool ParametroExists(Guid id)
        {
            return _context.Parametros.Any(e => e.IdParametro == id);
        }
        public bool SiExisteNombreDelParametro(string nombre)
        {
            return _context.Parametros.Any(p => p.Nombre == nombre);
        }
        public bool SiExisteNombreDelParametroAModificar(string nombre, Guid id)
        {
            return _context.Parametros.Any(p => p.Nombre == nombre && p.IdParametro != id);
        }
        public async Task<clResponse<Parametro>> InsertarParametro(Parametro parametro)
        {
            if (parametro == null)
                return new clResponse<Parametro>() { SiSatisfactorio = false, Mensaje = "El Parámetro a insertar es incorrecto." };
            try
            {
                _context.Add(parametro);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Parametro>() { SiSatisfactorio = true, Mensaje = "El Parámetro ha sido guardado satisfactoriamente.", Resultado = parametro };
            }
            catch (Exception ex)
            {
                return new clResponse<Parametro>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
        public async Task<clResponse<Parametro>> ModificarParametro(Parametro parametro)
        {
            if (parametro == null)
                return new clResponse<Parametro>() { SiSatisfactorio = false, Mensaje = "El Parámetro a modificar es incorrecto." };
            try
            {
                _context.Update(parametro);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Parametro>() { SiSatisfactorio = true, Mensaje = "El Parámetro ha sido modificado satisfactoriamente.", Resultado = parametro };
            }
            catch (Exception ex)
            {
                return new clResponse<Parametro>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
        public async Task<clResponse<Parametro>> EliminarParametro(Parametro parametro)
        {
            if (parametro == null)
                return new clResponse<Parametro>() { SiSatisfactorio = false, Mensaje = "El Parámetro a eliminar es incorrecto." };
            try
            {
                _context.Parametros.Remove(parametro);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Parametro>() { SiSatisfactorio = true, Mensaje = "El Parámetro ha sido eliminado satisfactoriamente." };
            }
            catch (Exception ex)
            {
                return new clResponse<Parametro>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
        public async Task<Parametro> SeleccionarParametroPorID(Guid? id)
        {
            var parametro = await _context.Parametros.FindAsync(id).ConfigureAwait(false);
            return parametro;
        }
    }
}