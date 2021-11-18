namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Identity.Models.Requests;
    using Body4U.Identity.Models.Responses;
    using System.Threading.Tasks;

    public interface IIdentityService
    {
        Task<Result<RegisterUserResponseModel>> Register(RegisterUserRequestModel request);

        Task<Result<string>> Login(LoginUserRequestModel request);

        Task<Result<MyProfileResponseModel>> MyProfile(string userId);

        Task<Result> ChangePassword(ChangePasswordRequestModel request, string userId);

        Task<Result<ForgotPasswordResponseModel>> ForgotPassword(ForgotPasswordRequestModel request);

        Task<Result> ResetPassword(string userId, string token, ResetPasswordRequestModel request);

        Task<Result> VerifyEmail(VerifyEmailRequestModel request);
    }
}
