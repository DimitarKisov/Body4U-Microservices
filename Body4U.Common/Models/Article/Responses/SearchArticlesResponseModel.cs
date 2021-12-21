namespace Body4U.Common.Models.Article.Responses
{
    using System.Collections.Generic;

    public class SearchArticlesResponseModel
    {
        public ICollection<ArticleResponseModel> Data { get; set; }

        public int TotalRecords { get; set; }
    }
}
