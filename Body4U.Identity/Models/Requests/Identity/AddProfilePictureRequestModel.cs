namespace Body4U.Identity.Models.Requests.Identity
{
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;

    public class AddProfilePictureRequestModel
    {
        [Required]
        public IFormFile ProfilePicture { get; set; }
    }
}
