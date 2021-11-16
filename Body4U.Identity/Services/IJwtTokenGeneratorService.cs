﻿namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Identity.Data.Models;
    using System.Threading.Tasks;

    public interface IJwtTokenGeneratorService
    {
        Result<string> GenerateToken(ApplicationUser user);

        Task<Result<string>> GenerateRefreshToken(string userId);
    }
}
