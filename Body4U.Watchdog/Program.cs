using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services
    .AddHealthChecksUI()
    .AddInMemoryStorage();

var app = builder.Build();
var env = app.Environment;

app
    .UseRouting()
    .UseEndpoints(endpoints =>
        endpoints.MapHealthChecksUI(health => health
            .UIPath = "/health"));

configuration.AddJsonFile($"appsettings.json");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Body4U.Watchdog...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Body4U.Watchdog failed to start!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}