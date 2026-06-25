using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanMuestreoAnual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanMuestreoAnual",
                columns: table => new
                {
                    Anno = table.Column<int>(type: "int", nullable: false),
                    CentroCosto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AreaResponsabilidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalActivos = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PlanMuestreoAnual", x => new { x.Anno, x.CentroCosto, x.AreaResponsabilidad });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanMuestreoAnual");
        }
    }
}
