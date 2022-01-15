namespace Body4U.Common.Models
{
    public class SearchModel
    {
        public string SortBy { get; set; }

        public string OrderBy { get; set; }

        public int PageIndex { get; set; } = 0;

        public int PageSize { get; set; } = 10;
    }
}
