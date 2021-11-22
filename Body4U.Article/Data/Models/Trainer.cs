namespace Body4U.Article.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Trainer;

    public class Trainer
    {
        public Trainer()
        {
            this.Articles = new HashSet<Article>();
        }

        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(MinBioLength)]
        [MaxLength(MaxBioLength)]
        public string Bio { get; set; }

        [Required]
        [MinLength(MinShortBioLength)]
        [MaxLength(MaxShortBioLength)]
        public string ShortBio { get; set; }

        [RegularExpression(FacebookUrlRegex)]
        public string FacebookUrl { get; set; }

        [RegularExpression(InstragramUrlRegex)]
        public string InstagramUrl { get; set; }

        [RegularExpression(YoutubeChannelUrlRegex)]
        public string YoutubeChannelUrl { get; set; }

        [DefaultValue(false)]
        public bool IsReadyToVisualize { get; set; }

        [DefaultValue(false)]
        public bool IsReadyToWrite { get; set; }

        [Required]
        public DateTime CreatedOn { get; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        public ICollection<Article> Articles { get; set; }
    }
}
