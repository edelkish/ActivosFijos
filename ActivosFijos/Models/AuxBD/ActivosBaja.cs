namespace ActivosFijos.Models.AuxBD
{
    public class ActivosBaja
    {
        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public Guid? ResponsableId { get; set; }
        public string Responsable { get; set; } = string.Empty;
        public Guid? AreaId { get; set; }
        public string Area { get; set; } = string.Empty;
        public decimal? ValorContable { get; set; }
        public decimal? ValorAdquisicion { get; set; }
        public DateTime FechaSubmayor { get; set; }
    }
}
