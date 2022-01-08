namespace Body4U.Identity.Models.Requests.Identity
{
    using Body4U.Identity.Data.Models.Identity;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.ApplicationUser;

    public class EditMyProfileRequestModel
    {
        [Required]
        public string Id { get; set; }

        [RegularExpression(PhoneNumberRegex)]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(FirstNameMinLenght)]
        [MaxLength(FirstNameMaxLength)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(LastNameMinLength)]
        [MaxLength(LastNameMaxLength)]
        public string LastName { get; set; }

        public int? Age { get; set; }

        public Gender Gender { get; set; }
    }
}
