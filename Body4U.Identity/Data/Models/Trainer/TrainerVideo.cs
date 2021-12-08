namespace Body4U.Identity.Data.Models.Trainer
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Trainer;

    public class TrainerVideo
    {
        public int Id { get; set; }

        [RegularExpression(YoutubeVideoUrlRegex)]
        public string VideoUrl { get; set; }

        public Trainer Trainer { get; set; }

        [Required]
        public int TrainerId { get; set; }
    }
}
