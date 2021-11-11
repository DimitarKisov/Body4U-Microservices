namespace Body4U.Identity.Data.Seeders
{
    using Body4U.Identity.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Common;

    internal class RoleToApplicationUserSeeder : ISeeder
    {
        private readonly IConfiguration configuration;

        public RoleToApplicationUserSeeder(IConfiguration configuration)
            => this.configuration = configuration;

        public async Task SeedAsync(IdentityDbContext dbContext, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var userName = configuration.GetSection("SeedInfo")["Email"];

            await AssignRoles(userManager, dbContext, userName, AdministratorRoleName);
        }

        public static async Task AssignRoles(UserManager<ApplicationUser> userManager, IdentityDbContext dbContext, string email, string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var result = await userManager.AddToRoleAsync(user, role);

                if (!result.Succeeded)
                {
                    Log.Error($"{nameof(RoleToApplicationUserSeeder)}.{nameof(AssignRoles)}", string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
