namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Identity.Data.Models.Identity;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IJwtTokenGeneratorService
    {
        Task<Result<string>> GenerateToken(ApplicationUser user, IEnumerable<string> roles = null);

        Task<Result<string>> GenerateRefreshToken();
    }
}
