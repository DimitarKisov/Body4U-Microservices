namespace Body4U.Common.Models.Identity.Responses
{
    using System.Collections.Generic;

    public class UserResponseModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public IEnumerable<RoleResponseModel> Roles { get; set; }
    }
}
