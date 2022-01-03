namespace Body4U.Common.Models.Comment.Requests
{
    public class SearchCommentsRequestModel
    {
        public int ArticleId { get; set; }

        public int PageIndex { get; set; } = 0;

        public int PageSize { get; set; } = 10;
    }
}
