using System;
using System.Collections.Generic;
using ActivosFijos.Models.AuxBD;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ActivosFijos.Models.Contexts
{
    public partial class aftContextAux : DbContext
    {
        public aftContextAux()
        {
        }

        public aftContextAux(DbContextOptions<aftContextAux> options)
            : base(options)
        {
        }

        public virtual DbSet<ExistenciaMovimiento> ExistenciaMovimientos { get; set; } = null!;
        public virtual DbSet<ExistenciaMovimientoUTH> ExistenciaMovimientosUTH { get; set; } = null!;
        public virtual DbSet<ActivosFijosConMovimientos> ActivosFijosConMovimientos { get; set; } = null!;
        public virtual DbSet<ActivosFijosMovimientos> ActivosFijosMovimientos { get; set; } = null!;
        public virtual DbSet<ActivosFijosMovimientosResponsables> ActivosFijosMovimientosResponsables { get; set; } = null!;
        public virtual DbSet<ActivosFijosSinClasificar> ActivosFijosSinClasificar { get; set; } = null!;
        public virtual DbSet<ActivosFijosClasificados> ActivosFijosClasificados { get; set; } = null!;
        public virtual DbSet<ClasificadoresTotales> ClasificadoresTotales { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Latin1_General_CI_AI");

            modelBuilder.Entity<ExistenciaMovimiento>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<ExistenciaMovimientoUTH>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<ActivosFijosConMovimientos>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<ActivosFijosMovimientos>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<ActivosFijosMovimientosResponsables>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<ActivosFijosSinClasificar>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<ActivosFijosClasificados>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<ClasificadoresTotales>(entity =>
            {
                entity.HasNoKey();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
