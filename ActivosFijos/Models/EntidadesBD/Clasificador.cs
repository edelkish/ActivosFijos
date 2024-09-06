using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("Clasificador")]
    public partial class Clasificador
    {
        public Clasificador()
        {
            ActFijos = new HashSet<ActFijo>();
        }

        [Key]
        [Column("idClasificador")]
        public Guid IdClasificador { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string? Nombre { get; set; }
        [StringLength(1000)]
        [Unicode(false)]
        public string? Descripcion { get; set; }

        [InverseProperty("ClasificadorNavigation")]
        public virtual ICollection<ActFijo> ActFijos { get; set; }
    }
}
