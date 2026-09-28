namespace ActivosFijos.Models.AuxBD
{
    public class ActivosFijosAnexo3
    {
        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public Guid? idAreaAnterior { get; set; }
        public string? AreaAnterior { get; set; } = string.Empty;
        public Guid? idAreaActual { get; set; }
        public string? AreaActual { get; set; } = string.Empty;
        public Guid? idResponsableAnterior { get; set; }
        public string? ResponsableAnterior { get; set; } = string.Empty;
        public Guid? idResponsableActual { get; set; }
        public string? ResponsableActual { get; set; } = string.Empty;
        public decimal? ValorContable { get; set; }
        public decimal? ValorAdquisicion { get; set; }
        public decimal? DepreciacionAcum { get; set; }
        public DateTime FechaSubmayor { get; set; }
    }
}