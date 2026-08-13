using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class AddIdUthMuestreoUTH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_MuestreoMensualDiezPorCientoUTH_Discriminante",
                table: "MuestreoMensualDiezPorCientoUTH");

            migrationBuilder.AddColumn<Guid>(
                name: "IdUth",
                table: "MuestreoMensualDiezPorCientoUTH",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "UQ_MuestreoMensualDiezPorCientoUTH_IdUth",
                table: "MuestreoMensualDiezPorCientoUTH",
                columns: new[] { "Anno", "Mes", "IdUth" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_MuestreoMensualDiezPorCientoUTH_IdUth",
                table: "MuestreoMensualDiezPorCientoUTH");

            migrationBuilder.DropColumn(
                name: "IdUth",
                table: "MuestreoMensualDiezPorCientoUTH");

            migrationBuilder.CreateIndex(
                name: "UQ_MuestreoMensualDiezPorCientoUTH_Discriminante",
                table: "MuestreoMensualDiezPorCientoUTH",
                columns: new[] { "Anno", "Mes", "Codigo", "Responsable", "CentroCosto", "AreaResponsabilidad" },
                unique: true);
        }
    }
}
