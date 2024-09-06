using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    public partial class Cargo
    {
        public Cargo()
        {
            Responsables = new HashSet<Responsable>();
        }

        [Key]
        [Column("idCargo")]
        public Guid IdCargo { get; set; }
        [StringLength(250)]
        [Unicode(false)]
        public string? Nombre { get; set; }
        [StringLength(500)]
        [Unicode(false)]
        public string? Descripcion { get; set; }

        [InverseProperty(nameof(Responsable.CargoNavigation))]
        public virtual ICollection<Responsable> Responsables { get; set; }
    }
}
