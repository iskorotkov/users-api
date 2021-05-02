using Microsoft.EntityFrameworkCore.Migrations;

namespace Models.Migrations
{
    public partial class UsePasswordHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "users",
                newName: "PasswordHash");

            migrationBuilder.AddColumn<string>(
                name: "Salt",
                table: "users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "Password");

            migrationBuilder.DropColumn(
                name: "Salt",
                table: "users");
        }
    }
}
