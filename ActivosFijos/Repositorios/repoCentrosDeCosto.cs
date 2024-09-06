using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoCentrosDeCosto : iClone<CentroCosto>
    {
        private aftContext _context;

        public repoCentrosDeCosto(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<CentroCosto>> ListadoCentroDeCosto()
        {
            return await _context.CentroCostos.ToListAsync();
        }
        public async Task<CentroCosto> EncontrarCentroCostoPorID(Guid? id)
        {
            var centroDeCosto = await _context.CentroCostos
                .FirstOrDefaultAsync(m => m.IdCentroCosto == id);

            return centroDeCosto ?? new CentroCosto();
        }
        public async Task<CentroCosto> EncontrarCentroCostoPorNumero(long numero)
        {
            var centroDeCosto = await _context.CentroCostos
                .FirstOrDefaultAsync(m => m.Numero == numero);

            return centroDeCosto ?? new CentroCosto();
        }
        public bool SiExisteNombreDelCentroCosto(string nombre)
        {
            return _context.CentroCostos.Any(e => e.Nombre == nombre);
        }
        public bool SiExisteNumeroDelCentroCosto(long? numero)
        {
            return _context.CentroCostos.Any(e => e.Numero == numero);
        }
        public bool SiExisteNombreDelCentroCostoAEditar(string nombre, Guid id)
        {
            bool siExiste = _context.CentroCostos.Any(e => e.Nombre == nombre);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.CentroCostos.Any(e => e.Nombre == nombre && e.IdCentroCosto == id);
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
        public bool SiExisteNumeroDelCentroCostoAEditar(long? numero, Guid id)
        {
            bool siExiste = _context.CentroCostos.Any(e => e.Numero == numero);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.CentroCostos.Any(e => e.Numero == numero && e.IdCentroCosto == id);
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
        public bool SiExisteElCentroCostoConElID(Guid? idCentroCosto)
        {
            return _context.CentroCostos.Any(e => e.IdCentroCosto == idCentroCosto);
        }
        public async Task<clResponse<CentroCosto>> GuardarCentroCosto(CentroCosto centroC, bool clonar = false)
        {
            try
            {
                if (centroC == null)
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El Centro Costo a insertar no existe." };
                else
                {
                    _context.Add(centroC);
                    await _context.SaveChangesAsync();

                    return new clResponse<CentroCosto>() { SiSatisfactorio = true, Mensaje = $"El Centro Costo con el #{centroC.Numero} ha sido guardado satisfactoriamente.", Resultado = clonar ? Clonar(centroC) : centroC };
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteElCentroCostoConElID(centroC.IdCentroCosto))
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El Centro Costo a insertar no existe." };
                }
                else
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido un error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteElCentroCostoConElID(centroC.IdCentroCosto))
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El Centro Costo a insertar no existe." };
                }
                else
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<CentroCosto>> ModificarCentroCosto(CentroCosto centroC)
        {
            try
            {
                if (centroC == null)
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El Centro Costo a modificar no existe." };

                _context.Update(centroC);
                await _context.SaveChangesAsync();
                return new clResponse<CentroCosto>() { SiSatisfactorio = true, Mensaje = "El Centro Costo ha sido modificado satisfactoriamente.", Resultado = centroC };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteElCentroCostoConElID(centroC.IdCentroCosto))
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El Centro Costo a modificar no existe." };
                }
                else
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteElCentroCostoConElID(centroC.IdCentroCosto))
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El Centro Costo a modificar no existe." };
                }
                else
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<CentroCosto>> EliminarCentroCosto(CentroCosto centroC)
        {
            if (centroC == null)
                return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El Centro Costo a eliminar es incorrecto." };
            try
            {
                _context.CentroCostos.Remove(centroC);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<CentroCosto>() { SiSatisfactorio = true, Mensaje = $"El Centro Costo con el #{centroC.Numero} ha sido eliminado satisfactoriamente." };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SiExisteElCentroCostoConElID(centroC.IdCentroCosto))
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El Centro Costo a eliminar no existe." };
                }
                else
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message + " " + ex.InnerException.Message };
                }
            }
            catch (Exception ex)
            {
                if (!SiExisteElCentroCostoConElID(centroC.IdCentroCosto))
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El Centro Costo a eliminar no existe." };
                }
                else
                {
                    return new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex1.Message + " " + ex1.InnerException.Message };
                }
            }
        }
        public CentroCosto Clonar(CentroCosto obj)
        {
            return new CentroCosto
            {                
                Nombre = obj.Nombre,
                Numero = obj.Numero,
                IdCentroCosto = obj.IdCentroCosto,
                ActFijos = obj.ActFijos
            };
        }
    }
}
