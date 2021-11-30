namespace Body4U.Identity.Services.Identity
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
    using System.Collections.Generic;
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
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IJwtTokenGeneratorService jwtTokenGeneratorService;
        private readonly ICurrentUserService currentUserService;
        private readonly IdentityDbContext dbContext;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenGeneratorService jwtTokenGeneratorService,
            ICurrentUserService currentUserService,
            IdentityDbContext dbContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
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

                var tokenResult = await this.jwtTokenGeneratorService.GenerateToken(user, roles);

                if (tokenResult.Succeeded)
                {
                    return Result<string>.SuccessWith(tokenResult.Data);
                }

                return Result<string>.Failure(LoginFailed);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(Login)}", ex);
                return Result<string>.Failure(string.Format(Wrong, nameof(Login)));
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
                Log.Error($"{nameof(IdentityService)}.{nameof(MyProfile)}", ex);
                return Result<MyProfileResponseModel>.Failure(string.Format(Wrong, nameof(MyProfile)));
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

                if (request.Id != this.currentUserService.UserId && !await userManager.IsInRoleAsync(user, AdministratorRoleName))
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
                Log.Error($"{nameof(IdentityService)}.{nameof(EditMyProfile)}", ex);
                return Result.Failure(string.Format(Wrong, nameof(EditMyProfile)));
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
                Log.Error($"{nameof(IdentityService)}.{nameof(ChangePassword)}", ex);
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
                Log.Error($"{nameof(IdentityService)}.{nameof(ForgotPassword)}", ex);
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
                Log.Error($"{nameof(IdentityService)}.{nameof(ResetPassword)}", ex);
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
                Log.Error($"{nameof(IdentityService)}.{nameof(VerifyEmail)}", ex);
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
                Log.Error($"{nameof(IdentityService)}.{nameof(Users)}", ex);
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
                Log.Error($"{nameof(IdentityService)}.{nameof(Roles)}", ex);
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

                var hasChanges = false;

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
                                var trainer = new Trainer() { ApplicationUserId = user.Id, ApplicationUser = user, CreatedOn = DateTime.Now };
                                await this.dbContext.Trainers.AddAsync(trainer);
                                hasChanges = true;
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
                                var trainer = await this.dbContext.Trainers.FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id);
                                if (trainer != null)
                                {
                                    this.dbContext.Trainers.Remove(trainer);
                                    hasChanges = true;
                                }
                                else
                                {
                                    errors.Add(string.Format(TrainerNotFound, user.Id));
                                }
                            }
                            else
                            {
                                errors.AddRange(identityResult.Errors.Select(x => x.Description));
                            }
                        }
                    }
                }

                if (hasChanges)
                {
                    await this.dbContext.SaveChangesAsync();
                }

                return errors.Count() == 0
                    ? Result.Success
                    : Result.Failure(errors);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(EditUserRoles)}", ex);
                return Result.Failure(string.Format(Wrong, nameof(EditUserRoles)));
            }
        }

        #region Private methods
        private Result<byte[]> ImageConverter(IFormFile file)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error($"{nameof(IdentityService)}.{nameof(ImageConverter)}", ex);
                return Result<byte[]>.Failure(string.Format(Wrong, nameof(ImageConverter)));
            }
        }
        #endregion
    }
}
