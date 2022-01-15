namespace Body4U.Identity.Models.Responses.Identity
{
    using System.Collections.Generic;

    public class RegisterUserResponseModel
    {
        public string Email { get; set; }

        public string UserId { get; set; }

        public string Token { get; set; }

        public ICollection<string> ErrorsInImageUploading { get; set; }
    }
}
