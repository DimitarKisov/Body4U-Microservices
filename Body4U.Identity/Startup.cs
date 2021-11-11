namespace Body4U.Identity
{
    using Body4U.Common.Infrastructure;
    using Body4U.Identity.Data;
    using Body4U.Identity.Data.Seeders;
    using Body4U.Identity.Infrastructure;
    using Body4U.Identity.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.Linq;

    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddWebService<IdentityDbContext>(this.Configuration)
                .AddUserStorage()
                .AddTransient<IIdentityService, IdentityService>()
                .AddTransient<IJwtTokenGeneratorService, JwtTokenGeneratorService>()
                .AddTransient<IEmailService, EmailService>();

            this.SeedIdentityData(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseWebService(env);

        private void SeedIdentityData(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            using (var dbContext = (IdentityDbContext)serviceProvider.GetService(typeof(IdentityDbContext)))
            {
                if (!(dbContext.GetService<IDatabaseCreator>() as RelationalDatabaseCreator)!.Exists())
                {
                    dbContext.Database.Migrate();
                }

                if (!dbContext.Users.Any())
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            new ApplicationDbContextSeeder(this.Configuration).SeedAsync(dbContext, serviceProvider).GetAwaiter().GetResult();

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Identity/{nameof(Startup)}.{nameof(SeedIdentityData)}", ex);
                        }
                    }
                }
            }
        }
    }
}
