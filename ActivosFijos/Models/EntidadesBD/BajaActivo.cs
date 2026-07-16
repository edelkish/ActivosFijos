using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("BajaActivo")]
    public partial class BajaActivo
    {
        [Key]
        public int IdBaja { get; set; }
        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime FechaSubmayor { get; set; }
        public int MotivoBajaId { get; set; }
        [StringLength(500)]
        [Unicode(false)]
        public string? Observaciones { get; set; }
        public int UsuarioId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool EsBaja { get; set; }

        [ForeignKey(nameof(MotivoBajaId))]
        public virtual MotivoBaja? MotivoBajaNavigation { get; set; }
    }
}
