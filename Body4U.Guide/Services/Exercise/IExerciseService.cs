namespace Body4U.Guide.Services.Exercise
{
    using Body4U.Common;
    using Body4U.Guide.Models.Requests.Exercise;
    using Body4U.Guide.Models.Responses.Exercise;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IExerciseService
    {
        Task<Result<CreateExerciseResponseModel>> Create(CreateExerciseRequestModel request);

        Task<Result<GetExerciceResponseModel>> Get(int id);

        Task<Result<SearchExercisesResponseModel>> Search(SearchExercisesRequestModel request);

        Task<Result> Edit(EditExerciseRequestModel request);

        Task<Result> Delete(int id);

        Task<Result<Dictionary<int, string>>> AutocompleteExerciseName(string term);
    }
}
