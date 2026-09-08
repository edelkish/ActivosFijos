using ActivosFijos.Clases;
using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoUbicacionesAFT
    {
        private aftContext _context;
        public repoUbicacionesAFT(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private async Task<DateTime> ObtenerUltimoSubmayor()
        {
            return await _context.ActFijos.MaxAsync(a => a.FechaSubmayor).ConfigureAwait(false);
        }

        public async Task<List<mdArbolUbicacion>> ObtenerCubiculosArbol()
        {
            return await _context.Cubiculos
                .OrderBy(c => c.Nombre)
                .Select(c => new mdArbolUbicacion
                {
                    Id = c.IdCubiculo,
                    Tipo = "cubiculo",
                    Nombre = c.Nombre ?? "",
                    Descripcion = c.Descripcion ?? "",
                    IdPadre = null,
                    TieneHijos = c.Locales.Any()
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<mdArbolUbicacion>> ObtenerLocalesDeCubiculo(Guid idCubiculo)
        {
            return await _context.Locales
                .Where(l => l.Cubiculo == idCubiculo)
                .OrderBy(l => l.Nombre)
                .Select(l => new mdArbolUbicacion
                {
                    Id = l.IdLocal,
                    Tipo = "local",
                    Nombre = l.Nombre,
                    Descripcion = l.Descripcion ?? "",
                    IdPadre = l.Cubiculo,
                    TieneHijos = l.PuestosTrabajo.Any()
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<mdArbolUbicacion>> ObtenerPuestosDeLocal(Guid idLocal)
        {
            return await _context.PuestosTrabajo
                .Where(p => p.IdLocal == idLocal)
                .OrderBy(p => p.Nombre)
                .Select(p => new mdArbolUbicacion
                {
                    Id = p.IdPuesto,
                    Tipo = "puesto",
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion ?? "",
                    IdPadre = p.IdLocal,
                    TieneHijos = false
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<mdAFTUbicacion>> ObtenerAFTsPorLocal(Guid idLocal)
        {
            DateTime fecha = await ObtenerUltimoSubmayor();
            return await _context.ActFijos
                .Where(a => a.FechaSubmayor == fecha && a.Local == idLocal)
                .OrderBy(a => a.NoInventario)
                .Select(a => new mdAFTUbicacion
                {
                    noInventario = a.NoInventario,
                    subNumero = a.SubNumero,
                    fechaSubmayor = a.FechaSubmayor,
                    descripcion = a.Descripcion ?? "",
                    clasificador = a.ClasificadorNavigation != null ? a.ClasificadorNavigation.Nombre : "",
                    localActual = a.LocalNavigation != null ? a.LocalNavigation.Nombre : "",
                    puestoActual = ""
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<mdAFTUbicacion>> ObtenerAFTsPorPuesto(Guid idPuesto)
        {
            DateTime fecha = await ObtenerUltimoSubmayor();
            return await _context.ActFijos
                .Where(a => a.FechaSubmayor == fecha && a.PuestoTrabajo == idPuesto)
                .OrderBy(a => a.NoInventario)
                .Select(a => new mdAFTUbicacion
                {
                    noInventario = a.NoInventario,
                    subNumero = a.SubNumero,
                    fechaSubmayor = a.FechaSubmayor,
                    descripcion = a.Descripcion ?? "",
                    clasificador = a.ClasificadorNavigation != null ? a.ClasificadorNavigation.Nombre : "",
                    localActual = a.PuestoTrabajoNavigation != null && a.PuestoTrabajoNavigation.LocalNavigation != null ? a.PuestoTrabajoNavigation.LocalNavigation.Nombre : "",
                    puestoActual = a.PuestoTrabajoNavigation != null ? a.PuestoTrabajoNavigation.Nombre : ""
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<mdAFTUbicacion>> ObtenerAFTsSinUbicacion()
        {
            DateTime fecha = await ObtenerUltimoSubmayor();
            return await _context.ActFijos
                .Where(a => a.FechaSubmayor == fecha && a.Local == null && a.PuestoTrabajo == null)
                .OrderBy(a => a.NoInventario)
                .Select(a => new mdAFTUbicacion
                {
                    noInventario = a.NoInventario,
                    subNumero = a.SubNumero,
                    fechaSubmayor = a.FechaSubmayor,
                    descripcion = a.Descripcion ?? "",
                    clasificador = a.ClasificadorNavigation != null ? a.ClasificadorNavigation.Nombre : "",
                    localActual = "",
                    puestoActual = ""
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<mdArbolUbicacion>> ObtenerContadoresGeneral()
        {
            DateTime fecha = await ObtenerUltimoSubmayor();
            var resultado = new List<mdArbolUbicacion>();

            var localesConAFT = await _context.ActFijos
                .Where(a => a.FechaSubmayor == fecha && a.Local != null)
                .GroupBy(a => a.Local)
                .Select(g => new { Id = g.Key, Contador = g.Count() })
                .ToListAsync().ConfigureAwait(false);

            var puestosConAFT = await _context.ActFijos
                .Where(a => a.FechaSubmayor == fecha && a.PuestoTrabajo != null)
                .GroupBy(a => a.PuestoTrabajo)
                .Select(g => new { Id = g.Key, Contador = g.Count() })
                .ToListAsync().ConfigureAwait(false);

            foreach (var item in localesConAFT)
                resultado.Add(new mdArbolUbicacion { Id = item.Id.Value, Tipo = "local", Contador = item.Contador });

            foreach (var item in puestosConAFT)
                resultado.Add(new mdArbolUbicacion { Id = item.Id.Value, Tipo = "puesto", Contador = item.Contador });

            int sinUbicacion = await _context.ActFijos
                .CountAsync(a => a.FechaSubmayor == fecha && a.Local == null && a.PuestoTrabajo == null).ConfigureAwait(false);

            resultado.Add(new mdArbolUbicacion { Id = Guid.Empty, Tipo = "sinUbicacion", Contador = sinUbicacion });

            return resultado;
        }

        public async Task<clResponse<bool>> QuitarUbicacion(long noInventario, string usuario)
        {
            DateTime fecha = await ObtenerUltimoSubmayor();
            var aft = await _context.ActFijos
                .FirstOrDefaultAsync(a => a.NoInventario == noInventario && a.FechaSubmayor == fecha).ConfigureAwait(false);
            if (aft == null)
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = $"El Activo Fijo {noInventario} no existe en el último submayor." };

            Guid? localOrigen = aft.Local;
            Guid? puestoOrigen = aft.PuestoTrabajo;

            if (localOrigen == null && puestoOrigen == null)
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = $"El Activo Fijo {noInventario} no tiene ubicación asignada." };

            aft.Local = null;
            aft.PuestoTrabajo = null;

            var historial = new HistorialUbicacionAFT
            {
                Id = Guid.NewGuid(),
                NoInventario = aft.NoInventario,
                SubNumero = aft.SubNumero,
                FechaSubmayor = aft.FechaSubmayor,
                FechaMovimiento = DateTime.Now,
                LocalOrigen = localOrigen,
                PuestoOrigen = puestoOrigen,
                LocalDestino = null,
                PuestoDestino = null,
                UsuarioRegistra = usuario
            };

            _context.Add(historial);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return new clResponse<bool> { SiSatisfactorio = true, Mensaje = $"La ubicación del Activo Fijo {noInventario} ha sido eliminada satisfactoriamente." };
        }

        public async Task<List<mdHistorialUbicacion>> ObtenerHistorial(long noInventario)
        {
            return await _context.HistorialUbicacionAFTs
                .Where(h => h.NoInventario == noInventario)
                .OrderByDescending(h => h.FechaMovimiento)
                .Select(h => new mdHistorialUbicacion
                {
                    noInventario = h.NoInventario,
                    descripcion = h.ActFijoNavigation != null ? h.ActFijoNavigation.Descripcion : "",
                    fechaMovimiento = h.FechaMovimiento,
                    localOrigen = h.LocalOrigenNavigation != null ? h.LocalOrigenNavigation.Nombre : "",
                    puestoOrigen = h.PuestoOrigenNavigation != null ? h.PuestoOrigenNavigation.Nombre : "",
                    localDestino = h.LocalDestinoNavigation != null ? h.LocalDestinoNavigation.Nombre : "",
                    puestoDestino = h.PuestoDestinoNavigation != null ? h.PuestoDestinoNavigation.Nombre : "",
                    usuarioRegistra = h.UsuarioRegistra ?? ""
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<clResponse<bool>> AsignarUbicacion(long noInventario, Guid? local, Guid? puesto, string usuario)
        {
            if (local == null && puesto == null)
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = "Debe indicar un Local o un Puesto de Trabajo como destino." };
            if (local != null && puesto != null)
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = "Un AFT no puede estar asociado a un Local y a un Puesto a la vez." };

            DateTime fecha = await ObtenerUltimoSubmayor();
            var aft = await _context.ActFijos
                .FirstOrDefaultAsync(a => a.NoInventario == noInventario && a.FechaSubmayor == fecha).ConfigureAwait(false);
            if (aft == null)
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = $"El Activo Fijo {noInventario} no existe en el último submayor." };

            if (local != null && !await _context.Locales.AnyAsync(l => l.IdLocal == local)) 
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = "El Local seleccionado no existe." };
            if (puesto != null && !await _context.PuestosTrabajo.AnyAsync(p => p.IdPuesto == puesto))
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = "El Puesto de Trabajo seleccionado no existe." };

            Guid? localOrigen = aft.Local;
            Guid? puestoOrigen = aft.PuestoTrabajo;

            if (localOrigen == local && puestoOrigen == puesto)
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = "El AFT ya se encuentra asignado en la ubicación seleccionada." };

            aft.Local = local;
            aft.PuestoTrabajo = puesto;

            var historial = new HistorialUbicacionAFT
            {
                Id = Guid.NewGuid(),
                NoInventario = aft.NoInventario,
                SubNumero = aft.SubNumero,
                FechaSubmayor = aft.FechaSubmayor,
                FechaMovimiento = DateTime.Now,
                LocalOrigen = localOrigen,
                PuestoOrigen = puestoOrigen,
                LocalDestino = local,
                PuestoDestino = puesto,
                UsuarioRegistra = usuario
            };

            _context.Add(historial);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return new clResponse<bool> { SiSatisfactorio = true, Mensaje = $"El Activo Fijo {noInventario} ha sido asignado satisfactoriamente." };
        }
    }
}