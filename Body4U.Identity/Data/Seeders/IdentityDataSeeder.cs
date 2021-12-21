namespace Body4U.Identity.Data.Seeders
{
    using Body4U.Common.Messages.Article;
    using Body4U.Common.Services;
    using Body4U.Identity.Data.Models.Identity;
    using MassTransit;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using static Body4U.Common.Constants.DataConstants.Common;

    public class IdentityDataSeeder : IDataSeeder
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IdentityDbContext dbContext;
        private readonly IBus publisher;

        public IdentityDataSeeder(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IdentityDbContext dbContext,
            IBus publisher)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.dbContext = dbContext;
            this.publisher = publisher;
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
                    var trainerRole = new IdentityRole(TrainerRoleName);

                    await this.roleManager.CreateAsync(adminRole);
                    await this.roleManager.CreateAsync(trainerRole);

                    var user = new ApplicationUser
                    {
                        Email = email,
                        UserName = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Gender = Gender.Male,
                        EmailConfirmed = true,
                        CreateOn = DateTime.Now
                    };

                    await userManager.CreateAsync(user, passsword);

                    await userManager.AddToRoleAsync(user, AdministratorRoleName);
                    await userManager.AddToRoleAsync(user, TrainerRoleName);

                    await this.publisher.Publish(new CreateTrainerMessage()
                    {
                        ApplicationUserId = user.Id,
                        CreatedOn = DateTime.Now,
                        FirstName = user.FirstName,
                        Lastname = user.LastName
                    });
                })
                .GetAwaiter()
                .GetResult();
            }
        }
    }
}
