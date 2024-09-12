using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class PermitirCubiculoNullEnAFT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActFijo_Cubiculos",
                table: "ActFijo");

            migrationBuilder.AlterColumn<Guid>(
                name: "Cubiculo",
                table: "ActFijo",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_ActFijo_Cubiculos",
                table: "ActFijo",
                column: "Cubiculo",
                principalTable: "Cubiculos",
                principalColumn: "IdCubiculo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActFijo_Cubiculos",
                table: "ActFijo");

            migrationBuilder.AlterColumn<Guid>(
                name: "Cubiculo",
                table: "ActFijo",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActFijo_Cubiculos",
                table: "ActFijo",
                column: "Cubiculo",
                principalTable: "Cubiculos",
                principalColumn: "IdCubiculo",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
