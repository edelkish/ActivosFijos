using System;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdArbolUbicacion
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public Guid? IdPadre { get; set; }
        public int Contador { get; set; }
        public bool TieneHijos { get; set; }
    }

    public class mdAFTUbicacion
    {
        public long noInventario { get; set; }
        public int subNumero { get; set; }
        public DateTime fechaSubmayor { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string clasificador { get; set; } = string.Empty;
        public string localActual { get; set; } = string.Empty;
        public string puestoActual { get; set; } = string.Empty;
    }

    public class mdHistorialUbicacion
    {
        public long noInventario { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public DateTime fechaMovimiento { get; set; }
        public string localOrigen { get; set; } = string.Empty;
        public string puestoOrigen { get; set; } = string.Empty;
        public string localDestino { get; set; } = string.Empty;
        public string puestoDestino { get; set; } = string.Empty;
        public string usuarioRegistra { get; set; } = string.Empty;
    }

    public class mdHierarchySource
    {
        public Guid? idFuente { get; set; }
        public string tipoFuente { get; set; } = string.Empty;
        public string nombreFuente { get; set; } = string.Empty;
        public Guid? idDestino { get; set; }
        public string tipoDestino { get; set; } = string.Empty;
        public string nombreDestino { get; set; } = string.Empty;
    }
}