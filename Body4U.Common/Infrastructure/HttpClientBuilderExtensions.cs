namespace Body4U.Common.Infrastructure
{
    using Body4U.Common.Services.Identity;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Polly;
    using System;
    using System.Net;
    using System.Net.Http.Headers;

    public static class HttpClientBuilderExtensions
    {
        public static void WithConfiguration(this IHttpClientBuilder httpClientBuilder, string baseAddress)
        {
            httpClientBuilder
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    client.BaseAddress = new Uri(baseAddress);

                    var requestServices = serviceProvider
                        .GetService<IHttpContextAccessor>()
                        ?.HttpContext
                        .RequestServices;

                    var currentToken = requestServices
                        ?.GetService<ICurrentTokenService>()
                        ?.Get();

                    if (currentToken == null)
                    {
                        return;
                    }

                    var authorizationHeader = new AuthenticationHeaderValue("Bearer", currentToken);
                    client.DefaultRequestHeaders.Authorization = authorizationHeader;
                })
                .AddTransientHttpErrorPolicy(policy => policy //Add Http policy for errors
                    .OrResult(result => result.StatusCode == HttpStatusCode.NotFound) //At result NotFound or anything else
                    .WaitAndRetryAsync(5, retry => //Wait and try again 5 times
                        TimeSpan.FromSeconds(Math.Pow(2, retry)))) //The first will be 2 sec, second will be 2^2, third 4^2, forth will be 16^2 and the last 256 ^ 2
                .AddTransientHttpErrorPolicy(policy => policy //Add Http policy for errors
                    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))); //Try max 5 times before you stop trying for 30 seconds
        }
    }
}
