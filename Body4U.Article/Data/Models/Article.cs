namespace Body4U.Article.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Article;

    public class Article
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(MinTitleLength)]
        [MaxLength(MaxTitleLength)]
        public string Title { get; set; }

        [Required]
        public byte[] Image { get; set; }

        [Required]
        [MinLength(MinContentLength)]
        [MaxLength(MaxContentLength)]
        public string Content { get; set; }

        public string Sources { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }

        [Required]
        public ArticleType ArticleType { get; set; }

        public Trainer Trainer { get; set; }

        public int TrainerId { get; set; }
    }
}
