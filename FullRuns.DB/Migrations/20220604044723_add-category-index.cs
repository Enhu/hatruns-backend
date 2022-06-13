using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HatCommunityWebsite.DB.Migrations
{
    public partial class addcategoryindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Subcategories");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Subcategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "Subcategories");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "Categories");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Subcategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
