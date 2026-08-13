using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("MuestreoMensualDiezPorCientoUTH")]
    public partial class MuestreoMensualDiezPorCientoUTH
    {
        [Key]
        public long Id { get; set; }
        public int Anno { get; set; }
        public int Mes { get; set; }
        public Guid IdUth { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string Codigo { get; set; } = null!;
        public Guid? Responsable { get; set; }
        public Guid? CentroCosto { get; set; }
        public Guid AreaResponsabilidad { get; set; }
        [StringLength(2000)]
        [Unicode(false)]
        public string? Descripcion { get; set; }
        [Column(TypeName = "money")]
        public decimal? Valor { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime FechaMuestreo { get; set; }
        public bool SiExiste { get; set; }
        public string? Observaciones { get; set; }
    }
}
