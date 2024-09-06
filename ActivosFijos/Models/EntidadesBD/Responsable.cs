using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("Responsable")]
    public partial class Responsable
    {
        public Responsable()
        {
            ActFijos = new HashSet<ActFijo>();
            UtilesHerramienta = new HashSet<UtilesHerramienta>();
            TrasladosAfts = new HashSet<TrasladosAft>();
        }

        [Column("nombre")]
        [StringLength(50)]
        [Unicode(false)]
        public string? Nombre { get; set; }
        [Column("apellidos")]
        [StringLength(100)]
        [Unicode(false)]
        public string? Apellidos { get; set; }
        [Column("cargo")]
        public Guid? Cargo { get; set; }
        [Key]
        [Column("idResponsable")]
        public Guid IdResponsable { get; set; }
        [Column("responsableDelArea")]
        public bool ResponsableDelArea { get; set; } = false;
        [Column("areaResponsabilidad")]
        public Guid? AreaResponsabilidad { get; set; }
        [Column("responsableDeEconomia")]
        public bool ResponsableDeEconomia { get; set; } = false;

        [ForeignKey(nameof(AreaResponsabilidad))]
        [InverseProperty(nameof(AreaDeResponsabilidad.Responsables))]
        public virtual AreaDeResponsabilidad? AreaResponsabilidadNavigation { get; set; }
        [ForeignKey(nameof(Cargo))]
        [InverseProperty("Responsables")]
        public virtual Cargo? CargoNavigation { get; set; }
        [InverseProperty(nameof(ActFijo.ResponsableNavigation))]
        public virtual ICollection<ActFijo> ActFijos { get; set; }
        [InverseProperty(nameof(TrasladosAft.ResponsableNavigation))]
        public virtual ICollection<TrasladosAft> TrasladosAfts { get; set; }

        [InverseProperty(nameof(EntidadesBD.UtilesHerramienta.ResponsableNavigation))]
        public virtual ICollection<UtilesHerramienta> UtilesHerramienta { get; set; }

    }
}
