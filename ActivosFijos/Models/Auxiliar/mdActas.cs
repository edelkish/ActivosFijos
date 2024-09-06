using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdActas
    {
        public Guid Responsable { get; set; }        
        public Guid Area { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Today;
        public string ResponsableArea { get; set; } = String.Empty;
        public List<SelectListItem> Responsables { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
        public bool VisualizarMovimiento { get; set; } = false;
    }
}
