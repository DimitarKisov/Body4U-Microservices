namespace Body4U.Common.Models.Article.Requests
{
    public class SearchArticlesRequestModel : SearchModel
    {
        public string Title { get; set; }

        public int ArticleType { get; set; }

        public string TrainerName { get; set; }
    }
}
