namespace Body4U.Article.Models.Requests.Trainer
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Trainer;

    public class EditMyTrainerProfileRequestModel
    {
        [Required]
        public int Id { get; set; }

        [MinLength(MinBioLength)]
        [MaxLength(MaxBioLength)]
        public string Bio { get; set; }

        [MinLength(MinShortBioLength)]
        [MaxLength(MaxShortBioLength)]
        public string ShortBio { get; set; }

        [RegularExpression(FacebookUrlRegex)]
        public string FacebookUrl { get; set; }

        [RegularExpression(InstragramUrlRegex)]
        public string InstagramUrl { get; set; }

        [RegularExpression(YoutubeChannelUrlRegex)]
        public string YoutubeChannelUrl { get; set; }
    }
}
