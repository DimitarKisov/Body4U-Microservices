namespace Body4U.Identity.Data.Seeders
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Common;

    internal class RolesSeeder : ISeeder
    {
        public async Task SeedAsync(IdentityDbContext dbContext, IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedRoleAsync(roleManager, AdministratorRoleName);
            //await SeedRoleAsync(roleManager, TrainerRoleName);
        }

        private static async Task SeedRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    Log.Error($"{nameof(RolesSeeder)}.{nameof(SeedRoleAsync)}", string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
