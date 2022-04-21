using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class AddPageRoleColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Page",
                type: "text",
                nullable: false,
                defaultValue: "");
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Page",
                oldDefaultValue: "",
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Page");
        }
    }
}
