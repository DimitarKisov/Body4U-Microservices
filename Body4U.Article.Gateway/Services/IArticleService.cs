namespace Body4U.Article.Gateway.Services
{
    using Body4U.Common.Models.Article.Responses;
    using Body4U.Common.Models.Favourites.Requests;
    using Body4U.Identity.Models.Favourites.Responses;
    using Refit;
    using System.Threading.Tasks;

    public interface IArticleService
    {
        [Get("/Article/{id}")]
        Task<GetArticleResponseModel> Get(int id);

        [Get("/Article/ArticleExists/{id}")]
        Task<bool> ArticleExists(int id);

        [Post("/Article/Favourites")]
        Task<SearchFavouritesResponseModel> Favourites(SearchFavouritesRequestModel request);
    }
}
