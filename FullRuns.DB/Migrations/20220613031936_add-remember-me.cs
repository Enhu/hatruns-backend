using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HatCommunityWebsite.DB.Migrations
{
    public partial class addrememberme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RememberLogIn",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RememberLogIn",
                table: "Users");
        }
    }
}
