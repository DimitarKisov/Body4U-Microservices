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
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

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

        public Result<string> GenerateToken(ApplicationUser user, IEnumerable<string> roles = null)
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

                if (roles != null)
                {
                    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(encodedKey),
                    SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var encryptedToken = tokenHandler.WriteToken(token);

                return Result<string>.SuccessWith(encryptedToken);
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

                return this.GenerateToken(user);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(JwtTokenGeneratorService)}.{nameof(this.GenerateRefreshToken)}", ex);
                return Result<string>.Failure(string.Format(Wrong, nameof(this.GenerateRefreshToken)));
            }
        }
    }
}
