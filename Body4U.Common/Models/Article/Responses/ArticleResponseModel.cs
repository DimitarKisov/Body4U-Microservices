namespace Body4U.Common.Models.Article.Responses
{
    using System;

    public class ArticleResponseModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string TrainerName { get; set; }

        public DateTime CreatedOn { get; set; }

        public int ArticleType { get; set; }
    }
}
