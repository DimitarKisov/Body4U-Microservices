namespace Body4U.Common.Models.Identity.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class DeleteProfilePictureRequestModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string PublicId { get; set; }
    }
}
