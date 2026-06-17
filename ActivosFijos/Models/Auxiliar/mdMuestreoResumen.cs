using System;
using System.Collections.Generic;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdMuestreoResumenArea
    {
        public Guid? AreaResponsabilidad { get; set; }
        public string? AreaNombre { get; set; }
        public int Cantidad { get; set; }
        public string? Inventarios { get; set; }
    }
}
