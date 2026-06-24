using System;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdMuestreoConfirmacion
    {
        public int Anno { get; set; }
        public int Mes { get; set; }
        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
        public DateTime FechaSubmayor { get; set; }
        public bool SiExiste { get; set; }
        public string? Observaciones { get; set; }
    }
}
