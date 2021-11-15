namespace Body4U.Identity.Services
{
    using Body4U.Common;
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

    using static Body4U.Common.Constants.MessageConstants.ApplicationUserConstants;
    using static Body4U.Common.Constants.MessageConstants.Common;

    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IJwtTokenGeneratorService jwtTokenGeneratorService;
        private readonly SignInManager<ApplicationUser> signInManager;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenGeneratorService jwtTokenGeneratorService,
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.jwtTokenGeneratorService = jwtTokenGeneratorService;
            this.signInManager = signInManager;
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
                    //TODO: Fire-ни евент като създадеш микросервиз за мейли
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
                var user = await this.userManager.Users
                    //.Include(x => x.Trainer)
                    .SingleAsync(x => x.NormalizedEmail == request.Email);

                if (user == null)
                {
                    return Result<string>.Failure(WrongUsernameOrPassword);
                }

                //TODO: Разкоментирай след като направиш сервиза за изпращане на мейли
                //var emailConfirmed = await this.userManager.IsEmailConfirmedAsync(user);
                //if (!emailConfirmed)
                //{
                //    return Result<string>.Failure(EmailNotConfirmed);
                //}

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

                var result = await this.signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, user.LockoutEnabled);
                if (result.Succeeded)
                {
                    var tokenResult = this.jwtTokenGeneratorService.GenerateToken(user);

                    if (tokenResult.Succeeded)
                    {
                        return Result<string>.SuccessWith(tokenResult.Data);
                    }

                    return Result<string>.Failure(LoginFailed);
                }

                return Result<string>.Failure(WrongUsernameOrPassword);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(this.Login)}", ex);
                return Result<string>.Failure(string.Format(Wrong, nameof(this.Login)));
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

                var tokenDecoded = HttpUtility.UrlDecode(request.Token);
                var result = await this.userManager.ConfirmEmailAsync(user, tokenDecoded);
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
