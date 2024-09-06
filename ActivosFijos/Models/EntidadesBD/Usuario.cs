using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    public partial class Usuario
    {
        public Usuario()
        {
            IdPermisos = new HashSet<Permiso>();
        }

        [Column("Usuario")]
        [StringLength(150)]
        [Unicode(false)]
        public string? Usuario1 { get; set; }
        [StringLength(500)]
        [Unicode(false)]
        public string? Contrasenna { get; set; }
        [Key]
        public Guid IdUsuario { get; set; }
        [StringLength(200)]
        [Unicode(false)]
        public string? Nombre { get; set; }
        [StringLength(350)]
        [Unicode(false)]
        public string? Apellidos { get; set; }
        [StringLength(150)]
        [Unicode(false)]
        public string? Correo { get; set; }

        [ForeignKey("IdUsuario")]
        [InverseProperty(nameof(Permiso.IdUsuarios))]
        public virtual ICollection<Permiso> IdPermisos { get; set; }
    }
}
