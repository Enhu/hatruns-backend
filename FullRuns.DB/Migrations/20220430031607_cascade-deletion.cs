using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HatCommunityWebsite.DB.Migrations
{
    public partial class cascadedeletion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RunVariables_Runs_RunId",
                table: "RunVariables");

            migrationBuilder.AddForeignKey(
                name: "FK_RunVariables_Runs_RunId",
                table: "RunVariables",
                column: "RunId",
                principalTable: "Runs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RunVariables_Runs_RunId",
                table: "RunVariables");

            migrationBuilder.AddForeignKey(
                name: "FK_RunVariables_Runs_RunId",
                table: "RunVariables",
                column: "RunId",
                principalTable: "Runs",
                principalColumn: "Id");
        }
    }
}
