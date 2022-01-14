namespace Body4U.Guide.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Exercise;

    public class Exercise
    {
        public int Id { get; set; }

        [Required]
        [MinLength(MinNameLength)]
        [MaxLength(MaxNameLength)]
        public string Name { get; set; }

        [MaxLength(MaxDescriptionLength)]
        public string Description { get; set; }

        [Required]
        public ExerciseType ExerciseType { get; set; }
    }
}
