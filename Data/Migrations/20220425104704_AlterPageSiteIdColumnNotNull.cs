using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class AlterPageSiteIdColumnNotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Page_Site_SiteId",
                table: "Page");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteId",
                table: "Page",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
            migrationBuilder.AlterColumn<Guid>(
                name: "SiteId",
                table: "Page",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                defaultValue: null);

            migrationBuilder.AddForeignKey(
                name: "FK_Page_Site_SiteId",
                table: "Page",
                column: "SiteId",
                principalTable: "Site",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Page_Site_SiteId",
                table: "Page");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteId",
                table: "Page",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Page_Site_SiteId",
                table: "Page",
                column: "SiteId",
                principalTable: "Site",
                principalColumn: "Id");
        }
    }
}
