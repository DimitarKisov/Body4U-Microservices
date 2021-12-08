namespace Body4U.Identity.Data.Models.Identity
{
    using Body4U.Identity.Data.Models.Trainer;
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.ApplicationUserConstants;

    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Roles = new HashSet<IdentityUserRole<string>>();
            this.Claims = new HashSet<IdentityUserClaim<string>>();
            this.Logins = new HashSet<IdentityUserLogin<string>>();
        }

        [Required]
        [MinLength(FirstNameMinLenght)]
        [MaxLength(FirstNameMaxLength)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(LastNameMinLength)]
        [MaxLength(LastNameMaxLength)]
        public string LastName { get; set; }

        [Range(MinAge, MaxAge)]
        public int? Age { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [DefaultValue("false")]
        public bool IsDisabled { get; set; }

        [Required]
        public DateTime CreateOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }

        public UserImageData UserImageData { get; set; }

        public Trainer Trainer { get; set; }

        public ICollection<IdentityUserRole<string>> Roles { get; set; }

        public ICollection<IdentityUserClaim<string>> Claims { get; set; }

        public ICollection<IdentityUserLogin<string>> Logins { get; set; }
    }
}
