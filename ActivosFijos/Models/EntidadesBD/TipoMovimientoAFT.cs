using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("TipoMovimientoAFT")]
    public partial class TipoMovimientoAFT
    {
        [Key]
        [Column("idTipoMovimientoAFT")]
        public Guid IdTipoMovimientoAFT { get; set; }
        [Required]
        [StringLength(200)]
        [Unicode(false)]
        public string Nombre { get; set; } = string.Empty;
        [StringLength(500)]
        [Unicode(false)]
        public string? Descripcion { get; set; }
        public int Orden { get; set; }
        public bool SiActivo { get; set; } = true;
    }
}