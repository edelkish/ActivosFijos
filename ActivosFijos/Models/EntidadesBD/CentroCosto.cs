using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("CentroCosto")]
    public partial class CentroCosto
    {
        public CentroCosto()
        {
            ActFijos = new HashSet<ActFijo>();
            UtilesHerramienta = new HashSet<UtilesHerramienta>();
        }

        [Column("numero")]
        public long? Numero { get; set; }
        [Column("nombre")]
        [StringLength(100)]
        [Unicode(false)]
        public string? Nombre { get; set; }
        [Key]
        [Column("idCentroCosto")]
        public Guid IdCentroCosto { get; set; }

        [InverseProperty(nameof(ActFijo.CentroCostoNavigation))]
        public virtual ICollection<ActFijo> ActFijos { get; set; }

        [InverseProperty(nameof(EntidadesBD.UtilesHerramienta.CentroCostoNavigation))]
        public virtual ICollection<UtilesHerramienta> UtilesHerramienta { get; set; }

        [InverseProperty(nameof(MuestreoMensualDiezPorCiento.CentroCostoNavigation))]
        public virtual ICollection<MuestreoMensualDiezPorCiento> MuestreoMensualDiezPorCientos { get; set; } = new List<MuestreoMensualDiezPorCiento>();
    }
}
