using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class DropLayoutTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Site_Layout_LayoutId",
                table: "Site");

            migrationBuilder.DropTable(
                name: "Layout");

            migrationBuilder.DropIndex(
                name: "IX_Site_LayoutId",
                table: "Site");

            migrationBuilder.DropColumn(
                name: "LayoutId",
                table: "Site");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LayoutId",
                table: "Site",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Layout",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layout", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Site_LayoutId",
                table: "Site",
                column: "LayoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_Site_Layout_LayoutId",
                table: "Site",
                column: "LayoutId",
                principalTable: "Layout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
