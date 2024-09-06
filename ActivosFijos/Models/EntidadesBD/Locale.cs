using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD;

public partial class Locale
{
    [Key]
    public Guid IdLocal { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    [Unicode(false)]
    public string? Descripcion { get; set; }

    [InverseProperty("LocalNavigation")]
    public virtual ICollection<Cubiculo> Cubiculos { get; set; } = new List<Cubiculo>();
}
