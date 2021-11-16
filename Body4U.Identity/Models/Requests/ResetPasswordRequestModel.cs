namespace Body4U.Identity.Models.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class ResetPasswordRequestModel
    {
        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmNewPassword { get; set; }
    }
}
