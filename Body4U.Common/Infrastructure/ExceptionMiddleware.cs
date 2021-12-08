namespace Body4U.Common.Infrastructure
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Server.IIS;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// An exception middleware that will convert the 500 status code to 413 when user upload image/s more than the allowed size. Used only when the app is in development environment
    /// </summary>
    public class ExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                HandleExceptionAsync(context, ex);
            }
        }

        private void HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception is BadHttpRequestException badRequestException && badRequestException.Message == "Request body too large.")
            {
                context.Response.StatusCode = (int)HttpStatusCode.RequestEntityTooLarge;
            }
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(
            this IApplicationBuilder app)
            => app
                .UseMiddleware<ExceptionMiddleware>();
    }
}
