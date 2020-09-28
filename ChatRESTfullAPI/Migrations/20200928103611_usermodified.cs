using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatRESTfullAPI.Migrations
{
    public partial class usermodified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Login",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "Users",
                nullable: true);
        }
    }
}
