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
                var email = "body4u2020@gmail.com";
                var firstName = "Body4U";
                var lastName = "Admin";

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

                    await userManager.CreateAsync(user, "123456");

                    await userManager.AddToRoleAsync(user, AdministratorRoleName);
                })
                .GetAwaiter()
                .GetResult();
            }
        }
    }
}
