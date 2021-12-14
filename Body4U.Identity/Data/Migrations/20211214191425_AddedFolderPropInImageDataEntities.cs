using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Identity.Data.Migrations
{
    public partial class AddedFolderPropInImageDataEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Folder",
                table: "UserImageDatas",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Folder",
                table: "TrainerImagesDatas",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Folder",
                table: "UserImageDatas");

            migrationBuilder.DropColumn(
                name: "Folder",
                table: "TrainerImagesDatas");
        }
    }
}
