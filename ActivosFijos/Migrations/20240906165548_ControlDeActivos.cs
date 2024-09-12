using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class ControlDeActivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AreaDeResponsabilidad",
                columns: table => new
                {
                    idArea = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Descripcion = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentroCosto", x => x.idArea);
                });

            migrationBuilder.CreateTable(
                name: "Cargos",
                columns: table => new
                {
                    idCargo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargos", x => x.idCargo);
                });

            migrationBuilder.CreateTable(
                name: "CentroCosto",
                columns: table => new
                {
                    idCentroCosto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: true),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaDeResponsabilidad", x => x.idCentroCosto);
                });

            migrationBuilder.CreateTable(
                name: "Clasificador",
                columns: table => new
                {
                    idClasificador = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calsificador", x => x.idClasificador);
                });

            migrationBuilder.CreateTable(
                name: "Configuracion",
                columns: table => new
                {
                    idConfiguracion = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Columna = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: true),
                    Fila = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracion", x => x.idConfiguracion);
                });

            migrationBuilder.CreateTable(
                name: "Locales",
                columns: table => new
                {
                    IdLocal = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locales", x => x.IdLocal);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    IdPermiso = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "varchar(350)", unicode: false, maxLength: 350, nullable: true),
                    Identificador = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.IdPermiso);
                });

            

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Usuario = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Contrasenna = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Nombre = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Apellidos = table.Column<string>(type: "varchar(350)", unicode: false, maxLength: 350, nullable: true),
                    Correo = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Responsable",
                columns: table => new
                {
                    idResponsable = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    apellidos = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    cargo = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    responsableDelArea = table.Column<bool>(type: "bit", nullable: false),
                    areaResponsabilidad = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    responsableDeEconomia = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsable", x => x.idResponsable);
                    table.ForeignKey(
                        name: "FK_Responsable_AreaDeResponsabilidad",
                        column: x => x.areaResponsabilidad,
                        principalTable: "AreaDeResponsabilidad",
                        principalColumn: "idArea",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Responsable_Cargos",
                        column: x => x.cargo,
                        principalTable: "Cargos",
                        principalColumn: "idCargo",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Cubiculos",
                columns: table => new
                {
                    IdCubiculo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Apodo = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Local = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cubiculos", x => x.IdCubiculo);
                    table.ForeignKey(
                        name: "FK_Cubiculos_Locales",
                        column: x => x.Local,
                        principalTable: "Locales",
                        principalColumn: "IdLocal");
                });

            migrationBuilder.CreateTable(
                name: "PermisosUsuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPermiso = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermisosUsuarios", x => new { x.IdUsuario, x.IdPermiso });
                    table.ForeignKey(
                        name: "FK_PermisosUsuarios_Permisos",
                        column: x => x.IdPermiso,
                        principalTable: "Permisos",
                        principalColumn: "IdPermiso");
                    table.ForeignKey(
                        name: "FK_PermisosUsuarios_Usuarios",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario");
                });

            migrationBuilder.CreateTable(
                name: "TrasladosAFT",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    noInventario = table.Column<long>(type: "bigint", nullable: false),
                    desde = table.Column<DateTime>(type: "datetime", nullable: false),
                    hasta = table.Column<DateTime>(type: "datetime", nullable: true),
                    responsable = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    motivo = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrasladosAFT", x => x.id);
                    table.ForeignKey(
                        name: "FK_TrasladosAFT_Responsable",
                        column: x => x.responsable,
                        principalTable: "Responsable",
                        principalColumn: "idResponsable");
                });

            migrationBuilder.CreateTable(
                name: "UtilesHerramientas",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    areaResponsabilidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fechaListadoUTH = table.Column<DateTime>(type: "datetime", nullable: false),
                    no = table.Column<long>(type: "bigint", nullable: false),
                    descripcion = table.Column<string>(type: "varchar(2000)", unicode: false, maxLength: 2000, nullable: true),
                    valorContable = table.Column<decimal>(type: "money", nullable: true),
                    responsable = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    centroCosto = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    valorAdquisicion = table.Column<decimal>(type: "money", nullable: true),
                    idUTH = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilesHerramientas_1", x => new { x.codigo, x.areaResponsabilidad, x.fechaListadoUTH, x.no });
                    table.ForeignKey(
                        name: "FK_UtilesHerramientas_AreaDeResponsabilidad",
                        column: x => x.areaResponsabilidad,
                        principalTable: "AreaDeResponsabilidad",
                        principalColumn: "idArea");
                    table.ForeignKey(
                        name: "FK_UtilesHerramientas_CentroCosto",
                        column: x => x.centroCosto,
                        principalTable: "CentroCosto",
                        principalColumn: "idCentroCosto");
                    table.ForeignKey(
                        name: "FK_UtilesHerramientas_Responsable",
                        column: x => x.responsable,
                        principalTable: "Responsable",
                        principalColumn: "idResponsable");
                });

            migrationBuilder.CreateTable(
                name: "ActFijo",
                columns: table => new
                {
                    noInventario = table.Column<long>(type: "bigint", nullable: false),
                    fechaSubmayor = table.Column<DateTime>(type: "datetime", nullable: false),
                    descripcion = table.Column<string>(type: "varchar(2000)", unicode: false, maxLength: 2000, nullable: true),
                    depreciacionAcum = table.Column<decimal>(type: "money", nullable: true),
                    valorContable = table.Column<decimal>(type: "money", nullable: true),
                    responsable = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    centroCosto = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    areaResponsabilidad = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    valorAdquisicion = table.Column<decimal>(type: "money", nullable: true),
                    valorResidual = table.Column<decimal>(type: "money", nullable: true),
                    clasificador = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Cubiculo = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActFijo", x => new { x.noInventario, x.fechaSubmayor });
                    table.ForeignKey(
                        name: "FK_ActFijo_AreaDeResponsabilidad1",
                        column: x => x.areaResponsabilidad,
                        principalTable: "AreaDeResponsabilidad",
                        principalColumn: "idArea",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ActFijo_CentroCosto",
                        column: x => x.centroCosto,
                        principalTable: "CentroCosto",
                        principalColumn: "idCentroCosto",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ActFijo_Clasificador",
                        column: x => x.clasificador,
                        principalTable: "Clasificador",
                        principalColumn: "idClasificador");
                    table.ForeignKey(
                        name: "FK_ActFijo_Cubiculos",
                        column: x => x.Cubiculo,
                        principalTable: "Cubiculos",
                        principalColumn: "IdCubiculo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActFijo_Responsable",
                        column: x => x.responsable,
                        principalTable: "Responsable",
                        principalColumn: "idResponsable",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActFijo_areaResponsabilidad",
                table: "ActFijo",
                column: "areaResponsabilidad");

            migrationBuilder.CreateIndex(
                name: "IX_ActFijo_centroCosto",
                table: "ActFijo",
                column: "centroCosto");

            migrationBuilder.CreateIndex(
                name: "IX_ActFijo_clasificador",
                table: "ActFijo",
                column: "clasificador");

            migrationBuilder.CreateIndex(
                name: "IX_ActFijo_Cubiculo",
                table: "ActFijo",
                column: "Cubiculo");

            migrationBuilder.CreateIndex(
                name: "IX_ActFijo_responsable",
                table: "ActFijo",
                column: "responsable");

            migrationBuilder.CreateIndex(
                name: "IX_Cubiculos_Local",
                table: "Cubiculos",
                column: "Local");

            migrationBuilder.CreateIndex(
                name: "IX_PermisosUsuarios_IdPermiso",
                table: "PermisosUsuarios",
                column: "IdPermiso");

            migrationBuilder.CreateIndex(
                name: "IX_Responsable_areaResponsabilidad",
                table: "Responsable",
                column: "areaResponsabilidad");

            migrationBuilder.CreateIndex(
                name: "IX_Responsable_cargo",
                table: "Responsable",
                column: "cargo");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosAFT_responsable",
                table: "TrasladosAFT",
                column: "responsable");

            migrationBuilder.CreateIndex(
                name: "IX_UtilesHerramientas_areaResponsabilidad",
                table: "UtilesHerramientas",
                column: "areaResponsabilidad");

            migrationBuilder.CreateIndex(
                name: "IX_UtilesHerramientas_centroCosto",
                table: "UtilesHerramientas",
                column: "centroCosto");

            migrationBuilder.CreateIndex(
                name: "IX_UtilesHerramientas_responsable",
                table: "UtilesHerramientas",
                column: "responsable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActFijo");

            migrationBuilder.DropTable(
                name: "Configuracion");

            migrationBuilder.DropTable(
                name: "PermisosUsuarios");

            migrationBuilder.DropTable(
                name: "PermisoUsuario");

            migrationBuilder.DropTable(
                name: "TrasladosAFT");

            migrationBuilder.DropTable(
                name: "UtilesHerramientas");

            migrationBuilder.DropTable(
                name: "Clasificador");

            migrationBuilder.DropTable(
                name: "Cubiculos");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "CentroCosto");

            migrationBuilder.DropTable(
                name: "Responsable");

            migrationBuilder.DropTable(
                name: "Locales");

            migrationBuilder.DropTable(
                name: "AreaDeResponsabilidad");

            migrationBuilder.DropTable(
                name: "Cargos");
        }
    }
}
