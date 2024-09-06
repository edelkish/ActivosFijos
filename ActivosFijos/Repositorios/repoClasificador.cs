using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.AuxBD;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoClasificador: iClone<Clasificador>
    {
        private aftContext _context;
        private aftContextAux _contextAux;
        public repoClasificador(aftContext context, aftContextAux contextAux)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAux = contextAux;
        }
        public async Task<List<Clasificador>> ObtenerListadoClasificador()
        {
            return await _context.Clasificadors.OrderBy(c => c.Nombre).ToListAsync().ConfigureAwait(false);
        }
        public bool ClasificadorsExists(Guid id)
        {
            return _context.Clasificadors.Any(e => e.IdClasificador == id);
        }
        public async Task<clResponse<Clasificador>> InsertarClasificador(Clasificador clasificador, bool clonar = false)
        {
            if (clasificador == null)
                return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a insertar es incorrecto." };
            try
            {
                _context.Add(clasificador);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Clasificador>() { SiSatisfactorio = true, Mensaje = "El Clasificador ha sido guardado satisfactoriamente.", Resultado = clonar ? Clonar(clasificador) : clasificador };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ClasificadorsExists(clasificador.IdClasificador))
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a insertar es incorrecto." };
                }
                else
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a insertar es incorrecto." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!ClasificadorsExists(clasificador.IdClasificador))
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a insertar es incorrecto." };
                }
                else
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Amdinistrador." }; //"Ha ocurrido un error, no se pudo insertar El Clasificador. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<Clasificador>> ModificarClasificador(Clasificador clasificador)
        {
            if (clasificador == null)
                return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a modificar es incorrecto." };
            try
            {
                _context.Update(clasificador);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Clasificador>() { SiSatisfactorio = true, Mensaje = "El Clasificador ha sido modificado satisfactoriamente.", Resultado = clasificador };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ClasificadorsExists(clasificador.IdClasificador))
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a modificar no existe." };
                }
                else
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
            catch (Exception ex)
            {
                if (!ClasificadorsExists(clasificador.IdClasificador))
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a modificar no existe." };
                }
                else
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message };
                }
            }
        }
        public async Task<clResponse<Clasificador>> EliminarClasificador(Clasificador clasificador)
        {
            if (clasificador == null)
                return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a eliminar es incorrecto." };
            try
            {
                _context.Clasificadors.Remove(clasificador);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<Clasificador>() { SiSatisfactorio = true, Mensaje = "El Clasificador ha sido eliminado satisfactoriamente." };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ClasificadorsExists(clasificador.IdClasificador))
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a eliminar no existe." };
                }
                else
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex.Message + " " + ex.InnerException.Message };
                }
            }
            catch (Exception ex)
            {
                if (!ClasificadorsExists(clasificador.IdClasificador))
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El Clasificador a eliminar no existe." };
                }
                else
                {
                    return new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };//ex1.Message + " " + ex1.InnerException.Message };
                }
            }
        }
        public async Task<Clasificador> SeleccionarClasificadorPorID(Guid? id)
        {
            var clasificador = await _context.Clasificadors
                .FindAsync(id).ConfigureAwait(false);
            return clasificador;
        }
        public bool SiExisteNombreDelClasificador(string nombre)
        {
            return _context.Clasificadors.Any(e => e.Nombre == nombre);
        }
        public bool SiExisteNombreDelClasificadorAModificar(string nombre, Guid id)
        {
            bool siExiste = _context.Clasificadors.Any(cg => cg.Nombre == nombre);
            if (!siExiste)
            {
                return false;
            }
            else
            {
                siExiste = _context.Clasificadors.Any(cg => cg.Nombre == nombre && cg.IdClasificador == id);
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
        public bool SiExisteElClasificadorConElID(Guid? id)
        {
            bool siExiste = _context.Clasificadors.Any(e => e.IdClasificador == id);
            return siExiste;
        }
        public async Task<Guid> ObtenerReferenciaSinClasificar()
        {
            Clasificador clasif = new Clasificador();
            clasif = await _context.Clasificadors.Where(a => a.Nombre.Equals("Sin Clasificar")).FirstOrDefaultAsync().ConfigureAwait(false);
            return clasif.IdClasificador;
        }
        public async Task<List<ClasificadoresTotales>> ListadoClasificadoresTotales()
        {
            try
            {
                List<ClasificadoresTotales> query = new List<ClasificadoresTotales>();
                query = await _contextAux.ClasificadoresTotales.FromSqlRaw($@"DECLARE @v_fecha DATETIME =
                                                                                        (
                                                                                            SELECT MAX(fechaSubmayor) FROM dbo.ActFijo
                                                                                        );

                                                                                SELECT CLASIF.Nombre AS [clasificador],
                                                                                       ISNULL(COUNT(AFT.noInventario), 0) AS [cantidad]
                                                                                FROM dbo.Clasificador CLASIF
                                                                                    LEFT JOIN dbo.ActFijo AFT
                                                                                        ON AFT.clasificador = CLASIF.idClasificador
                                                                                            AND AFT.fechaSubmayor = @v_fecha
                                                                                GROUP BY CLASIF.Nombre;").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ClasificadoresTotales>();
            }
        }
        public async Task<List<ClasificadoresTotales>> ListadoClasificadoresTotales(Guid centroCosto)
        {
            try
            {
                List<ClasificadoresTotales> query = new List<ClasificadoresTotales>();
                query = await _contextAux.ClasificadoresTotales.FromSqlRaw($@"DECLARE @v_fecha DATETIME =
                                                                                        (
                                                                                            SELECT MAX(fechaSubmayor) FROM dbo.ActFijo
                                                                                        ), @v_centroCosto UNIQUEIDENTIFIER;
                                                                                SET @v_centroCosto = '{centroCosto}';

                                                                                SELECT CLASIF.Nombre AS [clasificador],
                                                                                       ISNULL(COUNT(AFT.noInventario), 0) AS [cantidad]
                                                                                FROM dbo.Clasificador CLASIF
                                                                                    LEFT JOIN dbo.ActFijo AFT
                                                                                        ON AFT.clasificador = CLASIF.idClasificador
                                                                                            AND AFT.fechaSubmayor = @v_fecha
                                                                                            AND AFT.centroCosto = @v_centroCosto
                                                                                GROUP BY CLASIF.Nombre;").ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                return new List<ClasificadoresTotales>();
            }
        }
        public Clasificador Clonar(Clasificador obj)
        {
            return obj != null ? new Clasificador
            {
                IdClasificador = obj.IdClasificador,
                Nombre = obj.Nombre,
                Descripcion = obj.Descripcion
            } : null;
        }
    }
}
