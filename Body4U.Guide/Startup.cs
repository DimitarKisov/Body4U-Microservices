namespace Body4U.Guide
{
    using Body4U.Common.Infrastructure;
    using Body4U.Guide.Data;
    using Body4U.Guide.Services.Exercise;
    using Body4U.Guide.Services.Food;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public Startup(IConfiguration configuration)
            => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
            => services
                .AddWebService<GuideDbContext>(this.Configuration,
                               addMessagingHealthCheck: false)
                .AddTransient<IExerciseService, ExerciseService>()
                .AddTransient<IFoodService, FoodService>();

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseWebService(env,
                               userExceptionMiddleware: false)
                .Initialize();
    }
}
