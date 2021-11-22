using Microsoft.AspNetCore.Builder;

namespace Body4U.Identity.Infrastructure
{
    public static class ConfigureExtensions
    {
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
    }
}
