using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPuestosTrabajoYUbicacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActFijo_Cubiculos",
                table: "ActFijo");

            migrationBuilder.RenameColumn(
                name: "Cubiculo",
                table: "ActFijo",
                newName: "PuestoTrabajo");

            migrationBuilder.RenameIndex(
                name: "IX_ActFijo_Cubiculo",
                table: "ActFijo",
                newName: "IX_ActFijo_PuestoTrabajo");

            migrationBuilder.AddColumn<Guid>(
                name: "Local",
                table: "ActFijo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PuestosTrabajo",
                columns: table => new
                {
                    IdPuesto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    IdLocal = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuestosTrabajo", x => x.IdPuesto);
                    table.ForeignKey(
                        name: "FK_PuestosTrabajo_Locales",
                        column: x => x.IdLocal,
                        principalTable: "Locales",
                        principalColumn: "IdLocal",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialUbicacionAFT",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    NoInventario = table.Column<long>(type: "bigint", nullable: false),
                    SubNumero = table.Column<int>(type: "int", nullable: false),
                    fechaSubmayor = table.Column<DateTime>(type: "datetime", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    LocalOrigen = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PuestoOrigen = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LocalDestino = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PuestoDestino = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsuarioRegistra = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialUbicacionAFT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialUbicacionAFT_ActFijo",
                        columns: x => new { x.NoInventario, x.SubNumero, x.fechaSubmayor },
                        principalTable: "ActFijo",
                        principalColumns: new[] { "noInventario", "subNumero", "fechaSubmayor" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialUbicacionAFT_LocalDestino",
                        column: x => x.LocalDestino,
                        principalTable: "Locales",
                        principalColumn: "IdLocal",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialUbicacionAFT_LocalOrigen",
                        column: x => x.LocalOrigen,
                        principalTable: "Locales",
                        principalColumn: "IdLocal",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialUbicacionAFT_PuestoDestino",
                        column: x => x.PuestoDestino,
                        principalTable: "PuestosTrabajo",
                        principalColumn: "IdPuesto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialUbicacionAFT_PuestoOrigen",
                        column: x => x.PuestoOrigen,
                        principalTable: "PuestosTrabajo",
                        principalColumn: "IdPuesto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActFijo_Local",
                table: "ActFijo",
                column: "Local");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialUbicacionAFT_LocalDestino",
                table: "HistorialUbicacionAFT",
                column: "LocalDestino");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialUbicacionAFT_LocalOrigen",
                table: "HistorialUbicacionAFT",
                column: "LocalOrigen");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialUbicacionAFT_NoInventario_SubNumero_fechaSubmayor",
                table: "HistorialUbicacionAFT",
                columns: new[] { "NoInventario", "SubNumero", "fechaSubmayor" });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialUbicacionAFT_PuestoDestino",
                table: "HistorialUbicacionAFT",
                column: "PuestoDestino");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialUbicacionAFT_PuestoOrigen",
                table: "HistorialUbicacionAFT",
                column: "PuestoOrigen");

            migrationBuilder.CreateIndex(
                name: "IX_PuestosTrabajo_IdLocal",
                table: "PuestosTrabajo",
                column: "IdLocal");

            migrationBuilder.AddForeignKey(
                name: "FK_ActFijo_Locales",
                table: "ActFijo",
                column: "Local",
                principalTable: "Locales",
                principalColumn: "IdLocal",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ActFijo_PuestosTrabajo",
                table: "ActFijo",
                column: "PuestoTrabajo",
                principalTable: "PuestosTrabajo",
                principalColumn: "IdPuesto",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActFijo_Locales",
                table: "ActFijo");

            migrationBuilder.DropForeignKey(
                name: "FK_ActFijo_PuestosTrabajo",
                table: "ActFijo");

            migrationBuilder.DropTable(
                name: "HistorialUbicacionAFT");

            migrationBuilder.DropTable(
                name: "PuestosTrabajo");

            migrationBuilder.DropIndex(
                name: "IX_ActFijo_Local",
                table: "ActFijo");

            migrationBuilder.DropColumn(
                name: "Local",
                table: "ActFijo");

            migrationBuilder.RenameColumn(
                name: "PuestoTrabajo",
                table: "ActFijo",
                newName: "Cubiculo");

            migrationBuilder.RenameIndex(
                name: "IX_ActFijo_PuestoTrabajo",
                table: "ActFijo",
                newName: "IX_ActFijo_Cubiculo");

            migrationBuilder.AddForeignKey(
                name: "FK_ActFijo_Cubiculos",
                table: "ActFijo",
                column: "Cubiculo",
                principalTable: "Cubiculos",
                principalColumn: "IdCubiculo");
        }
    }
}
