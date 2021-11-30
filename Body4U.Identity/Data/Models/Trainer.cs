namespace Body4U.Identity.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class Trainer
    {
        public Trainer()
        {
            this.TrainerImages = new HashSet<TrainerImage>();
            this.TrainerVideos = new HashSet<TrainerVideo>();
        }

        public int Id { get; set; }

        public string Bio { get; set; }

        public string ShortBio { get; set; }

        public string FacebookUrl { get; set; }

        public string InstagramUrl { get; set; }

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

        public ICollection<TrainerImage> TrainerImages { get; set; }

        public ICollection<TrainerVideo> TrainerVideos { get; set; }
    }
}
