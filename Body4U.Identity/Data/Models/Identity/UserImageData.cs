namespace Body4U.Identity.Data.Models.Identity
{
    using System.ComponentModel.DataAnnotations;

    public class UserImageData
    {
        public string Id { get; set; }

        [Required]
        public string Folder { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
    }
}
