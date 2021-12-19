namespace Body4U.Article.Models.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class DeleteArticleRequestModel
    {
        [Required]
        public int Id { get; set; }
    }
}
