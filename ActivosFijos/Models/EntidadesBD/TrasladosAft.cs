using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("TrasladosAFT")]
    public partial class TrasladosAft
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        [Column("noInventario")]
        public long NoInventario { get; set; }
        [Column("desde", TypeName = "datetime")]
        public DateTime Desde { get; set; }
        [Column("hasta", TypeName = "datetime")]
        public DateTime? Hasta { get; set; }
        [Column("responsable")]
        public Guid Responsable { get; set; }
        [Column("motivo")]
        [StringLength(1000)]
        [Unicode(false)]
        public string? Motivo { get; set; }

        [ForeignKey("Responsable")]
        [InverseProperty("TrasladosAfts")]
        public virtual Responsable ResponsableNavigation { get; set; } = null!;
    }
}
