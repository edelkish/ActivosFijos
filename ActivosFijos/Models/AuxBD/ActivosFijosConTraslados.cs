namespace ActivosFijos.Models.AuxBD
{
    public class ActivosFijosConTraslados
    {
        public long NoInventario { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public Guid idArea { get; set; }
        public string Area { get; set; } = string.Empty;
        public Guid idResponsable { get; set; }
        public string Responsable { get; set; } = string.Empty;
        public DateTime desde { get; set; }
        public DateTime? hasta { get; set; }
        public string? motivo { get; set; } = string.Empty;
    }
}
