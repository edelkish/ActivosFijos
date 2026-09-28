using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("Parametro")]
    public partial class Parametro
    {
        [Key]
        [Column("idParametro")]
        public Guid IdParametro { get; set; }
        [Required]
        [StringLength(150)]
        [Unicode(false)]
        public string Nombre { get; set; } = string.Empty;
        [StringLength(500)]
        [Unicode(false)]
        public string? Valor { get; set; }
        [StringLength(150)]
        [Unicode(false)]
        public string? Grupo { get; set; }
    }
}