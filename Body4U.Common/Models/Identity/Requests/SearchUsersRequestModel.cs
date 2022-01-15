namespace Body4U.Common.Models.Identity.Requests
{
    public class SearchUsersRequestModel : SearchModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }
}
