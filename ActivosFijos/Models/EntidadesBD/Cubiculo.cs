using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD;

public partial class Cubiculo
{
    [Key]
    public Guid IdCubiculo { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string? Nombre { get; set; } = string.Empty;

    [StringLength(150)]
    [Unicode(false)]
    public string? Apodo { get; set; } = string.Empty;

    [StringLength(500)]
    [Unicode(false)]
    public string? Descripcion { get; set; }

    [InverseProperty("CubiculoNavigation")]
    public virtual ICollection<Locale> Locales { get; set; } = new List<Locale>();
}
