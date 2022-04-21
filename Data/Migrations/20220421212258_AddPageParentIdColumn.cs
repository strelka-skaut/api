using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class AddPageParentIdColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Page",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Page_ParentId",
                table: "Page",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Page_Page_ParentId",
                table: "Page",
                column: "ParentId",
                principalTable: "Page",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Page_Page_ParentId",
                table: "Page");

            migrationBuilder.DropIndex(
                name: "IX_Page_ParentId",
                table: "Page");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Page");
        }
    }
}
