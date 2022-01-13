namespace Body4U.EmailSender
{
    using Body4U.Common.Infrastructure;
    using Body4U.EmailSender.Data;
    using Body4U.EmailSender.Messages;
    using Body4U.EmailSender.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
            => services
                .AddDatabase<EmailSenderDbContext>(this.Configuration)
                .AddHealth(this.Configuration)
                .AddTransient<IEmailService, EmailService>()
                .AddMessaging(this.Configuration,
                              false,
                              typeof(SendEmailConsumer));

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseRouting()
                .UseEndpoints(endpoints => endpoints
                    .MapHealthChecks())
                .Initialize();
        }
    }
}
