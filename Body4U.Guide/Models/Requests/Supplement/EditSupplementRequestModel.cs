namespace Body4U.Guide.Models.Requests.Supplement
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Supplement;

    public class EditSupplementRequestModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(MinNameLength)]
        [MaxLength(MaxNameLength)]
        [RegularExpression(NameRegularExpression)]

        public string Name { get; set; }

        [Required]
        [MinLength(MinDescriptionLength)]
        [MaxLength(MaxDescriptionLength)]
        public string Description { get; set; }

        [Required]
        public int Category { get; set; }
    }
}
