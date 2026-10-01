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

    public Guid CentroCosto { get; set; }

    [ForeignKey("CentroCosto")]
    [InverseProperty("Locales")]
    public virtual CentroCosto CentroCostoNavigation { get; set; } = null!;

    [InverseProperty("LocalNavigation")]
    public virtual ICollection<PuestoTrabajo> PuestosTrabajo { get; set; } = new List<PuestoTrabajo>();

    [InverseProperty("LocalOrigenNavigation")]
    public virtual ICollection<HistorialUbicacionAFT> HistorialOrigen { get; set; } = new List<HistorialUbicacionAFT>();

    [InverseProperty("LocalDestinoNavigation")]
    public virtual ICollection<HistorialUbicacionAFT> HistorialDestino { get; set; } = new List<HistorialUbicacionAFT>();

    [InverseProperty("LocalNavigation")]
    public virtual ICollection<ActFijo> ActFijosLocales { get; set; } = new List<ActFijo>();
}
