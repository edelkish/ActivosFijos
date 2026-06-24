using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("MuestreoMensualDiezPorCiento")]
    public partial class MuestreoMensualDiezPorCiento
    {
        [Key]
        public int Anno { get; set; }
        [Key]
        public int Mes { get; set; }
        [Key]
        public long NoInventario { get; set; }
        [Key]
        public int SubNumero { get; set; }
        [Key]
        [Column("fechaSubmayor", TypeName = "datetime")]
        public DateTime FechaSubmayor { get; set; }
        [StringLength(2000)]
        [Unicode(false)]
        public string? Descripcion { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime FechaMuestreo { get; set; }
        public bool SiExiste { get; set; }
        public string? Observaciones { get; set; }
        public Guid? CentroCosto { get; set; }

        [ForeignKey(nameof(NoInventario) + "," + nameof(SubNumero) + "," + nameof(FechaSubmayor))]
        [InverseProperty(nameof(ActFijo.MuestreoMensualDiezPorCientos))]
        public virtual ActFijo ActFijoNavigation { get; set; } = null!;

        [ForeignKey(nameof(CentroCosto))]
        [InverseProperty(nameof(EntidadesBD.CentroCosto.MuestreoMensualDiezPorCientos))]
        public virtual CentroCosto? CentroCostoNavigation { get; set; }
    }
}
