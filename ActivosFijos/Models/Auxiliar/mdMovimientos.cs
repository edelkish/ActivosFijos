using System;
using System.Collections.Generic;
using ActivosFijos.Models.EntidadesBD;

namespace ActivosFijos.Models.Auxiliar
{
    public class mdMovimientos
    {
        public DateTime Fecha { get; set; } = DateTime.Today;

        public List<TrasladosAft> Movimientos { get; set; } = new List<TrasladosAft>();

        public List<TipoMovimientoAFT> Tipos { get; set; } = new List<TipoMovimientoAFT>();

        public List<Responsable> Responsables { get; set; } = new List<Responsable>();

        public TrasladosAft Formulario { get; set; } = new TrasladosAft();
    }
}
