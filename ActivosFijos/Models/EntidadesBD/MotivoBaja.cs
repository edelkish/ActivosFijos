using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("MotivoBaja")]
    public partial class MotivoBaja
    {
        [Key]
        public int IdMotivoBaja { get; set; }
        [StringLength(200)]
        [Unicode(false)]
        public string Nombre { get; set; } = string.Empty;
    }
}
