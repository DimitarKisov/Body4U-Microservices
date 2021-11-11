namespace Body4U.Identity.Services
{
    using Body4U.Identity.Data.Models;

    public interface IJwtTokenGeneratorService
    {
        string GenerateToken(ApplicationUser user);
    }
}
