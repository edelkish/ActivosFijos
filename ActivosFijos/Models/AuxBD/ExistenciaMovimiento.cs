namespace ActivosFijos.Models.AuxBD
{
    public class ExistenciaMovimiento
    {
        public long NoInventario { get; set; }
        public int Cantidad { get; set; }
    }

    public class ExistenciaMovimientoUTH
    {
        public string Codigo { get; set; } = string.Empty;
        public string Discriminante { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
