namespace Body4U.Identity.Data.Models.Trainer
{
    using System.ComponentModel.DataAnnotations;

    public class TrainerImage
    {
        public int Id { get; set; }

        public byte[] Image { get; set; }

        public Trainer Trainer { get; set; }

        [Required]
        public int TrainerId { get; set; }
    }
}
