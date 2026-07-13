using System;
using System.Collections.Generic;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdMuestreoPendiente
    {
        public int Anno { get; set; }
        public int Mes { get; set; }
        public List<mdMuestreoPendienteItem> Items { get; set; } = new();
    }

    public class mdMuestreoPendienteItem
    {
        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
        public DateTime FechaSubmayor { get; set; }
        public string? Descripcion { get; set; }
        public Guid? CentroCosto { get; set; }
        public string? NombreCentroCosto { get; set; }
        public Guid? AreaResponsabilidad { get; set; }
        public string? AreaNombre { get; set; }
        public string? NombreResponsable { get; set; }
        public string? ApellidosResponsable { get; set; }
        public bool SiExiste { get; set; }
        public string? Observaciones { get; set; }
    }
}
