namespace Body4U.Article.Gateway.Services.Article
{
    using Body4U.Common.Models.Article.Requests;
    using Refit;
    using System.Threading.Tasks;

    public interface IArticleService
    {
        [Post("/Article/Create")]
        Task<int> Create(CreateArticleRequestModel request);
    }
}
