using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatRESTfullAPI.Migrations
{
    public partial class chatModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrstUsrId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ScndUsrId",
                table: "Chats");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FrstUsrId",
                table: "Chats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScndUsrId",
                table: "Chats",
                nullable: false,
                defaultValue: 0);
        }
    }
}
