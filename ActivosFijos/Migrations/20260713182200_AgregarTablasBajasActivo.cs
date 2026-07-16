using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablasBajasActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MotivoBaja",
                columns: table => new
                {
                    IdMotivoBaja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivoBaja", x => x.IdMotivoBaja);
                });

            migrationBuilder.CreateTable(
                name: "BajaActivo",
                columns: table => new
                {
                    IdBaja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoInventario = table.Column<long>(type: "bigint", nullable: false),
                    SubNumero = table.Column<int>(type: "int", nullable: false),
                    FechaSubmayor = table.Column<DateTime>(type: "datetime", nullable: false),
                    MotivoBajaId = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BajaActivo", x => x.IdBaja);
                    table.ForeignKey(
                        name: "FK_BajaActivo_MotivoBaja",
                        column: x => x.MotivoBajaId,
                        principalTable: "MotivoBaja",
                        principalColumn: "IdMotivoBaja",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BajaActivo_NoInventario_SubNumero_FechaSubmayor",
                table: "BajaActivo",
                columns: new[] { "NoInventario", "SubNumero", "FechaSubmayor" });

            // Seed data for MotivoBaja
            migrationBuilder.InsertData(
                table: "MotivoBaja",
                columns: new[] { "IdMotivoBaja", "Nombre" },
                values: new object[,]
                {
                    { 1, "Baja Pendiente Materias Primas" },
                    { 2, "Traslado" },
                    { 3, "Baja (Destino Final)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BajaActivo");
            migrationBuilder.DropTable(name: "MotivoBaja");
        }
    }
}
