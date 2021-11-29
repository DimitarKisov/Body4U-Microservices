namespace Body4U.Common.Models.Identity.Requests
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class EditUserRolesRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public ICollection<string> RolesIds { get; set; }
    }
}
