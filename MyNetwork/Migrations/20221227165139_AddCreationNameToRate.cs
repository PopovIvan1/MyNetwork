using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNetwork.Migrations
{
    public partial class AddCreationNameToRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreationName",
                table: "Rates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationName",
                table: "Rates");
        }
    }
}
