﻿namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Identity.Data.Models;

    public interface IJwtTokenGeneratorService
    {
        Result<string> GenerateToken(ApplicationUser user);
    }
}