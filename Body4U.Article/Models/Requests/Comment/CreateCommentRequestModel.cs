namespace Body4U.Article.Models.Requests.Comment
{
    using System.ComponentModel.DataAnnotations;

    public class CreateCommentRequestModel
    {
        [Required]
        public int ArticleId { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
