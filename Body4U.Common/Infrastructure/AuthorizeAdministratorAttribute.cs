namespace Body4U.Common.Infrastructure
{
    using Microsoft.AspNetCore.Authorization;

    using static Body4U.Common.Constants.DataConstants.Common;

    public class AuthorizeAdministratorAttribute : AuthorizeAttribute
    {
        public AuthorizeAdministratorAttribute() => this.Roles = AdministratorRoleName;
    }
}
