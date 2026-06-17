using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("ActFijo")]
    public partial class ActFijo
    {
        [Key]
        [Column("noInventario")]
        public long NoInventario { get; set; }
        [Key]
        [Column("subNumero")]
        public int SubNumero { get; set; }
        [Column("descripcion")]
        [StringLength(2000)]
        [Unicode(false)]
        public string? Descripcion { get; set; }
        [Column("depreciacionAcum", TypeName = "money")]
        public decimal? DepreciacionAcum { get; set; }
        [Column("valorContable", TypeName = "money")]
        public decimal? ValorContable { get; set; }
        [Column("responsable")]
        public Guid? Responsable { get; set; }
        [Column("centroCosto")]
        public Guid? CentroCosto { get; set; }
        [Column("areaResponsabilidad")]
        public Guid? AreaResponsabilidad { get; set; }
        [Column("valorAdquisicion", TypeName = "money")]
        public decimal? ValorAdquisicion { get; set; }
        [Column("valorResidual", TypeName = "money")]
        public decimal? ValorResidual { get; set; }
        [Key]
        [Column("fechaSubmayor", TypeName = "datetime")]
        public DateTime FechaSubmayor { get; set; }
        [Column("clasificador")]
        public Guid? Clasificador { get; set; }
        public Guid? Cubiculo { get; set; }

        [ForeignKey(nameof(AreaResponsabilidad))]
        [InverseProperty(nameof(AreaDeResponsabilidad.ActFijos))]
        public virtual AreaDeResponsabilidad? AreaResponsabilidadNavigation { get; set; }
        [ForeignKey(nameof(CentroCosto))]
        [InverseProperty("ActFijos")]
        public virtual CentroCosto? CentroCostoNavigation { get; set; }
        [ForeignKey("Clasificador")]
        [InverseProperty("ActFijos")]
        public virtual Clasificador? ClasificadorNavigation { get; set; }
        [ForeignKey(nameof(Responsable))]
        [InverseProperty("ActFijos")]
        public virtual Responsable? ResponsableNavigation { get; set; }

        [ForeignKey("Cubiculo")]
        [InverseProperty("ActFijos")]
        public virtual Cubiculo? CubiculoNavigation { get; set; }

        [InverseProperty(nameof(MuestreoMensualDiezPorCiento.ActFijoNavigation))]
        public virtual ICollection<MuestreoMensualDiezPorCiento> MuestreoMensualDiezPorCientos { get; set; } = new List<MuestreoMensualDiezPorCiento>();
    }
}
