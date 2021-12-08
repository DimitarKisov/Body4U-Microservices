using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Identity.Data.Migrations
{
    public partial class AddedImageDataUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "IdentityUsers");

            migrationBuilder.AddColumn<string>(
                name: "UserImageDataId",
                table: "IdentityUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserImageDatas",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Folder = table.Column<string>(nullable: false),
                    ApplicationUserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserImageDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserImageDatas_IdentityUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "IdentityUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserImageDatas_ApplicationUserId",
                table: "UserImageDatas",
                column: "ApplicationUserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserImageDatas");

            migrationBuilder.DropColumn(
                name: "UserImageDataId",
                table: "IdentityUsers");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePicture",
                table: "IdentityUsers",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
