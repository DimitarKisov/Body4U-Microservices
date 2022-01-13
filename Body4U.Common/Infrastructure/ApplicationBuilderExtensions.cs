﻿namespace Body4U.Common.Infrastructure
{
    using Body4U.Common.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using System.IO;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseWebService(
            this IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage()
                    .UseExceptionMiddleware() // TODO: След време ако не е нужно от други апита, да го премахна и сложа само в Startup-ите на тези, които ще го ползват, тъй като не всички ще имат снимки
                    .UseSwagger();
            }

            app
                //.UseHttpsRedirection()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints => endpoints
                    .MapHealthChecks()
                    .MapControllers());

            return app;
        }

        public static IApplicationBuilder Initialize(
            this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;

                var db = serviceProvider.GetRequiredService<DbContext>();

                db.Database.Migrate();

                var seeders = serviceProvider.GetServices<IDataSeeder>();

                foreach (var seeder in seeders)
                {
                    seeder.SeedData();
                }

                return app;
            }
        }

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });

            return app;
        }

        public static IApplicationBuilder UseCustomStaticFiles(this IApplicationBuilder app)
            => app
                .UseFileServer(new FileServerOptions
                {
                    FileProvider = new PhysicalFileProvider(
                            Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
                    RequestPath = "/StaticFiles",
                    EnableDefaultFiles = true
                });
    }
}
