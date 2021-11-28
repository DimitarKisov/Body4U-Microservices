namespace Body4U.Common.Models.Identity.Responses
{
    using System.Collections.Generic;

    public class SearchUsersResponseModel
    {
        public ICollection<UserResponseModel> Data { get; set; }

        public int TotalRecords { get; set; }
    }
}
