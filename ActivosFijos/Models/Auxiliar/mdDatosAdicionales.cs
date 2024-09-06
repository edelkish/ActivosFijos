namespace ActivosFijos.Models.Auxiliar
{
    public class mdDatosAdicionales
    {
        public string Entidad { get; set; } = string.Empty;
        public double vAdquisicion { get; set; }
        public double vContable { get; set; }
        public double dAcumulada { get; set; }
        public double vResidual { get; set; }
        public DateTime fGeneracion { get; set; }
        public int TotalProductos { get; set; }
        public int TotalAreas { get; set; }
        public List<mdDatosAdicionalesAreasResponsabilidad> Areas { get; set; } = new List<mdDatosAdicionalesAreasResponsabilidad>();
    }

    public class mdDatosAdicionalesAreasResponsabilidad
    {
        public int TotalProductos { get; set; }
        public string AreaResponsabilidad { get; set; } = String.Empty;
        public double vAdquisicion { get; set; }
        public double vContable { get; set; }
        public double dAcumulada { get; set; }
        public double vResidual { get; set; }
    }

    public class mdDatosAdicionalesUTH
    {
        public string Entidad { get; set; } = string.Empty;
        public double vAdquisicion { get; set; }
        public double vContable { get; set; }
        public DateTime fGeneracion { get; set; }
        public int TotalProductos { get; set; }
        public int TotalAreas { get; set; }
        public List<mdDatosAdicionalesAreasResponsabilidadUTH> Areas { get; set; } = new List<mdDatosAdicionalesAreasResponsabilidadUTH>();
    }

    public class mdDatosAdicionalesAreasResponsabilidadUTH
    {
        public int TotalProductos { get; set; }
        public string AreaResponsabilidad { get; set; } = String.Empty;
        public double vAdquisicion { get; set; }
        public double vContable { get; set; }
    }
}
