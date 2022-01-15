namespace Body4U.Guide.Models.Requests.Exercise
{
    using Body4U.Common.Models;

    public class SearchExercisesRequestModel : SearchModel
    {
        public string Name { get; set; }

        public int ExerciseDifficulty { get; set; }
    }
}
