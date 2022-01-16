using Microsoft.EntityFrameworkCore.Migrations;

namespace Body4U.Guide.Data.Migrations
{
    public partial class AddedFoodEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Foods",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Protein = table.Column<double>(nullable: false),
                    Carbohydrates = table.Column<double>(nullable: false),
                    Fats = table.Column<double>(nullable: false),
                    Calories = table.Column<double>(nullable: false),
                    FoodCategory = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtherFoodValues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Water = table.Column<double>(nullable: true),
                    Fiber = table.Column<double>(nullable: true),
                    Calcium = table.Column<double>(nullable: true),
                    Magnesium = table.Column<double>(nullable: true),
                    Potassium = table.Column<double>(nullable: true),
                    Zinc = table.Column<double>(nullable: true),
                    Manganese = table.Column<double>(nullable: true),
                    VitaminC = table.Column<double>(nullable: true),
                    VitaminA = table.Column<double>(nullable: true),
                    VitaminE = table.Column<double>(nullable: true),
                    Sugars = table.Column<double>(nullable: true),
                    FoodId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherFoodValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtherFoodValues_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtherFoodValues_FoodId",
                table: "OtherFoodValues",
                column: "FoodId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtherFoodValues");

            migrationBuilder.DropTable(
                name: "Foods");
        }
    }
}
