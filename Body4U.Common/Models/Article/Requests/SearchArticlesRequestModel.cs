namespace Body4U.Common.Models.Article.Requests
{
    public class SearchArticlesRequestModel
    {
        public string Title { get; set; }

        public int ArticleType { get; set; }

        public string TrainerName { get; set; }

        public string SortBy { get; set; }

        public string OrderBy { get; set; }

        public int PageIndex { get; set; } = 0;

        public int PageSize { get; set; } = 10;
    }
}
