namespace Body4U.Common.Services.Identity
{
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
            }
        }

        public string UserId { get; }
    }
}
