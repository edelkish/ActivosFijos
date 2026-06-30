using System;
using System.Collections.Generic;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdPlanMuestreoAnual
    {
        public int Anio { get; set; }
        public bool EstaGuardado { get; set; }
        public int TotalActivosBase { get; set; }
        public int DiezPorCientoEsperado { get; set; }
        public bool HayAreasSinCentroCosto { get; set; }
        public List<string>? AreasSinCentroCosto { get; set; }
        public List<mdPlanMuestreoAnualItem> Items { get; set; } = new();
        public mdPlanMuestreoAnualItem? Totales { get; set; }
        public List<mdPlanMuestreoArea>? Areas { get; set; }
    }

    public class mdPlanMuestreoAnualItem
    {
        public Guid CentroCosto { get; set; }
        public string? NumeroCentroCosto { get; set; }
        public string? NombreCentroCosto { get; set; }
        public Guid AreaResponsabilidad { get; set; }
        public string? NombreArea { get; set; }
        public int TotalActivos { get; set; }
        public int Ene { get; set; }
        public int Feb { get; set; }
        public int Mar { get; set; }
        public int Abr { get; set; }
        public int May { get; set; }
        public int Jun { get; set; }
        public int Jul { get; set; }
        public int Ago { get; set; }
        public int Sep { get; set; }
        public int Oct { get; set; }
        public int Nov { get; set; }
        public int Dic { get; set; }
    }

    public class mdPlanMuestreoArea
    {
        public Guid AreaId { get; set; }
        public string? NombreArea { get; set; }
        public int CantidadCentros { get; set; }
        public bool Seleccionado { get; set; } = true;
    }
}
