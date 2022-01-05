namespace Body4U.Article.Models.Requests.Service
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Service;

    public class CreateServiceRequestModel
    {
        [Required]
        [MinLength(MinNameLength)]
        [MaxLength(MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [MinLength(MinDescriptionLength)]
        [MaxLength(MaxDescriptionLength)]
        public string Description { get; set; }

        [Range(typeof(decimal), MinPrice, MaxPrice)]
        public decimal? Price { get; set; }

        [Required]
        public int ServiceType { get; set; }

        [Required]
        public int ServiceDifficulty { get; set; }
    }
}
