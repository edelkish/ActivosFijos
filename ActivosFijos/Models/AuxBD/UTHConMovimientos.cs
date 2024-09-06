namespace ActivosFijos.Models.AuxBD
{
    public class UTHConMovimientos
    {
        public long NoInventario { get; set; }
        public int Cantidad { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public Guid idAreaAnterior { get; set; }
        public string AreaAnterior { get; set; } = string.Empty;
        public Guid idAreaActual { get; set; }
        public string AreaActual { get; set; } = string.Empty;
        public Guid idResponsableAnterior { get; set; }
        public string ResponsableAnterior { get; set; } = string.Empty;
        public Guid idResponsableActual { get; set; }
        public string ResponsableActual { get; set; } = string.Empty;
    }
}
