namespace Body4U.Article.Services
{
    using Body4U.Article.Data;
    using Body4U.Article.Models;
    using Body4U.Common;

    public class ArticleService : IArticleService
    {
        private readonly ArticleDbContext dbContext;

        public ArticleService(ArticleDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Result<int> Create(CreateArticleRequestModel request)
        {
            throw new System.NotImplementedException();
        }
    }
}
