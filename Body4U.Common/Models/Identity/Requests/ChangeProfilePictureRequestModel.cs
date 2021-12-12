namespace Body4U.Common.Models.Identity.Requests
{
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;

    public class ChangeProfilePictureRequestModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public IFormFile ProfilePicture { get; set; }
    }
}
