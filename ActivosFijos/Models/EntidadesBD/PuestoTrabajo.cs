using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("PuestosTrabajo")]
    public partial class PuestoTrabajo
    {
        [Key]
        public Guid IdPuesto { get; set; }

        [StringLength(150)]
        [Unicode(false)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Unicode(false)]
        public string? Descripcion { get; set; }

        public Guid IdLocal { get; set; }

        [ForeignKey(nameof(IdLocal))]
        [InverseProperty("PuestosTrabajo")]
        public virtual Locale? LocalNavigation { get; set; }

        [InverseProperty(nameof(ActFijo.PuestoTrabajoNavigation))]
        public virtual ICollection<ActFijo> ActFijos { get; set; } = new List<ActFijo>();

        [InverseProperty(nameof(HistorialUbicacionAFT.PuestoDestinoNavigation))]
        public virtual ICollection<HistorialUbicacionAFT> HistorialDestino { get; set; } = new List<HistorialUbicacionAFT>();

        [InverseProperty(nameof(HistorialUbicacionAFT.PuestoOrigenNavigation))]
        public virtual ICollection<HistorialUbicacionAFT> HistorialOrigen { get; set; } = new List<HistorialUbicacionAFT>();
    }
}
