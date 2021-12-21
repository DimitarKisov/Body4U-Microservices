using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Article.Data.Migrations
{
    public partial class AddedFullNameInTrainer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Trainers",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Trainers");
        }
    }
}
