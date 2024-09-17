namespace Body4U.Common.Infrastructure
{
    using Body4U.Common.Messages;
    using Body4U.Common.Services.Cloud;
    using Body4U.Common.Services.Identity;
    using CloudinaryDotNet;
    using MassTransit;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebService<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool addDbHealthCheck = true,
            bool addMessagingHealthCheck = true)
            where TDbContext : DbContext
        {
            services
                .AddDatabase<TDbContext>(configuration)
                .AddTokenAuthentication(configuration)
                .AddSwagger()
                .AddHealth(configuration, addDbHealthCheck, addMessagingHealthCheck)
                .AddControllers();

            return services;
        }

        public static IServiceCollection AddDatabase<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TDbContext : DbContext
        {
            services
                .AddScoped<DbContext, TDbContext>()
                .AddDbContext<TDbContext>(options => options
                    .UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions => sqlOptions
                        .EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null)));

            return services;
        }

        public static IServiceCollection AddTokenAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var secret = configuration.GetSection("JwtSettings")["Secret"];

            var key = Encoding.ASCII.GetBytes(secret);

            services
                .AddAuthentication(authentication =>
                {
                    authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(bearer =>
                {
                    bearer.RequireHttpsMetadata = false;
                    bearer.SaveToken = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }

        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration,
            bool useBackgroundWorker = true,
            params Type[] consumers)
        {
            services.AddMassTransit(mt =>
            {
                consumers.ForEach(consumer => mt.AddConsumer(consumer));

                mt.UsingRabbitMq((bus, rmq) =>
                {
                    rmq.ConfigureEndpoints(bus);
                    rmq.Host(configuration.GetSection("RabbitMQ")["Host"], host =>
                    {
                        host.Username(configuration.GetSection("RabbitMQ")["Username"]);
                        host.Password(configuration.GetSection("RabbitMQ")["Password"]);
                    });

                    consumers.ForEach(consumer => rmq.ReceiveEndpoint(consumer.FullName, endpoint =>
                    {
                        endpoint.PrefetchCount = 6;
                        endpoint.UseMessageRetry(retry => retry.Interval(10, 1000)); // try 10 times every 1 second to send the messages
                        endpoint.ConfigureConsumer(bus, consumer);
                    }));
                });
            });

            if (useBackgroundWorker)
            {
                services.AddHostedService<MessagesHostedService>();
            }

            return services;
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Body4U API",
                    Description = "A simple example ASP.NET Core Web API",
                    Contact = new OpenApiContact
                    {
                        Name = "Body4U Admin",
                        Email = "somemail@mail.com"
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddCloudinary(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var cloudinarySection = configuration.GetSection("Cloudinary");
            var cloudName = cloudinarySection["CloudName"];
            var apiKey = cloudinarySection["ApiKey"];
            var apiSecret = cloudinarySection["ApiSecret"];

            Account account = new Account
                (
                    cloudName,
                    apiKey,
                    apiSecret
                );

            Cloudinary cloudinary = new Cloudinary(account);

            services.AddSingleton(cloudinary);
            services.AddTransient<ICloudinaryService, CloudinaryService>();

            return services;
        }

        public static IServiceCollection AddHealth(
            this IServiceCollection services,
            IConfiguration configuration,
            bool addDbHealthCheck = true,
            bool addMessegingHealthCheck = true)
        {
            var healthChecks = services.AddHealthChecks();

            if (addDbHealthCheck)
            {
                healthChecks
                .AddSqlServer(configuration.GetConnectionString("DefaultConnection"));
            }

            if (addMessegingHealthCheck)
            {
                var rabbitMqHost = configuration.GetSection("RabbitMQ")["Host"];
                var rabbitMqUser = configuration.GetSection("RabbitMQ")["Username"];
                var rabbitMqPassword = configuration.GetSection("RabbitMQ")["Password"];

                healthChecks
                    .AddRabbitMQ(rabbitConnectionString: $"amqp://{rabbitMqUser}:{rabbitMqPassword}@{rabbitMqHost}/");
            }
            
            return services;
        }
    }
}
