namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Common.Constants;
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Data;
    using Body4U.Identity.Data.Models.Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
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

    using static Body4U.Common.Constants.MessageConstants.ApplicationUser;
    using static Body4U.Common.Constants.MessageConstants.Common;
    using static Body4U.Common.Constants.MessageConstants.StatusCodes;

    public class JwtTokenGeneratorService : IJwtTokenGeneratorService
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICurrentUserService currentUserService;
        private readonly IdentityDbContext dbContext;

        public JwtTokenGeneratorService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IdentityDbContext dbContext)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.currentUserService = currentUserService;
            this.dbContext = dbContext;
        }

        public Result<string> GenerateToken(ApplicationUser user, IEnumerable<string> roles = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = string.Empty;
            try
            {
                key = this.configuration.GetSection("JwtSettings")["Secret"];
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format(Wrong, $"{nameof(JwtTokenGeneratorService)}/{nameof(GenerateToken)}"));
                return Result<string>.Failure(InternalServerError);
            }

            var encodedKey = Encoding.ASCII.GetBytes(key);
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                };

            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            //TODO: Намали валидността на токена
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

        public async Task<Result<string>> GenerateRefreshToken()
        {
            var user = await this.userManager.FindByIdAsync(currentUserService.UserId);
            if (user == null)
            {
                return Result<string>.Failure(NotFound, string.Format(UserNotFound, currentUserService.UserId));
            }

            if (user.IsDisabled || user.LockoutEnd != null)
            {
                return Result<string>.Failure(Conflict, Locked); //TODO: 409???
            }

            return this.GenerateToken(user);
        }
    }
}
