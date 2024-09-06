using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdSubmayorErrores
    {
        public Guid Responsable { get; set; }
        public Guid Area { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Today;
        public List<SelectListItem> Responsables { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
        public bool VisualizarErrores { get; set; } = false;
    }

    public class mdExportarAFTClasificados
    {
        public Guid CentroCosto { get; set; }
        public Guid Clasificador { get; set; }
        public List<SelectListItem> CentrosDeCostos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Clasificadores { get; set; } = new List<SelectListItem>();
        public bool VisualizarErrores { get; set; } = false;
    }
}
