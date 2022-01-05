namespace Body4U.Article.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Service;

    public class Service
    {
        [Required]
        public int Id { get; set; }

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
        public ServiceType ServiceType { get; set; }

        [Required]
        public ServiceDifficulty ServiceDifficulty { get; set; }

        [Required]
        public int TrainerId { get; set; }

        public virtual Trainer Trainer { get; set; }
    }
}
