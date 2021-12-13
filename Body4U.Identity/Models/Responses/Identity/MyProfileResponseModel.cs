namespace Body4U.Identity.Models.Responses.Identity
{
    using Body4U.Identity.Data.Models.Identity;
    using System.IO;

    public class MyProfileResponseModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string ProfilePicturePath { get; set; }

        public int? Age { get; set; }

        public string PhoneNumber { get; set; }

        public Gender Gender { get; set; }
    }
}
