namespace Body4U.Common.Models.Comment.Requests
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
