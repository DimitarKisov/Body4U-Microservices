using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Identity.Data.Migrations
{
    public partial class AddedAddedInInFavouriteEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AddedIn",
                table: "Favourites",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedIn",
                table: "Favourites");
        }
    }
}
