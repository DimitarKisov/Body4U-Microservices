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

    internal class ApplicationUserSeeder : ISeeder
    {
        private readonly IConfiguration configuration;

        public ApplicationUserSeeder(IConfiguration configuration)
            => this.configuration = configuration;

        public async Task SeedAsync(IdentityDbContext dbContext, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var email = configuration.GetSection("SeedInfo")["Email"];
            var phoneNumber = configuration.GetSection("SeedInfo")["PhoneNumber"];
            var firstName = configuration.GetSection("SeedInfo")["FirstName"];
            var lastName = configuration.GetSection("SeedInfo")["LastName"];
            var age = int.Parse(configuration.GetSection("SeedInfo")["Age"]);
            var passsword = configuration.GetSection("SeedInfo")["Password"];

            await SeedUserAsync(userManager, email, phoneNumber, firstName, lastName, age, passsword);
        }

        private static async Task SeedUserAsync(UserManager<ApplicationUser> userManager, string email, string phoneNumber, string firstName, string lastName, int age, string password)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var newUser = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    PhoneNumber = phoneNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    Age = age,
                    ProfilePicture = null,
                    Gender = Gender.Male,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, password);

                if (!result.Succeeded)
                {
                    Log.Error($"{nameof(ApplicationUserSeeder)}.{nameof(SeedUserAsync)}", string.Join(Environment.NewLine, result.Errors));
                }
            }
        }
    }
}
