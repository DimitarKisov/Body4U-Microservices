namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Identity.Data.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IJwtTokenGeneratorService
    {
        Result<string> GenerateToken(ApplicationUser user, IEnumerable<string> roles = null);

        Task<Result<string>> GenerateRefreshToken();
    }
}
