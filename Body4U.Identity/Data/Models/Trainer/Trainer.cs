namespace Body4U.Identity.Data.Models.Trainer
{
    using Body4U.Identity.Data.Models.Identity;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Trainer;

    public class Trainer
    {
        public Trainer()
        {
            this.TrainerImagesDatas = new HashSet<TrainerImageData>();
            this.TrainerVideos = new HashSet<TrainerVideo>();
        }

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

        [DefaultValue(false)]
        public bool IsReadyToVisualize { get; set; }

        [DefaultValue(false)]
        public bool IsReadyToWrite { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        public ICollection<TrainerImageData> TrainerImagesDatas { get; set; }

        public ICollection<TrainerVideo> TrainerVideos { get; set; }
    }
}
