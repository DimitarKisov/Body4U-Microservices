namespace Body4U.Guide.Services.Food
{
    using Body4U.Common;
    using Body4U.Guide.Models.Requests.Food;
    using Body4U.Guide.Models.Responses.Food;
    using System.Threading.Tasks;

    public interface IFoodService
    {
        Task<Result<CreateFoodResponseModel>> Create(CreateFoodRequestModel request);

        Task<Result<GetFoodResponseModel>> Get(int id);
    }
}
