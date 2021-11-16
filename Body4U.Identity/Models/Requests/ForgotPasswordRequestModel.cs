namespace Body4U.Identity.Models.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class ForgotPasswordRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
