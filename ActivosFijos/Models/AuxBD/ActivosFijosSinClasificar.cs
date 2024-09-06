namespace ActivosFijos.Models.AuxBD
{
    public class ActivosFijosSinClasificar
    {
        public Int64 noInventario { get; set; }
        public string? descripcion { get; set; }
        public Guid? centroCosto { get; set; }
        public string nombre { get; set; }
        public Int64? numero { get; set; }
    }

    public class ActivosFijosClasificados
    {
        public Int64 noInventario { get; set; }
        public string? descripcion { get; set; }
        public Guid? centroCosto { get; set; }
        public string nombre { get; set; }
        public Int64? numero { get; set; }
        public string clasificador { get; set; }
    }

    public class ClasificadoresTotales
    {
        public string clasificador { get; set; }
        public Int32 cantidad { get; set; }
    }
}
