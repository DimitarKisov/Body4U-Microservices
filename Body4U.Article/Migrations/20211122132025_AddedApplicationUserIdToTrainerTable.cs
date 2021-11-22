using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Article.Migrations
{
    public partial class AddedApplicationUserIdToTrainerTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Trainer",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Trainer");
        }
    }
}
