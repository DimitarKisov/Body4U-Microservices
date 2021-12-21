namespace Body4U.Article.Models.Requests.Article
{
    using System.ComponentModel.DataAnnotations;

    public class DeleteArticleRequestModel
    {
        [Required]
        public int Id { get; set; }
    }
}
