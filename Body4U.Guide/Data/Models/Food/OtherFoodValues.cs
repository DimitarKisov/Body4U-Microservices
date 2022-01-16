﻿namespace Body4U.Guide.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class OtherFoodValues
    {
        public int Id { get; set; }

        [Range(0, 100)]
        public double? Water { get; set; }

        [Range(0, 100)]
        public double? Fiber { get; set; }

        [Range(0, 500)]
        public double? Calcium { get; set; }

        [Range(0, 500)]
        public double? Magnesium { get; set; }

        [Range(0, 500)]
        public double? Potassium { get; set; }

        [Range(0, 500)]
        public double? Zinc { get; set; }

        [Range(0, 500)]
        public double? Manganese { get; set; }

        [Range(0, 500)]
        public double? VitaminC { get; set; }

        [Range(0, 500)]
        public double? VitaminA { get; set; }

        [Range(0, 500)]
        public double? VitaminE { get; set; }

        [Range(0, 100)]
        public double? Sugars { get; set; }

        public Food Food { get; set; }

        [Required]
        public int FoodId { get; set; }
    }
}
