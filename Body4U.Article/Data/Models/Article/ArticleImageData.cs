namespace Body4U.Article.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ArticleImageData
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public string Folder { get; set; }

        public Article Article { get; set; }

        [Required]
        public int ArticleId { get; set; }
    }
}
