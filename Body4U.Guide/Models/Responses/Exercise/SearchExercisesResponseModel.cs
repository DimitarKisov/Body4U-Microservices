namespace Body4U.Guide.Models.Responses.Exercise
{
    using System.Collections.Generic;

    public class SearchExercisesResponseModel
    {
        public ICollection<ExerciseResponseModel> Data { get; set; }

        public int TotalRecords { get; set; }
    }
}
