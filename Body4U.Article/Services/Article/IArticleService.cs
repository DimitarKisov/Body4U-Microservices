namespace Body4U.Article.Services.Article
{
    using Body4U.Common;
    using Body4U.Common.Models.Article.Requests;
    using Refit;
    using System.Threading.Tasks;

    public interface IArticleService
    {
        Task<Result<int>> Create(CreateArticleRequestModel request);
    }
}
