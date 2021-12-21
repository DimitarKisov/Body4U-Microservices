namespace Body4U.Article.Services.Article
{
    using Body4U.Article.Models.Requests.Article;
    using Body4U.Common;
    using Body4U.Common.Models.Article.Requests;
    using Body4U.Common.Models.Article.Responses;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IArticleService
    {
        Task<Result<int>> Create(CreateArticleRequestModel request);

        Task<Result> Edit(EditArticleRequestModel request);

        Task<Result> Delete(DeleteArticleRequestModel request);

        Task<Result<GetArticleResponseModel>> Get(int id);

        Task<Result<SearchArticlesResponseModel>> Search(SearchArticlesRequestModel request);

        Task<Result<List<string>>> AutocompleteArticleTitle(string term);
    }
}
