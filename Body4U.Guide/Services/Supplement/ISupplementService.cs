namespace Body4U.Guide.Services.Supplement
{
    using Body4U.Common;
    using Body4U.Guide.Models.Requests.Supplement;
    using Body4U.Guide.Models.Responses.Supplement;
    using System.Threading.Tasks;

    public interface ISupplementService
    {
        Task<Result<CreateSupplementResponseModel>> Create(CreateSupplementRequestModel request);
    }
}
