namespace Body4U.Identity.Data.Seeders
{
    using Body4U.Common.Services;
    using Body4U.Identity.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Common;

    public class IdentityDataSeeder : IDataSeeder
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public IdentityDataSeeder(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public void SeedData()
        {
            if (!userManager.Users.Any() && !roleManager.Roles.Any())
            {
                var email = configuration.GetSection("SeedInfo")["Email"];
                var firstName = configuration.GetSection("SeedInfo")["FirstName"];
                var lastName = configuration.GetSection("SeedInfo")["LastName"];
                var passsword = configuration.GetSection("SeedInfo")["Password"];

                Task.Run(async () =>
                {
                    var adminRole = new IdentityRole(AdministratorRoleName);

                    await this.roleManager.CreateAsync(adminRole);

                    var user = new ApplicationUser
                    {
                        Email = email,
                        UserName = email,
                        FirstName = firstName,
                        LastName = lastName,
                        ProfilePicture = null,
                        Gender = Gender.Male,
                        EmailConfirmed = true
                    };

                    await userManager.CreateAsync(user, passsword);

                    await userManager.AddToRoleAsync(user, AdministratorRoleName);
                })
                .GetAwaiter()
                .GetResult();
            }
        }
    }
}
