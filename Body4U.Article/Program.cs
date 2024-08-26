using Body4U.Article.Data;
using Body4U.Article.Messages;
using Body4U.Article.Services.Article;
using Body4U.Article.Services.Comment;
using Body4U.Article.Services.Service;
using Body4U.Article.Services.Trainer;
using Body4U.Common.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services
    .AddWebService<ArticleDbContext>(configuration)
    .AddTransient<ExceptionMiddleware>()
    .AddCloudinary(configuration)
    .AddTransient<ITrainerService, TrainerService>()
    .AddTransient<IArticleService, ArticleService>()
    .AddTransient<ICommentService, CommentService>()
    .AddTransient<IServiceService, ServiceService>()
    .AddMessaging(configuration,
        useHangFire: false,
        typeof(CreateTrainerConsumer),
        typeof(DeleteTrainerConsumer),
        typeof(EdiTrainerNamesConsumer));

var app = builder.Build();
var env = app.Environment;

app
    .UseWebService(env)
    .Initialize();

var envName = env.EnvironmentName;
var environment = envName != null ? $".{envName}" : null;

configuration.AddJsonFile($"appsettings{environment}.json");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Body4U.Article...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Body4U.Article failed to start!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}