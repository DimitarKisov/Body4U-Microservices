using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Identity.Data.Migrations
{
    public partial class RemovedUserImageDataIdFromIdentityUsersAndAddedTrainerImageData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainerImages");

            migrationBuilder.DropColumn(
                name: "UserImageDataId",
                table: "IdentityUsers");

            migrationBuilder.CreateTable(
                name: "TrainerImagesDatas",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Folder = table.Column<string>(nullable: false),
                    TrainerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerImagesDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainerImagesDatas_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainerImagesDatas_TrainerId",
                table: "TrainerImagesDatas",
                column: "TrainerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainerImagesDatas");

            migrationBuilder.AddColumn<string>(
                name: "UserImageDataId",
                table: "IdentityUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainerImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Image = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    TrainerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainerImages_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainerImages_TrainerId",
                table: "TrainerImages",
                column: "TrainerId");
        }
    }
}
