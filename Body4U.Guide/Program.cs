using Body4U.Common.Infrastructure;
using Body4U.Guide.Data;
using Body4U.Guide.Services.Exercise;
using Body4U.Guide.Services.Food;
using Body4U.Guide.Services.Supplement;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services
    .AddWebService<GuideDbContext>(configuration,
                   addMessagingHealthCheck: false)
    .AddTransient<IExerciseService, ExerciseService>()
    .AddTransient<IFoodService, FoodService>()
    .AddTransient<ISupplementService, SupplementService>();

var app = builder.Build();
var env = app.Environment;

app
    .UseWebService(env,
                   userExceptionMiddleware: false)
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
    Log.Information("Starting Body4U.Guide...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Body4U.Guide failed to start!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
