namespace Body4U.Admin.Models.Identity
{
    using System.ComponentModel.DataAnnotations;

    public class LoginUserRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
