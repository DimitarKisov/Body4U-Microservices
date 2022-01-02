namespace Body4U.Article.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Comment;

    public class Comment
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(MinContentLength)]
        [MaxLength(MaxContentLength)]
        public string Content { get; set; }

        public DateTime CreatedOn { get; set; }

        [Required]
        public int ArticleId { get; set; }

        public Article Article { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
    }
}
