using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoCubiculos
    {
        private aftContext _context;
        public repoCubiculos(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<Cubiculo>> ObtenerListadoCubiculos()
        {
            return await _context.Cubiculos.OrderBy(c => c.Nombre).ToListAsync().ConfigureAwait(false);
        }
        public bool CubiculoExists(Guid id)
        {
            return _context.Cubiculos.Any(e => e.IdCubiculo == id);
        }
        public async Task<clResponse<Cubiculo>> InsertarCubiculo(Cubiculo cubiculo)
        {
            if (cubiculo == null)
                return new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = "El cubículo a insertar es incorrecto." };
            
            try
            {
                _context.Add(cubiculo);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Cubiculo>() { SiSatisfactorio = true, Mensaje = "El cubículo ha sido guardado satisfactoriamente.", Resultado = cubiculo };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
            }
            catch (Exception ex)
            {
                return new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
            }
        }
        public async Task<clResponse<Cubiculo>> ModificarCubiculo(Cubiculo cubiculo)
        {
            if (cubiculo == null)
                return new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = "El cubículo a modificar no existe." };
            try
            {
                _context.Update(cubiculo);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Cubiculo>() { SiSatisfactorio = true, Mensaje = "El cubículo ha sido modificado satisfactoriamente.", Resultado = cubiculo };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
            }
            catch (Exception ex)
            {
                return new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
            }
        }
        public async Task<clResponse<Cubiculo>> EliminarCubiculo(Cubiculo cubiculo)
        {
            if (cubiculo == null)
                return new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = "El cubículo a eliminar no existe." };
            try
            {
                _context.Cubiculos.Remove(cubiculo);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Cubiculo>() { SiSatisfactorio = true, Mensaje = "El cubículo ha sido eliminado satisfactoriamente." };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                    return new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message + " " + ex.InnerException.Message };
             }
            catch (Exception ex)
            {
                    return new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex1.Message + " " + ex1.InnerException.Message };
            }
        }
        public async Task<Cubiculo?> SeleccionarCubiculosPorID(Guid? id)
        {
            var cubiculos = await _context.Cubiculos.FindAsync(id).ConfigureAwait(false);
            return cubiculos;
        }
        public bool SiExisteNombreDelCubiculo(string nombre)
        {
            return _context.Cubiculos.Any(e => e.Nombre == nombre);
        }
        public bool SiExisteNombreDelCubiculoAModificar(string nombre, Guid id)
        {
            bool siExiste = _context.Cubiculos.Any(cg => cg.Nombre == nombre);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.Cubiculos.Any(cg => cg.Nombre == nombre && cg.IdCubiculo == id);
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
        public bool SiExisteApodoDelCubiculo(string apodo)
        {
            return _context.Cubiculos.Any(e => e.Apodo == apodo);
        }
        public bool SiExisteApodoDelCubiculoAModificar(string apodo, Guid id)
        {
            bool siExiste = _context.Cubiculos.Any(cg => cg.Apodo == apodo);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.Cubiculos.Any(cg => cg.Apodo == apodo && cg.IdCubiculo == id);
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
    }
}
