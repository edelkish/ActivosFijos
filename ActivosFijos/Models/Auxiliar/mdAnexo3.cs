namespace ActivosFijos.Models.Auxiliar
{
    public class mdAnexo3
    {
        public string TipoMovimiento { get; set; } = "Traslado Interno";
        public string Fundamentacion { get; set; } = string.Empty;
        public string InformeTecnico { get; set; } = string.Empty;
        public string NombreTecnico { get; set; } = string.Empty;
        public string CargoTecnico { get; set; } = string.Empty;
        public string Comprobante { get; set; } = string.Empty;
        public string Entidad { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string UEB { get; set; } = string.Empty;
        public string DireccionUEB { get; set; } = string.Empty;
        public string HechoPor { get; set; } = string.Empty;
        public string AprobadoPor { get; set; } = string.Empty;
        public string AutorizadoPor { get; set; } = string.Empty;
        public string CargoAprobadoPor { get; set; } = string.Empty;
        public string CargoAutorizadoPor { get; set; } = string.Empty;
        public string TransportadorReceptor { get; set; } = string.Empty;
        public string CargoTransportadorReceptor { get; set; } = string.Empty;

        /// <summary>
        /// Nombre y area de quien recepciona, elegidos en el panel. Si vienen vacios el
        /// generador usa los que calculo el submayor (ResponsableActual / AreaActual).
        /// </summary>
        public string ReceptorNombre { get; set; } = string.Empty;
        public string ReceptorArea { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fila del reverso cuando varios activos comparten un solo Anexo 3.
    /// </summary>
    public class mdAnexo3Detalle
    {
        public string NoInventario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }
}