using Body4U.Common.Infrastructure;
using Body4U.Common.Services;
using Body4U.Identity.Data;
using Body4U.Identity.Data.Seeders;
using Body4U.Identity.Infrastructure;
using Body4U.Identity.Services;
using Body4U.Identity.Services.Favourites;
using Body4U.Identity.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services
    .AddUserStorage()
    .AddWebService<IdentityDbContext>(configuration)
    .AddMessaging(configuration)
    .AddTransient<ExceptionMiddleware>()
    .AddCloudinary(configuration)
    .AddTransient<IDataSeeder, IdentityDataSeeder>()
    .AddTransient<IIdentityService, IdentityService>()
    .AddTransient<IFavouritesService, FavouritesService>()
    .AddTransient<IJwtTokenGeneratorService, JwtTokenGeneratorService>();

var app = builder.Build();
var env = app.Environment;

app
    .UseWebService(env)
    .Initialize();

var envName = env.EnvironmentName;
var environment = envName != null ? $".{envName}" : null;

configuration
    //.AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings{environment}.json", optional: true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Body4U.Identity...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Body4U.Identity failed to start!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
