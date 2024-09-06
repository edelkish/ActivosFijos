using System.ComponentModel.DataAnnotations;

namespace ActivosFijos.Models.Vistas
{
    public class mdVExcellDataImport
    {
        [Required]
        public IFormFile MyExcelFile { get; set; }

        public string? MSExcelTable { get; set; }
    }
}
