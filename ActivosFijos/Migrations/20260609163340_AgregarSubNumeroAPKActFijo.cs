using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSubNumeroAPKActFijo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ActFijo",
                table: "ActFijo");

            migrationBuilder.AddColumn<int>(
                name: "subNumero",
                table: "ActFijo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActFijo",
                table: "ActFijo",
                columns: new[] { "noInventario", "subNumero", "fechaSubmayor" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ActFijo",
                table: "ActFijo");

            migrationBuilder.DropColumn(
                name: "subNumero",
                table: "ActFijo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActFijo",
                table: "ActFijo",
                columns: new[] { "noInventario", "fechaSubmayor" });
        }
    }
}
