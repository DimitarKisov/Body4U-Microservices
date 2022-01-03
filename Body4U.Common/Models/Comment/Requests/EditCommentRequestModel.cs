namespace Body4U.Common.Models.Comment.Requests
{
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Comment;

    public class EditCommentRequestModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(MinContentLength)]
        [MaxLength(MaxContentLength)]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
