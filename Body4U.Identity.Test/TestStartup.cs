namespace Body4U.Identity.Test
{
    using Body4U.Common.Services.Identity;
    using Body4U.Identity.Data.Models;
    using Body4U.Identity.Services;
    using Body4U.Identity.Test.Mocks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using MyTested.AspNetCore.Mvc;

    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void ConfigureTestServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.ReplaceTransient<ICurrentUserService>(_ => CurrentUserServiceMock.Instance);
            services.ReplaceTransient<UserManager<ApplicationUser>>(_ => UserManagerMock.Instance);
            services.ReplaceTransient<IJwtTokenGeneratorService>(_ => JwtTokenGeneratorServiceMock.Instance);
        }
    }
}
