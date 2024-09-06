using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivosFijos.Models.Vistas
{
    public class mdVResponsable
    {
        [StringLength(50)]
        public string? Nombre { get; set; }
        [StringLength(100)]
        public string? Apellidos { get; set; }
        public Guid? Cargo { get; set; }
        public Guid IdResponsable { get; set; }
        public string[] IdAreasResponsables { get; set; }
    }
}
