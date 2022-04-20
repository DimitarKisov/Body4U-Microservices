namespace Body4U.Guide.Services.Food
{
    using Body4U.Common;
    using Body4U.Guide.Models.Requests.Food;
    using Body4U.Guide.Models.Responses.Food;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFoodService
    {
        Task<Result<CreateFoodResponseModel>> Create(CreateFoodRequestModel request);

        Task<Result<GetFoodResponseModel>> Get(int id);

        Task<Result<SearchFoodsResponseModel>> Search(SearchFoodsRequestModel request);

        //Search
        //Edit
        //Delete
        Task<Result<Dictionary<int, string>>> AutocompleteFoodName(string term);
    }
}
