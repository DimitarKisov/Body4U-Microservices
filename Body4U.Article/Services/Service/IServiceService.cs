namespace Body4U.Article.Services.Service
{
    using Body4U.Article.Models.Requests.Service;
    using Body4U.Article.Models.Responses.Services;
    using Body4U.Common;
    using Body4U.Common.Models.Service;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServiceService
    {
        Task<Result<int>> Create(CreateServiceRequestModel request);

        Task<Result<List<AllServicesResponseModel>>> All(int trainerId);

        Task<Result<GetServiceResponseModel>> Get(int id);

        Task<Result> Edit(EditServiceRequestModel request);
    }
}
