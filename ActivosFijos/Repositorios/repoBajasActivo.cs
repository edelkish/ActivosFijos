using ActivosFijos.Clases;
using ActivosFijos.Models.AuxBD;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoBajasActivo
    {
        private readonly aftContext _context;
        private readonly aftContextAux _contextAux;

        public repoBajasActivo(aftContext context, aftContextAux contextAux)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAux = contextAux ?? throw new ArgumentNullException(nameof(contextAux));
        }

        public async Task<List<MotivoBaja>> ObtenerMotivosBaja()
        {
            return await _context.MotivoBajas.ToListAsync();
        }

        public async Task<List<ActivosBaja>> ObtenerActivosBajaPeriodo(DateTime fechaOrigen, DateTime fechaDestino)
        {
            string fOrigen = fechaOrigen.ToString("yyyy-MM-dd");
            string fDestino = fechaDestino.ToString("yyyy-MM-dd");

            return await _contextAux.ActivosBajas.FromSqlRaw($@"
                SELECT
                    origen.NoInventario,
                    origen.SubNumero,
                    origen.Descripcion,
                    origen.Responsable AS ResponsableId,
                    ISNULL(R.Nombre + ' ' + R.Apellidos, '<<S/R>>') AS Responsable,
                    origen.AreaResponsabilidad AS AreaId,
                    ISNULL(A.Nombre, '') AS Area,
                    origen.ValorContable,
                    origen.ValorAdquisicion,
                    origen.FechaSubmayor
                FROM dbo.ActFijo origen
                LEFT JOIN dbo.Responsable R ON R.idResponsable = origen.responsable
                LEFT JOIN dbo.AreaDeResponsabilidad A ON A.idArea = origen.areaResponsabilidad
                WHERE YEAR(origen.fechaSubmayor) = YEAR('{fOrigen}')
                  AND MONTH(origen.fechaSubmayor) = MONTH('{fOrigen}')
                  AND NOT EXISTS (
                      SELECT 1
                      FROM dbo.ActFijo destino
                      WHERE destino.NoInventario = origen.NoInventario
                        AND destino.SubNumero = origen.SubNumero
                        AND YEAR(destino.fechaSubmayor) = YEAR('{fDestino}')
                        AND MONTH(destino.fechaSubmayor) = MONTH('{fDestino}')
                  )
                ORDER BY origen.NoInventario
            ").ToListAsync();
        }

        public async Task<DateTime?> ObtenerUltimaFechaSubmayorPorAnio(int anio)
        {
            var resultado = await _contextAux.ActivosBajas.FromSqlRaw($@"
                SELECT TOP 1
                    origen.NoInventario,
                    origen.SubNumero,
                    origen.Descripcion,
                    origen.Responsable AS ResponsableId,
                    ISNULL(R.Nombre + ' ' + R.Apellidos, '<<S/R>>') AS Responsable,
                    origen.AreaResponsabilidad AS AreaId,
                    ISNULL(A.Nombre, '') AS Area,
                    origen.ValorContable,
                    origen.ValorAdquisicion,
                    origen.fechaSubmayor
                FROM dbo.ActFijo origen
                LEFT JOIN dbo.Responsable R ON R.idResponsable = origen.responsable
                LEFT JOIN dbo.AreaDeResponsabilidad A ON A.idArea = origen.areaResponsabilidad
                WHERE YEAR(origen.fechaSubmayor) = {anio}
                ORDER BY origen.fechaSubmayor DESC
            ").ToListAsync();

            return resultado.FirstOrDefault()?.FechaSubmayor;
        }

        public async Task<List<ActivosBaja>> ObtenerActivosBajaYaRegistrados(DateTime fechaOrigen, DateTime fechaDestino)
        {
            string fOrigen = fechaOrigen.ToString("yyyy-MM-dd");

            return await _contextAux.ActivosBajas.FromSqlRaw($@"
                SELECT
                    ba.NoInventario,
                    ba.SubNumero,
                    act.Descripcion,
                    act.responsable AS ResponsableId,
                    ISNULL(R.Nombre + ' ' + R.Apellidos, '<<S/R>>') AS Responsable,
                    act.areaResponsabilidad AS AreaId,
                    ISNULL(A.Nombre, '') AS Area,
                    act.ValorContable,
                    act.ValorAdquisicion,
                    ba.FechaSubmayor
                FROM dbo.BajaActivo ba
                INNER JOIN dbo.ActFijo act ON act.NoInventario = ba.NoInventario
                    AND act.SubNumero = ba.SubNumero
                    AND act.fechaSubmayor = ba.FechaSubmayor
                LEFT JOIN dbo.Responsable R ON R.idResponsable = act.responsable
                LEFT JOIN dbo.AreaDeResponsabilidad A ON A.idArea = act.areaResponsabilidad
                WHERE ba.FechaSubmayor = '{fOrigen}'
                ORDER BY ba.NoInventario
            ").ToListAsync();
        }

        public async Task<int?> ObtenerMotivoBajaExistente(long noInventario, int subNumero, DateTime fechaSubmayor)
        {
            var baja = await _context.BajaActivos
                .FirstOrDefaultAsync(b => b.NoInventario == noInventario
                    && b.SubNumero == subNumero
                    && b.FechaSubmayor.Year == fechaSubmayor.Year
                    && b.FechaSubmayor.Month == fechaSubmayor.Month);

            return baja?.MotivoBajaId;
        }

        public async Task<(int? MotivoBajaId, string? Observaciones)> ObtenerBajaExistente(long noInventario, int subNumero, DateTime fechaSubmayor)
        {
            var baja = await _context.BajaActivos
                .FirstOrDefaultAsync(b => b.NoInventario == noInventario
                    && b.SubNumero == subNumero
                    && b.FechaSubmayor.Year == fechaSubmayor.Year
                    && b.FechaSubmayor.Month == fechaSubmayor.Month);

            return (baja?.MotivoBajaId, baja?.Observaciones);
        }

        private static readonly HashSet<int> MotivosBaja = new() { 1, 3 };

        public async Task<clResponse<BajaActivo>> RegistrarBaja(BajaActivo baja)
        {
            try
            {
                baja.EsBaja = MotivosBaja.Contains(baja.MotivoBajaId);

                var existente = await _context.BajaActivos
                    .FirstOrDefaultAsync(b => b.NoInventario == baja.NoInventario
                        && b.SubNumero == baja.SubNumero
                        && b.FechaSubmayor == baja.FechaSubmayor);

                if (existente != null)
                {
                    existente.MotivoBajaId = baja.MotivoBajaId;
                    existente.Observaciones = baja.Observaciones;
                    existente.EsBaja = baja.EsBaja;
                    existente.FechaRegistro = DateTime.Now;
                }
                else
                {
                    _context.BajaActivos.Add(baja);
                }

                await _context.SaveChangesAsync();
                return new clResponse<BajaActivo> { SiSatisfactorio = true, Mensaje = "Baja registrada satisfactoriamente.", Resultado = baja };
            }
            catch (Exception ex)
            {
                return new clResponse<BajaActivo> { SiSatisfactorio = false, Mensaje = $"Error al registrar la baja: {ex.Message}" };
            }
        }

        public async Task<clResponse<BajaActivo>> RegistrarBajasMasivas(List<BajaActivo> bajas)
        {
            try
            {
                foreach (var baja in bajas)
                {
                    baja.EsBaja = MotivosBaja.Contains(baja.MotivoBajaId);

                    var existente = await _context.BajaActivos
                        .FirstOrDefaultAsync(b => b.NoInventario == baja.NoInventario
                            && b.SubNumero == baja.SubNumero
                            && b.FechaSubmayor == baja.FechaSubmayor);

                    if (existente != null)
                    {
                        existente.MotivoBajaId = baja.MotivoBajaId;
                        existente.Observaciones = baja.Observaciones;
                        existente.EsBaja = baja.EsBaja;
                        existente.FechaRegistro = DateTime.Now;
                    }
                    else
                    {
                        _context.BajaActivos.Add(baja);
                    }
                }

                await _context.SaveChangesAsync();
                return new clResponse<BajaActivo> { SiSatisfactorio = true, Mensaje = $"{bajas.Count} bajas registradas satisfactoriamente." };
            }
            catch (Exception ex)
            {
                return new clResponse<BajaActivo> { SiSatisfactorio = false, Mensaje = $"Error al registrar las bajas: {ex.Message}" };
            }
        }
    }
}
