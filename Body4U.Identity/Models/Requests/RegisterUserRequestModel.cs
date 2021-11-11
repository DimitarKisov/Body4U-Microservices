namespace Body4U.Identity.Models.Requests
{
    using Body4U.Identity.Data.Models;
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.ApplicationUserConstants;

    public class RegisterUserRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [RegularExpression(PhoneNumberRegex, ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(FirstNameMaxLength, ErrorMessage = "Last name must be between {0} and {1} symbols long!", MinimumLength = FirstNameMinLenght)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(LastNameMaxLength, ErrorMessage = "Last name must be between {0} and {1} symbols long!", MinimumLength = LastNameMinLength)]
        public string LastName { get; set; }

        [Range(MinAge, MaxAge)]
        public int? Age { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public IFormFile ProfilePicture { get; set; }

        [Required]
        [StringLength(MaxPasswordLength, ErrorMessage = "Password must be between {2} and {1} symbols long!", MinimumLength = MinPasswordLength)]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords does not match.")]
        public string ConfirmPassword { get; set; }
    }
}
