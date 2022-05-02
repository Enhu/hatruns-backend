using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HatCommunityWebsite.DB.Migrations
{
    public partial class addrejectedreason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                table: "Runs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedReason",
                table: "Runs");
        }
    }
}
