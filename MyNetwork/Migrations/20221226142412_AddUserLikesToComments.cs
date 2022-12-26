using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNetwork.Migrations
{
    public partial class AddUserLikesToComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserLikes",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserLikes",
                table: "Comments");
        }
    }
}
