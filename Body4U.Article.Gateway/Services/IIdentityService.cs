namespace Body4U.Article.Gateway.Services
{
    using Body4U.Common.Models.Identity.Responses;
    using Refit;
    using System.Threading.Tasks;

    public interface IIdentityService
    {
        [Get("/Identity/GetUserInfo")]
        Task<GetUserInfoResponseModel> GetUserInfo(string id);
    }
}
