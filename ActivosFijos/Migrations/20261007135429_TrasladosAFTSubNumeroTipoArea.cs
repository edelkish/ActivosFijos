using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class TrasladosAFTSubNumeroTipoArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "areaDestino",
                table: "TrasladosAFT",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "subNumero",
                table: "TrasladosAFT",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "tipoMovimiento",
                table: "TrasladosAFT",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosAFT_areaDestino",
                table: "TrasladosAFT",
                column: "areaDestino");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosAFT_tipoMovimiento",
                table: "TrasladosAFT",
                column: "tipoMovimiento");

            migrationBuilder.AddForeignKey(
                name: "FK_TrasladosAFT_AreaDeResponsabilidad",
                table: "TrasladosAFT",
                column: "areaDestino",
                principalTable: "AreaDeResponsabilidad",
                principalColumn: "idArea");

            migrationBuilder.AddForeignKey(
                name: "FK_TrasladosAFT_TipoMovimientoAFT",
                table: "TrasladosAFT",
                column: "tipoMovimiento",
                principalTable: "TipoMovimientoAFT",
                principalColumn: "idTipoMovimientoAFT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrasladosAFT_AreaDeResponsabilidad",
                table: "TrasladosAFT");

            migrationBuilder.DropForeignKey(
                name: "FK_TrasladosAFT_TipoMovimientoAFT",
                table: "TrasladosAFT");

            migrationBuilder.DropIndex(
                name: "IX_TrasladosAFT_areaDestino",
                table: "TrasladosAFT");

            migrationBuilder.DropIndex(
                name: "IX_TrasladosAFT_tipoMovimiento",
                table: "TrasladosAFT");

            migrationBuilder.DropColumn(
                name: "areaDestino",
                table: "TrasladosAFT");

            migrationBuilder.DropColumn(
                name: "subNumero",
                table: "TrasladosAFT");

            migrationBuilder.DropColumn(
                name: "tipoMovimiento",
                table: "TrasladosAFT");
        }
    }
}
