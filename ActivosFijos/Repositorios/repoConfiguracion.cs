using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoConfiguracion : iClone<Configuracion>
    {
        private aftContext _context;
        public repoConfiguracion(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<Configuracion>> ObtenerListadoDeConfiguraciones()
        {
            return await _context.Configuracions.ToListAsync().ConfigureAwait(false);
        }
        public bool ConfiguracionExists(Guid id)
        {
            return _context.Configuracions.Any(e => e.IdConfiguracion == id);
        }
        public async Task<clResponse<Configuracion>> InsertarConfiguracion(Configuracion config, bool clonar = false)
        {
            if (config == null)
                return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "La Configuración a insertar es incorrecta." };
            try
            {
                _context.Add(config);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Configuracion>() { SiSatisfactorio = true, Mensaje = "La Configuración ha sido guardada satisfactoriamente.", Resultado = clonar ? Clonar(config) : config };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ConfiguracionExists(config.IdConfiguracion))
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "La Configuración a insertar no existe." };
                }
                else
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "Ha ocurrido un error, no se pudo insertar la Configuración. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!ConfiguracionExists(config.IdConfiguracion))
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "La Configuración a insertar no existe." };
                }
                else
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar el Cargo. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<Configuracion>> ModificarConfiguracion(Configuracion config)
        {
            if (config == null)
                return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "La Configuración a modificar es incorrecta." };
            try
            {
                _context.Update(config);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Configuracion>() { SiSatisfactorio = true, Mensaje = "La Configuración ha sido modificada satisfactoriamente.", Resultado = config };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ConfiguracionExists(config.IdConfiguracion))
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "La Configuración a modificar no existe." };
                }
                else
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!ConfiguracionExists(config.IdConfiguracion))
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "La Configuración a modificar no existe." };
                }
                else
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<Configuracion>> EliminarConfiguracion(Configuracion config)
        {
            if (config == null)
                return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "La Configuración a eliminar es incorrecto." };
            try
            {
                _context.Configuracions.Remove(config);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Configuracion>() { SiSatisfactorio = true, Mensaje = "La Configuración ha sido eliminada satisfactoriamente." };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ConfiguracionExists(config.IdConfiguracion))
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "La Configuración a eliminar no existe." };
                }
                else
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message + " " + ex.InnerException.Message };
                }
            }
            catch (Exception ex)
            {
                if (!ConfiguracionExists(config.IdConfiguracion))
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "La Configuración a eliminar no existe." };
                }
                else
                {
                    return new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex1.Message + " " + ex1.InnerException.Message };
                }
            }
        }
        public async Task<Configuracion> SeleccionarConfiguracionPorID(Guid? id)
        {
            var config = await _context.Configuracions
                .FindAsync(id).ConfigureAwait(false);
            return config;
        }
        public bool SiExisteNombreDeLaConfiguracion(string nombre)
        {
            return _context.Configuracions.Any(e => e.Nombre == nombre);
        }
        public bool SiExisteNombreDeLaConfiguracionAModificar(string nombre, Guid id)
        {
            bool siExiste = _context.Configuracions.Any(cg => cg.Nombre == nombre);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.Configuracions.Any(cg => cg.Nombre == nombre && cg.IdConfiguracion == id);
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
        public bool SiExisteValorDeLaColumna(string columna)
        {
            return _context.Configuracions.Any(e => e.Columna == columna);
        }
        public bool SiExisteValorDeLaColumnaAModificar(string columna, Guid id)
        {
            return _context.Configuracions.Any(e => e.Columna == columna && e.IdConfiguracion != id);
        }
        public Configuracion? Clonar(Configuracion obj)
        {
            return obj != null ? new Configuracion
            {
                IdConfiguracion = obj.IdConfiguracion,
                Nombre = obj.Nombre,
                Columna = obj.Columna,
                Fila = obj.Fila
            } : null;
        }
    }
}
