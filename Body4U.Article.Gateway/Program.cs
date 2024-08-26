using Body4U.Article.Gateway.Services;
using Body4U.Common.Infrastructure;
using Body4U.Common.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services
    .AddTokenAuthentication(configuration)
    .AddHealth(configuration,
               addDbHealthCheck: false,
               addMessegingHealthCheck: false)
    .AddScoped<ICurrentTokenService, CurrentTokenService>()
    .AddTransient<JwtHeaderAuthenticationMiddleware>()
    .AddTransient<ExceptionMiddleware>()
    .AddSwagger()
    .AddControllers();

services
    .AddRefitClient<IIdentityService>()
    .WithConfiguration(configuration.GetSection("ServiceEndpoints")["Identity"]);

services
    .AddRefitClient<IFavouritesService>()
    .WithConfiguration(configuration.GetSection("ServiceEndpoints")["Identity"]);

services
   .AddRefitClient<IArticleService>()
   .WithConfiguration(configuration.GetSection("ServiceEndpoints")["Article"]);

services
    .AddRefitClient<ICommentService>()
    .WithConfiguration(configuration.GetSection("ServiceEndpoints")["Article"]);

var app = builder.Build();
var env = app.Environment;

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
        .MapHealthChecks()
        .MapControllers());

var envName = env.EnvironmentName;
var environment = envName != null ? $".{envName}" : null;

configuration.AddJsonFile($"appsettings{environment}.json");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Body4U.Article.Gateway...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Body4U.Article.Gateway failed to start!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
