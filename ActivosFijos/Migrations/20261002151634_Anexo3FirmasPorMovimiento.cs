using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class Anexo3FirmasPorMovimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anexo3Firmas",
                columns: table => new
                {
                    NoInventario = table.Column<long>(type: "bigint", nullable: false),
                    SubNumero = table.Column<int>(type: "int", nullable: false),
                    FechaSubmayor = table.Column<DateTime>(type: "datetime", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Fundamentacion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    InformeTecnico = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    NombreTecnico = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    CargoTecnico = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    HechoPor = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    CargoHechoPor = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    TransportadorReceptor = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    CargoTransportadorReceptor = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Comprobante = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anexo3Firmas", x => new { x.NoInventario, x.SubNumero, x.FechaSubmayor });
                });

            migrationBuilder.Sql(@"INSERT INTO TipoMovimientoAFT (idTipoMovimientoAFT, Nombre, Descripcion, Orden, SiActivo)
VALUES ('7C3E1A54-9D82-4B6E-8F17-2A5B9C40D613', 'Traslado externo', 'Traslado del activo fijo tangible hacia otra entidad', 14, 1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM TipoMovimientoAFT WHERE idTipoMovimientoAFT = '7C3E1A54-9D82-4B6E-8F17-2A5B9C40D613';");

            migrationBuilder.DropTable(
                name: "Anexo3Firmas");
        }
    }
}
