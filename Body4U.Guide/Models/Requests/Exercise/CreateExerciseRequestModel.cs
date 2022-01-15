namespace Body4U.Guide.Models.Requests.Exercise
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Exercise;

    public class CreateExerciseRequestModel
    {
        [Required]
        [MinLength(MinNameLength)]
        [MaxLength(MaxNameLength)]
        public string Name { get; set; }

        [MaxLength(MaxDescriptionLength)]
        public string Description { get; set; }

        [Required]
        public int ExerciseType { get; set; }

        [Required]
        public int ExerciseDifficulty { get; set; }
    }
}
