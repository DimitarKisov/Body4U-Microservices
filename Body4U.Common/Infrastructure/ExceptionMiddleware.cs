namespace Body4U.Common.Infrastructure
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Server.IIS;
    using Serilog;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// An exception middleware that will convert the 500 status code to 413 when user upload image/s more than the allowed size. Used only when the app is in development environment
    /// </summary>
    public class ExceptionMiddleware : IMiddleware
    {
        private const string MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded with status code {StatusCode}.";

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                HandleExceptionAsync(context, ex);
                Log.Error(ex, MessageTemplate, context.Request.Method, GetPath(context), context.Response.StatusCode);
            }
        }

        private void HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception is BadHttpRequestException badRequestException && badRequestException.Message == "Request body too large.")
            {
                context.Response.StatusCode = (int)HttpStatusCode.RequestEntityTooLarge;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                context.Response.WriteAsync(new
                {
                    context.Response.StatusCode,
                    Message = "Something went wrong"
                }
                .ToString());
            }
        }

        private static string GetPath(HttpContext httpContext)
        {
            return httpContext
                    .Features
                    .Get<IHttpRequestFeature>()?
                    .RawTarget ??
                        httpContext.Request.Path.ToString();
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
