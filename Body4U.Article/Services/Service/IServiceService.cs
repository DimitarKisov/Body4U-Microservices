namespace Body4U.Article.Services.Service
{
    using Body4U.Article.Models.Requests.Service;
    using Body4U.Article.Models.Responses.Services;
    using Body4U.Common;
    using System.Threading.Tasks;

    public interface IServiceService
    {
        Task<Result<int>> Create(CreateServiceRequestModel request);

        Task<Result<GetServiceResponseModel>> Get(int id);
    }
}
