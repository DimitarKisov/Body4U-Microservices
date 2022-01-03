namespace Body4U.Identity.Services.Identity
{
    using Body4U.Common;
    using Body4U.Common.Messages.Article;
    using Body4U.Common.Messages.Identity;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Common.Models.Identity.Responses;
    using Body4U.Common.Services.Cloud;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Data;
    using Body4U.Identity.Data.Models.Identity;
    using Body4U.Identity.Models.Requests.Identity;
    using Body4U.Identity.Models.Responses.Identity;
    using MassTransit;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats.Jpeg;
    using SixLabors.ImageSharp.Processing;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    using static Body4U.Common.Constants.DataConstants.ApplicationUser;
    using static Body4U.Common.Constants.DataConstants.Common;

    using static Body4U.Common.Constants.MessageConstants.ApplicationUser;
    using static Body4U.Common.Constants.MessageConstants.Common;

    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IJwtTokenGeneratorService jwtTokenGeneratorService;
        private readonly ICurrentUserService currentUserService;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IdentityDbContext dbContext;
        private readonly IConfiguration configuration;
        private readonly IBus publisher;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenGeneratorService jwtTokenGeneratorService,
            ICurrentUserService currentUserService,
            ICloudinaryService cloudinaryService,
            IdentityDbContext dbContext,
            IConfiguration configuration,
            IBus publisher)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.jwtTokenGeneratorService = jwtTokenGeneratorService;
            this.currentUserService = currentUserService;
            this.cloudinaryService = cloudinaryService;
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.publisher = publisher;
        }

        public async Task<Result<RegisterUserResponseModel>> Register(RegisterUserRequestModel request)
        {
            try
            {
                var addImage = false;

                if (request.ProfilePicture != null && request.ProfilePicture?.Length > 0)
                {
                    if (request.ProfilePicture.ContentType != "image/jpeg" &&
                        request.ProfilePicture.ContentType != "image/jpg" &&
                        request.ProfilePicture.ContentType != "image/png" &&
                        request.ProfilePicture.ContentType != "image/bmp")
                    {
                        return Result<RegisterUserResponseModel>.Failure(WrongImageFormat);
                    }

                    using (var imageResult = Image.Load(request.ProfilePicture.OpenReadStream()))
                    {
                        if (imageResult.Width < ProfilePictureInProfileWidth || imageResult.Height < ProfilePictureInProfileHeight)
                        {
                            return Result<RegisterUserResponseModel>.Failure(string.Format(WrongWidthOrHeight, ProfilePictureInProfileWidth, ProfilePictureInProfileHeight));
                        }

                        addImage = true;
                    }
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
                    Gender = request.Gender,
                    CreateOn = DateTime.Now
                };

                var createUserResult = await this.userManager.CreateAsync(user, request.Password);
                List<string> errosInImageUploading = null;

                if (createUserResult.Succeeded)
                {
                    if (addImage)
                    {
                        var id = Guid.NewGuid().ToString();
                        var totalImages = await this.dbContext.UserImageDatas.CountAsync();
                        var folder = $"Identity/Profile/{totalImages % 1000}";

                        var uploadImageResult = await this.cloudinaryService.UploadImage(request.ProfilePicture.OpenReadStream(), id, folder);
                        if (uploadImageResult.Succeeded)
                        {
                            var userImageData = new UserImageData
                            {
                                Id = uploadImageResult.Data.PublicId,
                                Url = uploadImageResult.Data.Url,
                                Folder = folder,
                                ApplicationUserId = user.Id
                            };

                            await this.dbContext.UserImageDatas.AddAsync(userImageData);
                            await this.dbContext.SaveChangesAsync();
                        }
                        else
                        {
                            errosInImageUploading = new List<string>();
                            errosInImageUploading.AddRange(uploadImageResult.Errors);
                        }
                    }

                    var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);

                    return Result<RegisterUserResponseModel>.SuccessWith(new RegisterUserResponseModel { Email = user.Email, UserId = user.Id, Token = token, ErrorsInImageUploading = errosInImageUploading });
                }

                return Result<RegisterUserResponseModel>.Failure(createUserResult.Errors.Select(x => x.Description));

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(Register)}");
                return Result<RegisterUserResponseModel>.Failure(string.Format(Wrong, nameof(Register)));
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
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(Login)}");
                return Result<string>.Failure(string.Format(Wrong, nameof(Login)));
            }
        }

        public async Task<Result<MyProfileResponseModel>> MyProfile()
        {
            try
            {
                var user = await this.dbContext
                    .Users
                    .Select(x => new
                    {
                        x.Id,
                        x.FirstName,
                        x.LastName,
                        x.Email,
                        x.Age,
                        x.PhoneNumber,
                        x.Gender
                    })
                    .FirstOrDefaultAsync(x => x.Id == currentUserService.UserId);

                if (user == null)
                {
                    return Result<MyProfileResponseModel>.Failure(string.Format(UserNotFound, currentUserService.UserId));
                }

                var profilePicturePath = (await this.dbContext
                    .UserImageDatas
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id))?.Url;

                return Result<MyProfileResponseModel>.SuccessWith(
                    new MyProfileResponseModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        ProfilePictureUrl = profilePicturePath,
                        Age = user.Age,
                        PhoneNumber = user.PhoneNumber,
                        Gender = user.Gender
                    });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(MyProfile)}");
                return Result<MyProfileResponseModel>.Failure(string.Format(Wrong, nameof(MyProfile)));
            }
        }

        public async Task<Result> Edit(EditMyProfileRequestModel request)
        {
            try
            {
                if (request.Id != this.currentUserService.UserId && !this.currentUserService.IsAdministrator)
                {
                    return Result.Failure(WrongWrights);
                }

                var user = await this.userManager.FindByIdAsync(request.Id);
                if (user == null)
                {
                    return Result.Failure(string.Format(UserNotFound, request.Id));
                }

                if (!Enum.IsDefined(typeof(Gender), request.Gender))
                {
                    return Result.Failure(WrongGender);
                }

                var sendEditMessage = false;
                if (user.FirstName != request.FirstName || user.LastName != request.LastName)
                {
                    sendEditMessage = true;
                }

                user.PhoneNumber = request.PhoneNumber;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Age = request.Age;
                user.Gender = request.Gender;
                user.ModifiedOn = DateTime.Now;
                user.ModifiedBy = this.currentUserService.UserId;

                if (sendEditMessage)
                {
                    await this.publisher.Publish(new EditUserNamesMessage()
                    {
                        ApplicationUserId = user.Id,
                        FirstName = request.FirstName,
                        LastName = request.LastName
                    });
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(Edit)}");
                return Result.Failure(string.Format(Wrong, nameof(Edit)));
            }
        }

        public async Task<Result> AddProfilePicture(AddProfilePictureRequestModel request)
        {
            try
            {
                if (request.ProfilePicture != null && request.ProfilePicture?.Length > 0)
                {
                    if (request.ProfilePicture.ContentType != "image/jpeg" &&
                        request.ProfilePicture.ContentType != "image/jpg" &&
                        request.ProfilePicture.ContentType != "image/png" &&
                        request.ProfilePicture.ContentType != "image/bmp")
                    {
                        return Result<RegisterUserResponseModel>.Failure(WrongImageFormat);
                    }

                    using (var imageResult = Image.Load(request.ProfilePicture.OpenReadStream()))
                    {
                        if (imageResult.Width < ProfilePictureInProfileWidth || imageResult.Height < ProfilePictureInProfileHeight)
                        {
                            return Result.Failure(String.Format(WrongWidthOrHeight, ProfilePictureInProfileWidth, ProfilePictureInProfileHeight));
                        }
                    }

                    var userProfilePicture = await this.dbContext
                    .UserImageDatas
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId);

                    var id = Guid.NewGuid().ToString();
                    var totalImages = await this.dbContext.UserImageDatas.CountAsync();
                    var folder = $"Identity/Profile/{totalImages % 1000}";

                    if (userProfilePicture != null)
                    {
                        return Result.Failure(ChangeProfilePictureDeny);
                    }

                    var result = await this.cloudinaryService.UploadImage(request.ProfilePicture.OpenReadStream(), id, folder);
                    if (result.Succeeded)
                    {
                        var userImageData = new UserImageData
                        {
                            Id = result.Data.PublicId,
                            Url = result.Data.Url,
                            Folder = folder,
                            ApplicationUserId = this.currentUserService.UserId
                        };

                        await this.dbContext.UserImageDatas.AddAsync(userImageData);
                        await this.dbContext.SaveChangesAsync();

                        return Result.Success;
                    }

                    return Result.Failure(result.Errors);
                }

                return Result.Failure(NoImage);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(AddProfilePicture)}");
                return Result.Failure(string.Format(Wrong, nameof(AddProfilePicture)));
            }
        }

        public async Task<Result> DeleteProfilePicture(DeleteProfilePictureRequestModel request)
        {
            try
            {
                if (request.UserId != this.currentUserService.UserId && !this.currentUserService.IsAdministrator)
                {
                    return Result.Failure(WrongWrights);
                }

                var userProfilePicture = await this.dbContext
                .UserImageDatas
                .FirstOrDefaultAsync(x => x.ApplicationUserId == request.UserId);

                if (userProfilePicture == null)
                {
                    return Result.Failure(ProfilePictureNotFound);
                }

                var result = await this.cloudinaryService.DeleteImage(userProfilePicture.Id, userProfilePicture.Folder);
                if (result.Succeeded)
                {
                    this.dbContext.UserImageDatas.Remove(userProfilePicture);
                    await this.dbContext.SaveChangesAsync();

                    return Result.Success;
                }

                return Result<RegisterUserResponseModel>.Failure(result.Errors);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(DeleteProfilePicture)}");
                return Result.Failure(string.Format(Wrong, nameof(DeleteProfilePicture)));
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
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(ChangePassword)}");
                return Result.Failure(string.Format(Wrong, nameof(ChangePassword)));
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
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(ForgotPassword)}");
                return Result<ForgotPasswordResponseModel>.Failure(string.Format(Wrong, nameof(ForgotPassword)));
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
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(ResetPassword)}");
                return Result.Failure(string.Format(Wrong, nameof(ResetPassword)));
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
                var result = await this.userManager.ConfirmEmailAsync(user, request.Token);
                if (result.Succeeded)
                {
                    return Result.Success;
                }

                return Result.Failure(result.Errors.Select(x => x.Description));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(VerifyEmail)}");
                return Result.Failure(string.Format(Wrong, nameof(VerifyEmail)));
            }
        }

        public async Task<Result<SearchUsersResponseModel>> Users(SearchUsersRequestModel request)
        {
            try
            {
                var users = this.userManager
                    .Users
                    .Select(x => new UserResponseModel
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Email = x.Email,
                        PhoneNumber = x.PhoneNumber,
                        Roles = this.roleManager.Roles
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
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(Users)}");
                return Result<SearchUsersResponseModel>.Failure(string.Format(Wrong, nameof(Users)));
            }
        }

        public async Task<Result<List<RoleResponseModel>>> Roles()
        {
            try
            {
                var roles = await this.dbContext.Roles.Select(x => new RoleResponseModel { Id = x.Id, Name = x.Name }).ToListAsync();

                return Result<List<RoleResponseModel>>.SuccessWith(roles);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(Roles)}");
                return Result<List<RoleResponseModel>>.Failure(string.Format(Wrong, nameof(Roles)));
            }
        }

        public async Task<Result> EditUserRoles(EditUserRolesRequestModel request)
        {
            try
            {
                var user = await this.userManager.FindByEmailAsync(request.Email);

                //Взимаме всички имена на роли за дадения потребител
                var userRoleNames = await userManager.GetRolesAsync(user);
                //Взимаме тези роли, които отговарят за съответния потребител
                var roles = await roleManager.Roles
                    .Where(x => userRoleNames
                        .Contains(x.Name))
                    .Select(x => x.Id)
                    .ToListAsync();

                //Премахваме дублиранията ако има такива
                var rolesDistincted = request.RolesIds.Distinct();

                var rolesForAdd = rolesDistincted.Except(roles);
                var rolesForRemove = roles.Except(request.RolesIds);

                var errors = new List<string>();

                foreach (var roleId in rolesForAdd)
                {
                    var roleName = (await this.roleManager.Roles.FirstOrDefaultAsync(x => x.Id == roleId))?.Name;

                    IdentityResult identityResult;

                    if (roleName != null)
                    {
                        if (!await this.userManager.IsInRoleAsync(user, roleName))
                        {
                            identityResult = await this.userManager.AddToRoleAsync(user, roleName);

                            if (identityResult.Succeeded && roleName == TrainerRoleName)
                            {
                                await this.publisher.Publish(new CreateTrainerMessage()
                                {
                                    ApplicationUserId = user.Id,
                                    CreatedOn = DateTime.Now,
                                    FirstName = user.FirstName,
                                    Lastname = user.LastName
                                });
                            }
                            else
                            {
                                errors.AddRange(identityResult.Errors.Select(x => x.Description));
                            }
                        }
                    }
                }

                foreach (var roleId in rolesForRemove)
                {
                    var roleName = (await this.roleManager.Roles.FirstOrDefaultAsync(x => x.Id == roleId))?.Name;

                    IdentityResult identityResult;

                    if (roleName != null)
                    {
                        if (await this.userManager.IsInRoleAsync(user, roleName))
                        {
                            identityResult = await this.userManager.RemoveFromRoleAsync(user, roleName);

                            if (identityResult.Succeeded && roleName == TrainerRoleName)
                            {
                                await this.publisher.Publish(new DeleteTrainerMessage()
                                {
                                    ApplicationUserId = user.Id
                                });
                            }
                            else
                            {
                                errors.AddRange(identityResult.Errors.Select(x => x.Description));
                            }
                        }
                    }
                }

                return errors.Count() == 0
                    ? Result.Success
                    : Result.Failure(errors);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(EditUserRoles)}");
                return Result.Failure(string.Format(Wrong, nameof(EditUserRoles)));
            }
        }

        public async Task<Result<GetUserInfoResponseModel>> GetUserInfo(string id)
        {
            try
            {
                var user = await this.dbContext
                    .Users
                    .Select(x => new GetUserInfoResponseModel()
                    {
                        Id = x.Id,
                        FullName = x.FirstName + " " + x.LastName,
                        Age = x.Age
                    })
                    .FirstOrDefaultAsync(x => x.Id == id);

                var profileImageUrl = (await this.dbContext
                    .UserImageDatas
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id))?
                    .Url;

                user.ProfileImageUrl = profileImageUrl;

                return Result<GetUserInfoResponseModel>.SuccessWith(user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(GetUserInfo)}");
                return Result<GetUserInfoResponseModel>.Failure(string.Format(Wrong, nameof(GetUserInfo)));
            }
        }

        public async Task<Result<List<GetUserInfoResponseModel>>> GetUsersInfo(List<string> ids)
        {
            try
            {
                var users = await this.dbContext
                    .Users
                    .Select(x => new GetUserInfoResponseModel()
                    {
                        Id = x.Id,
                        FullName = x.FirstName + " " + x.LastName,
                        ProfileImageUrl = this.dbContext.UserImageDatas.FirstOrDefault(x => x.ApplicationUserId == x.Id).Url
                    })
                    .ToListAsync();

                return Result<List<GetUserInfoResponseModel>>.SuccessWith(users);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(GetUsersInfo)}");
                return Result<List<GetUserInfoResponseModel>>.Failure(string.Format(Wrong, nameof(GetUsersInfo)));
            }
        }

        #region Private methods
        private async Task SaveImage(IFormFile imageFile, string name, string path, int? resizedWidth = null, int? resizedHeight = null)
        {
            try
            {
                using (var image = Image.Load(imageFile.OpenReadStream()))
                {
                    var width = image.Width;
                    var height = image.Height;

                    if (resizedWidth != null && resizedHeight != null)
                    {
                        width = (int)resizedWidth;
                        height = (int)resizedHeight;
                    }

                    image.Mutate(x => x.Resize(new Size(width, height)));

                    //Used to remove information about the picture if someone download it.
                    image.Metadata.ExifProfile = null;

                    await image.SaveAsJpegAsync($"{path}/{name}", new JpegEncoder
                    {
                        Quality = 75
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(SaveImage)}");
            }
        }
        #endregion
    }
}
