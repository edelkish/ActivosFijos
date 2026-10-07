using ActivosFijos.Clases;
using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoAnexo3Firmas
    {
        private aftContext _context;
        public repoAnexo3Firmas(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Guarda o actualiza las firmas de los movimientos indicados. La clave es
        /// (NoInventario, SubNumero, FechaSubmayor), asi que volver a cargar el mismo
        /// mes actualiza en lugar de duplicar.
        /// </summary>
        public async Task<clResponse<int>> Upsert(DateTime fechaSubmayor, List<mdAnexo3Firmas> firmas)
        {
            if (firmas == null)
                return new clResponse<int> { SiSatisfactorio = false, Mensaje = "No se recibieron firmas para guardar." };

            try
            {
                List<long> inventarios = firmas.Select(f => f.NoInventario).Distinct().ToList();
                List<Anexo3Firmas> existentes = await _context.Anexo3Firmas
                    .Where(f => f.FechaSubmayor == fechaSubmayor && inventarios.Contains(f.NoInventario))
                    .ToListAsync().ConfigureAwait(false);

                foreach (var firma in firmas)
                {
                    Anexo3Firmas entidad = existentes.FirstOrDefault(e =>
                        e.NoInventario == firma.NoInventario && e.SubNumero == firma.SubNumero);

                    if (entidad == null)
                    {
                        entidad = new Anexo3Firmas
                        {
                            NoInventario = firma.NoInventario,
                            SubNumero = firma.SubNumero,
                            FechaSubmayor = fechaSubmayor
                        };
                        _context.Anexo3Firmas.Add(entidad);
                        existentes.Add(entidad);
                    }

                    entidad.TipoMovimiento = Normalizar(firma.TipoMovimiento);
                    entidad.Fundamentacion = Normalizar(firma.Fundamentacion);
                    entidad.InformeTecnico = Normalizar(firma.InformeTecnico);
                    entidad.NombreTecnico = Normalizar(firma.NombreTecnico);
                    entidad.CargoTecnico = Normalizar(firma.CargoTecnico);
                    entidad.HechoPor = Normalizar(firma.HechoPor);
                    entidad.CargoHechoPor = Normalizar(firma.CargoHechoPor);
                    entidad.TransportadorReceptor = Normalizar(firma.TransportadorReceptor);
                    entidad.CargoTransportadorReceptor = Normalizar(firma.CargoTransportadorReceptor);
                    entidad.Comprobante = Normalizar(firma.Comprobante);
                    entidad.DireccionReceptor = Normalizar(firma.DireccionReceptor);
                    entidad.ReceptorNombre = Normalizar(firma.ReceptorNombre);
                    entidad.ReceptorArea = Normalizar(firma.ReceptorArea);
                    entidad.FechaRegistro = DateTime.Now;
                }

                await _context.SaveChangesAsync().ConfigureAwait(false);
                return new clResponse<int> { SiSatisfactorio = true, Mensaje = "Se guardaron las firmas correctamente.", Resultado = firmas.Count };
            }
            catch (Exception ex)
            {
                return new clResponse<int> { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador." };
            }
        }

        public async Task<List<Anexo3Firmas>> ObtenerPorFecha(DateTime fechaSubmayor)
        {
            return await _context.Anexo3Firmas
                .Where(f => f.FechaSubmayor == fechaSubmayor)
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<Anexo3Firmas?> ObtenerPorClave(long noInventario, int subNumero, DateTime fechaSubmayor)
        {
            return await _context.Anexo3Firmas
                .FirstOrDefaultAsync(f => f.NoInventario == noInventario && f.SubNumero == subNumero && f.FechaSubmayor == fechaSubmayor)
                .ConfigureAwait(false);
        }

        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}