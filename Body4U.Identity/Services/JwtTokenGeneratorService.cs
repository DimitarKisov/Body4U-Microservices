namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Common.Constants;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Common;

    using static Body4U.Common.Constants.MessageConstants.ApplicationUserConstants;
    using static Body4U.Common.Constants.MessageConstants.Common;

    public class JwtTokenGeneratorService : IJwtTokenGeneratorService
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICurrentUserService currentUserService;

        public JwtTokenGeneratorService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.currentUserService = currentUserService;
        }

        public async Task<Result<string>> GenerateToken(ApplicationUser user)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var encodedKey = Encoding.ASCII.GetBytes(this.configuration.GetSection("JwtSettings")["Secret"]);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var isAdmin = await this.userManager.IsInRoleAsync(user, AdministratorRoleName);
                claims.Add(new Claim(CustomClaimTypes.IsAdmin, isAdmin.ToString()));

                SymmetricSecurityKey key = new SymmetricSecurityKey(encodedKey);
                SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                //TODO: Намали валидноста на токена на по-късен етап
                DateTime expires = DateTime.Now.AddDays(1);

                JwtSecurityToken token = new JwtSecurityToken(
                    "http://yourdomain.com",
                    "http://yourdomain.com",
                    claims,
                    expires: expires,
                    signingCredentials: signingCredentials
                );

                return Result<string>.SuccessWith(new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(JwtTokenGeneratorService)}.{nameof(this.GenerateToken)}", ex);
                return Result<string>.Failure(string.Format(Wrong, nameof(this.GenerateToken)));
            }
        }

        public async Task<Result<string>> GenerateRefreshToken()
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(currentUserService.UserId);
                if (user == null)
                {
                    return Result<string>.Failure(string.Format(UserNotFound, currentUserService.UserId));
                }

                if (user.IsDisabled || user.LockoutEnd != null)
                {
                    return Result<string>.Failure(Locked);
                }

                return await this.GenerateToken(user);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(JwtTokenGeneratorService)}.{nameof(this.GenerateRefreshToken)}", ex);
                return Result<string>.Failure(string.Format(Wrong, nameof(this.GenerateRefreshToken)));
            }
        }
    }
}
