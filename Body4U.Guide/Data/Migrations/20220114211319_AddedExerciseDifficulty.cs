using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Guide.Data.Migrations
{
    public partial class AddedExerciseDifficulty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExerciseDifficulty",
                table: "Exercises",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExerciseDifficulty",
                table: "Exercises");
        }
    }
}
