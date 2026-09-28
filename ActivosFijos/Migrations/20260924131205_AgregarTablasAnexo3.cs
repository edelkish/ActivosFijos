using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablasAnexo3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parametro",
                columns: table => new
                {
                    idParametro = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Valor = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Grupo = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametro", x => x.idParametro);
                });

            migrationBuilder.CreateTable(
                name: "TipoMovimientoAFT",
                columns: table => new
                {
                    idTipoMovimientoAFT = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    SiActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoMovimientoAFT", x => x.idTipoMovimientoAFT);
                });

            migrationBuilder.Sql(@"
                INSERT INTO TipoMovimientoAFT (idTipoMovimientoAFT, Nombre, Descripcion, Orden, SiActivo)
                VALUES
                    (NEWID(), N'Compra AFT Nuevo',      N'Compra de un Activo Fijo Tangible nuevo', 1, 1),
                    (NEWID(), N'Compra AFT de Uso',     N'Compra de un Activo Fijo Tangible de uso', 2, 1),
                    (NEWID(), N'Traspaso recibido',     N'Traspaso recibido de otra entidad', 3, 1),
                    (NEWID(), N'Ajuste Inv. Alta',      N'Ajuste de inventario por alta', 4, 1),
                    (NEWID(), N'Venta',                 N'Venta del activo fijo tangible', 5, 1),
                    (NEWID(), N'Traslado Interno',      N'Traslado interno entre áreas', 6, 1),
                    (NEWID(), N'Activo-Ocioso',         N'Activo fijo tangible ocioso', 7, 1),
                    (NEWID(), N'Retiro',                N'Retiro del activo fijo tangible', 8, 1),
                    (NEWID(), N'Pérdida',               N'Pérdida del activo fijo tangible', 9, 1),
                    (NEWID(), N'Traspaso efect.',       N'Traspaso efectuado a otra entidad', 10, 1),
                    (NEWID(), N'Ajuste Baja',           N'Ajuste de inventario por baja', 11, 1),
                    (NEWID(), N'Enviado a reparar',     N'Activo fijo tangible enviado a reparar', 12, 1),
                    (NEWID(), N'Ocioso Activo',         N'Activo fijo tangible reincorporado a uso', 13, 1);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO Parametro (idParametro, Nombre, Valor, Grupo)
                VALUES
                    (NEWID(), N'Entidad',       NULL, N'Anexo3'),
                    (NEWID(), N'Dirección',     NULL, N'Anexo3'),
                    (NEWID(), N'UEB',           NULL, N'Anexo3'),
                    (NEWID(), N'Dirección UEB', NULL, N'Anexo3');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parametro");

            migrationBuilder.DropTable(
                name: "TipoMovimientoAFT");
        }
    }
}
