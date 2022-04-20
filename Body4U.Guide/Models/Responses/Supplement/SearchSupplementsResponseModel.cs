namespace Body4U.Guide.Models.Responses.Supplement
{
    using Body4U.Guide.Models.Requests.Supplement;
    using System.Collections.Generic;

    public class SearchSupplementsResponseModel
    {
        public ICollection<SupplementResponseModel> Data { get; set; }

        public int TotalRecords { get; set; }
    }
}
