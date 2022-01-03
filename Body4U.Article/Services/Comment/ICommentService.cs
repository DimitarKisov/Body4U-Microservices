namespace Body4U.Article.Services.Comment
{
    using Body4U.Article.Models.Requests.Comment;
    using Body4U.Common;
    using Body4U.Common.Models.Comment.Requests;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ICommentService
    {
        Task<Result> Create(CreateCommentRequestModel request);

        Task<Result> Edit(EditCommentRequestModel request);

        Task<Result> Delete(DeleteCommentRequestModel request);

        Task<Result<List<SearchCommentsResponseModel>>> Search(SearchCommentsRequestModel request);
    }
}
