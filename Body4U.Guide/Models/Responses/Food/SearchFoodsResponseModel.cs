namespace Body4U.Guide.Models.Responses.Food
{
    using System.Collections.Generic;

    public class SearchFoodsResponseModel
    {
        public ICollection<FoodResponseModel> Data { get; set; }

        public int TotalRecords { get; set; }
    }
}
