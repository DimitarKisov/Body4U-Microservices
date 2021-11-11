namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Identity.Data.Models;
    using Body4U.Identity.Models.Requests;
    using Body4U.Identity.Models.Responses;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Serilog;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.ApplicationUserConstants;
    using static Body4U.Common.Constants.DataConstants.Common;

    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IJwtTokenGeneratorService jwtTokenGeneratorService;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenGeneratorService jwtTokenGeneratorService)
        {
            this.userManager = userManager;
            this.jwtTokenGeneratorService = jwtTokenGeneratorService;
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
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
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
