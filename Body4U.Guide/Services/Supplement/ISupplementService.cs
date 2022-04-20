namespace Body4U.Guide.Services.Supplement
{
    using Body4U.Common;
    using Body4U.Guide.Models.Requests.Supplement;
    using Body4U.Guide.Models.Responses.Supplement;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISupplementService
    {
        Task<Result<CreateSupplementResponseModel>> Create(CreateSupplementRequestModel request);

        Task<Result<GetSupplementResponseModel>> Get(int id);

        Task<Result<SearchSupplementsResponseModel>> Search(SearchSupplementsRequestModel request);

        Task<Result> Edit(EditSupplementRequestModel request);

        Task<Result> Delete(int id);

        Task<Result<Dictionary<int, string>>> AutocompleteSupplementName(string term);
    }
}
