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

                var trainerId = user.FindFirstValue(CustomClaimTypes.TrainerId);
                if (trainerId != null)
                {
                    this.TrainerId = int.Parse(trainerId);
                }
            }
        }

        public string UserId { get; }

        public int? TrainerId { get; }
    }
}
