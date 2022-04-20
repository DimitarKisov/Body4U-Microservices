namespace Body4U.Article.Services.Article
{
    using Body4U.Article.Models.Requests.Article;
    using Body4U.Article.Models.Responses.Article;
    using Body4U.Common;
    using Body4U.Common.Models.Article.Requests;
    using Body4U.Common.Models.Article.Responses;
    using Body4U.Common.Models.Favourites.Requests;
    using Body4U.Identity.Models.Favourites.Responses;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IArticleService
    {
        Task<Result<CreateArticleResponseModel>> Create(CreateArticleRequestModel request);

        Task<Result> Edit(EditArticleRequestModel request);

        Task<Result> Delete(DeleteArticleRequestModel request);

        Task<Result<GetArticleResponseModel>> Get(int id);

        Task<Result<SearchArticlesResponseModel>> Search(SearchArticlesRequestModel request);

        Task<Result<Dictionary<int, string>>> AutocompleteArticleTitle(string term);

        Task<Result<bool>> ArticleExists(int id);

        Task<Result<SearchFavouritesResponseModel>> Favourites(SearchFavouritesRequestModel request);
    }
}
