namespace Body4U.Article.Models.Requests.Trainer
{
    using System.ComponentModel.DataAnnotations;

    public class DeleteImageRequestModel
    {
        [Required]
        public int TrainerId { get; set; }

        //[Required]
        //public string ImageId { get; set; }
    }
}
