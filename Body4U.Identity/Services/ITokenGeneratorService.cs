namespace Body4U.Identity.Services
{
    using Body4U.Identity.Data.Models;

    public interface ITokenGeneratorService
    {
        string GenerateToken(ApplicationUser user);
    }
}
