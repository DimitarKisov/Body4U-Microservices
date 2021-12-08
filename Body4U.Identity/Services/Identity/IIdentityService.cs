﻿namespace Body4U.Identity.Services.Identity
{
    using Body4U.Common;
    using Body4U.Common.Models;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Common.Models.Identity.Responses;
    using Body4U.Identity.Models.Requests;
    using Body4U.Identity.Models.Responses;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IIdentityService
    {
        Task<Result<RegisterUserResponseModel>> Register(RegisterUserRequestModel request);

        Task<Result<string>> Login(LoginUserRequestModel request);

        Task<Result<MyProfileResponseModel>> MyProfile();

        Task<Result> EditMyProfile(EditMyProfileRequestModel request);

        Task<Result> ChangePassword(ChangePasswordRequestModel request);

        Task<Result<ForgotPasswordResponseModel>> ForgotPassword(ForgotPasswordRequestModel request);

        Task<Result> ResetPassword(string userId, string token, ResetPasswordRequestModel request);

        Task<Result> VerifyEmail(VerifyEmailRequestModel request);

        Task<Result<SearchUsersResponseModel>> Users(SearchUsersRequestModel request);

        Task<Result<List<RoleResponseModel>>> Roles();

        Task<Result> EditUserRoles(EditUserRolesRequestModel request);
    }
}