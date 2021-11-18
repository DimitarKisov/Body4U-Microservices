namespace Body4U.Identity.Models.Requests
{
    using Body4U.Identity.Data.Models;
    using Microsoft.AspNetCore.Http;

    public class EditMyProfileRequestModel
    {
        public string Id { get; set; }

        public string PhoneNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public Gender Gender { get; set; }

        public IFormFile ProfilePicture { get; set; }
    }
}
