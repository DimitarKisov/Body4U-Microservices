namespace Body4U.Identity.Data.Seeders
{
    using Body4U.Common.Messages;
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

    //ApplicationUser is passed because the service needs it but we are going to use the userManager for creating a user
    public class IdentityDataSeeder : DataService<ApplicationUser>, IDataSeeder
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
            : base(dbContext)
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

                    var messageData = new CreateTrainerMessage()
                    {
                        ApplicationUserId = user.Id,
                        CreatedOn = DateTime.Now,
                        FirstName = user.FirstName,
                        Lastname = user.LastName
                    };

                    var message = new Message(messageData);

                    await userManager.CreateAsync(user, passsword);
                    await userManager.AddToRoleAsync(user, AdministratorRoleName);
                    await userManager.AddToRoleAsync(user, TrainerRoleName);

                    var saveInDbSuccess = await this.Save(null, message);

                    await this.publisher.Publish(messageData);

                    if (saveInDbSuccess)
                    {
                        message.MarkAsPublished();
                    }
                    
                    await this.dbContext.SaveChangesAsync();
                })
                .GetAwaiter()
                .GetResult();
            }
        }
    }
}
