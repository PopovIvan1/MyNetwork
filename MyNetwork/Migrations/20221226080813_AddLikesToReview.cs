using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNetwork.Migrations
{
    public partial class AddLikesToReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Likes",
                table: "Reviews");
        }
    }
}
