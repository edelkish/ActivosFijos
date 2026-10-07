using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("Anexo3Firmas")]
    public partial class Anexo3Firmas
    {
        [Key]
        [Column("NoInventario")]
        public long NoInventario { get; set; }

        [Key]
        [Column("SubNumero")]
        public int SubNumero { get; set; }

        [Key]
        [Column("FechaSubmayor")]
        public DateTime FechaSubmayor { get; set; }

        [Column("TipoMovimiento")]
        [StringLength(100)]
        [Unicode(false)]
        public string? TipoMovimiento { get; set; }

        [Column("Fundamentacion")]
        [StringLength(500)]
        [Unicode(false)]
        public string? Fundamentacion { get; set; }

        [Column("InformeTecnico")]
        [StringLength(500)]
        [Unicode(false)]
        public string? InformeTecnico { get; set; }

        [Column("NombreTecnico")]
        [StringLength(150)]
        [Unicode(false)]
        public string? NombreTecnico { get; set; }

        [Column("CargoTecnico")]
        [StringLength(150)]
        [Unicode(false)]
        public string? CargoTecnico { get; set; }

        [Column("HechoPor")]
        [StringLength(150)]
        [Unicode(false)]
        public string? HechoPor { get; set; }

        [Column("CargoHechoPor")]
        [StringLength(150)]
        [Unicode(false)]
        public string? CargoHechoPor { get; set; }

        [Column("TransportadorReceptor")]
        [StringLength(150)]
        [Unicode(false)]
        public string? TransportadorReceptor { get; set; }

        [Column("CargoTransportadorReceptor")]
        [StringLength(150)]
        [Unicode(false)]
        public string? CargoTransportadorReceptor { get; set; }

        [Column("Comprobante")]
        [StringLength(100)]
        [Unicode(false)]
        public string? Comprobante { get; set; }

        /// <summary>
        /// Direccion del receptor. Solo se usa en traslado externo y enviado a reparar;
        /// en los demas tipos la direccion del receptor es la de la UEB, que sale de
        /// la tabla de parametros y no se guarda por movimiento.
        /// </summary>
        [Column("DireccionReceptor")]
        [StringLength(300)]
        [Unicode(false)]
        public string? DireccionReceptor { get; set; }

        /// <summary>
        /// Nombre de quien recepciona el Activo Fijo. En traslado interno por defecto es el
        /// ResponsableActual del movimiento, pero el panel lo deja cambiar; en traslado
        /// externo y enviado a reparar se captura a mano.
        /// </summary>
        [Column("ReceptorNombre")]
        [StringLength(150)]
        [Unicode(false)]
        public string? ReceptorNombre { get; set; }

        /// <summary>
        /// Area destino del receptor. En traslado interno se deriva del responsable elegido
        /// y en los demas tipos se captura a mano.
        /// </summary>
        [Column("ReceptorArea")]
        [StringLength(150)]
        [Unicode(false)]
        public string? ReceptorArea { get; set; }

        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }
    }
}