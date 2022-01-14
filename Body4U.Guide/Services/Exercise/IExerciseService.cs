namespace Body4U.Guide.Services.Exercise
{
    using Body4U.Common;
    using Body4U.Guide.Models.Requests.Exercise;
    using Body4U.Guide.Models.Responses.Exercise;
    using System.Threading.Tasks;

    public interface IExerciseService
    {
        Task<Result<CreateExerciseResponseModel>> Create(CreateExerciseRequestModel request);

        Task<Result> Edit(EditExerciseRequestModel request);
    }
}
