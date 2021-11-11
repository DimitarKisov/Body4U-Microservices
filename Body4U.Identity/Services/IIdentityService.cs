namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Identity.Models.Requests;
    using Body4U.Identity.Models.Responses;
    using System.Threading.Tasks;

    public interface IIdentityService
    {
        Task<Result<RegisterUserResponseModel>> Register(RegisterUserRequestModel request);
    }
}
