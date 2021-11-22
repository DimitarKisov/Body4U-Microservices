namespace Body4U.Article.Models
{
    using Body4U.Article.Data.Models;
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Article;

    public class CreateArticleRequestModel
    {
        [Required]
        [MinLength(MinTitleLength)]
        [MaxLength(MaxTitleLength)]
        public string Title { get; set; }

        [Required]
        public IFormFile Image { get; set; }

        [Required]
        [MinLength(MinContentLength)]
        [MaxLength(MaxContentLength)]
        public string Content { get; set; }

        [Required]
        public ArticleType ArticleType { get; set; }

        public string Sources { get; set; }
    }
}
