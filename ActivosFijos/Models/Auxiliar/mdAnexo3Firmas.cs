using System;

namespace ActivosFijos.Models.Auxiliar
{
    /// <summary>
    /// Valores de las firmas de un Anexo 3. Se persisten por movimiento
    /// en la tabla Anexo3Firmas, con clave (NoInventario, SubNumero, FechaSubmayor).
    /// </summary>
    public class mdAnexo3Firmas
    {
        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
        public string Fundamentacion { get; set; } = string.Empty;
        public string InformeTecnico { get; set; } = string.Empty;
        public string NombreTecnico { get; set; } = string.Empty;
        public string CargoTecnico { get; set; } = string.Empty;
        public string HechoPor { get; set; } = string.Empty;
        public string CargoHechoPor { get; set; } = string.Empty;
        public string TransportadorReceptor { get; set; } = string.Empty;
        public string CargoTransportadorReceptor { get; set; } = string.Empty;
        public string Comprobante { get; set; } = string.Empty;

        /// <summary>
        /// Direccion del receptor, solo en traslado externo y enviado a reparar.
        /// </summary>
        public string DireccionReceptor { get; set; } = string.Empty;

        /// <summary>
        /// Nombre y area de quien recepciona. En traslado interno el panel los deriva del
        /// ResponsableActual; en traslado externo y enviado a reparar se capturan a mano.
        /// </summary>
        public string ReceptorNombre { get; set; } = string.Empty;
        public string ReceptorArea { get; set; } = string.Empty;
    }

    /// <summary>
    /// Movimiento del mes con sus firmas ya cargadas, para pintar el panel lateral.
    /// </summary>
    public class mdAnexo3Movimiento
    {
        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string AreaAnterior { get; set; } = string.Empty;
        public string AreaActual { get; set; } = string.Empty;
        public string ResponsableAnterior { get; set; } = string.Empty;
        public string ResponsableActual { get; set; } = string.Empty;
        public bool SiFirmasCargadas { get; set; }
        public string TipoDeclarado { get; set; } = string.Empty;
        public mdAnexo3Firmas Firmas { get; set; } = new mdAnexo3Firmas();
    }

    /// <summary>
    /// Clave de un movimiento, para pedir que se generen solo algunos del lote.
    /// </summary>
    public class mdClaveMovimiento
    {
        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
    }

    /// <summary>
    /// Responsable con su cargo asociado. El cargo se deriva de aqui: si viene vacio,
    /// el panel deja elegirlo de la lista de cargos. El area viene junto al nombre porque
    /// es la que se preselecciona como area del Receptor.
    /// </summary>
    public class mdResponsableCargo
    {
        public string Nombre { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
    }
}