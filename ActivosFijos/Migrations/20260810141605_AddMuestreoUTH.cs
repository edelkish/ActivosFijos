using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class AddMuestreoUTH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MuestreoMensualDiezPorCientoUTH",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Anno = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Responsable = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CentroCosto = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AreaResponsabilidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(2000)", unicode: false, maxLength: 2000, nullable: true),
                    Valor = table.Column<decimal>(type: "money", nullable: true),
                    FechaMuestreo = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    SiExiste = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((0))"),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuestreoMensualDiezPorCientoUTH", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanMuestreoAnualUTH",
                columns: table => new
                {
                    Anno = table.Column<int>(type: "int", nullable: false),
                    CentroCosto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AreaResponsabilidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalUtiles = table.Column<int>(type: "int", nullable: false),
                    Ene = table.Column<int>(type: "int", nullable: false),
                    Feb = table.Column<int>(type: "int", nullable: false),
                    Mar = table.Column<int>(type: "int", nullable: false),
                    Abr = table.Column<int>(type: "int", nullable: false),
                    May = table.Column<int>(type: "int", nullable: false),
                    Jun = table.Column<int>(type: "int", nullable: false),
                    Jul = table.Column<int>(type: "int", nullable: false),
                    Ago = table.Column<int>(type: "int", nullable: false),
                    Sep = table.Column<int>(type: "int", nullable: false),
                    Oct = table.Column<int>(type: "int", nullable: false),
                    Nov = table.Column<int>(type: "int", nullable: false),
                    Dic = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanMuestreoAnualUTH", x => new { x.Anno, x.CentroCosto, x.AreaResponsabilidad });
                });

            migrationBuilder.CreateIndex(
                name: "UQ_MuestreoMensualDiezPorCientoUTH_Discriminante",
                table: "MuestreoMensualDiezPorCientoUTH",
                columns: new[] { "Anno", "Mes", "Codigo", "Responsable", "CentroCosto", "AreaResponsabilidad" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanMuestreoAnualUTH");

            migrationBuilder.DropTable(
                name: "MuestreoMensualDiezPorCientoUTH");
        }
    }
}
