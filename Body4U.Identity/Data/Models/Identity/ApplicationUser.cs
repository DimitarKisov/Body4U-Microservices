﻿namespace Body4U.Identity.Data.Models.Identity
{
    using Body4U.Identity.Data.Models.Favourites;
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using static Body4U.Common.Constants.DataConstants.ApplicationUser;

    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Favourites = new HashSet<Favourite>();
            this.Roles = new HashSet<IdentityUserRole<string>>();
            this.Claims = new HashSet<IdentityUserClaim<string>>();
            this.Logins = new HashSet<IdentityUserLogin<string>>();
        }

        [RegularExpression(PhoneNumberRegex)]
        public override string PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

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

        public ICollection<Favourite> Favourites { get; set; }

        public ICollection<IdentityUserRole<string>> Roles { get; set; }

        public ICollection<IdentityUserClaim<string>> Claims { get; set; }

        public ICollection<IdentityUserLogin<string>> Logins { get; set; }
    }
}
