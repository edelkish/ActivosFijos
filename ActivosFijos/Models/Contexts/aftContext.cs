using System;
using System.Collections.Generic;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ActivosFijos.Models.Contexts
{
    public partial class aftContext : DbContext
    {
        public aftContext()
        {
        }

        public aftContext(DbContextOptions<aftContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActFijo> ActFijos { get; set; } = null!;
        public virtual DbSet<AreaDeResponsabilidad> AreaDeResponsabilidads { get; set; } = null!;
        public virtual DbSet<Cargo> Cargos { get; set; } = null!;
        public virtual DbSet<CentroCosto> CentroCostos { get; set; } = null!;
        public virtual DbSet<Clasificador> Clasificadors { get; set; } = null!;
        public virtual DbSet<Configuracion> Configuracions { get; set; } = null!;
        public virtual DbSet<Cubiculo> Cubiculos { get; set; }
        public virtual DbSet<Locale> Locales { get; set; }
        public virtual DbSet<Permiso> Permisos { get; set; } = null!;
        public virtual DbSet<Responsable> Responsables { get; set; } = null!;
        public virtual DbSet<TrasladosAft> TrasladosAfts { get; set; } = null!;
        public virtual DbSet<Usuario> Usuarios { get; set; } = null!;
        public virtual DbSet<UtilesHerramienta> UtilesHerramientas { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Latin1_General_CI_AI");

            modelBuilder.Entity<ActFijo>(entity =>
            {
                entity.HasKey(e => new { e.NoInventario, e.FechaSubmayor });
                //entity.Property(e => e.NoInventario).ValueGeneratedNever();

                entity.HasOne(d => d.AreaResponsabilidadNavigation)
                    .WithMany(p => p.ActFijos)
                    .HasForeignKey(d => d.AreaResponsabilidad)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_ActFijo_AreaDeResponsabilidad1");

                entity.HasOne(d => d.CentroCostoNavigation)
                    .WithMany(p => p.ActFijos)
                    .HasForeignKey(d => d.CentroCosto)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_ActFijo_CentroCosto");

                entity.HasOne(d => d.ClasificadorNavigation)
                    .WithMany(p => p.ActFijos)
                    .HasForeignKey(d => d.Clasificador)
                    .HasConstraintName("FK_ActFijo_Clasificador");

                entity.HasOne(d => d.ResponsableNavigation)
                    .WithMany(p => p.ActFijos)
                    .HasForeignKey(d => d.Responsable)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_ActFijo_Responsable");

                entity.HasOne(d => d.CubiculoNavigation)
                    .WithMany(p => p.ActFijos)
                    .HasConstraintName("FK_ActFijo_Cubiculos");
            });

            modelBuilder.Entity<AreaDeResponsabilidad>(entity =>
            {
                entity.HasKey(e => e.IdArea)
                    .HasName("PK_CentroCosto");

                entity.Property(e => e.IdArea).ValueGeneratedNever();

                // entity.Property(e => e.Nombre).UseCollation("SQL_Latin1_General_CP1_CI_AI");
            });

            modelBuilder.Entity<Cargo>(entity =>
            {
                entity.Property(e => e.IdCargo).ValueGeneratedNever();
            });

            modelBuilder.Entity<CentroCosto>(entity =>
            {
                entity.HasKey(e => e.IdCentroCosto)
                    .HasName("PK_AreaDeResponsabilidad");

                entity.Property(e => e.IdCentroCosto).ValueGeneratedNever();
            });

            modelBuilder.Entity<Clasificador>(entity =>
            {
                entity.HasKey(e => e.IdClasificador)
                    .HasName("PK_Calsificador");

                entity.Property(e => e.IdClasificador).HasDefaultValueSql("(newid())");
            });

            modelBuilder.Entity<Configuracion>(entity =>
            {
                entity.Property(e => e.IdConfiguracion).ValueGeneratedNever();
            });

            modelBuilder.Entity<Cubiculo>(entity =>
            {
                entity.Property(e => e.IdCubiculo).ValueGeneratedNever();

                entity.HasOne(d => d.LocalNavigation).WithMany(p => p.Cubiculos).HasConstraintName("FK_Cubiculos_Locales");
            });

            modelBuilder.Entity<Locale>(entity =>
            {
                entity.Property(e => e.IdLocal).ValueGeneratedNever();
            });

            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.Property(e => e.IdPermiso).HasDefaultValueSql("(newid())");
            });

            modelBuilder.Entity<Responsable>(entity =>
            {
                entity.Property(e => e.IdResponsable).ValueGeneratedNever();

                entity.HasOne(d => d.AreaResponsabilidadNavigation)
                    .WithMany(p => p.Responsables)
                    .HasForeignKey(d => d.AreaResponsabilidad)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Responsable_AreaDeResponsabilidad");

                entity.HasOne(d => d.CargoNavigation)
                    .WithMany(p => p.Responsables)
                    .HasForeignKey(d => d.Cargo)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Responsable_Cargos");
            });

            modelBuilder.Entity<TrasladosAft>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.ResponsableNavigation)
                    .WithMany(p => p.TrasladosAfts)
                    .HasForeignKey(d => d.Responsable)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TrasladosAFT_Responsable");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(e => e.IdUsuario).HasDefaultValueSql("(newid())");

                entity.HasMany(d => d.IdPermisos)
                    .WithMany(p => p.IdUsuarios)
                    .UsingEntity<Dictionary<string, object>>(
                        "PermisosUsuario",
                        l => l.HasOne<Permiso>().WithMany().HasForeignKey("IdPermiso").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_PermisosUsuarios_Permisos"),
                        r => r.HasOne<Usuario>().WithMany().HasForeignKey("IdUsuario").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_PermisosUsuarios_Usuarios"),
                        j =>
                        {
                            j.HasKey("IdUsuario", "IdPermiso");

                            j.ToTable("PermisosUsuarios");
                        });
            });

            modelBuilder.Entity<UtilesHerramienta>(entity =>
            {
                entity.HasKey(e => new { e.Codigo, e.AreaResponsabilidad, e.FechaListadoUth, e.No })
                    .HasName("PK_UtilesHerramientas_1");

                //entity.Property(e => e.IdUth).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.AreaResponsabilidadNavigation)
                    .WithMany(p => p.UtilesHerramienta)
                    .HasForeignKey(d => d.AreaResponsabilidad)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UtilesHerramientas_AreaDeResponsabilidad");

                entity.HasOne(d => d.CentroCostoNavigation)
                    .WithMany(p => p.UtilesHerramienta)
                    .HasForeignKey(d => d.CentroCosto)
                    .HasConstraintName("FK_UtilesHerramientas_CentroCosto");

                entity.HasOne(d => d.ResponsableNavigation)
                    .WithMany(p => p.UtilesHerramienta)
                    .HasForeignKey(d => d.Responsable)
                    .HasConstraintName("FK_UtilesHerramientas_Responsable");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
