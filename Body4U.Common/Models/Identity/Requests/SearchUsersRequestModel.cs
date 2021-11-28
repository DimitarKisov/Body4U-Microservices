namespace Body4U.Common.Models.Identity.Requests
{
    public class SearchUsersRequestModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string SortBy { get; set; }

        public string OrderBy { get; set; }

        public int PageIndex { get; set; } = 0;

        public int PageSize { get; set; } = 10;
    }
}
