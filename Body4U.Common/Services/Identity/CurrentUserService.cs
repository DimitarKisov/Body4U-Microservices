namespace Body4U.Common.Services.Identity
{
    using Body4U.Common.Constants;
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;

    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user != null)
            {
                this.UserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                var isAdmin = user.FindFirstValue(CustomClaimTypes.IsAdmin);
                if (isAdmin == null)
                {
                    this.IsAdmin = null;
                }
                else
                {
                    this.IsAdmin = bool.Parse(isAdmin);
                }
            }
        }

        public string UserId { get; }

        public bool? IsAdmin { get; }
    }
}
