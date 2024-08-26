using Body4U.Common.Infrastructure;
using Body4U.EmailSender.Data;
using Body4U.EmailSender.Messages;
using Body4U.EmailSender.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services
    .AddDatabase<EmailSenderDbContext>(configuration)
    .AddHealth(configuration)
    .AddTransient<IEmailService, EmailService>()
    .AddMessaging(configuration,
                  false,
                  typeof(SendEmailConsumer));

var app = builder.Build();
var env = app.Environment;

if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app
    .UseRouting()
    .UseEndpoints(endpoints => endpoints
        .MapHealthChecks())
    .Initialize();

var envName = env.EnvironmentName;
var environment = envName != null ? $".{envName}" : null;

configuration.AddJsonFile($"appsettings{environment}.json");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Body4U.EmailSender...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Body4U.EmailSender failed to start!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}