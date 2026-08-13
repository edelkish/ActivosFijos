using System;
using System.Collections.Generic;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdMuestreoPendienteUTH
    {
        public int Anno { get; set; }
        public int Mes { get; set; }
        public List<mdMuestreoPendienteItemUTH> Items { get; set; } = new();
    }

    public class mdMuestreoPendienteItemUTH
    {
        public Guid IdUth { get; set; }
        public string Codigo { get; set; } = null!;
        public string? Descripcion { get; set; }
        public Guid? Responsable { get; set; }
        public string? NombreResponsable { get; set; }
        public string? ApellidosResponsable { get; set; }
        public Guid? CentroCosto { get; set; }
        public string? NombreCentroCosto { get; set; }
        public Guid AreaResponsabilidad { get; set; }
        public string? AreaNombre { get; set; }
        public decimal? Valor { get; set; }
        public bool SiExiste { get; set; }
        public string? Observaciones { get; set; }
    }
}
