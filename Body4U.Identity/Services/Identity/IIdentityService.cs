namespace Body4U.Identity.Services.Identity
{
    using Body4U.Common;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Common.Models.Identity.Responses;
    using Body4U.Common.Services;
    using Body4U.Identity.Data.Models.Identity;
    using Body4U.Identity.Models.Requests.Identity;
    using Body4U.Identity.Models.Responses.Identity;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IIdentityService : IDataService<ApplicationUser>
    {
        Task<Result<RegisterUserResponseModel>> Register(RegisterUserRequestModel request);

        Task<Result<LoginResponseModel>> Login(LoginUserRequestModel request);

        Task<Result<MyProfileResponseModel>> MyProfile();

        Task<Result> Edit(EditMyProfileRequestModel request);

        Task<Result> AddProfilePicture(AddProfilePictureRequestModel request);

        Task<Result> DeleteProfilePicture(DeleteProfilePictureRequestModel request);

        Task<Result> ChangePassword(ChangePasswordRequestModel request);

        Task<Result<ForgotPasswordResponseModel>> ForgotPassword(ForgotPasswordRequestModel request);

        Task<Result> ResetPassword(string userId, string token, ResetPasswordRequestModel request);

        Task<Result> VerifyEmail(VerifyEmailRequestModel request);

        Task<Result<SearchUsersResponseModel>> Users(SearchUsersRequestModel request);

        Task<Result<List<RoleResponseModel>>> Roles();

        Task<Result> EditUserRoles(EditUserRolesRequestModel request);

        Task<Result<GetUserInfoResponseModel>> GetUserInfo(string id);

        Task<Result<List<GetUserInfoResponseModel>>> GetUsersInfo(List<string> ids);
    }
}
