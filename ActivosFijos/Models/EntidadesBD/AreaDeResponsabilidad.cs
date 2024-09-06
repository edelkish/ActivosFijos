using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("AreaDeResponsabilidad")]
    public partial class AreaDeResponsabilidad
    {
        public AreaDeResponsabilidad()
        {
            ActFijos = new HashSet<ActFijo>();
            Responsables = new HashSet<Responsable>();
            UtilesHerramienta = new HashSet<UtilesHerramienta>();
        }

        [Key]
        [Column("idArea")]
        public Guid IdArea { get; set; }
        [StringLength(150)]
        [Unicode(false)]
        public string? Nombre { get; set; }
        [StringLength(250)]
        [Unicode(false)]
        public string? Descripcion { get; set; }

        [InverseProperty(nameof(ActFijo.AreaResponsabilidadNavigation))]
        public virtual ICollection<ActFijo> ActFijos { get; set; }
        [InverseProperty(nameof(Responsable.AreaResponsabilidadNavigation))]
        public virtual ICollection<Responsable> Responsables { get; set; }
        [InverseProperty(nameof(EntidadesBD.UtilesHerramienta.AreaResponsabilidadNavigation))]
        public virtual ICollection<UtilesHerramienta> UtilesHerramienta { get; set; }
    }
}
