﻿namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Common.Models.Identity.Responses;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Data;
    using Body4U.Identity.Data.Models;
    using Body4U.Identity.Models.Requests;
    using Body4U.Identity.Models.Responses;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    using static Body4U.Common.Constants.DataConstants.Common;

    using static Body4U.Common.Constants.MessageConstants.ApplicationUserConstants;
    using static Body4U.Common.Constants.MessageConstants.Common;

    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IJwtTokenGeneratorService jwtTokenGeneratorService;
        private readonly ICurrentUserService currentUserService;
        private readonly IdentityDbContext dbContext;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenGeneratorService jwtTokenGeneratorService,
            ICurrentUserService currentUserService,
            IdentityDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtTokenGeneratorService = jwtTokenGeneratorService;
            this.currentUserService = currentUserService;
            this.dbContext = dbContext;
        }

        public async Task<Result<RegisterUserResponseModel>> Register(RegisterUserRequestModel request)
        {
            try
            {
                var imageResult = this.ImageConverter(request.ProfilePicture);
                if (!imageResult.Succeeded)
                {
                    return Result<RegisterUserResponseModel>.Failure(imageResult.Errors);
                }

                if (!Enum.IsDefined(typeof(Gender), request.Gender))
                {
                    return Result<RegisterUserResponseModel>.Failure(WrongGender);
                }

                var user = new ApplicationUser
                {
                    Email = request.Email,
                    UserName = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Age = request.Age,
                    ProfilePicture = imageResult.Data.Length == 0 ? null : imageResult.Data,
                    Gender = request.Gender,
                    CreateOn = DateTime.Now
                };

                var result = await this.userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
                    return Result<RegisterUserResponseModel>.SuccessWith(new RegisterUserResponseModel { Email = user.Email, UserId = user.Id, Token = token });
                }

                return Result<RegisterUserResponseModel>.Failure(result.Errors.Select(x => x.Description));
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.Register)}", ex);
                return Result<RegisterUserResponseModel>.Failure(string.Format(Wrong, nameof(this.Register)));
            }
        }

        public async Task<Result<string>> Login(LoginUserRequestModel request)
        {
            try
            {
                var user = await this.userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    return Result<string>.Failure(WrongUsernameOrPassword);
                }

                var emailConfirmed = await this.userManager.IsEmailConfirmedAsync(user);
                if (!emailConfirmed)
                {
                    return Result<string>.Failure(EmailNotConfirmed);
                }

                var userLocked = await this.userManager.IsLockedOutAsync(user);
                if (userLocked)
                {
                    return Result<string>.Failure(Locked);
                }

                var passwordValid = await this.userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    return Result<string>.Failure(WrongUsernameOrPassword);
                }

                var roles = await this.userManager.GetRolesAsync(user);

                var tokenResult = this.jwtTokenGeneratorService.GenerateToken(user, roles);

                if (tokenResult.Succeeded)
                {
                    return Result<string>.SuccessWith(tokenResult.Data);
                }

                return Result<string>.Failure(LoginFailed);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.Login)}", ex);
                return Result<string>.Failure(string.Format(Wrong, nameof(this.Login)));
            }
        }

        public async Task<Result<MyProfileResponseModel>> MyProfile()
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(currentUserService.UserId);

                if (user == null)
                {
                    return Result<MyProfileResponseModel>.Failure(string.Format(UserNotFound, currentUserService.UserId));
                }

                var profilePicture = user.ProfilePicture != null
                    ? Convert.ToBase64String(user.ProfilePicture)
                    : null;

                return Result<MyProfileResponseModel>.SuccessWith(
                    new MyProfileResponseModel 
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        ProfilePicture = profilePicture,
                        Age = user.Age,
                        PhoneNumber = user.PhoneNumber,
                        Gender = user.Gender 
                    });
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.MyProfile)}", ex);
                return Result<MyProfileResponseModel>.Failure(string.Format(Wrong, nameof(this.MyProfile)));
            }
        }

        public async Task<Result> EditMyProfile(EditMyProfileRequestModel request)
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(request.Id);
                if (user == null)
                {
                    return Result.Failure(string.Format(UserNotFound, request.Id));
                }

                if (request.Id != this.currentUserService.UserId && await userManager.IsInRoleAsync(user, AdministratorRoleName))
                {
                    return Result.Failure(WrongWrights);
                }

                if (request.ProfilePicture != null &&
                    request.ProfilePicture.ContentType != "image/jpeg" &&
                    request.ProfilePicture.ContentType != "image/png" &&
                    request.ProfilePicture.ContentType != "image/jpg")
                {
                    return Result.Failure(WrongImageFormat);
                }

                if (!Enum.IsDefined(typeof(Gender), request.Gender))
                {
                    return Result.Failure(WrongGender);
                }

                user.PhoneNumber = request.PhoneNumber;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Age = request.Age;
                user.Gender = request.Gender;

                if (request.ProfilePicture.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await request.ProfilePicture.CopyToAsync(stream);

                        if (user.ProfilePicture != stream.ToArray())
                        {
                            user.ProfilePicture = stream.ToArray();
                        }
                    }
                }

                return Result.Success;

                //TODO: Когато се добавят и треньори го довърши? Може и да не се ъпдейтва оттука.
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.EditMyProfile)}", ex);
                return Result.Failure(string.Format(Wrong, nameof(this.EditMyProfile)));
            }
        }

        public async Task<Result> ChangePassword(ChangePasswordRequestModel request)
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(currentUserService.UserId);
                if (user == null)
                {
                    return Result.Failure(string.Format(UserNotFound, currentUserService.UserId));
                }

                var result = await this.userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

                if (result.Succeeded)
                {
                    return Result.Success;
                }

                var errors = result.Errors.Select(e => e.Description);
                return Result.Failure(errors);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.ChangePassword)}", ex);
                return Result.Failure(string.Format(Wrong, nameof(this.ChangePassword)));
            }
        }

        public async Task<Result<ForgotPasswordResponseModel>> ForgotPassword(ForgotPasswordRequestModel request)
        {
            try
            {
                var user = await this.userManager.FindByEmailAsync(request.Email);
                if (user != null && await this.userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    return Result<ForgotPasswordResponseModel>.SuccessWith(new ForgotPasswordResponseModel { Email = user.Email, UserId = user.Id, Token = token });
                }

                return Result<ForgotPasswordResponseModel>.SuccessWith(new ForgotPasswordResponseModel { Email = null, UserId = null, Token = null });
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.ForgotPassword)}", ex);
                return Result<ForgotPasswordResponseModel>.Failure(string.Format(Wrong, nameof(this.ForgotPassword)));
            }
        }

        public async Task<Result> ResetPassword(string userId, string token, ResetPasswordRequestModel request)
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result.Failure(string.Format(UserNotFound, userId));
                }

                var tokenDecoded = HttpUtility.UrlDecode(token);
                var result = await this.userManager.ResetPasswordAsync(user, token, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => x.Description);
                    return Result.Failure(errors);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.ResetPassword)}", ex);
                return Result.Failure(string.Format(Wrong, nameof(this.ResetPassword)));
            }
        }

        public async Task<Result> VerifyEmail(VerifyEmailRequestModel request)
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Result.Failure(string.Format(UserNotFound, request.UserId));
                }

                //var tokenDecoded = HttpUtility.UrlDecode(request.Token);
                var result = await this.userManager.ConfirmEmailAsync(user, request.Token);
                if (result.Succeeded)
                {
                    return Result.Success;
                }

                return Result.Failure(result.Errors.Select(x => x.Description));
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.VerifyEmail)}", ex);
                return Result.Failure(string.Format(Wrong, nameof(this.VerifyEmail)));
            }
        }

        public async Task<Result<SearchUsersResponseModel>> AllUsers(SearchUsersRequestModel request)
        {
            try
            {
                var users = this.dbContext
                .Users
                .Select(x => new UserResponseModel
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    Roles = this.dbContext.Roles
                        .Where(y => x.Roles
                            .Any(z => z.RoleId == y.Id))
                        .Select(y => new RoleResponseModel { Id = y.Id, Name = y.Name })
                })
                .AsQueryable();

                var totalRecords = await users.CountAsync();

                var pageIndex = request.PageIndex;
                var pageSize = request.PageSize;
                var sortingOrder = request.OrderBy!;
                var sortingField = request.SortBy!;

                var orderBy = "Id";

                if (!string.IsNullOrWhiteSpace(sortingField))
                {
                    if (sortingField.ToLower() == "firstname")
                    {
                        orderBy = nameof(request.FirstName);
                    }
                    else if (sortingField.ToLower() == "lastname")
                    {
                        orderBy = nameof(request.LastName);
                    }
                    else if (sortingField.ToLower() == "email")
                    {
                        orderBy = nameof(request.Email);
                    }
                    else if (sortingField.ToLower() == "phonenumber")
                    {
                        orderBy = nameof(request.PhoneNumber);
                    }
                }

                if (sortingOrder != null && sortingOrder.ToLower() == Desc)
                {
                    users = users.OrderByDescending(x => orderBy);
                }
                else
                {
                    users = users.OrderBy(x => orderBy);
                }

                var data = await users
                 .Skip(pageIndex * pageSize)
                 .Take(pageSize)
                 .ToListAsync();

                return Result<SearchUsersResponseModel>.SuccessWith(new SearchUsersResponseModel { Data = data, TotalRecords = totalRecords });
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.AllUsers)}", ex);
                return Result<SearchUsersResponseModel>.Failure(string.Format(Wrong, nameof(this.AllUsers)));
            }
        }

        #region Private methods
        private Result<byte[]> ImageConverter(IFormFile file)
        {
            if (file == null)
            {
                return Result<byte[]>.SuccessWith(new byte[0]);
            }
            if (file != null && file.ContentType != "image/jpeg" && file.ContentType != "image/png")
            {
                return Result<byte[]>.Failure(WrongImageFormat);
            }

            var result = new byte[file!.Length];

            if (file!.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    result = stream.ToArray();
                }
            }

            return Result<byte[]>.SuccessWith(result);
        }
        #endregion
    }
}
