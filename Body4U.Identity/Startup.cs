namespace Body4U.Identity
{
    using Body4U.Common.Infrastructure;
    using Body4U.Common.Services;
    using Body4U.Identity.Data;
    using Body4U.Identity.Data.Seeders;
    using Body4U.Identity.Infrastructure;
    using Body4U.Identity.Services;
    using Body4U.Identity.Services.Favourites;
    using Body4U.Identity.Services.Identity;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
            => services
                .AddUserStorage()
                .AddWebService<IdentityDbContext>(this.Configuration)
                .AddMessaging()
                .AddTransient<ExceptionMiddleware>()
                .AddCloudinary(this.Configuration)
                .AddTransient<IDataSeeder, IdentityDataSeeder>()
                .AddTransient<IIdentityService, IdentityService>()
                .AddTransient<IFavouritesService, FavouritesService>()
                .AddTransient<IJwtTokenGeneratorService, JwtTokenGeneratorService>();

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseWebService(env)
                .Initialize();
    }
}
