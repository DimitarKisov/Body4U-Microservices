namespace Body4U.Article.Gateway.Services
{
    using Body4U.Common.Models.Article.Responses;
    using Body4U.Common.Models.Comment.Requests;
    using Refit;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ICommentService
    {
        [Post("/Comment/Search")]
        Task<List<SearchCommentsResponseModel>> Search(SearchCommentsRequestModel request);
    }
}
