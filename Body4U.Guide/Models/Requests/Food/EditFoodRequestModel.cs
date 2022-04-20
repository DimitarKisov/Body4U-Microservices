namespace Body4U.Guide.Models.Requests.Food
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Food;

    public class EditFoodRequestModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(MinNameLength)]
        [MaxLength(MaxNameLength)]
        [RegularExpression(NameRegularExpression)]
        public string Name { get; set; }

        [Required]
        [Range(0, 100)]
        public double Protein { get; set; }

        [Required]
        [Range(0, 100)]
        public double Carbohydrates { get; set; }

        [Required]
        [Range(0, 100)]
        public double Fats { get; set; }

        [Required]
        [Range(0, 1000)]
        public double Calories { get; set; }

        [Required]
        public int FoodCategory { get; set; }

        public EditOtherFoodValuesRequestModel OtherFoodValues { get; set; }
    }
}
