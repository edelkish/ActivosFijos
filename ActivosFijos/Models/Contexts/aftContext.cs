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
        public virtual DbSet<PuestoTrabajo> PuestosTrabajo { get; set; } = null!;
        public virtual DbSet<HistorialUbicacionAFT> HistorialUbicacionAFTs { get; set; } = null!;
        public virtual DbSet<Permiso> Permisos { get; set; } = null!;
        public virtual DbSet<Responsable> Responsables { get; set; } = null!;
        public virtual DbSet<TrasladosAft> TrasladosAfts { get; set; } = null!;
        public virtual DbSet<Usuario> Usuarios { get; set; } = null!;
        public virtual DbSet<UtilesHerramienta> UtilesHerramientas { get; set; } = null!;
        public virtual DbSet<MuestreoMensualDiezPorCiento> MuestreoMensualDiezPorCientos { get; set; } = null!;
        public virtual DbSet<PlanMuestreoAnual> PlanMuestreoAnuals { get; set; } = null!;
        public virtual DbSet<MuestreoMensualDiezPorCientoUTH> MuestreoMensualDiezPorCientoUTHs { get; set; } = null!;
        public virtual DbSet<PlanMuestreoAnualUTH> PlanMuestreoAnualUTHs { get; set; } = null!;
        public virtual DbSet<BajaActivo> BajaActivos { get; set; } = null!;
        public virtual DbSet<MotivoBaja> MotivoBajas { get; set; } = null!;

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
                entity.HasKey(e => new { e.NoInventario, e.SubNumero, e.FechaSubmayor });
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

                entity.HasOne(d => d.LocalNavigation)
                    .WithMany(p => p.ActFijosLocales)
                    .HasForeignKey(d => d.Local)
                    .HasConstraintName("FK_ActFijo_Locales")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.PuestoTrabajoNavigation)
                    .WithMany(p => p.ActFijos)
                    .HasForeignKey(d => d.PuestoTrabajo)
                    .HasConstraintName("FK_ActFijo_PuestosTrabajo")
                    .OnDelete(DeleteBehavior.Restrict);
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
                
            });

            modelBuilder.Entity<Locale>(entity =>
            {
                entity.Property(e => e.IdLocal).ValueGeneratedNever();

                entity.HasOne(d => d.CubiculoNavigation).WithMany(p => p.Locales).HasConstraintName("FK_Locales_Cubiculos");
            });

            modelBuilder.Entity<PuestoTrabajo>(entity =>
            {
                entity.Property(e => e.IdPuesto).ValueGeneratedNever();

                entity.HasOne(d => d.LocalNavigation)
                    .WithMany(p => p.PuestosTrabajo)
                    .HasForeignKey(d => d.IdLocal)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_PuestosTrabajo_Locales");
            });

            modelBuilder.Entity<HistorialUbicacionAFT>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.FechaMovimiento).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.ActFijoNavigation)
                    .WithMany(p => p.HistorialUbicacionAFTs)
                    .HasForeignKey(d => new { d.NoInventario, d.SubNumero, d.FechaSubmayor })
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_HistorialUbicacionAFT_ActFijo");

                entity.HasOne(d => d.LocalOrigenNavigation)
                    .WithMany(p => p.HistorialOrigen)
                    .HasForeignKey(d => d.LocalOrigen)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_HistorialUbicacionAFT_LocalOrigen");

                entity.HasOne(d => d.PuestoOrigenNavigation)
                    .WithMany(p => p.HistorialOrigen)
                    .HasForeignKey(d => d.PuestoOrigen)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_HistorialUbicacionAFT_PuestoOrigen");

                entity.HasOne(d => d.LocalDestinoNavigation)
                    .WithMany(p => p.HistorialDestino)
                    .HasForeignKey(d => d.LocalDestino)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_HistorialUbicacionAFT_LocalDestino");

                entity.HasOne(d => d.PuestoDestinoNavigation)
                    .WithMany(p => p.HistorialDestino)
                    .HasForeignKey(d => d.PuestoDestino)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_HistorialUbicacionAFT_PuestoDestino");
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
                        "PermisosUsuarios",
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

            modelBuilder.Entity<MuestreoMensualDiezPorCiento>(entity =>
            {
                entity.HasKey(e => new { e.Anno, e.Mes, e.NoInventario, e.SubNumero, e.FechaSubmayor })
                    .HasName("PK_MuestreoMensualDiezPorciento");

                entity.Property(e => e.FechaMuestreo).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.SiExiste).HasDefaultValueSql("((0))");
                entity.Property(e => e.Observaciones).HasColumnType("nvarchar(max)");

                entity.HasOne(d => d.ActFijoNavigation)
                    .WithMany(p => p.MuestreoMensualDiezPorCientos)
                    .HasForeignKey(d => new { d.NoInventario, d.SubNumero, d.FechaSubmayor })
                    .HasConstraintName("FK_MuestreoMensualDiezPorCiento_ActFijo");

                entity.HasOne(d => d.CentroCostoNavigation)
                    .WithMany(p => p.MuestreoMensualDiezPorCientos)
                    .HasForeignKey(d => d.CentroCosto)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_MuestreoMensualDiezPorCiento_CentroCosto");
            });

            modelBuilder.Entity<PlanMuestreoAnual>(entity =>
            {
                entity.HasKey(e => new { e.Anno, e.CentroCosto, e.AreaResponsabilidad })
                    .HasName("PK_PlanMuestreoAnual");

                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<MuestreoMensualDiezPorCientoUTH>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_MuestreoMensualDiezPorCientoUTH");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasIndex(e => new { e.Anno, e.Mes, e.IdUth })
                    .IsUnique()
                    .HasDatabaseName("UQ_MuestreoMensualDiezPorCientoUTH_IdUth");

                entity.Property(e => e.FechaMuestreo).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.SiExiste).HasDefaultValueSql("((0))");
                entity.Property(e => e.Observaciones).HasColumnType("nvarchar(max)");
            });

            modelBuilder.Entity<PlanMuestreoAnualUTH>(entity =>
            {
                entity.HasKey(e => new { e.Anno, e.CentroCosto, e.AreaResponsabilidad })
                    .HasName("PK_PlanMuestreoAnualUTH");

                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<BajaActivo>(entity =>
            {
                entity.HasKey(e => e.IdBaja);

                entity.Property(e => e.EsBaja).HasDefaultValue(false);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<MotivoBaja>(entity =>
            {
                entity.HasKey(e => e.IdMotivoBaja);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
