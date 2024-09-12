namespace ActivosFijos.Models.AuxBD
{
    public class UTHConMovimientos
    {
        public string Discriminante { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public Guid idResponsable { get; set; }
        public string Responsable { get; set; } = string.Empty;
        public Guid idCentroCosto { get; set; }
        public string CentroCosto { get; set; } = string.Empty;
        public Guid idArea { get; set; }
        public string Area { get; set; } = string.Empty;
        public int CantidadAnterior { get; set; }
        public int Cantidad { get; set; }
    }

    public class UTHConMovimientosParcial
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public Guid Responsable { get; set; }
        public Guid CentroCosto { get; set; }
        public Guid Area { get; set; }
        public string Discriminante { get; set; } = string.Empty;
        public int CantidadAnterior { get; set; }
        public int Cantidad { get; set; }
    }
}
