namespace Body4U.Admin
{
    using Body4U.Admin.Services.Identity;
    using Body4U.Common.Infrastructure;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Refit;
    using System;

    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddTokenAuthentication(this.Configuration)
                .AddSwagger()
                .AddControllers();

            services
                .AddRefitClient<IIdentityService>()
                .ConfigureHttpClient(c => c
                    .BaseAddress = new Uri(this.Configuration.GetSection("ServiceEndpoints")["Identity"]));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseSwagger()
                .UseEndpoints(endpoints => endpoints
                    .MapControllers());
        }
    }
}
