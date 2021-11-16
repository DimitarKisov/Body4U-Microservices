namespace Body4U.Identity.Models.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class ChangePasswordRequestModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; }
    }
}
