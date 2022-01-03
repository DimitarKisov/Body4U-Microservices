namespace Body4U.Article.Gateway.Services
{
    using Body4U.Common.Models.Identity.Responses;
    using Refit;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IIdentityService
    {
        [Get("/Identity/GetUserInfo/{id}")]
        Task<GetUserInfoResponseModel> GetUserInfo(string id);

        [Get("/Identity/GetUsersInfo")]
        Task<List<GetUserInfoResponseModel>> GetUsersInfo([Body] List<string> ids);
    }
}
