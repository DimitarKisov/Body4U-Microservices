namespace Body4U.Identity.Data.Seeders
{
    using System;
    using System.Threading.Tasks;

    internal interface ISeeder
    {
        Task SeedAsync(IdentityDbContext dbContext, IServiceProvider serviceProvider);
    }
}
