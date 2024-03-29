﻿namespace Body4U.Article.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Article;

    public class Article
    {
        public Article()
        {
            this.Comments = new HashSet<Comment>();
        }

        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(MinTitleLength)]
        [MaxLength(MaxTitleLength)]
        public string Title { get; set; }

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

        public ArticleImageData ArticleImageData { get; set; }

        public Trainer Trainer { get; set; }

        [Required]
        public int TrainerId { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}
