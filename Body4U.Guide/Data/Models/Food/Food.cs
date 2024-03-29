﻿namespace Body4U.Guide.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Food;

    public class Food
    {
        public int Id { get; set; }

        [Required]
        [MinLength(MinNameLength)]
        [MaxLength(MaxNameLength)]
        [RegularExpression("^([a-z ,A-Z]+|[а-я ,А-Я]+)$")]
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
        public FoodCategory FoodCategory { get; set; }

        public OtherFoodValues OtherValues { get; set; }
    }
}
