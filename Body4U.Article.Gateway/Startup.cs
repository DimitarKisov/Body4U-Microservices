namespace Body4U.Article.Gateway
{
    using Body4U.Article.Gateway.Services;
    using Body4U.Common.Infrastructure;
    using Body4U.Common.Services.Identity;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Refit;

    public class Startup
    {
        public Startup(IConfiguration configuration)
            => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddTokenAuthentication(this.Configuration)
                .AddScoped<ICurrentTokenService, CurrentTokenService>()
                .AddTransient<JwtHeaderAuthenticationMiddleware>()
                .AddTransient<ExceptionMiddleware>()
                .AddSwagger()
                .AddControllers();

            services
               .AddRefitClient<IIdentityService>()
               .WithConfiguration(this.Configuration.GetSection("ServiceEndpoints")["Identity"]);

            services
                .AddRefitClient<IFavouritesService>()
                .WithConfiguration(this.Configuration.GetSection("ServiceEndpoints")["Identity"]);

            services
               .AddRefitClient<IArticleService>()
               .WithConfiguration(this.Configuration.GetSection("ServiceEndpoints")["Article"]);

            services
                .AddRefitClient<ICommentService>()
                .WithConfiguration(this.Configuration.GetSection("ServiceEndpoints")["Article"]);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app
                    .UseDeveloperExceptionPage()
                    .UseSwagger();
            }

            app
                .UseHttpsRedirection()
                .UseRouting()
                .UseJwtHeaderAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints => endpoints
                    .MapControllers());
        }
    }
}
