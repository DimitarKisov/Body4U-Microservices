namespace Body4U.Admin.Services.Identity
{
    using Body4U.Admin.Models.Identity;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Common.Models.Identity.Responses;
    using Refit;
    using System.Threading.Tasks;

    public interface IIdentityService
    {
        [Post("/Identity/Login")]
        Task<string> Login(LoginUserRequestModel request);

        [Post("/Identity/AllUsers")]
        Task<SearchUsersResponseModel> AllUsers(SearchUsersRequestModel request);
    }
}
