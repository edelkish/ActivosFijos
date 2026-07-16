namespace ActivosFijos.Models.Auxiliar
{
    public class mdBajasActivo
    {
    }

    public class mdBajaActivoItem
    {
        public long NoInventario { get; set; }
        public int SubNumero { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Responsable { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public decimal? ValorContable { get; set; }
        public decimal? ValorAdquisicion { get; set; }
        public DateTime FechaSubmayor { get; set; }
        public int MotivoBajaId { get; set; }
        public string? Observaciones { get; set; }
        public bool Seleccionado { get; set; }
    }
}
