namespace Body4U.Identity.Data.Models.Trainer
{
    using System.ComponentModel.DataAnnotations;

    public class TrainerImageData
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public string Folder { get; set; }

        public Trainer Trainer { get; set; }

        [Required]
        public int TrainerId { get; set; }
    }
}
