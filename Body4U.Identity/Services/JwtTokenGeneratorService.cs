namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Identity.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using Serilog;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.MessageConstants.ApplicationUserConstants;
    using static Body4U.Common.Constants.MessageConstants.Common;

    public class JwtTokenGeneratorService : IJwtTokenGeneratorService
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;

        public JwtTokenGeneratorService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            this.configuration = configuration;
            this.userManager = userManager;
        }

        public Result<string> GenerateToken(ApplicationUser user)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(this.configuration.GetSection("JwtSettings")["Secret"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
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

        public async Task<Result<string>> GenerateRefreshToken(string userId)
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(userId);

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
