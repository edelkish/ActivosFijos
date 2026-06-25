using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivosFijos.Models.EntidadesBD
{
    [Table("PlanMuestreoAnual")]
    public partial class PlanMuestreoAnual
    {
        [Key]
        public int Anno { get; set; }
        [Key]
        public Guid CentroCosto { get; set; }
        [Key]
        public Guid AreaResponsabilidad { get; set; }
        public int TotalActivos { get; set; }
        public int Ene { get; set; }
        public int Feb { get; set; }
        public int Mar { get; set; }
        public int Abr { get; set; }
        public int May { get; set; }
        public int Jun { get; set; }
        public int Jul { get; set; }
        public int Ago { get; set; }
        public int Sep { get; set; }
        public int Oct { get; set; }
        public int Nov { get; set; }
        public int Dic { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime FechaCreacion { get; set; }
    }
}
