namespace Body4U.Identity.Models.Requests.Identity
{
    using System.ComponentModel.DataAnnotations;

    public class VerifyEmailRequestModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
