namespace Body4U.Admin.Services.Identity
{
    using Body4U.Admin.Models.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Refit;
    using System.Threading.Tasks;

    public interface IIdentityService
    {
        [Post("/Identity/Login")]
        Task<string> Login(LoginUserRequestModel request);
    }
}
