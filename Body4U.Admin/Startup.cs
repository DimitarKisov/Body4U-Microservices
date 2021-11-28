namespace Body4U.Admin
{
    using Body4U.Admin.Services.Identity;
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
                .AddSwagger()
                .AddControllers();

            services
                .AddRefitClient<IIdentityService>()
                .WithConfiguration(this.Configuration.GetSection("ServiceEndpoints")["Identity"]);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseHttpsRedirection()
                .UseRouting()
                .UseJwtHeaderAuthentication()
                .UseAuthorization()
                .UseSwagger()
                .UseEndpoints(endpoints => endpoints
                    .MapControllers());
        }
    }
}
