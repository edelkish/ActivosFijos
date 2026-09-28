using ActivosFijos.Clases;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoTipoMovimiento
    {
        private aftContext _context;
        public repoTipoMovimiento(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<TipoMovimientoAFT>> ObtenerListadoDeTiposMovimiento()
        {
            return await _context.TipoMovimientoAFTs.OrderBy(t => t.Orden).ToListAsync().ConfigureAwait(false);
        }
        public async Task<List<TipoMovimientoAFT>> ObtenerListadoDeTiposMovimientoActivos()
        {
            return await _context.TipoMovimientoAFTs.Where(t => t.SiActivo).OrderBy(t => t.Orden).ToListAsync().ConfigureAwait(false);
        }
        public async Task<TipoMovimientoAFT?> SeleccionarTipoMovimientoPorNombre(string nombre)
        {
            return await _context.TipoMovimientoAFTs.FirstOrDefaultAsync(t => t.Nombre == nombre).ConfigureAwait(false);
        }
        public bool TipoMovimientoExists(Guid id)
        {
            return _context.TipoMovimientoAFTs.Any(e => e.IdTipoMovimientoAFT == id);
        }
        public async Task<clResponse<TipoMovimientoAFT>> InsertarTipoMovimiento(TipoMovimientoAFT tipoMovimiento)
        {
            if (tipoMovimiento == null)
                return new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = false, Mensaje = "El Tipo de Movimiento a insertar es incorrecto." };
            try
            {
                _context.Add(tipoMovimiento);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = true, Mensaje = "El Tipo de Movimiento ha sido guardado satisfactoriamente.", Resultado = tipoMovimiento };
            }
            catch (Exception ex)
            {
                return new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
        public async Task<clResponse<TipoMovimientoAFT>> ModificarTipoMovimiento(TipoMovimientoAFT tipoMovimiento)
        {
            if (tipoMovimiento == null)
                return new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = false, Mensaje = "El Tipo de Movimiento a modificar es incorrecto." };
            try
            {
                _context.Update(tipoMovimiento);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = true, Mensaje = "El Tipo de Movimiento ha sido modificado satisfactoriamente.", Resultado = tipoMovimiento };
            }
            catch (Exception ex)
            {
                return new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
        public async Task<clResponse<TipoMovimientoAFT>> EliminarTipoMovimiento(TipoMovimientoAFT tipoMovimiento)
        {
            if (tipoMovimiento == null)
                return new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = false, Mensaje = "El Tipo de Movimiento a eliminar es incorrecto." };
            try
            {
                _context.TipoMovimientoAFTs.Remove(tipoMovimiento);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = true, Mensaje = "El Tipo de Movimiento ha sido eliminado satisfactoriamente." };
            }
            catch (Exception ex)
            {
                return new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
        public async Task<TipoMovimientoAFT> SeleccionarTipoMovimientoPorID(Guid? id)
        {
            var tipoMovimiento = await _context.TipoMovimientoAFTs.FindAsync(id).ConfigureAwait(false);
            return tipoMovimiento;
        }
    }
}