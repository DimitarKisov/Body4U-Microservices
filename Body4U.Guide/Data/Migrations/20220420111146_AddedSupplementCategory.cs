using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Guide.Data.Migrations
{
    public partial class AddedSupplementCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Supplements",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Supplements");
        }
    }
}
