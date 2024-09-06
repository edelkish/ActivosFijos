using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    public partial class Permiso
    {
        public Permiso()
        {
            IdUsuarios = new HashSet<Usuario>();
        }

        [Key]
        public Guid IdPermiso { get; set; }
        [StringLength(350)]
        [Unicode(false)]
        public string? Nombre { get; set; }
        [StringLength(15)]
        [Unicode(false)]
        public string? Identificador { get; set; }

        [ForeignKey("IdPermiso")]
        [InverseProperty(nameof(Usuario.IdPermisos))]
        public virtual ICollection<Usuario> IdUsuarios { get; set; }
    }
}
