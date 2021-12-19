namespace Body4U.Common.Services.Identity
{
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;

    using static Body4U.Common.Constants.DataConstants.Common;

    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user != null)
            {
                this.UserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                this.IsAdministrator = user.IsInRole(AdministratorRoleName);
            }
        }

        public string UserId { get; }

        public bool IsAdministrator { get; }
    }
}
