using ActivosFijos.Clases;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoTrasladosAFT
    {
        private aftContext _context;
        public repoTrasladosAFT(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Movimientos declarados que tocan el mes pedido. Un movimiento abierto (sin hasta)
        /// sigue vigente todo el tiempo, por eso entra aunque haya empezado meses atras.
        /// </summary>
        public async Task<List<TrasladosAft>> ObtenerPorMes(DateTime fecha)
        {
            DateTime inicio = new DateTime(fecha.Year, fecha.Month, 1);
            DateTime fin = inicio.AddMonths(1);

            return await _context.TrasladosAfts
                .Include(t => t.ResponsableNavigation)
                .Include(t => t.TipoMovimientoNavigation)
                .Include(t => t.AreaDestinoNavigation)
                .Where(t => t.Desde < fin && (t.Hasta == null || t.Hasta >= inicio))
                .OrderBy(t => t.NoInventario)
                .ThenBy(t => t.SubNumero)
                .ThenBy(t => t.Desde)
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<TrasladosAft?> SeleccionarPorId(Guid id)
        {
            return await _context.TrasladosAfts
                .FirstOrDefaultAsync(t => t.Id == id)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Clave del movimiento: el mismo Activo Fijo no puede declararse dos veces el mismo dia.
        /// </summary>
        public async Task<bool> ExisteMismoDia(long noInventario, int subNumero, DateTime desde, Guid? idPropio = null)
        {
            return await _context.TrasladosAfts
                .AnyAsync(t => t.NoInventario == noInventario
                    && t.SubNumero == subNumero
                    && t.Desde == desde
                    && (idPropio == null || t.Id != idPropio))
                .ConfigureAwait(false);
        }

        public async Task<clResponse<TrasladosAft>> Guardar(TrasladosAft movimiento)
        {
            if (movimiento == null)
                return new clResponse<TrasladosAft> { SiSatisfactorio = false, Mensaje = "El movimiento a guardar es incorrecto." };

            try
            {
                if (movimiento.Id == Guid.Empty)
                {
                    movimiento.Id = Guid.NewGuid();
                    _context.TrasladosAfts.Add(movimiento);
                }
                else
                {
                    _context.TrasladosAfts.Update(movimiento);
                }

                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<TrasladosAft> { SiSatisfactorio = true, Mensaje = "El movimiento fue guardado satisfactoriamente.", Resultado = movimiento };
            }
            catch (Exception ex)
            {
                return new clResponse<TrasladosAft> { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }

        public async Task<clResponse<int>> Eliminar(Guid id)
        {
            try
            {
                TrasladosAft? movimiento = await SeleccionarPorId(id).ConfigureAwait(false);
                if (movimiento == null)
                    return new clResponse<int> { SiSatisfactorio = false, Mensaje = "El movimiento que se quiere eliminar no existe." };

                _context.TrasladosAfts.Remove(movimiento);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<int> { SiSatisfactorio = true, Mensaje = "El movimiento fue eliminado satisfactoriamente." };
            }
            catch (Exception ex)
            {
                return new clResponse<int> { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }
    }
}
