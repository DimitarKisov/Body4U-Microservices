namespace Body4U.Article.Models.Requests.Article
{
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.Article;

    public class EditArticleRequestModel
    {
        [Required]
        public int Id { get; }

        [Required]
        [MinLength(MinTitleLength)]
        [MaxLength(MaxTitleLength)]
        public string Title { get; }

        [Required]
        [MinLength(MinContentLength)]
        [MaxLength(MaxContentLength)]
        public string Content { get; }

        [Required]
        public IFormFile Image { get; }

        [Required]
        public int ArticleType { get; }

        public string Sources { get; }
    }
}
