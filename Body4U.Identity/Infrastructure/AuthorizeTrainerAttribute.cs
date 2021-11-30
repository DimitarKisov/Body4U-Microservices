﻿namespace Body4U.Identity.Infrastructure
{
    using Microsoft.AspNetCore.Authorization;

    using static Body4U.Common.Constants.DataConstants.Common;

    public class AuthorizeTrainerAttribute : AuthorizeAttribute
    {
        public AuthorizeTrainerAttribute() => this.Roles = TrainerRoleName;
    }
}
