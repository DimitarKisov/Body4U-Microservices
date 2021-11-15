namespace Body4U.Identity.Services
{
    using Body4U.Common;
    using Body4U.Identity.Data.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using Serilog;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    using static Body4U.Common.Constants.MessageConstants.Common;

    public class JwtTokenGeneratorService : IJwtTokenGeneratorService
    {
        private readonly IConfiguration configuration;

        public JwtTokenGeneratorService(IConfiguration configuration)
            => this.configuration = configuration;

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
    }
}
