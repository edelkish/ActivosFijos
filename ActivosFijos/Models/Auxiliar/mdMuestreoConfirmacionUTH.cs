using System;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdMuestreoConfirmacionUTH
    {
        public int Anno { get; set; }
        public int Mes { get; set; }
        public Guid IdUth { get; set; }
        public string Codigo { get; set; } = null!;
        public Guid? Responsable { get; set; }
        public Guid? CentroCosto { get; set; }
        public Guid AreaResponsabilidad { get; set; }
        public bool SiExiste { get; set; }
        public string? Observaciones { get; set; }
    }
}
