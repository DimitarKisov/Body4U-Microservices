namespace Body4U.Common.Models.Comment.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class DeleteCommentRequestModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
