namespace Body4U.Identity.Data.Seeders
{
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class ApplicationDbContextSeeder : ISeeder
    {
        private readonly IConfiguration configuration;

        public ApplicationDbContextSeeder(IConfiguration configuration)
            => this.configuration = configuration;

        public async Task SeedAsync(IdentityDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext == null)
            {
                Log.Information($"{nameof(ApplicationDbContextSeeder)}.{nameof(SeedAsync)} failed because of null dbContext");
                return;
            }

            if (serviceProvider == null)
            {
                Log.Information($"{nameof(ApplicationDbContextSeeder)}.{nameof(SeedAsync)} failed because of null serviceProvider");
                return;
            }

            var seeders = new List<ISeeder>
                {
                    new ApplicationUserSeeder(configuration),
                    new RolesSeeder(),
                    new RoleToApplicationUserSeeder(configuration)
                };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(dbContext, serviceProvider);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
