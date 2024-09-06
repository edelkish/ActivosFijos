namespace ActivosFijos.Models.AuxBD
{
    public class ActivosFijosMovimientos
    {
        public long noInventario { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public Guid responsable { get; set; }
        public string nombreResponsable { get; set; } = string.Empty;
        public Guid areaResponsabilidad { get; set; }
        public string nombreAreaResponsabilidad { get; set; } = string.Empty;
        public DateTime fechaSubmayor { get; set; }
    }

    public class ActivosFijosMovimientosResponsables
    {
        public Guid responsable { get; set; }
        public string nombreResponsable { get; set; } = string.Empty;
        public Guid areaResponsabilidad { get; set; }
        public string nombreAreaResponsabilidad { get; set; } = string.Empty;
        public DateTime fechaSubmayor { get; set; }
    }
}
