using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class ReceptorNombreAreaAnexo3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceptorArea",
                table: "Anexo3Firmas",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceptorNombre",
                table: "Anexo3Firmas",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceptorArea",
                table: "Anexo3Firmas");

            migrationBuilder.DropColumn(
                name: "ReceptorNombre",
                table: "Anexo3Firmas");
        }
    }
}
