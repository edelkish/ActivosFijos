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

        public async Task<List<mdArbolUbicacion>> ObtenerCentroCostosArbol()
        {
            return await _context.CentroCostos
                .Where(c => c.Locales.Any())
                .OrderBy(c => c.Nombre)
                .Select(c => new mdArbolUbicacion
                {
                    Id = c.IdCentroCosto,
                    Tipo = "centroCosto",
                    Nombre = c.Nombre ?? "",
                    Descripcion = c.Locales.Count().ToString(),
                    IdPadre = null,
                    TieneHijos = true
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<mdArbolUbicacion>> ObtenerLocalesDeCentroCosto(Guid idCentroCosto)
        {
            return await _context.Locales
                .Where(l => l.CentroCosto == idCentroCosto)
                .OrderBy(l => l.Nombre)
                .Select(l => new mdArbolUbicacion
                {
                    Id = l.IdLocal,
                    Tipo = "local",
                    Nombre = l.Nombre,
                    Descripcion = l.Descripcion ?? "",
                    IdPadre = l.CentroCosto,
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
                .Where(a => a.FechaSubmayor == fecha && a.Local == idLocal && a.PuestoTrabajo == null)
                .OrderBy(a => a.NoInventario)
                .Select(a => new mdAFTUbicacion
                {
                    noInventario = a.NoInventario,
                    subNumero = a.SubNumero,
                    fechaSubmayor = a.FechaSubmayor,
                    descripcion = a.Descripcion ?? "",
                    clasificador = a.ClasificadorNavigation != null ? a.ClasificadorNavigation.Nombre : "",
                    localActual = a.LocalNavigation != null ? a.LocalNavigation.Nombre : "",
                    puestoActual = "",
                    responsable = a.ResponsableNavigation != null ? (a.ResponsableNavigation.Nombre + " " + a.ResponsableNavigation.Apellidos) : "",
                    centroCostoActivo = a.CentroCostoNavigation != null ? a.CentroCostoNavigation.Nombre ?? "" : "",
                    area = a.AreaResponsabilidadNavigation != null ? a.AreaResponsabilidadNavigation.Nombre ?? "" : ""
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<mdAFTUbicacion>> ObtenerAFTsPorCentroCosto(Guid idCentroCosto)
        {
            DateTime fecha = await ObtenerUltimoSubmayor();
            return await _context.ActFijos
                .Where(a => a.FechaSubmayor == fecha && a.LocalNavigation != null && a.LocalNavigation.CentroCosto == idCentroCosto)
                .OrderBy(a => a.NoInventario)
                .Select(a => new mdAFTUbicacion
                {
                    noInventario = a.NoInventario,
                    subNumero = a.SubNumero,
                    fechaSubmayor = a.FechaSubmayor,
                    descripcion = a.Descripcion ?? "",
                    clasificador = a.ClasificadorNavigation != null ? a.ClasificadorNavigation.Nombre : "",
                    localActual = a.LocalNavigation != null ? a.LocalNavigation.Nombre : "",
                    puestoActual = a.PuestoTrabajoNavigation != null ? a.PuestoTrabajoNavigation.Nombre : "",
                    responsable = a.ResponsableNavigation != null ? (a.ResponsableNavigation.Nombre + " " + a.ResponsableNavigation.Apellidos) : "",
                    centroCostoActivo = a.CentroCostoNavigation != null ? a.CentroCostoNavigation.Nombre ?? "" : "",
                    area = a.AreaResponsabilidadNavigation != null ? a.AreaResponsabilidadNavigation.Nombre ?? "" : ""
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
                    puestoActual = a.PuestoTrabajoNavigation != null ? a.PuestoTrabajoNavigation.Nombre : "",
                    responsable = a.ResponsableNavigation != null ? (a.ResponsableNavigation.Nombre + " " + a.ResponsableNavigation.Apellidos) : "",
                    centroCostoActivo = a.CentroCostoNavigation != null ? a.CentroCostoNavigation.Nombre ?? "" : "",
                    area = a.AreaResponsabilidadNavigation != null ? a.AreaResponsabilidadNavigation.Nombre ?? "" : ""
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
                    puestoActual = "",
                    responsable = a.ResponsableNavigation != null ? (a.ResponsableNavigation.Nombre + " " + a.ResponsableNavigation.Apellidos) : "",
                    centroCostoActivo = a.CentroCostoNavigation != null ? a.CentroCostoNavigation.Nombre ?? "" : "",
                    area = a.AreaResponsabilidadNavigation != null ? a.AreaResponsabilidadNavigation.Nombre ?? "" : ""
                })
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<mdArbolUbicacion>> ObtenerContadoresGeneral()
        {
            DateTime fecha = await ObtenerUltimoSubmayor();
            var resultado = new List<mdArbolUbicacion>();

            var centrosCostoConAFT = await _context.ActFijos
                .Where(a => a.FechaSubmayor == fecha && a.LocalNavigation != null)
                .GroupBy(a => a.LocalNavigation!.CentroCosto)
                .Select(g => new { Id = g.Key, Contador = g.Count() })
                .ToListAsync().ConfigureAwait(false);

            var localesConAFT = await _context.ActFijos
                .Where(a => a.FechaSubmayor == fecha && a.Local != null && a.PuestoTrabajo == null)
                .GroupBy(a => a.Local)
                .Select(g => new { Id = g.Key, Contador = g.Count() })
                .ToListAsync().ConfigureAwait(false);

            var puestosConAFT = await _context.ActFijos
                .Where(a => a.FechaSubmayor == fecha && a.PuestoTrabajo != null)
                .GroupBy(a => a.PuestoTrabajo)
                .Select(g => new { Id = g.Key, Contador = g.Count() })
                .ToListAsync().ConfigureAwait(false);

            foreach (var item in centrosCostoConAFT)
                resultado.Add(new mdArbolUbicacion { Id = item.Id, Tipo = "centroCosto", Contador = item.Contador });

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

            DateTime fecha = await ObtenerUltimoSubmayor();
            var aft = await _context.ActFijos
                .FirstOrDefaultAsync(a => a.NoInventario == noInventario && a.FechaSubmayor == fecha).ConfigureAwait(false);
            if (aft == null)
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = $"El Activo Fijo {noInventario} no existe en el último submayor." };

            Guid? localDestino;
            Guid? puestoDestino;

            if (puesto != null)
            {
                var localDelPuesto = await _context.PuestosTrabajo
                    .Where(p => p.IdPuesto == puesto)
                    .Select(p => (Guid?)p.IdLocal)
                    .FirstOrDefaultAsync().ConfigureAwait(false);
                if (localDelPuesto == null)
                    return new clResponse<bool> { SiSatisfactorio = false, Mensaje = "El Puesto de Trabajo seleccionado no existe." };

                puestoDestino = puesto;
                localDestino = localDelPuesto.Value;
            }
            else
            {
                if (!await _context.Locales.AnyAsync(l => l.IdLocal == local).ConfigureAwait(false))
                    return new clResponse<bool> { SiSatisfactorio = false, Mensaje = "El Local seleccionado no existe." };

                localDestino = local;
                puestoDestino = null;
            }

            Guid? localOrigen = aft.Local;
            Guid? puestoOrigen = aft.PuestoTrabajo;

            if (localOrigen == localDestino && puestoOrigen == puestoDestino)
                return new clResponse<bool> { SiSatisfactorio = false, Mensaje = "El AFT ya se encuentra asignado en la ubicación seleccionada." };

            aft.Local = localDestino;
            aft.PuestoTrabajo = puestoDestino;

            var historial = new HistorialUbicacionAFT
            {
                Id = Guid.NewGuid(),
                NoInventario = aft.NoInventario,
                SubNumero = aft.SubNumero,
                FechaSubmayor = aft.FechaSubmayor,
                FechaMovimiento = DateTime.Now,
                LocalOrigen = localOrigen,
                PuestoOrigen = puestoOrigen,
                LocalDestino = localDestino,
                PuestoDestino = puestoDestino,
                UsuarioRegistra = usuario
            };

            _context.Add(historial);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return new clResponse<bool> { SiSatisfactorio = true, Mensaje = $"El Activo Fijo {noInventario} ha sido asignado satisfactoriamente." };
        }
    }
}