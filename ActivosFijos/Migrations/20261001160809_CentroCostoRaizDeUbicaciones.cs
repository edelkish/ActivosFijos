using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivosFijos.Migrations
{
    /// <inheritdoc />
    public partial class CentroCostoRaizDeUbicaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locales_Cubiculos",
                table: "Locales");

            migrationBuilder.AddColumn<Guid>(
                name: "CentroCosto",
                table: "Locales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE l SET l.CentroCosto = v.IdCentroCosto
FROM Locales l
INNER JOIN (VALUES
    ('A48628AB-805A-42AE-B388-99C5E13B8890', '07350F7D-D68A-46CD-8E99-AEE26817C92A'),
    ('3EE6EE7F-E2FF-4ED8-B6BD-F700A0C328B5', '5A90EE6B-CB19-45CB-9A1E-0B00A9856083'),
    ('8477658C-480F-4ABE-94A0-889337631F19', '5259B4C7-25AB-4716-98A6-14E383AE91DE'),
    ('7221B99B-B457-4855-A029-8D347E83E184', '9DCC3DD0-03E1-4082-ACB0-F668C424C780'),
    ('AB4A897F-2D8E-4E0E-BFAD-3BAE6977F426', '229470CA-E157-41D1-AF02-5277CEC69864'),
    ('986D7E30-35D4-4F7D-AC38-C37F354E02D5', '07350F7D-D68A-46CD-8E99-AEE26817C92A'),
    ('6ACCE2A4-4299-495A-B746-F33C3AF0EE4D', '9DCC3DD0-03E1-4082-ACB0-F668C424C780'),
    ('0B4D3165-A30B-4127-8104-0B9E8B44CA88', '07350F7D-D68A-46CD-8E99-AEE26817C92A'),
    ('0E71C036-A9F6-4023-9D00-2F02F5101FDE', '86805913-DBB8-4872-ADE4-3C03FBDB21DA'),
    ('FBD059BB-D166-43DF-8C50-81703AF2B52B', '96A03664-4D37-4392-BCF0-AF1877047D8F'),
    ('2AE9F87F-72D4-4EBD-ACD2-B1C586C8DB8D', '11B12187-AC7E-4FA3-ADFF-09E8448322F8'),
    ('425B8570-0FB7-46BF-95D4-928DF7AA70F9', '04D8A3BA-D5F7-48EB-83E9-05EB3974B0E9'),
    ('DAF8B65B-2F6E-44C6-BF1C-C19D9DBEB353', '79673F44-E19E-49AE-BDD3-E3523391C991'),
    ('4C4D33A5-B3CA-4ACC-9A54-41C41B6BDFEF', '0FF9BF65-A028-4174-A879-7B77AF350E40'),
    ('20A5D548-110C-46AD-97E7-0CFA9DCBAC2B', '229470CA-E157-41D1-AF02-5277CEC69864'),
    ('04D09602-5BC1-464A-8D2C-9D89BD3EE89C', '229470CA-E157-41D1-AF02-5277CEC69864'),
    ('2EEA329C-1AB1-4B77-85A6-3C952D0D0FDA', '9DCC3DD0-03E1-4082-ACB0-F668C424C780'),
    ('BA561456-415C-495D-9338-BD645F61F1E8', '96A03664-4D37-4392-BCF0-AF1877047D8F'),
    ('5A0D9BBD-E286-463D-B195-000AD44EE1B2', '229470CA-E157-41D1-AF02-5277CEC69864'),
    ('88CD569D-E8ED-43E7-80E0-0D65F5351E93', '229470CA-E157-41D1-AF02-5277CEC69864'),
    ('515DDC08-1EFB-4626-8770-D5B566292B53', '96A03664-4D37-4392-BCF0-AF1877047D8F'),
    ('036164D8-76C3-4287-8F26-46635CBC07FD', '96A03664-4D37-4392-BCF0-AF1877047D8F')
) AS v(IdLocal, IdCentroCosto) ON v.IdLocal = l.IdLocal;");

            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM Locales WHERE CentroCosto IS NULL)
BEGIN
    THROW 51000, 'Hay locales sin CentroCosto asignado: revise el mapeo antes de continuar.', 1;
END");

            migrationBuilder.AlterColumn<Guid>(
                name: "CentroCosto",
                table: "Locales",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Cubiculo",
                table: "Locales");

            migrationBuilder.DropTable(
                name: "Cubiculos");

            migrationBuilder.CreateIndex(
                name: "IX_Locales_CentroCosto",
                table: "Locales",
                column: "CentroCosto");

            migrationBuilder.AddForeignKey(
                name: "FK_Locales_CentroCosto",
                table: "Locales",
                column: "CentroCosto",
                principalTable: "CentroCosto",
                principalColumn: "idCentroCosto",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locales_CentroCosto",
                table: "Locales");

            migrationBuilder.DropIndex(
                name: "IX_Locales_CentroCosto",
                table: "Locales");

            migrationBuilder.DropColumn(
                name: "CentroCosto",
                table: "Locales");

            migrationBuilder.AddColumn<Guid>(
                name: "Cubiculo",
                table: "Locales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cubiculos",
                columns: table => new
                {
                    IdCubiculo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Apodo = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cubiculos", x => x.IdCubiculo);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locales_Cubiculo",
                table: "Locales",
                column: "Cubiculo");

            migrationBuilder.AddForeignKey(
                name: "FK_Locales_Cubiculos",
                table: "Locales",
                column: "Cubiculo",
                principalTable: "Cubiculos",
                principalColumn: "IdCubiculo");
        }
    }
}
