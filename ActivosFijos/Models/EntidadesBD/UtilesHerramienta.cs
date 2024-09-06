using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    public partial class UtilesHerramienta
    {
        [Key]
        [Column("codigo")]
        [StringLength(100)]
        [Unicode(false)]
        public string Codigo { get; set; } = null!;
        [Column("descripcion")]
        [StringLength(2000)]
        [Unicode(false)]
        public string? Descripcion { get; set; }
        [Column("valorContable", TypeName = "money")]
        public decimal? ValorContable { get; set; }
        [Column("responsable")]
        public Guid? Responsable { get; set; }
        [Column("centroCosto")]
        public Guid? CentroCosto { get; set; }
        [Key]
        [Column("areaResponsabilidad")]
        public Guid AreaResponsabilidad { get; set; }
        [Column("valorAdquisicion", TypeName = "money")]
        public decimal? ValorAdquisicion { get; set; }
        [Key]
        [Column("fechaListadoUTH", TypeName = "datetime")]
        public DateTime FechaListadoUth { get; set; }
        [Key]
        [Column("no")]
        public long No { get; set; }
        [Column("idUTH")]
        public Guid IdUth { get; set; }

        [ForeignKey("AreaResponsabilidad")]
        [InverseProperty("UtilesHerramienta")]
        public virtual AreaDeResponsabilidad AreaResponsabilidadNavigation { get; set; } = null!;
        [ForeignKey("CentroCosto")]
        [InverseProperty("UtilesHerramienta")]
        public virtual CentroCosto? CentroCostoNavigation { get; set; }
        [ForeignKey("Responsable")]
        [InverseProperty("UtilesHerramienta")]
        public virtual Responsable? ResponsableNavigation { get; set; }
    }
}
