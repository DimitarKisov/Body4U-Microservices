using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Identity.Data.Migrations
{
    public partial class ChangesTrainerImagesData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Folder",
                table: "TrainerImagesDatas");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "TrainerImagesDatas",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "TrainerImagesDatas");

            migrationBuilder.AddColumn<string>(
                name: "Folder",
                table: "TrainerImagesDatas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
