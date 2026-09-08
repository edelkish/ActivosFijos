using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("HistorialUbicacionAFT")]
    public partial class HistorialUbicacionAFT
    {
        [Key]
        public Guid Id { get; set; }

        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
        [Column("fechaSubmayor", TypeName = "datetime")]
        public DateTime FechaSubmayor { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime FechaMovimiento { get; set; }

        public Guid? LocalOrigen { get; set; }
        public Guid? PuestoOrigen { get; set; }
        public Guid? LocalDestino { get; set; }
        public Guid? PuestoDestino { get; set; }

        [StringLength(100)]
        [Unicode(false)]
        public string? UsuarioRegistra { get; set; }

        [ForeignKey(nameof(NoInventario) + "," + nameof(SubNumero) + "," + nameof(FechaSubmayor))]
        [InverseProperty(nameof(ActFijo.HistorialUbicacionAFTs))]
        public virtual ActFijo? ActFijoNavigation { get; set; }

        [ForeignKey(nameof(LocalOrigen))]
        [InverseProperty(nameof(Locale.HistorialOrigen))]
        public virtual Locale? LocalOrigenNavigation { get; set; }

        [ForeignKey(nameof(PuestoOrigen))]
        [InverseProperty(nameof(PuestoTrabajo.HistorialOrigen))]
        public virtual PuestoTrabajo? PuestoOrigenNavigation { get; set; }

        [ForeignKey(nameof(LocalDestino))]
        [InverseProperty(nameof(Locale.HistorialDestino))]
        public virtual Locale? LocalDestinoNavigation { get; set; }

        [ForeignKey(nameof(PuestoDestino))]
        [InverseProperty(nameof(PuestoTrabajo.HistorialDestino))]
        public virtual PuestoTrabajo? PuestoDestinoNavigation { get; set; }
    }
}
