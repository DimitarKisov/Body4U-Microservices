namespace Body4U.Identity.Services.Identity
{
    using Body4U.Common;
    using Body4U.Common.Messages;
    using Body4U.Common.Messages.Article;
    using Body4U.Common.Messages.Identity;
    using Body4U.Common.Models.Identity.Requests;
    using Body4U.Common.Models.Identity.Responses;
    using Body4U.Common.Services;
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
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    using static Body4U.Common.Constants.DataConstants.ApplicationUser;
    using static Body4U.Common.Constants.DataConstants.Common;

    using static Body4U.Common.Constants.MessageConstants.ApplicationUser;
    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.StatusCodes;

    public class IdentityService : DataService<ApplicationUser>, IIdentityService
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
            : base(dbContext)
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
            var addImage = false;

            if (request.ProfilePicture != null && request.ProfilePicture?.Length > 0)
            {
                if (request.ProfilePicture.ContentType != "image/jpeg" &&
                    request.ProfilePicture.ContentType != "image/jpg" &&
                    request.ProfilePicture.ContentType != "image/png" &&
                    request.ProfilePicture.ContentType != "image/bmp")
                {
                    return Result<RegisterUserResponseModel>.Failure(BadRequest, WrongImageFormat);
                }

                using (var imageResult = Image.Load(request.ProfilePicture.OpenReadStream()))
                {
                    if (imageResult.Width < ProfilePictureInProfileWidth || imageResult.Height < ProfilePictureInProfileHeight)
                    {
                        return Result<RegisterUserResponseModel>.Failure(BadRequest, string.Format(WrongWidthOrHeight, ProfilePictureInProfileWidth, ProfilePictureInProfileHeight));
                    }

                    addImage = true;
                }
            }

            if (!Enum.IsDefined(typeof(Gender), request.Gender))
            {
                return Result<RegisterUserResponseModel>.Failure(BadRequest, WrongGender);
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

            List<string> errosInImageUploading = null;

            var createUserResult = await this.userManager.CreateAsync(user, request.Password);

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

                        try
                        {
                            await this.dbContext.UserImageDatas.AddAsync(userImageData);
                            await this.dbContext.SaveChangesAsync();
                        }
                        catch (NotImplementedException ex) //TODO: NotImplementedException ???
                        {
                            Log.Error(ex, string.Format(Wrong, $"{nameof(IdentityService)}.{nameof(Register)}"));
                            return Result<RegisterUserResponseModel>.Failure(InternalServerError);
                        }
                    }
                    else
                    {
                        errosInImageUploading = new List<string>();
                        errosInImageUploading.AddRange(uploadImageResult.Errors);
                    }
                }

                var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);

                return Result<RegisterUserResponseModel>.SuccessWith(new RegisterUserResponseModel()
                { 
                    Email = user.Email,
                    UserId = user.Id,
                    Token = token,
                    ErrorsInImageUploading = errosInImageUploading
                });
            }

            return Result<RegisterUserResponseModel>.Failure(InternalServerError, createUserResult.Errors.Select(x => x.Description)); //TODO: 500?
        }

        public async Task<Result<string>> Login(LoginUserRequestModel request)
        {
            var user = await this.userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Result<string>.Failure(NotFound, WrongUsernameOrPassword);
            }

            var passwordValid = await this.userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                return Result<string>.Failure(BadRequest, WrongUsernameOrPassword);
            }

            var emailConfirmed = await this.userManager.IsEmailConfirmedAsync(user);
            if (!emailConfirmed)
            {
                return Result<string>.Failure(BadRequest, EmailNotConfirmed); //TODO: 400?
            }

            var userLocked = await this.userManager.IsLockedOutAsync(user);
            if (userLocked)
            {
                return Result<string>.Failure(BadRequest, Locked); //TODO: 400?
            }

            var roles = await this.userManager.GetRolesAsync(user);

            var tokenResult = this.jwtTokenGeneratorService.GenerateToken(user, roles);

            if (tokenResult.Succeeded)
            {
                return Result<string>.SuccessWith(tokenResult.Data);
            }

            return Result<string>.Failure(InternalServerError, LoginFailed); //TODO: 500?
        }

        public async Task<Result<MyProfileResponseModel>> MyProfile()
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
                return Result<MyProfileResponseModel>.Failure(NotFound, string.Format(UserNotFound, currentUserService.UserId));
            }

            var profilePicturePath = (await this.dbContext
                .UserImageDatas
                .FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id))?
                .Url;

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

        public async Task<Result> Edit(EditMyProfileRequestModel request)
        {
            if (request.Id != this.currentUserService.UserId && !this.currentUserService.IsAdministrator)
            {
                return Result.Failure(Forbidden);
            }

            if (!Enum.IsDefined(typeof(Gender), request.Gender))
            {
                return Result.Failure(BadRequest, WrongGender);
            }

            var user = await this.userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                return Result.Failure(NotFound, string.Format(UserNotFound, request.Id));
            }

            user.PhoneNumber = request.PhoneNumber;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Age = request.Age;
            user.Gender = request.Gender;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = this.currentUserService.UserId;

            var messageData = new EditTrainerNamesMessage()
            {
                ApplicationUserId = user.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ModifiedBy = this.currentUserService.UserId
            };

            var message = new Message(messageData);

            await this.Save(user, message);

            try
            {
                await this.publisher.Publish(messageData);

                message.MarkAsPublished();

                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(IdentityService)}/{nameof(Edit)}");
            }

            return Result.Success;
        }

        public async Task<Result> AddProfilePicture(AddProfilePictureRequestModel request)
        {
            if (request.ProfilePicture != null && request.ProfilePicture?.Length > 0)
            {
                if (request.ProfilePicture.ContentType != "image/jpeg" &&
                    request.ProfilePicture.ContentType != "image/jpg" &&
                    request.ProfilePicture.ContentType != "image/png" &&
                    request.ProfilePicture.ContentType != "image/bmp")
                {
                    return Result<RegisterUserResponseModel>.Failure(BadRequest, WrongImageFormat);
                }

                using (var imageResult = Image.Load(request.ProfilePicture.OpenReadStream()))
                {
                    if (imageResult.Width < ProfilePictureInProfileWidth || imageResult.Height < ProfilePictureInProfileHeight)
                    {
                        return Result.Failure(BadRequest, string.Format(WrongWidthOrHeight, ProfilePictureInProfileWidth, ProfilePictureInProfileHeight));
                    }
                }

                var userProfilePicture = await this.dbContext
                .UserImageDatas
                .FirstOrDefaultAsync(x => x.ApplicationUserId == this.currentUserService.UserId);

                if (userProfilePicture != null)
                {
                    return Result.Failure(Conflict, ChangeProfilePictureDeny); //TODO: 409?
                }

                var id = Guid.NewGuid().ToString();
                var totalImages = await this.dbContext.UserImageDatas.CountAsync();
                var folder = $"Identity/Profile/{totalImages % 1000}";

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

                    try
                    {
                        await this.dbContext.UserImageDatas.AddAsync(userImageData);
                        await this.dbContext.SaveChangesAsync();
                    }
                    catch (NotImplementedException ex) //TODO: NotImplementedException ???
                    {
                        Log.Error(ex, string.Format(Wrong, $"{nameof(IdentityService)}.{nameof(AddProfilePicture)}"));
                        return Result.Failure(InternalServerError);
                    }

                    return Result.Success;
                }

                return Result.Failure(InternalServerError, result.Errors);
            }

            return Result.Failure(BadRequest, NoImage);
        }

        public async Task<Result> DeleteProfilePicture(DeleteProfilePictureRequestModel request)
        {
            if (request.UserId != this.currentUserService.UserId && !this.currentUserService.IsAdministrator)
            {
                return Result.Failure(Forbidden, WrongWrights);
            }

            var userProfilePicture = await this.dbContext
            .UserImageDatas
            .FirstOrDefaultAsync(x => x.ApplicationUserId == request.UserId);

            if (userProfilePicture == null)
            {
                return Result.Failure(NotFound, ProfilePictureNotFound);
            }

            var result = await this.cloudinaryService.DeleteImage(userProfilePicture.Id, userProfilePicture.Folder);
            if (result.Succeeded)
            {
                this.dbContext.UserImageDatas.Remove(userProfilePicture);
                await this.dbContext.SaveChangesAsync();

                return Result.Success;
            }

            return Result<RegisterUserResponseModel>.Failure(InternalServerError, result.Errors);
        }

        public async Task<Result> ChangePassword(ChangePasswordRequestModel request)
        {
            var user = await this.userManager.FindByIdAsync(currentUserService.UserId);
            if (user == null)
            {
                return Result.Failure(NotFound, string.Format(UserNotFound, currentUserService.UserId));
            }

            var result = await this.userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

            if (result.Succeeded)
            {
                return Result.Success;
            }

            var errors = result.Errors.Select(e => e.Description);
            return Result.Failure(InternalServerError, errors); //TODO: 500?
        }

        public async Task<Result<ForgotPasswordResponseModel>> ForgotPassword(ForgotPasswordRequestModel request)
        {
            var user = await this.userManager.FindByEmailAsync(request.Email);
            if (user != null && await this.userManager.IsEmailConfirmedAsync(user))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                return Result<ForgotPasswordResponseModel>.SuccessWith(new ForgotPasswordResponseModel { Email = user.Email, UserId = user.Id, Token = token });
            }

            return Result<ForgotPasswordResponseModel>.SuccessWith(new ForgotPasswordResponseModel { Email = null, UserId = null, Token = null });
        }

        public async Task<Result> ResetPassword(string userId, string token, ResetPasswordRequestModel request)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure(NotFound, string.Format(UserNotFound, userId));
            }

            var tokenDecoded = HttpUtility.UrlDecode(token);
            var result = await this.userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);
                return Result.Failure(InternalServerError, errors);
            }

            return Result.Success;
        }

        public async Task<Result> VerifyEmail(VerifyEmailRequestModel request)
        {
            var user = await this.userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure(NotFound, string.Format(UserNotFound, request.UserId));
            }

            var tokenDecoded = HttpUtility.UrlDecode(request.Token);
            var result = await this.userManager.ConfirmEmailAsync(user, request.Token);
            if (result.Succeeded)
            {
                return Result.Success;
            }

            return Result.Failure(InternalServerError, result.Errors.Select(x => x.Description));
        }

        public async Task<Result<SearchUsersResponseModel>> Users(SearchUsersRequestModel request)
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

        public async Task<Result<List<RoleResponseModel>>> Roles()
        {
            var roles = await this.dbContext
                .Roles
                .Select(x => new RoleResponseModel 
                { 
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();

            return Result<List<RoleResponseModel>>.SuccessWith(roles);
        }

        public async Task<Result> EditUserRoles(EditUserRolesRequestModel request)
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
                var roleName = (await this.roleManager
                    .Roles
                    .FirstOrDefaultAsync(x => x.Id == roleId))?
                    .Name;

                IdentityResult identityResult;

                if (roleName != null)
                {
                    if (!await this.userManager.IsInRoleAsync(user, roleName))
                    {
                        identityResult = await this.userManager.AddToRoleAsync(user, roleName);

                        if (identityResult.Succeeded && roleName == TrainerRoleName)
                        {
                            var messageData = new CreateTrainerMessage()
                            {
                                ApplicationUserId = user.Id,
                                CreatedOn = DateTime.Now,
                                FirstName = user.FirstName,
                                Lastname = user.LastName
                            };

                            var message = new Message(messageData);

                            await this.Save(null, message);

                            try
                            {
                                await this.publisher.Publish(message);

                                message.MarkAsPublished();

                                await this.dbContext.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(EditUserRoles)} in adding users in role.");
                            }
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
                            var messageData = new DeleteTrainerMessage()
                            {
                                ApplicationUserId = user.Id
                            };

                            var message = new Message(messageData);

                            await this.Save(null, message);

                            try
                            {
                                await this.publisher.Publish(messageData);

                                message.MarkAsPublished();

                                await this.dbContext.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, $"{nameof(IdentityService)}.{nameof(EditUserRoles)} in removing users from role.");
                            }
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
                : Result.Failure(InternalServerError, errors); //TODO: 500?
        }

        public async Task<Result<GetUserInfoResponseModel>> GetUserInfo(string id)
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

        public async Task<Result<List<GetUserInfoResponseModel>>> GetUsersInfo(List<string> ids)
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
