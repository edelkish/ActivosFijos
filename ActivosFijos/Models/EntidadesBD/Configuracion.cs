using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("Configuracion")]
    public partial class Configuracion
    {
        [Key]
        [Column("idConfiguracion")]
        public Guid IdConfiguracion { get; set; }
        [StringLength(150)]
        [Unicode(false)]
        public string? Nombre { get; set; }
        [StringLength(2)]
        [Unicode(false)]
        public string? Columna { get; set; }
        public long? Fila { get; set; }
    }
}
