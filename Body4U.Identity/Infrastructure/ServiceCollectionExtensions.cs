﻿namespace Body4U.Identity.Infrastructure
{
    using Body4U.Identity.Data;
    using Body4U.Identity.Data.Models.Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserStorage(
            this IServiceCollection services)
        {
            services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
