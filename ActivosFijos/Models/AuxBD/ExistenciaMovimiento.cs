namespace ActivosFijos.Models.AuxBD
{
    public class ExistenciaMovimiento
    {
        public long NoInventario { get; set; }
        public int Cantidad { get; set; }
    }

    public class ExistenciaMovimientoUTH
    {
        public Guid IdUTH { get; set; }
        public int Cantidad { get; set; }
    }
}
